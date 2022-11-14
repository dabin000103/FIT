using System;
using System.Web;
using System.Xml;

namespace AirWebService
{
    public class SabreConfig
    {
        AirConfig ac = new AirConfig();

        /// <summary>
        /// GDS명
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get { return "Sabre"; }
        }

        /// <summary>
        /// Sabre용 XML 파일의 로컬 폴더 경로
        /// </summary>
        private string XmlPath
        {
            get { return String.Format(@"{0}Sabre\", ac.XmlPhysicalPath); }
        }

        /// <summary>
        /// Sabre용 XML 파일의 로컬 경로
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public string XmlFullPath(string ServiceName)
        {
            return String.Format("{0}{1}.xml", XmlPath, ServiceName);
        }

        /// <summary>
        /// Sabre 서버 URL
        /// </summary>
        /// <param name="Gubun"></param>
        /// <returns></returns>
        public static string ServiceDomain(string Gubun)
        {
            string ServiceUrl = string.Empty;

            switch (Gubun)
            {
                case "api": ServiceUrl = "http://sabre.modetour.com/cgi-bin/"; break;
                case "devapi": ServiceUrl = "http://172.30.52.123/cgi-bin/"; break;
                case "bfmapi": ServiceUrl = "http://sabre.modetour.com/api/bfm/"; break;
                case "devbfmapi": ServiceUrl = "http://172.30.52.123/api/bfm/"; break;
                case "devbfmapi2": ServiceUrl = "http://metasearch-dev.asianasabre.co.kr/api/bfm/"; break;
            }

            return ServiceUrl;
        }

        /// <summary>
        /// Sabre 호출 서비스 URL
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static string ServiceURL(string ServiceName)
        {
            string ServiceDomainGubun = string.Empty;
            string ServiceFileUrl = string.Empty;
            
            switch (ServiceName)
            {
                case "SearchFareAvailFMS": //운임_스케쥴 조회(FMS)
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "skd_fare7.cgi";
                    break;
                case "SearchFareAvail": //운임_스케쥴 조회(BFM)
                    ServiceDomainGubun = "bfmapi";
                    ServiceFileUrl = "realTimeSearchLC.do";
                    break;
                case "FareRule": //운임규정 조회(FMS용)
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "if_fare_rule.cgi";
                    break;
                case "BFMFareRule": //운임규정 조회(BFM용)
                    ServiceDomainGubun = "bfmapi";
                    ServiceFileUrl = "findFareRule.do";
                    break;
                case "SegHold": //예약 전처리
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "prs.cgi";
                    break;
                case "AirBook": //예약
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "prs.cgi";
                    break;
                case "RevailDate": //유효성체크(BFM용)
                    ServiceDomainGubun = "bfmapi";
                    ServiceFileUrl = "revaildate.do";
                    break;
                default:
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "skd_fare7.cgi";
                    break;
            }

            return String.Concat(ServiceDomain(ServiceDomainGubun), ServiceFileUrl);
        }

        /// <summary>
        /// 파라미터 설정
        /// </summary>
        /// <param name="URL">호출 URL</param>
        /// <param name="PushData">파라미터 값</param>
        /// <returns></returns>
        private string SabreRequestInfo(string URL, string PushData)
        {
            return String.Format("{0}?fareRec={1}", URL, HttpUtility.UrlEncode(PushData));
        }

        /// <summary>
        /// Sabre Request and Response(HttpWebRequest)
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public XmlElement HttpExecute(string ServiceName, string Parameters)
        {
            if (ServiceName.Equals("AirBook"))
                return XmlRequest.GetPostSendToXml2(ServiceURL(ServiceName), Parameters);
            else if (ServiceName.Equals("BFMFareRule") || ServiceName.Equals("RevailDate"))
                return XmlRequest.SabreSendToXml(SabreRequestInfo(ServiceURL(ServiceName), Parameters), "GET", "");
            else
                return XmlRequest.GetPostSendToXml(ServiceURL(ServiceName), Parameters);
        }

        /// <summary>
        /// 세션 생성
        /// </summary>
        /// <returns></returns>
        public XmlElement SessionCreate()
        {
            return XmlRequest.GetSend(ServiceURL("SessionStart"));
        }
    }
}