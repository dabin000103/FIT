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
using System.Xml.Linq;

namespace AirWebService
{
	/// <summary>
	/// 실시간 항공(해외) 예약 관리를 위한 웹서비스(통합용)
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class AirService2 : System.Web.Services.WebService
	{
		Common cm;
		ModeConfig mc;
        AirService asv;
		AmadeusAirService amd;
        SabreAirService sas;
        AES256Cipher aes;
        LogSave log;
		HttpContext hcc;
		XmlNode RunTime;
		XmlNode RunTimeService;
        
		public AirService2()
		{
			cm = new Common();
			mc = new ModeConfig();
            asv = new AirService();
			amd = new AmadeusAirService();
            sas = new SabreAirService();
            aes = new AES256Cipher();
            log = new LogSave();
			hcc = HttpContext.Current;

			//실행시간 체크용
			XmlDocument XmlCheck = new XmlDocument();
			XmlCheck.Load(mc.XmlFullPath("CheckRunTime"));
			RunTime = XmlCheck.SelectSingleNode("ResponseDetails/runTime");
			RunTimeService = RunTime.SelectSingleNode("service");
		}

        #region "Fare + Availability 동시조회"

        [WebMethod(Description = "Fare + Availability 동시조회(개발테스트용)")]
		public XmlElement SearchFareAvailRSTEST(string SNM)
        {
            string[] PTC = new String[3] { "ADT", "CHD", "INF" };
            int[] NOP = new Int32[3] { 1, 0, 0 };

            return SearchFareAvailRS((String.IsNullOrWhiteSpace(SNM) ? 2 : Convert.ToInt32(SNM)), "", "SEL", "NRT", "RT", "20190215", "20190220", "N", "", "M", "", PTC, NOP, 50, "WEBSERVICE", "");
            //return SearchFareAvailRS((String.IsNullOrWhiteSpace(SNM) ? 2 : Convert.ToInt32(SNM)), "", "SEL,HNL", "LAX,SEL", "DT", "20180527", "20180530", "N", "", "M", "", PTC, NOP, 50, "WEBSERVICE", "");
        }

        /// <summary>
        /// Fare + Availability 동시조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="PTC">탑승객 타입 코드(ADT, CHD, INF, STU, LBR)(단, 첫번째 배열에는 ADT, STU, LBR만 가능)</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="NRR">응답 결과 수(Default:200)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <param name="FTX">free text</param>
        [WebMethod(Description = "Fare + Availability 동시조회")]
        public XmlElement SearchFareAvailRS(int SNM, string SAC, string DLC, string ALC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string[] PTC, int[] NOP, int NRR, string RQT, string FTX)
        {
            int ServiceNumber = 518;
            string LogGUID = cm.GetGUID;

            #region "파라미터 로그 기록"

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
                        new SqlParameter("@요청14", SqlDbType.VarChar, 3000)
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
                sqlParam[13].Value = OPN;
                sqlParam[14].Value = FLD;
                sqlParam[15].Value = CCD;
                sqlParam[16].Value = ACQ;
                sqlParam[17].Value = String.Join("^", PTC);
                sqlParam[18].Value = String.Join("^", NOP);
                sqlParam[19].Value = NRR;
                sqlParam[20].Value = FTX;

                log.LogDBSave(sqlParam);
            }
            catch (Exception) { }
            finally { }

            #endregion "파라미터 로그 기록"

            try
            {
                #region "검색조건 정의"

                //응답결과수
                if (NRR.Equals(0))
                    NRR = 50;

                if (OPN.Equals("Y"))
                {
                    ARD = cm.OpenDate(DTD, ARD);
                }

                int ftxIdx = 0;
                string PUB = "N";   //PUB요금 출력여부(기본값으로 PUB운임 제외,2014-10-29,김지영과장요청)
                int WLR = 0;        //대기예약 포함 비율
                string LTD = "Y";   //발권마감일 체크여부
                string FTR = "Y";   //필터링 사용여부(발권마감일 체크 포함)
                string MTL = "N";   //ModeTL 적용여부
                string PRM = "Y";   //프로모션 적용여부
                string DEV = "N";   //개발서버용 여부

                //free text : PUB | WLR | LTD | FTR | MTL | PRM | DEV
                if (!String.IsNullOrWhiteSpace(FTX))
                {
                    foreach (string StrFTX in FTX.Split('|'))
                    {
                        if (ftxIdx.Equals(0))
                            PUB = (StrFTX.Equals("Y")) ? "Y" : "N";
                        else if (ftxIdx.Equals(1))
                            WLR = cm.RequestInt(StrFTX);
                        else if (ftxIdx.Equals(2))
                            LTD = (StrFTX.Equals("N")) ? "N" : "Y";
                        else if (ftxIdx.Equals(3))
                            FTR = (StrFTX.Equals("N")) ? "N" : "Y";
                        else if (ftxIdx.Equals(4))
                            MTL = (StrFTX.Equals("Y")) ? "Y" : "N";
                        else if (ftxIdx.Equals(5))
                            PRM = (StrFTX.Equals("N")) ? "N" : "Y";
                        else if (ftxIdx.Equals(6))
                            DEV = (StrFTX.Equals("Y")) ? "Y" : "N";

                        ftxIdx++;
                    }
                }

                //최초출발지
                string DepartureAirport = DLC.Split(',')[0];

                //최종도착지(목적지)
                string DestinationAirport = ALC.Split(',')[0];

                //한국출발여부
                bool DepartureFromKorea = Common.KoreaOfAirport(DepartureAirport.Trim());

                //성인요금 검색이 아닐경우 프리미엄이코노미는 검색 제외
                if (!PTC[0].Equals("ADT"))
                {
                    if (String.IsNullOrWhiteSpace(CCD))
                        CCD = "M,C,F";
                    else if (CCD.Equals("Y"))
                        CCD = "M";
                }

                //일반석 요청시 프리미엄이코노미 가능 지역해 한해서만 'Y' 처리
                if (CCD.Equals("Y"))
                {
                    if (!Common.PremiumEconomy(ALC))
                        CCD = "M";
                }

                //국내선 예약불가(2015-04-20 추가)
                if (ROT.Equals("RT") && DepartureFromKorea && Common.KoreaOfAirport(DestinationAirport.Trim()))
                {
                    throw new Exception("요청하신 서비스는 해외 전용으로 국내선은 예약할 수 없습니다.");
                }

                //모두닷컴(2,3915),스카이스캐너(4664,4837)는 PUB운임 오픈(2018-04-03,김경미차장)
                //전 사이트 PUB운임 오픈(2018-04-05,김경미차장)
                //if (SNM.Equals(2) || SNM.Equals(3915) || SNM.Equals(4664) || SNM.Equals(4837))
                //{
                    PUB = "Y";
                //}

                //ABS 예외사항
                if (SNM.Equals(68))
                {
                    //발권마감일 체크를 하지 않는다.(2014-09-30,김지영과장요청)
                    LTD = "N";

                    //대기예약 30% 지정(2015-01-09,김지영과장요청)
                    WLR = 30;
                }
                else
                {
                    //도착지가 필리핀(PH)일 경우 한국출발에 한해 왕복일 경우에만 예약가능(2014-12-24,정성하대리요청)
                    if (Common.PhilippinesOfAirport(DestinationAirport.Trim()))
                    {
                        if (ROT.Equals("OW"))
                            throw new Exception("필리핀은 왕복인 경우에만 항공예약을 진행할 수 있습니다.");
                        else
                        {
                            if (OPN.Equals("N"))
                            {
                                if (!DepartureFromKorea)
                                    throw new Exception("필리핀은 한국출발이면서 왕복인 경우에만 예약을 진행할 수 있습니다.");
                            }
                            else
                                throw new Exception("필리핀은 귀국일 미지정(오픈)인 경우 예약을 진행할 수 없습니다.");
                        }
                    }
                    //도착지가 미주(US)일 경우 편도(국적기), 왕복(전항공)일 경우에만 예약가능(2014-12-24,정성하대리요청)
                    else if (DepartureFromKorea && Common.UnitedStatesOfAirport(DestinationAirport.Trim()))
                    {
                        if (ROT.Equals("OW") || OPN.Equals("Y"))
                        {
                            if (String.IsNullOrWhiteSpace(SAC))
                                SAC = "KE,OZ";
                            else
                            {
                                string SAC2 = string.Empty;

                                foreach (string TmpSAC in SAC.Split(','))
                                {
                                    if (TmpSAC.Trim().Equals("KE") || TmpSAC.Trim().Equals("OZ"))
                                        SAC2 += String.Concat((String.IsNullOrWhiteSpace(SAC2) ? "" : ","), TmpSAC.Trim());
                                }

                                SAC = SAC2;
                            }
                        }
                    }
                }

                //해외출발(SOTO)
                if (!DepartureFromKorea)
                {
                    //해외출발일 경우 PUB 포함(2014-11-06,김지영과장요청)
                    PUB = "Y";

                    //출발지가 필리핀(PH)일 경우 KE,OZ,VN항공사만 예약가능(2015-03-24,TOPAS요청)
                    if (Common.PhilippinesOfAirport(DepartureAirport.Trim()))
                    {
                        if (String.IsNullOrWhiteSpace(SAC))
                            SAC = "KE,OZ,VN";
                        else
                        {
                            string SAC2 = string.Empty;

                            foreach (string TmpSAC in SAC.Split(','))
                            {
                                if (TmpSAC.Trim().Equals("KE") || TmpSAC.Trim().Equals("OZ") || TmpSAC.Trim().Equals("VN"))
                                    SAC2 += String.Concat((String.IsNullOrWhiteSpace(SAC2) ? "" : ","), TmpSAC.Trim());
                            }

                            SAC = SAC2;
                        }
                    }
                }

                #endregion "검색조건 정의"

                //11번가/티몬 다구간은 PUB 오픈(2017-01-23,정성하과장)
                if (ROT.Equals("MD") && (SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(4924) || SNM.Equals(4929))) //티몬(4925,4926),11번가(4924,4929)
                    PUB = "Y";

                //요청정보
                string ReqInfo = String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}^{20}", SNM, SAC, DLC, ALC, "", ROT, Common.ConvertToOnlyNumber(DTD), Common.ConvertToOnlyNumber(ARD), OPN, FLD, CCD, ACQ, "", String.Join("/", PTC), String.Join("/", NOP), NRR, PUB, WLR, LTD, FTR, MTL);
                
                //결과 XML - DB에 저장된 동일 조건의 결과가 있는지 체크
                XmlElement XmlMode = SearchFareListDBSelect(SNM, DEV, ReqInfo);
                
                if (XmlMode == null || !XmlMode.HasChildNodes || XmlMode.SelectNodes("errorSource").Count > 0)
                {
                    //모두닷컴모바일(3915)만 세이버 사용 가능
                    //티몬(4925,4926),11번가(4924,4929) 추가(2018-03-29,김지영차장)
                    //티몬(4925,4926),11번가(4924,4929) 제외(2018-03-30,김지영차장)
                    if (SNM.Equals(3915))
                    {
                        XmlDocument XmlDoc = new XmlDocument();
                        XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                        XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                        XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("guid").InnerText = LogGUID;

                        XmlNode ResponseDetails = XmlDoc.SelectSingleNode("ResponseDetails");
                        ResponseDetails.RemoveChild(ResponseDetails.SelectSingleNode("flightInfo"));

                        XmlNode PriceInfo = ResponseDetails.SelectSingleNode("priceInfo");
                        PriceInfo.RemoveChild(PriceInfo.SelectSingleNode("priceIndex"));

                        SearchFareAvailGrouping sfag = new SearchFareAvailGrouping();

                        foreach (XmlElement FareAvail in sfag.GetFareAvail(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, PTC, NOP, NRR, PUB, WLR, LTD, FTR, MTL, LogGUID))
                        {
                            if (FareAvail != null && FareAvail.SelectNodes("errorSource").Count.Equals(0))
                            {
                                //항공검색소스 번호
                                XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText += String.Concat((!String.IsNullOrWhiteSpace(XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText) ? "," : ""), FareAvail.Attributes.GetNamedItem("ref").InnerText);

                                if (FareAvail.SelectNodes("flightInfo").Count > 0)
                                {
                                    if (ResponseDetails.SelectNodes("flightInfo").Count.Equals(0))
                                        ResponseDetails.InsertBefore(XmlDoc.ImportNode(FareAvail.SelectSingleNode("flightInfo"), true), PriceInfo);
                                    else
                                    {
                                        foreach (XmlNode FlightIndex in FareAvail.SelectNodes("flightInfo/flightIndex"))
                                        {
                                            XmlNode TmpFlightIndex = ResponseDetails.SelectSingleNode(String.Format("flightInfo/flightIndex[@ref='{0}']", FlightIndex.Attributes.GetNamedItem("ref").InnerText));

                                            foreach (XmlNode SegGroup in FlightIndex.SelectNodes("segGroup"))
                                            {
                                                TmpFlightIndex.AppendChild(XmlDoc.ImportNode(SegGroup, true));
                                            }
                                        }
                                    }

                                    foreach (XmlNode PriceIndex in FareAvail.SelectNodes("priceInfo/priceIndex"))
                                    {
                                        PriceInfo.AppendChild(XmlDoc.ImportNode(PriceIndex, true));
                                    }
                                }

                                try
                                {
                                    if (FareAvail.SelectNodes("runTime").Count > 0)
                                    {
                                        XmlAttribute AttrPTC = FareAvail.OwnerDocument.CreateAttribute("ptc");
                                        AttrPTC.InnerText = (FareAvail.SelectNodes("flightInfo").Count > 0) ? FareAvail.SelectSingleNode("flightInfo").Attributes.GetNamedItem("ptc").InnerText : "";

                                        XmlAttribute AttrCabin = FareAvail.OwnerDocument.CreateAttribute("cabin");
                                        AttrCabin.InnerText = (FareAvail.SelectNodes("priceInfo").Count > 0) ? FareAvail.SelectSingleNode("priceInfo/priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/cabin").Attributes.GetNamedItem("cabin").InnerText : "";

                                        FareAvail.SelectSingleNode("runTime").Attributes.Append(AttrPTC);
                                        FareAvail.SelectSingleNode("runTime").Attributes.Append(AttrCabin);

                                        ResponseDetails.AppendChild(XmlDoc.ImportNode(FareAvail.SelectSingleNode("runTime"), true));
                                    }
                                }
                                catch (Exception) { }
                            }
                        }

                        if (!PriceInfo.HasChildNodes)
                            throw new Exception("항공요금 검색 결과가 없습니다.");

                        XmlMode = XmlDoc.DocumentElement;
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(CCD) || CCD.Equals("Y"))
                        {
                            XmlDocument XmlDoc = new XmlDocument();
                            XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                            XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("guid").InnerText = LogGUID;

                            XmlNode ResponseDetails = XmlDoc.SelectSingleNode("ResponseDetails");
                            ResponseDetails.RemoveChild(ResponseDetails.SelectSingleNode("flightInfo"));

                            XmlNode PriceInfo = ResponseDetails.SelectSingleNode("priceInfo");
                            PriceInfo.RemoveChild(PriceInfo.SelectSingleNode("priceIndex"));

                            SearchFareAvailCabin2 sfa = new SearchFareAvailCabin2();

                            foreach (XmlElement FareAvail in sfa.GetFareAvail(SNM, SAC, DLC, ALC, ROT, DTD, ARD, OPN, FLD, CCD, ACQ, PTC, NOP, NRR, PUB, WLR, LTD, FTR, MTL, LogGUID))
                            {
                                if (FareAvail != null && FareAvail.SelectNodes("errorSource").Count.Equals(0))
                                {
                                    //항공검색소스 번호
                                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText += String.Concat((!String.IsNullOrWhiteSpace(XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText) ? "," : ""), FareAvail.Attributes.GetNamedItem("ref").InnerText);

                                    if (FareAvail.SelectNodes("flightInfo").Count > 0)
                                    {
                                        if (ResponseDetails.SelectNodes("flightInfo").Count.Equals(0))
                                            ResponseDetails.InsertBefore(XmlDoc.ImportNode(FareAvail.SelectSingleNode("flightInfo"), true), PriceInfo);
                                        else
                                        {
                                            foreach (XmlNode FlightIndex in FareAvail.SelectNodes("flightInfo/flightIndex"))
                                            {
                                                XmlNode TmpFlightIndex = ResponseDetails.SelectSingleNode(String.Format("flightInfo/flightIndex[@ref='{0}']", FlightIndex.Attributes.GetNamedItem("ref").InnerText));

                                                foreach (XmlNode SegGroup in FlightIndex.SelectNodes("segGroup"))
                                                {
                                                    TmpFlightIndex.AppendChild(XmlDoc.ImportNode(SegGroup, true));
                                                }
                                            }
                                        }

                                        foreach (XmlNode PriceIndex in FareAvail.SelectNodes("priceInfo/priceIndex"))
                                        {
                                            PriceInfo.AppendChild(XmlDoc.ImportNode(PriceIndex, true));
                                        }
                                    }

                                    try
                                    {
                                        if (FareAvail.SelectNodes("runTime").Count > 0)
                                        {
                                            XmlAttribute AttrPTC = FareAvail.OwnerDocument.CreateAttribute("ptc");
                                            AttrPTC.InnerText = (FareAvail.SelectNodes("flightInfo").Count > 0) ? FareAvail.SelectSingleNode("flightInfo").Attributes.GetNamedItem("ptc").InnerText : "";

                                            XmlAttribute AttrCabin = FareAvail.OwnerDocument.CreateAttribute("cabin");
                                            AttrCabin.InnerText = (FareAvail.SelectNodes("priceInfo").Count > 0) ? FareAvail.SelectSingleNode("priceInfo/priceIndex/paxFareGroup/paxFare/segFareGroup/segFare/fare/cabin").Attributes.GetNamedItem("cabin").InnerText : "";

                                            FareAvail.SelectSingleNode("runTime").Attributes.Append(AttrPTC);
                                            FareAvail.SelectSingleNode("runTime").Attributes.Append(AttrCabin);

                                            ResponseDetails.AppendChild(XmlDoc.ImportNode(FareAvail.SelectSingleNode("runTime"), true));
                                        }
                                    }
                                    catch (Exception) { }
                                }
                            }

                            if (!PriceInfo.HasChildNodes)
                                throw new Exception("항공요금 검색 결과가 없습니다.");

                            XmlMode = XmlDoc.DocumentElement;
                        }
                        else
                        {
                            XmlMode = SearchFareAvailAmadeusRS(SNM, SAC, DLC, ALC, "", ROT, DTD, ARD, OPN, FLD, CCD, ACQ, "", PTC, NOP, NRR, PUB, WLR, LTD, FTR, MTL, LogGUID);

                            if (!XmlMode.SelectSingleNode("priceInfo").HasChildNodes)
                                throw new Exception("항공요금 검색 결과가 없습니다.");
                        }
                    }
                    
                    //검색 결과 DB 저장
                    XmlMode = SearchFareListDBSaveAndSelect(SNM, LogGUID, DEV, ReqInfo, XmlMode);
                }

