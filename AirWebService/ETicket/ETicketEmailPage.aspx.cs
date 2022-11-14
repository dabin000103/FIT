using System;

namespace AirWebService.ETicket
{
    public partial class ETicketEmailPage : System.Web.UI.Page
	{
        public string ETicketURL = string.Empty;
        public string ULC = "ko";

		protected void Page_Load(object sender, EventArgs e)
		{
            string ANM = Request["ANM"];
            string[] PaxNames = Request["PaxName"].Split('/');

            if (Request["AID"] != null && Request["AID"].Equals("2783675"))
            {
                ULC = "en";

                ltrEPaxName2.Text = String.Format("{0}/{1}", PaxNames[0].Trim(), new Common().SplitPaxType(PaxNames[1].Trim(), false)[1]);
                ltrEPaxName3.Text = ltrEPaxName2.Text;
            }
            else
            {
                ltrKPaxName.Text = String.Format("{0}/{1}", PaxNames[0].Trim(), new Common().SplitPaxType(PaxNames[1].Trim(), false)[1]);
                ltrEPaxName.Text = ltrKPaxName.Text;
                ltrAgentName.Text = Request["ANM"];
            }

            ETicketURL = String.Concat("http://airservice2.modetour.com/ETicket/ETicket.aspx?MINFO=", Server.UrlEncode(Request["MINFO"]));
		}
	}
}