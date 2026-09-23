Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_hapus_data
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    Dim errMessage As String = ""

    <WebMethod()>
    Public Function m0_hapus_dataGetMapTransaksi(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
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
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================



        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'apkode(0) As Integer, aptahun(1) As Integer, apbulan(2) As Integer, apaktif(3) As Integer, aptutupperiode(4) As Integer


        'MAPPING BUAT FLEX --------------------------------------------------------
        'apkode, aptahun, apbulan, apaktif, aptutupperiode

        'SPILIT PARAMETER DATA

        'SIMPAN KE DATABASE ==========================================================
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try
            '*** Start Transaction ***'  
            Dim dt As DataTable, arrtabel As String = ""
            dt = AsDataTableAmbilDariDB("SHOW TABLES")
            For Each dr As DataRow In dt.Rows
                If dr(0).ToString.Contains("m2_") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") And Not dr(0).ToString.Contains("print") And Not dr(0).ToString.Contains("template") And Not dr(0).ToString.Contains("pengajuan_dana") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m2r_") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m3_") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m4_") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m5_") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m6_") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                    arrtabel += dr(0) + sptRow
                ElseIf dr(0).ToString.Contains("m7_") And Not dr(0).ToString.Contains("asset_category") And Not dr(0).ToString.Contains("depreciation_category") Then
                    arrtabel += dr(0) + sptRow
                End If
            Next

            If arrtabel.ToString.Length <> 0 Then
                search = arrtabel.Substring(0, arrtabel.Length - 1)
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        '
        '
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData, sptParam, "tabel")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function m0_hapus_dataSimpan(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        'Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
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
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================



        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isupdate required numeric." : GoTo selesai
        Else
            'SET ISUPDATE
            If (Val(paramSplit(4)) = 1) Then
                isUpdate = True
            Else
                isUpdate = False
            End If
        End If
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'apkode(0) As Integer, aptahun(1) As Integer, apbulan(2) As Integer, apaktif(3) As Integer, aptutupperiode(4) As Integer


        'MAPPING BUAT FLEX --------------------------------------------------------
        'apkode, aptahun, apbulan, apaktif, aptutupperiode

        'SPILIT PARAMETER DATA

        'SIMPAN KE DATABASE ==========================================================

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dt As New DataTable
        Dim rowUpdate As Integer = 0
        Dim arrtabel As String = ""
        Try
            Select Case Int(paramSplit(5))
                Case 1 'Transaksi
                    If truncate(paramSplit(2).Split(sptSubParam)(2)) <> "" Then result(2) = errMessage : GoTo selesai
                Case 2 'Akun 
                    If paket("Akun") <> "" Then result(2) = errMessage : GoTo selesai
                Case 3 'Barang dan Informasi Barang
                    If paket("Barang") <> "" Then result(2) = errMessage : GoTo selesai
                    If paket("Informasi Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 4 'Tipe Barang
                    If paket("Tipe Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 5 'Tipe Transaksi Barang
                    If paket("Tipe Transaksi Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 6 'Kageori Barang
                    If paket("Kageori Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 7 'Hak Akses Barang
                    If paket("Hak Akses Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 8 'Kontak
                    If paket("Kontak") <> "" Then result(2) = errMessage : GoTo selesai
                Case 9 'Kategori Kontak
                    If paket("Kategori Kontak") <> "" Then result(2) = errMessage : GoTo selesai
                Case 10 'Area
                    If paket("Area") <> "" Then result(2) = errMessage : GoTo selesai
                Case 11 'Bank
                    If paket("Bank") <> "" Then result(2) = errMessage : GoTo selesai
                Case 12 'Biaya Lain
                    If paket("Biaya Lain") <> "" Then result(2) = errMessage : GoTo selesai
                Case 13 'Cabang
                    If paket("Cabang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 14 'Catatan Transaksi
                    If paket("Catatan Transaksi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 15 'Catatan Transaksi Detail
                    If paket("Catatan Transaksi Detail") <> "" Then result(2) = errMessage : GoTo selesai
                Case 16 'Cost Center
                    If paket("Cost Center") <> "" Then result(2) = errMessage : GoTo selesai
                Case 17 'Divisi
                    If paket("Divisi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 18 'Ekspedisi
                    If paket("Ekspedisi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 19 'Estimasi Kerja
                    If paket("Estimasi Kerja") <> "" Then result(2) = errMessage : GoTo selesai
                Case 20 'Gudang
                    If paket("Gudang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 21 'Kategori Pelanggan
                    If paket("Kategori Pelanggan") <> "" Then result(2) = errMessage : GoTo selesai
                Case 22 'Kategori Pemasok
                    If paket("Kategori Pemasok") <> "" Then result(2) = errMessage : GoTo selesai
                Case 23 'Kategori Produksi
                    If paket("Kategori Produksi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 24 'Kategeori Salesman
                    If paket("Kategeori Salesman") <> "" Then result(2) = errMessage : GoTo selesai
                Case 25 'Kota
                    If paket("Kota") <> "" Then result(2) = errMessage : GoTo selesai
                Case 26 'Lain-lain
                    If paket("Lain-lain") <> "" Then result(2) = errMessage : GoTo selesai
                Case 27 'Lokasi
                    If paket("Lokasi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 28 'Lokasi Barang
                    If paket("Lokasi Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 29 'Mata Uang
                    If paket("Mata Uang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 30 'Negara
                    If paket("Negara") <> "" Then result(2) = errMessage : GoTo selesai
                Case 31 'Pajak
                    If paket("Pajak") <> "" Then result(2) = errMessage : GoTo selesai
                Case 32 'Propinsi
                    If paket("Propinsi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 33 'Proyek
                    If paket("Proyek") <> "" Then result(2) = errMessage : GoTo selesai
                Case 34 'Satuan
                    If paket("Satuan") <> "" Then result(2) = errMessage : GoTo selesai
                Case 35 'Sub Divisi
                    If paket("Sub Divisi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 36 'Termin
                    If paket("Termin") <> "" Then result(2) = errMessage : GoTo selesai
                Case 37 'Tipe Barang
                    If paket("Tipe Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 38 'Tipe Transaksi Barang
                    If paket("Tipe Transaksi Barang") <> "" Then result(2) = errMessage : GoTo selesai
                Case 39 'Barang Hauling
                    If paket("Barang Hauling") <> "" Then result(2) = errMessage : GoTo selesai
                Case 40 'Kategori Pengecekan
                    If paket("Kategori Pengecekan") <> "" Then result(2) = errMessage : GoTo selesai
                Case 41 'Kategori Poin
                    If paket("Kategori Poin") <> "" Then result(2) = errMessage : GoTo selesai
                Case 42 'Kelas Produk
                    If paket("Kelas Produk") <> "" Then result(2) = errMessage : GoTo selesai
                Case 43 'Indeks Harga
                    If paket("Indeks Harga") <> "" Then result(2) = errMessage : GoTo selesai
                Case 44 'Departemen
                    If paket("Departemen") <> "" Then result(2) = errMessage : GoTo selesai
                Case 45 'Sub Departemen
                    If paket("Sub Departemen") <> "" Then result(2) = errMessage : GoTo selesai
                Case 46 'Komisi
                    If paket("Komisi") <> "" Then result(2) = errMessage : GoTo selesai
                Case 47 'Kategori Harga
                    If paket("Kategori Harga") <> "" Then result(2) = errMessage : GoTo selesai
                Case 48 'Tenaga Kerja
                    If paket("Tenaga Kerja") <> "" Then result(2) = errMessage : GoTo selesai
                Case 49 'Mesin
                    If paket("Mesin") <> "" Then result(2) = errMessage : GoTo selesai
            End Select

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        '
        '
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function m0_hapus_datasearch(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        'M2_Accounting_PeriodSearch --------------------------------------------------------
        'apkode, aptahun, apbulan, apaktif, aptutupperiode, apbulannama, apaktifnama, aptutupperiodenama

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
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "SELECT * FROM m0_hapusdata"

        'BUKA KONEKSI

        dt = AmbilData("aplikasi1-", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("id"), ""), sptField,
                             FxDB(dr("nama"), ""), sptField,
                             FxDB(dr("keterangan"), ""), sptField, sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Hapus Data data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("id, nama, keterangan"))

        Return wsResult
    End Function

    Function paket(ByVal menu As String) As String
        Dim dt As DataTable
        Select Case menu

            'Hapus Transaksi by modul
            Case "m1_transaksi"
                If truncate("m1_cogs_fifo_in") <> "" Then Return errMessage
                If truncate("m1_cogs_fifo_out") <> "" Then Return errMessage
                If truncate("m1_cogs_special_in") <> "" Then Return errMessage
                If truncate("m1_cogs_special_out") <> "" Then Return errMessage
                If truncate("m1_item_booking") <> "" Then Return errMessage
                If truncate("m1_item_booking_po") <> "" Then Return errMessage
                If truncate("m1_item_picking_list") <> "" Then Return errMessage
                If truncate("m1_item_stock_warehouse") <> "" Then Return errMessage
                If truncate("m1_item_stock_warehouse_log") <> "" Then Return errMessage
                If truncate("m1_item_transaction") <> "" Then Return errMessage
                If truncate("m1_no_batch_in") <> "" Then Return errMessage
                If truncate("m1_no_batch_out") <> "" Then Return errMessage
                If truncate("m1_no_batch_transaction") <> "" Then Return errMessage
                If truncate("m1_no_serial_in") <> "" Then Return errMessage
                If truncate("m1_no_serial_out") <> "" Then Return errMessage
                If truncate("m1_no_serial_transaction") <> "" Then Return errMessage
            Case "m2_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m2_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") And Not dr(0).ToString.Contains("print") And Not dr(0).ToString.Contains("template") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m2r_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m2r_") And dr(0).ToString.Contains("hisotry") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m3_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m3_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m4_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m4_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m5_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m5_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m6_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m6_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("files") And Not dr(0).ToString.Contains("notes") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next
            Case "m7_transaksi"
                dt = AsDataTableAmbilDariDB("SHOW TABLES")
                For Each dr As DataRow In dt.Rows
                    If dr(0).ToString.Contains("m7_") And Not dr(0).ToString.Contains("hisotry") And Not dr(0).ToString.Contains("asset_category") And Not dr(0).ToString.Contains("depreciation_category") Then
                        If truncate(dr(0)) <> "" Then Return errMessage
                    End If
                Next

            Case "m_12_transaksi"
                If truncate("m_12_ai") <> "" Then Return errMessage
                If truncate("m_12_ai_additional") <> "" Then Return errMessage
                If truncate("m_12_ai_detail") <> "" Then Return errMessage
                If truncate("m_12_bi") <> "" Then Return errMessage
                If truncate("m_12_bi_bonus") <> "" Then Return errMessage
                If truncate("m_12_bi_detail") <> "" Then Return errMessage
                If truncate("m_12_cpa") <> "" Then Return errMessage
                If truncate("m_12_cpa_detail") <> "" Then Return errMessage
                If truncate("m_12_di") <> "" Then Return errMessage
                If truncate("m_12_di_detail") <> "" Then Return errMessage
                If truncate("m_12_lp") <> "" Then Return errMessage
                If truncate("m_12_lp_detail") <> "" Then Return errMessage
                If truncate("m_12_pos_voucher_in") <> "" Then Return errMessage
                If truncate("m_12_pos_voucher_out") <> "" Then Return errMessage
                If truncate("m_12_ppa") <> "" Then Return errMessage
                If truncate("m_12_ppa_detail") <> "" Then Return errMessage
                If truncate("m_12_sbi") <> "" Then Return errMessage
                If truncate("m_12_sbi_detail") <> "" Then Return errMessage
                If truncate("m_12_sbi_substitution") <> "" Then Return errMessage
                If truncate("m_12_st") <> "" Then Return errMessage
                If truncate("m_12_st_detail") <> "" Then Return errMessage

                'Hapus History by modul
            Case "m1_history" : If truncatehistory("m1") <> "" Then Return errMessage
            Case "m2_history" : If truncatehistory("m2") <> "" Then Return errMessage
            Case "m3_history" : If truncatehistory("m3") <> "" Then Return errMessage
            Case "m4_history" : If truncatehistory("m4") <> "" Then Return errMessage
            Case "m5_history" : If truncatehistory("m5") <> "" Then Return errMessage
            Case "m6_history" : If truncatehistory("m6") <> "" Then Return errMessage
            Case "m7_history" : If truncatehistory("m7") <> "" Then Return errMessage
            Case "m8_history" : If truncatehistory("m8") <> "" Then Return errMessage
            Case "m9_history" : If truncatehistory("m9") <> "" Then Return errMessage
            Case "m_10_history" : If truncatehistory("m_10") <> "" Then Return errMessage
            Case "m_11_history" : If truncatehistory("m_11") <> "" Then Return errMessage
            Case "m_12_history" : If truncatehistory("m_12") <> "" Then Return errMessage

                'Hapus Master by paket
            Case "Akun"
                If truncate("m1_coa") <> "" Then Return errMessage
                If truncate("m1_coa_history") <> "" Then Return errMessage
            Case "Area"
                If truncate("m1_area") <> "" Then Return errMessage
                If truncate("m1_area_history") <> "" Then Return errMessage
            Case "Bank"
                If truncate("m1_bank") <> "" Then Return errMessage
                If truncate("m1_bank_history") <> "" Then Return errMessage
            Case "Barang"
                If truncate("m1_coa") <> "" Then Return errMessage
                If truncate("m1_coa_history") <> "" Then Return errMessage
            Case "Biaya Lain"
                If truncate("m1_other_cost") <> "" Then Return errMessage
                If truncate("m1_other_cost_history") <> "" Then Return errMessage
            Case "Cabang"
                If truncate("m1_branch") <> "" Then Return errMessage
                If truncate("m1_branch_history") <> "" Then Return errMessage
            Case "Catatan Transaksi"
                If truncate("m1_transaction_note") <> "" Then Return errMessage
                If truncate("m1_transaction_note_history") <> "" Then Return errMessage
            Case "Catatan Transaksi Detail"
                If truncate("m1_transaction_note_detail") <> "" Then Return errMessage
                If truncate("m1_transaction_note_detail_history") <> "" Then Return errMessage
            Case "Cost Center"
                If truncate("m1_cost_center") <> "" Then Return errMessage
                If truncate("m1_cost_center_history") <> "" Then Return errMessage
            Case "Divisi"
                If truncate("m1_division") <> "" Then Return errMessage
                If truncate("m1_division_history") <> "" Then Return errMessage
            Case "Ekspedisi"
                If truncate("m1_expedition") <> "" Then Return errMessage
                If truncate("m1_expedition_history") <> "" Then Return errMessage
            Case "Estimasi Kerja"
                If truncate("m1_working_estimate") <> "" Then Return errMessage
                If truncate("m1_working_estimate_history") <> "" Then Return errMessage
            Case "Gudang"
                If truncate("m1_warehouse") <> "" Then Return errMessage
                If truncate("m1_warehouse_history") <> "" Then Return errMessage
            Case "Informasi Barang"
                If truncate("m1_item_supplier") <> "" Then Return errMessage
                If truncate("m1_item_supplier_history") <> "" Then Return errMessage
                If truncate("m1_item_assembly") <> "" Then Return errMessage
                If truncate("m1_item_assembly_history") <> "" Then Return errMessage
                If truncate("m1_item_location_warehouse") <> "" Then Return errMessage
                If truncate("m1_item_location_warehouse_history") <> "" Then Return errMessage
            Case "Kategori Barang"
                If truncate("m1_item_category") <> "" Then Return errMessage
                If truncate("m1_item_category_history") <> "" Then Return errMessage
            Case "Kategori Kontak"
                If truncate("m1_contact_category") <> "" Then Return errMessage
                If truncate("m1_contact_category_history") <> "" Then Return errMessage
            Case "Kategori Pelanggan"
                If truncate("m1_customer_category") <> "" Then Return errMessage
                If truncate("m1_customer_category_history") <> "" Then Return errMessage
            Case "Kategori Pemasok"
                If truncate("m1_supplier_category") <> "" Then Return errMessage
                If truncate("m1_supplier_category_history") <> "" Then Return errMessage
            Case "Kategori Produksi"
                If truncate("m1_production_category") <> "" Then Return errMessage
                If truncate("m1_production_category_history") <> "" Then Return errMessage
            Case "Kategori Salesman"
                If truncate("m1_salesman_category") <> "" Then Return errMessage
                If truncate("m1_salesman_category_history") <> "" Then Return errMessage
            Case "Kontak"
                If truncate("m1_contact") <> "" Then Return errMessage
                If truncate("m1_contact_history") <> "" Then Return errMessage
            Case "Kota"
                If truncate("m1_city") <> "" Then Return errMessage
                If truncate("m1_city_history") <> "" Then Return errMessage
            Case "Lain-lain"
                If truncate("m1_other") <> "" Then Return errMessage
                If truncate("m1_other_history") <> "" Then Return errMessage
            Case "Lokasi"
                If truncate("m1_location") <> "" Then Return errMessage
                If truncate("m1_location_history") <> "" Then Return errMessage
            Case "Lokasi Barang"
                If truncate("m1_item_location") <> "" Then Return errMessage
                If truncate("m1_item_location_history") <> "" Then Return errMessage
            Case "Mata Uang"
                If truncate("m1_currency") <> "" Then Return errMessage
                If truncate("m1_currency_history") <> "" Then Return errMessage
            Case "Negara"
                If truncate("m1_country") <> "" Then Return errMessage
                If truncate("m1_country_history") <> "" Then Return errMessage
            Case "Pajak"
                If truncate("m1_tax") <> "" Then Return errMessage
                If truncate("m1_tax_history") <> "" Then Return errMessage
            Case "Propinsi"
                If truncate("m1_province") <> "" Then Return errMessage
                If truncate("m1_province_history") <> "" Then Return errMessage
            Case "Proyek"
                If truncate("m1_project") <> "" Then Return errMessage
                If truncate("m1_project_history") <> "" Then Return errMessage
            Case "satuan"
                If truncate("m1_unit") <> "" Then Return errMessage
                If truncate("m1_unit_history") <> "" Then Return errMessage
            Case "Sub Divisi"
                If truncate("m1_subdivision") <> "" Then Return errMessage
                If truncate("m1_subdivision_history") <> "" Then Return errMessage
            Case "Termin"
                If truncate("m1_terms") <> "" Then Return errMessage
                If truncate("m1_terms_history") <> "" Then Return errMessage
            Case "Tipe Barang"
                If truncate("m1_item_type") <> "" Then Return errMessage
                If truncate("m1_item_type_history") <> "" Then Return errMessage
            Case "Tipe Transaksi Barang"
                If truncate("m1_type_sa") <> "" Then Return errMessage
                If truncate("m1_type_sa_history") <> "" Then Return errMessage
            Case "Barang Hauling"
                If truncate("m1_item_hauling") <> "" Then Return errMessage
                If truncate("m1_item_hauling_history") <> "" Then Return errMessage
            Case "Kategori Pengecekan"
                If truncate("m1_checking_category") <> "" Then Return errMessage
                If truncate("m1_checking_category_history") <> "" Then Return errMessage
            Case "Kategori Poin"
                If truncate("m1_selling_point") <> "" Then Return errMessage
                If truncate("m1_selling_point_history") <> "" Then Return errMessage
            Case "Kelas Produk"
                If truncate("m1_class_product") <> "" Then Return errMessage
                If truncate("m1_class_product_history") <> "" Then Return errMessage
            Case "Indeks Harga"
                If truncate("m1_index_price") <> "" Then Return errMessage
                If truncate("m1_index_price_history") <> "" Then Return errMessage
            Case "Departemen"
                If truncate("m1_department") <> "" Then Return errMessage
                If truncate("m1_department_history") <> "" Then Return errMessage
            Case "Sub Departemen"
                If truncate("m1_subdepartment") <> "" Then Return errMessage
                If truncate("m1_subdepartment_history") <> "" Then Return errMessage
            Case "Komisi"
                If truncate("m1_commission") <> "" Then Return errMessage
                If truncate("m1_commission_history") <> "" Then Return errMessage
            Case "Kategori Harga"
                If truncate("m1_price_category") <> "" Then Return errMessage
                If truncate("m1_price_category_history") <> "" Then Return errMessage
            Case "Hak Akses Barang"
                If truncate("m1_item_permission") <> "" Then Return errMessage
                If truncate("m1_item_permission_history") <> "" Then Return errMessage
            Case "Tenaga Kerja"
                If truncate("m1_working_estimate") <> "" Then Return errMessage
                If truncate("m1_working_estimate_history") <> "" Then Return errMessage
            Case "Mesin"
                If truncate("m1_machine") <> "" Then Return errMessage
                If truncate("m1_machine_history") <> "" Then Return errMessage
        End Select
        Return ""
    End Function

    Function truncatehistory(ByVal modul As String) As String
        Try
            Dim dt As DataTable
            dt = AsDataTableAmbilDariDB("SHOW TABLES")
            For Each dr As DataRow In dt.Rows
                If dr(0).ToString.Contains(modul + "_") And dr(0).ToString.Contains("hisotry") Then
                    If truncate(dr(0)) <> "" Then Return errMessage
                End If
            Next
            Return ""
        Catch ex As Exception
            errMessage = Err.Description
            Return Err.Description
        End Try
    End Function

    Function truncate(ByVal namatabel As String) As String
        Try
            With New MySql.Data.MySqlClient.MySqlCommand()
                .Connection = Con1
                '.Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = "DELETE FROM " + namatabel
                .ExecuteNonQuery()
            End With
            Return ""
        Catch ex As Exception
            errMessage = Err.Description + "@DELETE FROM " + namatabel
            Return Err.Description
        End Try
    End Function
End Class