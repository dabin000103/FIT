using System;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AirWebService.InstantTravelBoardSearch
{
	/// <summary>
	/// Amadeus와의 통신을 위한 웹서비스
	/// </summary>
	[WebService(Namespace = "http://xml.amadeus.com/")]
	[WebServiceBindingAttribute(Namespace = "http://xml.amadeus.com/")]
	public class AmadeusWebService : SoapHttpClientProtocol
	{
        public MessageID MessageIDValue;
        public Action ActionValue;
        public To ToValue;
        public Security SecurityValue;
        public AMA_SecurityHostedUser AMA_SecurityHostedUserValue;

		[DebuggerStepThroughAttribute()]
		public AmadeusWebService()
		{
			//this.Url = AmadeusConfig.ServiceURL();
            this.Url = "https://nodeD1.production.webservices.amadeus.com";
		}

		[DebuggerStepThroughAttribute()]
        [SoapHeaderAttribute("MessageIDValue", Direction = SoapHeaderDirection.InOut)]
        [SoapHeaderAttribute("ActionValue", Direction = SoapHeaderDirection.InOut)]
        [SoapHeaderAttribute("ToValue", Direction = SoapHeaderDirection.InOut)]
        [SoapHeaderAttribute("SecurityValue", Direction = SoapHeaderDirection.InOut)]
        [SoapHeaderAttribute("AMA_SecurityHostedUserValue", Direction = SoapHeaderDirection.InOut)]
        [SoapDocumentMethodAttribute("http://webservices.amadeus.com/FIFRTQ_16_2_1A", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Bare)]
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

    [XmlTypeAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
    public class MessageID : SoapHeader
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string add;

        [XmlTextAttribute()]
        public string Value;
    }

    [XmlTypeAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
    public class Action : SoapHeader
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string add;

        [XmlTextAttribute()]
        public string Value;
    }

    [XmlTypeAttribute(Namespace = "http://www.w3.org/2005/08/addressing")]
    public class To : SoapHeader
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string add;

        [XmlTextAttribute()]
        public string Value;
    }

    [XmlTypeAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    [XmlRootAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", IsNullable = false)]
    public class Security : SoapHeader
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string oas;
        
        public UsernameToken UsernameToken;
    }

    [XmlTypeAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
    [XmlRootAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", IsNullable = false)]
    public class UsernameToken
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string Id;

        public string Username;
        public Nonce Nonce;
        public Password Password;

        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string Created;
    }

    [XmlTypeAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    public class Nonce
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string EncodingType;

        [XmlTextAttribute()]
        public string Value;
    }

    [XmlTypeAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    public class Password
    {
        [XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
        public string Type;

        [XmlTextAttribute()]
        public string Value;
    }

    [XmlTypeAttribute(Namespace = "http://xml.amadeus.com/2010/06/Security_v1")]
    [XmlRootAttribute(Namespace = "http://xml.amadeus.com/2010/06/Security_v1", IsNullable = false)]
    public class AMA_SecurityHostedUser : SoapHeader
    {
        [XmlElementAttribute(Form = XmlSchemaForm.Unqualified)]
        public UserID UserID;
    }

    public class UserID
    {
        [XmlAttribute()]
        public string AgentDutyCode;

        [XmlAttribute()]
        public string POS_Type;

        [XmlAttribute()]
        public string PseudoCityCode;

        [XmlAttribute()]
        public string RequestorType;
    }
}