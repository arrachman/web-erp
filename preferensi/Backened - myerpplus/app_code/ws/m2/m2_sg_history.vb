Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m2_sg_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Sg_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_sg_history(SELECT 0, sg.* FROM m2_sg sg WHERE sg.sgid = '" & idtransaksi & "')"
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
            sql = "SELECT sgidhistory FROM m2_sg_history WHERE sgid = '" & idtransaksi & "' ORDER BY sgmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_sg_detail_history (SELECT 0, '" & result(4) & "', sg.* FROM m2_sg_detail sg WHERE sg.idsg = '" & idtransaksi & "' )"
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
    Public Function M2_Sg_HistorySearch(ByVal param As String) As String
        'M2_Sg_HistorySearch --------------------------------------------------------
        'sgidhistory, sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, 
        'sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, 
        'sgmodifikasiusernama

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
            Filter = Filter.Replace("sgkontakkode", "c1.kkode")
            Filter = Filter.Replace("sgkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sg_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Sg", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sgid"), 0), sptField,
                     FxDB(dr("sgidhistory"), 0), sptField,
                     FxDB(dr("sgcabang"), ""), sptField,
                     FxDB(dr("sglokasi"), ""), sptField,
                     FxDB(dr("sgsumber"), ""), sptField,
                     FxDB(dr("sgautonotransaksi"), 0), sptField,
                     FxDB(dr("sgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sgtgl"), ""), formatTgl), sptField,
                     FxDB(dr("sgkodepa"), 0), sptField,
                     FxDB(dr("sgkontak"), 0), sptField,
                     FxDB(dr("sgkontakperson"), ""), sptField,
                     FxDB(dr("sguraian"), ""), sptField,
                     FxDB(dr("sgcatatan"), ""), sptField,
                     FxDB(dr("sgmatauang"), ""), sptField,
                     FxDB(dr("sgkurs"), 0), sptField,
                     FxDB(dr("sgjumlah"), 0), sptField,
                     FxDB(dr("sgjumlahvalas"), 0), sptField,
                     FxDB(dr("sgstatussgc"), 0), sptField,
                     FxDB(dr("sgstatus"), 0), sptField,
                     FxDB(dr("sgstatussebelumnya"), 0), sptField,
                     FxDB(dr("sgjmlrevisi"), 0), sptField,
                     FxDB(dr("sgcetakanke"), 0), sptField,
                     FxDB(dr("sgisclose"), 0), sptField,
                     FxDB(dr("sginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("sgcabangnama"), ""), sptField,
                     FxDB(dr("sglokasinama"), ""), sptField,
                     FxDB(dr("sgkontakkode"), ""), sptField,
                     FxDB(dr("sgkontaknama"), ""), sptField,
                     FxDB(dr("sgstatusnama"), ""), sptField,
                     FxDB(dr("sgstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sginputusernama"), ""), sptField,
                     FxDB(dr("sgmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgidhistory, sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, sgmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SgHistoryGetdataById(ByVal param As String) As String

        'M2_SgHistoryGetdataById Utama --------------------------------------------------------
        'sgidhistory, sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, 
        'sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, 
        'sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, 
        'sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, 
        'sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, 
        'sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3, 
        'sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, 
        'sgmodifikasiusernama

        'M2_RgHistoryGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, 
        'jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, 
        'urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, 
        'kontaknama, banknama, rekbanknama, rekgironama, statusgironama

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

        Dim NmMemcached As String = "aplikasi1-M2_Sg_history~M2_Sg_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "sgidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "sgidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sg_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("sgidhistory"), 0), sptField,
                     FxDB(drutama("sgid"), 0), sptField,
                     FxDB(drutama("sgcabang"), ""), sptField,
                     FxDB(drutama("sglokasi"), ""), sptField,
                     FxDB(drutama("sgsumber"), ""), sptField,
                     FxDB(drutama("sgautonotransaksi"), 0), sptField,
                     FxDB(drutama("sgnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sgtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sgkodepa"), 0), sptField,
                     FxDB(drutama("sgkontak"), 0), sptField,
                     FxDB(drutama("sgkontakperson"), ""), sptField,
                     FxDB(drutama("sguraian"), ""), sptField,
                     FxDB(drutama("sgcatatan"), ""), sptField,
                     FxDB(drutama("sgmatauang"), ""), sptField,
                     FxDB(drutama("sgkurs"), 0), sptField,
                     FxDB(drutama("sgjumlah"), 0), sptField,
                     FxDB(drutama("sgjumlahvalas"), 0), sptField,
                     FxDB(drutama("sgstatussgc"), 0), sptField,
                     FxDB(drutama("sgstatus"), 0), sptField,
                     FxDB(drutama("sgstatussebelumnya"), 0), sptField,
                     FxDB(drutama("sgjmlrevisi"), 0), sptField,
                     FxDB(drutama("sgcetakanke"), 0), sptField,
                     FxDB(drutama("sgisclose"), 0), sptField,
                     FxDB(drutama("sginputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sginputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("sgcustomtext1"), ""), sptField,
                     FxDB(drutama("sgcustomtext2"), ""), sptField,
                     FxDB(drutama("sgcustomtext3"), ""), sptField,
                     FxDB(drutama("sgcustomtext4"), ""), sptField,
                     FxDB(drutama("sgcustomtext5"), ""), sptField,
                     FxDB(drutama("sgcustomint1"), 0), sptField,
                     FxDB(drutama("sgcustomint2"), 0), sptField,
                     FxDB(drutama("sgcustomint3"), 0), sptField,
                     FxDB(drutama("sgcustomdbl1"), 0), sptField,
                     FxDB(drutama("sgcustomdbl2"), 0), sptField,
                     FxDB(drutama("sgcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sgcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("sgcabangnama"), ""), sptField,
                     FxDB(drutama("sglokasinama"), ""), sptField,
                     FxDB(drutama("sgkontakkode"), ""), sptField,
                     FxDB(drutama("sgkontaknama"), ""), sptField,
                     FxDB(drutama("sgstatusnama"), ""), sptField,
                     FxDB(drutama("sgstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sginputusernama"), ""), sptField,
                     FxDB(drutama("sgmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsgdetail"), 0), sptField,
                     FxDB(dr("idsg"), 0), sptField,
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
                     FxDB(dr("statussgc"), 0), sptField,
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
                     FxDB(dr("statusgironama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sgidhistory, sgid, sgcabang, sglokasi, sgsumber, sgautonotransaksi, sgnotransaksi, sgtgl, sgkodepa, sgkontak, sgkontakperson, sguraian, sgcatatan, sgmatauang, sgkurs, sgjumlah, sgjumlahvalas, sgstatussgc, sgstatus, sgstatussebelumnya, sgjmlrevisi, sgcetakanke, sgisclose, sginputuser, sginputtgl, sgmodifikasiuser, sgmodifikasitgl, sgposting, sgpostingtgl, sgcustomtext1, sgcustomtext2, sgcustomtext3, sgcustomtext4, sgcustomtext5, sgcustomint1, sgcustomint2, sgcustomint3, sgcustomdbl1, sgcustomdbl2, sgcustomdbl3, sgcustomdate1, sgcustomdate2, sgcustomdate3, sgcabangnama, sglokasinama, sgkontakkode, sgkontaknama, sgstatusnama, sgstatussebelumnyanama, sginputusernama, sgmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsgdetail, idsg, nogiro, kontak, matauang, kurs, jumlah, jumlahvalas, bank, noacbank, rekbank, rekgiro, tgljatuhtempo, catatan, urutan, statusgiro, statussgc, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kontakkode, kontaknama, banknama, rekbanknama, rekgironama, statusgironama"))

        Return wsResult
    End Function
End Class
