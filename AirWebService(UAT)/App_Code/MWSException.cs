using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Xml;
//using AirWebService.ModeWebService.MailService;
using System.Collections.Specialized;

namespace AirWebService
{
	/// <summary>
	/// 웹서비스용 Exception 재정의
	/// </summary>
	public class MWSException : Exception
	{
		private const string ServiceName = "AirWebService";
		
		private XmlElement mXmlElmError;
		private string mMessage = string.Empty;
		private string mToString = string.Empty;
		private string mGDS = string.Empty;
		private string mWebServiceName = string.Empty;
		private string mRequestLocalAddr = string.Empty;
        private int mOrderNumber = 0;
        private int mItemBookingNumber = 0;
		private DateTime Now;

		protected XmlElement XmlElmError
		{
			get { return mXmlElmError; }
		}

		public override string Message
		{
			get { return mMessage; }
		}

		public new string ToString
		{
			get { return mToString; }
		}

		public string GDS
		{
			get { return mGDS; }
		}

		public string WebServiceName
		{
			get { return mWebServiceName; }
		}

		public string RequestLocalAddr
		{
			get { return mRequestLocalAddr; }
		}

        public int OrderNumber
        {
            get { return mOrderNumber; }
        }

        public int ItemBookingNumber
        {
            get { return mItemBookingNumber; }
        }

		/// <summary>
		/// 에러 공통 형식으로 변경
		/// </summary>
		/// <returns></returns>
		public XmlElement ToErrors
		{
			get {
				ModeConfig mc = new ModeConfig();
				Common cm = new Common();
				
				XmlDocument XmlErr = new XmlDocument();
				XmlErr.Load(mc.XmlFullPath("Errors"));

				XmlErr.SelectSingleNode("ErrorMessage").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
				XmlErr.SelectSingleNode("ErrorMessage/errorSource/gds").InnerText = GDS;
				XmlErr.SelectSingleNode("ErrorMessage/errorSource/method").InnerText = WebServiceName;
				XmlErr.SelectSingleNode("ErrorMessage/errorSource/server").InnerText = Environment.MachineName;
				XmlErr.SelectSingleNode("ErrorMessage/errorSource/requestAddr").InnerText = RequestLocalAddr;
                XmlErr.SelectSingleNode("ErrorMessage/errorSource/orderNumber").InnerText = OrderNumber.ToString();
                XmlErr.SelectSingleNode("ErrorMessage/errorSource/itemBookingNumber").InnerText = ItemBookingNumber.ToString();
                XmlErr.SelectSingleNode("ErrorMessage/errorMessageText/description").AppendChild((XmlCDataSection)XmlErr.CreateCDataSection(Message.Equals("원격 서버에서 (500) 내부 서버 오류 오류를 반환했습니다.") ? "항공사로부터 요청하신 정보를 받을 수 없습니다." : Message));
				XmlErr.SelectSingleNode("ErrorMessage/errorOriginal").AppendChild((XmlCDataSection)XmlErr.CreateCDataSection(ToString));

				return XmlErr.DocumentElement;
			}
		}

        public MWSException(Exception ex, HttpContext hcc, string GDS, string WebServiceName, int OrderNumber, int ItemBookingNumber)
			: base(ex.ToString())
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlNode Node1 = XmlDoc.CreateElement("WSErrors");
			XmlNode Node2 = XmlDoc.CreateElement("WSError");
			XmlNode Node3;
			XmlNode Node4;

			string ServerAddress = string.Empty;
			string WebServiceURL = string.Empty;
			string UserHostAddress = string.Empty;
			string UserAgent = string.Empty;
			string HttpMethod = string.Empty;
            string HeaderInfo = string.Empty;
			string FormString = string.Empty;
			string QueryString = string.Empty;
			string Data = string.Empty;
			string HelpLink = string.Empty;
			string HResult = string.Empty;
			string InnerException = string.Empty;
			string Message = string.Empty;
			string Source = string.Empty;
			string StackTrace = string.Empty;
			string TargetSite = string.Empty;
			string Description = string.Empty;

			try
			{
				//현재시간
				Now = DateTime.Now;

				//서버정보
				Node3 = XmlDoc.CreateElement("ServerInfo");

				//오류발생 웹서비스 서버 IP
				ServerAddress = hcc.Request.ServerVariables["LOCAL_ADDR"].ToString();
				Node4 = XmlDoc.CreateElement("ServerAddress");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(ServerAddress));
				Node3.AppendChild(Node4);

