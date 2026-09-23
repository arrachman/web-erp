Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_sr
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_SrSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataBatch(), dataRowBatch(), dataSerial(), dataRowSerial() As String
        Dim dataAsset(), dataRowAsset() As String

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
        'srid(0) As Integer, srcabang(1) As String, srlokasi(2) As String, srgudang(3) As String, srasalbarang(4) As String, 
        'srasalbarangkategori(5) As Integer, srjenispenjulan(6) As String, srjenispenjualankategori(7) As Integer, srcarabayar(8) As Integer, srsumber(9) As String, 
        'srautonotransaksi(10) As Integer, srnotransaksi(11) As String, srtgl(12) As Date, srkodepa(13) As Integer, srcustomer(14) As Integer, 
        'srcustomerkontak(15) As String, sr1alamat1(16) As String, sr1alamat2(17) As String, sr1alamat3(18) As String, sr2alamat1(19) As String, 
        'sr2alamat2(20) As String, sr2alamat3(21) As String, srbagianpenjualan(22) As Integer, srekspedisi(23) As String, srtglkirim(24) As Date, 
        'srtermin(25) As String, srtgljatuhtempo(26) As Date, sruraian(27) As String, srcatatan(28) As String, srnoref(29) As String, 
        'srtglnoref(30) As Date, srtglpenutupan(31) As Date, srmatauang(32) As String, srkurs(33) As Double, srhargatermasukpajak(34) As Integer, 
        'srtotal(35) As Double, srdiskonpersen(36) As String, srjmldiskon(37) As Double, srtotalpajak1detail(38) As Double, srtotalpajak2detail(39) As Double, 
        'srbiayalainpersen(40) As Double, srbiayalain(41) As Double, srtotaltransaksi(42) As Double, srsisatransaksi(43) As Double, srjmlbayar(44) As Double, 
        'srstatuslunas(45) As Integer, srtgllunas(46) As Date, srnofakturpajak(47) As String, srsdhbayarpajak(48) As Integer, srtglbayarpajak(49) As Date, 
        'srrekdiskon(50) As String, srrekpajak1(51) As String, srrekpajak2(52) As String, srrekbiayalain(53) As String, srreksisa(54) As String, 
        'srrekbayar(55) As String, sridsq(56) As Integer, sridso(57) As Integer, sridpl(58) As Integer, sriddo(59) As Integer, 
        'sriddr(60) As Integer, sridpi(61) As Integer, sridsi(62) As Integer, sridrnr(63) As Integer, srstatus(64) As Integer, 
        'srstatussebelumnya(65) As Integer, srjmlrevisi(66) As Integer, srcetakanke(67) As Integer, srinputuser(68) As Integer, srinputtgl(69) As DateTime, 
        'srmodifikasiuser(70) As Integer, srmodifikasitgl(71) As DateTime, srposting(72) As Integer, srtutupperiode(73) As Integer, srisclose(74) As Integer, 
        'srcustomtext1(75) As String, srcustomtext2(76) As String, srcustomtext3(77) As String, srcustomtext4(78) As String, srcustomtext5(79) As String, 
        'srcustomint1(80) As Integer, srcustomint2(81) As Integer, srcustomint3(82) As Integer, srcustomdbl1(83) As Double, srcustomdbl2(84) As Double, 
        'srcustomdbl3(85) As Double, srcustomdate1(86) As Date, srcustomdate2(87) As Date, srcustomdate3(88) As Date, srjenis(89) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, 
        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, 
        'srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 90) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'srid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "srid required numeric." : GoTo selesai
        End If
        'srasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "srasalbarangkategori required numeric." : GoTo selesai
        End If
        'srjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "srjenispenjualankategori required numeric." : GoTo selesai
        End If
        'srcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "srcarabayar required numeric." : GoTo selesai
        End If
        'srautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "srautonotransaksi required numeric." : GoTo selesai
        End If
        'srtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "srtgl required date." : GoTo selesai
        End If
        'srkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "srkodepa required numeric." : GoTo selesai
        End If
        'srcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "srcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "srcustomer can't be empty." : GoTo selesai
        End If
        'srbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "srbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "srbagianpenjualan can't be empty." : GoTo selesai
        End If
        'srtglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "srtglkirim required date." : GoTo selesai
        End If
        'srtgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "srtgljatuhtempo required date." : GoTo selesai
        End If
        'srtglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "srtglnoref required date." : GoTo selesai
        End If
        'srtglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "srtglpenutupan required date." : GoTo selesai
        End If
        'srkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "srkurs required numeric." : GoTo selesai
        End If
        'srhargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "srhargatermasukpajak required numeric." : GoTo selesai
        End If
        'srtotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "srtotal required numeric." : GoTo selesai
        End If
        'srjmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "srjmldiskon required numeric." : GoTo selesai
        End If
        'srtotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "srtotalpajak1detail required numeric." : GoTo selesai
        End If
        'srtotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "srtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''srbiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "srbiayalainpersen required numeric." : GoTo selesai
        'End If
        'srbiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "srbiayalain required numeric." : GoTo selesai
        End If
        'srtotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "srtotaltransaksi required numeric." : GoTo selesai
        End If
        'srsisatransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "srsisatransaksi required numeric." : GoTo selesai
        End If
        'srjmlbayar(44) As Double
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "srjmlbayar required numeric." : GoTo selesai
        End If
        'srstatuslunas(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "srstatuslunas required numeric." : GoTo selesai
        End If
        'srtgllunas(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "srtgllunas required date." : GoTo selesai
        End If
        'srsdhbayarpajak(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "srsdhbayarpajak required numeric." : GoTo selesai
        End If
        'srtglbayarpajak(49) As Date
        If (IsDate(dataUtama(49)) = False) Then
            result(2) = "srtglbayarpajak required date." : GoTo selesai
        End If
        'sridsq(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "sridsq required numeric." : GoTo selesai
        End If
        'sridso(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sridso required numeric." : GoTo selesai
        End If
        'sridpl(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "sridpl required numeric." : GoTo selesai
        End If
        'sriddo(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sriddo required numeric." : GoTo selesai
        End If
        'sriddr(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "sriddr required numeric." : GoTo selesai
        End If
        'sridpi(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "sridpi required numeric." : GoTo selesai
        End If
        'sridsi(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "sridsi required numeric." : GoTo selesai
        End If
        'sridrnr(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "sridrnr required numeric." : GoTo selesai
        End If
        'srstatus(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "srstatus required numeric." : GoTo selesai
        End If
        'srstatussebelumnya(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "srstatussebelumnya required numeric." : GoTo selesai
        End If
        'srjmlrevisi(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "srjmlrevisi required numeric." : GoTo selesai
        End If
        'srcetakanke(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "srcetakanke required numeric." : GoTo selesai
        End If
        'srinputuser(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "srinputuser required numeric." : GoTo selesai
        End If
        'srinputtgl(69) As DateTime
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "srinputtgl required date." : GoTo selesai
        End If
        'srmodifikasiuser(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "srmodifikasiuser required numeric." : GoTo selesai
        End If
        'srmodifikasitgl(71) As DateTime
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "srmodifikasitgl required date." : GoTo selesai
        End If
        'srposting(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "srposting required numeric." : GoTo selesai
        End If
        'srtutupperiode(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "srtutupperiode required numeric." : GoTo selesai
        End If
        'srisclose(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "srisclose required numeric." : GoTo selesai
        End If
        'srcustomint1(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "srcustomint1 required numeric." : GoTo selesai
        End If
        'srcustomint2(81) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "srcustomint2 required numeric." : GoTo selesai
        End If
        'srcustomint3(82) As Integer
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "srcustomint3 required numeric." : GoTo selesai
        End If
        'srcustomdbl1(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "srcustomdbl1 required numeric." : GoTo selesai
        End If
        'srcustomdbl2(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "srcustomdbl2 required numeric." : GoTo selesai
        End If
        'srcustomdbl3(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "srcustomdbl3 required numeric." : GoTo selesai
        End If
        'srcustomdate1(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "srcustomdate1 required date." : GoTo selesai
        End If
        'srcustomdate2(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "srcustomdate2 required date." : GoTo selesai
        End If
        'srcustomdate3(88) As Date
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "srcustomdate3 required date." : GoTo selesai
        End If

        'srjenis(89) As Integer
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "srjenis required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'srcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "srcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "srcabang should not be more than 25 character." : GoTo selesai
        End If

        'srlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "srlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "srlokasi should not be more than 25 character." : GoTo selesai
        End If

        'srgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "srgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "srgudang should not be more than 25 character." : GoTo selesai
        End If

        'srsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "srsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "srsumber should not be more than 10 character." : GoTo selesai
        End If

        'srnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "srnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "srnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'srtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "srtgl can't be empty" : GoTo selesai
        End If

        'srtglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "srtglkirim can't be empty" : GoTo selesai
        End If

        'srtgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "srtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'srtglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "srtglnoref can't be empty" : GoTo selesai
        End If

        'srtglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "srtglpenutupan can't be empty" : GoTo selesai
        End If

        'srmatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "srmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "srmatauang should not be more than 25 character." : GoTo selesai
        End If

        'srkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "srkurs can't be empty" : GoTo selesai
        End If

        'srtotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "srtotal can't be empty" : GoTo selesai
        End If

        'srdiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "srdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "srdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'srjmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "srjmldiskon can't be empty" : GoTo selesai
        End If

        'srtotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "srtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'srtotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "srtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'srbiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "srbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "srbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'srbiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "srbiayalain can't be empty" : GoTo selesai
        End If

        'srtotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "srtotaltransaksi can't be empty" : GoTo selesai
        End If

        'srsisatransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "srsisatransaksi can't be empty" : GoTo selesai
        End If

        'srjmlbayar(44) As Double
        If Len(dataUtama(44)) = 0 Then
            result(2) = "srjmlbayar can't be empty" : GoTo selesai
        End If

        'srtgllunas(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "srtgllunas can't be empty" : GoTo selesai
        End If

        'srtglbayarpajak(49) As Date
        If Len(dataUtama(49)) = 0 Then
            result(2) = "srtglbayarpajak can't be empty" : GoTo selesai
        End If

        'srinputtgl(69) As DateTime
        If Len(dataUtama(69)) = 0 Then
            result(2) = "srinputtgl can't be empty" : GoTo selesai
        End If

        'srmodifikasitgl(71) As DateTime
        If Len(dataUtama(71)) = 0 Then
            result(2) = "srmodifikasitgl can't be empty" : GoTo selesai
        End If

        'srcustomdbl1(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "srcustomdbl1 can't be empty" : GoTo selesai
        End If

        'srcustomdbl2(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "srcustomdbl2 can't be empty" : GoTo selesai
        End If

        'srcustomdbl3(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "srcustomdbl3 can't be empty" : GoTo selesai
        End If

        'srcustomdate1(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "srcustomdate1 can't be empty" : GoTo selesai
        End If

        'srcustomdate2(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "srcustomdate2 can't be empty" : GoTo selesai
        End If

        'srcustomdate3(88) As Date
        If Len(dataUtama(88)) = 0 Then
            result(2) = "srcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "srid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjenispenjulan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjenis", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "srid~srcabang~srlokasi~srgudang~srasalbarang~srasalbarangkategori~srjenispenjulan~srjenispenjualankategori~srcarabayar~srsumber~srautonotransaksi~srnotransaksi~srtgl~srkodepa~srcustomer~srcustomerkontak~sr1alamat1~sr1alamat2~sr1alamat3~sr2alamat1~sr2alamat2~sr2alamat3~srbagianpenjualan~srekspedisi~srtglkirim~srtermin~srtgljatuhtempo~sruraian~srcatatan~srnoref~srtglnoref~srtglpenutupan~srmatauang~srkurs~srhargatermasukpajak~srtotal~srdiskonpersen~srjmldiskon~srtotalpajak1detail~srtotalpajak2detail~srbiayalainpersen~srbiayalain~srtotaltransaksi~srsisatransaksi~srjmlbayar~srstatuslunas~srtgllunas~srnofakturpajak~srsdhbayarpajak~srtglbayarpajak~srrekdiskon~srrekpajak1~srrekpajak2~srrekbiayalain~srreksisa~srrekbayar~sridsq~sridso~sridpl~sriddo~sriddr~sridpi~sridsi~sridrnr~srstatus~srstatussebelumnya~srjmlrevisi~srcetakanke~srinputuser~srinputtgl~srmodifikasiuser~srmodifikasitgl~srposting~srtutupperiode~srisclose~srcustomtext1~srcustomtext2~srcustomtext3~srcustomtext4~srcustomtext5~srcustomint1~srcustomint2~srcustomint3~srcustomdbl1~srcustomdbl2~srcustomdbl3~srcustomdate1~srcustomdate2~srcustomdate3~srjenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsrdetail(0) As Integer, idsr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususkeluar(12) As Integer, idhppfifokeluar(13) As Integer, harga(14) As Double, 
        'hargapricelist(15) As Double, hpp(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekhargapokok(29) As String, 
        'rekdiskonpenjualan(30) As String, rekreturpenjualan(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idsqdetail(38) As Integer, idsodetail(39) As Integer, 
        'idpldetail(40) As Integer, iddodetail(41) As Integer, iddrdetail(42) As Integer, idpidetail(43) As Integer, idsidetail(44) As Integer, 
        'idrnrdetail(45) As Integer, isclose(46) As Integer, customtext1(47) As String, customtext2(48) As String, customtext3(49) As String, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdate1(53) As Date, customdate2(54) As Date, 
        'customdate3(55) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, 
        'harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, 
        'iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idrnrdetail", AsEnumTypeData.AsInt64)
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

        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updFilterSI As String = ""
        Dim ftExistOutstandingRNR As String = "", ftOutstandingRNR As String = "", updNilaiRNR As String = "", updFilterRNR As String = ""
        Dim idbarang As Integer = 0, idsidetail As Integer = 0, idrnrdetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""
        Dim updStokBarang As String = "", ftStokBarang As String = ""

        'FILTER SI DAN RNR, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSI As String = "", ftRNR As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 56) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsrdetail required numeric." : GoTo selesai
            End If
            'idsr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsr required numeric." : GoTo selesai
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
            'idrnrdetail(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - idrnrdetail required numeric." : GoTo selesai
            End If
            'isclose(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
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
            'If dataRowDetail(14) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'hargapricelist(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargapricelist can't be empty" : GoTo selesai
            End If

            ''hpp(16) As Double
            'If (Double.Parse(dataRowDetail(16)) <= 0) Then
            '    result(2) = "Row : " & i & " - hpp can't be less than or equal to zero" : GoTo selesai
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
                'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(17) As String
                dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(17).ToString))
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

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsrdetail~idsr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususkeluar~idhppfifokeluar~harga~hargapricelist~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekreturpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpldetail~iddodetail~iddrdetail~idpidetail~idsidetail~idrnrdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtujuan(27) As String   , gudangtransit(26) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangIn = dataRowDetail(27) : gudangOut = dataRowDetail(26)
            'idsidetail(44) As Integer     , idrnrdetail(45) As Integer
            idsidetail = dataRowDetail(44) : idrnrdetail = dataRowDetail(45)

            'ValidasiHppI
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

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

            If idrnrdetail <> 0 Then 'RNR
                'CEK RNR YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRNR = IIf(Len(ftRNR.ToString) = 0, "", ftRNR & " OR ")
                ftRNR = String.Concat(ftRNR, " (rnrd.idrnrdetail = " & idrnrdetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingRNR = IIf(Len(ftExistOutstandingRNR.ToString) = 0, "", ftExistOutstandingRNR & " UNION ")
                ftExistOutstandingRNR = String.Concat(ftExistOutstandingRNR, "SELECT EXISTS(SELECT 1 FROM m5_rnr_detail JOIN m5_rnr ON idrnr = rnrid WHERE idrnrdetail = '" & idrnrdetail & "' AND (rnrstatus = 2 OR rnrstatus = 3 OR rnrstatus = 4 OR rnrstatus = 7) LIMIT 1) as rowExists, '" & idrnrdetail & "' as idrnrdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrnrdetail=" & idrnrdetail)
                ftOutstandingRNR = IIf(Len(ftOutstandingRNR.ToString) = 0, "", ftOutstandingRNR & " OR ")

                ftOutstandingRNR = String.Concat(ftOutstandingRNR, " (rnrd.idrnrdetail = " & idrnrdetail & " AND " & Outstanding & " > (rnrd.jmlbarang - rnrd.jmlrealisasi)) ")
                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiRNR = String.Concat("WHEN '" & idrnrdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRNR)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                updFilterRNR = String.Concat(updFilterRNR, "(idrnrdetail = '" & idrnrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            '5. SET NILAI UPDATE STOK M1_ITEM ------------
            Dim stokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & stokMasuk & "', 5) ", updStokBarang)
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
                vStatus = drutama("srstatus")
                vTgl = AsFormatTanggal(drutama("srtgl"))

                'CEK HAK AKSES STATUS ============================
                Dim vAkses As Integer = 0, msgAkses As String = ""
                'MODUL DAN MENU HARUS DISESUAIKAN
                Dim vModuleId As Integer = 5, vMenuId As Integer = 12
                Select Case drutama("srstatus")
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("srtgl")), AsFormatTanggal(drutama("srtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("srstatus") = 2 Or drutama("srstatus") = 1 Or drutama("srstatus") = 8 Or drutama("srstatus") = 9 Or drutama("srstatus") = 10 Or drutama("srstatus") = 11 Then
                    Dim rsValidasi As String = ""

                    'JIKA TANPA RNR MAKA CEK BATCH DAN SERIAL
                    If Double.Parse(drutama("srjenispenjualankategori")) = 0 Then
                        'VALIDASI BATCH SERIAL ---------------
                        'ValidasiBatchSerial
                        rsValidasi = ValidasiBatchSerialSR(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                        'END OF VALIDASI BATCH SERIAL --------

                        'VALIDASI ASSET ----------------------
                        'ValidasiAsset
                        rsValidasi = ValidasiAsset(dtdetail, dtasset, ftBarang, "jmlbarang", 1)
                        If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                        'END OF VALIDASI ASSET ---------------
                    End If

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSI, ftOutstandingSI, ftExistOutstandingRNR, ftOutstandingRNR, "", "", "", "", ftSI, ftRNR, drutama("srhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("srtermin").ToString, AsFormatTanggal(drutama("srtgl")), "srtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("srtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("srtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("srtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("srtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("srhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("srtotaltransaksi") = Double.Parse(drutama("srtotal")) - Double.Parse(drutama("srjmldiskon")) + Double.Parse(drutama("srtotalpajak1detail")) + Double.Parse(drutama("srtotalpajak2detail")) + Double.Parse(drutama("srbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("srtotaltransaksi") = Double.Parse(drutama("srtotal")) - Double.Parse(drutama("srjmldiskon")) + Double.Parse(drutama("srtotalpajak2detail")) + Double.Parse(drutama("srbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA RETUR LANGSUNG MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("srjenis")) = 1 Then
                    drutama("srjmlbayar") = drutama("srtotaltransaksi")
                    drutama("srtgllunas") = drutama("srtgl")
                    drutama("srstatuslunas") = 2

                Else
                    drutama("srjmlbayar") = 0 : drutama("srtgllunas") = "1900-01-01" : drutama("srstatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("srid")
                    notransaksi = drutama("srnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(srid), srnotransaksi FROM M5_sr WHERE srid='" & result(4) & "' AND srstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("srautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("srcabang"), drutama("srlokasi"), drutama("srsumber"), drutama("srtgl"), drutama("srsumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(srid) FROM M5_sr WHERE srnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("srsumber")) & "▼" & FixQuotes(drutama("srid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sr set srcabang  = '" & FixQuotes(drutama("srcabang")) & "', srlokasi  = '" & FixQuotes(drutama("srlokasi")) & "', srgudang  = '" & FixQuotes(drutama("srgudang")) & "', srasalbarang  = '" & FixQuotes(drutama("srasalbarang")) & "', srasalbarangkategori  = " & drutama("srasalbarangkategori") & ", srjenispenjulan  = '" & FixQuotes(drutama("srjenispenjulan")) & "', srjenispenjualankategori  = " & drutama("srjenispenjualankategori") & ", srcarabayar  = " & drutama("srcarabayar") & ", srsumber  = '" & FixQuotes(drutama("srsumber")) & "', srautonotransaksi  = " & drutama("srautonotransaksi") & ", srnotransaksi  = '" & FixQuotes(notransaksi) & "', srtgl  = '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', srkodepa  = " & drutama("srkodepa") & ", srcustomer  = " & drutama("srcustomer") & ", srcustomerkontak  = '" & FixQuotes(drutama("srcustomerkontak")) & "', sr1alamat1  = '" & FixQuotes(drutama("sr1alamat1")) & "', sr1alamat2  = '" & FixQuotes(drutama("sr1alamat2")) & "', sr1alamat3  = '" & FixQuotes(drutama("sr1alamat3")) & "', sr2alamat1  = '" & FixQuotes(drutama("sr2alamat1")) & "', sr2alamat2  = '" & FixQuotes(drutama("sr2alamat2")) & "', sr2alamat3  = '" & FixQuotes(drutama("sr2alamat3")) & "', srbagianpenjualan  = " & drutama("srbagianpenjualan") & ", srekspedisi  = '" & FixQuotes(drutama("srekspedisi")) & "', srtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', srtermin  = '" & FixQuotes(drutama("srtermin")) & "', srtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', sruraian  = '" & FixQuotes(drutama("sruraian")) & "', srcatatan  = '" & FixQuotes(drutama("srcatatan")) & "', srnoref  = '" & FixQuotes(drutama("srnoref")) & "', srtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', srtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', srmatauang  = '" & FixQuotes(drutama("srmatauang")) & "', srkurs  = '" & FixDouble(drutama("srkurs")) & "', srhargatermasukpajak  = " & drutama("srhargatermasukpajak") & ", srtotal  = '" & FixDouble(drutama("srtotal")) & "', srdiskonpersen  = '" & FixQuotes(drutama("srdiskonpersen")) & "', srjmldiskon  = '" & FixDouble(drutama("srjmldiskon")) & "', srtotalpajak1detail  = '" & FixDouble(drutama("srtotalpajak1detail")) & "', srtotalpajak2detail  = '" & FixDouble(drutama("srtotalpajak2detail")) & "', srbiayalainpersen  = '" & FixDouble(drutama("srbiayalainpersen")) & "', srbiayalain  = '" & FixDouble(drutama("srbiayalain")) & "', srtotaltransaksi  = '" & FixDouble(drutama("srtotaltransaksi")) & "', srsisatransaksi  = '" & FixDouble(drutama("srsisatransaksi")) & "', srjmlbayar  = '" & FixDouble(drutama("srjmlbayar")) & "', srstatuslunas  = " & drutama("srstatuslunas") & ", srtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', srnofakturpajak  = '" & FixQuotes(drutama("srnofakturpajak")) & "', srsdhbayarpajak  = " & drutama("srsdhbayarpajak") & ", srtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', srrekdiskon  = '" & FixQuotes(drutama("srrekdiskon")) & "', srrekpajak1  = '" & FixQuotes(drutama("srrekpajak1")) & "', srrekpajak2  = '" & FixQuotes(drutama("srrekpajak2")) & "', srrekbiayalain  = '" & FixQuotes(drutama("srrekbiayalain")) & "', srreksisa  = '" & FixQuotes(drutama("srreksisa")) & "', srrekbayar  = '" & FixQuotes(drutama("srrekbayar")) & "', sridsq  = " & drutama("sridsq") & ", sridso  = " & drutama("sridso") & ", sridpl  = " & drutama("sridpl") & ", sriddo  = " & drutama("sriddo") & ", sriddr  = " & drutama("sriddr") & ", sridpi  = " & drutama("sridpi") & ", sridsi  = " & drutama("sridsi") & ", sridrnr  = " & drutama("sridrnr") & ", srstatus  = " & drutama("srstatus") & ", srstatussebelumnya  = " & drutama("srstatussebelumnya") & ", srjmlrevisi  = srjmlrevisi+1, srcetakanke  = " & drutama("srcetakanke") & ", srmodifikasiuser  = " & drutama("srmodifikasiuser") & ", srmodifikasitgl  = NOW(), srposting  = 0, srtutupperiode  = " & drutama("srtutupperiode") & ", srcustomtext1  = '" & FixQuotes(drutama("srcustomtext1")) & "', srcustomtext2  = '" & FixQuotes(drutama("srcustomtext2")) & "', srcustomtext3  = '" & FixQuotes(drutama("srcustomtext3")) & "', srcustomtext4  = '" & FixQuotes(drutama("srcustomtext4")) & "', srcustomtext5  = '" & FixQuotes(drutama("srcustomtext5")) & "', srcustomint1  = " & drutama("srcustomint1") & ", srcustomint2  = " & drutama("srcustomint2") & ", srcustomint3  = " & drutama("srcustomint3") & ", srcustomdbl1  = '" & FixDouble(drutama("srcustomdbl1")) & "', srcustomdbl2  = '" & FixDouble(drutama("srcustomdbl2")) & "', srcustomdbl3  = '" & FixDouble(drutama("srcustomdbl3")) & "', srcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', srcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', srcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', srjenis = '" & FixQuotes(drutama("srjenis")) & "' where srid = '" & drutama("srid") & "'"
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

                    If drutama("srautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("srcabang"), drutama("srlokasi"), drutama("srsumber"), drutama("srtgl"), drutama("srsumber"), 5)
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
                        notransaksi = drutama("srnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(srid) FROM m5_sr WHERE srnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sr (srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis) values('" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(drutama("srgudang")) & "', '" & FixQuotes(drutama("srasalbarang")) & "', " & drutama("srasalbarangkategori") & ", '" & FixQuotes(drutama("srjenispenjulan")) & "', " & drutama("srjenispenjualankategori") & ", " & drutama("srcarabayar") & ", '" & FixQuotes(drutama("srsumber")) & "', " & drutama("srautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srkodepa") & ", " & drutama("srcustomer") & ", '" & FixQuotes(drutama("srcustomerkontak")) & "', '" & FixQuotes(drutama("sr1alamat1")) & "', '" & FixQuotes(drutama("sr1alamat2")) & "', '" & FixQuotes(drutama("sr1alamat3")) & "', '" & FixQuotes(drutama("sr2alamat1")) & "', '" & FixQuotes(drutama("sr2alamat2")) & "', '" & FixQuotes(drutama("sr2alamat3")) & "', " & drutama("srbagianpenjualan") & ", '" & FixQuotes(drutama("srekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', '" & FixQuotes(drutama("srtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(drutama("srnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', '" & FixQuotes(drutama("srmatauang")) & "', '" & FixDouble(drutama("srkurs")) & "', " & drutama("srhargatermasukpajak") & ", '" & FixDouble(drutama("srtotal")) & "', '" & FixQuotes(drutama("srdiskonpersen")) & "', '" & FixDouble(drutama("srjmldiskon")) & "', '" & FixDouble(drutama("srtotalpajak1detail")) & "', '" & FixDouble(drutama("srtotalpajak2detail")) & "', '" & FixDouble(drutama("srbiayalainpersen")) & "', '" & FixDouble(drutama("srbiayalain")) & "', '" & FixDouble(drutama("srtotaltransaksi")) & "', '" & FixDouble(drutama("srsisatransaksi")) & "', '" & FixDouble(drutama("srjmlbayar")) & "', " & drutama("srstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', '" & FixQuotes(drutama("srnofakturpajak")) & "', " & drutama("srsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', '" & FixQuotes(drutama("srrekdiskon")) & "', '" & FixQuotes(drutama("srrekpajak1")) & "', '" & FixQuotes(drutama("srrekpajak2")) & "', '" & FixQuotes(drutama("srrekbiayalain")) & "', '" & FixQuotes(drutama("srreksisa")) & "', '" & FixQuotes(drutama("srrekbayar")) & "', " & drutama("sridsq") & ", " & drutama("sridso") & ", " & drutama("sridpl") & ", " & drutama("sriddo") & ", " & drutama("sriddr") & ", " & drutama("sridpi") & ", " & drutama("sridsi") & ", " & drutama("sridrnr") & ", " & drutama("srstatus") & ", " & drutama("srstatussebelumnya") & ", " & drutama("srjmlrevisi") & ", " & drutama("srcetakanke") & ", " & drutama("srinputuser") & ", NOW(), " & drutama("srmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("srtutupperiode") & ", " & drutama("srisclose") & ", '" & FixQuotes(drutama("srcustomtext1")) & "', '" & FixQuotes(drutama("srcustomtext2")) & "', '" & FixQuotes(drutama("srcustomtext3")) & "', '" & FixQuotes(drutama("srcustomtext4")) & "', '" & FixQuotes(drutama("srcustomtext5")) & "', " & drutama("srcustomint1") & ", " & drutama("srcustomint2") & ", " & drutama("srcustomint3") & ", '" & FixDouble(drutama("srcustomdbl1")) & "', '" & FixDouble(drutama("srcustomdbl2")) & "', '" & FixDouble(drutama("srcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', '" & FixQuotes(drutama("srjenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select srid from M5_sr where srnotransaksi='" & notransaksi & "' AND srinputuser= '" & userid & "' order by srmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sr_Detail where idsr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hargapricelist")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekreturpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", " & dr1("iddrdetail") & ", " & dr1("idpidetail") & ", " & dr1("idsidetail") & ", " & dr1("idrnrdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Sr_Detail(idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'VALIDASI KETIKA SR LANGSUNG (SRJENIS = 1) MAKA TIDAK BOLEH AMBIL LEBIH DARI 1 NOMOR SI
                Dim IdSI As Double = 0
                If drutama("srjenis") = 1 Then
                    sql = "SELECT si.siid, si.sinotransaksi, si.sitotaltransaksi, si.sijmlbayar FROM m5_sr_detail srd JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail JOIN m5_si si ON sid.idsi = si.siid WHERE srd.idsr = '" & result(4) & "' GROUP BY si.siid"
                    Dim dtCekSI As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtCekSI.Rows.Count > 1 Then
                        result(2) = "Direct SR (Sales Retur) can only pick from one SI (Sales Invoice) transaction." : Trans.Rollback() : GoTo selesai

                    ElseIf dtCekSI.Rows.Count = 1 Then
                        'VALIDASI KETIKA SR LANGSUNG (SRJENIS = 1) MAKA TOTAL TRANSAKSI SR TIDAK BOLEH MELEBIHI SISA SI YANG BELUM DIBAYAR
                        If Len(dtCekSI.Rows(0)("siid")) > 0 Then
                            IdSI = Double.Parse(dtCekSI.Rows(0)("siid"))
                            If Double.Parse(drutama("srtotaltransaksi")) > (Double.Parse(dtCekSI.Rows(0)("sitotaltransaksi")) - Double.Parse(dtCekSI.Rows(0)("sijmlbayar"))) Then
                                Dim selisih(2) As String
                                selisih = F_Nominal(F_Round((Double.Parse(dtCekSI.Rows(0)("sitotaltransaksi")) - Double.Parse(dtCekSI.Rows(0)("sijmlbayar")))), True).Split(sptSubParam)

                                result(2) = "Total Direct SR (Sales Retur) exceeds the AR (Account Receivables) from SI (Sales Invoice) transaction no. " & dtCekSI.Rows(0)("sinotransaksi") & ". AR available : " & drutama("srmatauang") & " " & selisih(1) : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                    End If
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'SR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'SR'"
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
                    sql = "Delete from M7_Asset_Transaction where atidutama = '" & result(4) & "' AND atsumber = 'SR'"
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


                Dim sumber As String = "SR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0

                If drutama("srstatus") = 2 Then
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

                    If Len(updNilaiRNR) > 0 Then 'RNR
                        'UPDATE DETAIL
                        sql = "UPDATE m5_rnr_detail SET jmlrealisasi = (CASE idrnrdetail " & updNilaiRNR & " ELSE jmlrealisasi END) WHERE " & updFilterRNR
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idrnr FROM m5_rnr_detail WHERE " & updFilterRNR & " GROUP BY idrnr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idrnr = '" & dr1("idrnr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idrnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_rnr_detail WHERE " & ftDetail & " GROUP BY idrnr", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRNR = "" : updFilterRNR = ""
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
                                updNilaiRNR = String.Concat(updNilaiRNR, "WHEN '" & dr1("idrnr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                                updFilterRNR = String.Concat(updFilterRNR, "(rnrid = '" & dr1("idrnr") & "')")
                            Next

                            sql = "UPDATE m5_rnr SET rnrstatusrealisasi = (CASE rnrid " & updNilaiRNR & " ELSE rnrstatusrealisasi END) WHERE " & updFilterRNR
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


                    'UPDATE NO ASSET ===============================================================
                    If dtasset.Rows.Count > 0 Then
                        Dim strValue2 As New StringBuilder
                        For Each dr1 As DataRow In dtasset.Rows
                            'QUERY INSERT NO ASSET IN
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append(FixDouble(dr1("atasetid")))
                        Next
                        sql = "UPDATE m7_asset a SET a.aakumulasibeban = a.aakumulasibebansebelumnya, a.anilaibuku = a.anilaibukusebelumnya, a.aisclose = 0 WHERE a.aid IN(" & strValue2.ToString & ")"
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


                    'JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI =========================
                    If drutama("srjenis") = 1 And IdSI = 0 Then
                        IdSI = drutama("sridsi")
                    End If
					
                    If drutama("srjenis") = 1 And IdSI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET si.sijmlbayar = si.sijmlbayar + " & Double.Parse(drutama("srtotaltransaksi")) & ", si.sitgllunas = (CASE WHEN si.sijmlbayar + " & Double.Parse(drutama("srtotaltransaksi")) & " >= si.sitotaltransaksi THEN '" & AsFormatTanggal(FixQuotes(drutama("srtgl"))) & "' ELSE si.sitgllunas END) WHERE si.siid = '" & IdSI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE si.siid = '" & IdSI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI ==================


                    'UPDATE TOTAL PIUTANG ============================================================
                    'PLAFON
                    'sql = "UPDATE m0_setting s JOIN m5_sr sr ON sr.srid = '" & result(4) & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSR' AND s.snilai = 1 JOIN m1_contact c ON c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang - (sr.srtotaltransaksi * sr.srkurs)"
                    sql = "UPDATE m5_sr sr JOIN m1_contact c ON sr.srid = '" & result(4) & "' AND c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang - (sr.srtotaltransaksi * sr.srkurs)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                    'END OF UPDATE TOTAL PIUTANG =====================================================


                    'JIKA TANPA RNR MAKA HITUNG TRANSAKSI BARANG DAN POSTING HPP
                    If Double.Parse(drutama("srjenispenjualankategori")) = 0 Then

                        'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                        Dim dtDetailNew As DataTable = AsDataTableAmbilDariDBCon("SELECT srd.idsrdetail, srd.idbarang, srd.namabarang, srd.tipebarang, srd.jml, srd.satuan, srd.jmlbarang, srd.satuanbarang, srd.matauang, srd.kurs, srd.harga, srd.diskon, srd.jmldiskon, srd.hpp, srd.idhppkhususkeluar, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.catatan, srd.costcenter, srd.divisi, srd.subdivisi, srd.proyek, sr.srinputtgl, i.bhpp, IFNULL(sid.hpp,srd.hpp)as hppbaru FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON srd.idsidetail=sid.idsidetail WHERE srd.idsr = '" & result(4) & "'", myConn)

                        Dim hpp As Double = 0, postinghpp As Double = 0, gudang As String = "", bstok As Double = 0
                        Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                        Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                        If dtDetailNew.Rows.Count > 0 Then

                            'INSERT ITEM TRANSACTION ====================================================
                            For Each dr1 As DataRow In dtDetailNew.Rows 'SET NILAI VARIABEL
                                'SET NILAI VARIABEL
                                idbarang = Double.Parse(dr1("idbarang"))
                                jmlbarang = Double.Parse(dr1("jmlbarang"))
                                gudang = dr1("gudangtujuan")

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
                                    'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                          kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                          kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,                         idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                    strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("srkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("srsumber")) & "', " & result(4) & ", " & dr1("idsrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("srinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("srinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                            'END OF INSERT ITEM TRANSACTION =============================================

                        Else
                            result(2) = "Detail transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'INSERT MSMQ HPP ====================================================================
                        If drutama("srstatus") = 2 Then
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

                    End If
                    
                End If

                'INSERT MSMQ JURNAL =================================================================
                If drutama("srstatus") = 2 Then
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
    Public Function M5_SrUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "SR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim srjenis As Integer = 0, srtotaltransaksi As Double = 0

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Srtgl, Srnotransaksi, Srstatus, Srjenis, Srtotaltransaksi FROM M5_Sr WHERE Srid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'srjenis                                        'srtotaltransaksi
                srjenis = Integer.Parse(dtdetail.Rows(1)(3)) : srtotaltransaksi = Double.Parse(dtdetail.Rows(1)(4))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Srstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sr_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                'sql = query.m5_sr_terkait("srid = '" & idtransaksi & "'")
                sql = m5_sr_terkait("srid = '" & idtransaksi & "'")
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


                'UPDATE TOTAL PIUTANG ============================================================
                'PLAFON
                'sql = "UPDATE m0_setting s JOIN m5_sr sr ON sr.srid = '" & idtransaksi & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSR' AND s.snilai = 1 JOIN m1_contact c ON c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang + (sr.srtotaltransaksi * sr.srkurs)"
                sql = "UPDATE m5_sr sr JOIN m1_contact c ON sr.srid = '" & idtransaksi & "' AND c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang + (sr.srtotaltransaksi * sr.srkurs)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE TOTAL PIUTANG =====================================================


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsrdetail As Integer = 0, idsidetail As Integer = 0, idrnrdetail As Integer = 0
                Dim idhppkhususmasuk As Integer = 0, idhppkhususkeluar As Integer = 0, idhppfifomasuk As Integer = 0, idhppfifokeluar As Integer = 0
                Dim updNilaiSI As String = "", updFilterSI As String = "", updNilaiRNR As String = "", updFilterRNR As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT srd.idsrdetail, srd.idbarang, i.bkode as kodebarang, srd.tipebarang, srd.namabarang, srd.satuan, srd.nilaisatuan, srd.jmlbarang, srd.idsidetail, srd.idrnrdetail, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.idhppkhususkeluar, srd.idhppfifokeluar, srd.urutan, IFNULL(cso.idhppikm,0) as idhppkhususmasuk, IFNULL(cso.jmlkeluar,0) as jmlkeluar, IFNULL(cfo.cfoidcfi,0) as idhppfifomasuk, IFNULL(cfo.cfojmlkeluar,0) as cfojmlkeluar, i.bhpp, sr.srjenispenjualankategori FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m1_cogs_special_out cso ON srd.idhppkhususkeluar=cso.idhppikk LEFT JOIN m1_cogs_fifo_out cfo ON srd.idhppfifokeluar=cfo.cfoid WHERE srd.idsr = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang")
                        idsrdetail = dr1("idsrdetail") : idsidetail = dr1("idsidetail") : idrnrdetail = dr1("idrnrdetail")
                        gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan")
                        idhppkhususmasuk = dr1("idhppkhususmasuk") : idhppkhususkeluar = dr1("idhppkhususkeluar")
                        idhppfifomasuk = dr1("idhppfifomasuk") : idhppfifokeluar = dr1("idhppfifokeluar")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING SI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                            updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSI)

                            '2.2. SET FILTERUPDATE OUTSTANDING SI
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
                        End If

                        If idrnrdetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING RNR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrnrdetail=" & idrnrdetail)
                            updNilaiRNR = String.Concat("WHEN '" & idrnrdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRNR)

                            '2.2. SET FILTERUPDATE OUTSTANDING RNR
                            updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                            updFilterRNR = String.Concat(updFilterRNR, "(idrnrdetail = '" & idrnrdetail & "')")
                        End If

                        'JIKA SR TANPA RNR MAKA CEK STOK
                        If Double.Parse(dr1("srjenispenjualankategori")) = 0 Then
                            'VALIDASI STOK -------------------------------
                            '1. CEK DATA EXIST
                            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                            '2. CEK JML STOK
                            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
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
                            ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idsrdetail & "' AND sumber = 'SR')")

                            '6. BUAT FILER CEK HPP FIFO(F)
                            ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                            ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idsrdetail & "' AND cfisumber = 'SR')")

                            '7 SET NILAI UPDATE STOK BARANG
                            Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                            '8. SET FILTERUPDATE STOK BARANG
                            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                            ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                        End If

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftExistStok, ftStok, ftHppI, ftHppF, "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterSI) > 0 Then 'SI
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
                End If

                If Len(updFilterRNR) > 0 Then 'RNR
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_rnr_detail SET jmlrealisasi = (CASE idrnrdetail " & updNilaiRNR & " ELSE jmlrealisasi END) WHERE " & updFilterRNR
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idrnr FROM m5_rnr_detail WHERE " & updFilterRNR & " GROUP BY idrnr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idrnr = '" & dr1("idrnr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idrnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_rnr_detail WHERE " & ftDetail & " GROUP BY idrnr", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRNR = "" : updFilterRNR = ""
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
                            updNilaiRNR = String.Concat(updNilaiRNR, "WHEN '" & dr1("idrnr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                            updFilterRNR = String.Concat(updFilterRNR, "(rnrid = '" & dr1("idrnr") & "')")
                        Next

                        sql = "UPDATE m5_rnr SET rnrstatusrealisasi = (CASE rnrid " & updNilaiRNR & " ELSE rnrstatusrealisasi END) WHERE " & updFilterRNR
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


                'JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI =========================
                If srjenis = 1 Then
                    'AMBIL IDSI DARI DATA SR DETAIL
                    sql = "SELECT sid.idsi FROM m5_sr_detail srd JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail WHERE srd.idsr = '" & idtransaksi & "' GROUP BY sid.idsi"
                    Dim dtSI As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    Dim IdSi As Double = 0
                    If dtSI.Rows.Count > 0 Then
                        If Len(dtSI.Rows(0)("idsi")) > 0 Then
                            IdSi = Double.Parse(dtSI.Rows(0)("idsi"))
                        End If
                    End If
                    If IdSi = 0 Then
                        'AMBIL IDSI DARI DATA SR DETAIL
                        sql = "SELECT sridsi FROM m5_sr where srid = '" & idtransaksi & "' GROUP BY sridsi"
                        dtSI = AsDataTableAmbilDariDBCon(sql, myConn)
                        'result(2) = sql : GoTo selesai
                        If dtSI.Rows.Count > 0 Then
                            If Len(dtSI.Rows(0)("sridsi")) > 0 Then
                                IdSi = Double.Parse(dtSI.Rows(0)("sridsi"))
                            End If
                        End If
                    End If

                    'UPDATE JMLBAYAR SI
                    If IdSi > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET si.sijmlbayar = si.sijmlbayar - " & srtotaltransaksi & ", si.sitgllunas = '" & FixQuotes("1900-01-01") & "' WHERE si.siid = '" & IdSi & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE si.siid = '" & IdSi & "'"
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
                'END OF JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI ==================


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


                'UPDATE NO ASSET ===============================================================
                Dim dtasset As DataTable = AsDataTableAmbilDariDBCon("SELECT atasetid FROM m7_asset_transaction WHERE atsumber = '" & sumber & "' AND atidutama = '" & idtransaksi & "'", myConn)
                If dtasset.Rows.Count > 0 Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtasset.Rows
                        'QUERY INSERT NO ASSET IN
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append(FixDouble(dr1("atasetid")))
                    Next
                    sql = "UPDATE m7_asset a SET a.aakumulasibeban = 0, a.anilaibuku = 0, a.aisclose = 1 WHERE a.aid IN(" & strValue2.ToString & ")"
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
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m5_sr_detail srd ON i.bid = srd.idbarang AND srd.idsr = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m5_sr_detail srd ON it.idbarang = srd.idbarang AND srd.idsr = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m5_sr sr ON srd.idsr = sr.srid AND CONCAT(it.sumber,it.idutama) <> CONCAT(sr.srsumber,sr.srid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                If Len(updStokBarang) > 0 Then
                    sql = "  UPDATE m1_item i"
                    sql &= " JOIN ("
                    sql &= " SELECT srd.idbarang, ROUND(SUM(srd.jmlbarang * srd.hpp),2) as nilai, SUM(srd.jmlbarang) as jumlah"
                    sql &= " FROM m5_sr_detail srd"
                    sql &= " WHERE srd.idsr = '" & FixDouble(idtransaksi) & "'"
                    sql &= " GROUP BY srd.idbarang"
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
                End If
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
            sql = "UPDATE M5_Sr SET Srstatus = " & nilaiStatus & ", Srmodifikasiuser='" & userid & "', Srmodifikasitgl = NOW(), Srposting = 0, Srpostingtgl = '1971-01-01 00:00:00', Srjmlrevisi = Srjmlrevisi + 1 WHERE Srid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SrDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Srid, Srnotransaksi FROM M5_Sr WHERE Srid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT srcabang, srlokasi, srsumber, srautonotransaksi, srnotransaksi, srtgl"
            sql &= " FROM M5_sr"
            sql &= " WHERE srid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("srcabang")
                lokasi = dtNomorNext.Rows(0)("srlokasi")
                sumber = dtNomorNext.Rows(0)("srsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("srautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("srnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("srtgl"))
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
            sql = "DELETE FROM M5_Sr_Detail WHERE idsr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Sr WHERE srid='" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 5)
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SrGetdataById(ByVal param As String) As String

        'M5_SrGetdataById Utama --------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcustomtext1, 
        'srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, 
        'srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'srterminnama, srterminharijatuhtempo, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama, srrekbayarnama, 
        'srreksisanama, srnotransaksisi, srnotransaksirnr, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, 
        'ktingkatjual, srjenis, kpkp

        'M5_SrGetdataById Detail --------------------------------------------------------
        'idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, 
        'jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, 
        'hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, 
        'jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, 
        'rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, 
        'catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, 
        'idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, 
        'bhpp, bjenis, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, 
        'pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, 
        'divisinama, subdivisinama, proyeknama, sinotransaksi, rnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

        'M5_SrGetdataById Batch --------------------------------------------------------
        'nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, 
        'nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, 
        'nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang

        'M5_SrGetdataById Serial --------------------------------------------------------
        'nstid, nstjenismutasi, nstidserialin, nstgudang,  nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, 
        'nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, 
        'nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang

        'M5_SrGetdataById Asset --------------------------------------------------------
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
        Dim sumber As String = "SR", asset As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_sr~M5_sr_Detail-" & idtransaksi

        'replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "srid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "srid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_sr_getdata")
        'sql = "select `sr`.`srid` AS `srid`,`sr`.`srcabang` AS `srcabang`,`sr`.`srlokasi` AS `srlokasi`,`sr`.`srgudang` AS `srgudang`,`sr`.`srasalbarang` AS `srasalbarang`,`sr`.`srasalbarangkategori` AS `srasalbarangkategori`,`sr`.`srjenispenjulan` AS `srjenispenjulan`,`sr`.`srjenispenjualankategori` AS `srjenispenjualankategori`,`sr`.`srcarabayar` AS `srcarabayar`,`sr`.`srsumber` AS `srsumber`,`sr`.`srautonotransaksi` AS `srautonotransaksi`,`sr`.`srnotransaksi` AS `srnotransaksi`,`sr`.`srtgl` AS `srtgl`,`sr`.`srkodepa` AS `srkodepa`,`sr`.`srcustomer` AS `srcustomer`,`sr`.`srcustomerkontak` AS `srcustomerkontak`,`sr`.`sr1alamat1` AS `sr1alamat1`,`sr`.`sr1alamat2` AS `sr1alamat2`,`sr`.`sr1alamat3` AS `sr1alamat3`,`sr`.`sr2alamat1` AS `sr2alamat1`,`sr`.`sr2alamat2` AS `sr2alamat2`,`sr`.`sr2alamat3` AS `sr2alamat3`,`sr`.`srbagianpenjualan` AS `srbagianpenjualan`,`sr`.`srekspedisi` AS `srekspedisi`,`sr`.`srtglkirim` AS `srtglkirim`,`sr`.`srtermin` AS `srtermin`,`sr`.`srtgljatuhtempo` AS `srtgljatuhtempo`,`sr`.`sruraian` AS `sruraian`,`sr`.`srcatatan` AS `srcatatan`,`sr`.`srnoref` AS `srnoref`,`sr`.`srtglnoref` AS `srtglnoref`,`sr`.`srtglpenutupan` AS `srtglpenutupan`,`sr`.`srmatauang` AS `srmatauang`,`sr`.`srkurs` AS `srkurs`,`sr`.`srhargatermasukpajak` AS `srhargatermasukpajak`,`sr`.`srtotal` AS `srtotal`,`sr`.`srdiskonpersen` AS `srdiskonpersen`,`sr`.`srjmldiskon` AS `srjmldiskon`,`sr`.`srtotalpajak1detail` AS `srtotalpajak1detail`,`sr`.`srtotalpajak2detail` AS `srtotalpajak2detail`,`sr`.`srbiayalainpersen` AS `srbiayalainpersen`,`sr`.`srbiayalain` AS `srbiayalain`,`sr`.`srtotaltransaksi` AS `srtotaltransaksi`,`sr`.`srsisatransaksi` AS `srsisatransaksi`,`sr`.`srjmlbayar` AS `srjmlbayar`,`sr`.`srstatuslunas` AS `srstatuslunas`,`sr`.`srtgllunas` AS `srtgllunas`,`sr`.`srnofakturpajak` AS `srnofakturpajak`,`sr`.`srsdhbayarpajak` AS `srsdhbayarpajak`,`sr`.`srtglbayarpajak` AS `srtglbayarpajak`,`sr`.`srrekdiskon` AS `srrekdiskon`,`sr`.`srrekpajak1` AS `srrekpajak1`,`sr`.`srrekpajak2` AS `srrekpajak2`,`sr`.`srrekbiayalain` AS `srrekbiayalain`,`sr`.`srreksisa` AS `srreksisa`,`sr`.`srrekbayar` AS `srrekbayar`,`sr`.`sridsq` AS `sridsq`,`sr`.`sridso` AS `sridso`,`sr`.`sridpl` AS `sridpl`,`sr`.`sriddo` AS `sriddo`,`sr`.`sriddr` AS `sriddr`,`sr`.`sridpi` AS `sridpi`,`sr`.`sridsi` AS `sridsi`,`sr`.`sridrnr` AS `sridrnr`,`sr`.`srstatus` AS `srstatus`,`sr`.`srstatussebelumnya` AS `srstatussebelumnya`,`sr`.`srjmlrevisi` AS `srjmlrevisi`,`sr`.`srcetakanke` AS `srcetakanke`,`sr`.`srinputuser` AS `srinputuser`,`sr`.`srinputtgl` AS `srinputtgl`,`sr`.`srmodifikasiuser` AS `srmodifikasiuser`,`sr`.`srmodifikasitgl` AS `srmodifikasitgl`,`sr`.`srposting` AS `srposting`,`sr`.`srpostingtgl` AS `srpostingtgl`,`sr`.`srtutupperiode` AS `srtutupperiode`,`sr`.`srisclose` AS `srisclose`,`sr`.`srcustomtext1` AS `srcustomtext1`,`sr`.`srcustomtext2` AS `srcustomtext2`,`sr`.`srcustomtext3` AS `srcustomtext3`,`sr`.`srcustomtext4` AS `srcustomtext4`,`sr`.`srcustomtext5` AS `srcustomtext5`,`sr`.`srcustomint1` AS `srcustomint1`,`sr`.`srcustomint2` AS `srcustomint2`,`sr`.`srcustomint3` AS `srcustomint3`,`sr`.`srcustomdbl1` AS `srcustomdbl1`,`sr`.`srcustomdbl2` AS `srcustomdbl2`,`sr`.`srcustomdbl3` AS `srcustomdbl3`,`sr`.`srcustomdate1` AS `srcustomdate1`,`sr`.`srcustomdate2` AS `srcustomdate2`,`sr`.`srcustomdate3` AS `srcustomdate3`,`br`.`bnama` AS `srcabangnama`,`lc`.`lnama` AS `srlokasinama`,`wh`.`wnama` AS `srgudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `srcustomerkode`,`c1`.`knama` AS `srcustomernama`,`c2`.`kkode` AS `srbagianpenjualankode`,`c2`.`knama` AS `srbagianpenjualannama`,`e`.`enama` AS `srekspedisinama`,`tr`.`trnama` AS `srterminnama`,`tr`.`trharijatuhtempo` AS `srterminharijatuhtempo`,`coa1`.`cnama` AS `srrekdiskonnama`,`coa2`.`cnama` AS `srrekpajak1nama`,`coa3`.`cnama` AS `srrekpajak2nama`,`coa4`.`cnama` AS `srrekbiayalainnama`,`coa5`.`cnama` AS `srrekbayarnama`,`coa6`.`cnama` AS `srreksisanama`,`si`.`sinotransaksi` AS `srnotransaksisi`,`rnr`.`rnrnotransaksi` AS `srnotransaksirnr`,`st1`.`nama` AS `srstatusnama`,`st2`.`nama` AS `srstatussebelumnyanama`,`u1`.`unama` AS `srinputusernama`,`u2`.`unama` AS `srmodifikasiusernama`, sr.srjenis, `srd`.`idsrdetail` AS `idsrdetail`,`srd`.`idsr` AS `idsr`,`srd`.`idbarang` AS `idbarang`,`srd`.`namabarang` AS `namabarang`,`srd`.`tipebarang` AS `tipebarang`,`srd`.`jml` AS `jml`,`srd`.`satuan` AS `satuan`,`srd`.`nilaisatuan` AS `nilaisatuan`,`srd`.`jmlbarang` AS `jmlbarang`,`srd`.`satuanbarang` AS `satuanbarang`,`srd`.`matauang` AS `matauang`,`srd`.`kurs` AS `kurs`,`srd`.`idhppkhususkeluar` AS `idhppkhususkeluar`,`srd`.`idhppfifokeluar` AS `idhppfifokeluar`,`srd`.`harga` AS `harga`,`srd`.`hargapricelist` AS `hargapricelist`,`srd`.`hpp` AS `hpp`,`srd`.`diskon` AS `diskon`,`srd`.`jmldiskon` AS `jmldiskon`,`srd`.`pajak1` AS `pajak1`,`srd`.`jmlpajak1` AS `jmlpajak1`,`srd`.`pajak2` AS `pajak2`,`srd`.`jmlpajak2` AS `jmlpajak2`,`srd`.`cabang` AS `cabang`,`srd`.`lokasi` AS `lokasi`,`srd`.`gudangasal` AS `gudangasal`,`srd`.`gudangtransit` AS `gudangtransit`,`srd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekdiskonpenjualan` AS `rekdiskonpenjualan`,`i`.`brekreturpenjualan` AS `rekreturpenjualan`,`srd`.`costcenter` AS `costcenter`,`srd`.`divisi` AS `divisi`,`srd`.`subdivisi` AS `subdivisi`,`srd`.`proyek` AS `proyek`,`srd`.`catatan` AS `catatan`,`srd`.`urutan` AS `urutan`,`srd`.`idsqdetail` AS `idsqdetail`,`srd`.`idsodetail` AS `idsodetail`,`srd`.`idpldetail` AS `idpldetail`,`srd`.`iddodetail` AS `iddodetail`,`srd`.`iddrdetail` AS `iddrdetail`,`srd`.`idpidetail` AS `idpidetail`,`srd`.`idsidetail` AS `idsidetail`,`srd`.`idrnrdetail` AS `idrnrdetail`,`srd`.`isclose` AS `isclose`,`srd`.`customtext1` AS `customtext1`,`srd`.`customtext2` AS `customtext2`,`srd`.`customtext3` AS `customtext3`,`srd`.`customdbl1` AS `customdbl1`,`srd`.`customdbl2` AS `customdbl2`,`srd`.`customdbl3` AS `customdbl3`,`srd`.`customdate1` AS `customdate1`,`srd`.`customdate2` AS `customdate2`,`srd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`si2`.`sinotransaksi` AS `sinotransaksi`,`rnr2`.`rnrnotransaksi` AS `rnrnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from ((((((((((((((((((((((((((((((((((((`m5_sr` `sr` join `m5_sr_detail` `srd` on((`sr`.`srid` = `srd`.`idsr`))) left join `m1_branch` `br` on((`br`.`bkode` = `sr`.`srcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sr`.`srlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sr`.`srgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sr`.`srcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `sr`.`srbagianpenjualan`))) left join `m1_expedition` `e` on((`sr`.`srekspedisi` = `e`.`ekode`))) left join `m1_terms` `tr` on((`sr`.`srtermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`sr`.`srrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`sr`.`srrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`sr`.`srrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`sr`.`srrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`sr`.`srrekbayar` = `coa5`.`cnomor`))) left join `m1_coa` `coa6` on((`sr`.`srreksisa` = `coa6`.`cnomor`))) left join `m5_si` `si` on((`sr`.`sridsi` = `si`.`siid`))) left join `m5_rnr` `rnr` on((`sr`.`sridrnr` = `rnr`.`rnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `sr`.`srstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sr`.`srstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sr`.`srinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sr`.`srmodifikasiuser`))) left join `m1_cost_center` `cc` on((`srd`.`costcenter` = `cc`.`cckode`))) left join `m1_warehouse` `whd1` on((`srd`.`gudangasal` = `whd1`.`wkode`))) left join `m1_warehouse` `whd2` on((`srd`.`gudangtransit` = `whd2`.`wkode`))) left join `m1_warehouse` `whd3` on((`srd`.`gudangtujuan` = `whd3`.`wkode`))) left join `m1_location` `lcd` on((`srd`.`lokasi` = `lcd`.`lkode`))) left join `m1_tax` `t1` on((`srd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`srd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`srd`.`cabang` = `brd`.`bkode`))) left join `m1_division` `d` on((`srd`.`divisi` = `d`.`dkode`))) left join `m1_project` `p` on((`srd`.`proyek` = `p`.`pkode`))) left join `m1_subdivision` `sd` on((`srd`.`subdivisi` = `sd`.`sdkode`))) left join `m5_rnr_detail` `rnrd` on((`srd`.`idrnrdetail` = `rnrd`.`idrnrdetail`))) left join `m5_rnr` `rnr2` on((`rnrd`.`idrnr` = `rnr2`.`rnrid`))) left join `m5_si_detail` `sid` on((`srd`.`idsidetail` = `sid`.`idsidetail`))) left join `m5_si` `si2` on((`sid`.`idsi` = `si2`.`siid`))) left join `m1_item` `i` on((`i`.`bid` = `srd`.`idbarang`)))"
        sql = "select `sr`.`srid` AS `srid`,`sr`.`srcabang` AS `srcabang`,`sr`.`srlokasi` AS `srlokasi`,`sr`.`srgudang` AS `srgudang`,`sr`.`srasalbarang` AS `srasalbarang`,`sr`.`srasalbarangkategori` AS `srasalbarangkategori`,`sr`.`srjenispenjulan` AS `srjenispenjulan`,`sr`.`srjenispenjualankategori` AS `srjenispenjualankategori`,`sr`.`srcarabayar` AS `srcarabayar`,`sr`.`srsumber` AS `srsumber`,`sr`.`srautonotransaksi` AS `srautonotransaksi`,`sr`.`srnotransaksi` AS `srnotransaksi`,`sr`.`srtgl` AS `srtgl`,`sr`.`srkodepa` AS `srkodepa`,`sr`.`srcustomer` AS `srcustomer`,`sr`.`srcustomerkontak` AS `srcustomerkontak`,`sr`.`sr1alamat1` AS `sr1alamat1`,`sr`.`sr1alamat2` AS `sr1alamat2`,`sr`.`sr1alamat3` AS `sr1alamat3`,`sr`.`sr2alamat1` AS `sr2alamat1`,`sr`.`sr2alamat2` AS `sr2alamat2`,`sr`.`sr2alamat3` AS `sr2alamat3`,`sr`.`srbagianpenjualan` AS `srbagianpenjualan`,`sr`.`srekspedisi` AS `srekspedisi`,`sr`.`srtglkirim` AS `srtglkirim`,`sr`.`srtermin` AS `srtermin`,`sr`.`srtgljatuhtempo` AS `srtgljatuhtempo`,`sr`.`sruraian` AS `sruraian`,`sr`.`srcatatan` AS `srcatatan`,`sr`.`srnoref` AS `srnoref`,`sr`.`srtglnoref` AS `srtglnoref`,`sr`.`srtglpenutupan` AS `srtglpenutupan`,`sr`.`srmatauang` AS `srmatauang`,`sr`.`srkurs` AS `srkurs`,`sr`.`srhargatermasukpajak` AS `srhargatermasukpajak`,`sr`.`srtotal` AS `srtotal`,`sr`.`srdiskonpersen` AS `srdiskonpersen`,`sr`.`srjmldiskon` AS `srjmldiskon`,`sr`.`srtotalpajak1detail` AS `srtotalpajak1detail`,`sr`.`srtotalpajak2detail` AS `srtotalpajak2detail`,`sr`.`srbiayalainpersen` AS `srbiayalainpersen`,`sr`.`srbiayalain` AS `srbiayalain`,`sr`.`srtotaltransaksi` AS `srtotaltransaksi`,`sr`.`srsisatransaksi` AS `srsisatransaksi`,`sr`.`srjmlbayar` AS `srjmlbayar`,`sr`.`srstatuslunas` AS `srstatuslunas`,`sr`.`srtgllunas` AS `srtgllunas`,`sr`.`srnofakturpajak` AS `srnofakturpajak`,`sr`.`srsdhbayarpajak` AS `srsdhbayarpajak`,`sr`.`srtglbayarpajak` AS `srtglbayarpajak`,`sr`.`srrekdiskon` AS `srrekdiskon`,`sr`.`srrekpajak1` AS `srrekpajak1`,`sr`.`srrekpajak2` AS `srrekpajak2`,`sr`.`srrekbiayalain` AS `srrekbiayalain`,`sr`.`srreksisa` AS `srreksisa`,`sr`.`srrekbayar` AS `srrekbayar`,`sr`.`sridsq` AS `sridsq`,`sr`.`sridso` AS `sridso`,`sr`.`sridpl` AS `sridpl`,`sr`.`sriddo` AS `sriddo`,`sr`.`sriddr` AS `sriddr`,`sr`.`sridpi` AS `sridpi`,`sr`.`sridsi` AS `sridsi`,`sr`.`sridrnr` AS `sridrnr`,`sr`.`srstatus` AS `srstatus`,`sr`.`srstatussebelumnya` AS `srstatussebelumnya`,`sr`.`srjmlrevisi` AS `srjmlrevisi`,`sr`.`srcetakanke` AS `srcetakanke`,`sr`.`srinputuser` AS `srinputuser`,`sr`.`srinputtgl` AS `srinputtgl`,`sr`.`srmodifikasiuser` AS `srmodifikasiuser`,`sr`.`srmodifikasitgl` AS `srmodifikasitgl`,`sr`.`srposting` AS `srposting`,`sr`.`srpostingtgl` AS `srpostingtgl`,`sr`.`srtutupperiode` AS `srtutupperiode`,`sr`.`srisclose` AS `srisclose`,`sr`.`srcustomtext1` AS `srcustomtext1`,`sr`.`srcustomtext2` AS `srcustomtext2`,`sr`.`srcustomtext3` AS `srcustomtext3`,`sr`.`srcustomtext4` AS `srcustomtext4`,`sr`.`srcustomtext5` AS `srcustomtext5`,`sr`.`srcustomint1` AS `srcustomint1`,`sr`.`srcustomint2` AS `srcustomint2`,`sr`.`srcustomint3` AS `srcustomint3`,`sr`.`srcustomdbl1` AS `srcustomdbl1`,`sr`.`srcustomdbl2` AS `srcustomdbl2`,`sr`.`srcustomdbl3` AS `srcustomdbl3`,`sr`.`srcustomdate1` AS `srcustomdate1`,`sr`.`srcustomdate2` AS `srcustomdate2`,`sr`.`srcustomdate3` AS `srcustomdate3`,`br`.`bnama` AS `srcabangnama`,`lc`.`lnama` AS `srlokasinama`,`wh`.`wnama` AS `srgudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `srcustomerkode`,`c1`.`knama` AS `srcustomernama`,`c2`.`kkode` AS `srbagianpenjualankode`,`c2`.`knama` AS `srbagianpenjualannama`,`e`.`enama` AS `srekspedisinama`,`tr`.`trnama` AS `srterminnama`,`tr`.`trharijatuhtempo` AS `srterminharijatuhtempo`,`coa1`.`cnama` AS `srrekdiskonnama`,`coa2`.`cnama` AS `srrekpajak1nama`,`coa3`.`cnama` AS `srrekpajak2nama`,`coa4`.`cnama` AS `srrekbiayalainnama`,`coa5`.`cnama` AS `srrekbayarnama`,`coa6`.`cnama` AS `srreksisanama`,`si`.`sinotransaksi` AS `srnotransaksisi`,`rnr`.`rnrnotransaksi` AS `srnotransaksirnr`,`st1`.`nama` AS `srstatusnama`,`st2`.`nama` AS `srstatussebelumnyanama`,`u1`.`unama` AS `srinputusernama`,`u2`.`unama` AS `srmodifikasiusernama`, sr.srjenis, `srd`.`idsrdetail` AS `idsrdetail`,`srd`.`idsr` AS `idsr`,`srd`.`idbarang` AS `idbarang`,`srd`.`namabarang` AS `namabarang`,`srd`.`tipebarang` AS `tipebarang`,`srd`.`jml` AS `jml`,`srd`.`satuan` AS `satuan`,`srd`.`nilaisatuan` AS `nilaisatuan`,`srd`.`jmlbarang` AS `jmlbarang`,`srd`.`satuanbarang` AS `satuanbarang`,`srd`.`matauang` AS `matauang`,`srd`.`kurs` AS `kurs`,`srd`.`idhppkhususkeluar` AS `idhppkhususkeluar`,`srd`.`idhppfifokeluar` AS `idhppfifokeluar`,`srd`.`harga` AS `harga`,`srd`.`hargapricelist` AS `hargapricelist`,`srd`.`hpp` AS `hpp`,`srd`.`diskon` AS `diskon`,`srd`.`jmldiskon` AS `jmldiskon`,`srd`.`pajak1` AS `pajak1`,`srd`.`jmlpajak1` AS `jmlpajak1`,`srd`.`pajak2` AS `pajak2`,`srd`.`jmlpajak2` AS `jmlpajak2`,`srd`.`cabang` AS `cabang`,`srd`.`lokasi` AS `lokasi`,`srd`.`gudangasal` AS `gudangasal`,`srd`.`gudangtransit` AS `gudangtransit`,`srd`.`gudangtujuan` AS `gudangtujuan`,`i`.`brekpersediaan` AS `rekpersediaan`,`i`.`brekhargapokok` AS `rekhargapokok`,`i`.`brekdiskonpenjualan` AS `rekdiskonpenjualan`,`i`.`brekreturpenjualan` AS `rekreturpenjualan`,`srd`.`costcenter` AS `costcenter`,`srd`.`divisi` AS `divisi`,`srd`.`subdivisi` AS `subdivisi`,`srd`.`proyek` AS `proyek`,`srd`.`catatan` AS `catatan`,`srd`.`urutan` AS `urutan`,`srd`.`idsqdetail` AS `idsqdetail`,`srd`.`idsodetail` AS `idsodetail`,`srd`.`idpldetail` AS `idpldetail`,`srd`.`iddodetail` AS `iddodetail`,`srd`.`iddrdetail` AS `iddrdetail`,`srd`.`idpidetail` AS `idpidetail`,`srd`.`idsidetail` AS `idsidetail`,`srd`.`idrnrdetail` AS `idrnrdetail`,`srd`.`isclose` AS `isclose`,`srd`.`customtext1` AS `customtext1`,`srd`.`customtext2` AS `customtext2`,`srd`.`customtext3` AS `customtext3`,`srd`.`customdbl1` AS `customdbl1`,`srd`.`customdbl2` AS `customdbl2`,`srd`.`customdbl3` AS `customdbl3`,`srd`.`customdate1` AS `customdate1`,`srd`.`customdate2` AS `customdate2`,`srd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd1`.`wnama` AS `gudangasalnama`,`whd2`.`wnama` AS `gudangtransitnama`,`whd3`.`wnama` AS `gudangtujuannama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`si2`.`sinotransaksi` AS `sinotransaksi`,`rnr2`.`rnrnotransaksi` AS `rnrnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, cp.kkode AS bagpengirimankode, cp.knama AS bagpengirimannama from `m5_sr` `sr` join `m5_sr_detail` `srd` on`sr`.`srid` = `srd`.`idsr` left join `m1_branch` `br` on`br`.`bkode` = `sr`.`srcabang` left join `m1_location` `lc` on`lc`.`lkode` = `sr`.`srlokasi` left join `m1_warehouse` `wh` on`wh`.`wkode` = `sr`.`srgudang` left join `m1_contact` `c1` on`c1`.`kid` = `sr`.`srcustomer` left join `m1_contact` `c2` on`c2`.`kid` = `sr`.`srbagianpenjualan` left join `m1_expedition` `e` on`sr`.`srekspedisi` = `e`.`ekode` left join `m1_terms` `tr` on`sr`.`srtermin` = `tr`.`trkode` left join `m1_coa` `coa1` on`sr`.`srrekdiskon` = `coa1`.`cnomor` left join `m1_coa` `coa2` on`sr`.`srrekpajak1` = `coa2`.`cnomor` left join `m1_coa` `coa3` on`sr`.`srrekpajak2` = `coa3`.`cnomor` left join `m1_coa` `coa4` on`sr`.`srrekbiayalain` = `coa4`.`cnomor` left join `m1_coa` `coa5` on`sr`.`srrekbayar` = `coa5`.`cnomor` left join `m1_coa` `coa6` on`sr`.`srreksisa` = `coa6`.`cnomor` left join `m5_si` `si` on`sr`.`sridsi` = `si`.`siid` left join `m5_rnr` `rnr` on`sr`.`sridrnr` = `rnr`.`rnrid` left join `m0_status` `st1` on`st1`.`kode` = `sr`.`srstatus` left join `m0_status` `st2` on`st2`.`kode` = `sr`.`srstatussebelumnya` left join `m0_user` `u1` on`u1`.`userid` = `sr`.`srinputuser` left join `m0_user` `u2` on`u2`.`userid` = `sr`.`srmodifikasiuser` left join `m1_cost_center` `cc` on`srd`.`costcenter` = `cc`.`cckode` left join `m1_warehouse` `whd1` on`srd`.`gudangasal` = `whd1`.`wkode` left join `m1_warehouse` `whd2` on`srd`.`gudangtransit` = `whd2`.`wkode` left join `m1_warehouse` `whd3` on`srd`.`gudangtujuan` = `whd3`.`wkode` left join `m1_location` `lcd` on`srd`.`lokasi` = `lcd`.`lkode` left join `m1_tax` `t1` on`srd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on`srd`.`pajak2` = `t2`.`tkode` left join `m1_branch` `brd` on`srd`.`cabang` = `brd`.`bkode` left join `m1_division` `d` on`srd`.`divisi` = `d`.`dkode` left join `m1_project` `p` on`srd`.`proyek` = `p`.`pkode` left join `m1_subdivision` `sd` on`srd`.`subdivisi` = `sd`.`sdkode` left join `m5_rnr_detail` `rnrd` on`srd`.`idrnrdetail` = `rnrd`.`idrnrdetail` left join `m5_rnr` `rnr2` on`rnrd`.`idrnr` = `rnr2`.`rnrid` left join `m5_si_detail` `sid` on`srd`.`idsidetail` = `sid`.`idsidetail` left join `m5_si` `si2` on`sid`.`idsi` = `si2`.`siid` left join `m1_item` `i` on`i`.`bid` = `srd`.`idbarang` LEFT JOIN m1_contact cp ON cp.kid = sr.srcustomdbl2"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("srid"), 0), sptField,
                     FxDB(drutama("srcabang"), ""), sptField,
                     FxDB(drutama("srlokasi"), ""), sptField,
                     FxDB(drutama("srgudang"), ""), sptField,
                     FxDB(drutama("srasalbarang"), ""), sptField,
                     FxDB(drutama("srasalbarangkategori"), 0), sptField,
                     FxDB(drutama("srjenispenjulan"), ""), sptField,
                     FxDB(drutama("srjenispenjualankategori"), 0), sptField,
                     FxDB(drutama("srcarabayar"), 0), sptField,
                     FxDB(drutama("srsumber"), ""), sptField,
                     FxDB(drutama("srautonotransaksi"), 0), sptField,
                     FxDB(drutama("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgl"), ""), formatTgl), sptField,
                     FxDB(drutama("srkodepa"), 0), sptField,
                     FxDB(drutama("srcustomer"), 0), sptField,
                     FxDB(drutama("srcustomerkontak"), ""), sptField,
                     FxDB(drutama("sr1alamat1"), ""), sptField,
                     FxDB(drutama("sr1alamat2"), ""), sptField,
                     FxDB(drutama("sr1alamat3"), ""), sptField,
                     FxDB(drutama("sr2alamat1"), ""), sptField,
                     FxDB(drutama("sr2alamat2"), ""), sptField,
                     FxDB(drutama("sr2alamat3"), ""), sptField,
                     FxDB(drutama("srbagianpenjualan"), 0), sptField,
                     FxDB(drutama("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("sruraian"), ""), sptField,
                     FxDB(drutama("srcatatan"), ""), sptField,
                     FxDB(drutama("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("srmatauang"), ""), sptField,
                     FxDB(drutama("srkurs"), 0), sptField,
                     FxDB(drutama("srhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("srtotal"), 0), sptField,
                     FxDB(drutama("srdiskonpersen"), ""), sptField,
                     FxDB(drutama("srjmldiskon"), 0), sptField,
                     FxDB(drutama("srtotalpajak1detail"), 0), sptField,
                     FxDB(drutama("srtotalpajak2detail"), 0), sptField,
                     FxDB(drutama("srbiayalainpersen"), 0), sptField,
                     FxDB(drutama("srbiayalain"), 0), sptField,
                     FxDB(drutama("srtotaltransaksi"), 0), sptField,
                     FxDB(drutama("srsisatransaksi"), 0), sptField,
                     FxDB(drutama("srjmlbayar"), 0), sptField,
                     FxDB(drutama("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(drutama("srnofakturpajak"), ""), sptField,
                     FxDB(drutama("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(drutama("srrekdiskon"), ""), sptField,
                     FxDB(drutama("srrekpajak1"), ""), sptField,
                     FxDB(drutama("srrekpajak2"), ""), sptField,
                     FxDB(drutama("srrekbiayalain"), ""), sptField,
                     FxDB(drutama("srreksisa"), ""), sptField,
                     FxDB(drutama("srrekbayar"), ""), sptField,
                     FxDB(drutama("sridsq"), 0), sptField,
                     FxDB(drutama("sridso"), 0), sptField,
                     FxDB(drutama("sridpl"), 0), sptField,
                     FxDB(drutama("sriddo"), 0), sptField,
                     FxDB(drutama("sriddr"), 0), sptField,
                     FxDB(drutama("sridpi"), 0), sptField,
                     FxDB(drutama("sridsi"), 0), sptField,
                     FxDB(drutama("sridrnr"), 0), sptField,
                     FxDB(drutama("srstatus"), 0), sptField,
                     FxDB(drutama("srstatussebelumnya"), 0), sptField,
                     FxDB(drutama("srjmlrevisi"), 0), sptField,
                     FxDB(drutama("srcetakanke"), 0), sptField,
                     FxDB(drutama("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("srtutupperiode"), 0), sptField,
                     FxDB(drutama("srisclose"), 0), sptField,
                     FxDB(drutama("srcustomtext1"), ""), sptField,
                     FxDB(drutama("srcustomtext2"), ""), sptField,
                     FxDB(drutama("srcustomtext3"), ""), sptField,
                     FxDB(drutama("srcustomtext4"), ""), sptField,
                     FxDB(drutama("srcustomtext5"), ""), sptField,
                     FxDB(drutama("srcustomint1"), 0), sptField,
                     FxDB(drutama("srcustomint2"), 0), sptField,
                     FxDB(drutama("srcustomint3"), 0), sptField,
                     FxDB(drutama("srcustomdbl1"), 0), sptField,
                     FxDB(drutama("srcustomdbl2"), 0), sptField,
                     FxDB(drutama("srcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("srcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("srcabangnama"), ""), sptField,
                     FxDB(drutama("srlokasinama"), ""), sptField,
                     FxDB(drutama("srgudangnama"), ""), sptField,
                     FxDB(drutama("srcustomerkode"), ""), sptField,
                     FxDB(drutama("srcustomernama"), ""), sptField,
                     FxDB(drutama("srbagianpenjualankode"), ""), sptField,
                     FxDB(drutama("srbagianpenjualannama"), ""), sptField,
                     FxDB(drutama("srekspedisinama"), ""), sptField,
                     FxDB(drutama("srterminnama"), ""), sptField,
                     FxDB(drutama("srterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("srrekdiskonnama"), ""), sptField,
                     FxDB(drutama("srrekpajak1nama"), ""), sptField,
                     FxDB(drutama("srrekpajak2nama"), ""), sptField,
                     FxDB(drutama("srrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("srrekbayarnama"), ""), sptField,
                     FxDB(drutama("srreksisanama"), ""), sptField,
                     FxDB(drutama("srnotransaksisi"), ""), sptField,
                     FxDB(drutama("srnotransaksirnr"), ""), sptField,
                     FxDB(drutama("srstatusnama"), ""), sptField,
                     FxDB(drutama("srstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("srinputusernama"), ""), sptField,
                     FxDB(drutama("srmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("srjenis"), 0), sptField,
                     FxDB(drutama("bagpengirimankode"), ""), sptField,
                     FxDB(drutama("bagpengirimannama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsrdetail"), 0), sptField,
                     FxDB(dr("idsr"), 0), sptField,
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
                     FxDB(dr("idrnrdetail"), 0), sptField,
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
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
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
            result(2) = "Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, batch, sptSubParam, serial, sptSubParam, asset)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, srterminnama, srterminharijatuhtempo, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama, srrekbayarnama, srreksisanama, srnotransaksisi, srnotransaksirnr, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, ktingkatjual, srjenis, srbagianpengirimankode, srbagianpengirimannama, kpkp" & sptSubParam & "idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, bhpp, bjenis, bserial, bbatch, basset, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangasalnama, gudangtransitnama, gudangtujuannama, costcenternama, divisinama, subdivisinama, proyeknama, sinotransaksi, rnrnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "nbtid, nbtjenismutasi, nbtidbatchin, nbtgudang, nbtidbarang, nbtkode, nbtsumber, nbtidtransaksi, nbtsatuan, nbtjml, nbtcustomtext1, nbtcustomtext2, nbtcustomtext3, nbtcustomdbl1, nbtcustomdbl2, nbtcustomdbl3, nbtcustomdate1, nbtcustomdate2, nbtcustomdate3, kodebarang" & sptSubParam & "nstid, nstjenismutasi, nstidserialin, nstgudang, nstidbarang, nstkode, nstsumber, nstidtransaksi, nstsatuan, nstjml, nstcustomtext1, nstcustomtext2, nstcustomtext3, nstcustomdbl1, nstcustomdbl2, nstcustomdbl3, nstcustomdate1, nstcustomdate2, nstcustomdate3, kodebarang" & sptSubParam & "atid, atasetid, atjenismutasi, atsumber, atidutama, atidbarang, atkode, atnama, atkategori, atcabang, atlokasi, atgudang, atdivisi, atsubdivisi, atcostcenter, atproyek, atcatatan, atnomor, attglbeli, attglpakai, atjml, atsatuan, atmatauang, atkurs, atharga, atdiskon, atjmldiskon, atpajak1, atjmlpajak1, atpajak2, atjmlpajak2, athargabeli, atnilairesidu, atumurekonomis, atbebanperbln, atakumulasibeban, atnilaibuku, atnilaipenyusutan, atmetode, attabelpenyusutan, atintangible, atfiskal, atatastengahbulan, atrekasset, atrekakumdepresiasi, atrekdepresiasi, atrekpenghapusan, atprodusen, attglpensiun, atpenyusutanke, atnilaimenurun, atdispose, atpembelian, atpenjualan, atlocked, atstatus, atstatussebelumnya, atisclose, atinputuser, atinputtgl, atmodifikasiuser, atmodifikasitgl, atcustomtext1, atcustomtext2, atcustomtext3, atcustomtext4, atcustomtext5, atcustomint1, atcustomint2, atcustomint3, atcustomint4, atcustomint5, atcustomdbl1, atcustomdbl2, atcustomdbl3, atcustomdbl4, atcustomdbl5, atcustomdate1, atcustomdate2, atcustomdate3, atcustomdate4, atcustomdate5, atkategorinama, atcabangnama, atlokasinama, atgudangnama, atdivisinama, atsubdivisinama, atcostcenternama, atproyeknama, atmetodenama, atpajak1nama, atpajak1nilai, atpajak2nama, atpajak2nilai, atrekassetnama, atrekakumdepresiasinama, atrekdepresiasinama, atrekpenghapusannama, atprodusenkode, atprodusennama, atstatusnama, atstatussebelumnyanama, atinputusernama, atmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SrSearch(ByVal param As String) As String
        'M5_SrSearch --------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srjenis, srjenisnama

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
            Filter = Filter.Replace("srcustomerkode", "c1.kkode")
            Filter = Filter.Replace("srcustomernama", "c1.knama")
            Filter = Filter.Replace("Srcustomernama", "c1.knama")
            Filter = Filter.Replace("Srstatusnama", "`st1`.`nama`")
            Filter = Filter.Replace("Srinputusernama", "`u1`.`unama`")
            Filter = Filter.Replace("Srmodifikasiusernama", "`u2`.`unama`")

        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_sr_v")

        dt = AmbilData("aplikasi1-M5_Sr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("srid"), 0), sptField,
                     FxDB(dr("srcabang"), ""), sptField,
                     FxDB(dr("srlokasi"), ""), sptField,
                     FxDB(dr("srgudang"), ""), sptField,
                     FxDB(dr("srasalbarang"), ""), sptField,
                     FxDB(dr("srasalbarangkategori"), 0), sptField,
                     FxDB(dr("srjenispenjulan"), ""), sptField,
                     FxDB(dr("srjenispenjualankategori"), 0), sptField,
                     FxDB(dr("srcarabayar"), 0), sptField,
                     FxDB(dr("srsumber"), ""), sptField,
                     FxDB(dr("srautonotransaksi"), 0), sptField,
                     FxDB(dr("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgl"), ""), formatTgl), sptField,
                     FxDB(dr("srkodepa"), 0), sptField,
                     FxDB(dr("srcustomer"), 0), sptField,
                     FxDB(dr("srcustomerkontak"), ""), sptField,
                     FxDB(dr("sr1alamat1"), ""), sptField,
                     FxDB(dr("sr1alamat2"), ""), sptField,
                     FxDB(dr("sr1alamat3"), ""), sptField,
                     FxDB(dr("sr2alamat1"), ""), sptField,
                     FxDB(dr("sr2alamat2"), ""), sptField,
                     FxDB(dr("sr2alamat3"), ""), sptField,
                     FxDB(dr("srbagianpenjualan"), 0), sptField,
                     FxDB(dr("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sruraian"), ""), sptField,
                     FxDB(dr("srcatatan"), ""), sptField,
                     FxDB(dr("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("srmatauang"), ""), sptField,
                     FxDB(dr("srkurs"), 0), sptField,
                     FxDB(dr("srhargatermasukpajak"), 0), sptField,
                     FxDB(dr("srtotal"), 0), sptField,
                     FxDB(dr("srdiskonpersen"), ""), sptField,
                     FxDB(dr("srjmldiskon"), 0), sptField,
                     FxDB(dr("srtotalpajak1detail"), 0), sptField,
                     FxDB(dr("srtotalpajak2detail"), 0), sptField,
                     FxDB(dr("srbiayalainpersen"), 0), sptField,
                     FxDB(dr("srbiayalain"), 0), sptField,
                     FxDB(dr("srtotaltransaksi"), 0), sptField,
                     FxDB(dr("srsisatransaksi"), 0), sptField,
                     FxDB(dr("srjmlbayar"), 0), sptField,
                     FxDB(dr("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("srnofakturpajak"), ""), sptField,
                     FxDB(dr("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskon"), ""), sptField,
                     FxDB(dr("srrekpajak1"), ""), sptField,
                     FxDB(dr("srrekpajak2"), ""), sptField,
                     FxDB(dr("srrekbiayalain"), ""), sptField,
                     FxDB(dr("srreksisa"), ""), sptField,
                     FxDB(dr("srrekbayar"), ""), sptField,
                     FxDB(dr("sridsq"), 0), sptField,
                     FxDB(dr("sridso"), 0), sptField,
                     FxDB(dr("sridpl"), 0), sptField,
                     FxDB(dr("sriddo"), 0), sptField,
                     FxDB(dr("sriddr"), 0), sptField,
                     FxDB(dr("sridpi"), 0), sptField,
                     FxDB(dr("sridsi"), 0), sptField,
                     FxDB(dr("sridrnr"), 0), sptField,
                     FxDB(dr("srstatus"), 0), sptField,
                     FxDB(dr("srstatussebelumnya"), 0), sptField,
                     FxDB(dr("srjmlrevisi"), 0), sptField,
                     FxDB(dr("srcetakanke"), 0), sptField,
                     FxDB(dr("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srtutupperiode"), 0), sptField,
                     FxDB(dr("srisclose"), 0), sptField,
                     FxDB(dr("srcabangnama"), ""), sptField,
                     FxDB(dr("srlokasinama"), ""), sptField,
                     FxDB(dr("srgudangnama"), ""), sptField,
                     FxDB(dr("srcustomerkode"), ""), sptField,
                     FxDB(dr("srcustomernama"), ""), sptField,
                     FxDB(dr("srbagianpenjualankode"), ""), sptField,
                     FxDB(dr("srbagianpenjualannama"), ""), sptField,
                     FxDB(dr("srekspedisinama"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     FxDB(dr("srstatusnama"), ""), sptField,
                     FxDB(dr("srstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("srinputusernama"), ""), sptField,
                     FxDB(dr("srmodifikasiusernama"), ""), sptField,
                     FxDB(dr("srjenis"), 0), sptField,
                     FxDB(dr("srjenisnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srjenis, srjenisnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SrTerkait(ByVal param As String) As String
        'M5_SrTerkait --------------------------------------------------------
        'srid, srnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "srid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND srid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "srid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sr_terkait(Filter)
        sql = m5_sr_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_sr_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each sr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(sr("srid"), 0), sptField,
                     FxDB(sr("srnotransaksi"), ""), sptField,
                     FxDB(sr("sumber"), ""), sptField,
                     FxDB(sr("idterkait"), 0), sptField,
                     FxDB(sr("noterkait"), ""), sptField,
                     AsFormatTanggal(FxDB(sr("tglterkait"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(sr("inputtglterkait"), ""), formatTglWaktu), sptField,
                     AsFormatTanggal(FxDB(sr("modifikasitglterkait"), ""), formatTglWaktu), sptField,
                     FxDB(sr("jenisterkait"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related SR data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("srid, srnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingSI As String, ByVal ftOutstandingSI As String, ByVal ftExistOutstandingRNR As String, ByVal ftOutstandingRNR As String, ByVal ftExistStok As String, ByVal ftStok As String, ByVal ftHppI As String, ByVal ftHppF As String, ByVal ftSI As String, ByVal ftRNR As String, ByVal termasukPajak As String) As String
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
            sql = "SELECT si.sinotransaksi as notransaksi, (CASE si.sihargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_si_detail sid JOIN m5_si si ON sid.idsi = si.siid WHERE " & ftSI & " GROUP BY si.sihargatermasukpajak"
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

        'RNR
        If Len(ftExistOutstandingRNR) > 0 Then 'ftExistOutstanding = rowExists, idrnrdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingRNR)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "idrnrdetail=" & dtval.Rows(0)("idrnrdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in RNR" : GoTo selesai
            End If
        End If

        'CEK RNR YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftRNR) > 0 Then
            sql = "SELECT rnr.rnrnotransaksi as notransaksi, (CASE rnr.rnrhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid WHERE " & ftRNR & " GROUP BY rnr.rnrhargatermasukpajak"
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
                sql = "SELECT i.bkode, rnrd.idrnrdetail, rnr.rnrnotransaksi as notransaksi, (CASE rnr.rnrhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_rnr_detail rnrd JOIN m5_rnr rnr ON rnrd.idrnr = rnr.rnrid JOIN m1_item i ON rnrd.idbarang = i.bid WHERE (" & ftRNR & ") AND rnr.rnrhargatermasukpajak <> " & termasukPajak & " ORDER BY rnrd.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idrnrdetail = " & dtval.Rows(0)("idrnrdetail")
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
        If Len(ftOutstandingRNR) > 0 Then
            sql = "SELECT rnrd.idrnrdetail, (rnrd.jmlbarang - rnrd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_rnr_detail AS rnrd INNER JOIN m1_item AS i ON rnrd.idbarang = i.bid WHERE " & ftOutstandingRNR
            dtval = AsDataTableAmbilDariDB(sql)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")
                sisa = dtval.Rows(0)("sisarealisasi")

                filterLookup = "idrnrdetail=" & dtval.Rows(0)("idrnrdetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
                If dtLookup.Rows.Count > 0 Then
                    tipebarang = dtLookup.Rows(0)("tipebarang")
                    namabarang = dtLookup.Rows(0)("namabarang")
                    satuan = dtLookup.Rows(0)("satuan")
                    nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
                    urutan = dtLookup.Rows(0)("urutan")
                End If
                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in RNR, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
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
    Public Function m5_sr_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter_2 As String = "", filter_1 As String = "", filter0 As String = "", filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = ""
        Dim filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = "", filter10 As String = "", filter11 As String = ""

        'Replace Filter & srrt
        If (strFilter.Length > 0) Then

            filter_2 = strFilter

            filter_1 = strFilter

            filter0 = strFilter

            filter1 = strFilter

            filter2 = strFilter

            filter3 = strFilter

            filter4 = strFilter

            filter5 = strFilter

            filter6 = strFilter

            filter7 = strFilter

            filter8 = strFilter
            filter8 = filter8 & " AND ((`icd`.`sumber` = 'SR') and ((`ic`.`icstatus` = 2) or (`ic`.`icstatus` = 3) or (`ic`.`icstatus` = 4) or (`ic`.`icstatus` = 7)))"

            filter9 = strFilter
            filter9 = filter9 & " AND ((`pvd`.`sumber` = 'SR') and ((`pv`.`pvstatus` = 2) or (`pv`.`pvstatus` = 3) or (`pv`.`pvstatus` = 4) or (`pv`.`pvstatus` = 7)))"

            filter10 = strFilter

            filter11 = strFilter.Replace("srid", "atr.atidutama")

        Else
            'Default filter
            filter8 = "((`icd`.`sumber` = 'SR') and ((`ic`.`icstatus` = 2) or (`ic`.`icstatus` = 3) or (`ic`.`icstatus` = 4) or (`ic`.`icstatus` = 7)))"
            filter9 = "((`pvd`.`sumber` = 'SR') and ((`pv`.`pvstatus` = 2) or (`pv`.`pvstatus` = 3) or (`pv`.`pvstatus` = 4) or (`pv`.`pvstatus` = 7)))"

        End If

        If Len(filter_2) > 0 Then filter_2 = " WHERE " & filter_2
        If Len(filter_1) > 0 Then filter_1 = " WHERE " & filter_1
        If Len(filter0) > 0 Then filter0 = " WHERE " & filter0
        If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        If Len(filter9) > 0 Then filter9 = " WHERE " & filter9
        If Len(filter10) > 0 Then filter10 = " WHERE " & filter10
        If Len(filter11) > 0 Then filter11 = " WHERE " & filter11

        sql = "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `sq`.sqsumber AS sumber, `sq`.sqid AS idterkait, `sq`.sqnotransaksi AS noterkait, `sq`.sqtgl AS tglterkait, `sq`.sqinputtgl AS inputtglterkait, `sq`.sqmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_sq_detail sqd JOIN m5_sq `sq` ON sqd.idsq = sqid JOIN m5_sr_detail srd ON sqd.idsqdetail = srd.idsqdetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter_2 & " GROUP BY `sq`.sqid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `so`.sosumber AS sumber, `so`.soid AS idterkait, `so`.sonotransaksi AS noterkait, `so`.sotgl AS tglterkait, `so`.soinputtgl AS inputtglterkait, `so`.somodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_so_detail sod JOIN m5_so `so` ON sod.idso = soid JOIN m5_sr_detail srd ON sod.idsodetail = srd.idsodetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter_1 & " GROUP BY `so`.soid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `pi`.pisumber AS sumber, `pi`.piid AS idterkait, `pi`.pinotransaksi AS noterkait, `pi`.pitgl AS tglterkait, `pi`.piinputtgl AS inputtglterkait, `pi`.pimodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_pi_detail pid JOIN m5_pi `pi` ON pid.idpi = piid JOIN m5_sr_detail srd ON pid.idpidetail = srd.idpidetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter0 & " GROUP BY `pi`.piid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `pl`.plsumber AS sumber, `pl`.plid AS idterkait, `pl`.plnotransaksi AS noterkait, `pl`.pltgl AS tglterkait, `pl`.plinputtgl AS inputtglterkait, `pl`.plmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_pl_detail pld JOIN m5_pl `pl` ON pld.idpl = plid JOIN m5_sr_detail srd ON pld.idpldetail = srd.idpldetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter1 & " GROUP BY `pl`.plid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `do`.dosumber AS sumber, `do`.doid AS idterkait, `do`.donotransaksi AS noterkait, `do`.dotgl AS tglterkait, `do`.doinputtgl AS inputtglterkait, `do`.domodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_do_detail dod JOIN m5_do `do` ON dod.iddo = doid JOIN m5_sr_detail srd ON dod.iddodetail = srd.iddodetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter2 & " GROUP BY `do`.doid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `dr`.drsumber AS sumber, `dr`.drid AS idterkait, `dr`.drnotransaksi AS noterkait, `dr`.drtgl AS tglterkait, `dr`.drinputtgl AS inputtglterkait, `dr`.drmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_dr_detail drd JOIN m5_dr `dr` ON drd.iddr = drid JOIN m5_sr_detail srd ON drd.iddrdetail = srd.iddrdetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter3 & " GROUP BY `dr`.drid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `si`.sisumber AS sumber, `si`.siid AS idterkait, `si`.sinotransaksi AS noterkait, `si`.sitgl AS tglterkait, `si`.siinputtgl AS inputtglterkait, `si`.simodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_si_detail sid JOIN m5_si `si` ON sid.idsi = siid JOIN m5_sr_detail srd ON sid.idsidetail = srd.idsidetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter4 & " GROUP BY `si`.siid, sr.srid"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid AS srid, sr.srnotransaksi AS srnotransaksi, `rnr`.rnrsumber AS sumber, `rnr`.rnrid AS idterkait, `rnr`.rnrnotransaksi AS noterkait, `rnr`.rnrtgl AS tglterkait, `rnr`.rnrinputtgl AS inputtglterkait, `rnr`.rnrmodifikasitgl AS modifikasitglterkait, 0 as jenisterkait FROM m5_rnr_detail rnrd JOIN m5_rnr `rnr` ON rnrd.idrnr = rnrid JOIN m5_sr_detail srd ON rnrd.idrnrdetail = srd.idrnrdetail JOIN m5_sr sr ON srd.idsr = sr.srid " & filter5 & " GROUP BY `rnr`.rnrid, sr.srid"
        sql &= " UNION ALL "
        sql &= "select `sr`.`srid` AS `srid`,`sr`.`srnotransaksi` AS `srnotransaksi`,`cso`.`sumber` AS `sumber`,`it`.`idutama` AS `idterkait`,`it`.`notransaksi` AS `noterkait`,`it`.`tgl` AS `tglterkait`,`it`.`inputtgl` AS `inputtglterkait`,`it`.`inputtgl` AS `modifikasitglterkait`, 1 as jenisterkait from ((((`m1_cogs_special_in` `csi` join `m1_cogs_special_out` `cso` on((`csi`.`idhppikm` = `cso`.`idhppikm`))) join `m5_sr_detail` `srd` on(((`csi`.`sumber` = 'sr') and (`csi`.`idbarang` = `srd`.`idbarang`) and (`csi`.`idtransaksi` = `srd`.`idsrdetail`)))) join `m5_sr` `sr` on((`srd`.`idsr` = `sr`.`srid`))) join `m1_item_transaction` `it` on(((`cso`.`sumber` = `it`.`sumber`) and (`cso`.`idbarang` = `it`.`idbarang`) and (`cso`.`idtransaksi` = `it`.`iddetail`)))) " & filter6 & " group by `it`.`sumber`,`it`.`idutama`,`sr`.`srid` "
        sql &= " UNION ALL "
        sql &= "select `sr`.`srid` AS `srid`,`sr`.`srnotransaksi` AS `srnotransaksi`,`cfo`.`cfosumber` AS `sumber`,`it`.`idutama` AS `idterkait`,`it`.`notransaksi` AS `noterkait`,`it`.`tgl` AS `tglterkait`,`it`.`inputtgl` AS `inputtglterkait`,`it`.`inputtgl` AS `modifikasitglterkait`, 1 as jenisterkait from ((((`m1_cogs_fifo_in` `cfi` join `m1_cogs_fifo_out` `cfo` on((`cfi`.`cfiid` = `cfo`.`cfoidcfi`))) join `m5_sr_detail` `srd` on(((`cfi`.`cfisumber` = 'sr') and (`cfi`.`cfiidbarang` = `srd`.`idbarang`) and (`cfi`.`cfiidtransaksi` = `srd`.`idsrdetail`)))) join `m5_sr` `sr` on((`srd`.`idsr` = `sr`.`srid`))) join `m1_item_transaction` `it` on(((`cfo`.`cfosumber` = `it`.`sumber`) and (`cfo`.`cfoidbarang` = `it`.`idbarang`) and (`cfo`.`cfoidtransaksi` = `it`.`iddetail`)))) " & filter7 & " group by `it`.`sumber`,`it`.`idutama`,`sr`.`srid` "
        sql &= " UNION ALL "
        sql &= "select `sr`.`srid` AS `srid`,`sr`.`srnotransaksi` AS `srnotransaksi`,`ic`.`icsumber` AS `sumber`,`ic`.`icid` AS `idterkait`,`ic`.`icnotransaksi` AS `noterkait`,`ic`.`ictgl` AS `tglterkait`,`ic`.`icinputtgl` AS `inputtglterkait`,`ic`.`icmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from ((`m5_ic_detail` `icd` join `m5_ic` `ic` on((`icd`.`idic` = `ic`.`icid`))) join `m5_sr` `sr` on((`icd`.`idtransaksi` = `sr`.`srid`))) " & filter8 & "  group by `sr`.`srid`, `ic`.`icid` "
        sql &= " UNION ALL "
        sql &= "select `sr`.`srid` AS `srid`,`sr`.`srnotransaksi` AS `srnotransaksi`,`pv`.`pvsumber` AS `sumber`,`pv`.`pvid` AS `idterkait`,`pv`.`pvnotransaksi` AS `noterkait`,`pv`.`pvtgl` AS `tglterkait`,`pv`.`pvinputtgl` AS `inputtglterkait`,`pv`.`pvmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from ((`m5_pv_detail` `pvd` join `m5_pv` `pv` on((`pvd`.`idpv` = `pv`.`pvid`))) join `m5_sr` `sr` on((`pvd`.`idtransaksi` = `sr`.`srid`))) " & filter9 & "  group by `sr`.`srid`, `pv`.`pvid`"
        sql &= " UNION ALL "
        sql &= "SELECT sr.srid as srid, sr.srnotransaksi as srnotransaksi, da.dasumber as sumber, da.daid as idterkait, da.danotransaksi as noterkait, da.datgl as tglterkait, da.dainputtgl as inputtglterkait, da.damodifikasitgl as modifikasitglterkait, 1 as jenisterkait FROM m7_asset_transaction atr JOIN m5_sr sr ON atr.atsumber = sr.srsumber AND atr.atidutama = sr.srid JOIN m7_asset a ON atr.atkode = a.akode JOIN m7_da_detail dad ON a.aid = dad.idaset JOIN m7_da da ON dad.idda = da.daid AND da.dastatus IN(2,3,4,7) AND da.datgl > sr.srtgl " & filter10 & " GROUP BY da.daid, sr.srid "
        sql &= " UNION ALL "
        sql &= "SELECT atr.atidutama AS srid, atr.atnotransaksi AS srnotransaksi, atr2.atsumber AS sumber, atr2.atidutama AS idterkait, atr2.atnotransaksi AS noterkait, atr2.attgl AS tglterkait, CONCAT(atr2.attgl,'00:00:00') AS inputtglterkait, CONCAT(atr2.attgl,'00:00:00') AS modifikasitglterkait, 1 AS jenisterkait FROM m7_asset_transaction atr JOIN m7_asset_transaction atr2 ON atr.atkode = atr2.atkode AND atr2.atstatus IN(2,3,4,7) AND atr.atsumber = 'SR' AND atr2.attgl > atr.attgl AND NOT (atr.atsumber = atr2.atsumber AND atr.atidutama = atr2.atidutama) " & filter11 & "GROUP BY atr2.atsumber, atr2.atidutama"

        Return sql
    End Function

    <WebMethod()>
    Public Function M5_SrSimpanOld(ByVal param As String) As String
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
        'srid(0) As Integer, srcabang(1) As String, srlokasi(2) As String, srgudang(3) As String, srasalbarang(4) As String, 
        'srasalbarangkategori(5) As Integer, srjenispenjulan(6) As String, srjenispenjualankategori(7) As Integer, srcarabayar(8) As Integer, srsumber(9) As String, 
        'srautonotransaksi(10) As Integer, srnotransaksi(11) As String, srtgl(12) As Date, srkodepa(13) As Integer, srcustomer(14) As Integer, 
        'srcustomerkontak(15) As String, sr1alamat1(16) As String, sr1alamat2(17) As String, sr1alamat3(18) As String, sr2alamat1(19) As String, 
        'sr2alamat2(20) As String, sr2alamat3(21) As String, srbagianpenjualan(22) As Integer, srekspedisi(23) As String, srtglkirim(24) As Date, 
        'srtermin(25) As String, srtgljatuhtempo(26) As Date, sruraian(27) As String, srcatatan(28) As String, srnoref(29) As String, 
        'srtglnoref(30) As Date, srtglpenutupan(31) As Date, srmatauang(32) As String, srkurs(33) As Double, srhargatermasukpajak(34) As Integer, 
        'srtotal(35) As Double, srdiskonpersen(36) As String, srjmldiskon(37) As Double, srtotalpajak1detail(38) As Double, srtotalpajak2detail(39) As Double, 
        'srbiayalainpersen(40) As Double, srbiayalain(41) As Double, srtotaltransaksi(42) As Double, srsisatransaksi(43) As Double, srjmlbayar(44) As Double, 
        'srstatuslunas(45) As Integer, srtgllunas(46) As Date, srnofakturpajak(47) As String, srsdhbayarpajak(48) As Integer, srtglbayarpajak(49) As Date, 
        'srrekdiskon(50) As String, srrekpajak1(51) As String, srrekpajak2(52) As String, srrekbiayalain(53) As String, srreksisa(54) As String, 
        'srrekbayar(55) As String, sridsq(56) As Integer, sridso(57) As Integer, sridpl(58) As Integer, sriddo(59) As Integer, 
        'sriddr(60) As Integer, sridpi(61) As Integer, sridsi(62) As Integer, sridrnr(63) As Integer, srstatus(64) As Integer, 
        'srstatussebelumnya(65) As Integer, srjmlrevisi(66) As Integer, srcetakanke(67) As Integer, srinputuser(68) As Integer, srinputtgl(69) As DateTime, 
        'srmodifikasiuser(70) As Integer, srmodifikasitgl(71) As DateTime, srposting(72) As Integer, srtutupperiode(73) As Integer, srisclose(74) As Integer, 
        'srcustomtext1(75) As String, srcustomtext2(76) As String, srcustomtext3(77) As String, srcustomtext4(78) As String, srcustomtext5(79) As String, 
        'srcustomint1(80) As Integer, srcustomint2(81) As Integer, srcustomint3(82) As Integer, srcustomdbl1(83) As Double, srcustomdbl2(84) As Double, 
        'srcustomdbl3(85) As Double, srcustomdate1(86) As Date, srcustomdate2(87) As Date, srcustomdate3(88) As Date, srjenis(89) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, 
        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, 
        'srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 90) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'srid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "srid required numeric." : GoTo selesai
        End If
        'srasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "srasalbarangkategori required numeric." : GoTo selesai
        End If
        'srjenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "srjenispenjualankategori required numeric." : GoTo selesai
        End If
        'srcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "srcarabayar required numeric." : GoTo selesai
        End If
        'srautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "srautonotransaksi required numeric." : GoTo selesai
        End If
        'srtgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "srtgl required date." : GoTo selesai
        End If
        'srkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "srkodepa required numeric." : GoTo selesai
        End If
        'srcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "srcustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "srcustomer can't be empty." : GoTo selesai
        End If
        'srbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "srbagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "srbagianpenjualan can't be empty." : GoTo selesai
        End If
        'srtglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "srtglkirim required date." : GoTo selesai
        End If
        'srtgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "srtgljatuhtempo required date." : GoTo selesai
        End If
        'srtglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "srtglnoref required date." : GoTo selesai
        End If
        'srtglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "srtglpenutupan required date." : GoTo selesai
        End If
        'srkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "srkurs required numeric." : GoTo selesai
        End If
        'srhargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "srhargatermasukpajak required numeric." : GoTo selesai
        End If
        'srtotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "srtotal required numeric." : GoTo selesai
        End If
        'srjmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "srjmldiskon required numeric." : GoTo selesai
        End If
        'srtotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "srtotalpajak1detail required numeric." : GoTo selesai
        End If
        'srtotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "srtotalpajak2detail required numeric." : GoTo selesai
        End If
        ''srbiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "srbiayalainpersen required numeric." : GoTo selesai
        'End If
        'srbiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "srbiayalain required numeric." : GoTo selesai
        End If
        'srtotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "srtotaltransaksi required numeric." : GoTo selesai
        End If
        'srsisatransaksi(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "srsisatransaksi required numeric." : GoTo selesai
        End If
        'srjmlbayar(44) As Double
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "srjmlbayar required numeric." : GoTo selesai
        End If
        'srstatuslunas(45) As Integer
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "srstatuslunas required numeric." : GoTo selesai
        End If
        'srtgllunas(46) As Date
        If (IsDate(dataUtama(46)) = False) Then
            result(2) = "srtgllunas required date." : GoTo selesai
        End If
        'srsdhbayarpajak(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "srsdhbayarpajak required numeric." : GoTo selesai
        End If
        'srtglbayarpajak(49) As Date
        If (IsDate(dataUtama(49)) = False) Then
            result(2) = "srtglbayarpajak required date." : GoTo selesai
        End If
        'sridsq(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "sridsq required numeric." : GoTo selesai
        End If
        'sridso(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sridso required numeric." : GoTo selesai
        End If
        'sridpl(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "sridpl required numeric." : GoTo selesai
        End If
        'sriddo(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sriddo required numeric." : GoTo selesai
        End If
        'sriddr(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "sriddr required numeric." : GoTo selesai
        End If
        'sridpi(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "sridpi required numeric." : GoTo selesai
        End If
        'sridsi(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "sridsi required numeric." : GoTo selesai
        End If
        'sridrnr(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "sridrnr required numeric." : GoTo selesai
        End If
        'srstatus(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "srstatus required numeric." : GoTo selesai
        End If
        'srstatussebelumnya(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "srstatussebelumnya required numeric." : GoTo selesai
        End If
        'srjmlrevisi(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "srjmlrevisi required numeric." : GoTo selesai
        End If
        'srcetakanke(67) As Integer
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "srcetakanke required numeric." : GoTo selesai
        End If
        'srinputuser(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "srinputuser required numeric." : GoTo selesai
        End If
        'srinputtgl(69) As DateTime
        If (IsDate(dataUtama(69)) = False) Then
            result(2) = "srinputtgl required date." : GoTo selesai
        End If
        'srmodifikasiuser(70) As Integer
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "srmodifikasiuser required numeric." : GoTo selesai
        End If
        'srmodifikasitgl(71) As DateTime
        If (IsDate(dataUtama(71)) = False) Then
            result(2) = "srmodifikasitgl required date." : GoTo selesai
        End If
        'srposting(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "srposting required numeric." : GoTo selesai
        End If
        'srtutupperiode(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "srtutupperiode required numeric." : GoTo selesai
        End If
        'srisclose(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "srisclose required numeric." : GoTo selesai
        End If
        'srcustomint1(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "srcustomint1 required numeric." : GoTo selesai
        End If
        'srcustomint2(81) As Integer
        If (IsNumeric(dataUtama(81)) = False) Then
            result(2) = "srcustomint2 required numeric." : GoTo selesai
        End If
        'srcustomint3(82) As Integer
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "srcustomint3 required numeric." : GoTo selesai
        End If
        'srcustomdbl1(83) As Double
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "srcustomdbl1 required numeric." : GoTo selesai
        End If
        'srcustomdbl2(84) As Double
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "srcustomdbl2 required numeric." : GoTo selesai
        End If
        'srcustomdbl3(85) As Double
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "srcustomdbl3 required numeric." : GoTo selesai
        End If
        'srcustomdate1(86) As Date
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "srcustomdate1 required date." : GoTo selesai
        End If
        'srcustomdate2(87) As Date
        If (IsDate(dataUtama(87)) = False) Then
            result(2) = "srcustomdate2 required date." : GoTo selesai
        End If
        'srcustomdate3(88) As Date
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "srcustomdate3 required date." : GoTo selesai
        End If

        'srjenis(89) As Integer
        If (IsNumeric(dataUtama(89)) = False) Then
            result(2) = "srjenis required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'srcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "srcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "srcabang should not be more than 25 character." : GoTo selesai
        End If

        'srlokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "srlokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "srlokasi should not be more than 25 character." : GoTo selesai
        End If

        'srgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "srgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "srgudang should not be more than 25 character." : GoTo selesai
        End If

        'srsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "srsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "srsumber should not be more than 10 character." : GoTo selesai
        End If

        'srnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "srnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "srnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'srtgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "srtgl can't be empty" : GoTo selesai
        End If

        'srtglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "srtglkirim can't be empty" : GoTo selesai
        End If

        'srtgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "srtgljatuhtempo can't be empty" : GoTo selesai
        End If

        'srtglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "srtglnoref can't be empty" : GoTo selesai
        End If

        'srtglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "srtglpenutupan can't be empty" : GoTo selesai
        End If

        'srmatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "srmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "srmatauang should not be more than 25 character." : GoTo selesai
        End If

        'srkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "srkurs can't be empty" : GoTo selesai
        End If

        'srtotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "srtotal can't be empty" : GoTo selesai
        End If

        'srdiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "srdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "srdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'srjmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "srjmldiskon can't be empty" : GoTo selesai
        End If

        'srtotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "srtotalpajak1detail can't be empty" : GoTo selesai
        End If

        'srtotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "srtotalpajak2detail can't be empty" : GoTo selesai
        End If

        'srbiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "srbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "srbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'srbiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "srbiayalain can't be empty" : GoTo selesai
        End If

        'srtotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "srtotaltransaksi can't be empty" : GoTo selesai
        End If

        'srsisatransaksi(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "srsisatransaksi can't be empty" : GoTo selesai
        End If

        'srjmlbayar(44) As Double
        If Len(dataUtama(44)) = 0 Then
            result(2) = "srjmlbayar can't be empty" : GoTo selesai
        End If

        'srtgllunas(46) As Date
        If Len(dataUtama(46)) = 0 Then
            result(2) = "srtgllunas can't be empty" : GoTo selesai
        End If

        'srtglbayarpajak(49) As Date
        If Len(dataUtama(49)) = 0 Then
            result(2) = "srtglbayarpajak can't be empty" : GoTo selesai
        End If

        'srinputtgl(69) As DateTime
        If Len(dataUtama(69)) = 0 Then
            result(2) = "srinputtgl can't be empty" : GoTo selesai
        End If

        'srmodifikasitgl(71) As DateTime
        If Len(dataUtama(71)) = 0 Then
            result(2) = "srmodifikasitgl can't be empty" : GoTo selesai
        End If

        'srcustomdbl1(83) As Double
        If Len(dataUtama(83)) = 0 Then
            result(2) = "srcustomdbl1 can't be empty" : GoTo selesai
        End If

        'srcustomdbl2(84) As Double
        If Len(dataUtama(84)) = 0 Then
            result(2) = "srcustomdbl2 can't be empty" : GoTo selesai
        End If

        'srcustomdbl3(85) As Double
        If Len(dataUtama(85)) = 0 Then
            result(2) = "srcustomdbl3 can't be empty" : GoTo selesai
        End If

        'srcustomdate1(86) As Date
        If Len(dataUtama(86)) = 0 Then
            result(2) = "srcustomdate1 can't be empty" : GoTo selesai
        End If

        'srcustomdate2(87) As Date
        If Len(dataUtama(87)) = 0 Then
            result(2) = "srcustomdate2 can't be empty" : GoTo selesai
        End If

        'srcustomdate3(88) As Date
        If Len(dataUtama(88)) = 0 Then
            result(2) = "srcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "srid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjenispenjulan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjenis", AsEnumTypeData.AsInt64)
        If AsDataTableTambahData(dtutama, "srid~srcabang~srlokasi~srgudang~srasalbarang~srasalbarangkategori~srjenispenjulan~srjenispenjualankategori~srcarabayar~srsumber~srautonotransaksi~srnotransaksi~srtgl~srkodepa~srcustomer~srcustomerkontak~sr1alamat1~sr1alamat2~sr1alamat3~sr2alamat1~sr2alamat2~sr2alamat3~srbagianpenjualan~srekspedisi~srtglkirim~srtermin~srtgljatuhtempo~sruraian~srcatatan~srnoref~srtglnoref~srtglpenutupan~srmatauang~srkurs~srhargatermasukpajak~srtotal~srdiskonpersen~srjmldiskon~srtotalpajak1detail~srtotalpajak2detail~srbiayalainpersen~srbiayalain~srtotaltransaksi~srsisatransaksi~srjmlbayar~srstatuslunas~srtgllunas~srnofakturpajak~srsdhbayarpajak~srtglbayarpajak~srrekdiskon~srrekpajak1~srrekpajak2~srrekbiayalain~srreksisa~srrekbayar~sridsq~sridso~sridpl~sriddo~sriddr~sridpi~sridsi~sridrnr~srstatus~srstatussebelumnya~srjmlrevisi~srcetakanke~srinputuser~srinputtgl~srmodifikasiuser~srmodifikasitgl~srposting~srtutupperiode~srisclose~srcustomtext1~srcustomtext2~srcustomtext3~srcustomtext4~srcustomtext5~srcustomint1~srcustomint2~srcustomint3~srcustomdbl1~srcustomdbl2~srcustomdbl3~srcustomdate1~srcustomdate2~srcustomdate3~srjenis", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsrdetail(0) As Integer, idsr(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, idhppkhususkeluar(12) As Integer, idhppfifokeluar(13) As Integer, harga(14) As Double, 
        'hargapricelist(15) As Double, hpp(16) As Double, diskon(17) As String, jmldiskon(18) As Double, pajak1(19) As String, 
        'jmlpajak1(20) As Double, pajak2(21) As String, jmlpajak2(22) As Double, cabang(23) As String, lokasi(24) As String, 
        'gudangasal(25) As String, gudangtransit(26) As String, gudangtujuan(27) As String, rekpersediaan(28) As String, rekhargapokok(29) As String, 
        'rekdiskonpenjualan(30) As String, rekreturpenjualan(31) As String, costcenter(32) As String, divisi(33) As String, subdivisi(34) As String, 
        'proyek(35) As String, catatan(36) As String, urutan(37) As Integer, idsqdetail(38) As Integer, idsodetail(39) As Integer, 
        'idpldetail(40) As Integer, iddodetail(41) As Integer, iddrdetail(42) As Integer, idpidetail(43) As Integer, idsidetail(44) As Integer, 
        'idrnrdetail(45) As Integer, isclose(46) As Integer, customtext1(47) As String, customtext2(48) As String, customtext3(49) As String, 
        'customdbl1(50) As Double, customdbl2(51) As Double, customdbl3(52) As Double, customdate1(53) As Date, customdate2(54) As Date, 
        'customdate3(55) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, 
        'harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, 
        'pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, 
        'rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, 
        'iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, 
        'customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsrdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idrnrdetail", AsEnumTypeData.AsInt64)
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

        Dim ftExistOutstandingSI As String = "", ftOutstandingSI As String = "", updNilaiSI As String = "", updFilterSI As String = ""
        Dim ftExistOutstandingRNR As String = "", ftOutstandingRNR As String = "", updNilaiRNR As String = "", updFilterRNR As String = ""
        Dim idbarang As Integer = 0, idsidetail As Integer = 0, idrnrdetail As Integer = 0, jmlbarang As Double = 0
        Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
        Dim updStokIn As String = "", gudangIn As String = ""
        Dim updStokBarang As String = "", ftStokBarang As String = ""

        'FILTER SI DAN RNR, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSI As String = "", ftRNR As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 56) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsrdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsrdetail required numeric." : GoTo selesai
            End If
            'idsr(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idsr required numeric." : GoTo selesai
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
            'idrnrdetail(45) As Integer
            If (IsNumeric(dataRowDetail(45)) = False) Then
                result(2) = "Row : " & i & " - idrnrdetail required numeric." : GoTo selesai
            End If
            'isclose(46) As Integer
            If (IsNumeric(dataRowDetail(46)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(50) As Double
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(51) As Double
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(52) As Double
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(53) As Date
            If (IsDate(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(54) As Date
            If (IsDate(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(55) As Date
            If (IsDate(dataRowDetail(55)) = False) Then
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
            'If dataRowDetail(14) <= 0 Then
            '    result(2) = "Row : " & i & " - harga can't be less than or equal to zero" : GoTo selesai
            'End If

            'hargapricelist(15) As Double
            If Len(dataRowDetail(15)) = 0 Then
                result(2) = "Row : " & i & " - hargapricelist can't be empty" : GoTo selesai
            End If

            'hpp(16) As Double
            If (Double.Parse(dataRowDetail(16)) <= 0) Then
                result(2) = "Row : " & i & " - hpp can't be less than or equal to zero" : GoTo selesai
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
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(14) As Double, diskon(17) As String
                dataRowDetail(18) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(14)), FixQuotes(dataRowDetail(17).ToString))
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

            'customdbl1(50) As Double
            If Len(dataRowDetail(50)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(51) As Double
            If Len(dataRowDetail(51)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(52) As Double
            If Len(dataRowDetail(52)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(53) As Date
            If Len(dataRowDetail(53)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(54) As Date
            If Len(dataRowDetail(54)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(55) As Date
            If Len(dataRowDetail(55)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsrdetail~idsr~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~idhppkhususkeluar~idhppfifokeluar~harga~hargapricelist~hpp~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudangasal~gudangtransit~gudangtujuan~rekpersediaan~rekhargapokok~rekdiskonpenjualan~rekreturpenjualan~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~idsodetail~idpldetail~iddodetail~iddrdetail~idpidetail~idsidetail~idrnrdetail~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudangtujuan(27) As String   , gudangtransit(26) As String
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudangIn = dataRowDetail(27) : gudangOut = dataRowDetail(26)
            'idsidetail(44) As Integer     , idrnrdetail(45) As Integer
            idsidetail = dataRowDetail(44) : idrnrdetail = dataRowDetail(45)

            'ValidasiHppI
            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

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

            If idrnrdetail <> 0 Then 'RNR
                'CEK RNR YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftRNR = IIf(Len(ftRNR.ToString) = 0, "", ftRNR & " OR ")
                ftRNR = String.Concat(ftRNR, " (rnrd.idrnrdetail = " & idrnrdetail & ") ")

                '1. CEK DATA EXIST ------------------------
                ftExistOutstandingRNR = IIf(Len(ftExistOutstandingRNR.ToString) = 0, "", ftExistOutstandingRNR & " UNION ")
                ftExistOutstandingRNR = String.Concat(ftExistOutstandingRNR, "SELECT EXISTS(SELECT 1 FROM m5_rnr_detail JOIN m5_rnr ON idrnr = rnrid WHERE idrnrdetail = '" & idrnrdetail & "' AND (rnrstatus = 2 OR rnrstatus = 3 OR rnrstatus = 4 OR rnrstatus = 7) LIMIT 1) as rowExists, '" & idrnrdetail & "' as idrnrdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING -------------------
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrnrdetail=" & idrnrdetail)
                ftOutstandingRNR = IIf(Len(ftOutstandingRNR.ToString) = 0, "", ftOutstandingRNR & " OR ")

                ftOutstandingRNR = String.Concat(ftOutstandingRNR, " (rnrd.idrnrdetail = " & idrnrdetail & " AND " & Outstanding & " > (rnrd.jmlbarang - rnrd.jmlrealisasi)) ")
                '3. SET NILAI UPDATE OUTSTANDING ----------
                updNilaiRNR = String.Concat("WHEN '" & idrnrdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilaiRNR)

                '4. SET FILTER UPDATE OUTSTANDING ---------
                updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                updFilterRNR = String.Concat(updFilterRNR, "(idrnrdetail = '" & idrnrdetail & "')")
            End If

            'VALIDASI STOK -------------------------------
            '1. CEK DATA EXIST STOK KELUAR
            ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
            ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists, '" & idbarang & "' as idbarang, bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

            '2. CEK JML STOK KELUAR
            Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtransit='" & gudangOut & "'")
            ftStok = IIf(Len(ftStok.ToString) = 0, "", ftStok & " OR ")
            ftStok = String.Concat(ftStok, " (isw.idbarang = " & idbarang & " AND isw.kgudang='" & gudangOut & "' AND " & Stok & " > isw.stok) ")

            '3. SET NILAI UPDATE STOK KELUAR
            updStokOut = IIf(Len(updStokOut.ToString) = 0, "", updStokOut & ", ")
            updStokOut = String.Concat(updStokOut, "('" & idbarang & "', '" & gudangOut & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

            '4. SET NILAI UPDATE STOK MASUK
            updStokIn = IIf(Len(updStokIn.ToString) = 0, "", updStokIn & ", ")
            updStokIn = String.Concat(updStokIn, "('" & idbarang & "', '" & gudangIn & "', '" & jmlbarang & "')") ' idbarang, kgudang, stok

            '5. SET NILAI UPDATE STOK M1_ITEM ------------
            Dim stokMasuk As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
            ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
            ftStokBarang = String.Concat(ftStokBarang, " (bid = '" & idbarang & "') ")
            updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok + '" & stokMasuk & "', 5) ", updStokBarang)
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
                Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("srtgl")), AsFormatTanggal(drutama("srtgl")))
                arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                'END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("srstatus") = 2 Then

                    'VALIDASI BATCH SERIAL ---------------
                    'ValidasiBatchSerial
                    Dim rsValidasi As String = ValidasiBatchSerial(dtdetail, dtbatch, dtserial, ftBarang, "jmlbarang", 1)
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : GoTo selesai
                    'END OF VALIDASI BATCH SERIAL --------

                    'ValidasiSimpan
                    rsValidasi = ValidasiSimpan(dtdetail, ftExistOutstandingSI, ftOutstandingSI, ftExistOutstandingRNR, ftOutstandingRNR, "", "", "", "", ftSI, ftRNR, drutama("srhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("srtermin").ToString, AsFormatTanggal(drutama("srtgl")), "srtgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("srtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                'END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("srtotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("srtotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("srtotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("srhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("srtotaltransaksi") = Double.Parse(drutama("srtotal")) - Double.Parse(drutama("srjmldiskon")) + Double.Parse(drutama("srtotalpajak1detail")) + Double.Parse(drutama("srtotalpajak2detail")) + Double.Parse(drutama("srbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("srtotaltransaksi") = Double.Parse(drutama("srtotal")) - Double.Parse(drutama("srjmldiskon")) + Double.Parse(drutama("srbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                'JIKA RETUR LANGSUNG MAKA SET JMLBAYAR, STATUSLUNAS DAN TGLLUNAS
                If Integer.Parse(drutama("srjenis")) = 1 Then
                    drutama("srjmlbayar") = drutama("srtotaltransaksi")
                    drutama("srtgllunas") = drutama("srtgl")
                    drutama("srstatuslunas") = 2

                Else
                    drutama("srjmlbayar") = 0 : drutama("srtgllunas") = "1900-01-01" : drutama("srstatuslunas") = 0

                End If


                If isUpdate Then
                    result(4) = drutama("srid")
                    notransaksi = drutama("srnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(srid), srnotransaksi FROM M5_sr WHERE srid='" & result(4) & "' AND srstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(srid) FROM M5_sr WHERE srnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_sr_history
                        Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("srsumber")) & "▼" & FixQuotes(drutama("srid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Sr set srcabang  = '" & FixQuotes(drutama("srcabang")) & "', srlokasi  = '" & FixQuotes(drutama("srlokasi")) & "', srgudang  = '" & FixQuotes(drutama("srgudang")) & "', srasalbarang  = '" & FixQuotes(drutama("srasalbarang")) & "', srasalbarangkategori  = " & drutama("srasalbarangkategori") & ", srjenispenjulan  = '" & FixQuotes(drutama("srjenispenjulan")) & "', srjenispenjualankategori  = " & drutama("srjenispenjualankategori") & ", srcarabayar  = " & drutama("srcarabayar") & ", srsumber  = '" & FixQuotes(drutama("srsumber")) & "', srautonotransaksi  = " & drutama("srautonotransaksi") & ", srnotransaksi  = '" & FixQuotes(notransaksi) & "', srtgl  = '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', srkodepa  = " & drutama("srkodepa") & ", srcustomer  = " & drutama("srcustomer") & ", srcustomerkontak  = '" & FixQuotes(drutama("srcustomerkontak")) & "', sr1alamat1  = '" & FixQuotes(drutama("sr1alamat1")) & "', sr1alamat2  = '" & FixQuotes(drutama("sr1alamat2")) & "', sr1alamat3  = '" & FixQuotes(drutama("sr1alamat3")) & "', sr2alamat1  = '" & FixQuotes(drutama("sr2alamat1")) & "', sr2alamat2  = '" & FixQuotes(drutama("sr2alamat2")) & "', sr2alamat3  = '" & FixQuotes(drutama("sr2alamat3")) & "', srbagianpenjualan  = " & drutama("srbagianpenjualan") & ", srekspedisi  = '" & FixQuotes(drutama("srekspedisi")) & "', srtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', srtermin  = '" & FixQuotes(drutama("srtermin")) & "', srtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', sruraian  = '" & FixQuotes(drutama("sruraian")) & "', srcatatan  = '" & FixQuotes(drutama("srcatatan")) & "', srnoref  = '" & FixQuotes(drutama("srnoref")) & "', srtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', srtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', srmatauang  = '" & FixQuotes(drutama("srmatauang")) & "', srkurs  = '" & FixDouble(drutama("srkurs")) & "', srhargatermasukpajak  = " & drutama("srhargatermasukpajak") & ", srtotal  = '" & FixDouble(drutama("srtotal")) & "', srdiskonpersen  = '" & FixQuotes(drutama("srdiskonpersen")) & "', srjmldiskon  = '" & FixDouble(drutama("srjmldiskon")) & "', srtotalpajak1detail  = '" & FixDouble(drutama("srtotalpajak1detail")) & "', srtotalpajak2detail  = '" & FixDouble(drutama("srtotalpajak2detail")) & "', srbiayalainpersen  = '" & FixDouble(drutama("srbiayalainpersen")) & "', srbiayalain  = '" & FixDouble(drutama("srbiayalain")) & "', srtotaltransaksi  = '" & FixDouble(drutama("srtotaltransaksi")) & "', srsisatransaksi  = '" & FixDouble(drutama("srsisatransaksi")) & "', srjmlbayar  = '" & FixDouble(drutama("srjmlbayar")) & "', srstatuslunas  = " & drutama("srstatuslunas") & ", srtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', srnofakturpajak  = '" & FixQuotes(drutama("srnofakturpajak")) & "', srsdhbayarpajak  = " & drutama("srsdhbayarpajak") & ", srtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', srrekdiskon  = '" & FixQuotes(drutama("srrekdiskon")) & "', srrekpajak1  = '" & FixQuotes(drutama("srrekpajak1")) & "', srrekpajak2  = '" & FixQuotes(drutama("srrekpajak2")) & "', srrekbiayalain  = '" & FixQuotes(drutama("srrekbiayalain")) & "', srreksisa  = '" & FixQuotes(drutama("srreksisa")) & "', srrekbayar  = '" & FixQuotes(drutama("srrekbayar")) & "', sridsq  = " & drutama("sridsq") & ", sridso  = " & drutama("sridso") & ", sridpl  = " & drutama("sridpl") & ", sriddo  = " & drutama("sriddo") & ", sriddr  = " & drutama("sriddr") & ", sridpi  = " & drutama("sridpi") & ", sridsi  = " & drutama("sridsi") & ", sridrnr  = " & drutama("sridrnr") & ", srstatus  = " & drutama("srstatus") & ", srstatussebelumnya  = " & drutama("srstatussebelumnya") & ", srjmlrevisi  = srjmlrevisi+1, srcetakanke  = " & drutama("srcetakanke") & ", srmodifikasiuser  = " & drutama("srmodifikasiuser") & ", srmodifikasitgl  = NOW(), srposting  = 0, srtutupperiode  = " & drutama("srtutupperiode") & ", srcustomtext1  = '" & FixQuotes(drutama("srcustomtext1")) & "', srcustomtext2  = '" & FixQuotes(drutama("srcustomtext2")) & "', srcustomtext3  = '" & FixQuotes(drutama("srcustomtext3")) & "', srcustomtext4  = '" & FixQuotes(drutama("srcustomtext4")) & "', srcustomtext5  = '" & FixQuotes(drutama("srcustomtext5")) & "', srcustomint1  = " & drutama("srcustomint1") & ", srcustomint2  = " & drutama("srcustomint2") & ", srcustomint3  = " & drutama("srcustomint3") & ", srcustomdbl1  = '" & FixDouble(drutama("srcustomdbl1")) & "', srcustomdbl2  = '" & FixDouble(drutama("srcustomdbl2")) & "', srcustomdbl3  = '" & FixDouble(drutama("srcustomdbl3")) & "', srcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', srcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', srcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', srjenis = '" & FixQuotes(drutama("srjenis")) & "' where srid = '" & drutama("srid") & "'"
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

                    If drutama("srautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("srcabang"), drutama("srlokasi"), drutama("srsumber"), drutama("srtgl"))
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
                        notransaksi = drutama("srnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(srid) FROM m5_sr WHERE srnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Sr (srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srjenis) values('" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(drutama("srgudang")) & "', '" & FixQuotes(drutama("srasalbarang")) & "', " & drutama("srasalbarangkategori") & ", '" & FixQuotes(drutama("srjenispenjulan")) & "', " & drutama("srjenispenjualankategori") & ", " & drutama("srcarabayar") & ", '" & FixQuotes(drutama("srsumber")) & "', " & drutama("srautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srkodepa") & ", " & drutama("srcustomer") & ", '" & FixQuotes(drutama("srcustomerkontak")) & "', '" & FixQuotes(drutama("sr1alamat1")) & "', '" & FixQuotes(drutama("sr1alamat2")) & "', '" & FixQuotes(drutama("sr1alamat3")) & "', '" & FixQuotes(drutama("sr2alamat1")) & "', '" & FixQuotes(drutama("sr2alamat2")) & "', '" & FixQuotes(drutama("sr2alamat3")) & "', " & drutama("srbagianpenjualan") & ", '" & FixQuotes(drutama("srekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', '" & FixQuotes(drutama("srtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(drutama("srnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', '" & FixQuotes(drutama("srmatauang")) & "', '" & FixDouble(drutama("srkurs")) & "', " & drutama("srhargatermasukpajak") & ", '" & FixDouble(drutama("srtotal")) & "', '" & FixQuotes(drutama("srdiskonpersen")) & "', '" & FixDouble(drutama("srjmldiskon")) & "', '" & FixDouble(drutama("srtotalpajak1detail")) & "', '" & FixDouble(drutama("srtotalpajak2detail")) & "', '" & FixDouble(drutama("srbiayalainpersen")) & "', '" & FixDouble(drutama("srbiayalain")) & "', '" & FixDouble(drutama("srtotaltransaksi")) & "', '" & FixDouble(drutama("srsisatransaksi")) & "', '" & FixDouble(drutama("srjmlbayar")) & "', " & drutama("srstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', '" & FixQuotes(drutama("srnofakturpajak")) & "', " & drutama("srsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', '" & FixQuotes(drutama("srrekdiskon")) & "', '" & FixQuotes(drutama("srrekpajak1")) & "', '" & FixQuotes(drutama("srrekpajak2")) & "', '" & FixQuotes(drutama("srrekbiayalain")) & "', '" & FixQuotes(drutama("srreksisa")) & "', '" & FixQuotes(drutama("srrekbayar")) & "', " & drutama("sridsq") & ", " & drutama("sridso") & ", " & drutama("sridpl") & ", " & drutama("sriddo") & ", " & drutama("sriddr") & ", " & drutama("sridpi") & ", " & drutama("sridsi") & ", " & drutama("sridrnr") & ", " & drutama("srstatus") & ", " & drutama("srstatussebelumnya") & ", " & drutama("srjmlrevisi") & ", " & drutama("srcetakanke") & ", " & drutama("srinputuser") & ", NOW(), " & drutama("srmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("srtutupperiode") & ", " & drutama("srisclose") & ", '" & FixQuotes(drutama("srcustomtext1")) & "', '" & FixQuotes(drutama("srcustomtext2")) & "', '" & FixQuotes(drutama("srcustomtext3")) & "', '" & FixQuotes(drutama("srcustomtext4")) & "', '" & FixQuotes(drutama("srcustomtext5")) & "', " & drutama("srcustomint1") & ", " & drutama("srcustomint2") & ", " & drutama("srcustomint3") & ", '" & FixDouble(drutama("srcustomdbl1")) & "', '" & FixDouble(drutama("srcustomdbl2")) & "', '" & FixDouble(drutama("srcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', '" & FixQuotes(drutama("srjenis")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select srid from M5_sr where srnotransaksi='" & notransaksi & "' AND srinputuser= '" & userid & "' order by srmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_Sr_Detail where idsr = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsrdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("idhppkhususkeluar") & ", " & dr1("idhppfifokeluar") & ", '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("hargapricelist")) & "', '" & FixDouble(dr1("hpp")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudangasal")) & "', '" & FixQuotes(dr1("gudangtransit")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', '" & FixQuotes(dr1("rekpersediaan")) & "', '" & FixQuotes(dr1("rekhargapokok")) & "', '" & FixQuotes(dr1("rekdiskonpenjualan")) & "', '" & FixQuotes(dr1("rekreturpenjualan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", " & dr1("idsodetail") & ", " & dr1("idpldetail") & ", " & dr1("iddodetail") & ", " & dr1("iddrdetail") & ", " & dr1("idpidetail") & ", " & dr1("idsidetail") & ", " & dr1("idrnrdetail") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_Sr_Detail(idsrdetail, idsr, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, idhppkhususkeluar, idhppfifokeluar, harga, hargapricelist, hpp, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudangasal, gudangtransit, gudangtujuan, rekpersediaan, rekhargapokok, rekdiskonpenjualan, rekreturpenjualan, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, idsodetail, idpldetail, iddodetail, iddrdetail, idpidetail, idsidetail, idrnrdetail, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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


                'VALIDASI KETIKA SR LANGSUNG (SRJENIS = 1) MAKA TIDAK BOLEH AMBIL LEBIH DARI 1 NOMOR SI
                Dim IdSI As Double = 0
                If drutama("srjenis") = 1 Then
                    sql = "SELECT si.siid, si.sinotransaksi, si.sitotaltransaksi, si.sijmlbayar FROM m5_sr_detail srd JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail JOIN m5_si si ON sid.idsi = si.siid WHERE srd.idsr = '" & result(4) & "' GROUP BY si.siid"
                    Dim dtCekSI As DataTable = AsDataTableAmbilDariDB(sql)
                    If dtCekSI.Rows.Count > 1 Then
                        result(2) = "Direct SR (Sales Retur) can only pick from one SI (Sales Invoice) transaction." : Trans.Rollback() : GoTo selesai

                    ElseIf dtCekSI.Rows.Count = 1 Then
                        'VALIDASI KETIKA SR LANGSUNG (SRJENIS = 1) MAKA TOTAL TRANSAKSI SR TIDAK BOLEH MELEBIHI SISA SI YANG BELUM DIBAYAR
                        If Len(dtCekSI.Rows(0)("siid")) > 0 Then
                            IdSI = Double.Parse(dtCekSI.Rows(0)("siid"))
                            If Double.Parse(drutama("srtotaltransaksi")) > (Double.Parse(dtCekSI.Rows(0)("sitotaltransaksi")) - Double.Parse(dtCekSI.Rows(0)("sijmlbayar"))) Then
                                Dim selisih(2) As String
                                selisih = F_Nominal(F_Round((Double.Parse(dtCekSI.Rows(0)("sitotaltransaksi")) - Double.Parse(dtCekSI.Rows(0)("sijmlbayar")))), True).Split(sptSubParam)

                                result(2) = "Total Direct SR (Sales Retur) exceeds the AR (Account Receivables) from SI (Sales Invoice) transaction no. " & dtCekSI.Rows(0)("sinotransaksi") & ". AR available : " & drutama("srmatauang") & " " & selisih(1) : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                    End If
                End If


                'Hapus batch ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_No_Batch_Transaction  where nbtidtransaksi  = '" & result(4) & "' AND nbtsumber = 'SR'"
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
                    sql = "Delete from M1_No_Serial_Transaction  where nstidtransaksi  = '" & result(4) & "' AND nstsumber = 'SR'"
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


                If drutama("srstatus") = 2 Then
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

                    If Len(updNilaiRNR) > 0 Then 'RNR
                        'UPDATE DETAIL
                        sql = "UPDATE m5_rnr_detail SET jmlrealisasi = (CASE idrnrdetail " & updNilaiRNR & " ELSE jmlrealisasi END) WHERE " & updFilterRNR
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idrnr FROM m5_rnr_detail WHERE " & updFilterRNR & " GROUP BY idrnr")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idrnr = '" & dr1("idrnr") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idrnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_rnr_detail WHERE " & ftDetail & " GROUP BY idrnr")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiRNR = "" : updFilterRNR = ""
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
                                updNilaiRNR = String.Concat(updNilaiRNR, "WHEN '" & dr1("idrnr") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                                updFilterRNR = String.Concat(updFilterRNR, "(rnrid = '" & dr1("idrnr") & "')")
                            Next

                            sql = "UPDATE m5_rnr SET rnrstatusrealisasi = (CASE rnrid " & updNilaiRNR & " ELSE rnrstatusrealisasi END) WHERE " & updFilterRNR
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


                    'JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI =========================
                    If drutama("srjenis") = 1 And IdSI > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET si.sijmlbayar = si.sijmlbayar + " & Double.Parse(drutama("srtotaltransaksi")) & ", si.sitgllunas = (CASE WHEN si.sijmlbayar + " & Double.Parse(drutama("srtotaltransaksi")) & " >= si.sitotaltransaksi THEN '" & AsFormatTanggal(FixQuotes(drutama("srtgl"))) & "' ELSE si.sitgllunas END) WHERE si.siid = '" & IdSI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE si.siid = '" & IdSI & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If
                    'END OF JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI ==================


                    'AMBIL DATA DETAIL YANG BARU ++++++++++++++++++++++++++++++++++++++++++++++++++++
                    Dim dtDetailNew As DataTable = AsDataTableAmbilDariDB("SELECT srd.idsrdetail, srd.idbarang, srd.namabarang, srd.tipebarang, srd.jml, srd.satuan, srd.jmlbarang, srd.satuanbarang, srd.matauang, srd.kurs, srd.harga, srd.diskon, srd.jmldiskon, srd.hpp, srd.idhppkhususkeluar, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.catatan, srd.costcenter, srd.divisi, srd.subdivisi, srd.proyek, sr.srinputtgl, i.bhpp, IFNULL(sid.hpp,srd.hpp)as hppbaru FROM m5_sr_detail srd JOIN m5_sr sr ON srd.idsr = sr.srid JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m5_si_detail sid ON srd.idsidetail=sid.idsidetail WHERE srd.idsr = '" & result(4) & "'")

                    Dim hpp As Double = 0, postinghpp As Double = 0, gudang As String = "", bstok As Double = 0
                    Dim jenismutasi As Double = 0, saldojml As Double = 0, saldohpp As Double = 0, saldonilai As Double = 0
                    Dim strTransaksiBarang As New StringBuilder, dtSaldo As New DataTable

                    If dtDetailNew.Rows.Count > 0 Then

                        'INSERT ITEM TRANSACTION ====================================================
                        For Each dr1 As DataRow In dtDetailNew.Rows 'SET NILAI VARIABEL
                            'SET NILAI VARIABEL
                            idbarang = Double.Parse(dr1("idbarang"))
                            jmlbarang = Double.Parse(dr1("jmlbarang"))
                            gudang = dr1("gudangtujuan")

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
                                'mapping                        id,                            cabang,                                    lokasi,                                 gudang,                          kodepa,           jenismutasi,                              sumber,                    idutama,             iddetail,                      notransaksi,                                                 tgl,                          kontak,                idbarang,                           namabarang,                             tipebarang,                           tipehpp,                        jml,                             satuan,                            jmlbarang,                             satuanbarang,                             matauang,                             kurs,                             harga,                             diskon,                             jmldiskon,          idhppikm,                         idhppikk,                hpp,                                  uraian,                                    catatan,                     catatandetail,                               costcenter,                             divisi,                             subdivisi,                             proyek,                        saldojml,                      saldohpp,                      saldonilai,                                             inputtgl,                                              inputuser,  postingtgl, updatehpp,         postinghpp, hppfix,postingjurnal, jurnalfix,tutupperiode, isclose,             customtext1,             customtext2,             customtext3,          customtext4,                customtext5,             customtext6,             customtext7,             customtext8,             customtext9,            customtext10,customint1,customint2,customint3,customint4,customint5,customint6,customint7,customint8,customint9,customint10,            customdbl1,             customdbl2,             customdbl3,             customdbl4,             customdbl5,             customdbl6,             customdbl7,             customdbl8,             customdbl9,            customdbl10,                                 customdate1,                                        customdate2,                                        customdate3,                                        customdate4,                                        customdate5,                                        customdate6,                                        customdate7,                                        customdate8,                                        customdate9,                                        customdate10
                                strTransaksiBarang.Append("(" & 0 & ",'" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(dr1("gudangtujuan")) & "', " & drutama("srkodepa") & ", " & jenismutasi & ", '" & FixQuotes(drutama("srsumber")) & "', " & result(4) & ", " & dr1("idsrdetail") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srcustomer") & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', " & 0 & ", " & dr1("idhppkhususkeluar") & ", '" & FixDouble(hpp) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixDouble(saldojml) & "', '" & FixDouble(saldohpp) & "', '" & FixDouble(saldonilai) & "', '" & FixQuotes(AsFormatTanggal(dr1("srinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & drutama("srinputuser") & ", NOW(), " & 0 & ", " & postinghpp & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', '" & FixQuotes("") & "', " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", " & 0 & ", '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixDouble(0) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "', '" & FixQuotes(AsFormatTanggal("1900-01-01")) & "')")
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
                Dim sumber As String = "SR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                If drutama("srstatus") = 2 Then
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
                If drutama("srstatus") = 2 Then
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
    Public Function M5_SrUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "SR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            Dim srjenis As Integer = 0, srtotaltransaksi As Double = 0

            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Srtgl, Srnotransaksi, Srstatus, Srjenis, Srtotaltransaksi FROM M5_Sr WHERE Srid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
                'srjenis                                        'srtotaltransaksi
                srjenis = Integer.Parse(dtdetail.Rows(1)(3)) : srtotaltransaksi = Double.Parse(dtdetail.Rows(1)(4))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Srstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sr_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_sr_terkait("srid = '" & idtransaksi & "'")
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


                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsrdetail As Integer = 0, idsidetail As Integer = 0, idrnrdetail As Integer = 0
                Dim idhppkhususmasuk As Integer = 0, idhppkhususkeluar As Integer = 0, idhppfifomasuk As Integer = 0, idhppfifokeluar As Integer = 0
                Dim updNilaiSI As String = "", updFilterSI As String = "", updNilaiRNR As String = "", updFilterRNR As String = ""
                Dim ftExistStok As String = "", ftStok As String = "", updStokOut As String = "", gudangOut As String = ""
                Dim updStokIn As String = "", gudangIn As String = ""
                Dim ftHppI As String = "", ftHppF As String = ""
                Dim updStokBarang As String = "", ftStokBarang As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT srd.idsrdetail, srd.idbarang, i.bkode as kodebarang, srd.tipebarang, srd.namabarang, srd.satuan, srd.nilaisatuan, srd.jmlbarang, srd.idsidetail, srd.idrnrdetail, srd.gudangasal, srd.gudangtransit, srd.gudangtujuan, srd.idhppkhususkeluar, srd.idhppfifokeluar, srd.urutan, IFNULL(cso.idhppikm,0) as idhppkhususmasuk, IFNULL(cso.jmlkeluar,0) as jmlkeluar, IFNULL(cfo.cfoidcfi,0) as idhppfifomasuk, IFNULL(cfo.cfojmlkeluar,0) as cfojmlkeluar, i.bhpp FROM m5_sr_detail srd JOIN m1_item i ON srd.idbarang = i.bid LEFT JOIN m1_cogs_special_out cso ON srd.idhppkhususkeluar=cso.idhppikk LEFT JOIN m1_cogs_fifo_out cfo ON srd.idhppfifokeluar=cfo.cfoid WHERE srd.idsr = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        '1. SET NILAI
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang")
                        idsrdetail = dr1("idsrdetail") : idsidetail = dr1("idsidetail") : idrnrdetail = dr1("idrnrdetail")
                        gudangIn = dr1("gudangtransit") : gudangOut = dr1("gudangtujuan")
                        idhppkhususmasuk = dr1("idhppkhususmasuk") : idhppkhususkeluar = dr1("idhppkhususkeluar")
                        idhppfifomasuk = dr1("idhppfifomasuk") : idhppfifokeluar = dr1("idhppfifokeluar")

                        '2. BUAT FILTER UPDATE OUTSTANDING
                        If idsidetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING SI
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsidetail=" & idsidetail)
                            updNilaiSI = String.Concat("WHEN '" & idsidetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiSI)

                            '2.2. SET FILTERUPDATE OUTSTANDING SI
                            updFilterSI = IIf(Len(updFilterSI.ToString) = 0, "", updFilterSI & " OR ")
                            updFilterSI = String.Concat(updFilterSI, "(idsidetail = '" & idsidetail & "')")
                        End If

                        If idrnrdetail <> 0 Then
                            '2.1 SET NILAI UPDATE OUTSTANDING RNR
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idrnrdetail=" & idrnrdetail)
                            updNilaiRNR = String.Concat("WHEN '" & idrnrdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilaiRNR)

                            '2.2. SET FILTERUPDATE OUTSTANDING RNR
                            updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                            updFilterRNR = String.Concat(updFilterRNR, "(idrnrdetail = '" & idrnrdetail & "')")
                        End If

                        'VALIDASI STOK -------------------------------
                        '1. CEK DATA EXIST
                        ftExistStok = IIf(Len(ftExistStok.ToString) = 0, "", ftExistStok & " UNION ")
                        ftExistStok = String.Concat(ftExistStok, "SELECT EXISTS(SELECT 1 FROM m1_item_stock_warehouse WHERE kgudang = '" & gudangOut & "' AND idbarang = '" & idbarang & "' LIMIT 1) as rowExists,  bkode, '" & gudangOut & "' as gudang FROM m1_item WHERE bjenis <> 'J' AND bid = '" & idbarang & "'")

                        '2. CEK JML STOK
                        Dim Stok As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang & " AND gudangtujuan='" & gudangOut & "'")
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
                        ftHppI = String.Concat(ftHppI, "(idbarang = '" & idbarang & "' AND idtransaksi = '" & idsrdetail & "' AND sumber = 'SR')")

                        '6. BUAT FILER CEK HPP FIFO(F)
                        ftHppF = IIf(Len(ftHppF.ToString) = 0, "", ftHppF & " OR ")
                        ftHppF = String.Concat(ftHppF, "(cfiidbarang = '" & idbarang & "' AND cfiidtransaksi = '" & idsrdetail & "' AND cfisumber = 'SR')")

                        '7 SET NILAI UPDATE STOK BARANG
                        Dim stokBarang As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idbarang=" & idbarang)
                        updStokBarang = String.Concat("WHEN '" & idbarang & "' THEN ROUND(bstok - '" & stokBarang & "', 5) ", updStokBarang)

                        '8. SET FILTERUPDATE STOK BARANG
                        ftStokBarang = IIf(Len(ftStokBarang.ToString) = 0, "", ftStokBarang & " OR ")
                        ftStokBarang = String.Concat(ftStokBarang, "(bid = '" & idbarang & "')")

                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                'VALIDASI STOK ----------------------------------
                Dim rsValidasi As String = ValidasiSimpan(dtdetail, "", "", "", "", ftExistStok, ftStok, ftHppI, ftHppF, "", "", "")
                If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                'END OF VALIDASI STOK ---------------------------


                'UPDATE OUTSTANDING TRANSAKSI ====================================================
                If Len(updFilterSI) > 0 Then 'SI
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
                End If

                If Len(updFilterRNR) > 0 Then 'RNR
                    'UPDATE OUTSTANDING DETAIL -------------------
                    sql = "UPDATE m5_rnr_detail SET jmlrealisasi = (CASE idrnrdetail " & updNilaiRNR & " ELSE jmlrealisasi END) WHERE " & updFilterRNR
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idrnr FROM m5_rnr_detail WHERE " & updFilterRNR & " GROUP BY idrnr")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idrnr = '" & dr1("idrnr") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idrnr, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_rnr_detail WHERE " & ftDetail & " GROUP BY idrnr")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiRNR = "" : updFilterRNR = ""
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
                            updNilaiRNR = String.Concat(updNilaiRNR, "WHEN '" & dr1("idrnr") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterRNR = IIf(Len(updFilterRNR.ToString) = 0, "", updFilterRNR & " OR ")
                            updFilterRNR = String.Concat(updFilterRNR, "(rnrid = '" & dr1("idrnr") & "')")
                        Next

                        sql = "UPDATE m5_rnr SET rnrstatusrealisasi = (CASE rnrid " & updNilaiRNR & " ELSE rnrstatusrealisasi END) WHERE " & updFilterRNR
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


                'JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI =========================
                If srjenis = 1 Then
                    'AMBIL IDSI DARI DATA SR DETAIL
                    sql = "SELECT sid.idsi FROM m5_sr_detail srd JOIN m5_si_detail sid ON srd.idsidetail = sid.idsidetail WHERE srd.idsr = '" & idtransaksi & "' GROUP BY sid.idsi"
                    Dim dtSI As DataTable = AsDataTableAmbilDariDB(sql)
                    Dim IdSi As Double = 0
                    If dtSI.Rows.Count > 0 Then
                        If Len(dtSI.Rows(0)("idsi")) > 0 Then
                            IdSi = Double.Parse(dtSI.Rows(0)("idsi"))
                        End If
                    End If

                    'UPDATE JMLBAYAR SI
                    If IdSi > 0 Then
                        'TRANSAKSI
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET si.sijmlbayar = si.sijmlbayar - " & srtotaltransaksi & ", si.sitgllunas = '" & FixQuotes("1900-01-01") & "' WHERE si.siid = '" & IdSi & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'JURNAL
                        sql = "UPDATE m5_si si LEFT JOIN m2_transaction_journal t ON si.sisumber = t.tsumber AND si.siid = t.tidtransaksi AND si.sinotransaksi = t.tnotransaksi SET t.tstatuslunas = si.sistatuslunas, t.ttgllunas = si.sitgllunas WHERE si.siid = '" & IdSi & "'"
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
                'END OF JIKA SR LANGSUNG (SRJENIS = 1) MAKA UPDATE JMLBAYAR SI ==================


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


                'DELETE HPP KHUSUS (I)
                sql = "DELETE FROM m1_cogs_special_in WHERE " & ftHppI
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'DELETE HPP FIFO (F)
                sql = "DELETE FROM m1_cogs_fifo_in WHERE " & ftHppF
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
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
                        .Connection = Con1
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
                '        .Connection = Con1
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
                'sql = "  UPDATE m1_item i"
                'sql &= " JOIN m5_sr_detail srd ON i.bid = srd.idbarang AND srd.idsr = '" & FixDouble(idtransaksi) & "'"
                'sql &= " LEFT JOIN"
                'sql &= " (SELECT i.bid as idbarang, ROUND(SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang),2) as hppaverage"
                'sql &= " FROM m1_item_transaction it"
                'sql &= " JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1"
                'sql &= " JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1"
                'sql &= " JOIN m5_sr_detail srd ON it.idbarang = srd.idbarang AND srd.idsr = '" & FixDouble(idtransaksi) & "'"
                'sql &= " JOIN m5_sr sr ON srd.idsr = sr.srid AND CONCAT(it.sumber,it.idutama) <> CONCAT(sr.srsumber,sr.srid)"
                'sql &= " GROUP BY it.idbarang) as h ON i.bid = h.idbarang"
                'sql &= " SET i.bhppaverage = (CASE i.bjenis WHEN 'P' THEN (CASE i.bstok WHEN 0 THEN 0 ELSE IFNULL(h.hppaverage,0) END) ELSE IFNULL(h.hppaverage,0) END)"

                sql = "  UPDATE m1_item i"
                sql &= " JOIN ("
                sql &= " SELECT srd.idbarang, ROUND(SUM(srd.jmlbarang * srd.hpp),2) as nilai, SUM(srd.jmlbarang) as jumlah"
                sql &= " FROM m5_sr_detail srd"
                sql &= " WHERE srd.idsr = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY srd.idbarang"
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
                sql = "DELETE FROM M2_Transaction_Journal WHERE tsumber = '" & sumber & "' AND tidtransaksi = '" & idtransaksi & "' AND tnotransaksi = '" & notransaksi & "'"
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
            sql = "UPDATE M5_Sr SET Srstatus = " & nilaiStatus & ", Srmodifikasiuser='" & userid & "', Srmodifikasitgl = NOW(), Srposting = 0, Srpostingtgl = '1971-01-01 00:00:00', Srjmlrevisi = Srjmlrevisi + 1 WHERE Srid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SrDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "Sr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Srid, Srnotransaksi FROM M5_Sr WHERE Srid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT srcabang, srlokasi, srsumber, srautonotransaksi, srnotransaksi, srtgl"
            sql &= " FROM M5_sr"
            sql &= " WHERE srid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("srcabang")
                lokasi = dtNomorNext.Rows(0)("srlokasi")
                sumber = dtNomorNext.Rows(0)("srsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("srautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("srnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("srtgl"))
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
            sql = "DELETE FROM M5_Sr_Detail WHERE idsr='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()


            'DELETE UTAMA
            sql = "DELETE FROM M5_Sr WHERE srid='" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M5_SrBalance(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama(), dataRowUtama() As String
        Dim tglLunas As String = ""

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
        'srid(0) As Integer, srcabang(1) As String, srlokasi(2) As String, srgudang(3) As String, srasalbarang(4) As String, 
        'srasalbarangkategori(5) As Integer, srjenispenjulan(6) As String, srjenispenjualankategori(7) As Integer, srcarabayar(8) As Integer, srsumber(9) As String, 
        'srautonotransaksi(10) As Integer, srnotransaksi(11) As String, srtgl(12) As Date, srkodepa(13) As Integer, srcustomer(14) As Integer, 
        'srcustomerkontak(15) As String, sr1alamat1(16) As String, sr1alamat2(17) As String, sr1alamat3(18) As String, sr2alamat1(19) As String, 
        'sr2alamat2(20) As String, sr2alamat3(21) As String, srbagianpenjualan(22) As Integer, srekspedisi(23) As String, srtglkirim(24) As Date, 
        'srtermin(25) As String, srtgljatuhtempo(26) As Date, sruraian(27) As String, srcatatan(28) As String, srnoref(29) As String, 
        'srtglnoref(30) As Date, srtglpenutupan(31) As Date, srmatauang(32) As String, srkurs(33) As Double, srhargatermasukpajak(34) As Integer, 
        'srtotal(35) As Double, srdiskonpersen(36) As String, srjmldiskon(37) As Double, srtotalpajak1detail(38) As Double, srtotalpajak2detail(39) As Double, 
        'srbiayalainpersen(40) As Double, srbiayalain(41) As Double, srtotaltransaksi(42) As Double, srsisatransaksi(43) As Double, srjmlbayar(44) As Double, 
        'srstatuslunas(45) As Integer, srtgllunas(46) As Date, srnofakturpajak(47) As String, srsdhbayarpajak(48) As Integer, srtglbayarpajak(49) As Date, 
        'srrekdiskon(50) As String, srrekpajak1(51) As String, srrekpajak2(52) As String, srrekbiayalain(53) As String, srreksisa(54) As String, 
        'srrekbayar(55) As String, sridsq(56) As Integer, sridso(57) As Integer, sridpl(58) As Integer, sriddo(59) As Integer, 
        'sriddr(60) As Integer, sridpi(61) As Integer, sridsi(62) As Integer, sridrnr(63) As Integer, srstatus(64) As Integer, 
        'srstatussebelumnya(65) As Integer, srjmlrevisi(66) As Integer, srcetakanke(67) As Integer, srinputuser(68) As Integer, srinputtgl(69) As DateTime, 
        'srmodifikasiuser(70) As Integer, srmodifikasitgl(71) As DateTime, srposting(72) As Integer, srtutupperiode(73) As Integer, srisclose(74) As Integer, 
        'srcustomtext1(75) As String, srcustomtext2(76) As String, srcustomtext3(77) As String, srcustomtext4(78) As String, srcustomtext5(79) As String, 
        'srcustomint1(80) As Integer, srcustomint2(81) As Integer, srcustomint3(82) As Integer, srcustomdbl1(83) As Double, srcustomdbl2(84) As Double, 
        'srcustomdbl3(85) As Double, srcustomdate1(86) As Date, srcustomdate2(87) As Date, srcustomdate3(88) As Date

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, 
        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, 
        'srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "srid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srlokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjenispenjulan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sr2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sruraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srtotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsisatransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srstatuslunas", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtgllunas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srnofakturpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srsdhbayarpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtglbayarpajak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srreksisa", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sridsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sriddr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridpi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridsi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sridrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srtutupperiode", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "srcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "srcustomdate3", AsEnumTypeData.AsString)


        Dim JmlDt As Integer = dataUtama.Length
        For i = 1 To JmlDt
            'SPLIT DATA DETAIL
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA Utama -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowUtama.Length <> 89) Then
                result(2) = "Invalid main transaction data parameter. " & dataRowUtama.Length & "" : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW Utama ----------------------------

            ''VALIDASI TIPE DATA UTAMA ==========================================================
            'srid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "srid required numeric." : GoTo selesai
            End If
            'srasalbarangkategori(5) As Integer
            If (IsNumeric(dataRowUtama(5)) = False) Then
                result(2) = "srasalbarangkategori required numeric." : GoTo selesai
            End If
            'srjenispenjualankategori(7) As Integer
            If (IsNumeric(dataRowUtama(7)) = False) Then
                result(2) = "srjenispenjualankategori required numeric." : GoTo selesai
            End If
            'srcarabayar(8) As Integer
            If (IsNumeric(dataRowUtama(8)) = False) Then
                result(2) = "srcarabayar required numeric." : GoTo selesai
            End If
            'srautonotransaksi(10) As Integer
            If (IsNumeric(dataRowUtama(10)) = False) Then
                result(2) = "srautonotransaksi required numeric." : GoTo selesai
            End If
            'srtgl(12) As Date
            If (IsDate(dataRowUtama(12)) = False) Then
                result(2) = "srtgl required date." : GoTo selesai
            End If
            'srkodepa(13) As Integer
            If (IsNumeric(dataRowUtama(13)) = False) Then
                result(2) = "srkodepa required numeric." : GoTo selesai
            End If
            'srcustomer(14) As Integer
            If (IsNumeric(dataRowUtama(14)) = False) Then
                result(2) = "srcustomer required numeric." : GoTo selesai
            End If
            If (dataRowUtama(14) < 1) Then
                result(2) = "srcustomer can't be empty." : GoTo selesai
            End If
            'srbagianpenjualan(22) As Integer
            If (IsNumeric(dataRowUtama(22)) = False) Then
                result(2) = "srbagianpenjualan required numeric." : GoTo selesai
            End If
            If (dataRowUtama(22) < 1) Then
                result(2) = "srbagianpenjualan can't be empty." : GoTo selesai
            End If
            'srtglkirim(24) As Date
            If (IsDate(dataRowUtama(24)) = False) Then
                result(2) = "srtglkirim required date." : GoTo selesai
            End If
            'srtgljatuhtempo(26) As Date
            If (IsDate(dataRowUtama(26)) = False) Then
                result(2) = "srtgljatuhtempo required date." : GoTo selesai
            End If
            'srtglnoref(30) As Date
            If (IsDate(dataRowUtama(30)) = False) Then
                result(2) = "srtglnoref required date." : GoTo selesai
            End If
            'srtglpenutupan(31) As Date
            If (IsDate(dataRowUtama(31)) = False) Then
                result(2) = "srtglpenutupan required date." : GoTo selesai
            End If
            'srkurs(33) As Double
            If (IsNumeric(dataRowUtama(33)) = False) Then
                result(2) = "srkurs required numeric." : GoTo selesai
            End If
            'srhargatermasukpajak(34) As Integer
            If (IsNumeric(dataRowUtama(34)) = False) Then
                result(2) = "srhargatermasukpajak required numeric." : GoTo selesai
            End If
            'srtotal(35) As Double
            If (IsNumeric(dataRowUtama(35)) = False) Then
                result(2) = "srtotal required numeric." : GoTo selesai
            End If
            'srjmldiskon(37) As Double
            If (IsNumeric(dataRowUtama(37)) = False) Then
                result(2) = "srjmldiskon required numeric." : GoTo selesai
            End If
            'srtotalpajak1detail(38) As Double
            If (IsNumeric(dataRowUtama(38)) = False) Then
                result(2) = "srtotalpajak1detail required numeric." : GoTo selesai
            End If
            'srtotalpajak2detail(39) As Double
            If (IsNumeric(dataRowUtama(39)) = False) Then
                result(2) = "srtotalpajak2detail required numeric." : GoTo selesai
            End If
            ''srbiayalainpersen(40) As Double
            'If (IsNumeric(dataRowutama(40)) = False) Then
            '    result(2) = "srbiayalainpersen required numeric." : GoTo selesai
            'End If
            'srbiayalain(41) As Double
            If (IsNumeric(dataRowUtama(41)) = False) Then
                result(2) = "srbiayalain required numeric." : GoTo selesai
            End If
            'srtotaltransaksi(42) As Double
            If (IsNumeric(dataRowUtama(42)) = False) Then
                result(2) = "srtotaltransaksi required numeric." : GoTo selesai
            End If
            'srsisatransaksi(43) As Double
            If (IsNumeric(dataRowUtama(43)) = False) Then
                result(2) = "srsisatransaksi required numeric." : GoTo selesai
            End If
            'srjmlbayar(44) As Double
            If (IsNumeric(dataRowUtama(44)) = False) Then
                result(2) = "srjmlbayar required numeric." : GoTo selesai
            End If
            'srstatuslunas(45) As Integer
            If (IsNumeric(dataRowUtama(45)) = False) Then
                result(2) = "srstatuslunas required numeric." : GoTo selesai
            End If
            'srtgllunas(46) As Date
            If (IsDate(dataRowUtama(46)) = False) Then
                result(2) = "srtgllunas required date." : GoTo selesai
            End If
            'srsdhbayarpajak(48) As Integer
            If (IsNumeric(dataRowUtama(48)) = False) Then
                result(2) = "srsdhbayarpajak required numeric." : GoTo selesai
            End If
            'srtglbayarpajak(49) As Date
            If (IsDate(dataRowUtama(49)) = False) Then
                result(2) = "srtglbayarpajak required date." : GoTo selesai
            End If
            'sridsq(56) As Integer
            If (IsNumeric(dataRowUtama(56)) = False) Then
                result(2) = "sridsq required numeric." : GoTo selesai
            End If
            'sridso(57) As Integer
            If (IsNumeric(dataRowUtama(57)) = False) Then
                result(2) = "sridso required numeric." : GoTo selesai
            End If
            'sridpl(58) As Integer
            If (IsNumeric(dataRowUtama(58)) = False) Then
                result(2) = "sridpl required numeric." : GoTo selesai
            End If
            'sriddo(59) As Integer
            If (IsNumeric(dataRowUtama(59)) = False) Then
                result(2) = "sriddo required numeric." : GoTo selesai
            End If
            'sriddr(60) As Integer
            If (IsNumeric(dataRowUtama(60)) = False) Then
                result(2) = "sriddr required numeric." : GoTo selesai
            End If
            'sridpi(61) As Integer
            If (IsNumeric(dataRowUtama(61)) = False) Then
                result(2) = "sridpi required numeric." : GoTo selesai
            End If
            'sridsi(62) As Integer
            If (IsNumeric(dataRowUtama(62)) = False) Then
                result(2) = "sridsi required numeric." : GoTo selesai
            End If
            'sridrnr(63) As Integer
            If (IsNumeric(dataRowUtama(63)) = False) Then
                result(2) = "sridrnr required numeric." : GoTo selesai
            End If
            'srstatus(64) As Integer
            If (IsNumeric(dataRowUtama(64)) = False) Then
                result(2) = "srstatus required numeric." : GoTo selesai
            End If
            'srstatussebelumnya(65) As Integer
            If (IsNumeric(dataRowUtama(65)) = False) Then
                result(2) = "srstatussebelumnya required numeric." : GoTo selesai
            End If
            'srjmlrevisi(66) As Integer
            If (IsNumeric(dataRowUtama(66)) = False) Then
                result(2) = "srjmlrevisi required numeric." : GoTo selesai
            End If
            'srcetakanke(67) As Integer
            If (IsNumeric(dataRowUtama(67)) = False) Then
                result(2) = "srcetakanke required numeric." : GoTo selesai
            End If
            'srinputuser(68) As Integer
            If (IsNumeric(dataRowUtama(68)) = False) Then
                result(2) = "srinputuser required numeric." : GoTo selesai
            End If
            'srinputtgl(69) As DateTime
            If (IsDate(dataRowUtama(69)) = False) Then
                result(2) = "srinputtgl required date." : GoTo selesai
            End If
            'srmodifikasiuser(70) As Integer
            If (IsNumeric(dataRowUtama(70)) = False) Then
                result(2) = "srmodifikasiuser required numeric." : GoTo selesai
            End If
            'srmodifikasitgl(71) As DateTime
            If (IsDate(dataRowUtama(71)) = False) Then
                result(2) = "srmodifikasitgl required date." : GoTo selesai
            End If
            'srposting(72) As Integer
            If (IsNumeric(dataRowUtama(72)) = False) Then
                result(2) = "srposting required numeric." : GoTo selesai
            End If
            'srtutupperiode(73) As Integer
            If (IsNumeric(dataRowUtama(73)) = False) Then
                result(2) = "srtutupperiode required numeric." : GoTo selesai
            End If
            'srisclose(74) As Integer
            If (IsNumeric(dataRowUtama(74)) = False) Then
                result(2) = "srisclose required numeric." : GoTo selesai
            End If
            'srcustomint1(80) As Integer
            If (IsNumeric(dataRowUtama(80)) = False) Then
                result(2) = "srcustomint1 required numeric." : GoTo selesai
            End If
            'srcustomint2(81) As Integer
            If (IsNumeric(dataRowUtama(81)) = False) Then
                result(2) = "srcustomint2 required numeric." : GoTo selesai
            End If
            'srcustomint3(82) As Integer
            If (IsNumeric(dataRowUtama(82)) = False) Then
                result(2) = "srcustomint3 required numeric." : GoTo selesai
            End If
            'srcustomdbl1(83) As Double
            If (IsNumeric(dataRowUtama(83)) = False) Then
                result(2) = "srcustomdbl1 required numeric." : GoTo selesai
            End If
            'srcustomdbl2(84) As Double
            If (IsNumeric(dataRowUtama(84)) = False) Then
                result(2) = "srcustomdbl2 required numeric." : GoTo selesai
            End If
            'srcustomdbl3(85) As Double
            If (IsNumeric(dataRowUtama(85)) = False) Then
                result(2) = "srcustomdbl3 required numeric." : GoTo selesai
            End If
            'srcustomdate1(86) As Date
            If (IsDate(dataRowUtama(86)) = False) Then
                result(2) = "srcustomdate1 required date." : GoTo selesai
            End If
            'srcustomdate2(87) As Date
            If (IsDate(dataRowUtama(87)) = False) Then
                result(2) = "srcustomdate2 required date." : GoTo selesai
            End If
            'srcustomdate3(88) As Date
            If (IsDate(dataRowUtama(88)) = False) Then
                result(2) = "srcustomdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'srcabang(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "srcabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "srcabang should not be more than 25 character." : GoTo selesai
            End If

            'srlokasi(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "srlokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 25 Then
                result(2) = "srlokasi should not be more than 25 character." : GoTo selesai
            End If

            'srgudang(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "srgudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 25 Then
                result(2) = "srgudang should not be more than 25 character." : GoTo selesai
            End If

            'srsumber(9) As String
            If Len(dataRowUtama(9)) = 0 Then
                result(2) = "srsumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(9)) > 10 Then
                result(2) = "srsumber should not be more than 10 character." : GoTo selesai
            End If

            'srnotransaksi(11) As String
            If Len(dataRowUtama(11)) = 0 Then
                result(2) = "srnotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(11)) > 50 Then
                result(2) = "srnotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'srtgl(12) As Date
            If Len(dataRowUtama(12)) = 0 Then
                result(2) = "srtgl can't be empty" : GoTo selesai
            End If

            'srtglkirim(24) As Date
            If Len(dataRowUtama(24)) = 0 Then
                result(2) = "srtglkirim can't be empty" : GoTo selesai
            End If

            'srtgljatuhtempo(26) As Date
            If Len(dataRowUtama(26)) = 0 Then
                result(2) = "srtgljatuhtempo can't be empty" : GoTo selesai
            End If

            'srtglnoref(30) As Date
            If Len(dataRowUtama(30)) = 0 Then
                result(2) = "srtglnoref can't be empty" : GoTo selesai
            End If

            'srtglpenutupan(31) As Date
            If Len(dataRowUtama(31)) = 0 Then
                result(2) = "srtglpenutupan can't be empty" : GoTo selesai
            End If

            'srmatauang(32) As String
            If Len(dataRowUtama(32)) = 0 Then
                result(2) = "srmatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(32)) > 25 Then
                result(2) = "srmatauang should not be more than 25 character." : GoTo selesai
            End If

            'srkurs(33) As Double
            If Len(dataRowUtama(33)) = 0 Then
                result(2) = "srkurs can't be empty" : GoTo selesai
            End If

            'srtotal(35) As Double
            If Len(dataRowUtama(35)) = 0 Then
                result(2) = "srtotal can't be empty" : GoTo selesai
            End If

            'srdiskonpersen(36) As String
            If Len(dataRowUtama(36)) = 0 Then
                result(2) = "srdiskonpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(36)) > 25 Then
                result(2) = "srdiskonpersen should not be more than 25 character." : GoTo selesai
            End If

            'srjmldiskon(37) As Double
            If Len(dataRowUtama(37)) = 0 Then
                result(2) = "srjmldiskon can't be empty" : GoTo selesai
            End If

            'srtotalpajak1detail(38) As Double
            If Len(dataRowUtama(38)) = 0 Then
                result(2) = "srtotalpajak1detail can't be empty" : GoTo selesai
            End If

            'srtotalpajak2detail(39) As Double
            If Len(dataRowUtama(39)) = 0 Then
                result(2) = "srtotalpajak2detail can't be empty" : GoTo selesai
            End If

            'srbiayalainpersen(40) As Double
            If Len(dataRowUtama(40)) = 0 Then
                result(2) = "srbiayalainpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(40)) > 25 Then
                result(2) = "srbiayalainpersen should not be more than 25 character." : GoTo selesai
            End If

            'srbiayalain(41) As Double
            If Len(dataRowUtama(41)) = 0 Then
                result(2) = "srbiayalain can't be empty" : GoTo selesai
            End If

            'srtotaltransaksi(42) As Double
            If Len(dataRowUtama(42)) = 0 Then
                result(2) = "srtotaltransaksi can't be empty" : GoTo selesai
            End If

            'srsisatransaksi(43) As Double
            If Len(dataRowUtama(43)) = 0 Then
                result(2) = "srsisatransaksi can't be empty" : GoTo selesai
            End If

            'srjmlbayar(44) As Double
            If Len(dataRowUtama(44)) = 0 Then
                result(2) = "srjmlbayar can't be empty" : GoTo selesai
            End If

            'srtgllunas(46) As Date
            If Len(dataRowUtama(46)) = 0 Then
                result(2) = "srtgllunas can't be empty" : GoTo selesai
            End If

            'srtglbayarpajak(49) As Date
            If Len(dataRowUtama(49)) = 0 Then
                result(2) = "srtglbayarpajak can't be empty" : GoTo selesai
            End If

            'srinputtgl(69) As DateTime
            If Len(dataRowUtama(69)) = 0 Then
                result(2) = "srinputtgl can't be empty" : GoTo selesai
            End If

            'srmodifikasitgl(71) As DateTime
            If Len(dataRowUtama(71)) = 0 Then
                result(2) = "srmodifikasitgl can't be empty" : GoTo selesai
            End If

            'srcustomdbl1(83) As Double
            If Len(dataRowUtama(83)) = 0 Then
                result(2) = "srcustomdbl1 can't be empty" : GoTo selesai
            End If

            'srcustomdbl2(84) As Double
            If Len(dataRowUtama(84)) = 0 Then
                result(2) = "srcustomdbl2 can't be empty" : GoTo selesai
            End If

            'srcustomdbl3(85) As Double
            If Len(dataRowUtama(85)) = 0 Then
                result(2) = "srcustomdbl3 can't be empty" : GoTo selesai
            End If

            'srcustomdate1(86) As Date
            If Len(dataRowUtama(86)) = 0 Then
                result(2) = "srcustomdate1 can't be empty" : GoTo selesai
            End If

            'srcustomdate2(87) As Date
            If Len(dataRowUtama(87)) = 0 Then
                result(2) = "srcustomdate2 can't be empty" : GoTo selesai
            End If

            'srcustomdate3(88) As Date
            If Len(dataRowUtama(88)) = 0 Then
                result(2) = "srcustomdate3 can't be empty" : GoTo selesai
            End If

            'END OF VALIDASI DATA UTAMA ================================================

            If AsDataTableTambahData(dtutama, "srid~srcabang~srlokasi~srgudang~srasalbarang~srasalbarangkategori~srjenispenjulan~srjenispenjualankategori~srcarabayar~srsumber~srautonotransaksi~srnotransaksi~srtgl~srkodepa~srcustomer~srcustomerkontak~sr1alamat1~sr1alamat2~sr1alamat3~sr2alamat1~sr2alamat2~sr2alamat3~srbagianpenjualan~srekspedisi~srtglkirim~srtermin~srtgljatuhtempo~sruraian~srcatatan~srnoref~srtglnoref~srtglpenutupan~srmatauang~srkurs~srhargatermasukpajak~srtotal~srdiskonpersen~srjmldiskon~srtotalpajak1detail~srtotalpajak2detail~srbiayalainpersen~srbiayalain~srtotaltransaksi~srsisatransaksi~srjmlbayar~srstatuslunas~srtgllunas~srnofakturpajak~srsdhbayarpajak~srtglbayarpajak~srrekdiskon~srrekpajak1~srrekpajak2~srrekbiayalain~srreksisa~srrekbayar~sridsq~sridso~sridpl~sriddo~sriddr~sridpi~sridsi~sridrnr~srstatus~srstatussebelumnya~srjmlrevisi~srcetakanke~srinputuser~srinputtgl~srmodifikasiuser~srmodifikasitgl~srposting~srtutupperiode~srisclose~srcustomtext1~srcustomtext2~srcustomtext3~srcustomtext4~srcustomtext5~srcustomint1~srcustomint2~srcustomint3~srcustomdbl1~srcustomdbl2~srcustomdbl3~srcustomdate1~srcustomdate2~srcustomdate3", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85) & "~" & dataRowUtama(86) & "~" & dataRowUtama(87) & "~" & dataRowUtama(88)) = False Then
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
                    Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("srtgl")), AsFormatTanggal(drutama("srtgl")))
                    arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                    If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                    'END OF CEK PERIODE AKUNTANSI ===========================


                    ''SET TGL JATUH TEMPO ====================================
                    'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                    'rsTglJT = F_TglJT(drutama("srtermin").ToString, AsFormatTanggal(drutama("srtgl")), "srtgl").Split(sptSubParam)
                    'If rsTglJT(0) = 0 Then
                    '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                    'Else
                    '    drutama("srtgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                    'End If
                    ''END OF SET TGL JATUH TEMPO =============================


                    If isUpdate Then
                        result(4) = drutama("srid")
                        notransaksi = drutama("srnotransaksi")
                        'JIKA UPDATE CEK JML ROW PADA DATABASE
                        dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(srid), srnotransaksi FROM M5_sr WHERE srid='" & result(4) & "' AND srstatus NOT IN(2,3,4,7)", myConn)
                        rowUpdate = dtupdate.Rows(0)(0)

                        If (rowUpdate > 0) Then

                            'CEK NO TRANSAKSI ======================
                            If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                                Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(srid) FROM M5_sr WHERE srnotransaksi='" & notransaksi & "'", myConn)
                                Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                                If cekNo > 0 Then
                                    result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                            'END OF CEK NO TRANSAKSI ===============

                            'SIMPAN HISTORY ========================
                            Dim SimpanHistory As New m5_sr_history
                            Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("srsumber")) & "▼" & FixQuotes(drutama("srid")) & "")
                            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                            If (rsSplitResult(1) = 0) Then
                                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF SIMPAN HISTORY ==================

                            sql = "Update M5_Sr set srcabang  = '" & FixQuotes(drutama("srcabang")) & "', srlokasi  = '" & FixQuotes(drutama("srlokasi")) & "', srgudang  = '" & FixQuotes(drutama("srgudang")) & "', srasalbarang  = '" & FixQuotes(drutama("srasalbarang")) & "', srasalbarangkategori  = " & drutama("srasalbarangkategori") & ", srjenispenjulan  = '" & FixQuotes(drutama("srjenispenjulan")) & "', srjenispenjualankategori  = " & drutama("srjenispenjualankategori") & ", srcarabayar  = " & drutama("srcarabayar") & ", srsumber  = '" & FixQuotes(drutama("srsumber")) & "', srautonotransaksi  = " & drutama("srautonotransaksi") & ", srnotransaksi  = '" & FixQuotes(notransaksi) & "', srtgl  = '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', srkodepa  = " & drutama("srkodepa") & ", srcustomer  = " & drutama("srcustomer") & ", srcustomerkontak  = '" & FixQuotes(drutama("srcustomerkontak")) & "', sr1alamat1  = '" & FixQuotes(drutama("sr1alamat1")) & "', sr1alamat2  = '" & FixQuotes(drutama("sr1alamat2")) & "', sr1alamat3  = '" & FixQuotes(drutama("sr1alamat3")) & "', sr2alamat1  = '" & FixQuotes(drutama("sr2alamat1")) & "', sr2alamat2  = '" & FixQuotes(drutama("sr2alamat2")) & "', sr2alamat3  = '" & FixQuotes(drutama("sr2alamat3")) & "', srbagianpenjualan  = " & drutama("srbagianpenjualan") & ", srekspedisi  = '" & FixQuotes(drutama("srekspedisi")) & "', srtglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', srtermin  = '" & FixQuotes(drutama("srtermin")) & "', srtgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', sruraian  = '" & FixQuotes(drutama("sruraian")) & "', srcatatan  = '" & FixQuotes(drutama("srcatatan")) & "', srnoref  = '" & FixQuotes(drutama("srnoref")) & "', srtglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', srtglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', srmatauang  = '" & FixQuotes(drutama("srmatauang")) & "', srkurs  = '" & FixDouble(drutama("srkurs")) & "', srhargatermasukpajak  = " & drutama("srhargatermasukpajak") & ", srtotal  = '" & FixDouble(drutama("srtotal")) & "', srdiskonpersen  = '" & FixQuotes(drutama("srdiskonpersen")) & "', srjmldiskon  = '" & FixDouble(drutama("srjmldiskon")) & "', srtotalpajak1detail  = '" & FixDouble(drutama("srtotalpajak1detail")) & "', srtotalpajak2detail  = '" & FixDouble(drutama("srtotalpajak2detail")) & "', srbiayalainpersen  = '" & FixDouble(drutama("srbiayalainpersen")) & "', srbiayalain  = '" & FixDouble(drutama("srbiayalain")) & "', srtotaltransaksi  = '" & FixDouble(drutama("srtotaltransaksi")) & "', srsisatransaksi  = '" & FixDouble(drutama("srsisatransaksi")) & "', srjmlbayar  = '" & FixDouble(drutama("srjmlbayar")) & "', srstatuslunas  = " & drutama("srstatuslunas") & ", srtgllunas  = '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', srnofakturpajak  = '" & FixQuotes(drutama("srnofakturpajak")) & "', srsdhbayarpajak  = " & drutama("srsdhbayarpajak") & ", srtglbayarpajak  = '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', srrekdiskon  = '" & FixQuotes(drutama("srrekdiskon")) & "', srrekpajak1  = '" & FixQuotes(drutama("srrekpajak1")) & "', srrekpajak2  = '" & FixQuotes(drutama("srrekpajak2")) & "', srrekbiayalain  = '" & FixQuotes(drutama("srrekbiayalain")) & "', srreksisa  = '" & FixQuotes(drutama("srreksisa")) & "', srrekbayar  = '" & FixQuotes(drutama("srrekbayar")) & "', sridsq  = " & drutama("sridsq") & ", sridso  = " & drutama("sridso") & ", sridpl  = " & drutama("sridpl") & ", sriddo  = " & drutama("sriddo") & ", sriddr  = " & drutama("sriddr") & ", sridpi  = " & drutama("sridpi") & ", sridsi  = " & drutama("sridsi") & ", sridrnr  = " & drutama("sridrnr") & ", srstatus  = " & drutama("srstatus") & ", srstatussebelumnya  = " & drutama("srstatussebelumnya") & ", srjmlrevisi  = srjmlrevisi+1, srcetakanke  = " & drutama("srcetakanke") & ", srmodifikasiuser  = " & drutama("srmodifikasiuser") & ", srmodifikasitgl  = NOW(), srposting  = 0, srtutupperiode  = " & drutama("srtutupperiode") & ", srcustomtext1  = '" & FixQuotes(drutama("srcustomtext1")) & "', srcustomtext2  = '" & FixQuotes(drutama("srcustomtext2")) & "', srcustomtext3  = '" & FixQuotes(drutama("srcustomtext3")) & "', srcustomtext4  = '" & FixQuotes(drutama("srcustomtext4")) & "', srcustomtext5  = '" & FixQuotes(drutama("srcustomtext5")) & "', srcustomint1  = " & drutama("srcustomint1") & ", srcustomint2  = " & drutama("srcustomint2") & ", srcustomint3  = " & drutama("srcustomint3") & ", srcustomdbl1  = '" & FixDouble(drutama("srcustomdbl1")) & "', srcustomdbl2  = '" & FixDouble(drutama("srcustomdbl2")) & "', srcustomdbl3  = '" & FixDouble(drutama("srcustomdbl3")) & "', srcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', srcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', srcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', srsaldoawal = 1 where srid = '" & drutama("srid") & "'"
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

                        If drutama("srautonotransaksi") = 1 Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("srcabang"), drutama("srlokasi"), drutama("srsumber"), drutama("srtgl"), drutama("srsumber"), 5)
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
                            notransaksi = drutama("srnotransaksi")
                        End If

                        'CEK NO TRANSAKSI ======================
                        Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(srid) FROM m5_sr WHERE srnotransaksi='" & notransaksi & "'", myConn)
                        Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                        If cekNo > 0 Then
                            result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        sql = "Insert into M5_Sr (srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srtutupperiode, srisclose, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srsaldoawal) values('" & FixQuotes(drutama("srcabang")) & "', '" & FixQuotes(drutama("srlokasi")) & "', '" & FixQuotes(drutama("srgudang")) & "', '" & FixQuotes(drutama("srasalbarang")) & "', " & drutama("srasalbarangkategori") & ", '" & FixQuotes(drutama("srjenispenjulan")) & "', " & drutama("srjenispenjualankategori") & ", " & drutama("srcarabayar") & ", '" & FixQuotes(drutama("srsumber")) & "', " & drutama("srautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgl"))) & "', " & drutama("srkodepa") & ", " & drutama("srcustomer") & ", '" & FixQuotes(drutama("srcustomerkontak")) & "', '" & FixQuotes(drutama("sr1alamat1")) & "', '" & FixQuotes(drutama("sr1alamat2")) & "', '" & FixQuotes(drutama("sr1alamat3")) & "', '" & FixQuotes(drutama("sr2alamat1")) & "', '" & FixQuotes(drutama("sr2alamat2")) & "', '" & FixQuotes(drutama("sr2alamat3")) & "', " & drutama("srbagianpenjualan") & ", '" & FixQuotes(drutama("srekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglkirim"))) & "', '" & FixQuotes(drutama("srtermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtgljatuhtempo"))) & "', '" & FixQuotes(drutama("sruraian")) & "', '" & FixQuotes(drutama("srcatatan")) & "', '" & FixQuotes(drutama("srnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srtglpenutupan"))) & "', '" & FixQuotes(drutama("srmatauang")) & "', '" & FixDouble(drutama("srkurs")) & "', " & drutama("srhargatermasukpajak") & ", '" & FixDouble(drutama("srtotal")) & "', '" & FixQuotes(drutama("srdiskonpersen")) & "', '" & FixDouble(drutama("srjmldiskon")) & "', '" & FixDouble(drutama("srtotalpajak1detail")) & "', '" & FixDouble(drutama("srtotalpajak2detail")) & "', '" & FixDouble(drutama("srbiayalainpersen")) & "', '" & FixDouble(drutama("srbiayalain")) & "', '" & FixDouble(drutama("srtotaltransaksi")) & "', '" & FixDouble(drutama("srsisatransaksi")) & "', '" & FixDouble(drutama("srjmlbayar")) & "', " & drutama("srstatuslunas") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtgllunas"))) & "', '" & FixQuotes(drutama("srnofakturpajak")) & "', " & drutama("srsdhbayarpajak") & ", '" & FixQuotes(AsFormatTanggal(drutama("srtglbayarpajak"))) & "', '" & FixQuotes(drutama("srrekdiskon")) & "', '" & FixQuotes(drutama("srrekpajak1")) & "', '" & FixQuotes(drutama("srrekpajak2")) & "', '" & FixQuotes(drutama("srrekbiayalain")) & "', '" & FixQuotes(drutama("srreksisa")) & "', '" & FixQuotes(drutama("srrekbayar")) & "', " & drutama("sridsq") & ", " & drutama("sridso") & ", " & drutama("sridpl") & ", " & drutama("sriddo") & ", " & drutama("sriddr") & ", " & drutama("sridpi") & ", " & drutama("sridsi") & ", " & drutama("sridrnr") & ", " & drutama("srstatus") & ", " & drutama("srstatussebelumnya") & ", " & drutama("srjmlrevisi") & ", " & drutama("srcetakanke") & ", " & drutama("srinputuser") & ", NOW(), " & drutama("srmodifikasiuser") & ", '1971-01-01 00:00:00', 0, " & drutama("srtutupperiode") & ", " & drutama("srisclose") & ", '" & FixQuotes(drutama("srcustomtext1")) & "', '" & FixQuotes(drutama("srcustomtext2")) & "', '" & FixQuotes(drutama("srcustomtext3")) & "', '" & FixQuotes(drutama("srcustomtext4")) & "', '" & FixQuotes(drutama("srcustomtext5")) & "', " & drutama("srcustomint1") & ", " & drutama("srcustomint2") & ", " & drutama("srcustomint3") & ", '" & FixDouble(drutama("srcustomdbl1")) & "', '" & FixDouble(drutama("srcustomdbl2")) & "', '" & FixDouble(drutama("srcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("srcustomdate3"))) & "', 1)"
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
                        dt2 = AsDataTableAmbilDariDBCon("select srid from M5_sr where srnotransaksi='" & notransaksi & "' AND srinputuser= '" & userid & "' order by srmodifikasitgl desc limit 1", myConn)
                        If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If


                    If drutama("srstatus") = 2 Then
                        'UPDATE TOTAL PIUTANG ============================================================
                        'PLAFON
                        'sql = "UPDATE m0_setting s JOIN m5_sr sr ON sr.srid = '" & result(4) & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSR' AND s.snilai = 1 JOIN m1_contact c ON c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang - (sr.srtotaltransaksi * sr.srkurs)"
                        sql = "UPDATE m5_sr sr JOIN m1_contact c ON sr.srid = '" & result(4) & "' AND c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang - (sr.srtotaltransaksi * sr.srkurs)"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                        'END OF UPDATE TOTAL PIUTANG =====================================================
                    End If


                    'INSERT MSMQ JURNAL =================================================================
                    Dim sumber As String = "SR", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                    If drutama("srstatus") = 2 Then
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
    Public Function M5_SrBUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "SR", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Srtgl, Srnotransaksi, Srstatus FROM M5_Sr WHERE Srid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Srstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New m5_sr_history
            Dim rsSimpanHistory As String = SimpanHistory.m5_Sr_HistorySimpan("" & paramSplit(0) & "★M5_Sr_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_sr_terkait("srid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================


                'UPDATE TOTAL PIUTANG ============================================================
                'PLAFON
                'sql = "UPDATE m0_setting s JOIN m5_sr sr ON sr.srid = '" & idtransaksi & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSR' AND s.snilai = 1 JOIN m1_contact c ON c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang + (sr.srtotaltransaksi * sr.srkurs)"
                sql = "UPDATE m5_sr sr JOIN m1_contact c ON sr.srid = '" & idtransaksi & "' AND c.kid = sr.srcustomer SET c.ktotalpiutang = c.ktotalpiutang + (sr.srtotaltransaksi * sr.srkurs)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
                'END OF UPDATE TOTAL PIUTANG =====================================================


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
            sql = "UPDATE M5_Sr SET Srstatus = " & nilaiStatus & ", Srmodifikasiuser='" & userid & "', Srmodifikasitgl = NOW(), Srposting = 0, Srpostingtgl = '1971-01-01 00:00:00', Srjmlrevisi = Srjmlrevisi + 1 WHERE Srid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SrBDelete(ByVal param As String) As String

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
            Dim sumber As String = "Sr", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Srid, Srnotransaksi FROM M5_Sr WHERE Srid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT srcabang, srlokasi, srsumber, srautonotransaksi, srnotransaksi, srtgl"
            sql &= " FROM M5_sr"
            sql &= " WHERE srid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("srcabang")
                lokasi = dtNomorNext.Rows(0)("srlokasi")
                sumber = dtNomorNext.Rows(0)("srsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("srautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("srnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("srtgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE UTAMA
            sql = "DELETE FROM M5_Sr WHERE srid='" & idtransaksi & "'"
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
                Dim rsNomorNext As String = M0_DeleteNotransaksi(cabang, lokasi, sumber, tgl, notransaksi, sumber, 5)
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
            Dim paramSearch As String = M5_SrSearch(PostWsSearch(paramSplit(0), "M5_SrSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
        strResultData = ""
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SrBSearch(ByVal param As String) As String
        'M5_SrBSearch --------------------------------------------------------
        'srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, 
        'srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, 
        'srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, 
        'sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, 
        'srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, 
        'srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, 
        'srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, 
        'srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, 
        'sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, 
        'sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, 
        'srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, 
        'srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, 
        'sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1, stcustomtext2
        'srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3,
        'srcustomdate1, srcustomdate2, srcustomdate3

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
            Filter = Filter.Replace("srcustomerkode", "c1.kkode")
            Filter = Filter.Replace("srcustomernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `sr`.`srid` AS `srid`,`sr`.`srcabang` AS `srcabang`,`sr`.`srlokasi` AS `srlokasi`,`sr`.`srgudang` AS `srgudang`,`sr`.`srasalbarang` AS `srasalbarang`,`sr`.`srasalbarangkategori` AS `srasalbarangkategori`,`sr`.`srjenispenjulan` AS `srjenispenjulan`,`sr`.`srjenispenjualankategori` AS `srjenispenjualankategori`,`sr`.`srcarabayar` AS `srcarabayar`,`sr`.`srsumber` AS `srsumber`,`sr`.`srautonotransaksi` AS `srautonotransaksi`,`sr`.`srnotransaksi` AS `srnotransaksi`,`sr`.`srtgl` AS `srtgl`,`sr`.`srkodepa` AS `srkodepa`,`sr`.`srcustomer` AS `srcustomer`,`sr`.`srcustomerkontak` AS `srcustomerkontak`,`sr`.`sr1alamat1` AS `sr1alamat1`,`sr`.`sr1alamat2` AS `sr1alamat2`,`sr`.`sr1alamat3` AS `sr1alamat3`,`sr`.`sr2alamat1` AS `sr2alamat1`,`sr`.`sr2alamat2` AS `sr2alamat2`,`sr`.`sr2alamat3` AS `sr2alamat3`,`sr`.`srbagianpenjualan` AS `srbagianpenjualan`,`sr`.`srekspedisi` AS `srekspedisi`,`sr`.`srtglkirim` AS `srtglkirim`,`sr`.`srtermin` AS `srtermin`,`sr`.`srtgljatuhtempo` AS `srtgljatuhtempo`,`sr`.`sruraian` AS `sruraian`,`sr`.`srcatatan` AS `srcatatan`,`sr`.`srnoref` AS `srnoref`,`sr`.`srtglnoref` AS `srtglnoref`,`sr`.`srtglpenutupan` AS `srtglpenutupan`,`sr`.`srmatauang` AS `srmatauang`,`sr`.`srkurs` AS `srkurs`,`sr`.`srhargatermasukpajak` AS `srhargatermasukpajak`,`sr`.`srtotal` AS `srtotal`,`sr`.`srdiskonpersen` AS `srdiskonpersen`,`sr`.`srjmldiskon` AS `srjmldiskon`,`sr`.`srtotalpajak1detail` AS `srtotalpajak1detail`,`sr`.`srtotalpajak2detail` AS `srtotalpajak2detail`,`sr`.`srbiayalainpersen` AS `srbiayalainpersen`,`sr`.`srbiayalain` AS `srbiayalain`,`sr`.`srtotaltransaksi` AS `srtotaltransaksi`,`sr`.`srsisatransaksi` AS `srsisatransaksi`,`sr`.`srjmlbayar` AS `srjmlbayar`,`sr`.`srstatuslunas` AS `srstatuslunas`,`sr`.`srtgllunas` AS `srtgllunas`,`sr`.`srnofakturpajak` AS `srnofakturpajak`,`sr`.`srsdhbayarpajak` AS `srsdhbayarpajak`,`sr`.`srtglbayarpajak` AS `srtglbayarpajak`,`sr`.`srrekdiskon` AS `srrekdiskon`,`sr`.`srrekpajak1` AS `srrekpajak1`,`sr`.`srrekpajak2` AS `srrekpajak2`,`sr`.`srrekbiayalain` AS `srrekbiayalain`,`sr`.`srreksisa` AS `srreksisa`,`sr`.`srrekbayar` AS `srrekbayar`,`sr`.`sridsq` AS `sridsq`,`sr`.`sridso` AS `sridso`,`sr`.`sridpl` AS `sridpl`,`sr`.`sriddo` AS `sriddo`,`sr`.`sriddr` AS `sriddr`,`sr`.`sridpi` AS `sridpi`,`sr`.`sridsi` AS `sridsi`,`sr`.`sridrnr` AS `sridrnr`,`sr`.`srstatus` AS `srstatus`,`sr`.`srstatussebelumnya` AS `srstatussebelumnya`,`sr`.`srjmlrevisi` AS `srjmlrevisi`,`sr`.`srcetakanke` AS `srcetakanke`,`sr`.`srinputuser` AS `srinputuser`,`sr`.`srinputtgl` AS `srinputtgl`,`sr`.`srmodifikasiuser` AS `srmodifikasiuser`,`sr`.`srmodifikasitgl` AS `srmodifikasitgl`,`sr`.`srposting` AS `srposting`,`sr`.`srpostingtgl` AS `srpostingtgl`,`sr`.`srtutupperiode` AS `srtutupperiode`,`sr`.`srisclose` AS `srisclose`,`br`.`bnama` AS `srcabangnama`,`lc`.`lnama` AS `srlokasinama`,`wh`.`wnama` AS `srgudangnama`,`c1`.`kkode` AS `srcustomerkode`,`c1`.`knama` AS `srcustomernama`,`c2`.`kkode` AS `srbagianpenjualankode`,`c2`.`knama` AS `srbagianpenjualannama`,`e`.`enama` AS `srekspedisinama`,`si`.`sinotransaksi` AS `sinotransaksi`,`rnr`.`rnrnotransaksi` AS `rnrnotransaksi`,`st1`.`nama` AS `srstatusnama`,`st2`.`nama` AS `srstatussebelumnyanama`,`u1`.`unama` AS `srinputusernama`,`u2`.`unama` AS `srmodifikasiusernama`, `sr`.`srcustomtext1` AS `srcustomtext1`, `sr`.`srcustomtext2` AS `srcustomtext2`, `sr`.`srcustomtext3` AS `srcustomtext3`, `sr`.`srcustomtext4` AS `srcustomtext4`, `sr`.`srcustomtext5` AS `srcustomtext5`, `sr`.`srcustomint1` AS `srcustomint1`, `sr`.`srcustomint2` AS `srcustomint2`, `sr`.`srcustomint3` AS `srcustomint3`, `sr`.`srcustomdbl1` AS `srcustomdbl1`, `sr`.`srcustomdbl2` AS `srcustomdbl2`, `sr`.`srcustomdbl3` AS `srcustomdbl3`, `sr`.`srcustomdate1` AS `srcustomdate1`, `sr`.`srcustomdate2` AS `srcustomdate2`, `sr`.`srcustomdate3` AS `srcustomdate3`, cdis.cnama AS srrekdiskonnama, cpa.cnama AS srrekpajak1nama, cpa2.cnama AS srrekpajak2nama, cba.cnama AS srrekbiayalainnama from ((((((((((((`m5_sr` `sr` left join `m1_branch` `br` on((`br`.`bkode` = `sr`.`srcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `sr`.`srlokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `sr`.`srgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `sr`.`srcustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `sr`.`srbagianpenjualan`))) left join `m1_expedition` `e` on((`sr`.`srekspedisi` = `e`.`ekode`))) left join `m5_si` `si` on((`sr`.`sridsi` = `si`.`siid`))) left join `m5_rnr` `rnr` on((`sr`.`sridrnr` = `rnr`.`rnrid`))) left join `m0_status` `st1` on((`st1`.`kode` = `sr`.`srstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `sr`.`srstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `sr`.`srinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `sr`.`srmodifikasiuser`))) LEFT JOIN m1_coa cdis ON cdis.cnomor = sr.srrekdiskon LEFT JOIN m1_coa cpa ON cpa.cnomor = sr.srrekpajak1 LEFT JOIN m1_coa cpa2 ON cpa2.cnomor = sr.srrekpajak2 LEFT JOIN m1_coa cba ON cba.cnomor = sr.srrekbiayalain"

        dt = AmbilData("aplikasi1-M5_Sr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("srid"), 0), sptField,
                     FxDB(dr("srcabang"), ""), sptField,
                     FxDB(dr("srlokasi"), ""), sptField,
                     FxDB(dr("srgudang"), ""), sptField,
                     FxDB(dr("srasalbarang"), ""), sptField,
                     FxDB(dr("srasalbarangkategori"), 0), sptField,
                     FxDB(dr("srjenispenjulan"), ""), sptField,
                     FxDB(dr("srjenispenjualankategori"), 0), sptField,
                     FxDB(dr("srcarabayar"), 0), sptField,
                     FxDB(dr("srsumber"), ""), sptField,
                     FxDB(dr("srautonotransaksi"), 0), sptField,
                     FxDB(dr("srnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgl"), ""), formatTgl), sptField,
                     FxDB(dr("srkodepa"), 0), sptField,
                     FxDB(dr("srcustomer"), 0), sptField,
                     FxDB(dr("srcustomerkontak"), ""), sptField,
                     FxDB(dr("sr1alamat1"), ""), sptField,
                     FxDB(dr("sr1alamat2"), ""), sptField,
                     FxDB(dr("sr1alamat3"), ""), sptField,
                     FxDB(dr("sr2alamat1"), ""), sptField,
                     FxDB(dr("sr2alamat2"), ""), sptField,
                     FxDB(dr("sr2alamat3"), ""), sptField,
                     FxDB(dr("srbagianpenjualan"), 0), sptField,
                     FxDB(dr("srekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("srtermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sruraian"), ""), sptField,
                     FxDB(dr("srcatatan"), ""), sptField,
                     FxDB(dr("srnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("srtglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srtglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("srmatauang"), ""), sptField,
                     FxDB(dr("srkurs"), 0), sptField,
                     FxDB(dr("srhargatermasukpajak"), 0), sptField,
                     FxDB(dr("srtotal"), 0), sptField,
                     FxDB(dr("srdiskonpersen"), ""), sptField,
                     FxDB(dr("srjmldiskon"), 0), sptField,
                     FxDB(dr("srtotalpajak1detail"), 0), sptField,
                     FxDB(dr("srtotalpajak2detail"), 0), sptField,
                     FxDB(dr("srbiayalainpersen"), 0), sptField,
                     FxDB(dr("srbiayalain"), 0), sptField,
                     FxDB(dr("srtotaltransaksi"), 0), sptField,
                     FxDB(dr("srsisatransaksi"), 0), sptField,
                     FxDB(dr("srjmlbayar"), 0), sptField,
                     FxDB(dr("srstatuslunas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtgllunas"), ""), formatTgl), sptField,
                     FxDB(dr("srnofakturpajak"), ""), sptField,
                     FxDB(dr("srsdhbayarpajak"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srtglbayarpajak"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskon"), ""), sptField,
                     FxDB(dr("srrekpajak1"), ""), sptField,
                     FxDB(dr("srrekpajak2"), ""), sptField,
                     FxDB(dr("srrekbiayalain"), ""), sptField,
                     FxDB(dr("srreksisa"), ""), sptField,
                     FxDB(dr("srrekbayar"), ""), sptField,
                     FxDB(dr("sridsq"), 0), sptField,
                     FxDB(dr("sridso"), 0), sptField,
                     FxDB(dr("sridpl"), 0), sptField,
                     FxDB(dr("sriddo"), 0), sptField,
                     FxDB(dr("sriddr"), 0), sptField,
                     FxDB(dr("sridpi"), 0), sptField,
                     FxDB(dr("sridsi"), 0), sptField,
                     FxDB(dr("sridrnr"), 0), sptField,
                     FxDB(dr("srstatus"), 0), sptField,
                     FxDB(dr("srstatussebelumnya"), 0), sptField,
                     FxDB(dr("srjmlrevisi"), 0), sptField,
                     FxDB(dr("srcetakanke"), 0), sptField,
                     FxDB(dr("srinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("srtutupperiode"), 0), sptField,
                     FxDB(dr("srisclose"), 0), sptField,
                     FxDB(dr("srcabangnama"), ""), sptField,
                     FxDB(dr("srlokasinama"), ""), sptField,
                     FxDB(dr("srgudangnama"), ""), sptField,
                     FxDB(dr("srcustomerkode"), ""), sptField,
                     FxDB(dr("srcustomernama"), ""), sptField,
                     FxDB(dr("srbagianpenjualankode"), ""), sptField,
                     FxDB(dr("srbagianpenjualannama"), ""), sptField,
                     FxDB(dr("srekspedisinama"), ""), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     FxDB(dr("rnrnotransaksi"), ""), sptField,
                     FxDB(dr("srstatusnama"), ""), sptField,
                     FxDB(dr("srstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("srinputusernama"), ""), sptField,
                     FxDB(dr("srmodifikasiusernama"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext1"), ""), sptField,
                     FxDB(dr("srcustomtext4"), ""), sptField,
                     FxDB(dr("srcustomtext5"), ""), sptField,
                     FxDB(dr("srcustomint1"), 0), sptField,
                     FxDB(dr("srcustomint2"), 0), sptField,
                     FxDB(dr("srcustomint3"), 0), sptField,
                     FxDB(dr("srcustomdbl1"), 0), sptField,
                     FxDB(dr("srcustomdbl2"), 0), sptField,
                     FxDB(dr("srcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("srcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("srrekdiskonnama"), ""), sptField,
                     FxDB(dr("srrekpajak1nama"), ""), sptField,
                     FxDB(dr("srrekpajak2nama"), ""), sptField,
                     FxDB(dr("srrekbiayalainnama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("srid, srcabang, srlokasi, srgudang, srasalbarang, srasalbarangkategori, srjenispenjulan, srjenispenjualankategori, srcarabayar, srsumber, srautonotransaksi, srnotransaksi, srtgl, srkodepa, srcustomer, srcustomerkontak, sr1alamat1, sr1alamat2, sr1alamat3, sr2alamat1, sr2alamat2, sr2alamat3, srbagianpenjualan, srekspedisi, srtglkirim, srtermin, srtgljatuhtempo, sruraian, srcatatan, srnoref, srtglnoref, srtglpenutupan, srmatauang, srkurs, srhargatermasukpajak, srtotal, srdiskonpersen, srjmldiskon, srtotalpajak1detail, srtotalpajak2detail, srbiayalainpersen, srbiayalain, srtotaltransaksi, srsisatransaksi, srjmlbayar, srstatuslunas, srtgllunas, srnofakturpajak, srsdhbayarpajak, srtglbayarpajak, srrekdiskon, srrekpajak1, srrekpajak2, srrekbiayalain, srreksisa, srrekbayar, sridsq, sridso, sridpl, sriddo, sriddr, sridpi, sridsi, sridrnr, srstatus, srstatussebelumnya, srjmlrevisi, srcetakanke, srinputuser, srinputtgl, srmodifikasiuser, srmodifikasitgl, srposting, srpostingtgl, srtutupperiode, srisclose, srcabangnama, srlokasinama, srgudangnama, srcustomerkode, srcustomernama, srbagianpenjualankode, srbagianpenjualannama, srekspedisinama, sinotransaksi, rnrnotransaksi, srstatusnama, srstatussebelumnyanama, srinputusernama, srmodifikasiusernama, srcustomtext1, srcustomtext2, srcustomtext3, srcustomtext4, srcustomtext5, srcustomint1, srcustomint2, srcustomint3, srcustomdbl1, srcustomdbl2, srcustomdbl3, srcustomdate1, srcustomdate2, srcustomdate3, srrekdiskonnama, srrekpajak1nama, srrekpajak2nama, srrekbiayalainnama"))

        Return wsResult
    End Function

End Class