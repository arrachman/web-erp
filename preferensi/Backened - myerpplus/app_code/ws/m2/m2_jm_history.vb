Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_jm_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Jm_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_jm_history(SELECT 0, jm.* FROM m2_jm jm WHERE jm.jmid = '" & idtransaksi & "')"
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
            sql = "SELECT jmidhistory FROM m2_jm_history WHERE jmid = '" & idtransaksi & "' ORDER BY jmmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_jm_detail_history (SELECT 0, '" & result(4) & "', jm.* FROM m2_jm_detail jm WHERE jm.idjm = '" & idtransaksi & "' )"
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
    Public Function M2_Jm_HistorySearch(ByVal param As String) As String
        'M2_JmSearch --------------------------------------------------------
        'jmidhistory, jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcabangnama, jmlokasinama, 
        'jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama

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
            Filter = Filter.Replace("jmkontakkode", "c1.kkode")
            Filter = Filter.Replace("jmkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_jm_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Jm_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("jmidhistory"), 0), sptField,
                     FxDB(dr("jmid"), 0), sptField,
                     FxDB(dr("jmcabang"), ""), sptField,
                     FxDB(dr("jmlokasi"), ""), sptField,
                     FxDB(dr("jmsumber"), ""), sptField,
                     FxDB(dr("jmautonotransaksi"), 0), sptField,
                     FxDB(dr("jmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("jmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("jmkodepa"), 0), sptField,
                     FxDB(dr("jmkontakperson"), ""), sptField,
                     FxDB(dr("jmuraian"), ""), sptField,
                     FxDB(dr("jmcatatan"), ""), sptField,
                     FxDB(dr("jmmatauang"), ""), sptField,
                     FxDB(dr("jmkurs"), 0), sptField,
                     FxDB(dr("jmdebit"), 0), sptField,
                     FxDB(dr("jmdebitvalas"), 0), sptField,
                     FxDB(dr("jmkredit"), 0), sptField,
                     FxDB(dr("jmkreditvalas"), 0), sptField,
                     FxDB(dr("jmjumlahbayar"), 0), sptField,
                     FxDB(dr("jmjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("jmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("jmstatus"), 0), sptField,
                     FxDB(dr("jmstatussebelumnya"), 0), sptField,
                     FxDB(dr("jmjmlrevisi"), 0), sptField,
                     FxDB(dr("jmcetakanke"), 0), sptField,
                     FxDB(dr("jmisclose"), 0), sptField,
                     FxDB(dr("jminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("jmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jmcabangnama"), ""), sptField,
                     FxDB(dr("jmlokasinama"), ""), sptField,
                     FxDB(dr("jmstatusnama"), ""), sptField,
                     FxDB(dr("jmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("jminputusernama"), ""), sptField,
                     FxDB(dr("jmmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmidhistory, jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcabangnama, jmlokasinama, jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_JmHistoryGetdataById(ByVal param As String) As String

        'M2_JmHistoryGetdataById Utama --------------------------------------------------------
        'jmidhistory, jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, 
        'jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, 
        'jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, 
        'jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, 
        'jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcustomtext1, jmcustomtext2, 
        'jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, 
        'jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3, jmcabangnama, jmlokasinama, 
        'jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama

        'M2_JmHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idjmdetail, idjm, kontak
        'norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama,
        'kontakkode, kontaknama


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

        Dim NmMemcached As String = "aplikasi1-M2_Jm~M2_Jm_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "jmidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "jmidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_jm_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("jmidhistory"), 0), sptField,
                     FxDB(drutama("jmid"), 0), sptField,
                     FxDB(drutama("jmcabang"), ""), sptField,
                     FxDB(drutama("jmlokasi"), ""), sptField,
                     FxDB(drutama("jmsumber"), ""), sptField,
                     FxDB(drutama("jmautonotransaksi"), 0), sptField,
                     FxDB(drutama("jmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("jmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("jmkodepa"), 0), sptField,
                     FxDB(drutama("jmkontakperson"), ""), sptField,
                     FxDB(drutama("jmuraian"), ""), sptField,
                     FxDB(drutama("jmcatatan"), ""), sptField,
                     FxDB(drutama("jmmatauang"), ""), sptField,
                     FxDB(drutama("jmkurs"), 0), sptField,
                     FxDB(drutama("jmdebit"), 0), sptField,
                     FxDB(drutama("jmdebitvalas"), 0), sptField,
                     FxDB(drutama("jmkredit"), 0), sptField,
                     FxDB(drutama("jmkreditvalas"), 0), sptField,
                     FxDB(drutama("jmjumlahbayar"), 0), sptField,
                     FxDB(drutama("jmjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("jmstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("jmstatus"), 0), sptField,
                     FxDB(drutama("jmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("jmjmlrevisi"), 0), sptField,
                     FxDB(drutama("jmcetakanke"), 0), sptField,
                     FxDB(drutama("jmisclose"), 0), sptField,
                     FxDB(drutama("jminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("jmcustomtext1"), ""), sptField,
                     FxDB(drutama("jmcustomtext2"), ""), sptField,
                     FxDB(drutama("jmcustomtext3"), ""), sptField,
                     FxDB(drutama("jmcustomtext4"), ""), sptField,
                     FxDB(drutama("jmcustomtext5"), ""), sptField,
                     FxDB(drutama("jmcustomint1"), 0), sptField,
                     FxDB(drutama("jmcustomint2"), 0), sptField,
                     FxDB(drutama("jmcustomint3"), 0), sptField,
                     FxDB(drutama("jmcustomdbl1"), 0), sptField,
                     FxDB(drutama("jmcustomdbl2"), 0), sptField,
                     FxDB(drutama("jmcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("jmcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("jmcabangnama"), ""), sptField,
                     FxDB(drutama("jmlokasinama"), ""), sptField,
                     FxDB(drutama("jmstatusnama"), ""), sptField,
                     FxDB(drutama("jmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("jminputusernama"), ""), sptField,
                     FxDB(drutama("jmmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idjmdetail"), 0), sptField,
                     FxDB(dr("idjm"), 0), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("norek"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("debit"), 0), sptField,
                     FxDB(dr("debitvalas"), 0), sptField,
                     FxDB(dr("kredit"), 0), sptField,
                     FxDB(dr("kreditvalas"), 0), sptField,
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
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("jmidhistory, jmid, jmcabang, jmlokasi, jmsumber, jmautonotransaksi, jmnotransaksi, jmtgl, jmkodepa, jmkontakperson, jmuraian, jmcatatan, jmmatauang, jmkurs, jmdebit, jmdebitvalas, jmkredit, jmkreditvalas, jmjumlahbayar, jmjumlahbayarvalas, jmstatusbayar, jmtgllunas, jmstatus, jmstatussebelumnya, jmjmlrevisi, jmcetakanke, jmisclose, jminputuser, jminputtgl, jmmodifikasiuser, jmmodifikasitgl, jmposting, jmpostingtgl, jmcustomtext1, jmcustomtext2, jmcustomtext3, jmcustomtext4, jmcustomtext5, jmcustomint1, jmcustomint2, jmcustomint3, jmcustomdbl1, jmcustomdbl2, jmcustomdbl3, jmcustomdate1, jmcustomdate2, jmcustomdate3, jmcabangnama, jmlokasinama, jmstatusnama, jmstatussebelumnyanama, jminputusernama, jmmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idjmdetail, idjm, kontak, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama, kontakkode, kontaknama"))

        Return wsResult
    End Function


End Class