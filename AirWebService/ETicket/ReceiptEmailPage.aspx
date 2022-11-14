<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReceiptEmailPage.aspx.cs" Inherits="AirWebService.ETicket.ReceiptEmailPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>모두투어 항공권 카드전표 안내</title>
</head>
<body>
<div style="margin:63px 0;" align="center">
	<table width="800" border="0" align="center" cellpadding="0" cellspacing="0">
		<tr>
			<td colspan="3" style="width:800px;height:33px;padding:0;border:0;">
				<img src="http://img.modetour.com/modetour/2016/air_mail/img_mail_logo.gif" border="0" alt="모두투어 항공권" />
			</td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:30px;padding:0;border:0;"></td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:5px;padding:0;border:0;background:#dfdfdf;"></td>
		</tr>
		<tr>
			<td rowspan="3" style="width:5px;height:161px;padding:0;border:0;background:#dfdfdf;"></td>
			<td style="width:790px;height:75px;padding:0;border:0;">
				<img src="http://img.modetour.com/modetour/2016/air_mail/img_mail_tit2.gif" border="0" alt="카드전표 안내 메일입니다." />
			</td>
			<td rowspan="3" style="width:5px;height:161px;padding:0;border:0;background:#dfdfdf;"></td>
		</tr>
		<tr>
			<td style="width:790px;height:56px;padding:0;border:0;text-align:center;font-size:14px;color:#666666;line-height: 1.5">
				<span style="color:#12af3e;font-weight:bold;"><asp:Literal ID="ltrKPaxName" runat="server" /></span> 고객님. 안녕하세요.<br />
				<span style="color:#12af3e;font-weight:bold;"><asp:Literal ID="ltrAgentName" runat="server" /></span>에서 모두투어항공권을 이용해주셔서 감사드리며, 카드전표를 확인하실 수 있습니다.
			 </td>
		</tr>
		<tr>
			<td style="width:790px;height:30px;padding:0;border:0;"></td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:5px;padding:0;border:0;background:#dfdfdf;"></td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:25px;padding:0;border:0;"></td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:20px;padding:0;border:0;">
				<table width="800" border="0" cellpadding="0" cellspacing="0">
					<tr>
						<td style="width:790px;height:24px;padding:0;border:0;"><a href="<%=ETicketURL%>" target="_blank"><img src="http://img.modetour.com/modetour/2016/air_mail/btn_slip.gif" alt="카드전표 확인하기" border="0" /></a></td>
					</tr>
				</table> 
			</td>
		</tr>
		<tr>
			<td colspan="3" style="width:800px;height:156px;padding:0;border:0;">
				<img src="http://img.modetour.com/modetour/2016/air_mail/img_mail_txt2.gif" alt="카드전표 주의사항" border="0" />
			</td>
		</tr>
	</table>
</div>
</body>
</html>