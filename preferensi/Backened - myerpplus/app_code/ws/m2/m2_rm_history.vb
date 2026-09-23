Imports Microsoft.VisualBasic

Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_rm_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Rm_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_rm_history(SELECT 0, rm.* FROM m2_rm rm WHERE rm.rmid = '" & idtransaksi & "')"
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
            sql = "SELECT rmidhistory FROM m2_rm_history WHERE rmid = '" & idtransaksi & "' ORDER BY rmmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_rm_detail_history (SELECT 0, '" & result(4) & "', rm.* FROM m2_rm_detail rm WHERE rm.idrm = '" & idtransaksi & "' )"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con2
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF PROSES INSERT HISTORY DETAIL -------------------------------

            'PROSES INSERT HISTORY Pay --------------------------------------
            sql = "INSERT INTO m2_rm_pay_history (SELECT 0, '" & result(4) & "', rm.* FROM m2_rm_pay rm WHERE rm.idrm = '" & idtransaksi & "' )"
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
    Public Function M2_Rm_HistorySearch(ByVal param As String) As String
        'M2_RmSearch --------------------------------------------------------
        'rmidhistory, rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcabangnama, rmlokasinama, 
        'rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, 
        'rmmodifikasiusernama

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
            Filter = Filter.Replace("rmkontakkode", "c1.kkode")
            Filter = Filter.Replace("rmkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rm_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Rm_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rmid"), 0), sptField,
                     FxDB(dr("rmidhistory"), 0), sptField,
                     FxDB(dr("rmcabang"), ""), sptField,
                     FxDB(dr("rmlokasi"), ""), sptField,
                     FxDB(dr("rmsumber"), ""), sptField,
                     FxDB(dr("rmautonotransaksi"), 0), sptField,
                     FxDB(dr("rmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rmkodepa"), 0), sptField,
                     FxDB(dr("rmcarabayar"), 0), sptField,
                     FxDB(dr("rmkontak"), 0), sptField,
                     FxDB(dr("rmkontakperson"), ""), sptField,
                     FxDB(dr("rmnorek"), ""), sptField,
                     FxDB(dr("rmuraian"), ""), sptField,
                     FxDB(dr("rmcatatan"), ""), sptField,
                     FxDB(dr("rmmatauang"), ""), sptField,
                     FxDB(dr("rmkurs"), 0), sptField,
                     FxDB(dr("rmjumlah"), 0), sptField,
                     FxDB(dr("rmjumlahvalas"), 0), sptField,
                     FxDB(dr("rmjumlahbayar"), 0), sptField,
                     FxDB(dr("rmjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("rmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rmstatus"), 0), sptField,
                     FxDB(dr("rmstatussebelumnya"), 0), sptField,
                     FxDB(dr("rmjmlrevisi"), 0), sptField,
                     FxDB(dr("rmcetakanke"), 0), sptField,
                     FxDB(dr("rmisclose"), 0), sptField,
                     FxDB(dr("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rmcabangnama"), ""), sptField,
                     FxDB(dr("rmlokasinama"), ""), sptField,
                     FxDB(dr("rmcarabayarnama"), ""), sptField,
                     FxDB(dr("rmkontakkode"), ""), sptField,
                     FxDB(dr("rmkontaknama"), ""), sptField,
                     FxDB(dr("rmnoreknama"), ""), sptField,
                     FxDB(dr("rmstatusnama"), ""), sptField,
                     FxDB(dr("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rminputusernama"), ""), sptField,
                     FxDB(dr("rmmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmidhistory, rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcabangnama, rmlokasinama, rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_RmHistoryGetdataById(ByVal param As String) As String

        'M2_RmGetdataById Utama --------------------------------------------------------
        'rmidhistory, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, 
        'rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, 
        'rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, 
        'rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, 
        'rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcustomtext1, rmcustomtext2, 
        'rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, 
        'rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3, rmcabangnama, rmlokasinama, 
        'rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, 
        'rmmodifikasiusernama

        'M2_RmGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama

        'M2_RmGetdataById Pay -------------------------------------------------------
        'idrmcarabayarhistory, idhistory, idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, carabayarnama, banknama, rekbanknama, rekgironama

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
        Dim Filter As String = "", Sorting As String = "", notransaksi As String = ""
        Dim dt As New DataTable

        Dim utama As String = "", detail As String = "", giro As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Rm_History~M2_Rm_Detail_History-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rmidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rmidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_rm_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            notransaksi = FxDB(drutama("rmnotransaksi"), "")
            utama = String.Concat(FxDB(drutama("rmidhistory"), 0), sptField,
                     FxDB(drutama("rmid"), 0), sptField,
                     FxDB(drutama("rmcabang"), ""), sptField,
                     FxDB(drutama("rmlokasi"), ""), sptField,
                     FxDB(drutama("rmsumber"), ""), sptField,
                     FxDB(drutama("rmautonotransaksi"), 0), sptField,
                     FxDB(drutama("rmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rmkodepa"), 0), sptField,
                     FxDB(drutama("rmcarabayar"), 0), sptField,
                     FxDB(drutama("rmkontak"), 0), sptField,
                     FxDB(drutama("rmkontakperson"), ""), sptField,
                     FxDB(drutama("rmnorek"), ""), sptField,
                     FxDB(drutama("rmuraian"), ""), sptField,
                     FxDB(drutama("rmcatatan"), ""), sptField,
                     FxDB(drutama("rmmatauang"), ""), sptField,
                     FxDB(drutama("rmkurs"), 0), sptField,
                     FxDB(drutama("rmjumlah"), 0), sptField,
                     FxDB(drutama("rmjumlahvalas"), 0), sptField,
                     FxDB(drutama("rmjumlahbayar"), 0), sptField,
                     FxDB(drutama("rmjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("rmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rmstatus"), 0), sptField,
                     FxDB(drutama("rmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rmjmlrevisi"), 0), sptField,
                     FxDB(drutama("rmcetakanke"), 0), sptField,
                     FxDB(drutama("rmisclose"), 0), sptField,
                     FxDB(drutama("rminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rmcustomtext1"), ""), sptField,
                     FxDB(drutama("rmcustomtext2"), ""), sptField,
                     FxDB(drutama("rmcustomtext3"), ""), sptField,
                     FxDB(drutama("rmcustomtext4"), ""), sptField,
                     FxDB(drutama("rmcustomtext5"), ""), sptField,
                     FxDB(drutama("rmcustomint1"), 0), sptField,
                     FxDB(drutama("rmcustomint2"), 0), sptField,
                     FxDB(drutama("rmcustomint3"), 0), sptField,
                     FxDB(drutama("rmcustomdbl1"), 0), sptField,
                     FxDB(drutama("rmcustomdbl2"), 0), sptField,
                     FxDB(drutama("rmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rmcabangnama"), ""), sptField,
                     FxDB(drutama("rmlokasinama"), ""), sptField,
                     FxDB(drutama("rmcarabayarnama"), ""), sptField,
                     FxDB(drutama("rmkontakkode"), ""), sptField,
                     FxDB(drutama("rmkontaknama"), ""), sptField,
                     FxDB(drutama("rmnoreknama"), ""), sptField,
                     FxDB(drutama("rmstatusnama"), ""), sptField,
                     FxDB(drutama("rmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rminputusernama"), ""), sptField,
                     FxDB(drutama("rmmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idrmdetail"), 0), sptField,
                     FxDB(dr("idrm"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
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
                     FxDB(dr("noreknama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            'PANGGIL QUERY
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m2_rm_pay_history")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M2_Giro_List", "rmp.idrmhistory='" & idtransaksi & "'", "rmp.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                giro = String.Concat(giro,
                     FxDB(dr("idrmcarabayarhistory"), 0), sptField,
                     FxDB(dr("idrmhistory"), 0), sptField,
                     FxDB(dr("idrmcarabayar"), 0), sptField,
                     FxDB(dr("idrm"), 0), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("jumlahvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljt"), ""), formatTgl), sptField,
                     FxDB(dr("bank"), ""), sptField,
                     FxDB(dr("noacbank"), ""), sptField,
                     FxDB(dr("rekbank"), ""), sptField,
                     FxDB(dr("rekgiro"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If giro.Length > 0 Then giro = giro.Substring(0, giro.Length - sptRow.Length) Else giro = giro

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, giro)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rmidhistory, rmid, rmcabang, rmlokasi, rmsumber, rmautonotransaksi, rmnotransaksi, rmtgl, rmkodepa, rmcarabayar, rmkontak, rmkontakperson, rmnorek, rmuraian, rmcatatan, rmmatauang, rmkurs, rmjumlah, rmjumlahvalas, rmjumlahbayar, rmjumlahbayarvalas, rmstatusbayar, rmtgllunas, rmstatus, rmstatussebelumnya, rmjmlrevisi, rmcetakanke, rmisclose, rminputuser, rminputtgl, rmmodifikasiuser, rmmodifikasitgl, rmposting, rmpostingtgl, rmcustomtext1, rmcustomtext2, rmcustomtext3, rmcustomtext4, rmcustomtext5, rmcustomint1, rmcustomint2, rmcustomint3, rmcustomdbl1, rmcustomdbl2, rmcustomdbl3, rmcustomdate1, rmcustomdate2, rmcustomdate3, rmcabangnama, rmlokasinama, rmcarabayarnama, rmkontakkode, rmkontaknama, rmnoreknama, rmstatusnama, rmstatussebelumnyanama, rminputusernama, rmmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idrmdetail, idrm, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "idrmcarabayarhistory, idrmhistory, idrmcarabayar, idrm, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
