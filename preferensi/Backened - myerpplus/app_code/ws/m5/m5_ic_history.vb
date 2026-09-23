Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_ic_history
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_Ic_HistorySimpan(ByVal param As String) As String
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
            sql = "INSERT INTO m5_ic_history(SELECT 0, ic.* FROM m5_ic ic WHERE ic.icid = '" & idtransaksi & "')"
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
            sql = "SELECT icidhistory FROM m5_ic_history WHERE icid = '" & idtransaksi & "' ORDER BY icmodifikasitgl DESC LIMIT 1"
            dt2 = AsDataTableAmbilDariDB(sql, 2)
            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "History main transaction data not found." : Trans.Rollback() : GoTo selesai
            'END OF PROSES AMBIL ID HISTORY YANG BARUSAJA DIINSERT -------------


            'PROSES INSERT HISTORY DETAIL --------------------------------------
            sql = "INSERT INTO m5_ic_detail_history (SELECT 0, '" & result(4) & "', ic.* FROM m5_ic_detail ic WHERE ic.idic = '" & idtransaksi & "' )"
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
    Public Function M5_Ic_HistorySearch(ByVal param As String) As String
        'M5_Ic_HistorySearch --------------------------------------------------------
        'icidhistory, icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icposting, icpostingtgl, icisclose, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, 
        'iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, 
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama

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
        sql = query.PanggilQuery("m5_ic_v_history")

        dt = AmbilData("aplikasi1-M5_ic_V_history", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search, FxDB(dr("icid"), 0), sptField,
                     FxDB(dr("icidhistory"), 0), sptField,
                     FxDB(dr("iccabang"), ""), sptField,
                     FxDB(dr("iclokasi"), ""), sptField,
                     FxDB(dr("icgudang"), ""), sptField,
                     FxDB(dr("icsumber"), ""), sptField,
                     FxDB(dr("icautonotransaksi"), 0), sptField,
                     FxDB(dr("icnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ictgl"), ""), formatTgl), sptField,
                     FxDB(dr("ickodepa"), 0), sptField,
                     FxDB(dr("iccustomer"), 0), sptField,
                     FxDB(dr("iccustomerkontak"), ""), sptField,
                     FxDB(dr("ic1alamat1"), ""), sptField,
                     FxDB(dr("ic1alamat2"), ""), sptField,
                     FxDB(dr("ic1alamat3"), ""), sptField,
                     FxDB(dr("ic2alamat1"), ""), sptField,
                     FxDB(dr("ic2alamat2"), ""), sptField,
                     FxDB(dr("ic2alamat3"), ""), sptField,
                     FxDB(dr("icbagianpenjualan"), 0), sptField,
                     FxDB(dr("icbagianpenagihan"), 0), sptField,
                     FxDB(dr("icuraian"), ""), sptField,
                     FxDB(dr("iccatatan"), ""), sptField,
                     FxDB(dr("icnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ictglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("iccarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ictglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("icmatauang"), ""), sptField,
                     FxDB(dr("ickurs"), 0), sptField,
                     FxDB(dr("ictotalap"), 0), sptField,
                     FxDB(dr("ictotalapvalas"), 0), sptField,
                     FxDB(dr("ictotalar"), 0), sptField,
                     FxDB(dr("ictotalarvalas"), 0), sptField,
                     FxDB(dr("icjmltagih"), 0), sptField,
                     FxDB(dr("icjmltagihvalas"), 0), sptField,
                     FxDB(dr("icbayar"), 0), sptField,
                     FxDB(dr("icbayarvalas"), 0), sptField,
                     FxDB(dr("icselisihkurs"), 0), sptField,
                     FxDB(dr("icrekselisihkurs"), ""), sptField,
                     FxDB(dr("icdiskontermin"), 0), sptField,
                     FxDB(dr("icdiskonterminvalas"), 0), sptField,
                     FxDB(dr("icrekdiskontermin"), ""), sptField,
                     FxDB(dr("icstatuspv"), 0), sptField,
                     FxDB(dr("icstatus"), 0), sptField,
                     FxDB(dr("icstatussebelumnya"), 0), sptField,
                     FxDB(dr("icjmlrevisi"), 0), sptField,
                     FxDB(dr("iccetakanke"), 0), sptField,
                     FxDB(dr("icinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("icpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("icisclose"), 0), sptField,
                     FxDB(dr("iccabangnama"), ""), sptField,
                     FxDB(dr("iclokasinama"), ""), sptField,
                     FxDB(dr("icgudangnama"), ""), sptField,
                     FxDB(dr("iccustomerkode"), ""), sptField,
                     FxDB(dr("iccustomernama"), ""), sptField,
                     FxDB(dr("icbagianpenjualankode"), ""), sptField,
                     FxDB(dr("icbagianpenjualannama"), ""), sptField,
                     FxDB(dr("icbagianpenagihankode"), ""), sptField,
                     FxDB(dr("icbagianpenagihannama"), ""), sptField,
                     FxDB(dr("iccarabayarnama"), ""), sptField,
                     FxDB(dr("icrekselisihkursnama"), ""), sptField,
                     FxDB(dr("icrekdiskonterminnama"), ""), sptField,
                     FxDB(dr("icstatusnama"), ""), sptField,
                     FxDB(dr("icstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("icinputusernama"), ""), sptField,
                     FxDB(dr("icmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icidhistory, icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icposting, icpostingtgl, icisclose, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_IcHistoryGetdataById(ByVal param As String) As String
        'M5_IcHistoryGetdataById Utama --------------------------------------------------------
        'icidhistory, icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, 
        'ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, 
        'ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, 
        'icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, 
        'ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, 
        'icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, 
        'icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, 
        'icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, 
        'iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, 
        'iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, 
        'iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, 
        'icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama

        'M5_IcHistoryGetdataById Detail --------------------------------------------------------
        'idhistorydetail, idhistory, idicdetail, idic, sumber, idtransaksi, 
        'matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, 
        'diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, 
        'divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, 
        'tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, 
        'rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, 
        'inputtgl

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResultData As String = ""
        Dim strResult, strResultPaging As String

        Dim icl As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_ic_history~M5_ic_Detail_history-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "icidhistory = " & idtransaksi
        Else ' jika filter diisi
            Filter = "icidhistory = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        icl = query.PanggilQuery("m5_ic_getdata_history")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , icl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("icidhistory"), 0), sptField, FxDB(drutama("icid"), 0), sptField,
                     FxDB(drutama("iccabang"), ""), sptField,
                     FxDB(drutama("iclokasi"), ""), sptField,
                     FxDB(drutama("icgudang"), ""), sptField,
                     FxDB(drutama("icsumber"), ""), sptField,
                     FxDB(drutama("icautonotransaksi"), 0), sptField,
                     FxDB(drutama("icnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictgl"), ""), formatTgl), sptField,
                     FxDB(drutama("ickodepa"), 0), sptField,
                     FxDB(drutama("iccustomer"), 0), sptField,
                     FxDB(drutama("iccustomerkontak"), ""), sptField,
                     FxDB(drutama("ic1alamat1"), ""), sptField,
                     FxDB(drutama("ic1alamat2"), ""), sptField,
                     FxDB(drutama("ic1alamat3"), ""), sptField,
                     FxDB(drutama("ic2alamat1"), ""), sptField,
                     FxDB(drutama("ic2alamat2"), ""), sptField,
                     FxDB(drutama("ic2alamat3"), ""), sptField,
                     FxDB(drutama("icbagianpenjualan"), 0), sptField,
                     FxDB(drutama("icbagianpenagihan"), 0), sptField,
                     FxDB(drutama("icuraian"), ""), sptField,
                     FxDB(drutama("iccatatan"), ""), sptField,
                     FxDB(drutama("icnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("iccarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ictglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("icmatauang"), ""), sptField,
                     FxDB(drutama("ickurs"), 0), sptField,
                     FxDB(drutama("ictotalap"), 0), sptField,
                     FxDB(drutama("ictotalapvalas"), 0), sptField,
                     FxDB(drutama("ictotalar"), 0), sptField,
                     FxDB(drutama("ictotalarvalas"), 0), sptField,
                     FxDB(drutama("icjmltagih"), 0), sptField,
                     FxDB(drutama("icjmltagihvalas"), 0), sptField,
                     FxDB(drutama("icbayar"), 0), sptField,
                     FxDB(drutama("icbayarvalas"), 0), sptField,
                     FxDB(drutama("icselisihkurs"), 0), sptField,
                     FxDB(drutama("icrekselisihkurs"), ""), sptField,
                     FxDB(drutama("icdiskontermin"), 0), sptField,
                     FxDB(drutama("icdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("icrekdiskontermin"), ""), sptField,
                     FxDB(drutama("icstatuspv"), 0), sptField,
                     FxDB(drutama("icstatus"), 0), sptField,
                     FxDB(drutama("icstatussebelumnya"), 0), sptField,
                     FxDB(drutama("icjmlrevisi"), 0), sptField,
                     FxDB(drutama("iccetakanke"), 0), sptField,
                     FxDB(drutama("icinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("icpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("icisclose"), 0), sptField,
                     FxDB(drutama("iccustomtext1"), ""), sptField,
                     FxDB(drutama("iccustomtext2"), ""), sptField,
                     FxDB(drutama("iccustomtext3"), ""), sptField,
                     FxDB(drutama("iccustomtext4"), ""), sptField,
                     FxDB(drutama("iccustomtext5"), ""), sptField,
                     FxDB(drutama("iccustomint1"), 0), sptField,
                     FxDB(drutama("iccustomint2"), 0), sptField,
                     FxDB(drutama("iccustomint3"), 0), sptField,
                     FxDB(drutama("iccustomdbl1"), 0), sptField,
                     FxDB(drutama("iccustomdbl2"), 0), sptField,
                     FxDB(drutama("iccustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("iccustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("iccabangnama"), ""), sptField,
                     FxDB(drutama("iclokasinama"), ""), sptField,
                     FxDB(drutama("icgudangnama"), ""), sptField,
                     FxDB(drutama("iccustomerkode"), ""), sptField,
                     FxDB(drutama("iccustomernama"), ""), sptField,
                     FxDB(drutama("icbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("icbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("icbagianpenagihankode"), ""), sptField,
                     FxDB(drutama("icbagianpenagihannama"), ""), sptField,
                     FxDB(drutama("iccarabayarnama"), ""), sptField,
                     FxDB(drutama("icrekselisihkursnama"), ""), sptField,
                     FxDB(drutama("icrekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("icstatusnama"), ""), sptField,
                     FxDB(drutama("icstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("icinputusernama"), ""), sptField,
                     FxDB(drutama("icmodifikasiusernama"), ""))

            For Each dr As DataRow In dt.Rows
                Dim tglgiro As String = FxDB(dr("tgljtgiro"), "")
                If Len(tglgiro) > 0 Then tglgiro = AsFormatTanggal(FxDB(dr("tgljtgiro"), ""), formatTgl) Else tglgiro = tglgiro

                detail = String.Concat(detail, FxDB(dr("idhistorydetail"), 0), sptField, FxDB(dr("idhistory"), 0), sptField, FxDB(dr("idicdetail"), 0), sptField,
                     FxDB(dr("idic"), 0), sptField,
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
                     FxDB(dr("jmlpv"), 0), sptField,
                     FxDB(dr("jmlpvvalas"), 0), sptField,
                     FxDB(dr("statuspv"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("icidhistory, icid, iccabang, iclokasi, icgudang, icsumber, icautonotransaksi, icnotransaksi, ictgl, ickodepa, iccustomer, iccustomerkontak, ic1alamat1, ic1alamat2, ic1alamat3, ic2alamat1, ic2alamat2, ic2alamat3, icbagianpenjualan, icbagianpenagihan, icuraian, iccatatan, icnoref, ictglnoref, iccarabayar, ictglbayar, icmatauang, ickurs, ictotalap, ictotalapvalas, ictotalar, ictotalarvalas, icjmltagih, icjmltagihvalas, icbayar, icbayarvalas, icselisihkurs, icrekselisihkurs, icdiskontermin, icdiskonterminvalas, icrekdiskontermin, icstatuspv, icstatus, icstatussebelumnya, icjmlrevisi, iccetakanke, icinputuser, icinputtgl, icmodifikasiuser, icmodifikasitgl, icposting, icpostingtgl, icisclose, iccustomtext1, iccustomtext2, iccustomtext3, iccustomtext4, iccustomtext5, iccustomint1, iccustomint2, iccustomint3, iccustomdbl1, iccustomdbl2, iccustomdbl3, iccustomdate1, iccustomdate2, iccustomdate3, iccabangnama, iclokasinama, icgudangnama, iccustomerkode, iccustomernama, icbagianpenjualankode, icbagianpenjualannama, icbagianpenagihankode, icbagianpenagihannama, iccarabayarnama, icrekselisihkursnama, icrekdiskonterminnama, icstatusnama, icstatussebelumnyanama, icinputusernama, icmodifikasiusernama" & sptSubParam & "idhistorydetail, idhistory, idicdetail, idic, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, nogiro, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlpv, jmlpvvalas, statuspv, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, tgljtgiro, notransaksiic, inputtgl"))

        Return wsResult
    End Function

End Class
