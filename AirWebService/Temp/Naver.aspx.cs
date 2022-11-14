using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace AirWebService.Temp
{
    public partial class Naver : System.Web.UI.Page
    {
        AirService mas;
        public string DLC = string.Empty;
        public string DTD = string.Empty;
        public string ARD = string.Empty;
        public string AIRV = string.Empty;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            mas = new AirService();
            DLC = Request["DLC"];
            DTD = Request["DTD"];
            ARD = Request["ARD"];
            AIRV = Request["AIRV"];

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
            {
                using (DataSet ds = new DataSet())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        SqlDataAdapter adp = new SqlDataAdapter(cmd);

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_S_네이버_도착지리스트";

                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        adp.Fill(ds);
                        adp.Dispose();
                    }

                    rptNaverDestinationList.DataSource = ds.Tables[0];
                    rptNaverDestinationList.DataBind();
                }
                
                using (DataSet ds = new DataSet())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        SqlDataAdapter adp = new SqlDataAdapter(cmd);

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_S_네이버_운임리스트";

                        cmd.Parameters.Add("@ENDAIRP", SqlDbType.Char, 3);
                        cmd.Parameters.Add("@출발일", SqlDbType.VarChar, 8);
                        cmd.Parameters.Add("@귀국일", SqlDbType.VarChar, 8);
                        cmd.Parameters.Add("@AIRV", SqlDbType.NVarChar, 2);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@ENDAIRP"].Value = (String.IsNullOrWhiteSpace(DLC)) ? Convert.DBNull : DLC;
                        cmd.Parameters["@출발일"].Value = (String.IsNullOrWhiteSpace(DTD)) ? Convert.DBNull : DTD;
                        cmd.Parameters["@귀국일"].Value = (String.IsNullOrWhiteSpace(ARD)) ? Convert.DBNull : ARD;
                        cmd.Parameters["@AIRV"].Value = (String.IsNullOrWhiteSpace(AIRV)) ? Convert.DBNull : AIRV;
                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        adp.Fill(ds);
                        adp.Dispose();
                    }

                    rptNaverFareList.DataSource = ds.Tables[0];
                    rptNaverFareList.DataBind();
                }
            }
        }

        public string Availability(object DTD, object DLC, object ALC, object CLC, object SCD, object MCC, object Itinerary)
        {
            string TmpMCC = string.Empty;
            string TmpFLN = string.Empty;
            string[] TmpItinerary = Itinerary.ToString().Split('+');
            string StrItinerary = string.Empty;

            for (int i = 0; i < TmpItinerary.Length; i++)
            {
                if (i > 0)
                {
                    TmpMCC += ",";
                    TmpFLN += ",";
                }

                TmpMCC += MCC;
                TmpFLN += TmpItinerary[i].Substring(8, 4);
            }

            XmlElement ResXml = mas.AvailabilityRS(4638, DTD.ToString(), "", DLC.ToString(), ALC.ToString(), CLC.ToString(), SCD.ToString().Replace("+", ","), TmpMCC, TmpFLN, "1");

            foreach (XmlNode SegGroup in ResXml.SelectNodes("flightInfo/segGroup[1]"))
            {
                foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
                {
                    if (!String.IsNullOrWhiteSpace(StrItinerary))
                        StrItinerary += ",";

                    StrItinerary += String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}",
                                    Seg.Attributes.GetNamedItem("dlc").InnerText,
                                    Seg.Attributes.GetNamedItem("alc").InnerText,
                                    "",
                                    "",
                                    Seg.Attributes.GetNamedItem("ddt").InnerText.Substring(0, 10).Replace("-", ""),
                                    Seg.Attributes.GetNamedItem("ardt").InnerText.Substring(0, 10).Replace("-", ""),
                                    Seg.Attributes.GetNamedItem("ddt").InnerText.Substring(11, 5).Replace(":", ""),
                                    Seg.Attributes.GetNamedItem("ardt").InnerText.Substring(11, 5).Replace(":", ""),
                                    "",
                                    "",
                                    Seg.Attributes.GetNamedItem("mcc").InnerText,
                                    Seg.Attributes.GetNamedItem("fln").InnerText,
                                    Seg.SelectSingleNode("svcClass").Attributes.GetNamedItem("rbd").InnerText,
                                    "",
                                    "1",
                                    "0",
                                    "0",
                                    Seg.Attributes.GetNamedItem("occ").InnerText,
                                    "",
                                    "");
                }

                StrItinerary += String.Format(",{0}^^", SegGroup.Attributes.GetNamedItem("eft").InnerText);
            }

            return StrItinerary;
        }

        public string LinkTr(object GoodCode, object Rank, object Duplication)
        {
            return ((Convert.ToInt32(Eval("중복수")) - Convert.ToInt32(Eval("순위"))).Equals(0)) ? String.Format("<tr><td colspan=\"19\" style=\"text-align:left;background:#dfdfdf;white-space:initial;word-break:break-all;word-wrap:break-word;\"><span id=\"{0}\" class=\"schedule\"></span></td></tr>", GoodCode) : "";
        }
    }
}