<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ETicketEmailPage.aspx.cs" Inherits="AirWebService.ETicket.ETicketEmailPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="ko">
<head>
	<meta http-equiv="Content-Type" content="text/html;charset=UTF-8" />
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
	<div style="margin:0 auto; width:800px;font-family:dotum, '돋움', Helvetica,Arial,sans-serif;">
		<h1 style="margin-bottom:30px; padding-bottom:8px; border-bottom:2px solid #016f54;"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/e_ticket_logo.png" alt="모두투어 항공권" /></h1>
        <%if (ULC.Equals("en")) {%>
        <div style="margin-bottom:24px;padding-top:34px;padding-bottom:40px;text-align:center;border:5px solid #dfdfdf;">
			<h2 style="margin-bottom:18px;font-weight:bold;font-size:26px; color:#111;"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/eng_e_ticket_h1.png" alt="This is mail for your e-ticket." /></h2>
			<p style="font-size:14px;line-height:1.5em;color:#666;"><strong style="font-weight:bold;color:#12af3e;">Dear, <asp:Literal ID="ltrEPaxName2" runat="server" /></strong></p>
			<p style="font-size:14px;line-height:1.5em;color:#666;">Thank you for your booking through thePAY and MODETOUR. You can check and print your flight ticket we issued.<br />Please, reconfirm your name, schedule etc. on your flight ticket and print it to use on departure date in the airport.</p>
		</div>
		<p style="font-size:14px;color:#666;"><strong style="font-weight:bold;font-size:14px;color:#2380ff;">[<asp:Literal ID="ltrEPaxName3" runat="server" />]</strong> <a href="<%=ETicketURL%>" target="_blank"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/eng_btn_eticket_cnfm.png" alt="Confirm E-ticket" border="0" style="vertical-align:middle;" /></a></p>
		<%} else {%>
		<div style="margin-bottom:24px;padding-top:34px;padding-bottom:40px;text-align:center;border:5px solid #dfdfdf;">
			<h2 style="margin-bottom:18px;font-weight:bold;font-size:26px; color:#111;"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/e_ticket_h1.png" alt="전자항공권(E-티켓)이용 안내 메일입니다." /></h2>
			<p style="font-size:14px;line-height:1.5em;color:#666;"><strong style="font-weight:bold;color:#12af3e;"><asp:Literal ID="ltrKPaxName" runat="server" /></strong>고객님, 안녕하세요.</p>
			<p style="font-size:14px;line-height:1.5em;color:#666;"><strong style="font-weight:bold;color:#12af3e;"><asp:Literal ID="ltrAgentName" runat="server" /></strong>에서 모두투어항공권을 이용해주셔서 감사드리며, 전자항공권을 확인하실 수 있습니다.</p>
		</div>
		<p style="font-size:14px;color:#666;"><strong style="font-weight:bold;font-size:14px;color:#2380ff;">[<asp:Literal ID="ltrEPaxName" runat="server" />]</strong> 님의 <a href="<%=ETicketURL%>" target="_blank"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/btn_eticket_cnfm.png" alt="전자항공권(E-티켓) 확인하기" border="0" style="vertical-align:middle;" /></a></p>
		<%}%>
        <h3 style="margin-top:25px;"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/e_ticket_h2.png" alt="전자항공권(E-티켓) 안내" /></h3>
		<ul style="list-style-type:none;margin-top:12px;">
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">전자항공권 확인증은 탑승수속시, 입출국/세관 통과 시 제시하도록 요구될 수 있으니, 법적고지문을 확인하신 후 반드시 전자항공권 확인증을 출력하시고, <i class="space"></i>전 여행기간 동안 소지하여 주십시오.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">전자항공권 확인증의 이름과 여권상의 이름이 일치하는지 확인하여 주십시오.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">탑승수속 마감시각은 해당 항공편의 출발 1시간 전으로 되어 있습니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">규정시간까지 출국수속을 마치실 수 있도록, 항공기 출발 2~3시간 전까지 공항에 도착하여 주십시오.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">미주, 유럽지역에서 출발하는 항공편 탑승 고객께서는 해당 항공편 출발 60분 전까지 탑승수속을 완료하여 주시기 바랍니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">본 전자항공권 확인증을 임의로 수정 및 사용시 모두투어는 책임을 지지 않습니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">항공사가 제공하는 운송 및 기타 서비스는 운송 약관에 준하며, 필요시 참조하실 수 있습니다. 이 약관은 발행 항공사를 통해 확인하실 수 있습니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">출/귀국 72시간 전 항공사로 반드시 스케줄 재확인 하시기 바랍니다. 항공사 사정에 의해 변경된 일정에 대해서는 여행사에서 책임지지 않습니다. 모두투어는 변경사항이 확인되는 대로 최대한의 공지 노력을 다하겠습니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">해당 항공사에 중복 예약(모두투어 항공예약 또는 이외 여행사를 통한 예약)으로 인해 발권 이후에도 항공사에 의해 자동 취소될 수 있으며, 이에 대하여 모두투어는 책임지지 않습니다</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">스케줄 변경 및 기타 기내서비스 요청사항의 변경은 탑승전까지 발생할 수 있습니다. 반드시 담당자나 공항, 항공사에 스케줄 재확인 하시기 바랍니다. 항공사 사정에 의해 변경된 일정에 대해서는 여행사에서 책임지지 않습니다.</li>
			<li style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">투어마일리지(모두투어 회원) 적립은 출발전 회원가입 후 적립요청을 하셨을 경우에 가능합니다</li>
		</ul>
		<dl style="margin-top:40px;">
			<dt style="margin-bottom:10px;font-size:12px;color:#333;">&lt; 쿠폰 사용 순서 &gt;</dt>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">탑승용 쿠폰 또는 전자 쿠폰은 항공권에 명시된 출발지부터 순서대로 사용되어야 합니다.</dd>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">항공권상에 명시된 순서대로 사용하지 않을 경우 항공권은 운송을 위해 접수될 수 없으며, 환불 또는 무효로 처리됩니다.</dd>
		</dl>
		<dl style="margin-top:40px;">
			<dt style="margin-bottom:10px;font-size:12px;color:#333;">&lt; 변경 환불 업무 관련 &gt;</dt> 
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">전자항공권이 예약하신 내용과 일치하는지 반드시 미리 확인하시기 바랍니다.</dd>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">발권된 전자항공권의 변경 및 환불은 모두투어 주중 업무시간에만 가능합니다.</dd>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">업무시간 외 시간에 요청하신 고객문의사항은 처리되지 않을 수 있습니다. 업무시간 외에 요청하신 변경 환불 관련 피해에 대한 책임은 모두투어에서 지지 않습니다.</dd>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">평일 변경 환불 업무시간 : 09:00~16:00 / 주말불가</dd>
		</dl>
		<dl style="margin-top:20px;">
			<dt style="margin-bottom:10px;font-size:12px;color:#333;">&lt; 항공기 서비스 관련 &gt;</dt>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">좌석배정요청 불가 항공사 : 제주항공/진에어/이스타항공/에어부산/티웨이항공 및 유료서비스인 항공사, 특가좌석인 경우</dd>
			<dd style="margin-bottom:7px;padding-left:16px;font-size:12px;color:#333;background:url(http://img.modetour.com/modelivebooking/2014/e_ticket/ico_chk.png) no-repeat 0 4px;">대한항공과 아시아나항공의 경우 회원번호를 알려주시면 해당항공사 홈페이지에서 확인이 가능하게 처리하여 드립니다.</dd>
		</dl> 
		<h3 style="margin-top:25px;"><img src="http://img.modetour.com/modelivebooking/2014/e_ticket/e_ticket_h2_2.png" alt="법적고지문" /></h3>
		<pre style="margin-top:13px;font-size:12px; color:#333;white-space:pre-wrap;">계약 조건 및 중요 안내 사항
