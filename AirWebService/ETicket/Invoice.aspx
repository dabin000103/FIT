<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Invoice.aspx.cs" Inherits="AirWebService.ETicket.Invoice" %>
<!DOCTYPE html>
<html class="no-js" lang="ko">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
    <title>모두투어 항공권 인보이스</title>
    <link rel="stylesheet" type="text/css" href="//www.modetour.com/flight/include/css/air/air_20181008.min.css"/>
    <link rel="stylesheet" type="text/css" href="//js.modetour.com/css/modetour/2016/common/common_201609.min.css?ver=19.05.17" />
    <script type="text/javascript" src="//js.modetour.com/jquery/ver/1.11.3.min.js"></script>
    <script type="text/javascript" src="//js.modetour.com/main/libs/ui_plugins.min.js"></script>
    <style type="text/css" media="print">
        @page {
            size:auto;
            margin:12px;
        }
        body {
            background-color: #fff;
            margin:10px;
        }
    </style>
</head>
<body>
    <div class="invoice_layer">
        <div class="invoice_area">
            <div class="top">
                <span class="tit">인보이스 인쇄</span>
            </div> <!-- // top -->
            <div class="contents">
                <div class="tit">
                    <div style="margin-right: 5px;"><img src="//img.modetour.com/modetour/2018/air/img_modetourCI.png" alt="모두투어"/></div>
                    <div><img src="//img.modetour.com/modetour/2018/air/img_invoicetitle.png" alt="인보이스"/></div>
                    <a href="#" class="btn_print" onclick="window.print();return false;">
                        <img src="//img.modetour.com/modetour/2018/air/btn_print_invocie.png" alt="인쇄하기"/>
                    </a>
                </div>
                <table summary="모두투어회사정보입니다." class="tb1">
                    <colgroup>
                        <col width="101">
                        <col width="234">
                        <col width="101">
                        <col width="234">
                    </colgroup>
                    <caption>모두투어회사정보</caption>
                    <tbody>
                        <tr>
                            <th>수신</th>
                            <td><asp:Literal ID="ltrBooker" runat="server" /></td>
                            <th>사업자번호</th>
                            <td>202-81-45295</td>
                        </tr>
                        <tr>
                            <th>발신</th>
                            <td>(주)모두투어네트워크</td>
                            <th>상호/대표자</th>
                            <td>우종웅</td>
                        </tr>
                        <tr>
                            <th>발행일자</th>
                            <td><asp:Literal ID="ltrPublishDate" runat="server" /></td>
                            <th>업태/종목</th>
                            <td>서비스, 제조/여행사, 기타 여행알선</td>
                        </tr>
                        <tr>
                            <th>주소</th>
                            <td colspan="3">서울특별시 중구 을지로 16 백남빌딩 5층</td>
                        </tr>
                    </tbody>
                </table>
                <div class="tb2">
                    <span class="tit">예약내역</span>
                    <table summary="예약내역입니다" class="tb2">
                        <caption>예약내역</caption>
                        <thead>
                            <tr>
                                <th>예약번호</th>
                                <th>항공사</th>
                                <th>출발일</th>
                                <th>목적지</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptFlightInfo" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <%# Container.ItemIndex.Equals(0) ? String.Format("<td rowspan=\"{0}\">{1}</td>", SegCount, OID) : ""%>
                                        <td><%#XPath("@mccn")%>(<%#XPath("@mcc")%>)</td>
                                        <td><%#XPath("@ddt").ToString().Substring(0, 10).Replace("-", ".")%></td>
                                        <td><%#XPath("@alcn")%>(<%#XPath("@alc")%>)</td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <div class="tb2">
                    <span class="tit">운임내역</span>
                    <table summary="운임내역입니다." class="tb2">
                        <caption>운임내역</caption>
                        <thead>
                            <tr>
                                <th>구분</th>
                                <th>인원</th>
                                <th>항공요금</th>
                                <th>유류할증료</th>
                                <th>제세공과금</th>
                                <th>발권수수료</th>
                                <th>합계</th>
                            </tr>
                        </thead>
                        <tbody>
                            <%if (ADTFareCount > 0) {%>
                            <tr>
                                <td>성인</td>
                                <td><asp:Literal ID="ltrADTCount" runat="server" /></td>
                                <td><asp:Literal ID="ltrADTFare" runat="server" /></td>
                                <td><asp:Literal ID="ltrADTFsc" runat="server" /></td>
                                <td><asp:Literal ID="ltrADTTax" runat="server" /></td>
                                <td><asp:Literal ID="ltrADTTasf" runat="server" /></td>
                                <td><asp:Literal ID="ltrADTPrice" runat="server" /></td>
                            </tr>
                            <%}%>
                            <%if (CHDFareCount > 0) {%>
                            <tr>
                                <td>소아</td>
                                <td><asp:Literal ID="ltrCHDCount" runat="server" /></td>
                                <td><asp:Literal ID="ltrCHDFare" runat="server" /></td>
                                <td><asp:Literal ID="ltrCHDFsc" runat="server" /></td>
                                <td><asp:Literal ID="ltrCHDTax" runat="server" /></td>
                                <td><asp:Literal ID="ltrCHDTasf" runat="server" /></td>
                                <td><asp:Literal ID="ltrCHDPrice" runat="server" /></td>
                            </tr>
                            <%}%>
                            <%if (INFFareCount > 0) {%>
                            <tr>
                                <td>유아</td>
                                <td><asp:Literal ID="ltrINFCount" runat="server" /></td>
                                <td><asp:Literal ID="ltrINFFare" runat="server" /></td>
                                <td><asp:Literal ID="ltrINFFsc" runat="server" /></td>
                                <td><asp:Literal ID="ltrINFTax" runat="server" /></td>
                                <td><asp:Literal ID="ltrINFTasf" runat="server" /></td>
                                <td><asp:Literal ID="ltrINFPrice" runat="server" /></td>
                            </tr>
                            <%}%>
                        </tbody>
                    </table>
                </div>
                <div class="tb2">
                    <span class="tit">결제내역</span>
                    <table summary="결제내역입니다." class="tb2">
                        <caption>결제내역</caption>
                        <thead>
                            <tr>
                                <th>카드</th>
                                <th>현금</th>
                                <th>총 결제금액</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td><asp:Literal ID="ltrCardPayment" runat="server" /></td>
                                <td><asp:Literal ID="ltrBankPayment" runat="server" /></td>
                                <td><asp:Literal ID="ltrPayment" runat="server" /></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="sign_box">
                    <img src="//img.modetour.com/modetour/2018/air/img_md_sign.png" alt="모두투어직인"/>
                </div>        
            </div>
            <a href="#" class="btn_close" onclick="window.open('about:blank','_self').close();"><img src="//img.modetour.com/modetour/2018/air/btn_close_print.png" alt="닫기"></a>
        </div>
    </div>
</body>
</html>