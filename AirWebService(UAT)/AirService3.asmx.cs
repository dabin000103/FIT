using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
	/// 실시간 항공(해외) 예약 관리를 위한 웹서비스(통합용)
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class AirService3 : System.Web.Services.WebService
	{
		Common cm;
		ModeConfig mc;
        AirService asv;
		AmadeusAirService amd;
        SabreAirService sas;
        AES256Cipher aes;
        LogSave log;
        SearchSave sch;
		HttpContext hcc;
		XmlNode RunTime;
		XmlNode RunTimeService;
        System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        
		public AirService3()
		{
			cm = new Common();
			mc = new ModeConfig();
            asv = new AirService();
			amd = new AmadeusAirService();
            sas = new SabreAirService();
            aes = new AES256Cipher();
            log = new LogSave();
            sch = new SearchSave();
			hcc = HttpContext.Current;

			//실행시간 체크용
			XmlDocument XmlCheck = new XmlDocument();
			XmlCheck.Load(mc.XmlFullPath("CheckRunTime"));
			RunTime = XmlCheck.SelectSingleNode("ResponseDetails/runTime");
			RunTimeService = RunTime.SelectSingleNode("service");
		}

        #region "TEST"

        #region "Fare + Availability 동시조회"

        [WebMethod(Description = "Fare + Availability 동시조회(개발테스트용)(실시간검색)")]
        public XmlElement AAATEST1_SearchFareAvailRS()
        {
            Stopwatch sw; //실행시간 체크************
            XmlNode NewRunTime = RunTime.CloneNode(false); //실행시간 체크************

            sw = Stopwatch.StartNew(); //실행시간 체크************

            //XmlElement ModeXml = SearchFareAvailSimpleRS(2, "", "SEL", "SIN", "OW", "20190305", "20190310", "Y", "", 3, 0, 0, "WEBSERVICE");
            //XmlElement ModeXml = SearchFareAvailSimpleRS(2, "", "SEL", "HKT", "RT", "20190523", "20190526", "Y", "", 2, 1, 0, "WEBSERVICE");
            XmlElement ModeXml = SearchFareAvailSimpleRS(2, "", "SEL", "BKK", "RT", "20190923", "20190926", "M", "N", 1, 0, 0, "WEBSERVICE");
            //XmlElement ModeXml = SearchFareAvailSimpleRS(2, "", "SEL,BKK", "SIN,SEL", "MD", "20190305,20190310", "", "Y", "", 1, 0, 0, "WEBSERVICE");
            //XmlElement ModeXml = SearchFareAvailSimpleRS(2, "", "SEL,PQC,SGN", "PQC,SGN,SEL", "MD", "20190420,20190425,20190427", "", "Y", "", 1, 0, 0, "WEBSERVICE");

            CheckRunTimeEnd(NewRunTime, sw, "SearchFareAvailSimpleRS"); //실행시간 체크************
            //ModeXml.SelectSingleNode("runTime").AppendChild(ModeXml.OwnerDocument.ImportNode(NewRunTime.SelectSingleNode("service"), true)); //실행시간 체크************

            return ModeXml;
        }

        [WebMethod(Description = "Fare + Availability 동시조회(개발테스트용)(로그검색)")]
        public XmlElement AAATEST2_SearchFareAvailRS(string ROT)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath(String.Format("_SFA_{0}", ROT)));

            return XmlDoc.DocumentElement;
        }

        [WebMethod(Description = "Fare + Availability 동시조회(개발테스트용)(로그검색)")]
        public XmlElement AAATEST3_SearchFareAvailRS()
        {
            XmlDocument ResXml = new XmlDocument();
            ResXml.Load(mc.XmlFullPath("_MPIS"));

            XmlDocument PromXml = new XmlDocument();
            PromXml.Load(mc.XmlFullPath("_Promotion"));

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(ResXml.NameTable);
            xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_InstantTravelBoardSearch"));

            return ToModeSearchFareAvailGroupingAmadeusRS(2, ResXml.DocumentElement, PromXml.DocumentElement, xnMgr, "", "SEL", "SIN", "RT", "N", "M", "20190305", "Y", "Y", "Y", "", "MPIS", "", RunTime.CloneNode(false));
        }

        #endregion "Fare + Availability 동시조회"

        #region "Fare + Availability 동시조회 그루핑"

        [WebMethod(Description = "")]
        public XmlElement AAATEST1_MasterPricerSearchAmadeusRS()
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("_MP"));

            XmlElement XmlGDS = XmlDoc.DocumentElement;

            int SNM = 2;
            string SAC = "";
            string XAC = "";
            string DLC = "SEL";
            string ALC = "SPN";
            string CLC = "";
            string ROT = "RT";
            string DTD = "20190618";
            string ARD = "20190625";
            string OPN = "N";
            string FLD = "";
            string CCD = "M";
            string ACQ = "";
            string FAB = "";
            string PTC = "ADT";
            int ADC = 1;
            int CHC = 0;
            int IFC = 0;
            int NRR = 200;
            string PUB = "Y";
            int WLR = 0;
            string LTD = "Y";
            string FTR = "Y";
            string MTL = "";
            string GUID = "ITHV010-201906051551056769";
            XmlElement PromXml = null;

            string StepGUID = String.Format("{0}-{1}-{2}-{3}-{4}", GUID, ROT, PTC, CCD, SAC);
            
            Stopwatch sw; //실행시간 체크************
            XmlNode NewRunTime = RunTime.CloneNode(false); //실행시간 체크************
            XmlAttribute RunTimeGuid = NewRunTime.OwnerDocument.CreateAttribute("guid"); //실행시간 체크************
            RunTimeGuid.InnerText = String.Format("{0}/{1}/{2}", PTC, CCD, SAC); //실행시간 체크************
            NewRunTime.Attributes.Append(RunTimeGuid); //실행시간 체크************

            //네임스페이스 정의
            XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlGDS.OwnerDocument.NameTable);
            xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch"));

            //MaxStay(유효기간) 추가
            XmlAttribute MaxStay;
            string DepartureDate = cm.RequestDateTime(DTD.Split(',')[0].Trim());

            foreach (XmlNode Value in XmlGDS.SelectNodes("m:value", xnMgr))
            {
                MaxStay = Value.Attributes.Append(XmlGDS.OwnerDocument.CreateAttribute("maxStay"));
                MaxStay.InnerText = Common.ConvertToMaxStay(DepartureDate, Value.SelectSingleNode("m:criteriaDetails/m:value", xnMgr).InnerText);
            }

            //return XmlGDS;
            return ToModeSearchFareAvailGroupingAmadeusRS(SNM, XmlGDS, PromXml, xnMgr, SAC, DLC, ALC, ROT, OPN, CCD, DepartureDate, PUB, LTD, FTR, "", "", StepGUID, NewRunTime);
        }

        #endregion "Fare + Availability 동시조회 그루핑"

        #region "요금규정"

        [WebMethod(Description = "운임규정 조회")]
        public XmlElement AAATEST_SearchRuleRS()
        {
            string SXL = "<itinerary><segGroup ref=\"A00ADTM1001\" ejt=\"2735\" eft=\"2130\" ewt=\"0605\" mjc=\"CA\" cds=\"N\" nosp=\"1\" iti=\"ICN/JFK/201907051525/201907052355/2735/2130/0605/CA/1/0/N\" rtg=\"ICN/PEK/JFK\" aif=\"\" fiRef=\"1\"><seg dlc=\"ICN\" alc=\"PEK\" ddt=\"2019-07-05 15:25\" ardt=\"2019-07-05 16:30\" eft=\"0205\" ett=\"\" gwt=\"\" mcc=\"CA\" occ=\"CA\" cds=\"N\" fln=\"132\" stn=\"0\" etc=\"Y\" aif=\"\"/><seg dlc=\"PEK\" alc=\"JFK\" ddt=\"2019-07-05 22:35\" ardt=\"2019-07-05 23:55\" eft=\"1320\" ett=\"0605\" gwt=\"\" mcc=\"CA\" occ=\"CA\" cds=\"N\" fln=\"989\" stn=\"0\" etc=\"Y\" aif=\"\"/></segGroup><segGroup ref=\"A00ADTM2001\" ejt=\"2410\" eft=\"2010\" ewt=\"0400\" mjc=\"CA\" cds=\"N\" nosp=\"1\" iti=\"EWR/GMP/201907091240/201907102150/2410/2010/0400/CA/1/0/N\" rtg=\"EWR/PEK/GMP\" aif=\"\" fiRef=\"2\"><seg dlc=\"EWR\" alc=\"PEK\" ddt=\"2019-07-09 12:40\" ardt=\"2019-07-10 14:45\" eft=\"1405\" ett=\"\" gwt=\"\" mcc=\"CA\" occ=\"CA\" cds=\"N\" fln=\"820\" stn=\"0\" etc=\"Y\" aif=\"\"/><seg dlc=\"PEK\" alc=\"GMP\" ddt=\"2019-07-10 18:45\" ardt=\"2019-07-10 21:50\" eft=\"0205\" ett=\"0400\" gwt=\"\" mcc=\"CA\" occ=\"CA\" cds=\"N\" fln=\"137\" stn=\"0\" etc=\"Y\" aif=\"\"/></segGroup></itinerary>";
            string FXL = "<paxFareGroup ref=\"A00ADTMB001\" gds=\"Amadeus\" ptc=\"ADT\" mode=\"\"><summary price=\"954800\" subPrice=\"944800\" fare=\"590000\" disFare=\"590000\" tax=\"131800\" fsc=\"223000\" disPartner=\"0\" tasf=\"10000\" mTasf=\"10000\" aTasf=\"0\" pvc=\"CA\" mas=\"6M\" cabin=\"M\" ttl=\"2019-07-05\" cds=\"N\" sutf=\"N\" pmcd=\"305324\" pmtl=\"성인/삼성카드할인\" pmsc=\"01\" pmsn=\"삼성\" sscd=\"\" sstl=\"\"/><paxFare ptc=\"ADT\" price=\"954800\" fare=\"590000\" disFare=\"590000\" tax=\"131800\" fsc=\"223000\" disPartner=\"0\" tasf=\"10000\" mTasf=\"10000\" aTasf=\"0\" count=\"1\"><segFare ref=\"1\">1^^LKRCKR6^ADT^^L^M^8^^N^RN,MSP,NTF/2^6M^LKRCKR6^ADT^^L^M^8^^Y^RN,MSP,NTF</segFare><segFare ref=\"2\">1^^TKRCKR6^ADT^^T^M^9^^N^RN,MSP,NTF/2^6M^TKRCKR6^ADT^^T^M^9^^Y^RN,MSP,NTF</segFare></paxFare><promotionInfo>305324^2^CA^RT^^^^^^^0.0000^0.0000^0.0000^0.0000^ADT0120^삼성카드 할인^01^삼성^성인/삼성카드할인^N^^^N^N^9^201906301700</promotionInfo></paxFareGroup>";

            return SearchRuleRS(2, SXL, FXL, "5667", "ITHV010-201904221204064138", "WEBSERVICE");
        }

        #endregion "요금규정"

        #endregion "TEST"

        #region "Fare + Availability 동시조회"

        #region "Fare + Availability 동시조회(통합)"

        /// <summary>
        /// 항공 운임 + 스케쥴 동시 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="DRT">직항여부(Y:직항만, N:전체)</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        [WebMethod(Description = "항공 운임 + 스케쥴 동시 조회")]
        public XmlElement SearchFareAvailSimpleRS(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string CCD, string DRT, int ADC, int CHC, int IFC, string RQT)
        {
            int ServiceNumber = 663;
            string LogGUID = cm.GetGUID;

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

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
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
                sqlParam[14].Value = DRT;
                sqlParam[15].Value = ADC;
                sqlParam[16].Value = CHC;
                sqlParam[17].Value = IFC;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            return SearchFareAvail(SNM, SAC, "", DLC, ALC, "", ROT, DTD, ARD, "N", (DRT.Equals("Y") ? "N" : (DRT.Equals("N") ? "" : DRT)), CCD, "", "", "", ADC, CHC, IFC, 0, "Y", 0, "", "", "Y", "Y", RQT, ServiceNumber, LogGUID);
        }

        /// <summary>
        /// 항공 운임 + 스케쥴 동시 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드(여정구분은 콤마, SEG구분은 슬래시, ex:NRT/SIN,SIN/NRT,RON)</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight nosp allowed, 공백:전체)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PTC">운임타입(ADT:성인운임, STU:학생운임, LBR:노무자운임, 공백:전체)(멀티 선택시 콤마로 구분)</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <param name="NRR">응답 결과 수(Default:0)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <param name="FTX">free text</param>
        [WebMethod(Description = "항공 운임 + 스케쥴 동시 조회")]
        public XmlElement SearchFareAvailRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string RQT, string FTX)
        {
            int ServiceNumber = 655;
            string LogGUID = cm.GetGUID;

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
                        new SqlParameter("@요청18", SqlDbType.VarChar, 3000)
                    };

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = SAC;
                sqlParam[8].Value = DLC;
                sqlParam[9].Value = ALC;
                sqlParam[10].Value = CLC;
                sqlParam[11].Value = ROT;
                sqlParam[12].Value = DTD;
                sqlParam[13].Value = ARD;
                sqlParam[14].Value = OPN;
                sqlParam[15].Value = FLD;
                sqlParam[16].Value = CCD;
                sqlParam[17].Value = ACQ;
                sqlParam[18].Value = FAB;
                sqlParam[19].Value = PTC;
                sqlParam[20].Value = ADC;
                sqlParam[21].Value = CHC;
                sqlParam[22].Value = IFC;
                sqlParam[23].Value = NRR;
                sqlParam[24].Value = FTX;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            int ftxIdx = 0;
            string PUB = "Y";   //PUB요금 출력여부
            int WLR = 0;        //대기예약 포함 비율
            string LTD = "Y";   //발권마감일 체크여부
            string FTR = "Y";   //필터링 사용여부(발권마감일 체크 포함)
            string PRM = "Y";   //프로모션 적용여부
            string AAC = "Y";   //노선별 항공사 추가 검색 여부

            //free text : PUB | WLR | LTD | FTR | PRM | AAC
            if (!String.IsNullOrWhiteSpace(FTX))
            {
                foreach (string StrFTX in FTX.Split('|'))
                {
                    if (ftxIdx.Equals(0))
                        PUB = (StrFTX.Equals("N")) ? "N" : "Y";
                    else if (ftxIdx.Equals(1))
                        WLR = cm.RequestInt(StrFTX);
                    else if (ftxIdx.Equals(2))
                        LTD = (StrFTX.Equals("N")) ? "N" : "Y";
                    else if (ftxIdx.Equals(3))
                        FTR = (StrFTX.Equals("N")) ? "N" : "Y";
                    else if (ftxIdx.Equals(4))
                        PRM = (StrFTX.Equals("N")) ? "N" : "Y";
                    else if (ftxIdx.Equals(5))
                        AAC = (StrFTX.Equals("N")) ? "N" : "Y";

                    ftxIdx++;
                }
            }

            return SearchFareAvail(SNM, SAC, "", DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, FAB, PTC, ADC, CHC, IFC, NRR, PUB, WLR, LTD, FTR, PRM, AAC, RQT, ServiceNumber, LogGUID);
        }

        public XmlElement SearchFareAvail(int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string PRM, string AAC, string RQT, int ServiceNumber, string GUID)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew(); //실행시간 체크************
                XmlNode NewRunTime = RunTime.CloneNode(false); //실행시간 체크************

                //요청정보
                string ReqInfo = String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}^{20}^{21}^{22}", SNM, SAC, XAC, DLC, ALC, CLC, ROT, Common.ConvertToOnlyNumber(DTD), Common.ConvertToOnlyNumber(ARD), OPN, FLD, CCD, ACQ, FAB, PTC, ADC, CHC, IFC, NRR, PUB, WLR, LTD, FTR, PRM, AAC);

                //출도착이원구간 정보를 기존과 동일한 형태로 변경해 준다.
                if (ROT.Equals("MD") && DLC.Split(',').Length.Equals(2))
                {
                    string[] TmpDate = DTD.Split(',');
                    DTD = TmpDate[0];
                    ARD = TmpDate[1];
                    ROT = "DT";
                }

                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(mc.XmlFullPath("SearchFareAvailGrouping3RS"));

                XmlNode ResponseDetails = XmlDoc.SelectSingleNode("ResponseDetails");
                XmlNode PriceSummary = ResponseDetails.SelectSingleNode("priceSummary");
                XmlNode PriceMin = PriceSummary.SelectSingleNode("min");
                XmlNode NewPriceMin;

                ResponseDetails.Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                ResponseDetails.Attributes.GetNamedItem("guid").InnerText = GUID;
                ResponseDetails.Attributes.GetNamedItem("ref").InnerText = "";

                ResponseDetails.RemoveChild(ResponseDetails.SelectSingleNode("flightInfo"));
                ResponseDetails.RemoveChild(ResponseDetails.SelectSingleNode("priceInfo"));
                ResponseDetails.AppendChild(XmlDoc.ImportNode(NewRunTime, true));
                //ResponseDetails.InsertBefore(XmlDoc.ImportNode(NewRunTime, true), PriceSummary);

                SearchFareAvailGrouping2 sfag = new SearchFareAvailGrouping2();

                XmlNode FlightInfo = null;
                XmlNode PriceInfo = null;
                int piRef = 0;

                foreach (XmlElement FareAvail in sfag.GetFareAvail(SNM, SAC, XAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, FAB, PTC, ADC, CHC, IFC, NRR, PUB, WLR, LTD, FTR, PRM, AAC, GUID))
                {
                    if (FareAvail != null && FareAvail.SelectNodes("errorSource").Count.Equals(0))
                    {
                        ResponseDetails.SelectSingleNode("runTime").AppendChild(XmlDoc.ImportNode(FareAvail.SelectSingleNode("runTime"), true));

                        if (ResponseDetails.SelectNodes("flightInfo").Count.Equals(0))
                        {
                            ResponseDetails.InsertAfter(XmlDoc.ImportNode(FareAvail.SelectSingleNode("priceInfo"), true), PriceSummary);
                            ResponseDetails.InsertAfter(XmlDoc.ImportNode(FareAvail.SelectSingleNode("flightInfo"), true), PriceSummary);

                            FlightInfo = ResponseDetails.SelectSingleNode("flightInfo");
                            PriceInfo = ResponseDetails.SelectSingleNode("priceInfo");
                            piRef = FareAvail.SelectNodes("priceInfo/priceIndex").Count;
                        }
                        else
                        {
                            foreach (XmlNode SegGroup in FareAvail.SelectNodes("flightInfo/segGroup"))
                                FlightInfo.AppendChild(XmlDoc.ImportNode(SegGroup, true));

                            foreach (XmlNode PriceIndex in FareAvail.SelectNodes("priceInfo/priceIndex"))
                            {
                                PriceIndex.Attributes.GetNamedItem("ref").InnerText = (++piRef).ToString();
                                PriceInfo.AppendChild(XmlDoc.ImportNode(PriceIndex, true));
                            }
                        }
                    }
                }

                //검색 결과 없음
                if (PriceInfo == null || !PriceInfo.HasChildNodes)
                    throw new Exception("항공요금 검색 결과가 없습니다.");

                #region "항공사별 최저가"

                foreach (XmlNode PriceMinInfo in PriceInfo.SelectNodes("priceIndex[not(@pvc=preceding-sibling::priceIndex/@pvc)]"))
                {
                    XmlNode MinPrice = PriceInfo.SelectSingleNode(String.Format("priceIndex[@pvc='{0}'][not(@minPrice>preceding-sibling::priceIndex[@pvc='{0}']/@minPrice) and not(@minPrice>following-sibling::priceIndex[@pvc='{0}']/@minPrice)]", PriceMinInfo.Attributes.GetNamedItem("pvc").InnerText));

                    NewPriceMin = PriceSummary.AppendChild(PriceMin.CloneNode(true));
                    NewPriceMin.Attributes.GetNamedItem("airline").InnerText = PriceMinInfo.Attributes.GetNamedItem("pvc").InnerText;
                    NewPriceMin.Attributes.GetNamedItem("price").InnerText = (MinPrice != null) ? MinPrice.Attributes.GetNamedItem("minPrice").InnerText : PriceInfo.SelectSingleNode(String.Format("priceIndex[@pvc='{0}']", PriceMinInfo.Attributes.GetNamedItem("pvc").InnerText)).Attributes.GetNamedItem("minPrice").InnerText;
                }

                PriceSummary.RemoveChild(PriceMin);

                #endregion "항공사별 최저가"

                CheckRunTimeEnd(NewRunTime, sw, "SearchFareAvailRS"); //실행시간 체크************
                ResponseDetails.SelectSingleNode("runTime").AppendChild(XmlDoc.ImportNode(NewRunTime.SelectSingleNode("service"), true)); //실행시간 체크************

                //DB처리는 개발중이기 때문에 라이브서버에서는 실행되지 않는다.
                if (Common.MachineName.StartsWith("AHB"))
                {
                    //로그
                    cm.XmlFileSave(XmlDoc, mc.Name, "SearchFareAvailRS", "N", String.Format("{0}-{1}", GUID, ROT));
                }
                else
                {
                    //항공검색번호
                    Int64 S3Idx = sch.GetSearchIdx();
                    ResponseDetails.Attributes.GetNamedItem("ref").InnerText = S3Idx.ToString();

                    //로그
                    cm.XmlFileSave(XmlDoc, mc.Name, "SearchFareAvailRS", "N", String.Format("{0}-{1}", GUID, ROT));

                    //DB저장(비동기호출)
                    dgDevDBSave dg = new dgDevDBSave(sch.DevDBSave);
                    AsyncCallback ac = new AsyncCallback(DevDBSaveCallBack);
                    IAsyncResult ar = dg.BeginInvoke(SNM, GUID, "Y", ReqInfo, S3Idx, XmlDoc.DocumentElement.OuterXml, ac, dg);
                }

                return XmlDoc.DocumentElement;
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, GUID, "AirService3", MethodBase.GetCurrentMethod().Name, ServiceNumber, 0, 0).ToErrors;
            }
        }

        public void DevDBSaveCallBack(IAsyncResult ar)
        {
            dgDevDBSave dg = (dgDevDBSave)ar.AsyncState;
            Int64 DevDBSaveResult = dg.EndInvoke(ar);

            are.Set();
        }

        #endregion "Fare + Availability 동시조회(통합)"

        #region "Fare + Availability 동시조회(아마데우스)"

        /// <summary>
        /// MP Instant Search(MPIS)(아마데우스)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="XAC">제외 항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드(여정구분은 콤마, SEG구분은 슬래시, ex:NRT/SIN,SIN/NRT,RON)</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight nosp allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PTC">운임타입(ADT:성인운임, STU:학생운임, LBR:노무자운임, 공백:전체)(멀티 선택시 콤마로 구분)</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <param name="PUB">PUB요금 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="LTD">발권마감일 체크여부</param>
        /// <param name="FTR">필터링 사용여부(발권마감일 체크 포함)</param>
        /// <param name="MTL">ModeTL 적용여부</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="PromXml">프로모션정보</param>
        /// <returns></returns>
        public XmlElement SearchFareAvailAmadeusRS(int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID, XmlElement PromXml)
        {
            try
            {
                ////MPIS 사용가능 여부
                //bool MPIS = false;

                ////모두닷컴만 가능
                //if (SNM.Equals(2) || SNM.Equals(3915))
                //{
                //    //다음 조건은 없어야 함
                //    if (String.IsNullOrWhiteSpace(String.Concat(SAC, CLC, FLD, ACQ, FAB)))
                //    {
                //        //M클래스만 가능
                //        if (CCD.Equals("M"))
                //        {
                //            //성인요금으로 성인1명 또는 성인2명 조회시만 가능
                //            if (PTC.Equals("ADT") && CHC.Equals(0) && IFC.Equals(0) && ADC < 3)
                //            {
                //                //편도/왕복만 가능
                //                if (ROT.Equals("OW") || ROT.Equals("RT"))
                //                {
                //                    //미오픈만 가능
                //                    if (OPN.Equals("N"))
                //                    {
                //                        //출발지는 서울(SEL)에 한해서만 가능
                //                        if (DLC.Equals("SEL"))
                //                        {
                //                            //도착지가 다음 지역에 한해서만 가능
                //                            if ("/AKL/ALA/AMS/ATH/ATL/BCN/BER/BJS/BKI/BKK/BNE/BUD/CAN/CEB/CGQ/CHI/CNX/CPH/CRK/CTU/DAD/DEL/DLC/DPS/DXB/FRA/FUK/GUM/GVA/HAN/HEL/HGH/HKG/HKT/HND/HNL/HPH/HRB/HSG/IST/JKT/KHV/KLO/KMG/KMJ/KTM/KUL/LAS/LAX/LON/MAD/MDG/MEL/MFM/MIL/MNL/MOW/MUC/NGO/NHA/NKG/NRT/NYC/OIT/OKA/OSA/PAR/PNH/PRG/RGN/ROM/ROR/SEA/SFO/SGN/SHA/SHE/SIA/SIN/SPK/SPN/SYD/SZX/TAO/TPE/TSN/ULN/VIE/VTE/VVO/WAS/WAW/WEH/XMN/YNJ/YNT/YTO/YVR/ZAG/ZRH/".IndexOf(String.Format("/{0}/", ALC)) != -1)
                //                            {
                //                                DateTime NowDate = DateTime.Now;
                //                                int NowHour = NowDate.Hour;

                //                                //업무일(01~19시), 비업무일(01시~15시)까지만 가능
                //                                if (cm.WorkdayYN(NowDate.ToString("yyyy-MM-dd")))
                //                                {
                //                                    if (NowHour >= 1 && NowHour < 19)
                //                                        MPIS = true;
                //                                }
                //                                else
                //                                {
                //                                    if (NowHour >= 1 && NowHour < 15)
                //                                        MPIS = true;
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                //if (MPIS)
                //    return InstantSearchAmadeusRS(SNM, SAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, FAB, PTC, ADC, CHC, IFC, NRR, PUB, WLR, LTD, FTR, MTL, GUID, PromXml);
                //else
                    return MasterPricerSearchAmadeusRS(SNM, SAC, XAC, DLC, ALC, CLC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, FAB, PTC, ADC, CHC, IFC, NRR, PUB, WLR, LTD, FTR, MTL, GUID, PromXml);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// MP Instant Search(MPIS)(아마데우스)
        /// </summary>
        public XmlElement InstantSearchAmadeusRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID, XmlElement PromXml)
        {
            try
            {
                string StepGUID = String.Format("{0}-{1}-{2}-{3}-{4}", GUID, ROT, PTC, CCD, SAC);

                Stopwatch sw; //실행시간 체크************
                XmlNode NewRunTime = RunTime.CloneNode(false); //실행시간 체크************
                XmlAttribute RunTimeGuid = NewRunTime.OwnerDocument.CreateAttribute("guid"); //실행시간 체크************
                RunTimeGuid.InnerText = String.Format("{0}/{1}/{2}", PTC, CCD, SAC); //실행시간 체크************
                NewRunTime.Attributes.Append(RunTimeGuid); //실행시간 체크************

                sw = Stopwatch.StartNew(); //실행시간 체크************
                
                //GDS XML
                XmlElement XmlGDS = amd.InstantTravelBoardSearchRS(StepGUID, SNM, SAC, DLC, ALC, "", ROT, DTD, ARD, OPN, "", CCD, new String[3] { PTC, "CHD", "INF" }, new Int32[3] { ADC, CHC, IFC }, ACQ, "", PUB, WLR, MTL, NRR);

                CheckRunTimeEnd(NewRunTime, sw, "InstantTravelBoardSearchRS"); //실행시간 체크************
                
                //네임스페이스 정의
                XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlGDS.OwnerDocument.NameTable);
                xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_InstantTravelBoardSearch"));

                //오류 결과일 경우 예외 처리
                if (XmlGDS.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                {
                    if (XmlGDS.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                        throw new Exception(XmlGDS.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                    else
                        throw new Exception(XmlGDS.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                }

                XmlElement ModeXml = ToModeSearchFareAvailGroupingAmadeusRS(SNM, XmlGDS, PromXml, xnMgr, SAC, DLC, ALC, ROT, OPN, CCD, DTD, PUB, LTD, FTR, "0D", "MPIS", StepGUID, NewRunTime);

                //ModeXml.SelectSingleNode("runTime").AppendChild(ModeXml.OwnerDocument.ImportNode(NewRunTime.SelectSingleNode("service"), true)); //실행시간 체크************
                ModeXml.InsertBefore(ModeXml.OwnerDocument.ImportNode(NewRunTime, true), ModeXml.FirstChild); //실행시간 체크************

                return ModeXml;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Fare + Availability 동시조회(상세 조건 조회)(아마데우스)
        /// </summary>
        public XmlElement MasterPricerSearchAmadeusRS(int SNM, string SAC, string XAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID, XmlElement PromXml)
        {
            try
            {
                string StepGUID = String.Format("{0}-{1}-{2}-{3}-{4}", GUID, ROT, PTC, CCD, SAC);
                XmlElement XmlGDS = null;

                Stopwatch sw; //실행시간 체크************
                XmlNode NewRunTime = RunTime.CloneNode(false); //실행시간 체크************
                XmlAttribute RunTimeGuid = NewRunTime.OwnerDocument.CreateAttribute("guid"); //실행시간 체크************
                RunTimeGuid.InnerText = String.Format("{0}/{1}/{2}", PTC, CCD, SAC); //실행시간 체크************
                NewRunTime.Attributes.Append(RunTimeGuid); //실행시간 체크************

                #region "세션풀 사용"

                //string SID = string.Empty;
                //string SQN = string.Empty;
                //string SCT = string.Empty;

                //try
                //{
                //    sw = Stopwatch.StartNew();
                //    XmlElement Session = amd.SessionCreate(SNM, String.Format("{0}-{1}-{2}-01", GUID, PTC[0], CCD));
                //    CheckRunTimeEnd(NewRunTime, sw, "SessionCreate");

                //    SID = Session.SelectSingleNode("session/sessionId").InnerText;
                //    SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
                //    SCT = Session.SelectSingleNode("session/securityToken").InnerText;

                //    sw = Stopwatch.StartNew();
                //    XmlGDS = amd.MasterPricerTravelBoardSearchRS(SID, SQN, SCT, String.Format("{0}-{1}-{2}-02", GUID, PTC[0], CCD), SNM, SAC, DLC, ALC, "", ROT, DTD, ARD, OPN, FLD, CCD, new String[3] { PTC, "CHD", "INF" }, new Int32[3] { ADC, CHC, IFC }, ACQ, FAB, PUB, WLR, MTL, NRR);
                //    CheckRunTimeEnd(NewRunTime, sw, "MasterPricerTravelBoardSearchRS");

                //    //sw = Stopwatch.StartNew();
                //    //XmlElement ReqXml = amd.MasterPricerTravelBoardSearchRQ(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, PUB, WLR, MTL, NRR);
                //    //CheckRunTimeEnd(NewRunTime, sw, "MasterPricerTravelBoardSearchRQ");
                //    //cm.XmlFileSave(ReqXml, "Amadeus", "MasterPricerTravelBoardSearchRQ", "N", String.Format("{0}-{1}-{2}-02", GUID, PTC[0], CCD));

                //    //sw = Stopwatch.StartNew();
                //    //XmlElement ResXml = amd.MasterPricerTravelBoardSearchRSOnly(SID, SQN, SCT, ReqXml);
                //    //CheckRunTimeEnd(NewRunTime, sw, "MasterPricerTravelBoardSearchRS");
                //    //cm.XmlFileSave(ResXml, "Amadeus", "MasterPricerTravelBoardSearchRS", "N", String.Format("{0}-{1}-{2}-02", GUID, PTC[0], CCD));
                //}
                //catch (Exception ex)
                //{
                //    throw new Exception(ex.ToString());
                //}
                //finally
                //{
                //    if (!String.IsNullOrWhiteSpace(SID))
                //    {
                //        sw = Stopwatch.StartNew();
                //        amd.SessionClose(SID, SCT, String.Format("{0}-{1}-{2}-03", GUID, PTC[0], CCD));
                //        CheckRunTimeEnd(NewRunTime, sw, "SessionClose");
                //    }
                //}

                #endregion "세션풀 사용"

                #region "세션풀 미사용"

                try
                {
                    sw = Stopwatch.StartNew(); //실행시간 체크************

                    //GDS XML
                    XmlGDS = amd.MasterPricerTravelBoardSearch4RS(StepGUID, SNM, SAC, XAC, DLC, ALC, "", ROT, DTD, ARD, OPN, FLD, CCD, new String[3] { PTC, "CHD", "INF" }, new Int32[3] { ADC, CHC, IFC }, ACQ, FAB, PUB, WLR, MTL, NRR);
                    CheckRunTimeEnd(NewRunTime, sw, "MasterPricerTravelBoardSearch4RS"); //실행시간 체크************
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }

                #endregion "세션풀 미사용"

                //네임스페이스 정의
                XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlGDS.OwnerDocument.NameTable);
                xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch"));

                //오류 결과일 경우 예외 처리
                if (XmlGDS.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                {
                    if (XmlGDS.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                        throw new Exception(XmlGDS.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                    else
                        throw new Exception(XmlGDS.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                }

                //MaxStay(유효기간) 추가
                XmlAttribute MaxStay;
                string DepartureDate = cm.RequestDateTime(DTD.Split(',')[0].Trim());

                foreach (XmlNode Value in XmlGDS.SelectNodes("m:value", xnMgr))
                {
                    MaxStay = Value.Attributes.Append(XmlGDS.OwnerDocument.CreateAttribute("maxStay"));
                    MaxStay.InnerText = Common.ConvertToMaxStay(DepartureDate, Value.SelectSingleNode("m:criteriaDetails/m:value", xnMgr).InnerText);
                }

                XmlElement ModeXml = ToModeSearchFareAvailGroupingAmadeusRS(SNM, XmlGDS, PromXml, xnMgr, SAC, DLC, ALC, ROT, OPN, CCD, DepartureDate, PUB, LTD, FTR, "", "", StepGUID, NewRunTime);

                //ModeXml.SelectSingleNode("runTime").AppendChild(ModeXml.OwnerDocument.ImportNode(NewRunTime.SelectSingleNode("service"), true)); //실행시간 체크************
                ModeXml.InsertBefore(ModeXml.OwnerDocument.ImportNode(NewRunTime, true), ModeXml.FirstChild); //실행시간 체크************

                return ModeXml;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion "Fare + Availability 동시조회(아마데우스)"

        #region "Fare + Availability 동시조회결과(아마데우스)"

        public XmlElement ToModeSearchFareAvailGroupingAmadeusRS(int SNM, XmlElement ResXml, XmlElement PromXml, XmlNamespaceManager xnMgr, string SAC, string DLC, string ALC, string ROT, string OPN, string CCD, string DTD, string PUB, string LTD, string FTR, string UMaxStay, string MODE, string GUID, XmlNode NewRunTime)
        {
            //ToMode 데이타
            Stopwatch sw = Stopwatch.StartNew(); //실행시간 체크************
            XmlElement TMSA = ToModeSearchFareAvailAmadeusRS(SNM, ResXml, PromXml, xnMgr, SAC, DLC, ALC, ROT, OPN, CCD, DTD, PUB, LTD, FTR, UMaxStay, MODE, GUID);
            CheckRunTimeEnd(NewRunTime, sw, "ToModeSearchFareAvailAmadeusRS"); //실행시간 체크************
            XmlNode TMFlightInfo = TMSA.SelectSingleNode("flightInfo");
            XmlNode TMPriceInfo = TMSA.SelectSingleNode("priceInfo");
            
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("SearchFareAvailGrouping3RS"));

            XmlNode ResponseDetails = XmlDoc.SelectSingleNode("ResponseDetails");
            XmlNode FlightInfo = ResponseDetails.SelectSingleNode("flightInfo");
            XmlNode PriceInfo = ResponseDetails.SelectSingleNode("priceInfo");

            ResponseDetails.Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
            ResponseDetails.Attributes.GetNamedItem("guid").InnerText = TMSA.Attributes.GetNamedItem("guid").InnerText;
            ResponseDetails.Attributes.GetNamedItem("ref").InnerText = TMSA.Attributes.GetNamedItem("ref").InnerText;

            #region "여정"

            sw = Stopwatch.StartNew(); //실행시간 체크************
            ResponseDetails.ReplaceChild(XmlDoc.ImportNode(TMSA.SelectSingleNode("flightInfo"), true), FlightInfo);
            CheckRunTimeEnd(NewRunTime, sw, "FlightInfoGrouping"); //실행시간 체크************

            #endregion "여정"

            #region "요금"

            sw = Stopwatch.StartNew(); //실행시간 체크************

            XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");
            XmlNode SegGroup = PriceIndex.SelectSingleNode("segGroup");
            XmlNode Seg = SegGroup.SelectSingleNode("seg");
            XmlNode SegRef = Seg.SelectSingleNode("ref");

            Seg.RemoveAll();

            #region "방법1 : 첫번째 스케쥴 기준"

            //여정수 셋팅
            //for (int i = 1; i < TMPriceInfo.SelectNodes("priceIndex[1]/segGroup[1]/seg[1]/ref").Count; i++)
            //    SegGroup.AppendChild(Seg.CloneNode(false));

            //XmlNode NewSegGroup;
            //XmlNodeList NewSeg;
            //XmlNode NewSegRef;

            //XmlNode PaxFareInfo = PriceIndex.SelectSingleNode("paxFareInfo");
            //XmlNode NewPriceIndex;
            //XmlNode NewPaxFareInfo;

            //string PTC = TMPriceInfo.SelectSingleNode("priceIndex").Attributes.GetNamedItem("ptc").InnerText;
            //int pRef = 1;

            //foreach (XmlNode TMDepSeg in TMFlightInfo.SelectNodes("segGroup[@fiRef='1']"))
            //{
            //    NewPriceIndex = PriceInfo.AppendChild(PriceIndex.CloneNode(false));
            //    NewSegGroup = NewPriceIndex.AppendChild(SegGroup.CloneNode(true));
            //    NewPaxFareInfo = NewPriceIndex.AppendChild(PaxFareInfo.CloneNode(false));

            //    NewSeg = NewSegGroup.SelectNodes("seg");

            //    foreach (XmlNode TMSeg in TMPriceInfo.SelectNodes(String.Format("priceIndex/segGroup/seg[ref[@fiRef='1']/@sgRef='{0}']", TMDepSeg.Attributes.GetNamedItem("ref").InnerText)))
            //    {
            //        XmlNodeList TMRef = TMSeg.SelectNodes("ref");
                    
            //        for (int i = 0; i < TMRef.Count; i++)
            //        {
            //            if (NewSeg[i].SelectNodes(String.Format("ref[@sgRef='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText)).Count.Equals(0))
            //            {
            //                XmlNode TMSegGroup = TMFlightInfo.SelectSingleNode(String.Format("segGroup[@ref='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText));

            //                NewSegRef = NewSeg[i].AppendChild(SegRef.CloneNode(false));
            //                NewSegRef.Attributes.GetNamedItem("sgRef").InnerText = TMSegGroup.Attributes.GetNamedItem("ref").InnerText;
            //                NewSegRef.Attributes.GetNamedItem("rtg").InnerText = TMSegGroup.Attributes.GetNamedItem("rtg").InnerText;
            //                NewSegRef.Attributes.GetNamedItem("baggage").InnerText = TMRef[i].Attributes.GetNamedItem("baggage").InnerText;
            //                NewSegRef.InnerText = TMSegGroup.Attributes.GetNamedItem("iti").InnerText;
            //            }
            //        }

            //        if (NewPaxFareInfo.SelectNodes(String.Format("paxFareGroup[@ref='{0}']", TMSeg.SelectSingleNode("../../paxFareGroup").Attributes.GetNamedItem("ref").InnerText)).Count.Equals(0))
            //            NewPaxFareInfo.AppendChild(XmlDoc.ImportNode(TMSeg.SelectSingleNode("../../paxFareGroup"), true));
            //    }

            //    NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = (pRef++).ToString();
            //    NewPriceIndex.Attributes.GetNamedItem("ptc").InnerText = PTC;
            //    NewPriceIndex.Attributes.GetNamedItem("cabin").InnerText = NewPaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("cabin").InnerText;
            //    NewPriceIndex.Attributes.GetNamedItem("minPrice").InnerText = (NewPriceIndex.SelectNodes("paxFareInfo/paxFareGroup").Count > 1) ? NewPriceIndex.SelectSingleNode("paxFareInfo/paxFareGroup/paxFare[@ptc='ADT'][@price<=preceding::paxFare[@ptc='ADT']/@price]").Attributes.GetNamedItem("price").InnerText : NewPriceIndex.SelectSingleNode("paxFareInfo/paxFareGroup/paxFare").Attributes.GetNamedItem("price").InnerText;
            //    NewPriceIndex.Attributes.GetNamedItem("pvc").InnerText = NewPaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("pvc").InnerText;
            //}

            #endregion

            #region "방법2 : 정확한 스케쥴 + 운임"

            ////여정리스트 셋팅
            //for (int i = 1; i < TMPriceInfo.SelectNodes("priceIndex[1]/segGroup[1]/seg[1]/ref").Count; i++)
            //    SegGroup.AppendChild(Seg.CloneNode(false));

            //XmlNode NewSegGroup;
            //XmlNodeList NewSeg;
            //XmlNode NewSegRef;

            //XmlNode PaxFareInfo = PriceIndex.SelectSingleNode("paxFareInfo");
            //XmlNode NewPriceIndex;
            //XmlNode NewPaxFareInfo;

            //string PTC = TMPriceInfo.SelectSingleNode("priceIndex").Attributes.GetNamedItem("ptc").InnerText;
            //int pRef = 1;

            //foreach (XmlNode TMSegOnly in TMPriceInfo.SelectNodes("priceIndex/segGroup/seg[not(@ref=preceding::seg/@ref)]"))
            //{
            //    NewPriceIndex = PriceInfo.AppendChild(PriceIndex.CloneNode(false));
            //    NewSegGroup = NewPriceIndex.AppendChild(SegGroup.CloneNode(true));
            //    NewPaxFareInfo = NewPriceIndex.AppendChild(PaxFareInfo.CloneNode(false));

            //    NewSeg = NewSegGroup.SelectNodes("seg");

            //    foreach (XmlNode TMSeg in TMPriceInfo.SelectNodes(String.Format("priceIndex/segGroup/seg[@ref='{0}']", TMSegOnly.Attributes.GetNamedItem("ref").InnerText)))
            //    {
            //        XmlNodeList TMRef = TMSeg.SelectNodes("ref");

            //        for (int i = 0; i < TMRef.Count; i++)
            //        {
            //            if (NewSeg[i].SelectNodes(String.Format("ref[@sgRef='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText)).Count.Equals(0))
            //            {
            //                XmlNode TMSegGroup = TMFlightInfo.SelectSingleNode(String.Format("segGroup[@ref='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText));

            //                NewSegRef = NewSeg[i].AppendChild(SegRef.CloneNode(false));
            //                NewSegRef.Attributes.GetNamedItem("sgRef").InnerText = TMSegGroup.Attributes.GetNamedItem("ref").InnerText;
            //                NewSegRef.Attributes.GetNamedItem("rtg").InnerText = TMSegGroup.Attributes.GetNamedItem("rtg").InnerText;
            //                NewSegRef.Attributes.GetNamedItem("baggage").InnerText = TMRef[i].Attributes.GetNamedItem("baggage").InnerText;
            //                NewSegRef.InnerText = TMSegGroup.Attributes.GetNamedItem("iti").InnerText;
            //            }
            //        }

            //        NewPaxFareInfo.AppendChild(XmlDoc.ImportNode(TMSeg.SelectSingleNode("../../paxFareGroup"), true));
            //    }

            //    NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = (pRef++).ToString();
            //    NewPriceIndex.Attributes.GetNamedItem("ptc").InnerText = PTC;
            //    NewPriceIndex.Attributes.GetNamedItem("cabin").InnerText = NewPaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("cabin").InnerText;
            //    NewPriceIndex.Attributes.GetNamedItem("minPrice").InnerText = (NewPriceIndex.SelectNodes("paxFareInfo/paxFareGroup").Count > 1) ? NewPriceIndex.SelectSingleNode("paxFareInfo/paxFareGroup/paxFare[@ptc='ADT'][@price<=preceding::paxFare[@ptc='ADT']/@price]").Attributes.GetNamedItem("price").InnerText : NewPriceIndex.SelectSingleNode("paxFareInfo/paxFareGroup/paxFare").Attributes.GetNamedItem("price").InnerText;
            //    NewPriceIndex.Attributes.GetNamedItem("pvc").InnerText = NewPaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("pvc").InnerText;
            //}

            #endregion

            #region "방법3 : 운임 기준 동일 스케쥴(PC용)"

            //여정리스트 셋팅
            for (int i = 1; i < TMPriceInfo.SelectNodes("priceIndex[1]/segGroup[1]/seg[1]/ref").Count; i++)
                SegGroup.AppendChild(Seg.CloneNode(false));

            XmlNode NewSegGroup;
            XmlNodeList NewSeg;
            XmlNode NewSegRef;

            XmlNode ClonePriceIndex;
            XmlNode ClonePaxFareInfo;
            XmlNode NewPriceIndex;

            string PTC = TMPriceInfo.SelectSingleNode("priceIndex").Attributes.GetNamedItem("ptc").InnerText;
            string PVC = string.Empty;
            string Cabin = string.Empty;
            string MinPrice = string.Empty;
            int pRef = 1;

            foreach (XmlNode TMPriceIndex in TMPriceInfo.SelectNodes("priceIndex"))
            {
                ClonePriceIndex = PriceIndex.CloneNode(true);
                ClonePaxFareInfo = ClonePriceIndex.SelectSingleNode("paxFareInfo");

                //운임
                foreach (XmlNode PaxFareGroup in TMPriceIndex.SelectNodes("paxFareGroup"))
                    ClonePaxFareInfo.AppendChild(XmlDoc.ImportNode(PaxFareGroup, true));

                PVC = ClonePaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("pvc").InnerText;
                Cabin = ClonePaxFareInfo.SelectSingleNode("paxFareGroup[1]/summary").Attributes.GetNamedItem("cabin").InnerText;
                MinPrice = (ClonePaxFareInfo.SelectNodes("paxFareGroup").Count > 1) ? ClonePaxFareInfo.SelectSingleNode("paxFareGroup[not(@adtPrice>preceding-sibling::paxFareGroup/@adtPrice) and not(@adtPrice>following-sibling::paxFareGroup/@adtPrice)]").Attributes.GetNamedItem("adtPrice").InnerText : ClonePaxFareInfo.SelectSingleNode("paxFareGroup").Attributes.GetNamedItem("adtPrice").InnerText;
                ClonePriceIndex.RemoveChild(ClonePriceIndex.SelectSingleNode("segGroup"));

                //여정별 운임
                foreach (XmlNode TMDepSeg in TMPriceIndex.SelectNodes("segGroup/seg/ref[@fiRef='1'][not(@sgRef=preceding-sibling::ref[@fiRef='1']/@sgRef)]"))
                {
                    NewPriceIndex = PriceInfo.AppendChild(ClonePriceIndex.CloneNode(true));
                    NewSegGroup = NewPriceIndex.InsertBefore(SegGroup.CloneNode(true), NewPriceIndex.FirstChild);
                    NewSeg = NewSegGroup.SelectNodes("seg");

                    foreach (XmlNode TMSeg in TMPriceIndex.SelectNodes(String.Format("segGroup/seg[ref[@fiRef='1']/@sgRef='{0}']", TMDepSeg.Attributes.GetNamedItem("sgRef").InnerText)))
                    {
                        XmlNodeList TMRef = TMSeg.SelectNodes("ref");

                        for (int i = 0; i < TMRef.Count; i++)
                        {
                            if (NewSeg[i].SelectNodes(String.Format("ref[@sgRef='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText)).Count.Equals(0))
                            {
                                XmlNode TMSegGroup = TMFlightInfo.SelectSingleNode(String.Format("segGroup[@ref='{0}']", TMRef[i].Attributes.GetNamedItem("sgRef").InnerText));

                                NewSegRef = NewSeg[i].AppendChild(SegRef.CloneNode(false));
                                NewSegRef.Attributes.GetNamedItem("sgRef").InnerText = TMSegGroup.Attributes.GetNamedItem("ref").InnerText;
                                NewSegRef.Attributes.GetNamedItem("rtg").InnerText = TMSegGroup.Attributes.GetNamedItem("rtg").InnerText;
                                NewSegRef.Attributes.GetNamedItem("baggage").InnerText = TMRef[i].Attributes.GetNamedItem("baggage").InnerText;
                                NewSegRef.InnerText = TMSegGroup.Attributes.GetNamedItem("iti").InnerText;
                            }
                        }
                    }

                    NewPriceIndex.Attributes.GetNamedItem("ref").InnerText = (pRef++).ToString();
                    NewPriceIndex.Attributes.GetNamedItem("ptc").InnerText = PTC;
                    NewPriceIndex.Attributes.GetNamedItem("cabin").InnerText = Cabin;
                    NewPriceIndex.Attributes.GetNamedItem("minPrice").InnerText = MinPrice;
                    NewPriceIndex.Attributes.GetNamedItem("pvc").InnerText = PVC;
                    NewPriceIndex.Attributes.GetNamedItem("fareCount").InnerText = NewPriceIndex.SelectNodes("paxFareInfo/paxFareGroup").Count.ToString();
                }
            }

            #endregion

            PriceInfo.RemoveChild(PriceIndex);

            CheckRunTimeEnd(NewRunTime, sw, "PriceInfoGrouping"); //실행시간 체크************

            #endregion "요금"

            //로그
            cm.XmlFileSave(XmlDoc, mc.Name, "ToModeSearchFareAvailGroupingAmadeusRS", "N", GUID);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// SearchFareAvailAmadeusRS를 통합용 XML구조로 변경
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="ResXml">SearchFareAvailRS의 Data</param>
        /// <param name="PromXml">SearchPromotionList의 Data</param>
        /// <param name="xnMgr">XmlNamespaceManager</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="PUB">PUB요금 출력여부</param>
        /// <param name="LTD">발권마감일 체크여부</param>
        /// <param name="FTR">필터링 사용여부(발권마감일 체크 포함)</param>
        /// <param name="UMaxStay">유효기간 정보(MPIS 등 고정 처리시)</param>
        /// <param name="MODE">검색방식(MPIS 등)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public XmlElement ToModeSearchFareAvailAmadeusRS(int SNM, XmlElement ResXml, XmlElement PromXml, XmlNamespaceManager xnMgr, string SAC, string DLC, string ALC, string ROT, string OPN, string CCD, string DTD, string PUB, string LTD, string FTR, string UMaxStay, string MODE, string GUID)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(mc.XmlFullPath("SearchFareAvail3RS"));

            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText = ((XmlAttribute)ResXml.Attributes.GetNamedItem("ref") != null) ? ResXml.Attributes.GetNamedItem("ref").InnerText : "";
            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("guid").InnerText = GUID;

            XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
            XmlNode SegmentGroup = FlightInfo.SelectSingleNode("segGroup");
            XmlNode Segment = SegmentGroup.SelectSingleNode("seg");
            XmlNode StopSegment = Segment.SelectSingleNode("seg");

            XmlNode NewSegmentGroup;
            XmlNode NewSegment;
            XmlNode NewStopSegment1;
            XmlNode NewStopSegment2;

            CCD = String.IsNullOrWhiteSpace(CCD) ? ResXml.SelectSingleNode("m:recommendation[1]/m:paxFareProduct[1]/m:fareDetails[1]/m:majCabin/m:bookingClassDetails/m:designator", xnMgr).InnerText : CCD;

            bool MPIS = MODE.Equals("MPIS") ? true : false;
            bool Train = false;
            string PTC = Common.ChangePaxType1(ResXml.SelectSingleNode("m:recommendation[1]/m:paxFareProduct[1]/m:paxReference[1]/m:ptc", xnMgr).InnerText);
            string MCC = string.Empty;
            string OCC = string.Empty;
            string FlightSegRef = string.Empty;
            string BasicRef = Common.RefOverrideBasic("A", ((String.IsNullOrWhiteSpace(SAC) || SAC.IndexOf(',') != -1) ? "00" : SAC), PTC, CCD);
            string PrevARDT = string.Empty;
            string Itinerary = string.Empty;
            int PriceRef = 1;
            int SegmentRef = 1;
            int SegCount = 0;
            int StopoverCount = 0;
            int ChinaStopoverCount = 0;
            
            FlightInfo.Attributes.GetNamedItem("rot").InnerText = ROT;
            FlightInfo.Attributes.GetNamedItem("opn").InnerText = OPN;

            #region "여정"

            foreach (XmlNode flightIndex in ResXml.SelectNodes("m:flightIndex", xnMgr))
            {
                FlightSegRef = flightIndex.SelectSingleNode("m:requestedSegmentRef/m:segRef", xnMgr).InnerText;

                foreach (XmlNode groupOfFlights in flightIndex.SelectNodes("m:groupOfFlights", xnMgr))
                {
                    Train = false;
                    Itinerary = "";
                    SegCount = groupOfFlights.SelectNodes("m:flightDetails", xnMgr).Count;
                    StopoverCount = 0;
                    ChinaStopoverCount = 0;
                    SegmentRef = 1;

                    NewSegmentGroup = FlightInfo.AppendChild(SegmentGroup.CloneNode(false));

                    foreach (XmlNode flightDetails in groupOfFlights.SelectNodes("m:flightDetails", xnMgr))
                    {
                        XmlNode FlightInformation = flightDetails.SelectSingleNode("m:flightInformation", xnMgr);
                        
                        //항공이 아닌 다른 운송수단(철도 등)이 포함된 경우
                        if (FlightInformation.SelectNodes("m:productDetail/m:equipmentType", xnMgr).Count > 0 && "/BUS/TGV/THL/THS/TRS/MTL/THT/TRN/TSL/ICE/LCH/".IndexOf(FlightInformation.SelectSingleNode("m:productDetail/m:equipmentType", xnMgr).InnerText) != -1)
                        {
                            Train = true;
                            break;
                        }

                        //특정항공사 제외
                        MCC = FlightInformation.SelectSingleNode("m:companyId/m:marketingCarrier", xnMgr).InnerText;
                        OCC = (FlightInformation.SelectNodes("m:companyId/m:operatingCarrier", xnMgr).Count > 0) ? FlightInformation.SelectSingleNode("m:companyId/m:operatingCarrier", xnMgr).InnerText : "";

                        if (Common.ScheduleExceptionHandling(MCC, OCC))
                        {
                            Train = true;
                            break;
                        }

                        NewSegment = NewSegmentGroup.AppendChild(Segment.CloneNode(false));
                        NewSegment.Attributes.GetNamedItem("dlc").InnerText = FlightInformation.SelectSingleNode("m:location[1]/m:locationId", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("alc").InnerText = FlightInformation.SelectSingleNode("m:location[2]/m:locationId", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("ddt").InnerText = (FlightInformation.SelectNodes("m:productDateTime/m:timeOfDeparture", xnMgr).Count > 0) ? cm.ConvertToDateTime(FlightInformation.SelectSingleNode("m:productDateTime/m:dateOfDeparture", xnMgr).InnerText, FlightInformation.SelectSingleNode("m:productDateTime/m:timeOfDeparture", xnMgr).InnerText) : cm.RequestDateTime(FlightInformation.SelectSingleNode("m:productDateTime/m:dateOfDeparture", xnMgr).InnerText);
                        NewSegment.Attributes.GetNamedItem("ardt").InnerText = (FlightInformation.SelectNodes("m:productDateTime/m:timeOfArrival", xnMgr).Count > 0) ? cm.ConvertToDateTime(FlightInformation.SelectSingleNode("m:productDateTime/m:dateOfArrival", xnMgr).InnerText, FlightInformation.SelectSingleNode("m:productDateTime/m:timeOfArrival", xnMgr).InnerText) : cm.RequestDateTime(FlightInformation.SelectSingleNode("m:productDateTime/m:dateOfArrival", xnMgr).InnerText);
                        NewSegment.Attributes.GetNamedItem("eft").InnerText = (FlightInformation.SelectNodes("m:attributeDetails[m:attributeType='EFT']/m:attributeDescription", xnMgr).Count > 0) ? FlightInformation.SelectSingleNode("m:attributeDetails[m:attributeType='EFT']/m:attributeDescription", xnMgr).InnerText : "";
                        NewSegment.Attributes.GetNamedItem("mcc").InnerText = MCC;
                        NewSegment.Attributes.GetNamedItem("occ").InnerText = OCC;
                        NewSegment.Attributes.GetNamedItem("cds").InnerText = MCC.Equals(OCC) ? "N" : "Y";
                        NewSegment.Attributes.GetNamedItem("fln").InnerText = (FlightInformation.SelectNodes("m:flightOrtrainNumber", xnMgr).Count > 0) ? Common.ZeroPaddingFlight(FlightInformation.SelectSingleNode("m:flightOrtrainNumber", xnMgr).InnerText) : "";
                        NewSegment.Attributes.GetNamedItem("stn").InnerText = (FlightInformation.SelectNodes("m:productDetail/m:techStopNumber", xnMgr).Count > 0) ? cm.RequestInt(FlightInformation.SelectSingleNode("m:productDetail/m:techStopNumber", xnMgr).InnerText).ToString() : "0";
                        NewSegment.Attributes.GetNamedItem("etc").InnerText = FlightInformation.SelectSingleNode("m:addProductDetail/m:electronicTicketing", xnMgr).InnerText;
                        NewSegment.Attributes.GetNamedItem("aif").InnerText = "";

                        //기착은 1회까지만 허용(2018-05-15)
                        if (Convert.ToInt32(NewSegment.Attributes.GetNamedItem("stn").InnerText) > 1)
                        {
                            Train = true;
                            break;
                        }

                        if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                        {
                            XmlNode StopDetailsAA = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AA']", xnMgr);
                            XmlNode StopDetailsAD = flightDetails.SelectSingleNode("m:technicalStop/m:stopDetails[m:dateQualifier='AD']", xnMgr);

                            NewStopSegment1 = NewSegment.AppendChild(StopSegment.CloneNode(false));
                            NewStopSegment1.Attributes.GetNamedItem("group").InnerText = "1";
                            NewStopSegment1.Attributes.GetNamedItem("dlc").InnerText = NewSegment.Attributes.GetNamedItem("dlc").InnerText;
                            NewStopSegment1.Attributes.GetNamedItem("alc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
                            NewStopSegment1.Attributes.GetNamedItem("ddt").InnerText = NewSegment.Attributes.GetNamedItem("ddt").InnerText;
                            NewStopSegment1.Attributes.GetNamedItem("ardt").InnerText = (StopDetailsAA.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAA.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAA.SelectSingleNode("m:date", xnMgr).InnerText);
                            NewStopSegment1.Attributes.GetNamedItem("gwt").InnerText = "";

                            NewStopSegment2 = NewSegment.AppendChild(StopSegment.CloneNode(false));
                            NewStopSegment2.Attributes.GetNamedItem("group").InnerText = "1";
                            NewStopSegment2.Attributes.GetNamedItem("dlc").InnerText = StopDetailsAA.SelectSingleNode("m:locationId", xnMgr).InnerText;
                            NewStopSegment2.Attributes.GetNamedItem("alc").InnerText = NewSegment.Attributes.GetNamedItem("alc").InnerText;
                            NewStopSegment2.Attributes.GetNamedItem("ddt").InnerText = (StopDetailsAD.SelectNodes("m:firstTime", xnMgr).Count > 0) ? cm.ConvertToDateTime(StopDetailsAD.SelectSingleNode("m:date", xnMgr).InnerText, StopDetailsAD.SelectSingleNode("m:firstTime", xnMgr).InnerText) : cm.RequestDateTime(StopDetailsAD.SelectSingleNode("m:date", xnMgr).InnerText);
                            NewStopSegment2.Attributes.GetNamedItem("ardt").InnerText = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
                            NewStopSegment2.Attributes.GetNamedItem("gwt").InnerText = cm.CalWaitingTime(NewStopSegment1.Attributes.GetNamedItem("ardt").InnerText, NewStopSegment2.Attributes.GetNamedItem("ddt").InnerText);

                            //기착지 대기시간
                            NewSegment.Attributes.GetNamedItem("gwt").InnerText = NewStopSegment2.Attributes.GetNamedItem("gwt").InnerText;

                            Itinerary += String.Format("/*{0}/{1}", NewStopSegment2.Attributes.GetNamedItem("dlc").InnerText, NewStopSegment2.Attributes.GetNamedItem("alc").InnerText);
                        }
                        else
                        {
                            Itinerary += String.Concat("/", NewSegment.Attributes.GetNamedItem("alc").InnerText);
                        }

                        //경유지 대기시간
                        if (SegmentRef > 1)
                            NewSegment.Attributes.GetNamedItem("ett").InnerText = cm.CalWaitingTime(PrevARDT, NewSegment.Attributes.GetNamedItem("ddt").InnerText);
                        
                        //중국 2회 경유(기착 포함)인 경우 제외
                        if (SegCount > 1)
                        {
                            if (SegmentRef < SegCount && Common.ChinaOfAirport(NewSegment.Attributes.GetNamedItem("alc").InnerText))
                                ChinaStopoverCount++;

                            //기착지도 체크
                            if (NewSegment.Attributes.GetNamedItem("stn").InnerText.Equals("1"))
                            {
                                if (Common.ChinaOfAirport(NewSegment.SelectSingleNode("seg[1]").Attributes.GetNamedItem("alc").InnerText))
                                    ChinaStopoverCount++;
                            }
                        }

                        //MU(동방)항공은 경유시 첫번째 여정의 마케팅항공사(MCC)가 MU인 경우만 노출
                        //OZ(아시아나),QV(라오스)항공은 모든 여정의 마케팅항공사(MCC)가 OZ,QV인 경우만 노출

                        //경유지 대기시간 계산을 위해 도착일시 임시저장
                        PrevARDT = NewSegment.Attributes.GetNamedItem("ardt").InnerText;
                        StopoverCount += Convert.ToInt32(NewSegment.Attributes.GetNamedItem("stn").InnerText);
                        SegmentRef++;
                    }

                    //항공편이 아닌 경우 및 중국 2회 이상 경유 삭제
                    if (Train || ChinaStopoverCount >= 2)
                        FlightInfo.RemoveChild(NewSegmentGroup);
                    else
                    {
                        XmlNode SegFirst = NewSegmentGroup.FirstChild;
                        XmlNode SegLast = NewSegmentGroup.SelectSingleNode("seg[last()]");
                        
                        NewSegmentGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, FlightSegRef, groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[1]/m:ref", xnMgr).InnerText);
                        NewSegmentGroup.Attributes.GetNamedItem("eft").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[m:unitQualifier='EFT']/m:ref", xnMgr).InnerText;
                        NewSegmentGroup.Attributes.GetNamedItem("ewt").InnerText = cm.SumWaitingTime(NewSegmentGroup);
                        NewSegmentGroup.Attributes.GetNamedItem("ejt").InnerText = cm.SumTime(NewSegmentGroup.Attributes.GetNamedItem("eft").InnerText, NewSegmentGroup.Attributes.GetNamedItem("ewt").InnerText).Replace(":", "");
                        NewSegmentGroup.Attributes.GetNamedItem("mjc").InnerText = groupOfFlights.SelectSingleNode("m:propFlightGrDetail/m:flightProposal[m:unitQualifier='MCX']/m:ref", xnMgr).InnerText;
                        NewSegmentGroup.Attributes.GetNamedItem("cds").InnerText = (NewSegmentGroup.SelectNodes("seg[@cds='Y']").Count > 0) ? "Y" : "N";
                        NewSegmentGroup.Attributes.GetNamedItem("nosp").InnerText = (SegCount - 1).ToString();
                        NewSegmentGroup.Attributes.GetNamedItem("iti").InnerText = String.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/{9}/{10}", SegFirst.Attributes.GetNamedItem("dlc").InnerText, SegLast.Attributes.GetNamedItem("alc").InnerText, Common.ConvertToOnlyNumber(SegFirst.Attributes.GetNamedItem("ddt").InnerText), Common.ConvertToOnlyNumber(SegLast.Attributes.GetNamedItem("ardt").InnerText), NewSegmentGroup.Attributes.GetNamedItem("ejt").InnerText, NewSegmentGroup.Attributes.GetNamedItem("eft").InnerText, NewSegmentGroup.Attributes.GetNamedItem("ewt").InnerText, NewSegmentGroup.Attributes.GetNamedItem("mjc").InnerText, NewSegmentGroup.Attributes.GetNamedItem("nosp").InnerText, StopoverCount, NewSegmentGroup.Attributes.GetNamedItem("cds").InnerText);
                        NewSegmentGroup.Attributes.GetNamedItem("rtg").InnerText = String.Concat(SegFirst.Attributes.GetNamedItem("dlc").InnerText, Itinerary);
                        NewSegmentGroup.Attributes.GetNamedItem("aif").InnerText = "";
                        NewSegmentGroup.Attributes.GetNamedItem("fiRef").InnerText = FlightSegRef;
                    }
                }
            }

            FlightInfo.RemoveChild(SegmentGroup);

            #endregion "여정"

            #region "운임"

            XmlNode PriceInfo = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo");
            XmlNode PriceIndex = PriceInfo.SelectSingleNode("priceIndex");

            SegmentGroup = PriceIndex.SelectSingleNode("segGroup");
            Segment = SegmentGroup.SelectSingleNode("seg");

            XmlNode SegRef = Segment.SelectSingleNode("ref");
            XmlNode SegmentGroup1;
            XmlNode SegmentGroup2;

            XmlNode NewPriceIndex1 = null;
            XmlNode NewPriceIndex2 = null;
            XmlNode NewSegRef;

            //조건설정
            string NowDate = DateTime.Now.ToString("d");
            DateTime ToDate = Convert.ToDateTime(String.Concat(NowDate, cm.TLBasicTime(SNM, "", NowDate)));
            DateTime ModeTL = cm.ModeTL(SNM, "");
            DateTime DepartureDate = Convert.ToDateTime(cm.RequestDateTime(DTD));

            //업무일 여부
            bool WorkingDay = cm.WorkdayYN(NowDate);
            bool CheckChinaAir = (cm.DateDiff("d", ToDate, DepartureDate) < 11 && (!WorkingDay || !cm.WorkdayYN(ToDate.AddDays(1).ToString("d")))) ? true : false;

            //프로모션 정보
            XmlNodeList PromItems = (PromXml != null) ? PromXml.SelectNodes(String.Format("item[paxType='{0}' or paxType='']", PTC)) : null;
            XmlNodeList CSPromItems = (PromXml != null) ? PromXml.SelectNodes(String.Format("item[paxType='{0}' or paxType=''][codeshare='Y']", PTC)) : null;

            //무료수하물
            XmlNode ServiceFeesGrp = (ResXml.SelectNodes("m:serviceFeesGrp", xnMgr).Count > 0) ? ResXml.SelectSingleNode("m:serviceFeesGrp", xnMgr) : null;

            //한국출발여부
            bool DepartureFromKorea = Common.KoreaOfAirport(DLC.Split(',')[0].Trim());

            foreach (XmlNode recommendation in ResXml.SelectNodes(String.Format("m:recommendation[m:paxFareProduct/m:fareDetails/m:groupOfFares/m:productInformation/m:fareProductDetail[m:passengerType='{0}'{1}]]", Common.AmadeusPaxTypeCode(PTC), (PTC).Equals("ADT") ? " or m:passengerType='IT'" : ""), xnMgr))
            {
                //발권항공사
                string ValidatingCarrier = recommendation.SelectSingleNode("m:paxFareProduct/m:paxFareDetail/m:codeShareDetails[m:transportStageQualifier='V']/m:company", xnMgr).InnerText;

                //예외사항처리
                if (Common.FareExceptionHandling(ValidatingCarrier, DLC, ALC, ROT, DTD))
                {
                    //특정항공사의 경우 MSP운임만 출력(PUB운임 삭제)
                    if (Common.ExcludePubFare(ValidatingCarrier, recommendation.SelectSingleNode("m:paxFareProduct", xnMgr), xnMgr, PUB, DepartureFromKorea))
                    {
                        //발권가능 항공사 체크
                        if (Common.AirlineHost("Amadeus", ValidatingCarrier))
                        {
                            //MPIS일 경우 캐빈클래스 및 RP 운임 체크(2017-11-14,고재영)
                            if (Common.MPISFilter(MPIS, CCD, recommendation.SelectSingleNode("m:paxFareProduct", xnMgr), xnMgr))
                            {
                                //발권마감일
                                string TLDate = (recommendation.SelectNodes("m:paxFareProduct/m:fare/m:pricingMessage[m:freeTextQualification/m:textSubjectQualifier='LTD']", xnMgr).Count > 0 && recommendation.SelectNodes("m:paxFareProduct/m:fare/m:pricingMessage[m:freeTextQualification/m:textSubjectQualifier='LTD']/m:description", xnMgr).Count > 1) ? cm.ConvertToDateTime(recommendation.SelectSingleNode("m:paxFareProduct/m:fare/m:pricingMessage[m:freeTextQualification/m:textSubjectQualifier='LTD']/m:description[2]", xnMgr).InnerText) : NowDate;

                                //PUB운임 여부
                                bool RP = (recommendation.SelectNodes("//m:groupOfFares[m:productInformation/m:fareProductDetail/m:fareType='RP']", xnMgr).Count > 0) ? true : false;

                                //발권가능일 체크
                                if (cm.ApplyTLCondition(SNM, ModeTL, Convert.ToDateTime(String.Concat(TLDate, cm.TLBasicTime(SNM, ValidatingCarrier, TLDate))), ValidatingCarrier, CheckChinaAir, WorkingDay, LTD, FTR))
                                {
                                    SegmentGroup1 = SegmentGroup.CloneNode(false);
                                    SegmentGroup2 = SegmentGroup.CloneNode(false);

                                    //여정 정보
                                    foreach (XmlNode segmentFlightRef in recommendation.SelectNodes("m:segmentFlightRef", xnMgr))
                                    {
                                        NewSegment = Segment.CloneNode(false);

                                        int fRef = 1;
                                        string SegCDS = "N";
                                        string SegGroupRef = string.Empty;
                                        string SegGroupInfo = string.Empty;
                                        XmlNode FlightSegGroup;

                                        //무료수하물
                                        XmlNode ServiceCoverageInfoGrp = null;
                                        if (segmentFlightRef.SelectNodes("m:referencingDetail[m:refQualifier='B']", xnMgr).Count > 0)
                                        {
                                            ServiceCoverageInfoGrp = ServiceFeesGrp.SelectSingleNode(String.Format("m:serviceCoverageInfoGrp[m:itemNumberInfo/m:itemNumber/m:number='{0}'][m:serviceCovInfoGrp/m:paxRefInfo/m:travellerDetails/m:referenceNumber='1']", segmentFlightRef.SelectSingleNode("m:referencingDetail[m:refQualifier='B']/m:refNumber", xnMgr).InnerText), xnMgr);
                                        }

                                        foreach (XmlNode refNumber in segmentFlightRef.SelectNodes("m:referencingDetail[m:refQualifier='S']/m:refNumber", xnMgr))
                                        {
                                            NewSegRef = NewSegment.AppendChild(SegRef.CloneNode(false));

                                            SegGroupRef = Common.RefOverrideFull(BasicRef, fRef, refNumber.InnerText);
                                            FlightSegGroup = FlightInfo.SelectSingleNode(String.Format("segGroup[@ref='{0}']", SegGroupRef));

                                            if (FlightSegGroup != null)
                                            {
                                                NewSegRef.Attributes.GetNamedItem("fiRef").InnerText = fRef.ToString();
                                                NewSegRef.Attributes.GetNamedItem("sgRef").InnerText = SegGroupRef;

                                                //무료수하물
                                                if (ServiceCoverageInfoGrp != null)
                                                {
                                                    if (ServiceCoverageInfoGrp.SelectNodes(String.Format("m:serviceCovInfoGrp[m:coveragePerFlightsInfo/m:numberOfItemsDetails[m:referenceQualifier='RS']/m:refNum='{0}']/m:refInfo/m:referencingDetail[m:refQualifier='F']/m:refNumber", NewSegRef.Attributes.GetNamedItem("fiRef").InnerText), xnMgr).Count > 0)
                                                    {
                                                        XmlNode FreeBagAllownceInfo = ServiceFeesGrp.SelectSingleNode(String.Format("m:freeBagAllowanceGrp[m:itemNumberInfo/m:itemNumberDetails/m:number='{0}']/m:freeBagAllownceInfo", ServiceCoverageInfoGrp.SelectSingleNode(String.Format("m:serviceCovInfoGrp[m:coveragePerFlightsInfo/m:numberOfItemsDetails[m:referenceQualifier='RS']/m:refNum='{0}']/m:refInfo/m:referencingDetail[m:refQualifier='F']/m:refNumber", NewSegRef.Attributes.GetNamedItem("fiRef").InnerText), xnMgr).InnerText), xnMgr);
                                                        if (FreeBagAllownceInfo != null)
                                                            NewSegRef.Attributes.GetNamedItem("baggage").InnerText = Common.BaggageEmpty(String.Concat(FreeBagAllownceInfo.SelectSingleNode("m:baggageDetails/m:freeAllowance", xnMgr).InnerText, Common.BaggageUnitCode(FreeBagAllownceInfo.SelectSingleNode("m:baggageDetails/m:quantityCode", xnMgr).InnerText, ((FreeBagAllownceInfo.SelectNodes("m:baggageDetails/m:unitQualifier", xnMgr).Count > 0) ? FreeBagAllownceInfo.SelectSingleNode("m:baggageDetails/m:unitQualifier", xnMgr).InnerText : ""))));
                                                    }
                                                }
                                                
                                                if (SegCDS.Equals("N") && FlightSegGroup.Attributes.GetNamedItem("cds").InnerText.Equals("Y"))
                                                    SegCDS = "Y";

                                                SegGroupInfo += SegGroupRef;
                                            }

                                            fRef++;
                                        }

                                        //모든 여정정보가 존재할 경우에만 운임 출력
                                        if (NewSegment.SelectNodes("ref[@sgRef='']").Count.Equals(0))
                                        {
                                            NewSegment.Attributes.GetNamedItem("ref").InnerText = SegGroupInfo;
                                            (SegCDS.Equals("N") ? SegmentGroup1 : SegmentGroup2).AppendChild(NewSegment);
                                        }
                                    }

                                    //유효기간
                                    if (ResXml.SelectNodes("m:value", xnMgr).Count > 0)
                                    {
                                        XmlAttribute MaxStay;

                                        foreach (XmlNode FareFamiliesRef in recommendation.SelectNodes("//m:fareFamiliesRef[m:referencingDetail/m:refQualifier='M']", xnMgr))
                                        {
                                            MaxStay = FareFamiliesRef.Attributes.Append(ResXml.OwnerDocument.CreateAttribute("maxStay"));
                                            MaxStay.InnerText = ResXml.SelectSingleNode("m:value[m:ref='" + FareFamiliesRef.SelectSingleNode("m:referencingDetail/m:refNumber", xnMgr).InnerText + "']", xnMgr).Attributes.GetNamedItem("maxStay").InnerText;
                                        }
                                    }

                                    //TASF 적용 사용자 선택 가능 여부
                                    string SelectUserTASF = Common.SelectUserTASF(SNM, ValidatingCarrier);

                                    //모두투어 단독운임 여부
                                    bool SF = (recommendation.SelectNodes("//m:groupOfFares[m:productInformation/m:fareProductDetail/m:fareType='SF']", xnMgr).Count > 0) ? true : false;

                                    //프로모션 적용된 운임 존재 여부
                                    bool PromFareYN = false;

                                    //일련번호
                                    int PriceSubRef = 1;

                                    //프로모션 적용
                                    if (SegmentGroup1.HasChildNodes)
                                    {
                                        NewPriceIndex1 = PriceInfo.AppendChild(PriceIndex.CloneNode(true));
                                        NewPriceIndex1.ReplaceChild(SegmentGroup1.Clone(), NewPriceIndex1.SelectSingleNode("segGroup"));

                                        NewPriceIndex1.Attributes.GetNamedItem("gds").InnerText = "Amadeus";
                                        NewPriceIndex1.Attributes.GetNamedItem("mode").InnerText = MODE;
                                        NewPriceIndex1.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                        NewPriceIndex1.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "B", PriceRef++);
                                        NewPriceIndex1.Attributes.GetNamedItem("guid").InnerText = GUID;

                                        XmlNode PaxFareGroup = NewPriceIndex1.SelectSingleNode("paxFareGroup");
                                        XmlNode NewPaxFareGroup;

                                        if (PromItems != null)
                                        {
                                            foreach (XmlNode PromItem in PromItems)
                                            {
                                                string CabinClassItem = (PromItem.SelectSingleNode("cabinClass").InnerText.Equals("Y")) ? "Y,M,W" : PromItem.SelectSingleNode("cabinClass").InnerText;

                                                //프로모션 적용 여부 판단
                                                if (Common.ApplyPromotion(PromItem.SelectSingleNode("airCode").InnerText, PromItem.SelectSingleNode("fareType").InnerText, PromItem.SelectSingleNode("fareBasis").InnerText, CabinClassItem, PromItem.SelectSingleNode("bookingClass").InnerText, PromItem.SelectSingleNode("bookingClassExc").InnerText, PromItem.SelectSingleNode("specialYN").InnerText, recommendation.SelectSingleNode(String.Format("m:paxFareProduct[m:paxReference/m:ptc='{0}']", PTC), xnMgr), xnMgr))
                                                {
                                                    NewPaxFareGroup = NewPriceIndex1.InsertBefore(PaxFareGroup.CloneNode(true), PaxFareGroup);
                                                    SetPriceIndexAmadeus(SNM, NewPaxFareGroup, recommendation, xnMgr, PromItem, "N", DepartureFromKorea, SelectUserTASF, UMaxStay, TLDate);

                                                    //항공운임이 일만원 미만일 경우 삭제
                                                    if (cm.RequestDouble(NewPaxFareGroup.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText) < 10000)
                                                        NewPriceIndex1.RemoveChild(NewPaxFareGroup);
                                                    else
                                                    {
                                                        NewPaxFareGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "B", PriceSubRef++);
                                                        NewPaxFareGroup.Attributes.GetNamedItem("mode").InnerText = MODE;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("adtPrice").InnerText = NewPaxFareGroup.SelectSingleNode("paxFare[@ptc='ADT']").Attributes.GetNamedItem("price").InnerText;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("gds").InnerText = "Amadeus";
                                                        
                                                        if (!PromFareYN)
                                                            PromFareYN = true;
                                                    }
                                                }
                                            }
                                        }

                                        //프로모션 적용전 운임(SF이면서 프로모션이 적용된 운임이 존재할 경우에는 기본운임은 미노출,2018-06-14,방경도팀장)
                                        if (!SF || !PromFareYN)
                                        {
                                            NewPaxFareGroup = NewPriceIndex1.InsertBefore(PaxFareGroup.CloneNode(true), PaxFareGroup);
                                            SetPriceIndexAmadeus(SNM, NewPaxFareGroup, recommendation, xnMgr, null, "N", DepartureFromKorea, SelectUserTASF, UMaxStay, TLDate);

                                            //항공운임이 일만원 미만일 경우 삭제
                                            if (cm.RequestDouble(NewPaxFareGroup.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText) < 10000)
                                                NewPriceIndex1.RemoveChild(NewPaxFareGroup);
                                            else
                                            {
                                                NewPaxFareGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "A", PriceSubRef++);
                                                NewPaxFareGroup.Attributes.GetNamedItem("mode").InnerText = MODE;
                                                NewPaxFareGroup.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                                NewPaxFareGroup.Attributes.GetNamedItem("adtPrice").InnerText = NewPaxFareGroup.SelectSingleNode("paxFare[@ptc='ADT']").Attributes.GetNamedItem("price").InnerText;
                                                NewPaxFareGroup.Attributes.GetNamedItem("gds").InnerText = "Amadeus";
                                            }
                                        }

                                        NewPriceIndex1.RemoveChild(PaxFareGroup);
                                    }

                                    //프로모션 적용(코드쉐어)
                                    if (SegmentGroup2.HasChildNodes)
                                    {
                                        NewPriceIndex2 = PriceInfo.AppendChild(PriceIndex.CloneNode(true));
                                        NewPriceIndex2.ReplaceChild(SegmentGroup2.Clone(), NewPriceIndex2.SelectSingleNode("segGroup"));

                                        NewPriceIndex2.Attributes.GetNamedItem("gds").InnerText = "Amadeus";
                                        NewPriceIndex2.Attributes.GetNamedItem("mode").InnerText = MODE;
                                        NewPriceIndex2.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                        NewPriceIndex2.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "D", PriceRef++);
                                        NewPriceIndex2.Attributes.GetNamedItem("guid").InnerText = GUID;

                                        XmlNode PaxFareGroup = NewPriceIndex2.SelectSingleNode("paxFareGroup");
                                        XmlNode NewPaxFareGroup;

                                        PromFareYN = false;
                                        PriceSubRef = 1;

                                        if (CSPromItems != null)
                                        {
                                            foreach (XmlNode PromItem in CSPromItems)
                                            {
                                                string CabinClassItem = (PromItem.SelectSingleNode("cabinClass").InnerText.Equals("Y")) ? "Y,M,W" : PromItem.SelectSingleNode("cabinClass").InnerText;

                                                //프로모션 적용 여부 판단
                                                if (Common.ApplyPromotion(PromItem.SelectSingleNode("airCode").InnerText, PromItem.SelectSingleNode("fareType").InnerText, PromItem.SelectSingleNode("fareBasis").InnerText, CabinClassItem, PromItem.SelectSingleNode("bookingClass").InnerText, PromItem.SelectSingleNode("bookingClassExc").InnerText, PromItem.SelectSingleNode("specialYN").InnerText, recommendation.SelectSingleNode(String.Format("m:paxFareProduct[m:paxReference/m:ptc='{0}']", PTC), xnMgr), xnMgr))
                                                {
                                                    NewPaxFareGroup = NewPriceIndex2.InsertBefore(PaxFareGroup.CloneNode(true), PaxFareGroup);
                                                    SetPriceIndexAmadeus(SNM, NewPaxFareGroup, recommendation, xnMgr, PromItem, "N", DepartureFromKorea, SelectUserTASF, UMaxStay, TLDate);

                                                    //항공운임이 일만원 미만일 경우 삭제
                                                    if (cm.RequestDouble(NewPaxFareGroup.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText) < 10000)
                                                        NewPriceIndex2.RemoveChild(NewPaxFareGroup);
                                                    else
                                                    {
                                                        NewPaxFareGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "D", PriceSubRef++);
                                                        NewPaxFareGroup.Attributes.GetNamedItem("mode").InnerText = MODE;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("adtPrice").InnerText = NewPaxFareGroup.SelectSingleNode("paxFare[@ptc='ADT']").Attributes.GetNamedItem("price").InnerText;
                                                        NewPaxFareGroup.Attributes.GetNamedItem("gds").InnerText = "Amadeus";

                                                        if (!PromFareYN)
                                                            PromFareYN = true;
                                                    }
                                                }
                                            }
                                        }

                                        //프로모션 적용전 운임(SF이면서 프로모션이 적용된 운임이 존재할 경우에는 기본운임은 미노출,2018-06-14,방경도팀장)
                                        if (!SF || !PromFareYN)
                                        {
                                            NewPaxFareGroup = NewPriceIndex2.InsertBefore(PaxFareGroup.CloneNode(true), PaxFareGroup);
                                            SetPriceIndexAmadeus(SNM, NewPaxFareGroup, recommendation, xnMgr, null, "Y", DepartureFromKorea, SelectUserTASF, UMaxStay, TLDate);

                                            //항공운임이 일만원 미만일 경우 삭제
                                            if (cm.RequestDouble(NewPaxFareGroup.SelectSingleNode("summary").Attributes.GetNamedItem("fare").InnerText) < 10000)
                                                NewPriceIndex2.RemoveChild(NewPaxFareGroup);
                                            else
                                            {
                                                NewPaxFareGroup.Attributes.GetNamedItem("ref").InnerText = Common.RefOverrideFull(BasicRef, "C", PriceSubRef++);
                                                NewPaxFareGroup.Attributes.GetNamedItem("mode").InnerText = MODE;
                                                NewPaxFareGroup.Attributes.GetNamedItem("ptc").InnerText = PTC;
                                                NewPaxFareGroup.Attributes.GetNamedItem("adtPrice").InnerText = NewPaxFareGroup.SelectSingleNode("paxFare[@ptc='ADT']").Attributes.GetNamedItem("price").InnerText;
                                                NewPaxFareGroup.Attributes.GetNamedItem("gds").InnerText = "Amadeus";
                                            }
                                        }

                                        NewPriceIndex2.RemoveChild(PaxFareGroup);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PriceInfo.RemoveChild(PriceIndex);

            #endregion "운임"

            //로그
            cm.XmlFileSave(XmlDoc, mc.Name, "ToModeSearchFareAvailRS", "N", GUID);

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 운임정보리스트 생성
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="NewPaxFareGroup">신규 운임 노드</param>
        /// <param name="Recommendation">아마데우스 운임 정보</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <param name="PromItem">프로모션 정보</param>
        /// <param name="CDS">공동운항 운임 여부</param>
        /// <param name="DepartureFromKorea">한국출발여부</param>
        /// <param name="SelectUserTASF">TASF 적용 사용자 선택 가능 여부(Y:사용자 선택 가능, N:사용자 선택 불가)</param>
        /// <param name="UMaxStay">유효기간 정보(MPIS 등 고정 처리시)</param>
        /// <param name="TLDate">발권마감일(LTD)</param>
        public void SetPriceIndexAmadeus(int SNM, XmlNode NewPaxFareGroup, XmlNode Recommendation, XmlNamespaceManager xnMgr, XmlNode PromItem, string CDS, bool DepartureFromKorea, string SelectUserTASF, string UMaxStay, string TLDate)
        {
            XmlNode Summary = NewPaxFareGroup.SelectSingleNode("summary");
            XmlNode PromotionInfo = NewPaxFareGroup.SelectSingleNode("promotionInfo");
            XmlNode PaxFare = NewPaxFareGroup.SelectSingleNode("paxFare");
            XmlNode SegmentFare = PaxFare.SelectSingleNode("segFare");

            XmlNode NewPaxFare;
            XmlNode NewSegmentFare;

            string PTC = string.Empty;
            string FuelSurCharge = string.Empty;
            string QCharge = string.Empty;
            double FareAmount = 0;
            double PaxDiscountFare = 0;
            double TotalDiscountFare = 0;
            string MaxStay = string.Empty;
            string ValidatingCarrier = Recommendation.SelectSingleNode("m:paxFareProduct/m:paxFareDetail/m:codeShareDetails[m:transportStageQualifier='V']/m:company", xnMgr).InnerText;
            string CabinClassItem = Recommendation.SelectSingleNode("m:paxFareProduct/m:fareDetails[1]/m:majCabin/m:bookingClassDetails/m:designator", xnMgr).InnerText;
            int PaxCount = 0;
            int fRef = 1;

            //TASF(발권 여행사 수수료)
            bool UseTASF = Common.ApplyTASF(SNM, ValidatingCarrier);
            double TASF = 0;
            double TotalTASF = 0;

            //파트너 할인요금(추가할인금액)
            double PartnerDiscount = 0;
            double TotalPartnerDiscount = 0;

            foreach (XmlNode paxFareProduct in Recommendation.SelectNodes("m:paxFareProduct", xnMgr))
            {
                PTC = Common.ChangePaxType1(paxFareProduct.SelectSingleNode("m:paxReference/m:ptc", xnMgr).InnerText);
                FuelSurCharge = (paxFareProduct.SelectNodes("m:paxFareDetail/m:monetaryDetails[m:amountType='F']/m:amount", xnMgr).Count > 0) ? paxFareProduct.SelectSingleNode("m:paxFareDetail/m:monetaryDetails[m:amountType='F']/m:amount", xnMgr).InnerText : "0";
                QCharge = (paxFareProduct.SelectNodes("m:paxFareDetail/m:monetaryDetails[m:amountType='Q']/m:amount", xnMgr).Count > 0) ? paxFareProduct.SelectSingleNode("m:paxFareDetail/m:monetaryDetails[m:amountType='Q']/m:amount", xnMgr).InnerText : "0";

                FareAmount = cm.GetFare(ValidatingCarrier, paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalFareAmount", xnMgr).InnerText, paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText, QCharge);
                PaxDiscountFare = cm.PromotionFare(FareAmount, paxFareProduct.SelectSingleNode("m:paxReference/m:ptc", xnMgr).InnerText, PromItem) - PartnerDiscount;
                TASF = UseTASF ? Common.GetTASF(SNM, PTC, ValidatingCarrier, DepartureFromKorea) : 0;

                PaxCount = paxFareProduct.SelectNodes("m:paxReference/m:traveller", xnMgr).Count;
                TotalDiscountFare += (PaxDiscountFare * PaxCount);
                TotalTASF += (TASF * PaxCount);
                TotalPartnerDiscount += PartnerDiscount;

                NewPaxFare = NewPaxFareGroup.InsertBefore(PaxFare.CloneNode(false), PromotionInfo);
                NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = (PTC.Equals("CHD") || PTC.Equals("INF")) ? PTC : "ADT";
                NewPaxFare.Attributes.GetNamedItem("fare").InnerText = FareAmount.ToString();
                NewPaxFare.Attributes.GetNamedItem("disFare").InnerText = PaxDiscountFare.ToString();
                NewPaxFare.Attributes.GetNamedItem("tax").InnerText = cm.GetTax(paxFareProduct.SelectSingleNode("m:paxFareDetail/m:totalTaxAmount", xnMgr).InnerText, FuelSurCharge).ToString();
                NewPaxFare.Attributes.GetNamedItem("fsc").InnerText = cm.GetFuelSurCharge(ValidatingCarrier, FuelSurCharge, QCharge).ToString();
                NewPaxFare.Attributes.GetNamedItem("disPartner").InnerText = PartnerDiscount.ToString();
                NewPaxFare.Attributes.GetNamedItem("tasf").InnerText = TASF.ToString();
                NewPaxFare.Attributes.GetNamedItem("mTasf").InnerText = TASF.ToString();
                NewPaxFare.Attributes.GetNamedItem("aTasf").InnerText = "0";
                NewPaxFare.Attributes.GetNamedItem("count").InnerText = PaxCount.ToString();
                NewPaxFare.Attributes.GetNamedItem("price").InnerText = (PaxDiscountFare + Convert.ToInt32(NewPaxFare.Attributes.GetNamedItem("tax").InnerText) + Convert.ToInt32(NewPaxFare.Attributes.GetNamedItem("fsc").InnerText) + TASF).ToString();

                //여정별 요금정보
                foreach (XmlNode fareDetails in paxFareProduct.SelectNodes("m:fareDetails", xnMgr))
                {
                    NewSegmentFare = NewPaxFare.AppendChild(SegmentFare.CloneNode(true));
                    NewSegmentFare.Attributes.GetNamedItem("ref").InnerText = fareDetails.SelectSingleNode("m:segmentRef/m:segRef", xnMgr).InnerText;
                    fRef = 1;

                    foreach (XmlNode groupOfFares in fareDetails.SelectNodes("m:groupOfFares", xnMgr))
                    {
                        string StrMas = (groupOfFares.SelectNodes("m:fareFamiliesRef", xnMgr).Count > 0 && (XmlAttribute)groupOfFares.SelectSingleNode("m:fareFamiliesRef", xnMgr).Attributes.GetNamedItem("maxStay") != null) ? groupOfFares.SelectSingleNode("m:fareFamiliesRef", xnMgr).Attributes.GetNamedItem("maxStay").InnerText : ((groupOfFares.SelectSingleNode("m:productInformation/m:breakPoint", xnMgr).InnerText.Equals("Y")) ? "1Y" : "");
                        string StrFareType = string.Empty;
                        
                        foreach (XmlNode TmpFareType in groupOfFares.SelectNodes("m:productInformation/m:fareProductDetail/m:fareType", xnMgr))
                        {
                            if (!String.IsNullOrWhiteSpace(StrFareType))
                                StrFareType += ",";

                            StrFareType += TmpFareType.InnerText.Equals("MS") ? "MSP" : (TmpFareType.InnerText.Equals("NT") ? "NTF" : TmpFareType.InnerText);
                        }

                        if (!String.IsNullOrWhiteSpace(NewSegmentFare.InnerText))
                            NewSegmentFare.InnerText += "/";

                        NewSegmentFare.InnerText += String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}",
                            (fRef++),
                            StrMas,
                            groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:fareBasis", xnMgr).InnerText,
                            groupOfFares.SelectSingleNode("m:productInformation/m:fareProductDetail/m:passengerType", xnMgr).InnerText,
                            (groupOfFares.SelectNodes("m:ticketInfos", xnMgr).Count > 0 && groupOfFares.SelectNodes("m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).InnerText : "",
                            groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:rbd", xnMgr).InnerText,
                            (groupOfFares.SelectNodes("m:productInformation/m:cabinProduct/m:cabin", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:cabin", xnMgr).InnerText : "",
                            (groupOfFares.SelectNodes("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).Count > 0) ? ((groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).InnerText.Equals("L")) ? "0" : groupOfFares.SelectSingleNode("m:productInformation/m:cabinProduct/m:avlStatus", xnMgr).InnerText) : "",
                            (groupOfFares.SelectNodes("m:productInformation/m:corporateId", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:productInformation/m:corporateId", xnMgr).InnerText : "",
                            (groupOfFares.SelectNodes("m:productInformation/m:breakPoint", xnMgr).Count > 0) ? groupOfFares.SelectSingleNode("m:productInformation/m:breakPoint", xnMgr).InnerText : "",
                            StrFareType);

                        MaxStay = cm.SetMaxStay(MaxStay, StrMas);
                    }
                }
            }

            NewPaxFareGroup.RemoveChild(PaxFare);

            //요약 정보
            string SummaryFuelSurCharge = (Recommendation.SelectNodes("m:recPriceInfo/m:monetaryDetail[m:amountType='F']/m:amount", xnMgr).Count > 0) ? Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[m:amountType='F']/m:amount", xnMgr).InnerText : "0";
            string SummaryQCharge = (Recommendation.SelectNodes("m:recPriceInfo/m:monetaryDetail[m:amountType='Q']/m:amount", xnMgr).Count > 0) ? Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[m:amountType='Q']/m:amount", xnMgr).InnerText : "0";

            Summary.Attributes.GetNamedItem("fare").InnerText = cm.GetFare(ValidatingCarrier, Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[1]/m:amount", xnMgr).InnerText, Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText, SummaryQCharge).ToString();
            Summary.Attributes.GetNamedItem("disFare").InnerText = TotalDiscountFare.ToString();
            Summary.Attributes.GetNamedItem("tax").InnerText = cm.GetTax(Recommendation.SelectSingleNode("m:recPriceInfo/m:monetaryDetail[2]/m:amount", xnMgr).InnerText, SummaryFuelSurCharge).ToString();
            Summary.Attributes.GetNamedItem("fsc").InnerText = cm.GetFuelSurCharge(ValidatingCarrier, SummaryFuelSurCharge, SummaryQCharge).ToString();
            Summary.Attributes.GetNamedItem("disPartner").InnerText = TotalPartnerDiscount.ToString();
            Summary.Attributes.GetNamedItem("tasf").InnerText = TotalTASF.ToString();
            Summary.Attributes.GetNamedItem("mTasf").InnerText = TotalTASF.ToString();
            Summary.Attributes.GetNamedItem("aTasf").InnerText = "0";
            Summary.Attributes.GetNamedItem("pvc").InnerText = ValidatingCarrier;
            Summary.Attributes.GetNamedItem("mas").InnerText = String.IsNullOrWhiteSpace(UMaxStay) ? MaxStay : UMaxStay;
            Summary.Attributes.GetNamedItem("cabin").InnerText = CabinClassItem;
            Summary.Attributes.GetNamedItem("ttl").InnerText = TLDate;
            Summary.Attributes.GetNamedItem("cds").InnerText = CDS;
            Summary.Attributes.GetNamedItem("cff").InnerText = (Recommendation.SelectNodes("m:itemNumber/m:priceTicketing[m:priceType='UF']", xnMgr).Count > 0) ? "N" : "Y";
            Summary.Attributes.GetNamedItem("ttf").InnerText = (Recommendation.SelectNodes("m:paxFareProduct/m:fareDetails/m:groupOfFares/m:productInformation/m:fareProductDetail/m:fareType[.='NTF']", xnMgr).Count > 0) ? "N" : "Y";
            Summary.Attributes.GetNamedItem("sutf").InnerText = SelectUserTASF;
            Summary.Attributes.GetNamedItem("pmsc").InnerText = "";
            Summary.Attributes.GetNamedItem("pmsn").InnerText = "";
            Summary.Attributes.GetNamedItem("pmcd").InnerText = "";
            Summary.Attributes.GetNamedItem("pmtl").InnerText = "";
            Summary.Attributes.GetNamedItem("sscd").InnerText = "";
            Summary.Attributes.GetNamedItem("sstl").InnerText = "";
            Summary.Attributes.GetNamedItem("subPrice").InnerText = (TotalDiscountFare + cm.RequestDouble(Summary.Attributes.GetNamedItem("tax").InnerText) + cm.RequestDouble(Summary.Attributes.GetNamedItem("fsc").InnerText)).ToString();
            Summary.Attributes.GetNamedItem("price").InnerText = (cm.RequestDouble(Summary.Attributes.GetNamedItem("subPrice").InnerText) + TotalTASF).ToString();

            //운임번호
            //XmlNode RecommendationNumber = NewPriceIndex.OwnerDocument.CreateElement("itemNumber");
            //RecommendationNumber.InnerText = Recommendation.SelectSingleNode("m:itemNumber/m:itemNumberId/m:number", xnMgr).InnerText;
            //NewPriceIndex.SelectSingleNode("fareMessage").AppendChild(RecommendationNumber);

            //프로모션 정보
            if (PromItem != null)
            {
                foreach (XmlNode Item in PromItem.ChildNodes)
                {
                    if (Item.Name.Equals("promotionTL"))
                        PromotionInfo.InnerText += Common.ConvertToOnlyNumber(Item.InnerText);
                    else
                        PromotionInfo.InnerText += String.Concat(Item.InnerText, "^");
                }

                Summary.Attributes.GetNamedItem("pmsc").InnerText = PromItem.SelectSingleNode("incentiveSignCode").InnerText;
                Summary.Attributes.GetNamedItem("pmsn").InnerText = PromItem.SelectSingleNode("incentiveSignName").InnerText;
                Summary.Attributes.GetNamedItem("pmcd").InnerText = PromItem.SelectSingleNode("promotionId").InnerText;
                Summary.Attributes.GetNamedItem("pmtl").InnerText = PromItem.SelectSingleNode("fareTarget").InnerText;
            }
            else
                NewPaxFareGroup.RemoveChild(PromotionInfo);
        }

        #endregion "Fare + Availability 동시조회결과(아마데우스)"

        #region "Fare + Availability 동시조회(세이버)"

        /// <summary>
        /// Fare + Availability 동시조회(상세 조건 조회)(세이버)
        /// </summary>
        public XmlElement SearchFareAvailSabreRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string PTC, int ADC, int CHC, int IFC, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID)
        {
            try
            {
                string[] Dep = new String[4] { "", "", "", "" };
                string[] Arr = new String[4] { "", "", "", "" };
                string[] DepDate = new String[4] { "", "", "", "" };
                string RetDate = "";

                if (ROT.Equals("RT"))
                {
                    Dep[0] = DLC;
                    Arr[0] = ALC;
                    DepDate[0] = Common.ConvertToOnlyNumber(DTD);

                    Dep[1] = ALC;
                    Arr[1] = DLC;
                    DepDate[1] = Common.ConvertToOnlyNumber(ARD);

                    RetDate = DepDate[1];
                }
                else if (ROT.Equals("MD"))
                {
                    string[] DLCs = DLC.Split(',');
                    string[] ALCs = ALC.Split(',');
                    string[] DTDs = DTD.Split(',');

                    for (int i = 0; i < DLCs.Length; i++)
                    {
                        Dep[i] = DLCs[i];
                        Arr[i] = ALCs[i];
                        DepDate[i] = Common.ConvertToOnlyNumber(DTDs[i]);
                    }
                }
                else //OW
                {
                    Dep[0] = DLC;
                    Arr[0] = ALC;
                    DepDate[0] = Common.ConvertToOnlyNumber(DTD);
                }

                //GDS XML
                XmlElement XmlGDS = sas.SearchFareAvailRS(ROT, Dep, Arr, DepDate, RetDate, (OPN.Equals("Y") ? ARD : ""), (CCD.Equals("M") ? "Y" : (CCD.Equals("W") ? "P" : CCD)), ADC, CHC, IFC, "M", String.Format("{0}-{1}-{2}", GUID, PTC, CCD));

                //오류 결과일 경우 예외 처리
                if (XmlGDS.SelectSingleNode("error_no").InnerText != "0")
                    throw new Exception(XmlGDS.SelectSingleNode("error_desc").InnerText);

                else if (XmlGDS.SelectSingleNode("FSAMAP/NFSA").InnerText.Equals("0"))
                    throw new Exception("항공요금 검색 결과가 없습니다.");

                //통합 XML
                XmlElement XmlMode = XmlGDS;



                return XmlMode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion "Fare + Availability 동시조회(세이버)"

        #region "Fare + Availability 동시조회결과(세이버)"



        #endregion "Fare + Availability 동시조회결과(세이버)"

        #endregion "Fare + Availability 동시조회"

        #region "다운그레이션"

        /// <summary>
        /// 예약 가능한 PriceIndex 구조로 변경
        /// </summary>
        /// <param name="FXL">선택된 paxFareGroup XmlNode</param>
        /// <param name="FREF">SearchFareAvailRS RootNode의 'ref' 값</param>
        /// <param name="FGID">SearchFareAvailRS RootNode의 'guid' 값</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "예약 가능한 PriceIndex 구조로 변경")]
        public XmlElement FXLDowngrade(string FXL, string FREF, string FGID, string RQT)
        {
            int ServiceNumber = 667;
            string LogGUID = cm.GetGUID;

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
                        new SqlParameter("@요청41", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청42", SqlDbType.VarChar, -1),
                        new SqlParameter("@요청43", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = FXL;
                sqlParam[8].Value = FREF;
                sqlParam[9].Value = FGID;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            try
            {
                //테스트용
                if (String.IsNullOrWhiteSpace(FXL))
                {
                    FXL = "<paxFareGroup ref=\"A00STUMA001\" mode=\"\" ptc=\"STU\" adtPrice=\"1115900\" gds=\"Amadeus\"><summary price=\"1115900\" subPrice=\"1105900\" fare=\"850000\" disFare=\"850000\" tax=\"56900\" fsc=\"199000\" disPartner=\"0\" tasf=\"10000\" mTasf=\"10000\" aTasf=\"0\" pvc=\"GA\" mas=\"6M\" cabin=\"M\" ttl=\"2019-04-27\" cds=\"N\" cff=\"Y\" ttf=\"N\" sutf=\"N\" pmsc=\"\" pmsn=\"\" pmcd=\"\" pmtl=\"\" sscd=\"\" sstl=\"\"/><paxFare ptc=\"ADT\" price=\"1115900\" fare=\"850000\" disFare=\"850000\" tax=\"56900\" fsc=\"199000\" disPartner=\"0\" tasf=\"10000\" mTasf=\"10000\" aTasf=\"0\" count=\"1\"><segFare ref=\"1\">1^^ML1YNWKR^SD^SD^M^M^9^^N^RN,MSP,NTF/2^1Y^ML1YNWKR^SD^SD^M^M^9^^Y^RN,MSP,NTF</segFare><segFare ref=\"2\">1^^NL6MNWKR^SD^SD^N^M^9^^N^RN,MSP,NTF/2^6M^NL6MNWKR^SD^SD^N^M^9^^Y^RN,MSP,NTF</segFare></paxFare></paxFareGroup>";
                    FREF = "5819";
                    FGID = "ITHV010-201904251047415809";
                }

                XmlDocument XmlFXL = new XmlDocument();
                XmlFXL.LoadXml(FXL);

                XmlNode V3PaxFareGroup = XmlFXL.SelectSingleNode("paxFareGroup");
                XmlNode V3Summary = V3PaxFareGroup.SelectSingleNode("summary");

                //이전 버전 양식
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                XmlNode PriceIndex = XmlDoc.SelectSingleNode("ResponseDetails/priceInfo/priceIndex");
                XmlNode Summary = PriceIndex.SelectSingleNode("summary");
                XmlNode SegGroup = PriceIndex.SelectSingleNode("segGroup");
                XmlNode PaxFareGroup = PriceIndex.SelectSingleNode("paxFareGroup");
                XmlNode PaxFare = PaxFareGroup.SelectSingleNode("paxFare");
                XmlNode SegFareGroup;
                XmlNode SegFare;
                XmlNode Fare;
                XmlNode FareType;
                XmlNode Traveler;
                XmlNode TravelerRef;
                XmlNode NewPaxFare;
                XmlNode NewSegFare;
                XmlNode NewFare;
                XmlNode NewFareType;
                XmlNode NewTravelerRef;
                int TravelerIndex = 1;

                //priceIndex
                PriceIndex.Attributes.GetNamedItem("gds").InnerText = V3PaxFareGroup.Attributes.GetNamedItem("gds").InnerText;
                PriceIndex.Attributes.GetNamedItem("mode").InnerText = V3PaxFareGroup.Attributes.GetNamedItem("mode").InnerText;
                PriceIndex.Attributes.GetNamedItem("ptc").InnerText = V3PaxFareGroup.Attributes.GetNamedItem("ptc").InnerText;
                PriceIndex.Attributes.GetNamedItem("ref").InnerText = FREF;
                PriceIndex.Attributes.GetNamedItem("guid").InnerText = FGID;

                //priceIndex/summary
                Summary.Attributes.GetNamedItem("price").InnerText = V3Summary.Attributes.GetNamedItem("price").InnerText;
                Summary.Attributes.GetNamedItem("fare").InnerText = V3Summary.Attributes.GetNamedItem("fare").InnerText;
                Summary.Attributes.GetNamedItem("disFare").InnerText = V3Summary.Attributes.GetNamedItem("disFare").InnerText;
                Summary.Attributes.GetNamedItem("tax").InnerText = V3Summary.Attributes.GetNamedItem("tax").InnerText;
                Summary.Attributes.GetNamedItem("fsc").InnerText = V3Summary.Attributes.GetNamedItem("fsc").InnerText;
                Summary.Attributes.GetNamedItem("disPartner").InnerText = V3Summary.Attributes.GetNamedItem("disPartner").InnerText;
                Summary.Attributes.GetNamedItem("tasf").InnerText = V3Summary.Attributes.GetNamedItem("tasf").InnerText;
                Summary.Attributes.GetNamedItem("mTasf").InnerText = V3Summary.Attributes.GetNamedItem("mTasf").InnerText;
                Summary.Attributes.GetNamedItem("aTasf").InnerText = V3Summary.Attributes.GetNamedItem("aTasf").InnerText;
                Summary.Attributes.GetNamedItem("pvc").InnerText = V3Summary.Attributes.GetNamedItem("pvc").InnerText;
                Summary.Attributes.GetNamedItem("mas").InnerText = V3Summary.Attributes.GetNamedItem("mas").InnerText;
                Summary.Attributes.GetNamedItem("ttl").InnerText = V3Summary.Attributes.GetNamedItem("ttl").InnerText;
                Summary.Attributes.GetNamedItem("cds").InnerText = V3Summary.Attributes.GetNamedItem("cds").InnerText;
                Summary.Attributes.GetNamedItem("ucf").InnerText = ((XmlAttribute)V3Summary.Attributes.GetNamedItem("cff") != null) ? (V3Summary.Attributes.GetNamedItem("cff").InnerText.Equals("Y") ? "N" : "Y") : V3Summary.Attributes.GetNamedItem("ucf").InnerText;
                Summary.Attributes.GetNamedItem("ntf").InnerText = ((XmlAttribute)V3Summary.Attributes.GetNamedItem("ttf") != null) ? (V3Summary.Attributes.GetNamedItem("ttf").InnerText.Equals("Y") ? "N" : "Y") : V3Summary.Attributes.GetNamedItem("ntf").InnerText;
                Summary.Attributes.GetNamedItem("sutf").InnerText = V3Summary.Attributes.GetNamedItem("sutf").InnerText;

                //priceIndex/segGroup
                PriceIndex.RemoveChild(SegGroup);

                //priceIndex/paxFareGroup/paxFare
                foreach (XmlNode V3PaxFare in V3PaxFareGroup.SelectNodes("paxFare"))
                {
                    NewPaxFare = PaxFareGroup.AppendChild(PaxFare.Clone());
                    NewPaxFare.Attributes.GetNamedItem("ptc").InnerText = V3PaxFare.Attributes.GetNamedItem("ptc").InnerText.Equals("ADT") ? V3PaxFareGroup.Attributes.GetNamedItem("ptc").InnerText : V3PaxFare.Attributes.GetNamedItem("ptc").InnerText;

                    //priceIndex/paxFareGroup/paxFare/amount
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fare").InnerText = V3PaxFare.Attributes.GetNamedItem("fare").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disFare").InnerText = V3PaxFare.Attributes.GetNamedItem("disFare").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tax").InnerText = V3PaxFare.Attributes.GetNamedItem("tax").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("fsc").InnerText = V3PaxFare.Attributes.GetNamedItem("fsc").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("disPartner").InnerText = V3PaxFare.Attributes.GetNamedItem("disPartner").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("tasf").InnerText = V3PaxFare.Attributes.GetNamedItem("tasf").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("mTasf").InnerText = V3PaxFare.Attributes.GetNamedItem("mTasf").InnerText;
                    NewPaxFare.SelectSingleNode("amount").Attributes.GetNamedItem("aTasf").InnerText = V3PaxFare.Attributes.GetNamedItem("aTasf").InnerText;

                    //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare
                    SegFareGroup = NewPaxFare.SelectSingleNode("segFareGroup");
                    SegFare = SegFareGroup.SelectSingleNode("segFare");

                    foreach (XmlNode V3SegFare in V3PaxFare.SelectNodes("segFare"))
                    {
                        NewSegFare = SegFareGroup.AppendChild(SegFare.Clone());
                        NewSegFare.Attributes.GetNamedItem("ref").InnerText = V3SegFare.Attributes.GetNamedItem("ref").InnerText;

                        //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare
                        Fare = NewSegFare.SelectSingleNode("fare");

                        foreach (String V3Fare in V3SegFare.InnerText.Split('/'))
                        {
                            string[] V3FareInfo = V3Fare.Split('^');

                            NewFare = NewSegFare.AppendChild(Fare.Clone());
                            NewFare.Attributes.GetNamedItem("bpt").InnerText = V3FareInfo[9];
                            NewFare.Attributes.GetNamedItem("mas").InnerText = V3FareInfo[1];

                            //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/cabin
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("rbd").InnerText = V3FareInfo[5];
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("cabin").InnerText = V3FareInfo[6];
                            NewFare.SelectSingleNode("cabin").Attributes.GetNamedItem("avl").InnerText = V3FareInfo[7];

                            //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/fare
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("basis").InnerText = V3FareInfo[2];
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("ptc").InnerText = V3FareInfo[3];
                            NewFare.SelectSingleNode("fare").Attributes.GetNamedItem("tkd").InnerText = V3FareInfo[4];

                            //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/fare/fareType
                            FareType = NewFare.SelectSingleNode("fare/fareType");

                            foreach (String V3FareType in V3FareInfo[10].Split(','))
                            {
                                NewFareType = NewFare.SelectSingleNode("fare").AppendChild(FareType.Clone());
                                NewFareType.InnerText = V3FareType;
                            }

                            NewFare.SelectSingleNode("fare").RemoveChild(FareType);

                            //priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/corporateId
                            if (String.IsNullOrWhiteSpace(V3FareInfo[8]))
                                NewFare.RemoveChild(NewFare.SelectSingleNode("corporateId"));
                            else
                                NewFare.SelectSingleNode("corporateId").InnerText = V3FareInfo[8];
                        }

                        NewSegFare.RemoveChild(Fare);
                    }

                    SegFareGroup.RemoveChild(SegFare);

                    //priceIdex/paxFareGroup/paxFare/traveler
                    Traveler = NewPaxFare.SelectSingleNode("traveler");
                    TravelerRef = Traveler.SelectSingleNode("ref");

                    for (int i = 0; i < Convert.ToInt32(V3PaxFare.Attributes.GetNamedItem("count").InnerText); i++)
                    {
                        NewTravelerRef = Traveler.AppendChild(TravelerRef.CloneNode(false));
                        NewTravelerRef.InnerText = (TravelerIndex++).ToString();
                    }

                    Traveler.RemoveChild(TravelerRef);
                }

                PaxFareGroup.RemoveChild(PaxFare);

                //priceIndex/promotionInfo
                if (V3PaxFareGroup.SelectNodes("promotionInfo").Count > 0)
                {
                    String[] Items = V3PaxFareGroup.SelectSingleNode("promotionInfo").InnerText.Split('^');

                    XmlDocument XmlProm = new XmlDocument();
                    XmlProm.LoadXml(String.Format("<promotionInfo><item><promotionId>{0}</promotionId><siteNum>{1}</siteNum><airCode>{2}</airCode><tripType>{3}</tripType><fareType>{4}</fareType><fareBasis>{5}</fareBasis><cabinClass>{6}</cabinClass><bookingClass>{7}</bookingClass><bookingClassExc>{8}</bookingClassExc><paxType>{9}</paxType><discount>{10}</discount><commission>{11}</commission><fareDiscount>{12}</fareDiscount><incentive>{13}</incentive><incentiveCode>{14}</incentiveCode><incentiveName>{15}</incentiveName><fareTarget>{16}</fareTarget><childDiscountYN>{17}</childDiscountYN><supplementaryServiceCode>{18}</supplementaryServiceCode><supplementaryServiceTitle>{19}</supplementaryServiceTitle><codeshare>{20}</codeshare><specialYN>{21}</specialYN><promotionTL>{22}</promotionTL><promotions><promotion promotionId=\"{0}\" incentiveCode=\"{14}\" incentiveName=\"{15}\" fareTarget=\"{16}\" promotionTL=\"{22}\"/></promotions></item></promotionInfo>", Items[0], Items[1], Items[2], Items[3], Items[4], Items[5], Items[6], Items[8], Items[9], Items[7], Items[10], Items[11], Items[12], Items[13], Items[14], Items[15], Items[18], Items[19], Items[20], Items[21], Items[22], Items[23], Items[25]));

                    PriceIndex.ReplaceChild(XmlDoc.ImportNode(XmlProm.FirstChild, true), PriceIndex.SelectSingleNode("promotionInfo"));
                }

                XmlDocument XmlResult = new XmlDocument();
                XmlResult.LoadXml(PriceIndex.OuterXml);

                return XmlResult.DocumentElement;
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirService3", MethodBase.GetCurrentMethod().Name, ServiceNumber, 0, 0).ToErrors;
            }
        }

        #endregion "다운그레이션"

        #region "요금규정"

        /// <summary>
        /// 운임규정 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SXL">선택한 여정을 <itinerary>~</itinerary>노드에 삽입한 XML</param>
        /// <param name="FXL">선택한 <paxFareGroup>~</paxFareGroup> XmlNode</param>
        /// <param name="FREF">SearchFareAvailRS RootNode의 'ref' 값</param>
        /// <param name="FGID">SearchFareAvailRS RootNode의 'guid' 값</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "운임규정 조회")]
        public XmlElement SearchRuleRS(int SNM, string SXL, string FXL, string FREF, string FGID, string RQT)
        {
            int ServiceNumber = 665;
            string LogGUID = cm.GetGUID;

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
                        new SqlParameter("@요청42", SqlDbType.VarChar, -1)
                    };

                sqlParam[0].Value = ServiceNumber;
                sqlParam[1].Value = SNM;
                sqlParam[2].Value = RQT;
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = FREF;
                sqlParam[8].Value = FGID;
                sqlParam[9].Value = SXL;
                sqlParam[10].Value = FXL;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }
            
            try
            {
                XmlDocument XmlSXL = new XmlDocument();
                XmlSXL.LoadXml(SXL);

                XmlDocument XmlFXL = new XmlDocument();
                XmlFXL.LoadXml(FXL);

                string PMID = XmlFXL.SelectSingleNode("paxFareGroup/summary").Attributes.GetNamedItem("pmcd").InnerText;
                string PFG = String.Empty;

                int SegCount = XmlSXL.SelectNodes("itinerary/*/seg").Count;
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

                int x = 0;
                foreach (XmlNode SegGroup in XmlSXL.SelectNodes("itinerary/segGroup"))
                {
                    string FiRef = SegGroup.Attributes.GetNamedItem("fiRef").InnerText;
                    string[] FareSeg = XmlFXL.SelectSingleNode(String.Format("paxFareGroup/paxFare[1]/segFare[@ref='{0}']", FiRef)).InnerText.Split('/');
                    int y = 0;

                    foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
                    {
                        INO[x] = Convert.ToInt32(FiRef);
                        DTD[x] = Seg.Attributes.GetNamedItem("ddt").InnerText.Substring(0, 10);
                        DTT[x] = "";
                        ARD[x] = Seg.Attributes.GetNamedItem("ardt").InnerText.Substring(0, 10);
                        ART[x] = "";
                        DLC[x] = Seg.Attributes.GetNamedItem("dlc").InnerText;
                        ALC[x] = Seg.Attributes.GetNamedItem("alc").InnerText;
                        MCC[x] = Seg.Attributes.GetNamedItem("mcc").InnerText;
                        OCC[x] = Seg.Attributes.GetNamedItem("occ").InnerText;
                        FLN[x] = Seg.Attributes.GetNamedItem("fln").InnerText;
                        RBD[x] = FareSeg[y].Split('^')[5];

                        x++;
                        y++;
                    }
                }

                return asv.SearchRuleRS(SNM, PMID, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, FXLDowngrade(FXL, FREF, FGID, RQT).SelectSingleNode("paxFareGroup").OuterXml);
            }
            catch (Exception ex)
            {
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirService3", MethodBase.GetCurrentMethod().Name, ServiceNumber, 0, 0).ToErrors;
            }
        }

        [WebMethod(Description = "운임규정(ART) 조회")]
        public string SearchRuleARTRS(int SNM, string SXL, string FXL, string FREF, string FGID, string RQT)
        {
            XmlDocument XmlSXL = new XmlDocument();
            XmlSXL.LoadXml(SXL);

            XmlDocument XmlFXL = new XmlDocument();
            XmlFXL.LoadXml(FXL);

            string PMID = XmlFXL.SelectSingleNode("paxFareGroup/summary").Attributes.GetNamedItem("pmcd").InnerText;
            string PFG = String.Empty;

            int SegCount = XmlSXL.SelectNodes("itinerary/*/seg").Count;
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

            int x = 0;
            foreach (XmlNode SegGroup in XmlSXL.SelectNodes("itinerary/segGroup"))
            {
                string FiRef = SegGroup.Attributes.GetNamedItem("fiRef").InnerText;
                string[] FareSeg = XmlFXL.SelectSingleNode(String.Format("paxFareGroup/paxFare[1]/segFare[@ref='{0}']", FiRef)).InnerText.Split('/');
                int y = 0;

                foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
                {
                    INO[x] = Convert.ToInt32(FiRef);
                    DTD[x] = Seg.Attributes.GetNamedItem("ddt").InnerText.Substring(0, 10);
                    DTT[x] = "";
                    ARD[x] = Seg.Attributes.GetNamedItem("ardt").InnerText.Substring(0, 10);
                    ART[x] = "";
                    DLC[x] = Seg.Attributes.GetNamedItem("dlc").InnerText;
                    ALC[x] = Seg.Attributes.GetNamedItem("alc").InnerText;
                    MCC[x] = Seg.Attributes.GetNamedItem("mcc").InnerText;
                    OCC[x] = Seg.Attributes.GetNamedItem("occ").InnerText;
                    FLN[x] = Seg.Attributes.GetNamedItem("fln").InnerText;
                    RBD[x] = FareSeg[y].Split('^')[5];

                    x++;
                    y++;
                }
            }
            
            return new TopasAirService().AutomatedRuleTranslatorRS(SNM, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, FXLDowngrade(FXL, FREF, FGID, RQT).SelectSingleNode("paxFareGroup").OuterXml, cm.GetGUID);
        }

        #endregion "요금규정"

        #region "프로모션"

        /// <summary>
        /// 프로모션 리스트
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYY-MM-DD)</param>
        /// <param name="ARD">도착일(YYYY-MM-DD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="CCD">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <returns></returns>
        public static XmlElement SearchPromotionList(int SNM, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string CCD, string PTC)
        {
            XmlDocument XmlDoc = new XmlDocument();

            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                {
                    using (DataSet ds = new DataSet("promotionInfo"))
                    {
                        cmd.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString);
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "DBO.WSP_S_항공할인";

                        cmd.Parameters.Add("@SNM", SqlDbType.Int, 0);
                        cmd.Parameters.Add("@DLC", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@ALC", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@ROT", SqlDbType.Char, 2);
                        cmd.Parameters.Add("@DTD", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@ARD", SqlDbType.VarChar, 20);
                        cmd.Parameters.Add("@OPN", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@CCD", SqlDbType.Char, 1);
                        cmd.Parameters.Add("@PTC", SqlDbType.Char, 3);

                        cmd.Parameters["@SNM"].Value = SNM;
                        cmd.Parameters["@DLC"].Value = DLC;
                        cmd.Parameters["@ALC"].Value = ALC;
                        cmd.Parameters["@ROT"].Value = ROT;
                        cmd.Parameters["@DTD"].Value = DTD;
                        cmd.Parameters["@ARD"].Value = ARD;
                        cmd.Parameters["@OPN"].Value = OPN;
                        cmd.Parameters["@CCD"].Value = CCD;
                        cmd.Parameters["@PTC"].Value = PTC;

                        adp.Fill(ds, "item");

                        XmlDoc.LoadXml(ds.GetXml().Replace(" xml:space=\"preserve\"", "").Replace("&lt;", "<").Replace("&gt;", ">"));
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
        //[WebMethod(Description = "프로모션 상세정보")]
        public XmlElement SearchPromotionDetail(int PMID)
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

                sqlParam[0].Value = 35;
                sqlParam[1].Value = 0;
                sqlParam[2].Value = "";
                sqlParam[3].Value = Environment.MachineName;
                sqlParam[4].Value = hcc.Request.HttpMethod;
                sqlParam[5].Value = hcc.Request.UserHostAddress;
                sqlParam[6].Value = LogGUID;
                sqlParam[7].Value = PMID;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

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
                            cmd.CommandTimeout = 10;
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
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, mc.Name, MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "프로모션"

        #region "함수"

        /// <summary>
        /// 실행시간 체크
        /// </summary>
        /// <param name="RunTimeNode">XMLNode</param>
        /// <param name="sw">Stopwatch</param>
        /// <param name="ServiceName">서비스명</param>
        protected void CheckRunTimeEnd(XmlNode RunTimeNode, Stopwatch sw, string ServiceName)
        {
            sw.Stop();

            XmlNode NewRunTimeService = RunTimeNode.AppendChild(RunTimeService.CloneNode(true));

            NewRunTimeService.Attributes.GetNamedItem("name").InnerText = ServiceName;
            NewRunTimeService.Attributes.GetNamedItem("time").InnerText = String.Format("{0:#00}.{1:00}", sw.Elapsed.TotalSeconds, (sw.Elapsed.Milliseconds / 10));
        }

        #endregion "함수"
    }
}