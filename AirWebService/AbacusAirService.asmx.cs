using System;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// Abacus 실시간 항공 예약을 위한 각종 정보 제공
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	public class AbacusAirService : System.Web.Services.WebService
	{
		Common cm;
		AbacusConfig ac;
		HttpContext hcc;

		public AbacusAirService()
		{
			cm = new Common();
			ac = new AbacusConfig();
			hcc = HttpContext.Current;
		}

		#region "Security"

		[WebMethod(Description = "세션 생성")]
		public XmlElement SessionCreate()
		{
			try
			{
				return ac.SessionCreate();
			}
			catch (Exception)
			{
                //세션 생성이 실패하는 경우 발생하여 한 번 더 호출 후 오류 처리
                try
                {
                    return ac.SessionCreate();
                }
                catch (Exception ex)
                {

                    throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
			}
		}

		[WebMethod(Description = "세션 삭제")]
		public XmlElement SessionClose(string ConversionID, string SecurityToken)
		{
			try
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(ac.XmlFullPath("SessionClose"));

				return ac.Execute(XmlDoc.DocumentElement, "SessionCloseRQ", ConversionID, SecurityToken);
			}
			catch (Exception ex)
			{
				throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		#endregion "Security"

		#region "OTA_AirAvailLLS"

		[WebMethod(Description = "OTA_AirAvailLLS")]
		public XmlElement AirAvailLLSRQ(string SAC, string DLC, string ALC, string DTD, string DAC, string CLC, int MSQ)
		{
			try
			{
				XmlDocument XmlAvail = new XmlDocument();
				XmlAvail.Load(ac.XmlFullPath("OTA_AirAvailLLS"));

				XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlAvail.NameTable);
				nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

				XmlNode Node = XmlAvail.SelectSingleNode("m:OTA_AirAvailRQ", nsMgr);
				XmlNode OriginDestinationInformation = Node.SelectSingleNode("m:OriginDestinationInformation", nsMgr);
				XmlNode ConnectionLocations = OriginDestinationInformation.SelectSingleNode("m:ConnectionLocations", nsMgr);
				XmlNode ConnectionLocation = ConnectionLocations.SelectSingleNode("m:ConnectionLocation", nsMgr);
				XmlNode TravelPreferences = Node.SelectSingleNode("m:TravelPreferences", nsMgr);
				XmlNode VendorPref = TravelPreferences.SelectSingleNode("m:VendorPref", nsMgr);
				XmlNode TPA_Extensions = TravelPreferences.SelectSingleNode("m:TPA_Extensions", nsMgr);
				XmlNode NewConnectionLocation;
				XmlNode NewVendorPref;

				int i = 0;

				//여정
				OriginDestinationInformation.SelectSingleNode("m:DepartureDateTime", nsMgr).Attributes.GetNamedItem("DateTime").InnerText = cm.RequestDateTime(DTD);
				OriginDestinationInformation.SelectSingleNode("m:OriginLocation", nsMgr).Attributes.GetNamedItem("LocationCode").InnerText = DLC;
				OriginDestinationInformation.SelectSingleNode("m:DestinationLocation", nsMgr).Attributes.GetNamedItem("LocationCode").InnerText = ALC;

				//경유지
				if (String.IsNullOrWhiteSpace(CLC))
					OriginDestinationInformation.RemoveChild(ConnectionLocations);
				else
				{
					string[] CLCs = CLC.Split(',');

					for (i = 0; i < CLCs.Length; i++)
					{
						if (!String.IsNullOrWhiteSpace(CLCs[i]))
						{
							NewConnectionLocation = ConnectionLocations.AppendChild(ConnectionLocation.CloneNode(true));
							NewConnectionLocation.Attributes.GetNamedItem("LocationCode").InnerText = CLCs[i];
						}
					}

					ConnectionLocations.RemoveChild(ConnectionLocation);

					if (ConnectionLocations.ChildNodes.Count == 0 || ConnectionLocations.ChildNodes[0].Attributes.GetNamedItem("LocationCode").InnerText == "")
						OriginDestinationInformation.RemoveChild(ConnectionLocations);
				}

				//특정 항공사 지정 조회
				if (!String.IsNullOrWhiteSpace(SAC))
				{
					string[] SACs = SAC.Split(',');

					for (i = 0; i < SACs.Length; i++)
					{
						if (!String.IsNullOrWhiteSpace(SACs[i]))
						{
							NewVendorPref = TravelPreferences.InsertBefore(VendorPref.CloneNode(true), TPA_Extensions);
							NewVendorPref.Attributes.GetNamedItem("Code").InnerText = SACs[i];
						}
					}

					TravelPreferences.RemoveChild(VendorPref);

					//하나 이상의 항공사를 조회할 경우, 경유지를 지정하는 경우 중립모드로 조회(항공사모드 조회 불가)
					if (i > 1 || !String.IsNullOrWhiteSpace(CLC))
						TPA_Extensions.SelectSingleNode("m:DirectAccess", nsMgr).Attributes.GetNamedItem("Ind").InnerText = "false";
					else
					{
						if (String.IsNullOrWhiteSpace(DAC))
							TPA_Extensions.SelectSingleNode("m:DirectAccess", nsMgr).Attributes.GetNamedItem("Ind").InnerText = "true";
						else
							TPA_Extensions.SelectSingleNode("m:DirectAccess", nsMgr).Attributes.GetNamedItem("Ind").InnerText = DAC.Equals("Y") ? "true" : "false";
					}
				}

				//경유횟수
				TravelPreferences.Attributes.GetNamedItem("MaxStopsQuantity").InnerText = MSQ.ToString();

				return XmlAvail.DocumentElement;
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		/// <summary>
		/// Availability 조회
		/// </summary>
		/// <param name="CID">ConversionID</param>
		/// <param name="STK">SecurityToken</param>
		/// <param name="SAC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="DAC">항공사 모드 여부(Y/N)</param>
		/// <param name="CLC">경유지 공항 코드(콤마로 구분)</param>
		/// <param name="MSQ">최대 경유 수</param>
		/// <returns></returns>
		[WebMethod(Description = "OTA_AirAvailLLS")]
		public XmlElement AirAvailLLSRS(string CID, string STK, string SAC, string DLC, string ALC, string DTD, string DAC, string CLC, int MSQ)
		{
			try
			{
				return ac.Execute(AirAvailLLSRQ(SAC, DLC, ALC, DTD, DAC, CLC, MSQ), "OTA_AirAvailRQ", CID, STK);
			}
			catch (Exception ex)
			{
                throw new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
			}
		}

		[WebMethod(Description = "OTA_AirAvailLLS")]
		public XmlElement OTA_AirAvailLLSXml(string CID, string STK, string ReqXml)
		{
			try
			{
				return ac.Execute(ReqXml, "OTA_AirAvailRQ", CID, STK);
			}
			catch (Exception ex)
			{
                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "OTA_AirAvailLLS"

		#region "OTA_AirBookLLS"

		[WebMethod(Description = "OTA_AirBookLLS")]
		public XmlElement AbacusAirBookRQ(XmlElement SXL)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("OTA_AirBookLLS"));

			XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
			nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

			XmlNode OriginDestinationOption = XmlDoc.SelectSingleNode("m:OTA_AirBookRQ/m:AirItinerary/m:OriginDestinationOptions/m:OriginDestinationOption", nsMgr);
			XmlNode FlightSegment = OriginDestinationOption.SelectSingleNode("m:FlightSegment", nsMgr);
			XmlNode NewFlightSegment;

			foreach (XmlNode SegGroup in SXL.SelectNodes("segGroup"))
			{
				foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
				{
					NewFlightSegment = OriginDestinationOption.AppendChild(FlightSegment.CloneNode(true));

					NewFlightSegment.Attributes.GetNamedItem("DepartureDateTime").InnerText = cm.ConvertToAbacusDateTime(Seg.Attributes.GetNamedItem("ddt").InnerText);
					NewFlightSegment.Attributes.GetNamedItem("ArrivalDateTime").InnerText = cm.ConvertToAbacusDateTime(Seg.Attributes.GetNamedItem("ardt").InnerText);
					NewFlightSegment.Attributes.GetNamedItem("FlightNumber").InnerText = (String.IsNullOrWhiteSpace(Seg.Attributes.GetNamedItem("fln").InnerText)) ? "OPEN" : Seg.Attributes.GetNamedItem("fln").InnerText;
					NewFlightSegment.Attributes.GetNamedItem("ResBookDesigCode").InnerText = Seg.Attributes.GetNamedItem("rbd").InnerText;
					NewFlightSegment.Attributes.GetNamedItem("ActionCode").InnerText = (String.IsNullOrWhiteSpace(Seg.Attributes.GetNamedItem("fln").InnerText)) ? "DS" : "NN";
					NewFlightSegment.Attributes.GetNamedItem("NumberInParty").InnerText = Seg.Attributes.GetNamedItem("nos").InnerText;

					NewFlightSegment.SelectSingleNode("m:DepartureAirport", nsMgr).Attributes.GetNamedItem("LocationCode").InnerText = Seg.Attributes.GetNamedItem("dlc").InnerText;
					NewFlightSegment.SelectSingleNode("m:ArrivalAirport", nsMgr).Attributes.GetNamedItem("LocationCode").InnerText = Seg.Attributes.GetNamedItem("alc").InnerText;
					NewFlightSegment.SelectSingleNode("m:OperatingAirline", nsMgr).Attributes.GetNamedItem("Code").InnerText = Seg.Attributes.GetNamedItem("occ").InnerText;
					NewFlightSegment.SelectSingleNode("m:MarketingAirline", nsMgr).Attributes.GetNamedItem("Code").InnerText = Seg.Attributes.GetNamedItem("mcc").InnerText;

					NewFlightSegment.RemoveChild(NewFlightSegment.SelectSingleNode("m:MarriageGrp", nsMgr));
				}
			}

			OriginDestinationOption.RemoveChild(FlightSegment);

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 항공편 예약
		/// </summary>
		/// <param name="CID">ConversionID</param>
		/// <param name="STK">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <param name="SXL">여정 XML Element</param>
		/// <returns></returns>
		[WebMethod(Description = "OTA_AirBookLLS")]
        public XmlElement AbacusAirBookRS(string CID, string STK, string GUID, XmlElement SXL)
		{
			XmlElement ReqXml = AbacusAirBookRQ(SXL);
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusAirBookRQ", "N", GUID);

			XmlElement ResXml = AbacusAirBookXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusAirBookRS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "OTA_AirBookLLS")]
		public XmlElement AbacusAirBookXml(string CID, string STK, XmlElement ReqXml)
		{
			return ac.Execute(ReqXml.OuterXml, "OTA_AirBookRQ", CID, STK);
		}

		#endregion "OTA_AirBookLLS"
        
        #region "TravelItineraryAddInfoLLS"

        [WebMethod(Description = "TravelItineraryAddInfoLLS")]
        public XmlElement AbacusTravelItineraryAddInfoRQ(string[] PTC, string[] PTL, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("TravelItineraryAddInfoLLS"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

            XmlNode CustomerInfo = XmlDoc.SelectSingleNode("m:TravelItineraryAddInfoRQ/m:CustomerInfo", nsMgr);
            XmlNode PersonName = CustomerInfo.SelectSingleNode("m:PersonName", nsMgr);
            XmlNode Telephone = CustomerInfo.SelectSingleNode("m:Telephone", nsMgr);
            XmlNode Email = CustomerInfo.SelectSingleNode("m:Email", nsMgr);
            XmlNode PassengerType = CustomerInfo.SelectSingleNode("m:PassengerType", nsMgr);
            XmlNode NewPersonName;
            XmlNode NewTelephone;
            XmlNode NewEmail;

            for (int i = 0; i < PFN.Length; i++)
            {
                string NameNumber = ((i + 1).ToString() + ".1");

                NewPersonName = CustomerInfo.InsertBefore(PersonName.CloneNode(true), PersonName);
                NewPersonName.Attributes.GetNamedItem("TravelerRefNumber").InnerText = NameNumber;
                NewPersonName.SelectSingleNode("m:GivenName", nsMgr).InnerText = String.Format("{0} {1}", PFN[i].Replace(" ", "").ToUpper(), PTL[i].Trim());
                NewPersonName.SelectSingleNode("m:Surname", nsMgr).InnerText = PSN[i].Replace(" ", "").ToUpper();

                if (PTC[i].Equals("CHD"))
                {
                    NewPersonName.SelectSingleNode("m:NameReference", nsMgr).Attributes.GetNamedItem("Text").InnerText = String.Concat("C", cm.NumPosition(cm.KoreanAge(PBD[i], "").ToString(), 2));
                    NewPersonName.RemoveChild(NewPersonName.SelectSingleNode("m:Infant", nsMgr));
                }
                else if (PTC[i].Equals("INF"))
                    NewPersonName.SelectSingleNode("m:NameReference", nsMgr).Attributes.GetNamedItem("Text").InnerText = String.Concat("I", cm.NumPosition(cm.MonthAge(PBD[i], "").ToString(), 2));
                else
                    NewPersonName.RemoveChild(NewPersonName.SelectSingleNode("m:Infant", nsMgr));

                //연락처
                if (!String.IsNullOrWhiteSpace(PMN[i]) || !String.IsNullOrWhiteSpace(PTN[i]))
                {
                    NewTelephone = CustomerInfo.InsertBefore(Telephone.CloneNode(false), Telephone);
                    NewTelephone.Attributes.GetNamedItem("PhoneUseType").InnerText = String.IsNullOrWhiteSpace(PMN[i]) ? "H" : "M";
                    NewTelephone.Attributes.GetNamedItem("PhoneNumber").InnerText = String.IsNullOrWhiteSpace(PMN[i]) ? (String.IsNullOrWhiteSpace(PTN[i]) ? "" : PTN[i]) : PMN[i];
                    NewTelephone.Attributes.GetNamedItem("CountryAccessCode").InnerText = "82";
                    NewTelephone.Attributes.GetNamedItem("AreaCityCode").InnerText = "";
                    NewTelephone.Attributes.GetNamedItem("TravelerRefNumber").InnerText = NameNumber;
                }

                //이메일(2016-07-29,탑승객별 이메일 등록시 2번째 탑승객부터 오류 발생하여 첫번째 탑승객에 모든 이메일 등록)
                if (!String.IsNullOrWhiteSpace(PEA[i]))
                {
                    NewEmail = CustomerInfo.InsertBefore(Email.CloneNode(true), Email);
                    NewEmail.Attributes.GetNamedItem("EmailType").InnerText = "";
                    NewEmail.Attributes.GetNamedItem("EmailAddress").InnerText = PEA[i].ToUpper();
                    //NewEmail.Attributes.GetNamedItem("TravelerRefNumber").InnerText = NameNumber;
                    NewEmail.Attributes.GetNamedItem("TravelerRefNumber").InnerText = "1.1";
                    NewEmail.SelectSingleNode("m:Text", nsMgr).InnerText = String.Format("{0}/{1}", NewPersonName.SelectSingleNode("m:Surname", nsMgr).InnerText, NewPersonName.SelectSingleNode("m:GivenName", nsMgr).InnerText);
                }
            }

            CustomerInfo.RemoveChild(PersonName);
            CustomerInfo.RemoveChild(Telephone);
            CustomerInfo.RemoveChild(Email);
            CustomerInfo.RemoveChild(PassengerType);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 탑승객 정보 등록
        /// </summary>
        /// <param name="CID">ConversionID</param>
        /// <param name="STK">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PTC">탑승객 타입 코드 (ADT/CHD/INF/STU/LBR..)</param>
        /// <param name="PTL">탑승객 타이틀 (MR/MRS/MS/MSTR/MISS)</param>
        /// <param name="PSN">탑승객 영문성 (SurName)</param>
        /// <param name="PFN">탑승객 영문이름 (First Name)</param>
        /// <param name="PBD">탑승객 생년월일 (YYYYMMDD)</param>
        /// <param name="PTN">탑승객 전화번호</param>
        /// <param name="PMN">탑승객 휴대폰</param>
        /// <param name="PEA">탑승객 이메일주소</param>
        /// <returns></returns>
        [WebMethod(Description = "TravelItineraryAddInfoLLS")]
        public XmlElement AbacusTravelItineraryAddInfoRS(string CID, string STK, string GUID, string[] PTC, string[] PTL, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA)
        {
            XmlElement ReqXml = AbacusTravelItineraryAddInfoRQ(PTC, PTL, PSN, PFN, PBD, PTN, PMN, PEA);
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusTravelItineraryAddInfoRQ", "N", GUID);

            XmlElement ResXml = AbacusTravelItineraryAddInfoXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusTravelItineraryAddInfoRS", "N", GUID);

            return ResXml;
        }

        [WebMethod(Description = "TravelItineraryAddInfoLLS")]
        public XmlElement AbacusTravelItineraryAddInfoXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "TravelItineraryAddInfoRQ", CID, STK);
        }

        #endregion "TravelItineraryAddInfoLLS"

        #region "ArunkLLS"

        [WebMethod(Description = "ArunkLLS")]
        public XmlElement AbacusArunkRQ()
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("ArunkLLS"));

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// ARNK(비항공구간)
        /// </summary>
        /// <param name="CID">ConversionID</param>
        /// <param name="STK">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ArunkLLS")]
        public XmlElement AbacusArunkRS(string CID, string STK, string GUID)
        {
            XmlElement ReqXml = AbacusArunkRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusArunkRQ", "N", GUID);

            XmlElement ResXml = AbacusArunkXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusArunkRS", "N", GUID);

            return ResXml;
        }

        [WebMethod(Description = "ArunkLLS")]
        public XmlElement AbacusArunkXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "ArunkRQ", CID, STK);
        }

        #endregion "ArunkLLS"

		#region "SpecialServiceLLS"

		[WebMethod(Description = "SpecialServiceLLS")]
        public XmlElement AbacusSpecialServiceRQ(string MAC, string[] PTC, string[] NPF, string[] PSN, string[] PGN, string[] TBD)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("SpecialServiceLLS"));

			XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
			nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

			XmlNode SpecialServiceRQ = XmlDoc.SelectSingleNode("m:SpecialServiceRQ", nsMgr);
			XmlNode Service = SpecialServiceRQ.SelectSingleNode("m:Service", nsMgr);
			XmlNode NewService;

            string[] TRN = new String[PTC.Length];
			int n = 0;
			int m = 0;

            for (int i = 0; i < PTC.Length; i++)
			{
                if (PTC[i].Equals("CHD"))
				{
					NewService = SpecialServiceRQ.AppendChild(Service.CloneNode(true));
					NewService.Attributes.GetNamedItem("SSRCode").InnerText = "CHLD";
					NewService.SelectSingleNode("m:Airline", nsMgr).Attributes.GetNamedItem("Code").InnerText = MAC;
					NewService.SelectSingleNode("m:TPA_Extensions/m:Name", nsMgr).Attributes.GetNamedItem("Number").InnerText = ((i + 1).ToString() + ".1");
					NewService.SelectSingleNode("m:Text", nsMgr).InnerText = cm.AbacusDateTime(TBD[i]);
				}
                else if (PTC[i].Equals("INF"))
				{
					NewService = SpecialServiceRQ.AppendChild(Service.CloneNode(true));
					NewService.Attributes.GetNamedItem("SSRCode").InnerText = "INFT";
					NewService.SelectSingleNode("m:Airline", nsMgr).Attributes.GetNamedItem("Code").InnerText = MAC;
					NewService.SelectSingleNode("m:TPA_Extensions/m:Name", nsMgr).Attributes.GetNamedItem("Number").InnerText = TRN[m++];
					NewService.SelectSingleNode("m:Text", nsMgr).InnerText = String.Format("{0}/{1} {2}/{3}", PSN[i].ToUpper(), PGN[i].ToString(), NPF[i], cm.AbacusDateTime(TBD[i]));
				}
				else
					TRN[n++] = ((i + 1).ToString() + ".1");
			}

			SpecialServiceRQ.RemoveChild(Service);

			return XmlDoc.DocumentElement;
		}

		[WebMethod(Description = "SpecialServiceLLS")]
        public XmlElement AbacusSpecialServiceRS(string CID, string STK, string GUID, string MAC, string[] PTC, string[] NPF, string[] PSN, string[] PGN, string[] TBD)
		{
            XmlElement ReqXml = AbacusSpecialServiceRQ(MAC, PTC, NPF, PSN, PGN, TBD);
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusSpecialServiceRQ", "N", GUID);

			if (ReqXml.HasChildNodes)
			{
				XmlElement ResXml = AbacusSpecialServiceXml(CID, STK, ReqXml);
                cm.XmlFileSave(ResXml, ac.Name, "AbacusSpecialServiceRS", "N", GUID);

				return ResXml;
			}
			else
				return ReqXml;
		}

		[WebMethod(Description = "SpecialServiceLLS")]
		public XmlElement AbacusSpecialServiceXml(string CID, string STK, XmlElement ReqXml)
		{
			return ac.Execute(ReqXml.OuterXml, "SpecialServiceRQ", CID, STK);
		}

		#endregion "SpecialServiceLLS"

		#region "OTA_TravelItineraryReadLLS"

        [WebMethod(Description = "OTA_TravelItineraryReadLLS")]
        public XmlElement AbacusReadRQ(string PNR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("OTA_TravelItineraryReadLLS"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

            XmlDoc.SelectSingleNode("m:OTA_TravelItineraryReadRQ/m:UniqueID", nsMgr).Attributes.GetNamedItem("ID").InnerText = PNR;

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "OTA_TravelItineraryReadLLS")]
        public XmlElement AbacusReadRS(string CID, string STK, string PNR, string GUID)
        {
            XmlElement ReqXml = AbacusReadRQ(PNR);
            XmlElement ResXml = AbacusReadXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusReadRS_1", "N", GUID);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL(""));

            if (ResXml.SelectNodes("m:TravelItinerary/m:CustomerInfos/m:CustomerInfo/m:Customer/m:Telephone[starts-with(@PhoneNumber,'T*TAX/')]", nsMgr).Count.Equals(0))
            {
                AbacusAddTaxRS(CID, STK, "");
                ResXml = AbacusReadXml(CID, STK, ReqXml);
                cm.XmlFileSave(ResXml, ac.Name, "AbacusReadRS_2", "N", GUID);
            }

            return ResXml;
        }

        [WebMethod(Description = "OTA_TravelItineraryReadLLS")]
        public XmlElement AbacusReadXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "OTA_TravelItineraryReadRQ", CID, STK);
        }

		#endregion "OTA_TravelItineraryReadLLS"

        #region "TravelItineraryRead"

        [WebMethod(Description = "TravelItineraryRead")]
        public XmlElement TravelItineraryReadRQ(string PNR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("TravelItineraryReadRQ"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL("TravelItineraryRead_tir310"));

            XmlDoc.SelectSingleNode("m:TravelItineraryReadRQ/m:UniqueID", nsMgr).Attributes.GetNamedItem("ID").InnerText = PNR;

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "TravelItineraryRead")]
        public XmlElement TravelItineraryReadRS(string CID, string STK, string PNR, string GUID)
        {
            XmlElement ReqXml = TravelItineraryReadRQ(PNR);
            cm.XmlFileSave(ReqXml, ac.Name, "TravelItineraryReadRQ", "N", GUID);
            XmlElement ResXml = TravelItineraryReadXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "TravelItineraryReadRS_1", "N", GUID);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
            nsMgr.AddNamespace("tir310", AbacusConfig.NamespaceURL("TravelItineraryRead_tir310"));

            if (ResXml.SelectNodes("tir310:TravelItinerary/tir310:CustomerInfo/tir310:ContactNumbers/tir310:ContactNumber[starts-with(@Phone,'T*TAX/')]", nsMgr).Count.Equals(0))
            {
                AbacusAddTaxRS(CID, STK, "");
                ResXml = TravelItineraryReadXml(CID, STK, ReqXml);
                cm.XmlFileSave(ResXml, ac.Name, "TravelItineraryReadRS_2", "N", GUID);
            }

            return ResXml;
        }

        [WebMethod(Description = "TravelItineraryRead")]
        public XmlElement TravelItineraryReadXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "TravelItineraryReadRQ", CID, STK);
        }

        #endregion "TravelItineraryRead"

        #region "GetReservation"

        [WebMethod(Description = "GetReservation")]
        public XmlElement GetReservationRQ(string PNR)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("GetReservationRQ"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL("GetReservation_stl19"));

            XmlDoc.SelectSingleNode("m:GetReservationRQ/m:Locator", nsMgr).InnerText = PNR;

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "GetReservation")]
        public XmlElement GetReservationRS(string CID, string STK, string PNR, string GUID)
        {
            XmlElement ReqXml = GetReservationRQ(PNR);
            cm.XmlFileSave(ReqXml, ac.Name, "GetReservationRQ", "N", GUID);
            XmlElement ResXml = GetReservationXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "GetReservationRS_1", "N", GUID);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
            nsMgr.AddNamespace("stl19", AbacusConfig.NamespaceURL("GetReservation_stl19"));

            if (ResXml.SelectNodes("stl19:Reservation/stl19:PhoneNumbers/stl19:PhoneNumber/stl19:Number[starts-with(.,'T*TAX/')]", nsMgr).Count.Equals(0))
            {
                AbacusAddTaxRS(CID, STK, "");
                ResXml = GetReservationXml(CID, STK, ReqXml);
                cm.XmlFileSave(ResXml, ac.Name, "GetReservationRS_2", "N", GUID);
            }

            return ResXml;
        }

        [WebMethod(Description = "GetReservation")]
        public XmlElement GetReservationXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "GetReservationRQ", CID, STK);
        }

        #endregion "GetReservation"

        #region "OTA_AirPriceLLS"

        [WebMethod(Description = "OTA_AirPriceLLS")]
        public XmlElement AbacusAirPriceRQ(string VendorAirline)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("OTA_AirPriceLLS"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL("OTA_AirPriceLLS"));

            XmlDoc.SelectSingleNode("m:OTA_AirPriceRQ/m:PriceRequestInformation/m:OptionalQualifiers/m:FlightQualifiers/m:VendorPrefs/m:Airline", nsMgr).Attributes.GetNamedItem("Code").InnerText = VendorAirline;

			return XmlDoc.DocumentElement;
		}

		[WebMethod(Description = "OTA_AirPriceLLS")]
        public XmlElement AbacusAirPriceRS(string CID, string STK, string GUID, string VendorAirline)
		{
            XmlElement ReqXml = AbacusAirPriceRQ(VendorAirline);
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusAirPriceRQ", "N", GUID);

			XmlElement ResXml = AbacusAirPriceXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusAirPriceRS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "OTA_AirPriceLLS")]
		public XmlElement AbacusAirPriceXml(string CID, string STK, XmlElement ReqXml)
		{
			return ac.Execute(ReqXml.OuterXml, "OTA_AirPriceRQ", CID, STK);
		}

		#endregion "OTA_AirPriceLLS"

		#region "EndTransactionLLS"

		[WebMethod(Description = "EndTransactionLLS")]
		public XmlElement AbacusEndTransactionRQ()
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("EndTransactionLLS"));

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 트랜잭션 종료
		/// </summary>
		/// <param name="CID">ConversionID</param>
		/// <param name="STK">SecurityToken</param>
		/// <param name="GUID">고유번호</param>
		/// <returns></returns>
		[WebMethod(Description = "EndTransactionLLS")]
		public XmlElement AbacusEndTransactionRS(string CID, string STK, string GUID)
		{
			XmlElement ReqXml = AbacusEndTransactionRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "AbacusEndTransactionRQ", "N", GUID);

			XmlElement ResXml = AbacusEndTransactionXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusEndTransactionRS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "EndTransactionLLS")]
		public XmlElement AbacusEndTransaction2RS(string CID, string STK, string GUID)
		{
			XmlElement EndTransactionLLS = AbacusEndTransactionRQ();
			EndTransactionLLS.RemoveChild(EndTransactionLLS.FirstChild);
            cm.XmlFileSave(EndTransactionLLS, ac.Name, "AbacusEndTransaction2RQ", "N", GUID);

			XmlElement ResXml = AbacusEndTransactionXml(CID, STK, EndTransactionLLS);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusEndTransaction2RS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "EndTransactionLLS")]
		public XmlElement AbacusEndTransactionXml(string CID, string STK, XmlElement ReqXml)
		{
			return ac.Execute(ReqXml.OuterXml, "EndTransactionRQ", CID, STK);
		}

		#endregion "EndTransactionLLS"

		#region "OTA_CancelLLS"

		[WebMethod(Description = "OTA_CancelLLS")]
		public XmlElement AbacusCancelRQ()
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("OTA_CancelLLS"));

			return XmlDoc.DocumentElement;
		}

		[WebMethod(Description = "OTA_CancelLLS")]
		public XmlElement AbacusCancelRS(string CID, string STK, string PNR, string GUID)
		{
			AbacusCommand(CID, STK, String.Format("*{0}", PNR), GUID);
			AbacusCommand(CID, STK, "xi", GUID);
			AbacusCommand(CID, STK, "6KOJAEYOUNG", GUID);

			XmlElement ResXml = AbacusCommand(CID, STK, "E", GUID);
            cm.XmlFileSave(ResXml, ac.Name, "AbacusCancelRS", "N", GUID);

			return ResXml;
		}

		[WebMethod(Description = "OTA_CancelLLS")]
		public XmlElement AbacusCancelXml(string CID, string STK, XmlElement ReqXml)
		{
			return ac.Execute(ReqXml.OuterXml, "OTA_CancelRQ", CID, STK);
		}

		#endregion "OTA_CancelLLS"

        #region "DesignatePrinterLLS"

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterRQ(string Job, string JobValue)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("DesignatePrinterLLS"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL("DesignatePrinterLLS"));

            XmlNode Printers = XmlDoc.SelectSingleNode("m:DesignatePrinterRQ/m:Printers", nsMgr);

            if (Job.Equals("Hardcopy"))
            {
                Printers.RemoveChild(Printers.SelectSingleNode("m:InvoiceItinerary", nsMgr));
                Printers.RemoveChild(Printers.SelectSingleNode("m:Ticket", nsMgr));

                Printers.SelectSingleNode("m:Hardcopy", nsMgr).Attributes.GetNamedItem("LNIATA").InnerText = String.IsNullOrWhiteSpace(JobValue) ? ac.PrinterAddress : JobValue;
            }
            else if (Job.Equals("InvoiceItinerary"))
            {
                Printers.RemoveChild(Printers.SelectSingleNode("m:Hardcopy", nsMgr));
                Printers.RemoveChild(Printers.SelectSingleNode("m:Ticket", nsMgr));

                Printers.SelectSingleNode("m:InvoiceItinerary", nsMgr).Attributes.GetNamedItem("LNIATA").InnerText = String.IsNullOrWhiteSpace(JobValue) ? ac.PrinterAddress : JobValue;
            }
            else if (Job.Equals("Ticket"))
            {
                Printers.RemoveChild(Printers.SelectSingleNode("m:Hardcopy", nsMgr));
                Printers.RemoveChild(Printers.SelectSingleNode("m:InvoiceItinerary", nsMgr));

                Printers.SelectSingleNode("m:Ticket", nsMgr).Attributes.GetNamedItem("CountryCode").InnerText = JobValue;
            }

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterRS(string CID, string STK, string GUID, string Job, string JobValue)
        {
            XmlElement ReqXml = DesignatePrinterRQ(Job, JobValue);
            cm.XmlFileSave(ReqXml, ac.Name, "DesignatePrinterRQ", "N", GUID);

            XmlElement ResXml = DesignatePrinterXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "DesignatePrinterRS", "N", GUID);

            return ResXml;
        }

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterHardcopyRS(string CID, string STK, string GUID, string PrinterAddress)
        {
            return DesignatePrinterRS(CID, STK, GUID, "Hardcopy", PrinterAddress);
        }

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterInvoiceItineraryRS(string CID, string STK, string GUID, string PrinterAddress)
        {
            return DesignatePrinterRS(CID, STK, GUID, "InvoiceItinerary", PrinterAddress);
        }

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterTicketRS(string CID, string STK, string GUID, string CountryCode)
        {
            return DesignatePrinterRS(CID, STK, GUID, "Ticket", CountryCode);
        }

        [WebMethod(Description = "DesignatePrinterLLS")]
        public XmlElement DesignatePrinterXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "DesignatePrinterRQ", CID, STK);
        }

        #endregion "DesignatePrinterLLS"

        #region "AirTicketLLS"

        [WebMethod(Description = "AirTicketLLS")]
        public XmlElement AirTicketLLSRQ(string VendorAirline, string FOPType, string[] CardCode, string[] CardNumber, string[] CardExpireDate, string[] CardExtendedPayment, string[] Price, string[] NameNumber)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AirTicketLLS"));

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            nsMgr.AddNamespace("m", AbacusConfig.NamespaceURL("AirTicketLLS"));

            XmlNode AirTicketRQ = XmlDoc.SelectSingleNode("m:AirTicketRQ", nsMgr);
            XmlNode OptionalQualifiers = AirTicketRQ.SelectSingleNode("m:OptionalQualifiers", nsMgr);
            XmlNode FlightQualifiers = OptionalQualifiers.SelectSingleNode("m:FlightQualifiers", nsMgr);
            XmlNode FOPQualifiers = OptionalQualifiers.SelectSingleNode("m:FOP_Qualifiers", nsMgr);
            XmlNode MiscQualifiers = OptionalQualifiers.SelectSingleNode("m:MiscQualifiers", nsMgr);
            XmlNode PricingQualifiers = OptionalQualifiers.SelectSingleNode("m:PricingQualifiers", nsMgr);

            //탑승객 수
            AirTicketRQ.Attributes.GetNamedItem("NumResponses").InnerText = NameNumber.Length.ToString();

            //발권항공사
            FlightQualifiers.SelectSingleNode("m:VendorPrefs/m:Airline", nsMgr).Attributes.GetNamedItem("Code").InnerText = VendorAirline;

            //결제 정보
            if (FOPType.Equals("CA")) //현금
            {
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BasicFOP[not(@Type)]", nsMgr));
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BSP_Ticketing", nsMgr));
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP", nsMgr));
            }
            else if (FOPType.Equals("CD")) //카드
            {
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BasicFOP[@Type='CA']", nsMgr));
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BSP_Ticketing", nsMgr));
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP", nsMgr));

                SetPaymentCardInfo(FOPQualifiers.SelectSingleNode("m:BasicFOP/m:CC_Info/m:PaymentCard", nsMgr), CardCode[0].Trim(), CardNumber[0].Trim(), CardExpireDate[0].Trim(), CardExtendedPayment[0].Trim());
            }
            else if (FOPType.Equals("CC") && CardCode.Length.Equals(2)) //복합결제
            {
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BasicFOP[@Type='CA']", nsMgr));
                FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BasicFOP[not(@Type)]", nsMgr));

                if (!String.IsNullOrWhiteSpace(CardNumber[0]) || !String.IsNullOrWhiteSpace(CardNumber[1]))
                {
                    if (String.IsNullOrWhiteSpace(CardNumber[0])) //현금+카드
                    {
                        XmlNode BSPTicketing = FOPQualifiers.SelectSingleNode("m:BSP_Ticketing", nsMgr);
                        
                        FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP", nsMgr));
                        BSPTicketing.RemoveChild(BSPTicketing.SelectSingleNode("m:MultipleFOP[1]", nsMgr));

                        BSPTicketing.SelectSingleNode("m:MultipleFOP/m:Fare", nsMgr).Attributes.GetNamedItem("Amount").InnerText = Price[0].Trim();
                        SetPaymentCardInfo(BSPTicketing.SelectSingleNode("m:MultipleFOP/m:FOP_One/m:CC_Info/m:PaymentCard", nsMgr), CardCode[1].Trim(), CardNumber[1].Trim(), CardExpireDate[1].Trim(), CardExtendedPayment[1].Trim());
                    }
                    else if (String.IsNullOrWhiteSpace(CardNumber[1])) //카드+현금
                    {
                        XmlNode BSPTicketing = FOPQualifiers.SelectSingleNode("m:BSP_Ticketing", nsMgr);
                        
                        FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP", nsMgr));
                        BSPTicketing.RemoveChild(BSPTicketing.SelectSingleNode("m:MultipleFOP[2]", nsMgr));

                        BSPTicketing.SelectSingleNode("m:MultipleFOP/m:Fare", nsMgr).Attributes.GetNamedItem("Amount").InnerText = Price[0].Trim();
                        SetPaymentCardInfo(BSPTicketing.SelectSingleNode("m:MultipleFOP/m:FOP_Two/m:CC_Info/m:PaymentCard", nsMgr), CardCode[0].Trim(), CardNumber[0].Trim(), CardExpireDate[0].Trim(), CardExtendedPayment[0].Trim());
                    }
                    else //카드+카드
                    {
                        FOPQualifiers.RemoveChild(FOPQualifiers.SelectSingleNode("m:BSP_Ticketing", nsMgr));

                        FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP/m:Fare", nsMgr).Attributes.GetNamedItem("Amount").InnerText = Price[1].Trim();
                        SetPaymentCardInfo(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP/m:CC_One/m:CC_Info/m:PaymentCard", nsMgr), CardCode[0].Trim(), CardNumber[0].Trim(), CardExpireDate[0].Trim(), CardExtendedPayment[0].Trim());
                        SetPaymentCardInfo(FOPQualifiers.SelectSingleNode("m:MultipleCC_FOP/m:CC_Two/m:CC_Info/m:PaymentCard", nsMgr), CardCode[1].Trim(), CardNumber[1].Trim(), CardExpireDate[1].Trim(), CardExtendedPayment[1].Trim());
                    }
                }
            }

            //추가정보
            //MiscQualifiers.RemoveChild(MiscQualifiers.SelectSingleNode("m:Commission", nsMgr));
            MiscQualifiers.RemoveChild(MiscQualifiers.SelectSingleNode("m:Endorsement", nsMgr));
            MiscQualifiers.RemoveChild(MiscQualifiers.SelectSingleNode("m:Invoice", nsMgr));
            MiscQualifiers.RemoveChild(MiscQualifiers.SelectSingleNode("m:TourCode", nsMgr));

            //탑승객 정보
            if (NameNumber.Length > 1)
            {
                XmlNode NameSelect = PricingQualifiers.SelectSingleNode("m:NameSelect", nsMgr);
                XmlNode NewNameSelect = null;

                foreach (string StrNameNumber in NameNumber)
                {
                    if (!String.IsNullOrWhiteSpace(StrNameNumber))
                    {
                        NewNameSelect = PricingQualifiers.AppendChild(NameSelect.Clone());
                        NewNameSelect.Attributes.GetNamedItem("NameNumber").InnerText = StrNameNumber.Trim();
                    }
                }

                PricingQualifiers.RemoveChild(NameSelect);
            }
            else
                PricingQualifiers.SelectSingleNode("m:NameSelect", nsMgr).Attributes.GetNamedItem("NameNumber").InnerText = NameNumber[0].Trim();
            
            return XmlDoc.DocumentElement;
        }

        private void SetPaymentCardInfo(XmlNode PaymentCard, string CardCode, string CardNumber, string CardExpireDate, string CardExtendedPayment)
        {
            string StrCardExtendedPayment = cm.RequestInt(CardExtendedPayment).ToString();
            
            PaymentCard.Attributes.GetNamedItem("Code").InnerText = CardCode;
            PaymentCard.Attributes.GetNamedItem("Number").InnerText = CardNumber.Replace("-", "");
            PaymentCard.Attributes.GetNamedItem("ExpireDate").InnerText = CardExpireDate.Length.Equals(6) ? String.Format("{0}-{1}", CardExpireDate.Substring(0, 4), CardExpireDate.Substring(4, 2)) : CardExpireDate;

            if (StrCardExtendedPayment.Equals("0"))
                PaymentCard.Attributes.RemoveNamedItem("ExtendedPayment");
            else
                PaymentCard.Attributes.GetNamedItem("ExtendedPayment").InnerText = StrCardExtendedPayment;
        }

        /// <summary>
        /// 발권
        /// </summary>
        /// <param name="CID">ConversionID</param>
        /// <param name="STK">SecurityToken</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="VendorAirline">항공사코드</param>
        /// <param name="FOPType">결제수단(CA:현금, CD:카드, CC:복합결제(카드+현금,카드+카드)</param>
        /// <param name="CardCode">카드 2Code</param>
        /// <param name="CardNumber">카드 번호</param>
        /// <param name="CardExpireDate">카드 유효기간(YYYY-MM or YYYYMM)</param>
        /// <param name="CardExtendedPayment">카드 할부개월수(NN)</param>
        /// <param name="Price">현금결제일 경우 금액</param>
        /// <param name="NameNumber">탑승객번호</param>
        /// <returns></returns>
        [WebMethod(Description = "AirTicketLLS")]
        public XmlElement AirTicketLLSRS(string CID, string STK, string GUID, string VendorAirline, string FOPType, string[] CardCode, string[] CardNumber, string[] CardExpireDate, string[] CardExtendedPayment, string[] Price, string[] NameNumber)
        {
            XmlElement ReqXml = AirTicketLLSRQ(VendorAirline, FOPType, CardCode, CardNumber, CardExpireDate, CardExtendedPayment, Price, NameNumber);
            cm.XmlFileSave(ReqXml, ac.Name, "AirTicketLLSRQ", "N", GUID);

            XmlElement ResXml = AirTicketLLSXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AirTicketLLSRS", "N", GUID);

            return ResXml;
        }

        [WebMethod(Description = "AirTicketLLS")]
        public XmlElement AirTicketLLSXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "AirTicketLLSRQ", CID, STK);
        }

        #endregion "AirTicketLLS"

        #region "AirTicket(미개발)"

        [WebMethod(Description = "AirTicket")]
        public XmlElement AirTicketRQ()
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(ac.XmlFullPath("AirTicketRQ"));

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "AirTicket")]
        public XmlElement AirTicketRS(string CID, string STK, string GUID)
        {
            XmlElement ReqXml = AirTicketRQ();
            cm.XmlFileSave(ReqXml, ac.Name, "AirTicketRQ", "N", GUID);

            XmlElement ResXml = AirTicketXml(CID, STK, ReqXml);
            cm.XmlFileSave(ResXml, ac.Name, "AirTicketRS", "N", GUID);

            return ResXml;
        }

        [WebMethod(Description = "AirTicket")]
        public XmlElement AirTicketXml(string CID, string STK, XmlElement ReqXml)
        {
            return ac.Execute(ReqXml.OuterXml, "AirTicketRQ", CID, STK);
        }

        #endregion "AirTicket"

        #region "Queue"

        #endregion "Queue"

        #region "예약 후 텍스 추가 등록"

        //[WebMethod(Description = "PNR 조회 후 실행하면 TAX 조회하여 PNR에 TAX를 REMARK사항으로 등록해 준다.")]
		public void AbacusAddTaxRS(string CID, string STK, string GUID)
		{
			try
			{
				XmlElement AirPrice = AbacusAirPriceRS(CID, STK, GUID, "OZ");

				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.LoadXml(AirPrice.OuterXml.Replace("stl:", "").Replace(" xmlns=\"http://webservices.sabre.com/sabreXML/2011/10\"", "").Replace(" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"", "").Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "").Replace(" xmlns:stl=\"http://services.sabre.com/STL/v01\"", ""));

				if (XmlDoc.SelectNodes("//AirItineraryPricingInfo").Count > 0)
				{
					string[] Tax = new String[4];

					Tax[0] = "";
					Tax[1] = "";
					Tax[2] = "";
					Tax[3] = "";

					foreach (XmlNode AirItineraryPricingInfo in XmlDoc.SelectNodes("//AirItineraryPricingInfo"))
					{
						if (AirItineraryPricingInfo.SelectSingleNode("PassengerTypeQuantity").Attributes.GetNamedItem("Code").InnerText.Equals("ADT"))
						{
							Tax[0] = AirItineraryPricingInfo.SelectSingleNode("ItinTotalFare/Taxes").Attributes.GetNamedItem("TotalAmount").InnerText;

							if (AirItineraryPricingInfo.SelectNodes("//Surcharges").Count > 0)
							{
								Tax[3] = "6200"; //고정값으로 등록해도 무방(정확히는 Q값과 ROE를 곱한 후에 100원단위 올림해야 함)
							}
						}
						else if (AirItineraryPricingInfo.SelectSingleNode("PassengerTypeQuantity").Attributes.GetNamedItem("Code").InnerText.Equals("INF"))
							Tax[2] = AirItineraryPricingInfo.SelectSingleNode("ItinTotalFare/Taxes").Attributes.GetNamedItem("TotalAmount").InnerText;
						else if (AirItineraryPricingInfo.SelectSingleNode("PassengerTypeQuantity").Attributes.GetNamedItem("Code").InnerText.StartsWith("C"))
							Tax[1] = AirItineraryPricingInfo.SelectSingleNode("ItinTotalFare/Taxes").Attributes.GetNamedItem("TotalAmount").InnerText;
						else if (AirItineraryPricingInfo.SelectSingleNode("PassengerTypeQuantity").Attributes.GetNamedItem("Code").InnerText.StartsWith("I"))
							Tax[2] = AirItineraryPricingInfo.SelectSingleNode("ItinTotalFare/Taxes").Attributes.GetNamedItem("TotalAmount").InnerText;
					}

					AbacusCommand(CID, STK, String.Format("9T*TAX/A{0}/C{1}/I{2}/Q{3}", Tax[0], Tax[1], Tax[2], Tax[3]), GUID);
					AbacusCommand(CID, STK, "*A", GUID);
					AbacusCommand(CID, STK, "6KO/JAEYOUNG", GUID);
					AbacusCommand(CID, STK, "E", GUID);
				}
			}
			catch (Exception) { throw; }
		}

		#endregion "예약 후 텍스 추가 등록"

		#region "Command"

		[WebMethod(Description = "Command(Entry) 서비스용")]
		public XmlElement AbacusCommand(string ConversionID, string SecurityToken, string Entry, string GUID)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(ac.XmlFullPath("SabreCommandLLS"));
			XmlDoc.GetElementsByTagName("HostCommand")[0].InnerText = Entry;

            cm.XmlFileSave(XmlDoc, ac.Name, "AbacusCommandRQ", "N", GUID);

			XmlElement ExecuteXml = ac.Execute(XmlDoc.OuterXml, "SabreCommandLLSRQ", ConversionID, SecurityToken);
            cm.XmlFileSave(ExecuteXml, ac.Name, "AbacusCommandRS", "N", GUID);

			return ExecuteXml;
		}

		/// <summary>
		/// Command 서비스
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="Entry">명령어</param>
		/// <returns></returns>
		//[WebMethod(Description = "Command 서비스")]
		public XmlElement CommandRS(string Entry)
		{
			string GUID = cm.GetGUID;
			string CID = String.Empty;
			string STK = String.Empty;

			try
			{
				//### 01.세션생성 #####
				XmlElement Session = SessionCreate();

				CID = Session.ChildNodes[0].InnerText;
				STK = Session.ChildNodes[1].InnerText;

				//### 02.CommandCryptic #####
				XmlElement ResXml = AbacusCommand(CID, STK, Entry, GUID);
				
				//### 03.세션종료 #####
				SessionClose(CID, STK);
				CID = "";
				STK = "";

				return ResXml;
			}
			catch (Exception ex)
			{
				//### 세션종료 #####
				if (!String.IsNullOrWhiteSpace(CID))
					SessionClose(CID, STK);

                return new MWSException(ex, hcc, ac.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
			}
		}

		#endregion "Command"
	}
}