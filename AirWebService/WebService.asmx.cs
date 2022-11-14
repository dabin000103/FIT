using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
    /// <summary>
    /// 실시간 항공(해외) 예약 관리를 위한 웹서비스(통합용)(외주업체 제공용)
    /// </summary>
    [WebService(Namespace = "http://airservice2.modetour.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        AirService mas;

        public WebService()
        {
            mas = new AirService();
        }

        #region "운임조건 체크(사전발권, 발권일 등)"

        /// <summary>
        /// 운임조건 체크(사전발권, 발권일 등)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="DTD">출발일</param>
        /// <param name="TL">발권마감일</param>
        /// <param name="FareInfos">요금규정 XML내용 중 fareInfos Node Data String</param>
        /// <returns></returns>
        [WebMethod(Description = "AP조건 제한")]
        public bool CheckFareConditionString(int SNM, string AirCode, string DTD, string TL, string FareInfos)
        {
            return mas.CheckFareConditionString(SNM, AirCode, DTD, TL, FareInfos);
        }

        #endregion "운임조건 체크(사전발권, 발권일 등)"

        #region "운임규정 조회"

        #region "SearchRule"

        /// <summary>
        /// 운임규정 조회
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="SAC">Stock 항공사코드</param>
        /// <param name="PMID">프로모션 번호</param>
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
        /// <param name="PFG">paxFareGroup XmlNode</param>
        /// <returns></returns>
        [WebMethod(Description = "운임규정 조회")]
        public XmlElement SearchRuleRS(int SNM, string SAC, string PMID, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string PFG)
        {
            return mas.SearchRuleRS(SNM, SAC, PMID, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PFG);
        }

        #endregion "SearchRule"

        #region "예약 운임규정 조회"

        /// <summary>
        /// 운임규정 조회(예약시 저장된 규정)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <returns></returns>
        [WebMethod(Description = "운임규정 조회(예약시 저장된 규정)")]
        public XmlElement SearchBookingRuleRS(int OID, int PID)
        {
            return mas.SearchBookingRuleRS(OID, PID);
        }

        #endregion "예약 운임규정 조회"

        #endregion "운임규정 조회"

        #region "무료수하물"

        #region "무료수하물(스케쥴 이용)"

        /// <summary>
        /// 무료수하물 조회(스케쥴 이용)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
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
        /// <param name="ADC">성인 탑승객 수</param>
        /// <param name="CHC">소아 탑승객 수</param>
        /// <param name="IFC">유아 탑승객 수</param>
        [WebMethod(Description = "무료수하물 조회(스케쥴 이용)")]
        public XmlElement SearchBaggageRS(int SNM, string RQT, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, int ADC, int CHC, int IFC)
        {
            return mas.SearchBaggageRS(SNM, RQT, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, ADC, CHC, IFC);
        }

        #endregion "무료수하물(스케쥴 이용)"

        #endregion "무료수하물"

        #region "비행 상세조회"

        /// <summary>
        /// 비행 상세조회
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <param name="DTT">출발시간(HHMM)</param>
        /// <param name="DLC">출발지</param>
        /// <param name="ALC">도착지</param>
        /// <param name="MCC">마케팅항공사</param>
        /// <param name="OCC">운항항공사</param>
        /// <param name="FLN">편명</param>
        /// <returns></returns>
        [WebMethod(Description = "비행 상세조회")]
        public XmlElement FlightInfoRS(int SNM, string DTD, string DTT, string DLC, string ALC, string MCC, string OCC, string FLN)
        {
            return mas.FlightInfoRS(SNM, DTD, DTT, DLC, ALC, MCC, OCC, FLN);
        }

        #endregion "비행 상세조회"

        #region "예약"

        /// <summary>
        /// 항공예약
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
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
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
        /// <param name="ATSF">발권수수료(TASF) 적용 여부(Y:적용, N:미적용)</param>
        /// <param name="COOKIE">Header Cookie</param>
        /// <param name="FTX">free text</param>
        /// <returns></returns>
        [WebMethod(Description = "항공예약")]
        public XmlElement AddBookingRS(int[] PID, string[] PTC, string[] PTL, string[] PHN, string[] PSN, string[] PFN, string[] PBD, string[] PTN, string[] PMN, string[] PEA, string[] PMC, string[] PMT, string[] PMR, int RID, string RTL, string RHN, string RSN, string RFN, string RDB, string RGD, string RLF, string RTN, string RMN, string REA, string RMK, string RQT, string RQU, int SNM, int ANM, int AEN, string ROT, string OPN, string FXL, string SXL, string RXL, string DXL, string AKY, string ATSF, string COOKIE, string FTX)
        {
            return mas.AddBookingRS(PID, PTC, PTL, PHN, PSN, PFN, PBD, PTN, PMN, PEA, PMC, PMT, PMR, RID, RTL, RHN, RSN, RFN, RDB, RGD, RLF, RTN, RMN, REA, RMK, RQT, RQU, SNM, ANM, AEN, ROT, OPN, FXL, SXL, RXL, DXL, AKY, ATSF, COOKIE, FTX);
        }

        #endregion "예약"

        #region "예약조회"

        /// <summary>
        /// 예약조회
        /// </summary>
        /// <param name="OID">모두투어 주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="RIP">요청자IP</param>
        /// <returns></returns>
        [WebMethod(Description = "예약조회")]
        public XmlElement SearchBookingRS(int OID, int PID, string RIP)
        {
            return mas.SearchBookingRS(OID, PID, RIP);
        }

        #endregion "예약조회"

        #region "예약취소"

        /// <summary>
        /// 예약취소(PNR + DB 모두 취소)
        /// </summary>
        /// <param name="OID">모두투어 주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="CID">취소자번호(PTID)</param>
        /// <param name="RIP">요청자IP</param>
        /// <returns></returns>
        [WebMethod(Description = "예약취소(PNR + DB 모두 취소)")]
        public XmlElement CancelBookingRS(int OID, int PID, int CID, string RIP)
        {
            return mas.CancelBookingRS(OID, PID, CID, RIP);
        }

        #endregion "예약취소"

        #region "실시간좌석배정 인증키(토파스 ASP서비스용)"

        /// <summary>
        /// 실시간좌석배정 인증키(토파스 ASP서비스용)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PNR">PNR</param>
        /// <returns></returns>
        [WebMethod(Description = "실시간좌석배정 인증키")]
        public XmlElement SeatMapServiceKey(int SNM, string PNR)
        {
            return mas.SeatMapServiceKey(SNM, PNR);
        }

        #endregion "실시간좌석배정 인증키(토파스 ASP서비스용)"

        #region "APIS 등록"

        /// <summary>
        /// APIS 및 연락처 등록
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="RQR">작업 요청자(PTID 또는 이름)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <param name="SurName">영문성</param>
        /// <param name="GivenName">영문이름</param>
        /// <param name="PaxGender">성별(M/F)</param>
        /// <param name="BirthDate">생년월일(YYYYMMDD)</param>
        /// <param name="PassportNum">여권번호</param>
        /// <param name="ExpireDate">여권만료일(YYYYMMDD)</param>
        /// <param name="IssueCountry">여권발행국</param>
        /// <param name="HolderNationality">국적</param>
        /// <param name="Email">이메일주소</param>
        /// <param name="Tel">전화번호</param>
        /// <param name="HP">휴대폰번호</param>
        /// <param name="RIP">요청자IP</param>
        /// <returns></returns>
        [WebMethod(Description = "APIS 및 연락처 등록")]
        public XmlElement APISContactUpdate(int SNM, int OID, int PID, string RQR, string RQT, string[] SurName, string[] GivenName, string[] PaxGender, string[] BirthDate, string[] PassportNum, string[] ExpireDate, string[] IssueCountry, string[] HolderNationality, string[] Email, string[] Tel, string[] HP, string RIP)
        {
            return mas.APISContactUpdate(SNM, OID, PID, RQR, RQT, SurName, GivenName, PaxGender, BirthDate, PassportNum, ExpireDate, IssueCountry, HolderNationality, Email, Tel, HP, RIP);
        }

        #endregion "APIS 등록"

        #region "증빙서류"

        /// <summary>
        /// 증빙서류 조회
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <returns>증빙서류 내용</returns>
        [WebMethod(Description = "증빙서류 조회")]
        public string SearchProof(int OID, int PID)
        {
            return mas.SearchProof(OID, PID);
        }

        #endregion "증빙서류"

        #region "자동발권"

        #region "발권요청 상태값 변경"

        /// <summary>
        /// 발권요청 상태값 변경
        /// </summary>
        /// <param name="TIRN">요청일련번호</param>
        /// <param name="Status">자동발권상태값(1G:시스템불가, 1D:시스템실패, 1B:발권완료, 1J:발권완료(부분))</param>
        /// <param name="Message">자동발권메시지</param>
        /// <returns></returns>
        [WebMethod(Description = "발권요청 상태값 변경")]
        public void TicketIssuingRequestStatus(int TIRN, string Status, string Message)
        {
            mas.TicketIssuingRequestStatus(TIRN, Status, Message);
        }

        #endregion "발권요청 상태값 변경"

        #region "자동발권(Command)"

        /// <summary>
        /// 자동발권(Command)
        /// </summary>
        /// <param name="OID">모두투어 주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="FT">TourCode</param>
        /// <param name="FM">Commission</param>
        /// <param name="FE">Endorsement</param>
        /// <param name="FV">Validating Carrier</param>
        /// <param name="FP">결제요청정보</param>
        /// <param name="RQR">요청자번호(PTID)</param>
        /// <param name="RQT">요청단말기</param>
        /// <param name="TIRN">발권요청번호(TicketIssuingRequest 서비스의 결과값)</param>
        /// <returns></returns>
        [WebMethod(Description = "자동발권(Command)")]
        public XmlElement TicketIssuingSemiAutoStringRS(int OID, int PID, string FT, string FM, string FE, string FV, string FP, int RQR, string RQT, int TIRN)
        {
            return mas.TicketIssuingSemiAutoStringRS(OID, PID, FT, FM, FE, FV, FP, RQR, RQT, TIRN);
        }

        #endregion "자동발권(Command)"

        #region "자동 발권전 실행(발권 가능여부 체크 및 발권용 운임 재계산)"

        /// <summary>
        /// 자동 발권전 실행(발권 가능여부 체크 및 발권용 운임 재계산)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="OID">모두투어 주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="RIP">요청자IP</param>
        /// <param name="RQR">요청자번호(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "자동 발권전 실행(발권 가능여부 체크 및 발권용 운임 재계산)")]
        public XmlElement TicketIssuingBefore(int SNM, int OID, int PID, string RIP, int RQR, string RQT)
        {
            return mas.TicketIssuingBefore(SNM, OID, PID, RIP, RQR, RQT);
        }

        #endregion "자동 발권전 실행(발권 가능여부 체크 및 발권용 운임 재계산)"

        #region "자동발권(결제시스템 연동전 실행)"

        /// <summary>
        /// 자동발권(결제시스템 연동전 실행)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="RQR">요청자번호(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <returns></returns>
        [WebMethod(Description = "자동발권(결제시스템 연동전 실행)")]
        public XmlElement AutoTicketing01RS(int SNM, int OID, int PID, int RQR, string RQT)
        {
            return mas.AutoTicketing01RS(SNM, OID, PID, RQR, RQT);
        }

        #endregion "자동발권(결제시스템 연동전 실행)"

        #region "자동발권(결제시스템 연동후 실행)"

        /// <summary>
        /// 자동발권(결제시스템 연동후 실행)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="RQR">요청자번호(PTID)</param>
        /// <param name="RQT">요청단말기(WEB/MOBILEWEB/MOBILEAPP/CRS/MODEWARE)</param>
        /// <param name="FOP">결제정보("영어성^영어이름^카드타입^카드번호^유효기간(MMYY)^할부기간^승인번호^카드결제금액^현금결제금액")</param>
        /// <param name="ITREmail">ITR 발송 이메일주소</param>
        /// <returns></returns>
        [WebMethod(Description = "자동발권(결제시스템 연동후 실행)")]
        public XmlElement AutoTicketing02RS(int SNM, int OID, int PID, int RQR, string RQT, string FOP, string ITREmail)
        {
            return mas.AutoTicketing02RS(SNM, OID, PID, RQR, RQT, FOP, ITREmail);
        }

        #endregion "자동발권(결제시스템 연동후 실행)"

        #endregion "자동발권"

        #region "E-Ticket 조회"

        #region "E-Ticket 조회(PNR정보 이용)(탑승객별 문서출력용)"

        /// <summary>
        /// 탑승객별 E-Ticket 조회(PNR정보 이용)(탑승객별 문서출력용)
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="PaxName">탑승객명(ex HONG/GILDONGMR)</param>
        /// <param name="RIP">요청자IP</param>
        /// <returns></returns>
        [WebMethod(Description = "탑승객별 E-Ticket 조회(PNR정보 이용)(탑승객별 문서출력용)")]
        public XmlElement SearchETicketDocRS(int OID, int PID, string PaxName, string RIP)
        {
            return mas.SearchETicketDocRS(OID, PID, PaxName, RIP);
        }

        #endregion "E-Ticket 조회(PNR정보 이용)(탑승객별 문서출력용)"

        #endregion "E-Ticket 조회"

        #region "Agent Coupon 조회"

        /// <summary>
        /// Agent Coupon 조회
        /// </summary>
        /// <param name="OID">주문번호</param>
        /// <param name="PID">예약자번호(PTID)</param>
        /// <param name="TicketNumber">티켓번호</param>
        /// <returns></returns>
        [WebMethod(Description = "Agent Coupon 조회")]
        public string SearchAgentCoupon(int OID, int PID, string TicketNumber)
        {
            return mas.SearchAgentCoupon(OID, PID, TicketNumber);
        }

        #endregion "Agent Coupon 조회"

        #region "카드인증"

        /// <summary>
        /// 이니시스 카드 인증
        /// </summary>
        /// <param name="Owner">소유자명</param>
        /// <param name="CardNumber">카드번호</param>
        /// <param name="ValidThru">유효기간(YYYYMM OR YYMM)</param>
        /// <param name="IDNumbers">개인(생년월일 6자리) / 법인(사업자번호 10자리)</param>
        /// <param name="Password">비밀번호 앞2자리</param>
        /// <returns></returns>
        [WebMethod(Description = "이니시스 카드 인증")]
        public XmlElement CardCheckInicis(string Owner, string CardNumber, string ValidThru, string IDNumbers, string Password)
        {
            return mas.CardCheckInicis(Owner, CardNumber, ValidThru, IDNumbers, Password);
        }

        /// <summary>
        /// 카드사 BIN번호 조회
        /// </summary>
        /// <param name="CardNumber">카드번호(하이픈 제외, 최대 앞 7자리까지)</param>
        /// <returns></returns>
        [WebMethod(Description = "카드사 BIN번호 조회")]
        public XmlElement CardCheckBin(string CardNumber)
        {
            return mas.CardCheckBin(CardNumber);
        }

        #endregion "카드인증"

        #region "마일리지"

        /// <summary>
        /// 투어마일리지 예상 적립액
        /// </summary>
        /// <param name="AdultFare">성인요금</param>
        /// <param name="ChildFare">소아요금</param>
        /// <param name="InfantFare">유아요금</param>
        /// <returns></returns>
        [WebMethod(Description = "투어마일리지 예상 적립액")]
        public XmlElement TourMileageExpected(int AdultFare, int ChildFare, int InfantFare)
        {
            return mas.TourMileageExpected(AdultFare, ChildFare, InfantFare);
        }

        #endregion "마일리지"

        #region "갈릴레오 전용"

        #region "SearchRule(갈릴레오)"

        /// <summary>
        /// 운임규정 조회(갈릴레오)
        /// </summary>
        /// <param name="SNM">사이트 번호</param>
        /// <param name="FareRuleUrl">운임규정 파리미터 정보</param>
        /// <returns></returns>
        [WebMethod(Description = "운임규정 조회(갈릴레오)")]
        public XmlElement SearchRuleGalileoRS(int SNM, string FareRuleUrl)
        {
            return mas.SearchRuleGalileoRS(SNM, FareRuleUrl);
        }

        #endregion "SearchRule(갈릴레오)"

        #endregion "갈릴레오 전용"

        #region "메서드 설명"

        /// <summary>
        /// WebMethod의 입력 파라미터 및 출력값에 대한 설명
        /// </summary>
        /// <param name="WebMethodName">웹메서드명</param>
        /// <returns></returns>
        [WebMethod(Description = "WebMethod의 입력 파라미터 및 출력값에 대한 설명")]
        public XmlElement Help(string WebMethodName)
        {
            return mas.Help(WebMethodName);
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
            return mas.HelpXml(WebMethodName, Gubun);
        }

        #endregion "메서드 설명"
    }
}