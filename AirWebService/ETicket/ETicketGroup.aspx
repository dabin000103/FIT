<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ETicketGroup.aspx.cs" Inherits="AirWebService.ETicket.ETicketGroup" %>
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
            .hidePrint { display:none; }
		}
	</style>
</head>
<body>
    <div style="margin:0 auto; width:800px;font-family:dotum, '돋움', Helvetica,Arial,sans-serif;">
		<%if (PrintBtn) {%><div style="overflow:hidden;" class="hidePrint"><a href="./ETicket.aspx" onclick="window.print();return false;" style="display:inline-block;float:right;margin-bottom:5px;"><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/btn_print.png" alt="인쇄하기" style="border:0;" /></a></div><%}%>
        <div style="border-bottom:3px solid #006f54; overflow:hidden;">
			<h1 style="margin:0; float:left; height:50px; font-size:14px; color:#000;"><img src="//img.modetour.co.kr/modetour/2014/common/hn_modetour.png" alt="모두투어" width="148" height="27"/><br /><p style="padding-top:7px;">(주)모두투어네트워크</p></h1>
			<h2 style="margin:0; float:right; height:50px; font-size:18px; color:#000;"><p style="padding-top:15px; text-align:right;">전자 항공권 발행 확인서<br /><span style="font-size:12px;">e-Ticket Passenger Itinerary &amp; Receipt</span></p></h2>
		</div>
		<div style="margin:10px 0;">
			<table width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:12px; border:1px solid #ddd;">
				<caption style="display:none;">■ 승객정보 (Passenger Information)</caption>
				<colgroup>
					<col width="40%" />
					<col width="60%" />
				</colgroup>
				<tbody>
					<tr>
						<th scope="row" style="padding:5px; text-align:left; border-top:1px solid #ddd; background-color:#f2f2f2;">항공사 예약번호 <span style="font-weight:normal; font-size:12px; color:#818181;">(Booking Reference)</span></th>
						<td style="padding:0 5px; border-top:1px solid #ddd; border-left:1px solid #ddd;"><asp:Literal ID="ltrBookingNumber" runat="server" /></td>
					</tr>
					<tr>
						<th scope="row" style="padding:5px; text-align:left; border-top:1px solid #ddd; background-color:#f2f2f2;">모두투어 예약번호 <span style="font-weight:normal; font-size:12px; color:#818181;">(modetour Booking Reference)</span></th>
						<td style="padding:0 5px; border-top:1px solid #ddd; border-left:1px solid #ddd;"><asp:Literal ID="ltrModetourBookingNumber" runat="server" /></td>
					</tr>
				</tbody>
			</table>
        </div>
        <div style="margin:10px 0;">
            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:12px; border:1px solid #ddd; border-collapse:collapse;">
				<caption style="display:none;">■ 승객정보(Passenger Information)</caption>
				<colgroup>
					<col width="200px"/>
					<col width="100px"/>
					<col width="140px"/>
					<col />
				</colgroup>
				<thead>
					<tr>
						<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-left:0; border-bottom-width:2px; background-color:#f8f8f8;">탑승객 영문명(성/이름)<br /><span style="font-weight:normal; font-size:12px; color:#818181;">(Passenger Name)</span></th>
						<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8;">항공권 번호<br /><span style="font-weight:normal; font-size:12px; color:#818181;">(Ticket Number)</span></th>
						<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8;">탑승객코드<br /><span style="font-weight:normal; font-size:12px; color:#818181;">(PassengerType Code)</span></th>
						<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8;">좌석번호<br /><span style="font-weight:normal; font-size:12px; color:#818181;">(Seat Number)</span></th>
					</tr>
				</thead>
				<tbody>
					<asp:Repeater ID="rptTravellerInfo" runat="server">
				        <ItemTemplate>
                            <tr>
						        <td style="padding:4px 5px 1px; border:1px solid #ddd; font-weight:bold;"><%#XPath("name")%></td>
						        <td style="padding:4px 5px 1px; border:1px solid #ddd; border-right:0; border-bottom:0; text-align:center;"><%#XPath("ticketNumber")%></td>
						        <td style="padding:4px 5px 1px; border:1px solid #ddd; border-right:0; border-bottom:0; text-align:center;"><%#XPath("@ptc")%></td>
						        <td style="padding:4px 5px 1px; border:1px solid #ddd; border-right:0; border-bottom:0; font-family:'Times New Roman','Courier New',sans-serif;">&nbsp;</td>
					        </tr>
                        </ItemTemplate>
                    </asp:Repeater>
				</tbody>
			</table>
		</div>
		<div style="margin:10px 0;">
			<h3 style="margin:7px 0 5px; font-weight:bold; font-size:14px; line-height:normal; color:#333; text-align:left;">■ 여정 정보<span style="font-weight:normal; color:#818181;">(Itinerary Information)</span></h3>
			<asp:Repeater ID="rptFlightInfo" runat="server">
				<ItemTemplate>
					<div style="<%# Container.ItemIndex.Equals(0) ? "margin-bottom:5px;" : "margin:10px 0 5px;padding-top:5px;"%>">
						<p style="margin-bottom:1px; padding-left:5px; font-weight:bold; font-size:12px; height:17px; line-height:17px;"><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/ico/bu_gray_dot.gif" alt=""  style="vertical-align: middle;"/> 편명 <span style="font-weight:normal; color:#818181;">(Flight)</span> <span style="color:#2988f4;"><%#String.Format("{0}{1} (<span style=\"font-weight:normal;\">예약번호:</span>{2})", XPath("@mcc"), XPath("@fln"), XPath("airlineRefNumber"))%></span> <span>Operated by <%#XPath("@occ")%>(<%#XPath("operatingAirline")%>)</span></p>
						<table width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:12px; border:1px solid #ddd; border-collapse:collapse;">
							<caption style="display:none;">■ 여정 정보(Itinerary Information)</caption>
							<colgroup>
								<col width="17%"/>
								<col />
								<col width="14%"/>
								<col width="8%"/>
								<col width="11%"/>
								<col width="9%"/>
								<col width="9%"/>
							</colgroup>
							<thead>
								<tr>
									<th scope="col" colspan="2" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-left:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">도시/공항</th>
									<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">일자/시각</th>
									<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">터미널</th>
									<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">클래스</th>
									<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">비행시간</th>
									<th scope="col" style="padding:3px 5px 1px; border:1px solid #ddd; border-top:0; border-right:0; border-bottom-width:2px; background-color:#f8f8f8; font-weight:normal;">예약상태</th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<td style="padding:4px 5px 1px;">출발 <span style="font-weight:normal; color:#818181;">(Departure)</span></td>
									<td style="padding:4px 5px 1px; font-weight:bold;"><%#XPath("departureAirport")%></td>
									<td style="padding:4px 5px 1px; border-left:1px solid #ddd; font-weight:bold; text-align:center;"><%#cm.AbacusDateTime2(XPath("@ddt").ToString())%></td>
									<td style="padding:4px 5px 1px; border-left:1px solid #ddd; text-align:center; font-family:'Times New Roman','Courier New',sans-serif;"><%#(String.IsNullOrWhiteSpace(XPath("departureAirport/@terminalCode").ToString())) ? (String.IsNullOrWhiteSpace(XPath("departureAirport/@terminal").ToString())) ? "&nbsp;" : XPath("departureAirport/@terminal") : XPath("departureAirport/@terminalCode")%></td>
									<td rowspan="2" style="padding:4px 5px 1px; border-left:1px solid #ddd; text-align:center;"><%#XPath("@rbd")%><%#GetBookingClass(XPath("cabin").ToString())%></td>
									<td rowspan="2" style="padding:4px 5px 1px; border-left:1px solid #ddd; text-align:center;"><%#XPath("@eft")%></td>
									<td rowspan="2" style="padding:4px 5px 1px; border-left:1px solid #ddd; text-align:center;"><%#XPath("@rsc")%></td>
								</tr>
								<tr>
									<td style="padding:4px 5px 1px; border-top:1px solid #ddd;">도착 <span style="font-weight:normal; color:#818181;">(Arrival)</span></td>
									<td style="padding:4px 5px 1px; border-top:1px solid #ddd; font-weight:bold;"><%#XPath("arrivalAirport")%></td>
									<td style="padding:4px 5px 1px; border:1px solid #ddd; border-right:0; border-bottom:0; font-weight:bold; text-align:center;"><%#cm.AbacusDateTime2(XPath("@ardt").ToString())%></td>
									<td style="padding:4px 5px 1px; border:1px solid #ddd; border-right:0; border-bottom:0; text-align:center; font-family:'Times New Roman','Courier New',sans-serif;"><%#(String.IsNullOrWhiteSpace(XPath("arrivalAirport/@terminalCode").ToString())) ? (String.IsNullOrWhiteSpace(XPath("arrivalAirport/@terminal").ToString())) ? "&nbsp;" : XPath("arrivalAirport/@terminal") : XPath("arrivalAirport/@terminalCode")%></td>
								</tr>
							</tbody>
						</table>
						<table width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:12px; border:1px solid #ddd;">
							<colgroup>
								<col width="17%"/>
								<col width=""/>
								<col width="24%"/>
								<col width="17%"/>
								<col width="9%"/>
							</colgroup>
							<tbody>
								<tr style="display:<%#XPath("legInfo") != null ? "" : "none"%>;">
									<th scope="row" style="padding:4px 5px 1px; font-weight:normal; text-align:left;">경유 <span style="font-weight:normal; color:#818181;">(Via)</span></th>
									<td style="padding:4px 5px 1px; font-weight:bold; color:#818181;"><%#XPath("legInfo/seg")%></td>
									<th scope="row" style="padding:4px 5px 1px; font-weight:normal; text-align:left;">경유지 체류시간 <span style="font-weight:normal; color:#818181;">(Ground Time)</span></th>
									<td style="padding:4px 5px 1px; font-weight:bold;"></td>
									<td style="padding:4px 5px 1px; font-weight:bold; color:#818181;"><%#XPath("legInfo/seg/@gwt")%></td>
								</tr>
								<tr>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">기종 <span style="font-weight:normal; color:#818181;">(Aircraft Type)</span></th>
									<td style="padding:3px 5px 1px; font-weight:bold; color:#818181;"><%#XPath("equipment")%></td>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">항공권 유효기간 <span style="font-weight:normal; color:#818181;">(Validity)</span></th>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">Not Valid Before</th>
									<td style="padding:3px 5px 1px; font-weight:bold; color:#818181;"><%#XPath("fareInfo/fare") != null ? cm.AbacusDateTime(XPath("fareInfo/fare/@nvb").ToString()) : ""%></td>
								</tr>
								<tr>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;"></th>
									<td style="padding:3px 5px 1px; font-weight:bold; color:#818181;"></td>
									<td style="padding:3px 5px 1px; font-weight:bold;"></td>
									<td scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">Not Valid After</td>
									<td style="padding:3px 5px 1px; font-weight:bold; color:#818181;"><%#XPath("fareInfo/fare") != null ? cm.AbacusDateTime(XPath("fareInfo/fare/@nva").ToString()) : ""%></td>
								</tr>
								<tr>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">운임 <span style="font-weight:normal; color:#818181;">(fare Basis)</span></th>
									<td colspan="4" style="padding:3px 5px 1px; font-weight:bold; color:#818181;"><%#DisplayFarebasis((System.Xml.XmlNode)Container.DataItem) %></td>
								</tr>
								<tr>
									<th scope="row" style="padding:3px 5px 1px; font-weight:normal; text-align:left;">수하물 <span style="font-weight:normal; color:#818181;">(Baggage)</span></th>
									<td colspan="4" style="padding:3px 5px 1px; font-weight:bold; color:#818181;"><%#DisplayBaggage((System.Xml.XmlNode)Container.DataItem) %></td>
								</tr>
							</tbody>
						</table>
                        <%#CodeshareAgreement(XPath("@mcc"), XPath("@occ"), XPath("operatingAirline"))%>
					</div>
				</ItemTemplate>
			</asp:Repeater>
		</div>
        <%if (!SimpleTicket) {%>
        <div>
            <a href="http://www.modetour.com/event/plan/detail.aspx?mloc=07&eidx=9468" target="_blank"><img src="//img.modetour.co.kr/ModeLiveBooking/air/main/list_banner_180621.jpg" alt="간편가입여행자보험" style="vertical-align: middle;border:0;width:800px;height:200px;"/></a>
        </div>
        <div>
            <a href="https://kt.com/ukb5" target="_blank"><img src="//img.modetour.com/modetour/2018/air/banner_191028.png" alt="로밍에그20%할인!" style="vertical-align: middle;border:0;width:800px;height:200px;"/></a>
        </div>
        <div>
            <a href="http://www.modetour.com/event/plan/detail.aspx?mloc=07&eidx=10699" target="_blank"><img src="//img.modetour.co.kr/modetour/2018/air/banner_20190930.jpg" alt="스페셜현지투어" style="vertical-align: middle;border:0;width:800px;height:200px;"/></a>
        </div>
        <div>
            <a href="http://roaming.modoo.at/" target="_blank"><img src="//img.modetour.co.kr/ModeLiveBooking/air/main/list_banner_160711.jpg" alt="해외로밍가이드" style="vertical-align: middle;border:0;width:800px;height:200px;"/></a>
        </div>
        <%}%>
		<div>
			<h3 style="margin:7px 0 5px; font-weight:bold; font-size:14px; line-height:normal; color:#333; text-align:left;">■ e-티켓 조건 사항<span style="font-weight:normal; color:#818181;">(e-Ticket Condition Details)</span></h3>
			<p style="margin-bottom:10px; font-size:14px; height:19px; line-height:19px;">본 e-티켓 확인증과 함께 제공된 계약조건을 반드시 참고하여 주시기 바랍니다.</p>
			<div style="padding:5px; font-size:12px; border:1px solid #ddd;">
				<ul style="list-style:none;font-family:dotum,'돋움',verdana;">
					<li>▶ 대부분의 공항에서 탑승 수속 마감시간은 해당 항공편 출발 1 시간 전(미주,유럽출발/도착편은 그 이상)으로 되어 있으니, 해당 항공편 출발 예정시각 최소 2시간 전에는 공항에서 도착하시기 바랍니다.</li>
                    <li style="font-weight:bold;">중요추가)) 2017.10.26일부로 미국, 미국경유,미국령(사이판,괌) 을 여행하는 승객을 대상으로 보안검색 강화를 위해 탑승전 보안인터뷰를 시행할 예정입니다. 이에 해당되는 승객 께서는 출발 4~5시간전부터 공항에 도착하셔야 안전하게 탑승하실수 있습니다.</li>
                    <li>▶ 일부 공동 운항편의 경우 탑승 수속은 운항 항공사의 카운터에서 이루어지며, 운항항공사의 규정에 따라 탑승 수속 마감시간이 다를 수 있습니다.</li>
                    <li>▶ e-티켓 확인증은 탑승수속시, 입출국/세관 통과시 제시하도록 요구될 수 있으므로 반드시 전 여행기간 동안 소지하시기 바랍니다 . e-티켓 확인증의 이름과 여권상의 이름은 반드시 일치해야 합니다.</li>
					<li>▶ 본 e-티켓 확인증은 e-티켓의 정보 등을 확인하기 위하여 제공되는 서면에 불과하고 소지인에게 당해 운송 관련 어떠한 법적 권리를 부여 하지 않습니다.</li>
					<li>▶ 본 e-티켓 확인증을 임의로 수정 및 사용시 모두투어는 책임을 지지 않습니다.</li>
					<li>▶ 항공사가 제공하는 운송 및 기타 서비스는 운송 약관에 준하며, 필요시 참조하실 수 있습니다. 이 약관은 발행 항공사를 통해 확인하실 수 있습니다.</li>
					<li>▶ 출/귀국 72시간 전 항공사로 반드시 스케줄 재확인 하시기 바랍니다. 항공사 사정에 의해 변경된 일정에 대해서는 여행사에서 책임지지 않습니다. 모두투어는 변경사항이 확인되는 대로 최대한의 공지 노력을 다하겠습니다.</li>
					<li>▶ 사정으로 인한 변경 요청시 한국시간 기준 평일 오전 9:00 ~ 오후 5:00 (주말 및 공휴일은 휴무)에 처리가 가능합니다.</li>
					<li>▶ 이외 시간에 요청하신 고객문의사항은 처리되지 않을 수 있습니다. 또한, 주말에는 한정된 업무만 가능합니다. 양해 부탁드립니다.</li>
					<li>▶ 해당 항공사에 중복예약(모두투어 항공예약 또는 이외 여행사를 통한 예약)으로 인해 발권 이후에도 항공사에 의해  자동 취소될 수 있으며, 이에 대하여 모두투어는 책임지지 않습니다</li>
				</ul>
			</div>
		</div>
        <%if (!SimpleTicket) {%>
		<div>
			<h3 style="margin:7px 0 5px; font-weight:bold; font-size:14px; line-height:normal; color:#333; text-align:left;">■ 법적고지</h3>
			<div style="padding:5px;border:1px solid #ddd;">
				<p style="padding:0;margin:0;font-size:12px;line-height:18px;font-family:dotum,'돋움',verdana;">여객의 최종목적지 또는 도중착륙지가 출발국 이외의 타국 내의 일개 지점일 경우, 해당 여객은 출발지 국 또는 목적지 국 내의 구간을 포함한 전체 여행에 대하여 소위 몬트리올 협약, 또는 수정된 바르샤바 협약 체제를 포함한 선행 협약인 바르샤바 협약으로 알려진 국제 협약들의 규정이 적용될 수 있음을 알려 드립니다. 이러한 여객들을 위하여, 적용 가능한 태리프에 명시된 특별 운송 계약을 포함한 제 협약은 운송인의 책임을 규정하고 제한하기도 합니다.</p>
				<h4 style="line-height:19px;font-family:dotum,'돋움',verdana;text-align:center;"><strong>책임제한에관한고지</strong></h4>
				<p style="padding:0;margin:0;font-weight:bold;font-size:12px;line-height:19px;font-family:dotum,'돋움',verdana;">몬트리올협약또는바르샤바협약체제에속한협약이귀하의여행에적용될수있으며, 이러한협약들은사망또는신체상해, 수하물의분실또는손상, 운송지연등에대하여항공운송인의책임을제한할수있습니다. 몬트리올협약이적용되는경우, 책임한도는다음과같습니다.</p>
				<ol style="padding:0 20px;margin:10px 0;list-style:none;font-size:12px;line-height:19px;font-family:dotum,'돋움',verdana;letter-spacing:-1px;">
					<li>1. <strong>사망및신체상해의경우운송인의손해배상액에는제한이없습니다.</strong></li>
					<li>2. <strong>수화물의파괴, 분실, 손상및지연의경우, 대부분의경우여객 1인당 1,131 SDR로 (약 1,200 유로, 1,800 US달러상당액) 제한됩니다.</strong></li>
					<li>3. <strong>지연으로인한손해에관하여는대부분의경우여객 1인당 4,694 SDR로 (약 5,000 유로, 7,500 US 달러상당액) 제한됩니다.</strong></li>
				</ol>
				<p style="padding:0;margin:0;font-size:12px;line-height:18px;"><strong>EC Regulation 889/2002는유럽연합회원국운송인들에게여객및수하물의운송에대하여몬트리올협약의책임제한에관한조항들이적용되도록규정하고있습니다. 유럽연합이외지역의다수운송인들도승객과수하물의운송에대하여몬트리올협약의규정을따르고있습니다.</strong></p>
				<p>바르샤바 협약 체제에 속한 협약이 적용되는 경우 다음의 책임 한도액이 적용됩니다.</p>
				<ol style="padding:0 20px;margin:10px 0;list-style:none;font-size:12px;line-height:19px;font-family:dotum,'돋움',verdana;letter-spacing:-1px;">
					<li><strong>1.</strong>여객의 사망 및 신체 상해에 대하여 헤이그 의정서에 의하여 개정된 협약이 적용되는 경우 책임 한도액은 16,600 SDR (약 20,000유로, 20,000US달러 상당액), 바르샤바 협약이 적용되는 경우에는 8,300 SDR (약 10,000유로, 10,000 US 달러 상당액) 로 제한됩니다. 다수의 운송인들은 자발적으로 이러한 책임 제한을 포기한 바 있으며, 미국의 관련 법규는 미국을 출발, 도착지로 하거나 미국 내에 예정된 기항지가 있는 여행의 경우 책임 한도액을 75,000 US 달러 보다 많을 수도 있도록 요구하고 있습니다.</li>
					<li><strong>2.</strong>위탁 수하물의 분실, 손상 또는 지연에 대하여는 킬로그램 당 17 SDR (약 20유로, 20 US 달러 상당액), 휴대 수하물은 332 SDR (약 400유로, 400 US 달러 상당액).</li>
					<li><strong>3.</strong>운송인은 지연으로 인한 손해에 대하여 책임을 부담할 수도 있습니다.</li>
				</ol>
				<p style="padding:0;margin:0;font-weight:bold;font-size:12px;line-height:18px;">항공여행에적용될책임한도에관한자세한사항은해당운송인으로부터제공받으실수있습니다.</p>
				<p style="padding:0;margin:0;font-weight:bold;font-size:12px;line-height:18px;">다수의운송인들이포함된여정일경우, 적용될책임한도에대하여각운송인에게문의하시기바랍니다.</p>
				<p style="padding:0;margin:0;font-weight:bold;font-size:12px;line-height:18px;">귀하의여행에어떠한협약이적용되든지, 여객은탑승수속시수하물의가격을신고하고추가요금을지불함으로써수하물의분실,<br />손상또는지연에대하여높은책임한도액을적용받을수있습니다. 또한대안으로써,<br />귀하의수하물의가치가적용가능한책임한도액을초과하는경우, 여행전에충분한보험에가입하시기바랍니다.</p>
				<p style="padding:0;margin:0;font-size:12px;line-height:18px;">제소 기간 : 손해 배상을 위한 소송은 항공기가 도착한 날 또는 도착되었어야 할 날짜로부터 2년 내에 제기되어져야 합니다. <br />수하물 배상 청구 : 수하물 손상의 경우 운송인으로의 통보는 위탁 수하물을 수령한 날짜로부터 7 일 이내에, 지연의 경우에는 여객이 수하물을 처분할 수 있게 된 날짜로부터 21일 이내에 서면으로 하셔야 합니다.</p>
				<h4 style="margin:10px 0;text-align:center;">준용되는계약조건의고지</h4>
				<ol style="padding:0 10px;list-style:none;font-size:12px; line-height:19px;">
					<li>1.국제 여행, 국내 여행, 또는 국내 구간이 포함 된 국제 여행에 있어서, 귀하의 운송계약은 본 통지 또는 운송인의 통지, 확인증 그리고 운송인의 개별 계약조건, 관련규칙, 규정 및 해당 운임의 적용을 받게 됩니다.</li>
					<li>2.다수의 운송인을 포함하는 여정이라면, 각 운송인별로 상이한 조건, 규정, 그리고 이에 상응하는 요금 규정이 적용 될 수 있습니다.</li>
					<li>3.이 통지에 의하여, 각 운송인들의 계약 조건과 규정 및 적용 요금은 귀하와의 계약의 일부로서 전체 운송계약에 포함됩니다.</li>
					<li>
						<p>4.계약 조건은 다음 사항들을 포함할 수 있습니다. 그러나 아래 열거한 사항들에만 국한되는 것은 아닙니다.</p>
						<ul style="padding:0 10px;list-style:none;">
							<li>• 여객의 신체 상해나 사망 시 운송인의 책임 조건과 한계</li>
							<li>• 쉽게 손상되거나 부패될 수 있는 물품을 포함한, 여객의 물품이나 수하물의 분실, 훼손, 지연에 따른 운송인의 책임 조건과 한계</li>
							<li>• 고가의 수하물 신고와 추가 요금 지불에 관한 규정</li>
							<li>• 운송인에게 재화나 용역을 제공 하는 개인을 포함한 에이젼트, 고용인, 대리인의 행위에 관한 계약 조건과 책임 한계의 적용</li>
							<li>• 여객이 운송인에 대하여 제소나 청구를 할 수 있는 기간을 포함한 청구권에 관한 제한 사항</li>
							<li>• 예약 및 예약 재확인에 관한 규정; 탑승수속시간; 항공 운송 서비스의 유효성 및 유효기간; 운송인의 운송 거부권</li>
							<li>• 관련법에 근거하여 필요할 경우 여객에게 운항항공사 또는 대체 항공편을 고지 해야 하는 의무, 그리고 운항 스케쥴의 변경, 대체 운송인, 항공기 및 여정 변경을 포함하는 운송서비스의 지연 또는 불이행에 대한 운송인의 권한과 책임의 한계</li>
							<li>• 관련법에 의한 여행 부적격자 또는 일체의 여행 관련 구비 서류를 준비하지 않은 여객의 운송을 거부할 수 있는 운송인의 권리</li>
						</ul>
					</li>
					<li>5. 귀하의 운송계약에 관한 더욱 자세한 정보 및 사본은 항공운송권 판매처에서 얻으실 수 있습니다. 많은 운송인들은 자사의 웹사이트에 이러한 정보들을 올려 놓았습니다. 법에 의해 필요 시, 귀하는 운송인의 공항과 판매처에서 운송계약서의 세부 내용을 열람할 권리가 있고, 귀하께서 요청하실 경우, 우편 또는 다른 배송 방법으로 각 운송인으로부터 사본을 무료로 받아보실 수 있습니다.</li>
					<li>6. 만약 운송인이 다른 운송인의 항공운송서비스를 판매하거나 또는 수하물을 확인하는 경우, 이는 그 운송인의 대리인의 자격으로서 수행하는 것입니다.</li>
				</ol>
				<div style="padding:0;margin:20px 0 10px;font-size:12px;line-height:19px;">
					<p style="margin:5px 0;"><strong>여권이나비자와같은모든여행구비서류없이는여행을하실수없습니다.</strong></p>
					<p style="margin:5px 0;"><strong>정부는귀하의운송인에게여객자료를열람할수있는권리가있으며, 여객정보제공을요청할수도있습니다.</strong></p>
					<p style="padding:0;margin:0 0 20px;font:bold 12px/19px dotum,'돋움',verdana;">탑승거부 : 항공편이초과예약되어, 예약이확약되었더라도좌석부족이될수도있습니다. 이러한경우, 여객이강제로탑승을거부당했을시, 보상을받을수있도록되어있습니다. 법에의해필요시, 운송인은불특정여객에대한탑승거부이전에자발적인탑승포기자를찾아보아야합니다. 탑승거부에대한보상제도및전체규정과탑승우선권에관한정보를운송인에게확인하시기바랍니다.
						수하물안내 : 특정종류의물품은한도를초과하여신고할수도있습니다. 운송인은파손, 훼손되기쉽거나값비싼물품에관하여는특별규정을적용할수있습니다. 운송인에게확인하시기바랍니다.
						위탁수하물 : 운송인은무료로위탁수하물을허용하며, 허용한도는좌석등급과노선에따라다릅니다. 운송인은허용한도를초과한위탁수하물에대하여추가운임을청구할수있습니다. 운송인에게확인하시기바랍니다.
						기내반입휴대수하물 : 운송인은무료로기내반입휴대수하물을허용하며, 한도는좌석등급과노선, 항공기종류에따라다릅니다. 기내반입휴대수하물을최소한으로줄여주시기바랍니다. 운송인에게확인하시기바랍니다. 귀하의여정이둘이상의운송인에의하여제공된다면, 각운송인마다다른수하물규정이적용될수도있습니다. (위탁수하물, 기내반입휴대수하물) <i style="font-family:gulim, '굴림', sans-serif;">미국여행시특별수하물책임한도 : 미국내국내지점간여행일경우, 미연방정부규정상운송인의수하물배상책임한도액은최소한여객 1인당 3,300 US 달러이거나 14CFR 254.5에규정된금액을적용합니다.</i>
					</p>
					<p style="margin-bottom:20px;"><strong>탑승시간.</strong> 여정이나 영수증에 표기된 시간은 항공기의 출발 시간입니다. 항공기 출발 시간은, 여객이 체크 인해야 하는 시간 이거나 탑승할 수 있는 시간은 아닙니다. 여객이 늦을 경우 운송인은 여객의 탑승을 거부할 수도 있습니다. 귀하의 운송인이 안내한 체크인 시간은 여객이 여행을 위한 모든 탑승수속을 완료 하기 위한 최소한의 시간입니다. 귀하의 운송인이 안내한 탑승 시간은 여객이 항공기에 탑승을 하기 위하여 탑승구에 도착 해야 하는 시간입니다.</p>
					<p><strong>위험물품(위해물질).</strong> 안전상의이유로, 위험물품은 특별히 허가 받지 않은 이상 위탁 수하물 또는 기내 반입 수하물로 지참하실 수 없습니다. 위험물품에는 압축 가스, 부식성 물질, 폭발물, 가연성 액체 및 고체, 방사성 물질, 산화 물질, 유독성 물질, 전염성 물질 및 경보 장치가 부착된 서류 가방 등이 있습니다. 보안상의 이유로, 다른 제한 사항이 적용될 수도 있습니다. 귀하의 운송인에게 문의하시기 바랍니다.</p>
				</div>
				<p style="font-weight:bold;font-size:14px;text-align:center;">위험물품</p>
				<p style="font-weight:bold;font-size:14px;text-align:center;">운송인의확인없이아래그림과같은품목들을포장하거나지참하여탑승할수없습니다.</p>
				<p style="font-weight:bold;font-size:14px;text-align:center;"><img src="//www.iatatravelcentre.com/images/notices.jpg" alt=""/><br /><a href="http://www.iatatravelcentre.com/tickets" target="_blank">http://www.iatatravelcentre.com/tickets</a></p>
				<p style="font-weight:bold;font-size:14px;text-align:center;"><strong>귀하와다른여객들의안전에위해가초래되지않도록하여주십시오.</strong></p>
			</div>
		</div>
        <%}%>
	</div>
    <%if (PrintBtn) {%><div style="margin:0 auto;width:800px;overflow:hidden;" class="hidePrint"><a href="./ETicket.aspx" onclick="window.print();return false;" style="display:inline-block;float:right;margin-top:5px;"><img src="//img.modetour.co.kr/ModeLiveBooking/130913_air/btn_print.png" alt="인쇄하기" style="border:0;" /></a></div><%}%>
</body>
</html>