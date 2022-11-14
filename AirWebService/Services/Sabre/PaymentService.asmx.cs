using System;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace AirWebService.SabePaymentService
{
	/// <summary>
	/// Amadeus와의 통신을 위한 웹서비스
	/// </summary>
    [WebService(Namespace = "http://payment.ws.fep.abacus.com/")]
    [WebServiceBindingAttribute(Namespace = "http://payment.ws.fep.abacus.com/")]
	public class SabreWebService : SoapHttpClientProtocol
	{
		[DebuggerStepThroughAttribute()]
        public SabreWebService()
		{
			//this.Url = "http://www.sabreworkspace.co.kr:80/webservice/PaymentService";
            this.Url = "http://165.141.169.105:8080/webservice/PaymentService";
		}

		[DebuggerStepThroughAttribute()]
        [SoapDocumentMethodAttribute("http://payment.ws.fep.abacus.com/PaymentService/CardApproval", ParameterStyle = SoapParameterStyle.Bare)]
		[return: XmlAnyElementAttribute()]
		public XmlElement ServiceRQ([XmlAnyElementAttribute()]XmlElement inputXml)
		{
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
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