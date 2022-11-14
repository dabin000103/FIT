using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Security.Cryptography;

namespace AirWebService
{
	/// <summary>
	/// HttpWebRequest를 이용하기 위한 셋팅 및 데이타 받기
	/// </summary>
	public class XmlRequest
	{
		/// <summary>
		/// Soap Form 생성(아마데우스용)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="Namespace">Namespace</param>
		/// <param name="XmlData">XML 데이타</param>
		/// <returns></returns>
		public static string SoapHeaderForAmadeus(string SID, string SQN, string SCT, string Namespace, string XmlData)
		{
            return String.Format("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:wbs=\"http://xml.amadeus.com/ws/2009/01/WBS_Session-2.0.xsd\" xmlns:tfr=\"{3}\"><soapenv:Header><Session><SessionId>{0}</SessionId><SequenceNumber>{1}</SequenceNumber><SecurityToken>{2}</SecurityToken></Session></soapenv:Header><soapenv:Body>{4}</soapenv:Body></soapenv:Envelope>", SID, SQN, SCT, Namespace, XmlData);
		}

        /// <summary>
        /// Soap Form 생성(Soap Header 4.0 버전용)(아마데우스용)
        /// </summary>
        /// <param name="OID">Office ID</param>
        /// <param name="Url">End Point</param>
        /// <param name="Action">Action URL</param>
        /// <param name="XmlData">XML 데이타</param>
        /// <returns></returns>
        public static string SoapHeader4ForAmadeus(string OID, string Url, string Action, string XmlData)
        {
            SHA1Managed sha1 = new SHA1Managed();
            string MessageID = Guid.NewGuid().ToString();
            string Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss:fffZ");
            string Nonce = Common.Base64Encode(System.IO.Path.GetRandomFileName());
            byte[] pwd = sha1.ComputeHash(Encoding.UTF8.GetBytes(Common.Base64Decode("NGR1M1lTSSE=")));
            byte[] nonceBytes = Convert.FromBase64String(Nonce);
            byte[] createdBytes = Encoding.UTF8.GetBytes(Timestamp);
            byte[] operand = new byte[nonceBytes.Length + createdBytes.Length + pwd.Length];
            Array.Copy(nonceBytes, operand, nonceBytes.Length);
            Array.Copy(createdBytes, 0, operand, nonceBytes.Length, createdBytes.Length);
            Array.Copy(pwd, 0, operand, nonceBytes.Length + createdBytes.Length, pwd.Length);
            string PasswordDigest = Convert.ToBase64String(sha1.ComputeHash(operand));

            return String.Format("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:sec=\"http://xml.amadeus.com/2010/06/Security_v1\" xmlns:typ=\"http://xml.amadeus.com/2010/06/Types_v1\" xmlns:iat=\"http://www.iata.org/IATA/2007/00/IATA2010.1\" xmlns:app=\"http://xml.amadeus.com/2010/06/AppMdw_CommonTypes_v3\" xmlns:link=\"http://wsdl.amadeus.com/2010/06/ws/Link_v1\" xmlns:ses=\"http://xml.amadeus.com/2010/06/Session_v3\" xmlns:sat=\"http://xml.amadeus.com/SATRQT_07_1_1A\"><soapenv:Header><add:MessageID xmlns:add=\"http://www.w3.org/2005/08/addressing\">{0}</add:MessageID><add:Action xmlns:add=\"http://www.w3.org/2005/08/addressing\">{1}</add:Action><add:To xmlns:add=\"http://www.w3.org/2005/08/addressing\">{2}</add:To><link:TransactionFlowLink/><oas:Security xmlns:oas=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><oas:UsernameToken oas1:Id=\"UsernameToken-1\" xmlns:oas1=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"><oas:Username>WSMOTIBE</oas:Username><oas:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">{3}</oas:Nonce><oas:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">{4}</oas:Password><oas1:Created>{5}</oas1:Created></oas:UsernameToken></oas:Security><AMA_SecurityHostedUser xmlns=\"http://xml.amadeus.com/2010/06/Security_v1\"><UserID AgentDutyCode=\"SU\" POS_Type=\"1\" PseudoCityCode=\"{6}\" RequestorType=\"U\"/></AMA_SecurityHostedUser></soapenv:Header><soapenv:Body>{7}</soapenv:Body></soapenv:Envelope>", MessageID, Action, Url, Nonce, PasswordDigest, Timestamp, OID, XmlData);
        }

