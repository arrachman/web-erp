Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m8_content_chart
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""


    Class ChartData
        Public ccategories As String
        Public cjenis As String
        Public cdata As String
        Public cnominal As String
        Public cseriesnama1 As String
        Public cserieswarna1 As String
        Public cseriesdata1 As String
        Public cseriesnama2 As String
        Public cserieswarna2 As String
        Public cseriesdata2 As String
        Public clabel1 As String
        Public clabel2 As String
        Public clabel3 As String
    End Class

    <WebMethod()>
    Public Function M8_Content_ChartDataSearch(ByVal param As String) As String
        'M1_AreaSearch --------------------------------------------------------
        'akode, anama, acatatan, aaktif, ainputuser, ainputtgl, amodifikasiuser, 
        'amodifikasitgl

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
            result(2) = "Access denied for get data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter += " AND c.caktif = 1"
        Else
            Filter = " c.caktif = 1"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT c.cnama AS chnama, ch.* FROM `m8_content_chart` ch JOIN m8_content c ON c.ckode = ch.chkode "

        dt = AmbilData("aplikasi1-m8_content_chart", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("chnama"), ""), sptField,
                             FxDB(dr("chkode"), ""), sptField,
                             FxDB(dr("chjenis"), ""), sptField,
                             FxDB(dr("chdata"), ""), sptField,
                             FxDB(dr("chsatuan"), ""), sptField,
                             FxDB(dr("chlabel1"), ""), sptField,
                             FxDB(dr("chlabel2"), ""), sptField,
                             FxDB(dr("chlabel3"), ""), sptField,
                             FxDB(dr("chseriesnama1"), ""), sptField,
                             FxDB(dr("chserieswarna1"), ""), sptField,
                             FxDB(dr("chseriesnama2"), ""), sptField,
                             FxDB(dr("chserieswarna2"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Area data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("chnama, chkode, chjenis, chdata, chsatuan, chlabel1, chlabel2, chlabel3, chseriesnama1, chserieswarna1, chseriesnama2, chserieswarna2"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M8_Content_ChartSearch(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim filterSplit(3) As String    'tahun(0), bulan(1), filter(2)


        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim valtgl1 As String = ""
        Dim valtgl2 As String = ""
        Dim valfilter As String = ""

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            result(2) = "Invalid parameter." : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================

        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            result(2) = "Invalid paging parameter." : GoTo selesai
        End If

        'CEK PAGENUMBER
        If (IsNumeric(pagingSplit(0)) = False) Then
            result(2) = "pageNumber required numeric." : GoTo selesai
        End If

        'CEK ITEMLIMIT
        If (IsNumeric(pagingSplit(1)) = False) Then
            result(2) = "itemLimit required numeric." : GoTo selesai
        End If

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

        'CEK FORMATTGLWAKTU
        If Len(pagingSplit(5)) = 0 Then
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            filterSplit = pagingSplit(2).Split(spt2)
            'filter
            If (filterSplit(0).Length > 0) Then
                Filter = filterSplit(0) + " AND caktif = 1 "
            Else
                Filter = " caktif = 1 "

            End If
            'filter detail
            If (filterSplit(1).Length > 0) Then
                valfilter = filterSplit(1)
            End If
            'tgl (now)
            If (filterSplit(2).Length > 0) Then
                valtgl1 = filterSplit(2)
            End If
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT c.ckode, c.cmodule, c.cnama, c.cformula, c.cformat, c.cperiode, c.cketerangan, c.ctipe, c.csubtipe, c.clinkdetail, c.curutan, c.caktif FROM m8_content c"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1


        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Dim nama As String = dr("cformula")
                'set nilai
                Dim chart As ChartData = New ChartData()

                'konten data
                Select Case FxDB(dr("ckode"), "")

                    Case "C-F001" 'TOP 10 Saldo KAS
                        Dim wsFinance As New m8_chart_finance
                        chart = wsFinance.M8_TopAkun(valtgl1, "", 0)
                        chart.cnominal = "Rupiah"
                    Case "C-F002" 'TOP 10 Saldo BANK
                        Dim wsFinance As New m8_chart_finance
                        chart = wsFinance.M8_TopAkun(valtgl1, "", 1)
                        chart.cnominal = "Rupiah"
                    Case "C-F003" 'TOP 10 Saldo Hutang
                        Dim wsFinance As New m8_chart_finance
                        chart = wsFinance.M8_TopAkunPerkontak(valtgl1, "", 7)
                        chart.cnominal = "Rupiah"
                    Case "C-F004" 'Pembelian 10 Saldo Piutang
                        Dim wsFinance As New m8_chart_finance
                        chart = wsFinance.M8_TopAkunPerkontak(valtgl1, "", 2)
                        chart.cnominal = "Rupiah"


                    'Purchasing
                    Case "C-P01001" 'Grafik Order vs Pembelian 12 bulan terakhir
                        chart = M4_OrderVsPembelian(valtgl1, "")
                    Case "C-P01002" 'Pembelian 12 bulan terakhir
                        chart = M4_Pembelian(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-P01003" 'Supplier Baru 12 bulan terakhir
                        chart = M4_SupplierBaru(valtgl1, "")
                        chart.cnominal = "Kontak"
                    Case "C-P02001" 'TOP 10 Supplier Pembelian Terbesar
                        chart = M4_SupplierTerbesar(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-P02002" 'TOP 10 Supplier Pembelian Paling Aktif
                        chart = M4_SupplierTeraktif(valtgl1, "")
                        chart.cnominal = "Transaksi"
                    Case "C-P02003" 'TOP 10 Barang Pembelian Terbesar
                        chart = M4_BarangTerbesar(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-P02004" 'TOP 10 Barang Pembelian Paling Aktif
                        chart = M4_BarangTeraktif(valtgl1, "")
                        chart.cnominal = "Transaksi"

                    'Sales
                    Case "C-S01001" 'Order Vs Penjualan
                        chart = M5_OrderVsPenjualan(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-S01002" 'Penjualan 12 bulan terakhir
                        chart = M5_Penjualan(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-S01003" 'Customer Baru 12 bulan terakhir
                        chart = M5_CustomerBaru(valtgl1, "")
                        chart.cnominal = "Kontak"
                    Case "C-S02001" 'TOP 10 Customer Penjualan Terbesar
                        chart = M5_CustomerTerbesar(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-S02002" 'TOP 10 Customer Penjualan Paling Aktif
                        chart = M5_CustomerTeraktif(valtgl1, "")
                        chart.cnominal = "Transaksi"
                    Case "C-S02003" 'TOP 10 Barang Penjualan Terbesar
                        chart = M5_BarangTerbesar(valtgl1, "")
                        chart.cnominal = "Rupiah"
                    Case "C-S02004" 'TOP 10 Barang Penjualan Paling Aktif
                        chart = M5_BarangTeraktif(valtgl1, "")
                        chart.cnominal = "Transaksi"
                End Select

                'konten deskripsi
                Dim pg1c As New RsPaging
                Dim sqlc As String = "SELECT * FROM m8_content_chart"
                Dim Filterc As String = "chkode = '" + FxDB(dr("ckode"), "") + "'"
                Dim dtc As New DataTable
                dtc = AmbilData("aplikasi1-m8_content_chart", Filterc, "", True, , , 1, 12, pg1c, , , "", sqlc) ' Ambil data ke databases
                pg1c = pg1c
                If dtc.Rows.Count > 0 Then
                    For Each drc As DataRow In dtc.Rows
                        chart.cjenis = FxDB(drc("chjenis"), "")
                        chart.cdata = FxDB(drc("chdata"), "")
                        chart.cnominal = FxDB(drc("chsatuan"), "")
                        chart.clabel1 = FxDB(drc("chlabel1"), "")
                        chart.clabel2 = FxDB(drc("chlabel2"), "")
                        chart.clabel3 = FxDB(drc("chlabel3"), "")
                        chart.cseriesnama1 = FxDB(drc("chseriesnama1"), "")
                        chart.cserieswarna1 = FxDB(drc("chserieswarna1"), "")
                        chart.cseriesnama2 = FxDB(drc("chseriesnama2"), "")
                        chart.cserieswarna2 = FxDB(drc("chserieswarna2"), "")
                    Next
                End If

                search = String.Concat(search,
                     FxDB(dr("ckode"), ""), sptField,
                     FxDB(dr("cmodule"), 0), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cformula"), ""), sptField,
                     FxDB(dr("cformat"), ""), sptField,
                     FxDB(dr("cperiode"), ""), sptField,
                     FxDB(dr("cketerangan"), ""), sptField,
                     FxDB(dr("clinkdetail"), ""), sptField,
                     FxDB(dr("curutan"), 0), sptField,
                     FxDB(dr("ctipe"), ""), sptField,
                     FxDB(dr("csubtipe"), ""), sptField,
                     FxDB(chart.ccategories, ""), sptField,
                     FxDB(chart.cjenis, ""), sptField,
                     FxDB(chart.cdata, ""), sptField,
                     FxDB(chart.cnominal, ""), sptField,
                     FxDB(chart.clabel1, ""), sptField,
                     FxDB(chart.clabel2, ""), sptField,
                     FxDB(chart.clabel3, ""), sptField,
                     FxDB(chart.cseriesnama1, ""), sptField,
                     FxDB(chart.cserieswarna1, ""), sptField,
                     FxDB(chart.cseriesdata1, ""), sptField,
                     FxDB(chart.cseriesnama2, ""), sptField,
                     FxDB(chart.cserieswarna2, ""), sptField,
                     FxDB(chart.cseriesdata2, ""), sptField,
                     FxDB(dr("caktif"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Dashboard Content data not found."
        End If




selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ckode, cmodule, cnama, cformula, cformat, cperiode, cketerangan, clinkdetail, curutan, ctipe, csubtipe, ccategories, cjenis, cdata, cnominal, clabel1, clabel2, clabel3, cseriesnama1, cserieswarna1, cseriesdata1, cseriesnama2, cserieswarna2, cseriesdata2, caktif"))

        Return wsResult
    End Function

#Region "M4"

    Private Function M4_OrderVsPembelian(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String
        Dim arrdata2(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data1
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0
            'set kerangka data2
            arrdata2(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata2(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'update value data1
        sql = "SELECT CONCAT(YEAR(potgl),'-',MONTH(potgl)) AS periode, ROUND(SUM(pototaltransaksi), 2) As total FROM m4_po WHERE postatus IN (2,3,4,7) AND potgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "') "
        GroupBy = "YEAR(potgl), MONTH(potgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata1 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data1
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"


        'update value data2
        sql = "SELECT CONCAT(YEAR(ritgl),'-',MONTH(ritgl)) AS periode, ROUND(SUM(ritotaltransaksi), 2) As total FROM m4_ri ri WHERE ristatus IN (2,3,4,7) AND ritgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "') "
        GroupBy = "YEAR(ritgl), MONTH(ritgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata2.GetLength(0) - 1
                    If (dr("periode") = arrdata2(i, 0)) Then
                        arrdata2(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata2 = "["
        For i As Integer = 0 To arrdata2.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata2 += ", "
            End If
            Hasil.cseriesdata2 += arrdata2(i, 1)
        Next
        Hasil.cseriesdata2 += "]"


        Return Hasil
    End Function

    Private Function M4_Pembelian(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data1
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'update value data2
        sql = "SELECT CONCAT(YEAR(ritgl),'-',MONTH(ritgl)) AS periode, ROUND(SUM(ritotaltransaksi), 2) As total FROM m4_ri ri WHERE ristatus IN (2,3,4,7) AND ritgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "')"
        GroupBy = "YEAR(ritgl), MONTH(ritgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M4_SupplierBaru(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'update value data
        sql = "SELECT CONCAT(YEAR(kinputtgl),'-',MONTH(kinputtgl)) AS periode, COUNT(kid) AS total FROM m1_contact WHERE kaktif = 1 AND kkategori = 'S' AND kinputtgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "')"
        GroupBy = "YEAR(kinputtgl), MONTH(kinputtgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M4_SupplierTerbesar(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "ristatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(ritgl) = MONTH('" + Tgl + "') AND YEAR(ritgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT knama AS nama, SUM(ritotaltransaksi) AS total FROM m4_ri JOIN m1_contact ON kid = risupplier"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M4_SupplierTeraktif(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "ristatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(ritgl) = MONTH('" + Tgl + "') AND YEAR(ritgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT knama AS nama, COUNT(riid) AS total FROM m4_ri JOIN m1_contact ON kid = risupplier"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"
        Return Hasil
    End Function

    Private Function M4_BarangTerbesar(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "ristatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(ritgl) = MONTH('" + Tgl + "') AND YEAR(ritgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT bnama AS nama, SUM((jml*harga)-jmldiskon-jmlpajak1-jmlpajak2) AS total FROM m4_ri JOIN m4_ri_detail ON idri = riid JOIN m1_item ON bid = idbarang"
        GroupBy = "bid"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M4_BarangTeraktif(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "ristatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(ritgl) = MONTH('" + Tgl + "') AND YEAR(ritgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT bnama AS nama, COUNT(bid) AS total FROM m4_ri JOIN m4_ri_detail ON idri = riid JOIN m1_item ON bid = idbarang"
        GroupBy = "bid"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

#End Region

#Region "M5"

    Private Function M5_OrderVsPenjualan(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String
        Dim arrdata2(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data1
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0
            'set kerangka data2
            arrdata2(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata2(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'update value data1
        sql = "SELECT CONCAT(YEAR(sotgl),'-',MONTH(sotgl)) AS periode, ROUND(SUM(sototaltransaksi), 2) As total FROM m5_so WHERE sostatus IN (2,3,4,7) AND sotgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "') "
        GroupBy = "YEAR(sotgl), MONTH(sotgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata1 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data1
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"

        'update value data2
        sql = "SELECT CONCAT(YEAR(sitgl),'-',MONTH(sitgl)) AS periode, ROUND(SUM(sitotaltransaksi), 2) As total FROM m5_si si WHERE sistatus IN (2,3,4,7) AND sitgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "') "
        GroupBy = "YEAR(sitgl), MONTH(sitgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata2.GetLength(0) - 1
                    If (dr("periode") = arrdata2(i, 0)) Then
                        arrdata2(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata2 = "["
        For i As Integer = 0 To arrdata2.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata2 += ", "
            End If
            Hasil.cseriesdata2 += arrdata2(i, 1)
        Next
        Hasil.cseriesdata2 += "]"

        Return Hasil
    End Function

    Private Function M5_Penjualan(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data1
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'update value data2
        sql = "SELECT CONCAT(YEAR(sitgl),'-',MONTH(sitgl)) AS periode, ROUND(SUM(sitotaltransaksi), 2) As total FROM m5_si si WHERE sistatus IN (2,3,4,7) AND sitgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "')"
        GroupBy = "YEAR(sitgl), MONTH(sitgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M5_CustomerBaru(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim sql As String = ""
        Dim arrdata1(11, 1) As String


        'categories
        Dim dateFilter As String = Tgl
        Dim t As Integer = 11
        For i As Integer = 1 To 12
            'set data categories
            Dim myDate As DateTime = DateTime.Parse(dateFilter).AddMonths(t * -1)
            Hasil.ccategories += myDate.Year.ToString() + "-" + myDate.Month.ToString()
            If (t <> 0) Then
                Hasil.ccategories += ", "
            End If
            'set kerangka data
            arrdata1(i - 1, 0) = myDate.Year.ToString() + "-" + myDate.Month.ToString()
            arrdata1(i - 1, 1) = 0

            t = t - 1
        Next


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        'update value data
        sql = "SELECT CONCAT(YEAR(kinputtgl),'-',MONTH(kinputtgl)) AS periode, COUNT(kid) AS total FROM m1_contact WHERE kaktif = 1 AND kkategori = 'C' AND kinputtgl BETWEEN CONCAT(YEAR(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-', MONTH(DATE_ADD('" + Tgl + "', INTERVAL -11 MONTH)), '-01') AND LAST_DAY('" + Tgl + "')"
        GroupBy = "YEAR(kinputtgl), MONTH(kinputtgl)"
        dt = AmbilData("aplikasi1-m8_content", Filter, OrderBy, True, , , 1, 12, pg1, , , GroupBy, sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Hasil.cseriesdata2 += FxDB(dr("total"), 0)
                For i As Integer = 0 To arrdata1.GetLength(0) - 1
                    If (dr("periode") = arrdata1(i, 0)) Then
                        arrdata1(i, 1) = dr("total").ToString
                    End If
                Next
            Next
        End If
        'set data2
        Hasil.cseriesdata1 = "["
        For i As Integer = 0 To arrdata1.GetLength(0) - 1
            If (i <> 0) Then
                Hasil.cseriesdata1 += ", "
            End If
            Hasil.cseriesdata1 += arrdata1(i, 1)
        Next
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M5_CustomerTerbesar(ByVal Tgl As String, ByVal Filter As String) As ChartData
        Dim Hasil As ChartData = New ChartData()
        Dim pg1 As New RsPaging
        Dim dt As New DataTable
        Dim OrderBy As String = ""
        Dim GroupBy As String = ""
        Dim GroupD As String = ""
        Dim FilterD As String = ""
        Dim sql As String = ""

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'filter
        FilterD = "sistatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(sitgl) = MONTH('" + Tgl + "') AND YEAR(sitgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT knama AS nama, SUM(sitotaltransaksi) AS total FROM m5_si JOIN m1_contact ON kid = sicustomer"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"


        Return Hasil
    End Function

    Private Function M5_CustomerTeraktif(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "sistatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(sitgl) = MONTH('" + Tgl + "') AND YEAR(sitgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT knama AS nama, COUNT(siid) AS total FROM m5_si JOIN m1_contact ON kid = sicustomer"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M5_BarangTerbesar(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "sistatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(sitgl) = MONTH('" + Tgl + "') AND YEAR(sitgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT bnama AS nama, SUM((jml*harga)-jmldiskon-jmlpajak1-jmlpajak2) AS total FROM m5_si JOIN m5_si_detail ON idsi = siid JOIN m1_item ON bid = idbarang"
        GroupBy = "bid"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

    Private Function M5_BarangTeraktif(ByVal Tgl As String, ByVal Filter As String) As ChartData
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
        FilterD = "sistatus IN (2,3,4,7)"
        If (Tgl.Length > 0) Then
            FilterD += " AND MONTH(sitgl) = MONTH('" + Tgl + "') AND YEAR(sitgl) = YEAR('" + Tgl + "')"
        End If

        'update  data
        sql = "SELECT bnama AS nama, COUNT(bid) AS total FROM m5_si JOIN m5_si_detail ON idsi = siid JOIN m1_item ON bid = idbarang"
        GroupBy = "bid"
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
                Hasil.cseriesdata1 += dr("total").ToString
                i = i + 1
            Next
        End If
        Hasil.cseriesdata1 += "]"

        Return Hasil
    End Function

#End Region



End Class
