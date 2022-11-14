using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// 제휴용 항공 웹서비스
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class AllianceService : System.Web.Services.WebService
	{
		Common cm;
		ModeConfig mc;
        AirService mas;
        AmadeusAirService amd;
        GalileoAirService gas;
        SabreAirService sas;
        AES256Cipher aes;
        LogSave log;
		HttpContext hcc;

		public AllianceService()
		{
			cm = new Common();
			mc = new ModeConfig();
            mas = new AirService();
            amd = new AmadeusAirService();
            gas = new GalileoAirService();
            sas = new SabreAirService();
            aes = new AES256Cipher();
            log = new LogSave();
			hcc = HttpContext.Current;
		}

        #region "지마켓(미사용)"

        #region "항공운임조회"

        /// <summary>
        /// [지마켓] 항공운임조회
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="OPN">오픈여부(YN)</param>
		/// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] 항공운임조회")]
		public XmlElement SearchFareListRS(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string CCD, int ADC, int CHC, int IFC)
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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청11", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 393;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SAC;
                sqlParam[8].Value = DLC;
                sqlParam[9].Value = ALC;
                sqlParam[10].Value = ROT;
                sqlParam[11].Value = DTD;
                sqlParam[12].Value = ARD;
                sqlParam[13].Value = OPN;
                sqlParam[14].Value = CCD;
                sqlParam[15].Value = ADC;
                sqlParam[16].Value = CHC;
                sqlParam[17].Value = IFC;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
			{
                XmlElement FareList = SearchFareList(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, CCD, ADC, CHC, IFC);

				if (!FareList.HasChildNodes)
				{
                    XmlElement ResXml = mas.SearchFareAvailRS(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, CCD, ADC, CHC, IFC, "100", "");
					
					if (ResXml.SelectNodes("priceInfo").Count > 0)
                        FareList = SaveAndSearchFareList(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, CCD, ADC, CHC, IFC, ResXml);
					else
						FareList = ResXml;
				}

				return FareList;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 393, 0, 0).ToErrors;
			}
		}

		/// <summary>
        /// [지마켓] 항공운임조회(DB)
		/// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="OPN">오픈여부(YN)</param>
		/// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <returns></returns>
		protected static XmlElement SearchFareList(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string CCD, int ADC, int CHC, int IFC)
		{
			XmlDocument XmlDoc = new XmlDocument();

			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
				{
					using (DataSet ds = new DataSet("RealTime_Air"))
					{
						cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSV_S_항공검색_운임리스트";

						cmd.Parameters.Add("@웹사이트번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@항공사", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@출발도시", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@도착도시", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@여정", SqlDbType.Char, 2);
                        cmd.Parameters.Add("@출발일", SqlDbType.VarChar, 40);
						cmd.Parameters.Add("@귀국일", SqlDbType.VarChar, 8);
						cmd.Parameters.Add("@귀국일미정", SqlDbType.VarChar, 4);
						cmd.Parameters.Add("@좌석등급", SqlDbType.Char, 1);
						cmd.Parameters.Add("@성인수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@소아수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@유아수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
						cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

						cmd.Parameters["@웹사이트번호"].Value = SNM;
                        cmd.Parameters["@항공사"].Value = SAC;
                        cmd.Parameters["@출발도시"].Value = DLC;
						cmd.Parameters["@도착도시"].Value = ALC;
						cmd.Parameters["@여정"].Value = ROT;
						cmd.Parameters["@출발일"].Value = DTD.Replace("-", "");
						cmd.Parameters["@귀국일"].Value = ARD.Replace("-", "");
						cmd.Parameters["@귀국일미정"].Value = OPN;
						cmd.Parameters["@좌석등급"].Value = CCD;
						cmd.Parameters["@성인수"].Value = ADC;
						cmd.Parameters["@소아수"].Value = CHC;
						cmd.Parameters["@유아수"].Value = IFC;
						cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
						cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

						adp.Fill(ds, "AirFare");

						XmlDoc.LoadXml(ds.GetXml());
						ds.Clear();
					}
				}
			}

			return XmlDoc.DocumentElement;
		}

		/// <summary>
        /// [지마켓] 항공운임정보 DB저장 후 운임리스트 출력
		/// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="OPN">오픈여부(YN)</param>
		/// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="XMLDATA">실시간 항공운임정보</param>
		/// <returns></returns>
		protected static XmlElement SaveAndSearchFareList(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string CCD, int ADC, int CHC, int IFC, XmlElement XMLDATA)
		{
			XmlDocument XmlDoc = new XmlDocument();

			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
				{
					using (DataSet ds = new DataSet("RealTime_Air"))
					{
						cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSV_T_항공검색_추가및조회";

						cmd.Parameters.Add("@웹사이트번호", SqlDbType.Int, 0);
						cmd.Parameters.Add("@항공사", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@출발도시", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@도착도시", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@여정", SqlDbType.Char, 2);
						cmd.Parameters.Add("@출발일", SqlDbType.VarChar, 40);
						cmd.Parameters.Add("@귀국일", SqlDbType.VarChar, 8);
						cmd.Parameters.Add("@귀국일미정", SqlDbType.VarChar, 4);
						cmd.Parameters.Add("@좌석등급", SqlDbType.Char, 1);
						cmd.Parameters.Add("@성인수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@소아수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@유아수", SqlDbType.Int, 0);
						cmd.Parameters.Add("@XMLDATA", SqlDbType.Xml, -1);
						cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
						cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

						cmd.Parameters["@웹사이트번호"].Value = SNM;
						cmd.Parameters["@항공사"].Value = SAC;
                        cmd.Parameters["@출발도시"].Value = DLC;
						cmd.Parameters["@도착도시"].Value = ALC;
						cmd.Parameters["@여정"].Value = ROT;
						cmd.Parameters["@출발일"].Value = DTD.Replace("-", "");
						cmd.Parameters["@귀국일"].Value = ARD.Replace("-", "");
						cmd.Parameters["@귀국일미정"].Value = OPN;
						cmd.Parameters["@좌석등급"].Value = CCD;
						cmd.Parameters["@성인수"].Value = ADC;
						cmd.Parameters["@소아수"].Value = CHC;
						cmd.Parameters["@유아수"].Value = IFC;
						cmd.Parameters["@XMLDATA"].Value = XMLDATA.OuterXml;
						cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
						cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

						adp.Fill(ds, "AirFare");

						XmlDoc.LoadXml(ds.GetXml());
						ds.Clear();
					}
				}
			}

			return XmlDoc.DocumentElement;
		}

		#endregion ""항공운임조회"

		#region "Availability조회"

		/// <summary>
        /// [지마켓] Availability조회
		/// </summary>
		/// <param name="UniqueID">항공운임 고유번호</param>
		/// <param name="FareID">항공운임번호</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] Availability조회")]
		public XmlElement SearchAvailabilityRS(string UniqueID, int FareID)
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

                sqlParam[0].Value = 391;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = UniqueID;
                sqlParam[8].Value = FareID;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
			{
				XmlDocument XmlDoc = new XmlDocument();
				XmlDoc.Load(mc.XmlFullPath("SearchAvailabilityRS"));

				XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

				XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
				XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
				XmlNode FlightGroup = FlightInfo.SelectSingleNode("flightGroup");
				XmlNode NewFlightGroup = null;
				XmlAttribute NewAVL = null;
				XmlAttribute NewRBD = null;

				XmlDocument XmlFare = null;
				int GNum = 0;
				int i = 0;

				using (SqlCommand cmd = new SqlCommand())
				{
					using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
					{
						SqlDataReader dr = null;

						cmd.Connection = conn;
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSV_S_항공검색_여정리스트";

						cmd.Parameters.Add("@고유번호", SqlDbType.Char, 32);
						cmd.Parameters.Add("@운임참조번호", SqlDbType.Int, 0);
						cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
						cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

						cmd.Parameters["@고유번호"].Value = UniqueID;
						cmd.Parameters["@운임참조번호"].Value = FareID;
						cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
						cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

						try
						{
							conn.Open();
							dr = cmd.ExecuteReader();

							if (dr.Read())
							{
								XmlFare = new XmlDocument();
								XmlFare.LoadXml(dr["운임정보"].ToString());
							}
							
							if (XmlFare.HasChildNodes)
							{
								dr.NextResult();
								
								while (dr.Read())
								{
									XmlNodeList SegFare = XmlFare.SelectNodes(String.Format("priceIndex/paxFareGroup/paxFare[1]/segFareGroup/segFare[@ref='{0}']/fare", dr["여정번호"]));
									
									if (GNum.Equals(Convert.ToInt32(dr["그룹번호"])))
									{
										XmlDocument XmlSeg = new XmlDocument();
										XmlSeg.LoadXml(dr["XMLDATA"].ToString());

										i = 0;
										foreach(XmlNode Seg in NewFlightGroup.AppendChild(XmlDoc.ImportNode(XmlSeg.SelectSingleNode("segGroup"), true)).SelectNodes("seg"))
										{
											NewAVL = XmlDoc.CreateAttribute("avl");
											NewAVL.Value = SegFare[(i)].SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText;
											Seg.Attributes.Append(NewAVL);

											NewRBD = XmlDoc.CreateAttribute("rbd");
											NewRBD.Value = SegFare[(i)].SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText;
											Seg.Attributes.Append(NewRBD);

											i++;
										}
									}
									else
									{
										if (String.IsNullOrWhiteSpace(FlightInfo.Attributes.GetNamedItem("ptc").InnerText))
											FlightInfo.Attributes.GetNamedItem("ptc").InnerText = dr["PTC"].ToString();
									
										NewFlightGroup = FlightInfo.AppendChild(FlightGroup.CloneNode(false));
										NewFlightGroup.Attributes.GetNamedItem("ref").InnerText = dr["그룹번호"].ToString();

										XmlDocument XmlSeg = new XmlDocument();
										XmlSeg.LoadXml(dr["XMLDATA"].ToString());

										i = 0;
										foreach (XmlNode Seg in NewFlightGroup.AppendChild(XmlDoc.ImportNode(XmlSeg.SelectSingleNode("segGroup"), true)).SelectNodes("seg"))
										{
											NewAVL = XmlDoc.CreateAttribute("avl");
											NewAVL.Value = SegFare[(i)].SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText;
											Seg.Attributes.Append(NewAVL);

											NewRBD = XmlDoc.CreateAttribute("rbd");
											NewRBD.Value = SegFare[(i)].SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText;
											Seg.Attributes.Append(NewRBD);

											i++;
										}

										GNum = Convert.ToInt32(dr["그룹번호"]);
									}
								}
							}

							FlightInfo.RemoveChild(FlightGroup);

							//운임정보
							XmlFare.SelectSingleNode("priceIndex").RemoveChild(XmlFare.SelectSingleNode("priceIndex/segGroup"));
							PriceInfo.AppendChild(XmlDoc.ImportNode(XmlFare.SelectSingleNode("priceIndex"), true));
						}
						finally
						{
							dr.Dispose();
							dr.Close();
							conn.Close();
						}
					}
				}

				return XmlDoc.DocumentElement;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 391, 0, 0).ToErrors;
			}
		}

		#endregion "Availability조회"

		#region "선택운임 및 여정 임시저장"

		/// <summary>
        /// [지마켓] 선택운임 및 여정 임시저장 Key 발급
		/// </summary>
		/// <param name="UniqueID">항공운임 고유번호</param>
		/// <param name="FareID">항공운임번호</param>
		/// <param name="SNM">사이트번호</param>
		/// <param name="FXL">요금조회 결과 중 선택된 <priceIndex>~</priceIndex> XmlNode(segGroup는 제외)</param>
		/// <param name="SXL">선택한 여정을 <itinerary>~<itinerary>노드에 삽입한 XML</param>
		/// <param name="RXL">요금규정 <rules>~</rules> XML</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] 선택운임 및 여정 임시저장 Key 발급")]
		public XmlElement SaveFareAvailKeyRS(string UniqueID, int FareID, int SNM, string FXL, string SXL, string RXL)
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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청42", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청43", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = 390;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = UniqueID;
                sqlParam[8].Value = FareID;
                sqlParam[9].Value = FXL;
                sqlParam[10].Value = SXL;
                sqlParam[11].Value = RXL;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            //유효성 체크
            try
            {
                string VaFXL = FXL.ToUpper();
                string VaSXL = SXL.ToUpper();
                string VaRXL = RXL.ToUpper();

                if (VaFXL.IndexOf("?XML") != -1 || VaFXL.IndexOf("!DOCTYPE") != -1)
                    throw new Exception("올바르지 않은 정보입니다.");

                if (VaSXL.IndexOf("?XML") != -1 || VaSXL.IndexOf("!DOCTYPE") != -1)
                    throw new Exception("올바르지 않은 정보입니다.");

                if (VaRXL.IndexOf("?XML") != -1 || VaRXL.IndexOf("!DOCTYPE") != -1)
                    throw new Exception("올바르지 않은 정보입니다.");
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 390, 0, 0).ToErrors;
            }
            
            try
			{
				using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
				{
					SqlCommand cmd = new SqlCommand
					{
						Connection = conn,
						CommandTimeout = 60,
						CommandType = CommandType.StoredProcedure,
						CommandText = "DBO.WSV_T_항공검색_선택정보저장"
					};

					cmd.Parameters.Add("@항공운임고유번호", SqlDbType.Char, 32);
					cmd.Parameters.Add("@운임참조번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@요금XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@여정XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@규정XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@고유번호", SqlDbType.Char, 32);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@항공운임고유번호"].Value = UniqueID;
					cmd.Parameters["@운임참조번호"].Value = FareID;
					cmd.Parameters["@사이트번호"].Value = SNM;
					cmd.Parameters["@요금XML"].Value = (!String.IsNullOrWhiteSpace(FXL)) ? FXL : Convert.DBNull;
					cmd.Parameters["@여정XML"].Value = (!String.IsNullOrWhiteSpace(SXL)) ? SXL : Convert.DBNull;
					cmd.Parameters["@규정XML"].Value = (!String.IsNullOrWhiteSpace(RXL)) ? RXL : Convert.DBNull;
					cmd.Parameters["@고유번호"].Direction = ParameterDirection.Output;
					cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
					cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

					try
					{
						conn.Open();
						cmd.ExecuteNonQuery();

						if (cmd.Parameters["@결과"].Value.ToString().Equals("S"))
						{
							XmlDocument XmlDoc = new XmlDocument();
							XmlDoc.Load(mc.XmlFullPath("SaveFareAvailKeyRS"));

							XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
							XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/siteNo").InnerText = SNM.ToString();
							XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/uniqueKey").InnerText = cmd.Parameters["@고유번호"].Value.ToString();
							XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/saveDate").InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

							return XmlDoc.DocumentElement;
						}
						else
						{
							throw new Exception(cmd.Parameters["@에러메시지"].Value.ToString());
						}
					}
					catch (Exception ex)
					{
						throw new Exception(ex.Message);
					}
					finally
					{
						conn.Close();
					}
				}
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 390, 0, 0).ToErrors;
			}
		}

		/// <summary>
        /// [지마켓] 선택운임 및 여정 임시저장 정보 호출
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="UKey">선택운임 및 여정정보 임시저장 Key</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] 선택운임 및 여정 임시저장 정보 호출")]
		public XmlElement SaveFareAvailInfoRS(int SNM, string UKey)
		{
            string LogGUID = cm.GetGUID;
            string UniqueID = string.Empty;
			string FareID = string.Empty;
			string FXL = string.Empty;
			string SXL = string.Empty;
			string RXL = string.Empty;

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

                sqlParam[0].Value = 389;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = UKey;

                log.LogDBSave(sqlParam);
            }
            finally { }

			try
			{
				using (SqlCommand cmd = new SqlCommand())
				{
					using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
					{
						SqlDataReader dr = null;

						cmd.Connection = conn;
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSV_S_항공검색_선택정보";

						cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
						cmd.Parameters.Add("@고유번호", SqlDbType.Char, 32);
						cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
						cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

						cmd.Parameters["@사이트번호"].Value = SNM;
						cmd.Parameters["@고유번호"].Value = UKey;
						cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
						cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

						try
						{
							conn.Open();
							dr = cmd.ExecuteReader();

							if (dr.Read())
							{
								UniqueID = dr["항공운임고유번호"].ToString();
								FareID = dr["운임참조번호"].ToString();
								SNM = Convert.ToInt32(dr["사이트번호"]);
								FXL = dr["요금XML"].ToString();
								SXL = dr["여정XML"].ToString();
								RXL = dr["규정XML"].ToString();
							}
						}
						finally
						{
							dr.Dispose();
							dr.Close();
							conn.Close();
						}
					}

					if (cmd.Parameters["@결과"].Value.ToString().Equals("S"))
					{
						XmlDocument XmlDoc = new XmlDocument();
						XmlDoc.Load(mc.XmlFullPath("SaveFareAvailInfoRS"));

						XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
						XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/siteNo").InnerText = SNM.ToString();
						XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/uniqueId").InnerText = UniqueID;
						XmlDoc.SelectSingleNode("ResponseDetails/selectInfo/fareId").InnerText = FareID;

						if (!String.IsNullOrWhiteSpace(FXL))
						{
							XmlDocument XmlFxl = new XmlDocument();
							XmlFxl.LoadXml(FXL);

							XmlFxl.SelectSingleNode("priceIndex").RemoveChild(XmlFxl.SelectSingleNode("priceIndex/segGroup"));
							XmlDoc.SelectSingleNode("ResponseDetails").ReplaceChild(XmlDoc.ImportNode(XmlFxl.FirstChild, true), XmlDoc.SelectSingleNode("ResponseDetails/priceIndex"));
						}

						if (!String.IsNullOrWhiteSpace(SXL))
						{
							XmlDocument XmlSxl = new XmlDocument();
							XmlSxl.LoadXml(SXL);

							XmlDoc.SelectSingleNode("ResponseDetails").ReplaceChild(XmlDoc.ImportNode(XmlSxl.FirstChild, true), XmlDoc.SelectSingleNode("ResponseDetails/itinerary"));
						}

						if (!String.IsNullOrWhiteSpace(RXL))
						{
							XmlDocument XmlRxl = new XmlDocument();
							XmlRxl.LoadXml(RXL);

							XmlDoc.SelectSingleNode("ResponseDetails").ReplaceChild(XmlDoc.ImportNode(XmlRxl.FirstChild, true), XmlDoc.SelectSingleNode("ResponseDetails/rules"));
						}

						return XmlDoc.DocumentElement;
					}
					else
					{
						throw new Exception(cmd.Parameters["@에러메시지"].Value.ToString());
					}
				}
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 389, 0, 0).ToErrors;
			}
		}

		#endregion "선택운임 및 여정 임시저장"

		#region "예약"

		/// <summary>
        /// [지마켓] 항공임시예약번호 발급
		/// </summary>
		/// <param name="PID">탑승객 PTID</param>
		/// <param name="PTC">탑승객 타입 코드 (ADT/CHD/INF/STU/LBR..)</param>
		/// <param name="PTL">탑승객 타이틀 (MR/MRS/MS/MSTR/MISS)</param>
		/// <param name="PHN">탑승객 한글이름</param>
		/// <param name="PSN">탑승객 영문성 (SurName)</param>
		/// <param name="PFN">탑승객 영문이름 (First Name)</param>
		/// <param name="PBD">탑승객 생년월일 (YYYYMMDD) (소아,유아일 경우 필수)</param>
		/// <param name="PTN">탑승객 전화번호</param>
		/// <param name="PMN">탑승객 휴대폰</param>
        /// <param name="PEA">탑승객 이메일주소</param>
        /// <param name="PMC">탑승객 회원구분 (01:정회원, 02:웹회원, 03:투어마일리지회원)</param>
        /// <param name="PMT">탑승객 투어마일리지 카드번호</param>
        /// <param name="PMR">탑승객 투어마일리지 적립 요청여부</param>
		/// <param name="RID">예약자 PTID</param>
		/// <param name="RTL">예약자 타이틀 (MR/MS)</param>
		/// <param name="RHN">예약자 한글이름</param>
		/// <param name="RSN">예약자 영문성 (SurName)</param>
		/// <param name="RFN">예약자 영문이름 (First Name)</param>
		/// <param name="RDB">예약자 생년월일(YYYY-MM-DD)</param>
		/// <param name="RGD">예약자 성별(M:남성, F:여성)</param>
		/// <param name="RLF">예약자 내/외국인 여부(L:내국인, F:외국인)</param>
		/// <param name="RTN">예약자 전화번호</param>
		/// <param name="RMN">예약자 휴대폰</param>
		/// <param name="REA">예약자 이메일주소</param>
		/// <param name="RMK">추가요청사항</param>
		/// <param name="RQT">요청단말기(WEB/MOBILE/CRS/MODEWARE)</param>
		/// <param name="RQU">요청URL</param>
		/// <param name="SNM">사이트번호</param>
		/// <param name="ANM">거래처번호</param>
		/// <param name="AEN">거래처직원번호</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="OPN">오픈여부(YN)</param>
		/// <param name="FXL">요금조회 결과 중 선택된 <priceIndex>~</priceIndex> XmlNode(segGroup는 제외)</param>
		/// <param name="SXL">선택한 여정을 <itinerary>~<itinerary>노드에 삽입한 XML</param>
		/// <param name="RXL">요금규정 <rules>~</rules> XML</param>
		/// <param name="DXL">할인항공일 경우 선택된 <fare>~</fare> XML</param>
        /// <param name="AKY">제휴정보(네이로:NV|아이디)</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] 항공임시예약번호 발급")]
        public XmlElement AddBookingKeyRS(int[] PID, string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string[] PMC, string[] PMT, string[] PMR, int RID, string RTL, string RHN, string RSN, string RFN, string RDB, string RGD, string RLF, string RTN, string RMN, string REA, string RMK, string RQT, string RQU, int SNM, int ANM, int AEN, string ROT, string OPN, string FXL, string SXL, string RXL, string DXL, string AKY)
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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청11", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청12", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청13", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청14", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청15", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청16", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청17", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청18", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청19", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청20", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청21", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청22", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청23", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청24", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청25", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청26", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청27", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청28", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청29", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청30", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청42", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청43", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청44", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청31", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 384;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = String.Join("^", PID);
                sqlParam[8].Value = String.Join("^", PTC);
                sqlParam[9].Value = String.Join("^", PTL);
                sqlParam[10].Value = String.Join("^", PHN);
                sqlParam[11].Value = String.Join("^", PSN);
                sqlParam[12].Value = String.Join("^", PFN);
                sqlParam[13].Value = String.Join("^", PBD);
                sqlParam[14].Value = String.Join("^", PTN);
                sqlParam[15].Value = String.Join("^", PMN);
                sqlParam[16].Value = String.Join("^", PEA);
                sqlParam[17].Value = String.Join("^", PMC);
                sqlParam[18].Value = String.Join("^", PMT);
                sqlParam[19].Value = String.Join("^", PMR);
                sqlParam[20].Value = RID;
                sqlParam[21].Value = RTL;
                sqlParam[22].Value = RHN;
                sqlParam[23].Value = RSN;
                sqlParam[24].Value = RFN;
                sqlParam[25].Value = RDB;
                sqlParam[26].Value = RGD;
                sqlParam[27].Value = RLF;
                sqlParam[28].Value = RTN;
                sqlParam[29].Value = RMN;
                sqlParam[30].Value = REA;
                sqlParam[31].Value = RMK;
                sqlParam[32].Value = RQU;
                sqlParam[33].Value = ANM;
                sqlParam[34].Value = AEN;
                sqlParam[35].Value = ROT;
                sqlParam[36].Value = OPN;
                sqlParam[37].Value = FXL;
                sqlParam[38].Value = SXL;
                sqlParam[39].Value = RXL;
                sqlParam[40].Value = DXL;
                sqlParam[41].Value = AKY;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            try
			{
				using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
				{
					SqlCommand cmd = new SqlCommand
					{
						Connection = conn,
						CommandTimeout = 60,
						CommandType = CommandType.StoredProcedure,
						CommandText = "DBO.WSV_T_항공예약임시저장"
					};

					cmd.Parameters.Add("@탑승객PTID", SqlDbType.VarChar, 200);
					cmd.Parameters.Add("@탑승객타입코드", SqlDbType.VarChar, 200);
					cmd.Parameters.Add("@탑승객타이틀", SqlDbType.VarChar, 200);
					cmd.Parameters.Add("@탑승객한글이름", SqlDbType.NVarChar, 500);
					cmd.Parameters.Add("@탑승객영문성", SqlDbType.VarChar, 500);
					cmd.Parameters.Add("@탑승객영문이름", SqlDbType.VarChar, 500);
					cmd.Parameters.Add("@탑승객생년월일", SqlDbType.VarChar, 300);
					cmd.Parameters.Add("@탑승객전화번호", SqlDbType.VarChar, 300);
					cmd.Parameters.Add("@탑승객휴대폰", SqlDbType.VarChar, 300);
					cmd.Parameters.Add("@탑승객이메일주소", SqlDbType.VarChar, 500);
                    cmd.Parameters.Add("@탑승객회원구분", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("@탑승객마일리지번호", SqlDbType.VarChar, 300);
                    cmd.Parameters.Add("@탑승객마일리지적립요청", SqlDbType.VarChar, 50);
					cmd.Parameters.Add("@예약자PTID", SqlDbType.Int, 0);
					cmd.Parameters.Add("@예약자타이틀", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@예약자한글이름", SqlDbType.NVarChar, 100);
					cmd.Parameters.Add("@예약자영문성", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@예약자영문이름", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@예약자생년월일", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@예약자성별", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@예약자내외국인여부", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@예약자전화번호", SqlDbType.VarChar, 30);
					cmd.Parameters.Add("@예약자휴대폰", SqlDbType.VarChar, 30);
					cmd.Parameters.Add("@예약자이메일주소", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@추가요청사항", SqlDbType.NVarChar, -1);
					cmd.Parameters.Add("@요청단말기", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@요청URL", SqlDbType.VarChar, 500);
					cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@거래처번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@거래처직원번호", SqlDbType.Int, 0);
					cmd.Parameters.Add("@구간", SqlDbType.Char, 2);
                    cmd.Parameters.Add("@오픈여부", SqlDbType.VarChar, 10);
					cmd.Parameters.Add("@요금XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@여정XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@규정XML", SqlDbType.Xml, -1);
					cmd.Parameters.Add("@할인요금XML", SqlDbType.Xml, -1);
                    cmd.Parameters.Add("@제휴정보", SqlDbType.VarChar, 100);
					cmd.Parameters.Add("@고유번호", SqlDbType.Char, 32);
					cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
					cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

					cmd.Parameters["@탑승객PTID"].Value = cm.ConvertStringToArrInteger(PID, "|");
					cmd.Parameters["@탑승객타입코드"].Value = cm.ConvertStringToArrString(PTC, "|");
					cmd.Parameters["@탑승객타이틀"].Value = cm.ConvertStringToArrString(PTL, "|");
					cmd.Parameters["@탑승객한글이름"].Value = cm.ConvertStringToArrString(PHN, "|");
					cmd.Parameters["@탑승객영문성"].Value = cm.ConvertStringToArrString(PSN, "|");
					cmd.Parameters["@탑승객영문이름"].Value = cm.ConvertStringToArrString(PFN, "|");
					cmd.Parameters["@탑승객생년월일"].Value = cm.ConvertStringToArrString(PBD, "|");
					cmd.Parameters["@탑승객전화번호"].Value = cm.ConvertStringToArrString(PTN, "|");
					cmd.Parameters["@탑승객휴대폰"].Value = cm.ConvertStringToArrString(PMN, "|");
					cmd.Parameters["@탑승객이메일주소"].Value = cm.ConvertStringToArrString(PEA, "|");
                    cmd.Parameters["@탑승객회원구분"].Value = cm.ConvertStringToArrString(PMC, "|");
                    cmd.Parameters["@탑승객마일리지번호"].Value = cm.ConvertStringToArrString(PMT, "|");
                    cmd.Parameters["@탑승객마일리지적립요청"].Value = cm.ConvertStringToArrString(PMR, "|");
					cmd.Parameters["@예약자PTID"].Value = RID;
					cmd.Parameters["@예약자타이틀"].Value = RTL;
					cmd.Parameters["@예약자한글이름"].Value = RHN;
					cmd.Parameters["@예약자영문성"].Value = RSN;
					cmd.Parameters["@예약자영문이름"].Value = RFN;
					cmd.Parameters["@예약자생년월일"].Value = RDB;
					cmd.Parameters["@예약자성별"].Value = RGD;
					cmd.Parameters["@예약자내외국인여부"].Value = RLF;
					cmd.Parameters["@예약자전화번호"].Value = RTN;
					cmd.Parameters["@예약자휴대폰"].Value = RMN;
					cmd.Parameters["@예약자이메일주소"].Value = REA;
					cmd.Parameters["@추가요청사항"].Value = RMK.Replace("'", "''");
					cmd.Parameters["@요청단말기"].Value = RQT;
					cmd.Parameters["@요청URL"].Value = RQU;
					cmd.Parameters["@사이트번호"].Value = SNM;
					cmd.Parameters["@거래처번호"].Value = ANM;
					cmd.Parameters["@거래처직원번호"].Value = AEN;
					cmd.Parameters["@구간"].Value = ROT;
                    cmd.Parameters["@오픈여부"].Value = OPN;
					cmd.Parameters["@요금XML"].Value = (!String.IsNullOrWhiteSpace(FXL)) ? FXL : Convert.DBNull;
					cmd.Parameters["@여정XML"].Value = (!String.IsNullOrWhiteSpace(SXL)) ? SXL : Convert.DBNull;
					cmd.Parameters["@규정XML"].Value = (!String.IsNullOrWhiteSpace(RXL)) ? RXL : Convert.DBNull;
					cmd.Parameters["@할인요금XML"].Value = (!String.IsNullOrWhiteSpace(DXL)) ? DXL : Convert.DBNull;
                    cmd.Parameters["@제휴정보"].Value = AKY;
					cmd.Parameters["@고유번호"].Direction = ParameterDirection.Output;
					cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
					cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

					try
					{
						conn.Open();
						cmd.ExecuteNonQuery();

						if (cmd.Parameters["@결과"].Value.ToString().Equals("S"))
						{
							XmlDocument XmlDoc = new XmlDocument();
							XmlDoc.Load(mc.XmlFullPath("AddBookingKeyRS"));
				
							XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
							XmlDoc.SelectSingleNode("ResponseDetails/bookingInfo/siteNo").InnerText = SNM.ToString();
							XmlDoc.SelectSingleNode("ResponseDetails/bookingInfo/uniqueKey").InnerText = cmd.Parameters["@고유번호"].Value.ToString();
							XmlDoc.SelectSingleNode("ResponseDetails/bookingInfo/bookingCreationDate").InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

							return XmlDoc.DocumentElement;
						}
						else
						{
							throw new Exception(cmd.Parameters["@에러메시지"].Value.ToString());
						}
					}
					catch (Exception ex)
					{
						throw new Exception(ex.Message);
					}
					finally
					{
						conn.Close();
					}
				}
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 384, 0, 0).ToErrors;
			}
		}

		/// <summary>
        /// [지마켓] 항공예약
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="UKey">임시예약번호</param>
		/// <returns></returns>
        [WebMethod(Description = "[지마켓] 항공예약")]
		public XmlElement AddBookingRS(int SNM, string UKey)
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

                sqlParam[0].Value = 385;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = UKey;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            string[] StrPID = null;
			string[] PTC = null;
			string[] PTL = null;
			string[] PHN = null;
			string[] PSN = null;
			string[] PFN = null;
			string[] PBD = null;
			string[] PTN = null;
			string[] PMN = null;
            string[] PEA = null;
            string[] PMC = null;
            string[] PMT = null;
            string[] PMR = null;

			int RID = 0;
			string RTL = string.Empty;
			string RHN = string.Empty;
			string RSN = string.Empty;
			string RFN = string.Empty;
			string RDB = string.Empty;
			string RGD = string.Empty;
			string RLF = string.Empty;
			string RTN = string.Empty;
			string RMN = string.Empty;
			string REA = string.Empty;
			string RMK = string.Empty;
			string RQT = string.Empty;
			string RQU = string.Empty;
			int ANM = 0;
			int AEN = 0;
            string ROT = string.Empty;
            string OPN = string.Empty;
			string FXL = string.Empty;
			string SXL = string.Empty;
			string RXL = string.Empty;
			string DXL = string.Empty;
            string AKY = string.Empty;
			
			try
			{
				using (SqlCommand cmd = new SqlCommand())
				{
					using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
					{
						SqlDataReader dr = null;

						cmd.Connection = conn;
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSV_S_항공예약임시저장";

						cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
						cmd.Parameters.Add("@고유번호", SqlDbType.Char, 32);
						cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
						cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

						cmd.Parameters["@사이트번호"].Value = SNM;
						cmd.Parameters["@고유번호"].Value = UKey;
						cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
						cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

						try
						{
							conn.Open();
							dr = cmd.ExecuteReader();

							if (dr.Read())
							{
								StrPID = dr["탑승객PTID"].ToString().Split('|');
								PTC = dr["탑승객타입코드"].ToString().Split('|');
								PTL = dr["탑승객타이틀"].ToString().Split('|');
								PHN = dr["탑승객한글이름"].ToString().Split('|');
								PSN = dr["탑승객영문성"].ToString().Split('|');
								PFN = dr["탑승객영문이름"].ToString().Split('|');
								PBD = dr["탑승객생년월일"].ToString().Split('|');
								PTN = dr["탑승객전화번호"].ToString().Split('|');
								PMN = dr["탑승객휴대폰"].ToString().Split('|');
								PEA = dr["탑승객이메일주소"].ToString().Split('|');
                                PMC = dr["탑승객회원구분"].ToString().Split('|');
                                PMT = dr["탑승객마일리지번호"].ToString().Split('|');
                                PMR = dr["탑승객마일리지적립요청"].ToString().Split('|');
                                RID = Convert.ToInt32(dr["예약자PTID"]);
								RTL = dr["예약자타이틀"].ToString();
								RHN = dr["예약자한글이름"].ToString();
								RSN = dr["예약자영문성"].ToString();
								RFN = dr["예약자영문이름"].ToString();
								RDB = dr["예약자생년월일"].ToString();
								RGD = dr["예약자성별"].ToString();
								RLF = dr["예약자내외국인여부"].ToString();
								RTN = dr["예약자전화번호"].ToString();
								RMN = dr["예약자휴대폰"].ToString();
								REA = dr["예약자이메일주소"].ToString();
								RMK = dr["추가요청사항"].ToString();
								RQT = dr["요청단말기"].ToString();
								RQU = dr["요청URL"].ToString();
								SNM = Convert.ToInt32(dr["사이트번호"]);
								ANM = Convert.ToInt32(dr["거래처번호"]);
								AEN = Convert.ToInt32(dr["거래처직원번호"]);
                                ROT = dr["구간"].ToString();
                                OPN = dr["오픈여부"].ToString();
								FXL = dr["요금XML"].ToString();
								SXL = dr["여정XML"].ToString();
								RXL = dr["규정XML"].ToString();
								DXL = dr["할인요금XML"].ToString();
                                AKY = dr["제휴정보"].ToString();
							}
						}
						finally
						{
							dr.Dispose();
							dr.Close();
							conn.Close();
						}
					}

					if (cmd.Parameters["@결과"].Value.ToString().Equals("S"))
					{
						int[] PID = new Int32[StrPID.Length];

						for (int i = 0; i < PID.Length; i++)
							PID[i] = cm.RequestInt(StrPID[i]);

                        return mas.AddBookingRS(PID, PTC, PTL, PHN, PSN, PFN, PBD, PTN, PMN, PEA, PMC, PMT, PMR, RID, RTL, RHN, RSN, RFN, RDB, RGD, RLF, RTN, RMN, REA, RMK, RQT, RQU, SNM, ANM, AEN, ROT, OPN, FXL, SXL, RXL, DXL, AKY, "Y", "", "");
					}
					else
					{
						throw new Exception(cmd.Parameters["@에러메시지"].Value.ToString());
					}
				}
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 385, 0, 0).ToErrors;
			}
		}

		#endregion "예약"

        #endregion "지마켓"

        #region "네이버"

        //[WebMethod(Description = "갈릴레오 복호화")]
        public string GalileoDec(string Enc)
        {
            return cm.GalileoDeCompression(Enc);
        }

        #region "네이버(아마데우스)"

        /// <summary>
        /// [네이버] Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="DTT">출발시간(HHMM)</param>
        /// <param name="DLC">출발지</param>
        /// <param name="ALC">도착지</param>
        /// <param name="CLC">경유지</param>
        /// <param name="SCD">서비스클래스</param>
        /// <param name="MCC">항공사</param>
        /// <param name="FLN">편명</param>
        /// <param name="NOS">좌석수</param>
        /// <returns></returns>
        [WebMethod(Description = "[네이버] Availability 조회")]
        public XmlElement AvailabilityRS(int SNM, string DTD, string DTT, string DLC, string ALC, string CLC, string SCD, string MCC, string FLN, string NOS)
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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 386;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = DTD;
                sqlParam[8].Value = DTT;
                sqlParam[9].Value = DLC;
                sqlParam[10].Value = ALC;
                sqlParam[11].Value = CLC;
                sqlParam[12].Value = SCD;
                sqlParam[13].Value = MCC;
                sqlParam[14].Value = FLN;
                sqlParam[15].Value = NOS;

                log.LogDBSave(sqlParam);
            }
            finally { }
            
            return mas.AvailabilityRS(SNM, DTD, DTT, DLC, ALC, Regex.Replace(CLC, "(/1[A-Z]{3})|(1[A-Z]{3}/)|(1[A-Z]{3})", ""), SCD.Replace("+", ","), MCC, FLN, NOS);
        }

        /// <summary>
        /// [네이버] 오픈마켓용 운임정보 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GoodCode">상품코드</param>
        /// <returns></returns>
        [WebMethod(Description = "[네이버] 오픈마켓용 운임정보 조회")]
        public XmlElement SearchOpenMarketSingleFareRS(int SNM, string GoodCode)
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

                sqlParam[0].Value = 394;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = GoodCode;

                log.LogDBSave(sqlParam);
            }
            finally { }

            try
            {
                XmlDocument XmlDoc = new XmlDocument();

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet("fareList"))
                        {
                            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
                            cmd.CommandTimeout = 60;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSP_R_네이버국제선";

                            cmd.Parameters.Add("@GOODCODE", SqlDbType.NVarChar, 50);
                            cmd.Parameters["@GOODCODE"].Value = GoodCode;

                            adp.Fill(ds, "fare");

                            XmlDoc.LoadXml(ds.GetXml().Replace(" xml:space=\"preserve\"", ""));
                            ds.Clear();
                        }
                    }
                }

                return XmlDoc.DocumentElement;
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 394, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// [네이버] Fare + Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호(4638)</param>
        /// <param name="SAC">항공사 코드 [NAIRV]</param>
        /// <param name="SCity">출발지 공항 코드 [SCITY1/SCITY2/SCITY3]</param>
        /// <param name="ECity">도착지 공항 코드 [ECITY1/ECITY2/ECITY3]</param>
        /// <param name="SDate">출발일(YYYYMMDD) [SDATE1/SDATE2/SDATE3]</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복) [TRIP]</param>
        /// <param name="GoodCode">상품코드 [SGC/RGC]</param>
        /// <param name="EventCode">이벤트(프로모션)코드 [EventNum]</param>
        /// <param name="PromtionCode">네이버할인코드 [PromotionCode]</param>
        /// <param name="PromtionName">네이버할인명 [PromotionName]</param>
        /// <param name="PromtionAmount">네이버할인금액 [PromotionAmt/PromotionAdtInd/PromotionChdInd/PromotionInfInd]</param>
        /// <param name="Itinerary">여정정보 [Itinerary1/Itinerary2]</param>
        /// <param name="TaxInfo">텍스정보 [TaxInfo]</param>
        /// <param name="NaverFareJoin">결합조건코드 [NaverFareJoin]</param>
        /// <param name="ADC">성인수 [ADT]</param>
        /// <param name="CHC">소아수 [CHD]</param>
        /// <param name="IFC">유아수 [INF]</param>
        /// <returns></returns>
        //[WebMethod(Description = "[네이버] Fare + Availability 조회")]
        public XmlElement SearchFareAvailforNaverRSXXX(int SNM, string SAC, string SCity, string ECity, string SDate, string ROT, string GoodCode, string EventCode, string PromtionCode, string PromtionName, string PromtionAmount, string Itinerary, string TaxInfo, string NaverFareJoin, int ADC, int CHC, int IFC)
        {
            string GUID = cm.GetGUID;

            //파라미터 로그 기록
            //try
            //{
            //    SqlParameter[] sqlParam = new SqlParameter[] {
            //            new SqlParameter("@서비스번호", SqlDbType.Int, 0),
            //            new SqlParameter("@사이트번호", SqlDbType.Int, 0),
            //            new SqlParameter("@요청단말기", SqlDbType.VarChar, 30),
            //            new SqlParameter("@서버명", SqlDbType.VarChar, 20),
            //            new SqlParameter("@메서드", SqlDbType.VarChar, 10),
            //            new SqlParameter("@사용자IP", SqlDbType.VarChar, 30),
            //            new SqlParameter("@요청1", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청2", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청3", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청4", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청5", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청6", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청7", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청8", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청9", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청10", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청11", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청12", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청13", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청14", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청15", SqlDbType.VarChar, 8000),
            //            new SqlParameter("@요청16", SqlDbType.VarChar, 8000)
            //        };

            //    sqlParam[0].Value = 392;
            //    sqlParam[1].Value = SNM;
            //    sqlParam[2].Value = "";
            //    sqlParam[3].Value = Environment.MachineName;
            //    sqlParam[4].Value = hcc.Request.HttpMethod;
            //    sqlParam[5].Value = hcc.Request.UserHostAddress;
            //    sqlParam[6].Value = SAC;
            //    sqlParam[7].Value = SCity;
            //    sqlParam[8].Value = ECity;
            //    sqlParam[9].Value = SDate;
            //    sqlParam[10].Value = ROT;
            //    sqlParam[11].Value = GoodCode;
            //    sqlParam[12].Value = EventCode;
            //    sqlParam[13].Value = PromtionCode;
            //    sqlParam[14].Value = PromtionName;
            //    sqlParam[15].Value = PromtionAmount;
            //    sqlParam[16].Value = Itinerary;
            //    sqlParam[17].Value = TaxInfo;
            //    sqlParam[18].Value = NaverFareJoin;
            //    sqlParam[19].Value = ADC;
            //    sqlParam[20].Value = CHC;
            //    sqlParam[21].Value = IFC;

            //    dgLogDBSave dg = new dgLogDBSave(log.LogDBSave);
            //    dg.BeginInvoke(sqlParam, null, null);
            //}
            //finally { }

            try
            {
                XmlDocument XmlTmp = new XmlDocument();
                string Xml = string.Empty;

                Xml += "<SearchFareAvailforNaverRQ>";
                Xml += String.Format("<SNM>{0}</SNM>", SNM);
                Xml += String.Format("<SAC>{0}</SAC>", SAC);
                Xml += String.Format("<SCity>{0}</SCity>", SCity);
                Xml += String.Format("<ECity>{0}</ECity>", ECity);
                Xml += String.Format("<SDate>{0}</SDate>", SDate);
                Xml += String.Format("<ROT>{0}</ROT>", ROT);
                Xml += String.Format("<GoodCode>{0}</GoodCode>", GoodCode);
                Xml += String.Format("<EventCode>{0}</EventCode>", EventCode);
                Xml += String.Format("<PromotionCode>{0}</PromotionCode>", PromtionCode);
                Xml += String.Format("<PromotionName>{0}</PromotionName>", PromtionName);
                Xml += String.Format("<PromotionAmount>{0}</PromotionAmount>", PromtionAmount);
                Xml += String.Format("<Itinerary><![CDATA[{0}]]></Itinerary>", Itinerary);
                Xml += String.Format("<TaxInfo>{0}</TaxInfo>", TaxInfo);
                Xml += String.Format("<NaverFareJoin>{0}</NaverFareJoin>", NaverFareJoin);
                Xml += String.Format("<ADC>{0}</ADC>", ADC);
                Xml += String.Format("<CHC>{0}</CHC>", CHC);
                Xml += String.Format("<IFC>{0}</IFC>", IFC);
                Xml += "</SearchFareAvailforNaverRQ>";

                XmlTmp.LoadXml(Xml);
                cm.XmlFileSave(XmlTmp, "Naver", "SearchFareAvailforNaverRQ", "N", GUID);
            }
            finally { }
            
            try
            {
                if (!ROT.Equals("OW") && !ROT.Equals("RT"))
                    throw new Exception("(001) 편도 및 왕복일 경우에만 예약이 가능합니다.");

                string[] ArrSCity = SCity.Split('/');
                string[] ArrECity = ECity.Split('/');
                string[] ArrSDate = SDate.Split('/');
                string[] ArrGoodCode = GoodCode.Split('/');
                string[] ArrEventCode = EventCode.Split('/');
                string[] ArrItinerary = Itinerary.Split('/');
                string[] ArrTax = TaxInfo.Split('/');
                string[] ArrPromtionAmount = PromtionAmount.Split('/');

                if (!String.IsNullOrWhiteSpace(ArrSCity[0]) && !String.IsNullOrWhiteSpace(ArrECity[0]) && !String.IsNullOrWhiteSpace(ArrSDate[0]) && !String.IsNullOrWhiteSpace(ArrGoodCode[0]) && !String.IsNullOrWhiteSpace(ArrItinerary[0]))
                {
                    if (cm.DateDiff("d", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), cm.RequestDateTime(ArrSDate[0])) < 1)
                        throw new Exception("(012) 최소 2일 이후 출발인 경우에만 예약이 가능합니다.");

                    XmlElement FareSXml = SearchOpenMarketSingleFareRS(SNM, ArrGoodCode[0].Trim());
                    XmlElement FareEXml = null;
                    XmlElement FareXml = FareSXml;
                    
                    if (ArrGoodCode.Length > 1 && !String.IsNullOrWhiteSpace(ArrGoodCode[1].Trim()) && ArrGoodCode[0].Trim() != ArrGoodCode[1].Trim())
                    {
                        FareEXml = SearchOpenMarketSingleFareRS(SNM, ArrGoodCode[1].Trim());

                        //첫번째 운임정보 중 귀국편에 대한 정보 삭제
                        foreach (XmlNode Fare in FareXml.SelectNodes("fare[Way='R']"))
                            FareXml.RemoveChild(Fare);

                        //두번째 운임정보 중 귀국편에 대한 정보만 추가
                        foreach (XmlNode Fare in FareEXml.SelectNodes("fare[Way='R']"))
                            FareXml.AppendChild(FareXml.OwnerDocument.ImportNode(Fare, true));
                    }
                    
                    if (FareXml.SelectNodes("fare").Count > 0)
                    {
                        if (ROT != ((FareXml.SelectSingleNode("fare/Trip").InnerText).Equals("2") ? "OW" : "RT"))
                            throw new Exception("(002) 운임정보와 여정타입이 일치하지 않습니다.");

                        if (SAC != FareXml.SelectSingleNode("fare/AirV").InnerText)
                            throw new Exception("(003) 운임정보와 항공사정보가 일치하지 않습니다.");

                        if (ROT.Equals("RT") && ArrItinerary.Length < 2)
                            throw new Exception("(004) 스케쥴 정보가 정확하지 않습니다.");

                        int i = 0;
                        string[] RtgInfo = new String[ArrItinerary.Length];
                        string[] RtgClass = new String[ArrItinerary.Length];
                        string[] RtgStop = new String[ArrItinerary.Length];

                        foreach (string SegGroup in ArrItinerary)
                        {
                            RtgInfo[i] = "";
                            RtgClass[i] = "";
                            RtgStop[i] = "";

                            string[] SegInfos = SegGroup.Split(',');
                            int SegInfosLen = SegInfos.Length - 1;

                            for (int n = 0; n < SegInfosLen; n++)
                            {
                                string[] SegInfo = SegInfos[n].Split('^');
                                double DateChangeCode = 0;

                                //0:출발지^
                                //1:도착지^
                                //2:출발지이름^
                                //3:도착지이름^
                                //4:출발일^
                                //5:도착일^
                                //6:출발시간^
                                //7:도착시간^
                                //8:출발터미널^
                                //9:도착터미널^
                                //10:항공사^
                                //11:편명^
                                //12:클래스^
                                //13:비행시간^
                                //14:좌석수^
                                //15:날짜변경여부^
                                //16:중간스탑수^
                                //17:공동운항 항공사^
                                //18:숨은 경유지^
                                //19:중간 체류시간,

                                if (!String.IsNullOrWhiteSpace(RtgInfo[i]))
                                {
                                    RtgInfo[i] += "+";
                                    RtgClass[i] += "+";
                                }

                                if (SegInfosLen > (n + 1))
                                {
                                    string[] SegInfo2 = SegInfos[(n + 1)].Split('^');
                                    
                                    if (SegInfo[5].Equals(SegInfo2[4]))
                                        DateChangeCode = SegInfo[4].Equals(SegInfo[5]) ? 0 : cm.DateDiff("d", cm.RequestDateTime(SegInfo[4]), cm.RequestDateTime(SegInfo[5]));
                                    else
                                        DateChangeCode = SegInfo[4].Equals(SegInfo[5]) ? 1 : (cm.DateDiff("d", cm.RequestDateTime(SegInfo[4]), cm.RequestDateTime(SegInfo[5])) + 1);
                                }
                                else
                                    DateChangeCode = SegInfo[4].Equals(SegInfo[5]) ? 0 : cm.DateDiff("d", cm.RequestDateTime(SegInfo[4]), cm.RequestDateTime(SegInfo[5]));

                                RtgInfo[i] += String.Concat(SegInfo[0], SegInfo[1], SegInfo[10], cm.NumPosition(SegInfo[11], 4), cm.NumPosition(DateChangeCode.ToString(), 2));
                                RtgClass[i] += SegInfo[12];

                                if (n > 0)
                                {
                                    if (!String.IsNullOrWhiteSpace(RtgInfo[i]))
                                        RtgStop[i] += "/";

                                    RtgStop[i] += SegInfo[0];
                                }
                            }

                            i++;
                        }

                        foreach (XmlNode Fare in FareXml.SelectNodes("fare"))
                        {
                            int seg = Fare.SelectSingleNode("Way").InnerText.Equals("S") ? 0 : 1;

                            //여정정보 비교(편도/왕복만 비교 가능)
                            //if (!RtgInfo[seg].Equals(Fare.SelectSingleNode("Itinerary").InnerText) || !RtgClass[seg].Equals(Fare.SelectSingleNode("ItineraryClass").InnerText))
                            if (!RtgInfo[seg].Equals(Fare.SelectSingleNode("Itinerary").InnerText))
                                FareXml.RemoveChild(Fare);
                        }

                        if (ROT.Equals("OW"))
                            if (FareXml.SelectNodes("fare[Way='S']").Count != 1)
                                throw new Exception("(005) 조건에 일치하는 운임 정보가 없습니다.");

                        if (ROT.Equals("RT"))
                            if (FareXml.SelectNodes("fare[Way='S']").Count != 1 || FareXml.SelectNodes("fare[Way='R']").Count != 1)
                                throw new Exception("(006) 조건에 일치하는 운임 정보가 없습니다.");

                        
                        XmlNode DepFareXml = FareXml.SelectSingleNode("fare[Way='S']");
                        XmlNode ArrFareXml = (ROT.Equals("RT")) ? FareXml.SelectSingleNode("fare[Way='R']") : null;

                        string DLC = DepFareXml.SelectSingleNode("StartAirp").InnerText;
                        string ALC = DepFareXml.SelectSingleNode("EndAirp").InnerText;
                        string CLC = Common.ConvertingArraryToString(RtgStop, ",");
                        string DTD = ArrSDate[0];
                        string ARD = ROT.Equals("RT") ? ArrSDate[1] : "";
                        string OPN = "N";
                        string FLD = "";
                        string CCD = DepFareXml.SelectSingleNode("SeatGrade").InnerText;
                        string FAB = "";// DepFareXml.SelectSingleNode("FareBasisBK").InnerText;
                        string FareTypeCode = DepFareXml.SelectSingleNode("FareTypeCode").InnerText;
                        int NormalFare = (FareEXml != null) ? ((cm.RequestInt(DepFareXml.SelectSingleNode("NormalFare").InnerText) + cm.RequestInt(ArrFareXml.SelectSingleNode("NormalFare").InnerText)) / 2) : cm.RequestInt(DepFareXml.SelectSingleNode("NormalFare").InnerText);
                        string[] PTC = new String[3] { FareTypeCode, "CH", "INF" };
                        int[] NOP = new Int32[3] { ADC, CHC, IFC };
                        int NRR = 250;

                        //MP조회
                        XmlElement ResXml = mas.SearchFareAvailDetailRS(SNM, SAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, "", FAB, PTC, NOP, NRR, "N|75", GUID);
                        
                        if (ResXml.SelectNodes("errorMessageText").Count > 0)
                            throw new Exception(ResXml.SelectSingleNode("errorMessageText/description").InnerText);

                        //여정 필터링
                        foreach (XmlNode FlightIndex in ResXml.SelectNodes("flightInfo/flightIndex"))
                        {
                            int FlightIndexRef = cm.RequestInt(FlightIndex.Attributes.GetNamedItem("ref").InnerText);

                            foreach (XmlNode SegGroup in FlightIndex.SelectNodes("segGroup"))
                            {
                                if (SegGroup.Attributes.GetNamedItem("nosp").InnerText.Equals((cm.RequestInt(DepFareXml.SelectSingleNode("ViaNo").InnerText) + 1).ToString()))
                                {
                                    string[] TmpFareItinerary = (FlightIndexRef.Equals(1) ? DepFareXml : ArrFareXml).SelectSingleNode("Itinerary").InnerText.Split('+');
                                    string[] TmpItinerary = ArrItinerary[FlightIndexRef - 1].Split(',');
                                    XmlNodeList TmpSeg = SegGroup.SelectNodes("seg");
                                    bool SameValue = true;

                                    for (int s = 0; s < TmpSeg.Count; s++)
                                    {
                                        string[] TmpSegInfo = TmpItinerary[s].Split('^');

                                        //출발지
                                        if (TmpSeg[s].Attributes.GetNamedItem("dlc").InnerText != TmpFareItinerary[s].Substring(0, 3))
                                            SameValue = false;

                                        //도착지
                                        if (TmpSeg[s].Attributes.GetNamedItem("alc").InnerText != TmpFareItinerary[s].Substring(3, 3))
                                            SameValue = false;

                                        //마케팅항공사 or 운항항공사

                                        //편명
                                        if (cm.RequestInt(TmpSeg[s].Attributes.GetNamedItem("fln").InnerText) != cm.RequestInt(TmpFareItinerary[s].Substring(8, 4)))
                                            SameValue = false;

                                        //출발일시
                                        if (TmpSeg[s].Attributes.GetNamedItem("ddt").InnerText != cm.RequestDateTime(String.Concat(TmpSegInfo[4], TmpSegInfo[6]), "yyyy-MM-dd HH:mm"))
                                            SameValue = false;

                                        //도착일시
                                        if (TmpSeg[s].Attributes.GetNamedItem("ardt").InnerText != cm.RequestDateTime(String.Concat(TmpSegInfo[5], TmpSegInfo[7]), "yyyy-MM-dd HH:mm"))
                                            SameValue = false;
                                    }
                                    if (!SameValue)
                                        FlightIndex.RemoveChild(SegGroup);
                                }
                                else
                                    FlightIndex.RemoveChild(SegGroup);
                            }

                            if (FlightIndex.SelectNodes("segGroup").Count.Equals(0))
                            {
                                //사용중지(2018-05-21)
                                //SearchLogDBSave(DLC, ALC, DTD, ARD, SAC, ArrGoodCode[0], NormalFare, "Y");
                                throw new Exception("(007) 조건에 일치하는 여정 정보가 없습니다.");
                            }
                            else if (FlightIndex.SelectNodes("segGroup").Count > 1)
                            {
                                for (int x = FlightIndex.SelectNodes("segGroup").Count; x > 1; x--)
                                    FlightIndex.RemoveChild(FlightIndex.SelectNodes("segGroup")[(x - 1)]);
                            }
                        }
                        
                        //운임 필터링
                        bool RemoveOK = false;
                        
                        //요청보다 저렴한 요금 존재 여부
                        bool LowFareExist = false;

                        foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                        {
                            RemoveOK = false;

                            //if (PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare[@ptc='{0}']", FareTypeCode)).Count > 0 && PriceIndex.SelectSingleNode(String.Format("paxFareGroup/paxFare[@ptc='{0}']/amount", FareTypeCode)).Attributes.GetNamedItem("fare").InnerText.Equals(NormalFare.ToString()))
                            if (PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare[@ptc='{0}']", FareTypeCode)).Count > 0 && cm.RequestInt(PriceIndex.SelectSingleNode(String.Format("paxFareGroup/paxFare[@ptc='{0}']/amount", FareTypeCode)).Attributes.GetNamedItem("fare").InnerText) <= NormalFare)
                            {
                                if (ROT.Equals("OW"))
                                {
                                    if (PriceIndex.SelectNodes(String.Format("segGroup/seg[ref[1]='{0}']", ResXml.SelectSingleNode("flightInfo/flightIndex[@ref=1]/segGroup").Attributes.GetNamedItem("ref").InnerText)).Count.Equals(0))
                                    {
                                        ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                        RemoveOK = true;
                                    }
                                    ////fare basis
                                    //else if (PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare/segFareGroup/segFare[@ref='1']/fare/fare[@basis!='{0}']", DepFareXml.SelectSingleNode("FareBasisBK").InnerText)).Count > 0)
                                    //{
                                    //    ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                    //    RemoveOK = true;
                                    //}
                                }
                                else
                                {
                                    if (PriceIndex.SelectNodes(String.Format("segGroup/seg[ref[1]='{0}' and ref[2]='{1}']", ResXml.SelectSingleNode("flightInfo/flightIndex[@ref=1]/segGroup").Attributes.GetNamedItem("ref").InnerText, ResXml.SelectSingleNode("flightInfo/flightIndex[@ref=2]/segGroup").Attributes.GetNamedItem("ref").InnerText)).Count.Equals(0))
                                    {
                                        ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                        RemoveOK = true;
                                    }
                                    //fare basis
                                    //else if (PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare/segFareGroup/segFare[@ref='1']/fare/fare[@basis!='{0}']", DepFareXml.SelectSingleNode("FareBasisBK").InnerText)).Count > 0)
                                    //{
                                    //    ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                    //    RemoveOK = true;
                                    //}
                                    ////fare basis
                                    //else if (PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare/segFareGroup/segFare[@ref='2']/fare/fare[@basis!='{0}']", ArrFareXml.SelectSingleNode("FareBasisBK").InnerText)).Count > 0)
                                    //{
                                    //    ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                    //    RemoveOK = true;
                                    //}
                                }
                            }
                            else
                            {
                                ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                RemoveOK = true;
                            }
                            
                            //if (!RemoveOK)
                            //{
                            //    //부킹클래스
                            //    foreach (XmlNode SegFare in PriceIndex.SelectNodes("paxFareGroup/paxFare/segFareGroup/segFare"))
                            //    {
                            //        string[] ItineraryClass = (SegFare.Attributes.GetNamedItem("ref").InnerText.Equals("1") ? DepFareXml : ArrFareXml).SelectSingleNode("ItineraryClass").InnerText.Split('+');

                            //        for (int c = 0; c < SegFare.SelectNodes("fare").Count; c++)
                            //        {
                            //            if (SegFare.SelectNodes("fare")[c].SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText != ItineraryClass[c].Trim())
                            //            {
                            //                ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                            //                RemoveOK = true;
                            //                break;
                            //            }
                            //        }

                            //        if (RemoveOK)
                            //            break;
                            //    }
                            //}

                            //요청보다 저렴한 요금 존재할 경우
                            if (!LowFareExist && PriceIndex.SelectNodes(String.Format("paxFareGroup/paxFare[@ptc='{0}']", FareTypeCode)).Count > 0 && cm.RequestInt(PriceIndex.SelectSingleNode(String.Format("paxFareGroup/paxFare[@ptc='{0}']/amount", FareTypeCode)).Attributes.GetNamedItem("fare").InnerText) < NormalFare)
                                LowFareExist = true;
                        }
                        
                        if (ResXml.SelectNodes("priceInfo/priceIndex").Count.Equals(0))
                        {
                            //사용중지(2018-05-21)
                            //SearchLogDBSave(DLC, ALC, DTD, ARD, SAC, ArrGoodCode[0], NormalFare, "Y");
                            throw new Exception("(008) 조건에 일치하는 운임 정보를 찾을 수 없습니다.");
                        }
                        else
                        {
                            //프로모션 동일 운임
                            if (!String.IsNullOrWhiteSpace(ArrEventCode[0]))
                            {
                                if (ResXml.SelectNodes(String.Format("priceInfo/priceIndex[promotionInfo and promotionInfo/item/promotions/promotion/@promotionId='{0}']", ArrEventCode[0])).Count > 0)
                                {
                                    foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                                    {
                                        if (PriceIndex.SelectNodes("promotionInfo").Count.Equals(0) || PriceIndex.SelectNodes(String.Format("promotionInfo/item/promotions/promotion[@promotionId='{0}']", ArrEventCode[0])).Count.Equals(0))
                                            ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                    }
                                }
                            }

                            //요금리스트가 하나 이상일 경우 하나만 남기고 모두 삭제
                            if (ResXml.SelectNodes("priceInfo/priceIndex").Count > 1)
                            {
                                //원금액에 근사한 금액 구하기
                                //string ArrFare = string.Empty;

                                //foreach (XmlNode PaxFare in ResXml.SelectNodes("priceInfo/priceIndex/paxFareGroup/paxFare[@ptc='ADT']"))
                                //    ArrFare += String.Concat(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText, ",");

                                //string NearFare = Common.NearValue(ArrFare, NormalFare).ToString();

                                //foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                                //{
                                //    if (PriceIndex.SelectSingleNode("paxFareGroup/paxFare[@ptc='ADT']/amount").Attributes.GetNamedItem("fare").InnerText != NearFare)
                                //        ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                //}

                                //if (ResXml.SelectNodes("priceInfo/priceIndex").Count > 1)
                                //{
                                //    for (int x = ResXml.SelectNodes("priceInfo/priceIndex").Count; x > 1; x--)
                                //        ResXml.SelectSingleNode("priceInfo").RemoveChild(ResXml.SelectNodes("priceInfo/priceIndex")[(x - 1)]);
                                //}

                                //최저가 요금으로 출력
                                double TmpPaxFare = 0;

                                foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                                {
                                    if (TmpPaxFare > 0)
                                    {
                                        if (TmpPaxFare <= cm.RequestDouble(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText))
                                            ResXml.SelectSingleNode("priceInfo").RemoveChild(PriceIndex);
                                    }
                                    else
                                        TmpPaxFare = cm.RequestDouble(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText);
                                }
                                
                                //하나의 요금만 남기기
                                //for (int x = ResXml.SelectNodes("priceInfo/priceIndex").Count; x > 1; x--)
                                //    ResXml.SelectSingleNode("priceInfo").RemoveChild(ResXml.SelectNodes("priceInfo/priceIndex")[(x - 1)]);
                            }
                        }

                        if (ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg").Count > 1)
                        {
                            if (ROT.Equals("OW"))
                            {
                                for (int x = ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg").Count; x > 0; x--)
                                {
                                    if (ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)].SelectSingleNode("ref").InnerText != ResXml.SelectSingleNode("flightInfo/flightIndex[@ref='1']/segGroup").Attributes.GetNamedItem("ref").InnerText)
                                        ResXml.SelectSingleNode("priceInfo/priceIndex/segGroup").RemoveChild(ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)]);
                                }
                            }
                            else
                            {
                                for (int x = ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg").Count; x > 0; x--)
                                {
                                    if (!(ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)].SelectNodes("ref")[0].InnerText.Equals(ResXml.SelectSingleNode("flightInfo/flightIndex[@ref='1']/segGroup").Attributes.GetNamedItem("ref").InnerText) && ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)].SelectNodes("ref")[1].InnerText.Equals(ResXml.SelectSingleNode("flightInfo/flightIndex[@ref='2']/segGroup").Attributes.GetNamedItem("ref").InnerText)))
                                        ResXml.SelectSingleNode("priceInfo/priceIndex/segGroup").RemoveChild(ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)]);
                                }
                            }
                        }

                        if (ResXml.SelectNodes("priceInfo/priceIndex").Count.Equals(0))
                        {
                            //사용중지(2018-05-21)
                            //SearchLogDBSave(DLC, ALC, DTD, ARD, SAC, ArrGoodCode[0], NormalFare, "Y");
                            throw new Exception("(009) 조건에 일치하는 운임 정보를 찾을 수 없습니다.");
                        }

                        if (ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg").Count > 1)
                        {
                            for (int x = ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg").Count; x > 1; x--)
                                ResXml.SelectSingleNode("priceInfo/priceIndex/segGroup").RemoveChild(ResXml.SelectNodes("priceInfo/priceIndex/segGroup/seg")[(x - 1)]);
                        }

                        ResXml.SelectSingleNode("priceInfo/priceIndex/segGroup/seg/ref[1]").InnerText = ResXml.SelectSingleNode("flightInfo/flightIndex[@ref='1']/segGroup").Attributes.GetNamedItem("ref").InnerText;

                        if (ROT.Equals("RT"))
                            ResXml.SelectSingleNode("priceInfo/priceIndex/segGroup/seg/ref[2]").InnerText = ResXml.SelectSingleNode("flightInfo/flightIndex[@ref='2']/segGroup").Attributes.GetNamedItem("ref").InnerText;

                        //네이버에서 넘어 온 텍스와 유류할증료 및 프로모션정보 추가
                        int SumTax = 0;
                        int SumFsc = 0;
                        int PaxCount = 0;
                        int AdtFare = 0;
                        int NaverDiscount = cm.RequestInt(ArrPromtionAmount[0]);
                        int PaxNaverDiscount = 0;
                        int SumNaverDiscount = 0;

                        foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                        {
                            foreach (XmlNode PaxFare in PriceIndex.SelectNodes("paxFareGroup/paxFare"))
                            {
                                int baseRef1 = 0;
                                int baseRef2 = 1;

                                switch (PaxFare.Attributes.GetNamedItem("ptc").InnerText)
                                {
                                    case "CHD": baseRef1 = 2; baseRef2 = 2; break;
                                    case "INF": baseRef1 = 4; baseRef2 = 3; break;
                                    default: baseRef1 = 0; baseRef2 = 1; break;
                                }

                                PaxNaverDiscount = (NaverDiscount > 0 && ArrPromtionAmount[baseRef2].Trim().Equals("Y"))? NaverDiscount : 0;

                                PaxFare.SelectSingleNode("amount").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverTax")).InnerText = ArrTax[baseRef1];
                                PaxFare.SelectSingleNode("amount").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverFsc")).InnerText = ArrTax[(baseRef1 + 1)];
                                PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = PaxNaverDiscount.ToString();
                                PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) - PaxNaverDiscount).ToString();

                                PaxCount = PaxFare.SelectNodes("traveler/ref").Count;

                                SumTax += (cm.RequestInt(ArrTax[baseRef1]) * PaxCount);
                                SumFsc += (cm.RequestInt(ArrTax[(baseRef1 + 1)]) * PaxCount);
                                SumNaverDiscount += (PaxNaverDiscount * PaxCount);

                                //성인요금
                                if (AdtFare.Equals(0) && PaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("ADT"))
                                    AdtFare = cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText);
                            }

                            PriceIndex.SelectSingleNode("summary").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverTax")).InnerText = SumTax.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverFsc")).InnerText = SumFsc.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disPartner").InnerText = SumNaverDiscount.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText) - SumNaverDiscount).ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = (cm.RequestInt(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText) - SumNaverDiscount).ToString();
                        }

                        //프로모션 정리
                        if (ResXml.SelectNodes("priceInfo/priceIndex/promotionInfo").Count > 0)
                        {
                            XmlNode PriceIndex = ResXml.SelectSingleNode("priceInfo/priceIndex");
                            XmlNode PromotionInfo = PriceIndex.SelectSingleNode("promotionInfo");
                            bool PromApply = false;

                            if (!String.IsNullOrWhiteSpace(EventCode))
                            {
                                XmlNode SelProm = PromotionInfo.SelectSingleNode(String.Format("item/promotions/promotion[@promotionId='{0}']", ArrEventCode[0]));

                                if (SelProm != null)
                                {
                                    PromApply = true;

                                    PromotionInfo.SelectSingleNode("item/promotionId").InnerText = SelProm.Attributes.GetNamedItem("promotionId").InnerText;
                                    PromotionInfo.SelectSingleNode("item/incentiveCode").InnerText = SelProm.Attributes.GetNamedItem("incentiveCode").InnerText;
                                    PromotionInfo.SelectSingleNode("item/incentiveName").InnerText = SelProm.Attributes.GetNamedItem("incentiveName").InnerText;
                                    PromotionInfo.SelectSingleNode("item/fareTarget").InnerText = SelProm.Attributes.GetNamedItem("fareTarget").InnerText;
                                    PromotionInfo.SelectSingleNode("item/promotionTL").InnerText = SelProm.Attributes.GetNamedItem("promotionTL").InnerText;
                                }
                            }

                            if (!PromApply)
                            {
                                PriceIndex.RemoveChild(PromotionInfo);
                                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText) - SumNaverDiscount).ToString();
                                
                                foreach (XmlNode PaxFare in PriceIndex.SelectNodes("paxFareGroup/paxFare[@ptc!='INF']"))
                                {
                                    switch (PaxFare.Attributes.GetNamedItem("ptc").InnerText)
                                    {
                                        case "CHD": PaxNaverDiscount = (NaverDiscount > 0 && ArrPromtionAmount[2].Trim().Equals("Y")) ? NaverDiscount : 0; break;
                                        case "INF": PaxNaverDiscount = (NaverDiscount > 0 && ArrPromtionAmount[3].Trim().Equals("Y")) ? NaverDiscount : 0; break;
                                        default: PaxNaverDiscount = (NaverDiscount > 0 && ArrPromtionAmount[1].Trim().Equals("Y")) ? NaverDiscount : 0; break;
                                    }
                                    
                                    PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText) - PaxNaverDiscount).ToString();
                                }
                            }
                        }

                        //네이버 프로모션 등록
                        if (!String.IsNullOrWhiteSpace(PromtionCode))
                        {
                            XmlNode PartnerPromotions = ResXml.OwnerDocument.CreateElement("partnerPromotions");
                            XmlNode PartnerPromotion = ResXml.OwnerDocument.CreateElement("promotion");
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("partner")).InnerText = "NAVER";
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("code")).InnerText = PromtionCode.Split('^')[0];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("amount")).InnerText = ArrPromtionAmount[0];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("adultYN")).InnerText = ArrPromtionAmount[1];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("childYN")).InnerText = ArrPromtionAmount[2];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("infantYN")).InnerText = ArrPromtionAmount[3];
                            PartnerPromotion.InnerText = PromtionName;

                            PartnerPromotions.AppendChild(PartnerPromotion);

                            if (ResXml.SelectNodes("priceInfo/priceIndex/promotionInfo").Count > 0)
                                ResXml.SelectSingleNode("priceInfo/priceIndex/promotionInfo/item").AppendChild(PartnerPromotions);
                            else
                            {
                                XmlNode PromotionInfo = ResXml.OwnerDocument.CreateElement("promotionInfo");
                                XmlNode Item = ResXml.OwnerDocument.CreateElement("item");
                                XmlNode Promotions = ResXml.OwnerDocument.CreateElement("promotions");
                                XmlNode Promotion = ResXml.OwnerDocument.CreateElement("Promotion");

                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("promotionId"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("siteNum"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("airCode"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("tripType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("cabinClass"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("bookingClass"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("paxType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("discount"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("commission"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareDiscount"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentive"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentiveCode"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentiveName"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareTarget"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("childDiscountYN"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("promotionTL"));

                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("promotionId"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("incentiveCode"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("incentiveName"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("fareTarget"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("promotionTL"));

                                Promotions.AppendChild(Promotion);
                                Item.AppendChild(Promotions);
                                Item.AppendChild(PartnerPromotions);
                            }
                        }

                        //판매가 재계산
                        XmlNode Summary = ResXml.SelectSingleNode("priceInfo/priceIndex/summary");
                        Summary.Attributes.GetNamedItem("price").InnerText = Convert.ToString(cm.RequestDouble(Summary.Attributes.GetNamedItem("disFare").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("tax").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("fsc").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("tasf").InnerText));
                        Summary.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverPrice")).InnerText = Convert.ToString(cm.RequestDouble(Summary.Attributes.GetNamedItem("disFare").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("naverTax").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("naverFsc").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("tasf").InnerText));

                        //사용중지(2018-05-21)
                        //요청보다 저렴한 요금 존재할 경우 로그 기록
                        //if (LowFareExist)
                        //    SearchLogDBSave(DLC, ALC, DTD, ARD, SAC, ArrGoodCode[0], AdtFare, "N");

                        cm.XmlFileSave(ResXml, "Naver", "SearchFareAvailforNaverRS", "N", GUID);
                        return ResXml;
                    }
                    else
                        throw new Exception("(010) 조건에 일치하는 운임 정보를 찾을 수 없습니다.");
                }
                else
                    throw new Exception("(011) 정보가 부족합니다.");
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// [네이버] 검색 요청 내용 DB로 저장
        /// </summary>
        /// <param name="DLC">출발지</param>
        /// <param name="ALC">도착지</param>
        /// <param name="DTD">출발일</param>
        /// <param name="ARD">도착일</param>
        /// <param name="SAC">항공</param>
        /// <param name="Err">오류여부</param>
        public void SearchLogDBSave(string DLC, string ALC, string DTD, string ARD, string SAC, string ArrGoodCode, int AdtFare, string ErrYN)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SERVICELOG"].ConnectionString))
                    {
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 60;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_T_아이템예약_해외항공_네이버_로그";

                        cmd.Parameters.Add("@출발지", SqlDbType.Char, 3);
                        cmd.Parameters.Add("@도착지", SqlDbType.Char, 3);
                        cmd.Parameters.Add("@출발일", SqlDbType.VarChar, 10);
                        cmd.Parameters.Add("@도착일", SqlDbType.VarChar, 10);
                        cmd.Parameters.Add("@항공", SqlDbType.Char, 2);
                        cmd.Parameters.Add("@상품코드", SqlDbType.NVarChar, 50);
                        cmd.Parameters.Add("@성인요금", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@오류여부", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@출발지"].Value = DLC;
                        cmd.Parameters["@도착지"].Value = ALC;
                        cmd.Parameters["@출발일"].Value = DTD;
                        cmd.Parameters["@도착일"].Value = ARD;
                        cmd.Parameters["@항공"].Value = SAC;
                        cmd.Parameters["@상품코드"].Value = ArrGoodCode;
                        cmd.Parameters["@성인요금"].Value = AdtFare;
                        cmd.Parameters["@오류여부"].Value = ErrYN;
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
            }
            catch (Exception) { }
        }

        #endregion "네이버(아마데우스)"

        #region "네이버(갈릴레오)"

        /// <summary>
        /// [네이버] Fare + Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호(4638)</param>
        /// <param name="SAC">항공사 코드 [NAIRV]</param>
        /// <param name="SCity">출발지 공항 코드 [SCITY1/SCITY2/SCITY3/SCITY4/SCITY5/SCITY6/SCITY7]</param>
        /// <param name="ECity">도착지 공항 코드 [ECITY1/ECITY2/ECITY3/ECITY4/ECITY5/ECITY6/ECITY7]</param>
        /// <param name="SDate">출발일(YYYYMMDD) [SDATE1/SDATE2/SDATE3/SDATE4/SDATE5/SDATE6/SDATE7]</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복) [TRIP]</param>
        /// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
        /// <param name="StayLength">체류기간</param>
        /// <param name="GoodCode">상품코드 [SGC/RGC]</param>
        /// <param name="EventCode">이벤트(프로모션)코드 [EventNum]</param>
        /// <param name="PartnerNum">사이트할인코드 [PartnerNum/PartnerNum]</param>
        /// <param name="PromtionCode">네이버할인코드 [PromotionCode]</param>
        /// <param name="PromtionName">네이버할인명 [PromotionName]</param>
        /// <param name="PromtionAmount">네이버할인금액 [PromotionAmt/PromotionAdtInd/PromotionChdInd/PromotionInfInd]</param>
        /// <param name="Itinerary">여정정보 [Itinerary1/Itinerary2/Itinerary3]</param>
        /// <param name="TaxInfo">텍스정보 [TaxInfo]</param>
        /// <param name="NaverFareJoin">결합조건코드 [NaverFareJoin]</param>
        /// <param name="FareLocation">운임구분자 [FareLocation]</param>
        /// <param name="AddOnDom">국내구간정보 [AddOnDomStart/AddOnDomReturn]</param>
        /// <param name="BagInfo">수하물정보 [AdultBagInfo/ChildBagInfo/InfantBagInfo]</param>
        /// <param name="PlatingCarrier">발권항공사 [PlatingCarrier]</param>
        /// <param name="FareInfo">요금부가정보 [FareInfo]</param>
        /// <param name="ADC">성인수 [ADT]</param>
        /// <param name="CHC">소아수 [CHD]</param>
        /// <param name="IFC">유아수 [INF]</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[네이버] Fare + Availability 조회")]
        public XmlElement SearchFareAvailforNaverRS(int SNM, string SAC, string SCity, string ECity, string SDate, string ROT, string CCD, string StayLength, string GoodCode, string EventCode, string PartnerNum, string PromtionCode, string PromtionName, string PromtionAmount, string Itinerary, string TaxInfo, string NaverFareJoin, string FareLocation, string AddOnDom, string BagInfo, string PlatingCarrier, string FareInfo, int ADC, int CHC, int IFC, string RQT)
        {
            string GUID = cm.GetGUID;
            string LogGUID = GUID;

            string EncGoodCode = GoodCode;
            string EncItinerary = Itinerary;
            string EncTaxInfo = TaxInfo;
            string EncFareInfo = FareInfo;

            //호스트운임일 경우 디코딩
            if (!String.IsNullOrWhiteSpace(FareLocation) && FareLocation.Equals("H"))
            {
                string[] ArrGoodCode = (GoodCode.IndexOf('*') != -1) ? GoodCode.Split('*') : GoodCode.Split('/');
                string[] ArrItinerary = (Itinerary.IndexOf('*') != -1) ? Itinerary.Split('*') : Itinerary.Split('/');

                GoodCode = "";
                Itinerary = "";
                TaxInfo = cm.GalileoDeCompression(EncTaxInfo);
                FareInfo = cm.GalileoDeCompression(EncFareInfo);

                foreach (string tpGoodCode in EncGoodCode.Split('*'))
                {
                    if (!String.IsNullOrWhiteSpace(GoodCode))
                        GoodCode += "*";

                    GoodCode += cm.GalileoDeCompression(tpGoodCode);
                }

                foreach (string tpItinerary in EncItinerary.Split('*'))
                {
                    if (!String.IsNullOrWhiteSpace(Itinerary))
                        Itinerary += "*";

                    Itinerary += cm.GalileoDeCompression(tpItinerary);
                }
            }
            
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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청7", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청9", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청11", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청12", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청13", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청14", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청15", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청16", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청17", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청18", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청19", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청20", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청21", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청22", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청23", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청24", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 392;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SAC;
                sqlParam[8].Value = SCity;
                sqlParam[9].Value = ECity;
                sqlParam[10].Value = SDate;
                sqlParam[11].Value = ROT;
                sqlParam[12].Value = CCD;
                sqlParam[13].Value = StayLength;
                sqlParam[14].Value = GoodCode;
                sqlParam[15].Value = EventCode;
                sqlParam[16].Value = PartnerNum;
                sqlParam[17].Value = PromtionCode;
                sqlParam[18].Value = PromtionName;
                sqlParam[19].Value = PromtionAmount;
                sqlParam[20].Value = Itinerary;
                sqlParam[21].Value = TaxInfo;
                sqlParam[22].Value = NaverFareJoin;
                sqlParam[23].Value = ADC;
                sqlParam[24].Value = CHC;
                sqlParam[25].Value = IFC;
                sqlParam[26].Value = AddOnDom;
                sqlParam[27].Value = FareLocation;
                sqlParam[28].Value = BagInfo;
                sqlParam[29].Value = PlatingCarrier;
                sqlParam[30].Value = FareInfo;

                log.LogDBSave(sqlParam);
            }
            finally { }

            try
            {
                XmlDocument XmlTmp = new XmlDocument();
                string Xml = string.Empty;

                Xml += "<SearchFareAvailforNaverRQ>";
                Xml += String.Format("<SNM>{0}</SNM>", SNM);
                Xml += String.Format("<SAC>{0}</SAC>", SAC);
                Xml += String.Format("<SCity>{0}</SCity>", SCity);
                Xml += String.Format("<ECity>{0}</ECity>", ECity);
                Xml += String.Format("<SDate>{0}</SDate>", SDate);
                Xml += String.Format("<ROT>{0}</ROT>", ROT);
                Xml += String.Format("<CCD>{0}</CCD>", CCD);
                Xml += String.Format("<StayLength>{0}</StayLength>", StayLength);
                Xml += String.Format("<GoodCode>{0}</GoodCode>", EncGoodCode);
                Xml += String.Format("<EventCode>{0}</EventCode>", EventCode);
                Xml += String.Format("<PartnerNum>{0}</PartnerNum>", PartnerNum);
                Xml += String.Format("<PromotionCode>{0}</PromotionCode>", PromtionCode);
                Xml += String.Format("<PromotionName>{0}</PromotionName>", PromtionName);
                Xml += String.Format("<PromotionAmount>{0}</PromotionAmount>", PromtionAmount);
                Xml += String.Format("<Itinerary><![CDATA[{0}]]></Itinerary>", EncItinerary);
                Xml += String.Format("<TaxInfo>{0}</TaxInfo>", EncTaxInfo);
                Xml += String.Format("<NaverFareJoin>{0}</NaverFareJoin>", NaverFareJoin);
                Xml += String.Format("<FareLocation>{0}</FareLocation>", FareLocation);
                Xml += String.Format("<AddOnDom>{0}</AddOnDom>", AddOnDom);
                Xml += String.Format("<BagInfo>{0}</BagInfo>", BagInfo);
                Xml += String.Format("<PlatingCarrier>{0}</PlatingCarrier>", PlatingCarrier);
                Xml += String.Format("<FareInfo>{0}</FareInfo>", EncFareInfo);
                Xml += String.Format("<ADC>{0}</ADC>", ADC);
                Xml += String.Format("<CHC>{0}</CHC>", CHC);
                Xml += String.Format("<IFC>{0}</IFC>", IFC);
                Xml += String.Format("<RQT>{0}</RQT>", RQT);
                Xml += "</SearchFareAvailforNaverRQ>";

                XmlTmp.LoadXml(Xml);
                cm.XmlFileSave(XmlTmp, "Naver", "SearchFareAvailforNaverRQ", "N", GUID);
            }
            finally { }

            try
            {
                if (!ROT.Equals("OW") && !ROT.Equals("RT"))
                    throw new Exception("편도 및 왕복일 경우에만 예약이 가능합니다.");

                string[] ArrSCity = (SCity.IndexOf('*') != -1) ? SCity.Split('*') : SCity.Split('/');
                string[] ArrECity = (ECity.IndexOf('*') != -1) ? ECity.Split('*') : ECity.Split('/');
                string[] ArrSDate = (SDate.IndexOf('*') != -1) ? SDate.Split('*') : SDate.Split('/');
                string[] ArrGoodCode = (GoodCode.IndexOf('*') != -1) ? GoodCode.Split('*') : GoodCode.Split('/');
                string[] ArrEncGoodCode = (EncGoodCode.IndexOf('*') != -1) ? EncGoodCode.Split('*') : EncGoodCode.Split('/');
                string[] ArrEventCode = String.IsNullOrWhiteSpace(EventCode) ? "0/0".Split('/') : EventCode.Split('/');
                string[] ArrPartnerNum = (String.IsNullOrWhiteSpace(PartnerNum) ? "," : ((PartnerNum.IndexOf(',') != -1) ? PartnerNum : String.Concat(PartnerNum, ","))).Split(',');
                string[] ArrItinerary = (Itinerary.IndexOf('*') != -1) ? Itinerary.Split('*') : Itinerary.Split('/');
                string[] ArrEncItinerary = (EncItinerary.IndexOf('*') != -1) ? EncItinerary.Split('*') : EncItinerary.Split('/');
                string[] ArrPromtionCode = (String.IsNullOrWhiteSpace(PromtionCode) ? "" : PromtionCode).Split('^');
                string[] ArrPromtionAmount = (PromtionAmount.IndexOf('*') != -1) ? PromtionAmount.Split('*') : PromtionAmount.Split('/');
                string[] ArrAddOnDom = (AddOnDom.IndexOf('*') != -1) ? AddOnDom.Split('*') : AddOnDom.Split('/');
                string[] ArrBagInfo = (BagInfo.IndexOf('*') != -1) ? BagInfo.Split('*') : BagInfo.Split('/');
                string PMID = ((!String.IsNullOrWhiteSpace(ArrEventCode[0]) && ArrEventCode[0] != "0") ? ArrEventCode[0] : ArrPartnerNum[0]);

                if (!String.IsNullOrWhiteSpace(ArrSCity[0]) && !String.IsNullOrWhiteSpace(ArrECity[0]) && !String.IsNullOrWhiteSpace(ArrSDate[0]) && !String.IsNullOrWhiteSpace(ArrGoodCode[0]) && !String.IsNullOrWhiteSpace(ArrItinerary[0]))
                {
                    if (cm.DateDiff("d", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), cm.RequestDateTime(ArrSDate[0])) < 1)
                        throw new Exception("최소 2일 이후 출발인 경우에만 예약이 가능합니다.");

                    XmlElement ResInfoCreateXml = gas.ResInfoCreateRS(ArrSCity[0].Trim(), ArrECity[0].Trim(), ArrSCity[1].Trim(), ArrECity[1].Trim(), ArrSCity[2].Trim(), ArrECity[2].Trim(), ArrSCity[3].Trim(), ArrECity[3].Trim(), ArrSCity[4].Trim(), ArrECity[4].Trim(), ArrSCity[5].Trim(), ArrECity[5].Trim(), ArrSCity[2].Trim(), ArrECity[6].Trim(), ArrSDate[0].Trim(), ArrSDate[1].Trim(), ArrSDate[2].Trim(), ArrSDate[3].Trim(), ArrSDate[4].Trim(), ArrSDate[5].Trim(), ArrSDate[6].Trim(), ROT, CCD, StayLength, ADC, CHC, IFC, ArrEncItinerary[0].Trim(), ArrEncItinerary[1].Trim(), ((ArrEncItinerary.Length > 2) ? ArrEncItinerary[2].Trim() : ""), ArrEncGoodCode[0].Trim(), ArrEncGoodCode[1].Trim(), EventCode, ArrPartnerNum[0].Trim(), ArrPartnerNum[1].Trim(), EncTaxInfo, PromtionCode, PromtionName, ArrPromtionAmount[0].Trim(), ArrPromtionAmount[1].Trim(), ArrPromtionAmount[2].Trim(), ArrPromtionAmount[3].Trim(), NaverFareJoin, "NAVER", "N", FareLocation, ArrAddOnDom[0].Trim(), ArrAddOnDom[1].Trim(), ArrBagInfo[0].Trim(), ArrBagInfo[1].Trim(), ArrBagInfo[2].Trim(), PlatingCarrier, EncFareInfo, GUID);

                    if (ResInfoCreateXml.SelectSingleNode("ResultErrorNo").InnerText.Equals("0"))
                    {
                        XmlElement ResXml = null;
                        string[] ArrTax = TaxInfo.Split('/');

                        //운임항공사
                        SAC = ResInfoCreateXml.SelectSingleNode("FareInfo/MainAirV").InnerText.ToUpper();

                        //호스트 운임일 경우
                        if (ResInfoCreateXml.SelectNodes("FareInfo/GoodCode").Count > 0)
                            GoodCode = ResInfoCreateXml.SelectSingleNode("FareInfo/GoodCode").InnerText;

                        //LCC(LJ,TW,ZE,VJ)는 아마데우스 호출(2016-06-14부터)
                        //진에어(LJ)는 갈릴레오로 호출로 제외(2018-10-17,김지영팀장)
                        if (SAC != null && (SAC.Equals("TW") || SAC.Equals("ZE") || SAC.Equals("VJ")))
                        {
                            ResXml = SearchFareAvailforNaverRSAmadeus(SNM, SAC, GoodCode, PromtionCode, PromtionName, ArrSCity, ArrECity, ArrSDate, ArrGoodCode, ArrEventCode, ArrPartnerNum, ArrItinerary, ArrTax, ArrPromtionAmount, ADC, CHC, IFC, PMID, GUID, ResInfoCreateXml);

                            //갈릴레오 운임과 아마데우스 운임 비교(검증)
                            if (ResXml.SelectNodes("priceInfo").Count > 0 && ResXml.SelectNodes("priceInfo/priceIndex").Count > 0)
                            {
                                int AdtNormalFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/AdtNormalFare").InnerText);
                                int AdtSaleFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/AdtSaleFare").InnerText);
                                int ChdNormalFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/ChdNormalFare").InnerText);
                                int ChdSaleFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/ChdSaleFare").InnerText);
                                int InfNormalFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/InfNormalFare").InnerText);
                                int InfSaleFare = cm.RequestInt(ResInfoCreateXml.SelectSingleNode("FareInfo/InfSaleFare").InnerText);
                                bool UseOK = true;

                                //1천원 차이는 통과(2019-04-29,김지영팀장)
                                foreach (XmlNode PaxFare in ResXml.SelectNodes("priceInfo/priceIndex/paxFareGroup/paxFare"))
                                {
                                    if (PaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("CHD"))
                                    {
                                        if (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) > (ChdSaleFare + 1000))
                                        {
                                            UseOK = false;
                                            break;
                                        }
                                    }
                                    else if (PaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("INF"))
                                    {
                                        if (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) > (InfSaleFare + 1000))
                                        {
                                            UseOK = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) > (AdtSaleFare + 1000))
                                        {
                                            UseOK = false;
                                            break;
                                        }
                                    }
                                }

                                if (!UseOK)
                                    ResXml = null;
                            }
                            else
                                ResXml = null;

                            if (ResXml == null)
                                throw new Exception("예약 가능한 스케쥴 및 운임정보를 확인할 수 없습니다.");
                            
                            //갈릴레오 호출
                            //if (ResXml == null)
                            //    ResXml = mas.ToModeReservationProcessGalileo(SNM, ResInfoCreateXml, PMID);
                        }
                        else
                        {
                            //갈릴레오 호출
                            ResXml = mas.ToModeReservationProcessGalileo(SNM, ResInfoCreateXml, PMID);
                        }
                            
                        //갈릴레오 정보 저장
                        XmlNode FareMessage = ResXml.SelectSingleNode("priceInfo/priceIndex/fareMessage");

                        //갈릴레오 정보 저장 - 예약키
                        XmlNode ReqIdxNode = ResXml.OwnerDocument.CreateElement("reqIdx");
                        ReqIdxNode.InnerText = ResInfoCreateXml.SelectSingleNode("ReqIdx").InnerText;
                        FareMessage.AppendChild(ReqIdxNode);

                        //갈릴레오 정보 저장 - 프로모션번호
                        XmlNode PromIDNode = ResXml.OwnerDocument.CreateElement("promotionId");
                        PromIDNode.InnerText = PMID;
                        FareMessage.AppendChild(PromIDNode);

                        //갈릴레오 정보 저장 - 운임구분자
                        XmlNode FareLocationNode = ResXml.OwnerDocument.CreateElement("fareLocation");
                        FareLocationNode.InnerText = FareLocation;
                        FareMessage.AppendChild(FareLocationNode);

                        //갈릴레오 정보 저장 - 운임키
                        XmlNode FareKeyNode = ResXml.OwnerDocument.CreateElement("fareKey");
                        FareKeyNode.InnerText = GoodCode.Replace("*", "/");
                        FareMessage.AppendChild(FareKeyNode);

                        //갈릴레오 정보 저장 - 예약가능 GDS
                        XmlNode GDSTypeNode = ResXml.OwnerDocument.CreateElement("gdsType");
                        GDSTypeNode.InnerText = ResInfoCreateXml.SelectSingleNode("GDSType").InnerText;
                        FareMessage.AppendChild(GDSTypeNode);

                        //갈릴레오 정보 저장 - 사전발권
                        //발권마감일(LTD)이 명일이라면 24시간 이내 발권 조건 여부 판단하여 TL 업데이트(2016-07-26,김지영과장)
                        string LTD = ResInfoCreateXml.SelectSingleNode("FareInfo/TktLimitDate").InnerText;

                        if (!String.IsNullOrWhiteSpace(LTD) && LTD.Equals(DateTime.Now.AddDays(1).ToString("yyyyMMdd")))
                        {
                            //24시간 발권일 경우 예약 시간대별로 TL 설정(2016-09-22,정성하과장)
                            if (ResInfoCreateXml.SelectSingleNode("FareInfo/TktAfterReservation").InnerText.Equals("24H") || ResInfoCreateXml.SelectSingleNode("FareInfo/TktAfterReservation").InnerText.Equals("1D"))
                                LTD = cm.TL24(SNM);
                        }

                        XmlNode TicketLimitDateNode = ResXml.OwnerDocument.CreateElement("ticketLimitDate");
                        TicketLimitDateNode.InnerText = LTD;
                        FareMessage.AppendChild(TicketLimitDateNode);

                        //갈릴레오 정보 저장 - 요금규정
                        ResXml.SelectSingleNode("priceInfo/priceIndex/fareRuleUrl").AppendChild((XmlCDataSection)ResXml.OwnerDocument.CreateCDataSection(ResInfoCreateXml.SelectSingleNode("FareRuleUrl").InnerText.Replace("/Avail/FareRule.aspx?", "").Replace("&", "^") + "^PMID=" + PMID));

                        //네이버에서 넘어 온 텍스와 유류할증료 및 프로모션정보 추가
                        int SumTax = 0;
                        int SumFsc = 0;
                        int PaxCount = 0;
                        int AdtFare = 0;
                        int NaverDiscount = cm.RequestInt(ArrPromtionAmount[0]);
                        int PaxNaverDiscount = 0;
                        int SumNaverDiscount = 0;

                        foreach (XmlNode PriceIndex in ResXml.SelectNodes("priceInfo/priceIndex"))
                        {
                            foreach (XmlNode PaxFare in PriceIndex.SelectNodes("paxFareGroup/paxFare"))
                            {
                                int baseRef1 = 0;
                                int baseRef2 = 1;

                                switch (PaxFare.Attributes.GetNamedItem("ptc").InnerText)
                                {
                                    case "CHD": baseRef1 = 2; baseRef2 = 2; break;
                                    case "INF": baseRef1 = 4; baseRef2 = 3; break;
                                    default: baseRef1 = 0; baseRef2 = 1; break;
                                }

                                PaxNaverDiscount = (NaverDiscount > 0 && ArrPromtionAmount[baseRef2].Trim().Equals("Y")) ? NaverDiscount : 0;

                                PaxFare.SelectSingleNode("amount").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverTax")).InnerText = ArrTax[baseRef1];
                                PaxFare.SelectSingleNode("amount").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverFsc")).InnerText = ArrTax[(baseRef1 + 1)];
                                PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = PaxNaverDiscount.ToString();
                                PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) - PaxNaverDiscount).ToString();

                                PaxCount = PaxFare.SelectNodes("traveler/ref").Count;

                                SumTax += (cm.RequestInt(ArrTax[baseRef1]) * PaxCount);
                                SumFsc += (cm.RequestInt(ArrTax[(baseRef1 + 1)]) * PaxCount);
                                SumNaverDiscount += (PaxNaverDiscount * PaxCount);

                                //성인요금
                                if (AdtFare.Equals(0) && PaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("ADT"))
                                    AdtFare = cm.RequestInt(PaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText);
                            }

                            PriceIndex.SelectSingleNode("summary").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverTax")).InnerText = SumTax.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverFsc")).InnerText = SumFsc.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disPartner").InnerText = SumNaverDiscount.ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = (cm.RequestInt(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText) - SumNaverDiscount).ToString();
                            PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = (cm.RequestInt(PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText) - SumNaverDiscount).ToString();
                        }

                        //네이버 프로모션 등록
                        if (!String.IsNullOrWhiteSpace(PromtionCode))
                        {
                            XmlNode PartnerPromotions = ResXml.OwnerDocument.CreateElement("partnerPromotions");
                            XmlNode PartnerPromotion = ResXml.OwnerDocument.CreateElement("promotion");
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("partner")).InnerText = "NAVER";
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("code")).InnerText = ArrPromtionCode[0];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("originalCode")).InnerText = PromtionCode;
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("amount")).InnerText = ArrPromtionAmount[0];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("adultYN")).InnerText = ArrPromtionAmount[1];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("childYN")).InnerText = ArrPromtionAmount[2];
                            PartnerPromotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("infantYN")).InnerText = ArrPromtionAmount[3];
                            PartnerPromotion.InnerText = PromtionName;

                            PartnerPromotions.AppendChild(PartnerPromotion);

                            if (ResXml.SelectNodes("priceInfo/priceIndex/promotionInfo").Count > 0)
                                ResXml.SelectSingleNode("priceInfo/priceIndex/promotionInfo/item").AppendChild(PartnerPromotions);
                            else
                            {
                                XmlNode PromotionInfo = ResXml.OwnerDocument.CreateElement("promotionInfo");
                                XmlNode Item = ResXml.OwnerDocument.CreateElement("item");
                                XmlNode Promotions = ResXml.OwnerDocument.CreateElement("promotions");
                                XmlNode Promotion = ResXml.OwnerDocument.CreateElement("Promotion");

                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("promotionId"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("siteNum"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("airCode"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("tripType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("cabinClass"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("bookingClass"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("paxType"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("discount"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("commission"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareDiscount"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentive"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentiveCode"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("incentiveName"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("fareTarget"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("childDiscountYN"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("promotionTL"));
                                Item.AppendChild((XmlNode)ResXml.OwnerDocument.CreateElement("NaverEventTypeCode"));

                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("promotionId"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("incentiveCode"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("incentiveName"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("fareTarget"));
                                Promotion.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("promotionTL"));

                                Promotions.AppendChild(Promotion);
                                Item.AppendChild(Promotions);
                                Item.AppendChild(PartnerPromotions);
                                PromotionInfo.AppendChild(Item);
                                ResXml.SelectSingleNode("priceInfo/priceIndex").AppendChild(PromotionInfo);
                            }
                        }

                        //판매가 재계산
                        XmlNode Summary = ResXml.SelectSingleNode("priceInfo/priceIndex/summary");
                        Summary.Attributes.GetNamedItem("price").InnerText = Convert.ToString(cm.RequestDouble(Summary.Attributes.GetNamedItem("disFare").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("tax").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("fsc").InnerText));
                        Summary.Attributes.Append((XmlAttribute)ResXml.OwnerDocument.CreateAttribute("naverPrice")).InnerText = Convert.ToString(cm.RequestDouble(Summary.Attributes.GetNamedItem("disFare").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("naverTax").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("naverFsc").InnerText));

                        return ResXml;
                    }
                    else
                        throw new Exception(ResInfoCreateXml.SelectSingleNode("ResultMsg").InnerText);
                }
                else
                    throw new Exception("정보가 부족합니다.");
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 392, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// [네이버] 갈릴레오 운임정보로 아마데우스 Fare + Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="GoodCode">상품코드</param>
        /// <param name="PromtionCode">네이버할인코드</param>
        /// <param name="PromtionName">네이버할인명</param>
        /// <param name="ArrSCity">출발지 공항 코드 배열</param>
        /// <param name="ArrECity">도착지 공항 코드 배열</param>
        /// <param name="ArrSDate">출발일 배열</param>
        /// <param name="ArrGoodCode">상품코드 배열</param>
        /// <param name="ArrEventCode">이벤트(프로모션)코드 배열</param>
        /// <param name="ArrPartnerNum">사이트할인코드 배열</param>
        /// <param name="ArrItinerary">여정정보 배열</param>
        /// <param name="ArrTax">텍스정보 배열</param>
        /// <param name="ArrPromtionAmount">네이버할인금액 배열</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <param name="PMID">프로모션코드</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="ResInfoCreateXml">갈릴레오 예약정보</param>
        /// <returns></returns>
        //[WebMethod(Description = "[네이버] 갈릴레오 운임정보로 아마데우스 Fare + Availability 조회")]
        public XmlElement SearchFareAvailforNaverRSAmadeus(int SNM, string SAC, string GoodCode, string PromtionCode, string PromtionName, string[] ArrSCity, string[] ArrECity, string[] ArrSDate, string[] ArrGoodCode, string[] ArrEventCode, string[] ArrPartnerNum, string[] ArrItinerary, string[] ArrTax, string[] ArrPromtionAmount, int ADC, int CHC, int IFC, string PMID, string GUID, XmlElement ResInfoCreateXml)
        {
            //MP통합 구조
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

            //네이버랜딩페이지 항공상세정보 출력을 위한 속성 추가(Seg별 비행시간, 기착지 지상 대기시간)
            XmlNode AddAttrSeg = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo/flightIndex/segGroup/seg");
            AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
            AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));
            AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
            AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));

            //갈릴레오
            XmlNode FareInfo = ResInfoCreateXml.SelectSingleNode("FareInfo");

            //여정정보
            XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
            XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
            XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
            XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
            XmlNode StopSegment = Segment.SelectSingleNode("seg");

            XmlNode NewFlightIndex;
            XmlNode NewSegmentGroup;
            XmlNode NewSegment;
            XmlNode NewStopSegment;

            string AvailCLC = string.Empty;
            string AvailSCD = string.Empty;
            string AvailMCC = string.Empty;
            string AvailFLN = string.Empty;
            string AvailNOS = string.Empty;
            int x = 0;

            foreach (string SegGroup in ArrItinerary)
            {
                if (!String.IsNullOrEmpty(SegGroup))
                {
                    string[] SegInfos = SegGroup.Split(',');
                    int SegInfosLen = SegInfos.Length - 1;

                    AvailCLC = "";
                    AvailSCD = "";
                    AvailMCC = "";
                    AvailFLN = "";
                    AvailNOS = "";

                    for (int n = 0; n < SegInfosLen; n++)
                    {
                        string[] SegInfo = SegInfos[n].Split('^');

                        if (n > 0)
                        {
                            AvailCLC = SegInfo[0];
                            AvailSCD += ",";
                            AvailMCC += ",";
                            AvailFLN += ",";
                        }

                        AvailSCD += SegInfo[12];
                        AvailMCC += SegInfo[10];
                        AvailFLN += SegInfo[11];
                        AvailNOS = SegInfo[14];
                    }

                    //Availability 조회
                    XmlElement AvailXml = mas.AvailabilityRS(SNM, ArrSDate[x], "", ArrSCity[x], ArrECity[x], AvailCLC, AvailSCD, AvailMCC, AvailFLN, AvailNOS);
                        
                    if (AvailXml == null || AvailXml.SelectNodes("flightInfo").Count.Equals(0))
                        throw new Exception("선택하신 항공편에 대한 스케쥴 정보가 존재하지 않습니다.");

                    XmlNode AvailSegGroup = AvailXml.SelectSingleNode("flightInfo/segGroup");
                    string CDS = "N";

                    //여정정보
                    NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                    NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = (x + 1).ToString();

                    NewSegmentGroup = NewFlightIndex.AppendChild(SegmentGroup.CloneNode(false));
                    NewSegmentGroup.Attributes.GetNamedItem("ref").InnerText = "1";

                    foreach (XmlNode AvailSeg in AvailXml.SelectNodes("flightInfo/segGroup/seg"))
                    {
                        //비행 상세 스케쥴 정보
                        XmlElement FlightInfoXml = mas.FlightInfoRS(SNM, AvailSeg.Attributes.GetNamedItem("ddt").InnerText, "", AvailSeg.Attributes.GetNamedItem("dlc").InnerText, AvailSeg.Attributes.GetNamedItem("alc").InnerText, AvailSeg.Attributes.GetNamedItem("mcc").InnerText, "", AvailSeg.Attributes.GetNamedItem("fln").InnerText);

                        NewSegment = NewSegmentGroup.AppendChild(Segment.CloneNode(false));
                        NewSegment.Attributes.GetNamedItem("dlc").InnerText = AvailSeg.Attributes.GetNamedItem("dlc").InnerText;
                        NewSegment.Attributes.GetNamedItem("alc").InnerText = AvailSeg.Attributes.GetNamedItem("alc").InnerText;
                        NewSegment.Attributes.GetNamedItem("ddt").InnerText = AvailSeg.Attributes.GetNamedItem("ddt").InnerText;
                        NewSegment.Attributes.GetNamedItem("ardt").InnerText = AvailSeg.Attributes.GetNamedItem("ardt").InnerText;
                        NewSegment.Attributes.GetNamedItem("mcc").InnerText = AvailSeg.Attributes.GetNamedItem("mcc").InnerText;
                        NewSegment.Attributes.GetNamedItem("occ").InnerText = String.IsNullOrWhiteSpace(AvailSeg.Attributes.GetNamedItem("occ").InnerText) ? AvailSeg.Attributes.GetNamedItem("mcc").InnerText : AvailSeg.Attributes.GetNamedItem("occ").InnerText;
                        NewSegment.Attributes.GetNamedItem("fln").InnerText = AvailSeg.Attributes.GetNamedItem("fln").InnerText;
                        NewSegment.Attributes.GetNamedItem("eqt").InnerText = AvailSeg.Attributes.GetNamedItem("eqt").InnerText;
                        NewSegment.Attributes.GetNamedItem("stn").InnerText = cm.RequestInt(AvailSeg.Attributes.GetNamedItem("stn").InnerText).ToString();
                        NewSegment.Attributes.GetNamedItem("etc").InnerText = "";
                            
                        //부킹클래스 추가
                        XmlAttribute AttrRBD = XmlDoc.CreateAttribute("rbd");
                        AttrRBD.InnerText = AvailSeg.SelectSingleNode("svcClass").Attributes.GetNamedItem("rbd").InnerText;
                        NewSegment.Attributes.Append(AttrRBD);

                        //예약가능 좌석수 추가
                        XmlAttribute AttrAVL = XmlDoc.CreateAttribute("avl");
                        AttrAVL.InnerText = AvailSeg.SelectSingleNode("svcClass").Attributes.GetNamedItem("avl").InnerText;
                        NewSegment.Attributes.Append(AttrAVL);

                        if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                        {
                            if (FlightInfoXml.SelectNodes("flightInfo").Count > 0)
                            {
                                NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                                NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("dlc").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("alc").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("alc").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("ddt").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("ardt").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("eft").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("eft").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("gwt").InnerText;

                                NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                                NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("dlc").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("alc").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("alc").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("ddt").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("ardt").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("eft").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("eft").InnerText;
                                NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("gwt").InnerText;

                                if (!String.IsNullOrWhiteSpace(NewSegment.SelectSingleNode("seg[1]").Attributes.GetNamedItem("eft").InnerText) && !String.IsNullOrWhiteSpace(NewSegment.SelectSingleNode("seg[2]").Attributes.GetNamedItem("eft").InnerText))
                                    NewSegment.Attributes.GetNamedItem("eft").InnerText = cm.SumElapseFlyingTime(cm.ConvertToDateTime(FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("eft").InnerText), cm.ConvertToDateTime(FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("eft").InnerText), "").Replace(":", "");
                                else
                                    NewSegment.Attributes.GetNamedItem("eft").InnerText = String.Concat(NewSegment.SelectSingleNode("seg[1]").Attributes.GetNamedItem("eft").InnerText, NewSegment.SelectSingleNode("seg[2]").Attributes.GetNamedItem("eft").InnerText);

                                if (!String.IsNullOrWhiteSpace(NewSegment.SelectSingleNode("seg[1]").Attributes.GetNamedItem("gwt").InnerText) && !String.IsNullOrWhiteSpace(NewSegment.SelectSingleNode("seg[2]").Attributes.GetNamedItem("gwt").InnerText))
                                    NewSegment.Attributes.GetNamedItem("gwt").InnerText = cm.SumElapseFlyingTime(cm.ConvertToDateTime(FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("gwt").InnerText), cm.ConvertToDateTime(FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[2]").Attributes.GetNamedItem("gwt").InnerText), "").Replace(":", "");
                                else
                                    NewSegment.Attributes.GetNamedItem("gwt").InnerText = String.Concat(NewSegment.SelectSingleNode("seg[1]").Attributes.GetNamedItem("gwt").InnerText, NewSegment.SelectSingleNode("seg[2]").Attributes.GetNamedItem("gwt").InnerText);
                            }
                        }
                        else
                        {
                            NewSegment.Attributes.GetNamedItem("eft").InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg[1]").Attributes.GetNamedItem("eft").InnerText;
                            NewSegment.Attributes.GetNamedItem("gwt").InnerText = "";
                        }

                        //공동운항 여부
                        if (CDS.Equals("N") && (NewSegment.Attributes.GetNamedItem("mcc").InnerText != NewSegment.Attributes.GetNamedItem("occ").InnerText))
                            CDS = "Y";
                    }

                    NewSegmentGroup.Attributes.GetNamedItem("eft").InnerText = AvailSegGroup.Attributes.GetNamedItem("eft").InnerText;
                    NewSegmentGroup.Attributes.GetNamedItem("ewt").InnerText = cm.ElapseWaitingTime(NewSegmentGroup).Replace(":", "");
                    NewSegmentGroup.Attributes.GetNamedItem("mjc").InnerText = SAC;
                    NewSegmentGroup.Attributes.GetNamedItem("cds").InnerText = CDS;
                    NewSegmentGroup.Attributes.GetNamedItem("nosp").InnerText = AvailXml.SelectNodes("flightInfo/segGroup/seg").Count.ToString();

                    x++;
                }
            }

            FlightInfo.RemoveChild(FlightIndex);

            if (FlightInfo.HasChildNodes)
            {
                //운임정보
                XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
                XmlNode PriceIndex;

                int SegCount = FlightInfo.SelectNodes("flightIndex/segGroup/seg").Count;
                int[] INO = new Int32[SegCount];
                string[] DTD = new String[SegCount];
                string[] DTT = new String[SegCount];
                string[] ARD = new String[SegCount];
                string[] ART = new String[SegCount];
                string[] DLC = new String[SegCount];
                string[] ALC = new String[SegCount];
                string[] MCC = new String[SegCount];
                string[] OCC = new String[SegCount];
                string[] FLN = new String[SegCount];
                string[] RBD = new String[SegCount];
                string[] PTC = new String[3] { "ADT", "CHD", "INF" };
                int[] NOP = new Int32[3] { ADC, CHC, IFC };
                int idx = 0;
                int SegIdx = 1;

                foreach (XmlNode TmpFlightIndex in FlightInfo.SelectNodes("flightIndex"))
                {
                    foreach (XmlNode TmpSegGroup in TmpFlightIndex.SelectNodes("segGroup"))
                    {
                        foreach (XmlNode TmpSeg in TmpSegGroup.SelectNodes("seg"))
                        {
                            INO[idx] = SegIdx;
                            DTD[idx] = TmpSeg.Attributes.GetNamedItem("ddt").InnerText.Substring(0, 10);
                            DTT[idx] = cm.Right(TmpSeg.Attributes.GetNamedItem("ddt").InnerText, 5);
                            ARD[idx] = TmpSeg.Attributes.GetNamedItem("ardt").InnerText.Substring(0, 10);
                            ART[idx] = cm.Right(TmpSeg.Attributes.GetNamedItem("ardt").InnerText, 5);
                            DLC[idx] = TmpSeg.Attributes.GetNamedItem("dlc").InnerText;
                            ALC[idx] = TmpSeg.Attributes.GetNamedItem("alc").InnerText;
                            MCC[idx] = TmpSeg.Attributes.GetNamedItem("mcc").InnerText;
                            OCC[idx] = TmpSeg.Attributes.GetNamedItem("occ").InnerText;
                            FLN[idx] = TmpSeg.Attributes.GetNamedItem("fln").InnerText;
                            RBD[idx] = TmpSeg.Attributes.GetNamedItem("rbd").InnerText;

                            idx++;
                        }
                    }

                    SegIdx++;
                }

                XmlElement FareXml = mas.SearchFareScheduleRS(SNM, PMID, "RU", INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PTC, NOP);
                cm.XmlFileSave(FareXml, "Mode", "SearchFareScheduleRS", "N", GUID);

                if (FareXml.SelectNodes("errorMessageText").Count.Equals(0))
                {
                    //스케쥴 조회 XML에 운임조회 XML 삽입
                    PriceInfo.ReplaceChild(XmlDoc.ImportNode(FareXml.SelectSingleNode("priceInfo/priceIndex"), true), PriceInfo.SelectSingleNode("priceIndex"));
                    PriceIndex = PriceInfo.SelectSingleNode("priceIndex");

                    //갈릴레오 운임 정보로 처리(아마데우스에선 해당 데이타가 존재하지 않기 때문)
                    PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("mas").InnerText = FareInfo.SelectSingleNode("ValidateMax").InnerText;
                    PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ttl").InnerText = String.IsNullOrWhiteSpace(FareInfo.SelectSingleNode("TktLimitDate").InnerText) ? DateTime.Now.AddDays(10).ToString("yyyy-MM-dd") : cm.ConvertToDateTime(FareInfo.SelectSingleNode("TktLimitDate").InnerText);

                    XmlNode Seg = PriceIndex.SelectSingleNode("segGroup/seg");
                    XmlNode SegRef = Seg.SelectSingleNode("ref");
                    XmlNode NewSegRef;

                    foreach (XmlNode TmpFlightIndex in FlightInfo.SelectNodes("flightIndex"))
                    {
                        NewSegRef = Seg.AppendChild(SegRef.Clone());
                        NewSegRef.Attributes.GetNamedItem("fiRef").InnerText = TmpFlightIndex.Attributes.GetNamedItem("ref").InnerText;
                        NewSegRef.Attributes.GetNamedItem("nosp").InnerText = TmpFlightIndex.SelectSingleNode("segGroup").Attributes.GetNamedItem("nosp").InnerText;
                        NewSegRef.InnerText = TmpFlightIndex.SelectSingleNode("segGroup").Attributes.GetNamedItem("ref").InnerText;
                    }

                    Seg.RemoveChild(SegRef);

                    int SeatAvail = ADC + CHC; //필요좌석수
                    int SegStep = 1;
                    string Status = "HK";

                    foreach (XmlNode TmpFlightIndex in FlightInfo.SelectNodes("flightIndex"))
                    {
                        SegStep = 1;
                        foreach (XmlNode TmpSeg in TmpFlightIndex.SelectNodes("segGroup/seg"))
                        {
                            foreach (XmlNode TmpFare in PriceIndex.SelectNodes(String.Format("paxFareGroup/*/segFareGroup/segFare[@ref='{0}']/fare[{1}]", TmpFlightIndex.Attributes.GetNamedItem("ref").InnerText, SegStep)))
                            {
                                TmpFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = TmpSeg.Attributes.GetNamedItem("avl").InnerText;
                            }

                            if (SeatAvail > Convert.ToInt32(TmpSeg.Attributes.GetNamedItem("avl").InnerText))
                                Status = "HL";

                            SegStep++;
                        }
                    }

                    //상태
                    XmlAttribute AttrStatus = XmlDoc.CreateAttribute("status");
                    AttrStatus.InnerText = Status;
                    PriceIndex.SelectSingleNode("summary").Attributes.Append(AttrStatus);

                    cm.XmlFileSave(XmlDoc, "Mode", "SearchFareAvailforNaverRSAmadeus", "N", GUID);

                    return XmlDoc.DocumentElement;
                }
                else
                    throw new Exception(FareXml.SelectSingleNode("errorMessageText/description").InnerText);
            }
            else
                throw new Exception("스케쥴정보가 없습니다.");
        }

        #endregion "네이버(갈릴레오)"

        #endregion "네이버"

        #region "카약"

        /// <summary>
        /// [카약] Fare + Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호(4713,4820)</param>
        /// <param name="SessionIdx">세션인덱스(ResponseDetails/@ref)</param>
        /// <param name="GDS">GDS(priceIndex/@gds)</param>
        /// <param name="PTC">Passenger Type Code(priceIndex/@ptc)</param>
        /// <param name="PriceRef">여정인덱스(priceIndex/@ref)</param>
        /// <param name="Itinerary">여정인덱스(flightIndex/segGroup/@ref)(여정순서대로 콤마로 구분하여 입력)(ex: 2001,2001)</param>
        /// <returns></returns>
        [WebMethod(Description = "[카약] Fare + Availability 조회")]
        public XmlElement SearchFareAvailforKayakRS(int SNM, string SessionRef, string GDS, string PTC, string PriceRef, string Itinerary)
        {
            //FXL 대체 :  ResponseDetails/@sessionIdx, priceIndex/@gds + @ptc + @ref
            //SXL 대체 :  flightInfo/@ptc, segGroup/@ref

            /*
             * SNM=4713
             * SessionRef=34
             * GDS=Amadeus
             * PTC=ADT
             * PriceRef=2001
             * Itinerary=2001,2001
             * ADC=1
             * CHC=0
             * IFC=0
            */

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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 473;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SessionRef;
                sqlParam[8].Value = GDS;
                sqlParam[9].Value = PTC;
                sqlParam[10].Value = PriceRef;
                sqlParam[11].Value = Itinerary;

                log.LogDBSave(sqlParam);
            }
            finally { }

            try
            {
                string FXL = string.Empty;
                string SXL = string.Empty;
                string[] ArrSegGroup = Itinerary.Split(',');
                
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                    {
                        SqlDataReader dr = null;

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_S_항공검색_항공검색번호";

                        cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@항공검색번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@사이트번호"].Value = SNM;
                        cmd.Parameters["@항공검색번호"].Value = SessionRef;
                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        try
                        {
                            conn.Open();
                            dr = cmd.ExecuteReader();

                            if (dr.Read())
                            {
                                FXL = dr["FXL"].ToString();
                                SXL = dr["SXL"].ToString();
                            }
                        }
                        finally
                        {
                            dr.Dispose();
                            dr.Close();
                            conn.Close();
                        }
                    }

                    if (cmd.Parameters["@결과"].Value.ToString().Equals("S"))
                    {
                        XmlDocument XmlDoc = new XmlDocument();
                        XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                        XmlNode ResponseDetails = XmlDoc.SelectSingleNode("ResponseDetails");
                        XmlNode FlightInfo = ResponseDetails.SelectSingleNode("flightInfo");
                        XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
                        XmlNode PriceInfo = ResponseDetails.SelectSingleNode("priceInfo");

                        XmlNode NewFlightIndex;

                        ResponseDetails.Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                        ResponseDetails.Attributes.GetNamedItem("ref").InnerText = SessionRef;

                        if (!String.IsNullOrWhiteSpace(FXL))
                        {
                            XmlDocument XmlFxl = new XmlDocument();
                            XmlFxl.LoadXml(FXL);

                            if (ArrSegGroup.Length.Equals(XmlFxl.SelectNodes("priceIndex/segGroup/seg/ref").Count))
                            {
                                foreach (XmlNode Seg in XmlFxl.SelectNodes("priceIndex/segGroup/seg"))
                                {
                                    for (int i = 0; i < ArrSegGroup.Length; i++)
                                    {
                                        if (Seg.SelectSingleNode(String.Format("ref[@fiRef='{0}']", (i + 1))).InnerText != ArrSegGroup[i].Trim())
                                        {
                                            XmlFxl.SelectSingleNode("priceIndex/segGroup").RemoveChild(Seg);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                                throw new Exception("여정 정보가 정확하지 않습니다.");
                            
                            PriceInfo.ReplaceChild(XmlDoc.ImportNode(XmlFxl.FirstChild, true), PriceInfo.SelectSingleNode("priceIndex"));
                        }

                        if (!String.IsNullOrWhiteSpace(SXL))
                        {
                            XmlDocument XmlSxl = new XmlDocument();
                            XmlSxl.LoadXml(SXL);

                            int i = 1;
                            foreach (XmlNode SegGroup in XmlSxl.SelectNodes("itinerary/segGroup"))
                            {
                                NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                                NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = (i++).ToString();

                                NewFlightIndex.AppendChild(XmlDoc.ImportNode(SegGroup, true));
                            }

                            FlightInfo.RemoveChild(FlightIndex);
                        }

                        return XmlDoc.DocumentElement;
                    }
                    else
                    {
                        throw new Exception(cmd.Parameters["@에러메시지"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 473, 0, 0).ToErrors;
            }
        }

        #endregion "카약"

        #region "이베이"

        #region "랜딩페이지용"

        //[WebMethod(Description = "[이베이] 랜딩페이지용")]
        public XmlElement CheckBookingInfoEbayRSTEST()
        {
            XmlDocument XmlSegHold = new XmlDocument();
            XmlSegHold.Load(mc.XmlFullPath("_SegHold"));
            
            XmlElement SegHold = XmlSegHold.DocumentElement;

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

            //랜딩페이지 항공상세정보 출력을 위한 속성 추가(Seg별 비행시간, 기착지 지상 대기시간)
            XmlNode AddAttrSeg = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo/flightIndex/segGroup/seg");
            AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
            AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));
            AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
            AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));

            //여정정보
            XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
            XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
            XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
            XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
            XmlNode StopSegment = Segment.SelectSingleNode("seg");

            XmlNode NewFlightIndex;
            XmlNode NewSegmentGroup;

            string CCD = SegHold.SelectSingleNode("seg_detail_t/cabin").InnerText;
            int CCDRef = Common.RefOverride("Sabre", CCD);
            int PriceRef = 1;
            string SegGroupNo = string.Empty;

            //탑승객타입
            string[] PTC = new String[3] { SegHold.SelectSingleNode("reply_if_t[1]/idt_code").InnerText, "CHD", "INF" };

            //인원
            int[] NOP = new Int32[3] { Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_adt_pax").InnerText), Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_chd_pax").InnerText), Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_inf_pax").InnerText) };

            //발권항공사
            string ValidatingCarrier = SegHold.SelectSingleNode("reply_if_t/tkt_car").InnerText;

            //한국출발여부
            bool DepartureFromKorea = Common.KoreaOfAirport(SegHold.SelectSingleNode("seg_detail_t[1]/dep_city").InnerText);

            FlightInfo.Attributes.GetNamedItem("ptc").InnerText = PTC[0];
            FlightInfo.Attributes.GetNamedItem("rot").InnerText = SegHold.SelectSingleNode("pnr_common_data_t/trip_type").InnerText;
            FlightInfo.Attributes.GetNamedItem("opn").InnerText = SegHold.SelectNodes("reply_if_t[open_ind='Y']").Count > 0 ? "Y" : "N";

            foreach (XmlNode AirSegDetail in SegHold.SelectNodes("seg_detail_t[not(group_seq=preceding-sibling::seg_detail_t/group_seq)]"))
            {
                int RefNo = Convert.ToInt32(AirSegDetail.SelectSingleNode("group_seq").InnerText);

                NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = (RefNo + 1).ToString();

                NewSegmentGroup = NewFlightIndex.AppendChild(ToModeSegGroupSabre(CCD, SegmentGroup.CloneNode(true), SegHold.SelectNodes(String.Format("seg_detail_t[group_seq='{0}']", RefNo))));
            }

            FlightInfo.RemoveChild(FlightIndex);

            return XmlDoc.DocumentElement;
        }
        
        /// <summary>
        /// [이베이] 랜딩용 서비스
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="ParamValue">이베이에서 넘어온 파라미터 값</param>
        /// <param name="CouponType">아이템할인:쿠폰종류(I:Item,B:Buyer)</param>
        /// <param name="CouponNumber">아이템할인:쿠폰번호</param>
        /// <param name="DCType">아이템할인:할인방식(A:금액,R:%)</param>
        /// <param name="DCAmt">아이템할인:할인금액</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[이베이] 랜딩페이지용")]
        public XmlElement CheckBookingInfoEbayRS(int SNM, string ParamValue, string CouponType, string CouponNumber, string DCType, string DCAmt, string RQT)
        {
            int ServiceNumber = 626;
            string LogGUID = cm.GetGUID;
            string GUID = LogGUID;

            //임시테스트용
            if (String.IsNullOrWhiteSpace(ParamValue))
            {
                ParamValue = "trip=RT&scroll=1&row=9999&segdt=Y&ctrl=NO&comp=Y&CBFare=YY&CBClass=&CBIdt=&DBCharSet=UTF-8&CharSet=UTF-8&L1Code=2000&GroupCode=FG12&L1SiteType=W_GMARKET&GscCode=GMARKET&L2Code=&L2Grade=&L2SiteCode=&TLType=CT&MobileInd=N&CRuleType=B&dspmode=xml&rqitin=1&fbd=SEL&fba=SEA&py=N&dep0=SEL&arr0=SEA&depdate0=20191013&retdate=20191130&dep1=&arr1=&depdate1=20191130&dep2=&arr2=&depdate2=&dep3=&arr3=&depdate3=&car=YY&idt=ALL&tpno=99&adt=1&chd=0&inf=0&inf_st=0&sort=F&CityCheckType=B&LangCode=KOR&Pricing=KRW&header=ID&origin=MODETOUR&fare_origin=MODETOUR&cuid=&carsel=1&tasfOrigin=MODETOUR_GM&FareSource=G&actiontype=itin&ParmType=Inline&itindep00=GMP&itinarr00=SEA&itinfrbs00=NLW08YN0/ST1&itinbkcls00=N&itinaccode00=&itingovcar00=JL&itindir00=F&itindepdate00=20191013&itinbgrid00=&itinbgrgroupid00=&itindep01=SEA&itinarr01=GMP&itinfrbs01=NLW08YN0/ST1&itinbkcls01=N&itinaccode01=&itingovcar01=JL&itindir01=T&itindepdate01=20191130&itinbgrid01=&itinbgrgroupid01=&bfmpoolpd=5GH8&bfmltd=20190430&bfmPrivate=Y&afsrId=&mstay=&Segbkcls=GMP-JL(N)-HND^NRT-JL(N)-SEA^SEA-JL(N)-NRT^HND-JL(N)-GMP^&fareType=S&pdsc=&BfmRule=789C65514D6FC2300CFD2B55CE2EC46E19694E7C1536565244C3069AC665E230094D93D04EC07F9F5D88286A1A29F6CBF3F34B7D52B34A59A5408DC77C6E39584E953DA9A5E7B4F26B0646330E678BE5F9D94DCEF3E24CD1EFF7FE6B7FDCB9953F57F9F001935CF026267552DFC058763A64D98C0C690DFC31B2B95979132BF34238B998829FBFC341B2D1D508B0200C40A2D71570475857305D950B9817E000C115EFDA6C9DEE561E819738AD7903A8574DA3364D28421D04D15B0B5F5E4B92768958115653396DD1C4BC3CAACA8B685E449BAEDF9612709B68912276287B848B00BBF5D810757A2677936855E688643A89E99B3E6B0E79542AF48A1998B4FEE94B98E305942F38268D59ACD33891DB727254F6E3C467B8411D63E291ACD6BCA189A5166BCC414610C6C00FE60D4300DC35C9C6EADE8340EA7572C31C3C190823B90B107B7CB0826CD2235A6A28D518A1ED05A53E84A9DD95929B158A357ACC6CDA10A831224BE12D3D0833BC0BA4EAF279F9078B91BE35&BfmInd=Y&ri_header=JL^STU^KRW^928200^928200^0^0^0^0^N^268800^0^0^113200^0^0^0^0^0^Y^^^^^^^^^^^^^^^0^0^0^0^Y^^^^^^^^^^^^^0.0^0.0^0.0^^^^^^^^^0.0^0.0^0.0^^0^0^0^^^^^&sltdseghd0=1505/1110/0355/MIL//5503/&sltdseg00=JL//0092/GMP//HND//201910131200////201910131410////1////0/0210/0210/0000/MIL/734////////0/772//////O/9/N///Y//&sltdcmd00=&sltdseg01=JL//0068/NRT//SEA//201910131805////201910131105////2////0/0900/0900/0355/MIL/4769////////0/788//////O/9/Y///Y//&sltdcmd01=&sltdseghd1=1755/1305/0450/MIL//5503/&sltdseg10=JL//0067/SEA//NRT//201911301125////201912011455////1////0/1030/1030/0000/MIL/4769////////0/788//////N/9/N///Y//&sltdcmd10=&sltdseg11=JL//0095/HND//GMP//201912011945////201912012220////2////0/0235/0235/0450/MIL/734////////0/788//////N/9/Y///Y//&sltdcmd11=&FS1CLS=O9-O9&FS2CLS=N9-N9&FS3CLS=&FS4CLS=";
                CouponType = "";
                CouponNumber = "";
                DCType = "";
                DCAmt = "";
            }

            //'&amp;'형태로 넘어오는 경우 '&'변경
            ParamValue = Server.HtmlDecode(ParamValue);

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
                        new SqlParameter("@요청2", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청3", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청4", SqlDbType.VarChar, 3000),
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = CouponType;
                sqlParam[8].Value = CouponNumber;
                sqlParam[9].Value = DCType;
                sqlParam[10].Value = DCAmt;
                sqlParam[11].Value = ParamValue;

                log.LogDBSave(sqlParam);
            }
            finally { }

            try
            {
                //BFM 유효성 체크
                if (ParamValue.IndexOf("&BfmInd=Y") != -1)
                {
                    //유효성 체크 오류로 사용 중단(2018-09-14,투어소프트요청)
                    //XmlElement RevailDate = sas.RevailDateRS(ParamValue, String.Concat(GUID, "-01"));

                    //if (RevailDate.SelectNodes("FARE").Count.Equals(0))
                    //    throw new Exception("선택하신 여정에 대한 항공운임을 확인할 수 없습니다. - 001");
                }
                else
                {
                    //BFM이 아닌 경우 FareCheck=Y 파라미터 추가(2018-08-30,세이버요청)
                    ParamValue = String.Concat(ParamValue, "&FareCheck=Y");
                }
                
                //예약 전처리
                XmlElement SegHold = sas.SegHoldBFMRS(ParamValue, String.Concat(GUID, "-02"));

                //오류
                if (SegHold.SelectSingleNode("error_no").InnerText != "0")
                    throw new Exception(SegHold.SelectSingleNode("error_desc").InnerText);

                //요금 변동사항 체크
                if (SegHold.SelectNodes("reply_if_t/fare_change").Count > 0)
                {
                    //요금변동 발생
                    if (SegHold.SelectSingleNode("reply_if_t/fare_change").InnerText.Equals("Y"))
                        throw new Exception("선택하신 여정에 대한 항공운임을 확인할 수 없습니다. - 002");
                    //사용할 수 있는 요금 없음
                    else if (SegHold.SelectSingleNode("reply_if_t/fare_change").InnerText.Equals("X"))
                        throw new Exception("선택하신 여정에 대한 항공운임을 확인할 수 없습니다. - 003");
                }
                
                //아이템할인금액(성인1인 기준)
                int DCAdult = String.IsNullOrWhiteSpace(DCAmt) ? 0 : Convert.ToInt32(aes.AESDecrypt("Ebay", DCAmt));

                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

                //랜딩페이지 항공상세정보 출력을 위한 속성 추가(Seg별 비행시간, 기착지 지상 대기시간)
                XmlNode AddAttrSeg = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo/flightIndex/segGroup/seg");
                AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
                AddAttrSeg.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));
                AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("eft"));
                AddAttrSeg.SelectSingleNode("seg").Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("gwt"));

                //여정정보
                XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
                XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
                XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
                XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
                XmlNode StopSegment = Segment.SelectSingleNode("seg");

                XmlNode NewFlightIndex;
                XmlNode NewSegmentGroup;

                string CCD = SegHold.SelectSingleNode("seg_detail_t/cabin").InnerText;
                int CCDRef = Common.RefOverride("Sabre", CCD);
                int PriceRef = 1;
                string SegGroupNo = string.Empty;

                //탑승객타입
                string[] PTC = new String[3] { SegHold.SelectSingleNode("reply_if_t[1]/idt_code").InnerText, "CHD", "INF" };

                //인원
                int[] NOP = new Int32[3] { Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_adt_pax").InnerText), Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_chd_pax").InnerText), Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_inf_pax").InnerText) };

                //발권항공사
                string ValidatingCarrier = SegHold.SelectSingleNode("reply_if_t/tkt_car").InnerText;

                //한국출발여부
                bool DepartureFromKorea = Common.KoreaOfAirport(SegHold.SelectSingleNode("seg_detail_t[1]/dep_city").InnerText);

                FlightInfo.Attributes.GetNamedItem("ptc").InnerText = PTC[0];
                FlightInfo.Attributes.GetNamedItem("rot").InnerText = SegHold.SelectSingleNode("pnr_common_data_t/trip_type").InnerText;
                FlightInfo.Attributes.GetNamedItem("opn").InnerText = SegHold.SelectNodes("reply_if_t[open_ind='Y']").Count > 0 ? "Y" : "N";

                foreach (XmlNode AirSegDetail in SegHold.SelectNodes("seg_detail_t[not(group_seq=preceding-sibling::seg_detail_t/group_seq)]"))
                {
                    int RefNo = Convert.ToInt32(AirSegDetail.SelectSingleNode("group_seq").InnerText);

                    NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                    NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = (RefNo + 1).ToString();

                    NewSegmentGroup = NewFlightIndex.AppendChild(ToModeSegGroupSabre(CCD, SegmentGroup.CloneNode(true), SegHold.SelectNodes(String.Format("seg_detail_t[group_seq='{0}']", RefNo))));
                }

                FlightInfo.RemoveChild(FlightIndex);

                //운임정보
                XmlNode FareInfo = SegHold.SelectSingleNode("reply_if_t");

                XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
                XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");
                XmlNode Seg = PriceIndex.SelectSingleNode("segGroup/seg");
                XmlNode SegRef = Seg.SelectSingleNode("ref");
                XmlNode PaxFareGroup = PriceIndex.SelectSingleNode("paxFareGroup");
                XmlNode PaxFare = PaxFareGroup.SelectSingleNode("paxFare");
                XmlNode SegFareGroup;
                XmlNode SegFare;
                XmlNode Fare;
                XmlNode Traveler;
                XmlNode TravelerRef;

                XmlNode NewSegRef;
                XmlNode NewPaxFare;
                XmlNode NewSegFare;
                XmlNode NewFare;
                XmlNode NewTravelerRef;

                double TotalFare = 0;
                double TotalDisFare = 0;
                double TotalTax = 0;
                double TotalFsc = 0;
                double TotalMTasf = 0;
                double TotalATasf = 0;
                double AdtTasf = Convert.ToDouble(FareInfo.SelectSingleNode("adt_tfee").InnerText);
                double ChdTasf = Convert.ToDouble(FareInfo.SelectSingleNode("chd_tfee").InnerText);
                double InfTasf = Convert.ToDouble(FareInfo.SelectSingleNode("inf_tfee").InnerText);
                bool UseTASF = Common.ApplyTASF(SNM, ValidatingCarrier);
                double TASF = 0;
                double Tax = 0;
                double Fuel = 0;
                double QChrg = 0;

                string Status = "HK";
                int SegStep = 0;

                PriceIndex.Attributes.GetNamedItem("gds").InnerText = "Sabre";
                PriceIndex.Attributes.GetNamedItem("ptc").InnerText = PTC[0];
                PriceIndex.Attributes.GetNamedItem("ref").InnerText = PriceRef.ToString();

                foreach (XmlNode TmpFlightIndex in FlightInfo.SelectNodes("flightIndex"))
                {
                    NewSegRef = Seg.AppendChild(SegRef.Clone());
                    NewSegRef.Attributes.GetNamedItem("fiRef").InnerText = TmpFlightIndex.Attributes.GetNamedItem("ref").InnerText;
                    NewSegRef.Attributes.GetNamedItem("nosp").InnerText = TmpFlightIndex.SelectSingleNode("segGroup").Attributes.GetNamedItem("nosp").InnerText;
                    NewSegRef.InnerText = TmpFlightIndex.SelectSingleNode("segGroup").Attributes.GetNamedItem("ref").InnerText;
                }

                Seg.RemoveChild(SegRef);

                for (int i = 0; i < PTC.Length; i++)
                {
                    if (NOP[i] > 0)
                    {
                        NewPaxFare = PaxFareGroup.AppendChild(PaxFare.Clone());
                        NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = PTC[i];

                        SegFareGroup = NewPaxFare.SelectSingleNode("segFareGroup");
                        SegFare = SegFareGroup.SelectSingleNode("segFare");
                        Fare = SegFare.SelectSingleNode("fare");
                        SegStep = 0;

                        TASF = UseTASF ? Common.GetTASF(SNM, PTC[i], ValidatingCarrier, DepartureFromKorea) : 0;

                        if (PTC[i].Equals("CHD"))
                        {
                            Tax = Convert.ToDouble(FareInfo.SelectSingleNode("chd_tax").InnerText);
                            Fuel = Convert.ToDouble(FareInfo.SelectSingleNode("chd_fuel").InnerText);
                            QChrg = Convert.ToDouble(FareInfo.SelectSingleNode("chd_qchrg").InnerText);

                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = FareInfo.SelectSingleNode("chd_fare").InnerText.Split('/')[0];
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = FareInfo.SelectSingleNode("chd_disc_fare").InnerText.Split('/')[0];
                            //NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel - QChrg).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = FareInfo.SelectSingleNode("chd_fuel").InnerText;
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = "0";
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tasf").InnerText = (ChdTasf + TASF).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("mTasf").InnerText = TASF.ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("aTasf").InnerText = ChdTasf.ToString();
                        }
                        else if (PTC[i].Equals("INF"))
                        {
                            Tax = Convert.ToDouble(FareInfo.SelectSingleNode("inf_tax").InnerText);
                            Fuel = Convert.ToDouble(FareInfo.SelectSingleNode("inf_fuel").InnerText);
                            QChrg = Convert.ToDouble(FareInfo.SelectSingleNode("inf_qchrg").InnerText);

                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = FareInfo.SelectSingleNode("inf_fare").InnerText.Split('/')[0];
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = FareInfo.SelectSingleNode("inf_disc_fare").InnerText.Split('/')[0];
                            //NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel - QChrg).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = FareInfo.SelectSingleNode("inf_fuel").InnerText;
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = "0";
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tasf").InnerText = (InfTasf + TASF).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("mTasf").InnerText = TASF.ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("aTasf").InnerText = InfTasf.ToString();
                        }
                        else
                        {
                            Tax = Convert.ToDouble(FareInfo.SelectSingleNode("adt_tax").InnerText);
                            Fuel = Convert.ToDouble(FareInfo.SelectSingleNode("adt_fuel").InnerText);
                            QChrg = Convert.ToDouble(FareInfo.SelectSingleNode("adt_qchrg").InnerText);
                            
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = FareInfo.SelectSingleNode("sales_fare").InnerText.Split('/')[0];
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = (Convert.ToInt32(FareInfo.SelectSingleNode("disc_sales_fare").InnerText.Split('/')[0]) - DCAdult).ToString();
                            //NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel - QChrg).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = (Tax - Fuel).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = FareInfo.SelectSingleNode("adt_fuel").InnerText;
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = DCAdult.ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tasf").InnerText = (AdtTasf + TASF).ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("mTasf").InnerText = TASF.ToString();
                            NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("aTasf").InnerText = AdtTasf.ToString();
                        }

                        foreach (XmlNode TmpFlightIndex in FlightInfo.SelectNodes("flightIndex"))
                        {
                            NewSegFare = SegFareGroup.AppendChild(SegFare.CloneNode(false));
                            NewSegFare.Attributes.GetNamedItem("ref").InnerText = TmpFlightIndex.Attributes.GetNamedItem("ref").InnerText;

                            foreach (XmlNode TmpSeg in TmpFlightIndex.SelectNodes("segGroup/seg"))
                            {
                                NewFare = NewSegFare.AppendChild(Fare.Clone());
                                NewFare.Attributes.GetNamedItem("bpt").InnerText = "";
                                NewFare.Attributes.GetNamedItem("mas").InnerText = "";
                                NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText = SegHold.SelectNodes("seg_detail_t")[SegStep].SelectSingleNode("class").InnerText;
                                NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("cabin").InnerText = SegHold.SelectNodes("seg_detail_t")[SegStep].SelectSingleNode("cabin").InnerText;
                                NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = SegHold.SelectNodes("seg_detail_t")[SegStep].SelectSingleNode("no_of_avail_seat").InnerText;
                                NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText = (FareInfo.SelectNodes("reply_itin_t").Count > SegStep) ? FareInfo.SelectNodes("reply_itin_t")[SegStep].SelectSingleNode("fare_basis").InnerText : "";
                                NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("ptc").InnerText = PTC[i];
                                NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("tkd").InnerText = "";
                                NewFare.SelectSingleNode("fare/fareType").InnerText = "MSP";
                                NewFare.RemoveChild(NewFare.SelectSingleNode("corporateId"));

                                if (NOP[i] > Convert.ToInt32(NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText))
                                    Status = "HL";

                                SegStep++;
                            }
                        }

                        SegFareGroup.RemoveChild(SegFare);

                        Traveler = NewPaxFare.SelectSingleNode("traveler");
                        TravelerRef = Traveler.SelectSingleNode("ref");

                        for (int n = 1; n <= NOP[i]; n++)
                        {
                            NewTravelerRef = Traveler.AppendChild(TravelerRef.Clone());
                            NewTravelerRef.InnerText = n.ToString();
                            NewTravelerRef.Attributes.RemoveAll();
                        }

                        TotalFare += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText) * NOP[i];
                        TotalDisFare += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText) * NOP[i];
                        TotalTax += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText) * NOP[i];
                        TotalFsc += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText) * NOP[i];
                        TotalMTasf += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("mTasf").InnerText) * NOP[i];
                        TotalATasf += Convert.ToDouble(NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("aTasf").InnerText) * NOP[i];

                        Traveler.RemoveChild(TravelerRef);
                    }
                }

                PaxFareGroup.RemoveChild(PaxFare);

                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = (TotalDisFare + TotalTax + TotalFsc + TotalMTasf + TotalATasf).ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText = TotalFare.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = TotalDisFare.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tax").InnerText = TotalTax.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fsc").InnerText = TotalFsc.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disPartner").InnerText = (DCAdult * Convert.ToInt32(SegHold.SelectSingleNode("pnr_common_data_t/num_adt_pax").InnerText)).ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tasf").InnerText = (TotalMTasf + TotalATasf).ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("mTasf").InnerText = TotalMTasf.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("aTasf").InnerText = TotalATasf.ToString();
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("pvc").InnerText = ValidatingCarrier;
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("mas").InnerText = SegHold.SelectSingleNode("pnr_common_data_t/validity").InnerText;
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ttl").InnerText = String.IsNullOrWhiteSpace(SegHold.SelectSingleNode("pnr_common_data_t/ttl").InnerText) ? DateTime.Now.AddDays(10).ToString("yyyy-MM-dd") : cm.ConvertToDateTime(SegHold.SelectSingleNode("pnr_common_data_t/ttl").InnerText);
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ucf").InnerText = "N";
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ntf").InnerText = "N";
                PriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("sutf").InnerText = Common.SelectUserTASF(SNM, ValidatingCarrier);

                //상태
                XmlAttribute AttrStatus = XmlDoc.CreateAttribute("status");
                AttrStatus.InnerText = Status;

                PriceIndex.SelectSingleNode("summary").Attributes.Append(AttrStatus);

                //세이버정보 저장
                XmlNode FareMessage = PriceIndex.SelectSingleNode("fareMessage");

                //세이버정보 저장 - 예약용 파라미터
                XmlNode SegHoldNode = XmlDoc.CreateElement("segHold");
                SegHoldNode.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(ParamValue));
                FareMessage.AppendChild(SegHoldNode);

                //세이버정보 저장 - 랜딩 서버정보
                XmlNode LandingGuidNode = XmlDoc.CreateElement("landingGuid");
                LandingGuidNode.InnerText = GUID;
                FareMessage.AppendChild(LandingGuidNode);

                //세이버정보 저장 - 세그홀딩정보
                XmlNode PrsIdNode = XmlDoc.CreateElement("prsId");
                PrsIdNode.InnerText = SegHold.SelectSingleNode("prs_id").InnerText;
                FareMessage.AppendChild(PrsIdNode);

                //세이버정보 저장 - 요금정보
                XmlNode ReplyItinT = XmlDoc.CreateElement("replyItinT");
                foreach (XmlNode reply_itin_t in SegHold.SelectNodes("reply_if_t/reply_itin_t"))
                {
                    XmlNode SelReplyItinT = ReplyItinT.AppendChild(XmlDoc.ImportNode(reply_itin_t, true));

                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("dom_add_on_ind"));
                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("dep"));
                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("arr"));
                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("dom_dep"));
                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("dom_arr"));
                    SelReplyItinT.RemoveChild(SelReplyItinT.SelectSingleNode("dep_date"));
                }
                FareMessage.AppendChild(ReplyItinT);

                //세이버정보 저장 - 카드 프로모션 정보
                XmlNode CardRec = XmlDoc.CreateElement("cardRec");
                CardRec.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection((SegHold.SelectNodes("reply_if_t/CARD_REC").Count > 0) ? SegHold.SelectSingleNode("reply_if_t/CARD_REC").InnerText : ""));
                FareMessage.AppendChild(CardRec);

                //세이버정보 저장 - 사이트할인정보
                XmlNode SiteRuleId = XmlDoc.CreateElement("siteRuleId");
                SiteRuleId.InnerText = (SegHold.SelectNodes("reply_if_t/site_rule_id").Count > 0) ? SegHold.SelectSingleNode("reply_if_t/site_rule_id").InnerText : "";
                FareMessage.AppendChild(SiteRuleId);

                //이베이정보 저장 - 이벤트할인
                XmlNode ItemCouponInfo = XmlDoc.CreateElement("itemCouponInfo");

                XmlNode EbayCouponType = XmlDoc.CreateElement("couponType");
                EbayCouponType.InnerText = CouponType;
                ItemCouponInfo.AppendChild(EbayCouponType);

                XmlNode EbayCouponNumber = XmlDoc.CreateElement("couponNumber");
                EbayCouponNumber.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(CouponNumber));
                ItemCouponInfo.AppendChild(EbayCouponNumber);

                XmlNode EbayDCType = XmlDoc.CreateElement("dcType");
                EbayDCType.InnerText = DCType;
                ItemCouponInfo.AppendChild(EbayDCType);

                XmlNode EbayDCAmtSource = XmlDoc.CreateElement("dcAmtSource");
                EbayDCAmtSource.AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(DCAmt));
                ItemCouponInfo.AppendChild(EbayDCAmtSource);

                XmlNode EbayDCAmt = XmlDoc.CreateElement("dcAmt");
                EbayDCAmt.InnerText = DCAdult.ToString();
                ItemCouponInfo.AppendChild(EbayDCAmt);

                FareMessage.AppendChild(ItemCouponInfo);

                //프로모션 정보
                if (SegHold.SelectNodes("reply_if_t/CARD_REC").Count > 0 && !String.IsNullOrWhiteSpace(SegHold.SelectSingleNode("reply_if_t/CARD_REC").InnerText))
                {
                    string[] CardPromotion = SegHold.SelectSingleNode("reply_if_t/CARD_REC").InnerText.Split('=')[1].Split('^');
                    XmlElement SabrePromXml = sas.SearchPromotionDB(CardPromotion[0]);
                    XmlElement SabrePromMappingXml = sas.SearchPromotionMappingDB(CardPromotion[0], ((SNM.Equals(5161) || SNM.Equals(5163)) ? "AUCTION" : ((SNM.Equals(5162) || SNM.Equals(5164)) ? "G9" : "GMARKET")));
                    
                    //세이버에서 넘어온 프로모션 정보로 셋팅
                    if (SabrePromXml.SelectNodes("cardPromotion").Count > 0 && SabrePromMappingXml.SelectNodes("cardPromotion").Count > 0)
                    {
                        XmlNode CardProm = SabrePromXml.SelectSingleNode("cardPromotion");
                        XmlNode CardPromMapping = SabrePromMappingXml.SelectSingleNode("cardPromotion");
                        string StrPromXml = String.Concat("<promotionInfo>",
                                                                "<item>",
                                                                "<promotionId>", ((CardPromMapping.SelectNodes("CPM_CPC_CODE").Count > 0) ? CardPromMapping.SelectSingleNode("CPM_CPC_CODE").InnerText : ""), "</promotionId>",
                                                                "<siteNum>", SNM, "</siteNum>",
                                                                "<airCode>", ((CardPromMapping.SelectNodes("CPM_TKT_CAR").Count > 0) ? CardPromMapping.SelectSingleNode("CPM_TKT_CAR").InnerText : ""), "</airCode>",
                                                                "<fareType></fareType>",
                                                                "<fareBasis>", ((CardPromMapping.SelectNodes("CPM_FARE_GROUP_ID").Count > 0) ? CardPromMapping.SelectSingleNode("CPM_FARE_GROUP_ID").InnerText : ""), "</fareBasis>",
                                                                "<cabinClass>", ((CardPromMapping.SelectNodes("CPM_COMPARTMENT").Count > 0) ? CardPromMapping.SelectSingleNode("CPM_COMPARTMENT").InnerText : ""), "</cabinClass>",
                                                                "<bookingClass>", ((CardPromMapping.SelectNodes("CPM_BOOKING_CLS").Count > 0) ? CardPromMapping.SelectSingleNode("CPM_BOOKING_CLS").InnerText : ""), "</bookingClass>",
                                                                "<bookingClassExc></bookingClassExc>",
                                                                "<paxType></paxType>",
                                                                "<discount>0.0000</discount>",
                                                                "<commission>0.0000</commission>",
                                                                "<fareDiscount>0.0000</fareDiscount>",
                                                                "<incentive>", (CardPromotion[4].Equals("R") ? (Convert.ToDouble(CardPromotion[6]) * 0.01).ToString() : CardPromotion[6]), "</incentive>",
                                                                "<incentiveCode>", ((CardProm.SelectNodes("CPC_CARD_TYPE").Count > 0) ? CardProm.SelectSingleNode("CPC_CARD_TYPE").InnerText : ""), "</incentiveCode>",
                                                                "<incentiveName>", ((CardProm.SelectNodes("CPC_IDT_LCL_DESC").Count > 0) ? CardProm.SelectSingleNode("CPC_IDT_LCL_DESC").InnerText : ""), "</incentiveName>",
                                                                "<fareTarget>", ((CardProm.SelectNodes("CPC_IDT_LCL_DESC").Count > 0) ? String.Format("성인/{0}할인", CardProm.SelectSingleNode("CPC_IDT_LCL_DESC").InnerText) : ""), "</fareTarget>",
                                                                "<childDiscountYN></childDiscountYN>",
                                                                "<promotionTL></promotionTL>",
                                                                "<NaverEventTypeCode></NaverEventTypeCode>",
                                                                "</item>",
                                                            "</promotionInfo>");

                        XmlDocument PromXmDoc = new XmlDocument();
                        PromXmDoc.LoadXml(StrPromXml);

                        PriceIndex.SelectSingleNode("promotionInfo").AppendChild(XmlDoc.ImportNode(PromXmDoc.SelectSingleNode("promotionInfo/item"), true));
                    }
                    else
                        PriceIndex.RemoveChild(PriceIndex.SelectSingleNode("promotionInfo"));
                }
                else
                    PriceIndex.RemoveChild(PriceIndex.SelectSingleNode("promotionInfo"));

                //이베이 아이템할인 등록
                //if (DCAdult > 0)
                //{
                //    XmlNode PartnerPromotions = XmlDoc.CreateElement("partnerPromotions");
                //    XmlNode PartnerPromotion = XmlDoc.CreateElement("promotion");
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("partner")).InnerText = "EBAY";
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("code")).InnerText = "";
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("originalCode")).InnerText = CouponNumber;
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("amount")).InnerText = DCAdult.ToString();
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("adultYN")).InnerText = "Y";
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("childYN")).InnerText = "N";
                //    PartnerPromotion.Attributes.Append((XmlAttribute)XmlDoc.CreateAttribute("infantYN")).InnerText = "N";
                //    PartnerPromotion.InnerText = "";

                //    PartnerPromotions.AppendChild(PartnerPromotion);

                //    if (PriceIndex.SelectNodes("promotionInfo").Count > 0)
                //        PriceIndex.SelectSingleNode("promotionInfo/item").AppendChild(PartnerPromotions);
                //    else
                //    {
                //        XmlNode PromotionInfo = XmlDoc.CreateElement("promotionInfo");
                //        XmlNode Item = XmlDoc.CreateElement("item");

                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("promotionId"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("siteNum"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("airCode"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("tripType"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("fareType"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("cabinClass"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("bookingClass"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("paxType"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("discount"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("commission"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("fareDiscount"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("incentive"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("incentiveCode"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("incentiveName"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("fareTarget"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("childDiscountYN"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("promotionTL"));
                //        Item.AppendChild((XmlNode)XmlDoc.CreateElement("NaverEventTypeCode"));

                //        Item.AppendChild(PartnerPromotions);
                //        PromotionInfo.AppendChild(Item);
                //        PriceIndex.AppendChild(PromotionInfo);
                //    }
                //}

                //로그기록
                cm.XmlFileSave(XmlDoc, "AllianceService", MethodBase.GetCurrentMethod().Name, "N", GUID);

                return XmlDoc.DocumentElement;
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, ServiceNumber, 0, 0).ToErrors;
            }
        }

        public XmlNode ToModeSegGroupSabre(string CCD, XmlNode SegmentGroup, XmlNodeList AirSegDetails)
        {
            XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
            XmlNode StopSegment = Segment.SelectSingleNode("seg");

            XmlNode NewSegment;
            XmlNode NewStopSegment;

            string CDS = "N";
            int CCDRef = Common.RefOverride("Sabre", CCD);
            int RefNo = 1;
            
            SegmentGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefSum(RefNo, CCDRef).ToString();
            SegmentGroup.Attributes.GetNamedItem("eft").InnerText = cm.TotalTimeofGalileo(AirSegDetails, "flying_time");
            //SegmentGroup.Attributes.GetNamedItem("ewt").InnerText = cm.SumTime(cm.SumTime(cm.TotalTimeofGalileo(AirSegDetails, "connecting_time"), cm.TotalTimeofGalileo(AirSegDetails, "via1_connecting_time")), cm.TotalTimeofGalileo(AirSegDetails, "via2_connecting_time")).Replace(":", "").Replace("0000", "");
            //SegmentGroup.Attributes.GetNamedItem("ewt").InnerText = cm.TotalTimeofGalileo(AirSegDetails, "connecting_time").Replace(":", "").Replace("0000", "");
            SegmentGroup.Attributes.GetNamedItem("mjc").InnerText = AirSegDetails[0].SelectSingleNode("car_code").InnerText;
            SegmentGroup.Attributes.GetNamedItem("nosp").InnerText = AirSegDetails.Count.ToString();

            foreach (XmlNode AirSegDetail in AirSegDetails)
            {
                NewSegment = SegmentGroup.AppendChild(Segment.CloneNode(false));
                NewSegment.Attributes.GetNamedItem("dlc").InnerText = AirSegDetail.SelectSingleNode("dep_city").InnerText;
                NewSegment.Attributes.GetNamedItem("alc").InnerText = AirSegDetail.SelectSingleNode("arr_city").InnerText;
                NewSegment.Attributes.GetNamedItem("ddt").InnerText = cm.ConvertToDateTime(AirSegDetail.SelectSingleNode("dep_date_time").InnerText);
                NewSegment.Attributes.GetNamedItem("ardt").InnerText = cm.ConvertToDateTime(AirSegDetail.SelectSingleNode("arr_date_time").InnerText);
                NewSegment.Attributes.GetNamedItem("mcc").InnerText = AirSegDetail.SelectSingleNode("car_code").InnerText;
                NewSegment.Attributes.GetNamedItem("occ").InnerText = String.IsNullOrWhiteSpace(AirSegDetail.SelectSingleNode("op_car_code").InnerText) ? AirSegDetail.SelectSingleNode("car_code").InnerText : AirSegDetail.SelectSingleNode("op_car_code").InnerText;
                NewSegment.Attributes.GetNamedItem("fln").InnerText = AirSegDetail.SelectSingleNode("main_flt").InnerText;
                NewSegment.Attributes.GetNamedItem("eqt").InnerText = AirSegDetail.SelectSingleNode("flt_type1").InnerText;
                NewSegment.Attributes.GetNamedItem("stn").InnerText = String.IsNullOrWhiteSpace(AirSegDetail.SelectSingleNode("via_point1").InnerText) ? "0" : "1";
                NewSegment.Attributes.GetNamedItem("etc").InnerText = "Y";
                NewSegment.Attributes.GetNamedItem("eft").InnerText = AirSegDetail.SelectSingleNode("flying_time").InnerText;
                NewSegment.Attributes.GetNamedItem("gwt").InnerText = AirSegDetail.SelectSingleNode("via1_connecting_time").InnerText;

                if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                {
                    NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                    NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("alc").InnerText = AirSegDetail.SelectSingleNode("via_point1").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("eft").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = "";

                    NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                    NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = AirSegDetail.SelectSingleNode("via_point1").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("eft").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = AirSegDetail.SelectSingleNode("via1_connecting_time").InnerText;
                }
                else if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("2"))
                {
                    NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                    NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("alc").InnerText = AirSegDetail.SelectSingleNode("via_point1").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("eft").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = "";

                    NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                    NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = AirSegDetail.SelectSingleNode("via_point1").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("alc").InnerText = AirSegDetail.SelectSingleNode("via_point2").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("eft").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = AirSegDetail.SelectSingleNode("via1_connecting_time").InnerText;

                    NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                    NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = AirSegDetail.SelectSingleNode("via_point2").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
                    NewStopSegment.Attributes.GetNamedItem("eft").InnerText = "";
                    NewStopSegment.Attributes.GetNamedItem("gwt").InnerText = AirSegDetail.SelectSingleNode("via2_connecting_time").InnerText;
                }

                //공동운항 여부
                if (CDS.Equals("N") && (NewSegment.Attributes.GetNamedItem("mcc").InnerText != NewSegment.Attributes.GetNamedItem("occ").InnerText))
                    CDS = "Y";
            }

            SegmentGroup.RemoveChild(Segment);

            SegmentGroup.Attributes.GetNamedItem("ewt").InnerText = cm.SumTime(cm.SumTime(cm.ElapseWaitingTime(SegmentGroup), cm.TotalTimeofGalileo(AirSegDetails, "via1_connecting_time")), cm.TotalTimeofGalileo(AirSegDetails, "via2_connecting_time")).Replace(":", "");
            SegmentGroup.Attributes.GetNamedItem("cds").InnerText = CDS;

            return SegmentGroup;
        }

        #endregion "랜딩페이지용"

        #region "요금규정"

        /// <summary>
        /// [이베이] 요금규정 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="ParamValue">이베이에서 넘어온 파라미터 값</param>
        /// <returns></returns>
        [WebMethod(Description = "[이베이] 요금규정 조회")]
        public XmlElement SearchRuleEbayRS(int SNM, string ParamValue)
        {
            //임시테스트용
            if (String.IsNullOrWhiteSpace(ParamValue))
            {
                ParamValue = "trip=RT&dep0=ICN&arr0=FUK&depdate0=20180925&dep1=FUK&arr1=ICN&depdate1=20180926&retdate=20180926&comp=Y&origin=MODETOUR&tasfOrigin=MODETOUR_M_GM&adt=1&chd=0&inf=0&LangCode=KOR&DBCharSet=UTF-8&CharSet=UTF-8&Pricing=KRW&L1Code=2000&L1SiteType=W_GMARKET_M&GroupCode=FG12&MapType=SKD&CRuleType=B&GscCode=GMARKET&mSiteInd=M&MobileInd=Y&BfmInd=Y&actiontype=itin&ParmType=Inline&itindep00=ICN&itinarr00=FUK&itinfrbs00=OEE2KR&itinbkcls00=O&itinaccode00=&itingovcar00=7C&itindir00=F&itindepdate00=20180925&itinbgrid00=&itinbgrgroupid00=&itindep01=FUK&itinarr01=ICN&itinfrbs01=BEE2KR&itinbkcls01=B&itinaccode01=&itingovcar01=7C&itindir01=T&itindepdate01=20180926&itinbgrid01=&itinbgrgroupid01=&bfmpoolpd=5GH8&bfmltd=&bfmPrivate=Y&mstay=&Segbkcls=ICN-7C(O)-FUK^FUK-7C(B)-ICN^&fareType=S&pdsc=&BfmRule=789C5D90416B83401085FF8AEC79223B1BD7317B8AAE5ADAB42AC6144A692E2587422885D093FADF3BA33184AAE0CCB76FDEF0B657DE2BA7DE14A8A654AE574DC76D9A770CB2072E1F7D359487DD407EC0E0E7EBF479BA1CA5177EC7585EA62C3796486BE097C9AB589397B36CB6021E852D48B56BA7E6A981B2AD5F803CD480501785E1137E64CB24DC2EBAEB54578B380303D94D2C2B64D9BE780EC8073C407188466A46481B6EAA8337D686262AAA3C68EB0211E3D0E23A49783AE5F06A5E7E9C6D19FA5C3CBF7FCFE7C1445A13921A41D5F945B9F79EFF925763B2D29B95B19D8EDD5A3B0E7FCFC8592B8CB3453A81E50A38007F9002B2E37FABB8D3C975EC8EA17638B14CAC0896EBB95919357E8C7F0EAE6E09&ri_header=7C^ADT^KRW^285000^257700^0^0^0^0^N^58600^0^0^20600^0^0^0^0^0^Y^^^^^^^^^^^^^^^0^0^0^0^Y^^^^^^^^^^^^^27300.0^0.0^0.0^^^^^^^^^0.0^0.0^0.0^^&sltdseghd0=0125/0125//MIL//351/&sltdseg00=7C//1408/ICN//FUK//201809250630////201809250755////1////0/0125/0125/0000/MIL/351////////0/737//////O/9/N///Y/&sltdcmd00=&sltdseghd1=0120/0120//MIL//351/&sltdseg10=7C//1407/FUK//ICN//201809260855////201809261015////1////0/0120/0120/0000/MIL/351////////0/737//////B/9/N///Y/&sltdcmd10=&FS1CLS=O9&FS2CLS=B9&FS3CLS=";
            }
            
            return mas.SearchRuleSabreRS(SNM, "", "", "", Server.HtmlDecode(ParamValue));
        }

        #endregion "요금규정"

        #endregion "이베이"

        #region "트래블하우"

        #region "설정"

        protected static string TravelHowDevUrl = "http://devapi.travelhow.com";
        protected static string TravelHowUrl = "http://api.travelhow.com";
        protected static string TravelHowApiKey = "ZlauOiSCuJbgW9ziCU5aSwVlWnPcTtIULz1DRShruSY";

        /// <summary>
        /// [트래블하우] 기본정보
        /// </summary>
        /// <param name="Service"></param>
        /// <returns></returns>
        private string[] TravelHow(string Service)
        {
            string[] ServiceInfo = new String[2] { "", "" };

            switch(Service)
            {
                case "detailPromotion": //프로모션 조회
                    ServiceInfo[0] = "POST";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/discount");
                    break;
                case "insertPromotion": //프로모션 등록
                    ServiceInfo[0] = "POST";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/discount");
                    break;
                case "deletePromotion": //프로모션 삭제
                    ServiceInfo[0] = "DELETE";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/discount");
                    break;
                case "deleteAllPromotion": //프로모션 전체 삭제
                    ServiceInfo[0] = "DELETE";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/discountDeleteAll");
                    break;
                case "detailPartial": //예약 상세 조회
                    ServiceInfo[0] = "POST";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/bookings/item/detailPartial");
                    break;
                case "modification": //예약 데이타 수정
                    ServiceInfo[0] = "POST";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/bookings/item/modification");
                    break;
                case "cancel": //예약 취소
                    ServiceInfo[0] = "PUT";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/bookings/item/cancel");
                    break;
                case "consulting": //1:1 상담 수정
                    ServiceInfo[0] = "PUT";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/common/consulting");
                    break;
                case "retreive": //예약 Retreive
                    ServiceInfo[0] = "POST";
                    ServiceInfo[1] = String.Concat(TravelHowUrl, "/flights/bookings/item/retreive");
                    break;
                default:
                    ServiceInfo[0] = "";
                    ServiceInfo[1] = "";
                    break;
            }

            return ServiceInfo;
        }

        #endregion "설정"

        #region "예약조회"

        /// <summary>
        /// [트래블하우] 예약 상세조회
        /// </summary>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <returns>복호화된 예약정보</returns>
        [WebMethod(Description = "[트래블하우] 예약 상세조회")]
        public XmlElement TravelHowSearchBookingRS(string BookingItemCode)
        {
            try
            {
                string[] ServiceInfo = TravelHow("detailPartial");
                string PushData = String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"method\":\"read\",\"condition\":{\"bookingItemCode\":\"", BookingItemCode, "\"}}");

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                return ResXml;
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// [트래블하우] 예약 상세조회
        /// </summary>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="ItemList">복호화 하기 위한 아이템 노드명</param>
        /// <returns>복호화된 예약정보</returns>
        [WebMethod(Description = "[트래블하우] 예약 상세조회")]
        public XmlElement TravelHowSearchBookingDecRS(string BookingItemCode, string ItemList)
        {
            try
            {
                return TravelHowXmlBookDecrypt(TravelHowSearchBookingRS(BookingItemCode), (String.IsNullOrWhiteSpace(ItemList) ? "*" : ItemList));
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "예약조회"

        #region "예약데이타 복호화"

        /// <summary>
        /// 트래블하우 예약 XML 데이타 복호화
        /// </summary>
        /// <param name="XmlBooking">암호화된 예약 XML</param>
        /// <param name="ItemList">복호화 하기 위한 아이템 노드명</param>
        /// <returns></returns>
        public XmlElement TravelHowXmlBookDecrypt(XmlElement XmlBooking, string ItemList)
        {
            if (XmlBooking.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
            {
                XmlNode ItemNode = null;

                if (ItemList.Equals("*"))
                    ItemList = "/master/flightItem/flightTravelerTicket/travelerMaster/paymentDetail/travelerMileageCard/flightCertifiedDocument/";

                if (ItemList.IndexOf("/master/") != -1 && XmlBooking.SelectSingleNode("result/item/master").HasChildNodes)
                {
                    ItemNode = XmlBooking.SelectSingleNode("result/item/master");

                    ItemNode.SelectSingleNode("clientName").InnerText = aes.AESDecrypt("TravelHow", ItemNode.SelectSingleNode("clientName").InnerText);
                    ItemNode.SelectSingleNode("clientEmail").InnerText = aes.AESDecrypt("TravelHow", ItemNode.SelectSingleNode("clientEmail").InnerText);
                    ItemNode.SelectSingleNode("clientEmergencyPhoneNo").InnerText = aes.AESDecrypt("TravelHow", ItemNode.SelectSingleNode("clientEmergencyPhoneNo").InnerText);
                }

                if (ItemList.IndexOf("/flightItem/") != -1 && XmlBooking.SelectSingleNode("result/item/flightItem").HasChildNodes)
                {
                    ItemNode = XmlBooking.SelectSingleNode("result/item/flightItem");

                    ItemNode.SelectSingleNode("gdsMainPNR").InnerText = aes.AESDecrypt("TravelHow", ItemNode.SelectSingleNode("gdsMainPNR").InnerText);
                    ItemNode.SelectSingleNode("gdsSubPNR").InnerText = aes.AESDecrypt("TravelHow", ItemNode.SelectSingleNode("gdsSubPNR").InnerText);
                }

                if (ItemList.IndexOf("/flightTravelerTicket/") != -1 && XmlBooking.SelectSingleNode("result/item/flightTravelerTicket").HasChildNodes)
                {
                    foreach (XmlNode Item in XmlBooking.SelectNodes("result/item/flightTravelerTicket/item"))
                    {
                        Item.SelectSingleNode("ticketNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("ticketNo").InnerText);
                        Item.SelectSingleNode("ticketRefundBankAccountName").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("ticketRefundBankAccountName").InnerText);
                        Item.SelectSingleNode("ticketRefundBankAccountNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("ticketRefundBankAccountNo").InnerText);
                    }
                }

                if (ItemList.IndexOf("/travelerMaster/") != -1 && XmlBooking.SelectSingleNode("result/item/travelerMaster").HasChildNodes)
                {
                    foreach (XmlNode Item in XmlBooking.SelectNodes("result/item/travelerMaster/item"))
                    {
                        Item.SelectSingleNode("name").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("name").InnerText);
                        Item.SelectSingleNode("lastName").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("lastName").InnerText);
                        Item.SelectSingleNode("firstName").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("firstName").InnerText);
                        Item.SelectSingleNode("gender").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("gender").InnerText);
                        Item.SelectSingleNode("birthday").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("birthday").InnerText);
                        Item.SelectSingleNode("passportIssueCountryCode").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("passportIssueCountryCode").InnerText);
                        Item.SelectSingleNode("passportNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("passportNo").InnerText);
                        Item.SelectSingleNode("passportExpireDate").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("passportExpireDate").InnerText);
                        Item.SelectSingleNode("addressType").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressType").InnerText);
                        Item.SelectSingleNode("addressDetail").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressDetail").InnerText);
                        Item.SelectSingleNode("addressCityNameEN").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressCityNameEN").InnerText);
                        Item.SelectSingleNode("addressZipCode").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressZipCode").InnerText);
                        Item.SelectSingleNode("addressStateCode").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressStateCode").InnerText);
                        Item.SelectSingleNode("addressCountryCode").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("addressCountryCode").InnerText);
                        Item.SelectSingleNode("localPhoneNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("localPhoneNo").InnerText);
                        Item.SelectSingleNode("travelerNationalityCode").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("travelerNationalityCode").InnerText);
                    }
                }

                if (ItemList.IndexOf("/paymentDetail/") != -1 && XmlBooking.SelectSingleNode("result/item/paymentDetail").HasChildNodes)
                {
                    foreach (XmlNode Item in XmlBooking.SelectNodes("result/item/paymentDetail/item"))
                    {
                        Item.SelectSingleNode("cardAuthNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardAuthNo").InnerText);
                        Item.SelectSingleNode("cardQuotaMonth").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardQuotaMonth").InnerText);
                        Item.SelectSingleNode("cardNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardNo").InnerText);
                        Item.SelectSingleNode("cardValidPeriod").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardValidPeriod").InnerText);
                        Item.SelectSingleNode("cardHolderName").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardHolderName").InnerText);
                        Item.SelectSingleNode("cardPassword").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardPassword").InnerText);
                        Item.SelectSingleNode("cardSocialRegistrationNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("cardSocialRegistrationNo").InnerText);
                        Item.SelectSingleNode("bankAccountNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("bankAccountNo").InnerText);
                        Item.SelectSingleNode("bankAccountName").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("bankAccountName").InnerText);
                        Item.SelectSingleNode("isCashReceiptRequest").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("isCashReceiptRequest").InnerText);
                    }
                }

                if (ItemList.IndexOf("/travelerMileageCard/") != -1 && XmlBooking.SelectSingleNode("result/item/travelerMileageCard").HasChildNodes)
                {
                    foreach (XmlNode Item in XmlBooking.SelectNodes("result/item/travelerMileageCard/item"))
                    {
                        Item.SelectSingleNode("mileageCardNo").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("mileageCardNo").InnerText);
                    }
                }

                if (ItemList.IndexOf("/flightCertifiedDocument/") != -1 && XmlBooking.SelectSingleNode("result/item/flightCertifiedDocument").HasChildNodes)
                {
                    foreach (XmlNode Item in XmlBooking.SelectNodes("result/item/flightCertifiedDocument/item"))
                    {
                        Item.SelectSingleNode("documentImageUrl").InnerText = aes.AESDecrypt("TravelHow", Item.SelectSingleNode("documentImageUrl").InnerText);
                    }
                }
            }

            return XmlBooking;
        }

        #endregion "예약데이타 복호화"

        #region "프로모션"

        /// <summary>
        /// [트래블하우] 프로모션 전체 등록
        /// </summary>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 프로모션 전체 등록")]
        public XmlElement TravelHowAddAllPromotionRS(int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;
            
            try
            {
                XmlDocument XmlDoc = new XmlDocument();

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet("promotionInfo"))
                        {
                            cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
                            cmd.CommandTimeout = 60;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSP_S_항공할인_카카오";

                            adp.Fill(ds, "item");

                            XmlDoc.LoadXml(ds.GetXml().Replace("&amp;lt;BR&amp;gt;", "").Replace("&lt;", "<").Replace("&gt;", ">"));
                            ds.Clear();
                        }
                    }
                }

                string[] ServiceInfo = TravelHow("insertPromotion");
                string PushData = String.Format("<root><apiKey>{0}</apiKey>{1}</root>", TravelHowApiKey, XmlDoc.SelectSingleNode("promotionInfo/item/*/condition").OuterXml);
                
                QueueNumber = TravelHowQueueSave(0, "", "", PushData, "PROMOTION_ADDALL", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("condition").Count > 0)
                {
                    //if (ResXml.SelectNodes("condition/list/item/error").Count > 0)
                    //    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");
                    //else if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("False"))
                    //    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");
                    //else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                }
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 프로모션 전체 삭제
        /// </summary>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 프로모션 전체 삭제")]
        public XmlElement TravelHowDeleteAllPromotionRS(int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                string[] ServiceInfo = TravelHow("deleteAllPromotion");
                //string PushData = String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"condition\":{}}");
                string PushData = String.Concat("{\"condition\":{}, \"service\":\"flights.discountDeleteAll\", \"apiKey\":\"", TravelHowApiKey, "\", \"userAgent\":{\"bot\":\"False\", \"platform\":{\"name\":\"None\", \"version\":\"None\"}, \"browser\":{\"name\":\"Microsoft Internet Explorer\", \"version\":\"6.0\"}}, \"method\":\"delete\", \"sellerCompCode\":\"200002\"}");
                
                QueueNumber = TravelHowQueueSave(0, "", "", PushData, "PROMOTION_DELETEALL", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 프로모션 삭제
        /// </summary>
        /// <param name="PromotionCode">프로모션 코드</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 프로모션 삭제")]
        public XmlElement TravelHowDeletePromotionRS(string PromotionCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;
            
            try
            {
                string[] ServiceInfo = TravelHow("deleteAllPromotion");
                string PushData = String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"condition\":{\"list\":{\"isAllDelete\":\"true\", \"sellerRemarks\":\"", PromotionCode, "\"}}}");

                QueueNumber = TravelHowQueueSave(0, "", "", PushData, "PROMOTION_DELETE", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, PromotionCode));

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 프로모션 조회
        /// </summary>
        /// <param name="PromotionCode">프로모션 코드</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 프로모션 조회")]
        public XmlElement TravelHowSearchPromotionRS(string PromotionCode)
        {
            try
            {
                string[] ServiceInfo = TravelHow("detailPromotion");
                string Data = String.IsNullOrWhiteSpace(PromotionCode) ? String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"method\":\"read\",\"condition\":{}}") : String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"method\":\"read\",\"condition\":{\"sellerRemarks\":\"", PromotionCode, "\"}}");

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], Data);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, PromotionCode));

                return ResXml;
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "프로모션"

        #region "TL 업데이트"

        /// <summary>
        /// [트래블하우] 예약정보 중 TL정보 업데이트
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="TL">TL</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 데이타 수정(TL변경)")]
        public XmlElement TravelHowUpdateTLRS(int OID, string BookingItemCode, string TL, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;
            
            try
            {
                if (TravelHowSearchBookingRS(BookingItemCode).SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    string[] ServiceInfo = TravelHow("modification");
                    string PushData = String.Concat("<root>",
                                                        "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                        "<condition>",
                                                            "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                            "<item>",
                                                                "<flightItem>",
                                                                    "<sellerCancelDeadline>", TL, "</sellerCancelDeadline>",
                                                                "</flightItem>",
                                                            "</item>",
                                                        "</condition>",
                                                    "</root>");

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "TL_UPDATE", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                    if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }
        
        #endregion "TL 업데이트"

        #region "예약수정"

        /// <summary>
        /// [트래블하우] 예약정보 업데이트
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="RIP">요청자IP</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 데이타 수정")]
        public XmlElement TravelHowUpdateBookingRS(int OID, int PID, string BookingItemCode, string RIP, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;
            
            try
            {
                XmlElement XmlBook = TravelHowSearchBookingDecRS(BookingItemCode, "/travelerMaster/paymentDetail/");

                if (XmlBook.SelectNodes("result/isSucceed").Count > 0 && XmlBook.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    string ClientName = string.Empty;
                    string ClientEmail = string.Empty;
                    string ClientEmergencyPhoneNo = string.Empty;

                    string OrderNumber = string.Empty;
                    string GdsMainPNR = string.Empty;
                    string GdsSubPNR = string.Empty;
                    string OperatorName = string.Empty;
                    string TicketingPICName = string.Empty;
                    string SellerCancelDeadline = string.Empty;

                    string BankAccountNo = string.Empty;
                    string BankAccountName = string.Empty;

                    string FlightTravelerPriceNode = string.Empty;
                    string FlightPaymentTravelerNode = string.Empty;
                    string PaymentDetailNode = string.Empty;
                    string PaymentTravelerNode = string.Empty;

                    try
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            SqlDataReader dr = null;
                            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString);

                            cmd.Connection = conn;
                            cmd.CommandTimeout = 60;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSV_S_아이템예약_해외항공_트래블하우_예약업데이트용";

                            cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@예약자번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@요청자IP", SqlDbType.VarChar, 30);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@주문번호"].Value = OID;
                            cmd.Parameters["@예약자번호"].Value = PID;
                            cmd.Parameters["@요청자IP"].Value = RIP;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                            try
                            {
                                conn.Open();
                                dr = cmd.ExecuteReader();

                                if (dr.Read())
                                {
                                    ClientName = dr["예약자"].ToString();
                                    ClientEmail = dr["이메일"].ToString();
                                    ClientEmergencyPhoneNo = String.IsNullOrWhiteSpace(dr["휴대폰"].ToString()) ? dr["전화번호"].ToString() : dr["휴대폰"].ToString();

                                    OperatorName = dr["예약담당자"].ToString();
                                    TicketingPICName = dr["발권자"].ToString();
                                }

                                dr.NextResult();

                                if (dr.Read())
                                {
                                    OrderNumber = dr["주문번호"].ToString();
                                    GdsMainPNR = dr["MainPNR"].ToString();
                                    GdsSubPNR = dr["SubPNR"].ToString();
                                    SellerCancelDeadline = dr["TL"].ToString();
                                }

                                dr.NextResult();

                                while (dr.Read())
                                {
                                    string BookingTravelerCode = XmlBook.SelectSingleNode(String.Format("result/item/travelerMaster/item[lastName='{0}' and firstName='{1}']/bookingTravelerCode", dr["영문성"], dr["영문이름"])).InnerText;
                                    XmlNode TravelerPriceNode = XmlBook.SelectSingleNode(String.Format("result/item/flightTravelerPrice/item[bookingTravelerCode='{0}']", BookingTravelerCode));

                                    int ProductAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("productAmount").InnerText.Replace(".00", ""));
                                    int DiscountAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("discountAmount").InnerText.Replace(".00", ""));
                                    int TaxAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("taxAmount").InnerText.Replace(".00", ""));
                                    int FuelSurchargeAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("fuelSurchargeAmount").InnerText.Replace(".00", ""));
                                    int OthersAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("othersAmount").InnerText.Replace(".00", ""));
                                    int QChargeAmount = Convert.ToInt32(TravelerPriceNode.SelectSingleNode("qChargeAmount").InnerText.Replace(".00", ""));

                                    int mProductAmount = Convert.ToInt32(dr["판매액"]);
                                    int mDiscountAmount = Convert.ToInt32(dr["프로모션금액"]);
                                    int mTaxAmount = Convert.ToInt32(dr["추가판매(제세금)"]);
                                    int mFuelSurchargeAmount = Convert.ToInt32(dr["추가판매(유류)"]);
                                    int mOthersAmount = Convert.ToInt32(dr["추가판매(비자)"]) + Convert.ToInt32(dr["취급수수료"]);

                                    if (!ProductAmount.Equals(mProductAmount) || !DiscountAmount.Equals(mDiscountAmount) || !TaxAmount.Equals(mTaxAmount) || !FuelSurchargeAmount.Equals(mFuelSurchargeAmount) || !OthersAmount.Equals(mOthersAmount))
                                    {
                                        FlightTravelerPriceNode += String.Concat("<flightTravelerPrice>",
                                                                                    "<method>update</method>",
                                                                                    "<bookingTravelerCode>", BookingTravelerCode, "</bookingTravelerCode>",
                                                                                    "<priceNo>", TravelerPriceNode.SelectSingleNode("priceNo").InnerText, "</priceNo>",
                                                                                    ((!ProductAmount.Equals(mProductAmount)) ? String.Concat("<productAmount>", mProductAmount.ToString(), "</productAmount>") : ""),
                                                                                    ((!DiscountAmount.Equals(mDiscountAmount)) ? String.Concat("<discountAmount>", mDiscountAmount.ToString(), "</discountAmount>") : ""),
                                                                                    ((!ProductAmount.Equals(mProductAmount) || !DiscountAmount.Equals(mDiscountAmount)) ? String.Concat("<salesAmount>", (mProductAmount + mDiscountAmount).ToString(), "</salesAmount>") : ""),
                                                                                    ((!TaxAmount.Equals(mTaxAmount)) ? String.Concat("<taxAmount>", mTaxAmount.ToString(), "</taxAmount>") : ""),
                                                                                    ((!FuelSurchargeAmount.Equals(mFuelSurchargeAmount)) ? String.Concat("<fuelSurchargeAmount>", mFuelSurchargeAmount.ToString(), "</fuelSurchargeAmount>") : ""),
                                                                                    ((!OthersAmount.Equals(mOthersAmount)) ? String.Concat("<othersAmount>", mOthersAmount.ToString(), "</othersAmount>") : ""),
                                                                                    //"<qChargeAmount>", TravelerPriceNode.SelectSingleNode("qChargeAmount").InnerText, "</qChargeAmount>",
                                                                                    "<amountSum>", (mProductAmount + mTaxAmount + mFuelSurchargeAmount + Convert.ToInt32(TravelerPriceNode.SelectSingleNode("qChargeAmount").InnerText.Replace(".00", ""))).ToString(), "</amountSum>",
                                                                                    "<isAmountChanged>true</isAmountChanged>",
                                                                                "</flightTravelerPrice>");
                                    }
                                }

                                dr.NextResult();

                                if (dr.Read())
                                {
                                    BankAccountNo = dr["계좌번호"].ToString();
                                    BankAccountName = dr["가상계좌구분"].ToString();
                                }

                                dr.NextResult();

                                //트래블하우 결제요청 정보
                                XmlNode FlightPaymentTraveler = XmlBook.SelectSingleNode("result/item/flightPaymentTraveler");
                                XmlNode PaymentDetail = XmlBook.SelectSingleNode("result/item/paymentDetail");

                                foreach (XmlNode Item in XmlBook.SelectNodes("result/item/flightTravelerPrice/item"))
                                {
                                    PaymentTravelerNode += String.Concat("<paymentTraveler>",
                                                                            "<method>create</method>",
                                                                            "<bookingTravelerCode>", Item.SelectSingleNode("bookingTravelerCode").InnerText, "</bookingTravelerCode>",
                                                                            "<paymentAmount>", Item.SelectSingleNode("amountSum").InnerText, "</paymentAmount>",
                                                                        "</paymentTraveler>");
                                }

                                //기존 결제요청 정보 전체 삭제
                                if (PaymentDetail.SelectNodes("item").Count > 0)
                                {
                                    foreach (XmlNode Item in PaymentDetail.SelectNodes("item"))
                                    {
                                        PaymentDetailNode += String.Concat("<paymentDetail>",
                                                                                "<method>delete</method>",
                                                                                "<paymentCode>", Item.SelectSingleNode("paymentCode").InnerText, "</paymentCode>",
                                                                                "<paymentTraveler>",
                                                                                    "<method>delete</method>",
                                                                                    "<paymentCode>", Item.SelectSingleNode("paymentCode").InnerText, "</paymentCode>",
                                                                                "</paymentTraveler>",
                                                                            "</paymentDetail>");
                                    }
                                }

                                if (dr.Read())
                                {
                                    if (dr["카드결제여부"].ToString().Equals("Y"))
                                    {
                                        PaymentDetailNode += String.Concat("<paymentDetail>",
                                                                                "<method>create</method>",
                                                                                "<paymentTypeCode>PMT04</paymentTypeCode>",
                                                                                "<currencyCode>KRW</currencyCode>",
                                                                                "<paymentAmount>", dr["카드결제금액"].ToString(), "</paymentAmount>",
                                                                                "<settlementAmount></settlementAmount>",
                                                                                "<statusCode>VBS01</statusCode>",
                                                                                "<bankCardMasterCode>", BankCardMasterCode(dr["카드종류"].ToString()), "</bankCardMasterCode>",
                                                                                "<cardQuotaMonth>", aes.AESEncrypt("TravelHow", dr["할부기간"].ToString()), "</cardQuotaMonth>",
                                                                                "<cardNo>", aes.AESEncrypt("TravelHow", dr["카드번호"].ToString()), "</cardNo>",
                                                                                "<cardValidPeriod>", aes.AESEncrypt("TravelHow", dr["유효기간"].ToString()), "</cardValidPeriod>",
                                                                                "<cardHolderName>", aes.AESEncrypt("TravelHow", dr["소유자명"].ToString()), "</cardHolderName>",
                                                                                "<cardPassword>", aes.AESEncrypt("TravelHow", dr["비밀번호"].ToString()), "</cardPassword>",
                                                                                "<bankAccountNo></bankAccountNo>",
                                                                                "<bankAccountName></bankAccountName>",
                                                                                "<remitDeadline>", SellerCancelDeadline, "</remitDeadline>",
                                                                                "<paymentStatusCode>BPS08</paymentStatusCode>",
                                                                                PaymentTravelerNode,
                                                                            "</paymentDetail>");
                                    }

                                    if (dr["계좌이체여부"].ToString().Equals("Y"))
                                    {
                                        PaymentDetailNode += String.Concat("<paymentDetail>",
                                                                                "<method>create</method>",
                                                                                "<paymentTypeCode>PMT03</paymentTypeCode>",
                                                                                "<currencyCode>KRW</currencyCode>",
                                                                                "<paymentAmount>0</paymentAmount>",
                                                                                "<settlementAmount>", dr["계좌이체금액"].ToString(), "</settlementAmount>",
                                                                                "<statusCode>VBS01</statusCode>",
                                                                                "<bankCardMasterCode>B1011</bankCardMasterCode>",
                                                                                "<bankAccountNo>", aes.AESEncrypt("TravelHow", BankAccountNo), "</bankAccountNo>",
                                                                                "<bankAccountName>", aes.AESEncrypt("TravelHow", BankAccountName), "</bankAccountName>",
                                                                                "<remitDeadline>", SellerCancelDeadline, "</remitDeadline>",
                                                                                "<paymentStatusCode>BPS08</paymentStatusCode>",
                                                                                PaymentTravelerNode,
                                                                            "</paymentDetail>");
                                    }
                                }
                                //else
                                //{
                                //    if (!String.IsNullOrWhiteSpace(BankAccountNo))
                                //    {
                                //        PaymentDetailNode += String.Concat("<paymentDetail>",
                                //                                                "<method>create</method>",
                                //                                                "<paymentTypeCode>PMT03</paymentTypeCode>",
                                //                                                "<currencyCode>KRW</currencyCode>",
                                //                                                "<paymentAmount>0</paymentAmount>",
                                //                                                "<settlementAmount>0</settlementAmount>",
                                //                                                "<statusCode>VBS01</statusCode>",
                                //                                                "<bankCardMasterCode>B1011</bankCardMasterCode>",
                                //                                                "<bankAccountNo>", aes.AESEncrypt("TravelHow", BankAccountNo), "</bankAccountNo>",
                                //                                                "<bankAccountName>", aes.AESEncrypt("TravelHow", BankAccountName), "</bankAccountName>",
                                //                                                "<remitDeadline>", SellerCancelDeadline, "</remitDeadline>",
                                //                                                "<paymentStatusCode>BPS08</paymentStatusCode>",
                                //                                                PaymentTravelerNode,
                                //                                            "</paymentDetail>");
                                //    }
                                //}
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                dr.Dispose();
                                dr.Close();
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    string[] ServiceInfo = TravelHow("modification");
                    string PushData = String.Concat("<root>",
                                                        "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                        "<condition>",
                                                            "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                            "<item>",
                                                                "<master>",
                                                                    (String.IsNullOrWhiteSpace(ClientName) ? "" : String.Concat("<clientName>", aes.AESEncrypt("TravelHow", ClientName), "</clientName>")),
                                                                    (String.IsNullOrWhiteSpace(ClientEmail) ? "" : String.Concat("<clientEmail>", aes.AESEncrypt("TravelHow", ClientEmail), "</clientEmail>")),
                                                                    (String.IsNullOrWhiteSpace(ClientEmergencyPhoneNo) ? "" : String.Concat("<clientEmergencyPhoneNo>", aes.AESEncrypt("TravelHow", ClientEmergencyPhoneNo), "</clientEmergencyPhoneNo>")),
                                                                "</master>",
                                                                "<flightItem>",
                                                                    (String.IsNullOrWhiteSpace(GdsMainPNR) ? "" : String.Concat("<gdsMainPNR>", aes.AESEncrypt("TravelHow", GdsMainPNR), "</gdsMainPNR>")),
                                                                    (String.IsNullOrWhiteSpace(GdsSubPNR) ? "" : String.Concat("<gdsSubPNR>", aes.AESEncrypt("TravelHow", GdsSubPNR), "</gdsSubPNR>")),
                                                                    (String.IsNullOrWhiteSpace(OperatorName) ? "" : String.Concat("<operatorName>", OperatorName, "</operatorName>")),
                                                                    (String.IsNullOrWhiteSpace(TicketingPICName) ? "" : String.Concat("<ticketingPICName>", TicketingPICName, "</ticketingPICName>")),
                                                                    (String.IsNullOrWhiteSpace(SellerCancelDeadline) ? "" : String.Concat("<sellerCancelDeadline>", SellerCancelDeadline, "</sellerCancelDeadline>")),
                                                                "</flightItem>",
                                                                FlightTravelerPriceNode,
                                                                FlightPaymentTravelerNode,
                                                                PaymentDetailNode,
                                                            "</item>",
                                                        "</condition>",
                                                    "</root>");

                    //XmlDocument XmlDoc = new XmlDocument();
                    //XmlDoc.LoadXml(PushData);
                    //return XmlDoc.DocumentElement;

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "BOOKING_UPDATE", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                    if (ResXml.SelectNodes("result/isSucceed").Count.Equals(0) || !ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("False"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "예약수정"

        #region "예약취소"

        /// <summary>
        /// [트래블하우] 예약취소
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약취소")]
        public XmlElement TravelHowCancelBookingRS(int OID, string BookingItemCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                string[] ServiceInfo = TravelHow("cancel");
                string PushData = String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"method\":\"search\",\"condition\":{\"bookingItemCode\":\"", BookingItemCode, "\"}}");

                QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "BOOKING_CANCEL", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "예약취소"

        #region "1:1상담 답변 등록"

        /// <summary>
        /// [트래블하우] 1:1상담 답변 등록
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="ConsultingNo">트래블하우 문의번호</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 1:1상담 답변 등록")]
        public XmlElement TravelHowAddConsultingAnswerRS(int OID, string BookingItemCode, int ConsultingNo, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                if (TravelHowSearchBookingRS(BookingItemCode).SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    string AnswerYN = string.Empty;
                    string AnswerName = string.Empty;
                    string AnswerDetail = string.Empty;
                    string ConsultingClosedTime = string.Empty;

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                        {
                            SqlDataReader dr = null;

                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSV_S_아이템예약_해외항공_트래블하우_일대일문의";

                            cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@제휴사문의번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@주문번호"].Value = OID;
                            cmd.Parameters["@제휴사문의번호"].Value = ConsultingNo.Equals(0) ? Convert.DBNull : ConsultingNo;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                            try
                            {
                                conn.Open();
                                dr = cmd.ExecuteReader();

                                if (dr.Read())
                                {
                                    AnswerYN = dr["답변여부"].ToString().Trim();
                                    AnswerName = dr["답변자명"].ToString().Trim();
                                    AnswerDetail = dr["답변내용"].ToString().Trim();
                                    ConsultingClosedTime = dr["답변일"].ToString().Trim();
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                if (!dr.IsClosed)
                                {
                                    dr.Dispose();
                                    dr.Close();
                                }
                                conn.Close();
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(AnswerYN) && AnswerYN.Equals("Y"))
                    {
                        string[] ServiceInfo = TravelHow("consulting");
                        string PushData = String.Concat("<root>",
                                                            "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                            "<method>update</method>",
                                                            "<condition>",
                                                                "<consultingNo>", ConsultingNo, "</consultingNo>",
                                                                "<item>",
                                                                    "<consultingProcessTypeCode>CP003</consultingProcessTypeCode>",
                                                                    "<answerName>", AnswerName, "</answerName>",
                                                                    "<answerDetail><![CDATA[", AnswerDetail, "]]></answerDetail>",
                                                                    "<consultingClosedTime>", ConsultingClosedTime, "</consultingClosedTime>",
                                                                "</item>",
                                                            "</condition>",
                                                        "</root>");

                        //XmlDocument XmlDoc = new XmlDocument();
                        //XmlDoc.LoadXml(PushData);
                        //return XmlDoc.DocumentElement;

                        QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "CONSULTING_ANSWER", RQR, RQT);
                        SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                        XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                        cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                        if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                        else
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                        return ResXml;
                    }
                    else
                        throw new Exception("답변 내용이 존재하지 않습니다.");
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "1:1상담 답변 등록"

        #region "가상계좌번호"

        /// <summary>
        /// [트래블하우] 가상계좌번호 발급 및 조회
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 가상계좌번호")]
        public XmlElement TravelHowVirtualAccountRS(int OID, string BookingItemCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                if (TravelHowSearchBookingRS(BookingItemCode).SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    string Bank = string.Empty;
                    string AccountNumber = string.Empty;
                    string Holder = string.Empty;
                    string DeadLine = string.Empty;

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                        {
                            SqlDataReader dr = null;

                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSV_T_아이템예약_해외항공_트래블하우_가상계좌";

                            cmd.Parameters.Add("@제휴사예약번호", SqlDbType.VarChar, 30);
                            cmd.Parameters.Add("@결제시한", SqlDbType.VarChar, 20);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@제휴사예약번호"].Value = BookingItemCode;
                            cmd.Parameters["@결제시한"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                            try
                            {
                                conn.Open();
                                dr = cmd.ExecuteReader();

                                if (dr.Read())
                                {
                                    Bank = dr["가상계좌구분"].ToString().Trim();
                                    AccountNumber = dr["계좌번호"].ToString().Trim();
                                    Holder = dr["가상계좌예금주"].ToString().Trim();
                                }

                                dr.Close();
                                DeadLine = cmd.Parameters["@결제시한"].Value.ToString();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                if (!dr.IsClosed)
                                {
                                    dr.Dispose();
                                    dr.Close();
                                }
                                conn.Close();
                            }
                        }
                    }

                    string[] ServiceInfo = TravelHow("modification");
                    string PushData = String.Concat("<root>",
                                                        "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                        "<condition>",
                                                            "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                            "<item>",
                                                                "<paymentDetail>",
                                                                    "<method>create</method>",
                                                                    "<bankCardMasterCode>B1011</bankCardMasterCode>",
                                                                    "<bankAccountNo>", AccountNumber, "</bankAccountNo>",
                                                                    "<bankAccountName>", Holder, "</bankAccountName>",
                                                                    "<remitDeadline>", DeadLine, "</remitDeadline>",
                                                                "</paymentDetail>",
                                                            "</item>",
                                                        "</condition>",
                                                    "</root>");

                    //XmlDocument XmlDoc = new XmlDocument();
                    //XmlDoc.LoadXml(PushData);
                    //return XmlDoc.DocumentElement;

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "VIRTUAL_ACCOUNT", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                    if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "가상계좌번호"

        #region "결제요청 취소"

        /// <summary>
        /// [트래블하우] 결제요청 취소
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 결제요청 취소")]
        public XmlElement TravelHowCancelPaymentRequestRS(int OID, string BookingItemCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                XmlElement XmlBook = TravelHowSearchBookingDecRS(BookingItemCode, "/paymentDetail/");

                if (XmlBook.SelectNodes("result/isSucceed").Count > 0 && XmlBook.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    XmlNode PaymentDetail = XmlBook.SelectSingleNode("result/item/paymentDetail");
                    string PaymentDetailNode = string.Empty;

                    //결제요청 정보 전체 취소
                    if (PaymentDetail.SelectNodes("item").Count > 0)
                    {
                        foreach (XmlNode Item in PaymentDetail.SelectNodes("item"))
                        {
                            PaymentDetailNode += String.Concat("<paymentDetail>",
                                                                    "<paymentCode>", Item.SelectSingleNode("paymentCode").InnerText, "</paymentCode>",
                                                                    "<paymentStatusCode>BPS10</paymentStatusCode>",
                                                                "</paymentDetail>");
                        }
                    }

                    string[] ServiceInfo = TravelHow("modification");
                    string PushData = String.Concat("<root>",
                                                        "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                        "<condition>",
                                                            "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                            "<item>",
                                                                PaymentDetailNode,
                                                            "</item>",
                                                        "</condition>",
                                                    "</root>");

                    //XmlDocument XmlDoc = new XmlDocument();
                    //XmlDoc.LoadXml(PushData);
                    //return XmlDoc.DocumentElement;

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PAYMENTREQUEST_CANCEL", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                    if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "결제요청 취소"

        #region "결제취소(Refunded)"

        /// <summary>
        /// [트래블하우] 결제취소(Refunded)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 결제취소(Refunded)")]
        public XmlElement TravelHowCancelPaymentRS(int OID, string BookingItemCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                XmlElement XmlBook = TravelHowSearchBookingDecRS(BookingItemCode, "/paymentDetail/");

                if (XmlBook.SelectNodes("result/isSucceed").Count > 0 && XmlBook.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    XmlNode PaymentDetail = XmlBook.SelectSingleNode("result/item/paymentDetail");
                    string PaymentDetailNode = string.Empty;

                    //결제정보 전체 취소
                    if (PaymentDetail.SelectNodes("item").Count > 0)
                    {
                        foreach (XmlNode Item in PaymentDetail.SelectNodes("item"))
                        {
                            PaymentDetailNode += String.Concat("<paymentDetail>",
                                                                    "<method>update</method>",                                    
                                                                    "<paymentCode>", Item.SelectSingleNode("paymentCode").InnerText, "</paymentCode>",
                                                                    "<paymentStatusCode>BPS05</paymentStatusCode>",
                                                                "</paymentDetail>");
                        }
                    }

                    string[] ServiceInfo = TravelHow("modification");
                    string PushData = String.Concat("<root>",
                                                        "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                        "<condition>",
                                                            "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                            "<item>",
                                                                PaymentDetailNode,
                                                            "</item>",
                                                        "</condition>",
                                                    "</root>");

                    //XmlDocument XmlDoc = new XmlDocument();
                    //XmlDoc.LoadXml(PushData);
                    //return XmlDoc.DocumentElement;

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PAYMENT_CANCEL", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", String.Format("{0}_{1}", cm.GetGUID, BookingItemCode));

                    if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "결제취소(Refunded)"

        #region "VOID"

        /// <summary>
        /// [트래블하우] VOID
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="PaxEName">탑승객 영문명(성/이름)(공백: 전체보이드, 여러개 입력시 콤마로 구분)</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] VOID")]
        public XmlElement TravelHowTicketVOIDRS(int OID, string BookingItemCode, string PaxEName, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                XmlElement XmlBook = TravelHowSearchBookingDecRS(BookingItemCode, "/travelerMaster/");

                if (XmlBook.SelectNodes("result/isSucceed").Count > 0 && XmlBook.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    XmlNode TravelerMaster = XmlBook.SelectSingleNode("result/item/travelerMaster");
                    string FlightTravelerTicketNode = string.Empty;
                    string NowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (String.IsNullOrWhiteSpace(PaxEName))
                    {
                        foreach (XmlNode Item in TravelerMaster.SelectNodes("item"))
                        {
                            if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                                FlightTravelerTicketNode += ",";

                            FlightTravelerTicketNode += String.Concat("{ \"method\": \"update\",",
                                                                        "\"name\": \"", Item.SelectSingleNode("lastName").InnerText, "/", Item.SelectSingleNode("firstName").InnerText, "\",",
                                                                        "\"ticketingStatusCode\": \"TST09\",",
                                                                        "\"bookingTravelerCode\": \"", Item.SelectSingleNode("bookingTravelerCode").InnerText, "\",",
                                                                        "\"ticketIssuedTime\": \"", NowDate, "\",",
                                                                        "\"isPaperTicket\": false }");
                        }
                    }
                    else
                    {
                        foreach (String PaxName in PaxEName.Split(','))
                        {
                            string[] TmpPaxName = PaxName.Split('/');
                            XmlNode Traveler = TravelerMaster.SelectSingleNode(String.Format("item[lastName='{0}' and firstName='{1}']", TmpPaxName[0].Trim(), TmpPaxName[1].Trim()));

                            if (Traveler != null)
                            {
                                if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                                    FlightTravelerTicketNode += ",";

                                FlightTravelerTicketNode += String.Concat("{ \"method\": \"update\",",
                                                                            "\"name\": \"", Traveler.SelectSingleNode("lastName").InnerText, "/", Traveler.SelectSingleNode("firstName").InnerText, "\",",
                                                                            "\"ticketingStatusCode\": \"TST09\",",
                                                                            "\"bookingTravelerCode\": \"", Traveler.SelectSingleNode("bookingTravelerCode").InnerText, "\",",
                                                                            "\"ticketIssuedTime\": \"", NowDate, "\",",
                                                                            "\"isPaperTicket\": false }");
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                    {
                        string[] ServiceInfo = TravelHow("modification");
                        string PushData = String.Concat("{\"apiKey\": \"", TravelHowApiKey, "\",",
                                                            "\"condition\": {",
                                                                "\"bookingItemCode\": \"", BookingItemCode, "\",",
                                                                "\"item\": {",
                                                                    "\"flightTravelerTicket\": [",
                                                                        FlightTravelerTicketNode,
                                                                    "]",
                                                                "}",
                                                            "}}");

                        QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "TICKET_VOID", RQR, RQT);
                        SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                        XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                        cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                        if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                        else
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                        return ResXml;
                    }
                    else
                        throw new Exception("입력하신 탑승객명과 일치하는 탑승객이 없습니다.");
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "VOID"

        #region "환불(Refunded)"

        /// <summary>
        /// [트래블하우] 환불(Refunded)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="TicketNumber">티켓번호(공백: 전체환불, 여러개 입력시 콤마로 구분)</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 환불(Refunded)")]
        public XmlElement TravelHowTicketRefundRS(int OID, string BookingItemCode, string TicketNumber, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                XmlElement XmlBook = TravelHowSearchBookingDecRS(BookingItemCode, "/flightTravelerTicket/");

                if (XmlBook.SelectNodes("result/isSucceed").Count > 0 && XmlBook.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                {
                    XmlNode FlightTravelerTicket = XmlBook.SelectSingleNode("result/item/flightTravelerTicket");
                    string FlightTravelerTicketNode = string.Empty;
                    string NowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (String.IsNullOrWhiteSpace(TicketNumber))
                    {
                        foreach (XmlNode Item in FlightTravelerTicket.SelectNodes("item"))
                        {
                            if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                                FlightTravelerTicketNode += ",";

                            FlightTravelerTicketNode += String.Concat("{ \"method\": \"update\",",
                                                                        "\"ticketNo\": \"", aes.AESEncrypt("TravelHow", Item.SelectSingleNode("ticketNo").InnerText), "\",",
                                                                        "\"ticketingStatusCode\": \"TST06\",",
                                                                        "\"bookingTravelerCode\": \"", Item.SelectSingleNode("bookingTravelerCode").InnerText, "\",",
                                                                        "\"ticketIssuedTime\": \"", NowDate, "\",",
                                                                        "\"isPaperTicket\": false }");
                        }
                    }
                    else
                    {
                        foreach (String Item in TicketNumber.Split(','))
                        {
                            XmlNode TicketInfo = FlightTravelerTicket.SelectSingleNode(String.Format("item[ticketNo='{0}']", Item));

                            if (TicketInfo != null)
                            {
                                if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                                    FlightTravelerTicketNode += ",";

                                FlightTravelerTicketNode += String.Concat("{ \"method\": \"update\",",
                                                                            "\"ticketNo\": \"", aes.AESEncrypt("TravelHow", Item), "\",",
                                                                            "\"ticketingStatusCode\": \"TST06\",",
                                                                            "\"bookingTravelerCode\": \"", TicketInfo.SelectSingleNode("bookingTravelerCode").InnerText, "\",",
                                                                            "\"ticketIssuedTime\": \"", NowDate, "\",",
                                                                            "\"isPaperTicket\": false }");
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(FlightTravelerTicketNode))
                    {
                        string[] ServiceInfo = TravelHow("modification");
                        string PushData = String.Concat("{\"apiKey\": \"", TravelHowApiKey, "\",",
                                                            "\"condition\": {",
                                                                "\"bookingItemCode\": \"", BookingItemCode, "\",",
                                                                "\"item\": {",
                                                                     "\"flightTravelerTicket\": [",
                                                                        FlightTravelerTicketNode,
                                                                    "]",
                                                                "}",
                                                            "}}");

                        QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "TICKET_REFUND", RQR, RQT);
                        SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                        XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                        cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                        if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                        else
                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                        return ResXml;
                    }
                    else
                        throw new Exception("입력하신 티켓번호와 일치하는 정보가 없습니다.");
                }
                else
                    throw new Exception("예약 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "환불(Refunded)"

        #region "예약 Retreive"

        /// <summary>
        /// [트래블하우] 예약 Retreive
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="StockAirlineCode">Stock Airline 코드</param>
        /// <param name="NoticeType">요청 구분(flightTraveler/traveler/price/mileage/schedule/ticket/ticketTimeLimit)</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 Retreive")]
        public XmlElement TravelHowRetreiveRS(int OID, string BookingItemCode, string StockAirlineCode, string NoticeType, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                if (!String.IsNullOrWhiteSpace(NoticeType))
                {
                    string UpdateItems = string.Empty;

                    foreach (string UItem in NoticeType.Split('/'))
                        UpdateItems += String.Concat("<updateItems>", UItem, "</updateItems>");
                    
                    string[] ServiceInfo = TravelHow("retreive");
                    string PushData = String.Concat("<root>",
                                                                "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                                "<method>search</method>",
                                                                "<condition>",
                                                                    "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                                    "<stockAirlineCode>", StockAirlineCode, "</stockAirlineCode>",
                                                                    "<isChangedPNR>false</isChangedPNR>",
                                                                    //"<updateItems>flightTraveler</updateItems>",
                                                                    //"<updateItems>traveler</updateItems>",
                                                                    //"<updateItems>price</updateItems>",
                                                                    //"<updateItems>mileage</updateItems>",
                                                                    //"<updateItems>schedule</updateItems>",
                                                                    //"<updateItems>ticket</updateItems>",
                                                                    //"<updateItems>ticketTimeLimit</updateItems>",
                                                                    UpdateItems,
                                                                "</condition>",
                                                            "</root>");

                    //XmlDocument XmlDoc = new XmlDocument();
                    //XmlDoc.LoadXml(PushData);
                    //return XmlDoc.DocumentElement;

                    QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PNR_RETREIVE", RQR, RQT);
                    SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                    XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                    cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                    if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                    else
                        TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                    return ResXml;
                }
                else
                    throw new Exception("요청 구분을 입력하지 않았습니다.");
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 예약 Retreive(탑승객 정보 수정)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="StockAirlineCode">Stock Airline 코드</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 Retreive(탑승객 정보 수정)")]
        public XmlElement TravelHowRetreiveTravelerRS(int OID, string BookingItemCode, string StockAirlineCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                string[] ServiceInfo = TravelHow("retreive");
                string PushData = String.Concat("{\"apiKey\":\"", TravelHowApiKey, "\",\"method\":\"search\",\"condition\":{\"bookingItemCode\":\"", BookingItemCode, "\",\"stockAirlineCode\":\"", StockAirlineCode, "\",\"updateItems\":[\"flightTraveler\",\"traveler\"],\"isChangedPNR\":\"false\"}}");
                //string PushData = String.Concat("<root>",
                //                                            "<apiKey>", TravelHowApiKey, "</apiKey>",
                //                                            "<method>search</method>",
                //                                            "<condition>",
                //                                                "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                //                                                "<stockAirlineCode>", StockAirlineCode, "</stockAirlineCode>",
                //                                                "<isChangedPNR>false</isChangedPNR>",
                //                                                "<updateItems>flightTraveler,traveler</updateItems>",
                //                                            "</condition>",
                //                                        "</root>");

                //XmlDocument XmlDoc = new XmlDocument();
                //XmlDoc.LoadXml(PushData);
                //return XmlDoc.DocumentElement;

                QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PNR_RETREIVE_TRAVELER", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 예약 Retreive(스케쥴 정보 수정)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="StockAirlineCode">Stock Airline 코드</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 Retreive(스케쥴 정보 수정)")]
        public XmlElement TravelHowRetreiveScheduleRS(int OID, string BookingItemCode, string StockAirlineCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                string[] ServiceInfo = TravelHow("retreive");
                string PushData = String.Concat("<root>",
                                                            "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                            "<method>search</method>",
                                                            "<condition>",
                                                                "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                                "<stockAirlineCode>", StockAirlineCode, "</stockAirlineCode>",
                                                                "<isChangedPNR>false</isChangedPNR>",
                                                                "<updateItems>schedule</updateItems>",
                                                            "</condition>",
                                                        "</root>");

                //XmlDocument XmlDoc = new XmlDocument();
                //XmlDoc.LoadXml(PushData);
                //return XmlDoc.DocumentElement;

                QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PNR_RETREIVE_SCHEDULE", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        /// <summary>
        /// [트래블하우] 예약 Retreive(티켓 정보 수정)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">트래블하우 예약번호</param>
        /// <param name="StockAirlineCode">Stock Airline 코드</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "[트래블하우] 예약 Retreive(티켓 정보 수정)")]
        public XmlElement TravelHowRetreiveTicketRS(int OID, string BookingItemCode, string StockAirlineCode, int RQR, string RQT)
        {
            int QueueNumber = 0;
            int SyncNumber = 0;

            try
            {
                string[] ServiceInfo = TravelHow("retreive");
                string PushData = String.Concat("<root>",
                                                            "<apiKey>", TravelHowApiKey, "</apiKey>",
                                                            "<method>search</method>",
                                                            "<condition>",
                                                                "<bookingItemCode>", BookingItemCode, "</bookingItemCode>",
                                                                "<stockAirlineCode>", StockAirlineCode, "</stockAirlineCode>",
                                                                "<isChangedPNR>false</isChangedPNR>",
                                                                "<updateItems>flightTraveler</updateItems>",
                                                                "<updateItems>traveler</updateItems>",
                                                                "<updateItems>schedule</updateItems>",
                                                                "<updateItems>ticket</updateItems>",
                                                            "</condition>",
                                                        "</root>");

                //XmlDocument XmlDoc = new XmlDocument();
                //XmlDoc.LoadXml(PushData);
                //return XmlDoc.DocumentElement;

                QueueNumber = TravelHowQueueSave(OID, "", BookingItemCode, PushData, "PNR_RETREIVE_TICKET", RQR, RQT);
                SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);

                XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);
                cm.XmlFileSave(ResXml, "TravelHow", MethodBase.GetCurrentMethod().Name, "N", cm.GetGUID);

                if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "Y");
                else
                    TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, "N");

                return ResXml;
            }
            catch (Exception ex)
            {
                XmlElement ExceptionXml = new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, OID, 0).ToErrors;
                TravelHowQueueSyncRS(QueueNumber, SyncNumber, ExceptionXml, "N");

                return ExceptionXml;
            }
        }

        #endregion "예약 Retreive"

        #region "PUSH - 재실행"

        /// <summary>
        /// [트래블하우] 푸쉬 재실행
        /// </summary>
        /// <param name="QueueNumber">큐번호</param>
        /// <param name="BookingItemCode">TravelHow 예약번호</param>
        /// <param name="NoticeType">요청구분</param>
        /// <param name="PushData">요청데이타</param>
        [WebMethod(Description = "[트래블하우] 푸쉬 재실행")]
        public string TravelHowPushRetry(int QueueNumber, string BookingItemCode, string NoticeType, string PushData)
        {
            //성공여부
            string Success = "N";

            try
            {
                if (QueueNumber > 0)
                {
                    if (!String.IsNullOrWhiteSpace(BookingItemCode))
                    {
                        //TL업데이트
                        if (NoticeType.Equals("TL_UPDATE"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //예약수정
                        else if (NoticeType.Equals("BOOKING_UPDATE"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //예약취소
                        else if (NoticeType.Equals("BOOKING_CANCEL"))
                        {
                            string[] ServiceInfo = TravelHow("cancel");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //1:1문의 답변
                        else if (NoticeType.Equals("CONSULTING_ANSWER"))
                        {
                            string[] ServiceInfo = TravelHow("consulting");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //가상계좌번호
                        else if (NoticeType.Equals("VIRTUAL_ACCOUNT"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //결제요청 취소
                        else if (NoticeType.StartsWith("PAYMENTREQUEST_CANCEL"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //결제취소(Refunded)
                        else if (NoticeType.StartsWith("PAYMENT_CANCEL"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //VOID
                        else if (NoticeType.StartsWith("TICKET_VOID"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //환불(Refunded)
                        else if (NoticeType.StartsWith("TICKET_REFUND"))
                        {
                            string[] ServiceInfo = TravelHow("modification");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToJson(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        //예약 Retreive
                        else if (NoticeType.StartsWith("PNR_RETREIVE"))
                        {
                            string[] ServiceInfo = TravelHow("retreive");
                            int SyncNumber = TravelHowQueueSyncRQ(QueueNumber, ServiceInfo[1], ServiceInfo[0]);
                            XmlElement ResXml = XmlRequest.TravelHowSendToXml(ServiceInfo[1], ServiceInfo[0], PushData);

                            if (ResXml.SelectNodes("result/isSucceed").Count > 0 && ResXml.SelectSingleNode("result/isSucceed").InnerText.Equals("True"))
                                Success = "Y";

                            TravelHowQueueSyncRS(QueueNumber, SyncNumber, ResXml, Success);
                        }
                        ////프로모션 전체등록
                        //else if (NoticeType.Equals("PROMOTION_ADDALL"))
                        //{}
                        ////프로모션 전체삭제
                        //else if (NoticeType.Equals("PROMOTION_DELETEALL"))
                        //{}
                        ////프로모션 삭제
                        //else if (NoticeType.Equals("PROMOTION_DELETE"))
                        //{}
                    }
                    else
                        throw new Exception("트래블하우의 예약정보를 확인할 수 없습니다.");
                }
                else
                    throw new Exception("큐정보를 확인할 수 없습니다.");
            }
            catch (Exception ex)
            {
                throw new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0);
            }

            return Success;
        }

        #endregion "PUSH - 재실행"

        #region "알림 큐"

        /// <summary>
        /// 실시간 알림 저장
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="BookingItemCode">TravelHow 예약번호</param>
        /// <param name="PushXml">Push 전문</param>
        /// <param name="NoticeType">요청구분</param>
        /// <param name="RQR">작업 요청자(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns>큐번호</returns>
        protected int TravelHowQueueSave(int OID, string PNR, string BookingItemCode, string PushXml, string NoticeType, int RQR, string RQT)
        {
            int QueueNumber = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                    {
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_T_아이템예약_큐_제휴";

                        cmd.Parameters.Add("@서버명", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@품목코드", SqlDbType.Char, 2);
                        cmd.Parameters.Add("@주문번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@GDS주문번호", SqlDbType.VarChar, 30);
                        cmd.Parameters.Add("@제휴주문번호", SqlDbType.VarChar, 50);
                        cmd.Parameters.Add("@요청데이타", SqlDbType.VarChar, -1);
                        cmd.Parameters.Add("@요청구분", SqlDbType.VarChar, 30);
                        cmd.Parameters.Add("@요청자", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@요청단말기", SqlDbType.VarChar, 30);
                        cmd.Parameters.Add("@큐번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@서버명"].Value = Environment.MachineName;
                        cmd.Parameters["@사이트번호"].Value = 4716;
                        cmd.Parameters["@품목코드"].Value = "IA";
                        cmd.Parameters["@주문번호"].Value = OID;
                        cmd.Parameters["@GDS주문번호"].Value = PNR;
                        cmd.Parameters["@제휴주문번호"].Value = BookingItemCode;
                        cmd.Parameters["@요청데이타"].Value = String.IsNullOrWhiteSpace(PushXml) ? Convert.DBNull : PushXml;
                        cmd.Parameters["@요청구분"].Value = NoticeType;
                        cmd.Parameters["@요청자"].Value = RQR;
                        cmd.Parameters["@요청단말기"].Value = RQT;
                        cmd.Parameters["@큐번호"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();

                            QueueNumber = Convert.ToInt32(cmd.Parameters["@큐번호"].Value);
                        }
                        catch (Exception ex)
                        {
                            new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, OID, 0);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, OID, 0);
            }

            return QueueNumber;
        }

        /// <summary>
        /// 큐정보 싱크 요청
        /// </summary>
        /// <param name="QueueNumber">큐번호</param>
        /// <param name="URL">전송URL</param>
        /// <param name="Method">전송메소드</param>
        /// <returns>싱크번호</returns>
        protected int TravelHowQueueSyncRQ(int QueueNumber, string URL, string Method)
        {
            int SyncNumber = 0;
            
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                    {
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_T_아이템예약_큐_제휴_싱크요청";

                        cmd.Parameters.Add("@큐번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@서버명", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@전송URL", SqlDbType.VarChar, 300);
                        cmd.Parameters.Add("@전송메소드", SqlDbType.VarChar, 10);
                        cmd.Parameters.Add("@전송데이타", SqlDbType.VarChar, -1);
                        cmd.Parameters.Add("@싱크번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@큐번호"].Value = QueueNumber;
                        cmd.Parameters["@서버명"].Value = Environment.MachineName;
                        cmd.Parameters["@전송URL"].Value = URL;
                        cmd.Parameters["@전송메소드"].Value = Method;
                        cmd.Parameters["@전송데이타"].Value = Convert.DBNull;
                        cmd.Parameters["@싱크번호"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();

                            SyncNumber = Convert.ToInt32(cmd.Parameters["@싱크번호"].Value);
                        }
                        catch (Exception ex)
                        {
                            new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, 0, 0);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, 0, 0);
            }

            return SyncNumber;
        }

        /// <summary>
        /// 큐정보 싱크 응답
        /// </summary>
        /// <param name="QueueNumber">큐번호</param>
        /// <param name="SyncNumber">싱크번호</param>
        /// <param name="ResXml">응답XML</param>
        /// <param name="Success">성공여부</param>
        /// <returns></returns>
        protected void TravelHowQueueSyncRS(int QueueNumber, int SyncNumber, XmlElement ResXml, string Success)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                    {
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSV_T_아이템예약_큐_제휴_싱크응답";

                        cmd.Parameters.Add("@싱크번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@큐번호", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@응답데이타", SqlDbType.Xml, -1);
                        cmd.Parameters.Add("@성공여부", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                        cmd.Parameters["@싱크번호"].Value = SyncNumber;
                        cmd.Parameters["@큐번호"].Value = QueueNumber;
                        cmd.Parameters["@응답데이타"].Value = (ResXml != null) ? ResXml.OuterXml : Convert.DBNull;
                        cmd.Parameters["@성공여부"].Value = Success;
                        cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, 0, 0);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new MWSException(ex, hcc, "TravelHow", MethodBase.GetCurrentMethod().Name, 0, 0);
            }
        }

        #endregion "알림 큐"

        #region "기초코드"

        /// <summary>
        /// 트래블하우 카드 및 은행 코드 조회
        /// </summary>
        /// <param name="bankCardMasterName">모두투어 카드 및 은행명</param>
        /// <returns>트래블하우 카드 및 은행 코드</returns>
        protected string BankCardMasterCode(string bankCardMasterName)
        {
            string MasterCode = "";

            switch (bankCardMasterName.Trim())
            {
                
                case "국민은행": MasterCode = "B1002"; break;
                case "기업은행": MasterCode = "B1003"; break;
                case "농협": MasterCode = "B1004"; break;
                case "외환은행": MasterCode = "B1005"; break;
                case "우리은행": MasterCode = "B1009"; break;
                case "제일은행": MasterCode = "B1010"; break;
                case "신한은행": MasterCode = "B1011"; break;
                case "대구은행": MasterCode = "B1012"; break;
                case "부산은행": MasterCode = "B1013"; break;
                case "광주은행": MasterCode = "B1014"; break;
                case "제주은행": MasterCode = "B1015"; break;
                case "전북은행": MasterCode = "B1016"; break;
                case "경남은행": MasterCode = "B1018"; break;
                case "우체국": MasterCode = "B1019"; break;
                case "하나은행": MasterCode = "B1020"; break;
                case "외환":
                case "외환카드": MasterCode = "C1001"; break;
                case "롯데":
                case "롯데카드": MasterCode = "C1002"; break;
                case "현대":
                case "현대카드": MasterCode = "C1003"; break;
                case "국민":    
                case "국민카드":
                case "제휴국민카드": MasterCode = "C1004"; break;
                case "BC":
                case "BC카드": MasterCode = "C1005"; break;
                case "삼성":
                case "삼성카드":
                case "삼성앤마일리지":
                case "삼성프리미엄카드": MasterCode = "C1006"; break;
                case "신한(구,LG)":
                case "신한카드(구.LG)": MasterCode = "C1007"; break;
                case "신한":
                case "신한카드": MasterCode = "C1008"; break;
                case "광주":
                case "광주카드": MasterCode = "C1014"; break;
                case "전북":
                case "전북카드": MasterCode = "C1015"; break;
                case "NH":
                case "NH카드": MasterCode = "C1017"; break;
                case "씨티":
                case "씨티카드": MasterCode = "C1018"; break;
                case "수협":
                case "수협카드": MasterCode = "C1020"; break;
                case "제주":
                case "제주카드": MasterCode = "C1021"; break;
                case "우리":
                case "우리카드": MasterCode = "C1022"; break;
                case "강원":
                case "강원카드": MasterCode = "C1031"; break;
                case "한미(신세계)":
                case "한미카드(신세계)": MasterCode = "C1035"; break;
                case "한미":
                case "한미카드": MasterCode = "C1036"; break;
                case "하나":
                case "하나카드": MasterCode = "C1052"; break;
                case "제휴삼성":
                case "제휴삼성카드": MasterCode = "C1064"; break;
            }

            return MasterCode;
        }

        #endregion "기초코드"

        #endregion "트래블하우"

        #region "제휴사(공통)"

        [WebMethod(Description = "[제휴사] 랜딩페이지용")]
        public string CheckBookingInfoMKEYRS(int SNM, string MKEY)
        {
            return aes.AESDecrypt(AES256Cipher.KeyName(SNM), Server.UrlDecode(MKEY));
        }

        /// <summary>
        /// 제휴사 실시간 해외항공 랜딩용 서비스
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="MKEY">제휴사로부터 넘어온 랜딩 체크용 MKEY값</param>
        /// <returns></returns>
        [WebMethod(Description = "[제휴사] 랜딩페이지용")]
        public XmlElement CheckBookingInfoRS(int SNM, string MKEY)
        {
            try
            {
                string[] Params = null;

                //티몬(4925,4926),11번가(4924,4929),지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
                if (SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                {
                    Params = aes.AESDecrypt(AES256Cipher.KeyName(SNM), Server.UrlDecode(MKEY)).Split(':');
                    //고유번호 : Params[0]
                    //항공검색번호 : Params[2]
                    //사이트번호 : Params[4]
                    //여정타입 : Params[6]
                    //요금조건 : Params[8]
                    //운임번호 : Params[10]
                    //여정번호 : Params[12], Params[14], Params[16], Params[18]

                    //if (!SNM.Equals(cm.RequestInt(Params[4])))
                    //    throw new Exception("사이트 정보가 잘 못 되었습니다.");

                    if (!(Params[4].Equals("4925") || Params[4].Equals("4926") || Params[4].Equals("4924") || Params[4].Equals("4929") || Params[4].Equals("5020") || Params[4].Equals("5119") || Params[4].Equals("5161") || Params[4].Equals("5163") || Params[4].Equals("5162") || Params[4].Equals("5164")))
                        throw new Exception("사이트 정보가 잘 못 되었습니다.");
                    
                    SNM = cm.RequestInt(Params[4]);
                }
                else
                    throw new Exception("사용할 수 없는 사이트입니다.");

                DataSet ds = SelectInfo(SNM, Params[0], cm.RequestInt(Params[2]), cm.RequestInt(Params[10]), String.Format("{0},{1},{2},{3}", Params[12], Params[14], Params[16], Params[18]));
                DataTable dt1 = ds.Tables[0];
                DataTable dt2 = ds.Tables[1];

                if (dt1.Rows.Count > 0 && dt2.Rows.Count > 0)
                {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText = String.Format("{0}::{1}", Params[0], Params[2]);

                    //여정정보
                    XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
                    XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
                    XmlNode NewFlightIndex;
                    int idx = 0;

                    FlightInfo.Attributes.GetNamedItem("ptc").InnerText = Params[8];
                    FlightInfo.Attributes.GetNamedItem("rot").InnerText = Params[6];
                    FlightInfo.Attributes.GetNamedItem("opn").InnerText = dt1.Rows[0]["오픈"].ToString();

                    foreach (DataRow dr in dt2.Rows)
                    {
                        if (idx != Convert.ToInt32(dr["여정번호"]))
                        {
                            NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                            NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = dr["여정번호"].ToString();

                            XmlDocument SegXml = new XmlDocument();
                            SegXml.LoadXml(dr["XMLDATA"].ToString());

                            //랜딩페이지 항공상세정보 출력을 위한 속성 추가(이베이) ----> 추후 4924는 제외해야 함!!!!!!(2018-04-10,고재영)
                            if (Params[4].Equals("4924") || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                            {
                                foreach (XmlNode SegGroup in SegXml.SelectNodes("segGroup"))
                                {
                                    int TotalEFT = 0;
                                    int TotalEWT = 0;
                                    XmlAttribute JRT = SegXml.CreateAttribute("jrt");
                                    
                                    foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
                                    {
                                        XmlElement FlightInfoXml = mas.FlightInfoRS(SNM, Seg.Attributes.GetNamedItem("ddt").InnerText, "", Seg.Attributes.GetNamedItem("dlc").InnerText, Seg.Attributes.GetNamedItem("alc").InnerText, Seg.Attributes.GetNamedItem("mcc").InnerText, Seg.Attributes.GetNamedItem("occ").InnerText, Seg.Attributes.GetNamedItem("fln").InnerText);
                                        XmlAttribute EFT = SegXml.CreateAttribute("eft");
                                        XmlAttribute EWT = SegXml.CreateAttribute("ewt");

                                        if (FlightInfoXml != null && FlightInfoXml.SelectNodes("flightInfo").Count > 0)
                                        {
                                            if (Seg.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                                            {
                                                int LegEFT = 0;
                                                int LegGWT = 0;
                                                
                                                //존재하던 Leg 정보 삭제
                                                foreach (XmlNode Leg in Seg.SelectNodes("seg"))
                                                    Seg.RemoveChild(Leg);

                                                foreach (XmlNode LegInfo in FlightInfoXml.SelectNodes("flightInfo/flightIndex/segGroup/seg"))
                                                {
                                                    LegEFT += cm.ChangeMinutes(LegInfo.Attributes.GetNamedItem("eft").InnerText);
                                                    LegGWT += cm.ChangeMinutes(LegInfo.Attributes.GetNamedItem("gwt").InnerText);

                                                    Seg.AppendChild(SegXml.ImportNode(LegInfo, true));
                                                }

                                                EFT.InnerText = cm.ChangeTime(LegEFT);
                                                Seg.Attributes.Append(EFT);

                                                EWT.InnerText = LegGWT.Equals(0) ? "" : cm.ChangeTime(LegGWT);
                                                Seg.Attributes.Append(EWT);

                                                TotalEFT += LegEFT;
                                                TotalEWT += LegGWT;
                                            }
                                            else
                                            {
                                                EFT.InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg").Attributes.GetNamedItem("eft").InnerText;
                                                Seg.Attributes.Append(EFT);

                                                EWT.InnerText = FlightInfoXml.SelectSingleNode("flightInfo/flightIndex/segGroup/seg").Attributes.GetNamedItem("gwt").InnerText;
                                                Seg.Attributes.Append(EWT);

                                                TotalEFT += cm.ChangeMinutes(EFT.InnerText);
                                                TotalEWT += cm.ChangeMinutes(EWT.InnerText);
                                            }
                                        }
                                        else
                                        {
                                            EFT.InnerText = "";
                                            Seg.Attributes.Append(EFT);

                                            EWT.InnerText = "";
                                            Seg.Attributes.Append(EWT);
                                        }
                                    }

                                    if ((TotalEFT + TotalEWT) > 0)
                                    {
                                        JRT.InnerText = cm.ChangeTime(TotalEFT + TotalEWT);
                                        SegGroup.Attributes.Append(JRT);

                                        SegGroup.Attributes.GetNamedItem("eft").InnerText = cm.ChangeTime(TotalEFT);
                                        SegGroup.Attributes.GetNamedItem("ewt").InnerText = TotalEWT.Equals(0) ? "" : cm.ChangeTime(TotalEWT);
                                    }
                                    else
                                    {
                                        JRT.InnerText = "";
                                        SegGroup.Attributes.Append(JRT);
                                    }
                                }
                            }

                            NewFlightIndex.AppendChild(XmlDoc.ImportNode(SegXml.FirstChild, true));

                            idx = Convert.ToInt32(dr["여정번호"]);
                        }
                    }

                    FlightInfo.RemoveChild(FlightIndex);

                    //운임정보
                    XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");

                    XmlDocument FareXml = new XmlDocument();
                    FareXml.LoadXml(dt1.Rows[0]["XMLDATA"].ToString());

                    PriceInfo.ReplaceChild(XmlDoc.ImportNode(FareXml.FirstChild, true), PriceInfo.SelectSingleNode("priceIndex"));
                    PriceInfo.SelectSingleNode("priceIndex").RemoveChild(PriceInfo.SelectSingleNode("priceIndex/segGroup"));

                    //Price에 TASF를 합해준다.
                    int SumPrice = Convert.ToInt32(PriceInfo.SelectSingleNode("priceIndex/summary").Attributes.GetNamedItem("price").InnerText);
                    int SumTASF = Convert.ToInt32(PriceInfo.SelectSingleNode("priceIndex/summary").Attributes.GetNamedItem("tasf").InnerText);

                    PriceInfo.SelectSingleNode("priceIndex/summary").Attributes.GetNamedItem("price").InnerText = (SumPrice + SumTASF).ToString();

                    return XmlDoc.DocumentElement;
                }
                else
                    throw new Exception("항공 운임 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// [제휴사] 운임규정 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="MKEY">제휴사로부터 넘어온 규정 조회용 MKEY값</param>
        /// <returns></returns>
        [WebMethod(Description = "[제휴사] 운임규정 조회")]
        public XmlElement SearchRuleRS(int SNM, string MKEY)
        {
            try
            {
                string[] Params = null;

                //티몬(4925,4926),11번가(4924,4929),지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
                if (SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                {
                    Params = aes.AESDecrypt(AES256Cipher.KeyName(SNM), Server.UrlDecode(MKEY)).Split(':');
                    //고유번호 : Params[0]
                    //항공검색번호 : Params[2]
                    //사이트번호 : Params[4]
                    //여정타입 : Params[6]
                    //요금조건 : Params[8]
                    //운임번호 : Params[10]
                    //여정번호 : Params[12], Params[14], Params[16], Params[18]

                    //if (!SNM.Equals(cm.RequestInt(Params[4])))
                    //    throw new Exception("사이트 정보가 잘 못 되었습니다.");

                    if (!(Params[4].Equals("4925") || Params[4].Equals("4926") || Params[4].Equals("4924") || Params[4].Equals("4929") || Params[4].Equals("5020") || Params[4].Equals("5119") || Params[4].Equals("5161") || Params[4].Equals("5163") || Params[4].Equals("5162") || Params[4].Equals("5164")))
                        throw new Exception("사이트 정보가 잘 못 되었습니다.");
                    
                    SNM = cm.RequestInt(Params[4]);
                }
                else
                    throw new Exception("사용할 수 없는 사이트입니다.");

                DataSet ds = SelectInfo(SNM, Params[0], cm.RequestInt(Params[2]), cm.RequestInt(Params[10]), String.Format("{0},{1},{2},{3}", Params[12], Params[14], Params[16], Params[18]));
                DataTable dt1 = ds.Tables[0];
                DataTable dt2 = ds.Tables[1];

                XmlDocument FareXml = new XmlDocument();
                FareXml.LoadXml(dt1.Rows[0]["XMLDATA"].ToString());

                string PMID = dt1.Rows[0]["프로모션번호"].ToString();
                string PFG = FareXml.SelectSingleNode("priceIndex/paxFareGroup").OuterXml;
                XmlNodeList CabinList = FareXml.SelectNodes("priceIndex/paxFareGroup/paxFare[1]/segFareGroup/segFare/fare/cabin");

                int idx = dt2.Rows.Count;
                int[] INO = new Int32[idx];
                string[] DTD = new String[idx];
                string[] DTT = new String[idx];
                string[] ARD = new String[idx];
                string[] ART = new String[idx];
                string[] DLC = new String[idx];
                string[] ALC = new String[idx];
                string[] MCC = new String[idx];
                string[] OCC = new String[idx];
                string[] FLN = new String[idx];
                string[] RBD = new String[idx];
                int i = 0;

                foreach (DataRow dr in dt2.Rows)
                {
                    INO[i] = Convert.ToInt32(dr["여정번호"]);
                    DTD[i] = dr["출발일"].ToString();
                    DTT[i] = dr["출발시간"].ToString();
                    ARD[i] = dr["도착일"].ToString();
                    ART[i] = dr["도착시간"].ToString();
                    DLC[i] = dr["출발공항"].ToString();
                    ALC[i] = dr["도착공항"].ToString();
                    MCC[i] = dr["항공사"].ToString();
                    OCC[i] = dr["운항항공사"].ToString();
                    FLN[i] = dr["편명"].ToString();
                    RBD[i] = CabinList[i].Attributes.GetNamedItem("rbd").InnerText;
                    i++;
                }

                return mas.SearchRuleRS(SNM, PMID, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PFG);
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 항공 선택 정보
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="SKEY">고유번호</param>
        /// <param name="IDX">항공검색번호</param>
        /// <param name="FareNumber">운임번호</param>
        /// <param name="AvailNumbers">여정번호</param>
        /// <returns></returns>
        public DataSet SelectInfo(int SNM, string SKEY, int IDX, int FareNumber, string AvailNumbers)
        {
            DataSet ds = null;

            try
            {
                using (ds = new DataSet())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                        {
                            SqlDataAdapter adp = new SqlDataAdapter(cmd);

                            cmd.Connection = conn;
                            cmd.CommandTimeout = 10;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSV_S_해외항공검색_선택정보";

                            cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@항공검색번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@고유번호", SqlDbType.Char, 35);
                            cmd.Parameters.Add("@운임번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@여정번호", SqlDbType.VarChar, 100);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@사이트번호"].Value = SNM;
                            cmd.Parameters["@항공검색번호"].Value = IDX;
                            cmd.Parameters["@고유번호"].Value = SKEY;
                            cmd.Parameters["@운임번호"].Value = FareNumber;
                            cmd.Parameters["@여정번호"].Value = AvailNumbers;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                            adp.Fill(ds);
                            adp.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0);
            }

            return ds;
        }

        #endregion "제휴사(공통)"

        #region "제휴사(인벤토리)"

        /// <summary>
        /// 제휴사 인벤토리항공 랜딩용 서비스
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="MKEY">제휴사로부터 넘어온 랜딩 체크용 MKEY값(재고번호^성인수^소아수^유아수)</param>
        /// <returns></returns>
        [WebMethod(Description = "[제휴사] 랜딩페이지용(인벤토리)")]
        public XmlElement CheckInvBookingInfoRS(int SNM, string MKEY)
        {
            try
            {
                string[] Params = MKEY.Split('^');

                DataSet ds = SelectInvInfo(Convert.ToInt32(Params[0]), Convert.ToInt32(Params[1]), Convert.ToInt32(Params[2]), Convert.ToInt32(Params[3]));
                DataTable dt1 = ds.Tables[0];

                if (dt1.Rows.Count > 0)
                {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.LoadXml(dt1.Rows[0][0].ToString());

                    XmlAttribute TimeStamp = XmlDoc.CreateAttribute("timeStamp");
                    TimeStamp.InnerText = cm.TimeStamp;

                    XmlAttribute RootRef = XmlDoc.CreateAttribute("ref");
                    RootRef.InnerText = String.Format("{0}::{1}", SNM, MKEY);

                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.Append(TimeStamp);
                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.Append(RootRef);

                    return XmlDoc.DocumentElement;
                }
                else
                    throw new Exception("항공 운임 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// 인벤토리 항공 선택 정보
        /// </summary>
        /// <param name="InventoryNumber">재고번호</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <returns></returns>
        public DataSet SelectInvInfo(int InventoryNumber, int ADC, int CHC, int IFC)
        {
            DataSet ds = null;

            try
            {
                using (ds = new DataSet())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MODEWARE"].ConnectionString))
                        {
                            SqlDataAdapter adp = new SqlDataAdapter(cmd);

                            cmd.Connection = conn;
                            cmd.CommandTimeout = 10;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "DBO.WSV_S_아이템예약_단체항공_선택정보";

                            cmd.Parameters.Add("@재고번호", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@성인수", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@소아수", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@유아수", SqlDbType.Int, 0);
                            cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                            cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                            cmd.Parameters["@재고번호"].Value = InventoryNumber;
                            cmd.Parameters["@성인수"].Value = ADC;
                            cmd.Parameters["@소아수"].Value = CHC;
                            cmd.Parameters["@유아수"].Value = IFC;
                            cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                            cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                            adp.Fill(ds);
                            adp.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MWSException(ex, hcc, "AllianceService", MethodBase.GetCurrentMethod().Name, 0, 0);
            }

            return ds;
        }

        #endregion "제휴사(인벤토리)"

        #region "메서드 설명"

        /// <summary>
		/// WebMethod의 입력 파라미터 및 출력값에 대한 설명
		/// </summary>
		/// <param name="WebMethodName">웹메서드명</param>
		/// <returns></returns>
		[WebMethod(Description = "WebMethod의 입력 파라미터 및 출력값에 대한 설명")]
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

                sqlParam[0].Value = 387;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = WebMethodName;

                log.LogDBSave(sqlParam);
            }
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
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 387, 0, 0).ToErrors;
			}
		}

		/// <summary>
		/// WebMethod의 Request 또는 Resonse XML에 대한 설명
		/// </summary>
		/// <param name="WebMethodName">웹메서드명</param>
		/// <param name="Gubun">구분(RQ:Request XML, RS:Response XML)</param>
		/// <returns></returns>
		[WebMethod(Description = "WebMethod의 Request 또는 Resonse XML에 대한 설명")]
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

                sqlParam[0].Value = 388;
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
                return new MWSExceptionMode(ex, hcc, LogGUID, "AllianceService", MethodBase.GetCurrentMethod().Name, 388, 0, 0).ToErrors;
			}
		}

		#endregion "메서드 설명"
	}
}