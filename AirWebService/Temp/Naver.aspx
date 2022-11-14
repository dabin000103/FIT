<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Naver.aspx.cs" Inherits="AirWebService.Temp.Naver" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>네이버 테스트용</title>
    <meta http-equiv="Content-Style-Type" content="text/css" />
	<meta http-equiv="Content-Script-Type" content="text/javascript" />
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	<meta http-equiv="imagetoolbar" content="false" />
	<meta name="robots" content="noindex.nofollow" />
	<style type="text/css">
	* { margin:0; padding:0; font-size:12px; color:#414141; line-height:17px; outline:none; text-decoration:none; }
	.clearfix:before,.clearfix:after { content:"."; display:block; height:0; overflow:hidden; }.clearfix:after { clear:both; }.clearfix { display:inline-block; }/* ie-mac \*/*html .clearfix { height:1%; }.clearfix { display:block; }/* ie-mac */.clearfix { zoom:1; }/* IE < 8 */
	body { margin:10px; }
	table { border-collapse:collapse; border-spacing:0; border:2px solid #494949; width:100%; }
	th { padding:3px; color:#fff; background-color:#494949; border:1px solid #ccc; white-space:nowrap; }
	td { padding:3px 5px; border:1px solid #ccc; text-align:center; /*white-space:nowrap;*/ }
	p { float:right; padding:0 2px 0 0; }
    button { width:80px; height:21px; font-family:"맑은 고딕"; font-size:11px; cursor:pointer; }
    input.landingUrl { width:1000px; height:19px; }
    span.schedule { width:100%; color:#18609c; text-align:left; }
    span.avail { color:#d3d3d3; }
    p.destination { width:80px; font-weight:normal; text-align:left; float:left; }
    p.sel { width:80px; color:#bf0000; font-weight:bold; text-align:left; float:left; }
    .TALeft { text-align:left; }
    .hide { display:none; }
    a:link,a:visited,a:active { color:#18609c; text-decoration:none; }
    a:hover{ color:#58a0dc; text-decoration:underline; }
    </style>
</head>
<body>
    <div style="margin-bottom:10px; height:100px; overflow-y:auto;">
        <asp:Repeater ID="rptNaverDestinationList" runat="server">
            <ItemTemplate>
                <%#Eval("도착지").ToString().Equals(DLC) ? String.Format("<p class=\"sel\">{0} ({1})</p>", Eval("도착지"), Eval("요금수")) : String.Format("<p class=\"destination\"><a href=\"/Temp/Naver.aspx?DLC={0}&DTD={2}&ARD={3}&AIRV={4}\">{0} ({1})</a></p>", Eval("도착지"), Eval("요금수"), DTD, ARD, AIRV)%>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <table>
        <thead>
            <tr>
				<th rowspan="2">선택</th>
                <th rowspan="2">종류</th>
                <th rowspan="2">출발</th>
				<th rowspan="2">도착</th>
				<th rowspan="2">경유</th>
				<th rowspan="2">출발일</th>
				<th rowspan="2">여정</th>
				<th rowspan="2">항공</th>
				<th rowspan="2">상품코드</th>
                <th rowspan="2" class="hide">FareType</th>
                <th rowspan="2" class="hide">TaxInfo</th>
                <th rowspan="2">Class</th>
                <th rowspan="2">Itinerary</th>
                <th rowspan="2">스케쥴</th>
                <th colspan="4">요금</th>
                <th rowspan="2" class="hide">기착</th>
			</tr>
            <tr>
				<th>FareBasis</th>
                <th>운임</th>
                <th>Tax</th>
				<th>QCharge</th>
			</tr>
		</thead>
		<tbody>
			<asp:Repeater ID="rptNaverFareList" runat="server">
				<ItemTemplate>
					<tr>
						<td><input type="checkbox" id="<%#Eval("SGC")%>_<%#Eval("Trip")%>_<%#Eval("Way")%>_<%#Eval("순위")%>" name="<%#Eval("SGC")%>_<%#Eval("Trip")%>_<%#Eval("Way")%>" value="Y" onclick="CreateLink(this.id);" /></td>
                        <td><%#Eval("Gubun")%></td>
                        <td><%#Eval("SCity")%></td>
                        <td><%#Eval("ECity")%></td>
						<td><%#Eval("ViaCity")%></td>
                        <td><%#Eval("SDate")%></td>
						<td><%#Eval("Trip")%></td>
						<td><%#Eval("NAirV")%></td>
						<td class="TALeft"><%#Eval("SGC")%></td>
                        <td class="hide"><%#Eval("FareType")%></td>
                        <td class="hide"><%#Eval("TaxInfo")%></td>
                        <td><%#Eval("ItineraryClass")%></td>
                        <td class="TALeft"><%#Eval("Itinerary")%></td>
                        <td class="TALeft" style="white-space:initial;word-break:break-all;word-wrap:break-word;"><%//#Availability(Eval("SDate"), Eval("SCity"), Eval("ECity"), Eval("ViaCity"), Eval("ItineraryClass"), Eval("NAirV"), Eval("Itinerary")) %></td>
                        <td><%#Eval("FareBasis")%></td>
                        <td><%#String.Format("{0:#,##0}", Eval("NormalFare"))%></td>
                        <td><%#String.Format("{0:#,##0}", Eval("Tax"))%></td>
                        <td><%#String.Format("{0:#,##0}", Eval("Qcharge"))%></td>
                        <td class="hide"><%#Eval("StopCity")%></td>
					</tr>
                    <%#LinkTr(Eval("SGC"), Eval("순위"), Eval("중복수"))%>
				</ItemTemplate>
			</asp:Repeater>
		</tbody>
	</table>
    <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $('table tbody tr').each(function () {
                if ($(this).children('td').length > 10) {
                    var $tr = $(this);
                    var MCC = $tr.children('td:eq(7)').html();
                    var TmpMCC = "";
                    var TmpFLN = "";
                    var TmpItinerary = $tr.children('td:eq(12)').html().split("+");
                    var StrItinerary = "";
                    var Param = "";

                    for (var i = 0; i < TmpItinerary.length; i++) {
                        if (i > 0) {
                            TmpMCC += ",";
                            TmpFLN += ",";
                        }

                        TmpMCC += MCC;
                        TmpFLN += TmpItinerary[i].substr(8, 4);
                    }

                    Param = "<span class='avail'>SNM:4638, DTD:" + $tr.children('td:eq(5)').html() + ", DLC:" + $tr.children('td:eq(2)').html() + ", ALC:" + $tr.children('td:eq(3)').html() + ", CLC:" + $tr.children('td:eq(4)').html() + ", SCD:" + $tr.children('td:eq(11)').html() + ", MCC:" + TmpMCC + ", FLN:" + TmpFLN + "</span>";
                    //$tr.children('td:eq(12)').html($tr.children('td:eq(12)').html() + "<br/>" + Param);

                    $.ajax({
                        url: "/AllianceService.asmx/AvailabilityRS",
                        data: {
                            SNM: 4638,
                            DTD: $tr.children('td:eq(5)').html(),
                            DTT: "",
                            DLC: $tr.children('td:eq(2)').html(),
                            ALC: $tr.children('td:eq(3)').html(),
                            CLC: $tr.children('td:eq(4)').html(),
                            SCD: $tr.children('td:eq(11)').html(),
                            MCC: TmpMCC,
                            FLN: TmpFLN,
                            NOS: 1
                        },
                        type: "GET",
                        async: true,
                        dataType: "xml",
                        beforeSend: function () {
                            $tr.children('td:eq(13)').html("스케쥴 조회 중...");
                        },
                        success: function (data) {
                            if ($("segGroup", data).length > 0) {
                                $("segGroup", data).each(function () {
                                    $("seg", this).each(function () {
                                        if (StrItinerary > "")
                                            StrItinerary += ",";

                                        StrItinerary += $(this).attr("dlc");
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("alc");
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("ddt").substr(0, 10).replace(/-/g, "");
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("ardt").substr(0, 10).replace(/-/g, "");
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("ddt").substr(11, 5).replace(/:/g, "");
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("ardt").substr(11, 5).replace(/:/g, "");
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("mcc");
                                        StrItinerary += "^";
                                        StrItinerary += ($(this).attr("fln").length == 3) ? "0".concat($(this).attr("fln")) : $(this).attr("fln");
                                        StrItinerary += "^";
                                        StrItinerary += $("svcClass", this).attr("rbd");
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                        StrItinerary += "1";
                                        StrItinerary += "^";
                                        StrItinerary += ($(this).attr("ddt").substr(0, 10) == $(this).attr("ardt").substr(0, 10)) ? "0" : "1";
                                        StrItinerary += "^";
                                        StrItinerary += "0";
                                        StrItinerary += "^";
                                        StrItinerary += $(this).attr("occ");
                                        StrItinerary += "^";
                                        StrItinerary += $tr.children('td:eq(18)').html().replace(/\//g, "");
                                        StrItinerary += "^";
                                        StrItinerary += "";
                                        StrItinerary += "^";
                                    });

                                    StrItinerary += "," + $(this).attr("eft") + "^^";
                                });
                                
                                $tr.children('td:eq(13)').html(StrItinerary);
                            }
                            else {
                                $tr.children('td:eq(13)').html(Param);
                                $tr.find('input').prop('disabled', 'disabled');
                                $tr.children('td').attr("style", "color:#ccc");
                            }
                        },
                        error: function (error) {
                            $tr.children('td:eq(13)').html(error);
                            $tr.find('input').prop('disabled', 'disabled');
                            $tr.children('td').attr("style", "color:#ccc");
                        }
                    });
                }
            });
        });

        function CreateLink(id) {
            var ids = id.split("_");
            var URL = "";

            $('input:checkbox[name="' + ids[0] + '_' + ids[1] + '_' + ids[2] + '"]').prop("checked", false);
            $('input:checkbox[id="' + id + '"]').prop("checked", true);

            if (ids[1] == "RT" && $('input:checkbox[name^="' + ids[0] + '"]:checked').length < 2)
                return;

            if (ids[1] == "OW") {
                var $tr = $('input:checkbox[id="' + id + '"]').parent().parent();
                var $td = $tr.children('td');

                URL += "SCITY1=" + $td.eq(2).html();
                URL += "&ECITY1=" + $td.eq(3).html();
                URL += "&SCITY2=";
                URL += "&ECITY2=";
                URL += "&SCITY3=";
                URL += "&ECITY3=";
                URL += "&SDATE1=" + $td.eq(5).html();
                URL += "&SDATE2=";
                URL += "&SDATE3=";
                URL += "&TRIP=" + $td.eq(6).html();
                URL += "&FareType=" + $td.eq(9).html();
                URL += "&StayLength=";
                URL += "&Adt=1";
                URL += "&Chd=0";
                URL += "&Inf=0";
                URL += "&Itinerary1=" + $td.eq(13).html();
                URL += "&Itinerary2=";
                URL += "&SGC=" + $td.eq(8).html();
                URL += "&RGC=";
                URL += "&EventNum=";
                URL += "&PartnerNum=0";
                URL += "&PartnerNum=0";
                URL += "&TaxInfo=" + $td.eq(10).html();
                URL += "&NAirV=" + $td.eq(7).html();
                URL += "&NaPm=";
                URL += "&NVKWD=";
                URL += "&NVADKWD=";
                URL += "&NVAR=fit";
                URL += "&NVADID=";
            }
            else
            {
                var $trS = $('input:checkbox[name="' + ids[0] + '_' + ids[1] + '_S"]:checked').parent().parent();
                var $trR = $('input:checkbox[name="' + ids[0] + '_' + ids[1] + '_R"]:checked').parent().parent();
                var $tdS = $trS.children('td');
                var $tdR = $trR.children('td');

                URL += "SCITY1=" + $tdS.eq(2).html();
                URL += "&ECITY1=" + $tdS.eq(3).html();
                URL += "&SCITY2=" + $tdR.eq(2).html();
                URL += "&ECITY2=" + $tdR.eq(3).html();
                URL += "&SCITY3=";
                URL += "&ECITY3=";
                URL += "&SDATE1=" + $tdS.eq(5).html();
                URL += "&SDATE2=" + $tdR.eq(5).html();
                URL += "&SDATE3=";
                URL += "&TRIP=" + $tdS.eq(6).html();
                URL += "&FareType=" + $tdS.eq(9).html();
                URL += "&StayLength=";
                URL += "&Adt=1";
                URL += "&Chd=0";
                URL += "&Inf=0";
                URL += "&Itinerary1=" + $tdS.eq(13).html();
                URL += "&Itinerary2=" + $tdR.eq(13).html();
                URL += "&SGC=" + $tdS.eq(8).html();
                URL += "&RGC=" + $tdR.eq(8).html();
                URL += "&EventNum=";
                URL += "&PartnerNum=0";
                URL += "&PartnerNum=0";
                URL += "&TaxInfo=" + $tdS.eq(10).html();
                URL += "&NAirV=" + $tdS.eq(7).html();
                URL += "&NaPm=";
                URL += "&NVKWD=";
                URL += "&NVADKWD=";
                URL += "&NVAR=fit";
                URL += "&NVADID=";
            }

            $('#' + ids[0]).html(URL + "<br /><button onclick=\"window.open('/Temp/NaverLanding.aspx?" + URL + "');\">웹서비스</button>&nbsp;<button onclick=\"window.open('http://naverair.modetour.com/LiveBooking/Agent/Naver/FareAvailList.aspx?" + URL + "');\">랜딩페이지1</button>&nbsp;<input type=\"text\" value=\"http://m.naverair.modetour.com/Air/Agent/Naver/Schedule.htm?" + URL + "\" class=\"landingUrl\"/>");
        }
    </script>
</body>
</html>