        /// <summary>
        /// Soap Form 생성(토파스용)
        /// </summary>
        /// <param name="XmlData">XML 데이타</param>
        /// <returns></returns>
        public static string SoapHeaderForTopas(string XmlData)
        {
            return String.Format("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:top=\"http://TOPAS_GPS_Service_Library\"><soapenv:Header/><soapenv:Body>{0}</soapenv:Body></soapenv:Envelope>", XmlData);
        }

		/// <summary>
        /// SOAP 방식을 이용하여 데이타 전송 후 결과값 리턴(아마데우스용)
        /// </summary>
        /// <param name="Url">End Point</param>
        /// <param name="Action">Action URL</param>
        /// <param name="ServiceName">서비스명</param>
        /// <param name="XmlData">XML 데이타</param>
        /// <param name="GUID">고유번호</param>
		/// <returns></returns>
        public static XmlElement AmadeusSoapSend(string Url, string Action, string ServiceName, string XmlData, string GUID)
		{
            Common cm = new Common();
            XmlDocument ReqXml = new XmlDocument();
            ReqXml.LoadXml(XmlData);
            cm.XmlFileSave(ReqXml, "Amadeus", String.Format("{0}RQ-Soap", ServiceName), "N", GUID);
            
            XmlDocument XmlDoc = new XmlDocument();
            
            try
            {
                XmlDocument ResXml = new XmlDocument();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                //if (HttpContext.Current != null)
                //    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Headers.Add("SOAPAction", Action);
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.SendChunked = true;
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";

                using (StreamWriter reqStream = new StreamWriter(req.GetRequestStream(), Encoding.GetEncoding("UTF-8")))
                {
                    reqStream.Write(XmlData);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
                    {
                        ResXml.LoadXml(resStream.ReadToEnd());
                        //ResXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "SoapSend", "SendToXml_Success", GUID));
                    }
                }

                XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.NameTable);
                xnMgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

                XmlDoc.LoadXml(ResXml.SelectSingleNode("soap:Envelope/soap:Body", xnMgr).FirstChild.OuterXml);
            }
            catch (WebException ex)
            {
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        XmlDoc.LoadXml(resStream.ReadToEnd());
                        //XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "SoapSend", "SendToXml_Error", GUID));
                    }
                }
            }
            catch (Exception ex)
            {
                XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", ex.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                //XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", cm.StringFileSave(ex.ToString(), "SoapSend", "SendToXml_Exception", GUID), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            return XmlDoc.DocumentElement;
		}

        /// <summary>
        /// SOAP 방식을 이용하여 데이타 전송 후 결과값 리턴(아마데우스용)
        /// </summary>
        /// <param name="Url">End Point</param>
        /// <param name="Action">Action URL</param>
        /// <param name="ServiceName">서비스명</param>
        /// <param name="XmlData">XML 데이타</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public static XmlElement AmadeusSoapSendHeader4(string Url, string Action, string ServiceName, string XmlData, string GUID)
        {
            Common cm = new Common();
            XmlDocument ReqXml = new XmlDocument();
            ReqXml.LoadXml(XmlData);
            cm.XmlFileSave(ReqXml, "Amadeus", String.Format("{0}RQ-Soap", ServiceName), "N", GUID);
            
            XmlDocument XmlDoc = new XmlDocument();

            try
            {
                XmlDocument ResXml = new XmlDocument();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(Url));
                //if (HttpContext.Current != null)
                //    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Headers.Add("SOAPAction", Action);
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.SendChunked = true;
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";

                using (StreamWriter reqStream = new StreamWriter(req.GetRequestStream(), Encoding.GetEncoding("UTF-8")))
                {
                    reqStream.Write(XmlData);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
                    {
                        ResXml.LoadXml(resStream.ReadToEnd());

                        //Push Data 저장
                        //ResXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "SoapSendHeader4", String.Format("{0}_Success", Action), GUID));
                    }
                }

                XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.NameTable);
                xnMgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                xnMgr.AddNamespace("awsse", "http://xml.amadeus.com/2010/06/Session_v3");

                XmlDoc.LoadXml(ResXml.SelectSingleNode("soapenv:Envelope/soapenv:Body", xnMgr).FirstChild.OuterXml);

                //세션정보 추가
                XmlElement Session = XmlDoc.CreateElement("session");
                XmlElement SessionId = XmlDoc.CreateElement("sessionId");
                XmlElement SequenceNumber = XmlDoc.CreateElement("sequenceNumber");
                XmlElement SecurityToken = XmlDoc.CreateElement("securityToken");

                SessionId.InnerText = ResXml.SelectSingleNode("soapenv:Envelope/soapenv:Header/awsse:Session/awsse:SessionId", xnMgr).InnerText;
                SequenceNumber.InnerText = ResXml.SelectSingleNode("soapenv:Envelope/soapenv:Header/awsse:Session/awsse:SequenceNumber", xnMgr).InnerText;
                SecurityToken.InnerText = ResXml.SelectSingleNode("soapenv:Envelope/soapenv:Header/awsse:Session/awsse:SecurityToken", xnMgr).InnerText;

                Session.AppendChild(SessionId);
                Session.AppendChild(SequenceNumber);
                Session.AppendChild(SecurityToken);

                XmlDoc.FirstChild.AppendChild(Session);
            }
            catch (WebException ex)
            {
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        //XmlDoc.LoadXml(resStream.ReadToEnd());

                        //Push Data 저장
                        XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "SoapSendHeader4", String.Format("{0}_Error", Action), GUID));
                    }
                }
            }
            catch (Exception ex)
            {
                //XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", ex.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", cm.StringFileSave(ex.ToString(), "SoapSendHeader4", String.Format("{0}_Exception", Action), GUID), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 세이버 세션 생성용
        /// </summary>
        /// <param name="Url">End Point</param>
        /// <param name="ServiceName">서비스명</param>
        /// <param name="XmlData">XML 데이타</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public static XmlElement SabreSessionCreate(string Url, string ServiceName, string XmlData, string GUID)
        {
            XmlDocument XmlDoc = new XmlDocument();
            Common cm = new Common();

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Headers.Add("SOAPAction", "SessionCreateRQ");
                //req.Headers.Add("Accept-Encoding", "gzip, deflate");
                //req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                //req.SendChunked = true;
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";

                using (StreamWriter reqStream = new StreamWriter(req.GetRequestStream(), Encoding.GetEncoding("UTF-8")))
                {
                    reqStream.Write(XmlData);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
                    {
                        XmlDoc.LoadXml(resStream.ReadToEnd());
                        //XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "XmlRequest", "SabreSessionCreate_Success", ""));
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        XmlDoc.LoadXml(resStream.ReadToEnd());
                        //XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "XmlRequest", "SabreSessionCreate_WebException", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", ex.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                //XmlDoc.LoadXml(cm.StringFileSave(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", ex.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), "XmlRequest", "SabreSessionCreate_Exception", ""));
            }

            return XmlDoc.DocumentElement;
        }

        #region "TEST%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"

        public static XmlElement SoapSendMPISTEST(string Url, string Action, string XmlData, string GUID)
        {
            string TimeCheck = string.Empty;
            System.Diagnostics.Stopwatch sw;
            sw = System.Diagnostics.Stopwatch.StartNew();

            string ResString = string.Empty;
            XmlDocument ResXml = new XmlDocument();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(Url));
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
            req.Headers.Add("SOAPAction", Action);
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.SendChunked = true;
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";

            byte[] bytes = Encoding.UTF8.GetBytes(XmlData);
            req.ContentLength = XmlData.Length;

            //req.ReadWriteTimeout = 5000;
            req.ProtocolVersion = HttpVersion.Version11;

            //***
            TimeCheck += String.Format("{0} : {1:#00}.{2:00} / ", "1단계", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
            sw = System.Diagnostics.Stopwatch.StartNew();

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }

            //***
            TimeCheck += String.Format("{0} : {1:#00}.{2:00} / ", "2단계", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
            sw = System.Diagnostics.Stopwatch.StartNew();

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
                {
                    //ResXml.LoadXml(resStream.ReadToEnd());
                    ResString = resStream.ReadToEnd();
                }
            }

            //***
            TimeCheck += String.Format("{0} : {1:#00}.{2:00} / ", "3단계", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
            sw = System.Diagnostics.Stopwatch.StartNew();

            ResXml.LoadXml(ResString);

            //***
            TimeCheck += String.Format("{0} : {1:#00}.{2:00} / ", "4단계", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
            sw = System.Diagnostics.Stopwatch.StartNew();

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.NameTable);
            xnMgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(ResXml.SelectSingleNode("soapenv:Envelope/soapenv:Body", xnMgr).FirstChild.OuterXml);

            //***
            TimeCheck += String.Format("{0} : {1:#00}.{2:00} / ", "5단계", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
            sw = System.Diagnostics.Stopwatch.StartNew();

            XmlNode TC = XmlDoc.CreateElement("TimeCheck");
            TC.InnerXml = TimeCheck;
            XmlDoc.FirstChild.InsertBefore(TC, XmlDoc.FirstChild.FirstChild);

            return XmlDoc.DocumentElement;
        }

        #endregion "TEST%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"

        /// <summary>
		/// HttpWebReqeust GET 방식을 이용하여 데이타 전송 후 결과값 리턴
		/// </summary>
		/// <param name="URL">호출URL(파라미터 포함)</param>
		/// <returns>XmlElement 형식의 결과 Data</returns>
		public static XmlElement GetSend(string URL)
		{
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
			req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
			{
				XmlDocument resXml = new XmlDocument();
				resXml.LoadXml(resStream.ReadToEnd());
				resStream.Close();

				return resXml.DocumentElement;
			}
		}

        /// <summary>
        /// HttpWebReqeust POST 방식을 이용하여 데이타 전송 후 결과값 리턴
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XmlElement 형식의 결과 Data</returns>
        public static XmlElement GetPostSendToXml(string URL, string Data)
        {
            XmlDocument resXml = new XmlDocument();
            Common cm = new Common();

            try
            {
                cm.StringFileSave(URL, "XmlRequest", "GetPostSendToXml_URL", "");
                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                if (HttpContext.Current != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                //req.SendChunked = true;

                byte[] bytes = Encoding.UTF8.GetBytes(Data);
                req.ContentLength = bytes.Length;
                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    resXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "XmlRequest", "GetPostSendToXml_Success", ""));
                    resStream.Close();
                }
            }
            catch (WebException ex)
            {
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        resXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "XmlRequest", "GetPostSendToXml_Error", ""));
                        resStream.Close();
                    }
                }
            }

            return resXml.DocumentElement;
        }

        /// <summary>
        /// HttpWebReqeust POST 방식을 이용하여 데이타 전송 후 결과값 리턴
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XmlElement 형식의 결과 Data</returns>
        public static XmlElement GetPostSendToXml2(string URL, string Data)
        {
            XmlDocument resXml = new XmlDocument();

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                if (HttpContext.Current != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                //req.SendChunked = true;

                byte[] bytes = Encoding.UTF8.GetBytes(Data);
                req.ContentLength = bytes.Length;
                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    Common cm = new Common();
                    resXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "Sabre", "AirBook_Success", ""));
                    resStream.Close();
                }
            }
            catch (WebException ex)
            {
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        Common cm = new Common();
                        resXml.LoadXml(cm.StringFileSave(resStream.ReadToEnd().Trim(), "Sabre", "AirBook_Error", ""));
                        resStream.Close();
                    }
                }
            }

            return resXml.DocumentElement;
        }

		/// <summary>
		/// HttpWebReqeust POST 방식을 이용하여 데이타 전송 후 결과값 리턴
		/// </summary>
		/// <param name="URL">호출URL</param>
		/// <param name="Data">전송데이타</param>
		/// <returns>String 형식의 결과 Data</returns>
		public static string GetPostSendToString(string URL, string Data)
		{
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
			req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

            byte[] bytes = Encoding.UTF8.GetBytes(Data);
			req.ContentLength = bytes.Length;
			using (Stream stream = req.GetRequestStream())
			{
				stream.Write(bytes, 0, bytes.Length);
			}

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
			{
				return resStream.ReadToEnd();
			}
		}

		/// <summary>
		/// HttpWebReqeust GET 방식을 이용하여 JSON 데이타 전송 후 결과값 리턴
		/// </summary>
		/// <param name="URL">호출URL(파라미터 포함)</param>
		/// <returns>JSON 형식의 결과 Data</returns>
		public static string GetSendToJSon(string URL)
		{
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
			req.Method = "GET";
            req.ContentType = "application/json; charset=utf-8";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
			{
				return resStream.ReadToEnd();
			}
		}

        /// <summary>
        /// 세이버 정보 송/수신용(XML)
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static XmlElement SabreSendToXml(string URL, string Method, string Data)
        {
            XmlDocument XmlDoc = new XmlDocument();
            Common cm = new Common();

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                if (HttpContext.Current != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = false;
                req.Method = Method;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Accept = "application/xml;charset=utf-8";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                //req.SendChunked = true;

                if (Method != "GET")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Data);
                    req.ContentLength = bytes.Length;
                    using (Stream stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    //XmlDoc.LoadXml(resStream.ReadToEnd());

                    //Push Data 저장
                    XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "Sabre", "SabreSendToXml_Success", ""));
                }
            }
            catch (WebException ex)
            {
                cm.StringFileSave(ex.ToString(), "Sabre", "SabreSendToXml_WebException", "");

                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        //XmlDoc.LoadXml(resStream.ReadToEnd());

                        //Push Data 저장
                        XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "Sabre", "SabreSendToXml_Error", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                XmlDoc.LoadXml(String.Format("<WSErrors><WSError><ExceptionInfo><ToString><![CDATA[{0}]]></ToString></ExceptionInfo></WSError></WSErrors>", cm.StringFileSave(ex.ToString(), "Sabre", "SabreSendToXml_Exception", "")));
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 토파스 정보 송/수신용(Json)
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <param name="ItemGubun">품목코드</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static string TopasSendToJson(string URL, string Method, string Data, string ItemGubun)
        {
            Common cm = new Common();
            string ReturnJson = string.Empty;

            try
            {
                //RQ저장
                cm.StringFileSave(String.Format("URL: {0}{3}METHOD: {1}{3}DATA: {2}", URL, Method, Data, Environment.NewLine), "Topas", "TopasSendToJson_SourceData", "");

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = false;
                req.Timeout = 20000;
                req.ReadWriteTimeout = 20000;
                req.Method = Method;
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.Headers.Add("x-api-key", "TW9kZVRvdXItU0M5");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                if (Method != "GET" && !String.IsNullOrWhiteSpace(Data))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Data);
                    req.ContentLength = bytes.Length;
                    using (Stream stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    //Push Data 저장
                    ReturnJson = cm.StringFileSave(resStream.ReadToEnd(), "Topas", "TopasSendToJson_Success", "");
                }
            }
            //catch (WebException wex)
            //{
            //    try
            //    {
            //        using (WebResponse res = wex.Response)
            //        {
            //            HttpWebResponse hwres = (HttpWebResponse)res;
            //            using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
            //            {
            //                //Push Data 저장
            //                ReturnJson = cm.StringFileSave(resStream.ReadToEnd(), "Topas", "TopasSendToJson_Error", "");
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ReturnJson = String.Format("{\"WSErrors\":{\"WSError\":{\"ExceptionInfo\":{\"ToString\":\"{0}\"}}}}", cm.StringFileSave(ex.ToString(), "Topas", "TopasSendToJson_WebException", ""));
            //    }
            //}
            catch (Exception ex)
            {
                ReturnJson = String.Format("{\"WSErrors\":{\"WSError\":{\"ExceptionInfo\":{\"ToString\":\"{0}\"}}}}", cm.StringFileSave(ex.ToString(), "Topas", "TopasSendToJson_Exception", ""));
            }

            return ReturnJson;
        }

        /// <summary>
        /// 트래블하우 예약 정보 송/수신용
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="SendType">요청 데이타 타입(json or xml)</param>
        /// <param name="ReceiveType">응답 데이타 타입(json or xml)</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>JSON 형식의 결과 Data</returns>
        public static string TravelHowSendAndReceive(string URL, string Method, string SendType, string ReceiveType, string Data)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
            req.Method = Method;
            req.ContentType = String.Concat("application/", SendType.ToLower());
            req.Accept = String.Concat("application/", ReceiveType.ToLower());
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            req.ContentLength = bytes.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
            {
                return resStream.ReadToEnd();
            }
        }

        /// <summary>
        /// 트래블하우 예약 정보 송/수신용
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static XmlElement TravelHowSendToJson(string URL, string Method, string Data)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
            req.Method = Method;
            req.ContentType = "application/json";
            req.Accept = "application/xml";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            req.ContentLength = bytes.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            XmlDocument XmlDoc = new XmlDocument();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
            {
                XmlDoc.LoadXml(resStream.ReadToEnd());
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 트래블하우 예약 정보 송/수신용
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static XmlElement TravelHowSendToXml(string URL, string Method, string Data)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
            if (HttpContext.Current != null)
                req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            req.KeepAlive = true;
            req.Method = Method;
            req.ContentType = "application/xml";
            req.Accept = "application/xml";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.SendChunked = true;

            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            req.ContentLength = bytes.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            XmlDocument XmlDoc = new XmlDocument();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
            {
                XmlDoc.LoadXml(resStream.ReadToEnd());
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// JSON 형식으로 송/수신용
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static string SendJsonToJson(string URL, string Method, string Data)
        {
            Common cm = new Common();

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                if (HttpContext.Current != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Method = Method;
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                if (Method != "GET")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Data);
                    req.ContentLength = bytes.Length;
                    using (Stream stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    return cm.StringFileSave(resStream.ReadToEnd(), "XmlRequest", "SendToJson_Success", "");
                }
            }
            catch (WebException ex)
            {
                cm.StringFileSave(ex.ToString(), "XmlRequest", "SendToJson_WebException", "");

                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        return cm.StringFileSave(resStream.ReadToEnd(), "XmlRequest", "SendToJson_Error", "");
                    }
                }
            }
            catch (Exception ex)
            {
                return String.Format("{\"WSErrors\":{\"WSError\":{\"ExceptionInfo\":{\"ToString\":\"{0}\"}}}}", cm.StringFileSave(ex.ToString(), "XmlRequest", "SendToJson_Exception", ""));
            }
        }

        /// <summary>
        /// JSON 형식으로 송/수신용
        /// </summary>
        /// <param name="URL">호출URL</param>
        /// <param name="Method">전송방식</param>
        /// <param name="Data">전송데이타</param>
        /// <returns>XML 형식의 결과 Data</returns>
        public static XmlElement SendJsonToXml(string URL, string Method, string Data)
        {
            XmlDocument XmlDoc = new XmlDocument();
            Common cm = new Common();

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                if (HttpContext.Current != null)
                    req.UserAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                req.KeepAlive = true;
                req.Method = Method;
                req.ContentType = "application/json";
                req.Accept = "application/xml";
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                if (Method != "GET")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Data);
                    req.ContentLength = bytes.Length;
                    using (Stream stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    //XmlDoc.LoadXml(resStream.ReadToEnd());

                    //Push Data 저장
                    XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "XmlRequest", "SendJsonToXml_Success", ""));
                }
            }
            catch (WebException ex)
            {
                cm.StringFileSave(ex.ToString(), "XmlRequest", "SendJsonToXml_WebException", "");

                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse hwres = (HttpWebResponse)res;
                    using (StreamReader resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        //XmlDoc.LoadXml(resStream.ReadToEnd());

                        //Push Data 저장
                        XmlDoc.LoadXml(cm.StringFileSave(resStream.ReadToEnd(), "XmlRequest", "SendJsonToXml_Error", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                XmlDoc.LoadXml(String.Format("<WSErrors><WSError><ExceptionInfo><ToString><![CDATA[{0}]]></ToString></ExceptionInfo></WSError></WSErrors>", cm.StringFileSave(ex.ToString(), "XmlRequest", "SendJsonToXml_Exception", "")));
            }

            return XmlDoc.DocumentElement;
        }
	}
}