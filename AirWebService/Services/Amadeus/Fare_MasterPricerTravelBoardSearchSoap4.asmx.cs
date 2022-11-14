using System;
using System.Diagnostics;
using System.Net;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace AirWebService.MasterPricerTravelBoardSearchSoap4
{
	/// <summary>
	/// Amadeus와의 통신을 위한 웹서비스
	/// </summary>
	[WebService(Namespace = "http://xml.amadeus.com/")]
	[WebServiceBindingAttribute(Namespace = "http://xml.amadeus.com/")]
	public class AmadeusWebService : SoapHttpClientProtocol
	{
        public MessageHeader MessageHeaderValue;

		[DebuggerStepThroughAttribute()]
		public AmadeusWebService()
		{
			//this.Url = AmadeusConfig.ServiceURL();
            this.Url = "https://nodeD1.test.webservices.amadeus.com/1ASIWIBEMOT";
		}

        //protected override WebRequest GetWebRequest(Uri uri)
        //{

        //}

        //protected override WebResponse GetWebResponse(WebRequest request)
        //{

        //}

        //protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult ar)
        //{
        //    HttpWebResponse httpresponse = (HttpWebResponse)request.GetResponse();

        //    if (httpresponse.ContentEncoding != "gzip" && _acceptGZip != false)
        //        _acceptGZip = false;
        //    {
        //        if (_acceptGZip)
        //        {
        //            HttpWebResponse httpResponse = (HttpWebResponse)request.EndGetResponse(ar);
        //            GZipHttpWebResponse gZipResponse = new GZipHttpWebResponse(httpResponse);
        //            return gZipResponse;
        //        }
        //        else
        //        {
        //            return base.GetWebResponse(request, ar);
        //        }
        //    }
        //}









		[DebuggerStepThroughAttribute()]
        [SoapHeaderAttribute("MessageHeaderValue", Direction = SoapHeaderDirection.InOut)]
        [SoapDocumentMethodAttribute("http://webservices.amadeus.com/FMPTBQ_13_3_1A", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Bare)]
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