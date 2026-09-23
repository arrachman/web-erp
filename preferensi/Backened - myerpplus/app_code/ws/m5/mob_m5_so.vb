Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class mob_m5_so
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function MobM5_SoSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", sonoref As String = ""

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
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
        isUpdate = False
        'END OF VALIDASI DAN SET USERID ====================================================

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
        If Len(dataUtama(29)) = 0 Then
            result(2) = "sonoref can't be empty" : GoTo selesai
        ElseIf Len(dataUtama(29)) <> 8 Then
            result(2) = "sonoref format is wrong" : GoTo selesai
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

        sonoref = dataUtama(29)
        Dim dt As DataTable
        sql = "SELECT sonoref FROM m5_so WHERE sonoref = '" + sonoref + "'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            result(1) = 1 : GoTo selesai
        Else
            Dim mobuserid As Integer = sonoref.Substring(1, 3)
            Dim noberikutnya As Integer = sonoref.Substring(4, 4)
            sql = "UPDATE `m0_nomor_mobile` SET noberikutnya = '" + noberikutnya.ToString + "' WHERE userid = '" + mobuserid.ToString + "'"
            dt = AsDataTableAmbilDariDB(sql)
        End If

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
            If idsqdetail <> 0 Then
                '1. CEK DATA EXIST
                ftExistOutstanding = IIf(Len(ftExistOutstanding.ToString) = 0, "", ftExistOutstanding & " UNION ")
                ftExistOutstanding = String.Concat(ftExistOutstanding, "SELECT EXISTS(SELECT 1 FROM m5_sq_detail JOIN m5_sq ON idsq = sqid WHERE idsqdetail = '" & idsqdetail & "' AND (sqstatus = 2 OR sqstatus = 3 OR sqstatus = 4 OR sqstatus = 7) LIMIT 1) as rowExists, '" & idsqdetail & "' as idsqdetail, bkode FROM m1_item WHERE bid = '" & idbarang & "'")

                '2. CEK JML OUTSTANDING
                Dim Outstanding As Double = AsDataTableDSum(dtdetail, "jmlbarang", "idsqdetail=" & idsqdetail)
                ftOutstanding = IIf(Len(ftOutstanding.ToString) = 0, "", ftOutstanding & " OR ")
                ftOutstanding = String.Concat(ftOutstanding, " (sqd.idsqdetail = " & idsqdetail & " AND " & Outstanding & " > (sqd.jmlbarang - sqd.jmlrealisasi)) ")

                '3. SET NILAI UPDATE OUTSTANDING
                updNilai = String.Concat("WHEN '" & idsqdetail & "' THEN jmlrealisasi + '" & Outstanding & "' ", updNilai)

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

                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstanding, ftOutstanding)
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
                'MobM5_SoSearch(param)
                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)

                'AMBIL DATA =============================================================
                'Return MobM5_So_AllSearch(PostWsSearch(paramSplit(0), "MobM5_So_AllSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
                'Dim hasilSearch As New RsHasilWsSearch
                'hasilSearch = GetWsSearch(paramSearch)

                ''result(1) = hasilSearch.success
                ''result(2) = hasilSearch.errmessage

                'resultPaging(0) = hasilSearch.isPaging
                'resultPaging(1) = hasilSearch.isNext
                'resultPaging(2) = hasilSearch.isPrevious
                'resultPaging(3) = hasilSearch.countPage
                'resultPaging(4) = hasilSearch.countRow

                'search = hasilSearch.data
                'END OF AMBIL DATA ======================================================
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
    Public Function MobM5_SoSimpanAll(ByVal param As String) As String
        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama, dataDetail, dataRowDetail() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", deviceUUID As String = ""

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim searchDetail As String = ""

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        pagingSplit = paramSplit(2).Split(sptSubParam)
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

        'Replace disesuaikan dengan kebutuhan
        If (pagingSplit(2).Length > 0) Then
            Filter = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'VALIDASI DAN SET ISUPDATE =========================================================
        'CEK ISUPDATE
        isUpdate = False
        'Device UUID
        If (Len(paramSplit(4)) = 0) Then
            result(2) = "Device UUID can't be empty" : GoTo selesai
        Else
            deviceUUID = paramSplit(4).ToString
        End If

        'Query (cek userid apa sudah ada di tabel)
        sql = "SELECT * FROM m0_nomor_mobile WHERE macaddress = '" + deviceUUID + "' AND userid = " + userid
        Dim dt As DataTable = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count = 0 Then
            result(2) = "User has logged on other device." : GoTo selesai
        End If

        'END OF VALIDASI DAN SET USERID ====================================================
        Dim returnString As String, hasilString As String
        If Len(paramSplit(5)) > 0 Then
            dataSplit = paramSplit(5).Split(sptLogin)
            For i = 0 To dataSplit.Length - 1
                dataRowDetail = dataSplit(i).Split(sptSubParam)
                If dataRowDetail.Length <> 2 Then
                    result(2) = "invalid main data and details" : GoTo selesai
                End If

                dataUtama = dataRowDetail(0)
                dataDetail = dataRowDetail(1)

                If Len(dataUtama) = 0 Then
                    result(2) = "Main data can't be empty." : GoTo selesai
                End If

                If Len(dataDetail) = 0 Then
                    result(2) = "Detail data can't be empty." : GoTo selesai
                End If
            Next

            Dim dataParam As String = String.Concat(paramSplit(0), sptParam, paramSplit(1), sptParam, paramSplit(2), sptParam, paramSplit(3), sptParam, paramSplit(4))
            For i = 0 To dataSplit.Length - 1
                dataRowDetail = dataSplit(i).Split(sptSubParam)

                dataUtama = dataRowDetail(0)
                dataDetail = dataRowDetail(1)
                returnString = MobM5_SoSimpan(String.Concat(dataParam + sptParam + dataUtama + sptSubParam + dataDetail))
                hasilString = returnString.Split(sptParam)(0).ToString.Split(sptSubParam)(1)
                If hasilString = "0" Then
                    Return "MobM5_SoSimpan : " + returnString
                End If
            Next

        End If
        dt = AmbilData("aplikasi1-M5_So", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), ""), sptField,
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
                     FxDB(dr("sokodepa"), ""), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), ""), sptField,
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
                     FxDB(dr("sobiayalainpersen"), ""), sptField,
                     FxDB(dr("sobiayalain"), 0), sptField,
                     FxDB(dr("sototaltransaksi"), 0), sptField,
                     FxDB(dr("sojmlbayar"), 0), sptField,
                     FxDB(dr("sorekdiskon"), ""), sptField,
                     FxDB(dr("sorekpajak1"), ""), sptField,
                     FxDB(dr("sorekpajak2"), ""), sptField,
                     FxDB(dr("sorekbiayalain"), ""), sptField,
                     FxDB(dr("sorekbayar"), ""), sptField,
                     FxDB(dr("soidsq"), ""), sptField,
                     FxDB(dr("sostatuspi"), 0), sptField,
                     FxDB(dr("sostatuspl"), 0), sptField,
                     FxDB(dr("sostatusdo"), 0), sptField,
                     FxDB(dr("sostatusdr"), 0), sptField,
                     FxDB(dr("sostatussi"), 0), sptField,
                     FxDB(dr("sostatusrnr"), 0), sptField,
                     FxDB(dr("sostatussr"), 0), sptField,
                     FxDB(dr("sostatusrealisasi"), 0), sptField,
                     FxDB(dr("sostatus"), 0), sptField,
                     FxDB(dr("sostatussebelumnya"), 0), sptField,
                     FxDB(dr("sojmlrevisi"), 0), sptField,
                     FxDB(dr("socetakanke"), 0), sptField,
                     FxDB(dr("soinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("somodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soisclose"), 0), sptField,
                     FxDB(dr("socustomtext1"), ""), sptField,
                     FxDB(dr("socustomtext2"), ""), sptField,
                     FxDB(dr("socustomtext3"), ""), sptField,
                     FxDB(dr("socustomtext4"), ""), sptField,
                     FxDB(dr("socustomtext5"), ""), sptField,
                     FxDB(dr("socustomint1"), 0), sptField,
                     FxDB(dr("socustomint2"), 0), sptField,
                     FxDB(dr("socustomint3"), 0), sptField,
                     FxDB(dr("socustomdbl1"), 0), sptField,
                     FxDB(dr("socustomdbl2"), 0), sptField,
                     FxDB(dr("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            sql = "select `sod`.`idsodetail` AS `idsodetail`,`sod`.`idso` AS `idso`,`sod`.`idbarang` AS `idbarang`,`sod`.`namabarang` AS `namabarang`,`sod`.`tipebarang` AS `tipebarang`,`sod`.`jml` AS `jml`,`sod`.`satuan` AS `satuan`,`sod`.`nilaisatuan` AS `nilaisatuan`,`sod`.`jmlbarang` AS `jmlbarang`,`sod`.`satuanbarang` AS `satuanbarang`,`sod`.`matauang` AS `matauang`,`sod`.`kurs` AS `kurs`,`sod`.`harga` AS `harga`,`sod`.`diskon` AS `diskon`,`sod`.`jmldiskon` AS `jmldiskon`,`sod`.`pajak1` AS `pajak1`,`sod`.`jmlpajak1` AS `jmlpajak1`,`sod`.`pajak2` AS `pajak2`,`sod`.`jmlpajak2` AS `jmlpajak2`,`sod`.`cabang` AS `cabang`,`sod`.`lokasi` AS `lokasi`,`sod`.`gudang` AS `gudang`,`sod`.`costcenter` AS `costcenter`,`sod`.`divisi` AS `divisi`,`sod`.`subdivisi` AS `subdivisi`,`sod`.`proyek` AS `proyek`,`sod`.`catatan` AS `catatan`,`sod`.`urutan` AS `urutan`,`sod`.`idsqdetail` AS `idsqdetail`,`sod`.`jmlpi` AS `jmlpi`,`sod`.`statuspi` AS `statuspi`,`sod`.`jmlpl` AS `jmlpl`,`sod`.`statuspl` AS `statuspl`,`sod`.`jmldo` AS `jmldo`,`sod`.`statusdo` AS `statusdo`,`sod`.`jmldr` AS `jmldr`,`sod`.`statusdr` AS `statusdr`,`sod`.`jmlsi` AS `jmlsi`,`sod`.`statussi` AS `statussi`,`sod`.`jmlrnr` AS `jmlrnr`,`sod`.`statusrnr` AS `statusrnr`,`sod`.`jmlsr` AS `jmlsr`,`sod`.`statussr` AS `statussr`,`sod`.`jmlrealisasi` AS `jmlrealisasi`,`sod`.`statusrealisasi` AS `statusrealisasi`,`sod`.`isclose` AS `isclose`,`sod`.`customtext1` AS `customtext1`,`sod`.`customtext2` AS `customtext2`,`sod`.`customtext3` AS `customtext3`,`sod`.`customdbl1` AS `customdbl1`,`sod`.`customdbl2` AS `customdbl2`,`sod`.`customdbl3` AS `customdbl3`,`sod`.`customdate1` AS `customdate1`,`sod`.`customdate2` AS `customdate2`,`sod`.`customdate3` AS `customdate3` from (`m5_so` `so` join `m5_so_detail` `sod` on((`so`.`soid` = `sod`.`idso`)))"

            dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            pg1 = pg1
            If dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    searchDetail = String.Concat(searchDetail,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
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
                         FxDB(dr("idsqdetail"), ""), sptField,
                         FxDB(dr("jmlpi"), 0), sptField,
                         FxDB(dr("statuspi"), 0), sptField,
                         FxDB(dr("jmlpl"), 0), sptField,
                         FxDB(dr("statuspl"), 0), sptField,
                         FxDB(dr("jmldo"), 0), sptField,
                         FxDB(dr("statusdo"), 0), sptField,
                         FxDB(dr("jmldr"), 0), sptField,
                         FxDB(dr("statusdr"), 0), sptField,
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
                         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
                Next
                searchDetail = searchDetail.Substring(0, searchDetail.Length - sptRow.Length)
            End If

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, searchDetail)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspi, sostatuspl, sostatusdo, sostatusdr, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3"), sptSubParam, ReplaceMapping("idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpi, statuspi, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM5_SoSearch(ByVal param As String) As String
        'MobM5_SoSearch --------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspi, sostatuspl, sostatusdo, sostatusdr, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, 
        'socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, 
        'socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
        'End If
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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_so_v")

        dt = AmbilData("aplikasi1-M5_So", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), ""), sptField,
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
                     FxDB(dr("sokodepa"), ""), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), ""), sptField,
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
                     FxDB(dr("sobiayalainpersen"), ""), sptField,
                     FxDB(dr("sobiayalain"), 0), sptField,
                     FxDB(dr("sototaltransaksi"), 0), sptField,
                     FxDB(dr("sojmlbayar"), 0), sptField,
                     FxDB(dr("sorekdiskon"), ""), sptField,
                     FxDB(dr("sorekpajak1"), ""), sptField,
                     FxDB(dr("sorekpajak2"), ""), sptField,
                     FxDB(dr("sorekbiayalain"), ""), sptField,
                     FxDB(dr("sorekbayar"), ""), sptField,
                     FxDB(dr("soidsq"), ""), sptField,
                     FxDB(dr("sostatuspi"), 0), sptField,
                     FxDB(dr("sostatuspl"), 0), sptField,
                     FxDB(dr("sostatusdo"), 0), sptField,
                     FxDB(dr("sostatusdr"), 0), sptField,
                     FxDB(dr("sostatussi"), 0), sptField,
                     FxDB(dr("sostatusrnr"), 0), sptField,
                     FxDB(dr("sostatussr"), 0), sptField,
                     FxDB(dr("sostatusrealisasi"), 0), sptField,
                     FxDB(dr("sostatus"), 0), sptField,
                     FxDB(dr("sostatussebelumnya"), 0), sptField,
                     FxDB(dr("sojmlrevisi"), 0), sptField,
                     FxDB(dr("socetakanke"), 0), sptField,
                     FxDB(dr("soinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("somodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soisclose"), 0), sptField,
                     FxDB(dr("socustomtext1"), ""), sptField,
                     FxDB(dr("socustomtext2"), ""), sptField,
                     FxDB(dr("socustomtext3"), ""), sptField,
                     FxDB(dr("socustomtext4"), ""), sptField,
                     FxDB(dr("socustomtext5"), ""), sptField,
                     FxDB(dr("socustomint1"), 0), sptField,
                     FxDB(dr("socustomint2"), 0), sptField,
                     FxDB(dr("socustomint3"), 0), sptField,
                     FxDB(dr("socustomdbl1"), 0), sptField,
                     FxDB(dr("socustomdbl2"), 0), sptField,
                     FxDB(dr("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate3"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspi, sostatuspl, sostatusdo, sostatusdr, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM5_So_DetailSearch(ByVal param As String) As String
        'MobM5_So_DetailSearch --------------------------------------------------------
        'idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, 
        'jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, 
        'gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, 
        'idsqdetail, jmlpi, statuspi, jmlpl, statuspl, jmldo, statusdo, 
        'jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, 
        'statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sol = query.PanggilQuery("m5_so_detail_v")

        sol = "select `sod`.`idsodetail` AS `idsodetail`,`sod`.`idso` AS `idso`,`sod`.`idbarang` AS `idbarang`,`sod`.`namabarang` AS `namabarang`,`sod`.`tipebarang` AS `tipebarang`,`sod`.`jml` AS `jml`,`sod`.`satuan` AS `satuan`,`sod`.`nilaisatuan` AS `nilaisatuan`,`sod`.`jmlbarang` AS `jmlbarang`,`sod`.`satuanbarang` AS `satuanbarang`,`sod`.`matauang` AS `matauang`,`sod`.`kurs` AS `kurs`,`sod`.`harga` AS `harga`,`sod`.`diskon` AS `diskon`,`sod`.`jmldiskon` AS `jmldiskon`,`sod`.`pajak1` AS `pajak1`,`sod`.`jmlpajak1` AS `jmlpajak1`,`sod`.`pajak2` AS `pajak2`,`sod`.`jmlpajak2` AS `jmlpajak2`,`sod`.`cabang` AS `cabang`,`sod`.`lokasi` AS `lokasi`,`sod`.`gudang` AS `gudang`,`sod`.`costcenter` AS `costcenter`,`sod`.`divisi` AS `divisi`,`sod`.`subdivisi` AS `subdivisi`,`sod`.`proyek` AS `proyek`,`sod`.`catatan` AS `catatan`,`sod`.`urutan` AS `urutan`,`sod`.`idsqdetail` AS `idsqdetail`,`sod`.`jmlpi` AS `jmlpi`,`sod`.`statuspi` AS `statuspi`,`sod`.`jmlpl` AS `jmlpl`,`sod`.`statuspl` AS `statuspl`,`sod`.`jmldo` AS `jmldo`,`sod`.`statusdo` AS `statusdo`,`sod`.`jmldr` AS `jmldr`,`sod`.`statusdr` AS `statusdr`,`sod`.`jmlsi` AS `jmlsi`,`sod`.`statussi` AS `statussi`,`sod`.`jmlrnr` AS `jmlrnr`,`sod`.`statusrnr` AS `statusrnr`,`sod`.`jmlsr` AS `jmlsr`,`sod`.`statussr` AS `statussr`,`sod`.`jmlrealisasi` AS `jmlrealisasi`,`sod`.`statusrealisasi` AS `statusrealisasi`,`sod`.`isclose` AS `isclose`,`sod`.`customtext1` AS `customtext1`,`sod`.`customtext2` AS `customtext2`,`sod`.`customtext3` AS `customtext3`,`sod`.`customdbl1` AS `customdbl1`,`sod`.`customdbl2` AS `customdbl2`,`sod`.`customdbl3` AS `customdbl3`,`sod`.`customdate1` AS `customdate1`,`sod`.`customdate2` AS `customdate2`,`sod`.`customdate3` AS `customdate3` from (`m5_so` `so` join `m5_so_detail` `sod` on((`so`.`soid` = `sod`.`idso`)))"

        dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sol) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idsodetail"), ""), sptField,
                     FxDB(dr("idso"), ""), sptField,
                     FxDB(dr("idbarang"), ""), sptField,
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
                     FxDB(dr("idsqdetail"), ""), sptField,
                     FxDB(dr("jmlpi"), 0), sptField,
                     FxDB(dr("statuspi"), 0), sptField,
                     FxDB(dr("jmlpl"), 0), sptField,
                     FxDB(dr("statuspl"), 0), sptField,
                     FxDB(dr("jmldo"), 0), sptField,
                     FxDB(dr("statusdo"), 0), sptField,
                     FxDB(dr("jmldr"), 0), sptField,
                     FxDB(dr("statusdr"), 0), sptField,
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
                     AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpi, statuspi, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM5_So_AllSearch(ByVal param As String) As String
        'MobM5_SoSearch --------------------------------------------------------
        'soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, 
        'sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, 
        'socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, 
        'so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, 
        'socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, 
        'sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, 
        'sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, 
        'soidsq, sostatuspi, sostatuspl, sostatusdo, sostatusdr, sostatussi, sostatusrnr, 
        'sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, 
        'soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, 
        'socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, 
        'socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", searchDetail As String = ""

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
        'If Len(paramSplit(0)) = 0 Then
        '    result(2) = "WebsiteAccessKey can't be empty." : GoTo selesai
        'End If

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 3) = False Then
        '    result(2) = "Access denied for get data"
        'End If
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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m5_so_v")

        dt = AmbilData("aplikasi1-M5_So", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("soid"), ""), sptField,
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
                     FxDB(dr("sokodepa"), ""), sptField,
                     FxDB(dr("socustomer"), ""), sptField,
                     FxDB(dr("socustomerkontak"), ""), sptField,
                     FxDB(dr("so1alamat1"), ""), sptField,
                     FxDB(dr("so1alamat2"), ""), sptField,
                     FxDB(dr("so1alamat3"), ""), sptField,
                     FxDB(dr("so2alamat1"), ""), sptField,
                     FxDB(dr("so2alamat2"), ""), sptField,
                     FxDB(dr("so2alamat3"), ""), sptField,
                     FxDB(dr("sobagianpenjualan"), ""), sptField,
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
                     FxDB(dr("sobiayalainpersen"), ""), sptField,
                     FxDB(dr("sobiayalain"), 0), sptField,
                     FxDB(dr("sototaltransaksi"), 0), sptField,
                     FxDB(dr("sojmlbayar"), 0), sptField,
                     FxDB(dr("sorekdiskon"), ""), sptField,
                     FxDB(dr("sorekpajak1"), ""), sptField,
                     FxDB(dr("sorekpajak2"), ""), sptField,
                     FxDB(dr("sorekbiayalain"), ""), sptField,
                     FxDB(dr("sorekbayar"), ""), sptField,
                     FxDB(dr("soidsq"), ""), sptField,
                     FxDB(dr("sostatuspi"), 0), sptField,
                     FxDB(dr("sostatuspl"), 0), sptField,
                     FxDB(dr("sostatusdo"), 0), sptField,
                     FxDB(dr("sostatusdr"), 0), sptField,
                     FxDB(dr("sostatussi"), 0), sptField,
                     FxDB(dr("sostatusrnr"), 0), sptField,
                     FxDB(dr("sostatussr"), 0), sptField,
                     FxDB(dr("sostatusrealisasi"), 0), sptField,
                     FxDB(dr("sostatus"), 0), sptField,
                     FxDB(dr("sostatussebelumnya"), 0), sptField,
                     FxDB(dr("sojmlrevisi"), 0), sptField,
                     FxDB(dr("socetakanke"), 0), sptField,
                     FxDB(dr("soinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("soinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("somodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("somodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sopostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("soisclose"), 0), sptField,
                     FxDB(dr("socustomtext1"), ""), sptField,
                     FxDB(dr("socustomtext2"), ""), sptField,
                     FxDB(dr("socustomtext3"), ""), sptField,
                     FxDB(dr("socustomtext4"), ""), sptField,
                     FxDB(dr("socustomtext5"), ""), sptField,
                     FxDB(dr("socustomint1"), 0), sptField,
                     FxDB(dr("socustomint2"), 0), sptField,
                     FxDB(dr("socustomint3"), 0), sptField,
                     FxDB(dr("socustomdbl1"), 0), sptField,
                     FxDB(dr("socustomdbl2"), 0), sptField,
                     FxDB(dr("socustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("socustomdate3"), ""), formatTgl), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            sql = "select `sod`.`idsodetail` AS `idsodetail`,`sod`.`idso` AS `idso`,`sod`.`idbarang` AS `idbarang`,`sod`.`namabarang` AS `namabarang`,`sod`.`tipebarang` AS `tipebarang`,`sod`.`jml` AS `jml`,`sod`.`satuan` AS `satuan`,`sod`.`nilaisatuan` AS `nilaisatuan`,`sod`.`jmlbarang` AS `jmlbarang`,`sod`.`satuanbarang` AS `satuanbarang`,`sod`.`matauang` AS `matauang`,`sod`.`kurs` AS `kurs`,`sod`.`harga` AS `harga`,`sod`.`diskon` AS `diskon`,`sod`.`jmldiskon` AS `jmldiskon`,`sod`.`pajak1` AS `pajak1`,`sod`.`jmlpajak1` AS `jmlpajak1`,`sod`.`pajak2` AS `pajak2`,`sod`.`jmlpajak2` AS `jmlpajak2`,`sod`.`cabang` AS `cabang`,`sod`.`lokasi` AS `lokasi`,`sod`.`gudang` AS `gudang`,`sod`.`costcenter` AS `costcenter`,`sod`.`divisi` AS `divisi`,`sod`.`subdivisi` AS `subdivisi`,`sod`.`proyek` AS `proyek`,`sod`.`catatan` AS `catatan`,`sod`.`urutan` AS `urutan`,`sod`.`idsqdetail` AS `idsqdetail`,`sod`.`jmlpi` AS `jmlpi`,`sod`.`statuspi` AS `statuspi`,`sod`.`jmlpl` AS `jmlpl`,`sod`.`statuspl` AS `statuspl`,`sod`.`jmldo` AS `jmldo`,`sod`.`statusdo` AS `statusdo`,`sod`.`jmldr` AS `jmldr`,`sod`.`statusdr` AS `statusdr`,`sod`.`jmlsi` AS `jmlsi`,`sod`.`statussi` AS `statussi`,`sod`.`jmlrnr` AS `jmlrnr`,`sod`.`statusrnr` AS `statusrnr`,`sod`.`jmlsr` AS `jmlsr`,`sod`.`statussr` AS `statussr`,`sod`.`jmlrealisasi` AS `jmlrealisasi`,`sod`.`statusrealisasi` AS `statusrealisasi`,`sod`.`isclose` AS `isclose`,`sod`.`customtext1` AS `customtext1`,`sod`.`customtext2` AS `customtext2`,`sod`.`customtext3` AS `customtext3`,`sod`.`customdbl1` AS `customdbl1`,`sod`.`customdbl2` AS `customdbl2`,`sod`.`customdbl3` AS `customdbl3`,`sod`.`customdate1` AS `customdate1`,`sod`.`customdate2` AS `customdate2`,`sod`.`customdate3` AS `customdate3` from (`m5_so` `so` join `m5_so_detail` `sod` on((`so`.`soid` = `sod`.`idso`)))"

            dt = AmbilData("aplikasi1-M5_so_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            pg1 = pg1
            If dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    searchDetail = String.Concat(searchDetail,
                         FxDB(dr("idsodetail"), ""), sptField,
                         FxDB(dr("idso"), ""), sptField,
                         FxDB(dr("idbarang"), ""), sptField,
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
                         FxDB(dr("idsqdetail"), ""), sptField,
                         FxDB(dr("jmlpi"), 0), sptField,
                         FxDB(dr("statuspi"), 0), sptField,
                         FxDB(dr("jmlpl"), 0), sptField,
                         FxDB(dr("statuspl"), 0), sptField,
                         FxDB(dr("jmldo"), 0), sptField,
                         FxDB(dr("statusdo"), 0), sptField,
                         FxDB(dr("jmldr"), 0), sptField,
                         FxDB(dr("statusdr"), 0), sptField,
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
                         AsFormatTanggal(FxDB(dr("customdate3"), ""), formatTgl), sptRow)
                Next
                searchDetail = searchDetail.Substring(0, searchDetail.Length - sptRow.Length)
            End If

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, searchDetail)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("soid, socabang, solokasi, sogudang, soasalbarang, soasalbarangkategori, sojenispenjualan, sojenispenjualankategori, socarabayar, sosumber, soautonotransaksi, sonotransaksi, sotgl, sokodepa, socustomer, socustomerkontak, so1alamat1, so1alamat2, so1alamat3, so2alamat1, so2alamat2, so2alamat3, sobagianpenjualan, soekspedisi, sotglkirim, sotermin, sotgljatuhtempo, souraian, socatatan, sonoref, sotglnoref, sotglpenutupan, somatauang, sokurs, sohargatermasukpajak, sototal, sodiskonpersen, sojmldiskon, sototalpajak1detail, sototalpajak2detail, sobiayalainpersen, sobiayalain, sototaltransaksi, sojmlbayar, sorekdiskon, sorekpajak1, sorekpajak2, sorekbiayalain, sorekbayar, soidsq, sostatuspi, sostatuspl, sostatusdo, sostatusdr, sostatussi, sostatusrnr, sostatussr, sostatusrealisasi, sostatus, sostatussebelumnya, sojmlrevisi, socetakanke, soinputuser, soinputtgl, somodifikasiuser, somodifikasitgl, soposting, sopostingtgl, soisclose, socustomtext1, socustomtext2, socustomtext3, socustomtext4, socustomtext5, socustomint1, socustomint2, socustomint3, socustomdbl1, socustomdbl2, socustomdbl3, socustomdate1, socustomdate2, socustomdate3"), sptSubParam, ReplaceMapping("idsodetail, idso, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idsqdetail, jmlpi, statuspi, jmlpl, statuspl, jmldo, statusdo, jmldr, statusdr, jmlsi, statussi, jmlrnr, statusrnr, jmlsr, statussr, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3"))

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
End Class