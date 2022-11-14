using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Xml;

namespace AirWebService.ETicket
{
    public partial class Invoice : System.Web.UI.Page
    {
        private Common cm = new Common();
        public int OID = 0;
        private int PID = 0;
        private int SNM = 0;
        private string RIP = string.Empty;
        private string RQR = string.Empty;
        private string RQT = string.Empty;
        private string MINFO = string.Empty;
        public int SegCount = 0;
        public int ADTFareCount = 0;
        public int CHDFareCount = 0;
        public int INFFareCount = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            int ServiceNumber = 690;
            string LogGUID = cm.GetGUID;
            LogSave log = new LogSave();
            HttpContext hcc = HttpContext.Current;

            try
            {
                //암호화링크
                MINFO = Request["MINFO"];

                if (!String.IsNullOrWhiteSpace(MINFO))
                {
                    string[] Params = new AES256Cipher().AESDecrypt(AES256Cipher.KeyName(2), MINFO).Split(':');

                    OID = cm.RequestInt(Params[2]);
                    PID = cm.RequestInt(Params[6]);
                    SNM = cm.RequestInt(Params[0]);
                    RIP = Params[18];
                }
                else
                {
                    OID = cm.RequestInt(Request["OID"]);
                    PID = cm.RequestInt(Request["PID"]);
                    SNM = cm.RequestInt(Request["SNM"]);
                    RIP = Request["RIP"];
                    RQR = Request["RQR"];
                    RQT = Request["RQT"];
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                Response.End();
            }

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@주문번호", SqlDbType.Int, 0),
                        new SqlParameter("@주문아이템번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = OID;
                sqlParam[3].Value = 0;
                sqlParam[4].Value = RQT;
                sqlParam[5].Value = Environment.MachineName;
                sqlParam[6].Value = hcc.Request.HttpMethod;
                sqlParam[7].Value = hcc.Request.UserHostAddress;
                sqlParam[8].Value = LogGUID;
                sqlParam[9].Value = OID;
                sqlParam[10].Value = PID;
                sqlParam[11].Value = RIP;
                sqlParam[12].Value = RQR;
                sqlParam[13].Value = MINFO;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                if (OID > 0 && PID > 0)
                {
                    AirService mas = new AirService();
                    cm = new Common();
                    
                    XmlElement XmlBook = mas.SearchBooking(SNM, OID, PID, LogGUID, mas.SearchBookingDB(OID, PID, RIP));
                    XmlNodeList SegList = XmlBook.SelectNodes("flightInfo/segGroup/seg");
                    XmlNodeList ADTFareList = XmlBook.SelectNodes("travellerInfo/paxData[pax/@ptc='ADT']/fare");
                    XmlNodeList CHDFareList = XmlBook.SelectNodes("travellerInfo/paxData[pax/@ptc='CHD']/fare");
                    XmlNodeList INFFareList = XmlBook.SelectNodes("travellerInfo/paxData[pax/@ptc='INF']/fare");
                    XmlNode PaymentInfo = XmlBook.SelectSingleNode("paymentInfo");

                    SegCount = SegList.Count;

                    ltrBooker.Text = XmlBook.SelectSingleNode("attn").Attributes.GetNamedItem("rhn").InnerText;
                    ltrPublishDate.Text = DateTime.Now.ToString("yyyy.MM.dd");

                    rptFlightInfo.DataSource = SegList;
                    rptFlightInfo.DataBind();

                    if (ADTFareList.Count > 0)
                    {
                        ADTFareCount = ADTFareList.Count;
                        ltrADTCount.Text = ADTFareList.Count.ToString();
                        ltrADTFare.Text = String.Format("{0:#,##0}원", Convert.ToInt32(ADTFareList[0].Attributes.GetNamedItem("fare").InnerText));
                        ltrADTFsc.Text = String.Format("{0:#,##0}원", Convert.ToInt32(ADTFareList[0].Attributes.GetNamedItem("fsc").InnerText));
                        ltrADTTax.Text = String.Format("{0:#,##0}원", Convert.ToInt32(ADTFareList[0].Attributes.GetNamedItem("tax").InnerText));
                        ltrADTTasf.Text = String.Format("{0:#,##0}원", Convert.ToInt32(ADTFareList[0].Attributes.GetNamedItem("tasf").InnerText));
                        ltrADTPrice.Text = String.Format("{0:#,##0}원", Convert.ToInt32(ADTFareList[0].Attributes.GetNamedItem("price").InnerText));
                    }

                    if (CHDFareList.Count > 0)
                    {
                        CHDFareCount = CHDFareList.Count;
                        ltrCHDCount.Text = CHDFareList.Count.ToString();
                        ltrCHDFare.Text = String.Format("{0:#,##0}원", Convert.ToInt32(CHDFareList[0].Attributes.GetNamedItem("fare").InnerText));
                        ltrCHDFsc.Text = String.Format("{0:#,##0}원", Convert.ToInt32(CHDFareList[0].Attributes.GetNamedItem("fsc").InnerText));
                        ltrCHDTax.Text = String.Format("{0:#,##0}원", Convert.ToInt32(CHDFareList[0].Attributes.GetNamedItem("tax").InnerText));
                        ltrCHDTasf.Text = String.Format("{0:#,##0}원", Convert.ToInt32(CHDFareList[0].Attributes.GetNamedItem("tasf").InnerText));
                        ltrCHDPrice.Text = String.Format("{0:#,##0}원", Convert.ToInt32(CHDFareList[0].Attributes.GetNamedItem("price").InnerText));
                    }

                    if (INFFareList.Count > 0)
                    {
                        INFFareCount = INFFareList.Count;
                        ltrINFCount.Text = INFFareList.Count.ToString();
                        ltrINFFare.Text = String.Format("{0:#,##0}원", Convert.ToInt32(INFFareList[0].Attributes.GetNamedItem("fare").InnerText));
                        ltrINFFsc.Text = String.Format("{0:#,##0}원", Convert.ToInt32(INFFareList[0].Attributes.GetNamedItem("fsc").InnerText));
                        ltrINFTax.Text = String.Format("{0:#,##0}원", Convert.ToInt32(INFFareList[0].Attributes.GetNamedItem("tax").InnerText));
                        ltrINFTasf.Text = String.Format("{0:#,##0}원", Convert.ToInt32(INFFareList[0].Attributes.GetNamedItem("tasf").InnerText));
                        ltrINFPrice.Text = String.Format("{0:#,##0}원", Convert.ToInt32(INFFareList[0].Attributes.GetNamedItem("price").InnerText));
                    }

                    int CardGross = Gross(PaymentInfo.SelectNodes("cards/card"));
                    int BankGross = Gross(PaymentInfo.SelectNodes("banks/bank"));

                    ltrCardPayment.Text = String.Format("{0:#,##0}원", CardGross);
                    ltrBankPayment.Text = String.Format("{0:#,##0}원", BankGross);
                    ltrPayment.Text = String.Format("{0:#,##0}원", (CardGross + BankGross));
                }
            }
            catch (Exception ex) { Response.Write(ex.ToString()); }
        }

        protected int Gross(XmlNodeList NodeList)
        {
            int SumGross = 0;
            
            foreach (XmlNode node in NodeList)
            {
                SumGross += Convert.ToInt32(node.Attributes.GetNamedItem("gross").InnerText);
            }

            return SumGross;
        }
    }
}