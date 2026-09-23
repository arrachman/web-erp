Imports System.Messaging
Imports System.Net
Imports System.Xml
Imports System.IO

Imports System.Runtime.Serialization
Imports System.Data
Imports System.Diagnostics
Imports System.Web.Services


Partial Class ImportData
    Inherits System.Web.UI.Page

    Public idgenerate As String = "", posisi As Integer = 0, url, userAgent As String, urlkirim As String = ""

    Dim WebsiteAccessKey As String = "", Sumber As String = "", Paket As String = "", NamaFile As String = "", StrNamaSheet As String = ""
    Dim UserID As Integer = 0

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

#Region "f_Random"
    Private Function f_Random(ByVal size As Integer) As String
        Dim nilai As Char() = New Char(size - 1) {}
        Dim _rng As Random = New Random()
        Dim _chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        For i As Integer = 0 To size - 1
            nilai(i) = _chars(_rng.[Next](_chars.Length))
        Next
        Return New String(nilai)
    End Function
#End Region

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.load
        Dim a() As String
        Try
            If lblproses.Text = "Import Data" Then
                userAgent = Request.UserAgent
                'Request Param From Flex
                'WebsiteAccessKey▼Sumber▼Paket▼NamaFile▼UserID▼StrNamaSheet
                Req = Request("val")
				a = Req.Split(sptField)
                WebsiteAccessKey = a(0) : Sumber = a(1) : Paket = a(2) : NamaFile = a(3)
                UserID = a(4) : StrNamaSheet = a(5)

                url = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "")
                Title += " " + Paket

                If idgenerate = "" Then
                    'idgenerate = f_Random(12)
                    idgenerate = MD5CalcString(UserID & WebsiteAccessKey & "importdata" & Sumber & Paket & Now)
					'idgenerate = "2b8b8caa56c311c2efc6449d1bfc2da6"
                    Req = F_splitArrayToString({idgenerate, Req, NamaFile, HttpContext.Current.Server.MapPath("~")})
                    '                                                                                                                           miid, misumber, miprogresspersen, miprogress, mipesan, mitglantrian, mitglselesai, miuserid, mipaket, minamafile
                    Param = WebsiteAccessKey + "★M0_ImportdataSimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & UserID & "★0★" & F_splitArrayToString({idgenerate, Sumber, 0, 0, "", Format(Now, "yyyy-MM-dd H:mm:ss"), "1971-01-01 00:00:00", UserID, Paket, NamaFile})
				End If
				posisi = 1
				
				f_splitWs(Param + "△" + StrNamaSheet)
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
            Dim s As String
            urlkirim = url + "/ws/myerpplus.asmx/Ws?param=" + HttpContext.Current.Server.UrlEncode(param)
        Catch ex As Exception
            lblproses.Text = "Informasi Split WS : " + Err.Description
        End Try
    End Sub

End Class