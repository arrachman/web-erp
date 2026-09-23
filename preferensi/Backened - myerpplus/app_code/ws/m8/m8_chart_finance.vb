Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
Imports m8_content_chart
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m8_chart_finance
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    Public Function M8_TopAkun(ByVal Tgl As String, ByVal Filter As String, ByVal Ctipe As Integer) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim FilterD As String = ""
        Dim sql As String = ""

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'filter
        FilterD = "ctipe = " & Ctipe & " AND tstatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            'FilterD += " AND MONTH(ttgl) = MONTH('" + Tgl + "') AND YEAR(ttgl) = YEAR('" + Tgl + "')"
            FilterD += " AND ttgl < DATE_ADD('" + Tgl + "',INTERVAL 1 MONTH) "
        End If

        'update  data
        sql = "SELECT CONCAT(cnomor,' ',cnama) AS nama, SUM(tdebit-tkredit) AS total FROM m2_transaction_journal JOIN m1_coa ON cnomor = tnorek"
        GroupBy = "cnomor"
        'sql += " WHERE " + FilterD + " GROUP BY " + GroupBy + " HAVING total > 0 "
        sql += " WHERE " + FilterD + " GROUP BY " + GroupBy
        OrderBy = "total DESC"
        dt = AmbilData("aplikasi1-m8_content", "", OrderBy, True, , , 1, 10, pg1, , , "", sql) ' Ambil data ke databases
        pg1 = pg1
        Hasil.cseriesdata1 = "["
        If dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            For Each dr As DataRow In dt.Rows
                If (i <> 0) Then
                    Hasil.ccategories += ", "
                    Hasil.cseriesdata1 += ", "
                End If
                Hasil.ccategories += dr("nama")
                Hasil.cseriesdata1 += dr("total").ToString.Replace(",", ".")
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Public Function M8_TopAkunPerkontak(ByVal Tgl As String, ByVal Filter As String, ByVal Ctipe As Integer) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim FilterD As String = ""
        Dim sql As String = ""

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'filter
        FilterD = "ctipe = " & Ctipe & " AND tstatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            'FilterD += " AND MONTH(ttgl) = MONTH('" + Tgl + "') AND YEAR(ttgl) = YEAR('" + Tgl + "')"
            FilterD += " AND ttgl < DATE_ADD('" + Tgl + "',INTERVAL 1 MONTH) "
        End If

        'update  data
        sql = "SELECT CONCAT('(',kkode,') ',knama) AS nama, SUM(tkredit-tdebit) AS total FROM m2_transaction_journal JOIN m1_coa ON cnomor = tnorek JOIN m1_contact ON kid = tkontak"
        GroupBy = "kid"
        sql += " WHERE " + FilterD + " GROUP BY " + GroupBy + " HAVING total > 0 "
        OrderBy = "total DESC"
        dt = AmbilData("aplikasi1-m8_content", "", OrderBy, True, , , 1, 10, pg1, , , "", sql) ' Ambil data ke databases
        pg1 = pg1
        Hasil.cseriesdata1 = "["
        If dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            For Each dr As DataRow In dt.Rows
                If (i <> 0) Then
                    Hasil.ccategories += ", "
                    Hasil.cseriesdata1 += ", "
                End If
                Hasil.ccategories += dr("nama")
                Hasil.cseriesdata1 += dr("total").ToString.Replace(",", ".")
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function


End Class
