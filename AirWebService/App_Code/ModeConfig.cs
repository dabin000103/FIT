using System;
using System.Xml;

namespace AirWebService
{
	public class ModeConfig
	{
		AirConfig ac = new AirConfig();

		/// <summary>
		/// GDS명
		/// </summary>
		/// <returns></returns>
		public string Name
		{
			get { return "Mode"; }
		}

		/// <summary>
		/// Mode용 XML 파일의 로컬 폴더 경로
		/// </summary>
		private string XmlPath
		{
			get { return String.Format(@"{0}Mode\", ac.XmlPhysicalPath); }
		}

		/// <summary>
		/// Mode용 XML 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string XmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", XmlPath, ServiceName);
		}

		/// <summary>
		/// Mode용 Help XML 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string HelpXmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", ac.HelpXmlPhysicalPath, ServiceName);
		}

		/// <summary>
		/// Mode용 Help XML(RQ,RS) 파일의 로컬 경로
		/// </summary>
		/// <param name="ServiceName"></param>
		/// <returns></returns>
		public string RqRsXmlFullPath(string ServiceName)
		{
			return String.Format("{0}{1}.xml", ac.RqRsXmlPhysicalPath, ServiceName);
		}

		/// <summary>
		/// SaveXml 파일의 로컬 경로
		/// </summary>
		public string SaveXmlPath()
		{
			return ac.SaveXmlPhysicalPath;
		}
	}
}