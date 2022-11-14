using System;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace AirWebService.InicisWEB
{
	/// <summary>
	/// Amadeus와의 통신을 위한 웹서비스
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBindingAttribute(Namespace = "http://tempuri.org/")]
	public class ModeCrsService : SoapHttpClientProtocol
	{
		[DebuggerStepThroughAttribute()]
        public ModeCrsService()
		{
			this.Url = "https://crs.modetour.com/ModeCrsService/WebService/InicisWEB.asmx";
		}

		[DebuggerStepThroughAttribute()]
        [SoapDocumentMethodAttribute("http://tempuri.org/CardCheckInicis", ParameterStyle = SoapParameterStyle.Bare)]
		[return: XmlAnyElementAttribute()]
		public XmlElement ServiceRQ([XmlAnyElementAttribute()]XmlElement inputXml)
		{
            System.Net.ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
            object[] results = this.Invoke("ServiceRQ", new object[] { inputXml });
			return (XmlElement)(results[0]);
		}

		[DebuggerStepThroughAttribute()]
		protected IAsyncResult BeginServiceRQ([XmlAnyElementAttribute()]XmlElement inputXml, AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("ServiceRQ", new object[] { inputXml }, callback, asyncState);
		}

		[DebuggerStepThroughAttribute()]
		protected XmlElement EndServiceRQ(IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (XmlElement)(results[0]);
		}
	}
}