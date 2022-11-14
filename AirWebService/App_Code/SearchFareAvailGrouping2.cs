using System;
using System.Threading;
using System.Web;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// ThreadPool을 이용한 멀티 GDS(Amadeus/Sabre) 멀티 검색
	/// </summary>
	public class SearchFareAvailGrouping2
	{
		Common cm = new Common();
		private int ThreadCount = 0;
		XmlElement[] XmlFareAvail;

        public XmlElement[] GetFareAvail(int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string PRM, string AAC, string GUID)
		{
			try
			{
                string GDSString = "Amadeus";
                string CabinString = (String.IsNullOrWhiteSpace(CCD) || CCD.Equals("Y")) ? "M,W" : CCD;
                //string PTCString = String.IsNullOrWhiteSpace(PTC) ? ((CHC.Equals(0) && IFC.Equals(0)) ? "ADT,STU" : "ADT") : PTC;
                string PTCString = String.IsNullOrWhiteSpace(PTC) ? "ADT" : PTC; //성인요금만 조회(2019-06-05,김지영팀장)
                string AddAirString = "";
                string ExcAirString = XAC;
                bool AddAirStringYN = false;
                int NRRLimit = (SNM.Equals(2) || SNM.Equals(3915)) ? 200 : (NRR.Equals(0) ? 50 : NRR); //닷컴(WEB/MOBILE)결과수 200으로 지정(2019-06-05,김지영팀장)

                //미주 지역에 한해서 AA/DL/UA/AC 항공사 별도 검색
                if (AAC.Equals("Y") && (ROT.Equals("OW") || ROT.Equals("RT")) && String.IsNullOrWhiteSpace(SAC))
                {
                    if (Common.UnitedStatesOfAirport(ALC.Split(',')[0].Trim()))
                    {
                        AddAirString = "AA,DL,UA,AC";
                        AddAirStringYN = true;
                        ExcAirString = String.Concat(AddAirString, (String.IsNullOrWhiteSpace(XAC) ? "" : ","), XAC);
                        NRRLimit = SNM.Equals(2) ? 100 : 50;
                    }
                }

                string[] GDS = GDSString.Split(',');
                string[] Cabin = CabinString.Split(',');
                string[] PaxType = PTCString.Split(',');
                string[] AddAir = AddAirString.Split(',');
                int TableCount = AddAirStringYN ? ((GDS.Length * Cabin.Length * PaxType.Length * AddAir.Length) + (GDS.Length * Cabin.Length * PaxType.Length)) : (GDS.Length * Cabin.Length * PaxType.Length);

				ManualResetEvent[] doneEvents = new ManualResetEvent[TableCount];
                AirService3 airSvc = new AirService3();
                int i = 0;
                
                XmlFareAvail = new XmlElement[TableCount];
			
				//멀티쓰레드로 호출할 함수 설정 및 전달할 파라미터 설정
                for (int p = 0; p < PaxType.Length; p++)
                {
                    XmlElement PromXml = null;

                    if (PRM.Equals("Y"))
                    {
                        //프로모션 정보(운임타입별로 프로모션 생성)
                        string StepCCD = (CCD.Equals("C") || CCD.Equals("F")) ? CCD : "Y";
                        PromXml = AirService3.SearchPromotionList(SNM, DLC, ALC, ROT, DTD, ARD, OPN, StepCCD, PaxType[p]);
                        cm.XmlFileSave(PromXml, "Mode", "SearchPromotionList", "N", String.Format("{0}-{1}-{2}", GUID, PaxType[p], StepCCD));
                    }
                    
                    for (int n = 0; n < GDS.Length; n++)
                    {
                        for (int m = 0; m < Cabin.Length; m++)
                        {
                            doneEvents[i] = new ManualResetEvent(false);
                            XmlFareAvail[i] = null;

                            object[] objState = new object[] { i, doneEvents[i], HttpContext.Current, airSvc, GDS[n].Trim(), SNM, SAC, ExcAirString, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, Cabin[m], ACQ, FAB, PaxType[p], ADC, CHC, IFC, NRRLimit, PUB, WLR, LTD, FTR, "", GUID, PromXml };
                            ThreadPool.QueueUserWorkItem(MonitoringCallBack, objState);
                            i++;

                            if (AddAirStringYN && GDS[n].Trim().Equals("Amadeus"))
                            {
                                for (int a = 0; a < AddAir.Length; a++)
                                {
                                    doneEvents[i] = new ManualResetEvent(false);
                                    XmlFareAvail[i] = null;

                                    object[] objStates = new object[] { i, doneEvents[i], HttpContext.Current, airSvc, "Amadeus", SNM, AddAir[a].Trim(), "", DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, Cabin[m], ACQ, FAB, PaxType[p], ADC, CHC, IFC, 30, PUB, WLR, LTD, FTR, "", GUID, PromXml };
                                    ThreadPool.QueueUserWorkItem(MonitoringCallBack, objStates);
                                    i++;
                                }
                            }
                        }
                    }
                }

				//호출완료 또는 시간초과 체크 후 종료처리
				int Lop = 0;

				while (true)
				{
					Thread.Sleep(500);
					Lop++;

                    if (ThreadCount >= TableCount || Lop > 30)
                        break;
				}
			}
			catch (Exception ex)
			{
                new MWSException(ex, HttpContext.Current, "Mode", "SearchFareAvailGrouping2", 0, 0);
                XmlFareAvail[0] = null;
			}

			return XmlFareAvail;
		}

		public void MonitoringCallBack(Object context)
		{
			ManualResetEvent doneEvent = (ManualResetEvent)((object[])context)[1];
            //HttpContext hcc = (HttpContext)((object[])context)[2];
            int index = (int)((object[])context)[0];
			
			try
			{
				AirService3 airSvc = (AirService3)((object[])context)[3];

                if (((object[])context)[4].ToString().Equals("Sabre"))
                {
                    //XmlFareAvail[index] = airSvc.SearchFareAvailSabreRS((int)((object[])context)[5], (String.IsNullOrWhiteSpace(((object[])context)[6].ToString()) ? "RS/BX" : (string)((object[])context)[6]), (string)((object[])context)[7], (string)((object[])context)[8], "", (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], (string)((object[])context)[15], "", (string[])((object[])context)[16], (int[])((object[])context)[17], (int)((object[])context)[18], (string)((object[])context)[19], (int)((object[])context)[20], (string)((object[])context)[21], (string)((object[])context)[22], (string)((object[])context)[23], (string)((object[])context)[24]);
                    XmlFareAvail[index] = null;
                }
                else
                {
                    XmlFareAvail[index] = airSvc.SearchFareAvailAmadeusRS(
                        (int)((object[])context)[5], 
                        (string)((object[])context)[6], 
                        (string)((object[])context)[7], 
                        (string)((object[])context)[8], 
                        (string)((object[])context)[9], 
                        (string)((object[])context)[10], 
                        (string)((object[])context)[11], 
                        (string)((object[])context)[12], 
                        (string)((object[])context)[13], 
                        (string)((object[])context)[14], 
                        (string)((object[])context)[15], 
                        (string)((object[])context)[16], 
                        (string)((object[])context)[17], 
                        (string)((object[])context)[18],
                        (string)((object[])context)[19], 
                        (int)((object[])context)[20], 
                        (int)((object[])context)[21], 
                        (int)((object[])context)[22], 
                        (int)((object[])context)[23], 
                        (string)((object[])context)[24], 
                        (int)((object[])context)[25], 
                        (string)((object[])context)[26],
                        (string)((object[])context)[27],
                        (string)((object[])context)[28],
                        (string)((object[])context)[29],
                        (XmlElement)((object[])context)[30]);
                }
			}
			catch (Exception ex)
			{
                new MWSException(ex, (HttpContext)((object[])context)[2], "Mode", "SearchFareAvailGrouping2", 0, 0);
                XmlFareAvail[index] = new MWSExceptionMode(ex, (HttpContext)((object[])context)[2], (string)((object[])context)[28], "AirService3", "SearchFareAvailGrouping2", 655, 0, 0).ToErrors;
			}
			finally
			{
				ThreadCount++;
				doneEvent.Set();
			}
		}
	}
}