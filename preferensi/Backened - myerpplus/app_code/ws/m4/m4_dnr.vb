Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_dnr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_DnrSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataAsset(), dataRowAsset() As String
        Dim dataSerial(), dataRowSerial() As String

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
        'dnrid(0) As Integer, dnrcabang(1) As String, dnrlokasi(2) As String, dnrgudang(3) As String, dnrasalbarang(4) As String, 
        'dnrasalbarangkategori(5) As Integer, dnrjenispembelian(6) As String, dnrjenispembeliankategori(7) As Integer, dnrcarabayar(8) As Integer, dnrsumber(9) As String, 
        'dnrautonotransaksi(10) As Integer, dnrnotransaksi(11) As String, dnrtgl(12) As Date, dnrkodepa(13) As Integer, dnrsupplier(14) As Integer, 
        'dnrsupplierkontak(15) As String, dnr1alamat1(16) As String, dnr1alamat2(17) As String, dnr1alamat3(18) As String, dnr2alamat1(19) As String, 
        'dnr2alamat2(20) As String, dnr2alamat3(21) As String, dnrbagianpembelian(22) As Integer, dnrtermin(23) As String, dnrtgljatuhtempo(24) As Date, 
        'dnruraian(25) As String, dnrcatatan(26) As String, dnrnoref(27) As String, dnrtglnoref(28) As Date, dnrtglpenutupan(29) As Date, 
        'dnrmatauang(30) As String, dnrkurs(31) As Double, dnrhargatermasukpajak(32) As Integer, dnrtotal(33) As Double, dnrdiskonpersen(34) As String, 
        'dnrjmldiskon(35) As Double, dnrtotalpajak1detail(36) As Double, dnrtotalpajak2detail(37) As Double, dnrbiayalainpersen(38) As String, dnrbiayalain(39) As Double, 
        'dnrtotaltransaksi(40) As Double, dnrjmlbayar(41) As Double, dnrstatuslunas(42) As Integer, dnrtgllunas(43) As Date, dnrnofakturpajak(44) As String, 
        'dnrsdhbayarpajak(45) As Integer, dnrtglbayarpajak(46) As Date, dnrrekdiskon(47) As String, dnrrekpajak1(48) As String, dnrrekpajak2(49) As String, 
        'dnrrekbiayalain(50) As String, dnrrekbayar(51) As String, dnridpr(52) As Integer, dnridcs(53) As Integer, dnridrq(54) As Integer, 
        'dnridbs(55) As Integer, dnridpo(56) As Integer, dnridipc(57) As Integer, dnridgrn(58) As Integer, dnridri(59) As Integer, 
        'dnrstatusprt(60) As Integer, dnrstatus(61) As Integer, dnrstatussebelumnya(62) As Integer, dnrjmlrevisi(63) As Integer, dnrcetakanke(64) As Integer, 
        'dnrinputuser(65) As Integer, dnrinputtgl(66) As DateTime, dnrmodifikasiuser(67) As Integer, dnrmodifikasitgl(68) As DateTime, dnrposting(69) As Integer, 
        'dnrtutupperiode(70) As Integer, dnrisclose(71) As Integer, dnrcustomtext1(72) As String, dnrcustomtext2(73) As String, dnrcustomtext3(74) As String, 
        'dnrcustomtext4(75) As String, dnrcustomtext5(76) As String, dnrcustomint1(77) As Integer, dnrcustomint2(78) As Integer, dnrcustomint3(79) As Integer, 
        'dnrcustomdbl1(80) As Double, dnrcustomdbl2(81) As Double, dnrcustomdbl3(82) As Double, dnrcustomdate1(83) As Date, dnrcustomdate2(84) As Date, 
        'dnrcustomdate3(85) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatus, dnrstatussebelumnya, 
        'dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, 
        'dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, 
        'dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, 
        'dnrcustomdate2, dnrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 86) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'dnrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "dnrid required numeric." : GoTo selesai
        End If
        'dnrasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "dnrasalbarangkategori required numeric." : GoTo selesai
        End If
        'dnrjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "dnrjenispembeliankategori required numeric." : GoTo selesai
        End If
        'dnrcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "dnrcarabayar required numeric." : GoTo selesai
        End If
        'dnrautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "dnrautonotransaksi required numeric." : GoTo selesai
        End If
        'dnrtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "dnrtgl required date." : GoTo selesai
        End If
        'dnrkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "dnrkodepa required numeric." : GoTo selesai
        End If
        'dnrsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "dnrsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "dnrsupplier can't be empty." : GoTo selesai
        End If
        'dnrbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "dnrbagianpembelian required numeric." : GoTo selesai
        End If
        'dnrtgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "dnrtgljatuhtempo required date." : GoTo selesai
        End If
        'dnrtglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "dnrtglnoref required date." : GoTo selesai
        End If
        'dnrtglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "dnrtglpenutupan required date." : GoTo selesai
        End If
        'dnrkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "dnrkurs required numeric." : GoTo selesai
        End If
        'dnrhargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "dnrhargatermasukpajak required numeric." : GoTo selesai
        End If
        'dnrtotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "dnrtotal required numeric." : GoTo selesai
        End If
        'dnrjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dnrjmldiskon required numeric." : GoTo selesai
        End If
        'dnrtotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dnrtotalpajak1detail required numeric." : GoTo selesai
        End If
        'dnrtotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "dnrtotalpajak2detail required numeric." : GoTo selesai
        End If
        'dnrbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dnrbiayalain required numeric." : GoTo selesai
        End If
        'dnrtotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "dnrtotaltransaksi required numeric." : GoTo selesai
        End If
        'dnrjmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "dnrjmlbayar required numeric." : GoTo selesai
        End If
        'dnrstatuslunas(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "dnrstatuslunas required numeric." : GoTo selesai
        End If
        'dnrtgllunas(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "dnrtgllunas required date." : GoTo selesai
        End If
        'dnrsdhbayarpajak(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "dnrsdhbayarpajak required numeric." : GoTo selesai
        End If
        'dnrtglbayarpajak(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "dnrtglbayarpajak required date." : GoTo selesai
        End If
        'dnridpr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "dnridpr required numeric." : GoTo selesai
        End If
        'dnridcs(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "dnridcs required numeric." : GoTo selesai
        End If
        'dnridrq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "dnridrq required numeric." : GoTo selesai
        End If
        'dnridbs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "dnridbs required numeric." : GoTo selesai
        End If
        'dnridpo(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "dnridpo required numeric." : GoTo selesai
        End If
        'dnridipc(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "dnridipc required numeric." : GoTo selesai
        End If
        'dnridgrn(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "dnridgrn required numeric." : GoTo selesai
        End If
        'dnridri(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "dnridri required numeric." : GoTo selesai
        End If
        'dnrstatusprt(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "dnrstatusprt required numeric." : GoTo selesai
        End If
        'dnrstatus(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "dnrstatus required numeric." : GoTo selesai
        End If
        'dnrstatussebelumnya(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "dnrstatussebelumnya required numeric." : GoTo selesai
        End If
        'dnrjmlrevisi(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "dnrjmlrevisi required numeric." : GoTo selesai
        End If
        'dnrcetakanke(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "dnrcetakanke required numeric." : GoTo selesai
        End If
        'dnrinputuser(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "dnrinputuser required numeric." : GoTo selesai
        End If
        'dnrinputtgl(66) As DateTime
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "dnrinputtgl required date." : GoTo selesai
        End If
        'dnrmodifikasiuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "dnrmodifikasiuser required numeric." : GoTo selesai
        End If
        'dnrmodifikasitgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "dnrmodifikasitgl required date." : GoTo selesai
        End If
        'dnrposting(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "dnrposting required numeric." : GoTo selesai
        End If
        'dnrtutupperiode(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "dnrtutupperiode required numeric." : GoTo selesai
        End If
        'dnrisclose(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "dnrisclose required numeric." : GoTo selesai
        End If
        'dnrcustomint1(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "dnrcustomint1 required numeric." : GoTo selesai
        End If
        'dnrcustomint2(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "dnrcustomint2 required numeric." : GoTo selesai
        End If
        'dnrcustomint3(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "dnrcustomint3 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "dnrcustomdbl1 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "dnrcustomdbl2 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "dnrcustomdbl3 required numeric." : GoTo selesai
        End If
        'dnrcustomdate1(83) As Date
        If (IsDate(dataUtama(83)) = False) Then
            result(2) = "dnrcustomdate1 required date." : GoTo selesai
        End If
        'dnrcustomdate2(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "dnrcustomdate2 required date." : GoTo selesai
        End If
        'dnrcustomdate3(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "dnrcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dnrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dnrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dnrcabang should not be more than 25 character." : GoTo selesai
        End If

        'dnrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dnrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dnrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'dnrgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "dnrgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "dnrgudang should not be more than 25 character." : GoTo selesai
        End If

        'dnrsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dnrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "dnrsumber should not be more than 10 character." : GoTo selesai
        End If

        'dnrnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "dnrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "dnrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'dnrtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "dnrtgl can't be empty" : GoTo selesai
        End If

        'dnrtgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "dnrtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'dnrtglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "dnrtglnoref can't be empty" : GoTo selesai
        End If

        'dnrtglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "dnrtglpenutupan can't be empty" : GoTo selesai
        End If

        'dnrmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "dnrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "dnrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'dnrkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "dnrkurs can't be empty" : GoTo selesai
        End If

        'dnrtotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "dnrtotal can't be empty" : GoTo selesai
        End If

        'dnrdiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dnrdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "dnrdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'dnrjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "dnrjmldiskon can't be empty" : GoTo selesai
        End If

        'dnrtotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dnrtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'dnrtotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dnrtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'dnrbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dnrbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "dnrbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'dnrbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dnrbiayalain can't be empty" : GoTo selesai
        End If

        'dnrtotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dnrtotaltransaksi can't be empty" : GoTo selesai
        End If

        'dnrjmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dnrjmlbayar can't be empty" : GoTo selesai
        End If

        'dnrtgllunas(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "dnrtgllunas can't be empty" : GoTo selesai
        End If

        'dnrtglbayarpajak(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "dnrtglbayarpajak can't be empty" : GoTo selesai
        End If

        'dnrinputtgl(66) As DateTime
        If Len(dataUtama(66)) = 0 Then
            result(2) = "dnrinputtgl can't be empty" : GoTo selesai
        End If

        'dnrmodifikasitgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "dnrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "dnrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "dnrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "dnrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate1(83) As Date
        If Len(dataUtama(83)) = 0 Then
            result(2) = "dnrcustomdate1 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate2(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "dnrcustomdate2 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate3(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "dnrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "dnrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnridpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "dnrid~dnrcabang~dnrlokasi~dnrgudang~dnrasalbarang~dnrasalbarangkategori~dnrjenispembelian~dnrjenispembeliankategori~dnrcarabayar~dnrsumber~dnrautonotransaksi~dnrnotransaksi~dnrtgl~dnrkodepa~dnrsupplier~dnrsupplierkontak~dnr1alamat1~dnr1alamat2~dnr1alamat3~dnr2alamat1~dnr2alamat2~dnr2alamat3~dnrbagianpembelian~dnrtermin~dnrtgljatuhtempo~dnruraian~dnrcatatan~dnrnoref~dnrtglnoref~dnrtglpenutupan~dnrmatauang~dnrkurs~dnrhargatermasukpajak~dnrtotal~dnrdiskonpersen~dnrjmldiskon~dnrtotalpajak1detail~dnrtotalpajak2detail~dnrbiayalainpersen~dnrbiayalain~dnrtotaltransaksi~dnrjmlbayar~dnrstatuslunas~dnrtgllunas~dnrnofakturpajak~dnrsdhbayarpajak~dnrtglbayarpajak~dnrrekdiskon~dnrrekpajak1~dnrrekpajak2~dnrrekbiayalain~dnrrekbayar~dnridpr~dnridcs~dnridrq~dnridbs~dnridpo~dnridipc~dnridgrn~dnridri~dnrstatusprt~dnrstatus~dnrstatussebelumnya~dnrjmlrevisi~dnrcetakanke~dnrinputuser~dnrinputtgl~dnrmodifikasiuser~dnrmodifikasitgl~dnrposting~dnrtutupperiode~dnrisclose~dnrcustomtext1~dnrcustomtext2~dnrcustomtext3~dnrcustomtext4~dnrcustomtext5~dnrcustomint1~dnrcustomint2~dnrcustomint3~dnrcustomdbl1~dnrcustomdbl2~dnrcustomdbl3~dnrcustomdate1~dnrcustomdate2~dnrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddnrdetail(0) As Integer, iddnr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, idhppkhususmasuk(13) As Integer, idhppfifomasuk(14) As Integer, 
        'hpp(15) As Double, harga(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekdiskonpembelian(29) As String, 
        'rekhargapokok(30) As String, rekreturpembelian(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idprdetail(38) As Integer, idcsdetail(39) As Integer, 
        'idrqdetail(40) As Integer, idbsdetail(41) As Integer, idpodetail(42) As Integer, idipcdetail(43) As Integer, idgrndetail(44) As Integer, 
        'idridetail(45) As Integer, jmlprt(46) As Double, statusprt(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, 
        'idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, 
        'idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddnrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddnr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "transbarang", AsEnumTypeData.AsInt64)

        'Variabel ValidasiBatchSerial
        Dim ftBarang As String = ""

        'Variabel ValidasiSimpan
        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = ""
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = "", updStokIn As String = "", gudangIn As String = ""
        Dim idbarang As Integer = 0, idridetail As Integer = 0, jmlbarang As Double = 0
        Dim dtCostCenter As New DataTable, vTransBarang As Integer = 1

        'FILTER RI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRI As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 58) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddnrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddnrdetail required numeric." : GoTo selesai
            End If
            'iddnr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddnr required numeric." : GoTo selesai
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
            'jmlprt(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
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
            'customdate1(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
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
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(17) As String
                '    dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(17).ToString))
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

            'jmlprt(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
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

            'customdate1(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
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
            

            If AsDataTableTambahData(dtdetail, "iddnrdetail~iddnr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~idhppkhususmasuk~idhppfifomasuk~hpp~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekdiskonpembelian~rekhargapokok~rekreturpembelian~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~idridetail~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~transbarang", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & vTransBarang) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangasal(25) As String      , gudangtransit(26) As String   , idridetail(45) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(25) : gudangIn = dataRowDetail(26) : idridetail = dataRowDetail(45)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'ValidasiSimpan
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

            'VALIDASI STOK -------------------------------
            If vTransBarang = 1 Then
                '1. CEK DATA EXIST STOK KELUAR
                ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                '2. CEK JML STOK KELUAR
                Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangasal='" & gudangOut & "' AND transbarang = 1")
                ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                '3. SET NILAI UPDATE STOK KELUAR
                updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                '4. SET NILAI UPDATE STOK MASUK
                updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
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
                vStatus = drutama("dnrstatus")
                vTgl = AsFormatTanggal(drutama("dnrtgl"))


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 4, vMenuId As Integer = 12
                Select Case drutama("dnrstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("dnrtgl")), AsFormatTanggal(drutama("dnrtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("dnrstatus") = 2 Or drutama("dnrstatus") = 1 Or drutama("dnrstatus") = 8 Or drutama("dnrstatus") = 9 Or drutama("dnrstatus") = 10 Or drutama("dnrstatus") = 11 Then

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

                    ''ValidasiHppI
                    'Dim rsValidasi As String = ValidasiHppI(dtdetail, ftBarang)
                    'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingRI, ftOutstandingRI, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangasal", ftRI, drutama("dnrhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("dnrtermin").ToString, AsFormatTanggal(drutama("dnrtgl")), "dnrtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("dnrtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("dnrtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("dnrtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("dnrtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("dnrhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("dnrtotaltransaksi") = Double.Parse(drutama("dnrtotal")) - Double.Parse(drutama("dnrjmldiskon")) + Double.Parse(drutama("dnrtotalpajak1detail")) + Double.Parse(drutama("dnrtotalpajak2detail")) + Double.Parse(drutama("dnrbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("dnrtotaltransaksi") = Double.Parse(drutama("dnrtotal")) - Double.Parse(drutama("dnrjmldiskon")) + Double.Parse(drutama("dnrbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("dnrid")
                    notransaksi = drutama("dnrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(dnrid), dnrnotransaksi FROM M4_dnr WHERE dnrid='" & result(4) & "' AND dnrstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("dnrautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dnrcabang"), drutama("dnrlokasi"), drutama("dnrsumber"), drutama("dnrtgl"), drutama("dnrsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(dnrid) FROM m4_dnr WHERE dnrnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_dnr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Dnr_HistorySimpan("" & paramSplit(0) & "★M4_Dnr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("dnrsumber")) & "▼" & FixQuotes(drutama("dnrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Dnr set dnrcabang  = '" & FixQuotes(drutama("dnrcabang")) & "', dnrlokasi  = '" & FixQuotes(drutama("dnrlokasi")) & "', dnrgudang  = '" & FixQuotes(drutama("dnrgudang")) & "', dnrasalbarang  = '" & FixQuotes(drutama("dnrasalbarang")) & "', dnrasalbarangkategori  = " & drutama("dnrasalbarangkategori") & ", dnrjenispembelian  = '" & FixQuotes(drutama("dnrjenispembelian")) & "', dnrjenispembeliankategori  = " & drutama("dnrjenispembeliankategori") & ", dnrcarabayar  = " & drutama("dnrcarabayar") & ", dnrsumber  = '" & FixQuotes(drutama("dnrsumber")) & "', dnrautonotransaksi  = " & drutama("dnrautonotransaksi") & ", dnrnotransaksi  = '" & FixQuotes(notransaksi) & "', dnrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', dnrkodepa  = " & drutama("dnrkodepa") & ", dnrsupplier  = " & drutama("dnrsupplier") & ", dnrsupplierkontak  = '" & FixQuotes(drutama("dnrsupplierkontak")) & "', dnr1alamat1  = '" & FixQuotes(drutama("dnr1alamat1")) & "', dnr1alamat2  = '" & FixQuotes(drutama("dnr1alamat2")) & "', dnr1alamat3  = '" & FixQuotes(drutama("dnr1alamat3")) & "', dnr2alamat1  = '" & FixQuotes(drutama("dnr2alamat1")) & "', dnr2alamat2  = '" & FixQuotes(drutama("dnr2alamat2")) & "', dnr2alamat3  = '" & FixQuotes(drutama("dnr2alamat3")) & "', dnrbagianpembelian  = " & drutama("dnrbagianpembelian") & ", dnrtermin  = '" & FixQuotes(drutama("dnrtermin")) & "', dnrtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgljatuhtempo"))) & "', dnruraian  = '" & FixQuotes(drutama("dnruraian")) & "', dnrcatatan  = '" & FixQuotes(drutama("dnrcatatan")) & "', dnrnoref  = '" & FixQuotes(drutama("dnrnoref")) & "', dnrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglnoref"))) & "', dnrtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglpenutupan"))) & "', dnrmatauang  = '" & FixQuotes(drutama("dnrmatauang")) & "', dnrkurs  = '" & FixDouble(drutama("dnrkurs")) & "', dnrhargatermasukpajak  = " & drutama("dnrhargatermasukpajak") & ", dnrtotal  = '" & FixDouble(drutama("dnrtotal")) & "', dnrdiskonpersen  = '" & FixQuotes(drutama("dnrdiskonpersen")) & "', dnrjmldiskon  = '" & FixDouble(drutama("dnrjmldiskon")) & "', dnrtotalpajak1detail  = '" & FixDouble(drutama("dnrtotalpajak1detail")) & "', dnrtotalpajak2detail  = '" & FixDouble(drutama("dnrtotalpajak2detail")) & "', dnrbiayalainpersen  = '" & FixQuotes(drutama("dnrbiayalainpersen")) & "', dnrbiayalain  = '" & FixDouble(drutama("dnrbiayalain")) & "', dnrtotaltransaksi  = '" & FixDouble(drutama("dnrtotaltransaksi")) & "', dnrjmlbayar  = '" & FixDouble(drutama("dnrjmlbayar")) & "', dnrstatuslunas  = " & drutama("dnrstatuslunas") & ", dnrtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgllunas"))) & "', dnrnofakturpajak  = '" & FixQuotes(drutama("dnrnofakturpajak")) & "', dnrsdhbayarpajak  = " & drutama("dnrsdhbayarpajak") & ", dnrtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglbayarpajak"))) & "', dnrrekdiskon  = '" & FixQuotes(drutama("dnrrekdiskon")) & "', dnrrekpajak1  = '" & FixQuotes(drutama("dnrrekpajak1")) & "', dnrrekpajak2  = '" & FixQuotes(drutama("dnrrekpajak2")) & "', dnrrekbiayalain  = '" & FixQuotes(drutama("dnrrekbiayalain")) & "', dnrrekbayar  = '" & FixQuotes(drutama("dnrrekbayar")) & "', dnridpr  = " & drutama("dnridpr") & ", dnridcs  = " & drutama("dnridcs") & ", dnridrq  = " & drutama("dnridrq") & ", dnridbs  = " & drutama("dnridbs") & ", dnridpo  = " & drutama("dnridpo") & ", dnridipc  = " & drutama("dnridipc") & ", dnridgrn  = " & drutama("dnridgrn") & ", dnridri  = " & drutama("dnridri") & ", dnrstatusprt  = " & drutama("dnrstatusprt") & ", dnrstatus  = " & drutama("dnrstatus") & ", dnrstatussebelumnya  = " & drutama("dnrstatussebelumnya") & ", dnrjmlrevisi  = dnrjmlrevisi+1, dnrcetakanke  = " & drutama("dnrcetakanke") & ", dnrmodifikasiuser  = " & drutama("dnrmodifikasiuser") & ", dnrmodifikasitgl  = NOW(), dnrposting  = 0, dnrtutupperiode  = " & drutama("dnrtutupperiode") & ", dnrcustomtext1  = '" & FixQuotes(drutama("dnrcustomtext1")) & "', dnrcustomtext2  = '" & FixQuotes(drutama("dnrcustomtext2")) & "', dnrcustomtext3  = '" & FixQuotes(drutama("dnrcustomtext3")) & "', dnrcustomtext4  = '" & FixQuotes(drutama("dnrcustomtext4")) & "', dnrcustomtext5  = '" & FixQuotes(drutama("dnrcustomtext5")) & "', dnrcustomint1  = " & drutama("dnrcustomint1") & ", dnrcustomint2  = " & drutama("dnrcustomint2") & ", dnrcustomint3  = " & drutama("dnrcustomint3") & ", dnrcustomdbl1  = '" & FixDouble(drutama("dnrcustomdbl1")) & "', dnrcustomdbl2  = '" & FixDouble(drutama("dnrcustomdbl2")) & "', dnrcustomdbl3  = '" & FixDouble(drutama("dnrcustomdbl3")) & "', dnrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate1"))) & "', dnrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate2"))) & "', dnrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate3"))) & "' where dnrid = '" & drutama("dnrid") & "'"
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

                    If drutama("dnrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dnrcabang"), drutama("dnrlokasi"), drutama("dnrsumber"), drutama("dnrtgl"), drutama("dnrsumber"), 4)
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
                        notransaksi = drutama("dnrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(dnrid) FROM m4_dnr WHERE dnrnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Dnr (dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3) values('" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(drutama("dnrgudang")) & "', '" & FixQuotes(drutama("dnrasalbarang")) & "', " & drutama("dnrasalbarangkategori") & ", '" & FixQuotes(drutama("dnrjenispembelian")) & "', " & drutama("dnrjenispembeliankategori") & ", " & drutama("dnrcarabayar") & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & drutama("dnrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrkodepa") & ", " & drutama("dnrsupplier") & ", '" & FixQuotes(drutama("dnrsupplierkontak")) & "', '" & FixQuotes(drutama("dnr1alamat1")) & "', '" & FixQuotes(drutama("dnr1alamat2")) & "', '" & FixQuotes(drutama("dnr1alamat3")) & "', '" & FixQuotes(drutama("dnr2alamat1")) & "', '" & FixQuotes(drutama("dnr2alamat2")) & "', '" & FixQuotes(drutama("dnr2alamat3")) & "', " & drutama("dnrbagianpembelian") & ", '" & FixQuotes(drutama("dnrtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgljatuhtempo"))) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(drutama("dnrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtglpenutupan"))) & "', '" & FixQuotes(drutama("dnrmatauang")) & "', '" & FixDouble(drutama("dnrkurs")) & "', " & drutama("dnrhargatermasukpajak") & ", '" & FixDouble(drutama("dnrtotal")) & "', '" & FixQuotes(drutama("dnrdiskonpersen")) & "', '" & FixDouble(drutama("dnrjmldiskon")) & "', '" & FixDouble(drutama("dnrtotalpajak1detail")) & "', '" & FixDouble(drutama("dnrtotalpajak2detail")) & "', '" & FixQuotes(drutama("dnrbiayalainpersen")) & "', '" & FixDouble(drutama("dnrbiayalain")) & "', '" & FixDouble(drutama("dnrtotaltransaksi")) & "', '" & FixDouble(drutama("dnrjmlbayar")) & "', " & drutama("dnrstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("dnrtgllunas"))) & "', '" & FixQuotes(drutama("dnrnofakturpajak")) & "', " & drutama("dnrsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("dnrtglbayarpajak"))) & "', '" & FixQuotes(drutama("dnrrekdiskon")) & "', '" & FixQuotes(drutama("dnrrekpajak1")) & "', '" & FixQuotes(drutama("dnrrekpajak2")) & "', '" & FixQuotes(drutama("dnrrekbiayalain")) & "', '" & FixQuotes(drutama("dnrrekbayar")) & "', " & drutama("dnridpr") & ", " & drutama("dnridcs") & ", " & drutama("dnridrq") & ", " & drutama("dnridbs") & ", " & drutama("dnridpo") & ", " & drutama("dnridipc") & ", " & drutama("dnridgrn") & ", " & drutama("dnridri") & ", " & drutama("dnrstatusprt") & ", " & drutama("dnrstatus") & ", " & drutama("dnrstatussebelumnya") & ", " & drutama("dnrjmlrevisi") & ", " & drutama("dnrcetakanke") & ", " & drutama("dnrinputuser") & ", NOW(), " & drutama("dnrmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("dnrtutupperiode") & ", " & drutama("dnrisclose") & ", '" & FixQuotes(drutama("dnrcustomtext1")) & "', '" & FixQuotes(drutama("dnrcustomtext2")) & "', '" & FixQuotes(drutama("dnrcustomtext3")) & "', '" & FixQuotes(drutama("dnrcustomtext4")) & "', '" & FixQuotes(drutama("dnrcustomtext5")) & "', " & drutama("dnrcustomint1") & ", " & drutama("dnrcustomint2") & ", " & drutama("dnrcustomint3") & ", '" & FixDouble(drutama("dnrcustomdbl1")) & "', '" & FixDouble(drutama("dnrcustomdbl2")) & "', '" & FixDouble(drutama("dnrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select dnrid from M4_dnr where dnrnotransaksi='" & notransaksi & "' AND dnrinputuser= '" & userid & "' order by dnrmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Dnr_Detail where iddnr = '" & result(4) & "'"
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
                    Dim dtRI As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("dnrmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI RI ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m4_ri_detail WHERE idridetail = '" & FixDouble(dr1("idridetail")) & "'"
                        dtRI = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtRI.Rows.Count > 0 Then
                            'SET HARGA - ambil dari RI
                            dr1("harga") = Double.Parse(dtRI.Rows(0)("harga"))

                            'SET DISKON - ambil dari RI
                            dr1("diskon") = dtRI.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari RI
                            dr1("pajak1") = dtRI.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari RI = (jmlpajakri / jmlri) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtRI.Rows(0)("jmlpajak1")) / Double.Parse(dtRI.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari RI
                            dr1("pajak2") = dtRI.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari RI = (jmlpajakri / jmlri) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtRI.Rows(0)("jmlpajak2")) / Double.Parse(dtRI.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI RI -----------------------------------------------


                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddnrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekreturpembelian")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", " & dr1("idridetail") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Dnr_Detail(iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DNR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DNR'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama  = '" & result(4) & "' AND atsumber = 'DNR'"
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


                If drutama("dnrstatus") = 2 Then
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idri FROM m4_ri_detail WHERE " & updFilterRI & " GROUP BY idri", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_ri_detail WHERE " & ftDetail & " GROUP BY idri", myConn)
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'AMBIL GUDANG TRANSIT DARI SETTING ==============================================
                    Dim SetGudang As String = ""
                    'GUDANG SETTING TRANSIT DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG ASAL 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TRANSIT BERTAMBAH
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 3 AND sgrup = 'defaultgudang' AND skode = 'GudangTransit'"
                    Dim dtSetGudang As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtSetGudang.Rows.Count > 0 Then
                        SetGudang = dtSetGudang.Rows(0)("snilai")
                    Else
                        result(2) = "Setting for Transit Warehouse not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF AMBIL GUDANG TRANSIT DARI SETTING =======================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
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

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
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
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
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

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
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
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.agudang = '" & SetGudang & "' WHERE a.aid IN(" & strValue2.ToString & ")"
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
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    'sql = "SELECT dnrd.iddnrdetail, dnrd.idbarang, dnrd.namabarang, dnrd.tipebarang, dnrd.jml, dnrd.satuan, dnrd.jmlbarang, dnrd.satuanbarang, dnrd.matauang, dnrd.kurs, dnrd.harga, dnrd.diskon, dnrd.jmldiskon, dnrd.hpp, dnrd.idhppkhususmasuk, dnrd.gudangasal, dnrd.gudangtransit, dnrd.gudangtujuan, dnrd.catatan, dnrd.costcenter, dnrd.divisi, dnrd.subdivisi, dnrd.proyek, dnr.dnrinputtgl, i.bhpp FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid JOIN m1_item i ON dnrd.idbarang = i.bid WHERE dnrd.iddnr = '" & result(4) & "'"
                    sql = "SELECT dnrd.iddnrdetail, dnrd.idbarang, dnrd.namabarang, dnrd.tipebarang, dnrd.jml, dnrd.satuan, dnrd.jmlbarang, dnrd.satuanbarang, dnrd.matauang, dnrd.kurs, dnrd.harga, dnrd.diskon, dnrd.jmldiskon, dnrd.hpp, dnrd.idhppkhususmasuk, dnrd.gudangasal, dnrd.gudangtransit, dnrd.gudangtujuan, dnrd.catatan, dnrd.costcenter, dnrd.divisi, dnrd.subdivisi, dnrd.proyek, dnr.dnrinputtgl, i.bhpp, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid JOIN m1_item i ON dnrd.idbarang = i.bid LEFT JOIN m1_cost_center cc ON dnrd.costcenter = cc.cckode WHERE dnrd.iddnr = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            If Double.Parse(dr1("transbarang")) = 1 Then
                                'jenismutasi dan postinghpp 
                                '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                                '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                                '- untuk transaksi mutasi saja maka postinghpp = 0
                                postinghpp = 0

                                'hitung hpp = hpp
                                hpp = Double.Parse(dr1("hpp"))

                                'POSTING BARANG KELUAR (gudangasal)
                                jenismutasi = 0
                                'QUERY INSERT TRANSAKSI BARANG KELUAR
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                              cabang,                                    lokasi,                                 gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                                'POSTING BARANG MASUK (gudangtransit)
                                jenismutasi = 1
                                'QUERY INSERT TRANSAKSI BARANG MASUK
                                strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                                'mapping                        id,                              cabang,                                    lokasi,                                    gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                            End If
                        Next

                        If Len(strTransaksiBarang.ToString) > 0 Then
                            sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
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
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "DNR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_DnrUpdateStatus(ByVal param As String) As String

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
        Dim dtdetail As DataTable, dtasset As DataTable
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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
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
            Dim sumber As String = "Dnr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Dnrtgl, Dnrnotransaksi, Dnrstatus FROM M4_Dnr WHERE Dnrid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dnrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_dnr_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Dnr_HistorySimpan("" & paramSplit(0) & "★M4_Dnr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_dnr_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'CEK NO BATCH DAN SERIAL ========================================================
                'BATCH
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nbikode FROM m1_no_batch_in JOIN m1_item ON nbiidbarang = bid WHERE nbisumber = '" & sumber & "' AND nbiidtransaksi = '" & idtransaksi & "' AND nbijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Batch : " & dtdetail.Rows(0)("nbikode") & " has related transactions." : Trans.Rollback() : GoTo selesai

                'SERIAL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT bkode, nsikode FROM m1_no_serial_in JOIN m1_item ON nsiidbarang = bid WHERE nsisumber = '" & sumber & "' AND nsiidtransaksi = '" & idtransaksi & "' AND nsijmlkeluar > 0", myConn)
                If dtdetail.Rows.Count > 0 Then result(2) = "Item : " & dtdetail.Rows(0)("bkode") & " | No. Serial : " & dtdetail.Rows(0)("nsikode") & " has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK NO BATCH DAN SERIAL =================================================


                Dim idbarang As Integer = 0, idridetail As Integer = 0, idhppkhususmasuk As Integer = 0, jmlbarang As Double = 0
                Dim gudangOut As String = "", gudangIn As String = "", ftExistStok As String = "", ftStok As String = "", updStokIn As String = "", updStokOut As String = ""
                Dim updNilaiRI As String = "", updFilterRI As String = "", updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""


                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT iddnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idridetail, gudangasal, gudangtransit, gudangtujuan, idhppkhususmasuk, idhppfifomasuk, urutan FROM m4_dnr_detail WHERE iddnr = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT iddnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idridetail, gudangasal, gudangtransit, gudangtujuan, idhppkhususmasuk, idhppfifomasuk, urutan, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m4_dnr_detail dnrd LEFT JOIN m1_cost_center cc ON dnrd.costcenter = cc.cckode WHERE iddnr = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : idridetail = dr1("idridetail") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangasal") : gudangOut = dr1("gudangtransit") : idhppkhususmasuk = dr1("idhppkhususmasuk")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idridetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                            updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRI)

                            '2.2. SET FILTERUPDATE OUTSTANDING
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        If Double.Parse(dr1("transbarang")) = 1 Then
                            '1. CEK DATA EXIST
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idridetail & "' as idridetail, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            '2. CEK JML STOK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "' AND transbarang = 1")
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                            '3. SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                            '4. SET NILAI UPDATE STOK MASUK
                            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                        End If
                       
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If


                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'VALIDASI GUDANG ASSET ---------------
                'ValidasiGudangAsset
                dtasset = AsDataTableAmbilDariDBCon("SELECT atasetid, atidbarang, atkode FROM M7_Asset_Transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "' ", myConn)
                rsValidasi = ValidasiGudangAsset(dtasset, gudangOut)
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                'END OF VALIDASI GUDANG ASSET --------


                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterRI) > 0 Then
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
                    'END OF UPDATE OUTSTANDING DETAIL ---------------

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idri FROM m4_ri_detail WHERE " & updFilterRI & " GROUP BY idri", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_ri_detail WHERE " & ftDetail & " GROUP BY idri", myConn)
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
                    'END OF UPDATE OUTSTANDING UTAMA ----------------
                End If
                'END OF UPDATE OUTSTANDING ======================================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDBCon("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'", myConn)
                If dtBatch.Rows.Count > 0 Then
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


                'UPDATE NO ASSET ===============================================================
                If dtasset.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "UPDATE m7_asset a SET a.agudang = '" & gudangIn & "' WHERE a.aid IN(" & strValue2.ToString & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If
                'END OF UPDATE NO ASSET ========================================================


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
                'END OF UPDATE STOK =============================================================


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

            End If

            'update status utama
            sql = "UPDATE M4_Dnr SET Dnrstatus = " & nilaiStatus & ", Dnrmodifikasiuser='" & userid & "', Dnrmodifikasitgl = NOW(), Dnrposting = 0, Dnrpostingtgl = '1971-01-01 00:00:00', Dnrjmlrevisi = Dnrjmlrevisi + 1 WHERE Dnrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_DnrSearch(PostWsSearch(paramSplit(0), "M4_DnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_DnrDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
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
            Dim sumber As String = "DNR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Dnrid, Dnrnotransaksi FROM M4_Dnr WHERE Dnrid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dnrcabang, dnrlokasi, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl"
            sql &= " FROM M4_dnr"
            sql &= " WHERE dnrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dnrcabang")
                lokasi = dtNomorNext.Rows(0)("dnrlokasi")
                sumber = dtNomorNext.Rows(0)("dnrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("dnrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("dnrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("dnrtgl"))
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
            sql = "DELETE FROM M4_Dnr_Detail WHERE iddnr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Dnr WHERE dnrid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_DnrSearch(PostWsSearch(paramSplit(0), "M4_DnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_DnrGetdataById(ByVal param As String) As String

        'M4_DnrGetdataById Utama --------------------------------------------------------
        'dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, 
        'dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, 
        'dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, 
        'dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, 
        'dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3, dnrcabangnama, dnrlokasinama, dnrgudangnama, 
        'dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, dnrterminnama, dnrterminharijatuhtempo, dnrrekdiskonnama, 
        'dnrrekpajak1nama, dnrrekpajak2nama, dnrrekbiayalainnama, dnrrekbayarnama, dnrnotransaksigrn, dnrnotransaksiri, dnrstatusnama, 
        'dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama, kpkp

        'M4_DnrGetdataById Detail -------------------------------------------------------
        'iddnrdetail, iddnr, idbarang, namabarang, 
        'tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, 
        'kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, 
        'costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, 
        'idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, 
        'jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, 
        'kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, 
        'costcenternama, divisinama, subdivisinama, proyeknama, idgrn, grnnotransaksi, rinotransaksi, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M4_DnrGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M4_DnrGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M4_DnrGetdataById Asset --------------------------------------------------------
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
        Dim sumber As String = "DNR", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Dnr~M4_Dnr_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "dnrid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "dnrid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_dnr_getdata")
        sql = "select `dnr`.`dnrid` AS `dnrid`,`dnr`.`dnrcabang` AS `dnrcabang`,`dnr`.`dnrlokasi` AS `dnrlokasi`,`dnr`.`dnrgudang` AS `dnrgudang`,`dnr`.`dnrasalbarang` AS `dnrasalbarang`,`dnr`.`dnrasalbarangkategori` AS `dnrasalbarangkategori`,`dnr`.`dnrjenispembelian` AS `dnrjenispembelian`,`dnr`.`dnrjenispembeliankategori` AS `dnrjenispembeliankategori`,`dnr`.`dnrcarabayar` AS `dnrcarabayar`,`dnr`.`dnrsumber` AS `dnrsumber`,`dnr`.`dnrautonotransaksi` AS `dnrautonotransaksi`,`dnr`.`dnrnotransaksi` AS `dnrnotransaksi`,`dnr`.`dnrtgl` AS `dnrtgl`,`dnr`.`dnrkodepa` AS `dnrkodepa`,`dnr`.`dnrsupplier` AS `dnrsupplier`,`dnr`.`dnrsupplierkontak` AS `dnrsupplierkontak`,`dnr`.`dnr1alamat1` AS `dnr1alamat1`,`dnr`.`dnr1alamat2` AS `dnr1alamat2`,`dnr`.`dnr1alamat3` AS `dnr1alamat3`,`dnr`.`dnr2alamat1` AS `dnr2alamat1`,`dnr`.`dnr2alamat2` AS `dnr2alamat2`,`dnr`.`dnr2alamat3` AS `dnr2alamat3`,`dnr`.`dnrbagianpembelian` AS `dnrbagianpembelian`,`dnr`.`dnrtermin` AS `dnrtermin`,`dnr`.`dnrtgljatuhtempo` AS `dnrtgljatuhtempo`,`dnr`.`dnruraian` AS `dnruraian`,`dnr`.`dnrcatatan` AS `dnrcatatan`,`dnr`.`dnrnoref` AS `dnrnoref`,`dnr`.`dnrtglnoref` AS `dnrtglnoref`,`dnr`.`dnrtglpenutupan` AS `dnrtglpenutupan`,`dnr`.`dnrmatauang` AS `dnrmatauang`,`dnr`.`dnrkurs` AS `dnrkurs`,`dnr`.`dnrhargatermasukpajak` AS `dnrhargatermasukpajak`,`dnr`.`dnrtotal` AS `dnrtotal`,`dnr`.`dnrdiskonpersen` AS `dnrdiskonpersen`,`dnr`.`dnrjmldiskon` AS `dnrjmldiskon`,`dnr`.`dnrtotalpajak1detail` AS `dnrtotalpajak1detail`,`dnr`.`dnrtotalpajak2detail` AS `dnrtotalpajak2detail`,`dnr`.`dnrbiayalainpersen` AS `dnrbiayalainpersen`,`dnr`.`dnrbiayalain` AS `dnrbiayalain`,`dnr`.`dnrtotaltransaksi` AS `dnrtotaltransaksi`,`dnr`.`dnrjmlbayar` AS `dnrjmlbayar`,`dnr`.`dnrstatuslunas` AS `dnrstatuslunas`,`dnr`.`dnrtgllunas` AS `dnrtgllunas`,`dnr`.`dnrnofakturpajak` AS `dnrnofakturpajak`,`dnr`.`dnrsdhbayarpajak` AS `dnrsdhbayarpajak`,`dnr`.`dnrtglbayarpajak` AS `dnrtglbayarpajak`,`dnr`.`dnrrekdiskon` AS `dnrrekdiskon`,`dnr`.`dnrrekpajak1` AS `dnrrekpajak1`,`dnr`.`dnrrekpajak2` AS `dnrrekpajak2`,`dnr`.`dnrrekbiayalain` AS `dnrrekbiayalain`,`dnr`.`dnrrekbayar` AS `dnrrekbayar`,`dnr`.`dnridpr` AS `dnridpr`,`dnr`.`dnridcs` AS `dnridcs`,`dnr`.`dnridrq` AS `dnridrq`,`dnr`.`dnridbs` AS `dnridbs`,`dnr`.`dnridpo` AS `dnridpo`,`dnr`.`dnridipc` AS `dnridipc`,`dnr`.`dnridgrn` AS `dnridgrn`,`dnr`.`dnridri` AS `dnridri`,`dnr`.`dnrstatusprt` AS `dnrstatusprt`,`dnr`.`dnrstatusrealisasi` AS `dnrstatusrealisasi`,`dnr`.`dnrstatus` AS `dnrstatus`,`dnr`.`dnrstatussebelumnya` AS `dnrstatussebelumnya`,`dnr`.`dnrjmlrevisi` AS `dnrjmlrevisi`,`dnr`.`dnrcetakanke` AS `dnrcetakanke`,`dnr`.`dnrinputuser` AS `dnrinputuser`,`dnr`.`dnrinputtgl` AS `dnrinputtgl`,`dnr`.`dnrmodifikasiuser` AS `dnrmodifikasiuser`,`dnr`.`dnrmodifikasitgl` AS `dnrmodifikasitgl`,`dnr`.`dnrposting` AS `dnrposting`,`dnr`.`dnrpostingtgl` AS `dnrpostingtgl`,`dnr`.`dnrtutupperiode` AS `dnrtutupperiode`,`dnr`.`dnrisclose` AS `dnrisclose`,`dnr`.`dnrcustomtext1` AS `dnrcustomtext1`,`dnr`.`dnrcustomtext2` AS `dnrcustomtext2`,`dnr`.`dnrcustomtext3` AS `dnrcustomtext3`,`dnr`.`dnrcustomtext4` AS `dnrcustomtext4`,`dnr`.`dnrcustomtext5` AS `dnrcustomtext5`,`dnr`.`dnrcustomint1` AS `dnrcustomint1`,`dnr`.`dnrcustomint2` AS `dnrcustomint2`,`dnr`.`dnrcustomint3` AS `dnrcustomint3`,`dnr`.`dnrcustomdbl1` AS `dnrcustomdbl1`,`dnr`.`dnrcustomdbl2` AS `dnrcustomdbl2`,`dnr`.`dnrcustomdbl3` AS `dnrcustomdbl3`,`dnr`.`dnrcustomdate1` AS `dnrcustomdate1`,`dnr`.`dnrcustomdate2` AS `dnrcustomdate2`,`dnr`.`dnrcustomdate3` AS `dnrcustomdate3`,`br`.`bnama` AS `dnrcabangnama`,`lc`.`lnama` AS `dnrlokasinama`,`wh`.`wnama` AS `dnrgudangnama`,`c1`.`kkode` AS `dnrsupplierkode`,`c1`.`knama` AS `dnrsuppliernama`,`c2`.`kkode` AS `dnrbagianpembeliankode`,`c2`.`knama` AS `dnrbagianpembeliannama`,`tr`.`trnama` AS `dnrterminnama`,`tr`.`trharijatuhtempo` AS `dnrterminharijatuhtempo`,`coa1`.`cnama` AS `dnrrekdiskonnama`,`coa2`.`cnama` AS `dnrrekpajak1nama`,`coa3`.`cnama` AS `dnrrekpajak2nama`,`coa4`.`cnama` AS `dnrrekbiayalainnama`,`coa5`.`cnama` AS `dnrrekbayarnama`,`grn`.`grnnotransaksi` AS `dnrnotransaksigrn`,`ri`.`rinotransaksi` AS `dnrnotransaksiri`,`st1`.`nama` AS `dnrstatusnama`,`st2`.`nama` AS `dnrstatussebelumnyanama`,`u1`.`unama` AS `dnrinputusernama`,`u2`.`unama` AS `dnrmodifikasiusernama`,`dnrd`.`iddnrdetail` AS `iddnrdetail`,`dnrd`.`iddnr` AS `iddnr`,`dnrd`.`idbarang` AS `idbarang`,`dnrd`.`namabarang` AS `namabarang`,`dnrd`.`tipebarang` AS `tipebarang`,`dnrd`.`jml` AS `jml`,`dnrd`.`satuan` AS `satuan`,`dnrd`.`nilaisatuan` AS `nilaisatuan`,`dnrd`.`jmlbarang` AS `jmlbarang`,`dnrd`.`satuanbarang` AS `satuanbarang`,`dnrd`.`matauang` AS `matauang`,`dnrd`.`kurs` AS `kurs`,`dnrd`.`hargafix` AS `hargafix`,`dnrd`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`dnrd`.`idhppfifomasuk` AS `idhppfifomasuk`,`dnrd`.`hpp` AS `hpp`,`dnrd`.`harga` AS `harga`,`dnrd`.`diskon` AS `diskon`,`dnrd`.`jmldiskon` AS `jmldiskon`,`dnrd`.`pajak1` AS `pajak1`,`dnrd`.`jmlpajak1` AS `jmlpajak1`,`dnrd`.`pajak2` AS `pajak2`,`dnrd`.`jmlpajak2` AS `jmlpajak2`,`dnrd`.`cabang` AS `cabang`,`dnrd`.`lokasi` AS `lokasi`,`dnrd`.`gudangasal` AS `gudangasal`,`dnrd`.`gudangtransit` AS `gudangtransit`,`dnrd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`dnrd`.`rekdiskonpembelian` AS `rekdiskonpembelian`,`dnrd`.`rekhargapokok` AS `rekhargapokok`,`dnrd`.`rekreturpembelian` AS `rekreturpembelian`,`dnrd`.`costcenter` AS `costcenter`,`dnrd`.`divisi` AS `divisi`,`dnrd`.`subdivisi` AS `subdivisi`,`dnrd`.`proyek` AS `proyek`,`dnrd`.`catatan` AS `catatan`,`dnrd`.`urutan` AS `urutan`,`dnrd`.`idprdetail` AS `idprdetail`,`dnrd`.`idcsdetail` AS `idcsdetail`,`dnrd`.`idrqdetail` AS `idrqdetail`,`dnrd`.`idbsdetail` AS `idbsdetail`,`dnrd`.`idpodetail` AS `idpodetail`,`dnrd`.`idipcdetail` AS `idipcdetail`,`dnrd`.`idgrndetail` AS `idgrndetail`,`dnrd`.`idridetail` AS `idridetail`,`dnrd`.`jmlprt` AS `jmlprt`,`dnrd`.`statusprt` AS `statusprt`,`dnrd`.`jmlrealisasi` AS `jmlrealisasi`,`dnrd`.`statusrealisasi` AS `statusrealisasi`,`dnrd`.`isclose` AS `isclose`,`dnrd`.`customtext1` AS `customtext1`,`dnrd`.`customtext2` AS `customtext2`,`dnrd`.`customtext3` AS `customtext3`,`dnrd`.`customdbl1` AS `customdbl1`,`dnrd`.`customdbl2` AS `customdbl2`,`dnrd`.`customdbl3` AS `customdbl3`,`dnrd`.`customdate1` AS `customdate1`,`dnrd`.`customdate2` AS `customdate2`,`dnrd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`grnd`.`idgrn` AS `idgrn`,`grn2`.`grnnotransaksi` AS `grnnotransaksi`,`ri2`.`rinotransaksi` AS `rinotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from ((((((((((((((((((((((((((((((((((`m4_dnr` `dnr` join `m4_dnr_detail` `dnrd` on((`dnr`.`dnrid` = `dnrd`.`iddnr`))) left join `m1_branch` `br` on((`br`.`bkode` = `dnr`.`dnrcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `dnr`.`dnrlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `dnr`.`dnrgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `dnr`.`dnrsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `dnr`.`dnrbagianpembelian`))) left join `m1_terms` `tr` on((`dnr`.`dnrtermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`dnr`.`dnrrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`dnr`.`dnrrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`dnr`.`dnrrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`dnr`.`dnrrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`dnr`.`dnrrekbayar` = `coa5`.`cnomor`))) left join `m4_grn` `grn` on((`dnr`.`dnridgrn` = `grn`.`grnid`))) left join `m4_ri` `ri` on((`dnr`.`dnridri` = `ri`.`riid`))) left join `m0_status` `st1` on((`st1`.`kode` = `dnr`.`dnrstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `dnr`.`dnrstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `dnr`.`dnrinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `dnr`.`dnrmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `dnrd`.`idbarang`))) left join `m1_tax` `t1` on((`dnrd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`dnrd`.`pajak2` = `t2`.`tkode`))) left join `m1_subdivision` `sd` on((`dnrd`.`subdivisi` = `sd`.`sdkode`))) left join `m4_ri_detail` `rid` on((`dnrd`.`idridetail` = `rid`.`idridetail`))) left join `m4_ri` `ri2` on((`rid`.`idri` = `ri2`.`riid`))) left join `m1_branch` `brd` on((`dnrd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`dnrd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd1` on((`dnrd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`dnrd`.`gudangtransit` = `whd2`.`wkode`))) left join `m1_warehouse` `whd3` on((`dnrd`.`gudangtujuan` = `whd3`.`wkode`))) left join `m1_cost_center` `cc` on((`dnrd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`dnrd`.`divisi` = `d`.`dkode`))) left join `m1_project` `p` on((`dnrd`.`proyek` = `p`.`pkode`))) left join `m4_grn_detail` `grnd` on((`dnrd`.`idgrndetail` = `grnd`.`idgrndetail`))) left join `m4_grn` `grn2` on((`grnd`.`idgrn` = `grn2`.`grnid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("dnrid"), 0), sptField,
                     FxDB(drutama("dnrcabang"), ""), sptField,
                     FxDB(drutama("dnrlokasi"), ""), sptField,
                     FxDB(drutama("dnrgudang"), ""), sptField,
                     FxDB(drutama("dnrasalbarang"), ""), sptField,
                     FxDB(drutama("dnrasalbarangkategori"), 0), sptField,
                     FxDB(drutama("dnrjenispembelian"), ""), sptField,
                     FxDB(drutama("dnrjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("dnrcarabayar"), 0), sptField,
                     FxDB(drutama("dnrsumber"), ""), sptField,
                     FxDB(drutama("dnrautonotransaksi"), 0), sptField,
                     FxDB(drutama("dnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrkodepa"), 0), sptField,
                     FxDB(drutama("dnrsupplier"), 0), sptField,
                     FxDB(drutama("dnrsupplierkontak"), ""), sptField,
                     FxDB(drutama("dnr1alamat1"), ""), sptField,
                     FxDB(drutama("dnr1alamat2"), ""), sptField,
                     FxDB(drutama("dnr1alamat3"), ""), sptField,
                     FxDB(drutama("dnr2alamat1"), ""), sptField,
                     FxDB(drutama("dnr2alamat2"), ""), sptField,
                     FxDB(drutama("dnr2alamat3"), ""), sptField,
                     FxDB(drutama("dnrbagianpembelian"), 0), sptField,
                     FxDB(drutama("dnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("dnruraian"), ""), sptField,
                     FxDB(drutama("dnrcatatan"), ""), sptField,
                     FxDB(drutama("dnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrmatauang"), ""), sptField,
                     FxDB(drutama("dnrkurs"), 0), sptField,
                     FxDB(drutama("dnrhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("dnrtotal"), 0), sptField,
                     FxDB(drutama("dnrdiskonpersen"), ""), sptField,
                     FxDB(drutama("dnrjmldiskon"), 0), sptField,
                     FxDB(drutama("dnrtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("dnrtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("dnrbiayalainpersen"), ""), sptField,
                     FxDB(drutama("dnrbiayalain"), 0), sptField,
                     FxDB(drutama("dnrtotaltransaksi"), 0), sptField,
                     FxDB(drutama("dnrjmlbayar"), 0), sptField,
                     FxDB(drutama("dnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrnofakturpajak"), ""), sptField,
                     FxDB(drutama("dnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrrekdiskon"), ""), sptField,
                     FxDB(drutama("dnrrekpajak1"), ""), sptField,
                     FxDB(drutama("dnrrekpajak2"), ""), sptField,
                     FxDB(drutama("dnrrekbiayalain"), ""), sptField,
                     FxDB(drutama("dnrrekbayar"), ""), sptField,
                     FxDB(drutama("dnridpr"), 0), sptField,
                     FxDB(drutama("dnridcs"), 0), sptField,
                     FxDB(drutama("dnridrq"), 0), sptField,
                     FxDB(drutama("dnridbs"), 0), sptField,
                     FxDB(drutama("dnridpo"), 0), sptField,
                     FxDB(drutama("dnridipc"), 0), sptField,
                     FxDB(drutama("dnridgrn"), 0), sptField,
                     FxDB(drutama("dnridri"), 0), sptField,
                     FxDB(drutama("dnrstatusprt"), 0), sptField,
                     FxDB(drutama("dnrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("dnrstatus"), 0), sptField,
                     FxDB(drutama("dnrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("dnrjmlrevisi"), 0), sptField,
                     FxDB(drutama("dnrcetakanke"), 0), sptField,
                     FxDB(drutama("dnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("dnrtutupperiode"), 0), sptField,
                     FxDB(drutama("dnrisclose"), 0), sptField,
                     FxDB(drutama("dnrcustomtext1"), ""), sptField,
                     FxDB(drutama("dnrcustomtext2"), ""), sptField,
                     FxDB(drutama("dnrcustomtext3"), ""), sptField,
                     FxDB(drutama("dnrcustomtext4"), ""), sptField,
                     FxDB(drutama("dnrcustomtext5"), ""), sptField,
                     FxDB(drutama("dnrcustomint1"), 0), sptField,
                     FxDB(drutama("dnrcustomint2"), 0), sptField,
                     FxDB(drutama("dnrcustomint3"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl1"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl2"), 0), sptField,
                     FxDB(drutama("dnrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("dnrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("dnrcabangnama"), ""), sptField,
                     FxDB(drutama("dnrlokasinama"), ""), sptField,
                     FxDB(drutama("dnrgudangnama"), ""), sptField,
                     FxDB(drutama("dnrsupplierkode"), ""), sptField,
                     FxDB(drutama("dnrsuppliernama"), ""), sptField,
                     FxDB(drutama("dnrbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("dnrbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("dnrterminnama"), ""), sptField,
                     FxDB(drutama("dnrterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("dnrrekdiskonnama"), ""), sptField,
                     FxDB(drutama("dnrrekpajak1nama"), ""), sptField,
                     FxDB(drutama("dnrrekpajak2nama"), ""), sptField,
                     FxDB(drutama("dnrrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("dnrrekbayarnama"), ""), sptField,
                     FxDB(drutama("dnrnotransaksigrn"), ""), sptField,
                     FxDB(drutama("dnrnotransaksiri"), ""), sptField,
                     FxDB(drutama("dnrstatusnama"), ""), sptField,
                     FxDB(drutama("dnrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("dnrinputusernama"), ""), sptField,
                     FxDB(drutama("dnrmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("iddnrdetail"), 0), sptField,
                     FxDB(dr("iddnr"), 0), sptField,
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
                     FxDB(dr("gudangasalnama"), ""), sptField,
                     FxDB(dr("gudangtransitnama"), ""), sptField,
                     FxDB(dr("gudangtujuannama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("idgrn"), 0), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3, dnrcabangnama, dnrlokasinama, dnrgudangnama, dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, dnrterminnama, dnrterminharijatuhtempo, dnrrekdiskonnama, dnrrekpajak1nama, dnrrekpajak2nama, dnrrekbiayalainnama, dnrrekbayarnama, dnrnotransaksigrn, dnrnotransaksiri, dnrstatusnama, dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama, kpkp" & sptSubParam & "iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, idgrn, grnnotransaksi, rinotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_DnrSearch(ByVal param As String) As String
        'M4_DnrSearch --------------------------------------------------------
        'dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, 
        'dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, 
        'dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcabangnama, dnrlokasinama, dnrgudangnama, 
        'dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, grnnotransaksi, rinotransaksi, dnrstatusnama, 
        'dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama

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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m4_dnr_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Dnr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dnrid"), 0), sptField,
                     FxDB(dr("dnrcabang"), ""), sptField,
                     FxDB(dr("dnrlokasi"), ""), sptField,
                     FxDB(dr("dnrgudang"), ""), sptField,
                     FxDB(dr("dnrasalbarang"), ""), sptField,
                     FxDB(dr("dnrasalbarangkategori"), 0), sptField,
                     FxDB(dr("dnrjenispembelian"), ""), sptField,
                     FxDB(dr("dnrjenispembeliankategori"), 0), sptField,
                     FxDB(dr("dnrcarabayar"), 0), sptField,
                     FxDB(dr("dnrsumber"), ""), sptField,
                     FxDB(dr("dnrautonotransaksi"), 0), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("dnrkodepa"), 0), sptField,
                     FxDB(dr("dnrsupplier"), 0), sptField,
                     FxDB(dr("dnrsupplierkontak"), ""), sptField,
                     FxDB(dr("dnr1alamat1"), ""), sptField,
                     FxDB(dr("dnr1alamat2"), ""), sptField,
                     FxDB(dr("dnr1alamat3"), ""), sptField,
                     FxDB(dr("dnr2alamat1"), ""), sptField,
                     FxDB(dr("dnr2alamat2"), ""), sptField,
                     FxDB(dr("dnr2alamat3"), ""), sptField,
                     FxDB(dr("dnrbagianpembelian"), 0), sptField,
                     FxDB(dr("dnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("dnruraian"), ""), sptField,
                     FxDB(dr("dnrcatatan"), ""), sptField,
                     FxDB(dr("dnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("dnrmatauang"), ""), sptField,
                     FxDB(dr("dnrkurs"), 0), sptField,
                     FxDB(dr("dnrhargatermasukpajak"), 0), sptField,
                     FxDB(dr("dnrtotal"), 0), sptField,
                     FxDB(dr("dnrdiskonpersen"), ""), sptField,
                     FxDB(dr("dnrjmldiskon"), 0), sptField,
                     FxDB(dr("dnrtotalpajak1detail"), 0), sptField,
                     FxDB(dr("dnrtotalpajak2detail"), 0), sptField,
                     FxDB(dr("dnrbiayalainpersen"), ""), sptField,
                     FxDB(dr("dnrbiayalain"), 0), sptField,
                     FxDB(dr("dnrtotaltransaksi"), 0), sptField,
                     FxDB(dr("dnrjmlbayar"), 0), sptField,
                     FxDB(dr("dnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("dnrnofakturpajak"), ""), sptField,
                     FxDB(dr("dnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("dnrrekdiskon"), ""), sptField,
                     FxDB(dr("dnrrekpajak1"), ""), sptField,
                     FxDB(dr("dnrrekpajak2"), ""), sptField,
                     FxDB(dr("dnrrekbiayalain"), ""), sptField,
                     FxDB(dr("dnrrekbayar"), ""), sptField,
                     FxDB(dr("dnridpr"), 0), sptField,
                     FxDB(dr("dnridcs"), 0), sptField,
                     FxDB(dr("dnridrq"), 0), sptField,
                     FxDB(dr("dnridbs"), 0), sptField,
                     FxDB(dr("dnridpo"), 0), sptField,
                     FxDB(dr("dnridipc"), 0), sptField,
                     FxDB(dr("dnridgrn"), 0), sptField,
                     FxDB(dr("dnridri"), 0), sptField,
                     FxDB(dr("dnrstatusprt"), 0), sptField,
                     FxDB(dr("dnrstatusrealisasi"), 0), sptField,
                     FxDB(dr("dnrstatus"), 0), sptField,
                     FxDB(dr("dnrstatussebelumnya"), 0), sptField,
                     FxDB(dr("dnrjmlrevisi"), 0), sptField,
                     FxDB(dr("dnrcetakanke"), 0), sptField,
                     FxDB(dr("dnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("dnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("dnrtutupperiode"), 0), sptField,
                     FxDB(dr("dnrisclose"), 0), sptField,
                     FxDB(dr("dnrcabangnama"), ""), sptField,
                     FxDB(dr("dnrlokasinama"), ""), sptField,
                     FxDB(dr("dnrgudangnama"), ""), sptField,
                     FxDB(dr("dnrsupplierkode"), ""), sptField,
                     FxDB(dr("dnrsuppliernama"), ""), sptField,
                     FxDB(dr("dnrbagianpembeliankode"), ""), sptField,
                     FxDB(dr("dnrbagianpembeliannama"), ""), sptField,
                     FxDB(dr("grnnotransaksi"), ""), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     FxDB(dr("dnrstatusnama"), ""), sptField,
                     FxDB(dr("dnrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("dnrinputusernama"), ""), sptField,
                     FxDB(dr("dnrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatusrealisasi, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrpostingtgl, dnrtutupperiode, dnrisclose, dnrcabangnama, dnrlokasinama, dnrgudangnama, dnrsupplierkode, dnrsuppliernama, dnrbagianpembeliankode, dnrbagianpembeliannama, grnnotransaksi, rinotransaksi, dnrstatusnama, dnrstatussebelumnyanama, dnrinputusernama, dnrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_DnrTerkait(ByVal param As String) As String
        'M4_DnrTerkait --------------------------------------------------------
        'dnrid, dnrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
        sql = query.PanggilQuery("m4_dnr_terkait")
        sql = sql.Replace("validtransaksi", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("dnrid"), 0), sptField,
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
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
            result(2) = "Related DNR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("dnrid, dnrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Dnr_Detail_VSearch(ByVal param As String) As String
        'M4_Dnr_Detail_VSearch --------------------------------------------------------
        'iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, 
        'idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, 
        'idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, dnrnotransaksi, dnruraian, dnrcatatan, 
        'dnrnoref, dnrtglnoref, dnrnofakturpajak, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, 
        'dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrtermin, dnrterminnama, dnrterminharijatuhtempo, dnrbagianpembelian, 
        'dnrbagianpembeliankode, dnrbagianpembeliannama, kodebarang, bhpp, bjenis, bserial, bbatch, 
        'pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaprt, jmlsisarealisasi, 
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
            Filter = Filter.Replace("idbarang", "dnrd.idbarang")
            Filter = Filter.Replace("statusrealisasi", "dnrd.statusrealisasi")
            Filter = Filter.Replace("isclose", "dnrd.isclose")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_dnr_detail_v")
        'sql = "select `dnrd`.`iddnrdetail` AS `iddnrdetail`,`dnrd`.`iddnr` AS `iddnr`,`dnrd`.`idbarang` AS `idbarang`,`dnrd`.`namabarang` AS `namabarang`,`dnrd`.`tipebarang` AS `tipebarang`,`dnrd`.`jml` AS `jml`,`dnrd`.`satuan` AS `satuan`,`dnrd`.`nilaisatuan` AS `nilaisatuan`,`dnrd`.`jmlbarang` AS `jmlbarang`,`dnrd`.`satuanbarang` AS `satuanbarang`,`dnrd`.`matauang` AS `matauang`,`dnrd`.`kurs` AS `kurs`,`dnrd`.`hargafix` AS `hargafix`,`dnrd`.`idhppkhususmasuk` AS `idhppkhususmasuk`,`dnrd`.`idhppfifomasuk` AS `idhppfifomasuk`,`dnrd`.`hpp` AS `hpp`,`dnrd`.`harga` AS `harga`,`dnrd`.`diskon` AS `diskon`,`dnrd`.`jmldiskon` AS `jmldiskon`,`dnrd`.`pajak1` AS `pajak1`,`dnrd`.`jmlpajak1` AS `jmlpajak1`,`dnrd`.`pajak2` AS `pajak2`,`dnrd`.`jmlpajak2` AS `jmlpajak2`,`dnrd`.`cabang` AS `cabang`,`dnrd`.`lokasi` AS `lokasi`,`dnrd`.`gudangasal` AS `gudangasal`,`dnrd`.`gudangtransit` AS `gudangtransit`,`dnrd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`dnrd`.`rekdiskonpembelian` AS `rekdiskonpembelian`,`dnrd`.`rekhargapokok` AS `rekhargapokok`,`dnrd`.`rekreturpembelian` AS `rekreturpembelian`,`dnrd`.`costcenter` AS `costcenter`,`dnrd`.`divisi` AS `divisi`,`dnrd`.`subdivisi` AS `subdivisi`,`dnrd`.`proyek` AS `proyek`,`dnrd`.`catatan` AS `catatan`,`dnrd`.`urutan` AS `urutan`,`dnrd`.`idprdetail` AS `idprdetail`,`dnrd`.`idcsdetail` AS `idcsdetail`,`dnrd`.`idrqdetail` AS `idrqdetail`,`dnrd`.`idbsdetail` AS `idbsdetail`,`dnrd`.`idpodetail` AS `idpodetail`,`dnrd`.`idipcdetail` AS `idipcdetail`,`dnrd`.`idgrndetail` AS `idgrndetail`,`dnrd`.`idridetail` AS `idridetail`,`dnrd`.`jmlprt` AS `jmlprt`,`dnrd`.`statusprt` AS `statusprt`,`dnrd`.`jmlrealisasi` AS `jmlrealisasi`,`dnrd`.`statusrealisasi` AS `statusrealisasi`,`dnrd`.`isclose` AS `isclose`,`dnrd`.`customtext1` AS `customtext1`,`dnrd`.`customtext2` AS `customtext2`,`dnrd`.`customtext3` AS `customtext3`,`dnrd`.`customdbl1` AS `customdbl1`,`dnrd`.`customdbl2` AS `customdbl2`,`dnrd`.`customdbl3` AS `customdbl3`,`dnrd`.`customdate1` AS `customdate1`,`dnrd`.`customdate2` AS `customdate2`,`dnrd`.`customdate3` AS `customdate3`,`dnr`.`dnrnotransaksi` AS `dnrnotransaksi`,`dnr`.`dnruraian` AS `dnruraian`,`dnr`.`dnrcatatan` AS `dnrcatatan`,`dnr`.`dnrnoref` AS `dnrnoref`,`dnr`.`dnrtglnoref` AS `dnrtglnoref`,`dnr`.`dnrnofakturpajak` AS `dnrnofakturpajak`,`dnr`.`dnrsupplierkontak` AS `dnrsupplierkontak`,`dnr`.`dnr1alamat1` AS `dnr1alamat1`,`dnr`.`dnr1alamat2` AS `dnr1alamat2`,`dnr`.`dnr1alamat3` AS `dnr1alamat3`,`dnr`.`dnr2alamat1` AS `dnr2alamat1`,`dnr`.`dnr2alamat2` AS `dnr2alamat2`,`dnr`.`dnr2alamat3` AS `dnr2alamat3`,`dnr`.`dnrtermin` AS `dnrtermin`,`tr`.`trnama` AS `dnrterminnama`,`tr`.`trharijatuhtempo` AS `dnrterminharijatuhtempo`,`dnr`.`dnrbagianpembelian` AS `dnrbagianpembelian`,`c1`.`kkode` AS `dnrbagianpembeliankode`,`c1`.`knama` AS `dnrbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`dnrd`.`jmlbarang` - `dnrd`.`jmlprt`) / `dnrd`.`nilaisatuan`) AS `jmlsisaprt`,((`dnrd`.`jmlbarang` - `dnrd`.`jmlrealisasi`) / `dnrd`.`nilaisatuan`) AS `jmlsisarealisasi`, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from ((((((`m4_dnr_detail` `dnrd` left join `m4_dnr` `dnr` on((`dnrd`.`iddnr` = `dnr`.`dnrid`))) left join `m1_terms` `tr` on((`dnr`.`dnrtermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`dnr`.`dnrbagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`dnrd`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`dnrd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`dnrd`.`pajak2` = `t2`.`tkode`)))"
        sql = "select dnrd.iddnrdetail AS iddnrdetail, dnrd.iddnr AS iddnr, dnrd.idbarang AS idbarang, dnrd.namabarang AS namabarang, dnrd.tipebarang AS tipebarang, dnrd.jml AS jml, dnrd.satuan AS satuan, dnrd.nilaisatuan AS nilaisatuan, dnrd.jmlbarang AS jmlbarang, dnrd.satuanbarang AS satuanbarang, dnrd.matauang AS matauang, dnrd.kurs AS kurs, dnrd.hargafix AS hargafix, dnrd.idhppkhususmasuk AS idhppkhususmasuk, dnrd.idhppfifomasuk AS idhppfifomasuk, dnrd.hpp AS hpp, dnrd.harga AS harga, dnrd.diskon AS diskon, dnrd.jmldiskon AS jmldiskon, dnrd.pajak1 AS pajak1, dnrd.jmlpajak1 AS jmlpajak1, dnrd.pajak2 AS pajak2, dnrd.jmlpajak2 AS jmlpajak2, dnrd.cabang AS cabang, dnrd.lokasi AS lokasi, dnrd.gudangasal AS gudangasal, dnrd.gudangtransit AS gudangtransit, dnrd.gudangtujuan AS gudangtujuan, i.brekpersediaan AS rekpersediaan, dnrd.rekdiskonpembelian AS rekdiskonpembelian, dnrd.rekhargapokok AS rekhargapokok, dnrd.rekreturpembelian AS rekreturpembelian, dnrd.costcenter AS costcenter, dnrd.divisi AS divisi, dnrd.subdivisi AS subdivisi, dnrd.proyek AS proyek, dnrd.catatan AS catatan, dnrd.urutan AS urutan, dnrd.idprdetail AS idprdetail, dnrd.idcsdetail AS idcsdetail, dnrd.idrqdetail AS idrqdetail, dnrd.idbsdetail AS idbsdetail, dnrd.idpodetail AS idpodetail, dnrd.idipcdetail AS idipcdetail, dnrd.idgrndetail AS idgrndetail, dnrd.idridetail AS idridetail, dnrd.jmlprt AS jmlprt, dnrd.statusprt AS statusprt, dnrd.jmlrealisasi AS jmlrealisasi, dnrd.statusrealisasi AS statusrealisasi, dnrd.isclose AS isclose, dnrd.customtext1 AS customtext1, dnrd.customtext2 AS customtext2, dnrd.customtext3 AS customtext3, dnrd.customdbl1 AS customdbl1, dnrd.customdbl2 AS customdbl2, dnrd.customdbl3 AS customdbl3, dnrd.customdate1 AS customdate1, dnrd.customdate2 AS customdate2, dnrd.customdate3 AS customdate3, dnr.dnrnotransaksi AS dnrnotransaksi, dnr.dnruraian AS dnruraian, dnr.dnrcatatan AS dnrcatatan, dnr.dnrnoref AS dnrnoref, dnr.dnrtglnoref AS dnrtglnoref, dnr.dnrnofakturpajak AS dnrnofakturpajak, dnr.dnrsupplierkontak AS dnrsupplierkontak, dnr.dnr1alamat1 AS dnr1alamat1, dnr.dnr1alamat2 AS dnr1alamat2, dnr.dnr1alamat3 AS dnr1alamat3, dnr.dnr2alamat1 AS dnr2alamat1, dnr.dnr2alamat2 AS dnr2alamat2, dnr.dnr2alamat3 AS dnr2alamat3, dnr.dnrtermin AS dnrtermin, tr.trnama AS dnrterminnama, tr.trharijatuhtempo AS dnrterminharijatuhtempo, dnr.dnrbagianpembelian AS dnrbagianpembelian, c1.kkode AS dnrbagianpembeliankode, c1.knama AS dnrbagianpembeliannama, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bjenis AS bjenis, i.bserial AS bserial, i.bbatch AS bbatch, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, ((dnrd.jmlbarang - dnrd.jmlprt) / dnrd.nilaisatuan) AS jmlsisaprt, ((dnrd.jmlbarang - dnrd.jmlrealisasi) / dnrd.nilaisatuan) AS jmlsisarealisasi, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, ri.rinotransaksi, i.basset, ri.ricustomtext1, ri.ricustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from m4_dnr_detail dnrd join m4_dnr dnr on dnrd.iddnr = dnr.dnrid join m1_item i on dnrd.idbarang = i.bid left join m1_terms tr on dnr.dnrtermin = tr.trkode left join m1_contact c1 on dnr.dnrbagianpembelian = c1.kid left join m1_tax t1 on dnrd.pajak1 = t1.tkode  left join m1_tax t2 on dnrd.pajak2 = t2.tkode left join m4_ri_detail rid on dnrd.idridetail = rid.idridetail left join m4_ri ri on rid.idri = ri.riid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim ambilDNR As String = 2
        'AMBIL SETTING, PAKAI CABANG ATAU TIDAK
        Dim rsSetting As String = F_getSetting(4, "options", "NoTransaksiPRT")
        If Len(rsSetting) > 0 Then ambilDNR = rsSetting

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("iddnrdetail"), 0), sptField,
                     FxDB(dr("iddnr"), 0), sptField,
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
                     FxDB(dr("dnrnotransaksi"), ""), sptField,
                     FxDB(dr("dnruraian"), ""), sptField,
                     FxDB(dr("dnrcatatan"), ""), sptField,
                     FxDB(dr("dnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("dnrtglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("dnrnofakturpajak"), ""), sptField,
                     FxDB(dr("dnrsupplierkontak"), ""), sptField,
                     FxDB(dr("dnr1alamat1"), ""), sptField,
                     FxDB(dr("dnr1alamat2"), ""), sptField,
                     FxDB(dr("dnr1alamat3"), ""), sptField,
                     FxDB(dr("dnr2alamat1"), ""), sptField,
                     FxDB(dr("dnr2alamat2"), ""), sptField,
                     FxDB(dr("dnr2alamat3"), ""), sptField,
                     FxDB(dr("dnrtermin"), ""), sptField,
                     FxDB(dr("dnrterminnama"), ""), sptField,
                     FxDB(dr("dnrterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("dnrbagianpembelian"), 0), sptField,
                     FxDB(dr("dnrbagianpembeliankode"), ""), sptField,
                     FxDB(dr("dnrbagianpembeliannama"), ""), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisaprt"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     Replace(FxDB(IIf(ambilDNR = 2, dr("dnrnotransaksi"), dr("rinotransaksi")), ""), "DO", "NR"), sptField,
                     FxDB(dr("ricustomtext1"), ""), sptField,
                     FxDB(dr("ricustomtext2"), ""), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, dnrnotransaksi, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrnofakturpajak, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrtermin, dnrterminnama, dnrterminharijatuhtempo, dnrbagianpembelian, dnrbagianpembeliankode, dnrbagianpembeliannama, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaprt, jmlsisarealisasi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, ricustomtext1, ricustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama"))

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

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingRI As String, ByVal ftOutstandingRI As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftExistBatch As String, ByVal ftBatch As String, ByVal ftExistSerial As String, ByVal ftSerial As String, ByVal gudangBatchSerial As String, ByVal ftRI As String, ByRef termasukPajak As String) As String
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
            sql = "SELECT ri.rinotransaksi as notransaksi, ri.rihargatermasukpajak as termasukpajak, (CASE ri.rihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m4_ri_detail rid JOIN m4_ri ri ON rid.idri = ri.riid WHERE " & ftRI & " GROUP BY ri.rihargatermasukpajak"
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 1 Then
                errmessage = "Include Tax Price can't join with Exclude Tax Price as one Transaction"
                For Each dr1 As DataRow In dtval.Rows
                    errmessage &= ", " & dr1("notransaksi") & " " & dr1("termasukpajaknama")
                Next
                GoTo selesai

            ElseIf dtval.Rows.Count = 1 Then
                If Len(dtval.Rows(0)("termasukpajak")) > 0 Then
                    termasukPajak = Integer.Parse(dtval.Rows(0)("termasukpajak"))
                End If

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

selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M4_DnrSimpanOld(ByVal param As String) As String
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
        'dnrid(0) As Integer, dnrcabang(1) As String, dnrlokasi(2) As String, dnrgudang(3) As String, dnrasalbarang(4) As String, 
        'dnrasalbarangkategori(5) As Integer, dnrjenispembelian(6) As String, dnrjenispembeliankategori(7) As Integer, dnrcarabayar(8) As Integer, dnrsumber(9) As String, 
        'dnrautonotransaksi(10) As Integer, dnrnotransaksi(11) As String, dnrtgl(12) As Date, dnrkodepa(13) As Integer, dnrsupplier(14) As Integer, 
        'dnrsupplierkontak(15) As String, dnr1alamat1(16) As String, dnr1alamat2(17) As String, dnr1alamat3(18) As String, dnr2alamat1(19) As String, 
        'dnr2alamat2(20) As String, dnr2alamat3(21) As String, dnrbagianpembelian(22) As Integer, dnrtermin(23) As String, dnrtgljatuhtempo(24) As Date, 
        'dnruraian(25) As String, dnrcatatan(26) As String, dnrnoref(27) As String, dnrtglnoref(28) As Date, dnrtglpenutupan(29) As Date, 
        'dnrmatauang(30) As String, dnrkurs(31) As Double, dnrhargatermasukpajak(32) As Integer, dnrtotal(33) As Double, dnrdiskonpersen(34) As String, 
        'dnrjmldiskon(35) As Double, dnrtotalpajak1detail(36) As Double, dnrtotalpajak2detail(37) As Double, dnrbiayalainpersen(38) As String, dnrbiayalain(39) As Double, 
        'dnrtotaltransaksi(40) As Double, dnrjmlbayar(41) As Double, dnrstatuslunas(42) As Integer, dnrtgllunas(43) As Date, dnrnofakturpajak(44) As String, 
        'dnrsdhbayarpajak(45) As Integer, dnrtglbayarpajak(46) As Date, dnrrekdiskon(47) As String, dnrrekpajak1(48) As String, dnrrekpajak2(49) As String, 
        'dnrrekbiayalain(50) As String, dnrrekbayar(51) As String, dnridpr(52) As Integer, dnridcs(53) As Integer, dnridrq(54) As Integer, 
        'dnridbs(55) As Integer, dnridpo(56) As Integer, dnridipc(57) As Integer, dnridgrn(58) As Integer, dnridri(59) As Integer, 
        'dnrstatusprt(60) As Integer, dnrstatus(61) As Integer, dnrstatussebelumnya(62) As Integer, dnrjmlrevisi(63) As Integer, dnrcetakanke(64) As Integer, 
        'dnrinputuser(65) As Integer, dnrinputtgl(66) As DateTime, dnrmodifikasiuser(67) As Integer, dnrmodifikasitgl(68) As DateTime, dnrposting(69) As Integer, 
        'dnrtutupperiode(70) As Integer, dnrisclose(71) As Integer, dnrcustomtext1(72) As String, dnrcustomtext2(73) As String, dnrcustomtext3(74) As String, 
        'dnrcustomtext4(75) As String, dnrcustomtext5(76) As String, dnrcustomint1(77) As Integer, dnrcustomint2(78) As Integer, dnrcustomint3(79) As Integer, 
        'dnrcustomdbl1(80) As Double, dnrcustomdbl2(81) As Double, dnrcustomdbl3(82) As Double, dnrcustomdate1(83) As Date, dnrcustomdate2(84) As Date, 
        'dnrcustomdate3(85) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'dnrid, dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, 
        'dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, 
        'dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, 
        'dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, 
        'dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, 
        'dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, 
        'dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, 
        'dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, 
        'dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatus, dnrstatussebelumnya, 
        'dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, 
        'dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, 
        'dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, 
        'dnrcustomdate2, dnrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 86) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'dnrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "dnrid required numeric." : GoTo selesai
        End If
        'dnrasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "dnrasalbarangkategori required numeric." : GoTo selesai
        End If
        'dnrjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "dnrjenispembeliankategori required numeric." : GoTo selesai
        End If
        'dnrcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "dnrcarabayar required numeric." : GoTo selesai
        End If
        'dnrautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "dnrautonotransaksi required numeric." : GoTo selesai
        End If
        'dnrtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "dnrtgl required date." : GoTo selesai
        End If
        'dnrkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "dnrkodepa required numeric." : GoTo selesai
        End If
        'dnrsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "dnrsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "dnrsupplier can't be empty." : GoTo selesai
        End If
        'dnrbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "dnrbagianpembelian required numeric." : GoTo selesai
        End If
        'dnrtgljatuhtempo(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "dnrtgljatuhtempo required date." : GoTo selesai
        End If
        'dnrtglnoref(28) As Date
        If (IsDate(dataUtama(28)) = False) Then
            result(2) = "dnrtglnoref required date." : GoTo selesai
        End If
        'dnrtglpenutupan(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "dnrtglpenutupan required date." : GoTo selesai
        End If
        'dnrkurs(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "dnrkurs required numeric." : GoTo selesai
        End If
        'dnrhargatermasukpajak(32) As Integer
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "dnrhargatermasukpajak required numeric." : GoTo selesai
        End If
        'dnrtotal(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "dnrtotal required numeric." : GoTo selesai
        End If
        'dnrjmldiskon(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "dnrjmldiskon required numeric." : GoTo selesai
        End If
        'dnrtotalpajak1detail(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "dnrtotalpajak1detail required numeric." : GoTo selesai
        End If
        'dnrtotalpajak2detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "dnrtotalpajak2detail required numeric." : GoTo selesai
        End If
        'dnrbiayalain(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "dnrbiayalain required numeric." : GoTo selesai
        End If
        'dnrtotaltransaksi(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "dnrtotaltransaksi required numeric." : GoTo selesai
        End If
        'dnrjmlbayar(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "dnrjmlbayar required numeric." : GoTo selesai
        End If
        'dnrstatuslunas(42) As Integer
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "dnrstatuslunas required numeric." : GoTo selesai
        End If
        'dnrtgllunas(43) As Date
        If (IsDate(dataUtama(43)) = False) Then
            result(2) = "dnrtgllunas required date." : GoTo selesai
        End If
        'dnrsdhbayarpajak(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "dnrsdhbayarpajak required numeric." : GoTo selesai
        End If
        'dnrtglbayarpajak(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "dnrtglbayarpajak required date." : GoTo selesai
        End If
        'dnridpr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "dnridpr required numeric." : GoTo selesai
        End If
        'dnridcs(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "dnridcs required numeric." : GoTo selesai
        End If
        'dnridrq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "dnridrq required numeric." : GoTo selesai
        End If
        'dnridbs(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "dnridbs required numeric." : GoTo selesai
        End If
        'dnridpo(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "dnridpo required numeric." : GoTo selesai
        End If
        'dnridipc(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "dnridipc required numeric." : GoTo selesai
        End If
        'dnridgrn(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "dnridgrn required numeric." : GoTo selesai
        End If
        'dnridri(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "dnridri required numeric." : GoTo selesai
        End If
        'dnrstatusprt(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "dnrstatusprt required numeric." : GoTo selesai
        End If
        'dnrstatus(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "dnrstatus required numeric." : GoTo selesai
        End If
        'dnrstatussebelumnya(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "dnrstatussebelumnya required numeric." : GoTo selesai
        End If
        'dnrjmlrevisi(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "dnrjmlrevisi required numeric." : GoTo selesai
        End If
        'dnrcetakanke(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "dnrcetakanke required numeric." : GoTo selesai
        End If
        'dnrinputuser(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "dnrinputuser required numeric." : GoTo selesai
        End If
        'dnrinputtgl(66) As DateTime
        If (IsDate(dataUtama(66)) = False) Then
            result(2) = "dnrinputtgl required date." : GoTo selesai
        End If
        'dnrmodifikasiuser(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "dnrmodifikasiuser required numeric." : GoTo selesai
        End If
        'dnrmodifikasitgl(68) As DateTime
        If (IsDate(dataUtama(68)) = False) Then
            result(2) = "dnrmodifikasitgl required date." : GoTo selesai
        End If
        'dnrposting(69) As Integer
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "dnrposting required numeric." : GoTo selesai
        End If
        'dnrtutupperiode(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "dnrtutupperiode required numeric." : GoTo selesai
        End If
        'dnrisclose(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "dnrisclose required numeric." : GoTo selesai
        End If
        'dnrcustomint1(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "dnrcustomint1 required numeric." : GoTo selesai
        End If
        'dnrcustomint2(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "dnrcustomint2 required numeric." : GoTo selesai
        End If
        'dnrcustomint3(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "dnrcustomint3 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl1(80) As Double
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "dnrcustomdbl1 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl2(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "dnrcustomdbl2 required numeric." : GoTo selesai
        End If
        'dnrcustomdbl3(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "dnrcustomdbl3 required numeric." : GoTo selesai
        End If
        'dnrcustomdate1(83) As Date
        If (IsDate(dataUtama(83)) = False) Then
            result(2) = "dnrcustomdate1 required date." : GoTo selesai
        End If
        'dnrcustomdate2(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "dnrcustomdate2 required date." : GoTo selesai
        End If
        'dnrcustomdate3(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "dnrcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'dnrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "dnrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "dnrcabang should not be more than 25 character." : GoTo selesai
        End If

        'dnrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "dnrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "dnrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'dnrgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "dnrgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "dnrgudang should not be more than 25 character." : GoTo selesai
        End If

        'dnrsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "dnrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "dnrsumber should not be more than 10 character." : GoTo selesai
        End If

        'dnrnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "dnrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "dnrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'dnrtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "dnrtgl can't be empty" : GoTo selesai
        End If

        'dnrtgljatuhtempo(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "dnrtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'dnrtglnoref(28) As Date
        If Len(dataUtama(28)) = 0 Then
            result(2) = "dnrtglnoref can't be empty" : GoTo selesai
        End If

        'dnrtglpenutupan(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "dnrtglpenutupan can't be empty" : GoTo selesai
        End If

        'dnrmatauang(30) As String
        If Len(dataUtama(30)) = 0 Then
            result(2) = "dnrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(30)) > 25 Then
            result(2) = "dnrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'dnrkurs(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "dnrkurs can't be empty" : GoTo selesai
        End If

        'dnrtotal(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "dnrtotal can't be empty" : GoTo selesai
        End If

        'dnrdiskonpersen(34) As String
        If Len(dataUtama(34)) = 0 Then
            result(2) = "dnrdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(34)) > 25 Then
            result(2) = "dnrdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'dnrjmldiskon(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "dnrjmldiskon can't be empty" : GoTo selesai
        End If

        'dnrtotalpajak1detail(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "dnrtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'dnrtotalpajak2detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "dnrtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'dnrbiayalainpersen(38) As String
        If Len(dataUtama(38)) = 0 Then
            result(2) = "dnrbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(38)) > 25 Then
            result(2) = "dnrbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'dnrbiayalain(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "dnrbiayalain can't be empty" : GoTo selesai
        End If

        'dnrtotaltransaksi(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "dnrtotaltransaksi can't be empty" : GoTo selesai
        End If

        'dnrjmlbayar(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "dnrjmlbayar can't be empty" : GoTo selesai
        End If

        'dnrtgllunas(43) As Date
        If Len(dataUtama(43)) = 0 Then
            result(2) = "dnrtgllunas can't be empty" : GoTo selesai
        End If

        'dnrtglbayarpajak(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "dnrtglbayarpajak can't be empty" : GoTo selesai
        End If

        'dnrinputtgl(66) As DateTime
        If Len(dataUtama(66)) = 0 Then
            result(2) = "dnrinputtgl can't be empty" : GoTo selesai
        End If

        'dnrmodifikasitgl(68) As DateTime
        If Len(dataUtama(68)) = 0 Then
            result(2) = "dnrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl1(80) As Double
        If Len(dataUtama(80)) = 0 Then
            result(2) = "dnrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl2(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "dnrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'dnrcustomdbl3(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "dnrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate1(83) As Date
        If Len(dataUtama(83)) = 0 Then
            result(2) = "dnrcustomdate1 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate2(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "dnrcustomdate2 can't be empty" : GoTo selesai
        End If

        'dnrcustomdate3(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "dnrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "dnrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnridpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridpo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnridri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "dnrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "dnrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "dnrid~dnrcabang~dnrlokasi~dnrgudang~dnrasalbarang~dnrasalbarangkategori~dnrjenispembelian~dnrjenispembeliankategori~dnrcarabayar~dnrsumber~dnrautonotransaksi~dnrnotransaksi~dnrtgl~dnrkodepa~dnrsupplier~dnrsupplierkontak~dnr1alamat1~dnr1alamat2~dnr1alamat3~dnr2alamat1~dnr2alamat2~dnr2alamat3~dnrbagianpembelian~dnrtermin~dnrtgljatuhtempo~dnruraian~dnrcatatan~dnrnoref~dnrtglnoref~dnrtglpenutupan~dnrmatauang~dnrkurs~dnrhargatermasukpajak~dnrtotal~dnrdiskonpersen~dnrjmldiskon~dnrtotalpajak1detail~dnrtotalpajak2detail~dnrbiayalainpersen~dnrbiayalain~dnrtotaltransaksi~dnrjmlbayar~dnrstatuslunas~dnrtgllunas~dnrnofakturpajak~dnrsdhbayarpajak~dnrtglbayarpajak~dnrrekdiskon~dnrrekpajak1~dnrrekpajak2~dnrrekbiayalain~dnrrekbayar~dnridpr~dnridcs~dnridrq~dnridbs~dnridpo~dnridipc~dnridgrn~dnridri~dnrstatusprt~dnrstatus~dnrstatussebelumnya~dnrjmlrevisi~dnrcetakanke~dnrinputuser~dnrinputtgl~dnrmodifikasiuser~dnrmodifikasitgl~dnrposting~dnrtutupperiode~dnrisclose~dnrcustomtext1~dnrcustomtext2~dnrcustomtext3~dnrcustomtext4~dnrcustomtext5~dnrcustomint1~dnrcustomint2~dnrcustomint3~dnrcustomdbl1~dnrcustomdbl2~dnrcustomdbl3~dnrcustomdate1~dnrcustomdate2~dnrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'iddnrdetail(0) As Integer, iddnr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, hargafix(12) As Integer, idhppkhususmasuk(13) As Integer, idhppfifomasuk(14) As Integer, 
        'hpp(15) As Double, harga(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekdiskonpembelian(29) As String, 
        'rekhargapokok(30) As String, rekreturpembelian(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idprdetail(38) As Integer, idcsdetail(39) As Integer, 
        'idrqdetail(40) As Integer, idbsdetail(41) As Integer, idpodetail(42) As Integer, idipcdetail(43) As Integer, idgrndetail(44) As Integer, 
        'idridetail(45) As Integer, jmlprt(46) As Double, statusprt(47) As Integer, isclose(48) As Integer, customtext1(49) As String, 
        'customtext2(50) As String, customtext3(51) As String, customdbl1(52) As Double, customdbl2(53) As Double, customdbl3(54) As Double, 
        'customdate1(55) As Date, customdate2(56) As Date, customdate3(57) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, 
        'idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, 
        'idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, isclose, 
        'customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, 
        'customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "iddnrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "iddnr", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingRI As String = "", ftOutstandingRI As String = "", updNilaiRI As String = "", updFilterRI As String = ""
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = "", updStokIn As String = "", gudangIn As String = ""
        Dim idbarang As Integer = 0, idridetail As Integer = 0, jmlbarang As Double = 0

        'FILTER RI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftRI As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 58) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'iddnrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - iddnrdetail required numeric." : GoTo selesai
            End If
            'iddnr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - iddnr required numeric." : GoTo selesai
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
            'jmlprt(46) As Double
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - jmlprt required numeric." : GoTo selesai
            End If
            'statusprt(47) As Integer
            If (IsNumeric(dataRowDetail(47)) = False) Then
                result(2) = "Row : " & i & " - statusprt required numeric." : GoTo selesai
            End If
            'isclose(48) As Integer
            If (IsNumeric(dataRowDetail(48)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
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
            'customdate1(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(56) As Date
            If (IsDate(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(57) As Date
            If (IsDate(dataRowDetail(57)) = False) Then
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
                'Else
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(16) As Double, diskon(17) As String
                '    dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(16)), FixQuotes(dataRowDetail(17).ToString))
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

            'jmlprt(46) As Double
            If Len(dataRowDetail(46)) = 0 Then
                result(2) = "Row : " & i & " - jmlprt can't be empty" : GoTo selesai
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

            'customdate1(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(56) As Date
            If Len(dataRowDetail(56)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(57) As Date
            If Len(dataRowDetail(57)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "iddnrdetail~iddnr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~idhppkhususmasuk~idhppfifomasuk~hpp~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekdiskonpembelian~rekhargapokok~rekreturpembelian~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~idpodetail~idipcdetail~idgrndetail~idridetail~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangasal(25) As String      , gudangtransit(26) As String   , idridetail(45) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangOut = dataRowDetail(25) : gudangIn = dataRowDetail(26) : idridetail = dataRowDetail(45)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'ValidasiSimpan
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

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangasal='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("dnrtgl")), AsFormatTanggal(drutama("dnrtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("dnrstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 0)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    ''ValidasiHppI
                    'Dim rsValidasi As String = ValidasiHppI(dtdetail, ftBarang)
                    'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingRI, ftOutstandingRI, ftExistStok, ftStok, ftExistBatch, ftBatch, ftExistSerial, ftSerial, "gudangasal", ftRI, drutama("dnrhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("dnrtermin").ToString, AsFormatTanggal(drutama("dnrtgl")), "dnrtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("dnrtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("dnrtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("dnrtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("dnrtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("dnrhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("dnrtotaltransaksi") = Double.Parse(drutama("dnrtotal")) - Double.Parse(drutama("dnrjmldiskon")) + Double.Parse(drutama("dnrtotalpajak1detail")) + Double.Parse(drutama("dnrtotalpajak2detail")) + Double.Parse(drutama("dnrbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("dnrtotaltransaksi") = Double.Parse(drutama("dnrtotal")) - Double.Parse(drutama("dnrjmldiskon")) + Double.Parse(drutama("dnrbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("dnrid")
                    notransaksi = drutama("dnrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(dnrid), dnrnotransaksi FROM M4_dnr WHERE dnrid='" & result(4) & "' AND dnrstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(dnrid) FROM m4_dnr WHERE dnrnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_dnr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m4_Dnr_HistorySimpan("" & paramSplit(0) & "★M4_Dnr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("dnrsumber")) & "▼" & FixQuotes(drutama("dnrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Dnr set dnrcabang  = '" & FixQuotes(drutama("dnrcabang")) & "', dnrlokasi  = '" & FixQuotes(drutama("dnrlokasi")) & "', dnrgudang  = '" & FixQuotes(drutama("dnrgudang")) & "', dnrasalbarang  = '" & FixQuotes(drutama("dnrasalbarang")) & "', dnrasalbarangkategori  = " & drutama("dnrasalbarangkategori") & ", dnrjenispembelian  = '" & FixQuotes(drutama("dnrjenispembelian")) & "', dnrjenispembeliankategori  = " & drutama("dnrjenispembeliankategori") & ", dnrcarabayar  = " & drutama("dnrcarabayar") & ", dnrsumber  = '" & FixQuotes(drutama("dnrsumber")) & "', dnrautonotransaksi  = " & drutama("dnrautonotransaksi") & ", dnrnotransaksi  = '" & FixQuotes(notransaksi) & "', dnrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', dnrkodepa  = " & drutama("dnrkodepa") & ", dnrsupplier  = " & drutama("dnrsupplier") & ", dnrsupplierkontak  = '" & FixQuotes(drutama("dnrsupplierkontak")) & "', dnr1alamat1  = '" & FixQuotes(drutama("dnr1alamat1")) & "', dnr1alamat2  = '" & FixQuotes(drutama("dnr1alamat2")) & "', dnr1alamat3  = '" & FixQuotes(drutama("dnr1alamat3")) & "', dnr2alamat1  = '" & FixQuotes(drutama("dnr2alamat1")) & "', dnr2alamat2  = '" & FixQuotes(drutama("dnr2alamat2")) & "', dnr2alamat3  = '" & FixQuotes(drutama("dnr2alamat3")) & "', dnrbagianpembelian  = " & drutama("dnrbagianpembelian") & ", dnrtermin  = '" & FixQuotes(drutama("dnrtermin")) & "', dnrtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgljatuhtempo"))) & "', dnruraian  = '" & FixQuotes(drutama("dnruraian")) & "', dnrcatatan  = '" & FixQuotes(drutama("dnrcatatan")) & "', dnrnoref  = '" & FixQuotes(drutama("dnrnoref")) & "', dnrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglnoref"))) & "', dnrtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglpenutupan"))) & "', dnrmatauang  = '" & FixQuotes(drutama("dnrmatauang")) & "', dnrkurs  = '" & FixDouble(drutama("dnrkurs")) & "', dnrhargatermasukpajak  = " & drutama("dnrhargatermasukpajak") & ", dnrtotal  = '" & FixDouble(drutama("dnrtotal")) & "', dnrdiskonpersen  = '" & FixQuotes(drutama("dnrdiskonpersen")) & "', dnrjmldiskon  = '" & FixDouble(drutama("dnrjmldiskon")) & "', dnrtotalpajak1detail  = '" & FixDouble(drutama("dnrtotalpajak1detail")) & "', dnrtotalpajak2detail  = '" & FixDouble(drutama("dnrtotalpajak2detail")) & "', dnrbiayalainpersen  = '" & FixQuotes(drutama("dnrbiayalainpersen")) & "', dnrbiayalain  = '" & FixDouble(drutama("dnrbiayalain")) & "', dnrtotaltransaksi  = '" & FixDouble(drutama("dnrtotaltransaksi")) & "', dnrjmlbayar  = '" & FixDouble(drutama("dnrjmlbayar")) & "', dnrstatuslunas  = " & drutama("dnrstatuslunas") & ", dnrtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtgllunas"))) & "', dnrnofakturpajak  = '" & FixQuotes(drutama("dnrnofakturpajak")) & "', dnrsdhbayarpajak  = " & drutama("dnrsdhbayarpajak") & ", dnrtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("dnrtglbayarpajak"))) & "', dnrrekdiskon  = '" & FixQuotes(drutama("dnrrekdiskon")) & "', dnrrekpajak1  = '" & FixQuotes(drutama("dnrrekpajak1")) & "', dnrrekpajak2  = '" & FixQuotes(drutama("dnrrekpajak2")) & "', dnrrekbiayalain  = '" & FixQuotes(drutama("dnrrekbiayalain")) & "', dnrrekbayar  = '" & FixQuotes(drutama("dnrrekbayar")) & "', dnridpr  = " & drutama("dnridpr") & ", dnridcs  = " & drutama("dnridcs") & ", dnridrq  = " & drutama("dnridrq") & ", dnridbs  = " & drutama("dnridbs") & ", dnridpo  = " & drutama("dnridpo") & ", dnridipc  = " & drutama("dnridipc") & ", dnridgrn  = " & drutama("dnridgrn") & ", dnridri  = " & drutama("dnridri") & ", dnrstatusprt  = " & drutama("dnrstatusprt") & ", dnrstatus  = " & drutama("dnrstatus") & ", dnrstatussebelumnya  = " & drutama("dnrstatussebelumnya") & ", dnrjmlrevisi  = dnrjmlrevisi+1, dnrcetakanke  = " & drutama("dnrcetakanke") & ", dnrmodifikasiuser  = " & drutama("dnrmodifikasiuser") & ", dnrmodifikasitgl  = NOW(), dnrposting  = 0, dnrtutupperiode  = " & drutama("dnrtutupperiode") & ", dnrcustomtext1  = '" & FixQuotes(drutama("dnrcustomtext1")) & "', dnrcustomtext2  = '" & FixQuotes(drutama("dnrcustomtext2")) & "', dnrcustomtext3  = '" & FixQuotes(drutama("dnrcustomtext3")) & "', dnrcustomtext4  = '" & FixQuotes(drutama("dnrcustomtext4")) & "', dnrcustomtext5  = '" & FixQuotes(drutama("dnrcustomtext5")) & "', dnrcustomint1  = " & drutama("dnrcustomint1") & ", dnrcustomint2  = " & drutama("dnrcustomint2") & ", dnrcustomint3  = " & drutama("dnrcustomint3") & ", dnrcustomdbl1  = '" & FixDouble(drutama("dnrcustomdbl1")) & "', dnrcustomdbl2  = '" & FixDouble(drutama("dnrcustomdbl2")) & "', dnrcustomdbl3  = '" & FixDouble(drutama("dnrcustomdbl3")) & "', dnrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate1"))) & "', dnrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate2"))) & "', dnrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate3"))) & "' where dnrid = '" & drutama("dnrid") & "'"
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

                    If drutama("dnrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("dnrcabang"), drutama("dnrlokasi"), drutama("dnrsumber"), drutama("dnrtgl"))
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
                        notransaksi = drutama("dnrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(dnrid) FROM m4_dnr WHERE dnrnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Dnr (dnrcabang, dnrlokasi, dnrgudang, dnrasalbarang, dnrasalbarangkategori, dnrjenispembelian, dnrjenispembeliankategori, dnrcarabayar, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl, dnrkodepa, dnrsupplier, dnrsupplierkontak, dnr1alamat1, dnr1alamat2, dnr1alamat3, dnr2alamat1, dnr2alamat2, dnr2alamat3, dnrbagianpembelian, dnrtermin, dnrtgljatuhtempo, dnruraian, dnrcatatan, dnrnoref, dnrtglnoref, dnrtglpenutupan, dnrmatauang, dnrkurs, dnrhargatermasukpajak, dnrtotal, dnrdiskonpersen, dnrjmldiskon, dnrtotalpajak1detail, dnrtotalpajak2detail, dnrbiayalainpersen, dnrbiayalain, dnrtotaltransaksi, dnrjmlbayar, dnrstatuslunas, dnrtgllunas, dnrnofakturpajak, dnrsdhbayarpajak, dnrtglbayarpajak, dnrrekdiskon, dnrrekpajak1, dnrrekpajak2, dnrrekbiayalain, dnrrekbayar, dnridpr, dnridcs, dnridrq, dnridbs, dnridpo, dnridipc, dnridgrn, dnridri, dnrstatusprt, dnrstatus, dnrstatussebelumnya, dnrjmlrevisi, dnrcetakanke, dnrinputuser, dnrinputtgl, dnrmodifikasiuser, dnrmodifikasitgl, dnrposting, dnrtutupperiode, dnrisclose, dnrcustomtext1, dnrcustomtext2, dnrcustomtext3, dnrcustomtext4, dnrcustomtext5, dnrcustomint1, dnrcustomint2, dnrcustomint3, dnrcustomdbl1, dnrcustomdbl2, dnrcustomdbl3, dnrcustomdate1, dnrcustomdate2, dnrcustomdate3) values('" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(drutama("dnrgudang")) & "', '" & FixQuotes(drutama("dnrasalbarang")) & "', " & drutama("dnrasalbarangkategori") & ", '" & FixQuotes(drutama("dnrjenispembelian")) & "', " & drutama("dnrjenispembeliankategori") & ", " & drutama("dnrcarabayar") & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & drutama("dnrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrkodepa") & ", " & drutama("dnrsupplier") & ", '" & FixQuotes(drutama("dnrsupplierkontak")) & "', '" & FixQuotes(drutama("dnr1alamat1")) & "', '" & FixQuotes(drutama("dnr1alamat2")) & "', '" & FixQuotes(drutama("dnr1alamat3")) & "', '" & FixQuotes(drutama("dnr2alamat1")) & "', '" & FixQuotes(drutama("dnr2alamat2")) & "', '" & FixQuotes(drutama("dnr2alamat3")) & "', " & drutama("dnrbagianpembelian") & ", '" & FixQuotes(drutama("dnrtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgljatuhtempo"))) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(drutama("dnrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtglpenutupan"))) & "', '" & FixQuotes(drutama("dnrmatauang")) & "', '" & FixDouble(drutama("dnrkurs")) & "', " & drutama("dnrhargatermasukpajak") & ", '" & FixDouble(drutama("dnrtotal")) & "', '" & FixQuotes(drutama("dnrdiskonpersen")) & "', '" & FixDouble(drutama("dnrjmldiskon")) & "', '" & FixDouble(drutama("dnrtotalpajak1detail")) & "', '" & FixDouble(drutama("dnrtotalpajak2detail")) & "', '" & FixQuotes(drutama("dnrbiayalainpersen")) & "', '" & FixDouble(drutama("dnrbiayalain")) & "', '" & FixDouble(drutama("dnrtotaltransaksi")) & "', '" & FixDouble(drutama("dnrjmlbayar")) & "', " & drutama("dnrstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("dnrtgllunas"))) & "', '" & FixQuotes(drutama("dnrnofakturpajak")) & "', " & drutama("dnrsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("dnrtglbayarpajak"))) & "', '" & FixQuotes(drutama("dnrrekdiskon")) & "', '" & FixQuotes(drutama("dnrrekpajak1")) & "', '" & FixQuotes(drutama("dnrrekpajak2")) & "', '" & FixQuotes(drutama("dnrrekbiayalain")) & "', '" & FixQuotes(drutama("dnrrekbayar")) & "', " & drutama("dnridpr") & ", " & drutama("dnridcs") & ", " & drutama("dnridrq") & ", " & drutama("dnridbs") & ", " & drutama("dnridpo") & ", " & drutama("dnridipc") & ", " & drutama("dnridgrn") & ", " & drutama("dnridri") & ", " & drutama("dnrstatusprt") & ", " & drutama("dnrstatus") & ", " & drutama("dnrstatussebelumnya") & ", " & drutama("dnrjmlrevisi") & ", " & drutama("dnrcetakanke") & ", " & drutama("dnrinputuser") & ", NOW(), " & drutama("dnrmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("dnrtutupperiode") & ", " & drutama("dnrisclose") & ", '" & FixQuotes(drutama("dnrcustomtext1")) & "', '" & FixQuotes(drutama("dnrcustomtext2")) & "', '" & FixQuotes(drutama("dnrcustomtext3")) & "', '" & FixQuotes(drutama("dnrcustomtext4")) & "', '" & FixQuotes(drutama("dnrcustomtext5")) & "', " & drutama("dnrcustomint1") & ", " & drutama("dnrcustomint2") & ", " & drutama("dnrcustomint3") & ", '" & FixDouble(drutama("dnrcustomdbl1")) & "', '" & FixDouble(drutama("dnrcustomdbl2")) & "', '" & FixDouble(drutama("dnrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select dnrid from M4_dnr where dnrnotransaksi='" & notransaksi & "' AND dnrinputuser= '" & userid & "' order by dnrmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Dnr_Detail where iddnr = '" & result(4) & "'"
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
                    Dim dtRI As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("dnrmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI RI ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m4_ri_detail WHERE idridetail = '" & FixDouble(dr1("idridetail")) & "'"
                        dtRI = AsDataTableAmbilDariDB(sql)
                        If dtRI.Rows.Count > 0 Then
                            'SET HARGA - ambil dari RI
                            dr1("harga") = Double.Parse(dtRI.Rows(0)("harga"))

                            'SET DISKON - ambil dari RI
                            dr1("diskon") = dtRI.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari RI
                            dr1("pajak1") = dtRI.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari RI = (jmlpajakri / jmlri) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtRI.Rows(0)("jmlpajak1")) / Double.Parse(dtRI.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari RI
                            dr1("pajak2") = dtRI.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari RI = (jmlpajakri / jmlri) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtRI.Rows(0)("jmlpajak2")) / Double.Parse(dtRI.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI RI -----------------------------------------------


                        'QUERY INSERT DETAIL
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("iddnrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", " & dr1("idhppkhususmasuk") & ", " & dr1("idhppfifomasuk") & ", '" & FixDouble(dr1("hpp")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekdiskonpembelian")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekreturpembelian")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", " & dr1("idpodetail") & ", " & dr1("idipcdetail") & ", " & dr1("idgrndetail") & ", " & dr1("idridetail") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Dnr_Detail(iddnrdetail, iddnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, idhppkhususmasuk, idhppfifomasuk, hpp, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekdiskonpembelian, rekhargapokok, rekreturpembelian, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, idpodetail, idipcdetail, idgrndetail, idridetail, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'DNR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'DNR'"
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


                If drutama("dnrstatus") = 2 Then
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idri FROM m4_ri_detail WHERE " & updFilterRI & " GROUP BY idri")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_ri_detail WHERE " & ftDetail & " GROUP BY idri")
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
                    'END OF UPDATE OUTSTANDING TRANSAKSI ============================================


                    'AMBIL GUDANG TRANSIT DARI SETTING ==============================================
                    Dim SetGudang As String = ""
                    'GUDANG SETTING TRANSIT DIGUNAKAN UNTUK NO SERIAL DAN BATCH MASUK
                    'MISAL : GUDANG ASAL 'A', MAKA :
                    '-- NO SERIAL DAN BATCH GUDANG 'A' BERKURANG
                    '-- NO SERIAL DAN BATCH GUDANG TRANSIT BERTAMBAH
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 3 AND sgrup = 'defaultgudang' AND skode = 'GudangTransit'"
                    Dim dtSetGudang As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtSetGudang.Rows.Count > 0 Then
                        SetGudang = dtSetGudang.Rows(0)("snilai")
                    Else
                        result(2) = "Setting for Transit Warehouse not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF AMBIL GUDANG TRANSIT DARI SETTING =======================================


                    'INSERT NO BATCH ================================================================
                    If dtbatch.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtbatch.Rows
                            'QUERY INSERT NO BATCH OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping             nboid,            nboidbatchin,                           nbogudang,                  nboidbarang,                           nbokode,                             nbosumber,            nboidtransaksi,                     nbosatuan,                         nbojmlkeluar,       nboisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nbtidbatchin") & ", '" & FixQuotes(dr1("nbtgudang")) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO BATCH IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping        nbiidbatchin,                nbigudang,                nbiidbarang,                           nbikode,                             nbisumber,            nbiidtransaksi,                     nbisatuan,                 nbijmlmasuk,       nbijmlkeluar,                  nbijmlsisa, nbiisclose,                     nbicustomtext1,                             nbicustomtext2,                             nbicustomtext3,                             nbicustomdbl1,                             nbicustomdbl2,                             nbicustomdbl3,                                             nbicustomdate1,                                              nbicustomdate2,                                              nbicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nbtidbarang") & ", '" & FixQuotes(dr1("nbtkode")) & "', '" & FixQuotes(dr1("nbtsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nbtsatuan")) & "', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixDouble(dr1("nbtjml")) & "', '0', '" & FixQuotes(dr1("nbtcustomtext1")) & "', '" & FixQuotes(dr1("nbtcustomtext2")) & "', '" & FixQuotes(dr1("nbtcustomtext3")) & "', '" & FixDouble(dr1("nbtcustomdbl1")) & "', '" & FixDouble(dr1("nbtcustomdbl2")) & "', '" & FixDouble(dr1("nbtcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & "')")
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

                        'INSERT NO BATCH IN MASUK ----------------------------
                        sql = "Insert into M1_No_Batch_In(nbiidbatchin, nbigudang, nbiidbarang, nbikode, nbisumber, nbiidtransaksi, nbisatuan, nbijmlmasuk, nbijmlkeluar, nbijmlsisa, nbiisclose, nbicustomtext1, nbicustomtext2, nbicustomtext3, nbicustomdbl1, nbicustomdbl2, nbicustomdbl3, nbicustomdate1, nbicustomdate2, nbicustomdate3) values" & strValue3.ToString & ""
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
                        Dim strValue2 As New StringBuilder, strValue3 As New StringBuilder
                        For Each dr1 As DataRow In dtserial.Rows
                            'QUERY INSERT NO SERIAL OUT
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            'mapping            nsoid,             nsoidserialin,                           nsogudang,                  nsoidbarang,                           nsokode,                             nsosumber,            nsoidtransaksi,                     nsosatuan,                          nsojmlkeluar,      nsoisclose
                            strValue2.Append("(" & 0 & ", " & dr1("nstidserialin") & ", '" & FixQuotes(dr1("nstgudang")) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', " & 0 & ")")

                            'QUERY INSERT NO SERIAL IN
                            strValue3.Append(IIf(Len(strValue3.ToString) = 0, "", ", "))
                            'mapping       nsiidserialin,                nsigudang,                nsiidbarang,                           nsikode,                             nsisumber,            nsiidtransaksi,                     nsisatuan,                       nsijmlmasuk, nsijmlkeluar,                  nsijmlsisa, nsiisclose,                     nsicustomtext1,                             nsicustomtext2,                             nsicustomtext3,                             nsicustomdbl1,                             nsicustomdbl2,                             nsicustomdbl3,                                             nsicustomdate1,                                              nsicustomdate2,                                              nsicustomdate3
                            strValue3.Append("(" & 0 & ", '" & FixQuotes(SetGudang) & "', " & dr1("nstidbarang") & ", '" & FixQuotes(dr1("nstkode")) & "', '" & FixQuotes(dr1("nstsumber")) & "', " & result(4) & ", '" & FixQuotes(dr1("nstsatuan")) & "', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixDouble(dr1("nstjml")) & "', '0', '" & FixQuotes(dr1("nstcustomtext1")) & "', '" & FixQuotes(dr1("nstcustomtext2")) & "', '" & FixQuotes(dr1("nstcustomtext3")) & "', '" & FixDouble(dr1("nstcustomdbl1")) & "', '" & FixDouble(dr1("nstcustomdbl2")) & "', '" & FixDouble(dr1("nstcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & "')")
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

                        'INSERT NO SERIAL IN MASUK ---------------------------
                        sql = "Insert into M1_No_Serial_In(nsiidserialin, nsigudang, nsiidbarang, nsikode, nsisumber, nsiidtransaksi, nsisatuan, nsijmlmasuk, nsijmlkeluar, nsijmlsisa, nsiisclose, nsicustomtext1, nsicustomtext2, nsicustomtext3, nsicustomdbl1, nsicustomdbl2, nsicustomdbl3, nsicustomdate1, nsicustomdate2, nsicustomdate3) values" & strValue3.ToString & ""
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


                    'UPDATE STOK ====================================================================
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
                    'END OF UPDATE STOK =============================================================


                    'INSERT ITEM TRANSACTION ========================================================
                    'AMBIL DATA DETAIL YANG BARU
                    sql = "SELECT dnrd.iddnrdetail, dnrd.idbarang, dnrd.namabarang, dnrd.tipebarang, dnrd.jml, dnrd.satuan, dnrd.jmlbarang, dnrd.satuanbarang, dnrd.matauang, dnrd.kurs, dnrd.harga, dnrd.diskon, dnrd.jmldiskon, dnrd.hpp, dnrd.idhppkhususmasuk, dnrd.gudangasal, dnrd.gudangtransit, dnrd.gudangtujuan, dnrd.catatan, dnrd.costcenter, dnrd.divisi, dnrd.subdivisi, dnrd.proyek, dnr.dnrinputtgl, i.bhpp FROM m4_dnr_detail dnrd JOIN m4_dnr dnr ON dnrd.iddnr = dnr.dnrid JOIN m1_item i ON dnrd.idbarang = i.bid WHERE dnrd.iddnr = '" & result(4) & "'"
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim hpp As Double = 0, jenismutasi As Double = 0, postinghpp As Double = 0
                    Dim strTransaksiBarang As New StringBuilder

                    If dtDetailNew.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtDetailNew.Rows
                            'jenismutasi dan postinghpp 
                            '- jika barang masuk maka jenismutasi = 1 dan postinghpp = 1
                            '- jika barang keluar maka jenismutasi = 0 dan postinghpp = 0
                            '- untuk transaksi mutasi saja maka postinghpp = 0
                            postinghpp = 0

                            'hitung hpp = hpp
                            hpp = Double.Parse(dr1("hpp"))

                            'POSTING BARANG KELUAR (gudangasal)
                            jenismutasi = 0
                            'QUERY INSERT TRANSAKSI BARANG KELUAR
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                 gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")

                            'POSTING BARANG MASUK (gudangtransit)
                            jenismutasi = 1
                            'QUERY INSERT TRANSAKSI BARANG MASUK
                            strTransaksiBarang.Append(IIf(Len(strTransaksiBarang.ToString) = 0, "", ", "))
                            'mapping                        id,                              cabang,                                    lokasi,                                    gudang,                         kodepa,           jenismutasi,                               sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                              kontak,               idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,                  idhppikm,                idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                       saldojml,               saldohpp,             saldonilai,                                        inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                            strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("dnrcabang")) & "', '" & FixQuotes(drutama("dnrlokasi")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', " & drutama("dnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("dnrsumber")) & "', " & result(4) & ", " & dr1("iddnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("dnrtgl"))) & "', " & drutama("dnrsupplier") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & dr1("idhppkhususmasuk") & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("dnruraian")) & "', '" & FixQuotes(drutama("dnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal(dr1("dnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("dnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
                        Next

                        sql = "Insert into M1_Item_Transaction (id, cabang, lokasi, gudang, kodepa, jenismutasi, sumber, idutama, iddetail, notransaksi, tgl, kontak, idbarang, namabarang, tipebarang, tipehpp, jml, satuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, idhppikm, idhppikk, hpp, uraian, catatan, catatandetail, costcenter, divisi, subdivisi, proyek, saldojml, saldohpp, saldonilai, inputtgl, inputuser, postingtgl, updatehpp, postinghpp, hppfix, postingjurnal, jurnalfix, tutupperiode, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customtext6, customtext7, customtext8, customtext9, customtext10, customint1, customint2, customint3, customint4, customint5, customint6, customint7, customint8, customint9, customint10, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdbl6, customdbl7, customdbl8, customdbl9, customdbl10, customdate1, customdate2, customdate3, customdate4, customdate5, customdate6, customdate7, customdate8, customdate9, customdate10) values" & strTransaksiBarang.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                    Else
                        result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF INSERT ITEM TRANSACTION =================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "DNR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_DnrUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
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
            Dim sumber As String = "Dnr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Dnrtgl, Dnrnotransaksi, Dnrstatus FROM M4_Dnr WHERE Dnrid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Dnrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m4_dnr_history
            Dim rsSimpanHistory As String = SimpanHistory.m4_Dnr_HistorySimpan("" & paramSplit(0) & "★M4_Dnr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_dnr_terkait")
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


                Dim idbarang As Integer = 0, idridetail As Integer = 0, idhppkhususmasuk As Integer = 0, jmlbarang As Double = 0
                Dim gudangOut As String = "", gudangIn As String = "", ftExistStok As String = "", ftStok As String = "", updStokIn As String = "", updStokOut As String = ""
                Dim updNilaiRI As String = "", updFilterRI As String = "", updNilaiHppI As String = "", updFilterHppI As String = "", delFilterHppI As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT iddnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idridetail, gudangasal, gudangtransit, gudangtujuan, idhppkhususmasuk, idhppfifomasuk, urutan FROM m4_dnr_detail WHERE iddnr = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : idridetail = dr1("idridetail") : jmlbarang = dr1("jmlbarang") : gudangIn = dr1("gudangasal") : gudangOut = dr1("gudangtransit") : idhppkhususmasuk = dr1("idhppkhususmasuk")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idridetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idridetail=" & idridetail)
                            updNilaiRI = String.Concat("WHEN '" & idridetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRI)

                            '2.2. SET FILTERUPDATE OUTSTANDING
                            updFilterRI = IIf(Len(updFilterRI.ToString) = 0, "", updFilterRI & " OR ")
                            updFilterRI = String.Concat(updFilterRI, "(idridetail = '" & idridetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idridetail & "' as idridetail, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
                        ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")

                        '3. SET NILAI UPDATE STOK KELUAR
                        updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        '4. SET NILAI UPDATE STOK MASUK
                        updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
                        updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok, "", "", "", "", "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------

                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterRI) > 0 Then
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
                    'END OF UPDATE OUTSTANDING DETAIL ---------------

                    'UPDATE OUTSTANDING UTAMA -----------------------
                    Dim ftDetail As String = "", statusOut As Integer = 0
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idri FROM m4_ri_detail WHERE " & updFilterRI & " GROUP BY idri")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idri = '" & dr1("idri") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idri, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m4_ri_detail WHERE " & ftDetail & " GROUP BY idri")
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
                    'END OF UPDATE OUTSTANDING UTAMA ----------------
                End If
                'END OF UPDATE OUTSTANDING ======================================================


                'UPDATE NO BATCH ================================================================
                Dim updNilaiBatch As String = "", updFilterBatch As String = ""
                Dim dtBatch As DataTable = AsDataTableAmbilDariDB("SELECT nboidbatchin, nbogudang, nboidbarang, nbokode, nbojmlkeluar FROM m1_no_batch_out WHERE nbosumber = '" & sumber & "' AND nboidtransaksi = '" & idtransaksi & "'")
                If dtBatch.Rows.Count > 0 Then
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


                'UPDATE STOK ====================================================================
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
                'END OF UPDATE STOK =============================================================


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

            End If

            'update status utama
            sql = "UPDATE M4_Dnr SET Dnrstatus = " & nilaiStatus & ", Dnrmodifikasiuser='" & userid & "', Dnrmodifikasitgl = NOW(), Dnrposting = 0, Dnrpostingtgl = '1971-01-01 00:00:00', Dnrjmlrevisi = Dnrjmlrevisi + 1 WHERE Dnrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_DnrSearch(PostWsSearch(paramSplit(0), "M4_DnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_DnrDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("dnrsupplierkode", "c1.kkode")
            Filter = Filter.Replace("dnrsuppliernama", "c1.knama")
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
            Dim sumber As String = "DNR", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Dnrid, Dnrnotransaksi FROM M4_Dnr WHERE Dnrid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT dnrcabang, dnrlokasi, dnrsumber, dnrautonotransaksi, dnrnotransaksi, dnrtgl"
            sql &= " FROM M4_dnr"
            sql &= " WHERE dnrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("dnrcabang")
                lokasi = dtNomorNext.Rows(0)("dnrlokasi")
                sumber = dtNomorNext.Rows(0)("dnrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("dnrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("dnrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("dnrtgl"))
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
            sql = "DELETE FROM M4_Dnr_Detail WHERE iddnr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M4_Dnr WHERE dnrid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_DnrSearch(PostWsSearch(paramSplit(0), "M4_DnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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