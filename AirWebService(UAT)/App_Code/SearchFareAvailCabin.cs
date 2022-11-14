using System;
using System.Threading;
using System.Web;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// ThreadPool을 이용한 Amadeus MasterPricerTravelBoardSearch 동시조회
	/// </summary>
	public class SearchFareAvailCabin
	{
		Common cm = new Common();
		private int ThreadCount = 0;
		XmlElement[] XmlFareAvail;

        public XmlElement[] GetFareAvail(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string[] PTC, int[] NOP, int NRR, string FTX, string GUID)
		{
			try
			{
				string CabinString = (String.IsNullOrWhiteSpace(CCD)) ? "M,W,C,F," : ((CCD.Equals("Y")) ? "M,W," : String.Concat(CCD, ","));
				string[] Cabin = CabinString.Split(',');
				int TableCount = Cabin.Length - 1;
				ManualResetEvent[] doneEvents = new ManualResetEvent[TableCount];
				AirService airSvc = new AirService();
				XmlFareAvail = new XmlElement[TableCount];
			
				//멀티쓰레드로 호출할 함수 설정 및 전달할 파라미터 설정
				for (int i = 0; i < TableCount; i++)
				{
					doneEvents[i] = new ManualResetEvent(false);

                    object[] objState = new object[] { i, doneEvents[i], HttpContext.Current, airSvc, SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, Cabin[i].Trim(), ACQ, PTC, NOP, NRR, FTX, GUID };
					ThreadPool.QueueUserWorkItem(MonitoringCallBack, objState);
				}

				//호출완료 또는 시간초과 체크 후 종료처리
				int Lop = 0;

				while (true)
				{
					Thread.Sleep(500);
					Lop++;

					if (ThreadCount >= TableCount || Lop > 60)
						break;
				}
			}
			catch (Exception ex)
			{
				XmlFareAvail[0] = new MWSException(ex, HttpContext.Current, "Mode", "SearchFareAvailCabin", 0, 0).ToErrors;
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
				AirService airSvc = (AirService)((object[])context)[3];
                XmlFareAvail[index] = airSvc.SearchFareAvailDetailRS((int)((object[])context)[4], (string)((object[])context)[5], (string)((object[])context)[6], (string)((object[])context)[7], "", (string)((object[])context)[8], (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], (string)((object[])context)[14], "", (string[])((object[])context)[15], (int[])((object[])context)[16], (int)((object[])context)[17], (string)((object[])context)[18], (string)((object[])context)[19]);
			}
			catch (Exception ex)
			{
                XmlFareAvail[index] = new MWSException(ex, hcc, "Mode", "SearchFareAvailCabin", 0, 0).ToErrors;
			}
			finally
			{
				ThreadCount++;
				doneEvent.Set();
			}
		}
	}
}