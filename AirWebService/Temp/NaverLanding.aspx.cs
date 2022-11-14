using System;

namespace AirWebService.Temp
{
    public partial class NaverLanding : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            Session.CodePage = 65001;
            Response.Charset = "UTF-8";
            Response.ContentType = "text/xml";
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(new AllianceService().SearchFareAvailforNaverRS(
                                                    4638,
                                                    Request["NAirV"],
                                                    String.Format("{0}*{1}*{2}*{3}*{4}*{5}*{6}", Request["SCITY1"], Request["SCITY2"], Request["SCITY3"], Request["SCITY4"], Request["SCITY5"], Request["SCITY6"], Request["SCITY7"]),
                                                    String.Format("{0}*{1}*{2}*{3}*{4}*{5}*{6}", Request["ECITY1"], Request["ECITY2"], Request["ECITY3"], Request["ECITY4"], Request["ECITY5"], Request["ECITY6"], Request["ECITY7"]),
                                                    String.Format("{0}*{1}*{2}*{3}*{4}*{5}*{6}", Request["SDATE1"], Request["SDATE2"], Request["SDATE3"], Request["SDATE4"], Request["SDATE5"], Request["SDATE6"], Request["SDATE7"]),
                                                    Request["TRIP"],
                                                    Request["FareType"],
                                                    Request["StayLength"],
                                                    String.Format("{0}*{1}", Request["SGC"], Request["RGC"]),
                                                    Request["EventNum"],
                                                    Request["PartnerNum"],
                                                    Request["PromotionCode"],
                                                    Request["PromotionName"],
                                                    String.Format("{0}*{1}*{2}*{3}", Request["PromotionAmt"], Request["PromotionAdtInd"], Request["PromotionChdInd"], Request["PromotionInfInd"]),
                                                    String.Format("{0}*{1}*{2}", Request["Itinerary1"], Request["Itinerary2"], Request["Itinerary3"]),
                                                    Request["TaxInfo"],
                                                    Request["NaverFareJoin"],
                                                    Request["FareLocation"],
                                                    String.Format("{0}*{1}", Request["AddOnDomStart"], Request["AddOnDomReturn"]),
                                                    String.Format("{0}*{1}*{2}", Request["AdultBagInfo"], Request["ChildBagInfo"], Request["InfantBagInfo"]),
                                                    Request["PlatingCarrier"],
                                                    Request["FareInfo"],
                                                    Convert.ToInt32(Request["Adt"]),
                                                    Convert.ToInt32(Request["Chd"]),
                                                    Convert.ToInt32(Request["Inf"]),
                                                    "WEBSERVICE"
                                                ).OuterXml);
        }
    }
}