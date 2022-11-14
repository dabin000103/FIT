using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;

namespace AirWebService
{
	/// <summary>
	/// Amadeus 실시간 항공 예약을 위한 각종 정보 제공
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	public class AmadeusAirService : System.Web.Services.WebService
	{
		Common cm;
		AmadeusConfig ac;
		HttpContext hcc;

		public AmadeusAirService()
		{
			cm = new Common();
			ac = new AmadeusConfig();
			hcc = HttpContext.Current;
		}

		#region "Security"

        /// <summary>
        /// 고유번호 생성
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "고유번호 생성")]
        public string GUID()
        {
            return cm.GetGUID;
        }

        /// <summary>
        /// 오피스아이디
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <returns></returns>
        //[WebMethod(Description = "오피스아이디")]
        public string OfficeId(int SNM)
        {
            return AmadeusConfig.OfficeId(SNM);
        }

        /// <summary>
		/// 세션생성
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Security_Authenticate")]
		public XmlElement Authenticate(int SNM, string GUID)
		{
            XmlElement ResXml = ac.Authenticate(SNM);

            //로그 DB 저장을 닷컴에 한해서만 진행(2017-02-08,고재영)
            cm.XmlFileSave(ResXml, ac.Name, "Authenticate", ((SNM.Equals(2) || SNM.Equals(3915)) ? "Y" : "N"), GUID);

			return ResXml;
		}

		/// <summary>
		/// 세션종료
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Security_SignOut")]
		public int SignOut(string SID, string SQN, string SCT, string GUID)
		{
			try
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(ac.XmlFullPath("SignOutRQ"));

				XmlElement ResXml = ac.Execute("SignOut", XmlDoc.DocumentElement, SID, SQN, SCT);

                //세선종료는 로그 DB 저장을 제외(2017-02-08,고재영)
                cm.XmlFileSave(ResXml, ac.Name, "SignOut", "N", GUID);
			}
			catch (Exception ex)
			{
                new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}

			return 0;
		}

		#endregion "Security"

		#region "세션풀"

		/// <summary>
		/// 세션생성
        /// </summary>
        /// <param name="SNM">사이트번호</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "세션생성")]
		public XmlElement SessionCreate(int SNM, string GUID)
		{
			XmlElement Session;
			string SessionId = string.Empty;
			string SequenceNumber = string.Empty;
            string SecurityToken = string.Empty;
            string OfficeId = AmadeusConfig.OfficeId(SNM);
			
			using (SqlCommand cmd = new SqlCommand())
			{
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString))
				{
					SqlDataReader dr = null;

					cmd.Connection = conn;
                    cmd.CommandTimeout = 3;
					cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DBO.WSV_S_아이템_세션풀";

					cmd.Parameters.Add("@품목코드", SqlDbType.Char, 2);
					cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@OfficeId", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@품목코드"].Value = "IA";
					cmd.Parameters["@GDS"].Value = ac.Name;
                    cmd.Parameters["@OfficeId"].Value = OfficeId;
					cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
					cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

					try
					{
						conn.Open();
						dr = cmd.ExecuteReader();

						if (dr.Read())
						{
							SessionId = dr["SessionId"].ToString();
							SequenceNumber = dr["SequenceNumber"].ToString();
                            SecurityToken = dr["SecurityToken"].ToString();
						}
					}
					finally
					{
						dr.Dispose();
						dr.Close();
						conn.Close();
					}
				}
			}

			if (String.IsNullOrWhiteSpace(SessionId) || String.IsNullOrWhiteSpace(SequenceNumber) || String.IsNullOrWhiteSpace(SecurityToken))
			{
				Session = ac.Authenticate(SNM);
				Session.SelectSingleNode("session/sequenceNumber").InnerText = (cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1) + 1).ToString();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString))
				{
					SqlCommand cmd = new SqlCommand();

					cmd.Connection = conn;
                    cmd.CommandTimeout = 3;
					cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DBO.WSV_T_아이템_세션풀_추가";

					cmd.Parameters.Add("@품목코드", SqlDbType.Char, 2);
					cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@OfficeId", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@SessionId", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@SequenceNumber", SqlDbType.Int, 0);
					cmd.Parameters.Add("@SecurityToken", SqlDbType.VarChar, 50);
					cmd.Parameters.Add("@사용여부", SqlDbType.Char, 1);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@품목코드"].Value = "IA";
					cmd.Parameters["@GDS"].Value = ac.Name;
                    cmd.Parameters["@OfficeId"].Value = OfficeId;
					cmd.Parameters["@SessionId"].Value = Session.SelectSingleNode("session/sessionId").InnerText;
					cmd.Parameters["@SequenceNumber"].Value = Session.SelectSingleNode("session/sequenceNumber").InnerText;
					cmd.Parameters["@SecurityToken"].Value = Session.SelectSingleNode("session/securityToken").InnerText;
                    cmd.Parameters["@사용여부"].Value = "Y";
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
			}
			else
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(ac.XmlFullPath("AuthenticateRS"));

				XmlDoc.SelectSingleNode("Security_AuthenticateReply/session/sessionId").InnerText = SessionId;
				XmlDoc.SelectSingleNode("Security_AuthenticateReply/session/sequenceNumber").InnerText = SequenceNumber;
                XmlDoc.SelectSingleNode("Security_AuthenticateReply/session/securityToken").InnerText = SecurityToken;
                XmlDoc.SelectSingleNode("Security_AuthenticateReply/session/officeId").InnerText = OfficeId;

				Session = XmlDoc.DocumentElement;
			}

            //로그 DB 저장을 닷컴에 한해서만 진행(2017-02-08,고재영)
            cm.XmlFileSave(Session, ac.Name, "SessionCreate", ((SNM.Equals(2) || SNM.Equals(3915)) ? "Y" : "N"), GUID);

			return Session;
		}

		/// <summary>
		/// 세션종료
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "세션종료")]
		public void SessionClose(string SID, string SCT, string GUID)
		{
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString))
			{
				SqlCommand cmd = new SqlCommand();

				cmd.Connection = conn;
                cmd.CommandTimeout = 3;
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DBO.WSV_T_아이템_세션풀_사용완료";

				cmd.Parameters.Add("@품목코드", SqlDbType.Char, 2);
				cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 20);
				cmd.Parameters.Add("@SessionId", SqlDbType.VarChar, 20);
				cmd.Parameters.Add("@SecurityToken", SqlDbType.VarChar, 50);
				cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
				cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

				cmd.Parameters["@품목코드"].Value = "IA";
				cmd.Parameters["@GDS"].Value = ac.Name;
				cmd.Parameters["@SessionId"].Value = SID;
				cmd.Parameters["@SecurityToken"].Value = SCT;
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

			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("SignOutRQ"));

			XmlDoc.SelectSingleNode("Security_SignOut").InnerText = SID;

            //세선종료는 로그 DB 저장을 제외(2017-02-08,고재영)
			cm.XmlFileSave(XmlDoc, ac.Name, "SessionClose", "N", GUID);
		}

		/// <summary>
		/// 세션삭제
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		[WebMethod(Description = "세션삭제")]
		public int SessionDelete(string SID, string SQN, string SCT, string GUID)
		{
			try
			{
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString))
				{
					SqlCommand cmd = new SqlCommand();

					cmd.Connection = conn;
                    cmd.CommandTimeout = 3;
					cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DBO.WSV_T_아이템_세션풀_삭제";

					cmd.Parameters.Add("@품목코드", SqlDbType.Char, 2);
					cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@SessionId", SqlDbType.VarChar, 20);
					cmd.Parameters.Add("@SecurityToken", SqlDbType.VarChar, 50);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@품목코드"].Value = "IA";
					cmd.Parameters["@GDS"].Value = ac.Name;
					cmd.Parameters["@SessionId"].Value = SID;
					cmd.Parameters["@SecurityToken"].Value = SCT;
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
			
				return SignOut(SID, SQN, SCT, GUID);
			}
			catch (Exception ex)
			{
                new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
				return 0;
			}
		}

		#endregion "세션풀"

		#region "네임스페이스"

		/// <summary>
		/// 네임스페이스 조회
		/// </summary>
		/// <param name="ServiceName">서비스명</param>
		/// <returns></returns>
		[WebMethod(Description = "NamespaceURL")]
		public string NamespaceURL(string ServiceName)
		{
			return AmadeusConfig.NamespaceURL(ServiceName);
		}

		#endregion "네임스페이스"

		#region "Air"

		#region "FlightInfo"

		[WebMethod(Description = "Air_FlightInfoRQ")]
		public XmlElement FlightInfoRQ(string DTD, string DTT, string DLC, string ALC, string MCC, string OCC, string FLN)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("FlightInfoRQ"));

			XmlNode GeneralFlightInfo = XmlDoc.SelectSingleNode("Air_FlightInfo/generalFlightInfo");
			XmlNode FlightDate = GeneralFlightInfo.SelectSingleNode("flightDate");
			XmlNode BoardPointDetails = GeneralFlightInfo.SelectSingleNode("boardPointDetails");
			XmlNode OffPointDetails = GeneralFlightInfo.SelectSingleNode("offPointDetails");
			XmlNode CompanyDetails = GeneralFlightInfo.SelectSingleNode("companyDetails");
			XmlNode FlightIdentification = GeneralFlightInfo.SelectSingleNode("flightIdentification");

			if (String.IsNullOrWhiteSpace(DTD))
				GeneralFlightInfo.RemoveChild(FlightDate);
			else
			{
				FlightDate.SelectSingleNode("departureDate").InnerText = cm.ConvertToAmadeusDate(DTD);

				if (String.IsNullOrWhiteSpace(DTT))
					FlightDate.RemoveChild(FlightDate.SelectSingleNode("departureTime"));
				else
					FlightDate.SelectSingleNode("departureTime").InnerText = cm.ConvertToAmadeusTime(DTT);
			}

			if (String.IsNullOrWhiteSpace(DLC))
				GeneralFlightInfo.RemoveChild(BoardPointDetails);
			else
				BoardPointDetails.SelectSingleNode("trueLocationId").InnerText = DLC;

			if (String.IsNullOrWhiteSpace(ALC))
				GeneralFlightInfo.RemoveChild(OffPointDetails);
			else
				OffPointDetails.SelectSingleNode("trueLocationId").InnerText = ALC;

			if (String.IsNullOrWhiteSpace(MCC))
				GeneralFlightInfo.RemoveChild(CompanyDetails);
			else
			{
				CompanyDetails.SelectSingleNode("marketingCompany").InnerText = MCC;

				if (String.IsNullOrWhiteSpace(OCC))
					CompanyDetails.RemoveChild(CompanyDetails.SelectSingleNode("operatingCompany"));
				else
					CompanyDetails.SelectSingleNode("operatingCompany").InnerText = OCC;
			}

			if (String.IsNullOrWhiteSpace(FLN))
				GeneralFlightInfo.RemoveChild(FlightIdentification);
			else
				FlightIdentification.SelectSingleNode("flightNumber").InnerText = FLN;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// FlightInfo 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="DTT">출발시간(HHMM)</param>
		/// <param name="DLC">출발지</param>
		/// <param name="ALC">도착지</param>
		/// <param name="MCC">마케팅항공사</param>
		/// <param name="OCC">운항항공사</param>
		/// <param name="FLN">편명</param>
		/// <returns></returns>
		[WebMethod(Description = "Air_FlightInfoRS")]
		public XmlElement FlightInfoRS(string SID, string SQN, string SCT, string GUID, string DTD, string DTT, string DLC, string ALC, string MCC, string OCC, string FLN)
		{
			XmlElement ReqXml = FlightInfoRQ(DTD, DTT, DLC, ALC, MCC, OCC, FLN);
			cm.XmlFileSave(ReqXml, ac.Name, "FlightInfoRQ", "N", GUID);

			XmlElement ResXml = ac.Execute("FlightInfo", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "FlightInfoRS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Air_FlightInfoXml")]
		public XmlElement FlightInfoXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("FlightInfo", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "FlightInfo"

		#region "MultiAvailability"

		[WebMethod(Description = "Air_MultiAvailabilityRQ")]
		public XmlElement MultiAvailabilityRQ(string FAC, string DTD, string DTT, string DLC, string ALC, string CLC, string SCD, string MCC, string FLN, int NOS)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("MultiAvailabilityRQ"));

			XmlNode RequestSection = XmlDoc.SelectSingleNode("Air_MultiAvailability/requestSection");
			XmlNode AvailabilityProductInfo = RequestSection.SelectSingleNode("availabilityProductInfo");
			XmlNode OptionClass = RequestSection.SelectSingleNode("optionClass");
			XmlNode ConnectionOption = RequestSection.SelectSingleNode("connectionOption");
			XmlNode NumberOfSeatsInfo = RequestSection.SelectSingleNode("numberOfSeatsInfo");
			XmlNode AirlineOrFlightOption = RequestSection.SelectSingleNode("airlineOrFlightOption");
            XmlNode AvailabilityOptions = RequestSection.SelectSingleNode("availabilityOptions");

			XmlDoc.SelectSingleNode("Air_MultiAvailability/messageActionDetails/functionDetails/actionCode").InnerText = FAC;

			AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureDate").InnerText = cm.ConvertToAmadeusDate(DTD);
			AvailabilityProductInfo.SelectSingleNode("departureLocationInfo/cityAirport").InnerText = DLC;
			AvailabilityProductInfo.SelectSingleNode("arrivalLocationInfo/cityAirport").InnerText = ALC;

			if (String.IsNullOrWhiteSpace(DTT))
				AvailabilityProductInfo.SelectSingleNode("availabilityDetails").RemoveChild(AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureTime"));
			else
				AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureTime").InnerText = cm.ConvertToAmadeusTime(DTT);

			if (String.IsNullOrWhiteSpace(SCD))
				RequestSection.RemoveChild(OptionClass);
			else
			{
				XmlNode ProductClassDetails = OptionClass.SelectSingleNode("productClassDetails");
				XmlNode NewProductClassDetails;

				foreach (string SvcClass in SCD.Split(','))
				{
					NewProductClassDetails = OptionClass.AppendChild(ProductClassDetails.CloneNode(true));
					NewProductClassDetails.SelectSingleNode("serviceClass").InnerText = SvcClass.Trim();
				}

				OptionClass.RemoveChild(ProductClassDetails);
			}

			if (String.IsNullOrWhiteSpace(CLC))
				RequestSection.RemoveChild(ConnectionOption);
            else
			{
				string[] CLCs = CLC.Split(',');

				ConnectionOption.SelectSingleNode("firstConnection/location").InnerText = CLCs[0].Trim();
                ConnectionOption.SelectSingleNode("firstConnection").RemoveChild(ConnectionOption.SelectSingleNode("firstConnection/indicatorList"));

                if (CLCs.Length.Equals(1))
                    ConnectionOption.RemoveChild(ConnectionOption.SelectSingleNode("secondConnection"));
                else
                {
                    ConnectionOption.SelectSingleNode("secondConnection/location").InnerText = CLCs[1].Trim();
                    ConnectionOption.SelectSingleNode("secondConnection").RemoveChild(ConnectionOption.SelectSingleNode("secondConnection/indicatorList"));
                }
			}

			if (NOS.Equals(0))
				RequestSection.RemoveChild(NumberOfSeatsInfo);
			else
				NumberOfSeatsInfo.SelectSingleNode("numberOfPassengers").InnerText = NOS.ToString();

			if (String.IsNullOrWhiteSpace(MCC))
				RequestSection.RemoveChild(AirlineOrFlightOption);
			else
			{
				string[] MCCs = MCC.Split(',');
				string[] FLNs = FLN.Split(',');

				XmlNode FlightIdentification = AirlineOrFlightOption.SelectSingleNode("flightIdentification");
				XmlNode NewFlightIdentification;

				for (int i = 0; i < MCCs.Length; i++)
				{
					NewFlightIdentification = AirlineOrFlightOption.AppendChild(FlightIdentification.CloneNode(true));
					NewFlightIdentification.SelectSingleNode("airlineCode").InnerText = MCCs[i].Trim();

					if (FLNs.Length > i)
						NewFlightIdentification.SelectSingleNode("number").InnerText = FLNs[i].Trim();
					else
						NewFlightIdentification.RemoveChild(NewFlightIdentification.SelectSingleNode("number"));
				}

				AirlineOrFlightOption.RemoveChild(FlightIdentification);
			}

            AvailabilityOptions.SelectSingleNode("productTypeDetails/typeOfRequest").InnerText = "TN";
            AvailabilityOptions.RemoveChild(AvailabilityOptions.SelectSingleNode("optionInfo"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// Availability 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="FAC">구분코드(44:Availability, 48:Schedule, 51:TimeTable, 55:MD)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="DTT">출발시간(HHMM)</param>
		/// <param name="DLC">출발지</param>
		/// <param name="ALC">도착지</param>
		/// <param name="CLC">경유지(콤마로 구분)</param>
		/// <param name="SCD">서비스클래스(콤마로 구분)</param>
		/// <param name="MCC">항공사(콤마로 구분)</param>
		/// <param name="FLN">편명(콤마로 구분)</param>
		/// <param name="NOS">좌석수</param>
		/// <returns></returns>
		[WebMethod(Description = "Air_MultiAvailabilityRS")]
		public XmlElement MultiAvailabilityRS(string SID, string SQN, string SCT, string GUID, string FAC, string DTD, string DTT, string DLC, string ALC, string CLC, string SCD, string MCC, string FLN, int NOS)
		{
			XmlElement ReqXml = MultiAvailabilityRQ(FAC, DTD, DTT, DLC, ALC, CLC, SCD, MCC, FLN, NOS);
			cm.XmlFileSave(ReqXml, ac.Name, "MultiAvailabilityRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("MultiAvailability", ReqXml, SID, SQN, SCT);
			cm.XmlFileSave(ResXml, ac.Name, "MultiAvailabilityRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Air_MultiAvailabilityXml")]
		public XmlElement MultiAvailabilityXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("MultiAvailability", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "MultiAvailability"

        #region "MultiAvailability2"

        [WebMethod(Description = "Air_MultiAvailability2RQ")]
        public XmlElement MultiAvailability2RQ(string FAC, string DTD, string DTT, string DLC, string ALC, string CLC, string SCD, string MCC, string FLN, int NOS, string FLO, string TOR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("MultiAvailabilityRQ"));

            XmlNode RequestSection = XmlDoc.SelectSingleNode("Air_MultiAvailability/requestSection");
            XmlNode AvailabilityProductInfo = RequestSection.SelectSingleNode("availabilityProductInfo");
            XmlNode OptionClass = RequestSection.SelectSingleNode("optionClass");
            XmlNode ConnectionOption = RequestSection.SelectSingleNode("connectionOption");
            XmlNode NumberOfSeatsInfo = RequestSection.SelectSingleNode("numberOfSeatsInfo");
            XmlNode AirlineOrFlightOption = RequestSection.SelectSingleNode("airlineOrFlightOption");
            XmlNode AvailabilityOptions = RequestSection.SelectSingleNode("availabilityOptions");

            XmlDoc.SelectSingleNode("Air_MultiAvailability/messageActionDetails/functionDetails/actionCode").InnerText = FAC;

            AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureDate").InnerText = cm.ConvertToAmadeusDate(DTD);
            AvailabilityProductInfo.SelectSingleNode("departureLocationInfo/cityAirport").InnerText = DLC;
            AvailabilityProductInfo.SelectSingleNode("arrivalLocationInfo/cityAirport").InnerText = ALC;

            if (String.IsNullOrWhiteSpace(DTT))
                AvailabilityProductInfo.SelectSingleNode("availabilityDetails").RemoveChild(AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureTime"));
            else
                AvailabilityProductInfo.SelectSingleNode("availabilityDetails/departureTime").InnerText = cm.ConvertToAmadeusTime(DTT);

            if (String.IsNullOrWhiteSpace(SCD))
                RequestSection.RemoveChild(OptionClass);
            else
            {
                XmlNode ProductClassDetails = OptionClass.SelectSingleNode("productClassDetails");
                XmlNode NewProductClassDetails;

                foreach (string SvcClass in SCD.Split(','))
                {
                    NewProductClassDetails = OptionClass.AppendChild(ProductClassDetails.CloneNode(true));
                    NewProductClassDetails.SelectSingleNode("serviceClass").InnerText = SvcClass.Trim();
                }

                OptionClass.RemoveChild(ProductClassDetails);
            }

            if (String.IsNullOrWhiteSpace(CLC))
                RequestSection.RemoveChild(ConnectionOption);
            else
            {
                string[] CLCs = CLC.Split(',');

                ConnectionOption.SelectSingleNode("firstConnection/location").InnerText = CLCs[0].Trim();
                ConnectionOption.SelectSingleNode("firstConnection/indicatorList").InnerText = "701";

                if (CLCs.Length.Equals(1))
                    ConnectionOption.RemoveChild(ConnectionOption.SelectSingleNode("secondConnection"));
                else
                {
                    ConnectionOption.SelectSingleNode("secondConnection/location").InnerText = CLCs[1].Trim();
                    ConnectionOption.SelectSingleNode("secondConnection/indicatorList").InnerText = "701";
                }
            }

            if (NOS.Equals(0))
                RequestSection.RemoveChild(NumberOfSeatsInfo);
            else
                NumberOfSeatsInfo.SelectSingleNode("numberOfPassengers").InnerText = NOS.ToString();

            if (String.IsNullOrWhiteSpace(MCC))
                RequestSection.RemoveChild(AirlineOrFlightOption);
            else
            {
                string[] MCCs = MCC.Split(',');
                string[] FLNs = FLN.Split(',');

                XmlNode FlightIdentification = AirlineOrFlightOption.SelectSingleNode("flightIdentification");
                XmlNode NewFlightIdentification;

                for (int i = 0; i < MCCs.Length; i++)
                {
                    NewFlightIdentification = AirlineOrFlightOption.AppendChild(FlightIdentification.CloneNode(true));
                    NewFlightIdentification.SelectSingleNode("airlineCode").InnerText = MCCs[i].Trim();

                    if (FLNs.Length > i)
                        NewFlightIdentification.SelectSingleNode("number").InnerText = FLNs[i].Trim();
                    else
                        NewFlightIdentification.RemoveChild(NewFlightIdentification.SelectSingleNode("number"));
                }

                AirlineOrFlightOption.RemoveChild(FlightIdentification);
            }

            AvailabilityOptions.SelectSingleNode("productTypeDetails/typeOfRequest").InnerText = String.IsNullOrWhiteSpace(TOR) ? "TN" : TOR;

            if (String.IsNullOrWhiteSpace(FLO))
                AvailabilityOptions.RemoveChild(AvailabilityOptions.SelectSingleNode("optionInfo"));
            else
            {
                XmlNode OptionInfo = AvailabilityOptions.SelectSingleNode("optionInfo");
                XmlNode Arguments = OptionInfo.SelectSingleNode("arguments");
                XmlNode NewArguments;

                foreach (string argument in FLO.Split(','))
                {
                    NewArguments = OptionInfo.AppendChild(Arguments.Clone());
                    NewArguments.InnerText = argument.Trim();
                }

                OptionInfo.RemoveChild(Arguments);
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// Availability/Schedule/TimeTable 조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="FAC">구분코드(44:Availability, 48:Schedule, 51:TimeTable, 55:MD)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="DTT">출발시간(HHMM)</param>
        /// <param name="DLC">출발지</param>
        /// <param name="ALC">도착지</param>
        /// <param name="CLC">경유지(콤마로 구분)</param>
        /// <param name="SCD">서비스클래스(콤마로 구분)</param>
        /// <param name="MCC">항공사(콤마로 구분)</param>
        /// <param name="FLN">편명(콤마로 구분)</param>
        /// <param name="NOS">좌석수</param>
        /// <param name="FLO">항공조회옵션(OD:direct only, ON:non-stop only, OC:connections only)(콤마로 구분)</param>
        /// <param name="TOR">요청타입(SF:Flight Specific, TA:By arrival time, TD:By departure time, TE:By elapsed time, TG:Group availability, TN:By neutral order, TT:Negociated space)</param>
        /// <returns></returns>
        [WebMethod(Description = "Air_MultiAvailability2RS")]
        public XmlElement MultiAvailability2RS(string SID, string SQN, string SCT, string GUID, string FAC, string DTD, string DTT, string DLC, string ALC, string CLC, string SCD, string MCC, string FLN, int NOS, string FLO, string TOR)
        {
            XmlElement ReqXml = MultiAvailability2RQ(FAC, DTD, DTT, DLC, ALC, CLC, SCD, MCC, FLN, NOS, FLO, TOR);
            cm.XmlFileSave(ReqXml, ac.Name, "MultiAvailability2RQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("MultiAvailability", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "MultiAvailability2RS", "Y", GUID);

            return ResXml;
        }

        #endregion "MultiAvailability2"

		#region "SellFromRecommendation"

		[WebMethod(Description = "Air_SellFromRecommendationRQ")]
		public XmlElement SellFromRecommendationRQ(string OPN, XmlElement SXL)
		{
			try
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(ac.XmlFullPath("SellFromRecommendationRQ"));

				XmlNode ASFR = XmlDoc.SelectSingleNode("Air_SellFromRecommendation");
				XmlNode ItineraryDetails = ASFR.SelectSingleNode("itineraryDetails");
				XmlNode SegmentInformation;
				XmlNode TravelProductInformation;
				XmlNode NewItineraryDetails;
				XmlNode NewSegmentInformation;

				foreach (XmlNode SegGroup in SXL.SelectNodes("segGroup"))
				{
					NewItineraryDetails = ASFR.AppendChild(ItineraryDetails.CloneNode(true));
					SegmentInformation = NewItineraryDetails.SelectSingleNode("segmentInformation");

					NewItineraryDetails.SelectSingleNode("originDestinationDetails/origin").InnerText = SegGroup.SelectSingleNode("seg[1]").Attributes.GetNamedItem("dlc").InnerText;
					NewItineraryDetails.SelectSingleNode("originDestinationDetails/destination").InnerText = SegGroup.SelectSingleNode("seg[last()]").Attributes.GetNamedItem("alc").InnerText;

					foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
					{
						NewSegmentInformation = NewItineraryDetails.AppendChild(SegmentInformation.CloneNode(true));
						TravelProductInformation = NewSegmentInformation.SelectSingleNode("travelProductInformation");

						TravelProductInformation.SelectSingleNode("flightDate/departureDate").InnerText = cm.ConvertToAmadeusDate(Seg.Attributes.GetNamedItem("ddt").InnerText);
						TravelProductInformation.SelectSingleNode("flightDate/departureTime").InnerText = cm.ConvertToAmadeusTime(Seg.Attributes.GetNamedItem("ddt").InnerText);
						TravelProductInformation.SelectSingleNode("flightDate/arrivalDate").InnerText = cm.ConvertToAmadeusDate(Seg.Attributes.GetNamedItem("ardt").InnerText);
						TravelProductInformation.SelectSingleNode("flightDate/arrivalTime").InnerText = cm.ConvertToAmadeusTime(Seg.Attributes.GetNamedItem("ardt").InnerText);
						TravelProductInformation.SelectSingleNode("boardPointDetails/trueLocationId").InnerText = Seg.Attributes.GetNamedItem("dlc").InnerText;
						TravelProductInformation.SelectSingleNode("offpointDetails/trueLocationId").InnerText = Seg.Attributes.GetNamedItem("alc").InnerText;
						TravelProductInformation.SelectSingleNode("companyDetails/marketingCompany").InnerText = Seg.Attributes.GetNamedItem("mcc").InnerText;
						TravelProductInformation.SelectSingleNode("flightIdentification/flightNumber").InnerText = Seg.Attributes.GetNamedItem("fln").InnerText;
						TravelProductInformation.SelectSingleNode("flightIdentification/bookingClass").InnerText = Seg.Attributes.GetNamedItem("rbd").InnerText;
						NewSegmentInformation.SelectSingleNode("relatedproductInformation/quantity").InnerText = Seg.Attributes.GetNamedItem("nos").InnerText;
						NewSegmentInformation.SelectSingleNode("relatedproductInformation/statusCode").InnerText = "NN";
					}

					NewItineraryDetails.RemoveChild(SegmentInformation);

					if (OPN.Equals("Y"))
						break;
				}

				ASFR.RemoveChild(ItineraryDetails);

				return XmlDoc.DocumentElement;
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		/// <summary>
		/// 항공편 예약
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="OPN">오픈여부(YN)</param>
		/// <param name="SXL">여정 XML Element</param>
		/// <returns></returns>
		[WebMethod(Description = "Air_SellFromRecommendationRS")]
		public XmlElement SellFromRecommendationRS(string SID, string SQN, string SCT, string GUID, string OPN, XmlElement SXL)
		{
			try
			{
				XmlElement ReqXml = SellFromRecommendationRQ(OPN, SXL);
				cm.XmlFileSave(ReqXml, ac.Name, "SellFromRecommendationRQ", "Y", GUID);

				XmlElement ResXml = ac.Execute("SellFromRecommendation", ReqXml, SID, SQN, SCT);
				cm.XmlFileSave(ResXml, ac.Name, "SellFromRecommendationRS", "Y", GUID);

				return ResXml;
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		[WebMethod(Description = "Air_SellFromRecommendationXml")]
		public XmlElement SellFromRecommendationXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("SellFromRecommendation", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "SellFromRecommendation"

		#region "RetrieveSeatMap"

		[WebMethod(Description = "Air_RetrieveSeatMapRQ")]
		public XmlElement RetrieveSeatMapRQ(string FLN)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("RetrieveSeatMapRQ"));

			XmlDoc.SelectSingleNode("Air_RetrieveSeatMap/travelProductIdent/flightIdentification/flightNumber").InnerText = FLN;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// Seat Map 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="FLN">편명</param>
		/// <returns></returns>
		[WebMethod(Description = "Air_RetrieveSeatMapRS")]
		public XmlElement RetrieveSeatMapRS(string SID, string SQN, string SCT, string GUID, string FLN)
		{
			XmlElement ReqXml = RetrieveSeatMapRQ(FLN);
            cm.XmlFileSave(ReqXml, ac.Name, "RetrieveSeatMapRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("RetrieveSeatMap", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "RetrieveSeatMapRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Air_RetrieveSeatMapXml")]
		public XmlElement RetrieveSeatMapXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("RetrieveSeatMap", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "RetrieveSeatMap"

		#endregion "Air"

		#region "Fare"

		#region "GetRulesOfPricedItinerary"

		[WebMethod(Description = "Fare_GetRulesOfPricedItineraryRQ")]
		public XmlElement GetRulesOfPricedItineraryRQ(string ULC)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("GetRulesOfPricedItineraryRQ"));

			if (!String.IsNullOrWhiteSpace(ULC))
				XmlDoc.SelectSingleNode("Fare_GetRulesOfPricedItinerary/optionInfoGroup/optionInfo/selectionDetails/optionInformation").InnerText = ULC;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 운임규정 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="ULC">언어코드</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_GetRulesOfPricedItineraryRS")]
		public XmlElement GetRulesOfPricedItineraryRS(string SID, string SQN, string SCT, string GUID, string ULC)
		{
			XmlElement ReqXml = GetRulesOfPricedItineraryRQ(ULC);
            //cm.XmlFileSave(ReqXml, ac.Name, "GetRulesOfPricedItineraryRQ", "Y", GUID);

			XmlElement ResXml = ac.HttpExecute("GetRulesOfPricedItinerary", ReqXml, SID, SQN, SCT, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "GetRulesOfPricedItineraryRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_GetRulesOfPricedItineraryXml")]
		public XmlElement GetRulesOfPricedItineraryXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("GetRulesOfPricedItinerary", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "GetRulesOfPricedItinerary"

        #region "TravelBoardSearchCommon"

        /// <summary>
        /// MasterPricerTravelBoardSearch / InstantTravelBoardSearch 조회용 RQ 생성
        /// </summary>
        /// <param name="ROOT">Root XmlNode</param>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드(콤마로 구분)</param>
        /// <param name="XAC">제외 항공사 코드(콤마로 구분)</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="CNC">통화코드</param>
        /// <param name="ASI">Agent Sign in</param>
        /// <param name="PUB">PUB운임 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="MTL">ModeTL 지정여부</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <param name="MPIS">MPIS여부</param>
        public void TravelBoardSearchCommon(XmlNode ROOT, int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string CNC, string ASI, string PUB, int WLR, string MTL, int NRR, bool MPIS)
        {
            XmlNode NumberOfUnit = ROOT.SelectSingleNode("numberOfUnit");
            XmlNode PaxReference = ROOT.SelectSingleNode("paxReference");
            XmlNode PaxTypeCode = PaxReference.SelectSingleNode("ptc");
            XmlNode Traveller = PaxReference.SelectSingleNode("traveller");
            XmlNode FareFamilies = ROOT.SelectSingleNode("fareFamilies");
            XmlNode FareOptions = ROOT.SelectSingleNode("fareOptions");
            XmlNode PricingTickInfo = FareOptions.SelectSingleNode("pricingTickInfo");
            XmlNode PricingTicketing = PricingTickInfo.SelectSingleNode("pricingTicketing");
            XmlNode ConversionRate = FareOptions.SelectSingleNode("conversionRate");
            XmlNode TravelFlightInfo = ROOT.SelectSingleNode("travelFlightInfo");
            XmlNode Itinerary = ROOT.SelectSingleNode("itinerary");
            XmlNode OfficeIdDetails = ROOT.SelectSingleNode("officeIdDetails");
            XmlNode NewPaxReference = null;
            XmlNode NewTraveller;

            DateTime NowDate = DateTime.Now;
            string StrNowDate = NowDate.ToString("yyyy-MM-dd");

            //응답 수
            NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='RC']/numberOfUnits").InnerText = NRR.ToString();

            //탑승객 정의
            int PaxCount = 1;
            int TmpAdultCount = 1;
            string PaxType = string.Empty;
            bool Infant = false;

            for (int i = 0; i < PTC.Length; i++)
            {
                if (NOP[i] > 0)
                {
                    PaxType = PTC[i].Trim();

                    NewPaxReference = ROOT.InsertBefore(PaxReference.CloneNode(false), PaxReference);
                    NewPaxReference.AppendChild(PaxTypeCode.Clone());
                    NewPaxReference.SelectSingleNode("ptc").InnerText = Common.ChangePaxType3(PaxType);

                    if (PaxType.Equals("INF"))
                    {
                        for (int n = 0; n < NOP[i]; n++)
                        {
                            NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
                            NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(n + 1);
                            NewTraveller.SelectSingleNode("infantIndicator").InnerText = Convert.ToString((TmpAdultCount / 2) + (TmpAdultCount % 2));
                            TmpAdultCount++;
                        }

                        Infant = true;
                    }
                    else
                    {
                        for (int n = 0; n < NOP[i]; n++)
                        {
                            NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
                            NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
                            NewTraveller.RemoveChild(NewTraveller.SelectSingleNode("infantIndicator"));
                        }

                        //IT운임 조회 추가
                        //if (PaxType.Equals("ADT"))
                        //{
                        //    NewPaxReference.InsertBefore(PaxTypeCode.Clone(), NewPaxReference.SelectSingleNode("traveller")).InnerText = "IIT";
                        //}
                    }
                }
            }

            ROOT.RemoveChild(PaxReference);

            //탑승객 수
            NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='PX']/numberOfUnits").InnerText = (PaxCount - 1).ToString();

            //fare Basis
            if (String.IsNullOrWhiteSpace(FAB))
                ROOT.RemoveChild(FareFamilies);
            else
            {
                FareFamilies.SelectSingleNode("familyInformation/fareFamilyname").InnerText = String.Concat(DLC, ALC);
                FareFamilies.SelectSingleNode("familyCriteria/fareProductDetail/fareBasis").InnerText = FAB;

                XmlNode FamilyCriteria = FareFamilies.SelectSingleNode("familyCriteria");
                XmlNode CarrierId = FamilyCriteria.SelectSingleNode("carrierId");
                XmlNode NewCarrierId;

                foreach (string Airline in SAC.Split(','))
                {
                    NewCarrierId = FamilyCriteria.InsertBefore(CarrierId.CloneNode(false), CarrierId);
                    NewCarrierId.InnerText = Airline.Trim();
                }

                FamilyCriteria.RemoveChild(CarrierId);
            }

            //PUB운임 출력 여부
            if (PUB.Equals("Y"))
            {
                if (PricingTicketing.SelectNodes("priceType[.='RP']").Count.Equals(0))
                {
                    XmlNode NewPriceType = PricingTicketing.AppendChild(PricingTicketing.SelectSingleNode("priceType").CloneNode(false));
                    NewPriceType.InnerText = "RP";
                }
            }
            else
            {
                if (PricingTicketing.SelectNodes("priceType[.='RP']").Count > 0)
                    PricingTicketing.RemoveChild(PricingTicketing.SelectSingleNode("priceType[.='RP']"));
            }

            //학생/노무자 운임등 조회시
            if (PTC[0] != "ADT")
            {
                XmlNode NewPriceType = PricingTicketing.AppendChild(PricingTicketing.SelectSingleNode("priceType").CloneNode(false));
                NewPriceType.InnerText = "PTC";
            }

            //Farebasis가 성인과 다른 유아 운임 조회(유아 포함 조회시 Farebasis가 성인과 더 저렴한 운임 조회)
            if (Infant)
            {
                XmlNode NewPriceType = PricingTicketing.AppendChild(PricingTicketing.SelectSingleNode("priceType").CloneNode(false));
                NewPriceType.InnerText = "PSB";
            }

            //오픈
            if (OPN.Equals("Y"))
            {
                XmlNode NewPriceType = PricingTicketing.AppendChild(PricingTicketing.SelectSingleNode("priceType").CloneNode(false));
                NewPriceType.InnerText = "OPD";
            }

            //통화코드
            if (String.IsNullOrWhiteSpace(CNC))
            {
                FareOptions.RemoveChild(ConversionRate);
            }
            else
            {
                XmlNode NewPriceType = PricingTicketing.AppendChild(PricingTicketing.SelectSingleNode("priceType").CloneNode(false));
                NewPriceType.InnerText = "CUC";

                ConversionRate.SelectSingleNode("conversionRateDetail/currency").InnerText = CNC;
            }

            //좌석클래스
            if (string.IsNullOrWhiteSpace(CCD))
            {
                TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("cabinId"));
            }
            else
            {
                TravelFlightInfo.SelectSingleNode("cabinId/cabin").InnerText = CCD;
            }

            //TL설정
            if (MTL.Equals("Y"))
                PricingTickInfo.SelectSingleNode("ticketingDate/date").InnerText = cm.ModeTL(SNM, "").AddDays(-1).ToString("ddMMyy");
            else
                PricingTickInfo.RemoveChild(PricingTickInfo.SelectSingleNode("ticketingDate"));

            //한국출발여부
            bool DepartureFromKorea = Common.KoreaOfAirport(DLC.Split(',')[0].Trim());

            //항공사 지정
            if (string.IsNullOrWhiteSpace(SAC))
            {
                TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='V']"));
                //TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='M']"));

                //'YY'조건으로 조회 허용(2017-12-08,김지영차장)
                XmlNode CompanyIdentityM = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='M']");
                CompanyIdentityM.RemoveChild(CompanyIdentityM.SelectSingleNode("carrierId[.='']"));
            }
            else
            {
                XmlNode CompanyIdentityV = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='V']");
                XmlNode CompanyIdentityM = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='M']");
                XmlNode CarrierIdV = CompanyIdentityV.SelectSingleNode("carrierId");
                XmlNode CarrierIdM = CompanyIdentityM.SelectSingleNode("carrierId");
                XmlNode NewCarrierIdV;
                XmlNode NewCarrierIdM;
                bool OZInclude = false;

                foreach (string Airline in SAC.Split(','))
                {
                    if (!String.IsNullOrWhiteSpace(Airline))
                    {
                        NewCarrierIdV = CompanyIdentityV.AppendChild(CarrierIdV.CloneNode(false));
                        NewCarrierIdV.InnerText = Airline.Trim();

                        NewCarrierIdM = CompanyIdentityM.AppendChild(CarrierIdM.CloneNode(false));
                        NewCarrierIdM.InnerText = Airline.Trim();

                        //KE,OZ일 경우에만 'YY(결합운임)' 조건으로 검색(2017-11-20,김경미차장)
                        //'YY'조건으로 조회 허용(2017-12-08,김지영차장)
                        //if (Airline != "KE" && Airline != "OZ")
                        //{
                        //    if (CompanyIdentityM.SelectNodes("carrierId[.='YY']").Count > 0)
                        //        CompanyIdentityM.RemoveChild(CompanyIdentityM.SelectSingleNode("carrierId[.='YY']"));
                        //}
                        
                        if (!OZInclude && NewCarrierIdV.InnerText.Equals("OZ"))
                            OZInclude = true;
                    }
                }

                CompanyIdentityV.RemoveChild(CarrierIdV);
                CompanyIdentityM.RemoveChild(CarrierIdM);

                if (CompanyIdentityV.SelectNodes("carrierId").Count > 0)
                {
                    if (!OZInclude)
                        TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='T']"));

                    //if (DepartureFromKorea)
                    //    TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']"));
                }
            }
            
            //해외출발일 경우 LCC 5개사(ZE, 7C, TW, LJ, Z2) 제외
            //해외출발일 경우 중화항공(CI) 제외(2016-11-28,김지영과장)
            //해외출발일 경우 LCC 2개사(7C, TW) 제외 삭제(2017-10-24,김지영차장)
            //해외출발일 경우 필리핀항공(PR) 제외(2019-06-18,김지영팀장)
            if (!DepartureFromKorea && TravelFlightInfo.SelectNodes("companyIdentity[carrierQualifier='W']").Count > 0)
            {
                string LccAirCode = "ZE,LJ,Z2,CI,PR";
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
                XmlNode CarrierId = CompanyIdentity.SelectSingleNode("carrierId");
                XmlNode NewCarrierId;

                foreach (string Airline in LccAirCode.Split(','))
                {
                    NewCarrierId = CompanyIdentity.AppendChild(CarrierId.CloneNode(false));
                    NewCarrierId.InnerText = Airline.Trim();
                }
            }

            //SEL(ICN,GMP) 출발, BKK 도착인 경우 CA항공사 제외(단, ABS는 가능)(2016-08-08,송인혁차장)
            if (SNM != 68 && ALC.Equals("BKK") && (DLC.Equals("SEL") || DLC.Equals("ICN") || DLC.Equals("GMP")))
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
                XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "CA";
            }

            //중국남방항공(CZ)은 중국 국내선 단독 발권 불가로 출도착지가 중국일 경우 검색 제외(2016-10-20,김지영과장)
            if (Common.ChinaOfAirport(DLC) && Common.ChinaOfAirport(ALC))
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
                XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "CZ";
            }

            //닷컴(2,3915) 외에는 HA항공사 검색 제외(2017-02-13,김지영과장)
            //HA항공사 전체 검색 가능(2018-10-15,김경미매니저)
            //if (SNM != 2 && SNM != 3915)
            //{
            //    XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
            //    XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
            //    NewCarrierId.InnerText = "HA";
            //}

            //2016-08-18 18:00 ~ 2016-08-19 12:00 ZE 예약 불가(2016-08-17,김지영과장)
            //if (cm.DateDiff("m", "2016-08-18 17:00", NowDate.ToString("yyyy-MM-dd HH:mm")) > 0 && cm.DateDiff("m", "2016-08-19 12:00", NowDate.ToString("yyyy-MM-dd HH:mm")) <= 0)
            //{
            //    XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
            //    XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
            //    NewCarrierId.InnerText = "ZE";
            //}

            //유아 포함인 경우 티웨이항공(TW) 운임/스케쥴 모두 제외(2019-03-26,김경미매니저)
            if (Infant)
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "TW";
            }

            //출도착 도시중 하바나(HAV)(쿠바) 가 포함된 경우 델타항공(DL)은 운임/스케쥴 모두 제외(2017-06-09,김지영과장)
            if (DLC.IndexOf("HAV") != -1 || ALC.IndexOf("HAV") != -1)
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "DL";

                //TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']/carrierId").InnerText = "DL";
            }
            //else
            //{
            //    TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']"));
            //}

            //출발 4일 이내인 경우 업무시간 외에는 대한항공(KE) 운임/스케쥴 모두 제외(2019-10-22,김지영팀장)
            if (cm.DateDiff("d", StrNowDate, cm.RequestDateTime(DTD.Split(',')[0], "yyyy-MM-dd")) < 4)
            {
                bool ExceptKE = true;
                int NowTime = Convert.ToInt32(NowDate.ToString("HHmm"));

                //업무일 여부
                if (cm.WorkdayYN(StrNowDate))
                {
                    //업무시간 여부
                    if (SNM.Equals(68))
                    {
                        if (NowTime >= 0800 && NowTime < 1830)
                            ExceptKE = false;
                    }
                    else
                    {
                        if (NowTime >= 0800 && NowTime < 1930)
                            ExceptKE = false;
                    }
                }
                else
                {
                    //업무시간 여부
                    if (!SNM.Equals(68))
                    {
                        if (NowTime >= 0830 && NowTime < 1530)
                            ExceptKE = false;
                    }
                }

                if (ExceptKE)
                {
                    XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                    XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                    NewCarrierId.InnerText = "KE";
                }
            }

            //DL,AA 항공은 미국 국내선(US출발-US도착)일 경우 운임이 상이하게 적용되어 검색 제외(2018-08-09,김경미매니저)
            if (Common.UnitedStatesOfAirport(DLC) && Common.UnitedStatesOfAirport(ALC))
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='W']");
                XmlNode NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "AA";

                //DL은 운임/스케쥴 모두 제외(2019-02-08,김경미매니저)
                CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "DL";

                //B6은 운임/스케쥴 모두 제외(2019-02-22,김경미매니저)
                CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                NewCarrierId.InnerText = "B6";
            }

            //항공사 제외(2019-04-22)
            if (!string.IsNullOrWhiteSpace(XAC))
            {
                XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='X']");
                XmlNode NewCarrierId = null;
                
                foreach (string Airline in XAC.Split(','))
                {
                    if (!String.IsNullOrWhiteSpace(Airline))
                    {
                        NewCarrierId = CompanyIdentity.AppendChild(CompanyIdentity.SelectSingleNode("carrierId").CloneNode(false));
                        NewCarrierId.InnerText = Airline.Trim();
                    }
                }
            }

            //제외항공사와 지정항공사의 중복 제거(2018-04-05)
            foreach (XmlNode WCarrier in TravelFlightInfo.SelectNodes("companyIdentity[carrierQualifier='W']/carrierId"))
            {
                string Carrier = WCarrier.InnerText;

                foreach (XmlNode VCarrier in TravelFlightInfo.SelectNodes("companyIdentity[carrierQualifier='V']/carrierId"))
                {
                    if (VCarrier.InnerText.Equals(Carrier))
                        TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='V']").RemoveChild(VCarrier);
                }

                foreach (XmlNode MCarrier in TravelFlightInfo.SelectNodes("companyIdentity[carrierQualifier='M']/carrierId"))
                {
                    if (MCarrier.InnerText.Equals(Carrier))
                        TravelFlightInfo.SelectSingleNode("companyIdentity[carrierQualifier='M']").RemoveChild(MCarrier);
                }
            }

            //대기예약 포함 비율
            if (WLR > 0)
                TravelFlightInfo.SelectSingleNode("unitNumberDetail[typeOfUnit='WL']/numberOfUnits").InnerText = WLR.ToString();
            else
                TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("unitNumberDetail[typeOfUnit='WL']"));

            //상해(SHA)는 공항코드로만 검색(2017-08-17,김경미차장)
            if (ALC.IndexOf("SHA") != -1 && !MPIS)
                ACQ = "S";
            
            //여정의 출발/도착지 코드에 대해 도시 또는 공항코드 지정 사용 여부
            bool UseACQ = String.IsNullOrWhiteSpace(ACQ) ? false : true;

            //여정
            Itinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "1";
            Itinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLC.Split(',')[0].Trim();
            if (UseACQ)
                Itinerary.SelectSingleNode("departureLocalization/departurePoint/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, Itinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText);
            else
                Itinerary.SelectSingleNode("departureLocalization/departurePoint").RemoveChild(Itinerary.SelectSingleNode("departureLocalization/departurePoint/airportCityQualifier"));
            Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALC.Split(',')[0].Trim();
            if (UseACQ)
                Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText);
            else
                Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails").RemoveChild(Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/airportCityQualifier"));
            Itinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(DTD.Split(',')[0].Trim());

            //경유지 및 여정추가옵션
            XmlNode FlightInfo = null;
            XmlNode InclusionDetail = null;
            XmlNode NewInclusionDetail = null;
            string[] IncLocation = CLC.Split(',');

            if (String.IsNullOrWhiteSpace(CLC) && String.IsNullOrWhiteSpace(FLD))
                Itinerary.RemoveChild(Itinerary.SelectSingleNode("flightInfo"));
            else
            {
                //여정추가옵션
                FlightInfo = Itinerary.SelectSingleNode("flightInfo");

                XmlNode FlightDetail = FlightInfo.SelectSingleNode("flightDetail");
                XmlNode FlightType = FlightDetail.SelectSingleNode("flightType");
                XmlNode NewFlightType;

                if (!String.IsNullOrWhiteSpace(FLD))
                {
                    foreach (string TmpFLD in FLD.Split(','))
                    {
                        if (!String.IsNullOrWhiteSpace(TmpFLD))
                        {
                            NewFlightType = FlightDetail.AppendChild(FlightType.CloneNode(false));
                            NewFlightType.InnerText = TmpFLD.Trim();
                        }
                    }
                }

                FlightDetail.RemoveChild(FlightType);

                if (!FlightDetail.HasChildNodes)
                    FlightInfo.RemoveChild(FlightDetail);
            }

            if (ROT.Equals("RT"))
            {
                XmlNode NewItinerary = ROOT.InsertBefore(Itinerary.CloneNode(true), OfficeIdDetails);

                NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "2";
                NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = ALC;
                if (NewItinerary.SelectNodes("departureLocalization/departurePoint/airportCityQualifier").Count > 0)
                    NewItinerary.SelectSingleNode("departureLocalization/departurePoint/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, ALC);
                NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = DLC;
                if (NewItinerary.SelectNodes("arrivalLocalization/arrivalPointDetails/airportCityQualifier").Count > 0)
                    NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, DLC);
                NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(ARD);

                if (NewItinerary.SelectNodes("flightInfo").Count > 0)
                {
                    //경유지
                    FlightInfo = NewItinerary.SelectSingleNode("flightInfo");
                    InclusionDetail = FlightInfo.SelectSingleNode("inclusionDetail");

                    if (IncLocation.Length > 1 && !String.IsNullOrWhiteSpace(IncLocation[1]))
                    {
                        foreach (string TmpCLC in IncLocation[1].Split('/'))
                        {
                            NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
                            NewInclusionDetail.SelectSingleNode("locationId").InnerText = TmpCLC.Trim();
                            if (UseACQ)
                                NewInclusionDetail.SelectSingleNode("airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewInclusionDetail.SelectSingleNode("locationId").InnerText);
                            else
                                NewInclusionDetail.RemoveChild(NewInclusionDetail.SelectSingleNode("airportCityQualifier"));
                        }
                    }

                    FlightInfo.RemoveChild(InclusionDetail);

                    if (!NewItinerary.SelectSingleNode("flightInfo").HasChildNodes)
                        NewItinerary.RemoveChild(NewItinerary.SelectSingleNode("flightInfo"));
                }
            }
            else if (ROT.Equals("DT"))
            {
                XmlNode NewItinerary = ROOT.InsertBefore(Itinerary.CloneNode(true), OfficeIdDetails);

                NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "2";
                NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLC.Split(',')[1].Trim();
                if (NewItinerary.SelectNodes("departureLocalization/departurePoint/airportCityQualifier").Count > 0)
                    NewItinerary.SelectSingleNode("departureLocalization/departurePoint/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText);
                NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALC.Split(',')[1].Trim();
                if (NewItinerary.SelectNodes("arrivalLocalization/arrivalPointDetails/airportCityQualifier").Count > 0)
                    NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText);
                NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(ARD);

                if (NewItinerary.SelectNodes("flightInfo").Count > 0)
                {
                    //경유지
                    FlightInfo = NewItinerary.SelectSingleNode("flightInfo");
                    InclusionDetail = FlightInfo.SelectSingleNode("inclusionDetail");

                    if (IncLocation.Length > 1 && !String.IsNullOrWhiteSpace(IncLocation[1]))
                    {
                        foreach (string TmpCLC in IncLocation[1].Split('/'))
                        {
                            NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
                            NewInclusionDetail.SelectSingleNode("locationId").InnerText = TmpCLC.Trim();
                            if (UseACQ)
                                NewInclusionDetail.SelectSingleNode("airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewInclusionDetail.SelectSingleNode("locationId").InnerText);
                            else
                                NewInclusionDetail.RemoveChild(NewInclusionDetail.SelectSingleNode("airportCityQualifier"));
                        }
                    }

                    FlightInfo.RemoveChild(InclusionDetail);

                    if (!NewItinerary.SelectSingleNode("flightInfo").HasChildNodes)
                        NewItinerary.RemoveChild(NewItinerary.SelectSingleNode("flightInfo"));
                }
            }
            else if (ROT.Equals("MD"))
            {
                string[] DLCs = DLC.Split(',');
                string[] ALCs = ALC.Split(',');
                string[] DTDs = DTD.Split(',');

                for (int i = 1; i < DLCs.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(DLCs[i]))
                    {
                        XmlNode NewItinerary = ROOT.InsertBefore(Itinerary.CloneNode(true), OfficeIdDetails);

                        NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = (i + 1).ToString();
                        NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLCs[i].Trim();
                        if (NewItinerary.SelectNodes("departureLocalization/departurePoint/airportCityQualifier").Count > 0)
                            NewItinerary.SelectSingleNode("departureLocalization/departurePoint/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText);
                        NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALCs[i].Trim();
                        if (NewItinerary.SelectNodes("arrivalLocalization/arrivalPointDetails/airportCityQualifier").Count > 0)
                            NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText);
                        NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(DTDs[i].Trim());

                        if (NewItinerary.SelectNodes("flightInfo").Count > 0)
                        {
                            //경유지
                            FlightInfo = NewItinerary.SelectSingleNode("flightInfo");
                            InclusionDetail = FlightInfo.SelectSingleNode("inclusionDetail");

                            if (IncLocation.Length > i && !String.IsNullOrWhiteSpace(IncLocation[i]))
                            {
                                foreach (string TmpCLC in IncLocation[i].Split('/'))
                                {
                                    NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
                                    NewInclusionDetail.SelectSingleNode("locationId").InnerText = TmpCLC.Trim();
                                    if (UseACQ)
                                        NewInclusionDetail.SelectSingleNode("airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewInclusionDetail.SelectSingleNode("locationId").InnerText);
                                    else
                                        NewInclusionDetail.RemoveChild(NewInclusionDetail.SelectSingleNode("airportCityQualifier"));
                                }
                            }

                            FlightInfo.RemoveChild(InclusionDetail);

                            if (!NewItinerary.SelectSingleNode("flightInfo").HasChildNodes)
                                NewItinerary.RemoveChild(NewItinerary.SelectSingleNode("flightInfo"));
                        }
                    }
                }
            }

            //출발여정의 경유지 설정
            if (Itinerary.SelectNodes("flightInfo").Count > 0)
            {
                FlightInfo = Itinerary.SelectSingleNode("flightInfo");
                InclusionDetail = FlightInfo.SelectSingleNode("inclusionDetail");

                if (IncLocation.Length > 0 && !String.IsNullOrWhiteSpace(IncLocation[0]))
                {
                    foreach (string TmpCLC in IncLocation[0].Split('/'))
                    {
                        NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
                        NewInclusionDetail.SelectSingleNode("locationId").InnerText = TmpCLC.Trim();
                        if (UseACQ)
                            NewInclusionDetail.SelectSingleNode("airportCityQualifier").InnerText = Common.CheckAirportCity(ACQ, NewInclusionDetail.SelectSingleNode("locationId").InnerText);
                        else
                            NewInclusionDetail.RemoveChild(NewInclusionDetail.SelectSingleNode("airportCityQualifier"));
                    }
                }

                FlightInfo.RemoveChild(InclusionDetail);

                if (!Itinerary.SelectSingleNode("flightInfo").HasChildNodes)
                    Itinerary.RemoveChild(Itinerary.SelectSingleNode("flightInfo"));
            }

            //OID 상세 설정
            if (String.IsNullOrWhiteSpace(ASI))
            {
                ROOT.RemoveChild(OfficeIdDetails);
            }
            else
            {
                OfficeIdDetails.SelectSingleNode("officeIdInformation/officeIdentification/agentSignin").InnerText = ASI;
            }
        }

        #endregion "TravelBoardSearchCommon"

        #region "InstantTravelBoardSearch"

        [WebMethod(Description = "Fare_InstantTravelBoardSearchhRQ")]
        public XmlElement InstantTravelBoardSearchRQ(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string CNC, string ASI, string PUB, int WLR, string MTL, int NRR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("InstantTravelBoardSearchRQ"));

            TravelBoardSearchCommon(XmlDoc.SelectSingleNode("Fare_InstantTravelBoardSearch"), SNM, SAC, "", DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, CNC, ASI, PUB, WLR, MTL, NRR, true);

            //MPIS에서 필요없는 조건 삭제
            XmlNode ITBS = XmlDoc.SelectSingleNode("Fare_InstantTravelBoardSearch");
            XmlNode NumberOfUnit = ITBS.SelectSingleNode("numberOfUnit");

            ITBS.RemoveChild(ITBS.SelectSingleNode("fareOptions"));
            ITBS.RemoveChild(ITBS.SelectSingleNode("travelFlightInfo"));
            
            NumberOfUnit.RemoveChild(NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='RC']"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// InstantTravelBoardSearch 조회
        /// </summary>
        /// <param name="GUID">고유번호</param>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드(콤마로 구분)</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PUB">PUB운임 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="MTL">ModeTL 지정여부</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare_InstantTravelBoardSearchRS")]
        public XmlElement InstantTravelBoardSearchRS(string GUID, int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string PUB, int WLR, string MTL, int NRR)
        {
            XmlElement ReqXml = InstantTravelBoardSearchRQ(SNM, SAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, "", "", PUB, WLR, MTL, NRR);
            //cm.XmlFileSave(ReqXml, ac.Name, "InstantTravelBoardSearchRQ", DBSave, GUID);

            XmlElement ResXml = ac.HttpExecuteSoapHeader4("InstantTravelBoardSearch", "SELK138AB", ReqXml, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "InstantTravelBoardSearchRS", "N", GUID);

            return ResXml;
        }

        #endregion "InstantTravelBoardSearch"

        #region "MasterPricerTravelBoardSearch"

        [WebMethod(Description = "Fare_MasterPricerTravelBoardSearchRQ")]
        public XmlElement MasterPricerTravelBoardSearchRQ(int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string PUB, int WLR, string MTL, int NRR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("MasterPricerTravelBoardSearchRQ"));

            TravelBoardSearchCommon(XmlDoc.SelectSingleNode("Fare_MasterPricerTravelBoardSearch"), SNM, SAC, XAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, "", "", PUB, WLR, MTL, NRR, false);

            return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// MasterPricerTravelBoardSearch 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="SNM">사이트 번호</param>
		/// <param name="SAC">항공사 코드(콤마로 구분)</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="OPN">오픈여부(YN)</param>
		/// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
		/// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
		/// <param name="PTC">탑승객 타입 코드</param>
		/// <param name="NOP">탑승객 수</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시)</param>
        /// <param name="FAB">Fare Basis</param>
		/// <param name="PUB">PUB운임 출력여부</param>
		/// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="MTL">ModeTL 지정여부</param>
		/// <param name="NRR">응답 결과 수</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_MasterPricerTravelBoardSearchRS")]
        public XmlElement MasterPricerTravelBoardSearchRS(string SID, string SQN, string SCT, string GUID, int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string PUB, int WLR, string MTL, int NRR)
        {
            //로그 DB 저장을 닷컴에 한해서만 진행(2017-02-06,고재영)
            string DBSave = (SNM.Equals(2) || SNM.Equals(3915)) ? "Y" : "N";

            XmlElement ReqXml = MasterPricerTravelBoardSearchRQ(SNM, SAC, "", DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, PUB, WLR, MTL, NRR);
            cm.XmlFileSave(ReqXml, ac.Name, "MasterPricerTravelBoardSearchRQ", DBSave, GUID);

            XmlElement ResXml = ac.HttpExecute("MasterPricerTravelBoardSearch", ReqXml, SID, SQN, SCT, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "MasterPricerTravelBoardSearchRS", DBSave, GUID);

            return ResXml;
        }

		[WebMethod(Description = "Fare_MasterPricerTravelBoardSearchRS")]
		public XmlElement MasterPricerTravelBoardSearchRSOnly(string SID, string SQN, string SCT, XmlElement ReqXml)
		{
			return ac.Execute("MasterPricerTravelBoardSearch", ReqXml, SID, SQN, SCT);
		}

		[WebMethod(Description = "Fare_MasterPricerTravelBoardSearchXml")]
		public XmlElement MasterPricerTravelBoardSearchXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("MasterPricerTravelBoardSearch", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "MasterPricerTravelBoardSearch"

        #region "MasterPricerTravelBoardSearch(4.0)"

        [WebMethod(Description = "Fare_MasterPricerTravelBoardSearchRQ(4.0)")]
        public XmlElement MasterPricerTravelBoardSearch4RQ(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string PUB, int WLR, string MTL, int NRR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("MasterPricerTravelBoardSearchRQ"));

            TravelBoardSearchCommon(XmlDoc.SelectSingleNode("Fare_MasterPricerTravelBoardSearch"), SNM, SAC, "", DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, "", "", PUB, WLR, MTL, NRR, false);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// MasterPricerTravelBoardSearch 조회(4.0)
        /// </summary>
        /// <param name="GUID">고유번호</param>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드(콤마로 구분)</param>
        /// <param name="XAC">제외 항공사 코드(콤마로 구분)</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PUB">PUB운임 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="MTL">ModeTL 지정여부</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare_MasterPricerTravelBoardSearchRS(4.0)")]
        public XmlElement MasterPricerTravelBoardSearch4RS(string GUID, int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string[] PTC, int[] NOP, string ACQ, string FAB, string PUB, int WLR, string MTL, int NRR)
        {
            XmlElement ReqXml = MasterPricerTravelBoardSearchRQ(SNM, SAC, XAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, PUB, WLR, MTL, NRR);
            //cm.XmlFileSave(ReqXml, ac.Name, "MasterPricerTravelBoardSearchRQ", "N", GUID);

            XmlElement ResXml = ac.HttpExecuteSoapHeader4("MasterPricerTravelBoardSearch", OfficeId(SNM), ReqXml, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "MasterPricerTravelBoardSearchRS", "N", GUID);

            return ResXml;
        }

        #endregion "MasterPricerTravelBoardSearch(4.0)"

        #region "MasterPricerCalendar"

        [WebMethod(Description = "Fare_MasterPricerCalendarRQ")]
		public XmlElement MasterPricerCalendarRQ(string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string CCD, int ADC, int CHC, int IFC, int NRR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("MasterPricerCalendarRQ"));

			XmlNode MPTBS = XmlDoc.SelectSingleNode("Fare_MasterPricerCalendar");
			XmlNode NumberOfUnit = MPTBS.SelectSingleNode("numberOfUnit");
			XmlNode PaxReferenceADT = MPTBS.SelectSingleNode("paxReference[ptc!='CH' and ptc!='INF']");
			XmlNode PaxReferenceCHD = MPTBS.SelectSingleNode("paxReference[ptc='CH']");
			XmlNode PaxReferenceINF = MPTBS.SelectSingleNode("paxReference[ptc='INF']");
			XmlNode TravelFlightInfo = MPTBS.SelectSingleNode("travelFlightInfo");
			XmlNode Itinerary = MPTBS.SelectSingleNode("itinerary");

			//응답 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='RC']/numberOfUnits").InnerText = NRR.ToString();

			//탑승객 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='PX']/numberOfUnits").InnerText = (ADC + CHC).ToString();

			//탑승객 정의
			XmlNode Traveller;
			XmlNode NewTraveller;

			int PaxCount = 1;
			int TmpAdultCount = 1;

			if (ADC > 0)
			{
				Traveller = PaxReferenceADT.SelectSingleNode("traveller");

				for (int i = 0; i < ADC; i++)
				{
					NewTraveller = PaxReferenceADT.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
				}

				PaxReferenceADT.RemoveChild(Traveller);
			}
			else
			{
				MPTBS.RemoveChild(PaxReferenceADT);
			}

			if (CHC > 0)
			{
				Traveller = PaxReferenceCHD.SelectSingleNode("traveller");

				for (int i = 0; i < CHC; i++)
				{
					NewTraveller = PaxReferenceCHD.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
				}

				PaxReferenceCHD.RemoveChild(Traveller);
			}
			else
			{
				MPTBS.RemoveChild(PaxReferenceCHD);
			}

			if (IFC > 0)
			{
				Traveller = PaxReferenceINF.SelectSingleNode("traveller");

				for (int i = 0; i < IFC; i++)
				{
					NewTraveller = PaxReferenceINF.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(i + 1);
					NewTraveller.SelectSingleNode("infantIndicator").InnerText = Convert.ToString((TmpAdultCount / 2) + (TmpAdultCount % 2));
					TmpAdultCount++;
				}

				PaxReferenceINF.RemoveChild(Traveller);
			}
			else
			{
				MPTBS.RemoveChild(PaxReferenceINF);
			}

			//좌석클래스
			if (string.IsNullOrWhiteSpace(CCD))
			{
				TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("cabinId"));
			}
			else
			{
				TravelFlightInfo.SelectSingleNode("cabinId/cabin").InnerText = CCD;
			}

			//항공사 지정
			if (string.IsNullOrWhiteSpace(SAC))
			{
				TravelFlightInfo.RemoveChild(TravelFlightInfo.SelectSingleNode("companyIdentity"));
			}
			else
			{
				XmlNode CompanyIdentity = TravelFlightInfo.SelectSingleNode("companyIdentity");
				XmlNode CarrierId = CompanyIdentity.SelectSingleNode("carrierId");
				XmlNode NewCarrierId;

				foreach (string Airline in SAC.Split(','))
				{
					NewCarrierId = CompanyIdentity.AppendChild(CarrierId.CloneNode(false));
					NewCarrierId.InnerText = Airline.Trim();
				}

				CompanyIdentity.RemoveChild(CarrierId);
			}

			//여정
			Itinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "1";
			Itinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLC;
			Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALC;
			Itinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(DTD);

			if (ROT.Equals("RT"))
			{
				XmlNode NewItinerary = MPTBS.AppendChild(Itinerary.CloneNode(true));

				NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "2";
				NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = ALC;
				NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = DLC;
				NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(ARD);
			}

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// MasterPricerCalendar 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="SAC">항공사 코드(콤마로 구분)</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="NRR">응답 결과 수</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_MasterPricerCalendarRS")]
		public XmlElement MasterPricerCalendarRS(string SID, string SQN, string SCT, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string CCD, int ADC, int CHC, int IFC, int NRR)
		{
			return ac.Execute("MasterPricerCalendar", MasterPricerCalendarRQ(SAC, DLC, ALC, ROT, DTD, ARD, CCD, ADC, CHC, IFC, NRR), SID, SQN, SCT);
		}

		[WebMethod(Description = "Fare_MasterPricerCalendarXml")]
		public XmlElement MasterPricerCalendarXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("MasterPricerCalendar", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "MasterPricerCalendar"

		#region "InformativeBestPricingWithoutPNR"

		[WebMethod(Description = "Fare_InformativeBestPricingWithoutPNRRQ")]
		public XmlElement InformativeBestPricingWithoutPNRRQ()
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("InformativeBestPricingWithoutPNRRQ"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 최저가 운임 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_InformativeBestPricingWithoutPNRRS")]
		public XmlElement InformativeBestPricingWithoutPNRRS(string SID, string SQN, string SCT, string GUID)
		{
			XmlElement ReqXml = InformativeBestPricingWithoutPNRRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "InformativeBestPricingWithoutPNRRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("InformativeBestPricingWithoutPNR", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "InformativeBestPricingWithoutPNRRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_InformativeBestPricingWithoutPNRXml")]
		public XmlElement InformativeBestPricingWithoutPNRXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("InformativeBestPricingWithoutPNR", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "InformativeBestPricingWithoutPNR"

		#region "InformativePricingWithoutPNR"

		[WebMethod(Description = "Fare_InformativePricingWithoutPNRRQ")]
		public XmlElement InformativePricingWithoutPNRRQ(string FAT, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string[] PTC, int[] NOP)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("InformativePricingWithoutPNRRQ"));

			XmlNode Root = XmlDoc.SelectSingleNode("Fare_InformativePricingWithoutPNR");
			XmlNode PassengersGroup = Root.SelectSingleNode("passengersGroup");
			XmlNode SegmentGroup = Root.SelectSingleNode("segmentGroup");
            XmlNode PricingOptionGroup = Root.SelectSingleNode("pricingOptionGroup");

			XmlNode TravellersID;
			XmlNode TravellerDetails;
			XmlNode NewPassengersGroup;
			XmlNode NewTravellerDetails;
            XmlNode NewPricingOptionGroup;
			int AdultCount = 0;

			for (int i = 0; i < PTC.Length; i++)
			{
				if (NOP[i] > 0)
				{
                    NewPassengersGroup = Root.InsertBefore(PassengersGroup.CloneNode(true), PricingOptionGroup);
					TravellersID = NewPassengersGroup.SelectSingleNode("travellersID");
					TravellerDetails = TravellersID.SelectSingleNode("travellerDetails");

					NewPassengersGroup.SelectSingleNode("discountPtc/valueQualifier").InnerText = Common.ChangePaxType3(PTC[i]);
					if (PTC[i].Equals("INF"))
						NewPassengersGroup.SelectSingleNode("discountPtc/fareDetails/qualifier").InnerText = "766";
					else
						NewPassengersGroup.SelectSingleNode("discountPtc").RemoveChild(NewPassengersGroup.SelectSingleNode("discountPtc/fareDetails"));

					NewPassengersGroup.SelectSingleNode("segmentRepetitionControl/segmentControlDetails/quantity").InnerText = (i + 1).ToString();
					NewPassengersGroup.SelectSingleNode("segmentRepetitionControl/segmentControlDetails/numberOfUnits").InnerText = NOP[i].ToString();

					if (PTC[i].Equals("ADT"))
						AdultCount = NOP[i];

					if (PTC[i].Equals("CH") || PTC[i].Equals("CHD"))
					{
						for (int n = 1; n <= NOP[i]; n++)
						{
							NewTravellerDetails = TravellersID.AppendChild(TravellerDetails.CloneNode(true));
							NewTravellerDetails.SelectSingleNode("measurementValue").InnerText = (AdultCount + n).ToString();
						}
					}
					else
					{
						for (int n = 1; n <= NOP[i]; n++)
						{
							NewTravellerDetails = TravellersID.AppendChild(TravellerDetails.CloneNode(true));
							NewTravellerDetails.SelectSingleNode("measurementValue").InnerText = n.ToString();
						}
					}

					TravellersID.RemoveChild(TravellerDetails);
				}
			}

			Root.RemoveChild(PassengersGroup);

			XmlNode NewSegmentGroup;

			for (int i = 0; i < DTD.Length; i++)
			{
                NewSegmentGroup = Root.InsertBefore(SegmentGroup.CloneNode(true), PricingOptionGroup);
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/departureDate").InnerText = cm.ConvertToAmadeusDate(DTD[i].Trim());
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/departureTime").InnerText = cm.ConvertToAmadeusTime(DTT[i].Trim());
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/arrivalDate").InnerText = cm.ConvertToAmadeusDate(ARD[i].Trim());
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/arrivalTime").InnerText = cm.ConvertToAmadeusTime(ART[i].Trim());
				NewSegmentGroup.SelectSingleNode("segmentInformation/boardPointDetails/trueLocationId").InnerText = DLC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/offpointDetails/trueLocationId").InnerText = ALC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/companyDetails/marketingCompany").InnerText = MCC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/companyDetails/operatingCompany").InnerText = (String.IsNullOrWhiteSpace(OCC[i])) ? MCC[i].Trim() : OCC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightIdentification/flightNumber").InnerText = FLN[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightIdentification/bookingClass").InnerText = RBD[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightTypeDetails/flightIndicator").InnerText = INO[i].ToString();
				NewSegmentGroup.SelectSingleNode("segmentInformation/itemNumber").InnerText = (i + 1).ToString();
			}

			Root.RemoveChild(SegmentGroup);

            if (!String.IsNullOrWhiteSpace(FAT))
            {
                foreach (string TmpFAT in FAT.Split(','))
                {
                    NewPricingOptionGroup = Root.AppendChild(PricingOptionGroup.CloneNode(true));
                    NewPricingOptionGroup.SelectSingleNode("pricingOptionKey/pricingOptionKey").InnerText = TmpFAT.TrimEnd();
                }

                Root.RemoveChild(PricingOptionGroup);
            }

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 최저가 운임 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="FAT">Fare Type(RP/RU)</param>
		/// <param name="INO">여정번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="DTT">출발시간(HHMM)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="ART">도착시간(HHMM)</param>
		/// <param name="DLC">출발지</param>
		/// <param name="ALC">도착지</param>
		/// <param name="MCC">마케팅항공사</param>
		/// <param name="OCC">운항항공사</param>
		/// <param name="FLN">편명</param>
		/// <param name="RBD">좌석클래스</param>
		/// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_InformativePricingWithoutPNRRS")]
        public XmlElement InformativePricingWithoutPNRRS(string SID, string SQN, string SCT, string GUID, string FAT, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string[] PTC, int[] NOP)
		{
			XmlElement ReqXml = InformativePricingWithoutPNRRQ(FAT, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PTC, NOP);
            cm.XmlFileSave(ReqXml, ac.Name, "InformativePricingWithoutPNRRQ", "Y", GUID);
				
			XmlElement ResXml = ac.Execute("InformativePricingWithoutPNR", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "InformativePricingWithoutPNRRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_InformativePricingWithoutPNRXml")]
		public XmlElement InformativePricingWithoutPNRXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("InformativePricingWithoutPNR", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "InformativePricingWithoutPNR"

		#region "InformativePricingWithoutPNR(Rule)"

		[WebMethod(Description = "Fare_InformativePricingWithoutPNRRuleRQ")]
		public XmlElement InformativePricingWithoutPNRRuleRQ(int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string PFB, XmlElement PFG)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("InformativePricingWithoutPNRRuleRQ"));

			XmlNode Root = XmlDoc.SelectSingleNode("Fare_InformativePricingWithoutPNR");
			XmlNode CorporateFares = Root.SelectSingleNode("corporateFares");
			XmlNode PassengersGroup = Root.SelectSingleNode("passengersGroup");
			XmlNode PricingOptionsGroup = Root.SelectSingleNode("pricingOptionsGroup");
			XmlNode SegmentRepetitionControl = PassengersGroup.SelectSingleNode("segmentRepetitionControl");
			XmlNode TravellersID = PassengersGroup.SelectSingleNode("travellersID");
			XmlNode TravellerDetails = TravellersID.SelectSingleNode("travellerDetails");
			XmlNode PtcGroup = PassengersGroup.SelectSingleNode("ptcGroup");
			XmlNode TripsGroup = Root.SelectSingleNode("tripsGroup");
			XmlNode SegmentGroup = TripsGroup.SelectSingleNode("segmentGroup");
			XmlNode NewPassengersGroup = null;
			XmlNode NewSegmentRepetitionControl = null;

			int PaxTypeEtc = 0;
			int FareTypeRP = 0;
			int FareTypeRU = 0;
            int FareTypeRW = 0;
			int idx = 1;

			//운임타입
			foreach (XmlNode FareType in PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/fare/fareType"))
			{
				if (FareType.InnerText.Equals("RP"))
					FareTypeRP++;
                else if (FareType.InnerText.Equals("RB") || FareType.InnerText.Equals("RC") || FareType.InnerText.Equals("RX") || FareType.InnerText.Equals("RZ"))
                    FareTypeRW++;
                else
					FareTypeRU++;
			}

            if (FareTypeRP.Equals(0))
                CorporateFares.RemoveChild(CorporateFares.SelectSingleNode("corporateFareIdentifiers[fareQualifier='P']"));

            if (FareTypeRU.Equals(0) && FareTypeRW.Equals(0))
                CorporateFares.RemoveChild(CorporateFares.SelectSingleNode("corporateFareIdentifiers[fareQualifier='U']"));
            else
            {
                if (FareTypeRW.Equals(0) && FareTypeRU > 0)
                    CorporateFares.SelectSingleNode("corporateFareIdentifiers[fareQualifier='U']").RemoveChild(CorporateFares.SelectSingleNode("corporateFareIdentifiers[fareQualifier='U']/corporateID"));
                else
                    CorporateFares.SelectSingleNode("corporateFareIdentifiers[fareQualifier='U']/corporateID").InnerText = (PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/corporateId").Count > 0) ? PFG.SelectSingleNode("paxFare/segFareGroup/segFare/fare/corporateId").InnerText : "";
            }

			//탑승객
			foreach (XmlNode PaxFare in PFG.SelectNodes("paxFare"))
			{
				NewPassengersGroup = Root.InsertBefore(PassengersGroup.CloneNode(false), PassengersGroup);
				NewSegmentRepetitionControl = NewPassengersGroup.AppendChild(SegmentRepetitionControl.CloneNode(true));
				NewSegmentRepetitionControl.SelectSingleNode("segmentControlDetails/quantity").InnerText = (idx++).ToString();
				NewSegmentRepetitionControl.SelectSingleNode("segmentControlDetails/numberOfUnits").InnerText = "0";
				NewPassengersGroup.AppendChild(TravellersID.CloneNode(false));

				foreach (XmlNode TravelerRef in PaxFare.SelectNodes("traveler/ref"))
				{
					NewSegmentRepetitionControl.SelectSingleNode("segmentControlDetails/numberOfUnits").InnerText = (cm.RequestInt(NewSegmentRepetitionControl.SelectSingleNode("segmentControlDetails/numberOfUnits").InnerText) + 1).ToString();
					NewPassengersGroup.SelectSingleNode("travellersID").AppendChild(TravellerDetails.CloneNode(true)).SelectSingleNode("measurementValue").InnerText = TravelerRef.InnerText;
				}

				NewPassengersGroup.AppendChild(PtcGroup.CloneNode(true)).SelectSingleNode("discountPtc/valueQualifier").InnerText = PaxFare.SelectSingleNode("segFareGroup/segFare/fare/fare").Attributes.GetNamedItem("ptc").InnerText;

				//switch (PaxFare.SelectSingleNode("segFareGroup/segFare/fare/fare").Attributes.GetNamedItem("ptc").InnerText)
                switch (PaxFare.Attributes.GetNamedItem("ptc").InnerText)
				{
					case "ADT":
					case "CHD":
                    case "CH":
						NewPassengersGroup.SelectSingleNode("ptcGroup/discountPtc").RemoveChild(NewPassengersGroup.SelectSingleNode("ptcGroup/discountPtc/fareDetails")); 
						break;
					case "INF":
                    case "IN":
                        break;
					default: 
						PaxTypeEtc++;
						NewPassengersGroup.SelectSingleNode("ptcGroup/discountPtc").RemoveChild(NewPassengersGroup.SelectSingleNode("ptcGroup/discountPtc/fareDetails")); 
						break;
				}
			}

			Root.RemoveChild(PassengersGroup);

			//탑승객 타입이 ADT가 아닐경우
			if (PaxTypeEtc.Equals(0))
				Root.RemoveChild(PricingOptionsGroup);

			//여정정보
			TripsGroup.SelectSingleNode("originDestination/origin").InnerText = DLC[0];
			TripsGroup.SelectSingleNode("originDestination/destination").InnerText = ALC[0];

			XmlNode NewSegmentGroup;
			XmlNode NewPricePsgByFareBasisInfo;
			int ItemNum = 1;
			idx = 1;

			String[,] IDENumber = new String[3, 3];
			int PTCNum = 0;

			for (int i = 0; i < DTD.Length; i++)
			{
				if (i > 0 && (INO[i] != INO[(i - 1)]))
					ItemNum = 1;

				NewSegmentGroup = TripsGroup.AppendChild(SegmentGroup.CloneNode(true));
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/departureDate").InnerText = cm.ConvertToAmadeusDate(DTD[i].Trim());

				if (String.IsNullOrWhiteSpace(DTT[i]))
					NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate").RemoveChild(NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/departureTime"));
				else
					NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/departureTime").InnerText = cm.ConvertToAmadeusTime(DTT[i].Trim());

				NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/arrivalDate").InnerText = cm.ConvertToAmadeusDate(ARD[i].Trim());

				if (String.IsNullOrWhiteSpace(ART[i]))
					NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate").RemoveChild(NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/arrivalTime"));
				else
					NewSegmentGroup.SelectSingleNode("segmentInformation/flightDate/arrivalTime").InnerText = cm.ConvertToAmadeusTime(ART[i].Trim());

				NewSegmentGroup.SelectSingleNode("segmentInformation/boardPointDetails/trueLocationId").InnerText = DLC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/offpointDetails/trueLocationId").InnerText = ALC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/companyDetails/marketingCompany").InnerText = MCC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/companyDetails/operatingCompany").InnerText = (String.IsNullOrWhiteSpace(OCC[i])) ? MCC[i].Trim() : OCC[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightIdentification/flightNumber").InnerText = (String.IsNullOrWhiteSpace(FLN[i])) ? "OPEN" : FLN[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightIdentification/bookingClass").InnerText = RBD[i].Trim();
				NewSegmentGroup.SelectSingleNode("segmentInformation/flightTypeDetails/flightIndicator").InnerText = INO[i].ToString();
				NewSegmentGroup.SelectSingleNode("segmentInformation/itemNumber").InnerText = (ItemNum++).ToString();

				XmlNode PricePsgByFareBasisInfo = NewSegmentGroup.SelectSingleNode("pricePsgByFareBasisInfo");
				
				if (PFB.Equals("Y"))
				{
					foreach (XmlNode PaxFare in PFG.SelectNodes("paxFare"))
					{
						NewPricePsgByFareBasisInfo = NewSegmentGroup.InsertBefore(PricePsgByFareBasisInfo.CloneNode(true), PricePsgByFareBasisInfo);
						TravellerDetails = NewPricePsgByFareBasisInfo.SelectSingleNode("specificTravellerDetails/travellerDetails");

						foreach (XmlNode TravelerRef in PaxFare.SelectNodes("traveler/ref"))
						{
							NewPricePsgByFareBasisInfo.SelectSingleNode("specificTravellerDetails").AppendChild(TravellerDetails.CloneNode(true)).SelectSingleNode("measurementValue").InnerText = TravelerRef.InnerText;
						}

						NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/pricingByFareBasisInfo/discountDetails/rateCategory").InnerText = PaxFare.SelectNodes("segFareGroup/segFare/fare")[i].SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText;
						NewPricePsgByFareBasisInfo.SelectSingleNode("specificTravellerDetails").RemoveChild(TravellerDetails);

						if (!PaxFare.SelectSingleNode("segFareGroup/segFare/fare/fare").Attributes.GetNamedItem("ptc").InnerText.Equals("IN"))
							NewPricePsgByFareBasisInfo.SelectSingleNode("segmentRepetitionControl").RemoveAll();

						switch (PaxFare.Attributes.GetNamedItem("ptc").InnerText)
						{
							case "CHD": PTCNum = 1; break;
							case "INF": PTCNum = 2; break;
							default: PTCNum = 0; break;
						}

						if (String.IsNullOrWhiteSpace(IDENumber[PTCNum, 0]))
						{
							NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/fareInfo/identityNumber").InnerText = idx.ToString();
						
							IDENumber[PTCNum, 0] = NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/pricingByFareBasisInfo/discountDetails/rateCategory").InnerText;
							IDENumber[PTCNum, 1] = (idx++).ToString();
							IDENumber[PTCNum, 2] = PaxFare.SelectNodes("segFareGroup/segFare/fare")[i].Attributes.GetNamedItem("bpt").InnerText;
						}
						else
						{
							if (IDENumber[PTCNum, 0].Equals(NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/pricingByFareBasisInfo/discountDetails/rateCategory").InnerText))
							{
								if (IDENumber[PTCNum, 2].Equals("Y"))
								{
									NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/fareInfo/identityNumber").InnerText = idx.ToString();

									IDENumber[PTCNum, 1] = (idx++).ToString();
									IDENumber[PTCNum, 2] = PaxFare.SelectNodes("segFareGroup/segFare/fare")[i].Attributes.GetNamedItem("bpt").InnerText;
								}
								else
								{
									NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/fareInfo/identityNumber").InnerText = IDENumber[PTCNum, 1];
									IDENumber[PTCNum, 2] = PaxFare.SelectNodes("segFareGroup/segFare/fare")[i].Attributes.GetNamedItem("bpt").InnerText;
								}
							}
							else
							{
								NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/fareInfo/identityNumber").InnerText = (++idx).ToString();

								IDENumber[PTCNum, 0] = NewPricePsgByFareBasisInfo.SelectSingleNode("requestedPricingInfo/pricingByFareBasisInfo/discountDetails/rateCategory").InnerText;
								IDENumber[PTCNum, 1] = idx.ToString();
								IDENumber[PTCNum, 2] = PaxFare.SelectNodes("segFareGroup/segFare/fare")[i].Attributes.GetNamedItem("bpt").InnerText;
							}
						}
					}
				}

				NewSegmentGroup.RemoveChild(PricePsgByFareBasisInfo);
			}

			TripsGroup.RemoveChild(SegmentGroup);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 최저가 운임 조회(한글룰 조회용)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="INO">여정번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="DTT">출발시간(HHMM)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="ART">도착시간(HHMM)</param>
		/// <param name="DLC">출발지</param>
		/// <param name="ALC">도착지</param>
		/// <param name="MCC">마케팅항공사</param>
		/// <param name="OCC">운항항공사</param>
		/// <param name="FLN">편명</param>
		/// <param name="RBD">좌석클래스</param>
		/// <param name="PFB">PFB적용여부</param>
		/// <param name="PFG">paxFareGroup XmlElement</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_InformativePricingWithoutPNRRuleRS")]
		public XmlElement InformativePricingWithoutPNRRuleRS(string SID, string SQN, string SCT, string GUID, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string PFB, XmlElement PFG)
		{
			XmlElement ReqXml = InformativePricingWithoutPNRRuleRQ(INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PFB, PFG);
            cm.XmlFileSave(ReqXml, ac.Name, "InformativePricingWithoutPNRRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("InformativePricingWithoutPNRRule", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "InformativePricingWithoutPNRRS", "Y", GUID);

			return ResXml;
		}

        [WebMethod(Description = "Fare_InformativePricingWithoutPNRRuleRS")]
        public XmlElement InformativePricingWithoutPNRRuleRS2(string SID, string SQN, string SCT, XmlElement ReqXml)
        {
            return ac.Execute("InformativePricingWithoutPNRRule", ReqXml, SID, SQN, SCT);
        }

		#endregion "InformativePricingWithoutPNR(Rule)"

		#region "PricePNRWithBookingClass"

		[WebMethod(Description = "Fare_PricePNRWithBookingClassRQ")]
		public XmlElement PricePNRWithBookingClassRQ()
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("PricePNRWithBookingClassRQ"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 예약의 운임조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_PricePNRWithBookingClassRS")]
		public XmlElement PricePNRWithBookingClassRS(string SID, string SQN, string SCT, string GUID)
		{
			XmlElement ReqXml = PricePNRWithBookingClassRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "PricePNRWithBookingClassRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("PricePNRWithBookingClass", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "PricePNRWithBookingClassRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_PricePNRWithBookingClassXml")]
		public XmlElement PricePNRWithBookingClassXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("PricePNRWithBookingClass", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "PricePNRWithBookingClass"

		#region "PricePNRWithBookingClass(Pricing)"

		[WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRQ")]
		public XmlElement PricePNRWithBookingClassPricingRQ(string PVC, string ECF, XmlNode PFG)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("PricePNRWithBookingClassPricingRQ"));

			XmlNode Root = XmlDoc.SelectSingleNode("Fare_PricePNRWithBookingClass");
            XmlNode PaxSegReference = Root.SelectSingleNode("paxSegReference");
			XmlNode OverrideInformation = Root.SelectSingleNode("overrideInformation");
			XmlNode DateOverride = Root.SelectSingleNode("dateOverride");
			XmlNode ValidatingCarrier = Root.SelectSingleNode("validatingCarrier");
            XmlNode DiscountInformation = Root.SelectSingleNode("discountInformation");

            if (PFG != null && PFG.SelectNodes("paxFare/segFareGroup").Count > 0)
			{
				int FareTypeRP = 0;
                int FareTypeRU = 0;
                int FareTypeRW = 0;

				//운임타입
				foreach (XmlNode FareType in PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/fare/fareType"))
				{
					if (FareType.InnerText.Equals("RP"))
						FareTypeRP++;
                    else if (FareType.InnerText.Equals("RB") || FareType.InnerText.Equals("RC") || FareType.InnerText.Equals("RX") || FareType.InnerText.Equals("RZ"))
                        FareTypeRW++;
                    else
						FareTypeRU++;
				}

                if (FareTypeRP.Equals(0) || ECF.IndexOf("RP") != -1)
                    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));

                if (FareTypeRU.Equals(0) || ECF.IndexOf("RU") != -1)
                    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));

                if (FareTypeRW.Equals(0) || ECF.IndexOf("RW") != -1)
                    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
                else
                    OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']/attributeDescription").InnerText = (PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/corporateId").Count > 0) ? PFG.SelectSingleNode("paxFare/segFareGroup/segFare/fare/corporateId").InnerText : "";
			}
            else
            {
                if (ECF.IndexOf("RP") != -1)
                    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));

                if (ECF.IndexOf("RU") != -1)
                    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));
                
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
            }

            //탑승객 지정 옵션(미사용)
            Root.RemoveChild(PaxSegReference);

            //탑승객별 지정 옵션(미사용)
            Root.RemoveChild(DiscountInformation);

			//우선 RLI옵션 삭제(추후 개발 적용)
			OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RLI']"));

			//MP 조회 시 지정한 Ticketing date에 따른 옵션 설정(미사용)
			Root.RemoveChild(DateOverride);

			if (PVC != null)
				ValidatingCarrier.SelectSingleNode("carrierInformation/carrierCode").InnerText = PVC;
			else
				Root.RemoveChild(ValidatingCarrier);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 예약의 운임조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="PVC">Validating Carrier</param>
        /// <param name="ECF">Pricing시에 제외시킬 FareType</param>
		/// <param name="PFG">paxFareGroup XmlNode</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRS")]
		public XmlElement PricePNRWithBookingClassPricingRS(string SID, string SQN, string SCT, string GUID, string PVC, string ECF, XmlNode PFG)
		{
			XmlElement ReqXml = PricePNRWithBookingClassPricingRQ(PVC, ECF, PFG);
            cm.XmlFileSave(ReqXml, ac.Name, "PricePNRWithBookingClassPricingRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("PricePNRWithBookingClassPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "PricePNRWithBookingClassPricingRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_PricePNRWithBookingClassPricingXml")]
		public XmlElement PricePNRWithBookingClassPricingXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("PricePNRWithBookingClassPricing", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "PricePNRWithBookingClass(Pricing)"

        #region "PricePNRWithBookingClass(Pricing)(강제 RU, RP 운임 조회용)"

        [WebMethod(Description = "Fare_PricePNRWithBookingClassPricing2RQ")]
        public XmlElement PricePNRWithBookingClassPricing2RQ(string PVC, string FareType, bool AllFareList)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("PricePNRWithBookingClassPricingRQ"));

            XmlNode Root = XmlDoc.SelectSingleNode("Fare_PricePNRWithBookingClass");
            XmlNode PaxSegReference = Root.SelectSingleNode("paxSegReference");
            XmlNode OverrideInformation = Root.SelectSingleNode("overrideInformation");
            XmlNode DateOverride = Root.SelectSingleNode("dateOverride");
            XmlNode ValidatingCarrier = Root.SelectSingleNode("validatingCarrier");
            XmlNode DiscountInformation = Root.SelectSingleNode("discountInformation");

            if (FareType.Equals("RP"))
            {
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
            }
            else if (FareType.Equals("RU"))
            {
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
            }
            else if (FareType.Equals("RW"))
            {
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));
            }

            //탑승객 지정 옵션(미사용)
            Root.RemoveChild(PaxSegReference);

            //탑승객별 지정 옵션(미사용)
            Root.RemoveChild(DiscountInformation);

            //RLI(전체 운임 리스트 출력)/RLO(최저가 운임 출력) 옵션
            if (AllFareList)
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RLO']"));
            else
                OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RLI']"));

            //MP 조회 시 지정한 Ticketing date에 따른 옵션 설정(미사용)
            Root.RemoveChild(DateOverride);

            if (PVC != null)
                ValidatingCarrier.SelectSingleNode("carrierInformation/carrierCode").InnerText = PVC;
            else
                Root.RemoveChild(ValidatingCarrier);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 발권용 운임조회(강제 RU, RP 운임 조회용)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PVC">Validating Carrier</param>
        /// <param name="FareType">RU/RP/RW 구분값</param>
        /// <param name="AllFareList">RLI 옵션 사용여부(적용 가능한 모든 운임이 조회 됨)</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRS(강제 RU, RP 운임 조회용)")]
        public XmlElement PricePNRWithBookingClassPricing2RS(string SID, string SQN, string SCT, string GUID, string PVC, string FareType, bool AllFareList)
        {
            XmlElement ReqXml = PricePNRWithBookingClassPricing2RQ(PVC, FareType, AllFareList);
            cm.XmlFileSave(ReqXml, ac.Name, "PricePNRWithBookingClassPricing2RQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("PricePNRWithBookingClassPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "PricePNRWithBookingClassPricing2RS", "Y", GUID);

            return ResXml;
        }

        #endregion "PricePNRWithBookingClass(Pricing)(강제 RU, RP 운임 조회용)"

        #region "PricePNRWithBookingClass(Pricing)(KE용)(강제 RU+RP+RLI 운임 조회용)"

        [WebMethod(Description = "Fare_PricePNRWithBookingClassPricing3RQ")]
        public XmlElement PricePNRWithBookingClassPricing3RQ(string PaxReferenceNumber, string PVC)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("PricePNRWithBookingClassPricingKERQ"));

            XmlNode Root = XmlDoc.SelectSingleNode("Fare_PricePNRWithBookingClass");
            XmlNode PaxSegTstReference = Root.SelectSingleNode("pricingOptionGroup[pricingOptionKey/pricingOptionKey='SEL']");

            if (String.IsNullOrWhiteSpace(PaxReferenceNumber))
                Root.RemoveChild(PaxSegTstReference);
            else
                PaxSegTstReference.SelectSingleNode("paxSegTstReference/referenceDetails[type='P']/value").InnerText = PaxReferenceNumber;

            if (!String.IsNullOrWhiteSpace(PVC))
                Root.SelectSingleNode("pricingOptionGroup[pricingOptionKey/pricingOptionKey='VC']/carrierInformation/companyIdentification/otherCompany").InnerText = PVC;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 발권용 운임조회(KE용)(강제 RU+RP+RLI 운임 조회용)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PaxReferenceNumber">탑승객 참조번호(공백일 경우 전체)</param>
        /// <param name="PVC">Validating Carrier</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRS(KE용)(강제 RU+RP+RLI 운임 조회용)")]
        public XmlElement PricePNRWithBookingClassPricing3RS(string SID, string SQN, string SCT, string GUID, string PaxReferenceNumber, string PVC)
        {
            XmlElement ReqXml = PricePNRWithBookingClassPricing3RQ(PaxReferenceNumber, PVC);
            cm.XmlFileSave(ReqXml, ac.Name, "PricePNRWithBookingClassPricing3RQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("PricePNRWithBookingClassKEPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "PricePNRWithBookingClassPricing3RS", "Y", GUID);

            return ResXml;
        }

        #endregion "PricePNRWithBookingClass(Pricing)(KE용)(강제 RU+RP+RLI 운임 조회용)"

        #region "PricePNRWithBookingClass(Pricing)(탑승객별 운임조회)(미구현)"

        //[WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRQ")]
        //public XmlElement PricePNRWithBookingClassPricing4RQ(string PVC, string ECF, XmlNode PFG, XmlNodeList TravellerInfo, XmlNamespaceManager xnMgr)
        //{
        //    XmlDocument XmlDoc = new XmlDocument();
        //    XmlDoc.Load(ac.XmlFullPath("PricePNRWithBookingClassPricingRQ"));

        //    XmlNode Root = XmlDoc.SelectSingleNode("Fare_PricePNRWithBookingClass");
        //    XmlNode PaxSegReference = Root.SelectSingleNode("paxSegReference");
        //    XmlNode OverrideInformation = Root.SelectSingleNode("overrideInformation");
        //    XmlNode DateOverride = Root.SelectSingleNode("dateOverride");
        //    XmlNode ValidatingCarrier = Root.SelectSingleNode("validatingCarrier");
        //    XmlNode DiscountInformation = Root.SelectSingleNode("discountInformation");

        //    bool NotADT = false;

        //    foreach (XmlNode TVR in TravellerInfo)
        //    {
        //        if (TVR.SelectNodes("m:passengerData[m:travellerInformation/m:passenger/m:type!='ADT']", xnMgr).Count > 0)
        //        {
        //            NotADT = true;
        //            break;
        //        }
        //    }

        //    if (NotADT)
        //    {
        //        XmlNode PenDisInformation;
        //        XmlNode PenDisData;
        //        XmlNode NewPenDisData;
        //        XmlNode NewDiscountInformation;
        //        string PassengerType = (TravellerInfo[0].SelectNodes("m:passengerData/m:travellerInformation/m:passenger/m:type", xnMgr).Count > 0) ? TravellerInfo[0].SelectSingleNode("m:passengerData/m:travellerInformation/m:passenger/m:type", xnMgr).InnerText : "ADT";

        //        foreach (XmlNode TVR in TravellerInfo)
        //        {
        //            foreach (XmlNode PassengerData in TVR.SelectNodes("m:passengerData", xnMgr))
        //            {
        //                string PTC = PassengerData.SelectSingleNode("m:travellerInformation/m:passenger/m:type", xnMgr).InnerText;
                        
        //                NewDiscountInformation = Root.AppendChild(DiscountInformation.CloneNode(true));
        //                PenDisInformation = NewDiscountInformation.SelectSingleNode("penDisInformation");
        //                PenDisData = PenDisInformation.SelectSingleNode("penDisData");

        //                PenDisInformation.SelectSingleNode("infoQualifier").InnerText = "701";
        //                PenDisData.SelectSingleNode("discountCode").InnerText = PassengerType;

        //                if (PTC.Equals("CHD"))
        //                {
        //                    NewPenDisData = PenDisInformation.AppendChild(PenDisData.CloneNode(true));
        //                    NewPenDisData.SelectSingleNode("discountCode").InnerText = "CNN";
        //                }
        //                else if (PTC.Equals("INF"))
        //                {
        //                    NewPenDisData = PenDisInformation.AppendChild(PenDisData.CloneNode(true));
        //                    NewPenDisData.SelectSingleNode("discountCode").InnerText = "INF";
        //                }

        //                NewDiscountInformation.SelectSingleNode("referenceQualifier/refDetails/refQualifier").InnerText = PTC.Equals("INF") ? "PI" : "PA";
        //                NewDiscountInformation.SelectSingleNode("referenceQualifier/refDetails/refNumber").InnerText = TVR.SelectSingleNode("m:elementManagementPassenger/m:reference[m:qualifier='PT']/m:number", xnMgr).InnerText;
        //            }
        //        }

        //        Root.RemoveChild(DiscountInformation);
        //    }
        //    //else
        //    //{
                
        //    //}

        //    if (PFG != null)
        //    {
        //        int FareTypeRP = 0;
        //        int FareTypeRU = 0;
        //        int FareTypeRW = 0;

        //        //운임타입
        //        foreach (XmlNode FareType in PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/fare/fareType"))
        //        {
        //            if (FareType.InnerText.Equals("RP"))
        //                FareTypeRP++;
        //            else if (FareType.InnerText.Equals("RB") || FareType.InnerText.Equals("RC") || FareType.InnerText.Equals("RX") || FareType.InnerText.Equals("RZ"))
        //                FareTypeRW++;
        //            else
        //                FareTypeRU++;
        //        }

        //        if (FareTypeRP.Equals(0) || ECF.IndexOf("RP") != -1)
        //            OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));

        //        if (FareTypeRU.Equals(0) || ECF.IndexOf("RU") != -1)
        //            OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));

        //        if (FareTypeRW.Equals(0) || ECF.IndexOf("RW") != -1)
        //            OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
        //        else
        //            OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']/attributeDescription").InnerText = (PFG.SelectNodes("paxFare/segFareGroup/segFare/fare/corporateId").Count > 0) ? PFG.SelectSingleNode("paxFare/segFareGroup/segFare/fare/corporateId").InnerText : "";
        //    }
        //    else
        //    {
        //        if (ECF.IndexOf("RP") != -1)
        //            OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RP']"));

        //        if (ECF.IndexOf("RU") != -1)
        //            OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RU']"));

        //        OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RW']"));
        //    }

        //    //탑승객 지정 옵션(미사용)
        //    Root.RemoveChild(PaxSegReference);

        //    //우선 RLI옵션 삭제(추후 개발 적용)
        //    OverrideInformation.RemoveChild(OverrideInformation.SelectSingleNode("attributeDetails[attributeType='RLI']"));

        //    //MP 조회 시 지정한 Ticketing date에 따른 옵션 설정(미사용)
        //    Root.RemoveChild(DateOverride);

        //    if (PVC != null)
        //        ValidatingCarrier.SelectSingleNode("carrierInformation/carrierCode").InnerText = PVC;
        //    else
        //        Root.RemoveChild(ValidatingCarrier);

        //    return XmlDoc.DocumentElement;
        //}

        ///// <summary>
        ///// 발권용 운임조회(탑승객별 운임조회)
        ///// </summary>
        ///// <param name="SID">SessionId</param>
        ///// <param name="SQN">SequenceNumber</param>
        ///// <param name="SCT">SecurityToken</param>
        ///// <param name="GUID">고유번호</param>
        ///// <param name="PVC">Validating Carrier</param>
        ///// <param name="ECF">Pricing시에 제외시킬 FareType</param>
        ///// <param name="PFG">paxFareGroup XmlNode</param>
        ///// <param name="TravellerInfo">탑승객 정보</param>
        ///// <returns></returns>
        //[WebMethod(Description = "Fare_PricePNRWithBookingClassPricingRS(탑승객별 운임조회)")]
        //public XmlElement PricePNRWithBookingClassPricing4RS(string SID, string SQN, string SCT, string GUID, string PVC, string ECF, XmlNode PFG, XmlNodeList TravellerInfo, XmlNamespaceManager xnMgr)
        //{
        //    XmlElement ReqXml = PricePNRWithBookingClassPricing4RQ(PVC, ECF, PFG, TravellerInfo, xnMgr);
        //    cm.XmlFileSave(ReqXml, ac.Name, "PricePNRWithBookingClassPricing4RQ", "Y", GUID);

        //    XmlElement ResXml = ac.Execute("PricePNRWithBookingClassPricing", ReqXml, SID, SQN, SCT);
        //    cm.XmlFileSave(ResXml, ac.Name, "PricePNRWithBookingClassPricing4RS", "Y", GUID);

        //    return ResXml;
        //}

        #endregion "PricePNRWithBookingClass(Pricing)(탑승객별 운임조회)"

        #region "SellByFareSearch"

        [WebMethod(Description = "Fare_SellByFareSearchRQ")]
		public XmlElement SellByFareSearchRQ(string DTD, string ARD, string FAB, string MCC, string DLC, string ALC, string ROT, int? STN, int ADC, int CHC, int IFC, int NRR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("SellByFareSearchRQ"));

			XmlNode SBFC = XmlDoc.SelectSingleNode("Fare_SellByFareSearch");
			XmlNode NumberOfUnit = SBFC.SelectSingleNode("numberOfUnit");
			XmlNode PaxReference = SBFC.SelectSingleNode("paxReference");
			XmlNode PaxTypeCode = PaxReference.SelectSingleNode("ptc");
			XmlNode Traveller = PaxReference.SelectSingleNode("traveller");
			XmlNode FareFamilies = SBFC.SelectSingleNode("fareFamilies");
			XmlNode TravelFlightInfo = SBFC.SelectSingleNode("travelFlightInfo");
			XmlNode FlightDetail = TravelFlightInfo.SelectSingleNode("flightDetail");
			XmlNode Itinerary = SBFC.SelectSingleNode("itinerary");
			XmlNode NewPaxReference = null;
			XmlNode NewTraveller;

			//응답 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='RC']/numberOfUnits").InnerText = NRR.ToString();

			//탑승객 정의
			int PaxCount = 1;
			int TmpAdultCount = 1;

			if (ADC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "ADT";

				for (int i = 0; i < ADC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
					NewTraveller.RemoveChild(NewTraveller.SelectSingleNode("infantIndicator"));
				}
			}

			if (CHC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "CH";

				for (int i = 0; i < CHC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
					NewTraveller.RemoveChild(NewTraveller.SelectSingleNode("infantIndicator"));
				}
			}

			if (IFC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "INF";

				for (int i = 0; i < IFC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(i + 1);
					NewTraveller.SelectSingleNode("infantIndicator").InnerText = Convert.ToString((TmpAdultCount / 2) + (TmpAdultCount % 2));
					TmpAdultCount++;
				}
			}

			SBFC.RemoveChild(PaxReference);

			//탑승객 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='PX']/numberOfUnits").InnerText = (ADC + CHC).ToString();

			FareFamilies.SelectSingleNode("familyInformation/fareFamilyname").InnerText = String.Concat(DLC, ALC);
			FareFamilies.SelectSingleNode("familyCriteria/carrierId").InnerText = MCC;
			FareFamilies.SelectSingleNode("familyCriteria/fareProductDetail/fareBasis").InnerText = FAB;

            TravelFlightInfo.SelectSingleNode("companyIdentity/carrierId").InnerText = MCC;

            if (STN.HasValue)
            {
                if (STN > 0)
                {
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='D']"));
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='N']"));
                }
                else
                {
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='C']"));
                }
            }

			Itinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "1";
			Itinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLC;
			Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALC;
			Itinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(DTD);

			if (ROT.Equals("RT"))
			{
				XmlNode NewItinerary = SBFC.AppendChild(Itinerary.CloneNode(true));

				NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "2";
				NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = ALC;
				NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = DLC;
				NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(ARD);
			}

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// SellByFareSearch 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="FAB">Fare Basis</param>
		/// <param name="MCC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복)</param>
		/// <param name="STN">경유 수</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="NRR">응답 결과 수</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_SellByFareSearchRS")]
		public XmlElement SellByFareSearchRS(string SID, string SQN, string SCT, string GUID, string DTD, string ARD, string FAB, string MCC, string DLC, string ALC, string ROT, int? STN, int ADC, int CHC, int IFC, int NRR)
		{
			XmlElement ReqXml = SellByFareSearchRQ(DTD, ARD, FAB, MCC, DLC, ALC, ROT, STN, ADC, CHC, IFC, NRR);
            cm.XmlFileSave(ReqXml, ac.Name, "SellByFareSearchRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("SellByFareSearch", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "SellByFareSearchRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_SellByFareSearchXml")]
		public XmlElement SellByFareSearchXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("SellByFareSearch", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "SellByFareSearch"

		#region "SellByFareCalendar"

		[WebMethod(Description = "Fare_SellByFareCalendarRQ")]
		public XmlElement SellByFareCalendarRQ(string DTD, string ARD, string FAB, string MCC, string DLC, string ALC, string ROT, int? STN, int ADC, int CHC, int IFC, int NRR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("SellByFareCalendarRQ"));

			XmlNode SBFC = XmlDoc.SelectSingleNode("Fare_SellByFareCalendar");
			XmlNode NumberOfUnit = SBFC.SelectSingleNode("numberOfUnit");
			XmlNode PaxReference = SBFC.SelectSingleNode("paxReference");
			XmlNode PaxTypeCode = PaxReference.SelectSingleNode("ptc");
			XmlNode Traveller = PaxReference.SelectSingleNode("traveller");
			XmlNode FareFamilies = SBFC.SelectSingleNode("fareFamilies");
			XmlNode TravelFlightInfo = SBFC.SelectSingleNode("travelFlightInfo");
			XmlNode FlightDetail = TravelFlightInfo.SelectSingleNode("flightDetail");
			XmlNode Itinerary = SBFC.SelectSingleNode("itinerary");
			XmlNode NewPaxReference = null;
			XmlNode NewTraveller;

			//응답 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='RC']/numberOfUnits").InnerText = NRR.ToString();

			//탑승객 정의
			int PaxCount = 1;
			int TmpAdultCount = 1;

			if (ADC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "ADT";

				for (int i = 0; i < ADC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
					NewTraveller.RemoveChild(NewTraveller.SelectSingleNode("infantIndicator"));
				}
			}

			if (CHC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "CH";

				for (int i = 0; i < CHC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(PaxCount++);
					NewTraveller.RemoveChild(NewTraveller.SelectSingleNode("infantIndicator"));
				}
			}

			if (IFC > 0)
			{
				NewPaxReference = SBFC.InsertBefore(PaxReference.CloneNode(false), PaxReference);
				NewPaxReference.AppendChild(PaxTypeCode.Clone());
				NewPaxReference.SelectSingleNode("ptc").InnerText = "INF";

				for (int i = 0; i < IFC; i++)
				{
					NewTraveller = NewPaxReference.AppendChild(Traveller.CloneNode(true));
					NewTraveller.SelectSingleNode("ref").InnerText = Convert.ToString(i + 1);
					NewTraveller.SelectSingleNode("infantIndicator").InnerText = Convert.ToString((TmpAdultCount / 2) + (TmpAdultCount % 2));
					TmpAdultCount++;
				}
			}

			SBFC.RemoveChild(PaxReference);

			//탑승객 수
			NumberOfUnit.SelectSingleNode("unitNumberDetail[typeOfUnit='PX']/numberOfUnits").InnerText = (ADC + CHC).ToString();

			FareFamilies.SelectSingleNode("familyInformation/fareFamilyname").InnerText = String.Concat(DLC, ALC);
			FareFamilies.SelectSingleNode("familyCriteria/carrierId").InnerText = MCC;
			FareFamilies.SelectSingleNode("familyCriteria/fareProductDetail/fareBasis").InnerText = FAB;

			TravelFlightInfo.SelectSingleNode("companyIdentity/carrierId").InnerText = MCC;

            if (STN.HasValue)
            {
                if (STN > 0)
                {
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='D']"));
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='N']"));
                }
                else
                {
                    FlightDetail.RemoveChild(FlightDetail.SelectSingleNode("flightType[.='C']"));
                }
            }

			Itinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "1";
			Itinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = DLC;
			Itinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = ALC;
			Itinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(DTD);
			Itinerary.SelectSingleNode("timeDetails/rangeOfDate/rangeQualifier").InnerText = "P";
			Itinerary.SelectSingleNode("timeDetails/rangeOfDate/dayInterval").InnerText = "30";

			//경유지정보
			Itinerary.RemoveChild(Itinerary.SelectSingleNode("flightInfo"));
			//XmlNode FlightInfo = Itinerary.SelectSingleNode("flightInfo");
			//XmlNode InclusionDetail = FlightInfo.SelectSingleNode("inclusionDetail");
			//XmlNode NewInclusionDetail;

			//NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
			//NewInclusionDetail.SelectSingleNode("locationId").InnerText = "FUK";

			//NewInclusionDetail = FlightInfo.AppendChild(InclusionDetail.CloneNode(true));
			//NewInclusionDetail.SelectSingleNode("locationId").InnerText = "PAR";

			if (ROT.Equals("RT"))
			{
				XmlNode NewItinerary = SBFC.AppendChild(Itinerary.CloneNode(true));

				NewItinerary.SelectSingleNode("requestedSegmentRef/segRef").InnerText = "2";
				NewItinerary.SelectSingleNode("departureLocalization/departurePoint/locationId").InnerText = ALC;
				NewItinerary.SelectSingleNode("arrivalLocalization/arrivalPointDetails/locationId").InnerText = DLC;
				NewItinerary.SelectSingleNode("timeDetails/firstDateTimeDetail/date").InnerText = cm.ConvertToAmadeusDate(ARD);
				NewItinerary.SelectSingleNode("timeDetails/rangeOfDate/rangeQualifier").InnerText = "P";
				NewItinerary.SelectSingleNode("timeDetails/rangeOfDate/dayInterval").InnerText = "30";

				//XmlNode NewFlightInfo = NewItinerary.SelectSingleNode("flightInfo");
				//NewFlightInfo.RemoveAll();

				//NewInclusionDetail = NewFlightInfo.AppendChild(InclusionDetail.CloneNode(true));
				//NewInclusionDetail.SelectSingleNode("locationId").InnerText = "PAR";

				//NewInclusionDetail = NewFlightInfo.AppendChild(InclusionDetail.CloneNode(true));
				//NewInclusionDetail.SelectSingleNode("locationId").InnerText = "FUK";
			}

			//FlightInfo.RemoveChild(InclusionDetail);
			
			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// SellByFareCalendar 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="FAB">Fare Basis</param>
		/// <param name="MCC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복)</param>
		/// <param name="STN">경유 수</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="NRR">응답 결과 수</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_SellByFareCalendarRS")]
		public XmlElement SellByFareCalendarRS(string SID, string SQN, string SCT, string GUID, string DTD, string ARD, string FAB, string MCC, string DLC, string ALC, string ROT,	int? STN, int ADC, int CHC, int IFC, int NRR)
		{
			XmlElement ReqXml = SellByFareCalendarRQ(DTD, ARD, FAB, MCC, DLC, ALC, ROT, STN, ADC, CHC, IFC, NRR);
            cm.XmlFileSave(ReqXml, ac.Name, "SellByFareCalendarRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("SellByFareCalendar", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "SellByFareCalendarRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Fare_SellByFareCalendarXml")]
		public XmlElement SellByFareCalendarXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("SellByFareCalendar", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "SellByFareCalendar"

		#region "CheckRules"

		[WebMethod(Description = "Fare_CheckRulesRQ")]
		public XmlElement CheckRulesRQ(string fcNumber, int Seq)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CheckRulesRQ"));

			XmlDoc.SelectSingleNode("Fare_CheckRules/itemNumber/itemNumberDetails[type='FC']/number").InnerText = fcNumber;

			XmlNode TarifFareRule = XmlDoc.SelectSingleNode("Fare_CheckRules/fareRule/tarifFareRule");

			//Rule ID와 Tariff ID 조회용
            if (Seq.Equals(1))
			{
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='PE']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SU']"));
			}
            //일반 영문규정 호출
			else if (Seq.Equals(2))
			{
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='AP']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='TF']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='DA']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='FL']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='MN']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='MX']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SO']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='BO']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='TR']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SR']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='CD']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SE']"));
				TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='EL']"));
			}
            //미니룰 사용에 따른 증빙서류 조회용
            else if (Seq.Equals(3))
            {
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='AP']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='TF']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='DA']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='FL']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='MN']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='MX']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SO']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='BO']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SU']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='TR']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SR']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='PE']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='CD']"));
                TarifFareRule.RemoveChild(TarifFareRule.SelectSingleNode("ruleSectionId[.='SE']"));
            }

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 룰 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="fcNumber">Fare Component Number</param>
		/// <param name="Seq">규정조회 구분값</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_CheckRulesRS")]
		public XmlElement CheckRulesRS(string SID, string SQN, string SCT, string GUID, string fcNumber, int Seq)
		{
			XmlElement ReqXml = CheckRulesRQ(fcNumber, Seq);
            cm.XmlFileSave(ReqXml, ac.Name, "CheckRulesRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("CheckRules", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CheckRulesRS", "Y", GUID);

			return ResXml;
		}

		#endregion "CheckRules"

        #region "CheckRules(Advance Purchase)"

        [WebMethod(Description = "Fare_CheckRulesAPRQ")]
        public XmlElement CheckRulesAPRQ(string fcNumber)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("CheckRulesAPRQ"));

            XmlDoc.SelectSingleNode("Fare_CheckRules/itemNumber/itemNumberDetails[type='FC']/number").InnerText = fcNumber;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// Advance Purchase(사전발권) 조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="fcNumber">Fare Component Number</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare_CheckRulesAPRS")]
        public XmlElement CheckRulesAPRS(string SID, string SQN, string SCT, string GUID, string fcNumber)
        {
            XmlElement ReqXml = CheckRulesAPRQ(fcNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "CheckRulesAPRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("CheckRules", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CheckRulesAPRS", "Y", GUID);

            return ResXml;
        }

        #endregion "CheckRules(Advance Purchase)"

        #region "CheckRules(RuleID/TariffID)"

        [WebMethod(Description = "Fare_CheckRulesTariffRQ")]
		public XmlElement CheckRulesTariffRQ(string fcNumber)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CheckRulesTariffRQ"));

			XmlDoc.SelectSingleNode("Fare_CheckRules/itemNumber/itemNumberDetails[type='FC']/number").InnerText = fcNumber;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// Rule ID 및 Tariff ID 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="fcNumber">Fare Component Number</param>
		/// <returns></returns>
		[WebMethod(Description = "Fare_CheckRulesTariffRS")]
		public XmlElement CheckRulesTariffRS(string SID, string SQN, string SCT, string GUID, string fcNumber)
		{
			XmlElement ReqXml = CheckRulesTariffRQ(fcNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "CheckRulesTariffRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("CheckRules", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CheckRulesTariffRS", "Y", GUID);

			return ResXml;
		}

		#endregion "CheckRules(RuleID/TariffID)"

		#region "SearchRules"

		/// <summary>
		/// 운임규정조회(할인항공, 오픈마켓용)
		/// </summary>
		/// <param name="RsvAgtCd">오피스아이디</param>
		/// <param name="FareNo">FARE_NO</param>
		/// <param name="FareRuleFlag">구분값(할인항공:H, 이벤트:E)</param>
		/// <returns></returns>
		[WebMethod(Description = "할인항공, 오픈마켓용 운임규정 조회")]
		public XmlElement SearchRulesRS(string RsvAgtCd, string FareNo, string FareRuleFlag)
		{
			return XmlRequest.GetSend(String.Format("http://seatmap.cyberbooking.co.kr/tsm/TAI/TAIFARRUL1110110330.k1xml?Rsvagtcd={0}&Fareno={1}&Fareruleflag={2}", (String.IsNullOrWhiteSpace(RsvAgtCd) ? "SELK138AB" : RsvAgtCd), FareNo, (String.IsNullOrWhiteSpace(FareRuleFlag) ? "H" : FareRuleFlag)));
		}

		#endregion "SearchRules"

        #region "미니룰"

        [WebMethod(Description = "MiniRule_GetFromPricingRQ")]
        public XmlElement MiniRuleGetFromPricingRQ()
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("MiniRuleGetFromPricingRQ"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 예약의 운임조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "MiniRule_GetFromPricingRS")]
        public XmlElement MiniRuleGetFromPricingRS(string SID, string SQN, string SCT, string GUID)
        {
            XmlElement ReqXml = MiniRuleGetFromPricingRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "MiniRuleGetFromPricingRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("MiniRuleGetFromPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "MiniRuleGetFromPricingRS", "Y", GUID);

            return ResXml;
        }

        [WebMethod(Description = "MiniRule_GetFromPricingXml")]
        public XmlElement MiniRuleGetFromPricingXml(string ReqXml, string SID, string SQN, string SCT)
        {
            try
            {
                XmlDocument XmlReq = new XmlDocument();
                XmlReq.LoadXml(ReqXml);

                return ac.Execute("MiniRuleGetFromPricing", XmlReq.DocumentElement, SID, SQN, SCT);
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "미니룰"

		#endregion "Fare"

		#region "PNR"

		#region "AddMultiElements"

		[WebMethod(Description = "PNR_AddMultiElementsRQ")]
        public XmlElement AddMultiElementsRQ(string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string RTN, string RMN, string REA, string RMK, string RQT, int SNM, int ANM, string OPN, string MCC, XmlElement SXL)
		{
			//### 01.탑승객 정보 #####
			string TmpADT = string.Empty;
			string TmpCHD = string.Empty;
			string TmpINF = string.Empty;

			for (int i = 0; i < PTC.Length; i++)
			{
				if (PTC[i].Equals("CHD"))
					TmpCHD += String.Concat(i.ToString(), ",");
				else if (PTC[i].Equals("INF"))
					TmpINF += String.Concat(i.ToString(), ",");
				else
					TmpADT += String.Concat(i.ToString(), ",");
				
			}

			string[] PaxADT = TmpADT.Split(',');
			string[] PaxCHD = TmpCHD.Split(',');
			string[] PaxINF = TmpINF.Split(',');
			int n = 1;
			int idx1 = 0;
			int idx2 = 0;
				
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsRQ"));

			XmlNode PAME = XmlDoc.SelectSingleNode("PNR_AddMultiElements");
			XmlNode TravellerInfo = PAME.SelectSingleNode("travellerInfo");
			XmlNode OriginDestinationDetailsArnk = PAME.SelectSingleNode("originDestinationDetails[1]");
			XmlNode OriginDestinationDetailsOpen = PAME.SelectSingleNode("originDestinationDetails[2]");
			XmlNode PassengerData;
			XmlNode Passenger;
			XmlNode NewPassengerData;
			XmlNode NewPassenger;
			XmlNode NewTravellerInfo;
            string ACBirth = string.Empty;

			for (int i = 0; i < PaxADT.Length - 1; i++)
			{
				idx1 = Convert.ToInt32(PaxADT[i].Trim());
				
				NewTravellerInfo = PAME.InsertBefore(TravellerInfo.CloneNode(true), TravellerInfo);
				PassengerData = NewTravellerInfo.SelectSingleNode("passengerData");
				Passenger = PassengerData.SelectSingleNode("travellerInformation/passenger");

				NewTravellerInfo.SelectSingleNode("elementManagementPassenger/reference/number").InnerText = n.ToString();
				PassengerData.SelectSingleNode("travellerInformation/traveller/surname").InnerText = Common.ChangeSurname(PSN[idx1].Replace(" ", ""));
				Passenger.SelectSingleNode("firstName").InnerText = String.Concat(PFN[idx1].Replace(" ", ""), PTL[idx1]);
				Passenger.SelectSingleNode("type").InnerText = PTC[idx1];

				if (!String.IsNullOrWhiteSpace(TmpINF) && PaxINF.Length > i && !String.IsNullOrWhiteSpace(PaxINF[i]))
				{
					idx2 = Convert.ToInt32(PaxINF[i].Trim());

					if (String.Compare(PSN[idx1], PSN[idx2], true).Equals(0))
					{
						NewPassenger = PassengerData.SelectSingleNode("travellerInformation").AppendChild(Passenger.CloneNode(true));
						NewPassenger.SelectSingleNode("firstName").InnerText = String.Concat(PFN[idx2].Replace(" ", ""), PTL[idx2]);
						NewPassenger.SelectSingleNode("type").InnerText = "INF";
						NewPassenger.RemoveChild(NewPassenger.SelectSingleNode("infantIndicator"));

						Passenger.SelectSingleNode("infantIndicator").InnerText = "2";
						PassengerData.SelectSingleNode("travellerInformation/traveller/quantity").InnerText = "2";
						PassengerData.SelectSingleNode("dateOfBirth/dateAndTimeDetails/date").InnerText = cm.RequestDateTime(PBD[idx2], "ddMMyyyy");
					}
					else //보호자와 유아의 성이 다를 경우
					{
						//유아
						NewPassengerData = NewTravellerInfo.AppendChild(PassengerData.CloneNode(true));
						NewPassengerData.SelectSingleNode("travellerInformation/traveller/surname").InnerText = Common.ChangeSurname(PSN[idx2].Replace(" ", ""));
						NewPassengerData.SelectSingleNode("travellerInformation/traveller/quantity").InnerText = "2";
						NewPassengerData.SelectSingleNode("travellerInformation/passenger/firstName").InnerText = String.Concat(PFN[idx2].Replace(" ", ""), PTL[idx2]);
						NewPassengerData.SelectSingleNode("travellerInformation/passenger/type").InnerText = "INF";
						NewPassengerData.SelectSingleNode("dateOfBirth/dateAndTimeDetails/date").InnerText = cm.RequestDateTime(PBD[idx2], "ddMMyyyy");

						//보호자
						Passenger.SelectSingleNode("infantIndicator").InnerText = "3";
						PassengerData.SelectSingleNode("travellerInformation/traveller/quantity").InnerText = "2";
						PassengerData.RemoveChild(PassengerData.SelectSingleNode("dateOfBirth"));
					}
				}
				else
				{
					Passenger.RemoveChild(Passenger.SelectSingleNode("infantIndicator"));
					
                    //성인은 생년월일을 무조건 입력하지 않는다(2016-08-24,김지영과장)
                    //if (String.IsNullOrWhiteSpace(PBD[idx1]))
                    //    PassengerData.RemoveChild(PassengerData.SelectSingleNode("dateOfBirth"));
                    //else
                    //    PassengerData.SelectSingleNode("dateOfBirth/dateAndTimeDetails/date").InnerText = cm.RequestDateTime(PBD[idx1], "ddMMyyyy");
                    PassengerData.RemoveChild(PassengerData.SelectSingleNode("dateOfBirth"));

                    //AC항공의 14살 미만 승객에 대한 나이 OS 입력을 위함
                    ACBirth += String.Format("{0}/{1}^", n, PBD[idx1]);
				}

                n++;
			}

			for (int i = 0; i < PaxCHD.Length - 1; i++)
			{
				idx1 = Convert.ToInt32(PaxCHD[i].Trim());

				NewTravellerInfo = PAME.InsertBefore(TravellerInfo.CloneNode(true), TravellerInfo);
				PassengerData = NewTravellerInfo.SelectSingleNode("passengerData");

				NewTravellerInfo.SelectSingleNode("elementManagementPassenger/reference/number").InnerText = n.ToString();
				PassengerData.SelectSingleNode("travellerInformation/traveller/surname").InnerText = Common.ChangeSurname(PSN[idx1].Replace(" ", ""));
				PassengerData.SelectSingleNode("travellerInformation/passenger/firstName").InnerText = String.Concat(PFN[idx1].Replace(" ", ""), PTL[idx1]);
				PassengerData.SelectSingleNode("travellerInformation/passenger/type").InnerText = PTC[idx1];
				PassengerData.SelectSingleNode("dateOfBirth/dateAndTimeDetails/date").InnerText = cm.RequestDateTime(PBD[idx1], "ddMMyyyy");

				PassengerData.SelectSingleNode("travellerInformation/passenger").RemoveChild(PassengerData.SelectSingleNode("travellerInformation/passenger/infantIndicator"));

                //AC항공의 14살 미만 승객에 대한 나이 OS 입력을 위함
                ACBirth += String.Format("{0}/{1}^", n, PBD[idx1]);
                n++;
			}

			PAME.RemoveChild(TravellerInfo);

			//### 02.ARNK(비항공구간) #####
			bool Arnk = false;
			XmlNodeList Segs = SXL.SelectNodes("segGroup/seg");

			for (int i = 1; i < Segs.Count; i++)
			{
				if (Segs[i].Attributes.GetNamedItem("dlc").InnerText != Segs[(i - 1)].Attributes.GetNamedItem("alc").InnerText)
				{
					Arnk = true;
					break;
				}
			}

			if (!Arnk)
				PAME.RemoveChild(OriginDestinationDetailsArnk);

			//### 03.여정정보(오픈일 경우) #####
			if (OPN.Equals("Y"))
			{
				XmlNode SegGroup = SXL.SelectSingleNode("segGroup[2]");
				XmlNode NewOriginDestinationDetails;
				int sr = 1;

				foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
				{
					NewOriginDestinationDetails = PAME.InsertBefore(OriginDestinationDetailsOpen.CloneNode(true), OriginDestinationDetailsOpen);

					NewOriginDestinationDetails.SelectSingleNode("originDestination/origin").InnerText = Seg.Attributes.GetNamedItem("dlc").InnerText;
					NewOriginDestinationDetails.SelectSingleNode("originDestination/destination").InnerText = Seg.Attributes.GetNamedItem("alc").InnerText;
					NewOriginDestinationDetails.SelectSingleNode("itineraryInfo/elementManagementItinerary/reference/number").InnerText = (sr++).ToString();
					NewOriginDestinationDetails.SelectSingleNode("itineraryInfo/airAuxItinerary/travelProduct/boardpointDetail/cityCode").InnerText = Seg.Attributes.GetNamedItem("dlc").InnerText;
					NewOriginDestinationDetails.SelectSingleNode("itineraryInfo/airAuxItinerary/travelProduct/offpointDetail/cityCode").InnerText = Seg.Attributes.GetNamedItem("alc").InnerText;
					NewOriginDestinationDetails.SelectSingleNode("itineraryInfo/airAuxItinerary/travelProduct/company/identification").InnerText = Seg.Attributes.GetNamedItem("mcc").InnerText;
					NewOriginDestinationDetails.SelectSingleNode("itineraryInfo/airAuxItinerary/travelProduct/productDetails/classOfService").InnerText = Seg.Attributes.GetNamedItem("rbd").InnerText;
				}
			}

			PAME.RemoveChild(OriginDestinationDetailsOpen);

			//### 04.예약자정보 #####
			XmlNode DataElementsMaster = PAME.SelectSingleNode("dataElementsMaster");
			XmlNode DataElementsIndiv = DataElementsMaster.SelectSingleNode("dataElementsIndiv");
			XmlNode ElementManagementData;
			XmlNode TicketElement;
			XmlNode MiscellaneousRemark;
            XmlNode ServiceRequest;
            XmlNode ReferenceForDataElement;
            XmlNode FreetextData;
			XmlNode NewDataElementsIndiv;
			int OTNum = 1;

			//모두투어 전화번호
            if (SNM != 68)
            {
                NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
                ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
                ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
                FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "5";
                FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                FreetextData.SelectSingleNode("longFreetext").InnerText = Common.TelInfo(SNM, ANM);
                NewDataElementsIndiv.RemoveChild(TicketElement);
                NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                NewDataElementsIndiv.RemoveChild(ServiceRequest);
                NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
            }

			//전화번호(예약자)
			if (!String.IsNullOrWhiteSpace(cm.RequestString(RTN)))
			{
				NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
				ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
				TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
				MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
				FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

				ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
				ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
				ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
				FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
				FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "5";
                FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
				FreetextData.SelectSingleNode("longFreetext").InnerText = RTN;
				NewDataElementsIndiv.RemoveChild(TicketElement);
				NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                NewDataElementsIndiv.RemoveChild(ServiceRequest);
                NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
			}

			//전화번호(탑승객)
			foreach (string frText in PTN)
			{
				if (!String.IsNullOrWhiteSpace(frText))
				{
					NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
					ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
					TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
					MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
					FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

					ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
					ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
					ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
					FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
					FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "5";
                    FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                    FreetextData.SelectSingleNode("longFreetext").InnerText = frText;
					NewDataElementsIndiv.RemoveChild(TicketElement);
					NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
				}
			}

			//휴대폰(예약자)
			if (!String.IsNullOrWhiteSpace(cm.RequestString(RMN)))
			{
                NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
				ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
				TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
				MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
				FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

				ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
				ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
				ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
				FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                FreetextData.SelectSingleNode("freetextDetail/type").InnerText = MCC.Equals("VN") ? "5" : "7"; //VN항공사는 핸드폰 번호를 AP로 입력(2016-08-16,김지영과장요청)
                FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
				FreetextData.SelectSingleNode("longFreetext").InnerText = RMN;
				NewDataElementsIndiv.RemoveChild(TicketElement);
				NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                NewDataElementsIndiv.RemoveChild(ServiceRequest);
                NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);

                //7C,LH,MU,CA,CZ항공사는 항공사에서 원하는 양식이 따로 있어 추가 등록해 준다.(2015-08-06,김승미과장요청)
                //CI항공 추가(2015-10-06,김승미과장요청)
                //JL항공 추가(2016-05-19,김승미과장요청)
                //DL/SQ/AY항공 추가(2016-06-21,김승미과장요청)
                //ZE/AA/AC/BA/ET/EY/NH/NX/QR/SU/TK항공 추가(2016-08-11,김지영과장요청)
                //PR항공 추가(2016-11-18,김승미과장요청)
                //AI/AM/CX/EK/HA/HX/HY/PS/PX/VJ항공 추가(2016-11-21,김지영과장요청)
                //MF항공 추가(2017-04-20,김경미차장요청)
                if ("/7C/LH/QR/".IndexOf(MCC) != -1)
                {
                    NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
				    ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
				    TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
				    MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
				    FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

				    ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
				    ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
				    ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
				    FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
				    FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "7";
                    FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                    FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingTelFormat(MCC, RMN, false);
				    NewDataElementsIndiv.RemoveChild(TicketElement);
				    NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                }
                else if ("/MU/CA/CZ/CI/JL/DL/SQ/AY/ZE/AA/AC/BA/ET/EY/NH/NX/SU/TK/PR/AI/AM/CX/EK/HA/HX/HY/PS/PX/VJ/MF/".IndexOf(MCC) != -1)
                {
                    NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                    ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                    TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                    MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                    FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                    ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                    ElementManagementData.SelectSingleNode("segmentName").InnerText = "OS";
                    FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                    FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P27";
                    FreetextData.SelectSingleNode("freetextDetail/companyId").InnerText = MCC;
                    //FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                    FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingTelFormat(MCC, RMN, true);
                    NewDataElementsIndiv.RemoveChild(TicketElement);
                    NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                }
			}

			//휴대폰(탑승객)
            int PMNRef = 2;
			foreach (string frText in PMN)
			{
				if (!String.IsNullOrWhiteSpace(frText))
				{
					NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
					ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
					TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
					MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
					FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

					ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
					ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
					ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
					FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                    FreetextData.SelectSingleNode("freetextDetail/type").InnerText = MCC.Equals("VN") ? "5" : "7"; //VN항공사는 핸드폰 번호를 AP로 입력(2016-08-16,김지영과장요청)
                    FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                    FreetextData.SelectSingleNode("longFreetext").InnerText = MCC.Equals("KE") ? String.Format("SEL {0} (Y)", frText) : (MCC.Equals("7C") ? String.Concat("SEL ", frText) : frText);//7C항공사는 SEL을 앞에 추가(2016-08-23,김승미차장요청), KE추가(2017-08-01,김지영차장)
					NewDataElementsIndiv.RemoveChild(TicketElement);
					NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);

                    //7C,LH,MU,CA,CZ항공사는 항공사에서 원하는 양식이 따로 있어 추가 등록해 준다.(2015-08-06,김승미과장요청)
                    //CI항공 추가(2015-10-06,김승미과장요청)
                    //JL항공 추가(2016-05-19,김승미과장요청)
                    //DL/SQ/AY항공 추가(2016-06-21,김승미과장요청)
                    //ZE/AA/AC/BA/ET/EY/NH/NX/QR/SU/TK항공 추가(2016-08-11,김지영과장요청)
                    //PR항공 추가(2016-11-18,김승미과장요청)
                    //AI/AM/CX/EK/HA/HX/HY/PS/PX/VJ항공 추가(2016-11-21,김지영과장요청)
                    //MF항공 추가(2017-04-20,김경미차장요청)
                    //AC항공은 SR 입력으로 변경(2019-07-23,김경미매니저)
                    if ("/7C/LH/QR/".IndexOf(MCC) != -1)
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                        TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                        MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                        ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                        ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                        FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                        ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
                        ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
                        ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
                        FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                        FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "7";
                        FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                        FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingTelFormat(MCC, frText, false);
                        NewDataElementsIndiv.RemoveChild(TicketElement);
                        NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                        NewDataElementsIndiv.RemoveChild(ServiceRequest);
                        NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                    }
                    else if ("/MU/CA/CZ/CI/JL/DL/SQ/AY/ZE/AA/BA/ET/EY/NH/NX/SU/TK/PR/AI/AM/CX/EK/HA/HX/HY/PS/PX/VJ/MF/".IndexOf(MCC) != -1)
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                        TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                        MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                        ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                        ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                        FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                        ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                        ElementManagementData.SelectSingleNode("segmentName").InnerText = "OS";
                        FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                        FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P27";
                        FreetextData.SelectSingleNode("freetextDetail/companyId").InnerText = MCC;
                        //FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                        FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingTelFormat(MCC, frText, true);
                        NewDataElementsIndiv.RemoveChild(TicketElement);
                        NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                        NewDataElementsIndiv.RemoveChild(ServiceRequest);
                        NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                    }
                    else if (MCC.Equals("AC"))
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                        TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                        MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                        ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                        ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                        FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                        ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                        ElementManagementData.SelectSingleNode("segmentName").InnerText = "SSR";
                        ServiceRequest.SelectSingleNode("ssr/type").InnerText = "CTCM";
                        ServiceRequest.SelectSingleNode("ssr/companyId").InnerText = MCC;
                        ServiceRequest.SelectSingleNode("ssr/freetext").InnerText = String.Concat("82", frText.Replace("-", "").Substring(1));
                        ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/status"));
                        ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/quantity"));
                        ReferenceForDataElement.SelectSingleNode("reference/qualifier").InnerText = "PT";
                        ReferenceForDataElement.SelectSingleNode("reference/number").InnerText = PMNRef.ToString();
                        NewDataElementsIndiv.RemoveChild(TicketElement);
                        NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                        NewDataElementsIndiv.RemoveChild(FreetextData);
                    }
				}

                PMNRef++;
			}

			//이메일(예약자)
			if (!String.IsNullOrWhiteSpace(cm.RequestString(REA)))
			{
				//예약자 이메일을 첫번째 탑승객에 등록(2019-07-24,김경미매니저)
                if (String.IsNullOrWhiteSpace(cm.RequestString(PEA[0])))
                    PEA[0] = REA;
                
                NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
				ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
				TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
				MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
				FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

				ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
				ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
				ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
				FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
				FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P02";
                FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
				FreetextData.SelectSingleNode("longFreetext").InnerText = REA;
				NewDataElementsIndiv.RemoveChild(TicketElement);
				NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                NewDataElementsIndiv.RemoveChild(ServiceRequest);
                NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);

                //CI항공사는 항공사에서 원하는 양식이 따로 있어 추가 등록해 준다.(2015-10-06,김승미과장요청)
                //ZE/AA/CA/ET항공 추가(2016-08-11,김지영과장요청)
                //AI/AM/BA/CX/DL/EK/HY항공 추가(2016-11-21,김지영과장요청)
                //MU항공 추가(2017-08-25,김경미차장장요청)
                if ("/CI/ZE/AA/CA/ET/AI/AM/BA/CX/DL/EK/HY/MU/".IndexOf(MCC) != -1)
                {
                    NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                    ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                    TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                    MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                    FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                    ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                    ElementManagementData.SelectSingleNode("segmentName").InnerText = "OS";
                    FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                    FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P27";
                    FreetextData.SelectSingleNode("freetextDetail/companyId").InnerText = MCC;
                    FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingEmailFormat(MCC, REA, true);
                    NewDataElementsIndiv.RemoveChild(TicketElement);
                    NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                }
			}

			//이메일(탑승객)
            int PEARef = 2;
			foreach (string frText in PEA)
			{
				if (!String.IsNullOrWhiteSpace(frText))
				{
					NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
					ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
					TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
					MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
					FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

					ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
					ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
					ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
					FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
					FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P02";
                    FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
					FreetextData.SelectSingleNode("longFreetext").InnerText = frText;
					NewDataElementsIndiv.RemoveChild(TicketElement);
					NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                    NewDataElementsIndiv.RemoveChild(ServiceRequest);
                    NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);

                    //CI항공사는 항공사에서 원하는 양식이 따로 있어 추가 등록해 준다.(2015-10-06,김승미과장요청)
                    //ZE/AA/CA항공 추가(2016-08-11,김지영과장요청)
                    //AI/AM/BA/CX/DL/EK/HY항공 추가(2016-11-21,김지영과장요청)
                    //MU항공 추가(2017-08-25,김경미차장장요청)
                    //AC항공은 SR 입력으로 추가(2019-07-23,김경미매니저)
                    if ("/CI/ZE/AA/CA/ET/AI/AM/BA/CX/DL/EK/HY/MU/".IndexOf(MCC) != -1)
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                        TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                        MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                        ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                        ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                        FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                        ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                        ElementManagementData.SelectSingleNode("segmentName").InnerText = "OS";
                        FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                        FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P27";
                        FreetextData.SelectSingleNode("freetextDetail/companyId").InnerText = MCC;
                        FreetextData.SelectSingleNode("longFreetext").InnerText = Common.BookingEmailFormat(MCC, frText, true);
                        NewDataElementsIndiv.RemoveChild(TicketElement);
                        NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                        NewDataElementsIndiv.RemoveChild(ServiceRequest);
                        NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                    }
                    else if (MCC.Equals("AC"))
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                        TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                        MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                        ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                        ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                        FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                        ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                        ElementManagementData.SelectSingleNode("segmentName").InnerText = "SSR";
                        ServiceRequest.SelectSingleNode("ssr/type").InnerText = "CTCE";
                        ServiceRequest.SelectSingleNode("ssr/companyId").InnerText = MCC;
                        ServiceRequest.SelectSingleNode("ssr/freetext").InnerText = frText.Replace("@", "//").Replace("_", "..").Replace("-", "./").ToUpper();
                        ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/status"));
                        ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/quantity"));
                        ReferenceForDataElement.SelectSingleNode("reference/qualifier").InnerText = "PT";
                        ReferenceForDataElement.SelectSingleNode("reference/number").InnerText = PEARef.ToString();
                        NewDataElementsIndiv.RemoveChild(TicketElement);
                        NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                        NewDataElementsIndiv.RemoveChild(FreetextData);
                    }
				}

                PEARef++;
			}

            //AC항공은 OS 입력으로 14살 미만 승객에 대한 나이 입력 추가(2019-07-23,김경미매니저)
            if (MCC.Equals("AC"))
            {
                string DD = Segs[0].Attributes.GetNamedItem("ddt").InnerText.Substring(0, 10);

                foreach (string ACBirthInfo in ACBirth.Split('^'))
                {
                    if (!String.IsNullOrWhiteSpace(ACBirthInfo))
                    {
                        string[] BirthInfo = ACBirthInfo.Split('/');
                        int KAge = cm.KoreanAge(cm.RequestDateTime(BirthInfo[1], "yyyy-MM-dd"), DD);

                        if (KAge < 14)
                        {
                            NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                            ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
                            TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
                            MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                            ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                            ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
                            FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

                            ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
                            ElementManagementData.SelectSingleNode("segmentName").InnerText = "OS";
                            FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                            FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P27";
                            FreetextData.SelectSingleNode("freetextDetail/companyId").InnerText = MCC;
                            FreetextData.SelectSingleNode("longFreetext").InnerText = String.Format("CHD {0}YRS/P{1}", KAge, BirthInfo[0]);
                            NewDataElementsIndiv.RemoveChild(TicketElement);
                            NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
                            NewDataElementsIndiv.RemoveChild(ServiceRequest);
                            NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
                        }
                    }
                }
            }

			//RF(Received From Element)
			NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
			ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
			TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
			MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
            ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
            ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
			FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

			ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
			ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
			ElementManagementData.SelectSingleNode("segmentName").InnerText = "RF";
			FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
			FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "P22";
            FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
			FreetextData.SelectSingleNode("longFreetext").InnerText = String.Concat("WebService-", RQT);
			NewDataElementsIndiv.RemoveChild(TicketElement);
			NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
            NewDataElementsIndiv.RemoveChild(ServiceRequest);
            NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);

			//TK(Ticket Arragnement Element)
			NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
			ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
			TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
			MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
            ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
            ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
			FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

			ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
			ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
			ElementManagementData.SelectSingleNode("segmentName").InnerText = "TK";

			if (OPN.Equals("Y"))
			{
				TicketElement.SelectSingleNode("passengerType").InnerText = "PAX";
				TicketElement.SelectSingleNode("ticket/indicator").InnerText = "OK";
				TicketElement.SelectSingleNode("ticket").RemoveChild(TicketElement.SelectSingleNode("ticket/date"));
				TicketElement.SelectSingleNode("ticket").RemoveChild(TicketElement.SelectSingleNode("ticket/time"));
			}
			else
			{
				//TicketElement.SelectSingleNode("ticket/indicator").InnerText = "TL";
				//TicketElement.SelectSingleNode("ticket/date").InnerText = cm.ModeTL(SNM).ToString("ddMMyy");
				//TicketElement.SelectSingleNode("ticket/time").InnerText = "1700";
				TicketElement.SelectSingleNode("ticket/indicator").InnerText = "OK";
				TicketElement.SelectSingleNode("ticket").RemoveChild(TicketElement.SelectSingleNode("ticket/date"));
				TicketElement.SelectSingleNode("ticket").RemoveChild(TicketElement.SelectSingleNode("ticket/time"));
				TicketElement.RemoveChild(TicketElement.SelectSingleNode("passengerType"));
			}

            NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
            NewDataElementsIndiv.RemoveChild(ServiceRequest);
            NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
			NewDataElementsIndiv.RemoveChild(FreetextData);

			//RM(Remarks)
			if (cm.RequestString(RMK).Length > 0)
			{
				NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
				ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
				TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
				MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
                ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
                ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
				FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

				ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
				ElementManagementData.SelectSingleNode("reference/number").InnerText = (OTNum++).ToString();
				ElementManagementData.SelectSingleNode("segmentName").InnerText = "RM";
				MiscellaneousRemark.SelectSingleNode("remarks/type").InnerText = "RM";
				MiscellaneousRemark.SelectSingleNode("remarks/freetext").InnerText = RMK;
				NewDataElementsIndiv.RemoveChild(TicketElement);
                NewDataElementsIndiv.RemoveChild(ServiceRequest);
                NewDataElementsIndiv.RemoveChild(ReferenceForDataElement);
				NewDataElementsIndiv.RemoveChild(FreetextData);
			}

			DataElementsMaster.RemoveChild(DataElementsIndiv);

            //### 05.기내식(KE 국제선의 경우 유/소아 기내식 필수) #####
            //foreach (XmlNode TmpTravellerInfo in PAME.SelectNodes("travellerInfo[passengerData/travellerInformation/passenger/type='CHD']"))
            //{
            //    NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
            //    ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
            //    TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
            //    MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
            //    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
            //    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
            //    FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

            //    ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
            //    ElementManagementData.SelectSingleNode("segmentName").InnerText = "SSR";
            //    ServiceRequest.SelectSingleNode("ssr/type").InnerText = "CHML";
            //    ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/freetext"));
            //    ReferenceForDataElement.SelectSingleNode("reference/number").InnerText = TmpTravellerInfo.SelectSingleNode("elementManagementPassenger/reference/number").InnerText;
            //    NewDataElementsIndiv.RemoveChild(TicketElement);
            //    NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
            //    NewDataElementsIndiv.RemoveChild(FreetextData);
            //}

            //foreach (XmlNode TmpTravellerInfo in PAME.SelectNodes("travellerInfo[passengerData/travellerInformation/passenger/type='INF']"))
            //{
            //    NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
            //    ElementManagementData = NewDataElementsIndiv.SelectSingleNode("elementManagementData");
            //    TicketElement = NewDataElementsIndiv.SelectSingleNode("ticketElement");
            //    MiscellaneousRemark = NewDataElementsIndiv.SelectSingleNode("miscellaneousRemark");
            //    ServiceRequest = NewDataElementsIndiv.SelectSingleNode("serviceRequest");
            //    ReferenceForDataElement = NewDataElementsIndiv.SelectSingleNode("referenceForDataElement");
            //    FreetextData = NewDataElementsIndiv.SelectSingleNode("freetextData");

            //    ElementManagementData.RemoveChild(ElementManagementData.SelectSingleNode("reference"));
            //    ElementManagementData.SelectSingleNode("segmentName").InnerText = "SSR";
            //    ServiceRequest.SelectSingleNode("ssr/type").InnerText = "BBML";
            //    ServiceRequest.SelectSingleNode("ssr").RemoveChild(ServiceRequest.SelectSingleNode("ssr/freetext"));
            //    ReferenceForDataElement.SelectSingleNode("reference/number").InnerText = TmpTravellerInfo.SelectSingleNode("elementManagementPassenger/reference/number").InnerText;
            //    NewDataElementsIndiv.RemoveChild(TicketElement);
            //    NewDataElementsIndiv.RemoveChild(MiscellaneousRemark);
            //    NewDataElementsIndiv.RemoveChild(FreetextData);
            //}

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="PTC">탑승객 타입 코드 (ADT/CHD/INF)</param>
		/// <param name="PTL">탑승객 타이틀 (MR/MRS/MS/MSTR/MISS)</param>
		/// <param name="PHN">탑승객 한글이름</param>
		/// <param name="PSN">탑승객 영문성 (SurName)</param>
		/// <param name="PFN">탑승객 영문이름 (First Name)</param>
		/// <param name="PBD">탑승객 생년월일 (YYYYMMDD) (소아,유아일 경우 필수)</param>
		/// <param name="PTN">탑승객 전화번호</param>
		/// <param name="PMN">탑승객 휴대폰</param>
		/// <param name="PEA">탑승객 이메일주소</param>
		/// <param name="RTN">예약자 전화번호</param>
		/// <param name="RMN">예약자 휴대폰</param>
		/// <param name="REA">예약자 이메일주소</param>
		/// <param name="RMK">추가요청사항</param>
		/// <param name="RQT">요청단말기(WEB/MOBILE/CRS/MODEWARE)</param>
		/// <param name="SNM">사이트번호</param>
        /// <param name="ANM">거래처번호</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="MCC">마케팅항공사</param>
		/// <param name="SXL">여정 XML Element</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsRS")]
		public XmlElement AddMultiElementsRS(string SID, string SQN, string SCT, string GUID, string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string RTN, string RMN, string REA, string RMK, string RQT, int SNM, int ANM, string OPN, string MCC, XmlElement SXL)
		{
            XmlElement ReqXml = AddMultiElementsRQ(PTC, PTL, PHN, PSN, PFN, PBD, PTN, PMN, PEA, RTN, RMN, REA, RMK, RQT, SNM, ANM, OPN, MCC, SXL);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsRQ", "Y", GUID);
			
			XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "PNR_AddMultiElementsXml")]
		public XmlElement AddMultiElementsXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("AddMultiElements", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "AddMultiElements"

		#region "AddMultiElements(ReceiveFrom)"

		[WebMethod(Description = "PNR_AddMultiElementsReceiveFromRQ")]
		public XmlElement AddMultiElementsReceiveFromRQ(string ReceiveFrom)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsReceiveFromRQ"));

			XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/freetextData/longFreetext").InnerText = ReceiveFrom;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements(ReceiveFrom)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="ReceiveFrom">작업자 정보</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsReceiveFromRS")]
		public XmlElement AddMultiElementsReceiveFromRS(string SID, string SQN, string SCT, string GUID, string ReceiveFrom)
		{
			XmlElement ReqXml = AddMultiElementsReceiveFromRQ(ReceiveFrom);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsReceiveFromRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsReceiveFromRS", "Y", GUID);

			return ResXml;
		}

		#endregion "AddMultiElements(ReceiveFrom)"

		#region "AddMultiElements(MEAL)"

		[WebMethod(Description = "PNR_AddMultiElementsMEALRQ")]
		public XmlElement AddMultiElementsMEALRQ(string[] RefNumer, string[] MealCode)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsMEALRQ"));

			XmlNode DataElementsMaster = XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster");
			XmlNode DataElementsIndiv = DataElementsMaster.SelectSingleNode("dataElementsIndiv");
			XmlNode NewDataElementsIndiv;

			for (int i = 0; i < RefNumer.Length; i++)
			{
				if (!String.IsNullOrWhiteSpace(RefNumer[i]))
			    {
			        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
			        NewDataElementsIndiv.SelectSingleNode("serviceRequest/ssr/type").InnerText = MealCode[i].Trim();
			        NewDataElementsIndiv.SelectSingleNode("referenceForDataElement/reference/number").InnerText = RefNumer[i].Trim();
			    }
			}

			DataElementsMaster.RemoveChild(DataElementsIndiv);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements(MEAL)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="RefNumer">탑승객 참조번호</param>
		/// <param name="MealCode">기내식코드</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsMEALRS")]
		public XmlElement AddMultiElementsMEALRS(string SID, string SQN, string SCT, string GUID, string[] RefNumer, string[] MealCode)
		{
			XmlElement ReqXml = AddMultiElementsMEALRQ(RefNumer, MealCode);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsMEALRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsMEALRS", "Y", GUID);

			return ResXml;
		}

		#endregion "AddMultiElements(MEAL)"

		#region "AddMultiElements(APIS)"

		[WebMethod(Description = "PNR_AddMultiElementsAPISRQ")]
		public XmlElement AddMultiElementsAPISRQ(string[] SurName, string[] GivenName, string[] PaxGender, string[] BirthDate, string[] PassportNum, string[] ExpireDate, string[] IssueCountry, string[] HolderNationality, XmlElement PNRXml)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsAPISRQ"));

			XmlNode DataElementsMaster = XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster");
			XmlNode DataElementsIndiv = DataElementsMaster.SelectSingleNode("dataElementsIndiv");
			XmlNode NewDataElementsIndiv;
			bool SameTraveller = false;
			bool Infant = false;
			int i = 0;

			XmlNamespaceManager xnMgr = new XmlNamespaceManager(PNRXml.OwnerDocument.NameTable);
			xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("PNR_Reply"));

			foreach (XmlNode TravellerInfo in PNRXml.SelectNodes("m:travellerInfo/m:enhancedPassengerData", xnMgr))
			{
				SameTraveller = false;
				Infant = (TravellerInfo.SelectNodes("m:enhancedTravellerInformation/m:travellerNameInfo/m:type", xnMgr).Count >0 && TravellerInfo.SelectSingleNode("m:enhancedTravellerInformation/m:travellerNameInfo/m:type", xnMgr).InnerText.Equals("INF")) ? true : false;
				for (i = 0; i < SurName.Length; i++)
				{
                    if (TravellerInfo.SelectSingleNode("m:enhancedTravellerInformation/m:otherPaxNamesDetails/m:surname", xnMgr).InnerText.Equals(SurName[i].Trim()) && GivenName[i].Trim().Equals(cm.SplitPaxType(TravellerInfo.SelectSingleNode("m:enhancedTravellerInformation/m:otherPaxNamesDetails/m:givenName", xnMgr).InnerText.Trim(), Infant)[1]))
					{
						SameTraveller = true;
						break;
					}
				}

				if (SameTraveller)
				{
                    if (!String.IsNullOrWhiteSpace(SurName[i]) && !String.IsNullOrWhiteSpace(GivenName[i]) && !String.IsNullOrWhiteSpace(PassportNum[i]))
                    {
                        NewDataElementsIndiv = DataElementsMaster.AppendChild(DataElementsIndiv.CloneNode(true));
                        NewDataElementsIndiv.SelectSingleNode("serviceRequest/ssr/freetext[1]").InnerText = String.Format("P-{0}-{1}-{2}-{3}-{4}-{5}-", IssueCountry[i], PassportNum[i], HolderNationality[i], cm.AbacusDateTime(BirthDate[i]), String.Concat(PaxGender[i], (Infant) ? "I" : ""), cm.AbacusDateTime(ExpireDate[i]));
                        NewDataElementsIndiv.SelectSingleNode("serviceRequest/ssr/freetext[2]").InnerText = String.Format("{0}-{1}", SurName[i], GivenName[i]);
                        NewDataElementsIndiv.SelectSingleNode("referenceForDataElement/reference/number").InnerText = TravellerInfo.ParentNode.SelectSingleNode("m:elementManagementPassenger/m:reference[m:qualifier='PT']/m:number", xnMgr).InnerText;
                    }
				}
			}

			DataElementsMaster.RemoveChild(DataElementsIndiv);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements(APIS)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="SurName">영문성</param>
		/// <param name="GivenName">영문이름</param>
		/// <param name="PaxGender">성별(M/F)</param>
		/// <param name="BirthDate">생년월일</param>
		/// <param name="PassportNum">여권번호</param>
		/// <param name="ExpireDate">여권만료일</param>
		/// <param name="IssueCountry">여권발행국</param>
		/// <param name="HolderNationality">국적</param>
		/// <param name="PNRXml">PNR 조회 결과</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsAPISRS")]
		public XmlElement AddMultiElementsAPISRS(string SID, string SQN, string SCT, string GUID, string[] SurName, string[] GivenName, string[] PaxGender, string[] BirthDate, string[] PassportNum, string[] ExpireDate, string[] IssueCountry, string[] HolderNationality, XmlElement PNRXml)
		{
			XmlElement ReqXml = AddMultiElementsAPISRQ(SurName, GivenName, PaxGender, BirthDate, PassportNum, ExpireDate, IssueCountry, HolderNationality, PNRXml);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsAPISRQ", "Y", GUID);
				
			XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsAPISRS", "Y", GUID);

			return ResXml;
		}

		#endregion "AddMultiElements(APIS)"

		#region "AddMultiElements(PnrActions)"

		[WebMethod(Description = "PNR_AddMultiElementsActionsRQ")]
		public XmlElement AddMultiElementsActionsRQ(string AOC)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsActionsRQ"));

			XmlDoc.SelectSingleNode("PNR_AddMultiElements/pnrActions/optionCode").InnerText = AOC;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements(PnrActions)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="AOC">Actions Option Code</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsActionsRS")]
		public XmlElement AddMultiElementsActionsRS(string SID, string SQN, string SCT, string GUID, string AOC)
		{
			XmlElement ReqXml = AddMultiElementsActionsRQ(AOC);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsActionsRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsActionsRS", "Y", GUID);

			return ResXml;
		}

		#endregion "AddMultiElements(PnrActions)"

        #region "AddMultiElements(Commission)"

        [WebMethod(Description = "PNR_AddMultiElementsCommissionRQ")]
        public XmlElement AddMultiElementsCommissionRQ(string Commission)
		{
            Regex regex = new Regex(@"[0-9.]+[A-Z]{0,1}$", RegexOptions.Singleline);
            Match match = regex.Match(Commission);

            string TmpCommission = match.Value;
            string TmpCommissionNumber = Regex.Replace(TmpCommission, @"[^0-9.]", "");
            string TmpCommissionKey = cm.Right(TmpCommission, 1);

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AddMultiElementsCommissionRQ"));

            XmlNode CommissionInfo = XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/commission/commissionInfo");

            if (TmpCommissionKey.Equals("A"))
            {
                if (cm.IsInteger(TmpCommissionNumber))
                {
                    //Amount Com
                    CommissionInfo.SelectSingleNode("amount").InnerText = TmpCommissionNumber;
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("percentage"));
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("vatIndicator"));
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("remitIndicator"));
                }
                else
                {
                    //Gross Com
                    CommissionInfo.SelectSingleNode("percentage").InnerText = TmpCommissionNumber;
                    CommissionInfo.SelectSingleNode("vatIndicator").InnerText = "G";
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("amount"));
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("remitIndicator"));
                }
            }
            else if (TmpCommissionKey.Equals("N"))
            {
                if (cm.IsNumeric(TmpCommissionNumber) && cm.RequestDouble(TmpCommissionNumber).Equals(0))
                {
                    //Gross Com
                    CommissionInfo.SelectSingleNode("percentage").InnerText = TmpCommissionNumber;
                    CommissionInfo.SelectSingleNode("vatIndicator").InnerText = "G";
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("amount"));
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("remitIndicator"));
                }
                else
                {
                    //Net Com
                    CommissionInfo.SelectSingleNode("percentage").InnerText = TmpCommissionNumber;
                    CommissionInfo.SelectSingleNode("remitIndicator").InnerText = "P";
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("amount"));
                    CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("vatIndicator"));
                }
            }
            else
            {
                //Gross Com
                CommissionInfo.SelectSingleNode("percentage").InnerText = TmpCommissionNumber;
                CommissionInfo.SelectSingleNode("vatIndicator").InnerText = "G";
                CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("amount"));
                CommissionInfo.RemoveChild(CommissionInfo.SelectSingleNode("remitIndicator"));
            }
            
			return XmlDoc.DocumentElement;
		}

		/// <summary>
        /// AddMultiElements(Commission)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="Commission">커미션 정보</param>
		/// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsCommissionRS")]
        public XmlElement AddMultiElementsCommissionRS(string SID, string SQN, string SCT, string GUID, string Commission)
		{
            //XmlElement ReqXml = AddMultiElementsCommissionRQ(Commission);
            //cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsCommissionRQ", "Y", GUID);

            //XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            //cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsCommissionRS", "Y", GUID);

            //return ResXml;


            //G인디케이터 오류 관련 임시로 Command 방식으로 처리(2016-04-27)
            Regex regex = new Regex(@"[0-9.]+[A-Z]{0,1}$", RegexOptions.Singleline);
            Match match = regex.Match(Commission);

            string TmpCommission = match.Value;
            string TmpCommissionNumber = Regex.Replace(TmpCommission, @"[^0-9.]", "");
            string TmpCommissionKey = cm.Right(TmpCommission, 1);

            if (TmpCommissionKey.Equals("A"))
            {
                if (cm.IsInteger(TmpCommissionNumber))
                    Commission = TmpCommissionNumber;
                else
                    Commission = String.Concat(TmpCommissionNumber, "G");
            }
            else if (TmpCommissionKey.Equals("N"))
            {
                if (cm.IsNumeric(TmpCommissionNumber) && cm.RequestDouble(TmpCommissionNumber).Equals(0))
                    Commission = String.Concat(TmpCommissionNumber, "G");
                else
                    Commission = String.Concat(TmpCommissionNumber, "N");
            }
            else
                Commission = String.Concat(TmpCommissionNumber, "G");

            return AddMultiElementsCommissionModeRS(SID, SQN, SCT, GUID, Commission);
		}

        /// <summary>
        /// AddMultiElements(Modetour Commission)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="Commission">커미션 정보</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsCommissionModeRS")]
        public XmlElement AddMultiElementsCommissionModeRS(string SID, string SQN, string SCT, string GUID, string Commission)
        {
            return CommandCrypticRS(SID, SQN, SCT, GUID, String.Format("FM{0}", Commission));
        }

        #endregion "AddMultiElements(Commission)"

        #region "AddMultiElements(TourCode)"

        [WebMethod(Description = "PNR_AddMultiElementsTourCodeRQ")]
        public XmlElement AddMultiElementsTourCodeRQ(string PTC, string TourCode, int ActionCode)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AddMultiElementsTourCodeRQ"));

            XmlDoc.SelectSingleNode("PNR_AddMultiElements/pnrActions/optionCode").InnerText = ActionCode.ToString();

            XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/tourCode/passengerType").InnerText = PTC.Equals("INF") ? "INF" : "PAX";
            XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/tourCode/freeFormatTour/freetext").InnerText = TourCode;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// AddMultiElements(TourCode)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="TourCode">투어코드</param>
        /// <param name="ActionCode">PNR 적용코드(0:기본, 10:ET, 11:ER)</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsTourCodeRS")]
        public XmlElement AddMultiElementsTourCodeRS(string SID, string SQN, string SCT, string GUID, string PTC, string TourCode, int ActionCode)
        {
            XmlElement ReqXml = AddMultiElementsTourCodeRQ(PTC, TourCode, ActionCode);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsTourCodeRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsTourCodeRS", "Y", GUID);

            return ResXml;
        }

        #endregion "AddMultiElements(TourCode)"

        #region "AddMultiElements(Endorsement)"

        [WebMethod(Description = "PNR_AddMultiElementsEndorsementRQ")]
        public XmlElement AddMultiElementsEndorsementRQ(string PTC, string Endorsement, int ActionCode)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AddMultiElementsEndorsementRQ"));

            XmlDoc.SelectSingleNode("PNR_AddMultiElements/pnrActions/optionCode").InnerText = ActionCode.ToString();

            XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/fareElement/passengerType").InnerText = PTC.Equals("INF") ? "INF" : "PAX";
            XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/fareElement/freetextLong").InnerText = Endorsement;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// AddMultiElements(Endorsement)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="Endorsement">엔도스</param>
        /// <param name="ActionCode">PNR 적용코드(0:기본, 10:ET, 11:ER)</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsEndorsementRS")]
        public XmlElement AddMultiElementsEndorsementRS(string SID, string SQN, string SCT, string GUID, string PTC, string Endorsement, int ActionCode)
        {
            XmlElement ReqXml = AddMultiElementsEndorsementRQ(PTC, Endorsement, ActionCode);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsEndorsementRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsEndorsementRS", "Y", GUID);

            return ResXml;
        }

        #endregion "AddMultiElements(Endorsement)"

        #region "AddMultiElements(PaxDC)"

        [WebMethod(Description = "PNR_AddMultiElementsPaxDCRQ")]
		public XmlElement AddMultiElementsPaxDCRQ(string REF, string DCCode)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("AddMultiElementsPaxDCRQ"));

			XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/fareDiscount/discount/adjustmentReason").InnerText = DCCode;
			XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/referenceForDataElement/reference/number").InnerText = REF;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// AddMultiElements(PaxDC)
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="REF">참조번호</param>
		/// <param name="DCCode">할인코드</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_AddMultiElementsPaxDCRS")]
		public XmlElement AddMultiElementsPaxDCRS(string SID, string SQN, string SCT, string GUID, string REF, string DCCode)
		{
			XmlElement ReqXml = AddMultiElementsPaxDCRQ(REF, DCCode);
            //cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsPaxDCRQ", "Y", GUID);

			XmlElement ResXml = ac.HttpExecute("AddMultiElements", ReqXml, SID, SQN, SCT, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsPaxDCRS", "Y", GUID);
			
			return ResXml;
		}

		#endregion "AddMultiElements(PaxDC)"

        #region "AddMultiElements(Remark)"

        [WebMethod(Description = "PNR_AddMultiElementsRemarkRQ")]
        public XmlElement AddMultiElementsRemarkRQ(string Remark)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AddMultiElementsRemarkRQ"));

            XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv/miscellaneousRemark/remarks/freetext").InnerText = Remark;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// AddMultiElements(Remark)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="Remark">General Remark</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsRemarkRS")]
        public XmlElement AddMultiElementsRemarkRS(string SID, string SQN, string SCT, string GUID, string Remark)
        {
            try
            {
                XmlElement ReqXml = AddMultiElementsRemarkRQ(Remark);
                cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsRemarkRQ", "Y", GUID);

                XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
                cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsRemarkRS", "Y", GUID);

                return ResXml;
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "AddMultiElements(Remark)"

        #region "AddMultiElements(거래처연락처)"

        [WebMethod(Description = "PNR_AddMultiElementsTELRQ")]
        public XmlElement AddMultiElementsTELRQ(string ATN)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AddMultiElementsTELRQ"));

            if (!String.IsNullOrWhiteSpace(cm.RequestString(ATN)))
            {
                XmlNode DataElementsIndiv = XmlDoc.SelectSingleNode("PNR_AddMultiElements/dataElementsMaster/dataElementsIndiv");
                XmlNode ElementManagementData = DataElementsIndiv.SelectSingleNode("elementManagementData");
                XmlNode FreetextData = DataElementsIndiv.SelectSingleNode("freetextData");

                ElementManagementData.SelectSingleNode("reference/qualifier").InnerText = "OT";
                ElementManagementData.SelectSingleNode("reference/number").InnerText = "1";
                ElementManagementData.SelectSingleNode("segmentName").InnerText = "AP";
                FreetextData.SelectSingleNode("freetextDetail/subjectQualifier").InnerText = "3";
                FreetextData.SelectSingleNode("freetextDetail/type").InnerText = "5";
                FreetextData.SelectSingleNode("freetextDetail").RemoveChild(FreetextData.SelectSingleNode("freetextDetail/companyId"));
                FreetextData.SelectSingleNode("longFreetext").InnerText = ATN;
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// AddMultiElements(거래처연락처)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="ATN">거래처 전화번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR_AddMultiElementsTELRS")]
        public XmlElement AddMultiElementsTELRS(string SID, string SQN, string SCT, string GUID, string ATN)
        {
            XmlElement ReqXml = AddMultiElementsTELRQ(ATN);
            cm.XmlFileSave(ReqXml, ac.Name, "AddMultiElementsTELRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("AddMultiElements", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "AddMultiElementsTELRS", "Y", GUID);

            return ResXml;
        }

        #endregion "AddMultiElements"

        #region "Retrieve"

        [WebMethod(Description = "PNR_RetrieveRQ")]
		public XmlElement RetrieveRQ(string PNR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("RetrieveRQ"));

			XmlDoc.SelectSingleNode("PNR_Retrieve/retrievalFacts/reservationOrProfileIdentifier/reservation/controlNumber").InnerText = cm.ChangeAmadeusPNR(PNR);
				
			if (cm.IsInteger(PNR.Replace("-", "")))
				XmlDoc.SelectSingleNode("PNR_Retrieve/retrievalFacts/reservationOrProfileIdentifier/reservation/controlType").InnerText = "I";
			else
				XmlDoc.SelectSingleNode("PNR_Retrieve/retrievalFacts/reservationOrProfileIdentifier/reservation").RemoveChild(XmlDoc.SelectSingleNode("PNR_Retrieve/retrievalFacts/reservationOrProfileIdentifier/reservation/controlType"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// Retrieve
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="PNR">항공사 예약번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_RetrieveRS")]
		public XmlElement RetrieveRS(string SID, string SQN, string SCT, string GUID, string PNR)
		{
			XmlElement ReqXml = RetrieveRQ(PNR);
            //cm.XmlFileSave(ReqXml, ac.Name, "RetrieveRQ", "Y", GUID);

			XmlElement ResXml = ac.HttpExecute("Retrieve", ReqXml, SID, SQN, SCT, GUID);
            cm.XmlFileSave(ResXml, ac.Name, "RetrieveRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "PNR_RetrieveXml")]
		public XmlElement RetrieveXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("Retrieve", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "Retrieve"

		#region "Cancel"

		[WebMethod(Description = "PNR_CancelRQ")]
		public XmlElement CancelRQ(string PNR)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CancelRQ"));

			XmlDoc.SelectSingleNode("PNR_Cancel/reservationInfo/reservation/controlNumber").InnerText = cm.ChangeAmadeusPNR(PNR);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// Cancel
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="PNR">항공사 예약번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR_CancelRS")]
		public XmlElement CancelRS(string SID, string SQN, string SCT, string GUID, string PNR)
		{
			XmlElement ReqXml = CancelRQ(PNR);
            cm.XmlFileSave(ReqXml, ac.Name, "CancelRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("Cancel", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CancelRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "PNR_CancelXml")]
		public XmlElement CancelXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("Cancel", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "Cancel"

		#endregion "PNR"

		#region "Ticket"

        #region "CreateTSTFromPricing(TST생성)"

        [WebMethod(Description = "Ticket_CreateTSTFromPricingRQ")]
		public XmlElement CreateTSTFromPricingRQ(XmlElement PricePNRWithBookingClass, string NamespaceURL)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CreateTSTFromPricingRQ"));

			XmlNamespaceManager xnMgr = new XmlNamespaceManager(PricePNRWithBookingClass.OwnerDocument.NameTable);
			xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL(NamespaceURL));

			XmlNode CTST = XmlDoc.SelectSingleNode("Ticket_CreateTSTFromPricing");
			XmlNode PsaList = CTST.SelectSingleNode("psaList");
			XmlNode NewPsaList;

            //탑승객지정(미사용)
            PsaList.RemoveChild(PsaList.SelectSingleNode("paxReference"));

			foreach (XmlNode FareList in PricePNRWithBookingClass.SelectNodes("m:fareList", xnMgr))
			{
				NewPsaList = CTST.AppendChild(PsaList.CloneNode(true));
				NewPsaList.SelectSingleNode("itemReference/uniqueReference").InnerText = FareList.SelectSingleNode("m:fareReference[m:referenceType='TST']/m:uniqueReference", xnMgr).InnerText;
			}

			CTST.RemoveChild(PsaList);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// TST생성
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="PricePNRWithBookingClass">운임Pricing의 결과 XMLElement</param>
		/// <param name="NamespaceURL">네임스페이스</param>
		/// <returns></returns>
        [WebMethod(Description = "Ticket_CreateTSTFromPricingRS(TST생성)")]
		public XmlElement CreateTSTFromPricingRS(string SID, string SQN, string SCT, string GUID, XmlElement PricePNRWithBookingClass, string NamespaceURL)
		{
			XmlElement ReqXml = CreateTSTFromPricingRQ(PricePNRWithBookingClass, NamespaceURL);
            cm.XmlFileSave(ReqXml, ac.Name, "CreateTSTFromPricingRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("CreateTSTFromPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CreateTSTFromPricingRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Ticket_CreateTSTFromPricingXml")]
		public XmlElement CreateTSTFromPricingXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("CreateTSTFromPricing", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

        #endregion "CreateTSTFromPricing(TST생성)"

        #region "CreateTSTFromPricing(탑승객별 TST생성)"

        [WebMethod(Description = "Ticket_CreateTSTFromPricingRQ")]
        public XmlElement CreateTSTFromPricingPaxRQ(XmlElement PricePNRWithBookingClass, string NamespaceURL)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("CreateTSTFromPricingRQ"));

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(PricePNRWithBookingClass.OwnerDocument.NameTable);
            xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL(NamespaceURL));

            XmlNode CTST = XmlDoc.SelectSingleNode("Ticket_CreateTSTFromPricing");
            XmlNode PsaList = CTST.SelectSingleNode("psaList");
            XmlNode NewPsaList;

            foreach (XmlNode FareList in PricePNRWithBookingClass.SelectNodes("m:fareList", xnMgr))
            {
                NewPsaList = CTST.AppendChild(PsaList.CloneNode(true));
                NewPsaList.SelectSingleNode("itemReference/uniqueReference").InnerText = FareList.SelectSingleNode("m:fareReference[m:referenceType='TST']/m:uniqueReference", xnMgr).InnerText;

                NewPsaList.SelectSingleNode("paxReference/refDetails/refQualifier").InnerText = FareList.SelectSingleNode("m:paxSegReference/m:refDetails/m:refQualifier", xnMgr).InnerText;
                NewPsaList.SelectSingleNode("paxReference/refDetails/refNumber").InnerText = FareList.SelectSingleNode("m:paxSegReference/m:refDetails/m:refNumber", xnMgr).InnerText;
            }

            CTST.RemoveChild(PsaList);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 탑승객별 TST생성
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PricePNRWithBookingClass">운임Pricing의 결과 XMLElement</param>
        /// <param name="NamespaceURL">네임스페이스</param>
        /// <returns></returns>
        [WebMethod(Description = "Ticket_CreateTSTFromPricingRS(탑승객별 TST생성)")]
        public XmlElement CreateTSTFromPricingPaxRS(string SID, string SQN, string SCT, string GUID, XmlElement PricePNRWithBookingClass, string NamespaceURL)
        {
            XmlElement ReqXml = CreateTSTFromPricingPaxRQ(PricePNRWithBookingClass, NamespaceURL);
            cm.XmlFileSave(ReqXml, ac.Name, "CreateTSTFromPricingPaxRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("CreateTSTFromPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CreateTSTFromPricingPaxRS", "Y", GUID);

            return ResXml;
        }

        #endregion "CreateTSTFromPricing(탑승객별 TST생성)"

        #region "CreateTSTFromPricing(승객별)"

        [WebMethod(Description = "Ticket_CreateTSTFromPricingPAXRQ")]
        public XmlElement CreateTSTFromPricingPAXRQ(string TSTNumber, string PAXNumber)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("CreateTSTFromPricingRQ"));

            if (TSTNumber.IndexOf(",") != -1)
            {
                XmlNode TicketCreateTSTFromPricing = XmlDoc.SelectSingleNode("Ticket_CreateTSTFromPricing");
                XmlNode PsaList = TicketCreateTSTFromPricing.SelectSingleNode("psaList");
                XmlNode NewPsaList;

                string[] TSTN = TSTNumber.Split(',');
                string[] PAXN = PAXNumber.Split(',');

                for (int i = 0; i < TSTN.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(TSTN[i]))
                    {
                        NewPsaList = TicketCreateTSTFromPricing.AppendChild(PsaList.CloneNode(true));
                        NewPsaList.SelectSingleNode("itemReference/uniqueReference").InnerText = TSTN[i].Trim();

                        XmlNode PaxReference = NewPsaList.SelectSingleNode("paxReference");
                        XmlNode RefDetails = PaxReference.SelectSingleNode("refDetails");
                        XmlNode NewRefDetails;

                        foreach (string RefNumber in PAXN[i].Split('/'))
                        {
                            if (!String.IsNullOrWhiteSpace(RefNumber))
                            {
                                NewRefDetails = PaxReference.AppendChild(RefDetails.CloneNode(true));
                                NewRefDetails.SelectSingleNode("refNumber").InnerText = RefNumber.Trim();
                            }
                        }

                        PaxReference.RemoveChild(RefDetails);
                        NewPsaList.RemoveChild(PaxReference);
                    }
                }

                TicketCreateTSTFromPricing.RemoveChild(PsaList);
            }
            else
            {
                XmlDoc.SelectSingleNode("Ticket_CreateTSTFromPricing/psaList/itemReference/uniqueReference").InnerText = TSTNumber;
                XmlDoc.SelectSingleNode("Ticket_CreateTSTFromPricing/psaList/paxReference/refDetails/refNumber").InnerText = PAXNumber;
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// TST생성(승객별)
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="TSTNumber">TST 번호</param>
        /// <param name="PAXNumber">PAX 번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Ticket_CreateTSTFromPricingPAXRS")]
        public XmlElement CreateTSTFromPricingPAXRS(string SID, string SQN, string SCT, string GUID, string TSTNumber, string PAXNumber)
        {
            XmlElement ReqXml = CreateTSTFromPricingPAXRQ(TSTNumber, PAXNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "CreateTSTFromPricingPAXRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("CreateTSTFromPricing", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CreateTSTFromPricingPAXRS", "Y", GUID);

            return ResXml;
        }

        #endregion "CreateTSTFromPricing(승객별)"

        #region "ProcessEDoc"

        [WebMethod(Description = "Ticket_ProcessEDocRQ")]
		public XmlElement ProcessEDocRQ(string TicketNumber)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("ProcessEDocRQ"));

			XmlDoc.SelectSingleNode("Ticket_ProcessEDoc/infoGroup/docInfo/documentDetails/number").InnerText = TicketNumber;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// TST생성
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="TicketNumber">이티켓번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Ticket_ProcessEDocRS")]
		public XmlElement ProcessEDocRS(string SID, string SQN, string SCT, string GUID, string TicketNumber)
		{
			XmlElement ReqXml = ProcessEDocRQ(TicketNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "ProcessEDocRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("ProcessEDoc", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "ProcessEDocRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Ticket_ProcessEDocXml")]
		public XmlElement ProcessEDocXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("ProcessEDoc", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "ProcessEDoc"

		#region "DisplayTST"

		[WebMethod(Description = "Ticket_DisplayTSTRQ")]
		public XmlElement DisplayTSTRQ()
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("DisplayTSTRQ"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// TST 조회
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Ticket_DisplayTSTRS")]
		public XmlElement DisplayTSTRS(string SID, string SQN, string SCT, string GUID)
		{
			XmlElement ReqXml = DisplayTSTRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "DisplayTSTRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("DisplayTST", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "DisplayTSTRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Ticket_DisplayTSTXml")]
		public XmlElement DisplayTSTXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("DisplayTST", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "DisplayTST"
		
		#region "DisplayQuota"

		[WebMethod(Description = "Ticket_DisplayQuotaRQ")]
        public XmlElement DisplayQuotaRQ(string PVC)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("DisplayQuotaRQ"));

            if (String.IsNullOrWhiteSpace(PVC))
                XmlDoc.SelectSingleNode("Ticket_DisplayQuota").RemoveChild(XmlDoc.SelectSingleNode("Ticket_DisplayQuota/validatingCarrier"));
            else
                XmlDoc.SelectSingleNode("Ticket_DisplayQuota/validatingCarrier/companyIdentification/otherCompany").InnerText = PVC;

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 여행사 Capping 체크
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="PVC">Validating Carrier</param>
		/// <returns></returns>
		[WebMethod(Description = "Ticket_DisplayQuotaRS")]
        public XmlElement DisplayQuotaRS(string SID, string SQN, string SCT, string GUID, string PVC)
		{
            XmlElement ReqXml = DisplayQuotaRQ(PVC);
            cm.XmlFileSave(ReqXml, ac.Name, "DisplayQuotaRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("DisplayQuota", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "DisplayQuotaRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Ticket_DisplayQuotaXml")]
		public XmlElement DisplayQuotaXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("DisplayQuota", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "DisplayQuota"

		#region "CancelDocument"

		[WebMethod(Description = "Ticket_CancelDocumentRQ")]
        public XmlElement CancelDocumentRQ(int SNM, string CRC, string TicketNumber)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CancelDocumentRQ"));

            if (TicketNumber.StartsWith("180"))
                TicketNumber = TicketNumber.Substring(3);
            
            XmlDoc.SelectSingleNode("Ticket_CancelDocument/documentNumberDetails/documentDetails/number").InnerText = TicketNumber.Replace("-", "");
            XmlDoc.SelectSingleNode("Ticket_CancelDocument/stockProviderDetails/officeSettingsDetails/marketIataCode").InnerText = CRC;
            XmlDoc.SelectSingleNode("Ticket_CancelDocument/targetOfficeDetails/originatorDetails/inHouseIdentification2").InnerText = AmadeusConfig.OfficeId(SNM);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// E-Ticket VOID
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="SNM">사이트번호</param>
        /// <param name="CRC">국가코드</param>
        /// <param name="TicketNumber">이티켓번호</param>
		/// <returns></returns>
		[WebMethod(Description = "Ticket_CancelDocumentRS")]
        public XmlElement CancelDocumentRS(string SID, string SQN, string SCT, string GUID, int SNM, string CRC, string TicketNumber)
		{
            XmlElement ReqXml = CancelDocumentRQ(SNM, CRC, TicketNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "CancelDocumentRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("CancelDocument", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CancelDocumentRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "Ticket_CancelDocumentXml")]
		public XmlElement CancelDocumentXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("CancelDocument", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "CancelDocument"

        #region "특정 문서 조회"

        /// <summary>
        /// 특정 문서 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="DocType">문서종류(ITR, CCF, TKT(Agent Coupon), TSF(TASF), INI(이니시스 영수증))</param>
        /// <param name="DataType">데이터종류(URL(문서의 웹페이지 URL), XML(문서의 원본 XML))</param>
        /// <param name="PNR">PNR</param>
        /// <param name="TicketNumber">티켓번호</param>
        /// <param name="PAX">승객명(ex: HONG/GILDONGMR, Title 포함)</param>
		/// <param name="SDate">비행 첫 구간 출발날짜(ex: 19JAN14)</param>
		/// <returns>문서 출력용 URL</returns>
        [WebMethod(Description = "특정 문서 조회")]
        public string SearchDocData(int SNM, string DocType, string DataType, string PNR, string TicketNumber, string PAX, string SDate)
		{
            string ItrData = string.Empty;
            
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] ByteData = encoding.GetBytes(String.Format("docType={0}&dataType={1}&pnrNo={2}&ticketNumber={3}&officeId={4}&psName={5}&departureDate={6}&IBEResponse={7}", DocType, DataType, cm.ChangeAmadeusPNR(PNR), TicketNumber, AmadeusConfig.OfficeId(SNM), PAX, SDate, AmadeusConfig.ItrKey(SNM)));

			System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://pssums.topas.net/UMS/doc/api/docData");
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.ContentLength = ByteData.Length;

			System.IO.Stream reqStream = req.GetRequestStream();
			reqStream.Write(ByteData, 0, ByteData.Length);
			reqStream.Close();

			System.Net.HttpWebResponse res = (System.Net.HttpWebResponse)req.GetResponse();
			System.IO.StreamReader resStream = new System.IO.StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8);

			ItrData = resStream.ReadToEnd();
			resStream.Close();

			return ItrData;
		}

		#endregion "특정 문서 조회"

		#endregion "Ticket"

		#region "FOP"

		#region "CreateFormOfPayment"

		[WebMethod(Description = "FOP_CreateFormOfPaymentRQ")]
        public XmlElement CreateFormOfPaymentRQ(string PaxType, string PaxNumber, string ValidatingCarrier, string CardType, string CardNumber, string CardExpiryDate, string CardInstalments, string CardApprovalCode, int CardAmount, int CashAmount)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("CreateFormOfPaymentRQ"));

            XmlNode CFOP = XmlDoc.SelectSingleNode("FOP_CreateFormOfPayment");
            XmlNode FopGroup = CFOP.SelectSingleNode("fopGroup");
            XmlNode PassengerAssociation = FopGroup.SelectSingleNode("passengerAssociation");
            XmlNode MopCash = FopGroup.SelectSingleNode("mopDescription[fopSequenceNumber/sequenceDetails/number='1']");
            XmlNode MopCard = FopGroup.SelectSingleNode("mopDescription[fopSequenceNumber/sequenceDetails/number='2']");

            PassengerAssociation.SelectSingleNode("passengerReference/type").InnerText = PaxType.Equals("INF") ? "INF" : "PAX";
            PassengerAssociation.SelectSingleNode("passengerReference/value").InnerText = PaxNumber;

            if (CashAmount.Equals(0))
            {
                FopGroup.RemoveChild(MopCash);
                MopCard.SelectSingleNode("fopSequenceNumber/sequenceDetails/number").InnerText = "1";
            }
            
            if (CardAmount > 0)
            {
                MopCard.SelectSingleNode("mopDetails/fopPNRDetails/fopDetails/fopCode").InnerText = String.Concat("CC", CardType);

                MopCard.SelectSingleNode("paymentModule/paymentData/merchantInformation/companyCode").InnerText = ValidatingCarrier;
                MopCard.SelectSingleNode("paymentModule/paymentData/monetaryInformation/monetaryDetails/amount").InnerText = CardAmount.ToString();
                MopCard.SelectSingleNode("paymentModule/paymentData/extendedPaymentInfo/extendedPaymentDetails/instalmentsNumber").InnerText = cm.RequestString(CardInstalments, "0");

                MopCard.SelectSingleNode("paymentModule/mopInformation/creditCardData/creditCardDetails/ccInfo/vendorCode").InnerText = CardType;
                MopCard.SelectSingleNode("paymentModule/mopInformation/creditCardData/creditCardDetails/ccInfo/cardNumber").InnerText = CardNumber;
                MopCard.SelectSingleNode("paymentModule/mopInformation/creditCardData/creditCardDetails/ccInfo/expiryDate").InnerText = CardExpiryDate;

                MopCard.SelectSingleNode("paymentModule/mopDetailedData/creditCardDetailedData/approvalDetails/approvalCodeData/approvalCode").InnerText = CardApprovalCode;
            }
            else
                FopGroup.RemoveChild(MopCard);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 결제(FP)정보 저장
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="PaxType">탑승객타입(PAX|INF)</param>
        /// <param name="PaxNumber">탑승객번호</param>
        /// <param name="ValidatingCarrier">판매항공사</param>
        /// <param name="CardType">신용카드코드(2자리)</param>
        /// <param name="CardNumber">신용카드번호</param>
        /// <param name="CardExpiryDate">신용카드 유효기간(MMYY)</param>
        /// <param name="CardInstalments">신용카드 할부기간</param>
        /// <param name="CardApprovalCode">신용카드 승인코드</param>
        /// <param name="CardAmount">신용카드 결제금액</param>
        /// <param name="CashAmount">현금 결제금액</param>
		/// <returns></returns>
		[WebMethod(Description = "FOP_CreateFormOfPaymentRS")]
        public XmlElement CreateFormOfPaymentRS(string SID, string SQN, string SCT, string GUID, string PaxType, string PaxNumber, string ValidatingCarrier, string CardType, string CardNumber, string CardExpiryDate, string CardInstalments, string CardApprovalCode, int CardAmount, int CashAmount)
		{
            XmlElement ReqXml = CreateFormOfPaymentRQ(PaxType, PaxNumber, ValidatingCarrier, CardType, CardNumber, CardExpiryDate, CardInstalments, CardApprovalCode, CardAmount, CashAmount);
            cm.XmlFileSave(ReqXml, ac.Name, "CreateFormOfPaymentRQ", "Y", GUID);

            XmlElement ResXml = ac.Execute("CreateFormOfPayment", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "CreateFormOfPaymentRS", "Y", GUID);

            return ResXml;
		}

		[WebMethod(Description = "FOP_CreateFormOfPaymentXml")]
		public XmlElement CreateFormOfPaymentXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("CreateFormOfPayment", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "CreateFormOfPayment"

		#endregion "FOP"

		#region "DocIssuance"

		#region "IssueTicket"

		[WebMethod(Description = "DocIssuance_IssueTicketRQ")]
        public XmlElement IssueTicketRQ(string ValidatingCarrier)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("IssueTicketRQ"));

            //XmlDoc.SelectSingleNode("DocIssuance_IssueTicket").RemoveChild(XmlDoc.SelectSingleNode("DocIssuance_IssueTicket/optionGroup"));
            //XmlDoc.SelectSingleNode("DocIssuance_IssueTicket/otherCompoundOptions/attributeDetails/attributeDescription").InnerText = ValidatingCarrier;

            XmlDoc.SelectSingleNode("DocIssuance_IssueTicket").RemoveChild(XmlDoc.SelectSingleNode("DocIssuance_IssueTicket/otherCompoundOptions"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 결제(FP)정보 저장
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
        /// <param name="ValidatingCarrier">판매항공사</param>
		/// <returns></returns>
		[WebMethod(Description = "DocIssuance_IssueTicketRS")]
        public XmlElement IssueTicketRS(string SID, string SQN, string SCT, string GUID, string ValidatingCarrier)
		{
            XmlElement ReqXml = IssueTicketRQ(ValidatingCarrier);
            cm.XmlFileSave(ReqXml, ac.Name, "IssueTicketRQ", "Y", GUID);

			XmlElement ResXml = ac.Execute("IssueTicket", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "IssueTicketRS", "Y", GUID);

			return ResXml;
		}

		[WebMethod(Description = "DocIssuance_IssueTicketXml")]
		public XmlElement IssueTicketXml(string ReqXml, string SID, string SQN, string SCT)
		{
			try
			{
				XmlDocument XmlReq = new XmlDocument();
				XmlReq.LoadXml(ReqXml);

				return ac.Execute("IssueTicket", XmlReq.DocumentElement, SID, SQN, SCT);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "IssueTicket"

		#endregion "DocIssuance"

		#region "Queue"

        #region "CountTotal"

        [WebMethod(Description = "Queue_CountTotalRQ")]
        public XmlElement QueueCountTotalRQ(string OFID, int? QueueNumber, int? CategoryNumber)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueueCountTotalRQ"));

            XmlDoc.SelectSingleNode("Queue_CountTotal/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFID;

            if (QueueNumber.HasValue && CategoryNumber.HasValue)
            {
                XmlDoc.SelectSingleNode("Queue_CountTotal/queueingOptions/selectionDetails/option").InnerText = "QC";
                XmlDoc.SelectSingleNode("Queue_CountTotal/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();
                XmlDoc.SelectSingleNode("Queue_CountTotal/categorySelection/subQueueInfoDetails[identificationType='C']/itemNumber").InnerText = CategoryNumber.ToString();
            }
            else if (QueueNumber.HasValue)
            {
                XmlDoc.SelectSingleNode("Queue_CountTotal/queueingOptions/selectionDetails/option").InnerText = "QC";
                XmlDoc.SelectSingleNode("Queue_CountTotal/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();

                XmlDoc.SelectSingleNode("Queue_CountTotal/categorySelection/subQueueInfoDetails/identificationType").InnerText = "E";
                XmlDoc.SelectSingleNode("Queue_CountTotal/categorySelection/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_CountTotal/categorySelection/subQueueInfoDetails/itemNumber"));
            }
            else
            {
                XmlDoc.SelectSingleNode("Queue_CountTotal/queueingOptions/selectionDetails/option").InnerText = "QT";

                XmlDoc.SelectSingleNode("Queue_CountTotal").RemoveChild(XmlDoc.SelectSingleNode("Queue_CountTotal/queueNumber"));
                XmlDoc.SelectSingleNode("Queue_CountTotal").RemoveChild(XmlDoc.SelectSingleNode("Queue_CountTotal/categorySelection"));
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 큐방 또는 카테고리별 PNR 카운트
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_CountTotalRS")]
        public XmlElement QueueCountTotalRS(string SID, string SQN, string SCT, string GUID, string OFID, int? QueueNumber, int? CategoryNumber)
        {
            XmlElement ReqXml = QueueCountTotalRQ(OFID, QueueNumber, CategoryNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "QueueCountTotalRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueueCountTotal", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueueCountTotalRS", "N", GUID);

            return ResXml;
        }

        #endregion "CountTotal"

        #region "QueueList"

        [WebMethod(Description = "Queue_ListRQ")]
        public XmlElement QueueListRQ(string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, int RangeSNumber, int RangeENumber)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueueListRQ"));

            XmlDoc.SelectSingleNode("Queue_List/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFID;
            XmlDoc.SelectSingleNode("Queue_List/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_List/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryType;
            XmlDoc.SelectSingleNode("Queue_List/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumber.ToString();

            //if (CategoryType.Equals("CN"))
            if (!String.IsNullOrWhiteSpace(CategoryName))
                XmlDoc.SelectSingleNode("Queue_List/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryName;
            else
                XmlDoc.SelectSingleNode("Queue_List/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_List/categoryDetails/subQueueInfoDetails/itemDescription"));

            if (CategoryType.Equals("1") || CategoryType.Equals("2") || CategoryType.Equals("3") || CategoryType.Equals("4"))
                XmlDoc.SelectSingleNode("Queue_List/date/timeMode").InnerText = CategoryType;
            else
                XmlDoc.SelectSingleNode("Queue_List").RemoveChild(XmlDoc.SelectSingleNode("Queue_List/date"));

            XmlDoc.SelectSingleNode("Queue_List/scanRange/rangeDetails/min").InnerText = RangeSNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_List/scanRange/rangeDetails/max").InnerText = RangeENumber.ToString();

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 리스트를 조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, CN:닉네임, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <param name="RangeSNumber">조회범위 시작번호</param>
        /// <param name="RangeENumber">조회범위 마침번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_ListRS")]
        public XmlElement QueueListRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, int RangeSNumber, int RangeENumber)
        {
            XmlElement ReqXml = QueueListRQ(OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, RangeSNumber, RangeENumber);
            cm.XmlFileSave(ReqXml, ac.Name, "QueueListRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueueList", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueueListRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueueList"

        #region "QueuePlacePNR"

        [WebMethod(Description = "Queue_PlacePNRRQ")]
        public XmlElement QueuePlacePNRRQ(string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueuePlacePNRRQ"));

            XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFID;
            XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryType;
            XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_PlacePNR/recordLocator/reservation/controlNumber").InnerText = PNR;

            if (!String.IsNullOrWhiteSpace(CategoryName))
                XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryName;
            else
                XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_PlacePNR/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 리스트를 조회
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, CN:닉네임, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_PlacePNRRS")]
        public XmlElement QueuePlacePNRRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            XmlElement ReqXml = QueuePlacePNRRQ(OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, PNR);
            cm.XmlFileSave(ReqXml, ac.Name, "QueuePlacePNRRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueuePlacePNR", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueuePlacePNRRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueueList"

        #region "QueueMoveItem"

        [WebMethod(Description = "Queue_MoveItemRQ")]
        public XmlElement QueueMoveItemRQ(string OFIDFrom, int QueueNumberFrom, string CategoryTypeFrom, int CategoryNumberFrom, string CategoryNameFrom, string OFIDTo, int QueueNumberTo, string CategoryTypeTo, int CategoryNumberTo, string CategoryNameTo)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueueMoveItemRQ"));

            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFIDFrom;
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/queueNumber/queueDetails/number").InnerText = QueueNumberFrom.ToString();
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryTypeFrom;
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumberFrom.ToString();

            if (!String.IsNullOrWhiteSpace(CategoryNameFrom))
                XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryNameFrom;
            else
                XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[1]/categoryDetails/subQueueInfoDetails/itemDescription"));

            //XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/targetOffice/sourceType/sourceQualifier1").InnerText = (OFIDFrom.Equals(OFIDTo)) ? "3" : "4";
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFIDTo;
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/queueNumber/queueDetails/number").InnerText = QueueNumberTo.ToString();
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryTypeTo;
            XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumberTo.ToString();

            if (!String.IsNullOrWhiteSpace(CategoryNameTo))
                XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryNameTo;
            else
                XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_MoveItem/targetDetails[2]/categoryDetails/subQueueInfoDetails/itemDescription"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 리스트를 복사
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFIDFrom">OfficeId(복사될 원본)</param>
        /// <param name="QueueNumberFrom">큐방번호(복사될 원본)</param>
        /// <param name="CategoryTypeFrom">카테고리구분(C:카테고리, 1~4:Date range)(복사될 원본)</param>
        /// <param name="CategoryNumberFrom">카테고리번호(복사될 원본)</param>
        /// <param name="CategoryNameFrom">카테고리명(복사될 원본)</param>
        /// <param name="OFIDTo">OfficeId(복사된 정보)</param>
        /// <param name="QueueNumberTo">큐방번호(복사된 정보)</param>
        /// <param name="CategoryTypeTo">카테고리구분(C:카테고리, 1~4:Date range)(복사된 정보)</param>
        /// <param name="CategoryNumberTo">카테고리번호(복사된 정보)</param>
        /// <param name="CategoryNameTo">카테고리명(복사된 정보)</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_MoveItemRS")]
        public XmlElement QueueMoveItemRS(string SID, string SQN, string SCT, string GUID, string OFIDFrom, int QueueNumberFrom, string CategoryTypeFrom, int CategoryNumberFrom, string CategoryNameFrom, string OFIDTo, int QueueNumberTo, string CategoryTypeTo, int CategoryNumberTo, string CategoryNameTo)
        {
            XmlElement ReqXml = QueueMoveItemRQ(OFIDFrom, QueueNumberFrom, CategoryTypeFrom, CategoryNumberFrom, CategoryNameFrom, OFIDTo, QueueNumberTo, CategoryTypeTo, CategoryNumberTo, CategoryNameTo);
            cm.XmlFileSave(ReqXml, ac.Name, "QueueMoveItemRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueueMoveItem", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueueMoveItemRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueueMoveItem"

        #region "QueueRemoveItem"

        [WebMethod(Description = "Queue_RemoveItemRQ")]
        public XmlElement QueueRemoveItemRQ(string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueueRemoveItemRQ"));

            XmlDoc.SelectSingleNode("Queue_RemoveItem/removalOption/selectionDetails/option").InnerText = "QRP";

            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFID;
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryType;
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumber.ToString();

            if (CategoryType.Equals("2") || CategoryType.Equals("3") || CategoryType.Equals("4"))
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/placementDate/timeMode").InnerText = CategoryType;
            else
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/placementDate"));

            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/recordLocator/reservation/controlNumber").InnerText = PNR;

            if (!String.IsNullOrWhiteSpace(CategoryName))
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryName;
            else
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 삭제
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_RemoveItemRS")]
        public XmlElement QueueRemoveItemRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName, string PNR)
        {
            XmlElement ReqXml = QueueRemoveItemRQ(OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName, PNR);
            cm.XmlFileSave(ReqXml, ac.Name, "QueueRemoveItemRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueueRemoveItem", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueueRemoveItemRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueueRemoveItem"

        #region "QueueRemoveCategory"

        [WebMethod(Description = "Queue_RemoveCategoryRQ")]
        public XmlElement QueueRemoveCategoryRQ(string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("QueueRemoveItemRQ"));

            XmlDoc.SelectSingleNode("Queue_RemoveItem/removalOption/selectionDetails/option").InnerText = "QR";

            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/targetOffice/originatorDetails/inHouseIdentification1").InnerText = OFID;
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/queueNumber/queueDetails/number").InnerText = QueueNumber.ToString();
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/identificationType").InnerText = "C";// CategoryType;
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemNumber").InnerText = CategoryNumber.ToString();

            if (CategoryType.Equals("2") || CategoryType.Equals("3") || CategoryType.Equals("4"))
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/placementDate/timeMode").InnerText = CategoryType;
            else
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/placementDate"));

            //XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/targetOffice").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/targetOffice/originatorDetails"));
            XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/recordLocator"));

            if (!String.IsNullOrWhiteSpace(CategoryName))
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription").InnerText = CategoryName;
            else
                XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails").RemoveChild(XmlDoc.SelectSingleNode("Queue_RemoveItem/targetDetails/categoryDetails/subQueueInfoDetails/itemDescription"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 특정 큐방/카테고리 내의 PNR 삭제
        /// </summary>
        /// <param name="SID">SessionId</param>
        /// <param name="SQN">SequenceNumber</param>
        /// <param name="SCT">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="OFID">OfficeId</param>
        /// <param name="QueueNumber">큐방번호</param>
        /// <param name="CategoryType">카테고리구분(C:카테고리, 1~4:Date range)</param>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="CategoryName">카테고리명</param>
        /// <returns></returns>
        [WebMethod(Description = "Queue_RemoveCategoryRS")]
        public XmlElement QueueRemoveCategoryRS(string SID, string SQN, string SCT, string GUID, string OFID, int QueueNumber, string CategoryType, int CategoryNumber, string CategoryName)
        {
            XmlElement ReqXml = QueueRemoveCategoryRQ(OFID, QueueNumber, CategoryType, CategoryNumber, CategoryName);
            cm.XmlFileSave(ReqXml, ac.Name, "QueueRemoveCategoryRQ", "N", GUID);

            XmlElement ResXml = ac.Execute("QueueRemoveItem", ReqXml, SID, SQN, SCT);
            cm.XmlFileSave(ResXml, ac.Name, "QueueRemoveCategoryRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueueRemoveCategory"

        #endregion "Queue"

        #region "Command_Cryptic"

        [WebMethod(Description = "Command_CrypticRQ")]
		public XmlElement CommandCrypticRQ(string Entry)
		{
			try
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(ac.XmlFullPath("CommandCrypticRQ"));

				XmlDoc.SelectSingleNode("Command_Cryptic/longTextString/textStringDetails").InnerText = Entry;

				return XmlDoc.DocumentElement;
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		/// <summary>
		/// Command_Cryptic
		/// </summary>
		/// <param name="SID">SessionId</param>
		/// <param name="SQN">SequenceNumber</param>
		/// <param name="SCT">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="Entry">명령어</param>
		/// <returns></returns>
		[WebMethod(Description = "Command_CrypticRS")]
		public XmlElement CommandCrypticRS(string SID, string SQN, string SCT, string GUID, string Entry)
		{
			try
			{
				XmlElement ReqXml = CommandCrypticRQ(Entry);
                cm.XmlFileSave(ReqXml, ac.Name, "CommandCrypticRQ", "Y", GUID);

				XmlElement ResXml = ac.Execute("CommandCryptic", ReqXml, SID, SQN, SCT);
                cm.XmlFileSave(ResXml, ac.Name, "CommandCrypticRS", "Y", GUID);

				return ResXml;
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		/// <summary>
		/// Command 서비스
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="Entry">명령어</param>
		/// <returns></returns>
		//[WebMethod(Description = "Command 서비스")]
		public XmlElement CommandRS(int SNM, string Entry)
		{
			string GUID = cm.GetGUID;
			string SID = String.Empty;
			string SCT = String.Empty;
			int SQN = 0;

			try
			{
				//### 01.세션생성 #####
				XmlElement Session = Authenticate(SNM, String.Concat(GUID, "-01"));

				SID = Session.SelectSingleNode("session/sessionId").InnerText;
				SCT = Session.SelectSingleNode("session/securityToken").InnerText;
				SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

				//### 02.CommandCryptic #####
				XmlElement ResXml = CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), Entry);

				//### 03.세션종료 #####
				SQN = SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"));

				return ResXml;
			}
			catch (Exception ex)
			{
				//### 세션종료 #####
				if (SQN > 0)
					SignOut(SID, (++SQN).ToString(), SCT, GUID);

                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "Command_Cryptic"

		#region "Database"

        #region "운임규정 조회"

        /// <summary>
		/// SQL을 이용한 Oracle DB결과 리턴
		/// </summary>
		/// <param name="SQL">오라클 쿼리</param>
		/// <returns></returns>
		private XmlElement SearchRuleDB(string SQL)
		{
			using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["IFARE"].ConnectionString))
			{
				try
				{
					conn.Open();

					using (OracleCommand cmd = new OracleCommand())
					{
						OracleDataAdapter adp = new OracleDataAdapter(cmd);
						DataSet ds = new DataSet("rules");

						cmd.Connection = conn;
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = SQL;

						adp.Fill(ds, "rule");
						adp.Dispose();

						XmlDocument XmlDoc = new XmlDocument();
						XmlDoc.LoadXml(ds.GetXml());

						ds.Dispose();
						ds.Clear();

						return XmlDoc.DocumentElement;
					}
				}
				catch (OracleException ex)
				{
                    throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
				}
				catch (Exception ex)
				{
                    throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
				}
			}
		}

		[WebMethod(Description = "운임규정 조회")]
		public XmlElement SearchRule1RS(int SNM, string FareNo, string FareRuleFlag)
		{
			return SearchRuleDB(String.Concat("SELECT RULE_NO, RULE_ITEM_NM, REPLACE(RULE_CONTENT, '?', ' ') AS RULE_CONTENT, TO_NUMBER(RULE_NO) AS SORTING ",
											 "FROM TB_TAI_FB140 ",
											 "WHERE RSV_AGT_CD = 'SELK138AB' ",
											 "AND FARE_NO = '", FareNo, "' ",
											 "AND FARE_RULE_FLAG = '", cm.RequestString(FareRuleFlag, "H"), "' ",
											 "ORDER BY SORTING ASC"));
		}

		[WebMethod(Description = "운임규정 조회(할인항공용)")]
		public XmlElement SearchRule2RS(int SNM, string FareNo, string FareRuleFlag)
		{
			return SearchRuleDB(String.Concat("SELECT RULE_NO, RULE_ITEM_NM, REPLACE(RULE_CONTENT, '?', ' ') AS RULE_CONTENT, TO_NUMBER(RULE_NO) AS SORTING ",
											 "FROM TB_TAI_FB140 ",
											 "WHERE RSV_AGT_CD = 'SELK138AB' ",
											 "AND FARE_NO = '", FareNo, "' ",
											 "AND FARE_RULE_FLAG = '", cm.RequestString(FareRuleFlag, "H"), "' ",
											 "ORDER BY SORTING ASC"));
        }

        #endregion "운임규정 조회"

        #region "자동발권수행여부"

        /// <summary>
        /// 자동발권 수행 여부 조회
        /// </summary>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="ArrNationCode">도착지 국가코드</param>
        /// <param name="ArrCityCode">도착지 도시코드</param>
        /// <param name="ArrAirportCode">도착지 공항코드</param>
        /// <param name="ViaNationCode">경유지 국가코드</param>
        /// <param name="ViaCityCode">경유지 도시코드</param>
        /// <param name="ViaAirportCode">경유지 공항코드</param>
        /// <returns></returns>
        public string SearchTicketableDB(string AirCode, string ArrNationCode, string ArrCityCode, string ArrAirportCode, string ViaNationCode, string ViaCityCode, string ViaAirportCode)
        {
            string UseYN = "N";
            string SQL = string.Empty;
            
            //7C 자동발권 가능 처리(2019-01-31,김경미매니저)
            if (AirCode.Equals("7C"))
                UseYN = "Y";
            else
            {
                if (String.IsNullOrWhiteSpace(ViaAirportCode))
                {
                    SQL = String.Concat("SELECT USE_YN, (VIA_WGT + ARR_WGT) AS USE_WGT, REG_DTM, UPD_DTM ",
                                        "FROM (SELECT ENTRY_SEQNO, AIR_CD, FARE_TYPE, USE_YN, ",
                                        "             DECODE(VIA_KIND_FLAG, '$', 1, 'A', 2, 'C', 3, 'P',  4, 0) AS VIA_WGT, ",
                                        "             DECODE(ARR_KIND_FLAG, '$', 1, 'A', 5, 'C', 9, 'P', 13, 0) AS ARR_WGT, ",
                                        "             VIA_KIND_FLAG, VIA_CD, ARR_KIND_FLAG, ARR_CD, XTR_COL, REG_USR_ID, UPD_USR_ID, REG_DTM, UPD_DTM ",
                                        "      FROM TB_AIR_CD100 ",
                                        "      WHERE AIR_CD = '", AirCode, "' ",
                                        "            AND (ARR_CD = '$$' ",
                                        "                 OR ARR_CD LIKE '%", ArrNationCode, "%' ",
                                        "                 OR ARR_CD LIKE '%", ArrCityCode, "%' ",
                                        "                 OR ARR_CD LIKE '%", ArrAirportCode, "%')) ",
                                        "ORDER BY USE_WGT DESC, UPD_DTM DESC, REG_DTM DESC");
                }
                else
                {
                    SQL = String.Concat("SELECT USE_YN, (VIA_WGT + ARR_WGT) AS USE_WGT, REG_DTM, UPD_DTM ",
                                        "FROM (SELECT ENTRY_SEQNO, AIR_CD, FARE_TYPE, USE_YN, ",
                                        "             DECODE(VIA_KIND_FLAG, '$', 1, 'A', 2, 'C', 3, 'P',  4, 0) AS VIA_WGT, ",
                                        "             DECODE(ARR_KIND_FLAG, '$', 1, 'A', 5, 'C', 9, 'P', 13, 0) AS ARR_WGT, ",
                                        "             VIA_KIND_FLAG, VIA_CD, ARR_KIND_FLAG, ARR_CD, XTR_COL, REG_USR_ID, UPD_USR_ID, REG_DTM, UPD_DTM ",
                                        "      FROM TB_AIR_CD100 ",
                                        "      WHERE AIR_CD = '", AirCode, "' ",
                                        "            AND (VIA_CD = '$$' ",
                                        "                 OR VIA_CD LIKE '%", ViaNationCode, "%' ",
                                        "                 OR VIA_CD LIKE '%", ViaCityCode, "%' ",
                                        "                 OR VIA_CD LIKE '%", ViaAirportCode, "%')) ",
                                        "ORDER BY USE_WGT DESC, UPD_DTM DESC, REG_DTM DESC");
                }

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["IFARE"].ConnectionString))
                {
                    try
                    {
                        conn.Open();

                        using (OracleCommand cmd = new OracleCommand())
                        {
                            OracleDataAdapter adp = new OracleDataAdapter(cmd);
                            DataSet ds = new DataSet("ticketable");

                            cmd.Connection = conn;
                            cmd.CommandTimeout = 60;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = SQL;

                            adp.Fill(ds, "item");
                            adp.Dispose();

                            if (ds.Tables[0].Rows.Count > 0)
                                UseYN = ds.Tables[0].Rows[0]["USE_YN"].ToString();

                            ds.Dispose();
                            ds.Clear();
                        }
                    }
                    catch (OracleException ex)
                    {
                        throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return UseYN;
        }

        #endregion "자동발권수행여부"

        #region "자동발권가능항공사리스트"

        /// <summary>
        /// 자동발권 가능 항공사 리스트
        /// </summary>
        /// <returns></returns>
        public string SearchTicketableAirListDB()
        {
            //DL 항공사 제외(2016-12-29,김지영과장)
            string AirList = String.Empty;
            
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["IFARE"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        OracleDataAdapter adp = new OracleDataAdapter(cmd);
                        DataSet ds = new DataSet("ticketable");

                        cmd.Connection = conn;
                        cmd.CommandTimeout = 60;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT WM_CONCAT(AIR_CD) AIRCODE FROM (SELECT AIR_CD FROM TB_AIR_CD100 ORDER BY AIR_CD ASC) WHERE AIR_CD != 'DL'";

                        adp.Fill(ds, "item");
                        adp.Dispose();

                        if (ds.Tables[0].Rows.Count > 0)
                            AirList = ds.Tables[0].Rows[0][0].ToString();

                        ds.Dispose();
                        ds.Clear();
                    }
                }
                catch (OracleException ex)
                {
                    throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
                finally
                {
                    conn.Close();
                }
            }

            return AirList;
        }

        #endregion "자동발권가능항공사리스트"

        #endregion "Database"
    }
}