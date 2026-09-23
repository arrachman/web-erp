Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m3_rw
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M3_RwSearch(ByVal param As String) As String
        'M3_RwSearch --------------------------------------------------------
        'rwid, rwcabang, rwlokasi, rwgudangasal, rwgudangtransit, rwgudangtujuan, rwsumber, 
        'rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwbagianterima, rwbagianterimakontak, rwuraian, 
        'rwcatatan, rwnoref, rwtglnoref, rwidmr, rwidts, rwstatus, rwstatussebelumnya, 
        'rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, 
        'rwpostingtgl, rwisclose, rwcabangnama, rwlokasinama, rwgudangasalnama, rwgudangtransitnama, rwgudangtujuannama, 
        'rwbagianterimakode, rwbagianterimanama, mrnotransaksi, tsnotransaksi, rwstatusnama, rwstatussebelumnyanama, rwinputusernama, 
        'rwmodifikasiusernama

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        sql = "SELECT rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwuraian, rwcatatan, rwnoref, rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, st1.nama AS rwstatusnama, st2.nama AS rwstatussebelumnyanama,u1.unama AS rwinputusernama,u2.unama AS rwmodifikasiusernama, i.bnama, rwbruto, rwtara, rwneto FROM m3_rw rw JOIN m1_item i ON i.bid = rw.rwbid left join m0_status st1 on st1.kode = rw.rwstatus left join m0_status st2 on st2.kode = rw.rwstatussebelumnya left join m0_user u1 on u1.userid = rw.rwinputuser left join m0_user u2 on u2.userid = rw.rwmodifikasiuser"

        'BUKA KONEKSI

        dt = AmbilData("aplikasi1-M3_Rw", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rwid"), 0), sptField,
                     FxDB(dr("rwcabang"), ""), sptField,
                     FxDB(dr("rwlokasi"), ""), sptField,
                     FxDB(dr("rwsumber"), ""), sptField,
                     FxDB(dr("rwautonotransaksi"), 0), sptField,
                     FxDB(dr("rwnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rwtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rwkodepa"), 0), sptField,
                     FxDB(dr("rwuraian"), ""), sptField,
                     FxDB(dr("rwcatatan"), ""), sptField,
                     FxDB(dr("rwnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rwtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rwstatus"), 0), sptField,
                     FxDB(dr("rwstatussebelumnya"), 0), sptField,
                     FxDB(dr("rwjmlrevisi"), 0), sptField,
                     FxDB(dr("rwcetakanke"), 0), sptField,
                     FxDB(dr("rwinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rwinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rwmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rwmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rwposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rwpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rwisclose"), 0), sptField,
                     FxDB(dr("rwstatusnama"), ""), sptField,
                     FxDB(dr("rwstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rwinputusernama"), ""), sptField,
                     FxDB(dr("rwmodifikasiusernama"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("rwbruto"), ""), sptField,
                     FxDB(dr("rwtara"), ""), sptField,
                     FxDB(dr("rwneto"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwuraian, rwcatatan, rwnoref, rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwstatusnama, rwstatussebelumnyanama, rwinputusernama, rwmodifikasiusernama, bnama, rwbruto, rwtara, rwneto"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RwSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        'If (dataSplit.Length <> 1) Then
        '    result(2) = "Invalid transaction data parameter." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'rwid(0) As , rwcabang(1) As String, rwlokasi(2) As String, rwsumber(3) As String, rwautonotransaksi(4) As Integer, 
        'rwnotransaksi(5) As String, rwtgl(6) As Date, rwkodepa(7) As , rwnopol(8) As String, rwbid(9) As , 
        'rwkid(10) As , rwtglbruto(11) As DateTime, rwbruto(12) As Double, rwtgltara(13) As DateTime, rwtara(14) As Double, 
        'rwneto(15) As Double, rwharga(16) As Double, rwsopir(17) As String, rwuraian(18) As String, rwcatatan(19) As String, 
        'rwnoref(20) As String, rwtglnoref(21) As Date, rwstatus(22) As Integer, rwstatussebelumnya(23) As Integer, rwjmlrevisi(24) As Integer, 
        'rwcetakanke(25) As Integer, rwinputuser(26) As , rwinputtgl(27) As DateTime, rwmodifikasiuser(28) As , rwmodifikasitgl(29) As DateTime, 
        'rwposting(30) As Integer, rwpostingtgl(31) As DateTime, rwisclose(32) As Integer, rwcustomtext1(33) As String, rwcustomtext2(34) As String, 
        'rwcustomtext3(35) As String, rwcustomtext4(36) As String, rwcustomtext5(37) As String, rwcustomint1(38) As Integer, rwcustomint2(39) As Integer, 
        'rwcustomint3(40) As Integer, rwcustomdbl1(41) As Double, rwcustomdbl2(42) As Double, rwcustomdbl3(43) As Double, rwcustomdate1(44) As Date, 
        'rwcustomdate2(45) As Date, rwcustomdate3(46) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, 
        'rwkodepa, rwnopol, rwbid, rwkid, rwtglbruto, rwbruto, rwtgltara, 
        'rwtara, rwneto, rwharga, rwsopir, rwuraian, rwcatatan, rwnoref, 
        'rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, 
        'rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwcustomtext1, rwcustomtext2, 
        'rwcustomtext3, rwcustomtext4, rwcustomtext5, rwcustomint1, rwcustomint2, rwcustomint3, rwcustomdbl1, 
        'rwcustomdbl2, rwcustomdbl3, rwcustomdate1, rwcustomdate2, rwcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 47) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rwautonotransaksi(4) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "rwautonotransaksi required numeric." : GoTo selesai
        End If
        'rwtgl(6) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "rwtgl required date." : GoTo selesai
        End If
        'rwtglbruto(11) As DateTime
        If (IsDate(dataUtama(11)) = False) Then
            result(2) = "rwtglbruto required date." : GoTo selesai
        End If
        'rwbruto(12) As Double
        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "rwbruto required numeric." : GoTo selesai
        End If
        'rwtgltara(13) As DateTime
        If (IsDate(dataUtama(13)) = False) Then
            result(2) = "rwtgltara required date." : GoTo selesai
        End If
        'rwtara(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rwtara required numeric." : GoTo selesai
        End If
        'rwneto(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rwneto required numeric." : GoTo selesai
        End If
        'rwharga(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "rwharga required numeric." : GoTo selesai
        End If
        'rwtglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "rwtglnoref required date." : GoTo selesai
        End If
        'rwstatus(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rwstatus required numeric." : GoTo selesai
        End If
        'rwstatussebelumnya(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "rwstatussebelumnya required numeric." : GoTo selesai
        End If
        'rwjmlrevisi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "rwjmlrevisi required numeric." : GoTo selesai
        End If
        'rwcetakanke(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "rwcetakanke required numeric." : GoTo selesai
        End If
        'rwinputtgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "rwinputtgl required date." : GoTo selesai
        End If
        'rwmodifikasitgl(29) As DateTime
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "rwmodifikasitgl required date." : GoTo selesai
        End If
        'rwposting(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "rwposting required numeric." : GoTo selesai
        End If
        'rwpostingtgl(31) As DateTime
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "rwpostingtgl required date." : GoTo selesai
        End If
        'rwisclose(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rwisclose required numeric." : GoTo selesai
        End If
        'rwcustomint1(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rwcustomint1 required numeric." : GoTo selesai
        End If
        'rwcustomint2(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rwcustomint2 required numeric." : GoTo selesai
        End If
        'rwcustomint3(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rwcustomint3 required numeric." : GoTo selesai
        End If
        'rwcustomdbl1(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rwcustomdbl1 required numeric." : GoTo selesai
        End If
        'rwcustomdbl2(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rwcustomdbl2 required numeric." : GoTo selesai
        End If
        'rwcustomdbl3(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "rwcustomdbl3 required numeric." : GoTo selesai
        End If
        'rwcustomdate1(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "rwcustomdate1 required date." : GoTo selesai
        End If
        'rwcustomdate2(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rwcustomdate2 required date." : GoTo selesai
        End If
        'rwcustomdate3(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "rwcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rwid(0) As 
        If Len(dataUtama(0)) = 0 Then
            result(2) = "rwid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(0)) > 20 Then
            result(2) = "rwid should not be more than 20 character." : GoTo selesai
        End If

        'rwcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rwcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rwcabang should not be more than 25 character." : GoTo selesai
        End If

        'rwlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rwlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rwlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rwsumber(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rwsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "rwsumber should not be more than 10 character." : GoTo selesai
        End If

        'rwnotransaksi(5) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "rwnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "rwnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rwtgl(6) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rwtgl can't be empty" : GoTo selesai
        End If

        'rwkodepa(7) As 
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rwkodepa can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(7)) > 20 Then
            result(2) = "rwkodepa should not be more than 20 character." : GoTo selesai
        End If

        'rwnopol(8) As String
        'If Len(dataUtama(8)) = 0 Then
        '    result(2) = "rwnopol can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(8)) > 50 Then
            result(2) = "rwnopol should not be more than 50 character." : GoTo selesai
        End If

        'rwbid(9) As 
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rwbid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 20 Then
            result(2) = "rwbid should not be more than 20 character." : GoTo selesai
        End If

        'rwkid(10) As 
        If Len(dataUtama(10)) = 0 Then
            result(2) = "rwkid can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(10)) > 20 Then
            result(2) = "rwkid should not be more than 20 character." : GoTo selesai
        End If

        'rwtglbruto(11) As DateTime
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rwtglbruto can't be empty" : GoTo selesai
        End If

        'rwbruto(12) As Double
        If Len(dataUtama(12)) = 0 Then
            result(2) = "rwbruto can't be empty" : GoTo selesai
        End If

        'rwtgltara(13) As DateTime
        If Len(dataUtama(13)) = 0 Then
            result(2) = "rwtgltara can't be empty" : GoTo selesai
        End If

        'rwtara(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rwtara can't be empty" : GoTo selesai
        End If

        'rwneto(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rwneto can't be empty" : GoTo selesai
        End If

        'rwharga(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "rwharga can't be empty" : GoTo selesai
        End If

        'rwtglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "rwtglnoref can't be empty" : GoTo selesai
        End If

        'rwinputuser(26) As 
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rwinputuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 20 Then
            result(2) = "rwinputuser should not be more than 20 character." : GoTo selesai
        End If

        'rwinputtgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rwinputtgl can't be empty" : GoTo selesai
        End If

        'rwmodifikasiuser(28) As 
        If Len(dataUtama(28)) = 0 Then
            result(2) = "rwmodifikasiuser can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(28)) > 20 Then
            result(2) = "rwmodifikasiuser should not be more than 20 character." : GoTo selesai
        End If

        'rwmodifikasitgl(29) As DateTime
        If Len(dataUtama(29)) = 0 Then
            result(2) = "rwmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rwpostingtgl(31) As DateTime
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rwpostingtgl can't be empty" : GoTo selesai
        End If

        'rwcustomdbl1(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rwcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rwcustomdbl2(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rwcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rwcustomdbl3(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rwcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rwcustomdate1(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "rwcustomdate1 can't be empty" : GoTo selesai
        End If

        'rwcustomdate2(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rwcustomdate2 can't be empty" : GoTo selesai
        End If

        'rwcustomdate3(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "rwcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rwid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwautonotransaksi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwkodepa", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwnopol", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwbid", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwkid", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwtglbruto", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwbruto", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwtgltara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwtara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwneto", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwsopir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwstatus", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwstatussebelumnya", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwjmlrevisi", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwcetakanke", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwinputuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwmodifikasiuser", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwposting", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwisclose", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomint1", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwcustomint2", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwcustomint3", AsEnumTypeData.AsInt16)
        AsDataTableTambahField(dtutama, "rwcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rwcustomdate3", AsEnumTypeData.AsString)
        If Not AsDataTableTambahData(dtutama, "rwid~rwcabang~rwlokasi~rwsumber~rwautonotransaksi~rwnotransaksi~rwtgl~rwkodepa~rwnopol~rwbid~rwkid~rwtglbruto~rwbruto~rwtgltara~rwtara~rwneto~rwharga~rwsopir~rwuraian~rwcatatan~rwnoref~rwtglnoref~rwstatus~rwstatussebelumnya~rwjmlrevisi~rwcetakanke~rwinputuser~rwinputtgl~rwmodifikasiuser~rwmodifikasitgl~rwposting~rwpostingtgl~rwisclose~rwcustomtext1~rwcustomtext2~rwcustomtext3~rwcustomtext4~rwcustomtext5~rwcustomint1~rwcustomint2~rwcustomint3~rwcustomdbl1~rwcustomdbl2~rwcustomdbl3~rwcustomdate1~rwcustomdate2~rwcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46)) Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'SIMPAN KE DATABASE =================================================================
        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim dr1 As DataRow = dtutama.Rows(0)
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 3, vMenuId As Integer = 51
                Select Case drutama("rwstatus")
                    Case 0 : vAkses = 0
                    Case 1 : vAkses = 0
                    Case 2 : vAkses = 8
                    Case 3 : vAkses = 0
                    Case 4 : vAkses = 0
                    Case 5 : vAkses = 0
                    Case 6 : vAkses = 0
                    Case 7 : vAkses = 0
                    Case 8 : vAkses = 4
                    Case 9 : vAkses = 5
                    Case 10 : vAkses = 6
                    Case 11 : vAkses = 7
                    Case 12 : vAkses = 0
                End Select
                msgAkses = HakAkses(vModuleId, vMenuId, vAkses, userid)
                If Len(msgAkses) > 0 Then
                    result(2) = msgAkses : Trans.Rollback() : GoTo selesai
                End If
                'END OF CEK HAK AKSES STATUS =====================


                If isUpdate Then
                    result(4) = dr1("rwid")
                    notransaksi = dr1("rwnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rwid) FROM M3_Rw WHERE rwid=" & result(4))
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rwautonotransaksi") = 1 And notransaksi = "Auto" Then
                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rwcabang"), drutama("rwlokasi"), drutama("rwsumber"), drutama("rwtgl"))
                            Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                            arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                            'cek success generate notransaksi
                            If (arrNotransaksi(0) = 1) Then
                                notransaksi = arrNotransaksi(2)
                                'tambah query update m0_nomor_next
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = arrNotransaksi(3)
                                End With
                                objCmd.ExecuteNonQuery()
                            Else
                                result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF GENERATE NOTRANSAKSI ==================================
                        End If

                        sql = "Update M3_Rw set rwcabang  = '" & FixQuotes(dr1("rwcabang")) & "', rwlokasi  = '" & FixQuotes(dr1("rwlokasi")) & "', rwsumber  = '" & FixQuotes(dr1("rwsumber")) & "', rwautonotransaksi  = " & dr1("rwautonotransaksi") & ", rwnotransaksi  = '" & FixQuotes(dr1("rwnotransaksi")) & "', rwtgl  = '" & FixQuotes(AsFormatTanggal(dr1("rwtgl"))) & "', rwkodepa  = '" & FixQuotes(dr1("rwkodepa")) & "', rwnopol  = '" & FixQuotes(dr1("rwnopol")) & "', rwbid  = '" & FixQuotes(dr1("rwbid")) & "', rwkid  = '" & FixQuotes(dr1("rwkid")) & "', rwtglbruto  = '" & FixQuotes(AsFormatTanggal(dr1("rwtglbruto"), "yyyy-MM-dd HH:mm:ss")) & "', rwbruto  = '" & FixDouble(dr1("rwbruto")) & "', rwtgltara  = '" & FixQuotes(AsFormatTanggal(dr1("rwtgltara"), "yyyy-MM-dd HH:mm:ss")) & "', rwtara  = '" & FixDouble(dr1("rwtara")) & "', rwneto  = '" & FixDouble(dr1("rwneto")) & "', rwharga  = '" & FixDouble(dr1("rwharga")) & "', rwsopir  = '" & FixQuotes(dr1("rwsopir")) & "', rwuraian  = '" & FixQuotes(dr1("rwuraian")) & "', rwcatatan  = '" & FixQuotes(dr1("rwcatatan")) & "', rwnoref  = '" & FixQuotes(dr1("rwnoref")) & "', rwtglnoref  = '" & FixQuotes(AsFormatTanggal(dr1("rwtglnoref"))) & "', rwstatus  = " & dr1("rwstatus") & ", rwstatussebelumnya  = " & dr1("rwstatussebelumnya") & ", rwjmlrevisi  = " & dr1("rwjmlrevisi") & ", rwcetakanke  = " & dr1("rwcetakanke") & ", rwmodifikasiuser  = '" & FixQuotes(dr1("rwmodifikasiuser")) & "', rwmodifikasitgl  = NOW(), rwposting  = " & dr1("rwposting") & ", rwpostingtgl  = '" & FixQuotes(AsFormatTanggal(dr1("rwpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', rwcustomtext1  = '" & FixQuotes(dr1("rwcustomtext1")) & "', rwcustomtext2  = '" & FixQuotes(dr1("rwcustomtext2")) & "', rwcustomtext3  = '" & FixQuotes(dr1("rwcustomtext3")) & "', rwcustomtext4  = '" & FixQuotes(dr1("rwcustomtext4")) & "', rwcustomtext5  = '" & FixQuotes(dr1("rwcustomtext5")) & "', rwcustomint1  = " & dr1("rwcustomint1") & ", rwcustomint2  = " & dr1("rwcustomint2") & ", rwcustomint3  = " & dr1("rwcustomint3") & ", rwcustomdbl1  = '" & FixDouble(dr1("rwcustomdbl1")) & "', rwcustomdbl2  = '" & FixDouble(dr1("rwcustomdbl2")) & "', rwcustomdbl3  = '" & FixDouble(dr1("rwcustomdbl3")) & "', rwcustomdate1  = '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate1"))) & "', rwcustomdate2  = '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate2"))) & "', rwcustomdate3  = '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate3"))) & "' where rwid = " & dr1("rwid") & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : GoTo selesai
                    End If
                Else
                    If drutama("rwautonotransaksi") = 1 Then
                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rwcabang"), drutama("rwlokasi"), drutama("rwsumber"), drutama("rwtgl"))
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksi(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================
                    Else
                        notransaksi = drutama("rwnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rwid) FROM m3_rw WHERE rwnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M3_Rw (rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwnopol, rwbid, rwkid, rwtglbruto, rwbruto, rwtgltara, rwtara, rwneto, rwharga, rwsopir, rwuraian, rwcatatan, rwnoref, rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwcustomtext1, rwcustomtext2, rwcustomtext3, rwcustomtext4, rwcustomtext5, rwcustomint1, rwcustomint2, rwcustomint3, rwcustomdbl1, rwcustomdbl2, rwcustomdbl3, rwcustomdate1, rwcustomdate2, rwcustomdate3) values('" & FixQuotes(dr1("rwcabang")) & "', '" & FixQuotes(dr1("rwlokasi")) & "', '" & FixQuotes(dr1("rwsumber")) & "', " & dr1("rwautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(dr1("rwtgl"))) & "', '" & FixQuotes(dr1("rwkodepa")) & "', '" & FixQuotes(dr1("rwnopol")) & "', '" & FixQuotes(dr1("rwbid")) & "', '" & FixQuotes(dr1("rwkid")) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwtglbruto"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixDouble(dr1("rwbruto")) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwtgltara"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixDouble(dr1("rwtara")) & "', '" & FixDouble(dr1("rwbruto")) - FixDouble(dr1("rwtara")) & "', '" & FixDouble(dr1("rwharga")) & "', '" & FixQuotes(dr1("rwsopir")) & "', '" & FixQuotes(dr1("rwuraian")) & "', '" & FixQuotes(dr1("rwcatatan")) & "', '" & FixQuotes(dr1("rwnoref")) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwtglnoref"))) & "', " & dr1("rwstatus") & ", " & dr1("rwstatussebelumnya") & ", " & dr1("rwjmlrevisi") & ", " & dr1("rwcetakanke") & ", '" & FixQuotes(dr1("rwinputuser")) & "', NOW(), '" & FixQuotes(dr1("rwmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & dr1("rwposting") & ", '" & FixQuotes(AsFormatTanggal(dr1("rwpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & dr1("rwisclose") & ", '" & FixQuotes(dr1("rwcustomtext1")) & "', '" & FixQuotes(dr1("rwcustomtext2")) & "', '" & FixQuotes(dr1("rwcustomtext3")) & "', '" & FixQuotes(dr1("rwcustomtext4")) & "', '" & FixQuotes(dr1("rwcustomtext5")) & "', " & dr1("rwcustomint1") & ", " & dr1("rwcustomint2") & ", " & dr1("rwcustomint3") & ", '" & FixDouble(dr1("rwcustomdbl1")) & "', '" & FixDouble(dr1("rwcustomdbl2")) & "', '" & FixDouble(dr1("rwcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("rwcustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

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
        myConn.Close()
        myConn = Nothing
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
    Public Function M3_RwUpdateStatus(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim nilaiSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", nilaiStatus As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"
        Dim idtransaksi As String = "", idtransaksih As String = ""
        Dim dtdetail As DataTable
        Dim isDelete As Boolean = False

        Dim Filter As String = "", Sorting As String = "", search As String = ""

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISDELETE =========================================================
        'CEK ISDELETE
        If (IsNumeric(paramSplit(4)) = False) Then
            result(2) = "isdelete required numeric." : GoTo selesai
        Else
            'SET ISDELETE
            If (Val(paramSplit(4)) = 1) Then
                isDelete = True
            Else
                isDelete = False
            End If
        End If
        'END OF VALIDASI DAN SET ISDELETE ==================================================

        'VALIDASI DAN SET NILAISTATUS ======================================================
        'SPILIT PARAMETER NILAISTATUS
        nilaiSplit = paramSplit(5).Split(sptSubParam)

        'CEK ARRAY NILAISTATUS
        If (nilaiSplit.Length <> 2) Then
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If

        'CEK IDTRANSAKSI
        If (IsNumeric(nilaiSplit(0)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = nilaiSplit(0)

        'SET NILAI STATUS
        If (Len(nilaiSplit(1)) > 0) Then
            'JIKA NUMERIC MAKA NILAISTATUS = PARAM NILAI STATUS YG DIINPUT
            'JIKA TIDAK MAKA NILAISTATUS = UNCLOSE
            If (IsNumeric(nilaiSplit(1)) = True) Then
                nilaiStatus = nilaiSplit(1)
                'JIKA NILAI STATUS < 0 ATAU NILAI STATUS > 12 MAKA NILAISTATUS TIDAK VALID
                If (nilaiStatus < 0 Or nilaiStatus > 12) Then
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            Else
                If (nilaiSplit(1).ToString.ToLower = "unclose") Then
                    nilaiStatus = "unclose"
                Else
                    result(2) = "Invalid transaction status value." : GoTo selesai
                End If
            End If
        Else
            result(2) = "Invalid transaction status value." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET NILAISTATUS ================================================

        'UPDATE KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "RW", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT rwtgl, rwnotransaksi, rwstatus FROM m3_rw WHERE rwid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rwstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True


            'CEK PERIODE AKUNTANSI ==============================================================
            Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            'END OF CEK PERIODE AKUNTANSI =======================================================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                'Dim query As New m0_query
                'sql = query.PanggilQuery("m4_ap_terkait")
                'sql = sql.Replace("validtransaksi", idtransaksi)
                'Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                'dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                'If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'AP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
            End If

            'update status utama
            sql = "UPDATE m3_Rw SET Rwstatus = " & nilaiStatus & ", Rwmodifikasiuser='" & userid & "', Rwmodifikasitgl = NOW(), Rwposting = 0, Rwpostingtgl = '1971-01-01 00:00:00', Rwjmlrevisi = Rwjmlrevisi + 1 WHERE Rwid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()
            'END OF INSERT USER LOG =============================================================


            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi


            'AMBIL DATA =============================================================
            Dim paramSearch As String = M3_RwSearch(PostWsSearch(paramSplit(0), "M3_RwSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            resultPaging(0) = hasilSearch.isPaging
            resultPaging(1) = hasilSearch.isNext
            resultPaging(2) = hasilSearch.isPrevious
            resultPaging(3) = hasilSearch.countPage
            resultPaging(4) = hasilSearch.countRow

            search = hasilSearch.data
            'END OF AMBIL DATA ======================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***' 

            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi
        End Try

        objCmd = Nothing
        'myconn.Close()
        'myconn = Nothing
        'UPDATE OF SIMPAN KE DATABASE ==========================================================

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3_RwDelete(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String
        Dim Filter As String = "", Sorting As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

        Dim sql As String = "", idtransaksi As String = "", search As String

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'DELETE DI DATABASE ================================================================
        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'DELETE UTAMA
            sql = "DELETE FROM M3_Rw WHERE rwid = " & idtransaksi
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M3_RwSearch(PostWsSearch(paramSplit(0), "M3_RwSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            Return paramSearch
            Dim hasilSearch As New RsHasilWsSearch
            hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            resultPaging(0) = hasilSearch.isPaging
            resultPaging(1) = hasilSearch.isNext
            resultPaging(2) = hasilSearch.isPrevious
            resultPaging(3) = hasilSearch.countPage
            resultPaging(4) = hasilSearch.countRow

            search = hasilSearch.data
            'END OF AMBIL DATA ======================================================

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = idtransaksi

        End Try

        objCmd = Nothing
        myConn.Close()
        myConn = Nothing
        'END OF DELETE DI DATABASE ==========================================================

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
    Public Function M3_RwGetdataById(ByVal param As String) As String

        'M3_RwGetdataById Utama --------------------------------------------------------
        'rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, 
        'rwkodepa, rwnopol, rwbid, rwkid, rwtglbruto, rwbruto, rwtgltara, 
        'rwtara, rwneto, rwharga, rwsopir, rwuraian, rwcatatan, rwnoref, 
        'rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, 
        'rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwcustomtext1, rwcustomtext2, 
        'rwcustomtext3, rwcustomtext4, rwcustomtext5, rwcustomint1, rwcustomint2, rwcustomint3, rwcustomdbl1, 
        'rwcustomdbl2, rwcustomdbl3, rwcustomdate1, rwcustomdate2, rwcustomdate3

        'M3_RwGetdataById Detail -------------------------------------------------------
        'appname, appkey, appsecret, appactive, appcreated

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

        Dim utama As String = "", idtransaksi As String = ""

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
        'If Len(pagingSplit(5)) = 0 Then
        formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        'Else
        'formatTglWaktu = pagingSplit(5)
        'End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "idtransaksi required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rwid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rwid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        sql = "SELECT um.unama rwmodifikasiusernama, u.unama rwinputusernama, sb.nama rwstatussebelumnyanama, s.nama rwstatusnama, l.lnama rwlokasinama, b.bnama rwcabangnama, cs.kkode rwkodesopir, cs.knama rwnamasopir, i.bkode rwkodebarang, i.bnama rwnamabarang, rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwnopol, rwbid, rwkid, rwtglbruto, rwbruto, rwtgltara, rwtara, rwneto, rwharga, rwsopir, rwuraian, rwcatatan, rwnoref, rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwcustomtext1, rwcustomtext2, rwcustomtext3, rwcustomtext4, rwcustomtext5, rwcustomint1, rwcustomint2, rwcustomint3, rwcustomdbl1, rwcustomdbl2, rwcustomdbl3, rwcustomdate1, rwcustomdate2, rwcustomdate3  FROM M3_Rw rw JOIN m1_item i ON i.bid = rw.rwbid JOIN m1_contact cs ON cs.kid = rw.rwkid JOIN m1_branch b ON b.bkode = rw.rwcabang JOIN m1_location l ON l.lkode = rw.rwlokasi JOIN m0_status s ON s.kode = rw.rwstatus  JOIN m0_status sb ON sb.kode = rw.rwstatussebelumnya LEFT JOIN m0_user u ON u.userid = rw.rwinputuser LEFT JOIN m0_user um ON um.userid = rw.rwmodifikasiuser"

        dt = AmbilData("aplikasi1-m3_rw", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)
        'result(2) = dt.Rows.Count.ToString & " " & sql & " WHERE " + Filter + " ORDER BY " + Sorting : GoTo selesai
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rwid"), ""), sptField,
                            FxDB(drutama("rwcabang"), ""), sptField,
                            FxDB(drutama("rwlokasi"), ""), sptField,
                            FxDB(drutama("rwsumber"), ""), sptField,
                            FxDB(drutama("rwautonotransaksi"), 0), sptField,
                            FxDB(drutama("rwnotransaksi"), ""), sptField,
                            AsFormatTanggal(FxDB(drutama("rwtgl"), ""), formatTgl), sptField,
                            FxDB(drutama("rwkodepa"), ""), sptField,
                            FxDB(drutama("rwnopol"), ""), sptField,
                            FxDB(drutama("rwbid"), ""), sptField,
                            FxDB(drutama("rwkid"), ""), sptField,
                            AsFormatTanggal(FxDB(drutama("rwtglbruto"), ""), formatTglWaktu), sptField,
                            FxDB(drutama("rwbruto"), 0), sptField,
                            AsFormatTanggal(FxDB(drutama("rwtgltara"), ""), formatTglWaktu), sptField,
                            FxDB(drutama("rwtara"), 0), sptField,
                            FxDB(drutama("rwneto"), 0), sptField,
                            FxDB(drutama("rwharga"), 0), sptField,
                            FxDB(drutama("rwsopir"), ""), sptField,
                            FxDB(drutama("rwuraian"), ""), sptField,
                            FxDB(drutama("rwcatatan"), ""), sptField,
                            FxDB(drutama("rwnoref"), ""), sptField,
                            AsFormatTanggal(FxDB(drutama("rwtglnoref"), ""), formatTgl), sptField,
                            FxDB(drutama("rwstatus"), 0), sptField,
                            FxDB(drutama("rwstatussebelumnya"), 0), sptField,
                            FxDB(drutama("rwjmlrevisi"), 0), sptField,
                            FxDB(drutama("rwcetakanke"), 0), sptField,
                            FxDB(drutama("rwinputuser"), ""), sptField,
                            AsFormatTanggal(FxDB(drutama("rwinputtgl"), ""), formatTglWaktu), sptField,
                            FxDB(drutama("rwmodifikasiuser"), ""), sptField,
                            AsFormatTanggal(FxDB(drutama("rwmodifikasitgl"), ""), formatTglWaktu), sptField,
                            FxDB(drutama("rwposting"), 0), sptField,
                            AsFormatTanggal(FxDB(drutama("rwpostingtgl"), ""), formatTglWaktu), sptField,
                            FxDB(drutama("rwisclose"), 0), sptField,
                            FxDB(drutama("rwcustomtext1"), ""), sptField,
                            FxDB(drutama("rwcustomtext2"), ""), sptField,
                            FxDB(drutama("rwcustomtext3"), ""), sptField,
                            FxDB(drutama("rwcustomtext4"), ""), sptField,
                            FxDB(drutama("rwcustomtext5"), ""), sptField,
                            FxDB(drutama("rwcustomint1"), 0), sptField,
                            FxDB(drutama("rwcustomint2"), 0), sptField,
                            FxDB(drutama("rwcustomint3"), 0), sptField,
                            FxDB(drutama("rwcustomdbl1"), 0), sptField,
                            FxDB(drutama("rwcustomdbl2"), 0), sptField,
                            FxDB(drutama("rwcustomdbl3"), 0), sptField,
                            AsFormatTanggal(FxDB(drutama("rwcustomdate1"), ""), formatTgl), sptField,
                            AsFormatTanggal(FxDB(drutama("rwcustomdate2"), ""), formatTgl), sptField,
                            AsFormatTanggal(FxDB(drutama("rwcustomdate3"), ""), formatTgl), sptField,
                            FxDB(drutama("rwkodebarang"), ""), sptField,
                            FxDB(drutama("rwnamabarang"), ""), sptField,
                            FxDB(drutama("rwkodesopir"), ""), sptField,
                            FxDB(drutama("rwnamasopir"), ""), sptField,
                            FxDB(drutama("rwstatusnama"), ""), sptField,
                            FxDB(drutama("rwlokasinama"), ""), sptField,
                            FxDB(drutama("rwcabangnama"), ""), sptField,
                            FxDB(drutama("rwcabangnama"), ""), sptField,
                            FxDB(drutama("rwstatussebelumnyanama"), ""), sptField,
                            FxDB(drutama("rwinputusernama"), ""), sptField,
                            FxDB(drutama("rwmodifikasiusernama"), ""), sptRow)

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
        strResultData = String.Concat(utama)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rwid, rwcabang, rwlokasi, rwsumber, rwautonotransaksi, rwnotransaksi, rwtgl, rwkodepa, rwnopol, rwbid, rwkid, rwtglbruto, rwbruto, rwtgltara, rwtara, rwneto, rwharga, rwsopir, rwuraian, rwcatatan, rwnoref, rwtglnoref, rwstatus, rwstatussebelumnya, rwjmlrevisi, rwcetakanke, rwinputuser, rwinputtgl, rwmodifikasiuser, rwmodifikasitgl, rwposting, rwpostingtgl, rwisclose, rwcustomtext1, rwcustomtext2, rwcustomtext3, rwcustomtext4, rwcustomtext5, rwcustomint1, rwcustomint2, rwcustomint3, rwcustomdbl1, rwcustomdbl2, rwcustomdbl3, rwcustomdate1, rwcustomdate2, rwcustomdate3, rwkodebarang, rwnamabarang, rwkodesopir, rwnamasopir, rwstatusnama, rwlokasinama, rwcabangnama, rwstatussebelumnyanama, rwinputusernama, rwmodifikasiusernama"))

        Return wsResult
    End Function

End Class