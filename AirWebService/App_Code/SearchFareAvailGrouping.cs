using System;
using System.Threading;
using System.Web;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// ThreadPool을 이용한 멀티 GDS(Amadeus/Sabre) 동시조회
	/// </summary>
	public class SearchFareAvailGrouping
	{
		Common cm = new Common();
		private int ThreadCount = 0;
        private int MPISKey = 999;
        private int MPKey = 999;
        private int SBKey = 999;
		XmlElement[] XmlFareAvail;

        public XmlElement[] GetFareAvail(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string[] PTC, int[] NOP, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID)
		{
			try
			{
                //MPIS 사용 조건 체크
                bool MPIS = false;

                //모두닷컴만 가능
                if (SNM.Equals(2) || SNM.Equals(3915))
                {
                    //다음 조건은 없어야 함
                    if (String.IsNullOrWhiteSpace(String.Concat(SAC, FLD, ACQ)))
                    {
                        //M클래스만 가능
                        if (CCD.Equals("M"))
                        {
                            //성인요금으로 성인1명 또는 성인2명 조회시만 가능
                            if (PTC[0].Equals("ADT") && Convert.ToInt32(NOP[0]) < 3)
                            {
                                //편도/왕복만 가능
                                if (ROT.Equals("OW") || ROT.Equals("RT"))
                                {
                                    //미오픈만 가능
                                    if (OPN.Equals("N"))
                                    {
                                        //출발지는 서울(SEL)에 한해서만 가능
                                        if (DLC.Equals("SEL"))
                                        {
                                            //도착지가 다음 지역에 한해서만 가능
                                            if ("/AKL/ALA/AMS/ATH/ATL/BCN/BER/BJS/BKI/BKK/BNE/BUD/CAN/CEB/CGQ/CHI/CNX/CPH/CRK/CTU/DAD/DEL/DLC/DPS/DXB/FRA/FUK/GUM/GVA/HAN/HEL/HGH/HKG/HKT/HND/HNL/HPH/HRB/HSG/IST/JKT/KHV/KLO/KMG/KMJ/KTM/KUL/LAS/LAX/LON/MAD/MDG/MEL/MFM/MIL/MNL/MOW/MUC/NGO/NHA/NKG/NRT/NYC/OIT/OKA/OSA/PAR/PNH/PRG/RGN/ROM/ROR/SEA/SFO/SGN/SHA/SHE/SIA/SIN/SPK/SPN/SYD/SZX/TAO/TPE/TSN/ULN/VIE/VTE/VVO/WAS/WAW/WEH/XMN/YNJ/YNT/YTO/YVR/ZAG/ZRH/".IndexOf(String.Format("/{0}/", ALC)) != -1)
                                            {
                                                DateTime NowDate = DateTime.Now;
                                                int NowHour = NowDate.Hour;

                                                //업무일(01~19시), 비업무일(01시~15시)까지만 가능
                                                if (cm.WorkdayYN(NowDate.ToString("yyyy-MM-dd")))
                                                {
                                                    if (NowHour >= 1 && NowHour < 19)
                                                        MPIS = true;
                                                }
                                                else
                                                {
                                                    if (NowHour >= 1 && NowHour < 15)
                                                        MPIS = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                //MPIS 사용시 클래스는 무조건 하나의 클래스만 조회 가능
                //string GDSString = "Sabre";
                //string GDSString = (MPIS) ? "MPIS,Amadeus" : "Amadeus";
                string GDSString = String.Concat(((MPIS) ? "MPIS," : ""), PTC[0].Equals("ADT") ? "Amadeus,Sabre" : "Amadeus");
                string CabinString = (String.IsNullOrWhiteSpace(CCD) || CCD.Equals("Y")) ? "M" : CCD.Substring(0, 1);
                
                string[] GDS = GDSString.Split(',');
                string[] Cabin = CabinString.Split(',');
				int TableCount = (GDS.Length * Cabin.Length);

				ManualResetEvent[] doneEvents = new ManualResetEvent[TableCount];
				AirService2 airSvc = new AirService2();
                int i = 0;
                
                XmlFareAvail = new XmlElement[TableCount];
			
				//멀티쓰레드로 호출할 함수 설정 및 전달할 파라미터 설정
                for (int n = 0; n < GDS.Length; n++)
				{
                    for (int m = 0; m < Cabin.Length; m++)
                    {
                        doneEvents[i] = new ManualResetEvent(false);
                        XmlFareAvail[i] = null;

                        object[] objState = new object[] { i, doneEvents[i], HttpContext.Current, airSvc, GDS[n].Trim(), SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, Cabin[m].Trim(), ACQ, PTC, NOP, NRR, PUB, WLR, LTD, FTR, MTL, GUID };
                        ThreadPool.QueueUserWorkItem(MonitoringCallBack, objState);
                        i++;
                    }
				}

				//호출완료 또는 시간초과 체크 후 종료처리
				int Lop = 0;

				while (true)
				{
					Thread.Sleep(500);
					Lop++;

                    //MP 응답이 있을 경우
                    if (MPKey != 999)
                    {
                        if (XmlFareAvail[MPKey] != null && XmlFareAvail[MPKey].SelectNodes("flightInfo").Count > 0 && XmlFareAvail[MPKey].SelectNodes("priceInfo").Count > 0)
                        {
                            if (XmlFareAvail[MPKey].SelectNodes("flightInfo/flightIndex").Count > 0 && XmlFareAvail[MPKey].SelectNodes("priceInfo/priceIndex").Count > 0)
                            {
                                //if (SBKey.Equals(999))
                                //    break;
                                //else
                                //{
                                //    if (XmlFareAvail[SBKey] != null && XmlFareAvail[SBKey].SelectNodes("flightInfo").Count > 0 && XmlFareAvail[SBKey].SelectNodes("priceInfo").Count > 0)
                                //    {
                                //        if (XmlFareAvail[SBKey].SelectNodes("flightInfo/flightIndex").Count > 0 && XmlFareAvail[SBKey].SelectNodes("priceInfo/priceIndex").Count > 0)
                                //        {
                                //            break;
                                //        }
                                //    }
                                //}
                                break;
                            }
                        }
                    }
                    
                    //MPIS 응답이 있을 경우
                    if (MPISKey != 999)
                    {
                        if (XmlFareAvail[MPISKey] != null && XmlFareAvail[MPISKey].SelectNodes("flightInfo").Count > 0 && XmlFareAvail[MPISKey].SelectNodes("priceInfo").Count > 0)
                        {
                            if (XmlFareAvail[MPISKey].SelectNodes("flightInfo/flightIndex").Count > 0 && XmlFareAvail[MPISKey].SelectNodes("priceInfo/priceIndex").Count > 0)
                            {
                                //if (SBKey.Equals(999))
                                //    break;
                                //else
                                //{
                                //    if (XmlFareAvail[SBKey] != null && XmlFareAvail[SBKey].SelectNodes("flightInfo").Count > 0 && XmlFareAvail[SBKey].SelectNodes("priceInfo").Count > 0)
                                //    {
                                //        if (XmlFareAvail[SBKey].SelectNodes("flightInfo/flightIndex").Count > 0 && XmlFareAvail[SBKey].SelectNodes("priceInfo/priceIndex").Count > 0)
                                //        {
                                //            break;
                                //        }
                                //    }
                                //}
                                break;
                            }
                        }
                    }

                    if (ThreadCount >= TableCount || Lop > 60)
                        break;
				}
			}
			catch (Exception ex)
			{
                XmlFareAvail[0] = new MWSException(ex, HttpContext.Current, "Mode", "SearchFareAvailGrouping", 0, 0).ToErrors;
			}

			return XmlFareAvail;
		}

		public void MonitoringCallBack(Object context)
		{
			int index = (int)((object[])context)[0];
			ManualResetEvent doneEvent = (ManualResetEvent)((object[])context)[1];
			HttpContext hcc = (HttpContext)((object[])context)[2];
			
			try
			{
				AirService2 airSvc = (AirService2)((object[])context)[3];

                if (((object[])context)[4].ToString().Equals("MPIS"))
                {
                    MPISKey = index;
                    XmlFareAvail[index] = airSvc.InstantSearchAmadeusRS(3915, (string)((object[])context)[6], (string)((object[])context)[7], (string)((object[])context)[8], "", (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], (string)((object[])context)[15], "", (string[])((object[])context)[16], (int[])((object[])context)[17], (int)((object[])context)[18], (string)((object[])context)[19], (int)((object[])context)[20], (string)((object[])context)[21], (string)((object[])context)[22], (string)((object[])context)[23], (string)((object[])context)[24]);
                }
                else if (((object[])context)[4].ToString().Equals("Sabre"))
                {
                    SBKey = index;
                    //XmlFareAvail[index] = airSvc.SearchFareAvailSabreRS((int)((object[])context)[5], (string)((object[])context)[6], (string)((object[])context)[7], (string)((object[])context)[8], "", (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], (string)((object[])context)[15], "", (string[])((object[])context)[16], (int[])((object[])context)[17], (int)((object[])context)[18], (string)((object[])context)[19], (int)((object[])context)[20], (string)((object[])context)[21], (string)((object[])context)[22], (string)((object[])context)[23], (string)((object[])context)[24]);
                    XmlFareAvail[index] = airSvc.SearchFareAvailSabreRS((int)((object[])context)[5], (String.IsNullOrWhiteSpace(((object[])context)[6].ToString()) ? "RS/BX" : (string)((object[])context)[6]), (string)((object[])context)[7], (string)((object[])context)[8], "", (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], (string)((object[])context)[15], "", (string[])((object[])context)[16], (int[])((object[])context)[17], (int)((object[])context)[18], (string)((object[])context)[19], (int)((object[])context)[20], (string)((object[])context)[21], (string)((object[])context)[22], (string)((object[])context)[23], (string)((object[])context)[24]);
                }
                else
                {
                    MPKey = index;
                    XmlFareAvail[index] = airSvc.SearchFareAvailAmadeusRS((int)((object[])context)[5], (string)((object[])context)[6], (string)((object[])context)[7], (string)((object[])context)[8], "", (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], (string)((object[])context)[15], "", (string[])((object[])context)[16], (int[])((object[])context)[17], (int)((object[])context)[18], (string)((object[])context)[19], (int)((object[])context)[20], (string)((object[])context)[21], (string)((object[])context)[22], (string)((object[])context)[23], (string)((object[])context)[24]);
                }
			}
			catch (Exception ex)
			{
                XmlFareAvail[index] = new MWSException(ex, hcc, "Mode", "SearchFareAvailGrouping", 0, 0).ToErrors;
			}
			finally
			{
				ThreadCount++;
				doneEvent.Set();
			}
		}
	}
}