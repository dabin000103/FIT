using System;
using System.Web;
using System.Xml;

namespace AirWebService.ETicket
{
	public partial class ETicket : System.Web.UI.Page
	{
		public Common cm;
		private string OID = string.Empty;
		private string PID = string.Empty;
        private string SNM = string.Empty;
        private string GDS = string.Empty;
        private string PNR = string.Empty;
		private string PaxName = string.Empty;
        private string TicketNumber = string.Empty;
        private string Item = string.Empty;
        private string RIP = string.Empty;
        private string StrAgentInfo = string.Empty;
        private string Logo = string.Empty;
        private string MINFO = string.Empty;
        public bool ExistFareInfo = false;
        public bool SimpleTicket = false;
        public bool PrintBtn = false;
        public int SiteNum = 0;

		protected void Page_Load(object sender, EventArgs e)
		{
            try
            {
                OID = Request["OID"];
                PID = Request["PID"];
                SNM = Request["SNM"];
                GDS = Request["GDS"];
                PNR = Request["PNR"];
                PaxName = Request["PaxName"];
                TicketNumber = Request["TicketNumber"];
                Item = Request["Item"];
                RIP = Request["RIP"];

                //거래처직원명,연락처
                StrAgentInfo = Request["AgentInfo"];

                //로고출력여부
                Logo = Request["Logo"];

                //인쇄버튼 노출여부(기본값:미노출)
                PrintBtn = (Request["PrintBtn"] != null && Request["PrintBtn"].Equals("N")) ? false : true;
                
                //간략양식(요금, 배너 및 법적고지 내용 미출력)
                SimpleTicket = (Request["Simple"] != null && Request["Simple"].Equals("Y")) ? true : false;

                //암호화링크
                MINFO = Request["MINFO"];

                if (!String.IsNullOrWhiteSpace(MINFO))
                {
                    string[] Params = new AES256Cipher().AESDecrypt(AES256Cipher.KeyName(2), MINFO).Split(':');

                    OID = Params[2];
			        PID = Params[6];
                    SNM = Params[0];
                    GDS = Params[8];
                    PNR = Params[10];
			        PaxName = Params[12];
                    RIP = Params[18];
                    StrAgentInfo = Params[14];
                    Logo = Params[16];

                    if (Params.Length > 20)
                    {
                        TicketNumber = Params[22];
                        Item = Params[24];
                    }

                    if (OID.Equals("0"))
                        OID = "";

                    if (PID.Equals("0"))
                        PID = "";

                    //Response.Write("OID : " + OID + Environment.NewLine);
                    //Response.Write("PID : " + PID + Environment.NewLine);
                    //Response.Write("SNM : " + SNM + Environment.NewLine);
                    //Response.Write("GDS : " + GDS + Environment.NewLine);
                    //Response.Write("PNR : " + PNR + Environment.NewLine);
                    //Response.Write("PaxName : " + PaxName + Environment.NewLine);
                    //Response.Write("RIP : " + RIP + Environment.NewLine);
                    //Response.Write("StrAgentInfo : " + StrAgentInfo + Environment.NewLine);
                    //Response.Write("Logo : " + Logo + Environment.NewLine);
                    //Response.Write("TicketNumber : " + TicketNumber + Environment.NewLine);
                    //Response.Write("Item : " + Item + Environment.NewLine);
                }
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

                XmlElement XmlETicket = (Item != null && TicketNumber != null && Item.Equals("GA") && !String.IsNullOrWhiteSpace(TicketNumber)) ? mas.SearchETicketDocGroupRS(cm.RequestInt(OID), cm.RequestInt(PID), TicketNumber, RIP) : ((!String.IsNullOrWhiteSpace(OID) && !String.IsNullOrWhiteSpace(PID)) ? mas.SearchETicketDocRS(cm.RequestInt(OID), cm.RequestInt(PID), PaxName, RIP) : mas.SearchETicketDocPNRRS(cm.RequestInt(SNM, 67), GDS, PNR, PaxName));
                //Response.Write((Item != null && TicketNumber != null && Item.Equals("GA") && !String.IsNullOrWhiteSpace(TicketNumber)) ? "SearchETicketDocGroupRS" : ((!String.IsNullOrWhiteSpace(PNR)) ? "SearchETicketDocPNRRS" : "SearchETicketDocRS"));
                
				if (XmlETicket.SelectNodes("bookingInfo").Count > 0)
				{
					XmlNode BookingInfo = XmlETicket.SelectSingleNode("bookingInfo");
					XmlNode TravellerInfo = XmlETicket.SelectSingleNode("travellerInfo/paxData");
                    XmlNode FlightInfo = XmlETicket.SelectSingleNode("flightInfo");
                    XmlNode FareInfo = XmlETicket.SelectSingleNode("fareInfo");
                    XmlNode AgentInfo = XmlETicket.SelectSingleNode("agentInfo");

                    if (!FlightInfo.HasChildNodes)
                        throw new Exception("여정 정보가 존재하지 않습니다.");
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(FlightInfo.SelectSingleNode("seg[last()]").Attributes.GetNamedItem("ardt").InnerText) && cm.DateDiff("d", FlightInfo.SelectSingleNode("seg[last()]").Attributes.GetNamedItem("ardt").InnerText, DateTime.Now.ToString("yyyy-MM-dd HH:mm")) > 0)
                            throw new Exception("지난 여정에 대해서는 이티켓 확인이 불가합니다.");
                    }

					ltrPaxName.Text = BookingInfo.SelectSingleNode("paxName").InnerText;
					//ltrBookingNumber.Text = BookingInfo.SelectSingleNode("bookingNo").InnerText.Equals(BookingInfo.SelectSingleNode("bookingNo").Attributes.GetNamedItem("pnr").InnerText) ? BookingInfo.SelectSingleNode("bookingNo").InnerText : String.Format("{0} ({1})", BookingInfo.SelectSingleNode("bookingNo").InnerText, BookingInfo.SelectSingleNode("bookingNo").Attributes.GetNamedItem("pnr").InnerText);
                    ltrBookingNumber.Text = String.IsNullOrWhiteSpace(XmlETicket.SelectSingleNode("flightInfo/seg[1]/airlineRefNumber").InnerText) ? BookingInfo.SelectSingleNode("bookingNo").InnerText : String.Format("{0} ({1})", BookingInfo.SelectSingleNode("bookingNo").InnerText, XmlETicket.SelectSingleNode("flightInfo/seg[1]/airlineRefNumber").InnerText);
                    ltrTicketNumber.Text = String.IsNullOrWhiteSpace(TicketNumber) ? BookingInfo.SelectSingleNode("ticketNumber").InnerText : TicketNumber;
                    ltrModetourBookingNumber.Text = (String.IsNullOrWhiteSpace(BookingInfo.SelectSingleNode("modeBookingNo").InnerText)) ? "&nbsp;" : BookingInfo.SelectSingleNode("modeBookingNo").InnerText;
                    ltrAgentInfo.Text = String.IsNullOrWhiteSpace(StrAgentInfo) ? ((String.IsNullOrWhiteSpace(AgentInfo.SelectSingleNode("company").InnerText)) ? "(주)모두투어네트워크" : String.Format("{0} {1} (tel {2})", AgentInfo.SelectSingleNode("company").InnerText, AgentInfo.SelectSingleNode("name").InnerText, AgentInfo.SelectSingleNode("tel").InnerText)) : StrAgentInfo;
                    ltrLogo.Text = (!String.IsNullOrWhiteSpace(Logo) && Logo.Equals("N")) ? "<img src=\"//img.modetour.co.kr/blank.gif\" alt=\"모두투어\" height=\"27\"/>" : "<img src=\"//img.modetour.co.kr/modetour/2014/common/hn_modetour.png\" alt=\"모두투어\" height=\"27\"/>";
                    
                    if (!SimpleTicket && !String.IsNullOrWhiteSpace(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("fare").InnerText))
                    {
                        ltrCurrency.Text = FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("cnc").InnerText;
                        ltrFare.Text = String.Format("{0:#,##0}", Convert.ToInt32(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("fare").InnerText));
                        ltrTax.Text = String.Format("{0:#,##0}", Convert.ToInt32(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("tax").InnerText));
                        ltrFsc.Text = String.Format("{0:#,##0}", Convert.ToInt32(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("fsc").InnerText));
                        ltrTasf.Text = String.Format("{0:#,##0}", Convert.ToInt32(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("tasf").InnerText));
                        ltrAmount.Text = String.Format("{0:#,##0}", Convert.ToInt32(FareInfo.SelectSingleNode("fare").Attributes.GetNamedItem("amount").InnerText));
                        ltrPayment.Text = FareInfo.SelectSingleNode("payment").InnerText;

                        if (!String.IsNullOrWhiteSpace(FareInfo.SelectSingleNode("payment").Attributes.GetNamedItem("cardNo").InnerText))
                            ltrPayment.Text += String.Format(" ({0})", FareInfo.SelectSingleNode("payment").Attributes.GetNamedItem("cardNo").InnerText);

                        ExistFareInfo = true;
                    }

                    //티켓번호가 존재하는 여정만 이티켓에 출력(2019-10-08,김지영팀장)
                    rptFlightInfo.DataSource = XmlETicket.SelectNodes("flightInfo/seg[ticketNumber!='']");
					rptFlightInfo.DataBind();
				}

                if (XmlETicket.SelectNodes("agentInfo").Count > 0)
                {
                    if ((XmlAttribute)XmlETicket.SelectSingleNode("agentInfo").Attributes.GetNamedItem("snm") != null)
                    {
                        SiteNum = cm.RequestInt(XmlETicket.SelectSingleNode("agentInfo").Attributes.GetNamedItem("snm").InnerText);
                    }
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
                ex.Data.Add("TicketNumber", TicketNumber);

                HttpContext.Current.Response.Write(new MWSException(ex, HttpContext.Current, "ETicket", (!String.IsNullOrWhiteSpace(TicketNumber) ? TicketNumber : (!String.IsNullOrWhiteSpace(PaxName) ? PaxName : "ETicket")), cm.RequestInt(OID), 0).ToString());
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