여객의 최종 목적지 또는 도중 착륙지가 출발 국 이외의 타국 내의 일개 지점일 경우, 해당 여객은 출발지 국 또는 목적지 국 내의 구간을 포함한 전체 여행에 대하여 소위 몬트리올 협약, 또는 수정된 바르샤바 협약 체제를 포함한 선행 협약인 바르샤바 협약으로 알려진 국제 협약들의 규정이 적용될 수 있음을 알려드립니다. 이러한 여객들을 위하여, 적용 가능한 태리프에 명시된 특별 운송 계약을 포함한 국제 협약은 운송인의 책임을 규정하고 제한하기도 합니다.
책임 제한에 관한 고지 몬트리올 협약 또는 바르샤바 협약 체제에 속한 협약이 귀하의 여행에 적용될 수 있으며, 이러한 협약들은 사망 또는 신체 상해, 수하물의 분실 또는 손상, 운송 지연 등에 대하여 항공 운송인의 책임을 제한할 수 있습니다. 몬트리올 협약이 적용되는 경우, 책임 한도는 다음과 같습니다.
1.사망 및 신체 상해의 경우 운송인의 손해 배상액에는 제한이 없습니다.
2.수화물의 파괴, 분실, 손상 및 지연의 경우, 대부분의 경우 여객 1인당 1,131 SDR (약 1,200 유로, 1,800 US달러 상당액)로 제한됩니다.
3.지연으로 인한 손해에 관하여는 대부분의 경우 여객 1인당 4,694 SDR (약 5,000 유로, 7,500 US달러 상당액)로 제한됩니다.


