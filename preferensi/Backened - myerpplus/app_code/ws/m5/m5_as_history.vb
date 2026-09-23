Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_as_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_As_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_as_history(SELECT 0, ash.* FROM m5_as ash WHERE ash.asid = '" & idtransaksi & "')"
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
            sql = "SELECT asidhistory FROM m5_as_history WHERE asid = '" & idtransaksi & "' ORDER BY asmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_as_pay_history (SELECT 0, '" & result(4) & "', ash.* FROM m5_as_pay ash WHERE ash.idas = '" & idtransaksi & "' )"
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
    Public Function M5_As_HistorySearch(ByVal param As String) As String
        'M5_As_HistorySearch --------------------------------------------------------
        'asidhistory, asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, 
        'asisclose, ascabangnama, aslokasinama, asjenisnama, askontakkode, askontaknama, asbagianterimakode, 
        'asbagianterimanama, sonotransaksi, ipnotransaksi, asnoreknama, asstatusnama, asstatussebelumnyanama, asinputusernama, 
        'asmodifikasiusernama

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
            Filter = Filter.Replace("askontakkode", "c1.kkode")
            Filter = Filter.Replace("askontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_as_v_history")

        dt = AmbilData("aplikasi1-M5_As", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("asid"), 0), sptField, FxDB(dr("asidhistory"), 0), sptField,
                     FxDB(dr("ascabang"), ""), sptField,
                     FxDB(dr("aslokasi"), ""), sptField,
                     FxDB(dr("asjenis"), 0), sptField,
                     FxDB(dr("assumber"), ""), sptField,
                     FxDB(dr("asautonotransaksi"), 0), sptField,
                     FxDB(dr("asnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astgl"), ""), formatTgl), sptField,
                     FxDB(dr("askodepa"), 0), sptField,
                     FxDB(dr("askontak"), 0), sptField,
                     FxDB(dr("askontakperson"), ""), sptField,
                     FxDB(dr("as1alamat1"), ""), sptField,
                     FxDB(dr("as1alamat2"), ""), sptField,
                     FxDB(dr("as1alamat3"), ""), sptField,
                     FxDB(dr("as2alamat1"), ""), sptField,
                     FxDB(dr("as2alamat2"), ""), sptField,
                     FxDB(dr("as2alamat3"), ""), sptField,
                     FxDB(dr("asbagianterima"), 0), sptField,
                     FxDB(dr("astermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("asidso"), 0), sptField,
                     FxDB(dr("asidip"), 0), sptField,
                     FxDB(dr("asnorek"), ""), sptField,
                     FxDB(dr("asuraian"), ""), sptField,
                     FxDB(dr("ascatatan"), ""), sptField,
                     FxDB(dr("asnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("astglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("asmatauang"), ""), sptField,
                     FxDB(dr("askurs"), 0), sptField,
                     FxDB(dr("asjumlah"), 0), sptField,
                     FxDB(dr("asjumlahvalas"), 0), sptField,
                     FxDB(dr("asjumlahbayar"), 0), sptField,
                     FxDB(dr("asjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("asstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("astgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("ascostcenter"), ""), sptField,
                     FxDB(dr("asdivisi"), ""), sptField,
                     FxDB(dr("assubdivisi"), ""), sptField,
                     FxDB(dr("asproyek"), ""), sptField,
                     FxDB(dr("asstatus"), 0), sptField,
                     FxDB(dr("asstatussebelumnya"), 0), sptField,
                     FxDB(dr("asjmlrevisi"), 0), sptField,
                     FxDB(dr("ascetakanke"), 0), sptField,
                     FxDB(dr("asinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("asinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("asmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("aspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("asisclose"), 0), sptField,
                     FxDB(dr("ascabangnama"), ""), sptField,
                     FxDB(dr("aslokasinama"), ""), sptField,
                     FxDB(dr("asjenisnama"), ""), sptField,
                     FxDB(dr("askontakkode"), ""), sptField,
                     FxDB(dr("askontaknama"), ""), sptField,
                     FxDB(dr("asbagianterimakode"), ""), sptField,
                     FxDB(dr("asbagianterimanama"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptField,
                     FxDB(dr("asnoreknama"), ""), sptField,
                     FxDB(dr("asstatusnama"), ""), sptField,
                     FxDB(dr("asstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("asinputusernama"), ""), sptField,
                     FxDB(dr("asmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asidhistory, asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, asisclose, ascabangnama, aslokasinama, asjenisnama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, sonotransaksi, ipnotransaksi, asnoreknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_AsHistoryGetdataById(ByVal param As String) As String
        'M5_AsHistoryGetdataById Utama --------------------------------------------------------
        'asidhistory, asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, 
        'astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, 
        'as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, 
        'asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, 
        'askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, 
        'ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, 
        'ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, 
        'asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, 
        'ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, 
        'ascustomdate3, ascabangnama, aslokasinama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, 
        'asterminnama, asterminharijatuhtempo, asnotransaksiso, asnotransaksiip, asnoreknama, ascostcenternama, asdivisinama, 
        'assubdivisinama, asproyeknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama

        'M5_AsHistoryGetdataById Pay --------------------------------------------------------
        'idhistorycarabayar, idhistory, idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, 
        'tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, 
        'idip, isclose, carabayarnama, banknama, rekbanknama, rekgironama, ipnotransaksi

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
            formatTglWaktu = "yyy-MM-dd H:mm:ss"
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

        Dim NmMemcached As String = "aplikasi1-M5_as_history~M5_as_Detail_history-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "asidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "asidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_as_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("asidhistory"), 0), sptField, FxDB(drutama("asid"), 0), sptField,
                     FxDB(drutama("ascabang"), ""), sptField,
                     FxDB(drutama("aslokasi"), ""), sptField,
                     FxDB(drutama("asjenis"), 0), sptField,
                     FxDB(drutama("assumber"), ""), sptField,
                     FxDB(drutama("asautonotransaksi"), 0), sptField,
                     FxDB(drutama("asnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astgl"), ""), formatTgl), sptField,
                     FxDB(drutama("askodepa"), 0), sptField,
                     FxDB(drutama("askontak"), 0), sptField,
                     FxDB(drutama("askontakperson"), ""), sptField,
                     FxDB(drutama("as1alamat1"), ""), sptField,
                     FxDB(drutama("as1alamat2"), ""), sptField,
                     FxDB(drutama("as1alamat3"), ""), sptField,
                     FxDB(drutama("as2alamat1"), ""), sptField,
                     FxDB(drutama("as2alamat2"), ""), sptField,
                     FxDB(drutama("as2alamat3"), ""), sptField,
                     FxDB(drutama("asbagianterima"), 0), sptField,
                     FxDB(drutama("astermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("asidso"), 0), sptField,
                     FxDB(drutama("asidip"), 0), sptField,
                     FxDB(drutama("asnorek"), ""), sptField,
                     FxDB(drutama("asuraian"), ""), sptField,
                     FxDB(drutama("ascatatan"), ""), sptField,
                     FxDB(drutama("asnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("astglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("asmatauang"), ""), sptField,
                     FxDB(drutama("askurs"), 0), sptField,
                     FxDB(drutama("asjumlah"), 0), sptField,
                     FxDB(drutama("asjumlahvalas"), 0), sptField,
                     FxDB(drutama("asjumlahbayar"), 0), sptField,
                     FxDB(drutama("asjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("asstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("astgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("ascostcenter"), ""), sptField,
                     FxDB(drutama("asdivisi"), ""), sptField,
                     FxDB(drutama("assubdivisi"), ""), sptField,
                     FxDB(drutama("asproyek"), ""), sptField,
                     FxDB(drutama("asstatus"), 0), sptField,
                     FxDB(drutama("asstatussebelumnya"), 0), sptField,
                     FxDB(drutama("asjmlrevisi"), 0), sptField,
                     FxDB(drutama("ascetakanke"), 0), sptField,
                     FxDB(drutama("asinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("asinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("asmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aspostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("asisclose"), 0), sptField,
                     FxDB(drutama("ascustomtext1"), ""), sptField,
                     FxDB(drutama("ascustomtext2"), ""), sptField,
                     FxDB(drutama("ascustomtext3"), ""), sptField,
                     FxDB(drutama("ascustomtext4"), ""), sptField,
                     FxDB(drutama("ascustomtext5"), ""), sptField,
                     FxDB(drutama("ascustomint1"), 0), sptField,
                     FxDB(drutama("ascustomint2"), 0), sptField,
                     FxDB(drutama("ascustomint3"), 0), sptField,
                     FxDB(drutama("ascustomdbl1"), 0), sptField,
                     FxDB(drutama("ascustomdbl2"), 0), sptField,
                     FxDB(drutama("ascustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ascustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ascabangnama"), ""), sptField,
                     FxDB(drutama("aslokasinama"), ""), sptField,
                     FxDB(drutama("askontakkode"), ""), sptField,
                     FxDB(drutama("askontaknama"), ""), sptField,
                     FxDB(drutama("asbagianterimakode"), ""), sptField,
                     FxDB(drutama("asbagianterimanama"), ""), sptField,
                     FxDB(drutama("asterminnama"), ""), sptField,
                     FxDB(drutama("asterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("asnotransaksiso"), ""), sptField,
                     FxDB(drutama("asnotransaksiip"), ""), sptField,
                     FxDB(drutama("asnoreknama"), ""), sptField,
                     FxDB(drutama("ascostcenternama"), ""), sptField,
                     FxDB(drutama("asdivisinama"), ""), sptField,
                     FxDB(drutama("assubdivisinama"), ""), sptField,
                     FxDB(drutama("asproyeknama"), ""), sptField,
                     FxDB(drutama("asstatusnama"), ""), sptField,
                     FxDB(drutama("asstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("asinputusernama"), ""), sptField,
                     FxDB(drutama("asmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorycarabayar"), 0), sptField, FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idascarabayar"), 0), sptField,
                     FxDB(dr("idas"), 0), sptField,
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
                     FxDB(dr("idip"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptField,
                     FxDB(dr("ipnotransaksi"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

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
        strResultData = String.Concat(utama, sptSubParam, detail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("asidhistory, asid, ascabang, aslokasi, asjenis, assumber, asautonotransaksi, asnotransaksi, astgl, askodepa, askontak, askontakperson, as1alamat1, as1alamat2, as1alamat3, as2alamat1, as2alamat2, as2alamat3, asbagianterima, astermin, astgljatuhtempo, asidso, asidip, asnorek, asuraian, ascatatan, asnoref, astglnoref, asmatauang, askurs, asjumlah, asjumlahvalas, asjumlahbayar, asjumlahbayarvalas, asstatusbayar, astgllunas, ascostcenter, asdivisi, assubdivisi, asproyek, asstatus, asstatussebelumnya, asjmlrevisi, ascetakanke, asinputuser, asinputtgl, asmodifikasiuser, asmodifikasitgl, asposting, aspostingtgl, asisclose, ascustomtext1, ascustomtext2, ascustomtext3, ascustomtext4, ascustomtext5, ascustomint1, ascustomint2, ascustomint3, ascustomdbl1, ascustomdbl2, ascustomdbl3, ascustomdate1, ascustomdate2, ascustomdate3, ascabangnama, aslokasinama, askontakkode, askontaknama, asbagianterimakode, asbagianterimanama, asterminnama, asterminharijatuhtempo, asnotransaksiso, asnotransaksiip, asnoreknama, ascostcenternama, asdivisinama, assubdivisinama, asproyeknama, asstatusnama, asstatussebelumnyanama, asinputusernama, asmodifikasiusernama"), sptSubParam, ReplaceMapping("idhistorycarabayar, idhistory, idascarabayar, idas, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, idip, isclose, carabayarnama, banknama, rekbanknama, rekgironama, ipnotransaksi"))

        Return wsResult
    End Function
End Class
