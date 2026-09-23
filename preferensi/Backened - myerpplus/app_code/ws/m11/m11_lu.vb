Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_lu
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_LuSimpan(ByVal param As String) As String
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
        Dim strRekCostCenter As String = ""
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
        'luid(0) As Integer, lucabang(1) As String, lulokasi(2) As String, lugudang(3) As String, lusumber(4) As String, 
        'luautonotransaksi(5) As Integer, lunotransaksi(6) As String, lutgl(7) As Date, lukodepa(8) As Integer, lucustomer(9) As Integer,
        'lucustomerkontak(10) As String, luuraian(11) As String, lucatatan(12) As String, lunoref(13) As String, lutglnoref(14) As Date, 
        'lumatauang(15) As String, lukurs(16) As Double, lutotaltransaksi(17) As Double, luidkj(18) As Integer, lustatusrealisasi(19) As Interger, 
        'lustatus(20) As Integer, lustatussebelumnya(21) As Integer, lujmlrevisi(22) As Integer, lucetakanke(23) As Integer, luinputuser(24) As Integer, 
        'luinputtgl(25) As DateTime, lumodifikasiuser(26) As Integer, lumodifikasitgl(27) As DateTime, luposting(28) As Integer, luisclose(29) As Integer, 
        'lucustomtext1(30) As String, lucustomtext2(31) As String, lucustomtext3(32) As String, lucustomtext4(33) As String, lucustomtext5(34) As String, 
        'lucustomtext6(35) As String, lucustomtext7(36) As String, lucustomtext8(37) As String, lucustomtext9(38) As String, lucustomtext10(39) As String, 
        'lucustomtext11(40) As String, lucustomtext12(41) As String, lucustomtext13(42) As String, lucustomtext14(43) As String, lucustomtext15(44) As String, 
        'lucustomtext16(45) As String, lucustomtext17(46) As String, lucustomtext18(47) As String, lucustomtext19(48) As String, lucustomtext20(49) As String, 
        'lucustomint1(50) As Integer, lucustomint2(51) As Integer, lucustomint3(52) As Integer, lucustomint4(53) As Integer, lucustomint5(54) As Integer, 
        'lucustomint6(55) As Integer, lucustomint7(56) As Integer, lucustomint8(57) As Integer, lucustomint9(58) As Integer, lucustomint10(59) As Integer, 
        'lucustomint11(60) As Integer, lucustomint12(61) As Integer, lucustomint13(62) As Integer, lucustomint14(63) As Integer, lucustomint15(64) As Integer, 
        'lucustomint16(65) As Integer, lucustomint17(66) As Integer, lucustomint18(67) As Integer, lucustomint19(68) As Integer, lucustomint20(69) As Integer, 
        'lucustomdbl1(70) As Double, lucustomdbl2(71) As Double, lucustomdbl3(72) As Double, lucustomdbl4(73) As Double, lucustomdbl5(74) As Double, 
        'lucustomdbl6(75) As Double, lucustomdbl7(76) As Double, lucustomdbl8(77) As Double, lucustomdbl9(78) As Double, lucustomdbl10(79) As Double, 
        'lucustomdbl11(80) As Double, lucustomdbl12(81) As Double, lucustomdbl13(82) As Double, lucustomdbl14(83) As Double, lucustomdbl15(84) As Double, 
        'lucustomdbl16(85) As Double, lucustomdbl17(86) As Double, lucustomdbl18(87) As Double, lucustomdbl19(88) As Double, lucustomdbl20(89) As Double, 
        'lucustomdate1(90) As Date, lucustomdate2(91) As Date, lucustomdate3(92) As Date, lucustomdate4(93) As Date, lucustomdate5(94) As Date, 
        'lucustomdate6(95) As Date, lucustomdate7(96) As Date, lucustomdate8(97) As Date, lucustomdate9(98) As Date, lucustomdate10(99) As Date, 
        'lucustomdate11(100) As Date, lucustomdate12(101) As Date, lucustomdate13(102) As Date, lucustomdate14(103) As Date, lucustomdate15(104) As Date, 
        'lucustomdate16(105) As Date, lucustomdate17(106) As Date, lucustomdate18(107) As Date, lucustomdate19(108) As Date, lucustomdate20(109) As Date,
        'luperawatan(110) As String, lukategoripasien(111) As String, lukamar(112) As String, luawalankatpasien(113) As String


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'luid, lucabang, lulokasi, lugudang, lusumber, 
        'luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer,
        'lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, 
        'lumatauang, lukurs, lutotaltransaksi, luidkj, lustatusrealisasi, 
        'lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, 
        'luinputtgl, lumodifikasiuser, lumodifikasitgl, luposting, luisclose,
        'lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5,
        'lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10,
        'lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15,
        'lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20,
        'lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5,
        'lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10,
        'lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15,
        'lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20,
        'lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5,
        'lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10,
        'lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15,
        'lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20,
        'lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5,
        'lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10,
        'lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15,
        'lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20,
        'luperawatan, lukategoripasien, lukamar, luawalankatpasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 116) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'luid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "luid required numeric." : GoTo selesai
        End If
        'luautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "luautonotransaksi required numeric." : GoTo selesai
        End If
        'lutgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "lutgl required date." : GoTo selesai
        End If
        'lukodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "lukodepa required numeric." : GoTo selesai
        End If
        'lucustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "lucustomer required numeric." : GoTo selesai
        End If
        'lutglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "lutglnoref required date." : GoTo selesai
        End If
        'lukurs(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "lukurs required numeric." : GoTo selesai
        End If
        'lutotaltransaksi(17) As Double
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "lutotaltransaksi required numeric." : GoTo selesai
        End If
        'luidkj(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "luidkj required numeric." : GoTo selesai
        End If
        'lustatusrealisasi(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "lustatusrealisasi required numeric." : GoTo selesai
        End If
        'lustatus(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "lustatus required numeric." : GoTo selesai
        End If
        'lustatussebelumnya(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "lustatussebelumnya required numeric." : GoTo selesai
        End If
        'lujmlrevisi(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "lujmlrevisi required numeric." : GoTo selesai
        End If
        'lucetakanke(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "lucetakanke required numeric." : GoTo selesai
        End If
        'luinputuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "luinputuser required numeric." : GoTo selesai
        End If
        'luinputtgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "luinputtgl required date." : GoTo selesai
        End If
        'lumodifikasiuser(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "lumodifikasiuser required numeric." : GoTo selesai
        End If
        'lumodifikasitgl(27) As DateTime
        If (IsDate(dataUtama(27)) = False) Then
            result(2) = "lumodifikasitgl required date." : GoTo selesai
        End If
        'luposting(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "luposting required numeric." : GoTo selesai
        End If
        'luisclose(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "luisclose required numeric." : GoTo selesai
        End If
        'lucustomint1(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "lucustomint1 required numeric." : GoTo selesai
        End If
        'lucustomint2(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "lucustomint2 required numeric." : GoTo selesai
        End If
        'lucustomint3(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "lucustomint3 required numeric." : GoTo selesai
        End If
        'lucustomint4(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "lucustomint4 required numeric." : GoTo selesai
        End If
        'lucustomint5(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "lucustomint5 required numeric." : GoTo selesai
        End If
        'lucustomint6(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "lucustomint6 required numeric." : GoTo selesai
        End If
        'lucustomint7(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "lucustomint7 required numeric." : GoTo selesai
        End If
        'lucustomint8(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "lucustomint8 required numeric." : GoTo selesai
        End If
        'lucustomint9(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "lucustomint9 required numeric." : GoTo selesai
        End If
        'lucustomint10(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "lucustomint10 required numeric." : GoTo selesai
        End If
        'lucustomint11(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "lucustomint11 required numeric." : GoTo selesai
        End If
        'lucustomint12(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "lucustomint12 required numeric." : GoTo selesai
        End If
        'lucustomint13(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "lucustomint13 required numeric." : GoTo selesai
        End If
        'lucustomint14(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "lucustomint14 required numeric." : GoTo selesai
        End If
        'lucustomint15(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "lucustomint15 required numeric." : GoTo selesai
        End If
        'lucustomint16(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "lucustomint16 required numeric." : GoTo selesai
        End If
        'lucustomint17(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "lucustomint17 required numeric." : GoTo selesai
        End If
        'lucustomint18(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "lucustomint18 required numeric." : GoTo selesai
        End If
        'lucustomint19(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "lucustomint19 required numeric." : GoTo selesai
        End If
        'lucustomint20(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "lucustomint20 required numeric." : GoTo selesai
        End If
        'lucustomdbl1(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "lucustomdbl1 required numeric." : GoTo selesai
        End If
        'lucustomdbl2(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "lucustomdbl2 required numeric." : GoTo selesai
        End If
        'lucustomdbl3(72) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "lucustomdbl3 required numeric." : GoTo selesai
        End If
        'lucustomdbl4(73) As Double
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "lucustomdbl4 required numeric." : GoTo selesai
        End If
        'lucustomdbl5(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "lucustomdbl5 required numeric." : GoTo selesai
        End If
        'lucustomdbl6(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "lucustomdbl6 required numeric." : GoTo selesai
        End If
        'lucustomdbl7(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "lucustomdbl7 required numeric." : GoTo selesai
        End If
        'lucustomdbl8(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "lucustomdbl8 required numeric." : GoTo selesai
        End If
        'lucustomdbl9(78) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "lucustomdbl9 required numeric." : GoTo selesai
        End If
        'lucustomdbl10(79) As Double
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "lucustomdbl10 required numeric." : GoTo selesai
        End If
        'lucustomdbl11(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "lucustomdbl11 required numeric." : GoTo selesai
        End If
        'lucustomdbl12(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "lucustomdbl12 required numeric." : GoTo selesai
        End If
        'lucustomdbl13(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "lucustomdbl13 required numeric." : GoTo selesai
        End If
        'lucustomdbl14(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "lucustomdbl14 required numeric." : GoTo selesai
        End If
        'lucustomdbl15(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "lucustomdbl15 required numeric." : GoTo selesai
        End If
        'lucustomdbl16(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "lucustomdbl16 required numeric." : GoTo selesai
        End If
        'lucustomdbl17(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "lucustomdbl17 required numeric." : GoTo selesai
        End If
        'lucustomdbl18(87) As Double
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "lucustomdbl18 required numeric." : GoTo selesai
        End If
        'lucustomdbl19(88) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "lucustomdbl19 required numeric." : GoTo selesai
        End If
        'lucustomdbl20(89) As Double
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "lucustomdbl20 required numeric." : GoTo selesai
        End If
        'lucustomdate1(90) As Date
        If (IsDate(dataUtama(90)) = False) Then
            result(2) = "lucustomdate1 required date." : GoTo selesai
        End If
        'lucustomdate2(91) As Date
        If (IsDate(dataUtama(91)) = False) Then
            result(2) = "lucustomdate2 required date." : GoTo selesai
        End If
        'lucustomdate3(92) As Date
        If (IsDate(dataUtama(92)) = False) Then
            result(2) = "lucustomdate3 required date." : GoTo selesai
        End If
        'lucustomdate4(93) As Date
        If (IsDate(dataUtama(93)) = False) Then
            result(2) = "lucustomdate4 required date." : GoTo selesai
        End If
        'lucustomdate5(94) As Date
        If (IsDate(dataUtama(94)) = False) Then
            result(2) = "lucustomdate5 required date." : GoTo selesai
        End If
        'lucustomdate6(95) As Date
        If (IsDate(dataUtama(95)) = False) Then
            result(2) = "lucustomdate6 required date." : GoTo selesai
        End If
        'lucustomdate7(96) As Date
        If (IsDate(dataUtama(96)) = False) Then
            result(2) = "lucustomdate7 required date." : GoTo selesai
        End If
        'lucustomdate8(97) As Date
        If (IsDate(dataUtama(97)) = False) Then
            result(2) = "lucustomdate8 required date." : GoTo selesai
        End If
        'lucustomdate9(98) As Date
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "lucustomdate9 required date." : GoTo selesai
        End If
        'lucustomdate10(99) As Date
        If (IsDate(dataUtama(99)) = False) Then
            result(2) = "lucustomdate10 required date." : GoTo selesai
        End If
        'lucustomdate11(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "lucustomdate11 required date." : GoTo selesai
        End If
        'lucustomdate12(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "lucustomdate12 required date." : GoTo selesai
        End If
        'lucustomdate13(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "lucustomdate13 required date." : GoTo selesai
        End If
        'lucustomdate14(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "lucustomdate14 required date." : GoTo selesai
        End If
        'lucustomdate15(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "lucustomdate15 required date." : GoTo selesai
        End If
        'lucustomdate16(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "lucustomdate16 required date." : GoTo selesai
        End If
        'lucustomdate17(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "lucustomdate17 required date." : GoTo selesai
        End If
        'lucustomdate18(107) As Date
        If (IsDate(dataUtama(107)) = False) Then
            result(2) = "lucustomdate18 required date." : GoTo selesai
        End If
        'lucustomdate19(108) As Date
        If (IsDate(dataUtama(108)) = False) Then
            result(2) = "lucustomdate19 required date." : GoTo selesai
        End If
        'lucustomdate20(109) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "lucustomdate20 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'lucabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "lucabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "lucabang should not be more than 25 character." : GoTo selesai
        End If

        'lulokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "lulokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "lulokasi should not be more than 25 character." : GoTo selesai
        End If

        'lugudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "lugudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "lugudang should not be more than 25 character." : GoTo selesai
        End If

        'lusumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "lusumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "lusumber should not be more than 10 character." : GoTo selesai
        End If

        'lunotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "lunotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "lunotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'lutgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "lutgl can't be empty" : GoTo selesai
        End If

        'lutglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "lutglnoref can't be empty" : GoTo selesai
        End If

        'lumatauang(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "lumatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "lumatauang should not be more than 25 character." : GoTo selesai
        End If

        'lukurs(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "lukurs can't be empty" : GoTo selesai
        End If

        'lutotaltransaksi(17) As Double
        If Len(dataUtama(17)) = 0 Then
            result(2) = "lutotaltransaksi can't be empty" : GoTo selesai
        End If

        'luinputtgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "luinputtgl can't be empty" : GoTo selesai
        End If

        'lumodifikasitgl(27) As DateTime
        If Len(dataUtama(27)) = 0 Then
            result(2) = "lumodifikasitgl can't be empty" : GoTo selesai
        End If

        'lucustomdbl1(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "lucustomdbl1 can't be empty" : GoTo selesai
        End If

        'lucustomdbl2(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "lucustomdbl2 can't be empty" : GoTo selesai
        End If

        'lucustomdbl3(72) As Double
        If Len(dataUtama(72)) = 0 Then
            result(2) = "lucustomdbl3 can't be empty" : GoTo selesai
        End If

        'lucustomdbl4(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "lucustomdbl4 can't be empty" : GoTo selesai
        End If

        'lucustomdbl5(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "lucustomdbl5 can't be empty" : GoTo selesai
        End If

        'lucustomdbl6(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "lucustomdbl6 can't be empty" : GoTo selesai
        End If

        'lucustomdbl7(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "lucustomdbl7 can't be empty" : GoTo selesai
        End If

        'lucustomdbl8(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "lucustomdbl8 can't be empty" : GoTo selesai
        End If

        'lucustomdbl9(78) As Double
        If Len(dataUtama(78)) = 0 Then
            result(2) = "lucustomdbl9 can't be empty" : GoTo selesai
        End If

        'lucustomdbl10(79) As Double
        If Len(dataUtama(79)) = 0 Then
            result(2) = "lucustomdbl10 can't be empty" : GoTo selesai
        End If

        'lucustomdbl11(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "lucustomdbl11 can't be empty" : GoTo selesai
        End If

        'lucustomdbl12(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "lucustomdbl12 can't be empty" : GoTo selesai
        End If

        'lucustomdbl13(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "lucustomdbl13 can't be empty" : GoTo selesai
        End If

        'lucustomdbl14(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "lucustomdbl14 can't be empty" : GoTo selesai
        End If

        'lucustomdbl15(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "lucustomdbl15 can't be empty" : GoTo selesai
        End If

        'lucustomdbl16(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "lucustomdbl16 can't be empty" : GoTo selesai
        End If

        'lucustomdbl17(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "lucustomdbl17 can't be empty" : GoTo selesai
        End If

        'lucustomdbl18(87) As Double
        If Len(dataUtama(87)) = 0 Then
            result(2) = "lucustomdbl18 can't be empty" : GoTo selesai
        End If

        'lucustomdbl19(88) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "lucustomdbl19 can't be empty" : GoTo selesai
        End If

        'lucustomdbl20(89) As Double
        If Len(dataUtama(89)) = 0 Then
            result(2) = "lucustomdbl20 can't be empty" : GoTo selesai
        End If

        'lucustomdate1(90) As Date
        If Len(dataUtama(90)) = 0 Then
            result(2) = "lucustomdate1 can't be empty" : GoTo selesai
        End If

        'lucustomdate2(91) As Date
        If Len(dataUtama(91)) = 0 Then
            result(2) = "lucustomdate2 can't be empty" : GoTo selesai
        End If

        'lucustomdate3(92) As Date
        If Len(dataUtama(92)) = 0 Then
            result(2) = "lucustomdate3 can't be empty" : GoTo selesai
        End If

        'lucustomdate4(93) As Date
        If Len(dataUtama(93)) = 0 Then
            result(2) = "lucustomdate4 can't be empty" : GoTo selesai
        End If

        'lucustomdate5(94) As Date
        If Len(dataUtama(94)) = 0 Then
            result(2) = "lucustomdate5 can't be empty" : GoTo selesai
        End If

        'lucustomdate6(95) As Date
        If Len(dataUtama(95)) = 0 Then
            result(2) = "lucustomdate6 can't be empty" : GoTo selesai
        End If

        'lucustomdate7(96) As Date
        If Len(dataUtama(96)) = 0 Then
            result(2) = "lucustomdate7 can't be empty" : GoTo selesai
        End If

        'lucustomdate8(97) As Date
        If Len(dataUtama(97)) = 0 Then
            result(2) = "lucustomdate8 can't be empty" : GoTo selesai
        End If

        'lucustomdate9(98) As Date
        If Len(dataUtama(98)) = 0 Then
            result(2) = "lucustomdate9 can't be empty" : GoTo selesai
        End If

        'lucustomdate10(99) As Date
        If Len(dataUtama(99)) = 0 Then
            result(2) = "lucustomdate10 can't be empty" : GoTo selesai
        End If

        'lucustomdate11(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "lucustomdate11 can't be empty" : GoTo selesai
        End If

        'lucustomdate12(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "lucustomdate12 can't be empty" : GoTo selesai
        End If

        'lucustomdate13(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "lucustomdate13 can't be empty" : GoTo selesai
        End If

        'lucustomdate14(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "lucustomdate14 can't be empty" : GoTo selesai
        End If

        'lucustomdate15(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "lucustomdate15 can't be empty" : GoTo selesai
        End If

        'lucustomdate16(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "lucustomdate16 can't be empty" : GoTo selesai
        End If

        'lucustomdate17(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "lucustomdate17 can't be empty" : GoTo selesai
        End If

        'lucustomdate18(107) As Date
        If Len(dataUtama(107)) = 0 Then
            result(2) = "lucustomdate18 can't be empty" : GoTo selesai
        End If

        'lucustomdate19(108) As Date
        If Len(dataUtama(108)) = 0 Then
            result(2) = "lucustomdate19 can't be empty" : GoTo selesai
        End If

        'lucustomdate20(109) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "lucustomdate20 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "luid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lulokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lugudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lusumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lunotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lutgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lukodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lunoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lutglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lumatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lukurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "lutotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lustatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lustatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lustatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lujmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "luinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "luinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lumodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lumodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "luisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lucustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lucustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lukategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lukamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "luawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lujenisbilling", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lupetugas", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "luid~lucabang~lulokasi~lugudang~lusumber~luautonotransaksi~lunotransaksi~lutgl~lukodepa~lucustomer~lucustomerkontak~luuraian~lucatatan~lunoref~lutglnoref~lumatauang~lukurs~lutotaltransaksi~luidkj~lustatusrealisasi~lustatus~lustatussebelumnya~lujmlrevisi~lucetakanke~luinputuser~luinputtgl~lumodifikasiuser~lumodifikasitgl~luposting~luisclose~lucustomtext1~lucustomtext2~lucustomtext3~lucustomtext4~lucustomtext5~lucustomtext6~lucustomtext7~lucustomtext8~lucustomtext9~lucustomtext10~lucustomtext11~lucustomtext12~lucustomtext13~lucustomtext14~lucustomtext15~lucustomtext16~lucustomtext17~lucustomtext18~lucustomtext19~lucustomtext20~lucustomint1~lucustomint2~lucustomint3~lucustomint4~lucustomint5~lucustomint6~lucustomint7~lucustomint8~lucustomint9~lucustomint10~lucustomint11~lucustomint12~lucustomint13~lucustomint14~lucustomint15~lucustomint16~lucustomint17~lucustomint18~lucustomint19~lucustomint20~lucustomdbl1~lucustomdbl2~lucustomdbl3~lucustomdbl4~lucustomdbl5~lucustomdbl6~lucustomdbl7~lucustomdbl8~lucustomdbl9~lucustomdbl10~lucustomdbl11~lucustomdbl12~lucustomdbl13~lucustomdbl14~lucustomdbl15~lucustomdbl16~lucustomdbl17~lucustomdbl18~lucustomdbl19~lucustomdbl20~lucustomdate1~lucustomdate2~lucustomdate3~lucustomdate4~lucustomdate5~lucustomdate6~lucustomdate7~lucustomdate8~lucustomdate9~lucustomdate10~lucustomdate11~lucustomdate12~lucustomdate13~lucustomdate14~lucustomdate15~lucustomdate16~lucustomdate17~lucustomdate18~lucustomdate19~lucustomdate20~luperawatan~lukategoripasien~lukamar~luawalankatpasien~lujenisbilling~lupetugas", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idludetail(0) As Integer, idlu(1) As Integer, jenis(2) As String, idlayanan(3) As Integer, namalayanan(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmltotal(8) As Double, satuandefault(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, rekpersediaan(22) As String, rekhargapokok(23) As String, rekdiskonpenjualan(24) As String, 
        'rekpenjualan(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idkjdetail(32) As Integer, jmlrealisasi(33) As Double, statusrealisasi(34) As Integer, 
        'isclose(35) As Integer, iddokter(36) As Integer, namadokter(37) As String, customtext1(38) As String, customtext2(39) As String, 
        'customtext3(40) As String, customtext4(41) As String, customtext5(42) As String, customtext6(43) As String, customtext7(44) As String, 
        'customtext8(45) As String, customtext9(46) As String, customtext10(47) As String, customtext11(48) As String, customtext12(49) As String, 
        'customtext13(50) As String, customtext14(51) As String, customtext15(52) As String, customtext16(53) As String, customtext17(54) As String, 
        'customtext18(55) As String, customtext19(56) As String, customtext20(57) As String, customdbl1(58) As Double, customdbl2(59) As Double, 
        'customdbl3(60) As Double, customdbl4(61) As Double, customdbl5(62) As Double, customdbl6(63) As Double, customdbl7(64) As Double, 
        'customdbl8(65) As Double, customdbl9(66) As Double, customdbl10(67) As Double, customdbl11(68) As Double, customdbl12(69) As Double, 
        'customdbl13(70) As Double, customdbl14(71) As Double, customdbl15(72) As Double, customdbl16(73) As Double, customdbl17(74) As Double, 
        'customdbl18(75) As Double, customdbl19(76) As Double, customdbl20(77) As Double, customdate1(78) As Date, customdate2(79) As Date, 
        'customdate3(80) As Date, customdate4(81) As Date, customdate5(82) As Date, customdate6(83) As Date, customdate7(84) As Date, 
        'customdate8(85) As Date, customdate9(86) As Date, customdate10(87) As Date, customdate11(88) As Date, customdate12(89) As Date, 
        'customdate13(90) As Date, customdate14(91) As Date, customdate15(92) As Date, customdate16(93) As Date, customdate17(94) As Date, 
        'customdate18(95) As Date, customdate19(96) As Date, customdate20(97) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idludetail, idlu, jenis, idlayanan, namalayanan, 
        'jml, satuan, nilaisatuan, jmltotal, satuandefault, 
        'matauang, kurs, harga, diskon, jmldiskon, 
        'pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekhargapokok, rekdiskonpenjualan, 
        'rekpenjualan, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, 
        'isclose, iddokter, namadokter, customtext1, customtext2, 
        'customtext3, customtext4, customtext5, customtext6, customtext7, 
        'customtext8, customtext9, customtext10, customtext11, customtext12,
        'customtext13, customtext14, customtext15, customtext16, customtext17,
        'customtext18, customtext19, customtext20, customdbl1, customdbl2,
        'customdbl3, customdbl4, customdbl5, customdbl6, customdbl7,
        'customdbl8, customdbl9, customdbl10, customdbl11, customdbl12,
        'customdbl13, customdbl14, customdbl15, customdbl16, customdbl17,
        'customdbl18, customdbl19, customdbl20, customdate1, customdate2,
        'customdate3, customdate4, customdate5, customdate6, customdate7,
        'customdate8, customdate9, customdate10, customdate11, customdate12,
        'customdate13, customdate14, customdate15, customdate16, customdate17,
        'customdate18, customdate19, customdate20

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idludetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idlu", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idlayanan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namalayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmltotal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuandefault", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idkjdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrealisasi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "statusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "namadokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdate20", AsEnumTypeData.AsString)

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokBooking As String = ""
        Dim idlayanan As Integer = 0, idkjdetail As Integer = 0, jmltotal As Double = 0

        'Variabel Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 98) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idludetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idludetail required numeric." : GoTo selesai
            End If
            'idlu(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idlu required numeric." : GoTo selesai
            End If
            'idlayanan(2) As Integer
            If (IsNumeric(dataRowDetail(3)) = False) Then
                result(2) = "Row : " & i & " - idlayanan required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            dataRowDetail(7) = 1
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmltotal(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmltotal required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idkjdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idkjdetail required numeric." : GoTo selesai
            End If
            'statusrealisasi(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'isclose(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'iddokter(36) As Integer
            'If (IsNumeric(dataRowDetail(36)) = False) Then
            '    result(2) = "Row : " & i & " - iddokter required numeric." : GoTo selesai
            'End If
            'customdbl1(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(60) As Double
            If (IsNumeric(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdbl4(61) As Double
            If (IsNumeric(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - customdbl4 required numeric." : GoTo selesai
            End If
            'customdbl5(62) As Double
            If (IsNumeric(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - customdbl5 required numeric." : GoTo selesai
            End If
            'customdbl6(63) As Double
            If (IsNumeric(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - customdbl6 required numeric." : GoTo selesai
            End If
            'customdbl7(64) As Double
            If (IsNumeric(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - customdbl7 required numeric." : GoTo selesai
            End If
            'customdbl8(65) As Double
            If (IsNumeric(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - customdbl8 required numeric." : GoTo selesai
            End If
            'customdbl9(66) As Double
            If (IsNumeric(dataRowDetail(66)) = False) Then
                result(2) = "Row : " & i & " - customdbl9 required numeric." : GoTo selesai
            End If
            'customdbl10(67) As Double
            If (IsNumeric(dataRowDetail(67)) = False) Then
                result(2) = "Row : " & i & " - customdbl10 required numeric." : GoTo selesai
            End If
            'customdbl11(68) As Double
            If (IsNumeric(dataRowDetail(68)) = False) Then
                result(2) = "Row : " & i & " - customdbl11 required numeric." : GoTo selesai
            End If
            'customdbl12(69) As Double
            If (IsNumeric(dataRowDetail(69)) = False) Then
                result(2) = "Row : " & i & " - customdbl12 required numeric." : GoTo selesai
            End If
            'customdbl13(70) As Double
            If (IsNumeric(dataRowDetail(70)) = False) Then
                result(2) = "Row : " & i & " - customdbl13 required numeric." : GoTo selesai
            End If
            'customdbl14(71) As Double
            If (IsNumeric(dataRowDetail(71)) = False) Then
                result(2) = "Row : " & i & " - customdbl14 required numeric." : GoTo selesai
            End If
            'customdbl15(72) As Double
            If (IsNumeric(dataRowDetail(72)) = False) Then
                result(2) = "Row : " & i & " - customdbl15 required numeric." : GoTo selesai
            End If
            'customdbl16(73) As Double
            If (IsNumeric(dataRowDetail(73)) = False) Then
                result(2) = "Row : " & i & " - customdbl16 required numeric." : GoTo selesai
            End If
            'customdbl17(74) As Double
            If (IsNumeric(dataRowDetail(74)) = False) Then
                result(2) = "Row : " & i & " - customdbl17 required numeric." : GoTo selesai
            End If
            'customdbl18(75) As Double
            If (IsNumeric(dataRowDetail(75)) = False) Then
                result(2) = "Row : " & i & " - customdbl18 required numeric." : GoTo selesai
            End If
            'customdbl19(76) As Double
            If (IsNumeric(dataRowDetail(76)) = False) Then
                result(2) = "Row : " & i & " - customdbl19 required numeric." : GoTo selesai
            End If
            'customdbl20(77) As Double
            If (IsNumeric(dataRowDetail(77)) = False) Then
                result(2) = "Row : " & i & " - customdbl20 required numeric." : GoTo selesai
            End If
            'customdate1(78) As Date
            If (IsDate(dataRowDetail(78)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(79) As Date
            If (IsDate(dataRowDetail(79)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(80) As Date
            If (IsDate(dataRowDetail(80)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'customdate4(81) As Date
            If (IsDate(dataRowDetail(81)) = False) Then
                result(2) = "Row : " & i & " - customdate4 required date." : GoTo selesai
            End If
            'customdate5(82) As Date
            If (IsDate(dataRowDetail(82)) = False) Then
                result(2) = "Row : " & i & " - customdate5 required date." : GoTo selesai
            End If
            'customdate6(83) As Date
            If (IsDate(dataRowDetail(83)) = False) Then
                result(2) = "Row : " & i & " - customdate6 required date." : GoTo selesai
            End If
            'customdate7(84) As Date
            If (IsDate(dataRowDetail(84)) = False) Then
                result(2) = "Row : " & i & " - customdate7 required date." : GoTo selesai
            End If
            'customdate8(85) As Date
            If (IsDate(dataRowDetail(85)) = False) Then
                result(2) = "Row : " & i & " - customdate8 required date." : GoTo selesai
            End If
            'customdate9(86) As Date
            If (IsDate(dataRowDetail(86)) = False) Then
                result(2) = "Row : " & i & " - customdate9 required date." : GoTo selesai
            End If
            'customdate10(87) As Date
            If (IsDate(dataRowDetail(87)) = False) Then
                result(2) = "Row : " & i & " - customdate10 required date." : GoTo selesai
            End If
            'customdate11(88) As Date
            If (IsDate(dataRowDetail(88)) = False) Then
                result(2) = "Row : " & i & " - customdate11 required date." : GoTo selesai
            End If
            'customdate12(89) As Date
            If (IsDate(dataRowDetail(89)) = False) Then
                result(2) = "Row : " & i & " - customdate12 required date." : GoTo selesai
            End If
            'customdate13(90) As Date
            If (IsDate(dataRowDetail(90)) = False) Then
                result(2) = "Row : " & i & " - customdate13 required date." : GoTo selesai
            End If
            'customdate14(91) As Date
            If (IsDate(dataRowDetail(91)) = False) Then
                result(2) = "Row : " & i & " - customdate14 required date." : GoTo selesai
            End If
            'customdate15(92) As Date
            If (IsDate(dataRowDetail(92)) = False) Then
                result(2) = "Row : " & i & " - customdate15 required date." : GoTo selesai
            End If
            'customdate16(93) As Date
            If (IsDate(dataRowDetail(93)) = False) Then
                result(2) = "Row : " & i & " - customdate16 required date." : GoTo selesai
            End If
            'customdate17(94) As Date
            If (IsDate(dataRowDetail(94)) = False) Then
                result(2) = "Row : " & i & " - customdate17 required date." : GoTo selesai
            End If
            'customdate18(95) As Date
            If (IsDate(dataRowDetail(95)) = False) Then
                result(2) = "Row : " & i & " - customdate18 required date." : GoTo selesai
            End If
            'customdate19(96) As Date
            If (IsDate(dataRowDetail(96)) = False) Then
                result(2) = "Row : " & i & " - customdate19 required date." : GoTo selesai
            End If
            'customdate20(97) As Date
            If (IsDate(dataRowDetail(97)) = False) Then
                result(2) = "Row : " & i & " - customdate20 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'jenis(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - jenis can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 100 Then
                result(2) = "Row : " & i & " - jenis should not be more than 100 character." : GoTo selesai
            End If

            'namalayanan(4) As String
            If Len(dataRowDetail(4)) = 0 Then
                result(2) = "Row : " & i & " - namalayanan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(4)) > 100 Then
                result(2) = "Row : " & i & " - namalayanan should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowDetail(5)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(5) <= 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowDetail(6)) = 0 Then
                result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmltotal(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmltotal can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmltotal can't be less than or equal to zero" : GoTo selesai
            End If

            'satuandefault(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuandefault can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuandefault should not be more than 25 character." : GoTo selesai
            End If

            'matauang(10) As String
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(10) As Double, diskon(11) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'namadokter(37) As String
            'If Len(dataRowDetail(37)) = 0 Then
            '    result(2) = "Row : " & i & " - namadokter can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(37)) > 100 Then
                result(2) = "Row : " & i & " - namadokter should not be more than 100 character." : GoTo selesai
            End If

            'customdbl1(58) As Double
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If
            'customdbl2(59) As Double
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If
            'customdbl3(60) As Double
            If Len(dataRowDetail(60)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If
            'customdbl4(61) As Double
            If Len(dataRowDetail(61)) = 0 Then
                result(2) = "Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
            End If
            'customdbl5(62) As Double
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
            End If
            'customdbl6(63) As Double
            If Len(dataRowDetail(63)) = 0 Then
                result(2) = "Row : " & i & " - customdbl6 can't be empty" : GoTo selesai
            End If
            'customdbl7(64) As Double
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - customdbl7 can't be empty" : GoTo selesai
            End If
            'customdbl8(65) As Double
            If Len(dataRowDetail(65)) = 0 Then
                result(2) = "Row : " & i & " - customdbl8 can't be empty" : GoTo selesai
            End If
            'customdbl9(66) As Double
            If Len(dataRowDetail(66)) = 0 Then
                result(2) = "Row : " & i & " - customdbl9 can't be empty" : GoTo selesai
            End If
            'customdbl10(67) As Double
            If Len(dataRowDetail(67)) = 0 Then
                result(2) = "Row : " & i & " - customdbl10 can't be empty" : GoTo selesai
            End If
            'customdbl11(68) As Double
            If Len(dataRowDetail(68)) = 0 Then
                result(2) = "Row : " & i & " - customdbl11 can't be empty" : GoTo selesai
            End If
            'customdbl12(69) As Double
            If Len(dataRowDetail(69)) = 0 Then
                result(2) = "Row : " & i & " - customdbl12 can't be empty" : GoTo selesai
            End If
            'customdbl13(70) As Double
            If Len(dataRowDetail(70)) = 0 Then
                result(2) = "Row : " & i & " - customdbl13 can't be empty" : GoTo selesai
            End If
            'customdbl14(71) As Double
            If Len(dataRowDetail(71)) = 0 Then
                result(2) = "Row : " & i & " - customdbl14 can't be empty" : GoTo selesai
            End If
            'customdbl15(72) As Double
            If Len(dataRowDetail(72)) = 0 Then
                result(2) = "Row : " & i & " - customdbl15 can't be empty" : GoTo selesai
            End If
            'customdbl16(73) As Double
            If Len(dataRowDetail(73)) = 0 Then
                result(2) = "Row : " & i & " - customdbl16 can't be empty" : GoTo selesai
            End If
            'customdbl17(74) As Double
            If Len(dataRowDetail(74)) = 0 Then
                result(2) = "Row : " & i & " - customdbl17 can't be empty" : GoTo selesai
            End If
            'customdbl18(75) As Double
            If Len(dataRowDetail(75)) = 0 Then
                result(2) = "Row : " & i & " - customdbl18 can't be empty" : GoTo selesai
            End If
            'customdbl19(76) As Double
            If Len(dataRowDetail(76)) = 0 Then
                result(2) = "Row : " & i & " - customdbl19 can't be empty" : GoTo selesai
            End If
            'customdbl20(77) As Double
            If Len(dataRowDetail(77)) = 0 Then
                result(2) = "Row : " & i & " - customdbl20 can't be empty" : GoTo selesai
            End If
            'customdate1(78) As Date
            If Len(dataRowDetail(78)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If
            'customdate2(79) As Date
            If Len(dataRowDetail(79)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If
            'customdate3(80) As Date
            If Len(dataRowDetail(80)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'customdate4(81) As Date
            If Len(dataRowDetail(81)) = 0 Then
                result(2) = "Row : " & i & " - customdate4 can't be empty" : GoTo selesai
            End If
            'customdate5(82) As Date
            If Len(dataRowDetail(82)) = 0 Then
                result(2) = "Row : " & i & " - customdate5 can't be empty" : GoTo selesai
            End If
            'customdate6(83) As Date
            If Len(dataRowDetail(83)) = 0 Then
                result(2) = "Row : " & i & " - customdate6 can't be empty" : GoTo selesai
            End If
            'customdate7(84) As Date
            If Len(dataRowDetail(84)) = 0 Then
                result(2) = "Row : " & i & " - customdate7 can't be empty" : GoTo selesai
            End If
            'customdate8(85) As Date
            If Len(dataRowDetail(85)) = 0 Then
                result(2) = "Row : " & i & " - customdate8 can't be empty" : GoTo selesai
            End If
            'customdate9(86) As Date
            If Len(dataRowDetail(86)) = 0 Then
                result(2) = "Row : " & i & " - customdate9 can't be empty" : GoTo selesai
            End If
            'customdate10(87) As Date
            If Len(dataRowDetail(87)) = 0 Then
                result(2) = "Row : " & i & " - customdate10 can't be empty" : GoTo selesai
            End If
            'customdate11(88) As Date
            If Len(dataRowDetail(88)) = 0 Then
                result(2) = "Row : " & i & " - customdate11 can't be empty" : GoTo selesai
            End If
            'customdate12(89) As Date
            If Len(dataRowDetail(89)) = 0 Then
                result(2) = "Row : " & i & " - customdate12 can't be empty" : GoTo selesai
            End If
            'customdate13(90) As Date
            If Len(dataRowDetail(90)) = 0 Then
                result(2) = "Row : " & i & " - customdate13 can't be empty" : GoTo selesai
            End If
            'customdate14(91) As Date
            If Len(dataRowDetail(91)) = 0 Then
                result(2) = "Row : " & i & " - customdate14 can't be empty" : GoTo selesai
            End If
            'customdate15(92) As Date
            If Len(dataRowDetail(92)) = 0 Then
                result(2) = "Row : " & i & " - customdate15 can't be empty" : GoTo selesai
            End If
            'customdate16(93) As Date
            If Len(dataRowDetail(93)) = 0 Then
                result(2) = "Row : " & i & " - customdate16 can't be empty" : GoTo selesai
            End If
            'customdate17(94) As Date
            If Len(dataRowDetail(94)) = 0 Then
                result(2) = "Row : " & i & " - customdate17 can't be empty" : GoTo selesai
            End If
            'customdate18(95) As Date
            If Len(dataRowDetail(95)) = 0 Then
                result(2) = "Row : " & i & " - customdate18 can't be empty" : GoTo selesai
            End If
            'customdate19(96) As Date
            If Len(dataRowDetail(96)) = 0 Then
                result(2) = "Row : " & i & " - customdate19 can't be empty" : GoTo selesai
            End If
            'customdate20(97) As Date
            If Len(dataRowDetail(97)) = 0 Then
                result(2) = "Row : " & i & " - customdate20 can't be empty" : GoTo selesai
            End If


            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idludetail~idlu~jenis~idlayanan~namalayanan~jml~satuan~nilaisatuan~jmltotal~satuandefault~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idkjdetail~jmlrealisasi~statusrealisasi~isclose~iddokter~namadokter~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customtext11~customtext12~customtext13~customtext14~customtext15~customtext16~customtext17~customtext18~customtext19~customtext20~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdbl11~customdbl12~customdbl13~customdbl14~customdbl15~customdbl16~customdbl17~customdbl18~customdbl19~customdbl20~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~customdate11~customdate12~customdate13~customdate14~customdate15~customdate16~customdate17~customdate18~customdate19~customdate20", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76) & "~" & dataRowDetail(77) & "~" & dataRowDetail(78) & "~" & dataRowDetail(79) & "~" & dataRowDetail(80) & "~" & dataRowDetail(81) & "~" & dataRowDetail(82) & "~" & dataRowDetail(83) & "~" & dataRowDetail(84) & "~" & dataRowDetail(85) & "~" & dataRowDetail(86) & "~" & dataRowDetail(87) & "~" & dataRowDetail(88) & "~" & dataRowDetail(89) & "~" & dataRowDetail(90) & "~" & dataRowDetail(91) & "~" & dataRowDetail(92) & "~" & dataRowDetail(93) & "~" & dataRowDetail(94) & "~" & dataRowDetail(95) & "~" & dataRowDetail(96) & "~" & dataRowDetail(97)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            strRekCostCenter = IIf(Len(strRekCostCenter.ToString) = 0, "", strRekCostCenter & " OR ")
            strRekCostCenter = String.Concat(strRekCostCenter, "(bid = '" & dataRowDetail(3) & "')")

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idlayanan(3) As Integer     , jmltotal(8) As Double       , gudang(21) As String       , idkjdetail(32) As Integer
            idlayanan = dataRowDetail(3) : jmltotal = dataRowDetail(8) : gudang = dataRowDetail(21) : idkjdetail = dataRowDetail(32)
            'kurs(11) As Double                    , harga(12) As Double
            'kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        'result(2) = "as" : GoTo selesai
        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 11, vMenuId As Integer = 4
                Select Case drutama("lustatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("lutgl")), AsFormatTanggal(drutama("lutgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================

                If drutama("lustatus") = 2 Then
                    'Dim cekCoaCostCenter As String = ValidasiItemRequiredCostCenter(strRekCostCenter, dtdetail)
                    'If Len(cekCoaCostCenter) > 0 Then
                    '    result(2) = cekCoaCostCenter : Trans.Rollback() : GoTo selesai
                    'End If
                End If

                If isUpdate Then
                    result(4) = drutama("luid")
                    notransaksi = drutama("lunotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(luid), lunotransaksi FROM M_11_lu WHERE luid='" & result(4) & "' AND lustatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m_11_lu WHERE lunotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m11_lu_history
                        'Dim luSimpanHistory As String = SimpanHistory.m11_Lu_HistorySimpan("" & paramSplit(0) & "★M11_Lu_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("lusumber")) & "▼" & FixQuotes(drutama("luid")) & "")
                        'Dim luSplit() As String = luSimpanHistory.Split(sptParam)
                        'Dim luSplitResult() As String = luSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (luSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & luSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_11_Lu set lucabang  = '" & FixQuotes(drutama("lucabang")) & "', lulokasi  = '" & FixQuotes(drutama("lulokasi")) & "', lugudang  = '" & FixQuotes(drutama("lugudang")) & "', lusumber  = '" & FixQuotes(drutama("lusumber")) & "', luautonotransaksi  = " & drutama("luautonotransaksi") & ", lunotransaksi  = '" & FixQuotes(notransaksi) & "', lutgl  = '" & FixQuotes(AsFormatTanggal(drutama("lutgl"))) & "', lukodepa  = " & drutama("lukodepa") & ", lucustomer  = " & drutama("lucustomer") & ", lucustomerkontak  = '" & FixQuotes(drutama("lucustomerkontak")) & "', luuraian  = '" & FixQuotes(drutama("luuraian")) & "', lucatatan  = '" & FixQuotes(drutama("lucatatan")) & "', lunoref  = '" & FixQuotes(drutama("lunoref")) & "', lutglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("lutglnoref"))) & "', lutotaltransaksi  = '" & FixDouble(drutama("lutotaltransaksi")) & "', luidkj  = " & drutama("luidkj") & ", lustatusrealisasi  = " & drutama("lustatusrealisasi") & ", lustatus  = " & drutama("lustatus") & ", lustatussebelumnya  = " & drutama("lustatussebelumnya") & ", lujmlrevisi  = lujmlrevisi+1, lucetakanke  = " & drutama("lucetakanke") & ", lumodifikasiuser  = " & drutama("lumodifikasiuser") & ", lumodifikasitgl  = NOW(), lucustomtext1  = '" & FixQuotes(drutama("lucustomtext1")) & "', lucustomtext2  = '" & FixQuotes(drutama("lucustomtext2")) & "', lucustomtext3  = '" & FixQuotes(drutama("lucustomtext3")) & "', lucustomtext4  = '" & FixQuotes(drutama("lucustomtext4")) & "', lucustomtext5  = '" & FixQuotes(drutama("lucustomtext5")) & "', lucustomtext6  = '" & FixQuotes(drutama("lucustomtext6")) & "', lucustomtext7  = '" & FixQuotes(drutama("lucustomtext7")) & "', lucustomtext8  = '" & FixQuotes(drutama("lucustomtext8")) & "', lucustomtext9  = '" & FixQuotes(drutama("lucustomtext9")) & "', lucustomtext10  = '" & FixQuotes(drutama("lucustomtext10")) & "', lucustomtext11  = '" & FixQuotes(drutama("lucustomtext11")) & "', lucustomtext12  = '" & FixQuotes(drutama("lucustomtext12")) & "', lucustomtext13  = '" & FixQuotes(drutama("lucustomtext13")) & "', lucustomtext14  = '" & FixQuotes(drutama("lucustomtext14")) & "', lucustomtext15  = '" & FixQuotes(drutama("lucustomtext15")) & "', lucustomtext16  = '" & FixQuotes(drutama("lucustomtext16")) & "', lucustomtext17  = '" & FixQuotes(drutama("lucustomtext17")) & "', lucustomtext18  = '" & FixQuotes(drutama("lucustomtext18")) & "', lucustomtext19  = '" & FixQuotes(drutama("lucustomtext19")) & "', lucustomtext20  = '" & FixQuotes(drutama("lucustomtext20")) & "', lucustomint1  = " & drutama("lucustomint1") & ", lucustomint2  = " & drutama("lucustomint2") & ", lucustomint3  = " & drutama("lucustomint3") & ", lucustomint4  = " & drutama("lucustomint4") & ", lucustomint5  = " & drutama("lucustomint5") & ", lucustomint6  = " & drutama("lucustomint6") & ", lucustomint7  = " & drutama("lucustomint7") & ", lucustomint8  = " & drutama("lucustomint8") & ", lucustomint9  = " & drutama("lucustomint9") & ", lucustomint10  = " & drutama("lucustomint10") & ", lucustomint11  = " & drutama("lucustomint11") & ", lucustomint12  = " & drutama("lucustomint12") & ", lucustomint13  = " & drutama("lucustomint13") & ", lucustomint14  = " & drutama("lucustomint14") & ", lucustomint15  = " & drutama("lucustomint15") & ", lucustomint16  = " & drutama("lucustomint16") & ", lucustomint17  = " & drutama("lucustomint17") & ", lucustomint18  = " & drutama("lucustomint18") & ", lucustomint19  = " & drutama("lucustomint19") & ", lucustomint20  = " & drutama("lucustomint20") & ", lucustomdbl1  = '" & FixDouble(drutama("lucustomdbl1")) & "', lucustomdbl2  = '" & FixDouble(drutama("lucustomdbl2")) & "', lucustomdbl3  = '" & FixDouble(drutama("lucustomdbl3")) & "', lucustomdbl4  = '" & FixDouble(drutama("lucustomdbl4")) & "', lucustomdbl5  = '" & FixDouble(drutama("lucustomdbl5")) & "', lucustomdbl6  = '" & FixDouble(drutama("lucustomdbl6")) & "', lucustomdbl7  = '" & FixDouble(drutama("lucustomdbl7")) & "', lucustomdbl8  = '" & FixDouble(drutama("lucustomdbl8")) & "', lucustomdbl9  = '" & FixDouble(drutama("lucustomdbl9")) & "', lucustomdbl10  = '" & FixDouble(drutama("lucustomdbl10")) & "', lucustomdbl11  = '" & FixDouble(drutama("lucustomdbl11")) & "', lucustomdbl12  = '" & FixDouble(drutama("lucustomdbl12")) & "', lucustomdbl13  = '" & FixDouble(drutama("lucustomdbl13")) & "', lucustomdbl14  = '" & FixDouble(drutama("lucustomdbl14")) & "', lucustomdbl15  = '" & FixDouble(drutama("lucustomdbl15")) & "', lucustomdbl16  = '" & FixDouble(drutama("lucustomdbl16")) & "', lucustomdbl17  = '" & FixDouble(drutama("lucustomdbl17")) & "', lucustomdbl18  = '" & FixDouble(drutama("lucustomdbl18")) & "', lucustomdbl19  = '" & FixDouble(drutama("lucustomdbl19")) & "', lucustomdbl20  = '" & FixDouble(drutama("lucustomdbl20")) & "', lucustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate1"))) & "', lucustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate2"))) & "', lucustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate3"))) & "', lucustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate4"))) & "', lucustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate5"))) & "', lucustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate6"))) & "', lucustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate7"))) & "', lucustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate8"))) & "', lucustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate9"))) & "', lucustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate10"))) & "', lucustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate11"))) & "', lucustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate12"))) & "', lucustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate13"))) & "', lucustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate14"))) & "', lucustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate15"))) & "', lucustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate16"))) & "', lucustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate17"))) & "', lucustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate18"))) & "', lucustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate19"))) & "', lucustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate20"))) & "', lumatauang  = '" & FixQuotes(drutama("lumatauang")) & "', lukurs  = '" & FixDouble(drutama("lukurs")) & "', luperawatan  = '" & FixDouble(drutama("luperawatan")) & "', lukategoripasien  = '" & FixDouble(drutama("lukategoripasien")) & "', lukamar  = '" & FixDouble(drutama("lukamar")) & "', luposting  = 0, lujenisbilling = " & drutama("lujenisbilling") & ", lupetugas = " & drutama("lupetugas") & " where luid = '" & drutama("luid") & "'"
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

                    If drutama("luautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        'Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("lucabang"), drutama("lulokasi"), drutama("lusumber"), drutama("lutgl"))
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("luperawatan"), drutama("luawalankatpasien"), drutama("lusumber"), drutama("lutgl"))
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
                        notransaksi = drutama("lunotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(luid) FROM m_11_lu WHERE lunotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_11_Lu (lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lumatauang, lukurs, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20, luperawatan, lukategoripasien, lukamar, lujenisbilling, lupetugas) values('" & FixQuotes(drutama("lucabang")) & "', '" & FixQuotes(drutama("lulokasi")) & "', '" & FixQuotes(drutama("lugudang")) & "', '" & FixQuotes(drutama("lusumber")) & "', " & drutama("luautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("lutgl"))) & "', " & drutama("lukodepa") & ", " & drutama("lucustomer") & ", '" & FixQuotes(drutama("lucustomerkontak")) & "', '" & FixQuotes(drutama("luuraian")) & "', '" & FixQuotes(drutama("lucatatan")) & "', '" & FixQuotes(drutama("lunoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("lutglnoref"))) & "', '" & FixQuotes(drutama("lumatauang")) & "', '" & FixDouble(drutama("lukurs")) & "', '" & FixDouble(drutama("lutotaltransaksi")) & "', " & drutama("luidkj") & ", " & drutama("lustatusrealisasi") & ", " & drutama("lustatus") & ", " & drutama("lustatussebelumnya") & ", " & drutama("lujmlrevisi") & ", " & drutama("lucetakanke") & ", " & drutama("luinputuser") & ", NOW(), " & drutama("lumodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("luisclose") & ", '" & FixQuotes(drutama("lucustomtext1")) & "', '" & FixQuotes(drutama("lucustomtext2")) & "', '" & FixQuotes(drutama("lucustomtext3")) & "', '" & FixQuotes(drutama("lucustomtext4")) & "', '" & FixQuotes(drutama("lucustomtext5")) & "', '" & FixQuotes(drutama("lucustomtext6")) & "', '" & FixQuotes(drutama("lucustomtext7")) & "', '" & FixQuotes(drutama("lucustomtext8")) & "', '" & FixQuotes(drutama("lucustomtext9")) & "', '" & FixQuotes(drutama("lucustomtext10")) & "', '" & FixQuotes(drutama("lucustomtext11")) & "', '" & FixQuotes(drutama("lucustomtext12")) & "', '" & FixQuotes(drutama("lucustomtext13")) & "', '" & FixQuotes(drutama("lucustomtext14")) & "', '" & FixQuotes(drutama("lucustomtext15")) & "', '" & FixQuotes(drutama("lucustomtext16")) & "', '" & FixQuotes(drutama("lucustomtext17")) & "', '" & FixQuotes(drutama("lucustomtext18")) & "', '" & FixQuotes(drutama("lucustomtext19")) & "', '" & FixQuotes(drutama("lucustomtext20")) & "', " & drutama("lucustomint1") & ", " & drutama("lucustomint2") & ", " & drutama("lucustomint3") & ", " & drutama("lucustomint4") & ", " & drutama("lucustomint5") & ", " & drutama("lucustomint6") & ", " & drutama("lucustomint7") & ", " & drutama("lucustomint8") & ", " & drutama("lucustomint9") & ", " & drutama("lucustomint10") & ", " & drutama("lucustomint11") & ", " & drutama("lucustomint12") & ", " & drutama("lucustomint13") & ", " & drutama("lucustomint14") & ", " & drutama("lucustomint15") & ", " & drutama("lucustomint16") & ", " & drutama("lucustomint17") & ", " & drutama("lucustomint18") & ", " & drutama("lucustomint19") & ", " & drutama("lucustomint20") & ", '" & FixDouble(drutama("lucustomdbl1")) & "', '" & FixDouble(drutama("lucustomdbl2")) & "', '" & FixDouble(drutama("lucustomdbl3")) & "', '" & FixDouble(drutama("lucustomdbl4")) & "', '" & FixDouble(drutama("lucustomdbl5")) & "', '" & FixDouble(drutama("lucustomdbl6")) & "', '" & FixDouble(drutama("lucustomdbl7")) & "', '" & FixDouble(drutama("lucustomdbl8")) & "', '" & FixDouble(drutama("lucustomdbl9")) & "', '" & FixDouble(drutama("lucustomdbl10")) & "', '" & FixDouble(drutama("lucustomdbl11")) & "', '" & FixDouble(drutama("lucustomdbl12")) & "', '" & FixDouble(drutama("lucustomdbl13")) & "', '" & FixDouble(drutama("lucustomdbl14")) & "', '" & FixDouble(drutama("lucustomdbl15")) & "', '" & FixDouble(drutama("lucustomdbl16")) & "', '" & FixDouble(drutama("lucustomdbl17")) & "', '" & FixDouble(drutama("lucustomdbl18")) & "', '" & FixDouble(drutama("lucustomdbl19")) & "', '" & FixDouble(drutama("lucustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lucustomdate20"))) & "', '" & FixDouble(drutama("luperawatan")) & "', '" & FixDouble(drutama("lukategoripasien")) & "', '" & FixDouble(drutama("lukamar")) & "', " & drutama("lujenisbilling") & ", " & drutama("lupetugas") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select luid from M_11_lu where lunotransaksi='" & notransaksi & "' AND luinputuser= '" & userid & "' order by lumodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_11_Lu_Detail where idlu = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idludetail") & ", " & result(4) & ", '" & FixQuotes(dr1("jenis")) & "', " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idkjdetail") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("iddokter")) & "', '" & FixQuotes(dr1("namadokter")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', '" & FixQuotes(dr1("customtext11")) & "', '" & FixQuotes(dr1("customtext12")) & "', '" & FixQuotes(dr1("customtext13")) & "', '" & FixQuotes(dr1("customtext14")) & "', '" & FixQuotes(dr1("customtext15")) & "', '" & FixQuotes(dr1("customtext16")) & "', '" & FixQuotes(dr1("customtext17")) & "', '" & FixQuotes(dr1("customtext18")) & "', '" & FixQuotes(dr1("customtext19")) & "', '" & FixQuotes(dr1("customtext20")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixDouble(dr1("customdbl11")) & "', '" & FixDouble(dr1("customdbl12")) & "', '" & FixDouble(dr1("customdbl13")) & "', '" & FixDouble(dr1("customdbl14")) & "', '" & FixDouble(dr1("customdbl15")) & "', '" & FixDouble(dr1("customdbl16")) & "', '" & FixDouble(dr1("customdbl17")) & "', '" & FixDouble(dr1("customdbl18")) & "', '" & FixDouble(dr1("customdbl19")) & "', '" & FixDouble(dr1("customdbl20")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate11"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate12"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate13"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate14"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate15"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate16"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate17"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate18"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate19"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate20"))) & "')")
                    Next
                    sql = "Insert into M_11_lu_Detail(idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20) values" & strValue2.ToString & ""
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

                If drutama("lustatus") = 2 Then
                    'If Len(updNilai) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m_11_lu_detail SET jmlrealisasi = (CASE idkjdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = myconn
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()

                    '    'UPDATE UTAMA
                    '    Dim ftDetail As String = "", statusOut As Integer = 0
                    '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idkj FROM m_11_kj_detail WHERE " & updFilter & " GROUP BY idkj")
                    '    If dtOut.Rows.Count > 0 Then
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                    '            ftDetail = String.Concat(ftDetail, "(idkj = '" & dr1("idkj") & "')")
                    '        Next
                    '    End If
                    '    dtOut = AsDataTableAmbilDariDBCon("SELECT idkj, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m_11_kj_detail WHERE " & ftDetail & " GROUP BY idkj")
                    '    If dtOut.Rows.Count > 0 Then
                    '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                    '        updNilai = "" : updFilter = ""
                    '        For Each dr1 As DataRow In dtOut.Rows
                    '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                    '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                    '                statusOut = 2
                    '            ElseIf dr1("jmlrealisasi") < 1 Then
                    '                statusOut = 0
                    '            Else
                    '                statusOut = 1
                    '            End If
                    '            '2. SET NILAI UPDATE OUTSTANDING
                    '            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idkj") & "' THEN '" & statusOut & "' ")
                    '            '3. SET FILTERUPDATE OUTSTANDING
                    '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                    '            updFilter = String.Concat(updFilter, "(kjid = '" & dr1("idkj") & "')")
                    '        Next

                    '        sql = "UPDATE m_11_kj SET kjstatusrealisasi = (CASE kjid " & updNilai & " ELSE kjstatusrealisasi END) WHERE " & updFilter
                    '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '        With objCmd
                    '            .Connection = myconn
                    '            .Transaction = Trans
                    '            .CommandType = CommandType.Text
                    '            .CommandText = sql
                    '        End With
                    '        objCmd.ExecuteNonQuery()
                    '    End If
                    '    'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    'End If

                    If drutama("lujenisbilling") = 1 Then
                        Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatus, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("luidkj") & "'", myConn)
                        Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                        If cekKunjungan > 0 Then
                            sql = "Update M_11_Kj set kjstatus = 3 where kjid = '" & drutama("luidkj") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        If drutama("lukamar") <> "" Then
                            If cekKunjungan > 0 Then
                                sql = "Update M_11_Kj set kjkamar = '" & drutama("lukamar") & "' where kjid = '" & drutama("luidkj") & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If
                        End If
                    End If
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "LU", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("lustatus") = 2 Then
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
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                    End If

                End If
                'END OF INSERT MSMQ JURNAL ==========================================================

                'INSERT USER LOG =====================================================================
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
    Public Function M11_LuUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("lunotransaksikj", "kj.kjnotransaksi")
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
            Dim sumber As String = "Lu", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Lutgl, Lunotransaksi, Lustatus, Luidkj FROM M_11_Lu WHERE Luid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'idkj
                idkj = Integer.Parse(FxDB(dtdetail(1)(3), 0))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Lustatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m11_lu_history
            'Dim luSimpanHistory As String = SimpanHistory.m11_Lu_HistorySimpan("" & paramSplit(0) & "★M11_Lu_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim luSplit() As String = luSimpanHistory.Split(sptParam)
            'Dim luSplitResult() As String = luSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (luSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & luSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m5_so_terkait("luid = '" & idtransaksi & "'")

                sql = query.PanggilQuery("m11_lu_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                'BUKA KONEKSI
                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE STATUS KJ ===============================================================
                'CEK TRANSAKSI TERKAIT KJ
                sql = "  SELECT * FROM ( "
                sql &= " SELECT a.akid as idterkait, a.aksumber as sumber FROM m_11_ak a JOIN m_11_kj kj ON a.akidkj = kj.kjid AND a.akstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kmid as idterkait, a.kmsumber as sumber FROM m_11_km a JOIN m_11_kj kj ON a.kmidkj = kj.kjid AND a.kmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kwid as idterkait, a.kwsumber as sumber FROM m_11_kw a JOIN m_11_kw_detail b ON a.kwid = b.idkw AND a.kwstatus IN(2,3,4,7) JOIN m_11_kj kj ON b.sumber = 'KJ' AND b.idtransaksi = kj.kjid AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.lbid as idterkait, a.lbsumber as sumber FROM m_11_lb a JOIN m_11_kj kj ON a.lbidkj = kj.kjid AND a.lbstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.luid as idterkait, a.lusumber as sumber FROM m_11_lu a JOIN m_11_kj kj ON a.luidkj = kj.kjid AND a.lustatus IN(2,3,4,7) AND a.luid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rkid as idterkait, a.rksumber as sumber FROM m_11_rk a JOIN m_11_kj kj ON a.rkidkj = kj.kjid AND a.rkstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rmid as idterkait, 'RM' as sumber FROM m_11_rm a JOIN m_11_kj kj ON a.rmidkj = kj.kjid AND a.rmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.roid as idterkait, a.rosumber as sumber FROM m_11_ro a JOIN m_11_kj kj ON a.roidkj = kj.kjid AND a.rostatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " ) as terkait "
                sql &= " ORDER BY terkait.sumber = 'KW' DESC, terkait.sumber ASC "
                dtdetail = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtdetail.Rows.Count > 0 Then
                    'JIKA KJ MEMILIKI TRANSAKSI TERKAIT
                    If FxDB(dtdetail.Rows(0)("sumber"), "").ToUpper.Equals("KW") Then
                        'JIKA ADA KJ TERKAIT KW MAKA STATUS KJ = 4 (COMPLETE)
                        sql = "UPDATE M_11_Kj SET kjstatus = 4 WHERE kjid = '" & FixDouble(idkj) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        'JIKA ADA KJ TERKAIT SELAIN KW MAKA STATUS KJ = 3 (INPROGRESS)
                        sql = "UPDATE M_11_Kj SET kjstatus = 3 WHERE kjid = '" & FixDouble(idkj) & "'"
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
                    'JIKA KJ TIDAK MEMILIKI TRANSAKSI TERKAIT, STATUS KJ = 2 (APPROVED)
                    sql = "UPDATE M_11_Kj SET kjstatus = 2 WHERE kjid = '" & FixDouble(idkj) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                End If
                'END OF UPDATE STATUS KJ ========================================================


                'Dim idlayanan As Integer = 0, jmltotal As Double = 0, idkjdetail As Integer = 0
                'Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

                ''AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT jenis, idlayanan, namalayanan, satuan, nilaisatuan, jmltotal, gudang, idkjdetail, urutan FROM m11_lu_detail WHERE idlu = '" & idtransaksi & "'")
                'If dtdetail.Rows.Count > 0 Then
                '    For Each dr1 As DataRow In dtdetail.Rows
                '        'BUAT FILTER UNTUK UPDATE ---------------------------------
                '        idlayanan = dr1("idlayanan") : jmltotal = dr1("jmltotal") : gudang = dr1("gudang") : idkjdetail = dr1("idkjdetail")

                '        'UPDATE OUTSTANDING ---------------------------
                '        If idkjdetail <> 0 Then
                '            '1. SET NILAI UPDATE OUTSTANDING
                '            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmltotal", "idkjdetail=" & idkjdetail)
                '            updNilai = String.Concat("WHEN '" & idkjdetail & "' THEN jmlrealisasi - '" & Outstanding & "' ", updNilai)

                '            '2. SET FILTERUPDATE OUTSTANDING
                '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '            updFilter = String.Concat(updFilter, "(idkjdetail = '" & idkjdetail & "')")
                '        End If

                '        ''3. SET NILAI UPDATE STOK KELUAR -------------
                '        'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                '        'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                '        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                '    Next
                'Else
                '    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                'End If

                'If Len(updFilter) > 0 Then
                '    'UPDATE OUTSTANDING DETAIL ----------------------
                '    sql = "UPDATE m11_kj_detail SET jmlrealisasi = (CASE idkjdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                '    'END OF UPDATE OUTSTANDING DETAIL ---------------

                '    'UPDATE OUTSTANDING UTAMA -----------------------
                '    Dim ftDetail As String = "", statusOut As Integer = 0
                '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idkj FROM m11_kj_detail WHERE " & updFilter & " GROUP BY idkj")
                '    If dtOut.Rows.Count > 0 Then
                '        For Each dr1 As DataRow In dtOut.Rows
                '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                '            ftDetail = String.Concat(ftDetail, "(idkj = '" & dr1("idkj") & "')")
                '        Next
                '    End If
                '    dtOut = AsDataTableAmbilDariDBCon("SELECT idkj, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                '    If dtOut.Rows.Count > 0 Then
                '        'KOSONGKAN VARIABEL NILAI DAN FILTER
                '        updNilai = "" : updFilter = ""
                '        For Each dr1 As DataRow In dtOut.Rows
                '            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                '            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                '                statusOut = 2
                '            ElseIf dr1("jmlrealisasi") < 1 Then
                '                statusOut = 0
                '            Else
                '                statusOut = 1
                '            End If
                '            '2. SET NILAI UPDATE OUTSTANDING
                '            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                '            '3. SET FILTERUPDATE OUTSTANDING
                '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                '        Next

                '        sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = myconn
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    End If
                '    'END OF UPDATE OUTSTANDING UTAMA ----------------
                'End If

                ''UPDATE STOK BOOKING ================================
                ''BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                'sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang * -1 FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()

                'If Len(updStokBooking) > 0 Then
                '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                'END OF UPDATE STOK BOOKING =========================

                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'LU' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            'JIKA CLOSE MAKA KURANGI STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            'If jnsaktivitas = 7 Then
            '    'KURANGI STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
            '    sql = "  UPDATE m1_item_booking ib"
            '    sql &= " JOIN"
            '    sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
            '    sql &= " FROM ( "
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_do `do` "
            '    sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " UNION ALL"
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_si si "
            '    sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " ) as detail"
            '    sql &= " GROUP BY idsodetail"
            '    sql &= " ) sod  ON ib.idbarang = sod.idbarang"
            '    sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
            '    sql &= " SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.realisasi)"
            '    sql &= " WHERE sod.jmlbarang <> sod.realisasi"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myconn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()

            '    'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
            '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    'With objCmd
            '    '    .Connection = myconn
            '    '    .Transaction = Trans
            '    '    .CommandType = CommandType.Text
            '    '    .CommandText = sql
            '    'End With
            '    'objCmd.ExecuteNonQuery()
            'End If

            ''JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            'If jnsaktivitas = 17 Then
            '    'TAMBAH STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
            '    sql = "  UPDATE m1_item_booking ib"
            '    sql &= " JOIN"
            '    sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
            '    sql &= " FROM ( "
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_do `do` "
            '    sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " UNION ALL"
            '    sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
            '    sql &= " FROM m5_si si "
            '    sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
            '    sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
            '    sql &= " WHERE "
            '    sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
            '    sql &= " GROUP BY sod.idsodetail)"
            '    sql &= " ) as detail"
            '    sql &= " GROUP BY idsodetail"
            '    sql &= " ) sod  ON ib.idbarang = sod.idbarang"
            '    sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
            '    sql &= " SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.realisasi)"
            '    sql &= " WHERE sod.jmlbarang <> sod.realisasi"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = myconn
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()

            '    'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
            '    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    'With objCmd
            '    '    .Connection = myconn
            '    '    .Transaction = Trans
            '    '    .CommandType = CommandType.Text
            '    '    .CommandText = sql
            '    'End With
            '    'objCmd.ExecuteNonQuery()
            'End If

            'update status utama
            sql = "UPDATE M_11_Lu SET Lustatus = " & nilaiStatus & ", Lumodifikasiuser='" & userid & "', Lumodifikasitgl = NOW(), Lujmlrevisi = Lujmlrevisi + 1 WHERE Luid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_LuSearch(PostWsSearch(paramSplit(0), "M11_LuSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_LuDelete(ByVal param As String) As String

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
            Dim sumber As String = "Lu", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Luid, Lunotransaksi FROM M_11_Lu WHERE Luid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT lucabang, lulokasi, lusumber, luautonotransaksi, lunotransaksi, lutgl"
            sql &= " FROM M_11_lu"
            sql &= " WHERE luid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("lucabang")
                lokasi = dtNomorNext.Rows(0)("lulokasi")
                sumber = dtNomorNext.Rows(0)("lusumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("luautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("lunotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("lutgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_11_Lu_Detail WHERE idlu = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_11_Lu WHERE luid = '" & idtransaksi & "'"
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
            'Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_SoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
            'Dim hasilSearch As New RsHasilWsSearch
            'hasilSearch = GetWsSearch(paramSearch)

            'result(1) = hasilSearch.success
            'result(2) = hasilSearch.errmessage

            'resultPaging(0) = hasilSearch.isPaging
            'resultPaging(1) = hasilSearch.isNext
            'resultPaging(2) = hasilSearch.isPrevious
            'resultPaging(3) = hasilSearch.countPage
            'resultPaging(4) = hasilSearch.countRow

            'search = hasilSearch.data
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
    Public Function M11_LuGetdataById(ByVal param As String) As String
        'M11_Lu_GetdataById Utama --------------------------------------------------------
        'luid, lucabang, lulokasi, lugudang, lusumber, 
        'luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, 
        'lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, 
        'lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, 
        'lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, 
        'lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, 
        'lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8,
        'lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13,
        'lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18,
        'lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3,
        'lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8,
        'lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13,
        'lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18,
        'lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, 
        'lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8,
        'lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13,
        'lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18,
        'lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, 
        'lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8,
        'lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13,
        'lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18,
        'lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama, 
        'lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, 
        'luinputusernama, lumodifikasiusernama, lumatauang, lukurs, luposting
        'lupostingtgl

        'M11_Lu_GetdataById Detail --------------------------------------------------------
        'idludetail, idlu, jenis, idlayanan, namalayanan, 
        'jml, satuan, nilaisatuan, jmltotal, satuandefault, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudang, 
        'costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, 
        'iddokter, namadokter, customtext1, customtext2, customtext3, 
        'customtext4, customtext5, customtext6, customtext7, customtext8,
        'customtext9, customtext10, customtext11, customtext12, customtext13,
        'customtext14, customtext15, customtext16, customtext17, customtext18,
        'customtext19, customtext20, customdbl1, customdbl2, customdbl3, 
        'customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,
        'customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,
        'customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,
        'customdbl19, customdbl20, customdate1, customdate2, customdate3, 
        'customdate4, customdate5, customdate6, customdate7, customdate8,
        'customdate9, customdate10, customdate11, customdate12, customdate13,
        'customdate14, customdate15, customdate16, customdate17, customdate18,
        'customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi,
        'kodedokter, matauang, kurs, rekpersediaan, rekhargapokok
        'rekdiskonpenjualan, rekpenjualan

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

        Dim NmMemcached As String = "aplikasi1-M11_Lu~M11_Lu_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "luid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "luid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lu_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("luid"), 0), sptField,
                     FxDB(drutama("lucabang"), ""), sptField,
                     FxDB(drutama("lulokasi"), ""), sptField,
                     FxDB(drutama("lugudang"), ""), sptField,
                     FxDB(drutama("lusumber"), ""), sptField,
                     FxDB(drutama("luautonotransaksi"), 0), sptField,
                     FxDB(drutama("lunotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lutgl"), ""), formatTgl), sptField,
                     FxDB(drutama("lukodepa"), 0), sptField,
                     FxDB(drutama("lucustomer"), 0), sptField,
                     FxDB(drutama("lucustomerkontak"), ""), sptField,
                     FxDB(drutama("luuraian"), ""), sptField,
                     FxDB(drutama("lucatatan"), ""), sptField,
                     FxDB(drutama("lunoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lutglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("lutotaltransaksi"), 0), sptField,
                     FxDB(drutama("luidkj"), 0), sptField,
                     FxDB(drutama("lustatusrealisasi"), 0), sptField,
                     FxDB(drutama("lustatus"), 0), sptField,
                     FxDB(drutama("lustatussebelumnya"), 0), sptField,
                     FxDB(drutama("lujmlrevisi"), 0), sptField,
                     FxDB(drutama("lucetakanke"), 0), sptField,
                     FxDB(drutama("luinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("luinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("lumodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lumodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("luisclose"), 0), sptField,
                     FxDB(drutama("lucustomtext1"), ""), sptField,
                     FxDB(drutama("lucustomtext2"), ""), sptField,
                     FxDB(drutama("lucustomtext3"), ""), sptField,
                     FxDB(drutama("lucustomtext4"), ""), sptField,
                     FxDB(drutama("lucustomtext5"), ""), sptField,
                     FxDB(drutama("lucustomtext6"), ""), sptField,
                     FxDB(drutama("lucustomtext7"), ""), sptField,
                     FxDB(drutama("lucustomtext8"), ""), sptField,
                     FxDB(drutama("lucustomtext9"), ""), sptField,
                     FxDB(drutama("lucustomtext10"), ""), sptField,
                     FxDB(drutama("lucustomtext11"), ""), sptField,
                     FxDB(drutama("lucustomtext12"), ""), sptField,
                     FxDB(drutama("lucustomtext13"), ""), sptField,
                     FxDB(drutama("lucustomtext14"), ""), sptField,
                     FxDB(drutama("lucustomtext15"), ""), sptField,
                     FxDB(drutama("lucustomtext16"), ""), sptField,
                     FxDB(drutama("lucustomtext17"), ""), sptField,
                     FxDB(drutama("lucustomtext18"), ""), sptField,
                     FxDB(drutama("lucustomtext19"), ""), sptField,
                     FxDB(drutama("lucustomtext20"), ""), sptField,
                     FxDB(drutama("lucustomint1"), 0), sptField,
                     FxDB(drutama("lucustomint2"), 0), sptField,
                     FxDB(drutama("lucustomint3"), 0), sptField,
                     FxDB(drutama("lucustomint4"), 0), sptField,
                     FxDB(drutama("lucustomint5"), 0), sptField,
                     FxDB(drutama("lucustomint6"), 0), sptField,
                     FxDB(drutama("lucustomint7"), 0), sptField,
                     FxDB(drutama("lucustomint8"), 0), sptField,
                     FxDB(drutama("lucustomint9"), 0), sptField,
                     FxDB(drutama("lucustomint10"), 0), sptField,
                     FxDB(drutama("lucustomint11"), 0), sptField,
                     FxDB(drutama("lucustomint12"), 0), sptField,
                     FxDB(drutama("lucustomint13"), 0), sptField,
                     FxDB(drutama("lucustomint14"), 0), sptField,
                     FxDB(drutama("lucustomint15"), 0), sptField,
                     FxDB(drutama("lucustomint16"), 0), sptField,
                     FxDB(drutama("lucustomint17"), 0), sptField,
                     FxDB(drutama("lucustomint18"), 0), sptField,
                     FxDB(drutama("lucustomint19"), 0), sptField,
                     FxDB(drutama("lucustomint20"), 0), sptField,
                     FxDB(drutama("lucustomdbl1"), 0), sptField,
                     FxDB(drutama("lucustomdbl2"), 0), sptField,
                     FxDB(drutama("lucustomdbl3"), 0), sptField,
                     FxDB(drutama("lucustomdbl4"), 0), sptField,
                     FxDB(drutama("lucustomdbl5"), 0), sptField,
                     FxDB(drutama("lucustomdbl6"), 0), sptField,
                     FxDB(drutama("lucustomdbl7"), 0), sptField,
                     FxDB(drutama("lucustomdbl8"), 0), sptField,
                     FxDB(drutama("lucustomdbl9"), 0), sptField,
                     FxDB(drutama("lucustomdbl10"), 0), sptField,
                     FxDB(drutama("lucustomdbl11"), 0), sptField,
                     FxDB(drutama("lucustomdbl12"), 0), sptField,
                     FxDB(drutama("lucustomdbl13"), 0), sptField,
                     FxDB(drutama("lucustomdbl14"), 0), sptField,
                     FxDB(drutama("lucustomdbl15"), 0), sptField,
                     FxDB(drutama("lucustomdbl16"), 0), sptField,
                     FxDB(drutama("lucustomdbl17"), 0), sptField,
                     FxDB(drutama("lucustomdbl18"), 0), sptField,
                     FxDB(drutama("lucustomdbl19"), 0), sptField,
                     FxDB(drutama("lucustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lucustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("lucabangnama"), ""), sptField,
                     FxDB(drutama("lulokasinama"), ""), sptField,
                     FxDB(drutama("lugudangnama"), ""), sptField,
                     FxDB(drutama("lucustomerkode"), ""), sptField,
                     FxDB(drutama("lucustomernama"), ""), sptField,
                     FxDB(drutama("lunotransaksikj"), ""), sptField,
                     FxDB(drutama("lustatusnama"), ""), sptField,
                     FxDB(drutama("lustatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("luinputusernama"), ""), sptField,
                     FxDB(drutama("lumodifikasiusernama"), ""), sptField,
                     FxDB(drutama("lumatauang"), ""), sptField,
                     FxDB(drutama("lukurs"), 0), sptField,
                     FxDB(drutama("luposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lupostingtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("lutingkatjual"), 0), sptField,
                     FxDB(drutama("luperawatan"), ""), sptField,
                     FxDB(drutama("lukategoripasien"), ""), sptField,
                     FxDB(drutama("lukamar"), ""), sptField,
                     FxDB(drutama("lukategoripasiennama"), ""), sptField,
                     FxDB(drutama("lukamarnama"), ""), sptField,
                     FxDB(drutama("luawalankatpasien"), ""), sptField,
                     FxDB(drutama("lujenisbilling"), 0), sptField,
                     FxDB(drutama("lupetugas"), 0), sptField,
                     FxDB(drutama("lupetugaskode"), ""), sptField,
                     FxDB(drutama("lupetugasnama"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idludetail"), 0), sptField,
                     FxDB(dr("idlu"), 0), sptField,
                     FxDB(dr("jenis"), ""), sptField,
                     FxDB(dr("idlayanan"), 0), sptField,
                     FxDB(dr("namalayanan"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmltotal"), 0), sptField,
                     FxDB(dr("satuandefault"), ""), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idkjdetail"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("iddokter"), ""), sptField,
                     FxDB(dr("namadokter"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customtext11"), ""), sptField,
                     FxDB(dr("customtext12"), ""), sptField,
                     FxDB(dr("customtext13"), ""), sptField,
                     FxDB(dr("customtext14"), ""), sptField,
                     FxDB(dr("customtext15"), ""), sptField,
                     FxDB(dr("customtext16"), ""), sptField,
                     FxDB(dr("customtext17"), ""), sptField,
                     FxDB(dr("customtext18"), ""), sptField,
                     FxDB(dr("customtext19"), ""), sptField,
                     FxDB(dr("customtext20"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     FxDB(dr("customdbl11"), 0), sptField,
                     FxDB(dr("customdbl12"), 0), sptField,
                     FxDB(dr("customdbl13"), 0), sptField,
                     FxDB(dr("customdbl14"), 0), sptField,
                     FxDB(dr("customdbl15"), 0), sptField,
                     FxDB(dr("customdbl16"), 0), sptField,
                     FxDB(dr("customdbl17"), 0), sptField,
                     FxDB(dr("customdbl18"), 0), sptField,
                     FxDB(dr("customdbl19"), 0), sptField,
                     FxDB(dr("customdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("cabangnama"), ""), sptField,
                     FxDB(dr("lokasinama"), ""), sptField,
                     FxDB(dr("gudangnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kjnotransaksi"), ""), sptField,
                     FxDB(dr("kodedokter"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekpenjualan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama,  lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama, lumatauang, lukurs, luposting, lupostingtgl, lutingkatjual, luperawatan, lukategoripasien, lukamar, lukategoripasiennama, lukamarnama, luawalankatpasien, lujenisbilling, lupetugas, lupetugaskode, lupetugasnama" & sptSubParam & "idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_LuSearch(ByVal param As String) As String
        'M11_LuSearch --------------------------------------------------------
        'luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, 
        'lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, 
        'lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, 
        'lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucabangnama, 
        'lulokasinama, lugudangnama, lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, 
        'luinputusernama, lumodifikasiusernama, lupetugas, lupetugaskode, lupetugasnama

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
            Filter = Filter.Replace("lunotransaksikj", "kj.kjnotransaksi")
            Filter = Filter.Replace("lunorm", "c1.pkode")
            Filter = Filter.Replace("lunama", "c1.pnama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lu_v")

        dt = AmbilData("aplikasi1-M11_lu_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("luid"), 0), sptField,
                     FxDB(dr("lucabang"), ""), sptField,
                     FxDB(dr("lulokasi"), ""), sptField,
                     FxDB(dr("lugudang"), ""), sptField,
                     FxDB(dr("lusumber"), ""), sptField,
                     FxDB(dr("luautonotransaksi"), 0), sptField,
                     FxDB(dr("lunotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lutgl"), ""), formatTgl), sptField,
                     FxDB(dr("lukodepa"), 0), sptField,
                     FxDB(dr("lucustomer"), 0), sptField,
                     FxDB(dr("lucustomerkontak"), ""), sptField,
                     FxDB(dr("luuraian"), ""), sptField,
                     FxDB(dr("lucatatan"), ""), sptField,
                     FxDB(dr("lunoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lutglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("lutotaltransaksi"), 0), sptField,
                     FxDB(dr("luidkj"), 0), sptField,
                     FxDB(dr("lustatusrealisasi"), 0), sptField,
                     FxDB(dr("lustatus"), 0), sptField,
                     FxDB(dr("lustatussebelumnya"), 0), sptField,
                     FxDB(dr("lujmlrevisi"), 0), sptField,
                     FxDB(dr("lucetakanke"), 0), sptField,
                     FxDB(dr("luinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("luinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lumodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("lumodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("luisclose"), 0), sptField,
                     FxDB(dr("lucabangnama"), ""), sptField,
                     FxDB(dr("lulokasinama"), ""), sptField,
                     FxDB(dr("lugudangnama"), ""), sptField,
                     FxDB(dr("lucustomerkode"), ""), sptField,
                     FxDB(dr("lucustomernama"), ""), sptField,
                     FxDB(dr("lunotransaksikj"), ""), sptField,
                     FxDB(dr("lustatusnama"), ""), sptField,
                     FxDB(dr("lustatussebelumnyanama"), ""), sptField,
                     FxDB(dr("luinputusernama"), ""), sptField,
                     FxDB(dr("lumodifikasiusernama"), ""), sptField,
                     FxDB(dr("lupetugas"), 0), sptField,
                     FxDB(dr("lupetugaskode"), ""), sptField,
                     FxDB(dr("lupetugasnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucabangnama, lulokasinama, lugudangnama, lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama, lupetugas, lupetugaskode, lupetugasnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_LuTerkait(ByVal param As String) As String
        'M11_LuTerkait --------------------------------------------------------
        'luid, lunotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "soid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

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
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lu_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_lu_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("luid"), 0), sptField,
                     FxDB(dr("lunotransaksi"), ""), sptField,
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
            result(2) = "Related LU data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("luid, lunotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_Lu_Detail_VSearch(ByVal param As String) As String
        'M11_Lu_Detail_VSearch --------------------------------------------------------
        'idludetail, idlu, jenis, idlayanan, namalayanan, 
        'jml, satuan, nilaisatuan, jmltotal, satuandefault,
        'harga, diskon, jmldiskon, pajak1, jmlpajak1,
        'pajak2, jmlpajak2, cabang, lokasi, gudang,
        'costcenter, divisi, subdivisi, proyek, catatan,
        'urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose,
        'iddokter, namadokter, customtext1, customtext2, customtext3,
        'customtext4, customtext5, customtext6, customtext7, customtext8,
        'customtext9, customtext10, customtext11, customtext12, customtext13,
        'customtext14, customtext15, customtext16, customtext17, customtext18,
        'customtext19, customtext20, customdbl1, customdbl2, customdbl3,
        'customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,
        'customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,
        'customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,
        'customdbl19, customdbl20, customdate1, customdate2, customdate3,
        'customdate4, customdate5, customdate6, customdate7, customdate8,
        'customdate9, customdate10, customdate11, customdate12, customdate13,
        'customdate14, customdate15, customdate16, customdate17, customdate18,
        'customdate19, customdate20, lunotransaksi, luuraian, lucatatan,
        'lunoref, lutgl, lutglnoref, lucustomerkontak, kodelayanan,
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,
        'lucustomer, lucustomerkode, lucustomernama, kodedokter

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sol As String = ""

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
            Filter = Filter.Replace("idlayanan", "lud.idlayanan")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sol = query.PanggilQuery("m11_lu_detail_v")

        dt = AmbilData("aplikasi1-M11_lu_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idludetail"), 0), sptField,
                     FxDB(dr("idlu"), 0), sptField,
                     FxDB(dr("jenis"), ""), sptField,
                     FxDB(dr("idlayanan"), 0), sptField,
                     FxDB(dr("namalayanan"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmltotal"), 0), sptField,
                     FxDB(dr("satuandefault"), ""), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudang"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idkjdetail"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
                     FxDB(dr("isclose"), 0), sptField,
                     FxDB(dr("iddokter"), ""), sptField,
                     FxDB(dr("namadokter"), ""), sptField,
                     FxDB(dr("customtext1"), ""), sptField,
                     FxDB(dr("customtext2"), ""), sptField,
                     FxDB(dr("customtext3"), ""), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptField,
                     FxDB(dr("customtext6"), ""), sptField,
                     FxDB(dr("customtext7"), ""), sptField,
                     FxDB(dr("customtext8"), ""), sptField,
                     FxDB(dr("customtext9"), ""), sptField,
                     FxDB(dr("customtext10"), ""), sptField,
                     FxDB(dr("customtext11"), ""), sptField,
                     FxDB(dr("customtext12"), ""), sptField,
                     FxDB(dr("customtext13"), ""), sptField,
                     FxDB(dr("customtext14"), ""), sptField,
                     FxDB(dr("customtext15"), ""), sptField,
                     FxDB(dr("customtext16"), ""), sptField,
                     FxDB(dr("customtext17"), ""), sptField,
                     FxDB(dr("customtext18"), ""), sptField,
                     FxDB(dr("customtext19"), ""), sptField,
                     FxDB(dr("customtext20"), ""), sptField,
                     FxDB(dr("customdbl1"), 0), sptField,
                     FxDB(dr("customdbl2"), 0), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customdbl6"), 0), sptField,
                     FxDB(dr("customdbl7"), 0), sptField,
                     FxDB(dr("customdbl8"), 0), sptField,
                     FxDB(dr("customdbl9"), 0), sptField,
                     FxDB(dr("customdbl10"), 0), sptField,
                     FxDB(dr("customdbl11"), 0), sptField,
                     FxDB(dr("customdbl12"), 0), sptField,
                     FxDB(dr("customdbl13"), 0), sptField,
                     FxDB(dr("customdbl14"), 0), sptField,
                     FxDB(dr("customdbl15"), 0), sptField,
                     FxDB(dr("customdbl16"), 0), sptField,
                     FxDB(dr("customdbl17"), 0), sptField,
                     FxDB(dr("customdbl18"), 0), sptField,
                     FxDB(dr("customdbl19"), 0), sptField,
                     FxDB(dr("customdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
                     FxDB(dr("lunotransaksi"), ""), sptField,
                     FxDB(dr("luuraian"), ""), sptField,
                     FxDB(dr("lucatatan"), ""), sptField,
                     FxDB(dr("lunoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lutgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("lutglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("lucustomerkontak"), ""), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("lucustomer"), ""), sptField,
                     FxDB(dr("lucustomerkode"), ""), sptField,
                     FxDB(dr("lucustomernama"), ""), sptField,
                     FxDB(dr("kodedokter"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3,customtext4, customtext5, customtext6, customtext7, customtext8,customtext9, customtext10, customtext11, customtext12, customtext13,customtext14, customtext15, customtext16, customtext17, customtext18,customtext19, customtext20, customdbl1, customdbl2, customdbl3,customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,customdbl19, customdbl20, customdate1, customdate2, customdate3,customdate4, customdate5, customdate6, customdate7, customdate8,customdate9, customdate10, customdate11, customdate12, customdate13,customdate14, customdate15, customdate16, customdate17, customdate18,customdate19, customdate20, lunotransaksi, luuraian, lucatatan, lunoref, lutgl, lutglnoref, lucustomerkontak, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,lucustomer, lucustomerkode, lucustomernama, kodedokter"))

        Return wsResult
    End Function

End Class