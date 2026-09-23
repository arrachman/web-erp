Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_lb
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_LbSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataRowDetailHasil(), dataDetailHasil() As String

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
        'lbid(0) As Integer, lbcabang(1) As String, lblokasi(2) As String, lbgudang(3) As String, lbsumber(4) As String, 
        'lbautonotransaksi(5) As Integer, lbnotransaksi(6) As String, lbtgl(7) As Date, lbkodepa(8) As Integer, lbcustomer(9) As Integer,
        'lbcustomerkontak(10) As String, lburaian(11) As String, lbcatatan(12) As String, lbnoref(13) As String, lbtglnoref(14) As Date, 
        'lbtotaltransaksi(15) As Double, lbidkj(16) As Integer, lbstatusrealisasi(17) As Interger, lbstatus(18) As Integer, lbstatussebelumnya(19) As Integer, 
        'lbjmlrevisi(20) As Integer, lbcetakanke(21) As Integer, lbinputuser(22) As Integer, lbinputtgl(23) As DateTime, lbmodifikasiuser(24) As Integer, 
        'lbmodifikasitgl(25) As DateTime, lbisclose(26) As Integer, lbcustomtext1(27) As String, lbcustomtext2(28) As String, lbcustomtext3(29) As String, 
        'lbcustomtext4(30) As String, lbcustomtext5(31) As String, lbcustomtext6(32) As String, lbcustomtext7(33) As String, lbcustomtext8(34) As String, 
        'lbcustomtext9(35) As String, lbcustomtext10(36) As String, lbcustomtext11(37) As String, lbcustomtext12(38) As String, lbcustomtext13(39) As String, 
        'lbcustomtext14(40) As String, lbcustomtext15(41) As String, lbcustomtext16(42) As String, lbcustomtext17(43) As String, lbcustomtext18(44) As String, 
        'lbcustomtext19(45) As String, lbcustomtext20(46) As String, lbcustomint1(47) As Integer, lbcustomint2(48) As Integer, lbcustomint3(49) As Integer, 
        'lbcustomint4(50) As Integer, lbcustomint5(51) As Integer, lbcustomint6(52) As Integer, lbcustomint7(53) As Integer, lbcustomint8(54) As Integer, 
        'lbcustomint9(55) As Integer, lbcustomint10(56) As Integer, lbcustomint11(57) As Integer, lbcustomint12(58) As Integer, lbcustomint13(59) As Integer, 
        'lbcustomint14(60) As Integer, lbcustomint15(61) As Integer, lbcustomint16(62) As Integer, lbcustomint17(63) As Integer, lbcustomint18(64) As Integer, 
        'lbcustomint19(65) As Integer, lbcustomint20(66) As Integer, lbcustomdbl1(67) As Double, lbcustomdbl2(68) As Double, lbcustomdbl3(69) As Double, 
        'lbcustomdbl4(70) As Double, lbcustomdbl5(71) As Double, lbcustomdbl6(72) As Double, lbcustomdbl7(73) As Double, lbcustomdbl8(74) As Double, 
        'lbcustomdbl9(75) As Double, lbcustomdbl10(76) As Double, lbcustomdbl11(77) As Double, lbcustomdbl12(78) As Double, lbcustomdbl13(79) As Double, 
        'lbcustomdbl14(80) As Double, lbcustomdbl15(81) As Double, lbcustomdbl16(82) As Double, lbcustomdbl17(83) As Double, lbcustomdbl18(84) As Double, 
        'lbcustomdbl19(85) As Double, lbcustomdbl20(86) As Double, lbcustomdate1(87) As Date, lbcustomdate2(88) As Date, lbcustomdate3(89) As Date, 
        'lbcustomdate4(90) As Date, lbcustomdate5(91) As Date, lbcustomdate6(92) As Date, lbcustomdate7(93) As Date, lbcustomdate8(94) As Date, 
        'lbcustomdate9(95) As Date, lbcustomdate10(96) As Date, lbcustomdate11(97) As Date, lbcustomdate12(98) As Date, lbcustomdate13(99) As Date, 
        'lbcustomdate14(100) As Date, lbcustomdate15(101) As Date, lbcustomdate16(102) As Date, lbcustomdate17(103) As Date, lbcustomdate18(104) As Date, 
        'lbcustomdate19(105) As Date, lbcustomdate20(106) As Date, lbmatauang(107) As String, lbkurs(108) As Double, lbposting(109) As Integer


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'lbid, lbcabang, lblokasi, lbgudang, lbsumber, 
        'lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, lbcustomer,
        'lbcustomerkontak, lburaian, lbcatatan, lbnoref, lbtglnoref, 
        'lbtotaltransaksi, lbidkj, lbstatusrealisasi, lbstatus, lbstatussebelumnya, 
        'lbjmlrevisi, lbcetakanke, lbinputuser, lbinputtgl, lbmodifikasiuser, 
        'lbmodifikasitgl, lbisclose, lbcustomtext1, lbcustomtext2, lbcustomtext3, 
        'lbcustomtext4, lbcustomtext5, lbcustomtext6, lbcustomtext7, lbcustomtext8, 
        'lbcustomtext9, lbcustomtext10, lbcustomtext11, lbcustomtext12, lbcustomtext13, 
        'lbcustomtext14, lbcustomtext15, lbcustomtext16, lbcustomtext17, lbcustomtext18, 
        'lbcustomtext19, lbcustomtext20, lbcustomint1, lbcustomint2, lbcustomint3, 
        'lbcustomint4, lbcustomint5, lbcustomint6, lbcustomint7, lbcustomint8, 
        'lbcustomint9, lbcustomint10, lbcustomint11, lbcustomint12, lbcustomint13, 
        'lbcustomint14, lbcustomint15, lbcustomint16, lbcustomint17, lbcustomint18, 
        'lbcustomint19, lbcustomint20, lbcustomdbl1, lbcustomdbl2, lbcustomdbl3, 
        'lbcustomdbl4, lbcustomdbl5, lbcustomdbl6, lbcustomdbl7, lbcustomdbl8, 
        'lbcustomdbl9, lbcustomdbl10, lbcustomdbl11, lbcustomdbl12, lbcustomdbl13, 
        'lbcustomdbl14, lbcustomdbl15, lbcustomdbl16, lbcustomdbl17, lbcustomdbl18, 
        'lbcustomdbl19, lbcustomdbl20, lbcustomdate1, lbcustomdate2, lbcustomdate3, 
        'lbcustomdate4, lbcustomdate5, lbcustomdate6, lbcustomdate7, lbcustomdate8, 
        'lbcustomdate9, lbcustomdate10, lbcustomdate11, lbcustomdate12, lbcustomdate13, 
        'lbcustomdate14, lbcustomdate15, lbcustomdate16, lbcustomdate17, lbcustomdate18, 
        'lbcustomdate19, lbcustomdate20, lbmatauang, lbkurs, lbposting

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 120) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'lbid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "lbid required numeric." : GoTo selesai
        End If
        'lbautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "lbautonotransaksi required numeric." : GoTo selesai
        End If
        'lbtgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "lbtgl required date." : GoTo selesai
        End If
        'lbkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "lbkodepa required numeric." : GoTo selesai
        End If
        'lbcustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "lbcustomer required numeric." : GoTo selesai
        End If
        'lbtglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "lbtglnoref required date." : GoTo selesai
        End If
        'lbtotaltransaksi(15) As Double
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "lbtotaltransaksi required numeric." : GoTo selesai
        End If
        'lbidkj(16) As Integer
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "lbidkj required numeric." : GoTo selesai
        End If
        'lbstatusrealisasi(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "lbstatusrealisasi required numeric." : GoTo selesai
        End If
        'lbstatus(18) As Integer
        If (IsNumeric(dataUtama(18)) = False) Then
            result(2) = "lbstatus required numeric." : GoTo selesai
        End If
        'lbstatussebelumnya(19) As Integer
        If (IsNumeric(dataUtama(19)) = False) Then
            result(2) = "lbstatussebelumnya required numeric." : GoTo selesai
        End If
        'lbjmlrevisi(20) As Integer
        If (IsNumeric(dataUtama(20)) = False) Then
            result(2) = "lbjmlrevisi required numeric." : GoTo selesai
        End If
        'lbcetakanke(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "lbcetakanke required numeric." : GoTo selesai
        End If
        'lbinputuser(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "lbinputuser required numeric." : GoTo selesai
        End If
        'lbinputtgl(23) As DateTime
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "lbinputtgl required date." : GoTo selesai
        End If
        'lbmodifikasiuser(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "lbmodifikasiuser required numeric." : GoTo selesai
        End If
        'lbmodifikasitgl(25) As DateTime
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "lbmodifikasitgl required date." : GoTo selesai
        End If
        'lbisclose(26) As Integer
        If (IsNumeric(dataUtama(26)) = False) Then
            result(2) = "lbisclose required numeric." : GoTo selesai
        End If
        'lbcustomint1(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "lbcustomint1 required numeric." : GoTo selesai
        End If
        'lbcustomint2(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "lbcustomint2 required numeric." : GoTo selesai
        End If
        'lbcustomint3(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "lbcustomint3 required numeric." : GoTo selesai
        End If
        'lbcustomint4(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "lbcustomint4 required numeric." : GoTo selesai
        End If
        'lbcustomint5(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "lbcustomint5 required numeric." : GoTo selesai
        End If
        'lbcustomint6(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "lbcustomint6 required numeric." : GoTo selesai
        End If
        'lbcustomint7(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "lbcustomint7 required numeric." : GoTo selesai
        End If
        'lbcustomint8(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "lbcustomint8 required numeric." : GoTo selesai
        End If
        'lbcustomint9(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "lbcustomint9 required numeric." : GoTo selesai
        End If
        'lbcustomint10(56) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "lbcustomint10 required numeric." : GoTo selesai
        End If
        'lbcustomint11(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "lbcustomint11 required numeric." : GoTo selesai
        End If
        'lbcustomint12(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "lbcustomint12 required numeric." : GoTo selesai
        End If
        'lbcustomint13(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "lbcustomint13 required numeric." : GoTo selesai
        End If
        'lbcustomint14(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "lbcustomint14 required numeric." : GoTo selesai
        End If
        'lbcustomint15(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "lbcustomint15 required numeric." : GoTo selesai
        End If
        'lbcustomint16(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "lbcustomint16 required numeric." : GoTo selesai
        End If
        'lbcustomint17(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "lbcustomint17 required numeric." : GoTo selesai
        End If
        'lbcustomint18(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "lbcustomint18 required numeric." : GoTo selesai
        End If
        'lbcustomint19(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "lbcustomint19 required numeric." : GoTo selesai
        End If
        'lbcustomint20(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "lbcustomint20 required numeric." : GoTo selesai
        End If
        'lbcustomdbl1(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "lbcustomdbl1 required numeric." : GoTo selesai
        End If
        'lbcustomdbl2(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "lbcustomdbl2 required numeric." : GoTo selesai
        End If
        'lbcustomdbl3(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "lbcustomdbl3 required numeric." : GoTo selesai
        End If
        'lbcustomdbl4(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "lbcustomdbl4 required numeric." : GoTo selesai
        End If
        'lbcustomdbl5(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "lbcustomdbl5 required numeric." : GoTo selesai
        End If
        'lbcustomdbl6(72) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "lbcustomdbl6 required numeric." : GoTo selesai
        End If
        'lbcustomdbl7(73) As Double
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "lbcustomdbl7 required numeric." : GoTo selesai
        End If
        'lbcustomdbl8(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "lbcustomdbl8 required numeric." : GoTo selesai
        End If
        'lbcustomdbl9(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "lbcustomdbl9 required numeric." : GoTo selesai
        End If
        'lbcustomdbl10(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "lbcustomdbl10 required numeric." : GoTo selesai
        End If
        'lbcustomdbl11(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "lbcustomdbl11 required numeric." : GoTo selesai
        End If
        'lbcustomdbl12(78) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "lbcustomdbl12 required numeric." : GoTo selesai
        End If
        'lbcustomdbl13(79) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "lbcustomdbl13 required numeric." : GoTo selesai
        End If
        'lbcustomdbl14(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "lbcustomdbl14 required numeric." : GoTo selesai
        End If
        'lbcustomdbl15(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "lbcustomdbl15 required numeric." : GoTo selesai
        End If
        'lbcustomdbl16(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "lbcustomdbl16 required numeric." : GoTo selesai
        End If
        'lbcustomdbl17(83) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "lbcustomdbl17 required numeric." : GoTo selesai
        End If
        'lbcustomdbl18(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "lbcustomdbl18 required numeric." : GoTo selesai
        End If
        'lbcustomdbl19(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "lbcustomdbl19 required numeric." : GoTo selesai
        End If
        'lbcustomdbl20(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "lbcustomdbl20 required numeric." : GoTo selesai
        End If
        'lbcustomdate1(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "lbcustomdate1 required date." : GoTo selesai
        End If
        'lbcustomdate2(88) As Date
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "lbcustomdate2 required date." : GoTo selesai
        End If
        'lbcustomdate3(89) As Date
        If (IsDate(dataUtama(89)) = False) Then
            result(2) = "lbcustomdate3 required date." : GoTo selesai
        End If
        'lbcustomdate4(90) As Date
        If (IsDate(dataUtama(90)) = False) Then
            result(2) = "lbcustomdate4 required date." : GoTo selesai
        End If
        'lbcustomdate5(91) As Date
        If (IsDate(dataUtama(91)) = False) Then
            result(2) = "lbcustomdate5 required date." : GoTo selesai
        End If
        'lbcustomdate6(92) As Date
        If (IsDate(dataUtama(92)) = False) Then
            result(2) = "lbcustomdate6 required date." : GoTo selesai
        End If
        'lbcustomdate7(93) As Date
        If (IsDate(dataUtama(93)) = False) Then
            result(2) = "lbcustomdate7 required date." : GoTo selesai
        End If
        'lbcustomdate8(94) As Date
        If (IsDate(dataUtama(94)) = False) Then
            result(2) = "lbcustomdate8 required date." : GoTo selesai
        End If
        'lbcustomdate9(95) As Date
        If (IsDate(dataUtama(95)) = False) Then
            result(2) = "lbcustomdate9 required date." : GoTo selesai
        End If
        'lbcustomdate10(96) As Date
        If (IsDate(dataUtama(96)) = False) Then
            result(2) = "lbcustomdate10 required date." : GoTo selesai
        End If
        'lbcustomdate11(97) As Date
        If (IsDate(dataUtama(97)) = False) Then
            result(2) = "lbcustomdate11 required date." : GoTo selesai
        End If
        'lbcustomdate12(98) As Date
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "lbcustomdate12 required date." : GoTo selesai
        End If
        'lbcustomdate13(99) As Date
        If (IsDate(dataUtama(99)) = False) Then
            result(2) = "lbcustomdate13 required date." : GoTo selesai
        End If
        'lbcustomdate14(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "lbcustomdate14 required date." : GoTo selesai
        End If
        'lbcustomdate15(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "lbcustomdate15 required date." : GoTo selesai
        End If
        'lbcustomdate16(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "lbcustomdate16 required date." : GoTo selesai
        End If
        'lbcustomdate17(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "lbcustomdate17 required date." : GoTo selesai
        End If
        'lbcustomdate18(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "lbcustomdate18 required date." : GoTo selesai
        End If
        'lbcustomdate19(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "lbcustomdate19 required date." : GoTo selesai
        End If
        'lbcustomdate20(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "lbcustomdate20 required date." : GoTo selesai
        End If
        'lbkurs(108) As Double
        If (IsNumeric(dataUtama(108)) = False) Then
            result(2) = "lbkurs required numeric." : GoTo selesai
        End If
        'lbposting(109) As Integer
        If (IsNumeric(dataUtama(109)) = False) Then
            result(2) = "lbposting required numeric." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'lbcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "lbcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "lbcabang should not be more than 25 character." : GoTo selesai
        End If

        'lblokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "lblokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "lblokasi should not be more than 25 character." : GoTo selesai
        End If

        'lbgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "lbgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "lbgudang should not be more than 25 character." : GoTo selesai
        End If

        'lbsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "lbsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "lbsumber should not be more than 10 character." : GoTo selesai
        End If

        'lbnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "lbnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "lbnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'lbtgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "lbtgl can't be empty" : GoTo selesai
        End If

        'lbtglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "lbtglnoref can't be empty" : GoTo selesai
        End If

        'lbtotaltransaksi(15) As Double
        If Len(dataUtama(15)) = 0 Then
            result(2) = "lbtotaltransaksi can't be empty" : GoTo selesai
        End If

        'lbinputtgl(23) As DateTime
        If Len(dataUtama(23)) = 0 Then
            result(2) = "lbinputtgl can't be empty" : GoTo selesai
        End If

        'lbmodifikasitgl(25) As DateTime
        If Len(dataUtama(25)) = 0 Then
            result(2) = "lbmodifikasitgl can't be empty" : GoTo selesai
        End If

        'lbcustomdbl1(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "lbcustomdbl1 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl2(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "lbcustomdbl2 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl3(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "lbcustomdbl3 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl4(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "lbcustomdbl4 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl5(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "lbcustomdbl5 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl6(72) As Double
        If Len(dataUtama(72)) = 0 Then
            result(2) = "lbcustomdbl6 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl7(73) As Double
        If Len(dataUtama(73)) = 0 Then
            result(2) = "lbcustomdbl7 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl8(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "lbcustomdbl8 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl9(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "lbcustomdbl9 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl10(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "lbcustomdbl10 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl11(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "lbcustomdbl11 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl12(78) As Double
        If Len(dataUtama(78)) = 0 Then
            result(2) = "lbcustomdbl12 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl13(79) As Double
        If Len(dataUtama(79)) = 0 Then
            result(2) = "lbcustomdbl13 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl14(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "lbcustomdbl14 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl15(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "lbcustomdbl15 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl16(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "lbcustomdbl16 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl17(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "lbcustomdbl17 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl18(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "lbcustomdbl18 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl19(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "lbcustomdbl19 can't be empty" : GoTo selesai
        End If

        'lbcustomdbl20(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "lbcustomdbl20 can't be empty" : GoTo selesai
        End If

        'lbcustomdate1(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "lbcustomdate1 can't be empty" : GoTo selesai
        End If

        'lbcustomdate2(88) As Date
        If Len(dataUtama(88)) = 0 Then
            result(2) = "lbcustomdate2 can't be empty" : GoTo selesai
        End If

        'lbcustomdate3(89) As Date
        If Len(dataUtama(89)) = 0 Then
            result(2) = "lbcustomdate3 can't be empty" : GoTo selesai
        End If

        'lbcustomdate4(90) As Date
        If Len(dataUtama(90)) = 0 Then
            result(2) = "lbcustomdate4 can't be empty" : GoTo selesai
        End If

        'lbcustomdate5(91) As Date
        If Len(dataUtama(91)) = 0 Then
            result(2) = "lbcustomdate5 can't be empty" : GoTo selesai
        End If

        'lbcustomdate6(92) As Date
        If Len(dataUtama(92)) = 0 Then
            result(2) = "lbcustomdate6 can't be empty" : GoTo selesai
        End If

        'lbcustomdate7(93) As Date
        If Len(dataUtama(93)) = 0 Then
            result(2) = "lbcustomdate7 can't be empty" : GoTo selesai
        End If

        'lbcustomdate8(94) As Date
        If Len(dataUtama(94)) = 0 Then
            result(2) = "lbcustomdate8 can't be empty" : GoTo selesai
        End If

        'lbcustomdate9(95) As Date
        If Len(dataUtama(95)) = 0 Then
            result(2) = "lbcustomdate9 can't be empty" : GoTo selesai
        End If

        'lbcustomdate10(96) As Date
        If Len(dataUtama(96)) = 0 Then
            result(2) = "lbcustomdate10 can't be empty" : GoTo selesai
        End If

        'lbcustomdate11(97) As Date
        If Len(dataUtama(97)) = 0 Then
            result(2) = "lbcustomdate11 can't be empty" : GoTo selesai
        End If

        'lbcustomdate12(98) As Date
        If Len(dataUtama(98)) = 0 Then
            result(2) = "lbcustomdate12 can't be empty" : GoTo selesai
        End If

        'lbcustomdate13(99) As Date
        If Len(dataUtama(99)) = 0 Then
            result(2) = "lbcustomdate13 can't be empty" : GoTo selesai
        End If

        'lbcustomdate14(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "lbcustomdate14 can't be empty" : GoTo selesai
        End If

        'lbcustomdate15(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "lbcustomdate15 can't be empty" : GoTo selesai
        End If

        'lbcustomdate16(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "lbcustomdate16 can't be empty" : GoTo selesai
        End If

        'lbcustomdate17(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "lbcustomdate17 can't be empty" : GoTo selesai
        End If

        'lbcustomdate18(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "lbcustomdate18 can't be empty" : GoTo selesai
        End If

        'lbcustomdate19(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "lbcustomdate19 can't be empty" : GoTo selesai
        End If

        'lbcustomdate20(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "lbcustomdate20 can't be empty" : GoTo selesai
        End If

        'lbmatauang(107) As String
        If Len(dataUtama(107)) = 0 Then
            result(2) = "lbmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(107)) > 25 Then
            result(2) = "lbmatauang should not be more than 25 character." : GoTo selesai
        End If

        'lbkurs(108) As Double
        If Len(dataUtama(108)) = 0 Then
            result(2) = "lbkurs can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "lbid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lblokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lburaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbjenislab", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbpenjualanlangsung", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "lbumur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "lbketerangan", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "lbid~lbcabang~lblokasi~lbgudang~lbsumber~lbautonotransaksi~lbnotransaksi~lbtgl~lbkodepa~lbcustomer~lbcustomerkontak~lburaian~lbcatatan~lbnoref~lbtglnoref~lbtotaltransaksi~lbidkj~lbstatusrealisasi~lbstatus~lbstatussebelumnya~lbjmlrevisi~lbcetakanke~lbinputuser~lbinputtgl~lbmodifikasiuser~lbmodifikasitgl~lbisclose~lbcustomtext1~lbcustomtext2~lbcustomtext3~lbcustomtext4~lbcustomtext5~lbcustomtext6~lbcustomtext7~lbcustomtext8~lbcustomtext9~lbcustomtext10~lbcustomtext11~lbcustomtext12~lbcustomtext13~lbcustomtext14~lbcustomtext15~lbcustomtext16~lbcustomtext17~lbcustomtext18~lbcustomtext19~lbcustomtext20~lbcustomint1~lbcustomint2~lbcustomint3~lbcustomint4~lbcustomint5~lbcustomint6~lbcustomint7~lbcustomint8~lbcustomint9~lbcustomint10~lbcustomint11~lbcustomint12~lbcustomint13~lbcustomint14~lbcustomint15~lbcustomint16~lbcustomint17~lbcustomint18~lbcustomint19~lbcustomint20~lbcustomdbl1~lbcustomdbl2~lbcustomdbl3~lbcustomdbl4~lbcustomdbl5~lbcustomdbl6~lbcustomdbl7~lbcustomdbl8~lbcustomdbl9~lbcustomdbl10~lbcustomdbl11~lbcustomdbl12~lbcustomdbl13~lbcustomdbl14~lbcustomdbl15~lbcustomdbl16~lbcustomdbl17~lbcustomdbl18~lbcustomdbl19~lbcustomdbl20~lbcustomdate1~lbcustomdate2~lbcustomdate3~lbcustomdate4~lbcustomdate5~lbcustomdate6~lbcustomdate7~lbcustomdate8~lbcustomdate9~lbcustomdate10~lbcustomdate11~lbcustomdate12~lbcustomdate13~lbcustomdate14~lbcustomdate15~lbcustomdate16~lbcustomdate17~lbcustomdate18~lbcustomdate19~lbcustomdate20~lbmatauang~lbkurs~lbposting~lbjenislab~lbperawatan~lbkategoripasien~lbkamar~lbdokter~lbawalankatpasien~lbpenjualanlangsung~lbpetugas~lbumur~lbketerangan", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idlbdetail(0) As Integer, idlb(1) As Integer, jenis(2) As String, idlayanan(3) As Integer, namalayanan(4) As String, 
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
        'idlbdetail, idlb, jenis, idlayanan, namalayanan, 
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
        AsDataTableTambahField(dtdetail, "idlbdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idlb", AsEnumTypeData.AsInt64)
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
            'idlbdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idlbdetail required numeric." : GoTo selesai
            End If
            'idlb(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idlb required numeric." : GoTo selesai
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
            If (IsNumeric(dataRowDetail(93)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
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
            If Len(dataRowDetail(93)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idlbdetail~idlb~jenis~idlayanan~namalayanan~jml~satuan~nilaisatuan~jmltotal~satuandefault~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idkjdetail~jmlrealisasi~statusrealisasi~isclose~iddokter~namadokter~customtext1~customtext2~customtext3~customtext4~customtext5~customtext6~customtext7~customtext8~customtext9~customtext10~customtext11~customtext12~customtext13~customtext14~customtext15~customtext16~customtext17~customtext18~customtext19~customtext20~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdbl6~customdbl7~customdbl8~customdbl9~customdbl10~customdbl11~customdbl12~customdbl13~customdbl14~customdbl15~customdbl16~customdbl17~customdbl18~customdbl19~customdbl20~customdate1~customdate2~customdate3~customdate4~customdate5~customdate6~customdate7~customdate8~customdate9~customdate10~customdate11~customdate12~customdate13~customdate14~customdate15~customdate16~customdate17~customdate18~customdate19~customdate20~matauang~kurs~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekpenjualan", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76) & "~" & dataRowDetail(77) & "~" & dataRowDetail(78) & "~" & dataRowDetail(79) & "~" & dataRowDetail(80) & "~" & dataRowDetail(81) & "~" & dataRowDetail(82) & "~" & dataRowDetail(83) & "~" & dataRowDetail(84) & "~" & dataRowDetail(85) & "~" & dataRowDetail(86) & "~" & dataRowDetail(87) & "~" & dataRowDetail(88) & "~" & dataRowDetail(89) & "~" & dataRowDetail(90) & "~" & dataRowDetail(91) & "~" & dataRowDetail(92) & "~" & dataRowDetail(93) & "~" & dataRowDetail(94) & "~" & dataRowDetail(95) & "~" & dataRowDetail(96) & "~" & dataRowDetail(97)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            idlayanan = dataRowDetail(3) : jmltotal = dataRowDetail(8) : gudang = dataRowDetail(19) : idkjdetail = dataRowDetail(26)
        Next

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        Dim dtdetailhasil As New DataTable
        If dataSplit(2).Length > 0 Then
            'result(2) = "Nanana" : GoTo selesai
            dataDetailHasil = dataSplit(2).Split(sptRow)

            'Buat datatable detail

            AsDataTableTambahField(dtdetailhasil, "idlbhasil", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "idlb", AsEnumTypeData.AsInt64)
            AsDataTableTambahField(dtdetailhasil, "jenis", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "idlayanan", AsEnumTypeData.AsInt64)
            AsDataTableTambahField(dtdetailhasil, "namalayanan", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "hasil", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "standart", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "catatan", AsEnumTypeData.AsString)
            AsDataTableTambahField(dtdetailhasil, "urutan", AsEnumTypeData.AsInt64)
            AsDataTableTambahField(dtdetailhasil, "kelompok", AsEnumTypeData.AsInt64)
            AsDataTableTambahField(dtdetailhasil, "jml", AsEnumTypeData.AsInt64)

            'VALIDASI DAN SET DATA ROW DETAIL ==================================================
            Dim JmlDtDetailHasil As Integer = dataDetailHasil.Length
            For i = 1 To JmlDtDetailHasil
                'SPLIT DATA DETAIL
                dataRowDetailHasil = dataDetailHasil(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetailHasil.Length <> 11) Then
                    result(2) = "Row : " & i & " - Invalid detail hasil transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------
                'idlbdetail(0) As Integer
                If (IsNumeric(dataRowDetailHasil(0)) = False) Then
                    result(2) = "Row : " & i & " - idlbhasil required numeric." : GoTo selesai
                End If
                'idlb(1) As Integer
                If (IsNumeric(dataRowDetailHasil(1)) = False) Then
                    result(2) = "Row : " & i & " - idlb required numeric." : GoTo selesai
                End If
                'idlayanan(2) As Integer
                If (IsNumeric(dataRowDetailHasil(3)) = False) Then
                    result(2) = "Row : " & i & " - idlayanan required numeric." : GoTo selesai
                End If
                'jml(5) As Double
                'If (IsNumeric(dataRowDetailHasil(5)) = False) Then
                '    result(2) = "Row : " & i & " - hasil required numeric." : GoTo selesai
                'End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'jenis(2) As String
                If Len(dataRowDetailHasil(2)) = 0 Then
                    result(2) = "Row : " & i & " - jenis can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetailHasil(2)) > 100 Then
                    result(2) = "Row : " & i & " - jenis should not be more than 100 character." : GoTo selesai
                End If

                'namalayanan(4) As String
                'If Len(dataRowDetailHasil(4)) = 0 Then
                '   result(2) = "Row : " & i & " - namalayanan can't be empty" : GoTo selesai
                ' End If
                'If Len(dataRowDetailHasil(4)) > 100 Then
                '    result(2) = "Row : " & i & " - namalayanan should not be more than 100 character." : GoTo selesai
                'End If

                'jml(5) As Double
                'If Len(dataRowDetailHasil(5)) = 0 Then
                '    result(2) = "Row : " & i & " - hasil can't be empty" : GoTo selesai
                'End If
                'If Len(dataRowDetailHasil(5)) <= 0 Then
                '    result(2) = "Row : " & i & " - hasil can't be less than or equal to zero" : GoTo selesai
                'End If

                'If Len(dataRowDetailHasil(6)) = 0 Then
                '     result(2) = "Row : " & i & " - standart can't be empty" : GoTo selesai
                ' End If
                'If Len(dataRowDetailHasil(6)) <= 0 Then
                '     result(2) = "Row : " & i & " - standart can't be less than or equal to zero" : GoTo selesai
                'End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtdetailhasil, "idlbhasil~idlb~jenis~idlayanan~namalayanan~hasil~standart~catatan~urutan~kelompok~jml", dataRowDetailHasil(0) & "~" & dataRowDetailHasil(1) & "~" & dataRowDetailHasil(2) & "~" & dataRowDetailHasil(3) & "~" & dataRowDetailHasil(4) & "~" & dataRowDetailHasil(5) & "~" & dataRowDetailHasil(6) & "~" & dataRowDetailHasil(7) & "~" & dataRowDetailHasil(8) & "~" & dataRowDetailHasil(9) & "~" & dataRowDetailHasil(10)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next

        End If
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================        


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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 5
                Select Case drutama("lbstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("lbtgl")), AsFormatTanggal(drutama("lbtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                'If drutama("lustatus") = 2 Then
                'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                'Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                'If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding)
                'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'End If
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
                    result(4) = drutama("lbid")
                    notransaksi = drutama("lbnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid), lbnotransaksi FROM M_11_lb WHERE lbid='" & result(4) & "' AND lbstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid) FROM m_11_lb WHERE lbnotransaksi='" & notransaksi & "'", myConn)
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

                        sql = "Update M_11_lb set lbcabang  = '" & FixQuotes(drutama("lbcabang")) & "', lblokasi  = '" & FixQuotes(drutama("lblokasi")) & "', lbgudang  = '" & FixQuotes(drutama("lbgudang")) & "', lbsumber  = '" & FixQuotes(drutama("lbsumber")) & "', lbautonotransaksi  = " & drutama("lbautonotransaksi") & ", lbnotransaksi  = '" & FixQuotes(notransaksi) & "', lbtgl  = '" & FixQuotes(AsFormatTanggal(drutama("lbtgl"))) & "', lbkodepa  = " & drutama("lbkodepa") & ", lbcustomer  = " & drutama("lbcustomer") & ", lbcustomerkontak  = '" & FixQuotes(drutama("lbcustomerkontak")) & "', lburaian  = '" & FixQuotes(drutama("lburaian")) & "', lbcatatan  = '" & FixQuotes(drutama("lbcatatan")) & "', lbnoref  = '" & FixQuotes(drutama("lbnoref")) & "', lbtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("lbtglnoref"))) & "', lbtotaltransaksi  = '" & FixDouble(drutama("lbtotaltransaksi")) & "', lbidkj  = " & drutama("lbidkj") & ", lbstatusrealisasi  = " & drutama("lbstatusrealisasi") & ", lbstatus  = " & drutama("lbstatus") & ", lbstatussebelumnya  = " & drutama("lbstatussebelumnya") & ", lbjmlrevisi  = lbjmlrevisi+1, lbcetakanke  = " & drutama("lbcetakanke") & ", lbmodifikasiuser  = " & drutama("lbmodifikasiuser") & ", lbmodifikasitgl  = NOW(), lbcustomtext1  = '" & FixQuotes(drutama("lbcustomtext1")) & "', lbcustomtext2  = '" & FixQuotes(drutama("lbcustomtext2")) & "', lbcustomtext3  = '" & FixQuotes(drutama("lbcustomtext3")) & "', lbcustomtext4  = '" & FixQuotes(drutama("lbcustomtext4")) & "', lbcustomtext5  = '" & FixQuotes(drutama("lbcustomtext5")) & "', lbcustomtext6  = '" & FixQuotes(drutama("lbcustomtext6")) & "', lbcustomtext7  = '" & FixQuotes(drutama("lbcustomtext7")) & "', lbcustomtext8  = '" & FixQuotes(drutama("lbcustomtext8")) & "', lbcustomtext9  = '" & FixQuotes(drutama("lbcustomtext9")) & "', lbcustomtext10  = '" & FixQuotes(drutama("lbcustomtext10")) & "', lbcustomtext11  = '" & FixQuotes(drutama("lbcustomtext11")) & "', lbcustomtext12  = '" & FixQuotes(drutama("lbcustomtext12")) & "', lbcustomtext13  = '" & FixQuotes(drutama("lbcustomtext13")) & "', lbcustomtext14  = '" & FixQuotes(drutama("lbcustomtext14")) & "', lbcustomtext15  = '" & FixQuotes(drutama("lbcustomtext15")) & "', lbcustomtext16  = '" & FixQuotes(drutama("lbcustomtext16")) & "', lbcustomtext17  = '" & FixQuotes(drutama("lbcustomtext17")) & "', lbcustomtext18  = '" & FixQuotes(drutama("lbcustomtext18")) & "', lbcustomtext19  = '" & FixQuotes(drutama("lbcustomtext19")) & "', lbcustomtext20  = '" & FixQuotes(drutama("lbcustomtext20")) & "', lbcustomint1  = " & drutama("lbcustomint1") & ", lbcustomint2  = " & drutama("lbcustomint2") & ", lbcustomint3  = " & drutama("lbcustomint3") & ", lbcustomint4  = " & drutama("lbcustomint4") & ", lbcustomint5  = " & drutama("lbcustomint5") & ", lbcustomint6  = " & drutama("lbcustomint6") & ", lbcustomint7  = " & drutama("lbcustomint7") & ", lbcustomint8  = " & drutama("lbcustomint8") & ", lbcustomint9  = " & drutama("lbcustomint9") & ", lbcustomint10  = " & drutama("lbcustomint10") & ", lbcustomint11  = " & drutama("lbcustomint11") & ", lbcustomint12  = " & drutama("lbcustomint12") & ", lbcustomint13  = " & drutama("lbcustomint13") & ", lbcustomint14  = " & drutama("lbcustomint14") & ", lbcustomint15  = " & drutama("lbcustomint15") & ", lbcustomint16  = " & drutama("lbcustomint16") & ", lbcustomint17  = " & drutama("lbcustomint17") & ", lbcustomint18  = " & drutama("lbcustomint18") & ", lbcustomint19  = " & drutama("lbcustomint19") & ", lbcustomint20  = " & drutama("lbcustomint20") & ", lbcustomdbl1  = '" & FixDouble(drutama("lbcustomdbl1")) & "', lbcustomdbl2  = '" & FixDouble(drutama("lbcustomdbl2")) & "', lbcustomdbl3  = '" & FixDouble(drutama("lbcustomdbl3")) & "', lbcustomdbl4  = '" & FixDouble(drutama("lbcustomdbl4")) & "', lbcustomdbl5  = '" & FixDouble(drutama("lbcustomdbl5")) & "', lbcustomdbl6  = '" & FixDouble(drutama("lbcustomdbl6")) & "', lbcustomdbl7  = '" & FixDouble(drutama("lbcustomdbl7")) & "', lbcustomdbl8  = '" & FixDouble(drutama("lbcustomdbl8")) & "', lbcustomdbl9  = '" & FixDouble(drutama("lbcustomdbl9")) & "', lbcustomdbl10  = '" & FixDouble(drutama("lbcustomdbl10")) & "', lbcustomdbl11  = '" & FixDouble(drutama("lbcustomdbl11")) & "', lbcustomdbl12  = '" & FixDouble(drutama("lbcustomdbl12")) & "', lbcustomdbl13  = '" & FixDouble(drutama("lbcustomdbl13")) & "', lbcustomdbl14  = '" & FixDouble(drutama("lbcustomdbl14")) & "', lbcustomdbl15  = '" & FixDouble(drutama("lbcustomdbl15")) & "', lbcustomdbl16  = '" & FixDouble(drutama("lbcustomdbl16")) & "', lbcustomdbl17  = '" & FixDouble(drutama("lbcustomdbl17")) & "', lbcustomdbl18  = '" & FixDouble(drutama("lbcustomdbl18")) & "', lbcustomdbl19  = '" & FixDouble(drutama("lbcustomdbl19")) & "', lbcustomdbl20  = '" & FixDouble(drutama("lbcustomdbl20")) & "', lbcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate1"))) & "', lbcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate2"))) & "', lbcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate3"))) & "', lbcustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate4"))) & "', lbcustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate5"))) & "', lbcustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate6"))) & "', lbcustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate7"))) & "', lbcustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate8"))) & "', lbcustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate9"))) & "', lbcustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate10"))) & "', lbcustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate11"))) & "', lbcustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate12"))) & "', lbcustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate13"))) & "', lbcustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate14"))) & "', lbcustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate15"))) & "', lbcustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate16"))) & "', lbcustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate17"))) & "', lbcustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate18"))) & "', lbcustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate19"))) & "', lbcustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate20"))) & "', lbmatauang  = '" & FixQuotes(drutama("lbmatauang")) & "', lbkurs  = '" & FixDouble(drutama("lbkurs")) & "', lbposting  = 0, lbjenislab  = '" & FixQuotes(drutama("lbjenislab")) & "', lbperawatan  = '" & FixQuotes(drutama("lbperawatan")) & "', lbkategoripasien  = '" & FixQuotes(drutama("lbkategoripasien")) & "', lbkamar  = '" & FixQuotes(drutama("lbkamar")) & "', lbdokter  = '" & FixQuotes(drutama("lbdokter")) & "', lbpenjualanlangsung = " & drutama("lbpenjualanlangsung") & ", lbpetugas = " & drutama("lbpetugas") & ", lbumur = '" & FixQuotes(drutama("lbumur")) & "', lbketerangan = " & drutama("lbketerangan") & " where lbid = '" & drutama("lbid") & "'"
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

                    If FixQuotes(drutama("lbnoref")) <> "" Then
                        'Dim dtCekNoRegLab As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid), lbnoref, lbnotransaksi FROM m_11_lb WHERE lbnoref = '" & FixQuotes(drutama("lbnoref")) & "' AND lbperawatan = '" & FixQuotes(drutama("lbperawatan")) & "' AND lbkategoripasien = '" & FixQuotes(drutama("lbkategoripasien")) & "'")
                        Dim dtCekNoRegLab As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid), lbnoref, lbnotransaksi FROM m_11_lb WHERE lbnoref = '" & FixQuotes(drutama("lbnoref")) & "'", myConn)
                        Dim cekNoRegLab As Double = Val(dtCekNoRegLab.Rows(0)(0))
                        If cekNoRegLab > 0 Then
                            result(2) = "No. Reg Lab '" & dtCekNoRegLab.Rows(0)(1) & "' sudah digunakan di nomor transaksi '" & dtCekNoRegLab.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                        End If
                    End If


                    If drutama("lbautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("lbperawatan"), drutama("lbawalankatpasien"), drutama("lbsumber"), drutama("lbtgl"))
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
                        notransaksi = drutama("lbnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(lbid) FROM m_11_lb WHERE lbnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_11_lb (lbcabang, lblokasi, lbgudang, lbsumber, lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, lbcustomer, lbcustomerkontak, lburaian, lbcatatan, lbnoref, lbtglnoref, lbtotaltransaksi, lbidkj, lbstatusrealisasi, lbstatus, lbstatussebelumnya, lbjmlrevisi, lbcetakanke, lbinputuser, lbinputtgl, lbmodifikasiuser, lbmodifikasitgl, lbisclose, lbcustomtext1, lbcustomtext2, lbcustomtext3, lbcustomtext4, lbcustomtext5, lbcustomtext6, lbcustomtext7, lbcustomtext8, lbcustomtext9, lbcustomtext10, lbcustomtext11, lbcustomtext12, lbcustomtext13, lbcustomtext14, lbcustomtext15, lbcustomtext16, lbcustomtext17, lbcustomtext18, lbcustomtext19, lbcustomtext20, lbcustomint1, lbcustomint2, lbcustomint3, lbcustomint4, lbcustomint5, lbcustomint6, lbcustomint7, lbcustomint8, lbcustomint9, lbcustomint10, lbcustomint11, lbcustomint12, lbcustomint13, lbcustomint14, lbcustomint15, lbcustomint16, lbcustomint17, lbcustomint18, lbcustomint19, lbcustomint20, lbcustomdbl1, lbcustomdbl2, lbcustomdbl3, lbcustomdbl4, lbcustomdbl5, lbcustomdbl6, lbcustomdbl7, lbcustomdbl8, lbcustomdbl9, lbcustomdbl10, lbcustomdbl11, lbcustomdbl12, lbcustomdbl13, lbcustomdbl14, lbcustomdbl15, lbcustomdbl16, lbcustomdbl17, lbcustomdbl18, lbcustomdbl19, lbcustomdbl20, lbcustomdate1, lbcustomdate2, lbcustomdate3, lbcustomdate4, lbcustomdate5, lbcustomdate6, lbcustomdate7, lbcustomdate8, lbcustomdate9, lbcustomdate10, lbcustomdate11, lbcustomdate12, lbcustomdate13, lbcustomdate14, lbcustomdate15, lbcustomdate16, lbcustomdate17, lbcustomdate18, lbcustomdate19, lbcustomdate20, lbmatauang, lbkurs, lbjenislab, lbperawatan, lbkategoripasien, lbkamar, lbdokter, lbpenjualanlangsung, lbpetugas, lbumur, lbketerangan) values('" & FixQuotes(drutama("lbcabang")) & "', '" & FixQuotes(drutama("lblokasi")) & "', '" & FixQuotes(drutama("lbgudang")) & "', '" & FixQuotes(drutama("lbsumber")) & "', " & drutama("lbautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbtgl"))) & "', " & drutama("lbkodepa") & ", " & drutama("lbcustomer") & ", '" & FixQuotes(drutama("lbcustomerkontak")) & "', '" & FixQuotes(drutama("lburaian")) & "', '" & FixQuotes(drutama("lbcatatan")) & "', '" & FixQuotes(drutama("lbnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbtglnoref"))) & "', '" & FixDouble(drutama("lbtotaltransaksi")) & "', " & drutama("lbidkj") & ", " & drutama("lbstatusrealisasi") & ", " & drutama("lbstatus") & ", " & drutama("lbstatussebelumnya") & ", " & drutama("lbjmlrevisi") & ", " & drutama("lbcetakanke") & ", " & drutama("lbinputuser") & ", NOW(), " & drutama("lbmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("lbisclose") & ", '" & FixQuotes(drutama("lbcustomtext1")) & "', '" & FixQuotes(drutama("lbcustomtext2")) & "', '" & FixQuotes(drutama("lbcustomtext3")) & "', '" & FixQuotes(drutama("lbcustomtext4")) & "', '" & FixQuotes(drutama("lbcustomtext5")) & "', '" & FixQuotes(drutama("lbcustomtext6")) & "', '" & FixQuotes(drutama("lbcustomtext7")) & "', '" & FixQuotes(drutama("lbcustomtext8")) & "', '" & FixQuotes(drutama("lbcustomtext9")) & "', '" & FixQuotes(drutama("lbcustomtext10")) & "', '" & FixQuotes(drutama("lbcustomtext11")) & "', '" & FixQuotes(drutama("lbcustomtext12")) & "', '" & FixQuotes(drutama("lbcustomtext13")) & "', '" & FixQuotes(drutama("lbcustomtext14")) & "', '" & FixQuotes(drutama("lbcustomtext15")) & "', '" & FixQuotes(drutama("lbcustomtext16")) & "', '" & FixQuotes(drutama("lbcustomtext17")) & "', '" & FixQuotes(drutama("lbcustomtext18")) & "', '" & FixQuotes(drutama("lbcustomtext19")) & "', '" & FixQuotes(drutama("lbcustomtext20")) & "', " & drutama("lbcustomint1") & ", " & drutama("lbcustomint2") & ", " & drutama("lbcustomint3") & ", " & drutama("lbcustomint4") & ", " & drutama("lbcustomint5") & ", " & drutama("lbcustomint6") & ", " & drutama("lbcustomint7") & ", " & drutama("lbcustomint8") & ", " & drutama("lbcustomint9") & ", " & drutama("lbcustomint10") & ", " & drutama("lbcustomint11") & ", " & drutama("lbcustomint12") & ", " & drutama("lbcustomint13") & ", " & drutama("lbcustomint14") & ", " & drutama("lbcustomint15") & ", " & drutama("lbcustomint16") & ", " & drutama("lbcustomint17") & ", " & drutama("lbcustomint18") & ", " & drutama("lbcustomint19") & ", " & drutama("lbcustomint20") & ", '" & FixDouble(drutama("lbcustomdbl1")) & "', '" & FixDouble(drutama("lbcustomdbl2")) & "', '" & FixDouble(drutama("lbcustomdbl3")) & "', '" & FixDouble(drutama("lbcustomdbl4")) & "', '" & FixDouble(drutama("lbcustomdbl5")) & "', '" & FixDouble(drutama("lbcustomdbl6")) & "', '" & FixDouble(drutama("lbcustomdbl7")) & "', '" & FixDouble(drutama("lbcustomdbl8")) & "', '" & FixDouble(drutama("lbcustomdbl9")) & "', '" & FixDouble(drutama("lbcustomdbl10")) & "', '" & FixDouble(drutama("lbcustomdbl11")) & "', '" & FixDouble(drutama("lbcustomdbl12")) & "', '" & FixDouble(drutama("lbcustomdbl13")) & "', '" & FixDouble(drutama("lbcustomdbl14")) & "', '" & FixDouble(drutama("lbcustomdbl15")) & "', '" & FixDouble(drutama("lbcustomdbl16")) & "', '" & FixDouble(drutama("lbcustomdbl17")) & "', '" & FixDouble(drutama("lbcustomdbl18")) & "', '" & FixDouble(drutama("lbcustomdbl19")) & "', '" & FixDouble(drutama("lbcustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("lbcustomdate20"))) & "', '" & FixQuotes(drutama("lbmatauang")) & "', '" & FixDouble(drutama("lbkurs")) & "', '" & FixQuotes(drutama("lbjenislab")) & "', '" & FixQuotes(drutama("lbperawatan")) & "', '" & FixQuotes(drutama("lbkategoripasien")) & "', '" & FixQuotes(drutama("lbkamar")) & "', '" & FixQuotes(drutama("lbdokter")) & "', " & drutama("lbpenjualanlangsung") & ", " & drutama("lbpetugas") & ", '" & FixQuotes(drutama("lbumur")) & "', " & drutama("lbketerangan") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select lbid from M_11_lb where lbnotransaksi='" & notransaksi & "' AND lbinputuser= '" & userid & "' order by lbmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M_11_lb_Detail where idlb = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus detail hasil ketika update
                If (isUpdate) Then
                    sql = "Delete from M_11_lb_Hasil where idlb = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idlbdetail") & ", " & result(4) & ", '" & FixQuotes(dr1("jenis")) & "', " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmltotal")) & "', '" & FixQuotes(dr1("satuandefault")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idkjdetail") & ", '" & FixDouble(dr1("jmlrealisasi")) & "', " & dr1("statusrealisasi") & ", " & dr1("isclose") & ", " & dr1("iddokter") & ", '" & FixQuotes(dr1("namadokter")) & "', '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixQuotes(dr1("customtext6")) & "', '" & FixQuotes(dr1("customtext7")) & "', '" & FixQuotes(dr1("customtext8")) & "', '" & FixQuotes(dr1("customtext9")) & "', '" & FixQuotes(dr1("customtext10")) & "', '" & FixQuotes(dr1("customtext11")) & "', '" & FixQuotes(dr1("customtext12")) & "', '" & FixQuotes(dr1("customtext13")) & "', '" & FixQuotes(dr1("customtext14")) & "', '" & FixQuotes(dr1("customtext15")) & "', '" & FixQuotes(dr1("customtext16")) & "', '" & FixQuotes(dr1("customtext17")) & "', '" & FixQuotes(dr1("customtext18")) & "', '" & FixQuotes(dr1("customtext19")) & "', '" & FixQuotes(dr1("customtext20")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixDouble(dr1("customdbl6")) & "', '" & FixDouble(dr1("customdbl7")) & "', '" & FixDouble(dr1("customdbl8")) & "', '" & FixDouble(dr1("customdbl9")) & "', '" & FixDouble(dr1("customdbl10")) & "', '" & FixDouble(dr1("customdbl11")) & "', '" & FixDouble(dr1("customdbl12")) & "', '" & FixDouble(dr1("customdbl13")) & "', '" & FixDouble(dr1("customdbl14")) & "', '" & FixDouble(dr1("customdbl15")) & "', '" & FixDouble(dr1("customdbl16")) & "', '" & FixDouble(dr1("customdbl17")) & "', '" & FixDouble(dr1("customdbl18")) & "', '" & FixDouble(dr1("customdbl19")) & "', '" & FixDouble(dr1("customdbl20")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate10"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate11"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate12"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate13"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate14"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate15"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate16"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate17"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate18"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate19"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate20"))) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekpenjualan")) & "')")
                    Next
                    sql = "Insert into M_11_lb_Detail(idlbdetail, idlb, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan) values" & strValue2.ToString & ""
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

                'Proses detail hasil
                If (dtdetailhasil.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetailhasil.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idlbhasil") & ", " & result(4) & ", '" & FixQuotes(dr1("jenis")) & "', " & dr1("idlayanan") & ", '" & FixQuotes(dr1("namalayanan")) & "', '" & FixQuotes(dr1("hasil")) & "', '" & FixQuotes(dr1("standart")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("kelompok") & ", " & dr1("jml") & ")")
                    Next
                    sql = "Insert into M_11_lb_Hasil(idlbhasil, idlb, jenis, idlayanan, namalayanan, hasil, standart, catatan, urutan, kelompok, jml) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Else
                    '    result(2) = "Detail hasil Transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                If drutama("lbstatus") = 2 Then
                    'If Len(updNilai) > 0 Then
                    '    'UPDATE OUTSTANDING TRANSAKSI =======================================================
                    '    'UPDATE DETAIL
                    '    sql = "UPDATE m_11_lb_detail SET jmlrealisasi = (CASE idkjdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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

                    If drutama("lbpenjualanlangsung") = 0 Then
                        Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatus, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("lbidkj") & "'", myConn)
                        Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                        If cekKunjungan > 0 Then
                            sql = "Update M_11_Kj set kjstatus = 3 where kjid = '" & drutama("lbidkj") & "'"
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
                    'UPDATE STOK BOOKING ================================================================
                    'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                    'sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    'With objCmd
                    '.Connection = myconn
                    '.Transaction = Trans
                    '.CommandType = CommandType.Text
                    '.CommandText = sql
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
                    'END OF UPDATE STOK BOOKING =========================================================

                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "LB", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("lbstatus") = 2 Then
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

                'INSERT USER LOG ====================================================================
                'Dim sumber As String = "LB", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_LbUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("lbnotransaksikj", "kj.kjnotransaksi")
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
            Dim sumber As String = "Lb", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT lbtgl, lbnotransaksi, lbstatus, lbidkj FROM M_11_lb WHERE lbid='" & idtransaksi & "'", myConn)
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
                nilaiStatus = "lbstatussebelumnya" : jnsaktivitas = 17
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
                'sql = query.m5_so_terkait("lbid = '" & idtransaksi & "'")

                sql = query.PanggilQuery("m11_lb_terkait")
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
                sql &= " SELECT a.lbid as idterkait, a.lbsumber as sumber FROM m_11_lb a JOIN m_11_kj kj ON a.lbidkj = kj.kjid AND a.lbstatus IN(2,3,4,7) AND a.lbid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'LB' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M_11_lb SET lbstatus = " & nilaiStatus & ", lbmodifikasiuser='" & userid & "', lbmodifikasitgl = NOW(), lbjmlrevisi = lbjmlrevisi + 1 WHERE lbid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_LbSearch(PostWsSearch(paramSplit(0), "M11_LbSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_LbDelete(ByVal param As String) As String

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
            Dim sumber As String = "Lb", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT lbid, lbnotransaksi FROM M_11_lb WHERE lbid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT lbcabang, lblokasi, lbsumber, lbautonotransaksi, lbnotransaksi, lbtgl"
            sql &= " FROM M_11_lb"
            sql &= " WHERE lbid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("lbcabang")
                lokasi = dtNomorNext.Rows(0)("lblokasi")
                sumber = dtNomorNext.Rows(0)("lbsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("lbautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("lbnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("lbtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE JURNAL
            sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'LB' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M_11_lb_Detail WHERE idlb = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M_11_lb WHERE lbid = '" & idtransaksi & "'"
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
    Public Function M11_LbGetdataById(ByVal param As String) As String
        'M11_lb_GetdataById Utama --------------------------------------------------------
        'lbid, lbcabang, lblokasi, lbgudang, lbsumber, 
        'lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, lbcustomer, 
        'lbcustomerkontak, lburaian, lbcatatan, lbnoref, lbtglnoref, 
        'lbtotaltransaksi, lbidkj, lbstatusrealisasi, lbstatus, lbstatussebelumnya, 
        'lbjmlrevisi, lbcetakanke, lbinputuser, lbinputtgl, lbmodifikasiuser, 
        'lbmodifikasitgl, lbisclose, lbcustomtext1, lbcustomtext2, lbcustomtext3, 
        'lbcustomtext4, lbcustomtext5, lbcustomtext6, lbcustomtext7, lbcustomtext8,
        'lbcustomtext9, lbcustomtext10, lbcustomtext11, lbcustomtext12, lbcustomtext13,
        'lbcustomtext14, lbcustomtext15, lbcustomtext16, lbcustomtext17, lbcustomtext18,
        'lbcustomtext19, lbcustomtext20, lbcustomint1, lbcustomint2, lbcustomint3,
        'lbcustomint4, lbcustomint5, lbcustomint6, lbcustomint7, lbcustomint8,
        'lbcustomint9, lbcustomint10, lbcustomint11, lbcustomint12, lbcustomint13,
        'lbcustomint14, lbcustomint15, lbcustomint16, lbcustomint17, lbcustomint18,
        'lbcustomint19, lbcustomint20, lbcustomdbl1, lbcustomdbl2, lbcustomdbl3, 
        'lbcustomdbl4, lbcustomdbl5, lbcustomdbl6, lbcustomdbl7, lbcustomdbl8,
        'lbcustomdbl9, lbcustomdbl10, lbcustomdbl11, lbcustomdbl12, lbcustomdbl13,
        'lbcustomdbl14, lbcustomdbl15, lbcustomdbl16, lbcustomdbl17, lbcustomdbl18,
        'lbcustomdbl19, lbcustomdbl20, lbcustomdate1, lbcustomdate2, lbcustomdate3, 
        'lbcustomdate4, lbcustomdate5, lbcustomdate6, lbcustomdate7, lbcustomdate8,
        'lbcustomdate9, lbcustomdate10, lbcustomdate11, lbcustomdate12, lbcustomdate13,
        'lbcustomdate14, lbcustomdate15, lbcustomdate16, lbcustomdate17, lbcustomdate18,
        'lbcustomdate19, lbcustomdate20, lbcabangnama, lblokasinama, lbgudangnama, 
        'lbcustomerkode, lbcustomernama, lbnotransaksikj, lbstatusnama, lbstatussebelumnyanama, 
        'lbinputusernama, lbmodifikasiusernama, lbmatauang, lbkurs, lbposting
        'lbtglposting

        'M11_lb_GetdataById Detail --------------------------------------------------------
        'idlbdetail, idlb, jenis, idlayanan, namalayanan, 
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

        Dim utama As String = "", detail As String = "", hasil As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M11_lb~M11_lb_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "lbid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "lbid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lb_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("lbid"), 0), sptField,
                     FxDB(drutama("lbcabang"), ""), sptField,
                     FxDB(drutama("lblokasi"), ""), sptField,
                     FxDB(drutama("lbgudang"), ""), sptField,
                     FxDB(drutama("lbsumber"), ""), sptField,
                     FxDB(drutama("lbautonotransaksi"), 0), sptField,
                     FxDB(drutama("lbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lbtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("lbkodepa"), 0), sptField,
                     FxDB(drutama("lbcustomer"), 0), sptField,
                     FxDB(drutama("lbcustomerkontak"), ""), sptField,
                     FxDB(drutama("lburaian"), ""), sptField,
                     FxDB(drutama("lbcatatan"), ""), sptField,
                     FxDB(drutama("lbnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("lbtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("lbtotaltransaksi"), 0), sptField,
                     FxDB(drutama("lbidkj"), 0), sptField,
                     FxDB(drutama("lbstatusrealisasi"), 0), sptField,
                     FxDB(drutama("lbstatus"), 0), sptField,
                     FxDB(drutama("lbstatussebelumnya"), 0), sptField,
                     FxDB(drutama("lbjmlrevisi"), 0), sptField,
                     FxDB(drutama("lbcetakanke"), 0), sptField,
                     FxDB(drutama("lbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("lbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("lbisclose"), 0), sptField,
                     FxDB(drutama("lbcustomtext1"), ""), sptField,
                     FxDB(drutama("lbcustomtext2"), ""), sptField,
                     FxDB(drutama("lbcustomtext3"), ""), sptField,
                     FxDB(drutama("lbcustomtext4"), ""), sptField,
                     FxDB(drutama("lbcustomtext5"), ""), sptField,
                     FxDB(drutama("lbcustomtext6"), ""), sptField,
                     FxDB(drutama("lbcustomtext7"), ""), sptField,
                     FxDB(drutama("lbcustomtext8"), ""), sptField,
                     FxDB(drutama("lbcustomtext9"), ""), sptField,
                     FxDB(drutama("lbcustomtext10"), ""), sptField,
                     FxDB(drutama("lbcustomtext11"), ""), sptField,
                     FxDB(drutama("lbcustomtext12"), ""), sptField,
                     FxDB(drutama("lbcustomtext13"), ""), sptField,
                     FxDB(drutama("lbcustomtext14"), ""), sptField,
                     FxDB(drutama("lbcustomtext15"), ""), sptField,
                     FxDB(drutama("lbcustomtext16"), ""), sptField,
                     FxDB(drutama("lbcustomtext17"), ""), sptField,
                     FxDB(drutama("lbcustomtext18"), ""), sptField,
                     FxDB(drutama("lbcustomtext19"), ""), sptField,
                     FxDB(drutama("lbcustomtext20"), ""), sptField,
                     FxDB(drutama("lbcustomint1"), 0), sptField,
                     FxDB(drutama("lbcustomint2"), 0), sptField,
                     FxDB(drutama("lbcustomint3"), 0), sptField,
                     FxDB(drutama("lbcustomint4"), 0), sptField,
                     FxDB(drutama("lbcustomint5"), 0), sptField,
                     FxDB(drutama("lbcustomint6"), 0), sptField,
                     FxDB(drutama("lbcustomint7"), 0), sptField,
                     FxDB(drutama("lbcustomint8"), 0), sptField,
                     FxDB(drutama("lbcustomint9"), 0), sptField,
                     FxDB(drutama("lbcustomint10"), 0), sptField,
                     FxDB(drutama("lbcustomint11"), 0), sptField,
                     FxDB(drutama("lbcustomint12"), 0), sptField,
                     FxDB(drutama("lbcustomint13"), 0), sptField,
                     FxDB(drutama("lbcustomint14"), 0), sptField,
                     FxDB(drutama("lbcustomint15"), 0), sptField,
                     FxDB(drutama("lbcustomint16"), 0), sptField,
                     FxDB(drutama("lbcustomint17"), 0), sptField,
                     FxDB(drutama("lbcustomint18"), 0), sptField,
                     FxDB(drutama("lbcustomint19"), 0), sptField,
                     FxDB(drutama("lbcustomint20"), 0), sptField,
                     FxDB(drutama("lbcustomdbl1"), 0), sptField,
                     FxDB(drutama("lbcustomdbl2"), 0), sptField,
                     FxDB(drutama("lbcustomdbl3"), 0), sptField,
                     FxDB(drutama("lbcustomdbl4"), 0), sptField,
                     FxDB(drutama("lbcustomdbl5"), 0), sptField,
                     FxDB(drutama("lbcustomdbl6"), 0), sptField,
                     FxDB(drutama("lbcustomdbl7"), 0), sptField,
                     FxDB(drutama("lbcustomdbl8"), 0), sptField,
                     FxDB(drutama("lbcustomdbl9"), 0), sptField,
                     FxDB(drutama("lbcustomdbl10"), 0), sptField,
                     FxDB(drutama("lbcustomdbl11"), 0), sptField,
                     FxDB(drutama("lbcustomdbl12"), 0), sptField,
                     FxDB(drutama("lbcustomdbl13"), 0), sptField,
                     FxDB(drutama("lbcustomdbl14"), 0), sptField,
                     FxDB(drutama("lbcustomdbl15"), 0), sptField,
                     FxDB(drutama("lbcustomdbl16"), 0), sptField,
                     FxDB(drutama("lbcustomdbl17"), 0), sptField,
                     FxDB(drutama("lbcustomdbl18"), 0), sptField,
                     FxDB(drutama("lbcustomdbl19"), 0), sptField,
                     FxDB(drutama("lbcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("lbcustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("lbcabangnama"), ""), sptField,
                     FxDB(drutama("lblokasinama"), ""), sptField,
                     FxDB(drutama("lbgudangnama"), ""), sptField,
                     FxDB(drutama("lbcustomerkode"), ""), sptField,
                     FxDB(drutama("lbcustomernama"), ""), sptField,
                     FxDB(drutama("lbnotransaksikj"), ""), sptField,
                     FxDB(drutama("lbstatusnama"), ""), sptField,
                     FxDB(drutama("lbstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("lbinputusernama"), ""), sptField,
                     FxDB(drutama("lbmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("lbmatauang"), ""), sptField,
                     FxDB(drutama("lbkurs"), 0), sptField,
                     FxDB(drutama("lbposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("lbtglposting"), ""), formatTgl), sptField,
                     FxDB(drutama("lbjenislab"), ""), sptField,
                     FxDB(drutama("lbperawatan"), ""), sptField,
                     FxDB(drutama("lbkategoripasien"), ""), sptField,
                     FxDB(drutama("lbkamar"), ""), sptField,
                     FxDB(drutama("lbdokter"), ""), sptField,
                     FxDB(drutama("lbkategoripasiennama"), ""), sptField,
                     FxDB(drutama("lbkamarnama"), ""), sptField,
                     FxDB(drutama("lbdokternama"), ""), sptField,
                     FxDB(drutama("lbawalankatpasien"), ""), sptField,
                     FxDB(drutama("lbtingkatjual"), 0), sptField,
                     FxDB(drutama("lbpenjualanlangsung"), 0), sptField,
                     FxDB(drutama("lbpetugas"), 0), sptField,
                     FxDB(drutama("lbpetugaskode"), ""), sptField,
                     FxDB(drutama("lbpetugasnama"), ""), sptField,
                     FxDB(drutama("lbumur"), ""), sptField,
      FxDB(drutama("lbketerangan"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idlbdetail"), 0), sptField,
                     FxDB(dr("idlb"), 0), sptField,
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
                     FxDB(dr("rekpenjualan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA HASIL
            'PANGGIL QUERY
            Dim querygiro As New m0_query
            sql = querygiro.PanggilQuery("m11_lb_gethasil")

            Dim dtgiro As New DataTable
            dtgiro = AmbilData("aplikasi1-M11_Lb_Hasil", "lbh.idlb = '" & idtransaksi & "'", , True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtgiro.Rows
                hasil = String.Concat(hasil,
                     FxDB(dr("idlbhasil"), 0), sptField,
                     FxDB(dr("idlb"), 0), sptField,
                     FxDB(dr("jenis"), ""), sptField,
                     FxDB(dr("idlayanan"), 0), sptField,
                     FxDB(dr("namalayanan"), ""), sptField,
                     FxDB(dr("hasil"), 0), sptField,
                     FxDB(dr("standart"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("lbid"), 0), sptField,
                     FxDB(dr("kode"), ""), sptField,
      FxDB(dr("kelompok"), 0), sptField,
      FxDB(dr("jml"), 0), sptRow)
            Next
            If hasil.Length > 0 Then hasil = hasil.Substring(0, hasil.Length - sptRow.Length) Else hasil = hasil

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, hasil)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("lbid, lbcabang, lblokasi, lbgudang, lbsumber, lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, lbcustomer, lbcustomerkontak, lburaian, lbcatatan, lbnoref, lbtglnoref, lbtotaltransaksi, lbidkj, lbstatusrealisasi, lbstatus, lbstatussebelumnya, lbjmlrevisi, lbcetakanke, lbinputuser, lbinputtgl, lbmodifikasiuser, lbmodifikasitgl, lbisclose, lbcustomtext1, lbcustomtext2, lbcustomtext3, lbcustomtext4, lbcustomtext5, lbcustomtext6, lbcustomtext7, lbcustomtext8, lbcustomtext9, lbcustomtext10, lbcustomtext11, lbcustomtext12, lbcustomtext13, lbcustomtext14, lbcustomtext15, lbcustomtext16, lbcustomtext17, lbcustomtext18, lbcustomtext19, lbcustomtext20, lbcustomint1, lbcustomint2, lbcustomint3, lbcustomint4, lbcustomint5, lbcustomint6, lbcustomint7, lbcustomint8, lbcustomint9, lbcustomint10, lbcustomint11, lbcustomint12, lbcustomint13, lbcustomint14, lbcustomint15, lbcustomint16, lbcustomint17, lbcustomint18, lbcustomint19, lbcustomint20, lbcustomdbl1, lbcustomdbl2, lbcustomdbl3, lbcustomdbl4, lbcustomdbl5, lbcustomdbl6, lbcustomdbl7, lbcustomdbl8, lbcustomdbl9, lbcustomdbl10, lbcustomdbl11, lbcustomdbl12, lbcustomdbl13, lbcustomdbl14, lbcustomdbl15, lbcustomdbl16, lbcustomdbl17, lbcustomdbl18, lbcustomdbl19, lbcustomdbl20, lbcustomdate1, lbcustomdate2, lbcustomdate3, lbcustomdate4, lbcustomdate5, lbcustomdate6, lbcustomdate7, lbcustomdate8, lbcustomdate9, lbcustomdate10, lbcustomdate11, lbcustomdate12, lbcustomdate13, lbcustomdate14, lbcustomdate15, lbcustomdate16, lbcustomdate17, lbcustomdate18, lbcustomdate19, lbcustomdate20, lbcabangnama, lblokasinama, lbgudangnama,  lbcustomerkode, lbcustomernama, lbnotransaksikj, lbstatusnama, lbstatussebelumnyanama, lbinputusernama, lbmodifikasiusernama, lbmatauang, lbkurs, lbposting, lbtglposting, lbjenislab, lbperawatan, lbkategoripasien, lbkamar, lbdokter, lbkategoripasiennama, lbkamarnama, lbdokternama, lbawalankatpasien, lbtingkatjual, lbpenjualanlangsung, lbpetugas, lbpetugaskode, lbpetugasnama, lbumur, lbketerangan" & sptSubParam & "idlbdetail, idlb, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan" & sptSubParam & "idlbhasil,idlb,jenis,idlayanan,namalayanan,hasil,standart,catatan,urutan,lbid,kode,kelompok,jml"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_LbSearch(ByVal param As String) As String
        'M11_lbSearch --------------------------------------------------------
        'lbid, lbcabang, lblokasi, lbgudang, lbasalbarang, lbasalbarangkategori, lbjenispenjualan, 
        'lbjenispenjualankategori, lbcarabayar, lbsumber, lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, 
        'lbcustomer, lbcustomerkontak, lb1alamat1, lb1alamat2, lb1alamat3, lb2alamat1, lb2alamat2, 
        'lb2alamat3, lbbagianpenjualan, lbekspedisi, lbtglkirim, lbtermin, lbtgljatuhtempo, lburaian, 
        'lbcatatan, lbnoref, lbtglnoref, lbtglpenutupan, lbmatauang, lbkurs, lbhargatermasukpajak, 
        'lbtotal, lbdiskonpersen, lbjmldiskon, lbtotalpajak1detail, lbtotalpajak2detail, lbbiayalainpersen, lbbiayalain, 
        'lbtotaltransaksi, lbjmlbayar, lbrekdiskon, lbrekpajak1, lbrekpajak2, lbrekbiayalain, lbrekbayar, 
        'lbidsq, lbstatuspl, lbstatusdo, lbstatusdr, lbstatuspi, lbstatussi, lbstatusrnr, 
        'lbstatussr, lbstatusrealisasi, lbstatus, lbstatussebelumnya, lbjmlrevisi, lbcetakanke, lbinputuser, 
        'lbinputtgl, lbmodifikasiuser, lbmodifikasitgl, lbposting, lbpostingtgl, lbisclose, lbcabangnama, 
        'lblokasinama, lbgudangnama, lbcustomerkode, lbcustomernama, lbbagianpenjualankode, lbbagianpenjualannama, lbekspedisinama, 
        'lbnotransaksikj, lbstatusnama, lbstatussebelumnyanama, lbinputusernama, lbmodifikasiusernama

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
            Filter = Filter.Replace("lbnotransaksikj", "kj.kjnotransaksi")
            Filter = Filter.Replace("lbnorm", "p.pkode")
            Filter = Filter.Replace("lbnama", "p.pnama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_lb_v")

        dt = AmbilData("aplikasi1-M11_lb_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("lbid"), 0), sptField,
                     FxDB(dr("lbcabang"), ""), sptField,
                     FxDB(dr("lblokasi"), ""), sptField,
                     FxDB(dr("lbgudang"), ""), sptField,
                     FxDB(dr("lbsumber"), ""), sptField,
                     FxDB(dr("lbautonotransaksi"), 0), sptField,
                     FxDB(dr("lbnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lbtgl"), ""), formatTgl), sptField,
                     FxDB(dr("lbkodepa"), 0), sptField,
                     FxDB(dr("lbcustomer"), 0), sptField,
                     FxDB(dr("lbcustomerkontak"), ""), sptField,
                     FxDB(dr("lburaian"), ""), sptField,
                     FxDB(dr("lbcatatan"), ""), sptField,
                     FxDB(dr("lbnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lbtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("lbtotaltransaksi"), 0), sptField,
                     FxDB(dr("lbidkj"), 0), sptField,
                     FxDB(dr("lbstatusrealisasi"), 0), sptField,
                     FxDB(dr("lbstatus"), 0), sptField,
                     FxDB(dr("lbstatussebelumnya"), 0), sptField,
                     FxDB(dr("lbjmlrevisi"), 0), sptField,
                     FxDB(dr("lbcetakanke"), 0), sptField,
                     FxDB(dr("lbinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("lbinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lbmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("lbmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("lbisclose"), 0), sptField,
                     FxDB(dr("lbcabangnama"), ""), sptField,
                     FxDB(dr("lblokasinama"), ""), sptField,
                     FxDB(dr("lbgudangnama"), ""), sptField,
                     FxDB(dr("lbcustomerkode"), ""), sptField,
                     FxDB(dr("lbcustomernama"), ""), sptField,
                     FxDB(dr("lbnotransaksikj"), ""), sptField,
                     FxDB(dr("lbstatusnama"), ""), sptField,
                     FxDB(dr("lbstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("lbinputusernama"), ""), sptField,
                     FxDB(dr("lbmodifikasiusernama"), ""), sptField,
                     FxDB(dr("lbnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("lbid, lbcabang, lblokasi, lbgudang, lbsumber, lbautonotransaksi, lbnotransaksi, lbtgl, lbkodepa, lbcustomer, lbcustomerkontak, lburaian, lbcatatan, lbnoref, lbtglnoref, lbtotaltransaksi, lbidkj, lbstatusrealisasi, lbstatus, lbstatussebelumnya, lbjmlrevisi, lbcetakanke, lbinputuser, lbinputtgl, lbmodifikasiuser, lbmodifikasitgl, lbisclose, lbcabangnama, lblokasinama, lbgudangnama, lbcustomerkode, lbcustomernama, lbnotransaksikj, lbstatusnama, lbstatussebelumnyanama, lbinputusernama, lbmodifikasiusernama, lbnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_LbTerkait(ByVal param As String) As String
        'M11_lbTerkait --------------------------------------------------------
        'lbid, lbnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m11_lb_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_lb_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("lbid"), 0), sptField,
                     FxDB(dr("lbnotransaksi"), ""), sptField,
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
            result(2) = "Related LB data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("lbid, lbnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_Lb_Detail_VSearch(ByVal param As String) As String
        'M11_lb_Detail_VSearch --------------------------------------------------------
        'idlbdetail, idlb, jenis, idlayanan, namalayanan, 
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
        'customdate19, customdate20, lbnotransaksi, lburaian, lbcatatan,
        'lbnoref, lbtgl, lbtglnoref, lbcustomerkontak, kodelayanan,
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,
        'lbcustomer, lbcustomerkode, lbcustomernama, kodedokter

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
            Filter = Filter.Replace("idlayanan", "lbd.idlayanan")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sol = query.PanggilQuery("m11_lb_detail_v")

        dt = AmbilData("aplikasi1-M11_lb_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idlbdetail"), 0), sptField,
                     FxDB(dr("idlb"), 0), sptField,
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
                     FxDB(dr("lbnotransaksi"), ""), sptField,
                     FxDB(dr("lburaian"), ""), sptField,
                     FxDB(dr("lbcatatan"), ""), sptField,
                     FxDB(dr("lbnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("lbtgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("lbtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("lbcustomerkontak"), ""), sptField,
                     FxDB(dr("kodelayanan"), ""), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("lbcustomer"), ""), sptField,
                     FxDB(dr("lbcustomerkode"), ""), sptField,
                     FxDB(dr("lbcustomernama"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idlbdetail, idlb, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3,customtext4, customtext5, customtext6, customtext7, customtext8,customtext9, customtext10, customtext11, customtext12, customtext13,customtext14, customtext15, customtext16, customtext17, customtext18,customtext19, customtext20, customdbl1, customdbl2, customdbl3,customdbl4, customdbl5, customdbl6, customdbl7, customdbl8,customdbl9, customdbl10, customdbl11, customdbl12, customdbl13,customdbl14, customdbl15, customdbl16, customdbl17, customdbl18,customdbl19, customdbl20, customdate1, customdate2, customdate3,customdate4, customdate5, customdate6, customdate7, customdate8,customdate9, customdate10, customdate11, customdate12, customdate13,customdate14, customdate15, customdate16, customdate17, customdate18,customdate19, customdate20, lbnotransaksi, lburaian, lbcatatan, lbnoref, lbtgl, lbtglnoref, lbcustomerkontak, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisarealisasi,lbcustomer, lbcustomerkode, lbcustomernama, kodedokter"))

        Return wsResult
    End Function

End Class