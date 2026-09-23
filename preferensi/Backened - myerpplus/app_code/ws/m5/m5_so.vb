Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_so
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_SoSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataSalesman(), dataRowSalesman() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim notransaksiPDR As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim Filter As String = "", Sorting As String = ""

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


        'CEK NOREFF ========================================================================
        'CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        'JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        'JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        If Len(Filter) > 0 Then
            sql = "SELECT soid, sonotransaksi FROM m5_so WHERE sonoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("soid")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("sonotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("soid")
                    GoTo selesai
                End If
            End If

        Else
            Dim validKey As RsValidKey
            validKey = ValidateKey(paramSplit(0))
            If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        End If
        'END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 3) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'soid(0) As Integer, socabang(1) As String, solokasi(2) As String, sogudang(3) As String, soasalbarang(4) As String, 
        'soasalbarangkategori(5) As Double, sojenispenjualan(6) As String, sojenispenjualankategori(7) As Double, socarabayar(8) As Integer, sosumber(9) As String, 
        'soautonotransaksi(10) As Integer, sonotransaksi(11) As String, sotgl(12) As Date, sokodepa(13) As Integer, socustomer(14) As Integer, 
        'socustomerkontak(15) As String, so1alamat1(16) As String, so1alamat2(17) As String, so1alamat3(18) As String, so2alamat1(19) As String, 
        'so2alamat2(20) As String, so2alamat3(21) As String, sobagianpenjualan(22) As Integer, soekspedisi(23) As String, sotglkirim(24) As Date, 
        'sotermin(25) As String, sotgljatuhtempo(26) As Date, souraian(27) As String, socatatan(28) As String, sonoref(29) As String, 
        'sotglnoref(30) As Date, sotglpenutupan(31) As Date, somatauang(32) As String, sokurs(33) As Double, sohargatermasukpajak(34) As Integer, 
        'sototal(35) As Double, sodiskonpersen(36) As String, sojmldiskon(37) As Double, sototalpajak1detail(38) As Double, sototalpajak2detail(39) As Double, 
        'sobiayalainpersen(40) As Double, sobiayalain(41) As Double, sototaltransaksi(42) As Double, sojmlbayar(43) As Double, sorekdiskon(44) As String, 
        'sorekpajak1(45) As String, sorekpajak2(46) As String, sorekbiayalain(47) As String, sorekbayar(48) As String, soidsq(49) As Integer, 
        'sostatuspl(50) As Integer, sostatusdo(51) As Integer, sostatusdr(52) As Integer, sostatuspi(53) As Integer, sostatussi(54) As Integer, 
        'sostatusrnr(55) As Integer, sostatussr(56) As Integer, sostatus(57) As Integer, sostatussebelumnya(58) As Integer, sojmlrevisi(59) As Integer, 
        'socetakanke(60) As Integer, soinputuser(61) As Integer, soinputtgl(62) As DateTime, somodifikasiuser(63) As Integer, somodifikasitgl(64) As DateTime, 
        'soisclose(65) As Integer, socustomtext1(66) As String, socustomtext2(67) As String, socustomtext3(68) As String, socustomtext4(69) As String, 
        'socustomtext5(70) As String, socustomint1(71) As Integer, socustomint2(72) As Integer, socustomint3(73) As Integer, socustomdbl1(74) As Double, 
        'socustomdbl2(75) As Double, socustomdbl3(76) As Double, socustomdate1(77) As Date, socustomdate2(78) As Date, socustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, 
        'somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, 
        'socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, 
        'socustomdate1, socustomdate2, socustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'soid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "soid required numeric." : GoTo selesai
        End If
        'soasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "soasalbarangkategori required numeric." : GoTo selesai
        End If
        'sojenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sojenispenjualankategori required numeric." : GoTo selesai
        End If
        'socarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "socarabayar required numeric." : GoTo selesai
        End If
        'soautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "soautonotransaksi required numeric." : GoTo selesai
        End If
        'sotgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sotgl required date." : GoTo selesai
        End If
        'sokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sokodepa required numeric." : GoTo selesai
        End If
        'socustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "socustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "socustomer can't be empty." : GoTo selesai
        End If
        'sobagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sobagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sobagianpenjualan can't be empty." : GoTo selesai
        End If
        'sotglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "sotglkirim required date." : GoTo selesai
        End If
        'sotgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "sotgljatuhtempo required date." : GoTo selesai
        End If
        'sotglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sotglnoref required date." : GoTo selesai
        End If
        'sotglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "sotglpenutupan required date." : GoTo selesai
        End If
        'sokurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sokurs required numeric." : GoTo selesai
        End If
        'sohargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sohargatermasukpajak required numeric." : GoTo selesai
        End If
        'sototal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sototal required numeric." : GoTo selesai
        End If
        'sojmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sojmldiskon required numeric." : GoTo selesai
        End If
        'sototalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sototalpajak1detail required numeric." : GoTo selesai
        End If
        'sototalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "sototalpajak2detail required numeric." : GoTo selesai
        End If
        ''sobiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "sobiayalainpersen required numeric." : GoTo selesai
        'End If
        'sobiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sobiayalain required numeric." : GoTo selesai
        End If
        'sototaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sototaltransaksi required numeric." : GoTo selesai
        End If
        'sojmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sojmlbayar required numeric." : GoTo selesai
        End If
        'soidsq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "soidsq required numeric." : GoTo selesai
        End If
        'sostatuspl(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sostatuspl required numeric." : GoTo selesai
        End If
        'sostatusdo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sostatusdo required numeric." : GoTo selesai
        End If
        'sostatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sostatusdr required numeric." : GoTo selesai
        End If
        'sostatuspi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sostatuspi required numeric." : GoTo selesai
        End If
        'sostatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sostatussi required numeric." : GoTo selesai
        End If
        'sostatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sostatusrnr required numeric." : GoTo selesai
        End If
        'sostatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "sostatussr required numeric." : GoTo selesai
        End If
        'sostatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sostatus required numeric." : GoTo selesai
        End If
        'sostatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "sostatussebelumnya required numeric." : GoTo selesai
        End If
        'sojmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sojmlrevisi required numeric." : GoTo selesai
        End If
        'socetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "socetakanke required numeric." : GoTo selesai
        End If
        'soinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "soinputuser required numeric." : GoTo selesai
        End If
        'soinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "soinputtgl required date." : GoTo selesai
        End If
        'somodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "somodifikasiuser required numeric." : GoTo selesai
        End If
        'somodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "somodifikasitgl required date." : GoTo selesai
        End If
        'soisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "soisclose required numeric." : GoTo selesai
        End If
        'socustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "socustomint1 required numeric." : GoTo selesai
        End If
        'socustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "socustomint2 required numeric." : GoTo selesai
        End If
        'socustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "socustomint3 required numeric." : GoTo selesai
        End If
        'socustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "socustomdbl1 required numeric." : GoTo selesai
        End If
        'socustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "socustomdbl2 required numeric." : GoTo selesai
        End If
        'socustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "socustomdbl3 required numeric." : GoTo selesai
        End If
        'socustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "socustomdate1 required date." : GoTo selesai
        End If
        'socustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "socustomdate2 required date." : GoTo selesai
        End If
        'socustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "socustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'socabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "socabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "socabang should not be more than 25 character." : GoTo selesai
        End If

        'solokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "solokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "solokasi should not be more than 25 character." : GoTo selesai
        End If

        'sogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sogudang should not be more than 25 character." : GoTo selesai
        End If

        'sosumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sosumber should not be more than 10 character." : GoTo selesai
        End If

        'sonotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sotgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sotgl can't be empty" : GoTo selesai
        End If

        'sotglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "sotglkirim can't be empty" : GoTo selesai
        End If

        'sotgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "sotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sotglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sotglnoref can't be empty" : GoTo selesai
        End If

        'sotglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sotglpenutupan can't be empty" : GoTo selesai
        End If

        'somatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "somatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "somatauang should not be more than 25 character." : GoTo selesai
        End If

        'sokurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "sokurs can't be empty" : GoTo selesai
        End If

        'sototal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sototal can't be empty" : GoTo selesai
        End If

        'sodiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "sodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'sojmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sojmldiskon can't be empty" : GoTo selesai
        End If

        'sototalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sototalpajak1detail can't be empty" : GoTo selesai
        End If

        'sototalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sototalpajak2detail can't be empty" : GoTo selesai
        End If

        'sobiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "sobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sobiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sobiayalain can't be empty" : GoTo selesai
        End If

        'sototaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "sototaltransaksi can't be empty" : GoTo selesai
        End If

        'sojmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "sojmlbayar can't be empty" : GoTo selesai
        End If

        'soinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "soinputtgl can't be empty" : GoTo selesai
        End If

        'somodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "somodifikasitgl can't be empty" : GoTo selesai
        End If

        'socustomtext1(66) As String
        'CUSTOM TEXT DIISI DARI SOCATATAN(28) + SOTGLKIRIM(24)
        'dataUtama(66) = String.Concat(dataUtama(28), Replace(dataUtama(24), "-", ""))

        'socustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "socustomdbl1 can't be empty" : GoTo selesai
        End If

        'socustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "socustomdbl2 can't be empty" : GoTo selesai
        End If

        'socustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "socustomdbl3 can't be empty" : GoTo selesai
        End If

        'socustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "socustomdate1 can't be empty" : GoTo selesai
        End If

        'socustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "socustomdate2 can't be empty" : GoTo selesai
        End If

        'socustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "socustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "soid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "solokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarangkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojenispenjualankategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "souraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "somodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "soid~socabang~solokasi~sogudang~soasalbarang~soasalbarangkategori~sojenispenjualan~sojenispenjualankategori~socarabayar~sosumber~soautonotransaksi~sonotransaksi~sotgl~sokodepa~socustomer~socustomerkontak~so1alamat1~so1alamat2~so1alamat3~so2alamat1~so2alamat2~so2alamat3~sobagianpenjualan~soekspedisi~sotglkirim~sotermin~sotgljatuhtempo~souraian~socatatan~sonoref~sotglnoref~sotglpenutupan~somatauang~sokurs~sohargatermasukpajak~sototal~sodiskonpersen~sojmldiskon~sototalpajak1detail~sototalpajak2detail~sobiayalainpersen~sobiayalain~sototaltransaksi~sojmlbayar~sorekdiskon~sorekpajak1~sorekpajak2~sorekbiayalain~sorekbayar~soidsq~sostatuspl~sostatusdo~sostatusdr~sostatuspi~sostatussi~sostatusrnr~sostatussr~sostatus~sostatussebelumnya~sojmlrevisi~socetakanke~soinputuser~soinputtgl~somodifikasiuser~somodifikasitgl~soisclose~socustomtext1~socustomtext2~socustomtext3~socustomtext4~socustomtext5~socustomint1~socustomint2~socustomint3~socustomdbl1~socustomdbl2~socustomdbl3~socustomdate1~socustomdate2~socustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsodetail(0) As Integer, idso(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idsqdetail(28) As Integer, jmlpl(29) As Double, 
        'statuspl(30) As Integer, jmldo(31) As Double, statusdo(32) As Integer, jmldr(33) As Double, statusdr(34) As Integer, 
        'jmlpi(35) As Double, statuspi(36) As Integer, jmlsi(37) As Double, statussi(38) As Integer, jmlrnr(39) As Double, 
        'statusrnr(40) As Integer, jmlsr(41) As Double, statussr(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idso", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrnr", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "customdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customtext5", AsEnumTypeData.AsString)

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokBooking As String = ""
        Dim idbarang As Integer = 0, idsqdetail As Integer = 0, jmlbarang As Double = 0

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSQ As String = ""

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
            'idsodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idso(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idso required numeric." : GoTo selesai
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
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlpl(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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
            'customdbl4(53) As Double
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - bundle/pallet required numeric." : GoTo selesai
            End If
            'customdbl4(54) As Double
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - gross weight required numeric." : GoTo selesai
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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(12) As Double, diskon(13) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlpl(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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
            If dataRowDetail(49) <= 0 Then
                result(2) = "Row : " & i & " - Max Qty RC can't be less than or equal to zero" : GoTo selesai
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
            'customdbl4(53) As Double
            If (Len(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - qty bundle/pallet can't be empty." : GoTo selesai
            End If
            'customdbl4(54) As Double
            If (Len(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - gross weight required be empty." : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idsodetail~idso~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~customdbl4~customdbl5~customtext4~customtext5", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(21) As String       , idsqdetail(28) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(21) : idsqdetail = dataRowDetail(28)
            'kurs(11) As Double                    , harga(12) As Double
            kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            If idsqdetail <> 0 Then 'SQ
                'CEK SQ YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSQ = IIf(Len(ftSQ.ToString) = 0, "", ftSQ & " OR ")
                ftSQ = String.Concat(ftSQ, " (sqd.idsqdetail = " & idsqdetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                'ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND (CASE sqd.jmlbarang WHEN 0 THEN sqd.jmlbarang < 0 ELSE " & Outstanding & " > ROUND(sqd.jmlbarang - sqd.jmlrealisasi + (sqd.jmlbarang * s.snilai), 5) END)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            End If

            ''5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            'Validasi harga dibawah harga jual
            ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idsocost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idso", AsEnumTypeData.AsInt64)
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

        'CEK PARAMETER DATA ASSET

        If dataSplit.Length > 2 Then
            If dataSplit(2).Length > 0 Then

                'VALIDASI DAN SET DATA COST ======================================================
                'SPLIT PARAMETER DATA COST
                dataSalesman = dataSplit(2).Split(sptRow)
                'END OF VALIDASI DAN SET DATA COST ===============================================


                'VALIDASI DAN SET DATA ROW COST ==================================================
                Dim JmlDtCost As Integer = dataSalesman.Length
                For i = 1 To JmlDtCost
                    'SPLIT DATA COST
                    dataRowSalesman = dataSalesman(i - 1).Split(sptField)

                    'VALIDASI DAN SET ROW DATA COST -----------------------------------
                    'CEK ARRAY DATA COST
                    If (dataRowSalesman.Length <> 35) Then
                        result(2) = "Cost Row : " & i & " -  Invalid cost transaction data parameter." : GoTo selesai
                    End If
                    'END OF VALIDASI DAN SET DATA ROW COST ----------------------------

                    'VALIDASI TIPE DATA COST ------------------------------------------
                    'iddocost(0) As Integer
                    If (IsNumeric(dataRowSalesman(0)) = False) Then
                        result(2) = "Cost Row : " & i & " - iddocost required numeric." : GoTo selesai
                    End If
                    'iddo(1) As Integer
                    If (IsNumeric(dataRowSalesman(1)) = False) Then
                        result(2) = "Cost Row : " & i & " - iddo required numeric." : GoTo selesai
                    End If
                    'kurs(4) As Double
                    If (IsNumeric(dataRowSalesman(4)) = False) Then
                        result(2) = "Cost Row : " & i & " - kurs required numeric." : GoTo selesai
                    End If
                    'jumlah(5) As Double
                    If (IsNumeric(dataRowSalesman(5)) = False) Then
                        result(2) = "Cost Row : " & i & " - jumlah required numeric." : GoTo selesai
                    End If
                    'urutan(11) As Integer
                    If (IsNumeric(dataRowSalesman(11)) = False) Then
                        result(2) = "Cost Row : " & i & " - urutan required numeric." : GoTo selesai
                    End If
                    'idprcost(12) As Integer
                    If (IsNumeric(dataRowSalesman(12)) = False) Then
                        result(2) = "Cost Row : " & i & " - idprcost required numeric." : GoTo selesai
                    End If
                    'idcscost(13) As Integer
                    If (IsNumeric(dataRowSalesman(13)) = False) Then
                        result(2) = "Cost Row : " & i & " - idcscost required numeric." : GoTo selesai
                    End If
                    'idrqcost(14) As Integer
                    If (IsNumeric(dataRowSalesman(14)) = False) Then
                        result(2) = "Cost Row : " & i & " - idrqcost required numeric." : GoTo selesai
                    End If
                    'idbscost(15) As Integer
                    If (IsNumeric(dataRowSalesman(15)) = False) Then
                        result(2) = "Cost Row : " & i & " - idbscost required numeric." : GoTo selesai
                    End If
                    'idpocost(16) As Integer
                    If (IsNumeric(dataRowSalesman(16)) = False) Then
                        result(2) = "Cost Row : " & i & " - idpocost required numeric." : GoTo selesai
                    End If
                    'idipccost(17) As Integer
                    If (IsNumeric(dataRowSalesman(17)) = False) Then
                        result(2) = "Cost Row : " & i & " - idipccost required numeric." : GoTo selesai
                    End If
                    'idgrncost(18) As Integer
                    If (IsNumeric(dataRowSalesman(18)) = False) Then
                        result(2) = "Cost Row : " & i & " - idgrncost required numeric." : GoTo selesai
                    End If
                    'jumlahbayar(19) As Double
                    If (IsNumeric(dataRowSalesman(19)) = False) Then
                        result(2) = "Cost Row : " & i & " - jumlahbayar required numeric." : GoTo selesai
                    End If
                    'statusbayar(20) As Integer
                    If (IsNumeric(dataRowSalesman(20)) = False) Then
                        result(2) = "Cost Row : " & i & " - statusbayar required numeric." : GoTo selesai
                    End If
                    'isclose(21) As Integer
                    If (IsNumeric(dataRowSalesman(21)) = False) Then
                        result(2) = "Cost Row : " & i & " - isclose required numeric." : GoTo selesai
                    End If
                    'customdbl1(25) As Double
                    If (IsNumeric(dataRowSalesman(25)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdbl1 required numeric." : GoTo selesai
                    End If
                    'customdbl2(26) As Double
                    If (IsNumeric(dataRowSalesman(26)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdbl2 required numeric." : GoTo selesai
                    End If
                    'customdbl3(27) As Double
                    If (IsNumeric(dataRowSalesman(27)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdbl3 required numeric." : GoTo selesai
                    End If
                    'customdate1(28) As Date
                    If (IsDate(dataRowSalesman(28)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdate1 required date." : GoTo selesai
                    End If
                    'customdate2(29) As Date
                    If (IsDate(dataRowSalesman(29)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdate2 required date." : GoTo selesai
                    End If
                    'customdate3(30) As Date
                    If (IsDate(dataRowSalesman(30)) = False) Then
                        result(2) = "Cost Row : " & i & " - customdate3 required date." : GoTo selesai
                    End If
                    'kontak(33) As Integer
                    If (IsNumeric(dataRowSalesman(33)) = False) Then
                        result(2) = "Cost Row : " & i & " - kontak required numeric." : GoTo selesai
                    End If
                    'termasukhpp(34) As Integer
                    If (IsNumeric(dataRowSalesman(34)) = False) Then
                        result(2) = "Cost Row : " & i & " - termasukhpp required numeric." : GoTo selesai
                    End If
                    'END OF VALIDASI TIPE DATA COST -----------------------------------

                    'VALIDASI DATA COST ---------------------------------------
                    'kodecost(2) As String
                    If Len(dataRowSalesman(2)) = 0 Then
                        result(2) = "Cost Row : " & i & " - kodecost can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowSalesman(2)) > 25 Then
                        result(2) = "Cost Row : " & i & " - kodecost should not be more than 25 character." : GoTo selesai
                    End If

                    'matauang(3) As String
                    If Len(dataRowSalesman(3)) = 0 Then
                        result(2) = "Cost Row : " & i & " - matauang can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowSalesman(3)) > 25 Then
                        result(2) = "Cost Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
                    End If

                    'kurs(4) As Double
                    If Len(dataRowSalesman(4)) = 0 Then
                        result(2) = "Cost Row : " & i & " - kurs can't be empty" : GoTo selesai
                    End If

                    'jumlah(5) As Double
                    If Len(dataRowSalesman(5)) = 0 Then
                        result(2) = "Cost Row : " & i & " - jumlah can't be empty" : GoTo selesai
                    End If

                    'jumlahbayar(19) As Double
                    If Len(dataRowSalesman(19)) = 0 Then
                        result(2) = "Cost Row : " & i & " - jumlahbayar can't be empty" : GoTo selesai
                    End If

                    'customdbl1(25) As Double
                    If Len(dataRowSalesman(25)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
                    End If

                    'customdbl2(26) As Double
                    If Len(dataRowSalesman(26)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
                    End If

                    'customdbl3(27) As Double
                    If Len(dataRowSalesman(27)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
                    End If

                    'customdate1(28) As Date
                    If Len(dataRowSalesman(28)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdate1 can't be empty" : GoTo selesai
                    End If

                    'customdate2(29) As Date
                    If Len(dataRowSalesman(29)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdate2 can't be empty" : GoTo selesai
                    End If

                    'customdate3(30) As Date
                    If Len(dataRowSalesman(30)) = 0 Then
                        result(2) = "Cost Row : " & i & " - customdate3 can't be empty" : GoTo selesai
                    End If

                    'rekdebit(31) As String
                    If dataRowSalesman(34) = 0 Then
                        If Len(dataRowSalesman(31)) = 0 Then
                            result(2) = "Cost Row : " & i & " - rekdebit can't be empty" : GoTo selesai
                        End If
                    End If
                    If Len(dataRowSalesman(31)) > 25 Then
                        result(2) = "Cost Row : " & i & " - rekdebit should not be more than 25 character." : GoTo selesai
                    End If

                    'rekkredit(32) As String
                    If Len(dataRowSalesman(32)) = 0 Then
                        result(2) = "Cost Row : " & i & " - rekkredit can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowSalesman(32)) > 25 Then
                        result(2) = "Cost Row : " & i & " - rekkredit should not be more than 25 character." : GoTo selesai
                    End If
                    'END OF VALIDASI DATA COST --------------------------------

                    If AsDataTableTambahData(dtcost, "idsocost~idso~kodecost~matauang~kurs~jumlah~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~idpocost~idipccost~idgrncost~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3~rekdebit~rekkredit~kontak~termasukhpp", dataRowSalesman(0) & "~" & dataRowSalesman(1) & "~" & dataRowSalesman(2) & "~" & dataRowSalesman(3) & "~" & dataRowSalesman(4) & "~" & dataRowSalesman(5) & "~" & dataRowSalesman(6) & "~" & dataRowSalesman(7) & "~" & dataRowSalesman(8) & "~" & dataRowSalesman(9) & "~" & dataRowSalesman(10) & "~" & dataRowSalesman(11) & "~" & dataRowSalesman(12) & "~" & dataRowSalesman(13) & "~" & dataRowSalesman(14) & "~" & dataRowSalesman(15) & "~" & dataRowSalesman(16) & "~" & dataRowSalesman(17) & "~" & dataRowSalesman(18) & "~" & dataRowSalesman(19) & "~" & dataRowSalesman(20) & "~" & dataRowSalesman(21) & "~" & dataRowSalesman(22) & "~" & dataRowSalesman(23) & "~" & dataRowSalesman(24) & "~" & dataRowSalesman(25) & "~" & dataRowSalesman(26) & "~" & dataRowSalesman(27) & "~" & dataRowSalesman(28) & "~" & dataRowSalesman(29) & "~" & dataRowSalesman(30) & "~" & dataRowSalesman(31) & "~" & dataRowSalesman(32) & "~" & dataRowSalesman(33) & "~" & dataRowSalesman(34)) = False Then
                        result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                    End If

                Next
                'END OF VALIDASI DAN SET ROW DATA COST ===========================================

            End If
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 4
                Select Case drutama("sostatus")
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


                'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                Dim MUFungsional As String = "", MUUtama As String = ""
                'Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                End If

                'SET MATA UANG UTAMA
                MUUtama = drutama("somatauang")
                'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sotgl")), AsFormatTanggal(drutama("sotgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sostatus") = 2 Or drutama("sostatus") = 1 Or drutama("sostatus") = 8 Or drutama("sostatus") = 9 Or drutama("sostatus") = 10 Or drutama("sostatus") = 11 Then

                    'VALIDASI PLAFON PIUTANG -------------
                    Dim dtHACustom As DataTable = AsDataTableAmbilDariDBCon("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 5 AND pc.pcid = 4 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1", myConn)
                    If dtHACustom.Rows.Count > 0 Then
                        If dtHACustom.Rows(0)("rcakses") = 0 Then
                            GoTo validasiPlafon
                        End If

                    Else
validasiPlafon:
                        Dim dtPlafonP As DataTable = AsDataTableAmbilDariDBCon("SELECT c.kbataspiutang, c.ktotalpiutang FROM m0_setting s JOIN m1_contact c ON c.kid = '" & drutama("socustomer") & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSO' AND s.snilai = 1", myConn)
                        result(2) = dtPlafonP.Rows.Count.ToString : GoTo selesai
                        If dtPlafonP.Rows.Count > 0 Then
                            'JIKA BATAS PIUTANG > 0
                            If Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) > 0 Then
                                'JIKA TOTAL PIUTANG + TOTAL TRANSAKSI FUNGSIONAL > BATAS PIUTANG
                                If Double.Parse(dtPlafonP.Rows(0)("ktotalpiutang")) + (Double.Parse(drutama("sototaltransaksi")) * Double.Parse(drutama("sokurs"))) > Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) Then
                                    Dim selisih(2) As String
                                    selisih = F_Nominal(Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) - Double.Parse(dtPlafonP.Rows(0)("ktotalpiutang")), True).Split(sptSubParam)
                                    result(2) = "Total Transaction exceeds the limit of AR. (AR limit available " & MUFungsional & " " & selisih(1) & ")" : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If

                    End If
                    'END OF VALIDASI PLAFON PIUTANG ------

                    'VALIDASI BACKDATE
                    Dim dtHABackDate As DataTable = AsDataTableAmbilDariDBCon("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 0 AND pc.pcid = 3 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1", myConn)
                    If dtHABackDate.Rows.Count > 0 Then
                        If dtHABackDate.Rows(0)("rcakses") = 0 Then
                            GoTo validasiBackDate
                        End If

                    Else

validasiBackDate:
                        'result(2) = drutama("satgl") & " - " & AsFormatTanggal(Date.Now) : Trans.Rollback() : GoTo selesai

                        If DateDiff(DateInterval.Day, Date.Now, Date.Parse(drutama("sotgl"))) < 0 Then
                            result(2) = "This user can't to insert the back date transaction" : Trans.Rollback() : GoTo selesai

                        End If
                    End If
                    'END OF VALIDASI BACKDATE


                    'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                    Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                    If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftSQ, drutama("sohargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sotermin").ToString, AsFormatTanggal(drutama("sotgl")), "sotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak1detail")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("soid")
                    notransaksi = drutama("sonotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(soid), sonotransaksi FROM M5_so WHERE soid='" & result(4) & "' AND sostatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("soautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"), drutama("sosumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_so_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sosumber")) & "▼" & FixQuotes(drutama("soid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_So set socabang  = '" & FixQuotes(drutama("socabang")) & "', solokasi  = '" & FixQuotes(drutama("solokasi")) & "', sogudang  = '" & FixQuotes(drutama("sogudang")) & "', soasalbarang  = '" & FixQuotes(drutama("soasalbarang")) & "', soasalbarangkategori  = " & drutama("soasalbarangkategori") & ", sojenispenjualan  = '" & FixQuotes(drutama("sojenispenjualan")) & "', sojenispenjualankategori  = " & drutama("sojenispenjualankategori") & ", socarabayar  = " & drutama("socarabayar") & ", sosumber  = '" & FixQuotes(drutama("sosumber")) & "', soautonotransaksi  = " & drutama("soautonotransaksi") & ", sonotransaksi  = '" & FixQuotes(notransaksi) & "', sotgl  = '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', sokodepa  = " & drutama("sokodepa") & ", socustomer  = " & drutama("socustomer") & ", socustomerkontak  = '" & FixQuotes(drutama("socustomerkontak")) & "', so1alamat1  = '" & FixQuotes(drutama("so1alamat1")) & "', so1alamat2  = '" & FixQuotes(drutama("so1alamat2")) & "', so1alamat3  = '" & FixQuotes(drutama("so1alamat3")) & "', so2alamat1  = '" & FixQuotes(drutama("so2alamat1")) & "', so2alamat2  = '" & FixQuotes(drutama("so2alamat2")) & "', so2alamat3  = '" & FixQuotes(drutama("so2alamat3")) & "', sobagianpenjualan  = " & drutama("sobagianpenjualan") & ", soekspedisi  = '" & FixQuotes(drutama("soekspedisi")) & "', sotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', sotermin  = '" & FixQuotes(drutama("sotermin")) & "', sotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', souraian  = '" & FixQuotes(drutama("souraian")) & "', socatatan  = '" & FixQuotes(drutama("socatatan")) & "', sonoref  = '" & FixQuotes(drutama("sonoref")) & "', sotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', sotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', somatauang  = '" & FixQuotes(drutama("somatauang")) & "', sokurs  = '" & FixDouble(drutama("sokurs")) & "', sohargatermasukpajak  = " & drutama("sohargatermasukpajak") & ", sototal  = '" & FixDouble(drutama("sototal")) & "', sodiskonpersen  = '" & FixQuotes(drutama("sodiskonpersen")) & "', sojmldiskon  = '" & FixDouble(drutama("sojmldiskon")) & "', sototalpajak1detail  = '" & FixDouble(drutama("sototalpajak1detail")) & "', sototalpajak2detail  = '" & FixDouble(drutama("sototalpajak2detail")) & "', sobiayalainpersen  = '" & FixDouble(drutama("sobiayalainpersen")) & "', sobiayalain  = '" & FixDouble(drutama("sobiayalain")) & "', sototaltransaksi  = '" & FixDouble(drutama("sototaltransaksi")) & "', sojmlbayar  = '" & FixDouble(drutama("sojmlbayar")) & "', sorekdiskon  = '" & FixQuotes(drutama("sorekdiskon")) & "', sorekpajak1  = '" & FixQuotes(drutama("sorekpajak1")) & "', sorekpajak2  = '" & FixQuotes(drutama("sorekpajak2")) & "', sorekbiayalain  = '" & FixQuotes(drutama("sorekbiayalain")) & "', sorekbayar  = '" & FixQuotes(drutama("sorekbayar")) & "', soidsq  = " & drutama("soidsq") & ", sostatuspl  = " & drutama("sostatuspl") & ", sostatusdo  = " & drutama("sostatusdo") & ", sostatusdr  = " & drutama("sostatusdr") & ", sostatuspi  = " & drutama("sostatuspi") & ", sostatussi  = " & drutama("sostatussi") & ", sostatusrnr  = " & drutama("sostatusrnr") & ", sostatussr  = " & drutama("sostatussr") & ", sostatus  = " & drutama("sostatus") & ", sostatussebelumnya  = " & drutama("sostatussebelumnya") & ", sojmlrevisi  = sojmlrevisi+1, socetakanke  = " & drutama("socetakanke") & ", somodifikasiuser  = " & drutama("somodifikasiuser") & ", somodifikasitgl  = NOW(), socustomtext1  = '" & FixQuotes(drutama("socustomtext1")) & "', socustomtext2  = '" & FixQuotes(drutama("socustomtext2")) & "', socustomtext3  = '" & FixQuotes(drutama("socustomtext3")) & "', socustomtext4  = '" & FixQuotes(drutama("socustomtext4")) & "', socustomtext5  = '" & FixQuotes(drutama("socustomtext5")) & "', socustomint1  = " & drutama("socustomint1") & ", socustomint2  = " & drutama("socustomint2") & ", socustomint3  = " & drutama("socustomint3") & ", socustomdbl1  = '" & FixDouble(drutama("socustomdbl1")) & "', socustomdbl2  = '" & FixDouble(drutama("socustomdbl2")) & "', socustomdbl3  = '" & FixDouble(drutama("socustomdbl3")) & "', socustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', socustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', socustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "' where soid = '" & drutama("soid") & "'"
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

                    If drutama("soautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"), drutama("sosumber"), 5)
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
                        notransaksi = drutama("sonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_So (socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3) values('" & FixQuotes(drutama("socabang")) & "', '" & FixQuotes(drutama("solokasi")) & "', '" & FixQuotes(drutama("sogudang")) & "', '" & FixQuotes(drutama("soasalbarang")) & "', " & drutama("soasalbarangkategori") & ", '" & FixQuotes(drutama("sojenispenjualan")) & "', " & drutama("sojenispenjualankategori") & ", " & drutama("socarabayar") & ", '" & FixQuotes(drutama("sosumber")) & "', " & drutama("soautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', " & drutama("sokodepa") & ", " & drutama("socustomer") & ", '" & FixQuotes(drutama("socustomerkontak")) & "', '" & FixQuotes(drutama("so1alamat1")) & "', '" & FixQuotes(drutama("so1alamat2")) & "', '" & FixQuotes(drutama("so1alamat3")) & "', '" & FixQuotes(drutama("so2alamat1")) & "', '" & FixQuotes(drutama("so2alamat2")) & "', '" & FixQuotes(drutama("so2alamat3")) & "', " & drutama("sobagianpenjualan") & ", '" & FixQuotes(drutama("soekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', '" & FixQuotes(drutama("sotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', '" & FixQuotes(drutama("souraian")) & "', '" & FixQuotes(drutama("socatatan")) & "', '" & FixQuotes(drutama("sonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', '" & FixQuotes(drutama("somatauang")) & "', '" & FixDouble(drutama("sokurs")) & "', " & drutama("sohargatermasukpajak") & ", '" & FixDouble(drutama("sototal")) & "', '" & FixQuotes(drutama("sodiskonpersen")) & "', '" & FixDouble(drutama("sojmldiskon")) & "', '" & FixDouble(drutama("sototalpajak1detail")) & "', '" & FixDouble(drutama("sototalpajak2detail")) & "', '" & FixDouble(drutama("sobiayalainpersen")) & "', '" & FixDouble(drutama("sobiayalain")) & "', '" & FixDouble(drutama("sototaltransaksi")) & "', '" & FixDouble(drutama("sojmlbayar")) & "', '" & FixQuotes(drutama("sorekdiskon")) & "', '" & FixQuotes(drutama("sorekpajak1")) & "', '" & FixQuotes(drutama("sorekpajak2")) & "', '" & FixQuotes(drutama("sorekbiayalain")) & "', '" & FixQuotes(drutama("sorekbayar")) & "', " & drutama("soidsq") & ", " & drutama("sostatuspl") & ", " & drutama("sostatusdo") & ", " & drutama("sostatusdr") & ", " & drutama("sostatuspi") & ", " & drutama("sostatussi") & ", " & drutama("sostatusrnr") & ", " & drutama("sostatussr") & ", " & drutama("sostatus") & ", " & drutama("sostatussebelumnya") & ", " & drutama("sojmlrevisi") & ", " & drutama("socetakanke") & ", " & drutama("soinputuser") & ", NOW(), " & drutama("somodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("soisclose") & ", '" & FixQuotes(drutama("socustomtext1")) & "', '" & FixQuotes(drutama("socustomtext2")) & "', '" & FixQuotes(drutama("socustomtext3")) & "', '" & FixQuotes(drutama("socustomtext4")) & "', '" & FixQuotes(drutama("socustomtext5")) & "', " & drutama("socustomint1") & ", " & drutama("socustomint2") & ", " & drutama("socustomint3") & ", '" & FixDouble(drutama("socustomdbl1")) & "', '" & FixDouble(drutama("socustomdbl2")) & "', '" & FixDouble(drutama("socustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select soid from M5_so where sonotransaksi='" & notransaksi & "' AND soinputuser= '" & userid & "' order by somodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_So_Detail where idso = '" & result(4) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Dim ftSOProd As String = ""

                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows

                        'FILTER IDBARANG
                        ftSOProd = IIf(Len(ftSOProd.ToString) = 0, "", ftSOProd & " , ")
                        ftSOProd = String.Concat(ftSOProd, "'" & dr1("idbarang") & "'")

                        Dim wsM0_NomorPDR As New m0_nomor
                        Dim rsNotransaksiPDR As String = wsM0_NomorPDR.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), "PDR", drutama("sotgl"))
                        Dim arrNotransaksiPDR(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        arrNotransaksiPDR = rsNotransaksiPDR.Split(sptSubParam)
                        'cek success generate notransaksi
                        If (arrNotransaksiPDR(0) = 1) Then
                            notransaksiPDR = arrNotransaksiPDR(2)
                            'tambah query update m0_nomor_next
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = arrNotransaksiPDR(3)
                            End With
                            objCmd.ExecuteNonQuery()
                        Else
                            result(2) = arrNotransaksiPDR(1) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF GENERATE NOTRANSAKSI ==================================

                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idsodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(notransaksiPDR) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "')")
                    Next
                    sql = "Insert into M5_So_Detail(idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, customdbl4, customdbl5, customtext4, customtext5) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                Else
                    result(2) = "Detail Transaction data not found. - " & dtdetail.Rows.Count : Trans.Rollback() : GoTo selesai
                End If

                'Hapus cost ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_So_Cost where idso = " & result(4)
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
                        strValue2.Append("(" & dr1("idsocost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", " & dr1("idpocost") & ", " & dr1("idipccost") & ", " & dr1("idgrncost") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', '" & FixQuotes(dr1("kontak")) & "', '" & FixQuotes(dr1("termasukhpp")) & "')")
                    Next
                    sql = "Insert into M5_So_Cost(idsocost, idso, kodecost, matauang, kurs, jumlah, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, rekdebit, rekkredit, kontak, termasukhpp) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                'HAPUS SO PRODUCTION
                If Len(ftSOProd) > 0 Then
                    sql = "Delete from m5_so_production where idso = " & result(4) & " AND idbarang NOT IN (" & ftSOProd & ")"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'INSERT/UPDATE SO PRODUCTION
                sql = "  INSERT INTO m5_so_production( SELECT sod.idso, sod.idbarang, so.sonotransaksi as nomorso, so.sotgl as tglso, so.sotglkirim as tglkirimso, sod.namabarang, sod.tipebarang, SUM(sod.jml) as jml, sod.satuan, sod.nilaisatuan, SUM(sod.jmlbarang) as jmlbarang, sod.satuanbarang, sod.matauang, sod.kurs, sod.harga, sod.diskon, sod.jmldiskon, sod.pajak1, sod.jmlpajak1, sod.pajak2, sod.jmlpajak2, sod.cabang, sod.lokasi, sod.gudang, sod.costcenter, sod.divisi, sod.subdivisi, sod.proyek, sod.catatan, sod.urutan, sod.idsqdetail, (i.bcustom19 / 100) * SUM(sod.jmlbarang) as jmlpi, i.bcustom19 as statuspi, 0 as jmlpl, 0 as statuspl, 0 as jmldo, 0 as statusdo, 0 as jmldr, 0 as statusdr, 0 as jmlsi, 0 as statussi, sod.jmlrnr as jmlrnr, 0 as statusrnr, 0 as jmlsr, 0 as statussr, 0 as jmlrealisasi, 0 as statusrealisasi, sod.isclose, sod.customtext1, sod.customtext2, sod.customtext3, sod.customdbl1, sod.customdbl2, sod.customdbl3, sod.customdate1, sod.customdate2, sod.customdate3 "
                sql &= " FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid AND so.soid = '" & result(4) & "' JOIN m1_item i ON sod.idbarang = i.bid "
                sql &= " GROUP BY so.soid, sod.idbarang)  ON DUPLICATE KEY UPDATE idso= VALUES(idso), idbarang= VALUES(idbarang), nomorso= VALUES(nomorso), tglso= VALUES(tglso), tglkirimso= VALUES(tglkirimso), namabarang= VALUES(namabarang), tipebarang= VALUES(tipebarang), jml= VALUES(jml), satuan= VALUES(satuan), nilaisatuan= VALUES(nilaisatuan), jmlbarang= VALUES(jmlbarang), satuanbarang= VALUES(satuanbarang), matauang= VALUES(matauang), kurs= VALUES(kurs), harga= VALUES(harga), diskon= VALUES(diskon), jmldiskon= VALUES(jmldiskon), pajak1= VALUES(pajak1), jmlpajak1= VALUES(jmlpajak1), pajak2= VALUES(pajak2), jmlpajak2= VALUES(jmlpajak2), cabang= VALUES(cabang), lokasi= VALUES(lokasi), gudang= VALUES(gudang), costcenter= VALUES(costcenter), divisi= VALUES(divisi), subdivisi= VALUES(subdivisi), proyek= VALUES(proyek), catatan= VALUES(catatan), urutan= VALUES(urutan), idsqdetail= VALUES(idsqdetail), jmlpi= VALUES(jmlpi), statuspi= VALUES(statuspi), jmlpl= VALUES(jmlpl), statuspl= VALUES(statuspl), jmldo= VALUES(jmldo), statusdo= VALUES(statusdo), jmldr= VALUES(jmldr), statusdr= VALUES(statusdr), jmlsi= VALUES(jmlsi), statussi= VALUES(statussi), jmlrnr= VALUES(jmlrnr), statusrnr= VALUES(statusrnr), jmlsr= VALUES(jmlsr), statussr= VALUES(statussr), isclose= VALUES(isclose), customtext1= VALUES(customtext1), customtext2= VALUES(customtext2), customtext3= VALUES(customtext3), customdbl1= VALUES(customdbl1), customdbl2= VALUES(customdbl2), customdbl3= VALUES(customdbl3), customdate1= VALUES(customdate1), customdate2= VALUES(customdate2), customdate3= VALUES(customdate3); "
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                If drutama("sostatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
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
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                            Next

                            sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If

                    'UPDATE STATUS KONTRAK SF
                    If drutama("socustomdbl1") <> 0 Then
                        'UPDATE SF DETAIL
                        sql = "  UPDATE m5_sf_detail sfd "
                        sql &= " JOIN (SELECT so.socustomdbl1, i.bdivisi, SUM(sod.jmlbarang) as jmlso "
                        sql &= " FROM m5_so_detail sod "
                        sql &= " JOIN m5_so so ON sod.idso = so.soid AND so.soid = '" & FixDouble(result(4)) & "' "
                        sql &= " JOIN m1_item i ON sod.idbarang = i.bid "
                        sql &= " JOIN m1_division ON bdivisi = dkode "
                        sql &= " GROUP BY dkode) as rl ON sfd.idsf = rl.socustomdbl1 AND sfd.divisi = rl.bdivisi "
                        sql &= " SET sfd.jmlrealisasi = sfd.jmlrealisasi + rl.jmlso "
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'UPDATE SF UTAMA
                        sql = "  UPDATE m5_sf sf "
                        sql &= " JOIN (SELECT idsf, SUM(jmlbarang) as total, SUM(jmlrealisasi) as realisasi "
                        sql &= " FROM m5_sf_detail WHERE idsf = '" & FixDouble(Double.Parse(drutama("socustomdbl1"))) & "' "
                        sql &= " GROUP BY idsf) as sfd ON sf.sfid = sfd.idsf "
                        sql &= " SET sf.sfstatusrealisasi = (CASE WHEN sfd.realisasi > sfd.total THEN 2 "
                        sql &= " WHEN sfd.realisasi <= 0 THEN 0 ELSE 1 END) "
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    End If



                    'UPDATE STOK BOOKING ================================================================
                    'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                    sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "SO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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


    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftSQ As String, ByVal termasukPajak As String) As String
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
        End If

        'CEK SQ YANG DIAMBIL
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        If Len(ftSQ) > 0 Then
            sql = "SELECT sq.sqnotransaksi as notransaksi, (CASE sq.sqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sq.sqid WHERE " & ftSQ & " GROUP BY sq.sqhargatermasukpajak"
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
                sql = "SELECT i.bkode, sqd.idsqdetail, sq.sqnotransaksi as notransaksi, (CASE sq.sqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sq.sqid JOIN m1_item i ON sqd.idbarang = i.bid WHERE (" & ftSQ & ") AND sq.sqhargatermasukpajak <> " & termasukPajak & " ORDER BY sqd.urutan"
                dtval = AsDataTableAmbilDariDB(sql)
                If dtval.Rows.Count > 0 Then
                    'Ambil informasi utk errmessage
                    kodebarang = dtval.Rows(0)("bkode")

                    filterLookup = "idsqdetail = " & dtval.Rows(0)("idsqdetail")
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
        If Len(ftOutstanding) > 0 Then
            'sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            sql = "SELECT sqd.idsqdetail, ROUND(sqd.jmlbarang - sqd.jmlrealisasi, 5) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid JOIN m0_setting s ON s.smodule = 5 AND s.sgrup = 'options' AND s.skode = 'SOLebihDariSQ' WHERE " & ftOutstanding
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
    Public Function M5_SoSimpanSerenity(ByVal param As String) As String
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
        Dim Filter As String = "", Sorting As String = ""
        Dim notransaksiPDR As String = ""

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


        'CEK NOREFF ========================================================================
        'CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        'JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        'JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        If Len(Filter) > 0 Then
            sql = "SELECT soid, sonotransaksi FROM m5_so WHERE sonoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("soid")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("sonotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("soid")
                    GoTo selesai
                End If
            End If

        Else
            Dim validKey As RsValidKey
            validKey = ValidateKey(paramSplit(0))
            If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        End If
        'END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'soid(0) As Integer, socabang(1) As String, solokasi(2) As String, sogudang(3) As String, soasalbarang(4) As String, 
        'soasalbarangkategori(5) As Integer, sojenispenjualan(6) As String, sojenispenjualankategori(7) As Integer, socarabayar(8) As Integer, sosumber(9) As String, 
        'soautonotransaksi(10) As Integer, sonotransaksi(11) As String, sotgl(12) As Date, sokodepa(13) As Integer, socustomer(14) As Integer, 
        'socustomerkontak(15) As String, so1alamat1(16) As String, so1alamat2(17) As String, so1alamat3(18) As String, so2alamat1(19) As String, 
        'so2alamat2(20) As String, so2alamat3(21) As String, sobagianpenjualan(22) As Integer, soekspedisi(23) As String, sotglkirim(24) As Date, 
        'sotermin(25) As String, sotgljatuhtempo(26) As Date, souraian(27) As String, socatatan(28) As String, sonoref(29) As String, 
        'sotglnoref(30) As Date, sotglpenutupan(31) As Date, somatauang(32) As String, sokurs(33) As Double, sohargatermasukpajak(34) As Integer, 
        'sototal(35) As Double, sodiskonpersen(36) As String, sojmldiskon(37) As Double, sototalpajak1detail(38) As Double, sototalpajak2detail(39) As Double, 
        'sobiayalainpersen(40) As Double, sobiayalain(41) As Double, sototaltransaksi(42) As Double, sojmlbayar(43) As Double, sorekdiskon(44) As String, 
        'sorekpajak1(45) As String, sorekpajak2(46) As String, sorekbiayalain(47) As String, sorekbayar(48) As String, soidsq(49) As Integer, 
        'sostatuspl(50) As Integer, sostatusdo(51) As Integer, sostatusdr(52) As Integer, sostatuspi(53) As Integer, sostatussi(54) As Integer, 
        'sostatusrnr(55) As Integer, sostatussr(56) As Integer, sostatus(57) As Integer, sostatussebelumnya(58) As Integer, sojmlrevisi(59) As Integer, 
        'socetakanke(60) As Integer, soinputuser(61) As Integer, soinputtgl(62) As DateTime, somodifikasiuser(63) As Integer, somodifikasitgl(64) As DateTime, 
        'soisclose(65) As Integer, socustomtext1(66) As String, socustomtext2(67) As String, socustomtext3(68) As String, socustomtext4(69) As String, 
        'socustomtext5(70) As String, socustomint1(71) As Integer, socustomint2(72) As Integer, socustomint3(73) As Integer, socustomdbl1(74) As Double, 
        'socustomdbl2(75) As Double, socustomdbl3(76) As Double, socustomdate1(77) As Date, socustomdate2(78) As Date, socustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, 
        'somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, 
        'socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, 
        'socustomdate1, socustomdate2, socustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'soid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "soid required numeric." : GoTo selesai
        End If
        'soasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "soasalbarangkategori required numeric." : GoTo selesai
        End If
        'sojenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sojenispenjualankategori required numeric." : GoTo selesai
        End If
        'socarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "socarabayar required numeric." : GoTo selesai
        End If
        'soautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "soautonotransaksi required numeric." : GoTo selesai
        End If
        'sotgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sotgl required date." : GoTo selesai
        End If
        'sokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sokodepa required numeric." : GoTo selesai
        End If
        'socustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "socustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "socustomer can't be empty." : GoTo selesai
        End If
        'sobagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sobagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sobagianpenjualan can't be empty." : GoTo selesai
        End If
        'sotglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "sotglkirim required date." : GoTo selesai
        End If
        'sotgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "sotgljatuhtempo required date." : GoTo selesai
        End If
        'sotglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sotglnoref required date." : GoTo selesai
        End If
        'sotglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "sotglpenutupan required date." : GoTo selesai
        End If
        'sokurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sokurs required numeric." : GoTo selesai
        End If
        'sohargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sohargatermasukpajak required numeric." : GoTo selesai
        End If
        'sototal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sototal required numeric." : GoTo selesai
        End If
        'sojmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sojmldiskon required numeric." : GoTo selesai
        End If
        'sototalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sototalpajak1detail required numeric." : GoTo selesai
        End If
        'sototalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "sototalpajak2detail required numeric." : GoTo selesai
        End If
        ''sobiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "sobiayalainpersen required numeric." : GoTo selesai
        'End If
        'sobiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sobiayalain required numeric." : GoTo selesai
        End If
        'sototaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sototaltransaksi required numeric." : GoTo selesai
        End If
        'sojmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sojmlbayar required numeric." : GoTo selesai
        End If
        'soidsq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "soidsq required numeric." : GoTo selesai
        End If
        'sostatuspl(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sostatuspl required numeric." : GoTo selesai
        End If
        'sostatusdo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sostatusdo required numeric." : GoTo selesai
        End If
        'sostatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sostatusdr required numeric." : GoTo selesai
        End If
        'sostatuspi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sostatuspi required numeric." : GoTo selesai
        End If
        'sostatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sostatussi required numeric." : GoTo selesai
        End If
        'sostatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sostatusrnr required numeric." : GoTo selesai
        End If
        'sostatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "sostatussr required numeric." : GoTo selesai
        End If
        'sostatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sostatus required numeric." : GoTo selesai
        End If
        'sostatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "sostatussebelumnya required numeric." : GoTo selesai
        End If
        'sojmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sojmlrevisi required numeric." : GoTo selesai
        End If
        'socetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "socetakanke required numeric." : GoTo selesai
        End If
        'soinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "soinputuser required numeric." : GoTo selesai
        End If
        'soinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "soinputtgl required date." : GoTo selesai
        End If
        'somodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "somodifikasiuser required numeric." : GoTo selesai
        End If
        'somodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "somodifikasitgl required date." : GoTo selesai
        End If
        'soisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "soisclose required numeric." : GoTo selesai
        End If
        'socustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "socustomint1 required numeric." : GoTo selesai
        End If
        'socustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "socustomint2 required numeric." : GoTo selesai
        End If
        'socustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "socustomint3 required numeric." : GoTo selesai
        End If
        'socustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "socustomdbl1 required numeric." : GoTo selesai
        End If
        'socustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "socustomdbl2 required numeric." : GoTo selesai
        End If
        'socustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "socustomdbl3 required numeric." : GoTo selesai
        End If
        'socustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "socustomdate1 required date." : GoTo selesai
        End If
        'socustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "socustomdate2 required date." : GoTo selesai
        End If
        'socustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "socustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'socabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "socabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "socabang should not be more than 25 character." : GoTo selesai
        End If

        'solokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "solokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "solokasi should not be more than 25 character." : GoTo selesai
        End If

        'sogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sogudang should not be more than 25 character." : GoTo selesai
        End If

        'sosumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sosumber should not be more than 10 character." : GoTo selesai
        End If

        'sonotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sotgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sotgl can't be empty" : GoTo selesai
        End If

        'sotglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "sotglkirim can't be empty" : GoTo selesai
        End If

        'sotgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "sotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sotglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sotglnoref can't be empty" : GoTo selesai
        End If

        'sotglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sotglpenutupan can't be empty" : GoTo selesai
        End If

        'somatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "somatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "somatauang should not be more than 25 character." : GoTo selesai
        End If

        'sokurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "sokurs can't be empty" : GoTo selesai
        End If

        'sototal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sototal can't be empty" : GoTo selesai
        End If

        'sodiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "sodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'sojmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sojmldiskon can't be empty" : GoTo selesai
        End If

        'sototalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sototalpajak1detail can't be empty" : GoTo selesai
        End If

        'sototalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sototalpajak2detail can't be empty" : GoTo selesai
        End If

        'sobiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "sobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sobiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sobiayalain can't be empty" : GoTo selesai
        End If

        'sototaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "sototaltransaksi can't be empty" : GoTo selesai
        End If

        'sojmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "sojmlbayar can't be empty" : GoTo selesai
        End If

        'soinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "soinputtgl can't be empty" : GoTo selesai
        End If

        'somodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "somodifikasitgl can't be empty" : GoTo selesai
        End If

        'socustomtext1(66) As String
        'CUSTOM TEXT DIISI DARI SOCATATAN(28) + SOTGLKIRIM(24)
        'dataUtama(66) = String.Concat(dataUtama(28), Replace(dataUtama(24), "-", ""))

        'socustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "socustomdbl1 can't be empty" : GoTo selesai
        End If

        'socustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "socustomdbl2 can't be empty" : GoTo selesai
        End If

        'socustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "socustomdbl3 can't be empty" : GoTo selesai
        End If

        'socustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "socustomdate1 can't be empty" : GoTo selesai
        End If

        'socustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "socustomdate2 can't be empty" : GoTo selesai
        End If

        'socustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "socustomdate3 can't be empty" : GoTo selesai
        End If

        'result(2) = dataUtama(66) : GoTo selesai
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "soid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "solokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "souraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "somodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "soid~socabang~solokasi~sogudang~soasalbarang~soasalbarangkategori~sojenispenjualan~sojenispenjualankategori~socarabayar~sosumber~soautonotransaksi~sonotransaksi~sotgl~sokodepa~socustomer~socustomerkontak~so1alamat1~so1alamat2~so1alamat3~so2alamat1~so2alamat2~so2alamat3~sobagianpenjualan~soekspedisi~sotglkirim~sotermin~sotgljatuhtempo~souraian~socatatan~sonoref~sotglnoref~sotglpenutupan~somatauang~sokurs~sohargatermasukpajak~sototal~sodiskonpersen~sojmldiskon~sototalpajak1detail~sototalpajak2detail~sobiayalainpersen~sobiayalain~sototaltransaksi~sojmlbayar~sorekdiskon~sorekpajak1~sorekpajak2~sorekbiayalain~sorekbayar~soidsq~sostatuspl~sostatusdo~sostatusdr~sostatuspi~sostatussi~sostatusrnr~sostatussr~sostatus~sostatussebelumnya~sojmlrevisi~socetakanke~soinputuser~soinputtgl~somodifikasiuser~somodifikasitgl~soisclose~socustomtext1~socustomtext2~socustomtext3~socustomtext4~socustomtext5~socustomint1~socustomint2~socustomint3~socustomdbl1~socustomdbl2~socustomdbl3~socustomdate1~socustomdate2~socustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsodetail(0) As Integer, idso(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idsqdetail(28) As Integer, jmlpl(29) As Double, 
        'statuspl(30) As Integer, jmldo(31) As Double, statusdo(32) As Integer, jmldr(33) As Double, statusdr(34) As Integer, 
        'jmlpi(35) As Double, statuspi(36) As Integer, jmlsi(37) As Double, statussi(38) As Integer, jmlrnr(39) As Double, 
        'statusrnr(40) As Integer, jmlsr(41) As Double, statussr(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idso", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrnr", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", ftExistOutstandingSO As String = "", ftOutstandingSO As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updNilaiSO As String = "", updFilterSO As String = "", updStokBooking As String = ""
        Dim idbarang As Integer = 0, idsqdetail As Integer = 0, idsodetail As Integer = 0, jmlbarang As Double = 0
        Dim ftBarang As String = ""

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSQ As String = "", ftSO As String = ""

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
            'idsodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idso(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idso required numeric." : GoTo selesai
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
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlpl(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(12) As Double, diskon(13) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlpl(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idsodetail~idso~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(21) As String       , idsqdetail(28) As Integer      , customdbl3(49) As Double
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(21) : idsqdetail = dataRowDetail(28) : idsodetail = dataRowDetail(49)
            'kurs(11) As Double                    , harga(12) As Double
            kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            If idsqdetail <> 0 Then 'SQ
                'CEK SQ YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSQ = IIf(Len(ftSQ.ToString) = 0, "", ftSQ & " OR ")
                ftSQ = String.Concat(ftSQ, " (sqd.idsqdetail = " & idsqdetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            End If

            If idsodetail <> 0 Then 'SO TEMP
                'CEK SO YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSO = IIf(Len(ftSO.ToString) = 0, "", ftSO & " OR ")
                ftSO = String.Concat(ftSO, " (sod.idsodetail = " & idsodetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstandingSO = IIf(Len(ftExistOutstandingSO.ToString) = 0, "", ftExistOutstandingSO & " UNION ")
                ftExistOutstandingSO = String.Concat(ftExistOutstandingSO, "SELECT EXISTS(SELECT 1 FROM m5_so_detail JOIN m5_so ON idso = soid WHERE idsodetail = '" & idsodetail & "' AND (sostatus = 2 OR sostatus = 3 OR sostatus = 4 OR sostatus = 7) LIMIT 1) as rowExists, '" & idsodetail & "' as idsodetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim OutstandingSO As Double = AsDataTableDSum(dtdetail, "jmlbarang", "customdbl3=" & idsodetail)
                ftOutstandingSO = IIf(Len(ftOutstandingSO.ToString) = 0, "", ftOutstandingSO & " OR ")
                ftOutstandingSO = String.Concat(ftOutstandingSO, " (sod.idsodetail = " & idsodetail & " AND " & OutstandingSO & " > (sod.jmlbarang - sod.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi + '" & OutstandingSO & "', 5) ", updNilaiSO)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
            End If

            ''5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            ftBarang = IIf(Len(ftBarang.ToString) = 0, "", ftBarang & " OR ")
            ftBarang = String.Concat(ftBarang, "(bid = '" & idbarang & "')")

            'Validasi harga dibawah harga jual
            ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 4
                Select Case drutama("sostatus")
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


                'AMBIL MATA UANG FUNGSIONAL DARI SETTING ------------
                Dim MUFungsional As String = "", MUUtama As String = ""
                'Dim dtSetting As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')")
                Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='0') AND (sgrup='accounting') AND (skode='MataUangFungsional')", myConn)
                If dtSetting.Rows.Count > 0 Then
                    MUFungsional = dtSetting.Rows(0)(0)
                Else
                    result(2) = "Can't found 'Functional Currency' in Setting." : Trans.Rollback() : GoTo selesai
                End If

                'SET MATA UANG UTAMA
                MUUtama = drutama("somatauang")
                'END OF AMBIL MATA UANG FUNGSIONAL DARI SETTING ------


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sotgl")), AsFormatTanggal(drutama("sotgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sostatus") = 2 Or drutama("sostatus") = 1 Or drutama("sostatus") = 8 Or drutama("sostatus") = 9 Or drutama("sostatus") = 10 Or drutama("sostatus") = 11 Then

                    'VALIDASI PLAFON PIUTANG -------------
                    Dim dtHACustom As DataTable = AsDataTableAmbilDariDBCon("SELECT rc.rcmoduleid, rc.rcidpc, rc.rcrole, rc.rcakses FROM m0_permissions_custom pc JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc AND pc.pcmodule = 5 AND pc.pcid = 4 JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' ORDER BY rc.rcakses DESC LIMIT 1", myConn)
                    If dtHACustom.Rows.Count > 0 Then
                        If dtHACustom.Rows(0)("rcakses") = 0 Then
                            GoTo validasiPlafon
                        End If

                    Else
validasiPlafon:
                        Dim dtPlafonP As DataTable = AsDataTableAmbilDariDBCon("SELECT c.kbataspiutang, c.ktotalpiutang FROM m0_setting s JOIN m1_contact c ON c.kid = '" & drutama("socustomer") & "' AND s.smodule = 0 AND s.sgrup = 'options' AND s.skode = 'ValidasiPlafonPiutangSO' AND s.snilai = 1", myConn)
                        If dtPlafonP.Rows.Count > 0 Then
                            'JIKA BATAS PIUTANG > 0
                            If Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) > 0 Then
                                'JIKA TOTAL PIUTANG + TOTAL TRANSAKSI FUNGSIONAL > BATAS PIUTANG
                                If Double.Parse(dtPlafonP.Rows(0)("ktotalpiutang")) + (Double.Parse(drutama("sototaltransaksi")) * Double.Parse(drutama("sokurs"))) > Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) Then
                                    Dim selisih(2) As String
                                    selisih = F_Nominal(Double.Parse(dtPlafonP.Rows(0)("kbataspiutang")) - Double.Parse(dtPlafonP.Rows(0)("ktotalpiutang")), True).Split(sptSubParam)
                                    result(2) = "Total Transaction exceeds the limit of AR. (AR limit available " & MUFungsional & " " & selisih(1) & ")" : Trans.Rollback() : GoTo selesai
                                End If
                            End If
                        End If

                    End If
                    'END OF VALIDASI PLAFON PIUTANG ------


                    'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                    'Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                    'If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                    'Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftSQ, ftExistOutstandingSO, ftOutstandingSO, ftSO, drutama("sohargatermasukpajak"))
                    'If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================



                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sotermin").ToString, AsFormatTanggal(drutama("sotgl")), "sotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak1detail")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("soid")
                    notransaksi = drutama("sonotransaksi")

                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(soid), sonotransaksi FROM M5_so WHERE soid='" & result(4) & "' AND sostatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("soautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"), drutama("sosumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_so_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sosumber")) & "▼" & FixQuotes(drutama("soid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_So set socabang  = '" & FixQuotes(drutama("socabang")) & "', solokasi  = '" & FixQuotes(drutama("solokasi")) & "', sogudang  = '" & FixQuotes(drutama("sogudang")) & "', soasalbarang  = '" & FixQuotes(drutama("soasalbarang")) & "', soasalbarangkategori  = " & drutama("soasalbarangkategori") & ", sojenispenjualan  = '" & FixQuotes(drutama("sojenispenjualan")) & "', sojenispenjualankategori  = " & drutama("sojenispenjualankategori") & ", socarabayar  = " & drutama("socarabayar") & ", sosumber  = '" & FixQuotes(drutama("sosumber")) & "', soautonotransaksi  = " & drutama("soautonotransaksi") & ", sonotransaksi  = '" & FixQuotes(notransaksi) & "', sotgl  = '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', sokodepa  = " & drutama("sokodepa") & ", socustomer  = " & drutama("socustomer") & ", socustomerkontak  = '" & FixQuotes(drutama("socustomerkontak")) & "', so1alamat1  = '" & FixQuotes(drutama("so1alamat1")) & "', so1alamat2  = '" & FixQuotes(drutama("so1alamat2")) & "', so1alamat3  = '" & FixQuotes(drutama("so1alamat3")) & "', so2alamat1  = '" & FixQuotes(drutama("so2alamat1")) & "', so2alamat2  = '" & FixQuotes(drutama("so2alamat2")) & "', so2alamat3  = '" & FixQuotes(drutama("so2alamat3")) & "', sobagianpenjualan  = " & drutama("sobagianpenjualan") & ", soekspedisi  = '" & FixQuotes(drutama("soekspedisi")) & "', sotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', sotermin  = '" & FixQuotes(drutama("sotermin")) & "', sotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', souraian  = '" & FixQuotes(drutama("souraian")) & "', socatatan  = '" & FixQuotes(drutama("socatatan")) & "', sonoref  = '" & FixQuotes(drutama("sonoref")) & "', sotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', sotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', somatauang  = '" & FixQuotes(drutama("somatauang")) & "', sokurs  = '" & FixDouble(drutama("sokurs")) & "', sohargatermasukpajak  = " & drutama("sohargatermasukpajak") & ", sototal  = '" & FixDouble(drutama("sototal")) & "', sodiskonpersen  = '" & FixQuotes(drutama("sodiskonpersen")) & "', sojmldiskon  = '" & FixDouble(drutama("sojmldiskon")) & "', sototalpajak1detail  = '" & FixDouble(drutama("sototalpajak1detail")) & "', sototalpajak2detail  = '" & FixDouble(drutama("sototalpajak2detail")) & "', sobiayalainpersen  = '" & FixDouble(drutama("sobiayalainpersen")) & "', sobiayalain  = '" & FixDouble(drutama("sobiayalain")) & "', sototaltransaksi  = '" & FixDouble(drutama("sototaltransaksi")) & "', sojmlbayar  = '" & FixDouble(drutama("sojmlbayar")) & "', sorekdiskon  = '" & FixQuotes(drutama("sorekdiskon")) & "', sorekpajak1  = '" & FixQuotes(drutama("sorekpajak1")) & "', sorekpajak2  = '" & FixQuotes(drutama("sorekpajak2")) & "', sorekbiayalain  = '" & FixQuotes(drutama("sorekbiayalain")) & "', sorekbayar  = '" & FixQuotes(drutama("sorekbayar")) & "', soidsq  = " & drutama("soidsq") & ", sostatuspl  = " & drutama("sostatuspl") & ", sostatusdo  = " & drutama("sostatusdo") & ", sostatusdr  = " & drutama("sostatusdr") & ", sostatuspi  = " & drutama("sostatuspi") & ", sostatussi  = " & drutama("sostatussi") & ", sostatusrnr  = " & drutama("sostatusrnr") & ", sostatussr  = " & drutama("sostatussr") & ", sostatus  = " & drutama("sostatus") & ", sostatussebelumnya  = " & drutama("sostatussebelumnya") & ", sojmlrevisi  = sojmlrevisi+1, socetakanke  = " & drutama("socetakanke") & ", somodifikasiuser  = " & drutama("somodifikasiuser") & ", somodifikasitgl  = NOW(), socustomtext1  = '" & FixQuotes(drutama("socustomtext1")) & "', socustomtext2  = '" & FixQuotes(drutama("socustomtext2")) & "', socustomtext3  = '" & FixQuotes(drutama("socustomtext3")) & "', socustomtext4  = '" & FixQuotes(drutama("socustomtext4")) & "', socustomtext5  = '" & FixQuotes(drutama("socustomtext5")) & "', socustomint1  = " & drutama("socustomint1") & ", socustomint2  = " & drutama("socustomint2") & ", socustomint3  = " & drutama("socustomint3") & ", socustomdbl1  = '" & FixDouble(drutama("socustomdbl1")) & "', socustomdbl2  = '" & FixDouble(drutama("socustomdbl2")) & "', socustomdbl3  = '" & FixDouble(drutama("socustomdbl3")) & "', socustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', socustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', socustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "' where soid = '" & drutama("soid") & "'"
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

                    If drutama("soautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"), drutama("sosumber"), 5)
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
                        notransaksi = drutama("sonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_So (socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3) values('" & FixQuotes(drutama("socabang")) & "', '" & FixQuotes(drutama("solokasi")) & "', '" & FixQuotes(drutama("sogudang")) & "', '" & FixQuotes(drutama("soasalbarang")) & "', " & drutama("soasalbarangkategori") & ", '" & FixQuotes(drutama("sojenispenjualan")) & "', " & drutama("sojenispenjualankategori") & ", " & drutama("socarabayar") & ", '" & FixQuotes(drutama("sosumber")) & "', " & drutama("soautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', " & drutama("sokodepa") & ", " & drutama("socustomer") & ", '" & FixQuotes(drutama("socustomerkontak")) & "', '" & FixQuotes(drutama("so1alamat1")) & "', '" & FixQuotes(drutama("so1alamat2")) & "', '" & FixQuotes(drutama("so1alamat3")) & "', '" & FixQuotes(drutama("so2alamat1")) & "', '" & FixQuotes(drutama("so2alamat2")) & "', '" & FixQuotes(drutama("so2alamat3")) & "', " & drutama("sobagianpenjualan") & ", '" & FixQuotes(drutama("soekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', '" & FixQuotes(drutama("sotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', '" & FixQuotes(drutama("souraian")) & "', '" & FixQuotes(drutama("socatatan")) & "', '" & FixQuotes(drutama("sonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', '" & FixQuotes(drutama("somatauang")) & "', '" & FixDouble(drutama("sokurs")) & "', " & drutama("sohargatermasukpajak") & ", '" & FixDouble(drutama("sototal")) & "', '" & FixQuotes(drutama("sodiskonpersen")) & "', '" & FixDouble(drutama("sojmldiskon")) & "', '" & FixDouble(drutama("sototalpajak1detail")) & "', '" & FixDouble(drutama("sototalpajak2detail")) & "', '" & FixDouble(drutama("sobiayalainpersen")) & "', '" & FixDouble(drutama("sobiayalain")) & "', '" & FixDouble(drutama("sototaltransaksi")) & "', '" & FixDouble(drutama("sojmlbayar")) & "', '" & FixQuotes(drutama("sorekdiskon")) & "', '" & FixQuotes(drutama("sorekpajak1")) & "', '" & FixQuotes(drutama("sorekpajak2")) & "', '" & FixQuotes(drutama("sorekbiayalain")) & "', '" & FixQuotes(drutama("sorekbayar")) & "', " & drutama("soidsq") & ", " & drutama("sostatuspl") & ", " & drutama("sostatusdo") & ", " & drutama("sostatusdr") & ", " & drutama("sostatuspi") & ", " & drutama("sostatussi") & ", " & drutama("sostatusrnr") & ", " & drutama("sostatussr") & ", " & drutama("sostatus") & ", " & drutama("sostatussebelumnya") & ", " & drutama("sojmlrevisi") & ", " & drutama("socetakanke") & ", " & drutama("soinputuser") & ", NOW(), " & drutama("somodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("soisclose") & ", '" & FixQuotes(drutama("socustomtext1")) & "', '" & FixQuotes(drutama("socustomtext2")) & "', '" & FixQuotes(drutama("socustomtext3")) & "', '" & FixQuotes(drutama("socustomtext4")) & "', '" & FixQuotes(drutama("socustomtext5")) & "', " & drutama("socustomint1") & ", " & drutama("socustomint2") & ", " & drutama("socustomint3") & ", '" & FixDouble(drutama("socustomdbl1")) & "', '" & FixDouble(drutama("socustomdbl2")) & "', '" & FixDouble(drutama("socustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select soid from M5_so where sonotransaksi='" & notransaksi & "' AND soinputuser= '" & userid & "' order by somodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If


                ''INSERT COST CENTER
                'If drutama("solokasi") = "TES" Or drutama("solokasi") = "131" Then
                '    Dim vAkunPD As String = "", vAkunSI As String = "", sqlambil As String = ""
                '    Dim dt As New DataTable
                '    sqlambil = "SELECT IFNULL(c.cnomor,'') as akunPD, IFNULL(c2.cnomor,'') as akunSI FROM m1_location l LEFT JOIN m1_coa c ON l.lalamat2 = c.cnomor LEFT JOIN m1_coa c2 ON l.lkota = c2.cnomor WHERE l.lkode = '" & drutama("solokasi") & "'"
                '    dt = AsDataTableAmbilDariDBCon(sqlambil, myConn)
                '    If dt.Rows.Count > 0 Then
                '        vAkunPD = FxDB(dt.Rows(0)("akunPD"), "")
                '        vAkunSI = FxDB(dt.Rows(0)("akunSI"), "")
                '    Else
                '        result(2) = "Could not find Transaction Code for '" & drutama("solokasi") & "' location." : Trans.Rollback() : GoTo selesai
                '    End If

                '    sql = "INSERT INTO `m1_cost_center` (`cckode`, `ccnama`, `ccakun`, `cccatatan`) VALUES ('" & FixQuotes(notransaksi) & "', '" & FixQuotes(notransaksi) & "', '" & FixQuotes(vAkunPD) & "', '" & FixQuotes(vAkunSI) & "') ON DUPLICATE KEY UPDATE cckode = VALUES(cckode);"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = myConn
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If


                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_So_Detail where idso = '" & result(4) & "'"
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

                    Dim dtval As New DataTable, dtbarang As New DataTable
                    dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bjenis FROM m1_item WHERE (" & ftBarang & ")")

                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows

                        'dtval = AsDataTableFilterLimit(dtbarang, "bjenis = 'P' AND bid = '" & FixDouble(dr1("idbarang")) & "'", , , 1)
                        'If dtval.Rows.Count > 0 Then

                        '    'If drutama("solokasi") = "TES" Or drutama("solokasi") = "131" Then
                        '    '    dr1("costcenter") = notransaksi
                        '    'End If

                        '    If Len(dr1("costcenter")) = 0 Then
                        '        Dim wsM0_NomorPDR As New m0_nomor
                        '        Dim rsNotransaksiPDR As String = wsM0_NomorPDR.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), "PDR", drutama("sotgl"))
                        '        Dim arrNotransaksiPDR(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                        '        arrNotransaksiPDR = rsNotransaksiPDR.Split(sptSubParam)
                        '        'cek success generate notransaksi
                        '        If (arrNotransaksiPDR(0) = 1) Then
                        '            notransaksiPDR = arrNotransaksiPDR(2)
                        '            'tambah query update m0_nomor_next
                        '            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        '            With objCmd
                        '                .Connection = Con1
                        '                .Transaction = Trans
                        '                .CommandType = CommandType.Text
                        '                .CommandText = arrNotransaksiPDR(3)
                        '            End With
                        '            objCmd.ExecuteNonQuery()
                        '        Else
                        '            result(2) = arrNotransaksiPDR(1) : Trans.Rollback() : GoTo selesai
                        '        End If

                        '    Else
                        '        notransaksiPDR = dr1("costcenter")

                        '    End If

                        'Else
                        '    notransaksiPDR = ""

                        'End If


                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & dr1("idsodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_So_Detail(idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("sostatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
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
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                            Next

                            sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If


                    If Len(updNilaiSO) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilaiSO & " ELSE jmlrealisasi END) WHERE " & updFilterSO
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso", myConn)
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso", myConn)
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilaiSO = "" : updFilterSO = ""
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
                                updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                                updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                            Next

                            sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilaiSO & " ELSE sostatusrealisasi END) WHERE " & updFilterSO
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If


                    'UPDATE STOK BOOKING ================================================================
                    'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                    sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

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

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "SO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SoUpdateStatus20241128(ByVal param As String) As String
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
            Dim sumber As String = "So", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sotgl, Sonotransaksi, Sostatus FROM M5_So WHERE Soid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sostatussebelumnya" : jnsaktivitas = 17
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

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_so_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = m5_so_terkait("so.soid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0, idsodetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", updNilaiSO As String = "", updFilterSO As String = "", gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idsqdetail, urutan, customdbl3 FROM m5_so_detail WHERE idso = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idsqdetail = dr1("idsqdetail") : idsodetail = dr1("customdbl3")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                        End If

                        If idsodetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim OutstandingSO As Double = AsDataTableDSum(dtdetail, "jmlbarang", "customdbl3=" & idsodetail)
                            updNilaiSO = String.Concat("WHEN '" & idsodetail & "' THEN ROUND(jmlrealisasi - '" & OutstandingSO & "', 5) ", updNilaiSO)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                            updFilterSO = String.Concat(updFilterSO, "(idsodetail = '" & idsodetail & "')")
                        End If

                        ''3. SET NILAI UPDATE STOK KELUAR -------------
                        'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
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
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                        Next

                        sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
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


                If Len(updFilterSO) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_so_detail SET jmlrealisasi = (CASE idsodetail " & updNilaiSO & " ELSE jmlrealisasi END) WHERE " & updFilterSO
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idso FROM m5_so_detail WHERE " & updFilterSO & " GROUP BY idso", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idso = '" & dr1("idso") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idso, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_so_detail WHERE " & ftDetail & " GROUP BY idso", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilaiSO = "" : updFilterSO = ""
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
                            updNilaiSO = String.Concat(updNilaiSO, "WHEN '" & dr1("idso") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilterSO = IIf(Len(updFilterSO.ToString) = 0, "", updFilterSO & " OR ")
                            updFilterSO = String.Concat(updFilterSO, "(soid = '" & dr1("idso") & "')")
                        Next

                        sql = "UPDATE m5_so SET sostatusrealisasi = (CASE soid " & updNilaiSO & " ELSE sostatusrealisasi END) WHERE " & updFilterSO
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


                'UPDATE STOK BOOKING ================================
                'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang * -1 FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

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
            If jnsaktivitas = 7 Then
                'KURANGI STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            If jnsaktivitas = 17 Then
                'TAMBAH STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M5_So SET Sostatus = " & nilaiStatus & ", Somodifikasiuser='" & userid & "', Somodifikasitgl = NOW(), Soposting = 0, Sopostingtgl = '1971-01-01 00:00:00', Sojmlrevisi = Sojmlrevisi + 1 WHERE Soid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_soSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SoUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "So", tglTransaksi As String = "", customdbl1 As Double = 0
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sotgl, Sonotransaksi, Sostatus, socustomdbl1 FROM M5_So WHERE Soid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2) : customdbl1 = Double.Parse(dtdetail.Rows(1)(3))
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sostatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            'JIKA CLOSE MAKA CEK STOCK BATCH
            If jnsaktivitas = 7 Then
                sql = "  SELECT nbi.nbigudang, nbi.nbitgl, nbi.nbinotransaksi, i.bkode, i.bnama, nbi.nbijmlsisa, nbi.nbisatuan "
                sql &= " FROM m1_no_batch_in nbi "
                sql &= " JOIN m1_item i ON nbi.nbiidbarang = i.bid "
                sql &= " AND nbi.nbikode = '" & FixQuotes(notransaksi) & "' "
                sql &= " AND ROUND(nbi.nbijmlsisa,5) <> 0 "
                sql &= " ORDER BY nbi.nbitgl, nbi.nbinotransaksi, i.bkode "
                Dim dtCekBatch As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                If dtCekBatch.Rows.Count > 0 Then
                    result(2) = "Can't close '" & notransaksi & "'. It has stock in item : " & dtCekBatch.Rows(0)("bkode") : Trans.Rollback() : GoTo selesai
                End If
            End If

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(tglTransaksi), AsFormatTanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_so_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_so_terkait("soid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idsqdetail, urutan FROM m5_so_detail WHERE idso = '" & idtransaksi & "'", myConn)
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idsqdetail = dr1("idsqdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                        End If

                        ''3. SET NILAI UPDATE STOK KELUAR -------------
                        'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
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
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                        Next

                        sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
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

                'UPDATE STATUS KONTRAK SF
                If customdbl1 <> 0 Then
                    'UPDATE SF DETAIL
                    sql = "  UPDATE m5_sf_detail sfd "
                    sql &= " JOIN (SELECT so.socustomdbl1, i.bdivisi, SUM(sod.jmlbarang) as jmlso "
                    sql &= " FROM m5_so_detail sod "
                    sql &= " JOIN m5_so so ON sod.idso = so.soid AND so.soid = '" & FixDouble(idtransaksi) & "' "
                    sql &= " JOIN m1_item i ON sod.idbarang = i.bid "
                    sql &= " JOIN m1_division ON bdivisi = dkode "
                    sql &= " GROUP BY dkode) as rl ON sfd.idsf = rl.socustomdbl1 AND sfd.divisi = rl.bdivisi "
                    sql &= " SET sfd.jmlrealisasi = sfd.jmlrealisasi - rl.jmlso "
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'UPDATE SF UTAMA
                    sql = "  UPDATE m5_sf sf "
                    sql &= " JOIN (SELECT idsf, SUM(jmlbarang) as total, SUM(jmlrealisasi) as realisasi "
                    sql &= " FROM m5_sf_detail WHERE idsf = '" & FixDouble(Double.Parse(customdbl1)) & "' "
                    sql &= " GROUP BY idsf) as sfd ON sf.sfid = sfd.idsf "
                    sql &= " SET sf.sfstatusrealisasi = (CASE WHEN sfd.realisasi > sfd.total THEN 2 "
                    sql &= " WHEN sfd.realisasi <= 0 THEN 0 ELSE 1 END) "
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'UPDATE STOK BOOKING ================================
                'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang * -1 FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

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
            If jnsaktivitas = 7 Then
                'KURANGI STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            If jnsaktivitas = 17 Then
                'TAMBAH STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = myconn
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M5_So SET Sostatus = " & nilaiStatus & ", Somodifikasiuser='" & userid & "', Somodifikasitgl = NOW(), Soposting = 0, Sopostingtgl = '1971-01-01 00:00:00', Sojmlrevisi = Sojmlrevisi + 1 WHERE Soid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_soSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SoDelete(ByVal param As String) As String

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
            Dim sumber As String = "So", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Soid, Sonotransaksi FROM M5_So WHERE Soid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT socabang, solokasi, sosumber, soautonotransaksi, sonotransaksi, sotgl"
            sql &= " FROM M5_so"
            sql &= " WHERE soid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("socabang")
                lokasi = dtNomorNext.Rows(0)("solokasi")
                sumber = dtNomorNext.Rows(0)("sosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("soautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sonotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_So_Detail WHERE idso = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_So WHERE soid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_SoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SoGetdataById(ByVal param As String) As String
        'M5_So_GetdataById Utama --------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, 
        'socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, 
        'socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, 
        'solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, 
        'soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, 
        'sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama, ktingkatjual, kpkp

        'M5_So_GetdataById Detail --------------------------------------------------------
        'idsodetail, idso, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, 
        'statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, sqnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

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

        Dim utama As String = "", detail As String = "", cost As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M5_So~M5_So_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "soid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "soid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `so`.`soid` AS `soid`,`so`.`socabang` AS `socabang`,`so`.`solokasi` AS `solokasi`,`so`.`sogudang` AS `sogudang`,`so`.`soasalbarang` AS `soasalbarang`,`so`.`soasalbarangkategori` AS `soasalbarangkategori`,`so`.`sojenispenjualan` AS `sojenispenjualan`,`so`.`sojenispenjualankategori` AS `sojenispenjualankategori`,`so`.`socarabayar` AS `socarabayar`,`so`.`sosumber` AS `sosumber`,`so`.`soautonotransaksi` AS `soautonotransaksi`,`so`.`sonotransaksi` AS `sonotransaksi`,`so`.`sotgl` AS `sotgl`,`so`.`sokodepa` AS `sokodepa`,`so`.`socustomer` AS `socustomer`,`so`.`socustomerkontak` AS `socustomerkontak`,`so`.`so1alamat1` AS `so1alamat1`,`so`.`so1alamat2` AS `so1alamat2`,`so`.`so1alamat3` AS `so1alamat3`,`so`.`so2alamat1` AS `so2alamat1`,`so`.`so2alamat2` AS `so2alamat2`,`so`.`so2alamat3` AS `so2alamat3`,`so`.`sobagianpenjualan` AS `sobagianpenjualan`,`so`.`soekspedisi` AS `soekspedisi`,`so`.`sotglkirim` AS `sotglkirim`,`so`.`sotermin` AS `sotermin`,`so`.`sotgljatuhtempo` AS `sotgljatuhtempo`,`so`.`souraian` AS `souraian`,`so`.`socatatan` AS `socatatan`,`so`.`sonoref` AS `sonoref`,`so`.`sotglnoref` AS `sotglnoref`,`so`.`sotglpenutupan` AS `sotglpenutupan`,`so`.`somatauang` AS `somatauang`,`so`.`sokurs` AS `sokurs`,`so`.`sohargatermasukpajak` AS `sohargatermasukpajak`,`so`.`sototal` AS `sototal`,`so`.`sodiskonpersen` AS `sodiskonpersen`,`so`.`sojmldiskon` AS `sojmldiskon`,`so`.`sototalpajak1detail` AS `sototalpajak1detail`,`so`.`sototalpajak2detail` AS `sototalpajak2detail`,`so`.`sobiayalainpersen` AS `sobiayalainpersen`,`so`.`sobiayalain` AS `sobiayalain`,`so`.`sototaltransaksi` AS `sototaltransaksi`,`so`.`sojmlbayar` AS `sojmlbayar`,`so`.`sorekdiskon` AS `sorekdiskon`,`so`.`sorekpajak1` AS `sorekpajak1`,`so`.`sorekpajak2` AS `sorekpajak2`,`so`.`sorekbiayalain` AS `sorekbiayalain`,`so`.`sorekbayar` AS `sorekbayar`,`so`.`soidsq` AS `soidsq`,`so`.`sostatuspl` AS `sostatuspl`,`so`.`sostatusdo` AS `sostatusdo`,`so`.`sostatusdr` AS `sostatusdr`,`so`.`sostatuspi` AS `sostatuspi`,`so`.`sostatussi` AS `sostatussi`,`so`.`sostatusrnr` AS `sostatusrnr`,`so`.`sostatussr` AS `sostatussr`,`so`.`sostatusrealisasi` AS `sostatusrealisasi`,`so`.`sostatus` AS `sostatus`,`so`.`sostatussebelumnya` AS `sostatussebelumnya`,`so`.`sojmlrevisi` AS `sojmlrevisi`,`so`.`socetakanke` AS `socetakanke`,`so`.`soinputuser` AS `soinputuser`,`so`.`soinputtgl` AS `soinputtgl`,`so`.`somodifikasiuser` AS `somodifikasiuser`,`so`.`somodifikasitgl` AS `somodifikasitgl`,`so`.`soposting` AS `soposting`,`so`.`sopostingtgl` AS `sopostingtgl`,`so`.`soisclose` AS `soisclose`,`so`.`socustomtext1` AS `socustomtext1`,`so`.`socustomtext2` AS `socustomtext2`,`so`.`socustomtext3` AS `socustomtext3`,`so`.`socustomtext4` AS `socustomtext4`,`so`.`socustomtext5` AS `socustomtext5`,`so`.`socustomint1` AS `socustomint1`,`so`.`socustomint2` AS `socustomint2`,`so`.`socustomint3` AS `socustomint3`,`so`.`socustomdbl1` AS `socustomdbl1`,`so`.`socustomdbl2` AS `socustomdbl2`,`so`.`socustomdbl3` AS `socustomdbl3`,`so`.`socustomdate1` AS `socustomdate1`,`so`.`socustomdate2` AS `socustomdate2`,`so`.`socustomdate3` AS `socustomdate3`,`br`.`bnama` AS `socabangnama`,`lc`.`lnama` AS `solokasinama`,`wh`.`wnama` AS `sogudangnama`,`c1`.`ktingkatjual`,`c1`.`kkode` AS `socustomerkode`,`c1`.`knama` AS `socustomernama`,`c2`.`kkode` AS `sobagianpenjualankode`,`c2`.`knama` AS `sobagianpenjualannama`,`e`.`enama` AS `soekspedisinama`,`tr`.`trnama` AS `soterminnama`,`tr`.`trharijatuhtempo` AS `soterminharijatuhtempo`,`coa1`.`cnama` AS `sorekdiskonnama`,`coa2`.`cnama` AS `sorekpajak1nama`,`coa3`.`cnama` AS `sorekpajak2nama`,`coa4`.`cnama` AS `sorekbiayalainnama`,`coa5`.`cnama` AS `sorekbayarnama`,`sq`.`sqnotransaksi` AS `sonotransaksisq`,`st1`.`nama` AS `sostatusnama`,`st2`.`nama` AS `sostatussebelumnyanama`,`u1`.`unama` AS `soinputusernama`,`u2`.`unama` AS `somodifikasiusernama`,`sod`.`idsodetail` AS `idsodetail`,`sod`.`idso` AS `idso`,`sod`.`idbarang` AS `idbarang`,`sod`.`namabarang` AS `namabarang`,`sod`.`tipebarang` AS `tipebarang`,`sod`.`jml` AS `jml`,`sod`.`satuan` AS `satuan`,`sod`.`nilaisatuan` AS `nilaisatuan`,`sod`.`jmlbarang` AS `jmlbarang`,`sod`.`satuanbarang` AS `satuanbarang`,`sod`.`matauang` AS `matauang`,`sod`.`kurs` AS `kurs`,`sod`.`harga` AS `harga`,`sod`.`diskon` AS `diskon`,`sod`.`jmldiskon` AS `jmldiskon`,`sod`.`pajak1` AS `pajak1`,`sod`.`jmlpajak1` AS `jmlpajak1`,`sod`.`pajak2` AS `pajak2`,`sod`.`jmlpajak2` AS `jmlpajak2`,`sod`.`cabang` AS `cabang`,`sod`.`lokasi` AS `lokasi`,`sod`.`gudang` AS `gudang`,`sod`.`costcenter` AS `costcenter`,`sod`.`divisi` AS `divisi`,`sod`.`subdivisi` AS `subdivisi`,`sod`.`proyek` AS `proyek`,`sod`.`catatan` AS `catatan`,`sod`.`urutan` AS `urutan`,`sod`.`idsqdetail` AS `idsqdetail`,`sod`.`jmlpl` AS `jmlpl`,`sod`.`statuspl` AS `statuspl`,`sod`.`jmldo` AS `jmldo`,`sod`.`statusdo` AS `statusdo`,`sod`.`jmldr` AS `jmldr`,`sod`.`statusdr` AS `statusdr`,`sod`.`jmlpi` AS `jmlpi`,`sod`.`statuspi` AS `statuspi`,`sod`.`jmlsi` AS `jmlsi`,`sod`.`statussi` AS `statussi`,`sod`.`jmlrnr` AS `jmlrnr`,`sod`.`statusrnr` AS `statusrnr`,`sod`.`jmlsr` AS `jmlsr`,`sod`.`statussr` AS `statussr`,`sod`.`jmlrealisasi` AS `jmlrealisasi`,`sod`.`statusrealisasi` AS `statusrealisasi`,`sod`.`isclose` AS `isclose`,`sod`.`customtext1` AS `customtext1`,`sod`.`customtext2` AS `customtext2`,`sod`.`customtext3` AS `customtext3`,`sod`.`customdbl1` AS `customdbl1`,`sod`.`customdbl2` AS `customdbl2`,`sod`.`customdbl3` AS `customdbl3`,`sod`.`customdate1` AS `customdate1`,`sod`.`customdate2` AS `customdate2`,`sod`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`sq2`.`sqnotransaksi` AS `sqnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, sod.customdbl4, sod.customdbl5, sf.sfnotransaksi, sod.customtext4, sod.customtext5 from ((((((((((((((((((((((((((((((`m5_so` `so` join `m5_so_detail` `sod` on((`so`.`soid` = `sod`.`idso`))) left join `m1_branch` `br` on((`br`.`bkode` = `so`.`socabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `so`.`solokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `so`.`sogudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `so`.`socustomer`))) left join `m1_contact` `c2` on((`c2`.`kid` = `so`.`sobagianpenjualan`))) left join `m1_expedition` `e` on((`so`.`soekspedisi` = `e`.`ekode`))) left join `m1_terms` `tr` on((`so`.`sotermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`so`.`sorekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`so`.`sorekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`so`.`sorekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`so`.`sorekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`so`.`sorekbayar` = `coa5`.`cnomor`))) left join `m5_sq` `sq` on((`so`.`soidsq` = `sq`.`sqid`))) left join `m0_status` `st1` on((`st1`.`kode` = `so`.`sostatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `so`.`sostatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `so`.`soinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `so`.`somodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `sod`.`idbarang`))) left join `m1_tax` `t1` on((`sod`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`sod`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`sod`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`sod`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`sod`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`sod`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`sod`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`sod`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`sod`.`proyek` = `p`.`pkode`))) left join `m5_sq_detail` `sqd` on((`sod`.`idsqdetail` = `sqd`.`idsqdetail`))) left join `m5_sq` `sq2` on((`sqd`.`idsq` = `sq2`.`sqid`))) left join `m5_sf` `sf` on((`sf`.`sfid` = `so`.`socustomdbl1`))"

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("soid"), 0), sptField,
                     FxDB(drutama("socabang"), ""), sptField,
                     FxDB(drutama("solokasi"), ""), sptField,
                     FxDB(drutama("sogudang"), ""), sptField,
                     FxDB(drutama("soasalbarang"), ""), sptField,
                     FxDB(drutama("soasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sojenispenjualan"), ""), sptField,
                     FxDB(drutama("sojenispenjualankategori"), 0), sptField,
                     FxDB(drutama("socarabayar"), 0), sptField,
                     FxDB(drutama("sosumber"), ""), sptField,
                     FxDB(drutama("soautonotransaksi"), 0), sptField,
                     FxDB(drutama("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sokodepa"), 0), sptField,
                     FxDB(drutama("socustomer"), 0), sptField,
                     FxDB(drutama("socustomerkontak"), ""), sptField,
                     FxDB(drutama("so1alamat1"), ""), sptField,
                     FxDB(drutama("so1alamat2"), ""), sptField,
                     FxDB(drutama("so1alamat3"), ""), sptField,
                     FxDB(drutama("so2alamat1"), ""), sptField,
                     FxDB(drutama("so2alamat2"), ""), sptField,
                     FxDB(drutama("so2alamat3"), ""), sptField,
                     FxDB(drutama("sobagianpenjualan"), 0), sptField,
                     FxDB(drutama("soekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("souraian"), ""), sptField,
                     FxDB(drutama("socatatan"), ""), sptField,
                     FxDB(drutama("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("somatauang"), ""), sptField,
                     FxDB(drutama("sokurs"), 0), sptField,
                     FxDB(drutama("sohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sototal"), 0), sptField,
                     FxDB(drutama("sodiskonpersen"), ""), sptField,
                     FxDB(drutama("sojmldiskon"), 0), sptField,
                     FxDB(drutama("sototalpajak1detail"), 0), sptField,
                     FxDB(drutama("sototalpajak2detail"), 0), sptField,
                     FxDB(drutama("sobiayalainpersen"), 0), sptField,
                     FxDB(drutama("sobiayalain"), 0), sptField,
                     FxDB(drutama("sototaltransaksi"), 0), sptField,
                     FxDB(drutama("sojmlbayar"), 0), sptField,
                     FxDB(drutama("sorekdiskon"), ""), sptField,
                     FxDB(drutama("sorekpajak1"), ""), sptField,
                     FxDB(drutama("sorekpajak2"), ""), sptField,
                     FxDB(drutama("sorekbiayalain"), ""), sptField,
                     FxDB(drutama("sorekbayar"), ""), sptField,
                     FxDB(drutama("soidsq"), 0), sptField,
                     FxDB(drutama("sostatuspl"), 0), sptField,
                     FxDB(drutama("sostatusdo"), 0), sptField,
                     FxDB(drutama("sostatusdr"), 0), sptField,
                     FxDB(drutama("sostatuspi"), 0), sptField,
                     FxDB(drutama("sostatussi"), 0), sptField,
                     FxDB(drutama("sostatusrnr"), 0), sptField,
                     FxDB(drutama("sostatussr"), 0), sptField,
                     FxDB(drutama("sostatusrealisasi"), 0), sptField,
                     FxDB(drutama("sostatus"), 0), sptField,
                     FxDB(drutama("sostatussebelumnya"), 0), sptField,
                     FxDB(drutama("sojmlrevisi"), 0), sptField,
                     FxDB(drutama("socetakanke"), 0), sptField,
                     FxDB(drutama("soinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("somodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soisclose"), 0), sptField,
                     FxDB(drutama("socustomtext1"), ""), sptField,
                     FxDB(drutama("socustomtext2"), ""), sptField,
                     FxDB(drutama("socustomtext3"), ""), sptField,
                     FxDB(drutama("socustomtext4"), ""), sptField,
                     FxDB(drutama("socustomtext5"), ""), sptField,
                     FxDB(drutama("socustomint1"), 0), sptField,
                     FxDB(drutama("socustomint2"), 0), sptField,
                     FxDB(drutama("socustomint3"), 0), sptField,
                     FxDB(drutama("socustomdbl1"), 0), sptField,
                     FxDB(drutama("socustomdbl2"), 0), sptField,
                     FxDB(drutama("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("socabangnama"), ""), sptField,
                     FxDB(drutama("solokasinama"), ""), sptField,
                     FxDB(drutama("sogudangnama"), ""), sptField,
                     FxDB(drutama("socustomerkode"), ""), sptField,
                     FxDB(drutama("socustomernama"), ""), sptField,
                     FxDB(drutama("sobagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sobagianpenjualannama"), ""), sptField,
                     FxDB(drutama("soekspedisinama"), ""), sptField,
                     FxDB(drutama("soterminnama"), ""), sptField,
                     FxDB(drutama("soterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sorekdiskonnama"), ""), sptField,
                     FxDB(drutama("sorekpajak1nama"), ""), sptField,
                     FxDB(drutama("sorekpajak2nama"), ""), sptField,
                     FxDB(drutama("sorekbiayalainnama"), ""), sptField,
                     FxDB(drutama("sorekbayarnama"), ""), sptField,
                     FxDB(drutama("sonotransaksisq"), ""), sptField,
                     FxDB(drutama("sostatusnama"), ""), sptField,
                     FxDB(drutama("sostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("soinputusernama"), ""), sptField,
                     FxDB(drutama("somodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("sfnotransaksi"), ""))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), 0), sptField,
                     FxDB(dr("customdbl4"), 0), sptField,
                     FxDB(dr("customdbl5"), 0), sptField,
                     FxDB(dr("customtext4"), ""), sptField,
                     FxDB(dr("customtext5"), ""), sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA COST
            sql = "SELECT rc.idsocost, rc.idso, rc.kodecost, rc.matauang, rc.kurs, rc.jumlah, rc.rekdebit, rc.rekkredit, rc.catatan, rc.costcenter, rc.divisi, rc.subdivisi, rc.proyek, rc.urutan, rc.idprcost, rc.idcscost, rc.idrqcost, rc.idbscost, rc.idpocost, rc.idipccost, rc.idgrncost, rc.jumlahbayar, rc.statusbayar, rc.isclose, rc.customtext1, rc.customtext2, rc.customtext3, rc.customdbl1, rc.customdbl2, rc.customdbl3, rc.customdate1, rc.customdate2, rc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sdnama as subdivisinama, p.pnama as proyeknama, rc.kontak, c.kkode as kontakkode, c.knama as kontaknama, rc.termasukhpp FROM m5_so_cost rc JOIN m5_so ri ON rc.idso = ri.soid LEFT JOIN m1_other_cost oc ON rc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON rc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON rc.rekkredit = coa2.cnomor LEFT JOIN m1_cost_center cc ON rc.costcenter = cc.cckode LEFT JOIN m1_division d ON rc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON rc.subdivisi = sd.sdkode LEFT JOIN m1_project p ON rc.proyek = p.pkode LEFT JOIN m1_contact c ON rc.kontak = c.kid"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m5_so_cost", Filter, "rc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idsocost"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, cost)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama, ktingkatjual, kpkp, sfnotransaksi" & sptSubParam & "idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, customdbl4, customdbl5, customtext4, customtext5" & sptSubParam & "idsocost, idso, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, idpocost, idipccost, idgrncost, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, costcenternama, divisinama, subdivisinama, proyeknama, kontak, kontakkode, kontaknama, termasukhpp"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SoGetdataByIdSerenity(ByVal param As String) As String
        'M5_So_GetdataById Utama --------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, 
        'socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, 
        'socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, 
        'solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, 
        'soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, 
        'sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama, ktingkatjual, kpkp

        'M5_So_GetdataById Detail --------------------------------------------------------
        'idsodetail, idso, 
        'idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, 
        'satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, 
        'jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, 
        'divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, 
        'statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, 
        'jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, 
        'statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, 
        'pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, 
        'subdivisinama, proyeknama, sqnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan

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

        Dim NmMemcached As String = "aplikasi1-M5_So~M5_So_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "soid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "soid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m5_so_getdata")

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("soid"), 0), sptField,
                     FxDB(drutama("socabang"), ""), sptField,
                     FxDB(drutama("solokasi"), ""), sptField,
                     FxDB(drutama("sogudang"), ""), sptField,
                     FxDB(drutama("soasalbarang"), ""), sptField,
                     FxDB(drutama("soasalbarangkategori"), 0), sptField,
                     FxDB(drutama("sojenispenjualan"), ""), sptField,
                     FxDB(drutama("sojenispenjualankategori"), 0), sptField,
                     FxDB(drutama("socarabayar"), 0), sptField,
                     FxDB(drutama("sosumber"), ""), sptField,
                     FxDB(drutama("soautonotransaksi"), 0), sptField,
                     FxDB(drutama("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgl"), ""), formatTgl), sptField,
                     FxDB(drutama("sokodepa"), 0), sptField,
                     FxDB(drutama("socustomer"), 0), sptField,
                     FxDB(drutama("socustomerkontak"), ""), sptField,
                     FxDB(drutama("so1alamat1"), ""), sptField,
                     FxDB(drutama("so1alamat2"), ""), sptField,
                     FxDB(drutama("so1alamat3"), ""), sptField,
                     FxDB(drutama("so2alamat1"), ""), sptField,
                     FxDB(drutama("so2alamat2"), ""), sptField,
                     FxDB(drutama("so2alamat3"), ""), sptField,
                     FxDB(drutama("sobagianpenjualan"), 0), sptField,
                     FxDB(drutama("soekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(drutama("sotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("souraian"), ""), sptField,
                     FxDB(drutama("socatatan"), ""), sptField,
                     FxDB(drutama("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("sotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("somatauang"), ""), sptField,
                     FxDB(drutama("sokurs"), 0), sptField,
                     FxDB(drutama("sohargatermasukpajak"), 0), sptField,
                     FxDB(drutama("sototal"), 0), sptField,
                     FxDB(drutama("sodiskonpersen"), ""), sptField,
                     FxDB(drutama("sojmldiskon"), 0), sptField,
                     FxDB(drutama("sototalpajak1detail"), 0), sptField,
                     FxDB(drutama("sototalpajak2detail"), 0), sptField,
                     FxDB(drutama("sobiayalainpersen"), 0), sptField,
                     FxDB(drutama("sobiayalain"), 0), sptField,
                     FxDB(drutama("sototaltransaksi"), 0), sptField,
                     FxDB(drutama("sojmlbayar"), 0), sptField,
                     FxDB(drutama("sorekdiskon"), ""), sptField,
                     FxDB(drutama("sorekpajak1"), ""), sptField,
                     FxDB(drutama("sorekpajak2"), ""), sptField,
                     FxDB(drutama("sorekbiayalain"), ""), sptField,
                     FxDB(drutama("sorekbayar"), ""), sptField,
                     FxDB(drutama("soidsq"), 0), sptField,
                     FxDB(drutama("sostatuspl"), 0), sptField,
                     FxDB(drutama("sostatusdo"), 0), sptField,
                     FxDB(drutama("sostatusdr"), 0), sptField,
                     FxDB(drutama("sostatuspi"), 0), sptField,
                     FxDB(drutama("sostatussi"), 0), sptField,
                     FxDB(drutama("sostatusrnr"), 0), sptField,
                     FxDB(drutama("sostatussr"), 0), sptField,
                     FxDB(drutama("sostatusrealisasi"), 0), sptField,
                     FxDB(drutama("sostatus"), 0), sptField,
                     FxDB(drutama("sostatussebelumnya"), 0), sptField,
                     FxDB(drutama("sojmlrevisi"), 0), sptField,
                     FxDB(drutama("socetakanke"), 0), sptField,
                     FxDB(drutama("soinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("somodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("soisclose"), 0), sptField,
                     FxDB(drutama("socustomtext1"), ""), sptField,
                     FxDB(drutama("socustomtext2"), ""), sptField,
                     FxDB(drutama("socustomtext3"), ""), sptField,
                     FxDB(drutama("socustomtext4"), ""), sptField,
                     FxDB(drutama("socustomtext5"), ""), sptField,
                     FxDB(drutama("socustomint1"), 0), sptField,
                     FxDB(drutama("socustomint2"), 0), sptField,
                     FxDB(drutama("socustomint3"), 0), sptField,
                     FxDB(drutama("socustomdbl1"), 0), sptField,
                     FxDB(drutama("socustomdbl2"), 0), sptField,
                     FxDB(drutama("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("socustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("socabangnama"), ""), sptField,
                     FxDB(drutama("solokasinama"), ""), sptField,
                     FxDB(drutama("sogudangnama"), ""), sptField,
                     FxDB(drutama("socustomerkode"), ""), sptField,
                     FxDB(drutama("socustomernama"), ""), sptField,
                     FxDB(drutama("sobagianpenjualankode"), ""), sptField,
                     FxDB(drutama("sobagianpenjualannama"), ""), sptField,
                     FxDB(drutama("soekspedisinama"), ""), sptField,
                     FxDB(drutama("soterminnama"), ""), sptField,
                     FxDB(drutama("soterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("sorekdiskonnama"), ""), sptField,
                     FxDB(drutama("sorekpajak1nama"), ""), sptField,
                     FxDB(drutama("sorekpajak2nama"), ""), sptField,
                     FxDB(drutama("sorekbiayalainnama"), ""), sptField,
                     FxDB(drutama("sorekbayarnama"), ""), sptField,
                     FxDB(drutama("sonotransaksisq"), ""), sptField,
                     FxDB(drutama("sostatusnama"), ""), sptField,
                     FxDB(drutama("sostatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("soinputusernama"), ""), sptField,
                     FxDB(drutama("somodifikasiusernama"), ""), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
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
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3, socabangnama, solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, soterminnama, soterminharijatuhtempo, sorekdiskonnama, sorekpajak1nama, sorekpajak2nama, sorekbiayalainnama, sorekbayarnama, sonotransaksisq, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama, ktingkatjual, kpkp" & sptSubParam & "idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, sqnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SoSearch(ByVal param As String) As String
        'M5_SoSearch --------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socabangnama, 
        'solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, 
        'sqnotransaksi, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama

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
			Filter = Filter.Replace("Srcustomernama", "c1.knama")
            Filter = Filter.Replace("Sobagianpenjualannama", "`c2`.`knama`")
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
        sql = query.PanggilQuery("m5_so_v")

        dt = AmbilData("aplikasi1-M5_so_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("socabang"), ""), sptField,
                     FxDB(dr("solokasi"), ""), sptField,
                     FxDB(dr("sogudang"), ""), sptField,
                     FxDB(dr("soasalbarang"), ""), sptField,
                     FxDB(dr("soasalbarangkategori"), 0), sptField,
                     FxDB(dr("sojenispenjualan"), ""), sptField,
                     FxDB(dr("sojenispenjualankategori"), 0), sptField,
                     FxDB(dr("socarabayar"), 0), sptField,
                     FxDB(dr("sosumber"), ""), sptField,
                     FxDB(dr("soautonotransaksi"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     FxDB(dr("sokodepa"), 0), sptField,
                     FxDB(dr("socustomer"), 0), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), 0), sptField,
                     FxDB(dr("soekspedisi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("sotermin"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("souraian"), ""), sptField,
                     FxDB(dr("socatatan"), ""), sptField,
                     FxDB(dr("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("somatauang"), ""), sptField,
                     FxDB(dr("sokurs"), 0), sptField,
                     FxDB(dr("sohargatermasukpajak"), 0), sptField,
                     FxDB(dr("sototal"), 0), sptField,
                     FxDB(dr("sodiskonpersen"), ""), sptField,
                     FxDB(dr("sojmldiskon"), 0), sptField,
                     FxDB(dr("sototalpajak1detail"), 0), sptField,
                     FxDB(dr("sototalpajak2detail"), 0), sptField,
                     FxDB(dr("sobiayalainpersen"), 0), sptField,
                     FxDB(dr("sobiayalain"), 0), sptField,
                     FxDB(dr("sototaltransaksi"), 0), sptField,
                     FxDB(dr("sojmlbayar"), 0), sptField,
                     FxDB(dr("sorekdiskon"), ""), sptField,
                     FxDB(dr("sorekpajak1"), ""), sptField,
                     FxDB(dr("sorekpajak2"), ""), sptField,
                     FxDB(dr("sorekbiayalain"), ""), sptField,
                     FxDB(dr("sorekbayar"), ""), sptField,
                     FxDB(dr("soidsq"), 0), sptField,
                     FxDB(dr("sostatuspl"), 0), sptField,
                     FxDB(dr("sostatusdo"), 0), sptField,
                     FxDB(dr("sostatusdr"), 0), sptField,
                     FxDB(dr("sostatuspi"), 0), sptField,
                     FxDB(dr("sostatussi"), 0), sptField,
                     FxDB(dr("sostatusrnr"), 0), sptField,
                     FxDB(dr("sostatussr"), 0), sptField,
                     FxDB(dr("sostatusrealisasi"), 0), sptField,
                     FxDB(dr("sostatus"), 0), sptField,
                     FxDB(dr("sostatussebelumnya"), 0), sptField,
                     FxDB(dr("sojmlrevisi"), 0), sptField,
                     FxDB(dr("socetakanke"), 0), sptField,
                     FxDB(dr("soinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("somodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soisclose"), 0), sptField,
                     FxDB(dr("socabangnama"), ""), sptField,
                     FxDB(dr("solokasinama"), ""), sptField,
                     FxDB(dr("sogudangnama"), ""), sptField,
                     FxDB(dr("socustomerkode"), ""), sptField,
                     FxDB(dr("socustomernama"), ""), sptField,
                     FxDB(dr("sobagianpenjualankode"), ""), sptField,
                     FxDB(dr("sobagianpenjualannama"), ""), sptField,
                     FxDB(dr("soekspedisinama"), ""), sptField,
                     FxDB(dr("sqnotransaksi"), ""), sptField,
                     FxDB(dr("sostatusnama"), ""), sptField,
                     FxDB(dr("sostatussebelumnyanama"), ""), sptField,
                     FxDB(dr("soinputusernama"), ""), sptField,
                     FxDB(dr("somodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socabangnama, solokasinama, sogudangnama, socustomerkode, socustomernama, sobagianpenjualankode, sobagianpenjualannama, soekspedisinama, sqnotransaksi, sostatusnama, sostatussebelumnyanama, soinputusernama, somodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_So_Detail_VSearch(ByVal param As String) As String
        'M5_So_Detail_VSearch --------------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sonotransaksi, souraian, socatatan, sonoref, 
        'sotgl, sotglnoref, sotglkirim, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, 
        'sobagianpenjualan, sobagianpenjualankode, sobagianpenjualannama, soekspedisi, soekspedisinama, sotermin, soterminnama, 
        'soterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, 
        'brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, jmlsisadr, jmlsisapi, 
        'jmlsisasi, jmlsisarealisasi, socustomer, socustomerkode, socustomernama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, 
        'jmlsisarealisasips, bhargabeli, basset, ktingkatjual, somatauang, sokurs, sotgljatuhtempo, sohargatermasukpajak, kpkp, 
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, 
        'pajak2akunjualnama, bcustom12, bcustom11, divisinama, subdivisinama, costcenternama, proyeknama, sojenispenjualankategori

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
            Filter = Filter.Replace("idbarang", "sod.idbarang")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sol = query.PanggilQuery("m5_so_detail_v")
        sol = "select sod.idsodetail AS idsodetail, sod.idso AS idso, sod.idbarang AS idbarang, sod.namabarang AS namabarang, sod.tipebarang AS tipebarang, sod.jml AS jml, sod.satuan AS satuan, sod.nilaisatuan AS nilaisatuan, sod.jmlbarang AS jmlbarang, sod.satuanbarang AS satuanbarang, sod.matauang AS matauang, sod.kurs AS kurs, sod.harga AS harga, sod.diskon AS diskon, sod.jmldiskon AS jmldiskon, sod.pajak1 AS pajak1, sod.jmlpajak1 AS jmlpajak1, sod.pajak2 AS pajak2, sod.jmlpajak2 AS jmlpajak2, sod.cabang AS cabang, sod.lokasi AS lokasi, sod.gudang AS gudang, sod.costcenter AS costcenter, sod.divisi AS divisi, sod.subdivisi AS subdivisi, sod.proyek AS proyek, sod.catatan AS catatan, sod.urutan AS urutan, sod.idsqdetail AS idsqdetail, sod.jmlpl AS jmlpl, sod.statuspl AS statuspl, sod.jmldo AS jmldo, sod.statusdo AS statusdo, sod.jmldr AS jmldr, sod.statusdr AS statusdr, sod.jmlpi AS jmlpi, sod.statuspi AS statuspi, sod.jmlsi AS jmlsi, sod.statussi AS statussi, sod.jmlrnr AS jmlrnr, sod.statusrnr AS statusrnr, sod.jmlsr AS jmlsr, sod.statussr AS statussr, sod.jmlrealisasi AS jmlrealisasi, sod.statusrealisasi AS statusrealisasi, sod.isclose AS isclose, sod.customtext1 AS customtext1, sod.customtext2 AS customtext2, sod.customtext3 AS customtext3, sod.customdbl1 AS customdbl1, sod.customdbl2 AS customdbl2, sod.customdbl3 AS customdbl3, sod.customdate1 AS customdate1, sod.customdate2 AS customdate2, sod.customdate3 AS customdate3, so.sonotransaksi AS sonotransaksi, so.souraian AS souraian, so.socatatan AS socatatan, so.sonoref AS sonoref, so.sotgl AS sotgl, so.sotglnoref AS sotglnoref, so.sotglkirim AS sotglkirim, so.socustomerkontak AS socustomerkontak, so.so1alamat1 AS so1alamat1, so.so1alamat2 AS so1alamat2, so.so1alamat3 AS so1alamat3, so.so2alamat1 AS so2alamat1, so.so2alamat2 AS so2alamat2, so.so2alamat3 AS so2alamat3, so.sobagianpenjualan AS sobagianpenjualan, c1.kkode AS sobagianpenjualankode, c1.knama AS sobagianpenjualannama, so.soekspedisi AS soekspedisi, e.enama AS soekspedisinama, so.sotermin AS sotermin, tr.trnama AS soterminnama, tr.trharijatuhtempo AS soterminharijatuhtempo, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bjenis AS bjenis, i.brekpersediaan AS brekpersediaan, i.brekhargapokok AS brekhargapokok, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekpenjualan AS brekpenjualan, i.bserial AS bserial, i.bbatch AS bbatch, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, ((sod.jmlbarang - sod.jmlpl) / sod.nilaisatuan) AS jmlsisapl, ((sod.jmlbarang - sod.jmldo) / sod.nilaisatuan) AS jmlsisado, ((sod.jmlbarang - sod.jmldr) / sod.nilaisatuan) AS jmlsisadr, ((sod.jmlbarang - sod.jmlpi) / sod.nilaisatuan) AS jmlsisapi, ((sod.jmlbarang - sod.jmlsi) / sod.nilaisatuan) AS jmlsisasi, ((sod.jmlbarang - sod.jmlrealisasi) / sod.nilaisatuan) AS jmlsisarealisasi, ((sod.jmlbarang - sod.customdbl2) / sod.nilaisatuan) AS jmlsisarealisasips, so.socustomer AS socustomer, c.kkode AS socustomerkode, c.knama AS socustomernama, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.bhargabeli, i.basset, c.ktingkatjual, so.somatauang, so.sokurs, so.sotgljatuhtempo, so.sohargatermasukpajak, c.kpkp, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, i.bcustom12, i.bcustom11, d.dnama AS divisinama, sd.sdnama AS subdivisinama, cc.ccnama AS costcenternama, p.pnama AS proyeknama, so.sojenispenjualankategori  from m5_so_detail sod join m5_so so on sod.idso = so.soid left join m1_terms tr on so.sotermin = tr.trkode left join m1_contact c1 on so.sobagianpenjualan = c1.kid left join m1_expedition e on so.soekspedisi = e.ekode left join m1_item i on sod.idbarang = i.bid left join m1_tax t1 on sod.pajak1 = t1.tkode left join m1_tax t2 on sod.pajak2 = t2.tkode left join m1_contact c on so.socustomer = c.kid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor LEFT JOIN m1_division d ON d.dkode = sod.divisi LEFT JOIN m1_subdivision sd ON sd.sdkode = sod.subdivisi LEFT JOIN m1_cost_center cc ON cc.cckode = sod.costcenter LEFT JOIN m1_project p ON p.pkode = sod.proyek"

        dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
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
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("souraian"), ""), sptField,
                     FxDB(dr("socatatan"), ""), sptField,
                     FxDB(dr("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), 0), sptField,
                     FxDB(dr("sobagianpenjualankode"), ""), sptField,
                     FxDB(dr("sobagianpenjualannama"), ""), sptField,
                     FxDB(dr("soekspedisi"), ""), sptField,
                     FxDB(dr("soekspedisinama"), ""), sptField,
                     FxDB(dr("sotermin"), ""), sptField,
                     FxDB(dr("soterminnama"), ""), sptField,
                     FxDB(dr("soterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisapl"), 0), sptField,
                     FxDB(dr("jmlsisado"), 0), sptField,
                     FxDB(dr("jmlsisadr"), 0), sptField,
                     FxDB(dr("jmlsisapi"), 0), sptField,
                     FxDB(dr("jmlsisasi"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("socustomerkode"), ""), sptField,
                     FxDB(dr("socustomernama"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("jmlsisarealisasips"), 0), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("somatauang"), ""), sptField,
                     FxDB(dr("sokurs"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sohargatermasukpajak"), 0), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("bcustom12"), 0), sptField,
                     FxDB(dr("bcustom11"), 0), sptField,
                     FxDB(dr("divisinama"), ""), sptField,
                     FxDB(dr("subdivisinama"), ""), sptField,
                     FxDB(dr("costcenternama"), ""), sptField,
                     FxDB(dr("proyeknama"), ""), sptField,
                     FxDB(dr("sojenispenjualankategori"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sonotransaksi, souraian, socatatan, sonoref, sotgl, sotglnoref, sotglkirim, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, sobagianpenjualankode, sobagianpenjualannama, soekspedisi, soekspedisinama, sotermin, soterminnama, soterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, jmlsisadr, jmlsisapi, jmlsisasi, jmlsisarealisasi, socustomer, socustomerkode, socustomernama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisarealisasips, bhargabeli, basset, ktingkatjual, somatauang, sokurs, sotgljatuhtempo, sohargatermasukpajak, kpkp, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, bcustom12, bcustom11, divisinama, subdivisinama, costcenternama, proyeknama, sojenispenjualankategori"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_So_Detail_VSearchGroup(ByVal param As String) As String
        'M5_So_Detail_VSearch --------------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, 
        'harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, 
        'proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, 
        'jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sonotransaksi, souraian, socatatan, sonoref, 
        'sotgl, sotglnoref, sotglkirim, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, 
        'sobagianpenjualan, sobagianpenjualankode, sobagianpenjualannama, soekspedisi, soekspedisinama, sotermin, soterminnama, 
        'soterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, 
        'brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, jmlsisadr, jmlsisapi, 
        'jmlsisasi, jmlsisarealisasi, socustomer, socustomerkode, socustomernama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, 
        'jmlsisarealisasips, bhargabeli, basset, ktingkatjual, somatauang, sokurs, sotgljatuhtempo, sohargatermasukpajak, kpkp, 
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, 
        'pajak2akunjualnama, sojenispenjualankategori

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
            Filter = Filter.Replace("idbarang", "sod.idbarang")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sol = query.PanggilQuery("m5_so_detail_v")
        sol = "select sod.idsodetail AS idsodetail, sod.idso AS idso, sod.idbarang AS idbarang, sod.namabarang AS namabarang, sod.tipebarang AS tipebarang, SUM(sod.jml) AS jml, sod.satuan AS satuan, sod.nilaisatuan AS nilaisatuan, SUM(sod.jmlbarang) AS jmlbarang, sod.satuanbarang AS satuanbarang, sod.matauang AS matauang, sod.kurs AS kurs, sod.harga AS harga, sod.diskon AS diskon, sod.jmldiskon AS jmldiskon, sod.pajak1 AS pajak1, sod.jmlpajak1 AS jmlpajak1, sod.pajak2 AS pajak2, sod.jmlpajak2 AS jmlpajak2, sod.cabang AS cabang, sod.lokasi AS lokasi, sod.gudang AS gudang, sod.costcenter AS costcenter, sod.divisi AS divisi, sod.subdivisi AS subdivisi, sod.proyek AS proyek, sod.catatan AS catatan, sod.urutan AS urutan, sod.idsqdetail AS idsqdetail, sod.jmlpl AS jmlpl, sod.statuspl AS statuspl, sod.jmldo AS jmldo, sod.statusdo AS statusdo, sod.jmldr AS jmldr, sod.statusdr AS statusdr, sod.jmlpi AS jmlpi, sod.statuspi AS statuspi, sod.jmlsi AS jmlsi, sod.statussi AS statussi, sod.jmlrnr AS jmlrnr, sod.statusrnr AS statusrnr, sod.jmlsr AS jmlsr, sod.statussr AS statussr, SUM(sod.jmlrealisasi) AS jmlrealisasi, sod.statusrealisasi AS statusrealisasi, sod.isclose AS isclose, sod.customtext1 AS customtext1, sod.customtext2 AS customtext2, sod.customtext3 AS customtext3, sod.customdbl1 AS customdbl1, sod.customdbl2 AS customdbl2, sod.customdbl3 AS customdbl3, sod.customdate1 AS customdate1, sod.customdate2 AS customdate2, sod.customdate3 AS customdate3, so.sonotransaksi AS sonotransaksi, so.souraian AS souraian, so.socatatan AS socatatan, so.sonoref AS sonoref, so.sotgl AS sotgl, so.sotglnoref AS sotglnoref, so.sotglkirim AS sotglkirim, so.socustomerkontak AS socustomerkontak, so.so1alamat1 AS so1alamat1, so.so1alamat2 AS so1alamat2, so.so1alamat3 AS so1alamat3, so.so2alamat1 AS so2alamat1, so.so2alamat2 AS so2alamat2, so.so2alamat3 AS so2alamat3, so.sobagianpenjualan AS sobagianpenjualan, c1.kkode AS sobagianpenjualankode, c1.knama AS sobagianpenjualannama, so.soekspedisi AS soekspedisi, e.enama AS soekspedisinama, so.sotermin AS sotermin, tr.trnama AS soterminnama, tr.trharijatuhtempo AS soterminharijatuhtempo, i.bkode AS kodebarang, i.bhpp AS bhpp, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bjenis AS bjenis, i.brekpersediaan AS brekpersediaan, i.brekhargapokok AS brekhargapokok, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekpenjualan AS brekpenjualan, i.bserial AS bserial, i.bbatch AS bbatch, t1.tnama AS pajak1nama, t1.tnilai AS pajak1nilai, t2.tnama AS pajak2nama, t2.tnilai AS pajak2nilai, ((sod.jmlbarang - sod.jmlpl) / sod.nilaisatuan) AS jmlsisapl, ((sod.jmlbarang - sod.jmldo) / sod.nilaisatuan) AS jmlsisado, ((sod.jmlbarang - sod.jmldr) / sod.nilaisatuan) AS jmlsisadr, ((sod.jmlbarang - sod.jmlpi) / sod.nilaisatuan) AS jmlsisapi, ((sod.jmlbarang - sod.jmlsi) / sod.nilaisatuan) AS jmlsisasi, SUM(((sod.jmlbarang - sod.jmlrealisasi) / sod.nilaisatuan)) AS jmlsisarealisasi, SUM(((sod.jmlbarang - sod.customdbl2) / sod.nilaisatuan)) AS jmlsisarealisasips, so.socustomer AS socustomer, c.kkode AS socustomerkode, c.knama AS socustomernama, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan, i.bhargabeli, i.basset, c.ktingkatjual, so.somatauang, so.sokurs, so.sotgljatuhtempo, so.sohargatermasukpajak, c.kpkp, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama, so.sojenispenjualankategori from m5_so_detail sod join m5_so so on sod.idso = so.soid left join m1_terms tr on so.sotermin = tr.trkode left join m1_contact c1 on so.sobagianpenjualan = c1.kid left join m1_expedition e on so.soekspedisi = e.ekode left join m1_item i on sod.idbarang = i.bid left join m1_tax t1 on sod.pajak1 = t1.tkode left join m1_tax t2 on sod.pajak2 = t2.tkode left join m1_contact c on so.socustomer = c.kid left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        'dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotglkirim ASC, so.socatatan ASC, i.bkode ASC, i.bid", sol) ' Ambil data ke databases
        dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "sod.idsodetail", sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("idso"), 0), sptField,
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
                     FxDB(dr("idsqdetail"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlsi"), 0), sptField,
                     FxDB(dr("statussi"), 0), sptField,
                     FxDB(dr("jmlrnr"), 0), sptField,
                     FxDB(dr("statusrnr"), 0), sptField,
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
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("souraian"), ""), sptField,
                     FxDB(dr("socatatan"), ""), sptField,
                     FxDB(dr("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), 0), sptField,
                     FxDB(dr("sobagianpenjualankode"), ""), sptField,
                     FxDB(dr("sobagianpenjualannama"), ""), sptField,
                     FxDB(dr("soekspedisi"), ""), sptField,
                     FxDB(dr("soekspedisinama"), ""), sptField,
                     FxDB(dr("sotermin"), ""), sptField,
                     FxDB(dr("soterminnama"), ""), sptField,
                     FxDB(dr("soterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("kodebarang"), ""), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("pajak1nama"), ""), sptField,
                     FxDB(dr("pajak1nilai"), 0), sptField,
                     FxDB(dr("pajak2nama"), ""), sptField,
                     FxDB(dr("pajak2nilai"), 0), sptField,
                     FxDB(dr("jmlsisapl"), 0), sptField,
                     FxDB(dr("jmlsisado"), 0), sptField,
                     FxDB(dr("jmlsisadr"), 0), sptField,
                     FxDB(dr("jmlsisapi"), 0), sptField,
                     FxDB(dr("jmlsisasi"), 0), sptField,
                     FxDB(dr("jmlsisarealisasi"), 0), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("socustomerkode"), ""), sptField,
                     FxDB(dr("socustomernama"), ""), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("jmlsisarealisasips"), 0), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("somatauang"), ""), sptField,
                     FxDB(dr("sokurs"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sotgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("sohargatermasukpajak"), 0), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("pajak1akunbeli"), ""), sptField,
                     FxDB(dr("pajak1akunbelinama"), ""), sptField,
                     FxDB(dr("pajak1akunjual"), ""), sptField,
                     FxDB(dr("pajak1akunjualnama"), ""), sptField,
                     FxDB(dr("pajak2akunbeli"), ""), sptField,
                     FxDB(dr("pajak2akunbelinama"), ""), sptField,
                     FxDB(dr("pajak2akunjual"), ""), sptField,
                     FxDB(dr("pajak2akunjualnama"), ""), sptField,
                     FxDB(dr("sojenispenjualankategori"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, sonotransaksi, souraian, socatatan, sonoref, sotgl, sotglnoref, sotglkirim, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, sobagianpenjualankode, sobagianpenjualannama, soekspedisi, soekspedisinama, sotermin, soterminnama, soterminharijatuhtempo, kodebarang, bhpp, bhppaverage, bhargajual1, bjenis, brekpersediaan, brekhargapokok, brekdiskonpenjualan, brekpenjualan, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisapl, jmlsisado, jmlsisadr, jmlsisapi, jmlsisasi, jmlsisarealisasi, socustomer, socustomerkode, socustomernama, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan, jmlsisarealisasips, bhargabeli, basset, ktingkatjual, somatauang, sokurs, sotgljatuhtempo, sohargatermasukpajak, kpkp, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama, sojenispenjualankategori"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SoTerkait(ByVal param As String) As String
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
            Filter = pagingSplit(2) & " AND so.soid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "so.soid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = m5_so_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_so_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
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
            result(2) = "Related SO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, sonotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_SoTerkait_S(ByVal param As String) As String
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
            Filter = pagingSplit(2) & " AND so.soid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "so.soid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        sql = m5_so_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_so_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
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
            result(2) = "Related SO data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idtransaksi, notransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function m5_so_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = ""
        Dim filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = "" ', filter10 As String = ""
        Dim filter11 As String = "", filter12 As String = "", filter13 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            filter1 = strFilter

            filter2 = strFilter
            filter2 = filter2 & " AND ((`as`.`asstatus` = 2) or (`as`.`asstatus` = 3) or (`as`.`asstatus` = 4) or (`as`.`asstatus` = 7))"

            filter3 = strFilter
            filter3 = filter3 & " AND ((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"

            filter4 = strFilter
            filter4 = filter4 & " AND ((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"

            filter5 = strFilter
            filter5 = filter5 & " AND ((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"

            filter6 = strFilter
            filter6 = filter6 & " AND ((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"

            filter7 = strFilter
            filter7 = filter7 & " AND ((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"

            filter8 = strFilter
            filter8 = filter8 & " AND ((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"

            filter9 = strFilter
            filter9 = filter9 & " AND ((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"

            'filter10 = strFilter
            'filter10 = filter10 & " AND ((`m3_sa`.`sastatus` = 2) or (`m3_sa`.`sastatus` = 3) or (`m3_sa`.`sastatus` = 4) or (`m3_sa`.`sastatus` = 7))"

            filter11 = strFilter
            filter11 = filter11 & " AND ((`m3_ts`.`tsstatus` = 2) or (`m3_ts`.`tsstatus` = 3) or (`m3_ts`.`tsstatus` = 4) or (`m3_ts`.`tsstatus` = 7))"

            filter12 = strFilter
            filter12 = filter12 & " AND ((so2.`sostatus` = 2) or (so2.`sostatus` = 3) or (so2.`sostatus` = 4) or (so2.`sostatus` = 7))"

            filter13 = strFilter

        Else
            'Default filter
            filter2 = "((`as`.`asstatus` = 2) or (`as`.`asstatus` = 3) or (`as`.`asstatus` = 4) or (`as`.`asstatus` = 7))"
            filter3 = "((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"
            filter4 = "((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"
            filter5 = "((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"
            filter6 = "((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"
            filter7 = "((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"
            filter8 = "((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"
            filter9 = "((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
            'filter10 = "((`m3_sa`.`sastatus` = 2) or (`m3_sa`.`sastatus` = 3) or (`m3_sa`.`sastatus` = 4) or (`m3_sa`.`sastatus` = 7))"
            filter11 = "((`m3_ts`.`tsstatus` = 2) or (`m3_ts`.`tsstatus` = 3) or (`m3_ts`.`tsstatus` = 4) or (`m3_ts`.`tsstatus` = 7))"
            filter12 = "((so2.`sostatus` = 2) or (so2.`sostatus` = 3) or (so2.`sostatus` = 4) or (so2.`sostatus` = 7))"
        End If

        If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        If Len(filter9) > 0 Then filter9 = " WHERE " & filter9
        'If Len(filter10) > 0 Then filter10 = " WHERE " & filter10
        If Len(filter11) > 0 Then filter11 = " WHERE " & filter11
        If Len(filter12) > 0 Then filter12 = " WHERE " & filter12
        If Len(filter13) > 0 Then filter13 = " WHERE " & filter13

        sql = " SELECT so.soid AS soid, so.sonotransaksi AS sonotransaksi, sq.sqsumber AS sumber, sq.sqid AS idterkait, sq.sqnotransaksi AS noterkait, sq.sqtgl AS tglterkait, sq.sqinputtgl AS inputtglterkait,  sq.sqmodifikasitgl AS modifikasitglterkait,  0 as jenisterkait FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sqid JOIN m5_so_detail sod ON sqd.idsqdetail = sod.idsqdetail JOIN m5_so so ON sod.idso = so.soid " & filter1 & " GROUP BY sq.sqid, so.soid"
        sql &= " UNION ALL "
        sql &= "SELECT so.soid AS soid, so.sonotransaksi AS sonotransaksi, `as`.assumber AS sumber, `as`.asid AS idterkait, `as`.asnotransaksi AS noterkait, `as`.astgl AS tglterkait, `as`.asinputtgl AS inputtglterkait, `as`.asmodifikasitgl AS modifikasitglterkait, 1 as jenisterkait FROM m5_as `as` JOIN m5_so so ON `as`.asidso = so.soid " & filter2 & "  GROUP BY `as`.asid, so.soid"
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'PI' AS `sumber`,`m5_pi`.`piid` AS `idterkait`,`m5_pi`.`pinotransaksi` AS `noterkait`,`m5_pi`.`pitgl` AS `tglterkait`,`m5_pi`.`piinputtgl` AS `inputtglterkait`,`m5_pi`.`pimodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_pi_detail` on((`m5_pi_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_pi` on((`m5_pi_detail`.`idpi` = `m5_pi`.`piid`))) " & filter6 & "  group by `so`.`soid`, `m5_pi`.`piid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'PL' AS `sumber`,`m5_pl`.`plid` AS `idterkait`,`m5_pl`.`plnotransaksi` AS `noterkait`,`m5_pl`.`pltgl` AS `tglterkait`,`m5_pl`.`plinputtgl` AS `inputtglterkait`,`m5_pl`.`plmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_pl_detail` on((`m5_pl_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_pl` on((`m5_pl_detail`.`idpl` = `m5_pl`.`plid`))) " & filter3 & "  group by `so`.`soid`, `m5_pl`.`plid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'DO' AS `sumber`,`m5_do`.`doid` AS `idterkait`,`m5_do`.`donotransaksi` AS `noterkait`,`m5_do`.`dotgl` AS `tglterkait`,`m5_do`.`doinputtgl` AS `inputtglterkait`,`m5_do`.`domodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_do_detail` on((`m5_do_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_do` on((`m5_do_detail`.`iddo` = `m5_do`.`doid`))) " & filter4 & "  group by `so`.`soid`, `m5_do`.`doid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'DR' AS `sumber`,`m5_dr`.`drid` AS `idterkait`,`m5_dr`.`drnotransaksi` AS `noterkait`,`m5_dr`.`drtgl` AS `tglterkait`,`m5_dr`.`drinputtgl` AS `inputtglterkait`,`m5_dr`.`drmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_dr_detail` on((`m5_dr_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_dr` on((`m5_dr_detail`.`iddr` = `m5_dr`.`drid`))) " & filter5 & "  group by `so`.`soid`, `m5_dr`.`drid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'SI' AS `sumber`,`m5_si`.`siid` AS `idterkait`,`m5_si`.`sinotransaksi` AS `noterkait`,`m5_si`.`sitgl` AS `tglterkait`,`m5_si`.`siinputtgl` AS `inputtglterkait`,`m5_si`.`simodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_si_detail` on((`m5_si_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_si` on((`m5_si_detail`.`idsi` = `m5_si`.`siid`))) " & filter7 & "  group by `so`.`soid`, `m5_si`.`siid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'RNR' AS `sumber`,`m5_rnr`.`rnrid` AS `idterkait`,`m5_rnr`.`rnrnotransaksi` AS `noterkait`,`m5_rnr`.`rnrtgl` AS `tglterkait`,`m5_rnr`.`rnrinputtgl` AS `inputtglterkait`,`m5_rnr`.`rnrmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_rnr_detail` on((`m5_rnr_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_rnr` on((`m5_rnr_detail`.`idrnr` = `m5_rnr`.`rnrid`))) " & filter8 & "  group by `so`.`soid`, `m5_rnr`.`rnrid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'SR' AS `sumber`,`m5_sr`.`srid` AS `idterkait`,`m5_sr`.`srnotransaksi` AS `noterkait`,`m5_sr`.`srtgl` AS `tglterkait`,`m5_sr`.`srinputtgl` AS `inputtglterkait`,`m5_sr`.`srmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_sr_detail` on((`m5_sr_detail`.`idsodetail` = `sod`.`idsodetail`))) join `m5_sr` on((`m5_sr_detail`.`idsr` = `m5_sr`.`srid`))) " & filter9 & "  group by `so`.`soid`, `m5_sr`.`srid` "
        'sql &= " UNION ALL "
        'sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'SA' AS `sumber`,`m3_sa`.`said` AS `idterkait`,`m3_sa`.`sanotransaksi` AS `noterkait`,`m3_sa`.`satgl` AS `tglterkait`,`m3_sa`.`sainputtgl` AS `inputtglterkait`,`m3_sa`.`samodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m3_sa_detail` on((`m3_sa_detail`.`customdbl2` = `sod`.`idsodetail`))) join `m3_sa` on((`m3_sa_detail`.`idsa` = `m3_sa`.`said`))) " & filter10 & "  group by `so`.`soid`, `m3_sa`.`said` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'TS' AS `sumber`,`m3_ts`.`tsid` AS `idterkait`,`m3_ts`.`tsnotransaksi` AS `noterkait`,`m3_ts`.`tstgl` AS `tglterkait`,`m3_ts`.`tsinputtgl` AS `inputtglterkait`,`m3_ts`.`tsmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m3_ts_detail` on((`m3_ts_detail`.`customdbl2` = `sod`.`idsodetail`))) join `m3_ts` on((`m3_ts_detail`.`idts` = `m3_ts`.`tsid`))) " & filter11 & "  group by `so`.`soid`, `m3_ts`.`tsid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'SO' AS `sumber`,so2.`soid` AS `idterkait`,so2.`sonotransaksi` AS `noterkait`,so2.`sotgl` AS `tglterkait`,so2.`soinputtgl` AS `inputtglterkait`,so2.`somodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_so_detail` sod2 on((sod2.`customdbl3` = `sod`.`idsodetail`))) join `m5_so` so2 on((sod2.`idso` = so2.`soid`))) " & filter12 & "  group by `so`.`soid`, so2.`soid` "
        sql &= " UNION ALL "
        sql &= "select `so`.`soid` AS `soid`,`so`.`sonotransaksi` AS `sonotransaksi`,'SO' AS `sumber`,so2.`soid` AS `idterkait`,so2.`sonotransaksi` AS `noterkait`,so2.`sotgl` AS `tglterkait`,so2.`soinputtgl` AS `inputtglterkait`,so2.`somodifikasitgl` AS `modifikasitglterkait`, 0 as jenisterkait from (((`m5_so_detail` `sod` join `m5_so` `so` on((`sod`.`idso` = `so`.`soid`))) join `m5_so_detail` sod2 on((sod2.`idsodetail` = `sod`.`customdbl3`))) join `m5_so` so2 on((sod2.`idso` = so2.`soid`))) " & filter12 & "  group by `so`.`soid`, so2.`soid` "

        Return sql
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstanding As String, ByVal ftOutstanding As String, ByVal ftSQ As String, ByVal ftExistOutstandingSO As String, ByVal ftOutstandingSO As String, ByVal ftSO As String, ByVal termasukPajak As String) As String
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

            'CEK SQ YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftSQ) > 0 Then
                sql = "SELECT sq.sqnotransaksi as notransaksi, (CASE sq.sqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sq.sqid WHERE " & ftSQ & " GROUP BY sq.sqhargatermasukpajak"
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
                    sql = "SELECT i.bkode, sqd.idsqdetail, sq.sqnotransaksi as notransaksi, (CASE sq.sqhargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_sq_detail sqd JOIN m5_sq sq ON sqd.idsq = sq.sqid JOIN m1_item i ON sqd.idbarang = i.bid WHERE (" & ftSQ & ") AND sq.sqhargatermasukpajak <> " & termasukPajak & " ORDER BY sqd.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "idsqdetail = " & dtval.Rows(0)("idsqdetail")
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

            ''PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            'sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            'dtval = AsDataTableAmbilDariDB(sql)
            'If dtval.Rows.Count > 0 Then
            '    'Ambil informasi utk errmessage
            '    kodebarang = dtval.Rows(0)("bkode")
            '    sisa = dtval.Rows(0)("sisarealisasi")

            '    filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
            '    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
            '    If dtLookup.Rows.Count > 0 Then
            '        tipebarang = dtLookup.Rows(0)("tipebarang")
            '        namabarang = dtLookup.Rows(0)("namabarang")
            '        satuan = dtLookup.Rows(0)("satuan")
            '        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
            '        urutan = dtLookup.Rows(0)("urutan")
            '    End If
            '    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            'End If
        End If


        If Len(ftExistOutstandingSO) > 0 Then 'ftExistOutstanding = rowExists, idsqdetail, bkode
            'CEK DATA EXIST/TIDAK
            dtval = AsDataTableAmbilDariDB(ftExistOutstandingSO)
            filterLookup = "rowExists = 0"
            dtval = AsDataTableFilterLimit(dtval, filterLookup, , , 1)
            If dtval.Rows.Count > 0 Then
                'Ambil informasi utk errmessage
                kodebarang = dtval.Rows(0)("bkode")

                filterLookup = "customdbl3=" & dtval.Rows(0)("idsodetail")
                dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)

                tipebarang = dtLookup.Rows(0)("tipebarang")
                namabarang = dtLookup.Rows(0)("namabarang")
                urutan = dtLookup.Rows(0)("urutan")

                errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " doesn't exists/yet approved in SO" : GoTo selesai
            End If

            'CEK SO YANG DIAMBIL
            'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            If Len(ftSO) > 0 Then
                sql = "SELECT so.sonotransaksi as notransaksi, (CASE so.sohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid WHERE " & ftSQ & " GROUP BY so.sohargatermasukpajak"
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
                    sql = "SELECT i.bkode, sod.idsodetail, so.sonotransaksi as notransaksi, (CASE so.sohargatermasukpajak WHEN 0 THEN '(Exclude Tax)' ELSE '(Include Tax)' END) as termasukpajak FROM m5_so_detail sod JOIN m5_so so ON sod.idso = so.soid JOIN m1_item i ON sod.idbarang = i.bid WHERE (" & ftSO & ") AND so.sohargatermasukpajak <> " & termasukPajak & " ORDER BY sod.urutan"
                    dtval = AsDataTableAmbilDariDB(sql)
                    If dtval.Rows.Count > 0 Then
                        'Ambil informasi utk errmessage
                        kodebarang = dtval.Rows(0)("bkode")

                        filterLookup = "customdbl3 = " & dtval.Rows(0)("idsodetail")
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

            ''PERBANDINGAN ANTARA JMLBARANG YG DIAMBIL DAN SISA OUTSTANDING YG TERSEDIA
            'sql = "SELECT sqd.idsqdetail, (sqd.jmlbarang - sqd.jmlrealisasi) as sisarealisasi, i.bid, i.bkode FROM m5_sq_detail AS sqd INNER JOIN m1_item AS i ON sqd.idbarang = i.bid WHERE " & ftOutstanding
            'dtval = AsDataTableAmbilDariDB(sql)
            'If dtval.Rows.Count > 0 Then
            '    'Ambil informasi utk errmessage
            '    kodebarang = dtval.Rows(0)("bkode")
            '    sisa = dtval.Rows(0)("sisarealisasi")

            '    filterLookup = "idsqdetail=" & dtval.Rows(0)("idsqdetail")
            '    dtLookup = AsDataTableFilterLimit(dtdetail, filterLookup, , , 1)
            '    If dtLookup.Rows.Count > 0 Then
            '        tipebarang = dtLookup.Rows(0)("tipebarang")
            '        namabarang = dtLookup.Rows(0)("namabarang")
            '        satuan = dtLookup.Rows(0)("satuan")
            '        nilaiSatuan = dtLookup.Rows(0)("nilaiSatuan")
            '        urutan = dtLookup.Rows(0)("urutan")
            '    End If
            '    errmessage = "Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " exceeds the number of items in SQ, item(s) available " & sisa / nilaiSatuan & " " & satuan : GoTo selesai
            'End If
        End If
        'END OF VALIDASI OUTSTANDING --------------------------------
selesai:
        Return errmessage
    End Function

    <WebMethod()>
    Public Function M5_SoSimpanOld(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim Filter As String = "", Sorting As String = ""

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


        'CEK NOREFF ========================================================================
        'CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        'JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        'JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        If Len(Filter) > 0 Then
            sql = "SELECT soid, sonotransaksi FROM m5_so WHERE sonoref = '" & FixQuotes(Filter) & "'"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("soid")) > 0 Then
                    result(1) = 1
                    result(2) = dtNoreff.Rows(0)("sonotransaksi")
                    result(3) = 0
                    result(4) = dtNoreff.Rows(0)("soid")
                    GoTo selesai
                End If
            End If

        Else
            Dim validKey As RsValidKey
            validKey = ValidateKey(paramSplit(0))
            If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        End If
        'END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'soid(0) As Integer, socabang(1) As String, solokasi(2) As String, sogudang(3) As String, soasalbarang(4) As String, 
        'soasalbarangkategori(5) As Integer, sojenispenjualan(6) As String, sojenispenjualankategori(7) As Integer, socarabayar(8) As Integer, sosumber(9) As String, 
        'soautonotransaksi(10) As Integer, sonotransaksi(11) As String, sotgl(12) As Date, sokodepa(13) As Integer, socustomer(14) As Integer, 
        'socustomerkontak(15) As String, so1alamat1(16) As String, so1alamat2(17) As String, so1alamat3(18) As String, so2alamat1(19) As String, 
        'so2alamat2(20) As String, so2alamat3(21) As String, sobagianpenjualan(22) As Integer, soekspedisi(23) As String, sotglkirim(24) As Date, 
        'sotermin(25) As String, sotgljatuhtempo(26) As Date, souraian(27) As String, socatatan(28) As String, sonoref(29) As String, 
        'sotglnoref(30) As Date, sotglpenutupan(31) As Date, somatauang(32) As String, sokurs(33) As Double, sohargatermasukpajak(34) As Integer, 
        'sototal(35) As Double, sodiskonpersen(36) As String, sojmldiskon(37) As Double, sototalpajak1detail(38) As Double, sototalpajak2detail(39) As Double, 
        'sobiayalainpersen(40) As Double, sobiayalain(41) As Double, sototaltransaksi(42) As Double, sojmlbayar(43) As Double, sorekdiskon(44) As String, 
        'sorekpajak1(45) As String, sorekpajak2(46) As String, sorekbiayalain(47) As String, sorekbayar(48) As String, soidsq(49) As Integer, 
        'sostatuspl(50) As Integer, sostatusdo(51) As Integer, sostatusdr(52) As Integer, sostatuspi(53) As Integer, sostatussi(54) As Integer, 
        'sostatusrnr(55) As Integer, sostatussr(56) As Integer, sostatus(57) As Integer, sostatussebelumnya(58) As Integer, sojmlrevisi(59) As Integer, 
        'socetakanke(60) As Integer, soinputuser(61) As Integer, soinputtgl(62) As DateTime, somodifikasiuser(63) As Integer, somodifikasitgl(64) As DateTime, 
        'soisclose(65) As Integer, socustomtext1(66) As String, socustomtext2(67) As String, socustomtext3(68) As String, socustomtext4(69) As String, 
        'socustomtext5(70) As String, socustomint1(71) As Integer, socustomint2(72) As Integer, socustomint3(73) As Integer, socustomdbl1(74) As Double, 
        'socustomdbl2(75) As Double, socustomdbl3(76) As Double, socustomdate1(77) As Date, socustomdate2(78) As Date, socustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, 
        'somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, 
        'socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, 
        'socustomdate1, socustomdate2, socustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'soid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "soid required numeric." : GoTo selesai
        End If
        'soasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "soasalbarangkategori required numeric." : GoTo selesai
        End If
        'sojenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "sojenispenjualankategori required numeric." : GoTo selesai
        End If
        'socarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "socarabayar required numeric." : GoTo selesai
        End If
        'soautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "soautonotransaksi required numeric." : GoTo selesai
        End If
        'sotgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "sotgl required date." : GoTo selesai
        End If
        'sokodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "sokodepa required numeric." : GoTo selesai
        End If
        'socustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "socustomer required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "socustomer can't be empty." : GoTo selesai
        End If
        'sobagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "sobagianpenjualan required numeric." : GoTo selesai
        End If
        If (dataUtama(22) < 1) Then
            result(2) = "sobagianpenjualan can't be empty." : GoTo selesai
        End If
        'sotglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "sotglkirim required date." : GoTo selesai
        End If
        'sotgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "sotgljatuhtempo required date." : GoTo selesai
        End If
        'sotglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "sotglnoref required date." : GoTo selesai
        End If
        'sotglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "sotglpenutupan required date." : GoTo selesai
        End If
        'sokurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "sokurs required numeric." : GoTo selesai
        End If
        'sohargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "sohargatermasukpajak required numeric." : GoTo selesai
        End If
        'sototal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "sototal required numeric." : GoTo selesai
        End If
        'sojmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "sojmldiskon required numeric." : GoTo selesai
        End If
        'sototalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "sototalpajak1detail required numeric." : GoTo selesai
        End If
        'sototalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "sototalpajak2detail required numeric." : GoTo selesai
        End If
        ''sobiayalainpersen(40) As Double
        'If (IsNumeric(dataUtama(40)) = False) Then
        '    result(2) = "sobiayalainpersen required numeric." : GoTo selesai
        'End If
        'sobiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "sobiayalain required numeric." : GoTo selesai
        End If
        'sototaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "sototaltransaksi required numeric." : GoTo selesai
        End If
        'sojmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "sojmlbayar required numeric." : GoTo selesai
        End If
        'soidsq(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "soidsq required numeric." : GoTo selesai
        End If
        'sostatuspl(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "sostatuspl required numeric." : GoTo selesai
        End If
        'sostatusdo(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "sostatusdo required numeric." : GoTo selesai
        End If
        'sostatusdr(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "sostatusdr required numeric." : GoTo selesai
        End If
        'sostatuspi(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "sostatuspi required numeric." : GoTo selesai
        End If
        'sostatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "sostatussi required numeric." : GoTo selesai
        End If
        'sostatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "sostatusrnr required numeric." : GoTo selesai
        End If
        'sostatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "sostatussr required numeric." : GoTo selesai
        End If
        'sostatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "sostatus required numeric." : GoTo selesai
        End If
        'sostatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "sostatussebelumnya required numeric." : GoTo selesai
        End If
        'sojmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "sojmlrevisi required numeric." : GoTo selesai
        End If
        'socetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "socetakanke required numeric." : GoTo selesai
        End If
        'soinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "soinputuser required numeric." : GoTo selesai
        End If
        'soinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "soinputtgl required date." : GoTo selesai
        End If
        'somodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "somodifikasiuser required numeric." : GoTo selesai
        End If
        'somodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "somodifikasitgl required date." : GoTo selesai
        End If
        'soisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "soisclose required numeric." : GoTo selesai
        End If
        'socustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "socustomint1 required numeric." : GoTo selesai
        End If
        'socustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "socustomint2 required numeric." : GoTo selesai
        End If
        'socustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "socustomint3 required numeric." : GoTo selesai
        End If
        'socustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "socustomdbl1 required numeric." : GoTo selesai
        End If
        'socustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "socustomdbl2 required numeric." : GoTo selesai
        End If
        'socustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "socustomdbl3 required numeric." : GoTo selesai
        End If
        'socustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "socustomdate1 required date." : GoTo selesai
        End If
        'socustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "socustomdate2 required date." : GoTo selesai
        End If
        'socustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "socustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'socabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "socabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "socabang should not be more than 25 character." : GoTo selesai
        End If

        'solokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "solokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "solokasi should not be more than 25 character." : GoTo selesai
        End If

        'sogudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "sogudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "sogudang should not be more than 25 character." : GoTo selesai
        End If

        'sosumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "sosumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "sosumber should not be more than 10 character." : GoTo selesai
        End If

        'sonotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "sonotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "sonotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'sotgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "sotgl can't be empty" : GoTo selesai
        End If

        'sotglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "sotglkirim can't be empty" : GoTo selesai
        End If

        'sotgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "sotgljatuhtempo can't be empty" : GoTo selesai
        End If

        'sotglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "sotglnoref can't be empty" : GoTo selesai
        End If

        'sotglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "sotglpenutupan can't be empty" : GoTo selesai
        End If

        'somatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "somatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "somatauang should not be more than 25 character." : GoTo selesai
        End If

        'sokurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "sokurs can't be empty" : GoTo selesai
        End If

        'sototal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "sototal can't be empty" : GoTo selesai
        End If

        'sodiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "sodiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "sodiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'sojmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "sojmldiskon can't be empty" : GoTo selesai
        End If

        'sototalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "sototalpajak1detail can't be empty" : GoTo selesai
        End If

        'sototalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "sototalpajak2detail can't be empty" : GoTo selesai
        End If

        'sobiayalainpersen(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "sobiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "sobiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'sobiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "sobiayalain can't be empty" : GoTo selesai
        End If

        'sototaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "sototaltransaksi can't be empty" : GoTo selesai
        End If

        'sojmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "sojmlbayar can't be empty" : GoTo selesai
        End If

        'soinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "soinputtgl can't be empty" : GoTo selesai
        End If

        'somodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "somodifikasitgl can't be empty" : GoTo selesai
        End If

        'socustomtext1(66) As String
        'CUSTOM TEXT DIISI DARI SOCATATAN(28) + SOTGLKIRIM(24)
        dataUtama(66) = String.Concat(dataUtama(28), Replace(dataUtama(24), "-", ""))

        'socustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "socustomdbl1 can't be empty" : GoTo selesai
        End If

        'socustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "socustomdbl2 can't be empty" : GoTo selesai
        End If

        'socustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "socustomdbl3 can't be empty" : GoTo selesai
        End If

        'socustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "socustomdate1 can't be empty" : GoTo selesai
        End If

        'socustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "socustomdate2 can't be empty" : GoTo selesai
        End If

        'socustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "socustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "soid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "solokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "so2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "souraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "sorekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "sojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "soinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "somodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "somodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "soisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "socustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "socustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "soid~socabang~solokasi~sogudang~soasalbarang~soasalbarangkategori~sojenispenjualan~sojenispenjualankategori~socarabayar~sosumber~soautonotransaksi~sonotransaksi~sotgl~sokodepa~socustomer~socustomerkontak~so1alamat1~so1alamat2~so1alamat3~so2alamat1~so2alamat2~so2alamat3~sobagianpenjualan~soekspedisi~sotglkirim~sotermin~sotgljatuhtempo~souraian~socatatan~sonoref~sotglnoref~sotglpenutupan~somatauang~sokurs~sohargatermasukpajak~sototal~sodiskonpersen~sojmldiskon~sototalpajak1detail~sototalpajak2detail~sobiayalainpersen~sobiayalain~sototaltransaksi~sojmlbayar~sorekdiskon~sorekpajak1~sorekpajak2~sorekbiayalain~sorekbayar~soidsq~sostatuspl~sostatusdo~sostatusdr~sostatuspi~sostatussi~sostatusrnr~sostatussr~sostatus~sostatussebelumnya~sojmlrevisi~socetakanke~soinputuser~soinputtgl~somodifikasiuser~somodifikasitgl~soisclose~socustomtext1~socustomtext2~socustomtext3~socustomtext4~socustomtext5~socustomint1~socustomint2~socustomint3~socustomdbl1~socustomdbl2~socustomdbl3~socustomdate1~socustomdate2~socustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idsodetail(0) As Integer, idso(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
        'jml(5) As Double, satuan(6) As String, nilaisatuan(7) As Double, jmlbarang(8) As Double, satuanbarang(9) As String, 
        'matauang(10) As String, kurs(11) As Double, harga(12) As Double, diskon(13) As String, jmldiskon(14) As Double, 
        'pajak1(15) As String, jmlpajak1(16) As Double, pajak2(17) As String, jmlpajak2(18) As Double, cabang(19) As String, 
        'lokasi(20) As String, gudang(21) As String, costcenter(22) As String, divisi(23) As String, subdivisi(24) As String, 
        'proyek(25) As String, catatan(26) As String, urutan(27) As Integer, idsqdetail(28) As Integer, jmlpl(29) As Double, 
        'statuspl(30) As Integer, jmldo(31) As Double, statusdo(32) As Integer, jmldr(33) As Double, statusdr(34) As Integer, 
        'jmlpi(35) As Double, statuspi(36) As Integer, jmlsi(37) As Double, statussi(38) As Integer, jmlrnr(39) As Double, 
        'statusrnr(40) As Integer, jmlsr(41) As Double, statussr(42) As Integer, isclose(43) As Integer, customtext1(44) As String, 
        'customtext2(45) As String, customtext3(46) As String, customdbl1(47) As Double, customdbl2(48) As Double, customdbl3(49) As Double, 
        'customdate1(50) As Date, customdate2(51) As Date, customdate3(52) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idso", AsEnumTypeData.AsInt64)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrnr", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokBooking As String = ""
        Dim idbarang As Integer = 0, idsqdetail As Integer = 0, jmlbarang As Double = 0

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSQ As String = ""

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
            'idsodetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idso(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idso required numeric." : GoTo selesai
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
            'harga(12) As Double
            If (IsNumeric(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(14) As Double
            If (IsNumeric(dataRowDetail(14)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(16) As Double
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(18) As Double
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(27) As Integer
            If (IsNumeric(dataRowDetail(27)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(28) As Integer
            If (IsNumeric(dataRowDetail(28)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlpl(29) As Double
            If (IsNumeric(dataRowDetail(29)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(30) As Integer
            If (IsNumeric(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(31) As Double
            If (IsNumeric(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(32) As Integer
            If (IsNumeric(dataRowDetail(32)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(36) As Integer
            If (IsNumeric(dataRowDetail(36)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(38) As Integer
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(40) As Integer
            If (IsNumeric(dataRowDetail(40)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(42) As Integer
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
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

            'harga(12) As Double
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(13) As Double
            If Len(dataRowDetail(13)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(13)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(14) As Double
            If Len(dataRowDetail(14)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(5) As Double, harga(12) As Double, diskon(13) As String
                dataRowDetail(14) = F_Diskon(Double.Parse(dataRowDetail(5)), Double.Parse(dataRowDetail(12)), FixQuotes(dataRowDetail(13).ToString))
            End If

            'jmlpajak1(16) As Double
            If Len(dataRowDetail(16)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(18) As Double
            If Len(dataRowDetail(18)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlpl(29) As Double
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(31) As Double
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idsodetail~idso~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(2) As Integer     , jmlbarang(8) As Double       , gudang(21) As String       , idsqdetail(28) As Integer
            idbarang = dataRowDetail(2) : jmlbarang = dataRowDetail(8) : gudang = dataRowDetail(21) : idsqdetail = dataRowDetail(28)
            'kurs(11) As Double                    , harga(12) As Double
            kurs = Double.Parse(dataRowDetail(11)) : harga = Double.Parse(dataRowDetail(12))

            'VALIDASI OUTSTANDING -------------------------
            If idsqdetail <> 0 Then 'SQ
                'CEK SQ YANG DIAMBIL
                'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
                ftSQ = IIf(Len(ftSQ.ToString) = 0, "", ftSQ & " OR ")
                ftSQ = String.Concat(ftSQ, " (sqd.idsqdetail = " & idsqdetail & ") ")

                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi + '" & Outstanding & "', 5) ", updNilai)

                '4. SET FILTER UPDATE OUTSTANDING
                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            End If

            ''5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            'Validasi harga dibawah harga jual
            ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


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

                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sotgl")), AsFormatTanggal(drutama("sotgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                'ValidasiSimpan
                If drutama("sostatus") = 2 Then
                    'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                    Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetail, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                    If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding, ftSQ, "", "", "", drutama("sohargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH TEMPO ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("sotermin").ToString, AsFormatTanggal(drutama("sotgl")), "sotgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("sotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH TEMPO =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("sototal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("sototalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("sototalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak1detail")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sobiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("soid")
                    notransaksi = drutama("sonotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(soid), sonotransaksi FROM M5_so WHERE soid='" & result(4) & "' AND sostatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_so_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sosumber")) & "▼" & FixQuotes(drutama("soid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_So set socabang  = '" & FixQuotes(drutama("socabang")) & "', solokasi  = '" & FixQuotes(drutama("solokasi")) & "', sogudang  = '" & FixQuotes(drutama("sogudang")) & "', soasalbarang  = '" & FixQuotes(drutama("soasalbarang")) & "', soasalbarangkategori  = " & drutama("soasalbarangkategori") & ", sojenispenjualan  = '" & FixQuotes(drutama("sojenispenjualan")) & "', sojenispenjualankategori  = " & drutama("sojenispenjualankategori") & ", socarabayar  = " & drutama("socarabayar") & ", sosumber  = '" & FixQuotes(drutama("sosumber")) & "', soautonotransaksi  = " & drutama("soautonotransaksi") & ", sonotransaksi  = '" & FixQuotes(notransaksi) & "', sotgl  = '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', sokodepa  = " & drutama("sokodepa") & ", socustomer  = " & drutama("socustomer") & ", socustomerkontak  = '" & FixQuotes(drutama("socustomerkontak")) & "', so1alamat1  = '" & FixQuotes(drutama("so1alamat1")) & "', so1alamat2  = '" & FixQuotes(drutama("so1alamat2")) & "', so1alamat3  = '" & FixQuotes(drutama("so1alamat3")) & "', so2alamat1  = '" & FixQuotes(drutama("so2alamat1")) & "', so2alamat2  = '" & FixQuotes(drutama("so2alamat2")) & "', so2alamat3  = '" & FixQuotes(drutama("so2alamat3")) & "', sobagianpenjualan  = " & drutama("sobagianpenjualan") & ", soekspedisi  = '" & FixQuotes(drutama("soekspedisi")) & "', sotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', sotermin  = '" & FixQuotes(drutama("sotermin")) & "', sotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', souraian  = '" & FixQuotes(drutama("souraian")) & "', socatatan  = '" & FixQuotes(drutama("socatatan")) & "', sonoref  = '" & FixQuotes(drutama("sonoref")) & "', sotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', sotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', somatauang  = '" & FixQuotes(drutama("somatauang")) & "', sokurs  = '" & FixDouble(drutama("sokurs")) & "', sohargatermasukpajak  = " & drutama("sohargatermasukpajak") & ", sototal  = '" & FixDouble(drutama("sototal")) & "', sodiskonpersen  = '" & FixQuotes(drutama("sodiskonpersen")) & "', sojmldiskon  = '" & FixDouble(drutama("sojmldiskon")) & "', sototalpajak1detail  = '" & FixDouble(drutama("sototalpajak1detail")) & "', sototalpajak2detail  = '" & FixDouble(drutama("sototalpajak2detail")) & "', sobiayalainpersen  = '" & FixDouble(drutama("sobiayalainpersen")) & "', sobiayalain  = '" & FixDouble(drutama("sobiayalain")) & "', sototaltransaksi  = '" & FixDouble(drutama("sototaltransaksi")) & "', sojmlbayar  = '" & FixDouble(drutama("sojmlbayar")) & "', sorekdiskon  = '" & FixQuotes(drutama("sorekdiskon")) & "', sorekpajak1  = '" & FixQuotes(drutama("sorekpajak1")) & "', sorekpajak2  = '" & FixQuotes(drutama("sorekpajak2")) & "', sorekbiayalain  = '" & FixQuotes(drutama("sorekbiayalain")) & "', sorekbayar  = '" & FixQuotes(drutama("sorekbayar")) & "', soidsq  = " & drutama("soidsq") & ", sostatuspl  = " & drutama("sostatuspl") & ", sostatusdo  = " & drutama("sostatusdo") & ", sostatusdr  = " & drutama("sostatusdr") & ", sostatuspi  = " & drutama("sostatuspi") & ", sostatussi  = " & drutama("sostatussi") & ", sostatusrnr  = " & drutama("sostatusrnr") & ", sostatussr  = " & drutama("sostatussr") & ", sostatus  = " & drutama("sostatus") & ", sostatussebelumnya  = " & drutama("sostatussebelumnya") & ", sojmlrevisi  = sojmlrevisi+1, socetakanke  = " & drutama("socetakanke") & ", somodifikasiuser  = " & drutama("somodifikasiuser") & ", somodifikasitgl  = NOW(), socustomtext1  = '" & FixQuotes(drutama("socustomtext1")) & "', socustomtext2  = '" & FixQuotes(drutama("socustomtext2")) & "', socustomtext3  = '" & FixQuotes(drutama("socustomtext3")) & "', socustomtext4  = '" & FixQuotes(drutama("socustomtext4")) & "', socustomtext5  = '" & FixQuotes(drutama("socustomtext5")) & "', socustomint1  = " & drutama("socustomint1") & ", socustomint2  = " & drutama("socustomint2") & ", socustomint3  = " & drutama("socustomint3") & ", socustomdbl1  = '" & FixDouble(drutama("socustomdbl1")) & "', socustomdbl2  = '" & FixDouble(drutama("socustomdbl2")) & "', socustomdbl3  = '" & FixDouble(drutama("socustomdbl3")) & "', socustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', socustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', socustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "' where soid = '" & drutama("soid") & "'"
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

                    If drutama("soautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"))
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
                        notransaksi = drutama("sonotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

					'Dim wsM0_NomorPDR As New m0_nomor
                    'Dim rsNotransaksiPDR As String = wsM0_NomorPDR.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), "PDR", drutama("sotgl"))
                    'Dim arrNotransaksiPDR(4) As String 'success(0), errmessage(1), notransaksi(2), sql(3)
                    'arrNotransaksiPDR = rsNotransaksiPDR.Split(sptSubParam)
                    'cek success generate notransaksi
                    'If (arrNotransaksiPDR(0) = 1) Then
                    '    notransaksiPDR = arrNotransaksiPDR(2)
                        'tambah query update m0_nomor_next
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = Con1
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = arrNotransaksiPDR(3)
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    'Else
                    '    result(2) = arrNotransaksiPDR(1) : Trans.Rollback() : GoTo selesai
                    'End If
                    'END OF GENERATE NOTRANSAKSI PDR ==================================
					
                    sql = "Insert into M5_So (socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3) values('" & FixQuotes(drutama("socabang")) & "', '" & FixQuotes(drutama("solokasi")) & "', '" & FixQuotes(drutama("sogudang")) & "', '" & FixQuotes(drutama("soasalbarang")) & "', " & drutama("soasalbarangkategori") & ", '" & FixQuotes(drutama("sojenispenjualan")) & "', " & drutama("sojenispenjualankategori") & ", " & drutama("socarabayar") & ", '" & FixQuotes(drutama("sosumber")) & "', " & drutama("soautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', " & drutama("sokodepa") & ", " & drutama("socustomer") & ", '" & FixQuotes(drutama("socustomerkontak")) & "', '" & FixQuotes(drutama("so1alamat1")) & "', '" & FixQuotes(drutama("so1alamat2")) & "', '" & FixQuotes(drutama("so1alamat3")) & "', '" & FixQuotes(drutama("so2alamat1")) & "', '" & FixQuotes(drutama("so2alamat2")) & "', '" & FixQuotes(drutama("so2alamat3")) & "', " & drutama("sobagianpenjualan") & ", '" & FixQuotes(drutama("soekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', '" & FixQuotes(drutama("sotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', '" & FixQuotes(drutama("souraian")) & "', '" & FixQuotes(drutama("socatatan")) & "', '" & FixQuotes(drutama("sonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', '" & FixQuotes(drutama("somatauang")) & "', '" & FixDouble(drutama("sokurs")) & "', " & drutama("sohargatermasukpajak") & ", '" & FixDouble(drutama("sototal")) & "', '" & FixQuotes(drutama("sodiskonpersen")) & "', '" & FixDouble(drutama("sojmldiskon")) & "', '" & FixDouble(drutama("sototalpajak1detail")) & "', '" & FixDouble(drutama("sototalpajak2detail")) & "', '" & FixDouble(drutama("sobiayalainpersen")) & "', '" & FixDouble(drutama("sobiayalain")) & "', '" & FixDouble(drutama("sototaltransaksi")) & "', '" & FixDouble(drutama("sojmlbayar")) & "', '" & FixQuotes(drutama("sorekdiskon")) & "', '" & FixQuotes(drutama("sorekpajak1")) & "', '" & FixQuotes(drutama("sorekpajak2")) & "', '" & FixQuotes(drutama("sorekbiayalain")) & "', '" & FixQuotes(drutama("sorekbayar")) & "', " & drutama("soidsq") & ", " & drutama("sostatuspl") & ", " & drutama("sostatusdo") & ", " & drutama("sostatusdr") & ", " & drutama("sostatuspi") & ", " & drutama("sostatussi") & ", " & drutama("sostatusrnr") & ", " & drutama("sostatussr") & ", " & drutama("sostatus") & ", " & drutama("sostatussebelumnya") & ", " & drutama("sojmlrevisi") & ", " & drutama("socetakanke") & ", " & drutama("soinputuser") & ", NOW(), " & drutama("somodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("soisclose") & ", '" & FixQuotes(drutama("socustomtext1")) & "', '" & FixQuotes(drutama("socustomtext2")) & "', '" & FixQuotes(drutama("socustomtext3")) & "', '" & FixQuotes(drutama("socustomtext4")) & "', '" & FixQuotes(drutama("socustomtext5")) & "', " & drutama("socustomint1") & ", " & drutama("socustomint2") & ", " & drutama("socustomint3") & ", '" & FixDouble(drutama("socustomdbl1")) & "', '" & FixDouble(drutama("socustomdbl2")) & "', '" & FixDouble(drutama("socustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select soid from M5_so where sonotransaksi='" & notransaksi & "' AND soinputuser= '" & userid & "' order by somodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M5_So_Detail where idso = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idsodetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M5_So_Detail(idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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

                If drutama("sostatus") = 2 Then
                    If Len(updNilai) > 0 Then
                        'UPDATE OUTSTANDING TRANSAKSI =======================================================
                        'UPDATE DETAIL
                        sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                        Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                        If dtOut.Rows.Count > 0 Then
                            For Each dr1 As DataRow In dtOut.Rows
                                ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                            Next
                        End If
                        dtOut = AsDataTableAmbilDariDB("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                        If dtOut.Rows.Count > 0 Then
                            'KOSONGKAN VARIABEL NILAI DAN FILTER
                            updNilai = "" : updFilter = ""
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
                                updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                '3. SET FILTERUPDATE OUTSTANDING
                                updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                            Next

                            sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = Con1
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If
                        'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                    End If

                    'UPDATE STOK BOOKING ================================================================
                    'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                    sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'If Len(updStokBooking) > 0 Then
                    '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                    '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    '    With objCmd
                    '        .Connection = Con1
                    '        .Transaction = Trans
                    '        .CommandType = CommandType.Text
                    '        .CommandText = sql
                    '    End With
                    '    objCmd.ExecuteNonQuery()
                    'End If
                    'END OF UPDATE STOK BOOKING =========================================================

                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "SO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_SoUpdateStatusOld(ByVal param As String) As String
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
            Dim sumber As String = "So", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Sotgl, Sonotransaksi, Sostatus FROM M5_So WHERE Soid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Sostatussebelumnya" : jnsaktivitas = 17
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

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m5_so_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
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
                sql = query.m5_so_terkait("soid = '" & idtransaksi & "'")
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idsqdetail As Integer = 0
                Dim updNilai As String = "", updFilter As String = "", gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idsqdetail, urutan FROM m5_so_detail WHERE idso = '" & idtransaksi & "'")
                If dtdetail.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dtdetail.Rows
                        'BUAT FILTER UNTUK UPDATE ---------------------------------
                        idbarang = dr1("idbarang") : jmlbarang = dr1("jmlbarang") : gudang = dr1("gudang") : idsqdetail = dr1("idsqdetail")

                        'UPDATE OUTSTANDING ---------------------------
                        If idsqdetail <> 0 Then
                            '1. SET NILAI UPDATE OUTSTANDING
                            Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                            updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN ROUND(jmlrealisasi - '" & Outstanding & "', 5) ", updNilai)

                            '2. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
                        End If

                        ''3. SET NILAI UPDATE STOK KELUAR -------------
                        'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
                        'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('-" & jmlbarang & "'))") ' idbarang, kgudang, stok

                        'END OF BUAT FILTER UNTUK UPDATE --------------------------
                    Next
                Else
                    result(2) = "Detail transaction not found." : Trans.Rollback() : GoTo selesai
                End If

                If Len(updFilter) > 0 Then
                    'UPDATE OUTSTANDING DETAIL ----------------------
                    sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                    Dim dtOut As DataTable = AsDataTableAmbilDariDB("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq")
                    If dtOut.Rows.Count > 0 Then
                        For Each dr1 As DataRow In dtOut.Rows
                            ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                            ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                        Next
                    End If
                    dtOut = AsDataTableAmbilDariDB("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq")
                    If dtOut.Rows.Count > 0 Then
                        'KOSONGKAN VARIABEL NILAI DAN FILTER
                        updNilai = "" : updFilter = ""
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
                            updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                            '3. SET FILTERUPDATE OUTSTANDING
                            updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                            updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                        Next

                        sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
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

                'UPDATE STOK BOOKING ================================
                'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang * -1 FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & idtransaksi & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'If Len(updStokBooking) > 0 Then
                '    sql = "INSERT INTO m1_item_booking (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '    With objCmd
                '        .Connection = Con1
                '        .Transaction = Trans
                '        .CommandType = CommandType.Text
                '        .CommandText = sql
                '    End With
                '    objCmd.ExecuteNonQuery()
                'End If
                'END OF UPDATE STOK BOOKING =========================

            End If


            'JIKA CLOSE MAKA KURANGI STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            If jnsaktivitas = 7 Then
                'KURANGI STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking - (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = Con1
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'JIKA UNCLOSE MAKA TAMBAH STOK BOOKING SESUAI JMLBARANG YG OUTSTANDING
            If jnsaktivitas = 17 Then
                'TAMBAH STOK BOOKING SESUAI JMLBARANG - REALISASI DO - REALISASI SI
                sql = "  UPDATE m1_item_booking ib"
                sql &= " JOIN"
                sql &= " (SELECT idsodetail, idbarang, jmlbarang, SUM(realisasi) as realisasi"
                sql &= " FROM ( "
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(dod.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_do `do` "
                sql &= " LEFT JOIN m5_do_detail dod ON dod.iddo = `do`.doid AND `do`.dostatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = dod.idsodetail  "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " UNION ALL"
                sql &= " (SELECT sod.idsodetail, sod.idbarang, sod.jmlbarang, SUM(IFNULL(sid.jmlbarang,0)) as realisasi "
                sql &= " FROM m5_si si "
                sql &= " LEFT JOIN m5_si_detail sid ON sid.idsi = si.siid  AND sid.iddodetail = 0 AND sid.iddrdetail = 0 AND si.sistatus IN(2,3,4,7) "
                sql &= " RIGHT JOIN m5_so_detail sod ON sod.idsodetail = sid.idsodetail "
                sql &= " WHERE "
                sql &= " sod.idso = '" & FixDouble(idtransaksi) & "'"
                sql &= " GROUP BY sod.idsodetail)"
                sql &= " ) as detail"
                sql &= " GROUP BY idsodetail"
                sql &= " ) sod  ON ib.idbarang = sod.idbarang"
                sql &= " JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' "
                sql &= " SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.realisasi)"
                sql &= " WHERE sod.jmlbarang <> sod.realisasi"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'sql = "UPDATE m1_item_booking ib JOIN m5_so_detail sod ON ib.idbarang = sod.idbarang JOIN m1_item i ON sod.idbarang = i.bid AND i.bjenis <> 'J' AND i.bhpp <> 'I' SET ib.jmlbooking = ib.jmlbooking + (sod.jmlbarang - sod.jmlrealisasi) WHERE sod.idso = '" & FixDouble(idtransaksi) & "' AND sod.statusrealisasi <> 2"
                'objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                'With objCmd
                '    .Connection = Con1
                '    .Transaction = Trans
                '    .CommandType = CommandType.Text
                '    .CommandText = sql
                'End With
                'objCmd.ExecuteNonQuery()
            End If

            'update status utama
            sql = "UPDATE M5_So SET Sostatus = " & nilaiStatus & ", Somodifikasiuser='" & userid & "', Somodifikasitgl = NOW(), Soposting = 0, Sopostingtgl = '1971-01-01 00:00:00', Sojmlrevisi = Sojmlrevisi + 1 WHERE Soid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_soSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_SoDeleteOld(ByVal param As String) As String

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
            Dim sumber As String = "So", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Soid, Sonotransaksi FROM M5_So WHERE Soid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT socabang, solokasi, sosumber, soautonotransaksi, sonotransaksi, sotgl"
            sql &= " FROM M5_so"
            sql &= " WHERE soid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("socabang")
                lokasi = dtNomorNext.Rows(0)("solokasi")
                sumber = dtNomorNext.Rows(0)("sosumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("soautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("sonotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("sotgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE DETAIL
            sql = "DELETE FROM M5_So_Detail WHERE idso = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M5_So WHERE soid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_SoSearch(PostWsSearch(paramSplit(0), "M5_SoSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_SoImport(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        'Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String
        Dim dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim Filter As String = "", Sorting As String = ""

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


        ''CEK NOREFF ========================================================================
        ''CEK NOREFF UNTUK UPLOAD DATA POS, JIKA NOREFF TERISI MAKA CEK DATA YANG SUDAH ADA DI TABEL
        ''JIKA NOREFF SUDAH ADA MAKA BERI KEMBALIAN BERHASIL
        ''JIKA NOREF TIDAK ADA MAKA JALANKAN PROSES SIMPAN
        'If Len(Filter) > 0 Then
        '    sql = "SELECT soid, sonotransaksi FROM m5_so WHERE sonoref = '" & FixQuotes(Filter) & "'"
        '    Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
        '    If dtNoreff.Rows.Count > 0 Then
        '        If Len(dtNoreff.Rows(0)("soid")) > 0 Then
        '            result(1) = 1
        '            result(2) = dtNoreff.Rows(0)("sonotransaksi")
        '            result(3) = 0
        '            result(4) = dtNoreff.Rows(0)("soid")
        '            GoTo selesai
        '        End If
        '    End If

        '    'Else
        '    'Dim validKey As RsValidKey
        '    'validKey = ValidateKey(paramSplit(0))
        '    'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        'End If
        ''END OF CEK NOREFF =================================================================


        'VALIDASI DAN SET DATA =============================================================
        'dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        ''CEK ARRAY DATA
        'If (dataSplit.Length <> 2) Then
        '    result(2) = "Invalid transaction data parameter." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'soid(0) As Integer, socabang(1) As String, solokasi(2) As String, sogudang(3) As String, soasalbarang(4) As String, 
        'soasalbarangkategori(5) As Integer, sojenispenjualan(6) As String, sojenispenjualankategori(7) As Integer, socarabayar(8) As Integer, sosumber(9) As String, 
        'soautonotransaksi(10) As Integer, sonotransaksi(11) As String, sotgl(12) As Date, sokodepa(13) As Integer, socustomer(14) As String, 
        'socustomerkontak(15) As String, so1alamat1(16) As String, so1alamat2(17) As String, so1alamat3(18) As String, so2alamat1(19) As String, 
        'so2alamat2(20) As String, so2alamat3(21) As String, sobagianpenjualan(22) As String, soekspedisi(23) As String, sotglkirim(24) As Date, 
        'sotermin(25) As String, sotgljatuhtempo(26) As Date, souraian(27) As String, socatatan(28) As String, sonoref(29) As String, 
        'sotglnoref(30) As Date, sotglpenutupan(31) As Date, somatauang(32) As String, sokurs(33) As Double, sohargatermasukpajak(34) As Integer, 
        'sototal(35) As Double, sodiskonpersen(36) As String, sojmldiskon(37) As Double, sototalpajak1detail(38) As Double, sototalpajak2detail(39) As Double, 
        'sobiayalainpersen(40) As Double, sobiayalain(41) As Double, sototaltransaksi(42) As Double, sojmlbayar(43) As Double, sorekdiskon(44) As String, 
        'sorekpajak1(45) As String, sorekpajak2(46) As String, sorekbiayalain(47) As String, sorekbayar(48) As String, soidsq(49) As Integer, 
        'sostatuspl(50) As Integer, sostatusdo(51) As Integer, sostatusdr(52) As Integer, sostatuspi(53) As Integer, sostatussi(54) As Integer, 
        'sostatusrnr(55) As Integer, sostatussr(56) As Integer, sostatus(57) As Integer, sostatussebelumnya(58) As Integer, sojmlrevisi(59) As Integer, 
        'socetakanke(60) As Integer, soinputuser(61) As Integer, soinputtgl(62) As DateTime, somodifikasiuser(63) As Integer, somodifikasitgl(64) As DateTime, 
        'soisclose(65) As Integer, socustomtext1(66) As String, socustomtext2(67) As String, socustomtext3(68) As String, socustomtext4(69) As String, 
        'socustomtext5(70) As String, socustomint1(71) As Integer, socustomint2(72) As Integer, socustomint3(73) As Integer, socustomdbl1(74) As Double, 
        'socustomdbl2(75) As Double, socustomdbl3(76) As Double, socustomdate1(77) As Date, socustomdate2(78) As Date, socustomdate3(79) As Date,
        'idsodetail(80) As Integer, idso(81) As Integer, idbarang(82) As String, namabarang(83) As String, tipebarang(84) As String, 
        'jml(85) As Double, satuan(86) As String, nilaisatuan(87) As Double, jmlbarang(88) As Double, satuanbarang(89) As String, 
        'matauang(90) As String, kurs(91) As Double, harga(92) As Double, diskon(93) As String, jmldiskon(94) As Double, 
        'pajak1(95) As String, jmlpajak1(96) As Double, pajak2(97) As String, jmlpajak2(98) As Double, cabang(99) As String, 
        'lokasi(100) As String, gudang(101) As String, costcenter(102) As String, divisi(103) As String, subdivisi(104) As String, 
        'proyek(105) As String, catatan(106) As String, urutan(107) As Integer, idsqdetail(108) As Integer, jmlpl(109) As Double, 
        'statuspl(110) As Integer, jmldo(111) As Double, statusdo(112) As Integer, jmldr(113) As Double, statusdr(114) As Integer, 
        'jmlpi(115) As Double, statuspi(116) As Integer, jmlsi(117) As Double, statussi(118) As Integer, jmlrnr(119) As Double, 
        'statusrnr(120) As Integer, jmlsr(121) As Double, statussr(122) As Integer, isclose(123) As Integer, customtext1(124) As String, 
        'customtext2(125) As String, customtext3(126) As String, customdbl1(127) As Double, customdbl2(128) As Double, customdbl3(129) As Double, 
        'customdate1(130) As Date, customdate2(131) As Date, customdate3(132) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, 
        'sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, 
        'somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, 
        'socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, 
        'socustomdate1, socustomdate2, socustomdate3,
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, 
        'jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, 
        'customdbl3, customdate1, customdate2, customdate3

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = paramSplit(5).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "soid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "solokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sogudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sojenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sojenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sosumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sonotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sokodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "so2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sobagianpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "souraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sonoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sotglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "somatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sokurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sohargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sototal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sodiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sojmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sototalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sototalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sobiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sobiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sototaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sojmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sorekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sorekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sorekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sorekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "sorekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soidsq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sostatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "sojmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "soinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "soinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "somodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "somodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "soisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "socustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "socustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idsodetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "idbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "namabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "tipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jml", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "nilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "jmlbarang", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtdetail, "satuanbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "matauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kurs", AsEnumTypeData.AsString)
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
        AsDataTableTambahField(dtdetail, "idsqdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmldr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlpi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "jmlrnr", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "statusrnr", AsEnumTypeData.AsInt64)
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

        'Variabel ValidasiSimpan
        Dim ftExistOutstanding As String = "", ftOutstanding As String = "", gudang As String = ""
        Dim updNilai As String = "", updFilter As String = "", updStokBooking As String = ""
        Dim idbarang As String = "", idsqdetail As Integer = 0, jmlbarang As Double = 0

        'Validasi Harga dibawah harga jual
        Dim ftLowerPrice As String = "", kurs As Double = 0, harga As Double = 0

        'FILTER SQ, UNTUK CEK HARGA TERMASUK PAJAK ATAU TIDAK
        'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
        Dim ftSQ As String = ""

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 133) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------


            'VALIDASI TIPE DATA UTAMA ==========================================================
            'soid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - soid required numeric." : GoTo selesai
            End If
            'soasalbarangkategori(5) As Integer
            If (IsNumeric(dataRowDetail(5)) = False) Then
                result(2) = "Row : " & i & " - soasalbarangkategori required numeric." : GoTo selesai
            End If
            'sojenispenjualankategori(7) As Integer
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - sojenispenjualankategori required numeric." : GoTo selesai
            End If
            'socarabayar(8) As Integer
            If (IsNumeric(dataRowDetail(8)) = False) Then
                result(2) = "Row : " & i & " - socarabayar required numeric." : GoTo selesai
            End If
            'soautonotransaksi(10) As Integer
            If (IsNumeric(dataRowDetail(10)) = False) Then
                result(2) = "Row : " & i & " - soautonotransaksi required numeric." : GoTo selesai
            End If
            'sotgl(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - sotgl required date." : GoTo selesai
            End If
            'sokodepa(13) As Integer
            If (IsNumeric(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - sokodepa required numeric." : GoTo selesai
            End If
            'socustomer(14) As Integer
            'If (IsNumeric(dataRowDetail(14)) = False) Then
            '    result(2) = "Row : " & i & " - socustomer required numeric." : GoTo selesai
            'End If
            If (Len(dataRowDetail(14)) < 1) Then
                result(2) = "Row : " & i & " - socustomer can't be empty." : GoTo selesai
            End If
            'sobagianpenjualan(22) As Integer
            'If (IsNumeric(dataRowDetail(22)) = False) Then
            '    result(2) = "Row : " & i & " - sobagianpenjualan required numeric." : GoTo selesai
            'End If
            If (Len(dataRowDetail(22)) < 1) Then
                result(2) = "Row : " & i & " - sobagianpenjualan can't be empty." : GoTo selesai
            End If
            'sotglkirim(24) As Date
            If (IsDate(dataRowDetail(24)) = False) Then
                result(2) = "Row : " & i & " - sotglkirim required date." : GoTo selesai
            End If
            'sotgljatuhtempo(26) As Date
            If (IsDate(dataRowDetail(26)) = False) Then
                result(2) = "Row : " & i & " - sotgljatuhtempo required date." : GoTo selesai
            End If
            'sotglnoref(30) As Date
            If (IsDate(dataRowDetail(30)) = False) Then
                result(2) = "Row : " & i & " - sotglnoref required date." : GoTo selesai
            End If
            'sotglpenutupan(31) As Date
            If (IsDate(dataRowDetail(31)) = False) Then
                result(2) = "Row : " & i & " - sotglpenutupan required date." : GoTo selesai
            End If
            'sokurs(33) As Double
            If (IsNumeric(dataRowDetail(33)) = False) Then
                result(2) = "Row : " & i & " - sokurs required numeric." : GoTo selesai
            End If
            'sohargatermasukpajak(34) As Integer
            If (IsNumeric(dataRowDetail(34)) = False) Then
                result(2) = "Row : " & i & " - sohargatermasukpajak required numeric." : GoTo selesai
            End If
            'sototal(35) As Double
            If (IsNumeric(dataRowDetail(35)) = False) Then
                result(2) = "Row : " & i & " - sototal required numeric." : GoTo selesai
            End If
            'sojmldiskon(37) As Double
            If (IsNumeric(dataRowDetail(37)) = False) Then
                result(2) = "Row : " & i & " - sojmldiskon required numeric." : GoTo selesai
            End If
            'sototalpajak1detail(38) As Double
            If (IsNumeric(dataRowDetail(38)) = False) Then
                result(2) = "Row : " & i & " - sototalpajak1detail required numeric." : GoTo selesai
            End If
            'sototalpajak2detail(39) As Double
            If (IsNumeric(dataRowDetail(39)) = False) Then
                result(2) = "Row : " & i & " - sototalpajak2detail required numeric." : GoTo selesai
            End If
            ''sobiayalainpersen(40) As Double
            'If (IsNumeric(dataRowDetail(40)) = False) Then
            '    result(2) = "Row : " & i & " - sobiayalainpersen required numeric." : GoTo selesai
            'End If
            'sobiayalain(41) As Double
            If (IsNumeric(dataRowDetail(41)) = False) Then
                result(2) = "Row : " & i & " - sobiayalain required numeric." : GoTo selesai
            End If
            'sototaltransaksi(42) As Double
            If (IsNumeric(dataRowDetail(42)) = False) Then
                result(2) = "Row : " & i & " - sototaltransaksi required numeric." : GoTo selesai
            End If
            'sojmlbayar(43) As Double
            If (IsNumeric(dataRowDetail(43)) = False) Then
                result(2) = "Row : " & i & " - sojmlbayar required numeric." : GoTo selesai
            End If
            'soidsq(49) As Integer
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - soidsq required numeric." : GoTo selesai
            End If
            'sostatuspl(50) As Integer
            If (IsNumeric(dataRowDetail(50)) = False) Then
                result(2) = "Row : " & i & " - sostatuspl required numeric." : GoTo selesai
            End If
            'sostatusdo(51) As Integer
            If (IsNumeric(dataRowDetail(51)) = False) Then
                result(2) = "Row : " & i & " - sostatusdo required numeric." : GoTo selesai
            End If
            'sostatusdr(52) As Integer
            If (IsNumeric(dataRowDetail(52)) = False) Then
                result(2) = "Row : " & i & " - sostatusdr required numeric." : GoTo selesai
            End If
            'sostatuspi(53) As Integer
            If (IsNumeric(dataRowDetail(53)) = False) Then
                result(2) = "Row : " & i & " - sostatuspi required numeric." : GoTo selesai
            End If
            'sostatussi(54) As Integer
            If (IsNumeric(dataRowDetail(54)) = False) Then
                result(2) = "Row : " & i & " - sostatussi required numeric." : GoTo selesai
            End If
            'sostatusrnr(55) As Integer
            If (IsNumeric(dataRowDetail(55)) = False) Then
                result(2) = "Row : " & i & " - sostatusrnr required numeric." : GoTo selesai
            End If
            'sostatussr(56) As Integer
            If (IsNumeric(dataRowDetail(56)) = False) Then
                result(2) = "Row : " & i & " - sostatussr required numeric." : GoTo selesai
            End If
            'sostatus(57) As Integer
            If (IsNumeric(dataRowDetail(57)) = False) Then
                result(2) = "Row : " & i & " - sostatus required numeric." : GoTo selesai
            End If
            'sostatussebelumnya(58) As Integer
            If (IsNumeric(dataRowDetail(58)) = False) Then
                result(2) = "Row : " & i & " - sostatussebelumnya required numeric." : GoTo selesai
            End If
            'sojmlrevisi(59) As Integer
            If (IsNumeric(dataRowDetail(59)) = False) Then
                result(2) = "Row : " & i & " - sojmlrevisi required numeric." : GoTo selesai
            End If
            'socetakanke(60) As Integer
            If (IsNumeric(dataRowDetail(60)) = False) Then
                result(2) = "Row : " & i & " - socetakanke required numeric." : GoTo selesai
            End If
            'soinputuser(61) As Integer
            If (IsNumeric(dataRowDetail(61)) = False) Then
                result(2) = "Row : " & i & " - soinputuser required numeric." : GoTo selesai
            End If
            'soinputtgl(62) As DateTime
            If (IsDate(dataRowDetail(62)) = False) Then
                result(2) = "Row : " & i & " - soinputtgl required date." : GoTo selesai
            End If
            'somodifikasiuser(63) As Integer
            If (IsNumeric(dataRowDetail(63)) = False) Then
                result(2) = "Row : " & i & " - somodifikasiuser required numeric." : GoTo selesai
            End If
            'somodifikasitgl(64) As DateTime
            If (IsDate(dataRowDetail(64)) = False) Then
                result(2) = "Row : " & i & " - somodifikasitgl required date." : GoTo selesai
            End If
            'soisclose(65) As Integer
            If (IsNumeric(dataRowDetail(65)) = False) Then
                result(2) = "Row : " & i & " - soisclose required numeric." : GoTo selesai
            End If
            'socustomint1(71) As Integer
            If (IsNumeric(dataRowDetail(71)) = False) Then
                result(2) = "Row : " & i & " - socustomint1 required numeric." : GoTo selesai
            End If
            'socustomint2(72) As Integer
            If (IsNumeric(dataRowDetail(72)) = False) Then
                result(2) = "Row : " & i & " - socustomint2 required numeric." : GoTo selesai
            End If
            'socustomint3(73) As Integer
            If (IsNumeric(dataRowDetail(73)) = False) Then
                result(2) = "Row : " & i & " - socustomint3 required numeric." : GoTo selesai
            End If
            'socustomdbl1(74) As Double
            If (IsNumeric(dataRowDetail(74)) = False) Then
                result(2) = "Row : " & i & " - socustomdbl1 required numeric." : GoTo selesai
            End If
            'socustomdbl2(75) As Double
            If (IsNumeric(dataRowDetail(75)) = False) Then
                result(2) = "Row : " & i & " - socustomdbl2 required numeric." : GoTo selesai
            End If
            'socustomdbl3(76) As Double
            If (IsNumeric(dataRowDetail(76)) = False) Then
                result(2) = "Row : " & i & " - socustomdbl3 required numeric." : GoTo selesai
            End If
            'socustomdate1(77) As Date
            If (IsDate(dataRowDetail(77)) = False) Then
                result(2) = "Row : " & i & " - socustomdate1 required date." : GoTo selesai
            End If
            'socustomdate2(78) As Date
            If (IsDate(dataRowDetail(78)) = False) Then
                result(2) = "Row : " & i & " - socustomdate2 required date." : GoTo selesai
            End If
            'socustomdate3(79) As Date
            If (IsDate(dataRowDetail(79)) = False) Then
                result(2) = "Row : " & i & " - socustomdate3 required date." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'socabang(1) As String
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - socabang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(1)) > 25 Then
                result(2) = "Row : " & i & " - socabang should not be more than 25 character." : GoTo selesai
            End If

            'solokasi(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - solokasi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - solokasi should not be more than 25 character." : GoTo selesai
            End If

            'sogudang(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - sogudang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - sogudang should not be more than 25 character." : GoTo selesai
            End If

            'sosumber(9) As String
            If Len(dataRowDetail(9)) = 0 Then
                result(2) = "Row : " & i & " - sosumber can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(9)) > 10 Then
                result(2) = "Row : " & i & " - sosumber should not be more than 10 character." : GoTo selesai
            End If

            'sonotransaksi(11) As String
            If Len(dataRowDetail(11)) = 0 Then
                result(2) = "Row : " & i & " - sonotransaksi can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(11)) > 50 Then
                result(2) = "Row : " & i & " - sonotransaksi should not be more than 50 character." : GoTo selesai
            End If

            'sotgl(12) As Date
            If Len(dataRowDetail(12)) = 0 Then
                result(2) = "Row : " & i & " - sotgl can't be empty" : GoTo selesai
            End If

            'sotglkirim(24) As Date
            If Len(dataRowDetail(24)) = 0 Then
                result(2) = "Row : " & i & " - sotglkirim can't be empty" : GoTo selesai
            End If

            'sotgljatuhtempo(26) As Date
            If Len(dataRowDetail(26)) = 0 Then
                result(2) = "Row : " & i & " - sotgljatuhtempo can't be empty" : GoTo selesai
            End If

            'sonoref(29) As String
            If Len(dataRowDetail(29)) = 0 Then
                result(2) = "Row : " & i & " - sonoref can't be empty" : GoTo selesai
            End If

            'sotglnoref(30) As Date
            If Len(dataRowDetail(30)) = 0 Then
                result(2) = "Row : " & i & " - sotglnoref can't be empty" : GoTo selesai
            End If

            'sotglpenutupan(31) As Date
            If Len(dataRowDetail(31)) = 0 Then
                result(2) = "Row : " & i & " - sotglpenutupan can't be empty" : GoTo selesai
            End If

            'somatauang(32) As String
            If Len(dataRowDetail(32)) = 0 Then
                result(2) = "Row : " & i & " - somatauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(32)) > 25 Then
                result(2) = "Row : " & i & " - somatauang should not be more than 25 character." : GoTo selesai
            End If

            'sokurs(33) As Double
            If Len(dataRowDetail(33)) = 0 Then
                result(2) = "Row : " & i & " - sokurs can't be empty" : GoTo selesai
            End If

            'sototal(35) As Double
            If Len(dataRowDetail(35)) = 0 Then
                result(2) = "Row : " & i & " - sototal can't be empty" : GoTo selesai
            End If

            'sodiskonpersen(36) As String
            If Len(dataRowDetail(36)) = 0 Then
                result(2) = "Row : " & i & " - sodiskonpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(36)) > 25 Then
                result(2) = "Row : " & i & " - sodiskonpersen should not be more than 25 character." : GoTo selesai
            End If

            'sojmldiskon(37) As Double
            If Len(dataRowDetail(37)) = 0 Then
                result(2) = "Row : " & i & " - sojmldiskon can't be empty" : GoTo selesai
            End If

            'sototalpajak1detail(38) As Double
            If Len(dataRowDetail(38)) = 0 Then
                result(2) = "Row : " & i & " - sototalpajak1detail can't be empty" : GoTo selesai
            End If

            'sototalpajak2detail(39) As Double
            If Len(dataRowDetail(39)) = 0 Then
                result(2) = "Row : " & i & " - sototalpajak2detail can't be empty" : GoTo selesai
            End If

            'sobiayalainpersen(40) As Double
            If Len(dataRowDetail(40)) = 0 Then
                result(2) = "Row : " & i & " - sobiayalainpersen can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(40)) > 25 Then
                result(2) = "Row : " & i & " - sobiayalainpersen should not be more than 25 character." : GoTo selesai
            End If

            'sobiayalain(41) As Double
            If Len(dataRowDetail(41)) = 0 Then
                result(2) = "Row : " & i & " - sobiayalain can't be empty" : GoTo selesai
            End If

            'sototaltransaksi(42) As Double
            If Len(dataRowDetail(42)) = 0 Then
                result(2) = "Row : " & i & " - sototaltransaksi can't be empty" : GoTo selesai
            End If

            'sojmlbayar(43) As Double
            If Len(dataRowDetail(43)) = 0 Then
                result(2) = "Row : " & i & " - sojmlbayar can't be empty" : GoTo selesai
            End If

            'soinputtgl(62) As DateTime
            If Len(dataRowDetail(62)) = 0 Then
                result(2) = "Row : " & i & " - soinputtgl can't be empty" : GoTo selesai
            End If

            'somodifikasitgl(64) As DateTime
            If Len(dataRowDetail(64)) = 0 Then
                result(2) = "Row : " & i & " - somodifikasitgl can't be empty" : GoTo selesai
            End If

            'socustomtext1(66) As String
            'CUSTOM TEXT DIISI DARI SOCATATAN(28) + SOTGLKIRIM(24)
            'dataRowDetail(66) = String.Concat(dataRowDetail(28), Replace(dataRowDetail(24), "-", ""))

            'socustomdbl1(74) As Double
            If Len(dataRowDetail(74)) = 0 Then
                result(2) = "Row : " & i & " - socustomdbl1 can't be empty" : GoTo selesai
            End If

            'socustomdbl2(75) As Double
            If Len(dataRowDetail(75)) = 0 Then
                result(2) = "Row : " & i & " - socustomdbl2 can't be empty" : GoTo selesai
            End If

            'socustomdbl3(76) As Double
            If Len(dataRowDetail(76)) = 0 Then
                result(2) = "Row : " & i & " - socustomdbl3 can't be empty" : GoTo selesai
            End If

            'socustomdate1(77) As Date
            If Len(dataRowDetail(77)) = 0 Then
                result(2) = "Row : " & i & " - socustomdate1 can't be empty" : GoTo selesai
            End If

            'socustomdate2(78) As Date
            If Len(dataRowDetail(78)) = 0 Then
                result(2) = "Row : " & i & " - socustomdate2 can't be empty" : GoTo selesai
            End If

            'socustomdate3(79) As Date
            If Len(dataRowDetail(79)) = 0 Then
                result(2) = "Row : " & i & " - socustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idsodetail(80) As Integer
            If (IsNumeric(dataRowDetail(80)) = False) Then
                result(2) = "Row : " & i & " - idsodetail required numeric." : GoTo selesai
            End If
            'idso(81) As Integer
            If (IsNumeric(dataRowDetail(81)) = False) Then
                result(2) = "Row : " & i & " - idso required numeric." : GoTo selesai
            End If
            ''idbarang(82) As Integer
            'If (IsNumeric(dataRowDetail(82)) = False) Then
            '    result(2) = "Row : " & i & " - idbarang required numeric." : GoTo selesai
            'End If
            If Len(dataRowDetail(82)) = 0 Then
                result(2) = "Row : " & i & " - idbarang can't be empty" : GoTo selesai
            End If
            'jml(85) As Double
            If (IsNumeric(dataRowDetail(85)) = False) Then
                result(2) = "Row : " & i & " - jml required numeric." : GoTo selesai
            End If
            'nilaisatuan(87) As Double
            If (IsNumeric(dataRowDetail(7)) = False) Then
                result(2) = "Row : " & i & " - nilaisatuan required numeric." : GoTo selesai
            End If
            'jmlbarang(88) As Double
            'jmlbarang = jml * nilaisatuan
            dataRowDetail(88) = Double.Parse(dataRowDetail(85)) * Double.Parse(dataRowDetail(87))
            If (IsNumeric(dataRowDetail(88)) = False) Then
                result(2) = "Row : " & i & " - jmlbarang required numeric." : GoTo selesai
            End If
            'kurs(91) As Double
            If (IsNumeric(dataRowDetail(91)) = False) Then
                result(2) = "Row : " & i & " - kurs required numeric." : GoTo selesai
            End If
            'harga(92) As Double
            If (IsNumeric(dataRowDetail(92)) = False) Then
                result(2) = "Row : " & i & " - harga required numeric." : GoTo selesai
            End If
            'jmldiskon(94) As Double
            If (IsNumeric(dataRowDetail(94)) = False) Then
                result(2) = "Row : " & i & " - jmldiskon required numeric." : GoTo selesai
            End If
            'jmlpajak1(96) As Double
            If (IsNumeric(dataRowDetail(96)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak1 required numeric." : GoTo selesai
            End If
            'jmlpajak2(98) As Double
            If (IsNumeric(dataRowDetail(98)) = False) Then
                result(2) = "Row : " & i & " - jmlpajak2 required numeric." : GoTo selesai
            End If
            'urutan(107) As Integer
            If (IsNumeric(dataRowDetail(107)) = False) Then
                result(2) = "Row : " & i & " - urutan required numeric." : GoTo selesai
            End If
            'idsqdetail(108) As Integer
            If (IsNumeric(dataRowDetail(108)) = False) Then
                result(2) = "Row : " & i & " - idsqdetail required numeric." : GoTo selesai
            End If
            'jmlpl(109) As Double
            If (IsNumeric(dataRowDetail(109)) = False) Then
                result(2) = "Row : " & i & " - jmlpl required numeric." : GoTo selesai
            End If
            'statuspl(110) As Integer
            If (IsNumeric(dataRowDetail(110)) = False) Then
                result(2) = "Row : " & i & " - statuspl required numeric." : GoTo selesai
            End If
            'jmldo(111) As Double
            If (IsNumeric(dataRowDetail(111)) = False) Then
                result(2) = "Row : " & i & " - jmldo required numeric." : GoTo selesai
            End If
            'statusdo(112) As Integer
            If (IsNumeric(dataRowDetail(112)) = False) Then
                result(2) = "Row : " & i & " - statusdo required numeric." : GoTo selesai
            End If
            'jmldr(113) As Double
            If (IsNumeric(dataRowDetail(113)) = False) Then
                result(2) = "Row : " & i & " - jmldr required numeric." : GoTo selesai
            End If
            'statusdr(114) As Integer
            If (IsNumeric(dataRowDetail(114)) = False) Then
                result(2) = "Row : " & i & " - statusdr required numeric." : GoTo selesai
            End If
            'jmlpi(115) As Double
            If (IsNumeric(dataRowDetail(115)) = False) Then
                result(2) = "Row : " & i & " - jmlpi required numeric." : GoTo selesai
            End If
            'statuspi(116) As Integer
            If (IsNumeric(dataRowDetail(116)) = False) Then
                result(2) = "Row : " & i & " - statuspi required numeric." : GoTo selesai
            End If
            'jmlsi(117) As Double
            If (IsNumeric(dataRowDetail(117)) = False) Then
                result(2) = "Row : " & i & " - jmlsi required numeric." : GoTo selesai
            End If
            'statussi(118) As Integer
            If (IsNumeric(dataRowDetail(118)) = False) Then
                result(2) = "Row : " & i & " - statussi required numeric." : GoTo selesai
            End If
            'jmlrnr(119) As Double
            If (IsNumeric(dataRowDetail(119)) = False) Then
                result(2) = "Row : " & i & " - jmlrnr required numeric." : GoTo selesai
            End If
            'statusrnr(120) As Integer
            If (IsNumeric(dataRowDetail(120)) = False) Then
                result(2) = "Row : " & i & " - statusrnr required numeric." : GoTo selesai
            End If
            'jmlsr(121) As Double
            If (IsNumeric(dataRowDetail(121)) = False) Then
                result(2) = "Row : " & i & " - jmlsr required numeric." : GoTo selesai
            End If
            'statussr(122) As Integer
            If (IsNumeric(dataRowDetail(122)) = False) Then
                result(2) = "Row : " & i & " - statussr required numeric." : GoTo selesai
            End If
            'isclose(123) As Integer
            If (IsNumeric(dataRowDetail(123)) = False) Then
                result(2) = "Row : " & i & " - isclose required numeric." : GoTo selesai
            End If
            'customdbl1(127) As Double
            If (IsNumeric(dataRowDetail(127)) = False) Then
                result(2) = "Row : " & i & " - customdbl1 required numeric." : GoTo selesai
            End If
            'customdbl2(128) As Double
            If (IsNumeric(dataRowDetail(128)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            'customdbl3(129) As Double
            If (IsNumeric(dataRowDetail(49)) = False) Then
                result(2) = "Row : " & i & " - customdbl3 required numeric." : GoTo selesai
            End If
            'customdate1(130) As Date
            If (IsDate(dataRowDetail(130)) = False) Then
                result(2) = "Row : " & i & " - customdate1 required date." : GoTo selesai
            End If
            'customdate2(131) As Date
            If (IsDate(dataRowDetail(131)) = False) Then
                result(2) = "Row : " & i & " - customdate2 required date." : GoTo selesai
            End If
            'customdate3(132) As Date
            If (IsDate(dataRowDetail(132)) = False) Then
                result(2) = "Row : " & i & " - customdate3 required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'namabarang(83) As String
            'If Len(dataRowDetail(83)) = 0 Then
            '    result(2) = "Row : " & i & " - namabarang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(83)) > 100 Then
                result(2) = "Row : " & i & " - namabarang should not be more than 100 character." : GoTo selesai
            End If

            'jml(85) As Double
            If Len(dataRowDetail(85)) = 0 Then
                result(2) = "Row : " & i & " - jml can't be empty" : GoTo selesai
            End If
            If dataRowDetail(85) <= 0 Then
                result(2) = "Row : " & i & " - jml can't be less than or equal to zero" : GoTo selesai
            End If

            'satuan(86) As String
            'If Len(dataRowDetail(86)) = 0 Then
            '    result(2) = "Row : " & i & " - satuan can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(86)) > 25 Then
                result(2) = "Row : " & i & " - satuan should not be more than 25 character." : GoTo selesai
            End If

            'nilaisatuan(87) As Double
            If Len(dataRowDetail(87)) = 0 Then
                result(2) = "Row : " & i & " - nilaisatuan can't be empty" : GoTo selesai
            End If

            'jmlbarang(88) As Double
            If Len(dataRowDetail(88)) = 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be empty" : GoTo selesai
            End If
            If dataRowDetail(88) <= 0 Then
                result(2) = "Row : " & i & " - jmlbarang can't be less than or equal to zero" : GoTo selesai
            End If

            'satuanbarang(89) As String
            'If Len(dataRowDetail(89)) = 0 Then
            '    result(2) = "Row : " & i & " - satuanbarang can't be empty" : GoTo selesai
            'End If
            If Len(dataRowDetail(89)) > 25 Then
                result(2) = "Row : " & i & " - satuanbarang should not be more than 25 character." : GoTo selesai
            End If

            'matauang(90) As String
            If Len(dataRowDetail(90)) = 0 Then
                result(2) = "Row : " & i & " - matauang can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(90)) > 25 Then
                result(2) = "Row : " & i & " - matauang should not be more than 25 character." : GoTo selesai
            End If

            'kurs(91) As Double
            If Len(dataRowDetail(91)) = 0 Then
                result(2) = "Row : " & i & " - kurs can't be empty" : GoTo selesai
            End If

            'harga(92) As Double
            If Len(dataRowDetail(92)) = 0 Then
                result(2) = "Row : " & i & " - harga can't be empty" : GoTo selesai
            End If

            'diskon(93) As Double
            If Len(dataRowDetail(93)) = 0 Then
                result(2) = "Row : " & i & " - diskon can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(93)) > 25 Then
                result(2) = "Row : " & i & " - diskon should not be more than 25 character." : GoTo selesai
            End If

            'jmldiskon(94) As Double
            If Len(dataRowDetail(94)) = 0 Then
                result(2) = "Row : " & i & " - jmldiskon can't be empty" : GoTo selesai
            Else
                'HITUNG JMLDISKON : jml(85) As Double, harga(92) As Double, diskon(93) As String
                dataRowDetail(94) = F_Diskon(Double.Parse(dataRowDetail(85)), Double.Parse(dataRowDetail(92)), FixQuotes(dataRowDetail(93).ToString))
            End If

            'jmlpajak1(96) As Double
            If Len(dataRowDetail(96)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak1 can't be empty" : GoTo selesai
            End If

            'jmlpajak2(98) As Double
            If Len(dataRowDetail(98)) = 0 Then
                result(2) = "Row : " & i & " - jmlpajak2 can't be empty" : GoTo selesai
            End If

            'jmlpl(109) As Double
            If Len(dataRowDetail(109)) = 0 Then
                result(2) = "Row : " & i & " - jmlpl can't be empty" : GoTo selesai
            End If

            'jmldo(111) As Double
            If Len(dataRowDetail(111)) = 0 Then
                result(2) = "Row : " & i & " - jmldo can't be empty" : GoTo selesai
            End If

            'jmldr(113) As Double
            If Len(dataRowDetail(113)) = 0 Then
                result(2) = "Row : " & i & " - jmldr can't be empty" : GoTo selesai
            End If

            'jmlpi(115) As Double
            If Len(dataRowDetail(115)) = 0 Then
                result(2) = "Row : " & i & " - jmlpi can't be empty" : GoTo selesai
            End If

            'jmlsi(117) As Double
            If Len(dataRowDetail(117)) = 0 Then
                result(2) = "Row : " & i & " - jmlsi can't be empty" : GoTo selesai
            End If

            'jmlrnr(119) As Double
            If Len(dataRowDetail(119)) = 0 Then
                result(2) = "Row : " & i & " - jmlrnr can't be empty" : GoTo selesai
            End If

            'jmlsr(121) As Double
            If Len(dataRowDetail(121)) = 0 Then
                result(2) = "Row : " & i & " - jmlsr can't be empty" : GoTo selesai
            End If

            'customdbl1(127) As Double
            If Len(dataRowDetail(127)) = 0 Then
                result(2) = "Row : " & i & " - customdbl1 can't be empty" : GoTo selesai
            End If

            'customdbl2(128) As Double
            If Len(dataRowDetail(128)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            'customdbl3(129) As Double
            If Len(dataRowDetail(129)) = 0 Then
                result(2) = "Row : " & i & " - customdbl3 can't be empty" : GoTo selesai
            End If

            'customdate1(130) As Date
            If Len(dataRowDetail(130)) = 0 Then
                result(2) = "Row : " & i & " - customdate1 can't be empty" : GoTo selesai
            End If

            'customdate2(131) As Date
            If Len(dataRowDetail(131)) = 0 Then
                result(2) = "Row : " & i & " - customdate2 can't be empty" : GoTo selesai
            End If

            'customdate3(132) As Date
            If Len(dataRowDetail(132)) = 0 Then
                result(2) = "Row : " & i & " - customdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------


            If AsDataTableTambahData(dtdetail, "soid~socabang~solokasi~sogudang~soasalbarang~soasalbarangkategori~sojenispenjualan~sojenispenjualankategori~socarabayar~sosumber~soautonotransaksi~sonotransaksi~sotgl~sokodepa~socustomer~socustomerkontak~so1alamat1~so1alamat2~so1alamat3~so2alamat1~so2alamat2~so2alamat3~sobagianpenjualan~soekspedisi~sotglkirim~sotermin~sotgljatuhtempo~souraian~socatatan~sonoref~sotglnoref~sotglpenutupan~somatauang~sokurs~sohargatermasukpajak~sototal~sodiskonpersen~sojmldiskon~sototalpajak1detail~sototalpajak2detail~sobiayalainpersen~sobiayalain~sototaltransaksi~sojmlbayar~sorekdiskon~sorekpajak1~sorekpajak2~sorekbiayalain~sorekbayar~soidsq~sostatuspl~sostatusdo~sostatusdr~sostatuspi~sostatussi~sostatusrnr~sostatussr~sostatus~sostatussebelumnya~sojmlrevisi~socetakanke~soinputuser~soinputtgl~somodifikasiuser~somodifikasitgl~soisclose~socustomtext1~socustomtext2~socustomtext3~socustomtext4~socustomtext5~socustomint1~socustomint2~socustomint3~socustomdbl1~socustomdbl2~socustomdbl3~socustomdate1~socustomdate2~socustomdate3~idsodetail~idso~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idsqdetail~jmlpl~statuspl~jmldo~statusdo~jmldr~statusdr~jmlpi~statuspi~jmlsi~statussi~jmlrnr~statusrnr~jmlsr~statussr~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52) & "~" & dataRowDetail(53) & "~" & dataRowDetail(54) & "~" & dataRowDetail(55) & "~" & dataRowDetail(56) & "~" & dataRowDetail(57) & "~" & dataRowDetail(58) & "~" & dataRowDetail(59) & "~" & dataRowDetail(60) & "~" & dataRowDetail(61) & "~" & dataRowDetail(62) & "~" & dataRowDetail(63) & "~" & dataRowDetail(64) & "~" & dataRowDetail(65) & "~" & dataRowDetail(66) & "~" & dataRowDetail(67) & "~" & dataRowDetail(68) & "~" & dataRowDetail(69) & "~" & dataRowDetail(70) & "~" & dataRowDetail(71) & "~" & dataRowDetail(72) & "~" & dataRowDetail(73) & "~" & dataRowDetail(74) & "~" & dataRowDetail(75) & "~" & dataRowDetail(76) & "~" & dataRowDetail(77) & "~" & dataRowDetail(78) & "~" & dataRowDetail(79) & "~" & dataRowDetail(80) & "~" & dataRowDetail(81) & "~" & dataRowDetail(82) & "~" & dataRowDetail(83) & "~" & dataRowDetail(84) & "~" & dataRowDetail(85) & "~" & dataRowDetail(86) & "~" & dataRowDetail(87) & "~" & dataRowDetail(88) & "~" & dataRowDetail(89) & "~" & dataRowDetail(90) & "~" & dataRowDetail(91) & "~" & dataRowDetail(92) & "~" & dataRowDetail(93) & "~" & dataRowDetail(94) & "~" & dataRowDetail(95) & "~" & dataRowDetail(96) & "~" & dataRowDetail(97) & "~" & dataRowDetail(98) & "~" & dataRowDetail(99) & "~" & dataRowDetail(100) & "~" & dataRowDetail(101) & "~" & dataRowDetail(102) & "~" & dataRowDetail(103) & "~" & dataRowDetail(104) & "~" & dataRowDetail(105) & "~" & dataRowDetail(106) & "~" & dataRowDetail(107) & "~" & dataRowDetail(108) & "~" & dataRowDetail(109) & "~" & dataRowDetail(110) & "~" & dataRowDetail(111) & "~" & dataRowDetail(112) & "~" & dataRowDetail(113) & "~" & dataRowDetail(114) & "~" & dataRowDetail(115) & "~" & dataRowDetail(116) & "~" & dataRowDetail(117) & "~" & dataRowDetail(118) & "~" & dataRowDetail(119) & "~" & dataRowDetail(120) & "~" & dataRowDetail(121) & "~" & dataRowDetail(122) & "~" & dataRowDetail(123) & "~" & dataRowDetail(124) & "~" & dataRowDetail(125) & "~" & dataRowDetail(126) & "~" & dataRowDetail(127) & "~" & dataRowDetail(128) & "~" & dataRowDetail(129) & "~" & dataRowDetail(130) & "~" & dataRowDetail(131) & "~" & dataRowDetail(132)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

            'FILTER NO PO CUSTOMER
            If Len(dataRowDetail(126)) > 0 Then
                Filter &= IIf(Len(Filter) > 0, ", ", "")
                Filter &= "'" & FixQuotes(dataRowDetail(126)) & "'"
            End If

            'BUAT FILTER UNTUK VALIDASI ---------------------------------
            'ValidasiSimpan
            'idbarang(82) As Integer     , jmlbarang(88) As Double       , gudang(101) As String       , idsqdetail(108) As Integer
            'idbarang = dataRowDetail(82) : jmlbarang = dataRowDetail(88) : gudang = dataRowDetail(101) : idsqdetail = dataRowDetail(108)
            'kurs(91) As Double                    , harga(92) As Double
            'kurs = Double.Parse(dataRowDetail(91)) : harga = Double.Parse(dataRowDetail(92))

            'VALIDASI OUTSTANDING -------------------------
            'If idsqdetail <> 0 Then 'SQ
            '    'CEK SQ YANG DIAMBIL
            '    'DALAM 1 TRANSAKSI TIDAK BOLEH AMBIL DARI TRANSAKSI HARGA TERMASUK PAJAK BERCAMPUR DENGAN HARGA TIDAK TERMASUK PAJAK
            '    ftSQ = IIf(Len(ftSQ.ToString) = 0, "", ftSQ & " OR ")
            '    ftSQ = String.Concat(ftSQ, " (sqd.idsqdetail = " & idsqdetail & ") ")

            '    '1. CEK DATA EXIST
            '    ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
            '    ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

            '    '2. CEK JML OUTSTANDING
            '    Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
            '    ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
            '    ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")

            '    '3. SET NILAI UPDATE OUTSTANDING
            '    updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN jmlrealisasi + '" & Outstanding & "' ", updNilai)

            '    '4. SET FILTER UPDATE OUTSTANDING
            '    updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
            '    updFilter = String.Concat(updFilter, "(idsqdetail = '" & idsqdetail & "')")
            'End If

            ''5. SET NILAI UPDATE STOK BOOKING
            'updStokBooking = IIf(Len(updStokBooking.ToString) = 0, "", updStokBooking & ", ")
            'updStokBooking = String.Concat(updStokBooking, "('" & idbarang & "', '" & gudang & "', ('" & jmlbarang & "'))") ' idbarang, gudang, jmlbooking

            ''Validasi harga dibawah harga jual
            'ftLowerPrice = IIf(Len(ftLowerPrice.ToString) = 0, "", ftLowerPrice & " OR ")
            'ftLowerPrice = String.Concat(ftLowerPrice, "(bid = '" & idbarang & "' AND bhargajual1 > " & FixDouble(harga * kurs) & ")")
            'END OF BUAT FILTER UNTUK VALIDASI --------------------------

        Next


        If Len(Filter) > 0 Then
            sql = "SELECT soid, sonotransaksi, sod.customtext3 FROM m5_so so JOIN m5_so_detail sod ON so.soid = sod.idso AND sod.customtext3 IN(" & Filter & ") GROUP BY so.soid"
            Dim dtNoreff As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNoreff.Rows.Count > 0 Then
                If Len(dtNoreff.Rows(0)("soid")) > 0 Then
                    result(2) = "PO Cust No. '" & dtNoreff.Rows(0)("customtext3") & "' has been imported on Transaction No. '" & dtNoreff.Rows(0)("sonotransaksi") & "'" : GoTo selesai
                End If
            End If

        End If
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try

            Dim view As DataView = New DataView(dtdetail)
            Dim dtutama As DataTable = view.ToTable(True, "sonoref")
            Dim drutama As DataRow, dtUtamaTemp As New DataTable, dtdetailNew As New DataTable

            'Proses utama
            If (dtutama.Rows.Count > 0) Then

                'VALIDASI NOREF
                Dim strValNoref As String = ""
                For Each drvalnoref As DataRow In dtutama.Rows
                    strValNoref = IIf(Len(strValNoref) > 0, strValNoref & ", ", "")
                    strValNoref &= "'" & FixQuotes(drvalnoref("sonoref")) & "'"
                Next
                If Len(strValNoref) > 0 Then
                    sql = "SELECT GROUP_CONCAT(DISTINCT CONCAT(sonoref, ' (' , sonotransaksi, ')') SEPARATOR ', ') as errmessage FROM m5_so WHERE sonoref IN (" & strValNoref & ")"
                    Dim dtValNoref As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                    If dtValNoref.Rows.Count > 0 Then
                        If Len(FxDB(dtValNoref.Rows(0)(0), "")) > 0 Then
                            result(2) = "The following No Reff transaction has been imported : " & dtValNoref.Rows(0)(0) : GoTo selesai
                        End If

                    End If
                End If

                'AMBIL MASTER
                Dim dtMasterTemp As New DataTable, vKpkp As Double = 0, vBkp As Double = 0, vPajak1 As String = ""
                Dim strValMaster As String = "", nilaipajak1 As Double = 0

                Dim dtKontak As DataTable = AsDataTableAmbilDariDBCon("SELECT kid, kkode, ksalesman, kkontakperson, k1alamat1, k2alamat1, kpkp, kterminjual, kmatauang FROM m1_contact WHERE kkategori = 'C'", myConn)
                Dim dtSalesman As DataTable = AsDataTableAmbilDariDBCon("SELECT kid, kkode FROM m1_contact WHERE kkategori = 'M'", myConn)
                Dim dtEkspedisi As DataTable = AsDataTableAmbilDariDBCon("SELECT ekode FROM m1_expedition", myConn)
                Dim dtTermin As DataTable = AsDataTableAmbilDariDBCon("SELECT trkode, trharijatuhtempo from m1_terms", myConn)
                Dim dtMatauang As DataTable = AsDataTableAmbilDariDBCon("SELECT ckode, ckurs from m1_currency", myConn)
                Dim dtBarang As DataTable = AsDataTableAmbilDariDBCon("SELECT bid, bkode, bnama, bnamaalias1, btipe, bkp, bsatuan, bsatuandefault from m1_item", myConn)
                Dim dtSatuan As DataTable = AsDataTableAmbilDariDBCon("SELECT ukode, unilai from m1_unit", myConn)
                Dim dtPajak As DataTable = AsDataTableAmbilDariDBCon("SELECT tkode, tnilai from m1_tax", myConn)
                Dim dtCostcenter As DataTable = AsDataTableAmbilDariDBCon("SELECT cckode from m1_cost_center", myConn)
                Dim dtDivisi As DataTable = AsDataTableAmbilDariDBCon("SELECT dkode from m1_division", myConn)
                Dim dtSubdivisi As DataTable = AsDataTableAmbilDariDBCon("SELECT sdkode from m1_subdivision", myConn)
                Dim dtProyek As DataTable = AsDataTableAmbilDariDBCon("SELECT pkode from m1_project", myConn)
                Dim dtSett As DataTable = AsDataTableAmbilDariDBCon("SELECT smodule, sgrup, skode, snilai FROM m0_setting WHERE (smodule = '0' AND sgrup = 'pajak' AND skode = 'PajakKode')", myConn)

                'AMBIL SETTING PAJAK
                strValMaster = AsDataTableDLookup(dtSett, "snilai", "skode = 'PajakKode'", ".xxx. data not found .xxx.")
                If strValMaster = ".xxx. data not found .xxx." Then
                    result(2) = "Tax doesn't exist in Setting Data" : GoTo selesai
                Else
                    vPajak1 = strValMaster
                End If

                For Each drut As DataRow In dtutama.Rows
                    dtUtamaTemp = AsDataTableFilterSortDt(dtdetail, "sonoref = '" & FixQuotes(drut("sonoref")) & "'")

                    If dtUtamaTemp.Rows.Count > 0 Then

                        '*** Start Transaction ***'  
                        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                        drutama = dtUtamaTemp.Rows(0)

                        dtdetailNew = AsDataTableFilterSortDt(dtdetail, "sonoref = '" & FixQuotes(drut("sonoref")) & "'")

                        ''CEK PERIODE AKUNTANSI ==================================
                        'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                        'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(AsFormatTanggal(drutama("sotgl")), AsFormatTanggal(drutama("sotgl")))
                        'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                        'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                        ''END OF CEK PERIODE AKUNTANSI ===========================

                        'VALIDASI MASTER DATA
                        'SOCUSTOMER (kid, kkode, ksalesman, kkontakperson, k1alamat1, k2alamat1, kpkp, kterminjual, kmatauang)
                        If Len(FixQuotes(drutama("socustomer"))) > 0 Then
                            dtMasterTemp = AsDataTableFilterSortDt(dtKontak, "kkode = '" & FixQuotes(drutama("socustomer")) & "'")
                            If dtMasterTemp.Rows.Count > 0 Then
                                drutama("socustomer") = dtMasterTemp.Rows(0)("kid")
                                drutama("socustomerkontak") = dtMasterTemp.Rows(0)("kkontakperson")
                                drutama("so1alamat1") = dtMasterTemp.Rows(0)("k1alamat1")
                                drutama("so2alamat1") = dtMasterTemp.Rows(0)("k2alamat1")
                                drutama("sobagianpenjualan") = dtMasterTemp.Rows(0)("ksalesman")
                                drutama("sotermin") = dtMasterTemp.Rows(0)("kterminjual")
                                drutama("somatauang") = dtMasterTemp.Rows(0)("kmatauang")
                                vKpkp = dtMasterTemp.Rows(0)("kpkp")
                            Else
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & drutama("socustomer") & "' doesn't exist in Customer Data" : Trans.Rollback() : GoTo selesai
                            End If
                        End If

                        'SOBAGIANPENJUALAN
                        If Len(FixQuotes(drutama("sobagianpenjualan"))) > 0 Then
                            strValMaster = AsDataTableDLookup(dtSalesman, "kid", "kid = '" & FixQuotes(drutama("sobagianpenjualan")) & "'", ".xxx. data not found .xxx.")
                            If strValMaster = ".xxx. data not found .xxx." Then
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & drutama("sobagianpenjualan") & "' doesn't exist in Salesman Data" : Trans.Rollback() : GoTo selesai
                            Else
                                drutama("sobagianpenjualan") = strValMaster
                            End If
                        End If

                        'SOEKSPEDISI
                        If Len(FixQuotes(drutama("soekspedisi"))) > 0 Then
                            strValMaster = AsDataTableDLookup(dtEkspedisi, "ekode", "ekode = '" & FixQuotes(drutama("soekspedisi")) & "'", ".xxx. data not found .xxx.")
                            If strValMaster = ".xxx. data not found .xxx." Then
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & drutama("soekspedisi") & "' doesn't exist in Expedition Data" : Trans.Rollback() : GoTo selesai
                            Else
                                drutama("soekspedisi") = strValMaster
                            End If
                        End If

                        'SOTERMIN
                        If Len(FixQuotes(drutama("sotermin"))) > 0 Then
                            strValMaster = AsDataTableDLookup(dtTermin, "trharijatuhtempo", "trkode = '" & FixQuotes(drutama("sotermin")) & "'", ".xxx. data not found .xxx.")
                            If strValMaster = ".xxx. data not found .xxx." Then
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & drutama("sotermin") & "' doesn't exist in Terms Data" : Trans.Rollback() : GoTo selesai
                            Else
                                drutama("sotgljatuhtempo") = AsFormatTanggal(Date.Parse(drutama("sotgl")).AddDays(Double.Parse(strValMaster)), "yyyy-MM-dd")
                            End If
                        End If

                        'SOMATAUANG
                        If Len(FixQuotes(drutama("somatauang"))) > 0 Then
                            dtMasterTemp = AsDataTableFilterSortDt(dtMatauang, "ckode = '" & FixQuotes(drutama("somatauang")) & "'")
                            If dtMasterTemp.Rows.Count > 0 Then
                                drutama("sokurs") = dtMasterTemp.Rows(0)("ckurs")
                            Else
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & drutama("somatauang") & "' doesn't exist in Currency Data" : Trans.Rollback() : GoTo selesai
                            End If
                        End If


                        'Proses detail
                        Dim strValueDetail As New StringBuilder
                        If (dtdetailNew.Rows.Count > 0) Then
                            For Each dr1 As DataRow In dtdetailNew.Rows

                                'VALIDASI MASTERDATA
                                'IDBARANG (bid, bkode, bnama, bnamaalias1, btipe, bkp, bsatuan, bsatuandefault)
                                If Len(FixQuotes(dr1("idbarang"))) > 0 Then
                                    dtMasterTemp = AsDataTableFilterSortDt(dtBarang, "bnamaalias1 = '" & FixQuotes(dr1("idbarang")) & "'")
                                    If dtMasterTemp.Rows.Count > 0 Then
                                        dr1("idbarang") = dtMasterTemp.Rows(0)("bid")
                                        dr1("namabarang") = dtMasterTemp.Rows(0)("bnama")
                                        dr1("tipebarang") = dtMasterTemp.Rows(0)("btipe")
                                        dr1("satuan") = dtMasterTemp.Rows(0)("bsatuandefault")
                                        dr1("satuanbarang") = dtMasterTemp.Rows(0)("bsatuan")
                                        vBkp = dtMasterTemp.Rows(0)("bkp")
                                    Else
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("idbarang") & "' doesn't exist in Item Data" : Trans.Rollback() : GoTo selesai
                                    End If
                                End If

                                'SATUAN
                                If Len(FixQuotes(dr1("satuan"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtSatuan, "unilai", "ukode = '" & FixQuotes(dr1("satuan")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("satuan") & "' doesn't exist in Unit Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        dr1("nilaisatuan") = Double.Parse(strValMaster)
                                        dr1("jmlbarang") = Double.Parse(dr1("jml")) * Double.Parse(dr1("nilaisatuan"))
                                    End If
                                End If

                                'MATAUANG, KURS, CABANG, LOKASI, GUDANG
                                dr1("matauang") = drutama("somatauang")
                                dr1("kurs") = drutama("sokurs")
                                dr1("cabang") = drutama("socabang")
                                dr1("lokasi") = drutama("solokasi")
                                dr1("gudang") = drutama("sogudang")

                                'PAJAK1
                                nilaipajak1 = 0
                                If vKpkp = 1 And vBkp = 1 Then
                                    dr1("pajak1") = vPajak1

                                    If Len(FixQuotes(dr1("pajak1"))) > 0 Then
                                        strValMaster = AsDataTableDLookup(dtPajak, "tnilai", "tkode = '" & FixQuotes(dr1("pajak1")) & "'", ".xxx. data not found .xxx.")
                                        If strValMaster = ".xxx. data not found .xxx." Then
                                            result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("pajak1") & "' doesn't exist in Tax Data" : Trans.Rollback() : GoTo selesai
                                        Else
                                            nilaipajak1 = Double.Parse(strValMaster)
                                            If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                                                dr1("jmlpajak1") = (nilaipajak1 / 100) * ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")))
                                            Else
                                                dr1("jmlpajak1") = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) / (100 + nilaipajak1)) * nilaipajak1
                                            End If
                                        End If
                                    End If
                                End If

                                'PAJAK2
                                If Len(FixQuotes(dr1("pajak2"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtPajak, "tnilai", "tkode = '" & FixQuotes(dr1("pajak2")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("pajak2") & "' doesn't exist in Tax Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                                            dr1("jmlpajak2") = (Double.Parse(strValMaster) / 100) * ((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon")))
                                        Else
                                            dr1("jmlpajak2") = (((Double.Parse(dr1("jml")) * Double.Parse(dr1("harga"))) - Double.Parse(dr1("jmldiskon"))) / (100 + nilaipajak1)) * Double.Parse(strValMaster)
                                        End If

                                    End If
                                End If

                                'COSTCENTER
                                If Len(FixQuotes(dr1("costcenter"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtCostcenter, "cckode", "cckode = '" & FixQuotes(dr1("costcenter")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("costcenter") & "' doesn't exist in Cost Center Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        dr1("costcenter") = strValMaster
                                    End If
                                End If

                                'DIVISI
                                If Len(FixQuotes(dr1("divisi"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtDivisi, "dkode", "dkode = '" & FixQuotes(dr1("divisi")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("divisi") & "' doesn't exist in Division Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        dr1("divisi") = strValMaster
                                    End If
                                End If

                                'SUBDIVISI
                                If Len(FixQuotes(dr1("subdivisi"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtSubdivisi, "sdkode", "sdkode = '" & FixQuotes(dr1("subdivisi")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("subdivisi") & "' doesn't exist in Sub Division Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        dr1("subdivisi") = strValMaster
                                    End If
                                End If

                                'PROYEK
                                If Len(FixQuotes(dr1("proyek"))) > 0 Then
                                    strValMaster = AsDataTableDLookup(dtProyek, "pkode", "pkode = '" & FixQuotes(dr1("proyek")) & "'", ".xxx. data not found .xxx.")
                                    If strValMaster = ".xxx. data not found .xxx." Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & dr1("proyek") & "' doesn't exist in Project Data" : Trans.Rollback() : GoTo selesai
                                    Else
                                        dr1("proyek") = strValMaster
                                    End If
                                End If

                                'QUERY INSERT
                                strValueDetail.Append(IIf(Len(strValueDetail.ToString) = 0, "", ", "))
                                strValueDetail.Append("(" & dr1("idsodetail") & ", .xx.idsoutama.xx., " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("harga")) & "', '" & FixDouble(dr1("diskon")) & "', '" & FixDouble(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idsqdetail") & ", '" & FixDouble(dr1("jmlpl")) & "', " & dr1("statuspl") & ", '" & FixDouble(dr1("jmldo")) & "', " & dr1("statusdo") & ", '" & FixDouble(dr1("jmldr")) & "', " & dr1("statusdr") & ", '" & FixDouble(dr1("jmlpi")) & "', " & dr1("statuspi") & ", '" & FixDouble(dr1("jmlsi")) & "', " & dr1("statussi") & ", '" & FixDouble(dr1("jmlrnr")) & "', " & dr1("statusrnr") & ", '" & FixDouble(dr1("jmlsr")) & "', " & dr1("statussr") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & "')")
                            Next

                        Else
                            result(2) = "Detail Transaction data not found." : Trans.Rollback() : GoTo selesai

                        End If


                        'VALIDASI SIMPAN ========================================
                        'ValidasiSimpan
                        If drutama("sostatus") = 2 Then
                            'VALIDASI HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
                            '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                            '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid
                            Dim rsHakAksesLowerPrice As String = HakAksesLowerPrice(5, 10, 8, userid, dtdetailNew, ftLowerPrice) 'MODULEID, MENUID, INDEKS AKSES, USERID, DATA DETAIL, FILTER BARANG SESUAI TRANSAKSI
                            If Len(rsHakAksesLowerPrice) <> 0 Then result(2) = "PO Customer : " & drutama("sonoref") & " - '" & rsHakAksesLowerPrice : Trans.Rollback() : GoTo selesai

                            Dim rsValidasi As String = ValidasiSimpan(dtdetailNew, ftExistOutstanding, ftOutstanding, ftSQ, "", "", "", drutama("sohargatermasukpajak"))
                            If Len(rsValidasi) > 0 Then result(2) = "PO Customer : " & drutama("sonoref") & " - '" & rsValidasi : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF VALIDASI SIMPAN =================================


                        ''SET TGL JATUH TEMPO ====================================
                        'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                        'rsTglJT = F_TglJT(drutama("sotermin").ToString, AsFormatTanggal(drutama("sotgl")), "sotgl").Split(sptSubParam)
                        'If rsTglJT(0) = 0 Then
                        '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                        'Else
                        '    drutama("sotgljatuhtempo") = AsFormatTanggal(rsTglJT(1))
                        'End If
                        ''END OF SET TGL JATUH TEMPO =============================


                        'PERHITUNGAN TOTAL UTAMA ================================
                        'DIAMBILKAN DARI DATA DETAIL

                        'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                        'SUBTOTAL = (jml * harga) - jmldiskon
                        AsDataTableTambahField(dtdetailNew, "subtotal", AsEnumTypeData.AsDouble)
                        dtdetailNew.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                        'TOTAL = subtotal
                        drutama("sototal") = AsDataTableDSum(dtdetailNew, "subtotal")

                        'TOTALPAJAK1 = jmlpajak1
                        drutama("sototalpajak1detail") = AsDataTableDSum(dtdetailNew, "jmlpajak1")

                        'TOTALPAJAK2 = jmlpajak2
                        drutama("sototalpajak2detail") = AsDataTableDSum(dtdetailNew, "jmlpajak2")

                        'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                        'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                        If Integer.Parse(drutama("sohargatermasukpajak")) = 0 Then
                            'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                            drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak1detail")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                        Else
                            'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                            drutama("sototaltransaksi") = Double.Parse(drutama("sototal")) - Double.Parse(drutama("sojmldiskon")) + Double.Parse(drutama("sototalpajak2detail")) + Double.Parse(drutama("sobiayalain"))

                        End If
                        'END OF PERHITUNGAN TOTAL UTAMA =========================


                        If isUpdate Then
                            result(4) = drutama("soid")
                            notransaksi = drutama("sonotransaksi")
                            'JIKA UPDATE CEK JML ROW PADA DATABASE
                            dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(soid), sonotransaksi FROM M5_so WHERE soid='" & result(4) & "' AND sostatus NOT IN(2,3,4,7)", myConn)
                            rowUpdate = dtupdate.Rows(0)(0)

                            If (rowUpdate > 0) Then

                                'CEK NO TRANSAKSI ======================
                                If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                                    If cekNo > 0 Then
                                        result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                                    End If
                                End If
                                'END OF CEK NO TRANSAKSI ===============

                                'SIMPAN HISTORY ========================
                                Dim SimpanHistory As New m5_so_history
                                Dim rsSimpanHistory As String = SimpanHistory.M5_So_HistorySimpan("" & paramSplit(0) & "★M5_So_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("sosumber")) & "▼" & FixQuotes(drutama("soid")) & "")
                                Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                                Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                                'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                                If (rsSplitResult(1) = 0) Then
                                    result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                                End If
                                'END OF SIMPAN HISTORY ==================

                                sql = "Update M5_So set socabang  = '" & FixQuotes(drutama("socabang")) & "', solokasi  = '" & FixQuotes(drutama("solokasi")) & "', sogudang  = '" & FixQuotes(drutama("sogudang")) & "', soasalbarang  = '" & FixQuotes(drutama("soasalbarang")) & "', soasalbarangkategori  = " & drutama("soasalbarangkategori") & ", sojenispenjualan  = '" & FixQuotes(drutama("sojenispenjualan")) & "', sojenispenjualankategori  = " & drutama("sojenispenjualankategori") & ", socarabayar  = " & drutama("socarabayar") & ", sosumber  = '" & FixQuotes(drutama("sosumber")) & "', soautonotransaksi  = " & drutama("soautonotransaksi") & ", sonotransaksi  = '" & FixQuotes(notransaksi) & "', sotgl  = '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', sokodepa  = " & drutama("sokodepa") & ", socustomer  = " & drutama("socustomer") & ", socustomerkontak  = '" & FixQuotes(drutama("socustomerkontak")) & "', so1alamat1  = '" & FixQuotes(drutama("so1alamat1")) & "', so1alamat2  = '" & FixQuotes(drutama("so1alamat2")) & "', so1alamat3  = '" & FixQuotes(drutama("so1alamat3")) & "', so2alamat1  = '" & FixQuotes(drutama("so2alamat1")) & "', so2alamat2  = '" & FixQuotes(drutama("so2alamat2")) & "', so2alamat3  = '" & FixQuotes(drutama("so2alamat3")) & "', sobagianpenjualan  = " & drutama("sobagianpenjualan") & ", soekspedisi  = '" & FixQuotes(drutama("soekspedisi")) & "', sotglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', sotermin  = '" & FixQuotes(drutama("sotermin")) & "', sotgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', souraian  = '" & FixQuotes(drutama("souraian")) & "', socatatan  = '" & FixQuotes(drutama("socatatan")) & "', sonoref  = '" & FixQuotes(drutama("sonoref")) & "', sotglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', sotglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', somatauang  = '" & FixQuotes(drutama("somatauang")) & "', sokurs  = '" & FixDouble(drutama("sokurs")) & "', sohargatermasukpajak  = " & drutama("sohargatermasukpajak") & ", sototal  = '" & FixDouble(drutama("sototal")) & "', sodiskonpersen  = '" & FixQuotes(drutama("sodiskonpersen")) & "', sojmldiskon  = '" & FixDouble(drutama("sojmldiskon")) & "', sototalpajak1detail  = '" & FixDouble(drutama("sototalpajak1detail")) & "', sototalpajak2detail  = '" & FixDouble(drutama("sototalpajak2detail")) & "', sobiayalainpersen  = '" & FixDouble(drutama("sobiayalainpersen")) & "', sobiayalain  = '" & FixDouble(drutama("sobiayalain")) & "', sototaltransaksi  = '" & FixDouble(drutama("sototaltransaksi")) & "', sojmlbayar  = '" & FixDouble(drutama("sojmlbayar")) & "', sorekdiskon  = '" & FixQuotes(drutama("sorekdiskon")) & "', sorekpajak1  = '" & FixQuotes(drutama("sorekpajak1")) & "', sorekpajak2  = '" & FixQuotes(drutama("sorekpajak2")) & "', sorekbiayalain  = '" & FixQuotes(drutama("sorekbiayalain")) & "', sorekbayar  = '" & FixQuotes(drutama("sorekbayar")) & "', soidsq  = " & drutama("soidsq") & ", sostatuspl  = " & drutama("sostatuspl") & ", sostatusdo  = " & drutama("sostatusdo") & ", sostatusdr  = " & drutama("sostatusdr") & ", sostatuspi  = " & drutama("sostatuspi") & ", sostatussi  = " & drutama("sostatussi") & ", sostatusrnr  = " & drutama("sostatusrnr") & ", sostatussr  = " & drutama("sostatussr") & ", sostatus  = " & drutama("sostatus") & ", sostatussebelumnya  = " & drutama("sostatussebelumnya") & ", sojmlrevisi  = sojmlrevisi+1, socetakanke  = " & drutama("socetakanke") & ", somodifikasiuser  = " & drutama("somodifikasiuser") & ", somodifikasitgl  = NOW(), socustomtext1  = '" & FixQuotes(drutama("socustomtext1")) & "', socustomtext2  = '" & FixQuotes(drutama("socustomtext2")) & "', socustomtext3  = '" & FixQuotes(drutama("socustomtext3")) & "', socustomtext4  = '" & FixQuotes(drutama("socustomtext4")) & "', socustomtext5  = '" & FixQuotes(drutama("socustomtext5")) & "', socustomint1  = " & drutama("socustomint1") & ", socustomint2  = " & drutama("socustomint2") & ", socustomint3  = " & drutama("socustomint3") & ", socustomdbl1  = '" & FixDouble(drutama("socustomdbl1")) & "', socustomdbl2  = '" & FixDouble(drutama("socustomdbl2")) & "', socustomdbl3  = '" & FixDouble(drutama("socustomdbl3")) & "', socustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', socustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', socustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "' where soid = '" & drutama("soid") & "'"
                                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                With objCmd
                                    .Connection = myConn
                                    .Transaction = Trans
                                    .CommandType = CommandType.Text
                                    .CommandText = sql
                                End With
                                objCmd.ExecuteNonQuery()

                            Else
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "Can't update No. : '" & notransaksi & "' - it has been approved." : Trans.Rollback() : GoTo selesai
                            End If

                        Else

                            If drutama("soautonotransaksi") = 1 Then

                                'GENERATE NOTRANSAKSI =========================================
                                Dim wsM0_Nomor As New m0_nomor
                                Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("socabang"), drutama("solokasi"), drutama("sosumber"), drutama("sotgl"))
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
                                    result(2) = "PO Customer : " & drutama("sonoref") & " - '" & arrNotransaksi(1) : Trans.Rollback() : GoTo selesai
                                End If
                                'END OF GENERATE NOTRANSAKSI ==================================

                            Else
                                notransaksi = drutama("sonotransaksi")
                            End If

                            'CEK NO TRANSAKSI ======================
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(soid) FROM m5_so WHERE sonotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                            'END OF CEK NO TRANSAKSI ===============

                            sql = "Insert into M5_So (socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspl, sostatusdo, sostatusdr, sostatuspi, sostatussi, sostatusrnr, sostatussr, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3) values('" & FixQuotes(drutama("socabang")) & "', '" & FixQuotes(drutama("solokasi")) & "', '" & FixQuotes(drutama("sogudang")) & "', '" & FixQuotes(drutama("soasalbarang")) & "', " & drutama("soasalbarangkategori") & ", '" & FixQuotes(drutama("sojenispenjualan")) & "', " & drutama("sojenispenjualankategori") & ", " & drutama("socarabayar") & ", '" & FixQuotes(drutama("sosumber")) & "', " & drutama("soautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgl"))) & "', " & drutama("sokodepa") & ", " & drutama("socustomer") & ", '" & FixQuotes(drutama("socustomerkontak")) & "', '" & FixQuotes(drutama("so1alamat1")) & "', '" & FixQuotes(drutama("so1alamat2")) & "', '" & FixQuotes(drutama("so1alamat3")) & "', '" & FixQuotes(drutama("so2alamat1")) & "', '" & FixQuotes(drutama("so2alamat2")) & "', '" & FixQuotes(drutama("so2alamat3")) & "', " & drutama("sobagianpenjualan") & ", '" & FixQuotes(drutama("soekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglkirim"))) & "', '" & FixQuotes(drutama("sotermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotgljatuhtempo"))) & "', '" & FixQuotes(drutama("souraian")) & "', '" & FixQuotes(drutama("socatatan")) & "', '" & FixQuotes(drutama("sonoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("sotglpenutupan"))) & "', '" & FixQuotes(drutama("somatauang")) & "', '" & FixDouble(drutama("sokurs")) & "', " & drutama("sohargatermasukpajak") & ", '" & FixDouble(drutama("sototal")) & "', '" & FixQuotes(drutama("sodiskonpersen")) & "', '" & FixDouble(drutama("sojmldiskon")) & "', '" & FixDouble(drutama("sototalpajak1detail")) & "', '" & FixDouble(drutama("sototalpajak2detail")) & "', '" & FixDouble(drutama("sobiayalainpersen")) & "', '" & FixDouble(drutama("sobiayalain")) & "', '" & FixDouble(drutama("sototaltransaksi")) & "', '" & FixDouble(drutama("sojmlbayar")) & "', '" & FixQuotes(drutama("sorekdiskon")) & "', '" & FixQuotes(drutama("sorekpajak1")) & "', '" & FixQuotes(drutama("sorekpajak2")) & "', '" & FixQuotes(drutama("sorekbiayalain")) & "', '" & FixQuotes(drutama("sorekbayar")) & "', " & drutama("soidsq") & ", " & drutama("sostatuspl") & ", " & drutama("sostatusdo") & ", " & drutama("sostatusdr") & ", " & drutama("sostatuspi") & ", " & drutama("sostatussi") & ", " & drutama("sostatusrnr") & ", " & drutama("sostatussr") & ", " & drutama("sostatus") & ", " & drutama("sostatussebelumnya") & ", " & drutama("sojmlrevisi") & ", " & drutama("socetakanke") & ", " & drutama("soinputuser") & ", NOW(), " & drutama("somodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("soisclose") & ", '" & FixQuotes(drutama("socustomtext1")) & "', '" & FixQuotes(drutama("socustomtext2")) & "', '" & FixQuotes(drutama("socustomtext3")) & "', '" & FixQuotes(drutama("socustomtext4")) & "', '" & FixQuotes(drutama("socustomtext5")) & "', " & drutama("socustomint1") & ", " & drutama("socustomint2") & ", " & drutama("socustomint3") & ", '" & FixDouble(drutama("socustomdbl1")) & "', '" & FixDouble(drutama("socustomdbl2")) & "', '" & FixDouble(drutama("socustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("socustomdate3"))) & "')"
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
                            dt2 = AsDataTableAmbilDariDBCon("select soid from M5_so where sonotransaksi='" & notransaksi & "' AND soinputuser= '" & userid & "' order by somodifikasitgl desc limit 1", myConn)
                            If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                        End If

                        'Hapus detail ketika update
                        If (isUpdate) Then
                            sql = "Delete from M5_So_Detail where idso = '" & result(4) & "'"
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
                        If (Len(strValueDetail.ToString) > 0) Then
                            sql = "Insert into M5_So_Detail(idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlpi, statuspi, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValueDetail.ToString.Replace(".xx.idsoutama.xx.", result(4)) & ""
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()
                        End If


                        If drutama("sostatus") = 2 Then
                            If Len(updNilai) > 0 Then
                                'UPDATE OUTSTANDING TRANSAKSI =======================================================
                                'UPDATE DETAIL
                                sql = "UPDATE m5_sq_detail SET jmlrealisasi = (CASE idsqdetail " & updNilai & " ELSE jmlrealisasi END) WHERE " & updFilter
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
                                Dim dtOut As DataTable = AsDataTableAmbilDariDBCon("SELECT idsq FROM m5_sq_detail WHERE " & updFilter & " GROUP BY idsq", myConn)
                                If dtOut.Rows.Count > 0 Then
                                    For Each dr1 As DataRow In dtOut.Rows
                                        ftDetail = IIf(Len(ftDetail.ToString) = 0, "", ftDetail & " OR ")
                                        ftDetail = String.Concat(ftDetail, "(idsq = '" & dr1("idsq") & "')")
                                    Next
                                End If
                                dtOut = AsDataTableAmbilDariDBCon("SELECT idsq, SUM(jmlbarang) as jmlbarang, SUM(jmlrealisasi) as jmlrealisasi FROM m5_sq_detail WHERE " & ftDetail & " GROUP BY idsq", myConn)
                                If dtOut.Rows.Count > 0 Then
                                    'KOSONGKAN VARIABEL NILAI DAN FILTER
                                    updNilai = "" : updFilter = ""
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
                                        updNilai = String.Concat(updNilai, "WHEN '" & dr1("idsq") & "' THEN '" & statusOut & "' ")
                                        '3. SET FILTERUPDATE OUTSTANDING
                                        updFilter = IIf(Len(updFilter.ToString) = 0, "", updFilter & " OR ")
                                        updFilter = String.Concat(updFilter, "(sqid = '" & dr1("idsq") & "')")
                                    Next

                                    sql = "UPDATE m5_sq SET sqstatusrealisasi = (CASE sqid " & updNilai & " ELSE sqstatusrealisasi END) WHERE " & updFilter
                                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                    With objCmd
                                        .Connection = myConn
                                        .Transaction = Trans
                                        .CommandType = CommandType.Text
                                        .CommandText = sql
                                    End With
                                    objCmd.ExecuteNonQuery()
                                End If
                                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================
                            End If

                            'UPDATE STOK BOOKING ================================================================
                            'BOOKING HANYA UNTUK BARANG YG HPP NYA BUKAN KHUSUS (I)
                            sql = "INSERT INTO m1_item_booking (SELECT idbarang, gudang, jmlbarang FROM m5_so_detail JOIN m1_item ON idbarang = bid AND bjenis <> 'J' AND bhpp <> 'I' AND idso = '" & result(4) & "') ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

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

                        'INSERT USER LOG ====================================================================
                        Dim sumber As String = "SO", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
                        'ambil moduleid dan menuid dari m0_nomor
                        Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "'", myConn)
                        If dtnomor.Rows.Count > 0 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) Else result(2) = "PO Customer : " & drutama("sonoref") & " - '" & "Can't find '" & sumber & "' in M0_Nomor." : Trans.Rollback() : GoTo selesai
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

                        result(2) = notransaksi
                        result(3) = 0
                        result(4) = result(4)

                    Else
                        result(2) = "#1. Main transaction data not found." : Trans.Rollback() : GoTo selesai

                    End If

                    System.Threading.Thread.Sleep(1000)

                Next

                result(1) = 1

            Else
                result(2) = "#2. Main transaction data not found." : Trans.Rollback() : GoTo selesai

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
    Public Function M5_So_Detail_Outstanding(ByVal param As String) As String
        'M5_So_Detail_VSearch --------------------------------------------------------
        'soid, sonotransaksi, sonoref, sotgl, socustomer, sobagianpenjualan, souraian, sotglkirim, 
        'bkode, namabarang, catatan, jml, satuan, jmlrealisasi, jmlsisa, kcatatan, customdbl3, 
        'idsodetail, bkategori, bapanjang, baoem, baukuran, banozzle, bakonversikgpcs, sorefpo, notesrc, bavolume, 
        'bmaterial, bmaterialnama, bmaterialstok, bmaterialonorder, toleransi, bmin1, bmax1, bmin2, bmax2, 
        'nokontrak, ukuran

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
            Filter = Filter.Replace("idbarang", "sod.idbarang")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sol = query.PanggilQuery("m5_so_detail_v")
        sol = "SELECT so.soid , so.sonotransaksi ,so.sonoref, so.sotgl , kc.knama AS socustomer , ks.knama AS sobagianpenjualan , so.souraian , so.sotglkirim, b.bkode , sod.namabarang , sod.catatan ,  sod.jml ,  sod.satuan,  (sod.jmlrealisasi / sod.nilaisatuan) AS jmlrealisasi ,  (sod.jml - (sod.jmlrealisasi / sod.nilaisatuan)) AS jmlsisa, kc.kcatatan as kcatatan, sod.customdbl3 , sod.idsodetail, ic.icnama as bkategori, b.bapanjang, oem.onama as baoem, b.baukuran, b.bamodel as banozzle, b.bjmllapangan as bakonversikgpcs, so.sonoref as sorefpo, b.bnamaalias5 as notesrc, b.bavolume, b2.bkode as bmaterial, b2.bnama as bmaterialnama, b2.bstok as bmaterialstok, IFNULL(SUM(bp.jmlbooking),0) AS bmaterialonorder , sod.jmlrnr as toleransi, b.bcustom1 as bmin1, b.bcustom2 as bmax1, b.bcustom3 as bmin2, b.bcustom4 as bmax2, sf.sfnotransaksi as nokontrak, CASE  WHEN b.baukuran LIKE '%BMW%' THEN RIGHT(b.baukuran, LENGTH(b.baukuran)-8) WHEN b.baukuran LIKE '%BW%' THEN RIGHT(b.baukuran, LENGTH(b.baukuran)-7) ELSE b.baukuran END ukuran	FROM m5_so so JOIN m5_so_detail sod on so.soid = sod.idso AND so.sostatus in(2,3) AND sod.jml > sod.jmlrealisasi JOIN m1_item b ON sod.idbarang = b.bid JOIN m1_item_category ic ON b.bkategori = ic.ickode JOIN m1_contact kc ON so.socustomer = kc.kid JOIN m1_contact ks ON so.sobagianpenjualan = ks.kid LEFT JOIN m1_oem oem ON b.baoem = oem.okode LEFT JOIN m1_item b2 ON b.bmaterial = b2.bkode LEFT JOIN m1_item_booking_po bp ON b2.bid = bp.idbarang LEFT JOIN m5_sf sf ON so.socustomdbl1 = sf.sfid"

        dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "so.sotgl, so.soid, so.sonotransaksi, sod.urutan, sod.idsodetail", sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("sonoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), ""), sptField,
                     FxDB(dr("souraian"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("namabarang"), ""), sptField,
                     FxDB(dr("catatan"), ""), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("jmlrealisasi"), 0), sptField,
                     FxDB(dr("jmlsisa"), 0), sptField,
                     FxDB(dr("kcatatan"), ""), sptField,
                     FxDB(dr("customdbl3"), 0), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("bkategori"), ""), sptField,
                     FxDB(dr("bapanjang"), ""), sptField,
                     FxDB(dr("baoem"), ""), sptField,
                     FxDB(dr("baukuran"), ""), sptField,
                     FxDB(dr("banozzle"), ""), sptField,
                     FxDB(dr("bakonversikgpcs"), 0), sptField,
                     FxDB(dr("sorefpo"), ""), sptField,
                     FxDB(dr("notesrc"), ""), sptField,
                     FxDB(dr("bavolume"), 0), sptField,
                     FxDB(dr("bmaterial"), ""), sptField,
                     FxDB(dr("bmaterialnama"), ""), sptField,
                     FxDB(dr("bmaterialstok"), 0), sptField,
                     FxDB(dr("bmaterialonorder"), 0), sptField,
                     FxDB(dr("toleransi"), 0), sptField,
                     FxDB(dr("bmin1"), 0), sptField,
                     FxDB(dr("bmax1"), 0), sptField,
                     FxDB(dr("bmin2"), 0), sptField,
                     FxDB(dr("bmax2"), 0), sptField,
                     FxDB(dr("nokontrak"), ""), sptField,
                     FxDB(dr("ukuran"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, sonotransaksi, sonoref, sotgl, socustomer, sobagianpenjualan, souraian, sotglkirim, bkode, namabarang, catatan, jml, satuan, jmlrealisasi, jmlsisa, kcatatan, customdbl3, idsodetail, bkategori, bapanjang, baoem, baukuran, banozzle, bakonversikgpcs, sorefpo, notesrc, bavolume, bmaterial, bmaterialnama, bmaterialstok, bmaterialonorder, toleransi, bmin1, bmax1, bmin2, bmax2, nokontrak, ukuran"))

        Return wsResult
    End Function

End Class