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
Public Class m1_item
    Inherits System.Web.Services.WebService
    Public ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi

    <WebMethod()>
    Public Function M1_ItemSimpan2(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama(), dataILW(), dataRowILW(), dataIA(), dataRowIA(), dataIS(), dataRowIS() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim kode As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

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
        'bhargajual2(45) As Double, bhargajual3(46) As Double, bhargajual4(47) As Double, bhargajual5(48) As Double, bdiskonjual1(49) As Double, 
        'bdiskonjual2(50) As Double, bdiskonjual3(51) As Double, bdiskonjual4(52) As Double, bdiskonjual5(53) As Double, bstok(54) As Double, 
        'bkomisi(55) As Double, bmarginminimal(56) As Double, brekpersediaan(57) As String, brekpenjualan(58) As String, brekreturpenjualan(59) As String, 
        'brekdiskonpenjualan(60) As String, brekhargapokok(61) As String, brekreturpembelian(62) As String, brekdiskonpembelian(63) As String, brekkonsinyasi(64) As String, 
        'bapanjang(65) As Double, balebar(66) As Double, batinggi(67) As Double, bavolume(68) As Double, baberat(69) As Double, 
        'bawarna(70) As String, baoem(71) As String, bamerk(72) As String, baukuran(73) As String, bamodel(74) As String, 
        'bakelas(75) As String, bserial(76) As Integer, bbatch(77) As Integer, bpengganti(78) As Integer, bgambar(79) As String, 
        'burutan(80) As Integer, bcustom1(81) As String, bcustom2(82) As String, bcustom3(83) As String, bcustom4(84) As String, 
        'bcustom5(85) As String, bcustom6(86) As String, bcustom7(87) As String, bcustom8(88) As String, bcustom9(89) As String, 
        'bcustom10(90) As String, bcustom11(91) As Double, bcustom12(92) As Double, bcustom13(93) As Double, bcustom14(94) As Double, 
        'bcustom15(95) As Double, bcatatan(96) As String, binputuser(97) As Integer, binputtgl(98) As DateTime, bmodifikasiuser(99) As Integer, 
        'bmodifikasitgl(100) As DateTime, bedithpp(101) As Integer, bmobile(102) As Integer, bassembly(103) As Integer,
        'bkelasproduk(104) As String, bretur(105) As Integer, btag(106) As String, bminorder(107) As Double, 
        'bdepartemen (108) As String, bsubdepartemen (109) As String, bkp(110) As Integer, bkl(111) As Integer, bjmllapangan(112) As Double, bsatuanlapangan(113) As Double,
        'bsubkelas(114) As String, bmaterial(115) As String, bsection(116) As String, bvendor(117) As String, bdesigner(118) As String, basset(119) As Integer,
        'bhargajual6(120) As Double, bhargajual7(121) As Double, bhargajual8(122) As Double, bhargajual9(123) As Double, bhargajual10(124) As Double, 
        'bdiskonjual6(125) As Double, bdiskonjual7(126) As Double, bdiskonjual8(127) As Double, bdiskonjual9(128) As Double, bdiskonjual10(129) As Double

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, 
        'bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, 
        'bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, 
        'bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, 
        'bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, 
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile, bassembly,
        'bkelasproduk, bretur, btag, bminorder, bdepartemen, bsubdepartemen, bkp, bkl, bjmllapangan, bsatuanlapangan,
        'bsubkelas, bmaterial, bsection, bvendor, bdesigner, basset,
        'bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, 
        'bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 130 And dataUtama.Length <> 140) Then
            result(2) = "Invalid item data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
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
        Else
            Dim vItemAct As String = HakAksesCustom(1, 3, "Item Active", userid)
            If Len(vItemAct) > 0 And dataUtama(28) <> 0 Then
                result(2) = vItemAct : GoTo selesai
            End If
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
        ''bdiskonjual1(49) As Double
        'If (IsNumeric(dataUtama(49)) = False) Then
        '    result(2) = "bdiskonjual1 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual2(50) As Double
        'If (IsNumeric(dataUtama(50)) = False) Then
        '    result(2) = "bdiskonjual2 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual3(51) As Double
        'If (IsNumeric(dataUtama(51)) = False) Then
        '    result(2) = "bdiskonjual3 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual4(52) As Double
        'If (IsNumeric(dataUtama(52)) = False) Then
        '    result(2) = "bdiskonjual4 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual5(53) As Double
        'If (IsNumeric(dataUtama(53)) = False) Then
        '    result(2) = "bdiskonjual5 required numeric." : GoTo selesai
        'End If
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
        'bapanjang(65) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "bapanjang required numeric." : GoTo selesai
        End If
        'balebar(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "balebar required numeric." : GoTo selesai
        End If
        'batinggi(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "batinggi required numeric." : GoTo selesai
        End If
        'bavolume(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "bavolume required numeric." : GoTo selesai
        End If
        'baberat(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "baberat required numeric." : GoTo selesai
        End If
        'bserial(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "bserial required numeric." : GoTo selesai
        End If
        'bbatch(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "bbatch required numeric." : GoTo selesai
        End If
        'bpengganti(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "bpengganti required numeric." : GoTo selesai
        End If
        'burutan(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "burutan required numeric." : GoTo selesai
        End If
        'bcustom11(91) As Integer
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "bcustom11 required numeric." : GoTo selesai
        End If
        'bcustom12(92) As Integer
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "bcustom12 required numeric." : GoTo selesai
        End If
        'bcustom13(93) As Integer
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "bcustom13 required numeric." : GoTo selesai
        End If
        'bcustom14(94) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "bcustom14 required numeric." : GoTo selesai
        End If
        'bcustom15(95) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "bcustom15 required numeric." : GoTo selesai
        End If
        'binputuser(97) As Integer
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "binputuser required numeric." : GoTo selesai
        End If
        'binputtgl(98) As DateTime
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "binputtgl required date." : GoTo selesai
        End If
        'bmodifikasiuser(99) As Integer
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "bmodifikasiuser required numeric." : GoTo selesai
        End If
        'bmodifikasitgl(100) As DateTime
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "bmodifikasitgl required date." : GoTo selesai
        End If
        'bedithpp(101) As Integer
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "bedithpp required numeric." : GoTo selesai
        End If
        'bmobile(102) As Integer
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "bmobile required numeric." : GoTo selesai
        End If
        'bassembly(103) As Integer
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "bassembly required numeric." : GoTo selesai
        End If
        'bretur(105) As Integer
        If (IsNumeric(dataUtama(105)) = False) Then
            result(2) = "bretur required numeric." : GoTo selesai
        End If
        'bminorder(107) As Double
        If (IsNumeric(dataUtama(107)) = False) Then
            result(2) = "bminorder required numeric." : GoTo selesai
        End If
        'bkp (110) As Integer
        If (IsNumeric(dataUtama(110)) = False) Then
            result(2) = "bkp required numeric." : GoTo selesai
        End If
        'bkl(111) As Integer
        If (IsNumeric(dataUtama(111)) = False) Then
            result(2) = "bkl required numeric." : GoTo selesai
        End If
        'bjmllapangan(112) As Double
        If (IsNumeric(dataUtama(112)) = False) Then
            result(2) = "bjmllapangan required numeric." : GoTo selesai
        End If
        'basset(119) As Integer
        If (IsNumeric(dataUtama(119)) = False) Then
            result(2) = "basset required numeric." : GoTo selesai
        End If

        If dataUtama.Length > 120 Then

            'bhargajual6(120) As Double
            If (IsNumeric(dataUtama(120)) = False) Then
                result(2) = "bhargajual6 required numeric." : GoTo selesai
            End If
            'bhargajual7(121) As Double 
            If (IsNumeric(dataUtama(121)) = False) Then
                result(2) = "bhargajual7 required numeric." : GoTo selesai
            End If
            'bhargajual8(122) As Double 
            If (IsNumeric(dataUtama(122)) = False) Then
                result(2) = "bhargajual8 required numeric." : GoTo selesai
            End If
            'bhargajual9(123) As Double 
            If (IsNumeric(dataUtama(123)) = False) Then
                result(2) = "bhargajual9 required numeric." : GoTo selesai
            End If
            'bhargajual10(124) As Double
            If (IsNumeric(dataUtama(124)) = False) Then
                result(2) = "bhargajual10 required numeric." : GoTo selesai
            End If

            'bcustom16(124) As Double
            If (IsNumeric(dataUtama(124)) = False) Then
                result(2) = "bhargajual10 required numeric." : GoTo selesai
            End If

            'bhargajual10(124) As Double
            If (IsNumeric(dataUtama(124)) = False) Then
                result(2) = "bhargajual10 required numeric." : GoTo selesai
            End If



        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'bkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "bkode should not be more than 100 character." : GoTo selesai
        End If

        'bnama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "bnama should not be more than 100 character." : GoTo selesai
        End If

        'btipe(8) As String
        'If Len(dataUtama(8)) = 0 Then
        '    result(2) = "btipe can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(8)) > 100 Then
            result(2) = "btipe should not be more than 100 character." : GoTo selesai
        End If

        'bjenis(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 5 Then
            result(2) = "bjenis should not be more than 5 character." : GoTo selesai
        End If

        'bkategori(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "bkategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "bkategori should not be more than 50 character." : GoTo selesai
        End If

        'bsatuan(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bsatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "bsatuan should not be more than 25 character." : GoTo selesai
        End If

        'bsatuandefault(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bsatuandefault can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bsatuandefault should not be more than 25 character." : GoTo selesai
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

        'bdiskonjual1(49) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "bdiskonjual1 can't be empty" : GoTo selesai
        End If

        'bdiskonjual2(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bdiskonjual2 can't be empty" : GoTo selesai
        End If

        'bdiskonjual3(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bdiskonjual3 can't be empty" : GoTo selesai
        End If

        'bdiskonjual4(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bdiskonjual4 can't be empty" : GoTo selesai
        End If

        'bdiskonjual5(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bdiskonjual5 can't be empty" : GoTo selesai
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

        'bapanjang(65) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "bapanjang can't be empty" : GoTo selesai
        End If

        'balebar(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "balebar can't be empty" : GoTo selesai
        End If

        'batinggi(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "batinggi can't be empty" : GoTo selesai
        End If

        'bavolume(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "bavolume can't be empty" : GoTo selesai
        End If

        'baberat(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "baberat can't be empty" : GoTo selesai
        End If

        'bcustom14(94) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "bcustom14 can't be empty" : GoTo selesai
        End If

        'bcustom15(95) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "bcustom15 can't be empty" : GoTo selesai
        End If

        'binputtgl(98) As DateTime
        If Len(dataUtama(98)) = 0 Then
            result(2) = "binputtgl can't be empty" : GoTo selesai
        End If

        'bmodifikasitgl(100) As DateTime
        If Len(dataUtama(100)) = 0 Then
            result(2) = "bmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bkelasproduk(104) As String
        If Len(dataUtama(104)) = 0 Then
            result(2) = "bkelasproduk can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(104)) > 25 Then
            result(2) = "bkelasproduk should not be more than 25 character." : GoTo selesai
        End If

        'btag(106) As String
        If Len(dataUtama(106)) = 0 Then
            result(2) = "btag can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(106)) > 25 Then
            result(2) = "btag should not be more than 25 character." : GoTo selesai
        End If

        If Len(dataUtama(113)) = 0 Then
            result(2) = "bsatuanlapangan can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(113)) > 50 Then
            result(2) = "bsatuanlapangan should not be more than 50 character." : GoTo selesai
        End If

        If dataUtama.Length > 120 Then

            'bhargajual6(120) As Double
            If Len(dataUtama(120)) = 0 Then
                result(2) = "bhargajual6 can't be empty" : GoTo selesai
            End If

            'bhargajual7(121) As Double 
            If Len(dataUtama(121)) = 0 Then
                result(2) = "bhargajual7 can't be empty" : GoTo selesai
            End If

            'bhargajual8(122) As Double 
            If Len(dataUtama(122)) = 0 Then
                result(2) = "bhargajual8 can't be empty" : GoTo selesai
            End If

            'bhargajual9(123) As Double 
            If Len(dataUtama(123)) = 0 Then
                result(2) = "bhargajual9 can't be empty" : GoTo selesai
            End If

            'bhargajual10(124) As Double
            If Len(dataUtama(124)) = 0 Then
                result(2) = "bhargajual10 can't be empty" : GoTo selesai
            End If

            'bdiskonjual6(125) As Double 
            If Len(dataUtama(125)) = 0 Then
                result(2) = "bdiskonjual6 can't be empty" : GoTo selesai
            End If

            'bdiskonjual7(126) As Double 
            If Len(dataUtama(126)) = 0 Then
                result(2) = "bdiskonjual7 can't be empty" : GoTo selesai
            End If

            'bdiskonjual8(127) As Double
            If Len(dataUtama(127)) = 0 Then
                result(2) = "bdiskonjual8 can't be empty" : GoTo selesai
            End If

            'bdiskonjual9(128) As Double
            If Len(dataUtama(128)) = 0 Then
                result(2) = "bdiskonjual9 can't be empty" : GoTo selesai
            End If

            'bdiskonjual10(129) As Double
            If Len(dataUtama(129)) = 0 Then
                result(2) = "bdiskonjual10 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(130)) = 0 Then
                result(2) = "bcustom16 can't be empty" : GoTo selesai
            End If

            'bcustom17(131) As Double
            If Len(dataUtama(131)) = 0 Then
                result(2) = "bcustom17 can't be empty" : GoTo selesai
            End If

            'bcustom16(132) As Double
            If Len(dataUtama(132)) = 0 Then
                result(2) = "bcustom18 can't be empty" : GoTo selesai
            End If

            'bcustom16(133) As Double
            If Len(dataUtama(133)) = 0 Then
                result(2) = "bcustom19 can't be empty" : GoTo selesai
            End If

            'bcustom16(134) As Double
            If Len(dataUtama(134)) = 0 Then
                result(2) = "bcustom20 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(135)) = 0 Then
                result(2) = "bcustom21 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(136)) = 0 Then
                result(2) = "bcustom22 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(137)) = 0 Then
                result(2) = "bcustom23 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(138)) = 0 Then
                result(2) = "bcustom24 can't be empty" : GoTo selesai
            End If

            'bcustom16(130) As Double
            If Len(dataUtama(139)) = 0 Then
                result(2) = "bcustom25 can't be empty" : GoTo selesai
            End If

        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "btipe", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjenisdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bketerangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuandefault", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnilaisatuandefault", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "blokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubitem", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsubitemdari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bbarcode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsuplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "baktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "baktiftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "breorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjmlorderbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjmlorderjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkategoriumur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstatusmoving", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsifatharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpromo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bpromoberlaku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpajakbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpajakjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhppaverage", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkomisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmarginminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekreturpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekreturpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekkonsinyasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bapanjang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "balebar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "batinggi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bavolume", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baberat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bawarna", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baoem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bamerk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baukuran", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bamodel", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bakelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bserial", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bbatch", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bpengganti", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bgambar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "burutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bcustom1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom11", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom12", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom13", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "binputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "binputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bedithpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bmobile", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bassembly", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkelasproduk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bretur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "btag", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdepartemen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubdepartemen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bjmllapangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuanlapangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmaterial", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsection", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bvendor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdesigner", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "basset", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bhargajual6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom16", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom17", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom18", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom19", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom20", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom21", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom22", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom23", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom24", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom25", AsEnumTypeData.AsString)

        If dataUtama.Length > 120 Then
            If AsDataTableTambahData(dtutama, "bid~bkode~bnama~bnamaalias1~bnamaalias2~bnamaalias3~bnamaalias4~bnamaalias5~btipe~bjenis~bjenisdetail~bkategori~bketerangan~bsatuan~bnilaisatuan~bsatuandefault~bnilaisatuandefault~bhpp~bcabang~blokasi~bdivisi~bsubdivisi~bgudang~bproyek~bsubitem~bsubitemdari~bbarcode~bsuplier~baktif~baktiftgl~bstokminimal~bstokmaksimal~breorder~bjmlorderbeli~bjmlorderjual~bkategoriumur~bstatusmoving~bsifatharga~bpromo~bpromoberlaku~bpajakbeli~bpajakjual~bhargabeli~bhppaverage~bhargajual1~bhargajual2~bhargajual3~bhargajual4~bhargajual5~bdiskonjual1~bdiskonjual2~bdiskonjual3~bdiskonjual4~bdiskonjual5~bstok~bkomisi~bmarginminimal~brekpersediaan~brekpenjualan~brekreturpenjualan~brekdiskonpenjualan~brekhargapokok~brekreturpembelian~brekdiskonpembelian~brekkonsinyasi~bapanjang~balebar~batinggi~bavolume~baberat~bawarna~baoem~bamerk~baukuran~bamodel~bakelas~bserial~bbatch~bpengganti~bgambar~burutan~bcustom1~bcustom2~bcustom3~bcustom4~bcustom5~bcustom6~bcustom7~bcustom8~bcustom9~bcustom10~bcustom11~bcustom12~bcustom13~bcustom14~bcustom15~bcatatan~binputuser~binputtgl~bmodifikasiuser~bmodifikasitgl~bedithpp~bmobile~bassembly~bkelasproduk~bretur~btag~bminorder~bdepartemen~bsubdepartemen~bkp~bkl~bjmllapangan~bsatuanlapangan~bsubkelas~bmaterial~bsection~bvendor~bdesigner~basset~bhargajual6~bhargajual7~bhargajual8~bhargajual9~bhargajual10~bdiskonjual6~bdiskonjual7~bdiskonjual8~bdiskonjual9~bdiskonjual10~bcustom16~bcustom17~bcustom18~bcustom19~bcustom20~bcustom21~bcustom22~bcustom23~bcustom24~bcustom25", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & dataUtama(120) & "~" & dataUtama(121) & "~" & dataUtama(122) & "~" & dataUtama(123) & "~" & dataUtama(124) & "~" & dataUtama(125) & "~" & dataUtama(126) & "~" & dataUtama(127) & "~" & dataUtama(128) & "~" & dataUtama(129) & "~" & dataUtama(130) & "~" & dataUtama(131) & "~" & dataUtama(132) & "~" & dataUtama(133) & "~" & dataUtama(134) & "~" & dataUtama(135) & "~" & dataUtama(136) & "~" & dataUtama(137) & "~" & dataUtama(138) & "~" & dataUtama(139)) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        Else
            If AsDataTableTambahData(dtutama, "bid~bkode~bnama~bnamaalias1~bnamaalias2~bnamaalias3~bnamaalias4~bnamaalias5~btipe~bjenis~bjenisdetail~bkategori~bketerangan~bsatuan~bnilaisatuan~bsatuandefault~bnilaisatuandefault~bhpp~bcabang~blokasi~bdivisi~bsubdivisi~bgudang~bproyek~bsubitem~bsubitemdari~bbarcode~bsuplier~baktif~baktiftgl~bstokminimal~bstokmaksimal~breorder~bjmlorderbeli~bjmlorderjual~bkategoriumur~bstatusmoving~bsifatharga~bpromo~bpromoberlaku~bpajakbeli~bpajakjual~bhargabeli~bhppaverage~bhargajual1~bhargajual2~bhargajual3~bhargajual4~bhargajual5~bdiskonjual1~bdiskonjual2~bdiskonjual3~bdiskonjual4~bdiskonjual5~bstok~bkomisi~bmarginminimal~brekpersediaan~brekpenjualan~brekreturpenjualan~brekdiskonpenjualan~brekhargapokok~brekreturpembelian~brekdiskonpembelian~brekkonsinyasi~bapanjang~balebar~batinggi~bavolume~baberat~bawarna~baoem~bamerk~baukuran~bamodel~bakelas~bserial~bbatch~bpengganti~bgambar~burutan~bcustom1~bcustom2~bcustom3~bcustom4~bcustom5~bcustom6~bcustom7~bcustom8~bcustom9~bcustom10~bcustom11~bcustom12~bcustom13~bcustom14~bcustom15~bcatatan~binputuser~binputtgl~bmodifikasiuser~bmodifikasitgl~bedithpp~bmobile~bassembly~bkelasproduk~bretur~btag~bminorder~bdepartemen~bsubdepartemen~bkp~bkl~bjmllapangan~bsatuanlapangan~bsubkelas~bmaterial~bsection~bvendor~bdesigner~basset~bhargajual6~bhargajual7~bhargajual8~bhargajual9~bhargajual10~bdiskonjual6~bdiskonjual7~bdiskonjual8~bdiskonjual9~bdiskonjual10", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118) & "~" & dataUtama(119) & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0 & "~" & 0) = False Then
                result(2) = "Insert into main datatable failed." : GoTo selesai
            End If
        End If
        


        '********************************* ITEM LOCATION WAREHOUSE *********************************

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'blgidbarang(0) As Integer, blgkodebarang(1) As String, blggudang(2) As String, blgidlokasi(3) As Integer, blgkodelokasi(4) As String, 
        'blgnamalokasi(5) As String, blginputuser(6) As Integer, blginputtgl(7) As DateTime, blgmodifikasiuser(8) As Integer, blgmodifikasitgl(9) As DateTime

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, 
        'blginputtgl, blgmodifikasiuser, blgmodifikasitgl

        'Buat datatable detail
        Dim dtILW As New DataTable
        AsDataTableTambahField(dtILW, "blgidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtILW, "blgkodebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtILW, "blggudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtILW, "blgidlokasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtILW, "blgkodelokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtILW, "blgnamalokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtILW, "blginputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtILW, "blginputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtILW, "blgmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtILW, "blgmodifikasitgl", AsEnumTypeData.AsString)

        If (Len(dataSplit(1)) <> 0) Then

            'VALIDASI DAN SET DATA DETAIL ======================================================
            'SPLIT PARAMETER DATA DETAIL
            dataILW = dataSplit(1).Split(sptRow)
            'END OF VALIDASI DAN SET DATA DETAIL ===============================================

            'VALIDASI DAN SET DATA ROW DETAIL ==================================================
            Dim JmldtILW As Integer = dataILW.Length
            For i = 1 To JmldtILW
                'SPLIT DATA DETAIL
                dataRowILW = dataILW(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowILW.Length <> 10) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - Invalid item location warehouse data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------
                'blgidbarang(0) As Integer
                If (IsNumeric(dataRowILW(0)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgidbarang required numeric." : GoTo selesai
                End If
                'blgidlokasi(3) As Integer
                If (IsNumeric(dataRowILW(3)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgidlokasi required numeric." : GoTo selesai
                End If
                'blginputuser(6) As Integer
                If (IsNumeric(dataRowILW(6)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blginputuser required numeric." : GoTo selesai
                End If
                'blginputtgl(7) As DateTime
                If (IsDate(dataRowILW(7)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blginputtgl required date." : GoTo selesai
                End If
                'blgmodifikasiuser(8) As Integer
                If (IsNumeric(dataRowILW(8)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasiuser required numeric." : GoTo selesai
                End If
                'blgmodifikasitgl(9) As DateTime
                If (IsDate(dataRowILW(9)) = False) Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasitgl required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'blgkodebarang(1) As String
                If Len(dataRowILW(1)) = 0 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgkodebarang can't be empty" : GoTo selesai
                End If
                If Len(dataRowILW(1)) > 100 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgkodebarang should not be more than 100 character." : GoTo selesai
                End If

                'blggudang(2) As String
                If Len(dataRowILW(2)) = 0 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blggudang can't be empty" : GoTo selesai
                End If
                If Len(dataRowILW(2)) > 25 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blggudang should not be more than 25 character." : GoTo selesai
                End If

                'blginputtgl(7) As DateTime
                If Len(dataRowILW(7)) = 0 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blginputtgl can't be empty" : GoTo selesai
                End If

                'blgmodifikasitgl(9) As DateTime
                If Len(dataRowILW(9)) = 0 Then
                    result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasitgl can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtILW, "blgidbarang~blgkodebarang~blggudang~blgidlokasi~blgkodelokasi~blgnamalokasi~blginputuser~blginputtgl~blgmodifikasiuser~blgmodifikasitgl", dataRowILW(0) & "~" & dataRowILW(1) & "~" & dataRowILW(2) & "~" & dataRowILW(3) & "~" & dataRowILW(4) & "~" & dataRowILW(5) & "~" & dataRowILW(6) & "~" & dataRowILW(7) & "~" & dataRowILW(8) & "~" & dataRowILW(9)) = False Then
                    result(2) = "Item Location Warehouse Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================
        End If

        '***************************** END OF ITEM LOCATION WAREHOUSE ******************************



        '************************************* ITEM ASSEMBLY ***************************************

        'MAPPING BUAT WS DATA ASSEMBLY -------------------------------------------------------
        'iaidbarang(0) As Integer, iakodebarang(1) As String, iaidbarangpenyusun(2) As Integer, iakodebarangpenyusun(3) As String, iaurutan(4) As Integer, 
        'iajml(5) As Double, iasatuan(6) As String, iainputuser(7) As Integer, iainputtgl(8) As DateTime, iamodifikasiuser(9) As Integer, 
        'iamodifikasitgl(10) As DateTime

        'MAPPING BUAT FLEX DATA ASSEMBLY -----------------------------------------------------
        'iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, 
        'iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl

        'Buat datatable detail
        Dim dtIA As New DataTable
        AsDataTableTambahField(dtIA, "iaidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIA, "iakodebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIA, "iaidbarangpenyusun", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIA, "iakodebarangpenyusun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIA, "iaurutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIA, "iajml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIA, "iasatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIA, "iainputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIA, "iainputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIA, "iamodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIA, "iamodifikasitgl", AsEnumTypeData.AsString)

        'JIKA BARANG ASSEMBLY LANGSUNG MAKA WAJIB ISI BARANG PENYUSUN
        'bassembly(103) As Integer
        If Double.Parse(dataUtama(103)) = 1 And Len(dataSplit(2)) = 0 Then
            result(2) = "Item Assembly data not found." : GoTo selesai
        End If


        If (Len(dataSplit(2)) <> 0) Then
            'VALIDASI DAN SET DATA ASSEMBLY ======================================================
            'SPLIT PARAMETER DATA ASSEMBLY
            dataIA = dataSplit(2).Split(sptRow)
            'END OF VALIDASI DAN SET DATA ASSEMBLY ===============================================

            'VALIDASI DAN SET DATA ROW DETAIL ==================================================
            Dim JmldtIA As Integer = dataIA.Length
            For i = 1 To JmldtIA
                'SPLIT DATA DETAIL
                dataRowIA = dataIA(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
                'CEK ARRAY DATA DETAIL
                If (dataRowIA.Length <> 11) Then
                    result(2) = "Item Assembly Row : " & i & " - Invalid item assembly data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

                'VALIDASI TIPE DATA DETAIL ------------------------------------------
                'iaidbarang(0) As Integer
                If (IsNumeric(dataRowIA(0)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iaidbarang required numeric." : GoTo selesai
                End If
                'iaidbarangpenyusun(2) As Integer
                If (IsNumeric(dataRowIA(2)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iaidbarangpenyusun required numeric." : GoTo selesai
                End If
                'iaurutan(4) As Integer
                If (IsNumeric(dataRowIA(4)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iaurutan required numeric." : GoTo selesai
                End If
                'iajml(5) As Double
                If (IsNumeric(dataRowIA(5)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iajml required numeric." : GoTo selesai
                End If
                'iainputuser(7) As Integer
                If (IsNumeric(dataRowIA(7)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iainputuser required numeric." : GoTo selesai
                End If
                'iainputtgl(8) As DateTime
                If (IsDate(dataRowIA(8)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iainputtgl required date." : GoTo selesai
                End If
                'iamodifikasiuser(9) As Integer
                If (IsNumeric(dataRowIA(9)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iamodifikasiuser required numeric." : GoTo selesai
                End If
                'iamodifikasitgl(10) As DateTime
                If (IsDate(dataRowIA(10)) = False) Then
                    result(2) = "Item Assembly Row : " & i & " - iamodifikasitgl required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

                'VALIDASI DATA DETAIL ---------------------------------------
                'iakodebarang(1) As String
                If Len(dataRowIA(1)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarang can't be empty" : GoTo selesai
                End If
                If Len(dataRowIA(1)) > 25 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarang should not be more than 25 character." : GoTo selesai
                End If

                'iakodebarangpenyusun(3) As String
                If Len(dataRowIA(3)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarangpenyusun can't be empty" : GoTo selesai
                End If
                If Len(dataRowIA(3)) > 25 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarangpenyusun should not be more than 25 character." : GoTo selesai
                End If

                'iajml(5) As Double
                If Len(dataRowIA(5)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iajml can't be empty" : GoTo selesai
                End If

                'iasatuan(6) As String
                If Len(dataRowIA(6)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iasatuan can't be empty" : GoTo selesai
                End If
                If Len(dataRowIA(6)) > 25 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iasatuan should not be more than 25 character." : GoTo selesai
                End If

                'iainputtgl(8) As DateTime
                If Len(dataRowIA(8)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iainputtgl can't be empty" : GoTo selesai
                End If

                'iamodifikasitgl(10) As DateTime
                If Len(dataRowIA(10)) = 0 Then
                    result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iamodifikasitgl can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA DETAIL --------------------------------

                If AsDataTableTambahData(dtIA, "iaidbarang~iakodebarang~iaidbarangpenyusun~iakodebarangpenyusun~iaurutan~iajml~iasatuan~iainputuser~iainputtgl~iamodifikasiuser~iamodifikasitgl", dataRowIA(0) & "~" & dataRowIA(1) & "~" & dataRowIA(2) & "~" & dataRowIA(3) & "~" & dataRowIA(4) & "~" & dataRowIA(5) & "~" & dataRowIA(6) & "~" & dataRowIA(7) & "~" & dataRowIA(8) & "~" & dataRowIA(9) & "~" & dataRowIA(10)) = False Then
                    result(2) = "Item Assembly Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================
        End If

        '********************************** END OF ITEM ASSEMBLY ***********************************


        '************************************ ITEM SUPPLIER ****************************************

        'isidbarang(0) As Integer, isidkontak(1) As Integer, iscatatan(2) As String, isurutan(3) As Integer, iscustomtext1(4) As String, 
        'iscustomtext2(5) As String, iscustomtext3(6) As String, iscustomtext4(7) As String, iscustomtext5(8) As String, iscustomint1(9) As Integer, 
        'iscustomint2(10) As Integer, iscustomint3(11) As Integer, iscustomdbl1(12) As Double, iscustomdbl2(13) As Double, iscustomdbl3(14) As Double, 
        'iscustomdate1(15) As Date, iscustomdate2(16) As Date, iscustomdate3(17) As Date

        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, 
        'iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, 
        'iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3

        'Buat datatable Item Supplier
        Dim dtIS As New DataTable
        AsDataTableTambahField(dtIS, "isidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "isidkontak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "iscatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "isurutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "iscustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "iscustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "iscustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtIS, "iscustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtIS, "iscustomdate3", AsEnumTypeData.AsString)

        If (Len(dataSplit(3)) <> 0) Then

            'VALIDASI DAN SET DATA Item Supplier ======================================================
            'SPLIT PARAMETER DATA Item Supplier
            dataIS = dataSplit(3).Split(sptRow)
            'END OF VALIDASI DAN SET DATA Item Supplier ===============================================

            'VALIDASI DAN SET DATA ROW Item Supplier ==================================================
            Dim JmlDtIS As Integer = dataIS.Length
            For i = 1 To JmlDtIS
                'SPLIT DATA Item Supplier
                dataRowIS = dataIS(i - 1).Split(sptField)

                'VALIDASI DAN SET ROW DATA Item Supplier -----------------------------------
                'CEK ARRAY DATA Item Supplier
                If (dataRowIS.Length <> 18) Then
                    result(2) = "Item Supplier Row : " & i & " - Invalid Item Supplier transaction data parameter." : GoTo selesai
                End If
                'END OF VALIDASI DAN SET DATA ROW Item Supplier ----------------------------

                'VALIDASI TIPE DATA Item Supplier ------------------------------------------
                'isidbarang(0) As Integer
                If (IsNumeric(dataRowIS(0)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - isidbarang required numeric." : GoTo selesai
                End If
                'isidkontak(1) As Integer
                If (IsNumeric(dataRowIS(1)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - isidkontak required numeric." : GoTo selesai
                End If
                'isurutan(3) As Integer
                If (IsNumeric(dataRowIS(3)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - isurutan required numeric." : GoTo selesai
                End If
                'iscustomint1(9) As Integer
                If (IsNumeric(dataRowIS(9)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomint1 required numeric." : GoTo selesai
                End If
                'iscustomint2(10) As Integer
                If (IsNumeric(dataRowIS(10)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomint2 required numeric." : GoTo selesai
                End If
                'iscustomint3(11) As Integer
                If (IsNumeric(dataRowIS(11)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomint3 required numeric." : GoTo selesai
                End If
                'iscustomdbl1(12) As Double
                If (IsNumeric(dataRowIS(12)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl1 required numeric." : GoTo selesai
                End If
                'iscustomdbl2(13) As Double
                If (IsNumeric(dataRowIS(13)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl2 required numeric." : GoTo selesai
                End If
                'iscustomdbl3(14) As Double
                If (IsNumeric(dataRowIS(14)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl3 required numeric." : GoTo selesai
                End If
                'iscustomdate1(15) As Date
                If (IsDate(dataRowIS(15)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate1 required date." : GoTo selesai
                End If
                'iscustomdate2(16) As Date
                If (IsDate(dataRowIS(16)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate2 required date." : GoTo selesai
                End If
                'iscustomdate3(17) As Date
                If (IsDate(dataRowIS(17)) = False) Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate3 required date." : GoTo selesai
                End If
                'END OF VALIDASI TIPE DATA Item Supplier -----------------------------------

                'VALIDASI DATA Item Supplier ---------------------------------------
                'iscustomdbl1(12) As Double
                If Len(dataRowIS(12)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl1 can't be empty" : GoTo selesai
                End If

                'iscustomdbl2(13) As Double
                If Len(dataRowIS(13)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl2 can't be empty" : GoTo selesai
                End If

                'iscustomdbl3(14) As Double
                If Len(dataRowIS(14)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdbl3 can't be empty" : GoTo selesai
                End If

                'iscustomdate1(15) As Date
                If Len(dataRowIS(15)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate1 can't be empty" : GoTo selesai
                End If

                'iscustomdate2(16) As Date
                If Len(dataRowIS(16)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate2 can't be empty" : GoTo selesai
                End If

                'iscustomdate3(17) As Date
                If Len(dataRowIS(17)) = 0 Then
                    result(2) = "Item Supplier Row : " & i & " - iscustomdate3 can't be empty" : GoTo selesai
                End If

                'END OF VALIDASI DATA Item Supplier --------------------------------

                If AsDataTableTambahData(dtIS, "isidbarang~isidkontak~iscatatan~isurutan~iscustomtext1~iscustomtext2~iscustomtext3~iscustomtext4~iscustomtext5~iscustomint1~iscustomint2~iscustomint3~iscustomdbl1~iscustomdbl2~iscustomdbl3~iscustomdate1~iscustomdate2~iscustomdate3", dataRowIS(0) & "~" & dataRowIS(1) & "~" & dataRowIS(2) & "~" & dataRowIS(3) & "~" & dataRowIS(4) & "~" & dataRowIS(5) & "~" & dataRowIS(6) & "~" & dataRowIS(7) & "~" & dataRowIS(8) & "~" & dataRowIS(9) & "~" & dataRowIS(10) & "~" & dataRowIS(11) & "~" & dataRowIS(12) & "~" & dataRowIS(13) & "~" & dataRowIS(14) & "~" & dataRowIS(15) & "~" & dataRowIS(16) & "~" & dataRowIS(17)) = False Then
                    result(2) = "Item Supplier Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA Item Supplier ===========================================

        End If
        '******************************** END OF ITEM SUPPLIER *************************************


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
                Dim dr1 As DataRow = dtutama.Rows(0)
                If isUpdate Then
                    result(4) = dr1("bid")
                    kode = dr1("bkode")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bid) FROM M1_Item WHERE bid='" & result(4) & "'")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then
                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m1_item_History
                        Dim itemSimpanHistory As String = SimpanHistory.M1_Item_HistorySimpan("" & paramSplit(0) & "★M1_Item_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                        Dim itemSplit() As String = itemSimpanHistory.Split(sptParam)
                        Dim itemSplitResult() As String = itemSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (itemSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & itemSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M1_Item set bkode  = '" & FixQuotes(dr1("bkode")) & "', bnama  = '" & FixQuotes(dr1("bnama")) & "', bnamaalias1  = '" & FixQuotes(dr1("bnamaalias1")) & "', bnamaalias2  = '" & FixQuotes(dr1("bnamaalias2")) & "', bnamaalias3  = '" & FixQuotes(dr1("bnamaalias3")) & "', bnamaalias4  = '" & FixQuotes(dr1("bnamaalias4")) & "', bnamaalias5  = '" & FixQuotes(dr1("bnamaalias5")) & "', btipe  = '" & FixQuotes(dr1("btipe")) & "', bjenis  = '" & FixQuotes(dr1("bjenis")) & "', bjenisdetail  = " & dr1("bjenisdetail") & ", bkategori  = '" & FixQuotes(dr1("bkategori")) & "', bketerangan  = '" & FixQuotes(dr1("bketerangan")) & "', bsatuan  = '" & FixQuotes(dr1("bsatuan")) & "', bnilaisatuan  = '" & FixDouble(dr1("bnilaisatuan")) & "', bsatuandefault  = '" & FixQuotes(dr1("bsatuandefault")) & "', bnilaisatuandefault  = '" & FixDouble(dr1("bnilaisatuandefault")) & "', bhpp  = '" & FixQuotes(dr1("bhpp")) & "', bcabang  = '" & FixQuotes(dr1("bcabang")) & "', blokasi  = '" & FixQuotes(dr1("blokasi")) & "', bdivisi  = '" & FixQuotes(dr1("bdivisi")) & "', bsubdivisi  = '" & FixQuotes(dr1("bsubdivisi")) & "', bgudang  = '" & FixQuotes(dr1("bgudang")) & "', bproyek  = '" & FixQuotes(dr1("bproyek")) & "', bsubitem  = " & dr1("bsubitem") & ", bsubitemdari  = " & dr1("bsubitemdari") & ", bbarcode  = '" & FixQuotes(dr1("bbarcode")) & "', bsuplier  = " & dr1("bsuplier") & ", baktif  = " & dr1("baktif") & ", baktiftgl  = '" & FixQuotes(AsFormatTanggal(dr1("baktiftgl"))) & "', bstokminimal  = '" & FixDouble(dr1("bstokminimal")) & "', bstokmaksimal  = '" & FixDouble(dr1("bstokmaksimal")) & "', breorder  = '" & FixDouble(dr1("breorder")) & "', bjmlorderbeli  = '" & FixDouble(dr1("bjmlorderbeli")) & "', bjmlorderjual  = '" & FixDouble(dr1("bjmlorderjual")) & "', bkategoriumur  = '" & FixQuotes(dr1("bkategoriumur")) & "', bstatusmoving  = '" & FixQuotes(dr1("bstatusmoving")) & "', bsifatharga  = '" & FixQuotes(dr1("bsifatharga")) & "', bpromo  = " & dr1("bpromo") & ", bpromoberlaku  = '" & FixQuotes(AsFormatTanggal(dr1("bpromoberlaku"))) & "', bpajakbeli  = '" & FixQuotes(dr1("bpajakbeli")) & "', bpajakjual  = '" & FixQuotes(dr1("bpajakjual")) & "', bhargabeli  = '" & FixDouble(dr1("bhargabeli")) & "', bhppaverage  = '" & FixDouble(dr1("bhppaverage")) & "', bhargajual1  = '" & FixDouble(dr1("bhargajual1")) & "', bhargajual2  = '" & FixDouble(dr1("bhargajual2")) & "', bhargajual3  = '" & FixDouble(dr1("bhargajual3")) & "', bhargajual4  = '" & FixDouble(dr1("bhargajual4")) & "', bhargajual5  = '" & FixDouble(dr1("bhargajual5")) & "', bdiskonjual1  = '" & FixDouble(dr1("bdiskonjual1")) & "', bdiskonjual2  = '" & FixDouble(dr1("bdiskonjual2")) & "', bdiskonjual3  = '" & FixDouble(dr1("bdiskonjual3")) & "', bdiskonjual4  = '" & FixDouble(dr1("bdiskonjual4")) & "', bdiskonjual5  = '" & FixDouble(dr1("bdiskonjual5")) & "', bstok  = '" & FixDouble(dr1("bstok")) & "', bkomisi  = '" & FixDouble(dr1("bkomisi")) & "', bmarginminimal  = '" & FixDouble(dr1("bmarginminimal")) & "', brekpersediaan  = '" & FixQuotes(dr1("brekpersediaan")) & "', brekpenjualan  = '" & FixQuotes(dr1("brekpenjualan")) & "', brekreturpenjualan  = '" & FixQuotes(dr1("brekreturpenjualan")) & "', brekdiskonpenjualan  = '" & FixQuotes(dr1("brekdiskonpenjualan")) & "', brekhargapokok  = '" & FixQuotes(dr1("brekhargapokok")) & "', brekreturpembelian  = '" & FixQuotes(dr1("brekreturpembelian")) & "', brekdiskonpembelian  = '" & FixQuotes(dr1("brekdiskonpembelian")) & "', brekkonsinyasi  = '" & FixQuotes(dr1("brekkonsinyasi")) & "', bapanjang  = '" & FixDouble(dr1("bapanjang")) & "', balebar  = '" & FixDouble(dr1("balebar")) & "', batinggi  = '" & FixDouble(dr1("batinggi")) & "', bavolume  = '" & FixDouble(dr1("bavolume")) & "', baberat  = '" & FixDouble(dr1("baberat")) & "', bawarna  = '" & FixQuotes(dr1("bawarna")) & "', baoem  = '" & FixQuotes(dr1("baoem")) & "', bamerk  = '" & FixQuotes(dr1("bamerk")) & "', baukuran  = '" & FixQuotes(dr1("baukuran")) & "', bamodel  = '" & FixQuotes(dr1("bamodel")) & "', bakelas  = '" & FixQuotes(dr1("bakelas")) & "', bserial  = " & dr1("bserial") & ", bbatch  = " & dr1("bbatch") & ", bpengganti  = " & dr1("bpengganti") & ", bgambar  = '" & FixQuotes(dr1("bgambar")) & "', burutan  = " & dr1("burutan") & ", bcustom1  = '" & FixQuotes(dr1("bcustom1")) & "', bcustom2  = '" & FixQuotes(dr1("bcustom2")) & "', bcustom3  = '" & FixQuotes(dr1("bcustom3")) & "', bcustom4  = '" & FixQuotes(dr1("bcustom4")) & "', bcustom5  = '" & FixQuotes(dr1("bcustom5")) & "', bcustom6  = '" & FixQuotes(dr1("bcustom6")) & "', bcustom7  = '" & FixQuotes(dr1("bcustom7")) & "', bcustom8  = '" & FixQuotes(dr1("bcustom8")) & "', bcustom9  = '" & FixQuotes(dr1("bcustom9")) & "', bcustom10  = '" & FixQuotes(dr1("bcustom10")) & "', bcustom11  = " & FixDouble(dr1("bcustom11")) & ", bcustom12  = " & FixDouble(dr1("bcustom12")) & ", bcustom13  = " & FixDouble(dr1("bcustom13")) & ", bcustom14  = '" & FixDouble(dr1("bcustom14")) & "', bcustom15  = '" & FixDouble(dr1("bcustom15")) & "', bcatatan  = '" & FixQuotes(dr1("bcatatan")) & "', bmodifikasiuser  = " & dr1("bmodifikasiuser") & ", bmodifikasitgl  = NOW(), bedithpp  = " & dr1("bedithpp") & ", bmobile = " & dr1("bmobile") & ", bassembly = " & dr1("bassembly") & ", bdownloaded = 0, bkelasproduk = '" & dr1("bkelasproduk") & "', bretur = '" & dr1("bretur") & "', btag = '" & dr1("btag") & "', bminorder = '" & dr1("bminorder") & "', bdepartemen = '" & dr1("bdepartemen") & "', bsubdepartemen = '" & dr1("bsubdepartemen") & "', bkp = '" & dr1("bkp") & "', bkl = '" & dr1("bkl") & "' , bjmllapangan = '" & dr1("bjmllapangan") & "' , bsatuanlapangan = '" & dr1("bsatuanlapangan") & "', bsubkelas = '" & dr1("bsubkelas") & "', bmaterial = '" & dr1("bmaterial") & "', bsection = '" & dr1("bsection") & "', bvendor = '" & dr1("bvendor") & "', bdesigner = '" & dr1("bdesigner") & "', basset = '" & dr1("basset") & "', bhargajual6 = '" & FixDouble(dr1("bhargajual6")) & "', bhargajual7 = '" & FixDouble(dr1("bhargajual7")) & "', bhargajual8 = '" & FixDouble(dr1("bhargajual8")) & "', bhargajual9 = '" & FixDouble(dr1("bhargajual9")) & "', bhargajual10 = '" & FixDouble(dr1("bhargajual10")) & "', bdiskonjual6 = '" & FixDouble(dr1("bdiskonjual6")) & "', bdiskonjual7 = '" & FixDouble(dr1("bdiskonjual7")) & "', bdiskonjual8 = '" & FixDouble(dr1("bdiskonjual8")) & "', bdiskonjual9 = '" & FixDouble(dr1("bdiskonjual9")) & "', bdiskonjual10 = '" & FixDouble(dr1("bdiskonjual10")) & "', bcustom16  = " & FixDouble(dr1("bcustom16")) & ", bcustom17  = " & FixDouble(dr1("bcustom17")) & ", bcustom18  = " & FixDouble(dr1("bcustom18")) & ", bcustom19  = " & FixDouble(dr1("bcustom19")) & ", bcustom20  = " & FixDouble(dr1("bcustom20")) & ", bcustom21  = " & FixDouble(dr1("bcustom21")) & ", bcustom22  = " & FixDouble(dr1("bcustom22")) & ", bcustom23  = " & FixDouble(dr1("bcustom23")) & ", bcustom24  = " & FixDouble(dr1("bcustom24")) & ", bcustom25  = " & FixDouble(dr1("bcustom25")) & " where bid = '" & dr1("bid") & "'"
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = Con1
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()
                    Else
                        result(2) = "Item data not found." : Trans.Rollback() : GoTo selesai
                    End If
                Else

                    kode = dr1("bkode")
                    sql = "Insert into M1_Item (bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile, bassembly, bkelasproduk, bretur, btag, bminorder, bdepartemen, bsubdepartemen, bkp, bkl, bjmllapangan, bsatuanlapangan, bsubkelas, bmaterial, bsection, bvendor, bdesigner, basset, bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10, bcustom16, bcustom17, bcustom18, bcustom19, bcustom20, bcustom21, bcustom22, bcustom23, bcustom24, bcustom25) values('" & FixQuotes(dr1("bkode")) & "', '" & FixQuotes(dr1("bnama")) & "', '" & FixQuotes(dr1("bnamaalias1")) & "', '" & FixQuotes(dr1("bnamaalias2")) & "', '" & FixQuotes(dr1("bnamaalias3")) & "', '" & FixQuotes(dr1("bnamaalias4")) & "', '" & FixQuotes(dr1("bnamaalias5")) & "', '" & FixQuotes(dr1("btipe")) & "', '" & FixQuotes(dr1("bjenis")) & "', " & dr1("bjenisdetail") & ", '" & FixQuotes(dr1("bkategori")) & "', '" & FixQuotes(dr1("bketerangan")) & "', '" & FixQuotes(dr1("bsatuan")) & "', '" & FixDouble(dr1("bnilaisatuan")) & "', '" & FixQuotes(dr1("bsatuandefault")) & "', '" & FixDouble(dr1("bnilaisatuandefault")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixQuotes(dr1("bcabang")) & "', '" & FixQuotes(dr1("blokasi")) & "', '" & FixQuotes(dr1("bdivisi")) & "', '" & FixQuotes(dr1("bsubdivisi")) & "', '" & FixQuotes(dr1("bgudang")) & "', '" & FixQuotes(dr1("bproyek")) & "', " & dr1("bsubitem") & ", " & dr1("bsubitemdari") & ", '" & FixQuotes(dr1("bbarcode")) & "', " & dr1("bsuplier") & ", " & dr1("baktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("baktiftgl"))) & "', '" & FixDouble(dr1("bstokminimal")) & "', '" & FixDouble(dr1("bstokmaksimal")) & "', '" & FixDouble(dr1("breorder")) & "', '" & FixDouble(dr1("bjmlorderbeli")) & "', '" & FixDouble(dr1("bjmlorderjual")) & "', '" & FixQuotes(dr1("bkategoriumur")) & "', '" & FixQuotes(dr1("bstatusmoving")) & "', '" & FixQuotes(dr1("bsifatharga")) & "', " & dr1("bpromo") & ", '" & FixQuotes(AsFormatTanggal(dr1("bpromoberlaku"))) & "', '" & FixQuotes(dr1("bpajakbeli")) & "', '" & FixQuotes(dr1("bpajakjual")) & "', '" & FixDouble(dr1("bhargabeli")) & "', '" & FixDouble(dr1("bhppaverage")) & "', '" & FixDouble(dr1("bhargajual1")) & "', '" & FixDouble(dr1("bhargajual2")) & "', '" & FixDouble(dr1("bhargajual3")) & "', '" & FixDouble(dr1("bhargajual4")) & "', '" & FixDouble(dr1("bhargajual5")) & "', '" & FixDouble(dr1("bdiskonjual1")) & "', '" & FixDouble(dr1("bdiskonjual2")) & "', '" & FixDouble(dr1("bdiskonjual3")) & "', '" & FixDouble(dr1("bdiskonjual4")) & "', '" & FixDouble(dr1("bdiskonjual5")) & "', '" & FixDouble(dr1("bstok")) & "', '" & FixDouble(dr1("bkomisi")) & "', '" & FixDouble(dr1("bmarginminimal")) & "', '" & FixQuotes(dr1("brekpersediaan")) & "', '" & FixQuotes(dr1("brekpenjualan")) & "', '" & FixQuotes(dr1("brekreturpenjualan")) & "', '" & FixQuotes(dr1("brekdiskonpenjualan")) & "', '" & FixQuotes(dr1("brekhargapokok")) & "', '" & FixQuotes(dr1("brekreturpembelian")) & "', '" & FixQuotes(dr1("brekdiskonpembelian")) & "', '" & FixQuotes(dr1("brekkonsinyasi")) & "', '" & FixDouble(dr1("bapanjang")) & "', '" & FixDouble(dr1("balebar")) & "', '" & FixDouble(dr1("batinggi")) & "', '" & FixDouble(dr1("bavolume")) & "', '" & FixDouble(dr1("baberat")) & "', '" & FixQuotes(dr1("bawarna")) & "', '" & FixQuotes(dr1("baoem")) & "', '" & FixQuotes(dr1("bamerk")) & "', '" & FixQuotes(dr1("baukuran")) & "', '" & FixQuotes(dr1("bamodel")) & "', '" & FixQuotes(dr1("bakelas")) & "', " & dr1("bserial") & ", " & dr1("bbatch") & ", " & dr1("bpengganti") & ", '" & FixQuotes(dr1("bgambar")) & "', " & dr1("burutan") & ", '" & FixQuotes(dr1("bcustom1")) & "', '" & FixQuotes(dr1("bcustom2")) & "', '" & FixQuotes(dr1("bcustom3")) & "', '" & FixQuotes(dr1("bcustom4")) & "', '" & FixQuotes(dr1("bcustom5")) & "', '" & FixQuotes(dr1("bcustom6")) & "', '" & FixQuotes(dr1("bcustom7")) & "', '" & FixQuotes(dr1("bcustom8")) & "', '" & FixQuotes(dr1("bcustom9")) & "', '" & FixQuotes(dr1("bcustom10")) & "', " & FixDouble(dr1("bcustom11")) & ", " & FixDouble(dr1("bcustom12")) & ", " & FixDouble(dr1("bcustom13")) & ", '" & FixDouble(dr1("bcustom14")) & "', '" & FixDouble(dr1("bcustom15")) & "', '" & FixQuotes(dr1("bcatatan")) & "', " & dr1("binputuser") & ", NOW(), " & dr1("bmodifikasiuser") & ", '1971-01-01 00:00:00', " & dr1("bedithpp") & ", " & dr1("bmobile") & ", " & dr1("bassembly") & ", '" & dr1("bkelasproduk") & "', '" & dr1("bretur") & "', '" & dr1("btag") & "', '" & dr1("bminorder") & "', '" & dr1("bdepartemen") & "', '" & dr1("bsubdepartemen") & "', '" & dr1("bkp") & "', '" & dr1("bkl") & "', '" & dr1("bjmllapangan") & "', '" & dr1("bsatuanlapangan") & "', '" & FixQuotes(dr1("bsubkelas")) & "', '" & FixQuotes(dr1("bmaterial")) & "', '" & FixQuotes(dr1("bsection")) & "', '" & FixQuotes(dr1("bvendor")) & "', '" & FixQuotes(dr1("bdesigner")) & "', '" & FixQuotes(dr1("basset")) & "', '" & FixDouble(dr1("bhargajual6")) & "', '" & FixDouble(dr1("bhargajual7")) & "', '" & FixDouble(dr1("bhargajual8")) & "', '" & FixDouble(dr1("bhargajual9")) & "', '" & FixDouble(dr1("bhargajual10")) & "', '" & FixDouble(dr1("bdiskonjual6")) & "', '" & FixDouble(dr1("bdiskonjual7")) & "', '" & FixDouble(dr1("bdiskonjual8")) & "', '" & FixDouble(dr1("bdiskonjual9")) & "', '" & FixDouble(dr1("bdiskonjual10")) & "', '" & FixDouble(dr1("bcustom16")) & "', '" & FixDouble(dr1("bcustom17")) & "', '" & FixDouble(dr1("bcustom18")) & "', '" & FixDouble(dr1("bcustom19")) & "', '" & FixDouble(dr1("bcustom20")) & "', '" & FixDouble(dr1("bcustom21")) & "', '" & FixDouble(dr1("bcustom22")) & "', '" & FixDouble(dr1("bcustom23")) & "', '" & FixDouble(dr1("bcustom24")) & "', '" & FixDouble(dr1("bcustom25")) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select bid from M1_Item where bkode= '" & kode & "' limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Item data not found." : Trans.Rollback() : GoTo selesai
                End If

                'UPDATE BARCODE NO BERIKUTNYA
                If Len(dr1("bbarcode")) > 0 Then
                    Dim dt As New DataTable

                    'AMBIL JMLDIGIT NO URUT BARCODE
                    Dim jmldigit As Double = 0
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'options' AND skode = 'JmlDigitBarcode'"
                    dt = AsDataTableAmbilDariDB(sql)
                    If dt.Rows.Count > 0 Then
                        If IsNumeric(FxDB(dt.Rows(0)(0), 0)) Then
                            jmldigit = Double.Parse(FxDB(dt.Rows(0)(0), 0))
                        End If
                    End If

                    'JIKA BARCODE >= JMLDIGIT BARCODE SETTING
                    If dr1("bbarcode").Length >= jmldigit Then
                        'AMBIL AWALAN BARCODE
                        Dim awalan As String = Left(dr1("bbarcode"), dr1("bbarcode").Length - jmldigit)
                        'AMBIL URUTAN BARCODE
                        Dim nourut As Double = Double.Parse(Right(dr1("bbarcode"), jmldigit))

                        'AMBIL NO URUT BARCODE BERIKUTNYA
                        Dim noberikutnya As Double = 0
                        sql = "SELECT noberikutnya FROM m0_barcode_next WHERE awalan = '" & FixQuotes(awalan) & "'"
                        dt = AsDataTableAmbilDariDB(sql)
                        If dt.Rows.Count > 0 Then
                            noberikutnya = Double.Parse(FxDB(dt.Rows(0)(0), 1))
                        End If

                        If nourut >= noberikutnya Then
                            sql = "INSERT INTO m0_barcode_next (awalan, noberikutnya) VALUES ('" & FixQuotes(awalan) & "', " & FixDouble(nourut + 1) & ") ON DUPLICATE KEY UPDATE noberikutnya = VALUES(noberikutnya) "
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

            Else
                result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            'Hapus item location warehouse ketika update
            If (isUpdate) Then
                sql = "Delete from M1_Item_Location_Warehouse where blgidbarang = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses item location warehouse
            If (dtILW.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                If isUpdate Then
                    For Each dr1 As DataRow In dtILW.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("blgkodebarang")) & "', '" & FixQuotes(dr1("blggudang")) & "', " & dr1("blgidlokasi") & ", '" & FixQuotes(dr1("blgkodelokasi")) & "', '" & FixQuotes(dr1("blgnamalokasi")) & "', " & dr1("blginputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("blginputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("blgmodifikasiuser") & ", NOW())")
                    Next
                    sql = "Insert into M1_Item_Location_Warehouse(blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    For Each dr1 As DataRow In dtILW.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("blggudang")) & "', '" & FixQuotes(dr1("blgkodebarang")) & "', " & dr1("blgidlokasi") & ", '" & FixQuotes(dr1("blgkodelokasi")) & "', '" & FixQuotes(dr1("blgnamalokasi")) & "', " & dr1("blginputuser") & ", NOW(), " & dr1("blgmodifikasiuser") & ", '1971-01-01 00:00:00')")
                    Next
                    sql = "Insert into M1_Item_Location_Warehouse(blgidbarang, blggudang, blgkodebarang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl) values" & strValue2.ToString & ""
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

            'Hapus item assembly ketika update
            If (isUpdate) Then
                sql = "Delete from M1_Item_Assembly where iaidbarang = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses item assembly
            If (dtIA.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                If isUpdate Then
                    For Each dr1 As DataRow In dtIA.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("iakodebarang")) & "', " & dr1("iaidbarangpenyusun") & ", '" & FixQuotes(dr1("iakodebarangpenyusun")) & "', " & dr1("iaurutan") & ", '" & FixDouble(dr1("iajml")) & "', '" & FixQuotes(dr1("iasatuan")) & "', " & dr1("iainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("iainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("iamodifikasiuser") & ", NOW())")
                    Next
                    sql = "Insert into M1_Item_Assembly(iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl) values" & strValue2.ToString & ""
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                Else
                    For Each dr1 As DataRow In dtIA.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                        strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("iakodebarang")) & "', " & dr1("iaidbarangpenyusun") & ", '" & FixQuotes(dr1("iakodebarangpenyusun")) & "', " & dr1("iaurutan") & ", '" & FixDouble(dr1("iajml")) & "', '" & FixQuotes(dr1("iasatuan")) & "', " & dr1("iainputuser") & ", NOW(), " & dr1("iamodifikasiuser") & ", '1971-01-01 00:00:00')")
                    Next
                    sql = "Insert into M1_Item_Assembly(iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl) values" & strValue2.ToString & ""
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

            'Hapus item supplier ketika update
            If (isUpdate) Then
                sql = "Delete from M1_Item_Supplier where isidbarang = '" & result(4) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If

            'Proses item supplier
            If (dtIS.Rows.Count > 0) Then
                Dim strValue2 As New StringBuilder
                For Each dr1 As DataRow In dtIS.Rows
                    strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                    strValue2.Append("(" & result(4) & ", " & dr1("isidkontak") & ", '" & FixQuotes(dr1("iscatatan")) & "', " & dr1("isurutan") & ", '" & FixQuotes(dr1("iscustomtext1")) & "', '" & FixQuotes(dr1("iscustomtext2")) & "', '" & FixQuotes(dr1("iscustomtext3")) & "', '" & FixQuotes(dr1("iscustomtext4")) & "', '" & FixQuotes(dr1("iscustomtext5")) & "', " & dr1("iscustomint1") & ", " & dr1("iscustomint2") & ", " & dr1("iscustomint3") & ", '" & FixDouble(dr1("iscustomdbl1")) & "', '" & FixDouble(dr1("iscustomdbl2")) & "', '" & FixDouble(dr1("iscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate3"))) & "')")
                Next
                sql = "Insert into M1_Item_Supplier(isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3) values" & strValue2.ToString & ""
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
            result(2) = kode
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_Item_DataSearch(PostWsSearch(paramSplit(0), "M1_Item_DataSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_ItemDelete(ByVal param As String) As String

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
            Dim paramTerkait As String = M1_ItemTerkait(PostWsTerkait(paramSplit(0), "M1_ItemTerkait", pagingSplit(0), pagingSplit(1), "", "", formatTgl, formatTglWaktu, idtransaksi))
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
            Dim SimpanHistory As New m1_item_History
            Dim itemSimpanHistory As String = SimpanHistory.M1_Item_HistorySimpan("" & paramSplit(0) & "★M1_Item_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(idtransaksi) & "")
            Dim itemSplit() As String = itemSimpanHistory.Split(sptParam)
            Dim itemSplitResult() As String = itemSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (itemSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & itemSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================

            'DELETE ITEM SUPPLIER
            sql = "DELETE FROM m1_item_supplier WHERE isidbarang = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE ITEM ASSEMBLY
            sql = "DELETE FROM m1_item_assembly WHERE iaidbarang = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE ITEM LOCATION WAREHOUSE
            sql = "DELETE FROM m1_item_location_warehouse WHERE blgidbarang = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE ITEM
            sql = "DELETE FROM M1_Item WHERE bid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M1_Item_DataSearch(PostWsSearch(paramSplit(0), "M1_Item_DataSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M1_ItemSearch(ByVal param As String) As String
        'M1_ItemSearch --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, 
        'bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, 
        'bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, 
        'bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, 
        'bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, 
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, 
        'bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, 
        'bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, 
        'brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama,
        'bkelasproduk, bretur, btag, bminorder, bmobile, bassembly, bdownloaded, bkelasproduknama, btagnama, btagjual, btagmutasipusat, btagpermintaanmutasi, 
        'btagpermintaanmutasi, btagmutasicabang, btagretursupplier, btagpermintaanpembelian, bkp, bkl, basset,
        'bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, 
        'bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10


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
            Filter = Filter.Replace("bid", "i.bid")
            Filter = Filter.Replace("bkode", "i.bkode")
            Filter = Filter.Replace("btipe", "i.btipe")
            Filter = Filter.Replace("bstok", "i.bstok")
            Filter = Filter.Replace("bsatuan", "i.bsatuan")
            Filter = Filter.Replace("bnama", "i.bnama")
            '#Taruh fungsi replace disini...
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_v")
        sql = "SELECT i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.bnamaalias1 AS bnamaalias1, i.bnamaalias2 AS bnamaalias2, i.bnamaalias3 AS bnamaalias3, i.bnamaalias4 AS bnamaalias4, i.bnamaalias5 AS bnamaalias5, i.btipe AS btipe, i.bjenis AS bjenis, i.bjenisdetail AS bjenisdetail, i.bkategori AS bkategori, i.bketerangan AS bketerangan, i.bsatuan AS bsatuan, i.bnilaisatuan AS bnilaisatuan, i.bsatuandefault AS bsatuandefault, i.bnilaisatuandefault AS bnilaisatuandefault, i.bhpp AS bhpp, i.bcabang AS bcabang, i.blokasi AS blokasi, i.bdivisi AS bdivisi, i.bsubdivisi AS bsubdivisi, i.bgudang AS bgudang, i.bproyek AS bproyek, i.bsubitem AS bsubitem, i.bsubitemdari AS bsubitemdari, i.bbarcode AS bbarcode, i.bsuplier AS bsuplier, i.baktif AS baktif, i.baktiftgl AS baktiftgl, i.bstokminimal AS bstokminimal, i.bstokmaksimal AS bstokmaksimal, i.breorder AS breorder, i.bjmlorderbeli AS bjmlorderbeli, i.bjmlorderjual AS bjmlorderjual, i.bkategoriumur AS bkategoriumur, i.bstatusmoving AS bstatusmoving, i.bsifatharga AS bsifatharga, i.bpromo AS bpromo, i.bpromoberlaku AS bpromoberlaku, i.bpajakbeli AS bpajakbeli, i.bpajakjual AS bpajakjual, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bhargajual2 AS bhargajual2, i.bhargajual3 AS bhargajual3, i.bhargajual4 AS bhargajual4, i.bhargajual5 AS bhargajual5, i.bdiskonjual1 AS bdiskonjual1, i.bdiskonjual2 AS bdiskonjual2, i.bdiskonjual3 AS bdiskonjual3, i.bdiskonjual4 AS bdiskonjual4, i.bdiskonjual5 AS bdiskonjual5, (CASE i.bjenis WHEN 'J' THEN 0 ELSE i.bstok END) AS bstok, i.bkomisi AS bkomisi, i.bmarginminimal AS bmarginminimal, i.brekpersediaan AS brekpersediaan, i.brekpenjualan AS brekpenjualan, i.brekreturpenjualan AS brekreturpenjualan, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekhargapokok AS brekhargapokok, i.brekreturpembelian AS brekreturpembelian, i.brekdiskonpembelian AS brekdiskonpembelian, i.brekkonsinyasi AS brekkonsinyasi, i.bapanjang AS bapanjang, i.balebar AS balebar, i.batinggi AS batinggi, i.bavolume AS bavolume, i.baberat AS baberat, i.bawarna AS bawarna, i.baoem AS baoem, i.bamerk AS bamerk, i.baukuran AS baukuran, i.bamodel AS bamodel, i.bakelas AS bakelas, i.bserial AS bserial, i.bbatch AS bbatch, i.bpengganti AS bpengganti, i.bgambar AS bgambar, i.burutan AS burutan, i.bcustom1 AS bcustom1, i.bcustom2 AS bcustom2, i.bcustom3 AS bcustom3, i.bcustom4 AS bcustom4, i.bcustom5 AS bcustom5, i.bcustom6 AS bcustom6, i.bcustom7 AS bcustom7, i.bcustom8 AS bcustom8, i.bcustom9 AS bcustom9, i.bcustom10 AS bcustom10, i.bcustom11 AS bcustom11, i.bcustom12 AS bcustom12, i.bcustom13 AS bcustom13, i.bcustom14 AS bcustom14, i.bcustom15 AS bcustom15, i.bcatatan AS bcatatan, i.binputuser AS binputuser, i.binputtgl AS binputtgl, i.bmodifikasiuser AS bmodifikasiuser, i.bmodifikasitgl AS bmodifikasitgl, i.bedithpp AS bedithpp, it.itnama AS btipenama, ic.icnama AS bkategorinama, u1.unama AS bsatuannama, u2.unama AS bsatuandefaultnama, br.bnama AS bcabangnama, lc.lnama AS blokasinama, dv.dnama AS bdivisinama, sdv.sdnama AS bsubdivisinama, wh.wnama AS bgudangnama, p.pnama AS bproyeknama, i2.bkode AS bsubitemdarikode, c.kkode AS bsuplierkode, c.knama AS bsupliernama, tax1.tnama AS bpajakbelinama, tax2.tnama AS bpajakjualnama, coa1.cnama AS brekpersediaannama, coa2.cnama AS brekpenjualannama, coa3.cnama AS brekreturpenjualannama, coa4.cnama AS brekdiskonpenjualannama, coa5.cnama AS brekhargapokoknama, coa6.cnama AS brekreturpembeliannama, coa7.cnama AS brekdiskonpembeliannama, coa8.cnama AS brekkonsinyasinama, i.bkelasproduk, i.bretur, i.btag, i.bminorder, i.bmobile, i.bassembly, i.bdownloaded, cp.cpnama as bkelasproduknama, tag.ipnama as btagnama, tag.ipjual AS btagjual, tag.ipmutasipusat AS btagmutasipusat, tag.ippermintaanmutasi AS btagpermintaanmutasi ,tag.ipmutasicabang AS btagmutasicabang, tag.ipretursupplier AS btagretursupplier, tag.ippermintaanpembelian AS btagpermintaanpembelian, i.bkp, i.bkl, i.basset, i.bhargajual6, i.bhargajual7, i.bhargajual8, i.bhargajual9, i.bhargajual10, i.bdiskonjual6, i.bdiskonjual7, i.bdiskonjual8, i.bdiskonjual9, i.bdiskonjual10 from `m1_item` `i`  left join `m1_item_type` `it` on `i`.`btipe` = `it`.`itkode` left join `m1_item_category` `ic` on `i`.`bkategori` = `ic`.`ickode` left join `m1_unit` `u1` on `i`.`bsatuan` = `u1`.`ukode` left join `m1_unit` `u2` on `i`.`bsatuandefault` = `u2`.`ukode` left join `m1_branch` `br` on `i`.`bcabang` = `br`.`bkode` left join `m1_division` `dv` on `i`.`bdivisi` = `dv`.`dkode` left join `m1_subdivision` `sdv` on `i`.`bsubdivisi` = `sdv`.`sdkode` left join `m1_location` `lc` on `i`.`blokasi` = `lc`.`lkode` left join `m1_warehouse` `wh` on `i`.`bgudang` = `wh`.`wkode` left join `m1_project` `p` on `i`.`bproyek` = `p`.`pkode` left join `m1_item` `i2` on `i`.`bsubitemdari` = `i2`.`bid` left join `m1_contact` `c` on `i`.`bsuplier` = `c`.`kid` left join `m1_tax` `tax1` on `i`.`bpajakbeli` = `tax1`.`tkode` left join `m1_tax` `tax2` on `i`.`bpajakjual` = `tax2`.`tkode` left join `m1_coa` `coa1` on `i`.`brekpersediaan` = `coa1`.`cnomor` left join `m1_coa` `coa2` on `i`.`brekpenjualan` = `coa2`.`cnomor` left join `m1_coa` `coa3` on `i`.`brekreturpenjualan` = `coa3`.`cnomor` left join `m1_coa` `coa4` on `i`.`brekdiskonpenjualan` = `coa4`.`cnomor` left join `m1_coa` `coa5` on `i`.`brekhargapokok` = `coa5`.`cnomor` left join `m1_coa` `coa6` on `i`.`brekreturpembelian` = `coa6`.`cnomor` left join `m1_coa` `coa7` on `i`.`brekdiskonpembelian` = `coa7`.`cnomor` left join `m1_coa` `coa8` on `i`.`brekkonsinyasi` = `coa8`.`cnomor` left join m1_class_product cp on i.bkelasproduk = cp.cpkode left join m1_item_permission tag on i.btag = tag.ipkode"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), 0), sptField,
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
                     FxDB(dr("bsubitemdari"), 0), sptField,
                     FxDB(dr("bbarcode"), ""), sptField,
                     FxDB(dr("bsuplier"), 0), sptField,
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
                     FxDB(dr("bdiskonjual1"), 0), sptField,
                     FxDB(dr("bdiskonjual2"), 0), sptField,
                     FxDB(dr("bdiskonjual3"), 0), sptField,
                     FxDB(dr("bdiskonjual4"), 0), sptField,
                     FxDB(dr("bdiskonjual5"), 0), sptField,
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
                     FxDB(dr("bpengganti"), 0), sptField,
                     FxDB(dr("bgambar"), ""), sptField,
                     FxDB(dr("burutan"), 0), sptField,
                     FxDB(dr("bcustom1"), ""), sptField,
                     FxDB(dr("bcustom2"), ""), sptField,
                     FxDB(dr("bcustom3"), ""), sptField,
                     FxDB(dr("bcustom4"), ""), sptField,
                     FxDB(dr("bcustom5"), ""), sptField,
                     FxDB(dr("bcustom6"), ""), sptField,
                     FxDB(dr("bcustom7"), ""), sptField,
                     FxDB(dr("bcustom8"), ""), sptField,
                     FxDB(dr("bcustom9"), ""), sptField,
                     FxDB(dr("bcustom10"), ""), sptField,
                     FxDB(dr("bcustom11"), 0), sptField,
                     FxDB(dr("bcustom12"), 0), sptField,
                     FxDB(dr("bcustom13"), 0), sptField,
                     FxDB(dr("bcustom14"), 0), sptField,
                     FxDB(dr("bcustom15"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("binputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("bedithpp"), 0), sptField,
                     FxDB(dr("btipenama"), ""), sptField,
                     FxDB(dr("bkategorinama"), ""), sptField,
                     FxDB(dr("bsatuannama"), ""), sptField,
                     FxDB(dr("bsatuandefaultnama"), ""), sptField,
                     FxDB(dr("bcabangnama"), ""), sptField,
                     FxDB(dr("blokasinama"), ""), sptField,
                     FxDB(dr("bdivisinama"), ""), sptField,
                     FxDB(dr("bsubdivisinama"), ""), sptField,
                     FxDB(dr("bgudangnama"), ""), sptField,
                     FxDB(dr("bproyeknama"), ""), sptField,
                     FxDB(dr("bsubitemdarikode"), ""), sptField,
                     FxDB(dr("bsuplierkode"), ""), sptField,
                     FxDB(dr("bsupliernama"), ""), sptField,
                     FxDB(dr("bpajakbelinama"), ""), sptField,
                     FxDB(dr("bpajakjualnama"), ""), sptField,
                     FxDB(dr("brekpersediaannama"), ""), sptField,
                     FxDB(dr("brekpenjualannama"), ""), sptField,
                     FxDB(dr("brekreturpenjualannama"), ""), sptField,
                     FxDB(dr("brekdiskonpenjualannama"), ""), sptField,
                     FxDB(dr("brekhargapokoknama"), ""), sptField,
                     FxDB(dr("brekreturpembeliannama"), ""), sptField,
                     FxDB(dr("brekdiskonpembeliannama"), ""), sptField,
                     FxDB(dr("brekkonsinyasinama"), ""), sptField,
                     FxDB(dr("bkelasproduk"), ""), sptField,
                     FxDB(dr("bretur"), 0), sptField,
                     FxDB(dr("btag"), ""), sptField,
                     FxDB(dr("bminorder"), 0), sptField,
                     FxDB(dr("bmobile"), 0), sptField,
                     FxDB(dr("bassembly"), 0), sptField,
                     FxDB(dr("bdownloaded"), 0), sptField,
                     FxDB(dr("bkelasproduknama"), ""), sptField,
                     FxDB(dr("btagnama"), ""), sptField,
                     FxDB(dr("btagjual"), 0), sptField,
                     FxDB(dr("btagmutasipusat"), 0), sptField,
                     FxDB(dr("btagpermintaanmutasi"), 0), sptField,
                     FxDB(dr("btagmutasicabang"), 0), sptField,
                     FxDB(dr("btagretursupplier"), 0), sptField,
                     FxDB(dr("btagpermintaanpembelian"), 0), sptField,
                     FxDB(dr("bkp"), 0), sptField,
                     FxDB(dr("bkl"), 0), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("bhargajual6"), 0), sptField,
                     FxDB(dr("bhargajual7"), 0), sptField,
                     FxDB(dr("bhargajual8"), 0), sptField,
                     FxDB(dr("bhargajual9"), 0), sptField,
                     FxDB(dr("bhargajual10"), 0), sptField,
                     FxDB(dr("bdiskonjual6"), 0), sptField,
                     FxDB(dr("bdiskonjual7"), 0), sptField,
                     FxDB(dr("bdiskonjual8"), 0), sptField,
                     FxDB(dr("bdiskonjual9"), 0), sptField,
                     FxDB(dr("bdiskonjual10"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, bkelasproduk, bretur, btag, bminorder, bmobile, bassembly, bdownloaded, bkelasproduknama, btagnama, btagjual, btagmutasipusat, bpermintaanmutasi, bmutasicabang, bretursupplier, bpermintaanpembelian, bkp, bkl, basset, bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ItemSearchBooking(ByVal param As String) As String
        'M1_ItemSearchBooking --------------------------------------------------------
        'bid, stokglobal, stokbooking, stokakhir

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

        'PANGGIL QUERY
        sql = "Select i.bid, i.bstok as stokglobal, ib.jmlbooking as stokbooking, (i.bstok-SUM(ib.jmlbooking)) AS stokakhir FROM m1_item i JOIN m1_item_booking ib ON i.bid = ib.idbarang"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_ItemBooking", Filter, "i.bid", True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("stokglobal"), 0), sptField,
                     FxDB(dr("stokbooking"), 0), sptField,
                     FxDB(dr("stokakhir"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, stokglobal, stokbooking, stokakhir"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ItemCekId(ByVal param As String) As String

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
        dt = AsDataTableAmbilDariDB("SELECT COUNT(bkode) FROM m1_item WHERE bkode='" & idtransaksi & "'")
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
    Public Function M1_ItemTerkait(ByVal param As String) As String
        'M1_ItemTerkait --------------------------------------------------------
        'bid, bkode, sumber, idterkait

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
        If (IsNumeric(paramSplit(5)) = False) Then
            result(2) = "bid required numeric." : GoTo selesai
        Else
            'SET IDTRANSAKSI
            idtransaksi = paramSplit(5)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_terkait")
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                             FxDB(dr("bid"), ""), sptField,
                             FxDB(dr("bkode"), ""), sptField,
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
            result(2) = "Related Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, sumber, idterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ItemGetdataById(ByVal param As String) As String

        'M1_ItemGetdataById Utama --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, 
        'bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, 
        'bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, 
        'bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, 
        'bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, 
        'bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, 
        'brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, 
        'baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, 
        'bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, 
        'bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, 
        'bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, bsubitemdarikode, 
        'bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, brekreturpenjualannama, 
        'brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, 
        'bjmlterkait, bkomisikode, bkomisinama, bmobile, bassembly, bkelasproduk, bretur, btag, bminorder, bdownloaded, 
        'bkelasproduknama, btagnama, bdepartemen, bsubdepartemen, bdepartemennama, bsubdepartemennama, bkp, bkl, bjmllapangan, 
        'bsatuanlapangan, bakelasnama, bsubkelasnama, bawarnanama, bdesignernama, bamodelnama, bamerknama, bmaterialnama, 
        'baoemnama, bsectionnama, baukurannama, bvendornama, bsubkelas, bdesigner, bmaterial, bsection, bvendor, basset, 
        'bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, 
        'bdiskonjual10, bproductionroutekode, bproductionroutenama, bcustom16, bcustom17, bcustom18, bcustom19, bcustom20, 
        'bcustom21, bcustom22, bcustom23, bcustom24, bcustom25, wip1kode, wip1nama, wip2kode, wip2nama, wip3kode, wip3nama, 
        'bcustom21kode, bcustom21nama, bcustom22kode, bcustom22nama, bcustom23kode, bcustom23nama

        'M1_ItemGetdataById Item Location Warehouse ---------------------------------------
        'blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, 
        'blginputtgl, blgmodifikasiuser, blgmodifikasitgl

        'M1_ItemGetdataById Item Assembly -------------------------------------------------
        'iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, 
        'iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl

        'M1_ItemGetdataById Item Supplier -------------------------------------------------
        'isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, 
        'iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, 
        'iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3, kkode, knama

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

        Dim utama As String = "", lokasigudang As String = "", assembly As String = "", supplier As String = "", idtransaksi As String = ""

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

        Dim NmMemcached As String = "aplikasi1-M4_Vp~M4_Vp_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "i.bid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "i.bid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_getdata")
        'sql = "SELECT i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.bnamaalias1 AS bnamaalias1, i.bnamaalias2 AS bnamaalias2, i.bnamaalias3 AS bnamaalias3, i.bnamaalias4 AS bnamaalias4, i.bnamaalias5 AS bnamaalias5, i.btipe AS btipe, i.bjenis AS bjenis, i.bjenisdetail AS bjenisdetail, i.bkategori AS bkategori, i.bketerangan AS bketerangan, i.bsatuan AS bsatuan, i.bnilaisatuan AS bnilaisatuan, i.bsatuandefault AS bsatuandefault, i.bnilaisatuandefault AS bnilaisatuandefault, i.bhpp AS bhpp, i.bcabang AS bcabang, i.blokasi AS blokasi, i.bdivisi AS bdivisi, i.bsubdivisi AS bsubdivisi, i.bgudang AS bgudang, i.bproyek AS bproyek, i.bsubitem AS bsubitem, i.bsubitemdari AS bsubitemdari, i.bbarcode AS bbarcode, i.bsuplier AS bsuplier, i.baktif AS baktif, i.baktiftgl AS baktiftgl, i.bstokminimal AS bstokminimal, i.bstokmaksimal AS bstokmaksimal, i.breorder AS breorder, i.bjmlorderbeli AS bjmlorderbeli, i.bjmlorderjual AS bjmlorderjual, i.bkategoriumur AS bkategoriumur, i.bstatusmoving AS bstatusmoving, i.bsifatharga AS bsifatharga, i.bpromo AS bpromo, i.bpromoberlaku AS bpromoberlaku, i.bpajakbeli AS bpajakbeli, i.bpajakjual AS bpajakjual, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bhargajual2 AS bhargajual2, i.bhargajual3 AS bhargajual3, i.bhargajual4 AS bhargajual4, i.bhargajual5 AS bhargajual5, i.bdiskonjual1 AS bdiskonjual1, i.bdiskonjual2 AS bdiskonjual2, i.bdiskonjual3 AS bdiskonjual3, i.bdiskonjual4 AS bdiskonjual4, i.bdiskonjual5 AS bdiskonjual5, (CASE i.bjenis WHEN 'J' THEN 0 ELSE i.bstok END) AS bstok, i.bkomisi AS bkomisi, i.bmarginminimal AS bmarginminimal, i.brekpersediaan AS brekpersediaan, i.brekpenjualan AS brekpenjualan, i.brekreturpenjualan AS brekreturpenjualan, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekhargapokok AS brekhargapokok, i.brekreturpembelian AS brekreturpembelian, i.brekdiskonpembelian AS brekdiskonpembelian, i.brekkonsinyasi AS brekkonsinyasi, i.bapanjang AS bapanjang, i.balebar AS balebar, i.batinggi AS batinggi, i.bavolume AS bavolume, i.baberat AS baberat, i.bawarna AS bawarna, i.baoem AS baoem, i.bamerk AS bamerk, i.baukuran AS baukuran, i.bamodel AS bamodel, i.bakelas AS bakelas, i.bserial AS bserial, i.bbatch AS bbatch, i.bpengganti AS bpengganti, i.bgambar AS bgambar, i.burutan AS burutan, i.bcustom1 AS bcustom1, i.bcustom2 AS bcustom2, i.bcustom3 AS bcustom3, i.bcustom4 AS bcustom4, i.bcustom5 AS bcustom5, i.bcustom6 AS bcustom6, i.bcustom7 AS bcustom7, i.bcustom8 AS bcustom8, i.bcustom9 AS bcustom9, i.bcustom10 AS bcustom10, i.bcustom11 AS bcustom11, i.bcustom12 AS bcustom12, i.bcustom13 AS bcustom13, i.bcustom14 AS bcustom14, i.bcustom15 AS bcustom15, i.bcatatan AS bcatatan, i.binputuser AS binputuser, i.binputtgl AS binputtgl, i.bmodifikasiuser AS bmodifikasiuser, i.bmodifikasitgl AS bmodifikasitgl, i.bedithpp AS bedithpp, i.bmobile, it.itnama AS btipenama, ic.icnama AS bkategorinama, u1.unama AS bsatuannama, u2.unama AS bsatuandefaultnama, br.bnama AS bcabangnama, lc.lnama AS blokasinama, dv.dnama AS bdivisinama, sdv.sdnama AS bsubdivisinama, wh.wnama AS bgudangnama, p.pnama AS bproyeknama, i2.bkode AS bsubitemdarikode, c.kkode AS bsuplierkode, c.knama AS bsupliernama, tax1.tnama AS bpajakbelinama, tax2.tnama AS bpajakjualnama, coa1.cnama AS brekpersediaannama, coa2.cnama AS brekpenjualannama, coa3.cnama AS brekreturpenjualannama, coa4.cnama AS brekdiskonpenjualannama, coa5.cnama AS brekhargapokoknama, coa6.cnama AS brekreturpembeliannama, coa7.cnama AS brekdiskonpembeliannama, coa8.cnama AS brekkonsinyasinama, sp.spkode AS bkomisikode, sp.spnama AS bkomisinama, ilw.blgidbarang AS blgidbarang, ilw.blgkodebarang AS blgkodebarang, ilw.blggudang AS blggudang, ilw.blgidlokasi AS blgidlokasi, ilw.blgkodelokasi AS blgkodelokasi, ilw.blgnamalokasi AS blgnamalokasi, ilw.blginputuser AS blginputuser, ilw.blginputtgl AS blginputtgl, ilw.blgmodifikasiuser AS blgmodifikasiuser, ilw.blgmodifikasitgl AS blgmodifikasitgl, i.bassembly, i.bkelasproduk, i.bretur, i.btag, i.bminorder, i.bdownloaded, cp.cpnama as bkelasproduknama, tag.ipnama as btagnama, i.bdepartemen, i.bsubdepartemen, dp.dpnama as bdepartemennama, sdp.sdpnama as bsubdepartemennama, i.bkp, i.bkl, i.bjmllapangan, i.bsatuanlapangan, cls.cnama as bakelasnama, i.bsubkelas, scl.scnama as bsubkelasnama, clr.cnama as bawarnanama, i.bdesigner, dsg.dnama as bdesignernama, mdl.mnama as bamodelnama, mrk.mnama as bamerknama, i.bmaterial, mtr.mnama as bmaterialnama, oem.onama as baoemnama, i.bsection, sct.snama as bsectionnama, sze.snama as baukurannama, i.bvendor, vdr.vnama as bvendornama, i.basset, i.bhargajual6, i.bhargajual7, i.bhargajual8, i.bhargajual9, i.bhargajual10, i.bdiskonjual6, i.bdiskonjual7, i.bdiskonjual8, i.bdiskonjual9, i.bdiskonjual10, pr.prkode AS bproductionroutekode, pr.prnama AS bproductionroutenama,i.bcustom16, i.bcustom17, i.bcustom18, i.bcustom19, i.bcustom20, i.bcustom21, i.bcustom22, i.bcustom23, i.bcustom24, i.bcustom25, wip1.bkode as wip1kode, wip1.bnama as wip1nama,wip2.bkode as wip2kode, wip2.bnama as wip2nama,wip3.bkode as wip3kode, wip3.bnama as wip3nama from m1_item i left join m1_item_type it on i.btipe = it.itkode left join m1_item_category ic on i.bkategori = ic.ickode left join m1_unit u1 on i.bsatuan = u1.ukode left join m1_unit u2 on i.bsatuandefault = u2.ukode left join m1_branch br on i.bcabang = br.bkode left join m1_division dv on i.bdivisi = dv.dkode left join m1_subdivision sdv on i.bsubdivisi = sdv.sdkode left join m1_location lc on i.blokasi = lc.lkode left join m1_warehouse wh on i.bgudang = wh.wkode left join m1_project p on i.bproyek = p.pkode left join m1_item i2 on i.bsubitemdari = i2.bid left join m1_contact c on i.bsuplier = c.kid left join m1_tax tax1 on i.bpajakbeli = tax1.tkode left join m1_tax tax2 on i.bpajakjual = tax2.tkode left join m1_coa coa1 on i.brekpersediaan = coa1.cnomor left join m1_coa coa2 on i.brekpenjualan = coa2.cnomor left join m1_coa coa3 on i.brekreturpenjualan = coa3.cnomor left join m1_coa coa4 on i.brekdiskonpenjualan = coa4.cnomor left join m1_coa coa5 on i.brekhargapokok = coa5.cnomor left join m1_coa coa6 on i.brekreturpembelian = coa6.cnomor left join m1_coa coa7 on i.brekdiskonpembelian = coa7.cnomor left join m1_coa coa8 on i.brekkonsinyasi = coa8.cnomor left join m1_item_location_warehouse ilw on i.bid = ilw.blgidbarang left join m1_selling_point sp on i.bkomisi = sp.spid left join m1_class_product cp on i.bkelasproduk = cp.cpkode left join m1_department dp on i.bdepartemen = dp.dpkode left join m1_subdepartment sdp on i.bsubdepartemen = sdp.sdpkode left join m1_item_permission tag ON tag.ipkode = i.btag left join m1_class cls on i.bakelas = cls.ckode left join m1_subclass scl on i.bsubkelas = scl.sckode left join m1_color clr on i.bawarna = clr.ckode left join m1_designer dsg on i.bdesigner = dsg.dkode left join m1_model mdl on i.bamodel = mdl.mkode left join m1_merk mrk on i.bamerk = mrk.mkode left join m1_material mtr on i.bmaterial = mtr.mkode  left join m1_oem oem on i.baoem = oem.okode left join m1_section sct on i.bsection = sct.skode left join m1_size sze on i.baukuran = sze.skode  left join m1_vendor vdr on i.bvendor = vdr.vkode LEFT JOIN m1_production_route pr ON pr.prid = i.bcustom11 left join m1_item wip1 on wip1.bid = i.bcustom16 left join m1_item wip2 on wip2.bid = i.bcustom17 left join m1_item wip3 on wip3.bid = i.bcustom18"
        sql = "SELECT i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.bnamaalias1 AS bnamaalias1, i.bnamaalias2 AS bnamaalias2, i.bnamaalias3 AS bnamaalias3, i.bnamaalias4 AS bnamaalias4, i.bnamaalias5 AS bnamaalias5, i.btipe AS btipe, i.bjenis AS bjenis, i.bjenisdetail AS bjenisdetail, i.bkategori AS bkategori, i.bketerangan AS bketerangan, i.bsatuan AS bsatuan, i.bnilaisatuan AS bnilaisatuan, i.bsatuandefault AS bsatuandefault, i.bnilaisatuandefault AS bnilaisatuandefault, i.bhpp AS bhpp, i.bcabang AS bcabang, i.blokasi AS blokasi, i.bdivisi AS bdivisi, i.bsubdivisi AS bsubdivisi, i.bgudang AS bgudang, i.bproyek AS bproyek, i.bsubitem AS bsubitem, i.bsubitemdari AS bsubitemdari, i.bbarcode AS bbarcode, i.bsuplier AS bsuplier, i.baktif AS baktif, i.baktiftgl AS baktiftgl, i.bstokminimal AS bstokminimal, i.bstokmaksimal AS bstokmaksimal, i.breorder AS breorder, i.bjmlorderbeli AS bjmlorderbeli, i.bjmlorderjual AS bjmlorderjual, i.bkategoriumur AS bkategoriumur, i.bstatusmoving AS bstatusmoving, i.bsifatharga AS bsifatharga, i.bpromo AS bpromo, i.bpromoberlaku AS bpromoberlaku, i.bpajakbeli AS bpajakbeli, i.bpajakjual AS bpajakjual, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bhargajual2 AS bhargajual2, i.bhargajual3 AS bhargajual3, i.bhargajual4 AS bhargajual4, i.bhargajual5 AS bhargajual5, i.bdiskonjual1 AS bdiskonjual1, i.bdiskonjual2 AS bdiskonjual2, i.bdiskonjual3 AS bdiskonjual3, i.bdiskonjual4 AS bdiskonjual4, i.bdiskonjual5 AS bdiskonjual5, (CASE i.bjenis WHEN 'J' THEN 0 ELSE i.bstok END) AS bstok, i.bkomisi AS bkomisi, i.bmarginminimal AS bmarginminimal, i.brekpersediaan AS brekpersediaan, i.brekpenjualan AS brekpenjualan, i.brekreturpenjualan AS brekreturpenjualan, i.brekdiskonpenjualan AS brekdiskonpenjualan, i.brekhargapokok AS brekhargapokok, i.brekreturpembelian AS brekreturpembelian, i.brekdiskonpembelian AS brekdiskonpembelian, i.brekkonsinyasi AS brekkonsinyasi, i.bapanjang AS bapanjang, i.balebar AS balebar, i.batinggi AS batinggi, i.bavolume AS bavolume, i.baberat AS baberat, i.bawarna AS bawarna, i.baoem AS baoem, i.bamerk AS bamerk, i.baukuran AS baukuran, i.bamodel AS bamodel, i.bakelas AS bakelas, i.bserial AS bserial, i.bbatch AS bbatch, i.bpengganti AS bpengganti, i.bgambar AS bgambar, i.burutan AS burutan, i.bcustom1 AS bcustom1, i.bcustom2 AS bcustom2, i.bcustom3 AS bcustom3, i.bcustom4 AS bcustom4, i.bcustom5 AS bcustom5, i.bcustom6 AS bcustom6, i.bcustom7 AS bcustom7, i.bcustom8 AS bcustom8, i.bcustom9 AS bcustom9, i.bcustom10 AS bcustom10, i.bcustom11 AS bcustom11, i.bcustom12 AS bcustom12, i.bcustom13 AS bcustom13, i.bcustom14 AS bcustom14, i.bcustom15 AS bcustom15, i.bcatatan AS bcatatan, i.binputuser AS binputuser, i.binputtgl AS binputtgl, i.bmodifikasiuser AS bmodifikasiuser, i.bmodifikasitgl AS bmodifikasitgl, i.bedithpp AS bedithpp, i.bmobile, it.itnama AS btipenama, ic.icnama AS bkategorinama, u1.unama AS bsatuannama, u2.unama AS bsatuandefaultnama, br.bnama AS bcabangnama, lc.lnama AS blokasinama, dv.dnama AS bdivisinama, sdv.sdnama AS bsubdivisinama, wh.wnama AS bgudangnama, p.pnama AS bproyeknama, i2.bkode AS bsubitemdarikode, c.kkode AS bsuplierkode, c.knama AS bsupliernama, tax1.tnama AS bpajakbelinama, tax2.tnama AS bpajakjualnama, coa1.cnama AS brekpersediaannama, coa2.cnama AS brekpenjualannama, coa3.cnama AS brekreturpenjualannama, coa4.cnama AS brekdiskonpenjualannama, coa5.cnama AS brekhargapokoknama, coa6.cnama AS brekreturpembeliannama, coa7.cnama AS brekdiskonpembeliannama, coa8.cnama AS brekkonsinyasinama, sp.spkode AS bkomisikode, sp.spnama AS bkomisinama, ilw.blgidbarang AS blgidbarang, ilw.blgkodebarang AS blgkodebarang, ilw.blggudang AS blggudang, ilw.blgidlokasi AS blgidlokasi, ilw.blgkodelokasi AS blgkodelokasi, ilw.blgnamalokasi AS blgnamalokasi, ilw.blginputuser AS blginputuser, ilw.blginputtgl AS blginputtgl, ilw.blgmodifikasiuser AS blgmodifikasiuser, ilw.blgmodifikasitgl AS blgmodifikasitgl, i.bassembly, i.bkelasproduk, i.bretur, i.btag, i.bminorder, i.bdownloaded, cp.cpnama as bkelasproduknama, tag.ipnama as btagnama, i.bdepartemen, i.bsubdepartemen, dp.dpnama as bdepartemennama, sdp.sdpnama as bsubdepartemennama, i.bkp, i.bkl, i.bjmllapangan, i.bsatuanlapangan, cls.cnama as bakelasnama, i.bsubkelas, scl.scnama as bsubkelasnama, clr.cnama as bawarnanama, i.bdesigner, dsg.dnama as bdesignernama, mdl.mnama as bamodelnama, mrk.mnama as bamerknama, i.bmaterial, mtr.mnama as bmaterialnama, oem.onama as baoemnama, i.bsection, sct.snama as bsectionnama, sze.snama as baukurannama, i.bvendor, vdr.vnama as bvendornama, i.basset, i.bhargajual6, i.bhargajual7, i.bhargajual8, i.bhargajual9, i.bhargajual10, i.bdiskonjual6, i.bdiskonjual7, i.bdiskonjual8, i.bdiskonjual9, i.bdiskonjual10, pr.prkode AS bproductionroutekode, pr.prnama AS bproductionroutenama,i.bcustom16, i.bcustom17, i.bcustom18, i.bcustom19, i.bcustom20, i.bcustom21, i.bcustom22, i.bcustom23, i.bcustom24, i.bcustom25, wip1.bkode as wip1kode, wip1.bnama as wip1nama,wip2.bkode as wip2kode, wip2.bnama as wip2nama,wip3.bkode as wip3kode, wip3.bnama as wip3nama, mold1.bkode as bcustom21kode, mold1.bnama as bcustom21nama, mold2.bkode as bcustom22kode, mold2.bnama as bcustom22nama, mold3.bkode as bcustom23kode, mold3.bnama as bcustom23nama from m1_item i left join m1_item_type it on i.btipe = it.itkode left join m1_item_category ic on i.bkategori = ic.ickode left join m1_unit u1 on i.bsatuan = u1.ukode left join m1_unit u2 on i.bsatuandefault = u2.ukode left join m1_branch br on i.bcabang = br.bkode left join m1_division dv on i.bdivisi = dv.dkode left join m1_subdivision sdv on i.bsubdivisi = sdv.sdkode left join m1_location lc on i.blokasi = lc.lkode left join m1_warehouse wh on i.bgudang = wh.wkode left join m1_project p on i.bproyek = p.pkode left join m1_item i2 on i.bsubitemdari = i2.bid left join m1_contact c on i.bsuplier = c.kid left join m1_tax tax1 on i.bpajakbeli = tax1.tkode left join m1_tax tax2 on i.bpajakjual = tax2.tkode left join m1_coa coa1 on i.brekpersediaan = coa1.cnomor left join m1_coa coa2 on i.brekpenjualan = coa2.cnomor left join m1_coa coa3 on i.brekreturpenjualan = coa3.cnomor left join m1_coa coa4 on i.brekdiskonpenjualan = coa4.cnomor left join m1_coa coa5 on i.brekhargapokok = coa5.cnomor left join m1_coa coa6 on i.brekreturpembelian = coa6.cnomor left join m1_coa coa7 on i.brekdiskonpembelian = coa7.cnomor left join m1_coa coa8 on i.brekkonsinyasi = coa8.cnomor left join m1_item_location_warehouse ilw on i.bid = ilw.blgidbarang left join m1_selling_point sp on i.bkomisi = sp.spid left join m1_class_product cp on i.bkelasproduk = cp.cpkode left join m1_department dp on i.bdepartemen = dp.dpkode left join m1_subdepartment sdp on i.bsubdepartemen = sdp.sdpkode left join m1_item_permission tag ON tag.ipkode = i.btag left join m1_class cls on i.bakelas = cls.ckode left join m1_subclass scl on i.bsubkelas = scl.sckode left join m1_color clr on i.bawarna = clr.ckode left join m1_designer dsg on i.bdesigner = dsg.dkode left join m1_model mdl on i.bamodel = mdl.mkode left join m1_merk mrk on i.bamerk = mrk.mkode left join m1_material mtr on i.bmaterial = mtr.mkode  left join m1_oem oem on i.baoem = oem.okode left join m1_section sct on i.bsection = sct.skode left join m1_size sze on i.baukuran = sze.skode  left join m1_vendor vdr on i.bvendor = vdr.vkode LEFT JOIN m1_production_route pr ON pr.prid = i.bcustom11 left join m1_item wip1 on wip1.bid = i.bcustom16 left join m1_item wip2 on wip2.bid = i.bcustom17 left join m1_item wip3 on wip3.bid = i.bcustom18 left join m1_item mold1 on mold1.bid = i.bcustom21 left join m1_item mold2 on mold2.bid = i.bcustom22 left join m1_item mold3 on mold3.bid = i.bcustom23 "

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)

            'CEK TERKAIT ====================================================================
            'PANGGIL QUERY TERKAIT
            sql = query.PanggilQuery("m1_item_terkait")
            sql = sql.Replace("valkode", idtransaksi)
            Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
            'END OF CEK TERKAIT =============================================================

            utama = String.Concat(FxDB(drutama("bid"), 0), sptField,
                     FxDB(drutama("bkode"), ""), sptField,
                     FxDB(drutama("bnama"), ""), sptField,
                     FxDB(drutama("bnamaalias1"), ""), sptField,
                     FxDB(drutama("bnamaalias2"), ""), sptField,
                     FxDB(drutama("bnamaalias3"), ""), sptField,
                     FxDB(drutama("bnamaalias4"), ""), sptField,
                     FxDB(drutama("bnamaalias5"), ""), sptField,
                     FxDB(drutama("btipe"), ""), sptField,
                     FxDB(drutama("bjenis"), ""), sptField,
                     FxDB(drutama("bjenisdetail"), 0), sptField,
                     FxDB(drutama("bkategori"), ""), sptField,
                     FxDB(drutama("bketerangan"), ""), sptField,
                     FxDB(drutama("bsatuan"), ""), sptField,
                     FxDB(drutama("bnilaisatuan"), 0), sptField,
                     FxDB(drutama("bsatuandefault"), ""), sptField,
                     FxDB(drutama("bnilaisatuandefault"), 0), sptField,
                     FxDB(drutama("bhpp"), ""), sptField,
                     FxDB(drutama("bcabang"), ""), sptField,
                     FxDB(drutama("blokasi"), ""), sptField,
                     FxDB(drutama("bdivisi"), ""), sptField,
                     FxDB(drutama("bsubdivisi"), ""), sptField,
                     FxDB(drutama("bgudang"), ""), sptField,
                     FxDB(drutama("bproyek"), ""), sptField,
                     FxDB(drutama("bsubitem"), 0), sptField,
                     FxDB(drutama("bsubitemdari"), 0), sptField,
                     FxDB(drutama("bbarcode"), ""), sptField,
                     FxDB(drutama("bsuplier"), 0), sptField,
                     FxDB(drutama("baktif"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("bstokminimal"), 0), sptField,
                     FxDB(drutama("bstokmaksimal"), 0), sptField,
                     FxDB(drutama("breorder"), 0), sptField,
                     FxDB(drutama("bjmlorderbeli"), 0), sptField,
                     FxDB(drutama("bjmlorderjual"), 0), sptField,
                     FxDB(drutama("bkategoriumur"), ""), sptField,
                     FxDB(drutama("bstatusmoving"), ""), sptField,
                     FxDB(drutama("bsifatharga"), ""), sptField,
                     FxDB(drutama("bpromo"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bpromoberlaku"), ""), formatTgl), sptField,
                     FxDB(drutama("bpajakbeli"), ""), sptField,
                     FxDB(drutama("bpajakjual"), ""), sptField,
                     FxDB(drutama("bhargabeli"), 0), sptField,
                     FxDB(drutama("bhppaverage"), 0), sptField,
                     FxDB(drutama("bhargajual1"), 0), sptField,
                     FxDB(drutama("bhargajual2"), 0), sptField,
                     FxDB(drutama("bhargajual3"), 0), sptField,
                     FxDB(drutama("bhargajual4"), 0), sptField,
                     FxDB(drutama("bhargajual5"), 0), sptField,
                     FxDB(drutama("bdiskonjual1"), 0), sptField,
                     FxDB(drutama("bdiskonjual2"), 0), sptField,
                     FxDB(drutama("bdiskonjual3"), 0), sptField,
                     FxDB(drutama("bdiskonjual4"), 0), sptField,
                     FxDB(drutama("bdiskonjual5"), 0), sptField,
                     FxDB(drutama("bstok"), 0), sptField,
                     FxDB(drutama("bkomisi"), 0), sptField,
                     FxDB(drutama("bmarginminimal"), 0), sptField,
                     FxDB(drutama("brekpersediaan"), ""), sptField,
                     FxDB(drutama("brekpenjualan"), ""), sptField,
                     FxDB(drutama("brekreturpenjualan"), ""), sptField,
                     FxDB(drutama("brekdiskonpenjualan"), ""), sptField,
                     FxDB(drutama("brekhargapokok"), ""), sptField,
                     FxDB(drutama("brekreturpembelian"), ""), sptField,
                     FxDB(drutama("brekdiskonpembelian"), ""), sptField,
                     FxDB(drutama("brekkonsinyasi"), ""), sptField,
                     FxDB(drutama("bapanjang"), 0), sptField,
                     FxDB(drutama("balebar"), 0), sptField,
                     FxDB(drutama("batinggi"), 0), sptField,
                     FxDB(drutama("bavolume"), 0), sptField,
                     FxDB(drutama("baberat"), 0), sptField,
                     FxDB(drutama("bawarna"), ""), sptField,
                     FxDB(drutama("baoem"), ""), sptField,
                     FxDB(drutama("bamerk"), ""), sptField,
                     FxDB(drutama("baukuran"), ""), sptField,
                     FxDB(drutama("bamodel"), ""), sptField,
                     FxDB(drutama("bakelas"), ""), sptField,
                     FxDB(drutama("bserial"), 0), sptField,
                     FxDB(drutama("bbatch"), 0), sptField,
                     FxDB(drutama("bpengganti"), 0), sptField,
                     FxDB(drutama("bgambar"), ""), sptField,
                     FxDB(drutama("burutan"), 0), sptField,
                     FxDB(drutama("bcustom1"), ""), sptField,
                     FxDB(drutama("bcustom2"), ""), sptField,
                     FxDB(drutama("bcustom3"), ""), sptField,
                     FxDB(drutama("bcustom4"), ""), sptField,
                     FxDB(drutama("bcustom5"), ""), sptField,
                     FxDB(drutama("bcustom6"), ""), sptField,
                     FxDB(drutama("bcustom7"), ""), sptField,
                     FxDB(drutama("bcustom8"), ""), sptField,
                     FxDB(drutama("bcustom9"), ""), sptField,
                     FxDB(drutama("bcustom10"), ""), sptField,
                     FxDB(drutama("bcustom11"), 0), sptField,
                     FxDB(drutama("bcustom12"), 0), sptField,
                     FxDB(drutama("bcustom13"), 0), sptField,
                     FxDB(drutama("bcustom14"), 0), sptField,
                     FxDB(drutama("bcustom15"), 0), sptField,
                     FxDB(drutama("bcatatan"), ""), sptField,
                     FxDB(drutama("binputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("binputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bmodifikasiuser"), 0), sptField,
                     AsFormatTanggal(FxDB(drutama("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("bedithpp"), 0), sptField,
                     FxDB(drutama("btipenama"), ""), sptField,
                     FxDB(drutama("bkategorinama"), ""), sptField,
                     FxDB(drutama("bsatuannama"), ""), sptField,
                     FxDB(drutama("bsatuandefaultnama"), ""), sptField,
                     FxDB(drutama("bcabangnama"), ""), sptField,
                     FxDB(drutama("blokasinama"), ""), sptField,
                     FxDB(drutama("bdivisinama"), ""), sptField,
                     FxDB(drutama("bsubdivisinama"), ""), sptField,
                     FxDB(drutama("bgudangnama"), ""), sptField,
                     FxDB(drutama("bproyeknama"), ""), sptField,
                     FxDB(drutama("bsubitemdarikode"), ""), sptField,
                     FxDB(drutama("bsuplierkode"), ""), sptField,
                     FxDB(drutama("bsupliernama"), ""), sptField,
                     FxDB(drutama("bpajakbelinama"), ""), sptField,
                     FxDB(drutama("bpajakjualnama"), ""), sptField,
                     FxDB(drutama("brekpersediaannama"), ""), sptField,
                     FxDB(drutama("brekpenjualannama"), ""), sptField,
                     FxDB(drutama("brekreturpenjualannama"), ""), sptField,
                     FxDB(drutama("brekdiskonpenjualannama"), ""), sptField,
                     FxDB(drutama("brekhargapokoknama"), ""), sptField,
                     FxDB(drutama("brekreturpembeliannama"), ""), sptField,
                     FxDB(drutama("brekdiskonpembeliannama"), ""), sptField,
                     FxDB(drutama("brekkonsinyasinama"), ""), sptField,
                     dtTerkait.Rows.Count, sptField,
                     FxDB(drutama("bkomisikode"), ""), sptField,
                     FxDB(drutama("bkomisinama"), ""), sptField,
                     FxDB(drutama("bmobile"), 0), sptField,
                     FxDB(drutama("bassembly"), 0), sptField,
                     FxDB(drutama("bkelasproduk"), ""), sptField,
                     FxDB(drutama("bretur"), 0), sptField,
                     FxDB(drutama("btag"), ""), sptField,
                     FxDB(drutama("bminorder"), 0), sptField,
                     FxDB(drutama("bdownloaded"), 0), sptField,
                     FxDB(drutama("bkelasproduknama"), ""), sptField,
                     FxDB(drutama("btagnama"), ""), sptField,
                     FxDB(drutama("bdepartemen"), ""), sptField,
                     FxDB(drutama("bsubdepartemen"), ""), sptField,
                     FxDB(drutama("bdepartemennama"), ""), sptField,
                     FxDB(drutama("bsubdepartemennama"), ""), sptField,
                     FxDB(drutama("bkp"), 0), sptField,
                     FxDB(drutama("bkl"), 0), sptField,
                     FxDB(drutama("bjmllapangan"), 0), sptField,
                     FxDB(drutama("bsatuanlapangan"), ""), sptField,
                     FxDB(drutama("bakelasnama"), ""), sptField,
                     FxDB(drutama("bsubkelasnama"), ""), sptField,
                     FxDB(drutama("bawarnanama"), ""), sptField,
                     FxDB(drutama("bdesignernama"), ""), sptField,
                     FxDB(drutama("bamodelnama"), ""), sptField,
                     FxDB(drutama("bamerknama"), ""), sptField,
                     FxDB(drutama("bmaterialnama"), ""), sptField,
                     FxDB(drutama("baoemnama"), ""), sptField,
                     FxDB(drutama("bsectionnama"), ""), sptField,
                     FxDB(drutama("baukurannama"), ""), sptField,
                     FxDB(drutama("bvendornama"), ""), sptField,
                     FxDB(drutama("bsubkelas"), ""), sptField,
                     FxDB(drutama("bdesigner"), ""), sptField,
                     FxDB(drutama("bmaterial"), ""), sptField,
                     FxDB(drutama("bsection"), ""), sptField,
                     FxDB(drutama("bvendor"), ""), sptField,
                     FxDB(drutama("basset"), 0), sptField,
                     FxDB(drutama("bhargajual6"), 0), sptField,
                     FxDB(drutama("bhargajual7"), 0), sptField,
                     FxDB(drutama("bhargajual8"), 0), sptField,
                     FxDB(drutama("bhargajual9"), 0), sptField,
                     FxDB(drutama("bhargajual10"), 0), sptField,
                     FxDB(drutama("bdiskonjual6"), 0), sptField,
                     FxDB(drutama("bdiskonjual7"), 0), sptField,
                     FxDB(drutama("bdiskonjual8"), 0), sptField,
                     FxDB(drutama("bdiskonjual9"), 0), sptField,
                     FxDB(drutama("bdiskonjual10"), 0), sptField,
                     FxDB(drutama("bproductionroutekode"), ""), sptField,
                     FxDB(drutama("bproductionroutenama"), ""), sptField,
                     FxDB(drutama("bcustom16"), 0), sptField,
                     FxDB(drutama("bcustom17"), 0), sptField,
                     FxDB(drutama("bcustom18"), 0), sptField,
                     FxDB(drutama("bcustom19"), 0), sptField,
                     FxDB(drutama("bcustom20"), 0), sptField,
                     FxDB(drutama("bcustom21"), 0), sptField,
                     FxDB(drutama("bcustom22"), 0), sptField,
                     FxDB(drutama("bcustom23"), 0), sptField,
                     FxDB(drutama("bcustom24"), 0), sptField,
                     FxDB(drutama("bcustom25"), 0), sptField,
                     FxDB(drutama("wip1kode"), ""), sptField,
                     FxDB(drutama("wip1nama"), ""), sptField,
                     FxDB(drutama("wip2kode"), ""), sptField,
                     FxDB(drutama("wip2nama"), ""), sptField,
                     FxDB(drutama("wip3kode"), ""), sptField,
                     FxDB(drutama("wip3nama"), ""), sptField,
                     FxDB(drutama("bcustom21kode"), ""), sptField,
                     FxDB(drutama("bcustom21nama"), ""), sptField,
                     FxDB(drutama("bcustom22kode"), ""), sptField,
                     FxDB(drutama("bcustom22nama"), ""), sptField,
                     FxDB(drutama("bcustom23kode"), ""), sptField,
                     FxDB(drutama("bcustom23nama"), ""))

            Dim inputtgl As String = "", modiftgl As String = ""
            For Each dr As DataRow In dt.Rows
                inputtgl = FxDB(dr("blginputtgl"), "")
                modiftgl = FxDB(dr("blgmodifikasitgl"), "")

                If Len(inputtgl) > 0 Then inputtgl = AsFormatTanggal(inputtgl, formatTglWaktu)
                If Len(modiftgl) > 0 Then modiftgl = AsFormatTanggal(modiftgl, formatTglWaktu)

                lokasigudang = String.Concat(lokasigudang,
                     FxDB(dr("blgidbarang"), 0), sptField,
                     FxDB(dr("blgkodebarang"), ""), sptField,
                     FxDB(dr("blggudang"), ""), sptField,
                     FxDB(dr("blgidlokasi"), 0), sptField,
                     FxDB(dr("blgkodelokasi"), ""), sptField,
                     FxDB(dr("blgnamalokasi"), ""), sptField,
                     FxDB(dr("blginputuser"), 0), sptField,
                     inputtgl, sptField,
                     FxDB(dr("blgmodifikasiuser"), 0), sptField,
                     modiftgl, sptRow)
            Next
            If lokasigudang.Length > 0 Then lokasigudang = lokasigudang.Substring(0, lokasigudang.Length - sptRow.Length) Else lokasigudang = lokasigudang

            'AMBIL DATA ITEM ASSEMBLY
            Dim dtassembly As New DataTable
            dtassembly = AmbilData("aplikasi1-M1_Item_Assembly", "iaidbarang=" & idtransaksi, "iaidbarang ASC, iaurutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , ) ' Ambil data ke databases
            For Each dr As DataRow In dtassembly.Rows
                assembly = String.Concat(assembly,
                             FxDB(dr("iaidbarang"), 0), sptField,
                             FxDB(dr("iakodebarang"), ""), sptField,
                             FxDB(dr("iaidbarangpenyusun"), 0), sptField,
                             FxDB(dr("iakodebarangpenyusun"), ""), sptField,
                             FxDB(dr("iaurutan"), 0), sptField,
                             FxDB(dr("iajml"), 0), sptField,
                             FxDB(dr("iasatuan"), ""), sptField,
                             FxDB(dr("iainputuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("iainputtgl"), ""), formatTglWaktu), sptField,
                             FxDB(dr("iamodifikasiuser"), 0), sptField,
                             AsFormatTanggal(FxDB(dr("iamodifikasitgl"), ""), formatTglWaktu), sptRow)
            Next
            If assembly.Length > 0 Then assembly = assembly.Substring(0, assembly.Length - sptRow.Length) Else assembly = assembly

            'AMBIL DATA ITEM Supplier
            Dim dtSupplier As New DataTable
            sql = "SELECT its.isidbarang, its.isidkontak, its.iscatatan, its.isurutan, its.iscustomtext1, its.iscustomtext2, its.iscustomtext3, its.iscustomtext4, its.iscustomtext5, its.iscustomint1, its.iscustomint2, its.iscustomint3, its.iscustomdbl1, its.iscustomdbl2, its.iscustomdbl3, its.iscustomdate1, its.iscustomdate2, its.iscustomdate3, c.kkode, c.knama FROM m1_item_supplier its JOIN m1_contact c ON its.isidkontak = c.kid"
            dtSupplier = AmbilData("aplikasi1-M1_Item_Supplier", "isidbarang = " & idtransaksi, "isidbarang ASC, isurutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtSupplier.Rows
                supplier = String.Concat(supplier,
                     FxDB(dr("isidbarang"), 0), sptField,
                     FxDB(dr("isidkontak"), 0), sptField,
                     FxDB(dr("iscatatan"), ""), sptField,
                     FxDB(dr("isurutan"), 0), sptField,
                     FxDB(dr("iscustomtext1"), ""), sptField,
                     FxDB(dr("iscustomtext2"), ""), sptField,
                     FxDB(dr("iscustomtext3"), ""), sptField,
                     FxDB(dr("iscustomtext4"), ""), sptField,
                     FxDB(dr("iscustomtext5"), ""), sptField,
                     FxDB(dr("iscustomint1"), 0), sptField,
                     FxDB(dr("iscustomint2"), 0), sptField,
                     FxDB(dr("iscustomint3"), 0), sptField,
                     FxDB(dr("iscustomdbl1"), 0), sptField,
                     FxDB(dr("iscustomdbl2"), 0), sptField,
                     FxDB(dr("iscustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("iscustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("kkode"), ""), sptField,
                     FxDB(dr("knama"), ""), sptRow)
            Next
            If supplier.Length > 0 Then supplier = supplier.Substring(0, supplier.Length - sptRow.Length) Else supplier = supplier

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "item transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        strResultData = String.Concat(utama, sptSubParam, lokasigudang, sptSubParam, assembly, sptSubParam, supplier)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, btipenama, bkategorinama, bsatuannama, bsatuandefaultnama, bcabangnama, blokasinama, bdivisinama, bsubdivisinama, bgudangnama, bproyeknama, bsubitemdarikode, bsuplierkode, bsupliernama, bpajakbelinama, bpajakjualnama, brekpersediaannama, brekpenjualannama, brekreturpenjualannama, brekdiskonpenjualannama, brekhargapokoknama, brekreturpembeliannama, brekdiskonpembeliannama, brekkonsinyasinama, bjmlterkait, bkomisikode, bkomisinama, bmobile, bassembly, bkelasproduk, bretur, btag, bminorder, bdownloaded, bkelasproduknama, btagnama, bdepartemen, bsubdepartemen, bdepartemennama, bsubdepartemennama, bkp, bkl, bjmllapangan, bsatuanlapangan, bakelasnama, bsubkelasnama, bawarnanama, bdesignernama, bamodelnama, bamerknama, bmaterialnama, baoemnama, bsectionnama, baukurannama, bvendornama, bsubkelas, bdesigner, bmaterial, bsection, bvendor, basset, bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10, bproductionroutekode, bproductionroutenama, bcustom16, bcustom17, bcustom18, bcustom19, bcustom20, bcustom21, bcustom22, bcustom23, bcustom24, bcustom25, wip1kode, wip1nama, wip2kode, wip2nama, wip3kode, wip3nama, bcustom21kode, bcustom21nama, bcustom22kode, bcustom22nama, bcustom23kode, bcustom23nama" & sptSubParam & "blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl" & sptSubParam & "iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl" & sptSubParam & "isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3, kkode, knama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_DataSearch(ByVal param As String) As String
        'M1_Item_DataSearch --------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bjmlorderbeli, bjmlorderjual, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bserial, 
        'bbatch, bcatatan, fnamafile, bkp, bkl, bjmllapangan, bsatuanlapangan, baktif, baktiftgl, basset,
        'bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, 
        'bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10, binputuser, binputtgl, binputusernama, bstokminimal, bstokmaksimal

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

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m1_item_data_v")
        sql = "select i.bid AS bid, i.bkode AS bkode, i.bnama AS bnama, i.bnamaalias1 AS bnamaalias1, i.bnamaalias2 AS bnamaalias2, i.bnamaalias3 AS bnamaalias3, i.bnamaalias4 AS bnamaalias4, i.bnamaalias5 AS bnamaalias5, i.btipe AS btipe, i.bjenis AS bjenis, i.bjenisdetail AS bjenisdetail, i.bkategori AS bkategori, i.bketerangan AS bketerangan, i.bsatuan AS bsatuan, i.bnilaisatuan AS bnilaisatuan, i.bsatuandefault AS bsatuandefault, i.bnilaisatuandefault AS bnilaisatuandefault, i.bhpp AS bhpp, i.bcabang AS bcabang, i.blokasi AS blokasi, i.bdivisi AS bdivisi, i.bsubdivisi AS bsubdivisi, i.bgudang AS bgudang, i.bproyek AS bproyek, ifnull(sum(ibpo.jmlbooking), 0) AS bjmlorderbeli, ifnull(sum(ib.jmlbooking), 0) AS bjmlorderjual, i.bpajakbeli AS bpajakbeli, i.bpajakjual AS bpajakjual, i.bhargabeli AS bhargabeli, i.bhppaverage AS bhppaverage, i.bhargajual1 AS bhargajual1, i.bhargajual2 AS bhargajual2, i.bhargajual3 AS bhargajual3, i.bhargajual4 AS bhargajual4, i.bhargajual5 AS bhargajual5, i.bdiskonjual1 AS bdiskonjual1, i.bdiskonjual2 AS bdiskonjual2, i.bdiskonjual3 AS bdiskonjual3, i.bdiskonjual4 AS bdiskonjual4, i.bdiskonjual5 AS bdiskonjual5, (CASE i.bjenis WHEN 'J' THEN 0 ELSE i.bstok END) AS bstok, i.bserial AS bserial, i.bbatch AS bbatch, i.bcatatan AS bcatatan, ifnull(f.fnamafile,'') AS fnamafile, i.bkp, i.bkl, i.bjmllapangan, i.bsatuanlapangan, i.baktif, i.baktiftgl, i.basset, i.bhargajual6, i.bhargajual7, i.bhargajual8, i.bhargajual9, i.bhargajual10, i.bdiskonjual6, i.bdiskonjual7, i.bdiskonjual8, i.bdiskonjual9, i.bdiskonjual10, i.binputtgl, i.binputuser, i.bmodifikasitgl, i.bmodifikasiuser, uinput.unama AS binputusernama, umodif.unama AS bmodifikasiusernama, i.bstokminimal, i.bstokmaksimal from m1_item i left JOIN m0_user uinput ON i.binputuser = uinput.userid left JOIN m0_user umodif ON i.bmodifikasiuser = umodif.userid left join m1_item_booking ib on i.bid = ib.idbarang left join m1_item_booking_po ibpo on i.bid = ibpo.idbarang left join m1_files f on (i.bid = f.fidtransaksi) and (f.fdefault = 1) and (f.fsumber = 'Item') left join m1_item_supplier its on i.bid = its.isidbarang left join m1_contact c on its.isidkontak = c.kid"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "bid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), 0), sptField,
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
                     FxDB(dr("bjmlorderbeli"), 0), sptField,
                     FxDB(dr("bjmlorderjual"), 0), sptField,
                     FxDB(dr("bpajakbeli"), ""), sptField,
                     FxDB(dr("bpajakjual"), ""), sptField,
                     FxDB(dr("bhargabeli"), 0), sptField,
                     FxDB(dr("bhppaverage"), 0), sptField,
                     FxDB(dr("bhargajual1"), 0), sptField,
                     FxDB(dr("bhargajual2"), 0), sptField,
                     FxDB(dr("bhargajual3"), 0), sptField,
                     FxDB(dr("bhargajual4"), 0), sptField,
                     FxDB(dr("bhargajual5"), 0), sptField,
                     FxDB(dr("bdiskonjual1"), 0), sptField,
                     FxDB(dr("bdiskonjual2"), 0), sptField,
                     FxDB(dr("bdiskonjual3"), 0), sptField,
                     FxDB(dr("bdiskonjual4"), 0), sptField,
                     FxDB(dr("bdiskonjual5"), 0), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bserial"), 0), sptField,
                     FxDB(dr("bbatch"), 0), sptField,
                     FxDB(dr("bcatatan"), ""), sptField,
                     FxDB(dr("fnamafile"), ""), sptField,
                     FxDB(dr("bkp"), 0), sptField,
                     FxDB(dr("bkl"), 0), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), 0), sptField,
                     FxDB(dr("baktif"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("bhargajual6"), 0), sptField,
                     FxDB(dr("bhargajual7"), 0), sptField,
                     FxDB(dr("bhargajual8"), 0), sptField,
                     FxDB(dr("bhargajual9"), 0), sptField,
                     FxDB(dr("bhargajual10"), 0), sptField,
                     FxDB(dr("bdiskonjual6"), 0), sptField,
                     FxDB(dr("bdiskonjual7"), 0), sptField,
                     FxDB(dr("bdiskonjual8"), 0), sptField,
                     FxDB(dr("bdiskonjual9"), 0), sptField,
                     FxDB(dr("bdiskonjual10"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTgl), sptField,
                     FxDB(dr("binputuser"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTgl), sptField,
                     FxDB(dr("bmodifikasiuser"), 0), sptField,
                     FxDB(dr("binputusernama"), ""), sptField,
                     FxDB(dr("bmodifikasiusernama"), ""), sptField,
                     FxDB(dr("bstokminimal"), 0), sptField,
                     FxDB(dr("bstokmaksimal"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Item data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bjmlorderbeli, bjmlorderjual, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bserial, bbatch, bcatatan, fnamafile, bkp, bkl, bjmllapangan, bsatuanlapangan, baktif, baktiftgl, basset, bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10, binputtgl, binputuser, bmodifikasitgl, bmodifikasiuser, binputusernama, bmodifikasiusernama, bstokminimal, bstokmaksimal"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ItemTransaksiTerakhir(ByVal param As String) As String
        'M1_ItemTransaksiTerakhir --------------------------------------------------------
        'sumber, idtransaksi, notransaksi, tgl, kontak, kontakkode, kontaknama, 
        'iddetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, diskon, jmldiskon

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
        Dim idtransaksi As String = "", sumber As String = ""
        Dim idtrans(2) As String
        idtrans = paramSplit(5).Split(sptSubParam)
        If (idtrans.Length <> 2) Then
            result(2) = "Invalid primary key parameter." : GoTo selesai
        Else
            'CEK BID
            If (IsNumeric(idtrans(0)) = False) Then
                result(2) = "bid required numeric" : GoTo selesai
            Else
                idtransaksi = idtrans(0)
            End If
            'CEK SUMBER
            sumber = idtrans(1)
        End If
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

        'PANGGIL QUERY
        Dim query As New m0_query
        Select Case sumber
            Case "PO" : sql = query.PanggilQuery("m1_item_transaksi_terakhirPO")
            Case "RI" : sql = query.PanggilQuery("m1_item_transaksi_terakhirRI")
            Case "SO" : sql = query.PanggilQuery("m1_item_transaksi_terakhirSO")
            Case "SI" : sql = query.PanggilQuery("m1_item_transaksi_terakhirSI")
            Case Else : sql = query.PanggilQuery("m1_item_transaksi_terakhir")
        End Select
        sql = sql.Replace("valkode", idtransaksi)

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Contact_Category", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sumber"), ""), sptField,
                     FxDB(dr("idtransaksi"), 0), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                     FxDB(dr("kontak"), 0), sptField,
                     FxDB(dr("kontakkode"), ""), sptField,
                     FxDB(dr("kontaknama"), ""), sptField,
                     FxDB(dr("iddetail"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "Last Item Transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sumber, idtransaksi, notransaksi, tgl, kontak, kontakkode, kontaknama, iddetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistoryPoSearch(ByVal param As String) As String
        'M1_Item_HistoryPoSearch --------------------------------------------------------
        'posumber, poid, ponotransaksi, potgl, posupplier, posupplierkode, posuppliernama, 
        'idpodetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, diskon, jmldiskon

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
            Filter = "((`po`.`postatus` = 2) or (`po`.`postatus` = 3) or (`po`.`postatus` = 4) or (`po`.`postatus` = 7)) AND " & pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("posupplierkode", "c.kkode")
            Filter = Filter.Replace("posuppliernama", "c.knama")
        Else
            Filter = "((`po`.`postatus` = 2) or (`po`.`postatus` = 3) or (`po`.`postatus` = 4) or (`po`.`postatus` = 7)) "
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_history_po")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("posumber"), ""), sptField,
                     FxDB(dr("poid"), 0), sptField,
                     FxDB(dr("ponotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("potgl"), ""), formatTgl), sptField,
                     FxDB(dr("posupplier"), 0), sptField,
                     FxDB(dr("posupplierkode"), ""), sptField,
                     FxDB(dr("posuppliernama"), ""), sptField,
                     FxDB(dr("idpodetail"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "PO transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("posumber, poid, ponotransaksi, potgl, posupplier, posupplierkode, posuppliernama, idpodetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistoryRiSearch(ByVal param As String) As String
        'M1_Item_HistoryRiSearch --------------------------------------------------------
        'risumber, riid, rinotransaksi, ritgl, risupplier, risupplierkode, risuppliernama, 
        'idridetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, diskon, jmldiskon

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
            Filter = "((`ri`.`ristatus` = 2) or (`ri`.`ristatus` = 3) or (`ri`.`ristatus` = 4) or (`ri`.`ristatus` = 7)) AND " & pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("risupplierkode", "c.kkode")
            Filter = Filter.Replace("risuppliernama", "c.knama")
        Else
            Filter = "((`ri`.`ristatus` = 2) or (`ri`.`ristatus` = 3) or (`ri`.`ristatus` = 4) or (`ri`.`ristatus` = 7))"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_history_ri")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("risumber"), ""), sptField,
                     FxDB(dr("riid"), 0), sptField,
                     FxDB(dr("rinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("ritgl"), ""), formatTgl), sptField,
                     FxDB(dr("risupplier"), 0), sptField,
                     FxDB(dr("risupplierkode"), ""), sptField,
                     FxDB(dr("risuppliernama"), ""), sptField,
                     FxDB(dr("idridetail"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), ""), sptField,
                     FxDB(dr("jmldiskon"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "RI transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("risumber, riid, rinotransaksi, ritgl, risupplier, risupplierkode, risuppliernama, idridetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistorySoSearch(ByVal param As String) As String
        'M1_Item_HistorySoSearch --------------------------------------------------------
        'sosumber, soid, sonotransaksi, sotgl, socustomer, socustomerkode, socustomernama, 
        'idsodetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, diskon, jmldiskon

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
            Filter = "((`so`.`sostatus` = 2) or (`so`.`sostatus` = 3) or (`so`.`sostatus` = 4) or (`so`.`sostatus` = 7)) AND " & pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("socustomerkode", "c.kkode")
            Filter = Filter.Replace("socustomernama", "c.knama")
        Else
            Filter = "((`so`.`sostatus` = 2) or (`so`.`sostatus` = 3) or (`so`.`sostatus` = 4) or (`so`.`sostatus` = 7)) "
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_history_so")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sosumber"), ""), sptField,
                     FxDB(dr("soid"), 0), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sotgl"), ""), formatTgl), sptField,
                     FxDB(dr("socustomer"), 0), sptField,
                     FxDB(dr("socustomerkode"), ""), sptField,
                     FxDB(dr("socustomernama"), ""), sptField,
                     FxDB(dr("idsodetail"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), 0), sptField,
                     FxDB(dr("jmldiskon"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "SO transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sosumber, soid, sonotransaksi, sotgl, socustomer, socustomerkode, socustomernama, idsodetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistorySiSearch(ByVal param As String) As String
        'M1_Item_HistorySiSearch --------------------------------------------------------
        'sisumber, siid, sinotransaksi, sitgl, sicustomer, sicustomerkode, sicustomernama, 
        'idsidetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, 
        'matauang, kurs, harga, diskon, jmldiskon

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
            Filter = "((`si`.`sistatus` = 2) or (`si`.`sistatus` = 3) or (`si`.`sistatus` = 4) or (`si`.`sistatus` = 7)) AND " & pagingSplit(2)
            '#Taruh fungsi replace disini...
            Filter = Filter.Replace("sicustomerkode", "c.kkode")
            Filter = Filter.Replace("sicustomernama", "c.knama")
        Else
            Filter = "((`si`.`sistatus` = 2) or (`si`.`sistatus` = 3) or (`si`.`sistatus` = 4) or (`si`.`sistatus` = 7))"
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m1_item_history_si")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("sisumber"), ""), sptField,
                     FxDB(dr("siid"), 0), sptField,
                     FxDB(dr("sinotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("sitgl"), ""), formatTgl), sptField,
                     FxDB(dr("sicustomer"), 0), sptField,
                     FxDB(dr("sicustomerkode"), ""), sptField,
                     FxDB(dr("sicustomernama"), ""), sptField,
                     FxDB(dr("idsidetail"), 0), sptField,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("jml"), 0), sptField,
                     FxDB(dr("satuan"), ""), sptField,
                     FxDB(dr("nilaisatuan"), 0), sptField,
                     FxDB(dr("jmlbarang"), 0), sptField,
                     FxDB(dr("satuanbarang"), ""), sptField,
                     FxDB(dr("matauang"), ""), sptField,
                     FxDB(dr("kurs"), 0), sptField,
                     FxDB(dr("harga"), 0), sptField,
                     FxDB(dr("diskon"), 0), sptField,
                     FxDB(dr("jmldiskon"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
        Else
            result(2) = "SI transaction data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sisumber, siid, sinotransaksi, sitgl, sicustomer, sicustomerkode, sicustomernama, idsidetail, bid, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, harga, diskon, jmldiskon"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_Mutation_StokGetDataAll(ByVal param As String) As String
        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String
        'gudangAwal(4) As String, gudangAkhir(5) As String, orderBy(6) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'tglAwal, tglAkhir, barangAwal, barangAkhir, 
        'gudangAwal, gudangAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim dt As New DataTable
        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", orderBy As String = ""

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


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 7) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            tglAkhir = AsFormatTanggal(Now)
        End If

        'barangAwal(2) As String
        barangAwal = dataUtama(2)

        'barangAkhir(3) As String
        barangAkhir = dataUtama(3)

        'gudangAwal(4) As String
        gudangAwal = dataUtama(4)

        'gudangAkhir(5) As String
        gudangAkhir = dataUtama(5)

        'orderBy(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataUtama(6).ToString <> "bkode" And dataUtama(6).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataUtama(6)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ==================================================


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

        'TRANSAKSI KE DATABASE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'QUERY SALDO AWAL ###########################
        Dim dtSA As New DataTable
        Dim sqlSAGabung As String = "", saldoawal As Double = 0

        sql = "  SELECT IFNULL(it.id,0) as id, '" & FixQuotes(gudangAwal) & "' as gudang, w.wnama as gudangnama, it.idbarang, i.bkode as kodebarang, i.btipe as tipebarang, i.bnama as namabarang, '" & FixQuotes(tglAwal) & "' as tgl, '' as notransaksi, 0 as kontak,  '' as kontakkode,  '' as kontaknama, 'Saldo Awal' as uraian, 'Saldo Awal' as catatan, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlkeluar, '" & FixQuotes(tglAwal) & " 00:00:00' as inputtgl, it.inputuser as inputuser, u.unama as inputusernama, i.bhpp as tipehpp"
        sql &= " FROM m1_item i"
        sql &= " LEFT JOIN m1_warehouse w ON w.wkode = '" & FixQuotes(gudangAwal) & "'"
        sql &= " LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.gudang = w.wkode AND it.tgl < '" & FixQuotes(tglAwal) & "'"
        sql &= " LEFT JOIN m0_user u ON it.inputuser = u.userid"
        sql &= " WHERE i.bid = '" & FixDouble(barangAwal) & "'"
        If Len(pagingSplit(2)) > 0 Then
            sql &= " AND " & pagingSplit(2)
        End If

        sqlSAGabung = "  SELECT ms.id, ms.gudang, ms.gudangnama, ms.idbarang, ms.kodebarang, ms.tipebarang, ms.namabarang, ms.tgl, ms.notransaksi, ms.kontak,  ms.kontakkode,  ms.kontaknama,  ms.uraian, ms.catatan, 0 as jmlmasuk, 0 as jmlkeluar, SUM(ms.jmlmasuk - ms.jmlkeluar) as saldo, ms.inputtgl, ms.inputuser, ms.inputusernama, ms.tipehpp"
        sqlSAGabung &= " FROM ( " & sql & " ) as ms"
        sqlSAGabung &= " GROUP BY ms.gudang, ms.idbarang"


        'dt = AmbilData("aplikasi1-M1_Item", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sqlSAGabung) ' Ambil data ke databases
        'pg1 = pg1

        dt = AsDataTableAmbilDariDB(sqlSAGabung)
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                If pagingSplit(0) = 1 Then
                    search = String.Concat(search,
                            FxDB(dr("id"), 0), sptField,
                            FxDB(dr("gudang"), ""), sptField,
                            FxDB(dr("gudangnama"), ""), sptField,
                            FxDB(dr("idbarang"), 0), sptField,
                            FxDB(dr("kodebarang"), ""), sptField,
                            FxDB(dr("tipebarang"), ""), sptField,
                            FxDB(dr("namabarang"), ""), sptField,
                            AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                            FxDB(dr("notransaksi"), ""), sptField,
                            FxDB(dr("kontak"), 0), sptField,
                            FxDB(dr("kontakkode"), ""), sptField,
                            FxDB(dr("kontaknama"), ""), sptField,
                            FxDB(dr("uraian"), ""), sptField,
                            FxDB(dr("catatan"), ""), sptField,
                            FxDB(dr("jmlmasuk"), 0), sptField,
                            FxDB(dr("jmlkeluar"), 0), sptField,
                            FxDB(dr("saldo"), 0), sptField,
                            AsFormatTanggal(FxDB(dr("inputtgl"), ""), formatTglWaktu), sptField,
                            FxDB(dr("inputuser"), 0), sptField,
                            FxDB(dr("inputusernama"), ""), sptField,
                            FxDB(dr("tipehpp"), ""), sptRow)
                End If

                saldoawal = Double.Parse(dr("saldo"))

            Next
            'search = search.Substring(0, search.Length - sptRow.Length)


            'QUERY SALDO MUTASI #########################
            Dim sqlSM As String = "", sqlSMGabung As String = ""

            sqlSM = "  SELECT it.id as id, it.gudang as gudang, w.wnama as gudangnama, it.idbarang as idbarang, i.bkode as kodebarang, i.btipe as tipebarang, i.bnama as namabarang, it.tgl as tgl, it.notransaksi as notransaksi, it.kontak as kontak, c.kkode as kontakkode, c.knama as kontaknama,  it.uraian as uraian, it.catatan as catatan, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as jmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as jmlkeluar, it.inputtgl as inputtgl, it.inputuser as inputuser, u.unama as inputusernama, it.tipehpp as tipehpp"
            sqlSM &= " FROM m1_item_transaction it"
            sqlSM &= " JOIN m1_item i ON it.idbarang = i.bid"
            sqlSM &= " JOIN m1_warehouse w ON it.gudang = w.wkode"
            sqlSM &= " LEFT JOIN m1_contact c ON it.kontak = c.kid"
            sqlSM &= " LEFT JOIN m0_user u ON it.inputuser = u.userid"
            sqlSM &= " WHERE it.gudang = '" & FixQuotes(gudangAwal) & "'"
            sqlSM &= " AND it.idbarang = '" & FixDouble(barangAwal) & "'"
            sqlSM &= " AND it.tgl BETWEEN '" & FixDouble(tglAwal) & "' AND '" & FixDouble(tglAkhir) & "'"
            If Len(pagingSplit(2)) > 0 Then
                sqlSM &= " AND " & pagingSplit(2)
            End If
            sqlSM &= " ORDER BY it.tgl, it.inputtgl, it.id"

            sqlSMGabung = "  SELECT ms.id, ms.gudang, ms.gudangnama, ms.idbarang, ms.kodebarang, ms.tipebarang, ms.namabarang, ms.tgl, ms.notransaksi, ms.kontak,  ms.kontakkode,  ms.kontaknama,  ms.uraian, ms.catatan, ms.jmlmasuk, ms.jmlkeluar, @saldo := @saldo + ms.jmlmasuk - ms.jmlkeluar as saldo, ms.inputtgl, ms.inputuser, ms.inputusernama, ms.tipehpp"
            sqlSMGabung &= " FROM ( " & sqlSM & " ) as ms"
            sqlSMGabung &= " , (SELECT @saldo := " & FixDouble(saldoawal) & ") AS variableInit2"

            dt = AmbilData("aplikasi1-M1_Item", "", "", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sqlSMGabung) ' Ambil data ke databases
            pg1 = pg1

            If dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    search = String.Concat(search,
                                 FxDB(dr("id"), 0), sptField,
                                 FxDB(dr("gudang"), ""), sptField,
                                 FxDB(dr("gudangnama"), ""), sptField,
                                 FxDB(dr("idbarang"), 0), sptField,
                                 FxDB(dr("kodebarang"), ""), sptField,
                                 FxDB(dr("tipebarang"), ""), sptField,
                                 FxDB(dr("namabarang"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                                 FxDB(dr("notransaksi"), ""), sptField,
                                 FxDB(dr("kontak"), 0), sptField,
                                 FxDB(dr("kontakkode"), ""), sptField,
                                 FxDB(dr("kontaknama"), ""), sptField,
                                 FxDB(dr("uraian"), ""), sptField,
                                 FxDB(dr("catatan"), ""), sptField,
                                 FxDB(dr("jmlmasuk"), 0), sptField,
                                 FxDB(dr("jmlkeluar"), 0), sptField,
                                 FxDB(dr("saldo"), 0), sptField,
                                 FxDB(dr("inputtgl"), formatTglWaktu), sptField,
                                 FxDB(dr("inputuser"), 0), sptField,
                                 FxDB(dr("inputusernama"), ""), sptField,
                                 FxDB(dr("tipehpp"), ""), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)
            End If

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow

        Else
            result(2) = "Item data not found. #1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("id, gudang, gudangnama, idbarang, kodebarang, tipebarang, namabarang, tgl, notransaksi, kontak, kontakkode, kontaknama, uraian, catatan, jmlmasuk, jmlkeluar, saldo, inputtgl, inputuser, inputusernama, tipehpp"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_InformationSearch(ByVal param As String) As String
        'M1_Item_InformationSearch --------------------------------------------------------
        'bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan
        'bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2,
        'bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4,
        'bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan,
        'brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan,
        'bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving,
        'binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, baktif, baktiftgl,
        'bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, 
        'bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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
        'sql = query.PanggilQuery("m1_item_information_v")
        sql = "select `i`.`bid` AS `bid`,`i`.`bkode` AS `bkode`,`i`.`bnama` AS `bnama`,`i`.`btipe` AS `btipe`,`i`.`bjenis` AS `bjenis`,`i`.`bkategori` AS `bkategori`,`i`.`bsatuan` AS `bsatuan`,`i`.`bsatuandefault` AS `bsatuandefault`,`i`.`bhpp` AS `bhpp`,`i`.`bbarcode` AS `bbarcode`,`i`.`bhargabeli` AS `bhargabeli`,`i`.`bhppaverage` AS `bhppaverage`,`i`.`bhargajual1` AS `bhargajual1`,`i`.`bhargajual2` AS `bhargajual2`,`i`.`bhargajual3` AS `bhargajual3`,`i`.`bhargajual4` AS `bhargajual4`,`i`.`bhargajual5` AS `bhargajual5`,`i`.`bdiskonjual1` AS `bdiskonjual1`,`i`.`bdiskonjual2` AS `bdiskonjual2`,`i`.`bdiskonjual3` AS `bdiskonjual3`,`i`.`bdiskonjual4` AS `bdiskonjual4`,`i`.`bdiskonjual5` AS `bdiskonjual5`,(CASE i.bjenis WHEN 'J' THEN 0 ELSE `i`.`bstok` END) AS `bstok`,(CASE i.bjenis WHEN 'J' THEN 0 ELSE ifnull(sum(`ib`.`jmlbooking`),0) END) AS `bstokbooking`,`i`.`bmarginminimal` AS `bmarginminimal`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekpenjualan` AS `brekpenjualan`,`i`.`brekreturpenjualan` AS `brekreturpenjualan`,`i`.`brekdiskonpenjualan` AS `brekdiskonpenjualan`,`i`.`brekhargapokok` AS `brekhargapokok`,`i`.`brekreturpembelian` AS `brekreturpembelian`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`brekkonsinyasi` AS `brekkonsinyasi`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`bnilaisatuan` AS `bnilaisatuan`,`i`.`bnilaisatuandefault` AS `bnilaisatuandefault`,`i`.`bsuplier` AS `bsuplier`,`c`.`kkode` AS `bsuplierkode`,`c`.`knama` AS `bsupliernama`,`i`.`bstokminimal` AS `bstokminimal`,`i`.`bstokmaksimal` AS `bstokmaksimal`,`i`.`bstatusmoving` AS `bstatusmoving`,`i`.`binputuser` AS `binputuser`,`i`.`binputtgl` AS `binputtgl`,`i`.`bmodifikasiuser` AS `bmodifikasiuser`,`i`.`bmodifikasitgl` AS `bmodifikasitgl`,`f`.`fnamafile` AS `fnamafile`, i.baktif, i.baktiftgl, i.bhargajual6, i.bhargajual7, i.bhargajual8, i.bhargajual9, i.bhargajual10, i.bdiskonjual6, i.bdiskonjual7, i.bdiskonjual8, i.bdiskonjual9, i.bdiskonjual10 from (((`m1_item` `i` left join `m1_item_booking` `ib` on((`i`.`bid` = `ib`.`idbarang`))) left join `m1_contact` `c` on((`i`.`bsuplier` = `c`.`kid`))) left join `m1_files` `f` on(((`i`.`bid` = `f`.`fidtransaksi`) and (`f`.`fdefault` = 1) and (`f`.`fsumber` = 'Item'))))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "i.bid", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("bid"), 0), sptField,
                    FxDB(dr("bkode"), ""), sptField,
                    FxDB(dr("bnama"), ""), sptField,
                    FxDB(dr("btipe"), ""), sptField,
                    FxDB(dr("bjenis"), ""), sptField,
                    FxDB(dr("bkategori"), ""), sptField,
                    FxDB(dr("bsatuan"), ""), sptField,
                    FxDB(dr("bsatuandefault"), ""), sptField,
                    FxDB(dr("bhpp"), ""), sptField,
                    FxDB(dr("bbarcode"), ""), sptField,
                    FxDB(dr("bhargabeli"), 0), sptField,
                    FxDB(dr("bhppaverage"), 0), sptField,
                    FxDB(dr("bhargajual1"), 0), sptField,
                    FxDB(dr("bhargajual2"), 0), sptField,
                    FxDB(dr("bhargajual3"), 0), sptField,
                    FxDB(dr("bhargajual4"), 0), sptField,
                    FxDB(dr("bhargajual5"), 0), sptField,
                    FxDB(dr("bdiskonjual1"), 0), sptField,
                    FxDB(dr("bdiskonjual2"), 0), sptField,
                    FxDB(dr("bdiskonjual3"), 0), sptField,
                    FxDB(dr("bdiskonjual4"), 0), sptField,
                    FxDB(dr("bdiskonjual5"), 0), sptField,
                    FxDB(dr("bstok"), 0), sptField,
                    FxDB(dr("bstokbooking"), 0), sptField,
                    FxDB(dr("bmarginminimal"), 0), sptField,
                    FxDB(dr("brekpersediaan"), ""), sptField,
                    FxDB(dr("brekreturpenjualan"), ""), sptField,
                    FxDB(dr("brekdiskonpenjualan"), ""), sptField,
                    FxDB(dr("brekhargapokok"), ""), sptField,
                    FxDB(dr("brekreturpembelian"), ""), sptField,
                    FxDB(dr("brekdiskonpembelian"), ""), sptField,
                    FxDB(dr("brekkonsinyasi"), ""), sptField,
                    FxDB(dr("bserial"), ""), sptField,
                    FxDB(dr("bbatch"), ""), sptField,
                    FxDB(dr("bnilaisatuan"), 0), sptField,
                    FxDB(dr("bnilaisatuandefault"), 0), sptField,
                    FxDB(dr("bsuplier"), 0), sptField,
                    FxDB(dr("bsuplierkode"), ""), sptField,
                    FxDB(dr("bsupliernama"), ""), sptField,
                    FxDB(dr("bstokminimal"), 0), sptField,
                    FxDB(dr("bstokmaksimal"), 0), sptField,
                    FxDB(dr("bstatusmoving"), ""), sptField,
                    FxDB(dr("binputuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("binputtgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("bmodifikasiuser"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("bmodifikasitgl"), ""), formatTglWaktu), sptField,
                    FxDB(dr("fnamafile"), ""), sptField,
                    FxDB(dr("baktif"), 0), sptField,
                    AsFormatTanggal(FxDB(dr("baktiftgl"), ""), formatTgl), sptField,
                     FxDB(dr("bhargajual6"), 0), sptField,
                     FxDB(dr("bhargajual7"), 0), sptField,
                     FxDB(dr("bhargajual8"), 0), sptField,
                     FxDB(dr("bhargajual9"), 0), sptField,
                     FxDB(dr("bhargajual10"), 0), sptField,
                     FxDB(dr("bdiskonjual6"), 0), sptField,
                     FxDB(dr("bdiskonjual7"), 0), sptField,
                     FxDB(dr("bdiskonjual8"), 0), sptField,
                     FxDB(dr("bdiskonjual9"), 0), sptField,
                     FxDB(dr("bdiskonjual10"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, bnama, btipe, bjenis, bkategori, bsatuan, bsatuandefault, bhpp, bbarcode, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bstokbooking, bmarginminimal, brekpersediaan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bserial, bbatch, bnilaisatuan, bnilaisatuandefault, bsuplier, bsuplierkode, bsupliernama, bstokminimal, bstokmaksimal, bstatusmoving, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, fnamafile, baktif, baktiftgl, bhargajual6, bhargajual7, bhargajual8, bhargajual9, bhargajual10, bdiskonjual6, bdiskonjual7, bdiskonjual8, bdiskonjual9, bdiskonjual10"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_SpecialInSearch(ByVal param As String) As String
        'M1_ItemSpecialInSearch --------------------------------------------------------
        'idbarang, kodebarang, tipebarang, namabarang, jmlmasuk, jmlkeluar, harga
        'notransaksi, tgl, kontak, kontakkode, kontaknama, tipehpp

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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
        sql = query.PanggilQuery("m1_item_special_in_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("idbarang"), 0), sptField,
                    FxDB(dr("kodebarang"), ""), sptField,
                    FxDB(dr("tipebarang"), ""), sptField,
                    FxDB(dr("namabarang"), ""), sptField,
                    FxDB(dr("jmlmasuk"), ""), sptField,
                    FxDB(dr("jmlkeluar"), ""), sptField,
                    FxDB(dr("harga"), ""), sptField,
                    FxDB(dr("notransaksi"), ""), sptField,
                    AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                    FxDB(dr("kontak"), ""), sptField,
                    FxDB(dr("kontakkode"), 0), sptField,
                    FxDB(dr("kontaknama"), ""), sptField,
                    FxDB(dr("tipehpp"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, kodebarang, tipebarang, namabarang, jmlmasuk, jmlkeluar, harga, notransaksi, tgl, kontak, kontakkode, kontaknama, tipehpp"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_StockMutation(ByVal param As String) As String
        'M1_Item_StockMutation -----------------------------------------------------------
        'id, gudang, gudangnama, idbarang, kodebarang, tipebarang, namabarang, 
        'satuan, tgl, sumber, idutama, notransaksi, kontak, kontakkode, kontaknama, 
        'uraian, catatan, catatandetail, jmlmasuk, jmlkeluar, saldo, inputtgl, 
        'inputuser, inputusernama, saldoawal, saldomasuk, saldokeluar, saldoakhir

        'MAPPING BUAT WS ----------------------------------------------------------
        'Utama
        'tglAwal(0) As String, tglAkhir(1) As String, barangAwal(2) As String, barangAkhir(3) As String
        'gudangAwal(4) As String, gudangAkhir(5) As String, orderBy(6) As String

        'MAPPING BUAT FLEX --------------------------------------------------------
        'Utama
        'tglAwal, tglAkhir, barangAwal, barangAkhir, 
        'gudangAwal, gudangAkhir, orderBy

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String

        Dim sql As String = "", notransaksi As String = "", formatTgl As String = "", formatTglWaktu As String = ""

        Dim dt As New DataTable
        Dim pg1 As New RsPaging
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = "", GroupBy As String = "", stepKe As Double = 0, Prosentase As Double = 100
        Dim strValue As New StringBuilder
        Dim progressPersen As Double = 0

        'VARIABLE FUNGSI
        Dim tglAwal As String = "", tglAkhir As String = "", barangAwal As String = "", barangAkhir As String = ""
        Dim gudangAwal As String = "", gudangAkhir As String = "", orderBy As String = ""

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


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = paramSplit(5).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 7) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA =========================================================
        'tglAwal(0) As String
        If Len(dataUtama(0)) > 0 Then
            If (IsDate(dataUtama(0)) = False) Then
                result(2) = "tglAwal required date." : GoTo selesai
            Else
                tglAwal = AsFormatTanggal(dataUtama(0))
            End If
        Else
            tglAwal = AsFormatTanggal("1900-01-01")
        End If

        'tglAkhir(1) As String
        If Len(dataUtama(1)) > 0 Then
            If (IsDate(dataUtama(1)) = False) Then
                result(2) = "tglAkhir required date." : GoTo selesai
            Else
                tglAkhir = AsFormatTanggal(dataUtama(1))
            End If
        Else
            tglAkhir = AsFormatTanggal(Now)
        End If

        'barangAwal(2) As String
        barangAwal = dataUtama(2)

        'barangAkhir(3) As String
        barangAkhir = dataUtama(3)

        'gudangAwal(4) As String
        gudangAwal = dataUtama(4)

        'gudangAkhir(5) As String
        gudangAkhir = dataUtama(5)

        'orderBy(6) As String
        If Len(dataUtama(6)) = 0 Then
            result(2) = "orderBy can't be empty." : GoTo selesai
        ElseIf dataUtama(6).ToString <> "bkode" And dataUtama(6).ToString <> "bnama" Then
            result(2) = "Invalid orderBy criteria." : GoTo selesai
        Else
            orderBy = dataUtama(6)
        End If
        'END OF VALIDASI TIPE DATA UTAMA ==================================================


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

        'TRANSAKSI KE DATABASE
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        Dim dtSA As New DataTable, saldoawal As Double = 0
        Dim sqlSA As String = "", sqlSM As String = "", sqlSAGabung As String = "", sqlSMGabung As String = ""
        Dim sqlSAJadi As String = "", sqlSMJadi As String = ""


        'QUERY SALDO AWAL ###########################
        '                                      id,                                      gudang,              gudangnama,            idbarang,              kodebarang,              tipebarang,              namabarang,                satuan,                                         tgl,         sumber,        idutama,         notransaksi,        kontak,          kontakkode,          kontaknama,                   uraian,                   catatan,                   catatandetail,                                                                jmlmasuk,                                                                jmlkeluar,                                            inputtgl,        inputuser,         inputusernama
        sqlSA = "  SELECT IFNULL(it.id,0) as msid, '" & FixQuotes(gudangAwal) & "' as msgudang, w.wnama as msgudangnama, i.bid as msidbarang, i.bkode as mskodebarang, i.btipe as mstipebarang, i.bnama as msnamabarang, i.bsatuan as mssatuanbarang, '" & FixQuotes(tglAwal) & "' as mstgl, '' as mssumber, 0 as msidutama, '' as msnotransaksi, 0 as mskontak,  '' as mskontakkode,  '' as mskontaknama, 'Saldo Awal' as msuraian, 'Saldo Awal' as mscatatan, 'Saldo Awal' as mscatatandetail, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as msjmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as msjmlkeluar, '" & FixQuotes(tglAwal) & " 00:00:00' as msinputtgl, 0 as msinputuser, '' as msinputusernama"
        sqlSA &= " FROM m1_item i"
        sqlSA &= " LEFT JOIN m1_warehouse w ON w.wkode = '" & FixQuotes(gudangAwal) & "'"
        sqlSA &= " LEFT JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.gudang = w.wkode AND it.tgl < '" & FixQuotes(tglAwal) & "'"
        sqlSA &= " WHERE i.bid = '" & FixDouble(barangAwal) & "'"

        '                            id,      gudang,      gudangnama,      idbarang,      kodebarang,      tipebarang,      namabarang,      satuan,            tgl,      sumber,      idutama,      notransaksi,      kontak,       kontakkode,       kontaknama,       uraian,      catatan,      catatandetail,        jmlmasuk,        jmlkeluar,                                          saldo,      inputtgl,      inputuser,      inputusernama
        sqlSAGabung = "  SELECT ms.msid, ms.msgudang, ms.msgudangnama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msidutama, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, 0 as msjmlmasuk, 0 as msjmlkeluar, SUM(ms.msjmlmasuk - ms.msjmlkeluar) as mssaldo, ms.msinputtgl, ms.msinputuser, ms.msinputusernama"
        sqlSAGabung &= " FROM ( " & sqlSA & " ) as ms"
        sqlSAGabung &= " GROUP BY ms.msgudang, ms.msidbarang"

        'AMBIL SALDO AWAL
        dtSA = AsDataTableAmbilDariDB(sqlSAGabung)
        If dtSA.Rows.Count > 0 Then
            saldoawal = Double.Parse(dtSA.Rows(0)("mssaldo"))
        Else
            saldoawal = 0
        End If

        'QUERY SALDO MUTASI #########################
        '                            id,                gudang,              gudangnama,                  idbarang,              kodebarang,                    tipebarang,                    namabarang,                      satuan,                   tgl,                sumber,                 idutama,                     notransaksi,                kontak,               kontakkode,              kontaknama,                 uraian,                 catatan,                       catatandetail,                                                                jmlmasuk,                                                                jmlkeluar,                  inputtgl,                   inputuser,              inputusernama
        sqlSM = "  SELECT it.id as msid, it.gudang as msgudang, w.wnama as msgudangnama, it.idbarang as msidbarang, i.bkode as mskodebarang, it.tipebarang as mstipebarang, it.namabarang as msnamabarang, it.satuanbarang as mssatuanbarang, it.tgl as mstgl, it.sumber as mssumber, it.idutama as msidutama, it.notransaksi as msnotransaksi, it.kontak as mskontak,  c.kkode as mskontakkode, c.knama as mskontaknama,  it.uraian as msuraian, it.catatan as mscatatan, it.catatandetail as mscatatandetail, (CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE 0 END) as msjmlmasuk, (CASE it.jenismutasi WHEN 0 THEN it.jmlbarang ELSE 0 END) as msjmlkeluar, it.inputtgl as msinputtgl, it.inputuser as msinputuser, u.unama as msinputusernama"
        sqlSM &= " FROM m1_item_transaction it"
        sqlSM &= " JOIN m1_item i ON it.idbarang = i.bid"
        sqlSM &= " JOIN m1_warehouse w ON it.gudang = w.wkode"
        sqlSM &= " LEFT JOIN m1_contact c ON it.kontak = c.kid"
        sqlSM &= " LEFT JOIN m0_user u ON it.inputuser = u.userid"
        sqlSM &= " WHERE it.gudang = '" & FixQuotes(gudangAwal) & "'"
        sqlSM &= " AND it.idbarang = '" & FixDouble(barangAwal) & "'"
        sqlSM &= " AND it.tgl BETWEEN '" & FixDouble(tglAwal) & "' AND '" & FixDouble(tglAkhir) & "'"
        sqlSM &= " ORDER BY it.tgl, it.inputtgl, it.id"

        '                            id,      gudang,      gudangnama,      idbarang,      kodebarang,      tipebarang,      namabarang,      satuan,            tgl,      sumber,      idutama,      notransaksi,      kontak,       kontakkode,       kontaknama,       uraian,      catatan,      catatandetail,      jmlmasuk,      jmlkeluar,                                                       saldo,      inputtgl,       inputuser,       inputusernama
        sqlSMGabung = "  SELECT ms.msid, ms.msgudang, ms.msgudangnama, ms.msidbarang, ms.mskodebarang, ms.mstipebarang, ms.msnamabarang, ms.mssatuanbarang, ms.mstgl, ms.mssumber, ms.msidutama, ms.msnotransaksi, ms.mskontak,  ms.mskontakkode,  ms.mskontaknama,  ms.msuraian, ms.mscatatan, ms.mscatatandetail, ms.msjmlmasuk, ms.msjmlkeluar, @saldo := @saldo + ms.msjmlmasuk - ms.msjmlkeluar as mssaldo, ms.msinputtgl, ms.msinputuser,  ms.msinputusernama"
        sqlSMGabung &= " FROM ( " & sqlSM & " ) as ms"
        sqlSMGabung &= " , (SELECT @saldo := " & FixDouble(saldoawal) & ") AS variableInit2"


        'AMBIL DATA SALDO AWAL DAN SALDO MUTASI ######
        sqlSMJadi = "(" & sqlSAGabung & ") UNION (" & sqlSMGabung & ")"
        dt = AsDataTableAmbilDariDB(sqlSMJadi)
        If dt.Rows.Count > 0 Then

            'AMBIL SALDO MASUK, SALDO KELUAR, SALDO AKHIR
            Dim saldomasuk As Double = 0, saldokeluar As Double = 0, saldoakhir As Double = 0
            saldomasuk = AsDataTableDSum(dt, "msjmlmasuk")
            saldokeluar = AsDataTableDSum(dt, "msjmlkeluar")
            saldoakhir = Double.Parse(dt.Rows(dt.Rows.Count - 1)("mssaldo"))

            'SET PAGING
            If pagingSplit(0) > 0 Or pagingSplit(0) = -1 Then pg1.isPaging = True Else pg1.isPaging = False
            Dim rowStart As Integer = 0, dtJadi As New DataTable

            If pg1.isPaging Then
                'LIMIT LAST PAGE
                If pagingSplit(0) = -1 Then
                    'HITUNG PAGE NUMBER = jmldata/itemlimit
                    pagingSplit(0) = Math.Ceiling((dt.Rows.Count) / pagingSplit(1))
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)

                    'LIMIT SESUAI PAGENUMBER
                ElseIf pagingSplit(0) > 0 Then
                    rowStart = (pagingSplit(0) - 1) * pagingSplit(1)
                End If
                dtJadi = AsDataTableFilterLimit(dt, "", "", rowStart, pagingSplit(1))

            Else
                dtJadi = dt 'AsDataTableFilterLimit(dt, "", "")
            End If


            If dtJadi.Rows.Count > 0 Then
                For Each dr As DataRow In dtJadi.Rows
                    search = String.Concat(search,
                                 FxDB(dr("msid"), 0), sptField,
                                 FxDB(dr("msgudang"), ""), sptField,
                                 FxDB(dr("msgudangnama"), ""), sptField,
                                 FxDB(dr("msidbarang"), 0), sptField,
                                 FxDB(dr("mskodebarang"), ""), sptField,
                                 FxDB(dr("mstipebarang"), ""), sptField,
                                 FxDB(dr("msnamabarang"), ""), sptField,
                                 FxDB(dr("mssatuanbarang"), ""), sptField,
                                 AsFormatTanggal(FxDB(dr("mstgl"), ""), formatTgl), sptField,
                                 FxDB(dr("mssumber"), ""), sptField,
                                 FxDB(dr("msidutama"), 0), sptField,
                                 FxDB(dr("msnotransaksi"), ""), sptField,
                                 FxDB(dr("mskontak"), 0), sptField,
                                 FxDB(dr("mskontakkode"), ""), sptField,
                                 FxDB(dr("mskontaknama"), ""), sptField,
                                 FxDB(dr("msuraian"), ""), sptField,
                                 FxDB(dr("mscatatan"), ""), sptField,
                                 FxDB(dr("mscatatandetail"), ""), sptField,
                                 FxDB(dr("msjmlmasuk"), 0), sptField,
                                 FxDB(dr("msjmlkeluar"), 0), sptField,
                                 FxDB(dr("mssaldo"), 0), sptField,
                                 AsFormatTanggal(FxDB(dr("msinputtgl"), ""), formatTglWaktu), sptField,
                                 FxDB(dr("msinputuser"), 0), sptField,
                                 FxDB(dr("msinputusernama"), ""), sptField,
                                 FxDB(saldoawal, 0), sptField,
                                 FxDB(saldomasuk, 0), sptField,
                                 FxDB(saldokeluar, 0), sptField,
                                 FxDB(saldoakhir, 0), sptRow)
                Next
                search = search.Substring(0, search.Length - sptRow.Length)

                result(1) = 1

                If pg1.isPaging Then
                    pg1.isPrev = pagingSplit(0) > 1
                    pg1.isNext = dt.Rows.Count > pagingSplit(0) * pagingSplit(1)

                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(pg1.isNext))
                    resultPaging(2) = Math.Abs(Val(pg1.isPrev))
                    resultPaging(3) = pagingSplit(0)
                    resultPaging(4) = pg1.countRow
                Else
                    resultPaging(0) = Math.Abs(Val(pg1.isPaging))
                    resultPaging(1) = Math.Abs(Val(False))
                    resultPaging(2) = Math.Abs(Val(False))
                    resultPaging(3) = 0
                    resultPaging(4) = 0
                End If


            Else
                result(2) = "Item Stock Mutation data not found. #2" : GoTo selesai
            End If

        Else
            result(2) = "Item Stock Mutation data not found. #1" : GoTo selesai
        End If


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("id, gudang, gudangnama, idbarang, kodebarang, tipebarang, namabarang, satuan, tgl, sumber, idutama, notransaksi, kontak, kontakkode, kontaknama, uraian, catatan, catatandetail, jmlmasuk, jmlkeluar, saldo, inputtgl, inputuser, inputusernama, saldoawal, saldomasuk, saldokeluar, saldoakhir"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_Item_HistoryTransactionSearch(ByVal param As String) As String
        'M1_ItemSpecialInSearch --------------------------------------------------------
        'idbarang, tgl, notransaksi, kontak, kontakkode, kontaknama, jmlbarang, 
        'harga, matauang, diskon, jenismutasi, sumber, idutama

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
            formatTglWaktu = "yyy-MM-dd Hh:mm:ss"
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
        sql = query.PanggilQuery("m1_item_history_transaction_v")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M1_Item", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                    FxDB(dr("idbarang"), 0), sptField,
                    AsFormatTanggal(FxDB(dr("tgl"), ""), formatTgl), sptField,
                    FxDB(dr("notransaksi"), ""), sptField,
                    FxDB(dr("kontak"), ""), sptField,
                    FxDB(dr("kontakkode"), ""), sptField,
                    FxDB(dr("kontaknama"), ""), sptField,
                    FxDB(dr("jmlbarang"), 0), sptField,
                    FxDB(dr("harga"), 0), sptField,
                    FxDB(dr("matauang"), ""), sptField,
                    FxDB(dr("diskon"), 0), sptField,
                    FxDB(dr("jenismutasi"), 0), sptField,
                    FxDB(dr("sumber"), ""), sptField,
                    FxDB(dr("idutama"), 0), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idbarang, tgl, notransaksi, kontak, kontakkode, kontaknama, jmlbarang, harga, matauang, diskon, jenismutasi, sumber, idutama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M1_ItemImport(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        'Dim dataSplit(), dataUtama(), dataILW(), dataRowILW(), dataIA(), dataRowIA(), dataIS(), dataRowIS() As String
        Dim dataSplit(), dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging, strResultData As String

        Dim sql As String = "" : Dim kode As String = "" : Dim formatTgl As String = "", formatTglWaktu As String = "" : Dim isUpdate As Boolean
        Dim search As String = ""
        Dim Filter As String = "", Sorting As String = ""

        Dim pg1 As New RsPaging

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

        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 4) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

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
        'bhargajual2(45) As Double, bhargajual3(46) As Double, bhargajual4(47) As Double, bhargajual5(48) As Double, bdiskonjual1(49) As Double, 
        'bdiskonjual2(50) As Double, bdiskonjual3(51) As Double, bdiskonjual4(52) As Double, bdiskonjual5(53) As Double, bstok(54) As Double, 
        'bkomisi(55) As Double, bmarginminimal(56) As Double, brekpersediaan(57) As String, brekpenjualan(58) As String, brekreturpenjualan(59) As String, 
        'brekdiskonpenjualan(60) As String, brekhargapokok(61) As String, brekreturpembelian(62) As String, brekdiskonpembelian(63) As String, brekkonsinyasi(64) As String, 
        'bapanjang(65) As Double, balebar(66) As Double, batinggi(67) As Double, bavolume(68) As Double, baberat(69) As Double, 
        'bawarna(70) As String, baoem(71) As String, bamerk(72) As String, baukuran(73) As String, bamodel(74) As String, 
        'bakelas(75) As String, bserial(76) As Integer, bbatch(77) As Integer, bpengganti(78) As Integer, bgambar(79) As String, 
        'burutan(80) As Integer, bcustom1(81) As String, bcustom2(82) As String, bcustom3(83) As String, bcustom4(84) As String, 
        'bcustom5(85) As String, bcustom6(86) As String, bcustom7(87) As String, bcustom8(88) As String, bcustom9(89) As String, 
        'bcustom10(90) As String, bcustom11(91) As Integer, bcustom12(92) As Integer, bcustom13(93) As Integer, bcustom14(94) As Double, 
        'bcustom15(95) As Double, bcatatan(96) As String, binputuser(97) As Integer, binputtgl(98) As DateTime, bmodifikasiuser(99) As Integer, 
        'bmodifikasitgl(100) As DateTime, bedithpp(101) As Integer, bmobile(102) As Integer, bassembly(103) As Integer,
        'bkelasproduk(104) As String, bretur(105) As Integer, btag(106) As String, bminorder(107) As Double, 
        'bdepartemen (108) As String, bsubdepartemen (109) As String, bkp(110) As Integer, bkl(111) As Integer, bjmllapangan(112) As Double, bsatuanlapangan(113) As Double,
        'bsubkelas(114) As String, bmaterial(115) As String, bsection(116) As String, bvendor(117) As String, bdesigner(118) As String

        'MAPPING BUAT FLEX ----------------------------------------------------------
        'bid, bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, 
        'bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, 
        'bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, 
        'bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, 
        'baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, 
        'bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, 
        'bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, 
        'bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, 
        'bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, 
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, 
        'bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, 
        'bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, 
        'bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, 
        'bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, 
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile, bassembly,
        'bkelasproduk, bretur, btag, bminorder, bdepartemen, bsubdepartemen, bkp, bkl, bjmllapangan, bsatuanlapangan,
        'bsubkelas, bmaterial, bsection, bvendor, bdesigner

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 119) Then
            result(2) = "Invalid item data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
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
        ''bdiskonjual1(49) As Double
        'If (IsNumeric(dataUtama(49)) = False) Then
        '    result(2) = "bdiskonjual1 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual2(50) As Double
        'If (IsNumeric(dataUtama(50)) = False) Then
        '    result(2) = "bdiskonjual2 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual3(51) As Double
        'If (IsNumeric(dataUtama(51)) = False) Then
        '    result(2) = "bdiskonjual3 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual4(52) As Double
        'If (IsNumeric(dataUtama(52)) = False) Then
        '    result(2) = "bdiskonjual4 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual5(53) As Double
        'If (IsNumeric(dataUtama(53)) = False) Then
        '    result(2) = "bdiskonjual5 required numeric." : GoTo selesai
        'End If
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
        'bapanjang(65) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "bapanjang required numeric." : GoTo selesai
        End If
        'balebar(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "balebar required numeric." : GoTo selesai
        End If
        'batinggi(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "batinggi required numeric." : GoTo selesai
        End If
        'bavolume(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "bavolume required numeric." : GoTo selesai
        End If
        'baberat(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "baberat required numeric." : GoTo selesai
        End If
        'bserial(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "bserial required numeric." : GoTo selesai
        End If
        'bbatch(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "bbatch required numeric." : GoTo selesai
        End If
        'bpengganti(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "bpengganti required numeric." : GoTo selesai
        End If
        'burutan(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "burutan required numeric." : GoTo selesai
        End If
        'bcustom11(91) As Integer
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "bcustom11 required numeric." : GoTo selesai
        End If
        'bcustom12(92) As Integer
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "bcustom12 required numeric." : GoTo selesai
        End If
        'bcustom13(93) As Integer
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "bcustom13 required numeric." : GoTo selesai
        End If
        'bcustom14(94) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "bcustom14 required numeric." : GoTo selesai
        End If
        'bcustom15(95) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "bcustom15 required numeric." : GoTo selesai
        End If
        'binputuser(97) As Integer
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "binputuser required numeric." : GoTo selesai
        End If
        'binputtgl(98) As DateTime
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "binputtgl required date." : GoTo selesai
        End If
        'bmodifikasiuser(99) As Integer
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "bmodifikasiuser required numeric." : GoTo selesai
        End If
        'bmodifikasitgl(100) As DateTime
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "bmodifikasitgl required date." : GoTo selesai
        End If
        'bedithpp(101) As Integer
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "bedithpp required numeric." : GoTo selesai
        End If
        'bmobile(102) As Integer
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "bmobile required numeric." : GoTo selesai
        End If
        'bassembly(103) As Integer
        If (IsNumeric(dataUtama(103)) = False) Then
            result(2) = "bassembly required numeric." : GoTo selesai
        End If
        'bretur(105) As Integer
        If (IsNumeric(dataUtama(105)) = False) Then
            result(2) = "bretur required numeric." : GoTo selesai
        End If
        'bminorder(107) As Double
        If (IsNumeric(dataUtama(107)) = False) Then
            result(2) = "bminorder required numeric." : GoTo selesai
        End If
        'bkp (110) As Integer
        If (IsNumeric(dataUtama(110)) = False) Then
            result(2) = "bkp required numeric." : GoTo selesai
        End If
        'bkl(111) As Integer
        If (IsNumeric(dataUtama(111)) = False) Then
            result(2) = "bkl required numeric." : GoTo selesai
        End If
        'bjmllapangan(112) As Double
        If (IsNumeric(dataUtama(112)) = False) Then
            result(2) = "bjmllapangan required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'bkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "bkode should not be more than 100 character." : GoTo selesai
        End If

        'bnama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "bnama should not be more than 100 character." : GoTo selesai
        End If

        'btipe(8) As String
        'If Len(dataUtama(8)) = 0 Then
        '    result(2) = "btipe can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(8)) > 100 Then
            result(2) = "btipe should not be more than 100 character." : GoTo selesai
        End If

        'bjenis(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 5 Then
            result(2) = "bjenis should not be more than 5 character." : GoTo selesai
        End If

        'bkategori(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "bkategori can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "bkategori should not be more than 50 character." : GoTo selesai
        End If

        'bsatuan(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bsatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "bsatuan should not be more than 25 character." : GoTo selesai
        End If

        'bsatuandefault(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bsatuandefault can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bsatuandefault should not be more than 25 character." : GoTo selesai
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

        'bdiskonjual1(49) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "bdiskonjual1 can't be empty" : GoTo selesai
        End If

        'bdiskonjual2(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bdiskonjual2 can't be empty" : GoTo selesai
        End If

        'bdiskonjual3(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bdiskonjual3 can't be empty" : GoTo selesai
        End If

        'bdiskonjual4(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bdiskonjual4 can't be empty" : GoTo selesai
        End If

        'bdiskonjual5(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bdiskonjual5 can't be empty" : GoTo selesai
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

        'bapanjang(65) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "bapanjang can't be empty" : GoTo selesai
        End If

        'balebar(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "balebar can't be empty" : GoTo selesai
        End If

        'batinggi(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "batinggi can't be empty" : GoTo selesai
        End If

        'bavolume(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "bavolume can't be empty" : GoTo selesai
        End If

        'baberat(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "baberat can't be empty" : GoTo selesai
        End If

        'bcustom14(94) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "bcustom14 can't be empty" : GoTo selesai
        End If

        'bcustom15(95) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "bcustom15 can't be empty" : GoTo selesai
        End If

        'binputtgl(98) As DateTime
        If Len(dataUtama(98)) = 0 Then
            result(2) = "binputtgl can't be empty" : GoTo selesai
        End If

        'bmodifikasitgl(100) As DateTime
        If Len(dataUtama(100)) = 0 Then
            result(2) = "bmodifikasitgl can't be empty" : GoTo selesai
        End If

        'bkelasproduk(104) As String
        If Len(dataUtama(104)) = 0 Then
            result(2) = "bkelasproduk can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(104)) > 25 Then
            result(2) = "bkelasproduk should not be more than 25 character." : GoTo selesai
        End If

        'btag(106) As String
        If Len(dataUtama(106)) = 0 Then
            result(2) = "btag can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(106)) > 25 Then
            result(2) = "btag should not be more than 25 character." : GoTo selesai
        End If

        If Len(dataUtama(113)) = 0 Then
            result(2) = "bsatuanlapangan can't be empty" : GoTo selesai
        End If

        If Len(dataUtama(113)) > 50 Then
            result(2) = "bsatuanlapangan should not be more than 50 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "bid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnama", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnamaalias5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "btipe", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjenis", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjenisdetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkategori", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bketerangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuandefault", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bnilaisatuandefault", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhpp", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "blokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubdivisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bproyek", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubitem", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bsubitemdari", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bbarcode", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsuplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "baktif", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "baktiftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstokminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstokmaksimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "breorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjmlorderbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bjmlorderjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkategoriumur", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstatusmoving", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsifatharga", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpromo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bpromoberlaku", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpajakbeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bpajakjual", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargabeli", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhppaverage", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bhargajual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdiskonjual5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bstok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkomisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmarginminimal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekpersediaan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekreturpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekdiskonpenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekhargapokok", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekreturpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekdiskonpembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "brekkonsinyasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bapanjang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "balebar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "batinggi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bavolume", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baberat", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bawarna", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baoem", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bamerk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "baukuran", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bamodel", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bakelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bserial", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bbatch", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bpengganti", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bgambar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "burutan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bcustom1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom6", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom7", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom8", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom9", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom10", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom11", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bcustom12", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bcustom13", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bcustom14", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcustom15", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "binputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "binputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bedithpp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bmobile", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bassembly", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkelasproduk", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bretur", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "btag", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bminorder", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdepartemen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubdepartemen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bkp", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bkl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "bjmllapangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsatuanlapangan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsubkelas", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bmaterial", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bsection", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bvendor", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "bdesigner", AsEnumTypeData.AsString)

        If AsDataTableTambahData(dtutama, "bid~bkode~bnama~bnamaalias1~bnamaalias2~bnamaalias3~bnamaalias4~bnamaalias5~btipe~bjenis~bjenisdetail~bkategori~bketerangan~bsatuan~bnilaisatuan~bsatuandefault~bnilaisatuandefault~bhpp~bcabang~blokasi~bdivisi~bsubdivisi~bgudang~bproyek~bsubitem~bsubitemdari~bbarcode~bsuplier~baktif~baktiftgl~bstokminimal~bstokmaksimal~breorder~bjmlorderbeli~bjmlorderjual~bkategoriumur~bstatusmoving~bsifatharga~bpromo~bpromoberlaku~bpajakbeli~bpajakjual~bhargabeli~bhppaverage~bhargajual1~bhargajual2~bhargajual3~bhargajual4~bhargajual5~bdiskonjual1~bdiskonjual2~bdiskonjual3~bdiskonjual4~bdiskonjual5~bstok~bkomisi~bmarginminimal~brekpersediaan~brekpenjualan~brekreturpenjualan~brekdiskonpenjualan~brekhargapokok~brekreturpembelian~brekdiskonpembelian~brekkonsinyasi~bapanjang~balebar~batinggi~bavolume~baberat~bawarna~baoem~bamerk~baukuran~bamodel~bakelas~bserial~bbatch~bpengganti~bgambar~burutan~bcustom1~bcustom2~bcustom3~bcustom4~bcustom5~bcustom6~bcustom7~bcustom8~bcustom9~bcustom10~bcustom11~bcustom12~bcustom13~bcustom14~bcustom15~bcatatan~binputuser~binputtgl~bmodifikasiuser~bmodifikasitgl~bedithpp~bmobile~bassembly~bkelasproduk~bretur~btag~bminorder~bdepartemen~bsubdepartemen~bkp~bkl~bjmllapangan~bsatuanlapangan~bsubkelas~bmaterial~bsection~bvendor~bdesigner", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92) & "~" & dataUtama(93) & "~" & dataUtama(94) & "~" & dataUtama(95) & "~" & dataUtama(96) & "~" & dataUtama(97) & "~" & dataUtama(98) & "~" & dataUtama(99) & "~" & dataUtama(100) & "~" & dataUtama(101) & "~" & dataUtama(102) & "~" & dataUtama(103) & "~" & dataUtama(104) & "~" & dataUtama(105) & "~" & dataUtama(106) & "~" & dataUtama(107) & "~" & dataUtama(108) & "~" & dataUtama(109) & "~" & dataUtama(110) & "~" & dataUtama(111) & "~" & dataUtama(112) & "~" & dataUtama(113) & "~" & dataUtama(114) & "~" & dataUtama(115) & "~" & dataUtama(116) & "~" & dataUtama(117) & "~" & dataUtama(118)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If


        ''********************************* ITEM LOCATION WAREHOUSE *********************************

        ''MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        ''blgidbarang(0) As Integer, blgkodebarang(1) As String, blggudang(2) As String, blgidlokasi(3) As Integer, blgkodelokasi(4) As String, 
        ''blgnamalokasi(5) As String, blginputuser(6) As Integer, blginputtgl(7) As DateTime, blgmodifikasiuser(8) As Integer, blgmodifikasitgl(9) As DateTime

        ''MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        ''blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, 
        ''blginputtgl, blgmodifikasiuser, blgmodifikasitgl

        ''Buat datatable detail
        'Dim dtILW As New DataTable
        'AsDataTableTambahField(dtILW, "blgidbarang", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtILW, "blgkodebarang", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtILW, "blggudang", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtILW, "blgidlokasi", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtILW, "blgkodelokasi", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtILW, "blgnamalokasi", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtILW, "blginputuser", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtILW, "blginputtgl", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtILW, "blgmodifikasiuser", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtILW, "blgmodifikasitgl", AsEnumTypeData.AsString)

        'If (Len(dataSplit(1)) <> 0) Then

        '    'VALIDASI DAN SET DATA DETAIL ======================================================
        '    'SPLIT PARAMETER DATA DETAIL
        '    dataILW = dataSplit(1).Split(sptRow)
        '    'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        '    'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        '    Dim JmldtILW As Integer = dataILW.Length
        '    For i = 1 To JmldtILW
        '        'SPLIT DATA DETAIL
        '        dataRowILW = dataILW(i - 1).Split(sptField)

        '        'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
        '        'CEK ARRAY DATA DETAIL
        '        If (dataRowILW.Length <> 10) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - Invalid item location warehouse data parameter." : GoTo selesai
        '        End If
        '        'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

        '        'VALIDASI TIPE DATA DETAIL ------------------------------------------
        '        'blgidbarang(0) As Integer
        '        If (IsNumeric(dataRowILW(0)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgidbarang required numeric." : GoTo selesai
        '        End If
        '        'blgidlokasi(3) As Integer
        '        If (IsNumeric(dataRowILW(3)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgidlokasi required numeric." : GoTo selesai
        '        End If
        '        'blginputuser(6) As Integer
        '        If (IsNumeric(dataRowILW(6)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blginputuser required numeric." : GoTo selesai
        '        End If
        '        'blginputtgl(7) As DateTime
        '        If (IsDate(dataRowILW(7)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blginputtgl required date." : GoTo selesai
        '        End If
        '        'blgmodifikasiuser(8) As Integer
        '        If (IsNumeric(dataRowILW(8)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasiuser required numeric." : GoTo selesai
        '        End If
        '        'blgmodifikasitgl(9) As DateTime
        '        If (IsDate(dataRowILW(9)) = False) Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasitgl required date." : GoTo selesai
        '        End If
        '        'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

        '        'VALIDASI DATA DETAIL ---------------------------------------
        '        'blgkodebarang(1) As String
        '        If Len(dataRowILW(1)) = 0 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgkodebarang can't be empty" : GoTo selesai
        '        End If
        '        If Len(dataRowILW(1)) > 100 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgkodebarang should not be more than 100 character." : GoTo selesai
        '        End If

        '        'blggudang(2) As String
        '        If Len(dataRowILW(2)) = 0 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blggudang can't be empty" : GoTo selesai
        '        End If
        '        If Len(dataRowILW(2)) > 25 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blggudang should not be more than 25 character." : GoTo selesai
        '        End If

        '        'blginputtgl(7) As DateTime
        '        If Len(dataRowILW(7)) = 0 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blginputtgl can't be empty" : GoTo selesai
        '        End If

        '        'blgmodifikasitgl(9) As DateTime
        '        If Len(dataRowILW(9)) = 0 Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - blgmodifikasitgl can't be empty" : GoTo selesai
        '        End If

        '        'END OF VALIDASI DATA DETAIL --------------------------------

        '        If AsDataTableTambahData(dtILW, "blgidbarang~blgkodebarang~blggudang~blgidlokasi~blgkodelokasi~blgnamalokasi~blginputuser~blginputtgl~blgmodifikasiuser~blgmodifikasitgl", dataRowILW(0) & "~" & dataRowILW(1) & "~" & dataRowILW(2) & "~" & dataRowILW(3) & "~" & dataRowILW(4) & "~" & dataRowILW(5) & "~" & dataRowILW(6) & "~" & dataRowILW(7) & "~" & dataRowILW(8) & "~" & dataRowILW(9)) = False Then
        '            result(2) = "Item Location Warehouse Row : " & i & " - insert into datatable failed." : GoTo selesai
        '        End If

        '    Next
        '    'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================
        'End If

        ''***************************** END OF ITEM LOCATION WAREHOUSE ******************************



        ''************************************* ITEM ASSEMBLY ***************************************

        ''MAPPING BUAT WS DATA ASSEMBLY -------------------------------------------------------
        ''iaidbarang(0) As Integer, iakodebarang(1) As String, iaidbarangpenyusun(2) As Integer, iakodebarangpenyusun(3) As String, iaurutan(4) As Integer, 
        ''iajml(5) As Double, iasatuan(6) As String, iainputuser(7) As Integer, iainputtgl(8) As DateTime, iamodifikasiuser(9) As Integer, 
        ''iamodifikasitgl(10) As DateTime

        ''MAPPING BUAT FLEX DATA ASSEMBLY -----------------------------------------------------
        ''iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, 
        ''iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl

        ''Buat datatable detail
        'Dim dtIA As New DataTable
        'AsDataTableTambahField(dtIA, "iaidbarang", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIA, "iakodebarang", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIA, "iaidbarangpenyusun", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIA, "iakodebarangpenyusun", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIA, "iaurutan", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIA, "iajml", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIA, "iasatuan", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIA, "iainputuser", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIA, "iainputtgl", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIA, "iamodifikasiuser", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIA, "iamodifikasitgl", AsEnumTypeData.AsString)

        ''JIKA BARANG ASSEMBLY LANGSUNG MAKA WAJIB ISI BARANG PENYUSUN
        ''bassembly(103) As Integer
        'If Double.Parse(dataUtama(103)) = 1 And Len(dataSplit(2)) = 0 Then
        '    result(2) = "Item Assembly data not found." : GoTo selesai
        'End If


        'If (Len(dataSplit(2)) <> 0) Then
        '    'VALIDASI DAN SET DATA ASSEMBLY ======================================================
        '    'SPLIT PARAMETER DATA ASSEMBLY
        '    dataIA = dataSplit(2).Split(sptRow)
        '    'END OF VALIDASI DAN SET DATA ASSEMBLY ===============================================

        '    'VALIDASI DAN SET DATA ROW DETAIL ==================================================
        '    Dim JmldtIA As Integer = dataIA.Length
        '    For i = 1 To JmldtIA
        '        'SPLIT DATA DETAIL
        '        dataRowIA = dataIA(i - 1).Split(sptField)

        '        'VALIDASI DAN SET ROW DATA DETAIL -----------------------------------
        '        'CEK ARRAY DATA DETAIL
        '        If (dataRowIA.Length <> 11) Then
        '            result(2) = "Item Assembly Row : " & i & " - Invalid item assembly data parameter." : GoTo selesai
        '        End If
        '        'END OF VALIDASI DAN SET DATA ROW DETAIL ----------------------------

        '        'VALIDASI TIPE DATA DETAIL ------------------------------------------
        '        'iaidbarang(0) As Integer
        '        If (IsNumeric(dataRowIA(0)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iaidbarang required numeric." : GoTo selesai
        '        End If
        '        'iaidbarangpenyusun(2) As Integer
        '        If (IsNumeric(dataRowIA(2)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iaidbarangpenyusun required numeric." : GoTo selesai
        '        End If
        '        'iaurutan(4) As Integer
        '        If (IsNumeric(dataRowIA(4)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iaurutan required numeric." : GoTo selesai
        '        End If
        '        'iajml(5) As Double
        '        If (IsNumeric(dataRowIA(5)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iajml required numeric." : GoTo selesai
        '        End If
        '        'iainputuser(7) As Integer
        '        If (IsNumeric(dataRowIA(7)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iainputuser required numeric." : GoTo selesai
        '        End If
        '        'iainputtgl(8) As DateTime
        '        If (IsDate(dataRowIA(8)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iainputtgl required date." : GoTo selesai
        '        End If
        '        'iamodifikasiuser(9) As Integer
        '        If (IsNumeric(dataRowIA(9)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iamodifikasiuser required numeric." : GoTo selesai
        '        End If
        '        'iamodifikasitgl(10) As DateTime
        '        If (IsDate(dataRowIA(10)) = False) Then
        '            result(2) = "Item Assembly Row : " & i & " - iamodifikasitgl required date." : GoTo selesai
        '        End If
        '        'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

        '        'VALIDASI DATA DETAIL ---------------------------------------
        '        'iakodebarang(1) As String
        '        If Len(dataRowIA(1)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarang can't be empty" : GoTo selesai
        '        End If
        '        If Len(dataRowIA(1)) > 25 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarang should not be more than 25 character." : GoTo selesai
        '        End If

        '        'iakodebarangpenyusun(3) As String
        '        If Len(dataRowIA(3)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarangpenyusun can't be empty" : GoTo selesai
        '        End If
        '        If Len(dataRowIA(3)) > 25 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iakodebarangpenyusun should not be more than 25 character." : GoTo selesai
        '        End If

        '        'iajml(5) As Double
        '        If Len(dataRowIA(5)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iajml can't be empty" : GoTo selesai
        '        End If

        '        'iasatuan(6) As String
        '        If Len(dataRowIA(6)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iasatuan can't be empty" : GoTo selesai
        '        End If
        '        If Len(dataRowIA(6)) > 25 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iasatuan should not be more than 25 character." : GoTo selesai
        '        End If

        '        'iainputtgl(8) As DateTime
        '        If Len(dataRowIA(8)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iainputtgl can't be empty" : GoTo selesai
        '        End If

        '        'iamodifikasitgl(10) As DateTime
        '        If Len(dataRowIA(10)) = 0 Then
        '            result(2) = "Item Assembly Row : " & i & " - Row : " & i & " - iamodifikasitgl can't be empty" : GoTo selesai
        '        End If

        '        'END OF VALIDASI DATA DETAIL --------------------------------

        '        If AsDataTableTambahData(dtIA, "iaidbarang~iakodebarang~iaidbarangpenyusun~iakodebarangpenyusun~iaurutan~iajml~iasatuan~iainputuser~iainputtgl~iamodifikasiuser~iamodifikasitgl", dataRowIA(0) & "~" & dataRowIA(1) & "~" & dataRowIA(2) & "~" & dataRowIA(3) & "~" & dataRowIA(4) & "~" & dataRowIA(5) & "~" & dataRowIA(6) & "~" & dataRowIA(7) & "~" & dataRowIA(8) & "~" & dataRowIA(9) & "~" & dataRowIA(10)) = False Then
        '            result(2) = "Item Assembly Row : " & i & " - insert into datatable failed." : GoTo selesai
        '        End If

        '    Next
        '    'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================
        'End If

        ''********************************** END OF ITEM ASSEMBLY ***********************************


        ''************************************ ITEM SUPPLIER ****************************************

        ''isidbarang(0) As Integer, isidkontak(1) As Integer, iscatatan(2) As String, isurutan(3) As Integer, iscustomtext1(4) As String, 
        ''iscustomtext2(5) As String, iscustomtext3(6) As String, iscustomtext4(7) As String, iscustomtext5(8) As String, iscustomint1(9) As Integer, 
        ''iscustomint2(10) As Integer, iscustomint3(11) As Integer, iscustomdbl1(12) As Double, iscustomdbl2(13) As Double, iscustomdbl3(14) As Double, 
        ''iscustomdate1(15) As Date, iscustomdate2(16) As Date, iscustomdate3(17) As Date

        ''MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        ''isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, 
        ''iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, 
        ''iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3

        ''Buat datatable Item Supplier
        'Dim dtIS As New DataTable
        'AsDataTableTambahField(dtIS, "isidbarang", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "isidkontak", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "iscatatan", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "isurutan", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "iscustomtext1", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomtext2", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomtext3", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomtext4", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomtext5", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomint1", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "iscustomint2", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "iscustomint3", AsEnumTypeData.AsInt64)
        'AsDataTableTambahField(dtIS, "iscustomdbl1", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomdbl2", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomdbl3", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomdate1", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomdate2", AsEnumTypeData.AsString)
        'AsDataTableTambahField(dtIS, "iscustomdate3", AsEnumTypeData.AsString)

        'If (Len(dataSplit(3)) <> 0) Then

        '    'VALIDASI DAN SET DATA Item Supplier ======================================================
        '    'SPLIT PARAMETER DATA Item Supplier
        '    dataIS = dataSplit(3).Split(sptRow)
        '    'END OF VALIDASI DAN SET DATA Item Supplier ===============================================

        '    'VALIDASI DAN SET DATA ROW Item Supplier ==================================================
        '    Dim JmlDtIS As Integer = dataIS.Length
        '    For i = 1 To JmlDtIS
        '        'SPLIT DATA Item Supplier
        '        dataRowIS = dataIS(i - 1).Split(sptField)

        '        'VALIDASI DAN SET ROW DATA Item Supplier -----------------------------------
        '        'CEK ARRAY DATA Item Supplier
        '        If (dataRowIS.Length <> 18) Then
        '            result(2) = "Item Supplier Row : " & i & " - Invalid Item Supplier transaction data parameter." : GoTo selesai
        '        End If
        '        'END OF VALIDASI DAN SET DATA ROW Item Supplier ----------------------------

        '        'VALIDASI TIPE DATA Item Supplier ------------------------------------------
        '        'isidbarang(0) As Integer
        '        If (IsNumeric(dataRowIS(0)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - isidbarang required numeric." : GoTo selesai
        '        End If
        '        'isidkontak(1) As Integer
        '        If (IsNumeric(dataRowIS(1)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - isidkontak required numeric." : GoTo selesai
        '        End If
        '        'isurutan(3) As Integer
        '        If (IsNumeric(dataRowIS(3)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - isurutan required numeric." : GoTo selesai
        '        End If
        '        'iscustomint1(9) As Integer
        '        If (IsNumeric(dataRowIS(9)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomint1 required numeric." : GoTo selesai
        '        End If
        '        'iscustomint2(10) As Integer
        '        If (IsNumeric(dataRowIS(10)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomint2 required numeric." : GoTo selesai
        '        End If
        '        'iscustomint3(11) As Integer
        '        If (IsNumeric(dataRowIS(11)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomint3 required numeric." : GoTo selesai
        '        End If
        '        'iscustomdbl1(12) As Double
        '        If (IsNumeric(dataRowIS(12)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl1 required numeric." : GoTo selesai
        '        End If
        '        'iscustomdbl2(13) As Double
        '        If (IsNumeric(dataRowIS(13)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl2 required numeric." : GoTo selesai
        '        End If
        '        'iscustomdbl3(14) As Double
        '        If (IsNumeric(dataRowIS(14)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl3 required numeric." : GoTo selesai
        '        End If
        '        'iscustomdate1(15) As Date
        '        If (IsDate(dataRowIS(15)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate1 required date." : GoTo selesai
        '        End If
        '        'iscustomdate2(16) As Date
        '        If (IsDate(dataRowIS(16)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate2 required date." : GoTo selesai
        '        End If
        '        'iscustomdate3(17) As Date
        '        If (IsDate(dataRowIS(17)) = False) Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate3 required date." : GoTo selesai
        '        End If
        '        'END OF VALIDASI TIPE DATA Item Supplier -----------------------------------

        '        'VALIDASI DATA Item Supplier ---------------------------------------
        '        'iscustomdbl1(12) As Double
        '        If Len(dataRowIS(12)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl1 can't be empty" : GoTo selesai
        '        End If

        '        'iscustomdbl2(13) As Double
        '        If Len(dataRowIS(13)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl2 can't be empty" : GoTo selesai
        '        End If

        '        'iscustomdbl3(14) As Double
        '        If Len(dataRowIS(14)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdbl3 can't be empty" : GoTo selesai
        '        End If

        '        'iscustomdate1(15) As Date
        '        If Len(dataRowIS(15)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate1 can't be empty" : GoTo selesai
        '        End If

        '        'iscustomdate2(16) As Date
        '        If Len(dataRowIS(16)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate2 can't be empty" : GoTo selesai
        '        End If

        '        'iscustomdate3(17) As Date
        '        If Len(dataRowIS(17)) = 0 Then
        '            result(2) = "Item Supplier Row : " & i & " - iscustomdate3 can't be empty" : GoTo selesai
        '        End If

        '        'END OF VALIDASI DATA Item Supplier --------------------------------

        '        If AsDataTableTambahData(dtIS, "isidbarang~isidkontak~iscatatan~isurutan~iscustomtext1~iscustomtext2~iscustomtext3~iscustomtext4~iscustomtext5~iscustomint1~iscustomint2~iscustomint3~iscustomdbl1~iscustomdbl2~iscustomdbl3~iscustomdate1~iscustomdate2~iscustomdate3", dataRowIS(0) & "~" & dataRowIS(1) & "~" & dataRowIS(2) & "~" & dataRowIS(3) & "~" & dataRowIS(4) & "~" & dataRowIS(5) & "~" & dataRowIS(6) & "~" & dataRowIS(7) & "~" & dataRowIS(8) & "~" & dataRowIS(9) & "~" & dataRowIS(10) & "~" & dataRowIS(11) & "~" & dataRowIS(12) & "~" & dataRowIS(13) & "~" & dataRowIS(14) & "~" & dataRowIS(15) & "~" & dataRowIS(16) & "~" & dataRowIS(17)) = False Then
        '            result(2) = "Item Supplier Row : " & i & " - insert into datatable failed." : GoTo selesai
        '        End If

        '    Next
        '    'END OF VALIDASI DAN SET ROW DATA Item Supplier ===========================================

        'End If
        ''******************************** END OF ITEM SUPPLIER *************************************


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
                Dim dr1 As DataRow = dtutama.Rows(0)
                'If isUpdate Then
                '    result(4) = dr1("bid")
                '    kode = dr1("bkode")
                '    'JIKA UPDATE CEK JML ROW PADA DATABASE
                '    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bid) FROM M1_Item WHERE bid='" & result(4) & "'")
                '    rowUpdate = dtupdate.Rows(0)(0)

                '    If (rowUpdate > 0) Then
                '        'SIMPAN HISTORY ========================
                '        Dim SimpanHistory As New m1_item_History
                '        Dim itemSimpanHistory As String = SimpanHistory.M1_Item_HistorySimpan("" & paramSplit(0) & "★M1_Item_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                '        Dim itemSplit() As String = itemSimpanHistory.Split(sptParam)
                '        Dim itemSplitResult() As String = itemSplit(0).Split(sptSubParam)
                '        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                '        If (itemSplitResult(1) = 0) Then
                '            result(2) = "Insert history failed : " & itemSplitResult(2) : Trans.Rollback() : GoTo selesai
                '        End If
                '        'END OF SIMPAN HISTORY ==================

                '        sql = "Update M1_Item set bkode  = '" & FixQuotes(dr1("bkode")) & "', bnama  = '" & FixQuotes(dr1("bnama")) & "', bnamaalias1  = '" & FixQuotes(dr1("bnamaalias1")) & "', bnamaalias2  = '" & FixQuotes(dr1("bnamaalias2")) & "', bnamaalias3  = '" & FixQuotes(dr1("bnamaalias3")) & "', bnamaalias4  = '" & FixQuotes(dr1("bnamaalias4")) & "', bnamaalias5  = '" & FixQuotes(dr1("bnamaalias5")) & "', btipe  = '" & FixQuotes(dr1("btipe")) & "', bjenis  = '" & FixQuotes(dr1("bjenis")) & "', bjenisdetail  = " & dr1("bjenisdetail") & ", bkategori  = '" & FixQuotes(dr1("bkategori")) & "', bketerangan  = '" & FixQuotes(dr1("bketerangan")) & "', bsatuan  = '" & FixQuotes(dr1("bsatuan")) & "', bnilaisatuan  = '" & FixDouble(dr1("bnilaisatuan")) & "', bsatuandefault  = '" & FixQuotes(dr1("bsatuandefault")) & "', bnilaisatuandefault  = '" & FixDouble(dr1("bnilaisatuandefault")) & "', bhpp  = '" & FixQuotes(dr1("bhpp")) & "', bcabang  = '" & FixQuotes(dr1("bcabang")) & "', blokasi  = '" & FixQuotes(dr1("blokasi")) & "', bdivisi  = '" & FixQuotes(dr1("bdivisi")) & "', bsubdivisi  = '" & FixQuotes(dr1("bsubdivisi")) & "', bgudang  = '" & FixQuotes(dr1("bgudang")) & "', bproyek  = '" & FixQuotes(dr1("bproyek")) & "', bsubitem  = " & dr1("bsubitem") & ", bsubitemdari  = " & dr1("bsubitemdari") & ", bbarcode  = '" & FixQuotes(dr1("bbarcode")) & "', bsuplier  = " & dr1("bsuplier") & ", baktif  = " & dr1("baktif") & ", baktiftgl  = '" & FixQuotes(AsFormatTanggal(dr1("baktiftgl"))) & "', bstokminimal  = '" & FixDouble(dr1("bstokminimal")) & "', bstokmaksimal  = '" & FixDouble(dr1("bstokmaksimal")) & "', breorder  = '" & FixDouble(dr1("breorder")) & "', bjmlorderbeli  = '" & FixDouble(dr1("bjmlorderbeli")) & "', bjmlorderjual  = '" & FixDouble(dr1("bjmlorderjual")) & "', bkategoriumur  = '" & FixQuotes(dr1("bkategoriumur")) & "', bstatusmoving  = '" & FixQuotes(dr1("bstatusmoving")) & "', bsifatharga  = '" & FixQuotes(dr1("bsifatharga")) & "', bpromo  = " & dr1("bpromo") & ", bpromoberlaku  = '" & FixQuotes(AsFormatTanggal(dr1("bpromoberlaku"))) & "', bpajakbeli  = '" & FixQuotes(dr1("bpajakbeli")) & "', bpajakjual  = '" & FixQuotes(dr1("bpajakjual")) & "', bhargabeli  = '" & FixDouble(dr1("bhargabeli")) & "', bhppaverage  = '" & FixDouble(dr1("bhppaverage")) & "', bhargajual1  = '" & FixDouble(dr1("bhargajual1")) & "', bhargajual2  = '" & FixDouble(dr1("bhargajual2")) & "', bhargajual3  = '" & FixDouble(dr1("bhargajual3")) & "', bhargajual4  = '" & FixDouble(dr1("bhargajual4")) & "', bhargajual5  = '" & FixDouble(dr1("bhargajual5")) & "', bdiskonjual1  = '" & FixDouble(dr1("bdiskonjual1")) & "', bdiskonjual2  = '" & FixDouble(dr1("bdiskonjual2")) & "', bdiskonjual3  = '" & FixDouble(dr1("bdiskonjual3")) & "', bdiskonjual4  = '" & FixDouble(dr1("bdiskonjual4")) & "', bdiskonjual5  = '" & FixDouble(dr1("bdiskonjual5")) & "', bstok  = '" & FixDouble(dr1("bstok")) & "', bkomisi  = '" & FixDouble(dr1("bkomisi")) & "', bmarginminimal  = '" & FixDouble(dr1("bmarginminimal")) & "', brekpersediaan  = '" & FixQuotes(dr1("brekpersediaan")) & "', brekpenjualan  = '" & FixQuotes(dr1("brekpenjualan")) & "', brekreturpenjualan  = '" & FixQuotes(dr1("brekreturpenjualan")) & "', brekdiskonpenjualan  = '" & FixQuotes(dr1("brekdiskonpenjualan")) & "', brekhargapokok  = '" & FixQuotes(dr1("brekhargapokok")) & "', brekreturpembelian  = '" & FixQuotes(dr1("brekreturpembelian")) & "', brekdiskonpembelian  = '" & FixQuotes(dr1("brekdiskonpembelian")) & "', brekkonsinyasi  = '" & FixQuotes(dr1("brekkonsinyasi")) & "', bapanjang  = '" & FixDouble(dr1("bapanjang")) & "', balebar  = '" & FixDouble(dr1("balebar")) & "', batinggi  = '" & FixDouble(dr1("batinggi")) & "', bavolume  = '" & FixDouble(dr1("bavolume")) & "', baberat  = '" & FixDouble(dr1("baberat")) & "', bawarna  = '" & FixQuotes(dr1("bawarna")) & "', baoem  = '" & FixQuotes(dr1("baoem")) & "', bamerk  = '" & FixQuotes(dr1("bamerk")) & "', baukuran  = '" & FixQuotes(dr1("baukuran")) & "', bamodel  = '" & FixQuotes(dr1("bamodel")) & "', bakelas  = '" & FixQuotes(dr1("bakelas")) & "', bserial  = " & dr1("bserial") & ", bbatch  = " & dr1("bbatch") & ", bpengganti  = " & dr1("bpengganti") & ", bgambar  = '" & FixQuotes(dr1("bgambar")) & "', burutan  = " & dr1("burutan") & ", bcustom1  = '" & FixQuotes(dr1("bcustom1")) & "', bcustom2  = '" & FixQuotes(dr1("bcustom2")) & "', bcustom3  = '" & FixQuotes(dr1("bcustom3")) & "', bcustom4  = '" & FixQuotes(dr1("bcustom4")) & "', bcustom5  = '" & FixQuotes(dr1("bcustom5")) & "', bcustom6  = '" & FixQuotes(dr1("bcustom6")) & "', bcustom7  = '" & FixQuotes(dr1("bcustom7")) & "', bcustom8  = '" & FixQuotes(dr1("bcustom8")) & "', bcustom9  = '" & FixQuotes(dr1("bcustom9")) & "', bcustom10  = '" & FixQuotes(dr1("bcustom10")) & "', bcustom11  = " & dr1("bcustom11") & ", bcustom12  = " & dr1("bcustom12") & ", bcustom13  = " & dr1("bcustom13") & ", bcustom14  = '" & FixDouble(dr1("bcustom14")) & "', bcustom15  = '" & FixDouble(dr1("bcustom15")) & "', bcatatan  = '" & FixQuotes(dr1("bcatatan")) & "', bmodifikasiuser  = " & dr1("bmodifikasiuser") & ", bmodifikasitgl  = NOW(), bedithpp  = " & dr1("bedithpp") & ", bmobile = " & dr1("bmobile") & ", bassembly = " & dr1("bassembly") & ", bdownloaded = 0, bkelasproduk = '" & dr1("bkelasproduk") & "', bretur = '" & dr1("bretur") & "', btag = '" & dr1("btag") & "', bminorder = '" & dr1("bminorder") & "', bdepartemen = '" & dr1("bdepartemen") & "', bsubdepartemen = '" & dr1("bsubdepartemen") & "', bkp = '" & dr1("bkp") & "', bkl = '" & dr1("bkl") & "' , bjmllapangan = '" & dr1("bjmllapangan") & "' , bsatuanlapangan = '" & dr1("bsatuanlapangan") & "', bsubkelas = '" & dr1("bsubkelas") & "', bmaterial = '" & dr1("bmaterial") & "', bsection = '" & dr1("bsection") & "', bvendor = '" & dr1("bvendor") & "', bdesigner = '" & dr1("bdesigner") & "' where bid = '" & dr1("bid") & "'"
                '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '        With objCmd
                '            .Connection = Con1
                '            .Transaction = Trans
                '            .CommandType = CommandType.Text
                '            .CommandText = sql
                '        End With
                '        objCmd.ExecuteNonQuery()
                '    Else
                '        result(2) = "Item data not found." : Trans.Rollback() : GoTo selesai
                '    End If
                'Else

                kode = dr1("bkode")
                sql = "Insert into M1_Item (bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile, bassembly, bkelasproduk, bretur, btag, bminorder, bdepartemen, bsubdepartemen, bkp, bkl, bjmllapangan, bsatuanlapangan, bsubkelas, bmaterial, bsection, bvendor, bdesigner) values('" & FixQuotes(dr1("bkode")) & "', '" & FixQuotes(dr1("bnama")) & "', '" & FixQuotes(dr1("bnamaalias1")) & "', '" & FixQuotes(dr1("bnamaalias2")) & "', '" & FixQuotes(dr1("bnamaalias3")) & "', '" & FixQuotes(dr1("bnamaalias4")) & "', '" & FixQuotes(dr1("bnamaalias5")) & "', '" & FixQuotes(dr1("btipe")) & "', '" & FixQuotes(dr1("bjenis")) & "', " & dr1("bjenisdetail") & ", '" & FixQuotes(dr1("bkategori")) & "', '" & FixQuotes(dr1("bketerangan")) & "', '" & FixQuotes(dr1("bsatuan")) & "', '" & FixDouble(dr1("bnilaisatuan")) & "', '" & FixQuotes(dr1("bsatuandefault")) & "', '" & FixDouble(dr1("bnilaisatuandefault")) & "', '" & FixQuotes(dr1("bhpp")) & "', '" & FixQuotes(dr1("bcabang")) & "', '" & FixQuotes(dr1("blokasi")) & "', '" & FixQuotes(dr1("bdivisi")) & "', '" & FixQuotes(dr1("bsubdivisi")) & "', '" & FixQuotes(dr1("bgudang")) & "', '" & FixQuotes(dr1("bproyek")) & "', " & dr1("bsubitem") & ", " & dr1("bsubitemdari") & ", '" & FixQuotes(dr1("bbarcode")) & "', " & dr1("bsuplier") & ", " & dr1("baktif") & ", '" & FixQuotes(AsFormatTanggal(dr1("baktiftgl"))) & "', '" & FixDouble(dr1("bstokminimal")) & "', '" & FixDouble(dr1("bstokmaksimal")) & "', '" & FixDouble(dr1("breorder")) & "', '" & FixDouble(dr1("bjmlorderbeli")) & "', '" & FixDouble(dr1("bjmlorderjual")) & "', '" & FixQuotes(dr1("bkategoriumur")) & "', '" & FixQuotes(dr1("bstatusmoving")) & "', '" & FixQuotes(dr1("bsifatharga")) & "', " & dr1("bpromo") & ", '" & FixQuotes(AsFormatTanggal(dr1("bpromoberlaku"))) & "', '" & FixQuotes(dr1("bpajakbeli")) & "', '" & FixQuotes(dr1("bpajakjual")) & "', '" & FixDouble(dr1("bhargabeli")) & "', '" & FixDouble(dr1("bhppaverage")) & "', '" & FixDouble(dr1("bhargajual1")) & "', '" & FixDouble(dr1("bhargajual2")) & "', '" & FixDouble(dr1("bhargajual3")) & "', '" & FixDouble(dr1("bhargajual4")) & "', '" & FixDouble(dr1("bhargajual5")) & "', '" & FixDouble(dr1("bdiskonjual1")) & "', '" & FixDouble(dr1("bdiskonjual2")) & "', '" & FixDouble(dr1("bdiskonjual3")) & "', '" & FixDouble(dr1("bdiskonjual4")) & "', '" & FixDouble(dr1("bdiskonjual5")) & "', '" & FixDouble(dr1("bstok")) & "', '" & FixDouble(dr1("bkomisi")) & "', '" & FixDouble(dr1("bmarginminimal")) & "', '" & FixQuotes(dr1("brekpersediaan")) & "', '" & FixQuotes(dr1("brekpenjualan")) & "', '" & FixQuotes(dr1("brekreturpenjualan")) & "', '" & FixQuotes(dr1("brekdiskonpenjualan")) & "', '" & FixQuotes(dr1("brekhargapokok")) & "', '" & FixQuotes(dr1("brekreturpembelian")) & "', '" & FixQuotes(dr1("brekdiskonpembelian")) & "', '" & FixQuotes(dr1("brekkonsinyasi")) & "', '" & FixDouble(dr1("bapanjang")) & "', '" & FixDouble(dr1("balebar")) & "', '" & FixDouble(dr1("batinggi")) & "', '" & FixDouble(dr1("bavolume")) & "', '" & FixDouble(dr1("baberat")) & "', '" & FixQuotes(dr1("bawarna")) & "', '" & FixQuotes(dr1("baoem")) & "', '" & FixQuotes(dr1("bamerk")) & "', '" & FixQuotes(dr1("baukuran")) & "', '" & FixQuotes(dr1("bamodel")) & "', '" & FixQuotes(dr1("bakelas")) & "', " & dr1("bserial") & ", " & dr1("bbatch") & ", " & dr1("bpengganti") & ", '" & FixQuotes(dr1("bgambar")) & "', " & dr1("burutan") & ", '" & FixQuotes(dr1("bcustom1")) & "', '" & FixQuotes(dr1("bcustom2")) & "', '" & FixQuotes(dr1("bcustom3")) & "', '" & FixQuotes(dr1("bcustom4")) & "', '" & FixQuotes(dr1("bcustom5")) & "', '" & FixQuotes(dr1("bcustom6")) & "', '" & FixQuotes(dr1("bcustom7")) & "', '" & FixQuotes(dr1("bcustom8")) & "', '" & FixQuotes(dr1("bcustom9")) & "', '" & FixQuotes(dr1("bcustom10")) & "', " & FixDouble(dr1("bcustom11")) & ", " & FixDouble(dr1("bcustom12")) & ", " & FixDouble(dr1("bcustom13")) & ", '" & FixDouble(dr1("bcustom14")) & "', '" & FixDouble(dr1("bcustom15")) & "', '" & FixQuotes(dr1("bcatatan")) & "', " & dr1("binputuser") & ", NOW(), " & dr1("bmodifikasiuser") & ", '1971-01-01 00:00:00', " & dr1("bedithpp") & ", " & dr1("bmobile") & ", " & dr1("bassembly") & ", '" & dr1("bkelasproduk") & "', '" & dr1("bretur") & "', '" & dr1("btag") & "', '" & dr1("bminorder") & "', '" & dr1("bdepartemen") & "', '" & dr1("bsubdepartemen") & "', '" & dr1("bkp") & "', '" & dr1("bkl") & "', '" & dr1("bjmllapangan") & "', '" & dr1("bsatuanlapangan") & "', '" & FixQuotes(dr1("bsubkelas")) & "', '" & FixQuotes(dr1("bmaterial")) & "', '" & FixQuotes(dr1("bsection")) & "', '" & FixQuotes(dr1("bvendor")) & "', '" & FixQuotes(dr1("bdesigner")) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'INSERT SATUAN
                sql = "Insert into M1_Unit (ukode, unama, unilai, uketerangan, uaktif, uindexbarcode, uinputuser, uinputtgl, umodifikasiuser, umodifikasitgl) values('" & FixQuotes(dr1("bsatuan")) & "', '" & FixQuotes(dr1("bsatuan")) & "', '" & FixQuotes(dr1("bnilaisatuan")) & "', '', '1', '', '" & dr1("binputuser") & "', NOW(), 0, '1971-01-01 00:00:00'), ('" & FixQuotes(dr1("bsatuandefault")) & "', '" & FixQuotes(dr1("bsatuandefault")) & "', '" & FixQuotes(dr1("bnilaisatuandefault")) & "', '', '1', '', '" & dr1("binputuser") & "', NOW(), 0, '1971-01-01 00:00:00') ON DUPLICATE KEY UPDATE ukode = VALUES(ukode)"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Dim dt2 As New DataTable
                ''Sql disesuaikan sendiri, untuk parameternya disesuaikan sendiri.
                'dt2 = AsDataTableAmbilDariDB("select bid from M1_Item where bkode= '" & kode & "' limit 1")
                'If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Item data not found." : Trans.Rollback() : GoTo selesai

                'End If

                'UPDATE BARCODE NO BERIKUTNYA
                If Len(dr1("bbarcode")) > 0 Then
                    Dim dt As New DataTable

                    'AMBIL JMLDIGIT NO URUT BARCODE
                    Dim jmldigit As Double = 0
                    sql = "SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'options' AND skode = 'JmlDigitBarcode'"
                    dt = AsDataTableAmbilDariDB(sql)
                    If dt.Rows.Count > 0 Then
                        If IsNumeric(FxDB(dt.Rows(0)(0), 0)) Then
                            jmldigit = Double.Parse(FxDB(dt.Rows(0)(0), 0))
                        End If
                    End If

                    'JIKA BARCODE >= JMLDIGIT BARCODE SETTING
                    If dr1("bbarcode").Length >= jmldigit Then
                        'AMBIL AWALAN BARCODE
                        Dim awalan As String = Left(dr1("bbarcode"), dr1("bbarcode").Length - jmldigit)
                        'AMBIL URUTAN BARCODE
                        Dim nourut As Double = Double.Parse(Right(dr1("bbarcode"), jmldigit))

                        'AMBIL NO URUT BARCODE BERIKUTNYA
                        Dim noberikutnya As Double = 0
                        sql = "SELECT noberikutnya FROM m0_barcode_next WHERE awalan = '" & FixQuotes(awalan) & "'"
                        dt = AsDataTableAmbilDariDB(sql)
                        If dt.Rows.Count > 0 Then
                            noberikutnya = Double.Parse(FxDB(dt.Rows(0)(0), 1))
                        End If

                        If nourut >= noberikutnya Then
                            sql = "INSERT INTO m0_barcode_next (awalan, noberikutnya) VALUES ('" & FixQuotes(awalan) & "', " & FixDouble(nourut + 1) & ") ON DUPLICATE KEY UPDATE noberikutnya = VALUES(noberikutnya) "
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

            Else
                result(2) = "Main Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If

            ''Hapus item location warehouse ketika update
            'If (isUpdate) Then
            '    sql = "Delete from M1_Item_Location_Warehouse where blgidbarang = '" & result(4) & "'"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = Con1
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If

            ''Proses item location warehouse
            'If (dtILW.Rows.Count > 0) Then
            '    Dim strValue2 As New StringBuilder
            '    If isUpdate Then
            '        For Each dr1 As DataRow In dtILW.Rows
            '            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            '            strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("blgkodebarang")) & "', '" & FixQuotes(dr1("blggudang")) & "', " & dr1("blgidlokasi") & ", '" & FixQuotes(dr1("blgkodelokasi")) & "', '" & FixQuotes(dr1("blgnamalokasi")) & "', " & dr1("blginputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("blginputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("blgmodifikasiuser") & ", NOW())")
            '        Next
            '        sql = "Insert into M1_Item_Location_Warehouse(blgidbarang, blgkodebarang, blggudang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl) values" & strValue2.ToString & ""
            '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '        With objCmd
            '            .Connection = Con1
            '            .Transaction = Trans
            '            .CommandType = CommandType.Text
            '            .CommandText = sql
            '        End With
            '        objCmd.ExecuteNonQuery()
            '    Else
            '        For Each dr1 As DataRow In dtILW.Rows
            '            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            '            strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("blggudang")) & "', '" & FixQuotes(dr1("blgkodebarang")) & "', " & dr1("blgidlokasi") & ", '" & FixQuotes(dr1("blgkodelokasi")) & "', '" & FixQuotes(dr1("blgnamalokasi")) & "', " & dr1("blginputuser") & ", NOW(), " & dr1("blgmodifikasiuser") & ", '1971-01-01 00:00:00')")
            '        Next
            '        sql = "Insert into M1_Item_Location_Warehouse(blgidbarang, blggudang, blgkodebarang, blgidlokasi, blgkodelokasi, blgnamalokasi, blginputuser, blginputtgl, blgmodifikasiuser, blgmodifikasitgl) values" & strValue2.ToString & ""
            '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '        With objCmd
            '            .Connection = Con1
            '            .Transaction = Trans
            '            .CommandType = CommandType.Text
            '            .CommandText = sql
            '        End With
            '        objCmd.ExecuteNonQuery()
            '    End If
            'End If

            ''Hapus item assembly ketika update
            'If (isUpdate) Then
            '    sql = "Delete from M1_Item_Assembly where iaidbarang = '" & result(4) & "'"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = Con1
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If

            ''Proses item assembly
            'If (dtIA.Rows.Count > 0) Then
            '    Dim strValue2 As New StringBuilder
            '    If isUpdate Then
            '        For Each dr1 As DataRow In dtIA.Rows
            '            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            '            strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("iakodebarang")) & "', " & dr1("iaidbarangpenyusun") & ", '" & FixQuotes(dr1("iakodebarangpenyusun")) & "', " & dr1("iaurutan") & ", '" & FixDouble(dr1("iajml")) & "', '" & FixQuotes(dr1("iasatuan")) & "', " & dr1("iainputuser") & ", '" & FixQuotes(AsFormatTanggal(dr1("iainputtgl"), "yyyy-MM-dd H:mm:ss")) & "', " & dr1("iamodifikasiuser") & ", NOW())")
            '        Next
            '        sql = "Insert into M1_Item_Assembly(iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl) values" & strValue2.ToString & ""
            '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '        With objCmd
            '            .Connection = Con1
            '            .Transaction = Trans
            '            .CommandType = CommandType.Text
            '            .CommandText = sql
            '        End With
            '        objCmd.ExecuteNonQuery()
            '    Else
            '        For Each dr1 As DataRow In dtIA.Rows
            '            strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            '            strValue2.Append("(" & result(4) & ", '" & FixQuotes(dr1("iakodebarang")) & "', " & dr1("iaidbarangpenyusun") & ", '" & FixQuotes(dr1("iakodebarangpenyusun")) & "', " & dr1("iaurutan") & ", '" & FixDouble(dr1("iajml")) & "', '" & FixQuotes(dr1("iasatuan")) & "', " & dr1("iainputuser") & ", NOW(), " & dr1("iamodifikasiuser") & ", '1971-01-01 00:00:00')")
            '        Next
            '        sql = "Insert into M1_Item_Assembly(iaidbarang, iakodebarang, iaidbarangpenyusun, iakodebarangpenyusun, iaurutan, iajml, iasatuan, iainputuser, iainputtgl, iamodifikasiuser, iamodifikasitgl) values" & strValue2.ToString & ""
            '        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '        With objCmd
            '            .Connection = Con1
            '            .Transaction = Trans
            '            .CommandType = CommandType.Text
            '            .CommandText = sql
            '        End With
            '        objCmd.ExecuteNonQuery()
            '    End If
            'End If

            ''Hapus item supplier ketika update
            'If (isUpdate) Then
            '    sql = "Delete from M1_Item_Supplier where isidbarang = '" & result(4) & "'"
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = Con1
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If

            ''Proses item supplier
            'If (dtIS.Rows.Count > 0) Then
            '    Dim strValue2 As New StringBuilder
            '    For Each dr1 As DataRow In dtIS.Rows
            '        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
            '        strValue2.Append("(" & result(4) & ", " & dr1("isidkontak") & ", '" & FixQuotes(dr1("iscatatan")) & "', " & dr1("isurutan") & ", '" & FixQuotes(dr1("iscustomtext1")) & "', '" & FixQuotes(dr1("iscustomtext2")) & "', '" & FixQuotes(dr1("iscustomtext3")) & "', '" & FixQuotes(dr1("iscustomtext4")) & "', '" & FixQuotes(dr1("iscustomtext5")) & "', " & dr1("iscustomint1") & ", " & dr1("iscustomint2") & ", " & dr1("iscustomint3") & ", '" & FixDouble(dr1("iscustomdbl1")) & "', '" & FixDouble(dr1("iscustomdbl2")) & "', '" & FixDouble(dr1("iscustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(dr1("iscustomdate3"))) & "')")
            '    Next
            '    sql = "Insert into M1_Item_Supplier(isidbarang, isidkontak, iscatatan, isurutan, iscustomtext1, iscustomtext2, iscustomtext3, iscustomtext4, iscustomtext5, iscustomint1, iscustomint2, iscustomint3, iscustomdbl1, iscustomdbl2, iscustomdbl3, iscustomdate1, iscustomdate2, iscustomdate3) values" & strValue2.ToString & ""
            '    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            '    With objCmd
            '        .Connection = Con1
            '        .Transaction = Trans
            '        .CommandType = CommandType.Text
            '        .CommandText = sql
            '    End With
            '    objCmd.ExecuteNonQuery()
            'End If


            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = kode
            result(3) = 0
            result(4) = result(4)

            ''AMBIL DATA =============================================================
            'Dim paramSearch As String = M1_Item_DataSearch(PostWsSearch(paramSplit(0), "M1_Item_DataSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
            ''END OF AMBIL DATA ======================================================

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
    Public Function M1_ItemSimpan(ByVal param As String) As String

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
        'bid(0) As Integer, bkode(1) As String, bnama(2) As String, bnamaalias1(3) As String, bnamaalias2(4) As String, 
        'bnamaalias3(5) As String, bnamaalias4(6) As String, bnamaalias5(7) As String, btipe(8) As String, bjenis(9) As String, 
        'bjenisdetail(10) As Integer, bkategori(11) As String, bketerangan(12) As String, bsatuan(13) As String, bnilaisatuan(14) As Double, 
        'bsatuandefault(15) As String, bnilaisatuandefault(16) As Double, bhpp(17) As String, bcabang(18) As String, blokasi(19) As String, 
        'bdivisi(20) As String, bsubdivisi(21) As String, bgudang(22) As String, bproyek(23) As String, bsubitem(24) As Integer, 
        'bsubitemdari(25) As Integer, bbarcode(26) As String, bsuplier(27) As Integer, baktif(28) As Integer, baktiftgl(29) As Date, 
        'bstokminimal(30) As Double, bstokmaksimal(31) As Double, breorder(32) As Double, bjmlorderbeli(33) As Double, bjmlorderjual(34) As Double, 
        'bkategoriumur(35) As String, bstatusmoving(36) As String, bsifatharga(37) As String, bpromo(38) As Integer, bpromoberlaku(39) As Date, 
        'bpajakbeli(40) As String, bpajakjual(41) As String, bhargabeli(42) As Double, bhppaverage(43) As Double, bhargajual1(44) As Double, 
        'bhargajual2(45) As Double, bhargajual3(46) As Double, bhargajual4(47) As Double, bhargajual5(48) As Double, bdiskonjual1(49) As Double, 
        'bdiskonjual2(50) As Double, bdiskonjual3(51) As Double, bdiskonjual4(52) As Double, bdiskonjual5(53) As Double, bstok(54) As Double, 
        'bkomisi(55) As Double, bmarginminimal(56) As Double, brekpersediaan(57) As String, brekpenjualan(58) As String, brekreturpenjualan(59) As String, 
        'brekdiskonpenjualan(60) As String, brekhargapokok(61) As String, brekreturpembelian(62) As String, brekdiskonpembelian(63) As String, brekkonsinyasi(64) As String, 
        'bapanjang(65) As Double, balebar(66) As Double, batinggi(67) As Double, bavolume(68) As Double, baberat(69) As Double, 
        'bawarna(70) As String, baoem(71) As String, bamerk(72) As String, baukuran(73) As String, bamodel(74) As String, 
        'bakelas(75) As String, bserial(76) As Integer, bbatch(77) As Integer, bpengganti(78) As Integer, bgambar(79) As String, 
        'burutan(80) As Integer, bcustom1(81) As String, bcustom2(82) As String, bcustom3(83) As String, bcustom4(84) As String, 
        'bcustom5(85) As String, bcustom6(86) As String, bcustom7(87) As String, bcustom8(88) As String, bcustom9(89) As String, 
        'bcustom10(90) As String, bcustom11(91) As Integer, bcustom12(92) As Integer, bcustom13(93) As Integer, bcustom14(94) As Double, 
        'bcustom15(95) As Double, bcatatan(96) As String, binputuser(97) As Integer, binputtgl(98) As DateTime, bmodifikasiuser(99) As Integer, 
        'bmodifikasitgl(100) As DateTime, bedithpp(101) As Integer, bmobile(102) As Integer,
        'bsubkelas(103) As String, bmaterial(104) As String, bsection(105) As String, bvendor(106) As String, bdesigner(107) As String

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
        'brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, 
        'bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, 
        'bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, 
        'bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, 
        'bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, 
        'binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile,
        'bsubkelas, bmaterial, bsection, bvendor, bdesigner

        'VALIDASI DAN SET DATA =======================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 108) Then
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
        ''bdiskonjual1(49) As Double
        'If (IsNumeric(dataUtama(49)) = False) Then
        '    result(2) = "bdiskonjual1 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual2(50) As Double
        'If (IsNumeric(dataUtama(50)) = False) Then
        '    result(2) = "bdiskonjual2 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual3(51) As Double
        'If (IsNumeric(dataUtama(51)) = False) Then
        '    result(2) = "bdiskonjual3 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual4(52) As Double
        'If (IsNumeric(dataUtama(52)) = False) Then
        '    result(2) = "bdiskonjual4 required numeric." : GoTo selesai
        'End If
        ''bdiskonjual5(53) As Double
        'If (IsNumeric(dataUtama(53)) = False) Then
        '    result(2) = "bdiskonjual5 required numeric." : GoTo selesai
        'End If
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
        'bapanjang(65) As Double
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "bapanjang required numeric." : GoTo selesai
        End If
        'balebar(66) As Double
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "balebar required numeric." : GoTo selesai
        End If
        'batinggi(67) As Double
        If (IsNumeric(dataUtama(67)) = False) Then
            result(2) = "batinggi required numeric." : GoTo selesai
        End If
        'bavolume(68) As Double
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "bavolume required numeric." : GoTo selesai
        End If
        'baberat(69) As Double
        If (IsNumeric(dataUtama(69)) = False) Then
            result(2) = "baberat required numeric." : GoTo selesai
        End If
        'bserial(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "bserial required numeric." : GoTo selesai
        End If
        'bbatch(77) As Integer
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "bbatch required numeric." : GoTo selesai
        End If
        'bpengganti(78) As Integer
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "bpengganti required numeric." : GoTo selesai
        End If
        'burutan(80) As Integer
        If (IsNumeric(dataUtama(80)) = False) Then
            result(2) = "burutan required numeric." : GoTo selesai
        End If
        'bcustom11(91) As Integer
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "bcustom11 required numeric." : GoTo selesai
        End If
        'bcustom12(92) As Integer
        If (IsNumeric(dataUtama(92)) = False) Then
            result(2) = "bcustom12 required numeric." : GoTo selesai
        End If
        'bcustom13(93) As Integer
        If (IsNumeric(dataUtama(93)) = False) Then
            result(2) = "bcustom13 required numeric." : GoTo selesai
        End If
        'bcustom14(94) As Double
        If (IsNumeric(dataUtama(94)) = False) Then
            result(2) = "bcustom14 required numeric." : GoTo selesai
        End If
        'bcustom15(95) As Double
        If (IsNumeric(dataUtama(95)) = False) Then
            result(2) = "bcustom15 required numeric." : GoTo selesai
        End If
        'binputuser(97) As Integer
        If (IsNumeric(dataUtama(97)) = False) Then
            result(2) = "binputuser required numeric." : GoTo selesai
        End If
        'binputtgl(98) As DateTime
        If (IsDate(dataUtama(98)) = False) Then
            result(2) = "binputtgl required date." : GoTo selesai
        End If
        'bmodifikasiuser(99) As Integer
        If (IsNumeric(dataUtama(99)) = False) Then
            result(2) = "bmodifikasiuser required numeric." : GoTo selesai
        End If
        'bmodifikasitgl(100) As DateTime
        If (IsDate(dataUtama(100)) = False) Then
            result(2) = "bmodifikasitgl required date." : GoTo selesai
        End If
        'bedithpp(101) As Integer
        If (IsNumeric(dataUtama(101)) = False) Then
            result(2) = "bedithpp required numeric." : GoTo selesai
        End If
        'bmobile(102) As Integer
        If (IsNumeric(dataUtama(102)) = False) Then
            result(2) = "bmobile required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA ===================================================

        'VALIDASI DATA ===============================================================
        'bkode(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "bkode can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 100 Then
            result(2) = "bkode should not be more than 100 character." : GoTo selesai
        End If

        'bnama(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "bnama can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 100 Then
            result(2) = "bnama should not be more than 100 character." : GoTo selesai
        End If

        'btipe(8) As String
        'If Len(dataUtama(8)) = 0 Then
        '    result(2) = "btipe can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(8)) > 100 Then
            result(2) = "btipe should not be more than 100 character." : GoTo selesai
        End If

        'bjenis(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "bjenis can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 5 Then
            result(2) = "bjenis should not be more than 5 character." : GoTo selesai
        End If

        'bkategori(11) As String
        'If Len(dataUtama(11)) = 0 Then
        '    result(2) = "bkategori can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "bkategori should not be more than 50 character." : GoTo selesai
        End If

        'bsatuan(13) As String
        If Len(dataUtama(13)) = 0 Then
            result(2) = "bsatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(13)) > 25 Then
            result(2) = "bsatuan should not be more than 25 character." : GoTo selesai
        End If

        'bsatuandefault(15) As String
        If Len(dataUtama(15)) = 0 Then
            result(2) = "bsatuandefault can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(15)) > 25 Then
            result(2) = "bsatuandefault should not be more than 25 character." : GoTo selesai
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

        'bdiskonjual1(49) As Double
        If Len(dataUtama(49)) = 0 Then
            result(2) = "bdiskonjual1 can't be empty" : GoTo selesai
        End If

        'bdiskonjual2(50) As Double
        If Len(dataUtama(50)) = 0 Then
            result(2) = "bdiskonjual2 can't be empty" : GoTo selesai
        End If

        'bdiskonjual3(51) As Double
        If Len(dataUtama(51)) = 0 Then
            result(2) = "bdiskonjual3 can't be empty" : GoTo selesai
        End If

        'bdiskonjual4(52) As Double
        If Len(dataUtama(52)) = 0 Then
            result(2) = "bdiskonjual4 can't be empty" : GoTo selesai
        End If

        'bdiskonjual5(53) As Double
        If Len(dataUtama(53)) = 0 Then
            result(2) = "bdiskonjual5 can't be empty" : GoTo selesai
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

        'bapanjang(65) As Double
        If Len(dataUtama(65)) = 0 Then
            result(2) = "bapanjang can't be empty" : GoTo selesai
        End If

        'balebar(66) As Double
        If Len(dataUtama(66)) = 0 Then
            result(2) = "balebar can't be empty" : GoTo selesai
        End If

        'batinggi(67) As Double
        If Len(dataUtama(67)) = 0 Then
            result(2) = "batinggi can't be empty" : GoTo selesai
        End If

        'bavolume(68) As Double
        If Len(dataUtama(68)) = 0 Then
            result(2) = "bavolume can't be empty" : GoTo selesai
        End If

        'baberat(69) As Double
        If Len(dataUtama(69)) = 0 Then
            result(2) = "baberat can't be empty" : GoTo selesai
        End If

        'bcustom14(94) As Double
        If Len(dataUtama(94)) = 0 Then
            result(2) = "bcustom14 can't be empty" : GoTo selesai
        End If

        'bcustom15(95) As Double
        If Len(dataUtama(95)) = 0 Then
            result(2) = "bcustom15 can't be empty" : GoTo selesai
        End If

        'binputtgl(98) As DateTime
        If Len(dataUtama(98)) = 0 Then
            result(2) = "binputtgl can't be empty" : GoTo selesai
        End If

        'bmodifikasitgl(100) As DateTime
        If Len(dataUtama(100)) = 0 Then
            result(2) = "bmodifikasitgl can't be empty" : GoTo selesai
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
                dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(bid) FROM M1_Item WHERE bid='" & dataUtama(0) & "'")
                rowUpdate = dtupdate.Rows(0)(0)

                If (rowUpdate > 0) Then
                    'SIMPAN HISTORY ========================
                    Dim SimpanHistory As New m1_item_History
                    Dim itemSimpanHistory As String = SimpanHistory.M1_Item_HistorySimpan("" & paramSplit(0) & "★M1_Item_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(dataUtama(0)) & "")
                    Dim itemSplit() As String = itemSimpanHistory.Split(sptParam)
                    Dim itemSplitResult() As String = itemSplit(0).Split(sptSubParam)
                    'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                    If (itemSplitResult(1) = 0) Then
                        result(2) = "Insert history failed : " & itemSplitResult(2) : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF SIMPAN HISTORY ==================

                    sql = "Update M1_Item set bkode  = '" & FixQuotes(dataUtama(1)) & "', bnama  = '" & FixQuotes(dataUtama(2)) & "', bnamaalias1  = '" & FixQuotes(dataUtama(3)) & "', bnamaalias2  = '" & FixQuotes(dataUtama(4)) & "', bnamaalias3  = '" & FixQuotes(dataUtama(5)) & "', bnamaalias4  = '" & FixQuotes(dataUtama(6)) & "', bnamaalias5  = '" & FixQuotes(dataUtama(7)) & "', btipe  = '" & FixQuotes(dataUtama(8)) & "', bjenis  = '" & FixQuotes(dataUtama(9)) & "', bjenisdetail  = " & dataUtama(10) & ", bkategori  = '" & FixQuotes(dataUtama(11)) & "', bketerangan  = '" & FixQuotes(dataUtama(12)) & "', bsatuan  = '" & FixQuotes(dataUtama(13)) & "', bnilaisatuan  = '" & FixDouble(dataUtama(14)) & "', bsatuandefault  = '" & FixQuotes(dataUtama(15)) & "', bnilaisatuandefault  = '" & FixDouble(dataUtama(16)) & "', bhpp  = '" & FixQuotes(dataUtama(17)) & "', bcabang  = '" & FixQuotes(dataUtama(18)) & "', blokasi  = '" & FixQuotes(dataUtama(19)) & "', bdivisi  = '" & FixQuotes(dataUtama(20)) & "', bsubdivisi  = '" & FixQuotes(dataUtama(21)) & "', bgudang  = '" & FixQuotes(dataUtama(22)) & "', bproyek  = '" & FixQuotes(dataUtama(23)) & "', bsubitem  = " & dataUtama(24) & ", bsubitemdari  = " & dataUtama(25) & ", bbarcode  = '" & FixQuotes(dataUtama(26)) & "', bsuplier  = " & dataUtama(27) & ", baktif  = " & dataUtama(28) & ", baktiftgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(29))) & "', bstokminimal  = '" & FixDouble(dataUtama(30)) & "', bstokmaksimal  = '" & FixDouble(dataUtama(31)) & "', breorder  = '" & FixDouble(dataUtama(32)) & "', bjmlorderbeli  = '" & FixDouble(dataUtama(33)) & "', bjmlorderjual  = '" & FixDouble(dataUtama(34)) & "', bkategoriumur  = '" & FixQuotes(dataUtama(35)) & "', bstatusmoving  = '" & FixQuotes(dataUtama(36)) & "', bsifatharga  = '" & FixQuotes(dataUtama(37)) & "', bpromo  = " & dataUtama(38) & ", bpromoberlaku  = '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', bpajakbeli  = '" & FixQuotes(dataUtama(40)) & "', bpajakjual  = '" & FixQuotes(dataUtama(41)) & "', bhargabeli  = '" & FixDouble(dataUtama(42)) & "', bhppaverage  = '" & FixDouble(dataUtama(43)) & "', bhargajual1  = '" & FixDouble(dataUtama(44)) & "', bhargajual2  = '" & FixDouble(dataUtama(45)) & "', bhargajual3  = '" & FixDouble(dataUtama(46)) & "', bhargajual4  = '" & FixDouble(dataUtama(47)) & "', bhargajual5  = '" & FixDouble(dataUtama(48)) & "', bdiskonjual1  = '" & FixDouble(dataUtama(49)) & "', bdiskonjual2  = '" & FixDouble(dataUtama(50)) & "', bdiskonjual3  = '" & FixDouble(dataUtama(51)) & "', bdiskonjual4  = '" & FixDouble(dataUtama(52)) & "', bdiskonjual5  = '" & FixDouble(dataUtama(53)) & "', bstok  = '" & FixDouble(dataUtama(54)) & "', bkomisi  = '" & FixDouble(dataUtama(55)) & "', bmarginminimal  = '" & FixDouble(dataUtama(56)) & "', brekpersediaan  = '" & FixQuotes(dataUtama(57)) & "', brekpenjualan  = '" & FixQuotes(dataUtama(58)) & "', brekreturpenjualan  = '" & FixQuotes(dataUtama(59)) & "', brekdiskonpenjualan  = '" & FixQuotes(dataUtama(60)) & "', brekhargapokok  = '" & FixQuotes(dataUtama(61)) & "', brekreturpembelian  = '" & FixQuotes(dataUtama(62)) & "', brekdiskonpembelian  = '" & FixQuotes(dataUtama(63)) & "', brekkonsinyasi  = '" & FixQuotes(dataUtama(64)) & "', bapanjang  = '" & FixDouble(dataUtama(65)) & "', balebar  = '" & FixDouble(dataUtama(66)) & "', batinggi  = '" & FixDouble(dataUtama(67)) & "', bavolume  = '" & FixDouble(dataUtama(68)) & "', baberat  = '" & FixDouble(dataUtama(69)) & "', bawarna  = '" & FixQuotes(dataUtama(70)) & "', baoem  = '" & FixQuotes(dataUtama(71)) & "', bamerk  = '" & FixQuotes(dataUtama(72)) & "', baukuran  = '" & FixQuotes(dataUtama(73)) & "', bamodel  = '" & FixQuotes(dataUtama(74)) & "', bakelas  = '" & FixQuotes(dataUtama(75)) & "', bserial  = " & dataUtama(76) & ", bbatch  = " & dataUtama(77) & ", bpengganti  = " & dataUtama(78) & ", bgambar  = '" & FixQuotes(dataUtama(79)) & "', burutan  = " & dataUtama(80) & ", bcustom1  = '" & FixQuotes(dataUtama(81)) & "', bcustom2  = '" & FixQuotes(dataUtama(82)) & "', bcustom3  = '" & FixQuotes(dataUtama(83)) & "', bcustom4  = '" & FixQuotes(dataUtama(84)) & "', bcustom5  = '" & FixQuotes(dataUtama(85)) & "', bcustom6  = '" & FixQuotes(dataUtama(86)) & "', bcustom7  = '" & FixQuotes(dataUtama(87)) & "', bcustom8  = '" & FixQuotes(dataUtama(88)) & "', bcustom9  = '" & FixQuotes(dataUtama(89)) & "', bcustom10  = '" & FixQuotes(dataUtama(90)) & "', bcustom11  = " & FixDouble(dataUtama(91)) & ", bcustom12  = " & FixDouble(dataUtama(92)) & ", bcustom13  = " & FixDouble(dataUtama(93)) & ", bcustom14  = '" & FixDouble(dataUtama(94)) & "', bcustom15  = '" & FixDouble(dataUtama(95)) & "', bcatatan  = '" & FixQuotes(dataUtama(96)) & "', binputuser  = " & dataUtama(97) & ", binputtgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(98), "yyyy-MM-dd H:mm:ss")) & "', bmodifikasiuser  = " & dataUtama(99) & ", bmodifikasitgl  = '" & FixQuotes(AsFormatTanggal(dataUtama(100), "yyyy-MM-dd H:mm:ss")) & "', bedithpp  = " & dataUtama(101) & ", bmobile = " & dataUtama(102) & ", bsubkelas  = '" & FixQuotes(dataUtama(103)) & "', bmaterial  = '" & FixQuotes(dataUtama(104)) & "', bsection  = '" & FixQuotes(dataUtama(105)) & "', bvendor  = '" & FixQuotes(dataUtama(106)) & "', bdesigner  = '" & FixQuotes(dataUtama(107)) & "' where bid = '" & dataUtama(0) & "'"
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
                sql = "Insert into M1_Item (bkode, bnama, bnamaalias1, bnamaalias2, bnamaalias3, bnamaalias4, bnamaalias5, btipe, bjenis, bjenisdetail, bkategori, bketerangan, bsatuan, bnilaisatuan, bsatuandefault, bnilaisatuandefault, bhpp, bcabang, blokasi, bdivisi, bsubdivisi, bgudang, bproyek, bsubitem, bsubitemdari, bbarcode, bsuplier, baktif, baktiftgl, bstokminimal, bstokmaksimal, breorder, bjmlorderbeli, bjmlorderjual, bkategoriumur, bstatusmoving, bsifatharga, bpromo, bpromoberlaku, bpajakbeli, bpajakjual, bhargabeli, bhppaverage, bhargajual1, bhargajual2, bhargajual3, bhargajual4, bhargajual5, bdiskonjual1, bdiskonjual2, bdiskonjual3, bdiskonjual4, bdiskonjual5, bstok, bkomisi, bmarginminimal, brekpersediaan, brekpenjualan, brekreturpenjualan, brekdiskonpenjualan, brekhargapokok, brekreturpembelian, brekdiskonpembelian, brekkonsinyasi, bapanjang, balebar, batinggi, bavolume, baberat, bawarna, baoem, bamerk, baukuran, bamodel, bakelas, bserial, bbatch, bpengganti, bgambar, burutan, bcustom1, bcustom2, bcustom3, bcustom4, bcustom5, bcustom6, bcustom7, bcustom8, bcustom9, bcustom10, bcustom11, bcustom12, bcustom13, bcustom14, bcustom15, bcatatan, binputuser, binputtgl, bmodifikasiuser, bmodifikasitgl, bedithpp, bmobile, bsubkelas, bmaterial, bsection, bvendor, bdesigner) values('" & FixQuotes(dataUtama(1)) & "', '" & FixQuotes(dataUtama(2)) & "', '" & FixQuotes(dataUtama(3)) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(dataUtama(7)) & "', '" & FixQuotes(dataUtama(8)) & "', '" & FixQuotes(dataUtama(9)) & "', " & dataUtama(10) & ", '" & FixQuotes(dataUtama(11)) & "', '" & FixQuotes(dataUtama(12)) & "', '" & FixQuotes(dataUtama(13)) & "', '" & FixDouble(dataUtama(14)) & "', '" & FixQuotes(dataUtama(15)) & "', '" & FixDouble(dataUtama(16)) & "', '" & FixQuotes(dataUtama(17)) & "', '" & FixQuotes(dataUtama(18)) & "', '" & FixQuotes(dataUtama(19)) & "', '" & FixQuotes(dataUtama(20)) & "', '" & FixQuotes(dataUtama(21)) & "', '" & FixQuotes(dataUtama(22)) & "', '" & FixQuotes(dataUtama(23)) & "', " & dataUtama(24) & ", " & dataUtama(25) & ", '" & FixQuotes(dataUtama(26)) & "', " & dataUtama(27) & ", " & dataUtama(28) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(29))) & "', '" & FixDouble(dataUtama(30)) & "', '" & FixDouble(dataUtama(31)) & "', '" & FixDouble(dataUtama(32)) & "', '" & FixDouble(dataUtama(33)) & "', '" & FixDouble(dataUtama(34)) & "', '" & FixQuotes(dataUtama(35)) & "', '" & FixQuotes(dataUtama(36)) & "', '" & FixQuotes(dataUtama(37)) & "', " & dataUtama(38) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(39))) & "', '" & FixQuotes(dataUtama(40)) & "', '" & FixQuotes(dataUtama(41)) & "', '" & FixDouble(dataUtama(42)) & "', '" & FixDouble(dataUtama(43)) & "', '" & FixDouble(dataUtama(44)) & "', '" & FixDouble(dataUtama(45)) & "', '" & FixDouble(dataUtama(46)) & "', '" & FixDouble(dataUtama(47)) & "', '" & FixDouble(dataUtama(48)) & "', '" & FixDouble(dataUtama(49)) & "', '" & FixDouble(dataUtama(50)) & "', '" & FixDouble(dataUtama(51)) & "', '" & FixDouble(dataUtama(52)) & "', '" & FixDouble(dataUtama(53)) & "', '" & FixDouble(dataUtama(54)) & "', '" & FixDouble(dataUtama(55)) & "', '" & FixDouble(dataUtama(56)) & "', '" & FixQuotes(dataUtama(57)) & "', '" & FixQuotes(dataUtama(58)) & "', '" & FixQuotes(dataUtama(59)) & "', '" & FixQuotes(dataUtama(60)) & "', '" & FixQuotes(dataUtama(61)) & "', '" & FixQuotes(dataUtama(62)) & "', '" & FixQuotes(dataUtama(63)) & "', '" & FixQuotes(dataUtama(64)) & "', '" & FixDouble(dataUtama(65)) & "', '" & FixDouble(dataUtama(66)) & "', '" & FixDouble(dataUtama(67)) & "', '" & FixDouble(dataUtama(68)) & "', '" & FixDouble(dataUtama(69)) & "', '" & FixQuotes(dataUtama(70)) & "', '" & FixQuotes(dataUtama(71)) & "', '" & FixQuotes(dataUtama(72)) & "', '" & FixQuotes(dataUtama(73)) & "', '" & FixQuotes(dataUtama(74)) & "', '" & FixQuotes(dataUtama(75)) & "', " & dataUtama(76) & ", " & dataUtama(77) & ", " & dataUtama(78) & ", '" & FixQuotes(dataUtama(79)) & "', " & dataUtama(80) & ", '" & FixQuotes(dataUtama(81)) & "', '" & FixQuotes(dataUtama(82)) & "', '" & FixQuotes(dataUtama(83)) & "', '" & FixQuotes(dataUtama(84)) & "', '" & FixQuotes(dataUtama(85)) & "', '" & FixQuotes(dataUtama(86)) & "', '" & FixQuotes(dataUtama(87)) & "', '" & FixQuotes(dataUtama(88)) & "', '" & FixQuotes(dataUtama(89)) & "', '" & FixQuotes(dataUtama(90)) & "', " & FixDouble(dataUtama(91)) & ", " & FixDouble(dataUtama(92)) & ", " & FixDouble(dataUtama(93)) & ", '" & FixDouble(dataUtama(94)) & "', '" & FixDouble(dataUtama(95)) & "', '" & FixQuotes(dataUtama(96)) & "', " & dataUtama(97) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(98), "yyyy-MM-dd H:mm:ss")) & "', " & dataUtama(99) & ", '" & FixQuotes(AsFormatTanggal(dataUtama(100), "yyyy-MM-dd H:mm:ss")) & "', " & dataUtama(101) & ", " & dataUtama(102) & ", '" & FixQuotes(dataUtama(103)) & "', '" & FixQuotes(dataUtama(104)) & "', '" & FixQuotes(dataUtama(105)) & "', '" & FixQuotes(dataUtama(106)) & "', '" & FixQuotes(dataUtama(107)) & "')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = Con1
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()
            End If


            'UPDATE BARCODE NO BERIKUTNYA
            If Len(dataUtama(26)) > 0 Then
                Dim dt As New DataTable

                'AMBIL JMLDIGIT NO URUT BARCODE
                Dim jmldigit As Double = 0
                sql = "SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'options' AND skode = 'JmlDigitBarcode'"
                dt = AsDataTableAmbilDariDB(sql)
                If dt.Rows.Count > 0 Then
                    If IsNumeric(FxDB(dt.Rows(0)(0), 0)) Then
                        jmldigit = Double.Parse(FxDB(dt.Rows(0)(0), 0))
                    End If
                End If

                'JIKA BARCODE >= JMLDIGIT BARCODE SETTING
                If dataUtama(26).Length >= jmldigit Then
                    'AMBIL AWALAN BARCODE
                    Dim awalan As String = Left(dataUtama(26), dataUtama(26).Length - jmldigit)
                    'AMBIL URUTAN BARCODE
                    Dim nourut As Double = Double.Parse(Right(dataUtama(26), jmldigit))

                    'AMBIL NO URUT BARCODE BERIKUTNYA
                    Dim noberikutnya As Double = 0
                    sql = "SELECT noberikutnya FROM m0_barcode_next WHERE awalan = '" & FixQuotes(awalan) & "'"
                    dt = AsDataTableAmbilDariDB(sql)
                    If dt.Rows.Count > 0 Then
                        noberikutnya = Double.Parse(FxDB(dt.Rows(0)(0), 1))
                    End If

                    If nourut >= noberikutnya Then
                        sql = "INSERT INTO m0_barcode_next (awalan, noberikutnya) VALUES ('" & FixQuotes(awalan) & "', " & FixDouble(nourut + 1) & ") ON DUPLICATE KEY UPDATE noberikutnya = VALUES(noberikutnya) "
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

            Trans.Commit()  '*** Commit Transaction ***'
            result(1) = 1
            result(2) = notransaksi
            result(3) = 0
            result(4) = result(4)

            'AMBIL DATA =============================================================
            Dim paramSearch As String = M1_Item_DataSearch(PostWsSearch(paramSplit(0), "M1_Item_DataSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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

End Class