using System;
using System.Threading;
using System.Web;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// ThreadPool을 이용한 Amadeus MasterPricerTravelBoardSearch 동시조회
	/// </summary>
	public class SearchFareAvailPaxType
	{
		Common cm = new Common();
		private static string[] PaxType = new String[5]{"ADT", "DIS", "STU", "SRC", "LBR"};
		private int ThreadCount = 0;
		XmlElement[] XmlFareAvail;

		public XmlElement[] GetFareAvail(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, int ADC, int NRR, string FTX)
		{
			int TableCount = PaxType.Length;
			ManualResetEvent[] doneEvents = new ManualResetEvent[TableCount];
			AirService airSvc = new AirService();
			XmlFareAvail = new XmlElement[TableCount];
			
			//멀티쓰레드로 호출할 함수 설정 및 전달할 파라미터 설정
			for (int i = 0; i < PaxType.Length; i++)
			{
				doneEvents[i] = new ManualResetEvent(false);

				object[] objState = new object[] { i, doneEvents[i], HttpContext.Current, airSvc, SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, CCD, ADC, NRR, FTX };
				ThreadPool.QueueUserWorkItem(MonitoringCallBack, objState);
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

			return XmlFareAvail;
		}

		public void MonitoringCallBack(Object context)
		{
			int index = (int)((object[])context)[0];
			ManualResetEvent doneEvent = (ManualResetEvent)((object[])context)[1];
			HttpContext hcc = (HttpContext)((object[])context)[2];
			
			try
			{
				string[] PTC = new String[1] { PaxType[index] };
				int[] NOP = new Int32[1] { (int)((object[])context)[14] };

				AirService airSvc = (AirService)((object[])context)[3];
				XmlFareAvail[index] = airSvc.SearchFareAvailPaxTypeRS((int)((object[])context)[4], (string)((object[])context)[5], (string)((object[])context)[6], (string)((object[])context)[7], (string)((object[])context)[8], (string)((object[])context)[9], (string)((object[])context)[10], (string)((object[])context)[11], (string)((object[])context)[12], (string)((object[])context)[13], PTC, NOP, (int)((object[])context)[15], (string)((object[])context)[16]);
			}
			catch (Exception ex)
			{
                XmlFareAvail[index] = new MWSException(ex, hcc, "Mode", "SearchFareAvailPaxType", 0, 0).ToErrors;
			}
			finally
			{
				ThreadCount++;
				doneEvent.Set();
			}
		}
	}
}