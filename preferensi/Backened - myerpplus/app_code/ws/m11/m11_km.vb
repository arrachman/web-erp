Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m11_km
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M11_KmSimpan(ByVal param As String) As String
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
        'kmid(0) As Integer, kmcabang(1) As String, kmlokasi(2) As String, kmgudang(3) As String, kmsumber(4) As String, 
        'kmautonotransaksi(5) As Integer, kmnotransaksi(6) As String, kmtgl(7) As Date, kmkodepa(8) As Integer, kmcustomer(9) As Integer, 
        'kmcustomerkontak(10) As String, kmuraian(11) As String, kmcatatan(12) As String, kmnoref(13) As String, kmtglnoref(14) As Date, 
        'kmmatauang(15) As String, kmkurs(16) As Double, kmidkj(17) As Integer, kmkamar(18) As String, kmkasur(19) As String, 
        'kmtglmasuk(20) As DateTime, kmtglkeluar(21) As DateTime, kmjmlhari(22) As Integer, kmharga(23) As Double, kmtotaltransaksi(24) As Double, 
        'kmrekpersediaan(25) As String, kmrekhargapokok(26) As String, kmrekdiskonpenjualan(27) As String, kmrekpenjualan(28) As String, kmstatusrealisasi(29) As Interger, 
        'kmstatus(30) As Integer, kmstatussebelumnya(31) As Integer, kmjmlrevisi(32) As Integer, kmcetakanke(33) As Integer, kminputuser(34) As Integer, 
        'kminputtgl(35) As DateTime, kmmodifikasiuser(36) As Integer, kmmodifikasitgl(37) As DateTime, kmposting(38) As Integer, kmisclose(39) As Integer, 
        'kmcustomtext1(40) As String, kmcustomtext2(41) As String, kmcustomtext3(42) As String, kmcustomtext4(43) As String, kmcustomtext5(44) As String, 
        'kmcustomtext6(45) As String, kmcustomtext7(46) As String, kmcustomtext8(47) As String, kmcustomtext9(48) As String, kmcustomtext10(49) As String, 
        'kmcustomtext11(50) As String, kmcustomtext12(51) As String, kmcustomtext13(52) As String, kmcustomtext14(53) As String, kmcustomtext15(54) As String, 
        'kmcustomtext16(55) As String, kmcustomtext17(56) As String, kmcustomtext18(57) As String, kmcustomtext19(58) As String, kmcustomtext20(59) As String, 
        'kmcustomint1(60) As Integer, kmcustomint2(61) As Integer, kmcustomint3(62) As Integer, kmcustomint4(63) As Integer, kmcustomint5(64) As Integer, 
        'kmcustomint6(65) As Integer, kmcustomint7(66) As Integer, kmcustomint8(67) As Integer, kmcustomint9(68) As Integer, kmcustomint10(69) As Integer, 
        'kmcustomint11(70) As Integer, kmcustomint12(71) As Integer, kmcustomint13(72) As Integer, kmcustomint14(73) As Integer, kmcustomint15(74) As Integer, 
        'kmcustomint16(75) As Integer, kmcustomint17(76) As Integer, kmcustomint18(77) As Integer, kmcustomint19(78) As Integer, kmcustomint20(79) As Integer, 
        'kmcustomdbl1(80) As Double, kmcustomdbl2(81) As Double, kmcustomdbl3(82) As Double, kmcustomdbl4(83) As Double, kmcustomdbl5(84) As Double, 
        'kmcustomdbl6(85) As Double, kmcustomdbl7(86) As Double, kmcustomdbl8(87) As Double, kmcustomdbl9(88) As Double, kmcustomdbl10(89) As Double, 
        'kmcustomdbl11(90) As Double, kmcustomdbl12(91) As Double, kmcustomdbl13(92) As Double, kmcustomdbl14(93) As Double, kmcustomdbl15(94) As Double, 
        'kmcustomdbl16(95) As Double, kmcustomdbl17(96) As Double, kmcustomdbl18(97) As Double, kmcustomdbl19(98) As Double, kmcustomdbl20(99) As Double, 
        'kmcustomdate1(100) As Date, kmcustomdate2(101) As Date, kmcustomdate3(102) As Date, kmcustomdate4(103) As Date, kmcustomdate5(104) As Date, 
        'kmcustomdate6(105) As Date, kmcustomdate7(106) As Date, kmcustomdate8(107) As Date, kmcustomdate9(108) As Date, kmcustomdate10(109) As Date, 
        'kmcustomdate11(110) As Date, kmcustomdate12(111) As Date, kmcustomdate13(112) As Date, kmcustomdate14(113) As Date, kmcustomdate15(114) As Date, 
        'kmcustomdate16(115) As Date, kmcustomdate17(116) As Date, kmcustomdate18(117) As Date, kmcustomdate19(118) As Date, kmcustomdate20(119) As Date,
        'kmperawatan(120) As String, kmkategoripasien(121) As String, kmawalankatpasien(122) As String


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, 
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, 
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, 
        'kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, 
        'kmrekpersediaan, kmrekhargapokok, kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, 
        'kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, 
        'kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, 
        'kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, 
        'kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, 
        'kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, 
        'kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, 
        'kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, 
        'kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, 
        'kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, 
        'kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, 
        'kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, 
        'kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, 
        'kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, 
        'kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, 
        'kmcustomdate1, kmcustomdate2, kmcustomdate3, kmcustomdate4, kmcustomdate5, 
        'kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, 
        'kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, 
        'kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20
        'kmperawatan, kmkategoripasien, kmawalankatpasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 123) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kmid required numeric." : GoTo selesai
        End If
        'kmautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "kmautonotransaksi required numeric." : GoTo selesai
        End If
        'kmtgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "kmtgl required date." : GoTo selesai
        End If
        'kmkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "kmkodepa required numeric." : GoTo selesai
        End If
        'kmcustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "kmcustomer required numeric." : GoTo selesai
        End If
        'kmtglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "kmtglnoref required date." : GoTo selesai
        End If
        'kmkurs(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "kmkurs required numeric." : GoTo selesai
        End If
        'kmidkj(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "kmidkj required numeric." : GoTo selesai
        End If
        'kmtglmasuk(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "kmtglmasuk required date." : GoTo selesai
        End If
        'kmtglkeluar(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "kmtglkeluar required date." : GoTo selesai
        End If
        'kmjmlhari(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "kmjmlhari required numeric." : GoTo selesai
        End If
        'kmharga(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "kmharga required numeric." : GoTo selesai
        End If
        'kmtotaltransaksi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "kmtotaltransaksi required numeric." : GoTo selesai
        End If
        'kmstatusrealisasi(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "kmstatusrealisasi required numeric." : GoTo selesai
        End If
        'kmstatus(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "kmstatus required numeric." : GoTo selesai
        End If
        'kmstatussebelumnya(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "kmstatussebelumnya required numeric." : GoTo selesai
        End If
        'kmjmlrevisi(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "kmjmlrevisi required numeric." : GoTo selesai
        End If
        'kmcetakanke(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "kmcetakanke required numeric." : GoTo selesai
        End If
        'kminputuser(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "kminputuser required numeric." : GoTo selesai
        End If
        'kminputtgl(35) As DateTime
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "kminputtgl required date." : GoTo selesai
        End If
        'kmmodifikasiuser(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "kmmodifikasiuser required numeric." : GoTo selesai
        End If
        'kmmodifikasitgl(37) As DateTime
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "kmmodifikasitgl required date." : GoTo selesai
        End If
        'lmposting(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "kmposting required numeric." : GoTo selesai
        End If
        'kmisclose(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "kmisclose required numeric." : GoTo selesai
        End If
        'kmcustomint1(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "kmcustomint1 required numeric." : GoTo selesai
        End If
        'kmcustomint2(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "kmcustomint2 required numeric." : GoTo selesai
        End If
        'kmcustomint3(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "kmcustomint3 required numeric." : GoTo selesai
        End If
        'kmcustomint4(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "kmcustomint4 required numeric." : GoTo selesai
        End If
        'kmcustomint5(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "kmcustomint5 required numeric." : GoTo selesai
        End If
        'kmcustomint6(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "kmcustomint6 required numeric." : GoTo selesai
        End If
        'kmcustomint7(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "kmcustomint7 required numeric." : GoTo selesai
        End If
        'kmcustomint8(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "kmcustomint8 required numeric." : GoTo selesai
        End If
        'kmcustomint9(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "kmcustomint9 required numeric." : GoTo selesai
        End If
        'kmcustomint10(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "kmcustomint10 required numeric." : GoTo selesai
        End If
        'kmcustomint11(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "kmcustomint11 required numeric." : GoTo selesai
        End If
        'kmcustomint12(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "kmcustomint12 required numeric." : GoTo selesai
        End If
        'kmcustomint13(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "kmcustomint13 required numeric." : GoTo selesai
        End If
        'kmcustomint14(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "kmcustomint14 required numeric." : GoTo selesai
        End If
        'kmcustomint15(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "kmcustomint15 required numeric." : GoTo selesai
        End If
        'kmcustomint16(75) As Integer
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "kmcustomint16 required numeric." : GoTo selesai
        End If
        'kmcustomint17(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "kmcustomint17 required numeric." : GoTo selesai
        End If
        'kmcustomint18(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "kmcustomint18 required numeric." : GoTo selesai
        End If
        'kmcustomint19(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "kmcustomint19 required numeric." : GoTo selesai
        End If
        'kmcustomint20(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "kmcustomint20 required numeric." : GoTo selesai
        End If
        'kmcustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "kmcustomdbl1 required numeric." : GoTo selesai
        End If
        'kmcustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "kmcustomdbl2 required numeric." : GoTo selesai
        End If
        'kmcustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "kmcustomdbl3 required numeric." : GoTo selesai
        End If
        'kmcustomdbl4(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "kmcustomdbl4 required numeric." : GoTo selesai
        End If
        'kmcustomdbl5(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "kmcustomdbl5 required numeric." : GoTo selesai
        End If
        'kmcustomdbl6(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "kmcustomdbl6 required numeric." : GoTo selesai
        End If
        'kmcustomdbl7(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "kmcustomdbl7 required numeric." : GoTo selesai
        End If
        'kmcustomdbl8(87) As Double
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "kmcustomdbl8 required numeric." : GoTo selesai
        End If
        'kmcustomdbl9(88) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "kmcustomdbl9 required numeric." : GoTo selesai
        End If
        'kmcustomdbl10(89) As Double
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "kmcustomdbl10 required numeric." : GoTo selesai
        End If
        'kmcustomdbl11(90) As Double
        If (IsNumeric(dataUtama(90)) = False) Then
            result(2) = "kmcustomdbl11 required numeric." : GoTo selesai
        End If
        'kmcustomdbl12(91) As Double
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "kmcustomdbl12 required numeric." : GoTo selesai
        End If
        'kmcustomdbl13(92) As Double
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "kmcustomdbl13 required numeric." : GoTo selesai
        End If
        'kmcustomdbl14(93) As Double
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "kmcustomdbl14 required numeric." : GoTo selesai
        End If
        'kmcustomdbl15(94) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "kmcustomdbl15 required numeric." : GoTo selesai
        End If
        'kmcustomdbl16(95) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "kmcustomdbl16 required numeric." : GoTo selesai
        End If
        'kmcustomdbl17(96) As Double
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kmcustomdbl17 required numeric." : GoTo selesai
        End If
        'kmcustomdbl18(97) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kmcustomdbl18 required numeric." : GoTo selesai
        End If
        'kmcustomdbl19(98) As Double
        If (IsNumeric(dataUtama(98)) = False) Then
            result(2) = "kmcustomdbl19 required numeric." : GoTo selesai
        End If
        'kmcustomdbl20(99) As Double
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "kmcustomdbl20 required numeric." : GoTo selesai
        End If
        'kmcustomdate1(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "kmcustomdate1 required date." : GoTo selesai
        End If
        'kmcustomdate2(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "kmcustomdate2 required date." : GoTo selesai
        End If
        'kmcustomdate3(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "kmcustomdate3 required date." : GoTo selesai
        End If
        'kmcustomdate4(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "kmcustomdate4 required date." : GoTo selesai
        End If
        'kmcustomdate5(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "kmcustomdate5 required date." : GoTo selesai
        End If
        'kmcustomdate6(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "kmcustomdate6 required date." : GoTo selesai
        End If
        'kmcustomdate7(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "kmcustomdate7 required date." : GoTo selesai
        End If
        'kmcustomdate8(107) As Date
        If (IsDate(dataUtama(107)) = False) Then
            result(2) = "kmcustomdate8 required date." : GoTo selesai
        End If
        'kmcustomdate9(108) As Date
        If (IsDate(dataUtama(108)) = False) Then
            result(2) = "kmcustomdate9 required date." : GoTo selesai
        End If
        'kmcustomdate10(109) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "kmcustomdate10 required date." : GoTo selesai
        End If
        'kmcustomdate11(110) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "kmcustomdate11 required date." : GoTo selesai
        End If
        'kmcustomdate12(111) As Date
        If (IsDate(dataUtama(111)) = False) Then
            result(2) = "kmcustomdate12 required date." : GoTo selesai
        End If
        'kmcustomdate13(112) As Date
        If (IsDate(dataUtama(112)) = False) Then
            result(2) = "kmcustomdate13 required date." : GoTo selesai
        End If
        'kmcustomdate14(113) As Date
        If (IsDate(dataUtama(113)) = False) Then
            result(2) = "kmcustomdate14 required date." : GoTo selesai
        End If
        'kmcustomdate15(114) As Date
        If (IsDate(dataUtama(114)) = False) Then
            result(2) = "kmcustomdate15 required date." : GoTo selesai
        End If
        'kmcustomdate16(115) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "kmcustomdate16 required date." : GoTo selesai
        End If
        'kmcustomdate17(116) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "kmcustomdate17 required date." : GoTo selesai
        End If
        'kmcustomdate18(117) As Date
        If (IsDate(dataUtama(117)) = False) Then
            result(2) = "kmcustomdate18 required date." : GoTo selesai
        End If
        'kmcustomdate19(118) As Date
        If (IsDate(dataUtama(118)) = False) Then
            result(2) = "kmcustomdate19 required date." : GoTo selesai
        End If
        'kmcustomdate20(119) As Date
        If (IsDate(dataUtama(119)) = False) Then
            result(2) = "kmcustomdate20 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'kmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "kmcabang should not be more than 25 character." : GoTo selesai
        End If

        'kmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "kmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "kmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'kmgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kmgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "kmgudang should not be more than 25 character." : GoTo selesai
        End If

        'kmsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "kmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "kmsumber should not be more than 10 character." : GoTo selesai
        End If

        'kmnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "kmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "kmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kmtgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "kmtgl can't be empty" : GoTo selesai
        End If

        'kmtglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "kmtglnoref can't be empty" : GoTo selesai
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

        'kmkamar(18) As String
        If Len(dataUtama(18)) = 0 Then
            result(2) = "kmkamar can't be empty" : GoTo selesai
        End If

        'kmkasur(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "kmkasur can't be empty" : GoTo selesai
        End If

        'kmtglmasuk(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "kmtglmasuk can't be empty" : GoTo selesai
        End If

        'kmtglkeluar(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "kmtglkeluar can't be empty" : GoTo selesai
        End If

        'kmtotaltransaksi(24) As Double
        If Len(dataUtama(24)) = 0 Then
            result(2) = "kmtotaltransaksi can't be empty" : GoTo selesai
        End If

        'kminputtgl(35) As DateTime
        If Len(dataUtama(35)) = 0 Then
            result(2) = "kminputtgl can't be empty" : GoTo selesai
        End If

        'kmmodifikasitgl(37) As DateTime
        If Len(dataUtama(37)) = 0 Then
            result(2) = "kmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kmcustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "kmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "kmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "kmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl4(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "kmcustomdbl4 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl5(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "kmcustomdbl5 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl6(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "kmcustomdbl6 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl7(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "kmcustomdbl7 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl8(87) As Double
        If Len(dataUtama(87)) = 0 Then
            result(2) = "kmcustomdbl8 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl9(88) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "kmcustomdbl9 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl10(89) As Double
        If Len(dataUtama(89)) = 0 Then
            result(2) = "kmcustomdbl10 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl11(90) As Double
        If Len(dataUtama(90)) = 0 Then
            result(2) = "kmcustomdbl11 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl12(91) As Double
        If Len(dataUtama(91)) = 0 Then
            result(2) = "kmcustomdbl12 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl13(92) As Double
        If Len(dataUtama(92)) = 0 Then
            result(2) = "kmcustomdbl13 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl14(93) As Double
        If Len(dataUtama(93)) = 0 Then
            result(2) = "kmcustomdbl14 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl15(94) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "kmcustomdbl15 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl16(95) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "kmcustomdbl16 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl17(96) As Double
        If Len(dataUtama(96)) = 0 Then
            result(2) = "kmcustomdbl17 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl18(97) As Double
        If Len(dataUtama(97)) = 0 Then
            result(2) = "kmcustomdbl18 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl19(98) As Double
        If Len(dataUtama(98)) = 0 Then
            result(2) = "kmcustomdbl19 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl20(99) As Double
        If Len(dataUtama(99)) = 0 Then
            result(2) = "kmcustomdbl20 can't be empty" : GoTo selesai
        End If

        'kmcustomdate1(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "kmcustomdate1 can't be empty" : GoTo selesai
        End If

        'kmcustomdate2(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "kmcustomdate2 can't be empty" : GoTo selesai
        End If

        'kmcustomdate3(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "kmcustomdate3 can't be empty" : GoTo selesai
        End If

        'kmcustomdate4(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "kmcustomdate4 can't be empty" : GoTo selesai
        End If

        'kmcustomdate5(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "kmcustomdate5 can't be empty" : GoTo selesai
        End If

        'kmcustomdate6(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "kmcustomdate6 can't be empty" : GoTo selesai
        End If

        'kmcustomdate7(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "kmcustomdate7 can't be empty" : GoTo selesai
        End If

        'kmcustomdate8(107) As Date
        If Len(dataUtama(107)) = 0 Then
            result(2) = "kmcustomdate8 can't be empty" : GoTo selesai
        End If

        'kmcustomdate9(108) As Date
        If Len(dataUtama(108)) = 0 Then
            result(2) = "kmcustomdate9 can't be empty" : GoTo selesai
        End If

        'kmcustomdate10(109) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "kmcustomdate10 can't be empty" : GoTo selesai
        End If

        'kmcustomdate11(110) As Date
        If Len(dataUtama(110)) = 0 Then
            result(2) = "kmcustomdate11 can't be empty" : GoTo selesai
        End If

        'kmcustomdate12(111) As Date
        If Len(dataUtama(111)) = 0 Then
            result(2) = "kmcustomdate12 can't be empty" : GoTo selesai
        End If

        'kmcustomdate13(112) As Date
        If Len(dataUtama(112)) = 0 Then
            result(2) = "kmcustomdate13 can't be empty" : GoTo selesai
        End If

        'kmcustomdate14(113) As Date
        If Len(dataUtama(113)) = 0 Then
            result(2) = "kmcustomdate14 can't be empty" : GoTo selesai
        End If

        'kmcustomdate15(114) As Date
        If Len(dataUtama(114)) = 0 Then
            result(2) = "kmcustomdate15 can't be empty" : GoTo selesai
        End If

        'kmcustomdate16(115) As Date
        If Len(dataUtama(115)) = 0 Then
            result(2) = "kmcustomdate16 can't be empty" : GoTo selesai
        End If

        'kmcustomdate17(116) As Date
        If Len(dataUtama(116)) = 0 Then
            result(2) = "kmcustomdate17 can't be empty" : GoTo selesai
        End If

        'kmcustomdate18(117) As Date
        If Len(dataUtama(117)) = 0 Then
            result(2) = "kmcustomdate18 can't be empty" : GoTo selesai
        End If

        'kmcustomdate19(118) As Date
        If Len(dataUtama(118)) = 0 Then
            result(2) = "kmcustomdate19 can't be empty" : GoTo selesai
        End If

        'kmcustomdate20(119) As Date
        If Len(dataUtama(119)) = 0 Then
            result(2) = "kmcustomdate20 can't be empty" : GoTo selesai
        End If

        'kmperawatan(120) As Date
        If Len(dataUtama(120)) > 10 Then
            result(2) = "kmperawatan should not be more than 10 character." : GoTo selesai
        End If

        'kmkategoripasien(121) As Date
        If Len(dataUtama(121)) > 10 Then
            result(2) = "kmkategoripasien should not be more than 10 character." : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "kmidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkasur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmjmlhari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmharga", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmtotaltransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmrekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkategoripasien", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmawalankatpasien", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "kmid~kmcabang~kmlokasi~kmgudang~kmsumber~kmautonotransaksi~kmnotransaksi~kmtgl~kmkodepa~kmcustomer~kmcustomerkontak~kmuraian~kmcatatan~kmnoref~kmtglnoref~kmmatauang~kmkurs~kmidkj~kmkamar~kmkasur~kmtglmasuk~kmtglkeluar~kmjmlhari~kmharga~kmtotaltransaksi~kmrekpersediaan~kmrekhargapokok~kmrekdiskonpenjualan~kmrekpenjualan~kmstatusrealisasi~kmstatus~kmstatussebelumnya~kmjmlrevisi~kmcetakanke~kminputuser~kminputtgl~kmmodifikasiuser~kmmodifikasitgl~kmposting~kmisclose~kmcustomtext1~kmcustomtext2~kmcustomtext3~kmcustomtext4~kmcustomtext5~kmcustomtext6~kmcustomtext7~kmcustomtext8~kmcustomtext9~kmcustomtext10~kmcustomtext11~kmcustomtext12~kmcustomtext13~kmcustomtext14~kmcustomtext15~kmcustomtext16~kmcustomtext17~kmcustomtext18~kmcustomtext19~kmcustomtext20~kmcustomint1~kmcustomint2~kmcustomint3~kmcustomint4~kmcustomint5~kmcustomint6~kmcustomint7~kmcustomint8~kmcustomint9~kmcustomint10~kmcustomint11~kmcustomint12~kmcustomint13~kmcustomint14~kmcustomint15~kmcustomint16~kmcustomint17~kmcustomint18~kmcustomint19~kmcustomint20~kmcustomdbl1~kmcustomdbl2~kmcustomdbl3~kmcustomdbl4~kmcustomdbl5~kmcustomdbl6~kmcustomdbl7~kmcustomdbl8~kmcustomdbl9~kmcustomdbl10~kmcustomdbl11~kmcustomdbl12~kmcustomdbl13~kmcustomdbl14~kmcustomdbl15~kmcustomdbl16~kmcustomdbl17~kmcustomdbl18~kmcustomdbl19~kmcustomdbl20~kmcustomdate1~kmcustomdate2~kmcustomdate3~kmcustomdate4~kmcustomdate5~kmcustomdate6~kmcustomdate7~kmcustomdate8~kmcustomdate9~kmcustomdate10~kmcustomdate11~kmcustomdate12~kmcustomdate13~kmcustomdate14~kmcustomdate15~kmcustomdate16~kmcustomdate17~kmcustomdate18~kmcustomdate19~kmcustomdate20~kmperawatan~kmkategoripasien~kmawalankatpasien", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122)) = False Then
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
            'result(2) = "No. : '" & dtutama.Rows.Count : GoTo selesai
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 11, vMenuId As Integer = 7
                Select Case drutama("kmstatus")
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


                'result(2) = "Bed sdfsdfsdf" : Trans.Rollback() : GoTo selesai
                'CEK STATUS ISCLOSE KAMAR ================
                If drutama("kmstatus") = 2 Then
                    Dim dtCekIsclose As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkode = '" & drutama("kmkasur") & "' AND bisclose = 1", myConn)
                    Dim cekIsclose As Double = Val(dtCekIsclose.Rows(0)(0))
                    If cekIsclose > 0 Then
                        result(2) = "Bed : '" & drutama("kmkasur") & "' - not available." : Trans.Rollback() : GoTo selesai
                    End If
                End If
                'END OF STATUS ISCLOSE KAMAR =============

                If isUpdate Then

                    result(4) = drutama("kmid")
                    notransaksi = drutama("kmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(kmid), kmnotransaksi FROM M_11_km WHERE kmid='" & result(4) & "' AND kmstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kmid) FROM m_11_km WHERE kmnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m11_km_history
                        Dim kmSimpanHistory As String = SimpanHistory.M11_Km_HistorySimpan("" & paramSplit(0) & "★M11_Km_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("kmsumber")) & "▼" & FixQuotes(drutama("kmid")) & "")
                        Dim kmSplit() As String = kmSimpanHistory.Split(sptParam)
                        Dim kmSplitResult() As String = kmSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (kmSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & kmSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M_11_Km set kmcabang  = '" & FixQuotes(drutama("kmcabang")) & "', kmlokasi  = '" & FixQuotes(drutama("kmlokasi")) & "', kmgudang  = '" & FixQuotes(drutama("kmgudang")) & "', kmsumber  = '" & FixQuotes(drutama("kmsumber")) & "', kmautonotransaksi  = " & drutama("kmautonotransaksi") & ", kmnotransaksi  = '" & FixQuotes(notransaksi) & "', kmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("kmtgl"))) & "', kmkodepa  = " & drutama("kmkodepa") & ", kmcustomer  = " & drutama("kmcustomer") & ", kmcustomerkontak  = '" & FixQuotes(drutama("kmcustomerkontak")) & "', kmuraian  = '" & FixQuotes(drutama("kmuraian")) & "', kmcatatan  = '" & FixQuotes(drutama("kmcatatan")) & "', kmnoref  = '" & FixQuotes(drutama("kmnoref")) & "', kmtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("kmtglnoref"))) & "', kmmatauang  = '" & FixQuotes(drutama("kmmatauang")) & "', kmkurs  = '" & FixDouble(drutama("kmkurs")) & "', kmidkj  = " & drutama("kmidkj") & ", kmkamar  = '" & FixQuotes(drutama("kmkamar")) & "', kmkasur  = '" & FixQuotes(drutama("kmkasur")) & "', kmtglmasuk  = '" & FixQuotes(drutama("kmtglmasuk")) & "', kmtglkeluar  = '" & FixQuotes(drutama("kmtglkeluar")) & "', kmjmlhari  = " & drutama("kmjmlhari") & ", kmharga  = " & drutama("kmharga") & ", kmtotaltransaksi  = " & drutama("kmtotaltransaksi") & ", kmrekpersediaan  = '" & FixQuotes(drutama("kmrekpersediaan")) & "', kmrekhargapokok  = '" & FixQuotes(drutama("kmrekhargapokok")) & "', kmrekdiskonpenjualan  = '" & FixQuotes(drutama("kmrekdiskonpenjualan")) & "', kmrekpenjualan  = '" & FixQuotes(drutama("kmrekpenjualan")) & "', kmstatusrealisasi  = " & drutama("kmstatusrealisasi") & ", kmstatus  = " & drutama("kmstatus") & ", kmstatussebelumnya  = " & drutama("kmstatussebelumnya") & ", kmjmlrevisi  = kmjmlrevisi+1, kmcetakanke  = " & drutama("kmcetakanke") & ", kmmodifikasiuser  = " & drutama("kmmodifikasiuser") & ", kmmodifikasitgl  = NOW(), kmposting  = '" & FixDouble(drutama("kmposting")) & "', kmcustomtext1  = '" & FixQuotes(drutama("kmcustomtext1")) & "', kmcustomtext2  = '" & FixQuotes(drutama("kmcustomtext2")) & "', kmcustomtext3  = '" & FixQuotes(drutama("kmcustomtext3")) & "', kmcustomtext4  = '" & FixQuotes(drutama("kmcustomtext4")) & "', kmcustomtext5  = '" & FixQuotes(drutama("kmcustomtext5")) & "', kmcustomtext6  = '" & FixQuotes(drutama("kmcustomtext6")) & "', kmcustomtext7  = '" & FixQuotes(drutama("kmcustomtext7")) & "', kmcustomtext8  = '" & FixQuotes(drutama("kmcustomtext8")) & "', kmcustomtext9  = '" & FixQuotes(drutama("kmcustomtext9")) & "', kmcustomtext10  = '" & FixQuotes(drutama("kmcustomtext10")) & "', kmcustomtext11  = '" & FixQuotes(drutama("kmcustomtext11")) & "', kmcustomtext12  = '" & FixQuotes(drutama("kmcustomtext12")) & "', kmcustomtext13  = '" & FixQuotes(drutama("kmcustomtext13")) & "', kmcustomtext14  = '" & FixQuotes(drutama("kmcustomtext14")) & "', kmcustomtext15  = '" & FixQuotes(drutama("kmcustomtext15")) & "', kmcustomtext16  = '" & FixQuotes(drutama("kmcustomtext16")) & "', kmcustomtext17  = '" & FixQuotes(drutama("kmcustomtext17")) & "', kmcustomtext18  = '" & FixQuotes(drutama("kmcustomtext18")) & "', kmcustomtext19  = '" & FixQuotes(drutama("kmcustomtext19")) & "', kmcustomtext20  = '" & FixQuotes(drutama("kmcustomtext20")) & "', kmcustomint1  = " & drutama("kmcustomint1") & ", kmcustomint2  = " & drutama("kmcustomint2") & ", kmcustomint3  = " & drutama("kmcustomint3") & ", kmcustomint4  = " & drutama("kmcustomint4") & ", kmcustomint5  = " & drutama("kmcustomint5") & ", kmcustomint6  = " & drutama("kmcustomint6") & ", kmcustomint7  = " & drutama("kmcustomint7") & ", kmcustomint8  = " & drutama("kmcustomint8") & ", kmcustomint9  = " & drutama("kmcustomint9") & ", kmcustomint10  = " & drutama("kmcustomint10") & ", kmcustomint11  = " & drutama("kmcustomint11") & ", kmcustomint12  = " & drutama("kmcustomint12") & ", kmcustomint13  = " & drutama("kmcustomint13") & ", kmcustomint14  = " & drutama("kmcustomint14") & ", kmcustomint15  = " & drutama("kmcustomint15") & ", kmcustomint16  = " & drutama("kmcustomint16") & ", kmcustomint17  = " & drutama("kmcustomint17") & ", kmcustomint18  = " & drutama("kmcustomint18") & ", kmcustomint19  = " & drutama("kmcustomint19") & ", kmcustomint20  = " & drutama("kmcustomint20") & ", kmcustomdbl1  = '" & FixDouble(drutama("kmcustomdbl1")) & "', kmcustomdbl2  = '" & FixDouble(drutama("kmcustomdbl2")) & "', kmcustomdbl3  = '" & FixDouble(drutama("kmcustomdbl3")) & "', kmcustomdbl4  = '" & FixDouble(drutama("kmcustomdbl4")) & "', kmcustomdbl5  = '" & FixDouble(drutama("kmcustomdbl5")) & "', kmcustomdbl6  = '" & FixDouble(drutama("kmcustomdbl6")) & "', kmcustomdbl7  = '" & FixDouble(drutama("kmcustomdbl7")) & "', kmcustomdbl8  = '" & FixDouble(drutama("kmcustomdbl8")) & "', kmcustomdbl9  = '" & FixDouble(drutama("kmcustomdbl9")) & "', kmcustomdbl10  = '" & FixDouble(drutama("kmcustomdbl10")) & "', kmcustomdbl11  = '" & FixDouble(drutama("kmcustomdbl11")) & "', kmcustomdbl12  = '" & FixDouble(drutama("kmcustomdbl12")) & "', kmcustomdbl13  = '" & FixDouble(drutama("kmcustomdbl13")) & "', kmcustomdbl14  = '" & FixDouble(drutama("kmcustomdbl14")) & "', kmcustomdbl15  = '" & FixDouble(drutama("kmcustomdbl15")) & "', kmcustomdbl16  = '" & FixDouble(drutama("kmcustomdbl16")) & "', kmcustomdbl17  = '" & FixDouble(drutama("kmcustomdbl17")) & "', kmcustomdbl18  = '" & FixDouble(drutama("kmcustomdbl18")) & "', kmcustomdbl19  = '" & FixDouble(drutama("kmcustomdbl19")) & "', kmcustomdbl20  = '" & FixDouble(drutama("kmcustomdbl20")) & "', kmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate1"))) & "', kmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate2"))) & "', kmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate3"))) & "', kmcustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate4"))) & "', kmcustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate5"))) & "', kmcustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate6"))) & "', kmcustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate7"))) & "', kmcustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate8"))) & "', kmcustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate9"))) & "', kmcustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate10"))) & "', kmcustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate11"))) & "', kmcustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate12"))) & "', kmcustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate13"))) & "', kmcustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate14"))) & "', kmcustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate15"))) & "', kmcustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate16"))) & "', kmcustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate17"))) & "', kmcustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate18"))) & "', kmcustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate19"))) & "', kmcustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate20"))) & "', kmperawatan  = '" & FixQuotes(drutama("kmperawatan")) & "', kmkategoripasien  = '" & FixQuotes(drutama("kmkategoripasien")) & "' where kmid = '" & drutama("kmid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'update status kamar jika transaksi approve
                        If drutama("kmstatus") = 2 Then
                            'CEK STATUS KAMAR  ======================
                            Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDBCon("SELECT kjstatuskamar, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("kmidkj") & "'", myConn)
                            Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                            If cekKunjungan > 0 Then
                                result(2) = "No KJ. : '" & dtCekKunjungan.Rows(0)(1) & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK STATUS KAMAR ===============

                            'UPDATE STATUS KAMAR KJ =================
                            sql = "Update M_11_Kj set kjstatuskamar = 1 where kjid = '" & drutama("kmidkj") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                            'END OF UPDATE STATUS KAMAR KJ ==========

                            'UPDATE STATUS ISCLOSE KASUR =================
                            sql = "Update M1_bed set bisclose = 1 where bkode = '" & drutama("kmkasur") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                            'END OF STATUS ISCLOSE KASUR =================

                            'UPDATE STATUS ISCLOSE KAMAR ======================
                            'cek status isclose kasur
                            Dim dtCekJmlKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "'", myConn)
                            Dim cekJmlKasur As Double = Val(dtCekJmlKasur.Rows(0)(0))
                            Dim dtCekIscloseKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "' AND bisclose = 1", myConn)
                            Dim cekIscloseKasur As Double = Val(dtCekIscloseKasur.Rows(0)(0))
                            If cekIscloseKasur >= cekJmlKasur Then
                                'update status isclose kamar
                                sql = "Update M1_room set risclose = 1 where rkode = '" & drutama("kmkamar") & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If
                            'END OF UPDATE STATUS ISCLOSE KAMAR ===============
                        End If
                    Else
                        result(2) = "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    If drutama("kmautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_NotransaksiKJ(drutama("kmperawatan"), drutama("kmawalankatpasien"), drutama("kmsumber"), drutama("kmtgl"))
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
                        notransaksi = drutama("kmnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(kmid) FROM m_11_km WHERE kmnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M_11_Km (kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmrekpersediaan, kmrekhargapokok, kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2, kmcustomdate3, kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20, kmperawatan, kmkategoripasien) values('" & FixQuotes(drutama("kmcabang")) & "', '" & FixQuotes(drutama("kmlokasi")) & "', '" & FixQuotes(drutama("kmgudang")) & "', '" & FixQuotes(drutama("kmsumber")) & "', " & drutama("kmautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmtgl"))) & "', " & drutama("kmkodepa") & ", " & drutama("kmcustomer") & ", '" & FixQuotes(drutama("kmcustomerkontak")) & "', '" & FixQuotes(drutama("kmuraian")) & "', '" & FixQuotes(drutama("kmcatatan")) & "', '" & FixQuotes(drutama("kmnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmtglnoref"))) & "', '" & FixQuotes(drutama("kmmatauang")) & "', '" & FixDouble(drutama("kmkurs")) & "', " & drutama("kmidkj") & ", '" & FixQuotes(drutama("kmkamar")) & "', '" & FixQuotes(drutama("kmkasur")) & "', '" & FixQuotes(drutama("kmtglmasuk")) & "', '" & FixQuotes(drutama("kmtglkeluar")) & "', '" & FixDouble(drutama("kmjmlhari")) & "', '" & FixDouble(drutama("kmharga")) & "', '" & FixDouble(drutama("kmtotaltransaksi")) & "', '" & FixQuotes(drutama("kmrekpersediaan")) & "', '" & FixQuotes(drutama("kmrekhargapokok")) & "', '" & FixQuotes(drutama("kmrekdiskonpenjualan")) & "', '" & FixQuotes(drutama("kmrekpenjualan")) & "', " & drutama("kmstatusrealisasi") & ", " & drutama("kmstatus") & ", " & drutama("kmstatussebelumnya") & ", " & drutama("kmjmlrevisi") & ", " & drutama("kmcetakanke") & ", " & drutama("kminputuser") & ", NOW(), " & drutama("kmmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("kmisclose") & ", '" & FixQuotes(drutama("kmcustomtext1")) & "', '" & FixQuotes(drutama("kmcustomtext2")) & "', '" & FixQuotes(drutama("kmcustomtext3")) & "', '" & FixQuotes(drutama("kmcustomtext4")) & "', '" & FixQuotes(drutama("kmcustomtext5")) & "', '" & FixQuotes(drutama("kmcustomtext6")) & "', '" & FixQuotes(drutama("kmcustomtext7")) & "', '" & FixQuotes(drutama("kmcustomtext8")) & "', '" & FixQuotes(drutama("kmcustomtext9")) & "', '" & FixQuotes(drutama("kmcustomtext10")) & "', '" & FixQuotes(drutama("kmcustomtext11")) & "', '" & FixQuotes(drutama("kmcustomtext12")) & "', '" & FixQuotes(drutama("kmcustomtext13")) & "', '" & FixQuotes(drutama("kmcustomtext14")) & "', '" & FixQuotes(drutama("kmcustomtext15")) & "', '" & FixQuotes(drutama("kmcustomtext16")) & "', '" & FixQuotes(drutama("kmcustomtext17")) & "', '" & FixQuotes(drutama("kmcustomtext18")) & "', '" & FixQuotes(drutama("kmcustomtext19")) & "', '" & FixQuotes(drutama("kmcustomtext20")) & "', " & drutama("kmcustomint1") & ", " & drutama("kmcustomint2") & ", " & drutama("kmcustomint3") & ", " & drutama("kmcustomint4") & ", " & drutama("kmcustomint5") & ", " & drutama("kmcustomint6") & ", " & drutama("kmcustomint7") & ", " & drutama("kmcustomint8") & ", " & drutama("kmcustomint9") & ", " & drutama("kmcustomint10") & ", " & drutama("kmcustomint11") & ", " & drutama("kmcustomint12") & ", " & drutama("kmcustomint13") & ", " & drutama("kmcustomint14") & ", " & drutama("kmcustomint15") & ", " & drutama("kmcustomint16") & ", " & drutama("kmcustomint17") & ", " & drutama("kmcustomint18") & ", " & drutama("kmcustomint19") & ", " & drutama("kmcustomint20") & ", '" & FixDouble(drutama("kmcustomdbl1")) & "', '" & FixDouble(drutama("kmcustomdbl2")) & "', '" & FixDouble(drutama("kmcustomdbl3")) & "', '" & FixDouble(drutama("kmcustomdbl4")) & "', '" & FixDouble(drutama("kmcustomdbl5")) & "', '" & FixDouble(drutama("kmcustomdbl6")) & "', '" & FixDouble(drutama("kmcustomdbl7")) & "', '" & FixDouble(drutama("kmcustomdbl8")) & "', '" & FixDouble(drutama("kmcustomdbl9")) & "', '" & FixDouble(drutama("kmcustomdbl10")) & "', '" & FixDouble(drutama("kmcustomdbl11")) & "', '" & FixDouble(drutama("kmcustomdbl12")) & "', '" & FixDouble(drutama("kmcustomdbl13")) & "', '" & FixDouble(drutama("kmcustomdbl14")) & "', '" & FixDouble(drutama("kmcustomdbl15")) & "', '" & FixDouble(drutama("kmcustomdbl16")) & "', '" & FixDouble(drutama("kmcustomdbl17")) & "', '" & FixDouble(drutama("kmcustomdbl18")) & "', '" & FixDouble(drutama("kmcustomdbl19")) & "', '" & FixDouble(drutama("kmcustomdbl20")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate10"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate11"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate12"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate13"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate14"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate15"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate16"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate17"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate18"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate19"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate20"))) & "', '" & FixQuotes(drutama("kmperawatan")) & "', '" & FixQuotes(drutama("kmkategoripasien")) & "')"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()


                    If drutama("kmstatus") = 2 Then
                        'UPDATE STATUS KAMAR KJ =================
                        sql = "Update M_11_Kj set kjstatuskamar  = 1 where kjid = '" & drutama("kmidkj") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'END OF UPDATE STATUS KAMAR KJ =================

                        'UPDATE STATUS ISCLOSE KASUR =================
                        sql = "Update M1_bed set bisclose = 1 where bkode = '" & drutama("kmkasur") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'END OF STATUS ISCLOSE KASUR =================

                        'UPDATE STATUS ISCLOSE KAMAR ======================
                        'cek status isclose kasur
                        Dim dtCekJmlKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "'", myConn)
                        Dim cekJmlKasur As Double = Val(dtCekJmlKasur.Rows(0)(0))
                        Dim dtCekIscloseKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "' AND bisclose = 1", myConn)
                        Dim cekIscloseKasur As Double = Val(dtCekIscloseKasur.Rows(0)(0))
                        If cekIscloseKasur >= cekJmlKasur Then
                            'update status isclose kamar
                            sql = "Update M1_room set risclose = 1 where rkode = '" & drutama("kmkamar") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE STATUS ISCLOSE KAMAR ===============
                    End If

                    Dim dt2 As New DataTable

                    'Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                    'dt2 = AsDataTableAmbilDariDBCon("select kmid from M_11_km where kmnotransaksi='" & notransaksi & "' AND kminputuser= '" & userid & "' order by kmmodifikasitgl desc limit 1")
                    'If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "KM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M11_KmUpdateStatus(ByVal param As String) As String
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
            Filter = Filter.Replace("kmnotransaksikj", "kj.kjnotransaksi")
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
            Dim sumber As String = "Km", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0, idkj As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Kmtgl, Kmnotransaksi, Kmstatus, kmidkj FROM M_11_Km WHERE Kmid='" & idtransaksi & "'", myConn)
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
                nilaiStatus = "Kmstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m11_km_history
            Dim kmSimpanHistory As String = SimpanHistory.M11_Km_HistorySimpan("" & paramSplit(0) & "★M11_Km_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim kmSplit() As String = kmSimpanHistory.Split(sptParam)
            Dim kmSplitResult() As String = kmSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (kmSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & kmSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then

                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.m5_so_terkait("kmid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                'UPDATE STATUS KJ ===============================================================
                'CEK TRANSAKSI TERKAIT KJ
                sql = "  SELECT * FROM ( "
                sql &= " SELECT a.akid as idterkait, a.aksumber as sumber FROM m_11_ak a JOIN m_11_kj kj ON a.akidkj = kj.kjid AND a.akstatus IN(2,3,4,7) AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
                sql &= " UNION ALL "
                sql &= " SELECT a.kmid as idterkait, a.kmsumber as sumber FROM m_11_km a JOIN m_11_kj kj ON a.kmidkj = kj.kjid AND a.kmstatus IN(2,3,4,7) AND a.kmid <> '" & FixDouble(idtransaksi) & "' AND kj.kjid = '" & FixDouble(idkj) & "' GROUP BY kj.kjid "
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

                'Dim idlayanan As Integer = 0, jmltotal As Double = 0, idkjdetail As Integer = 0
                'Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

            End If

            'update status utama
            sql = "UPDATE M_11_Km SET Kmstatus = " & nilaiStatus & ", Kmmodifikasiuser='" & userid & "', Kmmodifikasitgl = NOW(), Kmjmlrevisi = Kmjmlrevisi + 1 WHERE Kmid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'cek ketika jadikan draft
            If jnsaktivitas = 0 Then
                'UPDATE STATUS KAMAR KJ =================
                Dim dtCekIdkj As DataTable = AsDataTableAmbilDariDBCon("SELECT kmidkj, kmkamar, kmkasur FROM m_11_km WHERE kmid='" & idtransaksi & "'", myConn)
                Dim cekIdkj As Double = Val(dtCekIdkj.Rows(0)(0))
                Dim cekKodeKamar As String = dtCekIdkj.Rows(0)(1)
                Dim cekKodeKasur As String = dtCekIdkj.Rows(0)(2)
                sql = "Update M_11_Kj set kjstatuskamar  = 0 where kjid = '" & cekIdkj & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE STATUS KAMAR KJ =================

                'UPDATE STATUS ISCLOSE KASUR =================
                sql = "Update M1_bed set bisclose = 0 where bkode = '" & cekKodeKasur & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE STATUS ISCLOSE KASUR =================

                'UPDATE STATUS ISCLOSE KAMAR ======================
                'cek status isclose kasur
                Dim dtCekJmlKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & cekKodeKamar & "'", myConn)
                Dim cekJmlKasur As Double = Val(dtCekJmlKasur.Rows(0)(0))
                Dim dtCekIscloseKasur As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & cekKodeKamar & "' AND bisclose = 0", myConn)
                Dim cekIscloseKasur As Double = Val(dtCekIscloseKasur.Rows(0)(0))
                If cekIscloseKasur <= cekJmlKasur Then
                    'update status isclose kamar
                    sql = "Update M1_room set risclose = 0 where rkode = '" & cekKodeKamar & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STATUS ISCLOSE KAMAR ===============
            End If


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
            Dim paramSearch As String = M11_KmSearch(PostWsSearch(paramSplit(0), "M11_KmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_KmDelete(ByVal param As String) As String

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
            Dim sumber As String = "Km", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Kmid, Kmnotransaksi FROM M_11_Km WHERE Kmid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT kmcabang, kmlokasi, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl"
            sql &= " FROM M_11_km"
            sql &= " WHERE kmid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("kmcabang")
                lokasi = dtNomorNext.Rows(0)("kmlokasi")
                sumber = dtNomorNext.Rows(0)("kmsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("kmautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("kmnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("kmtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE UTAMA
            sql = "DELETE FROM M_11_Km WHERE kmid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M11_KmSearch(PostWsSearch(paramSplit(0), "M11_KmSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M11_KmGetdataById(ByVal param As String) As String
        'M11_Km_GetdataById Utama --------------------------------------------------------
        'kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer,
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref,
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar,
        'kmjmlhari, kmharga, kmtotaltransaksi, kmrekpersediaan, kmrekhargapokok, 
        'kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, kmstatus,
        'kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl,
        'kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, kmcustomtext1, kmcustomtext2,
        'kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7,
        'kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12,
        'kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17,
        'kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2,
        'kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7,
        'kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12,
        'kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17,
        'kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2,
        'kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7,
        'kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12,
        'kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17,
        'kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2,
        'kmcustomdate3, kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7,
        'kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12,
        'kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17,
        'kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama,
        'kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama,
        'kmkasurnama, kmstatusnama, kmstatussebelumnyanama, kminputusernama, kmmodifikasiusernama,
        'kmtingkatjual, kmperawatan, kmkategoripasien, kmkategoripasiennama

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

        Dim NmMemcached As String = "aplikasi1-M11_Km~M11_Km_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "kmid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "kmid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_km_getdata")

        dt = AmbilData("aplikasi1-M11_km_getdata", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kmid"), 0), sptField,
                     FxDB(drutama("kmcabang"), ""), sptField,
                     FxDB(drutama("kmlokasi"), ""), sptField,
                     FxDB(drutama("kmgudang"), ""), sptField,
                     FxDB(drutama("kmsumber"), ""), sptField,
                     FxDB(drutama("kmautonotransaksi"), 0), sptField,
                     FxDB(drutama("kmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kmtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("kmkodepa"), 0), sptField,
                     FxDB(drutama("kmcustomer"), 0), sptField,
                     FxDB(drutama("kmcustomerkontak"), ""), sptField,
                     FxDB(drutama("kmuraian"), ""), sptField,
                     FxDB(drutama("kmcatatan"), ""), sptField,
                     FxDB(drutama("kmnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("kmtglnoref"), ""), formatTgl), sptField,
                     FxDB(drutama("kmmatauang"), ""), sptField,
                     FxDB(drutama("kmkurs"), 0), sptField,
                     FxDB(drutama("kmidkj"), 0), sptField,
                     FxDB(drutama("kmkamar"), ""), sptField,
                     FxDB(drutama("kmkasur"), ""), sptField,
                     FxDB(drutama("kmtglmasuk"), ""), sptField,
                     FxDB(drutama("kmtglkeluar"), ""), sptField,
                     FxDB(drutama("kmjmlhari"), 0), sptField,
                     FxDB(drutama("kmharga"), 0), sptField,
                     FxDB(drutama("kmtotaltransaksi"), 0), sptField,
                     FxDB(drutama("kmrekpersediaan"), ""), sptField,
                     FxDB(drutama("kmrekhargapokok"), ""), sptField,
                     FxDB(drutama("kmrekdiskonpenjualan"), ""), sptField,
                     FxDB(drutama("kmrekpenjualan"), ""), sptField,
                     FxDB(drutama("kmstatusrealisasi"), 0), sptField,
                     FxDB(drutama("kmstatus"), 0), sptField,
                     FxDB(drutama("kmstatussebelumnya"), 0), sptField,
                     FxDB(drutama("kmjmlrevisi"), 0), sptField,
                     FxDB(drutama("kmcetakanke"), 0), sptField,
                     FxDB(drutama("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kmposting"), 0), sptField,
                     FxDB(drutama("kmisclose"), 0), sptField,
                     FxDB(drutama("kmcustomtext1"), ""), sptField,
                     FxDB(drutama("kmcustomtext2"), ""), sptField,
                     FxDB(drutama("kmcustomtext3"), ""), sptField,
                     FxDB(drutama("kmcustomtext4"), ""), sptField,
                     FxDB(drutama("kmcustomtext5"), ""), sptField,
                     FxDB(drutama("kmcustomtext6"), ""), sptField,
                     FxDB(drutama("kmcustomtext7"), ""), sptField,
                     FxDB(drutama("kmcustomtext8"), ""), sptField,
                     FxDB(drutama("kmcustomtext9"), ""), sptField,
                     FxDB(drutama("kmcustomtext10"), ""), sptField,
                     FxDB(drutama("kmcustomtext11"), ""), sptField,
                     FxDB(drutama("kmcustomtext12"), ""), sptField,
                     FxDB(drutama("kmcustomtext13"), ""), sptField,
                     FxDB(drutama("kmcustomtext14"), ""), sptField,
                     FxDB(drutama("kmcustomtext15"), ""), sptField,
                     FxDB(drutama("kmcustomtext16"), ""), sptField,
                     FxDB(drutama("kmcustomtext17"), ""), sptField,
                     FxDB(drutama("kmcustomtext18"), ""), sptField,
                     FxDB(drutama("kmcustomtext19"), ""), sptField,
                     FxDB(drutama("kmcustomtext20"), ""), sptField,
                     FxDB(drutama("kmcustomint1"), 0), sptField,
                     FxDB(drutama("kmcustomint2"), 0), sptField,
                     FxDB(drutama("kmcustomint3"), 0), sptField,
                     FxDB(drutama("kmcustomint4"), 0), sptField,
                     FxDB(drutama("kmcustomint5"), 0), sptField,
                     FxDB(drutama("kmcustomint6"), 0), sptField,
                     FxDB(drutama("kmcustomint7"), 0), sptField,
                     FxDB(drutama("kmcustomint8"), 0), sptField,
                     FxDB(drutama("kmcustomint9"), 0), sptField,
                     FxDB(drutama("kmcustomint10"), 0), sptField,
                     FxDB(drutama("kmcustomint11"), 0), sptField,
                     FxDB(drutama("kmcustomint12"), 0), sptField,
                     FxDB(drutama("kmcustomint13"), 0), sptField,
                     FxDB(drutama("kmcustomint14"), 0), sptField,
                     FxDB(drutama("kmcustomint15"), 0), sptField,
                     FxDB(drutama("kmcustomint16"), 0), sptField,
                     FxDB(drutama("kmcustomint17"), 0), sptField,
                     FxDB(drutama("kmcustomint18"), 0), sptField,
                     FxDB(drutama("kmcustomint19"), 0), sptField,
                     FxDB(drutama("kmcustomint20"), 0), sptField,
                     FxDB(drutama("kmcustomdbl1"), 0), sptField,
                     FxDB(drutama("kmcustomdbl2"), 0), sptField,
                     FxDB(drutama("kmcustomdbl3"), 0), sptField,
                     FxDB(drutama("kmcustomdbl4"), 0), sptField,
                     FxDB(drutama("kmcustomdbl5"), 0), sptField,
                     FxDB(drutama("kmcustomdbl6"), 0), sptField,
                     FxDB(drutama("kmcustomdbl7"), 0), sptField,
                     FxDB(drutama("kmcustomdbl8"), 0), sptField,
                     FxDB(drutama("kmcustomdbl9"), 0), sptField,
                     FxDB(drutama("kmcustomdbl10"), 0), sptField,
                     FxDB(drutama("kmcustomdbl11"), 0), sptField,
                     FxDB(drutama("kmcustomdbl12"), 0), sptField,
                     FxDB(drutama("kmcustomdbl13"), 0), sptField,
                     FxDB(drutama("kmcustomdbl14"), 0), sptField,
                     FxDB(drutama("kmcustomdbl15"), 0), sptField,
                     FxDB(drutama("kmcustomdbl16"), 0), sptField,
                     FxDB(drutama("kmcustomdbl17"), 0), sptField,
                     FxDB(drutama("kmcustomdbl18"), 0), sptField,
                     FxDB(drutama("kmcustomdbl19"), 0), sptField,
                     FxDB(drutama("kmcustomdbl20"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate10"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate11"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate12"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate13"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate14"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate15"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate16"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate17"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate18"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate19"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kmcustomdate20"), ""), formatTgl), sptField,
                     FxDB(drutama("kmcabangnama"), ""), sptField,
                     FxDB(drutama("kmlokasinama"), ""), sptField,
                     FxDB(drutama("kmgudangnama"), ""), sptField,
                     FxDB(drutama("kmcustomerkode"), ""), sptField,
                     FxDB(drutama("kmcustomernama"), ""), sptField,
                     FxDB(drutama("kmnotransaksikj"), ""), sptField,
                     FxDB(drutama("kmkamarnama"), ""), sptField,
                     FxDB(drutama("kmkasurnama"), ""), sptField,
                     FxDB(drutama("kmstatusnama"), ""), sptField,
                     FxDB(drutama("kmstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("kminputusernama"), ""), sptField,
                     FxDB(drutama("kmmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kmtingkatjual"), ""), sptField,
                     FxDB(drutama("kmperawatan"), ""), sptField,
                     FxDB(drutama("kmkategoripasien"), ""), sptField,
                     FxDB(drutama("kmkategoripasiennama"), ""))

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmrekpersediaan, kmrekhargapokok, kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, kmcustomdate1, kmcustomdate2, kmcustomdate3,  kmcustomdate4, kmcustomdate5, kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebelumnyanama, kminputusernama, kmmodifikasiusernama, kmtingkatjual, kmperawatan, kmkategoripasien, kmkategoripasiennama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KmSearch(ByVal param As String) As String
        'M11_KmSearch --------------------------------------------------------
        'kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer,
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref,
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, 
        'kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi,
        'kmstatusrealisasi, kmstatus, kmstatussebekmmnya, kmjmlrevisi, kmcetakanke,
        'kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose,
        'kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama,
        'kmnotransaksikj, kmkamarnama, kmmasurnama, kmstatusnama, kmstatussebelumnyanama,
        'kminputusernama, kmmodifikasiusernama, kmperawatan, kmkategoripasien

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
            Filter = Filter.Replace("kmnotransaksikj", "kj.kjnotransaksi")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m11_km_v")

        dt = AmbilData("aplikasi1-M11_km_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kmid"), 0), sptField,
                     FxDB(dr("kmcabang"), ""), sptField,
                     FxDB(dr("kmlokasi"), ""), sptField,
                     FxDB(dr("kmgudang"), ""), sptField,
                     FxDB(dr("kmsumber"), ""), sptField,
                     FxDB(dr("kmautonotransaksi"), 0), sptField,
                     FxDB(dr("kmnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtgl"), ""), formatTgl), sptField,
                     FxDB(dr("kmkodepa"), 0), sptField,
                     FxDB(dr("kmcustomer"), 0), sptField,
                     FxDB(dr("kmcustomerkontak"), ""), sptField,
                     FxDB(dr("kmuraian"), ""), sptField,
                     FxDB(dr("kmcatatan"), ""), sptField,
                     FxDB(dr("kmnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("kmmatauang"), ""), sptField,
                     FxDB(dr("kmkurs"), 0), sptField,
                     FxDB(dr("kmidkj"), 0), sptField,
                     FxDB(dr("kmkamar"), ""), sptField,
                     FxDB(dr("kmkasur"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglmasuk"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("kmtglkeluar"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmjmlhari"), 0), sptField,
                     FxDB(dr("kmharga"), 0), sptField,
                     FxDB(dr("kmtotaltransaksi"), 0), sptField,
                     FxDB(dr("kmstatusrealisasi"), 0), sptField,
                     FxDB(dr("kmstatus"), 0), sptField,
                     FxDB(dr("kmstatussebelumnya"), 0), sptField,
                     FxDB(dr("kmjmlrevisi"), 0), sptField,
                     FxDB(dr("kmcetakanke"), 0), sptField,
                     FxDB(dr("kminputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kminputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kmmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmisclose"), 0), sptField,
                     FxDB(dr("kmcabangnama"), ""), sptField,
                     FxDB(dr("kmlokasinama"), ""), sptField,
                     FxDB(dr("kmgudangnama"), ""), sptField,
                     FxDB(dr("kmcustomerkode"), ""), sptField,
                     FxDB(dr("kmcustomernama"), ""), sptField,
                     FxDB(dr("kmnotransaksikj"), ""), sptField,
                     FxDB(dr("kmkamarnama"), ""), sptField,
                     FxDB(dr("kmkasurnama"), ""), sptField,
                     FxDB(dr("kmstatusnama"), ""), sptField,
                     FxDB(dr("kmstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("kminputusernama"), ""), sptField,
                     FxDB(dr("kmmodifikasiusernama"), ""),
                     FxDB(dr("kmperawatan"), ""),
                     FxDB(dr("kmkategoripasien"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmid, kmcabang, kmlokasi, kmgudang, kmsumber, kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, kmstatusrealisasi, kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmisclose, kmcabangnama, kmlokasinama, kmgudangnama, kmcustomerkode, kmcustomernama, kmnotransaksikj, kmkamarnama, kmkasurnama, kmstatusnama, kmstatussebekmmnyanama, kminputusernama, kmmodifikasiusernama, kmperawatan, kmkategoripasien"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KmTerkait(ByVal param As String) As String
        'M11_KmTerkait --------------------------------------------------------
        'kmid, kmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m11_km_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-m11_km_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kmid"), 0), sptField,
                     FxDB(dr("kmnotransaksi"), ""), sptField,
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
            result(2) = "Related KM data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kmid, kmnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M11_KmKeluarKamar(ByVal param As String) As String
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
        'kmid(0) As Integer, kmcabang(1) As String, kmlokasi(2) As String, kmgudang(3) As String, kmsumber(4) As String, 
        'kmautonotransaksi(5) As Integer, kmnotransaksi(6) As String, kmtgl(7) As Date, kmkodepa(8) As Integer, kmcustomer(9) As Integer, 
        'kmcustomerkontak(10) As String, kmuraian(11) As String, kmcatatan(12) As String, kmnoref(13) As String, kmtglnoref(14) As Date, 
        'kmmatauang(15) As String, kmkurs(16) As Double, kmidkj(17) As Integer, kmkamar(18) As String, kmkasur(19) As String, 
        'kmtglmasuk(20) As DateTime, kmtglkeluar(21) As DateTime, kmjmlhari(22) As Integer, kmharga(23) As Double, kmtotaltransaksi(24) As Double, 
        'kmrekpersediaan(25) As String, kmrekhargapokok(26) As String, kmrekdiskonpenjualan(27) As String, kmrekpenjualan(28) As String, kmstatusrealisasi(29) As Interger, 
        'kmstatus(30) As Integer, kmstatussebelumnya(31) As Integer, kmjmlrevisi(32) As Integer, kmcetakanke(33) As Integer, kminputuser(34) As Integer, 
        'kminputtgl(35) As DateTime, kmmodifikasiuser(36) As Integer, kmmodifikasitgl(37) As DateTime, kmposting(38) As Integer, kmisclose(39) As Integer, 
        'kmcustomtext1(40) As String, kmcustomtext2(41) As String, kmcustomtext3(42) As String, kmcustomtext4(43) As String, kmcustomtext5(44) As String, 
        'kmcustomtext6(45) As String, kmcustomtext7(46) As String, kmcustomtext8(47) As String, kmcustomtext9(48) As String, kmcustomtext10(49) As String, 
        'kmcustomtext11(50) As String, kmcustomtext12(51) As String, kmcustomtext13(52) As String, kmcustomtext14(53) As String, kmcustomtext15(54) As String, 
        'kmcustomtext16(55) As String, kmcustomtext17(56) As String, kmcustomtext18(57) As String, kmcustomtext19(58) As String, kmcustomtext20(59) As String, 
        'kmcustomint1(60) As Integer, kmcustomint2(61) As Integer, kmcustomint3(62) As Integer, kmcustomint4(63) As Integer, kmcustomint5(64) As Integer, 
        'kmcustomint6(65) As Integer, kmcustomint7(66) As Integer, kmcustomint8(67) As Integer, kmcustomint9(68) As Integer, kmcustomint10(69) As Integer, 
        'kmcustomint11(70) As Integer, kmcustomint12(71) As Integer, kmcustomint13(72) As Integer, kmcustomint14(73) As Integer, kmcustomint15(74) As Integer, 
        'kmcustomint16(75) As Integer, kmcustomint17(76) As Integer, kmcustomint18(77) As Integer, kmcustomint19(78) As Integer, kmcustomint20(79) As Integer, 
        'kmcustomdbl1(80) As Double, kmcustomdbl2(81) As Double, kmcustomdbl3(82) As Double, kmcustomdbl4(83) As Double, kmcustomdbl5(84) As Double, 
        'kmcustomdbl6(85) As Double, kmcustomdbl7(86) As Double, kmcustomdbl8(87) As Double, kmcustomdbl9(88) As Double, kmcustomdbl10(89) As Double, 
        'kmcustomdbl11(90) As Double, kmcustomdbl12(91) As Double, kmcustomdbl13(92) As Double, kmcustomdbl14(93) As Double, kmcustomdbl15(94) As Double, 
        'kmcustomdbl16(95) As Double, kmcustomdbl17(96) As Double, kmcustomdbl18(97) As Double, kmcustomdbl19(98) As Double, kmcustomdbl20(99) As Double, 
        'kmcustomdate1(100) As Date, kmcustomdate2(101) As Date, kmcustomdate3(102) As Date, kmcustomdate4(103) As Date, kmcustomdate5(104) As Date, 
        'kmcustomdate6(105) As Date, kmcustomdate7(106) As Date, kmcustomdate8(107) As Date, kmcustomdate9(108) As Date, kmcustomdate10(109) As Date, 
        'kmcustomdate11(110) As Date, kmcustomdate12(111) As Date, kmcustomdate13(112) As Date, kmcustomdate14(113) As Date, kmcustomdate15(114) As Date, 
        'kmcustomdate16(115) As Date, kmcustomdate17(116) As Date, kmcustomdate18(117) As Date, kmcustomdate19(118) As Date, kmcustomdate20(119) As Date
        'kmpelanggan(120) As String, kmkategoripasien(121) As String


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kmid, kmcabang, kmlokasi, kmgudang, kmsumber, 
        'kmautonotransaksi, kmnotransaksi, kmtgl, kmkodepa, kmcustomer, 
        'kmcustomerkontak, kmuraian, kmcatatan, kmnoref, kmtglnoref, 
        'kmmatauang, kmkurs, kmidkj, kmkamar, kmkasur, 
        'kmtglmasuk, kmtglkeluar, kmjmlhari, kmharga, kmtotaltransaksi, 
        'kmrekpersediaan, kmrekhargapokok, kmrekdiskonpenjualan, kmrekpenjualan, kmstatusrealisasi, 
        'kmstatus, kmstatussebelumnya, kmjmlrevisi, kmcetakanke, kminputuser, 
        'kminputtgl, kmmodifikasiuser, kmmodifikasitgl, kmposting, kmisclose, 
        'kmcustomtext1, kmcustomtext2, kmcustomtext3, kmcustomtext4, kmcustomtext5, 
        'kmcustomtext6, kmcustomtext7, kmcustomtext8, kmcustomtext9, kmcustomtext10, 
        'kmcustomtext11, kmcustomtext12, kmcustomtext13, kmcustomtext14, kmcustomtext15, 
        'kmcustomtext16, kmcustomtext17, kmcustomtext18, kmcustomtext19, kmcustomtext20, 
        'kmcustomint1, kmcustomint2, kmcustomint3, kmcustomint4, kmcustomint5, 
        'kmcustomint6, kmcustomint7, kmcustomint8, kmcustomint9, kmcustomint10, 
        'kmcustomint11, kmcustomint12, kmcustomint13, kmcustomint14, kmcustomint15, 
        'kmcustomint16, kmcustomint17, kmcustomint18, kmcustomint19, kmcustomint20, 
        'kmcustomdbl1, kmcustomdbl2, kmcustomdbl3, kmcustomdbl4, kmcustomdbl5, 
        'kmcustomdbl6, kmcustomdbl7, kmcustomdbl8, kmcustomdbl9, kmcustomdbl10, 
        'kmcustomdbl11, kmcustomdbl12, kmcustomdbl13, kmcustomdbl14, kmcustomdbl15, 
        'kmcustomdbl16, kmcustomdbl17, kmcustomdbl18, kmcustomdbl19, kmcustomdbl20, 
        'kmcustomdate1, kmcustomdate2, kmcustomdate3, kmcustomdate4, kmcustomdate5, 
        'kmcustomdate6, kmcustomdate7, kmcustomdate8, kmcustomdate9, kmcustomdate10, 
        'kmcustomdate11, kmcustomdate12, kmcustomdate13, kmcustomdate14, kmcustomdate15, 
        'kmcustomdate16, kmcustomdate17, kmcustomdate18, kmcustomdate19, kmcustomdate20
        'kmpelanggan, kmkategoripasien

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 120) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kmid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kmid required numeric." : GoTo selesai
        End If
        'kmautonotransaksi(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "kmautonotransaksi required numeric." : GoTo selesai
        End If
        'kmtgl(7) As Date
        If (IsDate(dataUtama(7)) = False) Then
            result(2) = "kmtgl required date." : GoTo selesai
        End If
        'kmkodepa(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "kmkodepa required numeric." : GoTo selesai
        End If
        'kmcustomer(9) As Integer
        If (IsNumeric(dataUtama(9)) = False) Then
            result(2) = "kmcustomer required numeric." : GoTo selesai
        End If
        'kmtglnoref(14) As Date
        If (IsDate(dataUtama(14)) = False) Then
            result(2) = "kmtglnoref required date." : GoTo selesai
        End If
        'kmkurs(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "kmkurs required numeric." : GoTo selesai
        End If
        'kmidkj(17) As Integer
        If (IsNumeric(dataUtama(17)) = False) Then
            result(2) = "kmidkj required numeric." : GoTo selesai
        End If
        'kmtglmasuk(20) As Date
        If (IsDate(dataUtama(20)) = False) Then
            result(2) = "kmtglmasuk required date." : GoTo selesai
        End If
        'kmtglkeluar(21) As Date
        If (IsDate(dataUtama(21)) = False) Then
            result(2) = "kmtglkeluar required date." : GoTo selesai
        End If
        'kmjmlhari(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "kmjmlhari required numeric." : GoTo selesai
        End If
        'kmharga(23) As Integer
        If (IsNumeric(dataUtama(23)) = False) Then
            result(2) = "kmharga required numeric." : GoTo selesai
        End If
        'kmtotaltransaksi(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "kmtotaltransaksi required numeric." : GoTo selesai
        End If
        'kmstatusrealisasi(29) As Integer
        If (IsNumeric(dataUtama(29)) = False) Then
            result(2) = "kmstatusrealisasi required numeric." : GoTo selesai
        End If
        'kmstatus(30) As Integer
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "kmstatus required numeric." : GoTo selesai
        End If
        'kmstatussebelumnya(31) As Integer
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "kmstatussebelumnya required numeric." : GoTo selesai
        End If
        'kmjmlrevisi(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "kmjmlrevisi required numeric." : GoTo selesai
        End If
        'kmcetakanke(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "kmcetakanke required numeric." : GoTo selesai
        End If
        'kminputuser(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "kminputuser required numeric." : GoTo selesai
        End If
        'kminputtgl(35) As DateTime
        If (IsDate(dataUtama(35)) = False) Then
            result(2) = "kminputtgl required date." : GoTo selesai
        End If
        'kmmodifikasiuser(36) As Integer
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "kmmodifikasiuser required numeric." : GoTo selesai
        End If
        'kmmodifikasitgl(37) As DateTime
        If (IsDate(dataUtama(37)) = False) Then
            result(2) = "kmmodifikasitgl required date." : GoTo selesai
        End If
        'lmposting(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "kmposting required numeric." : GoTo selesai
        End If
        'kmisclose(39) As Integer
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "kmisclose required numeric." : GoTo selesai
        End If
        'kmcustomint1(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "kmcustomint1 required numeric." : GoTo selesai
        End If
        'kmcustomint2(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "kmcustomint2 required numeric." : GoTo selesai
        End If
        'kmcustomint3(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "kmcustomint3 required numeric." : GoTo selesai
        End If
        'kmcustomint4(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "kmcustomint4 required numeric." : GoTo selesai
        End If
        'kmcustomint5(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "kmcustomint5 required numeric." : GoTo selesai
        End If
        'kmcustomint6(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "kmcustomint6 required numeric." : GoTo selesai
        End If
        'kmcustomint7(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "kmcustomint7 required numeric." : GoTo selesai
        End If
        'kmcustomint8(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "kmcustomint8 required numeric." : GoTo selesai
        End If
        'kmcustomint9(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "kmcustomint9 required numeric." : GoTo selesai
        End If
        'kmcustomint10(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "kmcustomint10 required numeric." : GoTo selesai
        End If
        'kmcustomint11(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "kmcustomint11 required numeric." : GoTo selesai
        End If
        'kmcustomint12(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "kmcustomint12 required numeric." : GoTo selesai
        End If
        'kmcustomint13(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "kmcustomint13 required numeric." : GoTo selesai
        End If
        'kmcustomint14(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "kmcustomint14 required numeric." : GoTo selesai
        End If
        'kmcustomint15(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "kmcustomint15 required numeric." : GoTo selesai
        End If
        'kmcustomint16(75) As Integer
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "kmcustomint16 required numeric." : GoTo selesai
        End If
        'kmcustomint17(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "kmcustomint17 required numeric." : GoTo selesai
        End If
        'kmcustomint18(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "kmcustomint18 required numeric." : GoTo selesai
        End If
        'kmcustomint19(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "kmcustomint19 required numeric." : GoTo selesai
        End If
        'kmcustomint20(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "kmcustomint20 required numeric." : GoTo selesai
        End If
        'kmcustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "kmcustomdbl1 required numeric." : GoTo selesai
        End If
        'kmcustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "kmcustomdbl2 required numeric." : GoTo selesai
        End If
        'kmcustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "kmcustomdbl3 required numeric." : GoTo selesai
        End If
        'kmcustomdbl4(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "kmcustomdbl4 required numeric." : GoTo selesai
        End If
        'kmcustomdbl5(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "kmcustomdbl5 required numeric." : GoTo selesai
        End If
        'kmcustomdbl6(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "kmcustomdbl6 required numeric." : GoTo selesai
        End If
        'kmcustomdbl7(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "kmcustomdbl7 required numeric." : GoTo selesai
        End If
        'kmcustomdbl8(87) As Double
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "kmcustomdbl8 required numeric." : GoTo selesai
        End If
        'kmcustomdbl9(88) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "kmcustomdbl9 required numeric." : GoTo selesai
        End If
        'kmcustomdbl10(89) As Double
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "kmcustomdbl10 required numeric." : GoTo selesai
        End If
        'kmcustomdbl11(90) As Double
        If (IsNumeric(dataUtama(90)) = False) Then
            result(2) = "kmcustomdbl11 required numeric." : GoTo selesai
        End If
        'kmcustomdbl12(91) As Double
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "kmcustomdbl12 required numeric." : GoTo selesai
        End If
        'kmcustomdbl13(92) As Double
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "kmcustomdbl13 required numeric." : GoTo selesai
        End If
        'kmcustomdbl14(93) As Double
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "kmcustomdbl14 required numeric." : GoTo selesai
        End If
        'kmcustomdbl15(94) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "kmcustomdbl15 required numeric." : GoTo selesai
        End If
        'kmcustomdbl16(95) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "kmcustomdbl16 required numeric." : GoTo selesai
        End If
        'kmcustomdbl17(96) As Double
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kmcustomdbl17 required numeric." : GoTo selesai
        End If
        'kmcustomdbl18(97) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kmcustomdbl18 required numeric." : GoTo selesai
        End If
        'kmcustomdbl19(98) As Double
        If (IsNumeric(dataUtama(98)) = False) Then
            result(2) = "kmcustomdbl19 required numeric." : GoTo selesai
        End If
        'kmcustomdbl20(99) As Double
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "kmcustomdbl20 required numeric." : GoTo selesai
        End If
        'kmcustomdate1(100) As Date
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "kmcustomdate1 required date." : GoTo selesai
        End If
        'kmcustomdate2(101) As Date
        If (IsDate(dataUtama(101)) = False) Then
            result(2) = "kmcustomdate2 required date." : GoTo selesai
        End If
        'kmcustomdate3(102) As Date
        If (IsDate(dataUtama(102)) = False) Then
            result(2) = "kmcustomdate3 required date." : GoTo selesai
        End If
        'kmcustomdate4(103) As Date
        If (IsDate(dataUtama(103)) = False) Then
            result(2) = "kmcustomdate4 required date." : GoTo selesai
        End If
        'kmcustomdate5(104) As Date
        If (IsDate(dataUtama(104)) = False) Then
            result(2) = "kmcustomdate5 required date." : GoTo selesai
        End If
        'kmcustomdate6(105) As Date
        If (IsDate(dataUtama(105)) = False) Then
            result(2) = "kmcustomdate6 required date." : GoTo selesai
        End If
        'kmcustomdate7(106) As Date
        If (IsDate(dataUtama(106)) = False) Then
            result(2) = "kmcustomdate7 required date." : GoTo selesai
        End If
        'kmcustomdate8(107) As Date
        If (IsDate(dataUtama(107)) = False) Then
            result(2) = "kmcustomdate8 required date." : GoTo selesai
        End If
        'kmcustomdate9(108) As Date
        If (IsDate(dataUtama(108)) = False) Then
            result(2) = "kmcustomdate9 required date." : GoTo selesai
        End If
        'kmcustomdate10(109) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "kmcustomdate10 required date." : GoTo selesai
        End If
        'kmcustomdate11(110) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "kmcustomdate11 required date." : GoTo selesai
        End If
        'kmcustomdate12(111) As Date
        If (IsDate(dataUtama(111)) = False) Then
            result(2) = "kmcustomdate12 required date." : GoTo selesai
        End If
        'kmcustomdate13(112) As Date
        If (IsDate(dataUtama(112)) = False) Then
            result(2) = "kmcustomdate13 required date." : GoTo selesai
        End If
        'kmcustomdate14(113) As Date
        If (IsDate(dataUtama(113)) = False) Then
            result(2) = "kmcustomdate14 required date." : GoTo selesai
        End If
        'kmcustomdate15(114) As Date
        If (IsDate(dataUtama(114)) = False) Then
            result(2) = "kmcustomdate15 required date." : GoTo selesai
        End If
        'kmcustomdate16(115) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "kmcustomdate16 required date." : GoTo selesai
        End If
        'kmcustomdate17(116) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "kmcustomdate17 required date." : GoTo selesai
        End If
        'kmcustomdate18(117) As Date
        If (IsDate(dataUtama(117)) = False) Then
            result(2) = "kmcustomdate18 required date." : GoTo selesai
        End If
        'kmcustomdate19(118) As Date
        If (IsDate(dataUtama(118)) = False) Then
            result(2) = "kmcustomdate19 required date." : GoTo selesai
        End If
        'kmcustomdate20(119) As Date
        If (IsDate(dataUtama(119)) = False) Then
            result(2) = "kmcustomdate20 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'kmcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kmcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "kmcabang should not be more than 25 character." : GoTo selesai
        End If

        'kmlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "kmlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "kmlokasi should not be more than 25 character." : GoTo selesai
        End If

        'kmgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kmgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "kmgudang should not be more than 25 character." : GoTo selesai
        End If

        'kmsumber(4) As String
        If Len(dataUtama(4)) = 0 Then
            result(2) = "kmsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(4)) > 10 Then
            result(2) = "kmsumber should not be more than 10 character." : GoTo selesai
        End If

        'kmnotransaksi(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "kmnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(6)) > 50 Then
            result(2) = "kmnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'kmtgl(7) As Date
        If Len(dataUtama(7)) = 0 Then
            result(2) = "kmtgl can't be empty" : GoTo selesai
        End If

        'kmtglnoref(14) As Date
        If Len(dataUtama(14)) = 0 Then
            result(2) = "kmtglnoref can't be empty" : GoTo selesai
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

        'kmkamar(18) As String
        If Len(dataUtama(18)) = 0 Then
            result(2) = "kmkamar can't be empty" : GoTo selesai
        End If

        'kmkasur(19) As Date
        If Len(dataUtama(19)) = 0 Then
            result(2) = "kmkasur can't be empty" : GoTo selesai
        End If

        'kmtglmasuk(20) As Date
        If Len(dataUtama(20)) = 0 Then
            result(2) = "kmtglmasuk can't be empty" : GoTo selesai
        End If

        'kmtglkeluar(21) As Date
        If Len(dataUtama(21)) = 0 Then
            result(2) = "kmtglkeluar can't be empty" : GoTo selesai
        End If

        'kmtotaltransaksi(24) As Double
        If Len(dataUtama(24)) = 0 Then
            result(2) = "kmtotaltransaksi can't be empty" : GoTo selesai
        End If

        'kminputtgl(35) As DateTime
        If Len(dataUtama(35)) = 0 Then
            result(2) = "kminputtgl can't be empty" : GoTo selesai
        End If

        'kmmodifikasitgl(37) As DateTime
        If Len(dataUtama(37)) = 0 Then
            result(2) = "kmmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kmcustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "kmcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "kmcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "kmcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl4(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "kmcustomdbl4 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl5(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "kmcustomdbl5 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl6(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "kmcustomdbl6 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl7(86) As Double
        If Len(dataUtama(86)) = 0 Then
            result(2) = "kmcustomdbl7 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl8(87) As Double
        If Len(dataUtama(87)) = 0 Then
            result(2) = "kmcustomdbl8 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl9(88) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "kmcustomdbl9 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl10(89) As Double
        If Len(dataUtama(89)) = 0 Then
            result(2) = "kmcustomdbl10 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl11(90) As Double
        If Len(dataUtama(90)) = 0 Then
            result(2) = "kmcustomdbl11 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl12(91) As Double
        If Len(dataUtama(91)) = 0 Then
            result(2) = "kmcustomdbl12 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl13(92) As Double
        If Len(dataUtama(92)) = 0 Then
            result(2) = "kmcustomdbl13 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl14(93) As Double
        If Len(dataUtama(93)) = 0 Then
            result(2) = "kmcustomdbl14 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl15(94) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "kmcustomdbl15 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl16(95) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "kmcustomdbl16 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl17(96) As Double
        If Len(dataUtama(96)) = 0 Then
            result(2) = "kmcustomdbl17 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl18(97) As Double
        If Len(dataUtama(97)) = 0 Then
            result(2) = "kmcustomdbl18 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl19(98) As Double
        If Len(dataUtama(98)) = 0 Then
            result(2) = "kmcustomdbl19 can't be empty" : GoTo selesai
        End If

        'kmcustomdbl20(99) As Double
        If Len(dataUtama(99)) = 0 Then
            result(2) = "kmcustomdbl20 can't be empty" : GoTo selesai
        End If

        'kmcustomdate1(100) As Date
        If Len(dataUtama(100)) = 0 Then
            result(2) = "kmcustomdate1 can't be empty" : GoTo selesai
        End If

        'kmcustomdate2(101) As Date
        If Len(dataUtama(101)) = 0 Then
            result(2) = "kmcustomdate2 can't be empty" : GoTo selesai
        End If

        'kmcustomdate3(102) As Date
        If Len(dataUtama(102)) = 0 Then
            result(2) = "kmcustomdate3 can't be empty" : GoTo selesai
        End If

        'kmcustomdate4(103) As Date
        If Len(dataUtama(103)) = 0 Then
            result(2) = "kmcustomdate4 can't be empty" : GoTo selesai
        End If

        'kmcustomdate5(104) As Date
        If Len(dataUtama(104)) = 0 Then
            result(2) = "kmcustomdate5 can't be empty" : GoTo selesai
        End If

        'kmcustomdate6(105) As Date
        If Len(dataUtama(105)) = 0 Then
            result(2) = "kmcustomdate6 can't be empty" : GoTo selesai
        End If

        'kmcustomdate7(106) As Date
        If Len(dataUtama(106)) = 0 Then
            result(2) = "kmcustomdate7 can't be empty" : GoTo selesai
        End If

        'kmcustomdate8(107) As Date
        If Len(dataUtama(107)) = 0 Then
            result(2) = "kmcustomdate8 can't be empty" : GoTo selesai
        End If

        'kmcustomdate9(108) As Date
        If Len(dataUtama(108)) = 0 Then
            result(2) = "kmcustomdate9 can't be empty" : GoTo selesai
        End If

        'kmcustomdate10(109) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "kmcustomdate10 can't be empty" : GoTo selesai
        End If

        'kmcustomdate11(110) As Date
        If Len(dataUtama(110)) = 0 Then
            result(2) = "kmcustomdate11 can't be empty" : GoTo selesai
        End If

        'kmcustomdate12(111) As Date
        If Len(dataUtama(111)) = 0 Then
            result(2) = "kmcustomdate12 can't be empty" : GoTo selesai
        End If

        'kmcustomdate13(112) As Date
        If Len(dataUtama(112)) = 0 Then
            result(2) = "kmcustomdate13 can't be empty" : GoTo selesai
        End If

        'kmcustomdate14(113) As Date
        If Len(dataUtama(113)) = 0 Then
            result(2) = "kmcustomdate14 can't be empty" : GoTo selesai
        End If

        'kmcustomdate15(114) As Date
        If Len(dataUtama(114)) = 0 Then
            result(2) = "kmcustomdate15 can't be empty" : GoTo selesai
        End If

        'kmcustomdate16(115) As Date
        If Len(dataUtama(115)) = 0 Then
            result(2) = "kmcustomdate16 can't be empty" : GoTo selesai
        End If

        'kmcustomdate17(116) As Date
        If Len(dataUtama(116)) = 0 Then
            result(2) = "kmcustomdate17 can't be empty" : GoTo selesai
        End If

        'kmcustomdate18(117) As Date
        If Len(dataUtama(117)) = 0 Then
            result(2) = "kmcustomdate18 can't be empty" : GoTo selesai
        End If

        'kmcustomdate19(118) As Date
        If Len(dataUtama(118)) = 0 Then
            result(2) = "kmcustomdate19 can't be empty" : GoTo selesai
        End If

        'kmcustomdate20(119) As Date
        If Len(dataUtama(119)) = 0 Then
            result(2) = "kmcustomdate20 can't be empty" : GoTo selesai
        End If

        'kmperawatan(120) As Date
        If Len(dataUtama(120)) > 10 Then
            result(2) = "kmperawatan should not be more than 10 character." : GoTo selesai
        End If

        'kmkategoripasien(121) As Date
        If Len(dataUtama(121)) > 10 Then
            result(2) = "kmkategoripasien should not be more than 10 character." : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kmid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "kmidkj", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmkamar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkasur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglmasuk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmtglkeluar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmjmlhari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmharga", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmtotaltransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmrekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmrekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kminputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomtext20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint14", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint15", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint16", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint17", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint18", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint19", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomint20", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdbl20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmcustomdate20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmperawatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmkategoripasien", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "kmid~kmcabang~kmlokasi~kmgudang~kmsumber~kmautonotransaksi~kmnotransaksi~kmtgl~kmkodepa~kmcustomer~kmcustomerkontak~kmuraian~kmcatatan~kmnoref~kmtglnoref~kmmatauang~kmkurs~kmidkj~kmkamar~kmkasur~kmtglmasuk~kmtglkeluar~kmjmlhari~kmharga~kmtotaltransaksi~kmrekpersediaan~kmrekhargapokok~kmrekdiskonpenjualan~kmrekpenjualan~kmstatusrealisasi~kmstatus~kmstatussebelumnya~kmjmlrevisi~kmcetakanke~kminputuser~kminputtgl~kmmodifikasiuser~kmmodifikasitgl~kmposting~kmisclose~kmcustomtext1~kmcustomtext2~kmcustomtext3~kmcustomtext4~kmcustomtext5~kmcustomtext6~kmcustomtext7~kmcustomtext8~kmcustomtext9~kmcustomtext10~kmcustomtext11~kmcustomtext12~kmcustomtext13~kmcustomtext14~kmcustomtext15~kmcustomtext16~kmcustomtext17~kmcustomtext18~kmcustomtext19~kmcustomtext20~kmcustomint1~kmcustomint2~kmcustomint3~kmcustomint4~kmcustomint5~kmcustomint6~kmcustomint7~kmcustomint8~kmcustomint9~kmcustomint10~kmcustomint11~kmcustomint12~kmcustomint13~kmcustomint14~kmcustomint15~kmcustomint16~kmcustomint17~kmcustomint18~kmcustomint19~kmcustomint20~kmcustomdbl1~kmcustomdbl2~kmcustomdbl3~kmcustomdbl4~kmcustomdbl5~kmcustomdbl6~kmcustomdbl7~kmcustomdbl8~kmcustomdbl9~kmcustomdbl10~kmcustomdbl11~kmcustomdbl12~kmcustomdbl13~kmcustomdbl14~kmcustomdbl15~kmcustomdbl16~kmcustomdbl17~kmcustomdbl18~kmcustomdbl19~kmcustomdbl20~kmcustomdate1~kmcustomdate2~kmcustomdate3~kmcustomdate4~kmcustomdate5~kmcustomdate6~kmcustomdate7~kmcustomdate8~kmcustomdate9~kmcustomdate10~kmcustomdate11~kmcustomdate12~kmcustomdate13~kmcustomdate14~kmcustomdate15~kmcustomdate16~kmcustomdate17~kmcustomdate18~kmcustomdate19~kmcustomdate20~kmperawatan~kmkategoripasien", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121)) = False Then
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
            'result(2) = "No. : '" & dtutama.Rows.Count : GoTo selesai
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)

                If isUpdate Then
                    result(4) = drutama("kmid")
                    notransaksi = drutama("kmnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kmid), kmnotransaksi FROM M_11_km WHERE kmid='" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    'If (rowUpdate > 0) Then

                    'CEK NO TRANSAKSI ======================
                    If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                        Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(kmid) FROM m_11_km WHERE kmnotransaksi='" & notransaksi & "'")
                        Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                        If cekNo > 0 Then
                            result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                        End If
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Update M_11_Km set kmcabang  = '" & FixQuotes(drutama("kmcabang")) & "', kmlokasi  = '" & FixQuotes(drutama("kmlokasi")) & "', kmgudang  = '" & FixQuotes(drutama("kmgudang")) & "', kmsumber  = '" & FixQuotes(drutama("kmsumber")) & "', kmautonotransaksi  = " & drutama("kmautonotransaksi") & ", kmnotransaksi  = '" & FixQuotes(notransaksi) & "', kmtgl  = '" & FixQuotes(AsFormatTanggal(drutama("kmtgl"))) & "', kmkodepa  = " & drutama("kmkodepa") & ", kmcustomer  = " & drutama("kmcustomer") & ", kmcustomerkontak  = '" & FixQuotes(drutama("kmcustomerkontak")) & "', kmuraian  = '" & FixQuotes(drutama("kmuraian")) & "', kmcatatan  = '" & FixQuotes(drutama("kmcatatan")) & "', kmnoref  = '" & FixQuotes(drutama("kmnoref")) & "', kmtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("kmtglnoref"))) & "', kmmatauang  = '" & FixQuotes(drutama("kmmatauang")) & "', kmkurs  = '" & FixDouble(drutama("kmkurs")) & "', kmidkj  = " & drutama("kmidkj") & ", kmkamar  = '" & FixQuotes(drutama("kmkamar")) & "', kmkasur  = '" & FixQuotes(drutama("kmkasur")) & "', kmtglkeluar  = '" & FixQuotes(drutama("kmtglkeluar")) & "', kmjmlhari  = " & drutama("kmjmlhari") & ", kmharga  = " & FixDouble(drutama("kmharga")) & ", kmtotaltransaksi  = " & FixDouble(drutama("kmtotaltransaksi")) & ", kmrekpersediaan  = '" & FixQuotes(drutama("kmrekpersediaan")) & "', kmrekhargapokok  = '" & FixQuotes(drutama("kmrekhargapokok")) & "', kmrekdiskonpenjualan  = '" & FixQuotes(drutama("kmrekdiskonpenjualan")) & "', kmrekpenjualan  = '" & FixQuotes(drutama("kmrekpenjualan")) & "', kmstatusrealisasi  = 2, kmstatus = 4, kmstatussebelumnya  = " & drutama("kmstatussebelumnya") & ", kmjmlrevisi = " & drutama("kmjmlrevisi") & ", kmcetakanke  = " & drutama("kmcetakanke") & ", kmmodifikasiuser  = " & drutama("kmmodifikasiuser") & ", kmmodifikasitgl  = '" & drutama("kmmodifikasitgl") & "', kmposting  = '" & FixDouble(drutama("kmposting")) & "', kmcustomtext1  = '" & FixQuotes(drutama("kmcustomtext1")) & "', kmcustomtext2  = '" & FixQuotes(drutama("kmcustomtext2")) & "', kmcustomtext3  = '" & FixQuotes(drutama("kmcustomtext3")) & "', kmcustomtext4  = '" & FixQuotes(drutama("kmcustomtext4")) & "', kmcustomtext5  = '" & FixQuotes(drutama("kmcustomtext5")) & "', kmcustomtext6  = '" & FixQuotes(drutama("kmcustomtext6")) & "', kmcustomtext7  = '" & FixQuotes(drutama("kmcustomtext7")) & "', kmcustomtext8  = '" & FixQuotes(drutama("kmcustomtext8")) & "', kmcustomtext9  = '" & FixQuotes(drutama("kmcustomtext9")) & "', kmcustomtext10  = '" & FixQuotes(drutama("kmcustomtext10")) & "', kmcustomtext11  = '" & FixQuotes(drutama("kmcustomtext11")) & "', kmcustomtext12  = '" & FixQuotes(drutama("kmcustomtext12")) & "', kmcustomtext13  = '" & FixQuotes(drutama("kmcustomtext13")) & "', kmcustomtext14  = '" & FixQuotes(drutama("kmcustomtext14")) & "', kmcustomtext15  = '" & FixQuotes(drutama("kmcustomtext15")) & "', kmcustomtext16  = '" & FixQuotes(drutama("kmcustomtext16")) & "', kmcustomtext17  = '" & FixQuotes(drutama("kmcustomtext17")) & "', kmcustomtext18  = '" & FixQuotes(drutama("kmcustomtext18")) & "', kmcustomtext19  = '" & FixQuotes(drutama("kmcustomtext19")) & "', kmcustomtext20  = '" & FixQuotes(drutama("kmcustomtext20")) & "', kmcustomint1  = " & drutama("kmcustomint1") & ", kmcustomint2  = " & drutama("kmcustomint2") & ", kmcustomint3  = " & drutama("kmcustomint3") & ", kmcustomint4  = " & drutama("kmcustomint4") & ", kmcustomint5  = " & drutama("kmcustomint5") & ", kmcustomint6  = " & drutama("kmcustomint6") & ", kmcustomint7  = " & drutama("kmcustomint7") & ", kmcustomint8  = " & drutama("kmcustomint8") & ", kmcustomint9  = " & drutama("kmcustomint9") & ", kmcustomint10  = " & drutama("kmcustomint10") & ", kmcustomint11  = " & drutama("kmcustomint11") & ", kmcustomint12  = " & drutama("kmcustomint12") & ", kmcustomint13  = " & drutama("kmcustomint13") & ", kmcustomint14  = " & drutama("kmcustomint14") & ", kmcustomint15  = " & drutama("kmcustomint15") & ", kmcustomint16  = " & drutama("kmcustomint16") & ", kmcustomint17  = " & drutama("kmcustomint17") & ", kmcustomint18  = " & drutama("kmcustomint18") & ", kmcustomint19  = " & drutama("kmcustomint19") & ", kmcustomint20  = " & drutama("kmcustomint20") & ", kmcustomdbl1  = '" & FixDouble(drutama("kmcustomdbl1")) & "', kmcustomdbl2  = '" & FixDouble(drutama("kmcustomdbl2")) & "', kmcustomdbl3  = '" & FixDouble(drutama("kmcustomdbl3")) & "', kmcustomdbl4  = '" & FixDouble(drutama("kmcustomdbl4")) & "', kmcustomdbl5  = '" & FixDouble(drutama("kmcustomdbl5")) & "', kmcustomdbl6  = '" & FixDouble(drutama("kmcustomdbl6")) & "', kmcustomdbl7  = '" & FixDouble(drutama("kmcustomdbl7")) & "', kmcustomdbl8  = '" & FixDouble(drutama("kmcustomdbl8")) & "', kmcustomdbl9  = '" & FixDouble(drutama("kmcustomdbl9")) & "', kmcustomdbl10  = '" & FixDouble(drutama("kmcustomdbl10")) & "', kmcustomdbl11  = '" & FixDouble(drutama("kmcustomdbl11")) & "', kmcustomdbl12  = '" & FixDouble(drutama("kmcustomdbl12")) & "', kmcustomdbl13  = '" & FixDouble(drutama("kmcustomdbl13")) & "', kmcustomdbl14  = '" & FixDouble(drutama("kmcustomdbl14")) & "', kmcustomdbl15  = '" & FixDouble(drutama("kmcustomdbl15")) & "', kmcustomdbl16  = '" & FixDouble(drutama("kmcustomdbl16")) & "', kmcustomdbl17  = '" & FixDouble(drutama("kmcustomdbl17")) & "', kmcustomdbl18  = '" & FixDouble(drutama("kmcustomdbl18")) & "', kmcustomdbl19  = '" & FixDouble(drutama("kmcustomdbl19")) & "', kmcustomdbl20  = '" & FixDouble(drutama("kmcustomdbl20")) & "', kmcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate1"))) & "', kmcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate2"))) & "', kmcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate3"))) & "', kmcustomdate4  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate4"))) & "', kmcustomdate5  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate5"))) & "', kmcustomdate6  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate6"))) & "', kmcustomdate7  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate7"))) & "', kmcustomdate8  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate8"))) & "', kmcustomdate9  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate9"))) & "', kmcustomdate10  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate10"))) & "', kmcustomdate11  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate11"))) & "', kmcustomdate12  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate12"))) & "', kmcustomdate13  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate13"))) & "', kmcustomdate14  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate14"))) & "', kmcustomdate15  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate15"))) & "', kmcustomdate16  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate16"))) & "', kmcustomdate17  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate17"))) & "', kmcustomdate18  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate18"))) & "', kmcustomdate19  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate19"))) & "', kmcustomdate20  = '" & FixQuotes(AsFormatTanggal(drutama("kmcustomdate20"))) & "',  kmperawatan  = '" & FixQuotes(drutama("kmperawatan")) & "', kmkategoripasien  = '" & FixQuotes(drutama("kmkategoripasien")) & "' where kmid = '" & drutama("kmid") & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'update status kamar jika transaksi approve
                    If drutama("kmstatus") = 2 Then
                        'UPDATE STATUS KAMAR KJ ======================
                        Dim dtCekKunjungan As DataTable = AsDataTableAmbilDariDB("SELECT kjstatuskamar, kjnotransaksi FROM m_11_kj WHERE kjid='" & drutama("kmidkj") & "'")
                        Dim cekKunjungan As Double = Val(dtCekKunjungan.Rows(0)(0))
                        If cekKunjungan > 0 Then
                            sql = "Update M_11_Kj set kjstatuskamar  = 0 where kjid = '" & drutama("kmidkj") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE STATUS KAMAR KJ ================

                        'UPDATE STATUS ISCLOSE KASUR =================
                        sql = "Update M1_bed set bisclose = 0 where bkode = '" & drutama("kmkasur") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'END OF UPDATE STATUS ISCLOSE KASUR =================

                        'UPDATE STATUS ISCLOSE KAMAR ======================
                        'cek status isclose kasur
                        Dim dtCekJmlKasur As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "'")
                        Dim cekJmlKasur As Double = Val(dtCekJmlKasur.Rows(0)(0))
                        Dim dtCekIscloseKasur As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(bkode) FROM m1_bed WHERE bkamar='" & drutama("kmkamar") & "' AND bisclose = 0")
                        Dim cekIscloseKasur As Double = Val(dtCekIscloseKasur.Rows(0)(0))
                        If cekIscloseKasur <= cekJmlKasur Then
                            'update status isclose kamar
                            sql = "Update M1_room set risclose = 0 where rkode = '" & drutama("kmkamar") & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE STATUS ISCLOSE KAMAR ===============

                    End If
                Else
                    result(2) = "Transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "KM", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("kmstatus") = 2 Then
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
                    hasilMsmq = SendMsmq(dirMsmq, "J", mjid, sumber, result(4), userid)
                    If Len(hasilMsmq) > 0 Then
                        result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
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

End Class