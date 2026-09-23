Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m8_content
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M8_ContentDataSearch(ByVal param As String) As String
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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT c.ckode, c.cmodule, c.cnama, c.cformula, c.cformat, c.cperiode, c.cketerangan, c.clinkdetail, c.curutan, c.caktif, c.cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl, m.mname, i.igreater, i.ivalue1, i.ivalue2 FROM m8_content AS c JOIN m0_module m ON m.mid = c.cmodule JOIN m8_indicator i ON i.ikode = c.ckode "

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'nama isgreater 
                Dim vgreater As String = ""
                If (dr("igreater") = 1) Then
                    vgreater = "Nilai Besar Lebih Baik"
                Else
                    vgreater = "Nilai Kecil Lebih Baik"
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
                             FxDB(dr("caktif"), 0), sptField,
                             FxDB(dr("mname"), ""), sptField,
                             FxDB(vgreater, ""), sptField,
                             FxDB(dr("ivalue1"), 0), sptField,
                             FxDB(dr("ivalue2"), 0), sptField,
                             FxDB(dr("cinputuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("cinputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("cmodifikasiuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("cmodifikasitgl"), ""), formatTglWaktu), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ckode, cmodule, cnama, cformula, cformat, cperiode, cketerangan, clinkdetail, curutan, caktif, mname, igreater, ivalue1, ivalue2, cinputuser, cinputtgl, cmodifikasiuser, cmodifikasitgl"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M8_ContentSearch(ByVal param As String) As String
        'CdM0_Carabayar --------------------------------------------------------
        'kode, nama

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

        Dim valbulan As Integer = 1
        Dim valtahun As Integer = 2021
        Dim valfilter As String = ""
        Dim valtgl1 As String = ""
        Dim valtgl2 As String = ""

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
            'tahun
            If (filterSplit(0).Length > 0) Then
                valtahun = filterSplit(0)
            End If
            'bulan
            If (filterSplit(1).Length > 0) Then
                valbulan = filterSplit(1)
            End If
            'tgl1
            If (filterSplit(2).Length > 0) Then
                valtgl1 = filterSplit(2)
            End If
            'tgl2
            If (filterSplit(3).Length > 0) Then
                valtgl2 = filterSplit(3)
            End If
            'filter Content
            If (filterSplit(4).Length > 0) Then
                Filter = filterSplit(4)
                Filter = Filter + " AND ctipe = 'dashboard'"
            Else
                Filter = "ctipe = 'dashboard'"
            End If
            'filter Dashboard
            If (filterSplit(5).Length > 0) Then
                valfilter = filterSplit(5)
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

        sql = "SELECT c.ckode, c.cmodule, c.cnama, c.cformula, c.cformat, c.cperiode, c.cketerangan, c.clinkdetail, c.curutan, c.caktif, IFNULL(i.igreater,3) AS igreater, IFNULL(i.ivalue1,0) AS ivalue1, IFNULL(i.ivalue2,0) AS ivalue2, IFNULL(i.ivalue3,0) AS ivalue3 FROM m8_content c LEFT JOIN m8_indicator i on i.ikode = c.ckode"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1


        Dim clow As String = "bg-red"
        Dim cmedium As String = "bg-yellow"
        Dim chigh As String = "bg-green"
        Dim cunset As String = "bg-aqua"

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                'Dim nama As String = dr("cformula")
                'set nilai
                Dim nilai As Double = 0
                Select Case FxDB(dr("ckode"), "")
                    'Inventory
                    Case "W001" 'Jml Penyesuaian Stok
                        nilai = M3_JmlPenyesuaianStok(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                        'nama = M3_JmlPenyesuaianStok_Test(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W002" 'Nilai Penyesuaian Stok
                        nilai = M3_NilaiPenyesuaianStok(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W003" 'Barang di Bawah Minimum
                        nilai = M3_BarangStokMinimum(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W004" 'Barang Tidak Aktif
                        nilai = M3_BarangTidakAktif(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W005" 'Barang Hilang Terbanyak
                        nilai = M3_BarangHilangTerbanyak(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W006" 'Nilai Barang Hilang
                        nilai = M3_NilaiBarangHilang(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W007" 'Barang Rusak Terbanyak
                        nilai = M3_BarangRusakTerbanyak(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W008" 'Nilai Barang Rusak
                        nilai = M3_NilaiBarangRusak(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W009" 'Barang Fast Moving
                        nilai = M3_BarangFastMoving(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W010" 'Barang Slow Moving
                        nilai = M3_BarangSlowMoving(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "W011" 'Mutasi Barang Outstanding
                        nilai = M3_MutasiBarangOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)

                    'Purchasing
                    Case "PPR001" 'Jml Transaksi PR
                        nilai = M4_JmlTransaksiPR(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPR002" 'PR Outstanding
                        nilai = M4_PROutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPR003" 'Pemenuhan PR
                        nilai = M4_PemenuhanPR(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPO001" 'Jml Transaksi PO
                        nilai = M4_JmlTransaksiPO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPO002" 'PO Outstanding
                        nilai = M4_POOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPO003" 'Pemenuhan PO
                        nilai = M4_PemenuhanPO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PPO004" 'Kecepatan PO
                        nilai = M4_KecepatanPO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PGRN001" 'Jml Transaksi GRN 
                        nilai = M4_JmlTransaksiGRN(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PGRN002" 'GRN Outstanding
                        nilai = M4_GRNOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PGRN003" 'Pemenuhan GRN
                        nilai = M4_PemenuhanGRN(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PGRN004" 'Kecepatan GRN
                        nilai = M4_KecepatanGRN(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PRI001" 'Jml Transaksi RI 
                        nilai = M4_JmlTransaksiRI(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PRI002" 'RI Outstanding
                        nilai = M4_RIOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PRI003" 'Pemenuhan RI
                        nilai = M4_PemenuhanRI(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PRI004" 'Kecepatan RI
                    Case "PPRT001" 'Jml Transaksi Retur 
                        nilai = M4_JmlTransaksiRetur(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PVP001" 'Jml Transaksi VP
                        nilai = M4_JmlTransaksiVP(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PVP002" 'Kecepatan VP
                        nilai = M4_KecepatanVP(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PS001" 'Supplier Baru
                        nilai = M4_SupplierBaru(valbulan, valtahun, valtgl1, valtgl2, valfilter)

                    'Sales
                    Case "SSO001" 'SO Outstanding
                        nilai = M5_JmlTransaksiSO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSO002" 'Jml Transaksi SO
                        nilai = M5_SOOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSO003" 'Pemenuhan SO
                        nilai = M5_PemenuhanSO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SDO001" 'Jml Transaksi DO
                        nilai = M5_JmlTransaksiDO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SDO002" 'DO Outstanding
                        nilai = M5_DOOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SDO003" 'Pemenuhan DO
                        nilai = M5_PemenuhanDO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SDO004" 'Kecepatan DO
                        nilai = M5_KecepatanDO(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSI001" 'Jml Transaksi SI
                        nilai = M5_JmlTransaksiSI(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSI002" 'SI Outstanding
                        nilai = M5_SIOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSI003" 'Pemenuhan SI
                        nilai = M5_PemenuhanSI(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSI004" 'Kecepatan SI
                        nilai = M5_KecepatanSI(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SSR001" 'Jml Transaksi Retur
                        nilai = M5_JmlTransaksiSR(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SCP001" 'Jml Transaksi PV
                        nilai = M5_JmlTransaksiPV(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SCP002" 'Kecepatan PV
                        nilai = M5_KecepatanPV(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "SC001" 'Customer Baru
                        nilai = M5_CustomerBaru(valbulan, valtahun, valtgl1, valtgl2, valfilter)

                    'Production
                    Case "PD001" 'Jml Produksi
                        nilai = M6_JmlProduksi(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD002" 'Jml Reject
                        nilai = M6_JmlReject(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD003" 'Jml Waste
                        nilai = M6_JmlWaste(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD004" 'Persentase Jml Bagus
                        nilai = M6_PersentaseBagus(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD005" 'Persentase Jml Reject
                        nilai = M6_PersentaseReject(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD006" 'Persentase Jml Waste
                        nilai = M6_PersentaseWaste(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD007" 'Permintaan Produksi Outstanding
                        nilai = M6_PermintaanProduksiOutstanding(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD008" 'Barang Produksiter banyak
                        nilai = M6_BarangProduksiterbanyak(valbulan, valtahun, valtgl1, valtgl2, valfilter)
                    Case "PD009" 'Persentare Realisasi vs Formula
                        nilai = M6_PersentareRealisasivsFormula(valbulan, valtahun, valtgl1, valtgl2, valfilter)

                End Select
                'set warna
                'Dim warna As String = ""
                'If (dr("igreater") = 1) Then 'nilai besar lebih baik
                '    If (nilai >= dr("ivalue1") And nilai < dr("ivalue2")) Then 'low
                '        warna = clow
                '    ElseIf (nilai >= dr("ivalue2") And nilai < dr("ivalue3")) Then 'medium
                '        warna = cmedium
                '    ElseIf (nilai >= dr("ivalue3")) Then 'high
                '        warna = chigh
                '    Else
                '        warna = cunset
                '    End If
                'ElseIf (dr("igreater") = 0) Then 'nilai kecil lebih baik
                '    If (nilai >= dr("ivalue1") And nilai < dr("ivalue2")) Then 'low
                '        warna = clow
                '    ElseIf (nilai >= dr("ivalue2") And nilai < dr("ivalue3")) Then 'medium
                '        warna = cmedium
                '    ElseIf (nilai >= dr("ivalue3")) Then 'high
                '        warna = chigh
                '    Else
                '        warna = cunset
                '    End If
                'End If
                Dim warna As String = cunset
                nilai = FxDB(nilai, 0)
                If (nilai < dr("ivalue1")) Then 'low
                    warna = clow
                ElseIf (nilai >= dr("ivalue1") And nilai <= dr("ivalue2")) Then 'medium
                    warna = cmedium
                ElseIf (nilai > dr("ivalue2")) Then 'high
                    warna = chigh
                End If
                'jika nilai blm diset
                If (dr("ivalue1") = 0 And dr("ivalue2") = 0) Then
                    warna = cunset
                End If
                'nama isgreater
                Dim vgreater As String = ""
                If (dr("igreater") = 1) Then
                    vgreater = "Nilai Besar Lebih Baik"
                Else
                    vgreater = "Nilai Kecil Lebih Baik"
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
                     FxDB(nilai, 0), sptField,
                     FxDB(dr("caktif"), 0), sptField,
                     FxDB(vgreater, ""), sptField,
                     FxDB(dr("ivalue1"), 0), sptField,
                     FxDB(dr("ivalue2"), 0), sptField,
                     FxDB(warna, ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("ckode, cmodule, cnama, cformula, cformat, cperiode, cketerangan, clinkdetail, curutan, cnilai, caktif, igreater, ivalue1, ivalue2, cwarna"))

        Return wsResult
    End Function

#Region "M3 Content"

    Public Function M3_JmlPenyesuaianStok(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(said) AS jml FROM `m3_sa`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    'Public Function M3_JmlPenyesuaianStok_Test(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As String
    '    Dim Hasil As String = ""

    '    Dim sql As String = ""

    '    Dim pg1 As New RsPaging
    '    Dim Sorting As String = ""
    '    Dim dt As New DataTable

    '    If (Filter.Length > 0) Then
    '        Filter = " sastatus IN (2,3,4,7) AND " + Filter
    '    Else
    '        Filter = " sastatus IN (2,3,4,7) "
    '    End If

    '    'filter Tgl
    '    If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
    '        Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
    '    End If

    '    'jika menggunakan filter bulan/tahun
    '    If (Tahun <> 0 And Bulan <> 0) Then
    '        Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
    '    End If

    '    'BUKA KONEKSI
    '    Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
    '    Con1.Open()

    '    sql = "SELECT COUNT(said) AS jml FROM `m3_sa`"

    '    dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
    '    pg1 = pg1

    '    Hasil = sql + " WHERE " + Filter
    '    'Hasil = "Hasil " + Tgl1.Length.ToString + " = " + Tgl1 + " | " + Tgl2.Length.ToString + " = " + Tgl2

    '    Return Hasil
    'End Function


    Public Function M3_NilaiPenyesuaianStok(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'Filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT IFNULL(SUM(jmlmasuk*hpp)-SUM(jmlkeluar*hpp),0) AS nilai FROM m3_sa_detail JOIN m3_sa ON said = idsa"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangStokMinimum(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'If (Filter.Length > 0) Then
        '    Filter = " baktif = 1 AND bstok < bstokminimal AND " + Filter
        'Else
        '    Filter = " baktif = 1 AND bstok < bstokminimal "
        'End If
        Filter = " baktif = 1 AND bstok < bstokminimal "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(bid) AS jml FROM m1_item"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangTidakAktif(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'If (Filter.Length > 0) Then
        '    Filter = " baktif = 0 AND " + Filter
        'Else
        '    Filter = " baktif = 0 "
        'End If
        Filter = " baktif = 0 "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(bid) AS jml FROM m1_item"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangHilangTerbanyak(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sajenis = 'LOST' AND sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sajenis = 'LOST' AND sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'filter bulan/tahun
        If (Tahun <> 0 And Tahun <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT SUM(jmlkeluar) AS jml FROM `m3_sa` JOIN m3_sa_detail ON said = idsa"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_NilaiBarangHilang(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sajenis = 'LOST' AND sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sajenis = 'LOST' AND sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'Filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT IFNULL(SUM(jmlmasuk*hpp)-SUM(jmlkeluar*hpp),0) AS nilai FROM m3_sa_detail JOIN m3_sa ON said = idsa"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangRusakTerbanyak(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sajenis = 'NG' AND sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sajenis = 'NG' AND sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT SUM(jmlkeluar) AS jml FROM `m3_sa` JOIN m3_sa_detail ON said = idsa"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_NilaiBarangRusak(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sajenis = 'NG' AND sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sajenis = 'NG' AND sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT IFNULL(SUM(jmlmasuk*hpp)-SUM(jmlkeluar*hpp),0) AS nilai FROM m3_sa_detail JOIN m3_sa ON said = idsa"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangFastMoving(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'If (Filter.Length > 0) Then
        '    Filter = " baktif = 1 AND bstatusmoving = 'F' AND " + Filter
        'Else
        '    Filter = " baktif = 1 AND bstatusmoving = 'F' "
        'End If

        Filter = " baktif = 1 AND bstatusmoving = 'F' "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(bid) AS jml FROM m1_item"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_BarangSlowMoving(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'If (Filter.Length > 0) Then
        '    Filter = " baktif = 1 AND bstatusmoving = 'S' AND " + Filter
        'Else
        '    Filter = " baktif = 1 AND bstatusmoving = 'S' "
        'End If
        Filter = " baktif = 1 AND bstatusmoving = 'S' "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(bid) AS jml FROM m1_item"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M3_MutasiBarangOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " AND tsstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " AND tsstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl2.Length > 0) Then
            Filter += " AND tstgl <= '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0) Then
            Filter += " AND MONTH(tstgl) = '" + Bulan.ToString + "' AND YEAR(tstgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(tsid) AS jml FROM `m3_ts`"

        dt = AmbilData("aplikasi1-m3_ts", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

#End Region

#Region "M4 content"

    Public Function M4_JmlTransaksiPR(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " prstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " prstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND prtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(prtgl) = '" + Bulan.ToString + "' AND YEAR(prtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(prid) AS jml FROM `m4_pr`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_PROutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " prstatus IN (2,3) AND " + Filter
        Else
            Filter = " prstatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND prtgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND prtgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(prid) AS jml FROM `m4_pr`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_PemenuhanPR(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND prtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(prtgl) = '" + Bulan.ToString + "' AND YEAR(prtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(prid) AS jml FROM m4_pr"
        filterr = "prstatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m4_pr", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(prid) AS jml FROM m4_pr"
        filtera = "prstatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_pr", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_JmlTransaksiPO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " postatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " postatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND potgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(potgl) = '" + Bulan.ToString + "' AND YEAR(potgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(poid) AS jml FROM `m4_po`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_POOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " postatus IN (2,3) AND " + Filter
        Else
            Filter = " postatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND potgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND potgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(poid) AS jml FROM `m4_po`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_PemenuhanPO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND potgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(potgl) = '" + Bulan.ToString + "' AND YEAR(potgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(poid) AS jml FROM m4_po"
        filterr = "postatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(poid) AS jml FROM m4_po"
        filtera = "postatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_KecepatanPO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND potgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(potgl) = '" + Bulan.ToString + "' AND YEAR(potgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(po.poid) AS jml FROM m4_po po JOIN m4_po_detail pod ON po.poid = pod.idpo JOIN m4_pr_detail prd ON prd.idprdetail = pod.idprdetail JOIN m4_pr pr ON pr.prid = prd.idpr"
        filterr = "po.postatus IN (2,3,4,7) AND po.potgl <= pr.prtgldipakai " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml po yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(poid) AS jml FROM m4_po po JOIN m4_po_detail pod ON po.poid = pod.idpo JOIN m4_pr_detail prd ON prd.idprdetail = pod.idprdetail JOIN m4_pr pr ON pr.prid = prd.idpr"
        filtera = "postatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_JmlTransaksiGRN(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " grnstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " grnstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND grntgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(grntgl) = '" + Bulan.ToString + "' AND YEAR(grntgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(grnid) AS jml FROM `m4_grn`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_GRNOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " grnstatus IN (2,3) AND " + Filter
        Else
            Filter = " grnstatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND grntgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND grntgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(grnid) AS jml FROM `m4_grn`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_PemenuhanGRN(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND grntgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(grntgl) = '" + Bulan.ToString + "' AND YEAR(grntgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(grnid) AS jml FROM m4_grn"
        filterr = "grnstatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m4_grn", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(grnid) AS jml FROM m4_grn"
        filtera = "grnstatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_grn", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_KecepatanGRN(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND potgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(potgl) = '" + Bulan.ToString + "' AND YEAR(potgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(grnid) AS jml FROM m4_grn grn JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn JOIN m4_po_detail pod ON pod.idpodetail = grnd.idpodetail JOIN m4_po po ON po.poid = pod.idpo"
        filterr = "grn.grnstatus (2,3,4,7) And grntgl <= potgldipenuhi " + Filter
        dtr = AmbilData("aplikasi1-m4_po", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml po yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(grnid) AS jml FROM m4_grn grn JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn JOIN m4_po_detail pod ON pod.idpodetail = grnd.idpodetail JOIN m4_po po ON po.poid = pod.idpo"
        filtera = "grn.grnstatus In (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_po", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_JmlTransaksiRI(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " ristatus In (2,3,4,7) And " + Filter
        Else
            Filter = " ristatus In (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " And ritgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(ritgl) = '" + Bulan.ToString + "' AND YEAR(ritgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(riid) AS jml FROM `m4_ri`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_RIOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " ristatus IN (2,3) AND " + Filter
        Else
            Filter = " ristatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND ritgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND ritgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(riid) AS jml FROM `m4_ri`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_PemenuhanRI(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND ritgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(ritgl) = '" + Bulan.ToString + "' AND YEAR(ritgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(riid) AS jml FROM m4_ri"
        filterr = "ristatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m4_ri", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(riid) AS jml FROM m4_ri"
        filtera = "ristatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_ri", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M4_JmlTransaksiVP(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " vpstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " vpstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND vptgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(vptgl) = '" + Bulan.ToString + "' AND YEAR(vptgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(vpid) AS jml FROM `m4_vp`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_KecepatanVP(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND vptgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(vptgl) = '" + Bulan.ToString + "' AND YEAR(vptgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(vpid) AS jml FROM m4_vp vp JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp JOIN m4_ri ri ON ri.riid = vpd.idtransaksi AND vpd.sumber = 'RI'"
        filterr = "vp.vpstatus IN (2,3,4,7) And vptgl <= ritgljatuhtempo " + Filter
        dtr = AmbilData("aplikasi1-m4_vp", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml VP yg menggunakan RI
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(vpid) AS jml FROM m4_vp vp JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp JOIN m4_ri ri ON ri.riid = vpd.idtransaksi AND vpd.sumber = 'RI'"
        filtera = "vp.vpstatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m4_vp", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If
        Return Hasil
    End Function

    Public Function M4_JmlTransaksiRetur(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " prtstatus In (2,3,4,7) And " + Filter
        Else
            Filter = " prtstatus In (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " And prttgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(prttgl) = '" + Bulan.ToString + "' AND YEAR(prttgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(prtid) AS jml FROM `m4_prt`"

        dt = AmbilData("aplikasr1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M4_SupplierBaru(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " kkategori = 'S' AND " + Filter
        Else
            Filter = " kkategori = 'S' "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND DATE(kinputtgl) BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(kinputtgl) = '" + Bulan.ToString + "' AND YEAR(kinputtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(kid) AS jml FROM m1_contact"

        dt = AmbilData("aplikasr1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function


#End Region

#Region "M5 content"

    Public Function M5_JmlTransaksiSO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sostatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sostatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sotgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(sotgl) = '" + Bulan.ToString + "' AND YEAR(sotgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(soid) AS jml FROM `m5_so`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_SOOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sostatus IN (2,3) AND " + Filter
        Else
            Filter = " sostatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sotgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND sotgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(soid) AS jml FROM `m5_so`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_PemenuhanSO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sotgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(sotgl) = '" + Bulan.ToString + "' AND YEAR(sotgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(soid) AS jml FROM m5_so"
        filterr = "sostatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m5_so", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(soid) AS jml FROM m5_so"
        filtera = "sostatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_so", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M5_JmlTransaksiDO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " dostatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " dostatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND dotgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(dotgl) = '" + Bulan.ToString + "' AND YEAR(dotgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(doid) AS jml FROM `m5_do`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_DOOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " dostatus IN (2,3) AND " + Filter
        Else
            Filter = " dostatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND dotgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND dotgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(doid) AS jml FROM `m5_do`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_PemenuhanDO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND dotgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(dotgl) = '" + Bulan.ToString + "' AND YEAR(dotgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(doid) AS jml FROM m5_do"
        filterr = "dostatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m5_do", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(doid) AS jml FROM m5_do"
        filtera = "dostatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_do", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M5_KecepatanDO(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND dotgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(dotgl) = '" + Bulan.ToString + "' AND YEAR(dotgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(do.doid) AS jml FROM m5_do do JOIN m5_do_detail dod ON do.doid = dod.iddo JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail JOIN m5_so so ON so.soid = sod.idso"
        filterr = "do.dostatus IN (2,3,4,7) AND do.dotgl <= so.sotglkirim " + Filter
        dtr = AmbilData("aplikasi1-m5_do", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml do yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(doid) AS jml FROM m5_do do JOIN m5_do_detail dod ON do.doid = dod.iddo JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail JOIN m5_so so ON so.soid = sod.idso"
        filtera = "dostatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_do", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M5_JmlTransaksiSI(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sistatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sistatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sitgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(sitgl) = '" + Bulan.ToString + "' AND YEAR(sitgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(siid) AS jml FROM `m5_si`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_SIOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sistatus IN (2,3) AND " + Filter
        Else
            Filter = " sistatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sitgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND sitgl <= '" + Tahun.ToString + "-" + Bulan.ToString + "-31'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(siid) AS jml FROM `m5_si`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_PemenuhanSI(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sitgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(sitgl) = '" + Bulan.ToString + "' AND YEAR(sitgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT COUNT(siid) AS jml FROM m5_si"
        filterr = "sistatus IN (4) " + Filter
        dtr = AmbilData("aplikasi1-m5_si", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT COUNT(siid) AS jml FROM m5_si"
        filtera = "sistatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_si", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M5_KecepatanSI(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND sitgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(sitgl) = '" + Bulan.ToString + "' AND YEAR(sitgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(si.siid) AS jml FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi JOIN m5_do_detail dod ON dod.iddodetail = sid.iddodetail JOIN m5_do do ON do.doid = dod.iddo"
        filterr = "si.sistatus IN (2,3,4,7) AND si.sitgl <= do.dotglkirim " + Filter
        dtr = AmbilData("aplikasi1-m5_si", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml si yg menggunakan PR
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(siid) AS jml FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi JOIN m5_do_detail dod ON dod.iddodetail = sid.iddodetail JOIN m5_do do ON do.doid = dod.iddo"
        filtera = "sistatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_si", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If

        Return Hasil
    End Function

    Public Function M5_JmlTransaksiSR(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " srstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " srstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND srtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(srtgl) = '" + Bulan.ToString + "' AND YEAR(srtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(srid) AS jml FROM `m5_sr`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_JmlTransaksiPV(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " pvstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " pvstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND pvtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(pvtgl) = '" + Bulan.ToString + "' AND YEAR(pvtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(pvid) AS jml FROM `m5_pv`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M5_KecepatanPV(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND pvtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(pvtgl) = '" + Bulan.ToString + "' AND YEAR(pvtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'ambil jml realisasi kecepatan
        Dim sqlr As String = ""
        Dim filterr As String = ""
        Dim jmlrealisasi As Double = 0
        Dim dtr As New DataTable
        sqlr = "SELECT DISTINCT COUNT(pvid) AS jml FROM m5_pv pv JOIN m5_pv_detail pvd ON pv.pvid = pvd.idpv JOIN m5_si si ON si.siid = pvd.idtransaksi AND pvd.sumber = 'SI'"
        filterr = "pv.pvstatus IN (2,3,4,7) And pvtgl <= sitgljatuhtempo " + Filter
        dtr = AmbilData("aplikasi1-m5_pv", filterr, Sorting, True, , , 1, 20, pg1, , , , sqlr) ' Ambil data ke databases
        If dtr.Rows.Count > 0 Then
            jmlrealisasi = FxDB(dtr.Rows(0)(0), 0)
        Else
            jmlrealisasi = 0
        End If

        'ambil jml PV yg menggunakan SI
        Dim sqla As String = ""
        Dim filtera As String = ""
        Dim jml As Double = 0
        Dim dta As New DataTable
        sqla = "SELECT DISTINCT COUNT(pvid) AS jml FROM m5_pv pv JOIN m5_pv_detail pvd ON pv.pvid = pvd.idpv JOIN m5_si si ON si.siid = pvd.idtransaksi AND pvd.sumber = 'SI'"
        filtera = "pv.pvstatus IN (2,3,4,7) " + Filter
        dta = AmbilData("aplikasi1-m5_pv", filtera, Sorting, True, , , 1, 20, pg1, , , , sqla) ' Ambil data ke databases
        If dta.Rows.Count > 0 Then
            jml = FxDB(dta.Rows(0)(0), 0)
        Else
            jml = 0
        End If

        If (jml <> 0) Then
            Hasil = (jmlrealisasi / jml) * 100
        End If
        Return Hasil
    End Function

    Public Function M5_CustomerBaru(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " kkategori = 'C' AND " + Filter
        Else
            Filter = " kkategori = 'C' "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND DATE(kinputtgl) BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(kinputtgl) = '" + Bulan.ToString + "' AND YEAR(kinputtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(kid) AS jml FROM m1_contact"

        dt = AmbilData("aplikasr1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function


#End Region

#Region "M6 Content"

    Public Function M6_JmlProduksi(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " pdstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " pdstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND pdtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(pdtgl) = '" + Bulan.ToString + "' AND YEAR(pdtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT SUM(pi.jml) AS jml FROM m6_pd pd JOIN m6_pd_in pi ON pd.pdid = pi.idpd"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M6_JmlReject(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " pdgudangproduksi = 'R' AND pdstatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " pdgudangproduksi = 'R' AND pdstatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND pdtgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(pdtgl) = '" + Bulan.ToString + "' AND YEAR(pdtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT SUM(pi.jml) AS jml FROM m6_pd pd JOIN m6_pd_in pi ON pd.pdid = pi.idpd"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M6_JmlWaste(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sajenis = 'PRODUKSI' AND sastatus IN (2,3,4,7) AND " + Filter
        Else
            Filter = " sajenis = 'PRODUKSI' AND sastatus IN (2,3,4,7) "
        End If

        'filter Tgl
        If (Tgl1.Length > 0 And Tgl2.Length > 0) Then
            Filter += " AND satgl BETWEEN '" + Tgl1 + "' AND '" + Tgl2 + "'"
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(said) AS jml FROM `m3_sa`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M6_PersentaseBagus(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim jml As Double = 0
        Dim jmlBagus As Double = 0

        jml = M6_JmlProduksi(Bulan, Tahun, Tgl1, Tgl2, Filter)
        jmlBagus = jml - M6_JmlReject(Bulan, Tahun, Tgl1, Tgl2, Filter)

        Hasil = Math.Round((jmlBagus / jml) * 100, 2)

        Return Hasil
    End Function

    Public Function M6_PersentaseReject(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim jml As Double = 0
        Dim jmlReject As Double = 0

        jml = M6_JmlProduksi(Bulan, Tahun, Tgl1, Tgl2, Filter)
        jmlReject = M6_JmlReject(Bulan, Tahun, Tgl1, Tgl2, Filter)

        Hasil = Math.Round((jmlReject / jml) * 100, 2)

        Return Hasil
    End Function

    Public Function M6_PersentaseWaste(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim jml As Double = 0
        Dim jmlWaste As Double = 0

        jml = M6_JmlProduksi(Bulan, Tahun, Tgl1, Tgl2, Filter)
        jmlWaste = M6_JmlWaste(Bulan, Tahun, Tgl1, Tgl2, Filter)

        Hasil = Math.Round((jmlWaste / jml) * 100, 2)

        Return Hasil
    End Function

    Public Function M6_PermintaanProduksiOutstanding(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " pdrstatus IN (2,3) AND " + Filter
        Else
            Filter = " pdrstatus IN (2,3) "
        End If

        'filter Tgl
        If (Tgl2.Length > 0) Then
            Filter += " AND pdrtgl <= " + Tgl2
        End If

        'jika menggunakan filter bulan/tahun
        If (Tahun <> 0 And Bulan <> 0) Then
            Filter += " AND MONTH(pdrtgl) = '" + Bulan.ToString + "' AND YEAR(pdrtgl) = '" + Tahun.ToString + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(pdrid) AS jml FROM m6_pdr"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M6_BarangProduksiterbanyak(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sastatus IN (2,3,4,7) AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'" + "' AND " + Filter
        Else
            Filter = " sastatus IN (2,3,4,7) AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'" + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(said) AS jml FROM `m3_sa`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

    Public Function M6_PersentareRealisasivsFormula(ByVal Bulan As Integer, ByVal Tahun As Integer, ByVal Tgl1 As String, ByVal Tgl2 As String, ByVal Filter As String) As Double
        Dim Hasil As Double = 0

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Sorting As String = ""
        Dim dt As New DataTable

        If (Filter.Length > 0) Then
            Filter = " sastatus IN (2,3,4,7) AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'" + "' AND " + Filter
        Else
            Filter = " sastatus IN (2,3,4,7) AND MONTH(satgl) = '" + Bulan.ToString + "' AND YEAR(satgl) = '" + Tahun.ToString + "'" + "'"
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        sql = "SELECT COUNT(said) AS jml FROM `m3_sa`"

        dt = AmbilData("aplikasi1-m8_content", Filter, Sorting, True, , , 1, 20, pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            Hasil = FxDB(dt.Rows(0)(0), 0)
        Else
            Hasil = 0
        End If

        Return Hasil
    End Function

#End Region

End Class
