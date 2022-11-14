using System;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AirWebService
{
	/// <summary>
	/// Abacus와의 통신을 위한 웹서비스
	/// </summary>
	[WebService(Namespace = "https://webservices.sabre.com/websvc")]
	[WebServiceBindingAttribute(Namespace = "https://webservices.sabre.com/websvc")]
	public class AbacusWebService : SoapHttpClientProtocol
	{
		public MessageHeader MessageHeaderValue;
		public Security SecurityValue;

		[DebuggerStepThroughAttribute()]
		public AbacusWebService()
		{
            this.Url = "https://webservices.havail.sabre.com/websvc";
		}

		[DebuggerStepThroughAttribute()]
		[SoapHeaderAttribute("MessageHeaderValue", Direction = SoapHeaderDirection.InOut)]
		[SoapHeaderAttribute("SecurityValue", Direction = SoapHeaderDirection.InOut)]
		[SoapDocumentMethodAttribute("OTA", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Bare)]
		[return: XmlAnyElementAttribute()]
		public XmlElement ServiceRQ([XmlAnyElementAttribute()]XmlElement inputXml)
		{
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            object[] results = this.Invoke("ServiceRQ", new object[] { inputXml });
			return (System.Xml.XmlElement)(results[0]);
		}

		[DebuggerStepThroughAttribute()]
		public IAsyncResult BeginServiceRQ([XmlAnyElementAttribute()]XmlElement inputXml, AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("ServiceRQ", new object[] { inputXml }, callback, asyncState);
		}

		[DebuggerStepThroughAttribute()]
		public XmlElement EndServiceRQ(IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (System.Xml.XmlElement)(results[0]);
		}
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	[XmlRootAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader", IsNullable = false)]
	public class MessageHeader : SoapHeader
	{
		public From From;
		public To To;
		public Service Service;
		public MessageData MessageData;
		public string Action;
		public string ConversationID;
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	public class From
	{
		[XmlElementAttribute("PartyId")]
		public PartyId[] PartyId;
		public string Role;
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	public class PartyId
	{
		[XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
		public string type;

		[XmlTextAttribute()]
		public string Value;
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	public class To
	{
		[XmlElementAttribute("PartyId")]
		public PartyId[] PartyId;
		public string Role;
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	public class Service
	{
		[XmlAttributeAttribute(Form = XmlSchemaForm.Qualified)]
		public string type;

		[XmlTextAttribute()]
		public string Value;
	}

	[XmlTypeAttribute(Namespace = "http://www.ebxml.org/namespaces/messageHeader")]
	public class MessageData
	{
		public string MessageID;
		public string TimeStamp;
		public string RefToMessageID;
		public DateTime TimeToLive;

		[XmlIgnoreAttribute()]
		public bool TimeToLiveSpecified;
	}

	[XmlTypeAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2002/12/secext")]
	[XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2002/12/secext", IsNullable = false)]
	public class Security : SoapHeader
	{
		public SecurityUserNameToken UserNameToken;
		public string BinarySecurityToken;
	}

	[XmlTypeAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2002/12/secext")]
	public class SecurityUserNameToken
	{
		public string UserName;
		public string Password;

		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified)]
		public string Organization;

		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified)]
		public string Domain;
	}
}