EC Regulation 889/2002는 유럽 연합 회원국 운송인들에게 여객 및 수하물의 운송에 대하여 몬트리올 협약의 책임 제한에 관한 조항들이 적용되도록 규정하고 있습니다. 유럽연합 이외 지역의 다수 운송인들도 승객과 수하물의 운송에 대하여 몬트리올 협약의 규정을 따르고 있습니다. 바르샤바 협약 체제에 속한 협약이 적용되는 경우 다음의 책임 한도액이 적용됩니다.
1.여객의 사망 및 신체 상해에 대하여 헤이그 의정서에 의하여 개정된 협약이 적용되는 경우 책임
한도액은 16,600 SDR(약 20,000 유로, 20,000 US달러 상당액), 바르샤바 협약이 적용되는 경우에는 8,300 SDR(약 10,000 유로, 10,000 US달러 상당액)로 제한 됩니다.
다수의 운송인들은 자발적으로 이러한 책임 제한을 포기한 바 있으며, 미국의 관련 법규는 미국을 출발, 도착지로 하거나 미국 내에 예정된 기항지가 있는 여행의 경우 책임 한도액을 75,000 US달러 보다 많을 수도 있도록 요구하고 있습니다.
2.위탁 수하물의 분실, 손상 또는 지연에 대하여는 킬로그램 당 17 SDR(약 20유로, 20 US 달러 상당액), 휴대 수하물은 332 SDR(약 400유로, 400 US 달러 상당액).
3.운송인은 지연으로 인한 손해에 대하여 책임을 부담할 수도 있습니다. 항공 여행에 적용될 책임 한도에 관한 자세한 사항은 해당 운송인으로부터 제공 받으실 수 있습니다.


다수의 운송인들이 포함된 여정일 경우, 적용될 책임 한도에 대하여 각 운송인에게 문의하시기 바랍니다.
귀하의 여행에 어떠한 협약이 적용되든지, 여객은 탑승 수속 시 수하물의 가격을 신고하고 추가 요금을 지불함 으로써 수하물의 분실, 손상 또는 지연에 대하여 높은 책임 한도액을 적용 받을 수 있습니다. 또한 대안으로써, 귀하의 수하물의 가치가 적용 가능한 책임 한도액을 초과하는 경우, 여행 전에 충분한
보험에 가입하시기 바랍니다.
 제소 기간 : 손해 배상을 위한 소송은 항공기가 도착한 날 또는 도착되었어야 할 날짜로부터 2년 내에 제기되어져야 합니다.
