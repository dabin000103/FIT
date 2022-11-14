using System;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace AirWebService.PricePNRWithBookingClassKEPricing
{
	/// <summary>
	/// Amadeus와의 통신을 위한 웹서비스
	/// </summary>
	[WebService(Namespace = "http://xml.amadeus.com/")]
	[WebServiceBindingAttribute(Namespace = "http://xml.amadeus.com/")]
	public class AmadeusWebService : SoapHttpClientProtocol
	{
		public Session session;

		[DebuggerStepThroughAttribute()]
		public AmadeusWebService()
		{
			this.Url = AmadeusConfig.ServiceURL();
		}

		[DebuggerStepThroughAttribute()]
		[SoapHeaderAttribute("session", Direction = SoapHeaderDirection.InOut)]
        [SoapDocumentMethodAttribute("http://webservices.amadeus.com/1ASIWIBEMOT/TPCBRQ_16_1_1A", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Bare)]
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

	[XmlTypeAttribute(Namespace = "http://xml.amadeus.com/ws/2009/01/WBS_Session-2.0.xsd")]
	[XmlRootAttribute(Namespace = "http://xml.amadeus.com/ws/2009/01/WBS_Session-2.0.xsd", IsNullable = false)]
	public class Session : SoapHeader
	{
		public string SessionId;
		public string SequenceNumber;
		public string SecurityToken;
	}
}