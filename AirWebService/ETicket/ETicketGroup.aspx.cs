using System;
using System.Web;
using System.Xml;

namespace AirWebService.ETicket
{
	public partial class ETicketGroup : System.Web.UI.Page
	{
		public Common cm;
        private string OID = string.Empty;
		private string PID = string.Empty;
        private string PNR = string.Empty;
        private string RIP = string.Empty;
        private string RQT = string.Empty;
        private string MINFO = string.Empty;
        public bool SimpleTicket = false;
        public bool PrintBtn = false;

		protected void Page_Load(object sender, EventArgs e)
		{
            try
            {
                OID = Request["OID"];
                PID = Request["PID"];
                PNR = Request["PNR"];
                RIP = Request["RIP"];
                RQT = Request["RQT"];

                //인쇄버튼 노출여부(기본값:미노출)
                PrintBtn = (Request["PrintBtn"] != null && Request["PrintBtn"].Equals("Y")) ? true : false;
                
                //간략양식(요금, 배너 및 법적고지 내용 미출력)
                SimpleTicket = (Request["Simple"] != null && Request["Simple"].Equals("Y")) ? true : false;

                //암호화링크
                MINFO = Request["MINFO"];

                //if (!String.IsNullOrWhiteSpace(MINFO))
                //{
                //    string[] Params = new AES256Cipher().AESDecrypt(AES256Cipher.KeyName(2), MINFO).Split(':');

                //    OID = Params[2];
                //    PID = Params[6];
                //    PNR = Params[10];
                //    RIP = Params[18];
                //}
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                Response.End();
            }
			
			try
			{
				AirService mas = new AirService();
				cm = new Common();

                XmlElement XmlETicket = mas.SearchETicketDocGroupPNRRS(cm.RequestInt(OID), cm.RequestInt(PID), PNR, RIP, RQT);
                
				if (XmlETicket.SelectNodes("bookingInfo").Count > 0)
				{
					XmlNode BookingInfo = XmlETicket.SelectSingleNode("bookingInfo");
                    XmlNode FlightInfo = XmlETicket.SelectSingleNode("flightInfo");
                    XmlNode TravellerInfo = XmlETicket.SelectSingleNode("travellerInfo");
                    XmlNode AgentInfo = XmlETicket.SelectSingleNode("agentInfo");

                    ltrBookingNumber.Text = String.Format("{0} ({1})", BookingInfo.SelectSingleNode("bookingNo").InnerText, BookingInfo.SelectSingleNode("bookingNo").Attributes.GetNamedItem("pnr").InnerText);
                    ltrModetourBookingNumber.Text = (String.IsNullOrWhiteSpace(BookingInfo.SelectSingleNode("modeBookingNo").InnerText)) ? "&nbsp;" : BookingInfo.SelectSingleNode("modeBookingNo").InnerText;

                    rptFlightInfo.DataSource = FlightInfo.SelectNodes("seg");
					rptFlightInfo.DataBind();

                    rptTravellerInfo.DataSource = TravellerInfo.SelectNodes("traveller");
                    rptTravellerInfo.DataBind();
				}
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("OID", OID);
                ex.Data.Add("PID", PID);
                ex.Data.Add("PNR", PID);
                ex.Data.Add("RIP", PID);
                ex.Data.Add("RQT", RQT);

                HttpContext.Current.Response.Write(new MWSException(ex, HttpContext.Current, "ETicketGroup", PNR, cm.RequestInt(OID), 0).ToString());
			}
		}

        protected string GetBookingClass(string BookingClassName)
		{
			return (String.IsNullOrWhiteSpace(BookingClassName)) ? "" : String.Format("({0})", BookingClassName);
		}

        protected string CodeshareAgreement(object MCC, object OCC, object OperatingAirline)
        {
            return (MCC.Equals(OCC)) ? "" : String.Format("<div style=\"margin-top:3px;padding:5px;border:1px solid #ddd;font-size:12px;color:#333;font-weight:bold;\"><span style=\"color:#2988f4;\">[공동운항]</span> 해당 구간은 항공사간 제휴로 실제 탑승은 <span style=\"color:#2988f4;\">[{0}](으)로 운항하는 공동운항편</span>이며, [{0}](으)로 구입시와 운임이 다를수 있습니다. 탑승수속은 실제 운항항공사 카운터를 이용해 주시기 바라며, 운항 항공사 규정에 따라 탑승수속 마감 시간이 상이할 수 있으니 반드시 확인 바랍니다.</div>", OperatingAirline);
        }

        protected string DisplayFarebasis(XmlNode FlightInfo)
        {
            string StrFB = String.Empty;

            foreach (XmlNode Fare in FlightInfo.SelectNodes("fareInfo/fare"))
            {
                if (!String.IsNullOrWhiteSpace(StrFB))
                    StrFB += "&nbsp;&nbsp;&nbsp;";

                StrFB += String.Format("[{0}] {1}", Fare.Attributes.GetNamedItem("ptc").InnerText, Fare.Attributes.GetNamedItem("basis").InnerText);
            }

            return StrFB;
        }

        protected string DisplayBaggage(XmlNode FlightInfo)
        {
            string StrFB = String.Empty;

            foreach (XmlNode Fare in FlightInfo.SelectNodes("fareInfo/fare"))
            {
                if (!String.IsNullOrWhiteSpace(StrFB))
                    StrFB += "&nbsp;&nbsp;&nbsp;";

                StrFB += String.Format("[{0}] {1}", Fare.Attributes.GetNamedItem("ptc").InnerText, Fare.Attributes.GetNamedItem("baggage").InnerText);
            }

            return StrFB;
        }
	}
}