using System;
using System.Xml;
using System.Text;

namespace AirWebService
{
	public class AmadeusConfig
	{
		AirConfig ac = new AirConfig();

		private string mSessionId = "";
		private string mSequenceNumber = "";
		private string mSecurityToken = "";
        private static string[] mOfficeId = new String[16] { "SELK138AB", "SELKP3300", "SELK138HB", "SELK138HM", "SELK138HS", "SELK138IB", "SELK138JR", "SELK138KD", "BKKOK27PO", "SELK138KZ", "SELK138KW", "SELK138LO", "SELK138QY", "TYOJA28EJ", "SELK138TG", "SELK138NQ" }; //온라인용, 오프라인용, CTrip, 취날, 모두닷컴 모바일웹/앱용, 트래블하우, 세계99, 투니우, 태국GM투어(임시), PK투어, FR24, 11번가, 화메이다, F-NESS, 삼성카드(항공)/삼성카드(항공_복지몰), 스카이스캐너

		/// <summary>
		/// GDS명
		/// </summary>
		/// <returns></returns>
		public string Name
		{
			get { return "Amadeus"; }
		}

		/// <summary>
		/// Amadeus용 XML 파일의 로컬 폴더 경로
		/// </summary>
		private string XmlPath
		{
			get { return String.Format(@"{0}Amadeus\", ac.XmlPhysicalPath); }
		}

		/// <summary>
		/// Amadeus용 XML 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string XmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", XmlPath, ServiceName);
		}

		/// <summary>
		/// Amadeus용 OfficeID
		/// </summary>
        /// <param name="SNM">사이트번호(67:오프라인용, 70:CTrip, 74:취날, 3915:모두닷컴 모바일웹/앱, 75:트래블하우, 86:세계99, 28:투니우, 10:태국GM투어(임시), 58:PK투어, 63:FR24, 4924/4929:11번가, 106:화메이다, 11:F-NESS(임시), 4578/4737:삼성카드(항공), 4547:삼성카드(항공_복지몰), 4837/4664:스카이스캐너, 그외:온라인용)</param>
		/// <returns></returns>
		public static string OfficeId(int SNM)
		{
            string OID = string.Empty;

            switch (SNM)
            {
                case 67: OID = mOfficeId[1]; break;
                case 70: OID = mOfficeId[2]; break;
                case 74: OID = mOfficeId[3]; break;
                case 3915: OID = mOfficeId[4]; break;
                case 75: OID = mOfficeId[5]; break;
                case 86: OID = mOfficeId[6]; break;
                case 28: OID = mOfficeId[7]; break;
                case 10: OID = mOfficeId[8]; break;
                case 58: OID = mOfficeId[9]; break;
                case 63: OID = mOfficeId[10]; break;
                case 4924:
                case 4929: OID = mOfficeId[11]; break;
                case 106: OID = mOfficeId[12]; break;
                case 11: OID = mOfficeId[13]; break;
                case 4578:
                case 4737:
                case 4547: OID = mOfficeId[14]; break;
                case 4837:
                case 4664: OID = mOfficeId[15]; break;
                default: OID = mOfficeId[0]; break;
            }

            return OID;
		}

		/// <summary>
		/// UMS 사용자 인증키
		/// </summary>
		/// <returns></returns>
		public static string ItrKey()
		{
            return ItrKey(2);
		}

        /// <summary>
        /// UMS 사용자 인증키
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <returns></returns>
        public static string ItrKey(int SNM)
        {
            UTF8Encoding UTF8Enc = new UTF8Encoding();
            byte[] byteStr = UTF8Enc.GetBytes(String.Format("OfficeId={0}&UserId=WSMOTIBE&UserPw=WSMOTIBE", OfficeId(SNM)));

            return Convert.ToBase64String(byteStr);
        }

		/// <summary>
		/// 네임스페이스
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public static string NamespaceURL(string ServiceName)
		{
			string Namespace = string.Empty;

			switch (ServiceName)
			{
				case "Air_FlightInfo":
					Namespace = "http://xml.amadeus.com/FLIRES_07_1_1A";
					break;
				case "Air_MultiAvailability":
					Namespace = "http://xml.amadeus.com/SATRSP_07_1_1A";
					break;
				case "Air_SellFromRecommendation":
					Namespace = "http://xml.amadeus.com/ITARES_05_2_IA";
					break;
				//case "Air_RetrieveSeatMap":
				//    Namespace = "http://xml.amadeus.com/SMPRES_97_1_IA";
				//    break;
				case "Fare_CheckRules":
					Namespace = "http://xml.amadeus.com/FARQNR_07_1_1A";
                    break;
                case "Fare_InstantTravelBoardSearch":
                    Namespace = "http://xml.amadeus.com/FIFRTR_16_2_1A";
                    break;
				case "Fare_MasterPricerTravelBoardSearch":
					Namespace = "http://xml.amadeus.com/FMPTBR_13_3_1A";
					break;
				case "Fare_MasterPricerCalendar":
					Namespace = "http://xml.amadeus.com/FMPCAR_12_2_1A";
					break;
				case "Fare_SellByFareCalendar":
					Namespace = "http://xml.amadeus.com/FSOCAR_12_2_1A";
					break;
				case "Fare_SellByFareSearch":
					Namespace = "http://xml.amadeus.com/FSOTBR_12_2_1A";
					break;
				case "Fare_GetRulesOfPricedItinerary":
					Namespace = "http://xml.amadeus.com/TFRUDR_12_1_1A";
					break;
				case "Fare_InformativeBestPricingWithoutPNR":
					Namespace = "http://xml.amadeus.com/TIBNRR_12_4_1A"; //http://xml.amadeus.com/TIBNRR_13_2_1A
					break;
				case "Fare_InformativePricingWithoutPNR":
					Namespace = "http://xml.amadeus.com/TIPNRR_13_2_1A";
					break;
				case "Fare_InformativePricingWithoutPNRRule":
					Namespace = "http://xml.amadeus.com/TIPNRR_12_4_1A";
					break;
				case "Fare_PricePNRWithBookingClass":
					Namespace = "http://xml.amadeus.com/TPCBRR_13_2_1A";
					break;
				case "Fare_PricePNRWithBookingClassPricing":
					Namespace = "http://xml.amadeus.com/TPCBRR_12_4_1A";
                    break;
                case "Fare_PricePNRWithBookingClassKEPricing":
                    Namespace = "http://xml.amadeus.com/TPCBRR_16_1_1A";
                    break;
				case "DocIssuance_IssueMiscellaneousDocuments":
					Namespace = "http://xml.amadeus.com/TMDSIR_09_2_1A";
					break;
				case "DocIssuance_IssueTicket":
					Namespace = "http://xml.amadeus.com/TTKTIR_09_1_1A";
					break;
				case "PNR_AddMultiElements":
					Namespace = "http://xml.amadeus.com/PNRADD_12_2_1A";
					break;
				case "PNR_Reply":
					Namespace = "http://xml.amadeus.com/PNRACC_12_2_1A";
					break;
				case "PNR_Retrieve":
					Namespace = "http://xml.amadeus.com/PNRRET_12_2_1A";
					break;
				case "PNR_Cancel":
					Namespace = "http://xml.amadeus.com/PNRXCL_12_2_1A";
					break;
				case "PNR_List":
					Namespace = "http://xml.amadeus.com/TNLRES_00_1_1A";
					break;
				case "Ticket_ProcessEDoc":
					Namespace = "http://xml.amadeus.com/TATRES_09_1_1A";
					break;
				case "Ticket_CreateTSTFromPricing":
					Namespace = "http://xml.amadeus.com/TAUTCR_04_1_1A";
					break;
				case "Ticket_CreateTSMFareElement":
					Namespace = "http://xml.amadeus.com/TFRECR_10_1_1A";
					break;
				case "Ticket_CancelDocument":
					Namespace = "http://xml.amadeus.com/TRCANR_11_1_1A";
					break;
				case "Ticket_CreateTASF":
					Namespace = "http://xml.amadeus.com/TTASCR_12_1_1A";
					break;
				case "Ticket_DisplayQuota":
					Namespace = "http://xml.amadeus.com/TTQSDR_11_1_1A";
					break;
				//case "Ticket_DisplayTST":
				//	Namespace = "http://xml.amadeus.com/TTSTRR_12_1_1A";
				//	break;
				case "Ticket_DisplayTSMP":
					Namespace = "http://xml.amadeus.com/TTSMRR_13_1_1A";
					break;
				case "FOP_CreateFormOfPayment":
					Namespace = "http://xml.amadeus.com/TFOPCR_11_1_1A";
					break;
                case "Queue_CountTotal":
                    Namespace = "http://xml.amadeus.com/QCSDRR_03_1_1A";
                    break;
                case "Queue_List":
                    Namespace = "http://xml.amadeus.com/QDQLRR_11_1_1A";
                    break;
                case "Queue_RemoveItem":
                    Namespace = "http://xml.amadeus.com/QUQMDR_03_1_1A";
                    break;
                case "Queue_MoveItem":
                    Namespace = "http://xml.amadeus.com/QUQMUR_03_1_1A";
                    break;
                case "Queue_PlacePNR":
                    Namespace = "http://xml.amadeus.com/QUQPCR_03_1_1A";
                    break;
                case "MiniRule_GetFromPricing":
                    Namespace = "http://xml.amadeus.com/TMRCRR_11_1_1A";
                    break;
                case "Command_Cryptic":
					Namespace = "http://xml.amadeus.com/HSFRES_07_3_1A";
					break;
				case "Security_Authenticate":
					Namespace = "http://xml.amadeus.com/VLSSLR_06_1_1A";
					break;
				case "Security_SignOut":
					Namespace = "http://xml.amadeus.com/VLSSOR_04_1_1A";
					break;
			}

			return Namespace;
		}

		/// <summary>
		/// Soap Action
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public static string ActionURL(string ServiceName)
		{
			string Action = string.Empty;

			switch (ServiceName)
			{
                case "Fare_InstantTravelBoardSearch":
                    Action = "http://webservices.amadeus.com/FIFRTQ_16_2_1A";
					break;
                case "Fare_MasterPricerTravelBoardSearch":
                    Action = "http://webservices.amadeus.com/1ASIWIBEMOT/FMPTBQ_13_3_1A";
                    break;
                case "Fare_GetRulesOfPricedItinerary":
					Action = "http://webservices.amadeus.com/1ASIWIBEMOT/TFRUDQ_12_1_1A";
					break;
				case "PNR_AddMultiElements":
					Action = "http://webservices.amadeus.com/1ASIWIBEMOT/PNRADD_12_2_1A";
					break;
				case "PNR_Retrieve":
					Action = "http://webservices.amadeus.com/1ASIWIBEMOT/PNRRET_12_2_1A";
					break;
			}

			return Action;
		}

		/// <summary>
		/// Amadeus 호출 서비스 URL
		/// </summary>
		/// <returns></returns>
		public static string ServiceURL()
		{
			return "https://production.webservices.amadeus.com";
		}

		/// <summary>
		/// Authenticate SessionId
		/// </summary>
		public string SessionId
		{
			set { mSessionId = value; }
			get { return mSessionId; }
		}

		/// <summary>
		/// Authenticate SequenceNumber
		/// </summary>
		public string SequenceNumber
		{
			set { mSequenceNumber = value; }
			get { return mSequenceNumber; }
		}

		/// <summary>
		/// Authenticate SecurityToken
		/// </summary>
		public string SecurityToken
		{
			set { mSecurityToken = value; }
			get { return mSecurityToken; }
		}

		/// <summary>
		/// Amadeus 인증
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <returns></returns>
		public XmlElement Authenticate(int SNM)
		{
			AirWebService.Authenticate.AmadeusWebService aws = new AirWebService.Authenticate.AmadeusWebService();

			XmlDocument XmlReq = new XmlDocument();
			XmlReq.Load(XmlFullPath("AuthenticateRQ"));
			XmlReq.SelectSingleNode("Security_Authenticate/userIdentifier/originIdentification/sourceOffice").InnerText = OfficeId(SNM);

            //태국GM투어의 경우 organizationId 별도 사용(2016-10-07,토파스요청)
            if (SNM.Equals(10))
                XmlReq.SelectSingleNode("Security_Authenticate/systemDetails/organizationDetails/organizationId").InnerText = "NMC-THAILA";

			XmlDocument XmlRes = new XmlDocument();
			XmlRes.LoadXml(aws.ServiceRQ(XmlReq.DocumentElement).OuterXml);

			SessionId = aws.session.SessionId;
			SequenceNumber = aws.session.SequenceNumber;
			SecurityToken = aws.session.SecurityToken;

			//Response Xml에 노드 추가
			XmlNode SessionNode = XmlRes.CreateElement("session");
			XmlNode NewNode;

			NewNode = XmlRes.CreateElement("sessionId");
			NewNode.InnerText = SessionId;
			SessionNode.AppendChild(NewNode);

			NewNode = XmlRes.CreateElement("sequenceNumber");
			NewNode.InnerText = SequenceNumber;
			SessionNode.AppendChild(NewNode);

			NewNode = XmlRes.CreateElement("securityToken");
			NewNode.InnerText = SecurityToken;
            SessionNode.AppendChild(NewNode);

            NewNode = XmlRes.CreateElement("officeId");
            NewNode.InnerText = OfficeId(SNM);
            SessionNode.AppendChild(NewNode);

			XmlRes.ChildNodes[0].AppendChild(SessionNode);

			return XmlRes.DocumentElement;
		}

		/// <summary>
		/// Amadeus Request and Response(HttpWebRequest) - 한글 인코딩 문제로 HttpWebRequest 사용
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <param name="ReqXml"></param>
		/// <param name="SessionId"></param>
		/// <param name="SequenceNumber"></param>
		/// <param name="SecurityToken"></param>
        /// <param name="GUID">고유번호</param>
		/// <returns></returns>
		public XmlElement HttpExecute(string ServiceName, XmlElement ReqXml, string SessionId, string SequenceNumber, string SecurityToken, string GUID)
		{
            if (ServiceName.Equals("InstantTravelBoardSearch"))
			{
                return XmlRequest.AmadeusSoapSend(ServiceURL(), ActionURL("Fare_InstantTravelBoardSearch"), ServiceName, XmlRequest.SoapHeaderForAmadeus(SessionId, SequenceNumber, SecurityToken, NamespaceURL("Fare_InstantTravelBoardSearch"), ReqXml.OuterXml), GUID);
            }
            else if (ServiceName.Equals("MasterPricerTravelBoardSearch"))
            {
                return XmlRequest.AmadeusSoapSend(ServiceURL(), ActionURL("Fare_MasterPricerTravelBoardSearch"), ServiceName, XmlRequest.SoapHeaderForAmadeus(SessionId, SequenceNumber, SecurityToken, NamespaceURL("Fare_MasterPricerTravelBoardSearch"), ReqXml.OuterXml), GUID);
            }
			else if (ServiceName.Equals("GetRulesOfPricedItinerary"))
			{
                return XmlRequest.AmadeusSoapSend(ServiceURL(), ActionURL("Fare_GetRulesOfPricedItinerary"), ServiceName, XmlRequest.SoapHeaderForAmadeus(SessionId, SequenceNumber, SecurityToken, NamespaceURL("Fare_GetRulesOfPricedItinerary"), ReqXml.OuterXml), GUID);
			}
			else if (ServiceName.Equals("AddMultiElements"))
			{
                return XmlRequest.AmadeusSoapSend(ServiceURL(), ActionURL("PNR_AddMultiElements"), ServiceName, XmlRequest.SoapHeaderForAmadeus(SessionId, SequenceNumber, SecurityToken, NamespaceURL("PNR_AddMultiElements"), ReqXml.OuterXml), GUID);
			}
			else if (ServiceName.Equals("Retrieve"))
			{
                return XmlRequest.AmadeusSoapSend(ServiceURL(), ActionURL("PNR_Retrieve"), ServiceName, XmlRequest.SoapHeaderForAmadeus(SessionId, SequenceNumber, SecurityToken, NamespaceURL("PNR_Retrieve"), ReqXml.OuterXml), GUID);
			}
			else
			{
				return null;
			}
		}

        /// <summary>
        /// Amadeus Request and Response(HttpWebRequest) - Soap Header 4.0 버전용
        /// </summary>
        /// <param name="ServiceName">서비스명</param>
        /// <param name="OID">Office ID</param>
        /// <param name="ReqXml">XML 데이타</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public XmlElement HttpExecuteSoapHeader4(string ServiceName, string OID, XmlElement ReqXml, string GUID)
        {
            if (ServiceName.Equals("InstantTravelBoardSearch"))
            {
                string ActionURL = "http://webservices.amadeus.com/FIFRTQ_16_2_1A";
                string EndPoint = "https://noded1.production.webservices.amadeus.com/1ASIWIBEMOT";

                return XmlRequest.AmadeusSoapSendHeader4(EndPoint, ActionURL, ServiceName, XmlRequest.SoapHeader4ForAmadeus(OID, EndPoint, ActionURL, ReqXml.OuterXml), GUID);
            }
            else if (ServiceName.Equals("MasterPricerTravelBoardSearch"))
            {
                string ActionURL = "http://webservices.amadeus.com/FMPTBQ_13_3_1A";
                string EndPoint = "https://noded1.production.webservices.amadeus.com/1ASIWIBEMOT";

                return XmlRequest.AmadeusSoapSendHeader4(EndPoint, ActionURL, ServiceName, XmlRequest.SoapHeader4ForAmadeus(OID, EndPoint, ActionURL, ReqXml.OuterXml), GUID);
            }
            else
            {
                return null;
            }
        }

        #region "TEST%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"

        public XmlElement HttpExecuteSoapHeader4TEST(string ServiceName, string OID, XmlElement ReqXml, string GUID)
        {
            string ActionURL = "http://webservices.amadeus.com/FIFRTQ_16_2_1A";
            string EndPoint = "https://noded1.production.webservices.amadeus.com/1ASIWIBEMOT";

            return XmlRequest.SoapSendMPISTEST(EndPoint, ActionURL, XmlRequest.SoapHeader4ForAmadeus(OID, EndPoint, ActionURL, ReqXml.OuterXml), GUID);

            //XmlDocument XmlDoc = new XmlDocument();
            //XmlDoc.LoadXml(XmlRequest.SoapHeader4ForAmadeus(OID, EndPoint, ActionURL, ReqXml.OuterXml));
            //return XmlDoc.DocumentElement;
        }

        #endregion "TEST%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"

        /// <summary>
		/// Amadeus Request and Response(Soap)
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <param name="ReqXml"></param>
		/// <param name="SessionId"></param>
		/// <param name="SequenceNumber"></param>
		/// <param name="SecurityToken"></param>
		/// <returns></returns>
		public XmlElement Execute(string ServiceName, XmlElement ReqXml, string SessionId, string SequenceNumber, string SecurityToken)
		{
			if (ServiceName.Equals("SignOut"))
			{
				AirWebService.SignOut.AmadeusWebService aws = new AirWebService.SignOut.AmadeusWebService();
				AirWebService.SignOut.Session ssn = new AirWebService.SignOut.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("FlightInfo"))
			{
				AirWebService.FlightInfo.AmadeusWebService aws = new AirWebService.FlightInfo.AmadeusWebService();
				AirWebService.FlightInfo.Session ssn = new AirWebService.FlightInfo.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("MultiAvailability"))
			{
				AirWebService.MultiAvailability.AmadeusWebService aws = new AirWebService.MultiAvailability.AmadeusWebService();
				AirWebService.MultiAvailability.Session ssn = new AirWebService.MultiAvailability.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("SellFromRecommendation"))
			{
				AirWebService.SellFromRecommendation.AmadeusWebService aws = new AirWebService.SellFromRecommendation.AmadeusWebService();
				AirWebService.SellFromRecommendation.Session ssn = new AirWebService.SellFromRecommendation.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("RetrieveSeatMap"))
			{
				AirWebService.RetrieveSeatMap.AmadeusWebService aws = new AirWebService.RetrieveSeatMap.AmadeusWebService();
				AirWebService.RetrieveSeatMap.Session ssn = new AirWebService.RetrieveSeatMap.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
            //else if (ServiceName.Equals("InstantTravelBoardSearch"))
            //{
            //    AirWebService.InstantTravelBoardSearch.AmadeusWebService aws = new AirWebService.InstantTravelBoardSearch.AmadeusWebService();
            //    AirWebService.InstantTravelBoardSearch.Session ssn = new AirWebService.InstantTravelBoardSearch.Session();

            //    ssn.SessionId = SessionId;
            //    ssn.SequenceNumber = SequenceNumber;
            //    ssn.SecurityToken = SecurityToken;

            //    aws.session = ssn;

            //    return aws.ServiceRQ(ReqXml);
            //}
            else if (ServiceName.Equals("MasterPricerTravelBoardSearch"))
            {
                AirWebService.MasterPricerTravelBoardSearch.AmadeusWebService aws = new AirWebService.MasterPricerTravelBoardSearch.AmadeusWebService();
                AirWebService.MasterPricerTravelBoardSearch.Session ssn = new AirWebService.MasterPricerTravelBoardSearch.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
			else if (ServiceName.Equals("MasterPricerCalendar"))
			{
				AirWebService.MasterPricerCalendar.AmadeusWebService aws = new AirWebService.MasterPricerCalendar.AmadeusWebService();
				AirWebService.MasterPricerCalendar.Session ssn = new AirWebService.MasterPricerCalendar.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("SellByFareCalendar"))
			{
				AirWebService.SellByFareCalendar.AmadeusWebService aws = new AirWebService.SellByFareCalendar.AmadeusWebService();
				AirWebService.SellByFareCalendar.Session ssn = new AirWebService.SellByFareCalendar.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("SellByFareSearch"))
			{
				AirWebService.SellByFareSearch.AmadeusWebService aws = new AirWebService.SellByFareSearch.AmadeusWebService();
				AirWebService.SellByFareSearch.Session ssn = new AirWebService.SellByFareSearch.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("AddMultiElements"))
			{
				AirWebService.AddMultiElements.AmadeusWebService aws = new AirWebService.AddMultiElements.AmadeusWebService();
				AirWebService.AddMultiElements.Session ssn = new AirWebService.AddMultiElements.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("CheckRules"))
			{
				AirWebService.CheckRules.AmadeusWebService aws = new AirWebService.CheckRules.AmadeusWebService();
				AirWebService.CheckRules.Session ssn = new AirWebService.CheckRules.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("GetRulesOfPricedItinerary"))
			{
				AirWebService.GetRulesOfPricedItinerary.AmadeusWebService aws = new AirWebService.GetRulesOfPricedItinerary.AmadeusWebService();
				AirWebService.GetRulesOfPricedItinerary.Session ssn = new AirWebService.GetRulesOfPricedItinerary.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("InformativeBestPricingWithoutPNR"))
			{
				AirWebService.InformativeBestPricingWithoutPNR.AmadeusWebService aws = new AirWebService.InformativeBestPricingWithoutPNR.AmadeusWebService();
				AirWebService.InformativeBestPricingWithoutPNR.Session ssn = new AirWebService.InformativeBestPricingWithoutPNR.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("InformativePricingWithoutPNR"))
			{
				AirWebService.InformativePricingWithoutPNR.AmadeusWebService aws = new AirWebService.InformativePricingWithoutPNR.AmadeusWebService();
				AirWebService.InformativePricingWithoutPNR.Session ssn = new AirWebService.InformativePricingWithoutPNR.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("InformativePricingWithoutPNRRule"))
			{
				AirWebService.InformativePricingWithoutPNRRule.AmadeusWebService aws = new AirWebService.InformativePricingWithoutPNRRule.AmadeusWebService();
				AirWebService.InformativePricingWithoutPNRRule.Session ssn = new AirWebService.InformativePricingWithoutPNRRule.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("PricePNRWithBookingClass"))
			{
				AirWebService.PricePNRWithBookingClass.AmadeusWebService aws = new AirWebService.PricePNRWithBookingClass.AmadeusWebService();
				AirWebService.PricePNRWithBookingClass.Session ssn = new AirWebService.PricePNRWithBookingClass.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("PricePNRWithBookingClassPricing"))
			{
				AirWebService.PricePNRWithBookingClassPricing.AmadeusWebService aws = new AirWebService.PricePNRWithBookingClassPricing.AmadeusWebService();
				AirWebService.PricePNRWithBookingClassPricing.Session ssn = new AirWebService.PricePNRWithBookingClassPricing.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("PricePNRWithBookingClassKEPricing"))
            {
                AirWebService.PricePNRWithBookingClassKEPricing.AmadeusWebService aws = new AirWebService.PricePNRWithBookingClassKEPricing.AmadeusWebService();
                AirWebService.PricePNRWithBookingClassKEPricing.Session ssn = new AirWebService.PricePNRWithBookingClassKEPricing.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
			else if (ServiceName.Equals("CreateTSTFromPricing"))
			{
				AirWebService.CreateTSTFromPricing.AmadeusWebService aws = new AirWebService.CreateTSTFromPricing.AmadeusWebService();
				AirWebService.CreateTSTFromPricing.Session ssn = new AirWebService.CreateTSTFromPricing.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("DisplayTST"))
			{
				AirWebService.DisplayTST.AmadeusWebService aws = new AirWebService.DisplayTST.AmadeusWebService();
				AirWebService.DisplayTST.Session ssn = new AirWebService.DisplayTST.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("DisplayQuota"))
			{
				AirWebService.DisplayQuota.AmadeusWebService aws = new AirWebService.DisplayQuota.AmadeusWebService();
				AirWebService.DisplayQuota.Session ssn = new AirWebService.DisplayQuota.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("Retrieve"))
			{
				AirWebService.Retrieve.AmadeusWebService aws = new AirWebService.Retrieve.AmadeusWebService();
				AirWebService.Retrieve.Session ssn = new AirWebService.Retrieve.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("Cancel"))
			{
				AirWebService.Cancel.AmadeusWebService aws = new AirWebService.Cancel.AmadeusWebService();
				AirWebService.Cancel.Session ssn = new AirWebService.Cancel.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("CreateFormOfPayment"))
			{
				AirWebService.CreateFormOfPayment.AmadeusWebService aws = new AirWebService.CreateFormOfPayment.AmadeusWebService();
				AirWebService.CreateFormOfPayment.Session ssn = new AirWebService.CreateFormOfPayment.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("ProcessEDoc"))
			{
				AirWebService.ProcessEDoc.AmadeusWebService aws = new AirWebService.ProcessEDoc.AmadeusWebService();
				AirWebService.ProcessEDoc.Session ssn = new AirWebService.ProcessEDoc.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("IssueTicket"))
			{
				AirWebService.IssueTicket.AmadeusWebService aws = new AirWebService.IssueTicket.AmadeusWebService();
				AirWebService.IssueTicket.Session ssn = new AirWebService.IssueTicket.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else if (ServiceName.Equals("CancelDocument"))
			{
				AirWebService.CancelDocument.AmadeusWebService aws = new AirWebService.CancelDocument.AmadeusWebService();
				AirWebService.CancelDocument.Session ssn = new AirWebService.CancelDocument.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("QueueCountTotal"))
            {
                AirWebService.QueueCountTotal.AmadeusWebService aws = new AirWebService.QueueCountTotal.AmadeusWebService();
                AirWebService.QueueCountTotal.Session ssn = new AirWebService.QueueCountTotal.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("QueueList"))
            {
                AirWebService.QueueList.AmadeusWebService aws = new AirWebService.QueueList.AmadeusWebService();
                AirWebService.QueueList.Session ssn = new AirWebService.QueueList.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("QueueMoveItem"))
            {
                AirWebService.QueueMoveItem.AmadeusWebService aws = new AirWebService.QueueMoveItem.AmadeusWebService();
                AirWebService.QueueMoveItem.Session ssn = new AirWebService.QueueMoveItem.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("QueuePlacePNR"))
            {
                AirWebService.QueuePlacePNR.AmadeusWebService aws = new AirWebService.QueuePlacePNR.AmadeusWebService();
                AirWebService.QueuePlacePNR.Session ssn = new AirWebService.QueuePlacePNR.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("QueueRemoveItem"))
            {
                AirWebService.QueueRemoveItem.AmadeusWebService aws = new AirWebService.QueueRemoveItem.AmadeusWebService();
                AirWebService.QueueRemoveItem.Session ssn = new AirWebService.QueueRemoveItem.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
            else if (ServiceName.Equals("MiniRuleGetFromPricing"))
            {
                AirWebService.MiniRuleGetFromPricing.AmadeusWebService aws = new AirWebService.MiniRuleGetFromPricing.AmadeusWebService();
                AirWebService.MiniRuleGetFromPricing.Session ssn = new AirWebService.MiniRuleGetFromPricing.Session();

                ssn.SessionId = SessionId;
                ssn.SequenceNumber = SequenceNumber;
                ssn.SecurityToken = SecurityToken;

                aws.session = ssn;

                return aws.ServiceRQ(ReqXml);
            }
			else if (ServiceName.Equals("CommandCryptic"))
			{
				AirWebService.CommandCryptic.AmadeusWebService aws = new AirWebService.CommandCryptic.AmadeusWebService();
				AirWebService.CommandCryptic.Session ssn = new AirWebService.CommandCryptic.Session();

				ssn.SessionId = SessionId;
				ssn.SequenceNumber = SequenceNumber;
				ssn.SecurityToken = SecurityToken;

				aws.session = ssn;

				return aws.ServiceRQ(ReqXml);
			}
			else
			{
				return null;
			}
		}
	}
}