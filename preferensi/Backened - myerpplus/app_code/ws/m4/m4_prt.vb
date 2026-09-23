Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_prt
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PrtSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch() As String
        Dim dataSerial(), dataRowSerial(), dataAsset(), dataRowAsset() As String

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
        If (dataSplit.Length <> 4 And dataSplit.Length <> 5) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'prtid(0) As Integer, prtcabang(1) As String, prtlokasi(2) As String, prtgudang(3) As String, prtasalbarang(4) As String, 
        'prtasalbarangkategori(5) As Integer, prtjenispembelian(6) As String, prtjenispembeliankategori(7) As Integer, prtcarabayar(8) As Integer, prtsumber(9) As String, 
        'prtautonotransaksi(10) As Integer, prtnotransaksi(11) As String, prttgl(12) As Date, prtkodepa(13) As Integer, prtsupplier(14) As Integer, 
        'prtsupplierkontak(15) As String, prt1alamat1(16) As String, prt1alamat2(17) As String, prt1alamat3(18) As String, prt2alamat1(19) As String, 
        'prt2alamat2(20) As String, prt2alamat3(21) As String, prtbagianpembelian(22) As Integer, prttermin(23) As String, prttgljatuhtempo(24) As Date, 
        'prturaian(25) As String, prtcatatan(26) As String, prtnoref(27) As String, prttglnoref(28) As Date, prttglpenutupan(29) As Date, 
        'prtmatauang(30) As String, prtkurs(31) As Double, prthargatermasukpajak(32) As Integer, prttotal(33) As Double, prtdiskonpersen(34) As String, 
        'prtjmldiskon(35) As Double, prttotalpajak1detail(36) As Double, prttotalpajak2detail(37) As Double, prtbiayalainpersen(38) As String, prtbiayalain(39) As Double, 
        'prttotaltransaksi(40) As Double, prtsisatransaksi(41) As Double, prtjmlbayar(42) As Double, prtstatuslunas(43) As Integer, prttgllunas(44) As Date, 
        'prtnofakturpajak(45) As String, prtsdhbayarpajak(46) As Integer, prttglbayarpajak(47) As Date, prtrekdiskon(48) As String, prtrekpajak1(49) As String, 
        'prtrekpajak2(50) As String, prtrekbiayalain(51) As String, prtrekbayar(52) As String, prtreksisa(53) As String, prtidpr(54) As Integer, 
        'prtidcs(55) As Integer, prtidrq(56) As Integer, prtidbs(57) As Integer, prtidpo(58) As Integer, prtidipc(59) As Integer, 
        'prtidgrn(60) As Integer, prtidri(61) As Integer, prtiddnr(62) As Integer, prtstatus(63) As Integer, prtstatussebelumnya(64) As Integer, 
        'prtjmlrevisi(65) As Integer, prtcetakanke(66) As Integer, prtinputuser(67) As Integer, prtinputtgl(68) As DateTime, prtmodifikasiuser(69) As Integer, 
        'prtmodifikasitgl(70) As DateTime, prtposting(71) As Integer, prttutupperiode(72) As Integer, prtisclose(73) As Integer, prtcustomtext1(74) As String, 
        'prtcustomtext2(75) As String, prtcustomtext3(76) As String, prtcustomtext4(77) As String, prtcustomtext5(78) As String, prtcustomint1(79) As Integer, 
        'prtcustomint2(80) As Integer, prtcustomint3(81) As Integer, prtcustomdbl1(82) As Double, prtcustomdbl2(83) As Double, prtcustomdbl3(84) As Double, 
        'prtcustomdate1(85) As Date, prtcustomdate2(86) As Date, prtcustomdate3(87) As Date, prtjenis(88) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, 
        'prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, 
        'prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis(88) As Integer

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 89) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'prtid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "prtid required numeric." : GoTo selesai
        End If
        'prtasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "prtasalbarangkategori required numeric." : GoTo selesai
        End If
        'prtjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "prtjenispembeliankategori required numeric." : GoTo selesai
        End If
        'prtcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "prtcarabayar required numeric." : GoTo selesai
        End If
        'prtautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "prtautonotransaksi required numeric." : GoTo selesai
        End If
        'prttgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "prttgl required date." : GoTo selesai
        End If
        'prtkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "prtkodepa required numeric." : GoTo selesai
        End If
        'prtsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "prtsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "prtsupplier can't be empty." : GoTo selesai
        End If
        'prtbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "prtbagianpembelian required numeric." : GoTo selesai
        End If
        'prttgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "prttgljatuhtempo required date." : GoTo selesai
        End If
        'prttglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "prttglnoref required date." : GoTo selesai
        End If
        'prttglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "prttglpenutupan required date." : GoTo selesai
        End If
        'prtkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "prtkurs required numeric." : GoTo selesai
        End If
        'prthargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "prthargatermasukpajak required numeric." : GoTo selesai
        End If
        'prttotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "prttotal required numeric." : GoTo selesai
        End If
        'prtjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "prtjmldiskon required numeric." : GoTo selesai
        End If
        'prttotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "prttotalpajak1detail required numeric." : GoTo selesai
        End If
        'prttotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "prttotalpajak2detail required numeric." : GoTo selesai
        End If
        'prtbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "prtbiayalain required numeric." : GoTo selesai
        End If
        'prttotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "prttotaltransaksi required numeric." : GoTo selesai
        End If
        'prtsisatransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "prtsisatransaksi required numeric." : GoTo selesai
        End If
        'prtjmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "prtjmlbayar required numeric." : GoTo selesai
        End If
        'prtstatuslunas(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "prtstatuslunas required numeric." : GoTo selesai
        End If
        'prttgllunas(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "prttgllunas required date." : GoTo selesai
        End If
        'prtsdhbayarpajak(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "prtsdhbayarpajak required numeric." : GoTo selesai
        End If
        'prttglbayarpajak(47) As Date
        If (IsDate(dataUtama(47)) = False) Then
            result(2) = "prttglbayarpajak required date." : GoTo selesai
        End If
        'prtidpr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "prtidpr required numeric." : GoTo selesai
        End If
        'prtidcs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "prtidcs required numeric." : GoTo selesai
        End If
        'prtidrq(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "prtidrq required numeric." : GoTo selesai
        End If
        'prtidbs(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "prtidbs required numeric." : GoTo selesai
        End If
        'prtidpo(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "prtidpo required numeric." : GoTo selesai
        End If
        'prtidipc(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "prtidipc required numeric." : GoTo selesai
        End If
        'prtidgrn(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "prtidgrn required numeric." : GoTo selesai
        End If
        'prtidri(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "prtidri required numeric." : GoTo selesai
        End If
        'prtiddnr(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "prtiddnr required numeric." : GoTo selesai
        End If
        'prtstatus(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "prtstatus required numeric." : GoTo selesai
        End If
        'prtstatussebelumnya(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "prtstatussebelumnya required numeric." : GoTo selesai
        End If
        'prtjmlrevisi(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "prtjmlrevisi required numeric." : GoTo selesai
        End If
        'prtcetakanke(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "prtcetakanke required numeric." : GoTo selesai
        End If
        'prtinputuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "prtinputuser required numeric." : GoTo selesai
        End If
        'prtinputtgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "prtinputtgl required date." : GoTo selesai
        End If
        'prtmodifikasiuser(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "prtmodifikasiuser required numeric." : GoTo selesai
        End If
        'prtmodifikasitgl(70) As DateTime
        If (IsDate(dataUtama(70)) = False) Then
            result(2) = "prtmodifikasitgl required date." : GoTo selesai
        End If
        'prtposting(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "prtposting required numeric." : GoTo selesai
        End If
        'prttutupperiode(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "prttutupperiode required numeric." : GoTo selesai
        End If
        'prtisclose(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "prtisclose required numeric." : GoTo selesai
        End If
        'prtcustomint1(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "prtcustomint1 required numeric." : GoTo selesai
        End If
        'prtcustomint2(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "prtcustomint2 required numeric." : GoTo selesai
        End If
        'prtcustomint3(81) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "prtcustomint3 required numeric." : GoTo selesai
        End If
        'prtcustomdbl1(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "prtcustomdbl1 required numeric." : GoTo selesai
        End If
        'prtcustomdbl2(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "prtcustomdbl2 required numeric." : GoTo selesai
        End If
        'prtcustomdbl3(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "prtcustomdbl3 required numeric." : GoTo selesai
        End If
        'prtcustomdate1(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "prtcustomdate1 required date." : GoTo selesai
        End If
        'prtcustomdate2(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "prtcustomdate2 required date." : GoTo selesai
        End If
        'prtcustomdate3(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "prtcustomdate3 required date." : GoTo selesai
        End If
        'prtjenis(88) As Integer
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "prtjenis required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'prtcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "prtcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "prtcabang should not be more than 25 character." : GoTo selesai
        End If

        'prtlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "prtlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "prtlokasi should not be more than 25 character." : GoTo selesai
        End If

        'prtgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "prtgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "prtgudang should not be more than 25 character." : GoTo selesai
        End If

        'prtsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "prtsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "prtsumber should not be more than 10 character." : GoTo selesai
        End If

        'prtnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "prtnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "prtnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'prttgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "prttgl can't be empty" : GoTo selesai
        End If

        'prttgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "prttgljatuhtempo can't be empty" : GoTo selesai
        End If

        'prttglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "prttglnoref can't be empty" : GoTo selesai
        End If

        'prttglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "prttglpenutupan can't be empty" : GoTo selesai
        End If

        'prtmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "prtmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "prtmatauang should not be more than 25 character." : GoTo selesai
        End If

        'prtkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "prtkurs can't be empty" : GoTo selesai
        End If

        'prttotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "prttotal can't be empty" : GoTo selesai
        End If

        'prtdiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "prtdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "prtdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'prtjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "prtjmldiskon can't be empty" : GoTo selesai
        End If

        'prttotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "prttotalpajak1detail can't be empty" : GoTo selesai
        End If

        'prttotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "prttotalpajak2detail can't be empty" : GoTo selesai
        End If

        'prtbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "prtbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "prtbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'prtbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "prtbiayalain can't be empty" : GoTo selesai
        End If

        'prttotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "prttotaltransaksi can't be empty" : GoTo selesai
        End If

        'prtsisatransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "prtsisatransaksi can't be empty" : GoTo selesai
        End If

        'prtjmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "prtjmlbayar can't be empty" : GoTo selesai
        End If

        'prttgllunas(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "prttgllunas can't be empty" : GoTo selesai
        End If

        'prttglbayarpajak(47) As Date
        If Len(dataUtama(47)) = 0 Then
            result(2) = "prttglbayarpajak can't be empty" : GoTo selesai
        End If

        'prtinputtgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "prtinputtgl can't be empty" : GoTo selesai
        End If

        'prtmodifikasitgl(70) As DateTime
        If Len(dataUtama(70)) = 0 Then
            result(2) = "prtmodifikasitgl can't be empty" : GoTo selesai
        End If

        'prtcustomdbl1(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "prtcustomdbl1 can't be empty" : GoTo selesai
        End If

        'prtcustomdbl2(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "prtcustomdbl2 can't be empty" : GoTo selesai
        End If

        'prtcustomdbl3(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "prtcustomdbl3 can't be empty" : GoTo selesai
        End If

        'prtcustomdate1(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "prtcustomdate1 can't be empty" : GoTo selesai
        End If

        'prtcustomdate2(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "prtcustomdate2 can't be empty" : GoTo selesai
        End If

        'prtcustomdate3(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "prtcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prthargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtiddnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjenis", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "prtid~prtcabang~prtlokasi~prtgudang~prtasalbarang~prtasalbarangkategori~prtjenispembelian~prtjenispembeliankategori~prtcarabayar~prtsumber~prtautonotransaksi~prtnotransaksi~prttgl~prtkodepa~prtsupplier~prtsupplierkontak~prt1alamat1~prt1alamat2~prt1alamat3~prt2alamat1~prt2alamat2~prt2alamat3~prtbagianpembelian~prttermin~prttgljatuhtempo~prturaian~prtcatatan~prtnoref~prttglnoref~prttglpenutupan~prtmatauang~prtkurs~prthargatermasukpajak~prttotal~prtdiskonpersen~prtjmldiskon~prttotalpajak1detail~prttotalpajak2detail~prtbiayalainpersen~prtbiayalain~prttotaltransaksi~prtsisatransaksi~prtjmlbayar~prtstatuslunas~prttgllunas~prtnofakturpajak~prtsdhbayarpajak~prttglbayarpajak~prtrekdiskon~prtrekpajak1~prtrekpajak2~prtrekbiayalain~prtrekbayar~prtreksisa~prtidpr~prtidcs~prtidrq~prtidbs~prtidpo~prtidipc~prtidgrn~prtidri~prtiddnr~prtstatus~prtstatussebelumnya~prtjmlrevisi~prtcetakanke~prtinputuser~prtinputtgl~prtmodifikasiuser~prtmodifikasitgl~prtposting~prttutupperiode~prtisclose~prtcustomtext1~prtcustomtext2~prtcustomtext3~prtcustomtext4~prtcustomtext5~prtcustomint1~prtcustomint2~prtcustomint3~prtcustomdbl1~prtcustomdbl2~prtcustomdbl3~prtcustomdate1~prtcustomdate2~prtcustomdate3~prtjenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idprtdetail(0) As Integer, idprt(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, idhppkhususmasuk(13) As Integer, idhppfifomasuk(14) As Integer, 
        'hpp(15) As Double, harga(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekdiskonpembelian(29) As String, 
        'rekhargapokok(30) As String, rekreturpembelian(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idprdetail(38) As Integer, idcsdetail(39) As Integer, 
        'idrqdetail(40) As Integer, idbsdetail(41) As Integer, idpodetail(42) As Integer, idipcdetail(43) As Integer, idgrndetail(44) As Integer, 
        'idridetail(45) As Integer, iddnrdetail(46) As Integer, isclose(47) As Integer, customtext1(48) As String, customtext2(49) As String, 
        'customtext3(50) As String, customdbl1(51) As Double, customdbl2(52) As Double, customdbl3(53) As Double, customdate1(54) As Date, 
        'customdate2(55) As Date, customdate3(56) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, 
        'idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, 
        'idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idprtdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idprt", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekreturpembelian", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idridetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddnrdetail", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "transbarang", AsEnumTypeData.AsInt64)

        'Variabel ValidasiSimpan
        Dim ftBarang As String = ""

        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = ""
        Dim ftExistOutstandingDNR As String = "", ftOutstandingDNR As String = "", updNilaiDNR As String = "", updFilterDNR As String = ""
        Dim idbarang As Integer = 0, idridetail As Integer = 0, iddnrdetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokBarang As String = "", ftStokBarang As String = ""
        Dim dtCostCenter As New DataTable, vTransBarang As Integer = 1

        'FILTER RI DAN DNR, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRI As String = "", ftDNR As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 57) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idprtdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idprtdetail required numeric." : GoTo selesai
            End If
            'idprt(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idprt required numeric." : GoTo selesai
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
            'idhppkhususmasuk(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'hpp(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'harga(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'idgrndetail(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'idridetail(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - idridetail required numeric." : GoTo selesai
            End If
            'iddnrdetail(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - iddnrdetail required numeric." : GoTo selesai
            End If
            'isclose(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
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

            'hpp(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'harga(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(16) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(17) As String
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(17)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(17) As String
                dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(17).ToString))
            End If

            'jmlpajak1(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudangasal(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(26) As String
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(27) As String
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(27)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            vTransBarang = 1
            'costcenter(32)
            If Len(dataRowDetail(32)) > 0 Then
                sql = "SELECT ccakun FROM m1_cost_center WHERE cckode = '" & FixQuotes(dataRowDetail(32)) & "'"
                dtCostCenter = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtCostCenter.Rows.Count > 0 Then
                    If Len(FxDB(dtCostCenter.Rows(0)(0), "")) > 0 Then
                        vTransBarang = 0
                    End If
                End If
            End If

            If AsDataTableTambahData(dtdetail, "idprtdetail~idprt~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~idhppkhususmasuk~idhppfifomasuk~hpp~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekdiskonpembelian~rekhargapokok~rekreturpembelian~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~idridetail~iddnrdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~transbarang", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & vTransBarang) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(26) As String    , idridetail(45) As Integer      , iddnrdetail(46) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(26) : idridetail = dataRowDetail(45) : iddnrdetail = dataRowDetail(46)

            'ValidasiHppI
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            If idridetail <> 0 Then 'RI
                'CEK RI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRI = IIf(Len(ftRI.ToString) = 0, "", ftRI & " OR ")
                ftRI = String.Concat(ftRI, " (rid.idridetail = " & idridetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingRI = IIf(Len(ftExistOutstandingRI.ToString) = 0, "", ftExistOutstandingRI & " UNION ")
                ftExistOutstandingRI = String.Concat(ftExistOutstandingRI, "SELECT EXISTS(SELECT 1 FROM m4_ri_detail JOIN m4_ri ON idri = riid WHERE idridetail = '" & idridetail & "' AND (ristatus = 2 OR ristatus = 3 OR ristatus = 4 OR ristatus = 7) LIMIT 1) as rowExists, '" & idridetail & "' as idridetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING 
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                ftOutstandingRI = IIf(Len(ftOutstandingRI.ToString) = 0, "", ftOutstandingRI & " OR ")
                ftOutstandingRI = String.Concat(ftOutstandingRI, " (rid.idridetail = " & idridetail & " AND " & Outstanding & " > (rid.jmlbarang - rid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING 
                updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRI)

                '4. SET FILTER UPDATE OUTSTANDING 
                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
            End If

            If iddnrdetail <> 0 Then 'DNR
                'CEK DNR YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftDNR = IIf(Len(ftDNR.ToString) = 0, "", ftDNR & " OR ")
                ftDNR = String.Concat(ftDNR, " (dnrd.iddnrdetail = " & iddnrdetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingDNR = IIf(Len(ftExistOutstandingDNR.ToString) = 0, "", ftExistOutstandingDNR & " UNION ")
                ftExistOutstandingDNR = String.Concat(ftExistOutstandingDNR, "SELECT EXISTS(SELECT 1 FROM m4_dnr_detail JOIN m4_dnr ON iddnr = dnrid WHERE iddnrdetail = '" & iddnrdetail & "' AND (dnrstatus = 2 OR dnrstatus = 3 OR dnrstatus = 4 OR dnrstatus = 7) LIMIT 1) as rowExists, '" & iddnrdetail & "' as iddnrdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING 
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddnrdetail=" & iddnrdetail)
                ftOutstandingDNR = IIf(Len(ftOutstandingDNR.ToString) = 0, "", ftOutstandingDNR & " OR ")
                ftOutstandingDNR = String.Concat(ftOutstandingDNR, " (dnrd.iddnrdetail = " & iddnrdetail & " AND " & Outstanding & " > (dnrd.jmlbarang - dnrd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING 
                updNilaiDNR = String.Concat("WHEN '" & iddnrdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiDNR)

                '4. SET FILTER UPDATE OUTSTANDING 
                updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                updFilterDNR = String.Concat(updFilterDNR, "(iddnrdetail = '" & iddnrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            If vTransBarang = 1 Then
                '1. CEK DATA EXIST STOK KELUAR
                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                '2. CEK JML STOK KELUAR
                Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "' AND transbarang = 1")
                ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                '3. SET NILAI UPDATE STOK KELUAR
                updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                '4. SET NILAI UPDATE STOK M1_ITEM
                Dim stokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND transbarang = 1")
                ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
                updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokKeluar & "', 5) ", updStokBarang)
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

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

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
                dataRowBatch(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

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
                dataRowSerial(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
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
        If dataSplit.Length > 4 Then
            If dataSplit(4).Length > 0 Then

                'VALIDASI DAN SET DATA ASSET ======================================================
                'SPLIT PARAMETER DATA ASSET
                dataAsset = dataSplit(4).Split(sptRow)
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
                    dataRowAsset(2) = 0
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
                vStatus = drutama("prtstatus")
                vTgl = AsFormatTanggal(drutama("prttgl"))


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 13
                Select Case drutama("prtstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("prttgl")), AsFormatTanggal(drutama("prttgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("prtstatus") = 2 Or drutama("prtstatus") = 1 Or drutama("prtstatus") = 8 Or drutama("prtstatus") = 9 Or drutama("prtstatus") = 10 Or drutama("prtstatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'VALIDASI ASSET ----------------------
                    'ValidasiAsset
                    rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI ASSET ---------------

                    'VALIDASI GUDANG ASSET ---------------
                    'ValidasiGudangAsset
                    rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI GUDANG ASSET --------

                    'ValidasiHppI
                    rsValidasi = ValidasiHppI(dtdetail, ftBarang)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    ''ValidasiHppF
                    'rsValidasi = ValidasiHppF(dtdetail, ftBarang)
                    'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingRI, ftOutstandingRI, ftExistOutstandingDNR, ftOutstandingDNR, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit", ftRI, ftDNR, drutama("prthargatermasukpajak"), dtasset)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'FUNGSI SET TANGGAL JATUH TEMPO DIHILANGKAN, KARENA di flex tambah inputan
                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("prttermin").ToString, AsFormatTanggal(drutama("prttgl")), "prttgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("prttgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'SET TANGGAL JATUH TEMPO BERDASARKAN SETTING
                'JIKA SETTING BERDASARKAN TUKAR FAKTUR MAKA TANGGAL JATUH TEMPO DISET 2100-12-31
                Dim setTglJT As String = F_getSetting(4, "tukarfaktur", "UpdateTglJatuhTempoPRT")
                If setTglJT.Equals("1") Then
                    drutama("prttgljatuhtempo") = "2100-12-31"
                End If


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("prttotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("prttotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("prttotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("prthargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("prttotaltransaksi") = Double.Parse(drutama("prttotal")) - Double.Parse(drutama("prtjmldiskon")) + Double.Parse(drutama("prttotalpajak1detail")) + Double.Parse(drutama("prttotalpajak2detail")) + Double.Parse(drutama("prtbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("prttotaltransaksi") = Double.Parse(drutama("prttotal")) - Double.Parse(drutama("prtjmldiskon")) + Double.Parse(drutama("prttotalpajak2detail")) + Double.Parse(drutama("prtbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA RETUR LANGSUNG MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("prtjenis")) = 1 Then
                    drutama("prtjmlbayar") = drutama("prttotaltransaksi")
                    drutama("prttgllunas") = drutama("prttgl")
                    drutama("prtstatuslunas") = 2

                Else
                    drutama("prtjmlbayar") = 0 : drutama("prttgllunas") = "1900-01-01" : drutama("prtstatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("prtid")
                    notransaksi = drutama("prtnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid), prtnotransaksi FROM M4_prt WHERE prtid='" & result(4) & "' AND prtstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("prtautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prtcabang"), drutama("prtlokasi"), drutama("prtsumber"), drutama("prttgl"), drutama("prtsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid) FROM M4_prt WHERE prtnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_prt_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prtsumber")) & "▼" & FixQuotes(drutama("prtid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Prt set prtcabang  = '" & FixQuotes(drutama("prtcabang")) & "', prtlokasi  = '" & FixQuotes(drutama("prtlokasi")) & "', prtgudang  = '" & FixQuotes(drutama("prtgudang")) & "', prtasalbarang  = '" & FixQuotes(drutama("prtasalbarang")) & "', prtasalbarangkategori  = " & drutama("prtasalbarangkategori") & ", prtjenispembelian  = '" & FixQuotes(drutama("prtjenispembelian")) & "', prtjenispembeliankategori  = " & drutama("prtjenispembeliankategori") & ", prtcarabayar  = " & drutama("prtcarabayar") & ", prtsumber  = '" & FixQuotes(drutama("prtsumber")) & "', prtautonotransaksi  = " & drutama("prtautonotransaksi") & ", prtnotransaksi  = '" & FixQuotes(notransaksi) & "', prttgl  = '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', prtkodepa  = " & drutama("prtkodepa") & ", prtsupplier  = " & drutama("prtsupplier") & ", prtsupplierkontak  = '" & FixQuotes(drutama("prtsupplierkontak")) & "', prt1alamat1  = '" & FixQuotes(drutama("prt1alamat1")) & "', prt1alamat2  = '" & FixQuotes(drutama("prt1alamat2")) & "', prt1alamat3  = '" & FixQuotes(drutama("prt1alamat3")) & "', prt2alamat1  = '" & FixQuotes(drutama("prt2alamat1")) & "', prt2alamat2  = '" & FixQuotes(drutama("prt2alamat2")) & "', prt2alamat3  = '" & FixQuotes(drutama("prt2alamat3")) & "', prtbagianpembelian  = " & drutama("prtbagianpembelian") & ", prttermin  = '" & FixQuotes(drutama("prttermin")) & "', prttgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', prturaian  = '" & FixQuotes(drutama("prturaian")) & "', prtcatatan  = '" & FixQuotes(drutama("prtcatatan")) & "', prtnoref  = '" & FixQuotes(drutama("prtnoref")) & "', prttglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', prttglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', prtmatauang  = '" & FixQuotes(drutama("prtmatauang")) & "', prtkurs  = '" & FixDouble(drutama("prtkurs")) & "', prthargatermasukpajak  = " & drutama("prthargatermasukpajak") & ", prttotal  = '" & FixDouble(drutama("prttotal")) & "', prtdiskonpersen  = '" & FixQuotes(drutama("prtdiskonpersen")) & "', prtjmldiskon  = '" & FixDouble(drutama("prtjmldiskon")) & "', prttotalpajak1detail  = '" & FixDouble(drutama("prttotalpajak1detail")) & "', prttotalpajak2detail  = '" & FixDouble(drutama("prttotalpajak2detail")) & "', prtbiayalainpersen  = '" & FixQuotes(drutama("prtbiayalainpersen")) & "', prtbiayalain  = '" & FixDouble(drutama("prtbiayalain")) & "', prttotaltransaksi  = '" & FixDouble(drutama("prttotaltransaksi")) & "', prtsisatransaksi  = '" & FixDouble(drutama("prtsisatransaksi")) & "', prtjmlbayar  = '" & FixDouble(drutama("prtjmlbayar")) & "', prtstatuslunas  = " & drutama("prtstatuslunas") & ", prttgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', prtnofakturpajak  = '" & FixQuotes(drutama("prtnofakturpajak")) & "', prtsdhbayarpajak  = " & drutama("prtsdhbayarpajak") & ", prttglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', prtrekdiskon  = '" & FixQuotes(drutama("prtrekdiskon")) & "', prtrekpajak1  = '" & FixQuotes(drutama("prtrekpajak1")) & "', prtrekpajak2  = '" & FixQuotes(drutama("prtrekpajak2")) & "', prtrekbiayalain  = '" & FixQuotes(drutama("prtrekbiayalain")) & "', prtrekbayar  = '" & FixQuotes(drutama("prtrekbayar")) & "', prtreksisa  = '" & FixQuotes(drutama("prtreksisa")) & "', prtidpr  = " & drutama("prtidpr") & ", prtidcs  = " & drutama("prtidcs") & ", prtidrq  = " & drutama("prtidrq") & ", prtidbs  = " & drutama("prtidbs") & ", prtidpo  = " & drutama("prtidpo") & ", prtidipc  = " & drutama("prtidipc") & ", prtidgrn  = " & drutama("prtidgrn") & ", prtidri  = " & drutama("prtidri") & ", prtiddnr  = " & drutama("prtiddnr") & ", prtstatus  = " & drutama("prtstatus") & ", prtstatussebelumnya  = " & drutama("prtstatussebelumnya") & ", prtjmlrevisi  = prtjmlrevisi+1, prtcetakanke  = " & drutama("prtcetakanke") & ", prtmodifikasiuser  = " & drutama("prtmodifikasiuser") & ", prtmodifikasitgl  = NOW(), prtposting  = 0, prttutupperiode  = " & drutama("prttutupperiode") & ", prtcustomtext1  = '" & FixQuotes(drutama("prtcustomtext1")) & "', prtcustomtext2  = '" & FixQuotes(drutama("prtcustomtext2")) & "', prtcustomtext3  = '" & FixQuotes(drutama("prtcustomtext3")) & "', prtcustomtext4  = '" & FixQuotes(drutama("prtcustomtext4")) & "', prtcustomtext5  = '" & FixQuotes(drutama("prtcustomtext5")) & "', prtcustomint1  = " & drutama("prtcustomint1") & ", prtcustomint2  = " & drutama("prtcustomint2") & ", prtcustomint3  = " & drutama("prtcustomint3") & ", prtcustomdbl1  = '" & FixDouble(drutama("prtcustomdbl1")) & "', prtcustomdbl2  = '" & FixDouble(drutama("prtcustomdbl2")) & "', prtcustomdbl3  = '" & FixDouble(drutama("prtcustomdbl3")) & "', prtcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', prtcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', prtcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', prtjenis = '" & FixQuotes(drutama("prtjenis")) & "' where prtid = '" & drutama("prtid") & "'"
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

                    If drutama("prtautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prtcabang"), drutama("prtlokasi"), drutama("prtsumber"), drutama("prttgl"), drutama("prtsumber"), 4)
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
                        notransaksi = drutama("prtnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid) FROM m4_prt WHERE prtnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Prt (prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis) values('" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(drutama("prtgudang")) & "', '" & FixQuotes(drutama("prtasalbarang")) & "', " & drutama("prtasalbarangkategori") & ", '" & FixQuotes(drutama("prtjenispembelian")) & "', " & drutama("prtjenispembeliankategori") & ", " & drutama("prtcarabayar") & ", '" & FixQuotes(drutama("prtsumber")) & "', " & drutama("prtautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtkodepa") & ", " & drutama("prtsupplier") & ", '" & FixQuotes(drutama("prtsupplierkontak")) & "', '" & FixQuotes(drutama("prt1alamat1")) & "', '" & FixQuotes(drutama("prt1alamat2")) & "', '" & FixQuotes(drutama("prt1alamat3")) & "', '" & FixQuotes(drutama("prt2alamat1")) & "', '" & FixQuotes(drutama("prt2alamat2")) & "', '" & FixQuotes(drutama("prt2alamat3")) & "', " & drutama("prtbagianpembelian") & ", '" & FixQuotes(drutama("prttermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(drutama("prtnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', '" & FixQuotes(drutama("prtmatauang")) & "', '" & FixDouble(drutama("prtkurs")) & "', " & drutama("prthargatermasukpajak") & ", '" & FixDouble(drutama("prttotal")) & "', '" & FixQuotes(drutama("prtdiskonpersen")) & "', '" & FixDouble(drutama("prtjmldiskon")) & "', '" & FixDouble(drutama("prttotalpajak1detail")) & "', '" & FixDouble(drutama("prttotalpajak2detail")) & "', '" & FixQuotes(drutama("prtbiayalainpersen")) & "', '" & FixDouble(drutama("prtbiayalain")) & "', '" & FixDouble(drutama("prttotaltransaksi")) & "', '" & FixDouble(drutama("prtsisatransaksi")) & "', '" & FixDouble(drutama("prtjmlbayar")) & "', " & drutama("prtstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', '" & FixQuotes(drutama("prtnofakturpajak")) & "', " & drutama("prtsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', '" & FixQuotes(drutama("prtrekdiskon")) & "', '" & FixQuotes(drutama("prtrekpajak1")) & "', '" & FixQuotes(drutama("prtrekpajak2")) & "', '" & FixQuotes(drutama("prtrekbiayalain")) & "', '" & FixQuotes(drutama("prtrekbayar")) & "', '" & FixQuotes(drutama("prtreksisa")) & "', " & drutama("prtidpr") & ", " & drutama("prtidcs") & ", " & drutama("prtidrq") & ", " & drutama("prtidbs") & ", " & drutama("prtidpo") & ", " & drutama("prtidipc") & ", " & drutama("prtidgrn") & ", " & drutama("prtidri") & ", " & drutama("prtiddnr") & ", " & drutama("prtstatus") & ", " & drutama("prtstatussebelumnya") & ", " & drutama("prtjmlrevisi") & ", " & drutama("prtcetakanke") & ", " & drutama("prtinputuser") & ", NOW(), " & drutama("prtmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("prttutupperiode") & ", " & drutama("prtisclose") & ", '" & FixQuotes(drutama("prtcustomtext1")) & "', '" & FixQuotes(drutama("prtcustomtext2")) & "', '" & FixQuotes(drutama("prtcustomtext3")) & "', '" & FixQuotes(drutama("prtcustomtext4")) & "', '" & FixQuotes(drutama("prtcustomtext5")) & "', " & drutama("prtcustomint1") & ", " & drutama("prtcustomint2") & ", " & drutama("prtcustomint3") & ", '" & FixDouble(drutama("prtcustomdbl1")) & "', '" & FixDouble(drutama("prtcustomdbl2")) & "', '" & FixDouble(drutama("prtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', '" & FixQuotes(drutama("prtjenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select prtid from M4_prt where prtnotransaksi='" & notransaksi & "' AND prtinputuser= '" & userid & "' order by prtmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Prt_Detail where idprt = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idprtdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekreturpembelian")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", " & dr1("idridetail") & ", " & dr1("iddnrdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Prt_Detail(idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'VALIDASI KETIKA PRT LANGSUNG (PRTJENIS = 1) MAKA TIDAK BOLEH AMBIL LEBIH DARI 1 NOMOR RI
                Dim IdRI As Double = 0
                If drutama("prtjenis") = 1 Then
                    sql = "SELECT ri.riid, ri.rinotransaksi, ri.ritotaltransaksi, ri.rijmlbayar FROM M4_Prt_detail Prtd JOIN M4_ri_detail rid ON Prtd.idridetail = rid.idridetail JOIN M4_ri ri ON rid.idri = ri.riid WHERE Prtd.idPrt = '" & result(4) & "' GROUP BY ri.riid"
                    Dim dtCekRi As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtCekRi.Rows.Count > 1 Then
                        result(2) = "Direct PRT (Purchase Retur) can only pick from one RI (Receive Invoice) transaction." : Trans.Rollback() : GoTo selesai

                    ElseIf dtCekRi.Rows.Count = 1 Then
                        'VALIDASI KETIKA PRT LANGSUNG (PRTJENIS = 1) MAKA TOTAL TRANSAKSI PRT TIDAK BOLEH MELEBIHI SISA RI YANG BELUM DIBAYAR
                        If Len(dtCekRi.Rows(0)("Riid")) > 0 Then
                            IdRI = Double.Parse(dtCekRi.Rows(0)("Riid"))
                            If Double.Parse(drutama("Prttotaltransaksi")) > (Double.Parse(dtCekRi.Rows(0)("Ritotaltransaksi")) - Double.Parse(dtCekRi.Rows(0)("Rijmlbayar"))) Then
                                Dim selisih(2) As String
                                selisih = F_Nominal(F_Round((Double.Parse(dtCekRi.Rows(0)("Ritotaltransaksi")) - Double.Parse(dtCekRi.Rows(0)("Rijmlbayar")))), True).Split(sptSubParam)

                                result(2) = "Total Direct PRT (Purchase Retur) exceeds the AP (Account Payables) from RI (Receive Invoice) transaction no. " & dtCekRi.Rows(0)("Rinotransaksi") & ". AP available : " & drutama("Prtmatauang") & " " & selisih(1) : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                    End If
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'PRT'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'PRT'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'PRT'"
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


                If drutama("prtstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiRI) > 0 Then 'RI
                        'UPDATE DETAIL
                        sql = "UPDATE m4_ri_detail SET jmlrealisasi = (CASE idridetail " & updNilaiRI & " ELSE jmlrealisasi END) WHERE " & updFilterRI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idri FROM M4_ri_detail WHERE " & updFilterRI & " GROUP BY idri", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_ri_detail WHERE " & ftDetail & " GROUP BY idri", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRI = "" : updFilterRI = ""
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
                                updNilaiRI = String.Concat(updNilaiRI, "WHEN '" & dr1("idri") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(riid = '" & dr1("idri") & "')")
                            Next

                            sql = "UPDATE m4_ri SET ristatusrealisasi = (CASE riid " & updNilaiRI & " ELSE ristatusrealisasi END) WHERE " & updFilterRI
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

                    If Len(updNilaiDNR) > 0 Then 'DNR
                        'UPDATE DETAIL
                        sql = "UPDATE m4_dnr_detail SET jmlrealisasi = (CASE iddnrdetail " & updNilaiDNR & " ELSE jmlrealisasi END) WHERE " & updFilterDNR
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT iddnr FROM m4_dnr_detail WHERE " & updFilterDNR & " GROUP BY iddnr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(iddnr = '" & dr1("iddnr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT iddnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_dnr_detail WHERE " & ftDetail & " GROUP BY iddnr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiDNR = "" : updFilterDNR = ""
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
                                updNilaiDNR = String.Concat(updNilaiDNR, "WHEN '" & dr1("iddnr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                                updFilterDNR = String.Concat(updFilterDNR, "(dnrid = '" & dr1("iddnr") & "')")
                            Next

                            sql = "UPDATE m4_dnr SET dnrstatusrealisasi = (CASE dnrid " & updNilaiDNR & " ELSE dnrstatusrealisasi END) WHERE " & updFilterDNR
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                    'END OF INSERT NO SERIAL ========================================================


                    'DELETE NO ASSET ===============================================================
                    If dtasset.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtasset.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "DELETE a FROM m7_asset a WHERE a.aid IN(" & strValue2.ToString & ")"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF DELETE NO ASSET ========================================================


                    'JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI =========================
                    If drutama("prtjenis") = 1 And IdRI = 0 Then
                        IdRI = drutama("prtidri")
                    End If
                    If drutama("prtjenis") = 1 And IdRI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET ri.rijmlbayar = ri.rijmlbayar + " & Double.Parse(drutama("prttotaltransaksi")) & ", ri.ritgllunas = (CASE WHEN ri.rijmlbayar + " & Double.Parse(drutama("prttotaltransaksi")) & " >= ri.ritotaltransaksi THEN '" & AsFormatTanggal(FixQuotes(drutama("prttgl"))) & "' ELSE ri.ritgllunas END) WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI ==================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT prtd.idprtdetail, prtd.idbarang, prtd.namabarang, prtd.tipebarang, prtd.jml, prtd.satuan, prtd.jmlbarang, prtd.satuanbarang, prtd.matauang, prtd.kurs, prtd.harga, prtd.diskon, prtd.jmldiskon, prtd.idhppkhususmasuk, prtd.hpp, prtd.gudangasal, prtd.gudangtransit, prtd.gudangtujuan, prtd.catatan, prtd.costcenter, prtd.divisi, prtd.subdivisi, prtd.proyek, prt.prtinputtgl, i.bhpp FROM m4_prt_detail prtd JOIN m4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid WHERE prtd.idprt = '" & result(4) & "'", myConn)
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT prtd.idprtdetail, prtd.idbarang, prtd.namabarang, prtd.tipebarang, prtd.jml, prtd.satuan, prtd.jmlbarang, prtd.satuanbarang, prtd.matauang, prtd.kurs, prtd.harga, prtd.diskon, prtd.jmldiskon, prtd.idhppkhususmasuk, prtd.hpp, prtd.gudangasal, prtd.gudangtransit, prtd.gudangtujuan, prtd.catatan, prtd.costcenter, prtd.divisi, prtd.subdivisi, prtd.proyek, prt.prtinputtgl, i.bhpp, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_prt_detail prtd JOIN m4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid LEFT JOIN m1_cost_center cc ON prtd.costcenter = cc.cckode WHERE prtd.idprt = '" & result(4) & "'", myConn)

                    Dim hpp As Double = 0, postinghpp As Double = 0, gudang As String = "", bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ====================================================
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            If Double.Parse(dr1("transbarang")) = 1 Then
                                'SET NILAI VARIABEL
                                idbarang = Double.Parse(dr1("idbarang"))
                                jmlbarang = Double.Parse(dr1("jmlbarang"))
                                gudang = dr1("gudangtransit")

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
                                    'mapping                        id,                            cabang,                                    lokasi,                                    gudang,                         kodepa,             jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                    strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("prtkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("prtsumber")) & "', " & result(4) & ", " & dr1("idprtdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("prtinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("prtinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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
                            End If

                        Next
                        'END OF INSERT ITEM TRANSACTION =============================================

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                End If


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PRT", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("prtstatus") = 2 Then
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


                'INSERT MSMQ HPP ====================================================================
                If drutama("prtstatus") = 2 Then
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
                    Dim ProsesHpp As String = F_getSetting(0, "accounting", "ProsesHpp")
                    If ProsesHpp.Equals("0") = False Then
                        hasilMsmq = SendMsmq(dirMsmq, "C", mjid, sumber, result(4), userid)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : Trans.Rollback() : GoTo selesai
                        End If
                    End If

                End If
                'END OF INSERT MSMQ HPP =============================================================


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
    Public Function M4_PrtUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "PRT", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim prtjenis As Integer = 0, prttotaltransaksi As Double = 0

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT prttgl, prtnotransaksi, prtstatus, prtjenis, prttotaltransaksi FROM M4_Prt WHERE Prtid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'prtjenis                                        'prttotaltransaksi
                prtjenis = Integer.Parse(dtdetail.Rows(1)(3)) : prttotaltransaksi = Double.Parse(dtdetail.Rows(1)(4))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Prtstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_prt_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_prt_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idridetail As Integer = 0, iddnrdetail As Integer = 0, idhppkhususmasuk As Integer = 0
                Dim updNilaiRI As String = "", updFilterRI As String = "", updNilaiDNR As String = "", updFilterDNR As String = ""
                Dim gudangIn As String = "", updStokIn As String = "", updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""

                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idprtdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idhppkhususmasuk, gudangtransit, gudangtujuan, idridetail, iddnrdetail, urutan FROM m4_prt_detail WHERE idprt = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idprtdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idhppkhususmasuk, gudangtransit, gudangtujuan, idridetail, iddnrdetail, urutan, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_prt_detail prtd LEFT JOIN m1_cost_center cc ON prtd.costcenter = cc.cckode WHERE idprt = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idhppkhususmasuk = dr1("idhppkhususmasuk") : gudangIn = dr1("gudangtransit") : idridetail = dr1("idridetail") : iddnrdetail = dr1("iddnrdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idridetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING RI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                            updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRI)
                            '2. SET FILTERUPDATE OUTSTANDING RI
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
                        End If

                        If iddnrdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING DNR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddnrdetail=" & iddnrdetail)
                            updNilaiDNR = String.Concat("WHEN '" & iddnrdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiDNR)
                            '2. SET FILTERUPDATE OUTSTANDING DNR
                            updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                            updFilterDNR = String.Concat(updFilterDNR, "(iddnrdetail = '" & iddnrdetail & "')")
                        End If

                        If Double.Parse(dr1("transbarang")) = 1 Then
                            'SET NILAI UPDATE STOK MASUK --------------
                            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                            'END OF BUAT FILTER UNTUK UPDATE --------------------------

                            '4. BUAT FILTER UPDATE HPP KHUSUS (I)
                            If idhppkhususmasuk <> 0 Then
                                'SET NILAI UPDATE HPP KHUSUS IN
                                Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                                updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN ROUND(jmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppI)
                                'SET FILTER UPDATE HPP KHUSUS IN
                                updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                                updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")
                                'SET FILTER DELETE HPP KHUSUS OUT
                                delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                                delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'PRT' AND idtransaksi = '" & dr1("idprtdetail") & "')")
                            End If

                            '5. BUAT FILTER UPDATE HPP FIFO (F)
                            filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                            filterHppF = String.Concat(filterHppF, "(cfosumber = 'PRT' AND cfoidtransaksi = '" & dr1("idprtdetail") & "')")

                            '6 SET NILAI UPDATE STOK BARANG
                            Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & stokBarang & "', 5) ", updStokBarang)
                            '7. SET FILTERUPDATE STOK BARANG
                            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                            ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")
                        End If

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
                        delFilterHppF = String.Concat(delFilterHppF, "(cfosumber = 'PRT' AND cfoidtransaksi = '" & dr1("cfoidtransaksi") & "')")
                        'SET NILAI UPDATE HPP FIFO IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtHppF, "cfojmlkeluar", "cfoidcfi='" & idhppfifoin & "'")
                        updNilaiHppF = String.Concat("WHEN '" & idhppfifoin & "' THEN ROUND(cfijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppF)
                        'SET FILTER UPDATE HPP FIFO IN
                        updFilterHppF = IIf(Len(updFilterHppF.ToString) = 0, "", updFilterHppF & " OR ")
                        updFilterHppF = String.Concat(updFilterHppF, "(cfiid = '" & idhppfifoin & "')")
                    Next
                End If
                'END OF CEK HPP FIFO =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterRI) > 0 Then 'RI
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_ri_detail SET jmlrealisasi = (CASE idridetail " & updNilaiRI & " ELSE jmlrealisasi END) WHERE " & updFilterRI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idri FROM M4_ri_detail WHERE " & updFilterRI & " GROUP BY idri", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_ri_detail WHERE " & ftDetail & " GROUP BY idri", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRI = "" : updFilterRI = ""
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
                            updNilaiRI = String.Concat(updNilaiRI, "WHEN '" & dr1("idri") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(riid = '" & dr1("idri") & "')")
                        Next

                        sql = "UPDATE m4_ri SET ristatusrealisasi = (CASE riid " & updNilaiRI & " ELSE ristatusrealisasi END) WHERE " & updFilterRI
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

                If Len(updFilterDNR) > 0 Then 'DNR
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_dnr_detail SET jmlrealisasi = (CASE iddnrdetail " & updNilaiDNR & " ELSE jmlrealisasi END) WHERE " & updFilterDNR
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT iddnr FROM m4_dnr_detail WHERE " & updFilterDNR & " GROUP BY iddnr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(iddnr = '" & dr1("iddnr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT iddnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_dnr_detail WHERE " & ftDetail & " GROUP BY iddnr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiDNR = "" : updFilterDNR = ""
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
                            updNilaiDNR = String.Concat(updNilaiDNR, "WHEN '" & dr1("iddnr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                            updFilterDNR = String.Concat(updFilterDNR, "(dnrid = '" & dr1("iddnr") & "')")
                        Next

                        sql = "UPDATE m4_dnr SET dnrstatusrealisasi = (CASE dnrid " & updNilaiDNR & " ELSE dnrstatusrealisasi END) WHERE " & updFilterDNR
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


                'JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI =========================
                If prtjenis = 1 Then
                    'AMBIL IDRI DARI DATA PRT DETAIL
                    sql = "SELECT rid.idri FROM m4_prt_detail prtd JOIN m4_ri_detail rid ON prtd.idridetail = rid.idridetail WHERE prtd.idprt = '" & idtransaksi & "' GROUP BY rid.idri"
                    Dim dtRI As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim IdRI As Double = 0
                    If dtRI.Rows.Count > 0 Then
                        If Len(dtRI.Rows(0)("idri")) > 0 Then
                            IdRI = Double.Parse(dtRI.Rows(0)("idri"))
                        End If
                    End If
                    If IdRI = 0 Then
                        sql = "SELECT prt.prtidri FROM m4_prt prt WHERE prt.prtid = '" & idtransaksi & "' GROUP BY prt.prtidri"
                        dtRI = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtRI.Rows.Count > 0 Then
                            If Len(dtRI.Rows(0)("prtidri")) > 0 Then
                                IdRI = Double.Parse(dtRI.Rows(0)("prtidri"))
                            End If
                        End If
                    End If

                    'UPDATE JMLBAYAR RI
                    If IdRI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET ri.rijmlbayar = ri.rijmlbayar - " & prttotaltransaksi & ", ri.ritgllunas = '" & FixQuotes("1900-01-01") & "' WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE ri.riid = '" & IdRI & "'"
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
                'END OF JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI ==================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDBCon("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'", myConn)
                If dtSerial.Rows.Count > 0 Then
                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


                'INSERT NO ASSET ===============================================================
                Dim dtasset As DataTable = AsDataTableAmbilDariDBCon("SELECT * FROM m7_asset_transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "'", myConn)
                If dtasset.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & FixQuotes(dr1("atasetid")) & "', '" & FixQuotes(dr1("atkode")) & "', '" & FixQuotes(dr1("atnama")) & "', '" & FixQuotes(dr1("atkategori")) & "', '" & FixQuotes(dr1("atcabang")) & "', '" & FixQuotes(dr1("atlokasi")) & "', '" & FixQuotes(dr1("atgudang")) & "', '" & FixQuotes(dr1("atdivisi")) & "', '" & FixQuotes(dr1("atsubdivisi")) & "', '" & FixQuotes(dr1("atcostcenter")) & "', '" & FixQuotes(dr1("atproyek")) & "', '" & FixQuotes(dr1("atcatatan")) & "', '" & FixQuotes(dr1("atnomor")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglbeli"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpakai"))) & "', '" & FixDouble(dr1("atjml")) & "', '" & FixQuotes(dr1("atsatuan")) & "', '" & FixQuotes(dr1("atmatauang")) & "', '" & FixDouble(dr1("atkurs")) & "', '" & FixDouble(dr1("atharga")) & "', '" & FixQuotes(dr1("atdiskon")) & "', '" & FixDouble(dr1("atjmldiskon")) & "', '" & FixQuotes(dr1("atpajak1")) & "', '" & FixDouble(dr1("atjmlpajak1")) & "', '" & FixQuotes(dr1("atpajak2")) & "', '" & FixDouble(dr1("atjmlpajak2")) & "', '" & FixDouble(dr1("athargabeli")) & "', '" & FixDouble(dr1("atnilairesidu")) & "', '" & FixDouble(dr1("atumurekonomis")) & "', '" & FixDouble(dr1("atbebanperbln")) & "', '" & FixDouble(dr1("atakumulasibeban")) & "', '" & FixDouble(dr1("atnilaibuku")) & "', " & dr1("atmetode") & ", '" & FixQuotes(dr1("attabelpenyusutan")) & "', " & dr1("atintangible") & ", " & dr1("atfiskal") & ", " & dr1("atatastengahbulan") & ", '" & FixQuotes(dr1("atrekasset")) & "', '" & FixQuotes(dr1("atrekakumdepresiasi")) & "', '" & FixQuotes(dr1("atrekdepresiasi")) & "', '" & FixQuotes(dr1("atrekpenghapusan")) & "', '" & FixQuotes(dr1("atprodusen")) & "', '" & FixQuotes(AsFormatTanggal(dr1("attglpensiun"))) & "', '" & FixDouble(dr1("atpenyusutanke")) & "', '" & FixDouble(dr1("atnilaimenurun")) & "', " & dr1("atdispose") & ", " & dr1("atpembelian") & ", " & dr1("atpenjualan") & ", " & dr1("atlocked") & ", " & dr1("atstatus") & ", " & dr1("atstatussebelumnya") & ", " & dr1("atisclose") & ", '" & FixQuotes(dr1("atinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("atcustomtext1")) & "', '" & FixQuotes(dr1("atcustomtext2")) & "', '" & FixQuotes(dr1("atcustomtext3")) & "', '" & FixQuotes(dr1("atcustomtext4")) & "', '" & FixQuotes(dr1("atcustomtext5")) & "', " & dr1("atcustomint1") & ", " & dr1("atcustomint2") & ", " & dr1("atcustomint3") & ", " & dr1("atcustomint4") & ", " & dr1("atcustomint5") & ", '" & FixDouble(dr1("atcustomdbl1")) & "', '" & FixDouble(dr1("atcustomdbl2")) & "', '" & FixDouble(dr1("atcustomdbl3")) & "', '" & FixDouble(dr1("atcustomdbl4")) & "', '" & FixDouble(dr1("atcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("atcustomdate5"))) & "', '" & FixQuotes(dr1("atidbarang")) & "')")
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
                'END OF UPDATE HPP FIFO (F) ====================================================


                'UPDATE STOK ====================================================================
                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT prtd.idbarang, ROUND(SUM(prtd.jmlbarang * prtd.hpp),2) as nilai, SUM(prtd.jmlbarang) as jumlah"
                sql &= " FROM m4_prt_detail prtd"
                sql &= " WHERE prtd.jmlbarang <> 0 AND prtd.idprt = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY prtd.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2),0) END)"
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PRT' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Prt SET Prtstatus = " & nilaiStatus & ", Prtmodifikasiuser='" & userid & "', Prtmodifikasitgl = NOW(), Prtposting = 0, Prtpostingtgl = '1971-01-01 00:00:00', Prtjmlrevisi = Prtjmlrevisi + 1 WHERE Prtid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "PRT", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Prtid, Prtnotransaksi FROM M4_Prt WHERE Prtid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT prtcabang, prtlokasi, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl"
            sql &= " FROM M4_prt"
            sql &= " WHERE prtid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("prtcabang")
                lokasi = dtNomorNext.Rows(0)("prtlokasi")
                sumber = dtNomorNext.Rows(0)("prtsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("prtautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("prtnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("prttgl"))
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


            'DELETE DETAIL
            sql = "DELETE FROM M4_Prt_Detail WHERE idprt='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Prt WHERE prtid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtGetdataById(ByVal param As String) As String

        'M4_PrtGetdataById Utama --------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, 
        'prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, 
        'prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, 
        'prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, 
        'prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, kpkp

        'M4_PrtGetdataById Detail -------------------------------------------------------
        'idprtdetail, idprt, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, 
        'rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, 
        'idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_PrtGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_PrtGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_PrtGetdataById Asset --------------------------------------------------------
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

        Dim utama As String = "", detail As String = "", batch As String = "", serial As String = "", idtransaksi As String = ""
        Dim sumber As String = "PRT", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Prt~M4_Prt_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "prtid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "prtid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_prt_getdata")
        sql = "select `prt`.`prtid` AS `prtid`,`prt`.`prtcabang` AS `prtcabang`,`prt`.`prtlokasi` AS `prtlokasi`,`prt`.`prtgudang` AS `prtgudang`,`prt`.`prtasalbarang` AS `prtasalbarang`,`prt`.`prtasalbarangkategori` AS `prtasalbarangkategori`,`prt`.`prtjenispembelian` AS `prtjenispembelian`,`prt`.`prtjenispembeliankategori` AS `prtjenispembeliankategori`,`prt`.`prtcarabayar` AS `prtcarabayar`,`prt`.`prtsumber` AS `prtsumber`,`prt`.`prtautonotransaksi` AS `prtautonotransaksi`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`prt`.`prttgl` AS `prttgl`,`prt`.`prtkodepa` AS `prtkodepa`,`prt`.`prtsupplier` AS `prtsupplier`,`prt`.`prtsupplierkontak` AS `prtsupplierkontak`,`prt`.`prt1alamat1` AS `prt1alamat1`,`prt`.`prt1alamat2` AS `prt1alamat2`,`prt`.`prt1alamat3` AS `prt1alamat3`,`prt`.`prt2alamat1` AS `prt2alamat1`,`prt`.`prt2alamat2` AS `prt2alamat2`,`prt`.`prt2alamat3` AS `prt2alamat3`,`prt`.`prtbagianpembelian` AS `prtbagianpembelian`,`prt`.`prttermin` AS `prttermin`,`prt`.`prttgljatuhtempo` AS `prttgljatuhtempo`,`prt`.`prturaian` AS `prturaian`,`prt`.`prtcatatan` AS `prtcatatan`,`prt`.`prtnoref` AS `prtnoref`,`prt`.`prttglnoref` AS `prttglnoref`,`prt`.`prttglpenutupan` AS `prttglpenutupan`,`prt`.`prtmatauang` AS `prtmatauang`,`prt`.`prtkurs` AS `prtkurs`,`prt`.`prthargatermasukpajak` AS `prthargatermasukpajak`,`prt`.`prttotal` AS `prttotal`,`prt`.`prtdiskonpersen` AS `prtdiskonpersen`,`prt`.`prtjmldiskon` AS `prtjmldiskon`,`prt`.`prttotalpajak1detail` AS `prttotalpajak1detail`,`prt`.`prttotalpajak2detail` AS `prttotalpajak2detail`,`prt`.`prtbiayalainpersen` AS `prtbiayalainpersen`,`prt`.`prtbiayalain` AS `prtbiayalain`,`prt`.`prttotaltransaksi` AS `prttotaltransaksi`,`prt`.`prtsisatransaksi` AS `prtsisatransaksi`,`prt`.`prtjmlbayar` AS `prtjmlbayar`,`prt`.`prtstatuslunas` AS `prtstatuslunas`,`prt`.`prttgllunas` AS `prttgllunas`,`prt`.`prtnofakturpajak` AS `prtnofakturpajak`,`prt`.`prtsdhbayarpajak` AS `prtsdhbayarpajak`,`prt`.`prttglbayarpajak` AS `prttglbayarpajak`,`prt`.`prtrekdiskon` AS `prtrekdiskon`,`prt`.`prtrekpajak1` AS `prtrekpajak1`,`prt`.`prtrekpajak2` AS `prtrekpajak2`,`prt`.`prtrekbiayalain` AS `prtrekbiayalain`,`prt`.`prtrekbayar` AS `prtrekbayar`,`prt`.`prtreksisa` AS `prtreksisa`,`prt`.`prtidpr` AS `prtidpr`,`prt`.`prtidcs` AS `prtidcs`,`prt`.`prtidrq` AS `prtidrq`,`prt`.`prtidbs` AS `prtidbs`,`prt`.`prtidpo` AS `prtidpo`,`prt`.`prtidipc` AS `prtidipc`,`prt`.`prtidgrn` AS `prtidgrn`,`prt`.`prtidri` AS `prtidri`,`prt`.`prtiddnr` AS `prtiddnr`,`prt`.`prtstatus` AS `prtstatus`,`prt`.`prtstatussebelumnya` AS `prtstatussebelumnya`,`prt`.`prtjmlrevisi` AS `prtjmlrevisi`,`prt`.`prtcetakanke` AS `prtcetakanke`,`prt`.`prtinputuser` AS `prtinputuser`,`prt`.`prtinputtgl` AS `prtinputtgl`,`prt`.`prtmodifikasiuser` AS `prtmodifikasiuser`,`prt`.`prtmodifikasitgl` AS `prtmodifikasitgl`,`prt`.`prtposting` AS `prtposting`,`prt`.`prtpostingtgl` AS `prtpostingtgl`,`prt`.`prttutupperiode` AS `prttutupperiode`,`prt`.`prtisclose` AS `prtisclose`,`prt`.`prtcustomtext1` AS `prtcustomtext1`,`prt`.`prtcustomtext2` AS `prtcustomtext2`,`prt`.`prtcustomtext3` AS `prtcustomtext3`,`prt`.`prtcustomtext4` AS `prtcustomtext4`,`prt`.`prtcustomtext5` AS `prtcustomtext5`,`prt`.`prtcustomint1` AS `prtcustomint1`,`prt`.`prtcustomint2` AS `prtcustomint2`,`prt`.`prtcustomint3` AS `prtcustomint3`,`prt`.`prtcustomdbl1` AS `prtcustomdbl1`,`prt`.`prtcustomdbl2` AS `prtcustomdbl2`,`prt`.`prtcustomdbl3` AS `prtcustomdbl3`,`prt`.`prtcustomdate1` AS `prtcustomdate1`,`prt`.`prtcustomdate2` AS `prtcustomdate2`,`prt`.`prtcustomdate3` AS `prtcustomdate3`,`br`.`bnama` AS `prtcabangnama`,`lc`.`lnama` AS `prtlokasinama`,`wh`.`wnama` AS `prtgudangnama`,`c1`.`kkode` AS `prtsupplierkode`,`c1`.`knama` AS `prtsuppliernama`,`c2`.`kkode` AS `prtbagianpembeliankode`,`c2`.`knama` AS `prtbagianpembeliannama`,`tr`.`trnama` AS `prtterminnama`,`tr`.`trharijatuhtempo` AS `prtterminharijatuhtempo`,`coa1`.`cnama` AS `prtrekdiskonnama`,`coa2`.`cnama` AS `prtrekpajak1nama`,`coa3`.`cnama` AS `prtrekpajak2nama`,`coa4`.`cnama` AS `prtrekbiayalainnama`,`coa5`.`cnama` AS `prtrekbayarnama`,`coa6`.`cnama` AS `prtreksisanama`,`ri`.`rinotransaksi` AS `prtnotransaksiri`,`dnr`.`dnrnotransaksi` AS `prtnotransaksidnr`,`st1`.`nama` AS `prtstatusnama`,`st2`.`nama` AS `prtstatussebelumnyanama`,`u1`.`unama` AS `prtinputusernama`,`u2`.`unama` AS `prtmodifikasiusernama`, prt.prtjenis, `prtd`.`idprtdetail` AS `idprtdetail`,`prtd`.`idprt` AS `idprt`,`prtd`.`idbarang` AS `idbarang`,`prtd`.`namabarang` AS `namabarang`,`prtd`.`tipebarang` AS `tipebarang`,`prtd`.`jml` AS `jml`,`prtd`.`satuan` AS `satuan`,`prtd`.`nilaisatuan` AS `nilaisatuan`,`prtd`.`jmlbarang` AS `jmlbarang`,`prtd`.`satuanbarang` AS `satuanbarang`,`prtd`.`matauang` AS `matauang`,`prtd`.`kurs` AS `kurs`,`prtd`.`hargafix` AS `hargafix`,`prtd`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`prtd`.`idhppfifomasuk` AS `idhppfifomasuk`,`prtd`.`hpp` AS `hpp`,`prtd`.`harga` AS `harga`,`prtd`.`diskon` AS `diskon`,`prtd`.`jmldiskon` AS `jmldiskon`,`prtd`.`pajak1` AS `pajak1`,`prtd`.`jmlpajak1` AS `jmlpajak1`,`prtd`.`pajak2` AS `pajak2`,`prtd`.`jmlpajak2` AS `jmlpajak2`,`prtd`.`cabang` AS `cabang`,`prtd`.`lokasi` AS `lokasi`,`prtd`.`gudangasal` AS `gudangasal`,`prtd`.`gudangtransit` AS `gudangtransit`,`prtd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekdiskonpembelian` AS `rekdiskonpembelian`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekreturpembelian` AS `rekreturpembelian`,`prtd`.`costcenter` AS `costcenter`,`prtd`.`divisi` AS `divisi`,`prtd`.`subdivisi` AS `subdivisi`,`prtd`.`proyek` AS `proyek`,`prtd`.`catatan` AS `catatan`,`prtd`.`urutan` AS `urutan`,`prtd`.`idprdetail` AS `idprdetail`,`prtd`.`idcsdetail` AS `idcsdetail`,`prtd`.`idrqdetail` AS `idrqdetail`,`prtd`.`idbsdetail` AS `idbsdetail`,`prtd`.`idpodetail` AS `idpodetail`,`prtd`.`idipcdetail` AS `idipcdetail`,`prtd`.`idgrndetail` AS `idgrndetail`,`prtd`.`idridetail` AS `idridetail`,`prtd`.`iddnrdetail` AS `iddnrdetail`,`prtd`.`isclose` AS `isclose`,`prtd`.`customtext1` AS `customtext1`,`prtd`.`customtext2` AS `customtext2`,`prtd`.`customtext3` AS `customtext3`,`prtd`.`customdbl1` AS `customdbl1`,`prtd`.`customdbl2` AS `customdbl2`,`prtd`.`customdbl3` AS `customdbl3`,`prtd`.`customdate1` AS `customdate1`,`prtd`.`customdate2` AS `customdate2`,`prtd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`ri2`.`rinotransaksi` AS `rinotransaksi`,`dnr2`.`dnrnotransaksi` AS `dnrnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from (((((((((((((((((((((((((((((((((((`m4_prt` `prt` join `m4_prt_detail` `prtd` on((`prt`.`prtid` = `prtd`.`idprt`))) left join `m1_branch` `br` on((`br`.`bkode` = `prt`.`prtcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `prt`.`prtlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `prt`.`prtgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `prt`.`prtsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `prt`.`prtbagianpembelian`))) left join `m1_terms` `tr` on((`prt`.`prttermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`prt`.`prtrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`prt`.`prtrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`prt`.`prtrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`prt`.`prtrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`prt`.`prtrekbayar` = `coa5`.`cnomor`))) left join `m1_coa` `coa6` on((`prt`.`prtreksisa` = `coa6`.`cnomor`))) left join `m4_ri` `ri` on((`prt`.`prtidri` = `ri`.`riid`))) left join `m4_dnr` `dnr` on((`prt`.`prtiddnr` = `dnr`.`dnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `prt`.`prtstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `prt`.`prtstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `prt`.`prtinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `prt`.`prtmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `prtd`.`idbarang`))) left join `m1_tax` `t1` on((`prtd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`prtd`.`pajak2` = `t2`.`tkode`))) left join `m4_dnr_detail` `dnrd` on((`prtd`.`iddnrdetail` = `dnrd`.`iddnrdetail`))) left join `m4_dnr` `dnr2` on((`dnrd`.`iddnr` = `dnr2`.`dnrid`))) left join `m1_branch` `brd` on((`prtd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`prtd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`prtd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`prtd`.`gudangtransit` = `whd2`.`wkode`))) left join `m1_warehouse` `whd3` on((`prtd`.`gudangtujuan` = `whd3`.`wkode`))) left join `m1_cost_center` `cc` on((`prtd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`prtd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`prtd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`prtd`.`proyek` = `p`.`pkode`))) left join `m4_ri_detail` `rid` on((`prtd`.`idridetail` = `rid`.`idridetail`))) left join `m4_ri` `ri2` on((`rid`.`idri` = `ri2`.`riid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("prtid"), 0), sptField,
                     FxDB(drutama("prtcabang"), ""), sptField,
                     FxDB(drutama("prtlokasi"), ""), sptField,
                     FxDB(drutama("prtgudang"), ""), sptField,
                     FxDB(drutama("prtasalbarang"), ""), sptField,
                     FxDB(drutama("prtasalbarangkategori"), 0), sptField,
                     FxDB(drutama("prtjenispembelian"), ""), sptField,
                     FxDB(drutama("prtjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("prtcarabayar"), 0), sptField,
                     FxDB(drutama("prtsumber"), ""), sptField,
                     FxDB(drutama("prtautonotransaksi"), 0), sptField,
                     FxDB(drutama("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgl"), ""), formatTgl), sptField,
                     FxDB(drutama("prtkodepa"), 0), sptField,
                     FxDB(drutama("prtsupplier"), 0), sptField,
                     FxDB(drutama("prtsupplierkontak"), ""), sptField,
                     FxDB(drutama("prt1alamat1"), ""), sptField,
                     FxDB(drutama("prt1alamat2"), ""), sptField,
                     FxDB(drutama("prt1alamat3"), ""), sptField,
                     FxDB(drutama("prt2alamat1"), ""), sptField,
                     FxDB(drutama("prt2alamat2"), ""), sptField,
                     FxDB(drutama("prt2alamat3"), ""), sptField,
                     FxDB(drutama("prtbagianpembelian"), 0), sptField,
                     FxDB(drutama("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("prturaian"), ""), sptField,
                     FxDB(drutama("prtcatatan"), ""), sptField,
                     FxDB(drutama("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("prtmatauang"), ""), sptField,
                     FxDB(drutama("prtkurs"), 0), sptField,
                     FxDB(drutama("prthargatermasukpajak"), 0), sptField,
                     FxDB(drutama("prttotal"), 0), sptField,
                     FxDB(drutama("prtdiskonpersen"), ""), sptField,
                     FxDB(drutama("prtjmldiskon"), 0), sptField,
                     FxDB(drutama("prttotalpajak1detail"), 0), sptField,
                     FxDB(drutama("prttotalpajak2detail"), 0), sptField,
                     FxDB(drutama("prtbiayalainpersen"), ""), sptField,
                     FxDB(drutama("prtbiayalain"), 0), sptField,
                     FxDB(drutama("prttotaltransaksi"), 0), sptField,
                     FxDB(drutama("prtsisatransaksi"), 0), sptField,
                     FxDB(drutama("prtjmlbayar"), 0), sptField,
                     FxDB(drutama("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("prtnofakturpajak"), ""), sptField,
                     FxDB(drutama("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("prtrekdiskon"), ""), sptField,
                     FxDB(drutama("prtrekpajak1"), ""), sptField,
                     FxDB(drutama("prtrekpajak2"), ""), sptField,
                     FxDB(drutama("prtrekbiayalain"), ""), sptField,
                     FxDB(drutama("prtrekbayar"), ""), sptField,
                     FxDB(drutama("prtreksisa"), ""), sptField,
                     FxDB(drutama("prtidpr"), 0), sptField,
                     FxDB(drutama("prtidcs"), 0), sptField,
                     FxDB(drutama("prtidrq"), 0), sptField,
                     FxDB(drutama("prtidbs"), 0), sptField,
                     FxDB(drutama("prtidpo"), 0), sptField,
                     FxDB(drutama("prtidipc"), 0), sptField,
                     FxDB(drutama("prtidgrn"), 0), sptField,
                     FxDB(drutama("prtidri"), 0), sptField,
                     FxDB(drutama("prtiddnr"), 0), sptField,
                     FxDB(drutama("prtstatus"), 0), sptField,
                     FxDB(drutama("prtstatussebelumnya"), 0), sptField,
                     FxDB(drutama("prtjmlrevisi"), 0), sptField,
                     FxDB(drutama("prtcetakanke"), 0), sptField,
                     FxDB(drutama("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("prttutupperiode"), 0), sptField,
                     FxDB(drutama("prtisclose"), 0), sptField,
                     FxDB(drutama("prtcustomtext1"), ""), sptField,
                     FxDB(drutama("prtcustomtext2"), ""), sptField,
                     FxDB(drutama("prtcustomtext3"), ""), sptField,
                     FxDB(drutama("prtcustomtext4"), ""), sptField,
                     FxDB(drutama("prtcustomtext5"), ""), sptField,
                     FxDB(drutama("prtcustomint1"), 0), sptField,
                     FxDB(drutama("prtcustomint2"), 0), sptField,
                     FxDB(drutama("prtcustomint3"), 0), sptField,
                     FxDB(drutama("prtcustomdbl1"), 0), sptField,
                     FxDB(drutama("prtcustomdbl2"), 0), sptField,
                     FxDB(drutama("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("prtcabangnama"), ""), sptField,
                     FxDB(drutama("prtlokasinama"), ""), sptField,
                     FxDB(drutama("prtgudangnama"), ""), sptField,
                     FxDB(drutama("prtsupplierkode"), ""), sptField,
                     FxDB(drutama("prtsuppliernama"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("prtbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("prtterminnama"), ""), sptField,
                     FxDB(drutama("prtterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("prtrekdiskonnama"), ""), sptField,
                     FxDB(drutama("prtrekpajak1nama"), ""), sptField,
                     FxDB(drutama("prtrekpajak2nama"), ""), sptField,
                     FxDB(drutama("prtrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("prtrekbayarnama"), ""), sptField,
                     FxDB(drutama("prtreksisanama"), ""), sptField,
                     FxDB(drutama("prtnotransaksiri"), ""), sptField,
                     FxDB(drutama("prtnotransaksidnr"), ""), sptField,
                     FxDB(drutama("prtstatusnama"), ""), sptField,
                     FxDB(drutama("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("prtinputusernama"), ""), sptField,
                     FxDB(drutama("prtmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("prtjenis"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idprtdetail"), 0), sptField,
                     FxDB(dr("idprt"), 0), sptField,
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
                     FxDB(dr("idhppkhususmasuk"), 0), sptField,
                     FxDB(dr("idhppfifomasuk"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptField,
                     FxDB(dr("pajak1"), ""), sptField,
                     FxDB(dr("jmlpajak1"), 0), sptField,
                     FxDB(dr("pajak2"), ""), sptField,
                     FxDB(dr("jmlpajak2"), 0), sptField,
                     FxDB(dr("cabang"), ""), sptField,
                     FxDB(dr("lokasi"), ""), sptField,
                     FxDB(dr("gudangasal"), ""), sptField,
                     FxDB(dr("gudangtransit"), ""), sptField,
                     FxDB(dr("gudangtujuan"), ""), sptField,
                     FxDB(dr("rekpersediaan"), ""), sptField,
                     FxDB(dr("rekdiskonpembelian"), ""), sptField,
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
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("iddnrdetail"), 0), sptField,
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
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA BATCH
            sql = "select `nbt`.`nbtid` AS `nbtid`,`nbt`.`nbtjenismutasi` AS `nbtjenismutasi`,`nbt`.`nbtidbatchin` AS `nbtidbatchin`,`nbt`.`nbtgudang` AS `nbtgudang`,`nbt`.`nbtidbarang` AS `nbtidbarang`,`nbt`.`nbtkode` AS `nbtkode`,`nbt`.`nbtsumber` AS `nbtsumber`,`nbt`.`nbtidtransaksi` AS `nbtidtransaksi`,`nbt`.`nbtsatuan` AS `nbtsatuan`,`nbt`.`nbtjml` AS `nbtjml`,`nbt`.`nbtcustomtext1` AS `nbtcustomtext1`,`nbt`.`nbtcustomtext2` AS `nbtcustomtext2`,`nbt`.`nbtcustomtext3` AS `nbtcustomtext3`,`nbt`.`nbtcustomdbl1` AS `nbtcustomdbl1`,`nbt`.`nbtcustomdbl2` AS `nbtcustomdbl2`,`nbt`.`nbtcustomdbl3` AS `nbtcustomdbl3`,`nbt`.`nbtcustomdate1` AS `nbtcustomdate1`,`nbt`.`nbtcustomdate2` AS `nbtcustomdate2`,`nbt`.`nbtcustomdate3` AS `nbtcustomdate3`,`i`.`bkode` AS `kodebarang`, nbi.nbinotransaksi from ((`m1_no_batch_transaction` `nbt` join `m1_item` `i` on((`nbt`.`nbtidbarang` = `i`.`bid`))) left join `m1_no_batch_in` `nbi` on((`nbt`.`nbtidbatchin` = `nbi`.`nbiidbatchin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nbinotransaksi"), ""), sptRow)
            Next
            If batch.Length > 0 Then batch = batch.Substring(0, batch.Length - sptRow.Length) Else batch = batch

            'AMBIL DATA SERIAL
            sql = "select `nst`.`nstid` AS `nstid`,`nst`.`nstjenismutasi` AS `nstjenismutasi`,`nst`.`nstidserialin` AS `nstidserialin`,`nst`.`nstgudang` AS `nstgudang`,`nst`.`nstidbarang` AS `nstidbarang`,`nst`.`nstkode` AS `nstkode`,`nst`.`nstsumber` AS `nstsumber`,`nst`.`nstidtransaksi` AS `nstidtransaksi`,`nst`.`nstsatuan` AS `nstsatuan`,`nst`.`nstjml` AS `nstjml`,`nst`.`nstcustomtext1` AS `nstcustomtext1`,`nst`.`nstcustomtext2` AS `nstcustomtext2`,`nst`.`nstcustomtext3` AS `nstcustomtext3`,`nst`.`nstcustomdbl1` AS `nstcustomdbl1`,`nst`.`nstcustomdbl2` AS `nstcustomdbl2`,`nst`.`nstcustomdbl3` AS `nstcustomdbl3`,`nst`.`nstcustomdate1` AS `nstcustomdate1`,`nst`.`nstcustomdate2` AS `nstcustomdate2`,`nst`.`nstcustomdate3` AS `nstcustomdate3`,`i`.`bkode` AS `kodebarang`, nsi.nsinotransaksi from ((`m1_no_serial_transaction` `nst` join `m1_item` `i` on((`nst`.`nstidbarang` = `i`.`bid`))) left join `m1_no_serial_in` `nsi` on((`nst`.`nstidserialin` = `nsi`.`nsiidserialin`)))"
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
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("nsinotransaksi"), ""), sptRow)
            Next
            If serial.Length > 0 Then serial = serial.Substring(0, serial.Length - sptRow.Length) Else serial = serial

            'AMBIL DATA ASSET
            sql = "select atr.atid AS atid, atr.atasetid AS atasetid, atr.atjenismutasi AS atjenismutasi, atr.atsumber AS atsumber, atr.atidutama AS atidutama,atr.atidbarang AS atidbarang,atr.atkode AS atkode, atr.atnama AS atnama, atr.atkategori AS atkategori, atr.atcabang AS atcabang, atr.atlokasi AS atlokasi, atr.atgudang AS atgudang,atr.atdivisi AS atdivisi, atr.atsubdivisi AS atsubdivisi, atr.atcostcenter AS atcostcenter, atr.atproyek AS atproyek, atr.atcatatan AS atcatatan, atr.atnomor AS atnomor, atr.attglbeli AS attglbeli, atr.attglpakai AS attglpakai, atr.atjml AS atjml, atr.atsatuan AS atsatuan, atr.atmatauang AS atmatauang, atr.atkurs AS atkurs, atr.atharga AS atharga, atr.atdiskon AS atdiskon, atr.atjmldiskon AS atjmldiskon, atr.atpajak1 AS atpajak1, atr.atjmlpajak1 AS atjmlpajak1, atr.atpajak2 AS atpajak2, atr.atjmlpajak2 AS atjmlpajak2, atr.athargabeli AS athargabeli, atr.atnilairesidu AS atnilairesidu, atr.atumurekonomis AS atumurekonomis, atr.atbebanperbln AS atbebanperbln, atr.atakumulasibeban AS atakumulasibeban, atr.atnilaibuku AS atnilaibuku, (CASE WHEN atr.atnilaibuku < atr.atbebanperbln THEN atr.atnilaibuku ELSE atr.atbebanperbln END) as atnilaipenyusutan, atr.atmetode AS atmetode, atr.attabelpenyusutan AS attabelpenyusutan, atr.atintangible AS atintangible, atr.atfiskal AS atfiskal, atr.atatastengahbulan AS atatastengahbulan, atr.atrekasset AS atrekasset, atr.atrekakumdepresiasi AS atrekakumdepresiasi, atr.atrekdepresiasi AS atrekdepresiasi, atr.atrekpenghapusan AS atrekpenghapusan, atr.atprodusen AS atprodusen, atr.attglpensiun AS attglpensiun, atr.atpenyusutanke AS atpenyusutanke, atr.atnilaimenurun AS atnilaimenurun, atr.atdispose AS atdispose, atr.atpembelian AS atpembelian, atr.atpenjualan AS atpenjualan, atr.atlocked AS atlocked, atr.atstatus AS atstatus, atr.atstatussebelumnya AS atstatussebelumnya, atr.atisclose AS atisclose, atr.atinputuser AS atinputuser, atr.atinputtgl AS atinputtgl, atr.atmodifikasiuser AS atmodifikasiuser, atr.atmodifikasitgl AS atmodifikasitgl, atr.atcustomtext1,atr.atcustomtext2,atr.atcustomtext3,atr.atcustomtext4,atr.atcustomtext5,atr.atcustomint1,atr.atcustomint2,atr.atcustomint3,atr.atcustomint4,atr.atcustomint5,atr.atcustomdbl1,atr.atcustomdbl2,atr.atcustomdbl3,atr.atcustomdbl4,atr.atcustomdbl5,atr.atcustomdate1,atr.atcustomdate2,atr.atcustomdate3,atr.atcustomdate4,atr.atcustomdate5,ac.acnama AS atkategorinama, br.bnama AS atcabangnama, l.lnama AS atlokasinama, w.wnama AS atgudangnama,d.dnama AS atdivisinama, sd.sdnama AS atsubdivisinama, cc.ccnama AS atcostcenternama, p.pnama AS atproyeknama, dc.nama AS atmetodenama, t1.tnama AS atpajak1nama, ifnull(t1.tnilai, 0) AS atpajak1nilai, t2.tnama AS atpajak2nama, ifnull(t2.tnilai, 0) AS atpajak2nilai,coa1.cnama AS atrekassetnama, coa2.cnama AS atrekakumdepresiasinama, coa3.cnama AS atrekdepresiasinama, coa4.cnama AS atrekpenghapusannama, c1.kkode AS atprodusenkode, c1.knama AS atprodusennama,  sp1.nama AS atstatusnama,  sp2.nama AS atstatussebelumnyanama,  u1.unama AS atinputusernama,  u2.unama AS atmodifikasiusernama from m7_asset_transaction atr left join m7_asset_category ac on atr.atkategori = ac.ackode left join m1_branch br on atr.atcabang = br.bkode left join m1_location l on atr.atlokasi = l.lkode left join m1_warehouse w on atr.atgudang = w.wkode left join m1_division d on atr.atdivisi = d.dkode left join m1_subdivision sd on atr.atsubdivisi = sd.sdkode left join m7_depreciation_category dc on atr.atmetode = dc.kode left join m1_coa coa1 on atr.atrekasset = coa1.cnomor left join m1_coa coa2 on atr.atrekakumdepresiasi = coa2.cnomor left join m1_coa coa3 on atr.atrekdepresiasi = coa3.cnomor left join m1_coa coa4 on atr.atrekpenghapusan = coa4.cnomor left join m1_contact c1 on atr.atprodusen = c1.kid left join m0_status_progress sp1 on atr.atstatus = sp1.kode left join m0_status_progress sp2 on atr.atstatussebelumnya = sp2.kode left join m0_user u1 on atr.atinputuser = u1.userid left join m0_user u2 on atr.atmodifikasiuser = u2.userid left join m1_cost_center cc on atr.atcostcenter = cc.cckode left join m1_project p on atr.atproyek = p.pkode left join m1_tax t1 on atr.atpajak1 = t1.tkode left join m1_tax t2 on atr.atpajak2 = t2.tkode"
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
                     FxDB(dr("atmodifikasiusernama"), ""), sptRow)
            Next
            If asset.Length > 0 Then asset = asset.Substring(0, asset.Length - sptRow.Length) Else asset = asset

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, prtterminnama, prtterminharijatuhtempo, prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama, prtrekbayarnama, prtreksisanama, prtnotransaksiri, prtnotransaksidnr, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, kpkp" &
            sptSubParam & "idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, rinotransaksi, dnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" &
            sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang, nbtnotransaksi" &
            sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang, nstnotransaksi" &
            sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrtSearch(ByVal param As String) As String
        'M4_PrtSearch --------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, 
        'prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, prtjenisnama

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_prt_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Prt", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prtid"), 0), sptField,
                     FxDB(dr("prtcabang"), ""), sptField,
                     FxDB(dr("prtlokasi"), ""), sptField,
                     FxDB(dr("prtgudang"), ""), sptField,
                     FxDB(dr("prtasalbarang"), ""), sptField,
                     FxDB(dr("prtasalbarangkategori"), 0), sptField,
                     FxDB(dr("prtjenispembelian"), ""), sptField,
                     FxDB(dr("prtjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prtcarabayar"), 0), sptField,
                     FxDB(dr("prtsumber"), ""), sptField,
                     FxDB(dr("prtautonotransaksi"), 0), sptField,
                     FxDB(dr("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgl"), ""), formatTgl), sptField,
                     FxDB(dr("prtkodepa"), 0), sptField,
                     FxDB(dr("prtsupplier"), 0), sptField,
                     FxDB(dr("prtsupplierkontak"), ""), sptField,
                     FxDB(dr("prt1alamat1"), ""), sptField,
                     FxDB(dr("prt1alamat2"), ""), sptField,
                     FxDB(dr("prt1alamat3"), ""), sptField,
                     FxDB(dr("prt2alamat1"), ""), sptField,
                     FxDB(dr("prt2alamat2"), ""), sptField,
                     FxDB(dr("prt2alamat3"), ""), sptField,
                     FxDB(dr("prtbagianpembelian"), 0), sptField,
                     FxDB(dr("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("prturaian"), ""), sptField,
                     FxDB(dr("prtcatatan"), ""), sptField,
                     FxDB(dr("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prtmatauang"), ""), sptField,
                     FxDB(dr("prtkurs"), 0), sptField,
                     FxDB(dr("prthargatermasukpajak"), 0), sptField,
                     FxDB(dr("prttotal"), 0), sptField,
                     FxDB(dr("prtdiskonpersen"), ""), sptField,
                     FxDB(dr("prtjmldiskon"), 0), sptField,
                     FxDB(dr("prttotalpajak1detail"), 0), sptField,
                     FxDB(dr("prttotalpajak2detail"), 0), sptField,
                     FxDB(dr("prtbiayalainpersen"), ""), sptField,
                     FxDB(dr("prtbiayalain"), 0), sptField,
                     FxDB(dr("prttotaltransaksi"), 0), sptField,
                     FxDB(dr("prtsisatransaksi"), 0), sptField,
                     FxDB(dr("prtjmlbayar"), 0), sptField,
                     FxDB(dr("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("prtnofakturpajak"), ""), sptField,
                     FxDB(dr("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskon"), ""), sptField,
                     FxDB(dr("prtrekpajak1"), ""), sptField,
                     FxDB(dr("prtrekpajak2"), ""), sptField,
                     FxDB(dr("prtrekbiayalain"), ""), sptField,
                     FxDB(dr("prtrekbayar"), ""), sptField,
                     FxDB(dr("prtreksisa"), ""), sptField,
                     FxDB(dr("prtidpr"), 0), sptField,
                     FxDB(dr("prtidcs"), 0), sptField,
                     FxDB(dr("prtidrq"), 0), sptField,
                     FxDB(dr("prtidbs"), 0), sptField,
                     FxDB(dr("prtidpo"), 0), sptField,
                     FxDB(dr("prtidipc"), 0), sptField,
                     FxDB(dr("prtidgrn"), 0), sptField,
                     FxDB(dr("prtidri"), 0), sptField,
                     FxDB(dr("prtiddnr"), 0), sptField,
                     FxDB(dr("prtstatus"), 0), sptField,
                     FxDB(dr("prtstatussebelumnya"), 0), sptField,
                     FxDB(dr("prtjmlrevisi"), 0), sptField,
                     FxDB(dr("prtcetakanke"), 0), sptField,
                     FxDB(dr("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prttutupperiode"), 0), sptField,
                     FxDB(dr("prtisclose"), 0), sptField,
                     FxDB(dr("prtcabangnama"), ""), sptField,
                     FxDB(dr("prtlokasinama"), ""), sptField,
                     FxDB(dr("prtgudangnama"), ""), sptField,
                     FxDB(dr("prtsupplierkode"), ""), sptField,
                     FxDB(dr("prtsuppliernama"), ""), sptField,
                     FxDB(dr("prtbagianpembeliankode"), ""), sptField,
                     FxDB(dr("prtbagianpembeliannama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("prtstatusnama"), ""), sptField,
                     FxDB(dr("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("prtinputusernama"), ""), sptField,
                     FxDB(dr("prtmodifikasiusernama"), ""), sptField,
                     FxDB(dr("prtjenis"), 0), sptField,
                     FxDB(dr("prtjenisnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtjenis, prtjenisnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PrtTerkait(ByVal param As String) As String
        'M4_PrtTerkait --------------------------------------------------------
        'prtid, prtnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "prtid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_prt_terkait")
		sql = "select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`pr`.`prsumber` AS `sumber`,`pr`.`prid` AS `idterkait`,`pr`.`prnotransaksi` AS `noterkait`,`pr`.`prtgl` AS `tglterkait`,`pr`.`prinputtgl` AS `inputtglterkait`,`pr`.`prmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_pr_detail` `prd` join `m4_pr` `pr` on((`prd`.`idpr` = `pr`.`prid`))) join `m4_prt_detail` `prtd` on((`prd`.`idprdetail` = `prtd`.`idprdetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `pr`.`prid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`rq`.`rqsumber` AS `sumber`,`rq`.`rqid` AS `idterkait`,`rq`.`rqnotransaksi` AS `noterkait`,`rq`.`rqtgl` AS `tglterkait`,`rq`.`rqinputtgl` AS `inputtglterkait`,`rq`.`rqmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_rq_detail` `rqd` join `m4_rq` `rq` on((`rqd`.`idrq` = `rq`.`rqid`))) join `m4_prt_detail` `prtd` on((`rqd`.`idrqdetail` = `prtd`.`idrqdetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `rq`.`rqid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`po`.`posumber` AS `sumber`,`po`.`poid` AS `idterkait`,`po`.`ponotransaksi` AS `noterkait`,`po`.`potgl` AS `tglterkait`,`po`.`poinputtgl` AS `inputtglterkait`,`po`.`pomodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_po_detail` `pod` join `m4_po` `po` on((`pod`.`idpo` = `po`.`poid`))) join `m4_prt_detail` `prtd` on((`pod`.`idpodetail` = `prtd`.`idpodetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `po`.`poid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`grn`.`grnsumber` AS `sumber`,`grn`.`grnid` AS `idterkait`,`grn`.`grnnotransaksi` AS `noterkait`,`grn`.`grntgl` AS `tglterkait`,`grn`.`grninputtgl` AS `inputtglterkait`,`grn`.`grnmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_grn_detail` `grnd` join `m4_grn` `grn` on((`grnd`.`idgrn` = `grn`.`grnid`))) join `m4_prt_detail` `prtd` on((`grnd`.`idgrndetail` = `prtd`.`idgrndetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `grn`.`grnid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`ri`.`risumber` AS `sumber`,`ri`.`riid` AS `idterkait`,`ri`.`rinotransaksi` AS `noterkait`,`ri`.`ritgl` AS `tglterkait`,`ri`.`riinputtgl` AS `inputtglterkait`,`ri`.`rimodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_ri_detail` `rid` join `m4_ri` `ri` on((`rid`.`idri` = `ri`.`riid`))) join `m4_prt_detail` `prtd` on((`rid`.`idridetail` = `prtd`.`idridetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `ri`.`riid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`ri`.`risumber` AS `sumber`,`ri`.`riid` AS `idterkait`,`ri`.`rinotransaksi` AS `noterkait`,`ri`.`ritgl` AS `tglterkait`,`ri`.`riinputtgl` AS `inputtglterkait`,`ri`.`rimodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (`m4_ri` `ri` join `m4_prt` `prt` on((`prt`.`prtidri` = `ri`.`riid`))) where (`prt`.`prtid` = 'validtransaksi') group by `ri`.`riid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`dnr`.`dnrsumber` AS `sumber`,`dnr`.`dnrid` AS `idterkait`,`dnr`.`dnrnotransaksi` AS `noterkait`,`dnr`.`dnrtgl` AS `tglterkait`,`dnr`.`dnrinputtgl` AS `inputtglterkait`,`dnr`.`dnrmodifikasitgl` AS `modifikasitglterkait`,0 AS `jenisterkait` from (((`m4_dnr_detail` `dnrd` join `m4_dnr` `dnr` on((`dnrd`.`iddnr` = `dnr`.`dnrid`))) join `m4_prt_detail` `prtd` on((`dnrd`.`iddnrdetail` = `prtd`.`iddnrdetail`))) join `m4_prt` `prt` on((`prtd`.`idprt` = `prt`.`prtid`))) where (`prt`.`prtid` = 'validtransaksi') group by `dnr`.`dnrid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`vpp`.`vppsumber` AS `sumber`,`vpp`.`vppid` AS `idterkait`,`vpp`.`vppnotransaksi` AS `noterkait`,`vpp`.`vpptgl` AS `tglterkait`,`vpp`.`vppinputtgl` AS `inputtglterkait`,`vpp`.`vppmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vpp_detail` `vppd` join `m4_vpp` `vpp` on((`vppd`.`idvpp` = `vpp`.`vppid`))) join `m4_prt` `prt` on((`vppd`.`idtransaksi` = `prt`.`prtid`))) where ((`vppd`.`sumber` = 'PRT') and ((`vpp`.`vppstatus` = 2) or (`vpp`.`vppstatus` = 3) or (`vpp`.`vppstatus` = 4) or (`vpp`.`vppstatus` = 7)) and (`prt`.`prtid` = 'validtransaksi')) group by `vpp`.`vppid`,`prt`.`prtid` union all select `prt`.`prtid` AS `prtid`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`vp`.`vpsumber` AS `sumber`,`vp`.`vpid` AS `idterkait`,`vp`.`vpnotransaksi` AS `noterkait`,`vp`.`vptgl` AS `tglterkait`,`vp`.`vpinputtgl` AS `inputtglterkait`,`vp`.`vpmodifikasitgl` AS `modifikasitglterkait`,1 AS `jenisterkait` from ((`m4_vp_detail` `vpd` join `m4_vp` `vp` on((`vpd`.`idvp` = `vp`.`vpid`))) join `m4_prt` `prt` on((`vpd`.`idtransaksi` = `prt`.`prtid`))) where ((`vpd`.`sumber` = 'PRT') and ((`vp`.`vpstatus` = 2) or (`vp`.`vpstatus` = 3) or (`vp`.`vpstatus` = 4) or (`vp`.`vpstatus` = 7)) and (`prt`.`prtid` = 'validtransaksi')) group by `vp`.`vpid`,`prt`.`prtid`"
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prtid"), 0), sptField,
                     FxDB(dr("prtnotransaksi"), ""), sptField,
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
            result(2) = "Related PRT data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiHppI(ByVal dtdetail As DataTable, ByVal ftBarang As String) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtval As New DataTable, dtbarang As New DataTable, dtHppI As New DataTable, dtLookup As New DataTable
        Dim ftExistHppI As String = "", ftHppI As String = "", filterLookup As String = ""
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaisatuan As Double = 0, urutan As Double = 0, sisa As Double = 0

        '1. AMBIL BARANG HPP KHUSUS (I)
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode FROM m1_item WHERE (bjenis <> 'J') AND (bhpp = 'I') AND (" & ftBarang & ")")
        '2. CEK ID HPP KHUSUS MASUK
        If dtbarang.Rows.Count > 0 Then
            '3. PERULANGAN SEBANYAK BARANG HPP KHUSUS
            For Each dr1 As DataRow In dtbarang.Rows
                '4. AMBIL BARANG HPP KHUSUS DARI DETAIL
                dtHppI = AsDataTableFilterSortDt(dtdetail, "idbarang = '" & dr1("bid") & "'")
                If dtHppI.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppI.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP KHUSUS
                        ftExistHppI = IIf(Len(ftExistHppI.ToString) = 0, "", ftExistHppI & " UNION ")
                        ftExistHppI = String.Concat(ftExistHppI, "SELECT EXISTS(SELECT 1 FROM m1_cogs_special_in WHERE idhppikm = '" & dr2("idhppkhususmasuk") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")
                        '6. BUAT FILTER CEK JML HPP KHUSUS
                        Dim StokHppI As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idhppkhususmasuk=" & dr2("idhppkhususmasuk") & "")
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, " (csi.idhppikm = " & dr2("idhppkhususmasuk") & " AND " & StokHppI & " > csi.sisa) ")
                    Next
                End If
            Next

            'VALIDASI HPP KHUSUS (I) ------------------------------------
            'CEK DATA EXIST/TIDAK
            If Len(ftExistHppI) > 0 Then
                dtval = AsDataTableAmbilDariDB(ftExistHppI) 'ftExistHppI = rowExists, idbarang, bkode
                filterLookup = "rowExists = 0"
                dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")

                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists in COGS Special list." : GoTo selesai
                End If
            End If

            'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA BATCH YG TERSEDIA
            If Len(ftHppI) > 0 Then
                sql = "SELECT csi.idhppikm, csi.idbarang, csi.sisa, i.bkode FROM m1_cogs_special_in csi JOIN m1_item i ON csi.idbarang = i.bid WHERE " & ftHppI
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")
                    sisa = dtval.Rows(0)("sisa")

                    filterLookup = "idhppkhususmasuk=" & dtval.Rows(0)("idhppikm")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaiSatuan")
                        urutan = dtLookup.Rows(0)("urutan")
                    End If
                    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of stock in COGS Special, item(s) available " & sisa / nilaisatuan & " " & satuan : GoTo selesai
                End If
            End If
            'END OF VALIDASI HPP KHUSUS (I) -----------------------------
        End If

selesai:
        Return errmessage
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
                dtHppF = AsDataTableFilterSortDt(dtdetail, "idbarang = '" & dr1("bid") & "'")
                If dtHppF.Rows.Count > 0 Then
                    For Each dr2 As DataRow In dtHppF.Rows
                        '5. BUAT FILTER CEK DATA EXIST HPP FIFO
                        ftExistHppF = IIf(Len(ftExistHppF.ToString) = 0, "", ftExistHppF & " UNION ")
                        ftExistHppF = String.Concat(ftExistHppF, "SELECT EXISTS(SELECT 1 FROM m1_cogs_fifo_in WHERE cfiisclose = 0 AND cfiidbarang = '" & dr1("bid") & "' LIMIT 1) as rowExists, '" & dr1("bid") & "' as idbarang, bkode FROM m1_item WHERE bid = '" & dr1("bid") & "'")
                        '6. BUAT FILTER CEK JML HPP FIFO
                        Dim StokHppF As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & dr1("bid") & "")
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

                    filterLookup = "idbarang=" & dtval.Rows(0)("idbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
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

                    filterLookup = "idbarang=" & dtval.Rows(0)("cfiidbarang")
                    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                    If dtLookup.Rows.Count > 0 Then
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuan = dtLookup.Rows(0)("satuan")
                        nilaisatuan = dtLookup.Rows(0)("nilaiSatuan")
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

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingRI As String, ByVal ftOutstandingRI As String, ByVal ftExistOutstandingDNR As String, ByVal ftOutstandingDNR As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String, ByVal ftRI As String, ByVal ftDNR As String, ByVal termasukPajak As String, ByVal dtasset As DataTable) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = "", noBatch As String = "", noSerial As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'RI
        If Len(ftExistOutstandingRI) > 0 Then 'ftExistOutstanding = rowExists, idridetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingRI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idridetail=" & dtval.Rows(0)("idridetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in RI" : GoTo selesai
            End If
        End If

        'CEK RI YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftRI) > 0 Then
            sql = "SELECT ri.rinotransaksi as notransaksi, (CASE ri.rihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid WHERE " & ftRI & " GROUP BY ri.rihargatermasukpajak"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 1 Then
                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                For Each dr1 As DataRow In dtval.Rows
                    errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                Next
                GoTo selesai
            End If

            If Len(termasukPajak) > 0 Then
                sql = "SELECT i.bkode, rid.idridetail, ri.rinotransaksi as notransaksi, (CASE ri.rihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid JOIN m1_item i ON rid.idbarang = i.bid WHERE (" & ftRI & ") AND ri.rihargatermasukpajak <> " & termasukPajak & " ORDER BY rid.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idridetail = " & dtval.Rows(0)("idridetail")
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
        If Len(ftOutstandingRI) > 0 Then
            sql = "SELECT rid.idridetail, (rid.jmlbarang - rid.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_ri_detail AS rid INNER JOIN m1_item AS i ON rid.idbarang = i.bid WHERE " & ftOutstandingRI
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idridetail=" & dtval.Rows(0)("idridetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in RI, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If

        'DNR
        If Len(ftExistOutstandingDNR) > 0 Then 'ftExistOutstanding = rowExists, iddnrdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingDNR)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "iddnrdetail=" & dtval.Rows(0)("iddnrdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in DNR" : GoTo selesai
            End If
        End If

        'CEK DNR YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftDNR) > 0 Then
            sql = "SELECT dnr.dnrnotransaksi as notransaksi, (CASE dnr.dnrhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid WHERE " & ftDNR & " GROUP BY dnr.dnrhargatermasukpajak"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 1 Then
                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                For Each dr1 As DataRow In dtval.Rows
                    errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajak")
                Next
                GoTo selesai
            End If

            If Len(termasukPajak) > 0 Then
                sql = "SELECT i.bkode, dnrd.iddnrdetail, dnr.dnrnotransaksi as notransaksi, (CASE dnr.dnrhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid JOIN m1_item i ON dnrd.idbarang = i.bid WHERE (" & ftDNR & ") AND dnr.dnrhargatermasukpajak <> " & termasukPajak & " ORDER BY dnrd.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "iddnrdetail = " & dtval.Rows(0)("iddnrdetail")
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
        If Len(ftOutstandingDNR) > 0 Then
            sql = "SELECT dnrd.iddnrdetail, (dnrd.jmlbarang - dnrd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m4_dnr_detail AS dnrd INNER JOIN m1_item AS i ON dnrd.idbarang = i.bid WHERE " & ftOutstandingDNR
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "iddnrdetail=" & dtval.Rows(0)("iddnrdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in DNR, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------


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
                sql = "SELECT isw.idbarang, isw.kgudang, isw.stok - IFNULL(isb.jmlbooking,0) as stok, i.bkode FROM m1_item_stock_warehouse isw JOIN m1_item i ON isw.idbarang = i.bid AND i.bjenis <> 'J' LEFT JOIN m1_warehouse w ON isw.kgudang = w.wkode LEFT JOIN m1_item_booking isb ON isw.idbarang = isb.idbarang AND w.wbookingstok = 1 WHERE " & ftStok
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


        'VALIDASI BATCH ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistBatch) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistBatch) 'ftExistBatch = rowExists, idbarang, bkode, nbikode, nbigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " doesn't exists in No. Batch list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA BATCH YG TERSEDIA
        If Len(ftBatch) > 0 Then
            sql = "SELECT nbi.nbiidbarang, nbi.nbikode, nbi.nbigudang, nbi.nbijmlsisa, i.bkode FROM m1_no_batch_in nbi JOIN m1_item i ON nbi.nbiidbarang = i.bid WHERE " & ftBatch
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nbijmlsisa")
                noBatch = dtval.Rows(0)("nbikode")
                gudang = dtval.Rows(0)("nbigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nbiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nbigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Batch : " & noBatch & " exceeds the number of stock in No. Batch list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI BATCH --------------------------------------

        'VALIDASI SERIAL ---------------------------------------------
        'CEK DATA EXIST/TIDAK
        If Len(ftExistSerial) > 0 Then
            dtval = AsDataTableAmbilDariDB(ftExistSerial) 'ftExistSerial = rowExists, idbarang, bkode, nsikode, nsigudang
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("idbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " doesn't exists in No. Serial list." : GoTo selesai
            End If
        End If

        'PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA SERIAL YG TERSEDIA
        If Len(ftSerial) > 0 Then
            sql = "SELECT nsi.nsiidbarang, nsi.nsikode, nsi.nsigudang, nsi.nsijmlsisa, i.bkode FROM m1_no_serial_in nsi JOIN m1_item i ON nsi.nsiidbarang = i.bid WHERE " & ftSerial
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("nsijmlsisa")
                noSerial = dtval.Rows(0)("nsikode")
                gudang = dtval.Rows(0)("nsigudang")

                filterLookup = "idbarang = " & dtval.Rows(0)("nsiidbarang") & " AND " & gudangBatchSerial & " = '" & dtval.Rows(0)("nsigudang") & "'"
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " | No. Serial : " & noSerial & " exceeds the number of stock in No. Serial list, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            End If
        End If
        'END OF VALIDASI SERIAL --------------------------------------


        'VALIDASI ASSET ----------------------------------------------
        If dtasset.Rows.Count > 0 Then
            Dim strValue2 As New StringBuilder
            For Each dr1 As DataRow In dtasset.Rows
                'QUERY INSERT NO ASSET IN
                strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                strValue2.Append(FixDouble(dr1("atasetid")))
            Next
            sql = "SELECT a.akode, a.anama, da.danotransaksi FROM m7_da_detail dad JOIN m7_da da ON dad.idda = da.daid AND da.dastatus IN(2,3,4,7) JOIN m7_asset a ON dad.idaset = a.aid AND dad.idaset IN(" & strValue2.ToString & ") GROUP BY da.daid, dad.idaset ORDER BY da.datgl, da.daid, dad.idaset LIMIT 1"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                errmessage = "Asset : " & dtval(0)("akode") & " | " & dtval(0)("anama") & " has related transaction in '" & dtval(0)("danotransaksi") & "'." : GoTo selesai
            End If
        End If
        'END OF VALIDASI ASSET ---------------------------------------

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_PrtSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String

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
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'prtid(0) As Integer, prtcabang(1) As String, prtlokasi(2) As String, prtgudang(3) As String, prtasalbarang(4) As String, 
        'prtasalbarangkategori(5) As Integer, prtjenispembelian(6) As String, prtjenispembeliankategori(7) As Integer, prtcarabayar(8) As Integer, prtsumber(9) As String, 
        'prtautonotransaksi(10) As Integer, prtnotransaksi(11) As String, prttgl(12) As Date, prtkodepa(13) As Integer, prtsupplier(14) As Integer, 
        'prtsupplierkontak(15) As String, prt1alamat1(16) As String, prt1alamat2(17) As String, prt1alamat3(18) As String, prt2alamat1(19) As String, 
        'prt2alamat2(20) As String, prt2alamat3(21) As String, prtbagianpembelian(22) As Integer, prttermin(23) As String, prttgljatuhtempo(24) As Date, 
        'prturaian(25) As String, prtcatatan(26) As String, prtnoref(27) As String, prttglnoref(28) As Date, prttglpenutupan(29) As Date, 
        'prtmatauang(30) As String, prtkurs(31) As Double, prthargatermasukpajak(32) As Integer, prttotal(33) As Double, prtdiskonpersen(34) As String, 
        'prtjmldiskon(35) As Double, prttotalpajak1detail(36) As Double, prttotalpajak2detail(37) As Double, prtbiayalainpersen(38) As String, prtbiayalain(39) As Double, 
        'prttotaltransaksi(40) As Double, prtsisatransaksi(41) As Double, prtjmlbayar(42) As Double, prtstatuslunas(43) As Integer, prttgllunas(44) As Date, 
        'prtnofakturpajak(45) As String, prtsdhbayarpajak(46) As Integer, prttglbayarpajak(47) As Date, prtrekdiskon(48) As String, prtrekpajak1(49) As String, 
        'prtrekpajak2(50) As String, prtrekbiayalain(51) As String, prtrekbayar(52) As String, prtreksisa(53) As String, prtidpr(54) As Integer, 
        'prtidcs(55) As Integer, prtidrq(56) As Integer, prtidbs(57) As Integer, prtidpo(58) As Integer, prtidipc(59) As Integer, 
        'prtidgrn(60) As Integer, prtidri(61) As Integer, prtiddnr(62) As Integer, prtstatus(63) As Integer, prtstatussebelumnya(64) As Integer, 
        'prtjmlrevisi(65) As Integer, prtcetakanke(66) As Integer, prtinputuser(67) As Integer, prtinputtgl(68) As DateTime, prtmodifikasiuser(69) As Integer, 
        'prtmodifikasitgl(70) As DateTime, prtposting(71) As Integer, prttutupperiode(72) As Integer, prtisclose(73) As Integer, prtcustomtext1(74) As String, 
        'prtcustomtext2(75) As String, prtcustomtext3(76) As String, prtcustomtext4(77) As String, prtcustomtext5(78) As String, prtcustomint1(79) As Integer, 
        'prtcustomint2(80) As Integer, prtcustomint3(81) As Integer, prtcustomdbl1(82) As Double, prtcustomdbl2(83) As Double, prtcustomdbl3(84) As Double, 
        'prtcustomdate1(85) As Date, prtcustomdate2(86) As Date, prtcustomdate3(87) As Date, prtjenis(88) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, 
        'prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, 
        'prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis(88) As Integer

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 89) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'prtid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "prtid required numeric." : GoTo selesai
        End If
        'prtasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "prtasalbarangkategori required numeric." : GoTo selesai
        End If
        'prtjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "prtjenispembeliankategori required numeric." : GoTo selesai
        End If
        'prtcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "prtcarabayar required numeric." : GoTo selesai
        End If
        'prtautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "prtautonotransaksi required numeric." : GoTo selesai
        End If
        'prttgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "prttgl required date." : GoTo selesai
        End If
        'prtkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "prtkodepa required numeric." : GoTo selesai
        End If
        'prtsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "prtsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "prtsupplier can't be empty." : GoTo selesai
        End If
        'prtbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "prtbagianpembelian required numeric." : GoTo selesai
        End If
        'prttgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "prttgljatuhtempo required date." : GoTo selesai
        End If
        'prttglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "prttglnoref required date." : GoTo selesai
        End If
        'prttglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "prttglpenutupan required date." : GoTo selesai
        End If
        'prtkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "prtkurs required numeric." : GoTo selesai
        End If
        'prthargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "prthargatermasukpajak required numeric." : GoTo selesai
        End If
        'prttotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "prttotal required numeric." : GoTo selesai
        End If
        'prtjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "prtjmldiskon required numeric." : GoTo selesai
        End If
        'prttotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "prttotalpajak1detail required numeric." : GoTo selesai
        End If
        'prttotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "prttotalpajak2detail required numeric." : GoTo selesai
        End If
        'prtbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "prtbiayalain required numeric." : GoTo selesai
        End If
        'prttotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "prttotaltransaksi required numeric." : GoTo selesai
        End If
        'prtsisatransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "prtsisatransaksi required numeric." : GoTo selesai
        End If
        'prtjmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "prtjmlbayar required numeric." : GoTo selesai
        End If
        'prtstatuslunas(43) As Integer
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "prtstatuslunas required numeric." : GoTo selesai
        End If
        'prttgllunas(44) As Date
        If (IsDate(dataUtama(44)) = False) Then
            result(2) = "prttgllunas required date." : GoTo selesai
        End If
        'prtsdhbayarpajak(46) As Integer
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "prtsdhbayarpajak required numeric." : GoTo selesai
        End If
        'prttglbayarpajak(47) As Date
        If (IsDate(dataUtama(47)) = False) Then
            result(2) = "prttglbayarpajak required date." : GoTo selesai
        End If
        'prtidpr(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "prtidpr required numeric." : GoTo selesai
        End If
        'prtidcs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "prtidcs required numeric." : GoTo selesai
        End If
        'prtidrq(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "prtidrq required numeric." : GoTo selesai
        End If
        'prtidbs(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "prtidbs required numeric." : GoTo selesai
        End If
        'prtidpo(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "prtidpo required numeric." : GoTo selesai
        End If
        'prtidipc(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "prtidipc required numeric." : GoTo selesai
        End If
        'prtidgrn(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "prtidgrn required numeric." : GoTo selesai
        End If
        'prtidri(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "prtidri required numeric." : GoTo selesai
        End If
        'prtiddnr(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "prtiddnr required numeric." : GoTo selesai
        End If
        'prtstatus(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "prtstatus required numeric." : GoTo selesai
        End If
        'prtstatussebelumnya(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "prtstatussebelumnya required numeric." : GoTo selesai
        End If
        'prtjmlrevisi(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "prtjmlrevisi required numeric." : GoTo selesai
        End If
        'prtcetakanke(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "prtcetakanke required numeric." : GoTo selesai
        End If
        'prtinputuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "prtinputuser required numeric." : GoTo selesai
        End If
        'prtinputtgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "prtinputtgl required date." : GoTo selesai
        End If
        'prtmodifikasiuser(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "prtmodifikasiuser required numeric." : GoTo selesai
        End If
        'prtmodifikasitgl(70) As DateTime
        If (IsDate(dataUtama(70)) = False) Then
            result(2) = "prtmodifikasitgl required date." : GoTo selesai
        End If
        'prtposting(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "prtposting required numeric." : GoTo selesai
        End If
        'prttutupperiode(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "prttutupperiode required numeric." : GoTo selesai
        End If
        'prtisclose(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "prtisclose required numeric." : GoTo selesai
        End If
        'prtcustomint1(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "prtcustomint1 required numeric." : GoTo selesai
        End If
        'prtcustomint2(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "prtcustomint2 required numeric." : GoTo selesai
        End If
        'prtcustomint3(81) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "prtcustomint3 required numeric." : GoTo selesai
        End If
        'prtcustomdbl1(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "prtcustomdbl1 required numeric." : GoTo selesai
        End If
        'prtcustomdbl2(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "prtcustomdbl2 required numeric." : GoTo selesai
        End If
        'prtcustomdbl3(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "prtcustomdbl3 required numeric." : GoTo selesai
        End If
        'prtcustomdate1(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "prtcustomdate1 required date." : GoTo selesai
        End If
        'prtcustomdate2(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "prtcustomdate2 required date." : GoTo selesai
        End If
        'prtcustomdate3(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "prtcustomdate3 required date." : GoTo selesai
        End If
        'prtjenis(88) As Integer
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "prtjenis required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'prtcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "prtcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "prtcabang should not be more than 25 character." : GoTo selesai
        End If

        'prtlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "prtlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "prtlokasi should not be more than 25 character." : GoTo selesai
        End If

        'prtgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "prtgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "prtgudang should not be more than 25 character." : GoTo selesai
        End If

        'prtsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "prtsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "prtsumber should not be more than 10 character." : GoTo selesai
        End If

        'prtnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "prtnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "prtnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'prttgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "prttgl can't be empty" : GoTo selesai
        End If

        'prttgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "prttgljatuhtempo can't be empty" : GoTo selesai
        End If

        'prttglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "prttglnoref can't be empty" : GoTo selesai
        End If

        'prttglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "prttglpenutupan can't be empty" : GoTo selesai
        End If

        'prtmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "prtmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "prtmatauang should not be more than 25 character." : GoTo selesai
        End If

        'prtkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "prtkurs can't be empty" : GoTo selesai
        End If

        'prttotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "prttotal can't be empty" : GoTo selesai
        End If

        'prtdiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "prtdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "prtdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'prtjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "prtjmldiskon can't be empty" : GoTo selesai
        End If

        'prttotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "prttotalpajak1detail can't be empty" : GoTo selesai
        End If

        'prttotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "prttotalpajak2detail can't be empty" : GoTo selesai
        End If

        'prtbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "prtbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "prtbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'prtbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "prtbiayalain can't be empty" : GoTo selesai
        End If

        'prttotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "prttotaltransaksi can't be empty" : GoTo selesai
        End If

        'prtsisatransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "prtsisatransaksi can't be empty" : GoTo selesai
        End If

        'prtjmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "prtjmlbayar can't be empty" : GoTo selesai
        End If

        'prttgllunas(44) As Date
        If Len(dataUtama(44)) = 0 Then
            result(2) = "prttgllunas can't be empty" : GoTo selesai
        End If

        'prttglbayarpajak(47) As Date
        If Len(dataUtama(47)) = 0 Then
            result(2) = "prttglbayarpajak can't be empty" : GoTo selesai
        End If

        'prtinputtgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "prtinputtgl can't be empty" : GoTo selesai
        End If

        'prtmodifikasitgl(70) As DateTime
        If Len(dataUtama(70)) = 0 Then
            result(2) = "prtmodifikasitgl can't be empty" : GoTo selesai
        End If

        'prtcustomdbl1(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "prtcustomdbl1 can't be empty" : GoTo selesai
        End If

        'prtcustomdbl2(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "prtcustomdbl2 can't be empty" : GoTo selesai
        End If

        'prtcustomdbl3(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "prtcustomdbl3 can't be empty" : GoTo selesai
        End If

        'prtcustomdate1(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "prtcustomdate1 can't be empty" : GoTo selesai
        End If

        'prtcustomdate2(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "prtcustomdate2 can't be empty" : GoTo selesai
        End If

        'prtcustomdate3(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "prtcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prthargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtiddnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjenis", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "prtid~prtcabang~prtlokasi~prtgudang~prtasalbarang~prtasalbarangkategori~prtjenispembelian~prtjenispembeliankategori~prtcarabayar~prtsumber~prtautonotransaksi~prtnotransaksi~prttgl~prtkodepa~prtsupplier~prtsupplierkontak~prt1alamat1~prt1alamat2~prt1alamat3~prt2alamat1~prt2alamat2~prt2alamat3~prtbagianpembelian~prttermin~prttgljatuhtempo~prturaian~prtcatatan~prtnoref~prttglnoref~prttglpenutupan~prtmatauang~prtkurs~prthargatermasukpajak~prttotal~prtdiskonpersen~prtjmldiskon~prttotalpajak1detail~prttotalpajak2detail~prtbiayalainpersen~prtbiayalain~prttotaltransaksi~prtsisatransaksi~prtjmlbayar~prtstatuslunas~prttgllunas~prtnofakturpajak~prtsdhbayarpajak~prttglbayarpajak~prtrekdiskon~prtrekpajak1~prtrekpajak2~prtrekbiayalain~prtrekbayar~prtreksisa~prtidpr~prtidcs~prtidrq~prtidbs~prtidpo~prtidipc~prtidgrn~prtidri~prtiddnr~prtstatus~prtstatussebelumnya~prtjmlrevisi~prtcetakanke~prtinputuser~prtinputtgl~prtmodifikasiuser~prtmodifikasitgl~prtposting~prttutupperiode~prtisclose~prtcustomtext1~prtcustomtext2~prtcustomtext3~prtcustomtext4~prtcustomtext5~prtcustomint1~prtcustomint2~prtcustomint3~prtcustomdbl1~prtcustomdbl2~prtcustomdbl3~prtcustomdate1~prtcustomdate2~prtcustomdate3~prtjenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idprtdetail(0) As Integer, idprt(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, idhppkhususmasuk(13) As Integer, idhppfifomasuk(14) As Integer, 
        'hpp(15) As Double, harga(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekdiskonpembelian(29) As String, 
        'rekhargapokok(30) As String, rekreturpembelian(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idprdetail(38) As Integer, idcsdetail(39) As Integer, 
        'idrqdetail(40) As Integer, idbsdetail(41) As Integer, idpodetail(42) As Integer, idipcdetail(43) As Integer, idgrndetail(44) As Integer, 
        'idridetail(45) As Integer, iddnrdetail(46) As Integer, isclose(47) As Integer, customtext1(48) As String, customtext2(49) As String, 
        'customtext3(50) As String, customdbl1(51) As Double, customdbl2(52) As Double, customdbl3(53) As Double, customdate1(54) As Date, 
        'customdate2(55) As Date, customdate3(56) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, 
        'idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, 
        'idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idprtdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idprt", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususmasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifomasuk", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "diskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmldiskon", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak1", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "pajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlpajak2", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "cabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "lokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangasal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtransit", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "gudangtujuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekreturpembelian", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idridetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddnrdetail", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftBarang As String = ""

        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = ""
        Dim ftExistOutstandingDNR As String = "", ftOutstandingDNR As String = "", updNilaiDNR As String = "", updFilterDNR As String = ""
        Dim idbarang As Integer = 0, idridetail As Integer = 0, iddnrdetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokBarang As String = "", ftStokBarang As String = ""

        'FILTER RI DAN DNR, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRI As String = "", ftDNR As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 57) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idprtdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idprtdetail required numeric." : GoTo selesai
            End If
            'idprt(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idprt required numeric." : GoTo selesai
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
            'idhppkhususmasuk(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususmasuk required numeric." : GoTo selesai
            End If
            'idhppfifomasuk(14) As Integer
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - idhppfifomasuk required numeric." : GoTo selesai
            End If
            'hpp(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
            End If
            'harga(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(20) As Double
            If (IsNumeric(dataRowDetail(20)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(22) As Double
            If (IsNumeric(dataRowDetail(22)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(37) As Integer
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idprdetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idprdetail required numeric." : GoTo selesai
            End If
            'idcsdetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idcsdetail required numeric." : GoTo selesai
            End If
            'idrqdetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idrqdetail required numeric." : GoTo selesai
            End If
            'idbsdetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - idbsdetail required numeric." : GoTo selesai
            End If
            'idpodetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - idpodetail required numeric." : GoTo selesai
            End If
            'idipcdetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - idipcdetail required numeric." : GoTo selesai
            End If
            'idgrndetail(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - idgrndetail required numeric." : GoTo selesai
            End If
            'idridetail(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - idridetail required numeric." : GoTo selesai
            End If
            'iddnrdetail(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - iddnrdetail required numeric." : GoTo selesai
            End If
            'isclose(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
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

            'hpp(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hpp can't be empty" : GoTo selesai
            End If

            'harga(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If
            'If dataRowDetail(16) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'diskon(17) As String
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(17)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(17) As String
                dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(17).ToString))
            End If

            'jmlpajak1(20) As Double
            If Len(dataRowDetail(20)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(22) As Double
            If Len(dataRowDetail(22)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'gudangasal(25) As String
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(26) As String
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(27) As String
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(27)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'customdbl1(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(53) As Double
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idprtdetail~idprt~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~idhppkhususmasuk~idhppfifomasuk~hpp~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekdiskonpembelian~rekhargapokok~rekreturpembelian~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~idridetail~iddnrdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(26) As String    , idridetail(45) As Integer      , iddnrdetail(46) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(26) : idridetail = dataRowDetail(45) : iddnrdetail = dataRowDetail(46)

            'ValidasiHppI
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'VALIDASI OUTSTANDING -------------------------
            If idridetail <> 0 Then 'RI
                'CEK RI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRI = IIf(Len(ftRI.ToString) = 0, "", ftRI & " OR ")
                ftRI = String.Concat(ftRI, " (rid.idridetail = " & idridetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingRI = IIf(Len(ftExistOutstandingRI.ToString) = 0, "", ftExistOutstandingRI & " UNION ")
                ftExistOutstandingRI = String.Concat(ftExistOutstandingRI, "SELECT EXISTS(SELECT 1 FROM m4_ri_detail JOIN m4_ri ON idri = riid WHERE idridetail = '" & idridetail & "' AND (ristatus = 2 OR ristatus = 3 OR ristatus = 4 OR ristatus = 7) LIMIT 1) as rowExists, '" & idridetail & "' as idridetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING 
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                ftOutstandingRI = IIf(Len(ftOutstandingRI.ToString) = 0, "", ftOutstandingRI & " OR ")
                ftOutstandingRI = String.Concat(ftOutstandingRI, " (rid.idridetail = " & idridetail & " AND " & Outstanding & " > (rid.jmlbarang - rid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING 
                updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRI)

                '4. SET FILTER UPDATE OUTSTANDING 
                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
            End If

            If iddnrdetail <> 0 Then 'DNR
                'CEK DNR YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftDNR = IIf(Len(ftDNR.ToString) = 0, "", ftDNR & " OR ")
                ftDNR = String.Concat(ftDNR, " (dnrd.iddnrdetail = " & iddnrdetail & ") ")

                '1. CEK DATA EXIST 
                ftExistOutstandingDNR = IIf(Len(ftExistOutstandingDNR.ToString) = 0, "", ftExistOutstandingDNR & " UNION ")
                ftExistOutstandingDNR = String.Concat(ftExistOutstandingDNR, "SELECT EXISTS(SELECT 1 FROM m4_dnr_detail JOIN m4_dnr ON iddnr = dnrid WHERE iddnrdetail = '" & iddnrdetail & "' AND (dnrstatus = 2 OR dnrstatus = 3 OR dnrstatus = 4 OR dnrstatus = 7) LIMIT 1) as rowExists, '" & iddnrdetail & "' as iddnrdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING 
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddnrdetail=" & iddnrdetail)
                ftOutstandingDNR = IIf(Len(ftOutstandingDNR.ToString) = 0, "", ftOutstandingDNR & " OR ")
                ftOutstandingDNR = String.Concat(ftOutstandingDNR, " (dnrd.iddnrdetail = " & iddnrdetail & " AND " & Outstanding & " > (dnrd.jmlbarang - dnrd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING 
                updNilaiDNR = String.Concat("WHEN '" & iddnrdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiDNR)

                '4. SET FILTER UPDATE OUTSTANDING 
                updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                updFilterDNR = String.Concat(updFilterDNR, "(iddnrdetail = '" & iddnrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK M1_ITEM
            Dim stokKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokKeluar & "', 5) ", updStokBarang)

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

        'ValidasiSimpan
        Dim ftExistBatch As String = "", ftBatch As String = ""
        Dim nbtkode As String = "", nbtgudang As String = "", nbtidbatchin As Integer = 0
        Dim updNilaiBatch As String = "", updFilterBatch As String = ""

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
                dataRowBatch(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nbtidbarang(2) As Integer , nbtkode(3) As String      , nbtjml(7) As Double         , nbtgudang(17) As String      , nbtidbatchin(18) As Integer
                idbarang = dataRowBatch(2) : nbtkode = dataRowBatch(3) : jmlbarang = dataRowBatch(7) : nbtgudang = dataRowBatch(17) : nbtidbatchin = dataRowBatch(18)

                'VALIDASI BATCH -------------------------------
                '1. CEK DATA EXIST BATCH KELUAR 
                ftExistBatch = IIf(Len(ftExistBatch.ToString) = 0, "", ftExistBatch & " UNION ")
                ftExistBatch = String.Concat(ftExistBatch, "SELECT EXISTS(SELECT 1 FROM m1_no_batch_in WHERE nbiidbatchin = '" & nbtidbatchin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nbtkode & "' as nbikode, '" & nbtgudang & "' as nbigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML BATCH KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtbatch, "nbtjml", "nbtidbatchin = " & nbtidbatchin & "")
                ftBatch = IIf(Len(ftBatch.ToString) = 0, "", ftBatch & " OR ")
                ftBatch = String.Concat(ftBatch, " (nbi.nbiidbatchin = " & nbtidbatchin & " AND " & jmlKeluar & " > nbi.nbijmlsisa) ")

                '3. SET NILAI UPDATE BATCH IN 
                updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & nbtidbatchin & "' THEN ROUND(nbijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiBatch)

                '4. SET FILTER UPDATE BATCH IN 
                updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & nbtidbatchin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

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

        'ValidasiSimpan
        Dim ftExistSerial As String = "", ftSerial As String = ""
        Dim nstkode As String = "", nstgudang As String = "", nstidserialin As Integer = 0
        Dim updNilaiSerial As String = "", updFilterSerial As String = ""

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
                dataRowSerial(1) = 0
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

                'ValidasiSimpan
                'BUAT FILTER UNTUK VALIDASI ---------------------------------
                'nstidbarang(2) As Integer  , nstkode(3) As String       , nstjml(7) As Double          , nstgudang(17) As String       , nstidserialin(18) As Integer
                idbarang = dataRowSerial(2) : nstkode = dataRowSerial(3) : jmlbarang = dataRowSerial(7) : nstgudang = dataRowSerial(17) : nstidserialin = dataRowSerial(18)

                'VALIDASI SERIAL -------------------------------
                '1. CEK DATA EXIST SERIAL KELUAR
                ftExistSerial = IIf(Len(ftExistSerial.ToString) = 0, "", ftExistSerial & " UNION ")
                ftExistSerial = String.Concat(ftExistSerial, "SELECT EXISTS(SELECT 1 FROM m1_no_serial_in WHERE nsiidserialin = '" & nstidserialin & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & nstkode & "' as nsikode, '" & nstgudang & "' as nsigudang FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML SERIAL KELUAR 
                Dim jmlKeluar As Double = AsDataTableDSum(dtserial, "nstjml", "nstidserialin = " & nstidserialin & "")
                ftSerial = IIf(Len(ftSerial.ToString) = 0, "", ftSerial & " OR ")
                ftSerial = String.Concat(ftSerial, " (nsi.nsiidserialin = " & nstidserialin & " AND " & jmlKeluar & " > nsi.nsijmlsisa) ")

                '3. SET NILAI UPDATE SERIAL IN 
                updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & nstidserialin & "' THEN ROUND(nsijmlkeluar + '" & jmlKeluar & "', 5) ", updNilaiSerial)

                '4. SET FILTER UPDATE SERIAL IN 
                updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & nstidserialin & "')")
                'END OF BUAT FILTER UNTUK VALIDASI --------------------------

            Next
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
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

                'CEK PERIODE AKUNTANSI ==================================
                Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("prttgl")), AsFormatTanggal(drutama("prttgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("prtstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'ValidasiHppI
                    rsValidasi = ValidasiHppI(dtdetail, ftBarang)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    ''ValidasiHppF
                    'rsValidasi = ValidasiHppF(dtdetail, ftBarang)
                    'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim dtasset As New DataTable
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingRI, ftOutstandingRI, ftExistOutstandingDNR, ftOutstandingDNR, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangtransit", ftRI, ftDNR, drutama("prthargatermasukpajak"), dtasset)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                'FUNGSI SET TANGGAL JATUH TEMPO DIHILANGKAN, KARENA di flex tambah inputan
                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("prttermin").ToString, AsFormatTanggal(drutama("prttgl")), "prttgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("prttgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'SET TANGGAL JATUH TEMPO BERDASARKAN SETTING
                'JIKA SETTING BERDASARKAN TUKAR FAKTUR MAKA TANGGAL JATUH TEMPO DISET 2100-12-31
                Dim setTglJT As String = F_getSetting(4, "tukarfaktur", "UpdateTglJatuhTempoPRT")
                If setTglJT.Equals("1") Then
                    drutama("prttgljatuhtempo") = "2100-12-31"
                End If


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("prttotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("prttotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("prttotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("prthargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("prttotaltransaksi") = Double.Parse(drutama("prttotal")) - Double.Parse(drutama("prtjmldiskon")) + Double.Parse(drutama("prttotalpajak1detail")) + Double.Parse(drutama("prttotalpajak2detail")) + Double.Parse(drutama("prtbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("prttotaltransaksi") = Double.Parse(drutama("prttotal")) - Double.Parse(drutama("prtjmldiskon")) + Double.Parse(drutama("prtbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA RETUR LANGSUNG MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("prtjenis")) = 1 Then
                    drutama("prtjmlbayar") = drutama("prttotaltransaksi")
                    drutama("prttgllunas") = drutama("prttgl")
                    drutama("prtstatuslunas") = 2

                Else
                    drutama("prtjmlbayar") = 0 : drutama("prttgllunas") = "1900-01-01" : drutama("prtstatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("prtid")
                    notransaksi = drutama("prtnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(prtid), prtnotransaksi FROM M4_prt WHERE prtid='" & result(4) & "' AND prtstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(prtid) FROM M4_prt WHERE prtnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_prt_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prtsumber")) & "▼" & FixQuotes(drutama("prtid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Prt set prtcabang  = '" & FixQuotes(drutama("prtcabang")) & "', prtlokasi  = '" & FixQuotes(drutama("prtlokasi")) & "', prtgudang  = '" & FixQuotes(drutama("prtgudang")) & "', prtasalbarang  = '" & FixQuotes(drutama("prtasalbarang")) & "', prtasalbarangkategori  = " & drutama("prtasalbarangkategori") & ", prtjenispembelian  = '" & FixQuotes(drutama("prtjenispembelian")) & "', prtjenispembeliankategori  = " & drutama("prtjenispembeliankategori") & ", prtcarabayar  = " & drutama("prtcarabayar") & ", prtsumber  = '" & FixQuotes(drutama("prtsumber")) & "', prtautonotransaksi  = " & drutama("prtautonotransaksi") & ", prtnotransaksi  = '" & FixQuotes(notransaksi) & "', prttgl  = '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', prtkodepa  = " & drutama("prtkodepa") & ", prtsupplier  = " & drutama("prtsupplier") & ", prtsupplierkontak  = '" & FixQuotes(drutama("prtsupplierkontak")) & "', prt1alamat1  = '" & FixQuotes(drutama("prt1alamat1")) & "', prt1alamat2  = '" & FixQuotes(drutama("prt1alamat2")) & "', prt1alamat3  = '" & FixQuotes(drutama("prt1alamat3")) & "', prt2alamat1  = '" & FixQuotes(drutama("prt2alamat1")) & "', prt2alamat2  = '" & FixQuotes(drutama("prt2alamat2")) & "', prt2alamat3  = '" & FixQuotes(drutama("prt2alamat3")) & "', prtbagianpembelian  = " & drutama("prtbagianpembelian") & ", prttermin  = '" & FixQuotes(drutama("prttermin")) & "', prttgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', prturaian  = '" & FixQuotes(drutama("prturaian")) & "', prtcatatan  = '" & FixQuotes(drutama("prtcatatan")) & "', prtnoref  = '" & FixQuotes(drutama("prtnoref")) & "', prttglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', prttglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', prtmatauang  = '" & FixQuotes(drutama("prtmatauang")) & "', prtkurs  = '" & FixDouble(drutama("prtkurs")) & "', prthargatermasukpajak  = " & drutama("prthargatermasukpajak") & ", prttotal  = '" & FixDouble(drutama("prttotal")) & "', prtdiskonpersen  = '" & FixQuotes(drutama("prtdiskonpersen")) & "', prtjmldiskon  = '" & FixDouble(drutama("prtjmldiskon")) & "', prttotalpajak1detail  = '" & FixDouble(drutama("prttotalpajak1detail")) & "', prttotalpajak2detail  = '" & FixDouble(drutama("prttotalpajak2detail")) & "', prtbiayalainpersen  = '" & FixQuotes(drutama("prtbiayalainpersen")) & "', prtbiayalain  = '" & FixDouble(drutama("prtbiayalain")) & "', prttotaltransaksi  = '" & FixDouble(drutama("prttotaltransaksi")) & "', prtsisatransaksi  = '" & FixDouble(drutama("prtsisatransaksi")) & "', prtjmlbayar  = '" & FixDouble(drutama("prtjmlbayar")) & "', prtstatuslunas  = " & drutama("prtstatuslunas") & ", prttgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', prtnofakturpajak  = '" & FixQuotes(drutama("prtnofakturpajak")) & "', prtsdhbayarpajak  = " & drutama("prtsdhbayarpajak") & ", prttglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', prtrekdiskon  = '" & FixQuotes(drutama("prtrekdiskon")) & "', prtrekpajak1  = '" & FixQuotes(drutama("prtrekpajak1")) & "', prtrekpajak2  = '" & FixQuotes(drutama("prtrekpajak2")) & "', prtrekbiayalain  = '" & FixQuotes(drutama("prtrekbiayalain")) & "', prtrekbayar  = '" & FixQuotes(drutama("prtrekbayar")) & "', prtreksisa  = '" & FixQuotes(drutama("prtreksisa")) & "', prtidpr  = " & drutama("prtidpr") & ", prtidcs  = " & drutama("prtidcs") & ", prtidrq  = " & drutama("prtidrq") & ", prtidbs  = " & drutama("prtidbs") & ", prtidpo  = " & drutama("prtidpo") & ", prtidipc  = " & drutama("prtidipc") & ", prtidgrn  = " & drutama("prtidgrn") & ", prtidri  = " & drutama("prtidri") & ", prtiddnr  = " & drutama("prtiddnr") & ", prtstatus  = " & drutama("prtstatus") & ", prtstatussebelumnya  = " & drutama("prtstatussebelumnya") & ", prtjmlrevisi  = prtjmlrevisi+1, prtcetakanke  = " & drutama("prtcetakanke") & ", prtmodifikasiuser  = " & drutama("prtmodifikasiuser") & ", prtmodifikasitgl  = NOW(), prtposting  = 0, prttutupperiode  = " & drutama("prttutupperiode") & ", prtcustomtext1  = '" & FixQuotes(drutama("prtcustomtext1")) & "', prtcustomtext2  = '" & FixQuotes(drutama("prtcustomtext2")) & "', prtcustomtext3  = '" & FixQuotes(drutama("prtcustomtext3")) & "', prtcustomtext4  = '" & FixQuotes(drutama("prtcustomtext4")) & "', prtcustomtext5  = '" & FixQuotes(drutama("prtcustomtext5")) & "', prtcustomint1  = " & drutama("prtcustomint1") & ", prtcustomint2  = " & drutama("prtcustomint2") & ", prtcustomint3  = " & drutama("prtcustomint3") & ", prtcustomdbl1  = '" & FixDouble(drutama("prtcustomdbl1")) & "', prtcustomdbl2  = '" & FixDouble(drutama("prtcustomdbl2")) & "', prtcustomdbl3  = '" & FixDouble(drutama("prtcustomdbl3")) & "', prtcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', prtcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', prtcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', prtjenis = '" & FixQuotes(drutama("prtjenis")) & "' where prtid = '" & drutama("prtid") & "'"
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

                    If drutama("prtautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prtcabang"), drutama("prtlokasi"), drutama("prtsumber"), drutama("prttgl"))
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
                        notransaksi = drutama("prtnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(prtid) FROM m4_prt WHERE prtnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Prt (prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtjenis) values('" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(drutama("prtgudang")) & "', '" & FixQuotes(drutama("prtasalbarang")) & "', " & drutama("prtasalbarangkategori") & ", '" & FixQuotes(drutama("prtjenispembelian")) & "', " & drutama("prtjenispembeliankategori") & ", " & drutama("prtcarabayar") & ", '" & FixQuotes(drutama("prtsumber")) & "', " & drutama("prtautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtkodepa") & ", " & drutama("prtsupplier") & ", '" & FixQuotes(drutama("prtsupplierkontak")) & "', '" & FixQuotes(drutama("prt1alamat1")) & "', '" & FixQuotes(drutama("prt1alamat2")) & "', '" & FixQuotes(drutama("prt1alamat3")) & "', '" & FixQuotes(drutama("prt2alamat1")) & "', '" & FixQuotes(drutama("prt2alamat2")) & "', '" & FixQuotes(drutama("prt2alamat3")) & "', " & drutama("prtbagianpembelian") & ", '" & FixQuotes(drutama("prttermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(drutama("prtnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', '" & FixQuotes(drutama("prtmatauang")) & "', '" & FixDouble(drutama("prtkurs")) & "', " & drutama("prthargatermasukpajak") & ", '" & FixDouble(drutama("prttotal")) & "', '" & FixQuotes(drutama("prtdiskonpersen")) & "', '" & FixDouble(drutama("prtjmldiskon")) & "', '" & FixDouble(drutama("prttotalpajak1detail")) & "', '" & FixDouble(drutama("prttotalpajak2detail")) & "', '" & FixQuotes(drutama("prtbiayalainpersen")) & "', '" & FixDouble(drutama("prtbiayalain")) & "', '" & FixDouble(drutama("prttotaltransaksi")) & "', '" & FixDouble(drutama("prtsisatransaksi")) & "', '" & FixDouble(drutama("prtjmlbayar")) & "', " & drutama("prtstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', '" & FixQuotes(drutama("prtnofakturpajak")) & "', " & drutama("prtsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', '" & FixQuotes(drutama("prtrekdiskon")) & "', '" & FixQuotes(drutama("prtrekpajak1")) & "', '" & FixQuotes(drutama("prtrekpajak2")) & "', '" & FixQuotes(drutama("prtrekbiayalain")) & "', '" & FixQuotes(drutama("prtrekbayar")) & "', '" & FixQuotes(drutama("prtreksisa")) & "', " & drutama("prtidpr") & ", " & drutama("prtidcs") & ", " & drutama("prtidrq") & ", " & drutama("prtidbs") & ", " & drutama("prtidpo") & ", " & drutama("prtidipc") & ", " & drutama("prtidgrn") & ", " & drutama("prtidri") & ", " & drutama("prtiddnr") & ", " & drutama("prtstatus") & ", " & drutama("prtstatussebelumnya") & ", " & drutama("prtjmlrevisi") & ", " & drutama("prtcetakanke") & ", " & drutama("prtinputuser") & ", NOW(), " & drutama("prtmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("prttutupperiode") & ", " & drutama("prtisclose") & ", '" & FixQuotes(drutama("prtcustomtext1")) & "', '" & FixQuotes(drutama("prtcustomtext2")) & "', '" & FixQuotes(drutama("prtcustomtext3")) & "', '" & FixQuotes(drutama("prtcustomtext4")) & "', '" & FixQuotes(drutama("prtcustomtext5")) & "', " & drutama("prtcustomint1") & ", " & drutama("prtcustomint2") & ", " & drutama("prtcustomint3") & ", '" & FixDouble(drutama("prtcustomdbl1")) & "', '" & FixDouble(drutama("prtcustomdbl2")) & "', '" & FixDouble(drutama("prtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', '" & FixQuotes(drutama("prtjenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select prtid from M4_prt where prtnotransaksi='" & notransaksi & "' AND prtinputuser= '" & userid & "' order by prtmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Prt_Detail where idprt = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idprtdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekreturpembelian")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", " & dr1("idridetail") & ", " & dr1("iddnrdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Prt_Detail(idprtdetail, idprt, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, iddnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'VALIDASI KETIKA PRT LANGSUNG (PRTJENIS = 1) MAKA TIDAK BOLEH AMBIL LEBIH DARI 1 NOMOR RI
                Dim IdRI As Double = 0
                If drutama("prtjenis") = 1 Then
                    sql = "SELECT ri.riid, ri.rinotransaksi, ri.ritotaltransaksi, ri.rijmlbayar FROM M4_Prt_detail Prtd JOIN M4_ri_detail rid ON Prtd.idridetail = rid.idridetail JOIN M4_ri ri ON rid.idri = ri.riid WHERE Prtd.idPrt = '" & result(4) & "' GROUP BY ri.riid"
                    Dim dtCekRi As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtCekRi.Rows.Count > 1 Then
                        result(2) = "Direct PRT (Purchase Retur) can only pick from one RI (Receive Invoice) transaction." : Trans.Rollback() : GoTo selesai

                    ElseIf dtCekRi.Rows.Count = 1 Then
                        'VALIDASI KETIKA PRT LANGSUNG (PRTJENIS = 1) MAKA TOTAL TRANSAKSI PRT TIDAK BOLEH MELEBIHI SISA RI YANG BELUM DIBAYAR
                        If Len(dtCekRi.Rows(0)("Riid")) > 0 Then
                            IdRI = Double.Parse(dtCekRi.Rows(0)("Riid"))
                            If Double.Parse(drutama("Prttotaltransaksi")) > (Double.Parse(dtCekRi.Rows(0)("Ritotaltransaksi")) - Double.Parse(dtCekRi.Rows(0)("Rijmlbayar"))) Then
                                Dim selisih(2) As String
                                selisih = F_Nominal(F_Round((Double.Parse(dtCekRi.Rows(0)("Ritotaltransaksi")) - Double.Parse(dtCekRi.Rows(0)("Rijmlbayar")))), True).Split(sptSubParam)

                                result(2) = "Total Direct PRT (Purchase Retur) exceeds the AP (Account Payables) from RI (Receive Invoice) transaction no. " & dtCekRi.Rows(0)("Rinotransaksi") & ". AP available : " & drutama("Prtmatauang") & " " & selisih(1) : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                    End If
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'PRT'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'PRT'"
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


                If drutama("prtstatus") = 2 Then
                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiRI) > 0 Then 'RI
                        'UPDATE DETAIL
                        sql = "UPDATE m4_ri_detail SET jmlrealisasi = (CASE idridetail " & updNilaiRI & " ELSE jmlrealisasi END) WHERE " & updFilterRI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idri FROM M4_ri_detail WHERE " & updFilterRI & " GROUP BY idri")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_ri_detail WHERE " & ftDetail & " GROUP BY idri")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRI = "" : updFilterRI = ""
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
                                updNilaiRI = String.Concat(updNilaiRI, "WHEN '" & dr1("idri") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                                updFilterRI = String.Concat(updFilterRI, "(riid = '" & dr1("idri") & "')")
                            Next

                            sql = "UPDATE m4_ri SET ristatusrealisasi = (CASE riid " & updNilaiRI & " ELSE ristatusrealisasi END) WHERE " & updFilterRI
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

                    If Len(updNilaiDNR) > 0 Then 'DNR
                        'UPDATE DETAIL
                        sql = "UPDATE m4_dnr_detail SET jmlrealisasi = (CASE iddnrdetail " & updNilaiDNR & " ELSE jmlrealisasi END) WHERE " & updFilterDNR
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT iddnr FROM m4_dnr_detail WHERE " & updFilterDNR & " GROUP BY iddnr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(iddnr = '" & dr1("iddnr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT iddnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_dnr_detail WHERE " & ftDetail & " GROUP BY iddnr")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiDNR = "" : updFilterDNR = ""
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
                                updNilaiDNR = String.Concat(updNilaiDNR, "WHEN '" & dr1("iddnr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                                updFilterDNR = String.Concat(updFilterDNR, "(dnrid = '" & dr1("iddnr") & "')")
                            Next

                            sql = "UPDATE m4_dnr SET dnrstatusrealisasi = (CASE dnrid " & updNilaiDNR & " ELSE dnrstatusrealisasi END) WHERE " & updFilterDNR
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")
                        Next

                        'INSERT NO BATCH OUT ---------------------------------
                        sql = "Insert into M1_No_Batch_Out(nboid, nboidbatchin, nbogudang, nboidbarang, nbokode, nbosumber, nboidtransaksi, nbosatuan, nbojmlkeluar, nboisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO BATCH IN KELUAR ---------------------------
                        If Len(updNilaiBatch) > 0 Then
                            sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                    'END OF INSERT NO BATCH =========================================================

                    'INSERT NO SERIAL ===============================================================
                    If dtserial.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")
                        Next

                        'INSERT NO SERIAL OUT --------------------------------
                        sql = "Insert into M1_No_Serial_Out(nsoid, nsoidserialin, nsogudang, nsoidbarang, nsokode, nsosumber, nsoidtransaksi, nsosatuan, nsojmlkeluar, nsoisclose) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE NO SERIAL IN KELUAR --------------------------
                        If Len(updNilaiSerial) > 0 Then
                            sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                    'END OF INSERT NO SERIAL ========================================================


                    'JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI =========================
                    If drutama("prtjenis") = 1 And IdRI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET ri.rijmlbayar = ri.rijmlbayar + " & Double.Parse(drutama("prttotaltransaksi")) & ", ri.ritgllunas = (CASE WHEN ri.rijmlbayar + " & Double.Parse(drutama("prttotaltransaksi")) & " >= ri.ritotaltransaksi THEN '" & AsFormatTanggal(FixQuotes(drutama("prttgl"))) & "' ELSE ri.ritgllunas END) WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI ==================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB("SELECT prtd.idprtdetail, prtd.idbarang, prtd.namabarang, prtd.tipebarang, prtd.jml, prtd.satuan, prtd.jmlbarang, prtd.satuanbarang, prtd.matauang, prtd.kurs, prtd.harga, prtd.diskon, prtd.jmldiskon, prtd.idhppkhususmasuk, prtd.hpp, prtd.gudangasal, prtd.gudangtransit, prtd.gudangtujuan, prtd.catatan, prtd.costcenter, prtd.divisi, prtd.subdivisi, prtd.proyek, prt.prtinputtgl, i.bhpp FROM m4_prt_detail prtd JOIN m4_prt prt ON prtd.idprt = prt.prtid JOIN m1_item i ON prtd.idbarang = i.bid WHERE prtd.idprt = '" & result(4) & "'")

                    Dim hpp As Double = 0, postinghpp As Double = 0, gudang As String = "", bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ====================================================
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            gudang = dr1("gudangtransit")

                            'AMBIL DATA STOK DAN HPPAVERAGE TERBARU
                            sql = "SELECT bstok FROM m1_item WHERE bid = '" & FixDouble(idbarang) & "'"
                            dtSaldo = AsDataTableAmbilDariDB(sql)
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
                                'mapping                        id,                            cabang,                                    lokasi,                                    gudang,                         kodepa,             jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                                idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("prtkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("prtsumber")) & "', " & result(4) & ", " & dr1("idprtdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("prtinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("prtinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES ('" & idbarang & "','" & gudang & "','-" & jmlbarang & "') ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = Con1
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                                'UPDATE STOK GLOBAL
                                sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
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
                End If


                'INSERT MSMQ JURNAL =================================================================
                Dim sumber As String = "PRT", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("prtstatus") = 2 Then
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


                'INSERT MSMQ HPP ====================================================================
                If drutama("prtstatus") = 2 Then
                    Dim Security As New ClsSecurity, mjid As String = "", hasilMsmq As String = ""
                    'BUAT ID UNIQUE
                    mjid = Security.MD5CalcString("C" & userid & sumber & result(4) & Now) 'RandomId.Generate(15)

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
                'END OF INSERT MSMQ HPP =============================================================


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
    Public Function M4_PrtUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "PRT", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim prtjenis As Integer = 0, prttotaltransaksi As Double = 0

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT prttgl, prtnotransaksi, prtstatus, prtjenis, prttotaltransaksi FROM M4_Prt WHERE Prtid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'prtjenis                                        'prttotaltransaksi
                prtjenis = Integer.Parse(dtdetail.Rows(1)(3)) : prttotaltransaksi = Double.Parse(dtdetail.Rows(1)(4))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Prtstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_prt_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_prt_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idridetail As Integer = 0, iddnrdetail As Integer = 0, idhppkhususmasuk As Integer = 0
                Dim updNilaiRI As String = "", updFilterRI As String = "", updNilaiDNR As String = "", updFilterDNR As String = ""
                Dim gudangIn As String = "", updStokIn As String = "", updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idprtdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idhppkhususmasuk, gudangtransit, gudangtujuan, idridetail, iddnrdetail, urutan FROM m4_prt_detail WHERE idprt = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idhppkhususmasuk = dr1("idhppkhususmasuk") : gudangIn = dr1("gudangtransit") : idridetail = dr1("idridetail") : iddnrdetail = dr1("iddnrdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idridetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING RI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                            updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRI)
                            '2. SET FILTERUPDATE OUTSTANDING RI
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
                        End If

                        If iddnrdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING DNR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "iddnrdetail=" & iddnrdetail)
                            updNilaiDNR = String.Concat("WHEN '" & iddnrdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiDNR)
                            '2. SET FILTERUPDATE OUTSTANDING DNR
                            updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                            updFilterDNR = String.Concat(updFilterDNR, "(iddnrdetail = '" & iddnrdetail & "')")
                        End If

                        'SET NILAI UPDATE STOK MASUK --------------
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        'END OF BUAT FILTER UNTUK UPDATE --------------------------

                        '4. BUAT FILTER UPDATE HPP KHUSUS (I)
                        If idhppkhususmasuk <> 0 Then
                            'SET NILAI UPDATE HPP KHUSUS IN
                            Dim jmlKeluar As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idhppkhususmasuk='" & idhppkhususmasuk & "'")
                            updNilaiHppI = String.Concat("WHEN '" & idhppkhususmasuk & "' THEN ROUND(jmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppI)
                            'SET FILTER UPDATE HPP KHUSUS IN
                            updFilterHppI = IIf(Len(updFilterHppI.ToString) = 0, "", updFilterHppI & " OR ")
                            updFilterHppI = String.Concat(updFilterHppI, "(idhppikm = '" & idhppkhususmasuk & "')")
                            'SET FILTER DELETE HPP KHUSUS OUT
                            delFilterHppI = IIf(Len(delFilterHppI.ToString) = 0, "", delFilterHppI & " OR ")
                            delFilterHppI = String.Concat(delFilterHppI, "(sumber = 'PRT' AND idtransaksi = '" & dr1("idprtdetail") & "')")
                        End If

                        '5. BUAT FILTER UPDATE HPP FIFO (F)
                        filterHppF = IIf(Len(filterHppF.ToString) = 0, "", filterHppF & " OR ")
                        filterHppF = String.Concat(filterHppF, "(cfosumber = 'PRT' AND cfoidtransaksi = '" & dr1("idprtdetail") & "')")

                        '6 SET NILAI UPDATE STOK BARANG
                        Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                        updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & stokBarang & "', 5) ", updStokBarang)
                        '7. SET FILTERUPDATE STOK BARANG
                        ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                        ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'CEK HPP FIFO ====================================================================
                'AMBIL DATA DARI HPP FIFO KELUAR - m1_cogs_fifo_out
                Dim dtHppF As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m1_cogs_fifo_out WHERE " & filterHppF)
                If dtHppF.Rows.Count > 0 Then
                    Dim idhppfifoin As Integer = 0
                    For Each dr1 As DataRow In dtHppF.Rows
                        'SET NILAI VARIABEL
                        idhppfifoin = dr1("cfoidcfi")

                        'SET FILTER DELETE HPP FIFO OUT
                        delFilterHppF = IIf(Len(delFilterHppF.ToString) = 0, "", delFilterHppF & " OR ")
                        delFilterHppF = String.Concat(delFilterHppF, "(cfosumber = 'PRT' AND cfoidtransaksi = '" & dr1("cfoidtransaksi") & "')")
                        'SET NILAI UPDATE HPP FIFO IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtHppF, "cfojmlkeluar", "cfoidcfi='" & idhppfifoin & "'")
                        updNilaiHppF = String.Concat("WHEN '" & idhppfifoin & "' THEN ROUND(cfijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiHppF)
                        'SET FILTER UPDATE HPP FIFO IN
                        updFilterHppF = IIf(Len(updFilterHppF.ToString) = 0, "", updFilterHppF & " OR ")
                        updFilterHppF = String.Concat(updFilterHppF, "(cfiid = '" & idhppfifoin & "')")
                    Next
                End If
                'END OF CEK HPP FIFO =============================================================


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterRI) > 0 Then 'RI
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m4_ri_detail SET jmlrealisasi = (CASE idridetail " & updNilaiRI & " ELSE jmlrealisasi END) WHERE " & updFilterRI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idri FROM M4_ri_detail WHERE " & updFilterRI & " GROUP BY idri")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM M4_ri_detail WHERE " & ftDetail & " GROUP BY idri")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRI = "" : updFilterRI = ""
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
                            updNilaiRI = String.Concat(updNilaiRI, "WHEN '" & dr1("idri") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(riid = '" & dr1("idri") & "')")
                        Next

                        sql = "UPDATE m4_ri SET ristatusrealisasi = (CASE riid " & updNilaiRI & " ELSE ristatusrealisasi END) WHERE " & updFilterRI
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

                If Len(updFilterDNR) > 0 Then 'DNR
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m4_dnr_detail SET jmlrealisasi = (CASE iddnrdetail " & updNilaiDNR & " ELSE jmlrealisasi END) WHERE " & updFilterDNR
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT iddnr FROM m4_dnr_detail WHERE " & updFilterDNR & " GROUP BY iddnr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(iddnr = '" & dr1("iddnr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT iddnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_dnr_detail WHERE " & ftDetail & " GROUP BY iddnr")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiDNR = "" : updFilterDNR = ""
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
                            updNilaiDNR = String.Concat(updNilaiDNR, "WHEN '" & dr1("iddnr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterDNR = IIf(Len(updFilterDNR.ToString) = 0, "", updFilterDNR & " OR ")
                            updFilterDNR = String.Concat(updFilterDNR, "(dnrid = '" & dr1("iddnr") & "')")
                        Next

                        sql = "UPDATE m4_dnr SET dnrstatusrealisasi = (CASE dnrid " & updNilaiDNR & " ELSE dnrstatusrealisasi END) WHERE " & updFilterDNR
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


                'JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI =========================
                If prtjenis = 1 Then
                    'AMBIL IDRI DARI DATA PRT DETAIL
                    sql = "SELECT rid.idri FROM m4_prt_detail prtd JOIN m4_ri_detail rid ON prtd.idridetail = rid.idridetail WHERE prtd.idprt = '" & idtransaksi & "' GROUP BY rid.idri"
                    Dim dtRI As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim IdRI As Double = 0
                    If dtRI.Rows.Count > 0 Then
                        If Len(dtRI.Rows(0)("idri")) > 0 Then
                            IdRI = Double.Parse(dtRI.Rows(0)("idri"))
                        End If
                    End If

                    'UPDATE JMLBAYAR RI
                    If IdRI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET ri.rijmlbayar = ri.rijmlbayar - " & prttotaltransaksi & ", ri.ritgllunas = '" & FixQuotes("1900-01-01") & "' WHERE ri.riid = '" & IdRI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m4_ri ri LEFT JOIN m2_transaction_journal t ON ri.risumber = t.tsumber AND ri.riid = t.tidtransaksi AND ri.rinotransaksi = t.tnotransaksi SET t.tstatuslunas = ri.ristatuslunas, t.ttgllunas = ri.ritgllunas WHERE ri.riid = '" & IdRI & "'"
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
                'END OF JIKA PRT LANGSUNG (PRTJENIS = 1) MAKA UPDATE JMLBAYAR RI ==================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
                    'DELETE NO BATCH OUT --------------------------------
                    sql = "DELETE FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO BATCH IN KELUAR --------------------------
                    For Each dr1 As DataRow In dtBatch.Rows
                        'SET NILAI UPDATE BATCH IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtBatch, "nbojmlkeluar", "nboidbatchin = " & dr1("nboidbatchin") & "")
                        updNilaiBatch = String.Concat("WHEN nbiidbatchin = '" & dr1("nboidbatchin") & "' THEN ROUND(nbijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiBatch)

                        'SET FILTER UPDATE BATCH IN
                        updFilterBatch = IIf(Len(updFilterBatch.ToString) = 0, "", updFilterBatch & " OR ")
                        updFilterBatch = String.Concat(updFilterBatch, "(nbiidbatchin = '" & dr1("nboidbatchin") & "')")
                    Next
                    If Len(updNilaiBatch) > 0 Then
                        sql = "UPDATE m1_no_batch_in SET nbijmlkeluar =  (CASE " & updNilaiBatch & " ELSE nbijmlkeluar END) WHERE " & updFilterBatch
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
                'END OF UPDATE NO BATCH =========================================================


                'UPDATE NO SERIAL ===============================================================
                Dim updNilaiSerial As String = "", updFilterSerial As String = ""
                Dim dtSerial As DataTable = AsDataTableAmbilDariDB("SELECT nsoidserialin, nsogudang, nsoidbarang, nsokode, nsojmlkeluar FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'")
                If dtSerial.Rows.Count > 0 Then
                    'DELETE NO SERIAL OUT -------------------------------
                    sql = "DELETE FROM m1_no_serial_out WHERE nsosumber = '" & sumber & "' AND nsoidtransaksi = '" & idtransaksi & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE NO SERIAL IN KELUAR -------------------------
                    For Each dr1 As DataRow In dtSerial.Rows
                        'SET NILAI UPDATE SERIAL IN
                        Dim jmlKeluar As Double = AsDataTableDSum(dtSerial, "nsojmlkeluar", "nsoidserialin = " & dr1("nsoidserialin") & "")
                        updNilaiSerial = String.Concat("WHEN nsiidserialin = '" & dr1("nsoidserialin") & "' THEN ROUND(nsijmlkeluar - '" & jmlKeluar & "', 5) ", updNilaiSerial)

                        'SET FILTER UPDATE SERIAL IN
                        updFilterSerial = IIf(Len(updFilterSerial.ToString) = 0, "", updFilterSerial & " OR ")
                        updFilterSerial = String.Concat(updFilterSerial, "(nsiidserialin = '" & dr1("nsoidserialin") & "')")
                    Next
                    If Len(updNilaiSerial) > 0 Then
                        sql = "UPDATE m1_no_serial_in SET nsijmlkeluar =  (CASE " & updNilaiSerial & " ELSE nsijmlkeluar END) WHERE " & updFilterSerial
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
                'END OF UPDATE NO SERIAL =======================================================


                'UPDATE HPP KHUSUS (I) =========================================================
                'DELETE HPP KHUSUS OUT
                If Len(delFilterHppI) > 0 Then
                    sql = "DELETE FROM m1_cogs_special_out WHERE " & delFilterHppI
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
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
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP KHUSUS (I) ==================================================


                'UPDATE HPP FIFO (F) ===========================================================
                'DELETE HPP FIFO OUT
                If Len(delFilterHppF) > 0 Then
                    sql = "DELETE FROM m1_cogs_fifo_out WHERE " & delFilterHppF
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
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
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE HPP FIFO (F) ====================================================


                'UPDATE STOK ====================================================================
                'STOK MASUK
                If Len(updStokIn) > 0 Then
                    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
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
                sql = "UPDATE m1_item SET bstok = (CASE bid " & updStokBarang & " ELSE bstok END) WHERE " & ftStokBarang
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
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
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF DELETE TRANSAKSI BARANG =================================================


                'UPDATE BHPPAVERAGE M1_ITEM ===================================================
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT prtd.idbarang, ROUND(SUM(prtd.jmlbarang * prtd.hpp),2) as nilai, SUM(prtd.jmlbarang) as jumlah"
                sql &= " FROM m4_prt_detail prtd"
                sql &= " WHERE prtd.jmlbarang <> 0 AND prtd.idprt = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY prtd.idbarang"
                sql &= " ) as h ON i.bid = h.idbarang"
                sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2) END) ELSE IFNULL(ROUND((((i.bstok - h.jumlah) * i.bhppaverage) + (h.nilai)) / (i.bstok),2),0) END)"
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PRT' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Prt SET Prtstatus = " & nilaiStatus & ", Prtmodifikasiuser='" & userid & "', Prtmodifikasitgl = NOW(), Prtposting = 0, Prtpostingtgl = '1971-01-01 00:00:00', Prtjmlrevisi = Prtjmlrevisi + 1 WHERE Prtid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "PRT", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Prtid, Prtnotransaksi FROM M4_Prt WHERE Prtid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT prtcabang, prtlokasi, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl"
            sql &= " FROM M4_prt"
            sql &= " WHERE prtid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("prtcabang")
                lokasi = dtNomorNext.Rows(0)("prtlokasi")
                sumber = dtNomorNext.Rows(0)("prtsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("prtautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("prtnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("prttgl"))
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


            'DELETE DETAIL
            sql = "DELETE FROM M4_Prt_Detail WHERE idprt='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Prt WHERE prtid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtBalance(ByVal param As String) As String
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
        'prtid(0) As Integer, prtcabang(1) As String, prtlokasi(2) As String, prtgudang(3) As String, prtasalbarang(4) As String, 
        'prtasalbarangkategori(5) As Integer, prtjenispembelian(6) As String, prtjenispembeliankategori(7) As Integer, prtcarabayar(8) As Integer, prtsumber(9) As String, 
        'prtautonotransaksi(10) As Integer, prtnotransaksi(11) As String, prttgl(12) As Date, prtkodepa(13) As Integer, prtsupplier(14) As Integer, 
        'prtsupplierkontak(15) As String, prt1alamat1(16) As String, prt1alamat2(17) As String, prt1alamat3(18) As String, prt2alamat1(19) As String, 
        'prt2alamat2(20) As String, prt2alamat3(21) As String, prtbagianpembelian(22) As Integer, prttermin(23) As String, prttgljatuhtempo(24) As Date, 
        'prturaian(25) As String, prtcatatan(26) As String, prtnoref(27) As String, prttglnoref(28) As Date, prttglpenutupan(29) As Date, 
        'prtmatauang(30) As String, prtkurs(31) As Double, prthargatermasukpajak(32) As Integer, prttotal(33) As Double, prtdiskonpersen(34) As String, 
        'prtjmldiskon(35) As Double, prttotalpajak1detail(36) As Double, prttotalpajak2detail(37) As Double, prtbiayalainpersen(38) As String, prtbiayalain(39) As Double, 
        'prttotaltransaksi(40) As Double, prtsisatransaksi(41) As Double, prtjmlbayar(42) As Double, prtstatuslunas(43) As Integer, prttgllunas(44) As Date, 
        'prtnofakturpajak(45) As String, prtsdhbayarpajak(46) As Integer, prttglbayarpajak(47) As Date, prtrekdiskon(48) As String, prtrekpajak1(49) As String, 
        'prtrekpajak2(50) As String, prtrekbiayalain(51) As String, prtrekbayar(52) As String, prtreksisa(53) As String, prtidpr(54) As Integer, 
        'prtidcs(55) As Integer, prtidrq(56) As Integer, prtidbs(57) As Integer, prtidpo(58) As Integer, prtidipc(59) As Integer, 
        'prtidgrn(60) As Integer, prtidri(61) As Integer, prtiddnr(62) As Integer, prtstatus(63) As Integer, prtstatussebelumnya(64) As Integer, 
        'prtjmlrevisi(65) As Integer, prtcetakanke(66) As Integer, prtinputuser(67) As Integer, prtinputtgl(68) As DateTime, prtmodifikasiuser(69) As Integer, 
        'prtmodifikasitgl(70) As DateTime, prtposting(71) As Integer, prttutupperiode(72) As Integer, prtisclose(73) As Integer, prtcustomtext1(74) As String, 
        'prtcustomtext2(75) As String, prtcustomtext3(76) As String, prtcustomtext4(77) As String, prtcustomtext5(78) As String, prtcustomint1(79) As Integer, 
        'prtcustomint2(80) As Integer, prtcustomint3(81) As Integer, prtcustomdbl1(82) As Double, prtcustomdbl2(83) As Double, prtcustomdbl3(84) As Double, 
        'prtcustomdate1(85) As Date, prtcustomdate2(86) As Date, prtcustomdate3(87) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, 
        'prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, 
        'prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "prtid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prt2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prturaian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prthargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prttotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtidri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtiddnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prttutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "prtcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "prtcustomdate3", AsEnumTypeData.AsString)



        Dim JmlDt As Integer = dataUtama.Length
        For i = 1 To JmlDt
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Utama -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 88) Then
                result(2) = "Invalid main transaction data parameter. " & dataRowUtama.Length & "" : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Utama ----------------------------


            'VALIDASI TIPE DATA UTAMA ==========================================================
            'prtid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "prtid required numeric." : GoTo selesai
            End If
            'prtasalbarangkategori(5) As Integer
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "prtasalbarangkategori required numeric." : GoTo selesai
            End If
            'prtjenispembeliankategori(7) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "prtjenispembeliankategori required numeric." : GoTo selesai
            End If
            'prtcarabayar(8) As Integer
            If (IsNumeric(dataRowUtama(8)) = False) Then
                result(2) = "prtcarabayar required numeric." : GoTo selesai
            End If
            'prtautonotransaksi(10) As Integer
            If (IsNumeric(dataRowUtama(10)) = False) Then
                result(2) = "prtautonotransaksi required numeric." : GoTo selesai
            End If
            'prttgl(12) As Date
            If (IsDate(dataRowUtama(12)) = False) Then
                result(2) = "prttgl required date." : GoTo selesai
            End If
            'prtkodepa(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "prtkodepa required numeric." : GoTo selesai
            End If
            'prtsupplier(14) As Integer
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "prtsupplier required numeric." : GoTo selesai
            End If
            If (dataRowUtama(14) < 1) Then
                result(2) = "prtsupplier can't be empty." : GoTo selesai
            End If
            'prtbagianpembelian(22) As Integer
            If (IsNumeric(dataRowUtama(22)) = False) Then
                result(2) = "prtbagianpembelian required numeric." : GoTo selesai
            End If
            'prttgljatuhtempo(24) As Date
            If (IsDate(dataRowUtama(24)) = False) Then
                result(2) = "prttgljatuhtempo required date." : GoTo selesai
            End If
            'prttglnoref(28) As Date
            If (IsDate(dataRowUtama(28)) = False) Then
                result(2) = "prttglnoref required date." : GoTo selesai
            End If
            'prttglpenutupan(29) As Date
            If (IsDate(dataRowUtama(29)) = False) Then
                result(2) = "prttglpenutupan required date." : GoTo selesai
            End If
            'prtkurs(31) As Double
            If (IsNumeric(dataRowUtama(31)) = False) Then
                result(2) = "prtkurs required numeric." : GoTo selesai
            End If
            'prthargatermasukpajak(32) As Integer
            If (IsNumeric(dataRowUtama(32)) = False) Then
                result(2) = "prthargatermasukpajak required numeric." : GoTo selesai
            End If
            'prttotal(33) As Double
            If (IsNumeric(dataRowUtama(33)) = False) Then
                result(2) = "prttotal required numeric." : GoTo selesai
            End If
            'prtjmldiskon(35) As Double
            If (IsNumeric(dataRowUtama(35)) = False) Then
                result(2) = "prtjmldiskon required numeric." : GoTo selesai
            End If
            'prttotalpajak1detail(36) As Double
            If (IsNumeric(dataRowUtama(36)) = False) Then
                result(2) = "prttotalpajak1detail required numeric." : GoTo selesai
            End If
            'prttotalpajak2detail(37) As Double
            If (IsNumeric(dataRowUtama(37)) = False) Then
                result(2) = "prttotalpajak2detail required numeric." : GoTo selesai
            End If
            'prtbiayalain(39) As Double
            If (IsNumeric(dataRowUtama(39)) = False) Then
                result(2) = "prtbiayalain required numeric." : GoTo selesai
            End If
            'prttotaltransaksi(40) As Double
            If (IsNumeric(dataRowUtama(40)) = False) Then
                result(2) = "prttotaltransaksi required numeric." : GoTo selesai
            End If
            'prtsisatransaksi(41) As Double
            If (IsNumeric(dataRowUtama(41)) = False) Then
                result(2) = "prtsisatransaksi required numeric." : GoTo selesai
            End If
            'prtjmlbayar(42) As Double
            If (IsNumeric(dataRowUtama(42)) = False) Then
                result(2) = "prtjmlbayar required numeric." : GoTo selesai
            End If
            'prtstatuslunas(43) As Integer
            If (IsNumeric(dataRowUtama(43)) = False) Then
                result(2) = "prtstatuslunas required numeric." : GoTo selesai
            End If
            'prttgllunas(44) As Date
            If (IsDate(dataRowUtama(44)) = False) Then
                result(2) = "prttgllunas required date." : GoTo selesai
            End If
            'prtsdhbayarpajak(46) As Integer
            If (IsNumeric(dataRowUtama(46)) = False) Then
                result(2) = "prtsdhbayarpajak required numeric." : GoTo selesai
            End If
            'prttglbayarpajak(47) As Date
            If (IsDate(dataRowUtama(47)) = False) Then
                result(2) = "prttglbayarpajak required date." : GoTo selesai
            End If
            'prtidpr(54) As Integer
            If (IsNumeric(dataRowUtama(54)) = False) Then
                result(2) = "prtidpr required numeric." : GoTo selesai
            End If
            'prtidcs(55) As Integer
            If (IsNumeric(dataRowUtama(55)) = False) Then
                result(2) = "prtidcs required numeric." : GoTo selesai
            End If
            'prtidrq(56) As Integer
            If (IsNumeric(dataRowUtama(56)) = False) Then
                result(2) = "prtidrq required numeric." : GoTo selesai
            End If
            'prtidbs(57) As Integer
            If (IsNumeric(dataRowUtama(57)) = False) Then
                result(2) = "prtidbs required numeric." : GoTo selesai
            End If
            'prtidpo(58) As Integer
            If (IsNumeric(dataRowUtama(58)) = False) Then
                result(2) = "prtidpo required numeric." : GoTo selesai
            End If
            'prtidipc(59) As Integer
            If (IsNumeric(dataRowUtama(59)) = False) Then
                result(2) = "prtidipc required numeric." : GoTo selesai
            End If
            'prtidgrn(60) As Integer
            If (IsNumeric(dataRowUtama(60)) = False) Then
                result(2) = "prtidgrn required numeric." : GoTo selesai
            End If
            'prtidri(61) As Integer
            If (IsNumeric(dataRowUtama(61)) = False) Then
                result(2) = "prtidri required numeric." : GoTo selesai
            End If
            'prtiddnr(62) As Integer
            If (IsNumeric(dataRowUtama(62)) = False) Then
                result(2) = "prtiddnr required numeric." : GoTo selesai
            End If
            'prtstatus(63) As Integer
            If (IsNumeric(dataRowUtama(63)) = False) Then
                result(2) = "prtstatus required numeric." : GoTo selesai
            End If
            'prtstatussebelumnya(64) As Integer
            If (IsNumeric(dataRowUtama(64)) = False) Then
                result(2) = "prtstatussebelumnya required numeric." : GoTo selesai
            End If
            'prtjmlrevisi(65) As Integer
            If (IsNumeric(dataRowUtama(65)) = False) Then
                result(2) = "prtjmlrevisi required numeric." : GoTo selesai
            End If
            'prtcetakanke(66) As Integer
            If (IsNumeric(dataRowUtama(66)) = False) Then
                result(2) = "prtcetakanke required numeric." : GoTo selesai
            End If
            'prtinputuser(67) As Integer
            If (IsNumeric(dataRowUtama(67)) = False) Then
                result(2) = "prtinputuser required numeric." : GoTo selesai
            End If
            'prtinputtgl(68) As DateTime
            If (IsDate(dataRowUtama(68)) = False) Then
                result(2) = "prtinputtgl required date." : GoTo selesai
            End If
            'prtmodifikasiuser(69) As Integer
            If (IsNumeric(dataRowUtama(69)) = False) Then
                result(2) = "prtmodifikasiuser required numeric." : GoTo selesai
            End If
            'prtmodifikasitgl(70) As DateTime
            If (IsDate(dataRowUtama(70)) = False) Then
                result(2) = "prtmodifikasitgl required date." : GoTo selesai
            End If
            'prtposting(71) As Integer
            If (IsNumeric(dataRowUtama(71)) = False) Then
                result(2) = "prtposting required numeric." : GoTo selesai
            End If
            'prttutupperiode(72) As Integer
            If (IsNumeric(dataRowUtama(72)) = False) Then
                result(2) = "prttutupperiode required numeric." : GoTo selesai
            End If
            'prtisclose(73) As Integer
            If (IsNumeric(dataRowUtama(73)) = False) Then
                result(2) = "prtisclose required numeric." : GoTo selesai
            End If
            'prtcustomint1(79) As Integer
            If (IsNumeric(dataRowUtama(79)) = False) Then
                result(2) = "prtcustomint1 required numeric." : GoTo selesai
            End If
            'prtcustomint2(80) As Integer
            If (IsNumeric(dataRowUtama(80)) = False) Then
                result(2) = "prtcustomint2 required numeric." : GoTo selesai
            End If
            'prtcustomint3(81) As Integer
            If (IsNumeric(dataRowUtama(81)) = False) Then
                result(2) = "prtcustomint3 required numeric." : GoTo selesai
            End If
            'prtcustomdbl1(82) As Double
            If (IsNumeric(dataRowUtama(82)) = False) Then
                result(2) = "prtcustomdbl1 required numeric." : GoTo selesai
            End If
            'prtcustomdbl2(83) As Double
            If (IsNumeric(dataRowUtama(83)) = False) Then
                result(2) = "prtcustomdbl2 required numeric." : GoTo selesai
            End If
            'prtcustomdbl3(84) As Double
            If (IsNumeric(dataRowUtama(84)) = False) Then
                result(2) = "prtcustomdbl3 required numeric." : GoTo selesai
            End If
            'prtcustomdate1(85) As Date
            If (IsDate(dataRowUtama(85)) = False) Then
                result(2) = "prtcustomdate1 required date." : GoTo selesai
            End If
            'prtcustomdate2(86) As Date
            If (IsDate(dataRowUtama(86)) = False) Then
                result(2) = "prtcustomdate2 required date." : GoTo selesai
            End If
            'prtcustomdate3(87) As Date
            If (IsDate(dataRowUtama(87)) = False) Then
                result(2) = "prtcustomdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'prtcabang(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "prtcabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "prtcabang should not be more than 25 character." : GoTo selesai
            End If

            'prtlokasi(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "prtlokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 25 Then
                result(2) = "prtlokasi should not be more than 25 character." : GoTo selesai
            End If

            'prtgudang(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "prtgudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "prtgudang should not be more than 25 character." : GoTo selesai
            End If

            'prtsumber(9) As String
            If Len(dataRowUtama(9)) = 0 Then
                result(2) = "prtsumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(9)) > 10 Then
                result(2) = "prtsumber should not be more than 10 character." : GoTo selesai
            End If

            'prtnotransaksi(11) As String
            If Len(dataRowUtama(11)) = 0 Then
                result(2) = "prtnotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(11)) > 50 Then
                result(2) = "prtnotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'prttgl(12) As Date
            If Len(dataRowUtama(12)) = 0 Then
                result(2) = "prttgl can't be empty" : GoTo selesai
            End If

            'prttgljatuhtempo(24) As Date
            If Len(dataRowUtama(24)) = 0 Then
                result(2) = "prttgljatuhtempo can't be empty" : GoTo selesai
            End If

            'prttglnoref(28) As Date
            If Len(dataRowUtama(28)) = 0 Then
                result(2) = "prttglnoref can't be empty" : GoTo selesai
            End If

            'prttglpenutupan(29) As Date
            If Len(dataRowUtama(29)) = 0 Then
                result(2) = "prttglpenutupan can't be empty" : GoTo selesai
            End If

            'prtmatauang(30) As String
            If Len(dataRowUtama(30)) = 0 Then
                result(2) = "prtmatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(30)) > 25 Then
                result(2) = "prtmatauang should not be more than 25 character." : GoTo selesai
            End If

            'prtkurs(31) As Double
            If Len(dataRowUtama(31)) = 0 Then
                result(2) = "prtkurs can't be empty" : GoTo selesai
            End If

            'prttotal(33) As Double
            If Len(dataRowUtama(33)) = 0 Then
                result(2) = "prttotal can't be empty" : GoTo selesai
            End If

            'prtdiskonpersen(34) As String
            If Len(dataRowUtama(34)) = 0 Then
                result(2) = "prtdiskonpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(34)) > 25 Then
                result(2) = "prtdiskonpersen should not be more than 25 character." : GoTo selesai
            End If

            'prtjmldiskon(35) As Double
            If Len(dataRowUtama(35)) = 0 Then
                result(2) = "prtjmldiskon can't be empty" : GoTo selesai
            End If

            'prttotalpajak1detail(36) As Double
            If Len(dataRowUtama(36)) = 0 Then
                result(2) = "prttotalpajak1detail can't be empty" : GoTo selesai
            End If

            'prttotalpajak2detail(37) As Double
            If Len(dataRowUtama(37)) = 0 Then
                result(2) = "prttotalpajak2detail can't be empty" : GoTo selesai
            End If

            'prtbiayalainpersen(38) As String
            If Len(dataRowUtama(38)) = 0 Then
                result(2) = "prtbiayalainpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(38)) > 25 Then
                result(2) = "prtbiayalainpersen should not be more than 25 character." : GoTo selesai
            End If

            'prtbiayalain(39) As Double
            If Len(dataRowUtama(39)) = 0 Then
                result(2) = "prtbiayalain can't be empty" : GoTo selesai
            End If

            'prttotaltransaksi(40) As Double
            If Len(dataRowUtama(40)) = 0 Then
                result(2) = "prttotaltransaksi can't be empty" : GoTo selesai
            End If

            'prtsisatransaksi(41) As Double
            If Len(dataRowUtama(41)) = 0 Then
                result(2) = "prtsisatransaksi can't be empty" : GoTo selesai
            End If

            'prtjmlbayar(42) As Double
            If Len(dataRowUtama(42)) = 0 Then
                result(2) = "prtjmlbayar can't be empty" : GoTo selesai
            End If

            'prttgllunas(44) As Date
            If Len(dataRowUtama(44)) = 0 Then
                result(2) = "prttgllunas can't be empty" : GoTo selesai
            End If

            'prttglbayarpajak(47) As Date
            If Len(dataRowUtama(47)) = 0 Then
                result(2) = "prttglbayarpajak can't be empty" : GoTo selesai
            End If

            'prtinputtgl(68) As DateTime
            If Len(dataRowUtama(68)) = 0 Then
                result(2) = "prtinputtgl can't be empty" : GoTo selesai
            End If

            'prtmodifikasitgl(70) As DateTime
            If Len(dataRowUtama(70)) = 0 Then
                result(2) = "prtmodifikasitgl can't be empty" : GoTo selesai
            End If

            'prtcustomdbl1(82) As Double
            If Len(dataRowUtama(82)) = 0 Then
                result(2) = "prtcustomdbl1 can't be empty" : GoTo selesai
            End If

            'prtcustomdbl2(83) As Double
            If Len(dataRowUtama(83)) = 0 Then
                result(2) = "prtcustomdbl2 can't be empty" : GoTo selesai
            End If

            'prtcustomdbl3(84) As Double
            If Len(dataRowUtama(84)) = 0 Then
                result(2) = "prtcustomdbl3 can't be empty" : GoTo selesai
            End If

            'prtcustomdate1(85) As Date
            If Len(dataRowUtama(85)) = 0 Then
                result(2) = "prtcustomdate1 can't be empty" : GoTo selesai
            End If

            'prtcustomdate2(86) As Date
            If Len(dataRowUtama(86)) = 0 Then
                result(2) = "prtcustomdate2 can't be empty" : GoTo selesai
            End If

            'prtcustomdate3(87) As Date
            If Len(dataRowUtama(87)) = 0 Then
                result(2) = "prtcustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA UTAMA ================================================

            If AsDataTableTambahData(dtutama, "prtid~prtcabang~prtlokasi~prtgudang~prtasalbarang~prtasalbarangkategori~prtjenispembelian~prtjenispembeliankategori~prtcarabayar~prtsumber~prtautonotransaksi~prtnotransaksi~prttgl~prtkodepa~prtsupplier~prtsupplierkontak~prt1alamat1~prt1alamat2~prt1alamat3~prt2alamat1~prt2alamat2~prt2alamat3~prtbagianpembelian~prttermin~prttgljatuhtempo~prturaian~prtcatatan~prtnoref~prttglnoref~prttglpenutupan~prtmatauang~prtkurs~prthargatermasukpajak~prttotal~prtdiskonpersen~prtjmldiskon~prttotalpajak1detail~prttotalpajak2detail~prtbiayalainpersen~prtbiayalain~prttotaltransaksi~prtsisatransaksi~prtjmlbayar~prtstatuslunas~prttgllunas~prtnofakturpajak~prtsdhbayarpajak~prttglbayarpajak~prtrekdiskon~prtrekpajak1~prtrekpajak2~prtrekbiayalain~prtrekbayar~prtreksisa~prtidpr~prtidcs~prtidrq~prtidbs~prtidpo~prtidipc~prtidgrn~prtidri~prtiddnr~prtstatus~prtstatussebelumnya~prtjmlrevisi~prtcetakanke~prtinputuser~prtinputtgl~prtmodifikasiuser~prtmodifikasitgl~prtposting~prttutupperiode~prtisclose~prtcustomtext1~prtcustomtext2~prtcustomtext3~prtcustomtext4~prtcustomtext5~prtcustomint1~prtcustomint2~prtcustomint3~prtcustomdbl1~prtcustomdbl2~prtcustomdbl3~prtcustomdate1~prtcustomdate2~prtcustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85) & "~" & dataRowUtama(86) & "~" & dataRowUtama(87)) = False Then
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
                    Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("prttgl")), AsFormatTanggal(drutama("prttgl")))
                    arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                    If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                    'END OF CEK PERIODE AKUNTANSI ===========================


                    ''SET TGL JATUH TEMPO ====================================
                    'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                    'rsTglJT = F_TglJT(drutama("prttermin").ToString, AsFormatTanggal(drutama("prttgl")), "prttgl").Split(sptSubParam)
                    'If rsTglJT(0) = 0 Then
                    '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                    'Else
                    '    drutama("prttgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                    'End If
                    ''END OF SET TGL JATUH TEMPO =============================


                    If isUpdate Then
                        result(4) = drutama("prtid")
                        notransaksi = drutama("prtnotransaksi")
                        'JIKA UPDATE CEK JML ROW PADA DATABASE
                        dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid), prtnotransaksi FROM M4_prt WHERE prtid='" & result(4) & "' AND prtstatus NOT IN(2,3,4,7)", myConn)
                        rowUpdate = dtupdate.Rows(0)(0)

                        If (rowUpdate > 0) Then

                            'CEK NO TRANSAKSI ======================
                            If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                                Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid) FROM M4_prt WHERE prtnotransaksi='" & notransaksi & "'", myConn)
                                Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                                If cekNo > 0 Then
                                    result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                            'END OF CEK NO TRANSAKSI ===============

                            'SIMPAN HISTORY ========================
                            Dim SimpanHistory As New m4_prt_history
                            Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("prtsumber")) & "▼" & FixQuotes(drutama("prtid")) & "")
                            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                            If (rsSplitResult(1) = 0) Then
                                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF SIMPAN HISTORY ==================

                            sql = "Update M4_Prt set prtcabang  = '" & FixQuotes(drutama("prtcabang")) & "', prtlokasi  = '" & FixQuotes(drutama("prtlokasi")) & "', prtgudang  = '" & FixQuotes(drutama("prtgudang")) & "', prtasalbarang  = '" & FixQuotes(drutama("prtasalbarang")) & "', prtasalbarangkategori  = " & drutama("prtasalbarangkategori") & ", prtjenispembelian  = '" & FixQuotes(drutama("prtjenispembelian")) & "', prtjenispembeliankategori  = " & drutama("prtjenispembeliankategori") & ", prtcarabayar  = " & drutama("prtcarabayar") & ", prtsumber  = '" & FixQuotes(drutama("prtsumber")) & "', prtautonotransaksi  = " & drutama("prtautonotransaksi") & ", prtnotransaksi  = '" & FixQuotes(notransaksi) & "', prttgl  = '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', prtkodepa  = " & drutama("prtkodepa") & ", prtsupplier  = " & drutama("prtsupplier") & ", prtsupplierkontak  = '" & FixQuotes(drutama("prtsupplierkontak")) & "', prt1alamat1  = '" & FixQuotes(drutama("prt1alamat1")) & "', prt1alamat2  = '" & FixQuotes(drutama("prt1alamat2")) & "', prt1alamat3  = '" & FixQuotes(drutama("prt1alamat3")) & "', prt2alamat1  = '" & FixQuotes(drutama("prt2alamat1")) & "', prt2alamat2  = '" & FixQuotes(drutama("prt2alamat2")) & "', prt2alamat3  = '" & FixQuotes(drutama("prt2alamat3")) & "', prtbagianpembelian  = " & drutama("prtbagianpembelian") & ", prttermin  = '" & FixQuotes(drutama("prttermin")) & "', prttgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', prturaian  = '" & FixQuotes(drutama("prturaian")) & "', prtcatatan  = '" & FixQuotes(drutama("prtcatatan")) & "', prtnoref  = '" & FixQuotes(drutama("prtnoref")) & "', prttglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', prttglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', prtmatauang  = '" & FixQuotes(drutama("prtmatauang")) & "', prtkurs  = '" & FixDouble(drutama("prtkurs")) & "', prthargatermasukpajak  = " & drutama("prthargatermasukpajak") & ", prttotal  = '" & FixDouble(drutama("prttotal")) & "', prtdiskonpersen  = '" & FixQuotes(drutama("prtdiskonpersen")) & "', prtjmldiskon  = '" & FixDouble(drutama("prtjmldiskon")) & "', prttotalpajak1detail  = '" & FixDouble(drutama("prttotalpajak1detail")) & "', prttotalpajak2detail  = '" & FixDouble(drutama("prttotalpajak2detail")) & "', prtbiayalainpersen  = '" & FixQuotes(drutama("prtbiayalainpersen")) & "', prtbiayalain  = '" & FixDouble(drutama("prtbiayalain")) & "', prttotaltransaksi  = '" & FixDouble(drutama("prttotaltransaksi")) & "', prtsisatransaksi  = '" & FixDouble(drutama("prtsisatransaksi")) & "', prtjmlbayar  = '" & FixDouble(drutama("prtjmlbayar")) & "', prtstatuslunas  = " & drutama("prtstatuslunas") & ", prttgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', prtnofakturpajak  = '" & FixQuotes(drutama("prtnofakturpajak")) & "', prtsdhbayarpajak  = " & drutama("prtsdhbayarpajak") & ", prttglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', prtrekdiskon  = '" & FixQuotes(drutama("prtrekdiskon")) & "', prtrekpajak1  = '" & FixQuotes(drutama("prtrekpajak1")) & "', prtrekpajak2  = '" & FixQuotes(drutama("prtrekpajak2")) & "', prtrekbiayalain  = '" & FixQuotes(drutama("prtrekbiayalain")) & "', prtrekbayar  = '" & FixQuotes(drutama("prtrekbayar")) & "', prtreksisa  = '" & FixQuotes(drutama("prtreksisa")) & "', prtidpr  = " & drutama("prtidpr") & ", prtidcs  = " & drutama("prtidcs") & ", prtidrq  = " & drutama("prtidrq") & ", prtidbs  = " & drutama("prtidbs") & ", prtidpo  = " & drutama("prtidpo") & ", prtidipc  = " & drutama("prtidipc") & ", prtidgrn  = " & drutama("prtidgrn") & ", prtidri  = " & drutama("prtidri") & ", prtiddnr  = " & drutama("prtiddnr") & ", prtstatus  = " & drutama("prtstatus") & ", prtstatussebelumnya  = " & drutama("prtstatussebelumnya") & ", prtjmlrevisi  = prtjmlrevisi+1, prtcetakanke  = " & drutama("prtcetakanke") & ", prtmodifikasiuser  = " & drutama("prtmodifikasiuser") & ", prtmodifikasitgl  = NOW(), prtposting  = 0, prttutupperiode  = " & drutama("prttutupperiode") & ", prtcustomtext1  = '" & FixQuotes(drutama("prtcustomtext1")) & "', prtcustomtext2  = '" & FixQuotes(drutama("prtcustomtext2")) & "', prtcustomtext3  = '" & FixQuotes(drutama("prtcustomtext3")) & "', prtcustomtext4  = '" & FixQuotes(drutama("prtcustomtext4")) & "', prtcustomtext5  = '" & FixQuotes(drutama("prtcustomtext5")) & "', prtcustomint1  = " & drutama("prtcustomint1") & ", prtcustomint2  = " & drutama("prtcustomint2") & ", prtcustomint3  = " & drutama("prtcustomint3") & ", prtcustomdbl1  = '" & FixDouble(drutama("prtcustomdbl1")) & "', prtcustomdbl2  = '" & FixDouble(drutama("prtcustomdbl2")) & "', prtcustomdbl3  = '" & FixDouble(drutama("prtcustomdbl3")) & "', prtcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', prtcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', prtcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', prtsaldoawal = 1 where prtid = " & drutama("prtid") & ""
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

                        If drutama("prtautonotransaksi") = 1 Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("prtcabang"), drutama("prtlokasi"), drutama("prtsumber"), drutama("prttgl"), drutama("prtsumber"), 4)
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
                            notransaksi = drutama("prtnotransaksi")
                        End If

                        'CEK NO TRANSAKSI ======================
                        Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(prtid) FROM m4_prt WHERE prtnotransaksi='" & notransaksi & "'", myConn)
                        Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                        If cekNo > 0 Then
                            result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Insert into M4_Prt (prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prttutupperiode, prtisclose, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtsaldoawal) values('" & FixQuotes(drutama("prtcabang")) & "', '" & FixQuotes(drutama("prtlokasi")) & "', '" & FixQuotes(drutama("prtgudang")) & "', '" & FixQuotes(drutama("prtasalbarang")) & "', " & drutama("prtasalbarangkategori") & ", '" & FixQuotes(drutama("prtjenispembelian")) & "', " & drutama("prtjenispembeliankategori") & ", " & drutama("prtcarabayar") & ", '" & FixQuotes(drutama("prtsumber")) & "', " & drutama("prtautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgl"))) & "', " & drutama("prtkodepa") & ", " & drutama("prtsupplier") & ", '" & FixQuotes(drutama("prtsupplierkontak")) & "', '" & FixQuotes(drutama("prt1alamat1")) & "', '" & FixQuotes(drutama("prt1alamat2")) & "', '" & FixQuotes(drutama("prt1alamat3")) & "', '" & FixQuotes(drutama("prt2alamat1")) & "', '" & FixQuotes(drutama("prt2alamat2")) & "', '" & FixQuotes(drutama("prt2alamat3")) & "', " & drutama("prtbagianpembelian") & ", '" & FixQuotes(drutama("prttermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttgljatuhtempo"))) & "', '" & FixQuotes(drutama("prturaian")) & "', '" & FixQuotes(drutama("prtcatatan")) & "', '" & FixQuotes(drutama("prtnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prttglpenutupan"))) & "', '" & FixQuotes(drutama("prtmatauang")) & "', '" & FixDouble(drutama("prtkurs")) & "', " & drutama("prthargatermasukpajak") & ", '" & FixDouble(drutama("prttotal")) & "', '" & FixQuotes(drutama("prtdiskonpersen")) & "', '" & FixDouble(drutama("prtjmldiskon")) & "', '" & FixDouble(drutama("prttotalpajak1detail")) & "', '" & FixDouble(drutama("prttotalpajak2detail")) & "', '" & FixQuotes(drutama("prtbiayalainpersen")) & "', '" & FixDouble(drutama("prtbiayalain")) & "', '" & FixDouble(drutama("prttotaltransaksi")) & "', '" & FixDouble(drutama("prtsisatransaksi")) & "', '" & FixDouble(drutama("prtjmlbayar")) & "', " & drutama("prtstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttgllunas"))) & "', '" & FixQuotes(drutama("prtnofakturpajak")) & "', " & drutama("prtsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("prttglbayarpajak"))) & "', '" & FixQuotes(drutama("prtrekdiskon")) & "', '" & FixQuotes(drutama("prtrekpajak1")) & "', '" & FixQuotes(drutama("prtrekpajak2")) & "', '" & FixQuotes(drutama("prtrekbiayalain")) & "', '" & FixQuotes(drutama("prtrekbayar")) & "', '" & FixQuotes(drutama("prtreksisa")) & "', " & drutama("prtidpr") & ", " & drutama("prtidcs") & ", " & drutama("prtidrq") & ", " & drutama("prtidbs") & ", " & drutama("prtidpo") & ", " & drutama("prtidipc") & ", " & drutama("prtidgrn") & ", " & drutama("prtidri") & ", " & drutama("prtiddnr") & ", " & drutama("prtstatus") & ", " & drutama("prtstatussebelumnya") & ", " & drutama("prtjmlrevisi") & ", " & drutama("prtcetakanke") & ", " & drutama("prtinputuser") & ", NOW(), " & drutama("prtmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("prttutupperiode") & ", " & drutama("prtisclose") & ", '" & FixQuotes(drutama("prtcustomtext1")) & "', '" & FixQuotes(drutama("prtcustomtext2")) & "', '" & FixQuotes(drutama("prtcustomtext3")) & "', '" & FixQuotes(drutama("prtcustomtext4")) & "', '" & FixQuotes(drutama("prtcustomtext5")) & "', " & drutama("prtcustomint1") & ", " & drutama("prtcustomint2") & ", " & drutama("prtcustomint3") & ", '" & FixDouble(drutama("prtcustomdbl1")) & "', '" & FixDouble(drutama("prtcustomdbl2")) & "', '" & FixDouble(drutama("prtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("prtcustomdate3"))) & "', 1)"
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
                        dt2 = AsDataTableAmbilDariDBCon("select prtid from M4_prt where prtnotransaksi='" & notransaksi & "' AND prtinputuser= '" & userid & "' order by prtmodifikasitgl desc limit 1", myConn)
                        If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If

                    'INSERT MSMQ JURNAL =================================================================
                    Dim sumber As String = "PRT", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                    If drutama("prtstatus") = 2 Then
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
    Public Function M4_PrtBUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "Prt", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Prttgl, Prtnotransaksi, Prtstatus FROM M4_Prt WHERE Prtid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Prtstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_prt_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Prt_HistorySimpan("" & paramSplit(0) & "★M4_Prt_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_prt_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idridetail As Integer = 0, iddnrdetail As Integer = 0, idhppkhususmasuk As Integer = 0
                Dim updNilaiRI As String = "", updFilterRI As String = "", updNilaiDNR As String = "", updFilterDNR As String = ""
                Dim gudangIn As String = "", updStokIn As String = "", updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""
                Dim filterHppF As String = "", updNilaiHppF As String = "", updFilterHppF As String = "", delFilterHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""




                'DELETE JURNAL
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = 'PRT' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M4_Prt SET Prtstatus = " & nilaiStatus & ", Prtmodifikasiuser='" & userid & "', Prtmodifikasitgl = NOW(), Prtposting = 0, Prtpostingtgl = '1971-01-01 00:00:00', Prtjmlrevisi = Prtjmlrevisi + 1 WHERE Prtid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtBDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
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
            Dim sumber As String = "PRT", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Prtid, Prtnotransaksi FROM M4_Prt WHERE Prtid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT prtcabang, prtlokasi, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl"
            sql &= " FROM M4_prt"
            sql &= " WHERE prtid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("prtcabang")
                lokasi = dtNomorNext.Rows(0)("prtlokasi")
                sumber = dtNomorNext.Rows(0)("prtsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("prtautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("prtnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("prttgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================



            'DELETE UTAMA
            sql = "DELETE FROM M4_Prt WHERE prtid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PrtSearch(PostWsSearch(paramSplit(0), "M4_PrtSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PrtBSearch(ByVal param As String) As String
        'M4_PrtBSearch --------------------------------------------------------
        'prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, 
        'prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, 
        'prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, 
        'prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, 
        'prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, 
        'prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, 
        'prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, 
        'prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, 
        'prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, 
        'prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, 
        'prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, 
        'prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, 
        'prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama

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
            Filter = Filter.Replace("prtsupplierkode", "c1.kkode")
            Filter = Filter.Replace("prtsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `prt`.`prtid` AS `prtid`,`prt`.`prtcabang` AS `prtcabang`,`prt`.`prtlokasi` AS `prtlokasi`,`prt`.`prtgudang` AS `prtgudang`,`prt`.`prtasalbarang` AS `prtasalbarang`,`prt`.`prtasalbarangkategori` AS `prtasalbarangkategori`,`prt`.`prtjenispembelian` AS `prtjenispembelian`,`prt`.`prtjenispembeliankategori` AS `prtjenispembeliankategori`,`prt`.`prtcarabayar` AS `prtcarabayar`,`prt`.`prtsumber` AS `prtsumber`,`prt`.`prtautonotransaksi` AS `prtautonotransaksi`,`prt`.`prtnotransaksi` AS `prtnotransaksi`,`prt`.`prttgl` AS `prttgl`,`prt`.`prtkodepa` AS `prtkodepa`,`prt`.`prtsupplier` AS `prtsupplier`,`prt`.`prtsupplierkontak` AS `prtsupplierkontak`,`prt`.`prt1alamat1` AS `prt1alamat1`,`prt`.`prt1alamat2` AS `prt1alamat2`,`prt`.`prt1alamat3` AS `prt1alamat3`,`prt`.`prt2alamat1` AS `prt2alamat1`,`prt`.`prt2alamat2` AS `prt2alamat2`,`prt`.`prt2alamat3` AS `prt2alamat3`,`prt`.`prtbagianpembelian` AS `prtbagianpembelian`,`prt`.`prttermin` AS `prttermin`,`prt`.`prttgljatuhtempo` AS `prttgljatuhtempo`,`prt`.`prturaian` AS `prturaian`,`prt`.`prtcatatan` AS `prtcatatan`,`prt`.`prtnoref` AS `prtnoref`,`prt`.`prttglnoref` AS `prttglnoref`,`prt`.`prttglpenutupan` AS `prttglpenutupan`,`prt`.`prtmatauang` AS `prtmatauang`,`prt`.`prtkurs` AS `prtkurs`,`prt`.`prthargatermasukpajak` AS `prthargatermasukpajak`,`prt`.`prttotal` AS `prttotal`,`prt`.`prtdiskonpersen` AS `prtdiskonpersen`,`prt`.`prtjmldiskon` AS `prtjmldiskon`,`prt`.`prttotalpajak1detail` AS `prttotalpajak1detail`,`prt`.`prttotalpajak2detail` AS `prttotalpajak2detail`,`prt`.`prtbiayalainpersen` AS `prtbiayalainpersen`,`prt`.`prtbiayalain` AS `prtbiayalain`,`prt`.`prttotaltransaksi` AS `prttotaltransaksi`,`prt`.`prtsisatransaksi` AS `prtsisatransaksi`,`prt`.`prtjmlbayar` AS `prtjmlbayar`,`prt`.`prtstatuslunas` AS `prtstatuslunas`,`prt`.`prttgllunas` AS `prttgllunas`,`prt`.`prtnofakturpajak` AS `prtnofakturpajak`,`prt`.`prtsdhbayarpajak` AS `prtsdhbayarpajak`,`prt`.`prttglbayarpajak` AS `prttglbayarpajak`,`prt`.`prtrekdiskon` AS `prtrekdiskon`,`prt`.`prtrekpajak1` AS `prtrekpajak1`,`prt`.`prtrekpajak2` AS `prtrekpajak2`,`prt`.`prtrekbiayalain` AS `prtrekbiayalain`,`prt`.`prtrekbayar` AS `prtrekbayar`,`prt`.`prtreksisa` AS `prtreksisa`,`prt`.`prtidpr` AS `prtidpr`,`prt`.`prtidcs` AS `prtidcs`,`prt`.`prtidrq` AS `prtidrq`,`prt`.`prtidbs` AS `prtidbs`,`prt`.`prtidpo` AS `prtidpo`,`prt`.`prtidipc` AS `prtidipc`,`prt`.`prtidgrn` AS `prtidgrn`,`prt`.`prtidri` AS `prtidri`,`prt`.`prtiddnr` AS `prtiddnr`,`prt`.`prtstatus` AS `prtstatus`,`prt`.`prtstatussebelumnya` AS `prtstatussebelumnya`,`prt`.`prtjmlrevisi` AS `prtjmlrevisi`,`prt`.`prtcetakanke` AS `prtcetakanke`,`prt`.`prtinputuser` AS `prtinputuser`,`prt`.`prtinputtgl` AS `prtinputtgl`,`prt`.`prtmodifikasiuser` AS `prtmodifikasiuser`,`prt`.`prtmodifikasitgl` AS `prtmodifikasitgl`,`prt`.`prtposting` AS `prtposting`,`prt`.`prtpostingtgl` AS `prtpostingtgl`,`prt`.`prttutupperiode` AS `prttutupperiode`,`prt`.`prtisclose` AS `prtisclose`,`br`.`bnama` AS `prtcabangnama`,`lc`.`lnama` AS `prtlokasinama`,`wh`.`wnama` AS `prtgudangnama`,`c1`.`kkode` AS `prtsupplierkode`,`c1`.`knama` AS `prtsuppliernama`,`c2`.`kkode` AS `prtbagianpembeliankode`,`c2`.`knama` AS `prtbagianpembeliannama`,`ri`.`rinotransaksi` AS `rinotransaksi`,`dnr`.`dnrnotransaksi` AS `dnrnotransaksi`,`st1`.`nama` AS `prtstatusnama`,`st2`.`nama` AS `prtstatussebelumnyanama`,`u1`.`unama` AS `prtinputusernama`,`u2`.`unama` AS `prtmodifikasiusernama`, `prt`.`prtcustomtext1` AS `prtcustomtext1`, `prt`.`prtcustomtext2` AS `prtcustomtext2`, `prt`.`prtcustomtext3` AS `prtcustomtext3`, `prt`.`prtcustomtext4` AS `prtcustomtext4`, `prt`.`prtcustomtext5` AS `prtcustomtext5`, `prt`.`prtcustomint1` AS `prtcustomint1`, `prt`.`prtcustomint2` AS `prtcustomint2`, `prt`.`prtcustomint3` AS `prtcustomint3`, `prt`.`prtcustomdbl1` AS `prtcustomdbl1`, `prt`.`prtcustomdbl2` AS `prtcustomdbl2`, `prt`.`prtcustomdbl3` AS `prtcustomdbl3`, `prt`.`prtcustomdate1` AS `prtcustomdate1`, `prt`.`prtcustomdate2` AS `prtcustomdate2`, `prt`.`prtcustomdate3` AS `prtcustomdate3`, cdis.cnama AS prtrekdiskonnama, cpa.cnama AS prtrekpajak1nama, cpa2.cnama AS prtrekpajak2nama, cba.cnama AS prtrekbiayalainnama from (((((((((((`m4_prt` `prt` left join `m1_branch` `br` on((`br`.`bkode` = `prt`.`prtcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `prt`.`prtlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `prt`.`prtgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `prt`.`prtsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `prt`.`prtbagianpembelian`))) left join `m4_ri` `ri` on((`prt`.`prtidri` = `ri`.`riid`))) left join `m4_dnr` `dnr` on((`prt`.`prtid` = `dnr`.`dnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `prt`.`prtstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `prt`.`prtstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `prt`.`prtinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `prt`.`prtmodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = prt.prtrekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = prt.prtrekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = prt.prtrekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = prt.prtrekbiayalain"
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Prt", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("prtid"), 0), sptField,
                     FxDB(dr("prtcabang"), ""), sptField,
                     FxDB(dr("prtlokasi"), ""), sptField,
                     FxDB(dr("prtgudang"), ""), sptField,
                     FxDB(dr("prtasalbarang"), ""), sptField,
                     FxDB(dr("prtasalbarangkategori"), 0), sptField,
                     FxDB(dr("prtjenispembelian"), ""), sptField,
                     FxDB(dr("prtjenispembeliankategori"), 0), sptField,
                     FxDB(dr("prtcarabayar"), 0), sptField,
                     FxDB(dr("prtsumber"), ""), sptField,
                     FxDB(dr("prtautonotransaksi"), 0), sptField,
                     FxDB(dr("prtnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgl"), ""), formatTgl), sptField,
                     FxDB(dr("prtkodepa"), 0), sptField,
                     FxDB(dr("prtsupplier"), 0), sptField,
                     FxDB(dr("prtsupplierkontak"), ""), sptField,
                     FxDB(dr("prt1alamat1"), ""), sptField,
                     FxDB(dr("prt1alamat2"), ""), sptField,
                     FxDB(dr("prt1alamat3"), ""), sptField,
                     FxDB(dr("prt2alamat1"), ""), sptField,
                     FxDB(dr("prt2alamat2"), ""), sptField,
                     FxDB(dr("prt2alamat3"), ""), sptField,
                     FxDB(dr("prtbagianpembelian"), 0), sptField,
                     FxDB(dr("prttermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("prturaian"), ""), sptField,
                     FxDB(dr("prtcatatan"), ""), sptField,
                     FxDB(dr("prtnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("prttglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prttglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("prtmatauang"), ""), sptField,
                     FxDB(dr("prtkurs"), 0), sptField,
                     FxDB(dr("prthargatermasukpajak"), 0), sptField,
                     FxDB(dr("prttotal"), 0), sptField,
                     FxDB(dr("prtdiskonpersen"), ""), sptField,
                     FxDB(dr("prtjmldiskon"), 0), sptField,
                     FxDB(dr("prttotalpajak1detail"), 0), sptField,
                     FxDB(dr("prttotalpajak2detail"), 0), sptField,
                     FxDB(dr("prtbiayalainpersen"), ""), sptField,
                     FxDB(dr("prtbiayalain"), 0), sptField,
                     FxDB(dr("prttotaltransaksi"), 0), sptField,
                     FxDB(dr("prtsisatransaksi"), 0), sptField,
                     FxDB(dr("prtjmlbayar"), 0), sptField,
                     FxDB(dr("prtstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("prtnofakturpajak"), ""), sptField,
                     FxDB(dr("prtsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prttglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskon"), ""), sptField,
                     FxDB(dr("prtrekpajak1"), ""), sptField,
                     FxDB(dr("prtrekpajak2"), ""), sptField,
                     FxDB(dr("prtrekbiayalain"), ""), sptField,
                     FxDB(dr("prtrekbayar"), ""), sptField,
                     FxDB(dr("prtreksisa"), ""), sptField,
                     FxDB(dr("prtidpr"), 0), sptField,
                     FxDB(dr("prtidcs"), 0), sptField,
                     FxDB(dr("prtidrq"), 0), sptField,
                     FxDB(dr("prtidbs"), 0), sptField,
                     FxDB(dr("prtidpo"), 0), sptField,
                     FxDB(dr("prtidipc"), 0), sptField,
                     FxDB(dr("prtidgrn"), 0), sptField,
                     FxDB(dr("prtidri"), 0), sptField,
                     FxDB(dr("prtiddnr"), 0), sptField,
                     FxDB(dr("prtstatus"), 0), sptField,
                     FxDB(dr("prtstatussebelumnya"), 0), sptField,
                     FxDB(dr("prtjmlrevisi"), 0), sptField,
                     FxDB(dr("prtcetakanke"), 0), sptField,
                     FxDB(dr("prtinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prtposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("prttutupperiode"), 0), sptField,
                     FxDB(dr("prtisclose"), 0), sptField,
                     FxDB(dr("prtcabangnama"), ""), sptField,
                     FxDB(dr("prtlokasinama"), ""), sptField,
                     FxDB(dr("prtgudangnama"), ""), sptField,
                     FxDB(dr("prtsupplierkode"), ""), sptField,
                     FxDB(dr("prtsuppliernama"), ""), sptField,
                     FxDB(dr("prtbagianpembeliankode"), ""), sptField,
                     FxDB(dr("prtbagianpembeliannama"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("prtstatusnama"), ""), sptField,
                     FxDB(dr("prtstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("prtinputusernama"), ""), sptField,
                     FxDB(dr("prtmodifikasiusernama"), ""), sptField,
                     FxDB(dr("prtcustomtext1"), ""), sptField,
                     FxDB(dr("prtcustomtext2"), ""), sptField,
                     FxDB(dr("prtcustomtext3"), ""), sptField,
                     FxDB(dr("prtcustomtext4"), ""), sptField,
                     FxDB(dr("prtcustomtext5"), ""), sptField,
                     FxDB(dr("prtcustomint1"), 0), sptField,
                     FxDB(dr("prtcustomint2"), 0), sptField,
                     FxDB(dr("prtcustomint3"), 0), sptField,
                     FxDB(dr("prtcustomdbl1"), 0), sptField,
                     FxDB(dr("prtcustomdbl2"), 0), sptField,
                     FxDB(dr("prtcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("prtcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("prtrekdiskonnama"), ""), sptField,
                     FxDB(dr("prtrekpajak1nama"), ""), sptField,
                     FxDB(dr("prtrekpajak2nama"), ""), sptField,
                     FxDB(dr("prtrekbiayalainnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("prtid, prtcabang, prtlokasi, prtgudang, prtasalbarang, prtasalbarangkategori, prtjenispembelian, prtjenispembeliankategori, prtcarabayar, prtsumber, prtautonotransaksi, prtnotransaksi, prttgl, prtkodepa, prtsupplier, prtsupplierkontak, prt1alamat1, prt1alamat2, prt1alamat3, prt2alamat1, prt2alamat2, prt2alamat3, prtbagianpembelian, prttermin, prttgljatuhtempo, prturaian, prtcatatan, prtnoref, prttglnoref, prttglpenutupan, prtmatauang, prtkurs, prthargatermasukpajak, prttotal, prtdiskonpersen, prtjmldiskon, prttotalpajak1detail, prttotalpajak2detail, prtbiayalainpersen, prtbiayalain, prttotaltransaksi, prtsisatransaksi, prtjmlbayar, prtstatuslunas, prttgllunas, prtnofakturpajak, prtsdhbayarpajak, prttglbayarpajak, prtrekdiskon, prtrekpajak1, prtrekpajak2, prtrekbiayalain, prtrekbayar, prtreksisa, prtidpr, prtidcs, prtidrq, prtidbs, prtidpo, prtidipc, prtidgrn, prtidri, prtiddnr, prtstatus, prtstatussebelumnya, prtjmlrevisi, prtcetakanke, prtinputuser, prtinputtgl, prtmodifikasiuser, prtmodifikasitgl, prtposting, prtpostingtgl, prttutupperiode, prtisclose, prtcabangnama, prtlokasinama, prtgudangnama, prtsupplierkode, prtsuppliernama, prtbagianpembeliankode, prtbagianpembeliannama, rinotransaksi, dnrnotransaksi, prtstatusnama, prtstatussebelumnyanama, prtinputusernama, prtmodifikasiusernama, prtcustomtext1, prtcustomtext2, prtcustomtext3, prtcustomtext4, prtcustomtext5, prtcustomint1, prtcustomint2, prtcustomint3, prtcustomdbl1, prtcustomdbl2, prtcustomdbl3, prtcustomdate1, prtcustomdate2, prtcustomdate3, prtrekdiskonnama, prtrekpajak1nama, prtrekpajak2nama, prtrekbiayalainnama"))

        Return wsResult
    End Function

End Class