수하물 배상 청구 : 수하물 손상의 경우 운송인으로의 통보는 위탁 수하물을 수령한 날짜로부터 7일 이내에, 지연의 경우에는 여객이 수하물을 처분할 수 있게 된 날짜로부터 21일 이내에 서면으로 하셔야 합니다. 위탁 수하물의 분실, 손상 또는 지연에 대하여는 킬로그램 당 17 SDR(약 20유로, 20 US 달러 상당액), 휴대 수하물은 332 SDR(약 400유로, 400 US 달러 상당액).


준용되는 계약 조건의 고지 
1.국제 여행, 국내 여행, 또는 국내 구간이 포함된 국제 여행에 있어서, 귀하의 운송 계약은 본 통지 또는 운송인의 통지, 확인증 그리고 운송인의 개별 계약 조건, 관련 규칙, 규정 및 해당 운임의 적용을 받게됩니다.
2.다수의 운송인을 포함하는 여정이라면, 각 운송인 별로 상이한 조건, 규정, 그리고 이에 상응하는 요금 규정이 적용될 수 있습니다.
3.이 통지에 의하여, 각 운송인들의 계약 조건과 규정 및 적용 요금은 귀하와의 계약의 일부로서 전체 운송 계약에 포함됩니다.
4.계약 조건은 다음 사항들을 포함할 수 있습니다. 그러나 아래 열거한 사항들에만 국한되는 것은 아닙니다.
- 여객의 신체 상해나 사망 시 운송인의 책임 조건과 한계
- 쉽게 손상되거나 부패될 수 있는 물품을 포함한, 여객의 물품이나 수하물의 분실, 훼손, 지연에 따른 운송인의 책임 조건과 한계 - 고가의 수하물 신고와 추가 요금 지불에 관한 규정
- 운송인에게 재화나 용역을 제공 하는 개인을 포함한 에이전트, 고용인, 대리인의 행위에 관한 계약 조건과 책임 한계의 적용
- 여객이 운송인에 대하여 제소나 청구를 할 수 있는 기간을 포함한 청구권에 관한 제한 사항 
- 예약 및 예약 재확인에 관한 규정; 탑승 수속 시간; 항공 운송 서비스의 유효성 및 유효 기간; 운송인의 운송 거부권
- 관련법에 근거하여 필요할 경우 여객에게 운항 항공사 또는 대체 항공편을 고지 해야 하는 의무, 그리고 운항 스케줄의 변경, 대체 운송인, 항공기 및 여정 변경을 포함하는 운송 서비스의 지연 또는 불이행에 대한 운송인의 권한과 책임의 한계
- 관련법에 의한 여행 부적격자 또는 일체의 여행 관련 구비 서류를 준비하지 않은 여객의 운송을 거부할 수 있는 운송인의 권리
5.귀하의 운송 계약에 관한 더욱 자세한 정보 및 사본은 항공 운송 권 판매처에서 얻으실 수 있습니다.
많은 운송인들은 자사의 웹사이트에 이러한 정보들을 올려 놓았습니다. 법에 의해 필요 시, 귀하는 운송인의 공항과 판매처에서 운송 계약서의 세부 내용을 열람할 권리가 있고, 귀하께서 요청하실 경우, 우편 또는 다른 배송 방법으로 각 운송인으로부터 사본을 무료로 받아보실 수 있습니다. 
6.만약 운송인이 다른 운송인의 항공 운송 서비스를 판매하거나 또는 수하물을 확인하는 경우, 이는 그 운송인의 대리인의 자격으로서 수행하는 것입니다.
여권이나 비자와 같은 모든 여행 구비 서류 없이는 여행을 하실 수 없습니다. 

