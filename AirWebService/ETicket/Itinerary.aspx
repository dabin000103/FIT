<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Itinerary.aspx.cs" Inherits="AirWebService.ETicket.Itinerary" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "//www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="//www.w3.org/1999/xhtml" xml:lang="ko">
<head>
	<meta http-equiv="Content-Type" content="text/html;charset=UTF-8"/>
	<title>전자 항공권 발행 확인서</title>
	<style type="text/css" media="all">
		* { margin:0; padding:0; }
		html, body { border:0; margin:0; padding:0; }
		@media print {
			@page {
			  size:21cm 29.7cm; /*A4*/
			  margin:15mm;  /* 여백 */
			}
			#eTicketReceipt{ page-break-before:always; }
		}
	</style>
</head>
<body>
    <table border="0" cellspacing="0" cellpadding="0" align="center" width="800" style="margin:0 auto;width:800px;mso-cellspacing:0cm;mso-yfti-tbllook:1184;mso-padding-alt:0cm 0cm 0cm 0cm;max-width:800px;border-collapse:collapse;">
		<tr style="padding:0;margin:0;background-color:transparent;">
			<td style="padding:0;margin:0;text-align:left;font-family:dotum, '돋움', Helvetica,Arial,sans-serif;">
				<table border="0" cellspacing="0" cellpadding="0" width="100%" style="width:100%; border-collapse:collapse; ">
					<tr>
						<th style="padding:0;margin:0;border:0;height:50px; font-size:11px; text-align:left; color:#000; border-bottom:3px solid #006f54;"><img src="//img.modetour.co.kr/modetour/2014/common/hn_modetour.png" alt="모두투어" height="35"/><br /><p style="padding-top:7px;"><asp:Literal ID="ltrAgentInfo" runat="server" /></p></th>
						<td style="padding:0;margin:0;height:50px; font-weight:bold; font-size:18px; text-align:right; color:#000; border-bottom:3px solid #006f54;"><p style="padding-top:13px;">여정표(Itinerary)<br /><span style="padding-right:3px; font-size:11px;">발행일 : <asp:Literal ID="ltrNowDateTime" runat="server" /></span></p></td>
					</tr>
				</table>
				<div style="width:100%;padding:0;margin:10px 0;">
					<p style="margin-bottom:10px;font-size:14px;line-height:19px;color:#333;"><strong>본 예약 확인증은 단지 귀하의 현재 예약 상황만 보여드리는 것으로 항공권 구매를 확인하는 증서는 아닙니다. 따라서, 항공권 구매 확인이 필요한 경우에는 해당 여행사로부터 별도의 항공권 지불 영수증 (ITR : Itinerary &amp; Receipt)을 수령하여 주시기 바랍니다.</strong></p>
                    <p style="padding:0;margin-bottom:10px;font-size:14px;line-height:19px;color:#ff0000;"><strong>당사는 고객님의 안전한 금융거래를 위하여 예금주가 "(주)모두투어네트워크" 인 계좌만 사용하고 있습니다. 이점 유념하시어 선의의 피해가 발생하지 않도록 주의하여 주시기 바랍니다.</strong></p>
					<div style="font-size:12px;line-height:19px;">
						<p style="margin:0;padding:0;">• TAX와 유류할증류는 발권 시점에 변경될 수 있습니다.</p>
						<p style="margin:0;padding:0;">• 예약하신 내용이 요청하신 내용과 일치하는지 확인하시기 바랍니다.</p>
						<p style="margin:0;padding:0;">• 아래 Comment에 별도 중요 공지사항이 있을 수 있으니 확인하시기 바랍니다.</p>
					</div>
				</div>
				<div style="width:100%;padding:10px 0;margin:0;">
					<table width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:0;padding:0;font-size:12px; border:1px solid #ddd;">
						<colgroup>
							<col width="40%" />
							<col width="60%" />
						</colgroup>
						<tbody>
							<%if (!String.IsNullOrWhiteSpace(PaxName)) {%>
                            <tr style="margin:0;padding:0;">
								<th scope="row" style="padding:5px; margin:0; text-align:left; background-color:#f2f2f2;">탑승객 영문명(성/이름) <span style="padding:0;font-weight:normal; font-size:12px; color:#818181;">(Passenger Name)</span></th>
								<td style="padding:5px; margin:0;border-left:1px solid #ddd;"><strong style="padding:0;margin:0;"><asp:Literal ID="ltrPaxName" runat="server" /></strong></td>
							</tr>
                            <%}%>
							<tr style="margin:0;padding:0;">
								<th scope="row" style="padding:5px; margin:0; text-align:left; border-top:1px solid #ddd; background-color:#f2f2f2;">예약 번호 <span style="font-weight:normal; font-size:12px; color:#818181;">(Booking Reference)</span></th>
								<td style="padding:4px; margin:0; border-top:1px solid #ddd; border-left:1px solid #ddd;"><strong style="padding:0;margin:0;"><asp:Literal ID="ltrBookingNumber" runat="server" /></strong></td>
							</tr>
						</tbody>
					</table>
				</div>
				<div style="width:100%;padding:0;margin:10px 0;">
					<h3 style="padding:0;margin:7px 0 5px; font-weight:bold; font-size:14px; line-height:normal; color:#333; text-align:left;">■ 여정 정보<span style="font-weight:normal; color:#818181;">(Itinerary Information)</span></h3>
					<asp:Repeater ID="rptFlightInfo" runat="server">
				        <ItemTemplate>
                            <div style="<%# Container.ItemIndex.Equals(0) ? "padding:0;margin-bottom:5px;" : "margin:10px 0 5px;padding-top:5px;"%>">
						        <p  style="padding:0;margin-bottom:3px; font-weight:bold; font-size:12px; height:17px; line-height:17px; "><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/ico/bu_gray_dot.gif" alt=""  style="vertical-align: top;"/> 편명(Flight) : <span style="color:#2988f4;"><%#String.Format("{0}{1} (<span style=\"font-weight:normal;\">예약번호:</span>{2})", XPath("@mcc"), XPath("@fln"), XPath("airlineRefNumber"))%></span> <span style="color:#666;">Operated by <%#XPath("@occ")%>(<%#XPath("operatingAirline")%>)</span></p>
						        <table width="100%" cellpadding="0" cellspacing="0" border="0" style="padding:0;margin:0;font-size:12px;border-top:1px solid #ddd; border-bottom:1px solid #ddd;">
							        <thead style="padding:0;margin:0;">
								        <tr style="padding:0;margin:0;">
									        <th scope="col" colspan="2" style="padding:5px; margin:0; border:0;border-bottom:2px solid #ddd; border-left:1px solid #ddd; background-color:#f2f2f2;">도시/공항</th>
									        <th scope="col" style="padding:5px; margin:0; border-bottom:2px solid #ddd; border-left:1px solid #ddd; background-color:#f2f2f2;">일자/시각</span></th>
									        <th scope="col" style="padding:5px; margin:0; border-left:1px solid #ddd; border-bottom:2px solid #ddd; background-color:#f2f2f2;">터미널</th>
									        <th scope="col" style="padding:5px; margin:0; border-left:1px solid #ddd; border-bottom:2px solid #ddd; background-color:#f2f2f2;">클래스</th>
									        <th scope="col" style="padding:5px; margin:0; border-left:1px solid #ddd; border-bottom:2px solid #ddd; background-color:#f2f2f2;">비행시간</span></th>
									        <th scope="col" style="padding:5px; margin:0; border-left:1px solid #ddd; border-right:1px solid #ddd; border-bottom:2px solid #ddd; background-color:#f2f2f2;">예약상태</th>
								        </tr>
							        </thead>
							        <tbody>
								        <tr>
									        <td style="padding:5px; margin:0; font-weight:bold; border-left:1px solid #ddd;">출발(Departure)</td>
									        <td style="padding:5px; margin:0;  font-weight:bold;"><%#XPath("departureAirport")%></td>
									        <td style="padding:5px; margin:0; font-weight:bold; border-bottom:1px solid #ddd; border-left:1px solid #ddd; text-align:center;"><%#cm.AbacusDateTime2(XPath("@ddt").ToString())%></td>
									        <td style="padding:5px; margin:0; font-weight:bold; border-bottom:1px solid #ddd; border-left:1px solid #ddd; text-align:center;"><%#(String.IsNullOrWhiteSpace(XPath("departureAirport/@terminalCode").ToString())) ? "&nbsp;" : XPath("departureAirport/@terminalCode")%></td>
									        <td rowspan="2" style="padding:0 5px; margin:0; font-weight:bold; border-left:1px solid #ddd; text-align:center;"><%#XPath("@rbd")%><%#GetBookingClass(XPath("cabin").ToString())%></td>
									        <td rowspan="2" style="padding:0 5px; margin:0; font-weight:bold; border-left:1px solid #ddd; text-align:center;"><%#XPath("@eft")%></td>
									        <td rowspan="2" style="padding:0 5px; margin:0; font-weight:bold; border-right:1px solid #ddd; border-left:1px solid #ddd; text-align:center;"><%#XPath("@rsc")%></td>
								        </tr>
								        <tr>
									        <td style="padding:5px; margin:0; font-weight:bold; border-top:1px solid #ddd; border-left:1px solid #ddd;">도착(Arrival)</td>
									        <td style="padding:5px; margin:0; font-weight:bold; border-top:1px solid #ddd; "><%#XPath("arrivalAirport")%></td>
									        <td style="padding:5px; margin:0; font-weight:bold; border-left:1px solid #ddd; text-align:center;"><%#cm.AbacusDateTime2(XPath("@ardt").ToString())%></td>
									        <td style="padding:5px; margin:0; font-weight:bold; border-left:1px solid #ddd; text-align:center;"><%#(String.IsNullOrWhiteSpace(XPath("arrivalAirport/@terminalCode").ToString())) ? "&nbsp;" : XPath("arrivalAirport/@terminalCode")%></td>
								        </tr>
							        </tbody>
						        </table>
						        <table width="100%" cellpadding="0" cellspacing="0" border="0" style="padding:0;margin:0;font-size:12px;border-collapse:collapse;">
							        <tbody>
								        <tr>
									        <th scope="row" style="padding:5px;margin:0;text-align:left;color:#666;border-bottom:1px solid #ddd;border-left:1px solid #ddd;">경유(Via)</th>
									        <td style="padding:5px;margin:0;color:#666;border-bottom:1px solid #ddd;"><%#XPath("legInfo/seg")%></td>
									        <th scope="row" style="padding:5px;margin:0;text-align:right;color:#666;border-bottom:1px solid #ddd;">좌석(Seat Number)</th>
									        <td style="padding:5px;text-align:left;color:#666;border-bottom:1px solid #ddd;"><%#XPath("fare/@seat")%></td>
									        <th scope="row" style="padding:5px; margin:0; text-align:right;color:#666;border-bottom:1px solid #ddd;">기종(Aircraft Type)</th>
									        <td style="padding:5px; margin:0; text-align:left;color:#666;border-right:1px solid #ddd;border-bottom:1px solid #ddd;"><%#XPath("equipment")%></td>
								        </tr>
							        </tbody>
						        </table>
                                <%#CodeshareAgreement(XPath("@mcc"), XPath("@occ"), XPath("operatingAirline"))%>
					        </div>
                         </ItemTemplate>
			        </asp:Repeater>
					<div style="padding:0;margin:0;font-size:12px;">
						<p style="padding:0;margin:0;line-height:normal;"><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/ico/bu_gray_dot.gif" alt="" style="vertical-align:middle;"/> 모든 시간은 현지 시간입니다.</p>
						<p style="padding:0;margin:0;line-height:normal;"><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/ico/bu_gray_dot.gif" alt="" style="vertical-align:middle;"/> 모든 정보는 항공사나 공항 사정에 의해서 변경 될 수 있습니다.</p>
					</div>
				</div>
                <div>
                    <a href="//img.modetour.com/ModeLiveBooking/air/main/ad_air_popup_171128.jpg" target="_blank"><img src="//img.modetour.com/ModeLiveBooking/air/main/ad_air_800_171128.jpg" alt="인천공항신청사안내" style="vertical-align: middle;border:0;width:800px;height:200px;"/></a>
                </div>
			</td>
		</tr>
	</table>
</body>
</html>