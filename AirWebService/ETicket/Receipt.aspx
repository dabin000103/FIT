<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Receipt.aspx.cs" Inherits="AirWebService.ETicket.Receipt" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "//www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--[if IE 7 ]><html class="no-js ie7" lang="ko"><![endif]-->
<!--[if IE 8 ]><html class="no-js ie8" lang="ko"><![endif]-->
<!--[if gt IE 8]><!--><html class="no-js no-ie" xmlns="//www.w3.org/1999/xhtml" xml:lang="ko"><!--<![endif]-->
<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html;charset=UTF-8" />
	<title>모두투어 항공권 카드전표</title>
	<link rel="Shortcut Icon" href="//img.modetour.co.kr/Modetour.ico" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/Include/css/warp.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/Include/css/Modetour/Modetour.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/Include/css/Modetour/common/common.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/Include/css/Modetour/common/GNB.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/Include/css/Modetour/common.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/include/css/modetour/jquery.mCustomScrollbar.css" />
	<link rel="stylesheet" type="text/css" href="//www.modetour.com/include/css/modetour/header.css" />
	<style type="text/css">
		body{line-height: 20px;}
		.receipt{padding: 15px 20px; width: 360px; margin: 0 auto;}
		.receipt .title{height: 53px; padding-bottom: 5px; border-bottom: 3px #c6c6c6 solid;}
		.receipt .title h1{font-size: 17px; color: #303030; float: left; line-height: 53px;}
		.receipt .title h1 img{float: left; margin-right: 12px;}
		.receipt .title a{float: right; margin-top: 15px; display: block; width: 90px;}
		.receipt .title a img{width: 100%;}
		.receipt .table ul li{padding: 5px 0; border-bottom: 1px #e2e2e2 solid;}
		.receipt .table ul li.bd-none{border: none;}
		.receipt .table ul li.fix{overflow: hidden;}
		.receipt .table dl{padding: 2px 0 0 15px;}
		.receipt .table ul li.fix dl{float: left; width: 164px;}
		.receipt .table dl.bd{border-right: 1px #e2e2e2 solid;}
		.receipt .table dl dt{font-size: 12px; color: #0877c5; font-weight: bold;}
		.receipt .table dl dd{font-size: 13px; color: #5e5e5e;}
		.receipt .pay_box{position: relative; margin: 5px auto 10px;}
		.receipt .pay_box img{width: 100%;}
		.receipt .pay_box .payment{padding: 7px 15px; position: absolute; top: 0; left: 0; right: 0;}
		.receipt .pay_box .payment li:first-child{border: none;}
		.receipt .pay_box .payment li{border-top: 1px #c2dbd7 solid; overflow: hidden; padding: 2px 10px;}
		.receipt .pay_box .payment dl dt{font-size: 13px; color: #0877c5; float: left;}
		.receipt .pay_box .payment dl dd{font-size: 14px; color: #5e5e5e; float: right;}
		.receipt .pay_box .payment dl dd.price{font-size: 14px; color: #ff2333; font-weight: bold;}
		.receipt .title_box{position: relative;}
		.receipt .title_box .bg{width: 100%;}
		.receipt .title_box .in_title{font-size: 14px; color: #5e5e5e; background-size: 100% auto; padding: 5px 18px; position: absolute; top: 0; left: 0; right: 0;}
		.receipt .title_box .in_title img{margin-right: 5px;}
		.receipt .seller_infor{margin: 10px 0;}
		.receipt .notice li{font-size: 13px; color: #5e5e5e; background: url('//img.modetour.com/modelivebooking/domestic_air/ico_list.png') 0 7px no-repeat; padding-left: 10px; margin-bottom: 5px;}
		.receipt a.close{width: 150px; margin: 20px auto 5px;display: block;}
		.receipt a.close img{width: 100%;}
		@page{ size:auto; margin : 13mm; }
		@media print{
			.receipt .title a img{display: none;}
			.receipt  a.close img{display: none;}
		}
		/*해외항공 카드영수증*/
		.receipt .total_box{position: relative;}
		.receipt .total_box .bg{width: 100%;}
		.receipt .total_box dl {height:40px;font-size: 16px; color: #0877c5; padding: 13px 18px; position: absolute; top: 0; left: 0; right: 0;}
		.receipt .total_box dt {float:left;}
		.receipt .total_box dd {float:right}
		.receipt .price{color: #ff2333; font-weight: bold;}
 	</style>
</head>
<body>
	<div class="receipt">
		<div class="title">
		 	<h1><img src="//img.modetour.com/modelivebooking/domestic_air/ico_tit01.png" alt="" />온라인 신용카드 매출전표</h1>
			<a href="./Receipt.aspx" onclick="window.print();return false;"><img src="//img.modetour.com/modelivebooking/domestic_air/btn_print.png" alt="인쇄하기" /></a>
		 </div>	
		 <div class="table">	
			<ul>
			 	<li class="fix">
			 		<dl class="bd">
			 			<dt>항공예약번호</dt>
			 			<dd><asp:Literal ID="ltrOrderNumber" runat="server" /></dd>
			 		</dl>
			 		<dl>
			 			<dt>항공사</dt>
			 			<dd><asp:Literal ID="ltrAirline" runat="server" /></dd>
			 		</dl>
			 	</li>
			 	<li>
			 		<dl>
			 			<dt>항공여정</dt>
			 			<dd><asp:Literal ID="ltrItinerary" runat="server" /></dd>
			 		</dl>
			 	</li>
			 	<li>
			 		<dl>
			 			<dt>승객명</dt>
			 			<dd><asp:Literal ID="ltrPaxInfo" runat="server" /></dd>
			 		</dl>
			 	</li>
			 	<li class="fix bd-none">
			 		<dl class="bd">
			 			<dt>거래유형</dt>
			 			<dd>신용카드</dd>
			 		</dl>
			 		<dl>
			 			<dt>거래일시</dt>
			 			<dd><asp:Literal ID="ltrPaymentDate" runat="server" /></dd>
			 		</dl>
			 	</li>
			</ul>
		</div><!-- table -->
		<div class="pay_box">	
			<img src="//img.modetour.com/modelivebooking/domestic_air/payment_bg2_180322.png" alt="" /> 
			<ul class="payment">
				<li>
					<dl>	
						<dt>운임</dt>
						<dd><asp:Literal ID="ltrFare" runat="server" /> 원</dd>
					</dl>
				</li>
				<li>
					<dl>	
						<dt>유류할증료</dt>
						<dd><asp:Literal ID="ltrFuelSurcharge" runat="server" /> 원</dd>
					</dl>
				</li>
				<li>
					<dl>	
						<dt>제세공과금</dt>
						<dd><asp:Literal ID="ltrTax" runat="server" /> 원</dd>
					</dl>
				</li>
				<li>
					<dl>	
						<dt>발권대행수수료</dt>
						<dd><asp:Literal ID="ltrTASF" runat="server" /> 원</dd>
					</dl>
				</li>
			</ul><!-- payment -->
		</div>
		<!-- [s]:신용카드결제금액시작 -->
		<div class="total_box">
			<img src="//img.modetour.com/modelivebooking/domestic_air/title_bg2.png" alt="" class="bg" />
			<dl>
				<dt>총 결제금액</dt>
				<dd class="price"><asp:Literal ID="ltrPayment" runat="server" /></dd>
			</dl>	
		</div>
		<!-- [s]:신용카드결제금액 끝 -->		
		<!-- [s]:판매자정보시작 -->
		<div class="seller_infor">
			<div class="title_box">
				<img src="//img.modetour.com/modelivebooking/domestic_air/title_bg.png" alt="" class="bg" />
				<h3 class="in_title"><img src="//img.modetour.com/modelivebooking/domestic_air/ico_list.png" alt="" />판매자 정보</h3>	
			</div>
			<div class="table">
				<ul>
					<li class="fix">
						<dl class="bd">
				 			<dt>상호</dt>
				 			<dd>(주)모두투어네트워크</dd>
				 		</dl>
				 		<dl>
				 			<dt>사업자등록번호</dt>
				 			<dd>202-81-45295</dd>
				 		</dl>
					</li>
					<li class="fix">
						<dl class="bd">
				 			<dt>대표자명</dt>
				 			<dd>우종웅</dd>
				 		</dl>
				 		<dl>
				 			<dt>이용문의</dt>
				 			<dd>1544-5353</dd>
				 		</dl>
					</li>
				</ul>
			</div>
		</div><!-- seller_infor -->
		<!-- [e]:판매자정보끝 -->
		<ul class="notice">
			<li>본 매출전표는 금액수령을 확인/영수하는 효력만 있으며,<br />세법상 지출을 증빙하는 효력은 없습니다.</li>
			<li>정확한 영수증이 필요하신 분은 1544-5353으로 부탁 드립니다.</li>
		</ul><!-- notice -->
		<a href="./Receipt.aspx" onclick="window.close();return false;" class="close"><img src="//img.modetour.com/modelivebooking/domestic_air/btn_close.png" alt="닫기" /></a>
	</div>
</body>
</html>