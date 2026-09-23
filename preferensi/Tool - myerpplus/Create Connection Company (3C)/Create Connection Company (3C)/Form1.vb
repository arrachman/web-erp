Imports System.Text
Imports System.IO

Public Class Form1
    Public appxml As String
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Focus()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btndirectoryapp.Click
        FolderBrowserApp.ShowDialog()
        txtdirectoryapp.Text = FolderBrowserApp.SelectedPath
    End Sub

    Private Sub btngenerate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btngenerate.Click
        Dim encod As String
        'app xml
        appxml = "<setting>" + vbCrLf
        appxml &= "    <url>http://" + txturl.Text + "</url>" + vbCrLf
        appxml &= "    <namapt>" + txtnamecompany.Text + "</namapt>" + vbCrLf
        appxml &= "    <kodeapp>" + txtkodeapp.Text + "</kodeapp>" + vbCrLf
        appxml &= "    <SqlServiceName>" + txtsqlservicename.Text + "</SqlServiceName>" + vbCrLf
        appxml &= "</setting>"
        simpanFile(txtdirectoryapp.Text + "\app\app.xml", appxml)

        'setting report
        appxml = "<setting>" + vbCrLf
        appxml &= "    <ConStr>Server=" + txtserver.Text + ";Port=3306;Database=" + txtdatabase.Text + ";Uid=" + txtuser.Text + ";Pwd=" + txtpassword.Text + ";Max Pool Size=10000;Allow User Variables=True;</ConStr>" + vbCrLf
        appxml &= "    <ReportConStr>DSN=" + txtdatabase.Text + ";DESCRIPTION=" + txtdatabase.Text + ";SERVER=" + txtserver.Text + ";UID=" + txtuser.Text + ";PWD=" + txtpassword.Text + ";DATABASE=" + txtdatabase.Text + ";PORT=3306</ReportConStr>" + vbCrLf
        appxml &= "</setting>"
        'simpanFile(txtdirectoryapp.Text + "\report\config\app.config", EncodeServerName("UUhKeU5HTm9iV0Z1TURnd01qa3p3aXphcmQ5Mw==" + EncodeServerName(appxml)))
        simpanFile(txtdirectoryapp.Text + "\report\config\app.config", EncodeServerName(appxml))

        'setting app global.asax
        encod = "Server=" + txtserver.Text + ";Port=3306;Database=" + txtdatabase.Text + ";Uid=" + txtuser.Text + ";Pwd=" + txtpassword.Text + ";Max Pool Size=10000;Allow User Variables=True;"
        appxml = "<%@ Application Language=""VB"" %>" + vbCrLf
        appxml &= "<%@ Import Namespace=""System.IO"" %>" + vbCrLf
        appxml &= "<%@ Import Namespace=""System.Text"" %>" + vbCrLf + vbCrLf
        appxml &= "<script runat=""server"">" + vbCrLf + vbCrLf
        appxml &= "    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)" + vbCrLf
        appxml &= "        AsModuleMySQL.KataKunci = ""Source code ini punya Alfasoft""" + vbCrLf
        appxml &= "        Dim fa As String = File.OpenText(HttpContext.Current.Server.MapPath(""~/"") + ""app\app.xml"").ReadToEnd" + vbCrLf
        appxml &= "        Application(""AppCode"") = """ + txtkodeapp.Text + """" + vbCrLf
        'appxml &= "        Application(""As_ConStr1"") = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(""" + EncodeServerName("NHJyYWNobUBuMDgwMjkzd2l6YXJkOTY=" + EncodeServerName(encod)) + """))).Replace(""NHJyYWNobUBuMDgwMjkzd2l6YXJkOTY="", """"))" + vbCrLf
        appxml &= "        Application(""As_ConStr1"") = Encoding.UTF8.GetString(Convert.FromBase64String(""" + EncodeServerName(encod) + """))" + vbCrLf
        appxml &= "    End Sub" + vbCrLf + vbCrLf

        appxml &= "    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)" + vbCrLf
        appxml &= "    End Sub" + vbCrLf + vbCrLf

        appxml &= "    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)" + vbCrLf
        appxml &= "    End Sub" + vbCrLf + vbCrLf

        appxml &= "    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)" + vbCrLf
        appxml &= "    End Sub" + vbCrLf + vbCrLf

        appxml &= "    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)" + vbCrLf
        appxml &= "    End Sub" + vbCrLf + vbCrLf

        appxml &= "</script>"
        simpanFile(txtdirectoryapp.Text + "\Global.asax", appxml)

        MsgBox("Done !")
    End Sub

    Public Function EncodeServerName(ByVal data As String) As String
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(data))
    End Function

    Public Function DecodeServerName(ByVal encodedServername As String) As String
        Return Encoding.UTF8.GetString(Convert.FromBase64String(encodedServername))
    End Function

    Public Sub simpanFile(ByVal lokasi As String, ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                If File.Exists(lokasi) Then
                    File.Create(lokasi).Dispose()
                End If
                File.WriteAllText(lokasi, StrVal)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        If (TextBox1.Text = "Mokodoyama2013^") Then
            Panel1.Hide()
        End If

    End Sub
End Class