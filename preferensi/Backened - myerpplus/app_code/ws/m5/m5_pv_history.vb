Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_pv_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Pv_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_pv_history(SELECT 0, pv.* FROM m5_pv pv WHERE pv.pvid = '" & idtransaksi & "')"
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
            sql = "SELECT pvidhistory FROM m5_pv_history WHERE pvid = '" & idtransaksi & "' ORDER BY pvmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_pv_detail_history (SELECT 0, '" & result(4) & "', pv.* FROM m5_pv_detail pv WHERE pv.idpv = '" & idtransaksi & "' )"
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
    Public Function M5_Pv_HistorySearch(ByVal param As String) As String
        'M5_Pv_HistorySearch --------------------------------------------------------
        'pvidhistory, pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, 
        'pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, 
        'pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, 
        'pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, 
        'pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, 
        'pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, 
        'pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, 
        'pvisclose, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, 
        'pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, icnotransaksi, 
        'pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama

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
        sql = query.PanggilQuery("m5_pv_v_history")

        dt = AmbilData("aplikasi1-M5_pv_V_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("pvid"), 0), sptField,
                     FxDB(dr("pvidhistory"), 0), sptField,
                     FxDB(dr("pvcabang"), ""), sptField,
                     FxDB(dr("pvlokasi"), ""), sptField,
                     FxDB(dr("pvgudang"), ""), sptField,
                     FxDB(dr("pvsumber"), ""), sptField,
                     FxDB(dr("pvautonotransaksi"), 0), sptField,
                     FxDB(dr("pvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pvtgl"), ""), formatTgl), sptField,
                     FxDB(dr("pvkodepa"), 0), sptField,
                     FxDB(dr("pvcustomer"), 0), sptField,
                     FxDB(dr("pvcustomerkontak"), ""), sptField,
                     FxDB(dr("pv1alamat1"), ""), sptField,
                     FxDB(dr("pv1alamat2"), ""), sptField,
                     FxDB(dr("pv1alamat3"), ""), sptField,
                     FxDB(dr("pv2alamat1"), ""), sptField,
                     FxDB(dr("pv2alamat2"), ""), sptField,
                     FxDB(dr("pv2alamat3"), ""), sptField,
                     FxDB(dr("pvbagianpenjualan"), 0), sptField,
                     FxDB(dr("pvbagianterima"), 0), sptField,
                     FxDB(dr("pvuraian"), ""), sptField,
                     FxDB(dr("pvcatatan"), ""), sptField,
                     FxDB(dr("pvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("pvtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvtglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("pvmatauang"), ""), sptField,
                     FxDB(dr("pvkurs"), 0), sptField,
                     FxDB(dr("pvtotalap"), 0), sptField,
                     FxDB(dr("pvtotalapvalas"), 0), sptField,
                     FxDB(dr("pvtotalar"), 0), sptField,
                     FxDB(dr("pvtotalarvalas"), 0), sptField,
                     FxDB(dr("pvbayar"), 0), sptField,
                     FxDB(dr("pvbayarvalas"), 0), sptField,
                     FxDB(dr("pvselisihkurs"), 0), sptField,
                     FxDB(dr("pvrekselisihkurs"), ""), sptField,
                     FxDB(dr("pvdiskontermin"), 0), sptField,
                     FxDB(dr("pvdiskonterminvalas"), 0), sptField,
                     FxDB(dr("pvrekdiskontermin"), ""), sptField,
                     FxDB(dr("pvidic"), 0), sptField,
                     FxDB(dr("pvstatus"), 0), sptField,
                     FxDB(dr("pvstatussebelumnya"), 0), sptField,
                     FxDB(dr("pvjmlrevisi"), 0), sptField,
                     FxDB(dr("pvcetakanke"), 0), sptField,
                     FxDB(dr("pvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("pvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pvisclose"), 0), sptField,
                     FxDB(dr("pvcabangnama"), ""), sptField,
                     FxDB(dr("pvlokasinama"), ""), sptField,
                     FxDB(dr("pvgudangnama"), ""), sptField,
                     FxDB(dr("pvcustomerkode"), ""), sptField,
                     FxDB(dr("pvcustomernama"), ""), sptField,
                     FxDB(dr("pvbagianpenjualankode"), ""), sptField,
                     FxDB(dr("pvbagianpenjualannama"), ""), sptField,
                     FxDB(dr("pvbagianterimakode"), ""), sptField,
                     FxDB(dr("pvbagianterimanama"), ""), sptField,
                     FxDB(dr("pvcarabayarnama"), ""), sptField,
                     FxDB(dr("pvrekselisihkursnama"), ""), sptField,
                     FxDB(dr("pvrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("icnotransaksi"), ""), sptField,
                     FxDB(dr("pvstatusnama"), ""), sptField,
                     FxDB(dr("pvstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pvinputusernama"), ""), sptField,
                     FxDB(dr("pvmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pvidhistory, pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, pvisclose, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, icnotransaksi, pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_PvHistoryGetdataById(ByVal param As String) As String
        'M5_PvHistoryGetdataById Utama --------------------------------------------------------
        'pvidhistory, pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, 
        'pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, 
        'pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, 
        'pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, 
        'pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, 
        'pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, 
        'pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, 
        'pvisclose, pvcustomtext1, pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, 
        'pvcustomint2, pvcustomint3, pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, 
        'pvcustomdate3, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, 
        'pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, pvnotransaksiic, 
        'pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama

        'M5_PvHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idpvdetail, idpv, sumber, idtransaksi, matauang, 
        'kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, 
        'jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, 
        'subdivisi, proyek, idicdetail, urutan, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, 
        'diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl

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

        Dim NmMemcached As String = "aplikasi1-M5_pv_history~M5_pv_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pvidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pvidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_pv_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pvidhistory"), 0), sptField, FxDB(drutama("pvid"), 0), sptField,
                     FxDB(drutama("pvcabang"), ""), sptField,
                     FxDB(drutama("pvlokasi"), ""), sptField,
                     FxDB(drutama("pvgudang"), ""), sptField,
                     FxDB(drutama("pvsumber"), ""), sptField,
                     FxDB(drutama("pvautonotransaksi"), 0), sptField,
                     FxDB(drutama("pvnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pvkodepa"), 0), sptField,
                     FxDB(drutama("pvcustomer"), 0), sptField,
                     FxDB(drutama("pvcustomerkontak"), ""), sptField,
                     FxDB(drutama("pv1alamat1"), ""), sptField,
                     FxDB(drutama("pv1alamat2"), ""), sptField,
                     FxDB(drutama("pv1alamat3"), ""), sptField,
                     FxDB(drutama("pv2alamat1"), ""), sptField,
                     FxDB(drutama("pv2alamat2"), ""), sptField,
                     FxDB(drutama("pv2alamat3"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualan"), 0), sptField,
                     FxDB(drutama("pvbagianterima"), 0), sptField,
                     FxDB(drutama("pvuraian"), ""), sptField,
                     FxDB(drutama("pvcatatan"), ""), sptField,
                     FxDB(drutama("pvnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("pvcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvtglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("pvmatauang"), ""), sptField,
                     FxDB(drutama("pvkurs"), 0), sptField,
                     FxDB(drutama("pvtotalap"), 0), sptField,
                     FxDB(drutama("pvtotalapvalas"), 0), sptField,
                     FxDB(drutama("pvtotalar"), 0), sptField,
                     FxDB(drutama("pvtotalarvalas"), 0), sptField,
                     FxDB(drutama("pvbayar"), 0), sptField,
                     FxDB(drutama("pvbayarvalas"), 0), sptField,
                     FxDB(drutama("pvselisihkurs"), 0), sptField,
                     FxDB(drutama("pvrekselisihkurs"), ""), sptField,
                     FxDB(drutama("pvdiskontermin"), 0), sptField,
                     FxDB(drutama("pvdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("pvrekdiskontermin"), ""), sptField,
                     FxDB(drutama("pvidic"), 0), sptField,
                     FxDB(drutama("pvstatus"), 0), sptField,
                     FxDB(drutama("pvstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pvjmlrevisi"), 0), sptField,
                     FxDB(drutama("pvcetakanke"), 0), sptField,
                     FxDB(drutama("pvinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pvisclose"), 0), sptField,
                     FxDB(drutama("pvcustomtext1"), ""), sptField,
                     FxDB(drutama("pvcustomtext2"), ""), sptField,
                     FxDB(drutama("pvcustomtext3"), ""), sptField,
                     FxDB(drutama("pvcustomtext4"), ""), sptField,
                     FxDB(drutama("pvcustomtext5"), ""), sptField,
                     FxDB(drutama("pvcustomint1"), 0), sptField,
                     FxDB(drutama("pvcustomint2"), 0), sptField,
                     FxDB(drutama("pvcustomint3"), 0), sptField,
                     FxDB(drutama("pvcustomdbl1"), 0), sptField,
                     FxDB(drutama("pvcustomdbl2"), 0), sptField,
                     FxDB(drutama("pvcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("pvcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pvcabangnama"), ""), sptField,
                     FxDB(drutama("pvlokasinama"), ""), sptField,
                     FxDB(drutama("pvgudangnama"), ""), sptField,
                     FxDB(drutama("pvcustomerkode"), ""), sptField,
                     FxDB(drutama("pvcustomernama"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("pvbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("pvbagianterimakode"), ""), sptField,
                     FxDB(drutama("pvbagianterimanama"), ""), sptField,
                     FxDB(drutama("pvcarabayarnama"), ""), sptField,
                     FxDB(drutama("pvrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("pvrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("pvnotransaksiic"), ""), sptField,
                     FxDB(drutama("pvstatusnama"), ""), sptField,
                     FxDB(drutama("pvstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pvinputusernama"), ""), sptField,
                     FxDB(drutama("pvmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro

                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idpvdetail"), 0), sptField,
                     FxDB(dr("idpv"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("jmlbayar"), 0), sptField,
                     FxDB(dr("jmlbayarvalas"), 0), sptField,
                     FxDB(dr("diskontermin"), ""), sptField,
                     FxDB(dr("jmldiskontermin"), 0), sptField,
                     FxDB(dr("jmldiskonterminvalas"), 0), sptField,
                     FxDB(dr("nogiro"), ""), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("idicdetail"), 0), sptField,
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
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     FxDB(dr("rekhutangpiutangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     tglgiro, sptField,
                     FxDB(dr("notransaksiic"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pvidhistory, pvid, pvcabang, pvlokasi, pvgudang, pvsumber, pvautonotransaksi, pvnotransaksi, pvtgl, pvkodepa, pvcustomer, pvcustomerkontak, pv1alamat1, pv1alamat2, pv1alamat3, pv2alamat1, pv2alamat2, pv2alamat3, pvbagianpenjualan, pvbagianterima, pvuraian, pvcatatan, pvnoref, pvtglnoref, pvcarabayar, pvtglbayar, pvmatauang, pvkurs, pvtotalap, pvtotalapvalas, pvtotalar, pvtotalarvalas, pvbayar, pvbayarvalas, pvselisihkurs, pvrekselisihkurs, pvdiskontermin, pvdiskonterminvalas, pvrekdiskontermin, pvidic, pvstatus, pvstatussebelumnya, pvjmlrevisi, pvcetakanke, pvinputuser, pvinputtgl, pvmodifikasiuser, pvmodifikasitgl, pvposting, pvpostingtgl, pvisclose, pvcustomtext1, pvcustomtext2, pvcustomtext3, pvcustomtext4, pvcustomtext5, pvcustomint1, pvcustomint2, pvcustomint3, pvcustomdbl1, pvcustomdbl2, pvcustomdbl3, pvcustomdate1, pvcustomdate2, pvcustomdate3, pvcabangnama, pvlokasinama, pvgudangnama, pvcustomerkode, pvcustomernama, pvbagianpenjualankode, pvbagianpenjualannama, pvbagianterimakode, pvbagianterimanama, pvcarabayarnama, pvrekselisihkursnama, pvrekdiskonterminnama, pvnotransaksiic, pvstatusnama, pvstatussebelumnyanama, pvinputusernama, pvmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idpvdetail, idpv, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, idicdetail, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl"))

        Return wsResult
    End Function

End Class
