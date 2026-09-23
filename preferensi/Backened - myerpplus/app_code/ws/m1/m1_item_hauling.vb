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
Public Class m1_item_hauling
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_Item_HaulingSimpan(ByVal param As String) As String

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

        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

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
        'bid(0) As Integer, bkode(1) As String, bnama(2) As String, bnamaalias1(3) As String, bnamaalias2(4) As String, 
        'bnamaalias3(5) As String, bnamaalias4(6) As String, bnamaalias5(7) As String, btipe(8) As String, bjenis(9) As String, 
        'bjenisdetail(10) As Integer, bkategori(11) As String, bketerangan(12) As String, bsatuan(13) As String, bnilaisatuan(14) As Double, 
        'bsatuandefault(15) As String, bnilaisatuandefault(16) As Double, bhpp(17) As String, bcabang(18) As String, blokasi(19) As String, 
        'bdivisi(20) As String, bsubdivisi(21) As String, bgudang(22) As String, bproyek(23) As String, bsubitem(24) As Integer, 
        'bsubitemdari(25) As Integer, bbarcode(26) As String, bsuplier(27) As Integer, baktif(28) As Integer, baktiftgl(29) As Date, 
        'bstokminimal(30) As Double, bstokmaksimal(31) As Double, breorder(32) As Double, bjmlorderbeli(33) As Double, bjmlorderjual(34) As Double, 
        'bkategoriumur(35) As String, bstatusmoving(36) As String, bsifatharga(37) As String, bpromo(38) As Integer, bpromoberlaku(39) As Date, 
        'bpajakbeli(40) As String, bpajakjual(41) As String, bhargabeli(42) As Double, bhppaverage(43) As Double, bhargajual1(44) As Double, 
        'bhargajual2(45) As Double, bhargajual3(46) As Double, bhargajual4(47) As Double, bhargajual5(48) As Double, bdiskonjual1(49) As String, 
        'bdiskonjual2(50) As String, bdiskonjual3(51) As String, bdiskonjual4(52) As String, bdiskonjual5(53) As String, bstok(54) As Double, 
        'bkomisi(55) As Double, bmarginminimal(56) As Double, brekpersediaan(57) As String, brekpenjualan(58) As String, brekreturpenjualan(59) As String, 
        'brekdiskonpenjualan(60) As String, brekhargapokok(61) As String, brekreturpembelian(62) As String, brekdiskonpembelian(63) As String, brekkonsinyasi(64) As String, 
        'bastatus(65) As Integer, bahourmeter(66) As Double, bapanjang(67) As Double, balebar(68) As Double, batinggi(69) As Double, 
        'bavolume(70) As Double, baberat(71) As Double, bawarna(72) As String, baoem(73) As String, bamerk(74) As String, 
        'baukuran(75) As String, bamodel(76) As String, bakelas(77) As String, bserial(78) As Integer, bbatch(79) As Integer, 
        'bpengganti(80) As Integer, bgambar(81) As String, bedithpp(82) As Integer, burutan(83) As Integer, bcatatan(84) As String, 
        'binputuser(85) As Integer, binputtgl(86) As DateTime, bmodifikasiuser(87) As Integer, bmodifikasitgl(88) As DateTime, bcustomtext1(89) As String, 
        'bcustomtext2(90) As String, bcustomtext3(91) As String, bcustomtext4(92) As String, bcustomtext5(93) As String, bcustomtext6(94) As String, 
        'bcustomtext7(95) As String, bcustomtext8(96) As String, bcustomtext9(97) As String, bcustomtext10(98) As String, bcustomint1(99) As Integer, 
        'bcustomint2(100) As Integer, bcustomint3(101) As Integer, bcustomint4(102) As Integer, bcustomint5(103) As Integer, bcustomdbl1(104) As Double, 
        'bcustomdbl2(105) As Double, bcustomdbl3(106) As Double, bcustomdbl4(107) As Double, bcustomdbl5(108) As Double, bcustomdate1(109) As Date, 
        'bcustomdate2(110) As Date, bcustomdate3(111) As Date, bcustomdate4(112) As Date, bcustomdate5(113) As Date

        'MAPPING BUAT FLEX --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, 
        'bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, 
        'bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, 
        'bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, 
        'bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, 
        'bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, 
        'bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, 
        'bcustomdate4, bcustomdate5


        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 114) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ================================================


        'VALIDASI TIPE DATA ==========================================================
        'bid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "bid required numeric." : GoTo selesai
        End If
        'bjenisdetail(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "bjenisdetail required numeric." : GoTo selesai
        End If
        'bnilaisatuan(14) As Double
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "bnilaisatuan required numeric." : GoTo selesai
        End If
        'bnilaisatuandefault(16) As Double
        If (IsNumeric(dataUtama(16)) = False) Then
            result(2) = "bnilaisatuandefault required numeric." : GoTo selesai
        End If
        'bsubitem(24) As Integer
        If (IsNumeric(dataUtama(24)) = False) Then
            result(2) = "bsubitem required numeric." : GoTo selesai
        End If
        'bsubitemdari(25) As Integer
        If (IsNumeric(dataUtama(25)) = False) Then
            result(2) = "bsubitemdari required numeric." : GoTo selesai
        End If
        'bsuplier(27) As Integer
        If (IsNumeric(dataUtama(27)) = False) Then
            result(2) = "bsuplier required numeric." : GoTo selesai
        End If
        'baktif(28) As Integer
        If (IsNumeric(dataUtama(28)) = False) Then
            result(2) = "baktif required numeric." : GoTo selesai
        End If
        'baktiftgl(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "baktiftgl required date." : GoTo selesai
        End If
        'bstokminimal(30) As Double
        If (IsNumeric(dataUtama(30)) = False) Then
            result(2) = "bstokminimal required numeric." : GoTo selesai
        End If
        'bstokmaksimal(31) As Double
        If (IsNumeric(dataUtama(31)) = False) Then
            result(2) = "bstokmaksimal required numeric." : GoTo selesai
        End If
        'breorder(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "breorder required numeric." : GoTo selesai
        End If
        'bjmlorderbeli(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "bjmlorderbeli required numeric." : GoTo selesai
        End If
        'bjmlorderjual(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "bjmlorderjual required numeric." : GoTo selesai
        End If
        'bpromo(38) As Integer
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "bpromo required numeric." : GoTo selesai
        End If
        'bpromoberlaku(39) As Date
        If (IsDate(dataUtama(39)) = False) Then
            result(2) = "bpromoberlaku required date." : GoTo selesai
        End If
        'bhargabeli(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "bhargabeli required numeric." : GoTo selesai
        End If
        'bhppaverage(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "bhppaverage required numeric." : GoTo selesai
        End If
        'bhargajual1(44) As Double
        If (IsNumeric(dataUtama(44)) = False) Then
            result(2) = "bhargajual1 required numeric." : GoTo selesai
        End If
        'bhargajual2(45) As Double
        If (IsNumeric(dataUtama(45)) = False) Then
            result(2) = "bhargajual2 required numeric." : GoTo selesai
        End If
        'bhargajual3(46) As Double
        If (IsNumeric(dataUtama(46)) = False) Then
            result(2) = "bhargajual3 required numeric." : GoTo selesai
        End If
        'bhargajual4(47) As Double
        If (IsNumeric(dataUtama(47)) = False) Then
            result(2) = "bhargajual4 required numeric." : GoTo selesai
        End If
        'bhargajual5(48) As Double
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "bhargajual5 required numeric." : GoTo selesai
        End If
        'bstok(54) As Double
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "bstok required numeric." : GoTo selesai
        End If
        'bkomisi(55) As Double
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "bkomisi required numeric." : GoTo selesai
        End If
        'bmarginminimal(56) As Double
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "bmarginminimal required numeric." : GoTo selesai
        End If
        'bastatus(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "bastatus required numeric." : GoTo selesai
        End If
        'bahourmeter(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "bahourmeter required numeric." : GoTo selesai
        End If
        'bapanjang(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "bapanjang required numeric." : GoTo selesai
        End If
        'balebar(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "balebar required numeric." : GoTo selesai
        End If
        'batinggi(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "batinggi required numeric." : GoTo selesai
        End If
        'bavolume(70) As Double
        If (IsNumeric(dataUtama(70)) = False) Then
            result(2) = "bavolume required numeric." : GoTo selesai
        End If
        'baberat(71) As Double
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "baberat required numeric." : GoTo selesai
        End If
        'bserial(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "bserial required numeric." : GoTo selesai
        End If
        'bbatch(79) As Integer
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "bbatch required numeric." : GoTo selesai
        End If
        'bpengganti(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "bpengganti required numeric." : GoTo selesai
        End If
        'bedithpp(82) As Integer
        If (IsNumeric(dataUtama(82)) = False) Then
            result(2) = "bedithpp required numeric." : GoTo selesai
        End If
        'burutan(83) As Integer
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "burutan required numeric." : GoTo selesai
        End If
        'binputuser(85) As Integer
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "binputuser required numeric." : GoTo selesai
        End If
        'binputtgl(86) As DateTime
        If (IsDate(dataUtama(86)) = False) Then
            result(2) = "binputtgl required date." : GoTo selesai
        End If
        'bmodifikasiuser(87) As Integer
        If (IsNumeric(dataUtama(87)) = False) Then
            result(2) = "bmodifikasiuser required numeric." : GoTo selesai
        End If
        'bmodifikasitgl(88) As DateTime
        If (IsDate(dataUtama(88)) = False) Then
            result(2) = "bmodifikasitgl required date." : GoTo selesai
        End If
        'bcustomint1(99) As Integer
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "bcustomint1 required numeric." : GoTo selesai
        End If
        'bcustomint2(100) As Integer
        If (IsNumeric(dataUtama(100)) = False) Then
            result(2) = "bcustomint2 required numeric." : GoTo selesai
        End If
        'bcustomint3(101) As Integer
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "bcustomint3 required numeric." : GoTo selesai
        End If
        'bcustomint4(102) As Integer
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "bcustomint4 required numeric." : GoTo selesai
        End If
        'bcustomint5(103) As Integer
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "bcustomint5 required numeric." : GoTo selesai
        End If
        'bcustomdbl1(104) As Double
        If (IsNumeric(dataUtama(104)) = False) Then
            result(2) = "bcustomdbl1 required numeric." : GoTo selesai
        End If
        'bcustomdbl2(105) As Double
        If (IsNumeric(dataUtama(105)) = False) Then
            result(2) = "bcustomdbl2 required numeric." : GoTo selesai
        End If
        'bcustomdbl3(106) As Double
        If (IsNumeric(dataUtama(106)) = False) Then
            result(2) = "bcustomdbl3 required numeric." : GoTo selesai
        End If
        'bcustomdbl4(107) As Double
        If (IsNumeric(dataUtama(107)) = False) Then
            result(2) = "bcustomdbl4 required numeric." : GoTo selesai
        End If
        'bcustomdbl5(108) As Double
        If (IsNumeric(dataUtama(108)) = False) Then
            result(2) = "bcustomdbl5 required numeric." : GoTo selesai
        End If
        'bcustomdate1(109) As Date
        If (IsDate(dataUtama(109)) = False) Then
            result(2) = "bcustomdate1 required date." : GoTo selesai
        End If
        'bcustomdate2(110) As Date
        If (IsDate(dataUtama(110)) = False) Then
            result(2) = "bcustomdate2 required date." : GoTo selesai
        End If
        'bcustomdate3(111) As Date
        If (IsDate(dataUtama(111)) = False) Then
            result(2) = "bcustomdate3 required date." : GoTo selesai
        End If
        'bcustomdate4(112) As Date
        If (IsDate(dataUtama(112)) = False) Then
            result(2) = "bcustomdate4 required date." : GoTo selesai
        End If
        'bcustomdate5(113) As Date
        If (IsDate(dataUtama(113)) = False) Then
            result(2) = "bcustomdate5 required date." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================


        'VALIDASI DATA ===============================================================
        'bkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "bkode should not be more than 25 character." : GoTo selesai
        End If

        'bnama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 250 Then
            result(2) = "bnama should not be more than 250 character." : GoTo selesai
        End If

        'bjenis(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 5 Then
            result(2) = "bjenis should not be more than 5 character." : GoTo selesai
        End If

        'bsatuan(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bsatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "bsatuan should not be more than 25 character." : GoTo selesai
        End If

        'bnilaisatuan(14) As Double
        If Len(dataUtama(14)) = 0 Then
            result(2) = "bnilaisatuan can't be empty" : GoTo selesai
        End If

        'bsatuandefault(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bsatuandefault can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bsatuandefault should not be more than 25 character." : GoTo selesai
        End If

        'bnilaisatuandefault(16) As Double
        If Len(dataUtama(16)) = 0 Then
            result(2) = "bnilaisatuandefault can't be empty" : GoTo selesai
        End If

        'bhpp(17) As String
        If Len(dataUtama(17)) = 0 Then
            result(2) = "bhpp can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(17)) > 2 Then
            result(2) = "bhpp should not be more than 2 character." : GoTo selesai
        End If

        'baktiftgl(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "baktiftgl can't be empty" : GoTo selesai
        End If

        'bstokminimal(30) As Double
        If Len(dataUtama(30)) = 0 Then
            result(2) = "bstokminimal can't be empty" : GoTo selesai
        End If

        'bstokmaksimal(31) As Double
        If Len(dataUtama(31)) = 0 Then
            result(2) = "bstokmaksimal can't be empty" : GoTo selesai
        End If

        'breorder(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "breorder can't be empty" : GoTo selesai
        End If

        'bjmlorderbeli(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "bjmlorderbeli can't be empty" : GoTo selesai
        End If

        'bjmlorderjual(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "bjmlorderjual can't be empty" : GoTo selesai
        End If

        'bpromoberlaku(39) As Date
        If Len(dataUtama(39)) = 0 Then
            result(2) = "bpromoberlaku can't be empty" : GoTo selesai
        End If

        'bhargabeli(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "bhargabeli can't be empty" : GoTo selesai
        End If

        'bhppaverage(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "bhppaverage can't be empty" : GoTo selesai
        End If

        'bhargajual1(44) As Double
        If Len(dataUtama(44)) = 0 Then
            result(2) = "bhargajual1 can't be empty" : GoTo selesai
        End If

        'bhargajual2(45) As Double
        If Len(dataUtama(45)) = 0 Then
            result(2) = "bhargajual2 can't be empty" : GoTo selesai
        End If

        'bhargajual3(46) As Double
        If Len(dataUtama(46)) = 0 Then
            result(2) = "bhargajual3 can't be empty" : GoTo selesai
        End If

        'bhargajual4(47) As Double
        If Len(dataUtama(47)) = 0 Then
            result(2) = "bhargajual4 can't be empty" : GoTo selesai
        End If

        'bhargajual5(48) As Double
        If Len(dataUtama(48)) = 0 Then
            result(2) = "bhargajual5 can't be empty" : GoTo selesai
        End If

        'bdiskonjual1(49) As String
        If Len(dataUtama(49)) = 0 Then
            result(2) = "bdiskonjual1 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(49)) > 25 Then
            result(2) = "bdiskonjual1 should not be more than 25 character." : GoTo selesai
        End If

        'bdiskonjual2(50) As String
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bdiskonjual2 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(50)) > 25 Then
            result(2) = "bdiskonjual2 should not be more than 25 character." : GoTo selesai
        End If

        'bdiskonjual3(51) As String
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bdiskonjual3 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(51)) > 25 Then
            result(2) = "bdiskonjual3 should not be more than 25 character." : GoTo selesai
        End If

        'bdiskonjual4(52) As String
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bdiskonjual4 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(52)) > 25 Then
            result(2) = "bdiskonjual4 should not be more than 25 character." : GoTo selesai
        End If

        'bdiskonjual5(53) As String
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bdiskonjual5 can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(53)) > 25 Then
            result(2) = "bdiskonjual5 should not be more than 25 character." : GoTo selesai
        End If

        'bstok(54) As Double
        If Len(dataUtama(54)) = 0 Then
            result(2) = "bstok can't be empty" : GoTo selesai
        End If

        'bkomisi(55) As Double
        If Len(dataUtama(55)) = 0 Then
            result(2) = "bkomisi can't be empty" : GoTo selesai
        End If

        'brekpersediaan(57) As String
        If Len(dataUtama(57)) = 0 Then
            result(2) = "brekpersediaan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(57)) > 15 Then
            result(2) = "brekpersediaan should not be more than 15 character." : GoTo selesai
        End If

        'brekpenjualan(58) As String
        If Len(dataUtama(58)) = 0 Then
            result(2) = "brekpenjualan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(58)) > 15 Then
            result(2) = "brekpenjualan should not be more than 15 character." : GoTo selesai
        End If

        'brekreturpenjualan(59) As String
        If Len(dataUtama(59)) = 0 Then
            result(2) = "brekreturpenjualan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(59)) > 15 Then
            result(2) = "brekreturpenjualan should not be more than 15 character." : GoTo selesai
        End If

        'brekdiskonpenjualan(60) As String
        If Len(dataUtama(60)) = 0 Then
            result(2) = "brekdiskonpenjualan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(60)) > 15 Then
            result(2) = "brekdiskonpenjualan should not be more than 15 character." : GoTo selesai
        End If

        'brekhargapokok(61) As String
        If Len(dataUtama(61)) = 0 Then
            result(2) = "brekhargapokok can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(61)) > 15 Then
            result(2) = "brekhargapokok should not be more than 15 character." : GoTo selesai
        End If

        'brekreturpembelian(62) As String
        If Len(dataUtama(62)) = 0 Then
            result(2) = "brekreturpembelian can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(62)) > 15 Then
            result(2) = "brekreturpembelian should not be more than 15 character." : GoTo selesai
        End If

        'brekdiskonpembelian(63) As String
        If Len(dataUtama(63)) = 0 Then
            result(2) = "brekdiskonpembelian can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(63)) > 15 Then
            result(2) = "brekdiskonpembelian should not be more than 15 character." : GoTo selesai
        End If

        'brekkonsinyasi(64) As String
        If Len(dataUtama(64)) = 0 Then
            result(2) = "brekkonsinyasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(64)) > 15 Then
            result(2) = "brekkonsinyasi should not be more than 15 character." : GoTo selesai
        End If

        'bahourmeter(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "bahourmeter can't be empty" : GoTo selesai
        End If

        'bapanjang(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "bapanjang can't be empty" : GoTo selesai
        End If

        'balebar(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "balebar can't be empty" : GoTo selesai
        End If

        'batinggi(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "batinggi can't be empty" : GoTo selesai
        End If

        'bavolume(70) As Double
        If Len(dataUtama(70)) = 0 Then
            result(2) = "bavolume can't be empty" : GoTo selesai
        End If

        'baberat(71) As Double
        If Len(dataUtama(71)) = 0 Then
            result(2) = "baberat can't be empty" : GoTo selesai
        End If

        'binputtgl(86) As DateTime
        If Len(dataUtama(86)) = 0 Then
            result(2) = "binputtgl can't be empty" : GoTo selesai
        End If

        'bmodifikasitgl(88) As DateTime
        If Len(dataUtama(88)) = 0 Then
            result(2) = "bmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bcustomdbl1(104) As Double
        If Len(dataUtama(104)) = 0 Then
            result(2) = "bcustomdbl1 can't be empty" : GoTo selesai
        End If

        'bcustomdbl2(105) As Double
        If Len(dataUtama(105)) = 0 Then
            result(2) = "bcustomdbl2 can't be empty" : GoTo selesai
        End If

        'bcustomdbl3(106) As Double
        If Len(dataUtama(106)) = 0 Then
            result(2) = "bcustomdbl3 can't be empty" : GoTo selesai
        End If

        'bcustomdbl4(107) As Double
        If Len(dataUtama(107)) = 0 Then
            result(2) = "bcustomdbl4 can't be empty" : GoTo selesai
        End If

        'bcustomdbl5(108) As Double
        If Len(dataUtama(108)) = 0 Then
            result(2) = "bcustomdbl5 can't be empty" : GoTo selesai
        End If

        'bcustomdate1(109) As Date
        If Len(dataUtama(109)) = 0 Then
            result(2) = "bcustomdate1 can't be empty" : GoTo selesai
        End If

        'bcustomdate2(110) As Date
        If Len(dataUtama(110)) = 0 Then
            result(2) = "bcustomdate2 can't be empty" : GoTo selesai
        End If

        'bcustomdate3(111) As Date
        If Len(dataUtama(111)) = 0 Then
            result(2) = "bcustomdate3 can't be empty" : GoTo selesai
        End If

        'bcustomdate4(112) As Date
        If Len(dataUtama(112)) = 0 Then
            result(2) = "bcustomdate4 can't be empty" : GoTo selesai
        End If

        'bcustomdate5(113) As Date
        If Len(dataUtama(113)) = 0 Then
            result(2) = "bcustomdate5 can't be empty" : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bid) FROM M1_Item_Hauling WHERE bid=" & dataUtama(0))
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then

                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_item_hauling_history
                    Dim rsSimpanHistory As String = SimpanHistory.M1_Item_Hauling_HistorySimpan("" & paramSplit(0) & "★M1_Item_Hauling_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                    Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (rsSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Item_Hauling set bkode  = '" & FixQuotes(dataUtama(1)) & "', bnama  = '" & FixQuotes(dataUtama(2)) & "', bnamaalias1  = '" & FixQuotes(dataUtama(3)) & "', bnamaalias2  = '" & FixQuotes(dataUtama(4)) & "', bnamaalias3  = '" & FixQuotes(dataUtama(5)) & "', bnamaalias4  = '" & FixQuotes(dataUtama(6)) & "', bnamaalias5  = '" & FixQuotes(dataUtama(7)) & "', btipe  = '" & FixQuotes(dataUtama(8)) & "', bjenis  = '" & FixQuotes(dataUtama(9)) & "', bjenisdetail  = " & dataUtama(10) & ", bkategori  = '" & FixQuotes(dataUtama(11)) & "', bketerangan  = '" & FixQuotes(dataUtama(12)) & "', bsatuan  = '" & FixQuotes(dataUtama(13)) & "', bnilaisatuan  = '" & FixDouble(dataUtama(14)) & "', bsatuandefault  = '" & FixQuotes(dataUtama(15)) & "', bnilaisatuandefault  = '" & FixDouble(dataUtama(16)) & "', bhpp  = '" & FixQuotes(dataUtama(17)) & "', bcabang  = '" & FixQuotes(dataUtama(18)) & "', blokasi  = '" & FixQuotes(dataUtama(19)) & "', bdivisi  = '" & FixQuotes(dataUtama(20)) & "', bsubdivisi  = '" & FixQuotes(dataUtama(21)) & "', bgudang  = '" & FixQuotes(dataUtama(22)) & "', bproyek  = '" & FixQuotes(dataUtama(23)) & "', bsubitem  = " & dataUtama(24) & ", bsubitemdari  = " & dataUtama(25) & ", bbarcode  = '" & FixQuotes(dataUtama(26)) & "', bsuplier  = " & dataUtama(27) & ", baktif  = " & dataUtama(28) & ", baktiftgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(29))) & "', bstokminimal  = '" & FixDouble(dataUtama(30)) & "', bstokmaksimal  = '" & FixDouble(dataUtama(31)) & "', breorder  = '" & FixDouble(dataUtama(32)) & "', bjmlorderbeli  = '" & FixDouble(dataUtama(33)) & "', bjmlorderjual  = '" & FixDouble(dataUtama(34)) & "', bkategoriumur  = '" & FixQuotes(dataUtama(35)) & "', bstatusmoving  = '" & FixQuotes(dataUtama(36)) & "', bsifatharga  = '" & FixQuotes(dataUtama(37)) & "', bpromo  = " & dataUtama(38) & ", bpromoberlaku  = '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', bpajakbeli  = '" & FixQuotes(dataUtama(40)) & "', bpajakjual  = '" & FixQuotes(dataUtama(41)) & "', bhargabeli  = '" & FixDouble(dataUtama(42)) & "', bhppaverage  = '" & FixDouble(dataUtama(43)) & "', bhargajual1  = '" & FixDouble(dataUtama(44)) & "', bhargajual2  = '" & FixDouble(dataUtama(45)) & "', bhargajual3  = '" & FixDouble(dataUtama(46)) & "', bhargajual4  = '" & FixDouble(dataUtama(47)) & "', bhargajual5  = '" & FixDouble(dataUtama(48)) & "', bdiskonjual1  = '" & FixQuotes(dataUtama(49)) & "', bdiskonjual2  = '" & FixQuotes(dataUtama(50)) & "', bdiskonjual3  = '" & FixQuotes(dataUtama(51)) & "', bdiskonjual4  = '" & FixQuotes(dataUtama(52)) & "', bdiskonjual5  = '" & FixQuotes(dataUtama(53)) & "', bstok  = '" & FixDouble(dataUtama(54)) & "', bkomisi  = '" & FixDouble(dataUtama(55)) & "', bmarginminimal  = '" & FixDouble(dataUtama(56)) & "', brekpersediaan  = '" & FixQuotes(dataUtama(57)) & "', brekpenjualan  = '" & FixQuotes(dataUtama(58)) & "', brekreturpenjualan  = '" & FixQuotes(dataUtama(59)) & "', brekdiskonpenjualan  = '" & FixQuotes(dataUtama(60)) & "', brekhargapokok  = '" & FixQuotes(dataUtama(61)) & "', brekreturpembelian  = '" & FixQuotes(dataUtama(62)) & "', brekdiskonpembelian  = '" & FixQuotes(dataUtama(63)) & "', brekkonsinyasi  = '" & FixQuotes(dataUtama(64)) & "', bastatus  = " & dataUtama(65) & ", bahourmeter  = '" & FixDouble(dataUtama(66)) & "', bapanjang  = '" & FixDouble(dataUtama(67)) & "', balebar  = '" & FixDouble(dataUtama(68)) & "', batinggi  = '" & FixDouble(dataUtama(69)) & "', bavolume  = '" & FixDouble(dataUtama(70)) & "', baberat  = '" & FixDouble(dataUtama(71)) & "', bawarna  = '" & FixQuotes(dataUtama(72)) & "', baoem  = '" & FixQuotes(dataUtama(73)) & "', bamerk  = '" & FixQuotes(dataUtama(74)) & "', baukuran  = '" & FixQuotes(dataUtama(75)) & "', bamodel  = '" & FixQuotes(dataUtama(76)) & "', bakelas  = '" & FixQuotes(dataUtama(77)) & "', bserial  = " & dataUtama(78) & ", bbatch  = " & dataUtama(79) & ", bpengganti  = " & dataUtama(80) & ", bgambar  = '" & FixQuotes(dataUtama(81)) & "', bedithpp  = " & dataUtama(82) & ", burutan  = " & dataUtama(83) & ", bcatatan  = '" & FixQuotes(dataUtama(84)) & "', binputuser  = " & dataUtama(85) & ", bmodifikasiuser  = " & dataUtama(87) & ", bmodifikasitgl  = NOW(), bcustomtext1  = '" & FixQuotes(dataUtama(89)) & "', bcustomtext2  = '" & FixQuotes(dataUtama(90)) & "', bcustomtext3  = '" & FixQuotes(dataUtama(91)) & "', bcustomtext4  = '" & FixQuotes(dataUtama(92)) & "', bcustomtext5  = '" & FixQuotes(dataUtama(93)) & "', bcustomtext6  = '" & FixQuotes(dataUtama(94)) & "', bcustomtext7  = '" & FixQuotes(dataUtama(95)) & "', bcustomtext8  = '" & FixQuotes(dataUtama(96)) & "', bcustomtext9  = '" & FixQuotes(dataUtama(97)) & "', bcustomtext10  = '" & FixQuotes(dataUtama(98)) & "', bcustomint1  = " & dataUtama(99) & ", bcustomint2  = " & dataUtama(100) & ", bcustomint3  = " & dataUtama(101) & ", bcustomint4  = " & dataUtama(102) & ", bcustomint5  = " & dataUtama(103) & ", bcustomdbl1  = '" & FixDouble(dataUtama(104)) & "', bcustomdbl2  = '" & FixDouble(dataUtama(105)) & "', bcustomdbl3  = '" & FixDouble(dataUtama(106)) & "', bcustomdbl4  = '" & FixDouble(dataUtama(107)) & "', bcustomdbl5  = '" & FixDouble(dataUtama(108)) & "', bcustomdate1  = '" & FixQuotes(AsFormatTanggal(dataUtama(109))) & "', bcustomdate2  = '" & FixQuotes(AsFormatTanggal(dataUtama(110))) & "', bcustomdate3  = '" & FixQuotes(AsFormatTanggal(dataUtama(111))) & "', bcustomdate4  = '" & FixQuotes(AsFormatTanggal(dataUtama(112))) & "', bcustomdate5  = '" & FixQuotes(AsFormatTanggal(dataUtama(113))) & "' where bid = '" & dataUtama(0) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    result(2) = "Data not found." : GoTo selesai
                End If

            Else
                sql = "Insert into M1_Item_Hauling (bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, bcustomdate4, bcustomdate5) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', " & dataUtama(10) & ", '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', '" & FixDouble(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixDouble(dataUtama(16)) & "', '" & FixQuotes(dataUtama(17)) & "', '" & FixQuotes(dataUtama(18)) & "', '" & FixQuotes(dataUtama(19)) & "', '" & FixQuotes(dataUtama(20)) & "', '" & FixQuotes(dataUtama(21)) & "', '" & FixQuotes(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "', " & dataUtama(24) & ", " & dataUtama(25) & ", '" & FixQuotes(dataUtama(26)) & "', " & dataUtama(27) & ", " & dataUtama(28) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(29))) & "', '" & FixDouble(dataUtama(30)) & "', '" & FixDouble(dataUtama(31)) & "', '" & FixDouble(dataUtama(32)) & "', '" & FixDouble(dataUtama(33)) & "', '" & FixDouble(dataUtama(34)) & "', '" & FixQuotes(dataUtama(35)) & "', '" & FixQuotes(dataUtama(36)) & "', '" & FixQuotes(dataUtama(37)) & "', " & dataUtama(38) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', '" & FixQuotes(dataUtama(40)) & "', '" & FixQuotes(dataUtama(41)) & "', '" & FixDouble(dataUtama(42)) & "', '" & FixDouble(dataUtama(43)) & "', '" & FixDouble(dataUtama(44)) & "', '" & FixDouble(dataUtama(45)) & "', '" & FixDouble(dataUtama(46)) & "', '" & FixDouble(dataUtama(47)) & "', '" & FixDouble(dataUtama(48)) & "', '" & FixQuotes(dataUtama(49)) & "', '" & FixQuotes(dataUtama(50)) & "', '" & FixQuotes(dataUtama(51)) & "', '" & FixQuotes(dataUtama(52)) & "', '" & FixQuotes(dataUtama(53)) & "', '" & FixDouble(dataUtama(54)) & "', '" & FixDouble(dataUtama(55)) & "', '" & FixDouble(dataUtama(56)) & "', '" & FixQuotes(dataUtama(57)) & "', '" & FixQuotes(dataUtama(58)) & "', '" & FixQuotes(dataUtama(59)) & "', '" & FixQuotes(dataUtama(60)) & "', '" & FixQuotes(dataUtama(61)) & "', '" & FixQuotes(dataUtama(62)) & "', '" & FixQuotes(dataUtama(63)) & "', '" & FixQuotes(dataUtama(64)) & "', " & dataUtama(65) & ", '" & FixDouble(dataUtama(66)) & "', '" & FixDouble(dataUtama(67)) & "', '" & FixDouble(dataUtama(68)) & "', '" & FixDouble(dataUtama(69)) & "', '" & FixDouble(dataUtama(70)) & "', '" & FixDouble(dataUtama(71)) & "', '" & FixQuotes(dataUtama(72)) & "', '" & FixQuotes(dataUtama(73)) & "', '" & FixQuotes(dataUtama(74)) & "', '" & FixQuotes(dataUtama(75)) & "', '" & FixQuotes(dataUtama(76)) & "', '" & FixQuotes(dataUtama(77)) & "', " & dataUtama(78) & ", " & dataUtama(79) & ", " & dataUtama(80) & ", '" & FixQuotes(dataUtama(81)) & "', " & dataUtama(82) & ", " & dataUtama(83) & ", '" & FixQuotes(dataUtama(84)) & "', " & dataUtama(85) & ", NOW(), 0, '1971-01-01 00:00:00', '" & FixQuotes(dataUtama(89)) & "', '" & FixQuotes(dataUtama(90)) & "', '" & FixQuotes(dataUtama(91)) & "', '" & FixQuotes(dataUtama(92)) & "', '" & FixQuotes(dataUtama(93)) & "', '" & FixQuotes(dataUtama(94)) & "', '" & FixQuotes(dataUtama(95)) & "', '" & FixQuotes(dataUtama(96)) & "', '" & FixQuotes(dataUtama(97)) & "', '" & FixQuotes(dataUtama(98)) & "', " & dataUtama(99) & ", " & dataUtama(100) & ", " & dataUtama(101) & ", " & dataUtama(102) & ", " & dataUtama(103) & ", '" & FixDouble(dataUtama(104)) & "', '" & FixDouble(dataUtama(105)) & "', '" & FixDouble(dataUtama(106)) & "', '" & FixDouble(dataUtama(107)) & "', '" & FixDouble(dataUtama(108)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(109))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(110))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(111))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(112))) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(113))) & "')"
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
            Dim paramSearch As String = M1_Item_HaulingSearch(PostWsSearch(paramSplit(0), "M1_Item_HaulingSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Item_HaulingDelete(ByVal param As String) As String

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
            result(2) = "bid required numeric." : GoTo selesai
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
            Dim paramTerkait As String = M1_Item_HaulingTerkait(PostWsTerkait(paramSplit(0), "M1_Item_HaulingTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_item_hauling_history
            Dim rsSimpanHistory As String = SimpanHistory.M1_Item_Hauling_HistorySimpan("" & paramSplit(0) & "★M1_Item_Hauling_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE
            sql = "DELETE FROM M1_Item_Hauling WHERE bid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Item_HaulingSearch(PostWsSearch(paramSplit(0), "M1_Item_HaulingSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_Item_HaulingGetdataAll(ByVal param As String) As String
        'M1_Item_HaulingGetdataAll --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, 
        'bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, 
        'bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, 
        'bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, 
        'bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, 
        'bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, 
        'bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, 
        'bcustomdate4, bcustomdate5, bcabangnama, blokasinama, bgudangnama, bdivisinama, bsubdivisinama, 
        'bproyeknama

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

        'BUAT QUERY
        sql = "select `ih`.`bid` AS `bid`,`ih`.`bkode` AS `bkode`,`ih`.`bnama` AS `bnama`,`ih`.`bnamaalias1` AS `bnamaalias1`,`ih`.`bnamaalias2` AS `bnamaalias2`,`ih`.`bnamaalias3` AS `bnamaalias3`,`ih`.`bnamaalias4` AS `bnamaalias4`,`ih`.`bnamaalias5` AS `bnamaalias5`,`ih`.`btipe` AS `btipe`,`ih`.`bjenis` AS `bjenis`,`ih`.`bjenisdetail` AS `bjenisdetail`,`ih`.`bkategori` AS `bkategori`,`ih`.`bketerangan` AS `bketerangan`,`ih`.`bsatuan` AS `bsatuan`,`ih`.`bnilaisatuan` AS `bnilaisatuan`,`ih`.`bsatuandefault` AS `bsatuandefault`,`ih`.`bnilaisatuandefault` AS `bnilaisatuandefault`,`ih`.`bhpp` AS `bhpp`,`ih`.`bcabang` AS `bcabang`,`ih`.`blokasi` AS `blokasi`,`ih`.`bdivisi` AS `bdivisi`,`ih`.`bsubdivisi` AS `bsubdivisi`,`ih`.`bgudang` AS `bgudang`,`ih`.`bproyek` AS `bproyek`,`ih`.`bsubitem` AS `bsubitem`,`ih`.`bsubitemdari` AS `bsubitemdari`,`ih`.`bbarcode` AS `bbarcode`,`ih`.`bsuplier` AS `bsuplier`,`ih`.`baktif` AS `baktif`,`ih`.`baktiftgl` AS `baktiftgl`,`ih`.`bstokminimal` AS `bstokminimal`,`ih`.`bstokmaksimal` AS `bstokmaksimal`,`ih`.`breorder` AS `breorder`,`ih`.`bjmlorderbeli` AS `bjmlorderbeli`,`ih`.`bjmlorderjual` AS `bjmlorderjual`,`ih`.`bkategoriumur` AS `bkategoriumur`,`ih`.`bstatusmoving` AS `bstatusmoving`,`ih`.`bsifatharga` AS `bsifatharga`,`ih`.`bpromo` AS `bpromo`,`ih`.`bpromoberlaku` AS `bpromoberlaku`,`ih`.`bpajakbeli` AS `bpajakbeli`,`ih`.`bpajakjual` AS `bpajakjual`,`ih`.`bhargabeli` AS `bhargabeli`,`ih`.`bhppaverage` AS `bhppaverage`,`ih`.`bhargajual1` AS `bhargajual1`,`ih`.`bhargajual2` AS `bhargajual2`,`ih`.`bhargajual3` AS `bhargajual3`,`ih`.`bhargajual4` AS `bhargajual4`,`ih`.`bhargajual5` AS `bhargajual5`,`ih`.`bdiskonjual1` AS `bdiskonjual1`,`ih`.`bdiskonjual2` AS `bdiskonjual2`,`ih`.`bdiskonjual3` AS `bdiskonjual3`,`ih`.`bdiskonjual4` AS `bdiskonjual4`,`ih`.`bdiskonjual5` AS `bdiskonjual5`,`ih`.`bstok` AS `bstok`,`ih`.`bkomisi` AS `bkomisi`,`ih`.`bmarginminimal` AS `bmarginminimal`,`ih`.`brekpersediaan` AS `brekpersediaan`,`ih`.`brekpenjualan` AS `brekpenjualan`,`ih`.`brekreturpenjualan` AS `brekreturpenjualan`,`ih`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`,`ih`.`brekhargapokok` AS `brekhargapokok`,`ih`.`brekreturpembelian` AS `brekreturpembelian`,`ih`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`ih`.`brekkonsinyasi` AS `brekkonsinyasi`,`ih`.`bastatus` AS `bastatus`,`ih`.`bahourmeter` AS `bahourmeter`,`ih`.`bapanjang` AS `bapanjang`,`ih`.`balebar` AS `balebar`,`ih`.`batinggi` AS `batinggi`,`ih`.`bavolume` AS `bavolume`,`ih`.`baberat` AS `baberat`,`ih`.`bawarna` AS `bawarna`,`ih`.`baoem` AS `baoem`,`ih`.`bamerk` AS `bamerk`,`ih`.`baukuran` AS `baukuran`,`ih`.`bamodel` AS `bamodel`,`ih`.`bakelas` AS `bakelas`,`ih`.`bserial` AS `bserial`,`ih`.`bbatch` AS `bbatch`,`ih`.`bpengganti` AS `bpengganti`,`ih`.`bgambar` AS `bgambar`,`ih`.`bedithpp` AS `bedithpp`,`ih`.`burutan` AS `burutan`,`ih`.`bcatatan` AS `bcatatan`,`ih`.`binputuser` AS `binputuser`,`ih`.`binputtgl` AS `binputtgl`,`ih`.`bmodifikasiuser` AS `bmodifikasiuser`,`ih`.`bmodifikasitgl` AS `bmodifikasitgl`,`ih`.`bcustomtext1` AS `bcustomtext1`,`ih`.`bcustomtext2` AS `bcustomtext2`,`ih`.`bcustomtext3` AS `bcustomtext3`,`ih`.`bcustomtext4` AS `bcustomtext4`,`ih`.`bcustomtext5` AS `bcustomtext5`,`ih`.`bcustomtext6` AS `bcustomtext6`,`ih`.`bcustomtext7` AS `bcustomtext7`,`ih`.`bcustomtext8` AS `bcustomtext8`,`ih`.`bcustomtext9` AS `bcustomtext9`,`ih`.`bcustomtext10` AS `bcustomtext10`,`ih`.`bcustomint1` AS `bcustomint1`,`ih`.`bcustomint2` AS `bcustomint2`,`ih`.`bcustomint3` AS `bcustomint3`,`ih`.`bcustomint4` AS `bcustomint4`,`ih`.`bcustomint5` AS `bcustomint5`,`ih`.`bcustomdbl1` AS `bcustomdbl1`,`ih`.`bcustomdbl2` AS `bcustomdbl2`,`ih`.`bcustomdbl3` AS `bcustomdbl3`,`ih`.`bcustomdbl4` AS `bcustomdbl4`,`ih`.`bcustomdbl5` AS `bcustomdbl5`,`ih`.`bcustomdate1` AS `bcustomdate1`,`ih`.`bcustomdate2` AS `bcustomdate2`,`ih`.`bcustomdate3` AS `bcustomdate3`,`ih`.`bcustomdate4` AS `bcustomdate4`,`ih`.`bcustomdate5` AS `bcustomdate5`,`br`.`bnama` AS `bcabangnama`,`lc`.`lnama` AS `blokasinama`,`w`.`wnama` AS `bgudangnama`,`d`.`dnama` AS `bdivisinama`,`sd`.`sdnama` AS `bsubdivisinama`,`p`.`pnama` AS `bproyeknama` from ((((((`m1_item_hauling` `ih` left join `m1_branch` `br` on((`ih`.`bcabang` = `br`.`bkode`))) left join `m1_location` `lc` on((`ih`.`blokasi` = `lc`.`lkode`))) left join `m1_warehouse` `w` on((`ih`.`bgudang` = `w`.`wkode`))) left join `m1_division` `d` on((`ih`.`bdivisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`ih`.`bsubdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`ih`.`bproyek` = `p`.`pkode`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Hauling", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bnamaalias1"), ""), sptField,
                     FxDB(dr("bnamaalias2"), ""), sptField,
                     FxDB(dr("bnamaalias3"), ""), sptField,
                     FxDB(dr("bnamaalias4"), ""), sptField,
                     FxDB(dr("bnamaalias5"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bjenis"), ""), sptField,
                     FxDB(dr("bjenisdetail"), 0), sptField,
                     FxDB(dr("bkategori"), ""), sptField,
                     FxDB(dr("bketerangan"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("bsatuandefault"), ""), sptField,
                     FxDB(dr("bnilaisatuandefault"), 0), sptField,
                     FxDB(dr("bhpp"), ""), sptField,
                     FxDB(dr("bcabang"), ""), sptField,
                     FxDB(dr("blokasi"), ""), sptField,
                     FxDB(dr("bdivisi"), ""), sptField,
                     FxDB(dr("bsubdivisi"), ""), sptField,
                     FxDB(dr("bgudang"), ""), sptField,
                     FxDB(dr("bproyek"), ""), sptField,
                     FxDB(dr("bsubitem"), 0), sptField,
                     FxDB(dr("bsubitemdari"), ""), sptField,
                     FxDB(dr("bbarcode"), ""), sptField,
                     FxDB(dr("bsuplier"), ""), sptField,
                     FxDB(dr("baktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("bstokminimal"), 0), sptField,
                     FxDB(dr("bstokmaksimal"), 0), sptField,
                     FxDB(dr("breorder"), 0), sptField,
                     FxDB(dr("bjmlorderbeli"), 0), sptField,
                     FxDB(dr("bjmlorderjual"), 0), sptField,
                     FxDB(dr("bkategoriumur"), ""), sptField,
                     FxDB(dr("bstatusmoving"), ""), sptField,
                     FxDB(dr("bsifatharga"), ""), sptField,
                     FxDB(dr("bpromo"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bpromoberlaku"), ""), formatTgl), sptField,
                     FxDB(dr("bpajakbeli"), ""), sptField,
                     FxDB(dr("bpajakjual"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bhargajual2"), 0), sptField,
                     FxDB(dr("bhargajual3"), 0), sptField,
                     FxDB(dr("bhargajual4"), 0), sptField,
                     FxDB(dr("bhargajual5"), 0), sptField,
                     FxDB(dr("bdiskonjual1"), ""), sptField,
                     FxDB(dr("bdiskonjual2"), ""), sptField,
                     FxDB(dr("bdiskonjual3"), ""), sptField,
                     FxDB(dr("bdiskonjual4"), ""), sptField,
                     FxDB(dr("bdiskonjual5"), ""), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bkomisi"), 0), sptField,
                     FxDB(dr("bmarginminimal"), 0), sptField,
                     FxDB(dr("brekpersediaan"), ""), sptField,
                     FxDB(dr("brekpenjualan"), ""), sptField,
                     FxDB(dr("brekreturpenjualan"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                     FxDB(dr("brekhargapokok"), ""), sptField,
                     FxDB(dr("brekreturpembelian"), ""), sptField,
                     FxDB(dr("brekdiskonpembelian"), ""), sptField,
                     FxDB(dr("brekkonsinyasi"), ""), sptField,
                     FxDB(dr("bastatus"), 0), sptField,
                     FxDB(dr("bahourmeter"), 0), sptField,
                     FxDB(dr("bapanjang"), 0), sptField,
                     FxDB(dr("balebar"), 0), sptField,
                     FxDB(dr("batinggi"), 0), sptField,
                     FxDB(dr("bavolume"), 0), sptField,
                     FxDB(dr("baberat"), 0), sptField,
                     FxDB(dr("bawarna"), ""), sptField,
                     FxDB(dr("baoem"), ""), sptField,
                     FxDB(dr("bamerk"), ""), sptField,
                     FxDB(dr("baukuran"), ""), sptField,
                     FxDB(dr("bamodel"), ""), sptField,
                     FxDB(dr("bakelas"), ""), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bpengganti"), ""), sptField,
                     FxDB(dr("bgambar"), ""), sptField,
                     FxDB(dr("bedithpp"), 0), sptField,
                     FxDB(dr("burutan"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bcustomtext1"), ""), sptField,
                     FxDB(dr("bcustomtext2"), ""), sptField,
                     FxDB(dr("bcustomtext3"), ""), sptField,
                     FxDB(dr("bcustomtext4"), ""), sptField,
                     FxDB(dr("bcustomtext5"), ""), sptField,
                     FxDB(dr("bcustomtext6"), ""), sptField,
                     FxDB(dr("bcustomtext7"), ""), sptField,
                     FxDB(dr("bcustomtext8"), ""), sptField,
                     FxDB(dr("bcustomtext9"), ""), sptField,
                     FxDB(dr("bcustomtext10"), ""), sptField,
                     FxDB(dr("bcustomint1"), 0), sptField,
                     FxDB(dr("bcustomint2"), 0), sptField,
                     FxDB(dr("bcustomint3"), 0), sptField,
                     FxDB(dr("bcustomint4"), 0), sptField,
                     FxDB(dr("bcustomint5"), 0), sptField,
                     FxDB(dr("bcustomdbl1"), 0), sptField,
                     FxDB(dr("bcustomdbl2"), 0), sptField,
                     FxDB(dr("bcustomdbl3"), 0), sptField,
                     FxDB(dr("bcustomdbl4"), 0), sptField,
                     FxDB(dr("bcustomdbl5"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate3"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate4"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("bcustomdate5"), ""), formatTgl), sptField,
                     FxDB(dr("bcabangnama"), ""), sptField,
                     FxDB(dr("blokasinama"), ""), sptField,
                     FxDB(dr("bgudangnama"), ""), sptField,
                     FxDB(dr("bdivisinama"), ""), sptField,
                     FxDB(dr("bsubdivisinama"), ""), sptField,
                     FxDB(dr("bproyeknama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Hauling data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bastatus, bahourmeter, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, bedithpp, burutan, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bcustomtext1, bcustomtext2, bcustomtext3, bcustomtext4, bcustomtext5, bcustomtext6, bcustomtext7, bcustomtext8, bcustomtext9, bcustomtext10, bcustomint1, bcustomint2, bcustomint3, bcustomint4, bcustomint5, bcustomdbl1, bcustomdbl2, bcustomdbl3, bcustomdbl4, bcustomdbl5, bcustomdate1, bcustomdate2, bcustomdate3, bcustomdate4, bcustomdate5, bcabangnama, blokasinama, bgudangnama, bdivisinama, bsubdivisinama, bproyeknama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HaulingSearch(ByVal param As String) As String
        'M1_Item_HaulingSearch --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, 
        'bastatus, bahourmeter, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, 
        'binputusernama, bmodifikasiusernama

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


        'BUAT QUERY
        sql = "select `ih`.`bid` AS `bid`,`ih`.`bkode` AS `bkode`,`ih`.`bnama` AS `bnama`,`ih`.`bnamaalias1` AS `bnamaalias1`,`ih`.`bnamaalias2` AS `bnamaalias2`,`ih`.`bnamaalias3` AS `bnamaalias3`,`ih`.`bnamaalias4` AS `bnamaalias4`,`ih`.`bnamaalias5` AS `bnamaalias5`,`ih`.`btipe` AS `btipe`,`ih`.`bketerangan` AS `bketerangan`,`ih`.`bsatuan` AS `bsatuan`,`ih`.`bnilaisatuan` AS `bnilaisatuan`,`ih`.`bsatuandefault` AS `bsatuandefault`,`ih`.`bnilaisatuandefault` AS `bnilaisatuandefault`,`ih`.`bastatus` AS `bastatus`,`ih`.`bahourmeter` AS `bahourmeter`,`ih`.`bcatatan` AS `bcatatan`,`ih`.`binputuser` AS `binputuser`,`ih`.`binputtgl` AS `binputtgl`,`ih`.`bmodifikasiuser` AS `bmodifikasiuser`,`ih`.`bmodifikasitgl` AS `bmodifikasitgl`,`u1`.`unama` AS `binputusernama`,`u2`.`unama` AS `bmodifikasiusernama` from ((`m1_item_hauling` `ih` left join `m0_user` `u1` on((`ih`.`binputuser` = `u1`.`userid`))) left join `m0_user` `u2` on((`ih`.`bmodifikasiuser` = `u2`.`userid`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item_Hauling", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), ""), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bnamaalias1"), ""), sptField,
                     FxDB(dr("bnamaalias2"), ""), sptField,
                     FxDB(dr("bnamaalias3"), ""), sptField,
                     FxDB(dr("bnamaalias4"), ""), sptField,
                     FxDB(dr("bnamaalias5"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bketerangan"), ""), sptField,
                     FxDB(dr("bsatuan"), ""), sptField,
                     FxDB(dr("bnilaisatuan"), 0), sptField,
                     FxDB(dr("bsatuandefault"), ""), sptField,
                     FxDB(dr("bnilaisatuandefault"), 0), sptField,
                     FxDB(dr("bastatus"), 0), sptField,
                     FxDB(dr("bahourmeter"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("binputusernama"), ""), sptField,
                     FxDB(dr("bmodifikasiusernama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item Hauling data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bastatus, bahourmeter, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, binputusernama, bmodifikasiusernama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HaulingCekId(ByVal param As String) As String

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
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "bkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        'CEK DI DATABASE ================================================================
        Dim dt As DataTable
        Dim exist As Integer = 0
        dt = AsDataTableAmbilDariDB("SELECT COUNT(bkode) FROM m1_item_hauling WHERE bkode='" & idtransaksi & "'")
        exist = dt.Rows(0)(0)

        If (exist > 0) Then
            result(2) = "'" & idtransaksi & "' already exist for column bkode." : GoTo selesai
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
    Public Function M1_Item_HaulingTerkait(ByVal param As String) As String
        'M1_Item_HaulingTerkait --------------------------------------------------------
        'bkode, bnama, sumber, idterkait

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
            result(2) = "bkode can't be empty." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================


        Dim query As String = "(SELECT ih.bkode, ih.bnama, rf.rfsumber as sumber, rf.rfnotransaksi as idterkait FROM m1_item_hauling ih JOIN m3_rf_detail rfd ON ih.bid = rfd.idbarang JOIN m3_rf rf ON rfd.idrf = rf.rfid WHERE ih.bid = 'valkode' GROUP BY rf.rfid) UNION ALL (SELECT ih.bkode, ih.bnama, dc.dcsumber as sumber, dc.dcnotransaksi as idterkait FROM m1_item_hauling ih JOIN m3_dc dc ON ih.bid = dc.dcidbarang WHERE ih.bid = 'valkode' GROUP BY dc.dcid)"
        query = query.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , query) ' Ambil data ke databases
        pg1 = pg1

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("bkode"), ""), sptField,
                             FxDB(dr("bnama"), ""), sptField,
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
            result(2) = "Related Item Hauling data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bkode, bnama, sumber, idterkait"))

        Return wsResult
    End Function

End Class