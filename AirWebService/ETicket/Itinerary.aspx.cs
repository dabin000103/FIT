using System;
using System.Web;
using System.Xml;

namespace AirWebService.ETicket
{
    public partial class Itinerary : System.Web.UI.Page
    {
        public Common cm;
        private string OID = string.Empty;
        private string PID = string.Empty;
        private string SNM = string.Empty;
        private string GDS = string.Empty;
        private string PNR = string.Empty;
        public string PaxName = string.Empty;
        private string RIP = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            OID = Request["OID"];
            PID = Request["PID"];
            SNM = Request["SNM"];
            GDS = Request["GDS"];
            PNR = Request["PNR"];
            PaxName = Request["PaxName"];
            RIP = Request["RIP"];
        
            try
            {
                AirService mas = new AirService();
                cm = new Common();

                XmlElement XmlETicket = (!String.IsNullOrWhiteSpace(PNR)) ? mas.SearchETicketDocPNRRS(cm.RequestInt(SNM, 67), GDS, PNR, PaxName) : mas.SearchETicketDocRS(cm.RequestInt(OID), cm.RequestInt(PID), PaxName, RIP);
                
                if (XmlETicket.SelectNodes("bookingInfo").Count > 0)
                {
                    XmlNode BookingInfo = XmlETicket.SelectSingleNode("bookingInfo");
                    XmlNode TravellerInfo = XmlETicket.SelectSingleNode("travellerInfo/paxData");
                    XmlNode AgentInfo = XmlETicket.SelectSingleNode("agentInfo");

                    ltrPaxName.Text = BookingInfo.SelectSingleNode("paxName").InnerText;
                    ltrBookingNumber.Text = BookingInfo.SelectSingleNode("bookingNo").InnerText.Equals(BookingInfo.SelectSingleNode("bookingNo").Attributes.GetNamedItem("pnr").InnerText) ? BookingInfo.SelectSingleNode("bookingNo").InnerText : String.Format("{0} ({1})", BookingInfo.SelectSingleNode("bookingNo").InnerText, BookingInfo.SelectSingleNode("bookingNo").Attributes.GetNamedItem("pnr").InnerText);
                    ltrAgentInfo.Text = (String.IsNullOrWhiteSpace(AgentInfo.SelectSingleNode("company").InnerText)) ? "(주)모두투어네트워크" : String.Format("{0} {1} (tel {2})", AgentInfo.SelectSingleNode("company").InnerText, AgentInfo.SelectSingleNode("name").InnerText, AgentInfo.SelectSingleNode("tel").InnerText);
                    ltrNowDateTime.Text = DateTime.Now.ToString("yyyy\\/MM\\/dd");

                    rptFlightInfo.DataSource = XmlETicket.SelectNodes("flightInfo/seg");
                    rptFlightInfo.DataBind();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("OID", OID);
                ex.Data.Add("PID", PID);
                ex.Data.Add("SNM", PID);
                ex.Data.Add("GDS", PID);
                ex.Data.Add("PNR", PID);
                ex.Data.Add("PaxName", PaxName);

                HttpContext.Current.Response.Write(new MWSException(ex, HttpContext.Current, "ETicket", PaxName, cm.RequestInt(OID), 0).ToString());
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
    }
}