                //티몬(4925,4926),11번가(4924,4929),지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
                if (SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                    return SearchFareAvailAllianceLandingInfo(SNM, ROT, PTC, NOP, DEV, XmlMode);
                else
                    return XmlMode;
            }
            catch (Exception ex)
            {
                XmlElement XmlError = new MWSExceptionMode(ex, hcc, LogGUID, "AirService2", MethodBase.GetCurrentMethod().Name, ServiceNumber, 0, 0).ToErrors;

                //티몬(4925,4926),11번가(4924,4929),지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
                if (SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.LoadXml(String.Format("<result><statusInfo><status>FAILE</status><returnMessage><![CDATA[{0}]]></returnMessage><datetime>{1}</datetime></statusInfo></result>", ex.Message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                    return XmlDoc.DocumentElement;
                }
                else
                    return XmlError;
            }
        }

        /// <summary>
        /// 제휴사일 경우 랜딩용 페이지 링크 및 파라미터 정보 추가 작업
        /// </summary>
        /// <param name="SNM"></param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="PTC">탑승객 타입 코드(ADT, CHD, INF, STU, LBR)(단, 첫번째 배열에는 ADT, STU, LBR만 가능)</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="DEV">개발서버용 여부</param>
        /// <param name="XmlMode"></param>
        /// <returns></returns>
        public XmlElement SearchFareAvailAllianceLandingInfo(int SNM, string ROT, string[] PTC, int[] NOP, string DEV, XmlElement XmlMode)
        {
            int ADC = 0;
            int CHC = 0;
            int IFC = 0;

            for (int i = 0; i < PTC.Length; i++)
            {
                if (PTC[i].Trim().Equals("CHD"))
                    CHC = NOP[i];
                else if (PTC[i].Trim().Equals("INF"))
                    IFC = NOP[i];
                else
                    ADC = NOP[i];
            }

            //티몬(4925,4926)
            if (SNM.Equals(4925) || SNM.Equals(4926))
            {
                string SKey = XmlMode.SelectSingleNode("rqInfo/property[@key='KSESID']").InnerText;

                foreach (XmlNode Mapping in XmlMode.SelectNodes("data/b2C_AirTmo_Controller_AIRTMOINTSCH010012001001_RS/fareScheduleSearchServiceRS/fareScheduleSearchProcessRS/MAPPINGS/MAPPING"))
                {
                    string Params = String.Format("MKEY={0}&SNM={1}&ADC={2}&CHC={3}&IFC={4}", Server.UrlEncode(aes.AESEncrypt(AES256Cipher.KeyName(SNM), String.Format("{0}::{1}::{2}::{3}::{4}::{5}::{6}::{7}::{8}",
                                                                                                                    SKey,
                                                                                                                    SNM,
                                                                                                                    ROT,
                                                                                                                    PTC[0],
                                                                                                                    Mapping.Attributes.GetNamedItem("FN").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN1").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN2").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN3").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN4").InnerText))), SNM, ADC, CHC, IFC);

                    Mapping.Attributes.GetNamedItem("LANDINGPARAM").InnerText = Params;
                    Mapping.Attributes.GetNamedItem("RULEPARAM").InnerText = Params;
                }
            }
            //11번가(4924,4929)
            else if (SNM.Equals(4924) || SNM.Equals(4929))
            {
                string SKey = XmlMode.SelectSingleNode("rqInfo/property[@key='KSESID']").InnerText;

                foreach (XmlNode Mapping in XmlMode.SelectNodes("data/b2c_AirSkm_Controller_AIRSKMINTSCH010012001001_RS/fareScheduleSearchServiceRS/fareScheduleSearchProcessRS/MAPPINGS/MAPPING"))
                {
                    string Params = String.Format("MKEY={0}&SNM={1}&ADC={2}&CHC={3}&IFC={4}", Server.UrlEncode(aes.AESEncrypt(AES256Cipher.KeyName(SNM), String.Format("{0}::{1}::{2}::{3}::{4}::{5}::{6}::{7}::{8}",
                                                                                                                    SKey,
                                                                                                                    SNM,
                                                                                                                    ROT,
                                                                                                                    PTC[0],
                                                                                                                    Mapping.Attributes.GetNamedItem("FN").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN1").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN2").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN3").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN4").InnerText))), SNM, ADC, CHC, IFC);

