Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_ri
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_RiSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch() As String
        Dim dataSerial(), dataRowSerial(), dataCost(), dataRowCost(), dataPay(), dataRowPay(), dataAsset(), dataRowAsset() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", tglLunas As String = ""
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
        If (dataSplit.Length <> 6 And dataSplit.Length <> 7) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
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
        'ricustomdate3(85) As Date, rijmluangmuka(86) As Double, rirekuangmuka(87) As String, riidap(88) As Integer

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
        'ricustomdate2, ricustomdate3, rijmluangmuka, rirekuangmuka, riidap


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 89) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'riid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "riid required numeric." : GoTo selesai
        End If
        'riasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "riasalbarangkategori required numeric." : GoTo selesai
        End If
        'rijenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rijenispembeliankategori required numeric." : GoTo selesai
        End If
        'ricarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ricarabayar required numeric." : GoTo selesai
        End If
        'riautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "riautonotransaksi required numeric." : GoTo selesai
        End If
        'ritgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "ritgl required date." : GoTo selesai
        End If
        'rikodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rikodepa required numeric." : GoTo selesai
        End If
        'risupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "risupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "risupplier can't be empty." : GoTo selesai
        End If
        'ribagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ribagianpembelian required numeric." : GoTo selesai
        End If
        'ritgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "ritgljatuhtempo required date." : GoTo selesai
        End If
        'ritglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "ritglnoref required date." : GoTo selesai
        End If
        'ritglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "ritglpenutupan required date." : GoTo selesai
        End If
        'rikurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rikurs required numeric." : GoTo selesai
        End If
        'rihargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rihargatermasukpajak required numeric." : GoTo selesai
        End If
        'ritotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "ritotal required numeric." : GoTo selesai
        End If
        'rijmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rijmldiskon required numeric." : GoTo selesai
        End If
        'ritotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "ritotalpajak1detail required numeric." : GoTo selesai
        End If
        'ritotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ritotalpajak2detail required numeric." : GoTo selesai
        End If
        'ribiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ribiayalain required numeric." : GoTo selesai
        End If
        'ritotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ritotaltransaksi required numeric." : GoTo selesai
        End If
        'rijmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rijmlbayar required numeric." : GoTo selesai
        End If
        'ristatuslunas(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ristatuslunas required numeric." : GoTo selesai
        End If
        'ritgllunas(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ritgllunas required date." : GoTo selesai
        End If
        'risdhbayarpajak(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "risdhbayarpajak required numeric." : GoTo selesai
        End If
        'ritglbayarpajak(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "ritglbayarpajak required date." : GoTo selesai
        End If
        'riidpr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "riidpr required numeric." : GoTo selesai
        End If
        'riidcs(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "riidcs required numeric." : GoTo selesai
        End If
        'riidrq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "riidrq required numeric." : GoTo selesai
        End If
        'riidbs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "riidbs required numeric." : GoTo selesai
        End If
        'riidpo(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "riidpo required numeric." : GoTo selesai
        End If
        'riidipc(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "riidipc required numeric." : GoTo selesai
        End If
        'riidgrn(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "riidgrn required numeric." : GoTo selesai
        End If
        'ristatusdnr(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ristatusdnr required numeric." : GoTo selesai
        End If
        'ristatusprt(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "ristatusprt required numeric." : GoTo selesai
        End If
        'ristatus(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "ristatus required numeric." : GoTo selesai
        End If
        'ristatussebelumnya(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "ristatussebelumnya required numeric." : GoTo selesai
        End If
        'rijmlrevisi(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rijmlrevisi required numeric." : GoTo selesai
        End If
        'ricetakanke(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "ricetakanke required numeric." : GoTo selesai
        End If
        'riinputuser(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "riinputuser required numeric." : GoTo selesai
        End If
        'riinputtgl(66) As DateTime
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "riinputtgl required date." : GoTo selesai
        End If
        'rimodifikasiuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rimodifikasiuser required numeric." : GoTo selesai
        End If
        'rimodifikasitgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "rimodifikasitgl required date." : GoTo selesai
        End If
        'riposting(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "riposting required numeric." : GoTo selesai
        End If
        'ritutupperiode(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "ritutupperiode required numeric." : GoTo selesai
        End If
        'riisclose(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "riisclose required numeric." : GoTo selesai
        End If
        'ricustomint1(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "ricustomint1 required numeric." : GoTo selesai
        End If
        'ricustomint2(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "ricustomint2 required numeric." : GoTo selesai
        End If
        'ricustomint3(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "ricustomint3 required numeric." : GoTo selesai
        End If
        'ricustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "ricustomdbl1 required numeric." : GoTo selesai
        End If
        'ricustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "ricustomdbl2 required numeric." : GoTo selesai
        End If
        'ricustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "ricustomdbl3 required numeric." : GoTo selesai
        End If
        'ricustomdate1(83) As Date
        If (IsDate(dataUtama(83)) = False) Then
            result(2) = "ricustomdate1 required date." : GoTo selesai
        End If
        'ricustomdate2(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "ricustomdate2 required date." : GoTo selesai
        End If
        'ricustomdate3(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "ricustomdate3 required date." : GoTo selesai
        End If
        'rijmluangmuka(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "rijmluangmuka required numeric." : GoTo selesai
        End If
        'riidap(88) As Integer
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "riidap required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'ricabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ricabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ricabang should not be more than 25 character." : GoTo selesai
        End If

        'rilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rilokasi should not be more than 25 character." : GoTo selesai
        End If

        'rigudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rigudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rigudang should not be more than 25 character." : GoTo selesai
        End If

        'risumber(9) As String
		dataUtama(9) = "RI"
        If Len(dataUtama(9)) = 0 Then
            result(2) = "risumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "risumber should not be more than 10 character." : GoTo selesai
        End If

        'rinotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "rinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ritgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ritgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS UM
        tglLunas = AsFormatTanggal(dataUtama(12))

        'ritgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "ritgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ritglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ritglnoref can't be empty" : GoTo selesai
        End If

        'ritglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ritglpenutupan can't be empty" : GoTo selesai
        End If

        'rimatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rimatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "rimatauang should not be more than 25 character." : GoTo selesai
        End If

        'rikurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rikurs can't be empty" : GoTo selesai
        End If

        'ritotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "ritotal can't be empty" : GoTo selesai
        End If

        'ridiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "ridiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "ridiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rijmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rijmldiskon can't be empty" : GoTo selesai
        End If

        'ritotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "ritotalpajak1detail can't be empty" : GoTo selesai
        End If

        'ritotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "ritotalpajak2detail can't be empty" : GoTo selesai
        End If

        'ribiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "ribiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "ribiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'ribiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "ribiayalain can't be empty" : GoTo selesai
        End If

        'ritotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ritotaltransaksi can't be empty" : GoTo selesai
        End If

        'rijmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rijmlbayar can't be empty" : GoTo selesai
        End If

        'ritgllunas(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ritgllunas can't be empty" : GoTo selesai
        End If

        'ritglbayarpajak(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "ritglbayarpajak can't be empty" : GoTo selesai
        End If

        'riinputtgl(66) As DateTime
        If Len(dataUtama(66)) = 0 Then
            result(2) = "riinputtgl can't be empty" : GoTo selesai
        End If

        'rimodifikasitgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "rimodifikasitgl can't be empty" : GoTo selesai
        End If

        'ricustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "ricustomdbl1 can't be empty" : GoTo selesai
        End If

        'ricustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "ricustomdbl2 can't be empty" : GoTo selesai
        End If

        'ricustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "ricustomdbl3 can't be empty" : GoTo selesai
        End If

        'ricustomdate1(83) As Date
        If Len(dataUtama(83)) = 0 Then
            result(2) = "ricustomdate1 can't be empty" : GoTo selesai
        End If

        'ricustomdate2(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "ricustomdate2 can't be empty" : GoTo selesai
        End If

        'ricustomdate3(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "ricustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "riid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ridiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ristatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "risdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmluangmuka", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rirekuangmuka", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidap", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "riid~ricabang~rilokasi~rigudang~riasalbarang~riasalbarangkategori~rijenispembelian~rijenispembeliankategori~ricarabayar~risumber~riautonotransaksi~rinotransaksi~ritgl~rikodepa~risupplier~risupplierkontak~ri1alamat1~ri1alamat2~ri1alamat3~ri2alamat1~ri2alamat2~ri2alamat3~ribagianpembelian~ritermin~ritgljatuhtempo~riuraian~ricatatan~rinoref~ritglnoref~ritglpenutupan~rimatauang~rikurs~rihargatermasukpajak~ritotal~ridiskonpersen~rijmldiskon~ritotalpajak1detail~ritotalpajak2detail~ribiayalainpersen~ribiayalain~ritotaltransaksi~rijmlbayar~ristatuslunas~ritgllunas~rinofakturpajak~risdhbayarpajak~ritglbayarpajak~rirekdiskon~rirekpajak1~rirekpajak2~rirekbiayalain~rirekbayar~riidpr~riidcs~riidrq~riidbs~riidpo~riidipc~riidgrn~ristatusdnr~ristatusprt~ristatus~ristatussebelumnya~rijmlrevisi~ricetakanke~riinputuser~riinputtgl~rimodifikasiuser~rimodifikasitgl~riposting~ritutupperiode~riisclose~ricustomtext1~ricustomtext2~ricustomtext3~ricustomtext4~ricustomtext5~ricustomint1~ricustomint2~ricustomint3~ricustomdbl1~ricustomdbl2~ricustomdbl3~ricustomdate1~ricustomdate2~ricustomdate3~rijmluangmuka~rirekuangmuka~riidap", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idridetail(0) As Integer, idri(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, rekpersediaan(23) As String, rekdiskonpembelian(24) As String, 
        'rekhutangsementara(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idprdetail(32) As Integer, idcsdetail(33) As Integer, idrqdetail(34) As Integer, 
        'idbsdetail(35) As Integer, idpodetail(36) As Integer, idipcdetail(37) As Integer, idgrndetail(38) As Integer, jmldnr(39) As Double, 
        'statusdnr(40) As Integer, jmlprt(41) As Double, statusprt(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, 
        'statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idridetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargafix", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangsementara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idipcdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idgrndetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlprt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusprt", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingPO As String = "", ftOutstandingPO As String = "", updNilaiPO As String = "", updFilterPO As String = ""
        Dim ftExistOutstandingGRN As String = "", ftOutstandingGRN As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""
        Dim idbarang As Integer = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0, jmlbarang As Double = 0
        Dim gudang As String = "", updStokOutBooking As String = ""

        'FILTER PO DAN GRN, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftPO As String = "", ftGRN As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 53) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idridetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idridetail required numeric." : GoTo selesai
            End If
            'idri(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idri required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargafix(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargafix required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'idgrndetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'jmldnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            'If Len(dataRowDetail(3)) > 100 Then
            '    result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            'End If

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

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
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

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudang(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'jmldnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idridetail~idri~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekdiskonpembelian~rekhutangsementara~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String       , idpodetail(36) As Integer      , idgrndetail(38) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idpodetail = dataRowDetail(36) : idgrndetail = dataRowDetail(38)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")


            'VALIDASI OUTSTANDING -------------------------
            If idpodetail <> 0 Then 'PO
                'CEK PO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPO = IIf(Len(ftPO.ToString) = 0, "", ftPO & " OR ")
                ftPO = String.Concat(ftPO, " (pod.idpodetail = " & idpodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPO = IIf(Len(ftExistOutstandingPO.ToString) = 0, "", ftExistOutstandingPO & " UNION ")
                ftExistOutstandingPO = String.Concat(ftExistOutstandingPO, "SELECT EXISTS(SELECT 1 FROM m4_po_detail JOIN m4_po ON idpo = poid WHERE idpodetail = '" & idpodetail & "' AND (postatus = 2 OR postatus = 3 OR postatus = 4 OR postatus = 7) LIMIT 1) as rowExists, '" & idpodetail & "' as idpodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                ftOutstandingPO = IIf(Len(ftOutstandingPO.ToString) = 0, "", ftOutstandingPO & " OR ")
                ftOutstandingPO = String.Concat(ftOutstandingPO, " (pod.idpodetail = " & idpodetail & " AND " & Outstanding & " > (pod.jmlbarang - pod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPO)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                'SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking
            End If

            If idgrndetail <> 0 Then 'GRN
                'CEK GRN YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftGRN = IIf(Len(ftGRN.ToString) = 0, "", ftGRN & " OR ")
                ftGRN = String.Concat(ftGRN, " (grnd.idgrndetail = " & idgrndetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingGRN = IIf(Len(ftExistOutstandingGRN.ToString) = 0, "", ftExistOutstandingGRN & " UNION ")
                ftExistOutstandingGRN = String.Concat(ftExistOutstandingGRN, "SELECT EXISTS(SELECT 1 FROM m4_grn_detail JOIN m4_grn ON idgrn = grnid WHERE idgrndetail = '" & idgrndetail & "' AND (grnstatus = 2 OR grnstatus = 3 OR grnstatus = 4 OR grnstatus = 7) LIMIT 1) as rowExists, '" & idgrndetail & "' as idgrndetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idgrndetail=" & idgrndetail)
                ftOutstandingGRN = IIf(Len(ftOutstandingGRN.ToString) = 0, "", ftOutstandingGRN & " OR ")
                ftOutstandingGRN = String.Concat(ftOutstandingGRN, " (grnd.idgrndetail = " & idgrndetail & " AND " & Outstanding & " > (grnd.jmlbarang - grnd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiGRN = String.Concat("WHEN '" & idgrndetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiGRN)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                updFilterGRN = String.Concat(updFilterGRN, "(idgrndetail = '" & idgrndetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)


        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 1
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)


        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then

            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 1
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ========================================
        End If

        'MAPPING BUAT WS DATA COST -------------------------------------------------------
        'idricost(0) As Integer, idri(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, catatan(6) As String, costcenter(7) As String, divisi(8) As String, subdivisi(9) As String, 
        'proyek(10) As String, urutan(11) As Integer, idprcost(12) As Integer, idcscost(13) As Integer, idrqcost(14) As Integer, 
        'idbscost(15) As Integer, idpocost(16) As Integer, idipccost(17) As Integer, idgrncost(18) As Integer, jumlahbayar(19) As Double, 
        'statusbayar(20) As Integer, isclose(21) As Integer, customtext1(22) As String, customtext2(23) As String, customtext3(24) As String, 
        'customdbl1(25) As Double, customdbl2(26) As Double, customdbl3(27) As Double, customdate1(28) As Date, customdate2(29) As Date, 
        'customdate3(30) As Date, rekdebit(31) As String, rekkredit(32) As String, kontak(33) As Integer, termasukhpp(34) As Integer

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idricost, idri, kodecost, matauang, kurs, jumlah, catatan, 
        'costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, 
        'idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, rekdebit, rekkredit, kontak, termasukhpp

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idricost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtcost, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idprcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idcscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idrqcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idbscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idpocost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idipccost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idgrncost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)

        'CEK PARAMETER DATA COST
        If dataSplit(4).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(4).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================


            'VALIDASI DAN SET DATA ROW COST ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA COST
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA COST -----------------------------------
                'CEK ARRAY DATA COST
                If (dataRowCost.Length <> 35) Then
                    result(2) = "Cost Row : " & i & " -  Invalid cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW COST ----------------------------

                'VALIDASI TIPE DATA COST ------------------------------------------
                'idricost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idricost required numeric." : GoTo selesai
                End If
                'idri(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idri required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'urutan(11) As Integer
                If (IsNumeric(dataRowCost(11)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(12) As Integer
                If (IsNumeric(dataRowCost(12)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(13) As Integer
                If (IsNumeric(dataRowCost(13)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(14) As Integer
                If (IsNumeric(dataRowCost(14)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'idpocost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idipccost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idipccost required numeric." : GoTo selesai
                End If
                'idgrncost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrncost required numeric." : GoTo selesai
                End If
                'jumlahbayar(19) As Double
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(20) As Integer
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(25) As Double
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(26) As Double
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(27) As Double
                If (IsNumeric(dataRowCost(27)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(28) As Date
                If (IsDate(dataRowCost(28)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(29) As Date
                If (IsDate(dataRowCost(29)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(30) As Date
                If (IsDate(dataRowCost(30)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'kontak(33) As Integer
                If (IsNumeric(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(34) As Integer
                If (IsNumeric(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA COST -----------------------------------

                'VALIDASI DATA COST ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                'jumlahbayar(19) As Double
                If Len(dataRowCost(19)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(25) As Double
                If Len(dataRowCost(25)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(26) As Double
                If Len(dataRowCost(26)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(27) As Double
                If Len(dataRowCost(27)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(28) As Date
                If Len(dataRowCost(28)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(29) As Date
                If Len(dataRowCost(29)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(30) As Date
                If Len(dataRowCost(30)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'rekdebit(31) As String
                If dataRowCost(34) = 0 Then
                    If Len(dataRowCost(31)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(31)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(32) As String
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(32)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA COST --------------------------------

                If AsDataTableTambahData(dtcost, "idricost~idri~kodecost~matauang~kurs~jumlah~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~idpocost~idipccost~idgrncost~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rekdebit~rekkredit~kontak~termasukhpp", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

        End If


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idricarabayar(0) As Integer, idri(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer, sumber(16) As String, idtransaksi(17) As Integer, totaltransaksi(18) As Double, terbayar(19) As Double

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose, sumber, idtransaksi, totaltransaksi, terbayar

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idricarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idri", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "sumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtpay, "totaltransaksi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtpay, "terbayar", AsEnumTypeData.AsDouble)

        Dim ftExistOutstandingAP As String = "", ftOutstandingAP As String = ""
        Dim updNilaiAP As String = "", updNilaiValasAP As String = ""
        Dim updFilterAP As String = "", updTglLunasAP As String = "", idAP As Integer = 0
        Dim OutstandingAP As Double = 0, OutstandingAPValas As Double = 0

        'CEK PARAMETER DATA PAY
        If dataSplit(5).Length > 0 Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(5).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA PAY
                If (dataRowPay.Length <> 20) Then
                    result(2) = "Row Pay : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idricarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Row Pay : " & i & " - idricarabayar required numeric." : GoTo selesai
                End If
                'idri(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Row Pay : " & i & " - idri required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Row Pay : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Row Pay : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Row Pay : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Row Pay : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Row Pay : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Row Pay : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'isclose(15) As Integer
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Row Pay : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'idtransaksi(17) As Integer
                If (IsNumeric(dataRowPay(17)) = False) Then
                    result(2) = "Row Pay : " & i & " - idtransaksi required numeric." : GoTo selesai
                End If
                'totaltransaksi(18) As Double
                If (IsNumeric(dataRowPay(18)) = False) Then
                    result(2) = "Row Pay : " & i & " - totaltransaksi required numeric." : GoTo selesai
                End If
                'terbayar(19) As Double
                If (IsNumeric(dataRowPay(19)) = False) Then
                    result(2) = "Row Pay : " & i & " - terbayar required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Row Pay : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Row Pay : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Row Pay : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Row Pay : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) < 0 Then
                    result(2) = "Row Pay : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Row Pay : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Row Pay : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Row Pay : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Row Pay : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Row Pay : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Row Pay : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Row Pay : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Row Pay : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Row Pay : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Row Pay : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Row Pay : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Row Pay : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If

                    'sumber(16) As String
                    If Len(dataRowPay(16)) = 0 Then
                        result(2) = "Row Pay : " & i & " - sumber can't be empty" : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idricarabayar~idri~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose~sumber~idtransaksi~totaltransaksi~terbayar", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15) & "~" & dataRowPay(16) & "~" & dataRowPay(17) & "~" & dataRowPay(18) & "~" & dataRowPay(19)) = False Then
                    result(2) = "Row Pay : " & i & " - insert into datatable failed." : GoTo selesai
                End If

                'VALIDASI OUTSTANDING AP (UM PEMB) -------------------
                'SET VARIABEL
                'jumlah(5) As Double, jumlahvalas(6) As Double, sumber(16) As String, idtransaksi(17) As Integer
                If dataRowPay(16).ToString.ToUpper.Equals("AP") Then
                    idAP = dataRowPay(17)
                    OutstandingAP = AsDataTableDSum(dtpay, "jumlah", "sumber = 'AP' AND idtransaksi = " & idAP & "")
                    OutstandingAPValas = AsDataTableDSum(dtpay, "jumlahvalas", "sumber = 'AP' AND idtransaksi = " & idAP & "")

                    If idAP <> 0 Then 'Ap
                        '1. CEK DATA EXIST
                        ftExistOutstandingAP = IIf(Len(ftExistOutstandingAP.ToString) = 0, "", ftExistOutstandingAP & " UNION ")
                        ftExistOutstandingAP = String.Concat(ftExistOutstandingAP, "SELECT EXISTS(SELECT 1 FROM m4_ap WHERE apid = '" & idAP & "' AND (apstatus = 2 OR apstatus = 3 OR apstatus = 4 OR apstatus = 7) LIMIT 1) as rowExists, apid, apsumber, apnotransaksi FROM m4_ap WHERE apid = '" & idAP & "'")

                        '2. CEK JML OUTSTANDING
                        ftOutstandingAP = IIf(Len(ftOutstandingAP.ToString) = 0, "", ftOutstandingAP & " OR ")
                        ftOutstandingAP = String.Concat(ftOutstandingAP, " (`ap`.apid = '" & idAP & "' AND (CASE `ap`.apmatauang WHEN s.snilai THEN " & OutstandingAP & " > `ap`.apjumlah - `ap`.apjumlahbayar ELSE " & OutstandingAPValas & " > `ap`.apjumlahvalas - `ap`.apjumlahbayarvalas END)) ")

                        '3. SET NILAI UPDATE OUTSTANDING
                        updNilaiAP = String.Concat("WHEN '" & idAP & "' THEN ROUND(apjumlahbayar + '" & OutstandingAP & "', 5) ", updNilaiAP)
                        updNilaiValasAP = String.Concat("WHEN '" & idAP & "' THEN ROUND(`ap`.apjumlahbayarvalas + '" & OutstandingAPValas & "', 5) ", updNilaiValasAP)

                        '4. SET FILTER UPDATE OUTSTANDING
                        updFilterAP = IIf(Len(updFilterAP.ToString) = 0, "", updFilterAP & " OR ")
                        updFilterAP = String.Concat(updFilterAP, "(`ap`.apid = '" & idAP & "')")

                        '5. SET NILAI TGLLUNAS TRANSAKSI
                        'If MUUtama = MUFungsional Then
                        updTglLunasAP = String.Concat(" WHEN '" & idAP & "' THEN (CASE `ap`.apmatauang WHEN s.snilai THEN (CASE WHEN ROUND(`ap`.apjumlahbayar + '" & OutstandingAP & "', 5) >= `ap`.apjumlah THEN '" & FixQuotes(tglLunas) & "' ELSE `ap`.aptgllunas END) ELSE (CASE WHEN ROUND(`ap`.apjumlahbayarvalas + '" & OutstandingAPValas & "', 5) >= `ap`.apjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE `ap`.aptgllunas END) END) ", updTglLunasAP)
                        'Else
                        'updTglLunasAP = String.Concat(" WHEN '" & idAP & "' THEN (CASE WHEN ROUND(`ap`.apjumlahbayarvalas + '" & OutstandingAPValas & "', 5) >= `ap`.apjumlahvalas THEN '" & FixQuotes(tglLunas) & "' ELSE `ap`.aptgllunas END) ", updTglLunasAP)
                        'End If
                    End If
                End If
                'END OF VALIDASI OUTSTANDING AS (UM PENJ) ------------

            Next
        End If


        'MAPPING BUAT WS DATA ASSET -------------------------------------------------------
        'atid(0) As Integer, atasetid(1) As Integer, atjenismutasi(2) As Integer, atsumber(3) As String, atidutama(4) As Integer, 
        'atidbarang(5) As Integer, atkode(6) As String, atnama(7) As String, atkategori(8) As String, atcabang(9) As String, 
        'atlokasi(10) As String, atgudang(11) As String, atdivisi(12) As String, atsubdivisi(13) As String, atcostcenter(14) As String, 
        'atproyek(15) As String, atcatatan(16) As String, atnomor(17) As String, attglbeli(18) As Date, attglpakai(19) As Date, 
        'atjml(20) As Double, atsatuan(21) As String, atmatauang(22) As String, atkurs(23) As Double, atharga(24) As Double, 
        'atdiskon(25) As String, atjmldiskon(26) As Double, atpajak1(27) As String, atjmlpajak1(28) As Double, atpajak2(29) As String, 
        'atjmlpajak2(30) As Double, athargabeli(31) As Double, atnilairesidu(32) As Double, atumurekonomis(33) As Double, atbebanperbln(34) As Double, 
        'atakumulasibeban(35) As Double, atnilaibuku(36) As Double, atmetode(37) As Integer, attabelpenyusutan(38) As String, atintangible(39) As Integer, 
        'atfiskal(40) As Integer, atatastengahbulan(41) As Integer, atrekasset(42) As String, atrekakumdepresiasi(43) As String, atrekdepresiasi(44) As String, 
        'atrekpenghapusan(45) As String, atprodusen(46) As Integer, attglpensiun(47) As Date, atpenyusutanke(48) As Double, atnilaimenurun(49) As Double, 
        'atdispose(50) As Integer, atpembelian(51) As Integer, atpenjualan(52) As Integer, atlocked(53) As Integer, atstatus(54) As Integer, 
        'atstatussebelumnya(55) As Integer, atisclose(56) As Integer, atinputuser(57) As Integer, atinputtgl(58) As DateTime, atmodifikasiuser(59) As Integer, 
        'atmodifikasitgl(60) As DateTime, atcustomtext1(61) As String, atcustomtext2(62) As String, atcustomtext3(63) As String, atcustomtext4(64) As String, 
        'atcustomtext5(65) As String, atcustomint1(66) As Integer, atcustomint2(67) As Integer, atcustomint3(68) As Integer, atcustomint4(69) As Integer, 
        'atcustomint5(70) As Integer, atcustomdbl1(71) As Double, atcustomdbl2(72) As Double, atcustomdbl3(73) As Double, atcustomdbl4(74) As Double, 
        'atcustomdbl5(75) As Double, atcustomdate1(76) As Date, atcustomdate2(77) As Date, atcustomdate3(78) As Date, atcustomdate4(79) As Date, 
        'atcustomdate5(80) As Date

        'MAPPING BUAT FLEX DATA ASSET -----------------------------------------------------
        'atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, 
        'atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, 
        'atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, 
        'atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, 
        'atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, 
        'atakumulasibeban, atnilaibuku, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, 
        'atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, 
        'atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, 
        'atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, 
        'atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, 
        'atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, 
        'atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5

        'Buat datatable asset
        Dim dtasset As New DataTable
        AsDataTableTambahField(dtasset, "atid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atasetid", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atidutama", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcostcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnomor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "attglbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "attglpakai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtasset, "atsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmlpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atjmlpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "athargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilairesidu", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atumurekonomis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atbebanperbln", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atakumulasibeban", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilaibuku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmetode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "attabelpenyusutan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atintangible", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atfiskal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atatastengahbulan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atrekasset", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekakumdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekdepresiasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atrekpenghapusan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atprodusen", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "attglpensiun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atpenyusutanke", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atnilaimenurun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atdispose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atlocked", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtasset, "atcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtasset, "atcustomdate5", AsEnumTypeData.AsString)

        'CEK PARAMETER DATA ASSET
        If dataSplit.Length > 6 Then
            If dataSplit(6).Length > 0 Then

                'VALIDASI DAN SET DATA ASSET ======================================================
                'SPLIT PARAMETER DATA ASSET
                dataAsset = dataSplit(6).Split(sptRow)
                'END OF VALIDASI DAN SET DATA ASSET ===============================================


                'VALIDASI DAN SET DATA ROW ASSET ==================================================
                Dim JmlDtAsset As Integer = dataAsset.Length
                For i = 1 To JmlDtAsset
                    'SPLIT DATA ASSET
                    dataRowAsset = dataAsset(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA ASSET -----------------------------------
                    'CEK ARRAY DATA ASSET
                    If (dataRowAsset.Length <> 81) Then
                        result(2) = "Asset Row : " & i & " - Invalid asset transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW ASSET ----------------------------

                    'VALIDASI TIPE DATA ASSET ------------------------------------------
                    'atjenismutasi(2) As Integer
                    'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                    dataRowAsset(2) = 1
                    If (IsNumeric(dataRowAsset(2)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjenismutasi required numeric." : GoTo selesai
                    End If
                    'attglbeli(18) As Date
                    If (IsDate(dataRowAsset(18)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglbeli required date." : GoTo selesai
                    End If
                    'attglpakai(19) As Date
                    If (IsDate(dataRowAsset(19)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglpakai required date." : GoTo selesai
                    End If
                    'atjml(20) As Double
                    If (IsNumeric(dataRowAsset(20)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjml required numeric." : GoTo selesai
                    End If
                    'atkurs(23) As Double
                    If (IsNumeric(dataRowAsset(23)) = False) Then
                        result(2) = "Asset Row : " & i & " - atkurs required numeric." : GoTo selesai
                    End If
                    'atharga(24) As Double
                    If (IsNumeric(dataRowAsset(24)) = False) Then
                        result(2) = "Asset Row : " & i & " - atharga required numeric." : GoTo selesai
                    End If
                    'atjmldiskon(26) As Double
                    If (IsNumeric(dataRowAsset(26)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmldiskon required numeric." : GoTo selesai
                    End If
                    'atjmlpajak1(28) As Double
                    If (IsNumeric(dataRowAsset(28)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak1 required numeric." : GoTo selesai
                    End If
                    'atjmlpajak2(30) As Double
                    If (IsNumeric(dataRowAsset(30)) = False) Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak2 required numeric." : GoTo selesai
                    End If
                    'athargabeli(31) As Double
                    If (IsNumeric(dataRowAsset(31)) = False) Then
                        result(2) = "Asset Row : " & i & " - athargabeli required numeric." : GoTo selesai
                    End If
                    'atnilairesidu(32) As Double
                    If (IsNumeric(dataRowAsset(32)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilairesidu required numeric." : GoTo selesai
                    End If
                    'atumurekonomis(33) As Double
                    If (IsNumeric(dataRowAsset(33)) = False) Then
                        result(2) = "Asset Row : " & i & " - atumurekonomis required numeric." : GoTo selesai
                    End If
                    'atbebanperbln(34) As Double
                    If (IsNumeric(dataRowAsset(34)) = False) Then
                        result(2) = "Asset Row : " & i & " - atbebanperbln required numeric." : GoTo selesai
                    End If
                    'atakumulasibeban(35) As Double
                    If (IsNumeric(dataRowAsset(35)) = False) Then
                        result(2) = "Asset Row : " & i & " - atakumulasibeban required numeric." : GoTo selesai
                    End If
                    'atnilaibuku(36) As Double
                    If (IsNumeric(dataRowAsset(36)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilaibuku required numeric." : GoTo selesai
                    End If
                    'atmetode(37) As Integer
                    If (IsNumeric(dataRowAsset(37)) = False) Then
                        result(2) = "Asset Row : " & i & " - atmetode required numeric." : GoTo selesai
                    End If
                    'atintangible(39) As Integer
                    If (IsNumeric(dataRowAsset(39)) = False) Then
                        result(2) = "Asset Row : " & i & " - atintangible required numeric." : GoTo selesai
                    End If
                    'atfiskal(40) As Integer
                    If (IsNumeric(dataRowAsset(40)) = False) Then
                        result(2) = "Asset Row : " & i & " - atfiskal required numeric." : GoTo selesai
                    End If
                    'atatastengahbulan(41) As Integer
                    If (IsNumeric(dataRowAsset(41)) = False) Then
                        result(2) = "Asset Row : " & i & " - atatastengahbulan required numeric." : GoTo selesai
                    End If
                    'attglpensiun(47) As Date
                    If (IsDate(dataRowAsset(47)) = False) Then
                        result(2) = "Asset Row : " & i & " - attglpensiun required date." : GoTo selesai
                    End If
                    'atpenyusutanke(48) As Double
                    If (IsNumeric(dataRowAsset(48)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpenyusutanke required numeric." : GoTo selesai
                    End If
                    'atnilaimenurun(49) As Double
                    If (IsNumeric(dataRowAsset(49)) = False) Then
                        result(2) = "Asset Row : " & i & " - atnilaimenurun required numeric." : GoTo selesai
                    End If
                    'atdispose(50) As Integer
                    If (IsNumeric(dataRowAsset(50)) = False) Then
                        result(2) = "Asset Row : " & i & " - atdispose required numeric." : GoTo selesai
                    End If
                    'atpembelian(51) As Integer
                    If (IsNumeric(dataRowAsset(51)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpembelian required numeric." : GoTo selesai
                    End If
                    'atpenjualan(52) As Integer
                    If (IsNumeric(dataRowAsset(52)) = False) Then
                        result(2) = "Asset Row : " & i & " - atpenjualan required numeric." : GoTo selesai
                    End If
                    'atlocked(53) As Integer
                    If (IsNumeric(dataRowAsset(53)) = False) Then
                        result(2) = "Asset Row : " & i & " - atlocked required numeric." : GoTo selesai
                    End If
                    'atstatus(54) As Integer
                    If (IsNumeric(dataRowAsset(54)) = False) Then
                        result(2) = "Asset Row : " & i & " - atstatus required numeric." : GoTo selesai
                    End If
                    'atstatussebelumnya(55) As Integer
                    If (IsNumeric(dataRowAsset(55)) = False) Then
                        result(2) = "Asset Row : " & i & " - atstatussebelumnya required numeric." : GoTo selesai
                    End If
                    'atisclose(56) As Integer
                    If (IsNumeric(dataRowAsset(56)) = False) Then
                        result(2) = "Asset Row : " & i & " - atisclose required numeric." : GoTo selesai
                    End If
                    'atinputtgl(58) As DateTime
                    If (IsDate(dataRowAsset(58)) = False) Then
                        result(2) = "Asset Row : " & i & " - atinputtgl required date." : GoTo selesai
                    End If
                    'atmodifikasitgl(60) As DateTime
                    If (IsDate(dataRowAsset(60)) = False) Then
                        result(2) = "Asset Row : " & i & " - atmodifikasitgl required date." : GoTo selesai
                    End If
                    'atcustomint1(66) As Integer
                    If (IsNumeric(dataRowAsset(66)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint1 required numeric." : GoTo selesai
                    End If
                    'atcustomint2(67) As Integer
                    If (IsNumeric(dataRowAsset(67)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint2 required numeric." : GoTo selesai
                    End If
                    'atcustomint3(68) As Integer
                    If (IsNumeric(dataRowAsset(68)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint3 required numeric." : GoTo selesai
                    End If
                    'atcustomint4(69) As Integer
                    If (IsNumeric(dataRowAsset(69)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint4 required numeric." : GoTo selesai
                    End If
                    'atcustomint5(70) As Integer
                    If (IsNumeric(dataRowAsset(70)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomint5 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl1(71) As Double
                    If (IsNumeric(dataRowAsset(71)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl1 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl2(72) As Double
                    If (IsNumeric(dataRowAsset(72)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl2 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl3(73) As Double
                    If (IsNumeric(dataRowAsset(73)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl3 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl4(74) As Double
                    If (IsNumeric(dataRowAsset(74)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl4 required numeric." : GoTo selesai
                    End If
                    'atcustomdbl5(75) As Double
                    If (IsNumeric(dataRowAsset(75)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl5 required numeric." : GoTo selesai
                    End If
                    'atcustomdate1(76) As Date
                    If (IsDate(dataRowAsset(76)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate1 required date." : GoTo selesai
                    End If
                    'atcustomdate2(77) As Date
                    If (IsDate(dataRowAsset(77)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate2 required date." : GoTo selesai
                    End If
                    'atcustomdate3(78) As Date
                    If (IsDate(dataRowAsset(78)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate3 required date." : GoTo selesai
                    End If
                    'atcustomdate4(79) As Date
                    If (IsDate(dataRowAsset(79)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate4 required date." : GoTo selesai
                    End If
                    'atcustomdate5(80) As Date
                    If (IsDate(dataRowAsset(80)) = False) Then
                        result(2) = "Asset Row : " & i & " - atcustomdate5 required date." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA ASSET -----------------------------------

                    'VALIDASI DATA ASSET ---------------------------------------
                    'atid(0) As 
                    If Len(dataRowAsset(0)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atid can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(0)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atid should not be more than 20 character." : GoTo selesai
                    End If

                    'atasetid(1) As 
                    If Len(dataRowAsset(1)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atasetid can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(1)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atasetid should not be more than 20 character." : GoTo selesai
                    End If

                    'atsumber(3) As String
                    If Len(dataRowAsset(3)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atsumber can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(3)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atsumber should not be more than 25 character." : GoTo selesai
                    End If

                    'atidutama(4) As 
                    If Len(dataRowAsset(4)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atidutama can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(4)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atidutama should not be more than 20 character." : GoTo selesai
                    End If

                    'atidbarang(5) As 
                    If Len(dataRowAsset(5)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atidbarang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(5)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atidbarang should not be more than 20 character." : GoTo selesai
                    End If

                    'atkode(6) As String
                    If Len(dataRowAsset(6)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkode can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(6)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atkode should not be more than 25 character." : GoTo selesai
                    End If

                    'atnama(7) As String
                    If Len(dataRowAsset(7)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnama can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(7)) > 100 Then
                        result(2) = "Asset Row : " & i & " - atnama should not be more than 100 character." : GoTo selesai
                    End If

                    'atkategori(8) As String
                    If Len(dataRowAsset(8)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkategori can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(8)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atkategori should not be more than 25 character." : GoTo selesai
                    End If

                    'attglbeli(18) As Date
                    If Len(dataRowAsset(18)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglbeli can't be empty" : GoTo selesai
                    End If

                    'attglpakai(19) As Date
                    If Len(dataRowAsset(19)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglpakai can't be empty" : GoTo selesai
                    End If

                    'atjml(20) As Double
                    If Len(dataRowAsset(20)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjml can't be empty" : GoTo selesai
                    End If

                    'atsatuan(21) As String
                    If Len(dataRowAsset(21)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atsatuan can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(21)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atsatuan should not be more than 25 character." : GoTo selesai
                    End If

                    'atmatauang(22) As String
                    If Len(dataRowAsset(22)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmatauang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(22)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atmatauang should not be more than 25 character." : GoTo selesai
                    End If

                    'atkurs(23) As Double
                    If Len(dataRowAsset(23)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atkurs can't be empty" : GoTo selesai
                    End If

                    'atharga(24) As Double
                    If Len(dataRowAsset(24)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atharga can't be empty" : GoTo selesai
                    End If

                    'atdiskon(25) As String
                    If Len(dataRowAsset(25)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atdiskon can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(25)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atdiskon should not be more than 25 character." : GoTo selesai
                    End If

                    'atjmldiskon(26) As Double
                    If Len(dataRowAsset(26)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmldiskon can't be empty" : GoTo selesai
                    End If

                    'atjmlpajak1(28) As Double
                    If Len(dataRowAsset(28)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak1 can't be empty" : GoTo selesai
                    End If

                    'atjmlpajak2(30) As Double
                    If Len(dataRowAsset(30)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atjmlpajak2 can't be empty" : GoTo selesai
                    End If

                    'athargabeli(31) As Double
                    If Len(dataRowAsset(31)) = 0 Then
                        result(2) = "Asset Row : " & i & " - athargabeli can't be empty" : GoTo selesai
                    End If

                    'atnilairesidu(32) As Double
                    If Len(dataRowAsset(32)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilairesidu can't be empty" : GoTo selesai
                    End If

                    'atumurekonomis(33) As Double
                    If Len(dataRowAsset(33)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atumurekonomis can't be empty" : GoTo selesai
                    End If

                    'atbebanperbln(34) As Double
                    If Len(dataRowAsset(34)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atbebanperbln can't be empty" : GoTo selesai
                    End If

                    'atakumulasibeban(35) As Double
                    If Len(dataRowAsset(35)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atakumulasibeban can't be empty" : GoTo selesai
                    End If

                    'atnilaibuku(36) As Double
                    If Len(dataRowAsset(36)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilaibuku can't be empty" : GoTo selesai
                    End If

                    'atrekasset(42) As String
                    If Len(dataRowAsset(42)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekasset can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(42)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekasset should not be more than 25 character." : GoTo selesai
                    End If

                    'atrekakumdepresiasi(43) As String
                    If Len(dataRowAsset(43)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekakumdepresiasi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(43)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekakumdepresiasi should not be more than 25 character." : GoTo selesai
                    End If

                    'atrekdepresiasi(44) As String
                    If Len(dataRowAsset(44)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atrekdepresiasi can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(44)) > 25 Then
                        result(2) = "Asset Row : " & i & " - atrekdepresiasi should not be more than 25 character." : GoTo selesai
                    End If

                    'atprodusen(46) As 
                    If Len(dataRowAsset(46)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atprodusen can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(46)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atprodusen should not be more than 20 character." : GoTo selesai
                    End If

                    'attglpensiun(47) As Date
                    If Len(dataRowAsset(47)) = 0 Then
                        result(2) = "Asset Row : " & i & " - attglpensiun can't be empty" : GoTo selesai
                    End If

                    'atpenyusutanke(48) As Double
                    If Len(dataRowAsset(48)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atpenyusutanke can't be empty" : GoTo selesai
                    End If

                    'atnilaimenurun(49) As Double
                    If Len(dataRowAsset(49)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atnilaimenurun can't be empty" : GoTo selesai
                    End If

                    'atinputuser(57) As 
                    If Len(dataRowAsset(57)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atinputuser can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(57)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atinputuser should not be more than 20 character." : GoTo selesai
                    End If

                    'atinputtgl(58) As DateTime
                    If Len(dataRowAsset(58)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atinputtgl can't be empty" : GoTo selesai
                    End If

                    'atmodifikasiuser(59) As 
                    If Len(dataRowAsset(59)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasiuser can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowAsset(59)) > 20 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasiuser should not be more than 20 character." : GoTo selesai
                    End If

                    'atmodifikasitgl(60) As DateTime
                    If Len(dataRowAsset(60)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atmodifikasitgl can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl1(71) As Double
                    If Len(dataRowAsset(71)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl1 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl2(72) As Double
                    If Len(dataRowAsset(72)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl2 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl3(73) As Double
                    If Len(dataRowAsset(73)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl3 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl4(74) As Double
                    If Len(dataRowAsset(74)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl4 can't be empty" : GoTo selesai
                    End If

                    'atcustomdbl5(75) As Double
                    If Len(dataRowAsset(75)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdbl5 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate1(76) As Date
                    If Len(dataRowAsset(76)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate1 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate2(77) As Date
                    If Len(dataRowAsset(77)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate2 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate3(78) As Date
                    If Len(dataRowAsset(78)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate3 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate4(79) As Date
                    If Len(dataRowAsset(79)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate4 can't be empty" : GoTo selesai
                    End If

                    'atcustomdate5(80) As Date
                    If Len(dataRowAsset(80)) = 0 Then
                        result(2) = "Asset Row : " & i & " - atcustomdate5 can't be empty" : GoTo selesai
                    End If

                    'END OF VALIDASI DATA ASSET --------------------------------

                    If AsDataTableTambahData(dtasset, "atid~atasetid~atjenismutasi~atsumber~atidutama~atidbarang~atkode~atnama~atkategori~atcabang~atlokasi~atgudang~atdivisi~atsubdivisi~atcostcenter~atproyek~atcatatan~atnomor~attglbeli~attglpakai~atjml~atsatuan~atmatauang~atkurs~atharga~atdiskon~atjmldiskon~atpajak1~atjmlpajak1~atpajak2~atjmlpajak2~athargabeli~atnilairesidu~atumurekonomis~atbebanperbln~atakumulasibeban~atnilaibuku~atmetode~attabelpenyusutan~atintangible~atfiskal~atatastengahbulan~atrekasset~atrekakumdepresiasi~atrekdepresiasi~atrekpenghapusan~atprodusen~attglpensiun~atpenyusutanke~atnilaimenurun~atdispose~atpembelian~atpenjualan~atlocked~atstatus~atstatussebelumnya~atisclose~atinputuser~atinputtgl~atmodifikasiuser~atmodifikasitgl~atcustomtext1~atcustomtext2~atcustomtext3~atcustomtext4~atcustomtext5~atcustomint1~atcustomint2~atcustomint3~atcustomint4~atcustomint5~atcustomdbl1~atcustomdbl2~atcustomdbl3~atcustomdbl4~atcustomdbl5~atcustomdate1~atcustomdate2~atcustomdate3~atcustomdate4~atcustomdate5", dataRowAsset(0) & "~" & dataRowAsset(1) & "~" & dataRowAsset(2) & "~" & dataRowAsset(3) & "~" & dataRowAsset(4) & "~" & dataRowAsset(5) & "~" & dataRowAsset(6) & "~" & dataRowAsset(7) & "~" & dataRowAsset(8) & "~" & dataRowAsset(9) & "~" & dataRowAsset(10) & "~" & dataRowAsset(11) & "~" & dataRowAsset(12) & "~" & dataRowAsset(13) & "~" & dataRowAsset(14) & "~" & dataRowAsset(15) & "~" & dataRowAsset(16) & "~" & dataRowAsset(17) & "~" & dataRowAsset(18) & "~" & dataRowAsset(19) & "~" & dataRowAsset(20) & "~" & dataRowAsset(21) & "~" & dataRowAsset(22) & "~" & dataRowAsset(23) & "~" & dataRowAsset(24) & "~" & dataRowAsset(25) & "~" & dataRowAsset(26) & "~" & dataRowAsset(27) & "~" & dataRowAsset(28) & "~" & dataRowAsset(29) & "~" & dataRowAsset(30) & "~" & dataRowAsset(31) & "~" & dataRowAsset(32) & "~" & dataRowAsset(33) & "~" & dataRowAsset(34) & "~" & dataRowAsset(35) & "~" & dataRowAsset(36) & "~" & dataRowAsset(37) & "~" & dataRowAsset(38) & "~" & dataRowAsset(39) & "~" & dataRowAsset(40) & "~" & dataRowAsset(41) & "~" & dataRowAsset(42) & "~" & dataRowAsset(43) & "~" & dataRowAsset(44) & "~" & dataRowAsset(45) & "~" & dataRowAsset(46) & "~" & dataRowAsset(47) & "~" & dataRowAsset(48) & "~" & dataRowAsset(49) & "~" & dataRowAsset(50) & "~" & dataRowAsset(51) & "~" & dataRowAsset(52) & "~" & dataRowAsset(53) & "~" & dataRowAsset(54) & "~" & dataRowAsset(55) & "~" & dataRowAsset(56) & "~" & dataRowAsset(57) & "~" & dataRowAsset(58) & "~" & dataRowAsset(59) & "~" & dataRowAsset(60) & "~" & dataRowAsset(61) & "~" & dataRowAsset(62) & "~" & dataRowAsset(63) & "~" & dataRowAsset(64) & "~" & dataRowAsset(65) & "~" & dataRowAsset(66) & "~" & dataRowAsset(67) & "~" & dataRowAsset(68) & "~" & dataRowAsset(69) & "~" & dataRowAsset(70) & "~" & dataRowAsset(71) & "~" & dataRowAsset(72) & "~" & dataRowAsset(73) & "~" & dataRowAsset(74) & "~" & dataRowAsset(75) & "~" & dataRowAsset(76) & "~" & dataRowAsset(77) & "~" & dataRowAsset(78) & "~" & dataRowAsset(79) & "~" & dataRowAsset(80)) = False Then
                        result(2) = "Asset Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA ASSET ===========================================

            End If
        End If


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0
        Dim vStatus As Integer = 0, vTgl As String = ""

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)
                Dim rsValidasi As String = ""
                vStatus = drutama("ristatus")
                vTgl = AsFormatTanggal(drutama("ritgl"))


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 11
                Select Case drutama("ristatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ritgl")), AsFormatTanggal(drutama("ritgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                Dim MUFungsional As String = "", MUUtama As String = ""
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
                End If

                'SET MATA UANG UTAMA
                MUUtama = drutama("rimatauang")
                'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("ristatus") = 2 Or drutama("ristatus") = 1 Or drutama("ristatus") = 8 Or drutama("ristatus") = 9 Or drutama("ristatus") = 10 Or drutama("ristatus") = 11 Then

                    'JIKA TANPA GRN MAKA CEK BATCH DAN SERIAL
                    If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                        'VALIDASI BATCH SERIAL ---------------
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                        'END OF VALIDASI BATCH SERIAL --------

                        'VALIDASI ASSET ----------------------
                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                        'END OF VALIDASI ASSET ---------------
                    End If

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingPO, ftOutstandingPO, ftExistOutstandingGRN, ftOutstandingGRN, "", "", "", "", ftPO, ftGRN, drutama("rihargatermasukpajak"), ftExistOutstandingAP, ftOutstandingAP)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'FUNGSI SET TANGGAL JATUH TEMPO DIHILANGKAN, KARENA di flex tambah inputan
                'SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("ritermin").ToString, AsFormatTanggal(drutama("ritgl")), "ritgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("ritgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'SET TANGGAL JATUH TEMPO BERDASARKAN SETTING
                'JIKA SETTING BERDASARKAN TUKAR FAKTUR MAKA TANGGAL JATUH TEMPO DISET 2100-12-31
                Dim setTglJT As String = F_getSetting(4, "tukarfaktur", "UpdateTglJatuhTempoRI")
                If setTglJT.Equals("1") Then
                    drutama("ritgljatuhtempo") = "2100-12-31"
                End If


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TAMBAHKAN FIELD SUBTOTAL PADA COST
                'SUBTOTAL = jumlah
                AsDataTableTambahField(dtcost, "subtotal", AsEnumTypeData.AsDouble)
                dtcost.Columns("subtotal").Expression = "jumlah"

                'TOTAL = subtotal detail + subtotal cost
                drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal") + AsDataTableDSum(dtcost, "subtotal")

                'TOTAL = subtotal detail
                drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("ritotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("ritotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("rihargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN - JMLUANGMUKA 
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ritotalpajak1detail")) + Double.Parse(drutama("ritotalpajak2detail")) + Double.Parse(drutama("ribiayalain")) - Double.Parse(drutama("rijmluangmuka"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN - JMLUANGMUKA 
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ritotalpajak2detail")) + Double.Parse(drutama("ribiayalain")) - Double.Parse(drutama("rijmluangmuka"))

                End If
                'End Of PERHITUNGAN TOTAL UTAMA =========================


                'JIKA TUNAI MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("ricarabayar")) = 0 Then

                    'SET JML BAYAR ==========================================
                    If MUUtama = MUFungsional Then
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAH
                        drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlah", "carabayar <> 10")

                    Else
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAHVALAS
                        drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlahvalas", "carabayar <> 10")

                    End If
                    'END OF SET JML BAYAR ===================================


                    'SET TGL LUNAS ==========================================
                    'JIKA TUNAI MAKA TGL LUNAS = TGL TRANSAKSI
                    If Double.Parse(drutama("rijmlbayar")) >= Double.Parse(drutama("ritotaltransaksi")) Then
                        drutama("ritgllunas") = drutama("ritgl") : drutama("ristatuslunas") = 2

                    ElseIf Double.Parse(drutama("rijmlbayar")) < 1 Then
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                    Else
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 1

                    End If
                    'END OF SET TGL LUNAS ===================================

                Else
                    drutama("rijmlbayar") = 0 : drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("riid")
                    notransaksi = drutama("rinotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(riid), rinotransaksi FROM m4_ri WHERE riid='" & result(4) & "' AND ristatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("riautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"), drutama("risumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_ri_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Ri set ricabang  = '" & FixQuotes(drutama("ricabang")) & "', rilokasi  = '" & FixQuotes(drutama("rilokasi")) & "', rigudang  = '" & FixQuotes(drutama("rigudang")) & "', riasalbarang  = '" & FixQuotes(drutama("riasalbarang")) & "', riasalbarangkategori  = " & drutama("riasalbarangkategori") & ", rijenispembelian  = '" & FixQuotes(drutama("rijenispembelian")) & "', rijenispembeliankategori  = " & drutama("rijenispembeliankategori") & ", ricarabayar  = " & drutama("ricarabayar") & ", risumber  = '" & FixQuotes(drutama("risumber")) & "', riautonotransaksi  = " & drutama("riautonotransaksi") & ", rinotransaksi  = '" & FixQuotes(notransaksi) & "', ritgl  = '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', rikodepa  = " & drutama("rikodepa") & ", risupplier  = " & drutama("risupplier") & ", risupplierkontak  = '" & FixQuotes(drutama("risupplierkontak")) & "', ri1alamat1  = '" & FixQuotes(drutama("ri1alamat1")) & "', ri1alamat2  = '" & FixQuotes(drutama("ri1alamat2")) & "', ri1alamat3  = '" & FixQuotes(drutama("ri1alamat3")) & "', ri2alamat1  = '" & FixQuotes(drutama("ri2alamat1")) & "', ri2alamat2  = '" & FixQuotes(drutama("ri2alamat2")) & "', ri2alamat3  = '" & FixQuotes(drutama("ri2alamat3")) & "', ribagianpembelian  = " & drutama("ribagianpembelian") & ", ritermin  = '" & FixQuotes(drutama("ritermin")) & "', ritgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', riuraian  = '" & FixQuotes(drutama("riuraian")) & "', ricatatan  = '" & FixQuotes(drutama("ricatatan")) & "', rinoref  = '" & FixQuotes(drutama("rinoref")) & "', ritglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', ritglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', rimatauang  = '" & FixQuotes(drutama("rimatauang")) & "', rikurs  = '" & FixDouble(drutama("rikurs")) & "', rihargatermasukpajak  = " & drutama("rihargatermasukpajak") & ", ritotal  = '" & FixDouble(drutama("ritotal")) & "', ridiskonpersen  = '" & FixQuotes(drutama("ridiskonpersen")) & "', rijmldiskon  = '" & FixDouble(drutama("rijmldiskon")) & "', ritotalpajak1detail  = '" & FixDouble(drutama("ritotalpajak1detail")) & "', ritotalpajak2detail  = '" & FixDouble(drutama("ritotalpajak2detail")) & "', ribiayalainpersen  = '" & FixQuotes(drutama("ribiayalainpersen")) & "', ribiayalain  = '" & FixDouble(drutama("ribiayalain")) & "', ritotaltransaksi  = '" & FixDouble(drutama("ritotaltransaksi")) & "', rijmlbayar  = '" & FixDouble(drutama("rijmlbayar")) & "', ristatuslunas  = " & drutama("ristatuslunas") & ", ritgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', rinofakturpajak  = '" & FixQuotes(drutama("rinofakturpajak")) & "', risdhbayarpajak  = " & drutama("risdhbayarpajak") & ", ritglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', rirekdiskon  = '" & FixQuotes(drutama("rirekdiskon")) & "', rirekpajak1  = '" & FixQuotes(drutama("rirekpajak1")) & "', rirekpajak2  = '" & FixQuotes(drutama("rirekpajak2")) & "', rirekbiayalain  = '" & FixQuotes(drutama("rirekbiayalain")) & "', rirekbayar  = '" & FixQuotes(drutama("rirekbayar")) & "', riidpr  = " & drutama("riidpr") & ", riidcs  = " & drutama("riidcs") & ", riidrq  = " & drutama("riidrq") & ", riidbs  = " & drutama("riidbs") & ", riidpo  = " & drutama("riidpo") & ", riidipc  = " & drutama("riidipc") & ", riidgrn  = " & drutama("riidgrn") & ", ristatusdnr  = " & drutama("ristatusdnr") & ", ristatusprt  = " & drutama("ristatusprt") & ", ristatus  = " & drutama("ristatus") & ", ristatussebelumnya  = " & drutama("ristatussebelumnya") & ", rijmlrevisi  = rijmlrevisi+1, ricetakanke  = " & drutama("ricetakanke") & ", rimodifikasiuser  = " & drutama("rimodifikasiuser") & ", rimodifikasitgl  = NOW(), riposting  = 0, ritutupperiode  = " & drutama("ritutupperiode") & ", ricustomtext1  = '" & FixQuotes(drutama("ricustomtext1")) & "', ricustomtext2  = '" & FixQuotes(drutama("ricustomtext2")) & "', ricustomtext3  = '" & FixQuotes(drutama("ricustomtext3")) & "', ricustomtext4  = '" & FixQuotes(drutama("ricustomtext4")) & "', ricustomtext5  = '" & FixQuotes(drutama("ricustomtext5")) & "', ricustomint1  = " & drutama("ricustomint1") & ", ricustomint2  = " & drutama("ricustomint2") & ", ricustomint3  = " & drutama("ricustomint3") & ", ricustomdbl1  = '" & FixDouble(drutama("ricustomdbl1")) & "', ricustomdbl2  = '" & FixDouble(drutama("ricustomdbl2")) & "', ricustomdbl3  = '" & FixDouble(drutama("ricustomdbl3")) & "', ricustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', ricustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', ricustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', rijmluangmuka = '" & FixDouble(drutama("rijmluangmuka")) & "', rirekuangmuka = '" & FixQuotes(drutama("rirekuangmuka")) & "', riidap = '" & FixDouble(drutama("riidap")) & "' where riid = '" & drutama("riid") & "'"
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

                    If drutama("riautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"), drutama("risumber"), 4)
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
                        notransaksi = drutama("rinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Ri (ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, rijmluangmuka, rirekuangmuka, riidap) values('" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("rigudang")) & "', '" & FixQuotes(drutama("riasalbarang")) & "', " & drutama("riasalbarangkategori") & ", '" & FixQuotes(drutama("rijenispembelian")) & "', " & drutama("rijenispembeliankategori") & ", " & drutama("ricarabayar") & ", '" & FixQuotes(drutama("risumber")) & "', " & drutama("riautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drutama("risupplierkontak")) & "', '" & FixQuotes(drutama("ri1alamat1")) & "', '" & FixQuotes(drutama("ri1alamat2")) & "', '" & FixQuotes(drutama("ri1alamat3")) & "', '" & FixQuotes(drutama("ri2alamat1")) & "', '" & FixQuotes(drutama("ri2alamat2")) & "', '" & FixQuotes(drutama("ri2alamat3")) & "', " & drutama("ribagianpembelian") & ", '" & FixQuotes(drutama("ritermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(drutama("rinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', " & drutama("rihargatermasukpajak") & ", '" & FixDouble(drutama("ritotal")) & "', '" & FixQuotes(drutama("ridiskonpersen")) & "', '" & FixDouble(drutama("rijmldiskon")) & "', '" & FixDouble(drutama("ritotalpajak1detail")) & "', '" & FixDouble(drutama("ritotalpajak2detail")) & "', '" & FixQuotes(drutama("ribiayalainpersen")) & "', '" & FixDouble(drutama("ribiayalain")) & "', '" & FixDouble(drutama("ritotaltransaksi")) & "', '" & FixDouble(drutama("rijmlbayar")) & "', " & drutama("ristatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', '" & FixQuotes(drutama("rinofakturpajak")) & "', " & drutama("risdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', '" & FixQuotes(drutama("rirekdiskon")) & "', '" & FixQuotes(drutama("rirekpajak1")) & "', '" & FixQuotes(drutama("rirekpajak2")) & "', '" & FixQuotes(drutama("rirekbiayalain")) & "', '" & FixQuotes(drutama("rirekbayar")) & "', " & drutama("riidpr") & ", " & drutama("riidcs") & ", " & drutama("riidrq") & ", " & drutama("riidbs") & ", " & drutama("riidpo") & ", " & drutama("riidipc") & ", " & drutama("riidgrn") & ", " & drutama("ristatusdnr") & ", " & drutama("ristatusprt") & ", " & drutama("ristatus") & ", " & drutama("ristatussebelumnya") & ", " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", NOW(), " & drutama("rimodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ritutupperiode") & ", " & drutama("riisclose") & ", '" & FixQuotes(drutama("ricustomtext1")) & "', '" & FixQuotes(drutama("ricustomtext2")) & "', '" & FixQuotes(drutama("ricustomtext3")) & "', '" & FixQuotes(drutama("ricustomtext4")) & "', '" & FixQuotes(drutama("ricustomtext5")) & "', " & drutama("ricustomint1") & ", " & drutama("ricustomint2") & ", " & drutama("ricustomint3") & ", '" & FixDouble(drutama("ricustomdbl1")) & "', '" & FixDouble(drutama("ricustomdbl2")) & "', '" & FixDouble(drutama("ricustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', '" & FixDouble(drutama("rijmluangmuka")) & "', '" & FixQuotes(drutama("rirekuangmuka")) & "', '" & FixDouble(drutama("riidap")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select riid from M4_ri where rinotransaksi='" & notransaksi & "' AND riinputuser= '" & userid & "' order by rimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ri_Detail where idri = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idridetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangsementara")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Ri_Detail(idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ri_Cost where idri = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses cost
                If (dtcost.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtcost.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idricost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", " & dr1("idpocost") & ", " & dr1("idipccost") & ", " & dr1("idgrncost") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', '" & FixQuotes(dr1("kontak")) & "', '" & FixQuotes(dr1("termasukhpp")) & "')")
                    Next
                    sql = "Insert into M4_Ri_Cost(idricost, idri, kodecost, matauang, kurs, jumlah, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rekdebit, rekkredit, kontak, termasukhpp) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_ri_Pay where idri = '" & result(4) & "'"
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
                'If (dtpay.Rows.Count > 0) And drutama("ricarabayar") = 0 Then
                If (dtpay.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder, strVoucher As New StringBuilder, ftVoucher As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idricarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("sumber")) & "', " & dr1("idtransaksi") & ", '" & FixDouble(dr1("totaltransaksi")) & "', '" & FixDouble(dr1("terbayar")) & "')")
                    Next
                    sql = "Insert into M4_ri_Pay(idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, sumber, idtransaksi, totaltransaksi, terbayar) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RI'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RI'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus asset ketika update
                If (isUpdate) Then
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'RI'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses asset
                If (dtasset.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('0', '" & FixQuotes(dr1("atasetid")) & "', " & dr1("atjenismutasi") & ", '" & FixQuotes(dr1("atsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("atidbarang")) & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & vStatus & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(vTgl)) & "')")
                    Next
                    sql = "Insert into M7_Asset_Transaction(atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atnotransaksi, attgl) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Dim sumber As String = "RI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("ristatus") = 2 Then
                    If Len(updNilaiPO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpo FROM M4_po_detail WHERE " & updFilterPO & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPO = "" : updFilterPO = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                                updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                            Next

                            sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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

                    If Len(updNilaiGRN) > 0 Then 'GRN
                        'UPDATE DETAIL
                        sql = "UPDATE m4_grn_detail SET jmlrealisasi = (CASE idgrndetail " & updNilaiGRN & " ELSE jmlrealisasi END) WHERE " & updFilterGRN
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idgrn FROM m4_grn_detail WHERE " & updFilterGRN & " GROUP BY idgrn", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idgrn = '" & dr1("idgrn") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idgrn, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_grn_detail WHERE " & ftDetail & " GROUP BY idgrn", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiGRN = "" : updFilterGRN = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiGRN = String.Concat(updNilaiGRN, "WHEN '" & dr1("idgrn") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                                updFilterGRN = String.Concat(updFilterGRN, "(grnid = '" & dr1("idgrn") & "')")
                            Next

                            sql = "UPDATE m4_grn SET grnstatusrealisasi = (CASE grnid " & updNilaiGRN & " ELSE grnstatusrealisasi END) WHERE " & updFilterGRN
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


                    If Len(updNilaiAP) > 0 Then 'Ap
                        'TRANSAKSI
                        sql = "UPDATE m4_Ap `Ap` JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' SET `Ap`.Apjumlahbayar = (CASE `Ap`.Apid " & updNilaiAP & " ELSE `Ap`.Apjumlahbayar END), `Ap`.Apjumlahbayarvalas = (CASE `Ap`.Apid " & updNilaiValasAP & " ELSE `Ap`.Apjumlahbayarvalas END), `Ap`.Aptgllunas = (CASE `Ap`.Apid " & updTglLunasAP & " ELSE `Ap`.Aptgllunas END) WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_Ap `Ap` JOIN m2_transaction_journal t ON `Ap`.Apsumber = t.tsumber AND `Ap`.Apid =  t.tidtransaksi AND `Ap`.Apnotransaksi = t.tnotransaksi SET t.tstatuslunas = `Ap`.Apstatusbayar, t.ttgllunas = `Ap`.Aptgllunas WHERE " & updFilterAP
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO BATCH =========================================================


                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                        Next
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO SERIAL ========================================================


                    'INSERT NO ASSET ===============================================================
                    If dtasset.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtasset.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("('" & 0 & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & dr1("atstatus") & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(dr1("atidbarang")) & "')")
                        Next
                        sql = "Insert into M7_Asset(aid, akode, anama, akategori, acabang, alokasi, agudang, adivisi, asubdivisi, acostcenter, aproyek, acatatan, anomor, atglbeli, atglpakai, ajml, asatuan, amatauang, akurs, aharga, adiskon, ajmldiskon, apajak1, ajmlpajak1, apajak2, ajmlpajak2, ahargabeli, anilairesidu, aumurekonomis, abebanperbln, aakumulasibeban, anilaibuku, ametode, atabelpenyusutan, aintangible, afiskal, aatastengahbulan, arekasset, arekakumdepresiasi, arekdepresiasi, arekpenghapusan, aprodusen, atglpensiun, apenyusutanke, anilaimenurun, adispose, apembelian, apenjualan, alocked, astatus, astatussebelumnya, aisclose, ainputuser, ainputtgl, amodifikasiuser, amodifikasitgl, acustomtext1, acustomtext2, acustomtext3, acustomtext4, acustomtext5, acustomint1, acustomint2, acustomint3, acustomint4, acustomint5, acustomdbl1, acustomdbl2, acustomdbl3, acustomdbl4, acustomdbl5, acustomdate1, acustomdate2, acustomdate3, acustomdate4, acustomdate5, aidbarang) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF INSERT NO ASSET ========================================================


                    'UPDATE STOK BOOKING ============================================================
                    If Len(updStokOutBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =====================================================


                    'JIKA TANPA GRN MAKA HITUNG TRANSAKSI BARANG DAN POSTING HPP
                    If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                        'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                        'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rid.idridetail, rid.idbarang, rid.namabarang, rid.tipebarang, rid.jml, rid.satuan, rid.jmlbarang, rid.satuanbarang, rid.matauang, rid.kurs, rid.harga, rid.diskon, rid.jmldiskon, rid.gudang, rid.catatan, rid.costcenter, rid.divisi, rid.subdivisi, rid.proyek, ri.riinputtgl, i.bhpp, rid.jmlpajak1, rid.jmlpajak2 FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE rid.idri = '" & result(4) & "' ORDER BY rid.urutan", myConn)
                        Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rid.idridetail, rid.idbarang, rid.namabarang, rid.tipebarang, rid.jml, rid.satuan, rid.jmlbarang, rid.satuanbarang, rid.matauang, rid.kurs, rid.harga, rid.diskon, rid.jmldiskon, rid.gudang, rid.catatan, rid.costcenter, rid.divisi, rid.subdivisi, rid.proyek, ri.riinputtgl, i.bhpp, rid.jmlpajak1, rid.jmlpajak2, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid LEFT JOIN m1_cost_center cc ON rid.costcenter = cc.cckode WHERE rid.idri = '" & result(4) & "' ORDER BY rid.urutan", myConn)

                        Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                        Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                        Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                        If dtDetailNew.Rows.Count > 0 Then

                            'INSERT ITEM TRANSACTION ====================================================
                            For Each dr1 As DataRow In dtDetailNew.Rows
                                If Double.Parse(dr1("transbarang")) = 1 Then
                                    'SET NILAI VARIABEL
                                    idbarang = Double.Parse(dr1("idbarang"))
                                    jmlbarang = Double.Parse(dr1("jmlbarang"))
                                    gudang = dr1("gudang")

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

                                        'hitung saldojml = bstok + jmlbarang
                                        saldojml = bstok + jmlbarang

                                        'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                        hpp = 0 : saldohpp = 0 : saldonilai = 0

                                        'QUERY INSERT TRANSAKSI BARANG
                                        strTransaksiBarang.Clear()
                                        'mapping                        id,                            cabang,                                    lokasi,                             gudang,                        kodepa,           jenismutasi,                              sumber,              idutama,                  iddetail,                      notransaksi,                                                 tgl,                          kontak,                 idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                        strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rikodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("risumber")) & "', " & result(4) & ", " & dr1("idridetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("risupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("riinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("riinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                        sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .Transaction = Trans
                                            .CommandType = CommandType.Text
                                            .CommandText = sql
                                        End With
                                        objCmd.ExecuteNonQuery()

                                        'UPDATE STOK GLOBAL
                                        'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                        If drutama("rihargatermasukpajak") = 0 Then
                                            sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                        Else
                                            sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                        End If
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
                            Next
                            'END OF INSERT ITEM TRANSACTION =============================================

                        Else
                            result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If


                        'INSERT MSMQ COGS ===============================================================
                        If drutama("ristatus") = 2 Then
                            Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                            'BUAT ID UNIQUE
                            mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

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
                            Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                            If ProsesHpp.Equals("0") = False Then
                                hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                                If Len(hasilMsmq) > 0 Then
                                    result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                        End If
                        'END OF INSERT MSMQ COGS ========================================================

                    End If

                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================


                'INSERT MSMQ JURNAL =================================================================
                If drutama("ristatus") = 2 Then
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
    Public Function M4_RiUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ritgl, Rinotransaksi, Ristatus FROM M4_Ri WHERE Riid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ristatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_ri_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_ri_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'OUTSTANDING UANG MUKA ==========================================================
                Dim OutstandingUM As Double = 0, OutstandingUMValas As Double = 0, tglLunas = "1900-01-01"
                Dim updNilaiAp As String = "", updNilaiValasAp As String = "", updFilterAp As String = "", MUUTama As String = ""
                Dim idAp As Integer = 0

                'AMBIL MATA UANG FUNGSIONAL DARI SETTING
                Dim MUFungsional As String = ""
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
                End If

                'AMBIL DATA UTAMA
                Dim dtUtama As DataTable = AsDataTableAmbilDariDBCon("SELECT rimatauang, rikurs, rijmluangmuka, riidap FROM m4_ri WHERE riid = '" & FixDouble(idtransaksi) & "'", myConn)
                If dtUtama.Rows.Count > 0 Then
                    MUUTama = FxDB(dtUtama.Rows(0)("rimatauang"), "")

                    Dim dtpay As DataTable = AsDataTableAmbilDariDBCon("SELECT rip.* FROM m4_ri_pay rip WHERE idri = '" & FixDouble(idtransaksi) & "' AND sumber = 'AP'", myConn)
                    For Each dr1 As DataRow In dtpay.Rows
                        '1.SET NILAI VARIABEL
                        idAp = dr1("idtransaksi")

                        '2. SET NILAI UPDATE OUTSTANDING
                        OutstandingUM = AsDataTableDSum(dtpay, "jumlah", "sumber = 'AP' AND idtransaksi = " & idAp & "")
                        OutstandingUMValas = AsDataTableDSum(dtpay, "jumlahvalas", "sumber = 'AP' AND idtransaksi = " & idAp & "")

                        '3. SET NILAI UPDATE OUTSTANDING
                        updNilaiAp = String.Concat("WHEN '" & idAp & "' THEN ROUND(`Ap`.Apjumlahbayar - '" & OutstandingUM & "', 5) ", updNilaiAp)
                        updNilaiValasAp = String.Concat("WHEN '" & idAp & "' THEN ROUND(`Ap`.Apjumlahbayarvalas - '" & OutstandingUMValas & "', 5) ", updNilaiValasAp)

                        '4. SET FILTER UPDATE OUTSTANDING
                        updFilterAp = IIf(Len(updFilterAp.ToString) = 0, "", updFilterAp & " OR ")
                        updFilterAp = String.Concat(updFilterAp, "(`Ap`.Apid = '" & idAp & "')")

                    Next
                End If
                'END OF OUTSTANDING UANG MUKA ===================================================


                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updStokOut As String = "", gudangOut As String = "", updStokInBooking As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idridetail As Integer = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0
                Dim updNilaiPO As String = "", updFilterPO As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""

                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idridetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, idgrndetail, gudang, urutan, rijenispembeliankategori FROM m4_ri_detail JOIN m4_ri ON idri = riid WHERE idri = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idridetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, idgrndetail, gudang, urutan, rijenispembeliankategori, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_ri_detail rid JOIN m4_ri ri ON idri = riid LEFT JOIN m1_cost_center cc ON rid.costcenter = cc.cckode WHERE idri = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idridetail = dr1("idridetail") : jmlbarang = dr1("jmlbarang") : idpodetail = dr1("idpodetail") : idgrndetail = dr1("idgrndetail") : gudangOut = dr1("gudang")

                        'JIKA RI TANPA GRN MAKA CEK STOK
                        If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                            If dr1("transbarang") = 1 Then
                                'BUAT FILTER CEK HPP KHUSUS(I)
                                ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                                ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idridetail & "' AND sumber = 'RI')")

                                'BUAT FILER CEK HPP FIFO(F)
                                ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                                ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idridetail & "' AND cfisumber = 'RI')")

                                'BUAT FILTER CEK STOCK EXIST
                                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                                'BUAT FILTER CEK JML STOCK
                                Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudang='" & gudangOut & "' AND transbarang = 1")
                                ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                                'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                                ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                                'SET NILAI UPDATE STOK KELUAR
                                updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                                updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok
                            End If
                        End If


                        'UPDATE OUTSTANDING ---------------------------
                        If idpodetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PO
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                            updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPO)
                            '2. SET FILTERUPDATE OUTSTANDING PO
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                            If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                                'SET NILAI UPDATE STOK BOOKING MASUK
                                updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                                updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangOut & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok
                            End If
                        End If

                        If idgrndetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING GRN
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idgrndetail=" & idgrndetail)
                            updNilaiGRN = String.Concat("WHEN '" & idgrndetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiGRN)
                            '2. SET FILTERUPDATE OUTSTANDING GRN
                            updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                            updFilterGRN = String.Concat(updFilterGRN, "(idgrndetail = '" & idgrndetail & "')")
                        End If


                        If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                            If dr1("transbarang") = 1 Then
                                'SET NILAI UPDATE STOK BARANG
                                Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND transbarang = 1")
                                updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                                'SET FILTERUPDATE STOK BARANG
                                ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                                ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                            End If
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftHppI, ftHppF, ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                'UPDATE JMLBAYAR AP (UM PEMB)
                If Len(updNilaiAp) > 0 And Len(updNilaiValasAp) > 0 Then 'Ap
                    'TRANSAKSI
                    sql = "UPDATE m4_Ap `Ap` SET `Ap`.Apjumlahbayar = (CASE `Ap`.Apid " & updNilaiAp & " ELSE `Ap`.Apjumlahbayar END), `Ap`.Apjumlahbayarvalas = (CASE `Ap`.Apid " & updNilaiValasAp & " ELSE `Ap`.Apjumlahbayarvalas END), `Ap`.Aptgllunas = '" & FixQuotes(tglLunas) & "' WHERE " & updFilterAp
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'JURNAL
                    sql = "UPDATE m4_Ap `Ap` LEFT JOIN m2_transaction_journal t ON `Ap`.Apsumber = t.tsumber AND `Ap`.Apid = t.tidtransaksi AND `Ap`.Apnotransaksi = t.tnotransaksi SET t.tstatuslunas = `Ap`.Apstatusbayar, t.ttgllunas = `Ap`.Aptgllunas WHERE " & updFilterAp
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                If Len(updFilterPO) > 0 Then 'PO
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpo FROM M4_po_detail WHERE " & updFilterPO & " GROUP BY idpo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPO = "" : updFilterPO = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                        Next

                        sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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

                If Len(updFilterGRN) > 0 Then 'GRN
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_grn_detail SET jmlrealisasi = (CASE idgrndetail " & updNilaiGRN & " ELSE jmlrealisasi END) WHERE " & updFilterGRN
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idgrn FROM m4_grn_detail WHERE " & updFilterGRN & " GROUP BY idgrn", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idgrn = '" & dr1("idgrn") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idgrn, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_grn_detail WHERE " & ftDetail & " GROUP BY idgrn", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiGRN = "" : updFilterGRN = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiGRN = String.Concat(updNilaiGRN, "WHEN '" & dr1("idgrn") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                            updFilterGRN = String.Concat(updFilterGRN, "(grnid = '" & dr1("idgrn") & "')")
                        Next

                        sql = "UPDATE m4_grn SET grnstatusrealisasi = (CASE grnid " & updNilaiGRN & " ELSE grnstatusrealisasi END) WHERE " & updFilterGRN
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
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================


                'DELETE HPP KHUSUS (I)
                If Len(ftHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE HPP FIFO (F)
                If Len(ftHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE NO BATCH IN MASUK ---------------------------
                sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE NO SERIAL IN MASUK --------------------------
                sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE NO ASSET IN MASUK --------------------------
                sql = "DELETE a FROM m7_asset_transaction atr JOIN m4_ri ri ON atr.atsumber = ri.risumber AND atr.atidutama = ri.riid AND ri.riid = '" & idtransaksi & "' JOIN m7_asset a ON atr.atkode = a.akode"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'UPDATE STOK BOOKING ============================================================
                If Len(updStokInBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =====================================================


                'UPDATE STOK ==================================================================
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

                'STOK BARANG m1_item
                If Len(updStokBarang) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarang & " ELSE bstok END) WHERE " & ftStokBarang
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK ===========================================================


                'DELETE TRANSAKSI BARANG ======================================================
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
                'END OF DELETE TRANSAKSI BARANG ===============================================


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m4_ri_detail rid ON i.bid = rid.idbarang AND rid.idri = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m4_ri_detail rid ON it.idbarang = rid.idbarang AND rid.idri = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m4_ri ri ON rid.idri = ri.riid AND CONCAT(it.sumber,it.idutama) <> CONCAT(ri.risumber,ri.riid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                Dim dtTotalFungsional As DataTable = AsDataTableAmbilDariDBCon("SELECT SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END)) as total FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid WHERE rid.idri = '" & FixDouble(idtransaksi) & "'", myConn)
                Dim dtBiayaFungsional As DataTable = AsDataTableAmbilDariDBCon("SELECT IFNULL(SUM(ric.jumlah * ric.kurs),0) as biaya FROM m4_ri ri LEFT JOIN m4_ri_cost ric ON ri.riid = ric.idri AND ric.termasukhpp = 1 WHERE ri.riid = '" & FixDouble(idtransaksi) & "'", myConn)
                Dim vTotalFungsional As Double = 0, vBiayaFungsional As Double = 0
                If dtTotalFungsional.Rows.Count > 0 Then
                    vTotalFungsional = Double.Parse(FixDouble(FxDB(dtTotalFungsional.Rows(0)("total"), 0)))
                End If
                If dtBiayaFungsional.Rows.Count > 0 Then
                    vBiayaFungsional = Double.Parse(FixDouble(FxDB(dtBiayaFungsional.Rows(0)("biaya"), 0)))
                End If

                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT rid.idbarang, "
                sql &= " ROUND((CASE " & FixDouble(vTotalFungsional) & " "
                sql &= " WHEN 0 THEN (SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " ELSE (SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " + (((SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " / " & FixDouble(vTotalFungsional) & ") * " & FixDouble(vBiayaFungsional) & ") END), 2) as nilai, "
                sql &= " SUM(rid.jmlbarang) as jumlah "
                sql &= " FROM m4_ri_detail rid "
                sql &= " JOIN m4_ri ri ON rid.idri = ri.riid "
                sql &= " WHERE rid.idri = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY rid.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE BHPPAVERAGE M1_ITEM ============================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RI' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Ri SET Ristatus = " & nilaiStatus & ", Rimodifikasiuser='" & userid & "', Rimodifikasitgl = NOW(), Riposting = 0, Ripostingtgl = '1971-01-01 00:00:00', Rijmlrevisi = Rijmlrevisi + 1 WHERE Riid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RiDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Riid, Rinotransaksi FROM M4_Ri WHERE Riid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ricabang, rilokasi, risumber, riautonotransaksi, rinotransaksi, ritgl"
            sql &= " FROM M4_ri"
            sql &= " WHERE riid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ricabang")
                lokasi = dtNomorNext.Rows(0)("rilokasi")
                sumber = dtNomorNext.Rows(0)("risumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("riautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ritgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS ASSET
            sql = "Delete from M7_Asset_Transaction where atidutama = '" & idtransaksi & "' AND atsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE COST
            sql = "DELETE FROM M4_Ri_Cost WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE PAY
            sql = "DELETE FROM M4_ri_Pay WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M4_Ri_Detail WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Ri WHERE riid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RiGetdataById(ByVal param As String) As String

        'M4_RiGetdataById Utama --------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, 
        'ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, 
        'ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, 
        'rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, kpkp, rijmluangmuka, rirekuangmuka, riidap, rirekuangmukanama, apnotransaksi

        'M4_RiGetdataById Detail -------------------------------------------------------
        'idridetail, idri, idbarang, 
        'namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, 
        'rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, 
        'bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, 
        'gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, 
        'grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_RiGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_RiGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_RiGetdataById Cost --------------------------------------------------------
        'idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, 
        'idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, 
        'jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, 
        'customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, 
        'rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp

        'M4_RiGetdataById Pay -------------------------------------------------------
        'idricarabayar, idri, carabayar, matauang, 
        'kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, 
        'rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, 
        'rekbanknama, rekgironama, sumber, idtransaksi, totaltransaksi, terbayar, notransaksi, tgl

        'M4_RiGetdataById Asset --------------------------------------------------------
        'atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, 
        'atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, 
        'atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, 
        'atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, 
        'atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, 
        'atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, 
        'atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, 
        'atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, 
        'atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, 
        'atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, 
        'atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, 
        'atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, 
        'atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, 
        'atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, 
        'atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama

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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", cost As String = "", idtransaksi As String = ""
        Dim pay As String = "", asset As String = ""

        Dim sumber As String = "RI"

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

        Dim NmMemcached As String = "aplikasi1-M4_Ri~M4_Ri_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "riid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "riid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ri_getdata")
        sql = "select ri.riid AS riid,ri.ricabang AS ricabang,ri.rilokasi AS rilokasi,ri.rigudang AS rigudang,ri.riasalbarang AS riasalbarang,ri.riasalbarangkategori AS riasalbarangkategori,ri.rijenispembelian AS rijenispembelian,ri.rijenispembeliankategori AS rijenispembeliankategori,ri.ricarabayar AS ricarabayar,ri.risumber AS risumber,ri.riautonotransaksi AS riautonotransaksi,ri.rinotransaksi AS rinotransaksi,ri.ritgl AS ritgl,ri.rikodepa AS rikodepa,ri.risupplier AS risupplier,ri.risupplierkontak AS risupplierkontak,ri.ri1alamat1 AS ri1alamat1,ri.ri1alamat2 AS ri1alamat2,ri.ri1alamat3 AS ri1alamat3,ri.ri2alamat1 AS ri2alamat1,ri.ri2alamat2 AS ri2alamat2,ri.ri2alamat3 AS ri2alamat3,ri.ribagianpembelian AS ribagianpembelian,ri.ritermin AS ritermin,ri.ritgljatuhtempo AS ritgljatuhtempo,ri.riuraian AS riuraian,ri.ricatatan AS ricatatan,ri.rinoref AS rinoref,ri.ritglnoref AS ritglnoref,ri.ritglpenutupan AS ritglpenutupan,ri.rimatauang AS rimatauang,ri.rikurs AS rikurs,ri.rihargatermasukpajak AS rihargatermasukpajak,ri.ritotal AS ritotal,ri.ridiskonpersen AS ridiskonpersen,ri.rijmldiskon AS rijmldiskon,ri.ritotalpajak1detail AS ritotalpajak1detail,ri.ritotalpajak2detail AS ritotalpajak2detail,ri.ribiayalainpersen AS ribiayalainpersen,ri.ribiayalain AS ribiayalain,ri.ritotaltransaksi AS ritotaltransaksi,ri.rijmlbayar AS rijmlbayar,ri.ristatuslunas AS ristatuslunas,ri.ritgllunas AS ritgllunas,ri.rinofakturpajak AS rinofakturpajak,ri.risdhbayarpajak AS risdhbayarpajak,ri.ritglbayarpajak AS ritglbayarpajak,ri.rirekdiskon AS rirekdiskon,ri.rirekpajak1 AS rirekpajak1,ri.rirekpajak2 AS rirekpajak2,ri.rirekbiayalain AS rirekbiayalain,ri.rirekbayar AS rirekbayar,ri.riidpr AS riidpr,ri.riidcs AS riidcs,ri.riidrq AS riidrq,ri.riidbs AS riidbs,ri.riidpo AS riidpo,ri.riidipc AS riidipc,ri.riidgrn AS riidgrn,ri.ristatusdnr AS ristatusdnr,ri.ristatusprt AS ristatusprt,ri.ristatusrealisasi AS ristatusrealisasi,ri.ristatus AS ristatus,ri.ristatussebelumnya AS ristatussebelumnya,ri.rijmlrevisi AS rijmlrevisi,ri.ricetakanke AS ricetakanke,ri.riinputuser AS riinputuser,ri.riinputtgl AS riinputtgl,ri.rimodifikasiuser AS rimodifikasiuser,ri.rimodifikasitgl AS rimodifikasitgl,ri.riposting AS riposting,ri.ripostingtgl AS ripostingtgl,ri.ritutupperiode AS ritutupperiode,ri.riisclose AS riisclose,ri.ricustomtext1 AS ricustomtext1,ri.ricustomtext2 AS ricustomtext2,ri.ricustomtext3 AS ricustomtext3,ri.ricustomtext4 AS ricustomtext4,ri.ricustomtext5 AS ricustomtext5,ri.ricustomint1 AS ricustomint1,ri.ricustomint2 AS ricustomint2,ri.ricustomint3 AS ricustomint3,ri.ricustomdbl1 AS ricustomdbl1,ri.ricustomdbl2 AS ricustomdbl2,ri.ricustomdbl3 AS ricustomdbl3,ri.ricustomdate1 AS ricustomdate1,ri.ricustomdate2 AS ricustomdate2,ri.ricustomdate3 AS ricustomdate3,br.bnama AS ricabangnama,lc.lnama AS rilokasinama,wh.wnama AS rigudangnama,c1.kkode AS risupplierkode,c1.knama AS risuppliernama,c2.kkode AS ribagianpembeliankode,c2.knama AS ribagianpembeliannama,tr.trnama AS riterminnama,tr.trharijatuhtempo AS riterminharijatuhtempo,coa1.cnama AS rirekdiskonnama,coa2.cnama AS rirekpajak1nama,coa3.cnama AS rirekpajak2nama,coa4.cnama AS rirekbiayalainnama,coa5.cnama AS rirekbayarnama,po.ponotransaksi AS rinotransaksipo,ipc.ipcnotransaksi AS rinotransaksiipc,grn.grnnotransaksi AS rinotransaksigrn,st1.nama AS ristatusnama,st2.nama AS ristatussebelumnyanama,u1.unama AS riinputusernama,u2.unama AS rimodifikasiusernama,rid.idridetail AS idridetail,rid.idri AS idri,rid.idbarang AS idbarang,rid.namabarang AS namabarang,rid.tipebarang AS tipebarang,rid.jml AS jml,rid.satuan AS satuan,rid.nilaisatuan AS nilaisatuan,rid.jmlbarang AS jmlbarang,rid.satuanbarang AS satuanbarang,rid.matauang AS matauang,rid.kurs AS kurs,rid.hargafix AS hargafix,rid.harga AS harga,rid.diskon AS diskon,rid.jmldiskon AS jmldiskon,rid.pajak1 AS pajak1,rid.jmlpajak1 AS jmlpajak1,rid.pajak2 AS pajak2,rid.jmlpajak2 AS jmlpajak2,rid.cabang AS cabang,rid.lokasi AS lokasi,rid.gudang AS gudang,i.brekpersediaan AS rekpersediaan,i.brekdiskonpembelian AS rekdiskonpembelian,rid.rekhutangsementara AS rekhutangsementara,rid.costcenter AS costcenter,rid.divisi AS divisi,rid.subdivisi AS subdivisi,rid.proyek AS proyek,rid.catatan AS catatan,rid.urutan AS urutan,rid.idprdetail AS idprdetail,rid.idcsdetail AS idcsdetail,rid.idrqdetail AS idrqdetail,rid.idbsdetail AS idbsdetail,rid.idpodetail AS idpodetail,rid.idipcdetail AS idipcdetail,rid.idgrndetail AS idgrndetail,rid.jmldnr AS jmldnr,rid.statusdnr AS statusdnr,rid.jmlprt AS jmlprt,rid.statusprt AS statusprt,rid.jmlrealisasi AS jmlrealisasi,rid.statusrealisasi AS statusrealisasi,rid.isclose AS isclose,rid.customtext1 AS customtext1,rid.customtext2 AS customtext2,rid.customtext3 AS customtext3,rid.customdbl1 AS customdbl1,rid.customdbl2 AS customdbl2,rid.customdbl3 AS customdbl3,rid.customdate1 AS customdate1,rid.customdate2 AS customdate2,rid.customdate3 AS customdate3,i.bkode AS kodebarang,i.bhpp AS bhpp,i.bjenis AS bjenis,i.bserial AS bserial,i.bbatch AS bbatch,i.basset AS basset,t1.tnama AS pajak1nama,t1.tnilai AS pajak1nilai,t2.tnama AS pajak2nama,t2.tnilai AS pajak2nilai,brd.bnama AS cabangnama,lcd.lnama AS lokasinama,whd.wnama AS gudangnama,cc.ccnama AS costcenternama,d.dnama AS divisinama,sd.sdnama AS subdivisinama,p.pnama AS proyeknama,po2.ponotransaksi AS ponotransaksi,ipc2.ipcnotransaksi AS ipcnotransaksi,grn2.grnnotransaksi AS grnnotransaksi, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, ri.rijmluangmuka, ri.rirekuangmuka, ri.riidap, coa6.cnama as rirekuangmukanama, ap.apnotransaksi as apnotransaksi  from m4_ri ri join m4_ri_detail rid on ri.riid = rid.idri left join m1_branch br on br.bkode = ri.ricabang left join m1_location lc on lc.lkode = ri.rilokasi left join m1_warehouse wh on wh.wkode = ri.rigudang left join m1_contact c1 on c1.kid = ri.risupplier left join m1_contact c2 on c2.kid = ri.ribagianpembelian left join m1_terms tr on ri.ritermin = tr.trkode left join m1_coa coa1 on ri.rirekdiskon = coa1.cnomor left join m1_coa coa2 on ri.rirekpajak1 = coa2.cnomor left join m1_coa coa3 on ri.rirekpajak2 = coa3.cnomor left join m1_coa coa4 on ri.rirekbiayalain = coa4.cnomor left join m1_coa coa5 on ri.rirekbayar = coa5.cnomor left join m4_po po on ri.riidpo = po.poid left join m4_ipc ipc on ri.riidipc = ipc.ipcid left join m4_grn grn on ri.riidgrn = grn.grnid left join m0_status st1 on st1.kode = ri.ristatus left join m0_status st2 on st2.kode = ri.ristatussebelumnya left join m0_user u1 on u1.userid = ri.riinputuser left join m0_user u2 on u2.userid = ri.rimodifikasiuser left join m1_item i on i.bid = rid.idbarang left join m1_tax t1 on rid.pajak1 = t1.tkode left join m1_tax t2 on rid.pajak2 = t2.tkode left join m1_branch brd on rid.cabang = brd.bkode left join m1_location lcd on rid.lokasi = lcd.lkode left join m1_warehouse whd on rid.gudang = whd.wkode left join m1_project p on rid.proyek = p.pkode left join m4_po_detail pod on rid.idpodetail = pod.idpodetail left join m4_po po2 on pod.idpo = po2.poid left join m4_ipc_detail ipcd on rid.idipcdetail = ipcd.idipcdetail left join m4_ipc ipc2 on ipcd.idipc = ipc2.ipcid left join m4_grn_detail grnd on rid.idgrndetail = grnd.idgrndetail left join m4_grn grn2 on grnd.idgrn = grn2.grnid left join m1_cost_center cc on rid.costcenter = cc.cckode left join m1_division d on rid.divisi = d.dkode left join m1_subdivision sd on rid.subdivisi = sd.sdkode left join m1_coa coa6 on ri.rirekuangmuka = coa6.cnomor left join m4_ap ap on ri.riidap = ap.apid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("riid"), 0), sptField,
                     FxDB(drutama("ricabang"), ""), sptField,
                     FxDB(drutama("rilokasi"), ""), sptField,
                     FxDB(drutama("rigudang"), ""), sptField,
                     FxDB(drutama("riasalbarang"), ""), sptField,
                     FxDB(drutama("riasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rijenispembelian"), ""), sptField,
                     FxDB(drutama("rijenispembeliankategori"), 0), sptField,
                     FxDB(drutama("ricarabayar"), 0), sptField,
                     FxDB(drutama("risumber"), ""), sptField,
                     FxDB(drutama("riautonotransaksi"), 0), sptField,
                     FxDB(drutama("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rikodepa"), 0), sptField,
                     FxDB(drutama("risupplier"), 0), sptField,
                     FxDB(drutama("risupplierkontak"), ""), sptField,
                     FxDB(drutama("ri1alamat1"), ""), sptField,
                     FxDB(drutama("ri1alamat2"), ""), sptField,
                     FxDB(drutama("ri1alamat3"), ""), sptField,
                     FxDB(drutama("ri2alamat1"), ""), sptField,
                     FxDB(drutama("ri2alamat2"), ""), sptField,
                     FxDB(drutama("ri2alamat3"), ""), sptField,
                     FxDB(drutama("ribagianpembelian"), 0), sptField,
                     FxDB(drutama("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("riuraian"), ""), sptField,
                     FxDB(drutama("ricatatan"), ""), sptField,
                     FxDB(drutama("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rimatauang"), ""), sptField,
                     FxDB(drutama("rikurs"), 0), sptField,
                     FxDB(drutama("rihargatermasukpajak"), 0), sptField,
                     FxDB(drutama("ritotal"), 0), sptField,
                     FxDB(drutama("ridiskonpersen"), ""), sptField,
                     FxDB(drutama("rijmldiskon"), 0), sptField,
                     FxDB(drutama("ritotalpajak1detail"), 0), sptField,
                     FxDB(drutama("ritotalpajak2detail"), 0), sptField,
                     FxDB(drutama("ribiayalainpersen"), ""), sptField,
                     FxDB(drutama("ribiayalain"), 0), sptField,
                     FxDB(drutama("ritotaltransaksi"), 0), sptField,
                     FxDB(drutama("rijmlbayar"), 0), sptField,
                     FxDB(drutama("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rinofakturpajak"), ""), sptField,
                     FxDB(drutama("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("rirekdiskon"), ""), sptField,
                     FxDB(drutama("rirekpajak1"), ""), sptField,
                     FxDB(drutama("rirekpajak2"), ""), sptField,
                     FxDB(drutama("rirekbiayalain"), ""), sptField,
                     FxDB(drutama("rirekbayar"), ""), sptField,
                     FxDB(drutama("riidpr"), 0), sptField,
                     FxDB(drutama("riidcs"), 0), sptField,
                     FxDB(drutama("riidrq"), 0), sptField,
                     FxDB(drutama("riidbs"), 0), sptField,
                     FxDB(drutama("riidpo"), 0), sptField,
                     FxDB(drutama("riidipc"), 0), sptField,
                     FxDB(drutama("riidgrn"), 0), sptField,
                     FxDB(drutama("ristatusdnr"), 0), sptField,
                     FxDB(drutama("ristatusprt"), 0), sptField,
                     FxDB(drutama("ristatusrealisasi"), 0), sptField,
                     FxDB(drutama("ristatus"), 0), sptField,
                     FxDB(drutama("ristatussebelumnya"), 0), sptField,
                     FxDB(drutama("rijmlrevisi"), 0), sptField,
                     FxDB(drutama("ricetakanke"), 0), sptField,
                     FxDB(drutama("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("ritutupperiode"), 0), sptField,
                     FxDB(drutama("riisclose"), 0), sptField,
                     FxDB(drutama("ricustomtext1"), ""), sptField,
                     FxDB(drutama("ricustomtext2"), ""), sptField,
                     FxDB(drutama("ricustomtext3"), ""), sptField,
                     FxDB(drutama("ricustomtext4"), ""), sptField,
                     FxDB(drutama("ricustomtext5"), ""), sptField,
                     FxDB(drutama("ricustomint1"), 0), sptField,
                     FxDB(drutama("ricustomint2"), 0), sptField,
                     FxDB(drutama("ricustomint3"), 0), sptField,
                     FxDB(drutama("ricustomdbl1"), 0), sptField,
                     FxDB(drutama("ricustomdbl2"), 0), sptField,
                     FxDB(drutama("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ricustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ricabangnama"), ""), sptField,
                     FxDB(drutama("rilokasinama"), ""), sptField,
                     FxDB(drutama("rigudangnama"), ""), sptField,
                     FxDB(drutama("risupplierkode"), ""), sptField,
                     FxDB(drutama("risuppliernama"), ""), sptField,
                     FxDB(drutama("ribagianpembeliankode"), ""), sptField,
                     FxDB(drutama("ribagianpembeliannama"), ""), sptField,
                     FxDB(drutama("riterminnama"), ""), sptField,
                     FxDB(drutama("riterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rirekdiskonnama"), ""), sptField,
                     FxDB(drutama("rirekpajak1nama"), ""), sptField,
                     FxDB(drutama("rirekpajak2nama"), ""), sptField,
                     FxDB(drutama("rirekbiayalainnama"), ""), sptField,
                     FxDB(drutama("rirekbayarnama"), ""), sptField,
                     FxDB(drutama("rinotransaksipo"), ""), sptField,
                     FxDB(drutama("rinotransaksiipc"), ""), sptField,
                     FxDB(drutama("rinotransaksigrn"), ""), sptField,
                     FxDB(drutama("ristatusnama"), ""), sptField,
                     FxDB(drutama("ristatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("riinputusernama"), ""), sptField,
                     FxDB(drutama("rimodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("rijmluangmuka"), 0), sptField,
                     FxDB(drutama("rirekuangmuka"), ""), sptField,
                     FxDB(drutama("riidap"), 0), sptField,
                     FxDB(drutama("rirekuangmukanama"), ""), sptField,
                     FxDB(drutama("apnotransaksi"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargafix"), 0), sptField,
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
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idipcdetail"), 0), sptField,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
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
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)


            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
            Dim dtbatch As New DataTable
            dtbatch = AmbilData("aplikasi1-m1_no_batch_out", "nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "' AND (nbtjenismutasi = 1 OR nbiidbarang IS NOT NULL)", "nbtidbarang, nbtkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtbatch.Rows
                batch = String.Concat(batch,
                     FxDB(dr("nbtid"), 0), sptField,
                     FxDB(dr("nbtjenismutasi"), 0), sptField,
                     FxDB(dr("nbtidbatchin"), 0), sptField,
                     FxDB(dr("nbtgudang"), ""), sptField,
                     FxDB(dr("nbtidbarang"), 0), sptField,
                     FxDB(dr("nbtkode"), ""), sptField,
                     FxDB(dr("nbtsumber"), ""), sptField,
                     FxDB(dr("nbtidtransaksi"), 0), sptField,
                     FxDB(dr("nbtsatuan"), ""), sptField,
                     FxDB(dr("nbtjml"), 0), sptField,
                     FxDB(dr("nbtcustomtext1"), ""), sptField,
                     FxDB(dr("nbtcustomtext2"), ""), sptField,
                     FxDB(dr("nbtcustomtext3"), ""), sptField,
                     FxDB(dr("nbtcustomdbl1"), 0), sptField,
                     FxDB(dr("nbtcustomdbl2"), 0), sptField,
                     FxDB(dr("nbtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nbtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang` from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
            Dim dtserial As New DataTable
            dtserial = AmbilData("aplikasi1-m1_no_serial_out", "nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "' AND (nstjenismutasi = 1 OR nsiidbarang IS NOT NULL)", "nstidbarang, nstkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtserial.Rows
                serial = String.Concat(serial,
                     FxDB(dr("nstid"), 0), sptField,
                     FxDB(dr("nstjenismutasi"), 0), sptField,
                     FxDB(dr("nstidserialin"), 0), sptField,
                     FxDB(dr("nstgudang"), ""), sptField,
                     FxDB(dr("nstidbarang"), 0), sptField,
                     FxDB(dr("nstkode"), ""), sptField,
                     FxDB(dr("nstsumber"), ""), sptField,
                     FxDB(dr("nstidtransaksi"), 0), sptField,
                     FxDB(dr("nstsatuan"), ""), sptField,
                     FxDB(dr("nstjml"), 0), sptField,
                     FxDB(dr("nstcustomtext1"), ""), sptField,
                     FxDB(dr("nstcustomtext2"), ""), sptField,
                     FxDB(dr("nstcustomtext3"), ""), sptField,
                     FxDB(dr("nstcustomdbl1"), 0), sptField,
                     FxDB(dr("nstcustomdbl2"), 0), sptField,
                     FxDB(dr("nstcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("nstcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial


            'AMBIL DATA COST
            sql = "SELECT rc.idricost, rc.idri, rc.kodecost, rc.matauang, rc.kurs, rc.jumlah, rc.rekdebit, rc.rekkredit, rc.catatan, rc.costcenter, rc.divisi, rc.subdivisi, rc.proyek, rc.urutan, rc.idprcost, rc.idcscost, rc.idrqcost, rc.idbscost, rc.idpocost, rc.idipccost, rc.idgrncost, rc.jumlahbayar, rc.statusbayar, rc.isclose, rc.customtext1, rc.customtext2, rc.customtext3, rc.customdbl1, rc.customdbl2, rc.customdbl3, rc.customdate1, rc.customdate2, rc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sdnama as subdivisinama, p.pnama as proyeknama, rc.kontak, c.kkode as kontakkode, c.knama as kontaknama, rc.termasukhpp FROM m4_ri_cost rc JOIN m4_ri ri ON rc.idri = ri.riid LEFT JOIN m1_other_cost oc ON rc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON rc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON rc.rekkredit = coa2.cnomor LEFT JOIN m1_cost_center cc ON rc.costcenter = cc.cckode LEFT JOIN m1_division d ON rc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON rc.subdivisi = sd.sdkode LEFT JOIN m1_project p ON rc.proyek = p.pkode LEFT JOIN m1_contact c ON rc.kontak = c.kid"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_ri_cost", Filter, "rc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idricost"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("kodecost"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("jumlah"), 0), sptField,
                     FxDB(dr("rekdebit"), ""), sptField,
                     FxDB(dr("rekkredit"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprcost"), 0), sptField,
                     FxDB(dr("idcscost"), 0), sptField,
                     FxDB(dr("idrqcost"), 0), sptField,
                     FxDB(dr("idbscost"), 0), sptField,
                     FxDB(dr("idpocost"), 0), sptField,
                     FxDB(dr("idipccost"), 0), sptField,
                     FxDB(dr("idgrncost"), 0), sptField,
                     FxDB(dr("jumlahbayar"), 0), sptField,
                     FxDB(dr("statusbayar"), 0), sptField,
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
                     FxDB(dr("kodecostnama"), ""), sptField,
                     FxDB(dr("rekdebitnama"), ""), sptField,
                     FxDB(dr("rekkreditnama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("termasukhpp"), ""), sptRow)
            Next
            If cost.Length > 0 Then cost = cost.Substring(0, cost.Length - sptRow.Length) Else cost = cost


            'AMBIL DATA PAY
            'sql = "SELECT rip.idricarabayar AS idricarabayar, rip.idri AS idri, rip.carabayar AS carabayar, rip.matauang AS matauang, rip.kurs AS kurs, rip.jumlah AS jumlah, rip.jumlahvalas AS jumlahvalas, rip.nogiro AS nogiro, rip.tgljt AS tgljt, rip.bank AS bank, rip.noacbank AS noacbank, rip.rekbank AS rekbank, rip.rekgiro AS rekgiro, rip.catatan AS catatan, rip.urutan AS urutan, rip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama, rip.sumber, rip.idtransaksi, rip.totaltransaksi, rip.terbayar, notransaksi, tgl FROM M4_ri_pay AS rip LEFT JOIN m0_payment_method AS pm ON rip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON rip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON rip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON rip.rekgiro = coa2.cnomor"
            sql = "SELECT rip.idricarabayar AS idricarabayar, rip.idri AS idri, rip.carabayar AS carabayar, rip.matauang AS matauang, rip.kurs AS kurs, rip.jumlah AS jumlah, rip.jumlahvalas AS jumlahvalas, rip.nogiro AS nogiro, rip.tgljt AS tgljt, rip.bank AS bank, rip.noacbank AS noacbank, rip.rekbank AS rekbank, rip.rekgiro AS rekgiro, rip.catatan AS catatan, rip.urutan AS urutan, rip.isclose AS isclose, pm.nama AS carabayarnama, b.bnama AS banknama, coa1.cnama AS rekbanknama, coa2.cnama AS rekgironama, rip.sumber, rip.idtransaksi, rip.totaltransaksi, rip.terbayar, ap.apnotransaksi as notransaksi, IFNULL(ap.aptgl,rip.tgljt) as tgl FROM M4_ri_pay AS rip LEFT JOIN m0_payment_method AS pm ON rip.carabayar = pm.kode LEFT JOIN m1_bank AS b ON rip.bank = b.bkode LEFT JOIN m1_coa AS coa1 ON rip.rekbank = coa1.cnomor LEFT JOIN m1_coa AS coa2 ON rip.rekgiro = coa2.cnomor LEFT JOIN m4_ap ap ON rip.sumber = ap.apsumber AND rip.idtransaksi = ap.apid"
            Dim dtpay As New DataTable
            dtpay = AmbilData("aplikasi1-M4_ri_Pay", "idri=" & idtransaksi, "idri ASC, urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtpay.Rows
                pay = String.Concat(pay,
                     FxDB(dr("idricarabayar"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
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
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("totaltransaksi"), 0), sptField,
                     FxDB(dr("terbayar"), 0), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptRow)
            Next
            If pay.Length > 0 Then pay = pay.Substring(0, pay.Length - sptRow.Length) Else pay = pay

            'AMBIL DATA ASSET
            'sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama, i.bkode as kodebarang from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode JOIN m1_item i ON i.bid = atr.atidbarang"
            Dim dtasset As New DataTable
            dtasset = AmbilData("aplikasi1-asset", "atidutama = '" & idtransaksi & "' AND atsumber = '" & sumber & "'", "atidbarang, atkode ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtasset.Rows
                asset = String.Concat(asset,
                     FxDB(dr("atid"), ""), sptField,
                     FxDB(dr("atasetid"), ""), sptField,
                     FxDB(dr("atjenismutasi"), 0), sptField,
                     FxDB(dr("atsumber"), ""), sptField,
                     FxDB(dr("atidutama"), ""), sptField,
                     FxDB(dr("atidbarang"), ""), sptField,
                     FxDB(dr("atkode"), ""), sptField,
                     FxDB(dr("atnama"), ""), sptField,
                     FxDB(dr("atkategori"), ""), sptField,
                     FxDB(dr("atcabang"), ""), sptField,
                     FxDB(dr("atlokasi"), ""), sptField,
                     FxDB(dr("atgudang"), ""), sptField,
                     FxDB(dr("atdivisi"), ""), sptField,
                     FxDB(dr("atsubdivisi"), ""), sptField,
                     FxDB(dr("atcostcenter"), ""), sptField,
                     FxDB(dr("atproyek"), ""), sptField,
                     FxDB(dr("atcatatan"), ""), sptField,
                     FxDB(dr("atnomor"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglbeli"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("attglpakai"), ""), formatTgl), sptField,
                     FxDB(dr("atjml"), 0), sptField,
                     FxDB(dr("atsatuan"), ""), sptField,
                     FxDB(dr("atmatauang"), ""), sptField,
                     FxDB(dr("atkurs"), 0), sptField,
                     FxDB(dr("atharga"), 0), sptField,
                     FxDB(dr("atdiskon"), ""), sptField,
                     FxDB(dr("atjmldiskon"), 0), sptField,
                     FxDB(dr("atpajak1"), ""), sptField,
                     FxDB(dr("atjmlpajak1"), 0), sptField,
                     FxDB(dr("atpajak2"), ""), sptField,
                     FxDB(dr("atjmlpajak2"), 0), sptField,
                     FxDB(dr("athargabeli"), 0), sptField,
                     FxDB(dr("atnilairesidu"), 0), sptField,
                     FxDB(dr("atumurekonomis"), 0), sptField,
                     FxDB(dr("atbebanperbln"), 0), sptField,
                     FxDB(dr("atakumulasibeban"), 0), sptField,
                     FxDB(dr("atnilaibuku"), 0), sptField,
                     FxDB(dr("atnilaipenyusutan"), 0), sptField,
                     FxDB(dr("atmetode"), 0), sptField,
                     FxDB(dr("attabelpenyusutan"), ""), sptField,
                     FxDB(dr("atintangible"), 0), sptField,
                     FxDB(dr("atfiskal"), 0), sptField,
                     FxDB(dr("atatastengahbulan"), 0), sptField,
                     FxDB(dr("atrekasset"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasi"), ""), sptField,
                     FxDB(dr("atrekdepresiasi"), ""), sptField,
                     FxDB(dr("atrekpenghapusan"), ""), sptField,
                     FxDB(dr("atprodusen"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("attglpensiun"), ""), formatTgl), sptField,
                     FxDB(dr("atpenyusutanke"), 0), sptField,
                     FxDB(dr("atnilaimenurun"), 0), sptField,
                     FxDB(dr("atdispose"), 0), sptField,
                     FxDB(dr("atpembelian"), 0), sptField,
                     FxDB(dr("atpenjualan"), 0), sptField,
                     FxDB(dr("atlocked"), 0), sptField,
                     FxDB(dr("atstatus"), 0), sptField,
                     FxDB(dr("atstatussebelumnya"), 0), sptField,
                     FxDB(dr("atisclose"), 0), sptField,
                     FxDB(dr("atinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("atmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("atcustomtext1"), ""), sptField,
                     FxDB(dr("atcustomtext2"), ""), sptField,
                     FxDB(dr("atcustomtext3"), ""), sptField,
                     FxDB(dr("atcustomtext4"), ""), sptField,
                     FxDB(dr("atcustomtext5"), ""), sptField,
                     FxDB(dr("atcustomint1"), 0), sptField,
                     FxDB(dr("atcustomint2"), 0), sptField,
                     FxDB(dr("atcustomint3"), 0), sptField,
                     FxDB(dr("atcustomint4"), 0), sptField,
                     FxDB(dr("atcustomint5"), 0), sptField,
                     FxDB(dr("atcustomdbl1"), 0), sptField,
                     FxDB(dr("atcustomdbl2"), 0), sptField,
                     FxDB(dr("atcustomdbl3"), 0), sptField,
                     FxDB(dr("atcustomdbl4"), 0), sptField,
                     FxDB(dr("atcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("atcustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("atkategorinama"), ""), sptField,
                     FxDB(dr("atcabangnama"), ""), sptField,
                     FxDB(dr("atlokasinama"), ""), sptField,
                     FxDB(dr("atgudangnama"), ""), sptField,
                     FxDB(dr("atdivisinama"), ""), sptField,
                     FxDB(dr("atsubdivisinama"), ""), sptField,
                     FxDB(dr("atcostcenternama"), ""), sptField,
                     FxDB(dr("atproyeknama"), ""), sptField,
                     FxDB(dr("atmetodenama"), ""), sptField,
                     FxDB(dr("atpajak1nama"), ""), sptField,
                     FxDB(dr("atpajak1nilai"), 0), sptField,
                     FxDB(dr("atpajak2nama"), ""), sptField,
                     FxDB(dr("atpajak2nilai"), 0), sptField,
                     FxDB(dr("atrekassetnama"), ""), sptField,
                     FxDB(dr("atrekakumdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekdepresiasinama"), ""), sptField,
                     FxDB(dr("atrekpenghapusannama"), ""), sptField,
                     FxDB(dr("atprodusenkode"), ""), sptField,
                     FxDB(dr("atprodusennama"), ""), sptField,
                     FxDB(dr("atstatusnama"), ""), sptField,
                     FxDB(dr("atstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("atinputusernama"), ""), sptField,
                     FxDB(dr("atmodifikasiusernama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset


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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, cost, sptSubParam, pay, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, riterminnama, riterminharijatuhtempo, rirekdiskonnama, rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama, rirekbayarnama, rinotransaksipo, rinotransaksiipc, rinotransaksigrn, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, kpkp, rijmluangmuka, rirekuangmuka, riidap, rirekuangmukanama, apnotransaksi" & sptSubParam & "idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, ponotransaksi, ipcnotransaksi, grnnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "idricost, idri, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp" & sptSubParam & "idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose, carabayarnama, banknama, rekbanknama, rekgironama, sumber, idtransaksi, totaltransaksi, terbayar, notransaksi, tgl" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama, kodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RiSearch(ByVal param As String) As String
        'M4_RiSearch --------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricarabayarnama

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_ri_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ri", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("ricabang"), ""), sptField,
                     FxDB(dr("rilokasi"), ""), sptField,
                     FxDB(dr("rigudang"), ""), sptField,
                     FxDB(dr("riasalbarang"), ""), sptField,
                     FxDB(dr("riasalbarangkategori"), 0), sptField,
                     FxDB(dr("rijenispembelian"), ""), sptField,
                     FxDB(dr("rijenispembeliankategori"), 0), sptField,
                     FxDB(dr("ricarabayar"), 0), sptField,
                     FxDB(dr("risumber"), ""), sptField,
                     FxDB(dr("riautonotransaksi"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     FxDB(dr("rikodepa"), 0), sptField,
                     FxDB(dr("risupplier"), 0), sptField,
                     FxDB(dr("risupplierkontak"), ""), sptField,
                     FxDB(dr("ri1alamat1"), ""), sptField,
                     FxDB(dr("ri1alamat2"), ""), sptField,
                     FxDB(dr("ri1alamat3"), ""), sptField,
                     FxDB(dr("ri2alamat1"), ""), sptField,
                     FxDB(dr("ri2alamat2"), ""), sptField,
                     FxDB(dr("ri2alamat3"), ""), sptField,
                     FxDB(dr("ribagianpembelian"), 0), sptField,
                     FxDB(dr("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("riuraian"), ""), sptField,
                     FxDB(dr("ricatatan"), ""), sptField,
                     FxDB(dr("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rimatauang"), ""), sptField,
                     FxDB(dr("rikurs"), 0), sptField,
                     FxDB(dr("rihargatermasukpajak"), 0), sptField,
                     FxDB(dr("ritotal"), 0), sptField,
                     FxDB(dr("ridiskonpersen"), ""), sptField,
                     FxDB(dr("rijmldiskon"), 0), sptField,
                     FxDB(dr("ritotalpajak1detail"), 0), sptField,
                     FxDB(dr("ritotalpajak2detail"), 0), sptField,
                     FxDB(dr("ribiayalainpersen"), ""), sptField,
                     FxDB(dr("ribiayalain"), 0), sptField,
                     FxDB(dr("ritotaltransaksi"), 0), sptField,
                     FxDB(dr("rijmlbayar"), 0), sptField,
                     FxDB(dr("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rinofakturpajak"), ""), sptField,
                     FxDB(dr("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rirekdiskon"), ""), sptField,
                     FxDB(dr("rirekpajak1"), ""), sptField,
                     FxDB(dr("rirekpajak2"), ""), sptField,
                     FxDB(dr("rirekbiayalain"), ""), sptField,
                     FxDB(dr("rirekbayar"), ""), sptField,
                     FxDB(dr("riidpr"), 0), sptField,
                     FxDB(dr("riidcs"), 0), sptField,
                     FxDB(dr("riidrq"), 0), sptField,
                     FxDB(dr("riidbs"), 0), sptField,
                     FxDB(dr("riidpo"), 0), sptField,
                     FxDB(dr("riidipc"), 0), sptField,
                     FxDB(dr("riidgrn"), 0), sptField,
                     FxDB(dr("ristatusdnr"), 0), sptField,
                     FxDB(dr("ristatusprt"), 0), sptField,
                     FxDB(dr("ristatusrealisasi"), 0), sptField,
                     FxDB(dr("ristatus"), 0), sptField,
                     FxDB(dr("ristatussebelumnya"), 0), sptField,
                     FxDB(dr("rijmlrevisi"), 0), sptField,
                     FxDB(dr("ricetakanke"), 0), sptField,
                     FxDB(dr("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ritutupperiode"), 0), sptField,
                     FxDB(dr("riisclose"), 0), sptField,
                     FxDB(dr("ricabangnama"), ""), sptField,
                     FxDB(dr("rilokasinama"), ""), sptField,
                     FxDB(dr("rigudangnama"), ""), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("ribagianpembeliankode"), ""), sptField,
                     FxDB(dr("ribagianpembeliannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("ristatusnama"), ""), sptField,
                     FxDB(dr("ristatussebelumnyanama"), ""), sptField,
                     FxDB(dr("riinputusernama"), ""), sptField,
                     FxDB(dr("rimodifikasiusernama"), ""), sptField,
                     FxDB(dr("ricarabayarnama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricarabayarnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RiTerkait(ByVal param As String) As String
        'M4_RiTerkait --------------------------------------------------------
        'riid, rinotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "riid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ri_terkait")
		sql = "select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`pr`.`prsumber` AS `sumber`,`pr`.`prid` AS `idterkait`,`pr`.`prnotransaksi` AS `noterkait`,`pr`.`prtgl` AS `tglterkait`,`pr`.`prinputtgl` AS `inputtglterkait`,`pr`.`prmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_pr_detail` `prd` join `m4_pr` `pr` on((`prd`.`idpr` = `pr`.`prid`))) join `m4_ri_detail` `rid` on((`prd`.`idprdetail` = `rid`.`idprdetail`))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) where (`ri`.`riid` = 'validtransaksi') group by `pr`.`prid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`rq`.`rqsumber` AS `sumber`,`rq`.`rqid` AS `idterkait`,`rq`.`rqnotransaksi` AS `noterkait`,`rq`.`rqtgl` AS `tglterkait`,`rq`.`rqinputtgl` AS `inputtglterkait`,`rq`.`rqmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_rq_detail` `rqd` join `m4_rq` `rq` on((`rqd`.`idrq` = `rq`.`rqid`))) join `m4_ri_detail` `rid` on((`rqd`.`idrqdetail` = `rid`.`idrqdetail`))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) where (`ri`.`riid` = 'validtransaksi') group by `rq`.`rqid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`po`.`posumber` AS `sumber`,`po`.`poid` AS `idterkait`,`po`.`ponotransaksi` AS `noterkait`,`po`.`potgl` AS `tglterkait`,`po`.`poinputtgl` AS `inputtglterkait`,`po`.`pomodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) join `m4_ri_detail` `rid` on((`pod`.`idpodetail` = `rid`.`idpodetail`))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) where (`ri`.`riid` = 'validtransaksi') group by `po`.`poid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`grn`.`grnsumber` AS `sumber`,`grn`.`grnid` AS `idterkait`,`grn`.`grnnotransaksi` AS `noterkait`,`grn`.`grntgl` AS `tglterkait`,`grn`.`grninputtgl` AS `inputtglterkait`,`grn`.`grnmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_grn_detail` `grnd` join `m4_grn` `grn` on((`grnd`.`idgrn` = `grn`.`grnid`))) join `m4_ri_detail` `rid` on((`grnd`.`idgrndetail` = `rid`.`idgrndetail`))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) where (`ri`.`riid` = 'validtransaksi') group by `grn`.`grnid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`dnr`.`dnrsumber` AS `sumber`,`dnr`.`dnrid` AS `idterkait`,`dnr`.`dnrnotransaksi` AS `noterkait`,`dnr`.`dnrtgl` AS `tglterkait`,`dnr`.`dnrinputtgl` AS `inputtglterkait`,`dnr`.`dnrmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_dnr_detail` `dnrd` join `m4_dnr` `dnr` on((`dnrd`.`iddnr` = `dnr`.`dnrid`))) join `m4_ri_detail` `rid` on((`rid`.`idridetail` = `dnrd`.`idridetail`))) join `m4_ri` `ri` on((`ri`.`riid` = `rid`.`idri`))) where (((`dnr`.`dnrstatus` = 2) or (`dnr`.`dnrstatus` = 3) or (`dnr`.`dnrstatus` = 4) or (`dnr`.`dnrstatus` = 7)) and (`ri`.`riid` = 'validtransaksi')) group by `dnr`.`dnrid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtid` AS `idterkait`,`prt`.`prtnotransaksi` AS `noterkait`,`prt`.`prttgl` AS `tglterkait`,`prt`.`prtinputtgl` AS `inputtglterkait`,`prt`.`prtmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (((`m4_prt_detail` `prtd` join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) join `m4_ri_detail` `rid` on((`rid`.`idridetail` = `prtd`.`idridetail`))) join `m4_ri` `ri` on((`ri`.`riid` = `rid`.`idri`))) where (((`prt`.`prtstatus` = 2) or (`prt`.`prtstatus` = 3) or (`prt`.`prtstatus` = 4) or (`prt`.`prtstatus` = 7)) and (`ri`.`riid` = 'validtransaksi')) group by `prt`.`prtid`,`ri`.`riid`  union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`prt`.`prtsumber` AS `sumber`,`prt`.`prtid` AS `idterkait`, `prt`.`prtnotransaksi` AS `noterkait`,`prt`.`prttgl` AS `tglterkait`,`prt`.`prtinputtgl` AS `inputtglterkait`, `prt`.`prtmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from (`m4_prt` `prt` join `m4_ri` `ri` on((`prt`.`prtidri` = `ri`.`riid`))) where (((`prt`.`prtstatus` = 2) or (`prt`.`prtstatus` = 3) or (`prt`.`prtstatus` = 4) or (`prt`.`prtstatus` = 7)) and (`ri`.`riid` = 'validtransaksi')) group by `prt`.`prtid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`vpp`.`vppsumber` AS `sumber`,`vpp`.`vppid` AS `idterkait`,`vpp`.`vppnotransaksi` AS `noterkait`,`vpp`.`vpptgl` AS `tglterkait`,`vpp`.`vppinputtgl` AS `inputtglterkait`,`vpp`.`vppmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vpp_detail` `vppd` join `m4_vpp` `vpp` on((`vppd`.`idvpp` = `vpp`.`vppid`))) join `m4_ri` `ri` on((`vppd`.`idtransaksi` = `ri`.`riid`))) where ((`vppd`.`sumber` = 'RI') and ((`vpp`.`vppstatus` = 2) or (`vpp`.`vppstatus` = 3) or (`vpp`.`vppstatus` = 4) or (`vpp`.`vppstatus` = 7)) and (`ri`.`riid` = 'validtransaksi')) group by `vpp`.`vppid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`vp`.`vpsumber` AS `sumber`,`vp`.`vpid` AS `idterkait`,`vp`.`vpnotransaksi` AS `noterkait`,`vp`.`vptgl` AS `tglterkait`,`vp`.`vpinputtgl` AS `inputtglterkait`,`vp`.`vpmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vp_detail` `vpd` join `m4_vp` `vp` on((`vpd`.`idvp` = `vp`.`vpid`))) join `m4_ri` `ri` on((`vpd`.`idtransaksi` = `ri`.`riid`))) where ((`vpd`.`sumber` = 'RI') and ((`vp`.`vpstatus` = 2) or (`vp`.`vpstatus` = 3) or (`vp`.`vpstatus` = 4) or (`vp`.`vpstatus` = 7)) and (`ri`.`riid` = 'validtransaksi')) group by `vp`.`vpid`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`cfo`.`cfosumber` AS `sumber`,`it`.`idutama` AS `idterkait`,`it`.`notransaksi` AS `noterkait`,`it`.`tgl` AS `tglterkait`,`it`.`inputtgl` AS `inputtglterkait`,`it`.`inputtgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((((`m1_cogs_fifo_in` `cfi` join `m1_cogs_fifo_out` `cfo` on((`cfi`.`cfiid` = `cfo`.`cfoidcfi`))) join `m4_ri_detail` `rid` on(((`cfi`.`cfisumber` = 'RI') and (`cfi`.`cfiidbarang` = `rid`.`idbarang`) and (`cfi`.`cfiidtransaksi` = `rid`.`idridetail`)))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) join `m1_item_transaction` `it` on(((`cfo`.`cfosumber` = `it`.`sumber`) and (`cfo`.`cfoidbarang` = `it`.`idbarang`) and (`cfo`.`cfoidtransaksi` = `it`.`iddetail`)))) where (`ri`.`riid` = 'validtransaksi') group by `it`.`sumber`,`it`.`idutama`,`ri`.`riid` union all select `ri`.`riid` AS `riid`,`ri`.`rinotransaksi` AS `rinotransaksi`,`cso`.`sumber` AS `sumber`,`it`.`idutama` AS `idterkait`,`it`.`notransaksi` AS `noterkait`,`it`.`tgl` AS `tglterkait`,`it`.`inputtgl` AS `inputtglterkait`,`it`.`inputtgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((((`m1_cogs_special_in` `csi` join `m1_cogs_special_out` `cso` on((`csi`.`idhppikm` = `cso`.`idhppikm`))) join `m4_ri_detail` `rid` on(((`csi`.`sumber` = 'RI') and (`csi`.`idbarang` = `rid`.`idbarang`) and (`csi`.`idtransaksi` = `rid`.`idridetail`)))) join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) join `m1_item_transaction` `it` on(((`cso`.`sumber` = `it`.`sumber`) and (`cso`.`idbarang` = `it`.`idbarang`) and (`cso`.`idtransaksi` = `it`.`iddetail`)))) where (`ri`.`riid` = 'validtransaksi') group by `it`.`sumber`,`it`.`idutama`,`ri`.`riid` union SELECT ri.riid as riid, ri.rinotransaksi as rinotransaksi, da.dasumber as sumber, da.daid as idterkait, da.danotransaksi as noterkait, da.datgl as tglterkait, da.dainputtgl as inputtglterkait, da.damodifikasitgl as modifikasitglterkait, 1 as jenisterkait FROM m7_asset_transaction atr JOIN m4_ri ri ON atr.atsumber = ri.risumber AND atr.atidutama = ri.riid AND ri.riid = 'validtransaksi' JOIN m7_asset a ON atr.atkode = a.akode JOIN m7_da_detail dad ON a.aid = dad.idaset JOIN m7_da da ON dad.idda = da.daid AND da.dastatus IN(2,3,4,7) GROUP BY da.daid, ri.riid union SELECT atr.atidutama AS riid, atr.atnotransaksi AS rinotransaksi, atr2.atsumber AS sumber, atr2.atidutama AS idterkait, atr2.atnotransaksi AS noterkait, atr2.attgl AS tglterkait, CONCAT(atr2.attgl,'00:00:00') AS inputtglterkait, CONCAT(atr2.attgl,'00:00:00') AS modifikasitglterkait, 1 AS jenisterkait FROM m7_asset_transaction atr JOIN m7_asset_transaction atr2 ON atr.atkode = atr2.atkode AND atr2.atstatus IN(2,3,4,7) AND atr.atsumber = 'RI' AND atr.atidutama = 'validtransaksi' AND NOT (atr.atsumber = atr2.atsumber AND atr.atidutama = atr2.atidutama) GROUP BY atr2.atsumber, atr2.atidutama"
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
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
            result(2) = "Related RI data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riid, rinotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Ri_Detail_VSearch(ByVal param As String) As String
        'M4_Ri_Detail_VSearch --------------------------------------------------------
        'idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, 
        'rekhargapokok, rekreturpembelian, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, 
        'statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rinotransaksi, 
        'riuraian, ricatatan, rinoref, ritgl, ritglnoref, rinofakturpajak, risupplierkontak, ri1alamat1, 
        'ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ritermin, riterminnama, 
        'riterminharijatuhtempo, ribagianpembelian, ribagianpembeliankode, ribagianpembeliannama, kodebarang, bhpp, bhppaverage, 
        'bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'idgrn, idhppfifomasuk, hppfifo, idhppkhususmasuk, hppkhusus, jmlsisadnr, jmlsisaprt, 
        'jmlsisarealisasi, risupplier, risupplierkode, risuppliernama, ridiskonpersen, ribiayalainpersen, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, ricustomtext1, ricustomtext2,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

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
            Filter = Filter.Replace("idbarang", "rid.idbarang")
            Filter = Filter.Replace("statusrealisasi", "rid.statusrealisasi")
            Filter = Filter.Replace("isclose", "rid.isclose")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_ri_detail_v")
        'sql = "select `rid`.`idridetail` AS `idridetail`,`rid`.`idri` AS `idri`,`rid`.`idbarang` AS `idbarang`,`rid`.`namabarang` AS `namabarang`,`rid`.`tipebarang` AS `tipebarang`,`rid`.`jml` AS `jml`,`rid`.`satuan` AS `satuan`,`rid`.`nilaisatuan` AS `nilaisatuan`,`rid`.`jmlbarang` AS `jmlbarang`,`rid`.`satuanbarang` AS `satuanbarang`,`rid`.`matauang` AS `matauang`,`rid`.`kurs` AS `kurs`,`rid`.`hargafix` AS `hargafix`,`rid`.`harga` AS `harga`,`rid`.`diskon` AS `diskon`,`rid`.`jmldiskon` AS `jmldiskon`,`rid`.`pajak1` AS `pajak1`,`rid`.`jmlpajak1` AS `jmlpajak1`,`rid`.`pajak2` AS `pajak2`,`rid`.`jmlpajak2` AS `jmlpajak2`,`rid`.`cabang` AS `cabang`,`rid`.`lokasi` AS `lokasi`,`rid`.`gudang` AS `gudang`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekdiskonpembelian` AS `rekdiskonpembelian`,`rid`.`rekhutangsementara` AS `rekhutangsementara`,`rid`.`costcenter` AS `costcenter`,`rid`.`divisi` AS `divisi`,`rid`.`subdivisi` AS `subdivisi`,`rid`.`proyek` AS `proyek`,`rid`.`catatan` AS `catatan`,`rid`.`urutan` AS `urutan`,`rid`.`idprdetail` AS `idprdetail`,`rid`.`idcsdetail` AS `idcsdetail`,`rid`.`idrqdetail` AS `idrqdetail`,`rid`.`idbsdetail` AS `idbsdetail`,`rid`.`idpodetail` AS `idpodetail`,`rid`.`idipcdetail` AS `idipcdetail`,`rid`.`idgrndetail` AS `idgrndetail`,`rid`.`jmldnr` AS `jmldnr`,`rid`.`statusdnr` AS `statusdnr`,`rid`.`jmlprt` AS `jmlprt`,`rid`.`statusprt` AS `statusprt`,`rid`.`jmlrealisasi` AS `jmlrealisasi`,`rid`.`statusrealisasi` AS `statusrealisasi`,`rid`.`isclose` AS `isclose`,`rid`.`customtext1` AS `customtext1`,`rid`.`customtext2` AS `customtext2`,`rid`.`customtext3` AS `customtext3`,`rid`.`customdbl1` AS `customdbl1`,`rid`.`customdbl2` AS `customdbl2`,`rid`.`customdbl3` AS `customdbl3`,`rid`.`customdate1` AS `customdate1`,`rid`.`customdate2` AS `customdate2`,`rid`.`customdate3` AS `customdate3`,`ri`.`rinotransaksi` AS `rinotransaksi`,`ri`.`riuraian` AS `riuraian`,`ri`.`ricatatan` AS `ricatatan`,`ri`.`rinoref` AS `rinoref`,`ri`.`ritgl` AS `ritgl`,`ri`.`ritglnoref` AS `ritglnoref`,`ri`.`rinofakturpajak` AS `rinofakturpajak`,`ri`.`risupplierkontak` AS `risupplierkontak`,`ri`.`ri1alamat1` AS `ri1alamat1`,`ri`.`ri1alamat2` AS `ri1alamat2`,`ri`.`ri1alamat3` AS `ri1alamat3`,`ri`.`ri2alamat1` AS `ri2alamat1`,`ri`.`ri2alamat2` AS `ri2alamat2`,`ri`.`ri2alamat3` AS `ri2alamat3`,`ri`.`ritermin` AS `ritermin`,`tr`.`trnama` AS `riterminnama`,`tr`.`trharijatuhtempo` AS `riterminharijatuhtempo`,`ri`.`ribagianpembelian` AS `ribagianpembelian`,`c1`.`kkode` AS `ribagianpembeliankode`,`c1`.`knama` AS `ribagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekreturpembelian` AS `rekreturpembelian`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`grnd`.`idgrn` AS `idgrn`,`cf`.`cfiid` AS `idhppfifomasuk`,`cf`.`cfiharga` AS `hppfifo`,`cs`.`idhppikm` AS `idhppkhususmasuk`,`cs`.`harga` AS `hppkhusus`,((`rid`.`jmlbarang` - `rid`.`jmldnr`) / `rid`.`nilaisatuan`) AS `jmlsisadnr`,((`rid`.`jmlbarang` - `rid`.`jmlprt`) / `rid`.`nilaisatuan`) AS `jmlsisaprt`,((`rid`.`jmlbarang` - `rid`.`jmlrealisasi`) / `rid`.`nilaisatuan`) AS `jmlsisarealisasi`,`ri`.`risupplier` AS `risupplier`,`c`.`kkode` AS `risupplierkode`,`c`.`knama` AS `risuppliernama`, ri.ridiskonpersen, ri.ribiayalainpersen, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset from ((((((((((`m4_ri_detail` `rid` left join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) left join `m1_terms` `tr` on((`ri`.`ritermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`ri`.`ribagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`rid`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`rid`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`rid`.`pajak2` = `t2`.`tkode`))) left join `m1_cogs_fifo_in` `cf` on(((`rid`.`idgrndetail` = `cf`.`cfiidtransaksi`) and (`cf`.`cfisumber` = 'GRN')))) left join `m1_cogs_special_in` `cs` on(((`rid`.`idgrndetail` = `cs`.`idtransaksi`) and (`cs`.`sumber` = 'GRN')))) left join `m4_grn_detail` `grnd` on((`rid`.`idgrndetail` = `grnd`.`idgrndetail`))) left join `m1_contact` `c` on((`ri`.`risupplier` = `c`.`kid`)))"
        sql = "select `rid`.`idridetail` AS `idridetail`,`rid`.`idri` AS `idri`,`rid`.`idbarang` AS `idbarang`,`rid`.`namabarang` AS `namabarang`,`rid`.`tipebarang` AS `tipebarang`,`rid`.`jml` AS `jml`,`rid`.`satuan` AS `satuan`,`rid`.`nilaisatuan` AS `nilaisatuan`,`rid`.`jmlbarang` AS `jmlbarang`,`rid`.`satuanbarang` AS `satuanbarang`,`rid`.`matauang` AS `matauang`,`rid`.`kurs` AS `kurs`,`rid`.`hargafix` AS `hargafix`,`rid`.`harga` AS `harga`,`rid`.`diskon` AS `diskon`,`rid`.`jmldiskon` AS `jmldiskon`,`rid`.`pajak1` AS `pajak1`,`rid`.`jmlpajak1` AS `jmlpajak1`,`rid`.`pajak2` AS `pajak2`,`rid`.`jmlpajak2` AS `jmlpajak2`,`rid`.`cabang` AS `cabang`,`rid`.`lokasi` AS `lokasi`,`rid`.`gudang` AS `gudang`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekdiskonpembelian` AS `rekdiskonpembelian`,`rid`.`rekhutangsementara` AS `rekhutangsementara`,`rid`.`costcenter` AS `costcenter`,`rid`.`divisi` AS `divisi`,`rid`.`subdivisi` AS `subdivisi`,`rid`.`proyek` AS `proyek`,`rid`.`catatan` AS `catatan`,`rid`.`urutan` AS `urutan`,`rid`.`idprdetail` AS `idprdetail`,`rid`.`idcsdetail` AS `idcsdetail`,`rid`.`idrqdetail` AS `idrqdetail`,`rid`.`idbsdetail` AS `idbsdetail`,`rid`.`idpodetail` AS `idpodetail`,`rid`.`idipcdetail` AS `idipcdetail`,`rid`.`idgrndetail` AS `idgrndetail`,`rid`.`jmldnr` AS `jmldnr`,`rid`.`statusdnr` AS `statusdnr`,`rid`.`jmlprt` AS `jmlprt`,`rid`.`statusprt` AS `statusprt`,`rid`.`jmlrealisasi` AS `jmlrealisasi`,`rid`.`statusrealisasi` AS `statusrealisasi`,`rid`.`isclose` AS `isclose`,`rid`.`customtext1` AS `customtext1`,`rid`.`customtext2` AS `customtext2`,`rid`.`customtext3` AS `customtext3`,`rid`.`customdbl1` AS `customdbl1`,`rid`.`customdbl2` AS `customdbl2`,`rid`.`customdbl3` AS `customdbl3`,`rid`.`customdate1` AS `customdate1`,`rid`.`customdate2` AS `customdate2`,`rid`.`customdate3` AS `customdate3`,`ri`.`rinotransaksi` AS `rinotransaksi`,`ri`.`riuraian` AS `riuraian`,`ri`.`ricatatan` AS `ricatatan`,`ri`.`rinoref` AS `rinoref`,`ri`.`ritgl` AS `ritgl`,`ri`.`ritglnoref` AS `ritglnoref`,`ri`.`rinofakturpajak` AS `rinofakturpajak`,`ri`.`risupplierkontak` AS `risupplierkontak`,`ri`.`ri1alamat1` AS `ri1alamat1`,`ri`.`ri1alamat2` AS `ri1alamat2`,`ri`.`ri1alamat3` AS `ri1alamat3`,`ri`.`ri2alamat1` AS `ri2alamat1`,`ri`.`ri2alamat2` AS `ri2alamat2`,`ri`.`ri2alamat3` AS `ri2alamat3`,`ri`.`ritermin` AS `ritermin`,`tr`.`trnama` AS `riterminnama`,`tr`.`trharijatuhtempo` AS `riterminharijatuhtempo`,`ri`.`ribagianpembelian` AS `ribagianpembelian`,`c1`.`kkode` AS `ribagianpembeliankode`,`c1`.`knama` AS `ribagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekreturpembelian` AS `rekreturpembelian`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`grnd`.`idgrn` AS `idgrn`,`cf`.`cfiid` AS `idhppfifomasuk`,`cf`.`cfiharga` AS `hppfifo`,`cs`.`idhppikm` AS `idhppkhususmasuk`,`cs`.`harga` AS `hppkhusus`,((`rid`.`jmlbarang` - `rid`.`jmldnr`) / `rid`.`nilaisatuan`) AS `jmlsisadnr`,((`rid`.`jmlbarang` - `rid`.`jmlprt`) / `rid`.`nilaisatuan`) AS `jmlsisaprt`,((`rid`.`jmlbarang` - `rid`.`jmlrealisasi`) / `rid`.`nilaisatuan`) AS `jmlsisarealisasi`,`ri`.`risupplier` AS `risupplier`,`c`.`kkode` AS `risupplierkode`,`c`.`knama` AS `risuppliernama`, ri.ridiskonpersen, ri.ribiayalainpersen, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, ri.ricustomtext1, ri.ricustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama from `m4_ri_detail` `rid` join `m4_ri` `ri` on `rid`.`idri` = `ri`.`riid` left join `m1_terms` `tr` on `ri`.`ritermin` = `tr`.`trkode` left join `m1_contact` `c1` on `ri`.`ribagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `rid`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `rid`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `rid`.`pajak2` = `t2`.`tkode` left join `m1_cogs_fifo_in` `cf` on (`rid`.`idgrndetail` = `cf`.`cfiidtransaksi`) and (`cf`.`cfisumber` = 'RI') left join `m1_cogs_special_in` `cs` on (`rid`.`idgrndetail` = `cs`.`idtransaksi`) and (`cs`.`sumber` = 'RI') left join `m4_grn_detail` `grnd` on `rid`.`idgrndetail` = `grnd`.`idgrndetail` left join `m1_contact` `c` on `ri`.`risupplier` = `c`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = rid.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = rid.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = rid.costcenter LEFT JOIN m1_project p ON p.pkode = rid.proyek"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("idri"), 0), sptField,
                     FxDB(dr("idbarang"), 0), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("tipebarang"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("hargafix"), 0), sptField,
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
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
                     FxDB(dr("rekhutangsementara"), ""), sptField,
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekreturpembelian"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idprdetail"), 0), sptField,
                     FxDB(dr("idcsdetail"), 0), sptField,
                     FxDB(dr("idrqdetail"), 0), sptField,
                     FxDB(dr("idbsdetail"), 0), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("idipcdetail"), 0), sptField,
                     FxDB(dr("idgrndetail"), 0), sptField,
                     FxDB(dr("jmldnr"), 0), sptField,
                     FxDB(dr("statusdnr"), 0), sptField,
                     FxDB(dr("jmlprt"), 0), sptField,
                     FxDB(dr("statusprt"), 0), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("statusrealisasi"), 0), sptField,
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
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("riuraian"), ""), sptField,
                     FxDB(dr("ricatatan"), ""), sptField,
                     FxDB(dr("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ritglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("rinofakturpajak"), ""), sptField,
                     FxDB(dr("risupplierkontak"), ""), sptField,
                     FxDB(dr("ri1alamat1"), ""), sptField,
                     FxDB(dr("ri1alamat2"), ""), sptField,
                     FxDB(dr("ri1alamat3"), ""), sptField,
                     FxDB(dr("ri2alamat1"), ""), sptField,
                     FxDB(dr("ri2alamat2"), ""), sptField,
                     FxDB(dr("ri2alamat3"), ""), sptField,
                     FxDB(dr("ritermin"), ""), sptField,
                     FxDB(dr("riterminnama"), ""), sptField,
                     FxDB(dr("riterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("ribagianpembelian"), 0), sptField,
                     FxDB(dr("ribagianpembeliankode"), ""), sptField,
                     FxDB(dr("ribagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("hppfifo"), 0), sptField,
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("hppkhusus"), 0), sptField,
                     FxDB(dr("jmlsisadnr"), 0), sptField,
                     FxDB(dr("jmlsisaprt"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("risupplier"), ""), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("ridiskonpersen"), 0), sptField,
                     FxDB(dr("ribiayalainpersen"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("ricustomtext1"), ""), sptField,
                     FxDB(dr("ricustomtext2"), ""), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rinotransaksi, riuraian, ricatatan, rinoref, ritgl, ritglnoref, rinofakturpajak, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ritermin, riterminnama, riterminharijatuhtempo, ribagianpembelian, ribagianpembeliankode, ribagianpembeliannama, kodebarang, bhpp, bhppaverage, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, idgrn, idhppfifomasuk, hppfifo, idhppkhususmasuk, hppkhusus, jmlsisadnr, jmlsisaprt, jmlsisarealisasi, risupplier, risupplierkode, risuppliernama, ridiskonpersen, ribiayalainpersen, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, bassset, ambilnotransaksi, ricustomtext1, ricustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, divisinama, subdivisinama, costcenternama, proyeknama"))

        Return wsResult
    End Function

    Public Function ValidasiBatchSerialOld(ByVal dtdetail As DataTable, ByRef dtbatch As DataTable, ByRef dtserial As DataTable, ByVal ftbarang As String, ByVal fieldJmlBarang As String, ByVal jenismutasi As Double) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtbatchBaru As New DataTable, dtserialBaru As New DataTable
        Dim dtval As New DataTable, dtbarang As New DataTable, dtLookup As New DataTable
        Dim jmlbarang As Double = 0, jmlnomor As Double = 0, urutan As Double = 0
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""

        'CEK VARIBEL
        If Len(fieldJmlBarang) = 0 Then errmessage = "Field jmlbarang can't be empty." : GoTo selesai
        If Len(ftbarang) = 0 Then errmessage = "Filter barang can't be empty." : GoTo selesai

        'AMBIL DTBATCH DAN SERIAL SESUAI JENISMUTASINYA
        dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
        dtserialBaru = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '" & jenismutasi & "'")

        'BUAT FILTER DT BATCH DAN SERIAL
        Dim ftCekBatch As String = "(nbtjenismutasi = '" & jenismutasi & "')"
        Dim ftCekSerial As String = "(nstjenismutasi = '" & jenismutasi & "')"

        '1. AMBIL BARANG BATCH DAN SERIAL
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bsatuan, bbatch, bserial FROM m1_item WHERE (bbatch = 1 OR bserial = 1) AND (" & ftbarang & ")")

        '2. CEK NO BATCH DAN SERIAL
        If dtbarang.Rows.Count > 0 Then
            '2.1 CEK NO BATCH
            dtval = AsDataTableFilterSortDt(dtbarang, "bbatch = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI BATCH
                    jmlnomor = AsDataTableDSum(dtbatchBaru, "nbtjml", "nbtjenismutasi = '" & jenismutasi & "' AND nbtidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN BATCH
                    If jmlbarang <> jmlnomor Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Batch for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekBatch = IIf(Len(ftCekBatch.ToString) = 0, "", ftCekBatch & " AND ")
                    ftCekBatch = String.Concat(ftCekBatch, "(nbtidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtbatchBaru, ftCekBatch)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Batch : " & dtval(0)("nbtkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            ElseIf (dtbatchBaru.Rows.Count > 0) Then
                'errmessage = "Batch Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            End If

            '2.2 CEK NO SERIAL
            dtval = AsDataTableFilterSortDt(dtbarang, "bserial = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI SERIAL
                    jmlnomor = AsDataTableDSum(dtserialBaru, "nstjml", "nstjenismutasi = '" & jenismutasi & "' AND nstidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN SERIAL
                    If jmlbarang <> jmlnomor Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Serial for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekSerial = IIf(Len(ftCekSerial.ToString) = 0, "", ftCekSerial & " AND ")
                    ftCekSerial = String.Concat(ftCekSerial, "(nstidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtserialBaru, ftCekSerial)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Serial : " & dtval(0)("nstkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtserial, ftCekSerial)

            ElseIf (dtserialBaru.Rows.Count > 0) Then
                'errmessage = "Serial Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)

            End If


        ElseIf (dtbatchBaru.Rows.Count > 0 Or dtserialBaru.Rows.Count > 0) Then
            'errmessage = "Batch Item not found." : GoTo selesai
            If dtbatchBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)
            End If
            If dtserialBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)
            End If

        End If

selesai:
        Return errmessage
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingPO As String, ByVal ftOutstandingPO As String, ByVal ftExistOutstandingGRN As String, ByVal ftOutstandingGRN As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftPO As String, ByVal ftGRN As String, ByVal termasukPajak As String, ByVal ftExistOutstandingAP As String, ByVal ftOutstandingAP As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = ""
        Dim notransaksi As String = "", sumber As String = "", matauang As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'PO
        If Len(ftExistOutstandingPO) > 0 Then 'ftExistOutstanding = rowExists, idpodetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingPO)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idpodetail=" & dtval.Rows(0)("idpodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in PO" : GoTo selesai
            End If
        End If

        'CEK PO YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftPO) > 0 Then
            sql = "SELECT po.ponotransaksi as notransaksi, (CASE po.pohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_po_detail pod JOIN m4_po po ON pod.idpo = po.poid WHERE " & ftPO & " GROUP BY po.pohargatermasukpajak"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 1 Then
                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                For Each dr1 As DataRow In dtval.Rows
                    errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                Next
                GoTo selesai
            End If

            'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
            If Len(termasukPajak) > 0 Then
                sql = "SELECT i.bkode, pod.idpodetail, po.ponotransaksi as notransaksi, (CASE po.pohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_po_detail pod JOIN m4_po po ON pod.idpo = po.poid JOIN m1_item i ON pod.idbarang = i.bid WHERE (" & ftPO & ") AND po.pohargatermasukpajak <> " & termasukPajak & " ORDER BY pod.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idpodetail = " & dtval.Rows(0)("idpodetail")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & ". " & dtval.Rows(0)("notransaksi") & " " & dtval.Rows(0)("termasukpajak") : GoTo selesai
                End If
            End If

        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingPO) > 0 Then
            sql = "SELECT pod.idpodetail, (pod.jmlbarang - pod.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_po_detail AS pod INNER JOIN m1_item AS i ON pod.idbarang = i.bid WHERE " & ftOutstandingPO
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idpodetail=" & dtval.Rows(0)("idpodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in PO, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If

        'GRN
        If Len(ftExistOutstandingGRN) > 0 Then 'ftExistOutstanding = rowExists, idgrndetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingGRN)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idgrndetail=" & dtval.Rows(0)("idgrndetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in GRN" : GoTo selesai
            End If
        End If

        'CEK GRN YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftGRN) > 0 Then
            sql = "SELECT grn.grnnotransaksi as notransaksi, (CASE grn.grnhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid WHERE " & ftGRN & " GROUP BY grn.grnhargatermasukpajak"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 1 Then
                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                For Each dr1 As DataRow In dtval.Rows
                    errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                Next
                GoTo selesai
            End If

            If Len(termasukPajak) > 0 Then
                sql = "SELECT i.bkode, grnd.idgrndetail, grn.grnnotransaksi as notransaksi, (CASE grn.grnhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_grn_detail grnd JOIN m4_grn grn ON grnd.idgrn = grn.grnid JOIN m1_item i ON grnd.idbarang = i.bid WHERE (" & ftGRN & ") AND grn.grnhargatermasukpajak <> " & termasukPajak & " ORDER BY grnd.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idgrndetail = " & dtval.Rows(0)("idgrndetail")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & ". " & dtval.Rows(0)("notransaksi") & " " & dtval.Rows(0)("termasukpajak") : GoTo selesai
                End If
            End If

        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingGRN) > 0 Then
            sql = "SELECT grnd.idgrndetail, (grnd.jmlbarang - grnd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_grn_detail AS grnd INNER JOIN m1_item AS i ON grnd.idbarang = i.bid WHERE " & ftOutstandingGRN
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idgrndetail=" & dtval.Rows(0)("idgrndetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in GRN, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If


        'Ap
        If Len(ftExistOutstandingAP) > 0 Then 'ftExistOutstanding = rowExists, Apid, Apsumber, Apnotransaksi
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingAP)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("Apnotransaksi")
                sumber = dtval.Rows(0)("Apsumber")
                errmessage = "Advance Sales - " & sumber & " : " & notransaksi & " doesn't exists/yet approved in AP" : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA SISA TRANSAKSI YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
        If Len(ftOutstandingAP) > 0 Then
            sql = "SELECT `Ap`.Apid, `Ap`.Apsumber, `Ap`.Apnotransaksi, `Ap`.Apmatauang, (CASE `Ap`.Apmatauang WHEN s.snilai THEN `Ap`.Apjumlah - `Ap`.Apjumlahbayar ELSE `Ap`.Apjumlahvalas - `Ap`.Apjumlahbayarvalas END) Apsisatransaksi FROM m4_Ap `Ap` LEFT JOIN m0_setting s ON s.smodule =0 AND s.sgrup='accounting' AND s.skode = 'MataUangFungsional' WHERE " & ftOutstandingAP
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                notransaksi = dtval.Rows(0)("Apnotransaksi")
                sumber = dtval.Rows(0)("Apsumber")
                sisa = dtval.Rows(0)("Apsisatransaksi")
                matauang = dtval.Rows(0)("Apmatauang")

                errmessage = "Advance Sales - " & sumber & " : " & notransaksi & " exceeds the amount of payment in AP, payment available " & matauang & " " & FormatNumber(sisa) : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


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


        Dim ProsesValidasiStok As String = F_getSetting(0, "company", "ValidasiStok")
        If ProsesValidasiStok.Equals("0") = False Then
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

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in '" & gudang & "' warehouse" : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA STOK PERGUDANG YG TERSEDIA
            If Len(ftStok) > 0 Then
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang WHERE " & ftStok
                'sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' WHERE " & ftStok
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("stok")
                    gudang = dtval.Rows(0)("kgudang")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in '" & gudang & "' warehouse, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI STOK ---------------------------------------
        End If


selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_RiSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial(), dataCost(), dataRowCost(), dataPay(), dataRowPay() As String

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
        If (dataSplit.Length <> 6) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
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
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 86) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'riid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "riid required numeric." : GoTo selesai
        End If
        'riasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "riasalbarangkategori required numeric." : GoTo selesai
        End If
        'rijenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rijenispembeliankategori required numeric." : GoTo selesai
        End If
        'ricarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ricarabayar required numeric." : GoTo selesai
        End If
        'riautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "riautonotransaksi required numeric." : GoTo selesai
        End If
        'ritgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "ritgl required date." : GoTo selesai
        End If
        'rikodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rikodepa required numeric." : GoTo selesai
        End If
        'risupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "risupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "risupplier can't be empty." : GoTo selesai
        End If
        'ribagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ribagianpembelian required numeric." : GoTo selesai
        End If
        'ritgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "ritgljatuhtempo required date." : GoTo selesai
        End If
        'ritglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "ritglnoref required date." : GoTo selesai
        End If
        'ritglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "ritglpenutupan required date." : GoTo selesai
        End If
        'rikurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rikurs required numeric." : GoTo selesai
        End If
        'rihargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rihargatermasukpajak required numeric." : GoTo selesai
        End If
        'ritotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "ritotal required numeric." : GoTo selesai
        End If
        'rijmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rijmldiskon required numeric." : GoTo selesai
        End If
        'ritotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "ritotalpajak1detail required numeric." : GoTo selesai
        End If
        'ritotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ritotalpajak2detail required numeric." : GoTo selesai
        End If
        'ribiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ribiayalain required numeric." : GoTo selesai
        End If
        'ritotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ritotaltransaksi required numeric." : GoTo selesai
        End If
        'rijmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rijmlbayar required numeric." : GoTo selesai
        End If
        'ristatuslunas(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ristatuslunas required numeric." : GoTo selesai
        End If
        'ritgllunas(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ritgllunas required date." : GoTo selesai
        End If
        'risdhbayarpajak(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "risdhbayarpajak required numeric." : GoTo selesai
        End If
        'ritglbayarpajak(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "ritglbayarpajak required date." : GoTo selesai
        End If
        'riidpr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "riidpr required numeric." : GoTo selesai
        End If
        'riidcs(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "riidcs required numeric." : GoTo selesai
        End If
        'riidrq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "riidrq required numeric." : GoTo selesai
        End If
        'riidbs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "riidbs required numeric." : GoTo selesai
        End If
        'riidpo(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "riidpo required numeric." : GoTo selesai
        End If
        'riidipc(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "riidipc required numeric." : GoTo selesai
        End If
        'riidgrn(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "riidgrn required numeric." : GoTo selesai
        End If
        'ristatusdnr(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ristatusdnr required numeric." : GoTo selesai
        End If
        'ristatusprt(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "ristatusprt required numeric." : GoTo selesai
        End If
        'ristatus(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "ristatus required numeric." : GoTo selesai
        End If
        'ristatussebelumnya(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "ristatussebelumnya required numeric." : GoTo selesai
        End If
        'rijmlrevisi(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rijmlrevisi required numeric." : GoTo selesai
        End If
        'ricetakanke(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "ricetakanke required numeric." : GoTo selesai
        End If
        'riinputuser(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "riinputuser required numeric." : GoTo selesai
        End If
        'riinputtgl(66) As DateTime
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "riinputtgl required date." : GoTo selesai
        End If
        'rimodifikasiuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rimodifikasiuser required numeric." : GoTo selesai
        End If
        'rimodifikasitgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "rimodifikasitgl required date." : GoTo selesai
        End If
        'riposting(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "riposting required numeric." : GoTo selesai
        End If
        'ritutupperiode(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "ritutupperiode required numeric." : GoTo selesai
        End If
        'riisclose(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "riisclose required numeric." : GoTo selesai
        End If
        'ricustomint1(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "ricustomint1 required numeric." : GoTo selesai
        End If
        'ricustomint2(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "ricustomint2 required numeric." : GoTo selesai
        End If
        'ricustomint3(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "ricustomint3 required numeric." : GoTo selesai
        End If
        'ricustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "ricustomdbl1 required numeric." : GoTo selesai
        End If
        'ricustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "ricustomdbl2 required numeric." : GoTo selesai
        End If
        'ricustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "ricustomdbl3 required numeric." : GoTo selesai
        End If
        'ricustomdate1(83) As Date
        If (IsDate(dataUtama(83)) = False) Then
            result(2) = "ricustomdate1 required date." : GoTo selesai
        End If
        'ricustomdate2(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "ricustomdate2 required date." : GoTo selesai
        End If
        'ricustomdate3(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "ricustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'ricabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ricabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ricabang should not be more than 25 character." : GoTo selesai
        End If

        'rilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rilokasi should not be more than 25 character." : GoTo selesai
        End If

        'rigudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rigudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rigudang should not be more than 25 character." : GoTo selesai
        End If

        'risumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "risumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "risumber should not be more than 10 character." : GoTo selesai
        End If

        'rinotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "rinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ritgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ritgl can't be empty" : GoTo selesai
        End If

        'ritgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "ritgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ritglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ritglnoref can't be empty" : GoTo selesai
        End If

        'ritglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ritglpenutupan can't be empty" : GoTo selesai
        End If

        'rimatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rimatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "rimatauang should not be more than 25 character." : GoTo selesai
        End If

        'rikurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rikurs can't be empty" : GoTo selesai
        End If

        'ritotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "ritotal can't be empty" : GoTo selesai
        End If

        'ridiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "ridiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "ridiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rijmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rijmldiskon can't be empty" : GoTo selesai
        End If

        'ritotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "ritotalpajak1detail can't be empty" : GoTo selesai
        End If

        'ritotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "ritotalpajak2detail can't be empty" : GoTo selesai
        End If

        'ribiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "ribiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "ribiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'ribiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "ribiayalain can't be empty" : GoTo selesai
        End If

        'ritotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ritotaltransaksi can't be empty" : GoTo selesai
        End If

        'rijmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rijmlbayar can't be empty" : GoTo selesai
        End If

        'ritgllunas(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ritgllunas can't be empty" : GoTo selesai
        End If

        'ritglbayarpajak(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "ritglbayarpajak can't be empty" : GoTo selesai
        End If

        'riinputtgl(66) As DateTime
        If Len(dataUtama(66)) = 0 Then
            result(2) = "riinputtgl can't be empty" : GoTo selesai
        End If

        'rimodifikasitgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "rimodifikasitgl can't be empty" : GoTo selesai
        End If

        'ricustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "ricustomdbl1 can't be empty" : GoTo selesai
        End If

        'ricustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "ricustomdbl2 can't be empty" : GoTo selesai
        End If

        'ricustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "ricustomdbl3 can't be empty" : GoTo selesai
        End If

        'ricustomdate1(83) As Date
        If Len(dataUtama(83)) = 0 Then
            result(2) = "ricustomdate1 can't be empty" : GoTo selesai
        End If

        'ricustomdate2(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "ricustomdate2 can't be empty" : GoTo selesai
        End If

        'ricustomdate3(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "ricustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "riid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ridiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ristatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "risdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "riid~ricabang~rilokasi~rigudang~riasalbarang~riasalbarangkategori~rijenispembelian~rijenispembeliankategori~ricarabayar~risumber~riautonotransaksi~rinotransaksi~ritgl~rikodepa~risupplier~risupplierkontak~ri1alamat1~ri1alamat2~ri1alamat3~ri2alamat1~ri2alamat2~ri2alamat3~ribagianpembelian~ritermin~ritgljatuhtempo~riuraian~ricatatan~rinoref~ritglnoref~ritglpenutupan~rimatauang~rikurs~rihargatermasukpajak~ritotal~ridiskonpersen~rijmldiskon~ritotalpajak1detail~ritotalpajak2detail~ribiayalainpersen~ribiayalain~ritotaltransaksi~rijmlbayar~ristatuslunas~ritgllunas~rinofakturpajak~risdhbayarpajak~ritglbayarpajak~rirekdiskon~rirekpajak1~rirekpajak2~rirekbiayalain~rirekbayar~riidpr~riidcs~riidrq~riidbs~riidpo~riidipc~riidgrn~ristatusdnr~ristatusprt~ristatus~ristatussebelumnya~rijmlrevisi~ricetakanke~riinputuser~riinputtgl~rimodifikasiuser~rimodifikasitgl~riposting~ritutupperiode~riisclose~ricustomtext1~ricustomtext2~ricustomtext3~ricustomtext4~ricustomtext5~ricustomint1~ricustomint2~ricustomint3~ricustomdbl1~ricustomdbl2~ricustomdbl3~ricustomdate1~ricustomdate2~ricustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idridetail(0) As Integer, idri(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, rekpersediaan(23) As String, rekdiskonpembelian(24) As String, 
        'rekhutangsementara(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idprdetail(32) As Integer, idcsdetail(33) As Integer, idrqdetail(34) As Integer, 
        'idbsdetail(35) As Integer, idpodetail(36) As Integer, idipcdetail(37) As Integer, idgrndetail(38) As Integer, jmldnr(39) As Double, 
        'statusdnr(40) As Integer, jmlprt(41) As Double, statusprt(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, 
        'statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idridetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargafix", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangsementara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idipcdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idgrndetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlprt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusprt", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingPO As String = "", ftOutstandingPO As String = "", updNilaiPO As String = "", updFilterPO As String = ""
        Dim ftExistOutstandingGRN As String = "", ftOutstandingGRN As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""
        Dim idbarang As Integer = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0, jmlbarang As Double = 0
        Dim gudang As String = "", updStokOutBooking As String = ""

        'FILTER PO DAN GRN, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftPO As String = "", ftGRN As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 53) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idridetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idridetail required numeric." : GoTo selesai
            End If
            'idri(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idri required numeric." : GoTo selesai
            End If
            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If
            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargafix(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargafix required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'idgrndetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'jmldnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
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

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
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

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudang(22) As String
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'jmldnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idridetail~idri~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekdiskonpembelian~rekhutangsementara~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String       , idpodetail(36) As Integer      , idgrndetail(38) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idpodetail = dataRowDetail(36) : idgrndetail = dataRowDetail(38)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")


            'VALIDASI OUTSTANDING -------------------------
            If idpodetail <> 0 Then 'PO
                'CEK PO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPO = IIf(Len(ftPO.ToString) = 0, "", ftPO & " OR ")
                ftPO = String.Concat(ftPO, " (pod.idpodetail = " & idpodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPO = IIf(Len(ftExistOutstandingPO.ToString) = 0, "", ftExistOutstandingPO & " UNION ")
                ftExistOutstandingPO = String.Concat(ftExistOutstandingPO, "SELECT EXISTS(SELECT 1 FROM m4_po_detail JOIN m4_po ON idpo = poid WHERE idpodetail = '" & idpodetail & "' AND (postatus = 2 OR postatus = 3 OR postatus = 4 OR postatus = 7) LIMIT 1) as rowExists, '" & idpodetail & "' as idpodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                ftOutstandingPO = IIf(Len(ftOutstandingPO.ToString) = 0, "", ftOutstandingPO & " OR ")
                ftOutstandingPO = String.Concat(ftOutstandingPO, " (pod.idpodetail = " & idpodetail & " AND " & Outstanding & " > (pod.jmlbarang - pod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPO)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                'SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking
            End If

            If idgrndetail <> 0 Then 'GRN
                'CEK GRN YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftGRN = IIf(Len(ftGRN.ToString) = 0, "", ftGRN & " OR ")
                ftGRN = String.Concat(ftGRN, " (grnd.idgrndetail = " & idgrndetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingGRN = IIf(Len(ftExistOutstandingGRN.ToString) = 0, "", ftExistOutstandingGRN & " UNION ")
                ftExistOutstandingGRN = String.Concat(ftExistOutstandingGRN, "SELECT EXISTS(SELECT 1 FROM m4_grn_detail JOIN m4_grn ON idgrn = grnid WHERE idgrndetail = '" & idgrndetail & "' AND (grnstatus = 2 OR grnstatus = 3 OR grnstatus = 4 OR grnstatus = 7) LIMIT 1) as rowExists, '" & idgrndetail & "' as idgrndetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idgrndetail=" & idgrndetail)
                ftOutstandingGRN = IIf(Len(ftOutstandingGRN.ToString) = 0, "", ftOutstandingGRN & " OR ")
                ftOutstandingGRN = String.Concat(ftOutstandingGRN, " (grnd.idgrndetail = " & idgrndetail & " AND " & Outstanding & " > (grnd.jmlbarang - grnd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiGRN = String.Concat("WHEN '" & idgrndetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiGRN)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                updFilterGRN = String.Concat(updFilterGRN, "(idgrndetail = '" & idgrndetail & "')")
            End If
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'MAPPING BUAT WS DATA BATCH -------------------------------------------------------
        'nbtid(0) As Integer, nbtjenismutasi(1) As Integer, nbtidbarang(2) As Integer, nbtkode(3) As String, nbtsumber(4) As String, 
        'nbtidtransaksi(5) As Integer, nbtsatuan(6) As String, nbtjml(7) As Double, nbtcustomtext1(8) As String, nbtcustomtext2(9) As String, 
        'nbtcustomtext3(10) As String, nbtcustomdbl1(11) As Double, nbtcustomdbl2(12) As Double, nbtcustomdbl3(13) As Double, nbtcustomdate1(14) As Date, 
        'nbtcustomdate2(15) As Date, nbtcustomdate3(16) As Date, nbtgudang(17) As String, nbtidbatchin(18) As Integer

        'MAPPING BUAT FLEX DATA BATCH -----------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, nbtgudang, nbtidbatchin

        'Buat datatable BATCH
        Dim dtbatch As New DataTable
        AsDataTableTambahField(dtbatch, "nbtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtbatch, "nbtsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtbatch, "nbtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtbatch, "nbtidbatchin", AsEnumTypeData.AsInt64)


        'CEK PARAMETER DATA BATCH
        If dataSplit(2).Length > 0 Then

            'VALIDASI DAN SET DATA BATCH ======================================================
            'SPLIT PARAMETER DATA BATCH
            dataBatch = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA BATCH ===============================================

            'VALIDASI DAN SET DATA ROW BATCH ==================================================
            Dim JmlDtBatch As Integer = dataBatch.Length
            For i = 1 To JmlDtBatch
                'SPLIT DATA DETAIL
                dataRowBatch = dataBatch(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA BATCH -----------------------------------
                'CEK ARRAY DATA BATCH
                If (dataRowBatch.Length <> 19) Then
                    result(2) = "Batch Row : " & i & " - Invalid batch number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW BATCH ----------------------------

                'VALIDASI TIPE DATA BATCH ------------------------------------------
                'nbtid(0) As Integer
                If (IsNumeric(dataRowBatch(0)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtid required numeric." : GoTo selesai
                End If
                'nbtjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowBatch(1) = 1
                If (IsNumeric(dataRowBatch(1)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjenismutasi required numeric." : GoTo selesai
                End If
                'nbtidbarang(2) As Integer
                If (IsNumeric(dataRowBatch(2)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbarang required numeric." : GoTo selesai
                End If
                'nbtidtransaksi(5) As Integer
                If (IsNumeric(dataRowBatch(5)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidtransaksi required numeric." : GoTo selesai
                End If
                'nbtjml(7) As Double
                If (IsNumeric(dataRowBatch(7)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtjml required numeric." : GoTo selesai
                End If
                'nbtcustomdbl1(11) As Double
                If (IsNumeric(dataRowBatch(11)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl2(12) As Double
                If (IsNumeric(dataRowBatch(12)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 required numeric." : GoTo selesai
                End If
                'nbtcustomdbl3(13) As Double
                If (IsNumeric(dataRowBatch(13)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 required numeric." : GoTo selesai
                End If
                'nbtcustomdate1(14) As Date
                If (IsDate(dataRowBatch(14)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 required date." : GoTo selesai
                End If
                'nbtcustomdate2(15) As Date
                If (IsDate(dataRowBatch(15)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 required date." : GoTo selesai
                End If
                'nbtcustomdate3(16) As Date
                If (IsDate(dataRowBatch(16)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 required date." : GoTo selesai
                End If
                'nbtidbatchin(18) As Integer
                If (IsNumeric(dataRowBatch(18)) = False) Then
                    result(2) = "Batch Row : " & i & " - nbtidbatchin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA BATCH -----------------------------------

                'VALIDASI DATA BATCH ---------------------------------------
                'nbtkode(3) As String
                If Len(dataRowBatch(3)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(3)) > 100 Then
                    result(2) = "Batch Row : " & i & " - nbtkode should not be more than 100 character." : GoTo selesai
                End If

                'nbtsumber(4) As String
                If Len(dataRowBatch(4)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(4)) > 10 Then
                    result(2) = "Batch Row : " & i & " - nbtsumber should not be more than 10 character." : GoTo selesai
                End If

                'nbtsatuan(6) As String
                If Len(dataRowBatch(6)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowBatch(6)) > 25 Then
                    result(2) = "Batch Row : " & i & " - nbtsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nbtjml(7) As Double
                If Len(dataRowBatch(7)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtjml can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl1(11) As Double
                If Len(dataRowBatch(11)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl2(12) As Double
                If Len(dataRowBatch(12)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdbl3(13) As Double
                If Len(dataRowBatch(13)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate1(14) As Date
                If Len(dataRowBatch(14)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate1 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate2(15) As Date
                If Len(dataRowBatch(15)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate2 can't be empty" : GoTo selesai
                End If

                'nbtcustomdate3(16) As Date
                If Len(dataRowBatch(16)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtcustomdate3 can't be empty" : GoTo selesai
                End If

                'nbtgudang(17) As String
                If Len(dataRowBatch(17)) = 0 Then
                    result(2) = "Batch Row : " & i & " - nbtgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA BATCH --------------------------------

                If AsDataTableTambahData(dtbatch, "nbtid~nbtjenismutasi~nbtidbarang~nbtkode~nbtsumber~nbtidtransaksi~nbtsatuan~nbtjml~nbtcustomtext1~nbtcustomtext2~nbtcustomtext3~nbtcustomdbl1~nbtcustomdbl2~nbtcustomdbl3~nbtcustomdate1~nbtcustomdate2~nbtcustomdate3~nbtgudang~nbtidbatchin", dataRowBatch(0) & "~" & dataRowBatch(1) & "~" & dataRowBatch(2) & "~" & dataRowBatch(3) & "~" & dataRowBatch(4) & "~" & dataRowBatch(5) & "~" & dataRowBatch(6) & "~" & dataRowBatch(7) & "~" & dataRowBatch(8) & "~" & dataRowBatch(9) & "~" & dataRowBatch(10) & "~" & dataRowBatch(11) & "~" & dataRowBatch(12) & "~" & dataRowBatch(13) & "~" & dataRowBatch(14) & "~" & dataRowBatch(15) & "~" & dataRowBatch(16) & "~" & dataRowBatch(17) & "~" & dataRowBatch(18)) = False Then
                    result(2) = "Batch Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA BATCH ===========================================

        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'nstid(0) As Integer, nstjenismutasi(1) As Integer, nstidbarang(2) As Integer, nstkode(3) As String, nstsumber(4) As String, 
        'nstidtransaksi(5) As Integer, nstsatuan(6) As String, nstjml(7) As Double, nstcustomtext1(8) As String, nstcustomtext2(9) As String, 
        'nstcustomtext3(10) As String, nstcustomdbl1(11) As Double, nstcustomdbl2(12) As Double, nstcustomdbl3(13) As Double, nstcustomdate1(14) As Date, 
        'nstcustomdate2(15) As Date, nstcustomdate3(16) As Date, nstgudang(17) As String, nstidserialin(18) As Integer

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'nstid, nstjenismutasi, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, nstgudang, nstidserialin

        'Buat datatable serial
        Dim dtserial As New DataTable
        AsDataTableTambahField(dtserial, "nstid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjenismutasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidtransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtserial, "nstsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstjml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtserial, "nstcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtserial, "nstidserialin", AsEnumTypeData.AsInt64)


        'CEK PARAMETER DATA SERIAL
        If dataSplit(3).Length > 0 Then

            'VALIDASI DAN SET DATA SERIAL ======================================================
            'SPLIT PARAMETER DATA SERIAL
            dataSerial = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA SERIAL ===============================================

            'VALIDASI DAN SET DATA ROW SERIAL ==================================================
            Dim JmlDtSerial As Integer = dataSerial.Length
            For i = 1 To JmlDtSerial
                'SPLIT DATA SERIAL
                dataRowSerial = dataSerial(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA SERIAL -----------------------------------
                'CEK ARRAY DATA SERIAL
                If (dataRowSerial.Length <> 19) Then
                    result(2) = "Serial Row : " & i & " - Invalid serial number data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW SERIAL ----------------------------

                'VALIDASI TIPE DATA SERIAL ------------------------------------------
                'nstid(0) As Integer
                If (IsNumeric(dataRowSerial(0)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstid required numeric." : GoTo selesai
                End If
                'nstjenismutasi(1) As Integer
                'JENISMUTASI BARANG MASUK = 1, KELUAR = 0
                dataRowSerial(1) = 1
                If (IsNumeric(dataRowSerial(1)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjenismutasi required numeric." : GoTo selesai
                End If
                'nstidbarang(2) As Integer
                If (IsNumeric(dataRowSerial(2)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidbarang required numeric." : GoTo selesai
                End If
                'nstidtransaksi(5) As Integer
                If (IsNumeric(dataRowSerial(5)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidtransaksi required numeric." : GoTo selesai
                End If
                'nstjml(7) As Double
                If (IsNumeric(dataRowSerial(7)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstjml required numeric." : GoTo selesai
                End If
                'nstcustomdbl1(11) As Double
                If (IsNumeric(dataRowSerial(11)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 required numeric." : GoTo selesai
                End If
                'nstcustomdbl2(12) As Double
                If (IsNumeric(dataRowSerial(12)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 required numeric." : GoTo selesai
                End If
                'nstcustomdbl3(13) As Double
                If (IsNumeric(dataRowSerial(13)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 required numeric." : GoTo selesai
                End If
                'nstcustomdate1(14) As Date
                If (IsDate(dataRowSerial(14)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 required date." : GoTo selesai
                End If
                'nstcustomdate2(15) As Date
                If (IsDate(dataRowSerial(15)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 required date." : GoTo selesai
                End If
                'nstcustomdate3(16) As Date
                If (IsDate(dataRowSerial(16)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 required date." : GoTo selesai
                End If
                'nstidserialin(18) As Integer
                If (IsNumeric(dataRowSerial(18)) = False) Then
                    result(2) = "Serial Row : " & i & " - nstidserialin required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA SERIAL -----------------------------------

                'VALIDASI DATA SERIAL ---------------------------------------
                'nstkode(3) As String
                If Len(dataRowSerial(3)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstkode can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(3)) > 100 Then
                    result(2) = "Serial Row : " & i & " - nstkode should not be more than 100 character." : GoTo selesai
                End If

                'nstsumber(4) As String
                If Len(dataRowSerial(4)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsumber can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(4)) > 10 Then
                    result(2) = "Serial Row : " & i & " - nstsumber should not be more than 10 character." : GoTo selesai
                End If

                'nstsatuan(6) As String
                If Len(dataRowSerial(6)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowSerial(6)) > 25 Then
                    result(2) = "Serial Row : " & i & " - nstsatuan should not be more than 25 character." : GoTo selesai
                End If

                'nstjml(7) As Double
                If Len(dataRowSerial(7)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstjml can't be empty" : GoTo selesai
                End If

                'nstcustomdbl1(11) As Double
                If Len(dataRowSerial(11)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl1 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl2(12) As Double
                If Len(dataRowSerial(12)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl2 can't be empty" : GoTo selesai
                End If

                'nstcustomdbl3(13) As Double
                If Len(dataRowSerial(13)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdbl3 can't be empty" : GoTo selesai
                End If

                'nstcustomdate1(14) As Date
                If Len(dataRowSerial(14)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate1 can't be empty" : GoTo selesai
                End If

                'nstcustomdate2(15) As Date
                If Len(dataRowSerial(15)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate2 can't be empty" : GoTo selesai
                End If

                'nstcustomdate3(16) As Date
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstcustomdate3 can't be empty" : GoTo selesai
                End If

                'nstgudang(17) As String
                If Len(dataRowSerial(16)) = 0 Then
                    result(2) = "Serial Row : " & i & " - nstgudang can't be empty" : GoTo selesai
                End If
                'END OF VALIDASI DATA SERIAL --------------------------------

                If AsDataTableTambahData(dtserial, "nstid~nstjenismutasi~nstidbarang~nstkode~nstsumber~nstidtransaksi~nstsatuan~nstjml~nstcustomtext1~nstcustomtext2~nstcustomtext3~nstcustomdbl1~nstcustomdbl2~nstcustomdbl3~nstcustomdate1~nstcustomdate2~nstcustomdate3~nstgudang~nstidserialin", dataRowSerial(0) & "~" & dataRowSerial(1) & "~" & dataRowSerial(2) & "~" & dataRowSerial(3) & "~" & dataRowSerial(4) & "~" & dataRowSerial(5) & "~" & dataRowSerial(6) & "~" & dataRowSerial(7) & "~" & dataRowSerial(8) & "~" & dataRowSerial(9) & "~" & dataRowSerial(10) & "~" & dataRowSerial(11) & "~" & dataRowSerial(12) & "~" & dataRowSerial(13) & "~" & dataRowSerial(14) & "~" & dataRowSerial(15) & "~" & dataRowSerial(16) & "~" & dataRowSerial(17) & "~" & dataRowSerial(18)) = False Then
                    result(2) = "Serial Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ========================================
        End If

        'MAPPING BUAT WS DATA COST -------------------------------------------------------
        'idricost(0) As Integer, idri(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, catatan(6) As String, costcenter(7) As String, divisi(8) As String, subdivisi(9) As String, 
        'proyek(10) As String, urutan(11) As Integer, idprcost(12) As Integer, idcscost(13) As Integer, idrqcost(14) As Integer, 
        'idbscost(15) As Integer, idpocost(16) As Integer, idipccost(17) As Integer, idgrncost(18) As Integer, jumlahbayar(19) As Double, 
        'statusbayar(20) As Integer, isclose(21) As Integer, customtext1(22) As String, customtext2(23) As String, customtext3(24) As String, 
        'customdbl1(25) As Double, customdbl2(26) As Double, customdbl3(27) As Double, customdate1(28) As Date, customdate2(29) As Date, 
        'customdate3(30) As Date, rekdebit(31) As String, rekkredit(32) As String, kontak(33) As Integer, termasukhpp(34) As Integer

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idricost, idri, kodecost, matauang, kurs, jumlah, catatan, 
        'costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, 
        'idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, rekdebit, rekkredit, kontak, termasukhpp

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idricost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "kodecost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "jumlah", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtcost, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idprcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idcscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idrqcost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idbscost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idpocost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idipccost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "idgrncost", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "jumlahbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "statusbayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "isclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "customtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "customdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekdebit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "rekkredit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "kontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtcost, "termasukhpp", AsEnumTypeData.AsInt64)

        'CEK PARAMETER DATA COST
        If dataSplit(4).Length > 0 Then

            'VALIDASI DAN SET DATA COST ======================================================
            'SPLIT PARAMETER DATA COST
            dataCost = dataSplit(4).Split(sptRow)
            'END OF VALIDASI DAN SET DATA COST ===============================================


            'VALIDASI DAN SET DATA ROW COST ==================================================
            Dim JmlDtCost As Integer = dataCost.Length
            For i = 1 To JmlDtCost
                'SPLIT DATA COST
                dataRowCost = dataCost(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA COST -----------------------------------
                'CEK ARRAY DATA COST
                If (dataRowCost.Length <> 35) Then
                    result(2) = "Cost Row : " & i & " -  Invalid cost transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW COST ----------------------------

                'VALIDASI TIPE DATA COST ------------------------------------------
                'idricost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idricost required numeric." : GoTo selesai
                End If
                'idri(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idri required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowCost(4)) = False) Then
                    result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowCost(5)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'urutan(11) As Integer
                If (IsNumeric(dataRowCost(11)) = False) Then
                    result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'idprcost(12) As Integer
                If (IsNumeric(dataRowCost(12)) = False) Then
                    result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                End If
                'idcscost(13) As Integer
                If (IsNumeric(dataRowCost(13)) = False) Then
                    result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                End If
                'idrqcost(14) As Integer
                If (IsNumeric(dataRowCost(14)) = False) Then
                    result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                End If
                'idbscost(15) As Integer
                If (IsNumeric(dataRowCost(15)) = False) Then
                    result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                End If
                'idpocost(16) As Integer
                If (IsNumeric(dataRowCost(16)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                End If
                'idipccost(17) As Integer
                If (IsNumeric(dataRowCost(17)) = False) Then
                    result(2) = "Cost Row : " & i & " - idipccost required numeric." : GoTo selesai
                End If
                'idgrncost(18) As Integer
                If (IsNumeric(dataRowCost(18)) = False) Then
                    result(2) = "Cost Row : " & i & " - idgrncost required numeric." : GoTo selesai
                End If
                'jumlahbayar(19) As Double
                If (IsNumeric(dataRowCost(19)) = False) Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                End If
                'statusbayar(20) As Integer
                If (IsNumeric(dataRowCost(20)) = False) Then
                    result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                End If
                'isclose(21) As Integer
                If (IsNumeric(dataRowCost(21)) = False) Then
                    result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'customdbl1(25) As Double
                If (IsNumeric(dataRowCost(25)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                End If
                'customdbl2(26) As Double
                If (IsNumeric(dataRowCost(26)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                End If
                'customdbl3(27) As Double
                If (IsNumeric(dataRowCost(27)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                End If
                'customdate1(28) As Date
                If (IsDate(dataRowCost(28)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                End If
                'customdate2(29) As Date
                If (IsDate(dataRowCost(29)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                End If
                'customdate3(30) As Date
                If (IsDate(dataRowCost(30)) = False) Then
                    result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                End If
                'kontak(33) As Integer
                If (IsNumeric(dataRowCost(33)) = False) Then
                    result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                End If
                'termasukhpp(34) As Integer
                If (IsNumeric(dataRowCost(34)) = False) Then
                    result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA COST -----------------------------------

                'VALIDASI DATA COST ---------------------------------------
                'kodecost(2) As String
                If Len(dataRowCost(2)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(2)) > 25 Then
                    result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                End If

                'matauang(3) As String
                If Len(dataRowCost(3)) = 0 Then
                    result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(3)) > 25 Then
                    result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowCost(4)) = 0 Then
                    result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowCost(5)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                End If

                'jumlahbayar(19) As Double
                If Len(dataRowCost(19)) = 0 Then
                    result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                End If

                'customdbl1(25) As Double
                If Len(dataRowCost(25)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                End If

                'customdbl2(26) As Double
                If Len(dataRowCost(26)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                End If

                'customdbl3(27) As Double
                If Len(dataRowCost(27)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                End If

                'customdate1(28) As Date
                If Len(dataRowCost(28)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                End If

                'customdate2(29) As Date
                If Len(dataRowCost(29)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                End If

                'customdate3(30) As Date
                If Len(dataRowCost(30)) = 0 Then
                    result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                End If

                'rekdebit(31) As String
                If dataRowCost(34) = 0 Then
                    If Len(dataRowCost(31)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                    End If
                End If
                If Len(dataRowCost(31)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                End If

                'rekkredit(32) As String
                If Len(dataRowCost(32)) = 0 Then
                    result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                End If
                If Len(dataRowCost(32)) > 25 Then
                    result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                End If
                'END OF VALIDASI DATA COST --------------------------------

                If AsDataTableTambahData(dtcost, "idricost~idri~kodecost~matauang~kurs~jumlah~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~idpocost~idipccost~idgrncost~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rekdebit~rekkredit~kontak~termasukhpp", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

        End If


        'MAPPING BUAT WS DATA PAY -------------------------------------------------------
        'idricarabayar(0) As Integer, idri(1) As Integer, carabayar(2) As Integer, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, jumlahvalas(6) As Double, nogiro(7) As String, tgljt(8) As Date, bank(9) As String, 
        'noacbank(10) As String, rekbank(11) As String, rekgiro(12) As String, catatan(13) As String, urutan(14) As Integer, 
        'isclose(15) As Integer

        'MAPPING BUAT FLEX DATA PAY -----------------------------------------------------
        'idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, 
        'nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, 
        'urutan, isclose

        'Buat datatable pay
        Dim dtpay As New DataTable
        AsDataTableTambahField(dtpay, "idricarabayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtpay, "idri", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtpay, "isclose", AsEnumTypeData.AsInt64)

        'CEK PARAMETER DATA PAY
        If dataSplit(5).Length > 0 Then

            'VALIDASI DAN SET DATA PAY ======================================================
            'SPLIT PARAMETER DATA PAY
            dataPay = dataSplit(5).Split(sptRow)
            'END OF VALIDASI DAN SET DATA PAY ===============================================

            'VALIDASI DAN SET DATA ROW PAY ==================================================
            Dim JmlDtPay As Integer = dataPay.Length
            For i = 1 To JmlDtPay
                'SPLIT DATA PAY
                dataRowPay = dataPay(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA PAY -----------------------------------
                'CEK ARRAY DATA PAY
                If (dataRowPay.Length <> 16) Then
                    result(2) = "Row Pay : " & i & " - Invalid pay transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW PAY ----------------------------

                'VALIDASI TIPE DATA PAY ------------------------------------------
                'idricarabayar(0) As Integer
                If (IsNumeric(dataRowPay(0)) = False) Then
                    result(2) = "Row Pay : " & i & " - idricarabayar required numeric." : GoTo selesai
                End If
                'idri(1) As Integer
                If (IsNumeric(dataRowPay(1)) = False) Then
                    result(2) = "Row Pay : " & i & " - idri required numeric." : GoTo selesai
                End If
                'carabayar(2) As Integer
                If (IsNumeric(dataRowPay(2)) = False) Then
                    result(2) = "Row Pay : " & i & " - carabayar required numeric." : GoTo selesai
                End If
                'kurs(4) As Double
                If (IsNumeric(dataRowPay(4)) = False) Then
                    result(2) = "Row Pay : " & i & " - kurs required numeric." : GoTo selesai
                End If
                'jumlah(5) As Double
                If (IsNumeric(dataRowPay(5)) = False) Then
                    result(2) = "Row Pay : " & i & " - jumlah required numeric." : GoTo selesai
                End If
                'jumlahvalas(6) As Double
                If (IsNumeric(dataRowPay(6)) = False) Then
                    result(2) = "Row Pay : " & i & " - jumlahvalas required numeric." : GoTo selesai
                End If
                'tgljt(8) As Date
                If (IsDate(dataRowPay(8)) = False) Then
                    result(2) = "Row Pay : " & i & " - tgljt required date." : GoTo selesai
                End If
                'urutan(14) As Integer
                If (IsNumeric(dataRowPay(14)) = False) Then
                    result(2) = "Row Pay : " & i & " - urutan required numeric." : GoTo selesai
                End If
                'isclose(15) As Integer
                If (IsNumeric(dataRowPay(15)) = False) Then
                    result(2) = "Row Pay : " & i & " - isclose required numeric." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA PAY -----------------------------------

                'VALIDASI DATA PAY ---------------------------------------
                'matauang(3) As String
                If Len(dataRowPay(3)) = 0 Then
                    result(2) = "Row Pay : " & i & " - matauang can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(3)) > 25 Then
                    result(2) = "Row Pay : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                End If

                'kurs(4) As Double
                If Len(dataRowPay(4)) = 0 Then
                    result(2) = "Row Pay : " & i & " - kurs can't be empty" : GoTo selesai
                End If

                'jumlah(5) As Double
                If Len(dataRowPay(5)) = 0 Then
                    result(2) = "Row Pay : " & i & " - jumlah can't be empty" : GoTo selesai
                End If
                If dataRowPay(5) <= 0 Then
                    result(2) = "Row Pay : " & i & " - jumlah must be more than zero" : GoTo selesai
                End If

                'jumlahvalas(6) As Double
                If Len(dataRowPay(6)) = 0 Then
                    result(2) = "Row Pay : " & i & " - jumlahvalas can't be empty" : GoTo selesai
                End If

                'tgljt(8) As Date
                If Len(dataRowPay(8)) = 0 Then
                    result(2) = "Row Pay : " & i & " - tgljt can't be empty" : GoTo selesai
                End If

                'rekbank(11) As String
                If Len(dataRowPay(11)) = 0 Then
                    result(2) = "Row Pay : " & i & " - rekbank can't be empty" : GoTo selesai
                End If
                If Len(dataRowPay(11)) > 25 Then
                    result(2) = "Row Pay : " & i & " - rekbank should not be more than 25 character." : GoTo selesai
                End If

                'JIKA CARABAYAR = GIRO, MAKA KOLOM DATA GIRO WAJIB DIISI
                If dataRowPay(2) = 2 Then
                    'nogiro(7) As String
                    If Len(dataRowPay(7)) = 0 Then
                        result(2) = "Row Pay : " & i & " - nogiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(7)) > 25 Then
                        result(2) = "Row Pay : " & i & " - nogiro should not be more than 25 character." : GoTo selesai
                    End If

                    'bank(9) As String
                    If Len(dataRowPay(9)) = 0 Then
                        result(2) = "Row Pay : " & i & " - bank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(9)) > 25 Then
                        result(2) = "Row Pay : " & i & " - bank should not be more than 25 character." : GoTo selesai
                    End If

                    'noacbank(10) As String
                    If Len(dataRowPay(10)) = 0 Then
                        result(2) = "Row Pay : " & i & " - noacbank can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(10)) > 50 Then
                        result(2) = "Row Pay : " & i & " - noacbank should not be more than 50 character." : GoTo selesai
                    End If

                    'rekgiro(12) As String
                    If Len(dataRowPay(12)) = 0 Then
                        result(2) = "Row Pay : " & i & " - rekgiro can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowPay(12)) > 25 Then
                        result(2) = "Row Pay : " & i & " - rekgiro should not be more than 25 character." : GoTo selesai
                    End If
                End If
                'END OF VALIDASI DATA PAY --------------------------------

                If AsDataTableTambahData(dtpay, "idricarabayar~idri~carabayar~matauang~kurs~jumlah~jumlahvalas~nogiro~tgljt~bank~noacbank~rekbank~rekgiro~catatan~urutan~isclose", dataRowPay(0) & "~" & dataRowPay(1) & "~" & dataRowPay(2) & "~" & dataRowPay(3) & "~" & dataRowPay(4) & "~" & dataRowPay(5) & "~" & dataRowPay(6) & "~" & dataRowPay(7) & "~" & dataRowPay(8) & "~" & dataRowPay(9) & "~" & dataRowPay(10) & "~" & dataRowPay(11) & "~" & dataRowPay(12) & "~" & dataRowPay(13) & "~" & dataRowPay(14) & "~" & dataRowPay(15)) = False Then
                    result(2) = "Row Pay : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
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
                Dim rsValidasi As String = ""


                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ritgl")), AsFormatTanggal(drutama("ritgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                Dim MUFungsional As String = "", MUUtama As String = ""
                Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
                End If

                'SET MATA UANG UTAMA
                MUUtama = drutama("rimatauang")
                'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("ristatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'JIKA TANPA GRN MAKA CEK BATCH DAN SERIAL
                    If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                        rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    End If
                    'END OF VALIDASI BATCH SERIAL --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingPO, ftOutstandingPO, ftExistOutstandingGRN, ftOutstandingGRN, "", "", "", "", ftPO, ftGRN, drutama("rihargatermasukpajak"), "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'FUNGSI SET TANGGAL JATUH TEMPO DIHILANGKAN, KARENA di flex tambah inputan
                'SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("ritermin").ToString, AsFormatTanggal(drutama("ritgl")), "ritgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("ritgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'SET TANGGAL JATUH TEMPO BERDASARKAN SETTING
                'JIKA SETTING BERDASARKAN TUKAR FAKTUR MAKA TANGGAL JATUH TEMPO DISET 2100-12-31
                Dim setTglJT As String = F_getSetting(4, "tukarfaktur", "UpdateTglJatuhTempoRI")
                If setTglJT.Equals("1") Then
                    drutama("ritgljatuhtempo") = "2100-12-31"
                End If


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TAMBAHKAN FIELD SUBTOTAL PADA COST
                'SUBTOTAL = jumlah
                AsDataTableTambahField(dtcost, "subtotal", AsEnumTypeData.AsDouble)
                dtcost.Columns("subtotal").Expression = "jumlah"

                ''TOTAL = subtotal detail + subtotal cost
                'drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal") + AsDataTableDSum(dtcost, "subtotal")

                'TOTAL = subtotal detail
                drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("ritotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("ritotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("rihargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ritotalpajak1detail")) + Double.Parse(drutama("ritotalpajak2detail")) + Double.Parse(drutama("ribiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ribiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA TUNAI MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("ricarabayar")) = 0 Then

                    'SET JML BAYAR ==========================================
                    If MUUtama = MUFungsional Then
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAH
                        drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlah")

                    Else
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAHVALAS
                        drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlahvalas")

                    End If
                    'END OF SET JML BAYAR ===================================


                    'SET TGL LUNAS ==========================================
                    'JIKA TUNAI MAKA TGL LUNAS = TGL TRANSAKSI
                    If Double.Parse(drutama("rijmlbayar")) >= Double.Parse(drutama("ritotaltransaksi")) Then
                        drutama("ritgllunas") = drutama("ritgl") : drutama("ristatuslunas") = 2

                    ElseIf Double.Parse(drutama("rijmlbayar")) < 1 Then
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                    Else
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 1

                    End If
                    'END OF SET TGL LUNAS ===================================

                Else
                    drutama("rijmlbayar") = 0 : drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("riid")
                    notransaksi = drutama("rinotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(riid), rinotransaksi FROM m4_ri WHERE riid='" & result(4) & "' AND ristatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_ri_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Ri set ricabang  = '" & FixQuotes(drutama("ricabang")) & "', rilokasi  = '" & FixQuotes(drutama("rilokasi")) & "', rigudang  = '" & FixQuotes(drutama("rigudang")) & "', riasalbarang  = '" & FixQuotes(drutama("riasalbarang")) & "', riasalbarangkategori  = " & drutama("riasalbarangkategori") & ", rijenispembelian  = '" & FixQuotes(drutama("rijenispembelian")) & "', rijenispembeliankategori  = " & drutama("rijenispembeliankategori") & ", ricarabayar  = " & drutama("ricarabayar") & ", risumber  = '" & FixQuotes(drutama("risumber")) & "', riautonotransaksi  = " & drutama("riautonotransaksi") & ", rinotransaksi  = '" & FixQuotes(notransaksi) & "', ritgl  = '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', rikodepa  = " & drutama("rikodepa") & ", risupplier  = " & drutama("risupplier") & ", risupplierkontak  = '" & FixQuotes(drutama("risupplierkontak")) & "', ri1alamat1  = '" & FixQuotes(drutama("ri1alamat1")) & "', ri1alamat2  = '" & FixQuotes(drutama("ri1alamat2")) & "', ri1alamat3  = '" & FixQuotes(drutama("ri1alamat3")) & "', ri2alamat1  = '" & FixQuotes(drutama("ri2alamat1")) & "', ri2alamat2  = '" & FixQuotes(drutama("ri2alamat2")) & "', ri2alamat3  = '" & FixQuotes(drutama("ri2alamat3")) & "', ribagianpembelian  = " & drutama("ribagianpembelian") & ", ritermin  = '" & FixQuotes(drutama("ritermin")) & "', ritgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', riuraian  = '" & FixQuotes(drutama("riuraian")) & "', ricatatan  = '" & FixQuotes(drutama("ricatatan")) & "', rinoref  = '" & FixQuotes(drutama("rinoref")) & "', ritglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', ritglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', rimatauang  = '" & FixQuotes(drutama("rimatauang")) & "', rikurs  = '" & FixDouble(drutama("rikurs")) & "', rihargatermasukpajak  = " & drutama("rihargatermasukpajak") & ", ritotal  = '" & FixDouble(drutama("ritotal")) & "', ridiskonpersen  = '" & FixQuotes(drutama("ridiskonpersen")) & "', rijmldiskon  = '" & FixDouble(drutama("rijmldiskon")) & "', ritotalpajak1detail  = '" & FixDouble(drutama("ritotalpajak1detail")) & "', ritotalpajak2detail  = '" & FixDouble(drutama("ritotalpajak2detail")) & "', ribiayalainpersen  = '" & FixQuotes(drutama("ribiayalainpersen")) & "', ribiayalain  = '" & FixDouble(drutama("ribiayalain")) & "', ritotaltransaksi  = '" & FixDouble(drutama("ritotaltransaksi")) & "', rijmlbayar  = '" & FixDouble(drutama("rijmlbayar")) & "', ristatuslunas  = " & drutama("ristatuslunas") & ", ritgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', rinofakturpajak  = '" & FixQuotes(drutama("rinofakturpajak")) & "', risdhbayarpajak  = " & drutama("risdhbayarpajak") & ", ritglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', rirekdiskon  = '" & FixQuotes(drutama("rirekdiskon")) & "', rirekpajak1  = '" & FixQuotes(drutama("rirekpajak1")) & "', rirekpajak2  = '" & FixQuotes(drutama("rirekpajak2")) & "', rirekbiayalain  = '" & FixQuotes(drutama("rirekbiayalain")) & "', rirekbayar  = '" & FixQuotes(drutama("rirekbayar")) & "', riidpr  = " & drutama("riidpr") & ", riidcs  = " & drutama("riidcs") & ", riidrq  = " & drutama("riidrq") & ", riidbs  = " & drutama("riidbs") & ", riidpo  = " & drutama("riidpo") & ", riidipc  = " & drutama("riidipc") & ", riidgrn  = " & drutama("riidgrn") & ", ristatusdnr  = " & drutama("ristatusdnr") & ", ristatusprt  = " & drutama("ristatusprt") & ", ristatus  = " & drutama("ristatus") & ", ristatussebelumnya  = " & drutama("ristatussebelumnya") & ", rijmlrevisi  = rijmlrevisi+1, ricetakanke  = " & drutama("ricetakanke") & ", rimodifikasiuser  = " & drutama("rimodifikasiuser") & ", rimodifikasitgl  = NOW(), riposting  = 0, ritutupperiode  = " & drutama("ritutupperiode") & ", ricustomtext1  = '" & FixQuotes(drutama("ricustomtext1")) & "', ricustomtext2  = '" & FixQuotes(drutama("ricustomtext2")) & "', ricustomtext3  = '" & FixQuotes(drutama("ricustomtext3")) & "', ricustomtext4  = '" & FixQuotes(drutama("ricustomtext4")) & "', ricustomtext5  = '" & FixQuotes(drutama("ricustomtext5")) & "', ricustomint1  = " & drutama("ricustomint1") & ", ricustomint2  = " & drutama("ricustomint2") & ", ricustomint3  = " & drutama("ricustomint3") & ", ricustomdbl1  = '" & FixDouble(drutama("ricustomdbl1")) & "', ricustomdbl2  = '" & FixDouble(drutama("ricustomdbl2")) & "', ricustomdbl3  = '" & FixDouble(drutama("ricustomdbl3")) & "', ricustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', ricustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', ricustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "' where riid = '" & drutama("riid") & "'"
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

                    If drutama("riautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"))
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
                        notransaksi = drutama("rinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Ri (ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3) values('" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("rigudang")) & "', '" & FixQuotes(drutama("riasalbarang")) & "', " & drutama("riasalbarangkategori") & ", '" & FixQuotes(drutama("rijenispembelian")) & "', " & drutama("rijenispembeliankategori") & ", " & drutama("ricarabayar") & ", '" & FixQuotes(drutama("risumber")) & "', " & drutama("riautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drutama("risupplierkontak")) & "', '" & FixQuotes(drutama("ri1alamat1")) & "', '" & FixQuotes(drutama("ri1alamat2")) & "', '" & FixQuotes(drutama("ri1alamat3")) & "', '" & FixQuotes(drutama("ri2alamat1")) & "', '" & FixQuotes(drutama("ri2alamat2")) & "', '" & FixQuotes(drutama("ri2alamat3")) & "', " & drutama("ribagianpembelian") & ", '" & FixQuotes(drutama("ritermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(drutama("rinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', " & drutama("rihargatermasukpajak") & ", '" & FixDouble(drutama("ritotal")) & "', '" & FixQuotes(drutama("ridiskonpersen")) & "', '" & FixDouble(drutama("rijmldiskon")) & "', '" & FixDouble(drutama("ritotalpajak1detail")) & "', '" & FixDouble(drutama("ritotalpajak2detail")) & "', '" & FixQuotes(drutama("ribiayalainpersen")) & "', '" & FixDouble(drutama("ribiayalain")) & "', '" & FixDouble(drutama("ritotaltransaksi")) & "', '" & FixDouble(drutama("rijmlbayar")) & "', " & drutama("ristatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', '" & FixQuotes(drutama("rinofakturpajak")) & "', " & drutama("risdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', '" & FixQuotes(drutama("rirekdiskon")) & "', '" & FixQuotes(drutama("rirekpajak1")) & "', '" & FixQuotes(drutama("rirekpajak2")) & "', '" & FixQuotes(drutama("rirekbiayalain")) & "', '" & FixQuotes(drutama("rirekbayar")) & "', " & drutama("riidpr") & ", " & drutama("riidcs") & ", " & drutama("riidrq") & ", " & drutama("riidbs") & ", " & drutama("riidpo") & ", " & drutama("riidipc") & ", " & drutama("riidgrn") & ", " & drutama("ristatusdnr") & ", " & drutama("ristatusprt") & ", " & drutama("ristatus") & ", " & drutama("ristatussebelumnya") & ", " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", NOW(), " & drutama("rimodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ritutupperiode") & ", " & drutama("riisclose") & ", '" & FixQuotes(drutama("ricustomtext1")) & "', '" & FixQuotes(drutama("ricustomtext2")) & "', '" & FixQuotes(drutama("ricustomtext3")) & "', '" & FixQuotes(drutama("ricustomtext4")) & "', '" & FixQuotes(drutama("ricustomtext5")) & "', " & drutama("ricustomint1") & ", " & drutama("ricustomint2") & ", " & drutama("ricustomint3") & ", '" & FixDouble(drutama("ricustomdbl1")) & "', '" & FixDouble(drutama("ricustomdbl2")) & "', '" & FixDouble(drutama("ricustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select riid from M4_ri where rinotransaksi='" & notransaksi & "' AND riinputuser= '" & userid & "' order by rimodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ri_Detail where idri = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idridetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangsementara")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Ri_Detail(idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ri_Cost where idri = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses cost
                If (dtcost.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtcost.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idricost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", " & dr1("idpocost") & ", " & dr1("idipccost") & ", " & dr1("idgrncost") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', '" & FixQuotes(dr1("kontak")) & "', '" & FixQuotes(dr1("termasukhpp")) & "')")
                    Next
                    sql = "Insert into M4_Ri_Cost(idricost, idri, kodecost, matauang, kurs, jumlah, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rekdebit, rekkredit, kontak, termasukhpp) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Hapus pay ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_ri_Pay where idri = '" & result(4) & "'"
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
                If (dtpay.Rows.Count > 0) And drutama("ricarabayar") = 0 Then
                    Dim strValue2 As New StringBuilder, strVoucher As New StringBuilder, ftVoucher As New StringBuilder
                    For Each dr1 As DataRow In dtpay.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idricarabayar") & ", " & result(4) & ", " & dr1("carabayar") & ", '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixDouble(dr1("jumlahvalas")) & "', '" & FixQuotes(dr1("nogiro")) & "', '" & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & "', '" & FixQuotes(dr1("bank")) & "', '" & FixQuotes(dr1("noacbank")) & "', '" & FixQuotes(dr1("rekbank")) & "', '" & FixQuotes(dr1("rekgiro")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ")")
                    Next
                    sql = "Insert into M4_ri_Pay(idricarabayar, idri, carabayar, matauang, kurs, jumlah, jumlahvalas, nogiro, tgljt, bank, noacbank, rekbank, rekgiro, catatan, urutan, isclose) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RI'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses batch
                If (dtbatch.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nbtjenismutasi") & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_Transaction(nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus serial ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RI'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses serial
                If (dtserial.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & 0 & ", " & dr1("nstjenismutasi") & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_Transaction(nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("ristatus") = 2 Then
                    If Len(updNilaiPO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpo FROM M4_po_detail WHERE " & updFilterPO & " GROUP BY idpo")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_po_detail WHERE " & ftDetail & " GROUP BY idpo")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPO = "" : updFilterPO = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                                updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                            Next

                            sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If

                    If Len(updNilaiGRN) > 0 Then 'GRN
                        'UPDATE DETAIL
                        sql = "UPDATE m4_grn_detail SET jmlrealisasi = (CASE idgrndetail " & updNilaiGRN & " ELSE jmlrealisasi END) WHERE " & updFilterGRN
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idgrn FROM m4_grn_detail WHERE " & updFilterGRN & " GROUP BY idgrn")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idgrn = '" & dr1("idgrn") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idgrn, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_grn_detail WHERE " & ftDetail & " GROUP BY idgrn")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiGRN = "" : updFilterGRN = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiGRN = String.Concat(updNilaiGRN, "WHEN '" & dr1("idgrn") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                                updFilterGRN = String.Concat(updFilterGRN, "(grnid = '" & dr1("idgrn") & "')")
                            Next

                            sql = "UPDATE m4_grn SET grnstatusrealisasi = (CASE grnid " & updNilaiGRN & " ELSE grnstatusrealisasi END) WHERE " & updFilterGRN
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                    End If
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================


                'INSERT NO BATCH ================================================================
                If dtbatch.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtbatch.Rows
                        'QUERY INSERT NO BATCH IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        'mapping        nbiidbatchin,                     nbigudang,                  nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                        strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF INSERT NO BATCH =========================================================


                'INSERT NO SERIAL ===============================================================
                If dtserial.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtserial.Rows
                        'QUERY INSERT NO SERIAL IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        'mapping       nsiidserialin,                     nsigudang,                  nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                        strValue2.Append("(" & 0 & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
                    Next
                    sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF INSERT NO SERIAL ========================================================


                'UPDATE STOK BOOKING ============================================================
                If Len(updStokOutBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =====================================================


                Dim sumber As String = "RI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0


                'JIKA TANPA GRN MAKA HITUNG TRANSAKSI BARANG DAN POSTING HPP
                If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB("SELECT rid.idridetail, rid.idbarang, rid.namabarang, rid.tipebarang, rid.jml, rid.satuan, rid.jmlbarang, rid.satuanbarang, rid.matauang, rid.kurs, rid.harga, rid.diskon, rid.jmldiskon, rid.gudang, rid.catatan, rid.costcenter, rid.divisi, rid.subdivisi, rid.proyek, ri.riinputtgl, i.bhpp, rid.jmlpajak1, rid.jmlpajak2 FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE rid.idri = '" & result(4) & "' ORDER BY rid.urutan")

                    Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ====================================================
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            gudang = dr1("gudang")

                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDB(sql)
                            If dtSaldo.Rows.Count > 0 Then
                                'set nilai stok
                                bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                'jenismutasi dan postinghpp 
                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                jenismutasi = 1 : postinghpp = 0

                                'hitung saldojml = bstok + jmlbarang
                                saldojml = bstok + jmlbarang

                                'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                hpp = 0 : saldohpp = 0 : saldonilai = 0

                                'QUERY INSERT TRANSAKSI BARANG
                                strTransaksiBarang.Clear()
                                'mapping                        id,                            cabang,                                    lokasi,                             gudang,                        kodepa,           jenismutasi,                              sumber,              idutama,                  iddetail,                      notransaksi,                                                 tgl,                          kontak,                 idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rikodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("risumber")) & "', " & result(4) & ", " & dr1("idridetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("risupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("riinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("riinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                                sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK PERGUDANG
                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK GLOBAL
                                'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                If drutama("rihargatermasukpajak") = 0 Then
                                    sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' WHERE bid = '" & idbarang & "'"
                                Else
                                    sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' WHERE bid = '" & idbarang & "'"
                                End If
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()
                            End If

                        Next
                        'END OF INSERT ITEM TRANSACTION =============================================

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If


                    'INSERT MSMQ COGS ===============================================================
                    If drutama("ristatus") = 2 Then
                        Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                        'BUAT ID UNIQUE
                        mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

                        'MSMQ TABEL
                        sql = "Insert into M0_Msmq_Cogs(mcid, mcsumber, mcidtransaksi, mcprogress, mcpesan, mctglantrian, mctglselesai, mcuserid) values ('" _
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
                        Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                        If ProsesHpp.Equals("0") = False Then
                            hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                            If Len(hasilMsmq) > 0 Then
                                result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                    End If
                    'END OF INSERT MSMQ COGS ========================================================

                End If


                'INSERT MSMQ JURNAL =================================================================
                If drutama("ristatus") = 2 Then
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
    Public Function M4_RiUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ritgl, Rinotransaksi, Ristatus FROM M4_Ri WHERE Riid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ristatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_ri_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_ri_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDB("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0")
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updStokOut As String = "", gudangOut As String = "", updStokInBooking As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idridetail As Integer = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0
                Dim updNilaiPO As String = "", updFilterPO As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idridetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, idgrndetail, gudang, urutan, rijenispembeliankategori FROM m4_ri_detail JOIN m4_ri ON idri = riid WHERE idri = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : idridetail = dr1("idridetail") : jmlbarang = dr1("jmlbarang") : idpodetail = dr1("idpodetail") : idgrndetail = dr1("idgrndetail") : gudangOut = dr1("gudang")

                        'JIKA RI TANPA GRN MAKA CEK STOK
                        If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                            'BUAT FILTER CEK HPP KHUSUS(I)
                            ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                            ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idridetail & "' AND sumber = 'RI')")

                            'BUAT FILER CEK HPP FIFO(F)
                            ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                            ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idridetail & "' AND cfisumber = 'RI')")

                            'BUAT FILTER CEK STOCK EXIST
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            'BUAT FILTER CEK JML STOCK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudang='" & gudangOut & "'")
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                            'SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok
                        End If


                        'UPDATE OUTSTANDING ---------------------------
                        If idpodetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING PO
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                            updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiPO)
                            '2. SET FILTERUPDATE OUTSTANDING PO
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                            If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                                'SET NILAI UPDATE STOK BOOKING MASUK
                                updStokInBooking = IIf(Len(updStokInBooking.ToString) = 0, "", updStokInBooking & ", ")
                                updStokInBooking = String.Concat(updStokInBooking, "('" & idbarang & "', '" & gudangOut & "', ('" & jmlbarang & "'))") ' idbarang, kgudang, stok
                            End If
                        End If

                        If idgrndetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING GRN
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idgrndetail=" & idgrndetail)
                            updNilaiGRN = String.Concat("WHEN '" & idgrndetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiGRN)
                            '2. SET FILTERUPDATE OUTSTANDING GRN
                            updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                            updFilterGRN = String.Concat(updFilterGRN, "(idgrndetail = '" & idgrndetail & "')")
                        End If


                        If Double.Parse(dr1("rijenispembeliankategori")) = 1 Then
                            'SET NILAI UPDATE STOK BARANG
                            Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                            'SET FILTERUPDATE STOK BARANG
                            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                            ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")
                        End If
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftHppI, ftHppF, ftExistStok, ftStok, "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterPO) > 0 Then 'PO
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idpo FROM M4_po_detail WHERE " & updFilterPO & " GROUP BY idpo")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_po_detail WHERE " & ftDetail & " GROUP BY idpo")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiPO = "" : updFilterPO = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                            updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                        Next

                        sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If

                If Len(updFilterGRN) > 0 Then 'GRN
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_grn_detail SET jmlrealisasi = (CASE idgrndetail " & updNilaiGRN & " ELSE jmlrealisasi END) WHERE " & updFilterGRN
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE OUTSTANDING UTAMA --------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idgrn FROM m4_grn_detail WHERE " & updFilterGRN & " GROUP BY idgrn")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idgrn = '" & dr1("idgrn") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idgrn, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_grn_detail WHERE " & ftDetail & " GROUP BY idgrn")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiGRN = "" : updFilterGRN = ""
                        For Each dr1 As DataRow In dtOut.Rows
                            '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                            If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                statusOut = 2
                            ElseIf dr1("jmlrealisasi") < 1 Then
                                statusOut = 0
                            Else
                                statusOut = 1
                            End If
                            '2. SET NILAI UPDATE OUTSTANDING
                            updNilaiGRN = String.Concat(updNilaiGRN, "WHEN '" & dr1("idgrn") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                            updFilterGRN = String.Concat(updFilterGRN, "(grnid = '" & dr1("idgrn") & "')")
                        Next

                        sql = "UPDATE m4_grn SET grnstatusrealisasi = (CASE grnid " & updNilaiGRN & " ELSE grnstatusrealisasi END) WHERE " & updFilterGRN
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI =============================================


                'DELETE HPP KHUSUS (I)
                If Len(ftHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE HPP FIFO (F)
                If Len(ftHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'DELETE NO BATCH IN MASUK ---------------------------
                sql = "DELETE FROM m1_no_batch_in WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE NO SERIAL IN MASUK --------------------------
                sql = "DELETE FROM m1_no_serial_in WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'UPDATE STOK BOOKING ============================================================
                If Len(updStokInBooking) > 0 Then
                    sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokInBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK BOOKING =====================================================


                'UPDATE STOK ==================================================================
                'STOK KELUAR
                If Len(updStokOut) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'STOK BARANG m1_item
                If Len(updStokBarang) > 0 Then
                    sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarang & " ELSE bstok END) WHERE " & ftStokBarang
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE STOK ===========================================================


                'DELETE TRANSAKSI BARANG ======================================================
                'HAPUS DI M1_ITEM_TRANSACTION
                sql = "DELETE FROM m1_item_transaction WHERE sumber = '" & sumber & "' AND idutama = '" & idtransaksi & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG ===============================================


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m4_ri_detail rid ON i.bid = rid.idbarang AND rid.idri = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m4_ri_detail rid ON it.idbarang = rid.idbarang AND rid.idri = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m4_ri ri ON rid.idri = ri.riid AND CONCAT(it.sumber,it.idutama) <> CONCAT(ri.risumber,ri.riid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                Dim dtTotalFungsional As DataTable = AsDataTableAmbilDariDB("SELECT SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END)) as total FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid WHERE rid.idri = '" & FixDouble(idtransaksi) & "'")
                Dim dtBiayaFungsional As DataTable = AsDataTableAmbilDariDB("SELECT IFNULL(SUM(ric.jumlah * ric.kurs),0) as biaya FROM m4_ri ri LEFT JOIN m4_ri_cost ric ON ri.riid = ric.idri AND ric.termasukhpp = 1 WHERE ri.riid = '" & FixDouble(idtransaksi) & "'")
                Dim vTotalFungsional As Double = 0, vBiayaFungsional As Double = 0
                If dtTotalFungsional.Rows.Count > 0 Then
                    vTotalFungsional = Double.Parse(FixDouble(FxDB(dtTotalFungsional.Rows(0)("total"), 0)))
                End If
                If dtBiayaFungsional.Rows.Count > 0 Then
                    vBiayaFungsional = Double.Parse(FixDouble(FxDB(dtBiayaFungsional.Rows(0)("biaya"), 0)))
                End If

                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT rid.idbarang, "
                sql &= " ROUND((CASE " & FixDouble(vTotalFungsional) & " "
                sql &= " WHEN 0 THEN (SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " ELSE (SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " + (((SUM((CASE ri.rihargatermasukpajak WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) * rid.kurs ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1 - rid.jmlpajak2) * rid.kurs END))) "
                sql &= " / " & FixDouble(vTotalFungsional) & ") * " & FixDouble(vBiayaFungsional) & ") END), 2) as nilai, "
                sql &= " SUM(rid.jmlbarang) as jumlah "
                sql &= " FROM m4_ri_detail rid "
                sql &= " JOIN m4_ri ri ON rid.idri = ri.riid "
                sql &= " WHERE rid.idri = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY rid.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok + h.jumlah) * i.bhppaverage) - (h.nilai)) / (i.bstok),2),0) END)"
                'result(2) = sql : Trans.Rollback() : GoTo selesai
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE BHPPAVERAGE M1_ITEM ============================================


                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RI' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Ri SET Ristatus = " & nilaiStatus & ", Rimodifikasiuser='" & userid & "', Rimodifikasitgl = NOW(), Riposting = 0, Ripostingtgl = '1971-01-01 00:00:00', Rijmlrevisi = Rijmlrevisi + 1 WHERE Riid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RiDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Riid, Rinotransaksi FROM M4_Ri WHERE Riid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ricabang, rilokasi, risumber, riautonotransaksi, rinotransaksi, ritgl"
            sql &= " FROM M4_ri"
            sql &= " WHERE riid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ricabang")
                lokasi = dtNomorNext.Rows(0)("rilokasi")
                sumber = dtNomorNext.Rows(0)("risumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("riautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ritgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'HAPUS BATCH
            sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi = '" & idtransaksi & "' AND nbtsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'HAPUS SERIAL
            sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi = '" & idtransaksi & "' AND nstsumber = '" & sumber & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE COST
            sql = "DELETE FROM M4_Ri_Cost WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE PAY
            sql = "DELETE FROM M4_ri_Pay WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE DETAIL
            sql = "DELETE FROM M4_Ri_Detail WHERE idri ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Ri WHERE riid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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



    <WebMethod()>
    Public Function M4_RiBalance(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

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
        AsDataTableTambahField(dtutama, "riid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ridiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ristatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "risdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate3", AsEnumTypeData.AsString)


        Dim JmlDt As Integer = dataUtama.Length
        For i = 1 To JmlDt
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Utama -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 86) Then
                result(2) = "Invalid main transaction data parameter. " & dataRowUtama.Length & "" : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Utama ----------------------------

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'riid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "riid required numeric." : GoTo selesai
            End If
            'riasalbarangkategori(5) As Integer
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "riasalbarangkategori required numeric." : GoTo selesai
            End If
            'rijenispembeliankategori(7) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "rijenispembeliankategori required numeric." : GoTo selesai
            End If
            'ricarabayar(8) As Integer
            dataRowUtama(8) = 1 'SALDO AWAL HUTANG KREDIT
            If (IsNumeric(dataRowUtama(8)) = False) Then
                result(2) = "ricarabayar required numeric." : GoTo selesai
            End If
            'riautonotransaksi(10) As Integer
            If (IsNumeric(dataRowUtama(10)) = False) Then
                result(2) = "riautonotransaksi required numeric." : GoTo selesai
            End If
            'ritgl(12) As Date
            If (IsDate(dataRowUtama(12)) = False) Then
                result(2) = "ritgl required date." : GoTo selesai
            End If
            'rikodepa(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "rikodepa required numeric." : GoTo selesai
            End If
            'risupplier(14) As Integer
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "risupplier required numeric." : GoTo selesai
            End If
            If (dataRowUtama(14) < 1) Then
                result(2) = "risupplier can't be empty." : GoTo selesai
            End If
            'ribagianpembelian(22) As Integer
            If (IsNumeric(dataRowUtama(22)) = False) Then
                result(2) = "ribagianpembelian required numeric." : GoTo selesai
            End If
            'ritgljatuhtempo(24) As Date
            If (IsDate(dataRowUtama(24)) = False) Then
                result(2) = "ritgljatuhtempo required date." : GoTo selesai
            End If
            'ritglnoref(28) As Date
            If (IsDate(dataRowUtama(28)) = False) Then
                result(2) = "ritglnoref required date." : GoTo selesai
            End If
            'ritglpenutupan(29) As Date
            If (IsDate(dataRowUtama(29)) = False) Then
                result(2) = "ritglpenutupan required date." : GoTo selesai
            End If
            'rikurs(31) As Double
            If (IsNumeric(dataRowUtama(31)) = False) Then
                result(2) = "rikurs required numeric." : GoTo selesai
            End If
            'rihargatermasukpajak(32) As Integer
            If (IsNumeric(dataRowUtama(32)) = False) Then
                result(2) = "rihargatermasukpajak required numeric." : GoTo selesai
            End If
            'ritotal(33) As Double
            If (IsNumeric(dataRowUtama(33)) = False) Then
                result(2) = "ritotal required numeric." : GoTo selesai
            End If
            'rijmldiskon(35) As Double
            If (IsNumeric(dataRowUtama(35)) = False) Then
                result(2) = "rijmldiskon required numeric." : GoTo selesai
            End If
            'ritotalpajak1detail(36) As Double
            If (IsNumeric(dataRowUtama(36)) = False) Then
                result(2) = "ritotalpajak1detail required numeric." : GoTo selesai
            End If
            'ritotalpajak2detail(37) As Double
            If (IsNumeric(dataRowUtama(37)) = False) Then
                result(2) = "ritotalpajak2detail required numeric." : GoTo selesai
            End If
            'ribiayalain(39) As Double
            If (IsNumeric(dataRowUtama(39)) = False) Then
                result(2) = "ribiayalain required numeric." : GoTo selesai
            End If
            'ritotaltransaksi(40) As Double
            If (IsNumeric(dataRowUtama(40)) = False) Then
                result(2) = "ritotaltransaksi required numeric." : GoTo selesai
            End If
            'rijmlbayar(41) As Double
            If (IsNumeric(dataRowUtama(41)) = False) Then
                result(2) = "rijmlbayar required numeric." : GoTo selesai
            End If
            'ristatuslunas(42) As Integer
            If (IsNumeric(dataRowUtama(42)) = False) Then
                result(2) = "ristatuslunas required numeric." : GoTo selesai
            End If
            'ritgllunas(43) As Date
            If (IsDate(dataRowUtama(43)) = False) Then
                result(2) = "ritgllunas required date." : GoTo selesai
            End If
            'risdhbayarpajak(45) As Integer
            If (IsNumeric(dataRowUtama(45)) = False) Then
                result(2) = "risdhbayarpajak required numeric." : GoTo selesai
            End If
            'ritglbayarpajak(46) As Date
            If (IsDate(dataRowUtama(46)) = False) Then
                result(2) = "ritglbayarpajak required date." : GoTo selesai
            End If
            'riidpr(52) As Integer
            If (IsNumeric(dataRowUtama(52)) = False) Then
                result(2) = "riidpr required numeric." : GoTo selesai
            End If
            'riidcs(53) As Integer
            If (IsNumeric(dataRowUtama(53)) = False) Then
                result(2) = "riidcs required numeric." : GoTo selesai
            End If
            'riidrq(54) As Integer
            If (IsNumeric(dataRowUtama(54)) = False) Then
                result(2) = "riidrq required numeric." : GoTo selesai
            End If
            'riidbs(55) As Integer
            If (IsNumeric(dataRowUtama(55)) = False) Then
                result(2) = "riidbs required numeric." : GoTo selesai
            End If
            'riidpo(56) As Integer
            If (IsNumeric(dataRowUtama(56)) = False) Then
                result(2) = "riidpo required numeric." : GoTo selesai
            End If
            'riidipc(57) As Integer
            If (IsNumeric(dataRowUtama(57)) = False) Then
                result(2) = "riidipc required numeric." : GoTo selesai
            End If
            'riidgrn(58) As Integer
            If (IsNumeric(dataRowUtama(58)) = False) Then
                result(2) = "riidgrn required numeric." : GoTo selesai
            End If
            'ristatusdnr(59) As Integer
            If (IsNumeric(dataRowUtama(59)) = False) Then
                result(2) = "ristatusdnr required numeric." : GoTo selesai
            End If
            'ristatusprt(60) As Integer
            If (IsNumeric(dataRowUtama(60)) = False) Then
                result(2) = "ristatusprt required numeric." : GoTo selesai
            End If
            'ristatus(61) As Integer
            If (IsNumeric(dataRowUtama(61)) = False) Then
                result(2) = "ristatus required numeric." : GoTo selesai
            End If
            'ristatussebelumnya(62) As Integer
            If (IsNumeric(dataRowUtama(62)) = False) Then
                result(2) = "ristatussebelumnya required numeric." : GoTo selesai
            End If
            'rijmlrevisi(63) As Integer
            If (IsNumeric(dataRowUtama(63)) = False) Then
                result(2) = "rijmlrevisi required numeric." : GoTo selesai
            End If
            'ricetakanke(64) As Integer
            If (IsNumeric(dataRowUtama(64)) = False) Then
                result(2) = "ricetakanke required numeric." : GoTo selesai
            End If
            'riinputuser(65) As Integer
            If (IsNumeric(dataRowUtama(65)) = False) Then
                result(2) = "riinputuser required numeric." : GoTo selesai
            End If
            'riinputtgl(66) As DateTime
            If (IsDate(dataRowUtama(66)) = False) Then
                result(2) = "riinputtgl required date." : GoTo selesai
            End If
            'rimodifikasiuser(67) As Integer
            If (IsNumeric(dataRowUtama(67)) = False) Then
                result(2) = "rimodifikasiuser required numeric." : GoTo selesai
            End If
            'rimodifikasitgl(68) As DateTime
            If (IsDate(dataRowUtama(68)) = False) Then
                result(2) = "rimodifikasitgl required date." : GoTo selesai
            End If
            'riposting(69) As Integer
            If (IsNumeric(dataRowUtama(69)) = False) Then
                result(2) = "riposting required numeric." : GoTo selesai
            End If
            'ritutupperiode(70) As Integer
            If (IsNumeric(dataRowUtama(70)) = False) Then
                result(2) = "ritutupperiode required numeric." : GoTo selesai
            End If
            'riisclose(71) As Integer
            If (IsNumeric(dataRowUtama(71)) = False) Then
                result(2) = "riisclose required numeric." : GoTo selesai
            End If
            'ricustomint1(77) As Integer
            If (IsNumeric(dataRowUtama(77)) = False) Then
                result(2) = "ricustomint1 required numeric." : GoTo selesai
            End If
            'ricustomint2(78) As Integer
            If (IsNumeric(dataRowUtama(78)) = False) Then
                result(2) = "ricustomint2 required numeric." : GoTo selesai
            End If
            'ricustomint3(79) As Integer
            If (IsNumeric(dataRowUtama(79)) = False) Then
                result(2) = "ricustomint3 required numeric." : GoTo selesai
            End If
            'ricustomdbl1(80) As Double
            If (IsNumeric(dataRowUtama(80)) = False) Then
                result(2) = "ricustomdbl1 required numeric." : GoTo selesai
            End If
            'ricustomdbl2(81) As Double
            If (IsNumeric(dataRowUtama(81)) = False) Then
                result(2) = "ricustomdbl2 required numeric." : GoTo selesai
            End If
            'ricustomdbl3(82) As Double
            If (IsNumeric(dataRowUtama(82)) = False) Then
                result(2) = "ricustomdbl3 required numeric." : GoTo selesai
            End If
            'ricustomdate1(83) As Date
            If (IsDate(dataRowUtama(83)) = False) Then
                result(2) = "ricustomdate1 required date." : GoTo selesai
            End If
            'ricustomdate2(84) As Date
            If (IsDate(dataRowUtama(84)) = False) Then
                result(2) = "ricustomdate2 required date." : GoTo selesai
            End If
            'ricustomdate3(85) As Date
            If (IsDate(dataRowUtama(85)) = False) Then
                result(2) = "ricustomdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'ricabang(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "ricabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "ricabang should not be more than 25 character." : GoTo selesai
            End If

            'rilokasi(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "rilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 25 Then
                result(2) = "rilokasi should not be more than 25 character." : GoTo selesai
            End If

            'rigudang(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "rigudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "rigudang should not be more than 25 character." : GoTo selesai
            End If

            'risumber(9) As String
            If Len(dataRowUtama(9)) = 0 Then
                result(2) = "risumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(9)) > 10 Then
                result(2) = "risumber should not be more than 10 character." : GoTo selesai
            End If

            'rinotransaksi(11) As String
            If Len(dataRowUtama(11)) = 0 Then
                result(2) = "rinotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(11)) > 50 Then
                result(2) = "rinotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'ritgl(12) As Date
            If Len(dataRowUtama(12)) = 0 Then
                result(2) = "ritgl can't be empty" : GoTo selesai
            End If

            'ritgljatuhtempo(24) As Date
            If Len(dataRowUtama(24)) = 0 Then
                result(2) = "ritgljatuhtempo can't be empty" : GoTo selesai
            End If

            'ritglnoref(28) As Date
            If Len(dataRowUtama(28)) = 0 Then
                result(2) = "ritglnoref can't be empty" : GoTo selesai
            End If

            'ritglpenutupan(29) As Date
            If Len(dataRowUtama(29)) = 0 Then
                result(2) = "ritglpenutupan can't be empty" : GoTo selesai
            End If

            'rimatauang(30) As String
            If Len(dataRowUtama(30)) = 0 Then
                result(2) = "rimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(30)) > 25 Then
                result(2) = "rimatauang should not be more than 25 character." : GoTo selesai
            End If

            'rikurs(31) As Double
            If Len(dataRowUtama(31)) = 0 Then
                result(2) = "rikurs can't be empty" : GoTo selesai
            End If

            'ritotal(33) As Double
            If Len(dataRowUtama(33)) = 0 Then
                result(2) = "ritotal can't be empty" : GoTo selesai
            End If

            'ridiskonpersen(34) As String
            If Len(dataRowUtama(34)) = 0 Then
                result(2) = "ridiskonpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(34)) > 25 Then
                result(2) = "ridiskonpersen should not be more than 25 character." : GoTo selesai
            End If

            'rijmldiskon(35) As Double
            If Len(dataRowUtama(35)) = 0 Then
                result(2) = "rijmldiskon can't be empty" : GoTo selesai
            End If

            'ritotalpajak1detail(36) As Double
            If Len(dataRowUtama(36)) = 0 Then
                result(2) = "ritotalpajak1detail can't be empty" : GoTo selesai
            End If

            'ritotalpajak2detail(37) As Double
            If Len(dataRowUtama(37)) = 0 Then
                result(2) = "ritotalpajak2detail can't be empty" : GoTo selesai
            End If

            'ribiayalainpersen(38) As String
            If Len(dataRowUtama(38)) = 0 Then
                result(2) = "ribiayalainpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(38)) > 25 Then
                result(2) = "ribiayalainpersen should not be more than 25 character." : GoTo selesai
            End If

            'ribiayalain(39) As Double
            If Len(dataRowUtama(39)) = 0 Then
                result(2) = "ribiayalain can't be empty" : GoTo selesai
            End If

            'ritotaltransaksi(40) As Double
            If Len(dataRowUtama(40)) = 0 Then
                result(2) = "ritotaltransaksi can't be empty" : GoTo selesai
            End If

            'rijmlbayar(41) As Double
            If Len(dataRowUtama(41)) = 0 Then
                result(2) = "rijmlbayar can't be empty" : GoTo selesai
            End If

            'ritgllunas(43) As Date
            If Len(dataRowUtama(43)) = 0 Then
                result(2) = "ritgllunas can't be empty" : GoTo selesai
            End If

            'ritglbayarpajak(46) As Date
            If Len(dataRowUtama(46)) = 0 Then
                result(2) = "ritglbayarpajak can't be empty" : GoTo selesai
            End If

            'riinputtgl(66) As DateTime
            If Len(dataRowUtama(66)) = 0 Then
                result(2) = "riinputtgl can't be empty" : GoTo selesai
            End If

            'rimodifikasitgl(68) As DateTime
            If Len(dataRowUtama(68)) = 0 Then
                result(2) = "rimodifikasitgl can't be empty" : GoTo selesai
            End If

            'ricustomdbl1(80) As Double
            If Len(dataRowUtama(80)) = 0 Then
                result(2) = "ricustomdbl1 can't be empty" : GoTo selesai
            End If

            'ricustomdbl2(81) As Double
            If Len(dataRowUtama(81)) = 0 Then
                result(2) = "ricustomdbl2 can't be empty" : GoTo selesai
            End If

            'ricustomdbl3(82) As Double
            If Len(dataRowUtama(82)) = 0 Then
                result(2) = "ricustomdbl3 can't be empty" : GoTo selesai
            End If

            'ricustomdate1(83) As Date
            If Len(dataRowUtama(83)) = 0 Then
                result(2) = "ricustomdate1 can't be empty" : GoTo selesai
            End If

            'ricustomdate2(84) As Date
            If Len(dataRowUtama(84)) = 0 Then
                result(2) = "ricustomdate2 can't be empty" : GoTo selesai
            End If

            'ricustomdate3(85) As Date
            If Len(dataRowUtama(85)) = 0 Then
                result(2) = "ricustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA UTAMA ================================================

            If AsDataTableTambahData(dtutama, "riid~ricabang~rilokasi~rigudang~riasalbarang~riasalbarangkategori~rijenispembelian~rijenispembeliankategori~ricarabayar~risumber~riautonotransaksi~rinotransaksi~ritgl~rikodepa~risupplier~risupplierkontak~ri1alamat1~ri1alamat2~ri1alamat3~ri2alamat1~ri2alamat2~ri2alamat3~ribagianpembelian~ritermin~ritgljatuhtempo~riuraian~ricatatan~rinoref~ritglnoref~ritglpenutupan~rimatauang~rikurs~rihargatermasukpajak~ritotal~ridiskonpersen~rijmldiskon~ritotalpajak1detail~ritotalpajak2detail~ribiayalainpersen~ribiayalain~ritotaltransaksi~rijmlbayar~ristatuslunas~ritgllunas~rinofakturpajak~risdhbayarpajak~ritglbayarpajak~rirekdiskon~rirekpajak1~rirekpajak2~rirekbiayalain~rirekbayar~riidpr~riidcs~riidrq~riidbs~riidpo~riidipc~riidgrn~ristatusdnr~ristatusprt~ristatus~ristatussebelumnya~rijmlrevisi~ricetakanke~riinputuser~riinputtgl~rimodifikasiuser~rimodifikasitgl~riposting~ritutupperiode~riisclose~ricustomtext1~ricustomtext2~ricustomtext3~ricustomtext4~ricustomtext5~ricustomint1~ricustomint2~ricustomint3~ricustomdbl1~ricustomdbl2~ricustomdbl3~ricustomdate1~ricustomdate2~ricustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Next


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
                For Each drutama As DataRow In dtutama.Rows

                    'CEK PERIODE AKUNTANSI ==================================
                    Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                    Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ritgl")), AsFormatTanggal(drutama("ritgl")))
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
                        result(4) = drutama("riid")
                        notransaksi = drutama("rinotransaksi")
                        'JIKA UPDATE CEK JML ROW PADA DATABASE
                        dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(riid), rinotransaksi FROM m4_ri WHERE riid='" & result(4) & "' AND ristatus NOT IN(2,3,4,7)", myConn)
                        rowUpdate = dtupdate.Rows(0)(0)

                        If (rowUpdate > 0) Then

                            'CEK NO TRANSAKSI ======================
                            If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                                Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                                Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                                If cekNo > 0 Then
                                    result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                            'END OF CEK NO TRANSAKSI ===============

                            'SIMPAN HISTORY ========================
                            Dim SimpanHistory As New m4_ri_history
                            Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                            If (rsSplitResult(1) = 0) Then
                                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF SIMPAN HISTORY ==================

                            sql = "Update M4_Ri set ricabang  = '" & FixQuotes(drutama("ricabang")) & "', rilokasi  = '" & FixQuotes(drutama("rilokasi")) & "', rigudang  = '" & FixQuotes(drutama("rigudang")) & "', riasalbarang  = '" & FixQuotes(drutama("riasalbarang")) & "', riasalbarangkategori  = " & drutama("riasalbarangkategori") & ", rijenispembelian  = '" & FixQuotes(drutama("rijenispembelian")) & "', rijenispembeliankategori  = " & drutama("rijenispembeliankategori") & ", ricarabayar  = " & drutama("ricarabayar") & ", risumber  = '" & FixQuotes(drutama("risumber")) & "', riautonotransaksi  = " & drutama("riautonotransaksi") & ", rinotransaksi  = '" & FixQuotes(notransaksi) & "', ritgl  = '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', rikodepa  = " & drutama("rikodepa") & ", risupplier  = " & drutama("risupplier") & ", risupplierkontak  = '" & FixQuotes(drutama("risupplierkontak")) & "', ri1alamat1  = '" & FixQuotes(drutama("ri1alamat1")) & "', ri1alamat2  = '" & FixQuotes(drutama("ri1alamat2")) & "', ri1alamat3  = '" & FixQuotes(drutama("ri1alamat3")) & "', ri2alamat1  = '" & FixQuotes(drutama("ri2alamat1")) & "', ri2alamat2  = '" & FixQuotes(drutama("ri2alamat2")) & "', ri2alamat3  = '" & FixQuotes(drutama("ri2alamat3")) & "', ribagianpembelian  = " & drutama("ribagianpembelian") & ", ritermin  = '" & FixQuotes(drutama("ritermin")) & "', ritgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', riuraian  = '" & FixQuotes(drutama("riuraian")) & "', ricatatan  = '" & FixQuotes(drutama("ricatatan")) & "', rinoref  = '" & FixQuotes(drutama("rinoref")) & "', ritglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', ritglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', rimatauang  = '" & FixQuotes(drutama("rimatauang")) & "', rikurs  = '" & FixDouble(drutama("rikurs")) & "', rihargatermasukpajak  = " & drutama("rihargatermasukpajak") & ", ritotal  = '" & FixDouble(drutama("ritotal")) & "', ridiskonpersen  = '" & FixQuotes(drutama("ridiskonpersen")) & "', rijmldiskon  = '" & FixDouble(drutama("rijmldiskon")) & "', ritotalpajak1detail  = '" & FixDouble(drutama("ritotalpajak1detail")) & "', ritotalpajak2detail  = '" & FixDouble(drutama("ritotalpajak2detail")) & "', ribiayalainpersen  = '" & FixQuotes(drutama("ribiayalainpersen")) & "', ribiayalain  = '" & FixDouble(drutama("ribiayalain")) & "', ritotaltransaksi  = '" & FixDouble(drutama("ritotaltransaksi")) & "', rijmlbayar  = '" & FixDouble(drutama("rijmlbayar")) & "', ristatuslunas  = " & drutama("ristatuslunas") & ", ritgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', rinofakturpajak  = '" & FixQuotes(drutama("rinofakturpajak")) & "', risdhbayarpajak  = " & drutama("risdhbayarpajak") & ", ritglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', rirekdiskon  = '" & FixQuotes(drutama("rirekdiskon")) & "', rirekpajak1  = '" & FixQuotes(drutama("rirekpajak1")) & "', rirekpajak2  = '" & FixQuotes(drutama("rirekpajak2")) & "', rirekbiayalain  = '" & FixQuotes(drutama("rirekbiayalain")) & "', rirekbayar  = '" & FixQuotes(drutama("rirekbayar")) & "', riidpr  = " & drutama("riidpr") & ", riidcs  = " & drutama("riidcs") & ", riidrq  = " & drutama("riidrq") & ", riidbs  = " & drutama("riidbs") & ", riidpo  = " & drutama("riidpo") & ", riidipc  = " & drutama("riidipc") & ", riidgrn  = " & drutama("riidgrn") & ", ristatusdnr  = " & drutama("ristatusdnr") & ", ristatusprt  = " & drutama("ristatusprt") & ", ristatus  = " & drutama("ristatus") & ", ristatussebelumnya  = " & drutama("ristatussebelumnya") & ", rijmlrevisi  = rijmlrevisi+1, ricetakanke  = " & drutama("ricetakanke") & ", rimodifikasiuser  = " & drutama("rimodifikasiuser") & ", rimodifikasitgl  = NOW(), riposting  = 0, ritutupperiode  = " & drutama("ritutupperiode") & ", ricustomtext1  = '" & FixQuotes(drutama("ricustomtext1")) & "', ricustomtext2  = '" & FixQuotes(drutama("ricustomtext2")) & "', ricustomtext3  = '" & FixQuotes(drutama("ricustomtext3")) & "', ricustomtext4  = '" & FixQuotes(drutama("ricustomtext4")) & "', ricustomtext5  = '" & FixQuotes(drutama("ricustomtext5")) & "', ricustomint1  = " & drutama("ricustomint1") & ", ricustomint2  = " & drutama("ricustomint2") & ", ricustomint3  = " & drutama("ricustomint3") & ", ricustomdbl1  = '" & FixDouble(drutama("ricustomdbl1")) & "', ricustomdbl2  = '" & FixDouble(drutama("ricustomdbl2")) & "', ricustomdbl3  = '" & FixDouble(drutama("ricustomdbl3")) & "', ricustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', ricustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', ricustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', risaldoawal = 1 where riid = '" & drutama("riid") & "'"
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

                        If drutama("riautonotransaksi") = 1 Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"), drutama("risumber"), 4)
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
                            notransaksi = drutama("rinotransaksi")
                        End If

                        'CEK NO TRANSAKSI ======================
                        Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                        Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                        If cekNo > 0 Then
                            result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Insert into M4_Ri (ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, risaldoawal) values('" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("rigudang")) & "', '" & FixQuotes(drutama("riasalbarang")) & "', " & drutama("riasalbarangkategori") & ", '" & FixQuotes(drutama("rijenispembelian")) & "', " & drutama("rijenispembeliankategori") & ", " & drutama("ricarabayar") & ", '" & FixQuotes(drutama("risumber")) & "', " & drutama("riautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drutama("risupplierkontak")) & "', '" & FixQuotes(drutama("ri1alamat1")) & "', '" & FixQuotes(drutama("ri1alamat2")) & "', '" & FixQuotes(drutama("ri1alamat3")) & "', '" & FixQuotes(drutama("ri2alamat1")) & "', '" & FixQuotes(drutama("ri2alamat2")) & "', '" & FixQuotes(drutama("ri2alamat3")) & "', " & drutama("ribagianpembelian") & ", '" & FixQuotes(drutama("ritermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(drutama("rinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', " & drutama("rihargatermasukpajak") & ", '" & FixDouble(drutama("ritotal")) & "', '" & FixQuotes(drutama("ridiskonpersen")) & "', '" & FixDouble(drutama("rijmldiskon")) & "', '" & FixDouble(drutama("ritotalpajak1detail")) & "', '" & FixDouble(drutama("ritotalpajak2detail")) & "', '" & FixQuotes(drutama("ribiayalainpersen")) & "', '" & FixDouble(drutama("ribiayalain")) & "', '" & FixDouble(drutama("ritotaltransaksi")) & "', '" & FixDouble(drutama("rijmlbayar")) & "', " & drutama("ristatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', '" & FixQuotes(drutama("rinofakturpajak")) & "', " & drutama("risdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', '" & FixQuotes(drutama("rirekdiskon")) & "', '" & FixQuotes(drutama("rirekpajak1")) & "', '" & FixQuotes(drutama("rirekpajak2")) & "', '" & FixQuotes(drutama("rirekbiayalain")) & "', '" & FixQuotes(drutama("rirekbayar")) & "', " & drutama("riidpr") & ", " & drutama("riidcs") & ", " & drutama("riidrq") & ", " & drutama("riidbs") & ", " & drutama("riidpo") & ", " & drutama("riidipc") & ", " & drutama("riidgrn") & ", " & drutama("ristatusdnr") & ", " & drutama("ristatusprt") & ", " & drutama("ristatus") & ", " & drutama("ristatussebelumnya") & ", " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", NOW(), " & drutama("rimodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ritutupperiode") & ", " & drutama("riisclose") & ", '" & FixQuotes(drutama("ricustomtext1")) & "', '" & FixQuotes(drutama("ricustomtext2")) & "', '" & FixQuotes(drutama("ricustomtext3")) & "', '" & FixQuotes(drutama("ricustomtext4")) & "', '" & FixQuotes(drutama("ricustomtext5")) & "', " & drutama("ricustomint1") & ", " & drutama("ricustomint2") & ", " & drutama("ricustomint3") & ", '" & FixDouble(drutama("ricustomdbl1")) & "', '" & FixDouble(drutama("ricustomdbl2")) & "', '" & FixDouble(drutama("ricustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', 1)"
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
                        dt2 = AsDataTableAmbilDariDBCon("select riid from M4_ri where rinotransaksi='" & notransaksi & "' AND riinputuser= '" & userid & "' order by rimodifikasitgl desc limit 1", myConn)
                        If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                    'INSERT MSMQ JURNAL =================================================================
                    Dim sumber As String = "RI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                    If drutama("ristatus") = 2 Then
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
    Public Function M4_RiBUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Ritgl, Rinotransaksi, Ristatus FROM M4_Ri WHERE Riid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Ristatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_ri_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_ri_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0
                Dim updNilaiPO As String = "", updFilterPO As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""
                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idpodetail, idgrndetail, urutan FROM m4_ri_detail WHERE idri = '" & idtransaksi & "'", myConn)



                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'RI' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Ri SET Ristatus = " & nilaiStatus & ", Rimodifikasiuser='" & userid & "', Rimodifikasitgl = NOW(), Riposting = 0, Ripostingtgl = '1971-01-01 00:00:00', Rijmlrevisi = Rijmlrevisi + 1 WHERE Riid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RiBDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
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
            Dim sumber As String = "Ri", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Riid, Rinotransaksi FROM M4_Ri WHERE Riid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT ricabang, rilokasi, risumber, riautonotransaksi, rinotransaksi, ritgl"
            sql &= " FROM M4_ri"
            sql &= " WHERE riid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("ricabang")
                lokasi = dtNomorNext.Rows(0)("rilokasi")
                sumber = dtNomorNext.Rows(0)("risumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("riautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rinotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("ritgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE UTAMA
            sql = "DELETE FROM M4_Ri WHERE riid ='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_RiSearch(PostWsSearch(paramSplit(0), "M4_RiSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_RiUpdateUraian(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

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
        AsDataTableTambahField(dtutama, "riid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ridiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ristatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "risdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate3", AsEnumTypeData.AsString)


        Dim JmlDt As Integer = dataUtama.Length
        For i = 1 To JmlDt
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Utama -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 86) Then
                result(2) = "Invalid main transaction data parameter. " & dataRowUtama.Length & "" : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Utama ----------------------------

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'riid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "riid required numeric." : GoTo selesai
            End If
            'riasalbarangkategori(5) As Integer
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "riasalbarangkategori required numeric." : GoTo selesai
            End If
            'rijenispembeliankategori(7) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "rijenispembeliankategori required numeric." : GoTo selesai
            End If
            'ricarabayar(8) As Integer
            dataRowUtama(8) = 1 'SALDO AWAL HUTANG KREDIT
            If (IsNumeric(dataRowUtama(8)) = False) Then
                result(2) = "ricarabayar required numeric." : GoTo selesai
            End If
            'riautonotransaksi(10) As Integer
            If (IsNumeric(dataRowUtama(10)) = False) Then
                result(2) = "riautonotransaksi required numeric." : GoTo selesai
            End If
            'ritgl(12) As Date
            If (IsDate(dataRowUtama(12)) = False) Then
                result(2) = "ritgl required date." : GoTo selesai
            End If
            'rikodepa(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "rikodepa required numeric." : GoTo selesai
            End If
            'risupplier(14) As Integer
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "risupplier required numeric." : GoTo selesai
            End If
            If (dataRowUtama(14) < 1) Then
                result(2) = "risupplier can't be empty." : GoTo selesai
            End If
            'ribagianpembelian(22) As Integer
            If (IsNumeric(dataRowUtama(22)) = False) Then
                result(2) = "ribagianpembelian required numeric." : GoTo selesai
            End If
            'ritgljatuhtempo(24) As Date
            If (IsDate(dataRowUtama(24)) = False) Then
                result(2) = "ritgljatuhtempo required date." : GoTo selesai
            End If
            'ritglnoref(28) As Date
            If (IsDate(dataRowUtama(28)) = False) Then
                result(2) = "ritglnoref required date." : GoTo selesai
            End If
            'ritglpenutupan(29) As Date
            If (IsDate(dataRowUtama(29)) = False) Then
                result(2) = "ritglpenutupan required date." : GoTo selesai
            End If
            'rikurs(31) As Double
            If (IsNumeric(dataRowUtama(31)) = False) Then
                result(2) = "rikurs required numeric." : GoTo selesai
            End If
            'rihargatermasukpajak(32) As Integer
            If (IsNumeric(dataRowUtama(32)) = False) Then
                result(2) = "rihargatermasukpajak required numeric." : GoTo selesai
            End If
            'ritotal(33) As Double
            If (IsNumeric(dataRowUtama(33)) = False) Then
                result(2) = "ritotal required numeric." : GoTo selesai
            End If
            'rijmldiskon(35) As Double
            If (IsNumeric(dataRowUtama(35)) = False) Then
                result(2) = "rijmldiskon required numeric." : GoTo selesai
            End If
            'ritotalpajak1detail(36) As Double
            If (IsNumeric(dataRowUtama(36)) = False) Then
                result(2) = "ritotalpajak1detail required numeric." : GoTo selesai
            End If
            'ritotalpajak2detail(37) As Double
            If (IsNumeric(dataRowUtama(37)) = False) Then
                result(2) = "ritotalpajak2detail required numeric." : GoTo selesai
            End If
            'ribiayalain(39) As Double
            If (IsNumeric(dataRowUtama(39)) = False) Then
                result(2) = "ribiayalain required numeric." : GoTo selesai
            End If
            'ritotaltransaksi(40) As Double
            If (IsNumeric(dataRowUtama(40)) = False) Then
                result(2) = "ritotaltransaksi required numeric." : GoTo selesai
            End If
            'rijmlbayar(41) As Double
            If (IsNumeric(dataRowUtama(41)) = False) Then
                result(2) = "rijmlbayar required numeric." : GoTo selesai
            End If
            'ristatuslunas(42) As Integer
            If (IsNumeric(dataRowUtama(42)) = False) Then
                result(2) = "ristatuslunas required numeric." : GoTo selesai
            End If
            'ritgllunas(43) As Date
            If (IsDate(dataRowUtama(43)) = False) Then
                result(2) = "ritgllunas required date." : GoTo selesai
            End If
            'risdhbayarpajak(45) As Integer
            If (IsNumeric(dataRowUtama(45)) = False) Then
                result(2) = "risdhbayarpajak required numeric." : GoTo selesai
            End If
            'ritglbayarpajak(46) As Date
            If (IsDate(dataRowUtama(46)) = False) Then
                result(2) = "ritglbayarpajak required date." : GoTo selesai
            End If
            'riidpr(52) As Integer
            If (IsNumeric(dataRowUtama(52)) = False) Then
                result(2) = "riidpr required numeric." : GoTo selesai
            End If
            'riidcs(53) As Integer
            If (IsNumeric(dataRowUtama(53)) = False) Then
                result(2) = "riidcs required numeric." : GoTo selesai
            End If
            'riidrq(54) As Integer
            If (IsNumeric(dataRowUtama(54)) = False) Then
                result(2) = "riidrq required numeric." : GoTo selesai
            End If
            'riidbs(55) As Integer
            If (IsNumeric(dataRowUtama(55)) = False) Then
                result(2) = "riidbs required numeric." : GoTo selesai
            End If
            'riidpo(56) As Integer
            If (IsNumeric(dataRowUtama(56)) = False) Then
                result(2) = "riidpo required numeric." : GoTo selesai
            End If
            'riidipc(57) As Integer
            If (IsNumeric(dataRowUtama(57)) = False) Then
                result(2) = "riidipc required numeric." : GoTo selesai
            End If
            'riidgrn(58) As Integer
            If (IsNumeric(dataRowUtama(58)) = False) Then
                result(2) = "riidgrn required numeric." : GoTo selesai
            End If
            'ristatusdnr(59) As Integer
            If (IsNumeric(dataRowUtama(59)) = False) Then
                result(2) = "ristatusdnr required numeric." : GoTo selesai
            End If
            'ristatusprt(60) As Integer
            If (IsNumeric(dataRowUtama(60)) = False) Then
                result(2) = "ristatusprt required numeric." : GoTo selesai
            End If
            'ristatus(61) As Integer
            If (IsNumeric(dataRowUtama(61)) = False) Then
                result(2) = "ristatus required numeric." : GoTo selesai
            End If
            'ristatussebelumnya(62) As Integer
            If (IsNumeric(dataRowUtama(62)) = False) Then
                result(2) = "ristatussebelumnya required numeric." : GoTo selesai
            End If
            'rijmlrevisi(63) As Integer
            If (IsNumeric(dataRowUtama(63)) = False) Then
                result(2) = "rijmlrevisi required numeric." : GoTo selesai
            End If
            'ricetakanke(64) As Integer
            If (IsNumeric(dataRowUtama(64)) = False) Then
                result(2) = "ricetakanke required numeric." : GoTo selesai
            End If
            'riinputuser(65) As Integer
            If (IsNumeric(dataRowUtama(65)) = False) Then
                result(2) = "riinputuser required numeric." : GoTo selesai
            End If
            'riinputtgl(66) As DateTime
            If (IsDate(dataRowUtama(66)) = False) Then
                result(2) = "riinputtgl required date." : GoTo selesai
            End If
            'rimodifikasiuser(67) As Integer
            If (IsNumeric(dataRowUtama(67)) = False) Then
                result(2) = "rimodifikasiuser required numeric." : GoTo selesai
            End If
            'rimodifikasitgl(68) As DateTime
            If (IsDate(dataRowUtama(68)) = False) Then
                result(2) = "rimodifikasitgl required date." : GoTo selesai
            End If
            'riposting(69) As Integer
            If (IsNumeric(dataRowUtama(69)) = False) Then
                result(2) = "riposting required numeric." : GoTo selesai
            End If
            'ritutupperiode(70) As Integer
            If (IsNumeric(dataRowUtama(70)) = False) Then
                result(2) = "ritutupperiode required numeric." : GoTo selesai
            End If
            'riisclose(71) As Integer
            If (IsNumeric(dataRowUtama(71)) = False) Then
                result(2) = "riisclose required numeric." : GoTo selesai
            End If
            'ricustomint1(77) As Integer
            If (IsNumeric(dataRowUtama(77)) = False) Then
                result(2) = "ricustomint1 required numeric." : GoTo selesai
            End If
            'ricustomint2(78) As Integer
            If (IsNumeric(dataRowUtama(78)) = False) Then
                result(2) = "ricustomint2 required numeric." : GoTo selesai
            End If
            'ricustomint3(79) As Integer
            If (IsNumeric(dataRowUtama(79)) = False) Then
                result(2) = "ricustomint3 required numeric." : GoTo selesai
            End If
            'ricustomdbl1(80) As Double
            If (IsNumeric(dataRowUtama(80)) = False) Then
                result(2) = "ricustomdbl1 required numeric." : GoTo selesai
            End If
            'ricustomdbl2(81) As Double
            If (IsNumeric(dataRowUtama(81)) = False) Then
                result(2) = "ricustomdbl2 required numeric." : GoTo selesai
            End If
            'ricustomdbl3(82) As Double
            If (IsNumeric(dataRowUtama(82)) = False) Then
                result(2) = "ricustomdbl3 required numeric." : GoTo selesai
            End If
            'ricustomdate1(83) As Date
            If (IsDate(dataRowUtama(83)) = False) Then
                result(2) = "ricustomdate1 required date." : GoTo selesai
            End If
            'ricustomdate2(84) As Date
            If (IsDate(dataRowUtama(84)) = False) Then
                result(2) = "ricustomdate2 required date." : GoTo selesai
            End If
            'ricustomdate3(85) As Date
            If (IsDate(dataRowUtama(85)) = False) Then
                result(2) = "ricustomdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'ricabang(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "ricabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "ricabang should not be more than 25 character." : GoTo selesai
            End If

            'rilokasi(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "rilokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 25 Then
                result(2) = "rilokasi should not be more than 25 character." : GoTo selesai
            End If

            'rigudang(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "rigudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "rigudang should not be more than 25 character." : GoTo selesai
            End If

            'risumber(9) As String
            If Len(dataRowUtama(9)) = 0 Then
                result(2) = "risumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(9)) > 10 Then
                result(2) = "risumber should not be more than 10 character." : GoTo selesai
            End If

            'rinotransaksi(11) As String
            If Len(dataRowUtama(11)) = 0 Then
                result(2) = "rinotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(11)) > 50 Then
                result(2) = "rinotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'ritgl(12) As Date
            If Len(dataRowUtama(12)) = 0 Then
                result(2) = "ritgl can't be empty" : GoTo selesai
            End If

            'ritgljatuhtempo(24) As Date
            If Len(dataRowUtama(24)) = 0 Then
                result(2) = "ritgljatuhtempo can't be empty" : GoTo selesai
            End If

            'ritglnoref(28) As Date
            If Len(dataRowUtama(28)) = 0 Then
                result(2) = "ritglnoref can't be empty" : GoTo selesai
            End If

            'ritglpenutupan(29) As Date
            If Len(dataRowUtama(29)) = 0 Then
                result(2) = "ritglpenutupan can't be empty" : GoTo selesai
            End If

            'rimatauang(30) As String
            If Len(dataRowUtama(30)) = 0 Then
                result(2) = "rimatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(30)) > 25 Then
                result(2) = "rimatauang should not be more than 25 character." : GoTo selesai
            End If

            'rikurs(31) As Double
            If Len(dataRowUtama(31)) = 0 Then
                result(2) = "rikurs can't be empty" : GoTo selesai
            End If

            'ritotal(33) As Double
            If Len(dataRowUtama(33)) = 0 Then
                result(2) = "ritotal can't be empty" : GoTo selesai
            End If

            'ridiskonpersen(34) As String
            If Len(dataRowUtama(34)) = 0 Then
                result(2) = "ridiskonpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(34)) > 25 Then
                result(2) = "ridiskonpersen should not be more than 25 character." : GoTo selesai
            End If

            'rijmldiskon(35) As Double
            If Len(dataRowUtama(35)) = 0 Then
                result(2) = "rijmldiskon can't be empty" : GoTo selesai
            End If

            'ritotalpajak1detail(36) As Double
            If Len(dataRowUtama(36)) = 0 Then
                result(2) = "ritotalpajak1detail can't be empty" : GoTo selesai
            End If

            'ritotalpajak2detail(37) As Double
            If Len(dataRowUtama(37)) = 0 Then
                result(2) = "ritotalpajak2detail can't be empty" : GoTo selesai
            End If

            'ribiayalainpersen(38) As String
            If Len(dataRowUtama(38)) = 0 Then
                result(2) = "ribiayalainpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(38)) > 25 Then
                result(2) = "ribiayalainpersen should not be more than 25 character." : GoTo selesai
            End If

            'ribiayalain(39) As Double
            If Len(dataRowUtama(39)) = 0 Then
                result(2) = "ribiayalain can't be empty" : GoTo selesai
            End If

            'ritotaltransaksi(40) As Double
            If Len(dataRowUtama(40)) = 0 Then
                result(2) = "ritotaltransaksi can't be empty" : GoTo selesai
            End If

            'rijmlbayar(41) As Double
            If Len(dataRowUtama(41)) = 0 Then
                result(2) = "rijmlbayar can't be empty" : GoTo selesai
            End If

            'ritgllunas(43) As Date
            If Len(dataRowUtama(43)) = 0 Then
                result(2) = "ritgllunas can't be empty" : GoTo selesai
            End If

            'ritglbayarpajak(46) As Date
            If Len(dataRowUtama(46)) = 0 Then
                result(2) = "ritglbayarpajak can't be empty" : GoTo selesai
            End If

            'riinputtgl(66) As DateTime
            If Len(dataRowUtama(66)) = 0 Then
                result(2) = "riinputtgl can't be empty" : GoTo selesai
            End If

            'rimodifikasitgl(68) As DateTime
            If Len(dataRowUtama(68)) = 0 Then
                result(2) = "rimodifikasitgl can't be empty" : GoTo selesai
            End If

            'ricustomdbl1(80) As Double
            If Len(dataRowUtama(80)) = 0 Then
                result(2) = "ricustomdbl1 can't be empty" : GoTo selesai
            End If

            'ricustomdbl2(81) As Double
            If Len(dataRowUtama(81)) = 0 Then
                result(2) = "ricustomdbl2 can't be empty" : GoTo selesai
            End If

            'ricustomdbl3(82) As Double
            If Len(dataRowUtama(82)) = 0 Then
                result(2) = "ricustomdbl3 can't be empty" : GoTo selesai
            End If

            'ricustomdate1(83) As Date
            If Len(dataRowUtama(83)) = 0 Then
                result(2) = "ricustomdate1 can't be empty" : GoTo selesai
            End If

            'ricustomdate2(84) As Date
            If Len(dataRowUtama(84)) = 0 Then
                result(2) = "ricustomdate2 can't be empty" : GoTo selesai
            End If

            'ricustomdate3(85) As Date
            If Len(dataRowUtama(85)) = 0 Then
                result(2) = "ricustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA UTAMA ================================================

            If AsDataTableTambahData(dtutama, "riid~ricabang~rilokasi~rigudang~riasalbarang~riasalbarangkategori~rijenispembelian~rijenispembeliankategori~ricarabayar~risumber~riautonotransaksi~rinotransaksi~ritgl~rikodepa~risupplier~risupplierkontak~ri1alamat1~ri1alamat2~ri1alamat3~ri2alamat1~ri2alamat2~ri2alamat3~ribagianpembelian~ritermin~ritgljatuhtempo~riuraian~ricatatan~rinoref~ritglnoref~ritglpenutupan~rimatauang~rikurs~rihargatermasukpajak~ritotal~ridiskonpersen~rijmldiskon~ritotalpajak1detail~ritotalpajak2detail~ribiayalainpersen~ribiayalain~ritotaltransaksi~rijmlbayar~ristatuslunas~ritgllunas~rinofakturpajak~risdhbayarpajak~ritglbayarpajak~rirekdiskon~rirekpajak1~rirekpajak2~rirekbiayalain~rirekbayar~riidpr~riidcs~riidrq~riidbs~riidpo~riidipc~riidgrn~ristatusdnr~ristatusprt~ristatus~ristatussebelumnya~rijmlrevisi~ricetakanke~riinputuser~riinputtgl~rimodifikasiuser~rimodifikasitgl~riposting~ritutupperiode~riisclose~ricustomtext1~ricustomtext2~ricustomtext3~ricustomtext4~ricustomtext5~ricustomint1~ricustomint2~ricustomint3~ricustomdbl1~ricustomdbl2~ricustomdbl3~ricustomdate1~ricustomdate2~ricustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Next


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
                For Each drutama As DataRow In dtutama.Rows

                    'CEK PERIODE AKUNTANSI ==================================
                    Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                    Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ritgl")), AsFormatTanggal(drutama("ritgl")))
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
                        result(4) = drutama("riid")
                        notransaksi = drutama("rinotransaksi")
                        'JIKA UPDATE CEK JML ROW PADA DATABASE
                        dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(riid), rinotransaksi FROM m4_ri WHERE riid='" & result(4) & "'", myConn)
                        rowUpdate = dtupdate.Rows(0)(0)

                        If (rowUpdate > 0) Then


                            'SIMPAN HISTORY ========================
                            Dim SimpanHistory As New m4_ri_history
                            Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                            If (rsSplitResult(1) = 0) Then
                                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF SIMPAN HISTORY ==================

                            sql = "Update M4_Ri set riuraian  = '" & FixQuotes(drutama("riuraian")) & "', rinofakturpajak = '" & FixQuotes(drutama("rinofakturpajak")) & "', rinoref = '" & FixQuotes(drutama("rinoref")) & "', ricustomtext1 = '" & FixQuotes(drutama("ricustomtext1")) & "' where riid =  '" & drutama("riid") & "'"
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

                    End If


                    'INSERT MSMQ JURNAL =================================================================
                    Dim sumber As String = "RI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

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
    Public Function M4_RiBSearch(ByVal param As String) As String
        'M4_RiBSearch --------------------------------------------------------
        'riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, 
        'rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, 
        'risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, 
        'ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, 
        'ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, 
        'rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, 
        'ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, 
        'rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, 
        'riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, 
        'ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, 
        'riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, 
        'risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, 
        'ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, 
        'ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1,
        'ricustomdate2, ricustomdate3

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
            Filter = Filter.Replace("risupplierkode", "c1.kkode")
            Filter = Filter.Replace("risuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `ri`.`riid` AS `riid`,`ri`.`ricabang` AS `ricabang`,`ri`.`rilokasi` AS `rilokasi`,`ri`.`rigudang` AS `rigudang`,`ri`.`riasalbarang` AS `riasalbarang`,`ri`.`riasalbarangkategori` AS `riasalbarangkategori`,`ri`.`rijenispembelian` AS `rijenispembelian`,`ri`.`rijenispembeliankategori` AS `rijenispembeliankategori`,`ri`.`ricarabayar` AS `ricarabayar`,`ri`.`risumber` AS `risumber`,`ri`.`riautonotransaksi` AS `riautonotransaksi`,`ri`.`rinotransaksi` AS `rinotransaksi`,`ri`.`ritgl` AS `ritgl`,`ri`.`rikodepa` AS `rikodepa`,`ri`.`risupplier` AS `risupplier`,`ri`.`risupplierkontak` AS `risupplierkontak`,`ri`.`ri1alamat1` AS `ri1alamat1`,`ri`.`ri1alamat2` AS `ri1alamat2`,`ri`.`ri1alamat3` AS `ri1alamat3`,`ri`.`ri2alamat1` AS `ri2alamat1`,`ri`.`ri2alamat2` AS `ri2alamat2`,`ri`.`ri2alamat3` AS `ri2alamat3`,`ri`.`ribagianpembelian` AS `ribagianpembelian`,`ri`.`ritermin` AS `ritermin`,`ri`.`ritgljatuhtempo` AS `ritgljatuhtempo`,`ri`.`riuraian` AS `riuraian`,`ri`.`ricatatan` AS `ricatatan`,`ri`.`rinoref` AS `rinoref`,`ri`.`ritglnoref` AS `ritglnoref`,`ri`.`ritglpenutupan` AS `ritglpenutupan`,`ri`.`rimatauang` AS `rimatauang`,`ri`.`rikurs` AS `rikurs`,`ri`.`rihargatermasukpajak` AS `rihargatermasukpajak`,`ri`.`ritotal` AS `ritotal`,`ri`.`ridiskonpersen` AS `ridiskonpersen`,`ri`.`rijmldiskon` AS `rijmldiskon`,`ri`.`ritotalpajak1detail` AS `ritotalpajak1detail`,`ri`.`ritotalpajak2detail` AS `ritotalpajak2detail`,`ri`.`ribiayalainpersen` AS `ribiayalainpersen`,`ri`.`ribiayalain` AS `ribiayalain`,`ri`.`ritotaltransaksi` AS `ritotaltransaksi`,`ri`.`rijmlbayar` AS `rijmlbayar`,`ri`.`ristatuslunas` AS `ristatuslunas`,`ri`.`ritgllunas` AS `ritgllunas`,`ri`.`rinofakturpajak` AS `rinofakturpajak`,`ri`.`risdhbayarpajak` AS `risdhbayarpajak`,`ri`.`ritglbayarpajak` AS `ritglbayarpajak`,`ri`.`rirekdiskon` AS `rirekdiskon`,`ri`.`rirekpajak1` AS `rirekpajak1`,`ri`.`rirekpajak2` AS `rirekpajak2`,`ri`.`rirekbiayalain` AS `rirekbiayalain`,`ri`.`rirekbayar` AS `rirekbayar`,`ri`.`riidpr` AS `riidpr`,`ri`.`riidcs` AS `riidcs`,`ri`.`riidrq` AS `riidrq`,`ri`.`riidbs` AS `riidbs`,`ri`.`riidpo` AS `riidpo`,`ri`.`riidipc` AS `riidipc`,`ri`.`riidgrn` AS `riidgrn`,`ri`.`ristatusdnr` AS `ristatusdnr`,`ri`.`ristatusprt` AS `ristatusprt`,`ri`.`ristatusrealisasi` AS `ristatusrealisasi`,`ri`.`ristatus` AS `ristatus`,`ri`.`ristatussebelumnya` AS `ristatussebelumnya`,`ri`.`rijmlrevisi` AS `rijmlrevisi`,`ri`.`ricetakanke` AS `ricetakanke`,`ri`.`riinputuser` AS `riinputuser`,`ri`.`riinputtgl` AS `riinputtgl`,`ri`.`rimodifikasiuser` AS `rimodifikasiuser`,`ri`.`rimodifikasitgl` AS `rimodifikasitgl`,`ri`.`riposting` AS `riposting`,`ri`.`ripostingtgl` AS `ripostingtgl`,`ri`.`ritutupperiode` AS `ritutupperiode`,`ri`.`riisclose` AS `riisclose`,`br`.`bnama` AS `ricabangnama`,`lc`.`lnama` AS `rilokasinama`,`wh`.`wnama` AS `rigudangnama`,`c1`.`kkode` AS `risupplierkode`,`c1`.`knama` AS `risuppliernama`,`c2`.`kkode` AS `ribagianpembeliankode`,`c2`.`knama` AS `ribagianpembeliannama`,`po`.`ponotransaksi` AS `ponotransaksi`,`ipc`.`ipcnotransaksi` AS `ipcnotransaksi`,`grn`.`grnnotransaksi` AS `grnnotransaksi`,`st1`.`nama` AS `ristatusnama`,`st2`.`nama` AS `ristatussebelumnyanama`,`u1`.`unama` AS `riinputusernama`,`u2`.`unama` AS `rimodifikasiusernama`, `ri`.`ricustomtext1` AS `ricustomtext1`, `ri`.`ricustomtext2` AS `ricustomtext2`, `ri`.`ricustomtext3` AS `ricustomtext3`, `ri`.`ricustomtext4` AS `ricustomtext4`, `ri`.`ricustomtext5` AS `ricustomtext5`, `ri`.`ricustomint1` AS `ricustomint1`, `ri`.`ricustomint2` AS `ricustomint2`, `ri`.`ricustomint3` AS `ricustomint3`, `ri`.`ricustomdbl1` AS `ricustomdbl1`, `ri`.`ricustomdbl2` AS `ricustomdbl2`, `ri`.`ricustomdbl3` AS `ricustomdbl3`, `ri`.`ricustomdate1` AS `ricustomdate1`, `ri`.`ricustomdate1` AS `ricustomdate1`, `ri`.`ricustomdate2` AS `ricustomdate2`, `ri`.`ricustomdate3` AS `ricustomdate3`, cdis.cnama AS rirekdiskonnama, cpa.cnama AS rirekpajak1nama, cpa2.cnama AS rirekpajak2nama, cba.cnama AS rirekbiayalainnama from ((((((((((((`m4_ri` `ri` left join `m1_branch` `br` on((`br`.`bkode` = `ri`.`ricabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `ri`.`rilokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `ri`.`rigudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `ri`.`risupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `ri`.`ribagianpembelian`))) left join `m4_po` `po` on((`ri`.`riidpo` = `po`.`poid`))) left join `m4_ipc` `ipc` on((`ri`.`riidipc` = `ipc`.`ipcid`))) left join `m4_grn` `grn` on((`ri`.`riidgrn` = `grn`.`grnid`))) left join `m0_status` `st1` on((`st1`.`kode` = `ri`.`ristatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `ri`.`ristatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `ri`.`riinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `ri`.`rimodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = ri.rirekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = ri.rirekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = ri.rirekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = ri.rirekbiayalain"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Ri", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("ricabang"), ""), sptField,
                     FxDB(dr("rilokasi"), ""), sptField,
                     FxDB(dr("rigudang"), ""), sptField,
                     FxDB(dr("riasalbarang"), ""), sptField,
                     FxDB(dr("riasalbarangkategori"), 0), sptField,
                     FxDB(dr("rijenispembelian"), ""), sptField,
                     FxDB(dr("rijenispembeliankategori"), 0), sptField,
                     FxDB(dr("ricarabayar"), 0), sptField,
                     FxDB(dr("risumber"), ""), sptField,
                     FxDB(dr("riautonotransaksi"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     FxDB(dr("rikodepa"), 0), sptField,
                     FxDB(dr("risupplier"), 0), sptField,
                     FxDB(dr("risupplierkontak"), ""), sptField,
                     FxDB(dr("ri1alamat1"), ""), sptField,
                     FxDB(dr("ri1alamat2"), ""), sptField,
                     FxDB(dr("ri1alamat3"), ""), sptField,
                     FxDB(dr("ri2alamat1"), ""), sptField,
                     FxDB(dr("ri2alamat2"), ""), sptField,
                     FxDB(dr("ri2alamat3"), ""), sptField,
                     FxDB(dr("ribagianpembelian"), 0), sptField,
                     FxDB(dr("ritermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("riuraian"), ""), sptField,
                     FxDB(dr("ricatatan"), ""), sptField,
                     FxDB(dr("rinoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ritglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rimatauang"), ""), sptField,
                     FxDB(dr("rikurs"), 0), sptField,
                     FxDB(dr("rihargatermasukpajak"), 0), sptField,
                     FxDB(dr("ritotal"), 0), sptField,
                     FxDB(dr("ridiskonpersen"), ""), sptField,
                     FxDB(dr("rijmldiskon"), 0), sptField,
                     FxDB(dr("ritotalpajak1detail"), 0), sptField,
                     FxDB(dr("ritotalpajak2detail"), 0), sptField,
                     FxDB(dr("ribiayalainpersen"), ""), sptField,
                     FxDB(dr("ribiayalain"), 0), sptField,
                     FxDB(dr("ritotaltransaksi"), 0), sptField,
                     FxDB(dr("rijmlbayar"), 0), sptField,
                     FxDB(dr("ristatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rinofakturpajak"), ""), sptField,
                     FxDB(dr("risdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ritglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rirekdiskon"), ""), sptField,
                     FxDB(dr("rirekpajak1"), ""), sptField,
                     FxDB(dr("rirekpajak2"), ""), sptField,
                     FxDB(dr("rirekbiayalain"), ""), sptField,
                     FxDB(dr("rirekbayar"), ""), sptField,
                     FxDB(dr("riidpr"), 0), sptField,
                     FxDB(dr("riidcs"), 0), sptField,
                     FxDB(dr("riidrq"), 0), sptField,
                     FxDB(dr("riidbs"), 0), sptField,
                     FxDB(dr("riidpo"), 0), sptField,
                     FxDB(dr("riidipc"), 0), sptField,
                     FxDB(dr("riidgrn"), 0), sptField,
                     FxDB(dr("ristatusdnr"), 0), sptField,
                     FxDB(dr("ristatusprt"), 0), sptField,
                     FxDB(dr("ristatusrealisasi"), 0), sptField,
                     FxDB(dr("ristatus"), 0), sptField,
                     FxDB(dr("ristatussebelumnya"), 0), sptField,
                     FxDB(dr("rijmlrevisi"), 0), sptField,
                     FxDB(dr("ricetakanke"), 0), sptField,
                     FxDB(dr("riinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("riinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rimodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rimodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("riposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ripostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("ritutupperiode"), 0), sptField,
                     FxDB(dr("riisclose"), 0), sptField,
                     FxDB(dr("ricabangnama"), ""), sptField,
                     FxDB(dr("rilokasinama"), ""), sptField,
                     FxDB(dr("rigudangnama"), ""), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("ribagianpembeliankode"), ""), sptField,
                     FxDB(dr("ribagianpembeliannama"), ""), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     FxDB(dr("ipcnotransaksi"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("ristatusnama"), ""), sptField,
                     FxDB(dr("ristatussebelumnyanama"), ""), sptField,
                     FxDB(dr("riinputusernama"), ""), sptField,
                     FxDB(dr("rimodifikasiusernama"), ""), sptField,
                     FxDB(dr("ricustomtext1"), ""), sptField,
                     FxDB(dr("ricustomtext2"), ""), sptField,
                     FxDB(dr("ricustomtext3"), ""), sptField,
                     FxDB(dr("ricustomtext4"), ""), sptField,
                     FxDB(dr("ricustomtext5"), ""), sptField,
                     FxDB(dr("ricustomint1"), 0), sptField,
                     FxDB(dr("ricustomint2"), 0), sptField,
                     FxDB(dr("ricustomint3"), 0), sptField,
                     FxDB(dr("ricustomdbl1"), 0), sptField,
                     FxDB(dr("ricustomdbl2"), 0), sptField,
                     FxDB(dr("ricustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate1"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate2"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(dr("ricustomdate3"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rirekdiskonnama"), ""), sptField,
                     FxDB(dr("rirekpajak1nama"), ""), sptField,
                     FxDB(dr("rirekpajak2nama"), ""), sptField,
                     FxDB(dr("rirekbiayalainnama"), ""), sptRow)

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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("riid, ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatusrealisasi, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ripostingtgl, ritutupperiode, riisclose, ricabangnama, rilokasinama, rigudangnama, risupplierkode, risuppliernama, ribagianpembeliankode, ribagianpembeliannama, ponotransaksi, ipcnotransaksi, grnnotransaksi, ristatusnama, ristatussebelumnyanama, riinputusernama, rimodifikasiusernama, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, rirekdiskonnama, rirekpajak1nama, rirekpajak2nama, rirekbiayalainnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_RiImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim dtMasterTemp As New DataTable
        Dim strValMaster As String = ""

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", tglLunas As String = ""
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
        'ricustomdate3(85) As Date, rijmluangmuka(86) As Double, rirekuangmuka(87) As String, riidap(88) As Integer

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
        'ricustomdate2, ricustomdate3, rijmluangmuka, rirekuangmuka, riidap


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 89) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'riid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "riid required numeric." : GoTo selesai
        End If
        'riasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "riasalbarangkategori required numeric." : GoTo selesai
        End If
        'rijenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rijenispembeliankategori required numeric." : GoTo selesai
        End If
        'ricarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "ricarabayar required numeric." : GoTo selesai
        End If
        'riautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "riautonotransaksi required numeric." : GoTo selesai
        End If
        'ritgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "ritgl required date." : GoTo selesai
        End If
        'rikodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rikodepa required numeric." : GoTo selesai
        End If
        'risupplier(14) As Integer
        'If (IsNumeric(dataUtama(14)) = False) Then
        '    result(2) = "risupplier required numeric." : GoTo selesai
        'End If
        'If (dataUtama(14) < 1) Then
        '    result(2) = "risupplier can't be empty." : GoTo selesai
        'End If
        'ribagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "ribagianpembelian required numeric." : GoTo selesai
        End If
        'ritgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "ritgljatuhtempo required date." : GoTo selesai
        End If
        'ritglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "ritglnoref required date." : GoTo selesai
        End If
        'ritglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "ritglpenutupan required date." : GoTo selesai
        End If
        'rikurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "rikurs required numeric." : GoTo selesai
        End If
        'rihargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "rihargatermasukpajak required numeric." : GoTo selesai
        End If
        'ritotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "ritotal required numeric." : GoTo selesai
        End If
        'rijmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rijmldiskon required numeric." : GoTo selesai
        End If
        'ritotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "ritotalpajak1detail required numeric." : GoTo selesai
        End If
        'ritotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "ritotalpajak2detail required numeric." : GoTo selesai
        End If
        'ribiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "ribiayalain required numeric." : GoTo selesai
        End If
        'ritotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "ritotaltransaksi required numeric." : GoTo selesai
        End If
        'rijmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rijmlbayar required numeric." : GoTo selesai
        End If
        'ristatuslunas(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "ristatuslunas required numeric." : GoTo selesai
        End If
        'ritgllunas(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "ritgllunas required date." : GoTo selesai
        End If
        'risdhbayarpajak(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "risdhbayarpajak required numeric." : GoTo selesai
        End If
        'ritglbayarpajak(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "ritglbayarpajak required date." : GoTo selesai
        End If
        'riidpr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "riidpr required numeric." : GoTo selesai
        End If
        'riidcs(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "riidcs required numeric." : GoTo selesai
        End If
        'riidrq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "riidrq required numeric." : GoTo selesai
        End If
        'riidbs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "riidbs required numeric." : GoTo selesai
        End If
        'riidpo(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "riidpo required numeric." : GoTo selesai
        End If
        'riidipc(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "riidipc required numeric." : GoTo selesai
        End If
        'riidgrn(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "riidgrn required numeric." : GoTo selesai
        End If
        'ristatusdnr(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "ristatusdnr required numeric." : GoTo selesai
        End If
        'ristatusprt(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "ristatusprt required numeric." : GoTo selesai
        End If
        'ristatus(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "ristatus required numeric." : GoTo selesai
        End If
        'ristatussebelumnya(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "ristatussebelumnya required numeric." : GoTo selesai
        End If
        'rijmlrevisi(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rijmlrevisi required numeric." : GoTo selesai
        End If
        'ricetakanke(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "ricetakanke required numeric." : GoTo selesai
        End If
        'riinputuser(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "riinputuser required numeric." : GoTo selesai
        End If
        'riinputtgl(66) As DateTime
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "riinputtgl required date." : GoTo selesai
        End If
        'rimodifikasiuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "rimodifikasiuser required numeric." : GoTo selesai
        End If
        'rimodifikasitgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "rimodifikasitgl required date." : GoTo selesai
        End If
        'riposting(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "riposting required numeric." : GoTo selesai
        End If
        'ritutupperiode(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "ritutupperiode required numeric." : GoTo selesai
        End If
        'riisclose(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "riisclose required numeric." : GoTo selesai
        End If
        'ricustomint1(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "ricustomint1 required numeric." : GoTo selesai
        End If
        'ricustomint2(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "ricustomint2 required numeric." : GoTo selesai
        End If
        'ricustomint3(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "ricustomint3 required numeric." : GoTo selesai
        End If
        'ricustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "ricustomdbl1 required numeric." : GoTo selesai
        End If
        'ricustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "ricustomdbl2 required numeric." : GoTo selesai
        End If
        'ricustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "ricustomdbl3 required numeric." : GoTo selesai
        End If
        'ricustomdate1(83) As Date
        If (IsDate(dataUtama(83)) = False) Then
            result(2) = "ricustomdate1 required date." : GoTo selesai
        End If
        'ricustomdate2(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "ricustomdate2 required date." : GoTo selesai
        End If
        'ricustomdate3(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "ricustomdate3 required date." : GoTo selesai
        End If
        'rijmluangmuka(86) As Double
        If (IsNumeric(dataUtama(86)) = False) Then
            result(2) = "rijmluangmuka required numeric." : GoTo selesai
        End If
        'riidap(88) As Integer
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "riidap required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'ricabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "ricabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "ricabang should not be more than 25 character." : GoTo selesai
        End If

        'rilokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rilokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rilokasi should not be more than 25 character." : GoTo selesai
        End If

        'rigudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rigudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rigudang should not be more than 25 character." : GoTo selesai
        End If

        'risumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "risumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "risumber should not be more than 10 character." : GoTo selesai
        End If

        'rinotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rinotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "rinotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'ritgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "ritgl can't be empty" : GoTo selesai
        End If
        'SET TGLTRANSAKSI ---> UNTUK UPDATE TGL LUNAS UM
        tglLunas = AsFormatTanggal(dataUtama(12))

        'risupplier(14)
        If Len(dataUtama(14)) = 0 Then
            result(2) = "risupplier can't be empty" : GoTo selesai
        End If

        'ritgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "ritgljatuhtempo can't be empty" : GoTo selesai
        End If

        'ritglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "ritglnoref can't be empty" : GoTo selesai
        End If

        'ritglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "ritglpenutupan can't be empty" : GoTo selesai
        End If

        'rimatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rimatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "rimatauang should not be more than 25 character." : GoTo selesai
        End If

        'rikurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rikurs can't be empty" : GoTo selesai
        End If

        'ritotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "ritotal can't be empty" : GoTo selesai
        End If

        'ridiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "ridiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "ridiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rijmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rijmldiskon can't be empty" : GoTo selesai
        End If

        'ritotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "ritotalpajak1detail can't be empty" : GoTo selesai
        End If

        'ritotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "ritotalpajak2detail can't be empty" : GoTo selesai
        End If

        'ribiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "ribiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "ribiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'ribiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "ribiayalain can't be empty" : GoTo selesai
        End If

        'ritotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "ritotaltransaksi can't be empty" : GoTo selesai
        End If

        'rijmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rijmlbayar can't be empty" : GoTo selesai
        End If

        'ritgllunas(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "ritgllunas can't be empty" : GoTo selesai
        End If

        'ritglbayarpajak(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "ritglbayarpajak can't be empty" : GoTo selesai
        End If

        'riinputtgl(66) As DateTime
        If Len(dataUtama(66)) = 0 Then
            result(2) = "riinputtgl can't be empty" : GoTo selesai
        End If

        'rimodifikasitgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "rimodifikasitgl can't be empty" : GoTo selesai
        End If

        'ricustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "ricustomdbl1 can't be empty" : GoTo selesai
        End If

        'ricustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "ricustomdbl2 can't be empty" : GoTo selesai
        End If

        'ricustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "ricustomdbl3 can't be empty" : GoTo selesai
        End If

        'ricustomdate1(83) As Date
        If Len(dataUtama(83)) = 0 Then
            result(2) = "ricustomdate1 can't be empty" : GoTo selesai
        End If

        'ricustomdate2(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "ricustomdate2 can't be empty" : GoTo selesai
        End If

        'ricustomdate3(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "ricustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "riid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rilokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rigudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rinotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "risupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ri2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rikurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rihargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ridiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ribiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ritotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ristatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rinofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "risdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rirekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ristatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rijmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rimodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rimodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ritutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "riisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ricustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ricustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rijmluangmuka", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "rirekuangmuka", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "riidap", AsEnumTypeData.AsInt64)



        'AMBIL MASTER
        Dim dtSatuan As DataTable = AsDataTableAmbilDariDBCon("SELECT ukode, unilai from m1_unit", myConn)
        Dim dtPajak As DataTable = AsDataTableAmbilDariDBCon("SELECT tkode, tnilai from m1_tax", myConn)
        Dim dtBarang As DataTable = AsDataTableAmbilDariDBCon("SELECT bid as idbarang, bkode as kodebarang, bnama as namabarang, btipe as tipebarang, bsatuan as satuanbarang, brekpersediaan as rekpersediaan, brekdiskonpembelian as rekdiskonpembelian from m1_item", myConn)

        'rigudang(3) As String
        Dim dtGudang As DataTable = AsDataTableAmbilDariDBCon("SELECT wkode from m1_warehouse WHERE wkode = '" & FixQuotes(dataUtama(3)) & "'", myConn)
        If dtGudang.Rows.Count = 0 Then
            result(2) = "RI NoTrans : " & dataUtama(11) & " - " & dataUtama(3) & " doesn't exist in Warehouse Data" : GoTo selesai
        End If

        'ritermin(23) As String
        Dim dtTermin As DataTable = AsDataTableAmbilDariDBCon("SELECT trkode, trharijatuhtempo from m1_terms WHERE trkode = '" & FixQuotes(dataUtama(23)) & "'", myConn)
        If Len(FixQuotes(dataUtama(23))) > 0 Then
            If dtTermin.Rows.Count = 0 Then
                result(2) = "RI NoTrans : " & dataUtama(11) & " - " & dataUtama(23) & " doesn't exist in Terms Data" : GoTo selesai
            Else
                'ritgljatuhtempo(24) As Date
                dataUtama(24) = AsFormatTanggal(Date.Parse(dataUtama(12)).AddDays(Double.Parse(dtTermin.Rows(0)("trharijatuhtempo"))), "yyyy-MM-dd")
            End If
        Else
            'ritgljatuhtempo(24) As Date
            dataUtama(24) = dataUtama(12)
        End If

        'rimatauang(30) As String
        Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT ckode, ckurs from m1_currency WHERE ckode = '" & FixQuotes(dataUtama(30)) & "'", myConn)
        If dtMatauang.Rows.Count = 0 Then
            result(2) = "RI NoTrans : " & dataUtama(11) & " - " & dataUtama(30) & " doesn't exist in Currency Data" : GoTo selesai
        End If

        'SUPPLIER
        If Len(FixQuotes(dataUtama(14))) > 0 Then
            dtMasterTemp = AsDataTableAmbilDariDBCon("SELECT kid, CONCAT(k1alamat1,'\n',k1alamat2) as ri1alamat1, CONCAT(k2alamat1,'\n', k2alamat2) as ri2alamat1, knpwp FROM m1_contact WHERE kkategori = 'S' AND kkode = '" & FixQuotes(dataUtama(14)) & "'", myConn)
            If dtMasterTemp.Rows.Count > 0 Then
                'risupplier(14)
                dataUtama(14) = dtMasterTemp.Rows(0)("kid")
                'ri1alamat1(16) As String
                dataUtama(16) = dtMasterTemp.Rows(0)("ri1alamat1")
                'ri2alamat1(19) As String
                dataUtama(19) = dtMasterTemp.Rows(0)("ri2alamat1")
                'rinoref(27) As String
                dataUtama(27) = dtMasterTemp.Rows(0)("knpwp")
            Else
                result(2) = "RI NoTrans : " & dataUtama(11) & " - " & dataUtama(14) & " doesn't exist in Supplier Data" : GoTo selesai
            End If
        End If

        If AsDataTableTambahData(dtutama, "riid~ricabang~rilokasi~rigudang~riasalbarang~riasalbarangkategori~rijenispembelian~rijenispembeliankategori~ricarabayar~risumber~riautonotransaksi~rinotransaksi~ritgl~rikodepa~risupplier~risupplierkontak~ri1alamat1~ri1alamat2~ri1alamat3~ri2alamat1~ri2alamat2~ri2alamat3~ribagianpembelian~ritermin~ritgljatuhtempo~riuraian~ricatatan~rinoref~ritglnoref~ritglpenutupan~rimatauang~rikurs~rihargatermasukpajak~ritotal~ridiskonpersen~rijmldiskon~ritotalpajak1detail~ritotalpajak2detail~ribiayalainpersen~ribiayalain~ritotaltransaksi~rijmlbayar~ristatuslunas~ritgllunas~rinofakturpajak~risdhbayarpajak~ritglbayarpajak~rirekdiskon~rirekpajak1~rirekpajak2~rirekbiayalain~rirekbayar~riidpr~riidcs~riidrq~riidbs~riidpo~riidipc~riidgrn~ristatusdnr~ristatusprt~ristatus~ristatussebelumnya~rijmlrevisi~ricetakanke~riinputuser~riinputtgl~rimodifikasiuser~rimodifikasitgl~riposting~ritutupperiode~riisclose~ricustomtext1~ricustomtext2~ricustomtext3~ricustomtext4~ricustomtext5~ricustomint1~ricustomint2~ricustomint3~ricustomdbl1~ricustomdbl2~ricustomdbl3~ricustomdate1~ricustomdate2~ricustomdate3~rijmluangmuka~rirekuangmuka~riidap", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idridetail(0) As Integer, idri(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, harga(13) As Double, diskon(14) As String, 
        'jmldiskon(15) As Double, pajak1(16) As String, jmlpajak1(17) As Double, pajak2(18) As String, jmlpajak2(19) As Double, 
        'cabang(20) As String, lokasi(21) As String, gudang(22) As String, rekpersediaan(23) As String, rekdiskonpembelian(24) As String, 
        'rekhutangsementara(25) As String, costcenter(26) As String, divisi(27) As String, subdivisi(28) As String, proyek(29) As String, 
        'catatan(30) As String, urutan(31) As Integer, idprdetail(32) As Integer, idcsdetail(33) As Integer, idrqdetail(34) As Integer, 
        'idbsdetail(35) As Integer, idpodetail(36) As Integer, idipcdetail(37) As Integer, idgrndetail(38) As Integer, jmldnr(39) As Double, 
        'statusdnr(40) As Integer, jmlprt(41) As Double, statusprt(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, 
        'subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, 
        'idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, 
        'statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================


        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idridetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hargafix", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaijmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaijmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhutangsementara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idprdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idcsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idrqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbsdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idipcdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idgrndetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlprt", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusprt", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingPO As String = "", ftOutstandingPO As String = "", updNilaiPO As String = "", updFilterPO As String = ""
        Dim ftExistOutstandingGRN As String = "", ftOutstandingGRN As String = "", updNilaiGRN As String = "", updFilterGRN As String = ""
        Dim idbarang As Integer = 0, idpodetail As Integer = 0, idgrndetail As Integer = 0, jmlbarang As Double = 0
        Dim gudang As String = "", updStokOutBooking As String = ""

        'FILTER PO DAN GRN, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftPO As String = "", ftGRN As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 53) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idridetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idridetail required numeric." : GoTo selesai
            End If
            'idri(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idri required numeric." : GoTo selesai
            End If

            dtMasterTemp = AsDataTableFilterSortDt(dtBarang, "kodebarang = '" & FixQuotes(dataRowDetail(2)) & "'")
            If dtMasterTemp.Rows.Count > 0 Then
                'idbarang(2) As Integer
                dataRowDetail(2) = dtMasterTemp.Rows(0)("idbarang")
                'namabarang(3) As String
                dataRowDetail(3) = dtMasterTemp.Rows(0)("namabarang")
                'tipebarang(4) As String
                dataRowDetail(4) = dtMasterTemp.Rows(0)("tipebarang")
                'satuanbarang(9) As String
                dataRowDetail(9) = dtMasterTemp.Rows(0)("satuanbarang")
                'rekpersediaan(23) As String
                dataRowDetail(23) = dtMasterTemp.Rows(0)("rekpersediaan")
                'rekdiskonpembelian(24) As String
                dataRowDetail(24) = dtMasterTemp.Rows(0)("rekdiskonpembelian")
            Else
                result(2) = "RI NoTrans : " & dataUtama(11) & " - '" & dataRowDetail(2) & "' doesn't exist in Item Data" : GoTo selesai
            End If

            'idbarang(2) As Integer
            If (IsNumeric(dataRowDetail(2)) = False) Then
                result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            End If

            'jml(5) As Double
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(7) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(8) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(11) As Double
            If (IsNumeric(dataRowDetail(11)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'hargafix(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - hargafix required numeric." : GoTo selesai
            End If
            'harga(13) As Double
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If

            'pajak1(16) As String
            If Len(FixQuotes(dataRowDetail(16))) > 0 Then
                strValMaster = AsDataTableDLookup(dtPajak, "tnilai", "tkode = '" & FixQuotes(dataRowDetail(16)) & "'", ".xxx. data not found .xxx.")
                If strValMaster = ".xxx. data not found .xxx." Then
                    result(2) = "RI NoTrans : " & dataUtama(11) & " - '" & dataRowDetail(16) & "' doesn't exist in Tax Data" : GoTo selesai
                Else
                    'jmlpajak1(17) As Double
                    dataRowDetail(17) = Double.Parse(strValMaster)
                End If
            Else
                'jmlpajak1(17) As Double
                dataRowDetail(17) = 0
            End If

            'jmlpajak1(17) As Double
            If (IsNumeric(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If

            'pajak2(18) As String
            If Len(FixQuotes(dataRowDetail(18))) > 0 Then
                strValMaster = AsDataTableDLookup(dtPajak, "tnilai", "tkode = '" & FixQuotes(dataRowDetail(18)) & "'", ".xxx. data not found .xxx.")
                If strValMaster = ".xxx. data not found .xxx." Then
                    result(2) = "RI NoTrans : " & dataUtama(11) & " - '" & dataRowDetail(18) & "' doesn't exist in Tax Data" : GoTo selesai
                Else
                    'jmlpajak2(19) As Double
                    dataRowDetail(19) = Double.Parse(strValMaster)
                End If
            Else
                'jmlpajak2(19) As Double
                dataRowDetail(19) = 0
            End If

            'jmlpajak2(19) As Double
            If (IsNumeric(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(31) As Integer
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(33) As Integer
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(35) As Integer
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'idgrndetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'jmldnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmldnr required numeric." : GoTo selesai
            End If
            'statusdnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusdnr required numeric." : GoTo selesai
            End If
            'jmlprt(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(47) As Double
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(49) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(50) As Date
            If (IsDate(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(51) As Date
            If (IsDate(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(52) As Date
            If (IsDate(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 100 Then
                result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
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
            strValMaster = AsDataTableDLookup(dtSatuan, "unilai", "ukode = '" & FixQuotes(dataRowDetail(6)) & "'", ".xxx. data not found .xxx.")
            If strValMaster = ".xxx. data not found .xxx." Then
                result(2) = "RI NoTrans : " & dataUtama(11) & " - '" & dataRowDetail(6) & "' doesn't exist in Unit Data" : GoTo selesai
            Else
                'nilaisatuan(7) As Double
                dataRowDetail(7) = Double.Parse(strValMaster)
                'jmlbarang(8) As Double                 jml(5) As Double
                dataRowDetail(8) = Double.Parse(dataRowDetail(5)) * Double.Parse(dataRowDetail(7))
            End If

            If Len(dataRowDetail(6)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(7) As Double
            If Len(dataRowDetail(7)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(8) As Double
            If Len(dataRowDetail(8)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(8) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(10) As String
            dataRowDetail(10) = dataUtama(30)
            If Len(dataRowDetail(10)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(10)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(11) As Double
            dataRowDetail(11) = dataUtama(31)
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(13) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(14) As String
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(14)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(13) As Double, diskon(14) As String
                dataRowDetail(15) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(13)), FixQuotes(dataRowDetail(14).ToString))
            End If

            'jmlpajak1(17) As Double
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(19) As Double
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudang(22) As String
            dataRowDetail(22) = dataUtama(3)
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - gudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(22)) > 25 Then
                result(2) = "Row : " & i & " - gudang should not be more than 25 character." : GoTo selesai
            End If

            'jmldnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmldnr can't be empty" : GoTo selesai
            End If

            'jmlprt(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
            End If

            'customdbl1(47) As Double
            If Len(dataRowDetail(47)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(48) As Double
            If Len(dataRowDetail(48)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(49) As Double
            If Len(dataRowDetail(49)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(50) As Date
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(51) As Date
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(52) As Date
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idridetail~idri~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~nilaijmlpajak1~pajak2~nilaijmlpajak2~cabang~lokasi~gudang~rekpersediaan~rekdiskonpembelian~rekhutangsementara~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(22) As String       , idpodetail(36) As Integer      , idgrndetail(38) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(22) : idpodetail = dataRowDetail(36) : idgrndetail = dataRowDetail(38)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")


            'VALIDASI OUTSTANDING -------------------------
            If idpodetail <> 0 Then 'PO
                'CEK PO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftPO = IIf(Len(ftPO.ToString) = 0, "", ftPO & " OR ")
                ftPO = String.Concat(ftPO, " (pod.idpodetail = " & idpodetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingPO = IIf(Len(ftExistOutstandingPO.ToString) = 0, "", ftExistOutstandingPO & " UNION ")
                ftExistOutstandingPO = String.Concat(ftExistOutstandingPO, "SELECT EXISTS(SELECT 1 FROM m4_po_detail JOIN m4_po ON idpo = poid WHERE idpodetail = '" & idpodetail & "' AND (postatus = 2 OR postatus = 3 OR postatus = 4 OR postatus = 7) LIMIT 1) as rowExists, '" & idpodetail & "' as idpodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idpodetail=" & idpodetail)
                ftOutstandingPO = IIf(Len(ftOutstandingPO.ToString) = 0, "", ftOutstandingPO & " OR ")
                ftOutstandingPO = String.Concat(ftOutstandingPO, " (pod.idpodetail = " & idpodetail & " AND " & Outstanding & " > (pod.jmlbarang - pod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiPO = String.Concat("WHEN '" & idpodetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiPO)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                updFilterPO = String.Concat(updFilterPO, "(idpodetail = '" & idpodetail & "')")

                'SET NILAI UPDATE STOK BOOKING (MENGURANGI)
                updStokOutBooking = IIf(Len(updStokOutBooking.ToString) = 0, "", updStokOutBooking & ", ")
                updStokOutBooking = String.Concat(updStokOutBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking
            End If

            If idgrndetail <> 0 Then 'GRN
                'CEK GRN YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftGRN = IIf(Len(ftGRN.ToString) = 0, "", ftGRN & " OR ")
                ftGRN = String.Concat(ftGRN, " (grnd.idgrndetail = " & idgrndetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingGRN = IIf(Len(ftExistOutstandingGRN.ToString) = 0, "", ftExistOutstandingGRN & " UNION ")
                ftExistOutstandingGRN = String.Concat(ftExistOutstandingGRN, "SELECT EXISTS(SELECT 1 FROM m4_grn_detail JOIN m4_grn ON idgrn = grnid WHERE idgrndetail = '" & idgrndetail & "' AND (grnstatus = 2 OR grnstatus = 3 OR grnstatus = 4 OR grnstatus = 7) LIMIT 1) as rowExists, '" & idgrndetail & "' as idgrndetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idgrndetail=" & idgrndetail)
                ftOutstandingGRN = IIf(Len(ftOutstandingGRN.ToString) = 0, "", ftOutstandingGRN & " OR ")
                ftOutstandingGRN = String.Concat(ftOutstandingGRN, " (grnd.idgrndetail = " & idgrndetail & " AND " & Outstanding & " > (grnd.jmlbarang - grnd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiGRN = String.Concat("WHEN '" & idgrndetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiGRN)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                updFilterGRN = String.Concat(updFilterGRN, "(idgrndetail = '" & idgrndetail & "')")
            End If
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
        Dim vStatus As Integer = 0, vTgl As String = ""

        Try
            'Proses utama
            If (dtutama.Rows.Count > 0) Then
                Dim drutama As DataRow = dtutama.Rows(0)
                Dim rsValidasi As String = ""
                vStatus = drutama("ristatus")
                vTgl = AsFormatTanggal(drutama("ritgl"))

                'SET JENIS PEMBELIAN
                'JIKA AMBIL GRN MAKA JENIS PEMBELIAN = 0, JIKA TANPA GRN MAKA JENIS PEMBELIAN = 1
                Dim vIdGrnDetail As Double = AsDataTableDCount(dtdetail, "idgrndetail > 0")
                If vIdGrnDetail > 0 Then
                    drutama("rijenispembeliankategori") = 0
                Else
                    drutama("rijenispembeliankategori") = 1
                End If


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 11
                Select Case drutama("ristatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("ritgl")), AsFormatTanggal(drutama("ritgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                Dim MUFungsional As String = "", MUUtama As String = ""
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : GoTo selesai
                End If

                'SET MATA UANG UTAMA
                MUUtama = drutama("rimatauang")
                'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                'validasi backdate
                Dim dtHACustom As DataTable = AsDataTableAmbilDariDBCon("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 3 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1", myConn)
                If dtHACustom.Rows.Count > 0 Then
                    If dtHACustom.Rows(0)("rcakses") = 0 Then
                        GoTo validasiBackDate
                    End If
                Else

validasiBackDate:
                    'result(2) = drutama("satgl") & " - " & AsFormatTanggal(Date.Now) : Trans.Rollback() : GoTo selesai
                    If DateDiff(DateInterval.Day, Date.Now, Date.Parse(drutama("ritgl"))) < 0 Then
                        result(2) = "This user can't to insert the back date transaction" : Trans.Rollback() : GoTo selesai

                    End If
                End If
                If drutama("ristatus") = 2 Or drutama("ristatus") = 1 Or drutama("ristatus") = 8 Or drutama("ristatus") = 9 Or drutama("ristatus") = 10 Or drutama("ristatus") = 11 Then

                    ''JIKA TANPA GRN MAKA CEK BATCH DAN SERIAL
                    'If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                    '    'VALIDASI BATCH SERIAL ---------------
                    '    rsValidasi = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    '    'END OF VALIDASI BATCH SERIAL --------

                    '    'VALIDASI ASSET ----------------------
                    '    'ValidasiAsset
                    '    rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 1)
                    '    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    '    'END OF VALIDASI ASSET ---------------
                    'End If

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingPO, ftOutstandingPO, ftExistOutstandingGRN, ftOutstandingGRN, "", "", "", "", ftPO, ftGRN, drutama("rihargatermasukpajak"), "", "")
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'FUNGSI SET TANGGAL JATUH TEMPO DIHILANGKAN, KARENA di flex tambah inputan
                'SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("ritermin").ToString, AsFormatTanggal(drutama("ritgl")), "ritgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("ritgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'SET TANGGAL JATUH TEMPO BERDASARKAN SETTING
                'JIKA SETTING BERDASARKAN TUKAR FAKTUR MAKA TANGGAL JATUH TEMPO DISET 2100-12-31
                Dim setTglJT As String = F_getSetting(4, "tukarfaktur", "UpdateTglJatuhTempoRI")
                If setTglJT.Equals("1") Then
                    drutama("ritgljatuhtempo") = "2100-12-31"
                End If


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
                dtdetail.Columns("jmlpajak1").Expression = "subtotal * (nilaijmlpajak1/100)"

                AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
                dtdetail.Columns("jmlpajak2").Expression = "subtotal * (nilaijmlpajak2/100)"

                'TAMBAHKAN FIELD SUBTOTAL PADA COST
                'SUBTOTAL = jumlah
                'AsDataTableTambahField(dtcost, "subtotal", AsEnumTypeData.AsDouble)
                'dtcost.Columns("subtotal").Expression = "jumlah"

                ''TOTAL = subtotal detail + subtotal cost
                'drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal") + AsDataTableDSum(dtcost, "subtotal")

                'TOTAL = subtotal detail
                drutama("ritotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("ritotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("ritotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("rihargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN - JMLUANGMUKA 
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ritotalpajak1detail")) + Double.Parse(drutama("ritotalpajak2detail")) + Double.Parse(drutama("ribiayalain")) - Double.Parse(drutama("rijmluangmuka"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN - JMLUANGMUKA 
                    drutama("ritotaltransaksi") = Double.Parse(drutama("ritotal")) - Double.Parse(drutama("rijmldiskon")) + Double.Parse(drutama("ritotalpajak2detail")) + Double.Parse(drutama("ribiayalain")) - Double.Parse(drutama("rijmluangmuka"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA TUNAI MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("ricarabayar")) = 0 Then

                    'SET JML BAYAR ==========================================
                    If MUUtama = MUFungsional Then
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAH
                        'drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlah", "carabayar <> 10")
                        drutama("rijmlbayar") = 0

                    Else
                        'JIKA MATAUANG FUNGSIONAL MAKA SUM FIELD JUMLAHVALAS
                        'drutama("rijmlbayar") = AsDataTableDSum(dtpay, "jumlahvalas", "carabayar <> 10")
                        drutama("rijmlbayar") = 0

                    End If
                    'END OF SET JML BAYAR ===================================


                    'SET TGL LUNAS ==========================================
                    'JIKA TUNAI MAKA TGL LUNAS = TGL TRANSAKSI
                    If Double.Parse(drutama("rijmlbayar")) >= Double.Parse(drutama("ritotaltransaksi")) Then
                        drutama("ritgllunas") = drutama("ritgl") : drutama("ristatuslunas") = 2

                    ElseIf Double.Parse(drutama("rijmlbayar")) < 1 Then
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                    Else
                        drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 1

                    End If
                    'END OF SET TGL LUNAS ===================================

                Else
                    drutama("rijmlbayar") = 0 : drutama("ritgllunas") = "1900-01-01" : drutama("ristatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("riid")
                    notransaksi = drutama("rinotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(riid), rinotransaksi FROM m4_ri WHERE riid='" & result(4) & "' AND ristatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("riautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"), drutama("risumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_ri_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Ri_HistorySimpan("" & paramSplit(0) & "★M4_Ri_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("risumber")) & "▼" & FixQuotes(drutama("riid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Ri set ricabang  = '" & FixQuotes(drutama("ricabang")) & "', rilokasi  = '" & FixQuotes(drutama("rilokasi")) & "', rigudang  = '" & FixQuotes(drutama("rigudang")) & "', riasalbarang  = '" & FixQuotes(drutama("riasalbarang")) & "', riasalbarangkategori  = " & drutama("riasalbarangkategori") & ", rijenispembelian  = '" & FixQuotes(drutama("rijenispembelian")) & "', rijenispembeliankategori  = " & drutama("rijenispembeliankategori") & ", ricarabayar  = " & drutama("ricarabayar") & ", risumber  = '" & FixQuotes(drutama("risumber")) & "', riautonotransaksi  = " & drutama("riautonotransaksi") & ", rinotransaksi  = '" & FixQuotes(notransaksi) & "', ritgl  = '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', rikodepa  = " & drutama("rikodepa") & ", risupplier  = " & drutama("risupplier") & ", risupplierkontak  = '" & FixQuotes(drutama("risupplierkontak")) & "', ri1alamat1  = '" & FixQuotes(drutama("ri1alamat1")) & "', ri1alamat2  = '" & FixQuotes(drutama("ri1alamat2")) & "', ri1alamat3  = '" & FixQuotes(drutama("ri1alamat3")) & "', ri2alamat1  = '" & FixQuotes(drutama("ri2alamat1")) & "', ri2alamat2  = '" & FixQuotes(drutama("ri2alamat2")) & "', ri2alamat3  = '" & FixQuotes(drutama("ri2alamat3")) & "', ribagianpembelian  = " & drutama("ribagianpembelian") & ", ritermin  = '" & FixQuotes(drutama("ritermin")) & "', ritgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', riuraian  = '" & FixQuotes(drutama("riuraian")) & "', ricatatan  = '" & FixQuotes(drutama("ricatatan")) & "', rinoref  = '" & FixQuotes(drutama("rinoref")) & "', ritglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', ritglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', rimatauang  = '" & FixQuotes(drutama("rimatauang")) & "', rikurs  = '" & FixDouble(drutama("rikurs")) & "', rihargatermasukpajak  = " & drutama("rihargatermasukpajak") & ", ritotal  = '" & FixDouble(drutama("ritotal")) & "', ridiskonpersen  = '" & FixQuotes(drutama("ridiskonpersen")) & "', rijmldiskon  = '" & FixDouble(drutama("rijmldiskon")) & "', ritotalpajak1detail  = '" & FixDouble(drutama("ritotalpajak1detail")) & "', ritotalpajak2detail  = '" & FixDouble(drutama("ritotalpajak2detail")) & "', ribiayalainpersen  = '" & FixQuotes(drutama("ribiayalainpersen")) & "', ribiayalain  = '" & FixDouble(drutama("ribiayalain")) & "', ritotaltransaksi  = '" & FixDouble(drutama("ritotaltransaksi")) & "', rijmlbayar  = '" & FixDouble(drutama("rijmlbayar")) & "', ristatuslunas  = " & drutama("ristatuslunas") & ", ritgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', rinofakturpajak  = '" & FixQuotes(drutama("rinofakturpajak")) & "', risdhbayarpajak  = " & drutama("risdhbayarpajak") & ", ritglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', rirekdiskon  = '" & FixQuotes(drutama("rirekdiskon")) & "', rirekpajak1  = '" & FixQuotes(drutama("rirekpajak1")) & "', rirekpajak2  = '" & FixQuotes(drutama("rirekpajak2")) & "', rirekbiayalain  = '" & FixQuotes(drutama("rirekbiayalain")) & "', rirekbayar  = '" & FixQuotes(drutama("rirekbayar")) & "', riidpr  = " & drutama("riidpr") & ", riidcs  = " & drutama("riidcs") & ", riidrq  = " & drutama("riidrq") & ", riidbs  = " & drutama("riidbs") & ", riidpo  = " & drutama("riidpo") & ", riidipc  = " & drutama("riidipc") & ", riidgrn  = " & drutama("riidgrn") & ", ristatusdnr  = " & drutama("ristatusdnr") & ", ristatusprt  = " & drutama("ristatusprt") & ", ristatus  = " & drutama("ristatus") & ", ristatussebelumnya  = " & drutama("ristatussebelumnya") & ", rijmlrevisi  = rijmlrevisi+1, ricetakanke  = " & drutama("ricetakanke") & ", rimodifikasiuser  = " & drutama("rimodifikasiuser") & ", rimodifikasitgl  = NOW(), riposting  = 0, ritutupperiode  = " & drutama("ritutupperiode") & ", ricustomtext1  = '" & FixQuotes(drutama("ricustomtext1")) & "', ricustomtext2  = '" & FixQuotes(drutama("ricustomtext2")) & "', ricustomtext3  = '" & FixQuotes(drutama("ricustomtext3")) & "', ricustomtext4  = '" & FixQuotes(drutama("ricustomtext4")) & "', ricustomtext5  = '" & FixQuotes(drutama("ricustomtext5")) & "', ricustomint1  = " & drutama("ricustomint1") & ", ricustomint2  = " & drutama("ricustomint2") & ", ricustomint3  = " & drutama("ricustomint3") & ", ricustomdbl1  = '" & FixDouble(drutama("ricustomdbl1")) & "', ricustomdbl2  = '" & FixDouble(drutama("ricustomdbl2")) & "', ricustomdbl3  = '" & FixDouble(drutama("ricustomdbl3")) & "', ricustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', ricustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', ricustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', rijmluangmuka = '" & FixDouble(drutama("rijmluangmuka")) & "', rirekuangmuka = '" & FixQuotes(drutama("rirekuangmuka")) & "', riidap = '" & FixDouble(drutama("riidap")) & "' where riid = '" & drutama("riid") & "'"
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

                    If drutama("riautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("ricabang"), drutama("rilokasi"), drutama("risumber"), drutama("ritgl"), drutama("risumber"), 4)
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
                        notransaksi = drutama("rinotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(riid) FROM m4_ri WHERE rinotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Ri (ricabang, rilokasi, rigudang, riasalbarang, riasalbarangkategori, rijenispembelian, rijenispembeliankategori, ricarabayar, risumber, riautonotransaksi, rinotransaksi, ritgl, rikodepa, risupplier, risupplierkontak, ri1alamat1, ri1alamat2, ri1alamat3, ri2alamat1, ri2alamat2, ri2alamat3, ribagianpembelian, ritermin, ritgljatuhtempo, riuraian, ricatatan, rinoref, ritglnoref, ritglpenutupan, rimatauang, rikurs, rihargatermasukpajak, ritotal, ridiskonpersen, rijmldiskon, ritotalpajak1detail, ritotalpajak2detail, ribiayalainpersen, ribiayalain, ritotaltransaksi, rijmlbayar, ristatuslunas, ritgllunas, rinofakturpajak, risdhbayarpajak, ritglbayarpajak, rirekdiskon, rirekpajak1, rirekpajak2, rirekbiayalain, rirekbayar, riidpr, riidcs, riidrq, riidbs, riidpo, riidipc, riidgrn, ristatusdnr, ristatusprt, ristatus, ristatussebelumnya, rijmlrevisi, ricetakanke, riinputuser, riinputtgl, rimodifikasiuser, rimodifikasitgl, riposting, ritutupperiode, riisclose, ricustomtext1, ricustomtext2, ricustomtext3, ricustomtext4, ricustomtext5, ricustomint1, ricustomint2, ricustomint3, ricustomdbl1, ricustomdbl2, ricustomdbl3, ricustomdate1, ricustomdate2, ricustomdate3, rijmluangmuka, rirekuangmuka, riidap) values('" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(drutama("rigudang")) & "', '" & FixQuotes(drutama("riasalbarang")) & "', " & drutama("riasalbarangkategori") & ", '" & FixQuotes(drutama("rijenispembelian")) & "', " & drutama("rijenispembeliankategori") & ", " & drutama("ricarabayar") & ", '" & FixQuotes(drutama("risumber")) & "', " & drutama("riautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("rikodepa") & ", " & drutama("risupplier") & ", '" & FixQuotes(drutama("risupplierkontak")) & "', '" & FixQuotes(drutama("ri1alamat1")) & "', '" & FixQuotes(drutama("ri1alamat2")) & "', '" & FixQuotes(drutama("ri1alamat3")) & "', '" & FixQuotes(drutama("ri2alamat1")) & "', '" & FixQuotes(drutama("ri2alamat2")) & "', '" & FixQuotes(drutama("ri2alamat3")) & "', " & drutama("ribagianpembelian") & ", '" & FixQuotes(drutama("ritermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgljatuhtempo"))) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(drutama("rinoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritglpenutupan"))) & "', '" & FixQuotes(drutama("rimatauang")) & "', '" & FixDouble(drutama("rikurs")) & "', " & drutama("rihargatermasukpajak") & ", '" & FixDouble(drutama("ritotal")) & "', '" & FixQuotes(drutama("ridiskonpersen")) & "', '" & FixDouble(drutama("rijmldiskon")) & "', '" & FixDouble(drutama("ritotalpajak1detail")) & "', '" & FixDouble(drutama("ritotalpajak2detail")) & "', '" & FixQuotes(drutama("ribiayalainpersen")) & "', '" & FixDouble(drutama("ribiayalain")) & "', '" & FixDouble(drutama("ritotaltransaksi")) & "', '" & FixDouble(drutama("rijmlbayar")) & "', " & drutama("ristatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritgllunas"))) & "', '" & FixQuotes(drutama("rinofakturpajak")) & "', " & drutama("risdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("ritglbayarpajak"))) & "', '" & FixQuotes(drutama("rirekdiskon")) & "', '" & FixQuotes(drutama("rirekpajak1")) & "', '" & FixQuotes(drutama("rirekpajak2")) & "', '" & FixQuotes(drutama("rirekbiayalain")) & "', '" & FixQuotes(drutama("rirekbayar")) & "', " & drutama("riidpr") & ", " & drutama("riidcs") & ", " & drutama("riidrq") & ", " & drutama("riidbs") & ", " & drutama("riidpo") & ", " & drutama("riidipc") & ", " & drutama("riidgrn") & ", " & drutama("ristatusdnr") & ", " & drutama("ristatusprt") & ", " & drutama("ristatus") & ", " & drutama("ristatussebelumnya") & ", " & drutama("rijmlrevisi") & ", " & drutama("ricetakanke") & ", " & drutama("riinputuser") & ", NOW(), " & drutama("rimodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("ritutupperiode") & ", " & drutama("riisclose") & ", '" & FixQuotes(drutama("ricustomtext1")) & "', '" & FixQuotes(drutama("ricustomtext2")) & "', '" & FixQuotes(drutama("ricustomtext3")) & "', '" & FixQuotes(drutama("ricustomtext4")) & "', '" & FixQuotes(drutama("ricustomtext5")) & "', " & drutama("ricustomint1") & ", " & drutama("ricustomint2") & ", " & drutama("ricustomint3") & ", '" & FixDouble(drutama("ricustomdbl1")) & "', '" & FixDouble(drutama("ricustomdbl2")) & "', '" & FixDouble(drutama("ricustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ricustomdate3"))) & "', '" & FixDouble(drutama("rijmluangmuka")) & "', '" & FixQuotes(drutama("rirekuangmuka")) & "', '" & FixDouble(drutama("riidap")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select riid from M4_ri where rinotransaksi='" & notransaksi & "' AND riinputuser= '" & userid & "' order by rimodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Ri_Detail where idri = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idridetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhutangsementara")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Ri_Detail(idridetail, idri, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, rekpersediaan, rekdiskonpembelian, rekhutangsementara, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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



                Dim sumber As String = "RI", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0


                'UPDATE OUTSTANDING TRANSAKSI ==========================================================
                If drutama("ristatus") = 2 Then
                    If Len(updNilaiPO) > 0 Then 'PO
                        'UPDATE DETAIL
                        sql = "UPDATE m4_po_detail SET jmlrealisasi = (CASE idpodetail " & updNilaiPO & " ELSE jmlrealisasi END) WHERE " & updFilterPO
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idpo FROM M4_po_detail WHERE " & updFilterPO & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idpo = '" & dr1("idpo") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idpo, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_po_detail WHERE " & ftDetail & " GROUP BY idpo", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiPO = "" : updFilterPO = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiPO = String.Concat(updNilaiPO, "WHEN '" & dr1("idpo") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterPO = IIf(Len(updFilterPO.ToString) = 0, "", updFilterPO & " OR ")
                                updFilterPO = String.Concat(updFilterPO, "(poid = '" & dr1("idpo") & "')")
                            Next

                            sql = "UPDATE m4_po SET postatusrealisasi = (CASE poid " & updNilaiPO & " ELSE postatusrealisasi END) WHERE " & updFilterPO
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

                    If Len(updNilaiGRN) > 0 Then 'GRN
                        'UPDATE DETAIL
                        sql = "UPDATE m4_grn_detail SET jmlrealisasi = (CASE idgrndetail " & updNilaiGRN & " ELSE jmlrealisasi END) WHERE " & updFilterGRN
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE UTAMA
                        Dim ftDetail As String = "", statusOut As Integer = 0
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idgrn FROM m4_grn_detail WHERE " & updFilterGRN & " GROUP BY idgrn", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idgrn = '" & dr1("idgrn") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idgrn, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_grn_detail WHERE " & ftDetail & " GROUP BY idgrn", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiGRN = "" : updFilterGRN = ""
                            For Each dr1 As DataRow In dtOut.Rows
                                '1. SET STATUS OUTSTANDING (2 = SUDAH, 1 = PROSES, 0 = BELUM)
                                If dr1("jmlrealisasi") >= dr1("jmlbarang") Then
                                    statusOut = 2
                                ElseIf dr1("jmlrealisasi") < 1 Then
                                    statusOut = 0
                                Else
                                    statusOut = 1
                                End If
                                '2. SET NILAI UPDATE OUTSTANDING
                                updNilaiGRN = String.Concat(updNilaiGRN, "WHEN '" & dr1("idgrn") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterGRN = IIf(Len(updFilterGRN.ToString) = 0, "", updFilterGRN & " OR ")
                                updFilterGRN = String.Concat(updFilterGRN, "(grnid = '" & dr1("idgrn") & "')")
                            Next

                            sql = "UPDATE m4_grn SET grnstatusrealisasi = (CASE grnid " & updNilaiGRN & " ELSE grnstatusrealisasi END) WHERE " & updFilterGRN
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



                    'UPDATE STOK BOOKING ============================================================
                    If Len(updStokOutBooking) > 0 Then
                        sql = "INSERT INTO m1_item_booking_po (idbarang, gudang, jmlbooking) VALUES " & updStokOutBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF UPDATE STOK BOOKING =====================================================


                    'JIKA TANPA GRN MAKA HITUNG TRANSAKSI BARANG DAN POSTING HPP
                    If Double.Parse(drutama("rijenispembeliankategori")) = 1 Then
                        'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                        Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rid.idridetail, rid.idbarang, rid.namabarang, rid.tipebarang, rid.jml, rid.satuan, rid.jmlbarang, rid.satuanbarang, rid.matauang, rid.kurs, rid.harga, rid.diskon, rid.jmldiskon, rid.gudang, rid.catatan, rid.costcenter, rid.divisi, rid.subdivisi, rid.proyek, ri.riinputtgl, i.bhpp, rid.jmlpajak1, rid.jmlpajak2 FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE rid.idri = '" & result(4) & "' ORDER BY rid.urutan", myConn)

                        Dim hpp As Double = 0, postinghpp As Double = 0, bstok As Double = 0
                        Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                        Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                        If dtDetailNew.Rows.Count > 0 Then

                            'INSERT ITEM TRANSACTION ====================================================
                            For Each dr1 As DataRow In dtDetailNew.Rows
                                'SET NILAI VARIABEL
                                idbarang = Double.Parse(dr1("idbarang"))
                                jmlbarang = Double.Parse(dr1("jmlbarang"))
                                gudang = dr1("gudang")

                                'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                                sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'  AND bjenis = 'P'"
                                dtSaldo = AsDataTableAmbilDariDBCon(sql, myConn)
                                If dtSaldo.Rows.Count > 0 Then
                                    'set nilai stok
                                    bstok = Double.Parse(dtSaldo.Rows(0)("bstok"))

                                    'jenismutasi dan postinghpp 
                                    '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 0
                                    '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                    jenismutasi = 1 : postinghpp = 0

                                    'hitung saldojml = bstok + jmlbarang
                                    saldojml = bstok + jmlbarang

                                    'hitung hpp = 0, saldohpp = 0, saldonilai = 0
                                    hpp = 0 : saldohpp = 0 : saldonilai = 0
                                    hpp = (FixDouble(dr1("harga")) - FixDouble(dr1("jmldiskon"))) * FixDouble(dr1("kurs"))
                                    'QUERY INSERT TRANSAKSI BARANG
                                    strTransaksiBarang.Clear()
                                    'mapping                        id,                            cabang,                                    lokasi,                             gudang,                        kodepa,           jenismutasi,                              sumber,              idutama,                  iddetail,                      notransaksi,                                                 tgl,                          kontak,                 idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                 uraian,                                   catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                    strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("ricabang")) & "', '" & FixQuotes(drutama("rilokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rikodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("risumber")) & "', " & result(4) & ", " & dr1("idridetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("ritgl"))) & "', " & drutama("risupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("riuraian")) & "', '" & FixQuotes(drutama("ricatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("riinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("riinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()

                                    'UPDATE STOK GLOBAL
                                    'sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = '" & FixDouble(Double.Parse(dr1("kurs")) * Double.Parse(dr1("harga"))) & "' WHERE bid = '" & idbarang & "'"
                                    If drutama("rihargatermasukpajak") = 0 Then
                                        sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak1")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs"))) + ((Double.Parse(dr1("jmlpajak2")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                    Else
                                        sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END) WHERE bid = '" & idbarang & "'"
                                    End If
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
                            'END OF INSERT ITEM TRANSACTION =============================================

                        Else
                            result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If


                        'INSERT MSMQ COGS ===============================================================
                        If drutama("ristatus") = 2 Then
                            Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                            'BUAT ID UNIQUE
                            mjid = Security.MD5CalcString(userid & sumber & result(4) & Now) 'RandomId.Generate(15)

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
                            Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                            If ProsesHpp.Equals("0") = False Then
                                hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                                If Len(hasilMsmq) > 0 Then
                                    result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                                End If
                            End If

                        End If
                        'END OF INSERT MSMQ COGS ========================================================

                    End If

                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'Dim dtNotifikasi As DataTable = AsDataTableAmbilDariDBCon("SELECT * FROM m0_notifikasi_email WHERE moduleid = 4 AND menuid = 11 AND userid = " & drutama("riinputuser"), myConn)
                'If dtNotifikasi.Rows.Count > 0 Then
                '    'For Each dr1 As DataRow In dtNotifikasi.Rows
                '    '    ftDetaill = IIf(Len(ftDetaill.ToString) = 0, "", ftDetaill & " OR ")
                '    '    ftDetaill = String.Concat(ftDetaill, "(idsq = '" & dr1("idsq") & "')")
                '    'Next
                '    If dtNotifikasi.Rows(0)(3) = drutama("ristatus") Then
                '        Dim fromAddress = New MailAddress(dtNotifikasi.Rows(0)(7).ToString, "Notifikasi RI")
                '        Dim toAddress = New MailAddress(dtNotifikasi.Rows(0)(6).ToString, "Notifikasi RI")
                '        Dim fromPassword As String = dtNotifikasi.Rows(0)(8).ToString
                '        Const subject As String = "RI Status"
                '        Const body As String = "RI Status"

                '        Dim smtp = New SmtpClient()
                '        smtp.Host = "smtp.gmail.com"
                '        smtp.Port = 587
                '        smtp.EnableSsl = True
                '        smtp.DeliveryMethod = SmtpDeliveryMethod.Network
                '        smtp.UseDefaultCredentials = True
                '        smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, fromPassword)

                '        Using message = New MailMessage(fromAddress, toAddress)
                '            message.Subject = subject
                '            message.Body = body
                '            smtp.Send(message)
                '        End Using
                '    End If
                'End If

                'INSERT MSMQ JURNAL =================================================================
                If drutama("ristatus") = 2 Then
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

End Class