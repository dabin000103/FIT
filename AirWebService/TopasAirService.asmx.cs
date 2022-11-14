using System;
using System.ComponentModel;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace AirWebService
{
	/// <summary>
    /// Topas 제공 정보
	/// </summary>
	[WebService(Namespace = "http://airservice2.modetour.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ToolboxItem(false)]
	[ScriptService]
	public class TopasAirService : System.Web.Services.WebService
	{
        Common cm;
        TopasConfig tc;
        HttpContext hcc;

        public TopasAirService()
		{
            cm = new Common();
            tc = new TopasConfig();
            hcc = HttpContext.Current;
		}

        #region "네임스페이스"

        /// <summary>
        /// 네임스페이스 조회
        /// </summary>
        /// <param name="ServiceName">서비스명</param>
        /// <returns></returns>
        [WebMethod(Description = "NamespaceURL")]
        public string NamespaceURL(string ServiceName)
        {
            return TopasConfig.NamespaceURL(ServiceName);
        }

        #endregion "네임스페이스"

        #region "AirLineRequestService"

        [WebMethod(Description = "AirLineRequestServiceRQ")]
        public XmlElement AirLineRequestServiceRQ(string AirCode)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(tc.XmlFullPath("AirLineRequest"));

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            xnMgr.AddNamespace("top", TopasConfig.NamespaceURL("AirLineRequestService"));

            XmlDoc.SelectSingleNode("top:AirLineRequestService/top:airlineCode", xnMgr).InnerText = AirCode;

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 항공사별 가맹점 조회
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "AirLineRequestServiceRS")]
        public XmlElement AirLineRequestServiceRS(string AirCode, string GUID)
        {
            XmlElement ReqXml = AirLineRequestServiceRQ(AirCode);
            //cm.XmlFileSave(ReqXml, tc.Name, "AirLineRequestServiceRQ", "N", GUID);

            XmlElement ResXml = tc.HttpExecute("AirLineRequestService", ReqXml, GUID);
            cm.XmlFileSave(ResXml, tc.Name, "AirLineRequestServiceRS", "N", GUID);

            return ResXml;
        }

        #endregion "AirLineRequestService"

        #region "ApprovalRequestService"

        [WebMethod(Description = "ApprovalRequestServiceRQ")]
        public XmlElement ApprovalRequestServiceRQ(string API, string PNR, string TID, string CPR, string CIP, string ICD, string ISK, string IED, string MCN, string MCV, string MXI, string MEC)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(tc.XmlFullPath("ApprovalRequest"));

            XmlNamespaceManager xnMgr = new XmlNamespaceManager(XmlDoc.NameTable);
            xnMgr.AddNamespace("m", TopasConfig.NamespaceURL("ApprovalRequestService"));

            XmlNode ApprovalRequestService = XmlDoc.SelectSingleNode("m:ApprovalRequestService", xnMgr);
            XmlNode GeneralInfo = ApprovalRequestService.SelectSingleNode("m:generalinfo", xnMgr);
            XmlNode AuthInfo = ApprovalRequestService.SelectSingleNode("m:authinfo", xnMgr);

            GeneralInfo.SelectSingleNode("m:ReqAmt", xnMgr).InnerText = CPR;
            GeneralInfo.SelectSingleNode("m:ReqNo", xnMgr).InnerText = MCN;
            GeneralInfo.SelectSingleNode("m:Authn", xnMgr).InnerText = API;
            GeneralInfo.SelectSingleNode("m:PnrRloc", xnMgr).InnerText = PNR;
            GeneralInfo.SelectSingleNode("m:Tid", xnMgr).InnerText = TID;
            
            if (API.Equals("MPI"))
            {
                GeneralInfo.SelectSingleNode("m:ReqNo", xnMgr).InnerText = MCN;
                AuthInfo.SelectSingleNode("m:cavv", xnMgr).InnerText = MCV;
                AuthInfo.SelectSingleNode("m:halbuinfo", xnMgr).InnerText = CIP;
                AuthInfo.SelectSingleNode("m:eci", xnMgr).InnerText = MEC;
                AuthInfo.SelectSingleNode("m:xid", xnMgr).InnerText = MXI;
            }
            else
            {
                AuthInfo.SelectSingleNode("m:kvp_cardcode", xnMgr).InnerText = ICD;
                AuthInfo.SelectSingleNode("m:kvp_encdata", xnMgr).InnerText = IED;
                AuthInfo.SelectSingleNode("m:kvp_sessionkey", xnMgr).InnerText = ISK;
                AuthInfo.SelectSingleNode("m:kvp_quota", xnMgr).InnerText = CIP;
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// 토파스 카드 결제
        /// </summary>
        /// <param name="API">카드구분값 ISP(BC카드 등), MPI(일반)</param>
        /// <param name="PNR">항공예약번호</param>
        /// <param name="TID">Stock Airline Code</param>
        /// <param name="CPR">승인금액</param>
        /// <param name="CIP">할부개월수(EV_install_period)</param>
        /// <param name="ICD">ISP 카드코드(번호) : KVP_CARDCODE</param>
        /// <param name="ISK">ISP 인증키1 : KVP_SESSIONKEY</param>
        /// <param name="IED">ISP 인증키2 : KVP_ENCADTA</param>
        /// <param name="MCN">MPI 카드번호 : cardno</param>
        /// <param name="MCV">MPI 인증키1 : Cavv</param>
        /// <param name="MXI">MPI 인증키 : xid</param>
        /// <param name="MEC">MPI 인증키 : eci</param>
        /// <param name="GUID">고유번호</param>
        /// <returns></returns>
        [WebMethod(Description = "ApprovalRequestServiceRS")]
        public XmlElement ApprovalRequestServiceRS(string API, string PNR, string TID, string CPR, string CIP, string ICD, string ISK, string IED, string MCN, string MCV, string MXI, string MEC, string GUID)
        {
            XmlElement ReqXml = ApprovalRequestServiceRQ(API, PNR, TID, CPR, CIP, ICD, ISK, IED, MCN, MCV, MXI, MEC);
            //cm.XmlFileSave(ReqXml, tc.Name, "ApprovalRequestServiceRQ", "N", GUID);

            XmlElement ResXml = tc.HttpExecute("ApprovalRequestService", ReqXml, GUID);
            cm.XmlFileSave(ResXml, tc.Name, "ApprovalRequestServiceRS", "N", GUID);

            return ResXml;
        }

        #endregion "ApprovalRequestService"

        #region "운임규정조회(PNR생성전)"

        [WebMethod(Description = "Automated Rule Translator RQ")]
        public string AutomatedRuleTranslatorRQ(int SNM, string SAC, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string PFG, string GUID)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(PFG);
            
            int i = 0;
            int SegCount = INO.Length - 1;
            string TripType = string.Empty;
            string Destination = string.Empty;
            string[] FareType = new String[XmlDoc.SelectNodes("paxFareGroup/paxFare[1]/segFareGroup/segFare/fare/fare").Count];

            if (INO[SegCount].Equals(1))
                TripType = "OW";
            else if (INO[SegCount].Equals(2))
                TripType = DLC[0].Equals(ALC[SegCount]) ? "RT" : "MT";
            else
                TripType = "MT";

            foreach (XmlNode FT in XmlDoc.SelectNodes("paxFareGroup/paxFare[1]/segFareGroup/segFare/fare/fare"))
            {
                string StrFT = string.Empty;

                foreach (XmlNode FT2 in FT.SelectNodes("fareType"))
                    StrFT += String.Concat((String.IsNullOrWhiteSpace(StrFT) ? "" : "^"), FT2.InnerText);

                FareType[(i++)] = StrFT;
            }

            for (i = 0; i <= SegCount; i++)
            {
                //날짜형식 변환
                DTD[i] = cm.RequestDateTime(DTD[i], "ddMMyy");

                //도착지
                if (INO[i].Equals(2) && String.IsNullOrWhiteSpace(Destination))
                    Destination = ALC[(i - 1)];
            }
            
            //도착지(편도일 경우)
            if (String.IsNullOrWhiteSpace(Destination))
                Destination = ALC[SegCount];
            
            return String.Concat(
                    "{",
                        "\"farerulerq\" : {",
                            "\"agtuuid\":\"" + GUID + "\",",
                            "\"data\" : {",
                                "\"depcitycd\" : [ \"" + String.Join("\", \"", DLC) + "\" ],",
                                "\"arrcitycd\" : [ \"" + String.Join("\", \"", ALC) + "\" ],",
                                "\"depdt\" : [ \"" + String.Join("\", \"", DTD) + "\" ],",
                                "\"stockaircd\" : \"" + (String.IsNullOrWhiteSpace(SAC) ? MCC[0].Trim() : SAC) + "\",",
                                "\"aircd\" : [ \"" + String.Join("\", \"", MCC) + "\" ],",
                                "\"flightno\" : [ \"" + String.Join("\", \"", FLN) + "\" ],",
                                "\"bkgclass\" : [ \"" + String.Join("\", \"", RBD) + "\" ],",
                                "\"adtcnt\" : \"" + XmlDoc.SelectNodes("paxFareGroup/paxFare[@ptc!='CHD' and @ptc!='INF']").Count.ToString() + "\",",
                                "\"adtptctype\" : \"" + XmlDoc.SelectSingleNode("paxFareGroup/paxFare[@ptc!='CHD' and @ptc!='INF']").Attributes.GetNamedItem("ptc").InnerText + "\",",
                                "\"infcnt\" : \"" + XmlDoc.SelectNodes("paxFareGroup/paxFare[@ptc='INF']").Count.ToString() + "\",",
                                "\"infptctype\" : \"INF\",",
                                "\"chdcnt\" : \"" + XmlDoc.SelectNodes("paxFareGroup/paxFare[@ptc='CHD']").Count.ToString() + "\",",
                                "\"chdptctype\" : \"CH\",",
                                "\"origin\" : \"" + DLC[0] + "\",",
                                "\"destination\" : \"" + Destination + "\",",
                                "\"faretype\" : [ \"" + String.Join("\", \"", FareType) + "\" ],",
                                "\"corporateid\" : \"" + ((XmlDoc.SelectNodes("paxFareGroup/paxFare[1]/segFareGroup/segFare[1]/fare[1]/corporateId").Count > 0) ? XmlDoc.SelectSingleNode("paxFareGroup/paxFare[1]/segFareGroup/segFare[1]/fare[1]/corporateId").InnerText : "") + "\",",
                                "\"lang\" : \"ko\",",
                                "\"svctype\" : \"1A\",",
                                "\"diflag\" : \"" + (Common.KoreaOfAirport(DLC[0]) ? "D" : "I") + "\",",
                                "\"triptype\" : \"" + TripType + "\"",
                            "}",
                        "}",
                    "}");
        }

        /// <summary>
        /// 운임규정조회(PNR 생성 이전)
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Automated Rule Translator RS")]
        public string AutomatedRuleTranslatorRS(int SNM, string SAC, int[] INO, string[] DTD, string[] DTT, string[] ARD, string[] ART, string[] DLC, string[] ALC, string[] MCC, string[] OCC, string[] FLN, string[] RBD, string PFG, string GUID)
        {
            string ReqXml = AutomatedRuleTranslatorRQ(SNM, SAC, INO, DTD, DTT, ARD, ART, DLC, ALC, MCC, OCC, FLN, RBD, PFG, GUID);
            cm.XmlFileSave(ReqXml, tc.Name, "AutomatedRuleTranslatorRQ", "N", GUID);

            string ResXml = tc.HttpExecute(SNM, "AutomatedRuleTranslator", ReqXml, GUID);
            cm.XmlFileSave(ResXml, tc.Name, "AutomatedRuleTranslatorRS", "N", GUID);

            return ResXml;
        }

        #endregion "운임규정조회(PNR생성전)"

        #region "운임규정조회(PNR생성후)"음.. 

        #endregion "운임규정조회(PNR생성후)"
    }
}