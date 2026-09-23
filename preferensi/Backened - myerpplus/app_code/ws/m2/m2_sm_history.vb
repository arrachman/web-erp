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
Public Class m2_sm_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M2_Sm_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m2_sm_history(SELECT 0, sm.* FROM m2_sm sm WHERE sm.smid = '" & idtransaksi & "')"
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
            sql = "SELECT smidhistory FROM m2_sm_history WHERE smid = '" & idtransaksi & "' ORDER BY smmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m2_sm_detail_history (SELECT 0, '" & result(4) & "', sm.* FROM m2_sm_detail sm WHERE sm.idsm = '" & idtransaksi & "' )"
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
            sql = "INSERT INTO m2_sm_pay_history (SELECT 0, '" & result(4) & "', sm.* FROM m2_sm_pay sm WHERE sm.idsm = '" & idtransaksi & "' )"
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
    Public Function M2_Sm_HistorySearch(ByVal param As String) As String
        'M2_SmSearch --------------------------------------------------------
        'smidhistory, smid, smcabang, smlokasi, smsumber, smautonotransaksi, smnotransaksi, smtgl, 
        'smkodepa, smcarabayar, smkontak, smkontakperson, smnorek, smuraian, smcatatan, 
        'smmatauang, smkurs, smjumlah, smjumlahvalas, smjumlahbayar, smjumlahbayarvalas, smstatusbayar, 
        'smtgllunas, smstatus, smstatussebelumnya, smjmlrevisi, smcetakanke, smisclose, sminputuser, 
        'sminputtgl, smmodifikasiuser, smmodifikasitgl, smposting, smpostingtgl, smcabangnama, smlokasinama, 
        'smcarabayarnama, smkontakkode, smkontaknama, smnoreknama, smstatusnama, smstatussebelumnyanama, sminputusernama, 
        'smmodifikasiusernama

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
            Filter = Filter.Replace("smkontakkode", "c1.kkode")
            Filter = Filter.Replace("mkontaknama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sm_v_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Sm_History", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("smid"), 0), sptField,
                     FxDB(dr("smidhistory"), 0), sptField,
                     FxDB(dr("smcabang"), ""), sptField,
                     FxDB(dr("smlokasi"), ""), sptField,
                     FxDB(dr("smsumber"), ""), sptField,
                     FxDB(dr("smautonotransaksi"), 0), sptField,
                     FxDB(dr("smnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("smtgl"), ""), formatTgl), sptField,
                     FxDB(dr("smkodepa"), 0), sptField,
                     FxDB(dr("smcarabayar"), 0), sptField,
                     FxDB(dr("smkontak"), 0), sptField,
                     FxDB(dr("smkontakperson"), ""), sptField,
                     FxDB(dr("smnorek"), ""), sptField,
                     FxDB(dr("smuraian"), ""), sptField,
                     FxDB(dr("smcatatan"), ""), sptField,
                     FxDB(dr("smmatauang"), ""), sptField,
                     FxDB(dr("smkurs"), 0), sptField,
                     FxDB(dr("smjumlah"), 0), sptField,
                     FxDB(dr("smjumlahvalas"), 0), sptField,
                     FxDB(dr("smjumlahbayar"), 0), sptField,
                     FxDB(dr("smjumlahbayarvalas"), 0), sptField,
                     FxDB(dr("smstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("smtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("smstatus"), 0), sptField,
                     FxDB(dr("smstatussebelumnya"), 0), sptField,
                     FxDB(dr("smjmlrevisi"), 0), sptField,
                     FxDB(dr("smcetakanke"), 0), sptField,
                     FxDB(dr("smisclose"), 0), sptField,
                     FxDB(dr("sminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("smmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("smmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("smposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("smpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("smcabangnama"), ""), sptField,
                     FxDB(dr("smlokasinama"), ""), sptField,
                     FxDB(dr("smcarabayarnama"), ""), sptField,
                     FxDB(dr("smkontakkode"), ""), sptField,
                     FxDB(dr("smkontaknama"), ""), sptField,
                     FxDB(dr("smnoreknama"), ""), sptField,
                     FxDB(dr("smstatusnama"), ""), sptField,
                     FxDB(dr("smstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("sminputusernama"), ""), sptField,
                     FxDB(dr("smmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("smidhistory, smid, smcabang, smlokasi, smsumber, smautonotransaksi, smnotransaksi, smtgl, smkodepa, smcarabayar, smkontak, smkontakperson, smnorek, smuraian, smcatatan, smmatauang, smkurs, smjumlah, smjumlahvalas, smjumlahbayar, smjumlahbayarvalas, smstatusbayar, smtgllunas, smstatus, smstatussebelumnya, smjmlrevisi, smcetakanke, smisclose, sminputuser, sminputtgl, smmodifikasiuser, smmodifikasitgl, smposting, smpostingtgl, smcabangnama, smlokasinama, smcarabayarnama, smkontakkode, smkontaknama, smnoreknama, smstatusnama, smstatussebelumnyanama, sminputusernama, smmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2_SmHistoryGetdataById(ByVal param As String) As String

        'M2_smGetdataById Utama --------------------------------------------------------
        'smidhistory, smcabang, smlokasi, smsumber, smautonotransaksi, smnotransaksi, smtgl, 
        'smkodepa, smcarabayar, smkontak, smkontakperson, smnorek, smuraian, smcatatan, 
        'smmatauang, smkurs, smjumlah, smjumlahvalas, smjumlahbayar, smjumlahbayarvalas, smstatusbayar, 
        'smtgllunas, smstatus, smstatussebelumnya, smjmlrevisi, smcetakanke, smisclose, sminputuser, 
        'sminputtgl, smmodifikasiuser, smmodifikasitgl, smposting, smpostingtgl, smcustomtext1, smcustomtext2, 
        'smcustomtext3, smcustomtext4, smcustomtext5, smcustomint1, smcustomint2, smcustomint3, smcustomdbl1, 
        'smcustomdbl2, smcustomdbl3, smcustomdate1, smcustomdate2, smcustomdate3, smcabangnama, smlokasinama, 
        'smcarabayarnama, smkontakkode, smkontaknama, smnoreknama, smstatusnama, smstatussebelumnyanama, sminputusernama, 
        'smmodifikasiusernama

        'M2_SmGetdataById Detail -------------------------------------------------------
        'idhistorydetail, idhistory, idsmdetail, idsm, norek, matauang, kurs, jumlah, jumlahvalas, 
        'catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama

        'M2_SmGetdataById Pay -------------------------------------------------------
        'idsmcarabayarhistory, idhistory, idsmcarabayar, idsm, carabayar, matauang, kurs, jumlah, jumlahvalas, 
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

        Dim NmMemcached As String = "aplikasi1-M2_Sm_History~M2_Sm_Detail_History-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "smidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "smidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2_sm_getdata_history")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            notransaksi = FxDB(drutama("smnotransaksi"), "")
            utama = String.Concat(FxDB(drutama("smidhistory"), 0), sptField,
                     FxDB(drutama("smid"), 0), sptField,
                     FxDB(drutama("smcabang"), ""), sptField,
                     FxDB(drutama("smlokasi"), ""), sptField,
                     FxDB(drutama("smsumber"), ""), sptField,
                     FxDB(drutama("smautonotransaksi"), 0), sptField,
                     FxDB(drutama("smnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("smtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("smkodepa"), 0), sptField,
                     FxDB(drutama("smcarabayar"), 0), sptField,
                     FxDB(drutama("smkontak"), 0), sptField,
                     FxDB(drutama("smkontakperson"), ""), sptField,
                     FxDB(drutama("smnorek"), ""), sptField,
                     FxDB(drutama("smuraian"), ""), sptField,
                     FxDB(drutama("smcatatan"), ""), sptField,
                     FxDB(drutama("smmatauang"), ""), sptField,
                     FxDB(drutama("smkurs"), 0), sptField,
                     FxDB(drutama("smjumlah"), 0), sptField,
                     FxDB(drutama("smjumlahvalas"), 0), sptField,
                     FxDB(drutama("smjumlahbayar"), 0), sptField,
                     FxDB(drutama("smjumlahbayarvalas"), 0), sptField,
                     FxDB(drutama("smstatusbayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("smtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("smstatus"), 0), sptField,
                     FxDB(drutama("smstatussebelumnya"), 0), sptField,
                     FxDB(drutama("smjmlrevisi"), 0), sptField,
                     FxDB(drutama("smcetakanke"), 0), sptField,
                     FxDB(drutama("smisclose"), 0), sptField,
                     FxDB(drutama("sminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("smmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("smmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("smposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("smpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("smcustomtext1"), ""), sptField,
                     FxDB(drutama("smcustomtext2"), ""), sptField,
                     FxDB(drutama("smcustomtext3"), ""), sptField,
                     FxDB(drutama("smcustomtext4"), ""), sptField,
                     FxDB(drutama("smcustomtext5"), ""), sptField,
                     FxDB(drutama("smcustomint1"), 0), sptField,
                     FxDB(drutama("smcustomint2"), 0), sptField,
                     FxDB(drutama("smcustomint3"), 0), sptField,
                     FxDB(drutama("smcustomdbl1"), 0), sptField,
                     FxDB(drutama("smcustomdbl2"), 0), sptField,
                     FxDB(drutama("smcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("smcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("smcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("smcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("smcabangnama"), ""), sptField,
                     FxDB(drutama("smlokasinama"), ""), sptField,
                     FxDB(drutama("smcarabayarnama"), ""), sptField,
                     FxDB(drutama("smkontakkode"), ""), sptField,
                     FxDB(drutama("smkontaknama"), ""), sptField,
                     FxDB(drutama("smnoreknama"), ""), sptField,
                     FxDB(drutama("smstatusnama"), ""), sptField,
                     FxDB(drutama("smstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("sminputusernama"), ""), sptField,
                     FxDB(drutama("smmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField,
                     FxDB(dr("idhistory"), 0), sptField,
                     FxDB(dr("idsmdetail"), 0), sptField,
                     FxDB(dr("idsm"), 0), sptField,
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
            sql = querygiro.PanggilQuery("m2_sm_pay_v_history")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M2_Giro_List", "smp.idsmhistory='" & idtransaksi & "'", "smp.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                giro = String.Concat(giro,
                     FxDB(dr("idsmcarabayarhistory"), 0), sptField,
                     FxDB(dr("idsmhistory"), 0), sptField,
                     FxDB(dr("idsmcarabayar"), 0), sptField,
                     FxDB(dr("idsm"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("smidhistory, smid, smcabang, smlokasi, smsumber, smautonotransaksi, smnotransaksi, smtgl, smkodepa, smcarabayar, smkontak, smkontakperson, smnorek, smuraian, smcatatan, smmatauang, smkurs, smjumlah, smjumlahvalas, smjumlahbayar, smjumlahbayarvalas, smstatusbayar, smtgllunas, smstatus, smstatussebelumnya, smjmlrevisi, smcetakanke, smisclose, sminputuser, sminputtgl, smmodifikasiuser, smmodifikasitgl, smposting, smpostingtgl, smcustomtext1, smcustomtext2, smcustomtext3, smcustomtext4, smcustomtext5, smcustomint1, smcustomint2, smcustomint3, smcustomdbl1, smcustomdbl2, smcustomdbl3, smcustomdate1, smcustomdate2, smcustomdate3, smcabangnama, smlokasinama, smcarabayarnama, smkontakkode, smkontaknama, smnoreknama, smstatusnama, smstatussebelumnyanama, sminputusernama, smmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idsmdetail, idsm, norek, matauang, kurs, jumlah, jumlahvalas, catatan, costcenter, divisi, subdivisi, proyek, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, noreknama, costcenternama, divisinama, subdivisinama, proyeknama" & sptSubParam & "idsmcarabayarhistory, idsmhistory, idsmcarabayar, idsm, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

End Class
