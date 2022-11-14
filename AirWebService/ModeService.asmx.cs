using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// MODEWARE, CRS 용 항공 웹서비스
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class ModeService : System.Web.Services.WebService
	{
		Common cm;
		ModeConfig mc;
		AirService mas;
		AmadeusAirService amd;
		AbacusAirService aas;
        GalileoAirService gas;
        LogSave log;
		HttpContext hcc;

		public ModeService()
		{
			cm = new Common();
			mc = new ModeConfig();
			mas = new AirService();
			amd = new AmadeusAirService();
			aas = new AbacusAirService();
            gas = new GalileoAirService();
            log = new LogSave();
			hcc = HttpContext.Current;
		}

		#region "GDS별 PNR 조회"

		/// <summary>
		/// 예약 조회(PNR 이용) - GDS별 구조로 결과 리턴
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="GDS">GDS명</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "예약 조회(PNR 이용) - GDS별 구조로 결과 리턴")]
		public XmlElement SearchBookingPNR(int SNM, string GDS, string PNR)
		{
            string LogGUID = cm.GetGUID;
            
            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 103;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
                if (GDS.Equals("Abacus"))
                    GDS = "Abacus_TravelItineraryRead";
                
                return mas.SearchBookingPNR(SNM, GDS, PNR, LogGUID);
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("SNM", SNM);
				ex.Data.Add("GDS", GDS);
				ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 103, 0, 0).ToErrors;
			}
		}

		/// <summary>
		/// 예약 조회(PNR 이용) - 통합용 구조로 결과 리턴
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="GDS">GDS명</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "예약 조회(PNR 이용) - 통합용 구조로 결과 리턴")]
		public XmlElement ModeSearchBookingPNR(int SNM, string GDS, string PNR)
		{
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 100;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
                XmlElement ModeXml = mas.ModeSearchBookingPNR(SNM, GDS, PNR, "", LogGUID);
                XmlNode BookingInfo = ModeXml.SelectSingleNode("bookingInfo");

                BookingInfo.SelectSingleNode("gds").InnerText = GDS;

                if (ModeXml.SelectSingleNode("flightInfo").HasChildNodes)
                {
                    BookingInfo.SelectSingleNode("bookingStatus").Attributes.GetNamedItem("code").InnerText = mas.BookingStatus(ModeXml.SelectSingleNode("flightInfo"));
                    BookingInfo.SelectSingleNode("bookingStatus").AppendChild((XmlCDataSection)ModeXml.OwnerDocument.CreateCDataSection(Common.BookingStatusText(BookingInfo.SelectSingleNode("bookingStatus").Attributes.GetNamedItem("code").InnerText)));
                    BookingInfo.SelectSingleNode("bookingAirline").Attributes.GetNamedItem("code").InnerText = ModeXml.SelectSingleNode("flightInfo/segGroup/seg").Attributes.GetNamedItem("mcc").InnerText;
                }

                //도시/공항명 추가
                string CodeList = string.Empty;

                foreach (XmlNode Seg in ModeXml.SelectNodes("flightInfo/segGroup/seg"))
                {
                    CodeList += String.Concat(Seg.Attributes.GetNamedItem("dlc").InnerText, ",");
                    CodeList += String.Concat(Seg.Attributes.GetNamedItem("alc").InnerText, ",");

                    foreach (XmlNode Seg2 in Seg.SelectNodes("seg"))
                    {
                        CodeList += String.Concat(Seg2.Attributes.GetNamedItem("dlc").InnerText, ",");
                        CodeList += String.Concat(Seg2.Attributes.GetNamedItem("alc").InnerText, ",");
                    }
                }

                DataView dv = Common.GetCityAirportName("A", CodeList).Tables[0].DefaultView;

                foreach (XmlNode Seg in ModeXml.SelectNodes("flightInfo/segGroup/seg"))
                {
                    dv.RowFilter = String.Format("코드='{0}'", Seg.Attributes.GetNamedItem("dlc").InnerText);
                    Seg.Attributes.GetNamedItem("dlcn").InnerText = dv[0]["한글명"].ToString();

                    dv.RowFilter = String.Format("코드='{0}'", Seg.Attributes.GetNamedItem("alc").InnerText);
                    Seg.Attributes.GetNamedItem("alcn").InnerText = dv[0]["한글명"].ToString();

                    foreach (XmlNode Seg2 in Seg.SelectNodes("seg"))
                    {
                        dv.RowFilter = String.Format("코드='{0}'", Seg2.Attributes.GetNamedItem("dlc").InnerText);
                        Seg2.Attributes.GetNamedItem("dlcn").InnerText = dv[0]["한글명"].ToString();

                        dv.RowFilter = String.Format("코드='{0}'", Seg2.Attributes.GetNamedItem("alc").InnerText);
                        Seg2.Attributes.GetNamedItem("alcn").InnerText = dv[0]["한글명"].ToString();
                    }
                }

                return ModeXml;
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("SNM", SNM);
				ex.Data.Add("GDS", GDS);
				ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 100, 0, 0).ToErrors;
			}
		}

        /// <summary>
        /// PNR 조회 결과를 통합용 구조로 변환
        /// </summary>
        /// <param name="GDS">GDS명</param>
        /// <param name="ResXml">PNR 조회 결과</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR조회 결과를 통합용 구조로 변환")]
        public XmlElement ToModeSearchBookingPNR(string GDS, XmlElement ResXml)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = 383;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = ResXml.OuterXml;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                XmlElement ModeXml = mas.ToModeSearchBookingPNR(0, GDS, "", ResXml);
                XmlNode BookingInfo = ModeXml.SelectSingleNode("bookingInfo");

                BookingInfo.SelectSingleNode("gds").InnerText = GDS;

                if (ModeXml.SelectSingleNode("flightInfo").HasChildNodes)
                {
                    BookingInfo.SelectSingleNode("bookingStatus").Attributes.GetNamedItem("code").InnerText = mas.BookingStatus(ModeXml.SelectSingleNode("flightInfo"));
                    BookingInfo.SelectSingleNode("bookingStatus").AppendChild((XmlCDataSection)ModeXml.OwnerDocument.CreateCDataSection(Common.BookingStatusText(BookingInfo.SelectSingleNode("bookingStatus").Attributes.GetNamedItem("code").InnerText)));
                    BookingInfo.SelectSingleNode("bookingAirline").Attributes.GetNamedItem("code").InnerText = ModeXml.SelectSingleNode("flightInfo/segGroup/seg").Attributes.GetNamedItem("mcc").InnerText;
                }

                return ModeXml;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("GDS", GDS);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 383, 0, 0).ToErrors;
            }
        }

		#endregion "GDS별 PNR 조회"

		#region "역큐잉"

		/// <summary>
		/// 모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="PNR">PNR No.</param>
		/// <param name="OfficeId">여행사 Office ID</param>
		/// <remarks>
		/// ① RT 1234-5678
		/// ② ES SELKP3300-B
		/// ③ ER
		/// </remarks>
		/// <returns></returns>
		[WebMethod(Description = "QTransfer(Amadeus 큐잉전용)(모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌)")]
		public XmlElement AmadeusQTransfer(int SNM, string PNR, string OfficeId)
		{
			string GUID = cm.GetGUID;
            string LogGUID = GUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 98;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;
                sqlParam[8].Value = OfficeId;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

			try
			{
                return mas.AmadeusQTransfer(SNM, PNR, OfficeId, GUID);
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("SNM", SNM);
				ex.Data.Add("PNR", PNR);
				ex.Data.Add("OfficeId", OfficeId);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 98, 0, 0).ToErrors;
			}
		}

		/// <summary>
		/// 모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌
		/// </summary>
		/// <param name="PNR">PNR No.</param>
		/// <param name="DKCode">여행사 DK Code</param>
		/// <remarks>
		/// ① *JNWXSB
		/// ② DKJ2GD00 OR (5WT-MD3D/ON)
		/// ③ E
		/// </remarks>
		/// <returns></returns>
		[WebMethod(Description = "QTransfer(Abacus 큐잉전용)(모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌)")]
		public XmlElement AbacusQTransfer(string PNR, string DKCode)
		{
			string GUID = cm.GetGUID;
            string LogGUID = GUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 96;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;
                sqlParam[8].Value = DKCode;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

			try
			{
				return mas.AbacusQTransfer(PNR, DKCode, GUID);
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("PNR", PNR);
				ex.Data.Add("DKCode", DKCode);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 96, 0, 0).ToErrors;
			}
		}

        /// <summary>
        /// 모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌
        /// </summary>
        /// <param name="PNR">PNR No.</param>
        /// <param name="PCC">여행사 PCC</param>
        /// <remarks>
        /// ① *JNWXSB
        /// ② DKJ2GD00 OR (5WT-MD3D/ON)
        /// ③ E
        /// </remarks>
        /// <returns></returns>
        [WebMethod(Description = "QTransfer(Abacus 큐잉전용)(모두투어 PNR을 대리점에서 사용 가능하도록 권한을 넘겨줌)")]
        public XmlElement AbacusQTransferPCC(string PNR, string PCC)
        {
            string GUID = cm.GetGUID;
            string LogGUID = GUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 377;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;
                sqlParam[8].Value = PCC;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                return mas.AbacusQTransferPCC(PNR, PCC, GUID);
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("PNR", PNR);
                ex.Data.Add("PCC", PCC);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 377, 0, 0).ToErrors;
            }
        }

		#endregion "역큐잉"

		#region "역큐잉 해지"

		/// <summary>
		/// 대리점에 주었던 모두투어 PNR 사용권한 해지
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="PNR">PNR No.</param>
		/// <remarks>
		/// ① RT 1234-5678
		/// ② ESX
		/// ③ ER
		/// </remarks>
		/// <returns></returns>
		[WebMethod(Description = "QTransferClose(Amadeus 큐잉해지전용)(대리점에 주었던 모두투어 PNR 사용권한 해지)")]
		public XmlElement AmadeusQTransferClose(int SNM, string PNR)
		{
			string GUID = cm.GetGUID;
            string LogGUID = GUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 99;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

			try
			{
				XmlElement XmlElem;
				XmlNode Session = amd.Authenticate(SNM, String.Concat(GUID, "-01"));
				string SID = Session.SelectSingleNode("session/sessionId").InnerText;
				string SCT = Session.SelectSingleNode("session/securityToken").InnerText;
				int SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

				amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Concat("RT", PNR));
				amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"), "ESX");
				XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-04"), "ER");

				XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlElem.OwnerDocument.NameTable);
				xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Command_Cryptic"));

				if (XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText.IndexOf("WARNING") != -1)
				{
					XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-05"), "ER");
				}

				amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-06"));

				return XmlElem;
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("SNM", SNM);
				ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 99, 0, 0).ToErrors;
			}
		}

		#endregion "역큐잉 해지"

		#region "룰번호 및 Tariff ID 조회"

		/// <summary>
		/// PNR정보를 이용하여 룰번호 및 Tariff ID 조회
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR정보를 이용하여 룰번호 및 Tariff ID 조회")]
		public XmlElement SearchRuleTariffWithPNR(int SNM, string PNR)
		{
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 107;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				return mas.CheckRuleTariffPNRRS(SNM, PNR);
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 107, 0, 0).ToErrors;
			}
		}

        /// <summary>
        /// PNR정보를 이용하여 룰번호 및 Tariff ID 조회한 후에 PNR에 Remarks 사항으로 추가
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR정보를 이용하여 룰번호 및 Tariff ID 조회한 후에 PNR에 Remarks 사항으로 추가")]
        public string AddRuleTariffWithPNR(int SNM, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 97;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                string RuleID = "";
                string TariffID = "";
                
                foreach (XmlNode FlightDetails in mas.CheckRuleTariffPNRRS(SNM, PNR).SelectNodes("flightDetails"))
                {
                    if (!String.IsNullOrWhiteSpace(RuleID))
                    {
                        RuleID = String.Concat(RuleID, ", ");
                        TariffID = String.Concat(TariffID, ", ");
                    }

                    RuleID = String.Concat(RuleID, FlightDetails.SelectSingleNode("rule").Attributes.GetNamedItem("id").InnerText);
                    TariffID = String.Concat(TariffID, FlightDetails.SelectSingleNode("tariff").Attributes.GetNamedItem("id").InnerText);
                }

                if (!String.IsNullOrWhiteSpace(RuleID))
                {
                    RuleID = String.Concat("RuleID: ", RuleID);
                    TariffID = String.Concat("TariffID: ", TariffID);

                    string GUID = cm.GetGUID;
                    string SID = String.Empty;
                    string SCT = String.Empty;
                    int SQN = 0;
                    
                    try
                    {
                        //### 01.세션생성 #####
                        XmlElement Session = amd.Authenticate(SNM, String.Concat(GUID, "-01"));

                        SID = Session.SelectSingleNode("session/sessionId").InnerText;
                        SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                        SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                        //### 02.PNR조회 #####
                        amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Concat("RT", PNR));

                        //### 03.Remarks 추가 #####
                        amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"), String.Format("RM {0} / {1}", RuleID, TariffID));

                        //### 04.변경사항 저장 #####
                        amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-04"), "ET");

                        //### 05.세션종료 #####
                        SQN = amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-05"));
                    }
                    catch (Exception ex)
                    {
                        //### 세션종료 #####
                        if (SQN > 0)
                            amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);

                        throw new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                    }

                    return String.Format("{0} / {1}", RuleID, TariffID);
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 97, 0, 0).ToString;
            }
        }

		#endregion "룰번호 및 Tariff ID 조회"

		#region "요금규정 조회"

		/// <summary>
		/// PNR정보를 이용하여 요금규정 조회
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR정보를 이용하여 요금규정 조회")]
		public XmlElement SearchRulePNRRS(int SNM, string PNR)
		{
            string LogGUID = cm.GetGUID;
            
            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 106;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				return mas.SearchRulePNRRS(SNM, PNR);
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 106, 0, 0).ToErrors;
			}
		}

		#endregion "요금규정 조회"

		#region "항공사 TL계산"

		/// <summary>
		/// PNR정보를 이용하여 항공사 TL계산
		/// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="OID">주문번호</param>
        /// <param name="IBN">주문아이템번호</param>
		/// <param name="GDS">GDS명</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR정보를 이용하여 항공사 TL계산")]
        public string SearchBookingPNRTL(int SNM, int OID, int IBN, string GDS, string PNR)
		{
            string LogGUID = cm.GetGUID;

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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 104;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = OID;
                sqlParam[3].Value = IBN;
                sqlParam[4].Value = "";
                sqlParam[5].Value = Environment.MachineName;
                sqlParam[6].Value = hcc.Request.HttpMethod;
                sqlParam[7].Value = hcc.Request.UserHostAddress;
                sqlParam[8].Value = LogGUID;
                sqlParam[9].Value = GDS;
                sqlParam[10].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
                return SearchBookingTL(OID, IBN, GDS, mas.SearchBookingPNR(SNM, GDS, PNR, LogGUID));
			}
			catch (Exception ex)
			{
                new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 104, OID, 0);
				return "";
			}
		}

		/// <summary>
		/// PNR정보를 이용하여 항공사 TL계산
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="IBN">주문아이템번호</param>
		/// <param name="GDS">GDS명</param>
		/// <param name="PNRInfo">PNR정보</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR정보를 이용하여 항공사 TL계산")]
        public string SearchBookingTL(int OID, int IBN, string GDS, XmlElement PNRInfo)
		{
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = 105;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNRInfo.OuterXml;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				return mas.SelectATL(OID, IBN, GDS, PNRInfo)[1];
			}
			catch (Exception ex)
			{
                new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 105, OID, 0);
				return "";
			}
		}

		#endregion "항공사 TL계산"

		#region "PNR Copy"

		/// <summary>
		/// PNR Copy
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="GDS">GDS명</param>
		/// <param name="PNR">PNR번호</param>
		/// <returns></returns>
		[WebMethod(Description = "PNR Copy")]
		public XmlElement PNRCopy(int SNM, string GDS, string PNR)
        {
            string LogGUID = cm.GetGUID;
            
            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 101;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				if (String.Compare(GDS, "Amadeus", true).Equals(0) || String.Compare(GDS, "Topas", true).Equals(0))
					return amd.CommandRS(SNM, String.Concat("RT", PNR));
				
                else if (String.Compare(GDS, "Abacus", true).Equals(0))
					return aas.CommandRS(String.Concat("*", PNR));
                
                else if (String.Compare(GDS, "Galileo", true).Equals(0))
                    return gas.CommandRS(String.Concat("*", PNR));
				
                else
					throw new Exception("서비스가 지원되지 않는 GDS입니다.");
			}
			catch (Exception ex)
			{
				ex.Data.Clear();
				ex.Data.Add("SNM", SNM);
				ex.Data.Add("GDS", GDS);
				ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 101, 0, 0).ToErrors;
			}
		}

		#endregion "PNR Copy"

        #region "PNR Pricing"

        /// <summary>
        /// PNR Pricing
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GDS">GDS명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR Pricing")]
        public XmlElement PNRPricing(int SNM, string GDS, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 102;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                string GUID = cm.GetGUID;
                
                if (String.Compare(GDS, "Amadeus", true).Equals(0) || String.Compare(GDS, "Topas", true).Equals(0))
                {
                    string SID = String.Empty;
                    string SCT = String.Empty;
                    int SQN = 0;

                    try
                    {
                        //결과
                        XmlElement ResXml;

                        //네임스페이스
                        XmlNamespaceManager xnMgr;

                        //### 01.세션생성 #####
                        XmlElement Session = amd.Authenticate(SNM, String.Concat(GUID, "-01"));

                        SID = Session.SelectSingleNode("session/sessionId").InnerText;
                        SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                        SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                        //### 02.PNR조회(PNR_Retrieve) #####
                        ResXml = amd.RetrieveRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), PNR);
                        cm.XmlFileSave(ResXml, mc.Name, "RetrieveRS", "N", String.Concat(GUID, "-02"));

                        xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("PNR_Reply"));

                        //한국출발여부
                        bool DepartureFromKorea = false;

                        if (ResXml.SelectNodes("m:originDestinationDetails/m:itineraryInfo", xnMgr).Count > 0)
                        {
                            if (ResXml.SelectNodes("m:originDestinationDetails/m:itineraryInfo/m:travelProduct/m:boardpointDetail", xnMgr).Count > 0)
                                DepartureFromKorea = Common.KoreaOfAirport(ResXml.SelectSingleNode("m:originDestinationDetails/m:itineraryInfo/m:travelProduct/m:boardpointDetail/m:cityCode", xnMgr).InnerText);
                        }
                        else
                        {
                            throw new Exception("여정 정보가 존재하지 않습니다.");
                        }

                        //### 03.운임Pricing(Fare_PricePNRWithBookingClass) #####
                        ResXml = amd.PricePNRWithBookingClassPricingRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"), ((ResXml.SelectNodes("m:originDestinationDetails/m:itineraryInfo/m:itineraryReservationInfo", xnMgr).Count > 0 && ResXml.SelectNodes("m:originDestinationDetails/m:itineraryInfo/m:itineraryReservationInfo/m:reservation/m:companyId", xnMgr).Count > 0) ? ResXml.SelectSingleNode("m:originDestinationDetails/m:itineraryInfo/m:itineraryReservationInfo/m:reservation/m:companyId", xnMgr).InnerText : null), (DepartureFromKorea) ? "RP" : "", null);
                        cm.XmlFileSave(ResXml, mc.Name, "PricePNRWithBookingClassRS", "N", String.Concat(GUID, "-03"));

                        //오류 결과일 경우 예외 처리
                        xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_PricePNRWithBookingClassPricing"));

                        if (ResXml.SelectNodes("m:errorGroup", xnMgr).Count > 0)
                        {
                            throw new Exception(ResXml.SelectSingleNode("m:errorGroup/m:errorWarningDescription/m:freeText", xnMgr).InnerText);
                        }

                        //### 04.세션종료 #####
                        SQN = amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-04"));

                        return mas.ToModeSearchBookingPriceAmadeus(ResXml, xnMgr, null, null);
                    }
                    catch (Exception ex)
                    {
                        //### 세션종료 #####
                        if (SQN > 0)
                            amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);

                        throw new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                    }
                }
                else if (String.Compare(GDS, "Abacus", true).Equals(0))
                {
                    string CID = String.Empty;
                    string STK = String.Empty;

                    try
                    {
                        //결과
                        XmlElement ResXml;

                        //네임스페이스
                        XmlNamespaceManager xnMgr;

                        //### 01.세션생성 #####
                        XmlElement Session = aas.SessionCreate();
                        CID = Session.ChildNodes[0].InnerText;
                        STK = Session.ChildNodes[1].InnerText;

                        //### 02.PNR조회(AbacusReadXml) #####
                        ResXml = aas.TravelItineraryReadXml(CID, STK, aas.TravelItineraryReadRQ(PNR));
                        cm.XmlFileSave(ResXml, mc.Name, "TravelItineraryReadXml", "N", String.Concat(GUID, "-02"));

                        xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("stl", AbacusConfig.NamespaceURL("TravelItineraryRead_stl"));
                        xnMgr.AddNamespace("tir38", AbacusConfig.NamespaceURL("TravelItineraryRead_tir38"));

                        //오류시
                        if (ResXml.SelectNodes("stl:ApplicationResults/stl:Error", xnMgr).Count > 0)
                        {
                            if (ResXml.SelectSingleNode("stl:ApplicationResults/stl:Error/stl:SystemSpecificResults/stl:Message", xnMgr).InnerText.Trim().Equals("NAK3 - UPDATED PNR CURRENTLY IN AAA - FINISH OR IGNORE"))
                            {
                                aas.AbacusCommand(CID, STK, "E", String.Concat(GUID, "-03"));
                                ResXml = aas.TravelItineraryReadXml(CID, STK, aas.TravelItineraryReadRQ(PNR));
                                cm.XmlFileSave(ResXml, "Abacus", "TravelItineraryReadXml", "N", String.Concat(GUID, "-03"));
                            }
                        }

                        //### 03.운임Pricing(AbacusAirPriceRS) #####
                        ResXml = aas.AbacusAirPriceRS(CID, STK, String.Concat(GUID, "-04"), "OZ");

                        xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("m", AbacusConfig.NamespaceURL("OTA_AirPriceLLS"));

                        //### 04.세션종료 #####
                        aas.SessionClose(CID, STK);
                        CID = "";
                        STK = "";

                        return mas.ToModeSearchBookingPriceAbacus(ResXml, xnMgr, null, null);
                    }
                    catch (Exception ex)
                    {
                        //### 세션종료 #####
                        if (!String.IsNullOrWhiteSpace(CID))
                            aas.SessionClose(CID, STK);

                        throw new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                    }
                }
                else
                    throw new Exception("서비스가 지원되지 않는 GDS입니다.");
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("SNM", SNM);
                ex.Data.Add("GDS", GDS);
                ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 102, 0, 0).ToErrors;
            }
        }

        #endregion "PNR Pricing"

        #region "PNR조회(RTW)"

        /// <summary>
        /// PNR Copy
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GDS">GDS명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR 조회(RTW)")]
        public XmlElement PNRRetrieveRTW(int SNM, string GDS, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 382;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GDS;
                sqlParam[8].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                string GUID = cm.GetGUID;
                XmlElement XmlElem = null;
                XmlNamespaceManager xnMgr = null;
                string TotalString = string.Empty;
                string PrevString = string.Empty;
                bool TheEnd = false;
                int idx = 0;

                if (String.Compare(GDS, "Amadeus", true).Equals(0) || String.Compare(GDS, "Topas", true).Equals(0))
                {
                    string SID = String.Empty;
                    string SCT = String.Empty;
                    int SQN = 0;
                    
                    try
                    {
                        //### 01.세션생성 #####
                        XmlNode Session = amd.Authenticate(SNM, String.Concat(GUID, "-01"));

                        SID = Session.SelectSingleNode("session/sessionId").InnerText;
                        SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                        SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                        //### 02.PNR조회 #####
                        amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Concat("RT", PNR));

                        //### 03.RTW #####
                        XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"), "RTW");

                        xnMgr = new XmlNamespaceManager(XmlElem.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Command_Cryptic"));

                        PrevString = XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText;
                        TotalString = String.Concat(PrevString, Environment.NewLine, Environment.NewLine);
                        idx = 4;

                        //### 04.MD #####
                        while (!TheEnd)
                        {
                            XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-", cm.NumPosition(Convert.ToString(idx++), 2)), "MD");

                            if (PrevString.Equals(XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText))
                                TheEnd = true;
                            else
                            {
                                PrevString = XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText;
                                TotalString += String.Concat(PrevString, Environment.NewLine, Environment.NewLine);

                                if (idx > 20)
                                    TheEnd = true;
                            }
                        }

                        //### 05.세션종료 #####
                        amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-", cm.NumPosition(Convert.ToString(idx++), 2)));

                        XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).RemoveAll();
                        XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).AppendChild((XmlCDataSection)XmlElem.OwnerDocument.CreateCDataSection(TotalString));

                        return XmlElem;
                    }
                    catch (Exception)
                    {
                        //### 세션종료 #####
                        if (SQN > 0)
                            amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);

                        throw;
                    }
                }
                else if (String.Compare(GDS, "Abacus", true).Equals(0))
                {
                    string CID = String.Empty;
                    string STK = String.Empty;

                    try
                    {
                        //### 01.세션생성 #####
                        XmlElement Session = aas.SessionCreate();
                        CID = Session.ChildNodes[0].InnerText;
                        STK = Session.ChildNodes[1].InnerText;

                        //### 02.PRN조회 #####
                        XmlElem = aas.AbacusCommand(CID, STK, String.Concat("*", PNR), String.Concat(GUID, "-01"));

                        xnMgr = new XmlNamespaceManager(XmlElem.OwnerDocument.NameTable);
                        xnMgr.AddNamespace("m", AbacusConfig.NamespaceURL("SabreCommandLLS"));

                        PrevString = XmlElem.SelectSingleNode("m:Response", xnMgr).InnerText;
                        TotalString = String.Concat(PrevString, Environment.NewLine, Environment.NewLine);
                        idx = 2;

                        //### 04.MD #####
                        while (!TheEnd)
                        {
                            XmlElem = aas.AbacusCommand(CID, STK, "MD", String.Concat(GUID, "-", cm.NumPosition(Convert.ToString(idx++), 2)));

                            if (XmlElem.SelectSingleNode("m:Response", xnMgr).InnerText.IndexOf("NOTHING TO SCROLL") != -1)
                                TheEnd = true;
                            else
                            {
                                PrevString = XmlElem.SelectSingleNode("m:Response", xnMgr).InnerText;
                                TotalString += String.Concat(PrevString, Environment.NewLine, Environment.NewLine);

                                if (idx > 20)
                                    TheEnd = true;
                            }
                        }

                        aas.SessionClose(CID, STK);
                        CID = "";
                        STK = "";

                        XmlElem.SelectSingleNode("m:Response", xnMgr).RemoveAll();
                        XmlElem.SelectSingleNode("m:Response", xnMgr).AppendChild((XmlCDataSection)XmlElem.OwnerDocument.CreateCDataSection(TotalString));

                        return XmlElem;
                    }
                    catch (Exception)
                    {
                        //### 세션종료 #####
                        if (!String.IsNullOrWhiteSpace(CID))
                            aas.SessionClose(CID, STK);

                        throw;
                    }
                }
                else
                    throw new Exception("서비스가 지원되지 않는 GDS입니다.");
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("SNM", SNM);
                ex.Data.Add("GDS", GDS);
                ex.Data.Add("PNR", PNR);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 382, 0, 0).ToErrors;
            }
        }

        private string XmlNodeString(XmlNode node)
        {
            string data = string.Empty;

            foreach (XmlCDataSection CData in node.ChildNodes)
            {
                data += String.Concat(CData.InnerText, Environment.NewLine);
            }

            return data;
        }

        #endregion "PNR조회(RTW)"

        #region "E-TicketNumber 리스트"

        /// <summary>
        /// E-TicketNumber 리스트
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "E-TicketNumber 리스트")]
        public XmlElement ETicketNumberListRS(int SNM, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 378;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                return mas.ETicketNumberListRS(SNM, PNR);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 378, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// E-TicketNumber 상태 리스트
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "E-TicketNumber 상태 리스트")]
        public XmlElement ETicketNumberStatusListRS(int SNM, string PNR)
        {
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 379;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                return mas.ETicketNumberStatusListRS(SNM, PNR);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 379, 0, 0).ToErrors;
            }
        }

        #endregion "E-TicketNumber 리스트"

        #region "E-Ticket VOID"

        /// <summary>
        /// E-Ticket VOID(Command)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <param name="TicketNumber">이티켓번호</param>
        /// <returns></returns>
        [WebMethod(Description = "E-Ticket VOID(Command)")]
        public XmlElement ETicketVoidCommandRS(int SNM, string PNR, string TicketNumber)
        {
            string GUID = cm.GetGUID;
            string LogGUID = GUID;
            string SID = String.Empty;
            string SCT = String.Empty;
            int SQN = 0;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 380;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;
                sqlParam[8].Value = TicketNumber;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                //결과
                XmlElement ResXml;

                //네임스페이스
                //XmlNamespaceManager xnMgr;

                //### 01.세션생성 #####
                XmlElement Session = amd.Authenticate(SNM, String.Concat(GUID, "-01"));

                SID = Session.SelectSingleNode("session/sessionId").InnerText;
                SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                //### 02.이티켓보이드(CommandCryptic) #####
                ResXml = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Format("TRDC/TK-{0}", TicketNumber));
                cm.XmlFileSave(Session, mc.Name, "CommandCryptic", "N", String.Concat(GUID, "-02"));

                //### 03.세션종료 #####
                SQN = amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"));

                return ResXml;
            }
            catch (Exception ex)
            {
                //### 세션종료 #####
                if (SQN > 0)
                    amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 380, 0, 0).ToErrors;
            }
        }

        #endregion "E-Ticket VOID"

        #region "PNR 큐 전송"

        /// <summary>
        /// PNR 큐 전송(Command)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PNR 큐 전송(Command)")]
        public XmlElement PNRQSendCommandRS(int SNM, string PNR)
        {
            string GUID = cm.GetGUID;
            string LogGUID = GUID;
            string SID = String.Empty;
            string SCT = String.Empty;
            int SQN = 0;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 381;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PNR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                if (!SNM.Equals(70) && !SNM.Equals(28))
                    throw new Exception("씨트립 또는 투니우 PNR만 사용 가능합니다.");
                
                //결과
                XmlElement ResXml;

                //네임스페이스
                //XmlNamespaceManager xnMgr;

                //### 01.세션생성 #####
                XmlElement Session = amd.Authenticate(67, String.Concat(GUID, "-01"));

                SID = Session.SelectSingleNode("session/sessionId").InnerText;
                SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                //### 02.PNR조회 #####
                amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Concat("RT", PNR));

                //### 03.큐 전송(CommandCryptic) #####
                ResXml = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"), String.Format("QE/{0}/{1}", AmadeusConfig.OfficeId(SNM), SNM.Equals(28) ? "55C0" : "30C0"));

                //### 04.세션종료 #####
                SQN = amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-04"));

                return ResXml;
            }
            catch (Exception ex)
            {
                //### 세션종료 #####
                if (SQN > 0)
                    amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);

                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 381, 0, 0).ToErrors;
            }
        }

        #endregion "PNR 큐 전송"

        #region "기종조회(RTW)(임시)"

        /// <summary>
        /// 기종조회(RTW)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GDS">GDS명</param>
        /// <param name="PNR">PNR번호</param>
        /// <returns></returns>
        //[WebMethod(Description = "기종조회(RTW)")]
        public XmlElement EquipmentAllSelectRTW(string EQT)
        {
            XmlElement XmlElem = null;
            XmlNamespaceManager xnMgr = null;
            string TotalString = string.Empty;
            string PrevString = string.Empty;
            bool TheEnd = false;
            int idx = 0;

            string GUID = cm.GetGUID;
            string SID = String.Empty;
            string SCT = String.Empty;
            int SQN = 0;

            try
            {
                //### 01.세션생성 #####
                XmlNode Session = amd.Authenticate(2, String.Concat(GUID, "-01"));

                SID = Session.SelectSingleNode("session/sessionId").InnerText;
                SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                //### 02.RTW #####
                XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), String.Concat("DNE ", EQT));

                xnMgr = new XmlNamespaceManager(XmlElem.OwnerDocument.NameTable);
                xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Command_Cryptic"));

                PrevString = XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText;
                TotalString = String.Concat(PrevString, Environment.NewLine, Environment.NewLine);
                idx = 3;

                //### 03.MD #####
                while (!TheEnd)
                {
                    XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-", cm.NumPosition(Convert.ToString(idx++), 2)), "MD");

                    if (PrevString.Equals(XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText))
                        TheEnd = true;
                    else
                    {
                        PrevString = XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).InnerText;
                        TotalString += String.Concat(PrevString, Environment.NewLine, Environment.NewLine);

                        if (idx > 100)
                            TheEnd = true;
                    }
                }

                //### 04.세션종료 #####
                amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-", cm.NumPosition(Convert.ToString(idx++), 2)));

                XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).RemoveAll();
                XmlElem.SelectSingleNode("m:longTextString/m:textStringDetails", xnMgr).AppendChild((XmlCDataSection)XmlElem.OwnerDocument.CreateCDataSection(TotalString));

                return XmlElem;
            }
            catch (Exception)
            {
                //### 세션종료 #####
                if (SQN > 0)
                    amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);
            }

            return XmlElem;
        }

        #endregion "기종조회(RTW)(임시)"

        #region "Entry"

        /// <summary>
        /// Entry
        /// </summary>
        /// <param name="Entry"></param>
        /// <returns></returns>
        //[WebMethod(Description = "Entry(온라인용)")]
        public XmlElement CommandCrypticRS(string Entry)
        {
            XmlElement XmlElem = null;

            string GUID = cm.GetGUID;
            string SID = String.Empty;
            string SCT = String.Empty;
            int SQN = 0;

            try
            {
                //### 01.세션생성 #####
                XmlNode Session = amd.Authenticate(2, String.Concat(GUID, "-01"));

                SID = Session.SelectSingleNode("session/sessionId").InnerText;
                SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                //### 02.ENTRY #####
                XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), Entry);

                //### 03.세션종료 #####
                amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"));

                return XmlElem;
            }
            catch (Exception)
            {
                //### 세션종료 #####
                if (SQN > 0)
                    amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);
            }

            return XmlElem;
        }

        /// <summary>
        /// Entry
        /// </summary>
        /// <param name="Entry"></param>
        /// <returns></returns>
        //[WebMethod(Description = "Entry(오프라인용)")]
        public XmlElement CommandCrypticRS2(string Entry)
        {
            XmlElement XmlElem = null;

            string GUID = cm.GetGUID;
            string SID = String.Empty;
            string SCT = String.Empty;
            int SQN = 0;

            try
            {
                //### 01.세션생성 #####
                XmlNode Session = amd.Authenticate(67, String.Concat(GUID, "-01"));

                SID = Session.SelectSingleNode("session/sessionId").InnerText;
                SCT = Session.SelectSingleNode("session/securityToken").InnerText;
                SQN = cm.RequestInt(Session.SelectSingleNode("session/sequenceNumber").InnerText, 1);

                //### 02.ENTRY #####
                XmlElem = amd.CommandCrypticRS(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-02"), Entry);

                //### 03.세션종료 #####
                amd.SignOut(SID, (++SQN).ToString(), SCT, String.Concat(GUID, "-03"));

                return XmlElem;
            }
            catch (Exception)
            {
                //### 세션종료 #####
                if (SQN > 0)
                    amd.SignOut(SID, (++SQN).ToString(), SCT, GUID);
            }

            return XmlElem;
        }

        #endregion "Entry"

        #region "메서드 설명"

        /// <summary>
		/// WebMethod의 입력 파라미터 및 출력값에 대한 설명
		/// </summary>
		/// <param name="WebMethodName">웹메서드명</param>
		/// <returns></returns>
		//[WebMethod(Description = "WebMethod의 입력 파라미터 및 출력값에 대한 설명")]
		public XmlElement Help(string WebMethodName)
		{
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 445;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = WebMethodName;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				WebMethodName = WebMethodName.Trim();
			
				XmlDocument XmlHelp = new XmlDocument();

				if (cm.FileExists(mc.HelpXmlFullPath(WebMethodName)))
					XmlHelp.Load(mc.HelpXmlFullPath(WebMethodName));
				else
					XmlHelp.LoadXml(String.Format("<HelpXml><Errors><WebMethodName>{0}</WebMethodName><Error><![CDATA[요청하신 웹메서드에 대한 설명은 제공되지 않습니다.]]></Error></Errors></HelpXml>", WebMethodName));

				return XmlHelp.DocumentElement;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 445, 0, 0).ToErrors;
			}
		}

		/// <summary>
		/// WebMethod의 Request 또는 Resonse XML에 대한 설명
		/// </summary>
		/// <param name="WebMethodName">웹메서드명</param>
		/// <param name="Gubun">구분(RQ:Request XML, RS:Response XML)</param>
		/// <returns></returns>
		//[WebMethod(Description = "WebMethod의 Request 또는 Resonse XML에 대한 설명")]
		public XmlElement HelpXml(string WebMethodName, string Gubun)
		{
            string LogGUID = cm.GetGUID;

            //파라미터 로그 기록
            try
            {
                SqlParameter[] sqlParam = new SqlParameter[] {
                        new SqlParameter("@서비스번호", SqlDbType.Int, 0),
                        new SqlParameter("@사이트번호", SqlDbType.Int, 0),
                        new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
                        new SqlParameter("@서버명", SqlDbType.VarChar, 20),
                        new SqlParameter("@메서드", SqlDbType.VarChar, 10),
                        new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50),
                        new SqlParameter("@요청1", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 446;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = WebMethodName;
                sqlParam[8].Value = Gubun;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				Gubun = Gubun.Trim().ToUpper();
				WebMethodName = WebMethodName.Trim();

				XmlDocument XmlHelp = new XmlDocument();
				string HelpXmlFileName = string.Empty;

				if (String.IsNullOrEmpty(Gubun))
					HelpXmlFileName = WebMethodName;
				else
				{
					if (cm.Right(WebMethodName, 2).Equals("RQ") || cm.Right(WebMethodName, 2).Equals("RS"))
						HelpXmlFileName = String.Format("{0}{1}", WebMethodName.Substring(0, WebMethodName.Length - 2), Gubun.ToUpper());
				}

				if (cm.FileExists(mc.RqRsXmlFullPath(HelpXmlFileName)))
					XmlHelp.Load(mc.RqRsXmlFullPath(HelpXmlFileName));
				else
					XmlHelp.LoadXml(String.Format("<HelpXml><Errors><WebMethodName>{0}</WebMethodName><Gubun>{1}</Gubun><Error><![CDATA[요청하신 XML 파일은 제공되지 않습니다.]]></Error></Errors></HelpXml>", WebMethodName, Gubun));

				return XmlHelp.DocumentElement;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "ModeService", MethodBase.GetCurrentMethod().Name, 446, 0, 0).ToErrors;
			}
		}

		#endregion "메서드 설명"
	}
}