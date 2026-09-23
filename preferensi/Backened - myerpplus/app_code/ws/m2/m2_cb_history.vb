Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_cb_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Cb_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_cb_history(SELECT 0, cb.* FROM m2_cb cb WHERE cb.cbid = '" & idtransaksi & "')"
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
            sql = "SELECT cbidhistory FROM m2_cb_history WHERE cbid = '" & idtransaksi & "' ORDER BY cbmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_cb_detail_history (SELECT 0, '" & result(4) & "', cb.* FROM m2_cb_detail cb WHERE cb.idcb = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m2_cb_pay_history (SELECT 0, '" & result(4) & "', cb.* FROM m2_cb_pay cb WHERE cb.idcb = '" & idtransaksi & "' )"
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
    Public Function M2_Cb_HistorySearch(ByVal param As String) As String
        'M2_CbSearch --------------------------------------------------------
        'cbidhistory, cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcabangnama, cblokasinama, 
        'cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama

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
            Filter = Filter.Replace("cbkontakkode", "c1.kkode")
            Filter = Filter.Replace("cbkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cb_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cb_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("cbid"), 0), sptField,
                     FxDB(dr("cbidhistory"), 0), sptField,
                     FxDB(dr("cbcabang"), ""), sptField,
                     FxDB(dr("cblokasi"), ""), sptField,
                     FxDB(dr("cbsumber"), ""), sptField,
                     FxDB(dr("cbautonotransaksi"), 0), sptField,
                     FxDB(dr("cbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cbtgl"), ""), formatTgl), sptField,
                     FxDB(dr("cbkodepa"), 0), sptField,
                     FxDB(dr("cbkontak"), 0), sptField,
                     FxDB(dr("cbkontakperson"), ""), sptField,
                     FxDB(dr("cburaian"), ""), sptField,
                     FxDB(dr("cbcatatan"), ""), sptField,
                     FxDB(dr("cbmatauang"), ""), sptField,
                     FxDB(dr("cbkurs"), 0), sptField,
                     FxDB(dr("cbdebit"), 0), sptField,
                     FxDB(dr("cbdebitvalas"), 0), sptField,
                     FxDB(dr("cbkredit"), 0), sptField,
                     FxDB(dr("cbkreditvalas"), 0), sptField,
                     FxDB(dr("cbjumlahbayar"), 0), sptField,
                     FxDB(dr("cbjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("cbstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("cbstatus"), 0), sptField,
                     FxDB(dr("cbstatussebelumnya"), 0), sptField,
                     FxDB(dr("cbjmlrevisi"), 0), sptField,
                     FxDB(dr("cbcetakanke"), 0), sptField,
                     FxDB(dr("cbisclose"), 0), sptField,
                     FxDB(dr("cbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cbpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("cbcabangnama"), ""), sptField,
                     FxDB(dr("cblokasinama"), ""), sptField,
                     FxDB(dr("cbkontakkode"), ""), sptField,
                     FxDB(dr("cbkontaknama"), ""), sptField,
                     FxDB(dr("cbstatusnama"), ""), sptField,
                     FxDB(dr("cbstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("cbinputusernama"), ""), sptField,
                     FxDB(dr("cbmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found. " & sql & " WHERE " & Filter & " ORDER BY " & Sorting
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cbidhistory, cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcabangnama, cblokasinama, cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_CbHistoryGetdataById(ByVal param As String) As String

        'M2_CbHistoryGetdataById Utama --------------------------------------------------------
        'cbidhistory, cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, 
        'cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, 
        'cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, 
        'cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, 
        'cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcustomtext1, cbcustomtext2, 
        'cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, 
        'cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3, cbcabangnama, cblokasinama, 
        'cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama

        'M2_CbHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idcbdetail, idcb, 
        'norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama

        'M2_CbHistoryGetdataById Pay -------------------------------------------------------
        'idcarabayarhistory, idhistory, idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, banknama, rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M2_Cb_history~M2_Cb_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "cbidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "cbidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_cb_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("cbidhistory"), 0), sptField, FxDB(drutama("cbid"), 0), sptField,
                     FxDB(drutama("cbcabang"), ""), sptField,
                     FxDB(drutama("cblokasi"), ""), sptField,
                     FxDB(drutama("cbsumber"), ""), sptField,
                     FxDB(drutama("cbautonotransaksi"), 0), sptField,
                     FxDB(drutama("cbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("cbtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("cbkodepa"), 0), sptField,
                     FxDB(drutama("cbkontak"), 0), sptField,
                     FxDB(drutama("cbkontakperson"), ""), sptField,
                     FxDB(drutama("cburaian"), ""), sptField,
                     FxDB(drutama("cbcatatan"), ""), sptField,
                     FxDB(drutama("cbmatauang"), ""), sptField,
                     FxDB(drutama("cbkurs"), 0), sptField,
                     FxDB(drutama("cbdebit"), 0), sptField,
                     FxDB(drutama("cbdebitvalas"), 0), sptField,
                     FxDB(drutama("cbkredit"), 0), sptField,
                     FxDB(drutama("cbkreditvalas"), 0), sptField,
                     FxDB(drutama("cbjumlahbayar"), 0), sptField,
                     FxDB(drutama("cbjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("cbstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("cbstatus"), 0), sptField,
                     FxDB(drutama("cbstatussebelumnya"), 0), sptField,
                     FxDB(drutama("cbjmlrevisi"), 0), sptField,
                     FxDB(drutama("cbcetakanke"), 0), sptField,
                     FxDB(drutama("cbisclose"), 0), sptField,
                     FxDB(drutama("cbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("cbcustomtext1"), ""), sptField,
                     FxDB(drutama("cbcustomtext2"), ""), sptField,
                     FxDB(drutama("cbcustomtext3"), ""), sptField,
                     FxDB(drutama("cbcustomtext4"), ""), sptField,
                     FxDB(drutama("cbcustomtext5"), ""), sptField,
                     FxDB(drutama("cbcustomint1"), 0), sptField,
                     FxDB(drutama("cbcustomint2"), 0), sptField,
                     FxDB(drutama("cbcustomint3"), 0), sptField,
                     FxDB(drutama("cbcustomdbl1"), 0), sptField,
                     FxDB(drutama("cbcustomdbl2"), 0), sptField,
                     FxDB(drutama("cbcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("cbcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("cbcabangnama"), ""), sptField,
                     FxDB(drutama("cblokasinama"), ""), sptField,
                     FxDB(drutama("cbkontakkode"), ""), sptField,
                     FxDB(drutama("cbkontaknama"), ""), sptField,
                     FxDB(drutama("cbstatusnama"), ""), sptField,
                     FxDB(drutama("cbstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("cbinputusernama"), ""), sptField,
                     FxDB(drutama("cbmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idcbdetail"), 0), sptField,
                     FxDB(dr("idcb"), 0), sptField,
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
                     FxDB(dr("proyeknama"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            'PANGGIL QUERY
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m2_cb_pay_v_history")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M2_Giro_List", "cbp.idhistory='" & idtransaksi & "'", "cbp.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                giro = String.Concat(giro,
                     FxDB(dr("idcarabayarhistory"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idcbcarabayar"), 0), sptField,
                     FxDB(dr("idcb"), 0), sptField,
                     FxDB(dr("jenisgiro"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cbidhistory, cbid, cbcabang, cblokasi, cbsumber, cbautonotransaksi, cbnotransaksi, cbtgl, cbkodepa, cbkontak, cbkontakperson, cburaian, cbcatatan, cbmatauang, cbkurs, cbdebit, cbdebitvalas, cbkredit, cbkreditvalas, cbjumlahbayar, cbjumlahbayarvalas, cbstatusbayar, cbtgllunas, cbstatus, cbstatussebelumnya, cbjmlrevisi, cbcetakanke, cbisclose, cbinputuser, cbinputtgl, cbmodifikasiuser, cbmodifikasitgl, cbposting, cbpostingtgl, cbcustomtext1, cbcustomtext2, cbcustomtext3, cbcustomtext4, cbcustomtext5, cbcustomint1, cbcustomint2, cbcustomint3, cbcustomdbl1, cbcustomdbl2, cbcustomdbl3, cbcustomdate1, cbcustomdate2, cbcustomdate3, cbcabangnama, cblokasinama, cbkontakkode, cbkontaknama, cbstatusnama, cbstatussebelumnyanama, cbinputusernama, cbmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idcbdetail, idcb, norek, matauang, kurs, debit, debitvalas, kredit, kreditvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "idcarabayarhistory, idhistory, idcbcarabayar, idcb, jenisgiro, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function
End Class
