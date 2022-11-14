using System;
using System.ComponentModel;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
    /// <summary>
    /// Galileo 실시간 항공 예약을 위한 각종 정보 제공
    /// </summary>
    [WebService(Namespace = "http://airservice2.modetour.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class GalileoAirService : System.Web.Services.WebService
    {

        Common cm;
        GalileoConfig gc;
        HttpContext hcc;

        public GalileoAirService()
        {
            cm = new Common();
            gc = new GalileoConfig();
            hcc = HttpContext.Current;
        }

        #region "AirAvailability"

        [WebMethod(Description = "AirAvailabilityRQ")]
        public string AirAvailabilityRQ(string SCity1, string ECity1, string SCity2, string ECity2, string SCity3, string ECity3, string SDate1, string SDate2, string SDate3, string Trip, string FareType, string StayLength, int Adult, int Child, int Infant, int PageNo, int SplitNo, string Soto, string Agent, string SeatType, string DocType, string Summary)
        {
            return string.Format("SCITY1={0}&ECITY1={1}&SCITY2={2}&ECITY2={3}&SCITY3={4}&ECITY3={5}&SDATE1={6}&SDATE2={7}&SDATE3={8}&TRIP={9}&FareType={10}&StayLength={11}&Adult={12}&Child={13}&Infant={14}&PageNo={15}&SplitNo={16}&SOTO={17}&Agent={18}&SeatType={19}&DocType={20}&Summary={21}", SCity1, ECity1, SCity2, ECity2, SCity3, ECity3, SDate1, SDate2, SDate3, Trip, FareType, StayLength, Adult, Child, Infant, PageNo, SplitNo, Soto, Agent, SeatType, DocType, Summary);
        }

        /// <summary>
        /// 항공 운임+스케쥴 동시조회(AirAvailability)
        /// </summary>
        /// <param name="SCity1">출발 도시1(공항코드)</param>
        /// <param name="ECity1">도착 도시1(공항코드)</param>
        /// <param name="SCity2">출발 도시2(공항코드)</param>
        /// <param name="ECity2">도착 도시2(공항코드)</param>
        /// <param name="SCity3">출발 도시3(공항코드)</param>
        /// <param name="ECity3">도착 도시3(공항코드)</param>
        /// <param name="SDate1">출발일1(편도/왕복/다구간1(출발일))(YYYYMMDD)</param>
        /// <param name="SDate2">출발일2(왕복(귀국일), 다구간2(출발일))(YYYYMMDD)</param>
        /// <param name="SDate3">출발일3(다구간3(출발일))(YYYYMMDD)</param>
        /// <param name="Trip">여정타입(OW:편도, RT:왕복(오픈,다른출귀국), MD:다구간)</param>
        /// <param name="FareType">좌석등급(F:일등석, C:비즈니스석, Y:일반석)</param>
        /// <param name="StayLength">오픈예약(체류기간 5D/1M/1Y)</param>
        /// <param name="Adult">성인수</param>
        /// <param name="Child">소아수</param>
        /// <param name="Infant">유아수</param>
        /// <param name="PageNo">호출페이지(1page에 모든 요금 노출)</param>
        /// <param name="SplitNo">페이지 당 상품 수</param>
        /// <param name="Soto">해외출발 여부(Y/N)</param>
        /// <param name="Agent">판매처 구분</param>
        /// <param name="SeatType">좌석상태(P:가능, W:대기포함(가능,대기), A:모든좌석(가능,대기,마감))</param>
        /// <param name="DocType">문서형태(X/""(공백):XML, J:JSON)</param>
        /// <param name="Summary">요약정보 노출 여부(Y/N)</param>
        /// <returns></returns>
        [WebMethod(Description = "AirAvailabilityRS")]
        public XmlElement AirAvailabilityRS(string SCity1, string ECity1, string SCity2, string ECity2, string SCity3, string ECity3, string SDate1, string SDate2, string SDate3, string Trip, string FareType, string StayLength, int Adult, int Child, int Infant, int PageNo, int SplitNo, string Soto, string Agent, string SeatType, string DocType, string Summary)
        {
            string GUID = cm.GetGUID;
            string Parameters = AirAvailabilityRQ(SCity1, ECity1, SCity2, ECity2, SCity3, ECity3, SDate1, SDate2, SDate3, Trip, FareType, StayLength, Adult, Child, Infant, PageNo, SplitNo, Soto, Agent, SeatType, DocType, Summary);
            cm.XmlFileSave(Parameters, gc.Name, "AirAvailabilityRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("AirAvailability", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "AirAvailabilityRS", "N", GUID);

            return ResXml;
        }

        #endregion "AirAvailability"

        #region "FareRule"

        /// <summary>
        /// 운임규정 조회
        /// </summary>
        /// <param name="FareRuleUrl">ResInfoCreate에서 넘어온 FareRuleUrl</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "FareRuleRS")]
        public XmlElement FareRuleRS(string FareRuleUrl, string GUID)
        {
            cm.XmlFileSave(FareRuleUrl, gc.Name, "FareRuleRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("FareRule", FareRuleUrl.Replace("^", "&"));
            cm.XmlFileSave(ResXml, gc.Name, "FareRuleRS", "N", GUID);

            return ResXml;
        }

        #endregion "FareRule"

        #region "ResInfoCreate"

        [WebMethod(Description = "ResInfoCreateRQ")]
        public string ResInfoCreateRQ(string SCity1, string ECity1, string SCity2, string ECity2, string SCity3, string ECity3, string SCity4, string ECity4, string SCity5, string ECity5, string SCity6, string ECity6, string SCity7, string ECity7, string SDate1, string SDate2, string SDate3, string SDate4, string SDate5, string SDate6, string SDate7, string Trip, string FareType, string StayLength, int ADC, int CHC, int IFC, string Itinerary1, string Itinerary2, string Itinerary3, string SGC, string RGC, string EventNum, string PartnerNum1, string PartnerNum2, string TaxInfo, string PromotionCode, string PromotionName, string PromotionAmt, string PromotionAdtInd, string PromotionChdInd, string PromotionInfInd, string NaverFareJoin, string PartnerCode, string NoFareInd, string FareLocation, string AddOnDomStart, string AddOnDomReturn, string AdultBagInfo, string ChildBagInfo, string InfantBagInfo, string PlatingCarrier, string FareInfo)
        {
            return String.Format("SCITY1={0}&ECITY1={1}&SCITY2={2}&ECITY2={3}&SCITY3={4}&ECITY3={5}&SCITY4={6}&ECITY4={7}&SCITY5={8}&ECITY5={9}&SCITY6={10}&ECITY6={11}&SCITY7={12}&ECITY7={13}&SDATE1={14}&SDATE2={15}&SDATE3={16}&SDATE4={17}&SDATE5={18}&SDATE6={19}&SDATE7={20}&TRIP={21}&FareType={22}&StayLength={23}&Adt={24}&Chd={25}&Inf={26}&Itinerary1={27}&Itinerary2={28}&Itinerary3={29}&SGC={30}&RGC={31}&EventNum={32}&PartnerNum={33}&PartnerNum={34}&TaxInfo={35}&PromotionCode={36}&PromotionName={37}&PromotionAmt={38}&PromotionAdtInd={39}&PromotionChdInd={40}&PromotionInfInd={41}&NaverFareJoin={42}&PartnerCode={43}&NoFareInd={44}&FareLocation={45}&AddOnDomStart={46}&AddOnDomReturn={47}&AdultBagInfo={48}&ChildBagInfo={49}&InfantBagInfo={50}&PlatingCarrier={51}&FareInfo={52}", SCity1, ECity1, SCity2, ECity2, SCity3, ECity3, SCity4, ECity4, SCity5, ECity5, SCity6, ECity6, SCity7, ECity7, SDate1, SDate2, SDate3, SDate4, SDate5, SDate6, SDate7, Trip, FareType, StayLength, ADC, CHC, IFC, Itinerary1, Itinerary2, Itinerary3, SGC, RGC, EventNum, PartnerNum1, PartnerNum2, TaxInfo, PromotionCode, PromotionName, PromotionAmt, PromotionAdtInd, PromotionChdInd, PromotionInfInd, NaverFareJoin, PartnerCode, (NoFareInd.Equals("Y") ? "Y" : "N"), FareLocation, AddOnDomStart, AddOnDomReturn, AdultBagInfo, ChildBagInfo, InfantBagInfo, PlatingCarrier, FareInfo);
        }

        /// <summary>
        /// 운임 및 여정 조회(예약전)
        /// </summary>
        /// <param name="SCity1">첫번째 출발지 공항 코드</param>
        /// <param name="ECity1">첫번째 도착지 공항 코드</param>
        /// <param name="SCity2">두번째 출발지 공항 코드</param>
        /// <param name="ECity2">두번째 도착지 공항 코드</param>
        /// <param name="SCity3">세번째 출발지 공항 코드</param>
        /// <param name="ECity3">세번째 도착지 공항 코드</param>
        /// <param name="SCity4">네번째 출발지 공항 코드</param>
        /// <param name="ECity4">네번째 도착지 공항 코드</param>
        /// <param name="SCity5">다섯번째 출발지 공항 코드</param>
        /// <param name="ECity5">다섯번째 도착지 공항 코드</param>
        /// <param name="SCity6">여섯번째 출발지 공항 코드</param>
        /// <param name="ECity6">여섯번째 도착지 공항 코드</param>
        /// <param name="SCity7">일곱번째 출발지 공항 코드</param>
        /// <param name="ECity7">일곱번째 도착지 공항 코드</param>
        /// <param name="SDate1">첫번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate2">두번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate3">세번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate4">네번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate5">다섯번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate6">여섯번째 출발일(YYYYMMDD)</param>
        /// <param name="SDate7">일곱번째 출발일(YYYYMMDD)</param>
        /// <param name="Trip">구간(OW:편도, RT:왕복)</param>
        /// <param name="FareType">캐빈 클래스(Y:일반석, C:비즈니스석, F:일등석)</param>
        /// <param name="StayLength">체류기간</param>
        /// <param name="ADC">성인수</param>
        /// <param name="CHC">소아수</param>
        /// <param name="IFC">유아수</param>
        /// <param name="Itinerary1">첫번째 여정정보</param>
        /// <param name="Itinerary2">두번째 여정정보</param>
        /// <param name="Itinerary3">세번째 여정정보</param>
        /// <param name="SGC">상품코드</param>
        /// <param name="RGC">상품코드</param>
        /// <param name="EventNum">이벤트(프로모션)코드</param>
        /// <param name="PartnerNum1">사이트할인코드</param>
        /// <param name="PartnerNum2">사이트할인코드</param>
        /// <param name="TaxInfo">텍스정보</param>
        /// <param name="PromotionCode">네이버할인코드</param>
        /// <param name="PromotionName">네이버할인명</param>
        /// <param name="PromotionAmt">네이버할인금액</param>
        /// <param name="PromotionAdtInd">네이버할인금액(성인)</param>
        /// <param name="PromotionChdInd">네이버할인금액(소아)</param>
        /// <param name="PromotionInfInd">네이버할인금액(유아)</param>
        /// <param name="NaverFareJoin">결합조건코드</param>
        /// <param name="PartnerCode">파트너코드(NAVER 고정값)</param>
        /// <param name="NoFareInd">운임존재여부(갈릴레오 운임으로 예약시:N, 아마데우스 운임으로 예약시:Y)</param>
        /// <param name="FareLocation">운임구분자(L:로컬운임, H:호스트운임, 공백:로컬운임)</param>
        /// <param name="AddOnDomStart">출발편 국내선 구간 포함 여부</param>
        /// <param name="AddOnDomReturn">귀국편 국내선 구간 포함 여부</param>
        /// <param name="AdultBagInfo">성인 수하물정보</param>
        /// <param name="ChildBagInfo">소아 수하물정보</param>
        /// <param name="InfantBagInfo">유아 수하물정보</param>
        /// <param name="PlatingCarrier">발권항공사</param>
        /// <param name="FareInfo">요금부가정보</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ResInfoCreateRS")]
        public XmlElement ResInfoCreateRS(string SCity1, string ECity1, string SCity2, string ECity2, string SCity3, string ECity3, string SCity4, string ECity4, string SCity5, string ECity5, string SCity6, string ECity6, string SCity7, string ECity7, string SDate1, string SDate2, string SDate3, string SDate4, string SDate5, string SDate6, string SDate7, string Trip, string FareType, string StayLength, int ADC, int CHC, int IFC, string Itinerary1, string Itinerary2, string Itinerary3, string SGC, string RGC, string EventNum, string PartnerNum1, string PartnerNum2, string TaxInfo, string PromotionCode, string PromotionName, string PromotionAmt, string PromotionAdtInd, string PromotionChdInd, string PromotionInfInd, string NaverFareJoin, string PartnerCode, string NoFareInd, string FareLocation, string AddOnDomStart, string AddOnDomReturn, string AdultBagInfo, string ChildBagInfo, string InfantBagInfo, string PlatingCarrier, string FareInfo, string GUID)
        {
            string Parameters = ResInfoCreateRQ(SCity1, ECity1, SCity2, ECity2, SCity3, ECity3, SCity4, ECity4, SCity5, ECity5, SCity6, ECity6, SCity7, ECity7, SDate1, SDate2, SDate3, SDate4, SDate5, SDate6, SDate7, Trip, FareType, StayLength, ADC, CHC, IFC, Itinerary1, Itinerary2, Itinerary3, SGC, RGC, EventNum, PartnerNum1, PartnerNum2, TaxInfo, PromotionCode, PromotionName, PromotionAmt, PromotionAdtInd, PromotionChdInd, PromotionInfInd, NaverFareJoin, PartnerCode, NoFareInd, FareLocation, AddOnDomStart, AddOnDomReturn, AdultBagInfo, ChildBagInfo, InfantBagInfo, PlatingCarrier, FareInfo);
            cm.XmlFileSave(Parameters, gc.Name, "ResInfoCreateRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("ResInfoCreate", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "ResInfoCreateRS", "N", GUID);

            return ResXml;
        }

        #endregion "ResInfoCreate"

        #region "ResProcess"

        [WebMethod(Description = "ResProcessRQ")]
        public string ResProcessRQ(int ReqIdx, string ResID, string ResName, string ResTel, string ResHp, string ResEmail, string ResEmpID, string ResEmpName, string SiteCode, string PartnerCode, string SiteType, string BtoBInd, string BtoBAgentCode, string BtoBAgentName, string BtoBTktEmpID, string BtoBTktEmpName, string PaxInfo, string AgentTel, int Adult, int Child, int Infant)
        {
            return String.Format("ReqIdx={0}&ResID={1}&ResName={2}&ResTel={3}&ResHp={4}&ResEmail={5}&ResEmpID={6}&ResEmpName={7}&SiteCode={8}&PartnerCode={9}&SiteType={10}&BtoBInd={11}&BtoBAgentCode={12}&BtoBAgentName={13}&BtoBTktEmpID={14}&BtoBTktEmpName={15}&PaxInfo={16}&AgentTel={17}&Adult={18}&Child={19}&Infant={20}", ReqIdx, ResID, ResName, ResTel, ResHp, ResEmail, ResEmpID, ResEmpName, SiteCode, PartnerCode, SiteType, BtoBInd, BtoBAgentCode, BtoBAgentName, BtoBTktEmpID, BtoBTktEmpName, PaxInfo, AgentTel.Replace("(", " - ").Replace(")", ""), Adult, Child, Infant);
        }

        /// <summary>
        /// 항공예약
        /// </summary>
        /// <param name="ReqIdx">ResInfoCreate에서 넘어온 ReqIdx</param>
        /// <param name="ResID">예약자 ID</param>
        /// <param name="ResName">예약자 이름</param>
        /// <param name="ResTel">예약자 전화번호</param>
        /// <param name="ResHp">예약자 휴대폰번호</param>
        /// <param name="ResEmail">예약자 이메일주소</param>
        /// <param name="ResEmpID">예약담당자 ID</param>
        /// <param name="ResEmpName">예약담당자 이름</param>
        /// <param name="SiteCode">판매사이트 코드</param>
        /// <param name="PartnerCode">제휴사 코드</param>
        /// <param name="SiteType">웹, 모바일(I:아이폰계열, A:안드로이드계열)</param>
        /// <param name="BtoBInd">홀세일 구분(Y:홀세일, N:일반)</param>
        /// <param name="BtoBAgentCode">홀세일 거래처 코드</param>
        /// <param name="BtoBAgentName">홀세일 거래처 이름</param>
        /// <param name="BtoBTktEmpID">홀세일 담당자 ID</param>
        /// <param name="BtoBTktEmpName">홀세일 담당자 이름</param>
        /// <param name="PaxInfo">탑승자 정보</param>
        /// <param name="AgentTel">거래처 연락처</param>
        /// <param name="Adult">성인수</param>
        /// <param name="Child">소아수</param>
        /// <param name="Infant">유아수</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ResProcessRS")]
        public XmlElement ResProcessRS(int ReqIdx, string ResID, string ResName, string ResTel, string ResHp, string ResEmail, string ResEmpID, string ResEmpName, string SiteCode, string PartnerCode, string SiteType, string BtoBInd, string BtoBAgentCode, string BtoBAgentName, string BtoBTktEmpID, string BtoBTktEmpName, string PaxInfo, string AgentTel, int Adult, int Child, int Infant, string GUID)
        {
            ResTel = "02-2049-3352";
            
            string Parameters = ResProcessRQ(ReqIdx, ResID, ResName, ResTel, ResHp, ResEmail, ResEmpID, ResEmpName, SiteCode, PartnerCode, SiteType, BtoBInd, BtoBAgentCode, BtoBAgentName, BtoBTktEmpID, BtoBTktEmpName, PaxInfo, AgentTel, Adult, Child, Infant);
            cm.XmlFileSave(Parameters, gc.Name, "ResProcessRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("ResProcess", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "ResProcessRS", "N", GUID);

            return ResXml;
        }

        #endregion "ResProcess"

        #region "PnrInfoDisplay"

        [WebMethod(Description = "PnrInfoDisplayRQ")]
        public string PnrInfoDisplayRQ(string ReservationCode, string PNR, string GDSType)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GDSType={2}", ReservationCode, PNR, GDSType);
        }

        /// <summary>
        /// PNR 조회
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PnrInfoDisplayRS")]
        public XmlElement PnrInfoDisplayRS(string ReservationCode, string PNR, string GDSType, string GUID)
        {
            string Parameters = PnrInfoDisplayRQ(ReservationCode, PNR, GDSType);
            cm.XmlFileSave(Parameters, gc.Name, "PnrInfoDisplayRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("PnrInfoDisplay", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "PnrInfoDisplayRS", "N", GUID);

            return ResXml;
        }

        #endregion "PnrInfoDisplay"

        #region "PnrCancel"

        [WebMethod(Description = "PnrCancelRQ")]
        public string PnrCancelRQ(string ReservationCode, string PNR, string GDSType, string ReqID, string XLoc, string PaxName, string ResDate)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GDSType={2}&ReqID={3}&XLoc={4}&PaxName={5}&ResDate={6}", ReservationCode, PNR, GDSType, ReqID, XLoc, PaxName, ResDate);
        }

        /// <summary>
        /// PNR 취소
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="ReqID">요청자 ID</param>
        /// <param name="XLoc">(C:고객단)</param>
        /// <param name="PaxName">PNR 탑승객 이름</param>
        /// <param name="ResDate">PNR 예약일(YYYYMMDD)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "PnrCancelRS")]
        public XmlElement PnrCancelRS(string ReservationCode, string PNR, string GDSType, string ReqID, string XLoc, string PaxName, string ResDate, string GUID)
        {
            string Parameters = PnrCancelRQ(ReservationCode, PNR, GDSType, ReqID, XLoc, PaxName, ResDate);
            cm.XmlFileSave(Parameters, gc.Name, "PnrCancelRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("PnrCancel", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "PnrCancelRS", "N", GUID);

            return ResXml;
        }

        #endregion "PnrCancel"

        #region "GKPnr"

        [WebMethod(Description = "GKPnrRQ")]
        public string GKPnrRQ(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string ApisInfo)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GDSType={2}&ReqID={3}&PaxCnt={4}&ApisInfo={5}", ReservationCode, PNR, GDSType, ReqID, PaxCnt, ApisInfo);
        }

        /// <summary>
        /// GK PNR 생성(아바쿠스 PNR 생성)
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="ReqID">요청자 ID</param>
        /// <param name="PaxCnt">해당 예약의 총 탑승객 수</param>
        /// <param name="ApisInfo">여권/체류지 입력 정보</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "GKPnrRS")]
        public XmlElement GKPnrRS(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string ApisInfo, string GUID)
        {
            string Parameters = GKPnrRQ(ReservationCode, PNR, GDSType, ReqID, PaxCnt, ApisInfo);
            cm.XmlFileSave(Parameters, gc.Name, "GKPnrRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("GKPnr", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "GKPnrRS", "N", GUID);

            return ResXml;
        }

        #endregion "GKPnr"

        #region "GKTicketNumUpdate"

        [WebMethod(Description = "GKTicketNumUpdateRQ")]
        public string GKTicketNumUpdateRQ(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string TKTPNR, string TKTGDSType)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GDSType={2}&ReqID={3}&PaxCnt={4}&TKTPNR={5}&TKTGDSType={6}", ReservationCode, PNR, GDSType, ReqID, PaxCnt, TKTPNR, TKTGDSType);
        }

        /// <summary>
        /// GK PNR 티켓번호 갈릴레오 PNR에 업데이트
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="ReqID">요청자 ID</param>
        /// <param name="PaxCnt">해당 예약의 총 탑승객 수</param>
        /// <param name="TKTPNR">발권된 PNR</param>
        /// <param name="TKTGDSType">발권된 PNR의 GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "GKTicketNumUpdateRS")]
        public XmlElement GKTicketNumUpdateRS(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string TKTPNR, string TKTGDSType, string GUID)
        {
            string Parameters = GKTicketNumUpdateRQ(ReservationCode, PNR, GDSType, ReqID, PaxCnt, TKTPNR, TKTGDSType);
            cm.XmlFileSave(Parameters, gc.Name, "GKTicketNumUpdateRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("GKTicketNumUpdate", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "GKTicketNumUpdateRS", "N", GUID);

            return ResXml;
        }

        #endregion "GKTicketNumUpdate"

        #region "ETicketInfoDisplay"

        [WebMethod(Description = "ETicketInfoDisplayRQ")]
        public string ETicketInfoDisplayRQ(string ReservationCode, string PNR, string TKTNO, string ANN, string GDSType)
        {
            return String.Format("ReservationCode={0}&PNR={1}&TKTNO={2}&ANN={3}&GDSType={4}", ReservationCode, PNR, TKTNO, ANN, GDSType);
        }

        /// <summary>
        /// 이티켓 조회
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="TKTNO">티켓번호</param>
        /// <param name="ANN">탑승객번호</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ETicketInfoDisplayRS")]
        public XmlElement ETicketInfoDisplayRS(string ReservationCode, string PNR, string TKTNO, string ANN, string GDSType, string GUID)
        {
            string Parameters = ETicketInfoDisplayRQ(ReservationCode, PNR, TKTNO, ANN, GDSType);
            cm.XmlFileSave(Parameters, gc.Name, "ETicketInfoDisplayRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("ETicketInfoDisplay", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "ETicketInfoDisplayRS", "N", GUID);

            return ResXml;
        }

        #endregion "ETicketInfoDisplay"

        #region "RemarksAdd"

        [WebMethod(Description = "RemarksAddRQ")]
        public string RemarksAddRQ(string ReservationCode, string PNR, string ReqID, string AgentRemarks)
        {
            return String.Format("ReservationCode={0}&PNR={1}&ReqID={2}&AgentRemarks={3}", ReservationCode, PNR, ReqID, AgentRemarks);
        }

        /// <summary>
        /// 리마크 저장
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="ReqID">요청자 ID</param>
        /// <param name="AgentRemarks">리마크</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "RemarksAddRS")]
        public XmlElement RemarksAddRS(string ReservationCode, string PNR, string ReqID, string AgentRemarks, string GUID)
        {
            string Parameters = RemarksAddRQ(ReservationCode, PNR, ReqID, AgentRemarks);
            cm.XmlFileSave(Parameters, gc.Name, "RemarksAddRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("RemarksAdd", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "RemarksAddRS", "N", GUID);

            return ResXml;
        }

        #endregion "RemarksAdd"

        #region "SessionStart"

        /// <summary>
        /// 세션생성
        /// </summary>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "SessionStartRS")]
        public XmlElement SessionStartRS(string GUID)
        {
            XmlElement ResXml = gc.SessionCreate();
            cm.XmlFileSave(ResXml, gc.Name, "SessionStartRS", "N", GUID);

            return ResXml;
        }

        #endregion "SessionStart"

        #region "SessionEnd"

        [WebMethod(Description = "SessionEndRQ")]
        public string SessionEndRQ(string Token)
        {
            return String.Format("Token={0}", Token);
        }

        /// <summary>
        /// 세션종료
        /// </summary>
        /// <param name="Token">세션키</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "SessionEndRS")]
        public XmlElement SessionEndRS(string Token, string GUID)
        {
            string Parameters = SessionEndRQ(Token);
            cm.XmlFileSave(Parameters, gc.Name, "SessionEndRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("SessionEnd", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "SessionEndRS", "N", GUID);

            return ResXml;
        }

        #endregion "SessionEnd"

        #region "TerminalSubmit"

        [WebMethod(Description = "TerminalSubmitRQ")]
        public string TerminalSubmitRQ(string Token, string Entry)
        {
            return String.Format("Token={0}&Entry={1}", Token, Entry);
        }

        /// <summary>
        /// Entry 모드
        /// </summary>
        /// <param name="Token">세션키</param>
        /// <param name="Entry">엔트리</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "TerminalSubmitRS")]
        public XmlElement TerminalSubmitRS(string Token, string Entry, string GUID)
        {
            string Parameters = TerminalSubmitRQ(Token, Entry);
            cm.XmlFileSave(Parameters, gc.Name, "TerminalSubmitRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("TerminalSubmit", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "TerminalSubmitRS", "N", GUID);

            return ResXml;
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
            string Token = String.Empty;

            try
            {
                //### 01.세션생성 #####
                Token = SessionStartRS(String.Concat(GUID, "-01")).SelectSingleNode("ResultMsg").InnerText;

                //### 02.TerminalSubmitRS #####
                XmlElement ResXml = TerminalSubmitRS(Token, Entry, String.Concat(GUID, "-02"));

                //### 03.세션종료 #####
                SessionEndRS(Token, String.Concat(GUID, "-03"));
                Token = "";

                return ResXml;
            }
            catch (Exception ex)
            {
                //### 세션종료 #####
                if (!String.IsNullOrWhiteSpace(Token))
                    SessionEndRS(Token, String.Concat(GUID, "-99"));

                return new MWSException(ex, hcc, gc.Name, "CommandRS", 0, 0).ToErrors;
            }
        }

        #endregion "TerminalSubmit"

        #region "SearchTax"

        [WebMethod(Description = "SearchTaxRQ")]
        public string SearchTaxRQ(string ReservationCode, string PNR, string GoodCode, string ReqID, int PaxCnt, string PaxAmountInfo, string KeyInOK, string FQMethod, string AdminAutoTkt, string CustomerTkt, string TaxOnly, string EventCode)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GoodCode={2}&ReqID={3}&PaxCnt={4}&PaxAmountInfo={5}&KeyInOK={6}&FQMethod={7}&AdminAutoTkt={8}&CustomerTkt={9}&TaxOnly={10}&EventCode={11}", ReservationCode, PNR, GoodCode, ReqID, PaxCnt, PaxAmountInfo, KeyInOK, FQMethod, AdminAutoTkt, CustomerTkt, TaxOnly, EventCode);
        }

        /// <summary>
        /// 예상 텍스 조회
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GoodCode">상품코드</param>
        /// <param name="ReqID">요청담당자 코드</param>
        /// <param name="PaxCnt">해당 예약의 총 탑승객 수</param>
        /// <param name="PaxAmountInfo">탑승객 정보(결제정보 포함)</param>
        /// <param name="KeyInOK">KE 키인승인 허용여부(키인으로 승인 받는 경우만 Y로 전달)</param>
        /// <param name="FQMethod">쿼트/발권 구분(FQ, TK)</param>
        /// <param name="AdminAutoTkt">관리자 발권 여부(Y, N)(N값 고정)</param>
        /// <param name="CustomerTkt">고객 발권 여부(Y, N)(Y값 고정)</param>
        /// <param name="TaxOnly">TAX 확인 여부(Y, N)(Y값 고정)</param>
        /// <param name="EventCode">할인요금 키값</param>
        /// <param name="FareLocation">운임종류(L:로컬운임, H:호스트운임)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "SearchTaxRS")]
        public XmlElement SearchTaxRS(string ReservationCode, string PNR, string GoodCode, string ReqID, int PaxCnt, string PaxAmountInfo, string KeyInOK, string FQMethod, string AdminAutoTkt, string CustomerTkt, string TaxOnly, string EventCode, string FareLocation, string GUID)
        {
            string Parameters = SearchTaxRQ(ReservationCode, PNR, GoodCode, ReqID, PaxCnt, PaxAmountInfo, KeyInOK, FQMethod, AdminAutoTkt, CustomerTkt, TaxOnly, EventCode);
            cm.XmlFileSave(Parameters, gc.Name, "SearchTaxRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute((FareLocation.Equals("H") ? "TaxConfirmGP" : "TktProcess"), Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "SearchTaxRS", "N", GUID);

            return ResXml;
        }

        #endregion "SearchTax"

        #region "Apis"

        [WebMethod(Description = "ApisRQ")]
        public string ApisRQ(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string ApisType, string ApisInfo)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GDSType={2}&ReqID={3}&PaxCnt={4}&ApisType={5}&ApisInfo={6}", ReservationCode, PNR, GDSType, ReqID, PaxCnt, ApisType, ApisInfo);
        }

        /// <summary>
        /// 여권/체류지 저장
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GDSType">GDS구분코드(G:갈릴레오, T:토파스, B:아바쿠스, D:아마데우스)</param>
        /// <param name="ReqID">요청자 ID</param>
        /// <param name="PaxCnt">해당 예약의 총 탑승객 수</param>
        /// <param name="ApisType">(DOCS:여권정보, DOCAD:체류지(목적지)정보, DOCAR:체류지(거주지)정보)</param>
        /// <param name="ApisInfo">여권/체류지 입력정보</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ApisRS")]
        public XmlElement ApisRS(string ReservationCode, string PNR, string GDSType, string ReqID, int PaxCnt, string ApisType, string ApisInfo, string GUID)
        {
            string Parameters = ApisRQ(ReservationCode, PNR, GDSType, ReqID, PaxCnt, ApisType, ApisInfo);
            cm.XmlFileSave(Parameters, gc.Name, "ApisRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("Apis", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "ApisRS", "N", GUID);

            return ResXml;
        }

        #endregion "Apis"

        #region "자동발권"

        [WebMethod(Description = "자동발권")]
        public string TicketIssuingRQ(string ReservationCode, string PNR, string GoodCode, string ReqID, int PaxCnt, string PaxAmountInfo, string KeyInOK, string FQMethod)
        {
            return String.Format("ReservationCode={0}&PNR={1}&GoodCode={2}&ReqID={3}&PaxCnt={4}&PaxAmountInfo={5}&KeyInOK={6}&FQMethod={7}&CustomerTkt=R&enc=u", ReservationCode, PNR, GoodCode, ReqID, PaxCnt, PaxAmountInfo, KeyInOK, FQMethod);
        }

        /// <summary>
        /// 자동발권
        /// </summary>
        /// <param name="ReservationCode">예약카드번호</param>
        /// <param name="PNR">PNR</param>
        /// <param name="GoodCode">상품코드</param>
        /// <param name="ReqID">요청담당자 코드</param>
        /// <param name="PaxCnt">해당 예약의 총 탑승객 수</param>
        /// <param name="PaxAmountInfo">탑승객 정보(결제정보 포함)</param>
        /// <param name="KeyInOK">KE 키인승인 허용여부(키인으로 승인 받는 경우만 Y로 전달)</param>
        /// <param name="FQMethod">쿼트/발권 구분(FQ, TK)</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "자동발권")]
        public XmlElement TicketIssuingRS(string ReservationCode, string PNR, string GoodCode, string ReqID, int PaxCnt, string PaxAmountInfo, string KeyInOK, string FQMethod, string GUID)
        {
            string Parameters = TicketIssuingRQ(ReservationCode, PNR, GoodCode, ReqID, PaxCnt, PaxAmountInfo, KeyInOK, FQMethod);
            cm.XmlFileSave(Parameters, gc.Name, "TicketIssuingRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("TktProcess", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "TicketIssuingRS", "N", GUID);

            return ResXml;

            //XmlDocument XmlDoc = new XmlDocument();
            //XmlDoc.LoadXml("<x><![CDATA[" + Parameters + "]]></x>");

            //return XmlDoc.DocumentElement;
        }

        #endregion "자동발권"

        #region "QueuePnrList"

        [WebMethod(Description = "QueuePnrListRQ")]
        public string QueuePnrListRQ(int QNum)
        {
            return String.Format("QNum={0}", QNum);
        }

        /// <summary>
        /// 큐방 개수 확인
        /// </summary>
        /// <param name="QNum">큐방번호</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "QueuePnrListRS")]
        public XmlElement QueuePnrListRS(int QNum, string GUID)
        {
            string Parameters = QueuePnrListRQ(QNum);
            cm.XmlFileSave(Parameters, gc.Name, "QueuePnrListRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("QueuePnrList", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "QueuePnrListRS", "N", GUID);

            return ResXml;
        }

        #endregion "QueuePnrList"

        #region "QroomTrans"

        [WebMethod(Description = "QroomTransRQ")]
        public string QroomTransRQ(string PCC, string PNR, int QNum, int QIdx)
        {
            return String.Format("PCC={0}&PNR={1}&QNum={2}&QR_idx={3}", PCC, PNR, QNum, QIdx);
        }

        /// <summary>
        /// 큐방 전송
        /// </summary>
        /// <param name="PCC">전송대상 PCC(58ZM:오프라인, 7D18:온라인)</param>
        /// <param name="PNR">전송대상 PNR</param>
        /// <param name="QNum">전송대상 큐방번호</param>
        /// <param name="QIdx">QueuePnrListRS에서 전달한 고유키값</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "QroomTransRS")]
        public XmlElement QroomTransRS(string PCC, string PNR, int QNum, int QIdx, string GUID)
        {
            GUID = String.IsNullOrWhiteSpace(GUID) ? cm.GetGUID : GUID;
            
            string Parameters = QroomTransRQ(PCC, PNR, QNum, QIdx);
            cm.XmlFileSave(Parameters, gc.Name, "QroomTransRQ", "N", GUID);

            XmlElement ResXml = gc.HttpExecute("QroomTrans", Parameters);
            cm.XmlFileSave(ResXml, gc.Name, "QroomTransRS", "N", GUID);

            return ResXml;
        }

        #endregion "QroomTrans"
    }
}