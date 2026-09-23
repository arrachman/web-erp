Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization

'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m1_contact
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_ContactSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataDetail(), dataRowDetail(), dataPrice(), dataRowPrice(), dataCommission(), dataRowCommission() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim notransaksi As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim search As String = "", Filter As String = "", Sorting As String = ""
        Dim kodekontak As String = "", kategorikontak As String = ""

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

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

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

        ''CEK ARRAY DATA
        'If (dataSplit.Length <> 2) Then
        '    result(2) = "Invalid transaction data parameter." : GoTo selesai
        'End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'kid(0) As Integer, kkode(1) As String, knama(2) As String, kkategori(3) As String, kkategorinama(4) As String, 
        'kcabang(5) As String, kcabangnama(6) As String, klokasi(7) As String, klokasinama(8) As String, kgudang(9) As String, 
        'kgudangnama(10) As String, kkategorisalesman(11) As String, kkategorisalesmannama(12) As String, karea(13) As String, kareanama(14) As String, 
        'kkategoricustomer(15) As String, kkategoricustomernama(16) As String, kdivisi(17) As String, kdivisinama(18) As String, ksubdivisi(19) As String, 
        'ksubdivisinama(20) As String, ksalesman(21) As Integer, ksalesmannama(22) As String, kkontakperson(23) As String, kterminglobal(24) As Integer, 
        'kaktif(25) As Integer, kaktiftgl(26) As Date, k1alamat1(27) As String, k1alamat2(28) As String, k1alamat3(29) As String, 
        'k1alamat4(30) As String, k1alamat5(31) As String, k1kota(32) As String, k1propinsi(33) As String, k1kodepos(34) As String, 
        'k1negara(35) As String, k1kontakperson(36) As String, k1kontaknohp(37) As String, k1kontakemail(38) As String, k1notelp1(39) As String, 
        'k1notelp2(40) As String, k1nofax(41) As String, k1email(42) As String, k1website(43) As String, k2alamat1(44) As String, 
        'k2alamat2(45) As String, k2alamat3(46) As String, k2alamat4(47) As String, k2alamat5(48) As String, k2propinsi(49) As String, 
        'k2kota(50) As String, k2kodepos(51) As String, k2negara(52) As String, k2kontakperson(53) As String, k2kontaknohp(54) As String, 
        'k2kontakemail(55) As String, k2notelp1(56) As String, k2notelp2(57) As String, k2nofax(58) As String, k2email(59) As String, 
        'k2website(60) As String, k3alamat1(61) As String, k3alamat2(62) As String, k3alamat3(63) As String, k3alamat4(64) As String, 
        'k3alamat5(65) As String, k3kota(66) As String, k3propinsi(67) As String, k3kodepos(68) As String, k3negara(69) As String, 
        'k3kontakperson(70) As String, k3kontaknohp(71) As String, k3kontakemail(72) As String, k3notelp1(73) As String, k3notelp2(74) As String, 
        'k3nofax(75) As String, k3email(76) As String, k3website(77) As String, k4alamat1(78) As String, k4alamat2(79) As String, 
        'k4alamat3(80) As String, k4alamat4(81) As String, k4alamat5(82) As String, k4kota(83) As String, k4propinsi(84) As String, 
        'k4kodepos(85) As String, k4negara(86) As String, k4kontakperson(87) As String, k4kontaknohp(88) As String, k4kontakemail(89) As String, 
        'k4notelp1(90) As String, k4notelp2(91) As String, k4nofax(92) As String, k4email(93) As String, k4website(94) As String, 
        'knpwp(95) As String, kpkp(96) As Integer, kbatashutang(97) As Double, kterminbeli(98) As String, krekhutang(99) As String, 
        'kbagpembelian(100) As Integer, kfobbeli(101) As String, kviabeli(102) As String, kbataspiutang(103) As Double, kterminjual(104) As String, 
        'krekpiutang(105) As String, kbagpenjualan(106) As Integer, ktingkatjual(107) As Integer, kfobjual(108) As String, kviajual(109) As String, 
        'ktglkontrak(110) As Date, kbank(111) As String, knorekening(112) As String, kjeniskelamin(113) As Integer, kmatauang(114) As String, 
        'ktgllahir(115) As Date, ktglnikah(116) As Date, kkomisipenjualan(117) As Double, kcatatan(118) As String, kinputuser(119) As Integer, 
        'kinputtgl(120) As DateTime, kcustomtext1(121) As String, kcustomtext2(122) As String, kcustomtext3(123) As String, kcustomtext4(124) As String, 
        'kcustomtext5(125) As String, kcustomtext6(126) As String, kcustomtext7(127) As String, kcustomtext8(128) As String, kcustomtext9(129) As String, 
        'kmodifikasiuser(130) As Integer, kmodifikasitgl(131) As DateTime, kcustomtext10(132) As String, kcustomint1(133) As Integer, kcustomint2(134) As Integer, 
        'kcustomint3(135) As Integer, kcustomdbl1(136) As Double, kcustomdbl2(137) As Double, kcustomdbl3(138) As Double, kcustomdate1(139) As Date, 
        'kcustomdate2(140) As Date, kcustomdate3(141) As Date, kkategorisupplier(142) As String, kkategorisuppliernama(143) As String, kkomisikode(144) As String,
        'khargacustom(145) As Integer

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, 
        'ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, 
        'k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, 
        'k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, 
        'k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, 
        'k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, 
        'k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, 
        'k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, 
        'k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, 
        'k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, 
        'kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, 
        'krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, 
        'knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, 
        'kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3, kkategorisupplier, kkategorisuppliernama, kkomisikode, khargacustom

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 146) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'kid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kid required numeric." : GoTo selesai
        End If
        'ksalesman(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "ksalesman required numeric." : GoTo selesai
        End If
        'kterminglobal(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "kterminglobal required numeric." : GoTo selesai
        End If
        'kaktif(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "kaktif required numeric." : GoTo selesai
        End If
        'kaktiftgl(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "kaktiftgl required date." : GoTo selesai
        End If
        'kpkp(96) As Integer
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kpkp required numeric." : GoTo selesai
        End If
        'kbatashutang(97) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kbatashutang required numeric." : GoTo selesai
        End If
        'kbagpembelian(100) As Integer
        If (IsNumeric(dataUtama(100)) = False) Then
            result(2) = "kbagpembelian required numeric." : GoTo selesai
        End If
        'kbataspiutang(103) As Double
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "kbataspiutang required numeric." : GoTo selesai
        End If
        'kbagpenjualan(106) As Integer
        If (IsNumeric(dataUtama(106)) = False) Then
            result(2) = "kbagpenjualan required numeric." : GoTo selesai
        End If
        'ktingkatjual(107) As Integer
        If (IsNumeric(dataUtama(107)) = False) Then
            result(2) = "ktingkatjual required numeric." : GoTo selesai
        End If
        'ktglkontrak(110) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "ktglkontrak required date." : GoTo selesai
        End If
        'kjeniskelamin(113) As Integer
        If (IsNumeric(dataUtama(113)) = False) Then
            result(2) = "kjeniskelamin required numeric." : GoTo selesai
        End If
        'ktgllahir(115) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "ktgllahir required date." : GoTo selesai
        End If
        'ktglnikah(116) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "ktglnikah required date." : GoTo selesai
        End If
        'kkomisipenjualan(117) As Double
        If (IsNumeric(dataUtama(117)) = False) Then
            result(2) = "kkomisipenjualan required numeric." : GoTo selesai
        End If
        'kinputuser(119) As Integer
        If (IsNumeric(dataUtama(119)) = False) Then
            result(2) = "kinputuser required numeric." : GoTo selesai
        End If
        'kinputtgl(120) As DateTime
        If (IsDate(dataUtama(120)) = False) Then
            result(2) = "kinputtgl required date." : GoTo selesai
        End If
        'kmodifikasiuser(130) As Integer
        If (IsNumeric(dataUtama(130)) = False) Then
            result(2) = "kmodifikasiuser required numeric." : GoTo selesai
        End If
        'kmodifikasitgl(131) As DateTime
        If (IsDate(dataUtama(131)) = False) Then
            result(2) = "kmodifikasitgl required date." : GoTo selesai
        End If
        'kcustomint1(133) As Integer
        If (IsNumeric(dataUtama(133)) = False) Then
            result(2) = "kcustomint1 required numeric." : GoTo selesai
        End If
        'kcustomint2(134) As Integer
        If (IsNumeric(dataUtama(134)) = False) Then
            result(2) = "kcustomint2 required numeric." : GoTo selesai
        End If
        'kcustomint3(135) As Integer
        If (IsNumeric(dataUtama(135)) = False) Then
            result(2) = "kcustomint3 required numeric." : GoTo selesai
        End If
        'kcustomdbl1(136) As Double
        If (IsNumeric(dataUtama(136)) = False) Then
            result(2) = "kcustomdbl1 required numeric." : GoTo selesai
        End If
        'kcustomdbl2(137) As Double
        If (IsNumeric(dataUtama(137)) = False) Then
            result(2) = "kcustomdbl2 required numeric." : GoTo selesai
        End If
        'kcustomdbl3(138) As Double
        If (IsNumeric(dataUtama(138)) = False) Then
            result(2) = "kcustomdbl3 required numeric." : GoTo selesai
        End If
        'kcustomdate1(139) As Date
        If (IsDate(dataUtama(139)) = False) Then
            result(2) = "kcustomdate1 required date." : GoTo selesai
        End If
        'kcustomdate2(140) As Date
        If (IsDate(dataUtama(140)) = False) Then
            result(2) = "kcustomdate2 required date." : GoTo selesai
        End If
        'kcustomdate3(141) As Date
        If (IsDate(dataUtama(141)) = False) Then
            result(2) = "kcustomdate3 required date." : GoTo selesai
        End If
        'khargacustom(145) As Integer
        If (IsNumeric(dataUtama(145)) = False) Then
            result(2) = "khargacustom required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'kkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "kkode should not be more than 25 character." : GoTo selesai
        End If
        kodekontak = dataUtama(1)

        'knama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "knama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "knama should not be more than 100 character." : GoTo selesai
        End If

        'kkategori(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kkategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 3 Then
            result(2) = "kkategori should not be more than 3 character." : GoTo selesai
        End If
        kategorikontak = dataUtama(3)

        'kmatauang(114) As String
        If Len(dataUtama(114)) = 0 Then
            result(2) = "Currency can't be empty" : GoTo selesai
        End If

        'krekhutang(99) As String
        If Len(dataUtama(99)) = 0 Then
            result(2) = "AP Account can't be empty" : GoTo selesai
        End If

        'krekpiutang(105) As String
        If Len(dataUtama(105)) = 0 Then
            result(2) = "AR Account can't be empty" : GoTo selesai
        End If

        'kaktiftgl(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "kaktiftgl can't be empty" : GoTo selesai
        End If

        'kinputtgl(120) As DateTime
        If Len(dataUtama(120)) = 0 Then
            result(2) = "kinputtgl can't be empty" : GoTo selesai
        End If

        'kmodifikasitgl(131) As DateTime
        If Len(dataUtama(131)) = 0 Then
            result(2) = "kmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kcustomdbl1(136) As Double
        If Len(dataUtama(136)) = 0 Then
            result(2) = "kcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kcustomdbl2(137) As Double
        If Len(dataUtama(137)) = 0 Then
            result(2) = "kcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kcustomdbl3(138) As Double
        If Len(dataUtama(138)) = 0 Then
            result(2) = "kcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kcustomdate1(139) As Date
        If Len(dataUtama(139)) = 0 Then
            result(2) = "kcustomdate1 can't be empty" : GoTo selesai
        End If

        'kcustomdate2(140) As Date
        If Len(dataUtama(140)) = 0 Then
            result(2) = "kcustomdate2 can't be empty" : GoTo selesai
        End If

        'kcustomdate3(141) As Date
        If Len(dataUtama(141)) = 0 Then
            result(2) = "kcustomdate3 can't be empty" : GoTo selesai
        End If

        'kkomisikode(144) As String
        If Len(dataUtama(144)) > 25 Then
            result(2) = "kkomisikode should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesman", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "karea", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kareanama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksalesman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ksalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminglobal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktiftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knpwp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kpkp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kbatashutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekhutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbataspiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ktingkatjual", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglkontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knorekening", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjeniskelamin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglnikah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkomisipenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisupplier", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisuppliernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkomisikode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "khargacustom", AsEnumTypeData.AsInt64)

        If AsDataTableTambahData(dtutama, "kid~kkode~knama~kkategori~kkategorinama~kcabang~kcabangnama~klokasi~klokasinama~kgudang~kgudangnama~kkategorisalesman~kkategorisalesmannama~karea~kareanama~kkategoricustomer~kkategoricustomernama~kdivisi~kdivisinama~ksubdivisi~ksubdivisinama~ksalesman~ksalesmannama~kkontakperson~kterminglobal~kaktif~kaktiftgl~k1alamat1~k1alamat2~k1alamat3~k1alamat4~k1alamat5~k1kota~k1propinsi~k1kodepos~k1negara~k1kontakperson~k1kontaknohp~k1kontakemail~k1notelp1~k1notelp2~k1nofax~k1email~k1website~k2alamat1~k2alamat2~k2alamat3~k2alamat4~k2alamat5~k2propinsi~k2kota~k2kodepos~k2negara~k2kontakperson~k2kontaknohp~k2kontakemail~k2notelp1~k2notelp2~k2nofax~k2email~k2website~k3alamat1~k3alamat2~k3alamat3~k3alamat4~k3alamat5~k3kota~k3propinsi~k3kodepos~k3negara~k3kontakperson~k3kontaknohp~k3kontakemail~k3notelp1~k3notelp2~k3nofax~k3email~k3website~k4alamat1~k4alamat2~k4alamat3~k4alamat4~k4alamat5~k4kota~k4propinsi~k4kodepos~k4negara~k4kontakperson~k4kontaknohp~k4kontakemail~k4notelp1~k4notelp2~k4nofax~k4email~k4website~knpwp~kpkp~kbatashutang~kterminbeli~krekhutang~kbagpembelian~kfobbeli~kviabeli~kbataspiutang~kterminjual~krekpiutang~kbagpenjualan~ktingkatjual~kfobjual~kviajual~ktglkontrak~kbank~knorekening~kjeniskelamin~kmatauang~ktgllahir~ktglnikah~kkomisipenjualan~kcatatan~kinputuser~kinputtgl~kcustomtext1~kcustomtext2~kcustomtext3~kcustomtext4~kcustomtext5~kcustomtext6~kcustomtext7~kcustomtext8~kcustomtext9~kmodifikasiuser~kmodifikasitgl~kcustomtext10~kcustomint1~kcustomint2~kcustomint3~kcustomdbl1~kcustomdbl2~kcustomdbl3~kcustomdate1~kcustomdate2~kcustomdate3~kkategorisupplier~kkategorisuppliernama~kkomisikode~khargacustom", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122) & "~" & dataUtama(123) & "~" & dataUtama(124) & "~" & dataUtama(125) & "~" & dataUtama(126) & "~" & dataUtama(127) & "~" & dataUtama(128) & "~" & dataUtama(129) & "~" & dataUtama(130) & "~" & dataUtama(131) & "~" & dataUtama(132) & "~" & dataUtama(133) & "~" & dataUtama(134) & "~" & dataUtama(135) & "~" & dataUtama(136) & "~" & dataUtama(137) & "~" & dataUtama(138) & "~" & dataUtama(139) & "~" & dataUtama(140) & "~" & dataUtama(141) & "~" & dataUtama(142) & "~" & dataUtama(143) & "~" & dataUtama(144) & "~" & dataUtama(145)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'kaid(0) As Integer, kaidkontak(1) As Integer, kakodekontak(2) As String, kanama(3) As String, kajabatan(4) As String, 
        'kanotelp(5) As String, kanofax(6) As String, kanohp(7) As String, kaemail(8) As String, kawebsite(9) As String, 
        'kamessenger(10) As String, kaalamat(11) As String, katgllahir(12) As Date, katglnikah(13) As Date, kacatatan(14) As String, 
        'kadefault(15) As Integer, kainputuser(16) As Integer, kainputtgl(17) As DateTime, kamodifikasiuser(18) As Integer, kamodifikasitgl(19) As DateTime

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, 
        'kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, 
        'kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "kaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaidkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kakodekontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kajabatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanotelp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kawebsite", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kamessenger", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "katgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "katglnikah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kadefault", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kamodifikasitgl", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        If Len(dataSplit(1)) > 0 Then
            Dim JmlDtDetail As Integer = dataDetail.Length
            For i = 1 To JmlDtDetail
                'SPLIT DATA DETAIL
                dataRowDetail = dataDetail(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowDetail.Length <> 20) Then
                    result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------
                'kaid(0) As Integer
                If (IsNumeric(dataRowDetail(0)) = False) Then
                    result(2) = "Row : " & i & " - kaid required numeric." : GoTo selesai
                End If
                'kaidkontak(1) As Integer
                If (IsNumeric(dataRowDetail(1)) = False) Then
                    result(2) = "Row : " & i & " - kaidkontak required numeric." : GoTo selesai
                End If
                'katgllahir(12) As Date
                If (IsDate(dataRowDetail(12)) = False) Then
                    result(2) = "Row : " & i & " - katgllahir required date." : GoTo selesai
                End If
                'katglnikah(13) As Date
                If (IsDate(dataRowDetail(13)) = False) Then
                    result(2) = "Row : " & i & " - katglnikah required date." : GoTo selesai
                End If
                'kadefault(15) As Integer
                If (IsNumeric(dataRowDetail(15)) = False) Then
                    result(2) = "Row : " & i & " - kadefault required numeric." : GoTo selesai
                End If
                'kainputuser(16) As Integer
                If (IsNumeric(dataRowDetail(16)) = False) Then
                    result(2) = "Row : " & i & " - kainputuser required numeric." : GoTo selesai
                End If
                'kainputtgl(17) As DateTime
                If (IsDate(dataRowDetail(17)) = False) Then
                    result(2) = "Row : " & i & " - kainputtgl required date." : GoTo selesai
                End If
                'kamodifikasiuser(18) As Integer
                If (IsNumeric(dataRowDetail(18)) = False) Then
                    result(2) = "Row : " & i & " - kamodifikasiuser required numeric." : GoTo selesai
                End If
                'kamodifikasitgl(19) As DateTime
                If (IsDate(dataRowDetail(19)) = False) Then
                    result(2) = "Row : " & i & " - kamodifikasitgl required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'kakodekontak(2) As String
                If Len(dataRowDetail(2)) = 0 Then
                    result(2) = "Row : " & i & " - kakodekontak can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(2)) > 25 Then
                    result(2) = "Row : " & i & " - kakodekontak should not be more than 25 character." : GoTo selesai
                End If

                'kanama(3) As String
                If Len(dataRowDetail(3)) = 0 Then
                    result(2) = "Row : " & i & " - kanama can't be empty" : GoTo selesai
                End If
                If Len(dataRowDetail(3)) > 100 Then
                    result(2) = "Row : " & i & " - kanama should not be more than 100 character." : GoTo selesai
                End If

                'kainputtgl(17) As DateTime
                If Len(dataRowDetail(17)) = 0 Then
                    result(2) = "Row : " & i & " - kainputtgl can't be empty" : GoTo selesai
                End If

                'kamodifikasitgl(19) As DateTime
                If Len(dataRowDetail(19)) = 0 Then
                    result(2) = "Row : " & i & " - kamodifikasitgl can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtdetail, "kaid~kaidkontak~kakodekontak~kanama~kajabatan~kanotelp~kanofax~kanohp~kaemail~kawebsite~kamessenger~kaalamat~katgllahir~katglnikah~kacatatan~kadefault~kainputuser~kainputtgl~kamodifikasiuser~kamodifikasitgl", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
                    result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
        End If
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'HARGA PERKONTAK
        'MAPPING BUAT WS DATA PRICE -------------------------------------------------------
        'khidkontak(0) As Integer, khidbarang(1) As Integer, khsatuan(2) As String, khkomisi(3) As Double, khhargabeli(4) As Double, 
        'khhargajual(5) As Double, khberlakudari(6) As Date, khberlakusampai(7) As Date, khcatatan(8) As String, khinputuser(9) As Integer, 
        'khinputtgl(10) As DateTime, khmodifikasiuser(11) As Integer, khmodifikasitgl(12) As DateTime, khcustomtext1(13) As String, khcustomtext2(14) As String, 
        'khcustomtext3(15) As String, khcustomtext4(16) As String, khcustomtext5(17) As String, khcustomint1(18) As Integer, khcustomint2(19) As Integer, 
        'khcustomint3(20) As Integer, khcustomint4(21) As Integer, khcustomint5(22) As Integer, khcustomdbl1(23) As Double, khcustomdbl2(24) As Double, 
        'khcustomdbl3(25) As Double, khcustomdbl4(26) As Double, khcustomdbl5(27) As Double, khcustomdate1(28) As Date, khcustomdate2(29) As Date, 
        'khcustomdate3(30) As Date, khcustomdate4(31) As Date, khcustomdate5(32) As Date

        'MAPPING BUAT FLEX DATA PRICE -----------------------------------------------------
        'khidkontak, khidbarang, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, 
        'khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, 
        'khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, 
        'khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, 
        'khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5
        dataPrice = dataSplit(2).Split(sptRow)

        'Buat datatable PRICE
        Dim dtPrice As New DataTable
        AsDataTableTambahField(dtPrice, "khidkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khkomisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khhargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khhargajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khberlakudari", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khberlakusampai", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khcustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khcustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtPrice, "khcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtPrice, "khcustomdate5", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA PRICE ======================================================
        'SPLIT PARAMETER DATA PRICE
        If Len(dataSplit(2)) > 0 Then

            Dim JmlDtPrice As Integer = dataPrice.Length
            For i = 1 To JmlDtPrice
                'SPLIT DATA PRICE
                dataRowPrice = dataPrice(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Price -----------------------------------
                'CEK ARRAY DATA Price
                If (dataRowPrice.Length <> 33) Then
                    result(2) = "Row : " & i & " - Invalid Price transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Price ----------------------------

                'VALIDASI TIPE DATA Price ------------------------------------------
                'khidkontak(0) As Integer
                If (IsNumeric(dataRowPrice(0)) = False) Then
                    result(2) = "khidkontak required numeric." : GoTo selesai
                End If
                'khidbarang(1) As Integer
                If (IsNumeric(dataRowPrice(1)) = False) Then
                    result(2) = "khidbarang required numeric." : GoTo selesai
                End If
                'khkomisi(3) As Double
                If (IsNumeric(dataRowPrice(3)) = False) Then
                    result(2) = "khkomisi required numeric." : GoTo selesai
                End If
                'khhargabeli(4) As Double
                If (IsNumeric(dataRowPrice(4)) = False) Then
                    result(2) = "khhargabeli required numeric." : GoTo selesai
                End If
                'khhargajual(5) As Double
                If (IsNumeric(dataRowPrice(5)) = False) Then
                    result(2) = "khhargajual required numeric." : GoTo selesai
                End If
                'khberlakudari(6) As Date
                If (IsDate(dataRowPrice(6)) = False) Then
                    result(2) = "khberlakudari required date." : GoTo selesai
                End If
                'khberlakusampai(7) As Date
                If (IsDate(dataRowPrice(7)) = False) Then
                    result(2) = "khberlakusampai required date." : GoTo selesai
                End If
                'khinputuser(9) As Integer
                If (IsNumeric(dataRowPrice(9)) = False) Then
                    result(2) = "khinputuser required numeric." : GoTo selesai
                End If
                'khinputtgl(10) As DateTime
                If (IsDate(dataRowPrice(10)) = False) Then
                    result(2) = "khinputtgl required date." : GoTo selesai
                End If
                'khmodifikasiuser(11) As Integer
                If (IsNumeric(dataRowPrice(11)) = False) Then
                    result(2) = "khmodifikasiuser required numeric." : GoTo selesai
                End If
                'khmodifikasitgl(12) As DateTime
                If (IsDate(dataRowPrice(12)) = False) Then
                    result(2) = "khmodifikasitgl required date." : GoTo selesai
                End If
                'khcustomint1(18) As Integer
                If (IsNumeric(dataRowPrice(18)) = False) Then
                    result(2) = "khcustomint1 required numeric." : GoTo selesai
                End If
                'khcustomint2(19) As Integer
                If (IsNumeric(dataRowPrice(19)) = False) Then
                    result(2) = "khcustomint2 required numeric." : GoTo selesai
                End If
                'khcustomint3(20) As Integer
                If (IsNumeric(dataRowPrice(20)) = False) Then
                    result(2) = "khcustomint3 required numeric." : GoTo selesai
                End If
                'khcustomint4(21) As Integer
                If (IsNumeric(dataRowPrice(21)) = False) Then
                    result(2) = "khcustomint4 required numeric." : GoTo selesai
                End If
                'khcustomint5(22) As Integer
                If (IsNumeric(dataRowPrice(22)) = False) Then
                    result(2) = "khcustomint5 required numeric." : GoTo selesai
                End If
                'khcustomdbl1(23) As Double
                If (IsNumeric(dataRowPrice(23)) = False) Then
                    result(2) = "khcustomdbl1 required numeric." : GoTo selesai
                End If
                'khcustomdbl2(24) As Double
                If (IsNumeric(dataRowPrice(24)) = False) Then
                    result(2) = "khcustomdbl2 required numeric." : GoTo selesai
                End If
                'khcustomdbl3(25) As Double
                If (IsNumeric(dataRowPrice(25)) = False) Then
                    result(2) = "khcustomdbl3 required numeric." : GoTo selesai
                End If
                'khcustomdbl4(26) As Double
                If (IsNumeric(dataRowPrice(26)) = False) Then
                    result(2) = "khcustomdbl4 required numeric." : GoTo selesai
                End If
                'khcustomdbl5(27) As Double
                If (IsNumeric(dataRowPrice(27)) = False) Then
                    result(2) = "khcustomdbl5 required numeric." : GoTo selesai
                End If
                'khcustomdate1(28) As Date
                If (IsDate(dataRowPrice(28)) = False) Then
                    result(2) = "khcustomdate1 required date." : GoTo selesai
                End If
                'khcustomdate2(29) As Date
                If (IsDate(dataRowPrice(29)) = False) Then
                    result(2) = "khcustomdate2 required date." : GoTo selesai
                End If
                'khcustomdate3(30) As Date
                If (IsDate(dataRowPrice(30)) = False) Then
                    result(2) = "khcustomdate3 required date." : GoTo selesai
                End If
                'khcustomdate4(31) As Date
                If (IsDate(dataRowPrice(31)) = False) Then
                    result(2) = "khcustomdate4 required date." : GoTo selesai
                End If
                'khcustomdate5(32) As Date
                If (IsDate(dataRowPrice(32)) = False) Then
                    result(2) = "khcustomdate5 required date." : GoTo selesai
                End If

                'khsatuan(2) As String
                If Len(dataRowPrice(2)) = 0 Then
                    result(2) = "Row : " & i & " - khsatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowPrice(2)) > 50 Then
                    result(2) = "Row : " & i & " - khsatuan should not be more than 50 character." : GoTo selesai
                End If

                'END OF VALIDASI TIPE DATA Price -----------------------------------

                AsDataTableTambahData(dtPrice, "khidkontak~khidbarang~khsatuan~khkomisi~khhargabeli~khhargajual~khberlakudari~khberlakusampai~khcatatan~khinputuser~khinputtgl~khmodifikasiuser~khmodifikasitgl~khcustomtext1~khcustomtext2~khcustomtext3~khcustomtext4~khcustomtext5~khcustomint1~khcustomint2~khcustomint3~khcustomint4~khcustomint5~khcustomdbl1~khcustomdbl2~khcustomdbl3~khcustomdbl4~khcustomdbl5~khcustomdate1~khcustomdate2~khcustomdate3~khcustomdate4~khcustomdate5", dataRowPrice(0) & "~" & dataRowPrice(1) & "~" & dataRowPrice(2) & "~" & dataRowPrice(3) & "~" & dataRowPrice(4) & "~" & dataRowPrice(5) & "~" & dataRowPrice(6) & "~" & dataRowPrice(7) & "~" & dataRowPrice(8) & "~" & dataRowPrice(9) & "~" & dataRowPrice(10) & "~" & dataRowPrice(11) & "~" & dataRowPrice(12) & "~" & dataRowPrice(13) & "~" & dataRowPrice(14) & "~" & dataRowPrice(15) & "~" & dataRowPrice(16) & "~" & dataRowPrice(17) & "~" & dataRowPrice(18) & "~" & dataRowPrice(19) & "~" & dataRowPrice(20) & "~" & dataRowPrice(21) & "~" & dataRowPrice(22) & "~" & dataRowPrice(23) & "~" & dataRowPrice(24) & "~" & dataRowPrice(25) & "~" & dataRowPrice(26) & "~" & dataRowPrice(27) & "~" & dataRowPrice(28) & "~" & dataRowPrice(29) & "~" & dataRowPrice(30) & "~" & dataRowPrice(31) & "~" & dataRowPrice(32))

            Next

        End If
        'END OF VALIDASI DAN SET DATA PRICE ===============================================

        'HARGA PERKONTAK
        'MAPPING BUAT WS DATA PRICE -------------------------------------------------------
        'khidkontak(0) As Integer, khidbarang(1) As Integer, khsatuan(2) As String, khkomisi(3) As Double, khhargabeli(4) As Double, 
        'khhargajual(5) As Double, khberlakudari(6) As Date, khberlakusampai(7) As Date, khcatatan(8) As String, khinputuser(9) As Integer, 
        'khinputtgl(10) As DateTime, khmodifikasiuser(11) As Integer, khmodifikasitgl(12) As DateTime, khcustomtext1(13) As String, khcustomtext2(14) As String, 
        'khcustomtext3(15) As String, khcustomtext4(16) As String, khcustomtext5(17) As String, khcustomint1(18) As Integer, khcustomint2(19) As Integer, 
        'khcustomint3(20) As Integer, khcustomint4(21) As Integer, khcustomint5(22) As Integer, khcustomdbl1(23) As Double, khcustomdbl2(24) As Double, 
        'khcustomdbl3(25) As Double, khcustomdbl4(26) As Double, khcustomdbl5(27) As Double, khcustomdate1(28) As Date, khcustomdate2(29) As Date, 
        'khcustomdate3(30) As Date, khcustomdate4(31) As Date, khcustomdate5(32) As Date

        'MAPPING BUAT FLEX DATA PRICE -----------------------------------------------------
        'khidkontak, khidbarang, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, 
        'khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, 
        'khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, 
        'khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, 
        'khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5
        dataCommission = dataSplit(3).Split(sptRow)

        'Buat datatable PRICE
        Dim dtCommission As New DataTable
        AsDataTableTambahField(dtCommission, "scidkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sckomisi1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sckomisi10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint4", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint5", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint6", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint7", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint8", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint9", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomint10", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtCommission, "sccustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdbl10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtCommission, "sccustomdate10", AsEnumTypeData.AsString)

        'VALIDASI DAN SET DATA PRICE ======================================================
        'SPLIT PARAMETER DATA PRICE
        If Len(dataSplit(3)) > 0 Then

            Dim JmlDtCommission As Integer = dataCommission.Length
            For i = 1 To JmlDtCommission
                'SPLIT DATA PRICE
                dataRowCommission = dataCommission(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Price -----------------------------------
                'CEK ARRAY DATA Price
                If (dataRowCommission.Length <> 51) Then
                    result(2) = "Row : " & i & " - Invalid commission transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Price ----------------------------

                'VALIDASI TIPE DATA Price ------------------------------------------
                'khidkontak(0) As Integer
                If (IsNumeric(dataRowCommission(0)) = False) Then
                    result(2) = "scidkontak required numeric." : GoTo selesai
                End If
                'sckomisi1(1) As Double
                If (IsNumeric(dataRowCommission(1)) = False) Then
                    result(2) = "sckomisi1 required numeric." : GoTo selesai
                End If
                'sckomisi2(2) As Double
                If (IsNumeric(dataRowCommission(2)) = False) Then
                    result(2) = "sckomisi2 required numeric." : GoTo selesai
                End If
                'sckomisi3(3) As Double
                If (IsNumeric(dataRowCommission(3)) = False) Then
                    result(2) = "sckomisi3 required numeric." : GoTo selesai
                End If
                'sckomisi4(4) As Double
                If (IsNumeric(dataRowCommission(4)) = False) Then
                    result(2) = "sckomisi4 required numeric." : GoTo selesai
                End If
                'sckomisi5(5) As Double
                If (IsNumeric(dataRowCommission(5)) = False) Then
                    result(2) = "sckomisi1 required numeric." : GoTo selesai
                End If
                'sckomisi6(6) As Double
                If (IsNumeric(dataRowCommission(6)) = False) Then
                    result(2) = "sckomisi6 required numeric." : GoTo selesai
                End If
                'sckomisi7(7) As Double
                If (IsNumeric(dataRowCommission(7)) = False) Then
                    result(2) = "sckomisi7 required numeric." : GoTo selesai
                End If
                'sckomisi8(8) As Double
                If (IsNumeric(dataRowCommission(8)) = False) Then
                    result(2) = "sckomisi8 required numeric." : GoTo selesai
                End If
                'sckomisi9(9) As Double
                If (IsNumeric(dataRowCommission(9)) = False) Then
                    result(2) = "sckomisi9 required numeric." : GoTo selesai
                End If
                'sckomisi10(10) As Double
                If (IsNumeric(dataRowCommission(10)) = False) Then
                    result(2) = "sckomisi10 required numeric." : GoTo selesai
                End If

                'sccustomint1(21) As Integer
                If (IsNumeric(dataRowCommission(21)) = False) Then
                    result(2) = "sccustomint1 required numeric." : GoTo selesai
                End If
                'sccustomint2(22) As Integer
                If (IsNumeric(dataRowCommission(22)) = False) Then
                    result(2) = "sccustomint2 required numeric." : GoTo selesai
                End If
                'sccustomint3(23) As Integer
                If (IsNumeric(dataRowCommission(23)) = False) Then
                    result(2) = "sccustomint3 required numeric." : GoTo selesai
                End If
                'sccustomint4(24) As Integer
                If (IsNumeric(dataRowCommission(24)) = False) Then
                    result(2) = "sccustomint4 required numeric." : GoTo selesai
                End If
                'sccustomint5(25) As Integer
                If (IsNumeric(dataRowCommission(25)) = False) Then
                    result(2) = "sccustomint5 required numeric." : GoTo selesai
                End If
                'sccustomint6(26) As Integer
                If (IsNumeric(dataRowCommission(26)) = False) Then
                    result(2) = "sccustomint6 required numeric." : GoTo selesai
                End If
                'sccustomint7(27) As Integer
                If (IsNumeric(dataRowCommission(27)) = False) Then
                    result(2) = "sccustomint7 required numeric." : GoTo selesai
                End If
                'sccustomint8(28) As Integer
                If (IsNumeric(dataRowCommission(28)) = False) Then
                    result(2) = "sccustomint8 required numeric." : GoTo selesai
                End If
                'sccustomint9(29) As Integer
                If (IsNumeric(dataRowCommission(29)) = False) Then
                    result(2) = "sccustomint9 required numeric." : GoTo selesai
                End If
                'sccustomint10(30) As Integer
                If (IsNumeric(dataRowCommission(30)) = False) Then
                    result(2) = "sccustomint10 required numeric." : GoTo selesai
                End If

                'sccustomdbl1(31) As Double
                If (IsNumeric(dataRowCommission(31)) = False) Then
                    result(2) = "sccustomdbl1 required numeric." : GoTo selesai
                End If
                'sccustomdbl2(32) As Double
                If (IsNumeric(dataRowCommission(32)) = False) Then
                    result(2) = "sccustomdbl2 required numeric." : GoTo selesai
                End If
                'sccustomdbl3(33) As Double
                If (IsNumeric(dataRowCommission(33)) = False) Then
                    result(2) = "sccustomdbl3 required numeric." : GoTo selesai
                End If
                'sccustomdbl4(34) As Double
                If (IsNumeric(dataRowCommission(34)) = False) Then
                    result(2) = "sccustomdbl4 required numeric." : GoTo selesai
                End If
                'sccustomdbl5(35) As Double
                If (IsNumeric(dataRowCommission(35)) = False) Then
                    result(2) = "sccustomdbl5 required numeric." : GoTo selesai
                End If
                'sccustomdbl6(36) As Double
                If (IsNumeric(dataRowCommission(36)) = False) Then
                    result(2) = "sccustomdbl6 required numeric." : GoTo selesai
                End If
                'sccustomdbl7(37) As Double
                If (IsNumeric(dataRowCommission(37)) = False) Then
                    result(2) = "sccustomdbl7 required numeric." : GoTo selesai
                End If
                'sccustomdbl8(38) As Double
                If (IsNumeric(dataRowCommission(38)) = False) Then
                    result(2) = "sccustomdbl8 required numeric." : GoTo selesai
                End If
                'sccustomdbl9(39) As Double
                If (IsNumeric(dataRowCommission(39)) = False) Then
                    result(2) = "sccustomdbl9 required numeric." : GoTo selesai
                End If
                'sccustomdbl10(40) As Double
                If (IsNumeric(dataRowCommission(40)) = False) Then
                    result(2) = "sccustomdbl10 required numeric." : GoTo selesai
                End If

                'sccustomdate1(41) As Date
                If (IsDate(dataRowCommission(41)) = False) Then
                    result(2) = "sccustomdate1 required date." : GoTo selesai
                End If
                'sccustomdate2(42) As Date
                If (IsDate(dataRowCommission(42)) = False) Then
                    result(2) = "sccustomdate2 required date." : GoTo selesai
                End If
                'sccustomdate3(43) As Date
                If (IsDate(dataRowCommission(43)) = False) Then
                    result(2) = "sccustomdate3 required date." : GoTo selesai
                End If
                'sccustomdate4(44) As Date
                If (IsDate(dataRowCommission(44)) = False) Then
                    result(2) = "sccustomdate4 required date." : GoTo selesai
                End If
                'sccustomdate5(45) As Date
                If (IsDate(dataRowCommission(45)) = False) Then
                    result(2) = "sccustomdate5 required date." : GoTo selesai
                End If
                'sccustomdate6(46) As Date
                If (IsDate(dataRowCommission(46)) = False) Then
                    result(2) = "sccustomdate6 required date." : GoTo selesai
                End If
                'sccustomdate7(47) As Date
                If (IsDate(dataRowCommission(47)) = False) Then
                    result(2) = "sccustomdate7 required date." : GoTo selesai
                End If
                'sccustomdate8(48) As Date
                If (IsDate(dataRowCommission(48)) = False) Then
                    result(2) = "sccustomdate8 required date." : GoTo selesai
                End If
                'sccustomdate9(49) As Date
                If (IsDate(dataRowCommission(49)) = False) Then
                    result(2) = "sccustomdate9 required date." : GoTo selesai
                End If
                'sccustomdate10(50) As Date
                If (IsDate(dataRowCommission(50)) = False) Then
                    result(2) = "sccustomdate10 required date." : GoTo selesai
                End If


                'END OF VALIDASI TIPE DATA Price -----------------------------------

                AsDataTableTambahData(dtCommission, "scidkontak~sckomisi1~sckomisi2~sckomisi3~sckomisi4~sckomisi5~sckomisi6~sckomisi7~sckomisi8~sckomisi9~sckomisi10~sccustomtext1~sccustomtext2~sccustomtext3~sccustomtext4~sccustomtext5~sccustomtext6~sccustomtext7~sccustomtext8~sccustomtext9~sccustomtext10~sccustomint1~sccustomint2~sccustomint3~sccustomint4~sccustomint5~sccustomint6~sccustomint7~sccustomint8~sccustomint9~sccustomint10~sccustomdbl1~sccustomdbl2~sccustomdbl3~sccustomdbl4~sccustomdbl5~sccustomdbl6~sccustomdbl7~sccustomdbl8~sccustomdbl9~sccustomdbl10~sccustomdate1~sccustomdate2~sccustomdate3~sccustomdate4~sccustomdate5~sccustomdate6~sccustomdate7~sccustomdate8~sccustomdate9~sccustomdate10", dataRowCommission(0) & "~" & dataRowCommission(1) & "~" & dataRowCommission(2) & "~" & dataRowCommission(3) & "~" & dataRowCommission(4) & "~" & dataRowCommission(5) & "~" & dataRowCommission(6) & "~" & dataRowCommission(7) & "~" & dataRowCommission(8) & "~" & dataRowCommission(9) & "~" & dataRowCommission(10) & "~" & dataRowCommission(11) & "~" & dataRowCommission(12) & "~" & dataRowCommission(13) & "~" & dataRowCommission(14) & "~" & dataRowCommission(15) & "~" & dataRowCommission(16) & "~" & dataRowCommission(17) & "~" & dataRowCommission(18) & "~" & dataRowCommission(19) & "~" & dataRowCommission(20) & "~" & dataRowCommission(21) & "~" & dataRowCommission(22) & "~" & dataRowCommission(23) & "~" & dataRowCommission(24) & "~" & dataRowCommission(25) & "~" & dataRowCommission(26) & "~" & dataRowCommission(27) & "~" & dataRowCommission(28) & "~" & dataRowCommission(29) & "~" & dataRowCommission(30) & "~" & dataRowCommission(31) & "~" & dataRowCommission(32) & "~" & dataRowCommission(33) & "~" & dataRowCommission(34) & "~" & dataRowCommission(35) & "~" & dataRowCommission(36) & "~" & dataRowCommission(37) & "~" & dataRowCommission(38) & "~" & dataRowCommission(39) & "~" & dataRowCommission(40) & "~" & dataRowCommission(41) & "~" & dataRowCommission(42) & "~" & dataRowCommission(43) & "~" & dataRowCommission(44) & "~" & dataRowCommission(45) & "~" & dataRowCommission(46) & "~" & dataRowCommission(47) & "~" & dataRowCommission(48) & "~" & dataRowCommission(49) & "~" & dataRowCommission(50))

            Next

        End If
        'END OF VALIDASI DAN SET DATA PRICE ===============================================

        If isUpdate = False Then
            'CEK TERKAIT =============================================================
            'CEK DI DATABASE ================================================================
            Dim dtc As DataTable
            Dim exist As Integer = 0
            dtc = AsDataTableAmbilDariDB("SELECT COUNT(kid) FROM m1_contact WHERE kkode='" & kodekontak & "' AND kkategori='" & kategorikontak & "'")
            exist = dtc.Rows(0)(0)

            If (exist > 0) Then
                result(2) = "'code " & kodekontak & "' and category '" & kategorikontak & "' already exist for column kkode and kkategori." : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================
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
                    result(4) = drutama("kid")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kid) FROM M1_Contact WHERE kid='" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m1_Contact_History
                        Dim contactSimpanHistory As String = SimpanHistory.M1_Contact_HistorySimpan("" & paramSplit(0) & "★M1_Contact_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                        Dim contactSplit() As String = contactSimpanHistory.Split(sptParam)
                        Dim contactSplitResult() As String = contactSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (contactSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & contactSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M1_Contact set kkode  = '" & FixQuotes(drutama("kkode")) & "', knama  = '" & FixQuotes(drutama("knama")) & "', kkategori  = '" & FixQuotes(drutama("kkategori")) & "', kkategorinama  = '" & FixQuotes(drutama("kkategorinama")) & "', kcabang  = '" & FixQuotes(drutama("kcabang")) & "', kcabangnama  = '" & FixQuotes(drutama("kcabangnama")) & "', klokasi  = '" & FixQuotes(drutama("klokasi")) & "', klokasinama  = '" & FixQuotes(drutama("klokasinama")) & "', kgudang  = '" & FixQuotes(drutama("kgudang")) & "', kgudangnama  = '" & FixQuotes(drutama("kgudangnama")) & "', kkategorisalesman  = '" & FixQuotes(drutama("kkategorisalesman")) & "', kkategorisalesmannama  = '" & FixQuotes(drutama("kkategorisalesmannama")) & "', karea  = '" & FixQuotes(drutama("karea")) & "', kareanama  = '" & FixQuotes(drutama("kareanama")) & "', kkategoricustomer  = '" & FixQuotes(drutama("kkategoricustomer")) & "', kkategoricustomernama  = '" & FixQuotes(drutama("kkategoricustomernama")) & "', kkategorisupplier  = '" & FixQuotes(drutama("kkategorisupplier")) & "', kkategorisuppliernama  = '" & FixQuotes(drutama("kkategorisuppliernama")) & "', kdivisi  = '" & FixQuotes(drutama("kdivisi")) & "', kdivisinama  = '" & FixQuotes(drutama("kdivisinama")) & "', ksubdivisi  = '" & FixQuotes(drutama("ksubdivisi")) & "', ksubdivisinama  = '" & FixQuotes(drutama("ksubdivisinama")) & "', ksalesman  = " & drutama("ksalesman") & ", ksalesmannama  = '" & FixQuotes(drutama("ksalesmannama")) & "', kkontakperson  = '" & FixQuotes(drutama("kkontakperson")) & "', kterminglobal  = " & drutama("kterminglobal") & ", kaktif  = " & drutama("kaktif") & ", kaktiftgl  = '" & FixQuotes(AsFormatTanggal(drutama("kaktiftgl"))) & "', k1alamat1  = '" & FixQuotes(drutama("k1alamat1")) & "', k1alamat2  = '" & FixQuotes(drutama("k1alamat2")) & "', k1alamat3  = '" & FixQuotes(drutama("k1alamat3")) & "', k1alamat4  = '" & FixQuotes(drutama("k1alamat4")) & "', k1alamat5  = '" & FixQuotes(drutama("k1alamat5")) & "', k1kota  = '" & FixQuotes(drutama("k1kota")) & "', k1propinsi  = '" & FixQuotes(drutama("k1propinsi")) & "', k1kodepos  = '" & FixQuotes(drutama("k1kodepos")) & "', k1negara  = '" & FixQuotes(drutama("k1negara")) & "', k1kontakperson  = '" & FixQuotes(drutama("k1kontakperson")) & "', k1kontaknohp  = '" & FixQuotes(drutama("k1kontaknohp")) & "', k1kontakemail  = '" & FixQuotes(drutama("k1kontakemail")) & "', k1notelp1  = '" & FixQuotes(drutama("k1notelp1")) & "', k1notelp2  = '" & FixQuotes(drutama("k1notelp2")) & "', k1nofax  = '" & FixQuotes(drutama("k1nofax")) & "', k1email  = '" & FixQuotes(drutama("k1email")) & "', k1website  = '" & FixQuotes(drutama("k1website")) & "', k2alamat1  = '" & FixQuotes(drutama("k2alamat1")) & "', k2alamat2  = '" & FixQuotes(drutama("k2alamat2")) & "', k2alamat3  = '" & FixQuotes(drutama("k2alamat3")) & "', k2alamat4  = '" & FixQuotes(drutama("k2alamat4")) & "', k2alamat5  = '" & FixQuotes(drutama("k2alamat5")) & "', k2propinsi  = '" & FixQuotes(drutama("k2propinsi")) & "', k2kota  = '" & FixQuotes(drutama("k2kota")) & "', k2kodepos  = '" & FixQuotes(drutama("k2kodepos")) & "', k2negara  = '" & FixQuotes(drutama("k2negara")) & "', k2kontakperson  = '" & FixQuotes(drutama("k2kontakperson")) & "', k2kontaknohp  = '" & FixQuotes(drutama("k2kontaknohp")) & "', k2kontakemail  = '" & FixQuotes(drutama("k2kontakemail")) & "', k2notelp1  = '" & FixQuotes(drutama("k2notelp1")) & "', k2notelp2  = '" & FixQuotes(drutama("k2notelp2")) & "', k2nofax  = '" & FixQuotes(drutama("k2nofax")) & "', k2email  = '" & FixQuotes(drutama("k2email")) & "', k2website  = '" & FixQuotes(drutama("k2website")) & "', k3alamat1  = '" & FixQuotes(drutama("k3alamat1")) & "', k3alamat2  = '" & FixQuotes(drutama("k3alamat2")) & "', k3alamat3  = '" & FixQuotes(drutama("k3alamat3")) & "', k3alamat4  = '" & FixQuotes(drutama("k3alamat4")) & "', k3alamat5  = '" & FixQuotes(drutama("k3alamat5")) & "', k3kota  = '" & FixQuotes(drutama("k3kota")) & "', k3propinsi  = '" & FixQuotes(drutama("k3propinsi")) & "', k3kodepos  = '" & FixQuotes(drutama("k3kodepos")) & "', k3negara  = '" & FixQuotes(drutama("k3negara")) & "', k3kontakperson  = '" & FixQuotes(drutama("k3kontakperson")) & "', k3kontaknohp  = '" & FixQuotes(drutama("k3kontaknohp")) & "', k3kontakemail  = '" & FixQuotes(drutama("k3kontakemail")) & "', k3notelp1  = '" & FixQuotes(drutama("k3notelp1")) & "', k3notelp2  = '" & FixQuotes(drutama("k3notelp2")) & "', k3nofax  = '" & FixQuotes(drutama("k3nofax")) & "', k3email  = '" & FixQuotes(drutama("k3email")) & "', k3website  = '" & FixQuotes(drutama("k3website")) & "', k4alamat1  = '" & FixQuotes(drutama("k4alamat1")) & "', k4alamat2  = '" & FixQuotes(drutama("k4alamat2")) & "', k4alamat3  = '" & FixQuotes(drutama("k4alamat3")) & "', k4alamat4  = '" & FixQuotes(drutama("k4alamat4")) & "', k4alamat5  = '" & FixQuotes(drutama("k4alamat5")) & "', k4kota  = '" & FixQuotes(drutama("k4kota")) & "', k4propinsi  = '" & FixQuotes(drutama("k4propinsi")) & "', k4kodepos  = '" & FixQuotes(drutama("k4kodepos")) & "', k4negara  = '" & FixQuotes(drutama("k4negara")) & "', k4kontakperson  = '" & FixQuotes(drutama("k4kontakperson")) & "', k4kontaknohp  = '" & FixQuotes(drutama("k4kontaknohp")) & "', k4kontakemail  = '" & FixQuotes(drutama("k4kontakemail")) & "', k4notelp1  = '" & FixQuotes(drutama("k4notelp1")) & "', k4notelp2  = '" & FixQuotes(drutama("k4notelp2")) & "', k4nofax  = '" & FixQuotes(drutama("k4nofax")) & "', k4email  = '" & FixQuotes(drutama("k4email")) & "', k4website  = '" & FixQuotes(drutama("k4website")) & "', knpwp  = '" & FixQuotes(drutama("knpwp")) & "', kpkp  = " & drutama("kpkp") & ", kbatashutang  = '" & FixDouble(drutama("kbatashutang")) & "', kterminbeli  = '" & FixQuotes(drutama("kterminbeli")) & "', krekhutang  = '" & FixQuotes(drutama("krekhutang")) & "', kbagpembelian  = " & drutama("kbagpembelian") & ", kfobbeli  = '" & FixQuotes(drutama("kfobbeli")) & "', kviabeli  = '" & FixQuotes(drutama("kviabeli")) & "', kbataspiutang  = '" & FixDouble(drutama("kbataspiutang")) & "', kterminjual  = '" & FixQuotes(drutama("kterminjual")) & "', krekpiutang  = '" & FixQuotes(drutama("krekpiutang")) & "', kbagpenjualan  = " & drutama("kbagpenjualan") & ", ktingkatjual  = " & drutama("ktingkatjual") & ", kfobjual  = '" & FixQuotes(drutama("kfobjual")) & "', kviajual  = '" & FixQuotes(drutama("kviajual")) & "', ktglkontrak  = '" & FixQuotes(AsFormatTanggal(drutama("ktglkontrak"))) & "', kbank  = '" & FixQuotes(drutama("kbank")) & "', knorekening  = '" & FixQuotes(drutama("knorekening")) & "', kjeniskelamin  = " & drutama("kjeniskelamin") & ", kmatauang  = '" & FixQuotes(drutama("kmatauang")) & "', ktgllahir  = '" & FixQuotes(AsFormatTanggal(drutama("ktgllahir"))) & "', ktglnikah  = '" & FixQuotes(AsFormatTanggal(drutama("ktglnikah"))) & "', kkomisipenjualan  = '" & FixDouble(drutama("kkomisipenjualan")) & "', kcatatan  = '" & FixQuotes(drutama("kcatatan")) & "', kcustomtext1  = '" & FixQuotes(drutama("kcustomtext1")) & "', kcustomtext2  = '" & FixQuotes(drutama("kcustomtext2")) & "', kcustomtext3  = '" & FixQuotes(drutama("kcustomtext3")) & "', kcustomtext4  = '" & FixQuotes(drutama("kcustomtext4")) & "', kcustomtext5  = '" & FixQuotes(drutama("kcustomtext5")) & "', kcustomtext6  = '" & FixQuotes(drutama("kcustomtext6")) & "', kcustomtext7  = '" & FixQuotes(drutama("kcustomtext7")) & "', kcustomtext8  = '" & FixQuotes(drutama("kcustomtext8")) & "', kcustomtext9  = '" & FixQuotes(drutama("kcustomtext9")) & "', kmodifikasiuser  = " & drutama("kmodifikasiuser") & ", kmodifikasitgl  = NOW(), kcustomtext10  = '" & FixQuotes(drutama("kcustomtext10")) & "', kcustomint1  = " & drutama("kcustomint1") & ", kcustomint2  = " & drutama("kcustomint2") & ", kcustomint3  = " & drutama("kcustomint3") & ", kcustomdbl1  = '" & FixDouble(drutama("kcustomdbl1")) & "', kcustomdbl2  = '" & FixDouble(drutama("kcustomdbl2")) & "', kcustomdbl3  = '" & FixDouble(drutama("kcustomdbl3")) & "', kcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate1"))) & "', kcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate2"))) & "', kcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate3"))) & "', kkomisikode = '" & FixQuotes(drutama("kkomisikode")) & "', kdownloaded = 0, khargacustom = '" & FixDouble(drutama("khargacustom")) & "' where kid = '" & drutama("kid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Transaction data not found." : Trans.Rollback() : GoTo selesai
                    End If
                Else
                    sql = "Insert into M1_Contact (kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, kkomisikode, khargacustom) values('" & FixQuotes(drutama("kkode")) & "', '" & FixQuotes(drutama("knama")) & "', '" & FixQuotes(drutama("kkategori")) & "', '" & FixQuotes(drutama("kkategorinama")) & "', '" & FixQuotes(drutama("kcabang")) & "', '" & FixQuotes(drutama("kcabangnama")) & "', '" & FixQuotes(drutama("klokasi")) & "', '" & FixQuotes(drutama("klokasinama")) & "', '" & FixQuotes(drutama("kgudang")) & "', '" & FixQuotes(drutama("kgudangnama")) & "', '" & FixQuotes(drutama("kkategorisalesman")) & "', '" & FixQuotes(drutama("kkategorisalesmannama")) & "', '" & FixQuotes(drutama("karea")) & "', '" & FixQuotes(drutama("kareanama")) & "', '" & FixQuotes(drutama("kkategoricustomer")) & "', '" & FixQuotes(drutama("kkategoricustomernama")) & "', '" & FixQuotes(drutama("kkategorisupplier")) & "', '" & FixQuotes(drutama("kkategorisuppliernama")) & "', '" & FixQuotes(drutama("kdivisi")) & "', '" & FixQuotes(drutama("kdivisinama")) & "', '" & FixQuotes(drutama("ksubdivisi")) & "', '" & FixQuotes(drutama("ksubdivisinama")) & "', " & drutama("ksalesman") & ", '" & FixQuotes(drutama("ksalesmannama")) & "', '" & FixQuotes(drutama("kkontakperson")) & "', " & drutama("kterminglobal") & ", " & drutama("kaktif") & ", '" & FixQuotes(AsFormatTanggal(drutama("kaktiftgl"))) & "', '" & FixQuotes(drutama("k1alamat1")) & "', '" & FixQuotes(drutama("k1alamat2")) & "', '" & FixQuotes(drutama("k1alamat3")) & "', '" & FixQuotes(drutama("k1alamat4")) & "', '" & FixQuotes(drutama("k1alamat5")) & "', '" & FixQuotes(drutama("k1kota")) & "', '" & FixQuotes(drutama("k1propinsi")) & "', '" & FixQuotes(drutama("k1kodepos")) & "', '" & FixQuotes(drutama("k1negara")) & "', '" & FixQuotes(drutama("k1kontakperson")) & "', '" & FixQuotes(drutama("k1kontaknohp")) & "', '" & FixQuotes(drutama("k1kontakemail")) & "', '" & FixQuotes(drutama("k1notelp1")) & "', '" & FixQuotes(drutama("k1notelp2")) & "', '" & FixQuotes(drutama("k1nofax")) & "', '" & FixQuotes(drutama("k1email")) & "', '" & FixQuotes(drutama("k1website")) & "', '" & FixQuotes(drutama("k2alamat1")) & "', '" & FixQuotes(drutama("k2alamat2")) & "', '" & FixQuotes(drutama("k2alamat3")) & "', '" & FixQuotes(drutama("k2alamat4")) & "', '" & FixQuotes(drutama("k2alamat5")) & "', '" & FixQuotes(drutama("k2propinsi")) & "', '" & FixQuotes(drutama("k2kota")) & "', '" & FixQuotes(drutama("k2kodepos")) & "', '" & FixQuotes(drutama("k2negara")) & "', '" & FixQuotes(drutama("k2kontakperson")) & "', '" & FixQuotes(drutama("k2kontaknohp")) & "', '" & FixQuotes(drutama("k2kontakemail")) & "', '" & FixQuotes(drutama("k2notelp1")) & "', '" & FixQuotes(drutama("k2notelp2")) & "', '" & FixQuotes(drutama("k2nofax")) & "', '" & FixQuotes(drutama("k2email")) & "', '" & FixQuotes(drutama("k2website")) & "', '" & FixQuotes(drutama("k3alamat1")) & "', '" & FixQuotes(drutama("k3alamat2")) & "', '" & FixQuotes(drutama("k3alamat3")) & "', '" & FixQuotes(drutama("k3alamat4")) & "', '" & FixQuotes(drutama("k3alamat5")) & "', '" & FixQuotes(drutama("k3kota")) & "', '" & FixQuotes(drutama("k3propinsi")) & "', '" & FixQuotes(drutama("k3kodepos")) & "', '" & FixQuotes(drutama("k3negara")) & "', '" & FixQuotes(drutama("k3kontakperson")) & "', '" & FixQuotes(drutama("k3kontaknohp")) & "', '" & FixQuotes(drutama("k3kontakemail")) & "', '" & FixQuotes(drutama("k3notelp1")) & "', '" & FixQuotes(drutama("k3notelp2")) & "', '" & FixQuotes(drutama("k3nofax")) & "', '" & FixQuotes(drutama("k3email")) & "', '" & FixQuotes(drutama("k3website")) & "', '" & FixQuotes(drutama("k4alamat1")) & "', '" & FixQuotes(drutama("k4alamat2")) & "', '" & FixQuotes(drutama("k4alamat3")) & "', '" & FixQuotes(drutama("k4alamat4")) & "', '" & FixQuotes(drutama("k4alamat5")) & "', '" & FixQuotes(drutama("k4kota")) & "', '" & FixQuotes(drutama("k4propinsi")) & "', '" & FixQuotes(drutama("k4kodepos")) & "', '" & FixQuotes(drutama("k4negara")) & "', '" & FixQuotes(drutama("k4kontakperson")) & "', '" & FixQuotes(drutama("k4kontaknohp")) & "', '" & FixQuotes(drutama("k4kontakemail")) & "', '" & FixQuotes(drutama("k4notelp1")) & "', '" & FixQuotes(drutama("k4notelp2")) & "', '" & FixQuotes(drutama("k4nofax")) & "', '" & FixQuotes(drutama("k4email")) & "', '" & FixQuotes(drutama("k4website")) & "', '" & FixQuotes(drutama("knpwp")) & "', " & drutama("kpkp") & ", '" & FixDouble(drutama("kbatashutang")) & "', '" & FixQuotes(drutama("kterminbeli")) & "', '" & FixQuotes(drutama("krekhutang")) & "', " & drutama("kbagpembelian") & ", '" & FixQuotes(drutama("kfobbeli")) & "', '" & FixQuotes(drutama("kviabeli")) & "', '" & FixDouble(drutama("kbataspiutang")) & "', '" & FixQuotes(drutama("kterminjual")) & "', '" & FixQuotes(drutama("krekpiutang")) & "', " & drutama("kbagpenjualan") & ", " & drutama("ktingkatjual") & ", '" & FixQuotes(drutama("kfobjual")) & "', '" & FixQuotes(drutama("kviajual")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ktglkontrak"))) & "', '" & FixQuotes(drutama("kbank")) & "', '" & FixQuotes(drutama("knorekening")) & "', " & drutama("kjeniskelamin") & ", '" & FixQuotes(drutama("kmatauang")) & "', '" & FixQuotes(AsFormatTanggal(drutama("ktgllahir"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("ktglnikah"))) & "', '" & FixDouble(drutama("kkomisipenjualan")) & "', '" & FixQuotes(drutama("kcatatan")) & "', " & drutama("kinputuser") & ", NOW(), '" & FixQuotes(drutama("kcustomtext1")) & "', '" & FixQuotes(drutama("kcustomtext2")) & "', '" & FixQuotes(drutama("kcustomtext3")) & "', '" & FixQuotes(drutama("kcustomtext4")) & "', '" & FixQuotes(drutama("kcustomtext5")) & "', '" & FixQuotes(drutama("kcustomtext6")) & "', '" & FixQuotes(drutama("kcustomtext7")) & "', '" & FixQuotes(drutama("kcustomtext8")) & "', '" & FixQuotes(drutama("kcustomtext9")) & "', " & drutama("kmodifikasiuser") & ", '1971-01-01 00:00:00', '" & FixQuotes(drutama("kcustomtext10")) & "', " & drutama("kcustomint1") & ", " & drutama("kcustomint2") & ", " & drutama("kcustomint3") & ", '" & FixDouble(drutama("kcustomdbl1")) & "', '" & FixDouble(drutama("kcustomdbl2")) & "', '" & FixDouble(drutama("kcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("kcustomdate3"))) & "', '" & FixQuotes(drutama("kkomisikode")) & "', '" & FixDouble(drutama("khargacustom")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select kid from M1_Contact where kkode='" & FixQuotes(drutama("kkode")) & "' AND kkategori='" & FixQuotes(drutama("kkategori")) & "' AND kinputuser= '" & userid & "' order by kmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_Contact_Attention where kaidkontak = '" & result(4) & "'"
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
                    If isUpdate Then
                        For Each dr1 As DataRow In dtdetail.Rows
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("(" & dr1("kaid") & ", " & result(4) & ", '" & FixQuotes(dr1("kakodekontak")) & "', '" & FixQuotes(dr1("kanama")) & "', '" & FixQuotes(dr1("kajabatan")) & "', '" & FixQuotes(dr1("kanotelp")) & "', '" & FixQuotes(dr1("kanofax")) & "', '" & FixQuotes(dr1("kanohp")) & "', '" & FixQuotes(dr1("kaemail")) & "', '" & FixQuotes(dr1("kawebsite")) & "', '" & FixQuotes(dr1("kamessenger")) & "', '" & FixQuotes(dr1("kaalamat")) & "', '" & FixQuotes(AsFormatTanggal(dr1("katgllahir"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("katglnikah"))) & "', '" & FixQuotes(dr1("kacatatan")) & "', " & dr1("kadefault") & ", " & dr1("kainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("kamodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kamodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "')")
                        Next
                        sql = "Insert into M1_Contact_Attention(kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl) values" & strValue2.ToString & ""
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        For Each dr1 As DataRow In dtdetail.Rows
                            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                            strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("kakodekontak")) & "', '" & FixQuotes(dr1("kanama")) & "', '" & FixQuotes(dr1("kajabatan")) & "', '" & FixQuotes(dr1("kanotelp")) & "', '" & FixQuotes(dr1("kanofax")) & "', '" & FixQuotes(dr1("kanohp")) & "', '" & FixQuotes(dr1("kaemail")) & "', '" & FixQuotes(dr1("kawebsite")) & "', '" & FixQuotes(dr1("kamessenger")) & "', '" & FixQuotes(dr1("kaalamat")) & "', '" & FixQuotes(AsFormatTanggal(dr1("katgllahir"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("katglnikah"))) & "', '" & FixQuotes(dr1("kacatatan")) & "', " & dr1("kadefault") & ", " & dr1("kainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("kamodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kamodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "')")
                        Next
                        sql = "Insert into M1_Contact_Attention(kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl) values" & strValue2.ToString & ""
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


                'Hapus price ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_Contact_Price where khidkontak = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses price
                If (dtPrice.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtPrice.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("khidbarang")) & "', '" & FixQuotes(dr1("khsatuan")) & "', '" & FixDouble(dr1("khkomisi")) & "', '" & FixDouble(dr1("khhargabeli")) & "', '" & FixDouble(dr1("khhargajual")) & "', '" & FixQuotes(AsFormatTanggal(dr1("khberlakudari"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("khberlakusampai"))) & "', '" & FixQuotes(dr1("khcatatan")) & "', '" & FixQuotes(dr1("khinputuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("khinputtgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("khmodifikasiuser")) & "', '" & FixQuotes(AsFormatTanggal(dr1("khmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & "', '" & FixQuotes(dr1("khcustomtext1")) & "', '" & FixQuotes(dr1("khcustomtext2")) & "', '" & FixQuotes(dr1("khcustomtext3")) & "', '" & FixQuotes(dr1("khcustomtext4")) & "', '" & FixQuotes(dr1("khcustomtext5")) & "', " & dr1("khcustomint1") & ", " & dr1("khcustomint2") & ", " & dr1("khcustomint3") & ", " & dr1("khcustomint4") & ", " & dr1("khcustomint5") & ", '" & FixDouble(dr1("khcustomdbl1")) & "', '" & FixDouble(dr1("khcustomdbl2")) & "', '" & FixDouble(dr1("khcustomdbl3")) & "', '" & FixDouble(dr1("khcustomdbl4")) & "', '" & FixDouble(dr1("khcustomdbl5")) & "', '" & FixQuotes(AsFormatTanggal(dr1("khcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("khcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("khcustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("khcustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("khcustomdate5"))) & "')")
                    Next
                    sql = "Insert into M1_Contact_Price(khidkontak, khidbarang, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Hapus price ketika update
                If (isUpdate) Then
                    sql = "Delete from M1_Salesman_Commission where scidkontak = " & result(4)
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                'Proses price
                If (dtCommission.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtCommission.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixDouble(dr1("sckomisi1")) & "', '" & FixDouble(dr1("sckomisi2")) & "', '" & FixDouble(dr1("sckomisi3")) & "', '" & FixDouble(dr1("sckomisi4")) & "', '" & FixDouble(dr1("sckomisi5")) & "', '" & FixDouble(dr1("sckomisi6")) & "', '" & FixDouble(dr1("sckomisi7")) & "', '" & FixDouble(dr1("sckomisi8")) & "', '" & FixDouble(dr1("sckomisi9")) & "', '" & FixDouble(dr1("sckomisi10")) & "', '" & FixQuotes(dr1("sccustomtext1")) & "', '" & FixQuotes(dr1("sccustomtext2")) & "', '" & FixQuotes(dr1("sccustomtext3")) & "', '" & FixQuotes(dr1("sccustomtext4")) & "', '" & FixQuotes(dr1("sccustomtext5")) & "', '" & FixQuotes(dr1("sccustomtext6")) & "', '" & FixQuotes(dr1("sccustomtext7")) & "', '" & FixQuotes(dr1("sccustomtext8")) & "', '" & FixQuotes(dr1("sccustomtext9")) & "', '" & FixQuotes(dr1("sccustomtext10")) & "', '" & FixQuotes(dr1("sccustomint1")) & "', '" & FixQuotes(dr1("sccustomint2")) & "', '" & FixQuotes(dr1("sccustomint3")) & "', '" & FixQuotes(dr1("sccustomint4")) & "', '" & FixQuotes(dr1("sccustomint5")) & "', '" & FixQuotes(dr1("sccustomint6")) & "', '" & FixQuotes(dr1("sccustomint7")) & "', '" & FixQuotes(dr1("sccustomint8")) & "', '" & FixQuotes(dr1("sccustomint9")) & "', '" & FixQuotes(dr1("sccustomint10")) & "', '" & FixDouble(dr1("sccustomdbl1")) & "', '" & FixDouble(dr1("sccustomdbl2")) & "', '" & FixDouble(dr1("sccustomdbl3")) & "', '" & FixDouble(dr1("sccustomdbl4")) & "', '" & FixDouble(dr1("sccustomdbl5")) & "', '" & FixDouble(dr1("sccustomdbl6")) & "', '" & FixDouble(dr1("sccustomdbl7")) & "', '" & FixDouble(dr1("sccustomdbl8")) & "', '" & FixDouble(dr1("sccustomdbl9")) & "', '" & FixDouble(dr1("sccustomdbl10")) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate3"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate4"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate5"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate6"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate7"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate8"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate9"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("sccustomdate10"))) & "')")
                    Next
                    sql = "Insert into M1_Salesman_Commission(scidkontak, sckomisi1, sckomisi2, sckomisi3, sckomisi4, sckomisi5, sckomisi6, sckomisi7, sckomisi8, sckomisi9, sckomisi10, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomtext6, sccustomtext7, sccustomtext8, sccustomtext9, sccustomtext10, sccustomint1, sccustomint2, sccustomint3, sccustomint4, sccustomint5, sccustomint6, sccustomint7, sccustomint8, sccustomint9, sccustomint10, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdbl4, sccustomdbl5, sccustomdbl6, sccustomdbl7, sccustomdbl8, sccustomdbl9, sccustomdbl10, sccustomdate1, sccustomdate2, sccustomdate3, sccustomdate4, sccustomdate5, sccustomdate6, sccustomdate7, sccustomdate8, sccustomdate9, sccustomdate10) values" & strValue2.ToString & ""
                    'result(2) = sql : GoTo selesai
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If


                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = notransaksi
                result(3) = 0
                result(4) = result(4)


                'AMBIL DATA =============================================================
                Dim paramSearch As String = M1_ContactSearch(PostWsSearch(paramSplit(0), "M1_ContactSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu, , userid))
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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactDelete(ByVal param As String) As String

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
        Dim search As String = "", Filter As String = "", Sorting As String = "", formatTgl As String = "", formatTglWaktu As String = ""

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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "kid required numeric." : GoTo selesai
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

            'CEK TERKAIT =============================================================
            Dim paramTerkait As String = M1_ContactTerkait(PostWsTerkait(paramSplit(0), "M1_ContactTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
            Dim hasilTerkait As New RsHasilWsSearch
            hasilTerkait = GetWsSearch(paramTerkait)
            If hasilTerkait.success = 1 Then
                result(2) = "It has related transactions."

                resultPaging(0) = hasilTerkait.isPaging
                resultPaging(1) = hasilTerkait.isNext
                resultPaging(2) = hasilTerkait.isPrevious
                resultPaging(3) = hasilTerkait.countPage
                resultPaging(4) = hasilTerkait.countRow

                search = hasilTerkait.data : Trans.Rollback() : GoTo selesai
            End If
            'END OF CEK TERKAIT ======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m1_Contact_History
            Dim contactSimpanHistory As String = SimpanHistory.M1_Contact_HistorySimpan("" & paramSplit(0) & "★M1_Contact_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim contactSplit() As String = contactSimpanHistory.Split(sptParam)
            Dim contactSplitResult() As String = contactSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (contactSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & contactSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            'DELETE HARGA
            sql = "DELETE FROM M1_Contact_Price WHERE khidkontak = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M1_Contact_Attention WHERE kaidkontak = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M1_Contact WHERE kid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_ContactSearch(PostWsSearch(paramSplit(0), "M1_ContactSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu, , userid))
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
    Public Function M1_ContactSearch(ByVal param As String) As String
        'M1_ContactSearch --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, 
        'kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, 
        'kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, 
        'kterminjual, ktingkatjual, ksalesmankode, kkomisipenjualan, cppoin, kpkp, dccnilai

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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


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
            Filter = Filter.Replace("kaktif", "c.kaktif")
            Filter = Filter.Replace("knama", "c.knama")
            Filter = Filter.Replace("kkode", "c.kkode")
            Filter = Filter.Replace("kkategori", "c.kkategori")
            Filter = Filter.Replace("k1kota", "c.k1kota")
            Filter = Filter.Replace("karea", "c.karea")
            Filter = Filter.Replace("ksalesmannama", "c.ksalesmannama")
            Filter = Filter.Replace("ksalesmankode", "cs.kkode")
            Filter = Filter.Replace("ksalesman", "c.ksalesman")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_cd")
        'sql = "SELECT c.kid AS kid, c.kkode AS kkode, c.knama AS knama, c.kkategori AS kkategori, cc.ccnama AS kkategorinama, c.kcabang AS kcabang, c.klokasi AS klokasi, c.kgudang AS kgudang, c.kkategorisalesman AS kkategorisalesman, sc.scnama AS kkategorisalesmannama, c.karea AS karea, a.anama AS kareanama, c.kkategoricustomer AS kkategoricustomer, custc.ccnama AS kkategoricustomernama, c.kkategorisupplier AS kkategorisupplier, suppc.scnama AS kkategorisuppliernama, c.ksalesman AS ksalesman, cs.knama AS ksalesmannama, ca.kanama AS kkontakperson, c.kaktif AS kaktif, c.k1alamat1 AS k1alamat1, c.k1alamat2 AS k1alamat2, c.k1kota AS k1kota, c.k1propinsi AS k1propinsi, c.k1kodepos AS k1kodepos, c.k1negara AS k1negara, c.k1kontakperson AS k1kontakperson, c.k1notelp1 AS k1notelp1, c.k2alamat1 AS k2alamat1, c.k2alamat2 AS k2alamat2, c.k2propinsi AS k2propinsi, c.k2kota AS k2kota, c.k2kodepos AS k2kodepos, c.k2negara AS k2negara, c.k2kontakperson AS k2kontakperson, c.k2notelp1 AS k2notelp1, c.kterminbeli AS kterminbeli, c.kterminjual AS kterminjual, c.ktingkatjual AS ktingkatjual, c.kkomisipenjualan AS kkomisipenjualan, cs.kkode AS ksalesmankode, cp.cppoin, c.kpkp, IFNULL(dcc.dccnilai,0) as dccnilai from `m1_contact` `c` join `m0_user` `u` on `u`.`userid` = 'valuserid' and  (`c`.`klokasi` = '' or `c`.`klokasi` = `u`.`ulokasi`) left join `m1_contact` `cs` on `c`.`ksalesman` = `cs`.`kid` left join `m1_contact_attention` `ca` on `c`.`kid` = `ca`.`kaidkontak` and `ca`.`kadefault` = 1 left join `m1_area` `a` on `c`.`karea` = `a`.`akode` left join `m1_contact_category` `cc` on `c`.`kkategori` = `cc`.`cckode` left join `m1_salesman_category` `sc` on `c`.`kkategorisalesman` = `sc`.`sckode` left join `m1_customer_category` `custc` on `c`.`kkategoricustomer` = `custc`.`cckode` left join `m1_supplier_category` `suppc` on `c`.`kkategorisupplier` = `suppc`.`sckode` left join m1_contact_point cp ON c.kid = cp.cpidkontak left join m1_location l on u.ulokasi = l.lkode left join m_12_pos_discount_category_customer dcc on l.lkategoripos = dcc.dcckategori and c.kkategoricustomer = dcc.dcckategoricustomer"
        sql = "SELECT c.kid AS kid, c.kkode AS kkode, c.knama AS knama, c.kkategori AS kkategori, cc.ccnama AS kkategorinama, c.kcabang AS kcabang, c.klokasi AS klokasi, c.kgudang AS kgudang, c.kkategorisalesman AS kkategorisalesman, sc.scnama AS kkategorisalesmannama, c.karea AS karea, a.anama AS kareanama, c.kkategoricustomer AS kkategoricustomer, custc.ccnama AS kkategoricustomernama, c.kkategorisupplier AS kkategorisupplier, suppc.scnama AS kkategorisuppliernama, c.ksalesman AS ksalesman, cs.knama AS ksalesmannama, ca.kanama AS kkontakperson, c.kaktif AS kaktif, c.k1alamat1 AS k1alamat1, c.k1alamat2 AS k1alamat2, c.k1kota AS k1kota, c.k1propinsi AS k1propinsi, c.k1kodepos AS k1kodepos, c.k1negara AS k1negara, c.k1kontakperson AS k1kontakperson, c.k1notelp1 AS k1notelp1, c.k2alamat1 AS k2alamat1, c.k2alamat2 AS k2alamat2, c.k2propinsi AS k2propinsi, c.k2kota AS k2kota, c.k2kodepos AS k2kodepos, c.k2negara AS k2negara, c.k2kontakperson AS k2kontakperson, c.k2notelp1 AS k2notelp1, c.kterminbeli AS kterminbeli, c.kterminjual AS kterminjual, c.ktingkatjual AS ktingkatjual, c.kkomisipenjualan AS kkomisipenjualan, cs.kkode AS ksalesmankode, cp.cppoin, c.kpkp, IFNULL(dcc.dccnilai,0) as dccnilai from `m1_contact` `c` join `m0_user` `u` on `u`.`userid` = 'valuserid' left join `m1_contact` `cs` on `c`.`ksalesman` = `cs`.`kid` left join `m1_contact_attention` `ca` on `c`.`kid` = `ca`.`kaidkontak` and `ca`.`kadefault` = 1 left join `m1_area` `a` on `c`.`karea` = `a`.`akode` left join `m1_contact_category` `cc` on `c`.`kkategori` = `cc`.`cckode` left join `m1_salesman_category` `sc` on `c`.`kkategorisalesman` = `sc`.`sckode` left join `m1_customer_category` `custc` on `c`.`kkategoricustomer` = `custc`.`cckode` left join `m1_supplier_category` `suppc` on `c`.`kkategorisupplier` = `suppc`.`sckode` left join m1_contact_point cp ON c.kid = cp.cpidkontak left join m1_location l on u.ulokasi = l.lkode left join m_12_pos_discount_category_customer dcc on l.lkategoripos = dcc.dcckategori and c.kkategoricustomer = dcc.dcckategoricustomer"

        If userid <> 0 Then
            sql = sql.Replace("= 'valuserid'", "= '" & userid & "'")
        Else
            sql = sql.Replace("= 'valuserid'", " LIKE '%'")
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "c.kid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kid"), 0), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptField,
                     FxDB(dr("kkategori"), ""), sptField,
                     FxDB(dr("kkategorinama"), ""), sptField,
                     FxDB(dr("kcabang"), ""), sptField,
                     FxDB(dr("klokasi"), ""), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("kkategorisalesman"), ""), sptField,
                     FxDB(dr("kkategorisalesmannama"), ""), sptField,
                     FxDB(dr("karea"), ""), sptField,
                     FxDB(dr("kareanama"), ""), sptField,
                     FxDB(dr("kkategoricustomer"), ""), sptField,
                     FxDB(dr("kkategoricustomernama"), ""), sptField,
                     FxDB(dr("kkategorisupplier"), ""), sptField,
                     FxDB(dr("kkategorisuppliernama"), ""), sptField,
                     FxDB(dr("ksalesman"), 0), sptField,
                     FxDB(dr("ksalesmannama"), ""), sptField,
                     FxDB(dr("kkontakperson"), ""), sptField,
                     FxDB(dr("kaktif"), 0), sptField,
                     FxDB(dr("k1alamat1"), ""), sptField,
                     FxDB(dr("k1alamat2"), ""), sptField,
                     FxDB(dr("k1kota"), ""), sptField,
                     FxDB(dr("k1propinsi"), ""), sptField,
                     FxDB(dr("k1kodepos"), ""), sptField,
                     FxDB(dr("k1negara"), ""), sptField,
                     FxDB(dr("k1kontakperson"), ""), sptField,
                     FxDB(dr("k1notelp1"), ""), sptField,
                     FxDB(dr("k2alamat1"), ""), sptField,
                     FxDB(dr("k2alamat2"), ""), sptField,
                     FxDB(dr("k2propinsi"), ""), sptField,
                     FxDB(dr("k2kota"), ""), sptField,
                     FxDB(dr("k2kodepos"), ""), sptField,
                     FxDB(dr("k2negara"), ""), sptField,
                     FxDB(dr("k2kontakperson"), ""), sptField,
                     FxDB(dr("k2notelp1"), ""), sptField,
                     FxDB(dr("kterminbeli"), ""), sptField,
                     FxDB(dr("kterminjual"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("ksalesmankode"), ""), sptField,
                     FxDB(dr("kkomisipenjualan"), 0), sptField,
                     FxDB(dr("cppoin"), 0), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("dccnilai"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Contact data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, kterminjual, ktingkatjual, ksalesmankode, kkomisipenjualan, cppoin, kpkp, dccnilai"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactSearch_Berliando(ByVal param As String) As String
        'M1_ContactSearch --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, 
        'kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, 
        'kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, 
        'kterminjual, ktingkatjual, ksalesmankode, kkomisipenjualan, cppoin, kpkp, dccnilai

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


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


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
            Filter = Filter.Replace("kaktif", "c.kaktif")
            Filter = Filter.Replace("knama", "c.knama")
            Filter = Filter.Replace("kkode", "c.kkode")
            Filter = Filter.Replace("kkategori", "c.kkategori")
            Filter = Filter.Replace("k1kota", "c.k1kota")
            Filter = Filter.Replace("karea", "c.karea")
            Filter = Filter.Replace("ksalesmannama", "c.ksalesmannama")
            Filter = Filter.Replace("ksalesmankode", "cs.kkode")
            Filter = Filter.Replace("ksalesman", "c.ksalesman")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_cd")
        'sql = "SELECT c.kid AS kid, c.kkode AS kkode, c.knama AS knama, c.kkategori AS kkategori, cc.ccnama AS kkategorinama, c.kcabang AS kcabang, c.klokasi AS klokasi, c.kgudang AS kgudang, c.kkategorisalesman AS kkategorisalesman, sc.scnama AS kkategorisalesmannama, c.karea AS karea, a.anama AS kareanama, c.kkategoricustomer AS kkategoricustomer, custc.ccnama AS kkategoricustomernama, c.kkategorisupplier AS kkategorisupplier, suppc.scnama AS kkategorisuppliernama, c.ksalesman AS ksalesman, cs.knama AS ksalesmannama, ca.kanama AS kkontakperson, c.kaktif AS kaktif, c.k1alamat1 AS k1alamat1, c.k1alamat2 AS k1alamat2, c.k1kota AS k1kota, c.k1propinsi AS k1propinsi, c.k1kodepos AS k1kodepos, c.k1negara AS k1negara, c.k1kontakperson AS k1kontakperson, c.k1notelp1 AS k1notelp1, c.k2alamat1 AS k2alamat1, c.k2alamat2 AS k2alamat2, c.k2propinsi AS k2propinsi, c.k2kota AS k2kota, c.k2kodepos AS k2kodepos, c.k2negara AS k2negara, c.k2kontakperson AS k2kontakperson, c.k2notelp1 AS k2notelp1, c.kterminbeli AS kterminbeli, c.kterminjual AS kterminjual, c.ktingkatjual AS ktingkatjual, c.kkomisipenjualan AS kkomisipenjualan, cs.kkode AS ksalesmankode, cp.cppoin, c.kpkp, c.knpwp from `m1_contact` `c` left join `m1_contact` `cs` on `c`.`ksalesman` = `cs`.`kid` left join `m1_contact_attention` `ca` on `c`.`kid` = `ca`.`kaidkontak` and `ca`.`kadefault` = 1 left join `m1_area` `a` on `c`.`karea` = `a`.`akode` left join `m1_contact_category` `cc` on `c`.`kkategori` = `cc`.`cckode` left join `m1_salesman_category` `sc` on `c`.`kkategorisalesman` = `sc`.`sckode` left join `m1_customer_category` `custc` on `c`.`kkategoricustomer` = `custc`.`cckode` left join `m1_supplier_category` `suppc` on `c`.`kkategorisupplier` = `suppc`.`sckode` left join m1_contact_point cp ON c.kid = cp.cpidkontak"
        sql = "SELECT c.kid AS kid, c.kkode AS kkode, c.knama AS knama, c.kkategori AS kkategori, cc.ccnama AS kkategorinama, c.kcabang AS kcabang, c.klokasi AS klokasi, c.kgudang AS kgudang, c.kkategorisalesman AS kkategorisalesman, sc.scnama AS kkategorisalesmannama, c.karea AS karea, a.anama AS kareanama, c.kkategoricustomer AS kkategoricustomer, custc.ccnama AS kkategoricustomernama, c.kkategorisupplier AS kkategorisupplier, suppc.scnama AS kkategorisuppliernama, c.ksalesman AS ksalesman, cs.knama AS ksalesmannama, ca.kanama AS kkontakperson, c.kaktif AS kaktif, c.k1alamat1 AS k1alamat1, c.k1alamat2 AS k1alamat2, c.k1kota AS k1kota, c.k1propinsi AS k1propinsi, c.k1kodepos AS k1kodepos, c.k1negara AS k1negara, c.k1kontakperson AS k1kontakperson, c.k1notelp1 AS k1notelp1, c.k2alamat1 AS k2alamat1, c.k2alamat2 AS k2alamat2, c.k2propinsi AS k2propinsi, c.k2kota AS k2kota, c.k2kodepos AS k2kodepos, c.k2negara AS k2negara, c.k2kontakperson AS k2kontakperson, c.k2notelp1 AS k2notelp1, c.kterminbeli AS kterminbeli, c.kterminjual AS kterminjual, c.ktingkatjual AS ktingkatjual, c.kkomisipenjualan AS kkomisipenjualan, cs.kkode AS ksalesmankode, cp.cppoin, c.kpkp, IFNULL(dcc.dccnilai,0) as dccnilai from `m1_contact` `c` join `m0_user` `u` on `u`.`userid` = 'valuserid' and  (`c`.`klokasi` = '' or `c`.`klokasi` = `u`.`ulokasi`) left join `m1_contact` `cs` on `c`.`ksalesman` = `cs`.`kid` left join `m1_contact_attention` `ca` on `c`.`kid` = `ca`.`kaidkontak` and `ca`.`kadefault` = 1 left join `m1_area` `a` on `c`.`karea` = `a`.`akode` left join `m1_contact_category` `cc` on `c`.`kkategori` = `cc`.`cckode` left join `m1_salesman_category` `sc` on `c`.`kkategorisalesman` = `sc`.`sckode` left join `m1_customer_category` `custc` on `c`.`kkategoricustomer` = `custc`.`cckode` left join `m1_supplier_category` `suppc` on `c`.`kkategorisupplier` = `suppc`.`sckode` left join m1_contact_point cp ON c.kid = cp.cpidkontak left join m1_location l on u.ulokasi = l.lkode left join m_12_pos_discount_category_customer dcc on l.lkategoripos = dcc.dcckategori and c.kkategoricustomer = dcc.dcckategoricustomer"
        If userid <> 0 Then
            sql = sql.Replace("= 'valuserid'", "= '" & userid & "'")
        Else
            sql = sql.Replace("= 'valuserid'", " LIKE '%'")
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "c.kid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kid"), 0), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptField,
                     FxDB(dr("kkategori"), ""), sptField,
                     FxDB(dr("kkategorinama"), ""), sptField,
                     FxDB(dr("kcabang"), ""), sptField,
                     FxDB(dr("klokasi"), ""), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("kkategorisalesman"), ""), sptField,
                     FxDB(dr("kkategorisalesmannama"), ""), sptField,
                     FxDB(dr("karea"), ""), sptField,
                     FxDB(dr("kareanama"), ""), sptField,
                     FxDB(dr("kkategoricustomer"), ""), sptField,
                     FxDB(dr("kkategoricustomernama"), ""), sptField,
                     FxDB(dr("kkategorisupplier"), ""), sptField,
                     FxDB(dr("kkategorisuppliernama"), ""), sptField,
                     FxDB(dr("ksalesman"), 0), sptField,
                     FxDB(dr("ksalesmannama"), ""), sptField,
                     FxDB(dr("kkontakperson"), ""), sptField,
                     FxDB(dr("kaktif"), 0), sptField,
                     FxDB(dr("k1alamat1"), ""), sptField,
                     FxDB(dr("k1alamat2"), ""), sptField,
                     FxDB(dr("k1kota"), ""), sptField,
                     FxDB(dr("k1propinsi"), ""), sptField,
                     FxDB(dr("k1kodepos"), ""), sptField,
                     FxDB(dr("k1negara"), ""), sptField,
                     FxDB(dr("k1kontakperson"), ""), sptField,
                     FxDB(dr("k1notelp1"), ""), sptField,
                     FxDB(dr("k2alamat1"), ""), sptField,
                     FxDB(dr("k2alamat2"), ""), sptField,
                     FxDB(dr("k2propinsi"), ""), sptField,
                     FxDB(dr("k2kota"), ""), sptField,
                     FxDB(dr("k2kodepos"), ""), sptField,
                     FxDB(dr("k2negara"), ""), sptField,
                     FxDB(dr("k2kontakperson"), ""), sptField,
                     FxDB(dr("k2notelp1"), ""), sptField,
                     FxDB(dr("kterminbeli"), ""), sptField,
                     FxDB(dr("kterminjual"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("ksalesmankode"), ""), sptField,
                     FxDB(dr("kkomisipenjualan"), 0), sptField,
                     FxDB(dr("cppoin"), 0), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("dccnilai"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Contact data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, klokasi, kgudang, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, ksalesman, ksalesmannama, kkontakperson, kaktif, k1alamat1, k1alamat2, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1notelp1, k2alamat1, k2alamat2, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2notelp1, kterminbeli, kterminjual, ktingkatjual, ksalesmankode, kkomisipenjualan, cppoin, kpkp, dccnilai"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactGetdataById(ByVal param As String) As String

        'M1_ContactGetdataById Utama --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, 
        'ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, 
        'k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, 
        'k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, 
        'k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, 
        'k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, 
        'k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, 
        'k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, 
        'k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, 
        'k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, 
        'kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, 
        'krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, 
        'knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, 
        'kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, 
        'kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom

        'M1_ContactGetdataById Detail -------------------------------------------------------
        'kaid, kaidkontak, kakodekontak, kanama, 
        'kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, 
        'kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, 
        'kamodifikasiuser, kamodifikasitgl

        'M1_ContactGetdataById Price --------------------------------------------------------
        'khidkontak, khidbarang, bkode, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, 
        'khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, 
        'khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, 
        'khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, 
        'khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5

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

        Dim utama As String = "", detail As String = "", price As String = "", commission As String = "", salesman As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "c1.kid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "c1.kid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_getdata")
        sql = "select `c1`.`kid` AS `kid`, `c1`.`kkode` AS `kkode`, `c1`.`knama` AS `knama`, `c1`.`kkategori` AS `kkategori`, `cc`.`ccnama` AS `kkategorinama`, `c1`.`kcabang` AS `kcabang`, `br`.`bnama` AS `kcabangnama`, `c1`.`klokasi` AS `klokasi`, `l`.`lnama` AS `klokasinama`, `c1`.`kgudang` AS `kgudang`, `w`.`wnama` AS `kgudangnama`, `c1`.`kkategorisalesman` AS `kkategorisalesman`, `sc`.`scnama` AS `kkategorisalesmannama`, `c1`.`karea` AS `karea`, `a`.`anama` AS `kareanama`, `c1`.`kkategoricustomer` AS `kkategoricustomer`, `cusc`.`ccnama` AS `kkategoricustomernama`, `c1`.`kkategorisupplier` AS `kkategorisupplier`, `suppc`.`scnama` AS `kkategorisuppliernama`, `c1`.`kdivisi` AS `kdivisi`, `d`.`dnama` AS `kdivisinama`, `c1`.`ksubdivisi` AS `ksubdivisi`, `sd`.`sdnama` AS `ksubdivisinama`, `c1`.`ksalesman` AS `ksalesman`, `c2`.`knama` AS `ksalesmannama`, `c1`.`kkontakperson` AS `kkontakperson`, `c1`.`kterminglobal` AS `kterminglobal`, `c1`.`kaktif` AS `kaktif`, `c1`.`kaktiftgl` AS `kaktiftgl`, `c1`.`k1alamat1` AS `k1alamat1`, `c1`.`k1alamat2` AS `k1alamat2`, `c1`.`k1alamat3` AS `k1alamat3`, `c1`.`k1alamat4` AS `k1alamat4`, `c1`.`k1alamat5` AS `k1alamat5`, `c1`.`k1kota` AS `k1kota`, `c1`.`k1propinsi` AS `k1propinsi`, `c1`.`k1kodepos` AS `k1kodepos`, `c1`.`k1negara` AS `k1negara`, `c1`.`k1kontakperson` AS `k1kontakperson`, `c1`.`k1kontaknohp` AS `k1kontaknohp`, `c1`.`k1kontakemail` AS `k1kontakemail`, `c1`.`k1notelp1` AS `k1notelp1`, `c1`.`k1notelp2` AS `k1notelp2`, `c1`.`k1nofax` AS `k1nofax`, `c1`.`k1email` AS `k1email`, `c1`.`k1website` AS `k1website`, `c1`.`k2alamat1` AS `k2alamat1`, `c1`.`k2alamat2` AS `k2alamat2`, `c1`.`k2alamat3` AS `k2alamat3`, `c1`.`k2alamat4` AS `k2alamat4`, `c1`.`k2alamat5` AS `k2alamat5`, `c1`.`k2propinsi` AS `k2propinsi`, `c1`.`k2kota` AS `k2kota`, `c1`.`k2kodepos` AS `k2kodepos`, `c1`.`k2negara` AS `k2negara`, `c1`.`k2kontakperson` AS `k2kontakperson`, `c1`.`k2kontaknohp` AS `k2kontaknohp`, `c1`.`k2kontakemail` AS `k2kontakemail`, `c1`.`k2notelp1` AS `k2notelp1`, `c1`.`k2notelp2` AS `k2notelp2`, `c1`.`k2nofax` AS `k2nofax`, `c1`.`k2email` AS `k2email`, `c1`.`k2website` AS `k2website`, `c1`.`k3alamat1` AS `k3alamat1`, `c1`.`k3alamat2` AS `k3alamat2`, `c1`.`k3alamat3` AS `k3alamat3`, `c1`.`k3alamat4` AS `k3alamat4`, `c1`.`k3alamat5` AS `k3alamat5`, `c1`.`k3kota` AS `k3kota`, `c1`.`k3propinsi` AS `k3propinsi`, `c1`.`k3kodepos` AS `k3kodepos`, `c1`.`k3negara` AS `k3negara`, `c1`.`k3kontakperson` AS `k3kontakperson`, `c1`.`k3kontaknohp` AS `k3kontaknohp`, `c1`.`k3kontakemail` AS `k3kontakemail`, `c1`.`k3notelp1` AS `k3notelp1`, `c1`.`k3notelp2` AS `k3notelp2`, `c1`.`k3nofax` AS `k3nofax`, `c1`.`k3email` AS `k3email`, `c1`.`k3website` AS `k3website`, `c1`.`k4alamat1` AS `k4alamat1`, `c1`.`k4alamat2` AS `k4alamat2`, `c1`.`k4alamat3` AS `k4alamat3`, `c1`.`k4alamat4` AS `k4alamat4`, `c1`.`k4alamat5` AS `k4alamat5`, `c1`.`k4kota` AS `k4kota`, `c1`.`k4propinsi` AS `k4propinsi`, `c1`.`k4kodepos` AS `k4kodepos`, `c1`.`k4negara` AS `k4negara`, `c1`.`k4kontakperson` AS `k4kontakperson`, `c1`.`k4kontaknohp` AS `k4kontaknohp`, `c1`.`k4kontakemail` AS `k4kontakemail`, `c1`.`k4notelp1` AS `k4notelp1`, `c1`.`k4notelp2` AS `k4notelp2`, `c1`.`k4nofax` AS `k4nofax`, `c1`.`k4email` AS `k4email`, `c1`.`k4website` AS `k4website`, `c1`.`knpwp` AS `knpwp`, `c1`.`kpkp` AS `kpkp`, `c1`.`kbatashutang` AS `kbatashutang`, `c1`.`kterminbeli` AS `kterminbeli`, `c1`.`krekhutang` AS `krekhutang`, `c1`.`kbagpembelian` AS `kbagpembelian`, `c1`.`kfobbeli` AS `kfobbeli`, `c1`.`kviabeli` AS `kviabeli`, `c1`.`kbataspiutang` AS `kbataspiutang`, `c1`.`kterminjual` AS `kterminjual`, `c1`.`krekpiutang` AS `krekpiutang`, `c1`.`kbagpenjualan` AS `kbagpenjualan`, `c1`.`ktingkatjual` AS `ktingkatjual`, `c1`.`kfobjual` AS `kfobjual`, `c1`.`kviajual` AS `kviajual`, `c1`.`ktglkontrak` AS `ktglkontrak`, `c1`.`kbank` AS `kbank`, `c1`.`knorekening` AS `knorekening`, `c1`.`kjeniskelamin` AS `kjeniskelamin`, `c1`.`kmatauang` AS `kmatauang`, `c1`.`ktgllahir` AS `ktgllahir`, `c1`.`ktglnikah` AS `ktglnikah`, `c1`.`kkomisipenjualan` AS `kkomisipenjualan`, `c1`.`kcatatan` AS `kcatatan`, `c1`.`kinputuser` AS `kinputuser`, `c1`.`kinputtgl` AS `kinputtgl`, `c1`.`kcustomtext1` AS `kcustomtext1`, `c1`.`kcustomtext2` AS `kcustomtext2`, `c1`.`kcustomtext3` AS `kcustomtext3`, `c1`.`kcustomtext4` AS `kcustomtext4`, `c1`.`kcustomtext5` AS `kcustomtext5`, `c1`.`kcustomtext6` AS `kcustomtext6`, `c1`.`kcustomtext7` AS `kcustomtext7`, `c1`.`kcustomtext8` AS `kcustomtext8`, `c1`.`kcustomtext9` AS `kcustomtext9`, `c1`.`kmodifikasiuser` AS `kmodifikasiuser`, `c1`.`kmodifikasitgl` AS `kmodifikasitgl`, `c1`.`kcustomtext10` AS `kcustomtext10`, `c1`.`kcustomint1` AS `kcustomint1`, `c1`.`kcustomint2` AS `kcustomint2`, `c1`.`kcustomint3` AS `kcustomint3`, `c1`.`kcustomdbl1` AS `kcustomdbl1`, `c1`.`kcustomdbl2` AS `kcustomdbl2`, `c1`.`kcustomdbl3` AS `kcustomdbl3`, `c1`.`kcustomdate1` AS `kcustomdate1`, `c1`.`kcustomdate2` AS `kcustomdate2`, `c1`.`kcustomdate3` AS `kcustomdate3`, `c2`.`kkode` AS `ksalesmankode`, `coa1`.`cnama` AS `krekhutangnama`, `c3`.`kkode` AS `kbagpembeliankode`, `c3`.`knama` AS `kbagpembeliannama`, `coa2`.`cnama` AS `krekpiutangnama`, `c4`.`kkode` AS `kbagpenjualankode`, `c4`.`knama` AS `kbagpenjualannama`, `b`.`bnama` AS `kbanknama`, `sr`.`nama` AS `ktingkatjualnama`, c1.kkomisikode, comm.kmnama as kkomisinama, `ca`.`kaid` AS `kaid`, `ca`.`kaidkontak` AS `kaidkontak`, `ca`.`kakodekontak` AS `kakodekontak`, `ca`.`kanama` AS `kanama`, `ca`.`kajabatan` AS `kajabatan`, `ca`.`kanotelp` AS `kanotelp`, `ca`.`kanofax` AS `kanofax`, `ca`.`kanohp` AS `kanohp`, `ca`.`kaemail` AS `kaemail`, `ca`.`kawebsite` AS `kawebsite`, `ca`.`kamessenger` AS `kamessenger`, `ca`.`kaalamat` AS `kaalamat`, `ca`.`katgllahir` AS `katgllahir`, `ca`.`katglnikah` AS `katglnikah`, `ca`.`kacatatan` AS `kacatatan`, `ca`.`kadefault` AS `kadefault`, `ca`.`kainputuser` AS `kainputuser`,  `ca`.`kainputtgl` AS `kainputtgl`,  `ca`.`kamodifikasiuser` AS `kamodifikasiuser`,  `ca`.`kamodifikasitgl` AS `kamodifikasitgl`, c1.khargacustom from `m1_contact` `c1`  left join `m1_contact` `c2` on `c1`.`ksalesman` = `c2`.`kid` left join `m1_coa` `coa1` on `c1`.`krekhutang` = `coa1`.`cnomor` left join `m1_contact` `c3` on `c1`.`kbagpembelian` = `c3`.`kid` left join `m1_coa` `coa2` on `c1`.`krekpiutang` = `coa2`.`cnomor` left join `m1_contact` `c4` on `c1`.`kbagpenjualan` = `c4`.`kid` left join `m1_bank` `b` on `c1`.`kbank` = `b`.`bkode` left join `m1_contact_attention` `ca` on `c1`.`kid` = `ca`.`kaidkontak` left join `m1_contact_category` `cc` on `c1`.`kkategori` = `cc`.`cckode` left join `m1_branch` `br` on `c1`.`kcabang` = `br`.`bkode` left join `m1_location` `l` on `c1`.`klokasi` = `l`.`lkode` left join `m1_warehouse` `w` on `c1`.`kgudang` = `w`.`wkode` left join `m1_salesman_category` `sc` on `c1`.`kkategorisalesman` = `sc`.`sckode` left join `m1_area` `a` on `c1`.`karea` = `a`.`akode` left join `m1_customer_category` `cusc` on `c1`.`kkategoricustomer` = `cusc`.`cckode` left join `m1_supplier_category` `suppc` on `c1`.`kkategorisupplier` = `suppc`.`sckode` left join `m1_division` `d` on `c1`.`kdivisi` = `d`.`dkode` left join `m1_subdivision` `sd` on `c1`.`ksubdivisi` = `sd`.`sdkode` left join `m0_selling_rate` `sr` on `c1`.`ktingkatjual` = `sr`.`kode` left join m1_commission comm on c1.kkomisikode = comm.kmkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kid"), 0), sptField,
                     FxDB(drutama("kkode"), ""), sptField,
                     FxDB(drutama("knama"), ""), sptField,
                     FxDB(drutama("kkategori"), ""), sptField,
                     FxDB(drutama("kkategorinama"), ""), sptField,
                     FxDB(drutama("kcabang"), ""), sptField,
                     FxDB(drutama("kcabangnama"), ""), sptField,
                     FxDB(drutama("klokasi"), ""), sptField,
                     FxDB(drutama("klokasinama"), ""), sptField,
                     FxDB(drutama("kgudang"), ""), sptField,
                     FxDB(drutama("kgudangnama"), ""), sptField,
                     FxDB(drutama("kkategorisalesman"), ""), sptField,
                     FxDB(drutama("kkategorisalesmannama"), ""), sptField,
                     FxDB(drutama("karea"), ""), sptField,
                     FxDB(drutama("kareanama"), ""), sptField,
                     FxDB(drutama("kkategoricustomer"), ""), sptField,
                     FxDB(drutama("kkategoricustomernama"), ""), sptField,
                     FxDB(drutama("kkategorisupplier"), ""), sptField,
                     FxDB(drutama("kkategorisuppliernama"), ""), sptField,
                     FxDB(drutama("kdivisi"), ""), sptField,
                     FxDB(drutama("kdivisinama"), ""), sptField,
                     FxDB(drutama("ksubdivisi"), ""), sptField,
                     FxDB(drutama("ksubdivisinama"), ""), sptField,
                     FxDB(drutama("ksalesman"), 0), sptField,
                     FxDB(drutama("ksalesmannama"), ""), sptField,
                     FxDB(drutama("kkontakperson"), ""), sptField,
                     FxDB(drutama("kterminglobal"), 0), sptField,
                     FxDB(drutama("kaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kaktiftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("k1alamat1"), ""), sptField,
                     FxDB(drutama("k1alamat2"), ""), sptField,
                     FxDB(drutama("k1alamat3"), ""), sptField,
                     FxDB(drutama("k1alamat4"), ""), sptField,
                     FxDB(drutama("k1alamat5"), ""), sptField,
                     FxDB(drutama("k1kota"), ""), sptField,
                     FxDB(drutama("k1propinsi"), ""), sptField,
                     FxDB(drutama("k1kodepos"), ""), sptField,
                     FxDB(drutama("k1negara"), ""), sptField,
                     FxDB(drutama("k1kontakperson"), ""), sptField,
                     FxDB(drutama("k1kontaknohp"), ""), sptField,
                     FxDB(drutama("k1kontakemail"), ""), sptField,
                     FxDB(drutama("k1notelp1"), ""), sptField,
                     FxDB(drutama("k1notelp2"), ""), sptField,
                     FxDB(drutama("k1nofax"), ""), sptField,
                     FxDB(drutama("k1email"), ""), sptField,
                     FxDB(drutama("k1website"), ""), sptField,
                     FxDB(drutama("k2alamat1"), ""), sptField,
                     FxDB(drutama("k2alamat2"), ""), sptField,
                     FxDB(drutama("k2alamat3"), ""), sptField,
                     FxDB(drutama("k2alamat4"), ""), sptField,
                     FxDB(drutama("k2alamat5"), ""), sptField,
                     FxDB(drutama("k2propinsi"), ""), sptField,
                     FxDB(drutama("k2kota"), ""), sptField,
                     FxDB(drutama("k2kodepos"), ""), sptField,
                     FxDB(drutama("k2negara"), ""), sptField,
                     FxDB(drutama("k2kontakperson"), ""), sptField,
                     FxDB(drutama("k2kontaknohp"), ""), sptField,
                     FxDB(drutama("k2kontakemail"), ""), sptField,
                     FxDB(drutama("k2notelp1"), ""), sptField,
                     FxDB(drutama("k2notelp2"), ""), sptField,
                     FxDB(drutama("k2nofax"), ""), sptField,
                     FxDB(drutama("k2email"), ""), sptField,
                     FxDB(drutama("k2website"), ""), sptField,
                     FxDB(drutama("k3alamat1"), ""), sptField,
                     FxDB(drutama("k3alamat2"), ""), sptField,
                     FxDB(drutama("k3alamat3"), ""), sptField,
                     FxDB(drutama("k3alamat4"), ""), sptField,
                     FxDB(drutama("k3alamat5"), ""), sptField,
                     FxDB(drutama("k3kota"), ""), sptField,
                     FxDB(drutama("k3propinsi"), ""), sptField,
                     FxDB(drutama("k3kodepos"), ""), sptField,
                     FxDB(drutama("k3negara"), ""), sptField,
                     FxDB(drutama("k3kontakperson"), ""), sptField,
                     FxDB(drutama("k3kontaknohp"), ""), sptField,
                     FxDB(drutama("k3kontakemail"), ""), sptField,
                     FxDB(drutama("k3notelp1"), ""), sptField,
                     FxDB(drutama("k3notelp2"), ""), sptField,
                     FxDB(drutama("k3nofax"), ""), sptField,
                     FxDB(drutama("k3email"), ""), sptField,
                     FxDB(drutama("k3website"), ""), sptField,
                     FxDB(drutama("k4alamat1"), ""), sptField,
                     FxDB(drutama("k4alamat2"), ""), sptField,
                     FxDB(drutama("k4alamat3"), ""), sptField,
                     FxDB(drutama("k4alamat4"), ""), sptField,
                     FxDB(drutama("k4alamat5"), ""), sptField,
                     FxDB(drutama("k4kota"), ""), sptField,
                     FxDB(drutama("k4propinsi"), ""), sptField,
                     FxDB(drutama("k4kodepos"), ""), sptField,
                     FxDB(drutama("k4negara"), ""), sptField,
                     FxDB(drutama("k4kontakperson"), ""), sptField,
                     FxDB(drutama("k4kontaknohp"), ""), sptField,
                     FxDB(drutama("k4kontakemail"), ""), sptField,
                     FxDB(drutama("k4notelp1"), ""), sptField,
                     FxDB(drutama("k4notelp2"), ""), sptField,
                     FxDB(drutama("k4nofax"), ""), sptField,
                     FxDB(drutama("k4email"), ""), sptField,
                     FxDB(drutama("k4website"), ""), sptField,
                     FxDB(drutama("knpwp"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("kbatashutang"), 0), sptField,
                     FxDB(drutama("kterminbeli"), ""), sptField,
                     FxDB(drutama("krekhutang"), ""), sptField,
                     FxDB(drutama("kbagpembelian"), 0), sptField,
                     FxDB(drutama("kfobbeli"), ""), sptField,
                     FxDB(drutama("kviabeli"), ""), sptField,
                     FxDB(drutama("kbataspiutang"), 0), sptField,
                     FxDB(drutama("kterminjual"), ""), sptField,
                     FxDB(drutama("krekpiutang"), ""), sptField,
                     FxDB(drutama("kbagpenjualan"), 0), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kfobjual"), ""), sptField,
                     FxDB(drutama("kviajual"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglkontrak"), ""), formatTgl), sptField,
                     FxDB(drutama("kbank"), ""), sptField,
                     FxDB(drutama("knorekening"), ""), sptField,
                     FxDB(drutama("kjeniskelamin"), 0), sptField,
                     FxDB(drutama("kmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglnikah"), ""), formatTgl), sptField,
                     FxDB(drutama("kkomisipenjualan"), 0), sptField,
                     FxDB(drutama("kcatatan"), ""), sptField,
                     FxDB(drutama("kinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext1"), ""), sptField,
                     FxDB(drutama("kcustomtext2"), ""), sptField,
                     FxDB(drutama("kcustomtext3"), ""), sptField,
                     FxDB(drutama("kcustomtext4"), ""), sptField,
                     FxDB(drutama("kcustomtext5"), ""), sptField,
                     FxDB(drutama("kcustomtext6"), ""), sptField,
                     FxDB(drutama("kcustomtext7"), ""), sptField,
                     FxDB(drutama("kcustomtext8"), ""), sptField,
                     FxDB(drutama("kcustomtext9"), ""), sptField,
                     FxDB(drutama("kmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext10"), ""), sptField,
                     FxDB(drutama("kcustomint1"), 0), sptField,
                     FxDB(drutama("kcustomint2"), 0), sptField,
                     FxDB(drutama("kcustomint3"), 0), sptField,
                     FxDB(drutama("kcustomdbl1"), 0), sptField,
                     FxDB(drutama("kcustomdbl2"), 0), sptField,
                     FxDB(drutama("kcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ksalesmankode"), ""), sptField,
                     FxDB(drutama("krekhutangnama"), ""), sptField,
                     FxDB(drutama("kbagpembeliankode"), ""), sptField,
                     FxDB(drutama("kbagpembeliannama"), ""), sptField,
                     FxDB(drutama("krekpiutangnama"), ""), sptField,
                     FxDB(drutama("kbagpenjualankode"), ""), sptField,
                     FxDB(drutama("kbagpenjualannama"), ""), sptField,
                     FxDB(drutama("kbanknama"), ""), sptField,
                     FxDB(drutama("ktingkatjualnama"), ""), sptField,
                     FxDB(drutama("kkomisikode"), ""), sptField,
                     FxDB(drutama("kkomisinama"), ""), sptField,
                     FxDB(drutama("khargacustom"), "0"))

            Dim tgllahir As String = "", tglnikah As String = "", tglinput As String = "", tglmodif As String = ""

            For Each dr As DataRow In dt.Rows

                'SET FORMAT TGL
                If Len(FxDB(dr("katgllahir"), "")) > 0 Then tgllahir = AsFormatTanggal(FxDB(dr("katgllahir"), ""), formatTgl)
                If Len(FxDB(dr("katglnikah"), "")) > 0 Then tglnikah = AsFormatTanggal(FxDB(dr("katglnikah"), ""), formatTgl)
                If Len(FxDB(dr("kainputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(dr("kainputtgl"), ""), formatTglWaktu)
                If Len(FxDB(dr("kamodifikasitgl"), "")) > 0 Then tglmodif = AsFormatTanggal(FxDB(dr("kamodifikasitgl"), ""), formatTglWaktu)

                detail = String.Concat(detail, FxDB(dr("kaid"), 0), sptField,
                     FxDB(dr("kaidkontak"), 0), sptField,
                     FxDB(dr("kakodekontak"), ""), sptField,
                     FxDB(dr("kanama"), ""), sptField,
                     FxDB(dr("kajabatan"), ""), sptField,
                     FxDB(dr("kanotelp"), ""), sptField,
                     FxDB(dr("kanofax"), ""), sptField,
                     FxDB(dr("kanohp"), ""), sptField,
                     FxDB(dr("kaemail"), ""), sptField,
                     FxDB(dr("kawebsite"), ""), sptField,
                     FxDB(dr("kamessenger"), ""), sptField,
                     FxDB(dr("kaalamat"), ""), sptField,
                     tgllahir, sptField,
                     tglnikah, sptField,
                     FxDB(dr("kacatatan"), ""), sptField,
                     FxDB(dr("kadefault"), 0), sptField,
                     FxDB(dr("kainputuser"), 0), sptField,
                     tglinput, sptField,
                     FxDB(dr("kamodifikasiuser"), 0), sptField,
                     tglmodif, sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA ITEM Price
            Dim dtPrice As New DataTable
            sql = "SELECT cp.khidkontak, cp.khidbarang, i.bkode, i.bnama, cp.khsatuan, cp.khkomisi, cp.khhargabeli, cp.khhargajual, cp.khberlakudari, cp.khberlakusampai, cp.khcatatan, cp.khinputuser, cp.khinputtgl, cp.khmodifikasiuser, cp.khmodifikasitgl, cp.khcustomtext1, cp.khcustomtext2, cp.khcustomtext3, cp.khcustomtext4, cp.khcustomtext5, cp.khcustomint1, cp.khcustomint2, cp.khcustomint3, cp.khcustomint4, cp.khcustomint5, cp.khcustomdbl1, cp.khcustomdbl2, cp.khcustomdbl3, cp.khcustomdbl4, cp.khcustomdbl5, cp.khcustomdate1, cp.khcustomdate2, cp.khcustomdate3, cp.khcustomdate4, cp.khcustomdate5 FROM m1_contact_price cp JOIN m1_contact c ON cp.khidkontak = c.kid AND cp.khidkontak = '" & FixDouble(idtransaksi) & "' JOIN m1_item i ON cp.khidbarang = i.bid"
            dtPrice = AmbilData("aplikasi1-M1_Item_Price", "", "cp.khidkontak, i.bkode", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtPrice.Rows
                price = String.Concat(price,
                     FxDB(dr("khidkontak"), ""), sptField,
                     FxDB(dr("khidbarang"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("khsatuan"), ""), sptField,
                     FxDB(dr("khkomisi"), 0), sptField,
                     FxDB(dr("khhargabeli"), 0), sptField,
                     FxDB(dr("khhargajual"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakudari"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("khcatatan"), ""), sptField,
                     FxDB(dr("khinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khcustomtext1"), ""), sptField,
                     FxDB(dr("khcustomtext2"), ""), sptField,
                     FxDB(dr("khcustomtext3"), ""), sptField,
                     FxDB(dr("khcustomtext4"), ""), sptField,
                     FxDB(dr("khcustomtext5"), ""), sptField,
                     FxDB(dr("khcustomint1"), 0), sptField,
                     FxDB(dr("khcustomint2"), 0), sptField,
                     FxDB(dr("khcustomint3"), 0), sptField,
                     FxDB(dr("khcustomint4"), 0), sptField,
                     FxDB(dr("khcustomint5"), 0), sptField,
                     FxDB(dr("khcustomdbl1"), 0), sptField,
                     FxDB(dr("khcustomdbl2"), 0), sptField,
                     FxDB(dr("khcustomdbl3"), 0), sptField,
                     FxDB(dr("khcustomdbl4"), 0), sptField,
                     FxDB(dr("khcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate5"), ""), formatTgl), sptRow)
            Next
            If price.Length > 0 Then price = price.Substring(0, price.Length - sptRow.Length) Else price = price

            'AMBIL DATA ITEM Price
            Dim dtCommission As New DataTable
            sql = "SELECT * FROM m1_salesman_commission where scidkontak = '" & FixDouble(idtransaksi) & "'"
            dtCommission = AmbilData("aplikasi1-M1_Item_Price", "", "scidkontak", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtCommission.Rows
                commission = String.Concat(commission,
                     FxDB(dr("scidkontak"), ""), sptField,
                     FxDB(dr("sckomisi1"), 0), sptField,
                     FxDB(dr("sckomisi2"), 0), sptField,
                     FxDB(dr("sckomisi3"), 0), sptField,
                     FxDB(dr("sckomisi4"), 0), sptField,
                     FxDB(dr("sckomisi5"), 0), sptField,
                     FxDB(dr("sckomisi6"), 0), sptField,
                     FxDB(dr("sckomisi7"), 0), sptField,
                     FxDB(dr("sckomisi8"), 0), sptField,
                     FxDB(dr("sckomisi9"), 0), sptField,
                     FxDB(dr("sckomisi10"), 0), sptField,
                     FxDB(dr("sccustomtext1"), ""), sptField,
                     FxDB(dr("sccustomtext2"), ""), sptField,
                     FxDB(dr("sccustomtext3"), ""), sptField,
                     FxDB(dr("sccustomtext4"), ""), sptField,
                     FxDB(dr("sccustomtext5"), ""), sptField,
                     FxDB(dr("sccustomtext6"), ""), sptField,
                     FxDB(dr("sccustomtext7"), ""), sptField,
                     FxDB(dr("sccustomtext8"), ""), sptField,
                     FxDB(dr("sccustomtext9"), ""), sptField,
                     FxDB(dr("sccustomtext10"), ""), sptField,
                     FxDB(dr("sccustomint1"), 0), sptField,
                     FxDB(dr("sccustomint2"), 0), sptField,
                     FxDB(dr("sccustomint3"), 0), sptField,
                     FxDB(dr("sccustomint4"), 0), sptField,
                     FxDB(dr("sccustomint5"), 0), sptField,
                     FxDB(dr("sccustomint6"), 0), sptField,
                     FxDB(dr("sccustomint7"), 0), sptField,
                     FxDB(dr("sccustomint8"), 0), sptField,
                     FxDB(dr("sccustomint9"), 0), sptField,
                     FxDB(dr("sccustomint10"), 0), sptField,
                     FxDB(dr("sccustomdbl1"), 0), sptField,
                     FxDB(dr("sccustomdbl2"), 0), sptField,
                     FxDB(dr("sccustomdbl3"), 0), sptField,
                     FxDB(dr("sccustomdbl4"), 0), sptField,
                     FxDB(dr("sccustomdbl5"), 0), sptField,
                     FxDB(dr("sccustomdbl6"), 0), sptField,
                     FxDB(dr("sccustomdbl7"), 0), sptField,
                     FxDB(dr("sccustomdbl8"), 0), sptField,
                     FxDB(dr("sccustomdbl9"), 0), sptField,
                     FxDB(dr("sccustomdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate10"), ""), formatTgl), sptRow)
            Next
            If commission.Length > 0 Then commission = commission.Substring(0, commission.Length - sptRow.Length) Else commission = commission

            'AMBIL DATA ITEM Price
            Dim dtSalesman As New DataTable
            sql = "SELECT cs.*, c.kkode as kodesalesman, c.knama as namasalesman, sc.sckomisi1, sc.sccustomint1 FROM m1_contact_salesman cs join m1_contact c on cs.csidsalesman = c.kid and kkategori = 'M' left join m1_salesman_commission sc on cs.csidsalesman = sc.scidkontak where csidkontak = '" & FixDouble(idtransaksi) & "'"
            dtSalesman = AmbilData("aplikasi1-M1_Item_Price", "", "csidkontak", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtSalesman.Rows
                salesman = String.Concat(salesman,
                    FxDB(dr("csidsalesman"), ""), sptField,
                     FxDB(dr("csidkontak"), ""), sptField,
                     FxDB(dr("csjeniskomisi"), ""), sptField,
                     FxDB(dr("cskomisi"), 0), sptField,
                     FxDB(dr("cscustomtext1"), ""), sptField,
                     FxDB(dr("cscustomtext2"), ""), sptField,
                     FxDB(dr("cscustomtext3"), ""), sptField,
                     FxDB(dr("cscustomtext4"), ""), sptField,
                     FxDB(dr("cscustomtext5"), ""), sptField,
                     FxDB(dr("cscustomint1"), 0), sptField,
                     FxDB(dr("cscustomint2"), 0), sptField,
                     FxDB(dr("cscustomint3"), 0), sptField,
                     FxDB(dr("cscustomint4"), 0), sptField,
                     FxDB(dr("cscustomint5"), 0), sptField,
                     FxDB(dr("cscustomdbl1"), 0), sptField,
                     FxDB(dr("cscustomdbl2"), 0), sptField,
                     FxDB(dr("cscustomdbl3"), 0), sptField,
                     FxDB(dr("cscustomdbl4"), 0), sptField,
                     FxDB(dr("cscustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cscustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cscustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cscustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("kodesalesman"), ""), sptField,
                     FxDB(dr("namasalesman"), ""), sptField,
                     FxDB(dr("sckomisi1"), 0), sptField,
                     FxDB(dr("sccustomint1"), 0), sptRow)
            Next
            If salesman.Length > 0 Then salesman = salesman.Substring(0, salesman.Length - sptRow.Length) Else salesman = salesman

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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, price, sptSubParam, commission, sptSubParam, salesman)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom" & sptSubParam & "kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl" & sptSubParam & "khidkontak, khidbarang, bkode, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5" & sptSubParam & "scidkontak, sckomisi1,sckomisi2,sckomisi3,sckomisi4,sckomisi5,sckomisi6,sckomisi7,sckomisi8,sckomisi9,sckomisi10, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomtext6,sccustomtext7,sccustomtext8,sccustomtext9,sccustomtext10,sccustomint1, sccustomint2, sccustomint3, sccustomint4, sccustomint5,sccustomint6,sccustomint7,sccustomint8,sccustomint9,sccustomint10, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdbl4, sccustomdbl5,sccustomdbl6,sccustomdbl7,sccustomdbl8,sccustomdbl9,sccustomdbl10, sccustomdate1, sccustomdate2, sccustomdate3, sccustomdate4, sccustomdate5,sccustomdate6,sccustomdate7,sccustomdate8,sccustomdate9,sccustomdate10" & sptSubParam & "csidsalesman, csidkontak, csjeniskomisi, cskomisi, cscustomtext1, cscustomtext2, cscustomtext3, cscustomtext4, cscustomtext5,cscustomint1, cscustomint2, cscustomint3, cscustomint4, cscustomint5, cscustomdbl1, cscustomdbl2, cscustomdbl3, cscustomdbl4, cscustomdbl5, cscustomdate1, cscustomdate2, cscustomdate3, cscustomdate4, cscustomdate5, kodesalesman, namasalesman, sckomisi1, sccustomint1"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactGetdataByIdSerenity(ByVal param As String) As String

        'M1_ContactGetdataById Utama --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, 
        'ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, 
        'k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, 
        'k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, 
        'k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, 
        'k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, 
        'k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, 
        'k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, 
        'k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, 
        'k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, 
        'kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, 
        'krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, 
        'knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, 
        'kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, 
        'kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom

        'M1_ContactGetdataById Detail -------------------------------------------------------
        'kaid, kaidkontak, kakodekontak, kanama, 
        'kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, 
        'kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, 
        'kamodifikasiuser, kamodifikasitgl

        'M1_ContactGetdataById Price --------------------------------------------------------
        'khidkontak, khidbarang, bkode, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, 
        'khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, 
        'khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, 
        'khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, 
        'khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5

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

        Dim utama As String = "", detail As String = "", price As String = "", commission As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M2_Aj~M2_Aj_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "c1.kid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "c1.kid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_contact_getdata")
        sql = "select `c1`.`kid` AS `kid`, `c1`.`kkode` AS `kkode`, `c1`.`knama` AS `knama`, `c1`.`kkategori` AS `kkategori`, `cc`.`ccnama` AS `kkategorinama`, `c1`.`kcabang` AS `kcabang`, `br`.`bnama` AS `kcabangnama`, `c1`.`klokasi` AS `klokasi`, `l`.`lnama` AS `klokasinama`, `c1`.`kgudang` AS `kgudang`, `w`.`wnama` AS `kgudangnama`, `c1`.`kkategorisalesman` AS `kkategorisalesman`, `sc`.`scnama` AS `kkategorisalesmannama`, `c1`.`karea` AS `karea`, `a`.`anama` AS `kareanama`, `c1`.`kkategoricustomer` AS `kkategoricustomer`, `cusc`.`ccnama` AS `kkategoricustomernama`, `c1`.`kkategorisupplier` AS `kkategorisupplier`, `suppc`.`scnama` AS `kkategorisuppliernama`, `c1`.`kdivisi` AS `kdivisi`, `d`.`dnama` AS `kdivisinama`, `c1`.`ksubdivisi` AS `ksubdivisi`, `sd`.`sdnama` AS `ksubdivisinama`, `c1`.`ksalesman` AS `ksalesman`, `c2`.`knama` AS `ksalesmannama`, `c1`.`kkontakperson` AS `kkontakperson`, `c1`.`kterminglobal` AS `kterminglobal`, `c1`.`kaktif` AS `kaktif`, `c1`.`kaktiftgl` AS `kaktiftgl`, `c1`.`k1alamat1` AS `k1alamat1`, `c1`.`k1alamat2` AS `k1alamat2`, `c1`.`k1alamat3` AS `k1alamat3`, `c1`.`k1alamat4` AS `k1alamat4`, `c1`.`k1alamat5` AS `k1alamat5`, `c1`.`k1kota` AS `k1kota`, `c1`.`k1propinsi` AS `k1propinsi`, `c1`.`k1kodepos` AS `k1kodepos`, `c1`.`k1negara` AS `k1negara`, `c1`.`k1kontakperson` AS `k1kontakperson`, `c1`.`k1kontaknohp` AS `k1kontaknohp`, `c1`.`k1kontakemail` AS `k1kontakemail`, `c1`.`k1notelp1` AS `k1notelp1`, `c1`.`k1notelp2` AS `k1notelp2`, `c1`.`k1nofax` AS `k1nofax`, `c1`.`k1email` AS `k1email`, `c1`.`k1website` AS `k1website`, `c1`.`k2alamat1` AS `k2alamat1`, `c1`.`k2alamat2` AS `k2alamat2`, `c1`.`k2alamat3` AS `k2alamat3`, `c1`.`k2alamat4` AS `k2alamat4`, `c1`.`k2alamat5` AS `k2alamat5`, `c1`.`k2propinsi` AS `k2propinsi`, `c1`.`k2kota` AS `k2kota`, `c1`.`k2kodepos` AS `k2kodepos`, `c1`.`k2negara` AS `k2negara`, `c1`.`k2kontakperson` AS `k2kontakperson`, `c1`.`k2kontaknohp` AS `k2kontaknohp`, `c1`.`k2kontakemail` AS `k2kontakemail`, `c1`.`k2notelp1` AS `k2notelp1`, `c1`.`k2notelp2` AS `k2notelp2`, `c1`.`k2nofax` AS `k2nofax`, `c1`.`k2email` AS `k2email`, `c1`.`k2website` AS `k2website`, `c1`.`k3alamat1` AS `k3alamat1`, `c1`.`k3alamat2` AS `k3alamat2`, `c1`.`k3alamat3` AS `k3alamat3`, `c1`.`k3alamat4` AS `k3alamat4`, `c1`.`k3alamat5` AS `k3alamat5`, `c1`.`k3kota` AS `k3kota`, `c1`.`k3propinsi` AS `k3propinsi`, `c1`.`k3kodepos` AS `k3kodepos`, `c1`.`k3negara` AS `k3negara`, `c1`.`k3kontakperson` AS `k3kontakperson`, `c1`.`k3kontaknohp` AS `k3kontaknohp`, `c1`.`k3kontakemail` AS `k3kontakemail`, `c1`.`k3notelp1` AS `k3notelp1`, `c1`.`k3notelp2` AS `k3notelp2`, `c1`.`k3nofax` AS `k3nofax`, `c1`.`k3email` AS `k3email`, `c1`.`k3website` AS `k3website`, `c1`.`k4alamat1` AS `k4alamat1`, `c1`.`k4alamat2` AS `k4alamat2`, `c1`.`k4alamat3` AS `k4alamat3`, `c1`.`k4alamat4` AS `k4alamat4`, `c1`.`k4alamat5` AS `k4alamat5`, `c1`.`k4kota` AS `k4kota`, `c1`.`k4propinsi` AS `k4propinsi`, `c1`.`k4kodepos` AS `k4kodepos`, `c1`.`k4negara` AS `k4negara`, `c1`.`k4kontakperson` AS `k4kontakperson`, `c1`.`k4kontaknohp` AS `k4kontaknohp`, `c1`.`k4kontakemail` AS `k4kontakemail`, `c1`.`k4notelp1` AS `k4notelp1`, `c1`.`k4notelp2` AS `k4notelp2`, `c1`.`k4nofax` AS `k4nofax`, `c1`.`k4email` AS `k4email`, `c1`.`k4website` AS `k4website`, `c1`.`knpwp` AS `knpwp`, `c1`.`kpkp` AS `kpkp`, `c1`.`kbatashutang` AS `kbatashutang`, `c1`.`kterminbeli` AS `kterminbeli`, `c1`.`krekhutang` AS `krekhutang`, `c1`.`kbagpembelian` AS `kbagpembelian`, `c1`.`kfobbeli` AS `kfobbeli`, `c1`.`kviabeli` AS `kviabeli`, `c1`.`kbataspiutang` AS `kbataspiutang`, `c1`.`kterminjual` AS `kterminjual`, `c1`.`krekpiutang` AS `krekpiutang`, `c1`.`kbagpenjualan` AS `kbagpenjualan`, `c1`.`ktingkatjual` AS `ktingkatjual`, `c1`.`kfobjual` AS `kfobjual`, `c1`.`kviajual` AS `kviajual`, `c1`.`ktglkontrak` AS `ktglkontrak`, `c1`.`kbank` AS `kbank`, `c1`.`knorekening` AS `knorekening`, `c1`.`kjeniskelamin` AS `kjeniskelamin`, `c1`.`kmatauang` AS `kmatauang`, `c1`.`ktgllahir` AS `ktgllahir`, `c1`.`ktglnikah` AS `ktglnikah`, `c1`.`kkomisipenjualan` AS `kkomisipenjualan`, `c1`.`kcatatan` AS `kcatatan`, `c1`.`kinputuser` AS `kinputuser`, `c1`.`kinputtgl` AS `kinputtgl`, `c1`.`kcustomtext1` AS `kcustomtext1`, `c1`.`kcustomtext2` AS `kcustomtext2`, `c1`.`kcustomtext3` AS `kcustomtext3`, `c1`.`kcustomtext4` AS `kcustomtext4`, `c1`.`kcustomtext5` AS `kcustomtext5`, `c1`.`kcustomtext6` AS `kcustomtext6`, `c1`.`kcustomtext7` AS `kcustomtext7`, `c1`.`kcustomtext8` AS `kcustomtext8`, `c1`.`kcustomtext9` AS `kcustomtext9`, `c1`.`kmodifikasiuser` AS `kmodifikasiuser`, `c1`.`kmodifikasitgl` AS `kmodifikasitgl`, `c1`.`kcustomtext10` AS `kcustomtext10`, `c1`.`kcustomint1` AS `kcustomint1`, `c1`.`kcustomint2` AS `kcustomint2`, `c1`.`kcustomint3` AS `kcustomint3`, `c1`.`kcustomdbl1` AS `kcustomdbl1`, `c1`.`kcustomdbl2` AS `kcustomdbl2`, `c1`.`kcustomdbl3` AS `kcustomdbl3`, `c1`.`kcustomdate1` AS `kcustomdate1`, `c1`.`kcustomdate2` AS `kcustomdate2`, `c1`.`kcustomdate3` AS `kcustomdate3`, `c2`.`kkode` AS `ksalesmankode`, `coa1`.`cnama` AS `krekhutangnama`, `c3`.`kkode` AS `kbagpembeliankode`, `c3`.`knama` AS `kbagpembeliannama`, `coa2`.`cnama` AS `krekpiutangnama`, `c4`.`kkode` AS `kbagpenjualankode`, `c4`.`knama` AS `kbagpenjualannama`, `b`.`bnama` AS `kbanknama`, `sr`.`nama` AS `ktingkatjualnama`, c1.kkomisikode, comm.kmnama as kkomisinama, `ca`.`kaid` AS `kaid`, `ca`.`kaidkontak` AS `kaidkontak`, `ca`.`kakodekontak` AS `kakodekontak`, `ca`.`kanama` AS `kanama`, `ca`.`kajabatan` AS `kajabatan`, `ca`.`kanotelp` AS `kanotelp`, `ca`.`kanofax` AS `kanofax`, `ca`.`kanohp` AS `kanohp`, `ca`.`kaemail` AS `kaemail`, `ca`.`kawebsite` AS `kawebsite`, `ca`.`kamessenger` AS `kamessenger`, `ca`.`kaalamat` AS `kaalamat`, `ca`.`katgllahir` AS `katgllahir`, `ca`.`katglnikah` AS `katglnikah`, `ca`.`kacatatan` AS `kacatatan`, `ca`.`kadefault` AS `kadefault`, `ca`.`kainputuser` AS `kainputuser`,  `ca`.`kainputtgl` AS `kainputtgl`,  `ca`.`kamodifikasiuser` AS `kamodifikasiuser`,  `ca`.`kamodifikasitgl` AS `kamodifikasitgl`, c1.khargacustom from `m1_contact` `c1`  left join `m1_contact` `c2` on `c1`.`ksalesman` = `c2`.`kid` left join `m1_coa` `coa1` on `c1`.`krekhutang` = `coa1`.`cnomor` left join `m1_contact` `c3` on `c1`.`kbagpembelian` = `c3`.`kid` left join `m1_coa` `coa2` on `c1`.`krekpiutang` = `coa2`.`cnomor` left join `m1_contact` `c4` on `c1`.`kbagpenjualan` = `c4`.`kid` left join `m1_bank` `b` on `c1`.`kbank` = `b`.`bkode` left join `m1_contact_attention` `ca` on `c1`.`kid` = `ca`.`kaidkontak` left join `m1_contact_category` `cc` on `c1`.`kkategori` = `cc`.`cckode` left join `m1_branch` `br` on `c1`.`kcabang` = `br`.`bkode` left join `m1_location` `l` on `c1`.`klokasi` = `l`.`lkode` left join `m1_warehouse` `w` on `c1`.`kgudang` = `w`.`wkode` left join `m1_salesman_category` `sc` on `c1`.`kkategorisalesman` = `sc`.`sckode` left join `m1_area` `a` on `c1`.`karea` = `a`.`akode` left join `m1_customer_category` `cusc` on `c1`.`kkategoricustomer` = `cusc`.`cckode` left join `m1_supplier_category` `suppc` on `c1`.`kkategorisupplier` = `suppc`.`sckode` left join `m1_division` `d` on `c1`.`kdivisi` = `d`.`dkode` left join `m1_subdivision` `sd` on `c1`.`ksubdivisi` = `sd`.`sdkode` left join `m0_selling_rate` `sr` on `c1`.`ktingkatjual` = `sr`.`kode` left join m1_commission comm on c1.kkomisikode = comm.kmkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("kid"), 0), sptField,
                     FxDB(drutama("kkode"), ""), sptField,
                     FxDB(drutama("knama"), ""), sptField,
                     FxDB(drutama("kkategori"), ""), sptField,
                     FxDB(drutama("kkategorinama"), ""), sptField,
                     FxDB(drutama("kcabang"), ""), sptField,
                     FxDB(drutama("kcabangnama"), ""), sptField,
                     FxDB(drutama("klokasi"), ""), sptField,
                     FxDB(drutama("klokasinama"), ""), sptField,
                     FxDB(drutama("kgudang"), ""), sptField,
                     FxDB(drutama("kgudangnama"), ""), sptField,
                     FxDB(drutama("kkategorisalesman"), ""), sptField,
                     FxDB(drutama("kkategorisalesmannama"), ""), sptField,
                     FxDB(drutama("karea"), ""), sptField,
                     FxDB(drutama("kareanama"), ""), sptField,
                     FxDB(drutama("kkategoricustomer"), ""), sptField,
                     FxDB(drutama("kkategoricustomernama"), ""), sptField,
                     FxDB(drutama("kkategorisupplier"), ""), sptField,
                     FxDB(drutama("kkategorisuppliernama"), ""), sptField,
                     FxDB(drutama("kdivisi"), ""), sptField,
                     FxDB(drutama("kdivisinama"), ""), sptField,
                     FxDB(drutama("ksubdivisi"), ""), sptField,
                     FxDB(drutama("ksubdivisinama"), ""), sptField,
                     FxDB(drutama("ksalesman"), 0), sptField,
                     FxDB(drutama("ksalesmannama"), ""), sptField,
                     FxDB(drutama("kkontakperson"), ""), sptField,
                     FxDB(drutama("kterminglobal"), 0), sptField,
                     FxDB(drutama("kaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kaktiftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("k1alamat1"), ""), sptField,
                     FxDB(drutama("k1alamat2"), ""), sptField,
                     FxDB(drutama("k1alamat3"), ""), sptField,
                     FxDB(drutama("k1alamat4"), ""), sptField,
                     FxDB(drutama("k1alamat5"), ""), sptField,
                     FxDB(drutama("k1kota"), ""), sptField,
                     FxDB(drutama("k1propinsi"), ""), sptField,
                     FxDB(drutama("k1kodepos"), ""), sptField,
                     FxDB(drutama("k1negara"), ""), sptField,
                     FxDB(drutama("k1kontakperson"), ""), sptField,
                     FxDB(drutama("k1kontaknohp"), ""), sptField,
                     FxDB(drutama("k1kontakemail"), ""), sptField,
                     FxDB(drutama("k1notelp1"), ""), sptField,
                     FxDB(drutama("k1notelp2"), ""), sptField,
                     FxDB(drutama("k1nofax"), ""), sptField,
                     FxDB(drutama("k1email"), ""), sptField,
                     FxDB(drutama("k1website"), ""), sptField,
                     FxDB(drutama("k2alamat1"), ""), sptField,
                     FxDB(drutama("k2alamat2"), ""), sptField,
                     FxDB(drutama("k2alamat3"), ""), sptField,
                     FxDB(drutama("k2alamat4"), ""), sptField,
                     FxDB(drutama("k2alamat5"), ""), sptField,
                     FxDB(drutama("k2propinsi"), ""), sptField,
                     FxDB(drutama("k2kota"), ""), sptField,
                     FxDB(drutama("k2kodepos"), ""), sptField,
                     FxDB(drutama("k2negara"), ""), sptField,
                     FxDB(drutama("k2kontakperson"), ""), sptField,
                     FxDB(drutama("k2kontaknohp"), ""), sptField,
                     FxDB(drutama("k2kontakemail"), ""), sptField,
                     FxDB(drutama("k2notelp1"), ""), sptField,
                     FxDB(drutama("k2notelp2"), ""), sptField,
                     FxDB(drutama("k2nofax"), ""), sptField,
                     FxDB(drutama("k2email"), ""), sptField,
                     FxDB(drutama("k2website"), ""), sptField,
                     FxDB(drutama("k3alamat1"), ""), sptField,
                     FxDB(drutama("k3alamat2"), ""), sptField,
                     FxDB(drutama("k3alamat3"), ""), sptField,
                     FxDB(drutama("k3alamat4"), ""), sptField,
                     FxDB(drutama("k3alamat5"), ""), sptField,
                     FxDB(drutama("k3kota"), ""), sptField,
                     FxDB(drutama("k3propinsi"), ""), sptField,
                     FxDB(drutama("k3kodepos"), ""), sptField,
                     FxDB(drutama("k3negara"), ""), sptField,
                     FxDB(drutama("k3kontakperson"), ""), sptField,
                     FxDB(drutama("k3kontaknohp"), ""), sptField,
                     FxDB(drutama("k3kontakemail"), ""), sptField,
                     FxDB(drutama("k3notelp1"), ""), sptField,
                     FxDB(drutama("k3notelp2"), ""), sptField,
                     FxDB(drutama("k3nofax"), ""), sptField,
                     FxDB(drutama("k3email"), ""), sptField,
                     FxDB(drutama("k3website"), ""), sptField,
                     FxDB(drutama("k4alamat1"), ""), sptField,
                     FxDB(drutama("k4alamat2"), ""), sptField,
                     FxDB(drutama("k4alamat3"), ""), sptField,
                     FxDB(drutama("k4alamat4"), ""), sptField,
                     FxDB(drutama("k4alamat5"), ""), sptField,
                     FxDB(drutama("k4kota"), ""), sptField,
                     FxDB(drutama("k4propinsi"), ""), sptField,
                     FxDB(drutama("k4kodepos"), ""), sptField,
                     FxDB(drutama("k4negara"), ""), sptField,
                     FxDB(drutama("k4kontakperson"), ""), sptField,
                     FxDB(drutama("k4kontaknohp"), ""), sptField,
                     FxDB(drutama("k4kontakemail"), ""), sptField,
                     FxDB(drutama("k4notelp1"), ""), sptField,
                     FxDB(drutama("k4notelp2"), ""), sptField,
                     FxDB(drutama("k4nofax"), ""), sptField,
                     FxDB(drutama("k4email"), ""), sptField,
                     FxDB(drutama("k4website"), ""), sptField,
                     FxDB(drutama("knpwp"), ""), sptField,
                     FxDB(drutama("kpkp"), 0), sptField,
                     FxDB(drutama("kbatashutang"), 0), sptField,
                     FxDB(drutama("kterminbeli"), ""), sptField,
                     FxDB(drutama("krekhutang"), ""), sptField,
                     FxDB(drutama("kbagpembelian"), 0), sptField,
                     FxDB(drutama("kfobbeli"), ""), sptField,
                     FxDB(drutama("kviabeli"), ""), sptField,
                     FxDB(drutama("kbataspiutang"), 0), sptField,
                     FxDB(drutama("kterminjual"), ""), sptField,
                     FxDB(drutama("krekpiutang"), ""), sptField,
                     FxDB(drutama("kbagpenjualan"), 0), sptField,
                     FxDB(drutama("ktingkatjual"), 0), sptField,
                     FxDB(drutama("kfobjual"), ""), sptField,
                     FxDB(drutama("kviajual"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglkontrak"), ""), formatTgl), sptField,
                     FxDB(drutama("kbank"), ""), sptField,
                     FxDB(drutama("knorekening"), ""), sptField,
                     FxDB(drutama("kjeniskelamin"), 0), sptField,
                     FxDB(drutama("kmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(drutama("ktgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("ktglnikah"), ""), formatTgl), sptField,
                     FxDB(drutama("kkomisipenjualan"), 0), sptField,
                     FxDB(drutama("kcatatan"), ""), sptField,
                     FxDB(drutama("kinputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext1"), ""), sptField,
                     FxDB(drutama("kcustomtext2"), ""), sptField,
                     FxDB(drutama("kcustomtext3"), ""), sptField,
                     FxDB(drutama("kcustomtext4"), ""), sptField,
                     FxDB(drutama("kcustomtext5"), ""), sptField,
                     FxDB(drutama("kcustomtext6"), ""), sptField,
                     FxDB(drutama("kcustomtext7"), ""), sptField,
                     FxDB(drutama("kcustomtext8"), ""), sptField,
                     FxDB(drutama("kcustomtext9"), ""), sptField,
                     FxDB(drutama("kmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("kcustomtext10"), ""), sptField,
                     FxDB(drutama("kcustomint1"), 0), sptField,
                     FxDB(drutama("kcustomint2"), 0), sptField,
                     FxDB(drutama("kcustomint3"), 0), sptField,
                     FxDB(drutama("kcustomdbl1"), 0), sptField,
                     FxDB(drutama("kcustomdbl2"), 0), sptField,
                     FxDB(drutama("kcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(drutama("kcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("ksalesmankode"), ""), sptField,
                     FxDB(drutama("krekhutangnama"), ""), sptField,
                     FxDB(drutama("kbagpembeliankode"), ""), sptField,
                     FxDB(drutama("kbagpembeliannama"), ""), sptField,
                     FxDB(drutama("krekpiutangnama"), ""), sptField,
                     FxDB(drutama("kbagpenjualankode"), ""), sptField,
                     FxDB(drutama("kbagpenjualannama"), ""), sptField,
                     FxDB(drutama("kbanknama"), ""), sptField,
                     FxDB(drutama("ktingkatjualnama"), ""), sptField,
                     FxDB(drutama("kkomisikode"), ""), sptField,
                     FxDB(drutama("kkomisinama"), ""), sptField,
                     FxDB(drutama("khargacustom"), "0"))

            Dim tgllahir As String = "", tglnikah As String = "", tglinput As String = "", tglmodif As String = ""

            For Each dr As DataRow In dt.Rows

                'SET FORMAT TGL
                If Len(FxDB(dr("katgllahir"), "")) > 0 Then tgllahir = AsFormatTanggal(FxDB(dr("katgllahir"), ""), formatTgl)
                If Len(FxDB(dr("katglnikah"), "")) > 0 Then tglnikah = AsFormatTanggal(FxDB(dr("katglnikah"), ""), formatTgl)
                If Len(FxDB(dr("kainputtgl"), "")) > 0 Then tglinput = AsFormatTanggal(FxDB(dr("kainputtgl"), ""), formatTglWaktu)
                If Len(FxDB(dr("kamodifikasitgl"), "")) > 0 Then tglmodif = AsFormatTanggal(FxDB(dr("kamodifikasitgl"), ""), formatTglWaktu)

                detail = String.Concat(detail, FxDB(dr("kaid"), 0), sptField,
                     FxDB(dr("kaidkontak"), 0), sptField,
                     FxDB(dr("kakodekontak"), ""), sptField,
                     FxDB(dr("kanama"), ""), sptField,
                     FxDB(dr("kajabatan"), ""), sptField,
                     FxDB(dr("kanotelp"), ""), sptField,
                     FxDB(dr("kanofax"), ""), sptField,
                     FxDB(dr("kanohp"), ""), sptField,
                     FxDB(dr("kaemail"), ""), sptField,
                     FxDB(dr("kawebsite"), ""), sptField,
                     FxDB(dr("kamessenger"), ""), sptField,
                     FxDB(dr("kaalamat"), ""), sptField,
                     tgllahir, sptField,
                     tglnikah, sptField,
                     FxDB(dr("kacatatan"), ""), sptField,
                     FxDB(dr("kadefault"), 0), sptField,
                     FxDB(dr("kainputuser"), 0), sptField,
                     tglinput, sptField,
                     FxDB(dr("kamodifikasiuser"), 0), sptField,
                     tglmodif, sptRow)
            Next
            detail = detail.Substring(0, detail.Length - sptRow.Length)

            'AMBIL DATA ITEM Price
            Dim dtPrice As New DataTable
            sql = "SELECT cp.khidkontak, cp.khidbarang, i.bkode, i.bnama, cp.khsatuan, cp.khkomisi, cp.khhargabeli, cp.khhargajual, cp.khberlakudari, cp.khberlakusampai, cp.khcatatan, cp.khinputuser, cp.khinputtgl, cp.khmodifikasiuser, cp.khmodifikasitgl, cp.khcustomtext1, cp.khcustomtext2, cp.khcustomtext3, cp.khcustomtext4, cp.khcustomtext5, cp.khcustomint1, cp.khcustomint2, cp.khcustomint3, cp.khcustomint4, cp.khcustomint5, cp.khcustomdbl1, cp.khcustomdbl2, cp.khcustomdbl3, cp.khcustomdbl4, cp.khcustomdbl5, cp.khcustomdate1, cp.khcustomdate2, cp.khcustomdate3, cp.khcustomdate4, cp.khcustomdate5 FROM m1_contact_price cp JOIN m1_contact c ON cp.khidkontak = c.kid AND cp.khidkontak = '" & FixDouble(idtransaksi) & "' JOIN m1_item i ON cp.khidbarang = i.bid"
            dtPrice = AmbilData("aplikasi1-M1_Item_Price", "", "cp.khidkontak, i.bkode", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtPrice.Rows
                price = String.Concat(price,
                     FxDB(dr("khidkontak"), ""), sptField,
                     FxDB(dr("khidbarang"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("khsatuan"), ""), sptField,
                     FxDB(dr("khkomisi"), 0), sptField,
                     FxDB(dr("khhargabeli"), 0), sptField,
                     FxDB(dr("khhargajual"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakudari"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khberlakusampai"), ""), formatTgl), sptField,
                     FxDB(dr("khcatatan"), ""), sptField,
                     FxDB(dr("khinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("khmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("khcustomtext1"), ""), sptField,
                     FxDB(dr("khcustomtext2"), ""), sptField,
                     FxDB(dr("khcustomtext3"), ""), sptField,
                     FxDB(dr("khcustomtext4"), ""), sptField,
                     FxDB(dr("khcustomtext5"), ""), sptField,
                     FxDB(dr("khcustomint1"), 0), sptField,
                     FxDB(dr("khcustomint2"), 0), sptField,
                     FxDB(dr("khcustomint3"), 0), sptField,
                     FxDB(dr("khcustomint4"), 0), sptField,
                     FxDB(dr("khcustomint5"), 0), sptField,
                     FxDB(dr("khcustomdbl1"), 0), sptField,
                     FxDB(dr("khcustomdbl2"), 0), sptField,
                     FxDB(dr("khcustomdbl3"), 0), sptField,
                     FxDB(dr("khcustomdbl4"), 0), sptField,
                     FxDB(dr("khcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("khcustomdate5"), ""), formatTgl), sptRow)
            Next
            If price.Length > 0 Then price = price.Substring(0, price.Length - sptRow.Length) Else price = price

            'AMBIL DATA ITEM Price
            Dim dtCommission As New DataTable
            sql = "SELECT * FROM m1_salesman_commission where scidkontak = '" & FixDouble(idtransaksi) & "'"
            dtCommission = AmbilData("aplikasi1-M1_Item_Price", "", "scidkontak", True, , , 0, 0, pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtCommission.Rows
                commission = String.Concat(commission,
                     FxDB(dr("scidkontak"), ""), sptField,
                     FxDB(dr("sckomisi1"), 0), sptField,
                     FxDB(dr("sckomisi2"), 0), sptField,
                     FxDB(dr("sckomisi3"), 0), sptField,
                     FxDB(dr("sckomisi4"), 0), sptField,
                     FxDB(dr("sckomisi5"), 0), sptField,
                     FxDB(dr("sckomisi6"), 0), sptField,
                     FxDB(dr("sckomisi7"), 0), sptField,
                     FxDB(dr("sckomisi8"), 0), sptField,
                     FxDB(dr("sckomisi9"), 0), sptField,
                     FxDB(dr("sckomisi10"), 0), sptField,
                     FxDB(dr("sccustomtext1"), ""), sptField,
                     FxDB(dr("sccustomtext2"), ""), sptField,
                     FxDB(dr("sccustomtext3"), ""), sptField,
                     FxDB(dr("sccustomtext4"), ""), sptField,
                     FxDB(dr("sccustomtext5"), ""), sptField,
                     FxDB(dr("sccustomtext6"), ""), sptField,
                     FxDB(dr("sccustomtext7"), ""), sptField,
                     FxDB(dr("sccustomtext8"), ""), sptField,
                     FxDB(dr("sccustomtext9"), ""), sptField,
                     FxDB(dr("sccustomtext10"), ""), sptField,
                     FxDB(dr("sccustomint1"), 0), sptField,
                     FxDB(dr("sccustomint2"), 0), sptField,
                     FxDB(dr("sccustomint3"), 0), sptField,
                     FxDB(dr("sccustomint4"), 0), sptField,
                     FxDB(dr("sccustomint5"), 0), sptField,
                     FxDB(dr("sccustomint6"), 0), sptField,
                     FxDB(dr("sccustomint7"), 0), sptField,
                     FxDB(dr("sccustomint8"), 0), sptField,
                     FxDB(dr("sccustomint9"), 0), sptField,
                     FxDB(dr("sccustomint10"), 0), sptField,
                     FxDB(dr("sccustomdbl1"), 0), sptField,
                     FxDB(dr("sccustomdbl2"), 0), sptField,
                     FxDB(dr("sccustomdbl3"), 0), sptField,
                     FxDB(dr("sccustomdbl4"), 0), sptField,
                     FxDB(dr("sccustomdbl5"), 0), sptField,
                     FxDB(dr("sccustomdbl6"), 0), sptField,
                     FxDB(dr("sccustomdbl7"), 0), sptField,
                     FxDB(dr("sccustomdbl8"), 0), sptField,
                     FxDB(dr("sccustomdbl9"), 0), sptField,
                     FxDB(dr("sccustomdbl10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate5"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate6"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate7"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate8"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate9"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("sccustomdate10"), ""), formatTgl), sptRow)
            Next
            If commission.Length > 0 Then commission = commission.Substring(0, commission.Length - sptRow.Length) Else commission = commission
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
        strResultData = String.Concat(utama, sptSubParam, detail, sptSubParam, price, sptSubParam, commission)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksalesmankode, krekhutangnama, kbagpembeliankode, kbagpembeliannama, krekpiutangnama, kbagpenjualankode, kbagpenjualannama, kbanknama, ktingkatjualnama, kkomisikode, kkomisinama, khargacustom" & sptSubParam & "kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl" & sptSubParam & "khidkontak, khidbarang, bkode, bnama, khsatuan, khkomisi, khhargabeli, khhargajual, khberlakudari, khberlakusampai, khcatatan, khinputuser, khinputtgl, khmodifikasiuser, khmodifikasitgl, khcustomtext1, khcustomtext2, khcustomtext3, khcustomtext4, khcustomtext5, khcustomint1, khcustomint2, khcustomint3, khcustomint4, khcustomint5, khcustomdbl1, khcustomdbl2, khcustomdbl3, khcustomdbl4, khcustomdbl5, khcustomdate1, khcustomdate2, khcustomdate3, khcustomdate4, khcustomdate5" & sptSubParam & "scidkontak, sckomisi1,sckomisi2,sckomisi3,sckomisi4,sckomisi5,sckomisi6,sckomisi7,sckomisi8,sckomisi9,sckomisi10, sccustomtext1, sccustomtext2, sccustomtext3, sccustomtext4, sccustomtext5, sccustomtext6,sccustomtext7,sccustomtext8,sccustomtext9,sccustomtext10,sccustomint1, sccustomint2, sccustomint3, sccustomint4, sccustomint5,sccustomint6,sccustomint7,sccustomint8,sccustomint9,sccustomint10, sccustomdbl1, sccustomdbl2, sccustomdbl3, sccustomdbl4, sccustomdbl5,sccustomdbl6,sccustomdbl7,sccustomdbl8,sccustomdbl9,sccustomdbl10, sccustomdate1, sccustomdate2, sccustomdate3, sccustomdate4, sccustomdate5,sccustomdate6,sccustomdate7,sccustomdate8,sccustomdate9,sccustomdate10"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactTerkait(ByVal param As String) As String
        'M1_ContactTerkait --------------------------------------------------------
        'kid, knama, sumber, idterkait

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
        'CEK IDTRANSAKSI
        Dim idtransaksi As String = ""
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "kid can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_contact_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("kid"), ""), sptField,
                             FxDB(dr("knama"), ""), sptField,
                             FxDB(dr("sumber"), ""), sptField,
                             FxDB(dr("idterkait"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Related Contact data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, knama, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactCekId(ByVal param As String) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim idSplit(1) As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", idtransaksi As String = ""

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        Dim kode As String = "", kategori As String = ""
        Dim idtrans(2) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 2) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK KODE
            If (Len(idtrans(0)) = 0) Then
                result(2) = "kkode can't be empty" : GoTo selesai
            Else
                kode = idtrans(0)
            End If
            'CEK KATEGORI
            If (Len(idtrans(1)) = 0) Then
                result(2) = "kkategori can't be empty" : GoTo selesai
            Else
                kategori = idtrans(1)
            End If
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(kid) FROM m1_contact WHERE kkode='" & kode & "' AND kkategori='" & kategori & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & kode & "' and '" & kategori & "' already exist for column kkode and kkategori." : GoTo selesai
        End If

        result(1) = 1
        result(2) = ""
        result(3) = 0
        result(4) = idtransaksi
        'END OF CEK DI DATABASE ==========================================================


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
    Public Function M1_ContactSimpan2(ByVal param As String) As String

        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""
        Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim dt As DataTable
        Dim pg1 As New RsPaging

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

        'MAPPING BUAT WS ----------------------------------------------------------
        'kid(0) As Integer, kkode(1) As String, knama(2) As String, kkategori(3) As String, kkategorinama(4) As String, 
        'kcabang(5) As String, kcabangnama(6) As String, klokasi(7) As String, klokasinama(8) As String, kgudang(9) As String, 
        'kgudangnama(10) As String, kkategorisalesman(11) As String, kkategorisalesmannama(12) As String, karea(13) As String, kareanama(14) As String, 
        'kkategoricustomer(15) As String, kkategoricustomernama(16) As String, kdivisi(17) As String, kdivisinama(18) As String, ksubdivisi(19) As String, 
        'ksubdivisinama(20) As String, ksalesman(21) As Integer, ksalesmannama(22) As String, kkontakperson(23) As String, kterminglobal(24) As Integer, 
        'kaktif(25) As Integer, kaktiftgl(26) As Date, k1alamat1(27) As String, k1alamat2(28) As String, k1alamat3(29) As String, 
        'k1alamat4(30) As String, k1alamat5(31) As String, k1kota(32) As String, k1propinsi(33) As String, k1kodepos(34) As String, 
        'k1negara(35) As String, k1kontakperson(36) As String, k1kontaknohp(37) As String, k1kontakemail(38) As String, k1notelp1(39) As String, 
        'k1notelp2(40) As String, k1nofax(41) As String, k1email(42) As String, k1website(43) As String, k2alamat1(44) As String, 
        'k2alamat2(45) As String, k2alamat3(46) As String, k2alamat4(47) As String, k2alamat5(48) As String, k2propinsi(49) As String, 
        'k2kota(50) As String, k2kodepos(51) As String, k2negara(52) As String, k2kontakperson(53) As String, k2kontaknohp(54) As String, 
        'k2kontakemail(55) As String, k2notelp1(56) As String, k2notelp2(57) As String, k2nofax(58) As String, k2email(59) As String, 
        'k2website(60) As String, k3alamat1(61) As String, k3alamat2(62) As String, k3alamat3(63) As String, k3alamat4(64) As String, 
        'k3alamat5(65) As String, k3kota(66) As String, k3propinsi(67) As String, k3kodepos(68) As String, k3negara(69) As String, 
        'k3kontakperson(70) As String, k3kontaknohp(71) As String, k3kontakemail(72) As String, k3notelp1(73) As String, k3notelp2(74) As String, 
        'k3nofax(75) As String, k3email(76) As String, k3website(77) As String, k4alamat1(78) As String, k4alamat2(79) As String, 
        'k4alamat3(80) As String, k4alamat4(81) As String, k4alamat5(82) As String, k4kota(83) As String, k4propinsi(84) As String, 
        'k4kodepos(85) As String, k4negara(86) As String, k4kontakperson(87) As String, k4kontaknohp(88) As String, k4kontakemail(89) As String, 
        'k4notelp1(90) As String, k4notelp2(91) As String, k4nofax(92) As String, k4email(93) As String, k4website(94) As String, 
        'knpwp(95) As String, kpkp(96) As Integer, kbatashutang(97) As Double, kterminbeli(98) As String, krekhutang(99) As String, 
        'kbagpembelian(100) As Integer, kfobbeli(101) As String, kviabeli(102) As String, kbataspiutang(103) As Double, kterminjual(104) As String, 
        'krekpiutang(105) As String, kbagpenjualan(106) As Integer, ktingkatjual(107) As Integer, kfobjual(108) As String, kviajual(109) As String, 
        'ktglkontrak(110) As Date, kbank(111) As String, knorekening(112) As String, kjeniskelamin(113) As Integer, kmatauang(114) As String, 
        'ktgllahir(115) As Date, ktglnikah(116) As Date, kkomisipenjualan(117) As Double, kcatatan(118) As String, kinputuser(119) As Integer, 
        'kinputtgl(120) As DateTime, kcustomtext1(121) As String, kcustomtext2(122) As String, kcustomtext3(123) As String, kcustomtext4(124) As String, 
        'kcustomtext5(125) As String, kcustomtext6(126) As String, kcustomtext7(127) As String, kcustomtext8(128) As String, kcustomtext9(129) As String, 
        'kmodifikasiuser(130) As Integer, kmodifikasitgl(131) As DateTime, kcustomtext10(132) As String, kcustomint1(133) As Integer, kcustomint2(134) As Integer, 
        'kcustomint3(135) As Integer, kcustomdbl1(136) As Double, kcustomdbl2(137) As Double, kcustomdbl3(138) As Double, kcustomdate1(139) As Date, 
        'kcustomdate2(140) As Date, kcustomdate3(141) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, 
        'ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, 
        'k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, 
        'k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, 
        'k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, 
        'k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, 
        'k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, 
        'k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, 
        'k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, 
        'k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, 
        'k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, 
        'k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, 
        'kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, 
        'krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, 
        'knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, 
        'kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, 
        'kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, 
        'kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, 
        'kcustomdate2, kcustomdate3

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 142) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================

        'VALIDASI TIPE DATA ==========================================================
        'kid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "kid required numeric." : GoTo selesai
        End If
        'ksalesman(21) As Integer
        If (IsNumeric(dataUtama(21)) = False) Then
            result(2) = "ksalesman required numeric." : GoTo selesai
        End If
        'kterminglobal(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "kterminglobal required numeric." : GoTo selesai
        End If
        'kaktif(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "kaktif required numeric." : GoTo selesai
        End If
        'kaktiftgl(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "kaktiftgl required date." : GoTo selesai
        End If
        'kpkp(96) As Integer
        If (IsNumeric(dataUtama(96)) = False) Then
            result(2) = "kpkp required numeric." : GoTo selesai
        End If
        'kbatashutang(97) As Double
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "kbatashutang required numeric." : GoTo selesai
        End If
        'kbagpembelian(100) As Integer
        If (IsNumeric(dataUtama(100)) = False) Then
            result(2) = "kbagpembelian required numeric." : GoTo selesai
        End If
        'kbataspiutang(103) As Double
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "kbataspiutang required numeric." : GoTo selesai
        End If
        'kbagpenjualan(106) As Integer
        If (IsNumeric(dataUtama(106)) = False) Then
            result(2) = "kbagpenjualan required numeric." : GoTo selesai
        End If
        'ktingkatjual(107) As Integer
        If (IsNumeric(dataUtama(107)) = False) Then
            result(2) = "ktingkatjual required numeric." : GoTo selesai
        End If
        'ktglkontrak(110) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "ktglkontrak required date." : GoTo selesai
        End If
        'kjeniskelamin(113) As Integer
        If (IsNumeric(dataUtama(113)) = False) Then
            result(2) = "kjeniskelamin required numeric." : GoTo selesai
        End If
        'ktgllahir(115) As Date
        If (IsDate(dataUtama(115)) = False) Then
            result(2) = "ktgllahir required date." : GoTo selesai
        End If
        'ktglnikah(116) As Date
        If (IsDate(dataUtama(116)) = False) Then
            result(2) = "ktglnikah required date." : GoTo selesai
        End If
        'kkomisipenjualan(117) As Double
        If (IsNumeric(dataUtama(117)) = False) Then
            result(2) = "kkomisipenjualan required numeric." : GoTo selesai
        End If
        'kinputuser(119) As Integer
        If (IsNumeric(dataUtama(119)) = False) Then
            result(2) = "kinputuser required numeric." : GoTo selesai
        End If
        'kinputtgl(120) As DateTime
        If (IsDate(dataUtama(120)) = False) Then
            result(2) = "kinputtgl required date." : GoTo selesai
        End If
        'kmodifikasiuser(130) As Integer
        If (IsNumeric(dataUtama(130)) = False) Then
            result(2) = "kmodifikasiuser required numeric." : GoTo selesai
        End If
        'kmodifikasitgl(131) As DateTime
        If (IsDate(dataUtama(131)) = False) Then
            result(2) = "kmodifikasitgl required date." : GoTo selesai
        End If
        'kcustomint1(133) As Integer
        If (IsNumeric(dataUtama(133)) = False) Then
            result(2) = "kcustomint1 required numeric." : GoTo selesai
        End If
        'kcustomint2(134) As Integer
        If (IsNumeric(dataUtama(134)) = False) Then
            result(2) = "kcustomint2 required numeric." : GoTo selesai
        End If
        'kcustomint3(135) As Integer
        If (IsNumeric(dataUtama(135)) = False) Then
            result(2) = "kcustomint3 required numeric." : GoTo selesai
        End If
        'kcustomdbl1(136) As Double
        If (IsNumeric(dataUtama(136)) = False) Then
            result(2) = "kcustomdbl1 required numeric." : GoTo selesai
        End If
        'kcustomdbl2(137) As Double
        If (IsNumeric(dataUtama(137)) = False) Then
            result(2) = "kcustomdbl2 required numeric." : GoTo selesai
        End If
        'kcustomdbl3(138) As Double
        If (IsNumeric(dataUtama(138)) = False) Then
            result(2) = "kcustomdbl3 required numeric." : GoTo selesai
        End If
        'kcustomdate1(139) As Date
        If (IsDate(dataUtama(139)) = False) Then
            result(2) = "kcustomdate1 required date." : GoTo selesai
        End If
        'kcustomdate2(140) As Date
        If (IsDate(dataUtama(140)) = False) Then
            result(2) = "kcustomdate2 required date." : GoTo selesai
        End If
        'kcustomdate3(141) As Date
        If (IsDate(dataUtama(141)) = False) Then
            result(2) = "kcustomdate3 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'kkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "kkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "kkode should not be more than 25 character." : GoTo selesai
        End If

        'knama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "knama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "knama should not be more than 100 character." : GoTo selesai
        End If

        'kkategori(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "kkategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 3 Then
            result(2) = "kkategori should not be more than 3 character." : GoTo selesai
        End If

        'kaktiftgl(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "kaktiftgl can't be empty" : GoTo selesai
        End If

        'kinputtgl(120) As DateTime
        If Len(dataUtama(120)) = 0 Then
            result(2) = "kinputtgl can't be empty" : GoTo selesai
        End If

        'kmodifikasitgl(131) As DateTime
        If Len(dataUtama(131)) = 0 Then
            result(2) = "kmodifikasitgl can't be empty" : GoTo selesai
        End If

        'kcustomdbl1(136) As Double
        If Len(dataUtama(136)) = 0 Then
            result(2) = "kcustomdbl1 can't be empty" : GoTo selesai
        End If

        'kcustomdbl2(137) As Double
        If Len(dataUtama(137)) = 0 Then
            result(2) = "kcustomdbl2 can't be empty" : GoTo selesai
        End If

        'kcustomdbl3(138) As Double
        If Len(dataUtama(138)) = 0 Then
            result(2) = "kcustomdbl3 can't be empty" : GoTo selesai
        End If

        'kcustomdate1(139) As Date
        If Len(dataUtama(139)) = 0 Then
            result(2) = "kcustomdate1 can't be empty" : GoTo selesai
        End If

        'kcustomdate2(140) As Date
        If Len(dataUtama(140)) = 0 Then
            result(2) = "kcustomdate2 can't be empty" : GoTo selesai
        End If

        'kcustomdate3(141) As Date
        If Len(dataUtama(141)) = 0 Then
            result(2) = "kcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA ========================================================

        'SIMPAN KE DATABASE ==========================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            If isUpdate Then
                'JIKA UPDATE CEK JML ROW PADA DATABASE
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(kid) FROM M1_Contact WHERE kid='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    sql = "Update M1_Contact set kkode  = '" & FixQuotes(dataUtama(1)) & "', knama  = '" & FixQuotes(dataUtama(2)) & "', kkategori  = '" & FixQuotes(dataUtama(3)) & "', kkategorinama  = '" & FixQuotes(dataUtama(4)) & "', kcabang  = '" & FixQuotes(dataUtama(5)) & "', kcabangnama  = '" & FixQuotes(dataUtama(6)) & "', klokasi  = '" & FixQuotes(dataUtama(7)) & "', klokasinama  = '" & FixQuotes(dataUtama(8)) & "', kgudang  = '" & FixQuotes(dataUtama(9)) & "', kgudangnama  = '" & FixQuotes(dataUtama(10)) & "', kkategorisalesman  = '" & FixQuotes(dataUtama(11)) & "', kkategorisalesmannama  = '" & FixQuotes(dataUtama(12)) & "', karea  = '" & FixQuotes(dataUtama(13)) & "', kareanama  = '" & FixQuotes(dataUtama(14)) & "', kkategoricustomer  = '" & FixQuotes(dataUtama(15)) & "', kkategoricustomernama  = '" & FixQuotes(dataUtama(16)) & "', kdivisi  = '" & FixQuotes(dataUtama(17)) & "', kdivisinama  = '" & FixQuotes(dataUtama(18)) & "', ksubdivisi  = '" & FixQuotes(dataUtama(19)) & "', ksubdivisinama  = '" & FixQuotes(dataUtama(20)) & "', ksalesman  = " & dataUtama(21) & ", ksalesmannama  = '" & FixQuotes(dataUtama(22)) & "', kkontakperson  = '" & FixQuotes(dataUtama(23)) & "', kterminglobal  = " & dataUtama(24) & ", kaktif  = " & dataUtama(25) & ", kaktiftgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(26))) & "', k1alamat1  = '" & FixQuotes(dataUtama(27)) & "', k1alamat2  = '" & FixQuotes(dataUtama(28)) & "', k1alamat3  = '" & FixQuotes(dataUtama(29)) & "', k1alamat4  = '" & FixQuotes(dataUtama(30)) & "', k1alamat5  = '" & FixQuotes(dataUtama(31)) & "', k1kota  = '" & FixQuotes(dataUtama(32)) & "', k1propinsi  = '" & FixQuotes(dataUtama(33)) & "', k1kodepos  = '" & FixQuotes(dataUtama(34)) & "', k1negara  = '" & FixQuotes(dataUtama(35)) & "', k1kontakperson  = '" & FixQuotes(dataUtama(36)) & "', k1kontaknohp  = '" & FixQuotes(dataUtama(37)) & "', k1kontakemail  = '" & FixQuotes(dataUtama(38)) & "', k1notelp1  = '" & FixQuotes(dataUtama(39)) & "', k1notelp2  = '" & FixQuotes(dataUtama(40)) & "', k1nofax  = '" & FixQuotes(dataUtama(41)) & "', k1email  = '" & FixQuotes(dataUtama(42)) & "', k1website  = '" & FixQuotes(dataUtama(43)) & "', k2alamat1  = '" & FixQuotes(dataUtama(44)) & "', k2alamat2  = '" & FixQuotes(dataUtama(45)) & "', k2alamat3  = '" & FixQuotes(dataUtama(46)) & "', k2alamat4  = '" & FixQuotes(dataUtama(47)) & "', k2alamat5  = '" & FixQuotes(dataUtama(48)) & "', k2propinsi  = '" & FixQuotes(dataUtama(49)) & "', k2kota  = '" & FixQuotes(dataUtama(50)) & "', k2kodepos  = '" & FixQuotes(dataUtama(51)) & "', k2negara  = '" & FixQuotes(dataUtama(52)) & "', k2kontakperson  = '" & FixQuotes(dataUtama(53)) & "', k2kontaknohp  = '" & FixQuotes(dataUtama(54)) & "', k2kontakemail  = '" & FixQuotes(dataUtama(55)) & "', k2notelp1  = '" & FixQuotes(dataUtama(56)) & "', k2notelp2  = '" & FixQuotes(dataUtama(57)) & "', k2nofax  = '" & FixQuotes(dataUtama(58)) & "', k2email  = '" & FixQuotes(dataUtama(59)) & "', k2website  = '" & FixQuotes(dataUtama(60)) & "', k3alamat1  = '" & FixQuotes(dataUtama(61)) & "', k3alamat2  = '" & FixQuotes(dataUtama(62)) & "', k3alamat3  = '" & FixQuotes(dataUtama(63)) & "', k3alamat4  = '" & FixQuotes(dataUtama(64)) & "', k3alamat5  = '" & FixQuotes(dataUtama(65)) & "', k3kota  = '" & FixQuotes(dataUtama(66)) & "', k3propinsi  = '" & FixQuotes(dataUtama(67)) & "', k3kodepos  = '" & FixQuotes(dataUtama(68)) & "', k3negara  = '" & FixQuotes(dataUtama(69)) & "', k3kontakperson  = '" & FixQuotes(dataUtama(70)) & "', k3kontaknohp  = '" & FixQuotes(dataUtama(71)) & "', k3kontakemail  = '" & FixQuotes(dataUtama(72)) & "', k3notelp1  = '" & FixQuotes(dataUtama(73)) & "', k3notelp2  = '" & FixQuotes(dataUtama(74)) & "', k3nofax  = '" & FixQuotes(dataUtama(75)) & "', k3email  = '" & FixQuotes(dataUtama(76)) & "', k3website  = '" & FixQuotes(dataUtama(77)) & "', k4alamat1  = '" & FixQuotes(dataUtama(78)) & "', k4alamat2  = '" & FixQuotes(dataUtama(79)) & "', k4alamat3  = '" & FixQuotes(dataUtama(80)) & "', k4alamat4  = '" & FixQuotes(dataUtama(81)) & "', k4alamat5  = '" & FixQuotes(dataUtama(82)) & "', k4kota  = '" & FixQuotes(dataUtama(83)) & "', k4propinsi  = '" & FixQuotes(dataUtama(84)) & "', k4kodepos  = '" & FixQuotes(dataUtama(85)) & "', k4negara  = '" & FixQuotes(dataUtama(86)) & "', k4kontakperson  = '" & FixQuotes(dataUtama(87)) & "', k4kontaknohp  = '" & FixQuotes(dataUtama(88)) & "', k4kontakemail  = '" & FixQuotes(dataUtama(89)) & "', k4notelp1  = '" & FixQuotes(dataUtama(90)) & "', k4notelp2  = '" & FixQuotes(dataUtama(91)) & "', k4nofax  = '" & FixQuotes(dataUtama(92)) & "', k4email  = '" & FixQuotes(dataUtama(93)) & "', k4website  = '" & FixQuotes(dataUtama(94)) & "', knpwp  = '" & FixQuotes(dataUtama(95)) & "', kpkp  = " & dataUtama(96) & ", kbatashutang  = '" & FixDouble(dataUtama(97)) & "', kterminbeli  = '" & FixQuotes(dataUtama(98)) & "', krekhutang  = '" & FixQuotes(dataUtama(99)) & "', kbagpembelian  = " & dataUtama(100) & ", kfobbeli  = '" & FixQuotes(dataUtama(101)) & "', kviabeli  = '" & FixQuotes(dataUtama(102)) & "', kbataspiutang  = '" & FixDouble(dataUtama(103)) & "', kterminjual  = '" & FixQuotes(dataUtama(104)) & "', krekpiutang  = '" & FixQuotes(dataUtama(105)) & "', kbagpenjualan  = " & dataUtama(106) & ", ktingkatjual  = " & dataUtama(107) & ", kfobjual  = '" & FixQuotes(dataUtama(108)) & "', kviajual  = '" & FixQuotes(dataUtama(109)) & "', ktglkontrak  = '" & FixQuotes(AsFormatTanggal(dataUtama(110))) & "', kbank  = '" & FixQuotes(dataUtama(111)) & "', knorekening  = '" & FixQuotes(dataUtama(112)) & "', kjeniskelamin  = " & dataUtama(113) & ", kmatauang  = '" & FixQuotes(dataUtama(114)) & "', ktgllahir  = '" & FixQuotes(AsFormatTanggal(dataUtama(115))) & "', ktglnikah  = '" & FixQuotes(AsFormatTanggal(dataUtama(116))) & "', kkomisipenjualan  = '" & FixDouble(dataUtama(117)) & "', kcatatan  = '" & FixQuotes(dataUtama(118)) & "', kinputuser  = " & dataUtama(119) & ", kinputtgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(120), "yyyy-MM-dd H:mm:ss")) & "', kcustomtext1  = '" & FixQuotes(dataUtama(121)) & "', kcustomtext2  = '" & FixQuotes(dataUtama(122)) & "', kcustomtext3  = '" & FixQuotes(dataUtama(123)) & "', kcustomtext4  = '" & FixQuotes(dataUtama(124)) & "', kcustomtext5  = '" & FixQuotes(dataUtama(125)) & "', kcustomtext6  = '" & FixQuotes(dataUtama(126)) & "', kcustomtext7  = '" & FixQuotes(dataUtama(127)) & "', kcustomtext8  = '" & FixQuotes(dataUtama(128)) & "', kcustomtext9  = '" & FixQuotes(dataUtama(129)) & "', kmodifikasiuser  = " & dataUtama(130) & ", kmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(131), "yyyy-MM-dd H:mm:ss")) & "', kcustomtext10  = '" & FixQuotes(dataUtama(132)) & "', kcustomint1  = " & dataUtama(133) & ", kcustomint2  = " & dataUtama(134) & ", kcustomint3  = " & dataUtama(135) & ", kcustomdbl1  = '" & FixDouble(dataUtama(136)) & "', kcustomdbl2  = '" & FixDouble(dataUtama(137)) & "', kcustomdbl3  = '" & FixDouble(dataUtama(138)) & "', kcustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(139))) & "', kcustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(140))) & "', kcustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(141))) & "' where kid = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : Trans.Rollback() : GoTo selesai
                End If

            Else

                'CEK KODE DAN KATEGORI KONTAK
                Dim dtCek As New DataTable
                sql = "SELECT kid FROM m1_contact WHERE kkode = '" & FixQuotes(dataUtama(1)) & "' AND kkategori = '" & FixQuotes(dataUtama(3)) & "'"
                dtCek = AsDataTableAmbilDariDB(sql)
                If dtCek.Rows.Count > 0 Then
                    result(2) = "Code : '" & FixQuotes(dataUtama(1)) & "' for Category :  '" & FixQuotes(dataUtama(3)) & "' is already exist." : Trans.Rollback() : GoTo selesai
                End If

                sql = "Insert into M1_Contact (kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kmodifikasiuser, kmodifikasitgl, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', '" & FixQuotes(dataUtama(10)) & "', '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', '" & FixQuotes(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixQuotes(dataUtama(16)) & "', '" & FixQuotes(dataUtama(17)) & "', '" & FixQuotes(dataUtama(18)) & "', '" & FixQuotes(dataUtama(19)) & "', '" & FixQuotes(dataUtama(20)) & "', " & dataUtama(21) & ", '" & FixQuotes(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "', " & dataUtama(24) & ", " & dataUtama(25) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(26))) & "', '" & FixQuotes(dataUtama(27)) & "', '" & FixQuotes(dataUtama(28)) & "', '" & FixQuotes(dataUtama(29)) & "', '" & FixQuotes(dataUtama(30)) & "', '" & FixQuotes(dataUtama(31)) & "', '" & FixQuotes(dataUtama(32)) & "', '" & FixQuotes(dataUtama(33)) & "', '" & FixQuotes(dataUtama(34)) & "', '" & FixQuotes(dataUtama(35)) & "', '" & FixQuotes(dataUtama(36)) & "', '" & FixQuotes(dataUtama(37)) & "', '" & FixQuotes(dataUtama(38)) & "', '" & FixQuotes(dataUtama(39)) & "', '" & FixQuotes(dataUtama(40)) & "', '" & FixQuotes(dataUtama(41)) & "', '" & FixQuotes(dataUtama(42)) & "', '" & FixQuotes(dataUtama(43)) & "', '" & FixQuotes(dataUtama(44)) & "', '" & FixQuotes(dataUtama(45)) & "', '" & FixQuotes(dataUtama(46)) & "', '" & FixQuotes(dataUtama(47)) & "', '" & FixQuotes(dataUtama(48)) & "', '" & FixQuotes(dataUtama(49)) & "', '" & FixQuotes(dataUtama(50)) & "', '" & FixQuotes(dataUtama(51)) & "', '" & FixQuotes(dataUtama(52)) & "', '" & FixQuotes(dataUtama(53)) & "', '" & FixQuotes(dataUtama(54)) & "', '" & FixQuotes(dataUtama(55)) & "', '" & FixQuotes(dataUtama(56)) & "', '" & FixQuotes(dataUtama(57)) & "', '" & FixQuotes(dataUtama(58)) & "', '" & FixQuotes(dataUtama(59)) & "', '" & FixQuotes(dataUtama(60)) & "', '" & FixQuotes(dataUtama(61)) & "', '" & FixQuotes(dataUtama(62)) & "', '" & FixQuotes(dataUtama(63)) & "', '" & FixQuotes(dataUtama(64)) & "', '" & FixQuotes(dataUtama(65)) & "', '" & FixQuotes(dataUtama(66)) & "', '" & FixQuotes(dataUtama(67)) & "', '" & FixQuotes(dataUtama(68)) & "', '" & FixQuotes(dataUtama(69)) & "', '" & FixQuotes(dataUtama(70)) & "', '" & FixQuotes(dataUtama(71)) & "', '" & FixQuotes(dataUtama(72)) & "', '" & FixQuotes(dataUtama(73)) & "', '" & FixQuotes(dataUtama(74)) & "', '" & FixQuotes(dataUtama(75)) & "', '" & FixQuotes(dataUtama(76)) & "', '" & FixQuotes(dataUtama(77)) & "', '" & FixQuotes(dataUtama(78)) & "', '" & FixQuotes(dataUtama(79)) & "', '" & FixQuotes(dataUtama(80)) & "', '" & FixQuotes(dataUtama(81)) & "', '" & FixQuotes(dataUtama(82)) & "', '" & FixQuotes(dataUtama(83)) & "', '" & FixQuotes(dataUtama(84)) & "', '" & FixQuotes(dataUtama(85)) & "', '" & FixQuotes(dataUtama(86)) & "', '" & FixQuotes(dataUtama(87)) & "', '" & FixQuotes(dataUtama(88)) & "', '" & FixQuotes(dataUtama(89)) & "', '" & FixQuotes(dataUtama(90)) & "', '" & FixQuotes(dataUtama(91)) & "', '" & FixQuotes(dataUtama(92)) & "', '" & FixQuotes(dataUtama(93)) & "', '" & FixQuotes(dataUtama(94)) & "', '" & FixQuotes(dataUtama(95)) & "', " & dataUtama(96) & ", '" & FixDouble(dataUtama(97)) & "', '" & FixQuotes(dataUtama(98)) & "', '" & FixQuotes(dataUtama(99)) & "', " & dataUtama(100) & ", '" & FixQuotes(dataUtama(101)) & "', '" & FixQuotes(dataUtama(102)) & "', '" & FixDouble(dataUtama(103)) & "', '" & FixQuotes(dataUtama(104)) & "', '" & FixQuotes(dataUtama(105)) & "', " & dataUtama(106) & ", " & dataUtama(107) & ", '" & FixQuotes(dataUtama(108)) & "', '" & FixQuotes(dataUtama(109)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(110))) & "', '" & FixQuotes(dataUtama(111)) & "', '" & FixQuotes(dataUtama(112)) & "', " & dataUtama(113) & ", '" & FixQuotes(dataUtama(114)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(115))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(116))) & "', '" & FixDouble(dataUtama(117)) & "', '" & FixQuotes(dataUtama(118)) & "', " & dataUtama(119) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(120), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dataUtama(121)) & "', '" & FixQuotes(dataUtama(122)) & "', '" & FixQuotes(dataUtama(123)) & "', '" & FixQuotes(dataUtama(124)) & "', '" & FixQuotes(dataUtama(125)) & "', '" & FixQuotes(dataUtama(126)) & "', '" & FixQuotes(dataUtama(127)) & "', '" & FixQuotes(dataUtama(128)) & "', '" & FixQuotes(dataUtama(129)) & "', " & dataUtama(130) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(131), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dataUtama(132)) & "', " & dataUtama(133) & ", " & dataUtama(134) & ", " & dataUtama(135) & ", '" & FixDouble(dataUtama(136)) & "', '" & FixDouble(dataUtama(137)) & "', '" & FixDouble(dataUtama(138)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(139))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(140))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(141))) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =========================================================================
            dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
            pg1 = pg1
            If dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    search = String.Concat(search,
                                 FxDB(dr("kid"), 0), sptField,
                                 FxDB(dr("kkode"), ""), sptField,
                                 FxDB(dr("knama"), ""), sptField,
                                 FxDB(dr("kkategori"), ""), sptField,
                                 FxDB(dr("kkategorinama"), ""), sptField,
                                 FxDB(dr("kcabang"), ""), sptField,
                                 FxDB(dr("kcabangnama"), ""), sptField,
                                 FxDB(dr("klokasi"), ""), sptField,
                                 FxDB(dr("klokasinama"), ""), sptField,
                                 FxDB(dr("kgudang"), ""), sptField,
                                 FxDB(dr("kgudangnama"), ""), sptField,
                                 FxDB(dr("kkategorisalesman"), ""), sptField,
                                 FxDB(dr("kkategorisalesmannama"), ""), sptField,
                                 FxDB(dr("karea"), ""), sptField,
                                 FxDB(dr("kareanama"), ""), sptField,
                                 FxDB(dr("kkategoricustomer"), ""), sptField,
                                 FxDB(dr("kkategoricustomernama"), ""), sptField,
                                 FxDB(dr("kdivisi"), ""), sptField,
                                 FxDB(dr("kdivisinama"), ""), sptField,
                                 FxDB(dr("ksubdivisi"), ""), sptField,
                                 FxDB(dr("ksubdivisinama"), ""), sptField,
                                 FxDB(dr("ksalesman"), 0), sptField,
                                 FxDB(dr("ksalesmannama"), ""), sptField,
                                 FxDB(dr("kkontakperson"), ""), sptField,
                                 FxDB(dr("kterminglobal"), 0), sptField,
                                 FxDB(dr("kaktif"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("kaktiftgl"), ""), formatTgl), sptField,
                                 FxDB(dr("k1alamat1"), ""), sptField,
                                 FxDB(dr("k1alamat2"), ""), sptField,
                                 FxDB(dr("k1alamat3"), ""), sptField,
                                 FxDB(dr("k1alamat4"), ""), sptField,
                                 FxDB(dr("k1alamat5"), ""), sptField,
                                 FxDB(dr("k1kota"), ""), sptField,
                                 FxDB(dr("k1propinsi"), ""), sptField,
                                 FxDB(dr("k1kodepos"), ""), sptField,
                                 FxDB(dr("k1negara"), ""), sptField,
                                 FxDB(dr("k1kontakperson"), ""), sptField,
                                 FxDB(dr("k1kontaknohp"), ""), sptField,
                                 FxDB(dr("k1kontakemail"), ""), sptField,
                                 FxDB(dr("k1notelp1"), ""), sptField,
                                 FxDB(dr("k1notelp2"), ""), sptField,
                                 FxDB(dr("k1nofax"), ""), sptField,
                                 FxDB(dr("k1email"), ""), sptField,
                                 FxDB(dr("k1website"), ""), sptField,
                                 FxDB(dr("k2alamat1"), ""), sptField,
                                 FxDB(dr("k2alamat2"), ""), sptField,
                                 FxDB(dr("k2alamat3"), ""), sptField,
                                 FxDB(dr("k2alamat4"), ""), sptField,
                                 FxDB(dr("k2alamat5"), ""), sptField,
                                 FxDB(dr("k2propinsi"), ""), sptField,
                                 FxDB(dr("k2kota"), ""), sptField,
                                 FxDB(dr("k2kodepos"), ""), sptField,
                                 FxDB(dr("k2negara"), ""), sptField,
                                 FxDB(dr("k2kontakperson"), ""), sptField,
                                 FxDB(dr("k2kontaknohp"), ""), sptField,
                                 FxDB(dr("k2kontakemail"), ""), sptField,
                                 FxDB(dr("k2notelp1"), ""), sptField,
                                 FxDB(dr("k2notelp2"), ""), sptField,
                                 FxDB(dr("k2nofax"), ""), sptField,
                                 FxDB(dr("k2email"), ""), sptField,
                                 FxDB(dr("k2website"), ""), sptField,
                                 FxDB(dr("k3alamat1"), ""), sptField,
                                 FxDB(dr("k3alamat2"), ""), sptField,
                                 FxDB(dr("k3alamat3"), ""), sptField,
                                 FxDB(dr("k3alamat4"), ""), sptField,
                                 FxDB(dr("k3alamat5"), ""), sptField,
                                 FxDB(dr("k3kota"), ""), sptField,
                                 FxDB(dr("k3propinsi"), ""), sptField,
                                 FxDB(dr("k3kodepos"), ""), sptField,
                                 FxDB(dr("k3negara"), ""), sptField,
                                 FxDB(dr("k3kontakperson"), ""), sptField,
                                 FxDB(dr("k3kontaknohp"), ""), sptField,
                                 FxDB(dr("k3kontakemail"), ""), sptField,
                                 FxDB(dr("k3notelp1"), ""), sptField,
                                 FxDB(dr("k3notelp2"), ""), sptField,
                                 FxDB(dr("k3nofax"), ""), sptField,
                                 FxDB(dr("k3email"), ""), sptField,
                                 FxDB(dr("k3website"), ""), sptField,
                                 FxDB(dr("k4alamat1"), ""), sptField,
                                 FxDB(dr("k4alamat2"), ""), sptField,
                                 FxDB(dr("k4alamat3"), ""), sptField,
                                 FxDB(dr("k4alamat4"), ""), sptField,
                                 FxDB(dr("k4alamat5"), ""), sptField,
                                 FxDB(dr("k4kota"), ""), sptField,
                                 FxDB(dr("k4propinsi"), ""), sptField,
                                 FxDB(dr("k4kodepos"), ""), sptField,
                                 FxDB(dr("k4negara"), ""), sptField,
                                 FxDB(dr("k4kontakperson"), ""), sptField,
                                 FxDB(dr("k4kontaknohp"), ""), sptField,
                                 FxDB(dr("k4kontakemail"), ""), sptField,
                                 FxDB(dr("k4notelp1"), ""), sptField,
                                 FxDB(dr("k4notelp2"), ""), sptField,
                                 FxDB(dr("k4nofax"), ""), sptField,
                                 FxDB(dr("k4email"), ""), sptField,
                                 FxDB(dr("k4website"), ""), sptField,
                                 FxDB(dr("knpwp"), ""), sptField,
                                 FxDB(dr("kpkp"), 0), sptField,
                                 FxDB(dr("kbatashutang"), 0), sptField,
                                 FxDB(dr("kterminbeli"), ""), sptField,
                                 FxDB(dr("krekhutang"), ""), sptField,
                                 FxDB(dr("kbagpembelian"), 0), sptField,
                                 FxDB(dr("kfobbeli"), ""), sptField,
                                 FxDB(dr("kviabeli"), ""), sptField,
                                 FxDB(dr("kbataspiutang"), 0), sptField,
                                 FxDB(dr("kterminjual"), ""), sptField,
                                 FxDB(dr("krekpiutang"), ""), sptField,
                                 FxDB(dr("kbagpenjualan"), 0), sptField,
                                 FxDB(dr("ktingkatjual"), 0), sptField,
                                 FxDB(dr("kfobjual"), ""), sptField,
                                 FxDB(dr("kviajual"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("ktglkontrak"), ""), formatTgl), sptField,
                                 FxDB(dr("kbank"), ""), sptField,
                                 FxDB(dr("knorekening"), ""), sptField,
                                 FxDB(dr("kjeniskelamin"), 0), sptField,
                                 FxDB(dr("kmatauang"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("ktgllahir"), ""), formatTgl), sptField,
                                 AsFormatTanggal(FxDB(dr("ktglnikah"), ""), formatTgl), sptField,
                                 FxDB(dr("kkomisipenjualan"), 0), sptField,
                                 FxDB(dr("kcatatan"), ""), sptField,
                                 FxDB(dr("kinputuser"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("kinputtgl"), ""), formatTglWaktu), sptField,
                                 FxDB(dr("kcustomtext1"), ""), sptField,
                                 FxDB(dr("kcustomtext2"), ""), sptField,
                                 FxDB(dr("kcustomtext3"), ""), sptField,
                                 FxDB(dr("kcustomtext4"), ""), sptField,
                                 FxDB(dr("kcustomtext5"), ""), sptField,
                                 FxDB(dr("kcustomtext6"), ""), sptField,
                                 FxDB(dr("kcustomtext7"), ""), sptField,
                                 FxDB(dr("kcustomtext8"), ""), sptField,
                                 FxDB(dr("kcustomtext9"), ""), sptField,
                                 FxDB(dr("kmodifikasiuser"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                                 FxDB(dr("kcustomtext10"), ""), sptField,
                                 FxDB(dr("kcustomint1"), 0), sptField,
                                 FxDB(dr("kcustomint2"), 0), sptField,
                                 FxDB(dr("kcustomint3"), 0), sptField,
                                 FxDB(dr("kcustomdbl1"), 0), sptField,
                                 FxDB(dr("kcustomdbl2"), 0), sptField,
                                 FxDB(dr("kcustomdbl3"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("kcustomdate1"), ""), formatTgl), sptField,
                                 AsFormatTanggal(FxDB(dr("kcustomdate2"), ""), formatTgl), sptField,
                                 AsFormatTanggal(FxDB(dr("kcustomdate3"), ""), formatTgl), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)

                result(1) = 1
                resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                resultPaging(1) = Math.Abs(Val(pg1.isNext))
                resultPaging(2) = Math.Abs(Val(pg1.isPrev))
                resultPaging(3) = pg1.countPage
                resultPaging(4) = pg1.countRow
            Else
                result(2) = "Contact data not found."
            End If
            'END OF AMBIL DATA ==================================================================

        Catch ex As Exception

            Trans.Rollback() '*** RollBack Transaction ***'  
            result(1) = 0
            result(2) = "Transaction Rollback : " & ex.Message
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
        strResultData = search
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactDelete2(ByVal param As String) As String

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
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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
        If ClsValidKey.ApaBisaAkses(1, 1, 2) = False Then
            result(2) = "Access denied for delete data"
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "kid required numeric." : GoTo selesai
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
            'DELETE
            sql = "DELETE FROM M1_Contact WHERE kid = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'.

            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = idtransaksi

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_ContactSearch(PostWsSearch(paramSplit(0), "M1_ContactSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_ContactDownload(ByVal param As String) As String
        'M1_ContactDownload --------------------------------------------------------
        'Utama
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, 
        'ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, 
        'kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, 
        'k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, 
        'k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, 
        'k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, 
        'k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, 
        'k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, 
        'k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, 
        'k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, 
        'k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, 
        'kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, 
        'kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, 
        'ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, 
        'kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, 
        'kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, 
        'kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, 
        'kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron

        'Detail
        'kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, 
        'kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, 
        'kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", detail As String = ""

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'AMBIL DATA UTAMA
        dt = AmbilData("aplikasi1-M1_Contact", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("kid"), ""), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptField,
                     FxDB(dr("kkategori"), ""), sptField,
                     FxDB(dr("kkategorinama"), ""), sptField,
                     FxDB(dr("kcabang"), ""), sptField,
                     FxDB(dr("kcabangnama"), ""), sptField,
                     FxDB(dr("klokasi"), ""), sptField,
                     FxDB(dr("klokasinama"), ""), sptField,
                     FxDB(dr("kgudang"), ""), sptField,
                     FxDB(dr("kgudangnama"), ""), sptField,
                     FxDB(dr("kkategorisalesman"), ""), sptField,
                     FxDB(dr("kkategorisalesmannama"), ""), sptField,
                     FxDB(dr("karea"), ""), sptField,
                     FxDB(dr("kareanama"), ""), sptField,
                     FxDB(dr("kkategoricustomer"), ""), sptField,
                     FxDB(dr("kkategoricustomernama"), ""), sptField,
                     FxDB(dr("kkategorisupplier"), ""), sptField,
                     FxDB(dr("kkategorisuppliernama"), ""), sptField,
                     FxDB(dr("kdivisi"), ""), sptField,
                     FxDB(dr("kdivisinama"), ""), sptField,
                     FxDB(dr("ksubdivisi"), ""), sptField,
                     FxDB(dr("ksubdivisinama"), ""), sptField,
                     FxDB(dr("ksalesman"), ""), sptField,
                     FxDB(dr("ksalesmannama"), ""), sptField,
                     FxDB(dr("kkontakperson"), ""), sptField,
                     FxDB(dr("kterminglobal"), 0), sptField,
                     FxDB(dr("kaktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kaktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("k1alamat1"), ""), sptField,
                     FxDB(dr("k1alamat2"), ""), sptField,
                     FxDB(dr("k1alamat3"), ""), sptField,
                     FxDB(dr("k1alamat4"), ""), sptField,
                     FxDB(dr("k1alamat5"), ""), sptField,
                     FxDB(dr("k1kota"), ""), sptField,
                     FxDB(dr("k1propinsi"), ""), sptField,
                     FxDB(dr("k1kodepos"), ""), sptField,
                     FxDB(dr("k1negara"), ""), sptField,
                     FxDB(dr("k1kontakperson"), ""), sptField,
                     FxDB(dr("k1kontaknohp"), ""), sptField,
                     FxDB(dr("k1kontakemail"), ""), sptField,
                     FxDB(dr("k1notelp1"), ""), sptField,
                     FxDB(dr("k1notelp2"), ""), sptField,
                     FxDB(dr("k1nofax"), ""), sptField,
                     FxDB(dr("k1email"), ""), sptField,
                     FxDB(dr("k1website"), ""), sptField,
                     FxDB(dr("k2alamat1"), ""), sptField,
                     FxDB(dr("k2alamat2"), ""), sptField,
                     FxDB(dr("k2alamat3"), ""), sptField,
                     FxDB(dr("k2alamat4"), ""), sptField,
                     FxDB(dr("k2alamat5"), ""), sptField,
                     FxDB(dr("k2propinsi"), ""), sptField,
                     FxDB(dr("k2kota"), ""), sptField,
                     FxDB(dr("k2kodepos"), ""), sptField,
                     FxDB(dr("k2negara"), ""), sptField,
                     FxDB(dr("k2kontakperson"), ""), sptField,
                     FxDB(dr("k2kontaknohp"), ""), sptField,
                     FxDB(dr("k2kontakemail"), ""), sptField,
                     FxDB(dr("k2notelp1"), ""), sptField,
                     FxDB(dr("k2notelp2"), ""), sptField,
                     FxDB(dr("k2nofax"), ""), sptField,
                     FxDB(dr("k2email"), ""), sptField,
                     FxDB(dr("k2website"), ""), sptField,
                     FxDB(dr("k3alamat1"), ""), sptField,
                     FxDB(dr("k3alamat2"), ""), sptField,
                     FxDB(dr("k3alamat3"), ""), sptField,
                     FxDB(dr("k3alamat4"), ""), sptField,
                     FxDB(dr("k3alamat5"), ""), sptField,
                     FxDB(dr("k3kota"), ""), sptField,
                     FxDB(dr("k3propinsi"), ""), sptField,
                     FxDB(dr("k3kodepos"), ""), sptField,
                     FxDB(dr("k3negara"), ""), sptField,
                     FxDB(dr("k3kontakperson"), ""), sptField,
                     FxDB(dr("k3kontaknohp"), ""), sptField,
                     FxDB(dr("k3kontakemail"), ""), sptField,
                     FxDB(dr("k3notelp1"), ""), sptField,
                     FxDB(dr("k3notelp2"), ""), sptField,
                     FxDB(dr("k3nofax"), ""), sptField,
                     FxDB(dr("k3email"), ""), sptField,
                     FxDB(dr("k3website"), ""), sptField,
                     FxDB(dr("k4alamat1"), ""), sptField,
                     FxDB(dr("k4alamat2"), ""), sptField,
                     FxDB(dr("k4alamat3"), ""), sptField,
                     FxDB(dr("k4alamat4"), ""), sptField,
                     FxDB(dr("k4alamat5"), ""), sptField,
                     FxDB(dr("k4kota"), ""), sptField,
                     FxDB(dr("k4propinsi"), ""), sptField,
                     FxDB(dr("k4kodepos"), ""), sptField,
                     FxDB(dr("k4negara"), ""), sptField,
                     FxDB(dr("k4kontakperson"), ""), sptField,
                     FxDB(dr("k4kontaknohp"), ""), sptField,
                     FxDB(dr("k4kontakemail"), ""), sptField,
                     FxDB(dr("k4notelp1"), ""), sptField,
                     FxDB(dr("k4notelp2"), ""), sptField,
                     FxDB(dr("k4nofax"), ""), sptField,
                     FxDB(dr("k4email"), ""), sptField,
                     FxDB(dr("k4website"), ""), sptField,
                     FxDB(dr("knpwp"), ""), sptField,
                     FxDB(dr("kpkp"), 0), sptField,
                     FxDB(dr("kbatashutang"), 0), sptField,
                     FxDB(dr("kterminbeli"), ""), sptField,
                     FxDB(dr("krekhutang"), ""), sptField,
                     FxDB(dr("kbagpembelian"), ""), sptField,
                     FxDB(dr("kfobbeli"), ""), sptField,
                     FxDB(dr("kviabeli"), ""), sptField,
                     FxDB(dr("kbataspiutang"), 0), sptField,
                     FxDB(dr("kterminjual"), ""), sptField,
                     FxDB(dr("krekpiutang"), ""), sptField,
                     FxDB(dr("kbagpenjualan"), ""), sptField,
                     FxDB(dr("ktingkatjual"), 0), sptField,
                     FxDB(dr("kfobjual"), ""), sptField,
                     FxDB(dr("kviajual"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ktglkontrak"), ""), formatTgl), sptField,
                     FxDB(dr("kbank"), ""), sptField,
                     FxDB(dr("knorekening"), ""), sptField,
                     FxDB(dr("kjeniskelamin"), 0), sptField,
                     FxDB(dr("kmatauang"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ktgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("ktglnikah"), ""), formatTgl), sptField,
                     FxDB(dr("kkomisipenjualan"), 0), sptField,
                     FxDB(dr("kcatatan"), ""), sptField,
                     FxDB(dr("kinputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kcustomtext1"), ""), sptField,
                     FxDB(dr("kcustomtext2"), ""), sptField,
                     FxDB(dr("kcustomtext3"), ""), sptField,
                     FxDB(dr("kcustomtext4"), ""), sptField,
                     FxDB(dr("kcustomtext5"), ""), sptField,
                     FxDB(dr("kcustomtext6"), ""), sptField,
                     FxDB(dr("kcustomtext7"), ""), sptField,
                     FxDB(dr("kcustomtext8"), ""), sptField,
                     FxDB(dr("kcustomtext9"), ""), sptField,
                     FxDB(dr("kcustomtext10"), ""), sptField,
                     FxDB(dr("kcustomint1"), 0), sptField,
                     FxDB(dr("kcustomint2"), 0), sptField,
                     FxDB(dr("kcustomint3"), 0), sptField,
                     FxDB(dr("kcustomdbl1"), 0), sptField,
                     FxDB(dr("kcustomdbl2"), 0), sptField,
                     FxDB(dr("kcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("kcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("ksinkron"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)


            'AMBIL DATA DETAIL
            sql = "SELECT ca.kaid, ca.kaidkontak, ca.kakodekontak, ca.kanama, ca.kajabatan, ca.kanotelp, ca.kanofax, ca.kanohp, ca.kaemail, ca.kawebsite, ca.kamessenger, ca.kaalamat, ca.katgllahir, ca.katglnikah, ca.kacatatan, ca.kadefault, ca.kainputuser, ca.kainputtgl, ca.kamodifikasiuser, ca.kamodifikasitgl FROM m1_contact_attention ca JOIN m1_contact c ON ca.kaidkontak = c.kid"
            Dim dtdetail As New DataTable
            dtdetail = AmbilData("aplikasi1-M1_Contact_Attention", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtdetail.Rows
                detail = String.Concat(detail,
                     FxDB(dr("kaid"), ""), sptField,
                     FxDB(dr("kaidkontak"), ""), sptField,
                     FxDB(dr("kakodekontak"), ""), sptField,
                     FxDB(dr("kanama"), ""), sptField,
                     FxDB(dr("kajabatan"), ""), sptField,
                     FxDB(dr("kanotelp"), ""), sptField,
                     FxDB(dr("kanofax"), ""), sptField,
                     FxDB(dr("kanohp"), ""), sptField,
                     FxDB(dr("kaemail"), ""), sptField,
                     FxDB(dr("kawebsite"), ""), sptField,
                     FxDB(dr("kamessenger"), ""), sptField,
                     FxDB(dr("kaalamat"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("katgllahir"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("katglnikah"), ""), formatTgl), sptField,
                     FxDB(dr("kacatatan"), ""), sptField,
                     FxDB(dr("kadefault"), 0), sptField,
                     FxDB(dr("kainputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kainputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("kamodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("kamodifikasitgl"), ""), formatTglWaktu), sptRow)
            Next
            If detail.Length > 0 Then detail = detail.Substring(0, detail.Length - sptRow.Length) Else detail = detail

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
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search, sptSubParam, detail)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron" & sptSubParam & "kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ContactImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataRowUtama(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 2) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'MAPPING BUAT WS ----------------------------------------------------------
        'kid(0) As Integer, kkode(1) As String, knama(2) As String, kkategori(3) As String, kkategorinama(4) As String, 
        'kcabang(5) As String, kcabangnama(6) As String, klokasi(7) As String, klokasinama(8) As String, kgudang(9) As String, 
        'kgudangnama(10) As String, kkategorisalesman(11) As String, kkategorisalesmannama(12) As String, karea(13) As String, kareanama(14) As String, 
        'kkategoricustomer(15) As String, kkategoricustomernama(16) As String, kkategorisupplier(17) As String, kkategorisuppliernama(18) As String, kdivisi(19) As String, 
        'kdivisinama(20) As String, ksubdivisi(21) As String, ksubdivisinama(22) As String, ksalesman(23) As Integer, ksalesmannama(24) As String, 
        'kkontakperson(25) As String, kterminglobal(26) As Integer, kaktif(27) As Integer, kaktiftgl(28) As Date, k1alamat1(29) As String, 
        'k1alamat2(30) As String, k1alamat3(31) As String, k1alamat4(32) As String, k1alamat5(33) As String, k1kota(34) As String, 
        'k1propinsi(35) As String, k1kodepos(36) As String, k1negara(37) As String, k1kontakperson(38) As String, k1kontaknohp(39) As String, 
        'k1kontakemail(40) As String, k1notelp1(41) As String, k1notelp2(42) As String, k1nofax(43) As String, k1email(44) As String, 
        'k1website(45) As String, k2alamat1(46) As String, k2alamat2(47) As String, k2alamat3(48) As String, k2alamat4(49) As String, 
        'k2alamat5(50) As String, k2propinsi(51) As String, k2kota(52) As String, k2kodepos(53) As String, k2negara(54) As String, 
        'k2kontakperson(55) As String, k2kontaknohp(56) As String, k2kontakemail(57) As String, k2notelp1(58) As String, k2notelp2(59) As String, 
        'k2nofax(60) As String, k2email(61) As String, k2website(62) As String, k3alamat1(63) As String, k3alamat2(64) As String, 
        'k3alamat3(65) As String, k3alamat4(66) As String, k3alamat5(67) As String, k3kota(68) As String, k3propinsi(69) As String, 
        'k3kodepos(70) As String, k3negara(71) As String, k3kontakperson(72) As String, k3kontaknohp(73) As String, k3kontakemail(74) As String, 
        'k3notelp1(75) As String, k3notelp2(76) As String, k3nofax(77) As String, k3email(78) As String, k3website(79) As String, 
        'k4alamat1(80) As String, k4alamat2(81) As String, k4alamat3(82) As String, k4alamat4(83) As String, k4alamat5(84) As String, 
        'k4kota(85) As String, k4propinsi(86) As String, k4kodepos(87) As String, k4negara(88) As String, k4kontakperson(89) As String, 
        'k4kontaknohp(90) As String, k4kontakemail(91) As String, k4notelp1(92) As String, k4notelp2(93) As String, k4nofax(94) As String, 
        'k4email(95) As String, k4website(96) As String, knpwp(97) As String, kpkp(98) As Integer, kbatashutang(99) As Double, 
        'kterminbeli(100) As String, krekhutang(101) As String, kbagpembelian(102) As Integer, kfobbeli(103) As String, kviabeli(104) As String, 
        'kbataspiutang(105) As Double, kterminjual(106) As String, krekpiutang(107) As String, kbagpenjualan(108) As Integer, ktingkatjual(109) As Integer, 
        'kfobjual(110) As String, kviajual(111) As String, ktglkontrak(112) As Date, kbank(113) As String, knorekening(114) As String, 
        'kjeniskelamin(115) As Integer, kmatauang(116) As String, ktgllahir(117) As Date, ktglnikah(118) As Date, kkomisipenjualan(119) As Double, 
        'kcatatan(120) As String, kinputuser(121) As Integer, kinputtgl(122) As DateTime, kmodifikasiuser(123) As Integer, kmodifikasitgl(124) As DateTime, 
        'kcustomtext1(125) As String, kcustomtext2(126) As String, kcustomtext3(127) As String, kcustomtext4(128) As String, kcustomtext5(129) As String, 
        'kcustomtext6(130) As String, kcustomtext7(131) As String, kcustomtext8(132) As String, kcustomtext9(133) As String, kcustomtext10(134) As String, 
        'kcustomint1(135) As Integer, kcustomint2(136) As Integer, kcustomint3(137) As Integer, kcustomdbl1(138) As Double, kcustomdbl2(139) As Double, 
        'kcustomdbl3(140) As Double, kcustomdate1(141) As Date, kcustomdate2(142) As Date, kcustomdate3(143) As Date, ksinkron(144) As Integer


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, 
        'klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, 
        'kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, 
        'ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, 
        'kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, 
        'k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, 
        'k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, 
        'k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, 
        'k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, 
        'k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, 
        'k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, 
        'k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, 
        'k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, 
        'k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, 
        'kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, 
        'kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, 
        'ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, 
        'kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, 
        'kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, 
        'kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, 
        'kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptRow)    'SPLIT PARAMETER DATA UTAMA
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "kid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcabangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "klokasinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kgudangnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesman", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "karea", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kareanama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomer", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategoricustomernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisupplier", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkategorisuppliernama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksubdivisinama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksalesman", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ksalesmannama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminglobal", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kaktiftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k1website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k2website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k3website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4alamat5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kota", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4propinsi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kodepos", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4negara", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakperson", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontaknohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4kontakemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4notelp2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4nofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4email", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "k4website", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knpwp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kpkp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kbatashutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekhutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbataspiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kterminjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "krekpiutang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbagpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "ktingkatjual", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kfobjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kviajual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglkontrak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kbank", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "knorekening", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kjeniskelamin", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ktglnikah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kkomisipenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomtext10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "kcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "kcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "ksinkron", AsEnumTypeData.AsInt64)

        Dim JmlDtUtama As Integer = dataUtama.Length
        For i = 1 To JmlDtUtama
            'SPLIT DATA UTAMA
            dataRowUtama = dataUtama(i - 1).Split(sptField)

            'CEK ARRAY DATA UTAMA
            If (dataRowUtama.Length <> 145) Then
                result(2) = "Main Row : " & i & " - Invalid main transaction data parameter." : GoTo selesai
            End If

            'VALIDASI TIPE DATA UTAMA ==========================================================
            'kid(0) As Integer
            If (IsNumeric(dataRowUtama(0)) = False) Then
                result(2) = "Main Row : " & i & " - kid required numeric." : GoTo selesai
            End If
            'ksalesman(23) As Integer
            If (IsNumeric(dataRowUtama(23)) = False) Then
                result(2) = "Main Row : " & i & " - ksalesman required numeric." : GoTo selesai
            End If
            'kterminglobal(26) As Integer
            If (IsNumeric(dataRowUtama(26)) = False) Then
                result(2) = "Main Row : " & i & " - kterminglobal required numeric." : GoTo selesai
            End If
            'kaktif(27) As Integer
            If (IsNumeric(dataRowUtama(27)) = False) Then
                result(2) = "Main Row : " & i & " - kaktif required numeric." : GoTo selesai
            End If
            'kaktiftgl(28) As Date
            If (IsDate(dataRowUtama(28)) = False) Then
                result(2) = "Main Row : " & i & " - kaktiftgl required date." : GoTo selesai
            End If
            'kpkp(98) As Integer
            If (IsNumeric(dataRowUtama(98)) = False) Then
                result(2) = "Main Row : " & i & " - kpkp required numeric." : GoTo selesai
            End If
            'kbatashutang(99) As Double
            If (IsNumeric(dataRowUtama(99)) = False) Then
                result(2) = "Main Row : " & i & " - kbatashutang required numeric." : GoTo selesai
            End If
            'kbagpembelian(102) As Integer
            If (IsNumeric(dataRowUtama(102)) = False) Then
                result(2) = "Main Row : " & i & " - kbagpembelian required numeric." : GoTo selesai
            End If
            'kbataspiutang(105) As Double
            If (IsNumeric(dataRowUtama(105)) = False) Then
                result(2) = "Main Row : " & i & " - kbataspiutang required numeric." : GoTo selesai
            End If
            'kbagpenjualan(108) As Integer
            If (IsNumeric(dataRowUtama(108)) = False) Then
                result(2) = "Main Row : " & i & " - kbagpenjualan required numeric." : GoTo selesai
            End If
            'ktingkatjual(109) As Integer
            If (IsNumeric(dataRowUtama(109)) = False) Then
                result(2) = "Main Row : " & i & " - ktingkatjual required numeric." : GoTo selesai
            End If
            'ktglkontrak(112) As Date
            If (IsDate(dataRowUtama(112)) = False) Then
                result(2) = "Main Row : " & i & " - ktglkontrak required date." : GoTo selesai
            End If
            'kjeniskelamin(115) As Integer
            If (IsNumeric(dataRowUtama(115)) = False) Then
                result(2) = "Main Row : " & i & " - kjeniskelamin required numeric." : GoTo selesai
            End If
            'ktgllahir(117) As Date
            If (IsDate(dataRowUtama(117)) = False) Then
                result(2) = "Main Row : " & i & " - ktgllahir required date." : GoTo selesai
            End If
            'ktglnikah(118) As Date
            If (IsDate(dataRowUtama(118)) = False) Then
                result(2) = "Main Row : " & i & " - ktglnikah required date." : GoTo selesai
            End If
            'kkomisipenjualan(119) As Double
            If (IsNumeric(dataRowUtama(119)) = False) Then
                result(2) = "Main Row : " & i & " - kkomisipenjualan required numeric." : GoTo selesai
            End If
            'kinputuser(121) As Integer
            If (IsNumeric(dataRowUtama(121)) = False) Then
                result(2) = "Main Row : " & i & " - kinputuser required numeric." : GoTo selesai
            End If
            'kinputtgl(122) As DateTime
            If (IsDate(dataRowUtama(122)) = False) Then
                result(2) = "Main Row : " & i & " - kinputtgl required date." : GoTo selesai
            End If
            'kmodifikasiuser(123) As Integer
            If (IsNumeric(dataRowUtama(123)) = False) Then
                result(2) = "Main Row : " & i & " - kmodifikasiuser required numeric." : GoTo selesai
            End If
            'kmodifikasitgl(124) As DateTime
            If (IsDate(dataRowUtama(124)) = False) Then
                result(2) = "Main Row : " & i & " - kmodifikasitgl required date." : GoTo selesai
            End If
            'kcustomint1(135) As Integer
            If (IsNumeric(dataRowUtama(135)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomint1 required numeric." : GoTo selesai
            End If
            'kcustomint2(136) As Integer
            If (IsNumeric(dataRowUtama(136)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomint2 required numeric." : GoTo selesai
            End If
            'kcustomint3(137) As Integer
            If (IsNumeric(dataRowUtama(137)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomint3 required numeric." : GoTo selesai
            End If
            'kcustomdbl1(138) As Double
            If (IsNumeric(dataRowUtama(138)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdbl1 required numeric." : GoTo selesai
            End If
            'kcustomdbl2(139) As Double
            If (IsNumeric(dataRowUtama(139)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdbl2 required numeric." : GoTo selesai
            End If
            'kcustomdbl3(140) As Double
            If (IsNumeric(dataRowUtama(140)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdbl3 required numeric." : GoTo selesai
            End If
            'kcustomdate1(141) As Date
            If (IsDate(dataRowUtama(141)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdate1 required date." : GoTo selesai
            End If
            'kcustomdate2(142) As Date
            If (IsDate(dataRowUtama(142)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdate2 required date." : GoTo selesai
            End If
            'kcustomdate3(143) As Date
            If (IsDate(dataRowUtama(143)) = False) Then
                result(2) = "Main Row : " & i & " - kcustomdate3 required date." : GoTo selesai
            End If
            'ksinkron(144) As Integer
            If (IsNumeric(dataRowUtama(144)) = False) Then
                result(2) = "Main Row : " & i & " - ksinkron required numeric." : GoTo selesai
            End If

            'END OF VALIDASI TIPE DATA UTAMA ===================================================

            'VALIDASI DATA UTAMA =======================================================
            'kkode(1) As String
            If Len(dataRowUtama(1)) = 0 Then
                result(2) = "Main Row : " & i & " - kkode can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(1)) > 25 Then
                result(2) = "Main Row : " & i & " - kkode should not be more than 25 character." : GoTo selesai
            End If

            'knama(2) As String
            If Len(dataRowUtama(2)) = 0 Then
                result(2) = "Main Row : " & i & " - knama can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(2)) > 100 Then
                result(2) = "Main Row : " & i & " - knama should not be more than 100 character." : GoTo selesai
            End If

            'kkategori(3) As String
            If Len(dataRowUtama(3)) = 0 Then
                result(2) = "Main Row : " & i & " - kkategori can't be empty" : GoTo selesai
            End If
            If Len(dataRowUtama(3)) > 3 Then
                result(2) = "Main Row : " & i & " - kkategori should not be more than 3 character." : GoTo selesai
            End If

            'kaktiftgl(28) As Date
            If Len(dataRowUtama(28)) = 0 Then
                result(2) = "Main Row : " & i & " - kaktiftgl can't be empty" : GoTo selesai
            End If

            'kinputtgl(122) As DateTime
            If Len(dataRowUtama(122)) = 0 Then
                result(2) = "Main Row : " & i & " - kinputtgl can't be empty" : GoTo selesai
            End If

            'kmodifikasitgl(124) As DateTime
            If Len(dataRowUtama(124)) = 0 Then
                result(2) = "Main Row : " & i & " - kmodifikasitgl can't be empty" : GoTo selesai
            End If

            'kcustomdbl1(138) As Double
            If Len(dataRowUtama(138)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdbl1 can't be empty" : GoTo selesai
            End If

            'kcustomdbl2(139) As Double
            If Len(dataRowUtama(139)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdbl2 can't be empty" : GoTo selesai
            End If

            'kcustomdbl3(140) As Double
            If Len(dataRowUtama(140)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdbl3 can't be empty" : GoTo selesai
            End If

            'kcustomdate1(141) As Date
            If Len(dataRowUtama(141)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdate1 can't be empty" : GoTo selesai
            End If

            'kcustomdate2(142) As Date
            If Len(dataRowUtama(142)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdate2 can't be empty" : GoTo selesai
            End If

            'kcustomdate3(143) As Date
            If Len(dataRowUtama(143)) = 0 Then
                result(2) = "Main Row : " & i & " - kcustomdate3 can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA UTAMA ================================================


            If AsDataTableTambahData(dtutama, "kid~kkode~knama~kkategori~kkategorinama~kcabang~kcabangnama~klokasi~klokasinama~kgudang~kgudangnama~kkategorisalesman~kkategorisalesmannama~karea~kareanama~kkategoricustomer~kkategoricustomernama~kkategorisupplier~kkategorisuppliernama~kdivisi~kdivisinama~ksubdivisi~ksubdivisinama~ksalesman~ksalesmannama~kkontakperson~kterminglobal~kaktif~kaktiftgl~k1alamat1~k1alamat2~k1alamat3~k1alamat4~k1alamat5~k1kota~k1propinsi~k1kodepos~k1negara~k1kontakperson~k1kontaknohp~k1kontakemail~k1notelp1~k1notelp2~k1nofax~k1email~k1website~k2alamat1~k2alamat2~k2alamat3~k2alamat4~k2alamat5~k2propinsi~k2kota~k2kodepos~k2negara~k2kontakperson~k2kontaknohp~k2kontakemail~k2notelp1~k2notelp2~k2nofax~k2email~k2website~k3alamat1~k3alamat2~k3alamat3~k3alamat4~k3alamat5~k3kota~k3propinsi~k3kodepos~k3negara~k3kontakperson~k3kontaknohp~k3kontakemail~k3notelp1~k3notelp2~k3nofax~k3email~k3website~k4alamat1~k4alamat2~k4alamat3~k4alamat4~k4alamat5~k4kota~k4propinsi~k4kodepos~k4negara~k4kontakperson~k4kontaknohp~k4kontakemail~k4notelp1~k4notelp2~k4nofax~k4email~k4website~knpwp~kpkp~kbatashutang~kterminbeli~krekhutang~kbagpembelian~kfobbeli~kviabeli~kbataspiutang~kterminjual~krekpiutang~kbagpenjualan~ktingkatjual~kfobjual~kviajual~ktglkontrak~kbank~knorekening~kjeniskelamin~kmatauang~ktgllahir~ktglnikah~kkomisipenjualan~kcatatan~kinputuser~kinputtgl~kmodifikasiuser~kmodifikasitgl~kcustomtext1~kcustomtext2~kcustomtext3~kcustomtext4~kcustomtext5~kcustomtext6~kcustomtext7~kcustomtext8~kcustomtext9~kcustomtext10~kcustomint1~kcustomint2~kcustomint3~kcustomdbl1~kcustomdbl2~kcustomdbl3~kcustomdate1~kcustomdate2~kcustomdate3~ksinkron", dataRowUtama(0) & "~" & dataRowUtama(1) & "~" & dataRowUtama(2) & "~" & dataRowUtama(3) & "~" & dataRowUtama(4) & "~" & dataRowUtama(5) & "~" & dataRowUtama(6) & "~" & dataRowUtama(7) & "~" & dataRowUtama(8) & "~" & dataRowUtama(9) & "~" & dataRowUtama(10) & "~" & dataRowUtama(11) & "~" & dataRowUtama(12) & "~" & dataRowUtama(13) & "~" & dataRowUtama(14) & "~" & dataRowUtama(15) & "~" & dataRowUtama(16) & "~" & dataRowUtama(17) & "~" & dataRowUtama(18) & "~" & dataRowUtama(19) & "~" & dataRowUtama(20) & "~" & dataRowUtama(21) & "~" & dataRowUtama(22) & "~" & dataRowUtama(23) & "~" & dataRowUtama(24) & "~" & dataRowUtama(25) & "~" & dataRowUtama(26) & "~" & dataRowUtama(27) & "~" & dataRowUtama(28) & "~" & dataRowUtama(29) & "~" & dataRowUtama(30) & "~" & dataRowUtama(31) & "~" & dataRowUtama(32) & "~" & dataRowUtama(33) & "~" & dataRowUtama(34) & "~" & dataRowUtama(35) & "~" & dataRowUtama(36) & "~" & dataRowUtama(37) & "~" & dataRowUtama(38) & "~" & dataRowUtama(39) & "~" & dataRowUtama(40) & "~" & dataRowUtama(41) & "~" & dataRowUtama(42) & "~" & dataRowUtama(43) & "~" & dataRowUtama(44) & "~" & dataRowUtama(45) & "~" & dataRowUtama(46) & "~" & dataRowUtama(47) & "~" & dataRowUtama(48) & "~" & dataRowUtama(49) & "~" & dataRowUtama(50) & "~" & dataRowUtama(51) & "~" & dataRowUtama(52) & "~" & dataRowUtama(53) & "~" & dataRowUtama(54) & "~" & dataRowUtama(55) & "~" & dataRowUtama(56) & "~" & dataRowUtama(57) & "~" & dataRowUtama(58) & "~" & dataRowUtama(59) & "~" & dataRowUtama(60) & "~" & dataRowUtama(61) & "~" & dataRowUtama(62) & "~" & dataRowUtama(63) & "~" & dataRowUtama(64) & "~" & dataRowUtama(65) & "~" & dataRowUtama(66) & "~" & dataRowUtama(67) & "~" & dataRowUtama(68) & "~" & dataRowUtama(69) & "~" & dataRowUtama(70) & "~" & dataRowUtama(71) & "~" & dataRowUtama(72) & "~" & dataRowUtama(73) & "~" & dataRowUtama(74) & "~" & dataRowUtama(75) & "~" & dataRowUtama(76) & "~" & dataRowUtama(77) & "~" & dataRowUtama(78) & "~" & dataRowUtama(79) & "~" & dataRowUtama(80) & "~" & dataRowUtama(81) & "~" & dataRowUtama(82) & "~" & dataRowUtama(83) & "~" & dataRowUtama(84) & "~" & dataRowUtama(85) & "~" & dataRowUtama(86) & "~" & dataRowUtama(87) & "~" & dataRowUtama(88) & "~" & dataRowUtama(89) & "~" & dataRowUtama(90) & "~" & dataRowUtama(91) & "~" & dataRowUtama(92) & "~" & dataRowUtama(93) & "~" & dataRowUtama(94) & "~" & dataRowUtama(95) & "~" & dataRowUtama(96) & "~" & dataRowUtama(97) & "~" & dataRowUtama(98) & "~" & dataRowUtama(99) & "~" & dataRowUtama(100) & "~" & dataRowUtama(101) & "~" & dataRowUtama(102) & "~" & dataRowUtama(103) & "~" & dataRowUtama(104) & "~" & dataRowUtama(105) & "~" & dataRowUtama(106) & "~" & dataRowUtama(107) & "~" & dataRowUtama(108) & "~" & dataRowUtama(109) & "~" & dataRowUtama(110) & "~" & dataRowUtama(111) & "~" & dataRowUtama(112) & "~" & dataRowUtama(113) & "~" & dataRowUtama(114) & "~" & dataRowUtama(115) & "~" & dataRowUtama(116) & "~" & dataRowUtama(117) & "~" & dataRowUtama(118) & "~" & dataRowUtama(119) & "~" & dataRowUtama(120) & "~" & dataRowUtama(121) & "~" & dataRowUtama(122) & "~" & dataRowUtama(123) & "~" & dataRowUtama(124) & "~" & dataRowUtama(125) & "~" & dataRowUtama(126) & "~" & dataRowUtama(127) & "~" & dataRowUtama(128) & "~" & dataRowUtama(129) & "~" & dataRowUtama(130) & "~" & dataRowUtama(131) & "~" & dataRowUtama(132) & "~" & dataRowUtama(133) & "~" & dataRowUtama(134) & "~" & dataRowUtama(135) & "~" & dataRowUtama(136) & "~" & dataRowUtama(137) & "~" & dataRowUtama(138) & "~" & dataRowUtama(139) & "~" & dataRowUtama(140) & "~" & dataRowUtama(141) & "~" & dataRowUtama(142) & "~" & dataRowUtama(143) & "~" & dataRowUtama(144)) = False Then
                result(2) = "Main Row : " & i & " - Insert into main datatable failed." : GoTo selesai
            End If

        Next


        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'kaid(0) As Integer, kaidkontak(1) As Integer, kakodekontak(2) As String, kanama(3) As String, kajabatan(4) As String, 
        'kanotelp(5) As String, kanofax(6) As String, kanohp(7) As String, kaemail(8) As String, kawebsite(9) As String, 
        'kamessenger(10) As String, kaalamat(11) As String, katgllahir(12) As Date, katglnikah(13) As Date, kacatatan(14) As String, 
        'kadefault(15) As Integer, kainputuser(16) As Integer, kainputtgl(17) As DateTime, kamodifikasiuser(18) As Integer, kamodifikasitgl(19) As DateTime


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, 
        'kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, 
        'kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl


        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(1).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "kaid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaidkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kakodekontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kajabatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanotelp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanofax", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kanohp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaemail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kawebsite", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kamessenger", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kaalamat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "katgllahir", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "katglnikah", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kacatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kadefault", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "kamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtdetail, "kamodifikasitgl", AsEnumTypeData.AsString)


        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        Dim JmlDtDetail As Integer = dataDetail.Length
        For i = 1 To JmlDtDetail
            'SPLIT DATA DETAIL
            dataRowDetail = dataDetail(i - 1).Split(sptField)

            'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
            'CEK ARRAY DATA DETAIL
            If (dataRowDetail.Length <> 20) Then
                result(2) = "Row : " & i & " - Invalid detail transaction data parameter." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'kaid(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - kaid required numeric." : GoTo selesai
            End If
            'kaidkontak(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - kaidkontak required numeric." : GoTo selesai
            End If
            'katgllahir(12) As Date
            If (IsDate(dataRowDetail(12)) = False) Then
                result(2) = "Row : " & i & " - katgllahir required date." : GoTo selesai
            End If
            'katglnikah(13) As Date
            If (IsDate(dataRowDetail(13)) = False) Then
                result(2) = "Row : " & i & " - katglnikah required date." : GoTo selesai
            End If
            'kadefault(15) As Integer
            If (IsNumeric(dataRowDetail(15)) = False) Then
                result(2) = "Row : " & i & " - kadefault required numeric." : GoTo selesai
            End If
            'kainputuser(16) As Integer
            If (IsNumeric(dataRowDetail(16)) = False) Then
                result(2) = "Row : " & i & " - kainputuser required numeric." : GoTo selesai
            End If
            'kainputtgl(17) As DateTime
            If (IsDate(dataRowDetail(17)) = False) Then
                result(2) = "Row : " & i & " - kainputtgl required date." : GoTo selesai
            End If
            'kamodifikasiuser(18) As Integer
            If (IsNumeric(dataRowDetail(18)) = False) Then
                result(2) = "Row : " & i & " - kamodifikasiuser required numeric." : GoTo selesai
            End If
            'kamodifikasitgl(19) As DateTime
            If (IsDate(dataRowDetail(19)) = False) Then
                result(2) = "Row : " & i & " - kamodifikasitgl required date." : GoTo selesai
            End If
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            'kakodekontak(2) As String
            If Len(dataRowDetail(2)) = 0 Then
                result(2) = "Row : " & i & " - kakodekontak can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(2)) > 25 Then
                result(2) = "Row : " & i & " - kakodekontak should not be more than 25 character." : GoTo selesai
            End If

            'kanama(3) As String
            If Len(dataRowDetail(3)) = 0 Then
                result(2) = "Row : " & i & " - kanama can't be empty" : GoTo selesai
            End If
            If Len(dataRowDetail(3)) > 25 Then
                result(2) = "Row : " & i & " - kanama should not be more than 25 character." : GoTo selesai
            End If

            'kainputtgl(17) As DateTime
            If Len(dataRowDetail(17)) = 0 Then
                result(2) = "Row : " & i & " - kainputtgl can't be empty" : GoTo selesai
            End If

            'kamodifikasitgl(19) As DateTime
            If Len(dataRowDetail(19)) = 0 Then
                result(2) = "Row : " & i & " - kamodifikasitgl can't be empty" : GoTo selesai
            End If
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "kaid~kaidkontak~kakodekontak~kanama~kajabatan~kanotelp~kanofax~kanohp~kaemail~kawebsite~kamessenger~kaalamat~katgllahir~katglnikah~kacatatan~kadefault~kainputuser~kainputtgl~kamodifikasiuser~kamodifikasitgl", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If

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
                'Hapus utama 
                sql = "Delete from M1_Contact"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Hapus detail 
                sql = "Delete from M1_Contact_Attention"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses utama
                Dim strValue1 As New StringBuilder
                For Each dr1 As DataRow In dtutama.Rows
                    strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", ", "))
                    strValue1.Append("('" & FixQuotes(dr1("kid")) & "', '" & FixQuotes(dr1("kkode")) & "', '" & FixQuotes(dr1("knama")) & "', '" & FixQuotes(dr1("kkategori")) & "', '" & FixQuotes(dr1("kkategorinama")) & "', '" & FixQuotes(dr1("kcabang")) & "', '" & FixQuotes(dr1("kcabangnama")) & "', '" & FixQuotes(dr1("klokasi")) & "', '" & FixQuotes(dr1("klokasinama")) & "', '" & FixQuotes(dr1("kgudang")) & "', '" & FixQuotes(dr1("kgudangnama")) & "', '" & FixQuotes(dr1("kkategorisalesman")) & "', '" & FixQuotes(dr1("kkategorisalesmannama")) & "', '" & FixQuotes(dr1("karea")) & "', '" & FixQuotes(dr1("kareanama")) & "', '" & FixQuotes(dr1("kkategoricustomer")) & "', '" & FixQuotes(dr1("kkategoricustomernama")) & "', '" & FixQuotes(dr1("kkategorisupplier")) & "', '" & FixQuotes(dr1("kkategorisuppliernama")) & "', '" & FixQuotes(dr1("kdivisi")) & "', '" & FixQuotes(dr1("kdivisinama")) & "', '" & FixQuotes(dr1("ksubdivisi")) & "', '" & FixQuotes(dr1("ksubdivisinama")) & "', " & dr1("ksalesman") & ", '" & FixQuotes(dr1("ksalesmannama")) & "', '" & FixQuotes(dr1("kkontakperson")) & "', " & dr1("kterminglobal") & ", " & dr1("kaktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("kaktiftgl"))) & "', '" & FixQuotes(dr1("k1alamat1")) & "', '" & FixQuotes(dr1("k1alamat2")) & "', '" & FixQuotes(dr1("k1alamat3")) & "', '" & FixQuotes(dr1("k1alamat4")) & "', '" & FixQuotes(dr1("k1alamat5")) & "', '" & FixQuotes(dr1("k1kota")) & "', '" & FixQuotes(dr1("k1propinsi")) & "', '" & FixQuotes(dr1("k1kodepos")) & "', '" & FixQuotes(dr1("k1negara")) & "', '" & FixQuotes(dr1("k1kontakperson")) & "', '" & FixQuotes(dr1("k1kontaknohp")) & "', '" & FixQuotes(dr1("k1kontakemail")) & "', '" & FixQuotes(dr1("k1notelp1")) & "', '" & FixQuotes(dr1("k1notelp2")) & "', '" & FixQuotes(dr1("k1nofax")) & "', '" & FixQuotes(dr1("k1email")) & "', '" & FixQuotes(dr1("k1website")) & "', '" & FixQuotes(dr1("k2alamat1")) & "', '" & FixQuotes(dr1("k2alamat2")) & "', '" & FixQuotes(dr1("k2alamat3")) & "', '" & FixQuotes(dr1("k2alamat4")) & "', '" & FixQuotes(dr1("k2alamat5")) & "', '" & FixQuotes(dr1("k2propinsi")) & "', '" & FixQuotes(dr1("k2kota")) & "', '" & FixQuotes(dr1("k2kodepos")) & "', '" & FixQuotes(dr1("k2negara")) & "', '" & FixQuotes(dr1("k2kontakperson")) & "', '" & FixQuotes(dr1("k2kontaknohp")) & "', '" & FixQuotes(dr1("k2kontakemail")) & "', '" & FixQuotes(dr1("k2notelp1")) & "', '" & FixQuotes(dr1("k2notelp2")) & "', '" & FixQuotes(dr1("k2nofax")) & "', '" & FixQuotes(dr1("k2email")) & "', '" & FixQuotes(dr1("k2website")) & "', '" & FixQuotes(dr1("k3alamat1")) & "', '" & FixQuotes(dr1("k3alamat2")) & "', '" & FixQuotes(dr1("k3alamat3")) & "', '" & FixQuotes(dr1("k3alamat4")) & "', '" & FixQuotes(dr1("k3alamat5")) & "', '" & FixQuotes(dr1("k3kota")) & "', '" & FixQuotes(dr1("k3propinsi")) & "', '" & FixQuotes(dr1("k3kodepos")) & "', '" & FixQuotes(dr1("k3negara")) & "', '" & FixQuotes(dr1("k3kontakperson")) & "', '" & FixQuotes(dr1("k3kontaknohp")) & "', '" & FixQuotes(dr1("k3kontakemail")) & "', '" & FixQuotes(dr1("k3notelp1")) & "', '" & FixQuotes(dr1("k3notelp2")) & "', '" & FixQuotes(dr1("k3nofax")) & "', '" & FixQuotes(dr1("k3email")) & "', '" & FixQuotes(dr1("k3website")) & "', '" & FixQuotes(dr1("k4alamat1")) & "', '" & FixQuotes(dr1("k4alamat2")) & "', '" & FixQuotes(dr1("k4alamat3")) & "', '" & FixQuotes(dr1("k4alamat4")) & "', '" & FixQuotes(dr1("k4alamat5")) & "', '" & FixQuotes(dr1("k4kota")) & "', '" & FixQuotes(dr1("k4propinsi")) & "', '" & FixQuotes(dr1("k4kodepos")) & "', '" & FixQuotes(dr1("k4negara")) & "', '" & FixQuotes(dr1("k4kontakperson")) & "', '" & FixQuotes(dr1("k4kontaknohp")) & "', '" & FixQuotes(dr1("k4kontakemail")) & "', '" & FixQuotes(dr1("k4notelp1")) & "', '" & FixQuotes(dr1("k4notelp2")) & "', '" & FixQuotes(dr1("k4nofax")) & "', '" & FixQuotes(dr1("k4email")) & "', '" & FixQuotes(dr1("k4website")) & "', '" & FixQuotes(dr1("knpwp")) & "', " & dr1("kpkp") & ", '" & FixDouble(dr1("kbatashutang")) & "', '" & FixQuotes(dr1("kterminbeli")) & "', '" & FixQuotes(dr1("krekhutang")) & "', " & dr1("kbagpembelian") & ", '" & FixQuotes(dr1("kfobbeli")) & "', '" & FixQuotes(dr1("kviabeli")) & "', '" & FixDouble(dr1("kbataspiutang")) & "', '" & FixQuotes(dr1("kterminjual")) & "', '" & FixQuotes(dr1("krekpiutang")) & "', " & dr1("kbagpenjualan") & ", " & dr1("ktingkatjual") & ", '" & FixQuotes(dr1("kfobjual")) & "', '" & FixQuotes(dr1("kviajual")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ktglkontrak"))) & "', '" & FixQuotes(dr1("kbank")) & "', '" & FixQuotes(dr1("knorekening")) & "', " & dr1("kjeniskelamin") & ", '" & FixQuotes(dr1("kmatauang")) & "', '" & FixQuotes(AsFormatTanggal(dr1("ktgllahir"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("ktglnikah"))) & "', '" & FixDouble(dr1("kkomisipenjualan")) & "', '" & FixQuotes(dr1("kcatatan")) & "', " & dr1("kinputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kinputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("kmodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kmodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "', '" & FixQuotes(dr1("kcustomtext1")) & "', '" & FixQuotes(dr1("kcustomtext2")) & "', '" & FixQuotes(dr1("kcustomtext3")) & "', '" & FixQuotes(dr1("kcustomtext4")) & "', '" & FixQuotes(dr1("kcustomtext5")) & "', '" & FixQuotes(dr1("kcustomtext6")) & "', '" & FixQuotes(dr1("kcustomtext7")) & "', '" & FixQuotes(dr1("kcustomtext8")) & "', '" & FixQuotes(dr1("kcustomtext9")) & "', '" & FixQuotes(dr1("kcustomtext10")) & "', " & dr1("kcustomint1") & ", " & dr1("kcustomint2") & ", " & dr1("kcustomint3") & ", '" & FixDouble(dr1("kcustomdbl1")) & "', '" & FixDouble(dr1("kcustomdbl2")) & "', '" & FixDouble(dr1("kcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("kcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("kcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("kcustomdate3"))) & "', " & dr1("ksinkron") & ")")
                Next
                sql = "Insert into M1_Contact(kid, kkode, knama, kkategori, kkategorinama, kcabang, kcabangnama, klokasi, klokasinama, kgudang, kgudangnama, kkategorisalesman, kkategorisalesmannama, karea, kareanama, kkategoricustomer, kkategoricustomernama, kkategorisupplier, kkategorisuppliernama, kdivisi, kdivisinama, ksubdivisi, ksubdivisinama, ksalesman, ksalesmannama, kkontakperson, kterminglobal, kaktif, kaktiftgl, k1alamat1, k1alamat2, k1alamat3, k1alamat4, k1alamat5, k1kota, k1propinsi, k1kodepos, k1negara, k1kontakperson, k1kontaknohp, k1kontakemail, k1notelp1, k1notelp2, k1nofax, k1email, k1website, k2alamat1, k2alamat2, k2alamat3, k2alamat4, k2alamat5, k2propinsi, k2kota, k2kodepos, k2negara, k2kontakperson, k2kontaknohp, k2kontakemail, k2notelp1, k2notelp2, k2nofax, k2email, k2website, k3alamat1, k3alamat2, k3alamat3, k3alamat4, k3alamat5, k3kota, k3propinsi, k3kodepos, k3negara, k3kontakperson, k3kontaknohp, k3kontakemail, k3notelp1, k3notelp2, k3nofax, k3email, k3website, k4alamat1, k4alamat2, k4alamat3, k4alamat4, k4alamat5, k4kota, k4propinsi, k4kodepos, k4negara, k4kontakperson, k4kontaknohp, k4kontakemail, k4notelp1, k4notelp2, k4nofax, k4email, k4website, knpwp, kpkp, kbatashutang, kterminbeli, krekhutang, kbagpembelian, kfobbeli, kviabeli, kbataspiutang, kterminjual, krekpiutang, kbagpenjualan, ktingkatjual, kfobjual, kviajual, ktglkontrak, kbank, knorekening, kjeniskelamin, kmatauang, ktgllahir, ktglnikah, kkomisipenjualan, kcatatan, kinputuser, kinputtgl, kmodifikasiuser, kmodifikasitgl, kcustomtext1, kcustomtext2, kcustomtext3, kcustomtext4, kcustomtext5, kcustomtext6, kcustomtext7, kcustomtext8, kcustomtext9, kcustomtext10, kcustomint1, kcustomint2, kcustomint3, kcustomdbl1, kcustomdbl2, kcustomdbl3, kcustomdate1, kcustomdate2, kcustomdate3, ksinkron) values" & strValue1.ToString & ""
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()


                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("('" & dr1("kaid") & "', '" & FixQuotes(dr1("kaidkontak")) & "', '" & FixQuotes(dr1("kakodekontak")) & "', '" & FixQuotes(dr1("kanama")) & "', '" & FixQuotes(dr1("kajabatan")) & "', '" & FixQuotes(dr1("kanotelp")) & "', '" & FixQuotes(dr1("kanofax")) & "', '" & FixQuotes(dr1("kanohp")) & "', '" & FixQuotes(dr1("kaemail")) & "', '" & FixQuotes(dr1("kawebsite")) & "', '" & FixQuotes(dr1("kamessenger")) & "', '" & FixQuotes(dr1("kaalamat")) & "', '" & FixQuotes(AsFormatTanggal(dr1("katgllahir"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("katglnikah"))) & "', '" & FixQuotes(dr1("kacatatan")) & "', " & dr1("kadefault") & ", " & dr1("kainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("kamodifikasiuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("kamodifikasitgl"), "yyyy-MM-dd H:mm:ss")) & "')")
                    Next
                    sql = "Insert into M1_Contact_Attention(kaid, kaidkontak, kakodekontak, kanama, kajabatan, kanotelp, kanofax, kanohp, kaemail, kawebsite, kamessenger, kaalamat, katgllahir, katglnikah, kacatatan, kadefault, kainputuser, kainputtgl, kamodifikasiuser, kamodifikasitgl) values" & strValue2.ToString & ""
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