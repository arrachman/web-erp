Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_rp_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Rp_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_rp_history(SELECT 0, rp.* FROM m5_rp rp WHERE rp.rpid = '" & idtransaksi & "')"
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
            sql = "SELECT rpidhistory FROM m5_rp_history WHERE rpid = '" & idtransaksi & "' ORDER BY rpmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_rp_pay_history (SELECT 0, '" & result(4) & "', rp.* FROM m5_rp_pay rp WHERE rp.idrp = '" & idtransaksi & "' )"
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
    Public Function M5_Rp_HistorySearch(ByVal param As String) As String
        'M5_Rp_HistorySearch --------------------------------------------------------
        'rpidhistory, rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
        'rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, 
        'rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, 
        'rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, 
        'rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, 
        'rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, 
        'rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rppostingtgl, rpisclose, 
        'rpcabangnama, rplokasinama, rpjenisnama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, 
        'sinotransaksi, rpnoreknama, rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama

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
            Filter = Filter.Replace("rpkontakkode", "c1.kkode")
            Filter = Filter.Replace("rpkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_rp_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Rp_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("rpid"), 0), sptField,
                     FxDB(dr("rpidhistory"), 0), sptField,
                     FxDB(dr("rpcabang"), ""), sptField,
                     FxDB(dr("rplokasi"), ""), sptField,
                     FxDB(dr("rpjenis"), 0), sptField,
                     FxDB(dr("rpsumber"), ""), sptField,
                     FxDB(dr("rpautonotransaksi"), 0), sptField,
                     FxDB(dr("rpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rptgl"), ""), formatTgl), sptField,
                     FxDB(dr("rpkodepa"), 0), sptField,
                     FxDB(dr("rpkontak"), 0), sptField,
                     FxDB(dr("rpkontakperson"), ""), sptField,
                     FxDB(dr("rp1alamat1"), ""), sptField,
                     FxDB(dr("rp1alamat2"), ""), sptField,
                     FxDB(dr("rp1alamat3"), ""), sptField,
                     FxDB(dr("rp2alamat1"), ""), sptField,
                     FxDB(dr("rp2alamat2"), ""), sptField,
                     FxDB(dr("rp2alamat3"), ""), sptField,
                     FxDB(dr("rpbagianterima"), 0), sptField,
                     FxDB(dr("rptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rpidsi"), 0), sptField,
                     FxDB(dr("rpnorek"), ""), sptField,
                     FxDB(dr("rpuraian"), ""), sptField,
                     FxDB(dr("rpcatatan"), ""), sptField,
                     FxDB(dr("rpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rpmatauang"), ""), sptField,
                     FxDB(dr("rpkurs"), 0), sptField,
                     FxDB(dr("rpjumlah"), 0), sptField,
                     FxDB(dr("rpjumlahvalas"), 0), sptField,
                     FxDB(dr("rpjumlahbayar"), 0), sptField,
                     FxDB(dr("rpjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("rpstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rptgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rpcostcenter"), ""), sptField,
                     FxDB(dr("rpdivisi"), ""), sptField,
                     FxDB(dr("rpsubdivisi"), ""), sptField,
                     FxDB(dr("rpproyek"), ""), sptField,
                     FxDB(dr("rpstatus"), 0), sptField,
                     FxDB(dr("rpstatussebelumnya"), 0), sptField,
                     FxDB(dr("rpjmlrevisi"), 0), sptField,
                     FxDB(dr("rpcetakanke"), 0), sptField,
                     FxDB(dr("rpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rpisclose"), 0), sptField,
                     FxDB(dr("rpcabangnama"), ""), sptField,
                     FxDB(dr("rplokasinama"), ""), sptField,
                     FxDB(dr("rpjenisnama"), ""), sptField,
                     FxDB(dr("rpkontakkode"), ""), sptField,
                     FxDB(dr("rpkontaknama"), ""), sptField,
                     FxDB(dr("rpbagianterimakode"), ""), sptField,
                     FxDB(dr("rpbagianterimanama"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rpnoreknama"), ""), sptField,
                     FxDB(dr("rpstatusnama"), ""), sptField,
                     FxDB(dr("rpstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rpinputusernama"), ""), sptField,
                     FxDB(dr("rpmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rpidhistory, rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rrproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rrposting, rrpostingtgl, rpisclose, rpcabangnama, rplokasinama, rpjenisnama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, sinotransaksi, rpnoreknama, rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RpHistoryGetdataById(ByVal param As String) As String
        'M5_RpHistoryGetdataById Utama --------------------------------------------------------
        'rpidhistory, rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
        'rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, 
        'rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, 
        'rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, 
        'rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, 
        'rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, 
        'rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rppostingtgl, rpisclose, 
        'rpcustomtext1, rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, 
        'rpcustomint3, rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3, 
        'rpcabangnama, rplokasinama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, rpterminnama, 
        'rpterminharijatuhtempo, sinotransaksi, rpnoreknama, rpcostcenternama, rpdivisinama, rpsubdivisinama, rpproyeknama, 
        'rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama

        'M5_RpHistoryGetdataById Pay -------------------------------------------------------
        'idhistorycarabayar, idhistory, idrpcarabayar, idrp, carabayar, matauang, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Rp~M5_Rp_Pay_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rpidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rpidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_rp_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rpidhistory"), 0), sptField, FxDB(drutama("rpid"), 0), sptField,
                     FxDB(drutama("rpcabang"), ""), sptField,
                     FxDB(drutama("rplokasi"), ""), sptField,
                     FxDB(drutama("rpjenis"), 0), sptField,
                     FxDB(drutama("rpsumber"), ""), sptField,
                     FxDB(drutama("rpautonotransaksi"), 0), sptField,
                     FxDB(drutama("rpnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rpkodepa"), 0), sptField,
                     FxDB(drutama("rpkontak"), 0), sptField,
                     FxDB(drutama("rpkontakperson"), ""), sptField,
                     FxDB(drutama("rp1alamat1"), ""), sptField,
                     FxDB(drutama("rp1alamat2"), ""), sptField,
                     FxDB(drutama("rp1alamat3"), ""), sptField,
                     FxDB(drutama("rp2alamat1"), ""), sptField,
                     FxDB(drutama("rp2alamat2"), ""), sptField,
                     FxDB(drutama("rp2alamat3"), ""), sptField,
                     FxDB(drutama("rpbagianterima"), 0), sptField,
                     FxDB(drutama("rptermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rptgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rpidsi"), 0), sptField,
                     FxDB(drutama("rpnorek"), ""), sptField,
                     FxDB(drutama("rpuraian"), ""), sptField,
                     FxDB(drutama("rpcatatan"), ""), sptField,
                     FxDB(drutama("rpnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rpmatauang"), ""), sptField,
                     FxDB(drutama("rpkurs"), 0), sptField,
                     FxDB(drutama("rpjumlah"), 0), sptField,
                     FxDB(drutama("rpjumlahvalas"), 0), sptField,
                     FxDB(drutama("rpjumlahbayar"), 0), sptField,
                     FxDB(drutama("rpjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("rpstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rptgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rpcostcenter"), ""), sptField,
                     FxDB(drutama("rpdivisi"), ""), sptField,
                     FxDB(drutama("rpsubdivisi"), ""), sptField,
                     FxDB(drutama("rpproyek"), ""), sptField,
                     FxDB(drutama("rpstatus"), 0), sptField,
                     FxDB(drutama("rpstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rpjmlrevisi"), 0), sptField,
                     FxDB(drutama("rpcetakanke"), 0), sptField,
                     FxDB(drutama("rpinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rpinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rpmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rpmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rpposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rpisclose"), 0), sptField,
                     FxDB(drutama("rpcustomtext1"), ""), sptField,
                     FxDB(drutama("rpcustomtext2"), ""), sptField,
                     FxDB(drutama("rpcustomtext3"), ""), sptField,
                     FxDB(drutama("rpcustomtext4"), ""), sptField,
                     FxDB(drutama("rpcustomtext5"), ""), sptField,
                     FxDB(drutama("rpcustomint1"), 0), sptField,
                     FxDB(drutama("rpcustomint2"), 0), sptField,
                     FxDB(drutama("rpcustomint3"), 0), sptField,
                     FxDB(drutama("rpcustomdbl1"), 0), sptField,
                     FxDB(drutama("rpcustomdbl2"), 0), sptField,
                     FxDB(drutama("rpcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rpcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rpcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rpcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rpcabangnama"), ""), sptField,
                     FxDB(drutama("rplokasinama"), ""), sptField,
                     FxDB(drutama("rpkontakkode"), ""), sptField,
                     FxDB(drutama("rpkontaknama"), ""), sptField,
                     FxDB(drutama("rpbagianterimakode"), ""), sptField,
                     FxDB(drutama("rpbagianterimanama"), ""), sptField,
                     FxDB(drutama("rpterminnama"), ""), sptField,
                     FxDB(drutama("rpterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sinotransaksi"), ""), sptField,
                     FxDB(drutama("rpnoreknama"), ""), sptField,
                     FxDB(drutama("rpcostcenternama"), ""), sptField,
                     FxDB(drutama("rpdivisinama"), ""), sptField,
                     FxDB(drutama("rpsubdivisinama"), ""), sptField,
                     FxDB(drutama("rpproyeknama"), ""), sptField,
                     FxDB(drutama("rpstatusnama"), ""), sptField,
                     FxDB(drutama("rpstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rpinputusernama"), ""), sptField,
                     FxDB(drutama("rpmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorycarabayar"), 0), FxDB(dr("idhistory"), 0), sptField, sptField, FxDB(dr("idrpcarabayar"), 0), sptField,
                     FxDB(dr("idrp"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rpidhistory, rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rppostingtgl, rpisclose, rpcustomtext1, rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3, rpcabangnama, rplokasinama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, rpterminnama, rpterminharijatuhtempo, sinotransaksi, rpnoreknama, rpcostcenternama, rpdivisinama, rpsubdivisinama, rpproyeknama, rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama"), sptSubParam, ReplaceMapping("idhistorycarabayar, idhistory, idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function
End Class
