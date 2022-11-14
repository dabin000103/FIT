using System;
using System.Web;

namespace AirWebService
{
	public class AirConfig
	{
		private static string mPath = HttpContext.Current.Request.PhysicalApplicationPath;
		private static string mHost = HttpContext.Current.Request.Url.Host;

		/// <summary>
		/// 웹서비스 호스트 정보
		/// </summary>
		public string Host
		{
			get { return mHost; }
		}

		/// <summary>
		/// 웹서비스 로컬 경로
		/// </summary>
		public string PhysicalPath
		{
			get { return mPath; }
		}

		/// <summary>
		/// XML 파일의 로컬 경로
		/// </summary>
		public string XmlPhysicalPath
		{
			get { return String.Format(@"{0}Xml\", mPath); }
		}

		/// <summary>
		/// Help XML 파일의 로컬 경로
		/// </summary>
		public string HelpXmlPhysicalPath
		{
			get { return String.Format(@"{0}Help\", XmlPhysicalPath); }
		}

		/// <summary>
		/// Help XML(RQ,RS) 파일의 로컬 경로
		/// </summary>
		public string RqRsXmlPhysicalPath
		{
			get { return String.Format(@"{0}HelpXml\", XmlPhysicalPath); }
		}

		/// <summary>
		/// SaveXml 파일의 로컬 경로
		/// </summary>
		public string SaveXmlPhysicalPath
		{
			get { return String.Format(@"{0}WebServiceLog2\AirWebService\SaveXml\", mPath.Substring(0, 3)); }
		}
	}
}