                    Mapping.Attributes.GetNamedItem("LANDINGPARAM").InnerText = Params;
                    Mapping.Attributes.GetNamedItem("RULEPARAM").InnerText = Params;
                }
            }
            //지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
            else if (SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
            {
                string SKey = XmlMode.SelectSingleNode("rqInfo/property[@key='KSESID']").InnerText;

                foreach (XmlNode Mapping in XmlMode.SelectNodes("data/b2c_AirSkm_Controller_AIRSKMINTSCH010012001001_RS/fareScheduleSearchServiceRS/fareScheduleSearchProcessRS/MAPPINGS/MAPPING"))
                {
                    string Params = String.Format("MKEY={0}&SNM={1}&ADC={2}&CHC={3}&IFC={4}", Server.UrlEncode(aes.AESEncrypt(AES256Cipher.KeyName(SNM), String.Format("{0}::{1}::{2}::{3}::{4}::{5}::{6}::{7}::{8}",
                                                                                                                    SKey,
                                                                                                                    SNM,
                                                                                                                    ROT,
                                                                                                                    PTC[0],
                                                                                                                    Mapping.Attributes.GetNamedItem("FN").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN1").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN2").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN3").InnerText,
                                                                                                                    Mapping.Attributes.GetNamedItem("AN4").InnerText))), SNM, ADC, CHC, IFC);

                    Mapping.Attributes.GetNamedItem("LANDINGPARAM").InnerText = Params;
                    Mapping.Attributes.GetNamedItem("RULEPARAM").InnerText = Params;
                }
            }

            return XmlMode;
        }

        /// <summary>
        /// MP Instant Search(MPIS)(아마데우스)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드(여정구분은 콤마, SEG구분은 슬래시, ex:NRT/SIN,SIN/NRT,RON)</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <param name="PUB">PUB요금 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="LTD">발권마감일 체크여부</param>
        /// <param name="FTR">필터링 사용여부(발권마감일 체크 포함)</param>
        /// <param name="MTL">ModeTL 적용여부</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public XmlElement InstantSearchAmadeusRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string[] PTC, int[] NOP, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID)
        {
            try
            {
                //요청정보
                string ReqInfo = String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}^{20}", SNM, SAC, DLC, ALC, CLC, ROT, Common.ConvertToOnlyNumber(DTD), Common.ConvertToOnlyNumber(ARD), OPN, FLD, CCD, ACQ, FAB, String.Join("/", PTC), String.Join("/", NOP), NRR, PUB, WLR, LTD, FTR, MTL);

                //결과 XML
                XmlElement XmlMode;

                //결과 XML - DB에 저장된 동일 조건의 결과가 있는지 체크
                XmlElement ResXml = SearchFareListGDSDBSelect("Amadeus", SNM, ReqInfo);

                //네임스페이스 정의
                XmlNamespaceManager xnMgr;

                Stopwatch sw;
                XmlNode NewRunTime = RunTime.CloneNode(false);

                if (ResXml == null)
                {
                    try
                    {
                        sw = Stopwatch.StartNew();
                        ResXml = amd.InstantTravelBoardSearchRS(String.Format("{0}-{1}-{2}", GUID, PTC[0], CCD), SNM, SAC, DLC, ALC, "", ROT, DTD, ARD, OPN, "", CCD, PTC, NOP, ACQ, "", PUB, WLR, MTL, NRR);
                        CheckRunTimeEnd(NewRunTime, sw, "InstantTravelBoardSearchRS");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }

                    //오류 결과일 경우 예외 처리
                    xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                    xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_InstantTravelBoardSearch"));

                    if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                    {
                        if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                            throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                        else
                            throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                    }

                    //검색 결과 DB 저장
                    ResXml = SearchFareListGDSDBSave("Amadeus", SNM, GUID, ReqInfo, ResXml);
                }
                else
                {
                    xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                    xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_InstantTravelBoardSearch"));
                }

                //프로모션 정보
                XmlElement PromXml = null;

                if (!ROT.Equals("MD"))
                {
                    PromXml = SearchPromotionList(SNM, DLC, ALC, ROT, DTD, ARD, OPN, (CCD.Equals("C") || CCD.Equals("F")) ? CCD : "Y", PTC[0]);
                    cm.XmlFileSave(PromXml, mc.Name, "SearchPromotionList", "Y", GUID);
                }

                //통합구성
                sw = Stopwatch.StartNew();
                XmlMode = asv.ToModeSearchFareAvailRS(SNM, ResXml, PromXml, xnMgr, DLC, ALC, ROT, CCD, DTD, PUB, LTD, FTR, "0D", "MPIS", GUID);
                CheckRunTimeEnd(NewRunTime, sw, "ToModeInstantSearchRS");

                //실행시간 데이타 XML에 추가
                XmlMode.AppendChild(XmlMode.OwnerDocument.ImportNode(NewRunTime, true));
                cm.XmlFileSave(XmlMode, mc.Name, "ToModeInstantSearchRS", "Y", GUID);

                return XmlMode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Fare + Availability 동시조회(상세 조건 조회)(아마데우스)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드(여정구분은 콤마, SEG구분은 슬래시, ex:NRT/SIN,SIN/NRT,RON)</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <param name="PUB">PUB요금 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="LTD">발권마감일 체크여부</param>
        /// <param name="FTR">필터링 사용여부(발권마감일 체크 포함)</param>
        /// <param name="MTL">ModeTL 적용여부</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public XmlElement SearchFareAvailAmadeusRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string[] PTC, int[] NOP, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID)
        {
            try
            {
                //요청정보
                string ReqInfo = String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}^{20}", SNM, SAC, DLC, ALC, CLC, ROT, Common.ConvertToOnlyNumber(DTD), Common.ConvertToOnlyNumber(ARD), OPN, FLD, CCD, ACQ, FAB, String.Join("/", PTC), String.Join("/", NOP), NRR, PUB, WLR, LTD, FTR, MTL);

                //결과 XML
                XmlElement XmlMode;

                //결과 XML - DB에 저장된 동일 조건의 결과가 있는지 체크
                XmlElement ResXml = SearchFareListGDSDBSelect("Amadeus", SNM, ReqInfo);

                //네임스페이스 정의
                XmlNamespaceManager xnMgr;

                Stopwatch sw;
                XmlNode NewRunTime = RunTime.CloneNode(false);

                if (ResXml == null)
                {
                    string SID = string.Empty;
                    string SQN = string.Empty;
                    string SCT = string.Empty;

                    try
                    {
                        sw = Stopwatch.StartNew();
                        XmlElement Session = amd.SessionCreate(SNM, String.Format("{0}-{1}-{2}-01", GUID, PTC[0], CCD));
                        CheckRunTimeEnd(NewRunTime, sw, "SessionCreate");

                        SID = Session.SelectSingleNode("session/sessionId").InnerText;
                        SQN = Session.SelectSingleNode("session/sequenceNumber").InnerText;
                        SCT = Session.SelectSingleNode("session/securityToken").InnerText;

                        sw = Stopwatch.StartNew();
                        ResXml = amd.MasterPricerTravelBoardSearchRS(SID, SQN, SCT, String.Format("{0}-{1}-{2}-02", GUID, PTC[0], CCD), SNM, SAC, DLC, ALC, "", ROT, DTD, ARD, OPN, FLD, CCD, PTC, NOP, ACQ, FAB, PUB, WLR, MTL, NRR);
                        CheckRunTimeEnd(NewRunTime, sw, "MasterPricerTravelBoardSearchRS");

                        sw = Stopwatch.StartNew();
                        amd.SessionClose(SID, SCT, String.Format("{0}-{1}-{2}-03", GUID, PTC[0], CCD));
                        CheckRunTimeEnd(NewRunTime, sw, "SessionClose");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally
                    {
                        if (!String.IsNullOrWhiteSpace(SID))
                        {
                            sw = Stopwatch.StartNew();
                            amd.SessionClose(SID, SCT, String.Format("{0}-{1}-{2}-03_Err", GUID, PTC[0], CCD));
                            CheckRunTimeEnd(NewRunTime, sw, "SessionClose");
                        }
                    }

                    //오류 결과일 경우 예외 처리
                    xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                    xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch"));

                    if (ResXml.SelectNodes("m:errorMessage", xnMgr).Count > 0)
                    {
                        if (ResXml.SelectNodes("m:errorMessage/m:errorMessageText", xnMgr).Count > 0)
                            throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:errorMessageText/m:description", xnMgr).InnerText);
                        else
                            throw new Exception(ResXml.SelectSingleNode("m:errorMessage/m:applicationError/m:applicationErrorDetail/m:error", xnMgr).InnerText);
                    }

                    //MaxStay(유효기간)
                    XmlAttribute MaxStay;
                    string DepartureDate = cm.RequestDateTime(DTD.Split(',')[0].Trim());

                    foreach (XmlNode Value in ResXml.SelectNodes("m:value", xnMgr))
                    {
                        MaxStay = Value.Attributes.Append(ResXml.OwnerDocument.CreateAttribute("maxStay"));
                        MaxStay.InnerText = Common.ConvertToMaxStay(DepartureDate, Value.SelectSingleNode("m:criteriaDetails/m:value", xnMgr).InnerText);
                    }

                    //검색 결과 DB 저장
                    ResXml = SearchFareListGDSDBSave("Amadeus", SNM, GUID, ReqInfo, ResXml);
                }
                else
                {
                    xnMgr = new XmlNamespaceManager(ResXml.OwnerDocument.NameTable);
                    xnMgr.AddNamespace("m", AmadeusConfig.NamespaceURL("Fare_MasterPricerTravelBoardSearch"));
                }

                //프로모션 정보
                XmlElement PromXml = null;

                if (!ROT.Equals("MD"))
                {
                    PromXml = SearchPromotionList(SNM, DLC, ALC, ROT, DTD, ARD, OPN, (CCD.Equals("C") || CCD.Equals("F")) ? CCD : "Y", PTC[0]);
                    cm.XmlFileSave(PromXml, mc.Name, "SearchPromotionList", "Y", GUID);
                }

                //통합구성
                sw = Stopwatch.StartNew();
                XmlMode = asv.ToModeSearchFareAvailRS(SNM, ResXml, PromXml, xnMgr, DLC, ALC, ROT, CCD, DTD, PUB, LTD, FTR, "", "", GUID);
                CheckRunTimeEnd(NewRunTime, sw, "ToModeSearchFareAvailRS");

                //실행시간 데이타 XML에 추가
                XmlMode.AppendChild(XmlMode.OwnerDocument.ImportNode(NewRunTime, true));
                cm.XmlFileSave(XmlMode, mc.Name, "ToModeSearchFareAvailRS", "Y", GUID);

                return XmlMode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Fare + Availability 동시조회(상세 조건 조회)(세이버)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">항공사 코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="CLC">경유지 공항 코드(여정구분은 콤마, SEG구분은 슬래시, ex:NRT/SIN,SIN/NRT,RON)</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="ARD">도착일(YYYYMMDD)</param>
        /// <param name="OPN">오픈여부(YN)</param>
        /// <param name="FLD">여정추가옵션(C:Connecting Service, D:Direct Service, N:Non-Stop Service, OV:Overnight not allowed)</param>
        /// <param name="CCD">캐빈 클래스(Y:Economic, M:Economic Standard, W:Economic Premium, C:Business, F:First/Supersonic)</param>
        /// <param name="ACQ">여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정(A:공항, C:도시, 공백:자동인식)</param>
        /// <param name="FAB">Fare Basis</param>
        /// <param name="PTC">탑승객 타입 코드</param>
        /// <param name="NOP">탑승객 수</param>
        /// <param name="NRR">응답 결과 수</param>
        /// <param name="PUB">PUB요금 출력여부</param>
        /// <param name="WLR">대기예약 포함 비율</param>
        /// <param name="LTD">발권마감일 체크여부</param>
        /// <param name="FTR">필터링 사용여부(발권마감일 체크 포함)</param>
        /// <param name="MTL">ModeTL 적용여부</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        public XmlElement SearchFareAvailSabreRS(int SNM, string SAC, string DLC, string ALC, string CLC, string ROT, string DTD, string ARD, string OPN, string FLD, string CCD, string ACQ, string FAB, string[] PTC, int[] NOP, int NRR, string PUB, int WLR, string LTD, string FTR, string MTL, string GUID)
        {
            try
            {
                //요청정보
                string ReqInfo = String.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}^{11}^{12}^{13}^{14}^{15}^{16}^{17}^{18}^{19}^{20}", SNM, SAC, DLC, ALC, CLC, ROT, Common.ConvertToOnlyNumber(DTD), Common.ConvertToOnlyNumber(ARD), OPN, FLD, CCD, ACQ, FAB, String.Join("/", PTC), String.Join("/", NOP), NRR, PUB, WLR, LTD, FTR, MTL);

                //결과 XML
                XmlElement XmlMode;

                //결과 XML - DB에 저장된 동일 조건의 결과가 있는지 체크
                XmlElement ResXml = SearchFareListGDSDBSelect("Sabre", SNM, ReqInfo);

                Stopwatch sw;
                XmlNode NewRunTime = RunTime.CloneNode(false);

                int ADC = 0;
                int CHC = 0;
                int IFC = 0;

                if (ResXml == null)
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
                    else if (ROT.Equals("DT"))
                    {
                        string[] DLCs = DLC.Split(',');
                        string[] ALCs = ALC.Split(',');

                        Dep[0] = DLCs[0];
                        Arr[0] = ALCs[0];
                        DepDate[0] = Common.ConvertToOnlyNumber(DTD);

                        Dep[1] = DLCs[1];
                        Arr[1] = ALCs[1];
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

                    for (int i = 0; i < PTC.Length; i++)
                    {
                        if (PTC[i].Equals("INF"))
                            IFC = NOP[i];
                        else if (PTC[i].Equals("CHD"))
                            CHC = NOP[i];
                        else
                            ADC = NOP[i];
                    }

                    //MaxFare 수 조정
                    NRR = (NRR > 100) ? 10 : 5;

                    sw = Stopwatch.StartNew();
                    ResXml = sas.SearchFareAvailFMSRS(ROT, Dep, Arr, DepDate, RetDate, (OPN.Equals("Y") ? ARD : ""), (CCD.Equals("M") ? "Y" : (CCD.Equals("W") ? "P" : CCD)), ADC, CHC, IFC, SAC.Replace(",", "/"), "", NRR, String.Format("{0}-{1}-{2}", GUID, PTC[0], CCD));
                    CheckRunTimeEnd(NewRunTime, sw, "SearchFareAvailRS");

                    //오류 결과일 경우 예외 처리
                    if (ResXml.SelectSingleNode("error_no").InnerText != "0")
                        throw new Exception(ResXml.SelectSingleNode("error_desc").InnerText);

                    if (ResXml.SelectSingleNode("FSAMAP/NFSA").InnerText.Equals("0"))
                        throw new Exception("항공요금 검색 결과가 없습니다.");

                    //검색 결과 DB 저장
                    ResXml = SearchFareListGDSDBSave("Sabre", SNM, GUID, ReqInfo, ResXml);
                }

                //프로모션 정보
                XmlElement PromXml = null;

                if (!ROT.Equals("MD"))
                {
                    PromXml = SearchPromotionList(SNM, DLC, ALC, ROT, DTD, ARD, OPN, (CCD.Equals("C") || CCD.Equals("F")) ? CCD : "Y", PTC[0]);
                    cm.XmlFileSave(PromXml, mc.Name, "SearchPromotionList", "Y", GUID);
                }

                //통합구성
                sw = Stopwatch.StartNew();
                XmlMode = asv.ToModeSearchFareAvailSabreRS(SNM, ResXml, PromXml, DLC, ALC, ROT, DTD, OPN, CCD, PUB, LTD, FTR, PTC[0], ADC, CHC, IFC);
                CheckRunTimeEnd(NewRunTime, sw, "ToModeSearchFareAvailSabreRS");

                //실행시간 데이타 XML에 추가
                XmlMode.AppendChild(XmlMode.OwnerDocument.ImportNode(NewRunTime, true));
                cm.XmlFileSave(XmlMode, mc.Name, "ToModeSearchFareAvailSabreRS", "Y", GUID);

                return XmlMode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion "Fare + Availability 동시조회(GROUPING)"

        #region "그루핑 운임용 랜딩서비스"

        /// <summary>
        /// 그루핑 운임용 랜딩서비스
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="MKEY">항공검색 결과 고유번호</param>
        /// <param name="FareNumber">운임번호</param>
        /// <param name="AvailNumbers">여정번호(하나 이상일 경우 콤마로 구분)</param>
        /// <returns></returns>
        [WebMethod(Description = "그루핑 운임용 랜딩서비스")]
        public XmlElement SearchFareAvailLandingRS(int SNM, string MKEY, int FareNumber, string AvailNumbers)
        {
            try
            {
                string[] MKEYS = MKEY.Split(':');

                DataSet ds = SelectInfo(SNM, MKEYS[0], cm.RequestInt(MKEYS[2]), FareNumber, AvailNumbers);
                DataTable dt1 = ds.Tables[0];
                DataTable dt2 = ds.Tables[1];

                if (dt1.Rows.Count > 0 && dt2.Rows.Count > 0)
                {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.Load(mc.XmlFullPath("SearchFareAvailRS"));

                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("timeStamp").InnerText = cm.TimeStamp;
                    XmlDoc.SelectSingleNode("ResponseDetails").Attributes.GetNamedItem("ref").InnerText = MKEY;

                    //여정정보
                    XmlNode FlightInfo = XmlDoc.SelectSingleNode("ResponseDetails/flightInfo");
                    XmlNode FlightIndex = FlightInfo.SelectSingleNode("flightIndex");
                    XmlNode NewFlightIndex;
                    int idx = 0;

                    FlightInfo.Attributes.GetNamedItem("ptc").InnerText = dt2.Rows[0]["PTC"].ToString();
                    FlightInfo.Attributes.GetNamedItem("rot").InnerText = dt1.Rows[0]["구간"].ToString();

                    foreach (DataRow dr in dt2.Rows)
                    {
                        if (idx != Convert.ToInt32(dr["여정번호"]))
                        {
                            NewFlightIndex = FlightInfo.AppendChild(FlightIndex.CloneNode(false));
                            NewFlightIndex.Attributes.GetNamedItem("ref").InnerText = dr["여정번호"].ToString();

                            XmlDocument SegXml = new XmlDocument();
                            SegXml.LoadXml(dr["XMLDATA"].ToString());

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

                    if (PriceInfo.SelectNodes("priceIndex/fareMessage").Count > 0)
                    {
                        //FareMessage CDATA 처리
                        XmlNode FareMessage = PriceInfo.SelectSingleNode("priceIndex/fareMessage");
                        
                        //CommonHeader
                        if (FareMessage.SelectNodes("CommonHeader").Count > 0)
                        {
                            string CommonHeader = FareMessage.SelectSingleNode("CommonHeader").InnerText;

                            FareMessage.SelectSingleNode("CommonHeader").InnerText = "";
                            FareMessage.SelectSingleNode("CommonHeader").AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(CommonHeader));
                        }

                        //링크정보
                        if (FareMessage.SelectNodes("FARE_REC1").Count > 0)
                        {
                            string FARE_REC1 = FareMessage.SelectSingleNode("FARE_REC1").InnerText;

                            FareMessage.SelectSingleNode("FARE_REC1").InnerText = "";
                            FareMessage.SelectSingleNode("FARE_REC1").AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(FARE_REC1));
                        }

                        if (FareMessage.SelectNodes("FARE_REC2").Count > 0)
                        {
                            string FARE_REC2 = FareMessage.SelectSingleNode("FARE_REC2").InnerText;

                            FareMessage.SelectSingleNode("FARE_REC2").InnerText = "";
                            FareMessage.SelectSingleNode("FARE_REC2").AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(FARE_REC2));
                        }

                        if (FareMessage.SelectNodes("FARE_REC3").Count > 0)
                        {
                            string FARE_REC3 = FareMessage.SelectSingleNode("FARE_REC3").InnerText;

                            FareMessage.SelectSingleNode("FARE_REC3").InnerText = "";
                            FareMessage.SelectSingleNode("FARE_REC3").AppendChild((XmlCDataSection)XmlDoc.CreateCDataSection(FARE_REC3));
                        }
                    }


                    return XmlDoc.DocumentElement;
                }
                else
                    throw new Exception("항공 운임 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AirService2", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "그루핑 운임용 랜딩서비스"

        #region "그루핑 운임용 요금규정 조회"

        /// <summary>
        /// 그루핑 운임용 요금규정 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="MKEY">항공검색 결과 고유번호</param>
        /// <param name="FareNumber">운임번호</param>
        /// <param name="AvailNumbers">여정번호(하나 이상일 경우 콤마로 구분)</param>
        /// <returns></returns>
        [WebMethod(Description = "그루핑 운임용 요금규정 조회")]
        public XmlElement SearchRuleRS(int SNM, string MKEY, int FareNumber, string AvailNumbers)
        {
            try
            {
                string[] MKEYS = MKEY.Split(':');

                DataSet ds = SelectInfo(SNM, MKEYS[0], cm.RequestInt(MKEYS[2]), FareNumber, AvailNumbers);
                DataTable dt1 = ds.Tables[0];
                DataTable dt2 = ds.Tables[1];

                if (dt1.Rows.Count > 0 && dt2.Rows.Count > 0)
                {
                    XmlDocument FareXml = new XmlDocument();
                    FareXml.LoadXml(dt1.Rows[0]["XMLDATA"].ToString());

                    string GDS = dt1.Rows[0]["GDS"].ToString();
                    string PMID = dt1.Rows[0]["프로모션번호"].ToString();
                    
                    if (GDS.Equals("Sabre"))
                    {
                        string AirCode = dt1.Rows[0]["항공사코드"].ToString();
                        string AddRule = String.Format("요금조건:{0}^유효기간:{1}", dt1.Rows[0]["요금조건명"].ToString(), cm.ExpiryDateText(dt1.Rows[0]["유효기간"].ToString()));

                        return asv.SearchRuleSabreRS(SNM, PMID, AirCode, AddRule, String.Concat(FareXml.SelectSingleNode("priceIndex/fareMessage/CommonHeader").InnerText, FareXml.SelectSingleNode("priceIndex/fareMessage/FARE_REC1").InnerText));
                    }
                    else
                    {
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

                        return asv.SearchRuleRS(SNM, PMID, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, FareXml.SelectSingleNode("priceIndex/paxFareGroup").OuterXml);
                    }
                }
                else
                    throw new Exception("항공 운임 정보가 존재하지 않습니다.");
            }
            catch (Exception ex)
            {
                return new MWSException(ex, hcc, "AirService2", MethodBase.GetCurrentMethod().Name, 0, 0).ToErrors;
            }
        }

        #endregion "그루핑 운임용 요금규정 조회"

        #region "DB"

        /// <summary>
        /// 항공운임+스케쥴 정보 DB 저장 후 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="DEV">개발서버용 여부</param>
        /// <param name="ReqInfo">검색요청정보</param>
        /// <param name="XMLDATA">실시간 항공운임정보</param>
        /// <returns></returns>
        protected XmlElement SearchFareListDBSaveAndSelect(int SNM, string GUID, string DEV, string ReqInfo, XmlElement ResXml)
        {
            XmlDocument XmlDoc = new XmlDocument();
            string ErrCode = string.Empty;
            string ErrMsg = string.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandTimeout = 60;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DBO.WSV_T_해외항공검색_저장및조회";

                    cmd.Parameters.Add("@오피스아이디", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                    cmd.Parameters.Add("@요청정보", SqlDbType.VarChar, 200);
                    cmd.Parameters.Add("@XMLDATA", SqlDbType.Xml, -1);
                    cmd.Parameters.Add("@GUID", SqlDbType.VarChar, 30);
                    cmd.Parameters.Add("@개발용도", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@항공검색번호", SqlDbType.BigInt, 0);
                    cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    cmd.Parameters["@오피스아이디"].Value = amd.OfficeId(SNM);
                    cmd.Parameters["@사이트번호"].Value = SNM;
                    cmd.Parameters["@요청정보"].Value = ReqInfo;
                    cmd.Parameters["@XMLDATA"].Value = ResXml.OuterXml;
                    cmd.Parameters["@GUID"].Value = GUID;
                    cmd.Parameters["@개발용도"].Value = DEV;
                    cmd.Parameters["@항공검색번호"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();

                        XmlDoc.LoadXml(cmd.ExecuteScalar().ToString());
                        ErrCode = cmd.Parameters["@결과"].Value.ToString();
                        ErrMsg = cmd.Parameters["@에러메시지"].Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception) { }

            return ErrCode.Equals("S") ? XmlDoc.DocumentElement : null;
        }

        /// <summary>
        /// 항공운임+스케쥴 정보 DB 조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="DEV">개발서버용 여부</param>
        /// <param name="ReqInfo">검색요청정보</param>
        /// <returns></returns>
        protected XmlElement SearchFareListDBSelect(int SNM, string DEV, string ReqInfo)
        {
            XmlDocument XmlDoc = new XmlDocument();
            string ErrCode = string.Empty;
            string ErrMsg = string.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandTimeout = 10;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = String.Format("DBO.WSV_S_해외항공검색_조회{0}", (SNM.Equals(4925) || SNM.Equals(4926)) ? "_티몬" : ((SNM.Equals(4924) || SNM.Equals(4929)) ? "_11번가" : ((SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164)) ? "_11번가" : "_모두닷컴")));

                    cmd.Parameters.Add("@오피스아이디", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                    cmd.Parameters.Add("@요청정보", SqlDbType.VarChar, 200);
                    cmd.Parameters.Add("@개발용도", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@항공검색번호", SqlDbType.Int, 0);
                    cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    cmd.Parameters["@오피스아이디"].Value = amd.OfficeId(SNM);
                    cmd.Parameters["@사이트번호"].Value = SNM;
                    cmd.Parameters["@요청정보"].Value = ReqInfo;
                    cmd.Parameters["@개발용도"].Value = DEV;
                    cmd.Parameters["@항공검색번호"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();

                        XmlDoc.LoadXml(cmd.ExecuteScalar().ToString());
                        ErrCode = cmd.Parameters["@결과"].Value.ToString();
                        ErrMsg = cmd.Parameters["@에러메시지"].Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception) { }

            return ErrCode.Equals("S") ? XmlDoc.DocumentElement : null;
        }

        /// <summary>
        /// 항공운임+스케쥴 정보 DB 저장
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="ReqInfo">검색요청정보</param>
        /// <param name="XMLDATA">실시간 항공운임정보</param>
        /// <returns></returns>
        protected XmlElement SearchFareListDBSave(int SNM, string GUID, string ReqInfo, XmlElement ResXml)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand
                    {
                        Connection = conn,
                        CommandTimeout = 10,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "DBO.WSV_T_해외항공검색_저장"
                    };

                    cmd.Parameters.Add("@오피스아이디", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                    cmd.Parameters.Add("@요청정보", SqlDbType.VarChar, 500);
                    cmd.Parameters.Add("@XMLDATA", SqlDbType.Xml, -1);
                    cmd.Parameters.Add("@GUID", SqlDbType.VarChar, 30);
                    cmd.Parameters.Add("@항공검색번호", SqlDbType.BigInt, 0);
                    cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    cmd.Parameters["@오피스아이디"].Value = amd.OfficeId(SNM);
                    cmd.Parameters["@사이트번호"].Value = SNM;
                    cmd.Parameters["@요청정보"].Value = ReqInfo;
                    cmd.Parameters["@XMLDATA"].Value = ResXml.OuterXml;
                    cmd.Parameters["@GUID"].Value = GUID;
                    cmd.Parameters["@항공검색번호"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception) { }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception) { }

            return ResXml;
        }

        /// <summary>
        /// 항공운임+스케쥴 정보 DB 조회(GDS)
        /// </summary>
        /// <param name="GDS">GDS코드</param>
        /// <param name="SNM">사이트번호</param>
        /// <param name="ReqInfo">검색요청정보</param>
        /// <returns></returns>
        protected XmlElement SearchFareListGDSDBSelect(string GDS, int SNM, string ReqInfo)
        {
            XmlDocument XmlDoc = new XmlDocument();
            string Idx = string.Empty;
            string ErrCode = string.Empty;
            string ErrMsg = string.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandTimeout = 10;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "DBO.WSV_S_해외항공검색_GDS_조회";

                    cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@오피스아이디", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@요청정보", SqlDbType.VarChar, 200);
                    cmd.Parameters.Add("@항공소스번호", SqlDbType.BigInt, 0);
                    cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    cmd.Parameters["@GDS"].Value = GDS;
                    cmd.Parameters["@오피스아이디"].Value = amd.OfficeId(SNM);
                    cmd.Parameters["@요청정보"].Value = ReqInfo;
                    cmd.Parameters["@항공소스번호"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();

                        string XmlData = cmd.ExecuteScalar().ToString();
                        Idx = cmd.Parameters["@항공소스번호"].Value.ToString();
                        ErrCode = cmd.Parameters["@결과"].Value.ToString();
                        ErrMsg = cmd.Parameters["@에러메시지"].Value.ToString();

                        if (ErrCode.Equals("S") && !String.IsNullOrWhiteSpace(XmlData))
                            XmlDoc.LoadXml(XmlData);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception) { }

            if (ErrCode.Equals("S"))
            {
                XmlAttribute SourceIdx = XmlDoc.FirstChild.Attributes.Append(XmlDoc.CreateAttribute("ref"));
                SourceIdx.InnerText = Idx;
                
                return XmlDoc.DocumentElement;
            }
            else
                return null;
        }

        /// <summary>
        /// 항공운임+스케쥴 정보 DB 저장(GDS)
        /// </summary>
        /// <param name="GDS">GDS코드</param>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GUID">고유번호</param>
        /// <param name="ReqInfo">검색요청정보</param>
        /// <param name="XMLDATA">실시간 항공 검색 결과</param>
        /// <returns></returns>
        protected XmlElement SearchFareListGDSDBSave(string GDS, int SNM, string GUID, string ReqInfo, XmlElement ResXml)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVLOG"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand
                    {
                        Connection = conn,
                        CommandTimeout = 10,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "DBO.WSV_T_해외항공검색_GDS_저장"
                    };

                    cmd.Parameters.Add("@GDS", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@오피스아이디", SqlDbType.VarChar, 10);
                    cmd.Parameters.Add("@요청정보", SqlDbType.VarChar, 200);
                    cmd.Parameters.Add("@GUID", SqlDbType.VarChar, 30);
                    cmd.Parameters.Add("@XMLDATA", SqlDbType.Xml, -1);
                    cmd.Parameters.Add("@항공소스번호", SqlDbType.BigInt, 0);
                    cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
                    cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    cmd.Parameters["@GDS"].Value = GDS;
                    cmd.Parameters["@오피스아이디"].Value = amd.OfficeId(SNM);
                    cmd.Parameters["@요청정보"].Value = ReqInfo;
                    cmd.Parameters["@GUID"].Value = GUID;
                    cmd.Parameters["@XMLDATA"].Value = ResXml.OuterXml;
                    cmd.Parameters["@항공소스번호"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        string Idx = cmd.Parameters["@항공소스번호"].Value.ToString();

                        if (!String.IsNullOrWhiteSpace(Idx) && Idx != "0")
                        {
                            XmlAttribute SourceIdx = ResXml.Attributes.Append(ResXml.OwnerDocument.CreateAttribute("ref"));
                            SourceIdx.InnerText = Idx;
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception) { }

            return ResXml;
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
                throw new MWSException(ex, hcc, "AirService2", MethodBase.GetCurrentMethod().Name, 0, 0);
            }

            return ds;
        }

        #endregion "DB"

        #region "프로모션"

        /// <summary>
        /// 프로모션 리스트
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)></param>
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
        [WebMethod(Description = "프로모션 상세정보")]
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

                sqlParam[0].Value = 15;
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

                switch (WebMethodName)
                {
                    case "SearchFareAvailRS": WebMethodName = "SearchFareAvailGroupingRS"; break;
                }
			
				XmlDocument XmlHelp = new XmlDocument();

				if (cm.FileExists(mc.HelpXmlFullPath(WebMethodName)))
					XmlHelp.Load(mc.HelpXmlFullPath(WebMethodName));
				else
					XmlHelp.LoadXml(String.Format("<HelpXml><Errors><WebMethodName>{0}</WebMethodName><Error><![CDATA[요청하신 웹메서드에 대한 설명은 제공되지 않습니다.]]></Error></Errors></HelpXml>", WebMethodName));

				return XmlHelp.DocumentElement;
			}
			catch (Exception ex)
			{
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirService", MethodBase.GetCurrentMethod().Name, 15, 0, 0).ToErrors;
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

                sqlParam[0].Value = 16;
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

                switch (WebMethodName)
                {
                    case "SearchFareAvailRS": WebMethodName = "SearchFareAvailGroupingRS"; break;
                }

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
                return new MWSExceptionMode(ex, hcc, LogGUID, "AirService", MethodBase.GetCurrentMethod().Name, 16, 0, 0).ToErrors;
			}
		}

		#endregion "메서드 설명"
    }
}