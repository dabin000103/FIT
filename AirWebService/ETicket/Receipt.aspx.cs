using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace AirWebService.ETicket
{
    public partial class Receipt : System.Web.UI.Page
	{
        private Common cm = new Common();
        private string OID = string.Empty;
        private string PID = string.Empty;
        private string NSI = string.Empty;
        private string SNM = string.Empty;
        private string RIP = string.Empty;
        private string RQR = string.Empty;
        private string RQT = string.Empty;
        private string MINFO = string.Empty;

		protected void Page_Load(object sender, EventArgs e)
		{
            int ServiceNumber = 532;
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

                    OID = Params[2];
                    PID = Params[6];
                    SNM = Params[0];
                    RIP = Params[18];
                }
                else
                {
                    OID = Request["OID"];
                    PID = Request["PID"];
                    NSI = Request["NSI"];
                    SNM = Request["SNM"];
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
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000)
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
                sqlParam[14].Value = NSI;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                if (cm.RequestInt(OID) > 0 && cm.RequestInt(PID) > 0)
                {
                    string CancelYN = "N"; //취소 여부
                    string IssueYN = "N"; //발권완료 여부
                    string IssueDate = ""; //발권완료일
                    int PaxCount = 0; //탑승객수
                    int TotalPrice = 0; //전체 금액
                    int TotalPayment = 0; //전체 결제금액
                    int PaymentCard = 0; //카드 결제금액
                    string PaymentCardDate = string.Empty; //카드 결제일(최종)
                    
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString);
                        SqlDataReader dr = null;

                        cmd.Connection = conn;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (String.IsNullOrWhiteSpace(NSI))
                        {
                            cmd.CommandText = "DBO.WSV_S_아이템예약_해외항공_매출전표";

                            cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@예약자번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@주문번호"].Value = OID;
                            cmd.Parameters["@예약자번호"].Value = PID;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            cmd.CommandText = "DBO.WSV_S_아이템예약_해외항공_매출전표_탑승자";

                            cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@예약자번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@탑승자번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@주문번호"].Value = OID;
                            cmd.Parameters["@예약자번호"].Value = PID;
                            cmd.Parameters["@탑승자번호"].Value = NSI;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;
                        }

                        try
                        {
                            conn.Open();
                            dr = cmd.ExecuteReader();

                            if (dr.Read())
                            {
                                PaxCount = Convert.ToInt32(dr["탑승객수"]);
                                TotalPrice = Convert.ToInt32(dr["총금액"]);
                                
                                ltrOrderNumber.Text = dr["주문번호"].ToString();
                                ltrAirline.Text = dr["대표항공사"].ToString();
                                ltrItinerary.Text = String.Format("{0} - {1}", dr["출발공항"], dr["도착공항"]);
                                ltrPaxInfo.Text = (PaxCount > 1) ? String.Format("{0} 외 {1}명", dr["대표탑승객명"], (PaxCount - 1)) : dr["대표탑승객명"].ToString();
                                ltrFare.Text = String.Format("{0:#,##0}", dr["총항공료"]);
                                ltrFuelSurcharge.Text = String.Format("{0:#,##0}", dr["총유류할증료"]);
                                ltrTax.Text = String.Format("{0:#,##0}", dr["총텍스"]);
                                ltrTASF.Text = String.Format("{0:#,##0}", Convert.ToInt32(dr["총발권수수료"]) + Convert.ToInt32(dr["총취급수수료"]));
                                ltrPayment.Text = "0 원";
                                //ltrPrice.Text = String.Format("{0:#,##0}", dr["총금액"]);

                                CancelYN = dr["취소"].ToString();
                                IssueYN = dr["발권완료여부"].ToString();
                                IssueDate = dr["발권완료일"].ToString();
                            }

                            dr.NextResult();

                            if (dr.Read())
                            {
                                PaymentCardDate = dr["카드결제일"].ToString();
                                PaymentCard = Convert.ToInt32(dr["카드지불금액"]);
                            }

                            dr.NextResult();

                            //if (dr.Read())
                            //{
                            //    PaymentCardDate = IssueDate;
                            //    PaymentCard = Convert.ToInt32(dr["카드결제요청금액"]);
                            //}

                            dr.NextResult();

                            if (dr.Read())
                            {
                                TotalPayment = Convert.ToInt32(dr["총지불금액"]);
                            }
                        }
                        catch (Exception ex) { Response.Write(ex.ToString()); }
                        finally
                        {
                            dr.Dispose();
                            dr.Close();
                            conn.Close();
                        }
                    }

                    //취소
                    if (CancelYN.Equals("Y"))
                    {
                        ltrPaymentDate.Text = PaymentCardDate;
                        ltrPayment.Text = "취소";
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(NSI))
                        {
                            //발권완료인 경우
                            if (IssueYN.Equals("Y"))
                            {
                                //총금액과 총지불금액이 같은 경우(2018-06-07,김지영팀장)
                                if (TotalPrice.Equals(TotalPayment))
                                {
                                    ltrPaymentDate.Text = PaymentCardDate;
                                    ltrPayment.Text = String.Format("{0:#,##0} 원", PaymentCard);
                                }
                                else
                                {
                                    ltrPaymentDate.Text = PaymentCardDate;
                                    ltrPayment.Text = "반영중";
                                }
                            }
                            else
                            {
                                ltrPaymentDate.Text = PaymentCardDate;
                                ltrPayment.Text = "발권 미완료";
                            }
                        }
                        else
                        {
                            //총금액과 총지불금액이 같은 경우(2018-06-07,김지영팀장)
                            if (TotalPrice.Equals(TotalPayment))
                            {
                                ltrPaymentDate.Text = PaymentCardDate;
                                ltrPayment.Text = String.Format("{0:#,##0} 원", PaymentCard);
                            }
                            else
                            {
                                ltrPaymentDate.Text = PaymentCardDate;
                                ltrPayment.Text = "반영중";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Response.Write(ex.ToString()); }
		}
	}
}