				//오류발생 웹서비스 URL
				WebServiceURL = hcc.Request.Url.AbsoluteUri;
				Node4 = XmlDoc.CreateElement("WebServiceURL");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(WebServiceURL)));
				Node3.AppendChild(Node4);

				Node2.AppendChild(Node3);

				//사용자정보
				Node3 = XmlDoc.CreateElement("UserInfo");

				//사용자 IP
				UserHostAddress = hcc.Request.UserHostAddress;
				Node4 = XmlDoc.CreateElement("UserHostAddress");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(UserHostAddress));
				Node3.AppendChild(Node4);

				//사용자 접속 환경
				UserAgent = hcc.Request.UserAgent;
				Node4 = XmlDoc.CreateElement("UserAgent");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(UserAgent)));
				Node3.AppendChild(Node4);

				Node2.AppendChild(Node3);

				//요청정보
				Node3 = XmlDoc.CreateElement("RequestInfo");

				//요청일시
				Node4 = XmlDoc.CreateElement("DateTime");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(Now.ToString()));
				Node3.AppendChild(Node4);

				//전송 Method
				HttpMethod = hcc.Request.HttpMethod;
				Node4 = XmlDoc.CreateElement("HttpMethod");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(HttpMethod));
				Node3.AppendChild(Node4);

                //Header 정보
                NameValueCollection HeaderList = hcc.Request.Headers;
                for (int i = 0; i < HeaderList.Count; i++)
                {
                    if (HeaderInfo.Length > 0)
                        HeaderInfo = String.Concat(HeaderInfo, "\n");

                    HeaderInfo += String.Format("KEY: {0} / VALUE: {1}", HeaderList.GetKey(i), HeaderList.Get(i));
                }

                Node4 = XmlDoc.CreateElement("HeaderInfo");
                Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(HeaderInfo));
                Node3.AppendChild(Node4);

				//전송 데이타
				FormString = hcc.Server.UrlDecode(hcc.Request.Form.ToString());
				Node4 = XmlDoc.CreateElement("FormString");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(FormString)));
				Node3.AppendChild(Node4);

				//전송 데이타
				QueryString = hcc.Server.UrlDecode(hcc.Request.QueryString.ToString());
				Node4 = XmlDoc.CreateElement("QueryString");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(QueryString)));
				Node3.AppendChild(Node4);

				Node2.AppendChild(Node3);

				//예외오류정보
				Node3 = XmlDoc.CreateElement("ExceptionInfo");

				//예외에 대한 사용자 정의 추가 정보를 제공하는 키/값 쌍의 컬렉션을 가져옴
				if (ex.Data.Count > 0)
				{
					foreach (DictionaryEntry de in ex.Data)
					{
						if (Data.Length > 0)
							Data = String.Concat(Data, "\n");

						Data += String.Format("KEY: {0} / VALUE: {1}", de.Key, de.Value);
					}
				}

				Node4 = XmlDoc.CreateElement("Data");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(Data)));
				Node3.AppendChild(Node4);

				//예외와 관련된 도움말 파일에 대한 링크를 가져오거나 설정
				Node4 = XmlDoc.CreateElement("HelpLink");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(ex.HelpLink)));
				Node3.AppendChild(Node4);

				//특정 예외에 할당된 코드화된 숫자 값인 HResult를 가져오거나 설정
				//HResult = ex.HResult.ToString();
				//Node4 = XmlDoc.CreateElement("HResult");
				//Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(HResult)));
				//Node3.AppendChild(Node4);

				//현재 예외를 발생시키는 Exception 인스턴스를 가져옴
				//InnerException = ex.InnerException.ToString();
				//Node4 = XmlDoc.CreateElement("InnerException");
				//Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(InnerException)));
				//Node3.AppendChild(Node4);

				//현재 예외를 설명하는 메시지를 가져옴
				Message = ex.Message;
				Node4 = XmlDoc.CreateElement("Message");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(Message)));
				Node3.AppendChild(Node4);

				//오류를 발생시키는 응용 프로그램 또는 개체의 이름을 가져오거나 설정
				Source = ex.Source;
				Node4 = XmlDoc.CreateElement("Source");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(Source)));
				Node3.AppendChild(Node4);

				//현재 예외가 throw된 시간에 호출 스택의 프레임에 대한 문자열 표현을 가져옴
				//Node4 = XmlDoc.CreateElement("StackTrace");
				//Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(ex.StackTrace)));
				//Node3.AppendChild(Node4);

				//오류 발생 라인
				System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

				//for (int i = 0; i < trace.FrameCount; i++)
				//{
				//    if (i > 0)
				//        StackTrace = String.Concat(StackTrace, "\n");

				//    StackTrace = String.Concat(StackTrace, "MethodName : ", trace.GetFrame(0).GetMethod().Name);
				//    StackTrace = String.Concat(StackTrace, ", Line : ", trace.GetFrame(0).GetFileLineNumber());
				//    StackTrace = String.Concat(StackTrace, ", Column : ", trace.GetFrame(0).GetFileColumnNumber());
				//}

				foreach (System.Diagnostics.StackFrame frame in trace.GetFrames())
				{
					StackTrace = String.Concat(StackTrace, String.Format("{0}:{1}({2},{3})\n", frame.GetFileName(), frame.GetMethod().Name, frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
				}

				Node4 = XmlDoc.CreateElement("StackTrace");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(StackTrace));
				Node3.AppendChild(Node4);

				//현재 예외를 throw하는 메서드를 가져옴
				TargetSite = ex.TargetSite.ToString();
				Node4 = XmlDoc.CreateElement("TargetSite");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(TargetSite)));
				Node3.AppendChild(Node4);

				Description = ex.ToString();
				Node4 = XmlDoc.CreateElement("ToString");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(Description)));
				Node3.AppendChild(Node4);

				Node2.AppendChild(Node3);
			}
			catch (Exception ex2)
			{
				//예외오류정보
				Node3 = XmlDoc.CreateElement("ExceptionInfo");

				Description = ex2.ToString();
				Node4 = XmlDoc.CreateElement("ToString");
				Node4.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(RequestXml(Description)));
				Node3.AppendChild(Node4);

				Node2.AppendChild(Node3);
			}

			Node1.AppendChild(Node2);
			XmlDoc.AppendChild(Node1);

            DBSave(OrderNumber, ItemBookingNumber, GDS, WebServiceName, ServerAddress, WebServiceURL, UserHostAddress, UserAgent, HttpMethod, FormString, QueryString, Data, HelpLink, HResult, InnerException, Message, Source, StackTrace, TargetSite, Description);
			SendException(hcc, XmlDoc.DocumentElement, GDS, WebServiceName);

			mMessage = ex.Message;
			mToString = ex.ToString();
			mGDS = GDS;
			mWebServiceName = WebServiceName;
            mRequestLocalAddr = (hcc.Request != null) ? hcc.Request.ServerVariables["LOCAL_ADDR"] : "";
            mOrderNumber = OrderNumber;
            mItemBookingNumber = ItemBookingNumber;
		}

		/// <summary>
		/// Request값의 유효성검사(Xml 용)
		/// </summary>
		/// <param name="reqText"></param>
		/// <returns></returns>
		protected string RequestXml(string reqText)
		{
			if (!String.IsNullOrEmpty(reqText))
			{
				reqText = reqText.Replace("[", "(");
				reqText = reqText.Replace("]", ")");
				//reqText = reqText.Replace("&", "&amp;");
			}

			return reqText;
		}

		/// <summary>
		/// 예외에 대한 데이타 저장
		/// </summary>
		protected void SendException(HttpContext hcc, XmlElement XmlEx, string GDS, string WebServiceName)
		{
			mXmlElmError = XmlEx;

			try
			{
				DateTime NowDate = DateTime.Now;
				string FolderPath = String.Format(@"{0}WebServiceLog2\{1}\ErrorXml\{2}\{3}\{4}\", hcc.Request.PhysicalApplicationPath.Substring(0, 3), ServiceName, GDS, NowDate.ToString("yyyyMM"), NowDate.ToString("dd"));
                string FileName = String.Format("{0:yyyyMMddHHmmssfff}_{1}({2}).xml", NowDate, WebServiceName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

				//폴더가 없을 경우 생성
				if (!Directory.Exists(FolderPath))
					Directory.CreateDirectory(FolderPath);

				//파일저장
				XmlEx.OwnerDocument.Save(String.Concat(FolderPath, FileName));

				//이메일 발송
				//EMailSend(FileName, EMailForm(XmlEx));
			}
			catch (Exception) { }
		}

		#region "이메일발송"

		/// <summary>
		/// 이메일 발송
		/// </summary>
		/// <param name="Subject">제목</param>
		/// <param name="BodyHtml">HTML 형태의 본문</param>
		/// <returns></returns>
		protected void EMailSend(string Subject, string BodyHtml)
		{
            /*
			using (MailService mailService = new MailService())
			{
				mailService.EMailSend("Exception", "", "harius@modetour.com", String.Concat(ServiceName, "2"), "harius@modetour.com", "고재영", String.Format("{0} (MWSException)", Subject), "", BodyHtml, "");
			}
            */
		}

		protected string EMailForm(XmlElement XmlEx)
		{
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder(2048);

				sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
				sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"ko\" xml:lang=\"ko\">");
				sb.Append("<head>");
				sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"/>");
				sb.Append("<title>MWSException</title>");
				sb.Append("</head>");
				sb.Append("<body>");

				foreach (XmlNode Node1 in XmlEx.SelectSingleNode("WSError").ChildNodes)
				{
					sb.Append(String.Format("<h3 style=\"margin:5px 0 0 0;padding:2px;font-size:12pt;\">{0}</h3>", Node1.Name));
					sb.Append("<table width=\"100%\" border=\"1\" cellpadding=\"2\" cellspacing=\"1\" style=\"border:1px solid #666633;border-collapse:collapse;\">");

					foreach (XmlNode Node2 in Node1.ChildNodes)
					{
						sb.Append("<tr>");
						sb.Append(String.Format("<td width=\"120\" style=\"font-size:9pt;\">{0}</td>", Node2.Name));
						sb.Append(String.Format("<td style=\"font-size:9pt;\">{0}</td>", Node2.InnerText.Replace("\n", "<br/>")));
						sb.Append("</tr>");
					}

					sb.Append("</table>");
				}

				sb.Append("</body>");
				sb.Append("</html>");

				return sb.ToString();
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
		}

		#endregion "이메일발송"

		#region "DB저장"

        protected void DBSave(int OrderNumber, int ItemBookingNumber, string GDS, string WebServiceName, string ServerAddress, string WebServiceURL, string UserHostAddress, string UserAgent, string HttpMethod, string FormString, string QueryString, string Data, string HelpLink, string HResult, string InnerException, string Message, string Source, string StackTrace, string TargetSite, string Description)
		{/*
			using (SqlCommand cmd = new SqlCommand())
			{
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString))
				{
					cmd.Connection = conn;
                    cmd.CommandTimeout = 10;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "DBO.WSV_T_웹서비스_오류_저장";

					cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@ServiceName", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@WebServiceName", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@WebServiceURL", SqlDbType.VarChar, 1000);
					cmd.Parameters.Add("@ServerAddress", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@UserHostAddress", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@UserAgent", SqlDbType.VarChar, 200);
					cmd.Parameters.Add("@HttpMethod", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@FormString", SqlDbType.NVarChar, -1);
					cmd.Parameters.Add("@QueryString", SqlDbType.VarChar, -1);
					cmd.Parameters.Add("@Data", SqlDbType.VarChar, -1);
					cmd.Parameters.Add("@HelpLink", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@HResult", SqlDbType.VarChar, 50);
					cmd.Parameters.Add("@InnerException", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@Message", SqlDbType.NVarChar, -1);
					cmd.Parameters.Add("@Source", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@StackTrace", SqlDbType.VarChar, -1);
					cmd.Parameters.Add("@TargetSite", SqlDbType.VarChar, 500);
					cmd.Parameters.Add("@ToString", SqlDbType.VarChar, -1);
					cmd.Parameters.Add("@발생일", SqlDbType.DateTime);
                    cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                    cmd.Parameters.Add("@주문아이템번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@GDS"].Value = GDS;
					cmd.Parameters["@ServiceName"].Value = String.Concat(ServiceName, "2");
					cmd.Parameters["@WebServiceName"].Value = WebServiceName;
					cmd.Parameters["@WebServiceURL"].Value = WebServiceURL;
					cmd.Parameters["@ServerAddress"].Value = ServerAddress;
					cmd.Parameters["@UserHostAddress"].Value = UserHostAddress;
					cmd.Parameters["@UserAgent"].Value = String.IsNullOrEmpty(UserAgent) ? "" : UserAgent;
					cmd.Parameters["@HttpMethod"].Value = HttpMethod;
					cmd.Parameters["@FormString"].Value = FormString.Replace("'", "''");
					cmd.Parameters["@QueryString"].Value = QueryString.Replace("'", "''");
					cmd.Parameters["@Data"].Value = Data.Replace("'", "''");
					cmd.Parameters["@HelpLink"].Value = HelpLink;
					cmd.Parameters["@HResult"].Value = HResult.Replace("'", "''");
					cmd.Parameters["@InnerException"].Value = InnerException.Replace("'", "''");
					cmd.Parameters["@Message"].Value = Message.Replace("'", "''");
					cmd.Parameters["@Source"].Value = Source;
					cmd.Parameters["@StackTrace"].Value = StackTrace;
					cmd.Parameters["@TargetSite"].Value = TargetSite;
					cmd.Parameters["@ToString"].Value = Description.Replace("'", "''");
					cmd.Parameters["@발생일"].Value = Now.ToString();
                    cmd.Parameters["@주문번호"].Value = OrderNumber;
                    cmd.Parameters["@주문아이템번호"].Value = ItemBookingNumber;
					cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
					cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

					try
					{
						conn.Open();
						cmd.ExecuteNonQuery();
					}
					finally
					{
						conn.Close();
					}
				}
			}*/
		}

		#endregion "DB저장"
	}
}