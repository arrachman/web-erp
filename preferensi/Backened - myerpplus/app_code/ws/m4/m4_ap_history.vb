Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_ap_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_Ap_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m4_ap_history(SELECT 0, ap.* FROM m4_ap ap WHERE ap.apid = '" & idtransaksi & "')"
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
            sql = "SELECT apidhistory FROM m4_ap_history WHERE apid = '" & idtransaksi & "' ORDER BY apmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m4_ap_pay_history (SELECT 0, '" & result(4) & "', ap.* FROM m4_ap_pay ap WHERE ap.idap = '" & idtransaksi & "' )"
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
    Public Function M4_Ap_HistorySearch(ByVal param As String) As String
        'M4_Ap_HistorySearch --------------------------------------------------------
        'apidhistory,apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, 
        'apcabangnama, aplokasinama, apjenisnama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, 
        'ponotransaksi, apnoreknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama

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
            Filter = Filter.Replace("apkontakkode", "c1.kkode")
            Filter = Filter.Replace("apkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ap_v_history")
        'result(2) = sql : GoTo selesai
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ap_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("apid"), 0), sptField,
                     FxDB(dr("apidhistory"), 0), sptField,
                     FxDB(dr("apcabang"), ""), sptField,
                     FxDB(dr("aplokasi"), ""), sptField,
                     FxDB(dr("apjenis"), 0), sptField,
                     FxDB(dr("apsumber"), ""), sptField,
                     FxDB(dr("apautonotransaksi"), 0), sptField,
                     FxDB(dr("apnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptgl"), ""), formatTgl), sptField,
                     FxDB(dr("apkodepa"), 0), sptField,
                     FxDB(dr("apkontak"), 0), sptField,
                     FxDB(dr("apkontakperson"), ""), sptField,
                     FxDB(dr("ap1alamat1"), ""), sptField,
                     FxDB(dr("ap1alamat2"), ""), sptField,
                     FxDB(dr("ap1alamat3"), ""), sptField,
                     FxDB(dr("ap2alamat1"), ""), sptField,
                     FxDB(dr("ap2alamat2"), ""), sptField,
                     FxDB(dr("ap2alamat3"), ""), sptField,
                     FxDB(dr("apbagianpembayaran"), 0), sptField,
                     FxDB(dr("aptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("apidpo"), 0), sptField,
                     FxDB(dr("apnorek"), ""), sptField,
                     FxDB(dr("apuraian"), ""), sptField,
                     FxDB(dr("apcatatan"), ""), sptField,
                     FxDB(dr("apnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("apmatauang"), ""), sptField,
                     FxDB(dr("apkurs"), 0), sptField,
                     FxDB(dr("apjumlah"), 0), sptField,
                     FxDB(dr("apjumlahvalas"), 0), sptField,
                     FxDB(dr("apjumlahbayar"), 0), sptField,
                     FxDB(dr("apjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("apstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("apcostcenter"), ""), sptField,
                     FxDB(dr("apdivisi"), ""), sptField,
                     FxDB(dr("apsubdivisi"), ""), sptField,
                     FxDB(dr("approyek"), ""), sptField,
                     FxDB(dr("apstatus"), 0), sptField,
                     FxDB(dr("apstatussebelumnya"), 0), sptField,
                     FxDB(dr("apjmlrevisi"), 0), sptField,
                     FxDB(dr("apcetakanke"), 0), sptField,
                     FxDB(dr("apinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("apinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("apmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("appostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("apisclose"), 0), sptField,
                     FxDB(dr("apcabangnama"), ""), sptField,
                     FxDB(dr("aplokasinama"), ""), sptField,
                     FxDB(dr("apjenisnama"), ""), sptField,
                     FxDB(dr("apkontakkode"), ""), sptField,
                     FxDB(dr("apkontaknama"), ""), sptField,
                     FxDB(dr("apbagianpembayarankode"), ""), sptField,
                     FxDB(dr("apbagianpembayarannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("apnoreknama"), ""), sptField,
                     FxDB(dr("apstatusnama"), ""), sptField,
                     FxDB(dr("apstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("apinputusernama"), ""), sptField,
                     FxDB(dr("apmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("apidhistory, apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, apcabangnama, aplokasinama, apjenisnama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, ponotransaksi, apnoreknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_ApHistoryGetdataById(ByVal param As String) As String
        'M4_ApHistoryGetdataById Utama --------------------------------------------------------
        'apidhistoryapid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, 
        'aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, 
        'ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, 
        'apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, 
        'apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, 
        'apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, 
        'apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, 
        'apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, 
        'apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3, 
        'apcabangnama, aplokasinama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, apterminnama, 
        'apterminharijatuhtempo, ponotransaksi, apnoreknama, apcostcenternama, apdivisinama, apsubdivisinama, approyeknama, 
        'apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama

        'M4_ApHistoryGetdataById Pay -------------------------------------------------------
        'idhistorycarabayar, idhistory, idapcarabayar, idap, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama

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

        Dim NmMemcached As String = "aplikasi1-M4_Ap_history~M4_Ap_Pay_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "apidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "apidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ap_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("apidhistory"), 0), sptField, FxDB(drutama("apid"), 0), sptField,
                     FxDB(drutama("apcabang"), ""), sptField,
                     FxDB(drutama("aplokasi"), ""), sptField,
                     FxDB(drutama("apjenis"), 0), sptField,
                     FxDB(drutama("apsumber"), ""), sptField,
                     FxDB(drutama("apautonotransaksi"), 0), sptField,
                     FxDB(drutama("apnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("apkodepa"), 0), sptField,
                     FxDB(drutama("apkontak"), 0), sptField,
                     FxDB(drutama("apkontakperson"), ""), sptField,
                     FxDB(drutama("ap1alamat1"), ""), sptField,
                     FxDB(drutama("ap1alamat2"), ""), sptField,
                     FxDB(drutama("ap1alamat3"), ""), sptField,
                     FxDB(drutama("ap2alamat1"), ""), sptField,
                     FxDB(drutama("ap2alamat2"), ""), sptField,
                     FxDB(drutama("ap2alamat3"), ""), sptField,
                     FxDB(drutama("apbagianpembayaran"), 0), sptField,
                     FxDB(drutama("aptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("apidpo"), 0), sptField,
                     FxDB(drutama("apnorek"), ""), sptField,
                     FxDB(drutama("apuraian"), ""), sptField,
                     FxDB(drutama("apcatatan"), ""), sptField,
                     FxDB(drutama("apnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("apmatauang"), ""), sptField,
                     FxDB(drutama("apkurs"), 0), sptField,
                     FxDB(drutama("apjumlah"), 0), sptField,
                     FxDB(drutama("apjumlahvalas"), 0), sptField,
                     FxDB(drutama("apjumlahbayar"), 0), sptField,
                     FxDB(drutama("apjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("apstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("apcostcenter"), ""), sptField,
                     FxDB(drutama("apdivisi"), ""), sptField,
                     FxDB(drutama("apsubdivisi"), ""), sptField,
                     FxDB(drutama("approyek"), ""), sptField,
                     FxDB(drutama("apstatus"), 0), sptField,
                     FxDB(drutama("apstatussebelumnya"), 0), sptField,
                     FxDB(drutama("apjmlrevisi"), 0), sptField,
                     FxDB(drutama("apcetakanke"), 0), sptField,
                     FxDB(drutama("apinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("appostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("apisclose"), 0), sptField,
                     FxDB(drutama("apcustomtext1"), ""), sptField,
                     FxDB(drutama("apcustomtext2"), ""), sptField,
                     FxDB(drutama("apcustomtext3"), ""), sptField,
                     FxDB(drutama("apcustomtext4"), ""), sptField,
                     FxDB(drutama("apcustomtext5"), ""), sptField,
                     FxDB(drutama("apcustomint1"), 0), sptField,
                     FxDB(drutama("apcustomint2"), 0), sptField,
                     FxDB(drutama("apcustomint3"), 0), sptField,
                     FxDB(drutama("apcustomdbl1"), 0), sptField,
                     FxDB(drutama("apcustomdbl2"), 0), sptField,
                     FxDB(drutama("apcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("apcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("apcabangnama"), ""), sptField,
                     FxDB(drutama("aplokasinama"), ""), sptField,
                     FxDB(drutama("apkontakkode"), ""), sptField,
                     FxDB(drutama("apkontaknama"), ""), sptField,
                     FxDB(drutama("apbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("apbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("apterminnama"), ""), sptField,
                     FxDB(drutama("apterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("ponotransaksi"), ""), sptField,
                     FxDB(drutama("apnoreknama"), ""), sptField,
                     FxDB(drutama("apcostcenternama"), ""), sptField,
                     FxDB(drutama("apdivisinama"), ""), sptField,
                     FxDB(drutama("apsubdivisinama"), ""), sptField,
                     FxDB(drutama("approyeknama"), ""), sptField,
                     FxDB(drutama("apstatusnama"), ""), sptField,
                     FxDB(drutama("apstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("apinputusernama"), ""), sptField,
                     FxDB(drutama("apmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorycarabayar"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idapcarabayar"), 0), sptField,
                     FxDB(dr("idap"), 0), sptField,
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
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("apidhistory, apid, apcabang, aplokasi, apjenis, apsumber, apautonotransaksi, apnotransaksi, aptgl, apkodepa, apkontak, apkontakperson, ap1alamat1, ap1alamat2, ap1alamat3, ap2alamat1, ap2alamat2, ap2alamat3, apbagianpembayaran, aptermin, aptgljatuhtempo, apidpo, apnorek, apuraian, apcatatan, apnoref, aptglnoref, apmatauang, apkurs, apjumlah, apjumlahvalas, apjumlahbayar, apjumlahbayarvalas, apstatusbayar, aptgllunas, apcostcenter, apdivisi, apsubdivisi, approyek, apstatus, apstatussebelumnya, apjmlrevisi, apcetakanke, apinputuser, apinputtgl, apmodifikasiuser, apmodifikasitgl, apposting, appostingtgl, apisclose, apcustomtext1, apcustomtext2, apcustomtext3, apcustomtext4, apcustomtext5, apcustomint1, apcustomint2, apcustomint3, apcustomdbl1, apcustomdbl2, apcustomdbl3, apcustomdate1, apcustomdate2, apcustomdate3, apcabangnama, aplokasinama, apkontakkode, apkontaknama, apbagianpembayarankode, apbagianpembayarannama, apterminnama, apterminharijatuhtempo, ponotransaksi, apnoreknama, apcostcenternama, apdivisinama, apsubdivisinama, approyeknama, apstatusnama, apstatussebelumnyanama, apinputusernama, apmodifikasiusernama"), sptSubParam, ReplaceMapping("idhistorycarabayar, idhistory, idapcarabayar, idap, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
