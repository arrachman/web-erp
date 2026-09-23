Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_vpp
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_VppSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPay(), dataRowPay() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'vppid(0) As Integer, vppcabang(1) As String, vpplokasi(2) As String, vppgudang(3) As String, vppsumber(4) As String, 
        'vppautonotransaksi(5) As Integer, vppnotransaksi(6) As String, vpptgl(7) As Date, vppkodepa(8) As Integer, vppsupplier(9) As Integer, 
        'vppsupplierkontak(10) As String, vpp1alamat1(11) As String, vpp1alamat2(12) As String, vpp1alamat3(13) As String, vpp2alamat1(14) As String, 
        'vpp2alamat2(15) As String, vpp2alamat3(16) As String, vppbagianpembayaran(17) As Integer, vppuraian(18) As String, vppcatatan(19) As String, 
        'vppnoref(20) As String, vpptglnoref(21) As Date, vppcarabayar(22) As Integer, vpptglbayar(23) As Date, vppmatauang(24) As String, 
        'vppkurs(25) As Double, vpptotalap(26) As Double, vpptotalapvalas(27) As Double, vpptotalar(28) As Double, vpptotalarvalas(29) As Double, 
        'vppbayar(30) As Double, vppbayarvalas(31) As Double, vppselisihkurs(32) As Double, vpprekselisihkurs(33) As String, vppdiskontermin(34) As Double, 
        'vppdiskonterminvalas(35) As Double, vpprekdiskontermin(36) As String, vppstatusvp(37) As Integer, vppstatus(38) As Integer, vppstatussebelumnya(39) As Integer, 
        'vppjmlrevisi(40) As Integer, vppcetakanke(41) As Integer, vppinputuser(42) As Integer, vppinputtgl(43) As DateTime, vppmodifikasiuser(44) As Integer, 
        'vppmodifikasitgl(45) As DateTime, vppisclose(46) As Integer, vppcustomtext1(47) As String, vppcustomtext2(48) As String, vppcustomtext3(49) As String, 
        'vppcustomtext4(50) As String, vppcustomtext5(51) As String, vppcustomint1(52) As Integer, vppcustomint2(53) As Integer, vppcustomint3(54) As Integer, 
        'vppcustomdbl1(55) As Double, vppcustomdbl2(56) As Double, vppcustomdbl3(57) As Double, vppcustomdate1(58) As Date, vppcustomdate2(59) As Date, 
        'vppcustomdate3(60) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppisclose, vppcustomtext1, vppcustomtext2, 
        'vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, 
        'vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 61) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'vppid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "vppid required numeric." : GoTo selesai
        End If
        'vppautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "vppautonotransaksi required numeric." : GoTo selesai
        End If
        'vpptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "vpptgl required date." : GoTo selesai
        End If
        'vppkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "vppkodepa required numeric." : GoTo selesai
        End If
        'vppsupplier(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "vppsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "vppsupplier can't be empty." : GoTo selesai
        End If
        'vppbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "vppbagianpembayaran required numeric." : GoTo selesai
        End If
        'vpptglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "vpptglnoref required date." : GoTo selesai
        End If
        'vppcarabayar(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "vppcarabayar required numeric." : GoTo selesai
        End If
        'vpptglbayar(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "vpptglbayar required date." : GoTo selesai
        End If
        'vppkurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "vppkurs required numeric." : GoTo selesai
        End If
        'vpptotalap(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "vpptotalap required numeric." : GoTo selesai
        End If
        'vpptotalapvalas(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "vpptotalapvalas required numeric." : GoTo selesai
        End If
        'vpptotalar(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "vpptotalar required numeric." : GoTo selesai
        End If
        'vpptotalarvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "vpptotalarvalas required numeric." : GoTo selesai
        End If
        'vppbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "vppbayar required numeric." : GoTo selesai
        End If
        'vppbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "vppbayarvalas required numeric." : GoTo selesai
        End If
        'vppselisihkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "vppselisihkurs required numeric." : GoTo selesai
        End If
        'vppdiskontermin(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "vppdiskontermin required numeric." : GoTo selesai
        End If
        'vppdiskonterminvalas(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "vppdiskonterminvalas required numeric." : GoTo selesai
        End If
        'vppstatusvp(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "vppstatusvp required numeric." : GoTo selesai
        End If
        'vppstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "vppstatus required numeric." : GoTo selesai
        End If
        'vppstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "vppstatussebelumnya required numeric." : GoTo selesai
        End If
        'vppjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "vppjmlrevisi required numeric." : GoTo selesai
        End If
        'vppcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "vppcetakanke required numeric." : GoTo selesai
        End If
        'vppinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "vppinputuser required numeric." : GoTo selesai
        End If
        'vppinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "vppinputtgl required date." : GoTo selesai
        End If
        'vppmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "vppmodifikasiuser required numeric." : GoTo selesai
        End If
        'vppmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "vppmodifikasitgl required date." : GoTo selesai
        End If
        'vppisclose(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "vppisclose required numeric." : GoTo selesai
        End If
        'vppcustomint1(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "vppcustomint1 required numeric." : GoTo selesai
        End If
        'vppcustomint2(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "vppcustomint2 required numeric." : GoTo selesai
        End If
        'vppcustomint3(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "vppcustomint3 required numeric." : GoTo selesai
        End If
        'vppcustomdbl1(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "vppcustomdbl1 required numeric." : GoTo selesai
        End If
        'vppcustomdbl2(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "vppcustomdbl2 required numeric." : GoTo selesai
        End If
        'vppcustomdbl3(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "vppcustomdbl3 required numeric." : GoTo selesai
        End If
        'vppcustomdate1(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "vppcustomdate1 required date." : GoTo selesai
        End If
        'vppcustomdate2(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "vppcustomdate2 required date." : GoTo selesai
        End If
        'vppcustomdate3(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "vppcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'vppcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "vppcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "vppcabang should not be more than 25 character." : GoTo selesai
        End If

        'vpplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "vpplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "vpplokasi should not be more than 25 character." : GoTo selesai
        End If

        'vppsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "vppsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "vppsumber should not be more than 10 character." : GoTo selesai
        End If

        'vppnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "vppnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "vppnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'vpptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "vpptgl can't be empty" : GoTo selesai
        End If

        'vpptglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "vpptglnoref can't be empty" : GoTo selesai
        End If

        'vpptglbayar(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "vpptglbayar can't be empty" : GoTo selesai
        End If

        'vppmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "vppmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "vppmatauang should not be more than 25 character." : GoTo selesai
        End If

        'vppkurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "vppkurs can't be empty" : GoTo selesai
        End If

        'vpptotalap(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "vpptotalap can't be empty" : GoTo selesai
        End If

        'vpptotalapvalas(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "vpptotalapvalas can't be empty" : GoTo selesai
        End If

        'vpptotalar(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "vpptotalar can't be empty" : GoTo selesai
        End If

        'vpptotalarvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "vpptotalarvalas can't be empty" : GoTo selesai
        End If

        'vppbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "vppbayar can't be empty" : GoTo selesai
        End If

        'vppbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "vppbayarvalas can't be empty" : GoTo selesai
        End If

        'vppselisihkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "vppselisihkurs can't be empty" : GoTo selesai
        End If

        'vppdiskontermin(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "vppdiskontermin can't be empty" : GoTo selesai
        End If

        'vppdiskonterminvalas(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "vppdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'vppinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "vppinputtgl can't be empty" : GoTo selesai
        End If

        'vppmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "vppmodifikasitgl can't be empty" : GoTo selesai
        End If

        'vppcustomdbl1(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "vppcustomdbl1 can't be empty" : GoTo selesai
        End If

        'vppcustomdbl2(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "vppcustomdbl2 can't be empty" : GoTo selesai
        End If

        'vppcustomdbl3(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "vppcustomdbl3 can't be empty" : GoTo selesai
        End If

        'vppcustomdate1(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "vppcustomdate1 can't be empty" : GoTo selesai
        End If

        'vppcustomdate2(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "vppcustomdate2 can't be empty" : GoTo selesai
        End If

        'vppcustomdate3(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "vppcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "vppid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpptglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vppbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vppselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpprekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpprekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppstatusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "vppid~vppcabang~vpplokasi~vppgudang~vppsumber~vppautonotransaksi~vppnotransaksi~vpptgl~vppkodepa~vppsupplier~vppsupplierkontak~vpp1alamat1~vpp1alamat2~vpp1alamat3~vpp2alamat1~vpp2alamat2~vpp2alamat3~vppbagianpembayaran~vppuraian~vppcatatan~vppnoref~vpptglnoref~vppcarabayar~vpptglbayar~vppmatauang~vppkurs~vpptotalap~vpptotalapvalas~vpptotalar~vpptotalarvalas~vppbayar~vppbayarvalas~vppselisihkurs~vpprekselisihkurs~vppdiskontermin~vppdiskonterminvalas~vpprekdiskontermin~vppstatusvp~vppstatus~vppstatussebelumnya~vppjmlrevisi~vppcetakanke~vppinputuser~vppinputtgl~vppmodifikasiuser~vppmodifikasitgl~vppisclose~vppcustomtext1~vppcustomtext2~vppcustomtext3~vppcustomtext4~vppcustomtext5~vppcustomint1~vppcustomint2~vppcustomint3~vppcustomdbl1~vppcustomdbl2~vppcustomdbl3~vppcustomdate1~vppcustomdate2~vppcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idvppdetail(0) As Integer, idvpp(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, rekhutangpiutang(14) As String, 
        'catatan(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'jmlvp(20) As Double, jmlvpvalas(21) As Double, statusvp(22) As Integer, urutan(23) As Integer, isclose(24) As Integer, 
        'customtext1(25) As String, customtext2(26) As String, customtext3(27) As String, customdbl1(28) As Double, customdbl2(29) As Double, 
        'customdbl3(30) As Double, customdate1(31) As Date, customdate2(32) As Date, customdate3(33) As Date, rencana(34) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, 
        'jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idvppdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlvp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlvpvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT
        Dim sumberDetail As String = "", idtransaksiDetail As Double = 0
        Dim updFilterRI As String = "", updFilterAP As String = "", updFilterPRT As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 35) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idvppdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idvppdetail required numeric." : GoTo selesai
            End If
            'idvpp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idvpp required numeric." : GoTo selesai
            End If
            'idtransaksi(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - idtransaksi required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'totaltransaksi(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - totaltransaksi required numeric." : GoTo selesai
            End If
            'terbayar(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - terbayar required numeric." : GoTo selesai
            End If
            'rencana(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - rencana required numeric." : GoTo selesai
            End If
            'sisa(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'jmlvp(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - jmlvp required numeric." : GoTo selesai
            End If
            'jmlvpvalas(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlvpvalas required numeric." : GoTo selesai
            End If
            'statusvp(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - statusvp required numeric." : GoTo selesai
            End If
            'urutan(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If
            If (dataRowDetail(2) <> "RI" And dataRowDetail(2) <> "AP" And dataRowDetail(2) <> "PRT" And dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
            End If

            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'totaltransaksi(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - totaltransaksi can't be empty" : GoTo selesai
            End If

            'terbayar(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - terbayar can't be empty" : GoTo selesai
            End If

            'rencana(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'jmlvp(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - jmlvp can't be empty" : GoTo selesai
            End If

            'jmlvpvalas(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlvpvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idvppdetail~idvpp~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~jmlvp~jmlvpvalas~statusvp~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'sumber(2) As String            , idtransaksi(3) As Integer
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "RI"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                    updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                Case "AP"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                    updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                Case "PRT"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                    updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idvppcarabayar(0) As Integer, idvpp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'jmlvp(15) As Double, jmlvpvalas(16) As Double, statusvp(17) As Integer, isclose(18) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, jmlvp, jmlvpvalas, statusvp, isclose

        'Buat datatable PAY
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idvppcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "jmlvp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jmlvpvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "statusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)

        'CEK PARAMETER DATA PAY
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowPay.Length <> 19) Then
                    result(2) = "Pay Row : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idvppcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvppcarabayar required numeric." : GoTo selesai
                End If
                'idvpp(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvpp required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Pay Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Pay Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Pay Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Pay Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'jmlvp(15) As Double
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Pay Row : " & i & " - jmlvp required numeric." : GoTo selesai
                End If
                'jmlvpvalas(16) As Double
                If (IsNumeric(dataRowPay(16)) = False) Then
                    result(2) = "Pay Row : " & i & " - jmlvpvalas required numeric." : GoTo selesai
                End If
                'statusvp(17) As Integer
                If (IsNumeric(dataRowPay(17)) = False) Then
                    result(2) = "Pay Row : " & i & " - statusvp required numeric." : GoTo selesai
                End If
                'isclose(18) As Integer
                If (IsNumeric(dataRowPay(18)) = False) Then
                    result(2) = "Pay Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Pay Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Pay Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Pay Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) <= 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Pay Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'jmlvp(15) As Double
                If Len(dataRowPay(15)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jmlvp can't be empty" : GoTo selesai
                End If

                'jmlvpvalas(16) As Double
                If Len(dataRowPay(16)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jmlvpvalas can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Pay Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Pay Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Pay Row : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Pay Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Pay Row : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Pay Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Pay Row : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Pay Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Pay Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Pay Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idvppcarabayar~idvpp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~jmlvp~jmlvpvalas~statusvp~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15) & "~" & dataRowPay(16) & "~" & dataRowPay(17) & "~" & dataRowPay(18)) = False Then
                    result(2) = "Pay Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA PAY ===========================================

        End If


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
                Dim vModuleId As Integer = 4, vMenuId As Integer = 14
                Select Case drutama("vppstatus")
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


                ''CEK TOTAL UTAMA DAN BAYAR ==============================
                'Dim jumlah As Double = AsDataTableDSum(dtpay, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtpay, "jumlahvalas")
                'If Double.Parse(drutama("vppbayar")) <> jumlah Then
                '    Dim selisih(2) As String
                '    selisih = F_Nominal(Double.Parse(drutama("vppbayar")) - jumlah, False).Split(sptSubParam)
                '    result(2) = "Total amount of pay is not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                '    'ElseIf drutama("vppbayarvalas") <> jumlahvalas Then
                '    '    result(2) = "Total amount of foreign pay is not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN BAYAR =======================


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("vpptgl")), AsFormatTanggal(drutama("vpptgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "vppmatauang", "vpprekselisihkurs~vpprekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK MATAUANG COA =======================================
                'PAY
                rsCekCoa = ValidasiMatauangCOA(dtutama, "vppmatauang", "", dtpay, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'VALIDASI SIMPAN ========================================
                If drutama("vppstatus") = 2 Or drutama("vppstatus") = 1 Or drutama("vppstatus") = 8 Or drutama("vppstatus") = 9 Or drutama("vppstatus") = 10 Or drutama("vppstatus") = 11 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, updFilterRI, updFilterAP, updFilterPRT)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("vppid")
                    notransaksi = drutama("vppnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(vppid), vppnotransaksi FROM M4_vpp WHERE vppid='" & result(4) & "' AND vppstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("vppautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vppcabang"), drutama("vpplokasi"), drutama("vppsumber"), drutama("vpptgl"), drutama("vppsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(vppid) FROM M4_vpp WHERE vppnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_vpp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Vpp_HistorySimpan("" & paramSplit(0) & "★M4_Vpp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("vppsumber")) & "▼" & FixQuotes(drutama("vppid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Vpp set vppcabang  = '" & FixQuotes(drutama("vppcabang")) & "', vpplokasi  = '" & FixQuotes(drutama("vpplokasi")) & "', vppgudang  = '" & FixQuotes(drutama("vppgudang")) & "', vppsumber  = '" & FixQuotes(drutama("vppsumber")) & "', vppautonotransaksi  = " & drutama("vppautonotransaksi") & ", vppnotransaksi  = '" & FixQuotes(notransaksi) & "', vpptgl  = '" & FixQuotes(AsFormatTanggal(drutama("vpptgl"))) & "', vppkodepa  = " & drutama("vppkodepa") & ", vppsupplier  = " & drutama("vppsupplier") & ", vppsupplierkontak  = '" & FixQuotes(drutama("vppsupplierkontak")) & "', vpp1alamat1  = '" & FixQuotes(drutama("vpp1alamat1")) & "', vpp1alamat2  = '" & FixQuotes(drutama("vpp1alamat2")) & "', vpp1alamat3  = '" & FixQuotes(drutama("vpp1alamat3")) & "', vpp2alamat1  = '" & FixQuotes(drutama("vpp2alamat1")) & "', vpp2alamat2  = '" & FixQuotes(drutama("vpp2alamat2")) & "', vpp2alamat3  = '" & FixQuotes(drutama("vpp2alamat3")) & "', vppbagianpembayaran  = " & drutama("vppbagianpembayaran") & ", vppuraian  = '" & FixQuotes(drutama("vppuraian")) & "', vppcatatan  = '" & FixQuotes(drutama("vppcatatan")) & "', vppnoref  = '" & FixQuotes(drutama("vppnoref")) & "', vpptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("vpptglnoref"))) & "', vppcarabayar  = " & drutama("vppcarabayar") & ", vpptglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("vpptglbayar"))) & "', vppmatauang  = '" & FixQuotes(drutama("vppmatauang")) & "', vppkurs  = '" & FixDouble(drutama("vppkurs")) & "', vpptotalap  = '" & FixDouble(drutama("vpptotalap")) & "', vpptotalapvalas  = '" & FixDouble(drutama("vpptotalapvalas")) & "', vpptotalar  = '" & FixDouble(drutama("vpptotalar")) & "', vpptotalarvalas  = '" & FixDouble(drutama("vpptotalarvalas")) & "', vppbayar  = '" & FixDouble(drutama("vppbayar")) & "', vppbayarvalas  = '" & FixDouble(drutama("vppbayarvalas")) & "', vppselisihkurs  = '" & FixDouble(drutama("vppselisihkurs")) & "', vpprekselisihkurs  = '" & FixQuotes(drutama("vpprekselisihkurs")) & "', vppdiskontermin  = '" & FixDouble(drutama("vppdiskontermin")) & "', vppdiskonterminvalas  = '" & FixDouble(drutama("vppdiskonterminvalas")) & "', vpprekdiskontermin  = '" & FixQuotes(drutama("vpprekdiskontermin")) & "', vppstatusvp  = " & drutama("vppstatusvp") & ", vppstatus  = " & drutama("vppstatus") & ", vppstatussebelumnya  = " & drutama("vppstatussebelumnya") & ", vppjmlrevisi  = vppjmlrevisi+1, vppcetakanke  = " & drutama("vppcetakanke") & ", vppmodifikasiuser  = " & drutama("vppmodifikasiuser") & ", vppmodifikasitgl  = NOW(), vppcustomtext1  = '" & FixQuotes(drutama("vppcustomtext1")) & "', vppcustomtext2  = '" & FixQuotes(drutama("vppcustomtext2")) & "', vppcustomtext3  = '" & FixQuotes(drutama("vppcustomtext3")) & "', vppcustomtext4  = '" & FixQuotes(drutama("vppcustomtext4")) & "', vppcustomtext5  = '" & FixQuotes(drutama("vppcustomtext5")) & "', vppcustomint1  = " & drutama("vppcustomint1") & ", vppcustomint2  = " & drutama("vppcustomint2") & ", vppcustomint3  = " & drutama("vppcustomint3") & ", vppcustomdbl1  = '" & FixDouble(drutama("vppcustomdbl1")) & "', vppcustomdbl2  = '" & FixDouble(drutama("vppcustomdbl2")) & "', vppcustomdbl3  = '" & FixDouble(drutama("vppcustomdbl3")) & "', vppcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate1"))) & "', vppcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate2"))) & "', vppcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate3"))) & "' where vppid = '" & drutama("vppid") & "'"
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

                    If drutama("vppautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vppcabang"), drutama("vpplokasi"), drutama("vppsumber"), drutama("vpptgl"), drutama("vppsumber"), 4)
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
                        notransaksi = drutama("vppnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(vppid) FROM m4_vpp WHERE vppnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Vpp (vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppisclose, vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3) values('" & FixQuotes(drutama("vppcabang")) & "', '" & FixQuotes(drutama("vpplokasi")) & "', '" & FixQuotes(drutama("vppgudang")) & "', '" & FixQuotes(drutama("vppsumber")) & "', " & drutama("vppautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpptgl"))) & "', " & drutama("vppkodepa") & ", " & drutama("vppsupplier") & ", '" & FixQuotes(drutama("vppsupplierkontak")) & "', '" & FixQuotes(drutama("vpp1alamat1")) & "', '" & FixQuotes(drutama("vpp1alamat2")) & "', '" & FixQuotes(drutama("vpp1alamat3")) & "', '" & FixQuotes(drutama("vpp2alamat1")) & "', '" & FixQuotes(drutama("vpp2alamat2")) & "', '" & FixQuotes(drutama("vpp2alamat3")) & "', " & drutama("vppbagianpembayaran") & ", '" & FixQuotes(drutama("vppuraian")) & "', '" & FixQuotes(drutama("vppcatatan")) & "', '" & FixQuotes(drutama("vppnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpptglnoref"))) & "', " & drutama("vppcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("vpptglbayar"))) & "', '" & FixQuotes(drutama("vppmatauang")) & "', '" & FixDouble(drutama("vppkurs")) & "', '" & FixDouble(drutama("vpptotalap")) & "', '" & FixDouble(drutama("vpptotalapvalas")) & "', '" & FixDouble(drutama("vpptotalar")) & "', '" & FixDouble(drutama("vpptotalarvalas")) & "', '" & FixDouble(drutama("vppbayar")) & "', '" & FixDouble(drutama("vppbayarvalas")) & "', '" & FixDouble(drutama("vppselisihkurs")) & "', '" & FixQuotes(drutama("vpprekselisihkurs")) & "', '" & FixDouble(drutama("vppdiskontermin")) & "', '" & FixDouble(drutama("vppdiskonterminvalas")) & "', '" & FixQuotes(drutama("vpprekdiskontermin")) & "', " & drutama("vppstatusvp") & ", " & drutama("vppstatus") & ", " & drutama("vppstatussebelumnya") & ", " & drutama("vppjmlrevisi") & ", " & drutama("vppcetakanke") & ", " & drutama("vppinputuser") & ", NOW(), " & drutama("vppmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("vppisclose") & ", '" & FixQuotes(drutama("vppcustomtext1")) & "', '" & FixQuotes(drutama("vppcustomtext2")) & "', '" & FixQuotes(drutama("vppcustomtext3")) & "', '" & FixQuotes(drutama("vppcustomtext4")) & "', '" & FixQuotes(drutama("vppcustomtext5")) & "', " & drutama("vppcustomint1") & ", " & drutama("vppcustomint2") & ", " & drutama("vppcustomint3") & ", '" & FixDouble(drutama("vppcustomdbl1")) & "', '" & FixDouble(drutama("vppcustomdbl2")) & "', '" & FixDouble(drutama("vppcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select vppid from M4_vpp where vppnotransaksi='" & notransaksi & "' AND vppinputuser= '" & userid & "' order by vppmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vpp_Detail where idvpp = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvppdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(dr1("jmlvp")) & "', '" & FixDouble(dr1("jmlvpvalas")) & "', " & dr1("statusvp") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Vpp_Detail(idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vpp_Pay where idvpp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvppcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlvp")) & "', '" & FixDouble(dr1("jmlvpvalas")) & "', " & dr1("statusvp") & ", " & dr1("isclose") & ")")
                    Next
                    sql = "Insert into M4_Vpp_Pay(idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, jmlvp, jmlvpvalas, statusvp, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("vppstatus") = 2 Then
                    'UPDATE STATUSVPP
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    'RI
                    If Len(updFilterRI) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri SET ri.ristatusvpp = 1 WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'AP
                    If Len(updFilterAP) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ap ap SET ap.apstatusvpp = 1 WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'PRT
                    If Len(updFilterPRT) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_prt prt SET prt.prtstatusvpp = 1 WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "VPP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_VppUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vpp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Vpptgl, Vppnotransaksi, Vppstatus FROM M4_Vpp WHERE Vppid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Vppstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m4_vpp_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Vpp_HistorySimpan("" & paramSplit(0) & "★M4_Vpp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_vpp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, CA
                Dim updFilterRI As String = "", updFilterAP As String = "", updFilterPRT As String = ""
                Dim idtransaksiDetail As Integer = 0, sumberDetail As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT sumber, idtransaksi FROM M4_vpp_detail WHERE idvpp = '" & idtransaksi & "'", myConn)

                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi")

                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "RI"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                            Case "AP"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                                updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                            Case "PRT"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                                updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai

                End If


                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'RI
                If Len(updFilterRI) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ri ri SET ri.ristatusvpp = 0 WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'AP
                If Len(updFilterAP) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ap ap SET ap.apstatusvpp = 0 WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'PRT
                If Len(updFilterPRT) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_prt prt SET prt.prtstatusvpp = 0 WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'UPDATE TRANSAKSI PEMBAYARAN ========================================================

            End If

            'update status utama
            sql = "UPDATE M4_Vpp SET Vppstatus = " & nilaiStatus & ", Vppmodifikasiuser='" & userid & "', Vppmodifikasitgl = NOW(), Vppposting = 0, Vpppostingtgl = '1971-01-01 00:00:00', Vppjmlrevisi = Vppjmlrevisi + 1 WHERE Vppid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VppSearch(PostWsSearch(paramSplit(0), "M4_VppSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VppDelete(ByVal param As String) As String

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vpp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Vppid, Vppnotransaksi FROM M4_Vpp WHERE Vppid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT vppcabang, vpplokasi, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl"
            sql &= " FROM M4_vpp"
            sql &= " WHERE vppid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("vppcabang")
                lokasi = dtNomorNext.Rows(0)("vpplokasi")
                sumber = dtNomorNext.Rows(0)("vppsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("vppautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("vppnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("vpptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PAY
            sql = "DELETE FROM M4_Vpp_Pay WHERE idvpp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Vpp_Detail WHERE idvpp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Vpp WHERE vppid='" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 4)
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
            Dim paramSearch As String = M4_VppSearch(PostWsSearch(paramSplit(0), "M4_VppSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VppGetdataByIdSerenity(ByVal param As String) As String

        'M4_VppGetdataById Utama --------------------------------------------------------
        'vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, 
        'vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, 
        'vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, 
        'vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, 
        'vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama, kpkp

        'M4_VppGetdataById Detail -------------------------------------------------------
        'idvppdetail, idvpp, 
        'sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, 
        'jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, 
        'costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, 
        'termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, 
        'haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, 
        'inputtgl

        'M4_VppGetdataById Pay -------------------------------------------------------
        'idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, 
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

        Dim utama As String = "", detail As String = "", detailRI As String = "", detailPRT As String = "", detailAP As String = "", detailCOA As String = "", pay As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vpp~M4_Vpp_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "vppid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vppid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vppid"), 0), sptField,
                     FxDB(drutama("vppcabang"), ""), sptField,
                     FxDB(drutama("vpplokasi"), ""), sptField,
                     FxDB(drutama("vppgudang"), ""), sptField,
                     FxDB(drutama("vppsumber"), ""), sptField,
                     FxDB(drutama("vppautonotransaksi"), 0), sptField,
                     FxDB(drutama("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vppkodepa"), 0), sptField,
                     FxDB(drutama("vppsupplier"), 0), sptField,
                     FxDB(drutama("vppsupplierkontak"), ""), sptField,
                     FxDB(drutama("vpp1alamat1"), ""), sptField,
                     FxDB(drutama("vpp1alamat2"), ""), sptField,
                     FxDB(drutama("vpp1alamat3"), ""), sptField,
                     FxDB(drutama("vpp2alamat1"), ""), sptField,
                     FxDB(drutama("vpp2alamat2"), ""), sptField,
                     FxDB(drutama("vpp2alamat3"), ""), sptField,
                     FxDB(drutama("vppbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vppuraian"), ""), sptField,
                     FxDB(drutama("vppcatatan"), ""), sptField,
                     FxDB(drutama("vppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vppmatauang"), ""), sptField,
                     FxDB(drutama("vppkurs"), 0), sptField,
                     FxDB(drutama("vpptotalap"), 0), sptField,
                     FxDB(drutama("vpptotalapvalas"), 0), sptField,
                     FxDB(drutama("vpptotalar"), 0), sptField,
                     FxDB(drutama("vpptotalarvalas"), 0), sptField,
                     FxDB(drutama("vppbayar"), 0), sptField,
                     FxDB(drutama("vppbayarvalas"), 0), sptField,
                     FxDB(drutama("vppselisihkurs"), 0), sptField,
                     FxDB(drutama("vpprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vppdiskontermin"), 0), sptField,
                     FxDB(drutama("vppdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vpprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vppstatusvp"), 0), sptField,
                     FxDB(drutama("vppstatus"), 0), sptField,
                     FxDB(drutama("vppstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vppjmlrevisi"), 0), sptField,
                     FxDB(drutama("vppcetakanke"), 0), sptField,
                     FxDB(drutama("vppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppisclose"), 0), sptField,
                     FxDB(drutama("vppcustomtext1"), ""), sptField,
                     FxDB(drutama("vppcustomtext2"), ""), sptField,
                     FxDB(drutama("vppcustomtext3"), ""), sptField,
                     FxDB(drutama("vppcustomtext4"), ""), sptField,
                     FxDB(drutama("vppcustomtext5"), ""), sptField,
                     FxDB(drutama("vppcustomint1"), 0), sptField,
                     FxDB(drutama("vppcustomint2"), 0), sptField,
                     FxDB(drutama("vppcustomint3"), 0), sptField,
                     FxDB(drutama("vppcustomdbl1"), 0), sptField,
                     FxDB(drutama("vppcustomdbl2"), 0), sptField,
                     FxDB(drutama("vppcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcabangnama"), ""), sptField,
                     FxDB(drutama("vpplokasinama"), ""), sptField,
                     FxDB(drutama("vppgudangnama"), ""), sptField,
                     FxDB(drutama("vppsupplierkode"), ""), sptField,
                     FxDB(drutama("vppsuppliernama"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vppcarabayarnama"), ""), sptField,
                     FxDB(drutama("vpprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vpprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vppstatusnama"), ""), sptField,
                     FxDB(drutama("vppstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vppinputusernama"), ""), sptField,
                     FxDB(drutama("vppmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                Dim sumberdetail As String = FxDB(dr("sumber"), "")

                Select Case sumberdetail
                    Case "RI"
                        detailRI = String.Concat(detailRI, FxDB(dr("idvppdetail"), 0), sptField,
                         FxDB(dr("idvpp"), 0), sptField,
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
                         FxDB(dr("rekhutangpiutang"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("jmlvp"), 0), sptField,
                         FxDB(dr("jmlvpvalas"), 0), sptField,
                         FxDB(dr("statusvp"), 0), sptField,
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
                         FxDB(dr("notransaksivpp"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "PRT"
                        detailPRT = String.Concat(detailPRT, FxDB(dr("idvppdetail"), 0), sptField,
                         FxDB(dr("idvpp"), 0), sptField,
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
                         FxDB(dr("rekhutangpiutang"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("jmlvp"), 0), sptField,
                         FxDB(dr("jmlvpvalas"), 0), sptField,
                         FxDB(dr("statusvp"), 0), sptField,
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
                         FxDB(dr("notransaksivpp"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "AP"
                        detailAP = String.Concat(detailAP, FxDB(dr("idvppdetail"), 0), sptField,
                         FxDB(dr("idvpp"), 0), sptField,
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
                         FxDB(dr("rekhutangpiutang"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("jmlvp"), 0), sptField,
                         FxDB(dr("jmlvpvalas"), 0), sptField,
                         FxDB(dr("statusvp"), 0), sptField,
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
                         FxDB(dr("notransaksivpp"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                    Case "CA"
                        detailCOA = String.Concat(detailCOA, FxDB(dr("idvppdetail"), 0), sptField,
                         FxDB(dr("idvpp"), 0), sptField,
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
                         FxDB(dr("rekhutangpiutang"), ""), sptField,
                         FxDB(dr("catatan"), ""), sptField,
                         FxDB(dr("costcenter"), ""), sptField,
                         FxDB(dr("divisi"), ""), sptField,
                         FxDB(dr("subdivisi"), ""), sptField,
                         FxDB(dr("proyek"), ""), sptField,
                         FxDB(dr("jmlvp"), 0), sptField,
                         FxDB(dr("jmlvpvalas"), 0), sptField,
                         FxDB(dr("statusvp"), 0), sptField,
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
                         FxDB(dr("notransaksivpp"), ""), sptField,
                         AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
                End Select
            Next
            If detailRI.Length > 0 Then detailRI = detailRI.Substring(0, detailRI.Length - sptRow.Length) Else detailRI = detailRI
            If detailPRT.Length > 0 Then detailPRT = detailPRT.Substring(0, detailPRT.Length - sptRow.Length) Else detailPRT = detailPRT
            If detailAP.Length > 0 Then detailAP = detailAP.Substring(0, detailAP.Length - sptRow.Length) Else detailAP = detailAP
            If detailCOA.Length > 0 Then detailCOA = detailCOA.Substring(0, detailCOA.Length - sptRow.Length) Else detailCOA = detailCOA

            'AMBIL DATA PAY
            sql = query.PanggilQuery("m4_vpp_getdata_pay")
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-m4_vpp_getdata_pay", "idvpp='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idvppcarabayar"), 0), sptField,
                     FxDB(dr("idvpp"), 0), sptField,
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
                     FxDB(dr("jmlvp"), 0), sptField,
                     FxDB(dr("jmlvpvalas"), 0), sptField,
                     FxDB(dr("statusvp"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

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
        'strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        strResultData = String.Concat(utama, sptSubParam, detailRI, sptSubParam, detailPRT, sptSubParam, pay, sptSubParam, detailAP, sptSubParam, detailCOA)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama, kpkp" &
                                                                    sptSubParam & "idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl" &
                                                                    sptSubParam & "idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl" &
                                                                    sptSubParam & "idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, rekbanknama, rekgironama" &
                                                                    sptSubParam & "idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl" &
                                                                    sptSubParam & "idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_VppGetdataById(ByVal param As String) As String

        'M4_VppGetdataById Utama --------------------------------------------------------
        'vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, 
        'vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, 
        'vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, 
        'vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, 
        'vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama, kpkp

        'M4_VppGetdataById Detail -------------------------------------------------------
        'idvppdetail, idvpp, 
        'sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, 
        'jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, 
        'costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, 
        'urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, 
        'termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, 
        'haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, 
        'inputtgl

        'M4_VppGetdataById Pay -------------------------------------------------------
        'idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, 
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

        Dim utama As String = "", detail As String = "", pay As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vpp~M4_Vpp_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "vppid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "vppid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_getdata")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("vppid"), 0), sptField,
                     FxDB(drutama("vppcabang"), ""), sptField,
                     FxDB(drutama("vpplokasi"), ""), sptField,
                     FxDB(drutama("vppgudang"), ""), sptField,
                     FxDB(drutama("vppsumber"), ""), sptField,
                     FxDB(drutama("vppautonotransaksi"), 0), sptField,
                     FxDB(drutama("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptgl"), ""), formatTgl), sptField,
                     FxDB(drutama("vppkodepa"), 0), sptField,
                     FxDB(drutama("vppsupplier"), 0), sptField,
                     FxDB(drutama("vppsupplierkontak"), ""), sptField,
                     FxDB(drutama("vpp1alamat1"), ""), sptField,
                     FxDB(drutama("vpp1alamat2"), ""), sptField,
                     FxDB(drutama("vpp1alamat3"), ""), sptField,
                     FxDB(drutama("vpp2alamat1"), ""), sptField,
                     FxDB(drutama("vpp2alamat2"), ""), sptField,
                     FxDB(drutama("vpp2alamat3"), ""), sptField,
                     FxDB(drutama("vppbagianpembayaran"), 0), sptField,
                     FxDB(drutama("vppuraian"), ""), sptField,
                     FxDB(drutama("vppcatatan"), ""), sptField,
                     FxDB(drutama("vppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpptglbayar"), ""), formatTgl), sptField,
                     FxDB(drutama("vppmatauang"), ""), sptField,
                     FxDB(drutama("vppkurs"), 0), sptField,
                     FxDB(drutama("vpptotalap"), 0), sptField,
                     FxDB(drutama("vpptotalapvalas"), 0), sptField,
                     FxDB(drutama("vpptotalar"), 0), sptField,
                     FxDB(drutama("vpptotalarvalas"), 0), sptField,
                     FxDB(drutama("vppbayar"), 0), sptField,
                     FxDB(drutama("vppbayarvalas"), 0), sptField,
                     FxDB(drutama("vppselisihkurs"), 0), sptField,
                     FxDB(drutama("vpprekselisihkurs"), ""), sptField,
                     FxDB(drutama("vppdiskontermin"), 0), sptField,
                     FxDB(drutama("vppdiskonterminvalas"), 0), sptField,
                     FxDB(drutama("vpprekdiskontermin"), ""), sptField,
                     FxDB(drutama("vppstatusvp"), 0), sptField,
                     FxDB(drutama("vppstatus"), 0), sptField,
                     FxDB(drutama("vppstatussebelumnya"), 0), sptField,
                     FxDB(drutama("vppjmlrevisi"), 0), sptField,
                     FxDB(drutama("vppcetakanke"), 0), sptField,
                     FxDB(drutama("vppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vpppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("vppisclose"), 0), sptField,
                     FxDB(drutama("vppcustomtext1"), ""), sptField,
                     FxDB(drutama("vppcustomtext2"), ""), sptField,
                     FxDB(drutama("vppcustomtext3"), ""), sptField,
                     FxDB(drutama("vppcustomtext4"), ""), sptField,
                     FxDB(drutama("vppcustomtext5"), ""), sptField,
                     FxDB(drutama("vppcustomint1"), 0), sptField,
                     FxDB(drutama("vppcustomint2"), 0), sptField,
                     FxDB(drutama("vppcustomint3"), 0), sptField,
                     FxDB(drutama("vppcustomdbl1"), 0), sptField,
                     FxDB(drutama("vppcustomdbl2"), 0), sptField,
                     FxDB(drutama("vppcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("vppcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("vppcabangnama"), ""), sptField,
                     FxDB(drutama("vpplokasinama"), ""), sptField,
                     FxDB(drutama("vppgudangnama"), ""), sptField,
                     FxDB(drutama("vppsupplierkode"), ""), sptField,
                     FxDB(drutama("vppsuppliernama"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarankode"), ""), sptField,
                     FxDB(drutama("vppbagianpembayarannama"), ""), sptField,
                     FxDB(drutama("vppcarabayarnama"), ""), sptField,
                     FxDB(drutama("vpprekselisihkursnama"), ""), sptField,
                     FxDB(drutama("vpprekdiskonterminnama"), ""), sptField,
                     FxDB(drutama("vppstatusnama"), ""), sptField,
                     FxDB(drutama("vppstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("vppinputusernama"), ""), sptField,
                     FxDB(drutama("vppmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idvppdetail"), 0), sptField,
                     FxDB(dr("idvpp"), 0), sptField,
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
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("jmlvp"), 0), sptField,
                     FxDB(dr("jmlvpvalas"), 0), sptField,
                     FxDB(dr("statusvp"), 0), sptField,
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
                     FxDB(dr("notransaksivpp"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA PAY
            sql = query.PanggilQuery("m4_vpp_getdata_pay")
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-m4_vpp_getdata_pay", "idvpp='" & idtransaksi & "'", "urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idvppcarabayar"), 0), sptField,
                     FxDB(dr("idvpp"), 0), sptField,
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
                     FxDB(dr("jmlvp"), 0), sptField,
                     FxDB(dr("jmlvpvalas"), 0), sptField,
                     FxDB(dr("statusvp"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("carabayarnama"), ""), sptField,
                     FxDB(dr("banknama"), ""), sptField,
                     FxDB(dr("rekbanknama"), ""), sptField,
                     FxDB(dr("rekgironama"), ""), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, pay)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3, vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama, kpkp" & sptSubParam & "idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, notransaksi, tgl, carabayar, termin, tgljatuhtempo, rencana, statuslunas, diskon1, haridiskon1, diskon2, haridiskon2, rekhutangpiutangnama, costcenternama, divisinama, subdivisinama, proyeknama, notransaksivpp, inputtgl" & sptSubParam & "idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, jmlvp, jmlvpvalas, statusvp, isclose, carabayarnama, banknama, rekbanknama, rekgironama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VppSearch(ByVal param As String) As String
        'M4_VppSearch --------------------------------------------------------
        'vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, 
        'vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, 
        'vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama

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
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Vpp", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("vppid"), 0), sptField,
                     FxDB(dr("vppcabang"), ""), sptField,
                     FxDB(dr("vpplokasi"), ""), sptField,
                     FxDB(dr("vppgudang"), ""), sptField,
                     FxDB(dr("vppsumber"), ""), sptField,
                     FxDB(dr("vppautonotransaksi"), 0), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vpptgl"), ""), formatTgl), sptField,
                     FxDB(dr("vppkodepa"), 0), sptField,
                     FxDB(dr("vppsupplier"), 0), sptField,
                     FxDB(dr("vppsupplierkontak"), ""), sptField,
                     FxDB(dr("vpp1alamat1"), ""), sptField,
                     FxDB(dr("vpp1alamat2"), ""), sptField,
                     FxDB(dr("vpp1alamat3"), ""), sptField,
                     FxDB(dr("vpp2alamat1"), ""), sptField,
                     FxDB(dr("vpp2alamat2"), ""), sptField,
                     FxDB(dr("vpp2alamat3"), ""), sptField,
                     FxDB(dr("vppbagianpembayaran"), 0), sptField,
                     FxDB(dr("vppuraian"), ""), sptField,
                     FxDB(dr("vppcatatan"), ""), sptField,
                     FxDB(dr("vppnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("vpptglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("vppcarabayar"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpptglbayar"), ""), formatTgl), sptField,
                     FxDB(dr("vppmatauang"), ""), sptField,
                     FxDB(dr("vppkurs"), 0), sptField,
                     FxDB(dr("vpptotalap"), 0), sptField,
                     FxDB(dr("vpptotalapvalas"), 0), sptField,
                     FxDB(dr("vpptotalar"), 0), sptField,
                     FxDB(dr("vpptotalarvalas"), 0), sptField,
                     FxDB(dr("vppbayar"), 0), sptField,
                     FxDB(dr("vppbayarvalas"), 0), sptField,
                     FxDB(dr("vppselisihkurs"), 0), sptField,
                     FxDB(dr("vpprekselisihkurs"), ""), sptField,
                     FxDB(dr("vppdiskontermin"), 0), sptField,
                     FxDB(dr("vppdiskonterminvalas"), 0), sptField,
                     FxDB(dr("vpprekdiskontermin"), ""), sptField,
                     FxDB(dr("vppstatusvp"), 0), sptField,
                     FxDB(dr("vppstatus"), 0), sptField,
                     FxDB(dr("vppstatussebelumnya"), 0), sptField,
                     FxDB(dr("vppjmlrevisi"), 0), sptField,
                     FxDB(dr("vppcetakanke"), 0), sptField,
                     FxDB(dr("vppinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vppmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("vpppostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("vppisclose"), 0), sptField,
                     FxDB(dr("vppcabangnama"), ""), sptField,
                     FxDB(dr("vpplokasinama"), ""), sptField,
                     FxDB(dr("vppgudangnama"), ""), sptField,
                     FxDB(dr("vppsupplierkode"), ""), sptField,
                     FxDB(dr("vppsuppliernama"), ""), sptField,
                     FxDB(dr("vppbagianpembayarankode"), ""), sptField,
                     FxDB(dr("vppbagianpembayarannama"), ""), sptField,
                     FxDB(dr("vppcarabayarnama"), ""), sptField,
                     FxDB(dr("vpprekselisihkursnama"), ""), sptField,
                     FxDB(dr("vpprekdiskonterminnama"), ""), sptField,
                     FxDB(dr("vppstatusnama"), ""), sptField,
                     FxDB(dr("vppstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("vppinputusernama"), ""), sptField,
                     FxDB(dr("vppmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppposting, vpppostingtgl, vppisclose, vppcabangnama, vpplokasinama, vppgudangnama, vppsupplierkode, vppsuppliernama, vppbagianpembayarankode, vppbagianpembayarannama, vppcarabayarnama, vpprekselisihkursnama, vpprekdiskonterminnama, vppstatusnama, vppstatussebelumnyanama, vppinputusernama, vppmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VppTakedataSearch(ByVal param As String) As String
        'M4_VppTakedataSearch --------------------------------------------------------
        'idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, 
        'termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, 
        'sisa, sisavalas, statuslunas, rekhutangpiutang, diskon1, haridiskon1, diskon2, 
        'haridiskon2, inputtgl, statusvpp, noref

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

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m4_vpp_takedata(Filter)
        sql = m4_vpp_takedata(Filter)
        'result(2) = sql : GoTo selesai
        dt = AmbilData("aplikasi1-M5_Ic_Takedata", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("carabayar"), 0), sptField,
                     FxDB(dr("termin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("rencana"), 0), sptField,
                     FxDB(dr("sisa"), 0), sptField,
                     FxDB(dr("sisavalas"), 0), sptField,
                     FxDB(dr("statuslunas"), 0), sptField,
                     FxDB(dr("rekhutangpiutang"), ""), sptField,
                     FxDB(dr("diskon1"), 0), sptField,
                     FxDB(dr("haridiskon1"), 0), sptField,
                     FxDB(dr("diskon2"), 0), sptField,
                     FxDB(dr("haridiskon2"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("statusvpp"), 0), sptField,
                     FxDB(dr("noref"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Transaction data not found." & sql
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, sumber, notransaksi, tgl, kontak, catatan, carabayar, termin, tgljatuhtempo, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, sisavalas, statuslunas, rekhutangpiutang, diskon1, haridiskon1, diskon2, haridiskon2, inputtgl, statusvpp, noref"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_VppTerkait(ByVal param As String) As String
        'M4_VppTerkait --------------------------------------------------------
        'vppid, vppnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "dnrid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_vpp_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("vppid"), 0), sptField,
                     FxDB(dr("vppnotransaksi"), ""), sptField,
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
            result(2) = "Related VPP data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("vppid, vppnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Public Function m4_vpp_takedata(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = ""

        'Replace Filter
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1.Replace("idtransaksi", "ri.riid")
            filter1 = filter1.Replace("sumber", "ri.risumber")
            filter1 = filter1.Replace("notransaksi", "ri.rinotransaksi")
            filter1 = filter1.Replace("kontak", "ri.risupplier")
            filter1 = filter1.Replace("tgl", "ri.ritgl")
            filter1 = filter1.Replace("matauang", "ri.rimatauang")
            filter1 = filter1.Replace("statuslunas", "ri.ristatuslunas")
            filter1 = filter1.Replace("tanggaljatuhtempo", "ri.ritgljatuhtempo")
            filter1 = filter1.Replace("uraian", "ri.riuraian")
            filter1 = filter1.Replace("statusvpp", "ri.ristatusvpp")
            filter1 = filter1.Replace("noref", "ri.rinoref")

            filter2 = strFilter
            filter2 = filter2.Replace("idtransaksi", "ap.apid")
            filter2 = filter2.Replace("sumber", "ap.apsumber")
            filter2 = filter2.Replace("notransaksi", "ap.apnotransaksi")
            filter2 = filter2.Replace("kontak", "ap.apkontak")
            filter2 = filter2.Replace("tgl", "ap.aptgl")
            filter2 = filter2.Replace("matauang", "ap.apmatauang")
            filter2 = filter2.Replace("statuslunas", "ap.apstatusbayar")
            filter2 = filter2.Replace("tanggaljatuhtempo", "ap.aptgljatuhtempo")
            filter2 = filter2.Replace("uraian", "ap.apuraian")
            filter2 = filter2.Replace("statusvpp", "ap.apstatusvpp")
            filter2 = filter2.Replace("noref", "ap.apnoref")

            filter3 = strFilter
            filter3 = filter3.Replace("idtransaksi", "prt.prtid")
            filter3 = filter3.Replace("sumber", "prt.prtsumber")
            filter3 = filter3.Replace("notransaksi", "prt.prtnotransaksi")
            filter3 = filter3.Replace("kontak", "prt.prtsupplier")
            filter3 = filter3.Replace("tgl", "prt.prttgl")
            filter3 = filter3.Replace("matauang", "prt.prtmatauang")
            filter3 = filter3.Replace("statuslunas", "prt.prtstatuslunas")
            filter3 = filter3.Replace("tanggaljatuhtempo", "prt.prttgljatuhtempo")
            filter3 = filter3.Replace("uraian", "prt.prturaian")
            filter3 = filter3.Replace("statusvpp", "prt.prtstatusvpp")
            filter3 = filter3.Replace("noref", "prt.prtnoref")
        End If


        'If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        filter1 = " WHERE ri.ristatus IN(2,3,4,7) AND ri.ricarabayar = 1 AND ri.ritotaltransaksi <> 0 AND " & filter1

        'If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        filter2 = " WHERE ap.apstatus IN(2,3,4,7) AND ap.apjumlah <> 0 AND " & filter2

        'If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        filter3 = " WHERE prt.prtstatus IN(2,3,4,7) AND prt.prtjenis = 0 AND prt.prttotaltransaksi <> 0 AND " & filter3


        'RI
        'sql = "select `ri`.`riid` AS `idtransaksi`,`ri`.`risumber` AS `sumber`,`ri`.`rinotransaksi` AS `notransaksi`,`ri`.`ritgl` AS `tgl`,`ri`.`risupplier` AS `kontak`,`ri`.`ricatatan` AS `catatan`,`ri`.`ricarabayar` AS `carabayar`,`ri`.`ritermin` AS `termin`,`ri`.`ritgljatuhtempo` AS `tgljatuhtempo`,`ri`.`rimatauang` AS `matauang`,`ri`.`rikurs` AS `kurs`,`ri`.`ritotaltransaksi` AS `totaltransaksi`,`ri`.`rijmlbayar` AS `terbayar`,(sum((`vppd`.`jmlbayar` - `vppd`.`jmlvp`)) / `ri`.`rikurs`) AS `rencana`,((`ri`.`ritotaltransaksi` - `ri`.`rijmlbayar`) * `ri`.`rikurs`) AS `sisa`,(case `ri`.`rimatauang` when `s2`.`snilai` then 0 else (`ri`.`ritotaltransaksi` - `ri`.`rijmlbayar`) end) AS `sisavalas`,`ri`.`ristatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`ri`.`riinputtgl` AS `inputtgl` from ((((`m4_ri` `ri` left join `m1_terms` `tr` on((`ri`.`ritermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'HutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m4_vpp_detail` `vppd` on(((`vppd`.`sumber` = 'RI') and (`vppd`.`idtransaksi` = `ri`.`riid`) and (`vppd`.`statusvp` <> 2)))) " & filter1 & " group by `ri`.`riid` "
        'sql = "select ri.riid AS idtransaksi, ri.risumber AS sumber, ri.rinotransaksi AS notransaksi,ri.ritgl AS tgl,ri.risupplier AS kontak,ri.ricatatan AS catatan,ri.ricarabayar AS carabayar,ri.ritermin AS termin,ri.ritgljatuhtempo AS tgljatuhtempo,ri.rimatauang AS matauang,ri.rikurs AS kurs,ri.ritotaltransaksi AS totaltransaksi,ri.rijmlbayar AS terbayar,(sum((vppd.jmlbayar - vppd.jmlvp)) / ri.rikurs) AS rencana,((ri.ritotaltransaksi - ri.rijmlbayar) * ri.rikurs) AS sisa,(case ri.rimatauang when s2.snilai then 0 else (ri.ritotaltransaksi - ri.rijmlbayar) end) AS sisavalas,ri.ristatuslunas AS statuslunas,s.snilai AS rekhutangpiutang,tr.trdiskon1 AS diskon1,tr.trharidiskon1 AS haridiskon1,tr.trdiskon2 AS diskon2,tr.trharidiskon2 AS haridiskon2,ri.riinputtgl AS inputtgl, ri.ristatusvpp as statusvpp from m4_ri ri join m1_contact c on ri.risupplier = c.kid join m0_setting s on s.smodule = 0 and s.sgrup = 'akun' and (case c.kcustomint1 when 0 then s.skode = 'HutangUsaha' else s.skode = 'HutangKonsinyasi' end) join m0_setting s2 on s2.smodule = 0 and s2.sgrup = 'accounting' and s2.skode = 'MataUangFungsional' left join m1_terms tr on ri.ritermin = tr.trkode left join m4_vpp_detail vppd on vppd.sumber = 'RI' and vppd.idtransaksi = ri.riid and vppd.statusvp <> 2 " & filter1 & " group by ri.riid"
        sql = "select ri.riid AS idtransaksi, ri.risumber AS sumber, ri.rinotransaksi AS notransaksi,ri.ritgl AS tgl,ri.risupplier AS kontak,ri.ricatatan AS catatan,ri.ricarabayar AS carabayar,ri.ritermin AS termin,ri.ritgljatuhtempo AS tgljatuhtempo,ri.rimatauang AS matauang,ri.rikurs AS kurs,ri.ritotaltransaksi AS totaltransaksi,ri.rijmlbayar AS terbayar,(sum((vppd.jmlbayar - vppd.jmlvp)) / ri.rikurs) AS rencana,((ri.ritotaltransaksi - ri.rijmlbayar) * ri.rikurs) AS sisa,(case ri.rimatauang when s2.snilai then 0 else (ri.ritotaltransaksi - ri.rijmlbayar) end) AS sisavalas,ri.ristatuslunas AS statuslunas,c.krekhutang AS rekhutangpiutang,tr.trdiskon1 AS diskon1,tr.trharidiskon1 AS haridiskon1,tr.trdiskon2 AS diskon2,tr.trharidiskon2 AS haridiskon2,ri.riinputtgl AS inputtgl, ri.ristatusvpp as statusvpp, ri.rinoref as noref from m4_ri ri join m1_contact c on ri.risupplier = c.kid join m0_setting s on s.smodule = 0 and s.sgrup = 'akun' and (case c.kcustomint1 when 0 then s.skode = 'HutangUsaha' else s.skode = 'HutangKonsinyasi' end) join m0_setting s2 on s2.smodule = 0 and s2.sgrup = 'accounting' and s2.skode = 'MataUangFungsional' left join m1_terms tr on ri.ritermin = tr.trkode left join m4_vpp_detail vppd on vppd.sumber = 'RI' and vppd.idtransaksi = ri.riid and vppd.statusvp <> 2 " & filter1 & " group by ri.riid"
        'AP
        sql &= " UNION "
        sql &= "select `ap`.`apid` AS `idtransaksi`,`ap`.`apsumber` AS `sumber`,`ap`.`apnotransaksi` AS `notransaksi`,`ap`.`aptgl` AS `tgl`,`ap`.`apkontak` AS `kontak`,`ap`.`apcatatan` AS `catatan`,0 AS `carabayar`,`ap`.`aptermin` AS `termin`,`ap`.`aptgljatuhtempo` AS `tgljatuhtempo`,`ap`.`apmatauang` AS `matauang`,`ap`.`apkurs` AS `kurs`,(case `ap`.`apmatauang` when `s2`.`snilai` then `ap`.`apjumlah` else `ap`.`apjumlahvalas` end) AS `totaltransaksi`,(case `ap`.`apmatauang` when `s2`.`snilai` then `ap`.`apjumlahbayar` else `ap`.`apjumlahbayarvalas` end) AS `terbayar`,(sum((`vppd`.`jmlbayar` - `vppd`.`jmlvp`)) / `ap`.`apkurs`) AS `rencana`,(`ap`.`apjumlah` - `ap`.`apjumlahbayar`) AS `sisa`,(case `ap`.`apmatauang` when `s2`.`snilai` then 0 else (`ap`.`apjumlahvalas` - `ap`.`apjumlahbayarvalas`) end) AS `sisavalas`,`ap`.`apstatusbayar` AS `statuslunas`,`ap`.`apnorek` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`ap`.`apinputtgl` AS `inputtgl`, ap.apstatusvpp as statusvpp, ap.apnoref as noref from (((`m4_ap` `ap` left join `m1_terms` `tr` on((`ap`.`aptermin` = `tr`.`trkode`))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m4_vpp_detail` `vppd` on(((`vppd`.`sumber` = 'AP') and (`vppd`.`idtransaksi` = `ap`.`apid`) and (`vppd`.`statusvp` <> 2)))) " & filter2 & " group by `ap`.`apid` "
        'PRT
        sql &= " UNION "
        'sql &= "select `prt`.`prtid` AS `idtransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtnotransaksi` AS `notransaksi`,`prt`.`prttgl` AS `tgl`,`prt`.`prtsupplier` AS `kontak`,`prt`.`prtcatatan` AS `catatan`,`prt`.`prtcarabayar` AS `carabayar`,`prt`.`prttermin` AS `termin`,`prt`.`prttgljatuhtempo` AS `tgljatuhtempo`,`prt`.`prtmatauang` AS `matauang`,`prt`.`prtkurs` AS `kurs`,`prt`.`prttotaltransaksi` AS `totaltransaksi`,`prt`.`prtjmlbayar` AS `terbayar`,(sum((`vppd`.`jmlbayar` - `vppd`.`jmlvp`)) / `prt`.`prtkurs`) AS `rencana`,((`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) * `prt`.`prtkurs`) AS `sisa`,(case `prt`.`prtmatauang` when `s2`.`snilai` then 0 else (`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) end) AS `sisavalas`,`prt`.`prtstatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`prt`.`prtinputtgl` AS `inputtgl` from ((((`m4_prt` `prt` left join `m1_terms` `tr` on((`prt`.`prttermin` = `tr`.`trkode`))) join `m0_setting` `s` on(((`s`.`smodule` = 0) and (`s`.`sgrup` = 'akun') and (`s`.`skode` = 'HutangUsaha')))) join `m0_setting` `s2` on(((`s2`.`smodule` = 0) and (`s2`.`sgrup` = 'accounting') and (`s2`.`skode` = 'MataUangFungsional')))) left join `m4_vpp_detail` `vppd` on(((`vppd`.`sumber` = 'prt') and (`vppd`.`idtransaksi` = `prt`.`prtid`) and (`vppd`.`statusvp` <> 2)))) " & filter3 & " group by `prt`.`prtid` "
        'sql &= "select `prt`.`prtid` AS `idtransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtnotransaksi` AS `notransaksi`,`prt`.`prttgl` AS `tgl`,`prt`.`prtsupplier` AS `kontak`,`prt`.`prtcatatan` AS `catatan`,`prt`.`prtcarabayar` AS `carabayar`,`prt`.`prttermin` AS `termin`,`prt`.`prttgljatuhtempo` AS `tgljatuhtempo`,`prt`.`prtmatauang` AS `matauang`,`prt`.`prtkurs` AS `kurs`,`prt`.`prttotaltransaksi` AS `totaltransaksi`,`prt`.`prtjmlbayar` AS `terbayar`,(sum((`vppd`.`jmlbayar` - `vppd`.`jmlvp`)) / `prt`.`prtkurs`) AS `rencana`,((`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) * `prt`.`prtkurs`) AS `sisa`,(case `prt`.`prtmatauang` when `s2`.`snilai` then 0 else (`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) end) AS `sisavalas`,`prt`.`prtstatuslunas` AS `statuslunas`,`s`.`snilai` AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`prt`.`prtinputtgl` AS `inputtgl`, prt.prtstatusvpp as statusvpp from `m4_prt` `prt` join m1_contact c on prt.prtsupplier = c.kid join `m0_setting` `s` on `s`.`smodule` = 0 and `s`.`sgrup` = 'akun' and (case c.kcustomint1 when 0 then `s`.`skode` = 'HutangUsaha' else `s`.`skode` = 'HutangKonsinyasi' end) join `m0_setting` `s2` on `s2`.`smodule` = 0 and `s2`.`sgrup` = 'accounting' and `s2`.`skode` = 'MataUangFungsional' left join `m1_terms` `tr` on `prt`.`prttermin` = `tr`.`trkode` left join `m4_vpp_detail` `vppd` on `vppd`.`sumber` = 'prt' and `vppd`.`idtransaksi` = `prt`.`prtid` and `vppd`.`statusvp` <> 2 " & filter3 & " group by `prt`.`prtid`"
        sql &= "select `prt`.`prtid` AS `idtransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtnotransaksi` AS `notransaksi`,`prt`.`prttgl` AS `tgl`,`prt`.`prtsupplier` AS `kontak`,`prt`.`prtcatatan` AS `catatan`,`prt`.`prtcarabayar` AS `carabayar`,`prt`.`prttermin` AS `termin`,`prt`.`prttgljatuhtempo` AS `tgljatuhtempo`,`prt`.`prtmatauang` AS `matauang`,`prt`.`prtkurs` AS `kurs`,`prt`.`prttotaltransaksi` AS `totaltransaksi`,`prt`.`prtjmlbayar` AS `terbayar`,(sum((`vppd`.`jmlbayar` - `vppd`.`jmlvp`)) / `prt`.`prtkurs`) AS `rencana`,((`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) * `prt`.`prtkurs`) AS `sisa`,(case `prt`.`prtmatauang` when `s2`.`snilai` then 0 else (`prt`.`prttotaltransaksi` - `prt`.`prtjmlbayar`) end) AS `sisavalas`,`prt`.`prtstatuslunas` AS `statuslunas`,c.krekhutang AS `rekhutangpiutang`,`tr`.`trdiskon1` AS `diskon1`,`tr`.`trharidiskon1` AS `haridiskon1`,`tr`.`trdiskon2` AS `diskon2`,`tr`.`trharidiskon2` AS `haridiskon2`,`prt`.`prtinputtgl` AS `inputtgl`, prt.prtstatusvpp as statusvpp, prt.prtnoref as noref from `m4_prt` `prt` join m1_contact c on prt.prtsupplier = c.kid join `m0_setting` `s` on `s`.`smodule` = 0 and `s`.`sgrup` = 'akun' and (case c.kcustomint1 when 0 then `s`.`skode` = 'HutangUsaha' else `s`.`skode` = 'HutangKonsinyasi' end) join `m0_setting` `s2` on `s2`.`smodule` = 0 and `s2`.`sgrup` = 'accounting' and `s2`.`skode` = 'MataUangFungsional' left join `m1_terms` `tr` on `prt`.`prttermin` = `tr`.`trkode` left join `m4_vpp_detail` `vppd` on `vppd`.`sumber` = 'prt' and `vppd`.`idtransaksi` = `prt`.`prtid` and `vppd`.`statusvp` <> 2 " & filter3 & " group by `prt`.`prtid`"

        Return sql
    End Function

    Private Function ValidasiSimpan(ByVal dtDetail As DataTable, ByVal updFilterRI As String, ByVal updFilterAP As String, ByVal updFilterPRT As String) As String

        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, sumber As String = "", notransaksi As String = "", notransaksivpp As String = "", matauang As String = "", tgl As String = ""
        Dim filterLookup As String = "", urutan As String = "", sisa As Double = 0


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'RI
        If Len(updFilterRI) > 0 Then
            sql = "SELECT ri.riid, ri.risumber, ri.rinotransaksi, vpp.vppnotransaksi FROM m4_ri ri JOIN m4_vpp_detail vppd ON ri.risumber = vppd.sumber AND ri.riid = vppd.idtransaksi AND (" & updFilterRI & ") JOIN m4_vpp vpp ON vppd.idvpp = vpp.vppid AND vpp.vppstatus IN(2,3,4,7) GROUP BY ri.riid, vpp.vppid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("risumber")
                notransaksi = dtval.Rows(0)("rinotransaksi")
                notransaksivpp = dtval.Rows(0)("vppnotransaksi")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("riid") & "'"
                dtLookup = AsDataTableFilterLimit(dtDetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : '" & notransaksi & "' has related transactions in '" & notransaksivpp & "'" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'AP
        If Len(updFilterAP) > 0 Then
            sql = "SELECT ap.apid, ap.apsumber, ap.apnotransaksi, vpp.vppnotransaksi FROM m4_ap ap JOIN m4_vpp_detail vppd ON ap.apsumber = vppd.sumber AND ap.apid = vppd.idtransaksi AND (" & updFilterAP & ") JOIN m4_vpp vpp ON vppd.idvpp = vpp.vppid AND vpp.vppstatus IN(2,3,4,7) GROUP BY ap.apid, vpp.vppid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("apsumber")
                notransaksi = dtval.Rows(0)("apnotransaksi")
                notransaksivpp = dtval.Rows(0)("vppnotransaksi")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("apid") & "'"
                dtLookup = AsDataTableFilterLimit(dtDetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : '" & notransaksi & "' has related transactions in '" & notransaksivpp & "'" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------


        'VALIDASI TRANSAKSI PEMBAYARAN ------------------------------
        'PRT
        If Len(updFilterPRT) > 0 Then
            sql = "SELECT prt.prtid, prt.prtsumber, prt.prtnotransaksi, vpp.vppnotransaksi FROM m4_prt prt JOIN m4_vpp_detail vppd ON prt.prtsumber = vppd.sumber AND prt.prtid = vppd.idtransaksi AND (" & updFilterPRT & ") JOIN m4_vpp vpp ON vppd.idvpp = vpp.vppid AND vpp.vppstatus IN(2,3,4,7) GROUP BY prt.prtid, vpp.vppid "
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                sumber = dtval.Rows(0)("prtsumber")
                notransaksi = dtval.Rows(0)("prtnotransaksi")
                notransaksivpp = dtval.Rows(0)("vppnotransaksi")

                filterLookup = "sumber = '" & sumber & "' AND idtransaksi = '" & dtval.Rows(0)("prtid") & "'"
                dtLookup = AsDataTableFilterLimit(dtDetail, filterLookup, , , 1)
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & sumber & " : '" & notransaksi & "' has related transactions in '" & notransaksivpp & "'" : GoTo selesai
            End If
        End If
        'END OF VALIDASI TRANSAKSI PEMBAYARAN -----------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_VppSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPay(), dataRowPay() As String

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
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'vppid(0) As Integer, vppcabang(1) As String, vpplokasi(2) As String, vppgudang(3) As String, vppsumber(4) As String, 
        'vppautonotransaksi(5) As Integer, vppnotransaksi(6) As String, vpptgl(7) As Date, vppkodepa(8) As Integer, vppsupplier(9) As Integer, 
        'vppsupplierkontak(10) As String, vpp1alamat1(11) As String, vpp1alamat2(12) As String, vpp1alamat3(13) As String, vpp2alamat1(14) As String, 
        'vpp2alamat2(15) As String, vpp2alamat3(16) As String, vppbagianpembayaran(17) As Integer, vppuraian(18) As String, vppcatatan(19) As String, 
        'vppnoref(20) As String, vpptglnoref(21) As Date, vppcarabayar(22) As Integer, vpptglbayar(23) As Date, vppmatauang(24) As String, 
        'vppkurs(25) As Double, vpptotalap(26) As Double, vpptotalapvalas(27) As Double, vpptotalar(28) As Double, vpptotalarvalas(29) As Double, 
        'vppbayar(30) As Double, vppbayarvalas(31) As Double, vppselisihkurs(32) As Double, vpprekselisihkurs(33) As String, vppdiskontermin(34) As Double, 
        'vppdiskonterminvalas(35) As Double, vpprekdiskontermin(36) As String, vppstatusvp(37) As Integer, vppstatus(38) As Integer, vppstatussebelumnya(39) As Integer, 
        'vppjmlrevisi(40) As Integer, vppcetakanke(41) As Integer, vppinputuser(42) As Integer, vppinputtgl(43) As DateTime, vppmodifikasiuser(44) As Integer, 
        'vppmodifikasitgl(45) As DateTime, vppisclose(46) As Integer, vppcustomtext1(47) As String, vppcustomtext2(48) As String, vppcustomtext3(49) As String, 
        'vppcustomtext4(50) As String, vppcustomtext5(51) As String, vppcustomint1(52) As Integer, vppcustomint2(53) As Integer, vppcustomint3(54) As Integer, 
        'vppcustomdbl1(55) As Double, vppcustomdbl2(56) As Double, vppcustomdbl3(57) As Double, vppcustomdate1(58) As Date, vppcustomdate2(59) As Date, 
        'vppcustomdate3(60) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'vppid, vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, 
        'vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, 
        'vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, 
        'vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, 
        'vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, 
        'vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, 
        'vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppisclose, vppcustomtext1, vppcustomtext2, 
        'vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, 
        'vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 61) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'vppid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "vppid required numeric." : GoTo selesai
        End If
        'vppautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "vppautonotransaksi required numeric." : GoTo selesai
        End If
        'vpptgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "vpptgl required date." : GoTo selesai
        End If
        'vppkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "vppkodepa required numeric." : GoTo selesai
        End If
        'vppsupplier(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "vppsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(9) < 1) Then
            result(2) = "vppsupplier can't be empty." : GoTo selesai
        End If
        'vppbagianpembayaran(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "vppbagianpembayaran required numeric." : GoTo selesai
        End If
        'vpptglnoref(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "vpptglnoref required date." : GoTo selesai
        End If
        'vppcarabayar(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "vppcarabayar required numeric." : GoTo selesai
        End If
        'vpptglbayar(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "vpptglbayar required date." : GoTo selesai
        End If
        'vppkurs(25) As Double
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "vppkurs required numeric." : GoTo selesai
        End If
        'vpptotalap(26) As Double
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "vpptotalap required numeric." : GoTo selesai
        End If
        'vpptotalapvalas(27) As Double
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "vpptotalapvalas required numeric." : GoTo selesai
        End If
        'vpptotalar(28) As Double
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "vpptotalar required numeric." : GoTo selesai
        End If
        'vpptotalarvalas(29) As Double
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "vpptotalarvalas required numeric." : GoTo selesai
        End If
        'vppbayar(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "vppbayar required numeric." : GoTo selesai
        End If
        'vppbayarvalas(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "vppbayarvalas required numeric." : GoTo selesai
        End If
        'vppselisihkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "vppselisihkurs required numeric." : GoTo selesai
        End If
        'vppdiskontermin(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "vppdiskontermin required numeric." : GoTo selesai
        End If
        'vppdiskonterminvalas(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "vppdiskonterminvalas required numeric." : GoTo selesai
        End If
        'vppstatusvp(37) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "vppstatusvp required numeric." : GoTo selesai
        End If
        'vppstatus(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "vppstatus required numeric." : GoTo selesai
        End If
        'vppstatussebelumnya(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "vppstatussebelumnya required numeric." : GoTo selesai
        End If
        'vppjmlrevisi(40) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "vppjmlrevisi required numeric." : GoTo selesai
        End If
        'vppcetakanke(41) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "vppcetakanke required numeric." : GoTo selesai
        End If
        'vppinputuser(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "vppinputuser required numeric." : GoTo selesai
        End If
        'vppinputtgl(43) As DateTime
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "vppinputtgl required date." : GoTo selesai
        End If
        'vppmodifikasiuser(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "vppmodifikasiuser required numeric." : GoTo selesai
        End If
        'vppmodifikasitgl(45) As DateTime
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "vppmodifikasitgl required date." : GoTo selesai
        End If
        'vppisclose(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "vppisclose required numeric." : GoTo selesai
        End If
        'vppcustomint1(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "vppcustomint1 required numeric." : GoTo selesai
        End If
        'vppcustomint2(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "vppcustomint2 required numeric." : GoTo selesai
        End If
        'vppcustomint3(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "vppcustomint3 required numeric." : GoTo selesai
        End If
        'vppcustomdbl1(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "vppcustomdbl1 required numeric." : GoTo selesai
        End If
        'vppcustomdbl2(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "vppcustomdbl2 required numeric." : GoTo selesai
        End If
        'vppcustomdbl3(57) As Double
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "vppcustomdbl3 required numeric." : GoTo selesai
        End If
        'vppcustomdate1(58) As Date
        If (IsDate(dataUtama(58)) = False) Then
            result(2) = "vppcustomdate1 required date." : GoTo selesai
        End If
        'vppcustomdate2(59) As Date
        If (IsDate(dataUtama(59)) = False) Then
            result(2) = "vppcustomdate2 required date." : GoTo selesai
        End If
        'vppcustomdate3(60) As Date
        If (IsDate(dataUtama(60)) = False) Then
            result(2) = "vppcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'vppcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "vppcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "vppcabang should not be more than 25 character." : GoTo selesai
        End If

        'vpplokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "vpplokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "vpplokasi should not be more than 25 character." : GoTo selesai
        End If

        'vppsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "vppsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "vppsumber should not be more than 10 character." : GoTo selesai
        End If

        'vppnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "vppnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "vppnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'vpptgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "vpptgl can't be empty" : GoTo selesai
        End If

        'vpptglnoref(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "vpptglnoref can't be empty" : GoTo selesai
        End If

        'vpptglbayar(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "vpptglbayar can't be empty" : GoTo selesai
        End If

        'vppmatauang(24) As String
        If Len(dataUtama(24)) = 0 Then
            result(2) = "vppmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(24)) > 25 Then
            result(2) = "vppmatauang should not be more than 25 character." : GoTo selesai
        End If

        'vppkurs(25) As Double
        If Len(dataUtama(25)) = 0 Then
            result(2) = "vppkurs can't be empty" : GoTo selesai
        End If

        'vpptotalap(26) As Double
        If Len(dataUtama(26)) = 0 Then
            result(2) = "vpptotalap can't be empty" : GoTo selesai
        End If

        'vpptotalapvalas(27) As Double
        If Len(dataUtama(27)) = 0 Then
            result(2) = "vpptotalapvalas can't be empty" : GoTo selesai
        End If

        'vpptotalar(28) As Double
        If Len(dataUtama(28)) = 0 Then
            result(2) = "vpptotalar can't be empty" : GoTo selesai
        End If

        'vpptotalarvalas(29) As Double
        If Len(dataUtama(29)) = 0 Then
            result(2) = "vpptotalarvalas can't be empty" : GoTo selesai
        End If

        'vppbayar(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "vppbayar can't be empty" : GoTo selesai
        End If

        'vppbayarvalas(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "vppbayarvalas can't be empty" : GoTo selesai
        End If

        'vppselisihkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "vppselisihkurs can't be empty" : GoTo selesai
        End If

        'vppdiskontermin(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "vppdiskontermin can't be empty" : GoTo selesai
        End If

        'vppdiskonterminvalas(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "vppdiskonterminvalas can't be empty" : GoTo selesai
        End If

        'vppinputtgl(43) As DateTime
        If Len(dataUtama(43)) = 0 Then
            result(2) = "vppinputtgl can't be empty" : GoTo selesai
        End If

        'vppmodifikasitgl(45) As DateTime
        If Len(dataUtama(45)) = 0 Then
            result(2) = "vppmodifikasitgl can't be empty" : GoTo selesai
        End If

        'vppcustomdbl1(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "vppcustomdbl1 can't be empty" : GoTo selesai
        End If

        'vppcustomdbl2(56) As Double
        If Len(dataUtama(56)) = 0 Then
            result(2) = "vppcustomdbl2 can't be empty" : GoTo selesai
        End If

        'vppcustomdbl3(57) As Double
        If Len(dataUtama(57)) = 0 Then
            result(2) = "vppcustomdbl3 can't be empty" : GoTo selesai
        End If

        'vppcustomdate1(58) As Date
        If Len(dataUtama(58)) = 0 Then
            result(2) = "vppcustomdate1 can't be empty" : GoTo selesai
        End If

        'vppcustomdate2(59) As Date
        If Len(dataUtama(59)) = 0 Then
            result(2) = "vppcustomdate2 can't be empty" : GoTo selesai
        End If

        'vppcustomdate3(60) As Date
        If Len(dataUtama(60)) = 0 Then
            result(2) = "vppcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "vppid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpplokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpp2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppbagianpembayaran", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vpptglbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalap", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalapvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpptotalarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppbayar", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vppbayarvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "vppselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpprekselisihkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppdiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vpprekdiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppstatusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "vppcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "vppcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "vppid~vppcabang~vpplokasi~vppgudang~vppsumber~vppautonotransaksi~vppnotransaksi~vpptgl~vppkodepa~vppsupplier~vppsupplierkontak~vpp1alamat1~vpp1alamat2~vpp1alamat3~vpp2alamat1~vpp2alamat2~vpp2alamat3~vppbagianpembayaran~vppuraian~vppcatatan~vppnoref~vpptglnoref~vppcarabayar~vpptglbayar~vppmatauang~vppkurs~vpptotalap~vpptotalapvalas~vpptotalar~vpptotalarvalas~vppbayar~vppbayarvalas~vppselisihkurs~vpprekselisihkurs~vppdiskontermin~vppdiskonterminvalas~vpprekdiskontermin~vppstatusvp~vppstatus~vppstatussebelumnya~vppjmlrevisi~vppcetakanke~vppinputuser~vppinputtgl~vppmodifikasiuser~vppmodifikasitgl~vppisclose~vppcustomtext1~vppcustomtext2~vppcustomtext3~vppcustomtext4~vppcustomtext5~vppcustomint1~vppcustomint2~vppcustomint3~vppcustomdbl1~vppcustomdbl2~vppcustomdbl3~vppcustomdate1~vppcustomdate2~vppcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idvppdetail(0) As Integer, idvpp(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, matauang(4) As String, 
        'kurs(5) As Double, totaltransaksi(6) As Double, terbayar(7) As Double, sisa(8) As Double, jmlbayar(9) As Double, 
        'jmlbayarvalas(10) As Double, diskontermin(11) As String, jmldiskontermin(12) As Double, jmldiskonterminvalas(13) As Double, rekhutangpiutang(14) As String, 
        'catatan(15) As String, costcenter(16) As String, divisi(17) As String, subdivisi(18) As String, proyek(19) As String, 
        'jmlvp(20) As Double, jmlvpvalas(21) As Double, statusvp(22) As Integer, urutan(23) As Integer, isclose(24) As Integer, 
        'customtext1(25) As String, customtext2(26) As String, customtext3(27) As String, customdbl1(28) As Double, customdbl2(29) As Double, 
        'customdbl3(30) As Double, customdate1(31) As Date, customdate2(32) As Date, customdate3(33) As Date, rencana(34) As Double

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, 
        'terbayar, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, 
        'rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, 
        'jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rencana

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idvppdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "totaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "terbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rencana", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbayarvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "diskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskontermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskonterminvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlvp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlvpvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)

        'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT
        Dim sumberDetail As String = "", idtransaksiDetail As Double = 0
        Dim updFilterRI As String = "", updFilterAP As String = "", updFilterPRT As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 35) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idvppdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idvppdetail required numeric." : GoTo selesai
            End If
            'idvpp(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idvpp required numeric." : GoTo selesai
            End If
            'idtransaksi(3) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - idtransaksi required numeric." : GoTo selesai
            End If
            'kurs(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'totaltransaksi(6) As Double
            If (IsNumeric(dataRowDetail(6)) = False) Then
                result(2) = "Row : " & i & " - totaltransaksi required numeric." : GoTo selesai
            End If
            'terbayar(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - terbayar required numeric." : GoTo selesai
            End If
            'rencana(34) As Double
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - rencana required numeric." : GoTo selesai
            End If
            'sisa(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - sisa required numeric." : GoTo selesai
            End If
            'jmlbayar(9) As Double
            If (IsNumeric(dataRowDetail(9)) = False) Then
                result(2) = "Row : " & i & " - jmlbayar required numeric." : GoTo selesai
            End If
            'jmlbayarvalas(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - jmlbayarvalas required numeric." : GoTo selesai
            End If
            'jmldiskontermin(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskontermin required numeric." : GoTo selesai
            End If
            'jmldiskonterminvalas(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas required numeric." : GoTo selesai
            End If
            'jmlvp(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - jmlvp required numeric." : GoTo selesai
            End If
            'jmlvpvalas(21) As Double
            If (IsNumeric(dataRowDetail(21)) = False) Then
                result(2) = "Row : " & i & " - jmlvpvalas required numeric." : GoTo selesai
            End If
            'statusvp(22) As Integer
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - statusvp required numeric." : GoTo selesai
            End If
            'urutan(23) As Integer
            If (IsNumeric(dataRowDetail(23)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'isclose(24) As Integer
            If (IsNumeric(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(28) As Double
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(30) As Double
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(32) As Date
            If (IsDate(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(33) As Date
            If (IsDate(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'sumber(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - sumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 10 Then
                result(2) = "Row : " & i & " - sumber should not be more than 10 character." : GoTo selesai
            End If
            If (dataRowDetail(2) <> "RI" And dataRowDetail(2) <> "AP" And dataRowDetail(2) <> "PRT" And dataRowDetail(2) <> "CA") Then
                result(2) = "Row : " & i & " - Invalid sumber" : GoTo selesai
            End If

            'matauang(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'totaltransaksi(6) As Double
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - totaltransaksi can't be empty" : GoTo selesai
            End If

            'terbayar(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - terbayar can't be empty" : GoTo selesai
            End If

            'rencana(34) As Double
            If Len(dataRowDetail(34)) = 0 Then
                result(2) = "Row : " & i & " - rencana can't be empty" : GoTo selesai
            End If

            'sisa(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - sisa can't be empty" : GoTo selesai
            End If

            'jmlbayar(9) As Double
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayar can't be empty" : GoTo selesai
            End If

            'jmlbayarvalas(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - jmlbayarvalas can't be empty" : GoTo selesai
            End If

            'diskontermin(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskontermin can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskontermin should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskontermin(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskontermin can't be empty" : GoTo selesai
            End If

            'jmldiskonterminvalas(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskonterminvalas can't be empty" : GoTo selesai
            End If

            'rekhutangpiutang(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - rekhutangpiutang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - rekhutangpiutang should not be more than 25 character." : GoTo selesai
            End If

            'jmlvp(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - jmlvp can't be empty" : GoTo selesai
            End If

            'jmlvpvalas(21) As Double
            If Len(dataRowDetail(21)) = 0 Then
                result(2) = "Row : " & i & " - jmlvpvalas can't be empty" : GoTo selesai
            End If

            'customdbl1(28) As Double
            If Len(dataRowDetail(28)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(30) As Double
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(32) As Date
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(33) As Date
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idvppdetail~idvpp~sumber~idtransaksi~matauang~kurs~totaltransaksi~terbayar~sisa~jmlbayar~jmlbayarvalas~diskontermin~jmldiskontermin~jmldiskonterminvalas~rekhutangpiutang~catatan~costcenter~divisi~subdivisi~proyek~jmlvp~jmlvpvalas~statusvp~urutan~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rencana", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'sumber(2) As String            , idtransaksi(3) As Integer
            sumberDetail = dataRowDetail(2) : idtransaksiDetail = dataRowDetail(3)

            'VALIDASI TRANSAKSI PEMBAYARAN ----------------
            Select Case sumberDetail
                Case "RI"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                    updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                Case "AP"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                    updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                Case "PRT"
                    'SET FILTER UPDATE OUTSTANDING
                    updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                    updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

            End Select
            'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================

        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idvppcarabayar(0) As Integer, idvpp(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'jmlvp(15) As Double, jmlvpvalas(16) As Double, statusvp(17) As Integer, isclose(18) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, jmlvp, jmlvpvalas, statusvp, isclose

        'Buat datatable PAY
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idvppcarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idvpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "carabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "jumlahvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "nogiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "tgljt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "bank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "noacbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "rekgiro", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "jmlvp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "jmlvpvalas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "statusvp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)

        'CEK PARAMETER DATA PAY
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowPay.Length <> 19) Then
                    result(2) = "Pay Row : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idvppcarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvppcarabayar required numeric." : GoTo selesai
                End If
                'idvpp(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Pay Row : " & i & " - idvpp required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Pay Row : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Pay Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Pay Row : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Pay Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'jmlvp(15) As Double
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Pay Row : " & i & " - jmlvp required numeric." : GoTo selesai
                End If
                'jmlvpvalas(16) As Double
                If (IsNumeric(dataRowPay(16)) = False) Then
                    result(2) = "Pay Row : " & i & " - jmlvpvalas required numeric." : GoTo selesai
                End If
                'statusvp(17) As Integer
                If (IsNumeric(dataRowPay(17)) = False) Then
                    result(2) = "Pay Row : " & i & " - statusvp required numeric." : GoTo selesai
                End If
                'isclose(18) As Integer
                If (IsNumeric(dataRowPay(18)) = False) Then
                    result(2) = "Pay Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Pay Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Pay Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Pay Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) <= 0 Then
                    result(2) = "Pay Row : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Pay Row : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'jmlvp(15) As Double
                If Len(dataRowPay(15)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jmlvp can't be empty" : GoTo selesai
                End If

                'jmlvpvalas(16) As Double
                If Len(dataRowPay(16)) = 0 Then
                    result(2) = "Pay Row : " & i & " - jmlvpvalas can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Pay Row : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Pay Row : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Pay Row : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Pay Row : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Pay Row : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Pay Row : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Pay Row : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Pay Row : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Pay Row : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Pay Row : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idvppcarabayar~idvpp~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~jmlvp~jmlvpvalas~statusvp~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15) & "~" & dataRowPay(16) & "~" & dataRowPay(17) & "~" & dataRowPay(18)) = False Then
                    result(2) = "Pay Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA PAY ===========================================

        End If


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

                ''CEK TOTAL UTAMA DAN BAYAR ==============================
                'Dim jumlah As Double = AsDataTableDSum(dtpay, "jumlah")
                'Dim jumlahvalas As Double = AsDataTableDSum(dtpay, "jumlahvalas")
                'If Double.Parse(drutama("vppbayar")) <> jumlah Then
                '    Dim selisih(2) As String
                '    selisih = F_Nominal(Double.Parse(drutama("vppbayar")) - jumlah, False).Split(sptSubParam)
                '    result(2) = "Total amount of pay is not balanced : " & selisih(1) & "" : Trans.Rollback() : GoTo selesai
                '    'ElseIf drutama("vppbayarvalas") <> jumlahvalas Then
                '    '    result(2) = "Total amount of foreign pay is not balanced" : Trans.Rollback() : GoTo selesai
                'End If
                ''END OF CEK TOTAL UTAMA DAN BAYAR =======================


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("vpptgl")), AsFormatTanggal(drutama("vpptgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'CEK MATAUANG COA =======================================
                'DETAIL
                Dim rsCekCoa As String = ValidasiMatauangCOA(dtutama, "vppmatauang", "vpprekselisihkurs~vpprekdiskontermin", dtdetail, "rekhutangpiutang")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'CEK MATAUANG COA =======================================
                'PAY
                rsCekCoa = ValidasiMatauangCOA(dtutama, "vppmatauang", "", dtpay, "rekbank")
                If Len(rsCekCoa) <> 0 Then result(2) = rsCekCoa : Trans.Rollback() : GoTo selesai
                'END OF CEK MATAUANG COA ================================


                'VALIDASI SIMPAN ========================================
                If drutama("vppstatus") = 2 Then
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, updFilterRI, updFilterAP, updFilterPRT)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                If isUpdate Then
                    result(4) = drutama("vppid")
                    notransaksi = drutama("vppnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(vppid), vppnotransaksi FROM M4_vpp WHERE vppid='" & result(4) & "' AND vppstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(vppid) FROM M4_vpp WHERE vppnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_vpp_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Vpp_HistorySimpan("" & paramSplit(0) & "★M4_Vpp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("vppsumber")) & "▼" & FixQuotes(drutama("vppid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Vpp set vppcabang  = '" & FixQuotes(drutama("vppcabang")) & "', vpplokasi  = '" & FixQuotes(drutama("vpplokasi")) & "', vppgudang  = '" & FixQuotes(drutama("vppgudang")) & "', vppsumber  = '" & FixQuotes(drutama("vppsumber")) & "', vppautonotransaksi  = " & drutama("vppautonotransaksi") & ", vppnotransaksi  = '" & FixQuotes(notransaksi) & "', vpptgl  = '" & FixQuotes(AsFormatTanggal(drutama("vpptgl"))) & "', vppkodepa  = " & drutama("vppkodepa") & ", vppsupplier  = " & drutama("vppsupplier") & ", vppsupplierkontak  = '" & FixQuotes(drutama("vppsupplierkontak")) & "', vpp1alamat1  = '" & FixQuotes(drutama("vpp1alamat1")) & "', vpp1alamat2  = '" & FixQuotes(drutama("vpp1alamat2")) & "', vpp1alamat3  = '" & FixQuotes(drutama("vpp1alamat3")) & "', vpp2alamat1  = '" & FixQuotes(drutama("vpp2alamat1")) & "', vpp2alamat2  = '" & FixQuotes(drutama("vpp2alamat2")) & "', vpp2alamat3  = '" & FixQuotes(drutama("vpp2alamat3")) & "', vppbagianpembayaran  = " & drutama("vppbagianpembayaran") & ", vppuraian  = '" & FixQuotes(drutama("vppuraian")) & "', vppcatatan  = '" & FixQuotes(drutama("vppcatatan")) & "', vppnoref  = '" & FixQuotes(drutama("vppnoref")) & "', vpptglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("vpptglnoref"))) & "', vppcarabayar  = " & drutama("vppcarabayar") & ", vpptglbayar  = '" & FixQuotes(AsFormatTanggal(drutama("vpptglbayar"))) & "', vppmatauang  = '" & FixQuotes(drutama("vppmatauang")) & "', vppkurs  = '" & FixDouble(drutama("vppkurs")) & "', vpptotalap  = '" & FixDouble(drutama("vpptotalap")) & "', vpptotalapvalas  = '" & FixDouble(drutama("vpptotalapvalas")) & "', vpptotalar  = '" & FixDouble(drutama("vpptotalar")) & "', vpptotalarvalas  = '" & FixDouble(drutama("vpptotalarvalas")) & "', vppbayar  = '" & FixDouble(drutama("vppbayar")) & "', vppbayarvalas  = '" & FixDouble(drutama("vppbayarvalas")) & "', vppselisihkurs  = '" & FixDouble(drutama("vppselisihkurs")) & "', vpprekselisihkurs  = '" & FixQuotes(drutama("vpprekselisihkurs")) & "', vppdiskontermin  = '" & FixDouble(drutama("vppdiskontermin")) & "', vppdiskonterminvalas  = '" & FixDouble(drutama("vppdiskonterminvalas")) & "', vpprekdiskontermin  = '" & FixQuotes(drutama("vpprekdiskontermin")) & "', vppstatusvp  = " & drutama("vppstatusvp") & ", vppstatus  = " & drutama("vppstatus") & ", vppstatussebelumnya  = " & drutama("vppstatussebelumnya") & ", vppjmlrevisi  = vppjmlrevisi+1, vppcetakanke  = " & drutama("vppcetakanke") & ", vppmodifikasiuser  = " & drutama("vppmodifikasiuser") & ", vppmodifikasitgl  = NOW(), vppcustomtext1  = '" & FixQuotes(drutama("vppcustomtext1")) & "', vppcustomtext2  = '" & FixQuotes(drutama("vppcustomtext2")) & "', vppcustomtext3  = '" & FixQuotes(drutama("vppcustomtext3")) & "', vppcustomtext4  = '" & FixQuotes(drutama("vppcustomtext4")) & "', vppcustomtext5  = '" & FixQuotes(drutama("vppcustomtext5")) & "', vppcustomint1  = " & drutama("vppcustomint1") & ", vppcustomint2  = " & drutama("vppcustomint2") & ", vppcustomint3  = " & drutama("vppcustomint3") & ", vppcustomdbl1  = '" & FixDouble(drutama("vppcustomdbl1")) & "', vppcustomdbl2  = '" & FixDouble(drutama("vppcustomdbl2")) & "', vppcustomdbl3  = '" & FixDouble(drutama("vppcustomdbl3")) & "', vppcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate1"))) & "', vppcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate2"))) & "', vppcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate3"))) & "' where vppid = '" & drutama("vppid") & "'"
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

                    If drutama("vppautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("vppcabang"), drutama("vpplokasi"), drutama("vppsumber"), drutama("vpptgl"))
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
                        notransaksi = drutama("vppnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(vppid) FROM m4_vpp WHERE vppnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Vpp (vppcabang, vpplokasi, vppgudang, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl, vppkodepa, vppsupplier, vppsupplierkontak, vpp1alamat1, vpp1alamat2, vpp1alamat3, vpp2alamat1, vpp2alamat2, vpp2alamat3, vppbagianpembayaran, vppuraian, vppcatatan, vppnoref, vpptglnoref, vppcarabayar, vpptglbayar, vppmatauang, vppkurs, vpptotalap, vpptotalapvalas, vpptotalar, vpptotalarvalas, vppbayar, vppbayarvalas, vppselisihkurs, vpprekselisihkurs, vppdiskontermin, vppdiskonterminvalas, vpprekdiskontermin, vppstatusvp, vppstatus, vppstatussebelumnya, vppjmlrevisi, vppcetakanke, vppinputuser, vppinputtgl, vppmodifikasiuser, vppmodifikasitgl, vppisclose, vppcustomtext1, vppcustomtext2, vppcustomtext3, vppcustomtext4, vppcustomtext5, vppcustomint1, vppcustomint2, vppcustomint3, vppcustomdbl1, vppcustomdbl2, vppcustomdbl3, vppcustomdate1, vppcustomdate2, vppcustomdate3) values('" & FixQuotes(drutama("vppcabang")) & "', '" & FixQuotes(drutama("vpplokasi")) & "', '" & FixQuotes(drutama("vppgudang")) & "', '" & FixQuotes(drutama("vppsumber")) & "', " & drutama("vppautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpptgl"))) & "', " & drutama("vppkodepa") & ", " & drutama("vppsupplier") & ", '" & FixQuotes(drutama("vppsupplierkontak")) & "', '" & FixQuotes(drutama("vpp1alamat1")) & "', '" & FixQuotes(drutama("vpp1alamat2")) & "', '" & FixQuotes(drutama("vpp1alamat3")) & "', '" & FixQuotes(drutama("vpp2alamat1")) & "', '" & FixQuotes(drutama("vpp2alamat2")) & "', '" & FixQuotes(drutama("vpp2alamat3")) & "', " & drutama("vppbagianpembayaran") & ", '" & FixQuotes(drutama("vppuraian")) & "', '" & FixQuotes(drutama("vppcatatan")) & "', '" & FixQuotes(drutama("vppnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vpptglnoref"))) & "', " & drutama("vppcarabayar") & ", '" & FixQuotes(AsFormatTanggal(drutama("vpptglbayar"))) & "', '" & FixQuotes(drutama("vppmatauang")) & "', '" & FixDouble(drutama("vppkurs")) & "', '" & FixDouble(drutama("vpptotalap")) & "', '" & FixDouble(drutama("vpptotalapvalas")) & "', '" & FixDouble(drutama("vpptotalar")) & "', '" & FixDouble(drutama("vpptotalarvalas")) & "', '" & FixDouble(drutama("vppbayar")) & "', '" & FixDouble(drutama("vppbayarvalas")) & "', '" & FixDouble(drutama("vppselisihkurs")) & "', '" & FixQuotes(drutama("vpprekselisihkurs")) & "', '" & FixDouble(drutama("vppdiskontermin")) & "', '" & FixDouble(drutama("vppdiskonterminvalas")) & "', '" & FixQuotes(drutama("vpprekdiskontermin")) & "', " & drutama("vppstatusvp") & ", " & drutama("vppstatus") & ", " & drutama("vppstatussebelumnya") & ", " & drutama("vppjmlrevisi") & ", " & drutama("vppcetakanke") & ", " & drutama("vppinputuser") & ", NOW(), " & drutama("vppmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("vppisclose") & ", '" & FixQuotes(drutama("vppcustomtext1")) & "', '" & FixQuotes(drutama("vppcustomtext2")) & "', '" & FixQuotes(drutama("vppcustomtext3")) & "', '" & FixQuotes(drutama("vppcustomtext4")) & "', '" & FixQuotes(drutama("vppcustomtext5")) & "', " & drutama("vppcustomint1") & ", " & drutama("vppcustomint2") & ", " & drutama("vppcustomint3") & ", '" & FixDouble(drutama("vppcustomdbl1")) & "', '" & FixDouble(drutama("vppcustomdbl2")) & "', '" & FixDouble(drutama("vppcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("vppcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select vppid from M4_vpp where vppnotransaksi='" & notransaksi & "' AND vppinputuser= '" & userid & "' order by vppmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vpp_Detail where idvpp = '" & result(4) & "'"
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
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvppdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "', '" & FixDouble(dr1("rencana")) & "', '" & FixDouble(dr1("sisa")) & "', '" & FixDouble(dr1("jmlbayar")) & "', '" & FixDouble(dr1("jmlbayarvalas")) & "', '" & FixQuotes(dr1("diskontermin")) & "', '" & FixDouble(dr1("jmldiskontermin")) & "', '" & FixDouble(dr1("jmldiskonterminvalas")) & "', '" & FixQuotes(dr1("rekhutangpiutang")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(dr1("jmlvp")) & "', '" & FixDouble(dr1("jmlvpvalas")) & "', " & dr1("statusvp") & ", " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Vpp_Detail(idvppdetail, idvpp, sumber, idtransaksi, matauang, kurs, totaltransaksi, terbayar, rencana, sisa, jmlbayar, jmlbayarvalas, diskontermin, jmldiskontermin, jmldiskonterminvalas, rekhutangpiutang, catatan, costcenter, divisi, subdivisi, proyek, jmlvp, jmlvpvalas, statusvp, urutan, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Vpp_Pay where idvpp = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses pay
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idvppcarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", '" & FixDouble(dr1("jmlvp")) & "', '" & FixDouble(dr1("jmlvpvalas")) & "', " & dr1("statusvp") & ", " & dr1("isclose") & ")")
                    Next
                    sql = "Insert into M4_Vpp_Pay(idvppcarabayar, idvpp, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, jmlvp, jmlvpvalas, statusvp, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                If drutama("vppstatus") = 2 Then
                    'UPDATE STATUSVPP
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                    'RI
                    If Len(updFilterRI) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri SET ri.ristatusvpp = 1 WHERE " & updFilterRI
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'AP
                    If Len(updFilterAP) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ap ap SET ap.apstatusvpp = 1 WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'PRT
                    If Len(updFilterPRT) > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_prt prt SET prt.prtstatusvpp = 1 WHERE " & updFilterPRT
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'UPDATE TRANSAKSI PEMBAYARAN ====================================================
                End If


                'INSERT USER LOG ====================================================================
                Dim sumber As String = "VPP", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_VppUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vpp", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Vpptgl, Vppnotransaksi, Vppstatus FROM M4_Vpp WHERE Vppid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Vppstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m4_vpp_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Vpp_HistorySimpan("" & paramSplit(0) & "★M4_Vpp_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_vpp_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'VARIABEL CEK TRANSAKSI PEMBAYARAN --> RI, AP, PRT, CA
                Dim updFilterRI As String = "", updFilterAP As String = "", updFilterPRT As String = ""
                Dim idtransaksiDetail As Integer = 0, sumberDetail As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT sumber, idtransaksi FROM M4_vpp_detail WHERE idvpp = '" & idtransaksi & "'")

                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        sumberDetail = dr1("sumber") : idtransaksiDetail = dr1("idtransaksi")

                        'VALIDASI TRANSAKSI PEMBAYARAN ----------------
                        Select Case sumberDetail
                            Case "RI"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(ri.riid = '" & idtransaksiDetail & "')")

                            Case "AP"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                                updFilterAP = String.Concat(updFilterAP, "(ap.apid = '" & idtransaksiDetail & "')")

                            Case "PRT"
                                'SET FILTER UPDATE OUTSTANDING
                                updFilterPRT = IIf(Len(updFilterPRT.ToString) = 0, "", updFilterPRT & " OR ")
                                updFilterPRT = String.Concat(updFilterPRT, "(prt.prtid = '" & idtransaksiDetail & "')")

                        End Select
                        'END OF VALIDASI TRANSAKSI PEMBAYARAN ---------

                    Next

                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai

                End If


                'UPDATE TRANSAKSI PEMBAYARAN ========================================================
                'RI
                If Len(updFilterRI) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ri ri SET ri.ristatusvpp = 0 WHERE " & updFilterRI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'AP
                If Len(updFilterAP) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_ap ap SET ap.apstatusvpp = 0 WHERE " & updFilterAP
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'PRT
                If Len(updFilterPRT) > 0 Then
                    'TRANSAKSI
                    sql = "UPDATE m4_prt prt SET prt.prtstatusvpp = 0 WHERE " & updFilterPRT
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'UPDATE TRANSAKSI PEMBAYARAN ========================================================

            End If

            'update status utama
            sql = "UPDATE M4_Vpp SET Vppstatus = " & nilaiStatus & ", Vppmodifikasiuser='" & userid & "', Vppmodifikasitgl = NOW(), Vppposting = 0, Vpppostingtgl = '1971-01-01 00:00:00', Vppjmlrevisi = Vppjmlrevisi + 1 WHERE Vppid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VppSearch(PostWsSearch(paramSplit(0), "M4_VppSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_VppDeleteOld(ByVal param As String) As String

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
            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        Else
            formatTglWaktu = pagingSplit(5)
        End If
        'END OF VALIDASI PARAMETER PAGING ==================================================

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("vppsupplierkode", "c1.kkode")
            Filter = Filter.Replace("vppsuppliernama", "c1.knama")
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
            Dim sumber As String = "Vpp", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Vppid, Vppnotransaksi FROM M4_Vpp WHERE Vppid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT vppcabang, vpplokasi, vppsumber, vppautonotransaksi, vppnotransaksi, vpptgl"
            sql &= " FROM M4_vpp"
            sql &= " WHERE vppid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("vppcabang")
                lokasi = dtNomorNext.Rows(0)("vpplokasi")
                sumber = dtNomorNext.Rows(0)("vppsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("vppautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("vppnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("vpptgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE PAY
            sql = "DELETE FROM M4_Vpp_Pay WHERE idvpp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Vpp_Detail WHERE idvpp='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Vpp WHERE vppid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_VppSearch(PostWsSearch(paramSplit(0), "M4_VppSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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