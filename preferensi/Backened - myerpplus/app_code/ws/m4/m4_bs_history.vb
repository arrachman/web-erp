Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_bs_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Bs_HistorySimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

        Dim sumber As String = "", idtransaksi As String = ""

        'SET DEFAULT RESULT
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
        'sumber(0) As String, idtransaksi(1) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'sumber, idtransaksi


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 2) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI DATA UTAMA ===============================================================
        'sumber(0) As String
        If Len(dataUtama(0)) = 0 Then
            result(2) = "sumber can't be empty" : GoTo selesai
        Else
            sumber = dataUtama(0)
        End If

        'idtransaksi(1) As Integer
        If (IsNumeric(dataUtama(1)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            idtransaksi = dataUtama(1)
        End If
        'END OF VALIDASI DATA UTAMA ========================================================


        'SIMPAN KE DATABASE ================================================================
        Con2 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con2.Open()

        '*** Start Transaction ***'  
        Trans = Con2.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m4_bs_history(SELECT 0, bs.* FROM m4_bs bs WHERE bs.bsid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------


            'PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT --------------------
            Dim dt2 As New DataTable
            sql = "SELECT bsidhistory FROM m4_bs_history WHERE bsid = '" & idtransaksi & "' ORDER BY bsmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_bs_detail_history (SELECT 0, '" & result(4) & "', bs.* FROM m4_bs_detail bs WHERE bs.idbs = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'Con2.Close()
        'Con2 = Nothing
        'END OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Bs_HistorySearch(ByVal param As String) As String
        'M4_Bs_HistorySearch --------------------------------------------------------
        'bsidhistory, bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bsnotransaksirq2, 
        'bsnotransaksirq3, bsnotransaksirq4, bsnotransaksirq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama

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
            Filter = Filter.Replace("bsbagianperbandingankode", "c1.kkode")
            Filter = Filter.Replace("bsbagianperbandingannama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_bs_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Bs_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("bsid"), 0), sptField,
                     FxDB(dr("bsidhistory"), 0), sptField,
                     FxDB(dr("bscabang"), ""), sptField,
                     FxDB(dr("bslokasi"), ""), sptField,
                     FxDB(dr("bsgudang"), ""), sptField,
                     FxDB(dr("bsasalbarang"), ""), sptField,
                     FxDB(dr("bsasalbarangkategori"), 0), sptField,
                     FxDB(dr("bsjenispembelian"), ""), sptField,
                     FxDB(dr("bsjenispembeliankategori"), 0), sptField,
                     FxDB(dr("bscarabayar"), 0), sptField,
                     FxDB(dr("bssumber"), ""), sptField,
                     FxDB(dr("bsnogrup"), ""), sptField,
                     FxDB(dr("bsautonotransaksi"), 0), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bstgl"), ""), formatTgl), sptField,
                     FxDB(dr("bskodepa"), 0), sptField,
                     FxDB(dr("bsbagianperbandingan"), 0), sptField,
                     FxDB(dr("bsbagianperbandingankontak"), ""), sptField,
                     FxDB(dr("bsuraian"), ""), sptField,
                     FxDB(dr("bscatatan"), ""), sptField,
                     FxDB(dr("bsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bstglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bstglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("bsmatauang"), ""), sptField,
                     FxDB(dr("bsidrq1"), 0), sptField,
                     FxDB(dr("bsidrq2"), 0), sptField,
                     FxDB(dr("bsidrq3"), 0), sptField,
                     FxDB(dr("bsidrq4"), 0), sptField,
                     FxDB(dr("bsidrq5"), 0), sptField,
                     FxDB(dr("bsidrq1statuspo"), 0), sptField,
                     FxDB(dr("bsidrq2statuspo"), 0), sptField,
                     FxDB(dr("bsidrq3statuspo"), 0), sptField,
                     FxDB(dr("bsidrq4statuspo"), 0), sptField,
                     FxDB(dr("bsidrq5statuspo"), 0), sptField,
                     FxDB(dr("bsstatus"), 0), sptField,
                     FxDB(dr("bsstatussebelumnya"), 0), sptField,
                     FxDB(dr("bsjmlrevisi"), 0), sptField,
                     FxDB(dr("bscetakanke"), 0), sptField,
                     FxDB(dr("bsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bsisclose"), 0), sptField,
                     FxDB(dr("bscabangnama"), ""), sptField,
                     FxDB(dr("bslokasinama"), ""), sptField,
                     FxDB(dr("bsgudangnama"), ""), sptField,
                     FxDB(dr("bsbagianperbandingankode"), ""), sptField,
                     FxDB(dr("bsbagianperbandingannama"), ""), sptField,
                     FxDB(dr("bsnotransaksirq1"), ""), sptField,
                     FxDB(dr("bsnotransaksirq2"), ""), sptField,
                     FxDB(dr("bsnotransaksirq3"), ""), sptField,
                     FxDB(dr("bsnotransaksirq4"), ""), sptField,
                     FxDB(dr("bsnotransaksirq5"), ""), sptField,
                     FxDB(dr("bsstatusnama"), ""), sptField,
                     FxDB(dr("bsstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("bsinputusernama"), ""), sptField,
                     FxDB(dr("bsmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bsidhistory, bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bsnotransaksirq2, bsnotransaksirq3, bsnotransaksirq4, bsnotransaksirq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_BsHistoryGetdataById(ByVal param As String) As String

        'M4_BsHistoryGetdataById Utama --------------------------------------------------------
        'bsidhistory, bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, 
        'bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, 
        'bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, 
        'bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, 
        'bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, 
        'bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, 
        'bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, 
        'bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3, 
        'bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bssupplierrq1, 
        'bssupplierkoderq1, bssuppliernamarq1, bsterminrq1, bsterminnamarq1, bsterminharijatuhtemporq1, bsnotransaksirq2, bssupplierrq2, 
        'bssupplierkoderq2, bssuppliernamarq2, bsterminrq2, bsterminnamarq2, bsterminharijatuhtemporq2, bsnotransaksirq3, bssupplierrq3, 
        'bssupplierkoderq3, bssuppliernamarq3, bsterminrq3, bsterminnamarq3, bsterminharijatuhtemporq3, bsnotransaksirq4, bssupplierrq4, 
        'bssupplierkoderq4, bssuppliernamarq4, bsterminrq4, bsterminnamarq4, bsterminharijatuhtemporq4, bsnotransaksirq5, bssupplierrq5, 
        'bssupplierkoderq5, bssuppliernamarq5, bsterminrq5, bsterminnamarq5, bsterminharijatuhtemporq5, bsstatusnama, bsstatussebelumnyanama, 
        'bsinputusernama, bsmodifikasiusernama

        'M4_BsHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idbsdetail, idbs, idrqdetail, terpilih, hargake, 
        'catatan, urutan, idrq, idbarang, namabarang, tipebarang, jml, 
        'satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, kodebarang, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", idtransaksi As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0
        result(2) = ""
        result(3) = 0
        result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0
        resultPaging(1) = 0
        resultPaging(2) = 0
        resultPaging(3) = 0
        resultPaging(4) = 0

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        Dim NmMemcached As String = "aplikasi1-M4_Bs_history~M4_Bs_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "bsidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "bsidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_bs_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("bsidhistory"), 0), sptField,
                     FxDB(drutama("bsid"), 0), sptField,
                     FxDB(drutama("bscabang"), ""), sptField,
                     FxDB(drutama("bslokasi"), ""), sptField,
                     FxDB(drutama("bsgudang"), ""), sptField,
                     FxDB(drutama("bsasalbarang"), ""), sptField,
                     FxDB(drutama("bsasalbarangkategori"), 0), sptField,
                     FxDB(drutama("bsjenispembelian"), ""), sptField,
                     FxDB(drutama("bsjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("bscarabayar"), 0), sptField,
                     FxDB(drutama("bssumber"), ""), sptField,
                     FxDB(drutama("bsnogrup"), ""), sptField,
                     FxDB(drutama("bsautonotransaksi"), 0), sptField,
                     FxDB(drutama("bsnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bstgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bskodepa"), 0), sptField,
                     FxDB(drutama("bsbagianperbandingan"), 0), sptField,
                     FxDB(drutama("bsbagianperbandingankontak"), ""), sptField,
                     FxDB(drutama("bsuraian"), ""), sptField,
                     FxDB(drutama("bscatatan"), ""), sptField,
                     FxDB(drutama("bsnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("bstglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bstglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("bsmatauang"), ""), sptField,
                     FxDB(drutama("bsidrq1"), 0), sptField,
                     FxDB(drutama("bsidrq2"), 0), sptField,
                     FxDB(drutama("bsidrq3"), 0), sptField,
                     FxDB(drutama("bsidrq4"), 0), sptField,
                     FxDB(drutama("bsidrq5"), 0), sptField,
                     FxDB(drutama("bsidrq1statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq2statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq3statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq4statuspo"), 0), sptField,
                     FxDB(drutama("bsidrq5statuspo"), 0), sptField,
                     FxDB(drutama("bsstatus"), 0), sptField,
                     FxDB(drutama("bsstatussebelumnya"), 0), sptField,
                     FxDB(drutama("bsjmlrevisi"), 0), sptField,
                     FxDB(drutama("bscetakanke"), 0), sptField,
                     FxDB(drutama("bsinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bsinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bsmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bsmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bsisclose"), 0), sptField,
                     FxDB(drutama("bscustomtext1"), ""), sptField,
                     FxDB(drutama("bscustomtext2"), ""), sptField,
                     FxDB(drutama("bscustomtext3"), ""), sptField,
                     FxDB(drutama("bscustomtext4"), ""), sptField,
                     FxDB(drutama("bscustomtext5"), ""), sptField,
                     FxDB(drutama("bscustomint1"), 0), sptField,
                     FxDB(drutama("bscustomint2"), 0), sptField,
                     FxDB(drutama("bscustomint3"), 0), sptField,
                     FxDB(drutama("bscustomdbl1"), 0), sptField,
                     FxDB(drutama("bscustomdbl2"), 0), sptField,
                     FxDB(drutama("bscustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("bscustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("bscabangnama"), ""), sptField,
                     FxDB(drutama("bslokasinama"), ""), sptField,
                     FxDB(drutama("bsgudangnama"), ""), sptField,
                     FxDB(drutama("bsbagianperbandingankode"), ""), sptField,
                     FxDB(drutama("bsbagianperbandingannama"), ""), sptField,
                     FxDB(drutama("bsnotransaksirq1"), ""), sptField,
                     FxDB(drutama("bssupplierrq1"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq1"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq1"), ""), sptField,
                     FxDB(drutama("bsterminrq1"), ""), sptField,
                     FxDB(drutama("bsterminnamarq1"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq1"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq2"), ""), sptField,
                     FxDB(drutama("bssupplierrq2"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq2"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq2"), ""), sptField,
                     FxDB(drutama("bsterminrq2"), ""), sptField,
                     FxDB(drutama("bsterminnamarq2"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq2"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq3"), ""), sptField,
                     FxDB(drutama("bssupplierrq3"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq3"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq3"), ""), sptField,
                     FxDB(drutama("bsterminrq3"), ""), sptField,
                     FxDB(drutama("bsterminnamarq3"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq3"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq4"), ""), sptField,
                     FxDB(drutama("bssupplierrq4"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq4"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq4"), ""), sptField,
                     FxDB(drutama("bsterminrq4"), ""), sptField,
                     FxDB(drutama("bsterminnamarq4"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq4"), 0), sptField,
                     FxDB(drutama("bsnotransaksirq5"), ""), sptField,
                     FxDB(drutama("bssupplierrq5"), 0), sptField,
                     FxDB(drutama("bssupplierkoderq5"), ""), sptField,
                     FxDB(drutama("bssuppliernamarq5"), ""), sptField,
                     FxDB(drutama("bsterminrq5"), ""), sptField,
                     FxDB(drutama("bsterminnamarq5"), ""), sptField,
                     FxDB(drutama("bsterminharijatuhtemporq5"), 0), sptField,
                     FxDB(drutama("bsstatusnama"), ""), sptField,
                     FxDB(drutama("bsstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("bsinputusernama"), ""), sptField,
                     FxDB(drutama("bsmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idbs"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("terpilih"), 0), sptField,
                     FxDB(dr("hargake"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idrq"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = " transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bsidhistory, bsid, bscabang, bslokasi, bsgudang, bsasalbarang, bsasalbarangkategori, bsjenispembelian, bsjenispembeliankategori, bscarabayar, bssumber, bsnogrup, bsautonotransaksi, bsnotransaksi, bstgl, bskodepa, bsbagianperbandingan, bsbagianperbandingankontak, bsuraian, bscatatan, bsnoref, bstglnoref, bstglpenutupan, bsmatauang, bsidrq1, bsidrq2, bsidrq3, bsidrq4, bsidrq5, bsidrq1statuspo, bsidrq2statuspo, bsidrq3statuspo, bsidrq4statuspo, bsidrq5statuspo, bsstatus, bsstatussebelumnya, bsjmlrevisi, bscetakanke, bsinputuser, bsinputtgl, bsmodifikasiuser, bsmodifikasitgl, bsisclose, bscustomtext1, bscustomtext2, bscustomtext3, bscustomtext4, bscustomtext5, bscustomint1, bscustomint2, bscustomint3, bscustomdbl1, bscustomdbl2, bscustomdbl3, bscustomdate1, bscustomdate2, bscustomdate3, bscabangnama, bslokasinama, bsgudangnama, bsbagianperbandingankode, bsbagianperbandingannama, bsnotransaksirq1, bssupplierrq1, bssupplierkoderq1, bssuppliernamarq1, bsterminrq1, bsterminnamarq1, bsterminharijatuhtemporq1, bsnotransaksirq2, bssupplierrq2, bssupplierkoderq2, bssuppliernamarq2, bsterminrq2, bsterminnamarq2, bsterminharijatuhtemporq2, bsnotransaksirq3, bssupplierrq3, bssupplierkoderq3, bssuppliernamarq3, bsterminrq3, bsterminnamarq3, bsterminharijatuhtemporq3, bsnotransaksirq4, bssupplierrq4, bssupplierkoderq4, bssuppliernamarq4, bsterminrq4, bsterminnamarq4, bsterminharijatuhtemporq4, bsnotransaksirq5, bssupplierrq5, bssupplierkoderq5, bssuppliernamarq5, bsterminrq5, bsterminnamarq5, bsterminharijatuhtemporq5, bsstatusnama, bsstatussebelumnyanama, bsinputusernama, bsmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idbsdetail, idbs, idrqdetail, terpilih, hargake, catatan, urutan, idrq, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai"))

        Return wsResult
    End Function

End Class
