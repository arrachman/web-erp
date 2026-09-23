Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_sgc_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Sgc_HistorySimpan(ByVal param As String) As String
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
        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try

            'PROSES INSERT HISTORY UTAMA ---------------------------------------
            sql = "INSERT INTO m2_sgc_history(SELECT 0, sgc.* FROM m2_sgc sgc WHERE sgc.sgcid = '" & idtransaksi & "')"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY UTAMA --------------------------------


            'PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT --------------------
            'Dim dt2 As New DataTable
            'sql = "SELECT sgcidhistory FROM m2_sgc_history WHERE sgcid = '" & idtransaksi & "' ORDER BY sgcmodifikasitgl DESC LIMIT 1"
            'dt2 = AsDataTableAmbilDariDB(sql)
            'If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found. X" : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_sgc_detail_history (SELECT 0, sgc.* FROM m2_sgc_detail sgc WHERE sgc.idsgc = '" & idtransaksi & "' )"
            'result(2) = sql : Trans.Rollback() : GoTo selesai
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
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
        'myConn.Close()
        'myConn = Nothing
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
    Public Function M2_Sgc_HistorySearch(ByVal param As String) As String
        'M2_Sgc_HistorySearch --------------------------------------------------------
        'sgcidhistory, sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgcpostingtgl, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, 
        'sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama

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
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sgc_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Sgc_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgcid"), 0), sptField,
                     FxDB(dr("sgcidhistory"), 0), sptField,
                     FxDB(dr("sgccabang"), ""), sptField,
                     FxDB(dr("sgclokasi"), ""), sptField,
                     FxDB(dr("sgcsumber"), ""), sptField,
                     FxDB(dr("sgcjenis"), 0), sptField,
                     FxDB(dr("sgcautonotransaksi"), 0), sptField,
                     FxDB(dr("sgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sgctgl"), ""), formatTgl), sptField,
                     FxDB(dr("sgckodepa"), 0), sptField,
                     FxDB(dr("sgckontak"), 0), sptField,
                     FxDB(dr("sgckontakperson"), ""), sptField,
                     FxDB(dr("sgcuraian"), ""), sptField,
                     FxDB(dr("sgccatatan"), ""), sptField,
                     FxDB(dr("sgcmatauang"), ""), sptField,
                     FxDB(dr("sgckurs"), 0), sptField,
                     FxDB(dr("sgcjumlah"), 0), sptField,
                     FxDB(dr("sgcjumlahvalas"), 0), sptField,
                     FxDB(dr("sgcidsg"), 0), sptField,
                     FxDB(dr("sgcstatus"), 0), sptField,
                     FxDB(dr("sgcstatussebelumnya"), 0), sptField,
                     FxDB(dr("sgcjmlrevisi"), 0), sptField,
                     FxDB(dr("sgccetakanke"), 0), sptField,
                     FxDB(dr("sgcisclose"), 0), sptField,
                     FxDB(dr("sgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgccabangnama"), ""), sptField,
                     FxDB(dr("sgclokasinama"), ""), sptField,
                     FxDB(dr("sgcjenisnama"), ""), sptField,
                     FxDB(dr("sgckontakkode"), ""), sptField,
                     FxDB(dr("sgckontaknama"), ""), sptField,
                     FxDB(dr("sgcnotransaksisg"), ""), sptField,
                     FxDB(dr("sgcstatusnama"), ""), sptField,
                     FxDB(dr("sgcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sgcinputusernama"), ""), sptField,
                     FxDB(dr("sgcmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgcidhistory, sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgcpostingtgl, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SgcHistoryGetdataById(ByVal param As String) As String

        'M2_ScHistoryGetdataById Utama --------------------------------------------------------
        'sgcidhistory, sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, 
        'sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, 
        'sgckurs, sgcjumlah, sgcjumlahvalas, sgcidrg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, 
        'sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, 
        'sgcpostingtgl, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, 
        'sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, 
        'sgccustomdate3, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksirg, 
        'sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama

        'M2_SgcHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idsgcdetail, idsgc, nogiro, kontak, 
        'matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, 
        'rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, 
        'statusgironama, rgnotransaksi

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

        Dim NmMemcached As String = "aplikasi1-M2_Sgc_history~M2_Sgc_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sgcidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sgcidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sgc_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sgcidhistory"), 0), sptField,
                     FxDB(drutama("sgcid"), 0), sptField,
                     FxDB(drutama("sgccabang"), ""), sptField,
                     FxDB(drutama("sgclokasi"), ""), sptField,
                     FxDB(drutama("sgcsumber"), ""), sptField,
                     FxDB(drutama("sgcjenis"), 0), sptField,
                     FxDB(drutama("sgcautonotransaksi"), 0), sptField,
                     FxDB(drutama("sgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sgctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sgckodepa"), 0), sptField,
                     FxDB(drutama("sgckontak"), 0), sptField,
                     FxDB(drutama("sgckontakperson"), ""), sptField,
                     FxDB(drutama("sgcuraian"), ""), sptField,
                     FxDB(drutama("sgccatatan"), ""), sptField,
                     FxDB(drutama("sgcmatauang"), ""), sptField,
                     FxDB(drutama("sgckurs"), 0), sptField,
                     FxDB(drutama("sgcjumlah"), 0), sptField,
                     FxDB(drutama("sgcjumlahvalas"), 0), sptField,
                     FxDB(drutama("sgcidsg"), 0), sptField,
                     FxDB(drutama("sgcstatus"), 0), sptField,
                     FxDB(drutama("sgcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sgcjmlrevisi"), 0), sptField,
                     FxDB(drutama("sgccetakanke"), 0), sptField,
                     FxDB(drutama("sgcisclose"), 0), sptField,
                     FxDB(drutama("sgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgccustomtext1"), ""), sptField,
                     FxDB(drutama("sgccustomtext2"), ""), sptField,
                     FxDB(drutama("sgccustomtext3"), ""), sptField,
                     FxDB(drutama("sgccustomtext4"), ""), sptField,
                     FxDB(drutama("sgccustomtext5"), ""), sptField,
                     FxDB(drutama("sgccustomint1"), 0), sptField,
                     FxDB(drutama("sgccustomint2"), 0), sptField,
                     FxDB(drutama("sgccustomint3"), 0), sptField,
                     FxDB(drutama("sgccustomdbl1"), 0), sptField,
                     FxDB(drutama("sgccustomdbl2"), 0), sptField,
                     FxDB(drutama("sgccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sgccabangnama"), ""), sptField,
                     FxDB(drutama("sgclokasinama"), ""), sptField,
                     FxDB(drutama("sgcjenisnama"), ""), sptField,
                     FxDB(drutama("sgckontakkode"), ""), sptField,
                     FxDB(drutama("sgckontaknama"), ""), sptField,
                     FxDB(drutama("sgcnotransaksisg"), ""), sptField,
                     FxDB(drutama("sgcstatusnama"), ""), sptField,
                     FxDB(drutama("sgcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sgcinputusernama"), ""), sptField,
                     FxDB(drutama("sgcmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsgcdetail"), 0), sptField,
                     FxDB(dr("idsgc"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("statusgiro"), 0), sptField,
                     FxDB(dr("idsgdetail"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("statusgironama"), ""), sptField,
                     FxDB(dr("sgnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgcidhistory, sgcid, sgccabang, sgclokasi, sgcsumber, sgcjenis, sgcautonotransaksi, sgcnotransaksi, sgctgl, sgckodepa, sgckontak, sgckontakperson, sgcuraian, sgccatatan, sgcmatauang, sgckurs, sgcjumlah, sgcjumlahvalas, sgcidsg, sgcstatus, sgcstatussebelumnya, sgcjmlrevisi, sgccetakanke, sgcisclose, sgcinputuser, sgcinputtgl, sgcmodifikasiuser, sgcmodifikasitgl, sgcposting, sgcpostingtgl, sgccustomtext1, sgccustomtext2, sgccustomtext3, sgccustomtext4, sgccustomtext5, sgccustomint1, sgccustomint2, sgccustomint3, sgccustomdbl1, sgccustomdbl2, sgccustomdbl3, sgccustomdate1, sgccustomdate2, sgccustomdate3, sgccabangnama, sgclokasinama, sgcjenisnama, sgckontakkode, sgckontaknama, sgcnotransaksisg, sgcstatusnama, sgcstatussebelumnyanama, sgcinputusernama, sgcmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsgcdetail, idsgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idsgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama, sgnotransaksi"))

        Return wsResult
    End Function
End Class
