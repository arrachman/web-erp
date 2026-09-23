Imports System.Messaging
Imports System.Net
Imports System.Xml
Imports System.IO

Imports System.Runtime.Serialization
Imports System.Data
Imports System.Diagnostics
Imports System.Web.Services


Partial Class Creator
    Inherits System.Web.UI.Page

    Public idgenerate As String = "", posisi As Integer = 0, Rquery, Idtransaksi As Integer, url, FileName As String, userAgent As String
    Public Modul, Menu, Item, Filter, Sort, GroupBy, FileFormat, Param1, UserId, UserNama, Judul, IdReport, Query, Sumber, WebsiteAccessKey As String

    'param split ws
    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public Req As String = ""
    Public Param As String = ""

#Region "MD5"
    Public Function MD5CalcString(ByVal strData As String) As String

        Dim objMD5 As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim arrData() As Byte
        Dim arrHash() As Byte

        ' first convert the string to bytes (using UTF8 encoding for unicode characters)
        arrData = System.Text.Encoding.UTF8.GetBytes(strData)

        ' hash contents of this byte array
        arrHash = objMD5.ComputeHash(arrData)

        ' thanks objects
        objMD5 = Nothing

        ' return formatted hash
        Return ByteArrayToString(arrHash)

    End Function

    ' utility function to convert a byte array into a hex string
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String

        Dim strOutput As New System.Text.StringBuilder(arrInput.Length)

        For i As Integer = 0 To arrInput.Length - 1
            strOutput.Append(arrInput(i).ToString("X2"))
        Next

        Return strOutput.ToString().ToLower

    End Function
#End Region

    Private Function f_Random(ByVal size As Integer) As String
        Dim nilai As Char() = New Char(size - 1) {}
        Dim _rng As Random = New Random()
        Dim _chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        For i As Integer = 0 To size - 1
            nilai(i) = _chars(_rng.[Next](_chars.Length))
        Next
        Return New String(nilai)
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.load
Dim a() As String
        Try
            If lblproses.Text = "Report Generator" Then
                userAgent = Request.UserAgent
                'Request Param From Flex
                'WebAccessKey(0), Module(1), Menu(2), Item(3), Filter(4), OrderBy(5), GroupBy(6), RQuery(7), Extension(8), Param(9), userid(10), unama(11), IDTransaksi(12), Sumber(13), NamaPerusahaan(14), Title(15), KotaTTD(16), watermark(17), ubahasa(18)
                Req = Request("val")
                a = Req.Split(sptField)
                Modul = a(1) : Menu = a(2) : Item = a(3)
                Filter = a(4) : Sort = a(5) : GroupBy = a(6)
                FileFormat = a(8) : Param1 = a(9) : UserId = a(10)
                UserNama = a(11) : Rquery = a(7) : Judul = a(15)
                Sumber = a(13) : Idtransaksi = a(12) : WebsiteAccessKey = a(0)
                url = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "")
                Title += " " + Judul
                FileName = Judul.Replace(" ", "_") + "-" + f_Random(4)
                If idgenerate = "" Then
                    'idgenerate = f_Random(12)
                    idgenerate = MD5CalcString(UserId & WebsiteAccessKey & Now & f_Random(3))
                    Req = F_splitArrayToString({idgenerate, Req, FileName, HttpContext.Current.Server.MapPath("~")})
                    Param = WebsiteAccessKey + "★M0_MsmqSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★0★" & F_splitArrayToString({idgenerate, Modul, Menu, Item, Filter, Sort, FileFormat, 0, Param1, "", "", "", "", 0, 0, UserId, Format(Now, "yyyy-MM-dd H:mm:ss"), "0000-00-00 00:00:00", "-", GroupBy, Sumber, Idtransaksi, 0, FileName})
                End If
                f_splitWs(Param + "△" + Req)
            End If
        Catch ex As Exception
            If ex.Message = "Object reference not set to an instance of an object." Then
                posisi = 2
                lblproses.Text = Err.Description
            Else
                lblproses.Text = Err.Description + " " + Request("val")
            End If
        End Try
        'lblproses.Text = Err.Description + " " + Request("val") + a.Length.ToString
    End Sub

    Function F_splitArrayToString(ByVal a() As String) As String
        Dim s As String = ""
        For i = 0 To a.Length - 1
            If i = 0 Then
                s = a(i)
            Else
                s += sptField + a(i)
            End If
        Next
        Return s
    End Function

    Private Sub f_splitWs(ByVal param As String)
        'Test Load Web Service
        Try
            Dim req As HttpWebRequest
            Dim res As HttpWebResponse
            Dim webStream As Stream
            Dim webStreamReader As XmlTextReader
            Dim s As String
            req = WebRequest.Create(url + "/ws/myerpplus.asmx/Ws?param=" + HttpContext.Current.Server.UrlEncode(param))
            req.Method = "GET"
            res = req.GetResponse()
            webStream = res.GetResponseStream()
            webStreamReader = New XmlTextReader(webStream)
            Do While (webStreamReader.Read())
                If webStreamReader.Name = "string" Then
                    s = webStreamReader.ReadElementContentAsString().ToString
                    If System.Convert.ToBoolean(Int(s.Split(sptParam)(0).ToString.Split(sptSubParam)(1))) Then
                        posisi = 1
                    Else
                        lblproses.Text = "Warning : " + s.Split(sptParam)(0).ToString.Split(sptSubParam)(2)
                    End If
                End If
            Loop
        Catch ex As Exception
            lblproses.Text = "Informasi Split WS : " + Err.Description
        End Try
    End Sub

End Class