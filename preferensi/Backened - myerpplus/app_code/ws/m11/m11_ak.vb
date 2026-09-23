Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_ak
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_AkSimpan(ByVal param As String) As String
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
        'akperawatan(110) As String, akkategoripasien(111) As String, akkamar(112) As String, akawalankatpasien(113) As String

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
        'akperawatan, akkategoripasien, akkamar, akawalankatpasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 122) Then
            result(2) = dataUtama.Length & " Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'akid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "akid required numeric." : GoTo selesai
        End If
        'akautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "akautonotransaksi required numeric." : GoTo selesai
        End If
        'aktgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "aktgl required date." : GoTo selesai
        End If
        'akkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "akkodepa required numeric." : GoTo selesai
        End If
        'akcustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "akcustomer required numeric." : GoTo selesai
        End If
        'aktglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "aktglnoref required date." : GoTo selesai
        End If
        'aktotaltransaksi(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "aktotaltransaksi required numeric." : GoTo selesai
        End If
        'akidkj(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "akidkj required numeric." : GoTo selesai
        End If
        'akstatusrealisasi(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "akstatusrealisasi required numeric." : GoTo selesai
        End If
        'akstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "akstatus required numeric." : GoTo selesai
        End If
        'akstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "akstatussebelumnya required numeric." : GoTo selesai
        End If
        'akjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "akjmlrevisi required numeric." : GoTo selesai
        End If
        'akcetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "akcetakanke required numeric." : GoTo selesai
        End If
        'akinputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "akinputuser required numeric." : GoTo selesai
        End If
        'akinputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "akinputtgl required date." : GoTo selesai
        End If
        'akmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "akmodifikasiuser required numeric." : GoTo selesai
        End If
        'akmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "akmodifikasitgl required date." : GoTo selesai
        End If
        'akisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "akisclose required numeric." : GoTo selesai
        End If
        'akcustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "akcustomint1 required numeric." : GoTo selesai
        End If
        'akcustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "akcustomint2 required numeric." : GoTo selesai
        End If
        'akcustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "akcustomint3 required numeric." : GoTo selesai
        End If
        'akcustomint4(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "akcustomint4 required numeric." : GoTo selesai
        End If
        'akcustomint5(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "akcustomint5 required numeric." : GoTo selesai
        End If
        'akcustomint6(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "akcustomint6 required numeric." : GoTo selesai
        End If
        'akcustomint7(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "akcustomint7 required numeric." : GoTo selesai
        End If
        'akcustomint8(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "akcustomint8 required numeric." : GoTo selesai
        End If
        'akcustomint9(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "akcustomint9 required numeric." : GoTo selesai
        End If
        'akcustomint10(56) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "akcustomint10 required numeric." : GoTo selesai
        End If
        'akcustomint11(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "akcustomint11 required numeric." : GoTo selesai
        End If
        'akcustomint12(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "akcustomint12 required numeric." : GoTo selesai
        End If
        'akcustomint13(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "akcustomint13 required numeric." : GoTo selesai
        End If
        'akcustomint14(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "akcustomint14 required numeric." : GoTo selesai
        End If
        'akcustomint15(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "akcustomint15 required numeric." : GoTo selesai
        End If
        'akcustomint16(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "akcustomint16 required numeric." : GoTo selesai
        End If
        'akcustomint17(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "akcustomint17 required numeric." : GoTo selesai
        End If
        'akcustomint18(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "akcustomint18 required numeric." : GoTo selesai
        End If
        'akcustomint19(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "akcustomint19 required numeric." : GoTo selesai
        End If
        'akcustomint20(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "akcustomint20 required numeric." : GoTo selesai
        End If
        'akcustomdbl1(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "akcustomdbl1 required numeric." : GoTo selesai
        End If
        'akcustomdbl2(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "akcustomdbl2 required numeric." : GoTo selesai
        End If
        'akcustomdbl3(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "akcustomdbl3 required numeric." : GoTo selesai
        End If
        'akcustomdbl4(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "akcustomdbl4 required numeric." : GoTo selesai
        End If
        'akcustomdbl5(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "akcustomdbl5 required numeric." : GoTo selesai
        End If
        'akcustomdbl6(72) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "akcustomdbl6 required numeric." : GoTo selesai
        End If
        'akcustomdbl7(73) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "akcustomdbl7 required numeric." : GoTo selesai
        End If
        'akcustomdbl8(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "akcustomdbl8 required numeric." : GoTo selesai
        End If
        'akcustomdbl9(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "akcustomdbl9 required numeric." : GoTo selesai
        End If
        'akcustomdbl10(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "akcustomdbl10 required numeric." : GoTo selesai
        End If
        'akcustomdbl11(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "akcustomdbl11 required numeric." : GoTo selesai
        End If
        'akcustomdbl12(78) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "akcustomdbl12 required numeric." : GoTo selesai
        End If
        'akcustomdbl13(79) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "akcustomdbl13 required numeric." : GoTo selesai
        End If
        'akcustomdbl14(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "akcustomdbl14 required numeric." : GoTo selesai
        End If
        'akcustomdbl15(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "akcustomdbl15 required numeric." : GoTo selesai
        End If
        'akcustomdbl16(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "akcustomdbl16 required numeric." : GoTo selesai
        End If
        'akcustomdbl17(83) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "akcustomdbl17 required numeric." : GoTo selesai
        End If
        'akcustomdbl18(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "akcustomdbl18 required numeric." : GoTo selesai
        End If
        'akcustomdbl19(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "akcustomdbl19 required numeric." : GoTo selesai
        End If
        'akcustomdbl20(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "akcustomdbl20 required numeric." : GoTo selesai
        End If
        'akcustomdate1(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "akcustomdate1 required date." : GoTo selesai
        End If
        'akcustomdate2(88) As Date
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "akcustomdate2 required date." : GoTo selesai
        End If
        'akcustomdate3(89) As Date
        If (IsDate(dataUtama(89)) = False) Then
            result(2) = "akcustomdate3 required date." : GoTo selesai
        End If
        'akcustomdate4(90) As Date
        If (IsDate(dataUtama(90)) = False) Then
            result(2) = "akcustomdate4 required date." : GoTo selesai
        End If
        'akcustomdate5(91) As Date
        If (IsDate(dataUtama(91)) = False) Then
            result(2) = "akcustomdate5 required date." : GoTo selesai
        End If
        'akcustomdate6(92) As Date
        If (IsDate(dataUtama(92)) = False) Then
            result(2) = "akcustomdate6 required date." : GoTo selesai
        End If
        'akcustomdate7(93) As Date
        If (IsDate(dataUtama(93)) = False) Then
            result(2) = "akcustomdate7 required date." : GoTo selesai
        End If
        'akcustomdate8(94) As Date
        If (IsDate(dataUtama(94)) = False) Then
            result(2) = "akcustomdate8 required date." : GoTo selesai
        End If
        'akcustomdate9(95) As Date
        If (IsDate(dataUtama(95)) = False) Then
            result(2) = "akcustomdate9 required date." : GoTo selesai
        End If
        'akcustomdate10(96) As Date
        If (IsDate(dataUtama(96)) = False) Then
            result(2) = "akcustomdate10 required date." : GoTo selesai
        End If
        'akcustomdate11(97) As Date
        If (IsDate(dataUtama(97)) = False) Then
            result(2) = "akcustomdate11 required date." : GoTo selesai
        End If
        'akcustomdate12(98) As Date
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "akcustomdate12 required date." : GoTo selesai
        End If
        'akcustomdate13(99) As Date
        If (IsDate(dataUtama(99)) = False) Then
            result(2) = "akcustomdate13 required date." : GoTo selesai
        End If
        'akcustomdate14(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "akcustomdate14 required date." : GoTo selesai
        End If
        'akcustomdate15(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "akcustomdate15 required date." : GoTo selesai
        End If
        'akcustomdate16(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "akcustomdate16 required date." : GoTo selesai
        End If
        'akcustomdate17(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "akcustomdate17 required date." : GoTo selesai
        End If
        'akcustomdate18(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "akcustomdate18 required date." : GoTo selesai
        End If
        'akcustomdate19(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "akcustomdate19 required date." : GoTo selesai
        End If
        'akcustomdate20(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "akcustomdate20 required date." : GoTo selesai
        End If
        'akkurs(108) As Double
        dataUtama(108) = 1
        If (IsNumeric(dataUtama(108)) = False) Then
            result(2) = "akkurs required numeric." : GoTo selesai
        End If
        'akposting(109) As Integer
        If (IsNumeric(dataUtama(109)) = False) Then
            result(2) = "akposting required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'akcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "akcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "akcabang should not be more than 25 character." : GoTo selesai
        End If

        'aklokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "aklokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "aklokasi should not be more than 25 character." : GoTo selesai
        End If

        'akgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "akgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "akgudang should not be more than 25 character." : GoTo selesai
        End If

        'aksumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "aksumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "aksumber should not be more than 10 character." : GoTo selesai
        End If

        'aknotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "aknotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "aknotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'aktgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "aktgl can't be empty" : GoTo selesai
        End If

        'aktglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "aktglnoref can't be empty" : GoTo selesai
        End If

        'aktotaltransaksi(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "aktotaltransaksi can't be empty" : GoTo selesai
        End If

        'akinputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "akinputtgl can't be empty" : GoTo selesai
        End If

        'akmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "akmodifikasitgl can't be empty" : GoTo selesai
        End If

        'akcustomdbl1(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "akcustomdbl1 can't be empty" : GoTo selesai
        End If

        'akcustomdbl2(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "akcustomdbl2 can't be empty" : GoTo selesai
        End If

        'akcustomdbl3(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "akcustomdbl3 can't be empty" : GoTo selesai
        End If

        'akcustomdbl4(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "akcustomdbl4 can't be empty" : GoTo selesai
        End If

        'akcustomdbl5(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "akcustomdbl5 can't be empty" : GoTo selesai
        End If

        'akcustomdbl6(72) As Double
        If Len(dataUtama(72)) = 0 Then
            result(2) = "akcustomdbl6 can't be empty" : GoTo selesai
        End If

        'akcustomdbl7(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "akcustomdbl7 can't be empty" : GoTo selesai
        End If

        'akcustomdbl8(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "akcustomdbl8 can't be empty" : GoTo selesai
        End If

        'akcustomdbl9(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "akcustomdbl9 can't be empty" : GoTo selesai
        End If

        'akcustomdbl10(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "akcustomdbl10 can't be empty" : GoTo selesai
        End If

        'akcustomdbl11(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "akcustomdbl11 can't be empty" : GoTo selesai
        End If

        'akcustomdbl12(78) As Double
        If Len(dataUtama(78)) = 0 Then
            result(2) = "akcustomdbl12 can't be empty" : GoTo selesai
        End If

        'akcustomdbl13(79) As Double
        If Len(dataUtama(79)) = 0 Then
            result(2) = "akcustomdbl13 can't be empty" : GoTo selesai
        End If

        'akcustomdbl14(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "akcustomdbl14 can't be empty" : GoTo selesai
        End If

        'akcustomdbl15(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "akcustomdbl15 can't be empty" : GoTo selesai
        End If

        'akcustomdbl16(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "akcustomdbl16 can't be empty" : GoTo selesai
        End If

        'akcustomdbl17(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "akcustomdbl17 can't be empty" : GoTo selesai
        End If

        'akcustomdbl18(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "akcustomdbl18 can't be empty" : GoTo selesai
        End If

        'akcustomdbl19(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "akcustomdbl19 can't be empty" : GoTo selesai
        End If

        'akcustomdbl20(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "akcustomdbl20 can't be empty" : GoTo selesai
        End If

        'akcustomdate1(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "akcustomdate1 can't be empty" : GoTo selesai
        End If

        'akcustomdate2(88) As Date
        If Len(dataUtama(88)) = 0 Then
            result(2) = "akcustomdate2 can't be empty" : GoTo selesai
        End If

        'akcustomdate3(89) As Date
        If Len(dataUtama(89)) = 0 Then
            result(2) = "akcustomdate3 can't be empty" : GoTo selesai
        End If

        'akcustomdate4(90) As Date
        If Len(dataUtama(90)) = 0 Then
            result(2) = "akcustomdate4 can't be empty" : GoTo selesai
        End If

        'akcustomdate5(91) As Date
        If Len(dataUtama(91)) = 0 Then
            result(2) = "akcustomdate5 can't be empty" : GoTo selesai
        End If

        'akcustomdate6(92) As Date
        If Len(dataUtama(92)) = 0 Then
            result(2) = "akcustomdate6 can't be empty" : GoTo selesai
        End If

        'akcustomdate7(93) As Date
        If Len(dataUtama(93)) = 0 Then
            result(2) = "akcustomdate7 can't be empty" : GoTo selesai
        End If

        'akcustomdate8(94) As Date
        If Len(dataUtama(94)) = 0 Then
            result(2) = "akcustomdate8 can't be empty" : GoTo selesai
        End If

        'akcustomdate9(95) As Date
        If Len(dataUtama(95)) = 0 Then
            result(2) = "akcustomdate9 can't be empty" : GoTo selesai
        End If

        'akcustomdate10(96) As Date
        If Len(dataUtama(96)) = 0 Then
            result(2) = "akcustomdate10 can't be empty" : GoTo selesai
        End If

        'akcustomdate11(97) As Date
        If Len(dataUtama(97)) = 0 Then
            result(2) = "akcustomdate11 can't be empty" : GoTo selesai
        End If

        'akcustomdate12(98) As Date
        If Len(dataUtama(98)) = 0 Then
            result(2) = "akcustomdate12 can't be empty" : GoTo selesai
        End If

        'akcustomdate13(99) As Date
        If Len(dataUtama(99)) = 0 Then
            result(2) = "akcustomdate13 can't be empty" : GoTo selesai
        End If

        'akcustomdate14(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "akcustomdate14 can't be empty" : GoTo selesai
        End If

        'akcustomdate15(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "akcustomdate15 can't be empty" : GoTo selesai
        End If

        'akcustomdate16(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "akcustomdate16 can't be empty" : GoTo selesai
        End If

        'akcustomdate17(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "akcustomdate17 can't be empty" : GoTo selesai
        End If

        'akcustomdate18(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "akcustomdate18 can't be empty" : GoTo selesai
        End If

        'akcustomdate19(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "akcustomdate19 can't be empty" : GoTo selesai
        End If

        'akcustomdate20(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "akcustomdate20 can't be empty" : GoTo selesai
        End If

        'akmatauang(107) As String
        If Len(dataUtama(107)) = 0 Then
            result(2) = "akmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(107)) > 25 Then
            result(2) = "akmatauang should not be more than 25 character." : GoTo selesai
        End If

        'akkurs(108) As Double
        dataUtama(108) = 1
        If Len(dataUtama(108)) = 0 Then
            result(2) = "akkurs can't be empty" : GoTo selesai
        End If
        'akperawatan, akkategoripasien, akkamar, akawalankatpasien
        'akawalankatpasien(113) As String


        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "akid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aklokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aksumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aknotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aktgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aknoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aktglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "aktotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akpenjualanlangsung", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "akdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "aktotalobat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akresep", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akracik", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akembalase", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "akketerangan", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "akid~akcabang~aklokasi~akgudang~aksumber~akautonotransaksi~aknotransaksi~aktgl~akkodepa~akcustomer~akcustomerkontak~akuraian~akcatatan~aknoref~aktglnoref~aktotaltransaksi~akidkj~akstatusrealisasi~akstatus~akstatussebelumnya~akjmlrevisi~akcetakanke~akinputuser~akinputtgl~akmodifikasiuser~akmodifikasitgl~akisclose~akcustomtext1~akcustomtext2~akcustomtext3~akcustomtext4~akcustomtext5~akcustomtext6~akcustomtext7~akcustomtext8~akcustomtext9~akcustomtext10~akcustomtext11~akcustomtext12~akcustomtext13~akcustomtext14~akcustomtext15~akcustomtext16~akcustomtext17~akcustomtext18~akcustomtext19~akcustomtext20~akcustomint1~akcustomint2~akcustomint3~akcustomint4~akcustomint5~akcustomint6~akcustomint7~akcustomint8~akcustomint9~akcustomint10~akcustomint11~akcustomint12~akcustomint13~akcustomint14~akcustomint15~akcustomint16~akcustomint17~akcustomint18~akcustomint19~akcustomint20~akcustomdbl1~akcustomdbl2~akcustomdbl3~akcustomdbl4~akcustomdbl5~akcustomdbl6~akcustomdbl7~akcustomdbl8~akcustomdbl9~akcustomdbl10~akcustomdbl11~akcustomdbl12~akcustomdbl13~akcustomdbl14~akcustomdbl15~akcustomdbl16~akcustomdbl17~akcustomdbl18~akcustomdbl19~akcustomdbl20~akcustomdate1~akcustomdate2~akcustomdate3~akcustomdate4~akcustomdate5~akcustomdate6~akcustomdate7~akcustomdate8~akcustomdate9~akcustomdate10~akcustomdate11~akcustomdate12~akcustomdate13~akcustomdate14~akcustomdate15~akcustomdate16~akcustomdate17~akcustomdate18~akcustomdate19~akcustomdate20~akmatauang~akkurs~akposting~akperawatan~akkategoripasien~akkamar~akawalankatpasien~akpenjualanlangsung~akdokter~akpetugas~aktotalobat~akresep~akracik~akembalase~akketerangan", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121)) = False Then
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
        AsDataTableTambahField(dtdetail, "idakdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idak", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
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
                result(2) = "Row : " & i & " - idakdetail required numeric." : GoTo selesai
            End If
            'idak(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idak required numeric." : GoTo selesai
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
            'idhppkhususmasuk(98) As Integer
            If (IsNumeric(dataRowDetail(98)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'hpp(99) As Double
            If (IsNumeric(dataRowDetail(99)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'jenis(2) As String
            'If Len(dataRowDetail(2)) = 0 Then
            '    result(2) = "Row : " & i & " - jenis can't be empty" : GoTo selesai
            'End If
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

            If AsDataTableTambahData(dtdetail, "idakdetail~idak~jenis~idlayanan~namalayanan~jml~satuan~nilaisatuan~jmltotal~satuandefault~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idkjdetail~jmlrealisasi~statusrealisasi~isclose~iddokter~namadokter~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customtext11~customtext12~customtext13~customtext14~customtext15~customtext16~customtext17~customtext18~customtext19~customtext20~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdbl11~customdbl12~customdbl13~customdbl14~customdbl15~customdbl16~customdbl17~customdbl18~customdbl19~customdbl20~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~customdate11~customdate12~customdate13~customdate14~customdate15~customdate16~customdate17~customdate18~customdate19~customdate20~matauang~kurs~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekpenjualan~idhppkhususmasuk~hpp~gudangtransit~gudangtujuan~tipebarang", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76) & "~" & dataRowDetail(77) & "~" & dataRowDetail(78) & "~" & dataRowDetail(79) & "~" & dataRowDetail(80) & "~" & dataRowDetail(81) & "~" & dataRowDetail(82) & "~" & dataRowDetail(83) & "~" & dataRowDetail(84) & "~" & dataRowDetail(85) & "~" & dataRowDetail(86) & "~" & dataRowDetail(87) & "~" & dataRowDetail(88) & "~" & dataRowDetail(89) & "~" & dataRowDetail(90) & "~" & dataRowDetail(91) & "~" & dataRowDetail(92) & "~" & dataRowDetail(93) & "~" & dataRowDetail(94) & "~" & dataRowDetail(95) & "~" & dataRowDetail(96) & "~" & dataRowDetail(97) & "~" & dataRowDetail(98) & "~" & dataRowDetail(99) & "~" & dataRowDetail(100) & "~" & dataRowDetail(101) & "~" & dataRowDetail(102)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'result(2) = dataRowDetail(98) & " " & dataRowDetail(99) & " " & dataRowDetail(100) & " " & dataRowDetail(101) : GoTo selesai
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idlayanan(3) As Integer     , jmltotal(8) As Double       , gudang(19) As String       , idkjdetail(26) As Integer
            idlayanan = dataRowDetail(3) : idbarang = dataRowDetail(3) : jmltotal = dataRowDetail(8) : gudang = dataRowDetail(19) : gudangOut = dataRowDetail(19) : idkjdetail = dataRowDetail(26)
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
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bassembly <> 1 AND bid = '" & idbarang & "'")

            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang & " AND gudang='" & gudangOut & "'")
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
            ftStokAvailable = IIf(Len(ftStokAvailable.ToString) = 0, "", ftStokAvailable & " OR ")
            ftStokAvailable = String.Concat(ftStokAvailable, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
            'End If

            'SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            'SET NILAI UPDATE STOK M1_ITEM
            Dim stokKeluar As Double = AsDataTableDSum(dtdetail, "jmltotal", "idlayanan=" & idbarang)

            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN bstok - '" & stokKeluar & "' ", updStokBarang)
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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 6
                Select Case drutama("akstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("aktgl")), AsFormatTanggal(drutama("aktgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("akstatus") = 2 Or drutama("akstatus") = 1 Or drutama("akstatus") = 8 Or drutama("akstatus") = 9 Or drutama("akstatus") = 10 Or drutama("akstatus") = 11 Then

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
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistStok, ftStokAvailable)

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
                    result(4) = drutama("akid")
                    notransaksi = drutama("aknotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(akid), aknotransaksi, aknoref FROM M_11_ak WHERE akid='" & result(4) & "' AND akstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)
                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid) FROM m_11_ak WHERE aknotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============


                        'CEK NO RESEP ==========================
                        If FixQuotes(drutama("aknoref")) <> dtupdate.Rows(0)(2).ToString Then
                            If FixQuotes(drutama("akperawatan")) = "RI" Then
                                Dim dtCekNoRef As DataTable = AsDataTableAmbilDariDBCon("  SELECT COUNT(akid), aknoref, aknotransaksi FROM m_11_ak WHERE YEAR(aktgl) = YEAR('" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "') AND aknoref = '" & FixQuotes(drutama("aknoref")) & "' AND akperawatan = '" & FixQuotes(drutama("akperawatan")) & "' AND akkategoripasien = '" & FixQuotes(drutama("akkategoripasien")) & "'", myConn)
                                Dim cekNoRef As Double = Val(dtCekNoRef.Rows(0)(0))
                                If cekNoRef > 0 Then
                                    result(2) = "No Resep '" & dtCekNoRef.Rows(0)(1) & "' sudah digunakan di nomor transaksi '" & dtCekNoRef.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                                End If
                            ElseIf FixQuotes(drutama("akperawatan")) = "RJ" Then
                                Dim dtCekNoRefRJ As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid), aknoref, aknotransaksi FROM m_11_ak WHERE YEAR(aktgl) = YEAR('" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "') AND aknoref = '" & FixQuotes(drutama("aknoref")) & "' AND akperawatan = '" & FixQuotes(drutama("akperawatan")) & "'", myConn)
                                Dim cekNoRefRJ As Double = Val(dtCekNoRefRJ.Rows(0)(0))
                                If cekNoRefRJ > 0 Then
                                    result(2) = "No Resep '" & dtCekNoRefRJ.Rows(0)(1) & "' sudah digunakan di nomor transaksi '" & dtCekNoRefRJ.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If
                        'END OF CEK NO RESEP ===================


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

                        sql = "Update M_11_ak set akcabang  = '" & FixQuotes(drutama("akcabang")) & "', aklokasi  = '" & FixQuotes(drutama("aklokasi")) & "', akgudang  = '" & FixQuotes(drutama("akgudang")) & "', aksumber  = '" & FixQuotes(drutama("aksumber")) & "', akautonotransaksi  = " & drutama("akautonotransaksi") & ", aknotransaksi  = '" & FixQuotes(notransaksi) & "', aktgl  = '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', akkodepa  = " & drutama("akkodepa") & ", akcustomer  = " & drutama("akcustomer") & ", akcustomerkontak  = '" & FixQuotes(drutama("akcustomerkontak")) & "', akuraian  = '" & FixQuotes(drutama("akuraian")) & "', akcatatan  = '" & FixQuotes(drutama("akcatatan")) & "', aknoref  = '" & FixQuotes(drutama("aknoref")) & "', aktglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("aktglnoref"))) & "', aktotaltransaksi  = '" & FixDouble(drutama("aktotaltransaksi")) & "', akidkj  = " & drutama("akidkj") & ", akstatusrealisasi  = " & drutama("akstatusrealisasi") & ", akstatus  = " & drutama("akstatus") & ", akstatussebelumnya  = " & drutama("akstatussebelumnya") & ", akjmlrevisi  = akjmlrevisi+1, akcetakanke  = " & drutama("akcetakanke") & ", akmodifikasiuser  = " & drutama("akmodifikasiuser") & ", akmodifikasitgl  = NOW(), akcustomtext1  = '" & FixQuotes(drutama("akcustomtext1")) & "', akcustomtext2  = '" & FixQuotes(drutama("akcustomtext2")) & "', akcustomtext3  = '" & FixQuotes(drutama("akcustomtext3")) & "', akcustomtext4  = '" & FixQuotes(drutama("akcustomtext4")) & "', akcustomtext5  = '" & FixQuotes(drutama("akcustomtext5")) & "', akcustomtext6  = '" & FixQuotes(drutama("akcustomtext6")) & "', akcustomtext7  = '" & FixQuotes(drutama("akcustomtext7")) & "', akcustomtext8  = '" & FixQuotes(drutama("akcustomtext8")) & "', akcustomtext9  = '" & FixQuotes(drutama("akcustomtext9")) & "', akcustomtext10  = '" & FixQuotes(drutama("akcustomtext10")) & "', akcustomtext11  = '" & FixQuotes(drutama("akcustomtext11")) & "', akcustomtext12  = '" & FixQuotes(drutama("akcustomtext12")) & "', akcustomtext13  = '" & FixQuotes(drutama("akcustomtext13")) & "', akcustomtext14  = '" & FixQuotes(drutama("akcustomtext14")) & "', akcustomtext15  = '" & FixQuotes(drutama("akcustomtext15")) & "', akcustomtext16  = '" & FixQuotes(drutama("akcustomtext16")) & "', akcustomtext17  = '" & FixQuotes(drutama("akcustomtext17")) & "', akcustomtext18  = '" & FixQuotes(drutama("akcustomtext18")) & "', akcustomtext19  = '" & FixQuotes(drutama("akcustomtext19")) & "', akcustomtext20  = '" & FixQuotes(drutama("akcustomtext20")) & "', akcustomint1  = " & drutama("akcustomint1") & ", akcustomint2  = " & drutama("akcustomint2") & ", akcustomint3  = " & drutama("akcustomint3") & ", akcustomint4  = " & drutama("akcustomint4") & ", akcustomint5  = " & drutama("akcustomint5") & ", akcustomint6  = " & drutama("akcustomint6") & ", akcustomint7  = " & drutama("akcustomint7") & ", akcustomint8  = " & drutama("akcustomint8") & ", akcustomint9  = " & drutama("akcustomint9") & ", akcustomint10  = " & drutama("akcustomint10") & ", akcustomint11  = " & drutama("akcustomint11") & ", akcustomint12  = " & drutama("akcustomint12") & ", akcustomint13  = " & drutama("akcustomint13") & ", akcustomint14  = " & drutama("akcustomint14") & ", akcustomint15  = " & drutama("akcustomint15") & ", akcustomint16  = " & drutama("akcustomint16") & ", akcustomint17  = " & drutama("akcustomint17") & ", akcustomint18  = " & drutama("akcustomint18") & ", akcustomint19  = " & drutama("akcustomint19") & ", akcustomint20  = " & drutama("akcustomint20") & ", akcustomdbl1  = '" & FixDouble(drutama("akcustomdbl1")) & "', akcustomdbl2  = '" & FixDouble(drutama("akcustomdbl2")) & "', akcustomdbl3  = '" & FixDouble(drutama("akcustomdbl3")) & "', akcustomdbl4  = '" & FixDouble(drutama("akcustomdbl4")) & "', akcustomdbl5  = '" & FixDouble(drutama("akcustomdbl5")) & "', akcustomdbl6  = '" & FixDouble(drutama("akcustomdbl6")) & "', akcustomdbl7  = '" & FixDouble(drutama("akcustomdbl7")) & "', akcustomdbl8  = '" & FixDouble(drutama("akcustomdbl8")) & "', akcustomdbl9  = '" & FixDouble(drutama("akcustomdbl9")) & "', akcustomdbl10  = '" & FixDouble(drutama("akcustomdbl10")) & "', akcustomdbl11  = '" & FixDouble(drutama("akcustomdbl11")) & "', akcustomdbl12  = '" & FixDouble(drutama("akcustomdbl12")) & "', akcustomdbl13  = '" & FixDouble(drutama("akcustomdbl13")) & "', akcustomdbl14  = '" & FixDouble(drutama("akcustomdbl14")) & "', akcustomdbl15  = '" & FixDouble(drutama("akcustomdbl15")) & "', akcustomdbl16  = '" & FixDouble(drutama("akcustomdbl16")) & "', akcustomdbl17  = '" & FixDouble(drutama("akcustomdbl17")) & "', akcustomdbl18  = '" & FixDouble(drutama("akcustomdbl18")) & "', akcustomdbl19  = '" & FixDouble(drutama("akcustomdbl19")) & "', akcustomdbl20  = '" & FixDouble(drutama("akcustomdbl20")) & "', akcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate1"))) & "', akcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate2"))) & "', akcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate3"))) & "', akcustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate4"))) & "', akcustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate5"))) & "', akcustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate6"))) & "', akcustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate7"))) & "', akcustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate8"))) & "', akcustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate9"))) & "', akcustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate10"))) & "', akcustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate11"))) & "', akcustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate12"))) & "', akcustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate13"))) & "', akcustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate14"))) & "', akcustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate15"))) & "', akcustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate16"))) & "', akcustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate17"))) & "', akcustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate18"))) & "', akcustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate19"))) & "', akcustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate20"))) & "', akmatauang  = '" & FixQuotes(drutama("akmatauang")) & "', akkurs  = '" & FixDouble(drutama("akkurs")) & "', akposting  = 0, akperawatan  = '" & FixDouble(drutama("akperawatan")) & "', akkategoripasien  = '" & FixDouble(drutama("akkategoripasien")) & "', akkamar  = '" & FixDouble(drutama("akkamar")) & "', akpenjualanlangsung = " & drutama("akpenjualanlangsung") & ", akdokter  = '" & FixDouble(drutama("akdokter")) & "', akpetugas = " & drutama("akpetugas") & ", aktotalobat  = '" & FixDouble(drutama("aktotalobat")) & "', akresep  = '" & FixDouble(drutama("akresep")) & "', akracik  = '" & FixDouble(drutama("akracik")) & "', akembalase  = '" & FixDouble(drutama("akembalase")) & "', akketerangan = " & drutama("akketerangan") & " where akid = '" & drutama("akid") & "'"
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

                    If FixQuotes(drutama("akperawatan")) = "RI" Then
                        Dim dtCekNoRef As DataTable = AsDataTableAmbilDariDBCon("  SELECT COUNT(akid), aknoref, aknotransaksi FROM m_11_ak WHERE YEAR(aktgl) = YEAR('" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "') AND aknoref = '" & FixQuotes(drutama("aknoref")) & "' AND akperawatan = '" & FixQuotes(drutama("akperawatan")) & "' AND akkategoripasien = '" & FixQuotes(drutama("akkategoripasien")) & "'", myConn)
                        Dim cekNoRef As Double = Val(dtCekNoRef.Rows(0)(0))
                        If cekNoRef > 0 Then
                            result(2) = "No Resep '" & dtCekNoRef.Rows(0)(1) & "' sudah digunakan di nomor transaksi '" & dtCekNoRef.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                        End If
                    ElseIf FixQuotes(drutama("akperawatan")) = "RJ" Then
                        Dim dtCekNoRefRJ As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid), aknoref, aknotransaksi FROM m_11_ak WHERE YEAR(aktgl) = YEAR('" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "') AND aknoref = '" & FixQuotes(drutama("aknoref")) & "' AND akperawatan = '" & FixQuotes(drutama("akperawatan")) & "'", myConn)
                        Dim cekNoRefRJ As Double = Val(dtCekNoRefRJ.Rows(0)(0))
                        If cekNoRefRJ > 0 Then
                            result(2) = "No Resep '" & dtCekNoRefRJ.Rows(0)(1) & "' sudah digunakan di nomor transaksi '" & dtCekNoRefRJ.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                    If drutama("akautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        'Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("akcabang"), drutama("aklokasi"), drutama("aksumber"), drutama("aktgl"))

                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("akperawatan"), drutama("akawalankatpasien"), drutama("aksumber"), drutama("aktgl"))
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
                        notransaksi = drutama("aknotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(akid) FROM m_11_ak WHERE aknotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_11_ak (akcabang, aklokasi, akgudang, aksumber, akautonotransaksi, aknotransaksi, aktgl, akkodepa, akcustomer, akcustomerkontak, akuraian, akcatatan, aknoref, aktglnoref, aktotaltransaksi, akidkj, akstatusrealisasi, akstatus, akstatussebelumnya, akjmlrevisi, akcetakanke, akinputuser, akinputtgl, akmodifikasiuser, akmodifikasitgl, akisclose, akcustomtext1, akcustomtext2, akcustomtext3, akcustomtext4, akcustomtext5, akcustomtext6, akcustomtext7, akcustomtext8, akcustomtext9, akcustomtext10, akcustomtext11, akcustomtext12, akcustomtext13, akcustomtext14, akcustomtext15, akcustomtext16, akcustomtext17, akcustomtext18, akcustomtext19, akcustomtext20, akcustomint1, akcustomint2, akcustomint3, akcustomint4, akcustomint5, akcustomint6, akcustomint7, akcustomint8, akcustomint9, akcustomint10, akcustomint11, akcustomint12, akcustomint13, akcustomint14, akcustomint15, akcustomint16, akcustomint17, akcustomint18, akcustomint19, akcustomint20, akcustomdbl1, akcustomdbl2, akcustomdbl3, akcustomdbl4, akcustomdbl5, akcustomdbl6, akcustomdbl7, akcustomdbl8, akcustomdbl9, akcustomdbl10, akcustomdbl11, akcustomdbl12, akcustomdbl13, akcustomdbl14, akcustomdbl15, akcustomdbl16, akcustomdbl17, akcustomdbl18, akcustomdbl19, akcustomdbl20, akcustomdate1, akcustomdate2, akcustomdate3, akcustomdate4, akcustomdate5, akcustomdate6, akcustomdate7, akcustomdate8, akcustomdate9, akcustomdate10, akcustomdate11, akcustomdate12, akcustomdate13, akcustomdate14, akcustomdate15, akcustomdate16, akcustomdate17, akcustomdate18, akcustomdate19, akcustomdate20, akmatauang, akkurs, akperawatan, akkategoripasien, akkamar, akpenjualanlangsung, akdokter, akpetugas, aktotalobat, akresep, akracik, akembalase, akketerangan) values('" & FixQuotes(drutama("akcabang")) & "', '" & FixQuotes(drutama("aklokasi")) & "', '" & FixQuotes(drutama("akgudang")) & "', '" & FixQuotes(drutama("aksumber")) & "', " & drutama("akautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & drutama("akkodepa") & ", " & drutama("akcustomer") & ", '" & FixQuotes(drutama("akcustomerkontak")) & "', '" & FixQuotes(drutama("akuraian")) & "', '" & FixQuotes(drutama("akcatatan")) & "', '" & FixQuotes(drutama("aknoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktglnoref"))) & "', '" & FixDouble(drutama("aktotaltransaksi")) & "', " & drutama("akidkj") & ", " & drutama("akstatusrealisasi") & ", " & drutama("akstatus") & ", " & drutama("akstatussebelumnya") & ", " & drutama("akjmlrevisi") & ", " & drutama("akcetakanke") & ", " & drutama("akinputuser") & ", NOW(), " & drutama("akmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("akisclose") & ", '" & FixQuotes(drutama("akcustomtext1")) & "', '" & FixQuotes(drutama("akcustomtext2")) & "', '" & FixQuotes(drutama("akcustomtext3")) & "', '" & FixQuotes(drutama("akcustomtext4")) & "', '" & FixQuotes(drutama("akcustomtext5")) & "', '" & FixQuotes(drutama("akcustomtext6")) & "', '" & FixQuotes(drutama("akcustomtext7")) & "', '" & FixQuotes(drutama("akcustomtext8")) & "', '" & FixQuotes(drutama("akcustomtext9")) & "', '" & FixQuotes(drutama("akcustomtext10")) & "', '" & FixQuotes(drutama("akcustomtext11")) & "', '" & FixQuotes(drutama("akcustomtext12")) & "', '" & FixQuotes(drutama("akcustomtext13")) & "', '" & FixQuotes(drutama("akcustomtext14")) & "', '" & FixQuotes(drutama("akcustomtext15")) & "', '" & FixQuotes(drutama("akcustomtext16")) & "', '" & FixQuotes(drutama("akcustomtext17")) & "', '" & FixQuotes(drutama("akcustomtext18")) & "', '" & FixQuotes(drutama("akcustomtext19")) & "', '" & FixQuotes(drutama("akcustomtext20")) & "', " & drutama("akcustomint1") & ", " & drutama("akcustomint2") & ", " & drutama("akcustomint3") & ", " & drutama("akcustomint4") & ", " & drutama("akcustomint5") & ", " & drutama("akcustomint6") & ", " & drutama("akcustomint7") & ", " & drutama("akcustomint8") & ", " & drutama("akcustomint9") & ", " & drutama("akcustomint10") & ", " & drutama("akcustomint11") & ", " & drutama("akcustomint12") & ", " & drutama("akcustomint13") & ", " & drutama("akcustomint14") & ", " & drutama("akcustomint15") & ", " & drutama("akcustomint16") & ", " & drutama("akcustomint17") & ", " & drutama("akcustomint18") & ", " & drutama("akcustomint19") & ", " & drutama("akcustomint20") & ", '" & FixDouble(drutama("akcustomdbl1")) & "', '" & FixDouble(drutama("akcustomdbl2")) & "', '" & FixDouble(drutama("akcustomdbl3")) & "', '" & FixDouble(drutama("akcustomdbl4")) & "', '" & FixDouble(drutama("akcustomdbl5")) & "', '" & FixDouble(drutama("akcustomdbl6")) & "', '" & FixDouble(drutama("akcustomdbl7")) & "', '" & FixDouble(drutama("akcustomdbl8")) & "', '" & FixDouble(drutama("akcustomdbl9")) & "', '" & FixDouble(drutama("akcustomdbl10")) & "', '" & FixDouble(drutama("akcustomdbl11")) & "', '" & FixDouble(drutama("akcustomdbl12")) & "', '" & FixDouble(drutama("akcustomdbl13")) & "', '" & FixDouble(drutama("akcustomdbl14")) & "', '" & FixDouble(drutama("akcustomdbl15")) & "', '" & FixDouble(drutama("akcustomdbl16")) & "', '" & FixDouble(drutama("akcustomdbl17")) & "', '" & FixDouble(drutama("akcustomdbl18")) & "', '" & FixDouble(drutama("akcustomdbl19")) & "', '" & FixDouble(drutama("akcustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("akcustomdate20"))) & "', '" & FixQuotes(drutama("akmatauang")) & "', '" & FixDouble(drutama("akkurs")) & "', '" & FixDouble(drutama("akperawatan")) & "', '" & FixDouble(drutama("akkategoripasien")) & "', '" & FixDouble(drutama("akkamar")) & "', " & drutama("akpenjualanlangsung") & ", '" & FixDouble(drutama("akdokter")) & "', " & drutama("akpetugas") & ", '" & FixDouble(drutama("aktotalobat")) & "', '" & FixDouble(drutama("akresep")) & "', '" & FixDouble(drutama("akracik")) & "', '" & FixDouble(drutama("akembalase")) & "', " & drutama("akketerangan") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select akid from M_11_ak where aknotransaksi='" & notransaksi & "' AND akinputuser= '" & userid & "' order by akmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_11_ak_Detail where idak = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idakdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("jenis")) & "', " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idkjdetail") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("isclose") & ", " & dr1("iddokter") & ", '" & FixQuotes(dr1("namadokter")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', '" & FixQuotes(dr1("customtext11")) & "', '" & FixQuotes(dr1("customtext12")) & "', '" & FixQuotes(dr1("customtext13")) & "', '" & FixQuotes(dr1("customtext14")) & "', '" & FixQuotes(dr1("customtext15")) & "', '" & FixQuotes(dr1("customtext16")) & "', '" & FixQuotes(dr1("customtext17")) & "', '" & FixQuotes(dr1("customtext18")) & "', '" & FixQuotes(dr1("customtext19")) & "', '" & FixQuotes(dr1("customtext20")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixDouble(dr1("customdbl11")) & "', '" & FixDouble(dr1("customdbl12")) & "', '" & FixDouble(dr1("customdbl13")) & "', '" & FixDouble(dr1("customdbl14")) & "', '" & FixDouble(dr1("customdbl15")) & "', '" & FixDouble(dr1("customdbl16")) & "', '" & FixDouble(dr1("customdbl17")) & "', '" & FixDouble(dr1("customdbl18")) & "', '" & FixDouble(dr1("customdbl19")) & "', '" & FixDouble(dr1("customdbl20")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate11"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate12"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate13"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate14"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate15"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate16"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate17"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate18"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate19"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate20"))) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekpenjualan")) & "', " & dr1("idhppkhususmasuk") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("tipebarang")) & "')")
                    Next
                    sql = "Insert into M_11_ak_Detail(idakdetail, idak, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, idhppkhususmasuk, hpp, gudangtransit, gudangtujuan, tipebarang) values" & strValue2.ToString & ""
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

                If drutama("akstatus") = 2 Then
                    'If Len(updNilai) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m_11_ak_detail SET jmlrealisasi = (CASE idkjdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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

                    If drutama("akpenjualanlangsung") = 0 Then
                        Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatus, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("akidkj") & "'", myConn)
                        Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                        If cekKunjungan > 0 Then
                            sql = "Update M_11_Kj set kjstatus = 3 where kjid = '" & drutama("akidkj") & "'"
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
                    'result(2) = "Nanana" : Trans.Rollback() : GoTo selesai
                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim hpp As Double = 0, postinghpp As Double = 0, gudangg As String = "", bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    'ITEM DETAIL ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    'PROSES BARANG DETAIL KELUAR
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT sid.idsidetail, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.harga, sid.diskon, sid.jmldiskon, sid.idhppkhususmasuk, sid.hpp, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.catatan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, si.siinputtgl, i.bhpp FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid AND i.bassembly <> 1 WHERE sid.idsi = '" & result(4) & "'")
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT akd.idakdetail, akd.idlayanan, akd.namalayanan, akd.tipebarang, akd.jml, akd.satuan, akd.jmltotal, akd.satuandefault, akd.matauang, akd.kurs, akd.harga, akd.diskon, akd.jmldiskon, akd.idhppkhususmasuk, akd.hpp, akd.gudang, akd.gudangtransit, akd.gudangtujuan, akd.catatan, akd.costcenter, akd.divisi, akd.subdivisi, akd.proyek, ak.akinputtgl, i.bhpp FROM m_11_ak_detail akd JOIN m_11_ak ak ON akd.idak = ak.akid JOIN m1_item i ON akd.idlayanan = i.bid WHERE akd.idak = '" & result(4) & "'", myConn)
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
                                jenismutasi = 0 : postinghpp = 0

                                'hitung saldojml = bstok - jmlbarang
                                saldojml = bstok - jmlbarang

                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                'QUERY INSERT TRANSAKSI BARANG
                                strTransaksiBarang.Clear()
                                'mapping                        id,                            cabang,                                    lokasi,                                gudang,                         kodepa,             jenismutasi,                              sumber,                    idutama,             iddetail,                     notransaksi,                                                 tgl,                           kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                        inputtgl,                                                    inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("akcabang")) & "', '" & FixQuotes(drutama("aklokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("akkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("aksumber")) & "', " & result(4) & ", " & dr1("idakdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("aktgl"))) & "', " & drutama("akcustomer") & ", " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', CONCAT('" & FixQuotes(drutama("akuraian")) & "', ' ', '" & FixQuotes(drutama("aknoref")) & "'), '" & FixQuotes(drutama("akcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("akinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("akinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudangg & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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
                Dim sumber As String = "AK", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("akstatus") = 2 Then
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
                If drutama("akstatus") = 2 Then
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
    Public Function M11_AkUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("aknotransaksikj", "kj.kjnotransaksi")
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
            Dim sumber As String = "Ak", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT aktgl, aknotransaksi, akstatus, akidkj FROM M_11_ak WHERE akid='" & idtransaksi & "'", myConn)
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
                nilaiStatus = "akstatussebelumnya" : jnsaktivitas = 17
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
            'Dim SimpanHistory As New m5_so_history
            'Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            'result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m5_so_terkait("akid = '" & idtransaksi & "'")
                sql = query.PanggilQuery("m11_ak_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'UPDATE STATUS KJ ===============================================================
                'CEK TRANSAKSI TERKAIT KJ
                sql = "  SELECT * FROM ( "
                sql &= " SELECT a.akid as idterkait, a.aksumber as sumber FROM m_11_ak a JOIN m_11_kj kj ON a.akidkj = kj.kjid AND a.akstatus IN(2,3,4,7) AND a.akid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
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

                Dim idlayanan As Integer = 0, jmltotal As Double = 0, idkjdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""
                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idhppkhususmasuk As Integer = 0
                Dim gudangIn As String = "" ', updStokIn As String = ""
                Dim updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idakdetail, idlayanan, tipebarang, namalayanan, satuan, nilaisatuan, jmltotal, idhppkhususmasuk, gudangtujuan, urutan FROM m_11_ak_detail WHERE idak = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idlayanan") : jmlbarang = dr1("jmltotal") : idhppkhususmasuk = dr1("idhppkhususmasuk") : gudangIn = dr1("gudangtujuan")

                        '4. BUAT FILTER UPDATE HPP KHUSUS (I)
                        If idhppkhususmasuk <> 0 Then
                            'SET NILAI UPDATE HPP KHUSUS IN
                            Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmltotal", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                            updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN jmlkeluar - '" & jmlKeluar & "' ", updNilaiHppI)

                            'SET FILTER UPDATE HPP KHUSUS IN
                            updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                            updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")

                            'SET FILTER DELETE HPP KHUSUS OUT
                            delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                            delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'AK' AND idtransaksi = '" & dr1("idakdetail") & "' AND idbarang = '" & dr1("idlayanan") & "')")
                        End If

                        '5. BUAT FILTER UPDATE HPP FIFO (F)
                        filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                        filterHppF = String.Concat(filterHppF, "(cfosumber = 'AK' AND cfoidtransaksi = '" & dr1("idakdetail") & "' AND cfoidbarang = '" & dr1("idlayanan") & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'CEK HPP FIFO ====================================================================
                'AMBIL DATA DARI HPP FIFO KELUAR - m1_cogs_fifo_out
                Dim dtHppF As DataTable = AsDataTableAmbilDariDBCon("SELECT * FROM m1_cogs_fifo_out WHERE " & filterHppF, myConn)
                If dtHppF.Rows.Count > 0 Then
                    Dim idhppfifoin As Integer = 0
                    For Each dr1 As DataRow In dtHppF.Rows
                        'SET NILAI VARIABEL
                        idhppfifoin = dr1("cfoidcfi")

                        'SET FILTER DELETE HPP FIFO OUT
                        delFilterHppF = IIf(Len(delFilterHppF.ToString) = 0, "", delFilterHppF & " OR ")
                        delFilterHppF = String.Concat(delFilterHppF, "(cfosumber = 'AK' AND cfoidtransaksi = '" & dr1("cfoidtransaksi") & "' AND cfoidbarang = '" & dr1("cfoidbarang") & "')")

                        'SET NILAI UPDATE HPP FIFO IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtHppF, "cfojmlkeluar", "cfoidcfi='" & idhppfifoin & "'")
                        updNilaiHppF = String.Concat("WHEN '" & idhppfifoin & "' THEN cfijmlkeluar - '" & jmlKeluar & "' ", updNilaiHppF)

                        'SET FILTER UPDATE HPP FIFO IN
                        updFilterHppF = IIf(Len(updFilterHppF.ToString) = 0, "", updFilterHppF & " OR ")
                        updFilterHppF = String.Concat(updFilterHppF, "(cfiid = '" & idhppfifoin & "')")
                    Next
                End If
                'END OF CEK HPP FIFO =============================================================

                'UPDATE HPP KHUSUS (I) =========================================================
                'DELETE HPP KHUSUS OUT
                If Len(delFilterHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_out WHERE " & delFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP KHUSUS IN
                If Len(updNilaiHppI) > 0 Then
                    sql = "UPDATE m1_cogs_special_in SET jmlkeluar = (CASE idhppikm " & updNilaiHppI & " ELSE jmlkeluar END) WHERE " & updFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'DELETE HPP KHUSUS IN
                sql = "DELETE csi FROM m1_cogs_special_in csi JOIN m_11_ak_detail akd ON csi.sumber = 'AK' AND csi.idtransaksi = akd.idakdetail AND csi.idbarang = akd.idlayanan WHERE akd.idak = '" & FixDouble(idtransaksi) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE HPP KHUSUS (I) ==================================================


                'UPDATE HPP FIFO (F) ===========================================================
                'DELETE HPP FIFO OUT
                If Len(delFilterHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_out WHERE " & delFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE HPP FIFO IN
                If Len(updNilaiHppF) > 0 Then
                    sql = "UPDATE m1_cogs_fifo_in SET cfijmlkeluar = (CASE cfiid " & updNilaiHppF & " ELSE cfijmlkeluar END) WHERE " & updFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'DELETE HPP FIFO IN
                sql = "DELETE cfi FROM m1_cogs_fifo_in cfi JOIN m_11_ak_detail akd ON cfi.cfisumber = 'AK' AND cfi.cfiidtransaksi = akd.idakdetail AND cfi.cfiidbarang = akd.idlayanan WHERE akd.idak = '" & FixDouble(idtransaksi) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE HPP FIFO (F) ====================================================

                'STOK MASUK
                sql = "INSERT INTO m1_item_stock_warehouse ( SELECT * FROM( SELECT akd.idlayanan, akd.gudangtujuan, akd.jmltotal FROM m_11_ak_detail akd JOIN m1_item i ON akd.idlayanan = i.bid AND i.bassembly <> 1 WHERE akd.idak = '" & FixDouble(idtransaksi) & "' )as stok ) ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'STOK BARANG m1_item
                Dim dtStokGlobal As New DataTable
                sql = "SELECT stok.idlayanan FROM ( SELECT akd.idlayanan FROM m_11_ak_detail akd JOIN m1_item i ON i.bid = akd.idlayanan AND i.bassembly <> 1 WHERE akd.idak = '" & FixDouble(idtransaksi) & "') as stok GROUP BY idlayanan"
                dtStokGlobal = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtStokGlobal.Rows.Count > 0 Then
                    For Each dr As DataRow In dtStokGlobal.Rows
                        sql = "UPDATE m1_item SET bstok = IFNULL((SELECT SUM(isw.stok) FROM m1_item_stock_warehouse isw WHERE isw.idbarang = '" & FixDouble(dr("idlayanan")) & "' GROUP BY isw.idbarang),0) WHERE bid = '" & FixDouble(dr("idlayanan")) & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Next
                End If
                ''END OF UPDATE STOK =============================================================

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
            sql = "UPDATE M_11_ak SET akstatus = " & nilaiStatus & ", akmodifikasiuser='" & userid & "', akmodifikasitgl = NOW(), akjmlrevisi = akjmlrevisi + 1 WHERE akid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_AkSearch(PostWsSearch(paramSplit(0), "M11_AkSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_AkDelete(ByVal param As String) As String

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
            Dim sumber As String = "Ak", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT akid, aknotransaksi FROM M_11_ak WHERE akid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT akcabang, aklokasi, aksumber, akautonotransaksi, aknotransaksi, aktgl"
            sql &= " FROM M_11_ak"
            sql &= " WHERE akid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("akcabang")
                lokasi = dtNomorNext.Rows(0)("aklokasi")
                sumber = dtNomorNext.Rows(0)("aksumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("akautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("aknotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("aktgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M_11_ak_Detail WHERE idak = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_11_ak WHERE akid = '" & idtransaksi & "'"
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
    Public Function M11_AkGetdataById(ByVal param As String) As String
        'M11_ak_GetdataById Utama --------------------------------------------------------
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
        'akinputusernama, akmodifikasiusernama

        'M11_ak_GetdataById Detail --------------------------------------------------------
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

        Dim NmMemcached As String = "aplikasi1-M11_ak~M11_ak_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "akid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "akid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_ak_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("akid"), 0), sptField,
                     FxDB(drutama("akcabang"), ""), sptField,
                     FxDB(drutama("aklokasi"), ""), sptField,
                     FxDB(drutama("akgudang"), ""), sptField,
                     FxDB(drutama("aksumber"), ""), sptField,
                     FxDB(drutama("akautonotransaksi"), 0), sptField,
                     FxDB(drutama("aknotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aktgl"), ""), formatTgl), sptField,
                     FxDB(drutama("akkodepa"), 0), sptField,
                     FxDB(drutama("akcustomer"), 0), sptField,
                     FxDB(drutama("akcustomerkontak"), ""), sptField,
                     FxDB(drutama("akuraian"), ""), sptField,
                     FxDB(drutama("akcatatan"), ""), sptField,
                     FxDB(drutama("aknoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("aktglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("aktotaltransaksi"), 0), sptField,
                     FxDB(drutama("akidkj"), 0), sptField,
                     FxDB(drutama("akstatusrealisasi"), 0), sptField,
                     FxDB(drutama("akstatus"), 0), sptField,
                     FxDB(drutama("akstatussebelumnya"), 0), sptField,
                     FxDB(drutama("akjmlrevisi"), 0), sptField,
                     FxDB(drutama("akcetakanke"), 0), sptField,
                     FxDB(drutama("akinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("akinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("akmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("akmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("akisclose"), 0), sptField,
                     FxDB(drutama("akcustomtext1"), ""), sptField,
                     FxDB(drutama("akcustomtext2"), ""), sptField,
                     FxDB(drutama("akcustomtext3"), ""), sptField,
                     FxDB(drutama("akcustomtext4"), ""), sptField,
                     FxDB(drutama("akcustomtext5"), ""), sptField,
                     FxDB(drutama("akcustomtext6"), ""), sptField,
                     FxDB(drutama("akcustomtext7"), ""), sptField,
                     FxDB(drutama("akcustomtext8"), ""), sptField,
                     FxDB(drutama("akcustomtext9"), ""), sptField,
                     FxDB(drutama("akcustomtext10"), ""), sptField,
                     FxDB(drutama("akcustomtext11"), ""), sptField,
                     FxDB(drutama("akcustomtext12"), ""), sptField,
                     FxDB(drutama("akcustomtext13"), ""), sptField,
                     FxDB(drutama("akcustomtext14"), ""), sptField,
                     FxDB(drutama("akcustomtext15"), ""), sptField,
                     FxDB(drutama("akcustomtext16"), ""), sptField,
                     FxDB(drutama("akcustomtext17"), ""), sptField,
                     FxDB(drutama("akcustomtext18"), ""), sptField,
                     FxDB(drutama("akcustomtext19"), ""), sptField,
                     FxDB(drutama("akcustomtext20"), ""), sptField,
                     FxDB(drutama("akcustomint1"), 0), sptField,
                     FxDB(drutama("akcustomint2"), 0), sptField,
                     FxDB(drutama("akcustomint3"), 0), sptField,
                     FxDB(drutama("akcustomint4"), 0), sptField,
                     FxDB(drutama("akcustomint5"), 0), sptField,
                     FxDB(drutama("akcustomint6"), 0), sptField,
                     FxDB(drutama("akcustomint7"), 0), sptField,
                     FxDB(drutama("akcustomint8"), 0), sptField,
                     FxDB(drutama("akcustomint9"), 0), sptField,
                     FxDB(drutama("akcustomint10"), 0), sptField,
                     FxDB(drutama("akcustomint11"), 0), sptField,
                     FxDB(drutama("akcustomint12"), 0), sptField,
                     FxDB(drutama("akcustomint13"), 0), sptField,
                     FxDB(drutama("akcustomint14"), 0), sptField,
                     FxDB(drutama("akcustomint15"), 0), sptField,
                     FxDB(drutama("akcustomint16"), 0), sptField,
                     FxDB(drutama("akcustomint17"), 0), sptField,
                     FxDB(drutama("akcustomint18"), 0), sptField,
                     FxDB(drutama("akcustomint19"), 0), sptField,
                     FxDB(drutama("akcustomint20"), 0), sptField,
                     FxDB(drutama("akcustomdbl1"), 0), sptField,
                     FxDB(drutama("akcustomdbl2"), 0), sptField,
                     FxDB(drutama("akcustomdbl3"), 0), sptField,
                     FxDB(drutama("akcustomdbl4"), 0), sptField,
                     FxDB(drutama("akcustomdbl5"), 0), sptField,
                     FxDB(drutama("akcustomdbl6"), 0), sptField,
                     FxDB(drutama("akcustomdbl7"), 0), sptField,
                     FxDB(drutama("akcustomdbl8"), 0), sptField,
                     FxDB(drutama("akcustomdbl9"), 0), sptField,
                     FxDB(drutama("akcustomdbl10"), 0), sptField,
                     FxDB(drutama("akcustomdbl11"), 0), sptField,
                     FxDB(drutama("akcustomdbl12"), 0), sptField,
                     FxDB(drutama("akcustomdbl13"), 0), sptField,
                     FxDB(drutama("akcustomdbl14"), 0), sptField,
                     FxDB(drutama("akcustomdbl15"), 0), sptField,
                     FxDB(drutama("akcustomdbl16"), 0), sptField,
                     FxDB(drutama("akcustomdbl17"), 0), sptField,
                     FxDB(drutama("akcustomdbl18"), 0), sptField,
                     FxDB(drutama("akcustomdbl19"), 0), sptField,
                     FxDB(drutama("akcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("akcustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("akcabangnama"), ""), sptField,
                     FxDB(drutama("aklokasinama"), ""), sptField,
                     FxDB(drutama("akgudangnama"), ""), sptField,
                     FxDB(drutama("akcustomerkode"), ""), sptField,
                     FxDB(drutama("akcustomernama"), ""), sptField,
                     FxDB(drutama("aknotransaksikj"), ""), sptField,
                     FxDB(drutama("akstatusnama"), ""), sptField,
                     FxDB(drutama("akstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("akinputusernama"), ""), sptField,
                     FxDB(drutama("akmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("akmatauang"), ""), sptField,
                     FxDB(drutama("akkurs"), 0), sptField,
                     FxDB(drutama("akposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("aktglposting"), ""), formatTgl), sptField,
                     FxDB(drutama("aknama"), ""), sptField,
                     FxDB(drutama("aktingkatjual"), 0), sptField,
                     FxDB(drutama("akperawatan"), ""), sptField,
                     FxDB(drutama("akkategoripasien"), ""), sptField,
                     FxDB(drutama("akkamar"), ""), sptField,
                     FxDB(drutama("akkategoripasiennama"), ""), sptField,
                     FxDB(drutama("akkamarnama"), ""), sptField,
                     FxDB(drutama("akawalankatpasien"), ""), sptField,
                     FxDB(drutama("akpenjualanlangsung"), 0), sptField,
                     FxDB(drutama("aknorm"), ""), sptField,
                     FxDB(drutama("akdokter"), ""), sptField,
                     FxDB(drutama("akdokternama"), ""), sptField,
                     FxDB(drutama("akpetugas"), 0), sptField,
                     FxDB(drutama("akpetugaskode"), ""), sptField,
                     FxDB(drutama("aktotalobat"), 0), sptField,
                     FxDB(drutama("akresep"), 0), sptField,
                     FxDB(drutama("akracik"), 0), sptField,
                     FxDB(drutama("akembalase"), 0), sptField,
                     FxDB(drutama("akpetugasnama"), ""), sptField,
      FxDB(drutama("akketerangan"), 0), sptField)
            'akperawatan, akkategoripasien, akkamar, akkategoripasiennama, akkamarnama, akawalankatpasien
            ' FxDB(drutama("aktingkatjual"), 1))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idakdetail"), 0), sptField,
                     FxDB(dr("idak"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akid, akcabang, aklokasi, akgudang, aksumber, akautonotransaksi, aknotransaksi, aktgl, akkodepa, akcustomer, akcustomerkontak, akuraian, akcatatan, aknoref, aktglnoref, aktotaltransaksi, akidkj, akstatusrealisasi, akstatus, akstatussebelumnya, akjmlrevisi, akcetakanke, akinputuser, akinputtgl, akmodifikasiuser, akmodifikasitgl, akisclose, akcustomtext1, akcustomtext2, akcustomtext3, akcustomtext4, akcustomtext5, akcustomtext6, akcustomtext7, akcustomtext8, akcustomtext9, akcustomtext10, akcustomtext11, akcustomtext12, akcustomtext13, akcustomtext14, akcustomtext15, akcustomtext16, akcustomtext17, akcustomtext18, akcustomtext19, akcustomtext20, akcustomint1, akcustomint2, akcustomint3, akcustomint4, akcustomint5, akcustomint6, akcustomint7, akcustomint8, akcustomint9, akcustomint10, akcustomint11, akcustomint12, akcustomint13, akcustomint14, akcustomint15, akcustomint16, akcustomint17, akcustomint18, akcustomint19, akcustomint20, akcustomdbl1, akcustomdbl2, akcustomdbl3, akcustomdbl4, akcustomdbl5, akcustomdbl6, akcustomdbl7, akcustomdbl8, akcustomdbl9, akcustomdbl10, akcustomdbl11, akcustomdbl12, akcustomdbl13, akcustomdbl14, akcustomdbl15, akcustomdbl16, akcustomdbl17, akcustomdbl18, akcustomdbl19, akcustomdbl20, akcustomdate1, akcustomdate2, akcustomdate3, akcustomdate4, akcustomdate5, akcustomdate6, akcustomdate7, akcustomdate8, akcustomdate9, akcustomdate10, akcustomdate11, akcustomdate12, akcustomdate13, akcustomdate14, akcustomdate15, akcustomdate16, akcustomdate17, akcustomdate18, akcustomdate19, akcustomdate20, akcabangnama, aklokasinama, akgudangnama,  akcustomerkode, akcustomernama, aknotransaksikj, akstatusnama, akstatussebelumnyanama, akinputusernama, akmodifikasiusernama, akmatauang, akkurs, akposting, aktglposting, aknama, aktingkatjual, akperawatan, akkategoripasien, akkamar, akkategoripasiennama, akkamarnama, akawalankatpasien, akpenjualanlangsung, aknorm, akdokter, akdokternama, akpetugas, akpetugaskode, aktotalobat, akresep, akracik, akembalase, akpetugasnama, akketerangan" & sptSubParam & "idakdetail, idak, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, idhppkhususmasuk, hpp, gudangtransit, gudangtujuan, tipebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_AkSearch(ByVal param As String) As String
        'M11_akSearch --------------------------------------------------------
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
            Filter = Filter.Replace("aknotransaksikj", "kj.kjnotransaksi")
            Filter = Filter.Replace("aknamapasien", "p1.pnama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        Dim aktotalsum As Double = 0
        sql = query.PanggilQuery("m11_ak_v")

        dt = AmbilData("aplikasi1-M11_ak_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        aktotalsum = AsDataTableDSum(dt, "aktotaltransaksi")
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim aktglbayar As String = ""
            For Each dr As DataRow In dt.Rows
                aktglbayar = FxDB(dr("aktglbayar"), "")
                If Len(aktglbayar) > 0 Then aktglbayar = AsFormatTanggal(FxDB(dr("aktglbayar"), ""), formatTgl) Else aktglbayar = aktglbayar
                search = String.Concat(search,
                     FxDB(dr("akid"), 0), sptField,
                     FxDB(dr("akcabang"), ""), sptField,
                     FxDB(dr("aklokasi"), ""), sptField,
                     FxDB(dr("akgudang"), ""), sptField,
                     FxDB(dr("aksumber"), ""), sptField,
                     FxDB(dr("akautonotransaksi"), 0), sptField,
                     FxDB(dr("aknotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aktgl"), ""), formatTgl), sptField,
                     FxDB(dr("akkodepa"), 0), sptField,
                     FxDB(dr("akcustomer"), 0), sptField,
                     FxDB(dr("akcustomerkontak"), ""), sptField,
                     FxDB(dr("akuraian"), ""), sptField,
                     FxDB(dr("akcatatan"), ""), sptField,
                     FxDB(dr("aknoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aktglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("aktotaltransaksi"), 0), sptField,
                     FxDB(dr("akidkj"), 0), sptField,
                     FxDB(dr("akstatusrealisasi"), 0), sptField,
                     FxDB(dr("akstatus"), 0), sptField,
                     FxDB(dr("akstatussebelumnya"), 0), sptField,
                     FxDB(dr("akjmlrevisi"), 0), sptField,
                     FxDB(dr("akcetakanke"), 0), sptField,
                     FxDB(dr("akinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("akinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("akmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("akisclose"), 0), sptField,
                     FxDB(dr("akcabangnama"), ""), sptField,
                     FxDB(dr("aklokasinama"), ""), sptField,
                     FxDB(dr("akgudangnama"), ""), sptField,
                     FxDB(dr("akcustomerkode"), ""), sptField,
                     FxDB(dr("akcustomernama"), ""), sptField,
                     FxDB(dr("aknotransaksikj"), ""), sptField,
                     FxDB(dr("akstatusnama"), ""), sptField,
                     FxDB(dr("akstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("akinputusernama"), ""), sptField,
                     FxDB(dr("akmodifikasiusernama"), ""), sptField,
                     FixDouble(aktotalsum), sptField,
                     aktglbayar, sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akid, akcabang, aklokasi, akgudang, aksumber, akautonotransaksi, aknotransaksi, aktgl, akkodepa, akcustomer, akcustomerkontak, akuraian, akcatatan, aknoref, aktglnoref, aktotaltransaksi, akidkj, akstatusrealisasi, akstatus, akstatussebelumnya, akjmlrevisi, akcetakanke, akinputuser, akinputtgl, akmodifikasiuser, akmodifikasitgl, akisclose, akcabangnama, aklokasinama, akgudangnama, akcustomerkode, akcustomernama, aknotransaksikj, akstatusnama, akstatussebelumnyanama, akinputusernama, akmodifikasiusernama, aktotalsum, aktglbayar"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_AkTerkait(ByVal param As String) As String
        'M11_AkTerkait --------------------------------------------------------
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
        sql = query.PanggilQuery("m11_ak_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_ak_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("akid"), 0), sptField,
                     FxDB(dr("aknotransaksi"), ""), sptField,
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
            result(2) = "Related AK data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("akid, aknotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_Ak_Detail_VSearch(ByVal param As String) As String
        'M11_ak_Detail_VSearch --------------------------------------------------------
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
        sol = query.PanggilQuery("m11_ak_detail_v")

        dt = AmbilData("aplikasi1-M11_ak_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idakdetail"), 0), sptField,
                     FxDB(dr("idak"), 0), sptField,
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
                     FxDB(dr("aknotransaksi"), ""), sptField,
                     FxDB(dr("akuraian"), ""), sptField,
                     FxDB(dr("akcatatan"), ""), sptField,
                     FxDB(dr("aknoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("aktgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("aktglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("akcustomerkontak"), ""), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("akcustomer"), ""), sptField,
                     FxDB(dr("akcustomerkode"), ""), sptField,
                     FxDB(dr("akcustomernama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idakdetail, idak, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3,customtext4, customtext5, customtext6, customtext7, customtext8,customtext9, customtext10, customtext11, customtext12, customtext13,customtext14, customtext15, customtext16, customtext17, customtext18,customtext19, customtext20, customdbl1, customdbl2, customdbl3,customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,customdbl19, customdbl20, customdate1, customdate2, customdate3,customdate4, customdate5, customdate6, customdate7, customdate8,customdate9, customdate10, customdate11, customdate12, customdate13,customdate14, customdate15, customdate16, customdate17, customdate18,customdate19, customdate20, aknotransaksi, akuraian, akcatatan, aknoref, aktgl, aktglnoref, akcustomerkontak, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,akcustomer, akcustomerkode, akcustomernama, kodedokter"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_AkCekNoRef(ByVal param As String) As String

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
            result(2) = "aknoresep can't be empty." : GoTo selesai
            'Else
            'SET IDTRANSAKSI
            '   idtransaksi = idtransaksi(0)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        If (idtransaksi(1) = "RI") Then
            dt = AsDataTableAmbilDariDB("SELECT COUNT(aknoref) FROM m_11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND akkategoripasien = '" & idtransaksi(2) & "' AND aknoref='" & idtransaksi(0) & "' AND YEAR(aktgl) = '" & idtransaksi(3) & "'")
            'result(2) = "nanananaa" : GoTo selesai
        Else
            'result(2) = "nanananaa" : GoTo selesai
            dt = AsDataTableAmbilDariDB("SELECT COUNT(aknoref) FROM m_11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND aknoref='" & idtransaksi(0) & "' AND YEAR(aktgl) = '" & idtransaksi(3) & "'")
        End If
        'dt = AsDataTableAmbilDariDB("SELECT COUNT(aknoref) FROM m_11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND akkategoripasien = '" & idtransaksi(2) & "' AND aknoref='" & idtransaksi(0) & "' AND YEAR(aktgl) = '" & idtransaksi(3) & "'")
        'result(2) = "SELECT COUNT(aknoref) FROM m11_ak WHERE akperawatan = '" & idtransaksi(1) & "' AND akkategoripasien = '" & idtransaksi(2) & "' AND aknoref='" & idtransaksi(0) & "'" : GoTo selesai
        exist = dt.Rows(0)(0)
        'result(2) = dt.Rows(0)(0) : GoTo selesai
        If (exist > 0) Then
            result(2) = "No Resep '" & idtransaksi(0) & "' sudah dipakai." : GoTo selesai
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


    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistStok As String, ByVal ftStokAvailable As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = "", noBatch As String = "", noSerial As String = ""
        Dim notransaksi As String = "", sumber As String = "", matauang As String = ""

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

                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namalayanan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in '" & gudang & "' warehouse" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK AVAILABLE PERGUDANG YG TERSEDIA
        If Len(ftStokAvailable) > 0 Then
            'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStokAvailable
            sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' AND i.bassembly <> 1 LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStokAvailable
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