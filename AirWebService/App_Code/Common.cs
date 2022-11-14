using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace AirWebService
{
	public class Common
	{
		private static string mMachineName = System.Environment.MachineName;
		private static string mPhysicalApplicationPath = HttpContext.Current.Request.PhysicalApplicationPath;
        private static Object thisLock = new Object();
		
		/// <summary>
		/// 서버명
		/// </summary>
        public static string MachineName
		{
			get { return mMachineName; }
		}

		/// <summary>
		/// 서버로컬경로
		/// </summary>
        public static string PhysicalApplicationPath
		{
			get { return mPhysicalApplicationPath; }
		}
		
		/// <summary>
		/// 접속자별 고유인덱스 생성
		/// </summary>
		/// <returns></returns>
		public string GetGUID
		{
			get
			{
                lock (thisLock)
				{
					return String.Format("{0}-{1}", MachineName, DateTime.Now.ToString("yyyyMMddHHmmssffff"));
				}
			}
		}

		/// <summary>
		/// 응답일시
		/// </summary>
		public string TimeStamp
		{
			get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
		}

		/// <summary>
		/// 항공사별 호출 GDS
        /// </summary>
        /// <param name="GDS">GDS코드</param>
		/// <param name="AirCode">항공사코드</param>
		/// <returns></returns>
		public static bool AirlineHost(string GDS, string AirCode)
		{
            //SK항공 제외(2016-03-03,김지영과장)
            //PC항공 포함(2016-05-09,김지영과장)
            //LX항공 제외(2018-01-08,김경미차장)
            //AI항공 제외(2018-11-23,김경미매니저)
            if (GDS.Equals("Amadeus"))
                return ("/AA/AC/AF/AM/AT/AY/AZ/BA/BI/BR/BT/BX/B7/CA/CI/CX/CZ/DL/DT/EK/ET/EY/FJ/FV/GA/GF/HA/HM/HU/HX/JD/JJ/JL/KC/KE/KL/KQ/K6/LA/LH/LJ/LO/LY/MD/MF/MH/MR/MU/NH/NX/NZ/OK/OM/OS/OZ/PC/PG/PR/PS/QF/QR/QV/RA/RJ/RS/SA/SB/SC/SQ/SU/S7/TG/TK/TP/TW/TZ/UA/UL/UN/VJ/VN/VT/WY/ZE/ZH/7C/8M/9W/".IndexOf(AirCode) != -1) ? true : false;
            else if (GDS.Equals("Sabre"))
                return ("/BX/PR/RS/5J/".IndexOf(AirCode) != -1) ? true : false;
            else
                return false;
		}

		/// <summary>
        /// PNR 생성 GDS 셋팅
        /// </summary>
        /// <param name="SNM">사이트번호</param>
		/// <param name="AirCode">항공사코드</param>
		/// <returns></returns>
        public static string AirlineBookingHost(int SNM, string AirCode)
		{
            //7C항공은 Abacus로 예약(2017-05-23,김지영차장)
            //7C항공은 Amadeus로 예약(2017-05-31,김경미차장)
            //BI항공은 Amadeus로 예약(2017-05-31,김경미차장)
            //HA항공은 Galileo로 예약(2018-10-10,김경미매니저)
            //CRS(68) 예약인 경우 HA항공은 Amadeus로 예약(2018-10-17,김경미매니저)
            return ("/BT/BX/FJ/MD/OZ/RA/RJ/UN/WY/".IndexOf(AirCode) != -1) ? "Abacus" : ("/HA/".IndexOf(AirCode) != -1) ? (SNM.Equals(68) ? "Abacus" : "Galileo") : "Amadeus";
		}

        /// <summary>
        /// PNR 생성 GDS 셋팅
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="GDS">운임GDS</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="CodeShare">코드쉐어 여부</param>
        /// <param name="AirShare">여정에 다른 항공사 포함 여부</param>
        /// <param name="GDSType">GDS지정</param>
        /// <returns></returns>
        public static string AirlineBookingHost2(int SNM, string GDS, string ROT, string AirCode, bool CodeShare, bool AirShare, string GDSType)
        {
            string HostGDS = GDS;

            if (GDS.Equals("Galileo"))
            {
                //LCC는 아마데우스로 예약
                //갈릴레오 운임일 경우 GDSType 정보에 따라 갈릴레오/아바쿠스 예약 GDS 정의(2016-09-05,갈릴레오 요청)
                //에어부산(BX), 에어서울(RS)는 세이버로 예약(2017-04-13,김지영과장)
                //아시아나(OZ) 세이버로 예약(2017-08-11,연선미차장)
                //아시아나(OZ) 갈릴레오로 예약(2017-08-28,김지영차장)
                //진에어(LJ)는 갈릴레오로 예약(2018-10-17,김지영팀장)
                if (AirCode.Equals("TW") || AirCode.Equals("ZE") || AirCode.Equals("VJ"))
                    HostGDS = "Amadeus";
                else if (GDSType.Equals("B") || AirCode.Equals("BX") || AirCode.Equals("RS"))
                    HostGDS = "Abacus";
                else
                    HostGDS = "Galileo";
            }
            else if (CodeShare)
            {
                //코드쉐어인 경우 FareGDS로 예약(2016-08-08,김지영과장)
                HostGDS = (GDS.Equals("Amadeus")) ? AirlineBookingHost(SNM, AirCode) : GDS;
            }
            else
            {
                if (SNM.Equals(2) || SNM.Equals(3915))
                {
                    if (GDS.Equals("Amadeus"))
                    {
                        //닷컴 아시아나는 갈릴레오로 예약(2016-08-26,김지영과장)
                        //닷컴 아시아나는 세이버로 예약(2017-01-19,김지영과장)
                        //닷컴 아시아나는 갈릴레오로 예약(2019-06-24,김지영팀장)
                        //닷컴 아시아나는 세이버로 예약(2019-06-28,김경미매니저)
                        if ("/OZ/".IndexOf(AirCode) != -1)
                        {
                            //여정 전체가 OZ인 경우에만 갈릴레오 예약 가능(2016-09-05,갈릴레오 요청)
                            //if (!AirShare)
                            //    HostGDS = "Galileo";
                            //else
                                HostGDS = "Abacus";
                        }
                        else if ("/BT/BX/FJ/MD/RA/RJ/UN/WY/5J/".IndexOf(AirCode) != -1)
                            HostGDS = "Abacus";
                        //닷컴 HA항공은 갈릴레오로 예약(2016-12-05,김지영과장)
                        else if ("/HA/".IndexOf(AirCode) != -1)
                            HostGDS = "Galileo";
                    }
                }
                else if (GDS.Equals("Amadeus"))
                {
                    HostGDS = AirlineBookingHost(SNM, AirCode);
                }
            }

            return HostGDS;
        }

        /// <summary>
        /// 프리미엄이코노미 가능 공항
        /// </summary>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <returns></returns>
        public static bool PremiumEconomy(string ALC)
        {
            return ("/SIN/BKK/HKT/MNL/SGN/HAN/KUL/DPS/KLO/CNX/JKT/DAD/KTM/PNH/REP/CMB/MLE/NYC/EWR/JFK/LGA/LAX/YVR/SFO/SEA/YTO/HNL/DFW/IAD/CHI/ATL/LAS/HOU/".IndexOf(ALC) != -1) ? true : false;
        }

		/// <summary>
		/// 특정 항공사의 경우 PUB운임은 제외시킨다.
		/// </summary>
		/// <param name="AirCode">항공사코드</param>
		/// <param name="FareInfo">fareType 정보</param>
        /// <param name="xnMgr">XmlNamespaceManager</param>
        /// <param name="PUB">PUB운임 출력여부</param>
        /// <param name="DepartureFromKorea">한국출발여부</param>
		/// <returns></returns>
        public static bool ExcludePubFare(string AirCode, XmlNode FareInfo, XmlNamespaceManager xnMgr, string PUB, bool DepartureFromKorea)
		{
			if (PUB.Equals("N"))
				return true;
			else
			{
				//7C,TW 항공은 해외출발인 경우 PUB운임 허용(2017-10-24,김지영)
                string EPAir = DepartureFromKorea ? "/7C/TW/ZE/MU/LJ/Z2/QV/PR/" : "/ZE/MU/LJ/Z2/QV/PR/";

				if (EPAir.IndexOf(AirCode) != -1)
					return (FareInfo.SelectNodes("m:fareType[.='RP']", xnMgr).Count > 0) ? false : true;
				else
					return true;
			}
		}

        /// <summary>
        /// MPIS 필터링
        /// </summary>
        /// <param name="MPIS">MPIS 여부</param>
        /// <param name="CCD">검색 요청 캐빈클래스</param>
        /// <param name="ProductInformation">운임정보 XMLNode</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <returns></returns>
        public static bool MPISFilter(bool MPIS, string CCD, XmlNode ProductInformation, XmlNamespaceManager xnMgr)
        {
            if (MPIS)
            {
                if (ProductInformation.SelectNodes(String.Format("m:fareDetails/m:groupOfFares/m:productInformation/m:cabinProduct/m:cabin[.!='{0}']", CCD), xnMgr).Count > 0)
                    return false;
                else if (ProductInformation.SelectNodes("m:fareDetails/m:groupOfFares/m:productInformation/m:fareProductDetail/m:fareType[.='RP']", xnMgr).Count > 0)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 스케쥴 출력시 예외처리
        /// </summary>
        /// <param name="MCC">마케팅항공사</param>
        /// <param name="OCC">운항항공사</param>
        /// <returns></returns>
        public static bool ScheduleExceptionHandling(string MCC, string OCC)
        {
            //AS항공 제외(2018-05-31,송인혁매니저)
            string Airline = "/JU/AS/";
            return (Airline.IndexOf(MCC) != -1 || Airline.IndexOf(String.Format("/{0}/", OCC)) != -1) ? true : false;
        }

        /// <summary>
        /// 운임출력시 예외처리
        /// </summary>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="DLC">출발지 공항 코드</param>
        /// <param name="ALC">도착지 공항 코드</param>
        /// <param name="ROT">구간(OW:편도, RT:왕복, DT:출도착이원구간, MD:다구간)</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <returns></returns>
        public static bool FareExceptionHandling(string AirCode, string DLC, string ALC, string ROT, string DTD)
        {
            bool ExceptionYN = true;
            
            //체코항공은 인천-프라하(편도,왕복)만 가능(2015-02-11,정성하대리요청) => 체코 모두 오픈(2015-05-26,진경욱과장)
            //if (AirCode.Equals("OK"))
            //    ExceptionYN = (DLC.Equals("ICN") && ALC.Equals("PRG") && (ROT.Equals("OW") || ROT.Equals("RT"))) ? true : false;

            //출발월이 2월인 경우 티웨이항공(TW) 제외(2016-02-05,김지영과장)
            //if (AirCode.Equals("TW"))
            //{
            //    if (new Common().RequestDateTime(DTD.Split(',')[0].Trim(), "MM").Equals("02"))
            //        ExceptionYN = false;
            //}

            //트래블스카이(1E) 시스템 정기점검으로 인해 다음 기간 동안 해당 항공사 제외(2018-10-19,김지영팀장)
            //if ("/3U/8L/BD/CA/CN/CZ/DZ/FM/GS/H9/HO/HU/HX/JD/LQ/MF/MU/NS/NX/O8/OX/PN/QD/SC/TV/ZH/".IndexOf(AirCode) != -1)
            //{
            //    DateTime NowDate = DateTime.Now;
            //    Common cm = new Common();
                
            //    //1차 (2018/10/19 23:30 ~ 10/20 03:30)
            //    if (cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-10-19 23:00") <= 0 && cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-10-20 03:30") >= 0)
            //    {
            //        ExceptionYN = false;
            //    }
            //    //2차 (2018/10/24 01:00 ~ 10/24 02:30)
            //    else if (cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-10-24 00:30") <= 0 && cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-10-24 02:30") >= 0)
            //    {
            //        ExceptionYN = false;
            //    }
            //}

            return ExceptionYN;
        }

		/// <summary>
		/// 한국 공항 여부
		/// </summary>
		/// <param name="Airport">공항코드</param>
		/// <returns></returns>
		public static bool KoreaOfAirport(string Airport)
		{
			return ("/SEL/ICN/GMP/CJJ/CJU/HIN/KAG/KPO/KUV/KWJ/MPK/MWX/PUS/RSU/SHO/TAE/USN/WJU/YEC/YNY/".IndexOf(Airport) != -1) ? true : false;
		}

		/// <summary>
		/// 필리핀 공항 여부
		/// </summary>
		/// <param name="Airport">공항코드</param>
		/// <returns></returns>
		public static bool PhilippinesOfAirport(string Airport)
		{
			return ("/AAV/BAG/BCD/BPH/BSO/BXU/CBO/CEB/CGY/CRK/CRM/CUJ/CYP/CYZ/DGT/DPL/DVO/GES/IGN/ILO/JOL/KLO/LAO/LBX/LGP/MBO/MBT/MLP/MNL/MPH/MRQ/MXI/OMC/OZC/PAG/PPS/RXS/SFE/SFS/SJI/SUG/TAC/TAG/TBH/TDG/TUG/TWT/USU/VRC/WNP/XMA/ZAM/".IndexOf(Airport) != -1) ? true : false;
		}

		/// <summary>
		/// 미국 공항 여부
		/// </summary>
		/// <param name="Airport">공항코드</param>
		/// <returns></returns>
		public static bool UnitedStatesOfAirport(string Airport)
		{
			return ("/ABE/ABI/ABL/ABQ/ABR/ABY/ACK/ACT/ACV/ACY/ADK/ADM/ADQ/ADW/AED/AET/AGN/AGS/AHD/AHH/AHN/AIA/AID/AIN/AIO/AIY/AIZ/AKB/AKI/AKK/AKN/AKO/AKP/ALB/ALE/ALM/ALO/ALS/ALW/ALZ/AMA/ANA/ANB/ANC/AND/ANI/ANN/ANV/ANY/AOH/AOO/AOS/APA/APF/APN/APV/ARA/ARB/ARC/ARG/ART/ARV/ARX/ASE/ASH/ASQ/AST/ASX/ATE/ATK/ATL/ATO/ATT/ATU/ATW/ATY/AUG/AUK/AUO/AUS/AUW/AUZ/AVL/AVP/AVX/AXB/AXN/AXS/AXX/AYS/AZO/BAM/BBF/BBX/BCB/BCE/BDG/BDL/BDR/BED/BEH/BET/BFD/BFF/BFI/BFL/BFP/BFT/BGM/BGR/BGS/BHB/BHC/BHM/BIC/BID/BIG/BIH/BIL/BIS/BJC/BJI/BKC/BKE/BKF/BKL/BKW/BKX/BLF/BLH/BLI/BLM/BMG/BMI/BML/BNA/BNF/BNG/BOI/BOS/BOT/BPI/BPT/BQK/BRD/BRG/BRL/BRO/BRW/BSQ/BSW/BSZ/BTI/BTL/BTM/BTN/BTP/BTR/BTT/BTV/BUF/BUR/BVX/BVY/BWD/BWI/BWM/BWS/BXC/BXS/BYA/BYW/BZN/CAD/CAE/CAK/CBA/CBE/CBK/CBZ/CCB/CCG/CCR/CDB/CDC/CDH/CDL/CDR/CDV/CEC/CEF/CEM/CEU/CEY/CEZ/CFA/CGA/CGF/CGI/CGX/CHA/CHI/CHL/CHO/CHP/CHS/CHU/CIB/CIC/CID/CIG/CIK/CIL/CIU/CIV/CJN/CKB/CKD/CKV/CKX/CLC/CLD/CLE/CLL/CLM/CLP/CLS/CLT/CLU/CMH/CMI/CMX/CMY/CNE/CNM/CNO/CNY/COA/COD/COE/CON/COS/COU/CPM/CPR/CRE/CRP/CRW/CRX/CSE/CSG/CSN/CSQ/CTH/CTK/CTW/CUW/CVG/CVN/CVO/CWA/CWI/CWS/CXC/CXL/CYF/CYM/CYS/CYT/CZF/CZN/CZP/DAB/DAG/DAL/DAN/DAY/DBN/DBQ/DCA/DDC/DEC/DEH/DEN/DET/DFW/DHN/DIK/DIO/DJN/DLG/DLH/DLL/DLO/DMO/DNS/DNV/DOF/DOV/DPA/DRA/DRE/DRG/DRO/DRT/DSI/DSM/DTA/DTH/DTL/DTR/DTT/DTW/DUF/DUG/DUJ/DUT/DVL/DVN/DXR/EAA/EAN/EAR/EAT/EAU/ECA/EDA/EDE/EDF/EDK/EDW/EEK/EEN/EFD/EFK/EGE/EGP/EGV/EGX/EHM/EKI/EKN/EKO/EKX/ELA/ELD/ELI/ELM/ELP/ELV/ELW/ELY/EMK/EMM/EMT/ENA/ENN/ENV/ENW/ERI/ESC/ESD/ESF/EST/ETN/ETS/EUE/EUG/EVV/EVW/EWB/EWN/EWR/EXI/EYR/EYW/FAI/FAK/FAR/FAT/FAY/FBK/FCA/FDK/FEP/FFM/FFT/FHU/FIC/FID/FIL/FKL/FLG/FLL/FLO/FLT/FLX/FMH/FMN/FMS/FMY/FNL/FNR/FNT/FOD/FOE/FPR/FRD/FRG/FRM/FRP/FSD/FSK/FSM/FSU/FTL/FTW/FTY/FUL/FWA/FWL/FXY/FYM/FYU/FYV/GAD/GAL/GAM/GBD/GBG/GCC/GCK/GCN/GCY/GDV/GEG/GEY/GFB/GFK/GFL/GGG/GGW/GJT/GKN/GKT/GLD/GLH/GLR/GLS/GLV/GMU/GNU/GNV/GNY/GOL/GON/GPT/GPZ/GQQ/GRB/GRD/GRI/GRR/GSO/GSP/GST/GTF/GTR/GUC/GUF/GUP/GUY/GVL/GVT/GWO/GXY/GYY/HAE/HAR/HBC/HBH/HBR/HCB/HCR/HCW/HDA/HDN/HDQ/HES/HEZ/HFD/HGR/HHH/HHR/HIB/HIE/HII/HKA/HKB/HKY/HLN/HMT/HNH/HNL/HNM/HNS/HOB/HOM/HON/HOT/HOU/HPB/HPN/HPT/HPV/HQM/HRL/HRO/HSH/HSI/HSL/HSP/HST/HSV/HTH/HTO/HTS/HUF/HUL/HUM/HUS/HUT/HVN/HVR/HWD/HWI/HYA/HYG/HYL/HYR/HYS/HZL/IAD/IAG/IAH/IAN/ICT/ICY/IDA/IDI/IFP/IGG/IGM/IJX/IKK/IKO/ILE/ILG/ILI/ILM/ILN/IMT/IND/INL/INT/INW/IPL/IPT/IRB/IRC/IRK/IRS/ISM/ISN/ISO/ISP/ISQ/ISS/ISW/ITH/ITO/IWD/IWS/IYK/JAC/JAN/JAX/JBC/JBP/JBR/JBS/JCE/JDM/JDX/JEF/JFK/JGC/JGQ/JHC/JHM/JHW/JHY/JID/JLN/JMC/JMH/JMS/JNU/JOC/JPB/JPT/JRA/JRE/JSL/JST/JTC/JVI/JVL/JWH/JWS/JXN/KAE/KAL/KBC/KBE/KBK/KBW/KCC/KCG/KCL/KCQ/KDK/KEB/KEH/KEK/KFP/KGK/KGX/KIB/KIC/KKA/KKB/KKH/KKI/KKL/KKT/KKU/KLG/KLL/KLN/KLW/KMO/KMY/KNB/KNK/KNT/KNW/KOA/KOT/KOY/KOZ/KPB/KPC/KPD/KPK/KPN/KPR/KPT/KPV/KPY/KQA/KTB/KTN/KTS/KTY/KUK/KUY/KVC/KVL/KWF/KWK/KWN/KWP/KXA/KYK/KYL/KYU/KZB/KZH/LAA/LAF/LAL/LAM/LAN/LAR/LAS/LAW/LAX/LBB/LBE/LBF/LBL/LCH/LCI/LDJ/LEB/LEW/LEX/LFK/LFT/LGA/LGB/LGC/LGD/LGU/LHU/LIH/LIJ/LIT/LJN/LKE/LLY/LMA/LMT/LNK/LNS/LNY/LOL/LOW/LOZ/LPC/LPO/LPS/LPW/LQK/LRD/LRU/LSE/LSK/LTH/LTW/LUK/LUL/LUP/LUR/LVD/LVK/LVM/LWB/LWC/LWL/LWS/LWT/LWV/LXN/LXV/LYH/LYO/MAF/MBL/MBS/MCD/MCE/MCG/MCI/MCK/MCL/MCN/MCO/MCW/MDF/MDH/MDR/MDT/MDW/MEI/MEJ/MEM/MEO/MFD/MFE/MFI/MFR/MFV/MGC/MGJ/MGM/MGR/MGW/MGY/MHE/MHK/MHL/MHT/MIA/MIE/MIF/MIV/MIW/MJQ/MKC/MKE/MKG/MKK/MKL/MKT/MLB/MLC/MLF/MLI/MLJ/MLL/MLS/MLU/MLY/MMH/MML/MMN/MMS/MMU/MNM/MNN/MNT/MNZ/MOB/MOD/MOP/MOR/MOT/MOU/MOX/MPB/MPE/MPJ/MPS/MPV/MQB/MQI/MQT/MQW/MRI/MRK/MRY/MSC/MSD/MSL/MSN/MSO/MSP/MSS/MSV/MSY/MTH/MTJ/MTM/MTO/MTW/MUE/MUV/MVC/MVE/MVL/MVM/MVN/MVW/MVY/MWA/MWH/MWL/MWS/MXA/MXC/MXG/MXO/MXY/MYF/MYH/MYR/MYU/MZJ/NCN/NGC/NHX/NIB/NIE/NKI/NLE/NLG/NME/NNK/NNL/NPH/NPT/NRS/NTJ/NUI/NUL/NUP/NYC/OAJ/OAK/OBU/OCA/OCE/OCF/OCH/OCN/OCW/ODW/OFK/OGG/OGS/OIC/OKC/OKK/OLF/OLH/OLM/OLS/OLU/OMA/OME/ONH/ONN/ONO/ONP/ONT/OOK/OOO/ORD/ORF/ORH/ORI/ORL/ORT/ORV/OSB/OSC/OSH/OSX/OTG/OTH/OTM/OTS/OTZ/OWB/OWD/OXC/OXR/OYS/PAE/PAH/PAQ/PBF/PBI/PBK/PCA/PCD/PCT/PDB/PDT/PDX/PEC/PFA/PFN/PGA/PGD/PGM/PGS/PGV/PHF/PHL/PHO/PHT/PHX/PIA/PIB/PIE/PIH/PII/PIM/PIP/PIR/PIT/PIZ/PKA/PKB/PLB/PLK/PLN/PMD/PML/PNC/PNE/PNF/PNS/PNU/PNX/POC/POE/POF/POJ/POQ/POU/POY/PPC/PPF/PPM/PPV/PQI/PQS/PRB/PRC/PRF/PRL/PRW/PRX/PSB/PSC/PSF/PSG/PSK/PSM/PSP/PSQ/PTA/PTC/PTD/PTH/PTK/PTL/PTN/PTR/PTS/PTU/PUB/PUC/PUL/PUO/PUW/PVC/PVD/PVU/PWK/PWM/PWT/PYM/QCE/QFE/QKB/QKS/RAL/RAP/RBF/RBG/RBH/RBK/RBN/RBY/RCE/RDB/RDD/RDG/RDM/RDU/RDV/RFD/RGR/RHI/RIC/RIE/RIF/RIL/RIW/RKD/RKH/RKS/RLD/RLU/RMG/RMP/RNC/RNG/RNH/RNO/ROA/ROC/ROG/ROW/RRL/RSH/RSJ/RSP/RST/RSW/RUI/RUT/RVR/RWB/RWI/RWL/SAA/SAC/SAD/SAF/SAN/SAT/SAV/SBA/SBM/SBN/SBO/SBP/SBS/SBT/SBY/SCC/SCE/SCF/SCJ/SCK/SCM/SDF/SDM/SDP/SDX/SDY/SEA/SEF/SFO/SFY/SGF/SGR/SGU/SGY/SHD/SHG/SHH/SHR/SHV/SHX/SIK/SIT/SIY/SJC/SJT/SKK/SKW/SLC/SLE/SLK/SLN/SLO/SLQ/SLT/SMF/SMK/SMO/SMX/SNA/SNP/SNS/SNY/SOP/SOV/SOW/SPA/SPF/SPI/SPQ/SPS/SPW/SPZ/SQA/SQI/SQV/SRQ/SRV/SSI/SSM/STC/STE/STG/STK/STL/STP/STQ/STS/SUA/SUC/SUE/SUM/SUN/SUS/SUW/SUX/SVA/SVC/SVS/SVW/SWD/SWF/SWO/SXC/SXP/SXQ/SXY/SYA/SYB/SYR/SZP/TAD/TAL/TBN/TCL/TCT/TEH/TEK/TEX/TGE/THP/TIW/TIX/TKA/TKE/TKF/TKI/TKJ/TLA/TLF/TLH/TLJ/TLT/TNC/TNK/TNP/TOA/TOG/TOL/TOP/TPA/TPH/TPL/TRI/TRM/TSM/TSS/TTN/TTO/TUL/TUP/TUS/TVC/TVF/TVL/TWA/TWD/TWF/TXK/TYR/TYS/TYZ/UBS/UCA/UDD/UES/UGB/UGI/UIN/UKN/ULM/UMT/UNK/UOX/UPP/UQE/UST/UTO/VAK/VCB/VCT/VDZ/VEE/VEL/VEX/VGT/VIS/VJI/VLD/VNC/VNY/VPS/VPZ/VRB/VSF/VYS/WAA/WAH/WAS/WAX/WBB/WBN/WBQ/WBU/WCR/WDG/WDN/WFB/WFK/WGO/WHD/WHR/WHT/WJF/WKK/WKL/WLB/WLK/WLL/WLM/WLR/WMC/WMH/WMK/WMO/WNA/WNC/WOW/WRG/WRL/WSG/WSJ/WSM/WSN/WST/WSX/WTK/WTL/WVL/WWA/WWD/WWP/WWT/WYB/WYS/XES/XMD/XNA/YAK/YKM/YKN/YNG/YUM/ZBS/ZBV/ZBX/ZHC/ZJO/ZNC/ZRA/ZRF/ZRK/ZRL/ZSH/ZSM/ZTL/ZTY/".IndexOf(Airport) != -1) ? true : false;
		}

        /// <summary>
        /// 캐나다 공항 여부
        /// </summary>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static bool CanadaOfAirport(string Airport)
        {
            return ("/CJH/ILF/KES/KNY/MSA/SUR/VTP/WNN/WPL/XPK/XPP/XQU/XRR/XSI/XTL/YAA/YAB/YAC/YAD/YAF/YAG/YAJ/YAL/YAM/YAQ/YAT/YAV/YAX/YAY/YAZ/YBA/YBB/YBC/YBD/YBE/YBF/YBG/YBI/YER/YEY/YFA/YFB/YFC/YFE/YFH/YFJ/YFO/YFR/YFS/YFX/YGA/YGB/YGE/YGG/YGH/YGK/YGL/YGN/YGO/YGP/YGQ/YGR/YGT/YGV/YGW/YGX/YGY/YGZ/YHA/YHC/YHD/YHF/YHG/YHH/YHI/YHK/YHM/YHN/YHO/YHP/YHR/YHS/YHU/YHY/YHZ/YIB/YIF/YIG/YMW/YMX/YMY/YNA/YNC/YND/YNE/YNK/YNL/YNM/YNO/YNS/YOC/YOD/YOE/YOG/YOH/YOJ/YOO/YOP/YOS/YOW/YPA/YPB/YPC/YPD/YPE/YPH/YPI/YPJ/YPL/YPM/YPN/YPO/YPP/YPQ/YPR/YPS/YPT/YPW/YPX/YPY/YQA/YQB/YQC/YTD/YTE/YTF/YTG/YTH/YTJ/YTL/YTO/YTP/YTQ/YTR/YTS/YTT/YTX/YTZ/YUB/YUD/YUF/YUL/YUT/YUX/YUY/YVB/YVC/YVE/YVM/YVO/YVP/YVQ/YVR/YVT/YVZ/YWB/YWF/YWG/YWH/YWJ/YWK/YWL/YWM/YWN/YWO/YWP/YWQ/YWS/YWY/YXC/YXD/ZBF/ZBM/ZEL/ZEM/ZFA/ZFB/ZFD/ZFL/ZFM/ZFN/ZFV/ZGF/ZGI/ZGR/ZGS/ZHP/ZJG/ZKE/ZKG/ZLT/ZMT/ZNA/XBE/XBR/XCL/XCM/XGL/XGR/XKS/XLB/YXE/YXH/YXJ/YXK/YXL/YXN/YXP/YXR/YXS/YXT/YXU/YXX/YXY/YXZ/YYA/YYB/YYC/YYD/YYE/YYF/YYG/YYH/YYJ/YYL/YYN/YYQ/YYR/YYT/YYU/YYY/YYZ/YZE/YZF/YZG/YZP/YZR/YZS/YZT/YZV/YZX/ZAA/ZAC/LAK/YIK/YIO/YIV/YJF/YJO/YJT/YKA/YKE/YKF/YKG/YKK/YKL/YKQ/YKT/YKU/YKX/YKY/YKZ/YLC/YLD/YLE/YLH/YLJ/YLL/YLO/YLP/YLR/YLS/YLV/YLW/YLY/YMA/YMB/YMC/YME/YMF/YMG/YMH/YMI/YML/YMM/YMN/YMO/YMP/YMQ/YMT/YQD/YQF/YQG/YQH/YQI/YQK/YQL/YQM/YQN/YQQ/YQR/YQS/YQT/YQU/YQV/YQW/YQX/YQY/YQZ/YRA/YRB/YRD/YRG/YRI/YRJ/YRL/YRN/YRR/YRS/YRT/YSB/YSC/YSE/YSF/YSG/YSI/YSJ/YSK/YSL/YSM/YSN/YSO/YSP/YSR/YST/YSV/YSX/YSY/YSZ/YTA/YTB/YTC/PIW/QBC/SSQ/ZNG/ZNU/ZOF/ZPB/ZQS/ZRJ/ZRR/ZSJ/ZSP/ZST/ZSW/ZTB/ZTM/ZTS/ZUC/ZUM/ZWL/YBJ/YBK/YBL/YBM/YBQ/YBR/YBS/YBT/YBV/YBW/YBX/YBY/YBZ/YCA/YCB/YCC/YCD/YCF/YCG/YCH/YCK/YCL/YCN/YCO/YCP/YCQ/YCR/YCS/YCU/YCY/YCZ/YDA/YDC/YDE/YDF/YDG/YDI/YDL/YDN/YDO/YDP/YDQ/YDT/YDX/YEA/YED/YEG/YEK/YEL/YEM/AKV/CXH/DUQ/KIF/LRQ/SYF/TUX/".IndexOf(Airport) != -1) ? true : false;
        }

        /// <summary>
        /// 중국 공항 여부
        /// </summary>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static bool ChinaOfAirport(string Airport)
        {
            return ("/AKA/AKU/AQG/BAV/KWE/KWL/CHW/CIF/CIH/CKG/AAT/BHY/CSX/DAX/DLU/DNH/FOC/HEK/HET/HFE/HGH/HHA/HRB/HSC/HSN/HTN/HUZ/INC/JDZ/JUZ/KHG/KHN/KOW/LHK/LYA/LYG/LZD/LZH/LZO/MXZ/NAO/NKG/NNG/NNY/PEK/SHA/SHE/SHF/SHP/SHS/SIA/SYX/SZO/SZV/SZX/TAO/TSN/UYN/WEF/WEH/WNZ/WUH/WUS/WUX/WUZ/WXN/XMN/XNN/XNT/XUZ/YNJ/YNT/YNZ/ZGC/ZHA/DSN/DYG/GOQ/KMG/LHW/LJG/NTG/SJW/SWA/SYM/TCG/TGO/XFN/XIC/XIL/XIN/XIY/ZAT/CAN/HYN/HZG/JGN/JHG/JIL/JIU/JJN/JMU/JNG/JNZ/KCA/NAY/NDG/NGB/RUG/TXN/TYN/AOG/KRL/KRY/YIH/YIN/YIW/BFU/DDG/DGM/DLC/URC/CGD/CGO/CGQ/CHG/HLD/HLH/HNY/TNA/TNH/IQM/IQN/MDG/CTU/BJS/PVG/ENH/ENY/FUG/FUO/FYN/WHU/LUM/LXA/BSD/HAK/ZUH/YBP/".IndexOf(Airport) != -1) ? true : false;
        }

        /// <summary>
        /// 베트남 공항 여부
        /// </summary>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static bool VietnamOfAirport(string Airport)
        {
            return ("/BMV/CXR/DAD/DLI/HUI/SGN/UIH/VCA/XVL/NHA/VII/VKG/DIN/PQC/HPH/PXU/SQH/HAN/".IndexOf(Airport) != -1) ? true : false;
        }

        /// <summary>
        /// 도시코드와 공항코드가 동일하면서 1개 도시에 2개 이상의 공항이 존재하는 도시코드
        /// </summary>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static bool SameCityAirport(string Airport)
        {
            return ("/ADM/ADQ/AGH/AHB/AIY/AMM/APW/AUW/AVX/BDL/BEG/BER/BFS/BHX/BHZ/BIM/BNK/BRU/BSL/BUH/BZE/CAS/CEQ/CPH/CQF/CSG/CVG/CWB/DAY/DOM/EIN/EIS/FAI/FMY/FNA/FUK/FYV/GLA/GOI/HAJ/HAM/HAR/HIJ/HUC/IEV/ISC/JKT/KCL/KIN/KTN/LAE/LHW/LIH/LUL/LYS/MES/MKC/MLI/MLW/MMA/MON/MSC/MSQ/NIC/NTL/NUE/NYC/OAG/ORN/OSA/PBM/PMD/PRY/PSP/PTY/REK/RIO/SAF/SGI/SHA/SJO/SJU/SLU/SMF/SNA/SRZ/SSI/SSM/STT/STX/SXB/SXM/TCI/TEX/TOP/TPA/TPE/WAS/WDH/WHR/YAZ/YBL/YCD/YEA/YHZ/YPR/YUF/YVA/YYJ/ZBX/".IndexOf(Airport) != -1) ? true : false;
        }

        /// <summary>
        /// 발권시 소아/유아 생년월일 및 티켓번호 입력 필수 항공사
        /// </summary>
        /// <param name="AirCode">항공사코드</param>
        /// <returns></returns>
        public static bool RequiredDOB(string AirCode)
        {
            return ("/PR/CZ/CA/MU/".IndexOf(AirCode) != -1) ? true : false;
        }

        /// <summary>
        /// 여정의 출발/도착지 코드에 대해 도시 또는 공항코드 여부 지정
        /// </summary>
        /// <param name="ACQ">(A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시)</param>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static string CheckAirportCity(string ACQ, string Airport)
        {
            //A:공항, C:도시, S:공항코드와 도시코드가 동일한 경우에는 공항으로 그 외에는 도시
            if (ACQ.Equals("S"))
                return SameCityAirport(Airport) ? "A" : "C";
            else
                return ACQ;
        }

		/// <summary>
		/// 도시별 공항코드 정보
		/// </summary>
		/// <param name="CityCode">도시코드</param>
		/// <returns></returns>
		public static string CityToAirport(string CityCode)
		{
			return CityCode.Equals("SEL") ? "/ICN/GMP/" : CityCode;
		}

        /// <summary>
        /// 동일 도시에 존재하는 모든 공항 출력
        /// </summary>
        /// <param name="Airport">공항코드</param>
        /// <returns></returns>
        public static string SelAirport(string Airport)
        {
            return (Airport.Equals("ICN") || Airport.Equals("GMP")) ? "ICN/GMP" : Airport;
        }

        /// <summary>
        /// 숫자만 존재하는 날짜/전화번호/카드번호 타입으로 변경
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string ConvertToOnlyNumber(string StrData)
        {
            return StrData.Replace("-", "").Replace(":", "").Replace(".", "").Replace(" ", "");
        }
        public static string ConvertToOnlyNumberDate(string StrData)
        {
            return StrData.Replace("-", "").Replace(":", "").Replace(".", "").Replace(" ", "");
        }

        /// <summary>
        /// Amadeus용 날짜타입으로 변경
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ConvertToAmadeusDate(string date)
		{
			return RequestDateTime(date, "ddMMyy");
		}

		/// <summary>
		/// Amadeus용 시간타입으로 변경
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public string ConvertToAmadeusTime(string time)
		{
			return (time.Length.Equals(4)) ? time : ((time.Length.Equals(5) && time.IndexOf(":") != -1) ? time.Replace(":", "") : (IsDateTime(time) ? RequestDateTime(time, "HHmm") : ""));
		}

		/// <summary>
		/// Amadeus용 날짜와 시간을 한국식으로 변경
		/// </summary>
		/// <param name="date">DDMMYY 형식의 날짜</param>
		/// <param name="time">HHMM 형식의 시간</param>
		/// <returns>YYYY-MM-DD HH:MM 형식의 날짜와 시간</returns>
		public string ConvertToDateTime(string date, string time)
		{
            if (time.Equals("2400"))
            {
                date = Convert.ToDateTime(RequestDateTime(date)).AddDays(1).ToString("yyyy-MM-dd");
                time = "0000";
            }
            else
                date = RequestDateTime(date);
            
            return String.Format("{0} {1}", date, String.Concat(time.Substring(0, 2), ":", time.Substring(2, 2)));
		}

		/// <summary>
		/// Amadeus/Abacus용 날짜 또는 시간을 한국식으로 변경
		/// </summary>
		/// <param name="date">DDMMYY 또는 DDMMMYY 형식의 날짜 또는 HHMM 형식의 시간</param>
		/// <returns>YYYY-MM-DD 형식의 날짜 또는 HH:MM 형식의 시간</returns>
		public string ConvertToDateTime(string datetime)
		{
			if (datetime.Length.Equals(7) || datetime.Length.Equals(9))
			{
				string[] MonthName = { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
				string StrM = datetime.Substring(2, 3);
				string NumM = string.Empty;

				for (int Mn = 1; Mn <= MonthName.Length; Mn++)
				{
					if (MonthName[Mn].Equals(StrM))
					{
						NumM = Mn.ToString().PadLeft(2, '0');
						break;
					}
				}

				return String.Format("{0}-{1}-{2}", ((datetime.Length.Equals(7)) ? String.Concat("20", datetime.Substring(5, 2)) : datetime.Substring(5, 4)), NumM, datetime.Substring(0, 2));
			}
			else if (datetime.Length.Equals(4))
				return String.Concat(datetime.Substring(0, 2), ":", datetime.Substring(2, 2));
			else if (datetime.Length.Equals(5))
				return datetime.Replace(".", ":");
			else if (datetime.Length.Equals(12))
                return String.Format("{0}-{1}-{2} {3}:{4}", datetime.Substring(0, 4), datetime.Substring(4, 2), datetime.Substring(6, 2), datetime.Substring(8, 2), datetime.Substring(10, 2));
            else if (datetime.IndexOf("T") != -1)
				return datetime.Substring(0, 16).Replace("T", " ");
			else
				return RequestDateTime(datetime);
		}

		/// <summary>
		/// Amadeus/Abacus용 날짜를 한국식으로 변경
		/// </summary>
		/// <param name="date">DDMMM 형식의 날짜</param>
		/// <returns>YYYY-MM-DD 형식의 날짜</returns>
		public string ConvertToDate(string date)
		{
            DateTime NowDate = DateTime.Now;
            string[] MonthName = { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            string StrM = date.Substring(2, 3);
            string NumM = string.Empty;

            for (int Mn = 1; Mn <= MonthName.Length; Mn++)
            {
                if (MonthName[Mn].Equals(StrM))
                {
                    NumM = Mn.ToString().PadLeft(2, '0');
                    break;
                }
            }

            return String.Format("{0}-{1}-{2}", ((Convert.ToInt32(String.Concat(NumM, date.Substring(0, 2))) < Convert.ToInt32(NowDate.ToString("MMdd"))) ? NowDate.AddYears(1) : NowDate).ToString("yyyy"), NumM, date.Substring(0, 2));
		}

        /// <summary>
        /// Amadeus/Abacus용 날짜를 한국식으로 변경(TL체크시에 사용)
        /// </summary>
        /// <param name="date">DDMMM 형식의 날짜</param>
        /// <param name="basisDate">PNR생성일</param>
        /// <returns>YYYY-MM-DD 형식의 날짜</returns>
        public string ConvertToDate(string date, string basisDate)
        {
            DateTime NowDate = Convert.ToDateTime(basisDate);
            string[] MonthName = { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            string StrM = date.Substring(2, 3);
            string NumM = string.Empty;

            for (int Mn = 1; Mn <= MonthName.Length; Mn++)
            {
                if (MonthName[Mn].Equals(StrM))
                {
                    NumM = Mn.ToString().PadLeft(2, '0');
                    break;
                }
            }

            return String.Format("{0}-{1}-{2}", ((Convert.ToInt32(String.Concat(NumM, date.Substring(0, 2))) < Convert.ToInt32(NowDate.ToString("MMdd"))) ? NowDate.AddYears(1) : NowDate).ToString("yyyy"), NumM, date.Substring(0, 2));
        }

        /// <summary>
        /// Abacus용 날짜 형식을 연도를 포함한 한국식으로 변경
        /// </summary>
        /// <param name="ArrivalDateTime">DD-MMTHH:MM 형식의 일시</param>
        /// <param name="DepartureDateTime">DD-MMTHH:MM 형식의 일시</param>
        /// <returns>YYYY-MM-DD HH:MM 형식의 일시</returns>
        public string ConvertToAbacusDateTime(string ArrivalDateTime, string DepartureDateTime)
        {
            DateTime TmpADT = Convert.ToDateTime(String.Format("{0}-{1}", DepartureDateTime.Substring(0, 4), ArrivalDateTime.Replace("T", " ")));
            DateTime TmpDDT = Convert.ToDateTime(DepartureDateTime);

            return ((DateDiff("d", TmpADT.ToString("yyyy-MM-dd"), TmpDDT.ToString("yyyy-MM-dd")) > 0 && TmpADT.Month != TmpDDT.Month) ? TmpADT.AddYears(1) : TmpADT).ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// GMP 시간 정보 추가
        /// </summary>
        /// <param name="date">YYYY-MM-DD HH:MM 형식의 날짜</param>
        /// <param name="gmt">GMP 추가 시간</param>
        /// <returns></returns>
        public string ConvertToGMP(string date, int gmt)
        {
            return Convert.ToDateTime(date).AddHours(gmt).ToString("yyyy-MM-dd HH:mm");
        }

		/// <summary>
		/// Amadeus용 생년월일을 한국식으로 변경
		/// </summary>
		/// <param name="date">DDMMYY 또는 DDMMMYY 또는 DDMMYYYY 형식의 날짜</param>
		/// <returns>YYYY-MM-DD 형식의 날짜</returns>
		public string ConvertToBirthDate(string date)
		{
			if (date.Length.Equals(6))
			{
				int Year = RequestInt(date.Substring(4, 2));
				string etc = ConvertToDateTime(date);

				return String.Format("{0}{1}", (Year > 20) ? Year + 1900 : Year + 2000, etc.Substring(4));
			}
			else if (date.Length.Equals(7))
			{
				int Year = RequestInt(date.Substring(5, 2));
				string etc = ConvertToDate(date.Substring(0, 5));

				return String.Format("{0}{1}", (Year > 20) ? Year + 1900 : Year + 2000, etc.Substring(4));
			}
			else if (date.Length.Equals(8))
			{
				return String.Format("{0}-{1}-{2}", date.Substring(4, 4), date.Substring(2, 2), date.Substring(0, 2));
			}
			else
				return date;
		}

		/// <summary>
		/// Abacus용 날짜타입으로 변경
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public string ConvertToAbacusDateTime(string date)
		{
            if (String.IsNullOrWhiteSpace(date))
                return "";
            else
            {
                date = date.Trim();

                if (date.Length > 20)
                    return date;

                if (IsDateTime(date))
                    return Convert.ToDateTime(date).ToString("yyyy-MM-ddTHH:mm:ss.000Z");
                else
                    return date;
            }
		}

		/// <summary>
		/// 날짜형태를 '22DEC06'과 같은 형식으로 변환
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public string AbacusDateTime(string date)
		{
            if (String.IsNullOrWhiteSpace(date))
                return "";
            else
            {
                string[] MonthName = { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

                date = RequestDateTime(date);

                return (IsDateTime(date)) ? String.Format("{0}{1}{2}", date.Substring(8, 2), MonthName[RequestInt(date.Substring(5, 2))], date.Substring(2, 2)) : date;
            }
		}

		/// <summary>
		/// 날짜형태를 '22DEC 12:00'과 같은 형식으로 변환
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public string AbacusDateTime2(string date)
		{
            if (String.IsNullOrWhiteSpace(date))
                return "";
            else
            {
                string[] TmpDate = RequestDateTime(date, "yyyy-MM-dd HH:mm").Split(' ');

                return String.Format("{0} {1}", AbacusDateTime(TmpDate[0]).Substring(0, 5), TmpDate[1]);
            }
		}

        /// <summary>
        /// 야간 발권 가능 여부
        /// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <param name="AirCode">항공사코드</param>
        /// <returns></returns>
        public bool TLNightTicketing(int SNM, string AirCode)
        {
            //모두닷컴(2,3915), 홈플러스(항공)(4657), 스카이스캐너(4664,4837), 위메프(해외항공)(4681,4907), 카카오(국제선)(4716), 카약(국제선)(4713,4820), 신한카드(항공)(4715) 추가(2016-04-15,김지영과장)
            //NV모두투어(항공)(4638) 추가(2016-05-03,정성하과장)
            //스카이스캐너, 네이버의 경우 대한항공일 경우 제외(2016-07-04,정성하과장)
            //티몬(4925,4926), 더페이(항공)(5025) 추가(2018-02-12,김덕열과장)
            //스카이스캐너, 네이버의 경우 대한항공일 경우 제외 로직 삭제(2019-07-04,김경미매니저)
            bool Swing = false;

            switch (SNM)
            {
                case 2:
                case 3915:
                case 4657:
                case 4681:
                case 4907:
                case 4716:
                case 4713:
                case 4820:
                case 4715:
                case 4925:
                case 4926:
                case 5025:
                case 4664:
                case 4837:
                case 4638: Swing = true; break;
                //case 4638: Swing = (AirCode.Equals("KE") ? false : true); break;
                default: Swing = false; break;
            }

            return Swing;
        }

        /// <summary>
        /// 주말 발권 가능 여부
        /// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <returns></returns>
        public bool TLWeekTicketing(int SNM)
        {
            //모두닷컴(2,3915) 추가(2016-03-14,김지영과장)    
            //홈플러스(항공)(4657),스카이스캐너(4664,4837),삼성카드(항공)(4578),삼성카드(항공_복지몰)(4547),위메프(해외항공)(4681,4907),라이나생명(항공권)(4680),NV모두투어(항공)(4638),카카오(국제선)(4716),카약(국제선)(4713,4820),신한카드(항공)(4715),씨티카드(항공)(4759) 추가(2016-09-26,정성하과장)
            //11번가(4924,4929) 추가(2017-02-28,정성하과장)
            //티몬(4925,4926), 더페이(항공)(5025) 추가(2018-02-12,김덕열과장)
            //G마켓(5020,5119),옥션(5161,5163),G9(5162,5164)은 발권마감일 체크를 하지 않는다.(2018-07-20,김지영매니저)
            
            bool Swing = false;

            if (SNM.Equals(2) || SNM.Equals(3915) || SNM.Equals(4657) || SNM.Equals(4664) || SNM.Equals(4837) || SNM.Equals(4578) || SNM.Equals(4547) || SNM.Equals(4681) || SNM.Equals(4907) || SNM.Equals(4680) || SNM.Equals(4638) || SNM.Equals(4716) || SNM.Equals(4713) || SNM.Equals(4820) || SNM.Equals(4715) || SNM.Equals(4759) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(4925) || SNM.Equals(4926) || SNM.Equals(5025) || SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                Swing = true;
            
            return Swing;
        }

        /// <summary>
        /// TL 기본시간 설정(TL 날짜가 토/일/공휴일인 경우 16:00로, 업무일인 경우 17:00로 셋팅)
        /// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="TLDate">TL</param>
        /// <returns></returns>
        public string TLBasicTime(int SNM, string AirCode, string TLDate)
        {
            return WorkdayYN(TLDate) ? (TLNightTicketing(SNM, AirCode) ? " 20:00" : " 17:00") : " 16:00";
        }

		/// <summary>
		/// 통합TL([업무일 기준으로 +1일] 후로 셋팅)
		/// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <param name="AirCode">항공사코드</param>
		/// <returns></returns>
        public DateTime ModeTL(int SNM, string AirCode)
		{
            //string ExhTL = DateTime.Now.ToString("yyyy-MM-dd");
            
            //설연휴(2016.02.05 ~ 09까지 기본 TL을 익일 16:00시로 고정)(2016-02-02,정성하과장)
            //if (DateDiff("d", ExhTL, "2016-02-05") <= 0 && DateDiff("d", ExhTL, "2016-02-09") >= 0)
            //    return Convert.ToDateTime(String.Concat(DateTime.Now.AddDays(1).ToString("d"), " 16:00"));
            //else
            //{
                string TLDate = string.Empty;

                //주말 발권 여부
                if (TLWeekTicketing(SNM))
                    TLDate = DateTime.Now.AddDays(1).ToString("d");
                else
                    TLDate = TLWeekday(DateTime.Now.AddDays(1).ToString("d"));
                
                return Convert.ToDateTime(String.Concat(TLDate, TLBasicTime(SNM, AirCode, TLDate)));
            //}
		}

        /// <summary>
        /// 24시간 이내 발권인 경우 TL
        /// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <returns></returns>
        public string TL24(int SNM)
        {
            //24시간 이내 TL은 예약시간별로 TL 지정(2016-09-22,정성하과장)
            //10시에서 11시로 변경(2017-09-19,김지영차장)
            //24:00~06:00 당일 11:00
            //06:00~14:00 당일 17:00
            //14:00~24:00 익일 11:00
            
            DateTime NowDate = DateTime.Now;
            int NowHour = NowDate.Hour;

            if (NowHour < 6)
                return String.Format("{0} 11:00", NowDate.ToString("yyyy-MM-dd"));
            else if (NowHour >= 14)
                return String.Format("{0} 11:00", NowDate.AddDays(1).ToString("yyyy-MM-dd"));
            else
                return String.Format("{0} 17:00", NowDate.ToString("yyyy-MM-dd"));
        }

		/// <summary>
		/// TL날짜가 주말 또는 공휴일일 경우 그 이후 평일로 수정해 준다.
		/// </summary>
		/// <param name="TL"></param>
		/// <returns></returns>
		public string TLWeekday(string TL)
		{
			string ReBasicDate = TL;

			try
			{
                //using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //{
                //	using (SqlCommand cmd = new SqlCommand
                //	{
                //		Connection = conn,
                //		CommandTimeout = 60,
                //		CommandType = CommandType.StoredProcedure,
                //		CommandText = "DBO.WSV_S_WORKDAY"
                //	})
                //	{
                //		cmd.Parameters.Add("@기준일", SqlDbType.Char, 10);
                //		cmd.Parameters["@기준일"].Value = TL;

                //		try
                //		{
                //			conn.Open();
                //			ReBasicDate = cmd.ExecuteScalar().ToString();
                //		}
                //		catch (Exception ex)
                //		{
                //                        new MWSException(ex, HttpContext.Current, "Common", "TLWeekday", 0, 0);
                //		}
                //		finally
                //		{
                //			conn.Close();
                //		}
                //	}
                //}

                return TL;
			}
			catch (Exception ex)
			{
                new MWSException(ex, HttpContext.Current, "Common", "TLWeekday", 0, 0);
			}

			return ReBasicDate;
		}

		/// <summary>
		/// 입력된 날짜가 업무일인지 여부
		/// </summary>
		/// <param name="Date"></param>
		/// <returns></returns>
		public bool WorkdayYN(string Date)
		{
			bool YN = false;

			try
			{
                //if (!String.IsNullOrWhiteSpace(Date))
                //{
                //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //    {
                //        using (SqlCommand cmd = new SqlCommand
                //        {
                //            Connection = conn,
                //            CommandTimeout = 60,
                //            CommandType = CommandType.StoredProcedure,
                //            CommandText = "DBO.WSV_S_업무일여부"
                //        })
                //        {
                //            cmd.Parameters.Add("@기준일", SqlDbType.Char, 10);
                //            cmd.Parameters["@기준일"].Value = Date.Length.Equals(10) ? Date : RequestDateTime(Date, "yyyy-MM-dd");

                //            try
                //            {
                //                conn.Open();
                //                YN = cmd.ExecuteScalar().ToString().Equals("Y") ? true : false;
                //            }
                //            catch (Exception ex)
                //            {
                //                new MWSException(ex, HttpContext.Current, "Common", "WorkdayYN", 0, 0);
                //            }
                //            finally
                //            {
                //                conn.Close();
                //            }
                //        }
                //    }
                //}
                return true;
			}
			catch (Exception ex)
			{
                new MWSException(ex, HttpContext.Current, "Common", "WorkdayYN", 0, 0);
			}

			return YN;
		}

		/// <summary>
		/// 출력용TL
		/// </summary>
		/// <param name="SNM">웹사이트 번호</param>
		/// <param name="AirCode">항공사코드</param>
		/// <param name="TL">항공사 TL</param>
        /// <param name="LTD">발권마감일</param>
        /// <param name="PromTL">프모모션 TL</param>
		/// <returns></returns>
		public string WebTL(int SNM, string AirCode, string TL, string LTD, string PromTL)
		{
			//항공사TL이 모두투어TL보다 빠른 경우 항공사TL 출력
			//항공사TL을 알 수 없는 경우 모두투어TL 출력
			//LTD가 모두투어TL/항공사TL보다 빠는 경우 LTD 출력

            DateTime ModeTTL = ModeTL(SNM, AirCode);
            DateTime PromTTL;
			DateTime TTL;
			
			if (!String.IsNullOrWhiteSpace(TL))
			{
                TL = (TL.Length.Equals(10)) ? RequestDateTime(String.Concat(TL, TLBasicTime(SNM, AirCode, TL)), "yyyy-MM-dd HH:mm") : RequestDateTime(TL, "yyyy-MM-dd HH:mm");

				if (IsDateTime(TL))
				{
					TTL = Convert.ToDateTime(TL);

					if (DateDiff("d", TTL, ModeTTL) > 0)
						ModeTTL = TTL;
				}
			}

			//모두닷컴(항공),모두닷컴(모바일),삼성카드(항공)(4578),삼성카드(항공_복지몰)(4547)은 LTD를 계산식에 추가한다.(2014-11-25,김지영과장요청)
            //NV모두투어(항공)(4638)는 LTD를 계산식에 추가한다.(2015-06-26,정성하과장요청)
            //스카이스캐너(4664,4837)는 LTD를 계산식에 추가한다.(2015-09-02,정성하과장요청)
            //홈플러스(항공)(4657),위메프(해외항공)(4681,4907),라이나생명(항공권)(4680),카카오(국제선)(4716),카약(국제선)(4713,4820),신한카드(항공)(4715),씨티카드(항공)(4759)는 LTD를 계산식에 추가한다.(2016-09-28,정성하과장요청)
            //전체 사이트에 대해서 LTD를 계산식에 추가한다.(2016-09-29,김지영과장)
            //if (SNM.Equals(2) || SNM.Equals(3915) || SNM.Equals(4578) || SNM.Equals(4547) || SNM.Equals(4638) || SNM.Equals(4664) || SNM.Equals(4837) || SNM.Equals(4657) || SNM.Equals(4681) || SNM.Equals(4907) || SNM.Equals(4680) || SNM.Equals(4716) || SNM.Equals(4713) || SNM.Equals(4820) || SNM.Equals(4715) || SNM.Equals(4759))
            //{
				if (!String.IsNullOrWhiteSpace(LTD))
				{
                    if (LTD.Length.Equals(10))
                        LTD = RequestDateTime(String.Concat(LTD, TLBasicTime(SNM, AirCode, LTD)), "yyyy-MM-dd HH:mm");
                    else if (LTD.Length.Equals(8))
                        LTD = RequestDateTime(String.Concat(LTD, TLBasicTime(SNM, AirCode, LTD).Replace(":", "").Replace(" ", "")), "yyyy-MM-dd HH:mm");
                    else
                        LTD = RequestDateTime(LTD, "yyyy-MM-dd HH:mm");

					if (IsDateTime(LTD))
					{
						TTL = Convert.ToDateTime(LTD);

						if (DateDiff("d", TTL, ModeTTL) > 0)
							ModeTTL = TTL;
					}
				}
			//}

            //SU항공의 경우에는 24시간 이내 발권으로 최종 TL이 24시간 보다 클 경우 24시간 이내로 처리한다.(2015-08-10,김승미과장요청)
            if (AirCode.Equals("SU"))
            {
                if (DateDiff("h", ModeTTL, DateTime.Now.AddHours(24)) < 0)
                    ModeTTL = DateTime.Now.AddHours(24);
            }

            //박람회 기간(2015-10-30 ~ 2015-11-01)에는 모두닷컴(항공) 예약일 경우 평일 TL 처리.(2015-10-27,정성하과장요청)
            //주말 근무 기간(2015-11-20 ~ 2015-11-22)에는 모두닷컴(항공) 예약일 경우 평일 TL 처리.(2015-11-18,김지영과장요청)
            //if (SNM.Equals(2) || SNM.Equals(3915))
            //{
            //    string ExhTL = DateTime.Now.ToString("yyyy-MM-dd");

            //    if (DateDiff("d", ExhTL, "2015-11-20") <= 0 && DateDiff("d", ExhTL, "2015-11-22") >= 0)
            //    {
            //        TTL = Convert.ToDateTime(String.Format("{0} 17:00", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")));

            //        if (DateDiff("d", TTL, ModeTTL) > 0)
            //            ModeTTL = TTL;
            //    }
            //}

            //프로모션 TL이 최종 TL보다 빠를 경우 프로모션 TL로 처리(2015-11-18,김지영과장요청)
            if (!String.IsNullOrWhiteSpace(PromTL) && IsDateTime(PromTL))
            {
                PromTTL = Convert.ToDateTime(PromTL);

                if (DateDiff("h", ModeTTL, PromTTL) < 0)
                    ModeTTL = PromTTL;
            }
            
            return ModeTTL.ToString("yyyy-MM-dd HH:mm");
		}

        /// <summary>
        /// 예약TL
        /// </summary>
        /// <param name="SNM">웹사이트 번호</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="TL">항공사 TL</param>
        /// <param name="LTD">발권마감일</param>
        /// <param name="PromTL">프모모션 TL</param>
        /// <param name="DDT">출발일시</param>
        /// <param name="DayTicket">당일발권여부</param>
        /// <param name="FareType">운임종류</param>
        /// <returns></returns>
        public string BookingTL(int SNM, string AirCode, string TL, string LTD, string PromTL, string DDT, bool DayTicket, string FareType)
        {
            DateTime ModeTTL;
            DateTime DepartureTTL;
            DateTime PromTTL;
            DateTime TTL;
            //DateTime FareTypeTTL;

            DateTime NowDate = DateTime.Now;
            int NowHour = NowDate.Hour;

            //모두닷컴 기본 TL은 예약 후 30분(2018-02-19,김지영차장) - 보류
            //if (SNM.Equals(2) || SNM.Equals(3915))
            //{
            //    ModeTTL = NowDate.AddMinutes(30);
            //}
            //else
            //{
                //기본 TL(2017-02-08,김지영과장)
                if (TLWeekTicketing(SNM) && TLNightTicketing(SNM, AirCode))
                {
                    //20:30분을 20:00로 변경(2018-03-29,김경미차장)
                    //00:00~09:00 당일 20:00 (공유일인 경우 16:00)
                    //09:00~19:00 익일 15:00
                    //19:00~00:00 익일 20:00 (공유일인 경우 16:00)

                    if (NowHour < 9)
                    {
                        //9월 1일은 17시로 조정(2017-08-30,김지영차장)
                        //if (NowDate.ToString("yyyy-MM-dd").Equals("2017-09-01"))
                        //    ModeTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.ToString("yyyy-MM-dd"), "17:00"));
                        //else
                        ModeTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.ToString("yyyy-MM-dd"), (WorkdayYN(NowDate.ToString("yyyy-MM-dd")) ? "20:00" : "16:00")));
                    }
                    else if (NowHour >= 19)
                        ModeTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.AddDays(1).ToString("yyyy-MM-dd"), (WorkdayYN(NowDate.AddDays(1).ToString("yyyy-MM-dd")) ? "20:00" : "16:00")));
                    else
                        ModeTTL = Convert.ToDateTime(String.Format("{0} 15:00", NowDate.AddDays(1).ToString("yyyy-MM-dd")));
                }
                else
                    ModeTTL = ModeTL(SNM, AirCode);
            //}

            //임시 TL설정(2017/11/02 ~ 11/07)
            //스카이스캐너(4664,4837),카약(4713,4820),네이버(4638),11번가(4924,4929),위메프(4681,4907),티몬(4925,4926)은 00:00~09:00 까지 예약시 당일 16시로 설정(2017-11-01,김지영차장)
            //if (DateDiff("d", NowDate.ToString("yyyy-MM-dd"), "2017-11-02") <= 0 && DateDiff("d", NowDate.ToString("yyyy-MM-dd"), "2017-11-07") >= 0)
            //{
            //    if (NowHour < 9)
            //    {
            //        if (SNM.Equals(4664) || SNM.Equals(4837) || SNM.Equals(4713) || SNM.Equals(4820) || SNM.Equals(4638) || SNM.Equals(4924) || SNM.Equals(4929) || SNM.Equals(4681) || SNM.Equals(4907) || SNM.Equals(4925) || SNM.Equals(4926))
            //        {
            //            ModeTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.ToString("yyyy-MM-dd"), "16:00"));
            //        }
            //    }
            //}

            if (!String.IsNullOrWhiteSpace(DDT))
            {
                //출발일이 14일 이내인 경우 TL(2017-02-09,정성하과장)
                if (DateDiff("d", NowDate.ToString("yyyy-MM-dd"), DDT) < 14)
                {
                    //10시에서 11시로 변경(2017-09-19,김지영차장)
                    //00:00~06:00 당일 11:00
                    //06:00~14:00 당일 17:00
                    //14:00~00:00 익일 11:00

                    if (NowHour < 6)
                        DepartureTTL = Convert.ToDateTime(String.Format("{0} 11:00", NowDate.ToString("yyyy-MM-dd")));
                    else if (NowHour >= 14)
                        DepartureTTL = Convert.ToDateTime(String.Format("{0} 11:00", NowDate.AddDays(1).ToString("yyyy-MM-dd")));
                    else
                        DepartureTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.ToString("yyyy-MM-dd"), (WorkdayYN(NowDate.ToString("yyyy-MM-dd")) ? "17:00" : "16:00")));

                    if (DateDiff("h", ModeTTL, DepartureTTL) < 0)
                        ModeTTL = DepartureTTL;
                }

                //KE항공의 경우 출발 4일 이내 예약인 경우 예약완료 +30분으로 TL 설정(2019-10-21,김경미매니저)
                if (AirCode.Equals("KE"))
                {
                    if (DateDiff("d", NowDate.ToString("yyyy-MM-dd"), DDT) < 4)
                    {
                        DepartureTTL = NowDate.AddMinutes(30);

                        if (DateDiff("h", ModeTTL, DepartureTTL) < 0)
                            ModeTTL = DepartureTTL;
                    }
                }
            }

            //항공사 TL
            if (!String.IsNullOrWhiteSpace(TL))
            {
                TL = (TL.Length.Equals(10)) ? RequestDateTime(String.Concat(TL, TLBasicTime(SNM, AirCode, TL)), "yyyy-MM-dd HH:mm") : RequestDateTime(TL, "yyyy-MM-dd HH:mm");

                if (IsDateTime(TL))
                {
                    TTL = Convert.ToDateTime(TL);

                    if (DateDiff("h", ModeTTL, TTL) < 0)
                        ModeTTL = TTL;
                }
            }

            //발권 TL
            if (!String.IsNullOrWhiteSpace(LTD))
            {
                if (LTD.Length.Equals(10))
                    LTD = RequestDateTime(String.Concat(LTD, TLBasicTime(SNM, AirCode, LTD)), "yyyy-MM-dd HH:mm");
                else if (LTD.Length.Equals(8))
                    LTD = RequestDateTime(String.Concat(LTD, TLBasicTime(SNM, AirCode, LTD).Replace(":", "").Replace(" ", "")), "yyyy-MM-dd HH:mm");
                else
                    LTD = RequestDateTime(LTD, "yyyy-MM-dd HH:mm");

                if (IsDateTime(LTD))
                {
                    TTL = Convert.ToDateTime(LTD);

                    if (DateDiff("h", ModeTTL, TTL) < 0)
                        ModeTTL = TTL;
                }
            }

            //당일발권
            if (DayTicket)
            {
                //발권마감일 체크 사이트라면 당일 발권운임일 경우 TL을 당일 20:30으로 설정(2017-09-19,김지영차장)
                //20:30분을 20:00로 변경(2018-03-29,김경미차장)
                if (ApplyLTD(SNM))
                {
                    TTL = Convert.ToDateTime(String.Format("{0} 20:00", NowDate.ToString("yyyy-MM-dd")));

                    if (DateDiff("h", ModeTTL, TTL) < 0)
                        ModeTTL = TTL;
                }
            }

            //SU항공의 경우에는 24시간 이내 발권으로 최종 TL이 24시간 보다 클 경우 24시간 이내로 처리한다.(2015-08-10,김승미과장요청)
            if (AirCode.Equals("SU"))
            {
                if (DateDiff("h", ModeTTL, DateTime.Now.AddHours(24)) < 0)
                    ModeTTL = DateTime.Now.AddHours(24);
            }

            //프로모션 TL
            if (!String.IsNullOrWhiteSpace(PromTL))
            {
                PromTL = RequestDateTime(PromTL, "yyyy-MM-dd HH:mm");

                if (IsDateTime(PromTL))
                {
                    PromTTL = Convert.ToDateTime(PromTL);

                    if (DateDiff("h", ModeTTL, PromTTL) < 0)
                        ModeTTL = PromTTL;
                }
            }

            //갈릴레오 호스트운임
            //호스트운임 체크 제외(2018-12-04,김지영팀장)
            //호스트운임 체크(2019-04-08,김경미매니저)
            //호스트운임 체크제외(2019-04-22,김경미매니저)
            //if (FareType.Equals("H"))
            //{
            //    //00:00~12:00 당일 14:00
            //    //평일 12:00~ 당일 20:00
            //    //주말(휴일) 12:00~ 당일 16:00

            //    if (NowHour < 12)
            //        FareTypeTTL = Convert.ToDateTime(String.Format("{0} 14:00", NowDate.ToString("yyyy-MM-dd")));
            //    else
            //        FareTypeTTL = Convert.ToDateTime(String.Format("{0} {1}", NowDate.ToString("yyyy-MM-dd"), WorkdayYN(NowDate.ToString("yyyy-MM-dd")) ? "20:00" : "16:00"));

            //    if (DateDiff("h", ModeTTL, FareTypeTTL) < 0)
            //        ModeTTL = FareTypeTTL;
            //}

            //TL 마감시간 임시 조정(2018-03-23,김지영차장)
            //if (ModeTTL.ToString("yyyy-MM-dd").Equals("2018-03-29"))
            //{
            //    if (Convert.ToInt32(ModeTTL.ToString("HHmm")) > 1900)
            //        ModeTTL = Convert.ToDateTime(String.Format("{0} 19:00", ModeTTL.ToString("yyyy-MM-dd")));
            //}

            return ModeTTL.ToString("yyyy-MM-dd HH:mm");
        }

		/// <summary>
		/// Abacus TL
		/// </summary>
		/// <param name="ADTK">SSR Type이 ADTK인 SSL 항목의 Text</param>
		/// <returns></returns>
        public string AbacusTL(string ADTK)
		{
			string TL = string.Empty;

			try
			{
				if (!String.IsNullOrEmpty(ADTK))
				{
					string[] TTL = ADTK.Trim().Split(' ');
					string TLText = string.Empty;
					string Y = string.Empty;
					string M = string.Empty;
					string D = string.Empty;
					string[] MonthName = { "", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

					if (TTL.Length > 3)
					{
						TLText = TTL[3];

						Y = String.Format("20{0}", TLText.Substring(5, 2));
						M = TLText.Substring(2, 3);
						D = TLText.Substring(0, 2);

						for (int Mn = 1; Mn <= MonthName.Length; Mn++)
						{
							if (MonthName[Mn] == M)
							{
								M = Right("00" + Mn.ToString(), 2);
								break;
							}
						}

                        TL = String.Format("{0}-{1}-{2}", Y, M, D);
                        TL = String.Concat(TL, TLBasicTime(0, "", TL));
					}
					else
						TL = "";
				}
				else
					TL = "";
			}
			catch (Exception)
			{
				TL = "";
			}

			return TL;
		}

		/// <summary>
		/// Amadeus용 탑승객 이름에서 타입과 이름을 분리
		/// </summary>
		/// <param name="StrName"></param>
		/// <param name="Infant"></param>
		/// <returns>0:NamePrefix, 1:GivenName, 2:PassengerTypeCode</returns>
		public string[] SplitPaxType(string StrName, bool Infant)
		{
			string[] reValue = new String[3];

			if (Right(StrName, 2).Equals("MR") || Right(StrName, 2).Equals("MS"))
			{
				reValue[0] = Right(StrName, 2);
				reValue[1] = StrName.Substring(0, StrName.Length - 2).TrimEnd();
				reValue[2] = "ADT";
			}
			else
			{
				if (Right(StrName, 3).Equals("MRS"))
				{
					reValue[0] = Right(StrName, 3);
					reValue[1] = StrName.Substring(0, StrName.Length - 3).TrimEnd();
					reValue[2] = "ADT";
				}
				else
				{
					if (Right(StrName, 4).Equals("MSTR") || Right(StrName, 4).Equals("MISS"))
					{
						reValue[0] = Right(StrName, 4);
						reValue[1] = StrName.Substring(0, StrName.Length - 4).TrimEnd();
						reValue[2] = (Infant) ? "INF" : "CHD";
                    }
                    else if (Right(StrName, 4).Equals("PROF"))
                    {
                        reValue[0] = Right(StrName, 4);
                        reValue[1] = StrName.Substring(0, StrName.Length - 4).TrimEnd();
                        reValue[2] = "ADT";
                    }
					else
					{
						reValue[0] = "";
						reValue[1] = StrName.TrimEnd();
						reValue[2] = "";
					}
				}
			}

			return reValue;
		}

		/// <summary>
		/// Amadeus용 탑승객 타입정보를 통합용으로 치환
		/// </summary>
		/// <param name="PaxType">탑승객 타입 코드</param>
		/// <returns></returns>
		public static string ChangePaxType1(string PaxType)
		{
			return PaxType.Equals("CH") ? "CHD" : (PaxType.Equals("IN") ? "INF" : PaxType);
		}

		/// <summary>
		/// 통합용 탑승객 타입정보를 Amadeus용으로 치환
		/// </summary>
		/// <param name="PaxType">탑승객 타입 코드</param>
		/// <returns></returns>
		public static string ChangePaxType2(string PaxType)
		{
			return PaxType.Equals("CHD") ? "CH" : (PaxType.Equals("INF") ? "IN" : PaxType);
		}

		/// <summary>
		/// 통합용 탑승객 타입정보를 Amadeus용으로 치환
		/// </summary>
		/// <param name="PaxType">탑승객 타입 코드</param>
		/// <returns></returns>
		public static string ChangePaxType3(string PaxType)
		{
			return PaxType.Equals("CHD") ? "CH" : PaxType;
		}

        /// <summary>
        /// 탑승객 영문성의 경우 1자 이상 입력(만약 1자라면 강제로 2자로 변경)
        /// </summary>
        /// <param name="Surname"></param>
        /// <returns></returns>
        public static string ChangeSurname(string Surname)
        {
            return Surname.Length.Equals(1) ? String.Concat(Surname, Surname) : Surname;
        }

		/// <summary>
		/// 토파스용 숫자 PNR을 아마데우스용으로 변경
		/// </summary>
		/// <param name="PNR">토파스 PNR</param>
		/// <returns></returns>
		public string ChangeAmadeusPNR(string PNR)
		{
			string StrPNR = PNR.Replace("-", "");

			return (StrPNR.Length.Equals(7) && IsInteger(StrPNR)) ? String.Concat("0", PNR) : PNR;
		}

        /// <summary>
        /// 기착수
        /// </summary>
        /// <param name="SegGroup">여정</param>
        /// <returns></returns>
        public int StopoverCount(XmlNode SegGroup)
        {
            int STC = 0;
            
            foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
                STC += Convert.ToInt32(Seg.Attributes.GetNamedItem("stn").InnerText);

            return STC;
        }

		/// <summary>
		/// 소요시간(비행시간) 또는 대기시간
		/// </summary>
		/// <param name="ddt">출발시간</param>
		/// <param name="adt">도착시간</param>
		/// <returns></returns>
		public string ElapseFlyingTime(string ddt, string adt)
		{
            double EFT = DateDiff("m", ddt, adt);
			double Hour = Math.Truncate(EFT / 60);

			return String.Format("{0}:{1}", NumPosition(Hour.ToString(), 2), NumPosition((EFT % 60).ToString(), 2));
		}

		/// <summary>
		/// 총 소요시간
		/// </summary>
		/// <param name="eft1">비행시간1</param>
		/// <param name="eft2">비행시간2</param>
		/// <param name="gwt">지상대기시간</param>
		/// <returns></returns>
		public string SumElapseFlyingTime(string eft1, string eft2, string gwt)
		{
			string[] EFT1 = eft1.Split(':');
			string[] EFT2 = eft2.Split(':');
			
			int H = RequestInt(EFT1[0]) + RequestInt(EFT2[0]);
			int M = RequestInt(EFT1[1]) + RequestInt(EFT2[1]);

			if (!String.IsNullOrWhiteSpace(gwt))
			{
				string[] GWT = gwt.Split(':');

				H = H - RequestInt(GWT[0]);
				M = M - RequestInt(GWT[1]);
			}

			H = H + (M / 60);
			M = M % 60;

			return String.Format("{0}:{1}", NumPosition(H.ToString(), 2), NumPosition(M.ToString(), 2));
		}

        /// <summary>
        /// 대기시간(경유일 경우)
        /// </summary>
        /// <param name="SegGroup">여정그룹정보</param>
        /// <returns></returns>
        public string ElapseWaitingTime(XmlNode SegGroup)
        {
            XmlNodeList Segs = SegGroup.SelectNodes("seg");
            int len = Segs.Count;
            string ewt = string.Empty;

            if (len > 1)
            {
                for (int i = 1; i < len; i++)
                {
                    if (String.IsNullOrWhiteSpace(ewt))
                        ewt = ElapseFlyingTime(Segs[(i - 1)].Attributes.GetNamedItem("ardt").InnerText, Segs[i].Attributes.GetNamedItem("ddt").InnerText);
                    else
                        ewt = SumElapseFlyingTime(ewt, ElapseFlyingTime(Segs[(i - 1)].Attributes.GetNamedItem("ardt").InnerText, Segs[i].Attributes.GetNamedItem("ddt").InnerText), "");
                }
            }

            return ewt;
        }

        /// <summary>
        /// 대기시간
        /// </summary>
        /// <param name="adt">도착시간</param>
        /// <param name="ddt">출발시간</param>
        /// <returns></returns>
        public string CalWaitingTime(string adt, string ddt)
        {
            double EWT = DateDiff("m", adt, ddt);
            double Hour = Math.Truncate(EWT / 60);

            return String.Format("{0}{1}", NumPosition(Hour.ToString(), 2), NumPosition((EWT % 60).ToString(), 2));
        }

        /// <summary>
        /// 총대기시간(경유지+기착지)
        /// </summary>
        /// <param name="SegGroup"></param>
        /// <returns></returns>
        public string SumWaitingTime(XmlNode SegGroup)
        {
            int TotalMinute = 0;

            foreach (XmlNode Seg in SegGroup.SelectNodes("seg"))
            {
                TotalMinute += ChangeMinutes(Seg.Attributes.GetNamedItem("ett").InnerText) + ChangeMinutes(Seg.Attributes.GetNamedItem("gwt").InnerText);
            }

            return TotalMinute.Equals(0) ? "" : String.Format("{0}{1}", NumPosition((TotalMinute / 60).ToString(), 2), NumPosition((TotalMinute % 60).ToString(), 2));
        }

        /// <summary>
        /// 전체 소요시간(갈릴레오)
        /// </summary>
        /// <param name="AirSegDetail">갈릴레오 여정별 NodeList</param>
        /// <param name="NodeName">시간 계산용 필드명</param>
        /// <returns>HHMM 형식의 시간</returns>
        public string TotalTimeofGalileo(XmlNodeList AirSegDetail, string NodeName)
        {
            string TotalJrnyTm = string.Empty;
            int TotalJrnyMinute = 0;

            if (AirSegDetail.Count.Equals(1))
            {
                TotalJrnyTm = AirSegDetail[0].SelectSingleNode(NodeName).InnerText;

                return String.IsNullOrWhiteSpace(TotalJrnyTm) ? "" : String.Format("{0}{1}", TotalJrnyTm.Substring(0, 2), TotalJrnyTm.Substring(2, 2));
            }
            else
            {
                foreach (XmlNode TmpAirSegDetail in AirSegDetail)
                {
                    if (!String.IsNullOrWhiteSpace(TmpAirSegDetail.SelectSingleNode(NodeName).InnerText))
                    {
                        TotalJrnyTm = TmpAirSegDetail.SelectSingleNode(NodeName).InnerText;
                        TotalJrnyMinute += ((RequestInt(TotalJrnyTm.Substring(0, 2)) * 60) + RequestInt(TotalJrnyTm.Substring(2, 2)));
                    }
                }

                return String.Format("{0}{1}", NumPosition((TotalJrnyMinute / 60).ToString(), 2), NumPosition((TotalJrnyMinute % 60).ToString(), 2));
            }
        }
        
        /// <summary>
        /// 시간의 합
        /// </summary>
        /// <param name="Time1">시간1</param>
        /// <param name="Time2">시간2</param>
        /// <returns>HHMM 형식의 시간</returns>
        public string SumTime(string Time1, string Time2)
        {
            if (String.IsNullOrWhiteSpace(Time1) && String.IsNullOrWhiteSpace(Time2))
                return "";
            else
            {
                if (String.IsNullOrWhiteSpace(Time2))
                {
                    return Time1.Replace(":", "");
                }
                else
                {
                    int TotalMinute = ChangeMinutes(Time1) + ChangeMinutes(Time2);

                    return String.Format("{0}:{1}", NumPosition((TotalMinute / 60).ToString(), 2), NumPosition((TotalMinute % 60).ToString(), 2));
                }
            }
        }

        /// <summary>
        /// 시간형식을 분으로 변경
        /// </summary>
        /// <param name="Time">시간(hh:mm 또는 hhmm)</param>
        /// <returns>분</returns>
        public int ChangeMinutes(string Time)
        {
            return String.IsNullOrWhiteSpace(Time) ? 0 : ((Convert.ToInt32(Time.Substring(0, 2)) * 60) + Convert.ToInt32(Time.Replace(":", "").Substring(2, 2)));
        }

        /// <summary>
        /// 분을 시간형식으로 변경
        /// </summary>
        /// <param name="Minutes">분</param>
        /// <returns>시간(hhmm)</returns>
        public string ChangeTime(int Minutes)
        {
            return String.Format("{0}{1}", NumPosition((Minutes / 60).ToString(), 2), NumPosition((Minutes % 60).ToString(), 2));
        }

		/// <summary>
		/// 기간 제한사항을 일정한 양식으로 변경
		/// </summary>
		/// <param name="unit">구분</param>
		/// <param name="value">값</param>
		/// <returns></returns>
		public string ChangeTerm(string unit, string value)
		{
			return String.Format("0{0}{1}", (unit.Equals("0")) ? "D" : unit, NumPosition(value, 3));
		}

		/// <summary>
		/// 오픈 정보로 귀국일 셋팅
		/// </summary>
		/// <param name="DTD">출발일</param>
		/// <param name="OpenCode">오픈기간</param>
		/// <returns></returns>
		public string OpenDate(string DTD, string OpenCode)
		{
			if (IsDateTime(OpenCode))
				return OpenCode;
			else
			{
				if (String.IsNullOrWhiteSpace(OpenCode))
					OpenCode = "300D";

				int Term = RequestInt(Regex.Replace(OpenCode, @"\D", ""));
				DateTime SDate = Convert.ToDateTime(RequestDateTime(DTD));
				string EDate = string.Empty;

				switch (Right(OpenCode, 1))
				{
					case "Y": EDate = SDate.AddYears(Term).ToString(); break;
					case "M": EDate = SDate.AddMonths(Term).ToString(); break;
					case "D": EDate = SDate.AddDays(Term).ToString(); break;
					default: EDate = ""; break;
				}

				return EDate;
			}
		}

		/// <summary>
		/// 날짜를 입력받아 해당하는 요일명을 구한다
		/// </summary>
		/// <param name="date">Java용 Calendar 형태의 날짜 타입</param>
		/// <returns></returns>
		public string WeekName(string date)
		{
			date = RequestDateTime(date);

			return (IsDateTime(date)) ? Convert.ToDateTime(date).ToString("ddd") : "";
		}

		/// <summary>
		/// 날짜형식인지 판단
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public bool IsDateTime(string date)
		{
			DateTime reDate;

			return DateTime.TryParse(date, out reDate);
		}

		/// <summary>
		/// 두 날짜의 간격값 구하기
		/// </summary>
		/// <param name="k">구분</param>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns></returns>
		public double DateDiff(string k, string date1, string date2)
		{
            DateTime d1 = Convert.ToDateTime(date1);
            DateTime d2 = Convert.ToDateTime(date2);

			return DateDiff(k, d1, d2);
		}

		/// <summary>
		/// 두 날짜의 간격값 구하기
		/// </summary>
		/// <param name="k">구분</param>
		/// <param name="d1"></param>
		/// <param name="d2"></param>
		/// <returns></returns>
		public double DateDiff(string k, DateTime d1, DateTime d2)
		{
			TimeSpan d3 = d2 - d1;
			double reValue = 0;

			switch (k)
			{
				case "y":
					reValue = (d3.TotalDays / (30 * 12));
					break;
				case "M":
					reValue = (d3.TotalDays / 30);
					break;
				case "d":
					reValue = d3.TotalDays;
					break;
				case "h":
					reValue = d3.TotalHours;
					break;
				case "m":
					reValue = d3.TotalMinutes;
					break;
				case "s":
					reValue = d3.TotalSeconds;
                    break;
                case "f":
                    reValue = d3.TotalMilliseconds;
                    break;
			}

			return reValue;
		}

		/// <summary>
		/// Int 배열을 구분자를 이용하여 문자열로 변환
		/// </summary>
		/// <param name="reqText">Int 배열</param>
		/// <param name="separator">구분자</param>
		/// <returns></returns>
		public string ConvertStringToArrInteger(int[] reqText, string separator)
		{
			string Spr = RequestString(separator, "|");
			string reValue = reqText[0].ToString();

			for (int i = 1; i < reqText.Length; i++)
			{
				reValue = String.Concat(reValue, Spr, reqText[i].ToString());
			}

			return reValue;
		}

		/// <summary>
		/// String 배열을 구분자를 이용하여 문자열로 변환
		/// </summary>
		/// <param name="reqText">String 배열</param>
		/// <param name="separator">구분자</param>
		/// <returns></returns>
		public string ConvertStringToArrString(string[] reqText, string separator)
		{
			string Spr = RequestString(separator, "|");
			string reValue = reqText[0];

			for (int i = 1; i < reqText.Length; i++)
			{
				reValue = String.Concat(reValue, Spr, reqText[i].Trim());
			}

			return reValue;
		}

		/// <summary>
		/// Request값의 유효성검사(String 용)
		/// </summary>
		/// <param name="reqText"></param>
		/// <returns></returns>
		public string RequestString(string reqText)
		{
			return RequestString(reqText, "");
		}

		/// <summary>
		/// Request값의 유효성검사(String 용)
		/// </summary>
		/// <param name="reqText"></param>
		/// <param name="defaultText"></param>
		/// <returns></returns>
		public string RequestString(string reqText, string defaultText)
		{
			if (String.IsNullOrWhiteSpace(reqText))
				reqText = defaultText;

			return reqText;
		}

		/// <summary>
		/// Request값의 유효성검사(Xml 용)
		/// </summary>
		/// <param name="reqText"></param>
		/// <returns></returns>
		public string RequestXml(string reqText)
		{
			if (!String.IsNullOrWhiteSpace(reqText))
			{
				reqText = reqText.Replace("[", "(");
				reqText = reqText.Replace("]", ")");
				reqText = reqText.Replace("&", "&amp;");
			}

			return reqText;
		}

		/// <summary>
		/// Request값의 유효성검사(Int 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <returns></returns>
		public int RequestInt(string reqNum)
		{
			return RequestInt(reqNum, 0);
		}

		/// <summary>
		/// Request값의 유효성검사(Int 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <param name="defaultNum"></param>
		/// <returns></returns>
		public int RequestInt(string reqNum, int defaultNum)
		{
			int reValue = defaultNum;

			if (!String.IsNullOrWhiteSpace(reqNum))
			{
				reqNum = reqNum.Replace(",", "");

				if (!int.TryParse(reqNum, out reValue))
					reValue = defaultNum;
			}

			return reValue;
		}

		/// <summary>
		/// Request값의 유효성검사(Int16 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <returns></returns>
		public Int16 RequestInt16(string reqNum)
		{
			return RequestInt16(reqNum, 0);
		}

		/// <summary>
		/// Request값의 유효성검사(Int16 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <param name="defaultNum"></param>
		/// <returns></returns>
		public Int16 RequestInt16(string reqNum, Int16 defaultNum)
		{
			Int16 reValue = defaultNum;

			if (!String.IsNullOrWhiteSpace(reqNum))
			{
				reqNum = reqNum.Replace(",", "");

				if (!Int16.TryParse(reqNum, out reValue))
					reValue = defaultNum;
			}

			return reValue;
		}

		/// <summary>
		/// Request값의 유효성검사(double 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <returns></returns>
		public double RequestDouble(string reqNum)
		{
			return RequestDouble(reqNum, 0);
		}

		/// <summary>
		/// Request값의 유효성검사(double 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <param name="defaultNum"></param>
		/// <returns></returns>
		public double RequestDouble(string reqNum, double defaultNum)
		{
			double reValue = defaultNum;

			if (!String.IsNullOrWhiteSpace(reqNum))
			{
				reqNum = reqNum.Replace(",", "");

				if (!double.TryParse(reqNum, out reValue))
					reValue = defaultNum;
			}

			return reValue;
		}

		/// <summary>
		/// Request값의 유효성검사(decimal 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <returns></returns>
		public decimal RequestDecimal(string reqNum)
		{
			return RequestDecimal(reqNum, 0);
		}

		/// <summary>
		/// Request값의 유효성검사(decimal 용)
		/// </summary>
		/// <param name="reqNum"></param>
		/// <param name="defaultNum"></param>
		/// <returns></returns>
		public decimal RequestDecimal(string reqNum, decimal defaultNum)
		{
			decimal reValue = defaultNum;

			if (!String.IsNullOrWhiteSpace(reqNum))
			{
				reqNum = reqNum.Replace(",", "");

				if (!decimal.TryParse(reqNum, out reValue))
					reValue = defaultNum;
			}

			return reValue;
		}

		/// <summary>
		/// Request값의 유효성검사(DateTime 용)
		/// </summary>
		/// <param name="reqDate"></param>
		/// <returns></returns>
		public string RequestDateTime(string reqDate)
		{
			return RequestDateTime(reqDate, "d");
		}

		/// <summary>
		/// Request값의 유효성검사(DateTime 용)
		/// </summary>
		/// <param name="reqDate"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public string RequestDateTime(string reqDate, string format)
		{
			string reValue = "";
			DateTime reDate;

			if (!String.IsNullOrWhiteSpace(reqDate))
			{
				if (reqDate.IndexOf("-") == -1)
				{
					if (reqDate.Length.Equals(8)) //YYYYMMDD
						reqDate = String.Format("{0}-{1}-{2}", reqDate.Substring(0, 4), reqDate.Substring(4, 2), reqDate.Substring(6, 2));
					else if (reqDate.Length.Equals(12)) //YYYYMMDDHHMM
						reqDate = String.Format("{0}-{1}-{2} {3}:{4}", reqDate.Substring(0, 4), reqDate.Substring(4, 2), reqDate.Substring(6, 2), reqDate.Substring(8, 2), reqDate.Substring(10, 2));
					else if (reqDate.Length.Equals(6)) //DDMMYY
						reqDate = String.Format("20{0}-{1}-{2}", reqDate.Substring(4, 2), reqDate.Substring(2, 2), reqDate.Substring(0, 2));
				}
				else if (reqDate.IndexOf("T") != -1 && reqDate.IndexOf("Z") != -1)
				{
					reqDate = String.Format("{0} {1}", reqDate.Substring(0, 10), reqDate.Substring(11, 5));
				}
                else if (reqDate.IndexOf(".") != -1)
                {
                    reqDate = reqDate.Replace(".", "-");
                }

				if (DateTime.TryParse(reqDate, out reDate))
					reValue = reDate.ToString(format);
			}

			return reValue;
		}

		/// <summary>
		/// 입력받은 값이 숫자(정수)인지 판단(0은 제외)
		/// </summary>
		/// <param name="strNum"></param>
		/// <returns></returns>
		public bool IsInteger(string strNum)
		{
			//return (RequestDouble(strNum).Equals(0) || (Regex.IsMatch(strNum, "^\\d+$")));
            return (Regex.IsMatch(strNum, "^\\d+$"));
		}

        /// <summary>
        /// 입력받은 값이 숫자(실수)인지 판단
        /// </summary>
        /// <param name="strNum"></param>
        /// <returns></returns>
        public bool IsNumeric(string strNum)
        {
            return (Regex.IsMatch(strNum, "^[0-9.,]+$"));
        }

		/// <summary>
		/// 스트링에서 숫자(정수)만 추려낸다
		/// </summary>
		/// <param name="strNum">숫자(정수)와 문자가 혼합된 String</param>
		/// <returns>숫자</returns>
		public int ExtractNumber(string strNum)
		{
			return RequestInt(Regex.Replace(strNum.Trim(), @"\D", ""));
		}

        /// <summary>
        /// 스트링에서 숫자(실수)만 추려낸다
        /// </summary>
        /// <param name="strNum">숫자(정수)와 문자가 혼합된 String</param>
        /// <returns>숫자</returns>
        public double ExtractNumeric(string strNum)
        {
            return RequestDouble(Regex.Replace(strNum.Trim(), @"[^0-9.,]", ""));
        }

		/// <summary>
		/// 스트링에서 알파벳만 제거한다(항공편만 구할경우 사용)
		/// </summary>
		/// <param name="strFlight">숫자와 문자가 혼합된 String</param>
		/// <returns></returns>
		public string ExtractAlphabet(string strFlight)
		{
			return (IsInteger(strFlight.Substring(0, 2))) ? Regex.Replace(strFlight.Trim(), @"[a-zA-Z]", "") : Regex.Replace(strFlight.Substring(2).Trim(), @"[a-zA-Z]", "");
		}

		/// <summary>
		/// 규정내용이 있는지 여부(영문 및 숫자가 포함되어 있는지 판단)
		/// </summary>
		/// <param name="strRule"></param>
		/// <returns></returns>
		public bool IsRuleText(string strRule)
		{
			return (Regex.IsMatch(strRule, "[a-zA-Z0-9]+"));
		}

		/// <summary>
		/// HTML 태그 제거
		/// </summary>
		/// <param name="strText">HTML 태그가 존재하는 String</param>
		/// <returns></returns>
		public string HtmlStrip(string strText)
		{
			return Regex.Replace(strText, @"<(.|\n)*?>", string.Empty);
		}

		/// <summary>
		/// 시작하는 문자열 삭제
		/// </summary>
		/// <param name="strText">원본 문자열</param>
		/// <param name="swText">삭제하려는 시작 문자열</param>
		/// <returns></returns>
		public static string StartWithStrip(string strText, string swText)
		{
			return Regex.Replace(strText, String.Format(@"^{0}", swText), string.Empty);
		}

        /// <summary>
        /// 조건에 처음 해당하는 것만 치환
        /// </summary>
        /// <param name="Expression">원본 문자열</param>
        /// <param name="Find">찾을 문자열</param>
        /// <param name="ReplaceWith">바꿀 문자열</param>
        /// <returns></returns>
        public static string ReplaceFirst(string Expression, string Find, string ReplaceWith)
        {
            return string.Concat(Expression.Substring(0, Expression.IndexOf(Find)), ReplaceWith, Expression.Substring(Expression.IndexOf(Find) + Find.Length, Expression.Length - (Expression.IndexOf(Find) + Find.Length)));
        }

		/// <summary>
		/// 입력받은 문자를 입력받은 길이만큼 오른쪽에서 추출
		/// </summary>
		/// <param name="StrData"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public string Right(string StrData, int length)
		{
			string reValue = string.Empty;

			if (StrData.Length - length >= 0)
				reValue = StrData.Substring(StrData.Length - length);
			else
				reValue = StrData;

			return reValue;
		}

        /// <summary>
        /// 텍스트 뒤집기
        /// </summary>
        /// <param name="StrData"></param>
        /// <returns></returns>
        public string Reverse(string StrData)
        {
            Stack<char> tmpStack = new Stack<char>(StrData.Length);

            foreach (char c in StrData)
            {
                tmpStack.Push(c);
            }

            StringBuilder sb = new StringBuilder(StrData.Length);

            while (tmpStack.Count > 0)
            {
                sb.Append(tmpStack.Pop());
            }

            return sb.ToString();
        }

        /// <summary>
        /// String 배열을 String으로 변환
        /// </summary>
        /// <param name="StrData">배열</param>
        /// <returns></returns>
        public static string ConvertingArraryToString(string[] StrData, string Separator)
        {
            string ReString = string.Empty;

            for (int i = 0; i < StrData.Length; i++)
            {
                if (i > 0)
                    ReString += Separator.Trim();

                ReString += StrData[i].Trim();
            }

            return ReString;
        }

        /// <summary>
        /// 일반 문자열을 Base64 문자열로 변환
        /// </summary>
        /// <param name="StrData">일반 문자열</param>
        /// <returns>Base64 문자열</returns>
        public static string Base64Encode(string StrData)
        {
            UTF8Encoding UTF8Enc = new UTF8Encoding();
            byte[] byteStr = UTF8Enc.GetBytes(StrData);

            return Convert.ToBase64String(byteStr);
        }

        /// <summary>
        /// Base64 문자열을 일반 문자열로 변환
        /// </summary>
        /// <param name="StrData">Base64 문자열</param>
        /// <returns>일반 문자열</returns>
        public static string Base64Decode(string StrData)
        {
            UTF8Encoding UTF8Enc = new UTF8Encoding();
            byte[] byteStr = Convert.FromBase64String(StrData);

            return UTF8Enc.GetString(byteStr);
        }

        /// <summary>
        /// SHA1 해시
        /// </summary>
        /// <param name="StrData">문자열</param>
        /// <returns>SHA1 해시문자열</returns>
        public static string SHA1Hash(string StrData)
        {
            SHA1 sha = SHA1.Create();
            byte[] byteStr = sha.ComputeHash(Encoding.UTF8.GetBytes(StrData));
            string hashStr = string.Empty;

            foreach (byte hash in byteStr)
                hashStr += hash.ToString("X2");

            return hashStr;
        }

        /// <summary>
        /// 일반 문자열을 ASCII로 변환
        /// </summary>
        /// <param name="StrData">일반 문자열</param>
        /// <returns>Base64 문자열</returns>
        public static byte[] ConvertToASCII(string StrData)
        {
            return Encoding.ASCII.GetBytes(StrData);
        }

        /// <summary>
        /// SAH1 해시 문자열을 Byte로 변환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ConvertHexaToBytes(string StrData)
        {
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 255; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < StrData.Length; i += 2)
                hexres.Add(hexindex[StrData.Substring(i, 2)]);

            return hexres.ToArray();
        }

		/// <summary>
		/// 숫자의 자릿수 맞추기
		/// </summary>
		/// <param name="Number"></param>
		/// <param name="Seq"></param>
		/// <returns></returns>
		public string NumPosition(string Number, int Seq)
		{
            return Number.PadLeft(Seq, '0');
		}

        /// <summary>
        /// 숫자의 자릿수 맞추기
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="Seq"></param>
        /// <returns></returns>
        public static string NumPositions(string Number, int Seq)
        {
            return Number.PadLeft(Seq, '0');
        }

		/// <summary>
		/// 절삭
		/// </summary>
		/// <param name="Price">기본금액</param>
		/// <param name="CNum">절삭기준</param>
		/// <returns></returns>
		public static double IntCutting(double Price, double CNum)
		{
			return Price.Equals(0) ? 0 : (Math.Truncate(Price / CNum) * CNum);
		}

		/// <summary>
		/// 절상
		/// </summary>
		/// <param name="Price">기본금액</param>
		/// <param name="CNum">절상기준</param>
		/// <returns></returns>
		public static double IntIncrement(double Price, double CNum)
		{
            if (Price.Equals(0))
                return 0;
            else
            {
                double RNum = Math.Truncate(Price / CNum) * CNum;
                return (RNum > Price) ? RNum : (RNum + CNum);
            }
		}

		/// <summary>
		/// 반올림
		/// </summary>
		/// <param name="Price">기본금액</param>
		/// <param name="CNum">반올림기준</param>
		/// <returns></returns>
		public static double IntRound(double Price, double CNum)
		{
			return IntCutting((Price + (CNum * 5)), CNum * 10);
		}

		/// <summary>
		/// Amadeus 항공운임 계산
		/// </summary>
		/// <param name="AirCode">항공사 코드</param>
		/// <param name="FareAmount">총 항공권 금액</param>
		/// <param name="TaxAmount">총 텍스 금액(유류할증료 포함)</param>
		/// <param name="QCharge">Q Charge</param>
		/// <returns></returns>
		public double GetFare(string AirCode, string FareAmount, string TaxAmount, string QCharge)
		{
			return GetFare(AirCode, RequestDouble(FareAmount), RequestDouble(TaxAmount), RequestDouble(QCharge));
		}

		/// <summary>
		/// Amadeus 항공운임 계산
		/// </summary>
		/// <param name="AirCode">항공사 코드</param>
		/// <param name="FareAmount">총 항공권 금액</param>
		/// <param name="TaxAmount">총 텍스 금액(유류할증료 포함)</param>
		/// <param name="QCharge">Q Charge</param>
		/// <returns></returns>
		public static double GetFare(string AirCode, double FareAmount, double TaxAmount, double QCharge)
		{
			//HA,TW 항공사의 경우 Q를 유류할증료에 포함시킨다.(운임에서 제외)
			return (AirCode.Equals("HA") || AirCode.Equals("TW")) ? FareAmount - TaxAmount - QCharge : FareAmount - TaxAmount;
		}

		/// <summary>
		/// Amadeus 항공운임 계산
		/// </summary>
		/// <param name="AirCode">항공사 코드</param>
		/// <param name="Fare">항공운임</param>
		/// <param name="QCharge">Q Charge</param>
		/// <returns></returns>
		public static double GetFare(string AirCode, double Fare, double QCharge)
		{
			//HA,TW 항공사를 제외하고 Q를 항공운임에 포함시킨다.
			return (AirCode.Equals("HA") || AirCode.Equals("TW")) ? Fare : Fare + QCharge;
		}

		/// <summary>
		/// Amadeus 유류할증료 계산
		/// </summary>
		/// <param name="AirCode">항공사 코드</param>
		/// <param name="FuelSurCharge">유류할증료</param>
		/// <param name="QCharge">Q Charge</param>
		/// <returns></returns>
		public double GetFuelSurCharge(string AirCode, string FuelSurCharge, string QCharge)
		{
			return GetFuelSurCharge(AirCode, RequestDouble(FuelSurCharge), RequestDouble(QCharge));
		}

		/// <summary>
		/// Amadeus 유류할증료 계산
		/// </summary>
		/// <param name="AirCode">항공사 코드</param>
		/// <param name="FuelSurCharge">유류할증료</param>
		/// <param name="QCharge">Q Charge</param>
		/// <returns></returns>
		public static double GetFuelSurCharge(string AirCode, double FuelSurCharge, double QCharge)
		{
			//HA,TW 항공사의 경우 Q를 유류할증료에 포함시킨다.
			return (AirCode.Equals("HA") || AirCode.Equals("TW")) ? FuelSurCharge + QCharge : FuelSurCharge;
		}

		/// <summary>
		/// Amadeus 텍스 계산
		/// </summary>
		/// <param name="TaxAmount">총 텍스 금액(유류할증료 포함)</param>
		/// <param name="FuelSurcharge">유류할증료</param>
		/// <returns></returns>
		public double GetTax(string TaxAmount, string FuelSurcharge)
		{
			return GetTax(RequestDouble(TaxAmount), RequestDouble(FuelSurcharge));
		}

		/// <summary>
		/// Amadeus 텍스 계산
		/// </summary>
		/// <param name="TaxAmount">총 텍스 금액(유류할증료 포함)</param>
		/// <param name="FuelSurcharge">유류할증료</param>
		/// <returns></returns>
		public static double GetTax(double TaxAmount, double FuelSurcharge)
		{
			return TaxAmount - FuelSurcharge;
		}

		/// <summary>
		/// Amadeus 티켓번호 구하기
		/// </summary>
		/// <param name="TicketInfo">티켓정보</param>
		/// <returns></returns>
		public static string SplitAmadeusTicketNumber(string TicketInfo)
		{
            string[] TmpTicketInfo = null;
            
            if (TicketInfo.IndexOf("/") != -1)
            {
                string[] TicketInfos = TicketInfo.Split('/');

                if (TicketInfos.Length >= 2 && TicketInfos[1].Substring(0, 2).Equals("ET"))
                {
                    TmpTicketInfo = TicketInfos[0].Split(' ');
                }
                else if (TicketInfos.Length.Equals(2))
                {
                    TmpTicketInfo = TicketInfos[0].Split(' ');
                }
            }
            else
            {
                TmpTicketInfo = TicketInfo.Split(' ');
            }

            if (TmpTicketInfo != null && TmpTicketInfo.Length > 0)
            {
                string TmpTicketNum = TmpTicketInfo[TmpTicketInfo.Length - 1];
                string[] TmpTicketNums = TmpTicketNum.Split('-');

                return (TmpTicketNums.Length > 2) ? TmpTicketNum.Replace(String.Concat(TmpTicketNums[0], "-"), TmpTicketNums[0]) : TmpTicketNum.Replace("-", "");
            }
            else
                return "";
		}

        /// <summary>
        /// Abacus 티켓번호 구하기
        /// </summary>
        /// <param name="TicketInfo">티켓정보</param>
        /// <returns></returns>
        public static string SplitAbacusTicketNumber(string TicketInfo)
        {
            string TmpTicketNumber = TicketInfo.Replace(".", "/").Split('/')[1];
            return TmpTicketNumber.StartsWith("INF") ? TmpTicketNumber.Substring(0, 16) : TmpTicketNumber.Substring(0, 13);
        }

        /// <summary>
        /// Abacus VOID 티켓여부
        /// </summary>
        /// <param name="TicketList">티켓정보 리스트</param>
        /// <param name="TicketNumber">티켓번호</param>
        /// <returns></returns>
        public static bool AbacusVoidTicket(XmlNodeList TicketList, string TicketNumber)
        {
            bool VoidValue = false;

            foreach (XmlNode Ticketing in TicketList)
            {
                string TicketInfo = Ticketing.Attributes.GetNamedItem("eTicketNumber").InnerText;
                
                if (TicketInfo.StartsWith("TV"))
                {
                    Match match = new Regex(@"[A-Z]{2}\s(?<TN>[0-9\/]{13,16})-.+", RegexOptions.Singleline).Match(TicketInfo);
                    
                    if (match.Success)
                    {
                        string[] TmpTicketNum = match.Groups["TN"].Value.Split('/');

                        if (TmpTicketNum[0].Equals(TicketNumber))
                        {
                            VoidValue = true;
                            break;
                        }
                        else if (TmpTicketNum.Length > 1)
                        {
                            if (String.Concat(TmpTicketNum[0].Substring(0, 11), TmpTicketNum[1]).Equals(TicketNumber))
                            {
                                VoidValue = true;
                                break;
                            }
                        }
                    }
                }
            }

            return VoidValue;
        }

		/// <summary>
		/// Flight 정보 기본 3자리로 변경
		/// </summary>
		/// <param name="Flight">flight 번호</param>
		/// <returns></returns>
		public static string ZeroPaddingFlight(string Flight)
		{
			Flight = Flight.Trim();
			return (Flight.Length < 3) ? Flight.PadLeft(3, '0') : Flight;
		}

		/// <summary>
		/// 캐빈클래스별 참조번호 재정의
		/// </summary>
        /// <param name="GDS">GDS코드</param>
		/// <param name="CabinClass">캐빈클래스</param>
		/// <returns></returns>
		public static int RefOverride(string GDS, string CabinClass)
		{
            if (GDS.Equals("Sabre"))
                return (CabinClass.Equals("Y") ? 6000 : (CabinClass.Equals("P") ? 7000 : (CabinClass.Equals("S") ? 7700 : (CabinClass.Equals("C") ? 8000 : CabinClass.Equals("J") ? 8700 : (CabinClass.Equals("F") ? 9000 : 0)))));
            else if (GDS.Equals("MPIS"))
                return (CabinClass.Equals("Y") ? 1700 : (CabinClass.Equals("M") ? 2700 : (CabinClass.Equals("W") ? 3700 : (CabinClass.Equals("C") ? 4700 : (CabinClass.Equals("F") ? 5700 : 0)))));
            else
                return (CabinClass.Equals("Y") ? 1000 : (CabinClass.Equals("M") ? 2000 : (CabinClass.Equals("W") ? 3000 : (CabinClass.Equals("C") ? 4000 : (CabinClass.Equals("F") ? 5000 : 0)))));
		}

		/// <summary>
		/// 참조번호 재정의
		/// </summary>
		/// <param name="RefNum">참조번호</param>
		/// <param name="CCDRef">참조번호 클래스기본값</param>
		/// <returns></returns>
		public static string RefSum(string RefNum, int CCDRef)
		{
			return RefSum(Convert.ToInt32(RefNum), CCDRef).ToString();
		}

		/// <summary>
		/// 참조번호 재정의
		/// </summary>
		/// <param name="RefNum">참조번호</param>
		/// <param name="CCDRef">참조번호 클래스기본값</param>
		/// <returns></returns>
		public static int RefSum(int RefNum, int CCDRef)
		{
			return (RefNum + CCDRef);
		}

        /// <summary>
        /// 참조번호 재정의(AirService3용)
        /// </summary>
        /// <param name="GDS">GDS</param>
        /// <param name="SAC">항공사</param>
        /// <param name="PTC">운임타입</param>
        /// <param name="CCD">캐빈클래스</param>
        /// <returns></returns>
        public static string RefOverrideBasic(string GDS, string SAC, string PTC, string CCD)
        {
            return String.Concat(GDS, ((String.IsNullOrWhiteSpace(SAC) || SAC.IndexOf(',') != -1) ? "00" : SAC), PTC, CCD);
        }

        /// <summary>
        /// 참조번호 재정의(AirService3용)
        /// </summary>
        /// <param name="BasicRef">재정의된 기본 참조번호</param>
        /// <param name="FlightSegRef">여정번호</param>
        /// <param name="SegRef">일련번호</param>
        /// <returns></returns>
        public static string RefOverrideFull(object BasicRef, object FlightSegRef, object SegRef)
        {
            return String.Concat(BasicRef, FlightSegRef, NumPositions(SegRef.ToString(), 3));
        }

		/// <summary>
		/// TASF 적용 가능 여부
		/// </summary>
		/// <param name="SNM">사이트번호</param>
        /// <param name="AirCode">항공사코드</param>
		/// <returns></returns>
        public static bool ApplyTASF(int SNM, string AirCode)
        {
            //닷컴(2/3915)/ON-BP(3738/2830/4020/4607)/제휴(4681/4907/4713/4820/4578/4547/4715) 전체 항공사 적용(2016-12-30,김지영과장)
            //네이버(4638)/스카이스캐너(4664,4837)(2017-01-17,김지영과장)
            //네이버(4638) 제외(2017-02-23,김지영과장)
            //삼성카드(항공) 모바일(4737) 추가(2017-07-21,김경미차장)
            //신한카드(항공) 모바일(5045) 추가(2017-07-25,박주영차장)
            //11번가(4929/4924), 티몬(4925/4926) 추가(2017-08-30,김지영차장)
            //11번가(4929/4924) 제외(2018-09-17,김경미매니저)
            //더페이(항공)(5025) 추가(2017-09-20,김덕열과장)
            //삼성카드(항공_TW몰)(5236) 추가(2018-09-17,김경미매니저)
            //11번가(4929/4924) 추가(2018-10-17,김경미매니저)
            //11번가(4929/4924) 제외(2018-11-01,김경미매니저)
            //11번가(4929/4924) 추가(2018-11-14,김경미매니저)
            //티몬(4925/4926) 제외(2018-11-16,김경미매니저)
            //티몬(4925/4926) 추가(2018-11-26,김경미매니저)
            //11번가(4929/4924) 제외(2018-11-30,김경미매니저)
            //11번가(4929/4924) 추가(2019-01-02,김경미매니저)
            string Site = "/2/3915/3738/2830/4020/4607/4681/4907/4713/4820/4578/4547/4715/4664/4837/4737/5045/5025/5236/4925/4926/4929/4924/";

            //네이버(4638)는 LCC(LJ,TW,ZE,VJ)만 적용(2017-02-28,김지영과장)
            //LJ는 갈릴레오 운임 사용으로 제외(201810-22,김경미매니저)
            if (SNM.Equals(4638) && (AirCode.Equals("TW") || AirCode.Equals("ZE") || AirCode.Equals("VJ")))
                Site += "4638/";

            return (Site.IndexOf(String.Format("/{0}/", SNM)) != -1) ? true : false;
        }

        /// <summary>
        /// TASF 적용 금액
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="PTC">탑승객코드(ADT/CHD/INF)</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="DepartureFromKorea">국내출발여부</param>
        /// <returns></returns>
        public static double GetTASF(int SNM, string PTC, string AirCode, bool DepartureFromKorea)
        {
            double TASF = 0;

            if (!PTC.Equals("INF"))
            {
                Common cm = new Common();
                DateTime NowDate = DateTime.Now;
                int IntTime = Convert.ToInt32(NowDate.ToString("HHmm"));
                
                //전채널 해외출발 TASF 일괄 적용(2018-07-13,김지영매니저)
                if (!DepartureFromKorea)
                {
                    TASF = ("/KE/OZ/7C/TW/AM/GA/TG/9W/BR/B7/PG/QF/CZ/MU/AY/SU/EY/HX/".IndexOf(String.Format("/{0}/", AirCode)).Equals(-1)) ? 10000 : 0;
                }
                else
                {
                    //닷컴(2/3915)(2016-12-30,김지영과장)
                    if ("/2/3915/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        //박람회 기간 TASF 조정(2017/11/17 18:00 ~ 11/19 17:00)
                        //if (cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2017-11-17 18:00") <= 0 && cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2017-11-19 17:00") >= 0)
                        //{
                        //    TASF = 0;
                        //}

                        //전항공사 00:30~05:30까지 TASF 0원(2018-10-18,김지영팀장)
                        //if (IntTime >= 30 && IntTime < 530)
                        //    TASF = 0;
                        //else
                        //{
                            TASF = (AirCode.Equals("BX")) ? 20000 : 10000;
                        //}
                    }
                    //ON-BP(3738/2830/4020/4607)(2016-12-30,김지영과장)
                    //더페이(항공)(5025) 추가(2017-09-20,김덕열과장)
                    //BX 국내출발 발권대행수수료 2만원으로 변경(기존 1만원 -> 2만원)(2017-09-21,김지영차장)
                    else if ("/3738/2830/4020/4607/5025/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        TASF = (AirCode.Equals("AF") || AirCode.Equals("KL") || AirCode.Equals("BX")) ? 20000 : 10000;
                    }
                    //제휴(4681/4907/4713/4820/4578/4547/4715)(2016-12-30,김지영과장)
                    //삼성카드(항공) 모바일(4737) 추가(2017-07-21,김경미차장)
                    //신한카드(항공) 모바일(5045) 추가(2017-07-25,박주영차장)
                    else if ("/4681/4907/4578/4547/4715/4737/5045/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        TASF = (AirCode.Equals("AF") || AirCode.Equals("KL")) ? 20000 : 10000;
                    }
                    //티몬 일괄 1만원 적용(2018-11-26,김경미매니저)
                    else if ("/4925/4926/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        TASF = 10000;
                    }
                    //스카이스캐너(4664/4837) 추가(2017-08-30,김지영차장)
                    else if ("/4664/4837/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        //전항공사 00:30~05:30까지 TASF 0원(2018-10-18,김지영매니저)
                        //if (IntTime >= 30 && IntTime < 530)
                        //    TASF = 0;
                        //else
                        //{
                            //AF,KL은 국내출발 20000원(2017-12-12,김지영차장)
                            //TASF = ("/AF/KL/".IndexOf(String.Format("/{0}/", AirCode)) != -1) ? 20000 : 10000;

                            //전체항공사 10000원(2018-08-09,김지영매니저)
                            TASF = 10000;

                            //2018-02-23 21:00 ~ 2018-02-26 07:00까지 TASF 0원(2018-02-21,김지영차장)
                            //if (cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-02-23 21:00") <= 0 && cm.DateDiff("h", NowDate.ToString("yyyy-MM-dd HH:mm"), "2018-02-26 07:00") >= 0)
                            //    TASF = 0;
                        //}
                    }
                    //카약(4713/4820) 추가(2018-07-10,김지영매니저)
                    else if ("/4713/4820/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        //전항공사 00:30~05:30까지 TASF 0원(2018-10-18,김지영팀장)
                        //if (IntTime >= 30 && IntTime < 530)
                        //    TASF = 0;
                        //else
                        //{
                            //AF,KL은 국내출발 20000원(2017-12-12,김지영차장)
                            //TASF = ("/AF/KL/".IndexOf(String.Format("/{0}/", AirCode)) != -1) ? 20000 : 10000;
                            
                            //전체항공사 10000원(2018-08-09,김지영매니저)
                            TASF = 10000;
                        //}
                    }
                    //네이버(4638)는 LCC(LJ,TW,ZE,VJ)일 경우 국내/해외 출발 모두 1만원 적용(2017-02-28,김지영과장)
                    else if (SNM.Equals(4638))
                    {
                        //LJ는 갈릴레오 운임 사용으로 제외(2018-10-22,김경미매니저)
                        if (AirCode.Equals("TW") || AirCode.Equals("ZE") || AirCode.Equals("VJ"))
                            TASF = 10000;
                    }
                    //삼성카드(항공_TW몰)(5236)
                    //11번가(4929/4924) 추가(2018-10-17,김경미매니저)
                    else if ("/5236/4929/4924/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                    {
                        TASF = 10000;
                    }
                }
            }
            
            return TASF;
        }

        /// <summary>
        /// TASF 적용 방식 선택 가능 여부
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="AirCode">항공사코드</param>
        /// <returns>Y:사용자 선택 가능, N:사용자 선택 불가</returns>
        public static string SelectUserTASF(int SNM, string AirCode)
        {
            string UserTASF = "N";

            //스카이스캐너,카약 사용자 선택 불가(2017-08-30,김지영차장)
            ////스카이스캐너(4664,4837)는 TASF 적용여부를 예약자가 선택하여 진행할 수 있도록 한다.(2017-01-02,김지영과장)
            //if (SNM.Equals(4664) || SNM.Equals(4837))
            //{
            //    //LJ항공은 TASF 강제 적용(2017-01-04,김지영과장)
            //    //모든 항공사에 대해서 사용자 선택 가능하도록 수정(2017-03-08,김지영과장)
            //    //if (!AirCode.Equals("LJ"))
            //        UserTASF = "Y";
            //}
            ////카약(4713,4820)은 TASF 적용여부를 예약자가 선택하여 진행할 수 있도록 한다.(2017-03-21,김지영과장)
            //else if (SNM.Equals(4713) || SNM.Equals(4820))
            //    UserTASF = "Y";

            return UserTASF;
        }

		/// <summary>
		/// MaxStay 계산
		/// </summary>
		/// <param name="DTD">출발일</param>
		/// <param name="MaxStayValue">MaxStay값</param>
		/// <returns></returns>
		public static string ConvertToMaxStay(string DTD, string MaxStayValue)
		{
			string MaxStay = string.Empty;
			DateTime SDate = Convert.ToDateTime(DTD);
			DateTime MDate = Convert.ToDateTime(new Common().ConvertToDateTime(MaxStayValue));

			int[] SDatePart = new Int32[3];
			int[] MDatePart = new Int32[3];

			SDatePart[0] = SDate.Year;
			SDatePart[1] = SDate.Month;
			SDatePart[2] = SDate.Day;

			MDatePart[0] = MDate.Year;
			MDatePart[1] = MDate.Month;
			MDatePart[2] = MDate.Day;

			if ((MDatePart[2] - SDatePart[2]) >= 0 && (MDatePart[2] - SDatePart[2]) < 3)
			{
				if (SDatePart[1] > MDatePart[1])
					MaxStay = String.Concat((MDatePart[1] + 12) - SDatePart[1], "M");
				else if (SDatePart[1] < MDatePart[1])
					MaxStay = String.Concat(MDatePart[1] - SDatePart[1], "M");
				else
				{
					if (SDatePart[0].Equals(MDatePart[0]))
						MaxStay = "0D";
					else
						MaxStay = "1Y";
				}
			}
			else
			{
				if (SDatePart[1].Equals(MDatePart[1]) && SDatePart[0].Equals(MDatePart[0]))
					MaxStay = String.Concat(MDatePart[2] - SDatePart[2], "D");
				else
				{
					DateTime SDateNext = Convert.ToDateTime(String.Concat(SDate.AddMonths(1).ToString("yyyy-MM"), "-01"));
					DateTime MDatePrev = Convert.ToDateTime(String.Concat(MDate.ToString("yyyy-MM"), "-01")).AddDays(-1);
					int SDateDays = SDateNext.AddDays(-1).Day;
					double TermDays = 0;

					if (MDate.Month - SDate.Month > 1 || MDate.Year > SDate.Year)
					{
						TermDays = new Common().DateDiff("d", SDateNext, MDatePrev);
					}

					MaxStay = String.Concat((SDateDays - SDate.Day) + TermDays + MDate.Day, "D");
				}
			}

			return MaxStay;
		}

		/// <summary>
		/// MaxStay 구하기
		/// </summary>
		/// <param name="MaxStay1">현재 MaxStay</param>
		/// <param name="MaxStay2">비교할 MaxStay</param>
		/// <returns></returns>
		public string SetMaxStay(string MaxStay1, string MaxStay2)
		{
			if (String.IsNullOrWhiteSpace(MaxStay1))
				return MaxStay2;
			else if (String.IsNullOrWhiteSpace(MaxStay2))
				return MaxStay1;
			else
			{
				int Num1 = ExtractNumber(MaxStay1);
				int Num2 = ExtractNumber(MaxStay2);
				string Term1 = Right(MaxStay1, 1);
				string Term2 = Right(MaxStay2, 1);

				if (Term1.Equals("Y"))
				{
					if (Term2.Equals("Y"))
					{
						return String.Concat(SetMinNumber(Num1, Num2), "Y");
					}
					else if (Term2.Equals("M"))
					{
						return (Num2 < (Num1 * 12)) ? MaxStay2 : MaxStay1;
					}
					else if (Term2.Equals("D"))
					{
						return (Num2 < (Num1 * 365)) ? MaxStay2 : MaxStay1;
					}
					else
					{
						return MaxStay1;
					}
				}
				else if (Term1.Equals("M"))
				{
					if (Term2.Equals("Y"))
					{
						return (Num1 < (Num2 * 12)) ? MaxStay1 : MaxStay2;
					}
					else if (Term2.Equals("M"))
					{
						return String.Concat(SetMinNumber(Num1, Num2), "M");
					}
					else if (Term2.Equals("D"))
					{
						return (Num2 < (Num1 * 30)) ? MaxStay2 : MaxStay1;
					}
					else
					{
						return MaxStay1;
					}
				}
				else if (Term1.Equals("D"))
				{
					if (Term2.Equals("Y"))
					{
						return (Num1 < (Num2 * 365)) ? MaxStay1 : MaxStay2;
					}
					else if (Term2.Equals("M"))
					{
						return (Num1 < (Num2 * 30)) ? MaxStay1 : MaxStay2;
					}
					else if (Term2.Equals("D"))
					{
						return String.Concat(SetMinNumber(Num1, Num2), "D");
					}
					else
					{
						return MaxStay1;
					}
				}
				else
					return MaxStay2;
			}
		}

        /// <summary>
        /// 발권마감일 체크 사이트
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <returns></returns>
        public bool ApplyLTD(int SNM)
        {
            //ABS는 발권마감일 체크를 하지 않는다.(2014-09-30,김지영과장요청)
            //모두닷컴(항공),모두닷컴(모바일),삼성카드(항공),삼성카드(임직원몰)은 발권마감일 체크를 하지 않는다.(2014-11-25,김지영과장요청)
            //NV모두투어(항공)(4638)는 발권마감일 체크를 하지 않는다.(2015-06-26,정성하과장요청)
            //스카이스캐너(4664,4837)는 발권마감일 체크를 하지 않는다.(2015-09-02,정성하과장요청)
            //홈플러스(항공)(4657),위메프(해외항공)(4681,4907),라이나생명(항공권)(4680),카카오(국제선)(4716),카약(국제선)(4713,4820),신한카드(항공)(4715),씨티카드(항공)(4759)는 발권마감일 체크를 하지 않는다.(2016-09-28,정성하과장요청)
            //11번가(4924,4929),티몬(4925,4926)은 발권마감일 체크를 하지 않는다.(2017-09-14,김지영차장)
            //G마켓(5020,5119),옥션(5161,5163),G9(5162,5164)은 발권마감일 체크를 하지 않는다.(2018-07-20,김지영매니저)
            if ("/68/2/3915/4578/4547/4638/4664/4837/4657/4681/4907/4680/4716/4713/4820/4715/4759/4924/4929/4925/4926/5020/5119/5161/5163/5162/5164/".IndexOf(String.Format("/{0}/", SNM)) != -1)
                return true;
            else
                return false;
        }

		/// <summary>
		/// 발권가능일 체크
		/// </summary>
		/// <param name="SNM">사이트 번호</param>
		/// <param name="MTL">모두투어 TL</param>
		/// <param name="ATL">발권마감일(항공사 TL)</param>
		/// <param name="AirCode">항공사코드</param>
        /// <param name="CheckChinaAir">중국항공사 발권가능여부</param>
        /// <param name="WorkingDay">업무일 여부</param>
        /// <param name="LTD">발권마감일 적용여부</param>
		/// <param name="FTR">필터링 적용여부</param>
		/// <returns></returns>
        public bool ApplyTLCondition(int SNM, DateTime MTL, DateTime ATL, string AirCode, bool CheckChinaAir, bool WorkingDay, string LTD, string FTR)
		{
			bool AppCondition = true;

			if (FTR.Equals("Y"))
			{
				if (LTD.Equals("Y"))
				{
                    if (!ApplyLTD(SNM))
                    {
						//발권가능일
						if (DateDiff("d", MTL, ATL) < 0)
							AppCondition = false;
					}
				}

				if (AppCondition)
				{
                    DateTime NowDate = DateTime.Now;
                    
                    //모두닷컴은 발권마감일이 오늘인 경우 업무일 여부에 따라 예약 불가 시간 처리(2018-02-19,김지영차장) -> 보류
                    //if (SNM.Equals(2) || SNM.Equals(3915))
                    //{
                    //    if (NowDate.ToString("yyyy-MM-dd").Equals(ATL.ToString("yyyy-MM-dd")))
                    //    {
                    //        if (WorkingDay)
                    //        {
                    //            //업무일인 경우 17시 이후라면 예약 불가(2018-02-19,김지영차장)
                    //            if (NowDate.Hour >= 17)
                    //                AppCondition = false;
                    //        }
                    //        else
                    //        {
                    //            //공휴일인 경우 15시 이후라면 예약 불가(2018-02-19,김지영차장)
                    //            if (NowDate.Hour >= 15)
                    //                AppCondition = false;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                        //19시 이후라면 발권마감일이 오늘인 운임은 예약 불가(2017-09-19,김지영과장요청)
                        if (NowDate.Hour >= 19 && NowDate.ToString("yyyy-MM-dd").Equals(ATL.ToString("yyyy-MM-dd")))
                            AppCondition = false;
                    //}
				    
					//중국항공사(CZ,CA,SC,ZH)의 경우 예약일 또는 예약일 다음날이 업무일이 아닐경우 출발일이 10일전인 경우 요금을 출력하지 않는다.
					if (CheckChinaAir && (AirCode.Equals("CZ") || AirCode.Equals("CA") || AirCode.Equals("SC") || AirCode.Equals("ZH")))
						AppCondition = false;
				}
			}

			return AppCondition;
		}

		/// <summary>
		/// 입력받은 날짜를 기준으로 기간 계산(AP조건 계산시 사용)
		/// </summary>
		/// <param name="BasicDate">기준이 되는 날짜(yyyy-MM-dd)</param>
		/// <param name="DateTerm">기간코드</param>
		/// <param name="Gubun">이전/이후(After/Before) 구분</param>
		/// <returns></returns>
		public DateTime AdvanceDateDiff(string BasicDate, string DateTerm, string Gubun)
		{
			return AdvanceDateDiff(Convert.ToDateTime(BasicDate), DateTerm, Gubun);
		}

		/// <summary>
		/// 입력받은 날짜를 기준으로 기간 계산(AP조건 계산시 사용)
		/// </summary>
		/// <param name="BasicDate">기준이 되는 날짜(yyyy-MM-dd)</param>
		/// <param name="Date2">기간코드</param>
		/// <param name="Gubun">이전/이후 구분</param>
		/// <returns></returns>
		public DateTime AdvanceDateDiff(DateTime BasicDate, string DateTerm, string Gubun)
		{
			string DateGubun = DateTerm.Substring(1, 1);
			int DateNum = RequestInt(DateTerm.Substring(2, 3)) * (Gubun.Equals("Before") ? -1 : 1);

			return (DateGubun.Equals("Y")) ? BasicDate.AddYears(DateNum) : ((DateGubun.Equals("M")) ? BasicDate.AddMonths(DateNum) : BasicDate.AddDays(DateNum));
		}

		/// <summary>
		/// 프로모션 적용 가능 여부(아마데우스)
		/// </summary>
		/// <param name="AirCodeItem">프로모션 적용 항공사</param>
		/// <param name="FareTypeItem">프로모션 적용 FareType</param>
        /// <param name="FareBasisItem">프로모션 적용 FareBasis</param>
        /// <param name="CabinClassItem">프로모션 적용 캐빈클래스</param>
        /// <param name="BookingClassItem">프로모션 적용 부킹클래스</param>
        /// <param name="BookingClassExcItem">프로모션 적용 제외 부킹클래스</param>
        /// <param name="SpecilItem">프로모션 적용 특가 제외</param>
        /// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
		/// <param name="xnMgr">네임스페이스</param>
		/// <returns></returns>
        public static bool ApplyPromotion(string AirCodeItem, string FareTypeItem, string FareBasisItem, string CabinClassItem, string BookingClassItem, string BookingClassExcItem, string SpecilItem, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
		{
			bool AppPromo = false;

			if (PaxFareProduct != null)
			{
				if (String.IsNullOrWhiteSpace(AirCodeItem) || AirCodeItem.Equals(PaxFareProduct.SelectSingleNode("m:paxFareDetail/m:codeShareDetails[m:transportStageQualifier='V']/m:company", xnMgr).InnerText))
				{
					if (String.IsNullOrWhiteSpace(FareTypeItem) || CheckFareType(FareTypeItem, PaxFareProduct, xnMgr))
					{
						if (String.IsNullOrWhiteSpace(CabinClassItem) || CabinClassItem.IndexOf(PaxFareProduct.SelectSingleNode("m:fareDetails[1]/m:groupOfFares[1]/m:productInformation/m:cabinProduct/m:cabin", xnMgr).InnerText) != -1)
						{
                            if (String.IsNullOrWhiteSpace(FareBasisItem) || CheckFareBasis(FareBasisItem, PaxFareProduct, xnMgr))
                            {
                                if (CheckBookingClass(BookingClassItem, PaxFareProduct, xnMgr) && CheckBookingExcludeClass(BookingClassExcItem, PaxFareProduct, xnMgr))
                                {
                                    if (SpecilItem.Equals("N") || CheckSpecilItem(SpecilItem, AirCodeItem, PaxFareProduct, xnMgr))
                                        AppPromo = true;
                                }
                            }
						}
					}
				}
			}

			return AppPromo;
		}
        
        /// <summary>
        /// 프로모션 적용 가능 여부(세이버)
        /// </summary>
        /// <param name="AirCodeItem">프로모션 적용 항공사</param>
        /// <param name="FareTypeItem">프로모션 적용 FareType</param>
        /// <param name="FareBasisItem">프로모션 적용 FareBasis</param>
        /// <param name="CabinClassItem">프로모션 적용 캐빈클래스</param>
        /// <param name="BookingClassItem">프로모션 적용 부킹클래스</param>
        /// <param name="BookingClassExcItem">프로모션 적용 제외 부킹클래스</param>
        /// <param name="SpecilItem">프로모션 적용 특가 제외</param>
        /// <param name="PaxFareProduct">세이버 요금정보 XmlNode</param>
        /// <returns></returns>
        public static bool ApplyPromotionSabre(string AirCodeItem, string FareTypeItem, string FareBasisItem, string CabinClassItem, string BookingClassItem, string BookingClassExcItem, string SpecilItem, XmlNode PaxFareProduct)
        {
            bool AppPromo = false;

            if (PaxFareProduct != null)
            {
                if (String.IsNullOrWhiteSpace(AirCodeItem) || AirCodeItem.Equals(PaxFareProduct.SelectSingleNode("summary").Attributes.GetNamedItem("pvc").InnerText))
                {
                    if (String.IsNullOrWhiteSpace(FareTypeItem) || FareTypeItem.IndexOf(String.Concat(PaxFareProduct.SelectSingleNode("paxFareGroup/paxFare/segFareGroup/segFare/fare/fare/fareType").InnerText)) != -1)
                    {
                        if (String.IsNullOrWhiteSpace(CabinClassItem) || CabinClassItem.IndexOf(PaxFareProduct.SelectSingleNode("paxFareGroup/paxFare/segFareGroup/segFare/fare/cabin").Attributes.GetNamedItem("cabin").InnerText) != -1)
                        {
                            if (String.IsNullOrWhiteSpace(FareBasisItem))
                            {
                                if (CheckBookingClassSabre(BookingClassItem, PaxFareProduct) && CheckBookingExcludeClassSabre(BookingClassExcItem, PaxFareProduct))
                                {
                                    if (SpecilItem.Equals("N"))
                                        AppPromo = true;
                                }
                            }
                        }
                    }
                }
            }

            return AppPromo;
        }

		/// <summary>
		/// 프로모션 적용
		/// </summary>
        /// <param name="Fare">항공운임</param>
        /// <param name="PTC">승객타입</param>
		/// <param name="PromItem">프로모션 정보</param>
		/// <returns></returns>
		public double PromotionFare(double Fare, string PTC, XmlNode PromItem)
		{
            if (PTC.Equals("INF"))
                return Fare;
            else
            {
                if (PromItem != null && PromItem.HasChildNodes)
                {
                    if (Common.ChangePaxType1(PTC).Equals("CHD") && PromItem.SelectSingleNode("childDiscountYN").InnerText.Equals("Y"))
                        return Fare;
                    else
                    {
                        double dblFare = Fare;
                        double Discount = RequestDouble(PromItem.SelectSingleNode("discount").InnerText);
                        double DisComm = RequestDouble(PromItem.SelectSingleNode("commission").InnerText);
                        double DisFare = RequestDouble(PromItem.SelectSingleNode("fareDiscount").InnerText);
                        double Incentive = RequestDouble(PromItem.SelectSingleNode("incentive").InnerText);

                        if (Discount >= 1)
                            dblFare = Common.IntCutting((dblFare - Discount), 100);
                        else if (Discount < 1 && Discount > 0)
                            dblFare = Common.IntCutting((dblFare - (dblFare * Discount)), 100);

                        if (DisComm >= 1)
                            dblFare = Common.IntCutting((dblFare - DisComm), 100);
                        else if (DisComm < 1 && DisComm > 0)
                            dblFare = Common.IntCutting((dblFare - (dblFare * DisComm)), 100);

                        if (DisFare >= 1)
                            dblFare = Common.IntCutting((dblFare - DisFare), 100);
                        else if (DisFare < 1 && DisFare > 0)
                            dblFare = Common.IntCutting((dblFare - (dblFare * DisFare)), 100);

                        if (Incentive >= 1)
                            dblFare = Common.IntCutting((dblFare - Incentive), 100);
                        else if (Incentive < 1 && Incentive > 0)
                            dblFare = Common.IntCutting((dblFare - (dblFare * Incentive)), 100);

                        return dblFare;
                    }
                }
                else
                    return Fare;
            }
		}

		/// <summary>
		/// FareType 체크
		/// </summary>
		/// <param name="FareTypeItem">프로모션 적용 FareType</param>
		/// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
		/// <param name="xnMgr">네임스페이스</param>
		/// <returns></returns>
		private static bool CheckFareType(string FareTypeItem, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
		{
			bool AppFareType = false;

            //SF인 페어타입의 경우 RB와 RA로 동일 프로모션 적용(2015-05-08,김지영과장)
            FareTypeItem = FareTypeItem.Replace("SF", "SF/RB/RA");

			foreach (string FarType in FareTypeItem.Split('/'))
			{
                if (!String.IsNullOrWhiteSpace(FarType))
                {
                    if (PaxFareProduct.SelectNodes(String.Format("m:fareDetails[1]/m:groupOfFares[1]/m:productInformation/m:fareProductDetail[m:fareType='{0}']", FarType.Trim()), xnMgr).Count > 0)
                    {
                        AppFareType = true;
                        break;
                    }
                }
			}

			return AppFareType;
		}

        /// <summary>
        /// FareBasis 체크
        /// </summary>
        /// <param name="FareBasisItem">프로모션 적용 FareBasis</param>
        /// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <returns></returns>
        private static bool CheckFareBasis(string FareBasisItem, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
        {
            bool AppFareBasis = false;

            foreach (string FareBasis in FareBasisItem.Split('/'))
            {
                if (!String.IsNullOrWhiteSpace(FareBasis))
                {
                    if (PaxFareProduct.SelectNodes(String.Format("m:fareDetails/m:groupOfFares/m:productInformation/m:fareProductDetail[m:fareBasis!='{0}']", FareBasis.Trim()), xnMgr).Count.Equals(0))
                    {
                        AppFareBasis = true;
                        break;
                    }
                }
            }

            return AppFareBasis;
        }

        /// <summary>
        /// BookingClass 체크(아마데우스)
        /// </summary>
        /// <param name="BookingClassItem">프로모션 적용 BookingClass</param>
        /// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <returns></returns>
        private static bool CheckBookingClass(string BookingClassItem, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
        {
            bool AppBookingClass = false;

            if (String.IsNullOrWhiteSpace(BookingClassItem))
                AppBookingClass = true;
            else
            {
                foreach (string BookingClass in BookingClassItem.Split('/'))
                {
                    if (!String.IsNullOrWhiteSpace(BookingClass))
                    {
                        if (PaxFareProduct.SelectNodes(String.Format("m:fareDetails/m:groupOfFares/m:productInformation/m:cabinProduct[m:rbd!='{0}']", BookingClass.Trim()), xnMgr).Count.Equals(0))
                        {
                            AppBookingClass = true;
                            break;
                        }
                    }
                }
            }

            return AppBookingClass;
        }

        /// <summary>
        /// BookingClass 체크(세이버)
        /// </summary>
        /// <param name="BookingClassItem">프로모션 적용 BookingClass</param>
        /// <param name="PaxFareProduct">세이버 요금정보 XmlNode</param>
        /// <returns></returns>
        private static bool CheckBookingClassSabre(string BookingClassItem, XmlNode PaxFareProduct)
        {
            bool AppBookingClass = false;

            if (String.IsNullOrWhiteSpace(BookingClassItem))
                AppBookingClass = true;
            else
            {
                foreach (string BookingClass in BookingClassItem.Split('/'))
                {
                    if (!String.IsNullOrWhiteSpace(BookingClass))
                    {
                        if (PaxFareProduct.SelectNodes(String.Format("paxFareGroup/paxFare[1]/segFareGroup/segFare/fare/cabin[@rbd!='{0}']", BookingClass.Trim())).Count.Equals(0))
                        {
                            AppBookingClass = true;
                            break;
                        }
                    }
                }
            }

            return AppBookingClass;
        }

        /// <summary>
        /// BookingClass 제외 체크(아마데우스)
        /// </summary>
        /// <param name="BookingClassItem">프로모션 적용 제외 BookingClass</param>
        /// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <returns></returns>
        private static bool CheckBookingExcludeClass(string BookingClassExcItem, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
        {
            bool AppBookingClass = true;

            if (!String.IsNullOrWhiteSpace(BookingClassExcItem))
            {
                foreach (string BookingClass in BookingClassExcItem.Split('/'))
                {
                    if (!String.IsNullOrWhiteSpace(BookingClass))
                    {
                        if (PaxFareProduct.SelectNodes(String.Format("m:fareDetails/m:groupOfFares/m:productInformation/m:cabinProduct[m:rbd='{0}']", BookingClass.Trim()), xnMgr).Count > 0)
                        {
                            AppBookingClass = false;
                            break;
                        }
                    }
                }
            }

            return AppBookingClass;
        }

        /// <summary>
        /// BookingClass 제외 체크(세이버)
        /// </summary>
        /// <param name="BookingClassItem">프로모션 적용 제외 BookingClass</param>
        /// <param name="PaxFareProduct">세이버 요금정보 XmlNode</param>
        /// <returns></returns>
        private static bool CheckBookingExcludeClassSabre(string BookingClassExcItem, XmlNode PaxFareProduct)
        {
            bool AppBookingClass = true;

            if (!String.IsNullOrWhiteSpace(BookingClassExcItem))
            {
                foreach (string BookingClass in BookingClassExcItem.Split('/'))
                {
                    if (!String.IsNullOrWhiteSpace(BookingClass))
                    {
                        if (PaxFareProduct.SelectNodes(String.Format("paxFareGroup/paxFare[1]/segFareGroup/segFare/fare/cabin[@rbd='{0}']", BookingClass.Trim())).Count > 0)
                        {
                            AppBookingClass = false;
                            break;
                        }
                    }
                }
            }

            return AppBookingClass;
        }

        /// <summary>
        /// 특가 체크
        /// </summary>
        /// <param name="SpecilItem">프로모션 적용 특가 제외</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="PaxFareProduct">아마데우스 요금정보 XmlNode</param>
        /// <param name="xnMgr">네임스페이스</param>
        /// <returns></returns>
        private static bool CheckSpecilItem(string SpecilItem, string AirCode, XmlNode PaxFareProduct, XmlNamespaceManager xnMgr)
        {
            bool AppSpecilItem = true;

            if (SpecilItem.Equals("Y"))
            {
                foreach (XmlNode FareDetails in PaxFareProduct.SelectNodes("m:fareDetails", xnMgr))
                {
                    if (FareDetails.SelectNodes("m:groupOfFares/m:ticketInfos", xnMgr).Count > 0 && FareDetails.SelectNodes("m:groupOfFares/m:ticketInfos/m:additionalFareDetails", xnMgr).Count > 0 && FareDetails.SelectNodes("m:groupOfFares/m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).Count > 0)
                    {
                        if (FareDetails.SelectSingleNode("m:groupOfFares/m:ticketInfos/m:additionalFareDetails/m:ticketDesignator", xnMgr).InnerText.Equals(AirCode))
                        {
                            AppSpecilItem = false;
                            break;
                        }
                    }
                }
            }

            return AppSpecilItem;
        }

        /// <summary>
        /// Q-Charge 구하기
        /// </summary>
        /// <param name="QChargeString">QCharge 정보 문자열</param>
        /// <returns>[0]QCharge, [1]ROE, [2]HKG</returns>
        public static double[] GetQCharge(string QChargeString)
        {
            double[] QCharge = new Double[3] { 0, 0, 0 };

            if (!String.IsNullOrWhiteSpace(QChargeString))
            {
                String[] Pattern = {
									@"\sQ(?<QCharge>[0-9]+\.[0-9]+)\s",                 //_Q5.80_
                                    @"\sQ(?<QCharge>[0-9]+\.[0-9]+)Q",                  //_Q17.12Q5.80_(앞부분)
                                    @"[0-9]+Q(?<QCharge>[0-9]+\.[0-9]+)\s",             //_Q17.12Q5.80_(뒷부분)
                                    @"\sQ\s[A-Z]{6}(?<QCharge>[0-9]+\.[0-9]+)\s",       //_Q YVRSEL11.73_
                                    @"\sQ\s[A-Z]{6}(?<QCharge>[0-9]+\.[0-9]+)Q\s",      //_Q HKGYVR5.79Q HKGYVR11.73_(앞부분)
                                    @"[0-9]+Q\s[A-Z]{6}(?<QCharge>[0-9]+\.[0-9]+)\s",   //_Q HKGYVR5.79Q HKGYVR11.73_(뒷부분)
									@"\sROE(?<ROE>[0-9]+\.?[0-9]*)$",                   //_ROE1159.796000
                                    @"\sROE(?<ROE>[0-9]+\.?[0-9]*)\s"                   //_ROE1159.796000_
                                    //@"[A-Z]{2}\s(?<HKG>HKG)[0-9]*"                      //_HKG_
                                    };

                foreach (string pattern in Pattern)
                {
                    Regex regex = new Regex(pattern, RegexOptions.Singleline);
                    Match match = regex.Match(QChargeString);

                    while (match.Success)
                    {
                        if (!String.IsNullOrWhiteSpace(match.Groups["QCharge"].Value))
                            QCharge[0] += Convert.ToDouble(match.Groups["QCharge"].Value);

                        if (!String.IsNullOrWhiteSpace(match.Groups["ROE"].Value))
                            QCharge[1] = Convert.ToDouble(match.Groups["ROE"].Value);

                        //if (QCharge[2].Equals(0) && !String.IsNullOrWhiteSpace(match.Groups["HKG"].Value))
                        //    QCharge[2] = 1;

                        match = match.NextMatch();
                    }
                }
            }

            return QCharge;
        }

        /// <summary>
        /// 무료수하물 표시
        /// </summary>
        /// <param name="baggage">무료수하물 정보</param>
        /// <returns></returns>
        public static string BaggageEmpty(string baggage)
        {
            return String.IsNullOrWhiteSpace(baggage) ? "없음" : baggage;
        }

		/// <summary>
		/// 탑승객 타이틀
		/// </summary>
		/// <param name="PaxType">탑승객 유형(ADT/CHD/INF)</param>
		/// <param name="PaxGender">탑승객 성별(M/F/MI/FI)</param>
        /// <returns>탑승객 타이틀(MR/MRS/MS/MSTR/MISS)</returns>
		public static string GetPaxTitle(string PaxType, string PaxGender)
		{
			return PaxType.Equals("ADT") ? (PaxGender.Equals("F") ? "MS" : "MR") : ((PaxGender.Equals("F") || PaxGender.Equals("FI")) ? "MISS" : "MSTR");
		}

        /// <summary>
        /// 탑승객 성별
        /// </summary>
        /// <param name="PaxTitle">탑승객 타이틀(MR/MRS/MS/MSTR/MISS)</param>
        /// <returns>탑승객 성별(M/F)</returns>
        public static string GetPaxGender(string PaxTitle)
        {
            return (PaxTitle.Equals("MR") || PaxTitle.Equals("MSTR")) ? "M" : "F";
        }

		/// <summary>
		/// 만 나이 구하기(Child 나이 등록시 사용)
		/// </summary>
		/// <param name="Birthday">생년월일(yyyy-mm-dd)</param>
		/// <param name="NowDate">기준일(yyyy-mm-dd)</param>
		/// <returns></returns>
		public int KoreanAge(string Birthday, string NowDate)
		{
			int fullAge = 0;

			Birthday = RequestDateTime(Birthday);

			if (IsDateTime(Birthday))
			{
				DateTime NDate = (String.IsNullOrEmpty(NowDate)) ? DateTime.Now : Convert.ToDateTime(RequestDateTime(NowDate));
				DateTime BDate = Convert.ToDateTime(Birthday);

				fullAge = (Convert.ToInt32(NDate.ToString("yyyy")) - Convert.ToInt32(BDate.ToString("yyyy")));

				if (Convert.ToInt32(BDate.ToString("MMdd")) - Convert.ToInt32(NDate.ToString("MMdd")) > 0)
					fullAge = fullAge - 1;
			}

			return fullAge;
		}

		/// <summary>
		/// 개월수 구하기(Infant 개월수 등록시 사용)
		/// </summary>
		/// <param name="Birthday">생년월일(yyyy-mm-dd)</param>
		/// <param name="NowDate">기준일(yyyy-mm-dd)</param>
		/// <returns></returns>
		public int MonthAge(string Birthday, string NowDate)
		{
			int month = 0;

			Birthday = RequestDateTime(Birthday);

			if (IsDateTime(Birthday))
			{
				DateTime NDate = (String.IsNullOrEmpty(NowDate)) ? DateTime.Now : Convert.ToDateTime(RequestDateTime(NowDate));
				DateTime BDate = Convert.ToDateTime(Birthday);

				month = Convert.ToInt32(DateDiff("M", BDate, NDate));
			}

			return month;
		}

		/// <summary>
		/// 작은 수 구하기
		/// </summary>
		/// <param name="Num1">첫번째 수</param>
		/// <param name="Num2">두번째 수</param>
		/// <returns></returns>
		public static int SetMinNumber(int Num1, int Num2)
		{
			return (Num1 < Num2) ? Num1 : Num2;
		}

		/// <summary>
		/// 큰 수 구하기
		/// </summary>
		/// <param name="Num1">첫번째 수</param>
		/// <param name="Num2">두번째 수</param>
		/// <returns></returns>
		public static int SetMaxNumber(int Num1, int Num2)
		{
			return (Num1 > Num2) ? Num1 : Num2;
		}

		/// <summary>
		/// 카드유효기간 형식으로 출력
		/// </summary>
		/// <param name="ValidThru">YYYYMM 형식의 날짜</param>
		/// <returns>MMYY 형식의 날짜</returns>
		public static string CardValidThru(string ValidThru)
		{
			return String.Concat(ValidThru.Substring(4, 2), ValidThru.Substring(2, 2));
		}

		/// <summary>
		/// Amadeus Command 발권시 할부개월 표시
		/// </summary>
		/// <param name="Installment">할부개월수</param>
		/// <returns></returns>
		public static string AmadeusEntryInstallment(string Installment)
		{
			return (Installment.Equals("00") || String.IsNullOrWhiteSpace(Installment)) ? "*E00" : String.Concat("*E", Installment);
		}

		/// <summary>
		/// Amadeus Command 발권시 결제요청금액 표시
		/// </summary>
		/// <param name="Currency">통화코드</param>
		/// <param name="Amount">결제요청금액</param>
		/// <returns></returns>
		public static string AmadeusEntryAmount(string Currency, string Amount)
		{
			Amount = Amount.Replace(",", "");
			return (String.IsNullOrWhiteSpace(Amount) || Amount.Equals("0")) ? "" : String.Concat("/", (String.IsNullOrWhiteSpace(Currency) ? "KRW" : Currency), Amount);
		}

        /// <summary>
        /// 예약시 항공사별 전화번호 양식
        /// </summary>
        /// <param name="AirCode"></param>
        /// <param name="Tel"></param>
        /// <param name="OSI">OSI 형식여부(항공사코드 삽입여부)(true:항공사코드 미삽입, false:항공사코드 삽입)</param>
        /// <returns></returns>
        public static string BookingTelFormat(string AirCode, string Tel, bool OSI)
        {
            string TelFormat = string.Empty;

            switch (AirCode)
            {
                case "AA": TelFormat = String.Format("{0}CTCM 82-{1}", (OSI ? "" : "AA "), Tel.Substring(1)); break;
                case "AC": TelFormat = String.Format("CTCM{0}-82{1}/P1", (OSI ? "" : "AC"), Tel.Replace("-", "").Substring(1)); break;
                case "AF": TelFormat = String.Format("+82{0}", Tel.Replace("-", "").Substring(1)); break;
                case "AI": TelFormat = String.Format("{0}CTCM 82-{1}", (OSI ? "" : "AI "), Tel.Substring(1)); break;
                case "AM": TelFormat = String.Format("{0}CTCM 82-{1}", (OSI ? "" : "AI "), Tel.Substring(1)); break;
                case "AY": TelFormat = String.Format("{0}CTCM 82 {1}", (OSI ? "" : "AY "), Tel.Replace("-", " ").Substring(1)); break;
                case "BA": TelFormat = String.Format("SK CTCM {0}-82{1}", (OSI ? "" : "BA"), Tel.Replace("-", "").Substring(1)); break;
                case "CA": TelFormat = String.Format("{0}CTCT {1}", (OSI ? "" : "CA "), Tel.Replace("-", "")); break;
                case "CI": TelFormat = String.Format("{0}CTCM 82-{1} PAX1", (OSI ? "" : "CI "), Tel.Replace("-", "")); break;
                case "CX": TelFormat = String.Format("{0}CTCM SEL {1}", (OSI ? "" : "CX "), Tel.Replace("-", "").Substring(1)); break;
                case "CZ": TelFormat = String.Format("{0}CTCM {1}/P1", (OSI ? "" : "CZ "), Tel.Replace("-", "")); break;
                case "DL": TelFormat = String.Format("{0}CTC 82-{1}", (OSI ? "" : "DL "), Tel.Substring(1)); break;
                case "EK": TelFormat = String.Format("{0}CTCM 82{1}", (OSI ? "" : "EK "), Tel.Replace("-", "").Substring(1)); break;
                case "ET": TelFormat = String.Format("{0}CTCM 82 {1}", (OSI ? "" : "ET "), Tel.Replace("-", " ").Substring(1)); break;
                case "EY": TelFormat = String.Format("{0}CTCP 82{1}", (OSI ? "" : "EY "), Tel.Replace("-", "").Substring(1)); break;
                case "HA": TelFormat = String.Format("{0}82 {1}", (OSI ? "" : "HA "), Tel.Replace("-", " ").Substring(1)); break;
                case "HX": TelFormat = String.Format("{0}CTCM SEL {1}", (OSI ? "" : "HX "), Tel.Replace("-", "").Substring(1)); break;
                case "HY": TelFormat = String.Format("{0}CTC 82-{1}", (OSI ? "" : "HY "), Tel.Substring(1)); break;
                case "JL": TelFormat = String.Format("{0}KOR {1}/M/P1", (OSI ? "" : "JL "), Tel); break;
                case "LH": TelFormat = String.Format(" +82{0}/P1", Tel.Replace("-", "").Substring(1)); break;
                case "MF": TelFormat = String.Format("{0}{1} PAX", (OSI ? "" : "MF "), Tel); break;
                case "MU": TelFormat = String.Format("{0}CTCM 0082{1}", (OSI ? "" : "MU "), Tel.Replace("-", "").Substring(1)); break;
                case "NH": TelFormat = String.Format("{0}CTCM {1}", (OSI ? "" : "NH "), Tel); break;
                case "NX": TelFormat = String.Format("{0}CTCM {1}", (OSI ? "" : "NX "), Tel); break;
                case "PR": TelFormat = String.Format("{0}CTCM 82 {1}/P1", (OSI ? "" : "PR "), Tel.Replace("-", " ").Substring(1)); break;
                case "PS": TelFormat = String.Format("{0}CTCM 82{1}", (OSI ? "" : "PS "), Tel.Replace("-", "").Substring(1)); break;
                case "PX": TelFormat = String.Format("{0}82 {1}", (OSI ? "" : "PX"), Tel.Replace("-", " ").Substring(1)); break;
                case "QR": TelFormat = String.Format("82{0}", Tel.Replace("-", "").Substring(1)); break;
                case "SU": TelFormat = String.Format("{0}CT MOBILE 82 {1}", (OSI ? "" : "SU "), Tel.Replace("-", " ").Substring(1)); break;
                case "SQ": TelFormat = String.Format("{0}CTCM 82 {1}", (OSI ? "" : "SQ "), Tel.Replace("-", " ").Substring(1)); break;
                case "TK": TelFormat = String.Format("{0}CTCP SEL 00 82 {1}", (OSI ? "" : "TK "), Tel.Replace("-", " ").Substring(1)); break;
                case "VJ": TelFormat = String.Format("{0}CTCM 82-{1}", (OSI ? "" : "VJ "), Tel.Substring(1)); break;
                case "ZE": TelFormat = String.Format("{0}CTCM {1}", (OSI ? "" : "ZE "), Tel); break;
                case "7C": TelFormat = String.Format("-SEL {0}", Tel.Replace("-", " ")); break;
                default: TelFormat = Tel; break;
            }

            return TelFormat;
        }

        /// <summary>
        /// 예약시 항공사별 이메일 양식
        /// </summary>
        /// <param name="AirCode"></param>
        /// <param name="Email"></param>
        /// <param name="OSI">OSI 형식여부(항공사코드 삽입여부)(true:항공사코드 미삽입, false:항공사코드 삽입)</param>
        /// <returns></returns>
        public static string BookingEmailFormat(string AirCode, string Email, bool OSI)
        {
            string EmailFormat = string.Empty;

            switch (AirCode)
            {
                case "AA": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "AA"), Email.Replace("@", "//").ToUpper()); break;
                case "AC": EmailFormat = String.Format("CTCE{0}-{1}/P1", (OSI ? "" : "AA"), Email.Replace("@", "//").Replace("_", "..").Replace("-", "./").ToUpper()); break;
                case "AI": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "AI"), Email.Replace("@", "//").ToUpper()); break;
                case "AM": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "AM"), Email.Replace("@", "//").ToUpper()); break;
                case "BA": EmailFormat = String.Format("SK CTCE {0}-{1}", (OSI ? "" : "BA"), Email.ToUpper()); break;
                case "CA": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "CA"), Email.Replace("@", "//").ToUpper()); break;
                case "CI": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "CI"), Email.Replace("@", "//").ToUpper()); break;
                case "CX": EmailFormat = String.Format("{0} CTCE SEL {1}", (OSI ? "" : "CX"), Email.Replace("@", "//").ToUpper()); break;
                case "DL": EmailFormat = String.Format("{0} CTC {1}", (OSI ? "" : "DL"), Email.Replace("@", "//").ToUpper()); break;
                case "EK": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "EK"), Email.Replace("@", "//").ToUpper()); break;
                case "ET": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "ET"), Email.Replace("@", "//").ToUpper()); break;
                case "MU": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "MU"), Email.Replace("@", "//").Replace("_", "..").ToUpper()); break;
                case "HY": EmailFormat = String.Format("{0} CTC {1}", (OSI ? "" : "HY"), Email.Replace("@", "//").ToUpper()); break;
                case "ZE": EmailFormat = String.Format("{0} CTCE {1}", (OSI ? "" : "ZE"), Email.Replace("@", "//").ToUpper()); break;
                default: EmailFormat = Email; break;
            }

            return EmailFormat;
        }

		/// <summary>
		/// 예약상태코드
		/// </summary>
		/// <param name="code">예약상태코드</param>
		/// <returns></returns>
		public static string BookingStatusCode(string code)
		{
			string reStatus = string.Empty;

			switch (code.Trim().ToUpper())
			{
				case "HK":
				case "HS":
				case "PK":
                case "KK":
                case "RR":
				case "DK": reStatus = "HK"; break;
				case "TK": reStatus = "TK"; break;
                case "NO":
				case "UC": reStatus = "UC"; break;
                case "QQ": reStatus = "QQ"; break;
				case "XX":
				case "UN":
				case "PN":
                case "HX": reStatus = "XX"; break;
                case "FLWN": reStatus = "FL"; break;
				default: reStatus = "HL"; break;
			}

			return reStatus;
		}

		/// <summary>
		/// 예약상태값
		/// </summary>
		/// <param name="code">예약상태코드</param>
		/// <returns></returns>
		public static string BookingStatusText(string code)
		{
			string reStatus = string.Empty;

			switch (code.Trim())
			{
				case "HK": reStatus = "확약"; break;
				case "HL": reStatus = "대기"; break;
				case "TK": reStatus = "스케쥴변경"; break;
				case "UC": reStatus = "예약불가 - 재예약요망"; break;
				case "QQ": reStatus = "오픈"; break;
                case "FL": reStatus = "출발"; break;
				case "XX": reStatus = "취소"; break;
				default: reStatus = code.Trim(); break;
			}

			return reStatus;
		}

		/// <summary>
		/// 서비스클래스
		/// </summary>
		/// <param name="code">서비스클래스코드</param>
		/// <returns></returns>
		public static string ServiceClass(string code)
		{
            return code;// (code.Equals("M") || code.Equals("W")) ? "Y" : code;
		}

		/// <summary>
		/// 서비스클래스
        /// </summary>
        /// <param name="GDSType">서비스구분(A:아마데우스, S:세이버, G:갈릴레오, MPIS:아마데우스, FMS:세이버, BFM:세이버)</param>
		/// <param name="code">서비스클래스코드</param>
		/// <returns></returns>
        public static string ServiceClassText(string GDSType, string code)
        {
            string reClassName = string.Empty;

            if (GDSType.Equals("FMS"))
            {
                switch (code.Trim())
                {
                    case "Y": reClassName = "일반석"; break;
                    case "P": reClassName = "프리미엄이코노미석"; break;
                    case "C": reClassName = "비즈니스석"; break;
                    case "F": reClassName = "일등석"; break;
                    case "G": reClassName = "그룹"; break;
                    default: reClassName = code.Trim(); break;
                }
            }
            else if (GDSType.Equals("BFM"))
            {
                switch (code.Trim())
                {
                    case "Y": reClassName = "일반석"; break; //Economy
                    case "S": reClassName = "프리미엄이코노미석"; break; //PremiumEconomy
                    case "C": reClassName = "비즈니스석"; break; //Business
                    case "J": reClassName = "비즈니스석"; break; //PremiumBusiness
                    case "F": reClassName = "일등석"; break; //First
                    case "P": reClassName = "일등석"; break; //PremiumFirst
                    case "G": reClassName = "그룹"; break;
                    default: reClassName = code.Trim(); break;
                }
            }
            else
            {
                switch (code.Trim())
                {
                    case "Y": reClassName = "일반석"; break;
                    case "M": reClassName = "일반석"; break;
                    case "W": reClassName = "프리미엄이코노미석"; break;
                    case "C": reClassName = "비즈니스석"; break;
                    case "F": reClassName = "일등석"; break;
                    case "G": reClassName = "그룹"; break;
                    default: reClassName = code.Trim(); break;
                }
            }

            return reClassName;
        }

        /// <summary>
        /// 세이버 FMS 클래스를 BFM 클래스로 변경
        /// </summary>
        /// <param name="FMS">FMS여부</param>
        /// <param name="code">서비스클래스코드</param>
        /// <returns></returns>
        public static string ServiceClassFMSToBFM(bool FMS, string code)
        {
            string reClassCode = code.Trim();

            if (FMS)
            {
                switch (code.Trim())
                {
                    case "Y": reClassCode = "Y"; break;
                    case "P": reClassCode = "S"; break;
                    case "C": reClassCode = "C"; break;
                    case "F": reClassCode = "F"; break;
                    case "G": reClassCode = "G"; break;
                    default: reClassCode = code.Trim(); break;
                }
            }

            return reClassCode;
        }

		/// <summary>
		/// Amadeus 승객유형코드
		/// </summary>
		/// <param name="PTC">승객유형코드</param>
		/// <returns></returns>
		public static string AmadeusPaxTypeCode(string PTC)
		{
            return PTC.Equals("STU") ? "SD" : (PTC.Equals("LBR") ? "DL" : (PTC.Equals("IIT") ? "IT" : PTC));
		}

		/// <summary>
		/// 승객유형(요금조건)
		/// </summary>
		/// <param name="code">요금조건코드</param>
		/// <returns></returns>
		public static string PaxTypeText(string code)
		{
            string StrPaxType = string.Empty;

            switch (code)
            {
                case "ADT": StrPaxType = "성인"; break;
                case "DIS": StrPaxType = "장애인"; break;
                case "STU": StrPaxType = "학생"; break;
                case "SRC": StrPaxType = "경로"; break;
                case "LBR": StrPaxType = "근로자"; break;
                case "CHD": StrPaxType = "소아"; break;
                case "INF": StrPaxType = "유아"; break;
                default: StrPaxType = "성인"; break;
            }

            return StrPaxType;
		}

		/// <summary>
		/// 유효기간(최대체류기간)
		/// </summary>
		/// <param name="code">기간코드</param>
		/// <returns></returns>
		public string ExpiryDateText(string code)
		{
			string Gubun = Right(code, 1);
			return String.Concat(code.Replace(Gubun, ""), Gubun.Equals("Y") ? "년" : (Gubun.Equals("M") ? "개월" : "일"));
		}

		/// <summary>
		/// 증빙서류 항목 치환
		/// </summary>
        /// <param name="ProofText">증빙서류 내용</param>
        /// <param name="AirCode">항공사코드</param>
		/// <returns>증빙서류 내용</returns>
		public static string ReplaceProof(string ProofText, string AirCode)
		{
            //러시아항공(SU)일 경우 'MODETOUR' or 'MODE TOUR' 문자 포함일 경우 증빙서류 관련 텍스트 전체 삭제(2016-02-18,박주영차장)
            if (AirCode.Equals("SU") && (ProofText.IndexOf("MODETOUR") != -1 || ProofText.IndexOf("MODE TOUR") != -1))
                ProofText = "";
            //BA항공일 경우 'VALID FOR ADULT' 문장 포함시 규정 내용 전체 삭제(2016-12-06,김경미과장)
            else if (AirCode.Equals("BA") && ProofText.IndexOf("VALID FOR ADULT") != -1)
                ProofText = "";
            else
            {
                string[] Pattern1 = {
                                "NOTE - UNACCOMAPANIED INFANT - NOT ELIGIBLE.",
                                "NOTE - UNACCOMAPANIED INFANT - NOT ELIGIBLE",
								"NOTE - UNACCOMPANIED INFANT - NOT ELIGIBLE.",
								"NOTE - UNACCOMPANIED INFANT - NOT ELIGIBLE",
								"NOTE - UNACCOMPANIED INFANT NOT ELIGIBLE.",
                                "NOTE - UNACCOMPANIED INFANT NOT ELIGIBLE",
								"NOTE - UNACCOMPANIED INFANTS NOT PERMITTED.",
                                "NOTE - UNACCOMPANIED INFANTS NOT PERMITTED",
								"NOTE - UNACCOMPANIED CHILD 5-11 NOT APPLY.",
                                "NOTE - UNACCOMPANIED CHILD 5-11 NOT APPLY",
								"NOTE - CHILD WITHOUT ADULT ACCOMPANY NOT PERMITTED",
								"NOTE - VALID FOR GENERAL PASSENGER.",
								"NOTE - UNACCOMPANIED INFANT-NOT ELIGIBLE UNACCOMPANIED CHILD UNDER 5 YEARS-NOT ELIGIBLE",
								"NOTE - UNACCOMPANIED INFANT-NOT ELIGIBLE UNACCOMPANIED CHILD UNDER 5 YEARS-NOT ELIGIBLE UNACCOMPANIED CHILD 5-11 YEARS OLD-NOT ELIGIBLE",
								"NOTE - UNACCOMPANIED CHILDREN 2 - 7 YEARS NOT ELIGIBLE TO TRAVEL. UNACCOMPANIED INFANT- NOT ELIGIBLE TO TRAVEL.",
								"NOTE - UNACCOMPANIED CHILDREN 2-7 YEARS NOT ELIGIBLE TO TRAVEL. UNACCOMPANIED INFANT NOT ELIGIBLE TO TRAVEL.",
								"NOTE - FOR TRAVEL ON AFTER 01APR04 - UNACCOMPANIED INFANT NOT ELIGIBLE",
								"NOTE - NOT APPLICABLE FOR INFANT.",
								"NOTE - EXCEPTION-UNACCOMPANIED INFANT NOT ELIGIBLE.",
								"NOTE - A/ ELIGIBILITY - FARES NOT APPLICABLE FOR UNACCOMPANIED INFANT UNDER 2 YEARS AND UNACCOMPANIED CHILD 2-7 YEARS B/ DOCUMENTATION - PROOF OF AGE MUST BE PRESENTED AT TIME OF TICKETING AND MAY BE REQUESTED AT ANY TIME",
								"NONE UNLESS OTHERWISE SPECIFIED",
								"VALID FOR ADULT. NOTE - FARES NOT APPLICABLE FOR UNACCOMPANIED INFANT UNDER 2 YEARS AND UNACCOMPANIED CHILD 2-7 YEARS OR - FOR NATIONAL/CITIZEN OF A SPECIFIED COUNTRY. NOTE - ONLY VALID FOR INDONESIAN PASSPORT HOLDER",
								"VALID FOR ADULT. NOTE - VALID FOR GENERAL PAX",
								"FOR YFFKRS TYPE FARES . INFANT UNDER 2 WITHOUT A SEAT NOT APPLY.",
								"FOR FARES WITH FOOTNOTE 7H NOTE - GENERAL RULE DOES NOT APPLY NOTE - FARES VALID FOR TICKETING AT SPECIFIC TIER 1 AGENTS ONLY",
								"BETWEEN AREA 3 AND CANADA",
                                "FARE BY RULE NOTE - INDIVIDUALS IN CONJUNCTION WITH AIR ONLY TRAVEL.",
                                "FARE BY RULE VALID FOR ADULT",
                                "BASE FARE",
                                "UNACCOMPANIED CHILD UNDER 5 YEARS - NOT ELIGIBLE"
                                };

                string[] Pattern2 = {
                                @"BETWEEN KOREA, REPUBLIC OF AND AREA 1 FOR [A-Z]{3}AIRTL TYPE FARES VALID FOR ADULT. THIS FARE IS VALID FOR A SPECIFIED ACCOUNT CODE. CONTACT CARRIER FOR DETAILS. BETWEEN KOREA, REPUBLIC OF AND AREA 1 FOR [A-Z]{3}AIRTL TYPE FARES VALID FOR ADULT. THIS FARE IS VALID FOR A SPECIFIED ACCOUNT CODE. CONTACT CARRIER FOR DETAILS.",
								};

                ProofText = ProofText.Replace("\r\n", "").Replace("  ", " ");

                if (!String.IsNullOrEmpty(ProofText))
                {
                    foreach (string pattern in Pattern1)
                        ProofText = ProofText.Replace(pattern, "");

                    foreach (string pattern in Pattern2)
                        ProofText = new Regex(pattern, RegexOptions.Singleline).Replace(ProofText, "");
                }
            }

			return ProofText;
		}

		/// <summary>
		/// 요청 좌석 상태코드 전환
		/// </summary>
		/// <param name="code">숫자형태의 코드</param>
		/// <returns>영문형태의 코드</returns>
		public string Status(string code)
		{
			string reStatus = string.Empty;

			switch (code.Trim())
			{
				case "1": reStatus = "XX"; break; //Cancel/confirmed/requested
				case "2": reStatus = "XL"; break; //Cancel listing
				case "3": reStatus = "OX"; break; //Cancel only if requested segment is available
				case "4": reStatus = "XR"; break; //Cancellation recommended
				case "5": reStatus = "IX"; break; //If holding, cancel
				case "7": reStatus = "IN"; break; //If not holding, need
				case "8": reStatus = "IS"; break; //If not holding, sold
				case "9": reStatus = "LL"; break; //List (add to waiting list)
				case "10": reStatus = "SA"; break; //List space available
				case "11": reStatus = "NN"; break; //Need
				case "12": reStatus = "NA"; break; //Need segment specified or alternative
				case "13": reStatus = "SS"; break; //Sold
				case "14": reStatus = "FS"; break; //Sold (on free sale basis)
				case "15": reStatus = "SQ"; break; //Space requested
				case "16": reStatus = "HX"; break; //Cancelled
				case "17": reStatus = "KK"; break; //Confirming
				case "18": reStatus = "KL"; break; //Confirming from waitlist
				case "19": reStatus = "NO"; break; //No action taken
				case "20": reStatus = "UU"; break; //Unable - have waitlisted
				case "21": reStatus = "UN"; break; //Unable - flight does not operate
				case "22": reStatus = "US"; break; //Unable to accept sale
				case "23": reStatus = "UC"; break; //Waitlist closed
				case "24": reStatus = "HL"; break; //Have listed
				case "25": reStatus = "HN"; break; //Have requested
				case "26": reStatus = "HK"; break; //Holds confirmed
				case "27": reStatus = "RR"; break; //Reconfirmed
				case "28": reStatus = "HS"; break; //Have sold
				case "29": reStatus = "HQ"; break; //Space already requested
				case "30": reStatus = "RR"; break; //OK
				case "31": reStatus = "HX"; break; //Have cancelled
				case "32": reStatus = "PN"; break; //Pending confirmation
				case "33": reStatus = "UL"; break; //Deferred from wait list
				case "50": reStatus = "QQ"; break; //open
				case "51": reStatus = "QK"; break; //passive segment ok
				case "52": reStatus = "QL"; break; //passive segment waiting
				case "53": reStatus = "QN"; break; //예약이 되어 있지 않음
				default: reStatus = code.Trim(); break;
			}

			return reStatus;
		}

		/// <summary>
		/// 항공사명
		/// </summary>
		/// <param name="ULC">언어</param>
		/// <param name="AirCode">항공사코드</param>
		/// <returns></returns>
		public static string GetAirlineName(string ULC, string AirCode)
		{
			string AirlineName = AirCode;

			try
			{
                //using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //{
                //	SqlDataReader dr = null;

                //	using (SqlCommand cmd = new SqlCommand
                //	{
                //		Connection = conn,
                //		CommandTimeout = 10,
                //		CommandType = CommandType.StoredProcedure,
                //		CommandText = "DBO.WSV_S_항공사"
                //	})
                //	{
                //		cmd.Parameters.Add("@항공사코드", SqlDbType.Char, 2);
                //		cmd.Parameters["@항공사코드"].Value = AirCode;

                //		try
                //		{
                //			conn.Open();
                //			dr = cmd.ExecuteReader();

                //			if (dr.Read())
                //				AirlineName = (String.IsNullOrWhiteSpace(ULC) || ULC.Equals("KO")) ? dr["항공사명K"].ToString() : dr["항공사명E"].ToString().ToUpper();
                //		}
                //		catch (Exception ex)
                //		{
                //			new Exception(ex.ToString());
                //		}
                //		finally
                //		{
                //			dr.Dispose();
                //			dr.Close();
                //			conn.Close();
                //		}
                //	}
                //}

                return "GetAirlineName";

            }
			catch (Exception ex)
			{
                new MWSException(ex, HttpContext.Current, "Common", "GetAirlineName", 0, 0);
			}

			return AirlineName;
		}

        /// <summary>
        /// 항공 기종명
        /// </summary>
        /// <param name="EQT">기종코드</param>
        /// <returns></returns>
        public static string GetEquipmentName(string EQT)
        {
            string EquipmentName = EQT;

            try
            {
                //using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //{
                //    SqlDataReader dr = null;

                //    using (SqlCommand cmd = new SqlCommand
                //    {
                //        Connection = conn,
                //        CommandTimeout = 10,
                //        CommandType = CommandType.StoredProcedure,
                //        CommandText = "DBO.WSV_S_항공사_기종"
                //    })
                //    {
                //        cmd.Parameters.Add("@기종코드", SqlDbType.Char, 3);
                //        cmd.Parameters["@기종코드"].Value = EQT;

                //        try
                //        {
                //            conn.Open();
                //            dr = cmd.ExecuteReader();

                //            if (dr.Read())
                //                EquipmentName = dr["기종명"].ToString();
                //        }
                //        catch (Exception ex)
                //        {
                //            new Exception(ex.ToString());
                //        }
                //        finally
                //        {
                //            dr.Dispose();
                //            dr.Close();
                //            conn.Close();
                //        }
                //    }
                //}

                return "GetEquipmentName";
            }
            catch (Exception ex)
            {
                new MWSException(ex, HttpContext.Current, "Common", "GetEquipmentName", 0, 0);
            }

            return EquipmentName;
        }

		/// <summary>
		/// 공항명
		/// </summary>
		/// <param name="ULC">언어</param>
		/// <param name="AirportCode">공항코드</param>
		/// <returns></returns>
		public static string GetAirportName(string ULC, string AirportCode)
		{
			string AirportName = AirportCode;

			try
			{
                //using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //{
                //	SqlDataReader dr = null;

                //	using (SqlCommand cmd = new SqlCommand
                //	{
                //		Connection = conn,
                //		CommandTimeout = 10,
                //		CommandType = CommandType.StoredProcedure,
                //		CommandText = "DBO.WSV_S_공항정보"
                //	})
                //	{
                //		cmd.Parameters.Add("@공항코드", SqlDbType.Char, 3);
                //		cmd.Parameters["@공항코드"].Value = AirportCode;

                //		try
                //		{
                //			conn.Open();
                //			dr = cmd.ExecuteReader();

                //			if (dr.Read())
                //			{
                //				if (String.IsNullOrWhiteSpace(ULC) || ULC.Equals("KO"))
                //				{
                //					AirportName = String.Format("{0} / {1}", dr["CityKName"].ToString(), dr["AirportKName"].ToString());
                //				}
                //				else
                //				{
                //					AirportName = String.Format("{0} / {1}", dr["CityEName"].ToString(), dr["AirportEName"].ToString()).ToUpper();
                //				}
                //			}
                //		}
                //		catch (Exception ex)
                //		{
                //                        new Exception(ex.ToString());
                //		}
                //		finally
                //		{
                //			dr.Dispose();
                //			dr.Close();
                //			conn.Close();
                //		}
                //	}
                //}
                return "GetAirportName";

            }
			catch (Exception ex)
			{
                new MWSException(ex, HttpContext.Current, "Common", "GetAirportName", 0, 0);
			}

			return AirportName;
		}

        /// <summary>
        /// 공항정보
        /// </summary>
        /// <param name="ULC">언어</param>
        /// <param name="AirportCode">공항코드</param>
        /// <returns></returns>
        public static DataSet GetAirportInfo(string ULC, string AirportCode)
        {
            DataSet ds = null;

            try
            {
                //using (ds = new DataSet())
                //{
                //    using (SqlCommand cmd = new SqlCommand())
                //    {
                //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //        {
                //            SqlDataAdapter adp = new SqlDataAdapter(cmd);

                //            cmd.Connection = conn;
                //            cmd.CommandType = CommandType.StoredProcedure;
                //            cmd.CommandText = "DBO.WSV_S_공항정보";

                //            cmd.Parameters.Add("@공항코드", SqlDbType.Char, 3);
                //            cmd.Parameters["@공항코드"].Value = AirportCode;

                //            adp.Fill(ds);
                //            adp.Dispose();
                //        }
                //    }
                //}
                return null;
            }
            catch (Exception ex)
            {
                new MWSException(ex, HttpContext.Current, "Common", "GetAirportInfo", 0, 0);
            }

            return ds;
        }

        /// <summary>
        /// 도시/공항명 조회
        /// </summary>
        /// <param name="Kind">구분(A:공항만 조회, C:도시만 조회, 공백: 도시+공항 조회)</param>
        /// <param name="Code">도시/공항코드(하나 이상일 경우 콤마로 구분)</param>
        /// <returns></returns>
        public static DataSet GetCityAirportName(string Kind, string Code)
        {
            DataSet ds = null;

            try
            {
                //using (ds = new DataSet())
                //{
                //    using (SqlCommand cmd = new SqlCommand())
                //    {
                //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //        {
                //            SqlDataAdapter adp = new SqlDataAdapter(cmd);

                //            cmd.Connection = conn;
                //            cmd.CommandType = CommandType.StoredProcedure;
                //            cmd.CommandText = "DBO.WSV_S_항공도시명";

                //            cmd.Parameters.Add("@구분", SqlDbType.Char, 1);
                //            cmd.Parameters.Add("@도시코드", SqlDbType.NVarChar, 1000);

                //            cmd.Parameters["@구분"].Value = Kind;
                //            cmd.Parameters["@도시코드"].Value = Code;

                //            adp.Fill(ds);
                //            adp.Dispose();
                //        }
                //    }
                //}
                return null;
            }
            catch (Exception ex)
            {
                new MWSException(ex, HttpContext.Current, "Common", "GetCityAirportName", 0, 0);
            }

            return ds;
        }

        /// <summary>
        /// 항공사명 조회
        /// </summary>
        /// <param name="Code">항공사코드(하나 이상일 경우 콤마로 구분)</param>
        /// <returns></returns>
        public static DataSet GetAirlineName(string Code)
        {
            DataSet ds = null;

            try
            {
                //using (ds = new DataSet())
                //{
                //    using (SqlCommand cmd = new SqlCommand())
                //    {
                //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["NEWEAGLE"].ConnectionString))
                //        {
                //            SqlDataAdapter adp = new SqlDataAdapter(cmd);

                //            cmd.Connection = conn;
                //            cmd.CommandType = CommandType.StoredProcedure;
                //            cmd.CommandText = "DBO.WSV_S_항공사명";

                //            cmd.Parameters.Add("@항공사코드", SqlDbType.NVarChar, 1000);
                //            cmd.Parameters["@항공사코드"].Value = Code;

                //            adp.Fill(ds);
                //            adp.Dispose();
                //        }
                //    }
                //}
                return null;
            }
            catch (Exception ex)
            {
                new MWSException(ex, HttpContext.Current, "Common", "GetAirlineName", 0, 0);
            }

            return ds;
        }

        /// <summary>
        /// 여정정보를 통해 경유지 정보 구하기
        /// </summary>
        /// <param name="RTG">전체 여정 정보</param>
        /// <returns></returns>
        public static string ConnectionLocation(string RTG)
        {
            string CLC = string.Empty;
            string[] ArrRTG = RTG.Split('~');

            if (ArrRTG.Length > 1)
            {
                string[] RTGSeg1 = ArrRTG[0].Split('-');

                for (int i = 1; i < RTGSeg1.Length; i++)
                {
                    if (i > 1)
                        CLC += "/";

                    CLC += RTGSeg1[i];
                }

                if (ArrRTG.Length > 2)
                {
                    CLC += ",";

                    string[] RTGSeg2 = ArrRTG[2].Split('-');

                    for (int i = 0; i < ((ArrRTG.Length > 3) ? RTGSeg2.Length : (RTGSeg2.Length - 1)); i++)
                    {
                        if (i > 0)
                            CLC += "/";

                        CLC += RTGSeg2[i];
                    }
                }

                if (ArrRTG.Length > 4)
                {
                    CLC += ",";

                    string[] RTGSeg3 = ArrRTG[4].Split('-');

                    for (int i = 0; i < (RTGSeg3.Length - 1); i++)
                    {
                        if (i > 0)
                            CLC += "/";

                        CLC += RTGSeg3[i];
                    }
                }
            }

            return CLC;
        }

        /// <summary>
        /// 무료수화물 적용 단위
        /// </summary>
        /// <param name="UnitCode">적용 기준</param>
        /// <param name="UnitQualifier">적용 단위</param>
        /// <returns></returns>
        public static string BaggageUnitCode(string UnitCode, string UnitQualifier)
        {
            string UnitName = string.Empty;

            switch (UnitCode)
            {
                case "N": UnitName = "PC"; break;
                case "K":
                case "700": UnitName = "KG"; break;
                case "W": UnitName = (UnitQualifier.Equals("K") ? "KG" : UnitQualifier); break;
                default: UnitName = UnitCode; break;
            }

            return UnitName;
        }

		/// <summary>
		/// 파일의 존재여부
		/// </summary>
		/// <param name="FilePath">파일명이 포함된 전체 경로</param>
		/// <returns></returns>
		public bool FileExists(string FilePath)
		{
			return File.Exists(FilePath);
		}

		/// <summary>
		/// 폴더의 존재여부 확인 및 생성
		/// </summary>
		/// <param name="FolderPath">폴더 전체 경로</param>
		/// /// <param name="Create">폴더가 없을 경우 생성 여부</param>
		/// <returns></returns>
		public bool FolderExists(string FolderPath, bool Create)
		{
			bool reBoolean = Directory.Exists(FolderPath);

			if (!reBoolean && Create)
			{
				Directory.CreateDirectory(FolderPath);
				reBoolean = true;
			}

			return reBoolean;
		}

        /// <summary>
        /// Stream 파일 저장
        /// </summary>
        /// <param name="ReqInputStream">InputStream</param>
        /// <param name="GDS">구분</param>
        /// <param name="FileName">파일명(확장자제외)</param>
        /// <param name="GUID">고유값</param>
        public string StreamFileSave(Stream ReqInputStream, string GDS, string FileName, string GUID)
        {
            string StrInputStream;

            using (Stream receiveStream = ReqInputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    StrInputStream = readStream.ReadToEnd();
                }
            }

            return StringFileSave(StrInputStream, GDS, FileName, GUID);
        }

        /// <summary>
        /// String 파일 저장
        /// </summary>
        /// <param name="ReqInputString">InputStringInputString</param>
        /// <param name="GDS">구분</param>
        /// <param name="FileName">파일명(확장자제외)</param>
        /// <param name="GUID">고유값</param>
        /// <returns></returns>
        public string StringFileSave(string ReqInputString, string GDS, string FileName, string GUID)
        {
            if (String.IsNullOrWhiteSpace(GDS)) { GDS = "Temp"; }
            if (String.IsNullOrWhiteSpace(GUID)) { GUID = GetGUID; }

            DateTime NowDate = DateTime.Now;
            string FolderPath = String.Format("{0}WebServiceLog2\\{1}\\SaveFile\\{2}\\{3}\\{4}\\", PhysicalApplicationPath.Substring(0, 3), "AirWebService", GDS, NowDate.ToString("yyyyMM"), NowDate.ToString("dd"));
            string FileFullPath = String.Format("{0}{1}_{2}.txt", FolderPath, GUID, FileName);

            //폴더가 없을 경우 생성
            FolderExists(FolderPath, true);

            FileStream fs = new FileStream(FileFullPath, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            fs.Close();

            StreamWriter wr = new StreamWriter(FileFullPath, false, Encoding.Default);
            wr.WriteLine(ReqInputString);

            wr.Close();

            return ReqInputString;
        }

        /// <summary>
        /// XML 파일 저장
        /// </summary>
        /// <param name="Parameters">Parameters 정보</param>
        /// <param name="GDS">구분</param>
        /// <param name="FileName">파일명(확장자제외)</param>
        /// <param name="DBSave">로그 DB 저장 여부(Y:DB저장, N:DB저장하지 않음)</param>
        /// <param name="GUID">예약 고유값</param>
        public void XmlFileSave(string Parameters, string GDS, string FileName, string DBSave, string GUID)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(String.Format("<{0}><Parameters><![CDATA[{1}]]></Parameters></{0}>", GDS, Parameters));

            XmlFileSave(XmlDoc, GDS, FileName, DBSave, GUID);
        }

		/// <summary>
		/// XML 파일 저장
		/// </summary>
		/// <param name="XmlData">XmlElement</param>
		/// <param name="GDS">구분</param>
        /// <param name="FileName">파일명(확장자제외)</param>
        /// <param name="DBSave">로그 DB 저장 여부(Y:DB저장, N:DB저장하지 않음)</param>
		/// <param name="GUID">예약 고유값</param>
        public void XmlFileSave(XmlElement XmlData, string GDS, string FileName, string DBSave, string GUID)
		{
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(XmlData.OuterXml);

            XmlFileSave(XmlDoc, GDS, FileName, DBSave, GUID);
		}

		/// <summary>
		/// XML 파일 저장
		/// </summary>
		/// <param name="XmlDoc">XmlDocument</param>
		/// <param name="GDS">구분</param>
        /// <param name="FileName">파일명(확장자제외)</param>
        /// <param name="DBSave">로그 DB 저장 여부(Y:DB저장, N:DB저장하지 않음)</param>
		/// <param name="GUID">예약 고유값</param>
		public void XmlFileSave(XmlDocument XmlDoc, string GDS, string FileName, string DBSave, string GUID)
		{
			if (String.IsNullOrWhiteSpace(GDS)) { GDS = "Temp"; }
			if (String.IsNullOrWhiteSpace(GUID)) { GUID = GetGUID; }

			DateTime NowDate = DateTime.Now;
			string FolderPath = String.Format("{0}WebServiceLog2\\{1}\\SaveXml\\{2}\\{3}\\{4}\\", PhysicalApplicationPath.Substring(0, 3), "AirWebService", GDS, NowDate.ToString("yyyyMM"), NowDate.ToString("dd"));
            string FilePath = String.Format("{0}{1}_{2}_({3}).xml", FolderPath, GUID, FileName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

			//폴더가 없을 경우 생성
			FolderExists(FolderPath, true);

            //파일존재여부
            //if (FileExists(FilePath))
            //{
            //    FilePath = String.Format("{0}{1}_{2}.xml", FolderPath, GUID, String.Concat(FileName, " (2)"));

            //    if (FileExists(FilePath))
            //        FilePath = String.Format("{0}{1}_{2}.xml", FolderPath, GUID, String.Concat(FileName, " (3)"));
            //}

            XmlDoc.Save(FilePath);

			//DB에 저장(2018-02-13 로그 DB 저장시 속도 저하 및 락 등 문제 발생으로 일시 중단,고재영)
            //if (DBSave.Equals("Y"))
            //    XmlFileDBSave(GDS, GUID, FileName, "AirWebService", XmlDoc.DocumentElement);
		}

		/// <summary>
		/// XML 파일 내용 DB로 저장
		/// </summary>
		/// <param name="GDS">구분</param>
		/// <param name="GUID">예약 고유값</param>
		/// <param name="ServerName">파일명(확장자제외)</param>
		/// <param name="ServiceVersion">서비스구분</param>
		/// <param name="XmlData">파일내용</param>
		public void XmlFileDBSave(string GDS, string GUID, string ServerName, string ServiceVersion, XmlElement XmlData)
		{
			try
			{
				//using (SqlCommand cmd = new SqlCommand())
				//{
				//	using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SERVICELOG"].ConnectionString))
				//	{
				//		cmd.Connection = conn;
				//		cmd.CommandTimeout = 10;
				//		cmd.CommandType = CommandType.StoredProcedure;
				//		cmd.CommandText = "DBO.WSV_T_아이템예약_해외항공_로그";

				//		cmd.Parameters.Add("@GDS", SqlDbType.NVarChar, 10);
				//		cmd.Parameters.Add("@GUID", SqlDbType.NVarChar, 100);
				//		cmd.Parameters.Add("@서비스명", SqlDbType.NVarChar, 100);
				//		cmd.Parameters.Add("@서비스버전", SqlDbType.NVarChar, 20);
				//		cmd.Parameters.Add("@DATA", SqlDbType.Xml, -1);
				//		cmd.Parameters.Add("@결과", SqlDbType.Char, 1);
				//		cmd.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

				//		cmd.Parameters["@GDS"].Value = GDS;
				//		cmd.Parameters["@GUID"].Value = GUID;
				//		cmd.Parameters["@서비스명"].Value = ServerName;
				//		cmd.Parameters["@서비스버전"].Value = ServiceVersion;
				//		cmd.Parameters["@DATA"].Value = XmlData.OuterXml;
				//		cmd.Parameters["@결과"].Direction = ParameterDirection.Output;
				//		cmd.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

				//		try
				//		{
				//			conn.Open();
				//			cmd.ExecuteNonQuery();
				//		}
				//		finally
				//		{
				//			conn.Close();
				//		}
				//	}
				//}
			}
			catch (Exception) { }
        }

        /// <summary>
        /// ART 사용 가능 OID 여부
        /// </summary>
        /// <param name="OID"></param>
        /// <returns></returns>
        public static bool IsOIDforART(string OID)
        {
            bool Available = false;

            switch (OID.Trim())
            {
                case "SELK138NQ": Available = true; break; //오픈마켓(SkyScanner)
                case "SELK138NR": Available = true; break; //오픈마켓(Kayak)
                case "SELK138PK": Available = true; break; //오픈마켓(티몬)
                case "SELK138TG": Available = true; break; //오픈마켓(삼성카드여행)
                case "SELK138FY": Available = true; break; //온라인(B2B)
                case "SELK138FZ": Available = true; break; //온라인
                case "SELK138AB": Available = true; break; //온라인(모두닷컴웹)
                case "SELK138HB": Available = true; break; //온라인
                case "SELK138HS": Available = true; break; //온라인(모두닷컴모바일)
                case "SELK138IB": Available = true; break; //오픈마켓(트래블하우)
                case "SELK138LO": Available = true; break; //오픈마켓(11번가)
                case "SELK138EN": Available = true; break; //TEST용
            }

            return Available;
        }

        /// <summary>
        /// 영문규정 타이틀 변경
        /// </summary>
        /// <param name="CategoryName">Category 명</param>
        /// <returns></returns>
        public static string SetCategoryTitle2(string CategoryName)
        {
            string CategoryTitle = string.Empty;

            switch (CategoryName.Substring(0, 2))
            {
                case "AP": CategoryTitle = "사전발권"; break;
                case "BO": CategoryTitle = "BLACKOUTS"; break;
                case "CD": CategoryTitle = "소아/유아요금"; break;
                case "DA": CategoryTitle = "출국/귀국요일"; break;
                case "EL": CategoryTitle = "증빙서류"; break;
                case "FL": CategoryTitle = "편명"; break;
                case "MX": CategoryTitle = "최대체류일"; break;
                case "MN": CategoryTitle = "최소체류일"; break;
                case "PE": CategoryTitle = "환불/변경 수수료"; break;
                case "SR": CategoryTitle = "판매 제한"; break;
                case "SE": CategoryTitle = "시즌"; break;
                case "SO": CategoryTitle = "경유지 체류"; break;
                case "SU": CategoryTitle = "SURCHARGES"; break;
                case "TR": CategoryTitle = "여행 제한"; break;
                default: CategoryTitle = CategoryName.Split('.')[1]; break;
            }

            return CategoryTitle;
        }

        /// <summary>
        /// 증빙서류 추가 텍스트
        /// </summary>
        /// <param name="PTC">운임타입코드</param>
        /// <returns></returns>
        public static string AppendELRuleText(string PTC)
        {
            string RuleText = string.Empty;

            switch (PTC)
            {
                case "STU": RuleText = "학생운임 : 학생비자(F1/M1), 입학허가서(항공사별로 상의할 수 있음)<BR><BR>필요 증빙은 항공사에 따라 상이할 수 있으며, 서류 미 충족시 구매불가.<BR>자세한 내용은 예약 후 담당자를 통해 재확인 바랍니다."; break;
                case "LBR": RuleText = "노무자운임(본국으로 여행하시는 외국인 근로자운임) : 여권사본증빙 필수"; break;
                default: RuleText = ""; break;
            }

            return RuleText;
        }

        /// <summary>
        /// 미니룰 타이틀 변경
        /// </summary>
        /// <param name="CategoryNumber">Category 번호</param>
        /// <returns></returns>
        public static string SetCategoryTitle1(string CategoryNumber)
        {
            string CategoryTitle = string.Empty;

            switch (CategoryNumber)
            {
                case "5": CategoryTitle = "사전발권"; break;
                case "6": CategoryTitle = "최소체류일"; break;
                case "7": CategoryTitle = "최대체류일"; break;
                case "31": CategoryTitle = "날짜변경/재발행"; break;
                case "33": CategoryTitle = "환불규정"; break;
                default: CategoryTitle = CategoryNumber; break;
            }

            return CategoryTitle;
        }

        /// <summary>
        /// 미니룰 배열 인덱스
        /// </summary>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="Abbreviation">약어</param>
        /// <returns></returns>
        public static int MiniRuleArrIndex(string CategoryNumber, string Abbreviation)
        {
            int Index = 0;

            //Advance Reservation/Ticketing (AP) (사전발권)
            if (CategoryNumber.Equals("5"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Index = 0; break;
                    case "ERD": Index = 1; break;
                    case "LRD": Index = 2; break;
                    case "ETD": Index = 3; break;
                    case "LTD": Index = 4; break;
                    case "LTR": Index = 5; break;
                }
            }
            //Minimum Stay (최소체류일)
            else if (CategoryNumber.Equals("6"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Index = 0; break;
                    case "MIS": Index = 1; break;
                }
            }
            //Maximum Stay (최대체류일)
            else if (CategoryNumber.Equals("7"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Index = 0; break;
                    case "MSP": Index = 1; break;
                    case "MSC": Index = 2; break;
                }
            }
            //Revalidation/Reissue (날짜변경/재발행)
            else if (CategoryNumber.Equals("31"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Index = 0; break;
                    case "BDV": Index = 1; break;
                    case "RVA": Index = 2; break;
                    case "BDA": Index = 3; break;
                    case "BNV": Index = 4; break;
                    case "ADV": Index = 5; break;
                    case "ANV": Index = 6; break;
                    case "ADR": Index = 7; break;
                    case "ADA": Index = 8; break;
                    case "WAI": Index = 9; break;
                    case "BDM":
                    case "BDX":
                    case "BDF":
                    case "BDG":
                    case "BDT": Index = 10; break;
                    case "BDI":
                    case "BDU":
                    case "BDH":
                    case "BDL":
                    case "BDC": Index = 11; break;
                    case "BNW": Index = 12; break;
                    case "BNR": Index = 13; break;
                    case "BNA": Index = 14; break;
                    case "BNM":
                    case "BNX":
                    case "BNF":
                    case "BNG":
                    case "BNT": Index = 15; break;
                    case "BNI":
                    case "BNU":
                    case "BNH":
                    case "BNL":
                    case "BNC": Index = 16; break;
                    case "ADW": Index = 17; break;
                    case "ADM":
                    case "ADX":
                    case "ADF":
                    case "ADG":
                    case "ADT": Index = 18; break;
                    case "ADI":
                    case "ADU":
                    case "ADH":
                    case "ADL":
                    case "ADC": Index = 19; break;
                    case "ANW": Index = 20; break;
                    case "ANR": Index = 21; break;
                    case "ANA": Index = 22; break;
                    case "ANM":
                    case "ANX":
                    case "ANF":
                    case "ANG":
                    case "ANT": Index = 23; break;
                    case "ANI":
                    case "ANU":
                    case "ANH":
                    case "ANL":
                    case "ANC": Index = 24; break;
                    case "FFT": Index = 25; break;
                }
            }
            //Refund (환불규정)
            else if (CategoryNumber.Equals("33"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Index = 0; break;
                    case "BDV": Index = 1; break;
                    case "BNV": Index = 2; break;
                    case "ADV": Index = 3; break;
                    case "ANV": Index = 4; break;
                    case "BDA": Index = 5; break;
                    case "BDM":
                    case "BDF":
                    case "BDX":
                    case "BDG":
                    case "BDT": Index = 6; break;
                    case "BNA": Index = 7; break;
                    case "BNM":
                    case "BNF":
                    case "BNX":
                    case "BNG":
                    case "BNT": Index = 8; break;
                    case "ADA": Index = 9; break;
                    case "ADM":
                    case "ADF":
                    case "ADX":
                    case "ADG":
                    case "ADT": Index = 10; break;
                    case "ANA": Index = 11; break;
                    case "ANM":
                    case "ANF":
                    case "ANX":
                    case "ANG":
                    case "ANT": Index = 12; break;
                    case "FFT": Index = 13; break;
                }
            }
            
            return Index;
        }
        
        /// <summary>
        /// 미니룰 약어 설명
        /// </summary>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="Abbreviation">약어</param>
        /// <returns></returns>
        public static string MiniRuleLegend(string CategoryNumber, string Abbreviation)
        {
            string Legend = string.Empty;
            
            //Advance Reservation/Ticketing (AP) (사전발권)
            if (CategoryNumber.Equals("5"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Legend = "귀국일 미지정 허용"; break;
                    case "ERD": Legend = "출발전 예약가능한 가장 빠른 날짜"; break;
                    case "LRD": Legend = "출발전 예약가능한 가장 늦은 날짜"; break;
                    case "ETD": Legend = "출발전 발권가능한 가장 빠른 날짜"; break;
                    case "LTD": Legend = "출발전 발권가능한 가장 늦은 날짜"; break;
                    case "LTR": Legend = "예약후 발권가능한 가장 늦은 날짜"; break;
                    default: Legend = Abbreviation; break;
                }
            }
            //Minimum Stay (최소체류일)
            else if (CategoryNumber.Equals("6"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Legend = "최소제류일 없음"; break;
                    case "MIS": Legend = "최소체류일"; break;
                    default: Legend = Abbreviation; break;
                }
            }
            //Maximum Stay (최대체류일)
            else if (CategoryNumber.Equals("7"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Legend = "최대체류일 없음"; break;
                    case "MSP": Legend = "여행종료일"; break;
                    case "MSC": Legend = "여행시작일"; break;
                    default: Legend = Abbreviation; break;
                }
            }
            //Revalidation/Reissue (날짜변경/재발행)
            else if (CategoryNumber.Equals("31"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Legend = "허용 안함"; break;
                    case "BDV": Legend = "출발전 유효기간"; break;
                    case "RVA": Legend = "출발전 단순 날짜변경 허용"; break;
                    case "BDA": Legend = "출발전 재발행 허용"; break;
                    case "BNV": Legend = "출발전 노쇼(사전 예약취소 안함)인 경우 티켓사용 허용"; break;
                    case "ADV": Legend = "출발후 유효기간"; break;
                    case "ANV": Legend = "출발후 노쇼(사전 예약취소 안함)인 경우 티켓사용 허용"; break;
                    case "ADR": Legend = "출발후 날짜변경 허용"; break;
                    case "ADA": Legend = "출발후 재발행 가능"; break;
                    case "WAI": Legend = "가족 사망으로 인한 패널티 면제"; break;
                    case "BDM": Legend = "재발행 최소 패널티 금액"; break;
                    case "BDX": Legend = "재발행 최대 패널티 금액"; break;
                    case "BDF": Legend = "재발행 최소 패널티 금액"; break;
                    case "BDG": Legend = "재발행 최대 패널티 금액"; break;
                    case "BDT": Legend = "재발행 최대 패널티 금액"; break;
                    case "BDI": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "BDU": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BDH": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "BDL": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BDC": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BNW": Legend = "가족 사망으로 인한 패널티 면제"; break;
                    case "BNR": Legend = "출발전 노쇼(사전 예약취소 안함)인 경우 변경 허용"; break;
                    case "BNA": Legend = "출발전 노쇼(사전 예약취소 안함)인 경우 재발행 허용"; break;
                    case "BNM": Legend = "재발행 최소 패널티 금액"; break;
                    case "BNX": Legend = "재발행 최대 패널티 금액"; break;
                    case "BNF": Legend = "재발행 최소 패널티 금액"; break;
                    case "BNG": Legend = "재발행 최대 패널티 금액"; break;
                    case "BNT": Legend = "재발행 최대 패널티 금액"; break;
                    case "BNI": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "BNU": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BNH": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BNL": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "BNC": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ADW": Legend = "가족 사망으로 인한 패널티 면제"; break;
                    case "ADM": Legend = "재발행 최소 패널티 금액"; break;
                    case "ADX": Legend = "재발행 최대 패널티 금액"; break;
                    case "ADF": Legend = "재발행 최소 패널티 금액"; break;
                    case "ADG": Legend = "재발행 최대 패널티 금액"; break;
                    case "ADT": Legend = "재발행 최대 패널티 금액"; break;
                    case "ADI": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "ADU": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ADH": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "ADL": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ADC": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ANW": Legend = "가족 사망으로 인한 패널티 면제"; break;
                    case "ANR": Legend = "출발후 노쇼(사전 예약취소 안함)인 경우 변경 허용"; break;
                    case "ANA": Legend = "출발후 노쇼(사전 예약취소 안함)인 경우 재발행 허용"; break;
                    case "ANM": Legend = "재발행 최소 패널티 금액"; break;
                    case "ANX": Legend = "재발행 최대 패널티 금액"; break;
                    case "ANF": Legend = "재발행 최소 패널티 금액"; break;
                    case "ANG": Legend = "재발행 최소 패널티 금액"; break;
                    case "ANT": Legend = "재발행 최대 패널티 금액"; break;
                    case "ANI": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "ANU": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ANH": Legend = "단순 날짜변경 최소 패널티 금액"; break;
                    case "ANL": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    case "ANC": Legend = "단순 날짜변경 최대 패널티 금액"; break;
                    default: Legend = Abbreviation; break;
                }
            }
            //Refund (환불규정)
            else if (CategoryNumber.Equals("33"))
            {
                switch (Abbreviation)
                {
                    case "ASS": Legend = "수수료 없이 환불 가능"; break;
                    case "BDV": Legend = "출발전 유효기간"; break;
                    case "BNV": Legend = "출발전 노쇼(사전 예약취소 안함)인 경우 티켓사용 허용"; break;
                    case "ADV": Legend = "출발후 유효기간"; break;
                    case "ANV": Legend = "출발후 노쇼(사전 예약취소 안함)인 경우 티켓사용 허용"; break;
                    case "BDA": Legend = "출발전 환불 허용"; break;
                    case "BDM": Legend = "최소 패널티 금액"; break;
                    case "BDX": Legend = "출발전 최대 패널티 금액"; break;
                    case "BDF": Legend = "최소 패널티 금액"; break;
                    case "BDG": Legend = "최대 패널티 금액"; break;
                    case "BDT": Legend = "최대 패널티 금액"; break;
                    case "BNA": Legend = "출발전 환불 허용"; break;
                    case "BNM": Legend = "최소 패널티 금액"; break;
                    case "BNX": Legend = "최대 패널티 금액"; break;
                    case "BNF": Legend = "최소 패널티 금액"; break;
                    case "BNG": Legend = "최대 패널티 금액"; break;
                    case "BNT": Legend = "출발전 노쇼(사전 예약취소 안함)인 경우 최대 패널티 금액"; break;
                    case "ADA": Legend = "출발후 환불가능"; break;
                    case "ADM": Legend = "최소 패널티 금액"; break;
                    case "ADX": Legend = "최대 패널티 금액"; break;
                    case "ADF": Legend = "최소 패널티 금액"; break;
                    case "ADG": Legend = "최대 패널티 금액"; break;
                    case "ADT": Legend = "최대 패널티 금액"; break;
                    case "ANA": Legend = "출발후 환불 허용"; break;
                    case "ANM": Legend = "출발후 최소 패널티 금액"; break;
                    case "ANX": Legend = "출발후 최대 패널티 금액"; break;
                    case "ANF": Legend = "최소 패널티 금액"; break;
                    case "ANG": Legend = "최대 패널티 금액"; break;
                    case "ANT": Legend = "최대 패널티 금액"; break;
                    default: Legend = Abbreviation; break;
                }
            }

            return Legend;
        }

        /// <summary>
        /// 미니룰 약어별 VALUE
        /// </summary>
        /// <param name="CategoryNumber">카테고리번호</param>
        /// <param name="Abbreviation">약어</param>
        /// <param name="Flag">구분코드</param>
        /// <returns></returns>
        public static string MiniRuleLegendValue(string CategoryNumber, string Abbreviation, string Flag)
        {
            string LegendValue = Flag;

            //Revalidation/Reissue (날짜변경/재발행)
            if (CategoryNumber.Equals("31"))
            {
                switch (Abbreviation)
                {
                    case "FFT": LegendValue = Flag.Equals("0") ? "No free form text from Cat16" : (Flag.Equals("1") ? "Part of the rule is free form text from Cat16" : "No free form text"); break;
                    case "RVA": LegendValue = Flag.Equals("0") ? "Reval not allowed" : (Flag.Equals("1") ? "Reval Allowed with restrictions" : "Not Allowed"); break;
                    case "BDA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                    case "ADA": LegendValue = Flag.Equals("0") ? "" : (Flag.Equals("1") ? "" : ""); break;
                    case "WAI": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "No waiver" : Flag; break;
                    case "BNW": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "No waiver" : Flag; break;
                    case "BNR": LegendValue = Flag.Equals("0") ? "Reval not allowed" : (Flag.Equals("1") ? "Reval Allowed with restrictions" : "Not Allowed"); break;
                    case "BNA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                    case "ADW": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "No waiver" : Flag; break;
                    case "ADR": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "Not Allowed" : Flag; break;
                    case "ANW": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "No waiver" : Flag; break;
                    case "ANR": LegendValue = Flag.Equals("0") ? "Reval not allowed" : (Flag.Equals("1") ? "Reval Allowed with restrictions" : "Not Allowed"); break;
                    case "ANA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                }
            }
            //Refund (환불규정)
            else if (CategoryNumber.Equals("33"))
            {
                switch (Abbreviation)
                {
                    case "FFT": LegendValue = Flag.Equals("0") ? "No free form text from Cat16" : (Flag.Equals("1") ? "Part of the rule is free form text from Cat16" : "No free form text"); break;
                    case "BDA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                    case "BNA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                    case "ADA": LegendValue = Flag.Equals("0") ? "Not allowed" : (Flag.Equals("1") ? "Allowed with restrictions" : "Not Allowed"); break;
                    case "ANA": LegendValue = String.IsNullOrWhiteSpace(Flag) ? "Not Allowed" : Flag; break;
                }
            }

            return LegendValue;
        }

        /// <summary>
        /// 미니룰 패널티 금액 계산
        /// </summary>
        /// <param name="Penalty">미니룰 패널티 금액</param>
        /// <param name="AirCode">항공사코드</param>
        /// <param name="RoundTrip">왕복 여부</param>
        /// <param name="DTD">출발일(YYYYMMDD)</param>
        /// <returns></returns>
        public static double MiniRulePenalty(double Penalty, string AirCode, bool RoundTrip, string DTD)
        {
            Common cm = new Common();

            //BA(영국항공) 미니룰 출력시 왕복인 경우 패널티금액 *2 처리(패널티금액이 편도 기준으로 넘어옴)(2016-08-11,김지영과장)
            //7C(제주항공) 미니룰 출력시 왕복인 경우 패널티금액 *2 처리(패널티금액이 편도 기준으로 넘어옴)(2017-05-10,김지영차장)
            //TW(티웨이항공) 미니룰 출력시 왕복인 경우 패널티금액 *2 처리(패널티금액이 편도 기준으로 넘어옴)(2017-10-24,김지영차장)
            if (RoundTrip && (AirCode.Equals("BA") || AirCode.Equals("7C") || AirCode.Equals("TW")))
                return (Penalty * 2);
            //LJ(진에어) 미니룰 출력시 왕복이면서 2018/03/25 출발편부터 패널티금액 *2 처리(2018-01-19,김경미차장)
            else if (RoundTrip && AirCode.Equals("LJ") && cm.DateDiff("d", "2018-03-25", cm.RequestDateTime(DTD, "yyyy-MM-dd")) >= 0)
                return (Penalty * 2);
            else
                return Penalty;
        }

        /// <summary>
        /// 대표 전화번호(PNR 생성시 대표번호 셋팅)
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <param name="ANM">거래처번호</param>
        /// <returns></returns>
        public static string TelInfo(int SNM, int ANM)
        {
            string AP = string.Empty;

            //닷컴 예약시 모두닷컴(항공_VIP) 거래처는 연락처를 "02-2049-3484" 등록(2016-08-23,김승미차장)
            //닷컴 연락처 변경(1544-5252 => 1544-5353)(2017-02-14,김경미과장)
            //11번가 연락처 추가(2017-03-14,정성하과장)
            //티몬 연락처 추가(2018-09-06,김경미매니저)
            switch (SNM)
            {
                case 2:
                case 3915: AP = String.Format("{0} MODETOUR NETWORK CENTER", (ANM.Equals(2338487) ? "02-2049-3484" : "1544-5353")); break;
                case 4657: AP = "02-755-3700 MODETOUR RESERVATION CENTER(HOMEPLUS)"; break;
                case 4638: AP = "02-862-7125 MODETOUR RESERVATION CENTER(NAVER)"; break;
                case 4664:
                case 4837: AP = "1661-6284 MODETOUR RESERVATION CENTER(SKYCANNER)"; break;
                case 4578:
                case 4737: AP = "1688-8200 MODETOUR RESERVATION CENTER(SAMSUNGCARD TRAVEL)"; break;
                case 4547: AP = "1688-8200 MODETOUR RESERVATION CENTER(SAMSUNGFAMILY)"; break;
                case 4681:
                case 4907: AP = "1661-9271 MODETOUR RESERVATION CENTER(WE MAKE PRICE)"; break;
                case 4716: AP = "1661-4637 MODETOUR RESERVATION CENTER(KAKAO)"; break;
                case 4713:
                case 4820: AP = "1661-4217 MODETOUR RESERVATION CENTER(KAYAK)"; break;
                case 4715: AP = "1661-8576 MODETOUR RESERVATION CENTER(SHINHAN CARD)"; break;
                case 4924:
                case 4929: AP = "1644-7490 MODETOUR RESERVATION CENTER(11ST)"; break;
                case 4925:
                case 4926: AP = "1544-5353 MODETOUR NETWORK"; break;
                default: AP = "02-2049-3200 MODETOUR NETWORK"; break;
            }

            return AP;
        }

        /// <summary>
        /// 이지페이 결제용 신용카드 코드정보
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public static string EasyPayCardCode(string CardCode)
        {
            string EasypayCode = string.Empty;

            switch (CardCode.Trim())
            {
                case "신한":
                case "신한카드":
                case "SH":
                case "LG": EasypayCode = "029"; break;
                case "현대":
                case "현대카드":
                case "HD":
                case "DI":EasypayCode = "027"; break;
                case "삼성":
                case "삼성카드":
                case "SW":
                case "AX":
                case "SS": EasypayCode = "031"; break;
                case "외환":
                case "외환카드":
                case "EX":
                case "KE": EasypayCode = "008"; break;
                case "비씨":
                case "비씨카드":
                case "BC": EasypayCode = "026"; break;
                case "국민":
                case "국민카드":
                case "KEB국민":
                case "KM":
                case "CN": EasypayCode = "016"; break;
                case "롯데":
                case "롯데(아멕스)카드":
                case "롯데카드":
                case "아멕스카드":
                case "LC":
                case "AM":
                case "LO": EasypayCode = "047"; break;
                case "(신)롯데": EasypayCode = "045"; break;
                case "NH농협":
                case "농협":
                case "농협카드":
                case "NH": EasypayCode = "018"; break;
                case "하나SK":
                case "하나 SK":
                case "하나카드":
                case "하나SK카드":
                case "SK":
                case "HN": EasypayCode = "006"; break;
                case "씨티":
                case "씨티카드":
                case "시티카드":
                case "CT": EasypayCode = "022"; break;
                case "우리":
                case "우리카드":
                case "PH": EasypayCode = "021"; break;
                case "광주":
                case "광주카드":
                case "KJ": EasypayCode = "002"; break;
                case "수협":
                case "수협카드":
                case "SU": EasypayCode = "017"; break;
                case "전북":
                case "전북카드":
                case "JB": EasypayCode = "010"; break;
                case "제주":
                case "제주카드":
                case "CJ": EasypayCode = "011"; break;
                case "조흥": EasypayCode = "001"; break;
                case "산업":
                case "산업카드":
                case "KD": EasypayCode = "058"; break;
                case "해외비자":
                case "해외VISA":
                case "해외 VISA": EasypayCode = "050"; break;
                case "해외JCB":
                case "해외 JCB":
                case "JCB": EasypayCode = "028"; break;
                case "동양 다이너스":
                case "다이너스":
                case "다이너스카드":
                case "현대다이너스":
                case "DC": EasypayCode = "048"; break;
                case "동양 해외": EasypayCode = "046"; break;
                case "해외마스터":
                case "해외MASTER":
                case "해외 MASTER": EasypayCode = "049"; break;
                case "은련": EasypayCode = "081"; break;
                case "신세계한미":
                case "SG": EasypayCode = ""; break;
                case "신협카드":
                case "CU": EasypayCode = ""; break;
                case "저축카드":
                case "SB": EasypayCode = ""; break;
                case "한미카드":
                case "HM": EasypayCode = ""; break;
            }

            return EasypayCode;
        }

        /// <summary>
        /// 규정 호출시 환불관련 통합으로 계산되어야 하는 항공사여부
        /// </summary>
        /// <param name="AirList">운임항공사 리스트</param>
        /// <returns></returns>
        public static bool RefundSumAir(string AirList)
        {
            bool SumAir = false;
            string BaseAir = "CZ/AC";
            
            foreach (string Air in BaseAir.Split('/'))
            {
                if (AirList.IndexOf(Air) != -1)
                {
                    SumAir = true;
                    break;
                }
            }

            return SumAir;
        }

        /// <summary>
        /// 발권시 오류체크(DocIssuance_IssueTicket 요청 시 오류 발생 한 경우)
        /// </summary>
        /// <param name="IssueMsg">발권완료 메시지</param>
        /// <returns></returns>
        public static bool CheckIssueError(string IssueMsg)
        {
            string[] IssueText = new String[8] { "ERRORCODE-TPSN", "ERRORCODETXXX", "IGNOREANDRE-ENTER", "IGNOREANDREENTER", "CONTACTHELPDESK", "SIMULTANEOUSCHANGESTOPNR", "UNABLETOPROCESS", "ALREADYTICKETED" };
            string IssueStr = IssueMsg.Replace(" ", "");
            bool Error = false;

            foreach (string Msg in IssueText)
            {
                if (IssueStr.IndexOf(Msg) != -1)
                {
                    Error = true;
                    break;
                }
            }

            return Error;
        }

        /// <summary>
        /// 근사값 구하기
        /// </summary>
        /// <param name="inData"></param>
        /// <param name="targetData">기준값</param>
        /// <returns></returns>
        public static int NearValue(string inData, int targetData)
        {
            int NearValue = 0;
            int k = Int32.MaxValue;

            foreach (string orgData in inData.Split(','))
            {
                if (!String.IsNullOrWhiteSpace(orgData))
                {
                    int intData = Convert.ToInt32(orgData);
                    int intDiff = 0;

                    intDiff = targetData - intData; //목표값과의 차이 값
                    intDiff = (intDiff < 0) ? -intDiff : intDiff; //intDiff값이 0보다 작을 경우, 즉 음수일 경우 -intDiff

                    if (intDiff < k) //목표값의 절대값의 최소값
                    {
                        NearValue = intData;
                    }
                    k = intDiff;
                }
            }

            return NearValue;
        }

        /// <summary>
        /// 네이버(갈리레오용 복호화)
        /// </summary>
        /// <param name="compressedStr"></param>
        /// <returns></returns>
        public string GalileoDeCompression(string compressedStr)
        {
            if (String.IsNullOrWhiteSpace(compressedStr))
                return compressedStr;
            else
            {
                compressedStr = compressedStr.Replace("^", "+").Replace("@", "#");

                string output = null;
                byte[] cmpData = Convert.FromBase64String(compressedStr);
                using (var decomStream = new MemoryStream(cmpData))
                {
                    using (var hgs = new System.IO.Compression.GZipStream(decomStream, CompressionMode.Decompress))
                    {
                        //decomStream에 압축 헤제된 데이타를 저장한다.
                        using (var reader = new StreamReader(hgs))
                        {
                            output = reader.ReadToEnd();
                        }
                    }
                }

                return output;
            }
        }
	}
}