using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// 할인 항공(해외) 조회를 위한 웹서비스(통합용)
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class AirDiscount : System.Web.Services.WebService
	{
		Common cm;
		ModeConfig mc;
		AmadeusAirService amd;
        LogSave log;
		HttpContext hcc;

		public AirDiscount()
		{
			cm = new Common();
			mc = new ModeConfig();
			amd = new AmadeusAirService();
            log = new LogSave();
			hcc = HttpContext.Current;
		}

		#region "할인항공 운임"

		/// <summary>
		/// SQL을 이용한 Oracle DB결과 리턴
		/// </summary>
		/// <param name="SQL">오라클 쿼리</param>
		/// <returns></returns>
		private XmlElement OracleFare(string SQL)
		{
			using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["IFARE"].ConnectionString))
			{
				try
				{
					conn.Open();

					using (OracleCommand cmd = new OracleCommand())
					{
						OracleDataAdapter adp = new OracleDataAdapter(cmd);
						DataSet ds = new DataSet("airDiscount");						

						cmd.Connection = conn;
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = SQL;

						adp.Fill(ds, "fare");
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
                    throw new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
				}
				catch (Exception ex)
				{
                    throw new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
				}
			}
		}

		/// <summary>
		/// 할인항공 DB 셀렉트시 출력 내용
		/// </summary>
		public static string SelectColumn(int SNM)
		{
			StringBuilder sb = new StringBuilder(8192);
			sb.Append("D1.IF_SEQNO, D1.FARE_NO, D1.FARE_BASIS, D1.DEPLOY_FARE_KIND, D1.RSV_AGT_CD, D1.FILE_AGT_CD, D1.FARE_RULE_NO, D1.IATA_AIR_CD, D4.AREA_CD, D2.NA_CD, ");
            sb.Append("D1.DEP_CITY_CD, (SELECT CITY_KOR_NM FROM TB_COM_CD130 WHERE CITY_CD = D1.DEP_CITY_CD) AS DEP_CITY_KOR_NM, D1.ARR_CITY_CD, D2.CITY_KOR_NM AS ARR_CITY_KOR_NM, ");
            sb.Append("D1.ADT_FARE, D1.ADT_FARE AS ADT_DSCNT_FARE, D5.ADT_BAF + NVL(D5.ADT_ADD_BAF, 0) AS ADT_BAF, D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) AS ADT_TAX_AMT, D1.CHD_FARE, D1.CHD_FARE AS CHD_DSCNT_FARE, D1.INF_FARE, ");
            sb.Append("D1.DEP_WKDAY_IDX, D1.IB_WKDAY_IDX, CASE WHEN D1.CABIN_SEAT_GRAD NOT IN ('F', 'C') THEN 'Y' ELSE D1.CABIN_SEAT_GRAD END AS SERVICE_CLASS, D1.CABIN_SEAT_GRAD, D1.PAX_TYPE, D1.PAX_TYPE AS PAX_TYPE_DESC, D1.MIN_STAY_DATE, D1.MAX_STAY_DATE, D1.AP_DAYCNT, D1.TRIP_TYPE, D1.RSV_SEAT_GRAD, D1.IB_DATE_ASIN_YN, D1.TOUR_CD, D1.COMM_FLAG, D1.COMM, D1.NO_OF_STOP, D1.RTG_CITY_NM, D1.RTG_AIR_NM, D1.RTG_SEAT_GRAD, D1.RTG_FLTNO, D1.PAX_LIMIT_INWON, D1.PAX_LIMIT_AGE, ");
            sb.Append("D1.FARE_FLAG1, D1.FARE_FLAG2, D1.FARE_FLAG3, D1.FARE_FLAG4, D1.FARE_FLAG5, D1.FARE_CREAT_DATE, D1.OPERT_FLAG, D1.REG_USR_ID, D1.REG_DTM, D1.UPD_USR_ID, D1.UPD_DTM, ");
			sb.Append("D1.EVENT_CD1, D1.EVENT_WKDAY_IDX1, D1.EVENT_BGN_HHMI1, D1.EVENT_END_HHMI1, D1.EVENT_BGN_DATE1, D1.EVENT_END_DATE1, ");
			sb.Append("D1.EVENT_CD2, D1.EVENT_WKDAY_IDX2, D1.EVENT_BGN_HHMI2, D1.EVENT_END_HHMI2, D1.EVENT_BGN_DATE2, D1.EVENT_END_DATE2, ");
			sb.Append("D1.EVENT_CD3, D1.EVENT_WKDAY_IDX3, D1.EVENT_BGN_HHMI3, D1.EVENT_END_HHMI3, D1.EVENT_BGN_DATE3, D1.EVENT_END_DATE3, ");
			sb.Append("D1.EVENT_CD4, D1.EVENT_WKDAY_IDX4, D1.EVENT_BGN_HHMI4, D1.EVENT_END_HHMI4, D1.EVENT_BGN_DATE4, D1.EVENT_END_DATE4, ");
			sb.Append("D1.EVENT_CD5, D1.EVENT_WKDAY_IDX5, D1.EVENT_BGN_HHMI5, D1.EVENT_END_HHMI5, D1.EVENT_BGN_DATE5, D1.EVENT_END_DATE5, ");
			sb.Append("LTRIM(");
			sb.Append("DECODE(LTRIM(D1.FARE_BGN_DATE), NULL, NULL, '/' || LTRIM(D1.FARE_BGN_DATE)) || DECODE(LTRIM(D1.FARE_END_DATE), NULL, NULL, '-' || LTRIM(D1.FARE_END_DATE)),'/') FARE_DATE, ");
			sb.Append("LTRIM(");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE1 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE1 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE1 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE1 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE2 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE2 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE2 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE2 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE3 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE3 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE3 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE3 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE4 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE4 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE4 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE4 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE5 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE5 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE5 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE5 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE6 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE6 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE6 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE6 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE7 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE7 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE7 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE7 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE8 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE8 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE8 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE8 )) || ");
			sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE9 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE9 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE9 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE9 )) || ");
            sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE10), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE10)) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE10), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE10)),'/') APPLY_DATE, ");
			sb.Append("LTRIM(");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE1 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE1 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE1 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE1 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE2 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE2 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE2 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE2 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE3 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE3 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE3 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE3 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE4 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE4 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE4 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE4 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE5 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE5 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE5 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE5 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE6 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE6 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE6 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE6 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE7 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE7 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE7 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE7 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE8 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE8 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE8 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE8 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE9 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE9 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE9 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE9 )) || ");
			sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE10), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE10)) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE10), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE10)),'/') SALE_DATE, ");
			sb.Append(String.Format("{0} AS SITE_NO ", SNM));

			return sb.ToString();
		}

		/// <summary>
		/// 할인항공권 조회
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="ARC">지역 코드(ASIA:동남아, JPN:일본, CHI:중국/대만/홍콩, EUR:유럽, AMCA:미주, SOPA:남태평양/괌/사이판, AFR:중동/아프리카, CSAM:중남미)</param>
		/// <param name="CRC">국가 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="SAC">항공사 코드</param>
		/// <param name="FLAG1">FLAG1(Y/N)</param>
		/// <param name="FLAG2">FLAG2(Y/N)</param>
		/// <param name="FLAG3">FLAG3(Y/N)</param>
		/// <param name="FLAG4">FLAG4(Y/N)</param>
		/// <param name="FLAG5">FLAG5(Y/N)</param>
		/// <returns></returns>
		[WebMethod(Description = "할인항공권 조회")]
		public XmlElement SearchAirDiscountRS(int SNM, string ARC, string CRC, string DLC, string ALC, string SAC, string FLAG1, string FLAG2, string FLAG3, string FLAG4, string FLAG5)
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
                        new SqlParameter("@요청10", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 433;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = ARC;
                sqlParam[8].Value = CRC;
                sqlParam[9].Value = DLC;
                sqlParam[10].Value = ALC;
                sqlParam[11].Value = SAC;
                sqlParam[12].Value = FLAG1;
                sqlParam[13].Value = FLAG2;
                sqlParam[14].Value = FLAG3;
                sqlParam[15].Value = FLAG4;
                sqlParam[16].Value = FLAG5;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                StringBuilder sb = new StringBuilder(8192);
                sb.Append(String.Format("SELECT {0}", SelectColumn(SNM)));
                sb.Append("FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4, TB_TAI_FB200 D5 ");
                sb.Append("WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ");
                sb.Append("AND D1.RTG_CITY_NM = D5.RTG_CITY_NM AND D1.RTG_AIR_NM = D5.RTG_AIR_NM AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.CABIN_SEAT_GRAD AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.RSV_SEAT_GRAD ");
                sb.Append("AND D1.USE_YN = 'Y' AND D1.RSV_AGT_CD = 'SELK138AB' AND D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) > 0 ");
                sb.Append("AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");

                if (!String.IsNullOrWhiteSpace(ARC))
                    sb.Append(String.Format("AND D4.AREA_CD = '{0}' ", ARC));

                if (!String.IsNullOrWhiteSpace(CRC))
                    sb.Append(String.Format("AND D2.NA_CD = '{0}' ", CRC));

                if (!String.IsNullOrWhiteSpace(DLC))
                    sb.Append(String.Format("AND CASE WHEN '{0}' = 'SEL' AND D1.DEP_CITY_CD IN ('SEL', 'GMP', 'ICN') THEN 'Y' WHEN '{0}' = 'ICN' AND D1.DEP_CITY_CD IN ('SEL', 'ICN') THEN 'Y' WHEN '{0}' = 'GMP' AND D1.DEP_CITY_CD IN ('SEL', 'GMP') THEN 'Y' WHEN '{0}' = D1.DEP_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", DLC));

                if (!String.IsNullOrWhiteSpace(ALC))
                    sb.Append(String.Format("AND CASE WHEN '{0}' = 'NRT' AND D1.ARR_CITY_CD IN ('TYO', 'NRT') THEN 'Y' WHEN '{0}' = 'HND' AND D1.ARR_CITY_CD IN ('TYO', 'HND') THEN 'Y' WHEN '{0}' = 'TYO' AND D1.ARR_CITY_CD IN ('TYO', 'HND', 'NRT') THEN 'Y' WHEN '{0}' = D1.ARR_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", ALC));

                if (!String.IsNullOrWhiteSpace(SAC))
                {
                    if (SAC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.IATA_AIR_CD IN ({0}) ", SplitForSQL(SAC, ",")));
                    else
                        sb.Append(String.Format("AND D1.IATA_AIR_CD = '{0}' ", SAC));
                }

                if (!String.IsNullOrWhiteSpace(FLAG1))
                    sb.Append(String.Format("AND D1.FARE_FLAG1 = '{0}' ", FLAG1));

                if (!String.IsNullOrWhiteSpace(FLAG2))
                    sb.Append(String.Format("AND D1.FARE_FLAG2 = '{0}' ", FLAG2));

                if (!String.IsNullOrWhiteSpace(FLAG3))
                    sb.Append(String.Format("AND D1.FARE_FLAG3 = '{0}' ", FLAG3));

                if (!String.IsNullOrWhiteSpace(FLAG4))
                    sb.Append(String.Format("AND D1.FARE_FLAG4 = '{0}' ", FLAG4));

                if (!String.IsNullOrWhiteSpace(FLAG5))
                    sb.Append(String.Format("AND D1.FARE_FLAG5 = '{0}' ", FLAG5));

                return ApplyPromotion(OracleFare(sb.ToString()), SearchPromotionList(SNM, ARC, CRC, DLC, ALC, SAC));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 433, 0, 0).ToErrors;
            }
		}

		/// <summary>
		/// 할인항공권 리스트(메인페이지용)
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <returns></returns>
		[WebMethod(Description = "할인항공권 리스트(메인페이지용)")]
		public XmlElement SearchAirDiscountMainRS(int SNM, string DLC)
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

                sqlParam[0].Value = 431;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = DLC;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                StringBuilder sb = new StringBuilder(8192);
                sb.Append(String.Format("SELECT {0}", SelectColumn(SNM)));
                sb.Append("FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4, TB_TAI_FB200 D5 ");
                sb.Append("WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ");
                sb.Append("AND D1.RTG_CITY_NM = D5.RTG_CITY_NM AND D1.RTG_AIR_NM = D5.RTG_AIR_NM AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.CABIN_SEAT_GRAD AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.RSV_SEAT_GRAD ");
                sb.Append("AND D1.USE_YN = 'Y' AND D1.RSV_AGT_CD = 'SELK138AB' AND D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) > 0 ");
                sb.Append("AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");

                if (!String.IsNullOrWhiteSpace(DLC))
                    sb.Append(String.Format("AND CASE WHEN '{0}' = 'SEL' AND D1.DEP_CITY_CD IN ('SEL', 'GMP', 'ICN') THEN 'Y' WHEN '{0}' = 'ICN' AND D1.DEP_CITY_CD IN ('SEL', 'ICN') THEN 'Y' WHEN '{0}' = 'GMP' AND D1.DEP_CITY_CD IN ('SEL', 'GMP') THEN 'Y' WHEN '{0}' = D1.DEP_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", DLC));

                sb.Append("AND D1.FARE_FLAG2 = 'Y' ");

                return ApplyPromotion(OracleFare(sb.ToString()), SearchPromotionList(SNM, "", "", DLC, "", ""));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 431, 0, 0).ToErrors;
            }
		}

		/// <summary>
		/// 할인항공권 리스트(기획전용)
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="ARC">지역 코드(ASIA:동남아, JPN:일본, CHI:중국/대만/홍콩, EUR:유럽, AMCA:미주, SOPA:남태평양/괌/사이판, AFR:중동/아프리카, CSAM:중남미)</param>
		/// <param name="CRC">국가 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="SAC">항공사 코드</param>
		/// <param name="FLAG3">FLAG3(Y/N)</param>
		/// <param name="FLAG4">FLAG4(Y/N)</param>
		/// <param name="FLAG5">FLAG5(Y/N)</param>
		/// <returns></returns>
		[WebMethod(Description = "할인항공권 리스트(기획전용)")]
		public XmlElement SearchAirDiscountEvent1RS(int SNM, string ARC, string CRC, string DLC, string ALC, string SAC, string FLAG3, string FLAG4, string FLAG5)
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
                        new SqlParameter("@요청8", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 430;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = ARC;
                sqlParam[8].Value = CRC;
                sqlParam[9].Value = DLC;
                sqlParam[10].Value = ALC;
                sqlParam[11].Value = SAC;
                sqlParam[12].Value = FLAG3;
                sqlParam[13].Value = FLAG4;
                sqlParam[14].Value = FLAG5;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                StringBuilder sb = new StringBuilder(8192);
                sb.Append(String.Format("SELECT {0}", SelectColumn(SNM)));
                sb.Append("FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4, TB_TAI_FB200 D5 ");
                sb.Append("WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ");
                sb.Append("AND D1.RTG_CITY_NM = D5.RTG_CITY_NM AND D1.RTG_AIR_NM = D5.RTG_AIR_NM AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.CABIN_SEAT_GRAD AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.RSV_SEAT_GRAD ");
                sb.Append("AND D1.USE_YN = 'Y' AND D1.RSV_AGT_CD = 'SELK138AB' AND D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) > 0 ");
                sb.Append("AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");

                if (!String.IsNullOrWhiteSpace(ARC))
                {
                    if (ARC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D4.AREA_CD IN ({0}) ", SplitForSQL(ARC, ",")));
                    else
                        sb.Append(String.Format("AND D4.AREA_CD = '{0}' ", ARC));
                }

                if (!String.IsNullOrWhiteSpace(CRC))
                {
                    if (CRC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D2.NA_CD IN ({0}) ", SplitForSQL(CRC, ",")));
                    else
                        sb.Append(String.Format("AND D2.NA_CD = '{0}' ", CRC));
                }

                if (!String.IsNullOrWhiteSpace(DLC))
                {
                    if (DLC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.DEP_CITY_CD IN ({0}) ", SplitForSQL(DLC, ",")));
                    else
                        sb.Append(String.Format("AND CASE WHEN '{0}' = 'SEL' AND D1.DEP_CITY_CD IN ('SEL', 'GMP', 'ICN') THEN 'Y' WHEN '{0}' = 'ICN' AND D1.DEP_CITY_CD IN ('SEL', 'ICN') THEN 'Y' WHEN '{0}' = 'GMP' AND D1.DEP_CITY_CD IN ('SEL', 'GMP') THEN 'Y' WHEN '{0}' = D1.DEP_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", DLC));
                }

                if (!String.IsNullOrWhiteSpace(ALC))
                {
                    if (ALC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.ARR_CITY_CD IN ({0}) ", SplitForSQL(ALC, ",")));
                    else
                        sb.Append(String.Format("AND CASE WHEN '{0}' = 'NRT' AND D1.ARR_CITY_CD IN ('TYO', 'NRT') THEN 'Y' WHEN '{0}' = 'HND' AND D1.ARR_CITY_CD IN ('TYO', 'HND') THEN 'Y' WHEN '{0}' = 'TYO' AND D1.ARR_CITY_CD IN ('TYO', 'HND', 'NRT') THEN 'Y' WHEN '{0}' = D1.ARR_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", ALC));
                }

                if (!String.IsNullOrWhiteSpace(SAC))
                {
                    if (SAC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.IATA_AIR_CD IN ({0}) ", SplitForSQL(SAC, ",")));
                    else
                        sb.Append(String.Format("AND D1.IATA_AIR_CD = '{0}' ", SAC));
                }

                if (!String.IsNullOrWhiteSpace(FLAG3))
                    sb.Append(String.Format("AND D1.FARE_FLAG3 = '{0}' ", FLAG3));

                if (!String.IsNullOrWhiteSpace(FLAG4))
                    sb.Append(String.Format("AND D1.FARE_FLAG4 = '{0}' ", FLAG4));

                if (!String.IsNullOrWhiteSpace(FLAG5))
                    sb.Append(String.Format("AND D1.FARE_FLAG5 = '{0}' ", FLAG5));

                return ApplyPromotion(OracleFare(sb.ToString()), SearchPromotionList(SNM, ARC, CRC, DLC, ALC, SAC));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 430, 0, 0).ToErrors;
            }
		}

        //[WebMethod(Description = "할인항공권 리스트(이벤트용)")]
        //public XmlElement SearchAirDiscountEvent2RS(int SNM, string ARC, string CRC, string DLC, string ALC, string SAC, string FLAG3, string FLAG4, string FLAG5)
        //{
        //    StringBuilder sb = new StringBuilder(8192);
        //    sb.Append(String.Format("SELECT {0}", SelectColumn(SNM)));
        //    sb.Append("FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4, TB_TAI_FB200 D5 ");
        //    sb.Append("WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ");
        //    sb.Append("AND D1.RTG_CITY_NM = D5.RTG_CITY_NM AND D1.RTG_AIR_NM = D5.RTG_AIR_NM AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.CABIN_SEAT_GRAD AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.RSV_SEAT_GRAD ");
        //    sb.Append("AND D1.USE_YN = 'Y' AND D1.RSV_AGT_CD = 'SELK138AB' AND D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) > 0 ");
        //    sb.Append("AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ");
        //    sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE1  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE2  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE3  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE4  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE5  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE6  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE7  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE8  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE9  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
        //    sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE1  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE2  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE3  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE4  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE5  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE6  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE7  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE8  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE9  THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
        //    sb.Append("AND D1.DEP_CITY_CD = 'ICN'");
        //    sb.Append("AND D1.ARR_CITY_CD = 'HND'");

        //    return ApplyPromotion(OracleFare(sb.ToString()), SearchPromotionList(SNM, ARC, CRC, DLC, ALC, SAC));
        //}

		/// <summary>
		/// 출발지 공항리스트
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <returns></returns>
		[WebMethod(Description = "출발지 공항리스트")]
		public XmlElement SearchDepAirportRS(int SNM)
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
                        new SqlParameter("@GUID", SqlDbType.VarChar, 50)
                    };

                sqlParam[0].Value = 439;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            return OracleFare(String.Concat("SELECT DEP_CITY_CD ",
											"FROM TB_TAI_FB100 ",
											"WHERE USE_YN = 'Y' AND RSV_AGT_CD = 'SELK138AB' ",
                                            "AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN FARE_BGN_DATE AND FARE_END_DATE ",
                                            "AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ",
                                            "AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ",
                                            "GROUP BY DEP_CITY_CD"));
		}

		/// <summary>
		/// 도착지 공항리스트
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="ARC">지역 코드(ASIA:동남아, JPN:일본, CHI:중국/대만/홍콩, EUR:유럽, AMCA:미주, SOPA:남태평양/괌/사이판, AFR:중동/아프리카, CSAM:중남미)</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <returns></returns>
		[WebMethod(Description = "도착지 공항리스트")]
		public XmlElement SearchArrAirportRS(int SNM, string ARC, string DLC)
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

                sqlParam[0].Value = 434;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = ARC;
                sqlParam[8].Value = DLC;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            return OracleFare(String.Concat("SELECT D4.AREA_CD, D2.NA_CD, D1.DEP_CITY_CD, D1.ARR_CITY_CD ",
											"FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4 ",
											"WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ",
											"AND D1.USE_YN = 'Y' ",
											"AND D1.RSV_AGT_CD = 'SELK138AB' ",
                                            "AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ",
                                            "AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ",
                                            "AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ",
                                            (String.IsNullOrWhiteSpace(ARC)) ? "" : String.Format("AND D4.AREA_CD = '{0}' ", ARC),
											(String.IsNullOrWhiteSpace(DLC)) ? "" : String.Format("AND D1.DEP_CITY_CD = '{0}' ", DLC),
											"GROUP BY D4.AREA_CD, D2.NA_CD, D1.DEP_CITY_CD, D1.ARR_CITY_CD ",
											"ORDER BY D4.AREA_CD, D2.NA_CD, D1.DEP_CITY_CD, D1.ARR_CITY_CD"));
		}

		/// <summary>
		/// 할인항공권 리스트(인쇄용)
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="ARC">지역 코드(ASIA:동남아, JPN:일본, CHI:중국/대만/홍콩, EUR:유럽, AMCA:미주, SOPA:남태평양/괌/사이판, AFR:중동/아프리카, CSAM:중남미)</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="SAC">항공사 코드</param>
		/// <param name="EXD">유효기간(최대체류일)</param>
		/// <returns></returns>
		[WebMethod(Description = "할인항공권 리스트(인쇄용)")]
		public XmlElement SearchAirDiscountPrintRS(int SNM, string ARC, string DLC, string ALC, string SAC, string EXD)
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
                        new SqlParameter("@요청5", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 432;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = ARC;
                sqlParam[8].Value = DLC;
                sqlParam[9].Value = ALC;
                sqlParam[10].Value = SAC;
                sqlParam[11].Value = EXD;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                StringBuilder sb = new StringBuilder(8192);
                sb.Append("SELECT D4.AREA_CD, D2.NA_CD, D1.DEP_CITY_CD, D1.ARR_CITY_CD, D1.IATA_AIR_CD, D1.FARE_RULE_NO, D1.TRIP_TYPE, D1.MAX_STAY_DATE, D1.MIN_STAY_DATE, D1.PAX_TYPE, D1.PAX_TYPE AS PAX_TYPE_DESC, D1.NO_OF_STOP, D1.RTG_CITY_NM, D1.RTG_AIR_NM, D1.RTG_FLTNO, D1.RSV_SEAT_GRAD, D1.DEP_WKDAY_IDX, D1.AP_DAYCNT, ");
                sb.Append("D1.ADT_FARE, D1.ADT_FARE AS ADT_DSCNT_FARE, D5.ADT_BAF + NVL(D5.ADT_ADD_BAF, 0) AS ADT_BAF, D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) AS ADT_TAX_AMT, D1.CHD_FARE, D1.CHD_FARE AS CHD_DSCNT_FARE, D1.INF_FARE, ");
                sb.Append("D1.FARE_NO, D1.CABIN_SEAT_GRAD, D1.DEPLOY_FARE_KIND, ");
                sb.Append("LTRIM(");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE1 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE1 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE1 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE1 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE2 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE2 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE2 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE2 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE3 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE3 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE3 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE3 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE4 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE4 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE4 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE4 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE5 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE5 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE5 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE5 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE6 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE6 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE6 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE6 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE7 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE7 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE7 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE7 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE8 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE8 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE8 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE8 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE9 ), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE9 )) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE9 ), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE9 )) || ");
                sb.Append("DECODE(LTRIM(D1.FARE_SEASN_BGN_DATE10), NULL, NULL, '/' || LTRIM(D1.FARE_SEASN_BGN_DATE10)) || DECODE(LTRIM(D1.FARE_SEASN_END_DATE10), NULL, NULL, '-' || LTRIM(D1.FARE_SEASN_END_DATE10)),'/') APPLY_DATE, ");
                sb.Append("LTRIM(");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE1 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE1 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE1 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE1 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE2 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE2 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE2 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE2 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE3 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE3 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE3 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE3 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE4 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE4 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE4 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE4 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE5 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE5 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE5 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE5 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE6 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE6 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE6 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE6 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE7 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE7 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE7 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE7 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE8 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE8 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE8 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE8 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE9 ), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE9 )) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE9 ), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE9 )) || ");
                sb.Append("DECODE(LTRIM(D1.SALE_POSBL_BGN_DATE10), NULL, NULL, '/' || LTRIM(D1.SALE_POSBL_BGN_DATE10)) || DECODE(LTRIM(D1.SALE_POSBL_END_DATE10), NULL, NULL, '-' || LTRIM(D1.SALE_POSBL_END_DATE10)),'/') SALE_DATE ");
                sb.Append("FROM TB_TAI_FB100 D1, TB_COM_CD130 D2, TB_COM_CD120 D3, TB_COM_CD112 D4, TB_TAI_FB200 D5 ");
                sb.Append("WHERE D1.ARR_CITY_CD = D2.CITY_CD AND D2.NA_CD = D3.NA_CD AND D3.AIR_AREA_CD = D4.AREA_CD ");
                sb.Append("AND D1.RTG_CITY_NM = D5.RTG_CITY_NM AND D1.RTG_AIR_NM = D5.RTG_AIR_NM AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.CABIN_SEAT_GRAD AND CASE D1.CABIN_SEAT_GRAD WHEN 'F' THEN 'F' WHEN 'C' THEN 'C' ELSE 'Y' END = D5.RSV_SEAT_GRAD ");
                sb.Append("AND D1.USE_YN = 'Y' AND D1.RSV_AGT_CD = 'SELK138AB' AND D5.ADT_TAX_AMT + NVL(D5.ADT_ADD_TAX_AMT, 0) > 0 ");
                sb.Append("AND TO_CHAR(SYSDATE, 'YYYYMMDD') BETWEEN D1.FARE_BGN_DATE AND D1.FARE_END_DATE ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.FARE_SEASN_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");
                sb.Append("AND (CASE WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE1 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE2 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE3 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE4 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE5 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE6 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE7 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE8 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE9 THEN 'Y' WHEN TO_CHAR(SYSDATE, 'YYYYMMDD') <= D1.SALE_POSBL_END_DATE10 THEN 'Y' ELSE 'N' END = 'Y') ");

                if (!String.IsNullOrWhiteSpace(ARC))
                    sb.Append(String.Format("AND D4.AREA_CD = '{0}' ", ARC));

                if (!String.IsNullOrWhiteSpace(DLC))
                    sb.Append(String.Format("AND CASE WHEN '{0}' = 'SEL' AND D1.DEP_CITY_CD IN ('SEL', 'GMP', 'ICN') THEN 'Y' WHEN '{0}' = 'ICN' AND D1.DEP_CITY_CD IN ('SEL', 'ICN') THEN 'Y' WHEN '{0}' = 'GMP' AND D1.DEP_CITY_CD IN ('SEL', 'GMP') THEN 'Y' WHEN '{0}' = D1.DEP_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", DLC));

                if (!String.IsNullOrWhiteSpace(ALC))
                {
                    if (ALC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.ARR_CITY_CD IN ({0}) ", SplitForSQL(ALC, ",")));
                    else
                        sb.Append(String.Format("AND CASE WHEN '{0}' = 'NRT' AND D1.ARR_CITY_CD IN ('TYO', 'NRT') THEN 'Y' WHEN '{0}' = 'HND' AND D1.ARR_CITY_CD IN ('TYO', 'HND') THEN 'Y' WHEN '{0}' = 'TYO' AND D1.ARR_CITY_CD IN ('TYO', 'HND', 'NRT') THEN 'Y' WHEN '{0}' = D1.ARR_CITY_CD THEN 'Y' WHEN NVL('{0}', '*') = '*' THEN 'Y' ELSE 'N' END = 'Y' ", ALC));
                }

                if (!String.IsNullOrWhiteSpace(SAC))
                {
                    if (SAC.IndexOf(",") != -1)
                        sb.Append(String.Format("AND D1.IATA_AIR_CD IN ({0}) ", SplitForSQL(SAC, ",")));
                    else
                        sb.Append(String.Format("AND D1.IATA_AIR_CD = '{0}' ", SAC));
                }

                if (!String.IsNullOrWhiteSpace(EXD))
                    sb.Append(String.Format("AND D1.MAX_STAY_DATE = '{0}' ", EXD));

                return ApplyPromotion(OracleFare(sb.ToString()), SearchPromotionList(SNM, ARC, "", DLC, ALC, SAC));
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 432, 0, 0).ToErrors;
            }
		}

		#endregion "할인항공 운임"

		#region "할인항공 스케쥴 조회"

		/// <summary>
        /// Availability 조회(할인항공운임 정보 이용)
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="FXL">할인항공운임의 fare XmlNode</param>
		/// <returns></returns>
		[WebMethod(Description = "Availability 조회(할인항공운임 정보 이용)")]
		public XmlElement SearchAvailRS(int SNM, string DTD, string ARD, int ADC, int CHC, int IFC, string FXL)
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
                        new SqlParameter("@요청6", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = 438;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = DTD;
                sqlParam[8].Value = ARD;
                sqlParam[9].Value = ADC;
                sqlParam[10].Value = CHC;
                sqlParam[11].Value = IFC;
                sqlParam[12].Value = FXL;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				string GUID = cm.GetGUID;

				XmlDocument XmlFare = new XmlDocument();
				XmlFare.LoadXml(FXL);
				cm.XmlFileSave(XmlFare, mc.Name, "SearchAvailRQ", "N", String.Concat(GUID, "-01"));

				XmlNode Fare = XmlFare.SelectSingleNode("fare");

				XmlElement Session = amd.SessionCreate(SNM, String.Concat(GUID, "-02"));

				string SID = Session.SelectSingleNode("session/sessionId").InnerText;
				string SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
				string SCT = Session.SelectSingleNode("session/securityToken").InnerText;

				XmlElement ResXml = amd.SellByFareSearchRS(SID, SQN, SCT, String.Concat(GUID, "-03"), DTD, ARD, Fare.SelectSingleNode("FARE_BASIS").InnerText, Fare.SelectSingleNode("IATA_AIR_CD").InnerText, Fare.SelectSingleNode("DEP_CITY_CD").InnerText, Fare.SelectSingleNode("ARR_CITY_CD").InnerText, Fare.SelectSingleNode("TRIP_TYPE").InnerText, cm.RequestInt(Fare.SelectSingleNode("NO_OF_STOP").InnerText), ADC, CHC, IFC, 200);
                cm.XmlFileSave(ResXml, mc.Name, "SellByFareSearchRS", "N", String.Concat(GUID, "-03"));

				amd.SessionClose(SID, SCT, String.Concat(GUID, "-04"));

				//오류 결과일 경우 예외 처리
				XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
				xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_SellByFareSearch"));

				if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
				{
					if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
						throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
					else
						throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
				}

				return ToModeSearchAvailRS(ResXml, xnMgr, XmlFare.DocumentElement, ADC, CHC, IFC);
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 438, 0, 0).ToErrors;
			}
		}

        /// <summary>
        /// Availability 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="ADC">성인 탑승객 수</param>
        /// <param name="CHC">소아 탑승객 수</param>
        /// <param name="IFC">유아 탑승객 수</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <returns></returns>
        [WebMethod(Description = "Availability 조회")]
        public XmlElement SearchAvail2RS(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string FAB, int ADC, int CHC, int IFC, int NRR)
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

                sqlParam[0].Value = 435;
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
                sqlParam[13].Value = FAB;
                sqlParam[14].Value = ADC;
                sqlParam[15].Value = CHC;
                sqlParam[16].Value = IFC;
                sqlParam[17].Value = NRR;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                string GUID = cm.GetGUID;

                XmlElement Session = amd.SessionCreate(SNM, String.Concat(GUID, "-01"));

                string SID = Session.SelectSingleNode("session/sessionId").InnerText;
                string SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
                string SCT = Session.SelectSingleNode("session/securityToken").InnerText;

                XmlElement ResXml = amd.SellByFareSearchRS(SID, SQN, SCT, String.Concat(GUID, "-02"), DTD, ARD, FAB, SAC, DLC, ALC, ROT, (int?)null, ADC, CHC, IFC, NRR);
                cm.XmlFileSave(ResXml, mc.Name, "SellByFareSearchRS", "N", String.Concat(GUID, "-02"));

                amd.SessionClose(SID, SCT, String.Concat(GUID, "-03"));

                //오류 결과일 경우 예외 처리
                XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_SellByFareSearch"));

                if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                {
                    if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                        throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                    else
                        throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                }

                return ToModeSearchAvailRS(ResXml, xnMgr, null, ADC, CHC, IFC);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 435, 0, 0).ToErrors;
            }
        }

		/// <summary>
		/// SearchAvailRS를 통합용 XML구조로 치환
		/// </summary>
		/// <param name="ResXml">SearchAvailRS의 Data</param>
		/// <param name="xnMgr">XmlNamespaceManager</param>
		/// <param name="XmlFare">할인항공운임의 fareXml 정보</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <returns></returns>
		protected XmlElement ToModeSearchAvailRS(XmlElement ResXml, XmlNamespaceManager xnMgr, XmlElement XmlFare, double ADC, double CHC, double IFC)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

			XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

			XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
			XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
			XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
			XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
			XmlNode StopSegment = Segment.SelectSingleNode("seg");

			XmlNode NewFlightIndex;
			XmlNode NewSegmentGroup;
			XmlNode NewSegment;
			XmlNode NewStopSegment;

			string PTC = Common.ChangePaxType1(ResXml.SelectSingleNode("m:recommendation[1]/m:paxFareProduct[1]/m:paxReference[1]/m:ptc", xnMgr).InnerText);
			string CDS = string.Empty;

			FlightInfo.Attributes.GetNamedItem("ptc").InnerText = PTC;

			foreach (XmlNode flightIndex in ResXml.SelectNodes("m:flightIndex", xnMgr))
			{
				NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
				NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = flightIndex.SelectSingleNode("m:requestedSegmentRef/m:segRef", xnMgr).InnerText;

				foreach (XmlNode groupOfFlights in flightIndex.SelectNodes("m:groupOfFlights", xnMgr))
				{
					NewSegmentGroup = NewFlightIndex.AppendChild(SegmentGroup.CloneNode(false));
					NewSegmentGroup.Attributes.GetNamedItem("ref").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[1]/m:ref", xnMgr).InnerText;
					NewSegmentGroup.Attributes.GetNamedItem("eft").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[m:unitQualifier='EFT']/m:ref", xnMgr).InnerText;
					NewSegmentGroup.Attributes.GetNamedItem("mjc").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[m:unitQualifier='MCX']/m:ref", xnMgr).InnerText;
                    NewSegmentGroup.Attributes.GetNamedItem("nosp").InnerText = groupOfFlights.SelectNodes("m:flightDetails", xnMgr).Count.ToString();
                    CDS = "N";

					foreach (XmlNode flightDetails in groupOfFlights.SelectNodes("m:flightDetails", xnMgr))
					{
						NewSegment = NewSegmentGroup.AppendChild(Segment.CloneNode(false));
						NewSegment.Attributes.GetNamedItem("dlc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[1]/m:locationId", xnMgr).InnerText;
						NewSegment.Attributes.GetNamedItem("alc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[2]/m:locationId", xnMgr).InnerText;
						NewSegment.Attributes.GetNamedItem("ddt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText);
						NewSegment.Attributes.GetNamedItem("ardt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText);
						NewSegment.Attributes.GetNamedItem("mcc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:marketingCarrier", xnMgr).InnerText;
						NewSegment.Attributes.GetNamedItem("occ").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).InnerText : "";
						NewSegment.Attributes.GetNamedItem("fln").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:flightOrtrainNumber", xnMgr).Count > 0) ? Common.ZeroPaddingFlight(flightDetails.SelectSingleNode("m:flightInformation/m:flightOrtrainNumber", xnMgr).InnerText) : "";
						NewSegment.Attributes.GetNamedItem("eqt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDetail/m:equipmentType", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:productDetail/m:equipmentType", xnMgr).InnerText : "";
						NewSegment.Attributes.GetNamedItem("stn").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).InnerText : "0";
						NewSegment.Attributes.GetNamedItem("etc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:addProductDetail/m:electronicTicketing", xnMgr).InnerText;

						if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
						{
							XmlNode StopDetailsAA = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AA']", xnMgr);
							XmlNode StopDetailsAD = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AD']", xnMgr);

							NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
							NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
							NewStopSegment.Attributes.GetNamedItem("alc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
							NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
							NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);

							NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
							NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
							NewStopSegment.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
							NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = (StopDetailsAD.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAD.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAD.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAD.SelectSingleNode("m:date", xnMgr).InnerText);
							NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
						}

						if (CDS.Equals("N") && (NewSegment.Attributes.GetNamedItem("mcc").InnerText != NewSegment.Attributes.GetNamedItem("occ").InnerText))
							CDS = "Y";
					}

					NewSegmentGroup.Attributes.GetNamedItem("cds").InnerText = CDS;
				}
			}

			FlightInfo.RemoveChild(FlightIndex);


			XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
			XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");

			SegmentGroup = PriceIndex.SelectSingleNode("segGroup");
			Segment = SegmentGroup.SelectSingleNode("seg");
			XmlNode SegRef = Segment.SelectSingleNode("ref");
			
			XmlNode NewPriceIndex;
			XmlNode NewSegRef;

			foreach (XmlNode recommendation in ResXml.SelectNodes("m:recommendation", xnMgr))
			{
				string ValidatingCarrier = recommendation.SelectSingleNode("m:paxFareProduct/m:paxFareDetail/m:codeShareDetails[m:transportStageQualifier='V']/m:company", xnMgr).InnerText;

				if (Common.AirlineHost("Amadeus", ValidatingCarrier))
				{
					NewPriceIndex = PriceInfo.AppendChild(PriceIndex.CloneNode(true));
					NewPriceIndex.RemoveChild(NewPriceIndex.SelectSingleNode("segGroup"));
					
					//여정 정보
					foreach (XmlNode segmentFlightRef in recommendation.SelectNodes("m:segmentFlightRef", xnMgr))
					{
						NewSegment = Segment.CloneNode(false);

						foreach (XmlNode refNumber in segmentFlightRef.SelectNodes("m:referencingDetail[m:refQualifier='S']/m:refNumber", xnMgr))
						{
							NewSegRef = NewSegment.AppendChild(SegRef.CloneNode(false));
							NewSegRef.InnerText = refNumber.InnerText;
						}

						NewPriceIndex.InsertBefore(NewSegment, NewPriceIndex.SelectSingleNode("paxFareGroup"));
					}

					//요약 정보
					NewPriceIndex.Attributes.GetNamedItem("ptc").InnerText = PTC;
					NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = recommendation.SelectSingleNode("m:itemNumber/m:itemNumberId/m:number", xnMgr).InnerText;
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[1]/m:amount", xnMgr).InnerText;
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText = cm.GetFare(ValidatingCarrier, recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[1]/m:amount", xnMgr).InnerText, recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText, "0").ToString();
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText;
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tax").InnerText = recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText;
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fsc").InnerText = "0";
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tasf").InnerText = "0";
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("pvc").InnerText = ValidatingCarrier;
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("mas").InnerText = "";
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ttl").InnerText = cm.ConvertToDateTime(recommendation.SelectSingleNode("m:paxFareProduct/m:fare/m:pricingMessage[m:freeTextQualification/m:textSubjectQualifier='LTD']/m:description[2]", xnMgr).InnerText);
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ucf").InnerText = (recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='UF']", xnMgr).Count > 0) ? "Y" : "N";
					NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ntf").InnerText = (recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='NTF']", xnMgr).Count > 0) ? "Y" : "N";

					//탑승객별 요금정보
					XmlNode PaxFareGroup = NewPriceIndex.SelectSingleNode("paxFareGroup");
					XmlNode PaxFare = PaxFareGroup.SelectSingleNode("paxFare");
					XmlNode SegmentFareGroup;
					XmlNode SegmentFare;
					XmlNode Traveler;
					XmlNode Fare;
					XmlNode FareType;
					XmlNode Ref;

					XmlNode NewPaxFare;
					XmlNode NewSegmentFare;
					XmlNode NewFare;
					XmlNode NewFareType;
					XmlNode NewRef;

					foreach (XmlNode paxFareProduct in recommendation.SelectNodes("m:paxFareProduct", xnMgr))
					{
						NewPaxFare = PaxFareGroup.AppendChild(PaxFare.CloneNode(true));
						NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = Common.ChangePaxType1(paxFareProduct.SelectSingleNode("m:paxReference/m:ptc", xnMgr).InnerText);
						NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = cm.GetFare(ValidatingCarrier, paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalFareAmount", xnMgr).InnerText, paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText, "0").ToString();
						NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText;
						NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText;
						NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = "0";

						//여정별 요금정보
						SegmentFareGroup = NewPaxFare.SelectSingleNode("segFareGroup");
						SegmentFare = SegmentFareGroup.SelectSingleNode("segFare");

						foreach (XmlNode fareDetails in paxFareProduct.SelectNodes("m:fareDetails", xnMgr))
						{
							NewSegmentFare = SegmentFareGroup.AppendChild(SegmentFare.CloneNode(true));
							NewSegmentFare.Attributes.GetNamedItem("ref").InnerText = fareDetails.SelectSingleNode("m:segmentRef/m:segRef", xnMgr).InnerText;

							Fare = NewSegmentFare.SelectSingleNode("fare");

							foreach (XmlNode groupOfFares in fareDetails.SelectNodes("m:groupOfFares", xnMgr))
							{
								NewFare = NewSegmentFare.AppendChild(Fare.CloneNode(true));
								NewFare.Attributes.GetNamedItem("bpt").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:breakPoint", xnMgr).InnerText;
								NewFare.Attributes.GetNamedItem("mas").InnerText = "";
								NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:rbd", xnMgr).InnerText;
								NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("cabin").InnerText = (groupOfFares.SelectNodes("m:productInformation/m:cabinProduct/m:cabin", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:cabin", xnMgr).InnerText : "";
								NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = (groupOfFares.SelectNodes("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).Count > 0) ? ((groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).InnerText.Equals("L")) ? "0" : groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).InnerText) : "";
								NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:fareBasis", xnMgr).InnerText;
								NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("ptc").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:passengerType", xnMgr).InnerText;
								NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("tkd").InnerText = (groupOfFares.SelectNodes("m:ticketInfos", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).InnerText : "";

								FareType = NewFare.SelectSingleNode("fare/fareType");

								foreach (XmlNode TmpFareType in groupOfFares.SelectNodes("m:productInformation/m:fareProductDetail/m:fareType", xnMgr))
								{
									NewFareType = NewFare.SelectSingleNode("fare").AppendChild(FareType.Clone());
									NewFareType.InnerText = TmpFareType.InnerText;
								}

								NewFare.SelectSingleNode("fare").RemoveChild(FareType);

                                if (groupOfFares.SelectNodes("m:productInformation/m:corporateId", xnMgr).Count > 0)
                                    NewFare.SelectSingleNode("corporateId").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:corporateId", xnMgr).InnerText;
                                else
                                    NewFare.RemoveChild(NewFare.SelectSingleNode("corporateId"));
							}

							NewSegmentFare.RemoveChild(Fare);
						}

						SegmentFareGroup.RemoveChild(SegmentFare);

						//탑승객 번호
						Traveler = NewPaxFare.SelectSingleNode("traveler");
						Ref = Traveler.SelectSingleNode("ref");

						foreach (XmlNode traveller in paxFareProduct.SelectNodes("m:paxReference/m:traveller", xnMgr))
						{
							NewRef = Traveler.AppendChild(Ref.CloneNode(false));
							NewRef.InnerText = traveller.SelectSingleNode("m:ref", xnMgr).InnerText;

							if (traveller.SelectNodes("m:infantIndicator", xnMgr).Count > 0)
							{
								NewRef.Attributes.GetNamedItem("ind").InnerText = traveller.SelectSingleNode("m:infantIndicator", xnMgr).InnerText;
							}
							else
							{
								NewRef.Attributes.RemoveNamedItem("ind");
							}
						}

						Traveler.RemoveChild(Ref);
					}

					PaxFareGroup.RemoveChild(PaxFare);

					//free Text
					foreach (XmlNode fare in recommendation.SelectNodes("m:paxFareProduct[1]/m:fare", xnMgr))
					{
						XmlDocument TmpXml = new XmlDocument();
						TmpXml.LoadXml(fare.OuterXml.Replace(String.Format(" xmlns=\"{0}\"", AmadeusConfig.NamespaceURL("Fare_SellByFareSearch")), ""));

						NewPriceIndex.SelectSingleNode("fareMessage").AppendChild(NewPriceIndex.OwnerDocument.ImportNode(TmpXml.SelectSingleNode("fare"), true));
					}

					if (!NewPriceIndex.SelectSingleNode("fareMessage").HasChildNodes)
						NewPriceIndex.RemoveChild(NewPriceIndex.SelectSingleNode("fareMessage"));
				}
			}

			PriceInfo.RemoveChild(PriceIndex);

			return XmlDoc.DocumentElement;
		}

		#endregion "할인항공 스케쥴 조회"

		#region "할인항공 스케쥴 조회(달력)(할인항공 운임정보 이용)"

		/// <summary>
        /// 월단위 Availability 조회(할인항공 운임정보 이용)
		/// </summary>
		/// <param name="SNM">사이트번호</param>
		/// <param name="DTD">출발일(YYYYMMDD)</param>
		/// <param name="ARD">도착일(YYYYMMDD)</param>
		/// <param name="ADC">성인 탑승객 수</param>
		/// <param name="CHC">소아 탑승객 수</param>
		/// <param name="IFC">유아 탑승객 수</param>
		/// <param name="FXL">할인항공운임의 fare XmlNode</param>
        [WebMethod(Description = "월단위 Availability 조회(할인항공 운임정보 이용)")]
		public XmlElement SearchAvailCalendarRS(int SNM, string DTD, string ARD, int ADC, int CHC, int IFC, string FXL)
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
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = 437;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = DTD;
                sqlParam[8].Value = ARD;
                sqlParam[9].Value = ADC;
                sqlParam[10].Value = CHC;
                sqlParam[11].Value = IFC;
                sqlParam[12].Value = FXL;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
			{
				string GUID = cm.GetGUID;

				XmlDocument XmlFare = new XmlDocument();
				XmlFare.LoadXml(FXL);
                cm.XmlFileSave(XmlFare, mc.Name, "SearchAvailCalendarRQ", "N", String.Concat(GUID, "-01"));

				XmlNode Fare = XmlFare.SelectSingleNode("fare");

				XmlElement Session = amd.SessionCreate(SNM, String.Concat(GUID, "-02"));

				string SID = Session.SelectSingleNode("session/sessionId").InnerText;
				string SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
				string SCT = Session.SelectSingleNode("session/securityToken").InnerText;

				//출도착일 검증
				int NowDate = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));

				if (NowDate > Convert.ToInt32(DTD))
					DTD = NowDate.ToString();

				if (NowDate > Convert.ToInt32(ARD))
					ARD = NowDate.ToString();

				XmlElement ResXml = amd.SellByFareCalendarRS(SID, SQN, SCT, String.Concat(GUID, "-03"), DTD, ARD, Fare.SelectSingleNode("FARE_BASIS").InnerText, Fare.SelectSingleNode("IATA_AIR_CD").InnerText, Fare.SelectSingleNode("DEP_CITY_CD").InnerText, Fare.SelectSingleNode("ARR_CITY_CD").InnerText, Fare.SelectSingleNode("TRIP_TYPE").InnerText, cm.RequestInt(Fare.SelectSingleNode("NO_OF_STOP").InnerText), ADC, CHC, IFC, 200);
                cm.XmlFileSave(ResXml, mc.Name, "SearchAvailCalendarRS", "N", String.Concat(GUID, "-03"));

				amd.SessionClose(SID, SCT, String.Concat(GUID, "-04"));

				//오류 결과일 경우 예외 처리
				XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
				xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_SellByFareCalendar"));

				if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
				{
					if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
						throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
					else
						throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
				}

				XmlElement XmlMode = ToModeSearchAvailCalendarRS(ResXml, xnMgr, XmlFare.DocumentElement, ADC, CHC);
                cm.XmlFileSave(XmlMode, mc.Name, "ToModeSearchAvailCalendarRS", "N", String.Concat(GUID, "-05"));

				return XmlMode;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 437, 0, 0).ToErrors;
			}
		}

		/// <summary>
		/// SearchAvailCalendarRS를 통합용 XML구조로 치환
		/// </summary>
		/// <param name="ResXml">SearchAvailCalendarRS의 Data</param>
		/// <param name="xnMgr">XmlNamespaceManager</param>
		/// <param name="XmlFare">할인항공운임의 fareXml 정보</param>
		/// <returns></returns>
		protected XmlElement ToModeSearchAvailCalendarRS(XmlElement ResXml, XmlNamespaceManager xnMgr, XmlElement XmlFare, int ADC, int CHC)
		{
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.Load(mc.XmlFullPath("SearchAvailCalendarRS"));

			XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

			XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
			XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
			XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
			XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
			XmlNode StopSegment = Segment.SelectSingleNode("seg");

			XmlNode NewFlightIndex;
			XmlNode NewSegmentGroup;
			XmlNode NewSegment;
			XmlNode NewStopSegment;

			string RtgCity = XmlFare.SelectSingleNode("RTG_CITY_NM").InnerText;
			string RtgAir = XmlFare.SelectSingleNode("RTG_AIR_NM").InnerText;
			string RtgSeat = XmlFare.SelectSingleNode("RTG_SEAT_GRAD").InnerText;

			string[] TmpRTG = RtgCity.Split('~');
			string[] RTG = (TmpRTG.Length.Equals(2) ? String.Format("{0}-{1}", TmpRTG[0], TmpRTG[1]) : (TmpRTG.Length.Equals(3) ? String.Format("{0}-{1}/{1}-{2}", TmpRTG[0], TmpRTG[1], TmpRTG[2]) : "")).Split('/');
			string[] CRR = RtgAir.Split('-');
			string[] CLS = RtgSeat.Split('-');
			int n = 0;
			int m = 0;
			bool SameInfo = true;

			foreach (XmlNode flightIndex in ResXml.SelectNodes("m:flightIndex", xnMgr))
			{
				NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
				NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = flightIndex.SelectSingleNode("m:requestedSegmentRef/m:segRef", xnMgr).InnerText;

				foreach (XmlNode groupOfFlights in flightIndex.SelectNodes("m:groupOfFlights", xnMgr))
				{
					string[] RTG2 = RTG[n].Split('-');

					//경유수 체크
					if (groupOfFlights.SelectNodes("m:flightDetails", xnMgr).Count.Equals(RTG2.Length - 1))
					{
						m = 0;
						SameInfo = true;
					
						NewSegmentGroup = NewFlightIndex.AppendChild(SegmentGroup.CloneNode(false));
						NewSegmentGroup.Attributes.GetNamedItem("ref").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[1]/m:ref", xnMgr).InnerText;

						foreach (XmlNode flightDetails in groupOfFlights.SelectNodes("m:flightDetails", xnMgr))
						{
							//도시(공항) 체크
							if (Common.CityToAirport(RTG2[m]).IndexOf(flightDetails.SelectSingleNode("m:flightInformation/m:location[1]/m:locationId", xnMgr).InnerText).Equals(-1) || Common.CityToAirport(RTG2[(m + 1)]).IndexOf(flightDetails.SelectSingleNode("m:flightInformation/m:location[2]/m:locationId", xnMgr).InnerText).Equals(-1))
							{
								SameInfo = false;
								break;
							}

							//항공 체크
							if (CRR[(n + m)] != flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:marketingCarrier", xnMgr).InnerText)
							{
								SameInfo = false;
								break;
							}

							NewSegment = NewSegmentGroup.AppendChild(Segment.CloneNode(false));
							NewSegment.Attributes.GetNamedItem("dlc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[1]/m:locationId", xnMgr).InnerText;
							NewSegment.Attributes.GetNamedItem("alc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[2]/m:locationId", xnMgr).InnerText;
							NewSegment.Attributes.GetNamedItem("ddt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText);
							NewSegment.Attributes.GetNamedItem("ardt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText);
							NewSegment.Attributes.GetNamedItem("mcc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:marketingCarrier", xnMgr).InnerText;
							NewSegment.Attributes.GetNamedItem("occ").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).InnerText : "";
							NewSegment.Attributes.GetNamedItem("fln").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:flightOrtrainNumber", xnMgr).Count > 0) ? Common.ZeroPaddingFlight(flightDetails.SelectSingleNode("m:flightInformation/m:flightOrtrainNumber", xnMgr).InnerText) : "";
							NewSegment.Attributes.GetNamedItem("stn").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).InnerText : "0";

							if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
							{
								XmlNode StopDetailsAA = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AA']", xnMgr);
								XmlNode StopDetailsAD = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AD']", xnMgr);

								NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
								NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
								NewStopSegment.Attributes.GetNamedItem("alc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
								NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
								NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);

								NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
								NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
								NewStopSegment.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
								NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);
								NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
							}

							m++;
						}

						if (!SameInfo)
						{
							NewFlightIndex.RemoveChild(NewSegmentGroup);
							break;
						}
					}
				}

				n++;
			}

			FlightInfo.RemoveChild(FlightIndex);

			XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
			XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");
			XmlNode PaxFareGroup;
			XmlNode PaxFare;
			XmlNode SegmentFareGroup;
			XmlNode SegmentFare;
			XmlNode Traveler;
			XmlNode Fare;
			XmlNode FareType;
			XmlNode FareFamily;
			XmlNode FamilyNumber;
			XmlNode Ref;

			XmlNode NewPriceIndex;
			XmlNode NewPaxFare;
			XmlNode NewSegmentFare;
			XmlNode NewFare;
			XmlNode NewFareType;
			XmlNode NewFamilyNumber;
			XmlNode NewRef;

			double AdultFare = cm.RequestDouble(XmlFare.SelectSingleNode("ADT_FARE").InnerText);
            double AdultDisFare = cm.RequestDouble(XmlFare.SelectSingleNode("ADT_DSCNT_FARE").InnerText);
            double AdultTax = cm.RequestDouble(XmlFare.SelectSingleNode("ADT_TAX_AMT").InnerText);
            double AdultFsc = cm.RequestDouble(XmlFare.SelectSingleNode("ADT_BAF").InnerText);
            double ChildFare = cm.RequestDouble(XmlFare.SelectSingleNode("CHD_FARE").InnerText);
            double ChildDisFare = cm.RequestDouble(XmlFare.SelectSingleNode("CHD_DSCNT_FARE").InnerText);
            double InfantFare = cm.RequestDouble(XmlFare.SelectSingleNode("INF_FARE").InnerText);
			double SumAirFare = 0;
            double SumAirDisFare = 0;
			double SumAirTax = 0;
			double SumAirFsc = 0;

			//TASF(발권 여행사 수수료)
			double TASF = 0;

			foreach (XmlNode Recommendation in ResXml.SelectNodes("m:recommendation", xnMgr))
			{
                //AirPrice = cm.RequestDouble(Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[1]/m:amount", xnMgr).InnerText);
                //AirTax = cm.RequestDouble(Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText);
                //AirFare = AirPrice - AirTax;
				
				NewPriceIndex = PriceInfo.AppendChild(PriceIndex.CloneNode(true));
				NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = Recommendation.SelectSingleNode("m:itemNumber/m:itemNumberId/m:number", xnMgr).InnerText;
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = AirPrice.ToString();
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText = AirFare.ToString();
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = AirFare.ToString();
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tax").InnerText = AirTax.ToString();
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fsc").InnerText = AirFsc.ToString();
                //NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tasf").InnerText = TASF.ToString();
				NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("pvc").InnerText = Recommendation.SelectSingleNode("m:paxFareProduct/m:paxFareDetail/m:codeShareDetails/m:company", xnMgr).InnerText;
				NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ucf").InnerText = (Recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && Recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='UF'", xnMgr).Count > 0) ? "Y" : "N";
				NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ntf").InnerText = (Recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && Recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='NTF'", xnMgr).Count > 0) ? "Y" : "N";

				SegmentGroup = NewPriceIndex.SelectSingleNode("segGroup");
				Segment = SegmentGroup.SelectSingleNode("seg");

				foreach (XmlNode segmentFlightRef in Recommendation.SelectNodes("m:segmentFlightRef", xnMgr))
				{
					NewSegment = SegmentGroup.AppendChild(Segment.CloneNode(true));

					Ref = NewSegment.SelectSingleNode("ref");

					foreach (XmlNode refNumber in segmentFlightRef.SelectNodes("m:referencingDetail[m:refQualifier='S']/m:refNumber", xnMgr))
					{
						NewRef = NewSegment.AppendChild(Ref.CloneNode(false));
						NewRef.InnerText = refNumber.InnerText;
					}

					NewSegment.RemoveChild(Ref);
				}

				SegmentGroup.RemoveChild(Segment);


				PaxFareGroup = NewPriceIndex.SelectSingleNode("paxFareGroup");
				PaxFare = PaxFareGroup.SelectSingleNode("paxFare");

				n = 0;
				SameInfo = true;

				foreach (XmlNode paxFareProduct in Recommendation.SelectNodes("m:paxFareProduct", xnMgr))
				{
					double TmpAirPrice = cm.RequestDouble(paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalFareAmount", xnMgr).InnerText);
                    double TmpAirTax = cm.RequestDouble(paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText);
                    double TmpAirFare = TmpAirPrice - TmpAirTax;


					NewPaxFare = PaxFareGroup.AppendChild(PaxFare.CloneNode(true));
					NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = Common.ChangePaxType1(paxFareProduct.SelectSingleNode("m:paxReference/m:ptc", xnMgr).InnerText);

                    if (NewPaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("INF"))
                    {
                        //할인항공요금과 비교
                        if (InfantFare != TmpAirFare)
                        {
                            SameInfo = false;
                            break;
                        }
                        
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = InfantFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = InfantFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = "";
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = "";

                        SumAirFare += InfantFare;
                        SumAirDisFare += InfantFare;
                    }
                    else if (NewPaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("CHD"))
                    {
                        //할인항공요금과 비교
                        if (ChildFare != TmpAirFare)
                        {
                            SameInfo = false;
                            break;
                        }

                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = ChildFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = ChildDisFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = "";
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = "";

                        SumAirFare += ChildFare;
                        SumAirDisFare += ChildDisFare;
                    }
                    else
                    {
                        //할인항공요금과 비교
                        if (AdultFare != TmpAirFare)
                        {
                            SameInfo = false;
                            break;
                        }

                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = AdultFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = AdultDisFare.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = AdultTax.ToString();
                        NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = AdultFsc.ToString();

                        SumAirFare += AdultFare;
                        SumAirDisFare += AdultDisFare;
                        SumAirTax += AdultTax;
                        SumAirFsc += AdultFsc;
                    }

					//여정별 요금정보
					SegmentFareGroup = NewPaxFare.SelectSingleNode("segFareGroup");
					SegmentFare = SegmentFareGroup.SelectSingleNode("segFare");

					foreach (XmlNode fareDetails in paxFareProduct.SelectNodes("m:fareDetails", xnMgr))
					{
						NewSegmentFare = SegmentFareGroup.AppendChild(SegmentFare.CloneNode(true));
						NewSegmentFare.Attributes.GetNamedItem("ref").InnerText = fareDetails.SelectSingleNode("m:segmentRef/m:segRef", xnMgr).InnerText;

						Fare = NewSegmentFare.SelectSingleNode("fare");
						FareFamily = NewSegmentFare.SelectSingleNode("family");
						FamilyNumber = FareFamily.SelectSingleNode("num");

						m = 0;

						foreach (XmlNode groupOfFares in fareDetails.SelectNodes("m:groupOfFares", xnMgr))
						{
							//부킹클래스 체크
							if (CLS[(n + m)] != groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:rbd", xnMgr).InnerText)
							{
								SameInfo = false;
								break;
							}
							
							NewFare = NewSegmentFare.InsertBefore(Fare.CloneNode(true), FareFamily);
							NewFare.Attributes.GetNamedItem("bpt").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:breakPoint", xnMgr).InnerText;
							NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:rbd", xnMgr).InnerText;
							NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("cabin").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:cabin", xnMgr).InnerText;
							NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = (ADC + CHC).ToString();
							NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:fareBasis", xnMgr).InnerText;
							NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("ptc").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:passengerType", xnMgr).InnerText;
							NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("tkd").InnerText = (groupOfFares.SelectNodes("m:ticketInfos", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).InnerText : "";

							FareType = NewFare.SelectSingleNode("fare/fareType");

							foreach (XmlNode TmpFareType in groupOfFares.SelectNodes("m:productInformation/m:fareProductDetail/m:fareType", xnMgr))
							{
								NewFareType = NewFare.SelectSingleNode("fare").AppendChild(FareType.Clone());
								NewFareType.InnerText = TmpFareType.InnerText;
							}

							NewFare.SelectSingleNode("fare").RemoveChild(FareType);

							foreach (XmlNode referencingDetail in groupOfFares.SelectNodes("m:fareFamiliesRef/m:referencingDetail[m:refQualifier='F']", xnMgr))
							{
								NewFamilyNumber = FareFamily.AppendChild(FamilyNumber.CloneNode(false));
								NewFamilyNumber.InnerText = referencingDetail.SelectSingleNode("m:refNumber", xnMgr).InnerText;
							}

							m++;
						}

						if (!SameInfo)
							break;

						FareFamily.RemoveChild(FamilyNumber);
						NewSegmentFare.RemoveChild(Fare);

						n++;
					}

					if (!SameInfo)
						break;

					SegmentFareGroup.RemoveChild(SegmentFare);

					//탑승객 번호
					Traveler = NewPaxFare.SelectSingleNode("traveler");
					Ref = Traveler.SelectSingleNode("ref");

					foreach (XmlNode traveller in paxFareProduct.SelectNodes("m:paxReference/m:traveller", xnMgr))
					{
						NewRef = Traveler.AppendChild(Ref.CloneNode(false));
						NewRef.InnerText = traveller.SelectSingleNode("m:ref", xnMgr).InnerText;

						if (traveller.SelectNodes("m:infantIndicator", xnMgr).Count > 0)
						{
							NewRef.Attributes.GetNamedItem("ind").InnerText = traveller.SelectSingleNode("m:infantIndicator", xnMgr).InnerText;
						}
						else
						{
							NewRef.Attributes.RemoveNamedItem("ind");
						}
					}

					Traveler.RemoveChild(Ref);
				}

				if (!SameInfo)
				{
					PriceInfo.RemoveChild(NewPriceIndex);
					break;
				}
                else
                {
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = (SumAirDisFare + SumAirTax + SumAirFsc + TASF).ToString();
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText = SumAirFare.ToString();
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = SumAirDisFare.ToString();
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tax").InnerText = SumAirTax.ToString();
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fsc").InnerText = SumAirFsc.ToString();
                    NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tasf").InnerText = TASF.ToString();
                }

				PaxFareGroup.RemoveChild(PaxFare);

				//free Text
				foreach (XmlNode fare in Recommendation.SelectNodes("m:paxFareProduct[1]/m:fare", xnMgr))
				{
					XmlDocument TmpXml = new XmlDocument();
					TmpXml.LoadXml(fare.OuterXml.Replace(String.Format(" xmlns=\"{0}\"", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch")), ""));

					NewPriceIndex.SelectSingleNode("fareMessage").AppendChild(NewPriceIndex.OwnerDocument.ImportNode(TmpXml.SelectSingleNode("fare"), true));
				}

				if (!NewPriceIndex.SelectSingleNode("fareMessage").HasChildNodes)
					NewPriceIndex.RemoveChild(NewPriceIndex.SelectSingleNode("fareMessage"));

                if (XmlFare.SelectNodes("promotionInfo").Count > 0)
                    NewPriceIndex.ReplaceChild(XmlDoc.ImportNode(XmlFare.SelectSingleNode("promotionInfo"), true), NewPriceIndex.SelectSingleNode("promotionInfo"));
			}

			PriceInfo.RemoveChild(PriceIndex);

			return XmlDoc.DocumentElement;
		}

        #endregion "할인항공 스케쥴 조회(달력)(할인항공 운임정보 이용)"

        #region "할인항공 스케쥴 조회(달력)"

        /// <summary>
        /// 월단위 Availability 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="ADC">성인 탑승객 수</param>
        /// <param name="CHC">소아 탑승객 수</param>
        /// <param name="IFC">유아 탑승객 수</param>
        [WebMethod(Description = "월단위 Availability 조회")]
        public XmlElement SearchAvailCalendar2RS(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string CCD, string FAB, int ADC, int CHC, int IFC)
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

                sqlParam[0].Value = 436;
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
                sqlParam[13].Value = CCD;
                sqlParam[14].Value = FAB;
                sqlParam[15].Value = ADC;
                sqlParam[16].Value = CHC;
                sqlParam[17].Value = IFC;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                string GUID = cm.GetGUID;

                XmlElement Session = amd.SessionCreate(SNM, String.Concat(GUID, "-01"));

                string SID = Session.SelectSingleNode("session/sessionId").InnerText;
                string SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
                string SCT = Session.SelectSingleNode("session/securityToken").InnerText;

                //출도착일 검증
                int NowDate = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));

                if (NowDate > Convert.ToInt32(DTD))
                    DTD = NowDate.ToString();

                if (NowDate > Convert.ToInt32(ARD))
                    ARD = NowDate.ToString();

                XmlElement ResXml = amd.SellByFareCalendarRS(SID, SQN, SCT, String.Concat(GUID, "-02"), DTD, ARD, FAB, SAC, DLC, ALC, ROT, (int?)null, ADC, CHC, IFC, 200);
                cm.XmlFileSave(ResXml, mc.Name, "SearchAvailCalendar2RS", "N", String.Concat(GUID, "-02"));

                amd.SessionClose(SID, SCT, String.Concat(GUID, "-03"));

                //오류 결과일 경우 예외 처리
                XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_SellByFareCalendar"));

                if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                {
                    if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                        throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                    else
                        throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                }

                XmlElement XmlMode = ToModeSearchAvailCalendar2RS(ResXml, xnMgr, ADC, CHC);
                cm.XmlFileSave(XmlMode, mc.Name, "ToModeSearchAvailCalendar2RS", "N", String.Concat(GUID, "-04"));

                return XmlMode;
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 436, 0, 0).ToErrors;
            }
        }

        /// <summary>
        /// SearchAvailCalendarRS를 통합용 XML구조로 치환
        /// </summary>
        /// <param name="ResXml">SearchAvailCalendarRS의 Data</param>
        /// <param name="xnMgr">XmlNamespaceManager</param>
        /// <param name="XmlFare">할인항공운임의 fareXml 정보</param>
        /// <returns></returns>
        protected XmlElement ToModeSearchAvailCalendar2RS(XmlElement ResXml, XmlNamespaceManager xnMgr, int ADC, int CHC)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("SearchAvailCalendarRS"));

            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;

            XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
            XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
            XmlNode SegmentGroup = FlightIndex.SelectSingleNode("segGroup");
            XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
            XmlNode StopSegment = Segment.SelectSingleNode("seg");

            XmlNode NewFlightIndex;
            XmlNode NewSegmentGroup;
            XmlNode NewSegment;
            XmlNode NewStopSegment;

            foreach (XmlNode flightIndex in ResXml.SelectNodes("m:flightIndex", xnMgr))
            {
                NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = flightIndex.SelectSingleNode("m:requestedSegmentRef/m:segRef", xnMgr).InnerText;

                foreach (XmlNode groupOfFlights in flightIndex.SelectNodes("m:groupOfFlights", xnMgr))
                {
                    NewSegmentGroup = NewFlightIndex.AppendChild(SegmentGroup.CloneNode(false));
                    NewSegmentGroup.Attributes.GetNamedItem("ref").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[1]/m:ref", xnMgr).InnerText;

                    foreach (XmlNode flightDetails in groupOfFlights.SelectNodes("m:flightDetails", xnMgr))
                    {
                        NewSegment = NewSegmentGroup.AppendChild(Segment.CloneNode(false));
                        NewSegment.Attributes.GetNamedItem("dlc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[1]/m:locationId", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("alc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:location[2]/m:locationId", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("ddt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfDeparture", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfDeparture", xnMgr).InnerText);
                        NewSegment.Attributes.GetNamedItem("ardt").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).Count > 0) ? cm.ConvertToDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText, flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:timeOfArrival", xnMgr).InnerText) : cm.RequestDateTime(flightDetails.SelectSingleNode("m:flightInformation/m:productDateTime/m:dateOfArrival", xnMgr).InnerText);
                        NewSegment.Attributes.GetNamedItem("mcc").InnerText = flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:marketingCarrier", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("occ").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:companyId/m:operatingCarrier", xnMgr).InnerText : "";
                        NewSegment.Attributes.GetNamedItem("fln").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:flightOrtrainNumber", xnMgr).Count > 0) ? Common.ZeroPaddingFlight(flightDetails.SelectSingleNode("m:flightInformation/m:flightOrtrainNumber", xnMgr).InnerText) : "";
                        NewSegment.Attributes.GetNamedItem("stn").InnerText = (flightDetails.SelectNodes("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).Count > 0) ? flightDetails.SelectSingleNode("m:flightInformation/m:productDetail/m:techStopNumber", xnMgr).InnerText : "0";

                        if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                        {
                            XmlNode StopDetailsAA = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AA']", xnMgr);
                            XmlNode StopDetailsAD = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AD']", xnMgr);

                            NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                            NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
                            NewStopSegment.Attributes.GetNamedItem("alc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
                            NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
                            NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);

                            NewStopSegment = NewSegment.AppendChild(StopSegment.CloneNode(false));
                            NewStopSegment.Attributes.GetNamedItem("dlc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
                            NewStopSegment.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
                            NewStopSegment.Attributes.GetNamedItem("ddt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);
                            NewStopSegment.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
                        }
                    }
                }
            }

            FlightInfo.RemoveChild(FlightIndex);

            XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
            XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");
            XmlNode PaxFareGroup;
            XmlNode PaxFare;
            XmlNode SegmentFareGroup;
            XmlNode SegmentFare;
            XmlNode Traveler;
            XmlNode Fare;
            XmlNode FareType;
            XmlNode FareFamily;
            XmlNode FamilyNumber;
            XmlNode Ref;

            XmlNode NewPriceIndex;
            XmlNode NewPaxFare;
            XmlNode NewSegmentFare;
            XmlNode NewFare;
            XmlNode NewFareType;
            XmlNode NewFamilyNumber;
            XmlNode NewRef;

            double AirPrice = 0;
            double AirFare = 0;
            double AirTax = 0;
            double AirFsc = 0;

            //TASF(발권 여행사 수수료)
            double TASF = 0;

            foreach (XmlNode Recommendation in ResXml.SelectNodes("m:recommendation", xnMgr))
            {
                AirPrice = cm.RequestDouble(Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[1]/m:amount", xnMgr).InnerText);
                AirTax = cm.RequestDouble(Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText);
                AirFare = AirPrice - AirTax;

                NewPriceIndex = PriceInfo.AppendChild(PriceIndex.CloneNode(true));
                NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = Recommendation.SelectSingleNode("m:itemNumber/m:itemNumberId/m:number", xnMgr).InnerText;
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("price").InnerText = AirPrice.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText = AirFare.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("disFare").InnerText = AirFare.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tax").InnerText = AirTax.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("fsc").InnerText = AirFsc.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("tasf").InnerText = TASF.ToString();
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("pvc").InnerText = Recommendation.SelectSingleNode("m:paxFareProduct/m:paxFareDetail/m:codeShareDetails/m:company", xnMgr).InnerText;
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ucf").InnerText = (Recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && Recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='UF'", xnMgr).Count > 0) ? "Y" : "N";
                NewPriceIndex.SelectSingleNode("summary").Attributes.GetNamedItem("ntf").InnerText = (Recommendation.SelectNodes("m:itemNumber/m:priceTicketing", xnMgr).Count > 0 && Recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='NTF'", xnMgr).Count > 0) ? "Y" : "N";

                SegmentGroup = NewPriceIndex.SelectSingleNode("segGroup");
                Segment = SegmentGroup.SelectSingleNode("seg");

                foreach (XmlNode segmentFlightRef in Recommendation.SelectNodes("m:segmentFlightRef", xnMgr))
                {
                    NewSegment = SegmentGroup.AppendChild(Segment.CloneNode(true));

                    Ref = NewSegment.SelectSingleNode("ref");

                    foreach (XmlNode refNumber in segmentFlightRef.SelectNodes("m:referencingDetail[m:refQualifier='S']/m:refNumber", xnMgr))
                    {
                        NewRef = NewSegment.AppendChild(Ref.CloneNode(false));
                        NewRef.InnerText = refNumber.InnerText;
                    }

                    NewSegment.RemoveChild(Ref);
                }

                SegmentGroup.RemoveChild(Segment);


                PaxFareGroup = NewPriceIndex.SelectSingleNode("paxFareGroup");
                PaxFare = PaxFareGroup.SelectSingleNode("paxFare");

                foreach (XmlNode paxFareProduct in Recommendation.SelectNodes("m:paxFareProduct", xnMgr))
                {
                    AirPrice = cm.RequestDouble(paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalFareAmount", xnMgr).InnerText);
                    AirTax = cm.RequestDouble(paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText);
                    AirFare = AirPrice - AirTax;

                    NewPaxFare = PaxFareGroup.AppendChild(PaxFare.CloneNode(true));
                    NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = Common.ChangePaxType1(paxFareProduct.SelectSingleNode("m:paxReference/m:ptc", xnMgr).InnerText);
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = AirFare.ToString();
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = AirFare.ToString();
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = AirTax.ToString();
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = "0";

                    //여정별 요금정보
                    SegmentFareGroup = NewPaxFare.SelectSingleNode("segFareGroup");
                    SegmentFare = SegmentFareGroup.SelectSingleNode("segFare");

                    foreach (XmlNode fareDetails in paxFareProduct.SelectNodes("m:fareDetails", xnMgr))
                    {
                        NewSegmentFare = SegmentFareGroup.AppendChild(SegmentFare.CloneNode(true));
                        NewSegmentFare.Attributes.GetNamedItem("ref").InnerText = fareDetails.SelectSingleNode("m:segmentRef/m:segRef", xnMgr).InnerText;

                        Fare = NewSegmentFare.SelectSingleNode("fare");
                        FareFamily = NewSegmentFare.SelectSingleNode("family");
                        FamilyNumber = FareFamily.SelectSingleNode("num");

                        foreach (XmlNode groupOfFares in fareDetails.SelectNodes("m:groupOfFares", xnMgr))
                        {
                            NewFare = NewSegmentFare.InsertBefore(Fare.CloneNode(true), FareFamily);
                            NewFare.Attributes.GetNamedItem("bpt").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:breakPoint", xnMgr).InnerText;
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:rbd", xnMgr).InnerText;
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("cabin").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:cabin", xnMgr).InnerText;
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = (ADC + CHC).ToString();
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:fareBasis", xnMgr).InnerText;
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("ptc").InnerText = groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:passengerType", xnMgr).InnerText;
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("tkd").InnerText = (groupOfFares.SelectNodes("m:ticketInfos", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).InnerText : "";

                            FareType = NewFare.SelectSingleNode("fare/fareType");

                            foreach (XmlNode TmpFareType in groupOfFares.SelectNodes("m:productInformation/m:fareProductDetail/m:fareType", xnMgr))
                            {
                                NewFareType = NewFare.SelectSingleNode("fare").AppendChild(FareType.Clone());
                                NewFareType.InnerText = TmpFareType.InnerText;
                            }

                            NewFare.SelectSingleNode("fare").RemoveChild(FareType);

                            foreach (XmlNode referencingDetail in groupOfFares.SelectNodes("m:fareFamiliesRef/m:referencingDetail[m:refQualifier='F']", xnMgr))
                            {
                                NewFamilyNumber = FareFamily.AppendChild(FamilyNumber.CloneNode(false));
                                NewFamilyNumber.InnerText = referencingDetail.SelectSingleNode("m:refNumber", xnMgr).InnerText;
                            }
                        }

                        FareFamily.RemoveChild(FamilyNumber);
                        NewSegmentFare.RemoveChild(Fare);
                    }

                    SegmentFareGroup.RemoveChild(SegmentFare);

                    //탑승객 번호
                    Traveler = NewPaxFare.SelectSingleNode("traveler");
                    Ref = Traveler.SelectSingleNode("ref");

                    foreach (XmlNode traveller in paxFareProduct.SelectNodes("m:paxReference/m:traveller", xnMgr))
                    {
                        NewRef = Traveler.AppendChild(Ref.CloneNode(false));
                        NewRef.InnerText = traveller.SelectSingleNode("m:ref", xnMgr).InnerText;

                        if (traveller.SelectNodes("m:infantIndicator", xnMgr).Count > 0)
                        {
                            NewRef.Attributes.GetNamedItem("ind").InnerText = traveller.SelectSingleNode("m:infantIndicator", xnMgr).InnerText;
                        }
                        else
                        {
                            NewRef.Attributes.RemoveNamedItem("ind");
                        }
                    }

                    Traveler.RemoveChild(Ref);
                }

                PaxFareGroup.RemoveChild(PaxFare);

                //free Text
                foreach (XmlNode fare in Recommendation.SelectNodes("m:paxFareProduct[1]/m:fare", xnMgr))
                {
                    XmlDocument TmpXml = new XmlDocument();
                    TmpXml.LoadXml(fare.OuterXml.Replace(String.Format(" xmlns=\"{0}\"", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch")), ""));

                    NewPriceIndex.SelectSingleNode("fareMessage").AppendChild(NewPriceIndex.OwnerDocument.ImportNode(TmpXml.SelectSingleNode("fare"), true));
                }

                if (!NewPriceIndex.SelectSingleNode("fareMessage").HasChildNodes)
                    NewPriceIndex.RemoveChild(NewPriceIndex.SelectSingleNode("fareMessage"));
            }

            PriceInfo.RemoveChild(PriceIndex);

            return XmlDoc.DocumentElement;
        }

        #endregion "할인항공 스케쥴 조회(달력)"

        #region "프로모션"

        /// <summary>
		/// 프로모션 리스트
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="ARC">지역 코드(ASIA:동남아, JPN:일본, CHI:중국, EUR:유럽, AMCA:미주, SOPA:남태평양, AFR:중동/아프리카, CSAM:중남미)</param>
		/// <param name="CRC">국가 코드</param>
		/// <param name="DLC">출발지 공항 코드</param>
		/// <param name="ALC">도착지 공항 코드</param>
		/// <param name="SAC">항공사 코드</param>
		/// <returns></returns>
		public static XmlElement SearchPromotionList(int SNM, string ARC, string CRC, string DLC, string ALC, string SAC)
		{
			XmlDocument XmlDoc = new XmlDocument();

			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
				{
					using (DataSet ds = new DataSet("promotionInfo"))
					{
						cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSP_S_항공할인_알뜰";

						cmd.Parameters.Add("@SNM", SqlDbType.Int, 0);
						cmd.Parameters.Add("@DLC", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@ARC", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@CRC", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@ALC", SqlDbType.VarChar, 20);
						cmd.Parameters.Add("@SAC", SqlDbType.VarChar, 20);

						cmd.Parameters["@SNM"].Value = SNM;
						cmd.Parameters["@DLC"].Value = DLC;
						cmd.Parameters["@ARC"].Value = ARC;
						cmd.Parameters["@CRC"].Value = CRC;
						cmd.Parameters["@ALC"].Value = ALC;
						cmd.Parameters["@SAC"].Value = SAC;

						adp.Fill(ds, "item");

						XmlDoc.LoadXml(ds.GetXml().Replace(" xml:space=\"preserve\"", ""));
						ds.Clear();
					}
				}
			}

			return XmlDoc.DocumentElement;
		}

		/// <summary>
		/// 프로모션 상세정보
		/// </summary>
		/// <param name="PMID">프로모션 번호</param>
		/// <returns></returns>
		public static XmlElement SearchPromotionDetail(int PMID)
		{
			XmlDocument XmlDoc = new XmlDocument();

			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
				{
					using (DataSet ds = new DataSet("promotionInfo"))
					{
						cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = "DBO.WSP_S_항공할인_할인번호";

						cmd.Parameters.Add("@할인번호", SqlDbType.Int, 0);
						cmd.Parameters["@할인번호"].Value = PMID;

						adp.Fill(ds, "item");

						XmlDoc.LoadXml(ds.GetXml().Replace(" xml:space=\"preserve\"", ""));
						ds.Clear();
					}
				}
			}

			return XmlDoc.DocumentElement;
		}

		#endregion "프로모션"

		#region "함수"

		/// <summary>
		/// SQL 구문에서 사용하기 위한 Split 변환
		/// </summary>
		/// <param name="Data">데이타</param>
		/// <param name="Separator">구분자</param>
		/// <returns></returns>
		public string SplitForSQL(string Data, string Separator)
		{
			string TmpSQL = "";

			foreach (string TmpData in Data.Split(Convert.ToChar(Separator)))
				TmpSQL = String.Concat(TmpSQL, (TmpSQL.Length.Equals(0) ? "" : ","), "'", TmpData, "'");
			
			return TmpSQL;
		}

		/// <summary>
		/// 할인항공 프로모션 적용
		/// </summary>
		/// <param name="DisXml">할인항공 리스트</param>
		/// <param name="PromXml">프모모션 리스트</param>
		/// <returns></returns>
		public static XmlElement ApplyPromotion(XmlElement DisXml, XmlElement PromXml)
		{
			Common cm = new Common();
			XmlNode NewFare;
			XmlNode PromotionInfo;

			foreach (XmlNode Fare in DisXml.SelectNodes("fare"))
			{
				string CabinClassItem = (Fare.SelectSingleNode("CABIN_SEAT_GRAD").InnerText.Equals("M")) ? "Y" : (Fare.SelectSingleNode("CABIN_SEAT_GRAD").InnerText.Equals("W") ? "Y" : Fare.SelectSingleNode("CABIN_SEAT_GRAD").InnerText);

				foreach (XmlNode PromItem in PromXml.SelectNodes(String.Format("item[desAirport='{2}' and orgArea='{0}' and orgNation='{1}' and orgAirport='{3}' and airCode='{4}' and paxType='{5}' and cabinClass='{6}' and tripType='{7}']", Fare.SelectSingleNode("AREA_CD").InnerText, Fare.SelectSingleNode("NA_CD").InnerText, Fare.SelectSingleNode("DEP_CITY_CD").InnerText, Fare.SelectSingleNode("ARR_CITY_CD").InnerText, Fare.SelectSingleNode("IATA_AIR_CD").InnerText, Fare.SelectSingleNode("PAX_TYPE").InnerText, CabinClassItem, Fare.SelectSingleNode("TRIP_TYPE").InnerText)))
				{
					if (PromItem.SelectSingleNode("fareType").InnerText.IndexOf(Fare.SelectSingleNode("DEPLOY_FARE_KIND").InnerText) != -1)
					{
						PromotionInfo = DisXml.OwnerDocument.CreateElement("promotionInfo");
						PromotionInfo.AppendChild(DisXml.OwnerDocument.ImportNode(PromItem, true));
						
						NewFare = DisXml.InsertBefore(Fare.Clone(), Fare);
						NewFare.AppendChild(PromotionInfo);

						NewFare.SelectSingleNode("ADT_DSCNT_FARE").InnerText = cm.PromotionFare(cm.RequestDouble(NewFare.SelectSingleNode("ADT_FARE").InnerText), "ADT", PromItem).ToString();
						NewFare.SelectSingleNode("CHD_DSCNT_FARE").InnerText = cm.PromotionFare(cm.RequestDouble(NewFare.SelectSingleNode("CHD_FARE").InnerText), "CHD", PromItem).ToString();
						NewFare.SelectSingleNode("PAX_TYPE").InnerText = PromItem.SelectSingleNode("incentiveCode").InnerText;
						NewFare.SelectSingleNode("PAX_TYPE_DESC").InnerText = PromItem.SelectSingleNode("fareTarget").InnerText;
					}
				}

				Fare.SelectSingleNode("PAX_TYPE_DESC").InnerText = Common.PaxTypeText(Fare.SelectSingleNode("PAX_TYPE_DESC").InnerText);
			}

			return DisXml;
		}
		#endregion "함수"

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

                sqlParam[0].Value = 428;
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
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 428, 0, 0).ToErrors;
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

                sqlParam[0].Value = 429;
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
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirDiscount", MethodBase.GetCurrentMethod().Name, 429, 0, 0).ToErrors;
			}
		}

		#endregion "메서드 설명"
	}
}