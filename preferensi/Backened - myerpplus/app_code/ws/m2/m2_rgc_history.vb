Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_rgc_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Rgc_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_rgc_history(SELECT 0, rgc.* FROM m2_rgc rgc WHERE rgc.rgcid = '" & idtransaksi & "')"
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
            sql = "SELECT rgcidhistory FROM m2_rgc_history WHERE rgcid = '" & idtransaksi & "' ORDER BY rgcmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_rgc_detail_history (SELECT 0, '" & result(4) & "', rgc.* FROM m2_rgc_detail rgc WHERE rgc.idrgc = '" & idtransaksi & "' )"
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
    Public Function M2_Rgc_HistorySearch(ByVal param As String) As String
        'M2_RgcSearch --------------------------------------------------------
        'rgcidhistory, rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgcpostingtgl, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, 
        'rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama

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
        sql = query.PanggilQuery("m2_rgc_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Rgc_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rgcid"), 0), sptField,
                     FxDB(dr("rgcidhistory"), 0), sptField,
                     FxDB(dr("rgccabang"), ""), sptField,
                     FxDB(dr("rgclokasi"), ""), sptField,
                     FxDB(dr("rgcsumber"), ""), sptField,
                     FxDB(dr("rgcjenis"), 0), sptField,
                     FxDB(dr("rgcautonotransaksi"), 0), sptField,
                     FxDB(dr("rgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rgctgl"), ""), formatTgl), sptField,
                     FxDB(dr("rgckodepa"), 0), sptField,
                     FxDB(dr("rgckontak"), 0), sptField,
                     FxDB(dr("rgckontakperson"), ""), sptField,
                     FxDB(dr("rgcuraian"), ""), sptField,
                     FxDB(dr("rgccatatan"), ""), sptField,
                     FxDB(dr("rgcmatauang"), ""), sptField,
                     FxDB(dr("rgckurs"), 0), sptField,
                     FxDB(dr("rgcjumlah"), 0), sptField,
                     FxDB(dr("rgcjumlahvalas"), 0), sptField,
                     FxDB(dr("rgcidrg"), 0), sptField,
                     FxDB(dr("rgcstatus"), 0), sptField,
                     FxDB(dr("rgcstatussebelumnya"), 0), sptField,
                     FxDB(dr("rgcjmlrevisi"), 0), sptField,
                     FxDB(dr("rgccetakanke"), 0), sptField,
                     FxDB(dr("rgcisclose"), 0), sptField,
                     FxDB(dr("rgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rgccabangnama"), ""), sptField,
                     FxDB(dr("rgclokasinama"), ""), sptField,
                     FxDB(dr("rgcjenisnama"), ""), sptField,
                     FxDB(dr("rgckontakkode"), ""), sptField,
                     FxDB(dr("rgckontaknama"), ""), sptField,
                     FxDB(dr("rgcnotransaksirg"), ""), sptField,
                     FxDB(dr("rgcstatusnama"), ""), sptField,
                     FxDB(dr("rgcstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rgcinputusernama"), ""), sptField,
                     FxDB(dr("rgcmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgcidhistory, rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgcpostingtgl, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RgcHistoryGetdataById(ByVal param As String) As String

        'M2_RgcHistoryGetdataById Utama --------------------------------------------------------
        'rgcidhistory, rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, 
        'rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, 
        'rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, 
        'rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, 
        'rgcpostingtgl, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, 
        'rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, 
        'rgccustomdate3, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, 
        'rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama

        'M2_RgcHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idrgcdetail, idrgc, nogiro, kontak, 
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

        Dim NmMemcached As String = "aplikasi1-M2_Rgc_history~M2_Rgc_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rgcidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rgcidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rgc_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rgcidhistory"), 0), sptField,
                     FxDB(drutama("rgcid"), 0), sptField,
                     FxDB(drutama("rgccabang"), ""), sptField,
                     FxDB(drutama("rgclokasi"), ""), sptField,
                     FxDB(drutama("rgcsumber"), ""), sptField,
                     FxDB(drutama("rgcjenis"), 0), sptField,
                     FxDB(drutama("rgcautonotransaksi"), 0), sptField,
                     FxDB(drutama("rgcnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rgctgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rgckodepa"), 0), sptField,
                     FxDB(drutama("rgckontak"), 0), sptField,
                     FxDB(drutama("rgckontakperson"), ""), sptField,
                     FxDB(drutama("rgcuraian"), ""), sptField,
                     FxDB(drutama("rgccatatan"), ""), sptField,
                     FxDB(drutama("rgcmatauang"), ""), sptField,
                     FxDB(drutama("rgckurs"), 0), sptField,
                     FxDB(drutama("rgcjumlah"), 0), sptField,
                     FxDB(drutama("rgcjumlahvalas"), 0), sptField,
                     FxDB(drutama("rgcidrg"), 0), sptField,
                     FxDB(drutama("rgcstatus"), 0), sptField,
                     FxDB(drutama("rgcstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rgcjmlrevisi"), 0), sptField,
                     FxDB(drutama("rgccetakanke"), 0), sptField,
                     FxDB(drutama("rgcisclose"), 0), sptField,
                     FxDB(drutama("rgcinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgcmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgcposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgcpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rgccustomtext1"), ""), sptField,
                     FxDB(drutama("rgccustomtext2"), ""), sptField,
                     FxDB(drutama("rgccustomtext3"), ""), sptField,
                     FxDB(drutama("rgccustomtext4"), ""), sptField,
                     FxDB(drutama("rgccustomtext5"), ""), sptField,
                     FxDB(drutama("rgccustomint1"), 0), sptField,
                     FxDB(drutama("rgccustomint2"), 0), sptField,
                     FxDB(drutama("rgccustomint3"), 0), sptField,
                     FxDB(drutama("rgccustomdbl1"), 0), sptField,
                     FxDB(drutama("rgccustomdbl2"), 0), sptField,
                     FxDB(drutama("rgccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rgccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rgccabangnama"), ""), sptField,
                     FxDB(drutama("rgclokasinama"), ""), sptField,
                     FxDB(drutama("rgcjenisnama"), ""), sptField,
                     FxDB(drutama("rgckontakkode"), ""), sptField,
                     FxDB(drutama("rgckontaknama"), ""), sptField,
                     FxDB(drutama("rgcnotransaksirg"), ""), sptField,
                     FxDB(drutama("rgcstatusnama"), ""), sptField,
                     FxDB(drutama("rgcstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rgcinputusernama"), ""), sptField,
                     FxDB(drutama("rgcmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrgcdetail"), 0), sptField,
                     FxDB(dr("idrgc"), 0), sptField,
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
                     FxDB(dr("idrgdetail"), 0), sptField,
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
                     FxDB(dr("rgnotransaksi"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rgcidhistory, rgcid, rgccabang, rgclokasi, rgcsumber, rgcjenis, rgcautonotransaksi, rgcnotransaksi, rgctgl, rgckodepa, rgckontak, rgckontakperson, rgcuraian, rgccatatan, rgcmatauang, rgckurs, rgcjumlah, rgcjumlahvalas, rgcidrg, rgcstatus, rgcstatussebelumnya, rgcjmlrevisi, rgccetakanke, rgcisclose, rgcinputuser, rgcinputtgl, rgcmodifikasiuser, rgcmodifikasitgl, rgcposting, rgcpostingtgl, rgccustomtext1, rgccustomtext2, rgccustomtext3, rgccustomtext4, rgccustomtext5, rgccustomint1, rgccustomint2, rgccustomint3, rgccustomdbl1, rgccustomdbl2, rgccustomdbl3, rgccustomdate1, rgccustomdate2, rgccustomdate3, rgccabangnama, rgclokasinama, rgcjenisnama, rgckontakkode, rgckontaknama, rgcnotransaksirg, rgcstatusnama, rgcstatussebelumnyanama, rgcinputusernama, rgcmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idrgcdetail, idrgc, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, idrgdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama, rgnotransaksi"))

        Return wsResult
    End Function
End Class
