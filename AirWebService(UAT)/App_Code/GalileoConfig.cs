using System;
using System.Xml;
using System.Text;

namespace AirWebService
{
    public class GalileoConfig
    {
        AirConfig ac = new AirConfig();

        /// <summary>
        /// GDS명
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get { return "Galileo"; }
        }

        /// <summary>
        /// Galileo용 XML 파일의 로컬 폴더 경로
        /// </summary>
        private string XmlPath
        {
            get { return String.Format(@"{0}Galileo\", ac.XmlPhysicalPath); }
        }

        /// <summary>
        /// Galileo용 XML 파일의 로컬 경로
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public string XmlFullPath(string ServiceName)
        {
            return String.Format("{0}{1}.xml", XmlPath, ServiceName);
        }

        /// <summary>
        /// Galileo 서버 URL
        /// </summary>
        /// <param name="Gubun"></param>
        /// <returns></returns>
        public static string ServiceDomain(string Gubun)
        {
            string ServiceUrl = string.Empty;

            switch (Gubun)
            {
                case "api": ServiceUrl = "http://galileoapi.modetour.com/Avail/"; break;
                case "tkt": ServiceUrl = "http://galileoticketapi.modetour.com/AutoTkt/"; break;
                case "devapi": ServiceUrl = "http://172.30.52.119:8081/Avail/"; break;
                case "devtkt": ServiceUrl = "http://172.30.52.119:8082/AutoTkt/"; break;
                //case "api": ServiceUrl = "http://172.30.52.119:8081/Avail/"; break;
                //case "tkt": ServiceUrl = "http://172.30.52.119:8082/AutoTkt/"; break;
            }

            return ServiceUrl;
        }

        /// <summary>
        /// Galileo 호출 서비스 URL
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static string ServiceURL(string ServiceName)
        {
            string ServiceDomainGubun = string.Empty;
            string ServiceFileUrl = string.Empty;
            
            switch (ServiceName)
            {
                case "ResInfoCreate":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "ResInfoCreate.aspx";
                    break;
                case "ResProcess":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "ResProcess.aspx";
                    break;
                case "FareRule":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "FareRule.aspx";
                    break;
                case "PnrInfoDisplay":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "PnrInfoDisplay.aspx";
                    break;
                case "PnrCancel":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "PnrCancel.aspx";
                    break;
                case "GKPnr":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "GKPnr.aspx";
                    break;
                case "GKTicketNumUpdate":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "GKTicketNumUpdate.aspx";
                    break;
                case "ETicketInfoDisplay":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "eTicketInfoDisplay.aspx";
                    break;
                case "RemarksAdd":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "RemarksAdd.aspx";
                    break;
                case "Apis":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "Apis.aspx";
                    break;
                case "SessionStart":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "SessionStart.aspx";
                    break;
                case "SessionEnd":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "SessionEnd.aspx";
                    break;
                case "TerminalSubmit":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "TerminalSubmit.aspx";
                    break;
                case "TktProcess":
                    ServiceDomainGubun = "tkt";
                    ServiceFileUrl = "TktProcess.aspx";
                    break;
                case "TaxConfirmGP":
                    ServiceDomainGubun = "tkt";
                    ServiceFileUrl = "TaxConfirmGP.aspx";
                    break;
                case "QueuePnrList":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "QroomPnrList.aspx";
                    break;
                case "QroomTrans":
                    ServiceDomainGubun = "api";
                    ServiceFileUrl = "QroomTrans.aspx";
                    break;
                default:
                    ServiceDomainGubun = "devapi";
                    ServiceFileUrl = "Avail.aspx";
                    break;
            }

            return String.Concat(ServiceDomain(ServiceDomainGubun), ServiceFileUrl);
        }

        /// <summary>
        /// Galileo Request and Response(HttpWebRequest)
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public XmlElement HttpExecute(string ServiceName, string Parameters)
        {
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