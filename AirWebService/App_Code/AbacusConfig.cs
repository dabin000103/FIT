using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace AirWebService
{
	public class AbacusConfig : AbacusWebService
	{
		AirConfig ac = new AirConfig();

		private static string mUserName = "7288";
		private static string mPassword = "mtour11";
		private static string mPCC = "K7M8";
        private static string mPrinterAddress = "031AE4";
		private static string mClientUrl = "SWSSession.abacus.com.sg";

		private string mConversionID = "";
		private string mSecurityToken = "";

		/// <summary>
		/// GDS명
		/// </summary>
		/// <returns></returns>
		public string Name
		{
			get { return "Abacus"; }
		}
        
        /// <summary>
        /// 프린터 번호
        /// </summary>
        /// <returns></returns>
        public string PrinterAddress
        {
            get { return mPrinterAddress; }
        }

		/// <summary>
		/// Abacus용 XML 파일의 로컬 폴더 경로
		/// </summary>
		private string XmlPath
		{
			get { return String.Format(@"{0}Abacus\", ac.XmlPhysicalPath); }
		}

		/// <summary>
		/// Abacus용 XML 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string XmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", XmlPath, ServiceName);
		}

		/// <summary>
		/// 네임스페이스
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public static string NamespaceURL(string ServiceName)
		{
            string Namespace = "http://webservices.sabre.com/sabreXML/2003/07";

            switch (ServiceName)
            {
                case "AirTicketLLS":
                case "OTA_AirBookRQ":
                case "OTA_AirPriceLLS":
                case "DesignatePrinterLLS": Namespace = "http://webservices.sabre.com/sabreXML/2011/10"; break;
                case "TravelItineraryRead_stl": Namespace = "http://services.sabre.com/STL/v01"; break;
                case "TravelItineraryRead_tir38": Namespace = "http://services.sabre.com/res/tir/v3_8"; break;
                case "TravelItineraryRead_tir310": Namespace = "http://services.sabre.com/res/tir/v3_10"; break;
                case "TravelItineraryRead_or16": Namespace = "http://services.sabre.com/res/or/v1_6"; break;
                case "TravelItineraryRead_or8": Namespace = "http://services.sabre.com/res/or/v1_8"; break;
                case "GetReservation_stl19": Namespace = "http://webservices.sabre.com/pnrbuilder/v1_19"; break;
                case "GetReservation_or114": Namespace = "http://services.sabre.com/res/or/v1_14"; break;
            }

            return Namespace;
		}

		/// <summary>
		/// Abacus 호출 서비스 URL
		/// </summary>
		/// <returns></returns>
		public static string ServiceURL()
		{
            return "https://webservices.havail.sabre.com/websvc";
            //return "https://webservices.sabre.com/websvc";
            //return "https://sws-tls.cert.sabre.com";
		}

		/// <summary>
		/// ConversionID
		/// </summary>
		public string ConversionID
		{
			set { mConversionID = value; }
			get { return mConversionID; }
		}

		/// <summary>
		/// SecurityToken
		/// </summary>
		public string SecurityToken
		{
			set { mSecurityToken = value; }
			get { return mSecurityToken; }
		}

		private string CreateMID()
		{
			return (String.Format("mid:{0}@{1}", Guid.NewGuid(), mClientUrl));
		}

		private string CreateTimeStamp()
		{
			return DateTime.Now.ToString("s", DateTimeFormatInfo.InvariantInfo);
		}

		private string CreateConversionID()
		{
			return (String.Format("{0}@{1}", Guid.NewGuid(), mClientUrl));
		}

		private string UnicodeToSWSEncoding(string ReqXml)
		{
			StringBuilder sb = new StringBuilder(ReqXml);

			for (int i = 0; i < sb.Length; i++)
			{
				if (sb[i] == '\u2021')
					sb[i] = '\u00E7';
			}

			return sb.ToString();
		}

		/// <summary>
		/// Abacus 세션 생성
		/// </summary>
		/// <returns></returns>
		public XmlElement SessionCreate()
		{
            string StrXML = String.Concat("<soap-env:Envelope xmlns:soap-env=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:eb=\"http://www.ebxml.org/namespaces/messageHeader\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:xsd=\"http://www.w3.org/1999/XMLSchema\">",
                                            "<soap-env:Header>",
                                                "<eb:MessageHeader soap-env:mustUnderstand=\"1\" eb:version=\"1.0\">",
                                                    "<eb:From><eb:PartyId /></eb:From>",
                                                    "<eb:To><eb:PartyId /></eb:To>",
                                                    "<eb:CPAId>" + mPCC + "</eb:CPAId>",
                                                    "<eb:ConversationId>" + CreateConversionID() + "</eb:ConversationId>",
                                                    "<eb:Service>SessionCreateRQ</eb:Service>",
                                                    "<eb:Action>SessionCreateRQ</eb:Action>",
                                                    "<eb:MessageData>",
                                                    "<eb:MessageId>" + CreateMID() + "</eb:MessageId>",
                                                    "<eb:Timestamp>" + CreateTimeStamp() + "</eb:Timestamp>",
                                                    "</eb:MessageData>",
                                                "</eb:MessageHeader>",
                                                "<wsse:Security xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/12/secext\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/12/utility\">",
                                                    "<wsse:UsernameToken>",
                                                    "<wsse:Username>" + mUserName + "</wsse:Username>",
                                                    "<wsse:Password>" + mPassword + "</wsse:Password>",
                                                    "<Organization>" + mPCC + "</Organization>",
                                                    "<Domain>Default</Domain>",
                                                    "</wsse:UsernameToken>",
                                                "</wsse:Security>",
                                            "</soap-env:Header>",
                                            "<soap-env:Body>",
                                                "<eb:Manifest soap-env:mustUnderstand=\"1\" eb:version=\"1.0\">",
                                                    "<eb:Reference xlink:href=\"cid:rootelement\" xlink:type=\"simple\" />",
                                                "</eb:Manifest>",
                                                "<SessionCreateRQ>",
                                                    "<POS><Source PseudoCityCode=\"" + mPCC + "\" /></POS>",
                                                "</SessionCreateRQ>",
                                                "<ns:SessionCreateRQ xmlns:ns=\"http://www.opentravel.org/OTA/2002/11\" />",
                                            "</soap-env:Body>",
                                        "</soap-env:Envelope>");

            XmlElement ResXml = XmlRequest.SabreSessionCreate(ServiceURL(), "SessionCreate", StrXML, new Common().GetGUID);

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
            xnMgr.AddNamespace("soap-env", "http://schemas.xmlsoap.org/soap/envelope/");
            xnMgr.AddNamespace("wsse", "http://schemas.xmlsoap.org/ws/2002/12/secext");

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(ResXml.SelectSingleNode("soap-env:Body", xnMgr).FirstChild.OuterXml);

            //Response Xml에 노드 추가
            XmlNode NewNode = XmlDoc.CreateElement("SecurityToken");
            NewNode.InnerText = ResXml.SelectSingleNode("soap-env:Header/wsse:Security/wsse:BinarySecurityToken", xnMgr).InnerText;

            XmlDoc.ChildNodes[0].AppendChild(NewNode);

            return XmlDoc.DocumentElement;
		}

        public XmlElement SessionCreate_OLD()
        {
            MessageHeader MessageHeaderData = new MessageHeader();
            From FromData = new From();
            PartyId[] FromPartyIdArr = new PartyId[1];
            PartyId FromPartyId = new PartyId { Value = "com.abacus.SWSSession", type = "urn:x12.org.IO5:01" };

            FromPartyIdArr[0] = FromPartyId;
            FromData.PartyId = FromPartyIdArr;
            MessageHeaderData.From = FromData;

            To ToData = new To();
            PartyId[] ToPartyIdArr = new PartyId[1];
            PartyId ToPartyId = new PartyId { Value = "webservices.sabre.com", type = "urn:x12.org.IO5:01" };

            ToPartyIdArr[0] = ToPartyId;
            ToData.PartyId = ToPartyIdArr;
            MessageHeaderData.To = ToData;

            Service ServiceData = new Service { type = "SabreXML", Value = "Session" };
            MessageHeaderData.Service = ServiceData;
            MessageHeaderData.Action = "SessionCreateRQ";

            MessageData MsgData = new MessageData { MessageID = CreateMID(), TimeStamp = CreateTimeStamp() };
            MessageHeaderData.MessageData = MsgData;

            Security SecurityData = new Security();
            SecurityUserNameToken SecurityUserNameTokenData = new SecurityUserNameToken { Domain = "Default", UserName = mUserName, Password = mPassword, Organization = mPCC };
            SecurityData.UserNameToken = SecurityUserNameTokenData;

            MessageHeaderData.ConversationID = CreateConversionID();
            ConversionID = MessageHeaderData.ConversationID;
            MessageHeaderValue = MessageHeaderData;
            SecurityValue = SecurityData;

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(XmlFullPath("SessionCreate"));
            XmlDoc.SelectSingleNode("SessionCreateRQ/POS/Source").Attributes.GetNamedItem("PseudoCityCode").InnerText = mPCC;

            XmlDoc.LoadXml(ServiceRQ(XmlDoc.DocumentElement).OuterXml);
            SecurityToken = SecurityValue.BinarySecurityToken;

            //Response Xml에 노드 추가
            XmlNode NewNode = XmlDoc.CreateElement("SecurityToken");
            NewNode.InnerText = SecurityToken;

            XmlDoc.ChildNodes[0].AppendChild(NewNode);

            return XmlDoc.DocumentElement;
        }

		/// <summary>
		/// Abacus Request and Response
		/// </summary>
		/// <param name="ReqXml"></param>
		/// <param name="Action"></param>
		/// <param name="ConversionID"></param>
		/// <param name="SecurityToken"></param>
		/// <returns></returns>
		public XmlElement Execute(string ReqXml, string Action, string ConversionID, string SecurityToken)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.LoadXml(UnicodeToSWSEncoding(ReqXml));

			if (XmlDoc.GetElementsByTagName("Source").Count > 0)
				XmlDoc.GetElementsByTagName("Source")[0].Attributes.GetNamedItem("PseudoCityCode").InnerText = mPCC;

			XmlDocument SwsDoc = new XmlDocument();
			SwsDoc.Load(XmlFullPath("SWSActions"));

			XmlNode SwsNode = SwsDoc.SelectSingleNode(String.Format("SWSServices/service[@node='{0}']", Action));

			return Execute(XmlDoc.DocumentElement, SwsNode.Attributes.GetNamedItem("type").InnerText, SwsNode.Attributes.GetNamedItem("value").InnerText, SwsNode.Attributes.GetNamedItem("action").InnerText, ConversionID, SecurityToken);
		}

		/// <summary>
		/// Abacus Request and Response
		/// </summary>
		/// <param name="ReqXml"></param>
		/// <param name="Action"></param>
		/// <param name="ConversionID"></param>
		/// <param name="SecurityToken"></param>
		/// <returns></returns>
		public XmlElement Execute(XmlElement ReqXml, string Action, string ConversionID, string SecurityToken)
		{
			return Execute(ReqXml.OwnerDocument.OuterXml, Action, ConversionID, SecurityToken);
		}

		/// <summary>
		/// Abacus Request and Response
		/// </summary>
		/// <param name="XmlDoc"></param>
		/// <param name="ServiceType"></param>
		/// <param name="ServiceValue"></param>
		/// <param name="Action"></param>
		/// <param name="ConversionID"></param>
		/// <param name="SecurityToken"></param>
		/// <returns></returns>
		public XmlElement Execute(XmlElement XmlDoc, string ServiceType, string ServiceValue, string Action, string ConversionID, string SecurityToken)
		{
			MessageHeader MessageHeaderData = new MessageHeader();
			From FromData = new From();
			PartyId[] FromPartyIdArr = new PartyId[1];
			PartyId FromPartyId = new PartyId { Value = "com.abacus.SWSSession", type = "urn:x12.org.IO5:01" };

			FromPartyIdArr[0] = FromPartyId;
			FromData.PartyId = FromPartyIdArr;
			MessageHeaderData.From = FromData;

			To ToData = new To();
			PartyId[] ToPartyIdArr = new PartyId[1];
			PartyId ToPartyId = new PartyId { Value = "webservices.sabre.com", type = "urn:x12.org.IO5:01" };

			ToPartyIdArr[0] = ToPartyId;
			ToData.PartyId = ToPartyIdArr;
			MessageHeaderData.To = ToData;

			Service ServiceData = new Service { type = ServiceType, Value = ServiceValue };
			MessageHeaderData.Service = ServiceData;
			MessageHeaderData.Action = Action;
			MessageHeaderData.ConversationID = ConversionID;

			MessageData MsgData = new MessageData { MessageID = CreateMID(), TimeStamp = CreateTimeStamp() };
			MessageHeaderData.MessageData = MsgData;

			Security SecurityData = new Security { BinarySecurityToken = SecurityToken };

			MessageHeaderValue = MessageHeaderData;
			SecurityValue = SecurityData;

			return ServiceRQ(XmlDoc);
		}
	}
}