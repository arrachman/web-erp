Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m5_cl
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M5_ClSimpan(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataUtama() As String

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


        'VALIDASI DAN SET DATA =============================================================
        dataSplit = paramSplit(5).Split(sptSubParam)    'SPLIT PARAMETER DATA

        'CEK ARRAY DATA
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

        'MAPPING BUAT WS ----------------------------------------------------------
        'clid(0) As Integer, clcabang(1) As String, cllokasi(2) As String, clgudang(3) As String, clasalbarang(4) As String, 
        'clasalbarangkategori(5) As Integer, cljenispenjualan(6) As String, cljenispenjualankategori(7) As Integer, clcarabayar(8) As Integer, clsumber(9) As String, 
        'clautonotransaksi(10) As Integer, clnotransaksi(11) As String, cltgl(12) As Date, clkodepa(13) As Integer, clcustomer(14) As Integer, 
        'clcustomerkontak(15) As String, cl1alamat1(16) As String, cl1alamat2(17) As String, cl1alamat3(18) As String, cl2alamat1(19) As String, 
        'cl2alamat2(20) As String, cl2alamat3(21) As String, clbagianpenjualan(22) As Integer, clekspedisi(23) As String, cltglkirim(24) As Date, 
        'cltermin(25) As String, cltgljatuhtempo(26) As Date, cluraian(27) As String, clcatatan(28) As String, clnoref(29) As String, 
        'cltglnoref(30) As Date, cltglpenutupan(31) As Date, clmatauang(32) As String, clkurs(33) As Double, clhargatermasukpajak(34) As Integer, 
        'cltotal(35) As Double, cldiskonpersen(36) As String, cljmldiskon(37) As Double, cltotalpajak1detail(38) As Double, cltotalpajak2detail(39) As Double, 
        'clbiayalainpersen(40) As String, clbiayalain(41) As Double, cltotaltransaksi(42) As Double, cljmlbayar(43) As Double, clrekdiskon(44) As String, 
        'clrekpajak1(45) As String, clrekpajak2(46) As String, clrekbiayalain(47) As String, clrekbayar(48) As String, clidso(49) As Integer, 
        'clstatuspi(50) As Integer, clstatuspl(51) As Integer, clstatusdo(52) As Integer, clstatusdr(53) As Integer, clstatussi(54) As Integer, 
        'clstatusrnr(55) As Integer, clstatussr(56) As Integer, clstatusrealisasi(57) As Integer, clstatus(58) As Integer, clstatussebelumnya(59) As Integer, 
        'cljmlrevisi(60) As Integer, clcetakanke(61) As Integer, clinputuser(62) As Integer, clinputtgl(63) As DateTime, clmodifikasiuser(64) As Integer, 
        'clmodifikasitgl(65) As DateTime, clposting(66) As Integer, clpostingtgl(67) As DateTime, clisclose(68) As Integer, clcustomtext1(69) As String, 
        'clcustomtext2(70) As String, clcustomtext3(71) As String, clcustomtext4(72) As String, clcustomtext5(73) As String, clcustomint1(74) As Integer, 
        'clcustomint2(75) As Integer, clcustomint3(76) As Integer, clcustomdbl1(77) As Double, clcustomdbl2(78) As Double, clcustomdbl3(79) As Double, 
        'clcustomdate1(80) As Date, clcustomdate2(81) As Date, clcustomdate3(82) As Date, cluploaded(83) As Integer, clidsodetail(84) As Integer, 
        'clidbarang(85) As Integer, clnamabarang(86) As String, cltipebarang(87) As String, cljml(88) As Double, clsatuan(89) As String, 
        'clnilaisatuan(90) As Double, cljmlbarang(91) As Double, clsatuanbarang(92) As String


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'clid, clcabang, cllokasi, clgudang, clasalbarang, clasalbarangkategori, cljenispenjualan, 
        'cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, clnotransaksi, cltgl, clkodepa, 
        'clcustomer, clcustomerkontak, cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, 
        'cl2alamat3, clbagianpenjualan, clekspedisi, cltglkirim, cltermin, cltgljatuhtempo, cluraian, 
        'clcatatan, clnoref, cltglnoref, cltglpenutupan, clmatauang, clkurs, clhargatermasukpajak, 
        'cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, cltotalpajak2detail, clbiayalainpersen, clbiayalain, 
        'cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, clrekpajak2, clrekbiayalain, clrekbayar, 
        'clidso, clstatuspi, clstatuspl, clstatusdo, clstatusdr, clstatussi, clstatusrnr, 
        'clstatussr, clstatusrealisasi, clstatus, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, 
        'clinputtgl, clmodifikasiuser, clmodifikasitgl, clposting, clpostingtgl, clisclose, clcustomtext1, 
        'clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, clcustomint2, clcustomint3, 
        'clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, clcustomdate3, cluploaded, 
        'clidsodetail, clidbarang, clnamabarang, cltipebarang, cljml, clsatuan, clnilaisatuan, 
        'cljmlbarang, clsatuanbarang


        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA
        'CEK ARRAY DATA UTAMA
        'result(2) = dataUtama.Length.ToString() : GoTo selesai
        If (dataUtama.Length <> 93) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================


        'VALIDASI TIPE DATA UTAMA ==========================================================
        'clid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "clid required numeric." : GoTo selesai
        End If
        'clasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "clasalbarangkategori required numeric." : GoTo selesai
        End If
        'cljenispenjualankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "cljenispenjualankategori required numeric." : GoTo selesai
        End If
        'clcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "clcarabayar required numeric." : GoTo selesai
        End If
        'clautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "clautonotransaksi required numeric." : GoTo selesai
        End If
        'cltgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "cltgl required date." : GoTo selesai
        End If
        'clkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "clkodepa required numeric." : GoTo selesai
        End If
        'clcustomer(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "clcustomer required numeric." : GoTo selesai
        End If
        'clbagianpenjualan(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "clbagianpenjualan required numeric." : GoTo selesai
        End If
        'cltglkirim(24) As Date
        If (IsDate(dataUtama(24)) = False) Then
            result(2) = "cltglkirim required date." : GoTo selesai
        End If
        'cltgljatuhtempo(26) As Date
        If (IsDate(dataUtama(26)) = False) Then
            result(2) = "cltgljatuhtempo required date." : GoTo selesai
        End If
        'cltglnoref(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "cltglnoref required date." : GoTo selesai
        End If
        'cltglpenutupan(31) As Date
        If (IsDate(dataUtama(31)) = False) Then
            result(2) = "cltglpenutupan required date." : GoTo selesai
        End If
        'clkurs(33) As Double
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "clkurs required numeric." : GoTo selesai
        End If
        'clhargatermasukpajak(34) As Integer
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "clhargatermasukpajak required numeric." : GoTo selesai
        End If
        'cltotal(35) As Double
        If (IsNumeric(dataUtama(35)) = False) Then
            result(2) = "cltotal required numeric." : GoTo selesai
        End If
        'cljmldiskon(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "cljmldiskon required numeric." : GoTo selesai
        End If
        'cltotalpajak1detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "cltotalpajak1detail required numeric." : GoTo selesai
        End If
        'cltotalpajak2detail(39) As Double
        If (IsNumeric(dataUtama(39)) = False) Then
            result(2) = "cltotalpajak2detail required numeric." : GoTo selesai
        End If
        'clbiayalain(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "clbiayalain required numeric." : GoTo selesai
        End If
        'cltotaltransaksi(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "cltotaltransaksi required numeric." : GoTo selesai
        End If
        'cljmlbayar(43) As Double
        If (IsNumeric(dataUtama(43)) = False) Then
            result(2) = "cljmlbayar required numeric." : GoTo selesai
        End If
        'clidso(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "clidso required numeric." : GoTo selesai
        End If
        'clstatuspi(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "clstatuspi required numeric." : GoTo selesai
        End If
        'clstatuspl(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "clstatuspl required numeric." : GoTo selesai
        End If
        'clstatusdo(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "clstatusdo required numeric." : GoTo selesai
        End If
        'clstatusdr(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "clstatusdr required numeric." : GoTo selesai
        End If
        'clstatussi(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "clstatussi required numeric." : GoTo selesai
        End If
        'clstatusrnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "clstatusrnr required numeric." : GoTo selesai
        End If
        'clstatussr(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "clstatussr required numeric." : GoTo selesai
        End If
        'clstatusrealisasi(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "clstatusrealisasi required numeric." : GoTo selesai
        End If
        'clstatus(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "clstatus required numeric." : GoTo selesai
        End If
        'clstatussebelumnya(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "clstatussebelumnya required numeric." : GoTo selesai
        End If
        'cljmlrevisi(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "cljmlrevisi required numeric." : GoTo selesai
        End If
        'clcetakanke(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "clcetakanke required numeric." : GoTo selesai
        End If
        'clinputuser(62) As Integer
        If (IsNumeric(dataUtama(62)) = False) Then
            result(2) = "clinputuser required numeric." : GoTo selesai
        End If
        'clinputtgl(63) As DateTime
        If (IsDate(dataUtama(63)) = False) Then
            result(2) = "clinputtgl required date." : GoTo selesai
        End If
        'clmodifikasiuser(64) As Integer
        If (IsNumeric(dataUtama(64)) = False) Then
            result(2) = "clmodifikasiuser required numeric." : GoTo selesai
        End If
        'clmodifikasitgl(65) As DateTime
        If (IsDate(dataUtama(65)) = False) Then
            result(2) = "clmodifikasitgl required date." : GoTo selesai
        End If
        'clposting(66) As Integer
        If (IsNumeric(dataUtama(66)) = False) Then
            result(2) = "clposting required numeric." : GoTo selesai
        End If
        'clpostingtgl(67) As DateTime
        If (IsDate(dataUtama(67)) = False) Then
            result(2) = "clpostingtgl required date." : GoTo selesai
        End If
        'clisclose(68) As Integer
        If (IsNumeric(dataUtama(68)) = False) Then
            result(2) = "clisclose required numeric." : GoTo selesai
        End If
        'clcustomint1(74) As Integer
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "clcustomint1 required numeric." : GoTo selesai
        End If
        'clcustomint2(75) As Integer
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "clcustomint2 required numeric." : GoTo selesai
        End If
        'clcustomint3(76) As Integer
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "clcustomint3 required numeric." : GoTo selesai
        End If
        'clcustomdbl1(77) As Double
        If (IsNumeric(dataUtama(77)) = False) Then
            result(2) = "clcustomdbl1 required numeric." : GoTo selesai
        End If
        'clcustomdbl2(78) As Double
        If (IsNumeric(dataUtama(78)) = False) Then
            result(2) = "clcustomdbl2 required numeric." : GoTo selesai
        End If
        'clcustomdbl3(79) As Double
        If (IsNumeric(dataUtama(79)) = False) Then
            result(2) = "clcustomdbl3 required numeric." : GoTo selesai
        End If
        'clcustomdate1(80) As Date
        If (IsDate(dataUtama(80)) = False) Then
            result(2) = "clcustomdate1 required date." : GoTo selesai
        End If
        'clcustomdate2(81) As Date
        If (IsDate(dataUtama(81)) = False) Then
            result(2) = "clcustomdate2 required date." : GoTo selesai
        End If
        'clcustomdate3(82) As Date
        If (IsDate(dataUtama(82)) = False) Then
            result(2) = "clcustomdate3 required date." : GoTo selesai
        End If
        'cluploaded(83) As Integer
        If (IsNumeric(dataUtama(83)) = False) Then
            result(2) = "cluploaded required numeric." : GoTo selesai
        End If
        'clidsodetail(84) As Integer
        If (IsNumeric(dataUtama(84)) = False) Then
            result(2) = "clidsodetail required numeric." : GoTo selesai
        End If
        'clidbarang(85) As Integer
        If (IsNumeric(dataUtama(85)) = False) Then
            result(2) = "clidbarang required numeric." : GoTo selesai
        End If
        'cljml(88) As Double
        If (IsNumeric(dataUtama(88)) = False) Then
            result(2) = "cljml required numeric." : GoTo selesai
        End If
        'clnilaisatuan(90) As Double
        If (IsNumeric(dataUtama(90)) = False) Then
            result(2) = "clnilaisatuan required numeric." : GoTo selesai
        End If
        'cljmlbarang(91) As Double
        If (IsNumeric(dataUtama(91)) = False) Then
            result(2) = "cljmlbarang required numeric." : GoTo selesai
        End If
        'END OF VALIDASI TIPE DATA UTAMA ===================================================


        'VALIDASI DATA UTAMA =======================================================
        'clcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "clcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "clcabang should not be more than 25 character." : GoTo selesai
        End If

        'cllokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "cllokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "cllokasi should not be more than 25 character." : GoTo selesai
        End If

        'clgudang(3) As String
        If Len(dataUtama(3)) = 0 Then
            result(2) = "clgudang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "clgudang should not be more than 25 character." : GoTo selesai
        End If

        'clsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "clsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "clsumber should not be more than 10 character." : GoTo selesai
        End If

        'clnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "clnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "clnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'cltgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "cltgl can't be empty" : GoTo selesai
        End If

        'cltglkirim(24) As Date
        If Len(dataUtama(24)) = 0 Then
            result(2) = "cltglkirim can't be empty" : GoTo selesai
        End If

        'cltgljatuhtempo(26) As Date
        If Len(dataUtama(26)) = 0 Then
            result(2) = "cltgljatuhtempo can't be empty" : GoTo selesai
        End If

        'cltglnoref(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "cltglnoref can't be empty" : GoTo selesai
        End If

        'cltglpenutupan(31) As Date
        If Len(dataUtama(31)) = 0 Then
            result(2) = "cltglpenutupan can't be empty" : GoTo selesai
        End If

        'clmatauang(32) As String
        If Len(dataUtama(32)) = 0 Then
            result(2) = "clmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(32)) > 25 Then
            result(2) = "clmatauang should not be more than 25 character." : GoTo selesai
        End If

        'clkurs(33) As Double
        If Len(dataUtama(33)) = 0 Then
            result(2) = "clkurs can't be empty" : GoTo selesai
        End If

        'cltotal(35) As Double
        If Len(dataUtama(35)) = 0 Then
            result(2) = "cltotal can't be empty" : GoTo selesai
        End If

        'cldiskonpersen(36) As String
        If Len(dataUtama(36)) = 0 Then
            result(2) = "cldiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(36)) > 25 Then
            result(2) = "cldiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'cljmldiskon(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "cljmldiskon can't be empty" : GoTo selesai
        End If

        'cltotalpajak1detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "cltotalpajak1detail can't be empty" : GoTo selesai
        End If

        'cltotalpajak2detail(39) As Double
        If Len(dataUtama(39)) = 0 Then
            result(2) = "cltotalpajak2detail can't be empty" : GoTo selesai
        End If

        'clbiayalainpersen(40) As String
        If Len(dataUtama(40)) = 0 Then
            result(2) = "clbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(40)) > 25 Then
            result(2) = "clbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'clbiayalain(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "clbiayalain can't be empty" : GoTo selesai
        End If

        'cltotaltransaksi(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "cltotaltransaksi can't be empty" : GoTo selesai
        End If

        'cljmlbayar(43) As Double
        If Len(dataUtama(43)) = 0 Then
            result(2) = "cljmlbayar can't be empty" : GoTo selesai
        End If

        'clinputtgl(63) As DateTime
        If Len(dataUtama(63)) = 0 Then
            result(2) = "clinputtgl can't be empty" : GoTo selesai
        End If

        'clmodifikasitgl(65) As DateTime
        If Len(dataUtama(65)) = 0 Then
            result(2) = "clmodifikasitgl can't be empty" : GoTo selesai
        End If

        'clpostingtgl(67) As DateTime
        If Len(dataUtama(67)) = 0 Then
            result(2) = "clpostingtgl can't be empty" : GoTo selesai
        End If

        'clcustomdbl1(77) As Double
        If Len(dataUtama(77)) = 0 Then
            result(2) = "clcustomdbl1 can't be empty" : GoTo selesai
        End If

        'clcustomdbl2(78) As Double
        If Len(dataUtama(78)) = 0 Then
            result(2) = "clcustomdbl2 can't be empty" : GoTo selesai
        End If

        'clcustomdbl3(79) As Double
        If Len(dataUtama(79)) = 0 Then
            result(2) = "clcustomdbl3 can't be empty" : GoTo selesai
        End If

        'clcustomdate1(80) As Date
        If Len(dataUtama(80)) = 0 Then
            result(2) = "clcustomdate1 can't be empty" : GoTo selesai
        End If

        'clcustomdate2(81) As Date
        If Len(dataUtama(81)) = 0 Then
            result(2) = "clcustomdate2 can't be empty" : GoTo selesai
        End If

        'clcustomdate3(82) As Date
        If Len(dataUtama(82)) = 0 Then
            result(2) = "clcustomdate3 can't be empty" : GoTo selesai
        End If

        'cljml(88) As Double
        If Len(dataUtama(88)) = 0 Then
            result(2) = "cljml can't be empty" : GoTo selesai
        End If

        'clsatuan(89) As String
        If Len(dataUtama(89)) = 0 Then
            result(2) = "clsatuan can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(89)) > 25 Then
            result(2) = "clsatuan should not be more than 25 character." : GoTo selesai
        End If

        'clnilaisatuan(90) As Double
        If Len(dataUtama(90)) = 0 Then
            result(2) = "clnilaisatuan can't be empty" : GoTo selesai
        End If

        'cljmlbarang(91) As Double
        If Len(dataUtama(91)) = 0 Then
            result(2) = "cljmlbarang can't be empty" : GoTo selesai
        End If

        'clsatuanbarang(92) As String
        If Len(dataUtama(92)) = 0 Then
            result(2) = "clsatuanbarang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(92)) > 25 Then
            result(2) = "clsatuanbarang should not be more than 25 character." : GoTo selesai
        End If
        'END OF VALIDASI DATA UTAMA ================================================


        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "clid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cllokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cljenispenjualan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cljenispenjualankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomer", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomerkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cl2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clbagianpenjualan", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clekspedisi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltglkirim", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cluraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cltotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cldiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cljmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cljmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clidso", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatuspi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatuspl", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatusdo", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatusdr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatussi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatusrnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatussr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatusrealisasi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "cljmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clposting", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clpostingtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clcustomdate3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cluploaded", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clidsodetail", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clidbarang", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "clnamabarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cltipebarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cljml", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clsatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clnilaisatuan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "cljmlbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "clsatuanbarang", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "clid~clcabang~cllokasi~clgudang~clasalbarang~clasalbarangkategori~cljenispenjualan~cljenispenjualankategori~clcarabayar~clsumber~clautonotransaksi~clnotransaksi~cltgl~clkodepa~clcustomer~clcustomerkontak~cl1alamat1~cl1alamat2~cl1alamat3~cl2alamat1~cl2alamat2~cl2alamat3~clbagianpenjualan~clekspedisi~cltglkirim~cltermin~cltgljatuhtempo~cluraian~clcatatan~clnoref~cltglnoref~cltglpenutupan~clmatauang~clkurs~clhargatermasukpajak~cltotal~cldiskonpersen~cljmldiskon~cltotalpajak1detail~cltotalpajak2detail~clbiayalainpersen~clbiayalain~cltotaltransaksi~cljmlbayar~clrekdiskon~clrekpajak1~clrekpajak2~clrekbiayalain~clrekbayar~clidso~clstatuspi~clstatuspl~clstatusdo~clstatusdr~clstatussi~clstatusrnr~clstatussr~clstatusrealisasi~clstatus~clstatussebelumnya~cljmlrevisi~clcetakanke~clinputuser~clinputtgl~clmodifikasiuser~clmodifikasitgl~clposting~clpostingtgl~clisclose~clcustomtext1~clcustomtext2~clcustomtext3~clcustomtext4~clcustomtext5~clcustomint1~clcustomint2~clcustomint3~clcustomdbl1~clcustomdbl2~clcustomdbl3~clcustomdate1~clcustomdate2~clcustomdate3~cluploaded~clidsodetail~clidbarang~clnamabarang~cltipebarang~cljml~clsatuan~clnilaisatuan~cljmlbarang~clsatuanbarang", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79) & "~" & dataUtama(80) & "~" & dataUtama(81) & "~" & dataUtama(82) & "~" & dataUtama(83) & "~" & dataUtama(84) & "~" & dataUtama(85) & "~" & dataUtama(86) & "~" & dataUtama(87) & "~" & dataUtama(88) & "~" & dataUtama(89) & "~" & dataUtama(90) & "~" & dataUtama(91) & "~" & dataUtama(92)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
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
                Dim vModuleId As Integer = 5, vMenuId As Integer = 78
                Select Case drutama("clstatus")
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


                If isUpdate Then
                    result(4) = drutama("clid")
                    notransaksi = drutama("clnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(clid), clnotransaksi FROM M5_Cl WHERE clid='" & result(4) & "' AND clstatus NOT IN(2,3,4,7)", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("clautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("clcabang"), drutama("cllokasi"), drutama("clsumber"), drutama("cltgl"), drutama("clsumber"), 5)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(clid) FROM M5_Cl WHERE clnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m5_cl_history
                        Dim rsSimpanHistory As String = SimpanHistory.M5_Cl_HistorySimpan("" & paramSplit(0) & "★M5_Cl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(drutama("clsumber")) & "▼" & FixQuotes(drutama("clid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M5_Cl set clcabang  = '" & FixQuotes(drutama("clcabang")) & "', cllokasi  = '" & FixQuotes(drutama("cllokasi")) & "', clgudang  = '" & FixQuotes(drutama("clgudang")) & "', clasalbarang  = '" & FixQuotes(drutama("clasalbarang")) & "', clasalbarangkategori  = " & drutama("clasalbarangkategori") & ", cljenispenjualan  = '" & FixQuotes(drutama("cljenispenjualan")) & "', cljenispenjualankategori  = " & drutama("cljenispenjualankategori") & ", clcarabayar  = " & drutama("clcarabayar") & ", clsumber  = '" & FixQuotes(drutama("clsumber")) & "', clautonotransaksi  = " & drutama("clautonotransaksi") & ", clnotransaksi  = '" & FixQuotes(notransaksi) & "', cltgl  = '" & FixQuotes(AsFormatTanggal(drutama("cltgl"))) & "', clkodepa  = " & drutama("clkodepa") & ", clcustomer  = " & drutama("clcustomer") & ", clcustomerkontak  = '" & FixQuotes(drutama("clcustomerkontak")) & "', cl1alamat1  = '" & FixQuotes(drutama("cl1alamat1")) & "', cl1alamat2  = '" & FixQuotes(drutama("cl1alamat2")) & "', cl1alamat3  = '" & FixQuotes(drutama("cl1alamat3")) & "', cl2alamat1  = '" & FixQuotes(drutama("cl2alamat1")) & "', cl2alamat2  = '" & FixQuotes(drutama("cl2alamat2")) & "', cl2alamat3  = '" & FixQuotes(drutama("cl2alamat3")) & "', clbagianpenjualan  = " & drutama("clbagianpenjualan") & ", clekspedisi  = '" & FixQuotes(drutama("clekspedisi")) & "', cltglkirim  = '" & FixQuotes(AsFormatTanggal(drutama("cltglkirim"))) & "', cltermin  = '" & FixQuotes(drutama("cltermin")) & "', cltgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("cltgljatuhtempo"))) & "', cluraian  = '" & FixQuotes(drutama("cluraian")) & "', clcatatan  = '" & FixQuotes(drutama("clcatatan")) & "', clnoref  = '" & FixQuotes(drutama("clnoref")) & "', cltglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("cltglnoref"))) & "', cltglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("cltglpenutupan"))) & "', clmatauang  = '" & FixQuotes(drutama("clmatauang")) & "', clkurs  = '" & FixDouble(drutama("clkurs")) & "', clhargatermasukpajak  = " & drutama("clhargatermasukpajak") & ", cltotal  = '" & FixDouble(drutama("cltotal")) & "', cldiskonpersen  = '" & FixQuotes(drutama("cldiskonpersen")) & "', cljmldiskon  = '" & FixDouble(drutama("cljmldiskon")) & "', cltotalpajak1detail  = '" & FixDouble(drutama("cltotalpajak1detail")) & "', cltotalpajak2detail  = '" & FixDouble(drutama("cltotalpajak2detail")) & "', clbiayalainpersen  = '" & FixQuotes(drutama("clbiayalainpersen")) & "', clbiayalain  = '" & FixDouble(drutama("clbiayalain")) & "', cltotaltransaksi  = '" & FixDouble(drutama("cltotaltransaksi")) & "', cljmlbayar  = '" & FixDouble(drutama("cljmlbayar")) & "', clrekdiskon  = '" & FixQuotes(drutama("clrekdiskon")) & "', clrekpajak1  = '" & FixQuotes(drutama("clrekpajak1")) & "', clrekpajak2  = '" & FixQuotes(drutama("clrekpajak2")) & "', clrekbiayalain  = '" & FixQuotes(drutama("clrekbiayalain")) & "', clrekbayar  = '" & FixQuotes(drutama("clrekbayar")) & "', clidso  = " & drutama("clidso") & ", clstatuspi  = " & drutama("clstatuspi") & ", clstatuspl  = " & drutama("clstatuspl") & ", clstatusdo  = " & drutama("clstatusdo") & ", clstatusdr  = " & drutama("clstatusdr") & ", clstatussi  = " & drutama("clstatussi") & ", clstatusrnr  = " & drutama("clstatusrnr") & ", clstatussr  = " & drutama("clstatussr") & ", clstatusrealisasi  = " & drutama("clstatusrealisasi") & ", clstatus  = " & drutama("clstatus") & ", clstatussebelumnya  = " & drutama("clstatussebelumnya") & ", cljmlrevisi  = cljmlrevisi+1, clcetakanke  = " & drutama("clcetakanke") & ", clmodifikasiuser  = " & drutama("clmodifikasiuser") & ", clmodifikasitgl  = NOW(), clposting  = " & drutama("clposting") & ", clpostingtgl  = '" & FixQuotes(AsFormatTanggal(drutama("clpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', clcustomtext1  = '" & FixQuotes(drutama("clcustomtext1")) & "', clcustomtext2  = '" & FixQuotes(drutama("clcustomtext2")) & "', clcustomtext3  = '" & FixQuotes(drutama("clcustomtext3")) & "', clcustomtext4  = '" & FixQuotes(drutama("clcustomtext4")) & "', clcustomtext5  = '" & FixQuotes(drutama("clcustomtext5")) & "', clcustomint1  = " & drutama("clcustomint1") & ", clcustomint2  = " & drutama("clcustomint2") & ", clcustomint3  = " & drutama("clcustomint3") & ", clcustomdbl1  = '" & FixDouble(drutama("clcustomdbl1")) & "', clcustomdbl2  = '" & FixDouble(drutama("clcustomdbl2")) & "', clcustomdbl3  = '" & FixDouble(drutama("clcustomdbl3")) & "', clcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate1"))) & "', clcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate2"))) & "', clcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate3"))) & "', cluploaded  = " & drutama("cluploaded") & ", clidsodetail  = " & drutama("clidsodetail") & ", clidbarang  = " & drutama("clidbarang") & ", clnamabarang  = '" & FixQuotes(drutama("clnamabarang")) & "', cltipebarang  = '" & FixQuotes(drutama("cltipebarang")) & "', cljml  = '" & FixDouble(drutama("cljml")) & "', clsatuan  = '" & FixQuotes(drutama("clsatuan")) & "', clnilaisatuan  = '" & FixDouble(drutama("clnilaisatuan")) & "', cljmlbarang  = '" & FixDouble(drutama("cljmlbarang")) & "', clsatuanbarang  = '" & FixQuotes(drutama("clsatuanbarang")) & "' where clid = '" & drutama("clid") & "'"
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

                    If drutama("clautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("clcabang"), drutama("cllokasi"), drutama("clsumber"), drutama("cltgl"), drutama("clsumber"), 5)
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
                        notransaksi = drutama("clnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(clid) FROM M5_Cl WHERE clnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M5_Cl (clcabang, cllokasi, clgudang, clasalbarang, clasalbarangkategori, cljenispenjualan, cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, clnotransaksi, cltgl, clkodepa, clcustomer, clcustomerkontak, cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, cl2alamat3, clbagianpenjualan, clekspedisi, cltglkirim, cltermin, cltgljatuhtempo, cluraian, clcatatan, clnoref, cltglnoref, cltglpenutupan, clmatauang, clkurs, clhargatermasukpajak, cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, cltotalpajak2detail, clbiayalainpersen, clbiayalain, cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, clrekpajak2, clrekbiayalain, clrekbayar, clidso, clstatuspi, clstatuspl, clstatusdo, clstatusdr, clstatussi, clstatusrnr, clstatussr, clstatusrealisasi, clstatus, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, clinputtgl, clmodifikasiuser, clmodifikasitgl, clposting, clpostingtgl, clisclose, clcustomtext1, clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, clcustomint2, clcustomint3, clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, clcustomdate3, cluploaded, clidsodetail, clidbarang, clnamabarang, cltipebarang, cljml, clsatuan, clnilaisatuan, cljmlbarang, clsatuanbarang) values('" & FixQuotes(drutama("clcabang")) & "', '" & FixQuotes(drutama("cllokasi")) & "', '" & FixQuotes(drutama("clgudang")) & "', '" & FixQuotes(drutama("clasalbarang")) & "', " & drutama("clasalbarangkategori") & ", '" & FixQuotes(drutama("cljenispenjualan")) & "', " & drutama("cljenispenjualankategori") & ", " & drutama("clcarabayar") & ", '" & FixQuotes(drutama("clsumber")) & "', " & drutama("clautonotransaksi") & ", '" & FixQuotes(notransaksi) & "', '" & FixQuotes(AsFormatTanggal(drutama("cltgl"))) & "', " & drutama("clkodepa") & ", " & drutama("clcustomer") & ", '" & FixQuotes(drutama("clcustomerkontak")) & "', '" & FixQuotes(drutama("cl1alamat1")) & "', '" & FixQuotes(drutama("cl1alamat2")) & "', '" & FixQuotes(drutama("cl1alamat3")) & "', '" & FixQuotes(drutama("cl2alamat1")) & "', '" & FixQuotes(drutama("cl2alamat2")) & "', '" & FixQuotes(drutama("cl2alamat3")) & "', " & drutama("clbagianpenjualan") & ", '" & FixQuotes(drutama("clekspedisi")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cltglkirim"))) & "', '" & FixQuotes(drutama("cltermin")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cltgljatuhtempo"))) & "', '" & FixQuotes(drutama("cluraian")) & "', '" & FixQuotes(drutama("clcatatan")) & "', '" & FixQuotes(drutama("clnoref")) & "', '" & FixQuotes(AsFormatTanggal(drutama("cltglnoref"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("cltglpenutupan"))) & "', '" & FixQuotes(drutama("clmatauang")) & "', '" & FixDouble(drutama("clkurs")) & "', " & drutama("clhargatermasukpajak") & ", '" & FixDouble(drutama("cltotal")) & "', '" & FixQuotes(drutama("cldiskonpersen")) & "', '" & FixDouble(drutama("cljmldiskon")) & "', '" & FixDouble(drutama("cltotalpajak1detail")) & "', '" & FixDouble(drutama("cltotalpajak2detail")) & "', '" & FixQuotes(drutama("clbiayalainpersen")) & "', '" & FixDouble(drutama("clbiayalain")) & "', '" & FixDouble(drutama("cltotaltransaksi")) & "', '" & FixDouble(drutama("cljmlbayar")) & "', '" & FixQuotes(drutama("clrekdiskon")) & "', '" & FixQuotes(drutama("clrekpajak1")) & "', '" & FixQuotes(drutama("clrekpajak2")) & "', '" & FixQuotes(drutama("clrekbiayalain")) & "', '" & FixQuotes(drutama("clrekbayar")) & "', " & drutama("clidso") & ", " & drutama("clstatuspi") & ", " & drutama("clstatuspl") & ", " & drutama("clstatusdo") & ", " & drutama("clstatusdr") & ", " & drutama("clstatussi") & ", " & drutama("clstatusrnr") & ", " & drutama("clstatussr") & ", " & drutama("clstatusrealisasi") & ", " & drutama("clstatus") & ", " & drutama("clstatussebelumnya") & ", " & drutama("cljmlrevisi") & ", " & drutama("clcetakanke") & ", " & drutama("clinputuser") & ", NOW(), " & drutama("clmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("clposting") & ", '" & FixQuotes(AsFormatTanggal(drutama("clpostingtgl"), "yyyy-MM-dd HH:mm:ss")) & "', " & drutama("clisclose") & ", '" & FixQuotes(drutama("clcustomtext1")) & "', '" & FixQuotes(drutama("clcustomtext2")) & "', '" & FixQuotes(drutama("clcustomtext3")) & "', '" & FixQuotes(drutama("clcustomtext4")) & "', '" & FixQuotes(drutama("clcustomtext5")) & "', " & drutama("clcustomint1") & ", " & drutama("clcustomint2") & ", " & drutama("clcustomint3") & ", '" & FixDouble(drutama("clcustomdbl1")) & "', '" & FixDouble(drutama("clcustomdbl2")) & "', '" & FixDouble(drutama("clcustomdbl3")) & "', '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate1"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate2"))) & "', '" & FixQuotes(AsFormatTanggal(drutama("clcustomdate3"))) & "', " & drutama("cluploaded") & ", " & drutama("clidsodetail") & ", " & drutama("clidbarang") & ", '" & FixQuotes(drutama("clnamabarang")) & "', '" & FixQuotes(drutama("cltipebarang")) & "', '" & FixDouble(drutama("cljml")) & "', '" & FixQuotes(drutama("clsatuan")) & "', '" & FixDouble(drutama("clnilaisatuan")) & "', '" & FixDouble(drutama("cljmlbarang")) & "', '" & FixQuotes(drutama("clsatuanbarang")) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select clid from M5_Cl where clnotransaksi='" & notransaksi & "' AND clinputuser= '" & userid & "' order by clmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "CL", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M5_ClUpdateStatus(ByVal param As String) As String
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
            Dim sumber As String = "Cl", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Cltgl, Clnotransaksi, Clstatus FROM M5_Cl WHERE Clid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Clstatussebelumnya" : jnsaktivitas = 17
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
            Dim SimpanHistory As New M5_Cl_history
            Dim rsSimpanHistory As String = SimpanHistory.M5_Cl_HistorySimpan("" & paramSplit(0) & "★M5_Cl_HistorySimpan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mms★" & paramSplit(3) & "★0★" & FixQuotes(sumber) & "▼" & FixQuotes(idtransaksi) & "")
            Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            If (rsSplitResult(1) = 0) Then
                result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            End If
            'END OF SIMPAN HISTORY ==================


            'If isDelete Then

            'End If


            'update status utama
            sql = "UPDATE M5_Cl SET Clstatus = " & nilaiStatus & ", Clmodifikasiuser='" & userid & "', Clmodifikasitgl = NOW(), Clposting = 0, Clpostingtgl = '1971-01-01 00:00:00', Cljmlrevisi = Cljmlrevisi + 1 WHERE Clid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_ClSearch(PostWsSearch(paramSplit(0), "M5_ClSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))

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
    Public Function M5_ClDelete(ByVal param As String) As String

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
            Dim sumber As String = "Cl", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Clid, Clnotransaksi FROM M5_Cl WHERE Clid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT Clcabang, Cllokasi, Clsumber, Clautonotransaksi, Clnotransaksi, Cltgl"
            sql &= " FROM M5_Cl"
            sql &= " WHERE Clid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("Clcabang")
                lokasi = dtNomorNext.Rows(0)("Cllokasi")
                sumber = dtNomorNext.Rows(0)("Clsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("Clautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("Clnotransaksi")
                tgl = AsFormatTanggal(dtNomorNext.Rows(0)("Cltgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE UTAMA
            sql = "DELETE FROM M5_Cl WHERE Clid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M5_ClSearch(PostWsSearch(paramSplit(0), "M5_ClSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M5_ClSearch(ByVal param As String) As String
        'M5_ClSearch --------------------------------------------------------
        'clid, clcabang, clcabangnama, cllokasi, cllokasinama, clgudang, clgudangnama, 
        'clasalbarang, clasalbarangkategori, cljenispenjualan, cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, 
        'clnotransaksi, cltgl, clkodepa, clcustomer, clcustomerkode, clcustomernama, clcustomerkontak, 
        'cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, cl2alamat3, clbagianpenjualan, 
        'clbagianpenjualankode, clbagianpenjualannama, clekspedisi, clekspedisinama, cltglkirim, cltermin, clterminnama, 
        'clterminharijatuhtempo, cltgljatuhtempo, cluraian, clcatatan, clnoref, cltglnoref, cltglpenutupan, 
        'clmatauang, clkurs, clhargatermasukpajak, cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, 
        'cltotalpajak2detail, clbiayalainpersen, clbiayalain, cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, 
        'clrekpajak2, clrekbiayalain, clrekbayar, clidso, sonotransaksi, clstatuspi, clstatuspl, 
        'clstatusdo, clstatusdr, clstatussi, clstatusrnr, clstatussr, clstatusrealisasi, clstatus, 
        'clstatusnama, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, clinputuserkode, clinputusernama, 
        'clinputtgl, clmodifikasiuser, clmodifikasiuserkode, clmodifikasiusernama, clmodifikasitgl, clposting, clpostingtgl, 
        'clisclose, clcustomtext1, clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, 
        'clcustomint2, clcustomint3, clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, 
        'clcustomdate3, cluploaded, clidsodetail, clidbarang, clkodebarang, clnamabarang, cltipebarang, 
        'cljml, clsatuan, clnilaisatuan, cljmlbarang, clsatuanbarang

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

        ''PANGGIL QUERY
        'Dim query As New m0_query
        sql = "SELECT cl.clid, cl.clcabang, br.bnama as clcabangnama, cl.cllokasi, lc.lnama as cllokasinama, cl.clgudang, wh.wnama as clgudangnama, cl.clasalbarang, cl.clasalbarangkategori, cl.cljenispenjualan, cl.cljenispenjualankategori, cl.clcarabayar, cl.clsumber, cl.clautonotransaksi, cl.clnotransaksi, cl.cltgl, cl.clkodepa, cl.clcustomer, cl.clcustomtext4 as clcustomerkode, cl.clcustomtext4 as clcustomernama, cl.clcustomerkontak, cl.cl1alamat1, cl.cl1alamat2, cl.cl1alamat3, cl.cl2alamat1, cl.cl2alamat2, cl.cl2alamat3, cl.clbagianpenjualan, cl.clcustomtext5 as clbagianpenjualankode, cl.clcustomtext5 as clbagianpenjualannama, cl.clekspedisi, ex.enama as clekspedisinama, cl.cltglkirim, cl.cltermin, tr.trnama as clterminnama, tr.trharijatuhtempo as clterminharijatuhtempo, cl.cltgljatuhtempo, cl.cluraian, cl.clcatatan, cl.clnoref, cl.cltglnoref, cl.cltglpenutupan, cl.clmatauang, cl.clkurs, cl.clhargatermasukpajak, cl.cltotal, cl.cldiskonpersen, cl.cljmldiskon, cl.cltotalpajak1detail, cl.cltotalpajak2detail, cl.clbiayalainpersen, cl.clbiayalain, cl.cltotaltransaksi, cl.cljmlbayar, cl.clrekdiskon, cl.clrekpajak1, cl.clrekpajak2, cl.clrekbiayalain, cl.clrekbayar, cl.clidso, cl.clcustomtext3 as sonotransaksi, cl.clstatuspi, cl.clstatuspl, cl.clstatusdo, cl.clstatusdr, cl.clstatussi, cl.clstatusrnr, cl.clstatussr, cl.clstatusrealisasi, cl.clstatus, st.nama as clstatusnama, cl.clstatussebelumnya, cl.cljmlrevisi, cl.clcetakanke, cl.clinputuser, u.ukode as clinputuserkode, u.unama as clinputusernama, cl.clinputtgl, cl.clmodifikasiuser, u2.ukode as clmodifikasiuserkode, u2.ukode as clmodifikasiusernama, cl.clmodifikasitgl, cl.clposting, cl.clpostingtgl, cl.clisclose, cl.clcustomtext1, cl.clcustomtext2, cl.clcustomtext3, cl.clcustomtext4, cl.clcustomtext5, cl.clcustomint1, cl.clcustomint2, cl.clcustomint3, cl.clcustomdbl1, cl.clcustomdbl2, cl.clcustomdbl3, cl.clcustomdate1, cl.clcustomdate2, cl.clcustomdate3, cl.cluploaded, cl.clidsodetail, cl.clidbarang, i.bkode as clkodebarang, cl.clnamabarang, cl.cltipebarang, cl.cljml, cl.clsatuan, cl.clnilaisatuan, cl.cljmlbarang, cl.clsatuanbarang FROM m5_cl cl JOIN m1_branch br ON cl.clcabang = br.bkode JOIN m1_location lc ON cl.cllokasi = lc.lkode JOIN m1_warehouse wh ON cl.clgudang = wh.wkode LEFT JOIN m1_contact c ON cl.clcustomer = c.kid LEFT JOIN m1_contact cs ON cl.clbagianpenjualan = cs.kid LEFT JOIN m1_item i ON cl.clidbarang = i.bid JOIN m0_user u ON cl.clinputuser = u.userid JOIN m0_status st ON cl.clstatus = st.kode LEFT JOIN m5_so so ON cl.clidso = so.soid LEFT JOIN m1_expedition ex ON cl.clekspedisi = ex.ekode LEFT JOIN m1_terms tr ON cl.cltermin = tr.trkode LEFT JOIN m0_user u2 ON cl.clmodifikasiuser = u2.userid"
        'result(2) = sql : GoTo selesai
        dt = AmbilData("aplikasi1-M5_Cl_v", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("clid"), ""), sptField,
                     FxDB(dr("clcabang"), ""), sptField,
                     FxDB(dr("clcabangnama"), ""), sptField,
                     FxDB(dr("cllokasi"), ""), sptField,
                     FxDB(dr("cllokasinama"), ""), sptField,
                     FxDB(dr("clgudang"), ""), sptField,
                     FxDB(dr("clgudangnama"), ""), sptField,
                     FxDB(dr("clasalbarang"), ""), sptField,
                     FxDB(dr("clasalbarangkategori"), 0), sptField,
                     FxDB(dr("cljenispenjualan"), ""), sptField,
                     FxDB(dr("cljenispenjualankategori"), 0), sptField,
                     FxDB(dr("clcarabayar"), 0), sptField,
                     FxDB(dr("clsumber"), ""), sptField,
                     FxDB(dr("clautonotransaksi"), 0), sptField,
                     FxDB(dr("clnotransaksi"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltgl"), ""), formatTgl), sptField,
                     FxDB(dr("clkodepa"), ""), sptField,
                     FxDB(dr("clcustomer"), ""), sptField,
                     FxDB(dr("clcustomerkode"), ""), sptField,
                     FxDB(dr("clcustomernama"), ""), sptField,
                     FxDB(dr("clcustomerkontak"), ""), sptField,
                     FxDB(dr("cl1alamat1"), ""), sptField,
                     FxDB(dr("cl1alamat2"), ""), sptField,
                     FxDB(dr("cl1alamat3"), ""), sptField,
                     FxDB(dr("cl2alamat1"), ""), sptField,
                     FxDB(dr("cl2alamat2"), ""), sptField,
                     FxDB(dr("cl2alamat3"), ""), sptField,
                     FxDB(dr("clbagianpenjualan"), ""), sptField,
                     FxDB(dr("clbagianpenjualankode"), ""), sptField,
                     FxDB(dr("clbagianpenjualannama"), ""), sptField,
                     FxDB(dr("clekspedisi"), ""), sptField,
                     FxDB(dr("clekspedisinama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltglkirim"), ""), formatTgl), sptField,
                     FxDB(dr("cltermin"), ""), sptField,
                     FxDB(dr("clterminnama"), ""), sptField,
                     FxDB(dr("clterminharijatuhtempo"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("cltgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("cluraian"), ""), sptField,
                     FxDB(dr("clcatatan"), ""), sptField,
                     FxDB(dr("clnoref"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("cltglnoref"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("cltglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("clmatauang"), ""), sptField,
                     FxDB(dr("clkurs"), 0), sptField,
                     FxDB(dr("clhargatermasukpajak"), 0), sptField,
                     FxDB(dr("cltotal"), 0), sptField,
                     FxDB(dr("cldiskonpersen"), ""), sptField,
                     FxDB(dr("cljmldiskon"), 0), sptField,
                     FxDB(dr("cltotalpajak1detail"), 0), sptField,
                     FxDB(dr("cltotalpajak2detail"), 0), sptField,
                     FxDB(dr("clbiayalainpersen"), ""), sptField,
                     FxDB(dr("clbiayalain"), 0), sptField,
                     FxDB(dr("cltotaltransaksi"), 0), sptField,
                     FxDB(dr("cljmlbayar"), 0), sptField,
                     FxDB(dr("clrekdiskon"), ""), sptField,
                     FxDB(dr("clrekpajak1"), ""), sptField,
                     FxDB(dr("clrekpajak2"), ""), sptField,
                     FxDB(dr("clrekbiayalain"), ""), sptField,
                     FxDB(dr("clrekbayar"), ""), sptField,
                     FxDB(dr("clidso"), ""), sptField,
                     FxDB(dr("sonotransaksi"), ""), sptField,
                     FxDB(dr("clstatuspi"), 0), sptField,
                     FxDB(dr("clstatuspl"), 0), sptField,
                     FxDB(dr("clstatusdo"), 0), sptField,
                     FxDB(dr("clstatusdr"), 0), sptField,
                     FxDB(dr("clstatussi"), 0), sptField,
                     FxDB(dr("clstatusrnr"), 0), sptField,
                     FxDB(dr("clstatussr"), 0), sptField,
                     FxDB(dr("clstatusrealisasi"), 0), sptField,
                     FxDB(dr("clstatus"), 0), sptField,
                     FxDB(dr("clstatusnama"), ""), sptField,
                     FxDB(dr("clstatussebelumnya"), 0), sptField,
                     FxDB(dr("cljmlrevisi"), 0), sptField,
                     FxDB(dr("clcetakanke"), 0), sptField,
                     FxDB(dr("clinputuser"), ""), sptField,
                     FxDB(dr("clinputuserkode"), ""), sptField,
                     FxDB(dr("clinputusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("clinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clmodifikasiuser"), ""), sptField,
                     FxDB(dr("clmodifikasiuserkode"), ""), sptField,
                     FxDB(dr("clmodifikasiusernama"), ""), sptField,
                     AsFormatTanggal(FxDB(dr("clmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clposting"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("clpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("clisclose"), 0), sptField,
                     FxDB(dr("clcustomtext1"), ""), sptField,
                     FxDB(dr("clcustomtext2"), ""), sptField,
                     FxDB(dr("clcustomtext3"), ""), sptField,
                     FxDB(dr("clcustomtext4"), ""), sptField,
                     FxDB(dr("clcustomtext5"), ""), sptField,
                     FxDB(dr("clcustomint1"), 0), sptField,
                     FxDB(dr("clcustomint2"), 0), sptField,
                     FxDB(dr("clcustomint3"), 0), sptField,
                     FxDB(dr("clcustomdbl1"), 0), sptField,
                     FxDB(dr("clcustomdbl2"), 0), sptField,
                     FxDB(dr("clcustomdbl3"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate1"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate2"), ""), formatTgl), sptField,
                     AsFormatTanggal(FxDB(dr("clcustomdate3"), ""), formatTgl), sptField,
                     FxDB(dr("cluploaded"), 0), sptField,
                     FxDB(dr("clidsodetail"), ""), sptField,
                     FxDB(dr("clidbarang"), ""), sptField,
                     FxDB(dr("clkodebarang"), ""), sptField,
                     FxDB(dr("clnamabarang"), ""), sptField,
                     FxDB(dr("cltipebarang"), ""), sptField,
                     FxDB(dr("cljml"), 0), sptField,
                     FxDB(dr("clsatuan"), ""), sptField,
                     FxDB(dr("clnilaisatuan"), 0), sptField,
                     FxDB(dr("cljmlbarang"), 0), sptField,
                     FxDB(dr("clsatuanbarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("clid, clcabang, clcabangnama, cllokasi, cllokasinama, clgudang, clgudangnama, clasalbarang, clasalbarangkategori, cljenispenjualan, cljenispenjualankategori, clcarabayar, clsumber, clautonotransaksi, clnotransaksi, cltgl, clkodepa, clcustomer, clcustomerkode, clcustomernama, clcustomerkontak, cl1alamat1, cl1alamat2, cl1alamat3, cl2alamat1, cl2alamat2, cl2alamat3, clbagianpenjualan, clbagianpenjualankode, clbagianpenjualannama, clekspedisi, clekspedisinama, cltglkirim, cltermin, clterminnama, clterminharijatuhtempo, cltgljatuhtempo, cluraian, clcatatan, clnoref, cltglnoref, cltglpenutupan, clmatauang, clkurs, clhargatermasukpajak, cltotal, cldiskonpersen, cljmldiskon, cltotalpajak1detail, cltotalpajak2detail, clbiayalainpersen, clbiayalain, cltotaltransaksi, cljmlbayar, clrekdiskon, clrekpajak1, clrekpajak2, clrekbiayalain, clrekbayar, clidso, sonotransaksi, clstatuspi, clstatuspl, clstatusdo, clstatusdr, clstatussi, clstatusrnr, clstatussr, clstatusrealisasi, clstatus, clstatusnama, clstatussebelumnya, cljmlrevisi, clcetakanke, clinputuser, clinputuserkode, clinputusernama, clinputtgl, clmodifikasiuser, clmodifikasiuserkode, clmodifikasiusernama, clmodifikasitgl, clposting, clpostingtgl, clisclose, clcustomtext1, clcustomtext2, clcustomtext3, clcustomtext4, clcustomtext5, clcustomint1, clcustomint2, clcustomint3, clcustomdbl1, clcustomdbl2, clcustomdbl3, clcustomdate1, clcustomdate2, clcustomdate3, cluploaded, clidsodetail, clidbarang, clkodebarang, clnamabarang, cltipebarang, cljml, clsatuan, clnilaisatuan, cljmlbarang, clsatuanbarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M5_ClTerkait(ByVal param As String) As String
        'M5_ClTerkait --------------------------------------------------------
        'clid, clnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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
            result(2) = "clid required numeric." : GoTo selesai
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
            Filter = pagingSplit(2) & " AND clid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "clid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_cl_terkait(Filter)
        sql = m5_cl_terkait(Filter)


        dt = AmbilData("aplikasi1-m5_cl_Terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("clid"), 0), sptField,
                     FxDB(dr("clnotransaksi"), ""), sptField,
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
            result(2) = "Related CL data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("clid, clnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function m5_cl_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            filter1 = strFilter
            filter1 = filter1 & " AND ((`m2_cd`.`cdstatus` = 2) or (`m2_cd`.`cdstatus` = 3) or (`m2_cd`.`cdstatus` = 4) or (`m2_cd`.`cdstatus` = 7))"

            filter2 = strFilter
            filter2 = filter2 & " AND ((`m3_sa`.`sastatus` = 2) or (`m3_sa`.`sastatus` = 3) or (`m3_sa`.`sastatus` = 4) or (`m3_sa`.`sastatus` = 7))"

        Else
            'Default filter
            filter1 = "((`m2_cd`.`cdstatus` = 2) or (`m2_cd`.`cdstatus` = 3) or (`m2_cd`.`cdstatus` = 4) or (`m2_cd`.`cdstatus` = 7))"
            filter2 = "((`m3_sa`.`sastatus` = 2) or (`m3_sa`.`sastatus` = 3) or (`m3_sa`.`sastatus` = 4) or (`m3_sa`.`sastatus` = 7))"
            
        End If

        If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2

        sql = "select `cl`.`clid` AS `clid`,`cl`.`clnotransaksi` AS `clnotransaksi`,'CD' AS `sumber`,`m2_CD`.`CDid` AS `idterkait`,`m2_CD`.`CDnotransaksi` AS `noterkait`,`m2_CD`.`CDtgl` AS `tglterkait`,`m2_CD`.`CDinputtgl` AS `inputtglterkait`,`m2_CD`.`CDmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from `m5_cl` `cl` join `m2_CD` on `cl`.`clnotransaksi` = `m2_CD`.`CDcustomtext1` " & filter1 & "  group by `cl`.`clid`, `m2_CD`.`CDid`  "
        sql &= " UNION ALL "
        sql &= "select `cl`.`clid` AS `clid`,`cl`.`clnotransaksi` AS `clnotransaksi`,'SA' AS `sumber`,`m3_SA`.`SAid` AS `idterkait`,`m3_SA`.`SAnotransaksi` AS `noterkait`,`m3_SA`.`SAtgl` AS `tglterkait`,`m3_SA`.`SAinputtgl` AS `inputtglterkait`,`m3_SA`.`SAmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from `m5_cl` `cl` join `m3_SA` on `cl`.`clnotransaksi` = `m3_SA`.`SAnoref` " & filter2 & " group by `cl`.`clid`, `m3_SA`.`SAid`  "

        Return sql
    End Function

End Class