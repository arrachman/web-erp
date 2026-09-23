Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_kj
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_KjSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

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
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'kjid(0) As Integer, kjcabang(1) As String, kjlokasi(2) As String, kjsumber(3) As String, kjautonotransaksi(4) As Integer, kjnotransaksi(5) As String, kjtgl(6) As Date, kjkodepa(7) As Integer, kjnopasien(8) As String, 
        'kjnama(9) As String, kjprefix(10) As String, kjtgllahir(11) As Date, kjumur(12) As Integer, kjjeniskelamin(13) As String, kjstatusperkawinan(14) As Integer,
        'kjagama(15) As Integer, kjayah(16) As String, kjibu(17) As String, kjsuamiistri(18) As String, kjnotelepon(19) As String, kjnofax(20) As String,
        'kjnohp(21) As String, kjemail(22) As String, kjalamat(23) As String, kjkota(24) As String, kjprovinsi(25) As String, kjnegara(26) As String, 
        'kjkodepos(27) As String, kjkeluargalain(28) As String, kjnoteleponlain(29) As String, kjcatatan(30) As String,
        'kjtglkeluar(31) As Date, kjtglmeninggal(32) As Date, kjcarakunjungan(33) As Integer, kjdirujukoleh(34) As Integer, kjditanggungoleh(35) As Integer, 
        'kjstatusrealisasi(36) As Interger, kjstatus(37) As Integer, kjstatussebelumnya(38) As Integer, kjjmlrevisi(39) As Integer, kjcetakanke(40) As Integer, 
        'kjinputuser(41) As Integer, kjinputtgl(42) As DateTime, kjmodifikasiuser(43) As Integer, kjmodifikasitgl(44) As DateTime, kjisclose(45) As Integer, 
        'kjcustomtext1(46) As String, kjcustomtext2(47) As String, kjcustomtext3(48) As String, kjcustomtext4(49) As String, kjcustomtext5(50) As String, 
        'kjcustomtext6(51) As String, kjcustomtext7(52) As String, kjcustomtext8(53) As String, kjcustomtext9(54) As String, kjcustomtext10(55) As String,
        'kjcustomtext11(56) As String, kjcustomtext12(57) As String, kjcustomtext13(58) As String, kjcustomtext14(59) As String, kjcustomtext15(60) As String, 
        'kjcustomtext16(61) As String, kjcustomtext17(62) As String, kjcustomtext18(63) As String, kjcustomtext19(64) As String, kjcustomtext20(65) As String, 
        'kjcustomint1(66) As Integer, kjcustomint2(67) As Integer, kjcustomint3(68) As Integer, kjcustomint4(69) As Integer, kjcustomint5(70) As Integer, 
        'kjcustomint6(71) As Integer, kjcustomint7(72) As Integer, kjcustomint8(73) As Integer, kjcustomint9(74) As Integer, kjcustomint10(75) As Integer, 
        'kjcustomint11(76) As Integer, kjcustomint12(77) As Integer, kjcustomint13(78) As Integer, kjcustomint14(79) As Integer, kjcustomint15(80) As Integer, 
        'kjcustomint16(81) As Integer, kjcustomint17(82) As Integer, kjcustomint18(83) As Integer, kjcustomint19(84) As Integer, kjcustomint20(85) As Integer,
        'kjcustomdbl1(86) As Double, kjcustomdbl2(87) As Double, kjcustomdbl3(88) As Double, kjcustomdbl4(89) As Double, kjcustomdbl5(90) As Double, 
        'kjcustomdbl6(91) As Double, kjcustomdbl7(92) As Double, kjcustomdbl8(93) As Double, kjcustomdbl9(94) As Double, kjcustomdbl10(95) As Double, 
        'kjcustomdbl11(96) As Double, kjcustomdbl12(97) As Double, kjcustomdbl13(98) As Double, kjcustomdbl14(99) As Double, kjcustomdbl15(100) As Double, 
        'kjcustomdbl16(101) As Double, kjcustomdbl17(102) As Double, kjcustomdbl18(103) As Double, kjcustomdbl19(104) As Double, kjcustomdbl20(105) As Double, 
        'kjcustomdate1(106) As Date, kjcustomdate2(107) As Date, kjcustomdate3(108) As Date, kjcustomdate4(109) As Date, kjcustomdate5(110) As Date,
        'kjcustomdate6(111) As Date, kjcustomdate7(112) As Date, kjcustomdate8(113) As Date, kjcustomdate9(114) As Date, kjcustomdate10(115) As Date,
        'kjcustomdate11(116) As Date, kjcustomdate12(117) As Date, kjcustomdate13(118) As Date, kjcustomdate14(119) As Date, kjcustomdate15(120) As Date,
        'kjcustomdate16(121) As Date, kjcustomdate17(122) As Date, kjcustomdate18(123) As Date, kjcustomdate19(124) As Date, kjcustomdate20(125) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 141) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kjid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kjid required numeric." : GoTo selesai
        End If
        'kjautonotransaksi(2) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "kjautonotransaksi required numeric." : GoTo selesai
        End If
        'kjtgl(4) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "kjtgl required date." : GoTo selesai
        End If
        'kjkodepa(5) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "kjkodepa required numeric." : GoTo selesai
        End If

        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "kjumur required numeric." : GoTo selesai
        End If
        'statusperkawinan(7) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "kjstatusperkawinan required numeric." : GoTo selesai
        End If
        'agama(8) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "kjagama required numeric." : GoTo selesai
        End If

        'kjstatusrealisasi(12) As Interger
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "kjstatusrealisasi required numeric." : GoTo selesai
        End If
        'kjstatus(13) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "kjstatus required numeric." : GoTo selesai
        End If
        'kjstatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "kjstatussebelumnya required numeric." : GoTo selesai
        End If
        'kjjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "kjjmlrevisi required numeric." : GoTo selesai
        End If
        'kjcetakanke(16) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "kjcetakanke required numeric." : GoTo selesai
        End If
        'kjinputuser(17) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "kjinputuser required numeric." : GoTo selesai
        End If
        'kjinputtgl(18) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "kjinputtgl required date." : GoTo selesai
        End If
        'kjmodifikasiuser(19) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "kjmodifikasiuser required numeric." : GoTo selesai
        End If
        'kjmodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "kjmodifikasitgl required date." : GoTo selesai
        End If
        'kjisclose(21) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "kjisclose required numeric." : GoTo selesai
        End If
        'kjcustomint1(42) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "kjcustomint1 required numeric." : GoTo selesai
        End If
        'kjcustomint2(43) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "kjcustomint2 required numeric." : GoTo selesai
        End If
        'kjcustomint3(44) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "kjcustomint3 required numeric." : GoTo selesai
        End If
        'kjcustomint4(45) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "kjcustomint4 required numeric." : GoTo selesai
        End If
        'kjcustomint5(46) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "kjcustomint5 required numeric." : GoTo selesai
        End If
        'kjcustomint6(47) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "kjcustomint6 required numeric." : GoTo selesai
        End If
        'kjcustomint7(48) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "kjcustomint7 required numeric." : GoTo selesai
        End If
        'kjcustomint8(49) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "kjcustomint8 required numeric." : GoTo selesai
        End If
        'kjcustomint9(50) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "kjcustomint9 required numeric." : GoTo selesai
        End If
        'kjcustomint10(51) As Integer
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "kjcustomint10 required numeric." : GoTo selesai
        End If
        'kjcustomint11(52) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "kjcustomint11 required numeric." : GoTo selesai
        End If
        'kjcustomint12(53) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "kjcustomint12 required numeric." : GoTo selesai
        End If
        'kjcustomint13(54) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "kjcustomint13 required numeric." : GoTo selesai
        End If
        'kjcustomint14(55) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "kjcustomint14 required numeric." : GoTo selesai
        End If
        'kjcustomint15(56) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "kjcustomint15 required numeric." : GoTo selesai
        End If
        'kjcustomint16(57) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "kjcustomint16 required numeric." : GoTo selesai
        End If
        'kjcustomint17(58) As Integer
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "kjcustomint17 required numeric." : GoTo selesai
        End If
        'kjcustomint18(59) As Integer
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "kjcustomint18 required numeric." : GoTo selesai
        End If
        'kjcustomint19(60) As Integer
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "kjcustomint19 required numeric." : GoTo selesai
        End If
        'kjcustomint20(61) As Integer
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "kjcustomint20 required numeric." : GoTo selesai
        End If
        'kjcustomdbl1(62) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "kjcustomdbl1 required numeric." : GoTo selesai
        End If
        'kjcustomdbl2(63) As Double
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "kjcustomdbl2 required numeric." : GoTo selesai
        End If
        'kjcustomdbl3(64) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "kjcustomdbl3 required numeric." : GoTo selesai
        End If
        'kjcustomdbl4(65) As Double
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "kjcustomdbl4 required numeric." : GoTo selesai
        End If
        'kjcustomdbl5(66) As Double
        If (IsNumeric(dataUtama(90)) = False) Then
            result(2) = "kjcustomdbl5 required numeric." : GoTo selesai
        End If
        'kjcustomdbl6(67) As Double
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "kjcustomdbl6 required numeric." : GoTo selesai
        End If
        'kjcustomdbl7(68) As Double
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "kjcustomdbl7 required numeric." : GoTo selesai
        End If
        'kjcustomdbl8(69) As Double
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "kjcustomdbl8 required numeric." : GoTo selesai
        End If
        'kjcustomdbl9(70) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "kjcustomdbl9 required numeric." : GoTo selesai
        End If
        'kjcustomdbl10(71) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "kjcustomdbl10 required numeric." : GoTo selesai
        End If
        'kjcustomdbl11(72) As Double
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kjcustomdbl11 required numeric." : GoTo selesai
        End If
        'kjcustomdbl12(73) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kjcustomdbl12 required numeric." : GoTo selesai
        End If
        'kjcustomdbl13(74) As Double
        If (IsNumeric(dataUtama(98)) = False) Then
            result(2) = "kjcustomdbl13 required numeric." : GoTo selesai
        End If
        'kjcustomdbl14(75) As Double
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "kjcustomdbl14 required numeric." : GoTo selesai
        End If
        'kjcustomdbl15(76) As Double
        If (IsNumeric(dataUtama(100)) = False) Then
            result(2) = "kjcustomdbl15 required numeric." : GoTo selesai
        End If
        'kjcustomdbl16(77) As Double
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "kjcustomdbl16 required numeric." : GoTo selesai
        End If
        'kjcustomdbl17(78) As Double
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "kjcustomdbl17 required numeric." : GoTo selesai
        End If
        'kjcustomdbl18(79) As Double
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "kjcustomdbl18 required numeric." : GoTo selesai
        End If
        'kjcustomdbl19(80) As Double
        If (IsNumeric(dataUtama(104)) = False) Then
            result(2) = "kjcustomdbl19 required numeric." : GoTo selesai
        End If
        'kjcustomdbl20(81) As Double
        If (IsNumeric(dataUtama(105)) = False) Then
            result(2) = "kjcustomdbl20 required numeric." : GoTo selesai
        End If
        'kjcustomdate1(82) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "kjcustomdate1 required date." : GoTo selesai
        End If
        'kjcustomdate2(83) As Date
        If (IsDate(dataUtama(107)) = False) Then
            result(2) = "kjcustomdate2 required date." : GoTo selesai
        End If
        'kjcustomdate3(84) As Date
        If (IsDate(dataUtama(108)) = False) Then
            result(2) = "kjcustomdate3 required date." : GoTo selesai
        End If
        'kjcustomdate4(85) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "kjcustomdate4 required date." : GoTo selesai
        End If
        'kjcustomdate5(86) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "kjcustomdate5 required date." : GoTo selesai
        End If
        'kjcustomdate6(87) As Date
        If (IsDate(dataUtama(111)) = False) Then
            result(2) = "kjcustomdate6 required date." : GoTo selesai
        End If
        'kjcustomdate7(88) As Date
        If (IsDate(dataUtama(112)) = False) Then
            result(2) = "kjcustomdate7 required date." : GoTo selesai
        End If
        'kjcustomdate8(89) As Date
        If (IsDate(dataUtama(113)) = False) Then
            result(2) = "kjcustomdate8 required date." : GoTo selesai
        End If
        'kjcustomdate9(90) As Date
        If (IsDate(dataUtama(114)) = False) Then
            result(2) = "kjcustomdate9 required date." : GoTo selesai
        End If
        'kjcustomdate10(91) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "kjcustomdate10 required date." : GoTo selesai
        End If
        'kjcustomdate11(92) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "kjcustomdate11 required date." : GoTo selesai
        End If
        'kjcustomdate12(93) As Date
        If (IsDate(dataUtama(117)) = False) Then
            result(2) = "kjcustomdate12 required date." : GoTo selesai
        End If
        'kjcustomdate13(94) As Date
        If (IsDate(dataUtama(118)) = False) Then
            result(2) = "kjcustomdate13 required date." : GoTo selesai
        End If
        'kjcustomdate14(95) As Date
        If (IsDate(dataUtama(119)) = False) Then
            result(2) = "kjcustomdate14 required date." : GoTo selesai
        End If
        'kjcustomdate15(96) As Date
        If (IsDate(dataUtama(120)) = False) Then
            result(2) = "kjcustomdate15 required date." : GoTo selesai
        End If
        'kjcustomdate16(97) As Date
        If (IsDate(dataUtama(121)) = False) Then
            result(2) = "kjcustomdate16 required date." : GoTo selesai
        End If
        'kjcustomdate17(98) As Date
        If (IsDate(dataUtama(122)) = False) Then
            result(2) = "kjcustomdate17 required date." : GoTo selesai
        End If
        'kjcustomdate18(99) As Date
        If (IsDate(dataUtama(123)) = False) Then
            result(2) = "kjcustomdate18 required date." : GoTo selesai
        End If
        'kjcustomdate19(100) As Date
        If (IsDate(dataUtama(124)) = False) Then
            result(2) = "kjcustomdate19 required date." : GoTo selesai
        End If
        'kjcustomdate20(101) As Date
        If (IsDate(dataUtama(125)) = False) Then
            result(2) = "kjcustomdate20 required date." : GoTo selesai
        End If
        'If (IsNumeric(dataUtama(127)) = False) Then
        '    result(2) = "kjkategoriharga required numeric." : GoTo selesai
        'End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kjcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 10 Then
            result(2) = "kjcabang should not be more than 10 character." : GoTo selesai
        End If

        If Len(dataUtama(2)) = 0 Then
            result(2) = "kjlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 10 Then
            result(2) = "kjlokasi should not be more than 10 character." : GoTo selesai
        End If

        'kjsumber(1) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kjsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "kjsumber should not be more than 10 character." : GoTo selesai
        End If

        'kjnotransaksi(3) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "kjnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "kjnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kjtgl(4) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "kjtgl can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(9)) = 0 Then
            result(2) = "kjnama can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(9)) > 100 Then
            result(2) = "kjnama should not be more than 100 character." : GoTo selesai
        End If

        'jml(5) As Double
        If Len(dataUtama(11)) = 0 Then
            result(2) = "kjtgllahir can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 10 Then
            result(2) = "kjtgllahir should not be more than 10 character." : GoTo selesai
        End If

        'satuan(6) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "kjumur can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 10 Then
            result(2) = "kjumur should not be more than 10 character." : GoTo selesai
        End If
        If Len(dataUtama(12)) <= 0 Then
            result(2) = "kjumur can't be less than or equal to zero" : GoTo selesai
        End If

        'satuanbarang(9) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "kjjeniskelamin can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 10 Then
            result(2) = "kjjeniskelamin should not be more than 10 character." : GoTo selesai
        End If

        'kjinputtgl(18) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "kjinputtgl can't be empty" : GoTo selesai
        End If

        'kjmodifikasitgl(20) As DateTime
        If Len(dataUtama(44)) = 0 Then
            result(2) = "kjmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kjcustomdbl1(62) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "kjcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl2(63) As Double
        If Len(dataUtama(87)) = 0 Then
            result(2) = "kjcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl3(64) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "kjcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl4(65) As Double
        If Len(dataUtama(89)) = 0 Then
            result(2) = "kjcustomdbl4 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl5(66) As Double
        If Len(dataUtama(90)) = 0 Then
            result(2) = "kjcustomdbl5 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl6(67) As Double
        If Len(dataUtama(91)) = 0 Then
            result(2) = "kjcustomdbl6 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl7(68) As Double
        If Len(dataUtama(92)) = 0 Then
            result(2) = "kjcustomdbl7 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl8(69) As Double
        If Len(dataUtama(93)) = 0 Then
            result(2) = "kjcustomdbl8 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl9(70) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "kjcustomdbl9 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl10(71) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "kjcustomdbl10 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl11(72) As Double
        If Len(dataUtama(96)) = 0 Then
            result(2) = "kjcustomdbl11 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl12(73) As Double
        If Len(dataUtama(97)) = 0 Then
            result(2) = "kjcustomdbl12 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl13(74) As Double
        If Len(dataUtama(98)) = 0 Then
            result(2) = "kjcustomdbl13 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl14(75) As Double
        If Len(dataUtama(99)) = 0 Then
            result(2) = "kjcustomdbl14 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl15(76) As Double
        If Len(dataUtama(100)) = 0 Then
            result(2) = "kjcustomdbl15 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl16(77) As Double
        If Len(dataUtama(101)) = 0 Then
            result(2) = "kjcustomdbl16 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl17(78) As Double
        If Len(dataUtama(102)) = 0 Then
            result(2) = "kjcustomdbl17 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl18(79) As Double
        If Len(dataUtama(103)) = 0 Then
            result(2) = "kjcustomdbl18 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl19(80) As Double
        If Len(dataUtama(104)) = 0 Then
            result(2) = "kjcustomdbl19 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl20(81) As Double
        If Len(dataUtama(105)) = 0 Then
            result(2) = "kjcustomdbl20 can't be empty" : GoTo selesai
        End If

        'kjcustomdate1(82) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "kjcustomdate1 can't be empty" : GoTo selesai
        End If

        'kjcustomdate2(83) As Date
        If Len(dataUtama(107)) = 0 Then
            result(2) = "kjcustomdate2 can't be empty" : GoTo selesai
        End If

        'kjcustomdate3(84) As Date
        If Len(dataUtama(108)) = 0 Then
            result(2) = "kjcustomdate3 can't be empty" : GoTo selesai
        End If

        'kjcustomdate4(85) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "kjcustomdate4 can't be empty" : GoTo selesai
        End If

        'kjcustomdate5(86) As Date
        If Len(dataUtama(110)) = 0 Then
            result(2) = "kjcustomdate5 can't be empty" : GoTo selesai
        End If

        'kjcustomdate6(87) As Date
        If Len(dataUtama(111)) = 0 Then
            result(2) = "kjcustomdate6 can't be empty" : GoTo selesai
        End If

        'kjcustomdate7(88) As Date
        If Len(dataUtama(112)) = 0 Then
            result(2) = "kjcustomdate7 can't be empty" : GoTo selesai
        End If

        'kjcustomdate8(89) As Date
        If Len(dataUtama(113)) = 0 Then
            result(2) = "kjcustomdate8 can't be empty" : GoTo selesai
        End If

        'kjcustomdate9(90) As Date
        If Len(dataUtama(114)) = 0 Then
            result(2) = "kjcustomdate9 can't be empty" : GoTo selesai
        End If

        'kjcustomdate10(91) As Date
        If Len(dataUtama(115)) = 0 Then
            result(2) = "kjcustomdate10 can't be empty" : GoTo selesai
        End If

        'kjcustomdate11(92) As Date
        If Len(dataUtama(116)) = 0 Then
            result(2) = "kjcustomdate11 can't be empty" : GoTo selesai
        End If

        'kjcustomdate12(93) As Date
        If Len(dataUtama(117)) = 0 Then
            result(2) = "kjcustomdate12 can't be empty" : GoTo selesai
        End If

        'kjcustomdate13(94) As Date
        If Len(dataUtama(118)) = 0 Then
            result(2) = "kjcustomdate13 can't be empty" : GoTo selesai
        End If

        'kjcustomdate14(95) As Date
        If Len(dataUtama(119)) = 0 Then
            result(2) = "kjcustomdate14 can't be empty" : GoTo selesai
        End If

        'kjcustomdate15(96) As Date
        If Len(dataUtama(120)) = 0 Then
            result(2) = "kjcustomdate15 can't be empty" : GoTo selesai
        End If

        'kjcustomdate16(97) As Date
        If Len(dataUtama(121)) = 0 Then
            result(2) = "kjcustomdate16 can't be empty" : GoTo selesai
        End If

        'kjcustomdate17(98) As Date
        If Len(dataUtama(122)) = 0 Then
            result(2) = "kjcustomdate17 can't be empty" : GoTo selesai
        End If

        'kjcustomdate18(99) As Date
        If Len(dataUtama(123)) = 0 Then
            result(2) = "kjcustomdate18 can't be empty" : GoTo selesai
        End If

        'kjcustomdate19(100) As Date
        If Len(dataUtama(124)) = 0 Then
            result(2) = "kjcustomdate19 can't be empty" : GoTo selesai
        End If

        'kjcustomdate20(101) As Date
        If Len(dataUtama(125)) = 0 Then
            result(2) = "kjcustomdate20 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kjid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnopasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprefix", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjumur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjeniskelamin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatusperkawinan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjagama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjayah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjibu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsuamiistri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnotelepon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprovinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnegara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkeluargalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnoteleponlain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglmeninggal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcarakunjungan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdirujukoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjditanggungoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuskamar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjkategoriharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdirujukke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuspasien", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdesa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkecamatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdiagnosa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjketerangan", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "kjid~kjcabang~kjlokasi~kjsumber~kjautonotransaksi~kjnotransaksi~kjtgl~kjkodepa~kjnopasien~kjnama~kjprefix~kjtgllahir~kjumur~kjjeniskelamin~kjstatusperkawinan~kjagama~kjayah~kjibu~kjsuamiistri~kjnotelepon~kjnofax~kjnohp~kjemail~kjalamat~kjkota~kjprovinsi~kjnegara~kjkodepos~kjkeluargalain~kjnoteleponlain~kjcatatan~kjtglkeluar~kjtglmeninggal~kjcarakunjungan~kjdirujukoleh~kjditanggungoleh~kjstatusrealisasi~kjstatus~kjstatussebelumnya~kjjmlrevisi~kjcetakanke~kjinputuser~kjinputtgl~kjmodifikasiuser~kjmodifikasitgl~kjisclose~kjcustomtext1~kjcustomtext2~kjcustomtext3~kjcustomtext4~kjcustomtext5~kjcustomtext6~kjcustomtext7~kjcustomtext8~kjcustomtext9~kjcustomtext10~kjcustomtext11~kjcustomtext12~kjcustomtext13~kjcustomtext14~kjcustomtext15~kjcustomtext16~kjcustomtext17~kjcustomtext18~kjcustomtext19~kjcustomtext20~kjcustomint1~kjcustomint2~kjcustomint3~kjcustomint4~kjcustomint5~kjcustomint6~kjcustomint7~kjcustomint8~kjcustomint9~kjcustomint10~kjcustomint11~kjcustomint12~kjcustomint13~kjcustomint14~kjcustomint15~kjcustomint16~kjcustomint17~kjcustomint18~kjcustomint19~kjcustomint20~kjcustomdbl1~kjcustomdbl2~kjcustomdbl3~kjcustomdbl4~kjcustomdbl5~kjcustomdbl6~kjcustomdbl7~kjcustomdbl8~kjcustomdbl9~kjcustomdbl10~kjcustomdbl11~kjcustomdbl12~kjcustomdbl13~kjcustomdbl14~kjcustomdbl15~kjcustomdbl16~kjcustomdbl17~kjcustomdbl18~kjcustomdbl19~kjcustomdbl20~kjcustomdate1~kjcustomdate2~kjcustomdate3~kjcustomdate4~kjcustomdate5~kjcustomdate6~kjcustomdate7~kjcustomdate8~kjcustomdate9~kjcustomdate10~kjcustomdate11~kjcustomdate12~kjcustomdate13~kjcustomdate14~kjcustomdate15~kjcustomdate16~kjcustomdate17~kjcustomdate18~kjcustomdate19~kjcustomdate20~kjstatuskamar~kjkategoriharga~kjperawatan~kjkategoripasien~kjlayanan~kjkamar~kjdokter~kjdirujukke~kjawalankatpasien~kjstatuspasien~kjpetugas~kjdesa~kjkecamatan~kjdiagnosa~kjketerangan", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122) & "~" & dataUtama(123) & "~" & dataUtama(124) & "~" & dataUtama(125) & "~" & dataUtama(126) & "~" & dataUtama(127) & "~" & dataUtama(128) & "~" & dataUtama(129) & "~" & dataUtama(130) & "~" & dataUtama(131) & "~" & dataUtama(132) & "~" & dataUtama(133) & "~" & dataUtama(134) & "~" & dataUtama(135) & "~" & dataUtama(136) & "~" & dataUtama(137) & "~" & dataUtama(138) & "~" & dataUtama(139) & "~" & dataUtama(140)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
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
                Dim vModuleId As Integer = 11, vMenuId As Integer = 3
                Select Case drutama("kjstatus")
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
                    result(4) = drutama("kjid")
                    notransaksi = drutama("kjnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(kjid), kjnotransaksi FROM M_11_kj WHERE kjid='" & result(4) & "' AND kjstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kjid) FROM M_11_kj WHERE kjnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m11_kj_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M11_Kj_HistorySimpan("" & paramSplit(0) & "★M11_Kj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kjsumber")) & "▼" & FixQuotes(drutama("kjid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update m_11_kj set kjcabang  = '" & FixQuotes(drutama("kjcabang")) & "', kjlokasi  = '" & FixQuotes(drutama("kjlokasi")) & "', kjsumber  = '" & FixQuotes(drutama("kjsumber")) & "', kjautonotransaksi  = '" & FixQuotes(drutama("kjautonotransaksi")) & "', kjnotransaksi  = '" & FixQuotes(drutama("kjnotransaksi")) & "', kjtgl  = '" & FixQuotes(AsFormatTanggal(drutama("kjtgl"))) & "', kjkodepa  = " & drutama("kjkodepa") & ", kjnopasien  = '" & FixQuotes(drutama("kjnopasien")) & "', kjnama = '" & FixQuotes(drutama("kjnama")) & "', kjprefix = '" & FixQuotes(drutama("kjprefix")) & "', kjtgllahir = '" & FixQuotes(AsFormatTanggal(drutama("kjtgllahir"))) & "', kjumur = " & drutama("kjumur") & ", kjjeniskelamin = '" & FixQuotes(drutama("kjjeniskelamin")) & "', kjstatusperkawinan = " & drutama("kjstatusperkawinan") & ", kjagama = " & drutama("kjagama") & ", kjayah = '" & FixQuotes(drutama("kjayah")) & "', kjibu = '" & FixQuotes(drutama("kjibu")) & "', kjsuamiistri = '" & FixQuotes(drutama("kjsuamiistri")) & "', kjnotelepon = '" & FixQuotes(drutama("kjnotelepon")) & "', kjnofax = '" & FixQuotes(drutama("kjnofax")) & "', kjnohp = '" & FixQuotes(drutama("kjnohp")) & "', kjemail = '" & FixQuotes(drutama("kjemail")) & "', kjalamat = '" & FixQuotes(drutama("kjalamat")) & "', kjkota = '" & FixQuotes(drutama("kjkota")) & "', kjprovinsi = '" & FixQuotes(drutama("kjprovinsi")) & "', kjnegara = '" & FixQuotes(drutama("kjnegara")) & "', kjkodepos = '" & FixQuotes(drutama("kjkodepos")) & "', kjkeluargalain = '" & FixQuotes(drutama("kjkeluargalain")) & "', kjnoteleponlain = '" & FixQuotes(drutama("kjnoteleponlain")) & "', kjcatatan = '" & FixQuotes(drutama("kjcatatan")) & "', kjtglkeluar  = '" & FixQuotes(AsFormatTanggal(drutama("kjtglkeluar"))) & "', kjtglmeninggal  = '" & FixQuotes(AsFormatTanggal(drutama("kjtglmeninggal"))) & "', kjcarakunjungan  = " & drutama("kjcarakunjungan") & ", kjdirujukoleh  = " & drutama("kjdirujukoleh") & ", kjditanggungoleh  = " & drutama("kjditanggungoleh") & ", kjstatusrealisasi  = " & drutama("kjstatusrealisasi") & ", kjstatus  = " & drutama("kjstatus") & ", kjstatussebelumnya  = " & drutama("kjstatussebelumnya") & ", kjjmlrevisi = kjjmlrevisi+1, kjcetakanke  = " & drutama("kjcetakanke") & ", kjmodifikasiuser  = " & drutama("kjmodifikasiuser") & ", kjmodifikasitgl  = NOW(), kjcustomtext1  = '" & FixQuotes(drutama("kjcustomtext1")) & "', kjcustomtext2  = '" & FixQuotes(drutama("kjcustomtext2")) & "', kjcustomtext3  = '" & FixQuotes(drutama("kjcustomtext3")) & "', kjcustomtext4  = '" & FixQuotes(drutama("kjcustomtext4")) & "', kjcustomtext5  = '" & FixQuotes(drutama("kjcustomtext5")) & "', kjcustomtext6  = '" & FixQuotes(drutama("kjcustomtext6")) & "', kjcustomtext7  = '" & FixQuotes(drutama("kjcustomtext7")) & "', kjcustomtext8  = '" & FixQuotes(drutama("kjcustomtext8")) & "', kjcustomtext9  = '" & FixQuotes(drutama("kjcustomtext9")) & "', kjcustomtext10  = '" & FixQuotes(drutama("kjcustomtext10")) & "', kjcustomtext11  = '" & FixQuotes(drutama("kjcustomtext11")) & "', kjcustomtext12  = '" & FixQuotes(drutama("kjcustomtext12")) & "', kjcustomtext13  = '" & FixQuotes(drutama("kjcustomtext13")) & "', kjcustomtext14  = '" & FixQuotes(drutama("kjcustomtext14")) & "', kjcustomtext15  = '" & FixQuotes(drutama("kjcustomtext15")) & "', kjcustomtext16  = '" & FixQuotes(drutama("kjcustomtext16")) & "', kjcustomtext17  = '" & FixQuotes(drutama("kjcustomtext17")) & "', kjcustomtext18  = '" & FixQuotes(drutama("kjcustomtext18")) & "', kjcustomtext19  = '" & FixQuotes(drutama("kjcustomtext19")) & "', kjcustomtext20  = '" & FixQuotes(drutama("kjcustomtext20")) & "', kjcustomint1  = " & drutama("kjcustomint1") & ", kjcustomint2  = " & drutama("kjcustomint2") & ", kjcustomint3  = " & drutama("kjcustomint3") & ", kjcustomint4  = " & drutama("kjcustomint14") & ", kjcustomint5  = " & drutama("kjcustomint5") & ", kjcustomint6  = " & drutama("kjcustomint6") & ", kjcustomint7  = " & drutama("kjcustomint7") & ", kjcustomint8  = " & drutama("kjcustomint8") & ", kjcustomint9  = " & drutama("kjcustomint9") & ", kjcustomint10  = " & drutama("kjcustomint10") & ", kjcustomint11  = " & drutama("kjcustomint11") & ", kjcustomint12  = " & drutama("kjcustomint12") & ", kjcustomint13  = " & drutama("kjcustomint13") & ", kjcustomint14 = " & drutama("kjcustomint14") & ", kjcustomint15  = " & drutama("kjcustomint15") & ", kjcustomint16  = " & drutama("kjcustomint16") & ", kjcustomint17  = " & drutama("kjcustomint17") & ", kjcustomint18  = " & drutama("kjcustomint18") & ", kjcustomint19  = " & drutama("kjcustomint19") & ", kjcustomint20  = " & drutama("kjcustomint20") & ", kjcustomdbl1  = '" & FixDouble(drutama("kjcustomdbl1")) & "', kjcustomdbl2  = '" & FixDouble(drutama("kjcustomdbl2")) & "', kjcustomdbl3  = '" & FixDouble(drutama("kjcustomdbl3")) & "', kjcustomdbl4  = '" & FixDouble(drutama("kjcustomdbl4")) & "', kjcustomdbl5  = '" & FixDouble(drutama("kjcustomdbl5")) & "', kjcustomdbl6  = '" & FixDouble(drutama("kjcustomdbl6")) & "', kjcustomdbl7  = '" & FixDouble(drutama("kjcustomdbl7")) & "', kjcustomdbl8  = '" & FixDouble(drutama("kjcustomdbl8")) & "', kjcustomdbl9  = '" & FixDouble(drutama("kjcustomdbl9")) & "', kjcustomdbl10  = '" & FixDouble(drutama("kjcustomdbl10")) & "', kjcustomdbl11  = '" & FixDouble(drutama("kjcustomdbl11")) & "', kjcustomdbl12  = '" & FixDouble(drutama("kjcustomdbl12")) & "', kjcustomdbl13  = '" & FixDouble(drutama("kjcustomdbl13")) & "', kjcustomdbl14  = '" & FixDouble(drutama("kjcustomdbl14")) & "', kjcustomdbl15  = '" & FixDouble(drutama("kjcustomdbl15")) & "', kjcustomdbl16  = '" & FixDouble(drutama("kjcustomdbl16")) & "', kjcustomdbl17  = '" & FixDouble(drutama("kjcustomdbl17")) & "', kjcustomdbl18  = '" & FixDouble(drutama("kjcustomdbl18")) & "', kjcustomdbl19  = '" & FixDouble(drutama("kjcustomdbl19")) & "', kjcustomdbl20  = '" & FixDouble(drutama("kjcustomdbl20")) & "', kjcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate1"))) & "', kjcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate2"))) & "', kjcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate3"))) & "', kjcustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate4"))) & "', kjcustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate5"))) & "', kjcustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate6"))) & "', kjcustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate7"))) & "', kjcustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate8"))) & "', kjcustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate9"))) & "', kjcustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate10"))) & "', kjcustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate11"))) & "', kjcustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate12"))) & "', kjcustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate13"))) & "', kjcustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate14"))) & "', kjcustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate15"))) & "', kjcustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate16"))) & "', kjcustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate17"))) & "', kjcustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate18"))) & "', kjcustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate19"))) & "', kjcustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate20"))) & "', kjstatuskamar = " & drutama("kjstatuskamar") & ", kjkategoriharga = '" & FixQuotes(drutama("kjkategoriharga")) & "', kjperawatan = '" & FixQuotes(drutama("kjperawatan")) & "', kjkategoripasien = '" & FixQuotes(drutama("kjkategoripasien")) & "', kjlayanan = '" & FixQuotes(drutama("kjlayanan")) & "', kjkamar = '" & FixQuotes(drutama("kjkamar")) & "', kjdokter = '" & FixQuotes(drutama("kjdokter")) & "', kjdirujukke = '" & FixQuotes(drutama("kjdirujukke")) & "', kjstatuspasien = " & drutama("kjstatuspasien") & ", kjpetugas = " & drutama("kjpetugas") & ", kjdesa = '" & FixQuotes(drutama("kjdesa")) & "', kjkecamatan = '" & FixQuotes(drutama("kjkecamatan")) & "', kjdiagnosa = '" & FixQuotes(drutama("kjdiagnosa")) & "', kjketerangan = " & drutama("kjketerangan") & " where kjid = '" & drutama("kjid") & "'"
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

                    Dim dtCekNoRM As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kjid), kjnopasien, kjnotransaksi FROM m_11_kj WHERE kjperawatan = 'RI' AND kjnopasien = '" & FixQuotes(drutama("kjnopasien")) & "' AND kjtgl = '" & drutama("kjtgl") & "'", myConn)
                    Dim cekNoRM As Double = Val(dtCekNoRM.Rows(0)(0))
                    If cekNoRM > 0 Then
                        result(2) = "Kunjungan pasien '" & dtCekNoRM.Rows(0)(1) & "' sudah dibuat di nomor '" & dtCekNoRM.Rows(0)(2) & "'" : Trans.Rollback() : GoTo selesai
                    End If

                    If drutama("kjautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("kjperawatan"), drutama("kjawalankatpasien"), drutama("kjsumber"), drutama("kjtgl"))
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
                        notransaksi = drutama("kjnotransaksi")
                    End If
                    'result(2) = notransaksi + " " + userid + " Dtdetail : " + dtdetail.Rows.Count.ToString : GoTo selesai
                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kjid) FROM m_11_kj WHERE kjnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into m_11_kj (kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10, kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20, kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5, kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10, kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15, kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20, kjstatuskamar, kjkategoriharga, kjperawatan, kjkategoripasien, kjlayanan, kjkamar, kjdokter, kjdirujukke, kjstatuspasien, kjpetugas, kjdesa, kjkecamatan, kjdiagnosa, kjketerangan) values('" & FixQuotes(drutama("kjcabang")) & "','" & FixQuotes(drutama("kjlokasi")) & "','" & FixQuotes(drutama("kjsumber")) & "', " & drutama("kjautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjtgl"))) & "', " & drutama("kjkodepa") & ", '" & FixQuotes(drutama("kjnopasien")) & "', '" & FixQuotes(drutama("kjnama")) & "', '" & FixQuotes(drutama("kjprefix")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjtgllahir"))) & "', " & drutama("kjumur") & ", '" & FixQuotes(drutama("kjjeniskelamin")) & "', " & drutama("kjstatusperkawinan") & ", " & drutama("kjagama") & ", '" & FixQuotes(drutama("kjayah")) & "', '" & FixQuotes(drutama("kjibu")) & "', '" & FixQuotes(drutama("kjsuamiistri")) & "', '" & FixQuotes(drutama("kjnotelepon")) & "', '" & FixQuotes(drutama("kjnofax")) & "', '" & FixQuotes(drutama("kjnohp")) & "', '" & FixQuotes(drutama("kjemail")) & "', '" & FixQuotes(drutama("kjalamat")) & "', '" & FixQuotes(drutama("kjkota")) & "', '" & FixQuotes(drutama("kjprovinsi")) & "', '" & FixQuotes(drutama("kjnegara")) & "', '" & FixQuotes(drutama("kjkodepos")) & "', '" & FixQuotes(drutama("kjkeluargalain")) & "', '" & FixQuotes(drutama("kjnoteleponlain")) & "', '" & FixQuotes(drutama("kjcatatan")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjtglkeluar"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjtglmeninggal"))) & "', " & drutama("kjcarakunjungan") & ", " & drutama("kjdirujukoleh") & ", " & drutama("kjditanggungoleh") & ", " & drutama("kjstatus") & ", " & drutama("kjstatussebelumnya") & ", " & drutama("kjjmlrevisi") & ", " & drutama("kjcetakanke") & ", " & drutama("kjinputuser") & ", NOW(), " & drutama("kjmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("kjisclose") & ", '" & FixQuotes(drutama("kjcustomtext1")) & "', '" & FixQuotes(drutama("kjcustomtext2")) & "', '" & FixQuotes(drutama("kjcustomtext3")) & "', '" & FixQuotes(drutama("kjcustomtext4")) & "', '" & FixQuotes(drutama("kjcustomtext5")) & "', '" & FixQuotes(drutama("kjcustomtext6")) & "', '" & FixQuotes(drutama("kjcustomtext7")) & "', '" & FixQuotes(drutama("kjcustomtext8")) & "', '" & FixQuotes(drutama("kjcustomtext9")) & "', '" & FixQuotes(drutama("kjcustomtext10")) & "', '" & FixQuotes(drutama("kjcustomtext11")) & "', '" & FixQuotes(drutama("kjcustomtext12")) & "', '" & FixQuotes(drutama("kjcustomtext13")) & "', '" & FixQuotes(drutama("kjcustomtext14")) & "', '" & FixQuotes(drutama("kjcustomtext15")) & "', '" & FixQuotes(drutama("kjcustomtext16")) & "', '" & FixQuotes(drutama("kjcustomtext17")) & "', '" & FixQuotes(drutama("kjcustomtext18")) & "', '" & FixQuotes(drutama("kjcustomtext19")) & "', '" & FixQuotes(drutama("kjcustomtext20")) & "', " & drutama("kjcustomint1") & ", " & drutama("kjcustomint2") & ", " & drutama("kjcustomint3") & ", " & drutama("kjcustomint4") & ", " & drutama("kjcustomint5") & ", " & drutama("kjcustomint6") & ", " & drutama("kjcustomint7") & ", " & drutama("kjcustomint8") & ", " & drutama("kjcustomint9") & ", " & drutama("kjcustomint10") & ", " & drutama("kjcustomint11") & ", " & drutama("kjcustomint12") & ", " & drutama("kjcustomint13") & ", " & drutama("kjcustomint14") & ", " & drutama("kjcustomint15") & ", " & drutama("kjcustomint16") & ", " & drutama("kjcustomint17") & ", " & drutama("kjcustomint18") & ", " & drutama("kjcustomint19") & ", " & drutama("kjcustomint20") & ", '" & FixDouble(drutama("kjcustomdbl1")) & "', '" & FixDouble(drutama("kjcustomdbl2")) & "', '" & FixDouble(drutama("kjcustomdbl3")) & "', '" & FixDouble(drutama("kjcustomdbl4")) & "', '" & FixDouble(drutama("kjcustomdbl5")) & "', '" & FixDouble(drutama("kjcustomdbl6")) & "', '" & FixDouble(drutama("kjcustomdbl7")) & "', '" & FixDouble(drutama("kjcustomdbl8")) & "', '" & FixDouble(drutama("kjcustomdbl9")) & "', '" & FixDouble(drutama("kjcustomdbl10")) & "', '" & FixDouble(drutama("kjcustomdbl11")) & "', '" & FixDouble(drutama("kjcustomdbl12")) & "', '" & FixDouble(drutama("kjcustomdbl13")) & "', '" & FixDouble(drutama("kjcustomdbl14")) & "', '" & FixDouble(drutama("kjcustomdbl15")) & "', '" & FixDouble(drutama("kjcustomdbl16")) & "', '" & FixDouble(drutama("kjcustomdbl17")) & "', '" & FixDouble(drutama("kjcustomdbl18")) & "', '" & FixDouble(drutama("kjcustomdbl19")) & "', '" & FixDouble(drutama("kjcustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kjcustomdate20"))) & "', " & drutama("kjstatuskamar") & ", '" & FixQuotes(drutama("kjkategoriharga")) & "', '" & FixQuotes(drutama("kjperawatan")) & "', '" & FixQuotes(drutama("kjkategoripasien")) & "', '" & FixQuotes(drutama("kjlayanan")) & "', '" & FixQuotes(drutama("kjkamar")) & "', '" & FixQuotes(drutama("kjdokter")) & "', '" & FixQuotes(drutama("kjdirujukke")) & "', " & drutama("kjstatuspasien") & ", " & drutama("kjpetugas") & ", '" & FixQuotes(drutama("kjdesa")) & "', '" & FixQuotes(drutama("kjkecamatan")) & "', '" & FixQuotes(drutama("kjdiagnosa")) & "', " & drutama("kjketerangan") & ")"
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
                    dt2 = AsDataTableAmbilDariDBCon("select kjid from m_11_kj where kjnotransaksi='" & notransaksi & "' AND kjinputuser= '" & userid & "' order by kjmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "KJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_KjUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "KJ", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT kjtgl, kjnotransaksi, kjstatus FROM m_11_kj WHERE kjid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "kjstatussebelumnya" : jnsaktivitas = 17
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

            ' ''SIMPAN HISTORY ========================
            ''Dim SimpanHistory As New m5_so_history
            ''Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            ''Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            ''Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ' ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            ''If (rsSplitResult(1) = 0) Then
            ''    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            ''End If
            ' ''END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                'sql = query.m11_kj_terkait("kjid = '" & idtransaksi & "'")
                sql = query.PanggilQuery("m11_kj_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)

                myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
                myConn.Open()

                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

                ''AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idsqdetail, urutan FROM m5_so_detail WHERE idso = '" & idtransaksi & "'")
                'If dtdetail.Rows.Count > 0 Then
                '    For Each dr1 As DataRow In dtdetail.Rows
                '        'BUAT FILTER UNTUK UPDATE ---------------------------------
                '        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idsqdetail = dr1("idsqdetail")

                '        'UPDATE OUTSTANDING ---------------------------
                '        If idsqdetail <> 0 Then
                '            '1. SET NILAI UPDATE OUTSTANDING
                '            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                '            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN jmlrealisasi - '" & Outstanding & "' ", updNilai)

                '            '2. SET FILTERUPDATE OUTSTANDING
                '            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                '            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
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
                '    sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                '    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                '    If dtOut.Rows.Count > 0 Then
                '        For Each dr1 As DataRow In dtOut.Rows
                '            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                '            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                '        Next
                '    End If
                '    dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
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

                'UPDATE STOK BOOKING ================================
                'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
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

            'JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
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
            sql = "UPDATE M_11_kj SET kjstatus = " & nilaiStatus & ", kjmodifikasiuser='" & userid & "', kjmodifikasitgl = NOW(), kjjmlrevisi = kjjmlrevisi + 1 WHERE kjid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_KjSearch(PostWsSearch(paramSplit(0), "M11_KjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_KjDelete(ByVal param As String) As String

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
            Dim sumber As String = "KJ", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT kjid, kjnotransaksi FROM m_11_kj WHERE kjid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl"
            sql &= " FROM m_11_kj"
            sql &= " WHERE kjid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("kjcabang")
                lokasi = dtNomorNext.Rows(0)("kjlokasi")
                sumber = dtNomorNext.Rows(0)("kjsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("kjautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("kjnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("kjtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE UTAMA
            sql = "DELETE FROM m_11_kj WHERE kjid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_KjSearch(PostWsSearch(paramSplit(0), "M11_KjSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M11_KjGetdataById(ByVal param As String) As String
        'M11_Kj_GetdataById Utama --------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20


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

        Dim utama As String = "", detail As String = "", akdetail As String = "", ludetail As String = "", kmutama As String = "", lbutama As String = "", rkutama As String = "", rodetail As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M11_Kj-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "kjid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "kjid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_kj_getdata")

        dt = AmbilData("aplikasi1-M11_kj_getdata", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'result(2) = idtransaksi & "  " & Filter & " jml dt: " & dt.Rows.Count.ToString : GoTo selesai

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kjid"), 0), sptField,
                     FxDB(drutama("kjcabang"), ""), sptField,
                     FxDB(drutama("kjlokasi"), ""), sptField,
                     FxDB(drutama("kjsumber"), ""), sptField,
                     FxDB(drutama("kjautonotransaksi"), 0), sptField,
                     FxDB(drutama("kjnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kjtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("kjkodepa"), 0), sptField,
                     FxDB(drutama("kjnopasien"), ""), sptField,
                     FxDB(drutama("kjnama"), ""), sptField,
                     FxDB(drutama("kjprefix"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kjtgllahir"), ""), formatTgl), sptField,
                     FxDB(drutama("kjumur"), 0), sptField,
                     FxDB(drutama("kjjeniskelamin"), ""), sptField,
                     FxDB(drutama("kjstatusperkawinan"), 0), sptField,
                     FxDB(drutama("kjagama"), 0), sptField,
                     FxDB(drutama("kjayah"), ""), sptField,
                     FxDB(drutama("kjibu"), ""), sptField,
                     FxDB(drutama("kjsuamiistri"), ""), sptField,
                     FxDB(drutama("kjnotelepon"), ""), sptField,
                     FxDB(drutama("kjnofax"), ""), sptField,
                     FxDB(drutama("kjnohp"), ""), sptField,
                     FxDB(drutama("kjemail"), ""), sptField,
                     FxDB(drutama("kjalamat"), ""), sptField,
                     FxDB(drutama("kjkota"), ""), sptField,
                     FxDB(drutama("kjprovinsi"), ""), sptField,
                     FxDB(drutama("kjnegara"), ""), sptField,
                     FxDB(drutama("kjkodepos"), ""), sptField,
                     FxDB(drutama("kjkeluargalain"), ""), sptField,
                     FxDB(drutama("kjnoteleponlain"), ""), sptField,
                     FxDB(drutama("kjcatatan"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kjtglkeluar"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjtglmeninggal"), ""), formatTgl), sptField,
                     FxDB(drutama("kjcarakunjungan"), 0), sptField,
                     FxDB(drutama("kjdirujukoleh"), 0), sptField,
                     FxDB(drutama("kjditanggungoleh"), 0), sptField,
                     FxDB(drutama("kjstatusrealisasi"), 0), sptField,
                     FxDB(drutama("kjstatus"), 0), sptField,
                     FxDB(drutama("kjstatussebelumnya"), 0), sptField,
                     FxDB(drutama("kjjmlrevisi"), 0), sptField,
                     FxDB(drutama("kjcetakanke"), 0), sptField,
                     FxDB(drutama("kjinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kjinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kjmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kjmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kjisclose"), 0), sptField,
                     FxDB(drutama("kjcustomtext1"), ""), sptField,
                     FxDB(drutama("kjcustomtext2"), ""), sptField,
                     FxDB(drutama("kjcustomtext3"), ""), sptField,
                     FxDB(drutama("kjcustomtext4"), ""), sptField,
                     FxDB(drutama("kjcustomtext5"), ""), sptField,
                     FxDB(drutama("kjcustomtext6"), ""), sptField,
                     FxDB(drutama("kjcustomtext7"), ""), sptField,
                     FxDB(drutama("kjcustomtext8"), ""), sptField,
                     FxDB(drutama("kjcustomtext9"), ""), sptField,
                     FxDB(drutama("kjcustomtext10"), ""), sptField,
                     FxDB(drutama("kjcustomtext11"), ""), sptField,
                     FxDB(drutama("kjcustomtext12"), ""), sptField,
                     FxDB(drutama("kjcustomtext13"), ""), sptField,
                     FxDB(drutama("kjcustomtext14"), ""), sptField,
                     FxDB(drutama("kjcustomtext15"), ""), sptField,
                     FxDB(drutama("kjcustomtext16"), ""), sptField,
                     FxDB(drutama("kjcustomtext17"), ""), sptField,
                     FxDB(drutama("kjcustomtext18"), ""), sptField,
                     FxDB(drutama("kjcustomtext19"), ""), sptField,
                     FxDB(drutama("kjcustomtext20"), ""), sptField,
                     FxDB(drutama("kjcustomint1"), 0), sptField,
                     FxDB(drutama("kjcustomint2"), 0), sptField,
                     FxDB(drutama("kjcustomint3"), 0), sptField,
                     FxDB(drutama("kjcustomint4"), 0), sptField,
                     FxDB(drutama("kjcustomint5"), 0), sptField,
                     FxDB(drutama("kjcustomint6"), 0), sptField,
                     FxDB(drutama("kjcustomint7"), 0), sptField,
                     FxDB(drutama("kjcustomint8"), 0), sptField,
                     FxDB(drutama("kjcustomint9"), 0), sptField,
                     FxDB(drutama("kjcustomint10"), 0), sptField,
                     FxDB(drutama("kjcustomint11"), 0), sptField,
                     FxDB(drutama("kjcustomint12"), 0), sptField,
                     FxDB(drutama("kjcustomint13"), 0), sptField,
                     FxDB(drutama("kjcustomint14"), 0), sptField,
                     FxDB(drutama("kjcustomint15"), 0), sptField,
                     FxDB(drutama("kjcustomint16"), 0), sptField,
                     FxDB(drutama("kjcustomint17"), 0), sptField,
                     FxDB(drutama("kjcustomint18"), 0), sptField,
                     FxDB(drutama("kjcustomint19"), 0), sptField,
                     FxDB(drutama("kjcustomint20"), 0), sptField,
                     FxDB(drutama("kjcustomdbl1"), 0), sptField,
                     FxDB(drutama("kjcustomdbl2"), 0), sptField,
                     FxDB(drutama("kjcustomdbl3"), 0), sptField,
                     FxDB(drutama("kjcustomdbl4"), 0), sptField,
                     FxDB(drutama("kjcustomdbl5"), 0), sptField,
                     FxDB(drutama("kjcustomdbl6"), 0), sptField,
                     FxDB(drutama("kjcustomdbl7"), 0), sptField,
                     FxDB(drutama("kjcustomdbl8"), 0), sptField,
                     FxDB(drutama("kjcustomdbl9"), 0), sptField,
                     FxDB(drutama("kjcustomdbl10"), 0), sptField,
                     FxDB(drutama("kjcustomdbl11"), 0), sptField,
                     FxDB(drutama("kjcustomdbl12"), 0), sptField,
                     FxDB(drutama("kjcustomdbl13"), 0), sptField,
                     FxDB(drutama("kjcustomdbl14"), 0), sptField,
                     FxDB(drutama("kjcustomdbl15"), 0), sptField,
                     FxDB(drutama("kjcustomdbl16"), 0), sptField,
                     FxDB(drutama("kjcustomdbl17"), 0), sptField,
                     FxDB(drutama("kjcustomdbl18"), 0), sptField,
                     FxDB(drutama("kjcustomdbl19"), 0), sptField,
                     FxDB(drutama("kjcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kjcustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("kjstatuskamar"), 0), sptField,
                     FxDB(drutama("kjstatusnama"), ""), sptField,
                     FxDB(drutama("kjstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("kjinputusernama"), ""), sptField,
                     FxDB(drutama("kjmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kjcabangnama"), ""), sptField,
                     FxDB(drutama("kjlokasinama"), ""), sptField,
                     FxDB(drutama("kjdirujukolehkode"), ""), sptField,
                     FxDB(drutama("kjdirujukolehnama"), ""), sptField,
                     FxDB(drutama("kjditanggungolehkode"), ""), sptField,
                     FxDB(drutama("kjditanggungolehnama"), ""), sptField,
                     FxDB(drutama("kjkategoriharga"), ""), sptField,
                     FxDB(drutama("kjperawatan"), ""), sptField,
                     FxDB(drutama("kjkategoripasien"), ""), sptField,
                     FxDB(drutama("kjlayanan"), ""), sptField,
                     FxDB(drutama("kjkamar"), ""), sptField,
                     FxDB(drutama("kjdokter"), ""), sptField,
                     FxDB(drutama("kjdirujukke"), ""), sptField,
                     FxDB(drutama("kjawalankatpasien"), ""), sptField,
                     FxDB(drutama("kjkategoripasiennama"), ""), sptField,
                     FxDB(drutama("kjkamarnama"), ""), sptField,
                     FxDB(drutama("kjdokternama"), ""), sptField,
                     FxDB(drutama("kjdirujukkenama"), ""), sptField,
                     FxDB(drutama("kjlayanannama"), ""), sptField,
                     FxDB(drutama("kjstatuspasien"), 0), sptField,
                     FxDB(drutama("kjpetugas"), 0), sptField,
                     FxDB(drutama("kjpetugaskode"), ""), sptField,
                     FxDB(drutama("kjdesa"), ""), sptField,
                     FxDB(drutama("kjkecamatan"), ""), sptField,
                     FxDB(drutama("kjkotanama"), 0), sptField,
                     FxDB(drutama("kjprovinsinama"), 0), sptField,
                     FxDB(drutama("kjnegaranama"), ""), sptField,
                     FxDB(drutama("kjkecamatannama"), ""), sptField,
                     FxDB(drutama("kjdesanama"), ""), sptField,
                     FxDB(drutama("kjpetugasnama"), ""), sptField,
                     FxDB(drutama("kjdiagnosa"), ""), sptField,
                     FxDB(drutama("kjdiagnosanama"), ""), sptField,
                     FxDB(drutama("kjketerangan"), 0), sptRow)

            'AMBIL DATA LU
            sql = query.PanggilQuery("m11_lu_getdata")
            Dim dtlu As New DataTable
            dtlu = AmbilData("aplikasi1-M11_lu_getdata", "luidkj = '" & idtransaksi & "' AND (lustatus = 2 OR lustatus = 3 OR lustatus = 4)", "lunotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "lunotransaksi", sql) ' Ambil data ke databases
            If dtlu.Rows.Count > 0 Then
                For Each drutamalu As DataRow In dtlu.Rows
                    ludetail = String.Concat(ludetail, FxDB(drutamalu("luid"), 0), sptField,
                          FxDB(drutamalu("lunotransaksi"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamalu("lutgl"), ""), formatTgl), sptField,
                          FxDB(drutamalu("luuraian"), ""), sptField,
                          FxDB(drutamalu("lutotaltransaksi"), 0), sptField,
                          FxDB(drutamalu("lucatatan"), ""), sptField,
                          FxDB(drutamalu("luinputusernama"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamalu("luinputtgl"), ""), formatTglWaktu), sptRow)

                Next
            End If
            'For Each dr As DataRow In dtlu.Rows
            '    ludetail = String.Concat(ludetail, FxDB(dr("luid"), 0), sptField,
            '         FxDB(dr("lunotransaksi"), ""), sptField,
            '         FxDB(dr("lutotaltransaksi"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("luinputtgl"), ""), formatTglWaktu), sptField,
            '         AsFormatTanggal(FxDB(dr("lumodifikasitgl"), ""), formatTglWaktu), sptField,
            '         FxDB(dr("luinputusernama"), ""), sptField,
            '         FxDB(dr("lumodifikasiusernama"), ""), sptField,
            '         FxDB(dr("idludetail"), 0), sptField,
            '         FxDB(dr("idlu"), 0), sptField,
            '         FxDB(dr("jenis"), ""), sptField,
            '         FxDB(dr("idlayanan"), 0), sptField,
            '         FxDB(dr("namalayanan"), ""), sptField,
            '         FxDB(dr("jml"), 0), sptField,
            '         FxDB(dr("satuan"), ""), sptField,
            '         FxDB(dr("nilaisatuan"), 0), sptField,
            '         FxDB(dr("jmltotal"), 0), sptField,
            '         FxDB(dr("satuandefault"), ""), sptField,
            '         FxDB(dr("harga"), 0), sptField,
            '         FxDB(dr("diskon"), ""), sptField,
            '         FxDB(dr("jmldiskon"), 0), sptField,
            '         FxDB(dr("pajak1"), ""), sptField,
            '         FxDB(dr("jmlpajak1"), 0), sptField,
            '         FxDB(dr("pajak2"), ""), sptField,
            '         FxDB(dr("jmlpajak2"), 0), sptField,
            '         FxDB(dr("cabang"), ""), sptField,
            '         FxDB(dr("lokasi"), ""), sptField,
            '         FxDB(dr("gudang"), ""), sptField,
            '         FxDB(dr("costcenter"), ""), sptField,
            '         FxDB(dr("divisi"), ""), sptField,
            '         FxDB(dr("subdivisi"), ""), sptField,
            '         FxDB(dr("proyek"), ""), sptField,
            '         FxDB(dr("catatan"), ""), sptField,
            '         FxDB(dr("urutan"), 0), sptField,
            '         FxDB(dr("idkjdetail"), 0), sptField,
            '         FxDB(dr("jmlrealisasi"), 0), sptField,
            '         FxDB(dr("statusrealisasi"), 0), sptField,
            '         FxDB(dr("isclose"), 0), sptField,
            '         FxDB(dr("iddokter"), 0), sptField,
            '         FxDB(dr("namadokter"), ""), sptField,
            '         FxDB(dr("customtext1"), ""), sptField,
            '         FxDB(dr("customtext2"), ""), sptField,
            '         FxDB(dr("customtext3"), ""), sptField,
            '         FxDB(dr("customtext4"), ""), sptField,
            '         FxDB(dr("customtext5"), ""), sptField,
            '         FxDB(dr("customtext6"), ""), sptField,
            '         FxDB(dr("customtext7"), ""), sptField,
            '         FxDB(dr("customtext8"), ""), sptField,
            '         FxDB(dr("customtext9"), ""), sptField,
            '         FxDB(dr("customtext10"), ""), sptField,
            '         FxDB(dr("customtext11"), ""), sptField,
            '         FxDB(dr("customtext12"), ""), sptField,
            '         FxDB(dr("customtext13"), ""), sptField,
            '         FxDB(dr("customtext14"), ""), sptField,
            '         FxDB(dr("customtext15"), ""), sptField,
            '         FxDB(dr("customtext16"), ""), sptField,
            '         FxDB(dr("customtext17"), ""), sptField,
            '         FxDB(dr("customtext18"), ""), sptField,
            '         FxDB(dr("customtext19"), ""), sptField,
            '         FxDB(dr("customtext20"), ""), sptField,
            '         FxDB(dr("customdbl1"), 0), sptField,
            '         FxDB(dr("customdbl2"), 0), sptField,
            '         FxDB(dr("customdbl3"), 0), sptField,
            '         FxDB(dr("customdbl4"), 0), sptField,
            '         FxDB(dr("customdbl5"), 0), sptField,
            '         FxDB(dr("customdbl6"), 0), sptField,
            '         FxDB(dr("customdbl7"), 0), sptField,
            '         FxDB(dr("customdbl8"), 0), sptField,
            '         FxDB(dr("customdbl9"), 0), sptField,
            '         FxDB(dr("customdbl10"), 0), sptField,
            '         FxDB(dr("customdbl11"), 0), sptField,
            '         FxDB(dr("customdbl12"), 0), sptField,
            '         FxDB(dr("customdbl13"), 0), sptField,
            '         FxDB(dr("customdbl14"), 0), sptField,
            '         FxDB(dr("customdbl15"), 0), sptField,
            '         FxDB(dr("customdbl16"), 0), sptField,
            '         FxDB(dr("customdbl17"), 0), sptField,
            '         FxDB(dr("customdbl18"), 0), sptField,
            '         FxDB(dr("customdbl19"), 0), sptField,
            '         FxDB(dr("customdbl20"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
            '         FxDB(dr("kodelayanan"), ""), sptField,
            '         FxDB(dr("pajak1nama"), ""), sptField,
            '         FxDB(dr("pajak1nilai"), 0), sptField,
            '         FxDB(dr("pajak2nama"), ""), sptField,
            '         FxDB(dr("pajak2nilai"), 0), sptField,
            '         FxDB(dr("cabangnama"), ""), sptField,
            '         FxDB(dr("lokasinama"), ""), sptField,
            '         FxDB(dr("gudangnama"), ""), sptField,
            '         FxDB(dr("costcenternama"), ""), sptField,
            '         FxDB(dr("divisinama"), ""), sptField,
            '         FxDB(dr("subdivisinama"), ""), sptField,
            '         FxDB(dr("proyeknama"), ""), sptField,
            '         FxDB(dr("kjnotransaksi"), ""), sptField,
            '         FxDB(dr("kodedokter"), ""), sptRow)
            'Next
            If ludetail.Length > 0 Then ludetail = ludetail.Substring(0, ludetail.Length - sptRow.Length) Else ludetail = ludetail

            'AMBIL DATA AK
            sql = query.PanggilQuery("m11_ak_getdata")
            Dim dtak As New DataTable
            dtak = AmbilData("aplikasi1-m_11_ak", "akidkj = '" & idtransaksi & "' AND (akstatus = 2 OR akstatus = 3 OR akstatus = 4)", "aknotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "aknotransaksi", sql) ' Ambil data ke databases
            'result(2) = "Nananana " & dtak.Rows.Count.ToString : GoTo selesai
            If dtak.Rows.Count > 0 Then
                For Each drutamaak As DataRow In dtak.Rows
                    akdetail = String.Concat(akdetail, FxDB(drutamaak("akid"), 0), sptField,
                          FxDB(drutamaak("aknotransaksi"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamaak("aktgl"), ""), formatTgl), sptField,
                          FxDB(drutamaak("aknoref"), ""), sptField,
                          FxDB(drutamaak("akuraian"), ""), sptField,
                          FxDB(drutamaak("aktotaltransaksi"), 0), sptField,
                          FxDB(drutamaak("akcatatan"), ""), sptField,
                          FxDB(drutamaak("akinputusernama"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamaak("akinputtgl"), ""), formatTglWaktu), sptField,
                          FxDB(drutamaak("aktotalobat"), 0), sptField,
                          FxDB(drutamaak("akresep"), 0), sptField,
                          FxDB(drutamaak("akracik"), 0), sptField,
                          FxDB(drutamaak("akembalase"), 0), sptRow)
                Next
            End If
            'For Each dr As DataRow In dtak.Rows
            '    akdetail = String.Concat(akdetail, FxDB(dr("akid"), 0), sptField,
            '         FxDB(dr("aknotransaksi"), ""), sptField,
            '         FxDB(dr("aktotaltransaksi"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("akinputtgl"), ""), formatTglWaktu), sptField,
            '         AsFormatTanggal(FxDB(dr("akmodifikasitgl"), ""), formatTglWaktu), sptField,
            '         FxDB(dr("akinputusernama"), ""), sptField,
            '         FxDB(dr("akmodifikasiusernama"), ""), sptField,
            '         FxDB(dr("idakdetail"), 0), sptField,
            '         FxDB(dr("idak"), 0), sptField,
            '         FxDB(dr("jenis"), ""), sptField,
            '         FxDB(dr("idlayanan"), 0), sptField,
            '         FxDB(dr("namalayanan"), ""), sptField,
            '         FxDB(dr("jml"), 0), sptField,
            '         FxDB(dr("satuan"), ""), sptField,
            '         FxDB(dr("nilaisatuan"), 0), sptField,
            '         FxDB(dr("jmltotal"), 0), sptField,
            '         FxDB(dr("satuandefault"), ""), sptField,
            '         FxDB(dr("harga"), 0), sptField,
            '         FxDB(dr("diskon"), ""), sptField,
            '         FxDB(dr("jmldiskon"), 0), sptField,
            '         FxDB(dr("pajak1"), ""), sptField,
            '         FxDB(dr("jmlpajak1"), 0), sptField,
            '         FxDB(dr("pajak2"), ""), sptField,
            '         FxDB(dr("jmlpajak2"), 0), sptField,
            '         FxDB(dr("cabang"), ""), sptField,
            '         FxDB(dr("lokasi"), ""), sptField,
            '         FxDB(dr("gudang"), ""), sptField,
            '         FxDB(dr("costcenter"), ""), sptField,
            '         FxDB(dr("divisi"), ""), sptField,
            '         FxDB(dr("subdivisi"), ""), sptField,
            '         FxDB(dr("proyek"), ""), sptField,
            '         FxDB(dr("catatan"), ""), sptField,
            '         FxDB(dr("urutan"), 0), sptField,
            '         FxDB(dr("idkjdetail"), 0), sptField,
            '         FxDB(dr("jmlrealisasi"), 0), sptField,
            '         FxDB(dr("statusrealisasi"), 0), sptField,
            '         FxDB(dr("isclose"), 0), sptField,
            '         FxDB(dr("iddokter"), 0), sptField,
            '         FxDB(dr("namadokter"), ""), sptField,
            '         FxDB(dr("customtext1"), ""), sptField,
            '         FxDB(dr("customtext2"), ""), sptField,
            '         FxDB(dr("customtext3"), ""), sptField,
            '         FxDB(dr("customtext4"), ""), sptField,
            '         FxDB(dr("customtext5"), ""), sptField,
            '         FxDB(dr("customtext6"), ""), sptField,
            '         FxDB(dr("customtext7"), ""), sptField,
            '         FxDB(dr("customtext8"), ""), sptField,
            '         FxDB(dr("customtext9"), ""), sptField,
            '         FxDB(dr("customtext10"), ""), sptField,
            '         FxDB(dr("customtext11"), ""), sptField,
            '         FxDB(dr("customtext12"), ""), sptField,
            '         FxDB(dr("customtext13"), ""), sptField,
            '         FxDB(dr("customtext14"), ""), sptField,
            '         FxDB(dr("customtext15"), ""), sptField,
            '         FxDB(dr("customtext16"), ""), sptField,
            '         FxDB(dr("customtext17"), ""), sptField,
            '         FxDB(dr("customtext18"), ""), sptField,
            '         FxDB(dr("customtext19"), ""), sptField,
            '         FxDB(dr("customtext20"), ""), sptField,
            '         FxDB(dr("customdbl1"), 0), sptField,
            '         FxDB(dr("customdbl2"), 0), sptField,
            '         FxDB(dr("customdbl3"), 0), sptField,
            '         FxDB(dr("customdbl4"), 0), sptField,
            '         FxDB(dr("customdbl5"), 0), sptField,
            '         FxDB(dr("customdbl6"), 0), sptField,
            '         FxDB(dr("customdbl7"), 0), sptField,
            '         FxDB(dr("customdbl8"), 0), sptField,
            '         FxDB(dr("customdbl9"), 0), sptField,
            '         FxDB(dr("customdbl10"), 0), sptField,
            '         FxDB(dr("customdbl11"), 0), sptField,
            '         FxDB(dr("customdbl12"), 0), sptField,
            '         FxDB(dr("customdbl13"), 0), sptField,
            '         FxDB(dr("customdbl14"), 0), sptField,
            '         FxDB(dr("customdbl15"), 0), sptField,
            '         FxDB(dr("customdbl16"), 0), sptField,
            '         FxDB(dr("customdbl17"), 0), sptField,
            '         FxDB(dr("customdbl18"), 0), sptField,
            '         FxDB(dr("customdbl19"), 0), sptField,
            '         FxDB(dr("customdbl20"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
            '         FxDB(dr("kodelayanan"), ""), sptField,
            '         FxDB(dr("pajak1nama"), ""), sptField,
            '         FxDB(dr("pajak1nilai"), 0), sptField,
            '         FxDB(dr("pajak2nama"), ""), sptField,
            '         FxDB(dr("pajak2nilai"), 0), sptField,
            '         FxDB(dr("cabangnama"), ""), sptField,
            '         FxDB(dr("lokasinama"), ""), sptField,
            '         FxDB(dr("gudangnama"), ""), sptField,
            '         FxDB(dr("costcenternama"), ""), sptField,
            '         FxDB(dr("divisinama"), ""), sptField,
            '         FxDB(dr("subdivisinama"), ""), sptField,
            '         FxDB(dr("proyeknama"), ""), sptField,
            '         FxDB(dr("kjnotransaksi"), ""), sptField,
            '         FxDB(dr("kodedokter"), ""), sptField,
            '         FxDB(dr("akresep"), 0), sptField,
            '         FxDB(dr("akracik"), 0), sptField,
            '         FxDB(dr("akembalase"), 0), sptRow)
            'Next
            If akdetail.Length > 0 Then akdetail = akdetail.Substring(0, akdetail.Length - sptRow.Length) Else akdetail = akdetail

            'AMBIL DATA KM
            sql = query.PanggilQuery("m11_km_getdata")
            Dim dtkm As New DataTable
            dtkm = AmbilData("aplikasi1-M11_km_getdata", "kmidkj = '" & idtransaksi & "' AND (kmstatus = 2 OR kmstatus = 3  OR kmstatus = 4)", "kmnotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            If dtkm.Rows.Count > 0 Then
                Dim drkmutama As DataRow = dtkm.Rows(0)
                kmutama = String.Concat(FxDB(drkmutama("kmid"), 0), sptField,
                         FxDB(drkmutama("kmcabang"), ""), sptField,
                         FxDB(drkmutama("kmlokasi"), ""), sptField,
                         FxDB(drkmutama("kmgudang"), ""), sptField,
                         FxDB(drkmutama("kmsumber"), ""), sptField,
                         FxDB(drkmutama("kmautonotransaksi"), 0), sptField,
                         FxDB(drkmutama("kmnotransaksi"), ""), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmtgl"), ""), formatTgl), sptField,
                         FxDB(drkmutama("kmkodepa"), 0), sptField,
                         FxDB(drkmutama("kmcustomer"), 0), sptField,
                         FxDB(drkmutama("kmcustomerkontak"), ""), sptField,
                         FxDB(drkmutama("kmuraian"), ""), sptField,
                         FxDB(drkmutama("kmcatatan"), ""), sptField,
                         FxDB(drkmutama("kmnoref"), ""), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmtglnoref"), ""), formatTgl), sptField,
                         FxDB(drkmutama("kmidkj"), 0), sptField,
                         FxDB(drkmutama("kmkamar"), ""), sptField,
                         FxDB(drkmutama("kmkasur"), ""), sptField,
                         FxDB(drkmutama("kmtglmasuk"), ""), sptField,
                         FxDB(drkmutama("kmtglkeluar"), ""), sptField,
                         FxDB(drkmutama("kmjmlhari"), 0), sptField,
                         FxDB(drkmutama("kmharga"), 0), sptField,
                         FxDB(drkmutama("kmtotaltransaksi"), 0), sptField,
                         FxDB(drkmutama("kmstatusrealisasi"), 0), sptField,
                         FxDB(drkmutama("kmstatus"), 0), sptField,
                         FxDB(drkmutama("kmstatussebelumnya"), 0), sptField,
                         FxDB(drkmutama("kmjmlrevisi"), 0), sptField,
                         FxDB(drkmutama("kmcetakanke"), 0), sptField,
                         FxDB(drkmutama("kminputuser"), 0), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kminputtgl"), ""), formatTglWaktu), sptField,
                         FxDB(drkmutama("kmmodifikasiuser"), 0), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                         FxDB(drkmutama("kmisclose"), 0), sptField,
                         FxDB(drkmutama("kmcustomtext1"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext2"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext3"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext4"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext5"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext6"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext7"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext8"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext9"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext10"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext11"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext12"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext13"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext14"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext15"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext16"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext17"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext18"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext19"), ""), sptField,
                         FxDB(drkmutama("kmcustomtext20"), ""), sptField,
                         FxDB(drkmutama("kmcustomint1"), 0), sptField,
                         FxDB(drkmutama("kmcustomint2"), 0), sptField,
                         FxDB(drkmutama("kmcustomint3"), 0), sptField,
                         FxDB(drkmutama("kmcustomint4"), 0), sptField,
                         FxDB(drkmutama("kmcustomint5"), 0), sptField,
                         FxDB(drkmutama("kmcustomint6"), 0), sptField,
                         FxDB(drkmutama("kmcustomint7"), 0), sptField,
                         FxDB(drkmutama("kmcustomint8"), 0), sptField,
                         FxDB(drkmutama("kmcustomint9"), 0), sptField,
                         FxDB(drkmutama("kmcustomint10"), 0), sptField,
                         FxDB(drkmutama("kmcustomint11"), 0), sptField,
                         FxDB(drkmutama("kmcustomint12"), 0), sptField,
                         FxDB(drkmutama("kmcustomint13"), 0), sptField,
                         FxDB(drkmutama("kmcustomint14"), 0), sptField,
                         FxDB(drkmutama("kmcustomint15"), 0), sptField,
                         FxDB(drkmutama("kmcustomint16"), 0), sptField,
                         FxDB(drkmutama("kmcustomint17"), 0), sptField,
                         FxDB(drkmutama("kmcustomint18"), 0), sptField,
                         FxDB(drkmutama("kmcustomint19"), 0), sptField,
                         FxDB(drkmutama("kmcustomint20"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl1"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl2"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl3"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl4"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl5"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl6"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl7"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl8"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl9"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl10"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl11"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl12"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl13"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl14"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl15"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl16"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl17"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl18"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl19"), 0), sptField,
                         FxDB(drkmutama("kmcustomdbl20"), 0), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate1"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate2"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate3"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate4"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate5"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate6"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate7"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate8"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate9"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate10"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate11"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate12"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate13"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate14"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate15"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate16"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate17"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate18"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate19"), ""), formatTgl), sptField,
                         AsFormatTanggal(FxDB(drkmutama("kmcustomdate20"), ""), formatTgl), sptField,
                         FxDB(drkmutama("kmcabangnama"), ""), sptField,
                         FxDB(drkmutama("kmlokasinama"), ""), sptField,
                         FxDB(drkmutama("kmgudangnama"), ""), sptField,
                         FxDB(drkmutama("kmcustomerkode"), ""), sptField,
                         FxDB(drkmutama("kmcustomernama"), ""), sptField,
                         FxDB(drkmutama("kmnotransaksikj"), ""), sptField,
                         FxDB(drkmutama("kmkamarnama"), ""), sptField,
                         FxDB(drkmutama("kmkasurnama"), ""), sptField,
                         FxDB(drkmutama("kmstatusnama"), ""), sptField,
                         FxDB(drkmutama("kmstatussebelumnyanama"), ""), sptField,
                         FxDB(drkmutama("kminputusernama"), ""), sptField,
                         FxDB(drkmutama("kmmodifikasiusernama"), ""), sptRow)
            End If
            If kmutama.Length > 0 Then kmutama = kmutama.Substring(0, kmutama.Length - sptRow.Length) Else kmutama = kmutama

            'AMBIL DATA LB
            sql = query.PanggilQuery("m11_lb_getdata")
            Dim dtlb As New DataTable
            dtlb = AmbilData("aplikasi1-m_11_lb~m_11_lb_detail", "lbidkj = '" & idtransaksi & "' AND (lbstatus = 2 OR lbstatus = 3 OR lbstatus = 4)", "lbnotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "lbnotransaksi", sql) ' Ambil data ke databases
            If dtlb.Rows.Count > 0 Then
                For Each drutamalb As DataRow In dtlb.Rows
                    lbutama = String.Concat(lbutama, FxDB(drutamalb("lbid"), 0), sptField,
                          FxDB(drutamalb("lbnotransaksi"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamalb("lbtgl"), ""), formatTgl), sptField,
                          FxDB(drutamalb("lbnoref"), ""), sptField,
                          FxDB(drutamalb("lburaian"), ""), sptField,
                          FxDB(drutamalb("lbtotaltransaksi"), 0), sptField,
                          FxDB(drutamalb("lbcatatan"), ""), sptField,
                          FxDB(drutamalb("lbinputusernama"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamalb("lbinputtgl"), ""), formatTglWaktu), sptRow)
                Next
            End If
            'For Each dr As DataRow In dtlb.Rows
            '    lbutama = String.Concat(lbutama, FxDB(dr("lbid"), 0), sptField,
            '         FxDB(dr("lbnotransaksi"), ""), sptField,
            '         FxDB(dr("lbtotaltransaksi"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("lbinputtgl"), ""), formatTglWaktu), sptField,
            '         AsFormatTanggal(FxDB(dr("lbmodifikasitgl"), ""), formatTglWaktu), sptField,
            '         FxDB(dr("lbinputusernama"), ""), sptField,
            '         FxDB(dr("lbmodifikasiusernama"), ""), sptField,
            '         FxDB(dr("idlbdetail"), 0), sptField,
            '         FxDB(dr("idlb"), 0), sptField,
            '         FxDB(dr("jenis"), ""), sptField,
            '         FxDB(dr("idlayanan"), 0), sptField,
            '         FxDB(dr("namalayanan"), ""), sptField,
            '         FxDB(dr("jml"), 0), sptField,
            '         FxDB(dr("satuan"), ""), sptField,
            '         FxDB(dr("nilaisatuan"), 0), sptField,
            '         FxDB(dr("jmltotal"), 0), sptField,
            '         FxDB(dr("satuandefault"), ""), sptField,
            '         FxDB(dr("harga"), 0), sptField,
            '         FxDB(dr("diskon"), ""), sptField,
            '         FxDB(dr("jmldiskon"), 0), sptField,
            '         FxDB(dr("pajak1"), ""), sptField,
            '         FxDB(dr("jmlpajak1"), 0), sptField,
            '         FxDB(dr("pajak2"), ""), sptField,
            '         FxDB(dr("jmlpajak2"), 0), sptField,
            '         FxDB(dr("cabang"), ""), sptField,
            '         FxDB(dr("lokasi"), ""), sptField,
            '         FxDB(dr("gudang"), ""), sptField,
            '         FxDB(dr("costcenter"), ""), sptField,
            '         FxDB(dr("divisi"), ""), sptField,
            '         FxDB(dr("subdivisi"), ""), sptField,
            '         FxDB(dr("proyek"), ""), sptField,
            '         FxDB(dr("catatan"), ""), sptField,
            '         FxDB(dr("urutan"), 0), sptField,
            '         FxDB(dr("idkjdetail"), 0), sptField,
            '         FxDB(dr("jmlrealisasi"), 0), sptField,
            '         FxDB(dr("statusrealisasi"), 0), sptField,
            '         FxDB(dr("isclose"), 0), sptField,
            '         FxDB(dr("iddokter"), 0), sptField,
            '         FxDB(dr("namadokter"), ""), sptField,
            '         FxDB(dr("customtext1"), ""), sptField,
            '         FxDB(dr("customtext2"), ""), sptField,
            '         FxDB(dr("customtext3"), ""), sptField,
            '         FxDB(dr("customtext4"), ""), sptField,
            '         FxDB(dr("customtext5"), ""), sptField,
            '         FxDB(dr("customtext6"), ""), sptField,
            '         FxDB(dr("customtext7"), ""), sptField,
            '         FxDB(dr("customtext8"), ""), sptField,
            '         FxDB(dr("customtext9"), ""), sptField,
            '         FxDB(dr("customtext10"), ""), sptField,
            '         FxDB(dr("customtext11"), ""), sptField,
            '         FxDB(dr("customtext12"), ""), sptField,
            '         FxDB(dr("customtext13"), ""), sptField,
            '         FxDB(dr("customtext14"), ""), sptField,
            '         FxDB(dr("customtext15"), ""), sptField,
            '         FxDB(dr("customtext16"), ""), sptField,
            '         FxDB(dr("customtext17"), ""), sptField,
            '         FxDB(dr("customtext18"), ""), sptField,
            '         FxDB(dr("customtext19"), ""), sptField,
            '         FxDB(dr("customtext20"), ""), sptField,
            '         FxDB(dr("customdbl1"), 0), sptField,
            '         FxDB(dr("customdbl2"), 0), sptField,
            '         FxDB(dr("customdbl3"), 0), sptField,
            '         FxDB(dr("customdbl4"), 0), sptField,
            '         FxDB(dr("customdbl5"), 0), sptField,
            '         FxDB(dr("customdbl6"), 0), sptField,
            '         FxDB(dr("customdbl7"), 0), sptField,
            '         FxDB(dr("customdbl8"), 0), sptField,
            '         FxDB(dr("customdbl9"), 0), sptField,
            '         FxDB(dr("customdbl10"), 0), sptField,
            '         FxDB(dr("customdbl11"), 0), sptField,
            '         FxDB(dr("customdbl12"), 0), sptField,
            '         FxDB(dr("customdbl13"), 0), sptField,
            '         FxDB(dr("customdbl14"), 0), sptField,
            '         FxDB(dr("customdbl15"), 0), sptField,
            '         FxDB(dr("customdbl16"), 0), sptField,
            '         FxDB(dr("customdbl17"), 0), sptField,
            '         FxDB(dr("customdbl18"), 0), sptField,
            '         FxDB(dr("customdbl19"), 0), sptField,
            '         FxDB(dr("customdbl20"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
            '         FxDB(dr("kodelayanan"), ""), sptField,
            '         FxDB(dr("pajak1nama"), ""), sptField,
            '         FxDB(dr("pajak1nilai"), 0), sptField,
            '         FxDB(dr("pajak2nama"), ""), sptField,
            '         FxDB(dr("pajak2nilai"), 0), sptField,
            '         FxDB(dr("cabangnama"), ""), sptField,
            '         FxDB(dr("lokasinama"), ""), sptField,
            '         FxDB(dr("gudangnama"), ""), sptField,
            '         FxDB(dr("costcenternama"), ""), sptField,
            '         FxDB(dr("divisinama"), ""), sptField,
            '         FxDB(dr("subdivisinama"), ""), sptField,
            '         FxDB(dr("proyeknama"), ""), sptField,
            '         FxDB(dr("kjnotransaksi"), ""), sptField,
            '         FxDB(dr("kodedokter"), ""), sptRow)
            'Next
            If lbutama.Length > 0 Then lbutama = lbutama.Substring(0, lbutama.Length - sptRow.Length) Else lbutama = lbutama

            'Ambil Data RK
            sql = query.PanggilQuery("m11_rk_getdata")
            Dim dtrk As New DataTable
            dtrk = AmbilData("aplikasi1-m_11_rk~m_11_rk_detail", "rkidkj = '" & idtransaksi & "' AND (rkstatus = 2 OR rkstatus = 3 OR rkstatus = 4)", "rknotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "rknotransaksi", sql) ' Ambil data ke databases
            For Each dr As DataRow In dtrk.Rows
                rkutama = String.Concat(rkutama, FxDB(dr("rkid"), 0), sptField,
                         FxDB(dr("rkautonotransaksi"), 0), sptField,
                         FxDB(dr("rknotransaksi"), ""), sptField,
                         FxDB(dr("rkmatauang"), ""), sptField,
                         FxDB(dr("rkkurs"), 0), sptField,
                         FxDB(dr("rkjumlah"), 0), sptField,
                         FxDB(dr("rkjumlahvalas"), 0), sptField,
                         FxDB(dr("rkjumlahbayar"), 0), sptField,
                         FxDB(dr("rkjumlahbayarvalas"), 0), sptField,
                         FxDB(dr("rkinputuser"), 0), sptField,
                         AsFormatTanggal(FxDB(dr("rkinputtgl"), ""), formatTglWaktu), sptField,
                         FxDB(dr("rkmodifikasiuser"), 0), sptField,
                         AsFormatTanggal(FxDB(dr("rkmodifikasitgl"), ""), formatTglWaktu), sptField,
                         FxDB(dr("rkidkj"), 0), sptField,
                         FxDB(dr("rkinputusernama"), ""), sptField,
                         FxDB(dr("rkmodifikasiusernama"), ""), sptField,
                         FxDB(dr("rknotransaksikj"), ""), sptField,
                         FxDB(dr("rknamakj"), ""), sptField,
                         FxDB(dr("idrkcarabayar"), 0), sptField,
                         FxDB(dr("idrk"), 0), sptField,
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
                         FxDB(dr("rekgironama"), ""), sptField,
                         FxDB(dr("rkkategori"), 0), sptRow)
            Next
            If rkutama.Length > 0 Then rkutama = rkutama.Substring(0, rkutama.Length - sptRow.Length) Else rkutama = rkutama

            'AMBIL DATA RO
            sql = query.PanggilQuery("m11_ro_getdata")
            Dim dtro As New DataTable
            dtro = AmbilData("aplikasi1-m_11_ro~m_11_ro_detail", "roidkj = '" & idtransaksi & "' AND (rostatus = 2 OR rostatus = 3 OR rostatus = 4)", "ronotransaksi ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "ronotransaksi", sql) ' Ambil data ke databases
            If dtro.Rows.Count > 0 Then
                For Each drutamaro As DataRow In dtro.Rows
                    rodetail = String.Concat(rodetail, FxDB(drutamaro("roid"), 0), sptField,
                          FxDB(drutamaro("ronotransaksi"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamaro("rotgl"), ""), formatTgl), sptField,
                          FxDB(drutamaro("rouraian"), ""), sptField,
                          FxDB(drutamaro("rototaltransaksi"), 0), sptField,
                          FxDB(drutamaro("rocatatan"), ""), sptField,
                          FxDB(drutamaro("roinputusernama"), ""), sptField,
                          AsFormatTanggal(FxDB(drutamaro("roinputtgl"), ""), formatTglWaktu), sptRow)
                Next
            End If
            'For Each dr As DataRow In dtro.Rows
            '    rodetail = String.Concat(rodetail, FxDB(dr("roid"), 0), sptField,
            '         FxDB(dr("ronotransaksi"), ""), sptField,
            '         FxDB(dr("rototaltransaksi"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("roinputtgl"), ""), formatTglWaktu), sptField,
            '         AsFormatTanggal(FxDB(dr("romodifikasitgl"), ""), formatTglWaktu), sptField,
            '         FxDB(dr("roinputusernama"), ""), sptField,
            '         FxDB(dr("romodifikasiusernama"), ""), sptField,
            '         FxDB(dr("idrodetail"), 0), sptField,
            '         FxDB(dr("idro"), 0), sptField,
            '         FxDB(dr("jenis"), ""), sptField,
            '         FxDB(dr("idlayanan"), 0), sptField,
            '         FxDB(dr("namalayanan"), ""), sptField,
            '         FxDB(dr("jml"), 0), sptField,
            '         FxDB(dr("satuan"), ""), sptField,
            '         FxDB(dr("nilaisatuan"), 0), sptField,
            '         FxDB(dr("jmltotal"), 0), sptField,
            '         FxDB(dr("satuandefault"), ""), sptField,
            '         FxDB(dr("harga"), 0), sptField,
            '         FxDB(dr("diskon"), ""), sptField,
            '         FxDB(dr("jmldiskon"), 0), sptField,
            '         FxDB(dr("pajak1"), ""), sptField,
            '         FxDB(dr("jmlpajak1"), 0), sptField,
            '         FxDB(dr("pajak2"), ""), sptField,
            '         FxDB(dr("jmlpajak2"), 0), sptField,
            '         FxDB(dr("cabang"), ""), sptField,
            '         FxDB(dr("lokasi"), ""), sptField,
            '         FxDB(dr("gudang"), ""), sptField,
            '         FxDB(dr("costcenter"), ""), sptField,
            '         FxDB(dr("divisi"), ""), sptField,
            '         FxDB(dr("subdivisi"), ""), sptField,
            '         FxDB(dr("proyek"), ""), sptField,
            '         FxDB(dr("catatan"), ""), sptField,
            '         FxDB(dr("urutan"), 0), sptField,
            '         FxDB(dr("idkjdetail"), 0), sptField,
            '         FxDB(dr("jmlrealisasi"), 0), sptField,
            '         FxDB(dr("statusrealisasi"), 0), sptField,
            '         FxDB(dr("isclose"), 0), sptField,
            '         FxDB(dr("iddokter"), 0), sptField,
            '         FxDB(dr("namadokter"), ""), sptField,
            '         FxDB(dr("customtext1"), ""), sptField,
            '         FxDB(dr("customtext2"), ""), sptField,
            '         FxDB(dr("customtext3"), ""), sptField,
            '         FxDB(dr("customtext4"), ""), sptField,
            '         FxDB(dr("customtext5"), ""), sptField,
            '         FxDB(dr("customtext6"), ""), sptField,
            '         FxDB(dr("customtext7"), ""), sptField,
            '         FxDB(dr("customtext8"), ""), sptField,
            '         FxDB(dr("customtext9"), ""), sptField,
            '         FxDB(dr("customtext10"), ""), sptField,
            '         FxDB(dr("customtext11"), ""), sptField,
            '         FxDB(dr("customtext12"), ""), sptField,
            '         FxDB(dr("customtext13"), ""), sptField,
            '         FxDB(dr("customtext14"), ""), sptField,
            '         FxDB(dr("customtext15"), ""), sptField,
            '         FxDB(dr("customtext16"), ""), sptField,
            '         FxDB(dr("customtext17"), ""), sptField,
            '         FxDB(dr("customtext18"), ""), sptField,
            '         FxDB(dr("customtext19"), ""), sptField,
            '         FxDB(dr("customtext20"), ""), sptField,
            '         FxDB(dr("customdbl1"), 0), sptField,
            '         FxDB(dr("customdbl2"), 0), sptField,
            '         FxDB(dr("customdbl3"), 0), sptField,
            '         FxDB(dr("customdbl4"), 0), sptField,
            '         FxDB(dr("customdbl5"), 0), sptField,
            '         FxDB(dr("customdbl6"), 0), sptField,
            '         FxDB(dr("customdbl7"), 0), sptField,
            '         FxDB(dr("customdbl8"), 0), sptField,
            '         FxDB(dr("customdbl9"), 0), sptField,
            '         FxDB(dr("customdbl10"), 0), sptField,
            '         FxDB(dr("customdbl11"), 0), sptField,
            '         FxDB(dr("customdbl12"), 0), sptField,
            '         FxDB(dr("customdbl13"), 0), sptField,
            '         FxDB(dr("customdbl14"), 0), sptField,
            '         FxDB(dr("customdbl15"), 0), sptField,
            '         FxDB(dr("customdbl16"), 0), sptField,
            '         FxDB(dr("customdbl17"), 0), sptField,
            '         FxDB(dr("customdbl18"), 0), sptField,
            '         FxDB(dr("customdbl19"), 0), sptField,
            '         FxDB(dr("customdbl20"), 0), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate4"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate5"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate6"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate7"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate8"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate9"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate10"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate11"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate12"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate13"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate14"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate15"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate16"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate17"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate18"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate19"), ""), formatTgl), sptField,
            '         AsFormatTanggal(FxDB(dr("customdate20"), ""), formatTgl), sptField,
            '         FxDB(dr("kodelayanan"), ""), sptField,
            '         FxDB(dr("pajak1nama"), ""), sptField,
            '         FxDB(dr("pajak1nilai"), 0), sptField,
            '         FxDB(dr("pajak2nama"), ""), sptField,
            '         FxDB(dr("pajak2nilai"), 0), sptField,
            '         FxDB(dr("cabangnama"), ""), sptField,
            '         FxDB(dr("lokasinama"), ""), sptField,
            '         FxDB(dr("gudangnama"), ""), sptField,
            '         FxDB(dr("costcenternama"), ""), sptField,
            '         FxDB(dr("divisinama"), ""), sptField,
            '         FxDB(dr("subdivisinama"), ""), sptField,
            '         FxDB(dr("proyeknama"), ""), sptField,
            '         FxDB(dr("kjnotransaksi"), ""), sptField,
            '         FxDB(dr("kodedokter"), ""), sptField,
            '         FxDB(dr("matauang"), ""), sptField,
            '         FxDB(dr("kurs"), ""), sptField,
            '         FxDB(dr("rekpersediaan"), ""), sptField,
            '         FxDB(dr("rekhargapokok"), ""), sptField,
            '         FxDB(dr("rekdiskonpenjualan"), ""), sptField,
            '         FxDB(dr("rekpenjualan"), ""), sptField,
            '         FxDB(dr("idhppkhususkeluar"), 0), sptField,
            '         FxDB(dr("hpp"), 0), sptField,
            '         FxDB(dr("gudangtransit"), ""), sptField,
            '         FxDB(dr("gudangtujuan"), ""), sptField,
            '         FxDB(dr("tipebarang"), ""), sptRow)
            'Next
            If rodetail.Length > 0 Then rodetail = rodetail.Substring(0, rodetail.Length - sptRow.Length) Else rodetail = rodetail

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
        'strResultData = String.Concat(utama, sptSubParam, luutama, sptSubParam, ludetail)
        strResultData = String.Concat(utama, sptSubParam, ludetail, sptSubParam, akdetail, sptSubParam, kmutama, sptSubParam, lbutama, sptSubParam, rkutama, sptSubParam, rodetail)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        ''wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5,kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20,kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5,kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15,kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5,kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20,kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20, kjstatusnama, kjstatussebelumnyanama, kjinputusernama, kjmodifikasiusernama, kjcabangnama, kjlokasinama" & sptSubParam & "luid, lucabang, lulokasi, lugudang, lusumber, luautonotransaksi, lunotransaksi, lutgl, lukodepa, lucustomer, lucustomerkontak, luuraian, lucatatan, lunoref, lutglnoref, lutotaltransaksi, luidkj, lustatusrealisasi, lustatus, lustatussebelumnya, lujmlrevisi, lucetakanke, luinputuser, luinputtgl, lumodifikasiuser, lumodifikasitgl, luisclose, lucustomtext1, lucustomtext2, lucustomtext3, lucustomtext4, lucustomtext5, lucustomtext6, lucustomtext7, lucustomtext8, lucustomtext9, lucustomtext10, lucustomtext11, lucustomtext12, lucustomtext13, lucustomtext14, lucustomtext15, lucustomtext16, lucustomtext17, lucustomtext18, lucustomtext19, lucustomtext20, lucustomint1, lucustomint2, lucustomint3, lucustomint4, lucustomint5, lucustomint6, lucustomint7, lucustomint8, lucustomint9, lucustomint10, lucustomint11, lucustomint12, lucustomint13, lucustomint14, lucustomint15, lucustomint16, lucustomint17, lucustomint18, lucustomint19, lucustomint20, lucustomdbl1, lucustomdbl2, lucustomdbl3, lucustomdbl4, lucustomdbl5, lucustomdbl6, lucustomdbl7, lucustomdbl8, lucustomdbl9, lucustomdbl10, lucustomdbl11, lucustomdbl12, lucustomdbl13, lucustomdbl14, lucustomdbl15, lucustomdbl16, lucustomdbl17, lucustomdbl18, lucustomdbl19, lucustomdbl20, lucustomdate1, lucustomdate2, lucustomdate3, lucustomdate4, lucustomdate5, lucustomdate6, lucustomdate7, lucustomdate8, lucustomdate9, lucustomdate10, lucustomdate11, lucustomdate12, lucustomdate13, lucustomdate14, lucustomdate15, lucustomdate16, lucustomdate17, lucustomdate18, lucustomdate19, lucustomdate20, lucabangnama, lulokasinama, lugudangnama,  lucustomerkode, lucustomernama, lunotransaksikj, lustatusnama, lustatussebelumnyanama, luinputusernama, lumodifikasiusernama" & sptSubParam & "idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter"))
        'wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5,kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20,kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5,kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15,kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5,kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20,kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20,kjstatuskamar, kjstatusnama, kjstatussebelumnyanama, kjinputusernama, kjmodifikasiusernama, kjcabangnama, kjlokasinama, kjdirujukolehkode, kjdirujukolehnama, kjditanggungolehkode, kjditanggungolehnama, kjkategoriharga, kjperawatan, kjkategoripasien, kjlayanan, kjkamar, kjdokter, kjdirujukke, kjawalankatpasien, kjkategoripasiennama, kjkamarnama, kjdokternama, kjdirujukkenama, kjlayanannama, kjstatuspasien, kjpetugas, kjpetugaskode, kjdesa, kjkecamatan, kjkotanama, kjprovinsinama, kjnegaranama, kjkecamatannama, kjdesanama, kjpetugasnama" & sptSubParam & "luid, lunotransaksi, lutotaltransaksi, luinputtgl, lumodifikasitgl, luinputusernama, lumodifikasiusernama, idludetail, idlu, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter" & sptSubParam & "akid, aknotransaksi, aktotaltransaksi, akinputtgl, akmodifikasitgl, akinputusernama, akmodifikasiusernama, idakdetail, idak, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, akresep, akracik, akembalase" & sptSubParam & "kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmstatusrealisasi, kmstatus, kmstatussebekmmnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2, kmcustomdate3,  kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebekmmnyanama, kminputusernama, kmmodifikasiusernama" & sptSubParam & "lbid, lbnotransaksi, lbtotaltransaksi, lbinputtgl, lbmodifikasitgl, lbinputusernama, lbmodifikasiusernama, idlbdetail, idlb, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter" & sptSubParam & "rkid, rkautonotransaksi, rknotransaksi, rkmatauang, rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkidkj, rkinputusernama, rkmodifikasiusernama, rknotransaksikj, rknamakj, idrkcarabayar, idrk, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama" & sptSubParam & "roid, ronotransaksi, rototaltransaksi, roinputtgl, romodifikasitgl, roinputusernama, romodifikasiusernama, idrodetail, idro, jenis, idlayanan, namalayanan, jml, satuan, nilaisatuan, jmltotal, satuandefault, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idkjdetail, jmlrealisasi, statusrealisasi, isclose, iddokter, namadokter, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customtext11, customtext12, customtext13, customtext14, customtext15, customtext16, customtext17, customtext18, customtext19, customtext20, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdbl11, customdbl12, customdbl13, customdbl14, customdbl15, customdbl16, customdbl17, customdbl18, customdbl19, customdbl20, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10, customdate11, customdate12, customdate13, customdate14, customdate15, customdate16, customdate17, customdate18, customdate19, customdate20, kodelayanan, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, kjnotransaksi, kodedokter, matauang, kurs, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekpenjualan, idhppkhususkeluar, hpp, gudangtransit, gudangtujuan, tipebarang"))
        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5,kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20,kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5,kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15,kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5,kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20,kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20,kjstatuskamar, kjstatusnama, kjstatussebelumnyanama, kjinputusernama, kjmodifikasiusernama, kjcabangnama, kjlokasinama, kjdirujukolehkode, kjdirujukolehnama, kjditanggungolehkode, kjditanggungolehnama, kjkategoriharga, kjperawatan, kjkategoripasien, kjlayanan, kjkamar, kjdokter, kjdirujukke, kjawalankatpasien, kjkategoripasiennama, kjkamarnama, kjdokternama, kjdirujukkenama, kjlayanannama, kjstatuspasien, kjpetugas, kjpetugaskode, kjdesa, kjkecamatan, kjkotanama, kjprovinsinama, kjnegaranama, kjkecamatannama, kjdesanama, kjpetugasnama, kjdiagnosa, kjdiagnosanama, kjketerangan" & sptSubParam & "luid, lunotransaksi, lutgl, luuraian, lutotaltransaksi, lucatatan, luinputusernama, luinputtgl" & sptSubParam & "akid, aknotransaksi, aktgl, aknoref, akuraian, aktotaltransaksi, akcatatan, akinputusernama, akinputtgl, aktotalobat, akresep, akracik, akembalase" & sptSubParam & "kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmstatusrealisasi, kmstatus, kmstatussebekmmnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2, kmcustomdate3,  kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebekmmnyanama, kminputusernama, kmmodifikasiusernama" & sptSubParam & "lbid, lbnotransaksi, lbtgl, lbnoref, lburaian, lbtotaltransaksi, lbcatatan, lbinputusernama, lbinputtgl" & sptSubParam & "rkid, rkautonotransaksi, rknotransaksi, rkmatauang, rkkurs, rkjumlah, rkjumlahvalas, rkjumlahbayar, rkjumlahbayarvalas, rkinputuser, rkinputtgl, rkmodifikasiuser, rkmodifikasitgl, rkidkj, rkinputusernama, rkmodifikasiusernama, rknotransaksikj, rknamakj, idrkcarabayar, idrk, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama, rkkategori" & sptSubParam & "roid, ronotransaksi, rotgl, rouraian, rototaltransaksi, rocatatan, roinputusernama, roinputtgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KjSearch(ByVal param As String) As String
        'M11_KjSearch --------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20

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
        sql = query.PanggilQuery("m11_kj_v")
        'result(2) = sql & " WHERE " & Filter & " ORDER BY " & Sorting : GoTo selesai
        dt = AmbilData("aplikasi1-M11_kj_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        'Dim hitung As Int16 = 0
        If dt.Rows.Count > 0 Then

            Dim kjtglkeluar As String = "", kjtglmeninggal As String = "", kjtglmasukkm As String = "", kjtglkeluarkm As String = ""

            For Each dr As DataRow In dt.Rows

                kjtglkeluar = FxDB(dr("kjtglkeluar"), "")
                kjtglmeninggal = FxDB(dr("kjtglmeninggal"), "")
                kjtglmasukkm = FxDB(dr("kjtglmasukkm"), "")
                kjtglkeluarkm = FxDB(dr("kjtglkeluarkm"), "")

                If Len(kjtglkeluar) > 0 Then kjtglkeluar = AsFormatTanggal(FxDB(dr("kjtglkeluar"), ""), formatTgl) Else kjtglkeluar = kjtglkeluar
                If Len(kjtglmeninggal) > 0 Then kjtglmeninggal = AsFormatTanggal(FxDB(dr("kjtglmeninggal"), ""), formatTgl) Else kjtglmeninggal = kjtglmeninggal
                If Len(kjtglmasukkm) > 0 Then kjtglmasukkm = AsFormatTanggal(FxDB(dr("kjtglmasukkm"), ""), formatTgl) Else kjtglmasukkm = kjtglmasukkm
                If Len(kjtglkeluarkm) > 0 Then kjtglkeluarkm = AsFormatTanggal(FxDB(dr("kjtglkeluarkm"), ""), formatTgl) Else kjtglkeluarkm = kjtglkeluarkm

                'hitung = hitung + 1
                search = String.Concat(search,
                     FxDB(dr("kjid"), 0), sptField,
                     FxDB(dr("kjcabang"), ""), sptField,
                     FxDB(dr("kjlokasi"), ""), sptField,
                     FxDB(dr("kjsumber"), ""), sptField,
                     FxDB(dr("kjautonotransaksi"), 0), sptField,
                     FxDB(dr("kjnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kjtgl"), ""), formatTgl), sptField,
                     FxDB(dr("kjkodepa"), 0), sptField,
                     FxDB(dr("kjnopasien"), ""), sptField,
                     FxDB(dr("kjnama"), ""), sptField,
                     FxDB(dr("kjprefix"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kjtgllahir"), ""), formatTgl), sptField,
                     FxDB(dr("kjumur"), 0), sptField,
                     FxDB(dr("kjjeniskelamin"), ""), sptField,
                     FxDB(dr("kjstatusperkawinan"), 0), sptField,
                     FxDB(dr("kjagama"), 0), sptField,
                     FxDB(dr("kjayah"), ""), sptField,
                     FxDB(dr("kjibu"), ""), sptField,
                     FxDB(dr("kjsuamiistri"), ""), sptField,
                     FxDB(dr("kjnotelepon"), ""), sptField,
                     FxDB(dr("kjnofax"), ""), sptField,
                     FxDB(dr("kjnohp"), ""), sptField,
                     FxDB(dr("kjemail"), ""), sptField,
                     FxDB(dr("kjalamat"), ""), sptField,
                     FxDB(dr("kjkota"), ""), sptField,
                     FxDB(dr("kjprovinsi"), ""), sptField,
                     FxDB(dr("kjnegara"), ""), sptField,
                     FxDB(dr("kjkodepos"), ""), sptField,
                     FxDB(dr("kjkeluargalain"), ""), sptField,
                     FxDB(dr("kjnoteleponlain"), ""), sptField,
                     FxDB(dr("kjcatatan"), ""), sptField,
                     kjtglkeluar, sptField,
                     kjtglmeninggal, sptField,
                     FxDB(dr("kjcarakunjungan"), 0), sptField,
                     FxDB(dr("kjdirujukoleh"), 0), sptField,
                     FxDB(dr("kjditanggungoleh"), 0), sptField,
                     FxDB(dr("kjstatusrealisasi"), 0), sptField,
                     FxDB(dr("kjstatus"), 0), sptField,
                     FxDB(dr("kjstatussebelumnya"), 0), sptField,
                     FxDB(dr("kjjmlrevisi"), 0), sptField,
                     FxDB(dr("kjcetakanke"), 0), sptField,
                     FxDB(dr("kjinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kjinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kjmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kjmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kjisclose"), 0), sptField,
                     FxDB(dr("kjcustomtext1"), ""), sptField,
                     FxDB(dr("kjcustomtext2"), ""), sptField,
                     FxDB(dr("kjcustomtext3"), ""), sptField,
                     FxDB(dr("kjcustomtext4"), ""), sptField,
                     FxDB(dr("kjcustomtext5"), ""), sptField,
                     FxDB(dr("kjcustomtext6"), ""), sptField,
                     FxDB(dr("kjcustomtext7"), ""), sptField,
                     FxDB(dr("kjcustomtext8"), ""), sptField,
                     FxDB(dr("kjcustomtext9"), ""), sptField,
                     FxDB(dr("kjcustomtext10"), ""), sptField,
                     FxDB(dr("kjcustomtext11"), ""), sptField,
                     FxDB(dr("kjcustomtext12"), ""), sptField,
                     FxDB(dr("kjcustomtext13"), ""), sptField,
                     FxDB(dr("kjcustomtext14"), ""), sptField,
                     FxDB(dr("kjcustomtext15"), ""), sptField,
                     FxDB(dr("kjcustomtext16"), ""), sptField,
                     FxDB(dr("kjcustomtext17"), ""), sptField,
                     FxDB(dr("kjcustomtext18"), ""), sptField,
                     FxDB(dr("kjcustomtext19"), ""), sptField,
                     FxDB(dr("kjcustomtext20"), ""), sptField,
                     FxDB(dr("kjcustomint1"), 0), sptField,
                     FxDB(dr("kjcustomint2"), 0), sptField,
                     FxDB(dr("kjcustomint3"), 0), sptField,
                     FxDB(dr("kjcustomint4"), 0), sptField,
                     FxDB(dr("kjcustomint5"), 0), sptField,
                     FxDB(dr("kjcustomint6"), 0), sptField,
                     FxDB(dr("kjcustomint7"), 0), sptField,
                     FxDB(dr("kjcustomint8"), 0), sptField,
                     FxDB(dr("kjcustomint9"), 0), sptField,
                     FxDB(dr("kjcustomint10"), 0), sptField,
                     FxDB(dr("kjcustomint11"), 0), sptField,
                     FxDB(dr("kjcustomint12"), 0), sptField,
                     FxDB(dr("kjcustomint13"), 0), sptField,
                     FxDB(dr("kjcustomint14"), 0), sptField,
                     FxDB(dr("kjcustomint15"), 0), sptField,
                     FxDB(dr("kjcustomint16"), 0), sptField,
                     FxDB(dr("kjcustomint17"), 0), sptField,
                     FxDB(dr("kjcustomint18"), 0), sptField,
                     FxDB(dr("kjcustomint19"), 0), sptField,
                     FxDB(dr("kjcustomint20"), 0), sptField,
                     FxDB(dr("kjcustomdbl1"), 0), sptField,
                     FxDB(dr("kjcustomdbl2"), 0), sptField,
                     FxDB(dr("kjcustomdbl3"), 0), sptField,
                     FxDB(dr("kjcustomdbl4"), 0), sptField,
                     FxDB(dr("kjcustomdbl5"), 0), sptField,
                     FxDB(dr("kjcustomdbl6"), 0), sptField,
                     FxDB(dr("kjcustomdbl7"), 0), sptField,
                     FxDB(dr("kjcustomdbl8"), 0), sptField,
                     FxDB(dr("kjcustomdbl9"), 0), sptField,
                     FxDB(dr("kjcustomdbl10"), 0), sptField,
                     FxDB(dr("kjcustomdbl11"), 0), sptField,
                     FxDB(dr("kjcustomdbl12"), 0), sptField,
                     FxDB(dr("kjcustomdbl13"), 0), sptField,
                     FxDB(dr("kjcustomdbl14"), 0), sptField,
                     FxDB(dr("kjcustomdbl15"), 0), sptField,
                     FxDB(dr("kjcustomdbl16"), 0), sptField,
                     FxDB(dr("kjcustomdbl17"), 0), sptField,
                     FxDB(dr("kjcustomdbl18"), 0), sptField,
                     FxDB(dr("kjcustomdbl19"), 0), sptField,
                     FxDB(dr("kjcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kjcustomdate20"), ""), formatTgl), sptField,
                     FxDB(dr("kjstatuskamar"), 0), sptField,
                     FxDB(dr("kjstatusnama"), ""), sptField,
                     FxDB(dr("kjstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("kjinputusernama"), ""), sptField,
                     FxDB(dr("kjmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kjcabangnama"), ""), sptField,
                     FxDB(dr("kjlokasinama"), ""), sptField,
                     FxDB(dr("kjkamarr"), ""), sptField,
                     FxDB(dr("kjkasur"), ""), sptField,
                     kjtglmasukkm, sptField,
                     kjtglkeluarkm, sptField,
                     FxDB(dr("kjkategoriharga"), ""), sptField,
                     FxDB(dr("kjperawatan"), ""), sptField,
                     FxDB(dr("kjkategoripasien"), ""), sptField,
                     FxDB(dr("kjlayanan"), ""), sptField,
                     FxDB(dr("kjkamar"), ""), sptField,
                     FxDB(dr("kjdokter"), ""), sptField,
                     FxDB(dr("kjdirujukke"), ""), sptField,
                     FxDB(dr("kjawalankatpasien"), ""), sptField,
                     FxDB(dr("kjpetugas"), 0), sptField,
                     FxDB(dr("kjpetugaskode"), ""), sptField,
                     FxDB(dr("kjdesa"), ""), sptField,
                     FxDB(dr("kjkecamatan"), ""), sptField,
                     FxDB(dr("kjkamarnama"), ""), sptRow)

            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            'result(2) = search : GoTo selesai
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien, kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5,kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20,kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5,kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15,kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5,kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20,kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20,kjstatuskamar, kjstatusnama, kjstatussebelumnyanama, kjinputusernama, kjmodifikasiusernama, kjcabangnama, kjlokasinama, kjkamarr, kjkasur, kjtglmasukkm, kjtglkeluarkm, kjkategoriharga, kjperawatan, kjkategoripasien, kjlayanan, kjkamar, kjdokter, kjdirujukke, kjawalankatpasien, kjpetugas, kjpetugaskode, kjdesa, kjkecamatan, kjkamarnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KjTerkait(ByVal param As String) As String
        'M5_SoTerkait --------------------------------------------------------
        'soid, sonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "kjid required numeric." : GoTo selesai
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
            'Filter = pagingSplit(2) & " AND kjid=" & idtransaksi
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
            'Else
            '    Filter = "kjid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.m11_kj_terkait(Filter)
        sql = query.PanggilQuery("m11_kj_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_kj_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kjid"), 0), sptField,
                     FxDB(dr("kjnotransaksi"), ""), sptField,
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
            result(2) = "Related KJ data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kjid, kjnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", gudang As String = "", urutan As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        If Len(ftExistOutstanding) > 0 Then 'ftExistOutstanding = rowExists, idsqdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstanding)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SQ" : GoTo selesai
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M11_KjUpdateKeterangan_(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama(), dataRowUtama() As String

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


        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'riid(0) As Integer, ricabang(1) As String, rilokasi(2) As String, rigudang(3) As String, riasalbarang(4) As String, 
        'riasalbarangkategori(5) As Integer, rijenispembelian(6) As String, rijenispembeliankategori(7) As Integer, ricarabayar(8) As Integer, risumber(9) As String, 
        'riautonotransaksi(10) As Integer, rinotransaksi(11) As String, ritgl(12) As Date, rikodepa(13) As Integer, risupplier(14) As Integer, 
        'risupplierkontak(15) As String, ri1alamat1(16) As String, ri1alamat2(17) As String, ri1alamat3(18) As String, ri2alamat1(19) As String, 
        'ri2alamat2(20) As String, ri2alamat3(21) As String, ribagianpembelian(22) As Integer, ritermin(23) As String, ritgljatuhtempo(24) As Date, 
        'riuraian(25) As String, ricatatan(26) As String, rinoref(27) As String, ritglnoref(28) As Date, ritglpenutupan(29) As Date, 
        'rimatauang(30) As String, rikurs(31) As Double, rihargatermasukpajak(32) As Integer, ritotal(33) As Double, ridiskonpersen(34) As String, 
        'rijmldiskon(35) As Double, ritotalpajak1detail(36) As Double, ritotalpajak2detail(37) As Double, ribiayalainpersen(38) As String, ribiayalain(39) As Double, 
        'ritotaltransaksi(40) As Double, rijmlbayar(41) As Double, ristatuslunas(42) As Integer, ritgllunas(43) As Date, rinofakturpajak(44) As String, 
        'risdhbayarpajak(45) As Integer, ritglbayarpajak(46) As Date, rirekdiskon(47) As String, rirekpajak1(48) As String, rirekpajak2(49) As String, 
        'rirekbiayalain(50) As String, rirekbayar(51) As String, riidpr(52) As Integer, riidcs(53) As Integer, riidrq(54) As Integer, 
        'riidbs(55) As Integer, riidpo(56) As Integer, riidipc(57) As Integer, riidgrn(58) As Integer, ristatusdnr(59) As Integer, 
        'ristatusprt(60) As Integer, ristatus(61) As Integer, ristatussebelumnya(62) As Integer, rijmlrevisi(63) As Integer, ricetakanke(64) As Integer, 
        'riinputuser(65) As Integer, riinputtgl(66) As DateTime, rimodifikasiuser(67) As Integer, rimodifikasitgl(68) As DateTime, riposting(69) As Integer, 
        'ritutupperiode(70) As Integer, riisclose(71) As Integer, ricustomtext1(72) As String, ricustomtext2(73) As String, ricustomtext3(74) As String, 
        'ricustomtext4(75) As String, ricustomtext5(76) As String, ricustomint1(77) As Integer, ricustomint2(78) As Integer, ricustomint3(79) As Integer, 
        'ricustomdbl1(80) As Double, ricustomdbl2(81) As Double, ricustomdbl3(82) As Double, ricustomdate1(83) As Date, ricustomdate2(84) As Date, 
        'ricustomdate3(85) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya, 
        'rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, 
        'ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, 
        'ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, 
        'ricustomdate2, ricustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kjid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnopasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprefix", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjumur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjeniskelamin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatusperkawinan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjagama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjayah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjibu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsuamiistri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnotelepon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprovinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnegara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkeluargalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnoteleponlain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglmeninggal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcarakunjungan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdirujukoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjditanggungoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuskamar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjkategoriharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdirujukke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuspasien", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdesa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkecamatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdiagnosa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjketerangan", AsEnumTypeData.AsInt64)


        Dim JmlDt As Integer = dataUtama.Length
        For i = 1 To JmlDt
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Utama -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 141) Then
                result(2) = "Invalid main transaction data parameter. " & dataRowUtama.Length & "" : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Utama ----------------------------

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'kjid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "kjid required numeric." : GoTo selesai
            End If
            'kjautonotransaksi(2) As Integer
            If (IsNumeric(dataRowUtama(4)) = False) Then
                result(2) = "kjautonotransaksi required numeric." : GoTo selesai
            End If
            'kjtgl(4) As Date
            If (IsDate(dataRowUtama(6)) = False) Then
                result(2) = "kjtgl required date." : GoTo selesai
            End If
            'kjkodepa(5) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "kjkodepa required numeric." : GoTo selesai
            End If

            If (IsNumeric(dataRowUtama(12)) = False) Then
                result(2) = "kjumur required numeric." : GoTo selesai
            End If
            'statusperkawinan(7) As Integer
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "kjstatusperkawinan required numeric." : GoTo selesai
            End If
            'agama(8) As Integer
            If (IsNumeric(dataRowUtama(15)) = False) Then
                result(2) = "kjagama required numeric." : GoTo selesai
            End If

            'kjstatusrealisasi(12) As Interger
            If (IsNumeric(dataRowUtama(36)) = False) Then
                result(2) = "kjstatusrealisasi required numeric." : GoTo selesai
            End If
            'kjstatus(13) As Integer
            If (IsNumeric(dataRowUtama(37)) = False) Then
                result(2) = "kjstatus required numeric." : GoTo selesai
            End If
            'kjstatussebelumnya(14) As Integer
            If (IsNumeric(dataRowUtama(38)) = False) Then
                result(2) = "kjstatussebelumnya required numeric." : GoTo selesai
            End If
            'kjjmlrevisi(15) As Integer
            If (IsNumeric(dataRowUtama(39)) = False) Then
                result(2) = "kjjmlrevisi required numeric." : GoTo selesai
            End If
            'kjcetakanke(16) As Integer
            If (IsNumeric(dataRowUtama(40)) = False) Then
                result(2) = "kjcetakanke required numeric." : GoTo selesai
            End If
            'kjinputuser(17) As Integer
            If (IsNumeric(dataRowUtama(41)) = False) Then
                result(2) = "kjinputuser required numeric." : GoTo selesai
            End If
            'kjinputtgl(18) As DateTime
            If (IsDate(dataRowUtama(42)) = False) Then
                result(2) = "kjinputtgl required date." : GoTo selesai
            End If
            'kjmodifikasiuser(19) As Integer
            If (IsNumeric(dataRowUtama(43)) = False) Then
                result(2) = "kjmodifikasiuser required numeric." : GoTo selesai
            End If
            'kjmodifikasitgl(20) As DateTime
            If (IsDate(dataRowUtama(44)) = False) Then
                result(2) = "kjmodifikasitgl required date." : GoTo selesai
            End If
            'kjisclose(21) As Integer
            If (IsNumeric(dataRowUtama(45)) = False) Then
                result(2) = "kjisclose required numeric." : GoTo selesai
            End If
            'kjcustomint1(42) As Integer
            If (IsNumeric(dataRowUtama(66)) = False) Then
                result(2) = "kjcustomint1 required numeric." : GoTo selesai
            End If
            'kjcustomint2(43) As Integer
            If (IsNumeric(dataRowUtama(67)) = False) Then
                result(2) = "kjcustomint2 required numeric." : GoTo selesai
            End If
            'kjcustomint3(44) As Integer
            If (IsNumeric(dataRowUtama(68)) = False) Then
                result(2) = "kjcustomint3 required numeric." : GoTo selesai
            End If
            'kjcustomint4(45) As Integer
            If (IsNumeric(dataRowUtama(69)) = False) Then
                result(2) = "kjcustomint4 required numeric." : GoTo selesai
            End If
            'kjcustomint5(46) As Integer
            If (IsNumeric(dataRowUtama(70)) = False) Then
                result(2) = "kjcustomint5 required numeric." : GoTo selesai
            End If
            'kjcustomint6(47) As Integer
            If (IsNumeric(dataRowUtama(71)) = False) Then
                result(2) = "kjcustomint6 required numeric." : GoTo selesai
            End If
            'kjcustomint7(48) As Integer
            If (IsNumeric(dataRowUtama(72)) = False) Then
                result(2) = "kjcustomint7 required numeric." : GoTo selesai
            End If
            'kjcustomint8(49) As Integer
            If (IsNumeric(dataRowUtama(73)) = False) Then
                result(2) = "kjcustomint8 required numeric." : GoTo selesai
            End If
            'kjcustomint9(50) As Integer
            If (IsNumeric(dataRowUtama(74)) = False) Then
                result(2) = "kjcustomint9 required numeric." : GoTo selesai
            End If
            'kjcustomint10(51) As Integer
            If (IsNumeric(dataRowUtama(75)) = False) Then
                result(2) = "kjcustomint10 required numeric." : GoTo selesai
            End If
            'kjcustomint11(52) As Integer
            If (IsNumeric(dataRowUtama(76)) = False) Then
                result(2) = "kjcustomint11 required numeric." : GoTo selesai
            End If
            'kjcustomint12(53) As Integer
            If (IsNumeric(dataRowUtama(77)) = False) Then
                result(2) = "kjcustomint12 required numeric." : GoTo selesai
            End If
            'kjcustomint13(54) As Integer
            If (IsNumeric(dataRowUtama(78)) = False) Then
                result(2) = "kjcustomint13 required numeric." : GoTo selesai
            End If
            'kjcustomint14(55) As Integer
            If (IsNumeric(dataRowUtama(79)) = False) Then
                result(2) = "kjcustomint14 required numeric." : GoTo selesai
            End If
            'kjcustomint15(56) As Integer
            If (IsNumeric(dataRowUtama(80)) = False) Then
                result(2) = "kjcustomint15 required numeric." : GoTo selesai
            End If
            'kjcustomint16(57) As Integer
            If (IsNumeric(dataRowUtama(81)) = False) Then
                result(2) = "kjcustomint16 required numeric." : GoTo selesai
            End If
            'kjcustomint17(58) As Integer
            If (IsNumeric(dataRowUtama(82)) = False) Then
                result(2) = "kjcustomint17 required numeric." : GoTo selesai
            End If
            'kjcustomint18(59) As Integer
            If (IsNumeric(dataRowUtama(83)) = False) Then
                result(2) = "kjcustomint18 required numeric." : GoTo selesai
            End If
            'kjcustomint19(60) As Integer
            If (IsNumeric(dataRowUtama(84)) = False) Then
                result(2) = "kjcustomint19 required numeric." : GoTo selesai
            End If
            'kjcustomint20(61) As Integer
            If (IsNumeric(dataRowUtama(85)) = False) Then
                result(2) = "kjcustomint20 required numeric." : GoTo selesai
            End If
            'kjcustomdbl1(62) As Double
            If (IsNumeric(dataRowUtama(86)) = False) Then
                result(2) = "kjcustomdbl1 required numeric." : GoTo selesai
            End If
            'kjcustomdbl2(63) As Double
            If (IsNumeric(dataRowUtama(87)) = False) Then
                result(2) = "kjcustomdbl2 required numeric." : GoTo selesai
            End If
            'kjcustomdbl3(64) As Double
            If (IsNumeric(dataRowUtama(88)) = False) Then
                result(2) = "kjcustomdbl3 required numeric." : GoTo selesai
            End If
            'kjcustomdbl4(65) As Double
            If (IsNumeric(dataRowUtama(89)) = False) Then
                result(2) = "kjcustomdbl4 required numeric." : GoTo selesai
            End If
            'kjcustomdbl5(66) As Double
            If (IsNumeric(dataRowUtama(90)) = False) Then
                result(2) = "kjcustomdbl5 required numeric." : GoTo selesai
            End If
            'kjcustomdbl6(67) As Double
            If (IsNumeric(dataRowUtama(91)) = False) Then
                result(2) = "kjcustomdbl6 required numeric." : GoTo selesai
            End If
            'kjcustomdbl7(68) As Double
            If (IsNumeric(dataRowUtama(92)) = False) Then
                result(2) = "kjcustomdbl7 required numeric." : GoTo selesai
            End If
            'kjcustomdbl8(69) As Double
            If (IsNumeric(dataRowUtama(93)) = False) Then
                result(2) = "kjcustomdbl8 required numeric." : GoTo selesai
            End If
            'kjcustomdbl9(70) As Double
            If (IsNumeric(dataRowUtama(94)) = False) Then
                result(2) = "kjcustomdbl9 required numeric." : GoTo selesai
            End If
            'kjcustomdbl10(71) As Double
            If (IsNumeric(dataRowUtama(95)) = False) Then
                result(2) = "kjcustomdbl10 required numeric." : GoTo selesai
            End If
            'kjcustomdbl11(72) As Double
            If (IsNumeric(dataRowUtama(96)) = False) Then
                result(2) = "kjcustomdbl11 required numeric." : GoTo selesai
            End If
            'kjcustomdbl12(73) As Double
            If (IsNumeric(dataRowUtama(97)) = False) Then
                result(2) = "kjcustomdbl12 required numeric." : GoTo selesai
            End If
            'kjcustomdbl13(74) As Double
            If (IsNumeric(dataRowUtama(98)) = False) Then
                result(2) = "kjcustomdbl13 required numeric." : GoTo selesai
            End If
            'kjcustomdbl14(75) As Double
            If (IsNumeric(dataRowUtama(99)) = False) Then
                result(2) = "kjcustomdbl14 required numeric." : GoTo selesai
            End If
            'kjcustomdbl15(76) As Double
            If (IsNumeric(dataRowUtama(100)) = False) Then
                result(2) = "kjcustomdbl15 required numeric." : GoTo selesai
            End If
            'kjcustomdbl16(77) As Double
            If (IsNumeric(dataRowUtama(101)) = False) Then
                result(2) = "kjcustomdbl16 required numeric." : GoTo selesai
            End If
            'kjcustomdbl17(78) As Double
            If (IsNumeric(dataRowUtama(102)) = False) Then
                result(2) = "kjcustomdbl17 required numeric." : GoTo selesai
            End If
            'kjcustomdbl18(79) As Double
            If (IsNumeric(dataRowUtama(103)) = False) Then
                result(2) = "kjcustomdbl18 required numeric." : GoTo selesai
            End If
            'kjcustomdbl19(80) As Double
            If (IsNumeric(dataRowUtama(104)) = False) Then
                result(2) = "kjcustomdbl19 required numeric." : GoTo selesai
            End If
            'kjcustomdbl20(81) As Double
            If (IsNumeric(dataRowUtama(105)) = False) Then
                result(2) = "kjcustomdbl20 required numeric." : GoTo selesai
            End If
            'kjcustomdate1(82) As Date
            If (IsDate(dataRowUtama(106)) = False) Then
                result(2) = "kjcustomdate1 required date." : GoTo selesai
            End If
            'kjcustomdate2(83) As Date
            If (IsDate(dataRowUtama(107)) = False) Then
                result(2) = "kjcustomdate2 required date." : GoTo selesai
            End If
            'kjcustomdate3(84) As Date
            If (IsDate(dataRowUtama(108)) = False) Then
                result(2) = "kjcustomdate3 required date." : GoTo selesai
            End If
            'kjcustomdate4(85) As Date
            If (IsDate(dataRowUtama(109)) = False) Then
                result(2) = "kjcustomdate4 required date." : GoTo selesai
            End If
            'kjcustomdate5(86) As Date
            If (IsDate(dataRowUtama(110)) = False) Then
                result(2) = "kjcustomdate5 required date." : GoTo selesai
            End If
            'kjcustomdate6(87) As Date
            If (IsDate(dataRowUtama(111)) = False) Then
                result(2) = "kjcustomdate6 required date." : GoTo selesai
            End If
            'kjcustomdate7(88) As Date
            If (IsDate(dataRowUtama(112)) = False) Then
                result(2) = "kjcustomdate7 required date." : GoTo selesai
            End If
            'kjcustomdate8(89) As Date
            If (IsDate(dataRowUtama(113)) = False) Then
                result(2) = "kjcustomdate8 required date." : GoTo selesai
            End If
            'kjcustomdate9(90) As Date
            If (IsDate(dataRowUtama(114)) = False) Then
                result(2) = "kjcustomdate9 required date." : GoTo selesai
            End If
            'kjcustomdate10(91) As Date
            If (IsDate(dataRowUtama(115)) = False) Then
                result(2) = "kjcustomdate10 required date." : GoTo selesai
            End If
            'kjcustomdate11(92) As Date
            If (IsDate(dataRowUtama(116)) = False) Then
                result(2) = "kjcustomdate11 required date." : GoTo selesai
            End If
            'kjcustomdate12(93) As Date
            If (IsDate(dataRowUtama(117)) = False) Then
                result(2) = "kjcustomdate12 required date." : GoTo selesai
            End If
            'kjcustomdate13(94) As Date
            If (IsDate(dataRowUtama(118)) = False) Then
                result(2) = "kjcustomdate13 required date." : GoTo selesai
            End If
            'kjcustomdate14(95) As Date
            If (IsDate(dataRowUtama(119)) = False) Then
                result(2) = "kjcustomdate14 required date." : GoTo selesai
            End If
            'kjcustomdate15(96) As Date
            If (IsDate(dataRowUtama(120)) = False) Then
                result(2) = "kjcustomdate15 required date." : GoTo selesai
            End If
            'kjcustomdate16(97) As Date
            If (IsDate(dataRowUtama(121)) = False) Then
                result(2) = "kjcustomdate16 required date." : GoTo selesai
            End If
            'kjcustomdate17(98) As Date
            If (IsDate(dataRowUtama(122)) = False) Then
                result(2) = "kjcustomdate17 required date." : GoTo selesai
            End If
            'kjcustomdate18(99) As Date
            If (IsDate(dataRowUtama(123)) = False) Then
                result(2) = "kjcustomdate18 required date." : GoTo selesai
            End If
            'kjcustomdate19(100) As Date
            If (IsDate(dataRowUtama(124)) = False) Then
                result(2) = "kjcustomdate19 required date." : GoTo selesai
            End If
            'kjcustomdate20(101) As Date
            If (IsDate(dataRowUtama(125)) = False) Then
                result(2) = "kjcustomdate20 required date." : GoTo selesai
            End If
            'If (IsNumeric(dataUtama(127)) = False) Then
            '    result(2) = "kjkategoriharga required numeric." : GoTo selesai
            'End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "kjcabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 10 Then
                result(2) = "kjcabang should not be more than 10 character." : GoTo selesai
            End If

            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "kjlokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 10 Then
                result(2) = "kjlokasi should not be more than 10 character." : GoTo selesai
            End If

            'kjsumber(1) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "kjsumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 10 Then
                result(2) = "kjsumber should not be more than 10 character." : GoTo selesai
            End If

            'kjnotransaksi(3) As String
            If Len(dataRowUtama(5)) = 0 Then
                result(2) = "kjnotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(5)) > 50 Then
                result(2) = "kjnotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'kjtgl(4) As Date
            If Len(dataRowUtama(6)) = 0 Then
                result(2) = "kjtgl can't be empty" : GoTo selesai
            End If

            If Len(dataRowUtama(9)) = 0 Then
                result(2) = "kjnama can't be empty" : GoTo selesai
            End If

            If Len(dataRowUtama(9)) > 100 Then
                result(2) = "kjnama should not be more than 100 character." : GoTo selesai
            End If

            'jml(5) As Double
            If Len(dataRowUtama(11)) = 0 Then
                result(2) = "kjtgllahir can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(11)) > 10 Then
                result(2) = "kjtgllahir should not be more than 10 character." : GoTo selesai
            End If

            'satuan(6) As String
            If Len(dataRowUtama(12)) = 0 Then
                result(2) = "kjumur can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(12)) > 10 Then
                result(2) = "kjumur should not be more than 10 character." : GoTo selesai
            End If
            If Len(dataRowUtama(12)) <= 0 Then
                result(2) = "kjumur can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowUtama(13)) = 0 Then
                result(2) = "kjjeniskelamin can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(13)) > 10 Then
                result(2) = "kjjeniskelamin should not be more than 10 character." : GoTo selesai
            End If

            'kjinputtgl(18) As DateTime
            If Len(dataRowUtama(42)) = 0 Then
                result(2) = "kjinputtgl can't be empty" : GoTo selesai
            End If

            'kjmodifikasitgl(20) As DateTime
            If Len(dataRowUtama(44)) = 0 Then
                result(2) = "kjmodifikasitgl can't be empty" : GoTo selesai
            End If

            'kjcustomdbl1(62) As Double
            If Len(dataRowUtama(86)) = 0 Then
                result(2) = "kjcustomdbl1 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl2(63) As Double
            If Len(dataRowUtama(87)) = 0 Then
                result(2) = "kjcustomdbl2 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl3(64) As Double
            If Len(dataRowUtama(88)) = 0 Then
                result(2) = "kjcustomdbl3 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl4(65) As Double
            If Len(dataRowUtama(89)) = 0 Then
                result(2) = "kjcustomdbl4 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl5(66) As Double
            If Len(dataRowUtama(90)) = 0 Then
                result(2) = "kjcustomdbl5 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl6(67) As Double
            If Len(dataRowUtama(91)) = 0 Then
                result(2) = "kjcustomdbl6 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl7(68) As Double
            If Len(dataRowUtama(92)) = 0 Then
                result(2) = "kjcustomdbl7 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl8(69) As Double
            If Len(dataRowUtama(93)) = 0 Then
                result(2) = "kjcustomdbl8 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl9(70) As Double
            If Len(dataRowUtama(94)) = 0 Then
                result(2) = "kjcustomdbl9 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl10(71) As Double
            If Len(dataRowUtama(95)) = 0 Then
                result(2) = "kjcustomdbl10 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl11(72) As Double
            If Len(dataRowUtama(96)) = 0 Then
                result(2) = "kjcustomdbl11 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl12(73) As Double
            If Len(dataRowUtama(97)) = 0 Then
                result(2) = "kjcustomdbl12 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl13(74) As Double
            If Len(dataRowUtama(98)) = 0 Then
                result(2) = "kjcustomdbl13 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl14(75) As Double
            If Len(dataRowUtama(99)) = 0 Then
                result(2) = "kjcustomdbl14 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl15(76) As Double
            If Len(dataRowUtama(100)) = 0 Then
                result(2) = "kjcustomdbl15 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl16(77) As Double
            If Len(dataRowUtama(101)) = 0 Then
                result(2) = "kjcustomdbl16 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl17(78) As Double
            If Len(dataRowUtama(102)) = 0 Then
                result(2) = "kjcustomdbl17 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl18(79) As Double
            If Len(dataRowUtama(103)) = 0 Then
                result(2) = "kjcustomdbl18 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl19(80) As Double
            If Len(dataRowUtama(104)) = 0 Then
                result(2) = "kjcustomdbl19 can't be empty" : GoTo selesai
            End If

            'kjcustomdbl20(81) As Double
            If Len(dataRowUtama(105)) = 0 Then
                result(2) = "kjcustomdbl20 can't be empty" : GoTo selesai
            End If

            'kjcustomdate1(82) As Date
            If Len(dataRowUtama(106)) = 0 Then
                result(2) = "kjcustomdate1 can't be empty" : GoTo selesai
            End If

            'kjcustomdate2(83) As Date
            If Len(dataRowUtama(107)) = 0 Then
                result(2) = "kjcustomdate2 can't be empty" : GoTo selesai
            End If

            'kjcustomdate3(84) As Date
            If Len(dataRowUtama(108)) = 0 Then
                result(2) = "kjcustomdate3 can't be empty" : GoTo selesai
            End If

            'kjcustomdate4(85) As Date
            If Len(dataRowUtama(109)) = 0 Then
                result(2) = "kjcustomdate4 can't be empty" : GoTo selesai
            End If

            'kjcustomdate5(86) As Date
            If Len(dataRowUtama(110)) = 0 Then
                result(2) = "kjcustomdate5 can't be empty" : GoTo selesai
            End If

            'kjcustomdate6(87) As Date
            If Len(dataRowUtama(111)) = 0 Then
                result(2) = "kjcustomdate6 can't be empty" : GoTo selesai
            End If

            'kjcustomdate7(88) As Date
            If Len(dataRowUtama(112)) = 0 Then
                result(2) = "kjcustomdate7 can't be empty" : GoTo selesai
            End If

            'kjcustomdate8(89) As Date
            If Len(dataRowUtama(113)) = 0 Then
                result(2) = "kjcustomdate8 can't be empty" : GoTo selesai
            End If

            'kjcustomdate9(90) As Date
            If Len(dataRowUtama(114)) = 0 Then
                result(2) = "kjcustomdate9 can't be empty" : GoTo selesai
            End If

            'kjcustomdate10(91) As Date
            If Len(dataRowUtama(115)) = 0 Then
                result(2) = "kjcustomdate10 can't be empty" : GoTo selesai
            End If

            'kjcustomdate11(92) As Date
            If Len(dataRowUtama(116)) = 0 Then
                result(2) = "kjcustomdate11 can't be empty" : GoTo selesai
            End If

            'kjcustomdate12(93) As Date
            If Len(dataRowUtama(117)) = 0 Then
                result(2) = "kjcustomdate12 can't be empty" : GoTo selesai
            End If

            'kjcustomdate13(94) As Date
            If Len(dataRowUtama(118)) = 0 Then
                result(2) = "kjcustomdate13 can't be empty" : GoTo selesai
            End If

            'kjcustomdate14(95) As Date
            If Len(dataRowUtama(119)) = 0 Then
                result(2) = "kjcustomdate14 can't be empty" : GoTo selesai
            End If

            'kjcustomdate15(96) As Date
            If Len(dataRowUtama(120)) = 0 Then
                result(2) = "kjcustomdate15 can't be empty" : GoTo selesai
            End If

            'kjcustomdate16(97) As Date
            If Len(dataRowUtama(121)) = 0 Then
                result(2) = "kjcustomdate16 can't be empty" : GoTo selesai
            End If

            'kjcustomdate17(98) As Date
            If Len(dataRowUtama(122)) = 0 Then
                result(2) = "kjcustomdate17 can't be empty" : GoTo selesai
            End If

            'kjcustomdate18(99) As Date
            If Len(dataRowUtama(123)) = 0 Then
                result(2) = "kjcustomdate18 can't be empty" : GoTo selesai
            End If

            'kjcustomdate19(100) As Date
            If Len(dataRowUtama(124)) = 0 Then
                result(2) = "kjcustomdate19 can't be empty" : GoTo selesai
            End If

            'kjcustomdate20(101) As Date
            If Len(dataRowUtama(125)) = 0 Then
                result(2) = "kjcustomdate20 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================

            If AsDataTableTambahData(dtutama, "kjid~kjcabang~kjlokasi~kjsumber~kjautonotransaksi~kjnotransaksi~kjtgl~kjkodepa~kjnopasien~kjnama~kjprefix~kjtgllahir~kjumur~kjjeniskelamin~kjstatusperkawinan~kjagama~kjayah~kjibu~kjsuamiistri~kjnotelepon~kjnofax~kjnohp~kjemail~kjalamat~kjkota~kjprovinsi~kjnegara~kjkodepos~kjkeluargalain~kjnoteleponlain~kjcatatan~kjtglkeluar~kjtglmeninggal~kjcarakunjungan~kjdirujukoleh~kjditanggungoleh~kjstatusrealisasi~kjstatus~kjstatussebelumnya~kjjmlrevisi~kjcetakanke~kjinputuser~kjinputtgl~kjmodifikasiuser~kjmodifikasitgl~kjisclose~kjcustomtext1~kjcustomtext2~kjcustomtext3~kjcustomtext4~kjcustomtext5~kjcustomtext6~kjcustomtext7~kjcustomtext8~kjcustomtext9~kjcustomtext10~kjcustomtext11~kjcustomtext12~kjcustomtext13~kjcustomtext14~kjcustomtext15~kjcustomtext16~kjcustomtext17~kjcustomtext18~kjcustomtext19~kjcustomtext20~kjcustomint1~kjcustomint2~kjcustomint3~kjcustomint4~kjcustomint5~kjcustomint6~kjcustomint7~kjcustomint8~kjcustomint9~kjcustomint10~kjcustomint11~kjcustomint12~kjcustomint13~kjcustomint14~kjcustomint15~kjcustomint16~kjcustomint17~kjcustomint18~kjcustomint19~kjcustomint20~kjcustomdbl1~kjcustomdbl2~kjcustomdbl3~kjcustomdbl4~kjcustomdbl5~kjcustomdbl6~kjcustomdbl7~kjcustomdbl8~kjcustomdbl9~kjcustomdbl10~kjcustomdbl11~kjcustomdbl12~kjcustomdbl13~kjcustomdbl14~kjcustomdbl15~kjcustomdbl16~kjcustomdbl17~kjcustomdbl18~kjcustomdbl19~kjcustomdbl20~kjcustomdate1~kjcustomdate2~kjcustomdate3~kjcustomdate4~kjcustomdate5~kjcustomdate6~kjcustomdate7~kjcustomdate8~kjcustomdate9~kjcustomdate10~kjcustomdate11~kjcustomdate12~kjcustomdate13~kjcustomdate14~kjcustomdate15~kjcustomdate16~kjcustomdate17~kjcustomdate18~kjcustomdate19~kjcustomdate20~kjstatuskamar~kjkategoriharga~kjperawatan~kjkategoripasien~kjlayanan~kjkamar~kjdokter~kjdirujukke~kjawalankatpasien~kjstatuspasien~kjpetugas~kjdesa~kjkecamatan~kjdiagnosa~kjketerangan", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122) & "~" & dataUtama(123) & "~" & dataUtama(124) & "~" & dataUtama(125) & "~" & dataUtama(126) & "~" & dataUtama(127) & "~" & dataUtama(128) & "~" & dataUtama(129) & "~" & dataUtama(130) & "~" & dataUtama(131) & "~" & dataUtama(132) & "~" & dataUtama(133) & "~" & dataUtama(134) & "~" & dataUtama(135) & "~" & dataUtama(136) & "~" & dataUtama(137) & "~" & dataUtama(138) & "~" & dataUtama(139) & "~" & dataUtama(140)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Next


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
                For Each drutama As DataRow In dtutama.Rows

                    'CEK PERIODE AKUNTANSI ==================================
                    Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                    Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("kjtgl")), AsFormatTanggal(drutama("kjtgl")))
                    arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                    If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                    'END OF CEK PERIODE AKUNTANSI ===========================


                    ''SET TGL JATUH TEMPO ====================================
                    'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                    'rsTglJT = F_TglJT(drutama("ritermin").ToString, AsFormatTanggal(drutama("ritgl")), "ritgl").Split(sptSubParam)
                    'If rsTglJT(0) = 0 Then
                    '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                    'Else
                    '    drutama("ritgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                    'End If
                    ''END OF SET TGL JATUH TEMPO =============================


                    If isUpdate Then
                        result(4) = drutama("kjid")
                        notransaksi = drutama("kjnotransaksi")
                        'JIKA UPDATE CEK JML ROW PADA DATABASE
                        dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kjid), kjnotransaksi FROM m_11_kj WHERE kjid='" & result(4) & "'")
                        rowUpdate = dtupdate.Rows(0)(0)

                        If (rowUpdate > 0) Then


                            'SIMPAN HISTORY ========================
                            'Dim SimpanHistory As New m4_ri_history
                            'Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                            'If (rsSplitResult(1) = 0) Then
                            'result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                            ' End If
                            'END OF SIMPAN HISTORY ==================

                            sql = "Update M_11_kj set kjketerangan  = " & drutama("kjketerangan") & " where kjid =  '" & drutama("kjid") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "Update M_11_lb set lbketerangan  = " & drutama("kjketerangan") & " where lbidkj =  '" & drutama("kjid") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            sql = "Update M_11_ak set akketerangan  = " & drutama("kjketerangan") & " where akidkj =  '" & drutama("kjid") & "'"
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

                    End If


                    'INSERT MSMQ JURNAL =================================================================
                    Dim sumber As String = "KJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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

                Next

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
    Public Function M11_KjUpdateKeterangan(ByVal param As String) As String
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
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'kjid(0) As Integer, kjcabang(1) As String, kjlokasi(2) As String, kjsumber(3) As String, kjautonotransaksi(4) As Integer, kjnotransaksi(5) As String, kjtgl(6) As Date, kjkodepa(7) As Integer, kjnopasien(8) As String, 
        'kjnama(9) As String, kjprefix(10) As String, kjtgllahir(11) As Date, kjumur(12) As Integer, kjjeniskelamin(13) As String, kjstatusperkawinan(14) As Integer,
        'kjagama(15) As Integer, kjayah(16) As String, kjibu(17) As String, kjsuamiistri(18) As String, kjnotelepon(19) As String, kjnofax(20) As String,
        'kjnohp(21) As String, kjemail(22) As String, kjalamat(23) As String, kjkota(24) As String, kjprovinsi(25) As String, kjnegara(26) As String, 
        'kjkodepos(27) As String, kjkeluargalain(28) As String, kjnoteleponlain(29) As String, kjcatatan(30) As String,
        'kjtglkeluar(31) As Date, kjtglmeninggal(32) As Date, kjcarakunjungan(33) As Integer, kjdirujukoleh(34) As Integer, kjditanggungoleh(35) As Integer, 
        'kjstatusrealisasi(36) As Interger, kjstatus(37) As Integer, kjstatussebelumnya(38) As Integer, kjjmlrevisi(39) As Integer, kjcetakanke(40) As Integer, 
        'kjinputuser(41) As Integer, kjinputtgl(42) As DateTime, kjmodifikasiuser(43) As Integer, kjmodifikasitgl(44) As DateTime, kjisclose(45) As Integer, 
        'kjcustomtext1(46) As String, kjcustomtext2(47) As String, kjcustomtext3(48) As String, kjcustomtext4(49) As String, kjcustomtext5(50) As String, 
        'kjcustomtext6(51) As String, kjcustomtext7(52) As String, kjcustomtext8(53) As String, kjcustomtext9(54) As String, kjcustomtext10(55) As String,
        'kjcustomtext11(56) As String, kjcustomtext12(57) As String, kjcustomtext13(58) As String, kjcustomtext14(59) As String, kjcustomtext15(60) As String, 
        'kjcustomtext16(61) As String, kjcustomtext17(62) As String, kjcustomtext18(63) As String, kjcustomtext19(64) As String, kjcustomtext20(65) As String, 
        'kjcustomint1(66) As Integer, kjcustomint2(67) As Integer, kjcustomint3(68) As Integer, kjcustomint4(69) As Integer, kjcustomint5(70) As Integer, 
        'kjcustomint6(71) As Integer, kjcustomint7(72) As Integer, kjcustomint8(73) As Integer, kjcustomint9(74) As Integer, kjcustomint10(75) As Integer, 
        'kjcustomint11(76) As Integer, kjcustomint12(77) As Integer, kjcustomint13(78) As Integer, kjcustomint14(79) As Integer, kjcustomint15(80) As Integer, 
        'kjcustomint16(81) As Integer, kjcustomint17(82) As Integer, kjcustomint18(83) As Integer, kjcustomint19(84) As Integer, kjcustomint20(85) As Integer,
        'kjcustomdbl1(86) As Double, kjcustomdbl2(87) As Double, kjcustomdbl3(88) As Double, kjcustomdbl4(89) As Double, kjcustomdbl5(90) As Double, 
        'kjcustomdbl6(91) As Double, kjcustomdbl7(92) As Double, kjcustomdbl8(93) As Double, kjcustomdbl9(94) As Double, kjcustomdbl10(95) As Double, 
        'kjcustomdbl11(96) As Double, kjcustomdbl12(97) As Double, kjcustomdbl13(98) As Double, kjcustomdbl14(99) As Double, kjcustomdbl15(100) As Double, 
        'kjcustomdbl16(101) As Double, kjcustomdbl17(102) As Double, kjcustomdbl18(103) As Double, kjcustomdbl19(104) As Double, kjcustomdbl20(105) As Double, 
        'kjcustomdate1(106) As Date, kjcustomdate2(107) As Date, kjcustomdate3(108) As Date, kjcustomdate4(109) As Date, kjcustomdate5(110) As Date,
        'kjcustomdate6(111) As Date, kjcustomdate7(112) As Date, kjcustomdate8(113) As Date, kjcustomdate9(114) As Date, kjcustomdate10(115) As Date,
        'kjcustomdate11(116) As Date, kjcustomdate12(117) As Date, kjcustomdate13(118) As Date, kjcustomdate14(119) As Date, kjcustomdate15(120) As Date,
        'kjcustomdate16(121) As Date, kjcustomdate17(122) As Date, kjcustomdate18(123) As Date, kjcustomdate19(124) As Date, kjcustomdate20(125) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kjid, kjcabang, kjlokasi, kjsumber, kjautonotransaksi, kjnotransaksi, kjtgl, kjkodepa, kjnopasien,
        'kjnama, kjprefix, kjtgllahir, kjumur, kjjeniskelamin, kjstatusperkawinan, kjagama, kjayah, 
        'kjibu, kjsuamiistri, kjnotelepon, kjnofax, kjnohp, kjemail, kjalamat, kjkota, kjprovinsi, kjnegara, 
        'kjkodepos, kjkeluargalain, kjnoteleponlain, kjcatatan, 
        'kjtglkeluar, kjtglmeninggal, kjcarakunjungan, kjdirujukoleh, kjditanggungoleh, 
        'kjstatusrealisasi, kjstatus, kjstatussebelumnya, kjjmlrevisi, kjcetakanke, 
        'kjinputuser, kjinputtgl, kjmodifikasiuser, kjmodifikasitgl, kjisclose, 
        'kjcustomtext1, kjcustomtext2, kjcustomtext3, kjcustomtext4, kjcustomtext5, 
        'kjcustomtext6, kjcustomtext7, kjcustomtext8, kjcustomtext9, kjcustomtext10,
        'kjcustomtext11, kjcustomtext12, kjcustomtext13, kjcustomtext14, kjcustomtext15, 
        'kjcustomtext16, kjcustomtext17, kjcustomtext18, kjcustomtext19, kjcustomtext20, 
        'kjcustomint1, kjcustomint2, kjcustomint3, kjcustomint4, kjcustomint5, 
        'kjcustomint6, kjcustomint7, kjcustomint8, kjcustomint9, kjcustomint10, 
        'kjcustomint11, kjcustomint12, kjcustomint13, kjcustomint14, kjcustomint15, 
        'kjcustomint16, kjcustomint17, kjcustomint18, kjcustomint19, kjcustomint20,
        'kjcustomdbl1, kjcustomdbl2, kjcustomdbl3, kjcustomdbl4, kjcustomdbl5, 
        'kjcustomdbl6, kjcustomdbl7, kjcustomdbl8, kjcustomdbl9, kjcustomdbl10, 
        'kjcustomdbl11, kjcustomdbl12, kjcustomdbl13, kjcustomdbl14, kjcustomdbl15, 
        'kjcustomdbl16, kjcustomdbl17, kjcustomdbl18, kjcustomdbl19, kjcustomdbl20, 
        'kjcustomdate1, kjcustomdate2, kjcustomdate3, kjcustomdate4, kjcustomdate5,
        'kjcustomdate6, kjcustomdate7, kjcustomdate8, kjcustomdate9, kjcustomdate10,
        'kjcustomdate11, kjcustomdate12, kjcustomdate13, kjcustomdate14, kjcustomdate15,
        'kjcustomdate16, kjcustomdate17, kjcustomdate18, kjcustomdate19, kjcustomdate20


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 141) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kjid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kjid required numeric." : GoTo selesai
        End If
        'kjautonotransaksi(2) As Integer
        If (IsNumeric(dataUtama(4)) = False) Then
            result(2) = "kjautonotransaksi required numeric." : GoTo selesai
        End If
        'kjtgl(4) As Date
        If (IsDate(dataUtama(6)) = False) Then
            result(2) = "kjtgl required date." : GoTo selesai
        End If
        'kjkodepa(5) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "kjkodepa required numeric." : GoTo selesai
        End If

        If (IsNumeric(dataUtama(12)) = False) Then
            result(2) = "kjumur required numeric." : GoTo selesai
        End If
        'statusperkawinan(7) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "kjstatusperkawinan required numeric." : GoTo selesai
        End If
        'agama(8) As Integer
        If (IsNumeric(dataUtama(15)) = False) Then
            result(2) = "kjagama required numeric." : GoTo selesai
        End If

        'kjstatusrealisasi(12) As Interger
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "kjstatusrealisasi required numeric." : GoTo selesai
        End If
        'kjstatus(13) As Integer
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "kjstatus required numeric." : GoTo selesai
        End If
        'kjstatussebelumnya(14) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "kjstatussebelumnya required numeric." : GoTo selesai
        End If
        'kjjmlrevisi(15) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "kjjmlrevisi required numeric." : GoTo selesai
        End If
        'kjcetakanke(16) As Integer
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "kjcetakanke required numeric." : GoTo selesai
        End If
        'kjinputuser(17) As Integer
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "kjinputuser required numeric." : GoTo selesai
        End If
        'kjinputtgl(18) As DateTime
        If (IsDate(dataUtama(42)) = False) Then
            result(2) = "kjinputtgl required date." : GoTo selesai
        End If
        'kjmodifikasiuser(19) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "kjmodifikasiuser required numeric." : GoTo selesai
        End If
        'kjmodifikasitgl(20) As DateTime
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "kjmodifikasitgl required date." : GoTo selesai
        End If
        'kjisclose(21) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "kjisclose required numeric." : GoTo selesai
        End If
        'kjcustomint1(42) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "kjcustomint1 required numeric." : GoTo selesai
        End If
        'kjcustomint2(43) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "kjcustomint2 required numeric." : GoTo selesai
        End If
        'kjcustomint3(44) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "kjcustomint3 required numeric." : GoTo selesai
        End If
        'kjcustomint4(45) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "kjcustomint4 required numeric." : GoTo selesai
        End If
        'kjcustomint5(46) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "kjcustomint5 required numeric." : GoTo selesai
        End If
        'kjcustomint6(47) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "kjcustomint6 required numeric." : GoTo selesai
        End If
        'kjcustomint7(48) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "kjcustomint7 required numeric." : GoTo selesai
        End If
        'kjcustomint8(49) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "kjcustomint8 required numeric." : GoTo selesai
        End If
        'kjcustomint9(50) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "kjcustomint9 required numeric." : GoTo selesai
        End If
        'kjcustomint10(51) As Integer
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "kjcustomint10 required numeric." : GoTo selesai
        End If
        'kjcustomint11(52) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "kjcustomint11 required numeric." : GoTo selesai
        End If
        'kjcustomint12(53) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "kjcustomint12 required numeric." : GoTo selesai
        End If
        'kjcustomint13(54) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "kjcustomint13 required numeric." : GoTo selesai
        End If
        'kjcustomint14(55) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "kjcustomint14 required numeric." : GoTo selesai
        End If
        'kjcustomint15(56) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "kjcustomint15 required numeric." : GoTo selesai
        End If
        'kjcustomint16(57) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "kjcustomint16 required numeric." : GoTo selesai
        End If
        'kjcustomint17(58) As Integer
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "kjcustomint17 required numeric." : GoTo selesai
        End If
        'kjcustomint18(59) As Integer
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "kjcustomint18 required numeric." : GoTo selesai
        End If
        'kjcustomint19(60) As Integer
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "kjcustomint19 required numeric." : GoTo selesai
        End If
        'kjcustomint20(61) As Integer
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "kjcustomint20 required numeric." : GoTo selesai
        End If
        'kjcustomdbl1(62) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "kjcustomdbl1 required numeric." : GoTo selesai
        End If
        'kjcustomdbl2(63) As Double
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "kjcustomdbl2 required numeric." : GoTo selesai
        End If
        'kjcustomdbl3(64) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "kjcustomdbl3 required numeric." : GoTo selesai
        End If
        'kjcustomdbl4(65) As Double
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "kjcustomdbl4 required numeric." : GoTo selesai
        End If
        'kjcustomdbl5(66) As Double
        If (IsNumeric(dataUtama(90)) = False) Then
            result(2) = "kjcustomdbl5 required numeric." : GoTo selesai
        End If
        'kjcustomdbl6(67) As Double
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "kjcustomdbl6 required numeric." : GoTo selesai
        End If
        'kjcustomdbl7(68) As Double
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "kjcustomdbl7 required numeric." : GoTo selesai
        End If
        'kjcustomdbl8(69) As Double
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "kjcustomdbl8 required numeric." : GoTo selesai
        End If
        'kjcustomdbl9(70) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "kjcustomdbl9 required numeric." : GoTo selesai
        End If
        'kjcustomdbl10(71) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "kjcustomdbl10 required numeric." : GoTo selesai
        End If
        'kjcustomdbl11(72) As Double
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kjcustomdbl11 required numeric." : GoTo selesai
        End If
        'kjcustomdbl12(73) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kjcustomdbl12 required numeric." : GoTo selesai
        End If
        'kjcustomdbl13(74) As Double
        If (IsNumeric(dataUtama(98)) = False) Then
            result(2) = "kjcustomdbl13 required numeric." : GoTo selesai
        End If
        'kjcustomdbl14(75) As Double
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "kjcustomdbl14 required numeric." : GoTo selesai
        End If
        'kjcustomdbl15(76) As Double
        If (IsNumeric(dataUtama(100)) = False) Then
            result(2) = "kjcustomdbl15 required numeric." : GoTo selesai
        End If
        'kjcustomdbl16(77) As Double
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "kjcustomdbl16 required numeric." : GoTo selesai
        End If
        'kjcustomdbl17(78) As Double
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "kjcustomdbl17 required numeric." : GoTo selesai
        End If
        'kjcustomdbl18(79) As Double
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "kjcustomdbl18 required numeric." : GoTo selesai
        End If
        'kjcustomdbl19(80) As Double
        If (IsNumeric(dataUtama(104)) = False) Then
            result(2) = "kjcustomdbl19 required numeric." : GoTo selesai
        End If
        'kjcustomdbl20(81) As Double
        If (IsNumeric(dataUtama(105)) = False) Then
            result(2) = "kjcustomdbl20 required numeric." : GoTo selesai
        End If
        'kjcustomdate1(82) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "kjcustomdate1 required date." : GoTo selesai
        End If
        'kjcustomdate2(83) As Date
        If (IsDate(dataUtama(107)) = False) Then
            result(2) = "kjcustomdate2 required date." : GoTo selesai
        End If
        'kjcustomdate3(84) As Date
        If (IsDate(dataUtama(108)) = False) Then
            result(2) = "kjcustomdate3 required date." : GoTo selesai
        End If
        'kjcustomdate4(85) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "kjcustomdate4 required date." : GoTo selesai
        End If
        'kjcustomdate5(86) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "kjcustomdate5 required date." : GoTo selesai
        End If
        'kjcustomdate6(87) As Date
        If (IsDate(dataUtama(111)) = False) Then
            result(2) = "kjcustomdate6 required date." : GoTo selesai
        End If
        'kjcustomdate7(88) As Date
        If (IsDate(dataUtama(112)) = False) Then
            result(2) = "kjcustomdate7 required date." : GoTo selesai
        End If
        'kjcustomdate8(89) As Date
        If (IsDate(dataUtama(113)) = False) Then
            result(2) = "kjcustomdate8 required date." : GoTo selesai
        End If
        'kjcustomdate9(90) As Date
        If (IsDate(dataUtama(114)) = False) Then
            result(2) = "kjcustomdate9 required date." : GoTo selesai
        End If
        'kjcustomdate10(91) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "kjcustomdate10 required date." : GoTo selesai
        End If
        'kjcustomdate11(92) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "kjcustomdate11 required date." : GoTo selesai
        End If
        'kjcustomdate12(93) As Date
        If (IsDate(dataUtama(117)) = False) Then
            result(2) = "kjcustomdate12 required date." : GoTo selesai
        End If
        'kjcustomdate13(94) As Date
        If (IsDate(dataUtama(118)) = False) Then
            result(2) = "kjcustomdate13 required date." : GoTo selesai
        End If
        'kjcustomdate14(95) As Date
        If (IsDate(dataUtama(119)) = False) Then
            result(2) = "kjcustomdate14 required date." : GoTo selesai
        End If
        'kjcustomdate15(96) As Date
        If (IsDate(dataUtama(120)) = False) Then
            result(2) = "kjcustomdate15 required date." : GoTo selesai
        End If
        'kjcustomdate16(97) As Date
        If (IsDate(dataUtama(121)) = False) Then
            result(2) = "kjcustomdate16 required date." : GoTo selesai
        End If
        'kjcustomdate17(98) As Date
        If (IsDate(dataUtama(122)) = False) Then
            result(2) = "kjcustomdate17 required date." : GoTo selesai
        End If
        'kjcustomdate18(99) As Date
        If (IsDate(dataUtama(123)) = False) Then
            result(2) = "kjcustomdate18 required date." : GoTo selesai
        End If
        'kjcustomdate19(100) As Date
        If (IsDate(dataUtama(124)) = False) Then
            result(2) = "kjcustomdate19 required date." : GoTo selesai
        End If
        'kjcustomdate20(101) As Date
        If (IsDate(dataUtama(125)) = False) Then
            result(2) = "kjcustomdate20 required date." : GoTo selesai
        End If
        'If (IsNumeric(dataUtama(127)) = False) Then
        '    result(2) = "kjkategoriharga required numeric." : GoTo selesai
        'End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kjcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 10 Then
            result(2) = "kjcabang should not be more than 10 character." : GoTo selesai
        End If

        If Len(dataUtama(2)) = 0 Then
            result(2) = "kjlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 10 Then
            result(2) = "kjlokasi should not be more than 10 character." : GoTo selesai
        End If

        'kjsumber(1) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kjsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 10 Then
            result(2) = "kjsumber should not be more than 10 character." : GoTo selesai
        End If

        'kjnotransaksi(3) As String
        If Len(dataUtama(5)) = 0 Then
            result(2) = "kjnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(5)) > 50 Then
            result(2) = "kjnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kjtgl(4) As Date
        If Len(dataUtama(6)) = 0 Then
            result(2) = "kjtgl can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(9)) = 0 Then
            result(2) = "kjnama can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(9)) > 100 Then
            result(2) = "kjnama should not be more than 100 character." : GoTo selesai
        End If

        'jml(5) As Double
        If Len(dataUtama(11)) = 0 Then
            result(2) = "kjtgllahir can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 10 Then
            result(2) = "kjtgllahir should not be more than 10 character." : GoTo selesai
        End If

        'satuan(6) As String
        If Len(dataUtama(12)) = 0 Then
            result(2) = "kjumur can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(12)) > 10 Then
            result(2) = "kjumur should not be more than 10 character." : GoTo selesai
        End If
        If Len(dataUtama(12)) <= 0 Then
            result(2) = "kjumur can't be less than or equal to zero" : GoTo selesai
        End If

        'satuanbarang(9) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "kjjeniskelamin can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 10 Then
            result(2) = "kjjeniskelamin should not be more than 10 character." : GoTo selesai
        End If

        'kjinputtgl(18) As DateTime
        If Len(dataUtama(42)) = 0 Then
            result(2) = "kjinputtgl can't be empty" : GoTo selesai
        End If

        'kjmodifikasitgl(20) As DateTime
        If Len(dataUtama(44)) = 0 Then
            result(2) = "kjmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kjcustomdbl1(62) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "kjcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl2(63) As Double
        If Len(dataUtama(87)) = 0 Then
            result(2) = "kjcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl3(64) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "kjcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl4(65) As Double
        If Len(dataUtama(89)) = 0 Then
            result(2) = "kjcustomdbl4 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl5(66) As Double
        If Len(dataUtama(90)) = 0 Then
            result(2) = "kjcustomdbl5 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl6(67) As Double
        If Len(dataUtama(91)) = 0 Then
            result(2) = "kjcustomdbl6 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl7(68) As Double
        If Len(dataUtama(92)) = 0 Then
            result(2) = "kjcustomdbl7 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl8(69) As Double
        If Len(dataUtama(93)) = 0 Then
            result(2) = "kjcustomdbl8 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl9(70) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "kjcustomdbl9 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl10(71) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "kjcustomdbl10 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl11(72) As Double
        If Len(dataUtama(96)) = 0 Then
            result(2) = "kjcustomdbl11 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl12(73) As Double
        If Len(dataUtama(97)) = 0 Then
            result(2) = "kjcustomdbl12 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl13(74) As Double
        If Len(dataUtama(98)) = 0 Then
            result(2) = "kjcustomdbl13 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl14(75) As Double
        If Len(dataUtama(99)) = 0 Then
            result(2) = "kjcustomdbl14 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl15(76) As Double
        If Len(dataUtama(100)) = 0 Then
            result(2) = "kjcustomdbl15 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl16(77) As Double
        If Len(dataUtama(101)) = 0 Then
            result(2) = "kjcustomdbl16 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl17(78) As Double
        If Len(dataUtama(102)) = 0 Then
            result(2) = "kjcustomdbl17 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl18(79) As Double
        If Len(dataUtama(103)) = 0 Then
            result(2) = "kjcustomdbl18 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl19(80) As Double
        If Len(dataUtama(104)) = 0 Then
            result(2) = "kjcustomdbl19 can't be empty" : GoTo selesai
        End If

        'kjcustomdbl20(81) As Double
        If Len(dataUtama(105)) = 0 Then
            result(2) = "kjcustomdbl20 can't be empty" : GoTo selesai
        End If

        'kjcustomdate1(82) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "kjcustomdate1 can't be empty" : GoTo selesai
        End If

        'kjcustomdate2(83) As Date
        If Len(dataUtama(107)) = 0 Then
            result(2) = "kjcustomdate2 can't be empty" : GoTo selesai
        End If

        'kjcustomdate3(84) As Date
        If Len(dataUtama(108)) = 0 Then
            result(2) = "kjcustomdate3 can't be empty" : GoTo selesai
        End If

        'kjcustomdate4(85) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "kjcustomdate4 can't be empty" : GoTo selesai
        End If

        'kjcustomdate5(86) As Date
        If Len(dataUtama(110)) = 0 Then
            result(2) = "kjcustomdate5 can't be empty" : GoTo selesai
        End If

        'kjcustomdate6(87) As Date
        If Len(dataUtama(111)) = 0 Then
            result(2) = "kjcustomdate6 can't be empty" : GoTo selesai
        End If

        'kjcustomdate7(88) As Date
        If Len(dataUtama(112)) = 0 Then
            result(2) = "kjcustomdate7 can't be empty" : GoTo selesai
        End If

        'kjcustomdate8(89) As Date
        If Len(dataUtama(113)) = 0 Then
            result(2) = "kjcustomdate8 can't be empty" : GoTo selesai
        End If

        'kjcustomdate9(90) As Date
        If Len(dataUtama(114)) = 0 Then
            result(2) = "kjcustomdate9 can't be empty" : GoTo selesai
        End If

        'kjcustomdate10(91) As Date
        If Len(dataUtama(115)) = 0 Then
            result(2) = "kjcustomdate10 can't be empty" : GoTo selesai
        End If

        'kjcustomdate11(92) As Date
        If Len(dataUtama(116)) = 0 Then
            result(2) = "kjcustomdate11 can't be empty" : GoTo selesai
        End If

        'kjcustomdate12(93) As Date
        If Len(dataUtama(117)) = 0 Then
            result(2) = "kjcustomdate12 can't be empty" : GoTo selesai
        End If

        'kjcustomdate13(94) As Date
        If Len(dataUtama(118)) = 0 Then
            result(2) = "kjcustomdate13 can't be empty" : GoTo selesai
        End If

        'kjcustomdate14(95) As Date
        If Len(dataUtama(119)) = 0 Then
            result(2) = "kjcustomdate14 can't be empty" : GoTo selesai
        End If

        'kjcustomdate15(96) As Date
        If Len(dataUtama(120)) = 0 Then
            result(2) = "kjcustomdate15 can't be empty" : GoTo selesai
        End If

        'kjcustomdate16(97) As Date
        If Len(dataUtama(121)) = 0 Then
            result(2) = "kjcustomdate16 can't be empty" : GoTo selesai
        End If

        'kjcustomdate17(98) As Date
        If Len(dataUtama(122)) = 0 Then
            result(2) = "kjcustomdate17 can't be empty" : GoTo selesai
        End If

        'kjcustomdate18(99) As Date
        If Len(dataUtama(123)) = 0 Then
            result(2) = "kjcustomdate18 can't be empty" : GoTo selesai
        End If

        'kjcustomdate19(100) As Date
        If Len(dataUtama(124)) = 0 Then
            result(2) = "kjcustomdate19 can't be empty" : GoTo selesai
        End If

        'kjcustomdate20(101) As Date
        If Len(dataUtama(125)) = 0 Then
            result(2) = "kjcustomdate20 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kjid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjnopasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprefix", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjumur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjeniskelamin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatusperkawinan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjagama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjayah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjibu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjsuamiistri", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnotelepon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjprovinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnegara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkeluargalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjnoteleponlain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjtglmeninggal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcarakunjungan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdirujukoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjditanggungoleh", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuskamar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjkategoriharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjlayanan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdokter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdirujukke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjawalankatpasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjstatuspasien", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjpetugas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kjdesa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjkecamatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjdiagnosa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjketerangan", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "kjid~kjcabang~kjlokasi~kjsumber~kjautonotransaksi~kjnotransaksi~kjtgl~kjkodepa~kjnopasien~kjnama~kjprefix~kjtgllahir~kjumur~kjjeniskelamin~kjstatusperkawinan~kjagama~kjayah~kjibu~kjsuamiistri~kjnotelepon~kjnofax~kjnohp~kjemail~kjalamat~kjkota~kjprovinsi~kjnegara~kjkodepos~kjkeluargalain~kjnoteleponlain~kjcatatan~kjtglkeluar~kjtglmeninggal~kjcarakunjungan~kjdirujukoleh~kjditanggungoleh~kjstatusrealisasi~kjstatus~kjstatussebelumnya~kjjmlrevisi~kjcetakanke~kjinputuser~kjinputtgl~kjmodifikasiuser~kjmodifikasitgl~kjisclose~kjcustomtext1~kjcustomtext2~kjcustomtext3~kjcustomtext4~kjcustomtext5~kjcustomtext6~kjcustomtext7~kjcustomtext8~kjcustomtext9~kjcustomtext10~kjcustomtext11~kjcustomtext12~kjcustomtext13~kjcustomtext14~kjcustomtext15~kjcustomtext16~kjcustomtext17~kjcustomtext18~kjcustomtext19~kjcustomtext20~kjcustomint1~kjcustomint2~kjcustomint3~kjcustomint4~kjcustomint5~kjcustomint6~kjcustomint7~kjcustomint8~kjcustomint9~kjcustomint10~kjcustomint11~kjcustomint12~kjcustomint13~kjcustomint14~kjcustomint15~kjcustomint16~kjcustomint17~kjcustomint18~kjcustomint19~kjcustomint20~kjcustomdbl1~kjcustomdbl2~kjcustomdbl3~kjcustomdbl4~kjcustomdbl5~kjcustomdbl6~kjcustomdbl7~kjcustomdbl8~kjcustomdbl9~kjcustomdbl10~kjcustomdbl11~kjcustomdbl12~kjcustomdbl13~kjcustomdbl14~kjcustomdbl15~kjcustomdbl16~kjcustomdbl17~kjcustomdbl18~kjcustomdbl19~kjcustomdbl20~kjcustomdate1~kjcustomdate2~kjcustomdate3~kjcustomdate4~kjcustomdate5~kjcustomdate6~kjcustomdate7~kjcustomdate8~kjcustomdate9~kjcustomdate10~kjcustomdate11~kjcustomdate12~kjcustomdate13~kjcustomdate14~kjcustomdate15~kjcustomdate16~kjcustomdate17~kjcustomdate18~kjcustomdate19~kjcustomdate20~kjstatuskamar~kjkategoriharga~kjperawatan~kjkategoripasien~kjlayanan~kjkamar~kjdokter~kjdirujukke~kjawalankatpasien~kjstatuspasien~kjpetugas~kjdesa~kjkecamatan~kjdiagnosa~kjketerangan", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122) & "~" & dataUtama(123) & "~" & dataUtama(124) & "~" & dataUtama(125) & "~" & dataUtama(126) & "~" & dataUtama(127) & "~" & dataUtama(128) & "~" & dataUtama(129) & "~" & dataUtama(130) & "~" & dataUtama(131) & "~" & dataUtama(132) & "~" & dataUtama(133) & "~" & dataUtama(134) & "~" & dataUtama(135) & "~" & dataUtama(136) & "~" & dataUtama(137) & "~" & dataUtama(138) & "~" & dataUtama(139) & "~" & dataUtama(140)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
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

                If isUpdate Then
                    result(4) = drutama("kjid")
                    notransaksi = drutama("kjnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kjid), kjnotransaksi FROM M_11_kj WHERE kjid='" & result(4) & "' AND kjstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    'If (rowUpdate > 0) Then

                    'CEK NO TRANSAKSI ======================
                    'If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                    'Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(kjid) FROM M_11_kj WHERE kjnotransaksi='" & notransaksi & "'")
                    'Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    ' If cekNo > 0 Then
                    'result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    ' End  If
                    'End If
                    'END OF CEK NO TRANSAKSI ===============

                    'SIMPAN HISTORY ========================
                    'Dim SimpanHistory As New m11_kj_history
                    'Dim rsSimpanHistory As String = SimpanHistory.M11_Kj_HistorySimpan("" & paramSplit(0) & "★M11_Kj_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kjsumber")) & "▼" & FixQuotes(drutama("kjid")) & "")
                    'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                    'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                    ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    'If (rsSplitResult(1) = 0) Then
                    '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                    'End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update m_11_kj set kjketerangan = " & drutama("kjketerangan") & " where kjid = '" & drutama("kjid") & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    sql = "Update m_11_lb set lbketerangan = " & drutama("kjketerangan") & " where lbidkj = '" & drutama("kjid") & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    sql = "Update m_11_ak set akketerangan = " & drutama("kjketerangan") & " where akidkj = '" & drutama("kjid") & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'Else
                    '     result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    ' End If
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "KJ", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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

End Class