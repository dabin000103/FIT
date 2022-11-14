using System;
using System.Xml;
using System.Text;

namespace AirWebService
{
	public class TopasConfig
	{
		AirConfig ac = new AirConfig();

		/// <summary>
		/// GDS명
		/// </summary>
		/// <returns></returns>
		public string Name
		{
			get { return "Topas"; }
		}

		/// <summary>
		/// Topas용 XML 파일의 로컬 폴더 경로
		/// </summary>
		private string XmlPath
		{
			get { return String.Format(@"{0}Topas\", ac.XmlPhysicalPath); }
		}

		/// <summary>
		/// Topas용 XML 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string XmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", XmlPath, ServiceName);
		}

		/// <summary>
		/// 네임스페이스
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public static string NamespaceURL(string ServiceName)
		{
			string Namespace = string.Empty;

			switch (ServiceName)
			{
                case "AirLineRequestService":
                case "ApprovalRequestService":
                    Namespace = "http://TOPAS_GPS_Service_Library";
                    break;
			}

			return Namespace;
		}

		/// <summary>
		/// Topas 호출 서비스 URL
		/// </summary>
		/// <returns></returns>
        public static string ServiceURL(string ServiceName)
		{
            string Url = string.Empty;

            switch (ServiceName)
            {
                case "AutomatedRuleTranslator":
                    //Url = "http://artdev.topas.net/api";
                    Url = "https://art.topas.net/prod/api";
                    break;
                default:
                    Url = "https://topasgps.topas.net/TOPASGPS.SRV.WS01/V1_01";
                    break;
            }

            return Url;
		}

        public XmlElement HttpExecute(string ServiceName, XmlElement ReqXml, string GUID)
        {
            if (ServiceName.Equals("AirLineRequestService"))
            {
                return XmlRequest.AmadeusSoapSend(ServiceURL(""), "", ServiceName, XmlRequest.SoapHeaderForTopas(ReqXml.OuterXml), GUID);
            }
            else if (ServiceName.Equals("ApprovalRequestService"))
            {
                return XmlRequest.AmadeusSoapSend(ServiceURL(""), "", ServiceName, XmlRequest.SoapHeaderForTopas(ReqXml.OuterXml), GUID);
            }
            else
            {
                return null;
            }
        }

        public string HttpExecute(int SNM, string ServiceName, string ReqXml, string GUID)
        {
            if (ServiceName.Equals("AutomatedRuleTranslator"))
            {
                //return XmlRequest.TopasSendToJson(String.Format("{0}/v1/art/getrule/SC9/{1}", ServiceURL(ServiceName), AmadeusConfig.OfficeId(SNM)), "POST", ReqXml, "IA");
                return XmlRequest.TopasSendToJson(String.Format("{0}/v1/art/getrule/SC9/SELK138NT", ServiceURL(ServiceName)), "POST", ReqXml, "IA");
            }
            else
            {
                return null;
            }
        }
	}
}