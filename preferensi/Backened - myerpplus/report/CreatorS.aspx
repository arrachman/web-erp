<%@ Page Language="VB" AutoEventWireup="false" CodeFile="CreatorS.aspx.vb" Inherits="CreatorS" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head id="Head1" runat="server">
        <title>Request -</title>
        <link rel="icon" type="image/ico" href="../app/css/fav.png"/>
    </head>
    <body onload="f_CekProgress()">
        <form id="form1" runat="server">
            <div>
                <br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br />
                <div style="width: 100%; text-align: center;">
                    <asp:Image ID="imgloading" runat="server" ImageUrl="config/loader.gif" BorderStyle="None"/>
                    <br /><br />
                    <asp:Textbox ID="lblproses" Width="70%" runat="server" Text="Report Generator" style="text-align:center" BorderStyle ="None" Wrap ="true" Height="500px" TextMode="MultiLine" ReadOnly="true"/>
                    </div>
                <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
            </div>
        </form>
        <script src="config/jquery-1.4.1.min.js" type="text/javascript"></script>
        <script type = "text/javascript">
            var sptParam = '<%= sptParam %>', sptSubParam = '<%= sptSubParam %>', sptField = '<%= sptField %>', loop = 0;

            function f_CekProgress() {
                switch ('<%= posisi %>') {
                    case '0':
                        window.setTimeout("f_CekProgress()", 50);
                        break;
                    case '1':
                        $.ajax({
                            url: '<%= url %>' + "/ws/myerpplus.asmx/Ws?param=WebAccesKey★M0_MsmqGetdataById★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" + '<%= idgenerate %>' + "★1★",
                            dataType: "text",
                            success: OnSuccess,
                            failure: function (response) {
                                document.getElementById('lblproses').value = "Failed Request Report";
                            }
                        });
                        break;
                    case '2':
                        window.location.replace("../app");
                        break;
                }
            }

            function OnSuccess(a) {
                a = a.replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").replace("<string xmlns=\"http://tempuri.org/\">", "").replace("</string>", "");
                var hasil = a.split(sptParam)[2].split(sptSubParam)[0].split(sptField), s = "";
                if (a.split(sptParam)[0].split(sptSubParam)[1] == "1") {
                    switch (hasil[0]) {
                        case '0':
                            document.getElementById('lblproses').value = "Request Report\nPlease Wait";
                            if (loop < 10) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 500);
                            } else if (loop < 20) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 1000);
                            } else
                                window.setTimeout("f_CekProgress()", 2000);
                            break;
                        case '1':
                            document.getElementById('lblproses').value = "Processing Report\nPlease Wait";
                            if (loop < 10) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 500);
                            } else if (loop < 20) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 1000);
                            } else
                                window.setTimeout("f_CekProgress()", 2000);
                            break;
                        case '2':
                            switch ('<%= FileFormat %>') {
                                case "0": s = "pdf"; break;
                                case "1": s = "xls"; break;
                                case "2": s = "html"; break;
                                case "3": s = "doc"; break;
                                case "4": s = "txt"; break;
                                case "5": s = "jpg"; break;
                                case "6": s = "xls"; break;
                            }
                            window.location.replace('<%= url %>' + "/report/?p=" + '<%= FileName %>' + "." + s + "&n=" + '<%= Judul %>');
                            break;
                        case '3':
                            //Report Failed
                            document.title = document.title.replace("Request", "Failed");
                            document.getElementById('imgloading').src = "config/close.png";
                            document.getElementById('lblproses').value = hasil[1];
                            break;
                        case '4':
                            document.getElementById('lblproses').value = a.split(sptField)[2].split(sptSubParam)[0] + "% \n Processing Report \n Please Wait";
                            if (loop < 10) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 500);
                            } else if (loop < 20) {
                                loop += 1;
                                window.setTimeout("f_CekProgress()", 1000);
                            } else
                                window.setTimeout("f_CekProgress()", 2000);
                            break;
                    }
                } else {
                    if (a.split(sptParam)[0].split(sptSubParam)[2] == "MSMQ data not found.")
                        window.setTimeout("f_CekProgress()", 500);
                    else
                        document.getElementById('lblproses').value = "Error aspx : " + a.split(sptParam)[0].split(sptSubParam)[2];
                }
            }
        </script>
    </body>
</html>