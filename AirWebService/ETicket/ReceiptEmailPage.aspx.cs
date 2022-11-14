using System;

namespace AirWebService.ETicket
{
    public partial class ReceiptEmailPage : System.Web.UI.Page
	{
        public string ETicketURL = string.Empty;

		protected void Page_Load(object sender, EventArgs e)
		{
            ltrKPaxName.Text = Request["BookerName"];
            ltrAgentName.Text = Request["ANM"];

            ETicketURL = String.Concat("http://airservice2.modetour.com/ETicket/Receipt.aspx?MINFO=", Server.UrlEncode(Request["MINFO"]));
		}
	}
}