Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_rp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_RpSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'rpid(0) As Integer, rpcabang(1) As String, rplokasi(2) As String, rpjenis(3) As Integer, rpsumber(4) As String, 
        'rpautonotransaksi(5) As Integer, rpnotransaksi(6) As String, rptgl(7) As Date, rpkodepa(8) As Integer, rpkontak(9) As Integer, 
        'rpkontakperson(10) As String, rp1alamat1(11) As String, rp1alamat2(12) As String, rp1alamat3(13) As String, rp2alamat1(14) As String, 
        'rp2alamat2(15) As String, rp2alamat3(16) As String, rpbagianterima(17) As Integer, rptermin(18) As String, rptgljatuhtempo(19) As Date, 
        'rpidsi(20) As Integer, rpnorek(21) As String, rpuraian(22) As String, rpcatatan(23) As String, rpnoref(24) As String, 
        'rptglnoref(25) As Date, rpmatauang(26) As String, rpkurs(27) As Double, rpjumlah(28) As Double, rpjumlahvalas(29) As Double, 
        'rpjumlahbayar(30) As Double, rpjumlahbayarvalas(31) As Double, rpstatusbayar(32) As Integer, rptgllunas(33) As Date, rpcostcenter(34) As String, 
        'rpdivisi(35) As String, rpsubdivisi(36) As String, rpproyek(37) As String, rpstatus(38) As Integer, rpstatussebelumnya(39) As Integer, 
        'rpjmlrevisi(40) As Integer, rpcetakanke(41) As Integer, rpinputuser(42) As Integer, rpinputtgl(43) As DateTime, rpmodifikasiuser(44) As Integer, 
        'rpmodifikasitgl(45) As DateTime, rpposting(46) As Integer, rpisclose(47) As Integer, rpcustomtext1(48) As String, rpcustomtext2(49) As String, 
        'rpcustomtext3(50) As String, rpcustomtext4(51) As String, rpcustomtext5(52) As String, rpcustomint1(53) As Integer, rpcustomint2(54) As Integer, 
        'rpcustomint3(55) As Integer, rpcustomdbl1(56) As Double, rpcustomdbl2(57) As Double, rpcustomdbl3(58) As Double, rpcustomdate1(59) As Date, 
        'rpcustomdate2(60) As Date, rpcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
        'rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, 
        'rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, 
        'rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, 
        'rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, 
        'rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, 
        'rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rpisclose, rpcustomtext1, 
        'rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, 
        'rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rpid required numeric." : GoTo selesai
        End If
        'rpjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "rpjenis required numeric." : GoTo selesai
        End If
        'rpautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rpautonotransaksi required numeric." : GoTo selesai
        End If
        'rptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "rptgl required date." : GoTo selesai
        End If
        'rpkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rpkodepa required numeric." : GoTo selesai
        End If
        'rpkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rpkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rpkontak can't be empty." : GoTo selesai
        End If
        'rpbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rpbagianterima required numeric." : GoTo selesai
        End If
        'rptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "rptgljatuhtempo required date." : GoTo selesai
        End If
        'rpidsi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rpidsi required numeric." : GoTo selesai
        End If
        'rptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rptglnoref required date." : GoTo selesai
        End If
        'rpkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rpkurs required numeric." : GoTo selesai
        End If
        'rpjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "rpjumlah required numeric." : GoTo selesai
        End If
        'rpjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "rpjumlahvalas required numeric." : GoTo selesai
        End If
        'rpjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "rpjumlahbayar required numeric." : GoTo selesai
        End If
        'rpjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rpjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'rpstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rpstatusbayar required numeric." : GoTo selesai
        End If
        'rptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "rptgllunas required date." : GoTo selesai
        End If
        'rpstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rpstatus required numeric." : GoTo selesai
        End If
        'rpstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rpstatussebelumnya required numeric." : GoTo selesai
        End If
        'rpjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rpjmlrevisi required numeric." : GoTo selesai
        End If
        'rpcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rpcetakanke required numeric." : GoTo selesai
        End If
        'rpinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rpinputuser required numeric." : GoTo selesai
        End If
        'rpinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rpinputtgl required date." : GoTo selesai
        End If
        'rpmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rpmodifikasiuser required numeric." : GoTo selesai
        End If
        'rpmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rpmodifikasitgl required date." : GoTo selesai
        End If
        'rpposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "rpposting required numeric." : GoTo selesai
        End If
        'rpisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rpisclose required numeric." : GoTo selesai
        End If
        'rpcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "rpcustomint1 required numeric." : GoTo selesai
        End If
        'rpcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rpcustomint2 required numeric." : GoTo selesai
        End If
        'rpcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rpcustomint3 required numeric." : GoTo selesai
        End If
        'rpcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rpcustomdbl1 required numeric." : GoTo selesai
        End If
        'rpcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "rpcustomdbl2 required numeric." : GoTo selesai
        End If
        'rpcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rpcustomdbl3 required numeric." : GoTo selesai
        End If
        'rpcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "rpcustomdate1 required date." : GoTo selesai
        End If
        'rpcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "rpcustomdate2 required date." : GoTo selesai
        End If
        'rpcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "rpcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rpcabang should not be more than 25 character." : GoTo selesai
        End If

        'rplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rplokasi should not be more than 25 character." : GoTo selesai
        End If

        'rpsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "rpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "rpsumber should not be more than 10 character." : GoTo selesai
        End If

        'rpnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "rpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rptgl can't be empty" : GoTo selesai
        End If

        'rptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rpnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "rpnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "rpnorek should not be more than 25 character." : GoTo selesai
        End If

        'rptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rptglnoref can't be empty" : GoTo selesai
        End If

        'rpmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "rpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rpkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rpkurs can't be empty" : GoTo selesai
        End If

        'rpjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "rpjumlah can't be empty" : GoTo selesai
        End If

        'rpjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "rpjumlahvalas can't be empty" : GoTo selesai
        End If

        'rpjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rpjumlahbayar can't be empty" : GoTo selesai
        End If

        'rpjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rpjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'rptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rptgllunas can't be empty" : GoTo selesai
        End If

        'rpinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rpinputtgl can't be empty" : GoTo selesai
        End If

        'rpmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rpcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "rpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rpcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "rpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rpcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "rpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rpcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "rpcustomdate1 can't be empty" : GoTo selesai
        End If

        'rpcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "rpcustomdate2 can't be empty" : GoTo selesai
        End If

        'rpcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "rpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpidsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rpjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rpjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rpid~rpcabang~rplokasi~rpjenis~rpsumber~rpautonotransaksi~rpnotransaksi~rptgl~rpkodepa~rpkontak~rpkontakperson~rp1alamat1~rp1alamat2~rp1alamat3~rp2alamat1~rp2alamat2~rp2alamat3~rpbagianterima~rptermin~rptgljatuhtempo~rpidsi~rpnorek~rpuraian~rpcatatan~rpnoref~rptglnoref~rpmatauang~rpkurs~rpjumlah~rpjumlahvalas~rpjumlahbayar~rpjumlahbayarvalas~rpstatusbayar~rptgllunas~rpcostcenter~rpdivisi~rpsubdivisi~rpproyek~rpstatus~rpstatussebelumnya~rpjmlrevisi~rpcetakanke~rpinputuser~rpinputtgl~rpmodifikasiuser~rpmodifikasitgl~rpposting~rpisclose~rpcustomtext1~rpcustomtext2~rpcustomtext3~rpcustomtext4~rpcustomtext5~rpcustomint1~rpcustomint2~rpcustomint3~rpcustomdbl1~rpcustomdbl2~rpcustomdbl3~rpcustomdate1~rpcustomdate2~rpcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrpcarabayar(0) As Integer, idrp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrpcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrpcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrpcarabayar required numeric." : GoTo selesai
            End If
            'idrp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrp required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'jumlah(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(5) <= 0 Then
            '    result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            'End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljt(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgljt can't be empty" : GoTo selesai
            End If

            'rekbank(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
            End If

            'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
            If dataRowDetail(2) = 2 Then
                'nogiro(7) As String
                If Len(dataRowDetail(7)) = 0 Then
                    result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(7)) > 25 Then
                    result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowDetail(9)) = 0 Then
                    result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(9)) > 25 Then
                    result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(10)) > 50 Then
                    result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowDetail(12)) = 0 Then
                    result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(12)) > 25 Then
                    result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrpcarabayar~idrp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 41
                Select Case drutama("rpstatus")
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


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rptgl")), AsFormatTanggal(drutama("rptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "rpmatauang", "rpnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rptermin").ToString, AsFormatTanggal(drutama("rptgl")), "rptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rpjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rpjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("rpjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("rpjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                If isUpdate Then
                    result(4) = drutama("rpid")
                    notransaksi = drutama("rpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rpid), rpnotransaksi FROM M5_rp WHERE rpid='" & result(4) & "' AND rpstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rpautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rpcabang"), drutama("rplokasi"), drutama("rpsumber"), drutama("rptgl"))
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

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rpid) FROM M5_rp WHERE rpnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_rp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Rp_HistorySimpan("" & paramSplit(0) & "★M5_Rp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rpsumber")) & "▼" & FixQuotes(drutama("rpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Rp set rpcabang  = '" & FixQuotes(drutama("rpcabang")) & "', rplokasi  = '" & FixQuotes(drutama("rplokasi")) & "', rpjenis  = " & drutama("rpjenis") & ", rpsumber  = '" & FixQuotes(drutama("rpsumber")) & "', rpautonotransaksi  = " & drutama("rpautonotransaksi") & ", rpnotransaksi  = '" & notransaksi & "', rptgl  = '" & FixQuotes(AsFormatTanggal(drutama("rptgl"))) & "', rpkodepa  = " & drutama("rpkodepa") & ", rpkontak  = " & drutama("rpkontak") & ", rpkontakperson  = '" & FixQuotes(drutama("rpkontakperson")) & "', rp1alamat1  = '" & FixQuotes(drutama("rp1alamat1")) & "', rp1alamat2  = '" & FixQuotes(drutama("rp1alamat2")) & "', rp1alamat3  = '" & FixQuotes(drutama("rp1alamat3")) & "', rp2alamat1  = '" & FixQuotes(drutama("rp2alamat1")) & "', rp2alamat2  = '" & FixQuotes(drutama("rp2alamat2")) & "', rp2alamat3  = '" & FixQuotes(drutama("rp2alamat3")) & "', rpbagianterima  = " & drutama("rpbagianterima") & ", rptermin  = '" & FixQuotes(drutama("rptermin")) & "', rptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rptgljatuhtempo"))) & "', rpidsi  = " & drutama("rpidsi") & ", rpnorek  = '" & FixQuotes(drutama("rpnorek")) & "', rpuraian  = '" & FixQuotes(drutama("rpuraian")) & "', rpcatatan  = '" & FixQuotes(drutama("rpcatatan")) & "', rpnoref  = '" & FixQuotes(drutama("rpnoref")) & "', rptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rptglnoref"))) & "', rpmatauang  = '" & FixQuotes(drutama("rpmatauang")) & "', rpkurs  = '" & FixDouble(drutama("rpkurs")) & "', rpjumlah  = '" & FixDouble(drutama("rpjumlah")) & "', rpjumlahvalas  = '" & FixDouble(drutama("rpjumlahvalas")) & "', rpjumlahbayar  = '" & FixDouble(drutama("rpjumlahbayar")) & "', rpjumlahbayarvalas  = '" & FixDouble(drutama("rpjumlahbayarvalas")) & "', rpstatusbayar  = " & drutama("rpstatusbayar") & ", rptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rptgllunas"))) & "', rpcostcenter  = '" & FixQuotes(drutama("rpcostcenter")) & "', rpdivisi  = '" & FixQuotes(drutama("rpdivisi")) & "', rpsubdivisi  = '" & FixQuotes(drutama("rpsubdivisi")) & "', rpproyek  = '" & FixQuotes(drutama("rpproyek")) & "', rpstatus  = " & drutama("rpstatus") & ", rpstatussebelumnya  = " & drutama("rpstatussebelumnya") & ", rpjmlrevisi  = rpjmlrevisi+1, rpcetakanke  = " & drutama("rpcetakanke") & ", rpmodifikasiuser  = " & drutama("rpmodifikasiuser") & ", rpmodifikasitgl  = NOW(), rpposting  = 0, rpcustomtext1  = '" & FixQuotes(drutama("rpcustomtext1")) & "', rpcustomtext2  = '" & FixQuotes(drutama("rpcustomtext2")) & "', rpcustomtext3  = '" & FixQuotes(drutama("rpcustomtext3")) & "', rpcustomtext4  = '" & FixQuotes(drutama("rpcustomtext4")) & "', rpcustomtext5  = '" & FixQuotes(drutama("rpcustomtext5")) & "', rpcustomint1  = " & drutama("rpcustomint1") & ", rpcustomint2  = " & drutama("rpcustomint2") & ", rpcustomint3  = " & drutama("rpcustomint3") & ", rpcustomdbl1  = '" & FixDouble(drutama("rpcustomdbl1")) & "', rpcustomdbl2  = '" & FixDouble(drutama("rpcustomdbl2")) & "', rpcustomdbl3  = '" & FixDouble(drutama("rpcustomdbl3")) & "', rpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate1"))) & "', rpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate2"))) & "', rpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate3"))) & "' where rpid = '" & drutama("rpid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("rpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rpcabang"), drutama("rplokasi"), drutama("rpsumber"), drutama("rptgl"))
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
                        notransaksi = drutama("rpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rpid) FROM M5_rp WHERE rpnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Rp (rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rpisclose, rpcustomtext1, rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3) values('" & FixQuotes(drutama("rpcabang")) & "', '" & FixQuotes(drutama("rplokasi")) & "', " & drutama("rpjenis") & ", '" & FixQuotes(drutama("rpsumber")) & "', " & drutama("rpautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rptgl"))) & "', " & drutama("rpkodepa") & ", " & drutama("rpkontak") & ", '" & FixQuotes(drutama("rpkontakperson")) & "', '" & FixQuotes(drutama("rp1alamat1")) & "', '" & FixQuotes(drutama("rp1alamat2")) & "', '" & FixQuotes(drutama("rp1alamat3")) & "', '" & FixQuotes(drutama("rp2alamat1")) & "', '" & FixQuotes(drutama("rp2alamat2")) & "', '" & FixQuotes(drutama("rp2alamat3")) & "', " & drutama("rpbagianterima") & ", '" & FixQuotes(drutama("rptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rptgljatuhtempo"))) & "', " & drutama("rpidsi") & ", '" & FixQuotes(drutama("rpnorek")) & "', '" & FixQuotes(drutama("rpuraian")) & "', '" & FixQuotes(drutama("rpcatatan")) & "', '" & FixQuotes(drutama("rpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rptglnoref"))) & "', '" & FixQuotes(drutama("rpmatauang")) & "', '" & FixDouble(drutama("rpkurs")) & "', '" & FixDouble(drutama("rpjumlah")) & "', '" & FixDouble(drutama("rpjumlahvalas")) & "', '" & FixDouble(drutama("rpjumlahbayar")) & "', '" & FixDouble(drutama("rpjumlahbayarvalas")) & "', " & drutama("rpstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("rptgllunas"))) & "', '" & FixQuotes(drutama("rpcostcenter")) & "', '" & FixQuotes(drutama("rpdivisi")) & "', '" & FixQuotes(drutama("rpsubdivisi")) & "', '" & FixQuotes(drutama("rpproyek")) & "', " & drutama("rpstatus") & ", " & drutama("rpstatussebelumnya") & ", " & drutama("rpjmlrevisi") & ", " & drutama("rpcetakanke") & ", " & drutama("rpinputuser") & ", NOW(), " & drutama("rpmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("rpisclose") & ", '" & FixQuotes(drutama("rpcustomtext1")) & "', '" & FixQuotes(drutama("rpcustomtext2")) & "', '" & FixQuotes(drutama("rpcustomtext3")) & "', '" & FixQuotes(drutama("rpcustomtext4")) & "', '" & FixQuotes(drutama("rpcustomtext5")) & "', " & drutama("rpcustomint1") & ", " & drutama("rpcustomint2") & ", " & drutama("rpcustomint3") & ", '" & FixDouble(drutama("rpcustomdbl1")) & "', '" & FixDouble(drutama("rpcustomdbl2")) & "', '" & FixDouble(drutama("rpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDBCon("select rpid from M5_rp where rpnotransaksi='" & notransaksi & "' AND rpinputuser= '" & userid & "' order by rpmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Rp_Pay where idrp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrpcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("rpstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(5, 41, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("rpsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("rpkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_Rp_Pay(idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("rpstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
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
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rpstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    If PostingJurnal.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'", myConn)
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

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

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'myconn.Close()
        'myconn = Nothing
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
    Public Function M5_RpUpdateStatus(ByVal param As String) As String

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

        Dim pg1 As New RsPaging
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
            Dim sumber As String = "Rp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rptgl, Rpnotransaksi, Rpstatus FROM M5_Rp WHERE Rpid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rpstatussebelumnya" : jnsaktivitas = 17
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

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_rp_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Rp_HistorySimpan("" & paramSplit(0) & "★M5_Rp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m5_rp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = asdatatableambildaridbcon("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'RP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M5_Rp SET Rpstatus = " & nilaiStatus & ", Rpmodifikasiuser='" & userid & "', Rpmodifikasitgl = NOW(), Rpposting = 0, Rppostingtgl = '1971-01-01 00:00:00', Rpjmlrevisi = Rpjmlrevisi + 1 WHERE Rpid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_RpSearch(PostWsSearch(paramSplit(0), "M5_RpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_RpDelete(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

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
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Rp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rpid, Rpnotransaksi FROM M5_Rp WHERE Rpid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rpcabang, rplokasi, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl"
            sql &= " FROM M5_rp"
            sql &= " WHERE rpid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rpcabang")
                lokasi = dtNomorNext.Rows(0)("rplokasi")
                sumber = dtNomorNext.Rows(0)("rpsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rpautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rpnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Rp_Pay WHERE idrp = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Rp WHERE rpid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
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
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


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
            Dim paramSearch As String = M5_RpSearch(PostWsSearch(paramSplit(0), "M5_RpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'END OF DELETE DI DATABASE ==========================================================

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
    Public Function M5_RpGetdataById(ByVal param As String) As String
        'M5_RpGetdataById Utama --------------------------------------------------------
        'rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
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
        'rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama, kpkp

        'M5_RpGetdataById Pay -------------------------------------------------------
        'idrpcarabayar, idrp, carabayar, matauang, 
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

        Dim NmMemcached As String = "aplikasi1-M5_Rp~M5_Rp_Pay-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rpid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rpid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_rp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rpid"), 0), sptField,
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
                     FxDB(drutama("rpmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrpcarabayar"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rppostingtgl, rpisclose, rpcustomtext1, rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3, rpcabangnama, rplokasinama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, rpterminnama, rpterminharijatuhtempo, sinotransaksi, rpnoreknama, rpcostcenternama, rpdivisinama, rpsubdivisinama, rpproyeknama, rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama, kpkp"), sptSubParam, ReplaceMapping("idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RpSearch(ByVal param As String) As String
        'M5_RpSearch --------------------------------------------------------
        'rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
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
        sql = query.PanggilQuery("m5_rp_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Rp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rpid"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rrproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rrposting, rrpostingtgl, rpisclose, rpcabangnama, rplokasinama, rpjenisnama, rpkontakkode, rpkontaknama, rpbagianterimakode, rpbagianterimanama, sinotransaksi, rpnoreknama, rpstatusnama, rpstatussebelumnyanama, rpinputusernama, rpmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RpTerkait(ByVal param As String) As String
        'M5_RpTerkait --------------------------------------------------------
        'rpid, rpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
        'modifikasitglterkait, jenisterkait

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
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "rpid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_rp_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rpid"), 0), sptField,
                     FxDB(dr("rpnotransaksi"), ""), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idterkait"), 0), sptField,
                     FxDB(dr("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(dr("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related RP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rpid, rpnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RpSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'rpid(0) As Integer, rpcabang(1) As String, rplokasi(2) As String, rpjenis(3) As Integer, rpsumber(4) As String, 
        'rpautonotransaksi(5) As Integer, rpnotransaksi(6) As String, rptgl(7) As Date, rpkodepa(8) As Integer, rpkontak(9) As Integer, 
        'rpkontakperson(10) As String, rp1alamat1(11) As String, rp1alamat2(12) As String, rp1alamat3(13) As String, rp2alamat1(14) As String, 
        'rp2alamat2(15) As String, rp2alamat3(16) As String, rpbagianterima(17) As Integer, rptermin(18) As String, rptgljatuhtempo(19) As Date, 
        'rpidsi(20) As Integer, rpnorek(21) As String, rpuraian(22) As String, rpcatatan(23) As String, rpnoref(24) As String, 
        'rptglnoref(25) As Date, rpmatauang(26) As String, rpkurs(27) As Double, rpjumlah(28) As Double, rpjumlahvalas(29) As Double, 
        'rpjumlahbayar(30) As Double, rpjumlahbayarvalas(31) As Double, rpstatusbayar(32) As Integer, rptgllunas(33) As Date, rpcostcenter(34) As String, 
        'rpdivisi(35) As String, rpsubdivisi(36) As String, rpproyek(37) As String, rpstatus(38) As Integer, rpstatussebelumnya(39) As Integer, 
        'rpjmlrevisi(40) As Integer, rpcetakanke(41) As Integer, rpinputuser(42) As Integer, rpinputtgl(43) As DateTime, rpmodifikasiuser(44) As Integer, 
        'rpmodifikasitgl(45) As DateTime, rpposting(46) As Integer, rpisclose(47) As Integer, rpcustomtext1(48) As String, rpcustomtext2(49) As String, 
        'rpcustomtext3(50) As String, rpcustomtext4(51) As String, rpcustomtext5(52) As String, rpcustomint1(53) As Integer, rpcustomint2(54) As Integer, 
        'rpcustomint3(55) As Integer, rpcustomdbl1(56) As Double, rpcustomdbl2(57) As Double, rpcustomdbl3(58) As Double, rpcustomdate1(59) As Date, 
        'rpcustomdate2(60) As Date, rpcustomdate3(61) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rpid, rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, 
        'rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, 
        'rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, 
        'rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, 
        'rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, 
        'rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, 
        'rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rpisclose, rpcustomtext1, 
        'rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, 
        'rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 62) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rpid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rpid required numeric." : GoTo selesai
        End If
        'rpjenis(3) As Integer
        If (IsNumeric(dataUtama(3)) = False) Then
            result(2) = "rpjenis required numeric." : GoTo selesai
        End If
        'rpautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rpautonotransaksi required numeric." : GoTo selesai
        End If
        'rptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "rptgl required date." : GoTo selesai
        End If
        'rpkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rpkodepa required numeric." : GoTo selesai
        End If
        'rpkontak(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rpkontak required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "rpkontak can't be empty." : GoTo selesai
        End If
        'rpbagianterima(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rpbagianterima required numeric." : GoTo selesai
        End If
        'rptgljatuhtempo(19) As Date
        If (IsDate(dataUtama(19)) = False) Then
            result(2) = "rptgljatuhtempo required date." : GoTo selesai
        End If
        'rpidsi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rpidsi required numeric." : GoTo selesai
        End If
        'rptglnoref(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "rptglnoref required date." : GoTo selesai
        End If
        'rpkurs(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "rpkurs required numeric." : GoTo selesai
        End If
        'rpjumlah(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "rpjumlah required numeric." : GoTo selesai
        End If
        'rpjumlahvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "rpjumlahvalas required numeric." : GoTo selesai
        End If
        'rpjumlahbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "rpjumlahbayar required numeric." : GoTo selesai
        End If
        'rpjumlahbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rpjumlahbayarvalas required numeric." : GoTo selesai
        End If
        'rpstatusbayar(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rpstatusbayar required numeric." : GoTo selesai
        End If
        'rptgllunas(33) As Date
        If (IsDate(dataUtama(33)) = False) Then
            result(2) = "rptgllunas required date." : GoTo selesai
        End If
        'rpstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rpstatus required numeric." : GoTo selesai
        End If
        'rpstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rpstatussebelumnya required numeric." : GoTo selesai
        End If
        'rpjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "rpjmlrevisi required numeric." : GoTo selesai
        End If
        'rpcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rpcetakanke required numeric." : GoTo selesai
        End If
        'rpinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rpinputuser required numeric." : GoTo selesai
        End If
        'rpinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "rpinputtgl required date." : GoTo selesai
        End If
        'rpmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rpmodifikasiuser required numeric." : GoTo selesai
        End If
        'rpmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rpmodifikasitgl required date." : GoTo selesai
        End If
        'rpposting(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "rpposting required numeric." : GoTo selesai
        End If
        'rpisclose(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rpisclose required numeric." : GoTo selesai
        End If
        'rpcustomint1(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "rpcustomint1 required numeric." : GoTo selesai
        End If
        'rpcustomint2(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rpcustomint2 required numeric." : GoTo selesai
        End If
        'rpcustomint3(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rpcustomint3 required numeric." : GoTo selesai
        End If
        'rpcustomdbl1(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rpcustomdbl1 required numeric." : GoTo selesai
        End If
        'rpcustomdbl2(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "rpcustomdbl2 required numeric." : GoTo selesai
        End If
        'rpcustomdbl3(58) As Double
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rpcustomdbl3 required numeric." : GoTo selesai
        End If
        'rpcustomdate1(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "rpcustomdate1 required date." : GoTo selesai
        End If
        'rpcustomdate2(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "rpcustomdate2 required date." : GoTo selesai
        End If
        'rpcustomdate3(61) As Date
        If (IsDate(dataUtama(61)) = False) Then
            result(2) = "rpcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rpcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rpcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rpcabang should not be more than 25 character." : GoTo selesai
        End If

        'rplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rplokasi should not be more than 25 character." : GoTo selesai
        End If

        'rpsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "rpsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "rpsumber should not be more than 10 character." : GoTo selesai
        End If

        'rpnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "rpnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "rpnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rptgl can't be empty" : GoTo selesai
        End If

        'rptgljatuhtempo(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "rptgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rpnorek(21) As String
        If Len(dataUtama(21)) = 0 Then
            result(2) = "rpnorek can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(21)) > 25 Then
            result(2) = "rpnorek should not be more than 25 character." : GoTo selesai
        End If

        'rptglnoref(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "rptglnoref can't be empty" : GoTo selesai
        End If

        'rpmatauang(26) As String
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rpmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(26)) > 25 Then
            result(2) = "rpmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rpkurs(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "rpkurs can't be empty" : GoTo selesai
        End If

        'rpjumlah(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "rpjumlah can't be empty" : GoTo selesai
        End If

        'rpjumlahvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "rpjumlahvalas can't be empty" : GoTo selesai
        End If

        'rpjumlahbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rpjumlahbayar can't be empty" : GoTo selesai
        End If

        'rpjumlahbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rpjumlahbayarvalas can't be empty" : GoTo selesai
        End If

        'rptgllunas(33) As Date
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rptgllunas can't be empty" : GoTo selesai
        End If

        'rpinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rpinputtgl can't be empty" : GoTo selesai
        End If

        'rpmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rpmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rpcustomdbl1(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "rpcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rpcustomdbl2(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "rpcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rpcustomdbl3(58) As Double
        If Len(dataUtama(58)) = 0 Then
            result(2) = "rpcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rpcustomdate1(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "rpcustomdate1 can't be empty" : GoTo selesai
        End If

        'rpcustomdate2(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "rpcustomdate2 can't be empty" : GoTo selesai
        End If

        'rpcustomdate3(61) As Date
        If Len(dataUtama(61)) = 0 Then
            result(2) = "rpcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rpid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjenis", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpbagianterima", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rptermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpidsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpnorek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rpjumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rpjumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpjumlahbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpstatusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rptgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rpcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rpcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rpid~rpcabang~rplokasi~rpjenis~rpsumber~rpautonotransaksi~rpnotransaksi~rptgl~rpkodepa~rpkontak~rpkontakperson~rp1alamat1~rp1alamat2~rp1alamat3~rp2alamat1~rp2alamat2~rp2alamat3~rpbagianterima~rptermin~rptgljatuhtempo~rpidsi~rpnorek~rpuraian~rpcatatan~rpnoref~rptglnoref~rpmatauang~rpkurs~rpjumlah~rpjumlahvalas~rpjumlahbayar~rpjumlahbayarvalas~rpstatusbayar~rptgllunas~rpcostcenter~rpdivisi~rpsubdivisi~rpproyek~rpstatus~rpstatussebelumnya~rpjmlrevisi~rpcetakanke~rpinputuser~rpinputtgl~rpmodifikasiuser~rpmodifikasitgl~rpposting~rpisclose~rpcustomtext1~rpcustomtext2~rpcustomtext3~rpcustomtext4~rpcustomtext5~rpcustomint1~rpcustomint2~rpcustomint3~rpcustomdbl1~rpcustomdbl2~rpcustomdbl3~rpcustomdate1~rpcustomdate2~rpcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrpcarabayar(0) As Integer, idrp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrpcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 16) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idrpcarabayar(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrpcarabayar required numeric." : GoTo selesai
            End If
            'idrp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrp required numeric." : GoTo selesai
            End If
            'carabayar(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - carabayar required numeric." : GoTo selesai
            End If
            'kurs(4) As Double
            If (IsNumeric(dataRowDetail(4)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'jumlah(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jumlah required numeric." : GoTo selesai
            End If
            'jumlahvalas(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
            End If
            'tgljt(8) As Date
            If (IsDate(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - tgljt required date." : GoTo selesai
            End If
            'urutan(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'matauang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(4) As Double
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'jumlah(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jumlah can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jumlah must be more than zero" : GoTo selesai
            End If

            'jumlahvalas(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
            End If

            'tgljt(8) As Date
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - tgljt can't be empty" : GoTo selesai
            End If

            'rekbank(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - rekbank can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
            End If

            'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
            If dataRowDetail(2) = 2 Then
                'nogiro(7) As String
                If Len(dataRowDetail(7)) = 0 Then
                    result(2) = "Row : " & i & " - nogiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(7)) > 25 Then
                    result(2) = "Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                End If

                'bank(9) As String
                If Len(dataRowDetail(9)) = 0 Then
                    result(2) = "Row : " & i & " - bank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(9)) > 25 Then
                    result(2) = "Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                End If

                'noacbank(10) As String
                If Len(dataRowDetail(10)) = 0 Then
                    result(2) = "Row : " & i & " - noacbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(10)) > 50 Then
                    result(2) = "Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                End If

                'rekgiro(12) As String
                If Len(dataRowDetail(12)) = 0 Then
                    result(2) = "Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(12)) > 25 Then
                    result(2) = "Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                End If
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrpcarabayar~idrp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rptgl")), AsFormatTanggal(drutama("rptgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "rpmatauang", "rpnorek", dtdetail, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rptermin").ToString, AsFormatTanggal(drutama("rptgl")), "rptgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rptgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'HITUNG TOTAL BERDASARKAN DATA DETAIL ===================
                drutama("rpjumlah") = AsDataTableDSum(dtdetail, "jumlah")
                drutama("rpjumlahvalas") = AsDataTableDSum(dtdetail, "jumlahvalas")
                'END OF HITUNG TOTAL BERDASARKAN DATA DETAIL ============


                ''CEK TOTAL UTAMA DAN DETAIL =============================
                'Dim jumlah As Double = AsDataTableDSum(dtdetail, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtdetail, "jumlahvalas")
                'If Double.Parse(drutama("rpjumlah")) <> jumlah Then
                '    result(2) = "Total amount of main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'ElseIf Double.Parse(drutama("rpjumlahvalas")) <> jumlahvalas Then
                '    result(2) = "Total amount of foreign main and detail are not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN DETAIL ======================


                If isUpdate Then
                    result(4) = drutama("rpid")
                    notransaksi = drutama("rpnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rpid), rpnotransaksi FROM M5_rp WHERE rpid='" & result(4) & "' AND rpstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rpid) FROM M5_rp WHERE rpnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_rp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Rp_HistorySimpan("" & paramSplit(0) & "★M5_Rp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rpsumber")) & "▼" & FixQuotes(drutama("rpid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Rp set rpcabang  = '" & FixQuotes(drutama("rpcabang")) & "', rplokasi  = '" & FixQuotes(drutama("rplokasi")) & "', rpjenis  = " & drutama("rpjenis") & ", rpsumber  = '" & FixQuotes(drutama("rpsumber")) & "', rpautonotransaksi  = " & drutama("rpautonotransaksi") & ", rpnotransaksi  = '" & notransaksi & "', rptgl  = '" & FixQuotes(AsFormatTanggal(drutama("rptgl"))) & "', rpkodepa  = " & drutama("rpkodepa") & ", rpkontak  = " & drutama("rpkontak") & ", rpkontakperson  = '" & FixQuotes(drutama("rpkontakperson")) & "', rp1alamat1  = '" & FixQuotes(drutama("rp1alamat1")) & "', rp1alamat2  = '" & FixQuotes(drutama("rp1alamat2")) & "', rp1alamat3  = '" & FixQuotes(drutama("rp1alamat3")) & "', rp2alamat1  = '" & FixQuotes(drutama("rp2alamat1")) & "', rp2alamat2  = '" & FixQuotes(drutama("rp2alamat2")) & "', rp2alamat3  = '" & FixQuotes(drutama("rp2alamat3")) & "', rpbagianterima  = " & drutama("rpbagianterima") & ", rptermin  = '" & FixQuotes(drutama("rptermin")) & "', rptgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rptgljatuhtempo"))) & "', rpidsi  = " & drutama("rpidsi") & ", rpnorek  = '" & FixQuotes(drutama("rpnorek")) & "', rpuraian  = '" & FixQuotes(drutama("rpuraian")) & "', rpcatatan  = '" & FixQuotes(drutama("rpcatatan")) & "', rpnoref  = '" & FixQuotes(drutama("rpnoref")) & "', rptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rptglnoref"))) & "', rpmatauang  = '" & FixQuotes(drutama("rpmatauang")) & "', rpkurs  = '" & FixDouble(drutama("rpkurs")) & "', rpjumlah  = '" & FixDouble(drutama("rpjumlah")) & "', rpjumlahvalas  = '" & FixDouble(drutama("rpjumlahvalas")) & "', rpjumlahbayar  = '" & FixDouble(drutama("rpjumlahbayar")) & "', rpjumlahbayarvalas  = '" & FixDouble(drutama("rpjumlahbayarvalas")) & "', rpstatusbayar  = " & drutama("rpstatusbayar") & ", rptgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rptgllunas"))) & "', rpcostcenter  = '" & FixQuotes(drutama("rpcostcenter")) & "', rpdivisi  = '" & FixQuotes(drutama("rpdivisi")) & "', rpsubdivisi  = '" & FixQuotes(drutama("rpsubdivisi")) & "', rpproyek  = '" & FixQuotes(drutama("rpproyek")) & "', rpstatus  = " & drutama("rpstatus") & ", rpstatussebelumnya  = " & drutama("rpstatussebelumnya") & ", rpjmlrevisi  = rpjmlrevisi+1, rpcetakanke  = " & drutama("rpcetakanke") & ", rpmodifikasiuser  = " & drutama("rpmodifikasiuser") & ", rpmodifikasitgl  = NOW(), rpposting  = 0, rpcustomtext1  = '" & FixQuotes(drutama("rpcustomtext1")) & "', rpcustomtext2  = '" & FixQuotes(drutama("rpcustomtext2")) & "', rpcustomtext3  = '" & FixQuotes(drutama("rpcustomtext3")) & "', rpcustomtext4  = '" & FixQuotes(drutama("rpcustomtext4")) & "', rpcustomtext5  = '" & FixQuotes(drutama("rpcustomtext5")) & "', rpcustomint1  = " & drutama("rpcustomint1") & ", rpcustomint2  = " & drutama("rpcustomint2") & ", rpcustomint3  = " & drutama("rpcustomint3") & ", rpcustomdbl1  = '" & FixDouble(drutama("rpcustomdbl1")) & "', rpcustomdbl2  = '" & FixDouble(drutama("rpcustomdbl2")) & "', rpcustomdbl3  = '" & FixDouble(drutama("rpcustomdbl3")) & "', rpcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate1"))) & "', rpcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate2"))) & "', rpcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate3"))) & "' where rpid = '" & drutama("rpid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("rpautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rpcabang"), drutama("rplokasi"), drutama("rpsumber"), drutama("rptgl"))
                        Dim arrNotransaksi(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksi = rsNotransaksi.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksi(0) = 1) Then
                            notransaksi = arrNotransaksi(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
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
                        notransaksi = drutama("rpnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rpid) FROM M5_rp WHERE rpnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Rp (rpcabang, rplokasi, rpjenis, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl, rpkodepa, rpkontak, rpkontakperson, rp1alamat1, rp1alamat2, rp1alamat3, rp2alamat1, rp2alamat2, rp2alamat3, rpbagianterima, rptermin, rptgljatuhtempo, rpidsi, rpnorek, rpuraian, rpcatatan, rpnoref, rptglnoref, rpmatauang, rpkurs, rpjumlah, rpjumlahvalas, rpjumlahbayar, rpjumlahbayarvalas, rpstatusbayar, rptgllunas, rpcostcenter, rpdivisi, rpsubdivisi, rpproyek, rpstatus, rpstatussebelumnya, rpjmlrevisi, rpcetakanke, rpinputuser, rpinputtgl, rpmodifikasiuser, rpmodifikasitgl, rpposting, rpisclose, rpcustomtext1, rpcustomtext2, rpcustomtext3, rpcustomtext4, rpcustomtext5, rpcustomint1, rpcustomint2, rpcustomint3, rpcustomdbl1, rpcustomdbl2, rpcustomdbl3, rpcustomdate1, rpcustomdate2, rpcustomdate3) values('" & FixQuotes(drutama("rpcabang")) & "', '" & FixQuotes(drutama("rplokasi")) & "', " & drutama("rpjenis") & ", '" & FixQuotes(drutama("rpsumber")) & "', " & drutama("rpautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(AsFormatTanggal(drutama("rptgl"))) & "', " & drutama("rpkodepa") & ", " & drutama("rpkontak") & ", '" & FixQuotes(drutama("rpkontakperson")) & "', '" & FixQuotes(drutama("rp1alamat1")) & "', '" & FixQuotes(drutama("rp1alamat2")) & "', '" & FixQuotes(drutama("rp1alamat3")) & "', '" & FixQuotes(drutama("rp2alamat1")) & "', '" & FixQuotes(drutama("rp2alamat2")) & "', '" & FixQuotes(drutama("rp2alamat3")) & "', " & drutama("rpbagianterima") & ", '" & FixQuotes(drutama("rptermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rptgljatuhtempo"))) & "', " & drutama("rpidsi") & ", '" & FixQuotes(drutama("rpnorek")) & "', '" & FixQuotes(drutama("rpuraian")) & "', '" & FixQuotes(drutama("rpcatatan")) & "', '" & FixQuotes(drutama("rpnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rptglnoref"))) & "', '" & FixQuotes(drutama("rpmatauang")) & "', '" & FixDouble(drutama("rpkurs")) & "', '" & FixDouble(drutama("rpjumlah")) & "', '" & FixDouble(drutama("rpjumlahvalas")) & "', '" & FixDouble(drutama("rpjumlahbayar")) & "', '" & FixDouble(drutama("rpjumlahbayarvalas")) & "', " & drutama("rpstatusbayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("rptgllunas"))) & "', '" & FixQuotes(drutama("rpcostcenter")) & "', '" & FixQuotes(drutama("rpdivisi")) & "', '" & FixQuotes(drutama("rpsubdivisi")) & "', '" & FixQuotes(drutama("rpproyek")) & "', " & drutama("rpstatus") & ", " & drutama("rpstatussebelumnya") & ", " & drutama("rpjmlrevisi") & ", " & drutama("rpcetakanke") & ", " & drutama("rpinputuser") & ", NOW(), " & drutama("rpmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("rpisclose") & ", '" & FixQuotes(drutama("rpcustomtext1")) & "', '" & FixQuotes(drutama("rpcustomtext2")) & "', '" & FixQuotes(drutama("rpcustomtext3")) & "', '" & FixQuotes(drutama("rpcustomtext4")) & "', '" & FixQuotes(drutama("rpcustomtext5")) & "', " & drutama("rpcustomint1") & ", " & drutama("rpcustomint2") & ", " & drutama("rpcustomint3") & ", '" & FixDouble(drutama("rpcustomdbl1")) & "', '" & FixDouble(drutama("rpcustomdbl2")) & "', '" & FixDouble(drutama("rpcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rpcustomdate3"))) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    Dim dt2 As New DataTable
                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    dt2 = AsDataTableAmbilDariDB("select rpid from M5_rp where rpnotransaksi='" & notransaksi & "' AND rpinputuser= '" & userid & "' order by rpmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Rp_Pay where idrp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strGiro As New StringBuilder
                    Dim rsCekGiro As String

                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrpcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")

                        'QUERY UNTUK INSERT GIRO
                        If dr1("carabayar") = 2 Then

                            'CEK HAK AKSES APPROVED GIRO KELUAR =====================
                            If drutama("rpstatus") = 2 Then
                                rsCekGiro = HakAksesGiro(5, 41, userid) 'MODULEID, MENUID, USERID SESUAI TRANSAKSI
                                If Len(rsCekGiro) <> 0 Then result(2) = rsCekGiro : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK HAK AKSES APPROVED GIRO KELUAR ==============

                            strGiro.Append(IIf(Len(strGiro.ToString) = 0, "", ", "))
                            strGiro.Append("('" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(drutama("rpsumber")) & "', " & result(4) & ", '" & FixQuotes(notransaksi) & "', " & drutama("rpkontak") & ", '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', " & 1 & ", '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', " & 0 & ", " & 0 & ", " & dr1("urutan") & ")")
                        End If
                    Next
                    sql = "Insert into M5_Rp_Pay(idrpcarabayar, idrp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'INSERT GIRO JIKA STATUS APPROVED DAN CARABAYAR = 2
                    If drutama("rpstatus") = 2 And Len(strGiro.ToString) > 0 Then
                        sql = "Insert into M2_Giro_List(glnogiro, glsumber, glidtransaksi, glnotransaksi, glkontak, glrekbank, glrekgiro, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, gltglcair, glstatus, glstatussebelumnya, glurutan) values" & strGiro.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rpstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Journal(mjid, mjsumber, mjidtransaksi, mjprogress, mjpesan, mjtglantrian, mjtglselesai, mjuserid) values ('" _
                        & mjid & "', '" & sumber & "', '" & result(4) & "', '" & 0 & "', " & "''" & ", NOW(), '1971-01-01 00:00:00', '" & userid & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'MSMQ ANTRIAN
                    Dim PostingJurnal As String = F_getSetting(0, "accounting", "AutoPosting")
                    If PostingJurnal.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG ====================================================================
                'ambil moduleid dan menuid dari m0_nomor
                Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'")
                If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
                'jika update jnsaktivitas = 14, jika insert : jnsaktivitas = 13
                If isUpdate Then jnsaktivitas = 14 Else jnsaktivitas = 13

                sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                    & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF INSERT USER LOG =============================================================

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

            Else
                result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = ex.Message
            result(3) = 0
            result(4) = result(4)

        End Try

        objCmd = Nothing
        'Con1.Close()
        'Con1 = Nothing
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
    Public Function M5_RpUpdateStatusOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

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

        Dim pg1 As New RsPaging
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
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Rp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rptgl, Rpnotransaksi, Rpstatus FROM M5_Rp WHERE Rpid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rpstatussebelumnya" : jnsaktivitas = 17
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

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_rp_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Rp_HistorySimpan("" & paramSplit(0) & "★M5_Rp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m5_rp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                ''CEK STATUS GIRO
                'dtdetail = AsDataTableAmbilDariDB("SELECT glnogiro FROM m2_giro_list WHERE glsumber = 'RP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "' AND glstatus <> 0")
                'If dtdetail.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RP' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'DELETE GIRO
                sql = "DELETE FROM m2_giro_list WHERE glsumber = 'RP' AND glidtransaksi = '" & idtransaksi & "' AND glnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M5_Rp SET Rpstatus = " & nilaiStatus & ", Rpmodifikasiuser='" & userid & "', Rpmodifikasitgl = NOW(), Rpposting = 0, Rppostingtgl = '1971-01-01 00:00:00', Rpjmlrevisi = Rpjmlrevisi + 1 WHERE Rpid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
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
                .Connection = Con1
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
            Dim paramSearch As String = M5_RpSearch(PostWsSearch(paramSplit(0), "M5_RpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'Con1.Close()
        'Con1 = Nothing
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
    Public Function M5_RpDeleteOld(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

        Dim pg1 As New RsPaging
        Dim Filter As String = "", Sorting As String = "", search As String = ""
        Dim formatTgl As String = "yyyy-MM-dd"
        Dim formatTglWaktu As String = "yyyy-MM-dd H:mm:ss"

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

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

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
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Rp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rpid, Rpnotransaksi FROM M5_Rp WHERE Rpid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rpcabang, rplokasi, rpsumber, rpautonotransaksi, rpnotransaksi, rptgl"
            sql &= " FROM M5_rp"
            sql &= " WHERE rpid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rpcabang")
                lokasi = dtNomorNext.Rows(0)("rplokasi")
                sumber = dtNomorNext.Rows(0)("rpsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rpautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rpnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_Rp_Pay WHERE idrp = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_Rp WHERE rpid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'UPDATE NOMOR BERIKUTNYA ============================================================
            'JIKA AUTO NO. TRANSAKSI
            If autonotransaksi = 1 Then
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi)
                Dim arrNomorNext(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                arrNomorNext = rsNomorNext.Split(sptSubParam)
                'Cek success M0_DeleteNotransaksi
                If (arrNomorNext(0) = 1) Then
                    sql = arrNomorNext(3)
                    'Tambah query update m0_nomor_next
                    If Len(sql) > 0 Then
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                Else
                    result(2) = arrNomorNext(1) : Trans.Rollback() : GoTo selesai
                End If
            End If
            'END OF UPDATE NOMOR BERIKUTNYA =====================================================


            'INSERT USER LOG ====================================================================
            sql = "Insert into M0_Userlog (uluserid, ulidmodule, ulidmenu, uljenisaktivitas, ulaktivitas, ultgl, ulkodepa) values(" _
                & userid & ", " & mdlid & ", " & mnid & ", " & jnsaktivitas & ", '" & notransaksi & "', NOW(), " & 0 & ")"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
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
            Dim paramSearch As String = M5_RpSearch(PostWsSearch(paramSplit(0), "M5_RpSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        'Con1.Close()
        'Con1 = Nothing
        'END OF DELETE DI DATABASE ==========================================================

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

End Class