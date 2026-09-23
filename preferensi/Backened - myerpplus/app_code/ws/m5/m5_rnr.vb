Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_rnr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_RnrSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

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
        'rnrid(0) As Integer, rnrcabang(1) As String, rnrlokasi(2) As String, rnrgudang(3) As String, rnrasalbarang(4) As String, 
        'rnrasalbarangkategori(5) As Integer, rnrjenispenjualan(6) As String, rnrjenispenjualankategori(7) As Integer, rnrcarabayar(8) As Integer, rnrsumber(9) As String, 
        'rnrautonotransaksi(10) As Integer, rnrnotransaksi(11) As String, rnrtgl(12) As Date, rnrkodepa(13) As Integer, rnrcustomer(14) As Integer, 
        'rnrcustomerkontak(15) As String, rnr1alamat1(16) As String, rnr1alamat2(17) As String, rnr1alamat3(18) As String, rnr2alamat1(19) As String, 
        'rnr2alamat2(20) As String, rnr2alamat3(21) As String, rnrbagianpenjualan(22) As Integer, rnrekspedisi(23) As String, rnrtglkirim(24) As Date, 
        'rnrtermin(25) As String, rnrtgljatuhtempo(26) As Date, rnruraian(27) As String, rnrcatatan(28) As String, rnrnoref(29) As String, 
        'rnrtglnoref(30) As Date, rnrtglpenutupan(31) As Date, rnrmatauang(32) As String, rnrkurs(33) As Double, rnrhargatermasukpajak(34) As Integer, 
        'rnrtotal(35) As Double, rnrdiskonpersen(36) As String, rnrjmldiskon(37) As Double, rnrtotalpajak1detail(38) As Double, rnrtotalpajak2detail(39) As Double, 
        'rnrbiayalainpersen(40) As Double, rnrbiayalain(41) As Double, rnrtotaltransaksi(42) As Double, rnrjmlbayar(43) As Double, rnrstatuslunas(44) As Integer, 
        'rnrtgllunas(45) As Date, rnrnofakturpajak(46) As String, rnrsdhbayarpajak(47) As Integer, rnrtglbayarpajak(48) As Date, rnrrekdiskon(49) As String, 
        'rnrrekpajak1(50) As String, rnrrekpajak2(51) As String, rnrrekbiayalain(52) As String, rnrrekbayar(53) As String, rnridsq(54) As Integer, 
        'rnridso(55) As Integer, rnridpl(56) As Integer, rnriddo(57) As Integer, rnriddr(58) As Integer, rnridpi(59) As Integer, 
        'rnridsi(60) As Integer, rnrstatussr(61) As Integer, rnrstatus(62) As Integer, rnrstatussebelumnya(63) As Integer, rnrjmlrevisi(64) As Integer, 
        'rnrcetakanke(65) As Integer, rnrinputuser(66) As Integer, rnrinputtgl(67) As DateTime, rnrmodifikasiuser(68) As Integer, rnrmodifikasitgl(69) As DateTime, 
        'rnrposting(70) As Integer, rnrtutupperiode(71) As Integer, rnrisclose(72) As Integer, rnrcustomtext1(73) As String, rnrcustomtext2(74) As String, 
        'rnrcustomtext3(75) As String, rnrcustomtext4(76) As String, rnrcustomtext5(77) As String, rnrcustomint1(78) As Integer, rnrcustomint2(79) As Integer, 
        'rnrcustomint3(80) As Integer, rnrcustomdbl1(81) As Double, rnrcustomdbl2(82) As Double, rnrcustomdbl3(83) As Double, rnrcustomdate1(84) As Date, 
        'rnrcustomdate2(85) As Date, rnrcustomdate3(86) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatus, 
        'rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, 
        'rnrposting, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, 
        'rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, 
        'rnrcustomdate1, rnrcustomdate2, rnrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 87) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rnrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rnrid required numeric." : GoTo selesai
        End If
        'rnrasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rnrasalbarangkategori required numeric." : GoTo selesai
        End If
        'rnrjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rnrjenispenjualankategori required numeric." : GoTo selesai
        End If
        'rnrcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rnrcarabayar required numeric." : GoTo selesai
        End If
        'rnrautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rnrautonotransaksi required numeric." : GoTo selesai
        End If
        'rnrtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "rnrtgl required date." : GoTo selesai
        End If
        'rnrkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rnrkodepa required numeric." : GoTo selesai
        End If
        'rnrcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rnrcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "rnrcustomer can't be empty." : GoTo selesai
        End If
        'rnrbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rnrbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "rnrbagianpenjualan can't be empty." : GoTo selesai
        End If
        'rnrtglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rnrtglkirim required date." : GoTo selesai
        End If
        'rnrtgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rnrtgljatuhtempo required date." : GoTo selesai
        End If
        'rnrtglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "rnrtglnoref required date." : GoTo selesai
        End If
        'rnrtglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "rnrtglpenutupan required date." : GoTo selesai
        End If
        'rnrkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rnrkurs required numeric." : GoTo selesai
        End If
        'rnrhargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rnrhargatermasukpajak required numeric." : GoTo selesai
        End If
        'rnrtotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rnrtotal required numeric." : GoTo selesai
        End If
        'rnrjmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rnrjmldiskon required numeric." : GoTo selesai
        End If
        'rnrtotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rnrtotalpajak1detail required numeric." : GoTo selesai
        End If
        'rnrtotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rnrtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''rnrbiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "rnrbiayalainpersen required numeric." : GoTo selesai
        'End If
        'rnrbiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rnrbiayalain required numeric." : GoTo selesai
        End If
        'rnrtotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rnrtotaltransaksi required numeric." : GoTo selesai
        End If
        'rnrjmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "rnrjmlbayar required numeric." : GoTo selesai
        End If
        'rnrstatuslunas(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rnrstatuslunas required numeric." : GoTo selesai
        End If
        'rnrtgllunas(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rnrtgllunas required date." : GoTo selesai
        End If
        'rnrsdhbayarpajak(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rnrsdhbayarpajak required numeric." : GoTo selesai
        End If
        'rnrtglbayarpajak(48) As Date
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "rnrtglbayarpajak required date." : GoTo selesai
        End If
        'rnridsq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rnridsq required numeric." : GoTo selesai
        End If
        'rnridso(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rnridso required numeric." : GoTo selesai
        End If
        'rnridpl(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rnridpl required numeric." : GoTo selesai
        End If
        'rnriddo(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "rnriddo required numeric." : GoTo selesai
        End If
        'rnriddr(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rnriddr required numeric." : GoTo selesai
        End If
        'rnridpi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "rnridpi required numeric." : GoTo selesai
        End If
        'rnridsi(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "rnridsi required numeric." : GoTo selesai
        End If
        'rnrstatussr(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "rnrstatussr required numeric." : GoTo selesai
        End If
        'rnrstatus(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "rnrstatus required numeric." : GoTo selesai
        End If
        'rnrstatussebelumnya(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rnrstatussebelumnya required numeric." : GoTo selesai
        End If
        'rnrjmlrevisi(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "rnrjmlrevisi required numeric." : GoTo selesai
        End If
        'rnrcetakanke(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "rnrcetakanke required numeric." : GoTo selesai
        End If
        'rnrinputuser(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "rnrinputuser required numeric." : GoTo selesai
        End If
        'rnrinputtgl(67) As DateTime
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "rnrinputtgl required date." : GoTo selesai
        End If
        'rnrmodifikasiuser(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "rnrmodifikasiuser required numeric." : GoTo selesai
        End If
        'rnrmodifikasitgl(69) As DateTime
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "rnrmodifikasitgl required date." : GoTo selesai
        End If
        'rnrposting(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "rnrposting required numeric." : GoTo selesai
        End If
        'rnrtutupperiode(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "rnrtutupperiode required numeric." : GoTo selesai
        End If
        'rnrisclose(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "rnrisclose required numeric." : GoTo selesai
        End If
        'rnrcustomint1(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "rnrcustomint1 required numeric." : GoTo selesai
        End If
        'rnrcustomint2(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "rnrcustomint2 required numeric." : GoTo selesai
        End If
        'rnrcustomint3(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "rnrcustomint3 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl1(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "rnrcustomdbl1 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl2(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "rnrcustomdbl2 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl3(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "rnrcustomdbl3 required numeric." : GoTo selesai
        End If
        'rnrcustomdate1(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "rnrcustomdate1 required date." : GoTo selesai
        End If
        'rnrcustomdate2(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "rnrcustomdate2 required date." : GoTo selesai
        End If
        'rnrcustomdate3(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "rnrcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'rnrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rnrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rnrcabang should not be more than 25 character." : GoTo selesai
        End If

        'rnrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rnrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rnrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rnrgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rnrgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rnrgudang should not be more than 25 character." : GoTo selesai
        End If

        'rnrsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rnrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "rnrsumber should not be more than 10 character." : GoTo selesai
        End If

        'rnrnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rnrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "rnrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rnrtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "rnrtgl can't be empty" : GoTo selesai
        End If

        'rnrtglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rnrtglkirim can't be empty" : GoTo selesai
        End If

        'rnrtgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rnrtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rnrtglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rnrtglnoref can't be empty" : GoTo selesai
        End If

        'rnrtglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rnrtglpenutupan can't be empty" : GoTo selesai
        End If

        'rnrmatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "rnrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "rnrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rnrkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rnrkurs can't be empty" : GoTo selesai
        End If

        'rnrtotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rnrtotal can't be empty" : GoTo selesai
        End If

        'rnrdiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rnrdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "rnrdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rnrjmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rnrjmldiskon can't be empty" : GoTo selesai
        End If

        'rnrtotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rnrtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'rnrtotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rnrtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'rnrbiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rnrbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "rnrbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'rnrbiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rnrbiayalain can't be empty" : GoTo selesai
        End If

        'rnrtotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rnrtotaltransaksi can't be empty" : GoTo selesai
        End If

        'rnrjmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rnrjmlbayar can't be empty" : GoTo selesai
        End If

        'rnrtgllunas(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rnrtgllunas can't be empty" : GoTo selesai
        End If

        'rnrtglbayarpajak(48) As Date
        If Len(dataUtama(48)) = 0 Then
            result(2) = "rnrtglbayarpajak can't be empty" : GoTo selesai
        End If

        'rnrinputtgl(67) As DateTime
        If Len(dataUtama(67)) = 0 Then
            result(2) = "rnrinputtgl can't be empty" : GoTo selesai
        End If

        'rnrmodifikasitgl(69) As DateTime
        If Len(dataUtama(69)) = 0 Then
            result(2) = "rnrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl1(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "rnrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl2(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "rnrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl3(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "rnrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate1(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "rnrcustomdate1 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate2(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "rnrcustomdate2 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate3(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "rnrcustomdate3 can't be empty" : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rnrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnriddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnriddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rnrid~rnrcabang~rnrlokasi~rnrgudang~rnrasalbarang~rnrasalbarangkategori~rnrjenispenjualan~rnrjenispenjualankategori~rnrcarabayar~rnrsumber~rnrautonotransaksi~rnrnotransaksi~rnrtgl~rnrkodepa~rnrcustomer~rnrcustomerkontak~rnr1alamat1~rnr1alamat2~rnr1alamat3~rnr2alamat1~rnr2alamat2~rnr2alamat3~rnrbagianpenjualan~rnrekspedisi~rnrtglkirim~rnrtermin~rnrtgljatuhtempo~rnruraian~rnrcatatan~rnrnoref~rnrtglnoref~rnrtglpenutupan~rnrmatauang~rnrkurs~rnrhargatermasukpajak~rnrtotal~rnrdiskonpersen~rnrjmldiskon~rnrtotalpajak1detail~rnrtotalpajak2detail~rnrbiayalainpersen~rnrbiayalain~rnrtotaltransaksi~rnrjmlbayar~rnrstatuslunas~rnrtgllunas~rnrnofakturpajak~rnrsdhbayarpajak~rnrtglbayarpajak~rnrrekdiskon~rnrrekpajak1~rnrrekpajak2~rnrrekbiayalain~rnrrekbayar~rnridsq~rnridso~rnridpl~rnriddo~rnriddr~rnridpi~rnridsi~rnrstatussr~rnrstatus~rnrstatussebelumnya~rnrjmlrevisi~rnrcetakanke~rnrinputuser~rnrinputtgl~rnrmodifikasiuser~rnrmodifikasitgl~rnrposting~rnrtutupperiode~rnrisclose~rnrcustomtext1~rnrcustomtext2~rnrcustomtext3~rnrcustomtext4~rnrcustomtext5~rnrcustomint1~rnrcustomint2~rnrcustomint3~rnrcustomdbl1~rnrcustomdbl2~rnrcustomdbl3~rnrcustomdate1~rnrcustomdate2~rnrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrnrdetail(0) As Integer, idrnr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususkeluar(12) As Integer, idhppfifokeluar(13) As Integer, harga(14) As Double, 
        'hargapricelist(15) As Double, hpp(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekhargapokok(29) As String, 
        'rekdiskonpenjualan(30) As String, rekreturpenjualan(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idsqdetail(38) As Integer, idsodetail(39) As Integer, 
        'idpldetail(40) As Integer, iddodetail(41) As Integer, iddrdetail(42) As Integer, idpidetail(43) As Integer, idsidetail(44) As Integer, 
        'jmlsr(45) As Double, statussr(46) As Integer, isclose(47) As Integer, customtext1(48) As String, customtext2(49) As String, 
        'customtext3(50) As String, customdbl1(51) As Double, customdbl2(52) As Double, customdbl3(53) As Double, customdate1(54) As Date, 
        'customdate2(55) As Date, customdate3(56) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, 
        'harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, 
        'iddrdetail, idpidetail, idsidetail, jmlsr, statussr, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrnrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrnr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususkeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifokeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hargapricelist", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekreturpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddrdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussr", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updFilterSI As String = ""
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsidetail As Integer = 0
        Dim updStokIn As String = "", gudangIn As String = ""

        'FILTER SI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSI As String = ""

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
            'idrnrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrnrdetail required numeric." : GoTo selesai
            End If
            'idrnr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrnr required numeric." : GoTo selesai
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
            'idhppkhususkeluar(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususkeluar required numeric." : GoTo selesai
            End If
            'idhppfifokeluar(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppfifokeluar required numeric." : GoTo selesai
            End If
            'harga(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hargapricelist(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hargapricelist required numeric." : GoTo selesai
            End If
            'hpp(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
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
            'idsqdetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpldetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'iddodetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'iddrdetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - iddrdetail required numeric." : GoTo selesai
            End If
            'idpidetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idsidetail(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'jmlsr(45) As Double
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'harga(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hargapricelist(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargapricelist can't be empty" : GoTo selesai
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
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(17) As String
                '    dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(17).ToString))
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
            dataRowDetail(25) = dataUtama(3)
            If Len(dataRowDetail(25)) = 0 Then
                result(2) = "Row : " & i & " - gudangasal can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(25)) > 25 Then
                result(2) = "Row : " & i & " - gudangasal should not be more than 25 character." : GoTo selesai
            End If

            'gudangtransit(26) As String
            dataRowDetail(26) = dataUtama(3)
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - gudangtransit can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(26)) > 25 Then
                result(2) = "Row : " & i & " - gudangtransit should not be more than 25 character." : GoTo selesai
            End If

            'gudangtujuan(27) As String
            dataRowDetail(27) = dataUtama(3)
            If Len(dataRowDetail(27)) = 0 Then
                result(2) = "Row : " & i & " - gudangtujuan can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(27)) > 25 Then
                result(2) = "Row : " & i & " - gudangtujuan should not be more than 25 character." : GoTo selesai
            End If

            'jmlsr(45) As Double
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idrnrdetail~idrnr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususkeluar~idhppfifokeluar~harga~hargapricelist~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekreturpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpldetail~iddodetail~iddrdetail~idpidetail~idsidetail~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(26) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangIn = dataRowDetail(26)
            'idsidetail(44) As Integer
            idsidetail = dataRowDetail(44)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'ValidasiSimpan
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'VALIDASI OUTSTANDING -------------------------
            If idsidetail <> 0 Then 'SI
                'CEK SI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSI = IIf(Len(ftSI.ToString) = 0, "", ftSI & " OR ")
                ftSI = String.Concat(ftSI, " (sid.idsidetail = " & idsidetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingSI = IIf(Len(ftExistOutstandingSI.ToString) = 0, "", ftExistOutstandingSI & " UNION ")
                ftExistOutstandingSI = String.Concat(ftExistOutstandingSI, "SELECT EXISTS(SELECT 1 FROM m5_si_detail JOIN m5_si ON idsi = siid WHERE idsidetail = '" & idsidetail & "' AND (sistatus = 2 OR sistatus = 3 OR sistatus = 4 OR sistatus = 7) LIMIT 1) as rowExists, '" & idsidetail & "' as idsidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                ftOutstandingSI = IIf(Len(ftOutstandingSI.ToString) = 0, "", ftOutstandingSI & " OR ")
                ftOutstandingSI = String.Concat(ftOutstandingSI, " (sid.idsidetail = " & idsidetail & " AND " & Outstanding & " > (sid.jmlbarang - sid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSI)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
            End If

            ''VALIDASI STOK -------------------------------
            ''1. SET NILAI UPDATE STOK MASUK --------------
            'updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            'updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
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
            'END OF VALIDASI DAN SET ROW DATA SERIAL ===========================================
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
                vStatus = drutama("rnrstatus")
                vTgl = AsFormatTanggal(drutama("rnrtgl"))


                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 11
                Select Case drutama("rnrstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rnrtgl")), AsFormatTanggal(drutama("rnrtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rnrstatus") = 2 Or drutama("rnrstatus") = 1 Or drutama("rnrstatus") = 8 Or drutama("rnrstatus") = 9 Or drutama("rnrstatus") = 10 Or drutama("rnrstatus") = 11 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSI, ftOutstandingSI, "", "", "", "", ftSI, drutama("rnrhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rnrtermin").ToString, AsFormatTanggal(drutama("rnrtgl")), "rnrtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rnrtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("rnrtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("rnrtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("rnrtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("rnrhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("rnrtotaltransaksi") = Double.Parse(drutama("rnrtotal")) - Double.Parse(drutama("rnrjmldiskon")) + Double.Parse(drutama("rnrtotalpajak1detail")) + Double.Parse(drutama("rnrtotalpajak2detail")) + Double.Parse(drutama("rnrbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("rnrtotaltransaksi") = Double.Parse(drutama("rnrtotal")) - Double.Parse(drutama("rnrjmldiskon")) + Double.Parse(drutama("rnrbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("rnrid")
                    notransaksi = drutama("rnrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(rnrid), rnrnotransaksi FROM M5_rnr WHERE rnrid='" & result(4) & "' AND rnrstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("rnrautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rnrcabang"), drutama("rnrlokasi"), drutama("rnrsumber"), drutama("rnrtgl"))
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rnrid) FROM M5_rnr WHERE rnrnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_rnr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_RnrHistorySimpan("" & paramSplit(0) & "★M5_RnrHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rnrsumber")) & "▼" & FixQuotes(drutama("rnrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Rnr set rnrcabang  = '" & FixQuotes(drutama("rnrcabang")) & "', rnrlokasi  = '" & FixQuotes(drutama("rnrlokasi")) & "', rnrgudang  = '" & FixQuotes(drutama("rnrgudang")) & "', rnrasalbarang  = '" & FixQuotes(drutama("rnrasalbarang")) & "', rnrasalbarangkategori  = " & drutama("rnrasalbarangkategori") & ", rnrjenispenjualan  = '" & FixQuotes(drutama("rnrjenispenjualan")) & "', rnrjenispenjualankategori  = " & drutama("rnrjenispenjualankategori") & ", rnrcarabayar  = " & drutama("rnrcarabayar") & ", rnrsumber  = '" & FixQuotes(drutama("rnrsumber")) & "', rnrautonotransaksi  = " & drutama("rnrautonotransaksi") & ", rnrnotransaksi  = '" & FixQuotes(notransaksi) & "', rnrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', rnrkodepa  = " & drutama("rnrkodepa") & ", rnrcustomer  = " & drutama("rnrcustomer") & ", rnrcustomerkontak  = '" & FixQuotes(drutama("rnrcustomerkontak")) & "', rnr1alamat1  = '" & FixQuotes(drutama("rnr1alamat1")) & "', rnr1alamat2  = '" & FixQuotes(drutama("rnr1alamat2")) & "', rnr1alamat3  = '" & FixQuotes(drutama("rnr1alamat3")) & "', rnr2alamat1  = '" & FixQuotes(drutama("rnr2alamat1")) & "', rnr2alamat2  = '" & FixQuotes(drutama("rnr2alamat2")) & "', rnr2alamat3  = '" & FixQuotes(drutama("rnr2alamat3")) & "', rnrbagianpenjualan  = " & drutama("rnrbagianpenjualan") & ", rnrekspedisi  = '" & FixQuotes(drutama("rnrekspedisi")) & "', rnrtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglkirim"))) & "', rnrtermin  = '" & FixQuotes(drutama("rnrtermin")) & "', rnrtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', rnruraian  = '" & FixQuotes(drutama("rnruraian")) & "', rnrcatatan  = '" & FixQuotes(drutama("rnrcatatan")) & "', rnrnoref  = '" & FixQuotes(drutama("rnrnoref")) & "', rnrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglnoref"))) & "', rnrtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglpenutupan"))) & "', rnrmatauang  = '" & FixQuotes(drutama("rnrmatauang")) & "', rnrkurs  = '" & FixDouble(drutama("rnrkurs")) & "', rnrhargatermasukpajak  = " & drutama("rnrhargatermasukpajak") & ", rnrtotal  = '" & FixDouble(drutama("rnrtotal")) & "', rnrdiskonpersen  = '" & FixQuotes(drutama("rnrdiskonpersen")) & "', rnrjmldiskon  = '" & FixDouble(drutama("rnrjmldiskon")) & "', rnrtotalpajak1detail  = '" & FixDouble(drutama("rnrtotalpajak1detail")) & "', rnrtotalpajak2detail  = '" & FixDouble(drutama("rnrtotalpajak2detail")) & "', rnrbiayalainpersen  = '" & FixDouble(drutama("rnrbiayalainpersen")) & "', rnrbiayalain  = '" & FixDouble(drutama("rnrbiayalain")) & "', rnrtotaltransaksi  = '" & FixDouble(drutama("rnrtotaltransaksi")) & "', rnrjmlbayar  = '" & FixDouble(drutama("rnrjmlbayar")) & "', rnrstatuslunas  = " & drutama("rnrstatuslunas") & ", rnrtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', rnrnofakturpajak  = '" & FixQuotes(drutama("rnrnofakturpajak")) & "', rnrsdhbayarpajak  = " & drutama("rnrsdhbayarpajak") & ", rnrtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglbayarpajak"))) & "', rnrrekdiskon  = '" & FixQuotes(drutama("rnrrekdiskon")) & "', rnrrekpajak1  = '" & FixQuotes(drutama("rnrrekpajak1")) & "', rnrrekpajak2  = '" & FixQuotes(drutama("rnrrekpajak2")) & "', rnrrekbiayalain  = '" & FixQuotes(drutama("rnrrekbiayalain")) & "', rnrrekbayar  = '" & FixQuotes(drutama("rnrrekbayar")) & "', rnridsq  = " & drutama("rnridsq") & ", rnridso  = " & drutama("rnridso") & ", rnridpl  = " & drutama("rnridpl") & ", rnriddo  = " & drutama("rnriddo") & ", rnriddr  = " & drutama("rnriddr") & ", rnridpi  = " & drutama("rnridpi") & ", rnridsi  = " & drutama("rnridsi") & ", rnrstatussr  = " & drutama("rnrstatussr") & ", rnrstatus  = " & drutama("rnrstatus") & ", rnrstatussebelumnya  = " & drutama("rnrstatussebelumnya") & ", rnrjmlrevisi  = rnrjmlrevisi+1, rnrcetakanke  = " & drutama("rnrcetakanke") & ", rnrmodifikasiuser  = " & drutama("rnrmodifikasiuser") & ", rnrmodifikasitgl  = NOW(), rnrposting  = 0, rnrtutupperiode  = " & drutama("rnrtutupperiode") & ", rnrcustomtext1  = '" & FixQuotes(drutama("rnrcustomtext1")) & "', rnrcustomtext2  = '" & FixQuotes(drutama("rnrcustomtext2")) & "', rnrcustomtext3  = '" & FixQuotes(drutama("rnrcustomtext3")) & "', rnrcustomtext4  = '" & FixQuotes(drutama("rnrcustomtext4")) & "', rnrcustomtext5  = '" & FixQuotes(drutama("rnrcustomtext5")) & "', rnrcustomint1  = " & drutama("rnrcustomint1") & ", rnrcustomint2  = " & drutama("rnrcustomint2") & ", rnrcustomint3  = " & drutama("rnrcustomint3") & ", rnrcustomdbl1  = '" & FixDouble(drutama("rnrcustomdbl1")) & "', rnrcustomdbl2  = '" & FixDouble(drutama("rnrcustomdbl2")) & "', rnrcustomdbl3  = '" & FixDouble(drutama("rnrcustomdbl3")) & "', rnrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate1"))) & "', rnrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate2"))) & "', rnrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate3"))) & "' where rnrid = '" & drutama("rnrid") & "'"
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

                    If drutama("rnrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rnrcabang"), drutama("rnrlokasi"), drutama("rnrsumber"), drutama("rnrtgl"))
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
                        notransaksi = drutama("rnrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(rnrid) FROM m5_rnr WHERE rnrnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Rnr (rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3) values('" & FixQuotes(drutama("rnrcabang")) & "', '" & FixQuotes(drutama("rnrlokasi")) & "', '" & FixQuotes(drutama("rnrgudang")) & "', '" & FixQuotes(drutama("rnrasalbarang")) & "', " & drutama("rnrasalbarangkategori") & ", '" & FixQuotes(drutama("rnrjenispenjualan")) & "', " & drutama("rnrjenispenjualankategori") & ", " & drutama("rnrcarabayar") & ", '" & FixQuotes(drutama("rnrsumber")) & "', " & drutama("rnrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', " & drutama("rnrkodepa") & ", " & drutama("rnrcustomer") & ", '" & FixQuotes(drutama("rnrcustomerkontak")) & "', '" & FixQuotes(drutama("rnr1alamat1")) & "', '" & FixQuotes(drutama("rnr1alamat2")) & "', '" & FixQuotes(drutama("rnr1alamat3")) & "', '" & FixQuotes(drutama("rnr2alamat1")) & "', '" & FixQuotes(drutama("rnr2alamat2")) & "', '" & FixQuotes(drutama("rnr2alamat3")) & "', " & drutama("rnrbagianpenjualan") & ", '" & FixQuotes(drutama("rnrekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglkirim"))) & "', '" & FixQuotes(drutama("rnrtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', '" & FixQuotes(drutama("rnruraian")) & "', '" & FixQuotes(drutama("rnrcatatan")) & "', '" & FixQuotes(drutama("rnrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglpenutupan"))) & "', '" & FixQuotes(drutama("rnrmatauang")) & "', '" & FixDouble(drutama("rnrkurs")) & "', " & drutama("rnrhargatermasukpajak") & ", '" & FixDouble(drutama("rnrtotal")) & "', '" & FixQuotes(drutama("rnrdiskonpersen")) & "', '" & FixDouble(drutama("rnrjmldiskon")) & "', '" & FixDouble(drutama("rnrtotalpajak1detail")) & "', '" & FixDouble(drutama("rnrtotalpajak2detail")) & "', '" & FixDouble(drutama("rnrbiayalainpersen")) & "', '" & FixDouble(drutama("rnrbiayalain")) & "', '" & FixDouble(drutama("rnrtotaltransaksi")) & "', '" & FixDouble(drutama("rnrjmlbayar")) & "', " & drutama("rnrstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', '" & FixQuotes(drutama("rnrnofakturpajak")) & "', " & drutama("rnrsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtglbayarpajak"))) & "', '" & FixQuotes(drutama("rnrrekdiskon")) & "', '" & FixQuotes(drutama("rnrrekpajak1")) & "', '" & FixQuotes(drutama("rnrrekpajak2")) & "', '" & FixQuotes(drutama("rnrrekbiayalain")) & "', '" & FixQuotes(drutama("rnrrekbayar")) & "', " & drutama("rnridsq") & ", " & drutama("rnridso") & ", " & drutama("rnridpl") & ", " & drutama("rnriddo") & ", " & drutama("rnriddr") & ", " & drutama("rnridpi") & ", " & drutama("rnridsi") & ", " & drutama("rnrstatussr") & ", " & drutama("rnrstatus") & ", " & drutama("rnrstatussebelumnya") & ", " & drutama("rnrjmlrevisi") & ", " & drutama("rnrcetakanke") & ", " & drutama("rnrinputuser") & ", NOW(), " & drutama("rnrmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("rnrtutupperiode") & ", " & drutama("rnrisclose") & ", '" & FixQuotes(drutama("rnrcustomtext1")) & "', '" & FixQuotes(drutama("rnrcustomtext2")) & "', '" & FixQuotes(drutama("rnrcustomtext3")) & "', '" & FixQuotes(drutama("rnrcustomtext4")) & "', '" & FixQuotes(drutama("rnrcustomtext5")) & "', " & drutama("rnrcustomint1") & ", " & drutama("rnrcustomint2") & ", " & drutama("rnrcustomint3") & ", '" & FixDouble(drutama("rnrcustomdbl1")) & "', '" & FixDouble(drutama("rnrcustomdbl2")) & "', '" & FixDouble(drutama("rnrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select rnrid from M5_rnr where rnrnotransaksi='" & notransaksi & "' AND rnrinputuser= '" & userid & "' order by rnrmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Rnr_Detail where idrnr = '" & result(4) & "'"
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
                    Dim dtBefore As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("rnrmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI SI ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2, IFNULL(t1.tnilai,0) as nilaipajak1, IFNULL(t2.tnilai,0) as nilaipajak2 FROM m5_si_detail LEFT JOIN m1_tax t1 ON pajak1 = t1.tkode LEFT JOIN m1_tax t2 ON pajak2 = t2.tkode WHERE idsidetail = '" & FixDouble(dr1("idsidetail")) & "'"
                        dtBefore = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtBefore.Rows.Count > 0 Then
                            'SET HARGA - ambil dari SI
                            dr1("harga") = Double.Parse(dtBefore.Rows(0)("harga"))

                            'SET DISKON - ambil dari SI
                            dr1("diskon") = dtBefore.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari SI
                            dr1("pajak1") = dtBefore.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari SI = (jmlpajakSI / jmlSI) * jml
                            'dr1("jmlpajak1") = (Double.Parse(dtBefore.Rows(0)("jmlpajak1")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari SI
                            dr1("pajak2") = dtBefore.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari SI = (jmlpajakSI / jmlSI) * jml
                            'dr1("jmlpajak2") = (Double.Parse(dtBefore.Rows(0)("jmlpajak2")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            If drutama("rnrhargatermasukpajak") = 1 Then

                                'SET JMLPAJAK1
                                dr1("jmlpajak1") = (((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) / (100 + Decimal.Parse(dtBefore.Rows(0)("nilaipajak1")))) * Decimal.Parse(dtBefore.Rows(0)("nilaipajak1"))

                                'SET JMLPAJAK2
                                dr1("jmlpajak2") = (((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) / (100 + Decimal.Parse(dtBefore.Rows(0)("nilaipajak1")))) * Decimal.Parse(dtBefore.Rows(0)("nilaipajak2"))

                            Else

                                'SET JMLPAJAK1
                                dr1("jmlpajak1") = ((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) * (Decimal.Parse(dtBefore.Rows(0)("nilaipajak1")) / 100)

                                'SET JMLPAJAK2
                                dr1("jmlpajak2") = ((Decimal.Parse(dr1("jml")) * Decimal.Parse(dr1("harga"))) - Decimal.Parse(dr1("jmldiskon"))) * (Decimal.Parse(dtBefore.Rows(0)("nilaipajak2")) / 100)

                            End If

                        End If
                        'END OF SET HARGA DARI SI -----------------------------------------------


                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrnrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hargapricelist")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekreturpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", " & dr1("iddrdetail") & ", " & dr1("idpidetail") & ", " & dr1("idsidetail") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Rnr_Detail(idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RNR'"
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
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RNR'"
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


                If drutama("rnrstatus") = 2 Then

                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiSI) > 0 Then 'SI
                        'UPDATE DETAIL
                        sql = "UPDATE m5_si_detail SET jmlrealisasi = (CASE idsidetail " & updNilaiSI & " ELSE jmlrealisasi END) WHERE " & updFilterSI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsi FROM m5_si_detail WHERE " & updFilterSI & " GROUP BY idsi", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsi = '" & dr1("idsi") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idsi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_si_detail WHERE " & ftDetail & " GROUP BY idsi", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiSI = "" : updFilterSI = ""
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
                                updNilaiSI = String.Concat(updNilaiSI, "WHEN '" & dr1("idsi") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                                updFilterSI = String.Concat(updFilterSI, "(siid = '" & dr1("idsi") & "')")
                            Next

                            sql = "UPDATE m5_si SET sistatusrealisasi = (CASE siid " & updNilaiSI & " ELSE sistatusrealisasi END) WHERE " & updFilterSI
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

                    ''UPDATE STOK ====================================================================
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
                    ''END OF UPDATE STOK =============================================================

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


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    'Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rnrd.idrnrdetail, rnrd.idbarang, rnrd.namabarang, rnrd.tipebarang, rnrd.jml, rnrd.satuan, rnrd.jmlbarang, rnrd.satuanbarang, rnrd.matauang, rnrd.kurs, rnrd.harga, rnrd.diskon, rnrd.hpp, rnrd.jmldiskon, rnr.rnrgudang as gudang, rnrd.catatan, rnrd.costcenter, rnrd.divisi, rnrd.subdivisi, rnrd.proyek, rnr.rnrinputtgl, i.bhpp, rnrd.jmlpajak1, rnrd.jmlpajak2 FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid JOIN m1_item i ON rnrd.idbarang = i.bid WHERE rnrd.idrnr = '" & result(4) & "' ORDER BY rnrd.urutan", myConn)
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT rnrd.idrnrdetail, rnrd.idbarang, rnrd.namabarang, rnrd.tipebarang, rnrd.jml, rnrd.satuan, rnrd.jmlbarang, rnrd.satuanbarang, rnrd.matauang, rnrd.kurs, rnrd.harga, rnrd.diskon, rnrd.hpp, rnrd.jmldiskon, rnr.rnrgudang as gudang, rnrd.catatan, rnrd.costcenter, rnrd.divisi, rnrd.subdivisi, rnrd.proyek, rnr.rnrinputtgl, i.bhpp, rnrd.jmlpajak1, rnrd.jmlpajak2, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid JOIN m1_item i ON rnrd.idbarang = i.bid LEFT JOIN m1_cost_center cc ON rnrd.costcenter = cc.cckode WHERE rnrd.idrnr = '" & result(4) & "' ORDER BY rnrd.urutan", myConn)

                    Dim gudang As String = ""
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
                                    'mapping                        id,                            cabang,                                    lokasi,                               gudang,                         kodepa,           jenismutasi,                              sumber,                     idutama,             iddetail,                      notransaksi,                                                  tgl,                            kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,  idhppikk,                hpp,                                  uraian,                                    catatan,                       catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                              inputtgl,                                                inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                    strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("rnrcabang")) & "', '" & FixQuotes(drutama("rnrlokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', " & drutama("rnrkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("rnrsumber")) & "', " & result(4) & ", " & dr1("idrnrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', " & drutama("rnrcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & 0 & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("rnruraian")) & "', '" & FixQuotes(drutama("rnrcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("rnrinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("rnrinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                                    'If drutama("rnrhargatermasukpajak") = 0 Then
                                    sql = "UPDATE m1_item SET bstok = '" & FixDouble(saldojml) & "' WHERE bid = '" & idbarang & "'"
                                    'Else
                                    ' sql = "UPDATE m1_item LEFT JOIN m0_setting ON smodule = 0 AND sgrup = 'options' AND skode = 'PembelianUpdateHargaBeli' SET bstok = '" & FixDouble(saldojml) & "', bhargabeli = (CASE IFNULL(snilai,0) WHEN 1 THEN '" & FixDouble((Double.Parse(dr1("harga")) * Double.Parse(dr1("kurs"))) - ((Double.Parse(dr1("jmldiskon")) / Double.Parse(dr1("jml"))) * Double.Parse(dr1("kurs")))) & "' ELSE bhargabeli END), baktiftgl = '" & drutama("rnrtgl") & "' WHERE bid = '" & idbarang & "'"
                                    'End If
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

                'INSERT MSMQ COGS ===================================================================
                Dim sumber As String = "RNR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("rnrstatus") = 2 Then
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
                'END OF INSERT MSMQ COGS ============================================================

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
    Public Function M5_RnrUpdateStatus(ByVal param As String) As String

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
            Dim sumber As String = "Rnr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rnrtgl, Rnrnotransaksi, Rnrstatus FROM M5_Rnr WHERE Rnrid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rnrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_rnr_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_RnrHistorySimpan("" & paramSplit(0) & "★M5_RnrHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'Dim query As New m0_query
                sql = m5_rnr_terkait("rnrid = '" & idtransaksi & "'")
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


                'UPDATE STOK DAN OUTSTANDING ====================================================
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim ftExistStok As String = "", ftStok As String = ""
                Dim updNilaiSI As String = "", updFilterSI As String = ""
                Dim updStokOut As String = "", gudangOut As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""
                Dim idrnrdetail As Integer = 0
                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsidetail As Integer = 0


                'AMBIL DATA DETAIL
                'dtdetail = AsDataTableAmbilDariDBCon("SELECT idrnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsidetail, gudangtransit, gudangtujuan, urutan FROM m5_rnr_detail WHERE idrnr = '" & idtransaksi & "'", myConn)
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idrnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsidetail, gudangtransit, gudangtujuan, urutan, (CASE LENGTH(IFNULL(cc.ccakun,'')) WHEN 0 THEN 1 ELSE 0 END) as transbarang FROM m5_rnr_detail rnrd LEFT JOIN m1_cost_center cc ON rnrd.costcenter = cc.cckode WHERE idrnr = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsidetail = dr1("idsidetail")
                        gudangOut = dr1("gudangtransit") : idrnrdetail = dr1("idrnrdetail")

                        '2. BUAT FILTER CEK HPP KHUSUS(I)
                        ftHppI = IIf(Len(ftHppI.ToString) = 0, "", ftHppI & " OR ")
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idrnrdetail & "' AND sumber = 'RNR')")

                        '3. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idrnrdetail & "' AND cfisumber = 'RNR')")

                        '4. BUAT FILTER CEK STOCK EXIST
                        If dr1("transbarang") = 1 Then
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            '5. BUAT FILTER CEK JML STOCK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudang='" & gudangOut & "'")
                            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                            'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > (isw.stok - IFNULL(isb.jmlbooking,0))) ")
                            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                            '6. SET NILAI UPDATE STOK KELUAR
                            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok
                        End If
                       

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING SI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                            updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSI)

                            '2.2. SET FILTERUPDATE OUTSTANDING SI
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
                        End If

                        '8 SET NILAI UPDATE STOK BARANG
                        If dr1("transbarang") = 1 Then
                            Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                            '9. SET FILTERUPDATE STOK BARANG
                            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                            ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")
                        End If
                       

                        ''VALIDASI STOK -------------------------------
                        ''1. CEK DATA EXIST STOK TUJUAN
                        'ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        'ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        ''2. CEK JML STOK TUJUAN
                        'Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
                        'ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        ''3. SET NILAI UPDATE STOK KELUAR TUJUAN
                        'updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        'updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                ''VALIDASI STOK ----------------------------------
                ''STOK TUJUAN
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok)
                'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                ''END OF VALIDASI STOK ---------------------------

                'VALIDASI HPP, STOK ==========================================================
                'ValidasiSimpan
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftHppI, ftHppF, ftExistStok, ftStok, "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI HPP, STOK ===================================================

                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterSI) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_si_detail SET jmlrealisasi = (CASE idsidetail " & updNilaiSI & " ELSE jmlrealisasi END) WHERE " & updFilterSI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsi FROM m5_si_detail WHERE " & updFilterSI & " GROUP BY idsi", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsi = '" & dr1("idsi") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idsi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_si_detail WHERE " & ftDetail & " GROUP BY idsi", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiSI = "" : updFilterSI = ""
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
                            updNilaiSI = String.Concat(updNilaiSI, "WHEN '" & dr1("idsi") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(siid = '" & dr1("idsi") & "')")
                        Next

                        sql = "UPDATE m5_si SET sistatusrealisasi = (CASE siid " & updNilaiSI & " ELSE sistatusrealisasi END) WHERE " & updFilterSI
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
                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT rnrd.idbarang, ROUND(SUM(rnrd.jmlbarang * rnrd.hpp),2) as nilai, SUM(rnrd.jmlbarang) as jumlah"
                sql &= " FROM m5_rnr_detail rnrd"
                sql &= " WHERE rnrd.idrnr = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY rnrd.idbarang"
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
            sql = "UPDATE M5_Rnr SET Rnrstatus = " & nilaiStatus & ", Rnrmodifikasiuser='" & userid & "', Rnrmodifikasitgl = NOW(), Rnrposting = 0, Rnrpostingtgl = '1971-01-01 00:00:00', Rnrjmlrevisi = Rnrjmlrevisi + 1 WHERE Rnrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_RnrSearch(PostWsSearch(paramSplit(0), "M5_RnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_RnrDelete(ByVal param As String) As String

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
            Dim sumber As String = "Rnr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rnrid, Rnrnotransaksi FROM M5_Rnr WHERE Rnrid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rnrcabang, rnrlokasi, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl"
            sql &= " FROM M5_rnr"
            sql &= " WHERE rnrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rnrcabang")
                lokasi = dtNomorNext.Rows(0)("rnrlokasi")
                sumber = dtNomorNext.Rows(0)("rnrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rnrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rnrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rnrtgl"))
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


            'DELETE DETAIL
            sql = "DELETE FROM M5_Rnr_Detail WHERE idrnr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Rnr WHERE rnrid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_RnrSearch(PostWsSearch(paramSplit(0), "M5_RnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_RnrGetdataById(ByVal param As String) As String

        'M5_RnrGetdataById Utama --------------------------------------------------------
        'rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, 
        'rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, 
        'rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, 
        'rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, 
        'rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3, rnrcabangnama, rnrlokasinama, 
        'rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, rnrterminnama, 
        'rnrterminharijatuhtempo, rnrrekdiskonnama, rnrrekpajak1nama, rnrrekpajak2nama, rnrrekbiayalainnama, rnrrekbayarnama, rnrnotransaksipi, 
        'rnrnotransaksisi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama, ktingkatjual, kpkp

        'M5_RnrGetdataById Detail --------------------------------------------------------
        'idrnrdetail, idrnr, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, 
        'hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, 
        'cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, 
        'rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, 
        'idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, 
        'pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, 
        'gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, pinotransaksi, sinotransaksi,
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_RnrGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_RnrGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

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
        Dim sumber As String = "RNR"

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

        Dim NmMemcached As String = "aplikasi1-M5_Rnr~M5_Rnr_Detail-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "rnrid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "rnrid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_rnr_getdata")
        sql = "select `rnr`.`rnrid` AS `rnrid`,`rnr`.`rnrcabang` AS `rnrcabang`,`rnr`.`rnrlokasi` AS `rnrlokasi`,`rnr`.`rnrgudang` AS `rnrgudang`,`rnr`.`rnrasalbarang` AS `rnrasalbarang`,`rnr`.`rnrasalbarangkategori` AS `rnrasalbarangkategori`,`rnr`.`rnrjenispenjualan` AS `rnrjenispenjualan`,`rnr`.`rnrjenispenjualankategori` AS `rnrjenispenjualankategori`,`rnr`.`rnrcarabayar` AS `rnrcarabayar`,`rnr`.`rnrsumber` AS `rnrsumber`,`rnr`.`rnrautonotransaksi` AS `rnrautonotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`rnr`.`rnrtgl` AS `rnrtgl`,`rnr`.`rnrkodepa` AS `rnrkodepa`,`rnr`.`rnrcustomer` AS `rnrcustomer`,`rnr`.`rnrcustomerkontak` AS `rnrcustomerkontak`,`rnr`.`rnr1alamat1` AS `rnr1alamat1`,`rnr`.`rnr1alamat2` AS `rnr1alamat2`,`rnr`.`rnr1alamat3` AS `rnr1alamat3`,`rnr`.`rnr2alamat1` AS `rnr2alamat1`,`rnr`.`rnr2alamat2` AS `rnr2alamat2`,`rnr`.`rnr2alamat3` AS `rnr2alamat3`,`rnr`.`rnrbagianpenjualan` AS `rnrbagianpenjualan`,`rnr`.`rnrekspedisi` AS `rnrekspedisi`,`rnr`.`rnrtglkirim` AS `rnrtglkirim`,`rnr`.`rnrtermin` AS `rnrtermin`,`rnr`.`rnrtgljatuhtempo` AS `rnrtgljatuhtempo`,`rnr`.`rnruraian` AS `rnruraian`,`rnr`.`rnrcatatan` AS `rnrcatatan`,`rnr`.`rnrnoref` AS `rnrnoref`,`rnr`.`rnrtglnoref` AS `rnrtglnoref`,`rnr`.`rnrtglpenutupan` AS `rnrtglpenutupan`,`rnr`.`rnrmatauang` AS `rnrmatauang`,`rnr`.`rnrkurs` AS `rnrkurs`,`rnr`.`rnrhargatermasukpajak` AS `rnrhargatermasukpajak`,`rnr`.`rnrtotal` AS `rnrtotal`,`rnr`.`rnrdiskonpersen` AS `rnrdiskonpersen`,`rnr`.`rnrjmldiskon` AS `rnrjmldiskon`,`rnr`.`rnrtotalpajak1detail` AS `rnrtotalpajak1detail`,`rnr`.`rnrtotalpajak2detail` AS `rnrtotalpajak2detail`,`rnr`.`rnrbiayalainpersen` AS `rnrbiayalainpersen`,`rnr`.`rnrbiayalain` AS `rnrbiayalain`,`rnr`.`rnrtotaltransaksi` AS `rnrtotaltransaksi`,`rnr`.`rnrjmlbayar` AS `rnrjmlbayar`,`rnr`.`rnrstatuslunas` AS `rnrstatuslunas`,`rnr`.`rnrtgllunas` AS `rnrtgllunas`,`rnr`.`rnrnofakturpajak` AS `rnrnofakturpajak`,`rnr`.`rnrsdhbayarpajak` AS `rnrsdhbayarpajak`,`rnr`.`rnrtglbayarpajak` AS `rnrtglbayarpajak`,`rnr`.`rnrrekdiskon` AS `rnrrekdiskon`,`rnr`.`rnrrekpajak1` AS `rnrrekpajak1`,`rnr`.`rnrrekpajak2` AS `rnrrekpajak2`,`rnr`.`rnrrekbiayalain` AS `rnrrekbiayalain`,`rnr`.`rnrrekbayar` AS `rnrrekbayar`,`rnr`.`rnridsq` AS `rnridsq`,`rnr`.`rnridso` AS `rnridso`,`rnr`.`rnridpl` AS `rnridpl`,`rnr`.`rnriddo` AS `rnriddo`,`rnr`.`rnriddr` AS `rnriddr`,`rnr`.`rnridpi` AS `rnridpi`,`rnr`.`rnridsi` AS `rnridsi`,`rnr`.`rnrstatussr` AS `rnrstatussr`,`rnr`.`rnrstatusrealisasi` AS `rnrstatusrealisasi`,`rnr`.`rnrstatus` AS `rnrstatus`,`rnr`.`rnrstatussebelumnya` AS `rnrstatussebelumnya`,`rnr`.`rnrjmlrevisi` AS `rnrjmlrevisi`,`rnr`.`rnrcetakanke` AS `rnrcetakanke`,`rnr`.`rnrinputuser` AS `rnrinputuser`,`rnr`.`rnrinputtgl` AS `rnrinputtgl`,`rnr`.`rnrmodifikasiuser` AS `rnrmodifikasiuser`,`rnr`.`rnrmodifikasitgl` AS `rnrmodifikasitgl`,`rnr`.`rnrposting` AS `rnrposting`,`rnr`.`rnrpostingtgl` AS `rnrpostingtgl`,`rnr`.`rnrtutupperiode` AS `rnrtutupperiode`,`rnr`.`rnrisclose` AS `rnrisclose`,`rnr`.`rnrcustomtext1` AS `rnrcustomtext1`,`rnr`.`rnrcustomtext2` AS `rnrcustomtext2`,`rnr`.`rnrcustomtext3` AS `rnrcustomtext3`,`rnr`.`rnrcustomtext4` AS `rnrcustomtext4`,`rnr`.`rnrcustomtext5` AS `rnrcustomtext5`,`rnr`.`rnrcustomint1` AS `rnrcustomint1`,`rnr`.`rnrcustomint2` AS `rnrcustomint2`,`rnr`.`rnrcustomint3` AS `rnrcustomint3`,`rnr`.`rnrcustomdbl1` AS `rnrcustomdbl1`,`rnr`.`rnrcustomdbl2` AS `rnrcustomdbl2`,`rnr`.`rnrcustomdbl3` AS `rnrcustomdbl3`,`rnr`.`rnrcustomdate1` AS `rnrcustomdate1`,`rnr`.`rnrcustomdate2` AS `rnrcustomdate2`,`rnr`.`rnrcustomdate3` AS `rnrcustomdate3`,`br`.`bnama` AS `rnrcabangnama`,`lc`.`lnama` AS `rnrlokasinama`,`wh`.`wnama` AS `rnrgudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `rnrcustomerkode`,`c1`.`knama` AS `rnrcustomernama`,`c2`.`kkode` AS `rnrbagianpenjualankode`,`c2`.`knama` AS `rnrbagianpenjualannama`,`e`.`enama` AS `rnrekspedisinama`,`tr`.`trnama` AS `rnrterminnama`,`tr`.`trharijatuhtempo` AS `rnrterminharijatuhtempo`,`coa1`.`cnama` AS `rnrrekdiskonnama`,`coa2`.`cnama` AS `rnrrekpajak1nama`,`coa3`.`cnama` AS `rnrrekpajak2nama`,`coa4`.`cnama` AS `rnrrekbiayalainnama`,`coa5`.`cnama` AS `rnrrekbayarnama`,`pi`.`pinotransaksi` AS `rnrnotransaksipi`,`si`.`sinotransaksi` AS `rnrnotransaksisi`,`st1`.`nama` AS `rnrstatusnama`,`st2`.`nama` AS `rnrstatussebelumnyanama`,`u1`.`unama` AS `rnrinputusernama`,`u2`.`unama` AS `rnrmodifikasiusernama`,`rnrd`.`idrnrdetail` AS `idrnrdetail`,`rnrd`.`idrnr` AS `idrnr`,`rnrd`.`idbarang` AS `idbarang`,`rnrd`.`namabarang` AS `namabarang`,`rnrd`.`tipebarang` AS `tipebarang`,`rnrd`.`jml` AS `jml`,`rnrd`.`satuan` AS `satuan`,`rnrd`.`nilaisatuan` AS `nilaisatuan`,`rnrd`.`jmlbarang` AS `jmlbarang`,`rnrd`.`satuanbarang` AS `satuanbarang`,`rnrd`.`matauang` AS `matauang`,`rnrd`.`kurs` AS `kurs`,`rnrd`.`idhppkhususkeluar` AS `idhppkhususkeluar`,`rnrd`.`idhppfifokeluar` AS `idhppfifokeluar`,`rnrd`.`harga` AS `harga`,`rnrd`.`hargapricelist` AS `hargapricelist`,`rnrd`.`hpp` AS `hpp`,`rnrd`.`diskon` AS `diskon`,`rnrd`.`jmldiskon` AS `jmldiskon`,`rnrd`.`pajak1` AS `pajak1`,`rnrd`.`jmlpajak1` AS `jmlpajak1`,`rnrd`.`pajak2` AS `pajak2`,`rnrd`.`jmlpajak2` AS `jmlpajak2`,`rnrd`.`cabang` AS `cabang`,`rnrd`.`lokasi` AS `lokasi`,`rnrd`.`gudangasal` AS `gudangasal`,`rnrd`.`gudangtransit` AS `gudangtransit`,`rnrd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`rnrd`.`rekhargapokok` AS `rekhargapokok`,`rnrd`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`,`rnrd`.`rekreturpenjualan` AS `rekreturpenjualan`,`rnrd`.`costcenter` AS `costcenter`,`rnrd`.`divisi` AS `divisi`,`rnrd`.`subdivisi` AS `subdivisi`,`rnrd`.`proyek` AS `proyek`,`rnrd`.`catatan` AS `catatan`,`rnrd`.`urutan` AS `urutan`,`rnrd`.`idsqdetail` AS `idsqdetail`,`rnrd`.`idsodetail` AS `idsodetail`,`rnrd`.`idpldetail` AS `idpldetail`,`rnrd`.`iddodetail` AS `iddodetail`,`rnrd`.`iddrdetail` AS `iddrdetail`,`rnrd`.`idpidetail` AS `idpidetail`,`rnrd`.`idsidetail` AS `idsidetail`,`rnrd`.`jmlsr` AS `jmlsr`,`rnrd`.`statussr` AS `statussr`,`rnrd`.`jmlrealisasi` AS `jmlrealisasi`,`rnrd`.`statusrealisasi` AS `statusrealisasi`,`rnrd`.`isclose` AS `isclose`,`rnrd`.`customtext1` AS `customtext1`,`rnrd`.`customtext2` AS `customtext2`,`rnrd`.`customtext3` AS `customtext3`,`rnrd`.`customdbl1` AS `customdbl1`,`rnrd`.`customdbl2` AS `customdbl2`,`rnrd`.`customdbl3` AS `customdbl3`,`rnrd`.`customdate1` AS `customdate1`,`rnrd`.`customdate2` AS `customdate2`,`rnrd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pi2`.`pinotransaksi` AS `pinotransaksi`,`si2`.`sinotransaksi` AS `sinotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from `m5_rnr` `rnr` join `m5_rnr_detail` `rnrd` on `rnr`.`rnrid` = `rnrd`.`idrnr` left join `m1_branch` `br` on `br`.`bkode` = `rnr`.`rnrcabang` left join `m1_location` `lc` on `lc`.`lkode` = `rnr`.`rnrlokasi` left join `m1_warehouse` `wh` on `wh`.`wkode` = `rnr`.`rnrgudang` left join `m1_contact` `c1` on `c1`.`kid` = `rnr`.`rnrcustomer` left join `m1_contact` `c2` on `c2`.`kid` = `rnr`.`rnrbagianpenjualan` left join `m1_expedition` `e` on `rnr`.`rnrekspedisi` = `e`.`ekode` left join `m1_terms` `tr` on `rnr`.`rnrtermin` = `tr`.`trkode` left join `m1_coa` `coa1` on `rnr`.`rnrrekdiskon` = `coa1`.`cnomor` left join `m1_coa` `coa2` on `rnr`.`rnrrekpajak1` = `coa2`.`cnomor` left join `m1_coa` `coa3` on `rnr`.`rnrrekpajak2` = `coa3`.`cnomor` left join `m1_coa` `coa4` on `rnr`.`rnrrekbiayalain` = `coa4`.`cnomor` left join `m1_coa` `coa5` on `rnr`.`rnrrekbayar` = `coa5`.`cnomor` left join `m5_pi` `pi` on `rnr`.`rnridpi` = `pi`.`piid` left join `m5_si` `si` on `rnr`.`rnridsi` = `si`.`siid` left join `m0_status` `st1` on `st1`.`kode` = `rnr`.`rnrstatus` left join `m0_status` `st2` on `st2`.`kode` = `rnr`.`rnrstatussebelumnya` left join `m0_user` `u1` on `u1`.`userid` = `rnr`.`rnrinputuser` left join `m0_user` `u2` on `u2`.`userid` = `rnr`.`rnrmodifikasiuser` left join `m1_item` `i` on `i`.`bid` = `rnrd`.`idbarang` left join `m1_tax` `t1` on `rnrd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `rnrd`.`pajak2` = `t2`.`tkode` left join `m1_branch` `brd` on `rnrd`.`cabang` = `brd`.`bkode` left join `m1_location` `lcd` on `rnrd`.`lokasi` = `lcd`.`lkode` left join `m1_warehouse` `whd1` on `rnrd`.`gudangasal` = `whd1`.`wkode` left join `m1_warehouse` `whd2` on `rnrd`.`gudangtransit` = `whd2`.`wkode` left join `m1_warehouse` `whd3` on `rnrd`.`gudangtujuan` = `whd3`.`wkode` left join `m1_cost_center` `cc` on `rnrd`.`costcenter` = `cc`.`cckode` left join `m1_division` `d` on `rnrd`.`divisi` = `d`.`dkode` left join `m1_subdivision` `sd` on `rnrd`.`subdivisi` = `sd`.`sdkode` left join `m1_project` `p` on `rnrd`.`proyek` = `p`.`pkode` left join `m5_pi_detail` `pid` on `rnrd`.`idpidetail` = `pid`.`idpidetail` left join `m5_pi` `pi2` on `pid`.`idpi` = `pi2`.`piid` left join `m5_si_detail` `sid` on `rnrd`.`idsidetail` = `sid`.`idsidetail` left join `m5_si` `si2` on `sid`.`idsi` = `si2`.`siid`"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("rnrid"), 0), sptField,
                     FxDB(drutama("rnrcabang"), ""), sptField,
                     FxDB(drutama("rnrlokasi"), ""), sptField,
                     FxDB(drutama("rnrgudang"), ""), sptField,
                     FxDB(drutama("rnrasalbarang"), ""), sptField,
                     FxDB(drutama("rnrasalbarangkategori"), 0), sptField,
                     FxDB(drutama("rnrjenispenjualan"), ""), sptField,
                     FxDB(drutama("rnrjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("rnrcarabayar"), 0), sptField,
                     FxDB(drutama("rnrsumber"), ""), sptField,
                     FxDB(drutama("rnrautonotransaksi"), 0), sptField,
                     FxDB(drutama("rnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrkodepa"), 0), sptField,
                     FxDB(drutama("rnrcustomer"), 0), sptField,
                     FxDB(drutama("rnrcustomerkontak"), ""), sptField,
                     FxDB(drutama("rnr1alamat1"), ""), sptField,
                     FxDB(drutama("rnr1alamat2"), ""), sptField,
                     FxDB(drutama("rnr1alamat3"), ""), sptField,
                     FxDB(drutama("rnr2alamat1"), ""), sptField,
                     FxDB(drutama("rnr2alamat2"), ""), sptField,
                     FxDB(drutama("rnr2alamat3"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualan"), 0), sptField,
                     FxDB(drutama("rnrekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("rnruraian"), ""), sptField,
                     FxDB(drutama("rnrcatatan"), ""), sptField,
                     FxDB(drutama("rnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrmatauang"), ""), sptField,
                     FxDB(drutama("rnrkurs"), 0), sptField,
                     FxDB(drutama("rnrhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("rnrtotal"), 0), sptField,
                     FxDB(drutama("rnrdiskonpersen"), ""), sptField,
                     FxDB(drutama("rnrjmldiskon"), 0), sptField,
                     FxDB(drutama("rnrtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("rnrtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("rnrbiayalainpersen"), 0), sptField,
                     FxDB(drutama("rnrbiayalain"), 0), sptField,
                     FxDB(drutama("rnrtotaltransaksi"), 0), sptField,
                     FxDB(drutama("rnrjmlbayar"), 0), sptField,
                     FxDB(drutama("rnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrnofakturpajak"), ""), sptField,
                     FxDB(drutama("rnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrrekdiskon"), ""), sptField,
                     FxDB(drutama("rnrrekpajak1"), ""), sptField,
                     FxDB(drutama("rnrrekpajak2"), ""), sptField,
                     FxDB(drutama("rnrrekbiayalain"), ""), sptField,
                     FxDB(drutama("rnrrekbayar"), ""), sptField,
                     FxDB(drutama("rnridsq"), 0), sptField,
                     FxDB(drutama("rnridso"), 0), sptField,
                     FxDB(drutama("rnridpl"), 0), sptField,
                     FxDB(drutama("rnriddo"), 0), sptField,
                     FxDB(drutama("rnriddr"), 0), sptField,
                     FxDB(drutama("rnridpi"), 0), sptField,
                     FxDB(drutama("rnridsi"), 0), sptField,
                     FxDB(drutama("rnrstatussr"), 0), sptField,
                     FxDB(drutama("rnrstatusrealisasi"), 0), sptField,
                     FxDB(drutama("rnrstatus"), 0), sptField,
                     FxDB(drutama("rnrstatussebelumnya"), 0), sptField,
                     FxDB(drutama("rnrjmlrevisi"), 0), sptField,
                     FxDB(drutama("rnrcetakanke"), 0), sptField,
                     FxDB(drutama("rnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("rnrtutupperiode"), 0), sptField,
                     FxDB(drutama("rnrisclose"), 0), sptField,
                     FxDB(drutama("rnrcustomtext1"), ""), sptField,
                     FxDB(drutama("rnrcustomtext2"), ""), sptField,
                     FxDB(drutama("rnrcustomtext3"), ""), sptField,
                     FxDB(drutama("rnrcustomtext4"), ""), sptField,
                     FxDB(drutama("rnrcustomtext5"), ""), sptField,
                     FxDB(drutama("rnrcustomint1"), 0), sptField,
                     FxDB(drutama("rnrcustomint2"), 0), sptField,
                     FxDB(drutama("rnrcustomint3"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl1"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl2"), 0), sptField,
                     FxDB(drutama("rnrcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("rnrcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("rnrcabangnama"), ""), sptField,
                     FxDB(drutama("rnrlokasinama"), ""), sptField,
                     FxDB(drutama("rnrgudangnama"), ""), sptField,
                     FxDB(drutama("rnrcustomerkode"), ""), sptField,
                     FxDB(drutama("rnrcustomernama"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("rnrbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("rnrekspedisinama"), ""), sptField,
                     FxDB(drutama("rnrterminnama"), ""), sptField,
                     FxDB(drutama("rnrterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("rnrrekdiskonnama"), ""), sptField,
                     FxDB(drutama("rnrrekpajak1nama"), ""), sptField,
                     FxDB(drutama("rnrrekpajak2nama"), ""), sptField,
                     FxDB(drutama("rnrrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("rnrrekbayarnama"), ""), sptField,
                     FxDB(drutama("rnrnotransaksipi"), ""), sptField,
                     FxDB(drutama("rnrnotransaksisi"), ""), sptField,
                     FxDB(drutama("rnrstatusnama"), ""), sptField,
                     FxDB(drutama("rnrstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("rnrinputusernama"), ""), sptField,
                     FxDB(drutama("rnrmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idrnrdetail"), 0), sptField,
                     FxDB(dr("idrnr"), 0), sptField,
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
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("idhppfifokeluar"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hargapricelist"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
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
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekreturpenjualan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idsidetail"), 0), sptField,
                     FxDB(dr("jmlsr"), 0), sptField,
                     FxDB(dr("statussr"), 0), sptField,
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
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3, rnrcabangnama, rnrlokasinama, rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, rnrterminnama, rnrterminharijatuhtempo, rnrrekdiskonnama, rnrrekpajak1nama, rnrrekpajak2nama, rnrrekbiayalainnama, rnrrekbayarnama, rnrnotransaksipi, rnrnotransaksisi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama, ktingkatjual, kpkp" & sptSubParam & "idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, pinotransaksi, sinotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RnrSearch(ByVal param As String) As String
        'M5_RnrSearch --------------------------------------------------------
        'rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, 
        'rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, 
        'rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcabangnama, rnrlokasinama, 
        'rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, donotransaksi, 
        'drnotransaksi, pinotransaksi, sinotransaksi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama

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
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_rnr_v")
        sql = "select `rnr`.`rnrid` AS `rnrid`,`rnr`.`rnrcabang` AS `rnrcabang`,`rnr`.`rnrlokasi` AS `rnrlokasi`,`rnr`.`rnrgudang` AS `rnrgudang`,`rnr`.`rnrasalbarang` AS `rnrasalbarang`,`rnr`.`rnrasalbarangkategori` AS `rnrasalbarangkategori`,`rnr`.`rnrjenispenjualan` AS `rnrjenispenjualan`,`rnr`.`rnrjenispenjualankategori` AS `rnrjenispenjualankategori`,`rnr`.`rnrcarabayar` AS `rnrcarabayar`,`rnr`.`rnrsumber` AS `rnrsumber`,`rnr`.`rnrautonotransaksi` AS `rnrautonotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`rnr`.`rnrtgl` AS `rnrtgl`,`rnr`.`rnrkodepa` AS `rnrkodepa`,`rnr`.`rnrcustomer` AS `rnrcustomer`,`rnr`.`rnrcustomerkontak` AS `rnrcustomerkontak`,`rnr`.`rnr1alamat1` AS `rnr1alamat1`,`rnr`.`rnr1alamat2` AS `rnr1alamat2`,`rnr`.`rnr1alamat3` AS `rnr1alamat3`,`rnr`.`rnr2alamat1` AS `rnr2alamat1`,`rnr`.`rnr2alamat2` AS `rnr2alamat2`,`rnr`.`rnr2alamat3` AS `rnr2alamat3`,`rnr`.`rnrbagianpenjualan` AS `rnrbagianpenjualan`,`rnr`.`rnrekspedisi` AS `rnrekspedisi`,`rnr`.`rnrtglkirim` AS `rnrtglkirim`,`rnr`.`rnrtermin` AS `rnrtermin`,`rnr`.`rnrtgljatuhtempo` AS `rnrtgljatuhtempo`,`rnr`.`rnruraian` AS `rnruraian`,`rnr`.`rnrcatatan` AS `rnrcatatan`,`rnr`.`rnrnoref` AS `rnrnoref`,`rnr`.`rnrtglnoref` AS `rnrtglnoref`,`rnr`.`rnrtglpenutupan` AS `rnrtglpenutupan`,`rnr`.`rnrmatauang` AS `rnrmatauang`,`rnr`.`rnrkurs` AS `rnrkurs`,`rnr`.`rnrhargatermasukpajak` AS `rnrhargatermasukpajak`,`rnr`.`rnrtotal` AS `rnrtotal`,`rnr`.`rnrdiskonpersen` AS `rnrdiskonpersen`,`rnr`.`rnrjmldiskon` AS `rnrjmldiskon`,`rnr`.`rnrtotalpajak1detail` AS `rnrtotalpajak1detail`,`rnr`.`rnrtotalpajak2detail` AS `rnrtotalpajak2detail`,`rnr`.`rnrbiayalainpersen` AS `rnrbiayalainpersen`,`rnr`.`rnrbiayalain` AS `rnrbiayalain`,`rnr`.`rnrtotaltransaksi` AS `rnrtotaltransaksi`,`rnr`.`rnrjmlbayar` AS `rnrjmlbayar`,`rnr`.`rnrstatuslunas` AS `rnrstatuslunas`,`rnr`.`rnrtgllunas` AS `rnrtgllunas`,`rnr`.`rnrnofakturpajak` AS `rnrnofakturpajak`,`rnr`.`rnrsdhbayarpajak` AS `rnrsdhbayarpajak`,`rnr`.`rnrtglbayarpajak` AS `rnrtglbayarpajak`,`rnr`.`rnrrekdiskon` AS `rnrrekdiskon`,`rnr`.`rnrrekpajak1` AS `rnrrekpajak1`,`rnr`.`rnrrekpajak2` AS `rnrrekpajak2`,`rnr`.`rnrrekbiayalain` AS `rnrrekbiayalain`,`rnr`.`rnrrekbayar` AS `rnrrekbayar`,`rnr`.`rnridsq` AS `rnridsq`,`rnr`.`rnridso` AS `rnridso`,`rnr`.`rnridpl` AS `rnridpl`,`rnr`.`rnriddo` AS `rnriddo`,`rnr`.`rnriddr` AS `rnriddr`,`rnr`.`rnridpi` AS `rnridpi`,`rnr`.`rnridsi` AS `rnridsi`,`rnr`.`rnrstatussr` AS `rnrstatussr`,`rnr`.`rnrstatusrealisasi` AS `rnrstatusrealisasi`,`rnr`.`rnrstatus` AS `rnrstatus`,`rnr`.`rnrstatussebelumnya` AS `rnrstatussebelumnya`,`rnr`.`rnrjmlrevisi` AS `rnrjmlrevisi`,`rnr`.`rnrcetakanke` AS `rnrcetakanke`,`rnr`.`rnrinputuser` AS `rnrinputuser`,`rnr`.`rnrinputtgl` AS `rnrinputtgl`,`rnr`.`rnrmodifikasiuser` AS `rnrmodifikasiuser`,`rnr`.`rnrmodifikasitgl` AS `rnrmodifikasitgl`,`rnr`.`rnrposting` AS `rnrposting`,`rnr`.`rnrpostingtgl` AS `rnrpostingtgl`,`rnr`.`rnrtutupperiode` AS `rnrtutupperiode`,`rnr`.`rnrisclose` AS `rnrisclose`,`br`.`bnama` AS `rnrcabangnama`,`lc`.`lnama` AS `rnrlokasinama`,`wh`.`wnama` AS `rnrgudangnama`,`c1`.`kkode` AS `rnrcustomerkode`,`c1`.`knama` AS `rnrcustomernama`,`c2`.`kkode` AS `rnrbagianpenjualankode`,`c2`.`knama` AS `rnrbagianpenjualannama`,`e`.`enama` AS `rnrekspedisinama`,`do`.`donotransaksi` AS `donotransaksi`,`dr`.`drnotransaksi` AS `drnotransaksi`,`pi`.`pinotransaksi` AS `pinotransaksi`,`si`.`sinotransaksi` AS `sinotransaksi`,`st1`.`nama` AS `rnrstatusnama`,`st2`.`nama` AS `rnrstatussebelumnyanama`,`u1`.`unama` AS `rnrinputusernama`,`u2`.`unama` AS `rnrmodifikasiusernama` from ((((((((((((((`m5_rnr` `rnr` left join `m1_branch` `br` on((`br`.`bkode` = `rnr`.`rnrcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `rnr`.`rnrlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `rnr`.`rnrgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `rnr`.`rnrcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `rnr`.`rnrbagianpenjualan`))) left join `m1_expedition` `e` on((`rnr`.`rnrekspedisi` = `e`.`ekode`))) left join `m5_do` `do` on((`rnr`.`rnriddo` = `do`.`doid`))) left join `m5_dr` `dr` on((`rnr`.`rnriddr` = `dr`.`drid`))) left join `m5_pi` `pi` on((`rnr`.`rnridpi` = `pi`.`piid`))) left join `m5_si` `si` on((`rnr`.`rnridsi` = `si`.`siid`))) left join `m0_status` `st1` on((`st1`.`kode` = `rnr`.`rnrstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `rnr`.`rnrstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `rnr`.`rnrinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `rnr`.`rnrmodifikasiuser`)))"

        dt = AmbilData("aplikasi1-M5_rnr_V", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("rnrid"), 0), sptField,
                     FxDB(dr("rnrcabang"), ""), sptField,
                     FxDB(dr("rnrlokasi"), ""), sptField,
                     FxDB(dr("rnrgudang"), ""), sptField,
                     FxDB(dr("rnrasalbarang"), ""), sptField,
                     FxDB(dr("rnrasalbarangkategori"), 0), sptField,
                     FxDB(dr("rnrjenispenjualan"), ""), sptField,
                     FxDB(dr("rnrjenispenjualankategori"), 0), sptField,
                     FxDB(dr("rnrcarabayar"), 0), sptField,
                     FxDB(dr("rnrsumber"), ""), sptField,
                     FxDB(dr("rnrautonotransaksi"), 0), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgl"), ""), formatTgl), sptField,
                     FxDB(dr("rnrkodepa"), 0), sptField,
                     FxDB(dr("rnrcustomer"), 0), sptField,
                     FxDB(dr("rnrcustomerkontak"), ""), sptField,
                     FxDB(dr("rnr1alamat1"), ""), sptField,
                     FxDB(dr("rnr1alamat2"), ""), sptField,
                     FxDB(dr("rnr1alamat3"), ""), sptField,
                     FxDB(dr("rnr2alamat1"), ""), sptField,
                     FxDB(dr("rnr2alamat2"), ""), sptField,
                     FxDB(dr("rnr2alamat3"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualan"), 0), sptField,
                     FxDB(dr("rnrekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("rnrtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("rnruraian"), ""), sptField,
                     FxDB(dr("rnrcatatan"), ""), sptField,
                     FxDB(dr("rnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("rnrmatauang"), ""), sptField,
                     FxDB(dr("rnrkurs"), 0), sptField,
                     FxDB(dr("rnrhargatermasukpajak"), 0), sptField,
                     FxDB(dr("rnrtotal"), 0), sptField,
                     FxDB(dr("rnrdiskonpersen"), ""), sptField,
                     FxDB(dr("rnrjmldiskon"), 0), sptField,
                     FxDB(dr("rnrtotalpajak1detail"), 0), sptField,
                     FxDB(dr("rnrtotalpajak2detail"), 0), sptField,
                     FxDB(dr("rnrbiayalainpersen"), 0), sptField,
                     FxDB(dr("rnrbiayalain"), 0), sptField,
                     FxDB(dr("rnrtotaltransaksi"), 0), sptField,
                     FxDB(dr("rnrjmlbayar"), 0), sptField,
                     FxDB(dr("rnrstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("rnrnofakturpajak"), ""), sptField,
                     FxDB(dr("rnrsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("rnrrekdiskon"), ""), sptField,
                     FxDB(dr("rnrrekpajak1"), ""), sptField,
                     FxDB(dr("rnrrekpajak2"), ""), sptField,
                     FxDB(dr("rnrrekbiayalain"), ""), sptField,
                     FxDB(dr("rnrrekbayar"), ""), sptField,
                     FxDB(dr("rnridsq"), 0), sptField,
                     FxDB(dr("rnridso"), 0), sptField,
                     FxDB(dr("rnridpl"), 0), sptField,
                     FxDB(dr("rnriddo"), 0), sptField,
                     FxDB(dr("rnriddr"), 0), sptField,
                     FxDB(dr("rnridpi"), 0), sptField,
                     FxDB(dr("rnridsi"), 0), sptField,
                     FxDB(dr("rnrstatussr"), 0), sptField,
                     FxDB(dr("rnrstatusrealisasi"), 0), sptField,
                     FxDB(dr("rnrstatus"), 0), sptField,
                     FxDB(dr("rnrstatussebelumnya"), 0), sptField,
                     FxDB(dr("rnrjmlrevisi"), 0), sptField,
                     FxDB(dr("rnrcetakanke"), 0), sptField,
                     FxDB(dr("rnrinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("rnrtutupperiode"), 0), sptField,
                     FxDB(dr("rnrisclose"), 0), sptField,
                     FxDB(dr("rnrcabangnama"), ""), sptField,
                     FxDB(dr("rnrlokasinama"), ""), sptField,
                     FxDB(dr("rnrgudangnama"), ""), sptField,
                     FxDB(dr("rnrcustomerkode"), ""), sptField,
                     FxDB(dr("rnrcustomernama"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualankode"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualannama"), ""), sptField,
                     FxDB(dr("rnrekspedisinama"), ""), sptField,
                     FxDB(dr("donotransaksi"), ""), sptField,
                     FxDB(dr("drnotransaksi"), ""), sptField,
                     FxDB(dr("pinotransaksi"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrstatusnama"), ""), sptField,
                     FxDB(dr("rnrstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("rnrinputusernama"), ""), sptField,
                     FxDB(dr("rnrmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatusrealisasi, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrpostingtgl, rnrtutupperiode, rnrisclose, rnrcabangnama, rnrlokasinama, rnrgudangnama, rnrcustomerkode, rnrcustomernama, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisinama, donotransaksi, drnotransaksi, pinotransaksi, sinotransaksi, rnrstatusnama, rnrstatussebelumnyanama, rnrinputusernama, rnrmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_Rnr_Detail_VSearch(ByVal param As String) As String
        'M5_Rnr_Detail_VSearch --------------------------------------------------------
        'idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, 
        'harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, 
        'iddrdetail, idpidetail, idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, rnrnotransaksi, rnruraian, rnrcatatan, rnrnoref, 
        'rnrtglnoref, rnrtglkirim, rnrnofakturpajak, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, 
        'rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisi, 
        'rnrekspedisinama, rnrtermin, rnrterminnama, rnrterminharijatuhtempo, kodebarang, bhpp, bjenis, 
        'bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisasr, 
        'jmlsisarealisasi, rnrcustomer, rnrcustomerkode, rnrcustomernama, rnrdiskonpersen, rnrbiayalainpersen, 
        'bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, rnrhargatermasukpajak, rnrtgljatuhtempo, kpkp,
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

        Dim rnrl As String = ""

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'rnrl = query.PanggilQuery("m5_rnr_detail_v")
        rnrl = "select `rnrd`.`idrnrdetail` AS `idrnrdetail`, `rnrd`.`idrnr` AS `idrnr`, `rnrd`.`idbarang` AS `idbarang`, `rnrd`.`namabarang` AS `namabarang`, `rnrd`.`tipebarang` AS `tipebarang`, `rnrd`.`jml` AS `jml`, `rnrd`.`satuan` AS `satuan`, `rnrd`.`nilaisatuan` AS `nilaisatuan`, `rnrd`.`jmlbarang` AS `jmlbarang`, `rnrd`.`satuanbarang` AS `satuanbarang`, `rnrd`.`matauang` AS `matauang`, `rnrd`.`kurs` AS `kurs`, ifnull(`cso`.`idhppikk`, `rnrd`.`idhppkhususkeluar`) AS `idhppkhususkeluar`, `rnrd`.`idhppfifokeluar` AS `idhppfifokeluar`, `rnrd`.`harga` AS `harga`, `rnrd`.`hargapricelist` AS `hargapricelist`, `rnrd`.`hpp` AS `hpp`, `rnrd`.`diskon` AS `diskon`, `rnrd`.`jmldiskon` AS `jmldiskon`, `rnrd`.`pajak1` AS `pajak1`, `rnrd`.`jmlpajak1` AS `jmlpajak1`, `rnrd`.`pajak2` AS `pajak2`, `rnrd`.`jmlpajak2` AS `jmlpajak2`, `rnrd`.`cabang` AS `cabang`, `rnrd`.`lokasi` AS `lokasi`, `rnrd`.`gudangasal` AS `gudangasal`, `rnrd`.`gudangtransit` AS `gudangtransit`, `rnrd`.`gudangtujuan` AS `gudangtujuan`, `i`.`brekpersediaan` AS `rekpersediaan`, `rnrd`.`rekhargapokok` AS `rekhargapokok`, `rnrd`.`rekdiskonpenjualan` AS `rekdiskonpenjualan`, `rnrd`.`rekreturpenjualan` AS `rekreturpenjualan`, `rnrd`.`costcenter` AS `costcenter`, `rnrd`.`divisi` AS `divisi`, `rnrd`.`subdivisi` AS `subdivisi`, `rnrd`.`proyek` AS `proyek`, `rnrd`.`catatan` AS `catatan`, `rnrd`.`urutan` AS `urutan`, `rnrd`.`idsqdetail` AS `idsqdetail`, `rnrd`.`idsodetail` AS `idsodetail`, `rnrd`.`idpldetail` AS `idpldetail`, `rnrd`.`iddodetail` AS `iddodetail`, `rnrd`.`iddrdetail` AS `iddrdetail`, `rnrd`.`idpidetail` AS `idpidetail`, `rnrd`.`idsidetail` AS `idsidetail`, `rnrd`.`jmlsr` AS `jmlsr`, `rnrd`.`statussr` AS `statussr`, `rnrd`.`jmlrealisasi` AS `jmlrealisasi`, `rnrd`.`statusrealisasi` AS `statusrealisasi`, `rnrd`.`isclose` AS `isclose`, `rnrd`.`customtext1` AS `customtext1`, `rnrd`.`customtext2` AS `customtext2`, `rnrd`.`customtext3` AS `customtext3`, `rnrd`.`customdbl1` AS `customdbl1`, `rnrd`.`customdbl2` AS `customdbl2`, `rnrd`.`customdbl3` AS `customdbl3`, `rnrd`.`customdate1` AS `customdate1`, `rnrd`.`customdate2` AS `customdate2`, `rnrd`.`customdate3` AS `customdate3`, `rnr`.`rnrnotransaksi` AS `rnrnotransaksi`, `rnr`.`rnruraian` AS `rnruraian`, `rnr`.`rnrcatatan` AS `rnrcatatan`, `rnr`.`rnrnoref` AS `rnrnoref`, `rnr`.`rnrtglnoref` AS `rnrtglnoref`, `rnr`.`rnrtglkirim` AS `rnrtglkirim`, `rnr`.`rnrnofakturpajak` AS `rnrnofakturpajak`, `rnr`.`rnrcustomerkontak` AS `rnrcustomerkontak`, `rnr`.`rnr1alamat1` AS `rnr1alamat1`, `rnr`.`rnr1alamat2` AS `rnr1alamat2`, `rnr`.`rnr1alamat3` AS `rnr1alamat3`, `rnr`.`rnr2alamat1` AS `rnr2alamat1`, `rnr`.`rnr2alamat2` AS `rnr2alamat2`, `rnr`.`rnr2alamat3` AS `rnr2alamat3`, `rnr`.`rnrbagianpenjualan` AS `rnrbagianpenjualan`, `c1`.`kkode` AS `rnrbagianpenjualankode`, `c1`.`knama` AS `rnrbagianpenjualannama`, `rnr`.`rnrekspedisi` AS `rnrekspedisi`, `e`.`enama` AS `rnrekspedisinama`, `rnr`.`rnrtermin` AS `rnrtermin`, `tr`.`trnama` AS `rnrterminnama`, `tr`.`trharijatuhtempo` AS `rnrterminharijatuhtempo`, `i`.`bkode` AS `kodebarang`, `i`.`bhpp` AS `bhpp`, `i`.`bjenis` AS `bjenis`, `i`.`bserial` AS `bserial`, `i`.`bbatch` AS `bbatch`, `t1`.`tnama` AS `pajak1nama`, `t1`.`tnilai` AS `pajak1nilai`, `t2`.`tnama` AS `pajak2nama`, `t2`.`tnilai` AS `pajak2nilai`, ((`rnrd`.`jmlbarang` - `rnrd`.`jmlsr`) / `rnrd`.`nilaisatuan`) AS `jmlsisasr`, ((`rnrd`.`jmlbarang` - `rnrd`.`jmlrealisasi`) / `rnrd`.`nilaisatuan`) AS `jmlsisarealisasi`, rnr.rnrcustomer, c2.kkode as rnrcustomerkode, c2.knama as rnrcustomernama, rnr.rnrdiskonpersen, rnr.rnrbiayalainpersen, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.basset, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, rnr.rnrhargatermasukpajak, rnr.rnrtgljatuhtempo, c2.kpkp from `m5_rnr_detail` `rnrd` left join `m5_rnr` `rnr` on `rnrd`.`idrnr` = `rnr`.`rnrid` left join `m5_si_detail` `sid` on `rnrd`.`idsidetail` = `sid`.`idsidetail` left join `m1_cogs_special_out` `cso` on (`sid`.`idsidetail` = `cso`.`idtransaksi`) and (`cso`.`sumber` = 'SI') left join `m1_terms` `tr` on `rnr`.`rnrtermin` = `tr`.`trkode` left join `m1_expedition` `e` on `rnr`.`rnrekspedisi` = `e`.`ekode` left join `m1_contact` `c1` on `rnr`.`rnrbagianpenjualan` = `c1`.`kid` left join `m1_contact` `c2` on `rnr`.`rnrcustomer` = `c2`.`kid` left join `m1_item` `i` on `rnrd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `rnrd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `rnrd`.`pajak2` = `t2`.`tkode` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        dt = AmbilData("aplikasi1-M5_rnr_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , rnrl) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idrnrdetail"), 0), sptField,
                     FxDB(dr("idrnr"), 0), sptField,
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
                     FxDB(dr("idhppkhususkeluar"), 0), sptField,
                     FxDB(dr("idhppfifokeluar"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("hargapricelist"), 0), sptField,
                     FxDB(dr("hpp"), 0), sptField,
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
                     FxDB(dr("rekhargapokok"), ""), sptField,
                     FxDB(dr("rekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("rekreturpenjualan"), ""), sptField,
                     FxDB(dr("costcenter"), ""), sptField,
                     FxDB(dr("divisi"), ""), sptField,
                     FxDB(dr("subdivisi"), ""), sptField,
                     FxDB(dr("proyek"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("urutan"), 0), sptField,
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idpldetail"), 0), sptField,
                     FxDB(dr("iddodetail"), 0), sptField,
                     FxDB(dr("iddrdetail"), 0), sptField,
                     FxDB(dr("idpidetail"), 0), sptField,
                     FxDB(dr("idsidetail"), 0), sptField,
                     FxDB(dr("jmlsr"), 0), sptField,
                     FxDB(dr("statussr"), 0), sptField,
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
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     FxDB(dr("rnruraian"), ""), sptField,
                     FxDB(dr("rnrcatatan"), ""), sptField,
                     FxDB(dr("rnrnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("rnrnofakturpajak"), ""), sptField,
                     FxDB(dr("rnrcustomerkontak"), ""), sptField,
                     FxDB(dr("rnr1alamat1"), ""), sptField,
                     FxDB(dr("rnr1alamat2"), ""), sptField,
                     FxDB(dr("rnr1alamat3"), ""), sptField,
                     FxDB(dr("rnr2alamat1"), ""), sptField,
                     FxDB(dr("rnr2alamat2"), ""), sptField,
                     FxDB(dr("rnr2alamat3"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualan"), 0), sptField,
                     FxDB(dr("rnrbagianpenjualankode"), ""), sptField,
                     FxDB(dr("rnrbagianpenjualannama"), ""), sptField,
                     FxDB(dr("rnrekspedisi"), ""), sptField,
                     FxDB(dr("rnrekspedisinama"), ""), sptField,
                     FxDB(dr("rnrtermin"), ""), sptField,
                     FxDB(dr("rnrterminnama"), ""), sptField,
                     FxDB(dr("rnrterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisasr"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("rnrcustomer"), ""), sptField,
                     FxDB(dr("rnrcustomerkode"), ""), sptField,
                     FxDB(dr("rnrcustomernama"), ""), sptField,
                     FxDB(dr("rnrdiskonpersen"), 0), sptField,
                     FxDB(dr("rnrbiayalainpersen"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("rnrhargatermasukpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("rnrtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rnrnotransaksi, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglkirim, rnrnofakturpajak, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrbagianpenjualankode, rnrbagianpenjualannama, rnrekspedisi, rnrekspedisinama, rnrtermin, rnrterminnama, rnrterminharijatuhtempo, kodebarang, bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisasr, jmlsisarealisasi, rnrcustomer, rnrcustomerkode, rnrcustomernama, rnrdiskonpersen, rnrbiayalainpersen, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, basset, rnrhargatermasukpajak, rnrtgljatuhtempo, kpkp, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_RnrTerkait(ByVal param As String) As String
        'M5_RnrTerkait --------------------------------------------------------
        'rnrid, rnrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "rnrid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND rnrid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "rnrid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = m5_rnr_terkait(Filter)

        dt = AmbilData("aplikasi1-m5_rnr_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each rnr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(rnr("rnrid"), 0), sptField,
                     FxDB(rnr("rnrnotransaksi"), ""), sptField,
                     FxDB(rnr("sumber"), ""), sptField,
                     FxDB(rnr("idterkait"), 0), sptField,
                     FxDB(rnr("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(rnr("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(rnr("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(rnr("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(rnr("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related RNR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("rnrid, rnrnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingSI As String, ByVal ftOutstandingSI As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftSI As String, ByRef termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = "", gudang As String = ""

        'VALIDASI OUTSTANDING ---------------------------------------
        'SI
        If Len(ftExistOutstandingSI) > 0 Then 'ftExistOutstanding = rowExists, idsidetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingSI)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idsidetail=" & dtval.Rows(0)("idsidetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SI" : GoTo selesai
            End If
        End If

        'CEK SI YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftSI) > 0 Then
            sql = "SELECT si.sinotransaksi as notransaksi, si.sihargatermasukpajak as termasukpajak, (CASE si.sihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajaknama FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid WHERE " & ftSI & " GROUP BY si.sihargatermasukpajak"
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

            'CEK TRANSAKSI HARGA TERMASUK PAJAK TIDAK BOLEH AMBIL TRANSAKSI HARGA TIDAK TERMASUK PAJAK, DAN SEBALIKNYA
            If Len(termasukPajak) > 0 Then
                sql = "SELECT i.bkode, sid.idsidetail, si.sinotransaksi as notransaksi, (CASE si.sihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid JOIN m1_item i ON sid.idbarang = i.bid WHERE (" & ftSI & ") AND si.sihargatermasukpajak <> " & termasukPajak & " ORDER BY sid.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idsidetail = " & dtval.Rows(0)("idsidetail")
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
        If Len(ftOutstandingSI) > 0 Then
            sql = "SELECT sid.idsidetail, (sid.jmlbarang - sid.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_si_detail AS sid INNER JOIN m1_item AS i ON sid.idbarang = i.bid WHERE " & ftOutstandingSI
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idsidetail=" & dtval.Rows(0)("idsidetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SI, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
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
    Public Function M5_RnrSimpanOld(ByVal param As String) As String
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
        'rnrid(0) As Integer, rnrcabang(1) As String, rnrlokasi(2) As String, rnrgudang(3) As String, rnrasalbarang(4) As String, 
        'rnrasalbarangkategori(5) As Integer, rnrjenispenjualan(6) As String, rnrjenispenjualankategori(7) As Integer, rnrcarabayar(8) As Integer, rnrsumber(9) As String, 
        'rnrautonotransaksi(10) As Integer, rnrnotransaksi(11) As String, rnrtgl(12) As Date, rnrkodepa(13) As Integer, rnrcustomer(14) As Integer, 
        'rnrcustomerkontak(15) As String, rnr1alamat1(16) As String, rnr1alamat2(17) As String, rnr1alamat3(18) As String, rnr2alamat1(19) As String, 
        'rnr2alamat2(20) As String, rnr2alamat3(21) As String, rnrbagianpenjualan(22) As Integer, rnrekspedisi(23) As String, rnrtglkirim(24) As Date, 
        'rnrtermin(25) As String, rnrtgljatuhtempo(26) As Date, rnruraian(27) As String, rnrcatatan(28) As String, rnrnoref(29) As String, 
        'rnrtglnoref(30) As Date, rnrtglpenutupan(31) As Date, rnrmatauang(32) As String, rnrkurs(33) As Double, rnrhargatermasukpajak(34) As Integer, 
        'rnrtotal(35) As Double, rnrdiskonpersen(36) As String, rnrjmldiskon(37) As Double, rnrtotalpajak1detail(38) As Double, rnrtotalpajak2detail(39) As Double, 
        'rnrbiayalainpersen(40) As Double, rnrbiayalain(41) As Double, rnrtotaltransaksi(42) As Double, rnrjmlbayar(43) As Double, rnrstatuslunas(44) As Integer, 
        'rnrtgllunas(45) As Date, rnrnofakturpajak(46) As String, rnrsdhbayarpajak(47) As Integer, rnrtglbayarpajak(48) As Date, rnrrekdiskon(49) As String, 
        'rnrrekpajak1(50) As String, rnrrekpajak2(51) As String, rnrrekbiayalain(52) As String, rnrrekbayar(53) As String, rnridsq(54) As Integer, 
        'rnridso(55) As Integer, rnridpl(56) As Integer, rnriddo(57) As Integer, rnriddr(58) As Integer, rnridpi(59) As Integer, 
        'rnridsi(60) As Integer, rnrstatussr(61) As Integer, rnrstatus(62) As Integer, rnrstatussebelumnya(63) As Integer, rnrjmlrevisi(64) As Integer, 
        'rnrcetakanke(65) As Integer, rnrinputuser(66) As Integer, rnrinputtgl(67) As DateTime, rnrmodifikasiuser(68) As Integer, rnrmodifikasitgl(69) As DateTime, 
        'rnrposting(70) As Integer, rnrtutupperiode(71) As Integer, rnrisclose(72) As Integer, rnrcustomtext1(73) As String, rnrcustomtext2(74) As String, 
        'rnrcustomtext3(75) As String, rnrcustomtext4(76) As String, rnrcustomtext5(77) As String, rnrcustomint1(78) As Integer, rnrcustomint2(79) As Integer, 
        'rnrcustomint3(80) As Integer, rnrcustomdbl1(81) As Double, rnrcustomdbl2(82) As Double, rnrcustomdbl3(83) As Double, rnrcustomdate1(84) As Date, 
        'rnrcustomdate2(85) As Date, rnrcustomdate3(86) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'rnrid, rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, 
        'rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, 
        'rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, 
        'rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, 
        'rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, 
        'rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, 
        'rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, 
        'rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, 
        'rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatus, 
        'rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, 
        'rnrposting, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, 
        'rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, 
        'rnrcustomdate1, rnrcustomdate2, rnrcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 87) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'rnrid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "rnrid required numeric." : GoTo selesai
        End If
        'rnrasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "rnrasalbarangkategori required numeric." : GoTo selesai
        End If
        'rnrjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "rnrjenispenjualankategori required numeric." : GoTo selesai
        End If
        'rnrcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "rnrcarabayar required numeric." : GoTo selesai
        End If
        'rnrautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "rnrautonotransaksi required numeric." : GoTo selesai
        End If
        'rnrtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "rnrtgl required date." : GoTo selesai
        End If
        'rnrkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "rnrkodepa required numeric." : GoTo selesai
        End If
        'rnrcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "rnrcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "rnrcustomer can't be empty." : GoTo selesai
        End If
        'rnrbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "rnrbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "rnrbagianpenjualan can't be empty." : GoTo selesai
        End If
        'rnrtglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "rnrtglkirim required date." : GoTo selesai
        End If
        'rnrtgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "rnrtgljatuhtempo required date." : GoTo selesai
        End If
        'rnrtglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "rnrtglnoref required date." : GoTo selesai
        End If
        'rnrtglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "rnrtglpenutupan required date." : GoTo selesai
        End If
        'rnrkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "rnrkurs required numeric." : GoTo selesai
        End If
        'rnrhargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "rnrhargatermasukpajak required numeric." : GoTo selesai
        End If
        'rnrtotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "rnrtotal required numeric." : GoTo selesai
        End If
        'rnrjmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "rnrjmldiskon required numeric." : GoTo selesai
        End If
        'rnrtotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "rnrtotalpajak1detail required numeric." : GoTo selesai
        End If
        'rnrtotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "rnrtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''rnrbiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "rnrbiayalainpersen required numeric." : GoTo selesai
        'End If
        'rnrbiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "rnrbiayalain required numeric." : GoTo selesai
        End If
        'rnrtotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "rnrtotaltransaksi required numeric." : GoTo selesai
        End If
        'rnrjmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "rnrjmlbayar required numeric." : GoTo selesai
        End If
        'rnrstatuslunas(44) As Integer
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "rnrstatuslunas required numeric." : GoTo selesai
        End If
        'rnrtgllunas(45) As Date
        If (IsDate(dataUtama(45)) = False) Then
            result(2) = "rnrtgllunas required date." : GoTo selesai
        End If
        'rnrsdhbayarpajak(47) As Integer
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "rnrsdhbayarpajak required numeric." : GoTo selesai
        End If
        'rnrtglbayarpajak(48) As Date
        If (IsDate(dataUtama(48)) = False) Then
            result(2) = "rnrtglbayarpajak required date." : GoTo selesai
        End If
        'rnridsq(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "rnridsq required numeric." : GoTo selesai
        End If
        'rnridso(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "rnridso required numeric." : GoTo selesai
        End If
        'rnridpl(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "rnridpl required numeric." : GoTo selesai
        End If
        'rnriddo(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "rnriddo required numeric." : GoTo selesai
        End If
        'rnriddr(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "rnriddr required numeric." : GoTo selesai
        End If
        'rnridpi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "rnridpi required numeric." : GoTo selesai
        End If
        'rnridsi(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "rnridsi required numeric." : GoTo selesai
        End If
        'rnrstatussr(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "rnrstatussr required numeric." : GoTo selesai
        End If
        'rnrstatus(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "rnrstatus required numeric." : GoTo selesai
        End If
        'rnrstatussebelumnya(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "rnrstatussebelumnya required numeric." : GoTo selesai
        End If
        'rnrjmlrevisi(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "rnrjmlrevisi required numeric." : GoTo selesai
        End If
        'rnrcetakanke(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "rnrcetakanke required numeric." : GoTo selesai
        End If
        'rnrinputuser(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "rnrinputuser required numeric." : GoTo selesai
        End If
        'rnrinputtgl(67) As DateTime
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "rnrinputtgl required date." : GoTo selesai
        End If
        'rnrmodifikasiuser(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "rnrmodifikasiuser required numeric." : GoTo selesai
        End If
        'rnrmodifikasitgl(69) As DateTime
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "rnrmodifikasitgl required date." : GoTo selesai
        End If
        'rnrposting(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "rnrposting required numeric." : GoTo selesai
        End If
        'rnrtutupperiode(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "rnrtutupperiode required numeric." : GoTo selesai
        End If
        'rnrisclose(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "rnrisclose required numeric." : GoTo selesai
        End If
        'rnrcustomint1(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "rnrcustomint1 required numeric." : GoTo selesai
        End If
        'rnrcustomint2(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "rnrcustomint2 required numeric." : GoTo selesai
        End If
        'rnrcustomint3(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "rnrcustomint3 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl1(81) As Double
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "rnrcustomdbl1 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl2(82) As Double
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "rnrcustomdbl2 required numeric." : GoTo selesai
        End If
        'rnrcustomdbl3(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "rnrcustomdbl3 required numeric." : GoTo selesai
        End If
        'rnrcustomdate1(84) As Date
        If (IsDate(dataUtama(84)) = False) Then
            result(2) = "rnrcustomdate1 required date." : GoTo selesai
        End If
        'rnrcustomdate2(85) As Date
        If (IsDate(dataUtama(85)) = False) Then
            result(2) = "rnrcustomdate2 required date." : GoTo selesai
        End If
        'rnrcustomdate3(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "rnrcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'rnrcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "rnrcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "rnrcabang should not be more than 25 character." : GoTo selesai
        End If

        'rnrlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "rnrlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "rnrlokasi should not be more than 25 character." : GoTo selesai
        End If

        'rnrgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "rnrgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "rnrgudang should not be more than 25 character." : GoTo selesai
        End If

        'rnrsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "rnrsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "rnrsumber should not be more than 10 character." : GoTo selesai
        End If

        'rnrnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "rnrnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "rnrnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'rnrtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "rnrtgl can't be empty" : GoTo selesai
        End If

        'rnrtglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "rnrtglkirim can't be empty" : GoTo selesai
        End If

        'rnrtgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "rnrtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'rnrtglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "rnrtglnoref can't be empty" : GoTo selesai
        End If

        'rnrtglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "rnrtglpenutupan can't be empty" : GoTo selesai
        End If

        'rnrmatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "rnrmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "rnrmatauang should not be more than 25 character." : GoTo selesai
        End If

        'rnrkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "rnrkurs can't be empty" : GoTo selesai
        End If

        'rnrtotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "rnrtotal can't be empty" : GoTo selesai
        End If

        'rnrdiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "rnrdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "rnrdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'rnrjmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "rnrjmldiskon can't be empty" : GoTo selesai
        End If

        'rnrtotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "rnrtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'rnrtotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "rnrtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'rnrbiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "rnrbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "rnrbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'rnrbiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "rnrbiayalain can't be empty" : GoTo selesai
        End If

        'rnrtotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "rnrtotaltransaksi can't be empty" : GoTo selesai
        End If

        'rnrjmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "rnrjmlbayar can't be empty" : GoTo selesai
        End If

        'rnrtgllunas(45) As Date
        If Len(dataUtama(45)) = 0 Then
            result(2) = "rnrtgllunas can't be empty" : GoTo selesai
        End If

        'rnrtglbayarpajak(48) As Date
        If Len(dataUtama(48)) = 0 Then
            result(2) = "rnrtglbayarpajak can't be empty" : GoTo selesai
        End If

        'rnrinputtgl(67) As DateTime
        If Len(dataUtama(67)) = 0 Then
            result(2) = "rnrinputtgl can't be empty" : GoTo selesai
        End If

        'rnrmodifikasitgl(69) As DateTime
        If Len(dataUtama(69)) = 0 Then
            result(2) = "rnrmodifikasitgl can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl1(81) As Double
        If Len(dataUtama(81)) = 0 Then
            result(2) = "rnrcustomdbl1 can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl2(82) As Double
        If Len(dataUtama(82)) = 0 Then
            result(2) = "rnrcustomdbl2 can't be empty" : GoTo selesai
        End If

        'rnrcustomdbl3(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "rnrcustomdbl3 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate1(84) As Date
        If Len(dataUtama(84)) = 0 Then
            result(2) = "rnrcustomdate1 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate2(85) As Date
        If Len(dataUtama(85)) = 0 Then
            result(2) = "rnrcustomdate2 can't be empty" : GoTo selesai
        End If

        'rnrcustomdate3(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "rnrcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "rnrid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrjenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnriddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnriddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnridsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "rnrcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "rnrcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "rnrid~rnrcabang~rnrlokasi~rnrgudang~rnrasalbarang~rnrasalbarangkategori~rnrjenispenjualan~rnrjenispenjualankategori~rnrcarabayar~rnrsumber~rnrautonotransaksi~rnrnotransaksi~rnrtgl~rnrkodepa~rnrcustomer~rnrcustomerkontak~rnr1alamat1~rnr1alamat2~rnr1alamat3~rnr2alamat1~rnr2alamat2~rnr2alamat3~rnrbagianpenjualan~rnrekspedisi~rnrtglkirim~rnrtermin~rnrtgljatuhtempo~rnruraian~rnrcatatan~rnrnoref~rnrtglnoref~rnrtglpenutupan~rnrmatauang~rnrkurs~rnrhargatermasukpajak~rnrtotal~rnrdiskonpersen~rnrjmldiskon~rnrtotalpajak1detail~rnrtotalpajak2detail~rnrbiayalainpersen~rnrbiayalain~rnrtotaltransaksi~rnrjmlbayar~rnrstatuslunas~rnrtgllunas~rnrnofakturpajak~rnrsdhbayarpajak~rnrtglbayarpajak~rnrrekdiskon~rnrrekpajak1~rnrrekpajak2~rnrrekbiayalain~rnrrekbayar~rnridsq~rnridso~rnridpl~rnriddo~rnriddr~rnridpi~rnridsi~rnrstatussr~rnrstatus~rnrstatussebelumnya~rnrjmlrevisi~rnrcetakanke~rnrinputuser~rnrinputtgl~rnrmodifikasiuser~rnrmodifikasitgl~rnrposting~rnrtutupperiode~rnrisclose~rnrcustomtext1~rnrcustomtext2~rnrcustomtext3~rnrcustomtext4~rnrcustomtext5~rnrcustomint1~rnrcustomint2~rnrcustomint3~rnrcustomdbl1~rnrcustomdbl2~rnrcustomdbl3~rnrcustomdate1~rnrcustomdate2~rnrcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idrnrdetail(0) As Integer, idrnr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususkeluar(12) As Integer, idhppfifokeluar(13) As Integer, harga(14) As Double, 
        'hargapricelist(15) As Double, hpp(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekhargapokok(29) As String, 
        'rekdiskonpenjualan(30) As String, rekreturpenjualan(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idsqdetail(38) As Integer, idsodetail(39) As Integer, 
        'idpldetail(40) As Integer, iddodetail(41) As Integer, iddrdetail(42) As Integer, idpidetail(43) As Integer, idsidetail(44) As Integer, 
        'jmlsr(45) As Double, statussr(46) As Integer, isclose(47) As Integer, customtext1(48) As String, customtext2(49) As String, 
        'customtext3(50) As String, customdbl1(51) As Double, customdbl2(52) As Double, customdbl3(53) As Double, customdate1(54) As Date, 
        'customdate2(55) As Date, customdate3(56) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, 
        'harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, 
        'iddrdetail, idpidetail, idsidetail, jmlsr, statussr, isclose, customtext1, 
        'customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, 
        'customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idrnrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idrnr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idhppkhususkeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idhppfifokeluar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "harga", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "hargapricelist", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "hpp", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "rekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "rekreturpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "costcenter", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "divisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "subdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "proyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "catatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "urutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpldetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "iddrdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idpidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idsidetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussr", AsEnumTypeData.AsInt64)
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
        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updFilterSI As String = ""
        Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsidetail As Integer = 0
        Dim updStokIn As String = "", gudangIn As String = ""

        'FILTER SI, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSI As String = ""

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
            'idrnrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idrnrdetail required numeric." : GoTo selesai
            End If
            'idrnr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idrnr required numeric." : GoTo selesai
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
            'idhppkhususkeluar(12) As Integer
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - idhppkhususkeluar required numeric." : GoTo selesai
            End If
            'idhppfifokeluar(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - idhppfifokeluar required numeric." : GoTo selesai
            End If
            'harga(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'hargapricelist(15) As Double
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - hargapricelist required numeric." : GoTo selesai
            End If
            'hpp(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - hpp required numeric." : GoTo selesai
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
            'idsqdetail(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'idsodetail(39) As Integer
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idpldetail(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - idpldetail required numeric." : GoTo selesai
            End If
            'iddodetail(41) As Integer
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - iddodetail required numeric." : GoTo selesai
            End If
            'iddrdetail(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - iddrdetail required numeric." : GoTo selesai
            End If
            'idpidetail(43) As Integer
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - idpidetail required numeric." : GoTo selesai
            End If
            'idsidetail(44) As Integer
            If (IsNumeric(dataRowDetail(44)) = False) Then
                result(2) = "Row : " & i & " - idsidetail required numeric." : GoTo selesai
            End If
            'jmlsr(45) As Double
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'harga(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'hargapricelist(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargapricelist can't be empty" : GoTo selesai
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
                '    'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(17) As String
                '    dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(17).ToString))
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

            'jmlsr(45) As Double
            If Len(dataRowDetail(45)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idrnrdetail~idrnr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususkeluar~idhppfifokeluar~harga~hargapricelist~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekreturpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpldetail~iddodetail~iddrdetail~idpidetail~idsidetail~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'Set variabel -----------------------------------------------
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtransit(26) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangIn = dataRowDetail(26)
            'idsidetail(44) As Integer
            idsidetail = dataRowDetail(44)

            'ValidasiBatchSerial
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'ValidasiSimpan
            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'VALIDASI OUTSTANDING -------------------------
            If idsidetail <> 0 Then 'SI
                'CEK SI YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSI = IIf(Len(ftSI.ToString) = 0, "", ftSI & " OR ")
                ftSI = String.Concat(ftSI, " (sid.idsidetail = " & idsidetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingSI = IIf(Len(ftExistOutstandingSI.ToString) = 0, "", ftExistOutstandingSI & " UNION ")
                ftExistOutstandingSI = String.Concat(ftExistOutstandingSI, "SELECT EXISTS(SELECT 1 FROM m5_si_detail JOIN m5_si ON idsi = siid WHERE idsidetail = '" & idsidetail & "' AND (sistatus = 2 OR sistatus = 3 OR sistatus = 4 OR sistatus = 7) LIMIT 1) as rowExists, '" & idsidetail & "' as idsidetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                ftOutstandingSI = IIf(Len(ftOutstandingSI.ToString) = 0, "", ftOutstandingSI & " OR ")
                ftOutstandingSI = String.Concat(ftOutstandingSI, " (sid.idsidetail = " & idsidetail & " AND " & Outstanding & " > (sid.jmlbarang - sid.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiSI)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
            End If

            ''VALIDASI STOK -------------------------------
            ''1. SET NILAI UPDATE STOK MASUK --------------
            'updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            'updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("rnrtgl")), AsFormatTanggal(drutama("rnrtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("rnrstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSI, ftOutstandingSI, "", "", "", "", ftSI, drutama("rnrhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("rnrtermin").ToString, AsFormatTanggal(drutama("rnrtgl")), "rnrtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("rnrtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                ''PERHITUNGAN TOTAL UTAMA ================================
                ''DIAMBILKAN DARI DATA DETAIL

                ''TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                ''SUBTOTAL = (jml * harga) - jmldiskon
                'AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                'dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                ''TOTAL = subtotal
                'drutama("rnrtotal") = AsDataTableDSum(dtdetail, "subtotal")

                ''TOTALPAJAK1 = jmlpajak1
                'drutama("rnrtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                ''TOTALPAJAK2 = jmlpajak2
                'drutama("rnrtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                ''JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                ''JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'If Integer.Parse(drutama("rnrhargatermasukpajak")) = 0 Then
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                '    drutama("rnrtotaltransaksi") = Double.Parse(drutama("rnrtotal")) - Double.Parse(drutama("rnrjmldiskon")) + Double.Parse(drutama("rnrtotalpajak1detail")) + Double.Parse(drutama("rnrtotalpajak2detail")) + Double.Parse(drutama("rnrbiayalain"))

                'Else
                '    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                '    drutama("rnrtotaltransaksi") = Double.Parse(drutama("rnrtotal")) - Double.Parse(drutama("rnrjmldiskon")) + Double.Parse(drutama("rnrbiayalain"))

                'End If
                ''END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("rnrid")
                    notransaksi = drutama("rnrnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(rnrid), rnrnotransaksi FROM M5_rnr WHERE rnrid='" & result(4) & "' AND rnrstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rnrid) FROM M5_rnr WHERE rnrnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_rnr_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_RnrHistorySimpan("" & paramSplit(0) & "★M5_RnrHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("rnrsumber")) & "▼" & FixQuotes(drutama("rnrid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Rnr set rnrcabang  = '" & FixQuotes(drutama("rnrcabang")) & "', rnrlokasi  = '" & FixQuotes(drutama("rnrlokasi")) & "', rnrgudang  = '" & FixQuotes(drutama("rnrgudang")) & "', rnrasalbarang  = '" & FixQuotes(drutama("rnrasalbarang")) & "', rnrasalbarangkategori  = " & drutama("rnrasalbarangkategori") & ", rnrjenispenjualan  = '" & FixQuotes(drutama("rnrjenispenjualan")) & "', rnrjenispenjualankategori  = " & drutama("rnrjenispenjualankategori") & ", rnrcarabayar  = " & drutama("rnrcarabayar") & ", rnrsumber  = '" & FixQuotes(drutama("rnrsumber")) & "', rnrautonotransaksi  = " & drutama("rnrautonotransaksi") & ", rnrnotransaksi  = '" & FixQuotes(notransaksi) & "', rnrtgl  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', rnrkodepa  = " & drutama("rnrkodepa") & ", rnrcustomer  = " & drutama("rnrcustomer") & ", rnrcustomerkontak  = '" & FixQuotes(drutama("rnrcustomerkontak")) & "', rnr1alamat1  = '" & FixQuotes(drutama("rnr1alamat1")) & "', rnr1alamat2  = '" & FixQuotes(drutama("rnr1alamat2")) & "', rnr1alamat3  = '" & FixQuotes(drutama("rnr1alamat3")) & "', rnr2alamat1  = '" & FixQuotes(drutama("rnr2alamat1")) & "', rnr2alamat2  = '" & FixQuotes(drutama("rnr2alamat2")) & "', rnr2alamat3  = '" & FixQuotes(drutama("rnr2alamat3")) & "', rnrbagianpenjualan  = " & drutama("rnrbagianpenjualan") & ", rnrekspedisi  = '" & FixQuotes(drutama("rnrekspedisi")) & "', rnrtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglkirim"))) & "', rnrtermin  = '" & FixQuotes(drutama("rnrtermin")) & "', rnrtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', rnruraian  = '" & FixQuotes(drutama("rnruraian")) & "', rnrcatatan  = '" & FixQuotes(drutama("rnrcatatan")) & "', rnrnoref  = '" & FixQuotes(drutama("rnrnoref")) & "', rnrtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglnoref"))) & "', rnrtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglpenutupan"))) & "', rnrmatauang  = '" & FixQuotes(drutama("rnrmatauang")) & "', rnrkurs  = '" & FixDouble(drutama("rnrkurs")) & "', rnrhargatermasukpajak  = " & drutama("rnrhargatermasukpajak") & ", rnrtotal  = '" & FixDouble(drutama("rnrtotal")) & "', rnrdiskonpersen  = '" & FixQuotes(drutama("rnrdiskonpersen")) & "', rnrjmldiskon  = '" & FixDouble(drutama("rnrjmldiskon")) & "', rnrtotalpajak1detail  = '" & FixDouble(drutama("rnrtotalpajak1detail")) & "', rnrtotalpajak2detail  = '" & FixDouble(drutama("rnrtotalpajak2detail")) & "', rnrbiayalainpersen  = '" & FixDouble(drutama("rnrbiayalainpersen")) & "', rnrbiayalain  = '" & FixDouble(drutama("rnrbiayalain")) & "', rnrtotaltransaksi  = '" & FixDouble(drutama("rnrtotaltransaksi")) & "', rnrjmlbayar  = '" & FixDouble(drutama("rnrjmlbayar")) & "', rnrstatuslunas  = " & drutama("rnrstatuslunas") & ", rnrtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', rnrnofakturpajak  = '" & FixQuotes(drutama("rnrnofakturpajak")) & "', rnrsdhbayarpajak  = " & drutama("rnrsdhbayarpajak") & ", rnrtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("rnrtglbayarpajak"))) & "', rnrrekdiskon  = '" & FixQuotes(drutama("rnrrekdiskon")) & "', rnrrekpajak1  = '" & FixQuotes(drutama("rnrrekpajak1")) & "', rnrrekpajak2  = '" & FixQuotes(drutama("rnrrekpajak2")) & "', rnrrekbiayalain  = '" & FixQuotes(drutama("rnrrekbiayalain")) & "', rnrrekbayar  = '" & FixQuotes(drutama("rnrrekbayar")) & "', rnridsq  = " & drutama("rnridsq") & ", rnridso  = " & drutama("rnridso") & ", rnridpl  = " & drutama("rnridpl") & ", rnriddo  = " & drutama("rnriddo") & ", rnriddr  = " & drutama("rnriddr") & ", rnridpi  = " & drutama("rnridpi") & ", rnridsi  = " & drutama("rnridsi") & ", rnrstatussr  = " & drutama("rnrstatussr") & ", rnrstatus  = " & drutama("rnrstatus") & ", rnrstatussebelumnya  = " & drutama("rnrstatussebelumnya") & ", rnrjmlrevisi  = rnrjmlrevisi+1, rnrcetakanke  = " & drutama("rnrcetakanke") & ", rnrmodifikasiuser  = " & drutama("rnrmodifikasiuser") & ", rnrmodifikasitgl  = NOW(), rnrposting  = 0, rnrtutupperiode  = " & drutama("rnrtutupperiode") & ", rnrcustomtext1  = '" & FixQuotes(drutama("rnrcustomtext1")) & "', rnrcustomtext2  = '" & FixQuotes(drutama("rnrcustomtext2")) & "', rnrcustomtext3  = '" & FixQuotes(drutama("rnrcustomtext3")) & "', rnrcustomtext4  = '" & FixQuotes(drutama("rnrcustomtext4")) & "', rnrcustomtext5  = '" & FixQuotes(drutama("rnrcustomtext5")) & "', rnrcustomint1  = " & drutama("rnrcustomint1") & ", rnrcustomint2  = " & drutama("rnrcustomint2") & ", rnrcustomint3  = " & drutama("rnrcustomint3") & ", rnrcustomdbl1  = '" & FixDouble(drutama("rnrcustomdbl1")) & "', rnrcustomdbl2  = '" & FixDouble(drutama("rnrcustomdbl2")) & "', rnrcustomdbl3  = '" & FixDouble(drutama("rnrcustomdbl3")) & "', rnrcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate1"))) & "', rnrcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate2"))) & "', rnrcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate3"))) & "' where rnrid = '" & drutama("rnrid") & "'"
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

                    If drutama("rnrautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("rnrcabang"), drutama("rnrlokasi"), drutama("rnrsumber"), drutama("rnrtgl"))
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
                        notransaksi = drutama("rnrnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(rnrid) FROM m5_rnr WHERE rnrnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Rnr (rnrcabang, rnrlokasi, rnrgudang, rnrasalbarang, rnrasalbarangkategori, rnrjenispenjualan, rnrjenispenjualankategori, rnrcarabayar, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl, rnrkodepa, rnrcustomer, rnrcustomerkontak, rnr1alamat1, rnr1alamat2, rnr1alamat3, rnr2alamat1, rnr2alamat2, rnr2alamat3, rnrbagianpenjualan, rnrekspedisi, rnrtglkirim, rnrtermin, rnrtgljatuhtempo, rnruraian, rnrcatatan, rnrnoref, rnrtglnoref, rnrtglpenutupan, rnrmatauang, rnrkurs, rnrhargatermasukpajak, rnrtotal, rnrdiskonpersen, rnrjmldiskon, rnrtotalpajak1detail, rnrtotalpajak2detail, rnrbiayalainpersen, rnrbiayalain, rnrtotaltransaksi, rnrjmlbayar, rnrstatuslunas, rnrtgllunas, rnrnofakturpajak, rnrsdhbayarpajak, rnrtglbayarpajak, rnrrekdiskon, rnrrekpajak1, rnrrekpajak2, rnrrekbiayalain, rnrrekbayar, rnridsq, rnridso, rnridpl, rnriddo, rnriddr, rnridpi, rnridsi, rnrstatussr, rnrstatus, rnrstatussebelumnya, rnrjmlrevisi, rnrcetakanke, rnrinputuser, rnrinputtgl, rnrmodifikasiuser, rnrmodifikasitgl, rnrposting, rnrtutupperiode, rnrisclose, rnrcustomtext1, rnrcustomtext2, rnrcustomtext3, rnrcustomtext4, rnrcustomtext5, rnrcustomint1, rnrcustomint2, rnrcustomint3, rnrcustomdbl1, rnrcustomdbl2, rnrcustomdbl3, rnrcustomdate1, rnrcustomdate2, rnrcustomdate3) values('" & FixQuotes(drutama("rnrcabang")) & "', '" & FixQuotes(drutama("rnrlokasi")) & "', '" & FixQuotes(drutama("rnrgudang")) & "', '" & FixQuotes(drutama("rnrasalbarang")) & "', " & drutama("rnrasalbarangkategori") & ", '" & FixQuotes(drutama("rnrjenispenjualan")) & "', " & drutama("rnrjenispenjualankategori") & ", " & drutama("rnrcarabayar") & ", '" & FixQuotes(drutama("rnrsumber")) & "', " & drutama("rnrautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgl"))) & "', " & drutama("rnrkodepa") & ", " & drutama("rnrcustomer") & ", '" & FixQuotes(drutama("rnrcustomerkontak")) & "', '" & FixQuotes(drutama("rnr1alamat1")) & "', '" & FixQuotes(drutama("rnr1alamat2")) & "', '" & FixQuotes(drutama("rnr1alamat3")) & "', '" & FixQuotes(drutama("rnr2alamat1")) & "', '" & FixQuotes(drutama("rnr2alamat2")) & "', '" & FixQuotes(drutama("rnr2alamat3")) & "', " & drutama("rnrbagianpenjualan") & ", '" & FixQuotes(drutama("rnrekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglkirim"))) & "', '" & FixQuotes(drutama("rnrtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtgljatuhtempo"))) & "', '" & FixQuotes(drutama("rnruraian")) & "', '" & FixQuotes(drutama("rnrcatatan")) & "', '" & FixQuotes(drutama("rnrnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrtglpenutupan"))) & "', '" & FixQuotes(drutama("rnrmatauang")) & "', '" & FixDouble(drutama("rnrkurs")) & "', " & drutama("rnrhargatermasukpajak") & ", '" & FixDouble(drutama("rnrtotal")) & "', '" & FixQuotes(drutama("rnrdiskonpersen")) & "', '" & FixDouble(drutama("rnrjmldiskon")) & "', '" & FixDouble(drutama("rnrtotalpajak1detail")) & "', '" & FixDouble(drutama("rnrtotalpajak2detail")) & "', '" & FixDouble(drutama("rnrbiayalainpersen")) & "', '" & FixDouble(drutama("rnrbiayalain")) & "', '" & FixDouble(drutama("rnrtotaltransaksi")) & "', '" & FixDouble(drutama("rnrjmlbayar")) & "', " & drutama("rnrstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtgllunas"))) & "', '" & FixQuotes(drutama("rnrnofakturpajak")) & "', " & drutama("rnrsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("rnrtglbayarpajak"))) & "', '" & FixQuotes(drutama("rnrrekdiskon")) & "', '" & FixQuotes(drutama("rnrrekpajak1")) & "', '" & FixQuotes(drutama("rnrrekpajak2")) & "', '" & FixQuotes(drutama("rnrrekbiayalain")) & "', '" & FixQuotes(drutama("rnrrekbayar")) & "', " & drutama("rnridsq") & ", " & drutama("rnridso") & ", " & drutama("rnridpl") & ", " & drutama("rnriddo") & ", " & drutama("rnriddr") & ", " & drutama("rnridpi") & ", " & drutama("rnridsi") & ", " & drutama("rnrstatussr") & ", " & drutama("rnrstatus") & ", " & drutama("rnrstatussebelumnya") & ", " & drutama("rnrjmlrevisi") & ", " & drutama("rnrcetakanke") & ", " & drutama("rnrinputuser") & ", NOW(), " & drutama("rnrmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("rnrtutupperiode") & ", " & drutama("rnrisclose") & ", '" & FixQuotes(drutama("rnrcustomtext1")) & "', '" & FixQuotes(drutama("rnrcustomtext2")) & "', '" & FixQuotes(drutama("rnrcustomtext3")) & "', '" & FixQuotes(drutama("rnrcustomtext4")) & "', '" & FixQuotes(drutama("rnrcustomtext5")) & "', " & drutama("rnrcustomint1") & ", " & drutama("rnrcustomint2") & ", " & drutama("rnrcustomint3") & ", '" & FixDouble(drutama("rnrcustomdbl1")) & "', '" & FixDouble(drutama("rnrcustomdbl2")) & "', '" & FixDouble(drutama("rnrcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("rnrcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select rnrid from M5_rnr where rnrnotransaksi='" & notransaksi & "' AND rnrinputuser= '" & userid & "' order by rnrmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Rnr_Detail where idrnr = '" & result(4) & "'"
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
                    Dim dtBefore As New DataTable
                    Dim strValue2 As New StringBuilder

                    For Each dr1 As DataRow In dtdetail.Rows

                        'VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA --------------------
                        If Not drutama("rnrmatauang").ToString.Equals(dr1("matauang").ToString) Then
                            result(2) = "Row : " & dr1("urutan") & " - " & dr1("tipebarang") & " | " & dr1("namabarang") & " currency (" & dr1("matauang") & ") doesn't belong to the main transactions." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI MATAUANG HARUS SAMA DENGAN TRANSAKSI UTAMA -------------


                        'SET HARGA DARI SI ------------------------------------------------------
                        sql = "SELECT jml, harga, diskon, pajak1, jmlpajak1, pajak2, jmlpajak2 FROM m5_si_detail WHERE idsidetail = '" & FixDouble(dr1("idsidetail")) & "'"
                        dtBefore = AsDataTableAmbilDariDB(sql)
                        If dtBefore.Rows.Count > 0 Then
                            'SET HARGA - ambil dari SI
                            dr1("harga") = Double.Parse(dtBefore.Rows(0)("harga"))

                            'SET DISKON - ambil dari SI
                            dr1("diskon") = dtBefore.Rows(0)("diskon")

                            'SET JMLDISKON - hitung diskon
                            dr1("jmldiskon") = F_Diskon(Double.Parse(dr1("jml")), Double.Parse(dr1("harga")), FixQuotes(dr1("diskon").ToString))

                            'SET PAJAK1 - ambil dari SI
                            dr1("pajak1") = dtBefore.Rows(0)("pajak1")

                            'SET JMLPAJAK1 - ambil dari SI = (jmlpajakSI / jmlSI) * jml
                            dr1("jmlpajak1") = (Double.Parse(dtBefore.Rows(0)("jmlpajak1")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))

                            'SET PAJAK2 - ambil dari SI
                            dr1("pajak2") = dtBefore.Rows(0)("pajak2")

                            'SET JMLPAJAK2 - ambil dari SI = (jmlpajakSI / jmlSI) * jml
                            dr1("jmlpajak2") = (Double.Parse(dtBefore.Rows(0)("jmlpajak2")) / Double.Parse(dtBefore.Rows(0)("jml"))) * Double.Parse(dr1("jml"))
                        End If
                        'END OF SET HARGA DARI SI -----------------------------------------------


                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idrnrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hargapricelist")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekreturpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", " & dr1("iddrdetail") & ", " & dr1("idpidetail") & ", " & dr1("idsidetail") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Rnr_Detail(idrnrdetail, idrnr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'RNR'"
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
                    sql = "Delete from M1_No_Serial_Transaction where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'RNR'"
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


                If drutama("rnrstatus") = 2 Then

                    'UPDATE OUTSTANDING TRANSAKSI ======================================================
                    If Len(updNilaiSI) > 0 Then 'SI
                        'UPDATE DETAIL
                        sql = "UPDATE m5_si_detail SET jmlrealisasi = (CASE idsidetail " & updNilaiSI & " ELSE jmlrealisasi END) WHERE " & updFilterSI
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsi FROM m5_si_detail WHERE " & updFilterSI & " GROUP BY idsi")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsi = '" & dr1("idsi") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idsi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_si_detail WHERE " & ftDetail & " GROUP BY idsi")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiSI = "" : updFilterSI = ""
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
                                updNilaiSI = String.Concat(updNilaiSI, "WHEN '" & dr1("idsi") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                                updFilterSI = String.Concat(updFilterSI, "(siid = '" & dr1("idsi") & "')")
                            Next

                            sql = "UPDATE m5_si SET sistatusrealisasi = (CASE siid " & updNilaiSI & " ELSE sistatusrealisasi END) WHERE " & updFilterSI
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

                    ''UPDATE STOK ====================================================================
                    ''STOK MASUK
                    'If Len(updStokIn) > 0 Then
                    '    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokIn & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = Con1
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    'End If
                    ''END OF UPDATE STOK =============================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "RNR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_RnrUpdateStatusOld(ByVal param As String) As String

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
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)
        Try

            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Rnr", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Rnrtgl, Rnrnotransaksi, Rnrstatus FROM M5_Rnr WHERE Rnrid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================



            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Rnrstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_rnr_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_RnrHistorySimpan("" & paramSplit(0) & "★M5_RnrHistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_rnr_terkait("rnrid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsidetail As Integer = 0
                Dim updNilaiSI As String = "", updFilterSI As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idrnrdetail, idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, idsidetail, gudangtransit, gudangtujuan, urutan FROM m5_rnr_detail WHERE idrnr = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : idsidetail = dr1("idsidetail")
                        gudangOut = dr1("gudangtransit")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING SI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                            updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5)", updNilaiSI)
                            '2.2. SET FILTERUPDATE OUTSTANDING SI
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
                        End If

                        ''VALIDASI STOK -------------------------------
                        ''1. CEK DATA EXIST STOK TUJUAN
                        'ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        'ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        ''2. CEK JML STOK TUJUAN
                        'Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
                        'ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
                        'ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

                        ''3. SET NILAI UPDATE STOK KELUAR TUJUAN
                        'updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
                        'updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                ''VALIDASI STOK ----------------------------------
                ''STOK TUJUAN
                'Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", ftExistStok, ftStok)
                'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                ''END OF VALIDASI STOK ---------------------------

                'UPDATE OUTSTANDING =============================================================
                If Len(updFilterSI) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_si_detail SET jmlrealisasi = (CASE idsidetail " & updNilaiSI & " ELSE jmlrealisasi END) WHERE " & updFilterSI
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsi FROM m5_si_detail WHERE " & updFilterSI & " GROUP BY idsi")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsi = '" & dr1("idsi") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idsi, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_si_detail WHERE " & ftDetail & " GROUP BY idsi")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiSI = "" : updFilterSI = ""
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
                            updNilaiSI = String.Concat(updNilaiSI, "WHEN '" & dr1("idsi") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(siid = '" & dr1("idsi") & "')")
                        Next

                        sql = "UPDATE m5_si SET sistatusrealisasi = (CASE siid " & updNilaiSI & " ELSE sistatusrealisasi END) WHERE " & updFilterSI
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

                ''UPDATE STOK ====================================================================
                ''STOK KELUAR TUJUAN
                'If Len(updStokOut) > 0 Then
                '    sql = "INSERT INTO m1_item_stock_warehouse (idbarang, kgudang, stok) VALUES " & updStokOut & " ON DUPLICATE KEY UPDATE stok = stok + VALUES(stok)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                ''END OF UPDATE STOK =============================================================

            End If

            'update status utama
            sql = "UPDATE M5_Rnr SET Rnrstatus = " & nilaiStatus & ", Rnrmodifikasiuser='" & userid & "', Rnrmodifikasitgl = NOW(), Rnrposting = 0, Rnrpostingtgl = '1971-01-01 00:00:00', Rnrjmlrevisi = Rnrjmlrevisi + 1 WHERE Rnrid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_RnrSearch(PostWsSearch(paramSplit(0), "M5_RnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_RnrDeleteOld(ByVal param As String) As String

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
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            'PERSIAPAN INSERT USER LOG ==========================================================
            Dim sumber As String = "Rnr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Rnrid, Rnrnotransaksi FROM M5_Rnr WHERE Rnrid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT rnrcabang, rnrlokasi, rnrsumber, rnrautonotransaksi, rnrnotransaksi, rnrtgl"
            sql &= " FROM M5_rnr"
            sql &= " WHERE rnrid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("rnrcabang")
                lokasi = dtNomorNext.Rows(0)("rnrlokasi")
                sumber = dtNomorNext.Rows(0)("rnrsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("rnrautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("rnrnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("rnrtgl"))
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
            sql = "DELETE FROM M5_Rnr_Detail WHERE idrnr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Rnr WHERE rnrid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_RnrSearch(PostWsSearch(paramSplit(0), "M5_RnrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function m5_rnr_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = "", filter6 As String = "", filter7 As String = "", filter8 As String = ""
        Dim filter9 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            filter2 = strFilter

            filter3 = strFilter

            filter4 = strFilter

            filter5 = strFilter

            filter6 = strFilter

            filter7 = strFilter

            filter8 = strFilter

            filter9 = strFilter
            filter9 = filter9 & " AND ((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        Else
            'Default filter
            filter9 = "((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        End If

        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        If Len(filter9) > 0 Then filter9 = " WHERE " & filter9

        sql = "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, sq.sqsumber AS sumber, sq.sqid AS idterkait, sq.sqnotransaksi AS noterkait, sq.sqtgl AS tglterkait, sq.sqinputtgl AS inputtglterkait, sq.sqmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sqid JOIN m5_rnr_detail rnrd ON sqd.idsqdetail = rnrd.idsqdetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter2 & " GROUP BY sq.sqid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, so.sosumber AS sumber, so.soid AS idterkait, so.sonotransaksi AS noterkait, so.sotgl AS tglterkait, so.soinputtgl AS inputtglterkait, so.somodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_so_detail sod JOIN m5_so so ON sod.idso = soid JOIN m5_rnr_detail rnrd ON sod.idsodetail = rnrd.idsodetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter2 & " GROUP BY so.soid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, pi.pisumber AS sumber, pi.piid AS idterkait, pi.pinotransaksi AS noterkait, pi.pitgl AS tglterkait, pi.piinputtgl AS inputtglterkait, pi.pimodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_pi_detail pid JOIN m5_pi pi ON pid.idpi = piid JOIN m5_rnr_detail rnrd ON pid.idpidetail = rnrd.idpidetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter3 & " GROUP BY pi.piid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, pl.plsumber AS sumber, pl.plid AS idterkait, pl.plnotransaksi AS noterkait, pl.pltgl AS tglterkait, pl.plinputtgl AS inputtglterkait, pl.plmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_pl_detail pld JOIN m5_pl pl ON pld.idpl = plid JOIN m5_rnr_detail rnrd ON pld.idpldetail = rnrd.idpldetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter4 & " GROUP BY pl.plid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, `do`.dosumber AS sumber, `do`.doid AS idterkait,`do`.donotransaksi AS noterkait, `do`.dotgl AS tglterkait, `do`.doinputtgl AS inputtglterkait, `do`.domodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = doid JOIN m5_rnr_detail rnrd ON dod.iddodetail = rnrd.iddodetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter5 & " GROUP BY `do`.doid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, `dr`.drsumber AS sumber, `dr`.drid AS idterkait,`dr`.drnotransaksi AS noterkait, `dr`.drtgl AS tglterkait, `dr`.drinputtgl AS inputtglterkait, `dr`.drmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_dr_detail drd JOIN m5_dr `dr` ON drd.iddr = drid JOIN m5_rnr_detail rnrd ON drd.iddrdetail = rnrd.iddrdetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter6 & " GROUP BY `dr`.drid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "SELECT rnr.rnrid AS rnrid, rnr.rnrnotransaksi AS rnrnotransaksi, `si`.sisumber AS sumber, `si`.siid AS idterkait,`si`.sinotransaksi AS noterkait, `si`.sitgl AS tglterkait, `si`.siinputtgl AS inputtglterkait, `si`.simodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_si_detail sid JOIN m5_si `si` ON sid.idsi = siid JOIN m5_rnr_detail rnrd ON sid.idsidetail = rnrd.idsidetail JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid " & filter7 & " GROUP BY `si`.siid, rnr.rnrid"
        sql &= " UNION ALL "
        sql &= "select `rnr`.`rnrid` AS `rnrid`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,'SR' AS `sumber`,`m5_sr`.`srid` AS `idterkait`,`m5_sr`.`srnotransaksi` AS `noterkait`,`m5_sr`.`srtgl` AS `tglterkait`,`m5_sr`.`srinputtgl` AS `inputtglterkait`,`m5_sr`.`srmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_rnr_detail` `rnrd` join `m5_rnr` `rnr` on((`rnrd`.`idrnr` = `rnr`.`rnrid`))) join `m5_sr_detail` on((`m5_sr_detail`.`idrnrdetail` = `rnrd`.`idrnrdetail`))) join `m5_sr` on((`m5_sr_detail`.`idsr` = `m5_sr`.`srid`))) " & filter9 & "  group by `m5_sr`.`srid`, `rnr`.`rnrid`"

        Return sql
    End Function

End Class