using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// Sabre 실시간 항공 예약을 위한 각종 정보 제공
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	public class SabreAirService : System.Web.Services.WebService
	{
		Common cm;
		SabreConfig sc;
		HttpContext hcc;

        public SabreAirService()
		{
			cm = new Common();
            sc = new SabreConfig();
			hcc = HttpContext.Current;
		}

        [WebMethod(Description = "Fare + Availability 동시조회")]
        public XmlElement SearchFareAvailRSTEST()
        {
            string[] Dep = new String[4] { "SEL", "YGJ", "", "" };
            string[] Arr = new String[4] { "YGJ", "SEL", "", "" };
            string[] DepDate = new String[4] { "20181211", "20181214", "", "" };

            return SearchFareAvailBFMRS("RT", Dep, Arr, DepDate, "20181211", "", "Y", 1, 0, 0, "", "", 10, cm.GetGUID);
        }
        
        #region "Fare + Availability 동시조회(FMS)"

        [WebMethod(Description = "Fare + Availability 동시조회(FMS)")]
        public string SearchFareAvailFMSRQ(string Trip, string[] Dep, string[] Arr, string[] DepDate, string RetDate, string Validity, string Cabin, int ADC, int CHC, int IFC, string SearchCar, string ExcvCar, int MaxFare)
		{
            if (MaxFare > 0)
                return String.Format("idtTotal=Y&Action=ALL&Mode=L&trip={0}&dep0={1}&arr0={2}&depdate0={3}&dep1={4}&arr1={5}&depdate1={6}&dep2={7}&arr2={8}&depdate2={9}&dep3={10}&arr3={11}&depdate3={12}&retdate={13}&val={14}&comp={15}&idt=ALL&adt={16}&chd={17}&inf={18}&inf_st=0&DBCharSet=UTF-8&CharSet=UTF-8&siteInd=MT&mSiteInd=P&MapType=FARE&origin=MODETOUR&fareOrigin=MODETOUR&GroupCode=FG11&L1SiteType=W_MODETOUR&tasfOrigin=MODETOUR&L1Code=2000&SearchCar={19}&ExcvCar={20}&Status=OK&MaxFare={21}", Trip, Dep[0], Arr[0], DepDate[0], Dep[1], Arr[1], DepDate[1], Dep[2], Arr[2], DepDate[2], Dep[3], Arr[3], DepDate[3], RetDate, Validity, Cabin, ADC, CHC, IFC, SearchCar, ExcvCar, MaxFare);
            else
                return String.Format("idtTotal=Y&Action=ALL&Mode=L&trip={0}&dep0={1}&arr0={2}&depdate0={3}&dep1={4}&arr1={5}&depdate1={6}&dep2={7}&arr2={8}&depdate2={9}&dep3={10}&arr3={11}&depdate3={12}&retdate={13}&val={14}&comp={15}&idt=ALL&adt={16}&chd={17}&inf={18}&inf_st=0&DBCharSet=UTF-8&CharSet=UTF-8&siteInd=MT&mSiteInd=P&MapType=FARE&origin=MODETOUR&fareOrigin=MODETOUR&GroupCode=FG11&L1SiteType=W_MODETOUR&tasfOrigin=MODETOUR&L1Code=2000&SearchCar={19}&ExcvCar={20}&Status=OK", Trip, Dep[0], Arr[0], DepDate[0], Dep[1], Arr[1], DepDate[1], Dep[2], Arr[2], DepDate[2], Dep[3], Arr[3], DepDate[3], RetDate, Validity, Cabin, ADC, CHC, IFC, SearchCar, ExcvCar);
		}

		/// <summary>
        /// Fare + Availability 동시조회(FMS)
		/// </summary>
		/// <param name="Trip">여정(OW:편도, RT:왕복, MD:다구간)</param>
		/// <param name="Dep">출발도시</param>
		/// <param name="Arr">도착도시</param>
		/// <param name="DepDate">출발일(YYYYMMDD)</param>
		/// <param name="RetDate">왕복여정의 리턴 출발일(YYYYMMDD)</param>
        /// <param name="Validity">오픈여정의 최소 체류일(5D/7D/14D/1M/2M/3M/6M/1Y)</param>
		/// <param name="Cabin">캐빈클래스(F:일등석, C:비즈니스석, P:프리미엄일반석, Y:일반석)</param>
		/// <param name="ADC">성인 탑승객수</param>
		/// <param name="CHC">소아 탑승객수</param>
		/// <param name="IFC">유아 탑승객수</param>
		/// <param name="SearchCar">선택 항공사(공백:전체 항공사 검색)</param>
        /// <param name="ExcvCar">제외 항공사(공백:전체 항공사 검색</param>
        /// <param name="MaxFare">항공사별 운임 결과 갯수(0:제한없음)</param>
        /// <param name="GUID">고유번호</param>
		/// <returns></returns>
        [WebMethod(Description = "Fare + Availability 동시조회(FMS)")]
        public XmlElement SearchFareAvailFMSRS(string Trip, string[] Dep, string[] Arr, string[] DepDate, string RetDate, string Validity, string Cabin, int ADC, int CHC, int IFC, string SearchCar, string ExcvCar, int MaxFare, string GUID)
		{
            string Parameters = SearchFareAvailFMSRQ((Trip.Equals("DT") ? "MD" : Trip), Dep, Arr, DepDate, RetDate, Validity, Cabin, ADC, CHC, IFC, SearchCar.Replace(",", "/"), ExcvCar, MaxFare);
            cm.XmlFileSave(Parameters, sc.Name, "SearchFareAvailFMSRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("SearchFareAvailFMS", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "SearchFareAvailFMSRS", "N", GUID);

            return ResXml;
        }

        #endregion "Fare + Availability 동시조회(FMS)"

        #region "Fare + Availability 동시조회(BFM)"

        [WebMethod(Description = "Fare + Availability 동시조회(BFM)")]
        public string SearchFareAvailRQ(string Trip, string[] Dep, string[] Arr, string[] DepDate, string RetDate, string Validity, string Cabin, int ADC, int CHC, int IFC, string SiteInd)
        {
            return String.Format("trip={0}&dep0={1}&arr0={2}&depdate0={3}&dep1={4}&arr1={5}&depdate1={6}&dep2={7}&arr2={8}&depdate2={9}&dep3={10}&arr3={11}&depdate3={12}&retdate={13}&comp={14}&adt={15}&chd={16}&inf={17}&mSiteInd={18}&dsize={19}&adtyn=false&sdate=&siteInd=LC&origin=MODETOUR", Trip, Dep[0], Arr[0], DepDate[0], Dep[1], Arr[1], DepDate[1], Dep[2], Arr[2], DepDate[2], Dep[3], Arr[3], DepDate[3], ((Trip.Equals("RT") && !String.IsNullOrWhiteSpace(Validity)) ? "OPEN" : RetDate), Cabin, ADC, CHC, IFC, SiteInd, (Trip.Equals("MD") ? (String.IsNullOrWhiteSpace(Dep[2]) ? "2" : (String.IsNullOrWhiteSpace(Dep[3]) ? "3" : "4")) : ""));
        }

        /// <summary>
        /// Fare + Availability 동시조회(BFM)
        /// </summary>
        /// <param name="Trip">여정(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="Dep">출발도시</param>
        /// <param name="Arr">도착도시</param>
        /// <param name="DepDate">출발일(YYYYMMDD)</param>
        /// <param name="RetDate">왕복여정의 리턴 출발일(YYYYMMDD)</param>
        /// <param name="Validity">오픈여정의 최소 체류일(5D/7D/14D/1M/2M/3M/6M/1Y)</param>
        /// <param name="Cabin">캐빈클래스(F:일등석, C:비즈니스석, P:프리미엄일반석, Y:일반석)</param>
        /// <param name="ADC">성인 탑승객수</param>
        /// <param name="CHC">소아 탑승객수</param>
        /// <param name="IFC">유아 탑승객수</param>
        /// <param name="SearchCar">선택 항공사(공백:전체 항공사 검색)</param>
        /// <param name="ExcvCar">제외 항공사(공백:전체 항공사 검색</param>
        /// <param name="MaxFare">항공사별 운임 결과 갯수(0:제한없음)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare + Availability 동시조회(BFM)")]
        public XmlElement SearchFareAvailBFMRS(string Trip, string[] Dep, string[] Arr, string[] DepDate, string RetDate, string Validity, string Cabin, int ADC, int CHC, int IFC, string SearchCar, string ExcvCar, int MaxFare, string GUID)
        {
            string Parameters = SearchFareAvailRQ((Trip.Equals("DT") ? "MD" : Trip), Dep, Arr, DepDate, RetDate, Validity, Cabin, ADC, CHC, IFC, "M");
            cm.XmlFileSave(Parameters, sc.Name, "SearchFareAvailRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("SearchFareAvail", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "SearchFareAvailRS", "N", GUID);

            return ResXml;
        }

        /// <summary>
        /// Fare + Availability 동시조회(BFM)
        /// </summary>
        /// <param name="Trip">여정(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="Dep">출발도시</param>
        /// <param name="Arr">도착도시</param>
        /// <param name="DepDate">출발일(YYYYMMDD)</param>
        /// <param name="RetDate">왕복여정의 리턴 출발일(YYYYMMDD)</param>
        /// <param name="Validity">오픈여정의 최소 체류일(5D/7D/14D/1M/2M/3M/6M/1Y)</param>
        /// <param name="Cabin">캐빈클래스(F:일등석, C:비즈니스석, P:프리미엄일반석, Y:일반석)</param>
        /// <param name="ADC">성인 탑승객수</param>
        /// <param name="CHC">소아 탑승객수</param>
        /// <param name="IFC">유아 탑승객수</param>
        /// <param name="SiteInd">PC/모바일 구분(P:PC, M:모바일)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Fare + Availability 동시조회(BFM)")]
        public XmlElement SearchFareAvailRS(string Trip, string[] Dep, string[] Arr, string[] DepDate, string RetDate, string Validity, string Cabin, int ADC, int CHC, int IFC, string SiteInd, string GUID)
        {
            string Parameters = SearchFareAvailRQ((Trip.Equals("MT") ? "MD" : Trip), Dep, Arr, DepDate, RetDate, Validity, Cabin, ADC, CHC, IFC, SiteInd);
            cm.XmlFileSave(Parameters, sc.Name, "SearchFareAvailRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("SearchFareAvail", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "SearchFareAvailRS", "N", GUID);

            return ResXml;
        }

        #endregion "Fare + Availability 동시조회(BFM)"

        #region "운임규정조회"

        /// <summary>
        /// 운임규정 조회
        /// </summary>
        /// <param name="FareRuleUrl">운임조회에서 넘어온 FareRuleUrl</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "운임규정 조회")]
        public XmlElement FareRuleRS(string FareRuleUrl, string GUID)
        {
            XmlElement ResXml = null;

            //target=_blank 태그 추가시키는 파라미터
            FareRuleUrl += "&addAnchor=Y";

            if (FareRuleUrl.IndexOf("&BfmInd=Y") != -1)
            {
                FareRuleUrl = FareRuleUrl.Replace("&amp;", "&").Replace("^", "&");
                cm.XmlFileSave(FareRuleUrl, sc.Name, "BFMFareRuleRQ", "N", GUID);

                ResXml = sc.HttpExecute("BFMFareRule", FareRuleUrl);
                cm.XmlFileSave(ResXml, sc.Name, "BFMFareRuleRS", "N", GUID);
            }
            else
            {
                FareRuleUrl = FareRuleUrl.Replace("&amp;", "&").Replace("^", "&");
                cm.XmlFileSave(FareRuleUrl, sc.Name, "FareRuleRQ", "N", GUID);

                ResXml = sc.HttpExecute("FareRule", FareRuleUrl);
                cm.XmlFileSave(ResXml, sc.Name, "FareRuleRS", "N", GUID);
            }

            return ResXml;
        }

        #endregion "운임규정조회"

        #region "유효성 체크(BFM용)"

        /// <summary>
        /// 유효성 체크(BFM용)
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        [WebMethod(Description = "유효성 체크(BFM용)")]
        public XmlElement RevailDateRS(string Parameters, string GUID)
        {
            cm.XmlFileSave(Parameters, sc.Name, "RevailDateRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("RevailDate", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "RevailDateRS", "N", GUID);

            return ResXml;
        }

        #endregion "유효성 체크(BFM용)"

        #region "예약 전처리"

        public string SegHoldRQ(string CommonHeader, string FARE_REC1, string FARE_REC2, string FARE_REC3, string FSCLS, string SelectedItin)
        {
            return String.Format("actiontype=itin&ParmType=Inline&{0}{1}{2}{3}{4}{5}", CommonHeader.Replace("&amp;", "&"), FARE_REC1.Replace("&amp;", "&"), FARE_REC2.Replace("&amp;", "&"), SelectedItin.Replace("&amp;", "&"), FARE_REC3.Replace("&amp;", "&"), FSCLS.Replace("&amp;", "&"));
        }

        /// <summary>
        /// 예약 전처리
        /// </summary>
        /// <param name="CommonHeader"></param>
        /// <param name="FARE_REC1"></param>
        /// <param name="FARE_REC2"></param>
        /// <param name="FARE_REC3"></param>
        /// <param name="FSCLS"></param>
        /// <param name="SelectedItin"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        [WebMethod(Description = "예약 전처리")]
        public XmlElement SegHoldRS(string CommonHeader, string FARE_REC1, string FARE_REC2, string FARE_REC3, string FSCLS, string SelectedItin, string GUID)
        {
            string Parameters = SegHoldRQ(CommonHeader, FARE_REC1, FARE_REC2, FARE_REC3, FSCLS, SelectedItin);
            cm.XmlFileSave(Parameters, sc.Name, "SegHoldRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("SegHold", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "SegHoldRS", "N", GUID);

            return ResXml;
        }

        /// <summary>
        /// 예약 전처리(BFM용)
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        [WebMethod(Description = "예약 전처리(BFM용)")]
        public XmlElement SegHoldBFMRS(string Parameters, string GUID)
        {
            cm.XmlFileSave(Parameters, sc.Name, "SegHoldBFMRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("SegHold", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "SegHoldBFMRS", "N", GUID);

            return ResXml;
        }

        #endregion "예약 전처리"

        #region "예약 생성"

        public string AirBookRQ(string PrsId, string Origin, string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string RTN, string RMN, string REA, string RMK)
        {
            string StrPram = string.Empty;
            string idx = string.Empty;
            int ADC = 0;
            int CHC = 0;
            int IFC = 0;

            StrPram = "actiontype=pnr&dspmode=xml";
            StrPram += String.Concat("&prs_id=", PrsId);
            StrPram += String.Concat("&pcd_mobile_ctc1=", RMN.Replace("-", ""));
            StrPram += String.Concat("&pcd_home_ctc1=", RTN.Replace("-", ""));
            StrPram += String.Concat("&pcd_email1=", REA);

            for (int i = 0; i < PTC.Length; i++)
            {
                idx = (i + 1).ToString();

                StrPram += String.Format("&pax0{0}_last_name={1}", idx, PSN[i]);
                StrPram += String.Format("&pax0{0}_gvn_name={1}", idx, PFN[i]);
                StrPram += String.Format("&pax0{0}_sex={1}", idx, Common.GetPaxGender(PTL[i]));
                StrPram += String.Format("&pax0{0}_idt={1}", idx, PTC[i]);
                StrPram += String.Format("&pax0{0}_birth={1}", idx, PBD[i].Replace("-", ""));
                StrPram += String.Format("&pax0{0}_ffp1=", idx);

                if (PTC[i].Equals("CHD"))
                    CHC++;
                else if (PTC[i].Equals("INF"))
                    IFC++;
                else
                    ADC++;
            }

            StrPram += String.Concat("&adt=", ADC);
            StrPram += String.Concat("&chd=", CHC);
            StrPram += String.Concat("&inf=", IFC);
            StrPram += "&cuid=nomember&b2cgrade=&CharSet=UTF-8&DBCharSet=UTF-8&CharSet=EUC-KR&LangCode=KOR&L1Code=2000&GroupCode=FG11&L1SiteType=W_OMEGA&L2Code=&L2Grade=&L2SiteCode=&TLType=CT&tasfOrigin=OMEGA&Email=Y&NewTL=Y&taCtc=1544-5353 MODETOUR NETWORK CENTER";

            return StrPram;
        }

        [WebMethod(Description = "예약 생성")]
        public XmlElement AirBookRS(string PrsId, string Origin, string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string RTN, string RMN, string REA, string RMK, string GUID)
        {
            string Parameters = AirBookRQ(PrsId, Origin, PTC, PTL, PHN, PSN, PFN, PBD, PTN, PMN, PEA, RTN, RMN, REA, RMK);
            cm.XmlFileSave(Parameters, sc.Name, "AirBookRQ", "N", GUID);

            XmlElement ResXml = sc.HttpExecute("AirBook", Parameters);
            cm.XmlFileSave(ResXml, sc.Name, "AirBookRS", "N", GUID);

            return ResXml;
        }

        #endregion "예약 생성"

        #region "Database"

        #region "무료수하물조회"

        /// <summary>
        /// SQL을 이용한 Oracle DB결과 리턴
        /// </summary>
        /// <param name="SQL">오라클 쿼리</param>
        /// <returns></returns>
        private XmlElement SearchBaggageDB(string SQL)
        {
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OMEGA"].ConnectionString))
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
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
            }
        }

        [WebMethod(Description = "무료수하물규정 조회")]
        public XmlElement SearchBaggageRS(string AirCode)
        {
            return SearchBaggageDB(String.Concat("SELECT * FROM (SELECT ROW_NUMBER() OVER(PARTITION BY ACB_CAR_CODE ORDER BY ACB_DATA_IND ASC) RN",
                                                                        ",AIR_CAR_LCL AS AIR_CAR_LCL",
                                                                        ",AIR_CAR_ENG AS AIR_CAR_ENG",
                                                                        ",ACB_CAR_CODE",
                                                                        ",ACB_BAGGAGE_DESC",
                                                                        ",ACB_BAGGAGE_EN_DESC",
                                                                        ",ACB_BAGGAGE_URL",
                                                                        ",ACB_HOMEPAGE_URL",
                                                                        ",ACB_DATA_IND ",
                                                                 "FROM AIR_CAR_BAGGAGE ACB, AIRLINE_CODE AIR ",
                                                                 "WHERE ACB_CAR_CODE = AIR_CAR(+) ",
                                                                       "AND ACB_CAR_CODE = '" + AirCode + "' ",
                                                                       "AND ACB_DATA_IND IN ('B','R'))",
                                                   "WHERE RN = 1"));
        }

        #endregion "무료수하물조회"

        #region "할인율 정보 조회"

        /// <summary>
        /// SQL을 이용한 Oracle DB결과 리턴
        /// </summary>
        /// <param name="CPC">프로모션코드</param>
        /// <returns></returns>
        public XmlElement SearchPromotionDB(string CPC)
        {
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OMEGA"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        OracleDataAdapter adp = new OracleDataAdapter(cmd);
                        DataSet ds = new DataSet("cardPromotions");

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = String.Format("SELECT * FROM CARD_PROMOTION_CODE WHERE CPC_CODE = '{0}'", CPC);

                        adp.Fill(ds, "cardPromotion");
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
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
            }
        }

        #endregion "할인율 정보 조회"

        #region "할인율 맵핑정보 조회"

        /// <summary>
        /// SQL을 이용한 Oracle DB결과 리턴
        /// </summary>
        /// <param name="CPC">프로모션코드</param>
        /// <param name="GSC">사이트코드</param>
        /// <returns></returns>
        public XmlElement SearchPromotionMappingDB(string CPC, string GSC)
        {
            using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OMEGA"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        OracleDataAdapter adp = new OracleDataAdapter(cmd);
                        DataSet ds = new DataSet("cardPromotions");

                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = String.Format("SELECT * FROM CARD_PROMOTION_MAPLIST WHERE CPM_CPC_CODE = '{0}' AND CPM_GSC_CODE = '{1}'", CPC, GSC);

                        adp.Fill(ds, "cardPromotion");
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
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new MWSException(ex, hcc, sc.Name, MethodBase.GetCurrentMethod().Name, 0, 0);
                }
            }
        }

        #endregion "할인율 맵핑정보 조회"

        #endregion "Database"
    }
}