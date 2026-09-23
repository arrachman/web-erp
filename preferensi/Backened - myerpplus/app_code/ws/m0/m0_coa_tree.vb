Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports AsModuleMySQL.CommonFunction
Imports System.Data

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_coa_tree
    Inherits System.Web.Services.WebService

    Dim DtAnak As DataTable

    <WebMethod()> _
    Public Function M0_Coa_Tree(ByVal WebsiteAccessKey As String) As String
        Dim dtCOA1 As DataTable
        Dim sb As New StringBuilder

        'Buka node root
        sb.Append("<node nm='root'>")

        'Ambil hanya level tertinggi saja, Level 1
        dtCOA1 = AsDataTableAmbilDariDB("SELECT * FROM m1_coa WHERE clevel=1 ORDER BY cnomor")

        If dtCOA1.Rows.Count > 0 Then
            For Each drCoa As DataRow In dtCOA1.Rows
                'Jika mempunyai anak maka buat anak
                If JmlAnak(drCoa("cnomor")) > 0 Then
                    'Buka node parent
                    sb.Append("<node nm='" & drCoa("cnomor") & " " & drCoa("cnama") & "' nomor='" & drCoa("cnomor") & "' nama='" & drCoa("cnama") & "' uang='" & drCoa("cmatauang") & "' sal='" & drCoa("csaldoawal") & "' sb='" & drCoa("csaldoberjalan") & "' sak='" & drCoa("csaldoberjalan") & "' icon='tbcoa' level='" & drCoa("clevel") & "' tipe='" & drCoa("ctipe") & "'>")

                    sb.Append(BuatAnak(drCoa("cnomor")))

                    'Tutup node parent
                    sb.Append("</node>")
                Else
                    'Jika tidak mempunyai anak langsung tutup node
                    sb.Append("<node nm='" & drCoa("cnomor") & " " & drCoa("cnama") & "' nomor='" & drCoa("cnomor") & "' nama='" & drCoa("cnama") & "' uang='" & drCoa("cmatauang") & "' sal='" & drCoa("csaldoawal") & "' sb='" & drCoa("csaldoberjalan") & "' sak='" & drCoa("csaldoberjalan") & "' icon='tbcoa' level='" & drCoa("clevel") & "' tipe='" & drCoa("ctipe") & "' />")
                End If
            Next
        End If

        'Tutup node root
        sb.Append("</node>")

        Return sb.ToString
    End Function

    Private Function JmlAnak(ByVal Nomor As String) As Integer
        Dim JmlData As Integer
        DtAnak = AsDataTableAmbilDariDB("SELECT * FROM m1_coa WHERE cparent='" & Nomor & "'")

        JmlData = AsDataTableDCount(DtAnak)

        Return JmlData

    End Function

    Private Function BuatAnak(ByVal Nomor As String) As String
        Dim dt As DataTable
        Dim sb As New StringBuilder

        'ambil data dari parent
        dt = AsDataTableAmbilDariDB("SELECT * FROM m1_coa WHERE cparent='" & Nomor & "' ORDER BY cnomor")

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Jika mempunyai anak maka buat anak
                If JmlAnak(dr("cnomor")) > 0 Then
                    'Buka node parent
                    sb.Append("<node nm='" & dr("cnomor") & " " & dr("cnama") & "' nomor='" & dr("cnomor") & "' nama='" & dr("cnama") & "' uang='" & dr("cmatauang") & "' sal='" & dr("csaldoawal") & "' sb='" & dr("csaldoberjalan") & "' sak='" & dr("csaldoberjalan") & "' icon='tbcoa' level='" & dr("clevel") & "' tipe='" & dr("ctipe") & "'>")

                    sb.Append(BuatAnak(dr("cnomor")))

                    'Tutup node parent
                    sb.Append("</node>")
                Else
                    'Jika tidak mempunyai anak langsung tutup node
                    sb.Append("<node nm='" & dr("cnomor") & " " & dr("cnama") & "' nomor='" & dr("cnomor") & "' nama='" & dr("cnama") & "' uang='" & dr("cmatauang") & "' sal='" & dr("csaldoawal") & "' sb='" & dr("csaldoberjalan") & "' sak='" & dr("csaldoberjalan") & "' icon='tbcoa' level='" & dr("clevel") & "' tipe='" & dr("ctipe") & "' />")
                End If
            Next
        End If

        Return sb.ToString
    End Function
End Class