Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_ro
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_RoSimpan(ByVal param As String) As String
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
        'akid(0) As Integer, akcabang(1) As String, aklokasi(2) As String, akgudang(3) As String, aksumber(4) As String, 
        'akautonotransaksi(5) As Integer, aknotransaksi(6) As String, aktgl(7) As Date, akkodepa(8) As Integer, akcustomer(9) As Integer,
        'akcustomerkontak(10) As String, akuraian(11) As String, akcatatan(12) As String, aknoref(13) As String, aktglnoref(14) As Date, 
        'aktotaltransaksi(15) As Double, akidkj(16) As Integer, akstatusrealisasi(17) As Interger, akstatus(18) As Integer, akstatussebelumnya(19) As Integer, 
        'akjmlrevisi(20) As Integer, akcetakanke(21) As Integer, akinputuser(22) As Integer, akinputtgl(23) As DateTime, akmodifikasiuser(24) As Integer, 
        'akmodifikasitgl(25) As DateTime, akisclose(26) As Integer, akcustomtext1(27) As String, akcustomtext2(28) As String, akcustomtext3(29) As String, 
        'akcustomtext4(30) As String, akcustomtext5(31) As String, akcustomtext6(32) As String, akcustomtext7(33) As String, akcustomtext8(34) As String, 
        'akcustomtext9(35) As String, akcustomtext10(36) As String, akcustomtext11(37) As String, akcustomtext12(38) As String, akcustomtext13(39) As String, 
        'akcustomtext14(40) As String, akcustomtext15(41) As String, akcustomtext16(42) As String, akcustomtext17(43) As String, akcustomtext18(44) As String, 
        'akcustomtext19(45) As String, akcustomtext20(46) As String, akcustomint1(47) As Integer, akcustomint2(48) As Integer, akcustomint3(49) As Integer, 
        'akcustomint4(50) As Integer, akcustomint5(51) As Integer, akcustomint6(52) As Integer, akcustomint7(53) As Integer, akcustomint8(54) As Integer, 
        'akcustomint9(55) As Integer, akcustomint10(56) As Integer, akcustomint11(57) As Integer, akcustomint12(58) As Integer, akcustomint13(59) As Integer, 
        'akcustomint14(60) As Integer, akcustomint15(61) As Integer, akcustomint16(62) As Integer, akcustomint17(63) As Integer, akcustomint18(64) As Integer, 
        'akcustomint19(65) As Integer, akcustomint20(66) As Integer, akcustomdbl1(67) As Double, akcustomdbl2(68) As Double, akcustomdbl3(69) As Double, 
        'akcustomdbl4(70) As Double, akcustomdbl5(71) As Double, akcustomdbl6(72) As Double, akcustomdbl7(73) As Double, akcustomdbl8(74) As Double, 
        'akcustomdbl9(75) As Double, akcustomdbl10(76) As Double, akcustomdbl11(77) As Double, akcustomdbl12(78) As Double, akcustomdbl13(79) As Double, 
        'akcustomdbl14(80) As Double, akcustomdbl15(81) As Double, akcustomdbl16(82) As Double, akcustomdbl17(83) As Double, akcustomdbl18(84) As Double, 
        'akcustomdbl19(85) As Double, akcustomdbl20(86) As Double, akcustomdate1(87) As Date, akcustomdate2(88) As Date, akcustomdate3(89) As Date, 
        'akcustomdate4(90) As Date, akcustomdate5(91) As Date, akcustomdate6(92) As Date, akcustomdate7(93) As Date, akcustomdate8(94) As Date, 
        'akcustomdate9(95) As Date, akcustomdate10(96) As Date, akcustomdate11(97) As Date, akcustomdate12(98) As Date, akcustomdate13(99) As Date, 
        'akcustomdate14(100) As Date, akcustomdate15(101) As Date, akcustomdate16(102) As Date, akcustomdate17(103) As Date, akcustomdate18(104) As Date, 
        'akcustomdate19(105) As Date, akcustomdate20(106) As Date, akmatauang(107) As String, akkurs(108) As Double, akposting(109) As Integer
        'roperawatan(110) As String, rokategoripasien(111) As String, rokamar(112) As String, roawalankatpasien(113) As String


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'akid, akcabang, aklokasi, akgudang, aksumber, 
        'akautonotransaksi, aknotransaksi, aktgl, akkodepa, akcustomer,
        'akcustomerkontak, akuraian, akcatatan, aknoref, aktglnoref, 
        'aktotaltransaksi, akidkj, akstatusrealisasi, akstatus, akstatussebelumnya, 
        'akjmlrevisi, akcetakanke, akinputuser, akinputtgl, akmodifikasiuser, 
        'akmodifikasitgl, akisclose, akcustomtext1, akcustomtext2, akcustomtext3, 
        'akcustomtext4, akcustomtext5, akcustomtext6, akcustomtext7, akcustomtext8, 
        'akcustomtext9, akcustomtext10, akcustomtext11, akcustomtext12, akcustomtext13, 
        'akcustomtext14, akcustomtext15, akcustomtext16, akcustomtext17, akcustomtext18, 
        'akcustomtext19, akcustomtext20, akcustomint1, akcustomint2, akcustomint3, 
        'akcustomint4, akcustomint5, akcustomint6, akcustomint7, akcustomint8, 
        'akcustomint9, akcustomint10, akcustomint11, akcustomint12, akcustomint13, 
        'akcustomint14, akcustomint15, akcustomint16, akcustomint17, akcustomint18, 
        'akcustomint19, akcustomint20, akcustomdbl1, akcustomdbl2, akcustomdbl3, 
        'akcustomdbl4, akcustomdbl5, akcustomdbl6, akcustomdbl7, akcustomdbl8, 
        'akcustomdbl9, akcustomdbl10, akcustomdbl11, akcustomdbl12, akcustomdbl13, 
        'akcustomdbl14, akcustomdbl15, akcustomdbl16, akcustomdbl17, akcustomdbl18, 
        'akcustomdbl19, akcustomdbl20, akcustomdate1, akcustomdate2, akcustomdate3, 
        'akcustomdate4, akcustomdate5, akcustomdate6, akcustomdate7, akcustomdate8, 
        'akcustomdate9, akcustomdate10, akcustomdate11, akcustomdate12, akcustomdate13, 
        'akcustomdate14, akcustomdate15, akcustomdate16, akcustomdate17, akcustomdate18, 
        'akcustomdate19, akcustomdate20, akmatauang, akkurs, akposting
        'roperawatan, rokategoripasien, rokamar, roawalankatpasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 116) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'roid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "roid required numeric." : GoTo selesai
        End If
        'roautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "roautonotransaksi required numeric." : GoTo selesai
        End If
        'rotgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "rotgl required date." : GoTo selesai
        End If
        'rokodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rokodepa required numeric." : GoTo selesai
        End If
        'rocustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "rocustomer required numeric." : GoTo selesai
        End If
        'rotglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "rotglnoref required date." : GoTo selesai
        End If
        'rototaltransaksi(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "rototaltransaksi required numeric." : GoTo selesai
        End If
        'roidkj(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "roidkj required numeric." : GoTo selesai
        End If
        'rostatusrealisasi(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "rostatusrealisasi required numeric." : GoTo selesai
        End If
        'rostatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "rostatus required numeric." : GoTo selesai
        End If
        'rostatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "rostatussebelumnya required numeric." : GoTo selesai
        End If
        'rojmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "rojmlrevisi required numeric." : GoTo selesai
        End If
        'rocetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "rocetakanke required numeric." : GoTo selesai
        End If
        'roinputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "roinputuser required numeric." : GoTo selesai
        End If
        'roinputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "roinputtgl required date." : GoTo selesai
        End If
        'romodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "romodifikasiuser required numeric." : GoTo selesai
        End If
        'romodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "romodifikasitgl required date." : GoTo selesai
        End If
        'roisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "roisclose required numeric." : GoTo selesai
        End If
        'rocustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rocustomint1 required numeric." : GoTo selesai
        End If
        'rocustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "rocustomint2 required numeric." : GoTo selesai
        End If
        'rocustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "rocustomint3 required numeric." : GoTo selesai
        End If
        'rocustomint4(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "rocustomint4 required numeric." : GoTo selesai
        End If
        'rocustomint5(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "rocustomint5 required numeric." : GoTo selesai
        End If
        'rocustomint6(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "rocustomint6 required numeric." : GoTo selesai
        End If
        'rocustomint7(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "rocustomint7 required numeric." : GoTo selesai
        End If
        'rocustomint8(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rocustomint8 required numeric." : GoTo selesai
        End If
        'rocustomint9(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rocustomint9 required numeric." : GoTo selesai
        End If
        'rocustomint10(56) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rocustomint10 required numeric." : GoTo selesai
        End If
        'rocustomint11(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "rocustomint11 required numeric." : GoTo selesai
        End If
        'rocustomint12(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rocustomint12 required numeric." : GoTo selesai
        End If
        'rocustomint13(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "rocustomint13 required numeric." : GoTo selesai
        End If
        'rocustomint14(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "rocustomint14 required numeric." : GoTo selesai
        End If
        'rocustomint15(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "rocustomint15 required numeric." : GoTo selesai
        End If
        'rocustomint16(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "rocustomint16 required numeric." : GoTo selesai
        End If
        'rocustomint17(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rocustomint17 required numeric." : GoTo selesai
        End If
        'rocustomint18(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "rocustomint18 required numeric." : GoTo selesai
        End If
        'rocustomint19(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "rocustomint19 required numeric." : GoTo selesai
        End If
        'rocustomint20(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "rocustomint20 required numeric." : GoTo selesai
        End If
        'rocustomdbl1(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rocustomdbl1 required numeric." : GoTo selesai
        End If
        'rocustomdbl2(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "rocustomdbl2 required numeric." : GoTo selesai
        End If
        'rocustomdbl3(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "rocustomdbl3 required numeric." : GoTo selesai
        End If
        'rocustomdbl4(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "rocustomdbl4 required numeric." : GoTo selesai
        End If
        'rocustomdbl5(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "rocustomdbl5 required numeric." : GoTo selesai
        End If
        'rocustomdbl6(72) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "rocustomdbl6 required numeric." : GoTo selesai
        End If
        'rocustomdbl7(73) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "rocustomdbl7 required numeric." : GoTo selesai
        End If
        'rocustomdbl8(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "rocustomdbl8 required numeric." : GoTo selesai
        End If
        'rocustomdbl9(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "rocustomdbl9 required numeric." : GoTo selesai
        End If
        'rocustomdbl10(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "rocustomdbl10 required numeric." : GoTo selesai
        End If
        'rocustomdbl11(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "rocustomdbl11 required numeric." : GoTo selesai
        End If
        'rocustomdbl12(78) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "rocustomdbl12 required numeric." : GoTo selesai
        End If
        'rocustomdbl13(79) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "rocustomdbl13 required numeric." : GoTo selesai
        End If
        'rocustomdbl14(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "rocustomdbl14 required numeric." : GoTo selesai
        End If
        'rocustomdbl15(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "rocustomdbl15 required numeric." : GoTo selesai
        End If
        'rocustomdbl16(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "rocustomdbl16 required numeric." : GoTo selesai
        End If
        'rocustomdbl17(83) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "rocustomdbl17 required numeric." : GoTo selesai
        End If
        'rocustomdbl18(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "rocustomdbl18 required numeric." : GoTo selesai
        End If
        'rocustomdbl19(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "rocustomdbl19 required numeric." : GoTo selesai
        End If
        'rocustomdbl20(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "rocustomdbl20 required numeric." : GoTo selesai
        End If
        'rocustomdate1(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "rocustomdate1 required date." : GoTo selesai
        End If
        'rocustomdate2(88) As Date
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "rocustomdate2 required date." : GoTo selesai
        End If
        'rocustomdate3(89) As Date
        If (IsDate(dataUtama(89)) = False) Then
            result(2) = "rocustomdate3 required date." : GoTo selesai
        End If
        'rocustomdate4(90) As Date
        If (IsDate(dataUtama(90)) = False) Then
            result(2) = "rocustomdate4 required date." : GoTo selesai
        End If
        'rocustomdate5(91) As Date
        If (IsDate(dataUtama(91)) = False) Then
            result(2) = "rocustomdate5 required date." : GoTo selesai
        End If
        'rocustomdate6(92) As Date
        If (IsDate(dataUtama(92)) = False) Then
            result(2) = "rocustomdate6 required date." : GoTo selesai
        End If
        'rocustomdate7(93) As Date
        If (IsDate(dataUtama(93)) = False) Then
            result(2) = "rocustomdate7 required date." : GoTo selesai
        End If
        'rocustomdate8(94) As Date
        If (IsDate(dataUtama(94)) = False) Then
            result(2) = "rocustomdate8 required date." : GoTo selesai
        End If
        'rocustomdate9(95) As Date
        If (IsDate(dataUtama(95)) = False) Then
            result(2) = "rocustomdate9 required date." : GoTo selesai
        End If
        'rocustomdate10(96) As Date
        If (IsDate(dataUtama(96)) = False) Then
            result(2) = "rocustomdate10 required date." : GoTo selesai
        End If
        'rocustomdate11(97) As Date
        If (IsDate(dataUtama(97)) = False) Then
            result(2) = "rocustomdate11 required date." : GoTo selesai
        End If
        'rocustomdate12(98) As Date
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "rocustomdate12 required date." : GoTo selesai
        End If
        'rocustomdate13(99) As Date
        If (IsDate(dataUtama(99)) = False) Then
            result(2) = "rocustomdate13 required date." : GoTo selesai
        End If
        'rocustomdate14(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "rocustomdate14 required date." : GoTo selesai
        End If
        'rocustomdate15(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "rocustomdate15 required date." : GoTo selesai
        End If
        'rocustomdate16(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "rocustomdate16 required date." : GoTo selesai
        End If
        'rocustomdate17(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "rocustomdate17 required date." : GoTo selesai
        End If
        'rocustomdate18(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "rocustomdate18 required date." : GoTo selesai
        End If
        'rocustomdate19(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "rocustomdate19 required date." : GoTo selesai
        End If
        'rocustomdate20(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "rocustomdate20 required date." : GoTo selesai
        End If
        'rokurs(108) As Double
        dataUtama(108) = 1
        If (IsNumeric(dataUtama(108)) = False) Then
            result(2) = "rokurs required numeric." : GoTo selesai
        End If
        'roposting(109) As Integer
        If (IsNumeric(dataUtama(109)) = False) Then
            result(2) = "roposting required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'rocabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rocabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rocabang should not be more than 25 character." : GoTo selesai
        End If

        'rolokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rolokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rolokasi should not be more than 25 character." : GoTo selesai
        End If

        'rogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rogudang should not be more than 25 character." : GoTo selesai
        End If

        'rosumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "rosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "rosumber should not be more than 10 character." : GoTo selesai
        End If

        'ronotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "ronotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "ronotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rotgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "rotgl can't be empty" : GoTo selesai
        End If

        'rotglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "rotglnoref can't be empty" : GoTo selesai
        End If

        'rototaltransaksi(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "rototaltransaksi can't be empty" : GoTo selesai
        End If

        'roinputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "roinputtgl can't be empty" : GoTo selesai
        End If

        'romodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "romodifikasitgl can't be empty" : GoTo selesai
        End If

        'rocustomdbl1(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "rocustomdbl1 can't be empty" : GoTo selesai
        End If

        'rocustomdbl2(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "rocustomdbl2 can't be empty" : GoTo selesai
        End If

        'rocustomdbl3(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "rocustomdbl3 can't be empty" : GoTo selesai
        End If

        'rocustomdbl4(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "rocustomdbl4 can't be empty" : GoTo selesai
        End If

        'rocustomdbl5(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "rocustomdbl5 can't be empty" : GoTo selesai
        End If

        'rocustomdbl6(72) As Double
        If Len(dataUtama(72)) = 0 Then
            result(2) = "rocustomdbl6 can't be empty" : GoTo selesai
        End If

        'rocustomdbl7(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "rocustomdbl7 can't be empty" : GoTo selesai
        End If

        'rocustomdbl8(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "rocustomdbl8 can't be empty" : GoTo selesai
        End If

        'rocustomdbl9(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "rocustomdbl9 can't be empty" : GoTo selesai
        End If

        'rocustomdbl10(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "rocustomdbl10 can't be empty" : GoTo selesai
        End If

        'rocustomdbl11(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "rocustomdbl11 can't be empty" : GoTo selesai
        End If

        'rocustomdbl12(78) As Double
        If Len(dataUtama(78)) = 0 Then
            result(2) = "rocustomdbl12 can't be empty" : GoTo selesai
        End If

        'rocustomdbl13(79) As Double
        If Len(dataUtama(79)) = 0 Then
            result(2) = "rocustomdbl13 can't be empty" : GoTo selesai
        End If

        'rocustomdbl14(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "rocustomdbl14 can't be empty" : GoTo selesai
        End If

        'rocustomdbl15(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "rocustomdbl15 can't be empty" : GoTo selesai
        End If

        'rocustomdbl16(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "rocustomdbl16 can't be empty" : GoTo selesai
        End If

        'rocustomdbl17(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "rocustomdbl17 can't be empty" : GoTo selesai
        End If

        'rocustomdbl18(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "rocustomdbl18 can't be empty" : GoTo selesai
        End If

        'rocustomdbl19(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "rocustomdbl19 can't be empty" : GoTo selesai
        End If

        'rocustomdbl20(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "rocustomdbl20 can't be empty" : GoTo selesai
        End If

        'rocustomdate1(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "rocustomdate1 can't be empty" : GoTo selesai
        End If

        'rocustomdate2(88) As Date
        If Len(dataUtama(88)) = 0 Then
            result(2) = "rocustomdate2 can't be empty" : GoTo selesai
        End If

        'rocustomdate3(89) As Date
        If Len(dataUtama(89)) = 0 Then
            result(2) = "rocustomdate3 can't be empty" : GoTo selesai
        End If

        'rocustomdate4(90) As Date
        If Len(dataUtama(90)) = 0 Then
            result(2) = "rocustomdate4 can't be empty" : GoTo selesai
        End If

        'rocustomdate5(91) As Date
        If Len(dataUtama(91)) = 0 Then
            result(2) = "rocustomdate5 can't be empty" : GoTo selesai
        End If

        'rocustomdate6(92) As Date
        If Len(dataUtama(92)) = 0 Then
            result(2) = "rocustomdate6 can't be empty" : GoTo selesai
        End If

        'rocustomdate7(93) As Date
        If Len(dataUtama(93)) = 0 Then
            result(2) = "rocustomdate7 can't be empty" : GoTo selesai
        End If

        'rocustomdate8(94) As Date
        If Len(dataUtama(94)) = 0 Then
            result(2) = "rocustomdate8 can't be empty" : GoTo selesai
        End If

        'rocustomdate9(95) As Date
        If Len(dataUtama(95)) = 0 Then
            result(2) = "rocustomdate9 can't be empty" : GoTo selesai
        End If

        'rocustomdate10(96) As Date
        If Len(dataUtama(96)) = 0 Then
            result(2) = "rocustomdate10 can't be empty" : GoTo selesai
        End If

        'rocustomdate11(97) As Date
        If Len(dataUtama(97)) = 0 Then
            result(2) = "rocustomdate11 can't be empty" : GoTo selesai
        End If

        'rocustomdate12(98) As Date
        If Len(dataUtama(98)) = 0 Then
            result(2) = "rocustomdate12 can't be empty" : GoTo selesai
        End If

        'rocustomdate13(99) As Date
        If Len(dataUtama(99)) = 0 Then
            result(2) = "rocustomdate13 can't be empty" : GoTo selesai
        End If

        'rocustomdate14(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "rocustomdate14 can't be empty" : GoTo selesai
        End If

        'rocustomdate15(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "rocustomdate15 can't be empty" : GoTo selesai
        End If

        'rocustomdate16(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "rocustomdate16 can't be empty" : GoTo selesai
        End If

        'rocustomdate17(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "rocustomdate17 can't be empty" : GoTo selesai
        End If

        'rocustomdate18(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "rocustomdate18 can't be empty" : GoTo selesai
        End If

        'rocustomdate19(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "rocustomdate19 can't be empty" : GoTo selesai
        End If

        'rocustomdate20(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "rocustomdate20 can't be empty" : GoTo selesai
        End If

        'romatauang(107) As String
        If Len(dataUtama(107)) = 0 Then
            result(2) = "romatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(107)) > 25 Then
            result(2) = "romatauang should not be more than 25 character." : GoTo selesai
        End If

        'rokurs(108) As Double
        dataUtama(108) = 1
        If Len(dataUtama(108)) = 0 Then
            result(2) = "rokurs can't be empty" : GoTo selesai
        End If

        'roperawatan(110) As Double
        If Len(dataUtama(110)) > 10 Then
            result(2) = "roperawatan should not be more than 10 character." : GoTo selesai
        End If

        'rokurs(111) As Double
        If Len(dataUtama(111)) > 10 Then
            result(2) = "rokategoripasien should not be more than 10 character." : GoTo selesai
        End If

        'rokamar(112) As Double
        If Len(dataUtama(112)) > 100 Then
            result(2) = "rokamar should not be more than 100 character." : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "roid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rolokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "roautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ronotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rouraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ronoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "roidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rostatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "roinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "roinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "romodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "romodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "roisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rocustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rocustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "romatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "roposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "roperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rokategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rokamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "roawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ropetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rojenistransaksi", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "roid~rocabang~rolokasi~rogudang~rosumber~roautonotransaksi~ronotransaksi~rotgl~rokodepa~rocustomer~rocustomerkontak~rouraian~rocatatan~ronoref~rotglnoref~rototaltransaksi~roidkj~rostatusrealisasi~rostatus~rostatussebelumnya~rojmlrevisi~rocetakanke~roinputuser~roinputtgl~romodifikasiuser~romodifikasitgl~roisclose~rocustomtext1~rocustomtext2~rocustomtext3~rocustomtext4~rocustomtext5~rocustomtext6~rocustomtext7~rocustomtext8~rocustomtext9~rocustomtext10~rocustomtext11~rocustomtext12~rocustomtext13~rocustomtext14~rocustomtext15~rocustomtext16~rocustomtext17~rocustomtext18~rocustomtext19~rocustomtext20~rocustomint1~rocustomint2~rocustomint3~rocustomint4~rocustomint5~rocustomint6~rocustomint7~rocustomint8~rocustomint9~rocustomint10~rocustomint11~rocustomint12~rocustomint13~rocustomint14~rocustomint15~rocustomint16~rocustomint17~rocustomint18~rocustomint19~rocustomint20~rocustomdbl1~rocustomdbl2~rocustomdbl3~rocustomdbl4~rocustomdbl5~rocustomdbl6~rocustomdbl7~rocustomdbl8~rocustomdbl9~rocustomdbl10~rocustomdbl11~rocustomdbl12~rocustomdbl13~rocustomdbl14~rocustomdbl15~rocustomdbl16~rocustomdbl17~rocustomdbl18~rocustomdbl19~rocustomdbl20~rocustomdate1~rocustomdate2~rocustomdate3~rocustomdate4~rocustomdate5~rocustomdate6~rocustomdate7~rocustomdate8~rocustomdate9~rocustomdate10~rocustomdate11~rocustomdate12~rocustomdate13~rocustomdate14~rocustomdate15~rocustomdate16~rocustomdate17~rocustomdate18~rocustomdate19~rocustomdate20~romatauang~rokurs~roposting~roperawatan~rokategoripasien~rokamar~roawalankatpasien~ropetugas~rojenistransaksi", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idakdetail(0) As Integer, idak(1) As Integer, jenis(2) As String, idlayanan(3) As Integer, namalayanan(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmltotal(8) As Double, satuandefault(9) As String, 
        'harga(10) As Double, diskon(11) As String, jmldiskon(12) As Double, pajak1(13) As String, jmlpajak1(14) As Double, 
        'pajak2(15) As String, jmlpajak2(16) As Double, cabang(17) As String, lokasi(18) As String, gudang(19) As String, 
        'costcenter(20) As String, divisi(21) As String, subdivisi(22) As String, proyek(23) As String, catatan(24) As String, 
        'urutan(25) As Integer, idkjdetail(26) As Integer, jmlrealisasi(27) As Double, statusrealisasi(28) As Integer, isclose(29) As Integer, 
        'iddokter(30) As Integer, namadokter(31) As String, customtext1(32) As String, customtext2(33) As String, customtext3(34) As String,
        'customtext4(35) As String, customtext5(36) As String, customtext6(37) As String, customtext7(38) As String, customtext8(39) As String, 
        'customtext9(40) As String, customtext10(41) As String, customtext11(42) As String, customtext12(43) As String, customtext13(44) As String, 
        'customtext14(45) As String, customtext15(46) As String, customtext16(47) As String, customtext17(48) As String, customtext18(49) As String, 
        'customtext19(50) As String, customtext20(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdbl4(55) As Double, customdbl5(56) As Double, customdbl6(57) As Double, customdbl7(58) As Double, customdbl8(59) As Double,
        'customdbl9(60) As Double, customdbl10(61) As Double, customdbl11(62) As Double, customdbl12(63) As Double, customdbl13(64) As Double,
        'customdbl14(65) As Double, customdbl15(66) As Double, customdbl16(67) As Double, customdbl17(68) As Double, customdbl18(69) As Double,
        'customdbl19(70) As Double, customdbl20(71) As Double, customdate1(72) As Date, customdate2(73) As Date, customdate3(74) As Date,
        'customdate4(75) As Date, customdate5(76) As Date, customdate6(77) As Date, customdate7(78) As Date, customdate8(79) As Date,
        'customdate9(80) As Date, customdate10(81) As Date, customdate11(82) As Date, customdate12(83) As Date, customdate13(84) As Date,
        'customdate14(85) As Date, customdate15(86) As Date, customdate16(87) As Date, customdate17(88) As Date, customdate18(89) As Date,
        'customdate19(90) As Date, customdate20(91) As Date, matauang(92) As String, kurs(93) As Double, rekpersediaan(94) As String, 
        'rekhargapokok(95) As String, rekdiskonpenjualan(96) As String, rekpenjualan(97) As String

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idakdetail, idak, jenis, idlayanan, namalayanan, 
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
        'customdate19, customdate20, matauang, kurs, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, rekpenjualan

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idro", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idlayanan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namalayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmltotal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuandefault", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "iddokter", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idhppkhususkeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)

        'Variabel Hpp
        Dim ftBarang As String = ""
        Dim idbarang As Integer = 0, jmlbarang As Double = 0

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokBooking As String = ""
        Dim idlayanan As Integer = 0, idkjdetail As Integer = 0, jmltotal As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokBarang As String = "", ftStokBarang As String = "", ftStokAvailable As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""

        'Variabel Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 103) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idakdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrodetail required numeric." : GoTo selesai
            End If
            'idro(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idro required numeric." : GoTo selesai
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
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmltotal(8) As Double
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmltotal required numeric." : GoTo selesai
            End If
            'harga(10) As Double
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(25) As Integer
            If (IsNumeric(dataRowDetail(25)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idkjdetail(26) As Integer
            If (IsNumeric(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - idkjdetail required numeric." : GoTo selesai
            End If
            'statusrealisasi(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - statusrealisasi required numeric." : GoTo selesai
            End If
            'isclose(29) As Integer
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'iddokter(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - iddokter required numeric." : GoTo selesai
            End If
            'customdbl1(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdbl4(55) As Double
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdbl4 required numeric." : GoTo selesai
            End If
            'customdbl5(56) As Double
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdbl5 required numeric." : GoTo selesai
            End If
            'customdbl6(57) As Double
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - customdbl6 required numeric." : GoTo selesai
            End If
            'customdbl7(58) As Double
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - customdbl7 required numeric." : GoTo selesai
            End If
            'customdbl8(59) As Double
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - customdbl8 required numeric." : GoTo selesai
            End If
            'customdbl9(60) As Double
            If (IsNumeric(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - customdbl9 required numeric." : GoTo selesai
            End If
            'customdbl10(61) As Double
            If (IsNumeric(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - customdbl10 required numeric." : GoTo selesai
            End If
            'customdbl11(62) As Double
            If (IsNumeric(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - customdbl11 required numeric." : GoTo selesai
            End If
            'customdbl12(63) As Double
            If (IsNumeric(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - customdbl12 required numeric." : GoTo selesai
            End If
            'customdbl13(64) As Double
            If (IsNumeric(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - customdbl13 required numeric." : GoTo selesai
            End If
            'customdbl14(65) As Double
            If (IsNumeric(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - customdbl14 required numeric." : GoTo selesai
            End If
            'customdbl15(66) As Double
            If (IsNumeric(dataRowDetail(66)) = False) Then
                result(2) = "Row : " & i & " - customdbl15 required numeric." : GoTo selesai
            End If
            'customdbl16(67) As Double
            If (IsNumeric(dataRowDetail(67)) = False) Then
                result(2) = "Row : " & i & " - customdbl16 required numeric." : GoTo selesai
            End If
            'customdbl17(68) As Double
            If (IsNumeric(dataRowDetail(68)) = False) Then
                result(2) = "Row : " & i & " - customdbl17 required numeric." : GoTo selesai
            End If
            'customdbl18(69) As Double
            If (IsNumeric(dataRowDetail(69)) = False) Then
                result(2) = "Row : " & i & " - customdbl18 required numeric." : GoTo selesai
            End If
            'customdbl19(70) As Double
            If (IsNumeric(dataRowDetail(70)) = False) Then
                result(2) = "Row : " & i & " - customdbl19 required numeric." : GoTo selesai
            End If
            'customdbl20(71) As Double
            If (IsNumeric(dataRowDetail(71)) = False) Then
                result(2) = "Row : " & i & " - customdbl20 required numeric." : GoTo selesai
            End If
            'customdate1(72) As Date
            If (IsDate(dataRowDetail(72)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(73) As Date
            If (IsDate(dataRowDetail(73)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(74) As Date
            If (IsDate(dataRowDetail(74)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'customdate4(75) As Date
            If (IsDate(dataRowDetail(75)) = False) Then
                result(2) = "Row : " & i & " - customdate4 required date." : GoTo selesai
            End If
            'customdate5(76) As Date
            If (IsDate(dataRowDetail(76)) = False) Then
                result(2) = "Row : " & i & " - customdate5 required date." : GoTo selesai
            End If
            'customdate6(77) As Date
            If (IsDate(dataRowDetail(77)) = False) Then
                result(2) = "Row : " & i & " - customdate6 required date." : GoTo selesai
            End If
            'customdate7(78) As Date
            If (IsDate(dataRowDetail(78)) = False) Then
                result(2) = "Row : " & i & " - customdate7 required date." : GoTo selesai
            End If
            'customdate8(79) As Date
            If (IsDate(dataRowDetail(79)) = False) Then
                result(2) = "Row : " & i & " - customdate8 required date." : GoTo selesai
            End If
            'customdate9(80) As Date
            If (IsDate(dataRowDetail(80)) = False) Then
                result(2) = "Row : " & i & " - customdate9 required date." : GoTo selesai
            End If
            'customdate10(81) As Date
            If (IsDate(dataRowDetail(81)) = False) Then
                result(2) = "Row : " & i & " - customdate10 required date." : GoTo selesai
            End If
            'customdate11(82) As Date
            If (IsDate(dataRowDetail(82)) = False) Then
                result(2) = "Row : " & i & " - customdate11 required date." : GoTo selesai
            End If
            'customdate12(83) As Date
            If (IsDate(dataRowDetail(83)) = False) Then
                result(2) = "Row : " & i & " - customdate12 required date." : GoTo selesai
            End If
            'customdate13(84) As Date
            If (IsDate(dataRowDetail(84)) = False) Then
                result(2) = "Row : " & i & " - customdate13 required date." : GoTo selesai
            End If
            'customdate14(85) As Date
            If (IsDate(dataRowDetail(85)) = False) Then
                result(2) = "Row : " & i & " - customdate14 required date." : GoTo selesai
            End If
            'customdate15(86) As Date
            If (IsDate(dataRowDetail(86)) = False) Then
                result(2) = "Row : " & i & " - customdate15 required date." : GoTo selesai
            End If
            'customdate16(87) As Date
            If (IsDate(dataRowDetail(87)) = False) Then
                result(2) = "Row : " & i & " - customdate16 required date." : GoTo selesai
            End If
            'customdate17(88) As Date
            If (IsDate(dataRowDetail(88)) = False) Then
                result(2) = "Row : " & i & " - customdate17 required date." : GoTo selesai
            End If
            'customdate18(89) As Date
            If (IsDate(dataRowDetail(89)) = False) Then
                result(2) = "Row : " & i & " - customdate18 required date." : GoTo selesai
            End If
            'customdate19(90) As Date
            If (IsDate(dataRowDetail(90)) = False) Then
                result(2) = "Row : " & i & " - customdate19 required date." : GoTo selesai
            End If
            'customdate20(91) As Date
            If (IsDate(dataRowDetail(91)) = False) Then
                result(2) = "Row : " & i & " - customdate20 required date." : GoTo selesai
            End If
            'kurs(93) As Double
            dataRowDetail(93) = 1
            If (IsNumeric(dataRowDetail(93)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'idhppkhususkeluar(98) As Integer
            If (IsNumeric(dataRowDetail(98)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususkeluar required numeric." : GoTo selesai
            End If
            'hpp(99) As Double
            If (IsNumeric(dataRowDetail(99)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
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

            'harga(10) As Double
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(11) As Double
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(10) As Double, diskon(11) As String
                dataRowDetail(12) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(10)), FixQuotes(dataRowDetail(11).ToString))
            End If

            'jmlpajak1(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'namadokter(31) As String
            'If Len(dataRowDetail(31)) = 0 Then
            '    result(2) = "Row : " & i & " - namadokter can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(31)) > 100 Then
                result(2) = "Row : " & i & " - namadokter should not be more than 100 character." : GoTo selesai
            End If

            'customdbl1(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If
            'customdbl2(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If
            'customdbl3(54) As Double
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If
            'customdbl4(55) As Double
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdbl4 can't be empty" : GoTo selesai
            End If
            'customdbl5(56) As Double
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdbl5 can't be empty" : GoTo selesai
            End If
            'customdbl6(57) As Double
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdbl6 can't be empty" : GoTo selesai
            End If
            'customdbl7(58) As Double
            If Len(dataRowDetail(58)) = 0 Then
                result(2) = "Row : " & i & " - customdbl7 can't be empty" : GoTo selesai
            End If
            'customdbl8(59) As Double
            If Len(dataRowDetail(59)) = 0 Then
                result(2) = "Row : " & i & " - customdbl8 can't be empty" : GoTo selesai
            End If
            'customdbl9(60) As Double
            If Len(dataRowDetail(60)) = 0 Then
                result(2) = "Row : " & i & " - customdbl9 can't be empty" : GoTo selesai
            End If
            'customdbl10(61) As Double
            If Len(dataRowDetail(61)) = 0 Then
                result(2) = "Row : " & i & " - customdbl10 can't be empty" : GoTo selesai
            End If
            'customdbl11(62) As Double
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - customdbl11 can't be empty" : GoTo selesai
            End If
            'customdbl12(63) As Double
            If Len(dataRowDetail(63)) = 0 Then
                result(2) = "Row : " & i & " - customdbl12 can't be empty" : GoTo selesai
            End If
            'customdbl13(64) As Double
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - customdbl13 can't be empty" : GoTo selesai
            End If
            'customdbl14(65) As Double
            If Len(dataRowDetail(65)) = 0 Then
                result(2) = "Row : " & i & " - customdbl14 can't be empty" : GoTo selesai
            End If
            'customdbl15(66) As Double
            If Len(dataRowDetail(66)) = 0 Then
                result(2) = "Row : " & i & " - customdbl15 can't be empty" : GoTo selesai
            End If
            'customdbl16(67) As Double
            If Len(dataRowDetail(67)) = 0 Then
                result(2) = "Row : " & i & " - customdbl16 can't be empty" : GoTo selesai
            End If
            'customdbl17(68) As Double
            If Len(dataRowDetail(68)) = 0 Then
                result(2) = "Row : " & i & " - customdbl17 can't be empty" : GoTo selesai
            End If
            'customdbl18(69) As Double
            If Len(dataRowDetail(69)) = 0 Then
                result(2) = "Row : " & i & " - customdbl18 can't be empty" : GoTo selesai
            End If
            'customdbl19(70) As Double
            If Len(dataRowDetail(70)) = 0 Then
                result(2) = "Row : " & i & " - customdbl19 can't be empty" : GoTo selesai
            End If
            'customdbl20(71) As Double
            If Len(dataRowDetail(71)) = 0 Then
                result(2) = "Row : " & i & " - customdbl20 can't be empty" : GoTo selesai
            End If
            'customdate1(72) As Date
            If Len(dataRowDetail(72)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If
            'customdate2(73) As Date
            If Len(dataRowDetail(73)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If
            'customdate3(74) As Date
            If Len(dataRowDetail(74)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'customdate4(75) As Date
            If Len(dataRowDetail(75)) = 0 Then
                result(2) = "Row : " & i & " - customdate4 can't be empty" : GoTo selesai
            End If
            'customdate5(76) As Date
            If Len(dataRowDetail(76)) = 0 Then
                result(2) = "Row : " & i & " - customdate5 can't be empty" : GoTo selesai
            End If
            'customdate6(77) As Date
            If Len(dataRowDetail(77)) = 0 Then
                result(2) = "Row : " & i & " - customdate6 can't be empty" : GoTo selesai
            End If
            'customdate7(78) As Date
            If Len(dataRowDetail(78)) = 0 Then
                result(2) = "Row : " & i & " - customdate7 can't be empty" : GoTo selesai
            End If
            'customdate8(79) As Date
            If Len(dataRowDetail(79)) = 0 Then
                result(2) = "Row : " & i & " - customdate8 can't be empty" : GoTo selesai
            End If
            'customdate9(80) As Date
            If Len(dataRowDetail(80)) = 0 Then
                result(2) = "Row : " & i & " - customdate9 can't be empty" : GoTo selesai
            End If
            'customdate10(81) As Date
            If Len(dataRowDetail(81)) = 0 Then
                result(2) = "Row : " & i & " - customdate10 can't be empty" : GoTo selesai
            End If
            'customdate11(82) As Date
            If Len(dataRowDetail(82)) = 0 Then
                result(2) = "Row : " & i & " - customdate11 can't be empty" : GoTo selesai
            End If
            'customdate12(83) As Date
            If Len(dataRowDetail(83)) = 0 Then
                result(2) = "Row : " & i & " - customdate12 can't be empty" : GoTo selesai
            End If
            'customdate13(84) As Date
            If Len(dataRowDetail(84)) = 0 Then
                result(2) = "Row : " & i & " - customdate13 can't be empty" : GoTo selesai
            End If
            'customdate14(85) As Date
            If Len(dataRowDetail(85)) = 0 Then
                result(2) = "Row : " & i & " - customdate14 can't be empty" : GoTo selesai
            End If
            'customdate15(86) As Date
            If Len(dataRowDetail(86)) = 0 Then
                result(2) = "Row : " & i & " - customdate15 can't be empty" : GoTo selesai
            End If
            'customdate16(87) As Date
            If Len(dataRowDetail(87)) = 0 Then
                result(2) = "Row : " & i & " - customdate16 can't be empty" : GoTo selesai
            End If
            'customdate17(88) As Date
            If Len(dataRowDetail(88)) = 0 Then
                result(2) = "Row : " & i & " - customdate17 can't be empty" : GoTo selesai
            End If
            'customdate18(89) As Date
            If Len(dataRowDetail(89)) = 0 Then
                result(2) = "Row : " & i & " - customdate18 can't be empty" : GoTo selesai
            End If
            'customdate19(90) As Date
            If Len(dataRowDetail(90)) = 0 Then
                result(2) = "Row : " & i & " - customdate19 can't be empty" : GoTo selesai
            End If
            'customdate20(91) As Date
            If Len(dataRowDetail(91)) = 0 Then
                result(2) = "Row : " & i & " - customdate20 can't be empty" : GoTo selesai
            End If
            'matauang(92) As String
            If Len(dataRowDetail(92)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(92)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If
            'kurs(93) As Double
            dataRowDetail(93) = 1
            If Len(dataRowDetail(93)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If
            'hpp(99) As Double
            If Len(dataRowDetail(99)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(100)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If
            'gudangtujuan(101) As String
            If Len(dataRowDetail(101)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(101)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idrodetail~idro~jenis~idlayanan~namalayanan~jml~satuan~nilaisatuan~jmltotal~satuandefault~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idkjdetail~jmlrealisasi~statusrealisasi~isclose~iddokter~namadokter~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customtext11~customtext12~customtext13~customtext14~customtext15~customtext16~customtext17~customtext18~customtext19~customtext20~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdbl11~customdbl12~customdbl13~customdbl14~customdbl15~customdbl16~customdbl17~customdbl18~customdbl19~customdbl20~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~customdate11~customdate12~customdate13~customdate14~customdate15~customdate16~customdate17~customdate18~customdate19~customdate20~matauang~kurs~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekpenjualan~idhppkhususkeluar~hpp~gudangtransit~gudangtujuan~tipebarang", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76) & "~" & dataRowDetail(77) & "~" & dataRowDetail(78) & "~" & dataRowDetail(79) & "~" & dataRowDetail(80) & "~" & dataRowDetail(81) & "~" & dataRowDetail(82) & "~" & dataRowDetail(83) & "~" & dataRowDetail(84) & "~" & dataRowDetail(85) & "~" & dataRowDetail(86) & "~" & dataRowDetail(87) & "~" & dataRowDetail(88) & "~" & dataRowDetail(89) & "~" & dataRowDetail(90) & "~" & dataRowDetail(91) & "~" & dataRowDetail(92) & "~" & dataRowDetail(93) & "~" & dataRowDetail(94) & "~" & dataRowDetail(95) & "~" & dataRowDetail(96) & "~" & dataRowDetail(97) & "~" & dataRowDetail(98) & "~" & dataRowDetail(99) & "~" & dataRowDetail(100) & "~" & dataRowDetail(101) & "~" & dataRowDetail(102)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'result(2) = dataRowDetail(98) & " " & dataRowDetail(99) & " " & dataRowDetail(100) & " " & dataRowDetail(101) : GoTo selesai
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idlayanan(3) As Integer     , jmltotal(8) As Double       , gudang(19) As String       , idkjdetail(26) As Integer
            idlayanan = dataRowDetail(3) : idbarang = dataRowDetail(3) : jmltotal = dataRowDetail(8) : gudang = dataRowDetail(19) : gudangIn = dataRowDetail(19) : gudangOut = dataRowDetail(100) : idkjdetail = dataRowDetail(26)
            'kurs(11) As Double                    , harga(12) As Double
            'kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            'If idkjdetail <> 0 Then
            '1. CEK DATA EXIST
            'ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
            'ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m_11_kj_detail JOIN m_11_kj ON idkj = kjid WHERE idkjdetail = '" & idkjdetail & "' AND (kjstatus = 2 OR kjstatus = 3 OR kjstatus = 4 OR kjstatus = 7) LIMIT 1) as rowExists, '" & idkjdetail & "' as idkjdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

            '2. CEK JML OUTSTANDING
            'Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
            'ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
            'ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")

            '3. SET NILAI UPDATE OUTSTANDING
            'updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN jmlrealisasi + '" & Outstanding & "' ", updNilai)

            '4. SET FILTER UPDATE OUTSTANDING
            'updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
            'updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            'End If

            ''5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            'Validasi harga dibawah harga jual
            'ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            'ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            'ValidasiHpp
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI STOK -------------------------------
            'VALIDASI STOK #1, CEK STOK ADA ATAU TIDAK
            'ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            'ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bassembly <> 1 AND bid = '" & idbarang & "'")

            'Dim Stok As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang & " AND gudang='" & gudangOut & "'")
            'result(2) = Stok.ToString : GoTo selesai
            'VALIDASI STOK DIBAGI MENJADI 2 JENIS, YAKNI :
            'VALIDASI STOK #1, CEK STOK PERGUDANG (TOTAL STOK PERGUDANG)
            '   - JIKA AMBIL SO
            '   - JIKA AMBIL PI YANG TERKAIT DARI SO
            '   - JIKA AMBIL PL YANG TERKAIT DARI SO
            '   - JIKA AMBIL DO YANG TERKAIT DARI SO
            '   - JIKA AMBIL DR YANG TERKAIT DARI SO
            'VALIDASI STOK #2, CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)
            '   - JIKA TIDAK AMBIL TRANSAKSI SEBELUMNYA
            '   - JIKA AMBIL PI YANG TIDAK TERKAIT DARI SO
            '   - JIKA AMBIL PL YANG TIDAK TERKAIT DARI SO
            '   - JIKA AMBIL DO YANG TIDAK TERKAIT DARI SO
            '   - JIKA AMBIL DR YANG TIDAK TERKAIT DARI SO

            'If idsodetail <> 0 Then
            '    'VALIDASI STOK #1, CEK STOK PERGUDANG (TOTAL STOK PERGUDANG)
            '    'CEK JML STOK KELUAR
            '    ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            '    ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            'Else
            'VALIDASI STOK #2, CEK STOK AVAILABLE PERGUDANG (TOTAL STOK PERGUDANG - STOK BOOKING)
            'CEK JML STOK KELUAR
            'ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
            'ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
            'End If

            'SET NILAI UPDATE STOK KELUAR
            'updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            'updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            'SET NILAI UPDATE STOK M1_ITEM
            'Dim stokKeluar As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang)

            'ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            'ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            'updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN bstok - '" & stokKeluar & "' ", updStokBarang)

            ' ''1. SET NILAI UPDATE STOK MASUK
            ''updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            ''updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            ' ''2. SET NILAI UPDATE STOK M1_ITEM ------------
            ''Dim stokMasuk As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang)
            ''ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ''ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            ''updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN bstok + '" & stokMasuk & "' ", updStokBarang)
            ' ''END OF BUAT FILTER UNTUK VALIDASI --------------------------

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            '5. SET NILAI UPDATE STOK M1_ITEM ------------
            Dim stokMasuk As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang)
            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN bstok + '" & stokMasuk & "' ", updStokBarang)
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 41
                Select Case drutama("rostatus")
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


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("lutgl")), AsFormatTanggal(drutama("lutgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("rostatus") = 2 Or drutama("rostatus") = 1 Or drutama("rostatus") = 8 Or drutama("rostatus") = 9 Or drutama("rostatus") = 10 Or drutama("rostatus") = 11 Then

                    Dim rsValidasi As String = ""

                    'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                    Dim MUFungsional As String = ""
                    Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                    If dtSetting.Rows.Count > 0 Then
                        MUFungsional = dtSetting.Rows(0)(0)
                    Else
                        result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
                    End If
                    'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, "", "", "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                    ' result(2) = "dsds" : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sotermin").ToString, AsFormatTanggal(drutama("sotgl")), "sotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                'result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                'drutama("sotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                'drutama("sototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                'drutama("sototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                'drutama("sototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                'drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak1detail")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("roid")
                    notransaksi = drutama("ronotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(roid), ronotransaksi FROM M_11_ro WHERE roid='" & result(4) & "' AND rostatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m_11_ro WHERE ronotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m5_so_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sosumber")) & "▼" & FixQuotes(drutama("soid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        'result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_11_ro set rocabang  = '" & FixQuotes(drutama("rocabang")) & "', rolokasi  = '" & FixQuotes(drutama("rolokasi")) & "', rogudang  = '" & FixQuotes(drutama("rogudang")) & "', rosumber  = '" & FixQuotes(drutama("rosumber")) & "', roautonotransaksi  = " & drutama("roautonotransaksi") & ", ronotransaksi  = '" & FixQuotes(notransaksi) & "', rotgl  = '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', rokodepa  = " & drutama("rokodepa") & ", rocustomer  = " & drutama("rocustomer") & ", rocustomerkontak  = '" & FixQuotes(drutama("rocustomerkontak")) & "', rouraian  = '" & FixQuotes(drutama("rouraian")) & "', rocatatan  = '" & FixQuotes(drutama("rocatatan")) & "', ronoref  = '" & FixQuotes(drutama("ronoref")) & "', rotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rotglnoref"))) & "', rototaltransaksi  = '" & FixDouble(drutama("rototaltransaksi")) & "', roidkj  = " & drutama("roidkj") & ", rostatusrealisasi  = " & drutama("rostatusrealisasi") & ", rostatus  = " & drutama("rostatus") & ", rostatussebelumnya  = " & drutama("rostatussebelumnya") & ", rojmlrevisi  = rojmlrevisi+1, rocetakanke  = " & drutama("rocetakanke") & ", romodifikasiuser  = " & drutama("romodifikasiuser") & ", romodifikasitgl  = NOW(), rocustomtext1  = '" & FixQuotes(drutama("rocustomtext1")) & "', rocustomtext2  = '" & FixQuotes(drutama("rocustomtext2")) & "', rocustomtext3  = '" & FixQuotes(drutama("rocustomtext3")) & "', rocustomtext4  = '" & FixQuotes(drutama("rocustomtext4")) & "', rocustomtext5  = '" & FixQuotes(drutama("rocustomtext5")) & "', rocustomtext6  = '" & FixQuotes(drutama("rocustomtext6")) & "', rocustomtext7  = '" & FixQuotes(drutama("rocustomtext7")) & "', rocustomtext8  = '" & FixQuotes(drutama("rocustomtext8")) & "', rocustomtext9  = '" & FixQuotes(drutama("rocustomtext9")) & "', rocustomtext10  = '" & FixQuotes(drutama("rocustomtext10")) & "', rocustomtext11  = '" & FixQuotes(drutama("rocustomtext11")) & "', rocustomtext12  = '" & FixQuotes(drutama("rocustomtext12")) & "', rocustomtext13  = '" & FixQuotes(drutama("rocustomtext13")) & "', rocustomtext14  = '" & FixQuotes(drutama("rocustomtext14")) & "', rocustomtext15  = '" & FixQuotes(drutama("rocustomtext15")) & "', rocustomtext16  = '" & FixQuotes(drutama("rocustomtext16")) & "', rocustomtext17  = '" & FixQuotes(drutama("rocustomtext17")) & "', rocustomtext18  = '" & FixQuotes(drutama("rocustomtext18")) & "', rocustomtext19  = '" & FixQuotes(drutama("rocustomtext19")) & "', rocustomtext20  = '" & FixQuotes(drutama("rocustomtext20")) & "', rocustomint1  = " & drutama("rocustomint1") & ", rocustomint2  = " & drutama("rocustomint2") & ", rocustomint3  = " & drutama("rocustomint3") & ", rocustomint4  = " & drutama("rocustomint4") & ", rocustomint5  = " & drutama("rocustomint5") & ", rocustomint6  = " & drutama("rocustomint6") & ", rocustomint7  = " & drutama("rocustomint7") & ", rocustomint8  = " & drutama("rocustomint8") & ", rocustomint9  = " & drutama("rocustomint9") & ", rocustomint10  = " & drutama("rocustomint10") & ", rocustomint11  = " & drutama("rocustomint11") & ", rocustomint12  = " & drutama("rocustomint12") & ", rocustomint13  = " & drutama("rocustomint13") & ", rocustomint14  = " & drutama("rocustomint14") & ", rocustomint15  = " & drutama("rocustomint15") & ", rocustomint16  = " & drutama("rocustomint16") & ", rocustomint17  = " & drutama("rocustomint17") & ", rocustomint18  = " & drutama("rocustomint18") & ", rocustomint19  = " & drutama("rocustomint19") & ", rocustomint20  = " & drutama("rocustomint20") & ", rocustomdbl1  = '" & FixDouble(drutama("rocustomdbl1")) & "', rocustomdbl2  = '" & FixDouble(drutama("rocustomdbl2")) & "', rocustomdbl3  = '" & FixDouble(drutama("rocustomdbl3")) & "', rocustomdbl4  = '" & FixDouble(drutama("rocustomdbl4")) & "', rocustomdbl5  = '" & FixDouble(drutama("rocustomdbl5")) & "', rocustomdbl6  = '" & FixDouble(drutama("rocustomdbl6")) & "', rocustomdbl7  = '" & FixDouble(drutama("rocustomdbl7")) & "', rocustomdbl8  = '" & FixDouble(drutama("rocustomdbl8")) & "', rocustomdbl9  = '" & FixDouble(drutama("rocustomdbl9")) & "', rocustomdbl10  = '" & FixDouble(drutama("rocustomdbl10")) & "', rocustomdbl11  = '" & FixDouble(drutama("rocustomdbl11")) & "', rocustomdbl12  = '" & FixDouble(drutama("rocustomdbl12")) & "', rocustomdbl13  = '" & FixDouble(drutama("rocustomdbl13")) & "', rocustomdbl14  = '" & FixDouble(drutama("rocustomdbl14")) & "', rocustomdbl15  = '" & FixDouble(drutama("rocustomdbl15")) & "', rocustomdbl16  = '" & FixDouble(drutama("rocustomdbl16")) & "', rocustomdbl17  = '" & FixDouble(drutama("rocustomdbl17")) & "', rocustomdbl18  = '" & FixDouble(drutama("rocustomdbl18")) & "', rocustomdbl19  = '" & FixDouble(drutama("rocustomdbl19")) & "', rocustomdbl20  = '" & FixDouble(drutama("rocustomdbl20")) & "', rocustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate1"))) & "', rocustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate2"))) & "', rocustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate3"))) & "', rocustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate4"))) & "', rocustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate5"))) & "', rocustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate6"))) & "', rocustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate7"))) & "', rocustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate8"))) & "', rocustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate9"))) & "', rocustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate10"))) & "', rocustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate11"))) & "', rocustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate12"))) & "', rocustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate13"))) & "', rocustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate14"))) & "', rocustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate15"))) & "', rocustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate16"))) & "', rocustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate17"))) & "', rocustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate18"))) & "', rocustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate19"))) & "', rocustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate20"))) & "', romatauang  = '" & FixQuotes(drutama("romatauang")) & "', rokurs  = '" & FixDouble(drutama("rokurs")) & "', roposting  = 0, roperawatan  = '" & FixDouble(drutama("roperawatan")) & "', rokategoripasien  = '" & FixDouble(drutama("rokategoripasien")) & "', rokamar  = '" & FixDouble(drutama("rokamar")) & "', ropetugas  = " & drutama("ropetugas") & ", rojenistransaksi  = " & drutama("rojenistransaksi") & "  where roid = '" & drutama("roid") & "'"
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

                    If drutama("roautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("roperawatan"), drutama("roawalankatpasien"), drutama("rosumber"), drutama("rotgl"))
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
                        notransaksi = drutama("ronotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(roid) FROM m_11_ro WHERE ronotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_11_ro (rocabang, rolokasi, rogudang, rosumber, roautonotransaksi, ronotransaksi, rotgl, rokodepa, rocustomer, rocustomerkontak, rouraian, rocatatan, ronoref, rotglnoref, rototaltransaksi, roidkj, rostatusrealisasi, rostatus, rostatussebelumnya, rojmlrevisi, rocetakanke, roinputuser, roinputtgl, romodifikasiuser, romodifikasitgl, roisclose, rocustomtext1, rocustomtext2, rocustomtext3, rocustomtext4, rocustomtext5, rocustomtext6, rocustomtext7, rocustomtext8, rocustomtext9, rocustomtext10, rocustomtext11, rocustomtext12, rocustomtext13, rocustomtext14, rocustomtext15, rocustomtext16, rocustomtext17, rocustomtext18, rocustomtext19, rocustomtext20, rocustomint1, rocustomint2, rocustomint3, rocustomint4, rocustomint5, rocustomint6, rocustomint7, rocustomint8, rocustomint9, rocustomint10, rocustomint11, rocustomint12, rocustomint13, rocustomint14, rocustomint15, rocustomint16, rocustomint17, rocustomint18, rocustomint19, rocustomint20, rocustomdbl1, rocustomdbl2, rocustomdbl3, rocustomdbl4, rocustomdbl5, rocustomdbl6, rocustomdbl7, rocustomdbl8, rocustomdbl9, rocustomdbl10, rocustomdbl11, rocustomdbl12, rocustomdbl13, rocustomdbl14, rocustomdbl15, rocustomdbl16, rocustomdbl17, rocustomdbl18, rocustomdbl19, rocustomdbl20, rocustomdate1, rocustomdate2, rocustomdate3, rocustomdate4, rocustomdate5, rocustomdate6, rocustomdate7, rocustomdate8, rocustomdate9, rocustomdate10, rocustomdate11, rocustomdate12, rocustomdate13, rocustomdate14, rocustomdate15, rocustomdate16, rocustomdate17, rocustomdate18, rocustomdate19, rocustomdate20, romatauang, rokurs, roperawatan, rokategoripasien, rokamar, ropetugas, rojenistransaksi) values('" & FixQuotes(drutama("rocabang")) & "', '" & FixQuotes(drutama("rolokasi")) & "', '" & FixQuotes(drutama("rogudang")) & "', '" & FixQuotes(drutama("rosumber")) & "', " & drutama("roautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & drutama("rokodepa") & ", " & drutama("rocustomer") & ", '" & FixQuotes(drutama("rocustomerkontak")) & "', '" & FixQuotes(drutama("rouraian")) & "', '" & FixQuotes(drutama("rocatatan")) & "', '" & FixQuotes(drutama("ronoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotglnoref"))) & "', '" & FixDouble(drutama("rototaltransaksi")) & "', " & drutama("roidkj") & ", " & drutama("rostatusrealisasi") & ", " & drutama("rostatus") & ", " & drutama("rostatussebelumnya") & ", " & drutama("rojmlrevisi") & ", " & drutama("rocetakanke") & ", " & drutama("roinputuser") & ", NOW(), " & drutama("romodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("roisclose") & ", '" & FixQuotes(drutama("rocustomtext1")) & "', '" & FixQuotes(drutama("rocustomtext2")) & "', '" & FixQuotes(drutama("rocustomtext3")) & "', '" & FixQuotes(drutama("rocustomtext4")) & "', '" & FixQuotes(drutama("rocustomtext5")) & "', '" & FixQuotes(drutama("rocustomtext6")) & "', '" & FixQuotes(drutama("rocustomtext7")) & "', '" & FixQuotes(drutama("rocustomtext8")) & "', '" & FixQuotes(drutama("rocustomtext9")) & "', '" & FixQuotes(drutama("rocustomtext10")) & "', '" & FixQuotes(drutama("rocustomtext11")) & "', '" & FixQuotes(drutama("rocustomtext12")) & "', '" & FixQuotes(drutama("rocustomtext13")) & "', '" & FixQuotes(drutama("rocustomtext14")) & "', '" & FixQuotes(drutama("rocustomtext15")) & "', '" & FixQuotes(drutama("rocustomtext16")) & "', '" & FixQuotes(drutama("rocustomtext17")) & "', '" & FixQuotes(drutama("rocustomtext18")) & "', '" & FixQuotes(drutama("rocustomtext19")) & "', '" & FixQuotes(drutama("rocustomtext20")) & "', " & drutama("rocustomint1") & ", " & drutama("rocustomint2") & ", " & drutama("rocustomint3") & ", " & drutama("rocustomint4") & ", " & drutama("rocustomint5") & ", " & drutama("rocustomint6") & ", " & drutama("rocustomint7") & ", " & drutama("rocustomint8") & ", " & drutama("rocustomint9") & ", " & drutama("rocustomint10") & ", " & drutama("rocustomint11") & ", " & drutama("rocustomint12") & ", " & drutama("rocustomint13") & ", " & drutama("rocustomint14") & ", " & drutama("rocustomint15") & ", " & drutama("rocustomint16") & ", " & drutama("rocustomint17") & ", " & drutama("rocustomint18") & ", " & drutama("rocustomint19") & ", " & drutama("rocustomint20") & ", '" & FixDouble(drutama("rocustomdbl1")) & "', '" & FixDouble(drutama("rocustomdbl2")) & "', '" & FixDouble(drutama("rocustomdbl3")) & "', '" & FixDouble(drutama("rocustomdbl4")) & "', '" & FixDouble(drutama("rocustomdbl5")) & "', '" & FixDouble(drutama("rocustomdbl6")) & "', '" & FixDouble(drutama("rocustomdbl7")) & "', '" & FixDouble(drutama("rocustomdbl8")) & "', '" & FixDouble(drutama("rocustomdbl9")) & "', '" & FixDouble(drutama("rocustomdbl10")) & "', '" & FixDouble(drutama("rocustomdbl11")) & "', '" & FixDouble(drutama("rocustomdbl12")) & "', '" & FixDouble(drutama("rocustomdbl13")) & "', '" & FixDouble(drutama("rocustomdbl14")) & "', '" & FixDouble(drutama("rocustomdbl15")) & "', '" & FixDouble(drutama("rocustomdbl16")) & "', '" & FixDouble(drutama("rocustomdbl17")) & "', '" & FixDouble(drutama("rocustomdbl18")) & "', '" & FixDouble(drutama("rocustomdbl19")) & "', '" & FixDouble(drutama("rocustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rocustomdate20"))) & "', '" & FixQuotes(drutama("romatauang")) & "', '" & FixDouble(drutama("rokurs")) & "', '" & FixDouble(drutama("roperawatan")) & "', '" & FixDouble(drutama("rokategoripasien")) & "', '" & FixDouble(drutama("rokamar")) & "', " & drutama("ropetugas") & ", " & drutama("rojenistransaksi") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select roid from M_11_ro where ronotransaksi='" & notransaksi & "' AND roinputuser= '" & userid & "' order by romodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_11_ro_Detail where idro = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idrodetail") & ", " & result(4) & ", '" & FixQuotes(dr1("jenis")) & "', " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idkjdetail") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("isclose") & ", " & dr1("iddokter") & ", '" & FixQuotes(dr1("namadokter")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', '" & FixQuotes(dr1("customtext11")) & "', '" & FixQuotes(dr1("customtext12")) & "', '" & FixQuotes(dr1("customtext13")) & "', '" & FixQuotes(dr1("customtext14")) & "', '" & FixQuotes(dr1("customtext15")) & "', '" & FixQuotes(dr1("customtext16")) & "', '" & FixQuotes(dr1("customtext17")) & "', '" & FixQuotes(dr1("customtext18")) & "', '" & FixQuotes(dr1("customtext19")) & "', '" & FixQuotes(dr1("customtext20")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixDouble(dr1("customdbl11")) & "', '" & FixDouble(dr1("customdbl12")) & "', '" & FixDouble(dr1("customdbl13")) & "', '" & FixDouble(dr1("customdbl14")) & "', '" & FixDouble(dr1("customdbl15")) & "', '" & FixDouble(dr1("customdbl16")) & "', '" & FixDouble(dr1("customdbl17")) & "', '" & FixDouble(dr1("customdbl18")) & "', '" & FixDouble(dr1("customdbl19")) & "', '" & FixDouble(dr1("customdbl20")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate11"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate12"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate13"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate14"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate15"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate16"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate17"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate18"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate19"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate20"))) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekpenjualan")) & "', " & dr1("idhppkhususkeluar") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("tipebarang")) & "')")
                    Next
                    sql = "Insert into M_11_ro_Detail(idrodetail, idro, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, idhppkhususkeluar, hpp, gudangtransit, gudangtujuan, tipebarang) values" & strValue2.ToString & ""
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

                If drutama("rostatus") = 2 Then
                    'If Len(updNilai) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m_11_ro_detail SET jmlrealisasi = (CASE idkjdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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

                    'Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatus, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("roidkj") & "'")
                    'Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                    If drutama("rojenistransaksi") = 0 Then
                        sql = "Update M_11_Kj set kjstatus = 3 where kjid = '" & drutama("roidkj") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If

                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim hpp As Double = 0, postinghpp As Double = 0, gudangg As String = "", bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    'ITEM DETAIL ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    'PROSES BARANG DETAIL KELUAR
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.harga, sid.diskon, sid.jmldiskon, sid.idhppkhususmasuk, sid.hpp, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.catatan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, si.siinputtgl, i.bhpp FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly <> 1 WHERE sid.idsi = '" & result(4) & "'")
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rod.idrodetail, rod.idlayanan, rod.namalayanan, rod.tipebarang, rod.jml, rod.satuan, rod.jmltotal, rod.satuandefault, rod.matauang, rod.kurs, rod.harga, rod.diskon, rod.jmldiskon, rod.idhppkhususkeluar, rod.hpp, rod.gudang, rod.gudangtransit, rod.gudangtujuan, rod.catatan, rod.costcenter, rod.divisi, rod.subdivisi, rod.proyek, ro.roinputtgl, i.bhpp FROM m_11_ro_detail rod JOIN m_11_ro ro ON rod.idro = ro.roid JOIN m1_item i ON rod.idlayanan = i.bid WHERE rod.idro = '" & result(4) & "'", myConn)
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT srd.idsrdetail, srd.idbarang, srd.namabarang, srd.tipebarang, srd.jml, srd.satuan, srd.jmlbarang, srd.satuanbarang, srd.matauang, srd.kurs, srd.harga, srd.diskon, srd.jmldiskon, srd.hpp, srd.idhppkhususkeluar, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.catatan, srd.costcenter, srd.divisi, srd.subdivisi, srd.proyek, sr.srinputtgl, i.bhpp, IFNULL(sid.hpp,srd.hpp)as hppbaru FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON srd.idsidetail=sid.idsidetail WHERE srd.idsr = '" & result(4) & "'")

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idlayanan"))
                            jmlbarang = Double.Parse(dr1("jmltotal"))
                            gudangg = dr1("gudang")

                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                            If dtSaldo.Rows.Count > 0 Then
                                'set nilai stok
                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                'jenismutasi dan postinghpp 
                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                jenismutasi = 1 : postinghpp = 0

                                'hitung saldojml = bstok - jmlbarang
                                saldojml = bstok + jmlbarang

                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                'QUERY INSERT TRANSAKSI BARANG
                                strTransaksiBarang.Clear()
                                'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rocabang")) & "', '" & FixQuotes(drutama("rolokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rokodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rosumber")) & "', " & result(4) & ", " & dr1("idrodetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rotgl"))) & "', " & drutama("rocustomer") & ", " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rouraian")) & "', '" & FixQuotes(drutama("rocatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("roinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("roinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK PERGUDANG
                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudangg & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK GLOBAL
                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            End If

                        Next

                        'Else
                        '    result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "RO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rostatus") = 2 Then
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

                'INSERT MSMQ HPP ====================================================================
                If drutama("rostatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString("C" & userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                    'MSMQ TABEL
                    sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
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
                    hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                    End If

                End If
                'END OF INSERT MSMQ HPP =============================================================

                'INSERT USER LOG ====================================================================
                'Dim sumber As String = "AK", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_RoUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "RO", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rotgl, Ronotransaksi, Rostatus, roidkj FROM M_11_Ro WHERE Roid='" & idtransaksi & "'", myConn)
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
                nilaiStatus = "Rostatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m5_sr_history
            'Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.m11_ro_terkait()
                sql = sql.Replace("validtransaksi", idtransaksi)
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
                sql &= " SELECT a.luid as idterkait, a.lusumber as sumber FROM m_11_lu a JOIN m_11_kj kj ON a.luidkj = kj.kjid AND a.lustatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rkid as idterkait, a.rksumber as sumber FROM m_11_rk a JOIN m_11_kj kj ON a.rkidkj = kj.kjid AND a.rkstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.rmid as idterkait, 'RM' as sumber FROM m_11_rm a JOIN m_11_kj kj ON a.rmidkj = kj.kjid AND a.rmstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.roid as idterkait, a.rosumber as sumber FROM m_11_ro a JOIN m_11_kj kj ON a.roidkj = kj.kjid AND a.rostatus IN(2,3,4,7) AND a.roid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
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

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idrodetail As Integer = 0
                Dim idhppkhususmasuk As Integer = 0, idhppkhususkeluar As Integer = 0
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT rod.idrodetail, rod.idlayanan, i.bkode as kodebarang, rod.tipebarang, rod.namalayanan, rod.satuan, rod.nilaisatuan, rod.jmltotal, rod.gudang, rod.gudangtransit, rod.gudangtujuan, rod.idhppkhususkeluar, rod.urutan, i.bhpp FROM m_11_ro_detail rod JOIN m1_item i ON rod.idlayanan = i.bid WHERE rod.idro = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idlayanan") : jmlbarang = dr1("jmltotal")
                        gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan")
                        idhppkhususkeluar = dr1("idhppkhususkeluar") : idrodetail = dr1("idrodetail")

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

                        '5. BUAT FILTER CEK HPP KHUSUS(I)
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idrodetail & "' AND sumber = 'RO')")

                        '6. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idrodetail & "' AND cfisumber = 'RO')")

                        '7 SET NILAI UPDATE STOK BARANG
                        Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang)
                        updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN bstok - '" & stokBarang & "' ", updStokBarang)

                        '8. SET FILTERUPDATE STOK BARANG
                        ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                        ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistStok, ftStok, ftHppI, ftHppF)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------

                'DELETE HPP KHUSUS (I)
                sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE HPP FIFO (F)
                sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'UPDATE STOK ====================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                ''STOK MASUK
                'If Len(updStokIn) > 0 Then
                '    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myconn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If

                'STOK BARANG m1_item
                sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarang & " ELSE bstok END) WHERE " & ftStokBarang
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE STOK =============================================================


                'DELETE TRANSAKSI BARANG ========================================================
                'HAPUS DI M1_ITEM_TRANSACTION
                sql = "DELETE FROM m1_item_transaction WHERE sumber = '" & sumber & "' AND idutama = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG =================================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = '" & sumber & "' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M_11_ro SET Rostatus = " & nilaiStatus & ", Romodifikasiuser='" & userid & "', Romodifikasitgl = NOW(), Roposting = 0, Rotglposting = '1971-01-01 00:00:00', Rojmlrevisi = Rojmlrevisi + 1 WHERE Roid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_RoSearch(PostWsSearch(paramSplit(0), "M11_RoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_RoDelete(ByVal param As String) As String

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
            Dim sumber As String = "Ro", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT roid, ronotransaksi FROM M_11_ro WHERE roid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rocabang, rolokasi, rosumber, roautonotransaksi, ronotransaksi, rotgl"
            sql &= " FROM M_11_ro"
            sql &= " WHERE roid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rocabang")
                lokasi = dtNomorNext.Rows(0)("rolokasi")
                sumber = dtNomorNext.Rows(0)("rosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("roautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("ronotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_11_ro_Detail WHERE idro = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_11_ro WHERE roid = '" & idtransaksi & "'"
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
    Public Function M11_RoGetdataById(ByVal param As String) As String
        'M11_Ro_GetdataById Utama --------------------------------------------------------
        'akid, akcabang, aklokasi, akgudang, aksumber, 
        'akautonotransaksi, aknotransaksi, aktgl, akkodepa, akcustomer, 
        'akcustomerkontak, akuraian, akcatatan, aknoref, aktglnoref, 
        'aktotaltransaksi, akidkj, akstatusrealisasi, akstatus, akstatussebelumnya, 
        'akjmlrevisi, akcetakanke, akinputuser, akinputtgl, akmodifikasiuser, 
        'akmodifikasitgl, akisclose, akcustomtext1, akcustomtext2, akcustomtext3, 
        'akcustomtext4, akcustomtext5, akcustomtext6, akcustomtext7, akcustomtext8,
        'akcustomtext9, akcustomtext10, akcustomtext11, akcustomtext12, akcustomtext13,
        'akcustomtext14, akcustomtext15, akcustomtext16, akcustomtext17, akcustomtext18,
        'akcustomtext19, akcustomtext20, akcustomint1, akcustomint2, akcustomint3,
        'akcustomint4, akcustomint5, akcustomint6, akcustomint7, akcustomint8,
        'akcustomint9, akcustomint10, akcustomint11, akcustomint12, akcustomint13,
        'akcustomint14, akcustomint15, akcustomint16, akcustomint17, akcustomint18,
        'akcustomint19, akcustomint20, akcustomdbl1, akcustomdbl2, akcustomdbl3, 
        'akcustomdbl4, akcustomdbl5, akcustomdbl6, akcustomdbl7, akcustomdbl8,
        'akcustomdbl9, akcustomdbl10, akcustomdbl11, akcustomdbl12, akcustomdbl13,
        'akcustomdbl14, akcustomdbl15, akcustomdbl16, akcustomdbl17, akcustomdbl18,
        'akcustomdbl19, akcustomdbl20, akcustomdate1, akcustomdate2, akcustomdate3, 
        'akcustomdate4, akcustomdate5, akcustomdate6, akcustomdate7, akcustomdate8,
        'akcustomdate9, akcustomdate10, akcustomdate11, akcustomdate12, akcustomdate13,
        'akcustomdate14, akcustomdate15, akcustomdate16, akcustomdate17, akcustomdate18,
        'akcustomdate19, akcustomdate20, akcabangnama, aklokasinama, akgudangnama, 
        'akcustomerkode, akcustomernama, aknotransaksikj, akstatusnama, akstatussebelumnyanama, 
        'akinputusernama, akmodifikasiusernama, roperawatan, rokategoripasien, rokamar
        'rokategoripasiennama, rokamarnama

        'M11_Ro_GetdataById Detail --------------------------------------------------------
        'idakdetail, idak, jenis, idlayanan, namalayanan, 
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
        'kodedokter

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

        Dim NmMemcached As String = "aplikasi1-M11_Ro~M11_Ro_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "roid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "roid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_ro_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("roid"), 0), sptField,
                     FxDB(drutama("rocabang"), ""), sptField,
                     FxDB(drutama("rolokasi"), ""), sptField,
                     FxDB(drutama("rogudang"), ""), sptField,
                     FxDB(drutama("rosumber"), ""), sptField,
                     FxDB(drutama("roautonotransaksi"), 0), sptField,
                     FxDB(drutama("ronotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rokodepa"), 0), sptField,
                     FxDB(drutama("rocustomer"), 0), sptField,
                     FxDB(drutama("rocustomerkontak"), ""), sptField,
                     FxDB(drutama("rouraian"), ""), sptField,
                     FxDB(drutama("rocatatan"), ""), sptField,
                     FxDB(drutama("ronoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rotglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("rototaltransaksi"), 0), sptField,
                     FxDB(drutama("roidkj"), 0), sptField,
                     FxDB(drutama("rostatusrealisasi"), 0), sptField,
                     FxDB(drutama("rostatus"), 0), sptField,
                     FxDB(drutama("rostatussebelumnya"), 0), sptField,
                     FxDB(drutama("rojmlrevisi"), 0), sptField,
                     FxDB(drutama("rocetakanke"), 0), sptField,
                     FxDB(drutama("roinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("roinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("romodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("romodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("roisclose"), 0), sptField,
                     FxDB(drutama("rocustomtext1"), ""), sptField,
                     FxDB(drutama("rocustomtext2"), ""), sptField,
                     FxDB(drutama("rocustomtext3"), ""), sptField,
                     FxDB(drutama("rocustomtext4"), ""), sptField,
                     FxDB(drutama("rocustomtext5"), ""), sptField,
                     FxDB(drutama("rocustomtext6"), ""), sptField,
                     FxDB(drutama("rocustomtext7"), ""), sptField,
                     FxDB(drutama("rocustomtext8"), ""), sptField,
                     FxDB(drutama("rocustomtext9"), ""), sptField,
                     FxDB(drutama("rocustomtext10"), ""), sptField,
                     FxDB(drutama("rocustomtext11"), ""), sptField,
                     FxDB(drutama("rocustomtext12"), ""), sptField,
                     FxDB(drutama("rocustomtext13"), ""), sptField,
                     FxDB(drutama("rocustomtext14"), ""), sptField,
                     FxDB(drutama("rocustomtext15"), ""), sptField,
                     FxDB(drutama("rocustomtext16"), ""), sptField,
                     FxDB(drutama("rocustomtext17"), ""), sptField,
                     FxDB(drutama("rocustomtext18"), ""), sptField,
                     FxDB(drutama("rocustomtext19"), ""), sptField,
                     FxDB(drutama("rocustomtext20"), ""), sptField,
                     FxDB(drutama("rocustomint1"), 0), sptField,
                     FxDB(drutama("rocustomint2"), 0), sptField,
                     FxDB(drutama("rocustomint3"), 0), sptField,
                     FxDB(drutama("rocustomint4"), 0), sptField,
                     FxDB(drutama("rocustomint5"), 0), sptField,
                     FxDB(drutama("rocustomint6"), 0), sptField,
                     FxDB(drutama("rocustomint7"), 0), sptField,
                     FxDB(drutama("rocustomint8"), 0), sptField,
                     FxDB(drutama("rocustomint9"), 0), sptField,
                     FxDB(drutama("rocustomint10"), 0), sptField,
                     FxDB(drutama("rocustomint11"), 0), sptField,
                     FxDB(drutama("rocustomint12"), 0), sptField,
                     FxDB(drutama("rocustomint13"), 0), sptField,
                     FxDB(drutama("rocustomint14"), 0), sptField,
                     FxDB(drutama("rocustomint15"), 0), sptField,
                     FxDB(drutama("rocustomint16"), 0), sptField,
                     FxDB(drutama("rocustomint17"), 0), sptField,
                     FxDB(drutama("rocustomint18"), 0), sptField,
                     FxDB(drutama("rocustomint19"), 0), sptField,
                     FxDB(drutama("rocustomint20"), 0), sptField,
                     FxDB(drutama("rocustomdbl1"), 0), sptField,
                     FxDB(drutama("rocustomdbl2"), 0), sptField,
                     FxDB(drutama("rocustomdbl3"), 0), sptField,
                     FxDB(drutama("rocustomdbl4"), 0), sptField,
                     FxDB(drutama("rocustomdbl5"), 0), sptField,
                     FxDB(drutama("rocustomdbl6"), 0), sptField,
                     FxDB(drutama("rocustomdbl7"), 0), sptField,
                     FxDB(drutama("rocustomdbl8"), 0), sptField,
                     FxDB(drutama("rocustomdbl9"), 0), sptField,
                     FxDB(drutama("rocustomdbl10"), 0), sptField,
                     FxDB(drutama("rocustomdbl11"), 0), sptField,
                     FxDB(drutama("rocustomdbl12"), 0), sptField,
                     FxDB(drutama("rocustomdbl13"), 0), sptField,
                     FxDB(drutama("rocustomdbl14"), 0), sptField,
                     FxDB(drutama("rocustomdbl15"), 0), sptField,
                     FxDB(drutama("rocustomdbl16"), 0), sptField,
                     FxDB(drutama("rocustomdbl17"), 0), sptField,
                     FxDB(drutama("rocustomdbl18"), 0), sptField,
                     FxDB(drutama("rocustomdbl19"), 0), sptField,
                     FxDB(drutama("rocustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rocustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("rocabangnama"), ""), sptField,
                     FxDB(drutama("rolokasinama"), ""), sptField,
                     FxDB(drutama("rogudangnama"), ""), sptField,
                     FxDB(drutama("rocustomerkode"), ""), sptField,
                     FxDB(drutama("rocustomernama"), ""), sptField,
                     FxDB(drutama("ronotransaksikj"), ""), sptField,
                     FxDB(drutama("rostatusnama"), ""), sptField,
                     FxDB(drutama("rostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("roinputusernama"), ""), sptField,
                     FxDB(drutama("romodifikasiusernama"), ""), sptField,
                     FxDB(drutama("romatauang"), ""), sptField,
                     FxDB(drutama("rokurs"), 0), sptField,
                     FxDB(drutama("roposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rotglposting"), ""), formatTgl), sptField,
                     FxDB(drutama("ronama"), ""), sptField,
                     FxDB(drutama("rotingkatjual"), 0), sptField,
                     FxDB(drutama("roperawatan"), ""), sptField,
                     FxDB(drutama("rokategoripasien"), ""), sptField,
                     FxDB(drutama("rokamar"), ""), sptField,
                     FxDB(drutama("rokategoripasiennama"), ""), sptField,
                     FxDB(drutama("rokamarnama"), ""), sptField,
                     FxDB(drutama("ropetugas"), 0), sptField,
                     FxDB(drutama("ropetugaskode"), ""), sptField,
                     FxDB(drutama("ropetugasnama"), ""), sptField,
                     FxDB(drutama("rojenistransaksi"), ""))
            ' FxDB(drutama("aktingkatjual"), 1))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrodetail"), 0), sptField,
                     FxDB(dr("idro"), 0), sptField,
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
                     FxDB(dr("iddokter"), 0), sptField,
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
                     FxDB(dr("rekpenjualan"), ""), sptField,
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("roid, rocabang, rolokasi, rogudang, rosumber, roautonotransaksi, ronotransaksi, rotgl, rokodepa, rocustomer, rocustomerkontak, rouraian, rocatatan, ronoref, rotglnoref, rototaltransaksi, roidkj, rostatusrealisasi, rostatus, rostatussebelumnya, rojmlrevisi, rocetakanke, roinputuser, roinputtgl, romodifikasiuser, romodifikasitgl, roisclose, rocustomtext1, rocustomtext2, rocustomtext3, rocustomtext4, rocustomtext5, rocustomtext6, rocustomtext7, rocustomtext8, rocustomtext9, rocustomtext10, rocustomtext11, rocustomtext12, rocustomtext13, rocustomtext14, rocustomtext15, rocustomtext16, rocustomtext17, rocustomtext18, rocustomtext19, rocustomtext20, rocustomint1, rocustomint2, rocustomint3, rocustomint4, rocustomint5, rocustomint6, rocustomint7, rocustomint8, rocustomint9, rocustomint10, rocustomint11, rocustomint12, rocustomint13, rocustomint14, rocustomint15, rocustomint16, rocustomint17, rocustomint18, rocustomint19, rocustomint20, rocustomdbl1, rocustomdbl2, rocustomdbl3, rocustomdbl4, rocustomdbl5, rocustomdbl6, rocustomdbl7, rocustomdbl8, rocustomdbl9, rocustomdbl10, rocustomdbl11, rocustomdbl12, rocustomdbl13, rocustomdbl14, rocustomdbl15, rocustomdbl16, rocustomdbl17, rocustomdbl18, rocustomdbl19, rocustomdbl20, rocustomdate1, rocustomdate2, rocustomdate3, rocustomdate4, rocustomdate5, rocustomdate6, rocustomdate7, rocustomdate8, rocustomdate9, rocustomdate10, rocustomdate11, rocustomdate12, rocustomdate13, rocustomdate14, rocustomdate15, rocustomdate16, rocustomdate17, rocustomdate18, rocustomdate19, rocustomdate20, rocabangnama, rolokasinama, rogudangnama,  rocustomerkode, rocustomernama, ronotransaksikj, rostatusnama, rostatussebelumnyanama, roinputusernama, romodifikasiusernama, romatauang, rokurs, roposting, rotglposting, ronama, rotingkatjual, roperawatan, rokategoripasien, rokamar, rokategoripasiennama, rokamarnama, ropetugas, ropetugaskode, ropetugasnama, rojenistransaksi" & sptSubParam & "idrodetail, idro, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, idhppkhususkeluar, hpp, gudangtransit, gudangtujuan, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RoSearch(ByVal param As String) As String
        'M11_RoSearch --------------------------------------------------------
        'akid, akcabang, aklokasi, akgudang, akasalbarang, akasalbarangkategori, akjenispenjualan, 
        'akjenispenjualankategori, akcarabayar, aksumber, akautonotransaksi, aknotransaksi, aktgl, akkodepa, 
        'akcustomer, akcustomerkontak, ak1alamat1, ak1alamat2, ak1alamat3, ak2alamat1, ak2alamat2, 
        'ak2alamat3, akbagianpenjualan, akekspedisi, aktglkirim, aktermin, aktgljatuhtempo, akuraian, 
        'akcatatan, aknoref, aktglnoref, aktglpenutupan, akmatauang, akkurs, akhargatermasukpajak, 
        'aktotal, akdiskonpersen, akjmldiskon, aktotalpajak1detail, aktotalpajak2detail, akbiayalainpersen, akbiayalain, 
        'aktotaltransaksi, akjmlbayar, akrekdiskon, akrekpajak1, akrekpajak2, akrekbiayalain, akrekbayar, 
        'akidsq, akstatuspl, akstatusdo, akstatusdr, akstatuspi, akstatussi, akstatusrnr, 
        'akstatussr, akstatusrealisasi, akstatus, akstatussebeakmnya, akjmlrevisi, akcetakanke, akinputuser, 
        'akinputtgl, akmodifikasiuser, akmodifikasitgl, akposting, akpostingtgl, akisclose, akcabangnama, 
        'aklokasinama, akgudangnama, akcustomerkode, akcustomernama, akbagianpenjualankode, akbagianpenjualannama, akekspedisinama, 
        'aknotransaksikj, akstatusnama, akstatussebelumnyanama, akinputusernama, akmodifikasiusernama
        'roperawatan, rokategoripasien, rokamar

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
            Filter = Filter.Replace("ronotransaksikj", "kj.kjnotransaksi")
            Filter = Filter.Replace("ronorm", "p.pkode")
            Filter = Filter.Replace("ronama", "p.pnama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_ro_v")

        dt = AmbilData("aplikasi1-M11_Ro_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("roid"), 0), sptField,
                     FxDB(dr("rocabang"), ""), sptField,
                     FxDB(dr("rolokasi"), ""), sptField,
                     FxDB(dr("rogudang"), ""), sptField,
                     FxDB(dr("rosumber"), ""), sptField,
                     FxDB(dr("roautonotransaksi"), 0), sptField,
                     FxDB(dr("ronotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rotgl"), ""), formatTgl), sptField,
                     FxDB(dr("rokodepa"), 0), sptField,
                     FxDB(dr("rocustomer"), 0), sptField,
                     FxDB(dr("rocustomerkontak"), ""), sptField,
                     FxDB(dr("rouraian"), ""), sptField,
                     FxDB(dr("rocatatan"), ""), sptField,
                     FxDB(dr("ronoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rotglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rototaltransaksi"), 0), sptField,
                     FxDB(dr("roidkj"), 0), sptField,
                     FxDB(dr("rostatusrealisasi"), 0), sptField,
                     FxDB(dr("rostatus"), 0), sptField,
                     FxDB(dr("rostatussebelumnya"), 0), sptField,
                     FxDB(dr("rojmlrevisi"), 0), sptField,
                     FxDB(dr("rocetakanke"), 0), sptField,
                     FxDB(dr("roinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("roinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("romodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("romodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("roisclose"), 0), sptField,
                     FxDB(dr("rocabangnama"), ""), sptField,
                     FxDB(dr("rolokasinama"), ""), sptField,
                     FxDB(dr("rogudangnama"), ""), sptField,
                     FxDB(dr("rocustomerkode"), ""), sptField,
                     FxDB(dr("rocustomernama"), ""), sptField,
                     FxDB(dr("ronotransaksikj"), ""), sptField,
                     FxDB(dr("rostatusnama"), ""), sptField,
                     FxDB(dr("rostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("roinputusernama"), ""), sptField,
                     FxDB(dr("romodifikasiusernama"), ""), sptField,
                     FxDB(dr("roperawatan"), ""), sptField,
                     FxDB(dr("rokategoripasien"), ""), sptField,
                     FxDB(dr("rokamar"), ""), sptField,
                     FxDB(dr("ronama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("roid, rocabang, rolokasi, rogudang, rosumber, roautonotransaksi, ronotransaksi, rotgl, rokodepa, rocustomer, rocustomerkontak, rouraian, rocatatan, ronoref, rotglnoref, rototaltransaksi, roidkj, rostatusrealisasi, rostatus, rostatussebelumnya, rojmlrevisi, rocetakanke, roinputuser, roinputtgl, romodifikasiuser, romodifikasitgl, roisclose, rocabangnama, rolokasinama, rogudangnama, rocustomerkode, rocustomernama, ronotransaksikj, rostatusnama, rostatussebelumnyanama, roinputusernama, romodifikasiusernama, roperawatan, rokategoripasien, rokamar, ronama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RoTerkait(ByVal param As String) As String
        'M11_RoTerkait --------------------------------------------------------
        'akid, aknotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m11_ro_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_ro_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("roid"), 0), sptField,
                     FxDB(dr("ronotransaksi"), ""), sptField,
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
            result(2) = "Related RO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("roid, ronotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_Ro_Detail_VSearch(ByVal param As String) As String
        'M11_Ro_Detail_VSearch --------------------------------------------------------
        'idakdetail, idak, jenis, idlayanan, namalayanan, 
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
        'customdate19, customdate20, aknotransaksi, akuraian, akcatatan,
        'aknoref, aktgl, aktglnoref, akcustomerkontak, kodelayanan,
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,
        'akcustomer, akcustomerkode, akcustomernama, kodedokter

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
            Filter = Filter.Replace("idlayanan", "akd.idlayanan")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sol = query.PanggilQuery("M11_Ro_detail_v")

        dt = AmbilData("aplikasi1-M11_Ro_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idrodetail"), 0), sptField,
                     FxDB(dr("idro"), 0), sptField,
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
                     FxDB(dr("iddokter"), 0), sptField,
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
                     FxDB(dr("ronotransaksi"), ""), sptField,
                     FxDB(dr("rouraian"), ""), sptField,
                     FxDB(dr("rocatatan"), ""), sptField,
                     FxDB(dr("ronoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rotgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rotglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rocustomerkontak"), ""), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("rocustomer"), ""), sptField,
                     FxDB(dr("rocustomerkode"), ""), sptField,
                     FxDB(dr("rocustomernama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idrodetail, idro, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3,customtext4, customtext5, customtext6, customtext7, customtext8,customtext9, customtext10, customtext11, customtext12, customtext13,customtext14, customtext15, customtext16, customtext17, customtext18,customtext19, customtext20, customdbl1, customdbl2, customdbl3,customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,customdbl19, customdbl20, customdate1, customdate2, customdate3,customdate4, customdate5, customdate6, customdate7, customdate8,customdate9, customdate10, customdate11, customdate12, customdate13,customdate14, customdate15, customdate16, customdate17, customdate18,customdate19, customdate20, ronotransaksi, rouraian, rocatatan, ronoref, rotgl, rotglnoref, rocustomerkontak, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,rocustomer, rocustomerkode, rocustomernama, kodedokter"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_RoCekNoRef(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(3) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi(4) As String

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        'If (IsNumeric(paramSplit(3)) = False) Then
        'result(2) = "userid required numeric." : GoTo selesai
        'End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        pagingSplit = paramSplit(2).Split(sptSubParam)
        idtransaksi = paramSplit(3).Split("~")

        'result(2) = "nananana " & idtransaksi(0) & " " & idtransaksi(1) & " " & idtransaksi.Length.ToString : GoTo selesai

        If (idtransaksi.Length <> 4) Then
            result(2) = "Invalid filter parameter." : GoTo selesai
        End If

        If (Len(idtransaksi(0)) = 0) Then
            result(2) = "ronoref can't be empty." : GoTo selesai
            'Else
            'SET IDTRANSAKSI
            '   idtransaksi = idtransaksi(0)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        If (idtransaksi(1) = "RI") Then
            dt = AsDataTableAmbilDariDB("SELECT COUNT(ronoref) FROM m_11_ro WHERE roperawatan = '" & idtransaksi(1) & "' AND rokategoripasien = '" & idtransaksi(2) & "' AND ronoref='" & idtransaksi(0) & "' AND YEAR(rotgl) = '" & idtransaksi(3) & "'")
        Else
            'result(2) = "nanananaa" : GoTo selesai
            dt = AsDataTableAmbilDariDB("SELECT COUNT(ronoref) FROM m_11_ro WHERE roperawatan = '" & idtransaksi(1) & "' AND ronoref='" & idtransaksi(0) & "' AND YEAR(rotgl) = '" & idtransaksi(3) & "'")
        End If
        'dt = AsDataTableAmbilDariDB("SELECT COUNT(aknoref) FROM m_11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND akkategoripasien = '" & idtransaksi(2) & "' AND aknoref='" & idtransaksi(0) & "' AND YEAR(aktgl) = '" & idtransaksi(3) & "'")
        'result(2) = "SELECT COUNT(aknoref) FROM m11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND akkategoripasien = '" & idtransaksi(2) & "' AND aknoref='" & idtransaksi(0) & "'" : GoTo selesai
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "No Retur '" & idtransaksi(0) & "' sudah dipakai." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi(0)
        'END OF CEK DI DATABASE ==========================================================


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

    Private Function ValidasiHppF(ByVal dtdetail As DataTable, ByVal ftBarang As String) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtval As New DataTable, dtbarang As New DataTable, dtHppF As New DataTable, dtLookup As New DataTable
        Dim ftExistHppF As String = "", ftHppF As String = "", havingHppF As String = "", filterLookup As String = ""
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaisatuan As Double = 0, urutan As Double = 0, sisa As Double = 0

        '1. AMBIL BARANG HPP FIFO (F)
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item WHERE (bjenis <> 'J') AND (bhpp = 'F') AND (" & ftBarang & ")")
        '2. CEK ID HPP FIFO MASUK
        If dtbarang.Rows.Count > 0 Then
            '3. PERULANGAN SEBANYAK BARANG HPP FIFO
            For Each dr1 As DataRow In dtbarang.Rows
                '4. AMBIL BARANG HPP FIFO DARI DETAIL
                dtHppF = AsDataTableFilterSortDt(dtdetail, "idlayanan = '" & dr1("bid") & "'")
                If dtHppF.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppF.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP FIFO
                        ftExistHppF = IIf(Len(ftExistHppF.ToString) = 0, "", ftExistHppF & " UNION ")
                        ftExistHppF = String.Concat(ftExistHppF, "SELECT EXISTS(SELECT 1 FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & dr1("bid") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")
                        '6. BUAT FILTER CEK JML HPP FIFO
                        Dim StokHppF As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & dr1("bid") & "")
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, " (cfiidbarang = '" & dr1("bid") & "' AND cfiisclose = 0) ")
                        havingHppF = IIf(Len(havingHppF.ToString) = 0, "", havingHppF & " OR ")
                        havingHppF = String.Concat(havingHppF, " (cfiidbarang = '" & dr1("bid") & "' AND " & StokHppF & " > cfitotalsisa) ")
                    Next
                End If
            Next

            'VALIDASI HPP FIFO (F) ------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistHppF) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistHppF) 'ftExistHppI = rowExists, idbarang, bkode
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idlayanan=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namalayanan")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS FIFO list." : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA YG TERSEDIA
            If Len(ftHppF) > 0 Then
                sql = "SELECT bkode, cfiidbarang, SUM(cfisisa) as cfitotalsisa FROM m1_cogs_fifo_in JOIN m1_item ON cfiidbarang = bid WHERE " & ftHppF & " GROUP BY cfiidbarang HAVING " & havingHppF
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("cfitotalsisa")

                    filterLookup = "idlayanan=" & dtval.Rows(0)("cfiidbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namalayanan")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaisatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS FIFO, item(s) available " & sisa / nilaisatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI HPP FIFO (F) -----------------------------
        End If

selesai:
        Return errmessage
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftHppI As String, ByVal ftHppF As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = ""

        'VALIDASI HPP -----------------------------------------------
        'HPP KHUSUS (I)
        If Len(ftHppI) > 0 Then
            dtval = AsDataTableAmbilDariDB("SELECT idbarang, bkode FROM m1_cogs_special_in JOIN m1_item ON idbarang = bid AND bjenis <> 'J' WHERE (" & ftHppI & ") AND jmlkeluar > 0")
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")
                errmessage = "COGS Special for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " has related transactions." : GoTo selesai
            End If
        End If

        'HPP FIFO (F)
        If Len(ftHppF) > 0 Then
            dtval = AsDataTableAmbilDariDB("SELECT cfiidbarang, bkode FROM m1_cogs_fifo_in JOIN m1_item ON cfiidbarang = bid AND bjenis <> 'J' WHERE (" & ftHppI & ") AND cfijmlkeluar > 0")
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                filterLookup = "idbarang=" & dtval.Rows(0)("cfiidbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")
                errmessage = "COGS FIFO for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " has related transactions." : GoTo selesai
            End If
        End If
        'END OF VALIDASI HPP ----------------------------------------

        'VALIDASI STOK ----------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistStok) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistStok) 'ftExistStok = rowExists, idbarang, bkode, gudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                gudang = dtval.Rows(0)("gudang")

                filterLookup = "idlayanan=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namalayanan")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in '" & gudang & "' warehouse" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK PERGUDANG YG TERSEDIA
        If Len(ftStok) > 0 Then
            'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' WHERE " & ftStok
            'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStok
            'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
            sql = "SELECT isw.idbarang, isw.kgudang, isw.stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' WHERE " & ftStok
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("stok")
                gudang = dtval.Rows(0)("kgudang")

                filterLookup = "idlayanan=" & dtval.Rows(0)("idbarang")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namalayanan")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaisatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI STOK ---------------------------------------

selesai:
        Return errmessage
    End Function

End Class