정부는 귀하의 운송인에게 여객 자료를 열람할 수 있는 권리가 있으며, 여객 정보 제공을 요청할 수도 있습니다.
탑승 거부 : 항공편이 초과 예약되어, 예약이 확약되었더라도 좌석 부족이 될 수도 있습니다. 이러한 경우, 여객이 강제로 탑승을 거부 당했을 시, 보상을 받을 수 있도록 되어있습니다. 법에 의해 필요 시, 운송인은 불특정 여객에 대한 탑승 거부 이전에 자발적인 탑승 포기 자를 찾아보아야 합니다. 탑승 거부에 대한 보상 제도 및 전체 규정과 탑승 우선권에 관한 정보를 운송인에게 확인하시기 바랍니다. 
수하물 안내 : 특정 종류의 물품은 한도를 초과하여 신고할 수도 있습니다. 운송인은 파손, 훼손되기 쉽거나 값비싼 물품에 관하여는 특별 규정을 적용할 수 있습니다. 운송인에게 확인하시기 바랍니다. 
위탁 수하물 : 운송인은 무료로 위탁 수하물을 허용하며, 허용 한도는 좌석 등급과 노선에 따라 다릅니다. 운송인은 허용 한도를 초과한 위탁 수하물에 대하여 추가 운임을 청구할 수 있습니다. 운송인에게 확인하시기 바랍니다.
기내 반입 휴대 수하물 : 운송인은 무료로 기내 반입 휴대 수하물을 허용하며, 한도는 좌석 등급과 노선, 항공기 종류에 따라 다릅니다. 기내 반입 휴대 수하물을 최소한으로 줄여주시기 바랍니다. 운송인에게 확인하시기 바랍니다. 귀하의 여정이 둘 이상의 운송인에 의하여 제공된다면, 각 운송인마다 다른 수하물 규정이 적용될 수도 있습니다. (위탁 수하물, 기내 반입 휴대 수하물)
미국 여행시 특별 수하물 책임 한도 : 미국 내 국내 지점간 여행일 경우, 미연방정부 규정상 운송인의 수하물 배상 책임 한도액은 최소한 여객 1인당 3,300 US달러이거나 14CFR 254.5에 규정된 금액을 적용합니다.
탑승 시간.
여정이나 영수증에 표기된 시간은 항공기의 출발 시간입니다. 항공기 출발 시간은, 여객이 체크인해야 하는 시간이거나 탑승할 수 있는 시간은 아닙니다. 여객이 늦을 경우 운송인은 여객의 탑승을 거부할 수도 있습니다. 귀하의 운송인이 안내한 체크인 시간은 여객이 여행을 위한 모든 탑승 수속을 완료 하기위한 최소한의 시간입니다. 귀하의 운송인이 안내한 탑승 시간은 여객이 항공기에 탑승을 하기 위하여 탑승구에 도착해야 하는 시간입니다.

위험 물품(위해 물질).
안전상의 이유로, 위험 물품은 특별히 허가 받지 않은 이상 위탁 수하물 또는 기내 반입 수하물로 지참하실 수 없습니다. 위험 물품에는 압축 가스, 부식성 물질, 폭발물, 가연성 액체 및 고체, 방사성 물질, 산화 물질, 유독성 물질, 전염성 물질 및 경보 장치가 부착된 서류 가방 등이 있습니다. 보안상의 이유로, 다른 제한 사항이 적용될 수도 있습니다. 귀하의 운송인에게 문의하시기 바랍니다.

위험 물품 
운송인의 확인 없이 아래 그림과 같은 품목들을 포장하거나 지참하여 탑승할 수 없습니다.
<a href="http://www.iatatravelcentre.com/tickets" target="_blank">http://www.iatatravelcentre.com/tickets</a>

귀하와 다른 여객들의 안전에 위해가 초래되지 않도록 하여 주십시오.
보다 더 많은 정보는 귀하의 운송인에게 문의하시기 바랍니다.</pre>
	</div>
</body>
</html>